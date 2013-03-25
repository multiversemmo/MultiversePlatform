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
using Axiom.Core;
using Axiom.Graphics;
using Tao.OpenGl;

namespace Axiom.RenderSystems.OpenGL {
	/// <summary>
	/// Summary description for GLHardwareOcclusionQuery.
	/// </summary>
	public class GLHardwareOcclusionQuery : IHardwareOcclusionQuery {
		/// <summary>
		///		Number of fragments returned from the last query.
		/// </summary>
		private int lastFragmentCount;
		/// <summary>
		///		Flag that indicates whether hardware queries are supported
		/// </summary>
		private bool isSupported;
		/// <summary>
		///		Rate at which queries are skipped (in frames).
		/// </summary>
		private int skipRate;
		/// <summary>
		///		Current count of number of skipped frames since query last ran.
		/// </summary>
		private int skipCounter;
		/// <summary>
		///		Id of the GL query.
		/// </summary>
		private int id;
		
		public GLHardwareOcclusionQuery() {
			isSupported = Root.Instance.RenderSystem.Caps.CheckCap(Capabilities.HardwareOcculusion);

			if(isSupported) {
				Gl.glGenOcclusionQueriesNV(1, out id);
			}
		}

		#region IHardwareOcclusionQuery Members

		public void Begin() {
			// proceed if supported, or silently fail otherwise
			if(isSupported) {
				if(skipCounter == skipRate) {
					skipCounter = 0;
				}

				if(skipCounter == 0) { // && lastFragmentCount != 0) {
					Gl.glBeginOcclusionQueryNV(id);
				}
			}
		}

		public int PullResults(bool flush) {
			// note: flush doesn't apply to GL

			// default to returning a high count.  will be set otherwise if the query runs
			lastFragmentCount = 100000;

			if(isSupported) {
				Gl.glGetOcclusionQueryivNV(id, Gl.GL_PIXEL_COUNT_NV, out lastFragmentCount);
			}
				
			return lastFragmentCount;
		}

		public void End() {
			// proceed if supported, or silently fail otherwise
			if(isSupported) {
				if(skipCounter == 0) { // && lastFragmentCount != 0) {
					Gl.glEndOcclusionQueryNV();
				}

				skipCounter++;
			}
		}

		public int SkipRate {
			get {
				return skipRate;
			}
			set {
				skipRate = value;
			}
		}

		public int LastFragmentCount {
			get {
				return lastFragmentCount;
			}
		}

		#endregion
	}
}
