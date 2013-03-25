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
using System.Collections;
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.RenderSystems.DirectX9 {

    public class DefaultForm : System.Windows.Forms.Form {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DefaultForm));

        public static IDictionary WndOverrides = new Hashtable();
        public static void AddWndOverride(int wm_msg, IWindowTarget target)
        {
            lock (WndOverrides)
            {
                if (!WndOverrides.Contains(wm_msg))
                {
                    WndOverrides.Add(wm_msg, target);
                }
            }
        }
        public static bool HasWndOverride(int wm_msg)
        {
            lock (WndOverrides)
            {
                return WndOverrides.Contains(wm_msg);
            }
        }
        public static void RemoveWndOverride(int wm_msg)
        {
            lock (WndOverrides)
            {
                if (WndOverrides.Contains(wm_msg))
                {
                    WndOverrides.Remove(wm_msg);
                }
            }
        }
        public static bool WndOverride(ref Message m)
        {
            lock (WndOverrides)
            {
                if (WndOverrides.Contains(m.Msg))
                {
                    // do we need to handle multiple overrides, or overrides
                    // that choose not to do anything?
                    IWindowTarget iw = (IWindowTarget)WndOverrides[m.Msg];
                    iw.OnMessage(ref m);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private RenderWindow renderWindow;
        // This is the picture box we will use if we are not full screen
        private AxiomRenderBox pictureBox;
        // This variable just keeps track of where we are rendering.
        private Control renderBox = null;

        public DefaultForm(bool useRenderBox, string initialLoadBitmap) {
            InitializeComponent();
            if (initialLoadBitmap != "")
                this.BackgroundImage = new System.Drawing.Bitmap(initialLoadBitmap);

            if (useRenderBox) {
                // In this case, we will render into the picture box
                // instead of into the top level form.  This is generally
                // the case if we are not full screen.
                renderBox = pictureBox;
            } else {
                // In this case, we are rendering into the top level form.
                // I want to hide the pictureBox, since I don't want the
                // events to get intercepted.  I could always just render 
                // into the top level form, but this would cause us to use
                // the wrong cursor for the top bar (move) and the edges 
                // (resize)
                pictureBox.Hide();
            }

            this.Deactivate += new System.EventHandler(this.DefaultForm_Deactivate);
            this.Activated += new System.EventHandler(this.DefaultForm_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.DefaultForm_Close);
            this.Resize += new System.EventHandler(this.DefaultForm_Resize);
        }

        protected override void WndProc(ref Message m) 
        {
            if (!WndOverride(ref m))
            {
                log.DebugFormat("Pre Message m = {0}", m);
                switch (m.Msg)
                {
                    case 0x00000005: // WM_SIZE
                        if (renderWindow != null && renderWindow.IsFullScreen) {
                            log.InfoFormat("Ignoring size message, since we are full screen");
                            break;
                        }
                        base.WndProc(ref m);
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
                log.DebugFormat("Post Message m = {0}", m);
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public void DefaultForm_Deactivate(object source, System.EventArgs e) {
			if (renderWindow != null) {
				renderWindow.IsActive = false;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void DefaultForm_Activated(object source, System.EventArgs e) {
            if (renderWindow != null) {
                renderWindow.IsActive = true;
            }
        }

		private void InitializeComponent() {
            this.pictureBox = new Axiom.RenderSystems.DirectX9.AxiomRenderBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(640, 480);
            this.pictureBox.TabIndex = 1;
            this.pictureBox.TabStop = false;
            this.pictureBox.UseWaitCursor = true;
            this.pictureBox.Visible = false;
            // 
            // DefaultForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.Controls.Add(this.pictureBox);
            this.Name = "DefaultForm";
            this.UseWaitCursor = true;
            this.Load += new System.EventHandler(this.DefaultForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);

		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public void DefaultForm_Close(object source, System.ComponentModel.CancelEventArgs e) {
			// set the window to inactive
			renderWindow.IsActive = false;
	
			// remove it from the list of render windows, which will halt the rendering loop
			// since there should now be 0 windows left
			Root.Instance.RenderSystem.DetachRenderTarget(renderWindow);
		}
	
		private void DefaultForm_Load(object sender, System.EventArgs e) {
			// this.Icon = new System.Drawing.Icon(Axiom.Core.ResourceManager.FindCommonResourceData("AxiomIcon.ico"));
		}

        // Not currently used, but there should be some code to trap resize events
        // when I am fullscreen.
        private void DefaultForm_Resizing(object sender, System.ComponentModel.CancelEventArgs ce) {
            object device = renderWindow.GetCustomAttribute("D3DDEVICE");
            if (device != null) {
                Microsoft.DirectX.Direct3D.Device d3dDevice = device as Microsoft.DirectX.Direct3D.Device;
                if (d3dDevice.PresentationParameters.Windowed == false)
                    ce.Cancel = true;
                log.InfoFormat("Canceled resize event for full screen window");
                return;
            }
        }

		private void DefaultForm_Resize(object sender, System.EventArgs e) {
			Root.Instance.SuspendRendering = this.WindowState == FormWindowState.Minimized;
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

        public Control PictureBox {
			get { 
				return pictureBox; 
			}
        }
        
        public Control Target {
            get {
                if (renderBox != null)
                    return renderBox;
                return this;
            }
        }
	}
}