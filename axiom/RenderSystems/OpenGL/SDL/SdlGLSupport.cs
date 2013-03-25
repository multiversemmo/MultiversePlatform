using System;
using System.Data;
using Axiom.Configuration;
using Axiom.Core;
using Axiom.Graphics;
using Tao.OpenGl;
using Tao.Sdl;

namespace Axiom.RenderSystems.OpenGL
{
	/// <summary>
	///		Summary description for SdlGLSupport.
	/// </summary>
	public class GLSupport : BaseGLSupport {
		public GLSupport() : base() {
			Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO);
		}

		#region BaseGLSupport Members

		/// <summary>
		///		Returns the pointer to the specified extension function in the GL driver.
		/// </summary>
		/// <param name="extension"></param>
		/// <returns></returns>
		public override IntPtr GetProcAddress(string extension) {
			return Sdl.SDL_GL_GetProcAddress(extension);
		}

		/// <summary>
		///		
		/// </summary>
		public override void AddConfig() {
			// get the available OpenGL resolutions
			Sdl.SDL_Rect[] modes = Sdl.SDL_ListModes(IntPtr.Zero, Sdl.SDL_FULLSCREEN | Sdl.SDL_OPENGL);

			// add the resolutions to the config
			foreach(Sdl.SDL_Rect mode in modes) {
				int width = mode.w;
				int height = mode.h;
				// HACK: How to get bpp?  Assume 16 and 32 are available?
				int bpp = 32;

				// filter out the lower resolutions and dupe frequencies
				if(width >= 640 && height >= 480 && bpp >= 16) {
					string query = string.Format("Width = {0} AND Height= {1} AND Bpp = {2}", width, height, bpp);

					if(engineConfig.DisplayMode.Select(query).Length == 0) {
						// add a new row to the display settings table
						engineConfig.DisplayMode.AddDisplayModeRow(width, height, bpp, false, false);
					}
				}
			}
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="name"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colorDepth"></param>
		/// <param name="fullScreen"></param>
		/// <param name="left"></param>
		/// <param name="top"></param>
		/// <param name="depthBuffer"></param>
		/// <param name="vsync"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public override RenderWindow NewWindow(string name, int width, int height, int colorDepth, bool fullScreen, int left, int top, bool depthBuffer, bool vsync, object target) {
			SdlWindow window = new SdlWindow();
			window.Create(name, width, height, colorDepth, fullScreen, left, top, depthBuffer, vsync);
			return window;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="autoCreateWindow"></param>
		/// <param name="renderSystem"></param>
		/// <param name="windowTitle"></param>
		/// <returns></returns>
		public override RenderWindow CreateWindow(bool autoCreateWindow, GLRenderSystem renderSystem, string windowTitle) {
			RenderWindow autoWindow = null;

			if(autoCreateWindow) {
				// MONO: Could not cast result of Select to strongly typed data row
				DataRow[] modes = 
					(DataRow[])engineConfig.DisplayMode.Select("Selected = true");

				DataRow mode = modes[0];

				int width = (int)mode["Width"];
				int height = (int)mode["Height"];
				int bpp = (int)mode["Bpp"];
				bool fullscreen = (bool)mode["FullScreen"];

				// create the window with the default form as the target
				autoWindow = renderSystem.CreateRenderWindow(windowTitle, width, height, 32, fullscreen, 0, 0, true, false, null);
			}

			return autoWindow;
		}

		#endregion BaseGLSupport Members
	}
}
