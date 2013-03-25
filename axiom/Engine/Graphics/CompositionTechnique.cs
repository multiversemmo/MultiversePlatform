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
	///    Base composition technique, can be subclassed in plugins.
	///</summary>
    public class CompositionTechnique {

        #region Fields
		
        ///<summary>
		///    Parent compositor
		///</summary>
        protected Compositor parent;

        ///<summary>
        ///    Local texture definitions
		///</summary>
		protected List<CompositionTextureDefinition> textureDefinitions;
        ///<summary>
        ///    Intermediate target passes
		///</summary>
        protected List<CompositionTargetPass> targetPasses;
        ///<summary>
        ///    Output target pass (can be only one)
		///</summary>
        protected CompositionTargetPass outputTarget;    

        ///<summary>
		///    List of instances
		///</summary>
		protected List<CompositorInstance> instances;

		#endregion Fields

        #region Constructor

        public CompositionTechnique(Compositor parent) {
			this.parent = parent;
			textureDefinitions = new List<CompositionTextureDefinition>();
			targetPasses = new List<CompositionTargetPass>();
			outputTarget = new CompositionTargetPass(this);
			instances = new List<CompositorInstance>();
		}

		#endregion Constructor

		#region Properties

        ///<summary>
		///    Get the compositor parent
		///</summary>
        public Compositor Parent {
			get { return parent; }
		}

        ///<summary>
		///    Get list of texture definitions
		///</summary>
        public List<CompositionTextureDefinition> TextureDefinitions {
			get { return textureDefinitions; }
		}

        ///<summary>
		///    Get output (final) target pass
		///</summary>
		public CompositionTargetPass OutputTarget {
			get { return outputTarget; }
		}

        ///<summary>
		///    Get the target passes
		///</summary>
		public List<CompositionTargetPass> TargetPasses {
			get { return targetPasses; }
		}


		#endregion Properties
		
		#region Methods

        ///<summary>
		///    Create a new local texture definition, and return a pointer to it.
		///</summary>
		///<param name="name">Name of the local texture</param>
        public CompositionTextureDefinition CreateTextureDefinition(string name) {
			CompositionTextureDefinition t = new CompositionTextureDefinition();
			t.Name = name;
			textureDefinitions.Add(t);
			return t;
		}
        
        ///<summary>
		///    Remove and destroy a local texture definition.
		///</summary>
		public void RemoveTextureDefinition(int idx) {
			textureDefinitions.RemoveAt(idx);
		}
        
        ///<summary>
		///    Get a local texture definition.
		///</summary>
        public CompositionTextureDefinition GetTextureDefinition(int idx) {
			return textureDefinitions[idx];
		}
        
        ///<summary>
		///    Remove all Texture Definitions
		///</summary>
		public void RemoveAllTextureDefinitions() {
			textureDefinitions.Clear();
		}
        
        ///<summary>
		///    Create a new target pass, and return a pointer to it.
		///</summary>
        public CompositionTargetPass CreateTargetPass() {
			CompositionTargetPass t = new CompositionTargetPass(this);
			targetPasses.Add(t);
			return t;
		}
        
        ///<summary>
		///    Remove a target pass. It will also be destroyed.
		///</summary>
		public void RemoveTargetPass(int idx) {
			targetPasses.RemoveAt(idx);
		}
        
        ///<summary>
		///    Get a target pass.
		///</summary>
		public CompositionTargetPass GetTargetPass(int idx) {
			return targetPasses[idx];
		}
		
        ///<summary>
		///    Remove all target passes.
		///</summary>
		public void RemoveAllTargetPasses() {
			targetPasses.Clear();
		}
        
        
        ///<summary>
		///    Determine if this technique is supported on the current rendering device. 
		///</summary>
		///<param name="allowTextureDegradation">True to accept a reduction in texture depth<param>
		public virtual bool IsSupported(bool allowTextureDegradation) {
			// A technique is supported if all materials referenced have a supported
			// technique, and the intermediate texture formats requested are supported
			// Material support is a cast-iron requirement, but if no texture formats 
			// are directly supported we can let the rendersystem create the closest 
			// match for the least demanding technique


			// Check output target pass is supported
			if (!outputTarget.IsSupported)
				return false;

			// Check all target passes is supported
			foreach (CompositionTargetPass targetPass in targetPasses) {
				if (!targetPass.IsSupported)
					return false;
			}

			TextureManager texMgr = TextureManager.Instance;
			foreach (CompositionTextureDefinition td in textureDefinitions)
			{
				// Check whether equivalent supported
				if(allowTextureDegradation) {
					// Don't care about exact format so long as something is supported
					if(texMgr.GetNativeFormat(TextureType.TwoD, td.Format,
                                              TextureUsage.RenderTarget) == Axiom.Media.PixelFormat.Unknown)
						return false;
				}
				else {
					// Need a format which is the same number of bits to pass
					if (!texMgr.IsEquivalentFormatSupported(TextureType.TwoD, td.Format, TextureUsage.RenderTarget))
						return false;
				}
			}

			// Must be ok
			return true;
		}

        ///<summary>
		///    Create an instance of this technique.
		///</summary>
		public virtual CompositorInstance CreateInstance(CompositorChain chain) {
			CompositorInstance mew = new CompositorInstance(parent, this, chain);
			instances.Add(mew);
			return mew;
		}
        
        ///<summary>
		///    Destroy an instance of this technique.
		///</summary>
        public virtual void DestroyInstance(CompositorInstance instance) {
			instances.Remove(instance);
		}

        #endregion Methods
		
	}
		
    public class CompositionTextureDefinition {
		
		#region Fields

		protected string name;
        protected int width = 0;       // 0 means adapt to target width
		protected int height = 0;      // 0 means adapt to target height
		protected Axiom.Media.PixelFormat format = Axiom.Media.PixelFormat.A8R8G8B8;

		#endregion Fields


		#region Constructor

		public CompositionTextureDefinition() {
        }

		#endregion Constructor


		#region Properties

		public string Name {
			get { return name; }
            set { name = value; }
		}

		public int Width {
			get { return width; }
			set { width = value; }
		}

        public int Height {
            get { return height; }
            set { height = value; }
        }
					
		public Axiom.Media.PixelFormat Format {
			get { return format; }
			set { format = value; }
		}

		#endregion Properties

    }

}
