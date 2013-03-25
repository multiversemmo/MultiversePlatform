/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

namespace NormalBump
{
    partial class Form1
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
            this.normalMapOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.bumpMapOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.normalMapOpenButton = new System.Windows.Forms.Button();
            this.bumpMapOpenButton = new System.Windows.Forms.Button();
            this.normalMapTextBox = new System.Windows.Forms.TextBox();
            this.bumpMapTextBox = new System.Windows.Forms.TextBox();
            this.outputMapTextBox = new System.Windows.Forms.TextBox();
            this.outputMapSaveButton = new System.Windows.Forms.Button();
            this.generateButton = new System.Windows.Forms.Button();
            this.outputMapSaveDialog = new System.Windows.Forms.OpenFileDialog();
            this.trackBar = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.scaleFactorLabel = new System.Windows.Forms.Label();
            this.reverseBumpDirection = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.generateLogFile = new System.Windows.Forms.CheckBox();
            this.doneLabel = new System.Windows.Forms.Label();
            this.outputPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // normalMapOpenDialog
            // 
            this.normalMapOpenDialog.DefaultExt = "bmp";
            this.normalMapOpenDialog.Filter = "Bitmap files (*.bmp)|*.bmp|All files (*.*)|*.*";
            this.normalMapOpenDialog.ReadOnlyChecked = true;
            this.normalMapOpenDialog.ShowReadOnly = true;
            // 
            // bumpMapOpenDialog
            // 
            this.bumpMapOpenDialog.DefaultExt = "bmp";
            this.bumpMapOpenDialog.Filter = "Bitmap files (*.bmp)|*.bmp|All files (*.*)|*.*";
            this.bumpMapOpenDialog.ReadOnlyChecked = true;
            this.bumpMapOpenDialog.ShowReadOnly = true;
            // 
            // normalMapOpenButton
            // 
            this.normalMapOpenButton.Location = new System.Drawing.Point(41, 18);
            this.normalMapOpenButton.Name = "normalMapOpenButton";
            this.normalMapOpenButton.Size = new System.Drawing.Size(115, 23);
            this.normalMapOpenButton.TabIndex = 0;
            this.normalMapOpenButton.Text = "Open Normal Map";
            this.normalMapOpenButton.UseVisualStyleBackColor = true;
            this.normalMapOpenButton.Click += new System.EventHandler(this.normalMapOpenButton_Click);
            // 
            // bumpMapOpenButton
            // 
            this.bumpMapOpenButton.Location = new System.Drawing.Point(41, 49);
            this.bumpMapOpenButton.Name = "bumpMapOpenButton";
            this.bumpMapOpenButton.Size = new System.Drawing.Size(115, 23);
            this.bumpMapOpenButton.TabIndex = 1;
            this.bumpMapOpenButton.Text = "Open Bump  Map";
            this.bumpMapOpenButton.UseVisualStyleBackColor = true;
            this.bumpMapOpenButton.Click += new System.EventHandler(this.bumpMapOpenButton_Click);
            // 
            // normalMapTextBox
            // 
            this.normalMapTextBox.Location = new System.Drawing.Point(169, 18);
            this.normalMapTextBox.Name = "normalMapTextBox";
            this.normalMapTextBox.Size = new System.Drawing.Size(318, 20);
            this.normalMapTextBox.TabIndex = 2;
            this.normalMapTextBox.TextChanged += new System.EventHandler(this.normalMapTextBox_TextChanged);
            // 
            // bumpMapTextBox
            // 
            this.bumpMapTextBox.Location = new System.Drawing.Point(169, 52);
            this.bumpMapTextBox.Name = "bumpMapTextBox";
            this.bumpMapTextBox.Size = new System.Drawing.Size(318, 20);
            this.bumpMapTextBox.TabIndex = 3;
            this.bumpMapTextBox.TextChanged += new System.EventHandler(this.bumpMapTextBox_TextChanged);
            // 
            // outputMapTextBox
            // 
            this.outputMapTextBox.Location = new System.Drawing.Point(169, 84);
            this.outputMapTextBox.Name = "outputMapTextBox";
            this.outputMapTextBox.Size = new System.Drawing.Size(318, 20);
            this.outputMapTextBox.TabIndex = 5;
            this.outputMapTextBox.TextChanged += new System.EventHandler(this.outputMapTextBox_TextChanged);
            // 
            // outputMapSaveButton
            // 
            this.outputMapSaveButton.Location = new System.Drawing.Point(41, 81);
            this.outputMapSaveButton.Name = "outputMapSaveButton";
            this.outputMapSaveButton.Size = new System.Drawing.Size(115, 23);
            this.outputMapSaveButton.TabIndex = 4;
            this.outputMapSaveButton.Text = "Output Map";
            this.outputMapSaveButton.UseVisualStyleBackColor = true;
            this.outputMapSaveButton.Click += new System.EventHandler(this.outputMapSaveButton_Click);
            // 
            // generateButton
            // 
            this.generateButton.Enabled = false;
            this.generateButton.Location = new System.Drawing.Point(325, 185);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(129, 23);
            this.generateButton.TabIndex = 6;
            this.generateButton.Text = "Generate Output Map";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // outputMapSaveDialog
            // 
            this.outputMapSaveDialog.CheckFileExists = false;
            this.outputMapSaveDialog.DefaultExt = "bmp";
            this.outputMapSaveDialog.Filter = "Bitmap files (*.bmp)|*.bmp|All files (*.*)|*.*";
            // 
            // trackBar
            // 
            this.trackBar.Location = new System.Drawing.Point(169, 116);
            this.trackBar.Maximum = 400;
            this.trackBar.Name = "trackBar";
            this.trackBar.Size = new System.Drawing.Size(285, 42);
            this.trackBar.TabIndex = 7;
            this.trackBar.TickFrequency = 20;
            this.trackBar.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trackBar.Value = 100;
            this.trackBar.Scroll += new System.EventHandler(this.trackBar_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(173, 157);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "0%";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(231, 157);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "100%";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(296, 157);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "200%";
            // 
            // scaleFactorLabel
            // 
            this.scaleFactorLabel.AutoSize = true;
            this.scaleFactorLabel.Location = new System.Drawing.Point(38, 133);
            this.scaleFactorLabel.Name = "scaleFactorLabel";
            this.scaleFactorLabel.Size = new System.Drawing.Size(129, 13);
            this.scaleFactorLabel.TabIndex = 11;
            this.scaleFactorLabel.Text = "Bump Scale Factor: 100%";
            // 
            // reverseBumpDirection
            // 
            this.reverseBumpDirection.AutoSize = true;
            this.reverseBumpDirection.Location = new System.Drawing.Point(25, 187);
            this.reverseBumpDirection.Name = "reverseBumpDirection";
            this.reverseBumpDirection.Size = new System.Drawing.Size(141, 17);
            this.reverseBumpDirection.TabIndex = 12;
            this.reverseBumpDirection.Text = "Reverse Bump Direction";
            this.reverseBumpDirection.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(361, 157);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "300%";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(427, 157);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "400%";
            // 
            // generateLogFile
            // 
            this.generateLogFile.AutoSize = true;
            this.generateLogFile.Location = new System.Drawing.Point(172, 187);
            this.generateLogFile.Name = "generateLogFile";
            this.generateLogFile.Size = new System.Drawing.Size(110, 17);
            this.generateLogFile.TabIndex = 15;
            this.generateLogFile.Text = "Generate Log File";
            this.generateLogFile.UseVisualStyleBackColor = true;
            // 
            // doneLabel
            // 
            this.doneLabel.AutoSize = true;
            this.doneLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.doneLabel.ForeColor = System.Drawing.Color.Green;
            this.doneLabel.Location = new System.Drawing.Point(460, 182);
            this.doneLabel.Name = "doneLabel";
            this.doneLabel.Size = new System.Drawing.Size(75, 26);
            this.doneLabel.TabIndex = 16;
            this.doneLabel.Text = "Done!";
            this.doneLabel.Visible = false;
            // 
            // outputPictureBox
            // 
            this.outputPictureBox.Location = new System.Drawing.Point(13, 223);
            this.outputPictureBox.Name = "outputPictureBox";
            this.outputPictureBox.Size = new System.Drawing.Size(512, 512);
            this.outputPictureBox.TabIndex = 17;
            this.outputPictureBox.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(537, 744);
            this.Controls.Add(this.outputPictureBox);
            this.Controls.Add(this.doneLabel);
            this.Controls.Add(this.generateLogFile);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.reverseBumpDirection);
            this.Controls.Add(this.scaleFactorLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.outputMapTextBox);
            this.Controls.Add(this.outputMapSaveButton);
            this.Controls.Add(this.bumpMapTextBox);
            this.Controls.Add(this.normalMapTextBox);
            this.Controls.Add(this.bumpMapOpenButton);
            this.Controls.Add(this.normalMapOpenButton);
            this.Controls.Add(this.trackBar);
            this.Name = "Form1";
            this.Text = "Normal Map & Bump Map Combiner";
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog normalMapOpenDialog;
        private System.Windows.Forms.OpenFileDialog bumpMapOpenDialog;
        private System.Windows.Forms.Button normalMapOpenButton;
        private System.Windows.Forms.Button bumpMapOpenButton;
        private System.Windows.Forms.TextBox normalMapTextBox;
        private System.Windows.Forms.TextBox bumpMapTextBox;
        private System.Windows.Forms.TextBox outputMapTextBox;
        private System.Windows.Forms.Button outputMapSaveButton;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.OpenFileDialog outputMapSaveDialog;
		private System.Windows.Forms.TrackBar trackBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label scaleFactorLabel;
        private System.Windows.Forms.CheckBox reverseBumpDirection;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox generateLogFile;
        private System.Windows.Forms.Label doneLabel;
        private System.Windows.Forms.PictureBox outputPictureBox;
	}
}

