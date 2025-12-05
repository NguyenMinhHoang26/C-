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
    public partial class Form4 : Form
    {
        // Khai báo các đối tượng và biến cấp lớp
        PictureBox pbEgg = new PictureBox();
        System.Windows.Forms.Timer tmEgg = new System.Windows.Forms.Timer();

        int xEgg = 300; // Tọa độ X ban đầu
        int yEgg = 0;   // Tọa độ Y ban đầu
        int xDelta = 3; // Độ thay đổi X (hiện không dùng trong tmEgg_Tick)
        int yDelta = 3; // Tốc độ rơi (độ thay đổi Y)

        public Form4()
        {
            InitializeComponent();
            // Đảm bảo sự kiện Load Form được gán
            this.Load += Form4_Load;
        }

        // --- Phương thức Form Load (Thiết lập ban đầu) ---
        private void Form4_Load(object sender, EventArgs e)
        {
            // Thiết lập Timer
            tmEgg.Interval = 10;
            tmEgg.Tick += tmEgg_Tick;
            tmEgg.Start();

            // Thiết lập PictureBox
            pbEgg.SizeMode = PictureBoxSizeMode.StretchImage;
            pbEgg.Size = new Size(100, 100);
            pbEgg.Location = new Point(xEgg, yEgg);
            pbEgg.BackColor = Color.Transparent;

            // Thêm PictureBox vào Form
            this.Controls.Add(pbEgg);

            // Tải hình ảnh ban đầu
            // *Lưu ý: Bạn nên thêm đuôi file (.png, .jpg) vào đường dẫn!*
            // Giả định đường dẫn đầy đủ là thư mục "image" nằm cùng cấp với thư mục debug/release.
            try
            {
                // Sử dụng đường dẫn tương đối, đã thêm đuôi file .png giả định
                pbEgg.Image = Image.FromFile(@"image/bigboy.png");
            }
            catch (Exception)
            {
                // Xử lý lỗi nếu không tìm thấy file
            }
        }

        // --- Phương thức tmEgg_Tick (Logic chuyển động và va chạm) ---
        void tmEgg_Tick(object sender, EventArgs e)
        {
            // Cập nhật vị trí Y (di chuyển xuống)
            yEgg += yDelta;

            // Kiểm tra va chạm với ranh giới Form
            if (yEgg > this.ClientSize.Height - pbEgg.Height || yEgg <= 0)
            {
                // Xử lý va chạm: Đổi hình ảnh thành "boom"
                try
                {
                    // *Lưu ý: Bạn nên thêm đuôi file (.png, .jpg) vào đường dẫn!*
                    pbEgg.Image = Image.FromFile(@"image/boom.png");
                }
                catch (Exception)
                {
                    // Xử lý lỗi nếu không tìm thấy file
                }

                // *Gợi ý cải tiến:* Nếu muốn dừng lại khi va chạm:
                // tmEgg.Stop();

                // *Gợi ý cải tiến:* Nếu muốn nảy lên khi va chạm:
                // yDelta = -yDelta;
            }

            // Cập nhật vị trí hiển thị của PictureBox
            pbEgg.Location = new Point(xEgg, yEgg);
        }
    }
}
