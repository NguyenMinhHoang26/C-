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
        // ===== FIX DUP =====
        private bool _loadedOnce = false;

        // ================= TIMER =================
        private WinTimer gameTimer;

        // ================= IMAGE =================
        private Image imgBackground, imgBoat, imgFish, imgBomb, imgBoss;

        // ================= BOAT =================
        private PictureBox pbBoat;
        private int boatSpeed = 20;
        private bool moveLeft, moveRight;

        // ================= HOOK =================
        private bool hookDown = false;
        private int hookLength = 0;
        private int hookSpeed = 12;

        // ================= SCORE / LEVEL / TIME =================
        private int score = 0;
        private int level = 1;
        private DateTime endTime;

        private Random rand = new Random();

        // ================= FISH =================
        private class Fish
        {
            public PictureBox Pb;
            public int Speed;
            public int Dir;
        }
        private readonly List<Fish> fishes = new();

        // ================= BOMB =================
        private class Bomb
        {
            public PictureBox Pb;
            public int Speed;
            public int Dir;
        }
        private readonly List<Bomb> bombs = new();

        // ================= BOSS =================
        private bool isBossFight = false;
        private int bossHp = 0;
        private PictureBox pbBoss;
        private int bossSpeed = 10;
        private int bossDir = 1;

        // ================= MUSIC =================
        private WaveOutEvent bgOut;
        private AudioFileReader bgReader;

        public Form7()
        {
            InitializeComponent();

            DoubleBuffered = true;
            KeyPreview = true;

            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;

            gameTimer = new WinTimer();
            gameTimer.Interval = 30;

            // ===== FIX DUP EVENT: gỡ rồi gắn lại để không bị bind 2 lần =====
            gameTimer.Tick -= GameLoop;
            gameTimer.Tick += GameLoop;

            Load -= Form7_Load; Load += Form7_Load;
            Paint -= Form7_Paint; Paint += Form7_Paint;
            KeyDown -= Form7_KeyDown; KeyDown += Form7_KeyDown;
            KeyUp -= Form7_KeyUp; KeyUp += Form7_KeyUp;
            Resize -= Form7_Resize; Resize += Form7_Resize;
            FormClosing -= Form7_FormClosing;
            FormClosing += Form7_FormClosing;
        }

        private void Form7_Resize(object sender, EventArgs e) => UpdateBoatY();

        // ================= LOAD =================
        private void Form7_Load(object sender, EventArgs e)
        {
            // ===== FIX DUP LOAD =====
            if (_loadedOnce) return;
            _loadedOnce = true;

            imgBackground = LoadImage("image", "underwater_bg.png");
            imgBoat = LoadImage("image", "boat.png");
            imgFish = LoadImage("image", "fish.png");
            imgBomb = LoadImage("image", "bigboy.png");
            imgBoss = LoadImage("image", "Boss.png");

            // Nền bằng BackgroundImage (không vẽ nền trong Paint)
            if (imgBackground != null)
            {
                BackgroundImage = imgBackground;
                BackgroundImageLayout = ImageLayout.Stretch;
            }

            // Nếu có thuyền cũ (do load lại / code khác), xóa trước
            if (pbBoat != null)
            {
                Controls.Remove(pbBoat);
                pbBoat.Dispose();
                pbBoat = null;
            }

            // Boat (tạo đúng 1 lần)
            pbBoat = new PictureBox
            {
                Size = new Size(150, 100),
                Image = imgBoat,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };
            Controls.Add(pbBoat);
            pbBoat.Parent = this;
            pbBoat.BringToFront();

            pbBoat.Left = ClientSize.Width / 2 - pbBoat.Width / 2;
            UpdateBoatY();

            RestartGameInternal(playMusic: true);
        }

        // ================= GAME LOOP =================
        private void GameLoop(object sender, EventArgs e)
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

            if (isBossFight && pbBoss != null)
            {
                pbBoss.Left += bossSpeed * bossDir;
                if (pbBoss.Left <= 0 || pbBoss.Right >= ClientSize.Width)
                    bossDir *= -1;
            }

            Invalidate();
        }

        // ================= SOUND =================
        private void PlaySfx(string file)
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

        private void PlayBackgroundMusic()
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "sound", "beach-366853.mp3");
                if (!File.Exists(path)) return;

                bgReader?.Dispose();
                bgOut?.Dispose();

                bgReader = new AudioFileReader(path);
                bgOut = new WaveOutEvent();
                bgOut.Init(bgReader);

                bgOut.PlaybackStopped -= BgOut_PlaybackStopped;
                bgOut.PlaybackStopped += BgOut_PlaybackStopped;

                bgOut.Play();
            }
            catch { }
        }

        private void BgOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (bgReader == null || bgOut == null) return;
            if (bgReader.Position >= bgReader.Length)
            {
                bgReader.Position = 0;
                bgOut.Play();
            }
        }

        // ================= HOOK COLLISION =================
        private void CheckHookCatch()
        {
            int hx = pbBoat.Left + pbBoat.Width / 2;
            Rectangle hookRect = new Rectangle(
                hx - 5,
                pbBoat.Bottom + hookLength - 5,
                10, 10);

            // Boss
            if (isBossFight && pbBoss != null && pbBoss.Bounds.IntersectsWith(hookRect))
            {
                bossHp--;
                hookDown = false;
                hookLength = 0;

                PlaySfx("yeah-boy-114748.mp3");

                if (bossHp <= 0)
                    EndBossFight();
                return;
            }

            // Fish
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

            // Bomb
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
        private void CheckLevelUp()
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
        private void StartBossFight()
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
                BackColor = Color.Transparent,
                Location = new Point(
                    ClientSize.Width / 2 - 70,
                    ClientSize.Height - 220)
            };
            Controls.Add(pbBoss);
            pbBoss.Parent = this;
            pbBoss.BringToFront();
        }

        private void EndBossFight()
        {
            isBossFight = false;

            if (pbBoss != null)
            {
                Controls.Remove(pbBoss);
                pbBoss.Dispose();
                pbBoss = null;
            }

            score += 2;
            endTime = DateTime.Now.AddSeconds(30);

            for (int i = 0; i < 5; i++) SpawnFish();
        }

        // ================= SPAWN =================
        private void SpawnFish()
        {
            if (isBossFight) return;

            var pb = new PictureBox
            {
                Size = new Size(40, 20),
                Image = imgFish,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                Location = new Point(
                    rand.Next(50, Math.Max(51, ClientSize.Width - 50)),
                    rand.Next(Math.Max(1, ClientSize.Height / 2), Math.Max(2, ClientSize.Height - 60)))
            };
            Controls.Add(pb);
            pb.Parent = this;
            pb.BringToFront();

            fishes.Add(new Fish
            {
                Pb = pb,
                Speed = 20 + level,
                Dir = rand.Next(0, 2) == 0 ? 1 : -1
            });
        }

        private void SpawnBomb()
        {
            var pb = new PictureBox
            {
                Size = new Size(30, 30),
                Image = imgBomb,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                Location = new Point(
                    rand.Next(50, Math.Max(51, ClientSize.Width - 50)),
                    rand.Next(Math.Max(1, ClientSize.Height / 2), Math.Max(2, ClientSize.Height - 60)))
            };
            Controls.Add(pb);
            pb.Parent = this;
            pb.BringToFront();

            bombs.Add(new Bomb
            {
                Pb = pb,
                Speed = 3 + level * 2,
                Dir = rand.Next(0, 2) == 0 ? 1 : -1
            });
        }

        private void ClearFish()
        {
            foreach (var f in fishes)
            {
                Controls.Remove(f.Pb);
                f.Pb.Dispose();
            }
            fishes.Clear();
        }

        private void ClearBombs()
        {
            foreach (var b in bombs)
            {
                Controls.Remove(b.Pb);
                b.Pb.Dispose();
            }
            bombs.Clear();
        }

        // ================= DRAW =================
        private void Form7_Paint(object sender, PaintEventArgs e)
        {
            if (hookDown && pbBoat != null)
            {
                int hx = pbBoat.Left + pbBoat.Width / 2;
                e.Graphics.DrawLine(Pens.Black, hx, pbBoat.Bottom, hx, pbBoat.Bottom + hookLength);
                e.Graphics.FillEllipse(Brushes.Red, hx - 5, pbBoat.Bottom + hookLength - 5, 10, 10);
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
        private void Form7_KeyDown(object sender, KeyEventArgs e)
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

        private void Form7_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A) moveLeft = false;
            if (e.KeyCode == Keys.D) moveRight = false;
        }

        // ================= UTILS =================
        private void UpdateBoatY()
        {
            if (pbBoat == null) return;
            pbBoat.Top = (int)(ClientSize.Height * 0.16) - pbBoat.Height / 2;
            pbBoat.BringToFront();
        }

        private Image LoadImage(string folder, string file)
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, folder, file);
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                return Image.FromStream(fs);
            }
            catch { return null; }
        }

        // ================= GAME OVER / RESTART =================
        private void GameOver()
        {
            gameTimer.Stop();

            var result = MessageBox.Show(
                $"GAME OVER\nScore: {score}\nLevel: {level}\n\nChơi lại?",
                "Game Over",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
                RestartGameInternal(playMusic: false);
            else
                Close();
        }

        private void RestartGameInternal(bool playMusic)
        {
            hookDown = false;
            hookLength = 0;
            moveLeft = moveRight = false;

            score = 0;
            level = 1;
            endTime = DateTime.Now.AddSeconds(30);

            ClearFish();
            ClearBombs();

            if (pbBoss != null)
            {
                Controls.Remove(pbBoss);
                pbBoss.Dispose();
                pbBoss = null;
            }
            isBossFight = false;
            bossHp = 0;

            if (pbBoat != null)
            {
                pbBoat.Left = ClientSize.Width / 2 - pbBoat.Width / 2;
                UpdateBoatY();
                pbBoat.BackColor = Color.Transparent;
                pbBoat.BringToFront();
            }

            for (int i = 0; i < 5; i++) SpawnFish();
            SpawnBomb();

            if (playMusic) PlayBackgroundMusic();

            gameTimer.Start();
            Invalidate();
        }

        private void Form7_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                gameTimer?.Stop();

                if (bgOut != null)
                {
                    bgOut.PlaybackStopped -= BgOut_PlaybackStopped;
                    bgOut.Stop();
                    bgOut.Dispose();
                    bgOut = null;
                }

                bgReader?.Dispose();
                bgReader = null;
            }
            catch { }
        }
    }
}
