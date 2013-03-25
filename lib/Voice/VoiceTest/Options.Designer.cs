namespace VoiceTest
{
    partial class Options
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DecoderOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.AGCGroupBox = new System.Windows.Forms.GroupBox();
            this.AGCCheckBox = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.AGCGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.AGCGroupBox);
            this.groupBox1.Location = new System.Drawing.Point(16, 19);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(648, 226);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Encoder Options";
            // 
            // DecoderOptionsGroupBox
            // 
            this.DecoderOptionsGroupBox.Location = new System.Drawing.Point(16, 263);
            this.DecoderOptionsGroupBox.Name = "DecoderOptionsGroupBox";
            this.DecoderOptionsGroupBox.Size = new System.Drawing.Size(648, 175);
            this.DecoderOptionsGroupBox.TabIndex = 1;
            this.DecoderOptionsGroupBox.TabStop = false;
            this.DecoderOptionsGroupBox.Text = "Decoder Options";
            // 
            // AGCGroupBox
            // 
            this.AGCGroupBox.Controls.Add(this.textBox1);
            this.AGCGroupBox.Controls.Add(this.AGCCheckBox);
            this.AGCGroupBox.Location = new System.Drawing.Point(18, 29);
            this.AGCGroupBox.Name = "AGCGroupBox";
            this.AGCGroupBox.Size = new System.Drawing.Size(245, 151);
            this.AGCGroupBox.TabIndex = 0;
            this.AGCGroupBox.TabStop = false;
            this.AGCGroupBox.Text = "AGC";
            // 
            // AGCCheckBox
            // 
            this.AGCCheckBox.AutoSize = true;
            this.AGCCheckBox.Location = new System.Drawing.Point(25, 29);
            this.AGCCheckBox.Name = "AGCCheckBox";
            this.AGCCheckBox.Size = new System.Drawing.Size(112, 21);
            this.AGCCheckBox.TabIndex = 0;
            this.AGCCheckBox.Text = "AGC Enabled";
            this.AGCCheckBox.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(88, 66);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 22);
            this.textBox1.TabIndex = 1;
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(677, 566);
            this.Controls.Add(this.DecoderOptionsGroupBox);
            this.Controls.Add(this.groupBox1);
            this.Name = "Options";
            this.Text = "Audio Options";
            this.groupBox1.ResumeLayout(false);
            this.AGCGroupBox.ResumeLayout(false);
            this.AGCGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox AGCGroupBox;
        private System.Windows.Forms.CheckBox AGCCheckBox;
        private System.Windows.Forms.GroupBox DecoderOptionsGroupBox;
        private System.Windows.Forms.TextBox textBox1;
    }
}