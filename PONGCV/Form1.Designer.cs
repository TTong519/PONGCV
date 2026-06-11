namespace PONGCV
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            imageBox1 = new Emgu.CV.UI.ImageBox();
            imageBox2 = new Emgu.CV.UI.ImageBox();
            ((System.ComponentModel.ISupportInitialize)imageBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)imageBox2).BeginInit();
            SuspendLayout();
            // 
            // imageBox1
            // 
            imageBox1.Location = new Point(1, -2);
            imageBox1.Name = "imageBox1";
            imageBox1.Size = new Size(800, 454);
            imageBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            imageBox1.TabIndex = 2;
            imageBox1.TabStop = false;
            imageBox1.Click += imageBox1_Click;
            // 
            // imageBox2
            // 
            imageBox2.Location = new Point(1, 294);
            imageBox2.Name = "imageBox2";
            imageBox2.Size = new Size(193, 158);
            imageBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            imageBox2.TabIndex = 2;
            imageBox2.TabStop = false;
            // 
            // comboPaddleColor
            // 
            comboPaddleColor = new ComboBox();
            comboPaddleColor.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPaddleColor.Items.AddRange(new object[] { "Blue", "Red", "Green", "Custom" });
            comboPaddleColor.SelectedIndex = 0;
            comboPaddleColor.Location = new Point(660, 10);
            comboPaddleColor.Name = "comboPaddleColor";
            comboPaddleColor.Size = new Size(120, 23);
            comboPaddleColor.TabIndex = 3;
            comboPaddleColor.SelectedIndexChanged += comboPaddleColor_SelectedIndexChanged;

            // 
            // chkSimulate
            // 
            chkSimulate = new CheckBox();
            chkSimulate.AutoSize = true;
            chkSimulate.Location = new Point(660, 40);
            chkSimulate.Name = "chkSimulate";
            chkSimulate.Size = new Size(120, 19);
            chkSimulate.TabIndex = 4;
            chkSimulate.Text = "Simulate Paddle";
            chkSimulate.UseVisualStyleBackColor = true;

            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(chkSimulate);
            Controls.Add(comboPaddleColor);
            Controls.Add(imageBox2);
            Controls.Add(imageBox1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)imageBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)imageBox2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Emgu.CV.UI.ImageBox imageBox1;
        private Emgu.CV.UI.ImageBox imageBox2;
        private ComboBox comboPaddleColor;
        private CheckBox chkSimulate;
    }
}
