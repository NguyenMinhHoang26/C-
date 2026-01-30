using System;
using System.Drawing;
using System.Windows.Forms;
using TowerDefenseVS2022.Auth;

namespace TowerDefenseVS2022.Forms
{
    public class LoginForm : Form
    {
        private readonly TextBox _tbUser = new() { Width = 220 };
        private readonly TextBox _tbPass = new() { Width = 220, UseSystemPasswordChar = true };
        private readonly Label _lbMsg = new() { AutoSize = true };

        private readonly UserStore _store = new();

        public LoginForm()
        {
            Text = "TD - Login";
            Width = 400;
            Height = 260;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var lbl1 = new Label { Text = "Username", AutoSize = true };
            var lbl2 = new Label { Text = "Password", AutoSize = true };

            var btnLogin = new Button { Text = "Đăng nhập", Width = 110 };
            var btnReg = new Button { Text = "Đăng ký", Width = 110 };

            btnLogin.Click += (_, __) =>
            {
                try
                {
                    if (_store.Login(_tbUser.Text, _tbPass.Text, out var msg))
                    {
                        Hide();
                        var menu = new MainMenuForm(_tbUser.Text.Trim());
                        menu.FormClosed += (_, __) => Close();
                        menu.Show();
                    }
                    else
                    {
                        ShowMsg(msg, ok: false);
                        MessageBox.Show(msg, "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnReg.Click += (_, __) =>
            {
                try
                {
                    bool ok = _store.Register(_tbUser.Text, _tbPass.Text, out var msg);
                    ShowMsg(msg, ok);

                    // Popup luôn hiện để bạn chắc chắn nút đã chạy
                    MessageBox.Show(
                        msg + "\n\nFile lưu: users.json trong thư mục chạy (bin\\Debug\\net8.0-windows)",
                        "Register",
                        MessageBoxButtons.OK,
                        ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi Register", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                RowCount = 6,
                ColumnCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Set row heights để label không bị “nuốt”
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); // user
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); // pass
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // buttons
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // msg
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));

            layout.Controls.Add(lbl1, 0, 0);
            layout.Controls.Add(_tbUser, 1, 0);

            layout.Controls.Add(lbl2, 0, 1);
            layout.Controls.Add(_tbPass, 1, 1);

            var pnlBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            pnlBtn.Controls.Add(btnLogin);
            pnlBtn.Controls.Add(btnReg);

            layout.Controls.Add(pnlBtn, 1, 2);
            layout.Controls.Add(_lbMsg, 1, 3);

            Controls.Add(layout);
            AcceptButton = btnLogin;
        }

        private void ShowMsg(string msg, bool ok)
        {
            _lbMsg.Text = msg;
            _lbMsg.ForeColor = ok ? Color.DarkGreen : Color.DarkRed;
        }
    }
}
