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
    partial class AddObjectDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.objectTypeComboBox = new System.Windows.Forms.ComboBox();
            this.objectComboBox = new System.Windows.Forms.ComboBox();
            this.randomRotationCheckBox = new System.Windows.Forms.CheckBox();
            this.randomScaleCheckBox = new System.Windows.Forms.CheckBox();
            this.maxScaleTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.createButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.objectNameTextBox = new System.Windows.Forms.TextBox();
            this.helpButton = new System.Windows.Forms.Button();
            this.clickTextLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.scaleGroupBox = new System.Windows.Forms.GroupBox();
            this.minScaleTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.placeMultipleCheckBox = new System.Windows.Forms.CheckBox();
            this.scaleGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Category:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(281, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Object";
            // 
            // objectTypeComboBox
            // 
            this.objectTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.objectTypeComboBox.FormattingEnabled = true;
            this.objectTypeComboBox.Location = new System.Drawing.Point(74, 33);
            this.objectTypeComboBox.Name = "objectTypeComboBox";
            this.objectTypeComboBox.Size = new System.Drawing.Size(168, 21);
            this.objectTypeComboBox.Sorted = true;
            this.objectTypeComboBox.TabIndex = 1;
            this.objectTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.objectTypeComboBox_SelectedIndexChanged);
            // 
            // objectComboBox
            // 
            this.objectComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.objectComboBox.FormattingEnabled = true;
            this.objectComboBox.Location = new System.Drawing.Point(325, 33);
            this.objectComboBox.Name = "objectComboBox";
            this.objectComboBox.Size = new System.Drawing.Size(150, 21);
            this.objectComboBox.Sorted = true;
            this.objectComboBox.TabIndex = 2;
            // 
            // randomRotationCheckBox
            // 
            this.randomRotationCheckBox.AutoSize = true;
            this.randomRotationCheckBox.Location = new System.Drawing.Point(248, 130);
            this.randomRotationCheckBox.Name = "randomRotationCheckBox";
            this.randomRotationCheckBox.Size = new System.Drawing.Size(104, 17);
            this.randomRotationCheckBox.TabIndex = 7;
            this.randomRotationCheckBox.Text = "Random rotation";
            this.randomRotationCheckBox.UseVisualStyleBackColor = true;
            // 
            // randomScaleCheckBox
            // 
            this.randomScaleCheckBox.AutoSize = true;
            this.randomScaleCheckBox.Location = new System.Drawing.Point(5, 14);
            this.randomScaleCheckBox.Name = "randomScaleCheckBox";
            this.randomScaleCheckBox.Size = new System.Drawing.Size(94, 17);
            this.randomScaleCheckBox.TabIndex = 4;
            this.randomScaleCheckBox.Text = "Random scale";
            this.randomScaleCheckBox.UseVisualStyleBackColor = true;
            this.randomScaleCheckBox.CheckedChanged += new System.EventHandler(this.randomScaleCheckBox_CheckedChanged);
            // 
            // maxScaleTextBox
            // 
            this.maxScaleTextBox.Enabled = false;
            this.maxScaleTextBox.Location = new System.Drawing.Point(72, 65);
            this.maxScaleTextBox.Name = "maxScaleTextBox";
            this.maxScaleTextBox.Size = new System.Drawing.Size(60, 20);
            this.maxScaleTextBox.TabIndex = 6;
            this.maxScaleTextBox.Text = "1.2";
            this.maxScaleTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.floatVerifyevent);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Max. Scale:";
            // 
            // createButton
            // 
            this.createButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.createButton.Location = new System.Drawing.Point(57, 275);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(75, 23);
            this.createButton.TabIndex = 9;
            this.createButton.Text = "Add";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(363, 275);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 11;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(21, 89);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Name:";
            // 
            // objectNameTextBox
            // 
            this.objectNameTextBox.Location = new System.Drawing.Point(74, 86);
            this.objectNameTextBox.Name = "objectNameTextBox";
            this.objectNameTextBox.Size = new System.Drawing.Size(209, 20);
            this.objectNameTextBox.TabIndex = 3;
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(282, 275);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 10;
            this.helpButton.Tag = "Object";
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.onHelpButton_clicked);
            // 
            // clickTextLabel
            // 
            this.clickTextLabel.AutoSize = true;
            this.clickTextLabel.Location = new System.Drawing.Point(21, 231);
            this.clickTextLabel.Name = "clickTextLabel";
            this.clickTextLabel.Size = new System.Drawing.Size(455, 13);
            this.clickTextLabel.TabIndex = 15;
            this.clickTextLabel.Text = "Click Add to attach specified object to mouse pointer. Position mouse pointer in " +
                "world view, and";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(21, 244);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(134, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "click again to place object.";
            // 
            // scaleGroupBox
            // 
            this.scaleGroupBox.Controls.Add(this.label4);
            this.scaleGroupBox.Controls.Add(this.minScaleTextBox);
            this.scaleGroupBox.Controls.Add(this.label7);
            this.scaleGroupBox.Controls.Add(this.maxScaleTextBox);
            this.scaleGroupBox.Controls.Add(this.randomScaleCheckBox);
            this.scaleGroupBox.Location = new System.Drawing.Point(24, 116);
            this.scaleGroupBox.Name = "scaleGroupBox";
            this.scaleGroupBox.Size = new System.Drawing.Size(200, 100);
            this.scaleGroupBox.TabIndex = 4;
            this.scaleGroupBox.TabStop = false;
            // 
            // minScaleTextBox
            // 
            this.minScaleTextBox.Enabled = false;
            this.minScaleTextBox.Location = new System.Drawing.Point(72, 40);
            this.minScaleTextBox.Name = "minScaleTextBox";
            this.minScaleTextBox.Size = new System.Drawing.Size(60, 20);
            this.minScaleTextBox.TabIndex = 5;
            this.minScaleTextBox.Text = "0.8";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 43);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Min. Scale:";
            // 
            // placeMultipleCheckBox
            // 
            this.placeMultipleCheckBox.AutoSize = true;
            this.placeMultipleCheckBox.Location = new System.Drawing.Point(248, 153);
            this.placeMultipleCheckBox.Name = "placeMultipleCheckBox";
            this.placeMultipleCheckBox.Size = new System.Drawing.Size(157, 17);
            this.placeMultipleCheckBox.TabIndex = 8;
            this.placeMultipleCheckBox.Text = "Place Multiple of this Object";
            this.placeMultipleCheckBox.UseVisualStyleBackColor = true;
            // 
            // AddObjectDialog
            // 
            this.AcceptButton = this.createButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 315);
            this.Controls.Add(this.placeMultipleCheckBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.clickTextLabel);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.objectNameTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.randomRotationCheckBox);
            this.Controls.Add(this.objectComboBox);
            this.Controls.Add(this.objectTypeComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.scaleGroupBox);
            this.Name = "AddObjectDialog";
            this.ShowInTaskbar = false;
            this.Text = "Add Object";
            this.scaleGroupBox.ResumeLayout(false);
            this.scaleGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox objectTypeComboBox;
        private System.Windows.Forms.ComboBox objectComboBox;
        private System.Windows.Forms.CheckBox randomRotationCheckBox;
        private System.Windows.Forms.CheckBox randomScaleCheckBox;
        private System.Windows.Forms.TextBox maxScaleTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox objectNameTextBox;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Label clickTextLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox scaleGroupBox;
        private System.Windows.Forms.TextBox minScaleTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox placeMultipleCheckBox;
    }
}
