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

namespace MVImportTool
{
    partial class CommandControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && (components != null) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.ConvertCheckBox = new System.Windows.Forms.CheckBox();
            this.CommandFlagTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.InstallControlsPanel = new System.Windows.Forms.Panel();
            this.OverwritePolicyGroup = new System.Windows.Forms.GroupBox();
            this.AskRadioButton = new System.Windows.Forms.RadioButton();
            this.NeverRadioButton = new System.Windows.Forms.RadioButton();
            this.AlwaysRadioButton = new System.Windows.Forms.RadioButton();
            this.ConfirmInstallationCheckBox = new System.Windows.Forms.CheckBox();
            this.FilterGroupBox = new System.Windows.Forms.GroupBox();
            this.InstallTexturesCheckBox = new System.Windows.Forms.CheckBox();
            this.InstallPhysicsCheckBox = new System.Windows.Forms.CheckBox();
            this.InstallMeshCheckBox = new System.Windows.Forms.CheckBox();
            this.InstallMaterialCheckBox = new System.Windows.Forms.CheckBox();
            this.InstallSkeletonCheckBox = new System.Windows.Forms.CheckBox();
            this.InstallCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.InstallControlsPanel.SuspendLayout();
            this.OverwritePolicyGroup.SuspendLayout();
            this.FilterGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add( this.ConvertCheckBox );
            this.groupBox4.Controls.Add( this.CommandFlagTextBox );
            this.groupBox4.Controls.Add( this.label1 );
            this.groupBox4.Location = new System.Drawing.Point( 3, 3 );
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size( 427, 94 );
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Conversion";
            // 
            // ConvertCheckBox
            // 
            this.ConvertCheckBox.AutoSize = true;
            this.ConvertCheckBox.Checked = true;
            this.ConvertCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ConvertCheckBox.Location = new System.Drawing.Point( 7, 19 );
            this.ConvertCheckBox.Name = "ConvertCheckBox";
            this.ConvertCheckBox.Size = new System.Drawing.Size( 115, 17 );
            this.ConvertCheckBox.TabIndex = 2;
            this.ConvertCheckBox.Text = "Convert COLLADA";
            this.ConvertCheckBox.UseVisualStyleBackColor = true;
            this.ConvertCheckBox.CheckedChanged += new System.EventHandler( this.ConvertCheckBox_CheckedChanged );
            // 
            // CommandFlagTextBox
            // 
            this.CommandFlagTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandFlagTextBox.Location = new System.Drawing.Point( 7, 55 );
            this.CommandFlagTextBox.Name = "CommandFlagTextBox";
            this.CommandFlagTextBox.Size = new System.Drawing.Size( 414, 20 );
            this.CommandFlagTextBox.TabIndex = 0;
            this.CommandFlagTextBox.TextChanged += new System.EventHandler( this.CommandFlagTextBox_TextChanged );
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point( 6, 39 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 101, 13 );
            this.label1.TabIndex = 1;
            this.label1.Text = "Command-line Flags";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add( this.InstallControlsPanel );
            this.groupBox2.Controls.Add( this.InstallCheckBox );
            this.groupBox2.Location = new System.Drawing.Point( 3, 103 );
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size( 427, 168 );
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Copying";
            // 
            // InstallControlsPanel
            // 
            this.InstallControlsPanel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.InstallControlsPanel.Controls.Add( this.OverwritePolicyGroup );
            this.InstallControlsPanel.Controls.Add( this.ConfirmInstallationCheckBox );
            this.InstallControlsPanel.Controls.Add( this.FilterGroupBox );
            this.InstallControlsPanel.Location = new System.Drawing.Point( 9, 42 );
            this.InstallControlsPanel.Name = "InstallControlsPanel";
            this.InstallControlsPanel.Size = new System.Drawing.Size( 414, 122 );
            this.InstallControlsPanel.TabIndex = 4;
            // 
            // OverwritePolicyGroup
            // 
            this.OverwritePolicyGroup.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.OverwritePolicyGroup.Controls.Add( this.AskRadioButton );
            this.OverwritePolicyGroup.Controls.Add( this.NeverRadioButton );
            this.OverwritePolicyGroup.Controls.Add( this.AlwaysRadioButton );
            this.OverwritePolicyGroup.Location = new System.Drawing.Point( 3, 71 );
            this.OverwritePolicyGroup.Name = "OverwritePolicyGroup";
            this.OverwritePolicyGroup.Size = new System.Drawing.Size( 408, 40 );
            this.OverwritePolicyGroup.TabIndex = 5;
            this.OverwritePolicyGroup.TabStop = false;
            this.OverwritePolicyGroup.Text = "Overwrite existing files?";
            // 
            // AskRadioButton
            // 
            this.AskRadioButton.AutoSize = true;
            this.AskRadioButton.Location = new System.Drawing.Point( 146, 17 );
            this.AskRadioButton.Name = "AskRadioButton";
            this.AskRadioButton.Size = new System.Drawing.Size( 43, 17 );
            this.AskRadioButton.TabIndex = 2;
            this.AskRadioButton.Text = "Ask";
            this.AskRadioButton.UseVisualStyleBackColor = true;
            this.AskRadioButton.CheckedChanged += new System.EventHandler( this.OverwritePolicyRadioButton_CheckedChanged );
            // 
            // NeverRadioButton
            // 
            this.NeverRadioButton.AutoSize = true;
            this.NeverRadioButton.Location = new System.Drawing.Point( 76, 17 );
            this.NeverRadioButton.Name = "NeverRadioButton";
            this.NeverRadioButton.Size = new System.Drawing.Size( 54, 17 );
            this.NeverRadioButton.TabIndex = 1;
            this.NeverRadioButton.Text = "Never";
            this.NeverRadioButton.UseVisualStyleBackColor = true;
            this.NeverRadioButton.CheckedChanged += new System.EventHandler( this.OverwritePolicyRadioButton_CheckedChanged );
            // 
            // AlwaysRadioButton
            // 
            this.AlwaysRadioButton.AutoSize = true;
            this.AlwaysRadioButton.Checked = true;
            this.AlwaysRadioButton.Location = new System.Drawing.Point( 6, 17 );
            this.AlwaysRadioButton.Name = "AlwaysRadioButton";
            this.AlwaysRadioButton.Size = new System.Drawing.Size( 58, 17 );
            this.AlwaysRadioButton.TabIndex = 0;
            this.AlwaysRadioButton.TabStop = true;
            this.AlwaysRadioButton.Text = "Always";
            this.AlwaysRadioButton.UseVisualStyleBackColor = true;
            this.AlwaysRadioButton.CheckedChanged += new System.EventHandler( this.OverwritePolicyRadioButton_CheckedChanged );
            // 
            // ConfirmInstallationCheckBox
            // 
            this.ConfirmInstallationCheckBox.AutoSize = true;
            this.ConfirmInstallationCheckBox.Checked = true;
            this.ConfirmInstallationCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ConfirmInstallationCheckBox.Location = new System.Drawing.Point( 9, 3 );
            this.ConfirmInstallationCheckBox.Name = "ConfirmInstallationCheckBox";
            this.ConfirmInstallationCheckBox.Size = new System.Drawing.Size( 134, 17 );
            this.ConfirmInstallationCheckBox.TabIndex = 4;
            this.ConfirmInstallationCheckBox.Text = "Confirm before copying";
            this.ConfirmInstallationCheckBox.UseVisualStyleBackColor = true;
            this.ConfirmInstallationCheckBox.CheckedChanged += new System.EventHandler( this.ConfirmInstallationCheckBox_CheckedChanged );
            // 
            // FilterGroupBox
            // 
            this.FilterGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterGroupBox.Controls.Add( this.InstallTexturesCheckBox );
            this.FilterGroupBox.Controls.Add( this.InstallPhysicsCheckBox );
            this.FilterGroupBox.Controls.Add( this.InstallMeshCheckBox );
            this.FilterGroupBox.Controls.Add( this.InstallMaterialCheckBox );
            this.FilterGroupBox.Controls.Add( this.InstallSkeletonCheckBox );
            this.FilterGroupBox.Location = new System.Drawing.Point( 3, 26 );
            this.FilterGroupBox.Name = "FilterGroupBox";
            this.FilterGroupBox.Size = new System.Drawing.Size( 408, 40 );
            this.FilterGroupBox.TabIndex = 3;
            this.FilterGroupBox.TabStop = false;
            this.FilterGroupBox.Text = "Check types to copy to Asset Repositories";
            // 
            // InstallTexturesCheckBox
            // 
            this.InstallTexturesCheckBox.AutoSize = true;
            this.InstallTexturesCheckBox.Checked = true;
            this.InstallTexturesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.InstallTexturesCheckBox.Location = new System.Drawing.Point( 286, 16 );
            this.InstallTexturesCheckBox.Name = "InstallTexturesCheckBox";
            this.InstallTexturesCheckBox.Size = new System.Drawing.Size( 67, 17 );
            this.InstallTexturesCheckBox.TabIndex = 8;
            this.InstallTexturesCheckBox.Text = "Textures";
            this.InstallTexturesCheckBox.UseVisualStyleBackColor = true;
            this.InstallTexturesCheckBox.CheckedChanged += new System.EventHandler( this.InstallFilterCheckBox_CheckedChanged );
            // 
            // InstallPhysicsCheckBox
            // 
            this.InstallPhysicsCheckBox.AutoSize = true;
            this.InstallPhysicsCheckBox.Checked = true;
            this.InstallPhysicsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.InstallPhysicsCheckBox.Location = new System.Drawing.Point( 216, 16 );
            this.InstallPhysicsCheckBox.Name = "InstallPhysicsCheckBox";
            this.InstallPhysicsCheckBox.Size = new System.Drawing.Size( 62, 17 );
            this.InstallPhysicsCheckBox.TabIndex = 7;
            this.InstallPhysicsCheckBox.Text = "Physics";
            this.InstallPhysicsCheckBox.UseVisualStyleBackColor = true;
            this.InstallPhysicsCheckBox.CheckedChanged += new System.EventHandler( this.InstallFilterCheckBox_CheckedChanged );
            // 
            // InstallMeshCheckBox
            // 
            this.InstallMeshCheckBox.AutoSize = true;
            this.InstallMeshCheckBox.Checked = true;
            this.InstallMeshCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.InstallMeshCheckBox.Location = new System.Drawing.Point( 6, 16 );
            this.InstallMeshCheckBox.Name = "InstallMeshCheckBox";
            this.InstallMeshCheckBox.Size = new System.Drawing.Size( 52, 17 );
            this.InstallMeshCheckBox.TabIndex = 4;
            this.InstallMeshCheckBox.Text = "Mesh";
            this.InstallMeshCheckBox.UseVisualStyleBackColor = true;
            this.InstallMeshCheckBox.CheckedChanged += new System.EventHandler( this.InstallFilterCheckBox_CheckedChanged );
            // 
            // InstallMaterialCheckBox
            // 
            this.InstallMaterialCheckBox.AutoSize = true;
            this.InstallMaterialCheckBox.Checked = true;
            this.InstallMaterialCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.InstallMaterialCheckBox.Location = new System.Drawing.Point( 76, 16 );
            this.InstallMaterialCheckBox.Name = "InstallMaterialCheckBox";
            this.InstallMaterialCheckBox.Size = new System.Drawing.Size( 63, 17 );
            this.InstallMaterialCheckBox.TabIndex = 5;
            this.InstallMaterialCheckBox.Text = "Material";
            this.InstallMaterialCheckBox.UseVisualStyleBackColor = true;
            this.InstallMaterialCheckBox.CheckedChanged += new System.EventHandler( this.InstallFilterCheckBox_CheckedChanged );
            // 
            // InstallSkeletonCheckBox
            // 
            this.InstallSkeletonCheckBox.AutoSize = true;
            this.InstallSkeletonCheckBox.Checked = true;
            this.InstallSkeletonCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.InstallSkeletonCheckBox.Location = new System.Drawing.Point( 146, 16 );
            this.InstallSkeletonCheckBox.Name = "InstallSkeletonCheckBox";
            this.InstallSkeletonCheckBox.Size = new System.Drawing.Size( 68, 17 );
            this.InstallSkeletonCheckBox.TabIndex = 6;
            this.InstallSkeletonCheckBox.Text = "Skeleton";
            this.InstallSkeletonCheckBox.UseVisualStyleBackColor = true;
            this.InstallSkeletonCheckBox.CheckedChanged += new System.EventHandler( this.InstallFilterCheckBox_CheckedChanged );
            // 
            // InstallCheckBox
            // 
            this.InstallCheckBox.AutoSize = true;
            this.InstallCheckBox.Checked = true;
            this.InstallCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.InstallCheckBox.Location = new System.Drawing.Point( 9, 19 );
            this.InstallCheckBox.Name = "InstallCheckBox";
            this.InstallCheckBox.Size = new System.Drawing.Size( 173, 17 );
            this.InstallCheckBox.TabIndex = 3;
            this.InstallCheckBox.Text = "Copy files to Asset Repositories";
            this.InstallCheckBox.UseVisualStyleBackColor = true;
            this.InstallCheckBox.CheckedChanged += new System.EventHandler( this.InstallCheckBox_CheckedChanged );
            // 
            // CommandControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.groupBox4 );
            this.Controls.Add( this.groupBox2 );
            this.MinimumSize = new System.Drawing.Size( 210, 250 );
            this.Name = "CommandControl";
            this.Size = new System.Drawing.Size( 433, 279 );
            this.Load += new System.EventHandler( this.CommandControl_Load );
            this.groupBox4.ResumeLayout( false );
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout( false );
            this.groupBox2.PerformLayout();
            this.InstallControlsPanel.ResumeLayout( false );
            this.InstallControlsPanel.PerformLayout();
            this.OverwritePolicyGroup.ResumeLayout( false );
            this.OverwritePolicyGroup.PerformLayout();
            this.FilterGroupBox.ResumeLayout( false );
            this.FilterGroupBox.PerformLayout();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox CommandFlagTextBox;
        private System.Windows.Forms.CheckBox InstallCheckBox;
        private System.Windows.Forms.CheckBox ConvertCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox InstallMeshCheckBox;
        private System.Windows.Forms.CheckBox InstallPhysicsCheckBox;
        private System.Windows.Forms.CheckBox InstallSkeletonCheckBox;
        private System.Windows.Forms.CheckBox InstallMaterialCheckBox;
        private System.Windows.Forms.GroupBox FilterGroupBox;
        private System.Windows.Forms.CheckBox InstallTexturesCheckBox;
        private System.Windows.Forms.Panel InstallControlsPanel;
        private System.Windows.Forms.GroupBox OverwritePolicyGroup;
        private System.Windows.Forms.CheckBox ConfirmInstallationCheckBox;
        private System.Windows.Forms.RadioButton AskRadioButton;
        private System.Windows.Forms.RadioButton NeverRadioButton;
        private System.Windows.Forms.RadioButton AlwaysRadioButton;
        private System.Windows.Forms.GroupBox groupBox4;
    }
}
