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
    public partial class Form3 : Form
    {
        PictureBox pb = new PictureBox();
        System.Windows.Forms.Timer tmGame = new System.Windows.Forms.Timer();
        int xBall = 0;
        int yBall = 0;
        int xDelta = 5;
        int yDelta = 5;
        public Form3()
        {
            InitializeComponent();
        }


        private void Form3_Load(object sender, EventArgs e)
        {
            // Cài đặt Timer cho game loop
            tmGame.Interval = 10;
            tmGame.Tick += tmGame_Tick; // Đảm bảo đã có Timer control tên là 'tmGame'
            tmGame.Start();

            // Cài đặt PictureBox
            pb.SizeMode = PictureBoxSizeMode.StretchImage; // Đảm bảo đã có PictureBox control tên là 'pb'
            pb.Size = new Size(100, 100);
            // Giả sử xBall và yBall là các biến int đã được khai báo và khởi tạo
            pb.Location = new Point(xBall, yBall);
            this.Controls.Add(pb); // Thêm PictureBox vào Form
            pb.ImageLocation = @"image/ball.jpg"; // Đường dẫn tuyệt đối tới file ảnh
        }

        // Hàm xử lý sự kiện Tick của Timer
        private void tmGame_Tick(object sender, EventArgs e)
        {
            // Cập nhật vị trí của bóng
            // Giả sử xDelta và yDelta là các biến int (vận tốc) đã được khai báo và khởi tạo
            xBall += xDelta;
            yBall += yDelta;

            // Kiểm tra va chạm ngang (biên phải và biên trái)
            // this.ClientSize.Width: Chiều rộng khu vực client của Form
            // pb.Width: Chiều rộng của PictureBox (bóng)
            if (xBall > this.ClientSize.Width - pb.Width || xBall <= 0)
            {
                xDelta = -xDelta; // Đảo ngược hướng di chuyển ngang
            }

            // Kiểm tra va chạm dọc (biên dưới và biên trên)
            // this.ClientSize.Height: Chiều cao khu vực client của Form
            // pb.Height: Chiều cao của PictureBox (bóng)
            if (yBall > this.ClientSize.Height - pb.Height || yBall <= 0)
            {
                yDelta = -yDelta; // Đảo ngược hướng di chuyển dọc
            }

            // Cập nhật vị trí mới cho PictureBox
            pb.Location = new Point(xBall, yBall);
        }
    }
}
