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
	public class CompositionPass {
		
		#region Fields

        ///<summary>
        ///    Parent technique
		///</summary>
        protected CompositionTargetPass parent;
        ///<summary>
        ///    Type of composition pass
		///</summary>
		protected CompositorPassType type;
        ///<summary>
        ///    Identifier for this pass
		///</summary>
		protected uint identifier;
        ///<summary>
        ///    Material used for rendering
		///</summary>
        protected Material material;
        ///<summary>
        ///    first render queue to render this pass (in case of CompositorPassType.RenderScene)
		///</summary>
		protected RenderQueueGroupID firstRenderQueue;
        ///<summary>
        ///    last render queue to render this pass (in case of CompositorPassType.RenderScene)
		///</summary>
		protected RenderQueueGroupID lastRenderQueue;
        ///<summary>
        ///    Clear buffers (in case of CompositorPassType.Clear)
		///</summary>
        protected FrameBuffer clearBuffers;
        ///<summary>
        ///    Clear colour (in case of CompositorPassType.Clear)
		///</summary>
        protected ColorEx clearColor;
        ///<summary>
        ///    Clear depth (in case of CompositorPassType.Clear)
		///</summary>
		protected float clearDepth;
        ///<summary>
        ///    Clear stencil value (in case of CompositorPassType.Clear)
		///</summary>
		protected int clearStencil;
        ///<summary>
        ///    Inputs (for material used for rendering the quad)
        ///    An empty string signifies that no input is used
		///</summary>
        protected string [] inputs = new string[Config.MaxTextureLayers];
        ///<summary>
        ///    Stencil operation parameters
		///</summary>
		protected bool stencilCheck;
		protected CompareFunction stencilFunc; 
		protected int stencilRefValue;
		protected int stencilMask;
		protected StencilOperation stencilFailOp;
		protected StencilOperation stencilDepthFailOp;
		protected StencilOperation stencilPassOp;
		protected bool stencilTwoSidedOperation;

		#endregion Fields


		#region Constructor

		public CompositionPass(CompositionTargetPass parent) {
			this.parent = parent;
			type = CompositorPassType.RenderQuad;
			identifier = 0;
			firstRenderQueue = RenderQueueGroupID.SkiesEarly;
			lastRenderQueue = RenderQueueGroupID.SkiesLate;
			clearBuffers = FrameBuffer.Color | FrameBuffer.Depth;
			clearColor = new ColorEx(0f, 0f, 0f, 0f);
			clearDepth = 1.0f;
			clearStencil = 0;
			stencilCheck = false;
			stencilFunc = CompareFunction.AlwaysPass;
			stencilRefValue = 0;
			stencilMask = (int)0x7FFFFFFF;
			stencilFailOp = StencilOperation.Keep;
			stencilDepthFailOp = StencilOperation.Keep;
			stencilPassOp = StencilOperation.Keep;
			stencilTwoSidedOperation = false;
		}

        #endregion Constructor


        #region Properties

		public CompositionTargetPass Parent {
			get { return parent; }
		}
		
		public CompositorPassType Type {
			get { return type; }
			set { type = value; }
		}
				
		public uint Identifier {
			get { return identifier; }
			set { identifier = value; }
		}
				
		public Material Material {
			get { return material; }
			set { material = value; }
		}
				
		public string MaterialName {
			set { material = MaterialManager.Instance.GetByName(value); }
		}
		
		public RenderQueueGroupID FirstRenderQueue {
			get { return firstRenderQueue; }
			set { firstRenderQueue = value; }
		}
				
		public RenderQueueGroupID LastRenderQueue {
			get { return lastRenderQueue; }
			set { lastRenderQueue = value; }
		}
				
		public FrameBuffer ClearBuffers {
			get { return clearBuffers; }
			set { clearBuffers = value; }
		}
				
		public ColorEx ClearColor {
			get { return clearColor; }
			set { clearColor = value; }
		}
				
		public float ClearDepth {
			get { return clearDepth; }
			set { clearDepth = value; }
		}
				
		public int ClearStencil {
			get { return clearStencil; }
			set { clearStencil = value; }
		}
				
		public bool StencilCheck {
			get { return stencilCheck; }
			set { stencilCheck = value; }
		}
				
		public CompareFunction StencilFunc {
			get { return stencilFunc; }
			set { stencilFunc = value; }
		}
				
		public int StencilRefValue {
			get { return stencilRefValue; }
			set { stencilRefValue = value; }
		}
				
		public int StencilMask {
			get { return stencilMask; }
			set { stencilMask = value; }
		}
				
		public StencilOperation StencilFailOp {
			get { return stencilFailOp; }
			set { stencilFailOp = value; }
		}
				
		public StencilOperation StencilDepthFailOp {
			get { return stencilDepthFailOp; }
			set { stencilDepthFailOp = value; }
		}
				
		public StencilOperation StencilPassOp {
			get { return stencilPassOp; }
			set { stencilPassOp = value; }
		}
				
		public bool StencilTwoSidedOperation {
			get { return stencilTwoSidedOperation; }
			set { stencilTwoSidedOperation = value; }
		}
				
        public bool IsSupported {
			get { 
				if (type == CompositorPassType.RenderQuad) {
					if (material == null)
						return false;
					material.Compile();
					if (material.SupportedTechniques.Count == 0)
						return false;
				}
				return true;
			}
		}
		
		public string[] Inputs {
			get { return inputs; }
		}
		
		#endregion Properties


		#region Methods

        ///<summary>
        ///    Set an input local texture. An empty string clears the input.
		///</summary>
        ///<param name="id">Input to set. Must be in 0..Config.MaxTextureLayers-1</param>
        ///<param name="input"Which texture to bind to this input. An empty string clears the input.</param>
		///<remarks>
		///    Note applies when CompositorPassType is RenderQuad 
		///</remarks>	
		public void SetInput(int id, string input) {
            inputs[id] = input;
		}
        
		public void SetInput(int id) {
			SetInput(id, "");
		}
        
        ///<summary>
		///    Get the value of an input.
		///</summary>
		///<param name="id">Input to get. Must be in 0..Config.MaxTextureLayers-1.</param>
		///<remarks>
		///    Note applies when CompositorPassType is RenderQuad 
		///</remarks>	
		public string GetInput(int id) {
			return inputs[id];
		}
        
        ///<summary>
		///    Get the number of inputs used.  If there are holes in the inputs array,
        ///    this number will include those entries as well.
		///</summary>
		///<remarks>
		///    Note applies when CompositorPassType is RenderQuad 
		///</remarks>	
        public int GetNumInputs() {
			int count = 0;
            for (int i = 0; i < inputs.Length; ++i) {
                string s = inputs[i];
                if (s != null && s != "")
                    count = i + 1;
			}
			return count;
		}
        
        ///<summary>
		///    Clear all inputs.
		///</summary>
		///<remarks>
		///    Note applies when CompositorPassType is RenderQuad 
		///</remarks>	
		public void ClearAllInputs() {
			for (int i=0; i<Config.MaxTextureLayers; i++)
				inputs[i] = "";
		}
			
        #endregion Methods
		
	}
}
