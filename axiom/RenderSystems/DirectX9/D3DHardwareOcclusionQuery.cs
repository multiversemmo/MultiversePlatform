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
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;

namespace Axiom.RenderSystems.DirectX9 {
	/// <summary>
	///		Direct3D implementation of a hardware occlusion query.
	/// </summary>
	// Original Author: Lee Sandberg
	public class D3DHardwareOcclusionQuery : IHardwareOcclusionQuery {
		#region Fields

		/// <summary>
		///		Reference to the current Direct3D device object.
		/// </summary>
		private D3D.Device device;
		/// <summary>
		///		Reference to the query object being used.
		/// </summary>
		private D3D.Query query;
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

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Default constructor.
		/// </summary>
		/// <param name="device">Reference to a Direct3D device.</param>
		public D3DHardwareOcclusionQuery(D3D.Device device) {
			this.device = device;

			// check if queries are supported
			isSupported = Root.Instance.RenderSystem.Caps.CheckCap(Capabilities.HardwareOcculusion);

			if(isSupported) {
				// attempt to create an occlusion query
				query = new D3D.Query(device, QueryType.Occlusion);
			}
		}

		#endregion Constructor

		#region IHardwareOcclusionQuery Members

		public void Begin() {
			// proceed if supported, or silently fail otherwise
			if(isSupported) {
				if(skipCounter == skipRate) {
					skipCounter = 0;
				}

				if(skipCounter == 0) { // && lastFragmentCount != 0) {
					query.Issue(IssueFlags.Begin);
				}
			}
		}

		public int PullResults(bool flush) {
			// default to returning a high count.  will be set otherwise if the query runs
			lastFragmentCount = 100000;

			if(isSupported) {
				lastFragmentCount = (int)query.GetData(typeof(int), flush);
			}

			return lastFragmentCount;
		}

		public void End() {
			// proceed if supported, or silently fail otherwise
			if(isSupported) {
				if(skipCounter == 0) { // && lastFragmentCount != 0) {
					query.Issue(IssueFlags.End);
				}

				skipCounter++;
			}
		}

		/// <summary>
		///		Rate (in frames) at which queries are skipped.
		/// </summary>
		public int SkipRate {
			get {
				return skipRate;
			}
			set {
				skipRate = value;
			}
		}

		/// <summary>
		///		Gets the number of fragments returned from the last execution of this query.
		/// </summary>
		public int LastFragmentCount {
			get {
				return lastFragmentCount;
			}
		}

		#endregion
	}
}
