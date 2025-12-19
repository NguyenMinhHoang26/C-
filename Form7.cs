using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace NMHwin
{
    public partial class Form7 : Form
    {
        PictureBox pbBoat = new PictureBox();
        int boatX = 300;
        int boatY = 50;

        int hookLength = 0;
        bool hookGoingDown = false;
        int maxHookLength = 300;
        int hookSpeed = 5;
        bool hookHasFish = false;
        Fish caughtFish = null;

        Timer tmFish = new Timer();
        Timer tmHook = new Timer();
        Timer tmWater = new Timer();
        List<Fish> fishes = new List<Fish>();
        List<Bomb> bombs = new List<Bomb>();
        Random rand = new Random();
        Timer tmBoat = new Timer();

        int score = 0;
        int waterOffset = 0;
        int boatWaveOffset = 0;
        int boatDirection = 1; // 1 = sang phải, -1 = sang trái
        int boatSpeed = 2;     // tốc độ thuyền
        int level = 1;  // Initial level
        int scoreForNextLevel = 5;  // Number of points needed to level up


        public Form7()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Load += Form7_Load;
            this.Paint += Form7_Paint;
            this.KeyDown += Form7_KeyDown;
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            // --- Thuyền ---
            pbBoat.Size = new Size(120, 60);
            pbBoat.Location = new Point(boatX, boatY);
            pbBoat.Image = Image.FromFile("image/boat.png");
            pbBoat.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(pbBoat);
            // --- Timer thuyền ---
            tmBoat.Interval = 30;
            tmBoat.Tick += AnimateBoat;
            tmBoat.Start();

            // --- Timer hook ---
            tmHook.Interval = 20;
            tmHook.Tick += AnimateHook;
            tmHook.Start();

            // --- Timer cá ---
            tmFish.Interval = 30;
            tmFish.Tick += AnimateFish;
            tmFish.Start();

            // --- Timer nền nước ---
            tmWater.Interval = 50;
            tmWater.Tick += AnimateWater;
            tmWater.Start();

            // --- Spawn cá và bom ---
            for (int i = 0; i < 5; i++) SpawnFish();
            for (int i = 0; i < 2; i++) SpawnBomb();

            this.Text = "Điểm: 0 | Level: " + level;  // Update the title to include level

        }

        private void AnimateBoat(object sender, EventArgs e)
        {
            boatX += boatSpeed * boatDirection;
            if (boatX <= 0 || boatX >= this.ClientSize.Width - pbBoat.Width)
                boatDirection *= -1; // đổi hướng khi chạm biên

            pbBoat.Location = new Point(boatX, boatY + boatWaveOffset);
            Invalidate();
        }
        private void AnimateWater(object sender, EventArgs e)
        {
            waterOffset += 1;
            if (waterOffset > this.ClientSize.Width) waterOffset = 0;

            // Thuyền nhấp nhô nhẹ
            boatWaveOffset = (int)(5 * Math.Sin(waterOffset * 0.05));

            Invalidate(); // Vẽ lại Form
        }

        private void Form7_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // --- Vẽ sóng dưới thuyền ---
            using (Pen wavePen = new Pen(Color.FromArgb(120, Color.Blue), 2))
            {
                for (int i = 0; i < this.ClientSize.Width; i += 10)
                {
                    int y = (int)(10 * Math.Sin((i + waterOffset) * 0.1) + boatY + pbBoat.Height / 2);
                    g.DrawLine(wavePen, i, y, i + 10, y);
                }
            }

            // --- Dây câu và mồi ---
            Pen pen = new Pen(Color.Black, 2);
            int hookTipX = boatX + pbBoat.Width / 2;
            int hookTipY = boatY + pbBoat.Height + hookLength;
            g.DrawLine(pen, boatX + pbBoat.Width / 2, boatY + pbBoat.Height, hookTipX, hookTipY);
            g.FillEllipse(Brushes.Red, hookTipX - 5, hookTipY - 5, 10, 10);
            pen.Dispose();

            // --- Thuyền lên trên ---
            pbBoat.Location = new Point(boatX, boatY + boatWaveOffset);
            pbBoat.BringToFront();

            // --- Đảm bảo cá và bom nổi trên sóng ---
            foreach (var f in fishes) f.Pb.BringToFront();
            foreach (var b in bombs) b.Pb.BringToFront();

            // Vẽ điểm số và cấp độ trực tiếp trên màn hình
            Font font = new Font("Arial", 16, FontStyle.Bold);
            Brush brush = Brushes.Black;
            string text = "Điểm: " + score + " | Level: " + level;
            g.DrawString(text, font, brush, new PointF(10, 10));  // Điều chỉnh vị trí theo nhu cầu
        }

        private void AnimateHook(object sender, EventArgs e)
        {
            if (hookGoingDown)
            {
                hookLength += hookSpeed;

                if (!hookHasFish)
                {
                    // Kiểm tra va chạm với cá
                    foreach (var f in fishes)
                    {
                        Rectangle hookRect = new Rectangle(
                            boatX + pbBoat.Width / 2 - 5,
                            boatY + pbBoat.Height + hookLength - 5, 10, 10);

                        if (f.Pb.Bounds.IntersectsWith(hookRect))
                        {
                            hookHasFish = true;
                            caughtFish = f;
                            break;
                        }
                    }

                    // Kiểm tra va chạm với bom
                    foreach (var b in bombs)
                    {
                        Rectangle hookRect = new Rectangle(
                            boatX + pbBoat.Width / 2 - 5,
                            boatY + pbBoat.Height + hookLength - 5, 10, 10);

                        if (b.Pb.Bounds.IntersectsWith(hookRect))
                        {
                            // Dừng tất cả timer
                            tmHook.Stop();
                            tmFish.Stop();
                            tmWater.Stop();
                            tmBoat.Stop(); // dừng thuyền
                            MessageBox.Show($"GAME OVER! Bạn trúng bom!\nĐiểm: {score}", "Game Over", MessageBoxButtons.OK);
                            this.Close();
                            return;
                        }
                    }
                }

                if (hookLength >= maxHookLength || hookHasFish)
                    hookGoingDown = false;
            }
            else
            {
                hookLength -= hookSpeed;
                if (hookLength < 0) hookLength = 0;

                if (hookHasFish && caughtFish != null)
                {
                    caughtFish.y = boatY + pbBoat.Height + hookLength;
                    caughtFish.x = boatX + pbBoat.Width / 2 - caughtFish.Pb.Width / 2;
                    caughtFish.Pb.Location = new Point(caughtFish.x, caughtFish.y);

                    if (hookLength == 0)
                    {
                        Controls.Remove(caughtFish.Pb);
                        fishes.Remove(caughtFish);
                        hookHasFish = false;
                        caughtFish = null;
                        score++;
                        this.Text = "Điểm: " + score + " | Level: " + level;


                        // Check for level up
                        if (score >= level * scoreForNextLevel)  // Check if player has enough points for next level
                        {
                            level++;
                            scoreForNextLevel *= 2;  // Increase the difficulty for the next level
                            IncreaseDifficulty();    // Call method to increase difficulty
                        }

                        SpawnFish();
                    }

                }
            }

            Invalidate();
        }
        private void IncreaseDifficulty()
        {
            boatSpeed++;
            for (int i = 0; i < 2; i++) SpawnBomb();
        }

        private void AnimateFish(object sender, EventArgs e)
        {
            // --- Cá di chuyển ---
            foreach (var f in fishes)
            {
                f.x += f.speed * f.direction;
                if (f.x < 0 || f.x > this.ClientSize.Width - f.Pb.Width)
                    f.direction *= -1;

                f.Pb.Image = f.direction == 1 ? Image.FromFile("image/fish.png") : Image.FromFile("image/fish_flip.png");
                f.Pb.Location = new Point(f.x, f.y);
            }

            // --- Bomb di chuyển giống cá ---
            foreach (var b in bombs)
            {
                if (!b.direction.HasValue) b.direction = rand.Next(0, 2) == 0 ? 1 : -1; // khởi tạo hướng nếu null
                b.x += b.speed * b.direction.Value;
                if (b.x < 0 || b.x > this.ClientSize.Width - b.Pb.Width)
                    b.direction *= -1;

                b.Pb.Location = new Point(b.x, b.y);
            }
        }


        private void SpawnFish()
        {
            Fish f = new Fish();
            f.Pb = new PictureBox();
            f.Pb.Size = new Size(40, 20);
            f.Pb.SizeMode = PictureBoxSizeMode.StretchImage;
            f.Pb.Image = Image.FromFile("image/fish.png");
            f.x = rand.Next(50, this.ClientSize.Width - 50);
            f.y = rand.Next(this.ClientSize.Height / 2 + 50, this.ClientSize.Height - 50);
            f.speed = rand.Next(2, 4);
            f.direction = rand.Next(0, 2) == 0 ? 1 : -1;
            f.Pb.Location = new Point(f.x, f.y);
            fishes.Add(f);
            Controls.Add(f.Pb);
        }

        private void SpawnBomb()
        {
            Bomb b = new Bomb();
            b.Pb = new PictureBox();
            b.Pb.Size = new Size(30, 30);
            b.Pb.SizeMode = PictureBoxSizeMode.StretchImage;
            b.Pb.Image = Image.FromFile("image/bigboy.png");
            b.x = rand.Next(50, this.ClientSize.Width - 50);
            b.y = rand.Next(this.ClientSize.Height / 2 + 50, this.ClientSize.Height - 50);
            b.speed = rand.Next(2, 4);          // tốc độ giống cá
            b.direction = rand.Next(0, 2) == 0 ? 1 : -1; // hướng di chuyển
            b.Pb.Location = new Point(b.x, b.y);
            bombs.Add(b);
            Controls.Add(b.Pb);
        }

        private void Form7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && boatX > 0) boatX -= 10;
            if (e.KeyCode == Keys.Right && boatX < this.ClientSize.Width - pbBoat.Width) boatX += 10;
            if (e.KeyCode == Keys.Space) hookGoingDown = true;
            pbBoat.Location = new Point(boatX, boatY + boatWaveOffset);
            Invalidate();
        }
    }

    class Fish
    {
        public PictureBox Pb;
        public int x, y;
        public int speed;
        public int direction;
    }

    class Bomb
    {
        public PictureBox Pb;
        public int x, y;
        public int speed;
        public int? direction; // 1 hoặc -1
    }
}
