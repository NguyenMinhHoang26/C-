using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NMHwin
{
    public partial class Form5 : Form
    {
        PictureBox pbBasket = new PictureBox();
        PictureBox pbEgg = new PictureBox();
        PictureBox pbChicken = new PictureBox();
        System.Windows.Forms.Timer tmEgg = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer tmChicken = new System.Windows.Forms.Timer();

        // Biến cho Rổ (Basket)
        int xBasket = 300;
        int yBasket = 550;
        int xDeltaBasket = 30; // Tốc độ di chuyển ngang của rổ

        // Biến cho Gà (Chicken)
        int xChicken = 300;
        int yChicken = 10;
        int xDeltaChicken = 5; // Tốc độ di chuyển ngang của gà

        // Biến cho Trứng (Egg)
        int xEgg = 300;
        int yEgg = 10;
        int yDeltaEgg = 3; // Tốc độ rơi của trứng

        int score = 0; // Thêm biến điểm số
        bool isEggBroken = false; // Trạng thái trứng
        public Form5()
        {
            InitializeComponent();
            this.KeyPreview = true;
            // Đảm bảo các sự kiện được gán
            this.Load += Form5_Load;
            this.KeyDown += Form5_KeyDown; // Gán Form5_KeyDown
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            tmEgg.Interval = 10;
            tmEgg.Tick += tmEgg_Tick;
            tmEgg.Start();

            // --- Thiết lập Timer Gà ---
            tmChicken.Interval = 10;
            tmChicken.Tick += tmChicken_Tick;
            tmChicken.Start();

            // --- Thiết lập PictureBox Rổ (Basket) ---
            pbBasket.SizeMode = PictureBoxSizeMode.StretchImage;
            pbBasket.Size = new Size(70, 70);
            pbBasket.Location = new Point(xBasket, yBasket);
            pbBasket.BackColor = Color.Transparent;
            this.Controls.Add(pbBasket);
            // Lưu ý: Đường dẫn có thể cần điều chỉnh:
            pbBasket.Image = Image.FromFile(@"image/eat.png");

            // --- Thiết lập PictureBox Trứng (Egg) ---
            pbEgg.SizeMode = PictureBoxSizeMode.StretchImage;
            pbEgg.Size = new Size(50, 50); // Lưu ý kích thước khác 
            pbEgg.Location = new Point(xEgg, yEgg);
            pbEgg.BackColor = Color.Transparent;
            this.Controls.Add(pbEgg);
            // Lưu ý: Đường dẫn có thể cần điều chỉnh:
            pbEgg.Image = Image.FromFile(@"image/bigboy.png");

            // --- Thiết lập PictureBox Gà (Chicken) ---
            pbChicken.SizeMode = PictureBoxSizeMode.StretchImage;
            pbChicken.Size = new Size(100, 100);
            pbChicken.Location = new Point(xChicken, yChicken);
            pbChicken.BackColor = Color.Transparent;
            this.Controls.Add(pbChicken);
            // Lưu ý: Đường dẫn có thể cần điều chỉnh:
            pbChicken.Image = Image.FromFile(@"image/b52.png");
        }
        void tmEgg_Tick(object sender, EventArgs e)
        {
            // Cập nhật vị trí Y: Trứng rơi xuống
            if (!isEggBroken)
            {
                yEgg += yDeltaEgg; // Trứng rơi xuống

                // --- KIỂM TRA VA CHẠM ---

                // 1. Va chạm với RỔ (Bắt trứng)
                // Kiểm tra xem hình chữ nhật của pbEgg có giao với hình chữ nhật của pbBasket không.
                if (pbEgg.Bounds.IntersectsWith(pbBasket.Bounds))
                {
                    score++; // Tăng điểm
                    isEggBroken = true; // Đánh dấu trứng đã được bắt/xử lý
                                        // Thông báo điểm (hoặc dùng Label để hiển thị)
                    this.Text = "Điểm số: " + score;

                    // Dừng timer để tạo cảm giác 'bắt' và reset trứng sau đó
                    tmEgg.Stop();
                    // Gọi phương thức reset trứng sau 500ms (0.5 giây)
                    System.Windows.Forms.Timer resetTimer = new System.Windows.Forms.Timer { Interval = 500, Enabled = true };
                    resetTimer.Tick += (s, ev) =>
                    {
                        ResetEgg();
                        resetTimer.Stop();
                        resetTimer.Dispose();
                    };
                }

                // 2. Va chạm với ĐÁY (Làm rơi trứng)
                // Khi trứng chạm đáy Form
                if (yEgg > this.ClientSize.Height - pbEgg.Height)
                {
                    // Trứng vỡ
                    pbEgg.Image = Image.FromFile(@"image/boom.png");
                    isEggBroken = true;

                    // Tạm dừng game hoặc hiện thông báo
                    tmEgg.Stop();
                    MessageBox.Show($"Bạn đã làm rơi trứng! Điểm cuối cùng: {score}", "Game Over");

                    // Có thể thêm logic để Reset game tại đây.
                }

                // 3. (Va chạm với Đỉnh) - Giữ lại logic cũ nhưng không cần thiết
                if (yEgg <= 0)
                {
                    // Logic này thường chỉ dùng cho vật thể nảy lên. 
                    // Trong game bắt trứng thì chủ yếu là kiểm tra va chạm đáy.
                }

                // Cập nhật vị trí hiển thị
                pbEgg.Location = new Point(xEgg, yEgg);
            }
        }
        private void ResetEgg()
        {
            Random rand = new Random();

            // Đặt lại vị trí X ngẫu nhiên (hoặc dựa trên vị trí của gà pbChicken)
            // Ở đây ta dùng vị trí ngẫu nhiên để trứng rơi từ vị trí bất kỳ
            xEgg = rand.Next(0, this.ClientSize.Width - pbEgg.Width);
            yEgg = 10; // Đặt lại vị trí Y ở đỉnh

            // Đặt lại hình ảnh ban đầu
            pbEgg.Image = Image.FromFile(@"image/bigboy.png");

            // Reset trạng thái
            isEggBroken = false;

            // Bắt đầu lại timer để trứng tiếp tục rơi
            tmEgg.Start();
        }
        void tmChicken_Tick(object sender, EventArgs e)
        {
            // Cập nhật vị trí X: Gà di chuyển ngang
            xChicken += xDeltaChicken;

            // Kiểm tra va chạm với ranh giới Form (trái hoặc phải)
            if (xChicken > this.ClientSize.Width - pbChicken.Width || xChicken <= 0)
            {
                // Đảo chiều di chuyển khi chạm biên
                xDeltaChicken = -xDeltaChicken;
            }

            // Cập nhật vị trí hiển thị
            pbChicken.Location = new Point(xChicken, yChicken);
        }
        private void Form5_KeyDown(object sender, KeyEventArgs e)
        {
            // Phím Mũi tên Phải (Key Value 39)
            if (e.KeyValue == 39 && xBasket < this.ClientSize.Width - pbBasket.Width)
            {
                xBasket += xDeltaBasket;
            }

            // Phím Mũi tên Trái (Key Value 37)
            if (e.KeyValue == 37 && xBasket > 0)
            {
                xBasket -= xDeltaBasket;
            }

            // Cập nhật vị trí hiển thị của Rổ
            pbBasket.Location = new Point(xBasket, yBasket);
        }
    }
}
