using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using WinTimer = System.Windows.Forms.Timer;

namespace NMHwin
{
    public partial class Form7 : Form
    {
        // ================= TIMER =================
        WaveOutEvent sfxOut;

        WinTimer gameTimer;

        // ================= IMAGE =================
        Image imgBackground, imgBoat, imgFish, imgBomb, imgBoss;

        // ================= BOAT =================
        PictureBox pbBoat;
        int boatSpeed = 8;
        bool moveLeft, moveRight;

        // ================= HOOK =================
        bool hookDown = false;
        int hookLength = 0;
        int hookSpeed = 12;

        // ================= SCORE / LEVEL / TIME =================
        int score = 0;
        int level = 1;
        DateTime endTime;

        Random rand = new Random();

        // ================= FISH =================
        class Fish
        {
            public PictureBox Pb;
            public int Speed;
            public int Dir;
        }
        List<Fish> fishes = new();

        // ================= BOMB =================
        class Bomb
        {
            public PictureBox Pb;
            public int Speed;
            public int Dir;
        }
        List<Bomb> bombs = new();

        // ================= BOSS =================
        bool isBossFight = false;
        int bossHp = 0;
        PictureBox pbBoss;
        int bossSpeed = 4;
        int bossDir = 1;

        // =====================================================
        // ================= MUSIC =================
        WaveOutEvent bgOut;
        AudioFileReader bgReader;

        public Form7()
        {
            InitializeComponent();

            DoubleBuffered = true;
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;

            gameTimer = new WinTimer();
            gameTimer.Interval = 30;
            gameTimer.Tick += GameLoop;

            Paint += Form7_Paint;
            KeyDown += Form7_KeyDown;
            KeyUp += Form7_KeyUp;
            Resize += (s, e) => UpdateBoatY();
        }

        // ================= LOAD =================
        private void Form7_Load(object sender, EventArgs e)
        {
            imgBackground = LoadImage("image", "underwater_bg.png");
            imgBoat = LoadImage("image", "boat.png");
            imgFish = LoadImage("image", "fish.png");
            imgBomb = LoadImage("image", "bigboy.png");
            imgBoss = LoadImage("image", "Boss.png");

            pbBoat = new PictureBox
            {
                Size = new Size(120, 60),
                Image = imgBoat,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            Controls.Add(pbBoat);
            pbBoat.Left = ClientSize.Width / 2 - pbBoat.Width / 2;
            UpdateBoatY();

            endTime = DateTime.Now.AddSeconds(30);

            for (int i = 0; i < 5; i++) SpawnFish();
            SpawnBomb();
            PlayBackgroundMusic();


            gameTimer.Start();
        }

        // ================= GAME LOOP =================
        void PlaySfx(string file)
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "sound", file);
                if (!File.Exists(path)) return;

                var reader = new AudioFileReader(path);
                var output = new WaveOutEvent();

                output.Init(reader);
                output.Play();

                output.PlaybackStopped += (s, e) =>
                {
                    output.Dispose();
                    reader.Dispose();
                };
            }
            catch { }
        }

        void GameLoop(object sender, EventArgs e)
        {
            if (pbBoat == null) return;

            if ((endTime - DateTime.Now).TotalSeconds <= 0)
            {
                GameOver();
                return;
            }

            if (moveLeft && pbBoat.Left > 0) pbBoat.Left -= boatSpeed;
            if (moveRight && pbBoat.Right < ClientSize.Width) pbBoat.Left += boatSpeed;

            if (hookDown)
            {
                hookLength += hookSpeed;
                CheckHookCatch();

                if (pbBoat.Bottom + hookLength >= ClientSize.Height)
                {
                    hookDown = false;
                    hookLength = 0;
                }
            }

            foreach (var f in fishes)
            {
                f.Pb.Left += f.Speed * f.Dir;
                if (f.Pb.Left <= 0 || f.Pb.Right >= ClientSize.Width)
                    f.Dir *= -1;
            }

            foreach (var b in bombs)
            {
                b.Pb.Left += b.Speed * b.Dir;
                if (b.Pb.Left <= 0 || b.Pb.Right >= ClientSize.Width)
                    b.Dir *= -1;
            }
            // ===== BOSS MOVE =====
            if (isBossFight && pbBoss != null)
            {
                pbBoss.Left += bossSpeed * bossDir;
                if (pbBoss.Left <= 0 || pbBoss.Right >= ClientSize.Width)
                    bossDir *= -1;
            }

            Invalidate();
        }
        void PlayBackgroundMusic()
        {
            try
            {
                string path = Path.Combine(
                    Application.StartupPath,
                    "sound",
                    "beach-366853.mp3"
                );

                if (!File.Exists(path)) return;

                bgReader = new AudioFileReader(path);
                bgOut = new WaveOutEvent();
                bgOut.Init(bgReader);

                bgOut.PlaybackStopped += BgOut_PlaybackStopped;
                bgOut.Play();
            }
            catch { }
        }

        void BgOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (bgReader == null || bgOut == null) return;
            if (bgReader.Position >= bgReader.Length)
            {
                bgReader.Position = 0;
                bgOut.Play();
            }
        }


        // ================= HOOK COLLISION =================
        void CheckHookCatch()
        {
            int hx = pbBoat.Left + pbBoat.Width / 2;
            Rectangle hookRect = new Rectangle(
                hx - 5,
                pbBoat.Bottom + hookLength - 5,
                10, 10);

            // ===== BOSS =====
            if (isBossFight && pbBoss != null &&
                pbBoss.Bounds.IntersectsWith(hookRect))
            {
                bossHp--;
                hookDown = false;
                hookLength = 0;

                if (bossHp <= 0)
                    EndBossFight();
                return;
            }

            // ===== FISH =====
            for (int i = fishes.Count - 1; i >= 0; i--)
            {
                if (fishes[i].Pb.Bounds.IntersectsWith(hookRect))
                {
                    Controls.Remove(fishes[i].Pb);
                    fishes[i].Pb.Dispose();
                    fishes.RemoveAt(i);

                    score++;
                    PlaySfx("yeah-boy-114748.mp3");
                    CheckLevelUp();

                    hookDown = false;
                    hookLength = 0;
                    if (!isBossFight) SpawnFish();
                    return;
                }
            }

            // ===== BOMB =====
            foreach (var b in bombs)
            {
                if (b.Pb.Bounds.IntersectsWith(hookRect))
                {
                    PlaySfx("explosion-42132.mp3");
                    GameOver();
                    return;
                }
            }
        }

        // ================= LEVEL UP =================
        void CheckLevelUp()
        {
            if (score >= level * 3)
            {
                level++;
                endTime = DateTime.Now.AddSeconds(30);

                if (level % 5 == 0)
                    StartBossFight();
                else
                    SpawnBomb();
            }
        }

        // ================= BOSS =================
        void StartBossFight()
        {
            isBossFight = true;
            bossHp = 3;

            ClearFish();
            ClearBombs();

            for (int i = 0; i < level * 2; i++)
                SpawnBomb();

            pbBoss = new PictureBox
            {
                Size = new Size(140, 140),
                Image = imgBoss,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(
                    ClientSize.Width / 2 - 70,
                    ClientSize.Height - 220) // dưới đáy
            };
            Controls.Add(pbBoss);
        }

        void EndBossFight()
        {
            isBossFight = false;

            Controls.Remove(pbBoss);
            pbBoss.Dispose();
            pbBoss = null;

            score += 2;
            endTime = DateTime.Now.AddSeconds(30);

            for (int i = 0; i < 5; i++) SpawnFish();
        }

        // ================= SPAWN =================
        void SpawnFish()
        {
            if (isBossFight) return;

            var pb = new PictureBox
            {
                Size = new Size(40, 20),
                Image = imgFish,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(
                    rand.Next(50, ClientSize.Width - 50),
                    rand.Next(ClientSize.Height / 2, ClientSize.Height - 60))
            };
            Controls.Add(pb);

            fishes.Add(new Fish
            {
                Pb = pb,
                Speed = 2 + level,
                Dir = rand.Next(0, 2) == 0 ? 1 : -1
            });
        }

        void SpawnBomb()
        {
            var pb = new PictureBox
            {
                Size = new Size(30, 30),
                Image = imgBomb,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(
                    rand.Next(50, ClientSize.Width - 50),
                    rand.Next(ClientSize.Height / 2, ClientSize.Height - 60))
            };
            Controls.Add(pb);

            bombs.Add(new Bomb
            {
                Pb = pb,
                Speed = 3 + level * 2,
                Dir = rand.Next(0, 2) == 0 ? 1 : -1
            });
        }

        void ClearFish()
        {
            foreach (var f in fishes)
            {
                Controls.Remove(f.Pb);
                f.Pb.Dispose();
            }
            fishes.Clear();
        }

        void ClearBombs()
        {
            foreach (var b in bombs)
            {
                Controls.Remove(b.Pb);
                b.Pb.Dispose();
            }
            bombs.Clear();
        }

        // ================= DRAW =================
        void Form7_Paint(object sender, PaintEventArgs e)
        {
            if (imgBackground != null)
                e.Graphics.DrawImage(imgBackground, ClientRectangle);

            if (hookDown)
            {
                int hx = pbBoat.Left + pbBoat.Width / 2;
                e.Graphics.DrawLine(Pens.Black, hx, pbBoat.Bottom,
                    hx, pbBoat.Bottom + hookLength);
                e.Graphics.FillEllipse(Brushes.Red,
                    hx - 5, pbBoat.Bottom + hookLength - 5, 10, 10);
            }

            TimeSpan t = endTime - DateTime.Now;
            if (t < TimeSpan.Zero) t = TimeSpan.Zero;

            e.Graphics.DrawString(
                $"Score: {score}   Level: {level}   Time: {t:mm\\:ss}",
                new Font("Arial", 16, FontStyle.Bold),
                Brushes.White,
                10, 10);
        }

        // ================= INPUT =================
        void Form7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A) moveLeft = true;
            if (e.KeyCode == Keys.D) moveRight = true;

            if (e.KeyCode == Keys.Space && !hookDown)
            {
                hookDown = true;
                hookLength = 0;
            }

            if (e.KeyCode == Keys.Escape) Close();
        }

        void Form7_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A) moveLeft = false;
            if (e.KeyCode == Keys.D) moveRight = false;
        }

        // ================= UTILS =================
        void UpdateBoatY()
        {
            if (pbBoat == null) return;
            pbBoat.Top = (int)(ClientSize.Height * 0.16) - pbBoat.Height / 2;
        }

        Image LoadImage(string folder, string file)
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, folder, file);
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                return Image.FromStream(fs);
            }
            catch { return null; }
        }

        void GameOver()
        {
            gameTimer.Stop();

            if (bgOut != null)
            {
                bgOut.PlaybackStopped -= BgOut_PlaybackStopped;
                bgOut.Stop();
                bgOut.Dispose();
                bgOut = null;
            }

            if (bgReader != null)
            {
                bgReader.Dispose();
                bgReader = null;
            }

            MessageBox.Show($"GAME OVER\nScore: {score}\nLevel: {level}");
            Close();
        }


    }
}
