using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TowerDefenseVS2022.Game;

namespace TowerDefenseVS2022.Forms
{
    public class MainMenuForm : Form
    {
        private readonly string _username;

        private readonly ComboBox _cbAI = new()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Top
        };

        public MainMenuForm(string username)
        {
            _username = username;

            // DPI + Font
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10F);

            Text = "TD - Menu";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 246, 250);
            ClientSize = new Size(560, 320);

            // ===== Card panel (bo góc) =====
            var card = new RoundedPanel
            {
                Radius = 18,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(18),
                Padding = new Padding(22)
            };

            var header = new Label
            {
                Text = "Tower Defense",
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 45, 60),
                Margin = new Padding(0, 0, 0, 2)
            };

            var sub = new Label
            {
                Text = $"Xin chào, {_username} 👋",
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                ForeColor = Color.FromArgb(90, 100, 115),
                Margin = new Padding(0, 0, 0, 18)
            };

            var lblAI = new Label
            {
                Text = "Chọn AI",
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 80),
                Margin = new Padding(0, 0, 0, 6)
            };

            // Combo style nhẹ
            _cbAI.Font = new Font("Segoe UI", 10F);
            _cbAI.Height = 32;

            // Bind AI
            var all = AIRegistry.All();
            _cbAI.DataSource = all;
            _cbAI.DisplayMember = "Name";
            if (_cbAI.Items.Count > 0) _cbAI.SelectedIndex = 0;

            // ===== Buttons =====
            var btnStart = new Button
            {
                Text = "▶  Vào Game",
                Dock = DockStyle.Top,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Margin = new Padding(0, 16, 0, 0),
                Cursor = Cursors.Hand
            };
            btnStart.FlatAppearance.BorderSize = 0;

            var btnExit = new Button
            {
                Text = "Thoát",
                Dock = DockStyle.Top,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(243, 244, 246),
                ForeColor = Color.FromArgb(55, 65, 80),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 10, 0, 0),
                Cursor = Cursors.Hand
            };
            btnExit.FlatAppearance.BorderSize = 0;

            btnStart.Click += (_, __) =>
            {
                if (_cbAI.SelectedItem is not IEnemyAI ai)
                {
                    MessageBox.Show("Chưa chọn được AI.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Hide();
                var game = new GameForm(_username, ai);
                game.FormClosed += (_, __) => Close();
                game.Show();
            };

            btnExit.Click += (_, __) => Close();

            // ===== Layout inside card =====
            var content = new Panel { Dock = DockStyle.Fill };

            // Dock top stack: add controls reverse order
            content.Controls.Add(btnExit);
            content.Controls.Add(btnStart);
            content.Controls.Add(_cbAI);
            content.Controls.Add(lblAI);
            content.Controls.Add(sub);
            content.Controls.Add(header);

            card.Controls.Add(content);

            // Outer padding
            var outer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18) };
            outer.Controls.Add(card);
            Controls.Add(outer);
        }

        // Panel bo góc có shadow nhẹ
        private class RoundedPanel : Panel
        {
            public int Radius { get; set; } = 16;

            public RoundedPanel()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                var rect = new Rectangle(0, 0, Width - 1, Height - 1);

                // shadow
                using (var shadowPath = RoundedRect(new Rectangle(rect.X + 2, rect.Y + 4, rect.Width, rect.Height), Radius))
                using (var sb = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
                {
                    e.Graphics.FillPath(sb, shadowPath);
                }

                // card
                using (var path = RoundedRect(rect, Radius))
                using (var b = new SolidBrush(BackColor))
                using (var pen = new Pen(Color.FromArgb(235, 238, 244)))
                {
                    e.Graphics.FillPath(b, path);
                    e.Graphics.DrawPath(pen, path);
                }

                base.OnPaint(e);
            }

            protected override void OnResize(EventArgs eventargs)
            {
                base.OnResize(eventargs);
                Invalidate();
            }

            private static GraphicsPath RoundedRect(Rectangle r, int radius)
            {
                int d = radius * 2;
                var path = new GraphicsPath();
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                return path;
            }
        }
    }
}
