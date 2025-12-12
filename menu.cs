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
    public partial class menu : Form
    {
        public menu()
        {
            InitializeComponent();

            // Gọi hàm tạo menu ngay sau khi khởi tạo các thành phần
            CreateMainMenu();
        }

        private void CreateMainMenu()
        {
            // 1. Tạo đối tượng Menu chính
            MenuStrip mainMenuStrip = new MenuStrip();

            // 2. Tạo các mục menu cấp 1 (top-level items)
            ToolStripMenuItem fileMenuItem = new ToolStripMenuItem("File");
            ToolStripMenuItem helpMenuItem = new ToolStripMenuItem("Help");

            // 3. Tạo các mục menu con cho "File"
            ToolStripMenuItem exitSubItem = new ToolStripMenuItem("Exit");

            // 4. Liên kết sự kiện Click cho mục "Exit"
            exitSubItem.Click += new EventHandler(ExitSubItem_Click);

            // 5. Thêm mục con vào mục cha "File"
            fileMenuItem.DropDownItems.Add(exitSubItem);

            // 6. Thêm các mục cấp 1 vào MenuStrip
            mainMenuStrip.Items.Add(fileMenuItem);
            mainMenuStrip.Items.Add(helpMenuItem);

            // 7. Gán MenuStrip vào Form
            this.Controls.Add(mainMenuStrip);
            this.MainMenuStrip = mainMenuStrip; // Thiết lập thanh menu chính
        }


        // 8. Định nghĩa hàm xử lý sự kiện Click cho Exit
        private void ExitSubItem_Click(object sender, EventArgs e)
        {
            this.Close(); // Đóng Form khi người dùng chọn Exit
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 newForm = new Form1();

            newForm.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 newForm = new Form2();

            newForm.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form3 newForm = new Form3();

            newForm.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form5 newForm = new Form5();

            newForm.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form4 newForm = new Form4();

            newForm.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form6 newForm = new Form6();

            newForm.Show();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form7 newForm = new Form7();

            newForm.Show();
        }
    }
}
