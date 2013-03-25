#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Windows.Forms;
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.RenderSystems.OpenGL {
	
	public class DefaultForm : System.Windows.Forms.Form {
		private System.Windows.Forms.PictureBox pictureBox1;
		private RenderWindow renderWindow;		
	
		public DefaultForm() {
			InitializeComponent();
	
			this.Deactivate += new System.EventHandler(this.DefaultForm_Deactivate);
			this.Activated += new System.EventHandler(this.DefaultForm_Activated);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DefaultForm_Close);
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public void DefaultForm_Deactivate(object source, System.EventArgs e) {
			if(renderWindow != null) {
				renderWindow.IsActive = false;
			}
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public void DefaultForm_Activated(object source, System.EventArgs e) {
			if(renderWindow != null) {
				renderWindow.IsActive = true;
			}
		}
	
		private void InitializeComponent() {
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.BackColor = System.Drawing.Color.Black;
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(292, 266);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// DefaultForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.pictureBox1);
			this.Name = "DefaultForm";
			this.Load += new System.EventHandler(this.DefaultForm_Load);
			this.ResumeLayout(false);
	
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public void DefaultForm_Close(object source, System.ComponentModel.CancelEventArgs e) {
			// set the window to inactive
			//window.IsActive = false;
	
			// remove it from the list of render windows, which will halt the rendering loop
			// since there should now be 0 windows left
			//Root.Instance.RenderSystem.RenderWindows.Remove(renderWindow);
		}
	
		private void DefaultForm_Load(object sender, System.EventArgs e) {
			this.Icon = new System.Drawing.Icon(Axiom.Core.ResourceManager.FindCommonResourceData("AxiomIcon.ico"));
		}
	
		/// <summary>
		///		Get/Set the RenderWindow associated with this form.
		/// </summary>
		public RenderWindow RenderWindow {
			get { 
				return renderWindow; 
			}
			set {
				renderWindow = value; 
			}
		}
	
		/// <summary>
		///		
		/// </summary>
		public PictureBox Target {
			get { 
				return pictureBox1; 
			}
		}
	}
}