using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace NMHwin
{
    public partial class Form5 : Form
    {
        PictureBox pbBasket = new PictureBox();
        PictureBox pbChicken = new PictureBox();

        Timer tmEgg = new Timer();
        Timer tmChicken = new Timer();
        Timer tmSpawn = new Timer();  // Tạo trứng mới

        int xBasket = 300;
        int yBasket = 550;
        int xDeltaBasket = 30;

        int xChicken = 300;
        int yChicken = 10;
        int xDeltaChicken = 2;

        int score = 0;
        int lives = 3;
        bool isGameOver = false;

        List<Egg> eggs = new List<Egg>();

        Random rand = new Random();

        public Form5()
        {
            InitializeComponent();
            KeyPreview = true;

            Load += Form5_Load;
            KeyDown += Form5_KeyDown;
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            // Basket
            pbBasket.Size = new Size(70, 70);
            pbBasket.Location = new Point(xBasket, yBasket);
            pbBasket.SizeMode = PictureBoxSizeMode.StretchImage;
            pbBasket.Image = Image.FromFile(@"image/eat.png");
            Controls.Add(pbBasket);

            // Chicken
            pbChicken.Size = new Size(100, 100);
            pbChicken.Location = new Point(xChicken, yChicken);
            pbChicken.SizeMode = PictureBoxSizeMode.StretchImage;
            pbChicken.Image = Image.FromFile(@"image/b52.png");
            Controls.Add(pbChicken);

            // Chicken movement
            tmChicken.Interval = 10;
            tmChicken.Tick += tmChicken_Tick;
            tmChicken.Start();

            // Egg falling
            tmEgg.Interval = 10;
            tmEgg.Tick += tmEgg_Tick;
            tmEgg.Start();

            // Spawn egg repeatedly
            tmSpawn.Interval = 1200; // 1.2 giây tạo 1 quả trứng
            tmSpawn.Tick += tmSpawn_Tick;
            tmSpawn.Start();

            UpdateTitle();
        }

        private void UpdateTitle()
        {
            this.Text = $"Điểm: {score}  |  Mạng: {lives}";
        }

        // Tạo 1 quả trứng
        private void SpawnEgg()
        {
            Egg e = new Egg();
            e.Pb = new PictureBox();
            e.Pb.Size = new Size(40, 40);
            e.Pb.SizeMode = PictureBoxSizeMode.StretchImage;
            e.Pb.Image = Image.FromFile(@"image/bigboy.png");

            // Trứng rơi từ con gà
            e.x = xChicken + pbChicken.Width / 2 - 20;
            e.y = yChicken + pbChicken.Height;

            e.Pb.Location = new Point(e.x, e.y);

            eggs.Add(e);
            Controls.Add(e.Pb);
        }

        private void tmSpawn_Tick(object sender, EventArgs e)
        {
            if (!isGameOver)
                SpawnEgg();
        }

        private void tmEgg_Tick(object sender, EventArgs e)
        {
            if (isGameOver) return;

            // Duyệt tất cả trứng
            for (int i = eggs.Count - 1; i >= 0; i--)
            {
                Egg eg = eggs[i];
                if (isGameOver) return;
                if (!eg.broken)
                {
                    eg.y += 2;
                    eg.x += (int)(Math.Sin(eg.y / 10.0) * 2); // rung trái phải
                    eg.Pb.Location = new Point(eg.x, eg.y);

                    // Va chạm giỏ
                    if (eg.Pb.Bounds.IntersectsWith(pbBasket.Bounds))
                    {
                        eg.broken = true;
                        //eg.Pb.Image = Image.FromFile(@"image/boom.png");

                        // Hiệu ứng bay lên
                        ShowFloatingScore(eg.x, eg.y);

                        score++;
                        UpdateTitle();

                        RemoveEggLater(eg, 300);
                        continue;
                    }

                    // Chạm đất
                    if (eg.y > this.ClientSize.Height - eg.Pb.Height)
                    {
                        eg.broken = true;
                        eg.Pb.Image = Image.FromFile(@"image/boom.png");

                        lives--;
                        UpdateTitle();

                        if (lives <= 0)
                        {
                            GameOver();
                            return;
                        }

                        RemoveEggLater(eg, 350);
                        continue;
                    }
                }
            }
        }


        private void RemoveEggLater(Egg e, int delay)
        {
            Timer t = new Timer { Interval = delay };
            t.Tick += (s, ev) =>
            {
                t.Stop();
                t.Dispose();

                Controls.Remove(e.Pb);
                e.Pb.Dispose();
                eggs.Remove(e);
            };
            t.Start();
        }

        private void GameOver()
        {
            isGameOver = true;
            tmEgg.Stop();
            tmChicken.Stop();
            tmSpawn.Stop();

            DialogResult rs = MessageBox.Show(
                $"GAME OVER!\nĐiểm: {score}\n\nChơi lại?",
                "Game Over",
                MessageBoxButtons.YesNo
            );

            if (rs == DialogResult.Yes)
            {
                RestartGame();
            }
            else
            {
                this.Close();
            }
        }
        private void ShowFloatingScore(int x, int y)
        {
            Label lb = new Label();
            lb.Text = "+1";
            lb.Font = new Font("Arial", 16, FontStyle.Bold);
            lb.ForeColor = Color.Gold;
            lb.BackColor = Color.Transparent;
            lb.AutoSize = true;
            lb.Location = new Point(x, y);
            Controls.Add(lb);

            Timer t = new Timer();
            t.Interval = 15;

            int floatHeight = 0;

            t.Tick += (s, e) =>
            {
                floatHeight++;

                lb.Top -= 2;   // bay lên
                lb.Left += 1;  // nhẹ sang phải cho đẹp

                if (floatHeight > 30) // bay lên 30 lần → xoá
                {
                    t.Stop();
                    Controls.Remove(lb);
                    lb.Dispose();
                }
            };

            t.Start();
        }
        private void RestartGame()
        {
            isGameOver = false;

            score = 0;
            lives = 3;

            this.Text = "Điểm số: 0";

            // Reset gà
            xChicken = 300;
            pbChicken.Location = new Point(xChicken, yChicken);

            // Reset rổ
            xBasket = 300;
            pbBasket.Location = new Point(xBasket, yBasket);

            // Reset trứng
            ResetEgg();

            // bật lại timer
            tmChicken.Start();
            tmEgg.Start();
            tmSpawn.Start();
        }
        private void ResetEgg()
        {
            // Xóa toàn bộ trứng còn trên màn hình
            foreach (var eg in eggs)
            {
                if (eg.Pb != null)
                {
                    Controls.Remove(eg.Pb);
                    eg.Pb.Dispose();
                }
            }

            eggs.Clear();  // Xóa danh sách trứng
        }
        private void tmChicken_Tick(object sender, EventArgs e)
        {
            if (isGameOver) return;  // ⛔ DỪNG NGAY – KHÔNG DI CHUYỂN GÀ
            if (!tmChicken.Enabled) return;
            xChicken += xDeltaChicken;

            if (xChicken < 0 || xChicken > this.ClientSize.Width - pbChicken.Width)
            {
                xDeltaChicken *= -1;
            }

            pbChicken.Location = new Point(xChicken, yChicken);
        }

        private void Form5_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGameOver)
            {
                if (e.KeyCode == Keys.Enter)
                    RestartGame();

                return;
            }

            if (e.KeyCode == Keys.Right && xBasket < this.ClientSize.Width - pbBasket.Width)
                xBasket += xDeltaBasket;

            if (e.KeyCode == Keys.Left && xBasket > 0)
                xBasket -= xDeltaBasket;

            pbBasket.Location = new Point(xBasket, yBasket);
        }
    }

    // CLASS TRỨNG
    class Egg
    {
        public PictureBox Pb;
        public int x, y;
        public bool broken = false;
    }
}
    