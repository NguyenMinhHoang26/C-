using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace NMHwin
{
    public partial class FormLogin : Form
    {
        private readonly string connStr =
            @"Data Source=.\SQLEXPRESS;Initial Catalog=sale;Integrated Security=True;TrustServerCertificate=True;";

        private TextBox tbUser;
        private TextBox tbPass;
        private Button btLogin;
        private Button btRegister;
        private Label lbMsg;

        public FormLogin()
        {
            InitializeComponent();   // giữ để hợp partial (nhưng không cần Designer)
            BuildUI();               // tạo giao diện

            this.AcceptButton = btLogin;
        }

        // Nếu không có Designer, bạn vẫn cần 1 hàm InitializeComponent rỗng để compile
        private void InitializeComponent() { }

        private void BuildUI()
        {
            Text = "Đăng nhập";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Width = 420;
            Height = 280;
            Font = new Font("Segoe UI", 10);

            var lbl1 = new Label { Text = "Tài khoản", AutoSize = true, Location = new Point(30, 30) };
            var lbl2 = new Label { Text = "Mật khẩu", AutoSize = true, Location = new Point(30, 85) };

            tbUser = new TextBox { Location = new Point(130, 25), Size = new Size(230, 27) };
            tbPass = new TextBox { Location = new Point(130, 80), Size = new Size(230, 27), UseSystemPasswordChar = true };

            btLogin = new Button { Text = "Đăng nhập", Location = new Point(130, 130), Size = new Size(110, 40) };
            btRegister = new Button { Text = "Đăng ký", Location = new Point(250, 130), Size = new Size(110, 40) };

            lbMsg = new Label
            {
                Location = new Point(30, 180),
                Size = new Size(340, 60),
                ForeColor = Color.Maroon
            };

            btLogin.Click += BtLogin_Click;
            btRegister.Click += BtRegister_Click;

            Controls.AddRange(new Control[] { lbl1, lbl2, tbUser, tbPass, btLogin, btRegister, lbMsg });
        }

        // ===== PBKDF2 HASH =====
        private static byte[] MakeSalt(int len = 16)
        {
            var salt = new byte[len];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        // ===== DB =====
        private bool UsernameExists(string username)
        {
            using var conn = new SqlConnection(connStr);
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.app_users WHERE username=@u", conn);
            cmd.Parameters.AddWithValue("@u", username);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private bool TryGetUser(string username, out byte[] hash, out byte[] salt)
        {
            hash = null; salt = null;

            using var conn = new SqlConnection(connStr);
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT password_hash, password_salt FROM dbo.app_users WHERE username=@u", conn);
            cmd.Parameters.AddWithValue("@u", username);

            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return false;

            hash = (byte[])rd["password_hash"];
            salt = (byte[])rd["password_salt"];
            return true;
        }

        private void InsertUser(string username, string password)
        {
            var salt = MakeSalt();
            var hash = HashPassword(password, salt);

            using var conn = new SqlConnection(connStr);
            conn.Open();
            using var cmd = new SqlCommand(
                @"INSERT INTO dbo.app_users(username, password_hash, password_salt)
                  VALUES (@u, @h, @s)", conn);

            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.Add("@h", System.Data.SqlDbType.VarBinary, 32).Value = hash;
            cmd.Parameters.Add("@s", System.Data.SqlDbType.VarBinary, 16).Value = salt;

            cmd.ExecuteNonQuery();
        }

        // ===== EVENTS =====
        private void BtRegister_Click(object sender, EventArgs e)
        {
            lbMsg.Text = "";

            string u = tbUser.Text.Trim();
            string p = tbPass.Text;

            if (u.Length < 3)
            {
                lbMsg.Text = "Tài khoản tối thiểu 3 ký tự.";
                return;
            }
            if (p.Length < 4)
            {
                lbMsg.Text = "Mật khẩu tối thiểu 4 ký tự.";
                return;
            }

            try
            {
                if (UsernameExists(u))
                {
                    lbMsg.Text = "Tài khoản đã tồn tại!";
                    return;
                }

                InsertUser(u, p);
                MessageBox.Show("Đăng ký thành công! Hãy đăng nhập.");
                tbPass.Clear();
                tbPass.Focus();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL: " + ex.Message);
            }
        }

        private void BtLogin_Click(object sender, EventArgs e)
        {
            lbMsg.Text = "";

            string u = tbUser.Text.Trim();
            string p = tbPass.Text;

            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                lbMsg.Text = "Nhập đầy đủ tài khoản và mật khẩu.";
                return;
            }

            try
            {
                if (!TryGetUser(u, out var savedHash, out var salt))
                {
                    lbMsg.Text = "Sai tài khoản hoặc mật khẩu.";
                    return;
                }

                var inputHash = HashPassword(p, salt);
                if (!SlowEquals(inputHash, savedHash))
                {
                    lbMsg.Text = "Sai tài khoản hoặc mật khẩu.";
                    return;
                }

                Hide();
                var f = new Form8(u); // Form8(string username) phải có
                f.FormClosed += (s, _) => Close();
                f.Show();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL: " + ex.Message);
            }
        }
    }
}
