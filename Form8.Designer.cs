namespace NMHwin
{
    partial class Form8
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.DataGridView dgvCustomer;
        private System.Windows.Forms.Button btRead;
        private System.Windows.Forms.Button btNew;
        private System.Windows.Forms.Button btDelete;
        private System.Windows.Forms.Button btEdit;
        private System.Windows.Forms.Button btExit;

        private System.Windows.Forms.DataGridViewTextBoxColumn colMa;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTen;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLop;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNgaySinh;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGioiTinh;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKhuVuc;
        private System.Windows.Forms.DataGridViewImageColumn colAnh;

        private System.Windows.Forms.Panel panelBottom;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            dgvCustomer = new System.Windows.Forms.DataGridView();

            colMa = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colTen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colLop = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colNgaySinh = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colGioiTinh = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colKhuVuc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colAnh = new System.Windows.Forms.DataGridViewImageColumn();

            btRead = new System.Windows.Forms.Button();
            btNew = new System.Windows.Forms.Button();
            btDelete = new System.Windows.Forms.Button();
            btEdit = new System.Windows.Forms.Button();
            btExit = new System.Windows.Forms.Button();

            panelBottom = new System.Windows.Forms.Panel();

            ((System.ComponentModel.ISupportInitialize)dgvCustomer).BeginInit();
            SuspendLayout();

            // dgvCustomer
            dgvCustomer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCustomer.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                colMa, colTen, colLop, colNgaySinh, colGioiTinh, colKhuVuc, colAnh
            });
            dgvCustomer.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvCustomer.MultiSelect = false;
            dgvCustomer.Name = "dgvCustomer";
            dgvCustomer.RowHeadersWidth = 40;
            dgvCustomer.RowTemplate.Height = 60;
            dgvCustomer.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvCustomer.TabIndex = 0;
            dgvCustomer.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(dgvCustomer_CellClick);

            // colMa
            colMa.HeaderText = "Mã";
            colMa.MinimumWidth = 50;
            colMa.Name = "colMa";
            colMa.Width = 70;

            // colTen (Fill)
            colTen.HeaderText = "Tên";
            colTen.MinimumWidth = 150;
            colTen.Name = "colTen";
            colTen.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            colTen.FillWeight = 40;

            // colLop
            colLop.HeaderText = "Lớp";
            colLop.MinimumWidth = 60;
            colLop.Name = "colLop";
            colLop.Width = 90;

            // colNgaySinh
            colNgaySinh.HeaderText = "Ngày sinh";
            colNgaySinh.MinimumWidth = 90;
            colNgaySinh.Name = "colNgaySinh";
            colNgaySinh.Width = 110;

            // colGioiTinh
            colGioiTinh.HeaderText = "Giới tính";
            colGioiTinh.MinimumWidth = 70;
            colGioiTinh.Name = "colGioiTinh";
            colGioiTinh.Width = 90;

            // colKhuVuc (Fill)
            colKhuVuc.HeaderText = "Khu vực";
            colKhuVuc.MinimumWidth = 140;
            colKhuVuc.Name = "colKhuVuc";
            colKhuVuc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            colKhuVuc.FillWeight = 30;

            // colAnh
            colAnh.HeaderText = "Ảnh";
            colAnh.MinimumWidth = 60;
            colAnh.Name = "colAnh";
            colAnh.Width = 90;
            colAnh.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;

            // panelBottom
            panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBottom.Height = 190;
            panelBottom.Name = "panelBottom";

            // btRead
            btRead.Name = "btRead";
            btRead.Text = "Đọc dữ liệu";
            btRead.UseVisualStyleBackColor = true;
            btRead.Click += new System.EventHandler(btRead_Click);

            // btNew
            btNew.Name = "btNew";
            btNew.Text = "Thêm";
            btNew.UseVisualStyleBackColor = true;
            btNew.Click += new System.EventHandler(btNew_Click);

            // btDelete
            btDelete.Name = "btDelete";
            btDelete.Text = "Xóa";
            btDelete.UseVisualStyleBackColor = true;
            btDelete.Click += new System.EventHandler(btDelete_Click);

            // btEdit
            btEdit.Name = "btEdit";
            btEdit.Text = "Sửa";
            btEdit.UseVisualStyleBackColor = true;
            btEdit.Click += new System.EventHandler(btEdit_Click);

            // btExit
            btExit.Name = "btExit";
            btExit.Text = "Thoát";
            btExit.UseVisualStyleBackColor = true;
            btExit.Click += new System.EventHandler(btExit_Click);

            // Form8
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1200, 680);
            Controls.Add(dgvCustomer);
            Controls.Add(panelBottom);
            Name = "Form8";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "ADO Example";
            Load += new System.EventHandler(Form8_Load);

            ((System.ComponentModel.ISupportInitialize)dgvCustomer).EndInit();
            ResumeLayout(false);
        }
    }
}
