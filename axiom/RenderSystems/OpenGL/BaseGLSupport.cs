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
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using Axiom.Configuration;
using Axiom.Graphics;
using Tao.OpenGl;

namespace Axiom.RenderSystems.OpenGL {
	/// <summary>
	/// Summary description for GLHelper.
	/// </summary>
	public abstract class BaseGLSupport {
		#region Fields

		/// <summary>
		///		Collection of extensions supported by the current hardware.
		/// </summary>
		private static StringCollection extensionList;
		/// <summary>
		///		OpenGL version string.
		/// </summary>
		private static string glVersion;
		/// <summary>
		///		Vendor of the current hardware.
		/// </summary>
		private static string vendor;
		/// <summary>
		///		Name of the video card in use.
		/// </summary>
		private static string videoCard;
		/// <summary>
		///		Config options.
		/// </summary>
		protected EngineConfig engineConfig = new EngineConfig();

		#endregion Fields

		#region Base Members

		#region Properties

		/// <summary>
		///		Gets the options currently set by the current GL implementation.
		/// </summary>
		public EngineConfig ConfigOptions {
			get {
				return engineConfig;
			}
		}

		/// <summary>
		///		Gets a collection of strings listing all the available extensions.
		/// </summary>
		public StringCollection Extensions {
			get {
				return extensionList; 
			}
		}

		/// <summary>
		///		Name of the vendor for the current video hardware.
		/// </summary>
		public string Vendor {
			get { 
				return vendor; 
			}
		}

		/// <summary>
		///		Name/brand of the current video hardware.
		/// </summary>
		public string VideoCard {
			get { 
				return videoCard; 
			}
		}

		/// <summary>
		///		Version string for the current OpenGL driver.
		/// </summary>
		public string Version {
			get { 
				return glVersion; 
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>
		///		Handy check to see if the current GL version is at least what is supplied.
		/// </summary>
		/// <param name="version">What you want to check for, i.e. "1.3" </param>
		/// <returns></returns>
		public bool CheckMinVersion(string version) {
			return glVersion.StartsWith(version);
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="extention"></param>
		/// <returns></returns>
		public bool CheckExtension(string extention) {
			// check if the extension is supported
			return extensionList.Contains(extention);
		}

		/// <summary>
		/// 
		/// </summary>
		public void InitializeExtensions() {
			if(extensionList == null) {
				GlExtensionLoader.LoadAllExtensions();

				// get the OpenGL version string and vendor name
				glVersion = Marshal.PtrToStringAnsi(Gl.glGetString(Gl.GL_VERSION));
				videoCard = Marshal.PtrToStringAnsi(Gl.glGetString(Gl.GL_RENDERER));
				vendor = Marshal.PtrToStringAnsi(Gl.glGetString(Gl.GL_VENDOR));

				// parse out the first piece of the vendor string if there are spaces in it
				if(vendor.IndexOf(" ") != -1) {
					vendor = vendor.Substring(0, vendor.IndexOf(" "));
				}

				// create a new extension list
				extensionList = new StringCollection();

				string allExt = Marshal.PtrToStringAnsi(Gl.glGetString(Gl.GL_EXTENSIONS));
				string[] splitExt = allExt.Split(Char.Parse(" "));

				// store the parsed extension list
				for(int i = 0; i < splitExt.Length; i++) {
					extensionList.Add(splitExt[i]);
				}
			}
		}

		#endregion Methods

		#endregion Base Members

		#region Abstract Members

		/// <summary>
		///		Add any special config values to the system.
		/// </summary>
		public abstract void AddConfig();

		/// <summary>
		///		
		/// </summary>
		/// <param name="autoCreateWindow"></param>
		/// <param name="renderSystem"></param>
		/// <param name="windowTitle"></param>
		/// <returns></returns>
		public abstract RenderWindow CreateWindow(bool autoCreateWindow, GLRenderSystem renderSystem, string windowTitle);

		/// <summary>
		///		Subclasses need to implement a means to return the pointer to the extension function
		///		for OpenGL calls.
		/// </summary>
		/// <param name="extension">Name of the extension to retreive the pointer for.</param>
		/// <returns>Pointer to the location of the function in the OpenGL driver modules.</returns>
		public abstract IntPtr GetProcAddress(string extension);

		/// <summary>
		///		Creates a specific instance of a render window.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colorDepth"></param>
		/// <param name="fullScreen"></param>
		/// <param name="left"></param>
		/// <param name="top"></param>
		/// <param name="depthBuffer"></param>
		/// <param name="parent"></param>
		/// <param name="vsync"></param>
		/// <returns></returns>
		public abstract RenderWindow NewWindow(string name, int width, int height, int colorDepth, bool fullScreen, 
			int left, int top, bool depthBuffer, bool vsync, object target);

		#endregion Abstract Members
	}
}
