namespace CLIENT.View
{
    partial class frmDeviceSelection
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBoxCamPreview = new PictureBox();
            btnCamera = new Button();
            btnMicrophone = new Button();
            btnExit = new Button();
            cbCamera = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            cbOutput = new ComboBox();
            label3 = new Label();
            cbInput = new ComboBox();
            progessBarMic = new ProgressBar();
            btnOutputTest = new Button();
            label4 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxCamPreview).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxCamPreview
            // 
            pictureBoxCamPreview.Location = new Point(12, 12);
            pictureBoxCamPreview.Name = "pictureBoxCamPreview";
            pictureBoxCamPreview.Size = new Size(414, 279);
            pictureBoxCamPreview.TabIndex = 0;
            pictureBoxCamPreview.TabStop = false;
            // 
            // btnCamera
            // 
            btnCamera.Location = new Point(109, 297);
            btnCamera.Name = "btnCamera";
            btnCamera.Size = new Size(98, 44);
            btnCamera.TabIndex = 2;
            btnCamera.Text = "Mở cam";
            btnCamera.UseVisualStyleBackColor = true;
            // 
            // btnMicrophone
            // 
            btnMicrophone.Location = new Point(213, 297);
            btnMicrophone.Name = "btnMicrophone";
            btnMicrophone.Size = new Size(98, 44);
            btnMicrophone.TabIndex = 3;
            btnMicrophone.Text = "Mở mic";
            btnMicrophone.UseVisualStyleBackColor = true;
            // 
            // btnExit
            // 
            btnExit.Location = new Point(674, 394);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(98, 44);
            btnExit.TabIndex = 4;
            btnExit.Text = "Thoát";
            btnExit.UseVisualStyleBackColor = true;
            // 
            // cbCamera
            // 
            cbCamera.FormattingEnabled = true;
            cbCamera.Location = new Point(558, 51);
            cbCamera.Name = "cbCamera";
            cbCamera.Size = new Size(214, 28);
            cbCamera.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(481, 54);
            label1.Name = "label1";
            label1.Size = new Size(60, 20);
            label1.TabIndex = 6;
            label1.Text = "Camera";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(481, 96);
            label2.Name = "label2";
            label2.Size = new Size(55, 20);
            label2.TabIndex = 8;
            label2.Text = "Output";
            // 
            // cbOutput
            // 
            cbOutput.FormattingEnabled = true;
            cbOutput.Location = new Point(558, 93);
            cbOutput.Name = "cbOutput";
            cbOutput.Size = new Size(214, 28);
            cbOutput.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(481, 187);
            label3.Name = "label3";
            label3.Size = new Size(43, 20);
            label3.TabIndex = 10;
            label3.Text = "Input";
            // 
            // cbInput
            // 
            cbInput.FormattingEnabled = true;
            cbInput.Location = new Point(558, 184);
            cbInput.Name = "cbInput";
            cbInput.Size = new Size(214, 28);
            cbInput.TabIndex = 9;
            // 
            // progessBarMic
            // 
            progessBarMic.Location = new Point(558, 231);
            progessBarMic.Name = "progessBarMic";
            progessBarMic.Size = new Size(212, 10);
            progessBarMic.TabIndex = 11;
            // 
            // btnOutputTest
            // 
            btnOutputTest.Location = new Point(693, 127);
            btnOutputTest.Name = "btnOutputTest";
            btnOutputTest.Size = new Size(79, 40);
            btnOutputTest.TabIndex = 12;
            btnOutputTest.Text = "Thử loa";
            btnOutputTest.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(481, 221);
            label4.Name = "label4";
            label4.Size = new Size(62, 20);
            label4.TabIndex = 13;
            label4.Text = "Thử mic";
            // 
            // frmDeviceSelection
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 450);
            Controls.Add(label4);
            Controls.Add(btnOutputTest);
            Controls.Add(progessBarMic);
            Controls.Add(label3);
            Controls.Add(cbInput);
            Controls.Add(label2);
            Controls.Add(cbOutput);
            Controls.Add(label1);
            Controls.Add(cbCamera);
            Controls.Add(btnExit);
            Controls.Add(btnMicrophone);
            Controls.Add(btnCamera);
            Controls.Add(pictureBoxCamPreview);
            MaximizeBox = false;
            Name = "frmDeviceSelection";
            Text = "CLIENT - Preview";
            ((System.ComponentModel.ISupportInitialize)pictureBoxCamPreview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBoxCamPreview;
        private Button btnCamera;
        private Button btnMicrophone;
        private Button btnExit;
        private ComboBox cbCamera;
        private Label label1;
        private Label label2;
        private ComboBox cbOutput;
        private Label label3;
        private ComboBox cbInput;
        private ProgressBar progessBarMic;
        private Button btnOutputTest;
        private Label label4;
    }
}