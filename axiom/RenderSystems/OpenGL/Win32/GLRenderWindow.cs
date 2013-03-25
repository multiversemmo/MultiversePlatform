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
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Media;
using Tao.OpenGl;
using Tao.Platform.Windows;

namespace Axiom.RenderSystems.OpenGL {
    /// <summary>
    /// Summary description for GLWindow.
    /// </summary>
    public class Win32Window : RenderWindow {
		#region Fields

		/// <summary>Window handle.</summary>
		private static IntPtr hWnd = IntPtr.Zero;
		/// <summary>GDI Device Context</summary>
		private IntPtr hDC = IntPtr.Zero;
		/// <summary>Rendering context.</summary>
		private IntPtr hRC = IntPtr.Zero;
		/// <summary>Retains initial screen settings.</summary>        
		private Gdi.DEVMODE intialScreenSettings;

		#endregion Fields

        #region Constructor

        /// <summary>
        ///		Constructor.
		/// </summary>
        public Win32Window() : base() { }

        #endregion Constructor

        #region Implementation of RenderWindow

        public override void Create(string name, int width, int height, int colorDepth, bool isFullScreen, int left, int top, bool depthBuffer, params object[] miscParams) {
			// see if a OpenGLContext has been created yet
			if(hDC == IntPtr.Zero) {
				// grab the current display settings
				User.EnumDisplaySettings(null, User.ENUM_CURRENT_SETTINGS, out intialScreenSettings);

				if(isFullScreen) {
					Gdi.DEVMODE screenSettings = new Gdi.DEVMODE();
					screenSettings.dmSize = (short)Marshal.SizeOf(screenSettings);
					screenSettings.dmPelsWidth = width;                         // Selected Screen Width
					screenSettings.dmPelsHeight = height;                       // Selected Screen Height
					screenSettings.dmBitsPerPel = colorDepth;                         // Selected Bits Per Pixel
					screenSettings.dmFields = Gdi.DM_BITSPERPEL | Gdi.DM_PELSWIDTH | Gdi.DM_PELSHEIGHT;

					// Try To Set Selected Mode And Get Results.  NOTE: CDS_FULLSCREEN Gets Rid Of Start Bar.
					int result = User.ChangeDisplaySettings(ref screenSettings, User.CDS_FULLSCREEN);

					if(result != User.DISP_CHANGE_SUCCESSFUL) {
						throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Unable to change user display settings.");
					}
				}

				// grab the HWND from the supplied target control
				hWnd = (IntPtr)((Control)this.Handle).Handle;
   
				Gdi.PIXELFORMATDESCRIPTOR pfd = new Gdi.PIXELFORMATDESCRIPTOR();
				pfd.nSize = (short)Marshal.SizeOf(pfd);
				pfd.nVersion = 1;
				pfd.dwFlags = Gdi.PFD_DRAW_TO_WINDOW |
					Gdi.PFD_SUPPORT_OPENGL |
					Gdi.PFD_DOUBLEBUFFER;
				pfd.iPixelType = (byte) Gdi.PFD_TYPE_RGBA;
				pfd.cColorBits = (byte) colorDepth;
				pfd.cDepthBits = 32;
				// TODO: Find the best setting and use that
				pfd.cStencilBits = 8;
				pfd.iLayerType = (byte) Gdi.PFD_MAIN_PLANE;

				// get the device context
				hDC = User.GetDC(hWnd);

				if(hDC == IntPtr.Zero) {
					throw new Exception("Cannot create a GL device context.");
				}

				// attempt to find an appropriate pixel format
				int pixelFormat = Gdi.ChoosePixelFormat(hDC, ref pfd);

				if(pixelFormat == 0) {
					throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Unable to find a suitable pixel format.");
				}

				if(!Gdi.SetPixelFormat(hDC, pixelFormat, ref pfd)) {
					throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Unable to set the pixel format.");
				}

				// attempt to get the rendering context
				hRC = Wgl.wglCreateContext(hDC);

				if(hRC == IntPtr.Zero) {
					throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Unable to create a GL rendering context.");
				}

				if(!Wgl.wglMakeCurrent(hDC, hRC)) {
					throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Unable to activate the GL rendering context.");
				}

				// init the GL context
				Gl.glShadeModel(Gl.GL_SMOOTH);							// Enable Smooth Shading
				Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.5f);				// Black Background
				Gl.glClearDepth(1.0f);									// Depth Buffer Setup
				Gl.glEnable(Gl.GL_DEPTH_TEST);							// Enables Depth Testing
				Gl.glDepthFunc(Gl.GL_LEQUAL);								// The Type Of Depth Testing To Do
				Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);	// Really Nice Perspective Calculations
			}

            // set the params of the window
            // TODO: deal with depth buffer
            this.Name = name;
            this.colorDepth = colorDepth;
            this.width = width;
            this.height = height;
            this.isFullScreen = isFullScreen;
            this.top = top;
            this.left = left;

            // make this window active
            this.isActive = true;
        }

        public override void Dispose() {
            base.Dispose();

            if(hRC != IntPtr.Zero) {                                        // Do We Not Have A Rendering Context?
                if(!Wgl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero)) {         // Are We Able To Release The DC And RC Contexts?
                    MessageBox.Show("Release Of DC And RC Failed.", "SHUTDOWN ERROR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if(!Wgl.wglDeleteContext(hRC)) {                            // Are We Not Able To Delete The RC?
                    MessageBox.Show("Release Rendering Context Failed.", "SHUTDOWN ERROR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                hRC = IntPtr.Zero;                                          // Set RC To NULL
            }

//            if(hDC != IntPtr.Zero && !User.ReleaseDC(control.Handle, hDC)) {          // Are We Not Able To Release The DC
//                MessageBox.Show("Release Device Context Failed.", "SHUTDOWN ERROR", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                hDC = IntPtr.Zero;                                          // Set DC To NULL
//            }

//            // if the control is a form, then close it
//            if(control is System.Windows.Forms.Form) {
//                form = control as System.Windows.Forms.Form;
//                form.Close();
//            }
//            else {
//                if(control.Parent != null) {
//                    form = (Form)control.Parent;
//                    form.Close();
//                }
//            }

            //form.Dispose();

            // make sure this window is no longer active
            this.isActive = false;
        }

        public override void Reposition(int left, int right) {

        }

        public override void Resize(int width, int height) {

        }

        public override void SwapBuffers(bool waitForVSync) {
            //int sync = waitForVSync ? 1: 0;
            //Gl.wglSwapIntervalEXT((uint)sync);

             // swap buffers
            Gdi.SwapBuffersFast(hDC);
        }

        public override bool IsActive {
            get { 
				return isActive; 
			}
            set { 
				isActive = value; 
			}
        }

        /// <summary>
        ///		Saves RenderWindow contents to a stream.
        /// </summary>
        /// <param name="stream">Target stream to save the window contents to.</param>
        public override void Save(Stream stream) {
            // create a RGB buffer
            byte[] buffer = new byte[width * height * 3];

			// read the pixels from the GL buffer
            Gl.glReadPixels(0, 0, width - 1, height - 1, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, buffer);

			stream.Write(buffer, 0, buffer.Length);
        }

        #endregion
    }
}
