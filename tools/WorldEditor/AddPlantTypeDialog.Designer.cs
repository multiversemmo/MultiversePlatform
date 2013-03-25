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

namespace Multiverse.Tools.WorldEditor
{
	partial class AddPlantTypeDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		// private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.imageNameComboBox = new System.Windows.Forms.ComboBox();
            this.PlantComboBoxLabel = new System.Windows.Forms.Label();
            this.minimumWidthScaleTextBox = new System.Windows.Forms.TextBox();
            this.minimumWidthScaleLabel = new System.Windows.Forms.Label();
            this.maximumWidthScaleLabel = new System.Windows.Forms.Label();
            this.maximumWidthScaleTextBox = new System.Windows.Forms.TextBox();
            this.maximumHeightScaleLabel = new System.Windows.Forms.Label();
            this.maximumHeightScaleTextBox = new System.Windows.Forms.TextBox();
            this.minimumHeightScaleLabel = new System.Windows.Forms.Label();
            this.minimumHeightScaleTextBox = new System.Windows.Forms.TextBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.windMagnatudeLabel = new System.Windows.Forms.Label();
            this.windMagnitudeTextBox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.InstancesLabel = new System.Windows.Forms.Label();
            this.instancesTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.colorMultHiLabel = new System.Windows.Forms.Label();
            this.colorMultHiTextBox = new System.Windows.Forms.TextBox();
            this.colorMultLowLabel = new System.Windows.Forms.Label();
            this.colorMultLowTextBox = new System.Windows.Forms.TextBox();
            this.colorSelectLabel = new System.Windows.Forms.Label();
            this.colorSelectButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageNameComboBox
            // 
            this.imageNameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imageNameComboBox.FormattingEnabled = true;
            this.imageNameComboBox.Location = new System.Drawing.Point(86, 12);
            this.imageNameComboBox.Name = "imageNameComboBox";
            this.imageNameComboBox.Size = new System.Drawing.Size(188, 21);
            this.imageNameComboBox.TabIndex = 1;
            // 
            // PlantComboBoxLabel
            // 
            this.PlantComboBoxLabel.AutoSize = true;
            this.PlantComboBoxLabel.Location = new System.Drawing.Point(22, 15);
            this.PlantComboBoxLabel.Name = "PlantComboBoxLabel";
            this.PlantComboBoxLabel.Size = new System.Drawing.Size(57, 13);
            this.PlantComboBoxLabel.TabIndex = 0;
            this.PlantComboBoxLabel.Text = "Plant type:";
            // 
            // minimumWidthScaleTextBox
            // 
            this.minimumWidthScaleTextBox.Location = new System.Drawing.Point(131, 13);
            this.minimumWidthScaleTextBox.Name = "minimumWidthScaleTextBox";
            this.minimumWidthScaleTextBox.Size = new System.Drawing.Size(131, 20);
            this.minimumWidthScaleTextBox.TabIndex = 3;
            this.minimumWidthScaleTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // minimumWidthScaleLabel
            // 
            this.minimumWidthScaleLabel.AutoSize = true;
            this.minimumWidthScaleLabel.Location = new System.Drawing.Point(16, 16);
            this.minimumWidthScaleLabel.Name = "minimumWidthScaleLabel";
            this.minimumWidthScaleLabel.Size = new System.Drawing.Size(51, 13);
            this.minimumWidthScaleLabel.TabIndex = 2;
            this.minimumWidthScaleLabel.Text = "Minimum:";
            // 
            // maximumWidthScaleLabel
            // 
            this.maximumWidthScaleLabel.AutoSize = true;
            this.maximumWidthScaleLabel.Location = new System.Drawing.Point(16, 42);
            this.maximumWidthScaleLabel.Name = "maximumWidthScaleLabel";
            this.maximumWidthScaleLabel.Size = new System.Drawing.Size(54, 13);
            this.maximumWidthScaleLabel.TabIndex = 4;
            this.maximumWidthScaleLabel.Text = "Maximum:";
            // 
            // maximumWidthScaleTextBox
            // 
            this.maximumWidthScaleTextBox.Location = new System.Drawing.Point(131, 39);
            this.maximumWidthScaleTextBox.Name = "maximumWidthScaleTextBox";
            this.maximumWidthScaleTextBox.Size = new System.Drawing.Size(131, 20);
            this.maximumWidthScaleTextBox.TabIndex = 5;
            this.maximumWidthScaleTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // maximumHeightScaleLabel
            // 
            this.maximumHeightScaleLabel.AutoSize = true;
            this.maximumHeightScaleLabel.Location = new System.Drawing.Point(16, 42);
            this.maximumHeightScaleLabel.Name = "maximumHeightScaleLabel";
            this.maximumHeightScaleLabel.Size = new System.Drawing.Size(54, 13);
            this.maximumHeightScaleLabel.TabIndex = 8;
            this.maximumHeightScaleLabel.Text = "Maximum:";
            // 
            // maximumHeightScaleTextBox
            // 
            this.maximumHeightScaleTextBox.Location = new System.Drawing.Point(131, 39);
            this.maximumHeightScaleTextBox.Name = "maximumHeightScaleTextBox";
            this.maximumHeightScaleTextBox.Size = new System.Drawing.Size(131, 20);
            this.maximumHeightScaleTextBox.TabIndex = 9;
            this.maximumHeightScaleTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // minimumHeightScaleLabel
            // 
            this.minimumHeightScaleLabel.AutoSize = true;
            this.minimumHeightScaleLabel.Location = new System.Drawing.Point(16, 16);
            this.minimumHeightScaleLabel.Name = "minimumHeightScaleLabel";
            this.minimumHeightScaleLabel.Size = new System.Drawing.Size(51, 13);
            this.minimumHeightScaleLabel.TabIndex = 6;
            this.minimumHeightScaleLabel.Text = "Minimum:";
            // 
            // minimumHeightScaleTextBox
            // 
            this.minimumHeightScaleTextBox.Location = new System.Drawing.Point(131, 13);
            this.minimumHeightScaleTextBox.Name = "minimumHeightScaleTextBox";
            this.minimumHeightScaleTextBox.Size = new System.Drawing.Size(131, 20);
            this.minimumHeightScaleTextBox.TabIndex = 7;
            this.minimumHeightScaleTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // colorDialog1
            // 
            this.colorDialog1.AnyColor = true;
            this.colorDialog1.FullOpen = true;
            this.colorDialog1.ShowHelp = true;
            // 
            // windMagnatudeLabel
            // 
            this.windMagnatudeLabel.AutoSize = true;
            this.windMagnatudeLabel.Location = new System.Drawing.Point(22, 354);
            this.windMagnatudeLabel.Name = "windMagnatudeLabel";
            this.windMagnatudeLabel.Size = new System.Drawing.Size(88, 13);
            this.windMagnatudeLabel.TabIndex = 18;
            this.windMagnatudeLabel.Text = "Wind Magnitude:";
            // 
            // windMagnitudeTextBox
            // 
            this.windMagnitudeTextBox.Location = new System.Drawing.Point(143, 351);
            this.windMagnitudeTextBox.Name = "windMagnitudeTextBox";
            this.windMagnitudeTextBox.Size = new System.Drawing.Size(131, 20);
            this.windMagnitudeTextBox.TabIndex = 19;
            this.windMagnitudeTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(25, 387);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 20;
            this.okButton.Text = "Add";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(197, 387);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 22;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(116, 387);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 21;
            this.helpButton.Tag = "Plant_Type";
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_clicked);
            // 
            // InstancesLabel
            // 
            this.InstancesLabel.AutoSize = true;
            this.InstancesLabel.Location = new System.Drawing.Point(22, 45);
            this.InstancesLabel.Name = "InstancesLabel";
            this.InstancesLabel.Size = new System.Drawing.Size(56, 13);
            this.InstancesLabel.TabIndex = 23;
            this.InstancesLabel.Text = "Instances:";
            // 
            // instancesTextBox
            // 
            this.instancesTextBox.Location = new System.Drawing.Point(143, 42);
            this.instancesTextBox.Name = "instancesTextBox";
            this.instancesTextBox.Size = new System.Drawing.Size(131, 20);
            this.instancesTextBox.TabIndex = 24;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.minimumWidthScaleLabel);
            this.groupBox1.Controls.Add(this.minimumWidthScaleTextBox);
            this.groupBox1.Controls.Add(this.maximumWidthScaleTextBox);
            this.groupBox1.Controls.Add(this.maximumWidthScaleLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 68);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(276, 68);
            this.groupBox1.TabIndex = 25;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Width scale (mm)";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.minimumHeightScaleLabel);
            this.groupBox2.Controls.Add(this.minimumHeightScaleTextBox);
            this.groupBox2.Controls.Add(this.maximumHeightScaleTextBox);
            this.groupBox2.Controls.Add(this.maximumHeightScaleLabel);
            this.groupBox2.Location = new System.Drawing.Point(12, 142);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(276, 68);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Height scale (mm)";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.colorMultHiLabel);
            this.groupBox3.Controls.Add(this.colorMultHiTextBox);
            this.groupBox3.Controls.Add(this.colorMultLowLabel);
            this.groupBox3.Controls.Add(this.colorMultLowTextBox);
            this.groupBox3.Controls.Add(this.colorSelectLabel);
            this.groupBox3.Controls.Add(this.colorSelectButton);
            this.groupBox3.Location = new System.Drawing.Point(12, 216);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(276, 122);
            this.groupBox3.TabIndex = 27;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Color";
            // 
            // colorMultHiLabel
            // 
            this.colorMultHiLabel.AutoSize = true;
            this.colorMultHiLabel.Location = new System.Drawing.Point(16, 96);
            this.colorMultHiLabel.Name = "colorMultHiLabel";
            this.colorMultHiLabel.Size = new System.Drawing.Size(100, 13);
            this.colorMultHiLabel.TabIndex = 20;
            this.colorMultHiLabel.Text = "Color multiplier high:";
            // 
            // colorMultHiTextBox
            // 
            this.colorMultHiTextBox.Location = new System.Drawing.Point(131, 93);
            this.colorMultHiTextBox.Name = "colorMultHiTextBox";
            this.colorMultHiTextBox.Size = new System.Drawing.Size(131, 20);
            this.colorMultHiTextBox.TabIndex = 21;
            // 
            // colorMultLowLabel
            // 
            this.colorMultLowLabel.AutoSize = true;
            this.colorMultLowLabel.Location = new System.Drawing.Point(16, 70);
            this.colorMultLowLabel.Name = "colorMultLowLabel";
            this.colorMultLowLabel.Size = new System.Drawing.Size(96, 13);
            this.colorMultLowLabel.TabIndex = 18;
            this.colorMultLowLabel.Text = "Color multiplier low:";
            // 
            // colorMultLowTextBox
            // 
            this.colorMultLowTextBox.Location = new System.Drawing.Point(131, 67);
            this.colorMultLowTextBox.Name = "colorMultLowTextBox";
            this.colorMultLowTextBox.Size = new System.Drawing.Size(131, 20);
            this.colorMultLowTextBox.TabIndex = 19;
            // 
            // colorSelectLabel
            // 
            this.colorSelectLabel.AutoSize = true;
            this.colorSelectLabel.Location = new System.Drawing.Point(16, 31);
            this.colorSelectLabel.Name = "colorSelectLabel";
            this.colorSelectLabel.Size = new System.Drawing.Size(109, 13);
            this.colorSelectLabel.TabIndex = 16;
            this.colorSelectLabel.Text = "Click to choose color:";
            // 
            // colorSelectButton
            // 
            this.colorSelectButton.Location = new System.Drawing.Point(131, 17);
            this.colorSelectButton.Name = "colorSelectButton";
            this.colorSelectButton.Size = new System.Drawing.Size(62, 41);
            this.colorSelectButton.TabIndex = 17;
            this.colorSelectButton.UseVisualStyleBackColor = true;
            this.colorSelectButton.Click += new System.EventHandler(this.colorSelectButton_Click);
            // 
            // AddPlantTypeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 421);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.InstancesLabel);
            this.Controls.Add(this.instancesTextBox);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.windMagnatudeLabel);
            this.Controls.Add(this.windMagnitudeTextBox);
            this.Controls.Add(this.PlantComboBoxLabel);
            this.Controls.Add(this.imageNameComboBox);
            this.Controls.Add(this.groupBox3);
            this.Name = "AddPlantTypeDialog";
            this.ShowInTaskbar = false;
            this.Text = "Add Plant Type";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox imageNameComboBox;
		private System.Windows.Forms.Label PlantComboBoxLabel;
		private System.Windows.Forms.TextBox minimumWidthScaleTextBox;
		private System.Windows.Forms.Label minimumWidthScaleLabel;
		private System.Windows.Forms.Label maximumWidthScaleLabel;
		private System.Windows.Forms.TextBox maximumWidthScaleTextBox;
		private System.Windows.Forms.Label maximumHeightScaleLabel;
		private System.Windows.Forms.TextBox maximumHeightScaleTextBox;
		private System.Windows.Forms.Label minimumHeightScaleLabel;
        private System.Windows.Forms.TextBox minimumHeightScaleTextBox;
		private System.Windows.Forms.Label windMagnatudeLabel;
        private System.Windows.Forms.TextBox windMagnitudeTextBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		public System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Label InstancesLabel;
        private System.Windows.Forms.TextBox instancesTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label colorMultHiLabel;
        private System.Windows.Forms.TextBox colorMultHiTextBox;
        private System.Windows.Forms.Label colorMultLowLabel;
        private System.Windows.Forms.TextBox colorMultLowTextBox;
        private System.Windows.Forms.Label colorSelectLabel;
        private System.Windows.Forms.Button colorSelectButton;
	}
}
