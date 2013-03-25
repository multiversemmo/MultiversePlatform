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

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	partial class EventEditorUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.panelTools = new System.Windows.Forms.Panel();
            this.buttonEditScript = new System.Windows.Forms.Button();
            this.buttonCreateJump = new System.Windows.Forms.Button();
            this.textBoxScript = new System.Windows.Forms.TextBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTools
            // 
            this.panelTools.Controls.Add(this.buttonEditScript);
            this.panelTools.Controls.Add(this.buttonCreateJump);
            this.panelTools.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTools.Location = new System.Drawing.Point(0, 0);
            this.panelTools.Name = "panelTools";
            this.panelTools.Size = new System.Drawing.Size(230, 24);
            this.panelTools.TabIndex = 0;
            // 
            // buttonEditScript
            // 
            this.buttonEditScript.Location = new System.Drawing.Point(78, 0);
            this.buttonEditScript.Name = "buttonEditScript";
            this.buttonEditScript.Size = new System.Drawing.Size(75, 24);
            this.buttonEditScript.TabIndex = 1;
            this.buttonEditScript.Text = "Edit Script";
            this.buttonEditScript.UseVisualStyleBackColor = true;
            this.buttonEditScript.Click += new System.EventHandler(this.buttonEditScript_Click);
            // 
            // buttonCreateJump
            // 
            this.buttonCreateJump.Location = new System.Drawing.Point(0, 0);
            this.buttonCreateJump.Name = "buttonCreateJump";
            this.buttonCreateJump.Size = new System.Drawing.Size(78, 24);
            this.buttonCreateJump.TabIndex = 0;
            this.buttonCreateJump.Text = "Create/Jump";
            this.buttonCreateJump.UseVisualStyleBackColor = true;
            this.buttonCreateJump.Click += new System.EventHandler(this.buttonCreateJump_Click);
            // 
            // textBoxScript
            // 
            this.textBoxScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxScript.Location = new System.Drawing.Point(0, 24);
            this.textBoxScript.Multiline = true;
            this.textBoxScript.Name = "textBoxScript";
            this.textBoxScript.Size = new System.Drawing.Size(230, 64);
            this.textBoxScript.TabIndex = 1;
            this.textBoxScript.TextChanged += new System.EventHandler(this.textBoxScript_TextChanged);
            // 
            // EventEditorUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxScript);
            this.Controls.Add(this.panelTools);
            this.Name = "EventEditorUserControl";
            this.Size = new System.Drawing.Size(230, 88);
            this.panelTools.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel panelTools;
		private System.Windows.Forms.Button buttonCreateJump;
		private System.Windows.Forms.TextBox textBoxScript;
		private System.Windows.Forms.Button buttonEditScript;
        private System.Windows.Forms.ToolTip toolTip;
	}
}
