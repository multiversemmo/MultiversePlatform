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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Axiom.Core;
using Axiom.Configuration;

namespace Axiom.Graphics {

	///<summary>
	///    Object representing one pass or operation in a composition sequence. This provides a 
	///    method to conviently interleave RenderSystem commands between Render Queues.
	///</summary>
	public class CompositionTargetPass {

		#region Fields

        ///<summary>
        ///    Parent technique
		///</summary>
        protected CompositionTechnique parent;
        ///<summary>
        ///    Input mode
		///</summary>
        protected CompositorInputMode inputMode;
        ///<summary>
        ///    (local) output texture
		///</summary>
        protected string outputName;
        ///<summary>
        ///    Passes
		///</summary>
		protected List<CompositionPass> passes;
        ///<summary>
        ///    This target pass is only executed initially after the effect
        ///    has been enabled.
		///</summary>
        protected bool onlyInitial;
        ///<summary>
        ///    Visibility mask for this render
		///</summary>
        protected uint visibilityMask;
        ///<summary>
        ///    LOD bias of this render
		///</summary>
        protected float lodBias;
        ///<summary>
        ///    Material scheme name
		///</summary>
		protected string materialScheme;

        #endregion Fields


        #region Constructors

		public CompositionTargetPass(CompositionTechnique parent) {
			this.parent = parent;
			inputMode = CompositorInputMode.None;
			passes = new List<CompositionPass>();
			onlyInitial = false;
			visibilityMask = 0xFFFFFFFF;
			lodBias = 1.0f;
			materialScheme = MaterialManager.DefaultSchemeName;
		}
		
        #endregion Constructors


        #region Properties

		public List<CompositionPass> Passes {
			get { return passes; }
		}
		
		public CompositorInputMode InputMode {
			get { return inputMode; }
			set { inputMode = value; }
		}
				
		public string OutputName {
			get { return outputName; }
			set { outputName = value; }
		}
				
		public bool OnlyInitial {
			get { return onlyInitial; }
			set { onlyInitial = value; }
		}
				
		public uint VisibilityMask {
			get { return visibilityMask; }
			set { visibilityMask = value; }
		}
				
		public string MaterialScheme {
			get { return materialScheme; }
			set { materialScheme = value; }
		}
				
		public float LodBias {
			get { return lodBias; }
			set { lodBias = value; }
		}
				
        ///<summary>
        ///    Determine if this target pass is supported on the current rendering device. 
		///</summary>
		public bool IsSupported {
			get {
				// A target pass is supported if all passes are supported
				foreach (CompositionPass pass in passes) {
					if (!pass.IsSupported)
						return false;
				}
				return true;
			}
		}

        #endregion Properties


        #region Methods

        ///<summary>
        ///    Create a new pass, and return a pointer to it.
		///</summary>
		public CompositionPass CreatePass() {
			CompositionPass t = new CompositionPass(this);
			passes.Add(t);
			return t;
		}


        ///<summary>
        ///    Remove a pass.
		///</summary>
		public void RemovePass(int index) {
			passes.RemoveAt(index);
		}

        ///<summary>
        ///    Get a pass.
		///</summary>
		public CompositionPass GetPass(int index)
		{
			return passes[index];
		}

        ///<summary>
        ///    Remove all passes
		///</summary>
		public void RemoveAllPasses() {
			passes.Clear();
		}

        #endregion Methods
	}
}
