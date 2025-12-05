

namespace NMHwin
{
    partial class Form6
    {
        // Khai báo các Controls (Slide 77)
        private System.Windows.Forms.Button btMul;
        private System.Windows.Forms.Button btEquals;
        private System.Windows.Forms.Button btPlus;

        // Khai báo các nút số (chỉ ví dụ)
        private System.Windows.Forms.Button bt0;
        private System.Windows.Forms.Button bt1;
        private System.Windows.Forms.Button bt2;

        // Khai báo TextBox hiển thị kết quả
        private System.Windows.Forms.TextBox tbDisplay;

        /// <summary>
        /// Phương thức bắt buộc cho Designer.
        /// </summary>
        private void InitializeComponent()
        {
            // --- Khởi tạo và Thiết lập tbDisplay ---
            this.tbDisplay = new System.Windows.Forms.TextBox();
            this.tbDisplay.Location = new System.Drawing.Point(12, 12);
            this.tbDisplay.Name = "tbDisplay";
            this.tbDisplay.Size = new System.Drawing.Size(250, 20);
            this.tbDisplay.TabIndex = 0;
            this.tbDisplay.Text = "0";

            // --- Khởi tạo btPlus ---
            this.btPlus = new System.Windows.Forms.Button();
            this.btPlus.Location = new System.Drawing.Point(12, 50);
            this.btPlus.Name = "btPlus";
            this.btPlus.Size = new System.Drawing.Size(50, 50);
            this.btPlus.Text = "+";
            this.btPlus.TabIndex = 1;

            // --- Khởi tạo btEquals ---
            this.btEquals = new System.Windows.Forms.Button();
            this.btEquals.Location = new System.Drawing.Point(184, 162);
            this.btEquals.Name = "btEquals";
            this.btEquals.Size = new System.Drawing.Size(50, 50);
            this.btEquals.Text = "=";
            this.btEquals.TabIndex = 5;

            // --- Khởi tạo bt1 ---
            this.bt1 = new System.Windows.Forms.Button();
            this.bt1.Location = new System.Drawing.Point(68, 106);
            this.bt1.Name = "bt1";
            this.bt1.Size = new System.Drawing.Size(50, 50);
            this.bt1.Text = "1";
            this.bt1.TabIndex = 3;
            // Gán sự kiện cho nút 1 (theo Slide 77)
            this.bt1.Click += new System.EventHandler(this.bt1_Click);

            // (Thêm các khởi tạo khác cho bt0, bt2, btMul, v.v...)

            // --- Thiết lập Form ---
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);

            // Thêm Controls vào Form
            this.Controls.Add(this.btEquals);
            this.Controls.Add(this.btPlus);
            this.Controls.Add(this.tbDisplay);
            this.Controls.Add(this.bt1);
            // ... (Thêm các Controls khác)

            this.Name = "Form1";
            this.Text = "Simple Calculator";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void bt1_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}