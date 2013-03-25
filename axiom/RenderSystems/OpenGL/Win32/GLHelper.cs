using System;
using System.Windows.Forms;
using Axiom.Configuration;
using Axiom.Graphics;
using Tao.Platform.Windows;

namespace Axiom.RenderSystems.OpenGL {
	/// <summary>
	/// Summary description for GLSupport.
	/// </summary>
	public class GLSupport : BaseGLSupport {

		public GLSupport() : base() {
		}

		/// <summary>
		///		Uses Wgl to return the procedure address for an extension function.
		/// </summary>
		/// <param name="extension"></param>
		/// <returns></returns>
		public override IntPtr GetProcAddress(string extension) {
			return Wgl.wglGetProcAddress(extension);
		}

		/// <summary>
		///		Query the display modes and deal with any other config options.
		/// </summary>
		public override void AddConfig() {
			Gdi.DEVMODE setting;
			int i = 0;
			int width, height, bpp, freq;
            
			bool more = User.EnumDisplaySettings(null, i++, out setting);

			while(more) {
				width = setting.dmPelsWidth;
				height = setting.dmPelsHeight;
				bpp = setting.dmBitsPerPel;
				freq = setting.dmDisplayFrequency;
			
				// filter out the lower resolutions and dupe frequencies
				if(width >= 640 && height >= 480 && bpp >= 16) {
					string query = string.Format("Width = {0} AND Height= {1} AND Bpp = {2}", width, height, bpp);

					if(engineConfig.DisplayMode.Select(query).Length == 0) {
						// add a new row to the display settings table
						engineConfig.DisplayMode.AddDisplayModeRow(width, height, bpp, false, false);
					}
				}

				// grab the current display settings
				more = User.EnumDisplaySettings(null, i++, out setting);
			}
		}

		public override Axiom.Graphics.RenderWindow CreateWindow(bool autoCreateWindow, GLRenderSystem renderSystem, string windowTitle) {
			RenderWindow autoWindow = null;

			if(autoCreateWindow) {
				EngineConfig.DisplayModeRow[] modes = 
					(EngineConfig.DisplayModeRow[])engineConfig.DisplayMode.Select("Selected = true");

				EngineConfig.DisplayModeRow mode = modes[0];

				int width = mode.Width;
				int height = mode.Height;
				int bpp = mode.Bpp;
				bool fullscreen = mode.FullScreen;

				// create a default form to use for a rendering target
				DefaultForm form = CreateDefaultForm(windowTitle, 0, 0, width, height, fullscreen);

				// create the window with the default form as the target
				autoWindow = renderSystem.CreateRenderWindow(windowTitle, width, height, bpp, fullscreen, 0, 0, true, false, form.Target);

				// set the default form's renderwindow so it can access it internally
				form.RenderWindow = autoWindow;

				// show the window
				form.Show();
			}

			return autoWindow;
		}

		public override Axiom.Graphics.RenderWindow NewWindow(string name, int width, int height, int colorDepth, bool fullScreen, int left, int top, bool depthBuffer, bool vsync, object target) {
			Win32Window window = new Win32Window();

			window.Handle = target;

			window.Create(name, width, height, colorDepth, fullScreen, left, top, depthBuffer, vsync);

			return window;
		}

		/// <summary>
		///		Creates a default form to use for a rendering target.
		/// </summary>
		/// <remarks>
		///		This is used internally whenever <see cref="Initialize"/> is called and autoCreateWindow is set to true.
		/// </remarks>
		/// <param name="windowTitle">Title of the window.</param>
		/// <param name="top">Top position of the window.</param>
		/// <param name="left">Left position of the window.</param>
		/// <param name="width">Width of the window.</param>
		/// <param name="height">Height of the window</param>
		/// <param name="fullScreen">Prepare the form for fullscreen mode?</param>
		/// <returns>A form suitable for using as a rendering target.</returns>
		private DefaultForm CreateDefaultForm(string windowTitle, int top, int left, int width, int height, bool fullScreen) {
			DefaultForm form = new DefaultForm();

			form.ClientSize = new System.Drawing.Size(width,height);
			form.MaximizeBox = false;
			form.MinimizeBox = false;
			form.StartPosition = FormStartPosition.CenterScreen;

			if(fullScreen) {
				form.Top = 0;
				form.Left = 0;
				form.FormBorderStyle = FormBorderStyle.None;
				form.WindowState = FormWindowState.Maximized;
				form.TopMost = true;
				form.TopLevel = true;
			}
			else {
				form.Top = top;
				form.Left = left;
				form.FormBorderStyle = FormBorderStyle.FixedSingle;
				form.WindowState = FormWindowState.Normal;
				form.Text = windowTitle;
			}

			return form;
		}
	}
}
