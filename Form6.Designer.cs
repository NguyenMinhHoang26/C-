namespace NMHwin
{
    partial class Form6
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblPlayerCard;
        private System.Windows.Forms.Label lblComputerCard;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Button btnDraw;

        // Các thành phần khác như Dispose sẽ được giữ nguyên

        private void InitializeComponent()
        {
            this.lblPlayerCard = new System.Windows.Forms.Label();
            this.lblComputerCard = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.btnDraw = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // lblPlayerCard
            this.lblPlayerCard.AutoSize = true;
            this.lblPlayerCard.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblPlayerCard.Location = new System.Drawing.Point(50, 100);  // Tách xa hơn
            this.lblPlayerCard.Name = "lblPlayerCard";
            this.lblPlayerCard.Size = new System.Drawing.Size(300, 27);
            this.lblPlayerCard.TabIndex = 0;
            this.lblPlayerCard.Text = "Your cards:";

            // lblComputerCard
            this.lblComputerCard.AutoSize = true;
            this.lblComputerCard.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblComputerCard.Location = new System.Drawing.Point(50, 250);  // Tách xa hơn
            this.lblComputerCard.Name = "lblComputerCard";
            this.lblComputerCard.Size = new System.Drawing.Size(300, 27);
            this.lblComputerCard.TabIndex = 1;
            this.lblComputerCard.Text = "Computer's cards:";

            // lblResult
            this.lblResult.AutoSize = true;
            this.lblResult.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblResult.Location = new System.Drawing.Point(50, 400);  // Tách xa hơn
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(100, 27);
            this.lblResult.TabIndex = 2;
            this.lblResult.Text = "Result:";

            // btnDraw
            this.btnDraw.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnDraw.Location = new System.Drawing.Point(50, 50);  // Nơi đặt nút
            this.btnDraw.Name = "btnDraw";
            this.btnDraw.Size = new System.Drawing.Size(150, 40);
            this.btnDraw.TabIndex = 3;
            this.btnDraw.Text = "Draw Cards";
            this.btnDraw.UseVisualStyleBackColor = true;
            this.btnDraw.Click += new System.EventHandler(this.BtnDraw_Click);

            // Form6
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnDraw);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.lblComputerCard);
            this.Controls.Add(this.lblPlayerCard);
            this.Name = "Form6";
            this.Text = "Card Game";
            this.Load += new System.EventHandler(this.Form6_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }


    }
}
