using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace NMHwin
{
    public partial class Form8 : Form
    {
        private readonly string connStr =
            @"Data Source=.\SQLEXPRESS;Initial Catalog=sale;Integrated Security=True;TrustServerCertificate=True;";

        private TextBox tbId, tbName, tbLop, tbKhuVuc;
        private DateTimePicker dtpNgaySinh;
        private ComboBox cbGioiTinh;
        private PictureBox pbAnh;
        private Button btChonAnh;

        private byte[] _currentImageBytes = null;

        public Form8()
        {
            InitializeComponent();
            SetupBottomLayoutNice();
        }

        private void Form8_Load(object sender, EventArgs e)
        {
            dgvCustomer.AllowUserToAddRows = true;
            dgvCustomer.AutoGenerateColumns = false;

            dgvCustomer.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCustomer.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            LoadData();
        }

        // =================== LAYOUT ĐẸP (TABLELAYOUT) ===================
        private void SetupBottomLayoutNice()
        {
            panelBottom.Controls.Clear();

            var tbl = new TableLayoutPanel();
            tbl.Parent = panelBottom;
            tbl.Dock = DockStyle.Fill;
            tbl.Padding = new Padding(12, 10, 12, 10);
            tbl.ColumnCount = 8;
            tbl.RowCount = 2;

            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 85));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));    // ID
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));     // Tên
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));   // Lớp
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));   // Ngày sinh
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));   // Giới tính
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));     // Khu vực
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));   // Ảnh
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));   // Chọn ảnh

            GroupBox MakeBox(string title, Control inner)
            {
                var gb = new GroupBox();
                gb.Text = title;
                gb.Dock = DockStyle.Fill;
                gb.Padding = new Padding(10, 22, 10, 10);
                inner.Dock = DockStyle.Fill;
                inner.Margin = new Padding(0);
                gb.Controls.Add(inner);
                return gb;
            }

            tbId = new TextBox { PlaceholderText = "VD: 1" };
            tbName = new TextBox { PlaceholderText = "Họ và tên" };
            tbLop = new TextBox { PlaceholderText = "VD: 10A1" };
            tbKhuVuc = new TextBox { PlaceholderText = "VD: HCM" };

            dtpNgaySinh = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Value = new DateTime(2000, 1, 1)
            };

            cbGioiTinh = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            cbGioiTinh.SelectedIndex = 0;

            pbAnh = new PictureBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            btChonAnh = new Button
            {
                Text = "Chọn ảnh",
                Dock = DockStyle.Fill
            };
            btChonAnh.Click += btChonAnh_Click;

            tbl.Controls.Add(MakeBox("ID", tbId), 0, 0);
            tbl.Controls.Add(MakeBox("Tên", tbName), 1, 0);
            tbl.Controls.Add(MakeBox("Lớp", tbLop), 2, 0);
            tbl.Controls.Add(MakeBox("Ngày sinh", dtpNgaySinh), 3, 0);
            tbl.Controls.Add(MakeBox("Giới tính", cbGioiTinh), 4, 0);
            tbl.Controls.Add(MakeBox("Khu vực", tbKhuVuc), 5, 0);
            tbl.Controls.Add(MakeBox("Ảnh", pbAnh), 6, 0);
            tbl.Controls.Add(btChonAnh, 7, 0);

            var flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.FlowDirection = FlowDirection.LeftToRight;
            flow.WrapContents = false;
            flow.Padding = new Padding(0, 12, 0, 0);

            foreach (var b in new[] { btRead, btNew, btDelete, btEdit, btExit })
            {
                b.Parent = flow;
                b.Size = new Size(120, 40);
                b.Margin = new Padding(0, 0, 12, 0);
            }

            tbl.Controls.Add(flow, 0, 1);
            tbl.SetColumnSpan(flow, 8);

            tbKhuVuc.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    AddFromInputs();
                }
            };
        }

        // =================== IMAGE HELPERS ===================
        private static byte[] ImageToBytes(Image img)
        {
            if (img == null) return null;
            using var ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        private static Image BytesToImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            using var ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        private void btChonAnh_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            ofd.Title = "Chọn ảnh";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var img = Image.FromFile(ofd.FileName);
                pbAnh.Image = img;
                _currentImageBytes = ImageToBytes(img);
            }
        }

        // =================== LOAD ===================
        private void LoadData()
        {
            dgvCustomer.Rows.Clear();

            using SqlConnection conn = new SqlConnection(connStr);
            conn.Open();

            using var cmd = new SqlCommand(
                "SELECT id, name, lop, ngaysinh, gioitinh, khuvuc, anh FROM customer ORDER BY id", conn);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                int id = rd.IsDBNull(0) ? 0 : rd.GetInt32(0);
                string name = rd.IsDBNull(1) ? "" : rd.GetString(1);
                string lop = rd.IsDBNull(2) ? "" : rd.GetString(2);

                object ngaySinhObj = rd.IsDBNull(3) ? null : rd.GetDateTime(3).ToString("dd/MM/yyyy");
                string gioitinh = rd.IsDBNull(4) ? "" : rd.GetString(4);
                string khuvuc = rd.IsDBNull(5) ? "" : rd.GetString(5);

                byte[] bytes = rd.IsDBNull(6) ? null : (byte[])rd[6];
                Image img = BytesToImage(bytes);

                dgvCustomer.Rows.Add(id, name, lop, ngaySinhObj, gioitinh, khuvuc, img);
            }
        }

        private void btRead_Click(object sender, EventArgs e) => LoadData();

        // =================== CLICK -> FILL INPUTS ===================
        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvCustomer.Rows[e.RowIndex];
            if (row.IsNewRow) return;

            tbId.Text = Convert.ToString(row.Cells["colMa"].Value);
            tbName.Text = Convert.ToString(row.Cells["colTen"].Value);
            tbLop.Text = Convert.ToString(row.Cells["colLop"].Value);

            var s = row.Cells["colNgaySinh"].Value?.ToString();
            if (!string.IsNullOrWhiteSpace(s) && DateTime.TryParse(s, out var d))
                dtpNgaySinh.Value = d;

            string gt = Convert.ToString(row.Cells["colGioiTinh"].Value);
            if (!string.IsNullOrWhiteSpace(gt))
            {
                int idx = cbGioiTinh.FindStringExact(gt);
                cbGioiTinh.SelectedIndex = idx >= 0 ? idx : 0;
            }
            else cbGioiTinh.SelectedIndex = 0;

            tbKhuVuc.Text = Convert.ToString(row.Cells["colKhuVuc"].Value);

            var img = row.Cells["colAnh"].Value as Image;
            pbAnh.Image = img;
            _currentImageBytes = ImageToBytes(img);

            tbName.Focus();
            tbName.SelectionStart = tbName.Text.Length;
        }

        // =================== ADD ===================
        private void btNew_Click(object sender, EventArgs e)
        {
            dgvCustomer.EndEdit();
            AddFromInputs();
        }

        private void AddFromInputs()
        {
            if (!int.TryParse(tbId.Text.Trim(), out int id))
            {
                MessageBox.Show("ID phải là số!");
                tbId.Focus();
                tbId.SelectAll();
                return;
            }

            string name = tbName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Tên không được để trống!");
                tbName.Focus();
                return;
            }

            string lop = tbLop.Text.Trim();
            DateTime ngaysinh = dtpNgaySinh.Value.Date;
            string gioitinh = cbGioiTinh.SelectedItem?.ToString() ?? "";
            string khuvuc = tbKhuVuc.Text.Trim();
            byte[] anh = _currentImageBytes;

            try
            {
                using SqlConnection conn = new SqlConnection(connStr);
                conn.Open();

                using var cmd = new SqlCommand(
                    @"INSERT INTO customer (id, name, lop, ngaysinh, gioitinh, khuvuc, anh)
                      VALUES (@id, @name, @lop, @ngaysinh, @gioitinh, @khuvuc, @anh)", conn);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@lop", (object)(lop ?? ""));
                cmd.Parameters.AddWithValue("@ngaysinh", ngaysinh);
                cmd.Parameters.AddWithValue("@gioitinh", (object)(gioitinh ?? ""));
                cmd.Parameters.AddWithValue("@khuvuc", (object)(khuvuc ?? ""));
                cmd.Parameters.AddWithValue("@anh", (object)anh ?? DBNull.Value);

                cmd.ExecuteNonQuery();

                LoadData();
                MessageBox.Show("Đã thêm thành công!");
                ClearInputs();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL: " + ex.Message);
            }
        }

        private void ClearInputs()
        {
            tbId.Clear();
            tbName.Clear();
            tbLop.Clear();
            tbKhuVuc.Clear();
            dtpNgaySinh.Value = new DateTime(2000, 1, 1);
            cbGioiTinh.SelectedIndex = 0;
            pbAnh.Image = null;
            _currentImageBytes = null;
            tbId.Focus();
        }

        // =================== EDIT ===================
        private void btEdit_Click(object sender, EventArgs e)
        {
            dgvCustomer.EndEdit();

            if (!int.TryParse(tbId.Text.Trim(), out int id))
            {
                MessageBox.Show("ID phải là số!");
                return;
            }

            string name = tbName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Tên không được để trống!");
                return;
            }

            string lop = tbLop.Text.Trim();
            DateTime ngaysinh = dtpNgaySinh.Value.Date;
            string gioitinh = cbGioiTinh.SelectedItem?.ToString() ?? "";
            string khuvuc = tbKhuVuc.Text.Trim();
            byte[] anh = _currentImageBytes;

            try
            {
                using SqlConnection conn = new SqlConnection(connStr);
                conn.Open();

                using var cmd = new SqlCommand(
                    @"UPDATE customer
                      SET name=@name, lop=@lop, ngaysinh=@ngaysinh, gioitinh=@gioitinh, khuvuc=@khuvuc, anh=@anh
                      WHERE id=@id", conn);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@lop", (object)(lop ?? ""));
                cmd.Parameters.AddWithValue("@ngaysinh", ngaysinh);
                cmd.Parameters.AddWithValue("@gioitinh", (object)(gioitinh ?? ""));
                cmd.Parameters.AddWithValue("@khuvuc", (object)(khuvuc ?? ""));
                cmd.Parameters.AddWithValue("@anh", (object)anh ?? DBNull.Value);

                cmd.ExecuteNonQuery();

                LoadData();
                MessageBox.Show("Đã cập nhật!");
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL: " + ex.Message);
            }
        }

        // =================== DELETE ===================
        private void btDelete_Click(object sender, EventArgs e)
        {
            dgvCustomer.EndEdit();

            if (!int.TryParse(tbId.Text.Trim(), out int id))
            {
                MessageBox.Show("Chọn dòng cần xóa (ID phải là số)!");
                return;
            }

            if (MessageBox.Show($"Xóa khách hàng ID = {id} ?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using SqlConnection conn = new SqlConnection(connStr);
                conn.Open();

                using var cmd = new SqlCommand("DELETE FROM customer WHERE id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                LoadData();
                MessageBox.Show("Đã xóa!");
                ClearInputs();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL: " + ex.Message);
            }
        }

        private void btExit_Click(object sender, EventArgs e) => Close();
    }
}
