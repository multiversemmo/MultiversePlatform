using System;
using Axiom.Core;
using Axiom.Graphics;
using Tao.OpenGl;
using Tao.Sdl;

namespace Axiom.RenderSystems.OpenGL {
	/// <summary>
	/// Summary description for SdlWindow.
	/// </summary>
	public class SdlWindow : RenderWindow {
		#region Fields

		//private Sdl.SDL_Surface surface;

		#endregion Fields

		public SdlWindow() {
		}

		#region RenderWindow Members

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
		/// <param name="miscParams"></param>
		public override void Create(string name, int width, int height, int colorDepth, bool fullScreen, int left, int top, bool depthBuffer, params object[] miscParams) {
			this.name = name;
			this.width = width;
			this.height = height;
			this.colorDepth = colorDepth;

			int flags = Sdl.SDL_OPENGL | Sdl.SDL_HWPALETTE;

			// full screen?
			if(fullScreen) {
				flags |= Sdl.SDL_FULLSCREEN;
			}

			// we want double buffering
			Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_DOUBLEBUFFER, 1 );

			// request good stencil size if 32-bit color
			if (colorDepth == 32 && depthBuffer) {
				Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_STENCIL_SIZE, 8);
			}

			// set the video mode (and create the surface)
			// TODO: Grab return val once changed to the right type
			Sdl.SDL_SetVideoMode(width, height, colorDepth, flags);

			// lets get active!
			isActive = true;

			// set the window text for windowed mode
			if(!fullScreen) {
				Sdl.SDL_WM_SetCaption(name, null);
			}
		}

		public void Destroy() {
			//Sdl.SDL_FreeSurface(
		}

		public override void Reposition(int left, int right) {

		}

		public override void Resize(int width, int height) {

		}

		public void SaveToFile(string fileName) {

		}

		/// <summary>
		///		Update the render window.
		/// </summary>
		/// <param name="waitForVSync"></param>
		public override void SwapBuffers(bool waitForVSync) {
			Sdl.SDL_GL_SwapBuffers();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		public override void Save(System.IO.Stream stream)
		{
		}



		#endregion RenderWindow Members
	}
}
