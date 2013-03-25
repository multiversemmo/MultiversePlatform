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
	///    An instance of a Compositor object for one Viewport. It is part of the CompositorChain
	///    for a Viewport.
	///</summary>
	public class CompositorInstance {

		#region Fields

		///<summary>
		///    Compositor of which this is an instance
		///</summary>
        protected Compositor compositor;
		///<summary>
		///    Composition technique used by this instance
		///</summary>
        protected CompositionTechnique technique;
		///<summary>
		///    Composition chain of which this instance is part
		///</summary>
        protected CompositorChain chain;
		///<summary>
		///    Is this instance enabled?
		///</summary>
        protected bool enabled;
		///<summary>
		///    Map from name->local texture
		///</summary>
        protected Dictionary<string, Texture> localTextures;
		///<summary>
		///    Render System operations queued by last compile, these are created by this
		///    instance thus managed and deleted by it. The list is cleared with 
		///    clearCompilationState()
		///</summary>
		protected List<QueueIDAndOperation> renderSystemOperations;
		///<summary>
		///    Vector of listeners
		///</summary>
		protected List<CompositorInstanceListener> listeners;
		///<summary>
		///    Previous instance (set by chain)
		///</summary>
        protected CompositorInstance previousInstance;

        protected static int materialDummyCounter = 0;
 
        protected static int resourceDummyCounter = 0;
       
		#endregion Fields

		#region Constructor

        public CompositorInstance(Compositor filter, CompositionTechnique technique, CompositorChain chain) {
			this.compositor = filter;
			this.technique = technique;
			this.chain = chain;
			this.enabled = false;
			localTextures = new Dictionary<string, Texture>();
			renderSystemOperations = new List<QueueIDAndOperation>();
			listeners = new List<CompositorInstanceListener>();
		}

		#endregion Constructor


		#region Properties

        public Compositor Compositor {
			get { return compositor; }
			set { compositor = value; }
		}

        public CompositionTechnique Technique {
			get { return technique; }
			set { technique = value; }
		}

        public CompositorChain Chain {
			get { return chain; }
			set { chain = value; }
		}

		public bool Enabled {
			get {
				return enabled;
			}
			set {
				if (enabled != value) {
					enabled = value;
					// Create of free resource.
					if (value)
						CreateResources();
					else
						FreeResources();
				}
				/// Notify chain state needs recompile.
					chain.Dirty = true;
			}
		}
		
        public Dictionary<string, Texture> LocalTextures {
			get { return localTextures; }
			set { localTextures = value; }
		}

		public List<QueueIDAndOperation> RenderSystemOperations {
			get { return renderSystemOperations; }
			set { renderSystemOperations = value; }
		}

		public List<CompositorInstanceListener> Listeners {
			get { return listeners; }
			set { listeners = value; }
		}

        public CompositorInstance PreviousInstance {
			get { return previousInstance; }
			set { previousInstance = value; }
		}

		#endregion Properties


		#region Methods

		///<summary>
		///    Collect rendering passes. Here, passes are converted into render target operations
		///    and queued with queueRenderSystemOp.
		///</summary>
		protected void CollectPasses(CompositorTargetOperation finalState, CompositionTargetPass target) {
			/// Here, passes are converted into render target operations
			Pass targetpass;
			Technique srctech;
			Material srcmat;

			foreach (CompositionPass pass in target.Passes) {
				switch(pass.Type) {
				case CompositorPassType.Clear:
					QueueRenderSystemOp(finalState, new RSClearOperation(
						pass.ClearBuffers,
						pass.ClearColor,
						pass.ClearDepth,
						pass.ClearStencil));
					break;
				case CompositorPassType.Stencil:
					QueueRenderSystemOp(finalState, new RSStencilOperation(
						pass.StencilCheck, pass.StencilFunc, pass.StencilRefValue,
						pass.StencilMask, pass.StencilFailOp, pass.StencilDepthFailOp,
						pass.StencilPassOp, pass.StencilTwoSidedOperation
						));
					break;
				case CompositorPassType.RenderScene:
					if((int)pass.FirstRenderQueue < (int)finalState.CurrentQueueGroupID) {
						/// Mismatch -- warn user
						/// XXX We could support repeating the last queue, with some effort
						LogManager.Instance.Write("Warning in compilation of Compositor "
							+ compositor.Name + ": Attempt to render queue " +
							pass.FirstRenderQueue + " before "+
							finalState.CurrentQueueGroupID);
					}
					/// Add render queues
					for(RenderQueueGroupID x=pass.FirstRenderQueue; x<=pass.LastRenderQueue; ++x) {
						Debug.Assert(x>=0);
						finalState.RenderQueues[(int)x] = true;
					}
					finalState.CurrentQueueGroupID = (RenderQueueGroupID)((int)pass.LastRenderQueue + 1);
					finalState.FindVisibleObjects = true;
					finalState.MaterialScheme = target.MaterialScheme;

					break;
				case CompositorPassType.RenderQuad:
					srcmat = pass.Material;
					if(srcmat == null) {
						/// No material -- warn user
						LogManager.Instance.Write("Warning in compilation of Compositor "
							+ compositor.Name + ": No material defined for composition pass");
						break;
					}
					srcmat.Load();
					if(srcmat.SupportedTechniques.Count == 0) {
						/// No supported techniques -- warn user
						LogManager.Instance.Write("Warning in compilation of Compositor "
							+ compositor.Name + ": material " + srcmat.Name + " has no supported techniques");
						break;
					}
					srctech = srcmat.GetBestTechnique(0);
					/// Create local material
					Material localMat = CreateLocalMaterial();
					/// Copy and adapt passes from source material
					for (int i=0; i<srctech.NumPasses; i++) {
					    Pass srcpass = srctech.GetPass(i);
						/// Create new target pass
				        targetpass = localMat.GetTechnique(0).CreatePass();
			            srcpass.CopyTo(targetpass);
	                    /// Set up inputs
                        int numInputs = pass.GetNumInputs();
                        for (int x = 0; x < numInputs; x++) {
						    string inp = pass.Inputs[x];
							if (inp != string.Empty) {
								if (x < targetpass.NumTextureUnitStages)
									targetpass.GetTextureUnitState(x).SetTextureName(GetSourceForTex(inp));
								else {
									/// Texture unit not there
									LogManager.Instance.Write("Warning in compilation of Compositor "
								    		 + compositor.Name + ": material " + srcmat.Name + " texture unit "
									    	 + x + " out of bounds");
								}
							}
						}
					}
					QueueRenderSystemOp(finalState, new RSQuadOperation(this, pass.Identifier, localMat));
					break;
				}
			}
		}

		///<summary>
		///    Recursively collect target states (except for final Pass).
		///</summary>
        ///<param name="compiledState">This vector will contain a list of TargetOperation objects</param>
		public void CompileTargetOperations(List<CompositorTargetOperation> compiledState) {
			/// Collect targets of previous state
			if(previousInstance != null)
				previousInstance.CompileTargetOperations(compiledState);
			/// Texture targets
			foreach (CompositionTargetPass target in technique.TargetPasses) {
				CompositorTargetOperation ts = new CompositorTargetOperation(GetTargetForTex(target.OutputName));
				/// Set "only initial" flag, visibilityMask and lodBias according to CompositionTargetPass.
				ts.OnlyInitial = target.OnlyInitial;
				ts.VisibilityMask = target.VisibilityMask;
				ts.LodBias = target.LodBias;
				/// Check for input mode previous
				if(target.InputMode == CompositorInputMode.Previous) {
					/// Collect target state for previous compositor
					/// The TargetOperation for the final target is collected seperately as it is merged
					/// with later operations
					previousInstance.CompileOutputOperation(ts);
				}
				/// Collect passes of our own target
				CollectPasses(ts, target);
				compiledState.Add(ts);
			}
		}

		///<summary>
		///    Compile the final (output) operation. This is done seperately because this
		///    is combined with the input in chained filters.
		///</summary>
		public void CompileOutputOperation(CompositorTargetOperation finalState)
		{
			/// Final target
			CompositionTargetPass tpass = technique.OutputTarget;

			/// Logical-and together the visibilityMask, and multiply the lodBias
			finalState.VisibilityMask &= tpass.VisibilityMask;
			finalState.LodBias *= tpass.LodBias;

			if(tpass.InputMode == CompositorInputMode.Previous) {
				/// Collect target state for previous compositor
				/// The TargetOperation for the final target is collected seperately as it is merged
				/// with later operations
				previousInstance.CompileOutputOperation(finalState);
			}
			/// Collect passes
			CollectPasses(finalState, tpass);
		}

		///<summary>
		///    Get the instance name for a local texture.
		///</summary>
		///<remarks>
		///    It is only valid to call this when local textures have been loaded, 
		///    which in practice means that the compositor instance is active. Calling
		///    it at other times will cause an exception. Note that since textures
		///    are cleaned up aggressively, this name is not guaranteed to stay the
		///    same if you disable and renable the compositor instance.
		///</remarks>
		///<param name="name">The name of the texture in the original compositor definition</param>
		///<returns>The instance name for the texture, corresponds to a real texture</returns>
		public string GetTextureInstanceName(string name) {
			return GetSourceForTex(name);
		}

		///<summary>
		///    Create a local dummy material with one technique but no passes.
		///    The material is detached from the Material Manager to make sure it is destroyed
		///    when going out of scope.
		///</summary>
		protected Material CreateLocalMaterial() {
			Material mat = (Material)MaterialManager.Instance.Create("CompositorInstanceMaterial" + materialDummyCounter);
			++materialDummyCounter;
            // Ogre removed it from the resource list, but no such API exists in
            // in Axiom.
			MaterialManager.Instance.Unload(mat);
			/// Remove all passes from first technique
			mat.GetTechnique(0).RemoveAllPasses();
			return mat;
		}

		///<summary>
		///    Create local rendertextures and other resources. Builds mLocalTextures.
		///</summary>
		protected void CreateResources() {
			FreeResources();
			/// Create temporary textures
			/// In principle, temporary textures could be shared between multiple viewports
			/// (CompositorChains). This will save a lot of memory in case more viewports
			/// are composited.
			foreach (CompositionTextureDefinition def in technique.TextureDefinitions) {
				/// Determine width and height
				int width = def.Width;
				int height = def.Height;
				if(width == 0)
					width = chain.Viewport.ActualWidth;
				if(height == 0)
					height = chain.Viewport.ActualHeight;
				/// Make the texture
				Texture tex = TextureManager.Instance.CreateManual(
					"CompositorInstanceTexture" + resourceDummyCounter, 
					TextureType.TwoD, width, height, 0, def.Format, TextureUsage.RenderTarget);    
				++resourceDummyCounter;
				localTextures[def.Name] = tex;

				/// Set up viewport over entire texture
                RenderTexture rtt = tex.GetBuffer().GetRenderTarget();
				rtt.IsAutoUpdated = false;

				Camera camera = chain.Viewport.Camera;

				// Save last viewport and current aspect ratio
				Viewport oldViewport = camera.Viewport;
				float aspectRatio = camera.AspectRatio;

				Viewport v = rtt.AddViewport(camera);
				v.ClearEveryFrame = false;
				v.OverlaysEnabled = false;
				v.BackgroundColor = new ColorEx(0f, 0f, 0f, 0f);

				// Should restore aspect ratio, in case of auto aspect ratio
				// enabled, it'll changed when add new viewport.
				camera.AspectRatio = aspectRatio;
				// Should restore last viewport, i.e. never disturb user code
				// which might based on that.
				camera.NotifyViewport(oldViewport);
			}
		}

		///<summary>
		///    Destroy local rendertextures and other resources.
		///</summary>
		protected void FreeResources() {
			/// Remove temporary textures
			foreach (Texture t in localTextures.Values)
				TextureManager.Instance.Unload(t);
			localTextures.Clear();
		}

		///<summary>
		///    Get the instance name for a local texture.
		///</summary>
		///<remarks>
		///    It is only valid to call this when local textures have been loaded, 
		///    it at other times will cause an exception. Note that since textures
		///    are cleaned up aggressively, this name is not guaranteed to stay the
		///    same if you disable and renable the compositor instance.
		///</remarks>
		///<param name="name">The name of the texture in the original compositor definition</param>
		///<returns>The instance name for the texture, corresponds to a real texture</returns>
		public RenderTarget GetTargetForTex(string name) {
			Texture tex;
			if (localTextures.TryGetValue(name, out tex))
				return tex.GetBuffer().GetRenderTarget();
			else
				throw new Exception("Non-existent local texture name" + name);
		}

		///<summary>
		///    Get source texture name for a named local texture.
		///</summary>
		protected string GetSourceForTex(string name) {
			Texture tex;
			if (localTextures.TryGetValue(name, out tex))
				return tex.Name;
			else
				throw new Exception("Non-existent local texture name" + name);
		}

		///<summary>
		///    Queue a render system operation.
		///</summary>
		///<returns>destination pass</return>
		protected void QueueRenderSystemOp(CompositorTargetOperation finalState, CompositorRenderSystemOperation op) {
			/// Store operation for current QueueGroup ID
			finalState.RenderSystemOperations.Add(new QueueIDAndOperation(finalState.CurrentQueueGroupID, op));
			/// Save a pointer, so that it will be freed on recompile
			chain.RenderSystemOperations.Add(op);
		}

		///<summary>
		///    Add a listener. Listeners provide an interface to "listen in" to to render system 
		///    operations executed by this CompositorInstance so that materials can be 
		///    programmatically set up.
		///    @see CompositorInstance::Listener
		///</summary>
		public void AddListener(CompositorInstanceListener listener) {
			listeners.Add(listener);
		}

		///<summary>
		///    Remove a listener.
		///    @see CompositorInstance::Listener
		///</summary>
		public void RemoveListener(CompositorInstanceListener listener) {
			listeners.Remove(listener);
		}

		///<summary>
		///    Notify listeners of a material compilation.
		///</summary>
		public void FireNotifyMaterialSetup(uint pass_id, Material mat) {
			foreach (CompositorInstanceListener listener in listeners)
				listener.NotifyMaterialSetup(pass_id, mat);
		}

		///<summary>
		///    Notify listeners of a material render.
		///</summary>
		public void FireNotifyMaterialRender(uint pass_id, Material mat) {
			foreach (CompositorInstanceListener listener in listeners)
				listener.NotifyMaterialRender(pass_id, mat);
		}

		#endregion Methods
		
	}

	public class CompositorInstanceListener {

		///<summary>
		///    Notification of when a render target operation involving a material (like
		///    rendering a quad) is compiled, so that miscelleneous parameters that are different
		///    per Compositor instance can be set up.
		///</summary>
        ///<param name="pass_id">
        ///    Pass identifier within Compositor instance, this is speficied 
		///    by the user by CompositionPass::setIdentifier().
        ///</param>			
		///<param name="mat"
        ///    Material, this may be changed at will and will only affect
		///    the current instance of the Compositor, not the global material
		///    it was cloned from.
        ///</param>
		public virtual void NotifyMaterialSetup(uint pass_id, Material mat) {
        }

		///<summary>
        ///    Notification before a render target operation involving a material (like
		///    rendering a quad), so that material parameters can be varied.
		///</summary>
        ///<param name="pass_id">
        ///    Pass identifier within Compositor instance, this is speficied 
		///    by the user by CompositionPass::setIdentifier().
        ///</param>			
		///<param name="mat"
        ///    Material, this may be changed at will and will only affect
		///    the current instance of the Compositor, not the global material
		///    it was cloned from.
        ///</param>
        public virtual void NotifyMaterialRender(uint pass_id, Material mat) {
		}
			
	}

	///<summary>
	///    Base class for other render system operations
	///</summary>
	abstract public class CompositorRenderSystemOperation {
		/// Set state to SceneManager and RenderSystem
		public abstract void Execute(SceneManager sm, RenderSystem rs);
	}

	///<summary>
	///    Clear framebuffer RenderSystem operation
	///</summary>
	public class RSClearOperation : CompositorRenderSystemOperation {

		#region Fields

		///<summary>
		///    Which buffers to clear (FrameBuffer)
		///</summary>
		protected FrameBuffer buffers;
		///<summary>
		///    Color to clear in case FrameBuffer.Color is set
		///</summary>
		protected ColorEx color;
		///<summary>
		///    Depth to set in case FrameBuffer.Depth is set
		///</summary>
		protected float depth;
		///<summary>
		///    Stencil value to set in case FrameBuffer.Stencil is set
		///</summary>
		protected int stencil;

		#endregion Fields

		#region Constructor

		public RSClearOperation(FrameBuffer buffers, ColorEx color, float depth, int stencil) {
			this.buffers = buffers;
			this.color = color;
			this.depth = depth;
			this.stencil = stencil;
		}

		#endregion Constructor
		
		#region Methods

		public override void Execute(SceneManager sm, RenderSystem rs) {
			rs.ClearFrameBuffer(buffers, color, depth, stencil);
		}

		#endregion Methods
	}

	///<summary>
	///    "Set stencil state" RenderSystem operation
	///</summary>
	public class RSStencilOperation : CompositorRenderSystemOperation {

		#region Fields

		protected bool stencilCheck;
		protected CompareFunction func; 
		protected int refValue;
		protected int mask;
		protected StencilOperation stencilFailOp;
		protected StencilOperation depthFailOp;
		protected StencilOperation passOp;
		protected bool twoSidedOperation;

		#endregion Fields

		#region Constructor

		public RSStencilOperation(bool stencilCheck, CompareFunction func, int refValue, int mask,
								  StencilOperation stencilFailOp, StencilOperation depthFailOp, StencilOperation passOp,
								  bool twoSidedOperation) {
			this.stencilCheck = stencilCheck;
			this.func = func;
			this.refValue = refValue;
			this.mask = mask;
			this.stencilFailOp = stencilFailOp;
			this.depthFailOp = depthFailOp;
			this.passOp = passOp;
			this.twoSidedOperation = twoSidedOperation;
		}
		
		#endregion Constructor

		#region Methods

		public override void Execute(SceneManager sm, RenderSystem rs) {
			rs.StencilCheckEnabled = stencilCheck;
			rs.SetStencilBufferParams(func, refValue, mask, stencilFailOp, 
									  depthFailOp, passOp, twoSidedOperation);
		}

		#endregion Methods

	}

	///<summary>
	///    "Render quad" RenderSystem operation
	///</summary>
	public class RSQuadOperation : CompositorRenderSystemOperation {

		#region Fields

		protected Material mat;
		protected Technique technique;
		protected CompositorInstance instance;
		protected uint pass_id;

		#endregion Fields

		#region Constructor

		public RSQuadOperation(CompositorInstance instance, uint pass_id, Material mat) {
			this.mat = mat;
			this.instance = instance;
			this.pass_id = pass_id;
			mat.Load();
			instance.FireNotifyMaterialSetup(pass_id, mat);
			technique = mat.GetTechnique(0);
			Debug.Assert(technique != null);
		}
		
		#endregion Constructor

		#region Methods

		public override void Execute(SceneManager sm, RenderSystem rs) {
			// Fire listener
			instance.FireNotifyMaterialRender(pass_id, mat);
			// Queue passes from mat
			for (int i=0; i<technique.NumPasses; i++) {
                Pass pass = technique.GetPass(i);
				sm.InjectRenderWithPass(pass,
										CompositorManager.Instance.GetTexturedRectangle2D(),
										false // don't allow replacement of shadow passes
										);
			}
		}

		#endregion Methods

	}

	///<summary>
	///    A pairing of int and CompositeRenderSystemOperation, needed because the collection 
	///    in CompositorTargetOperation must be ordered
	///</summary>
	public class QueueIDAndOperation {
		
		#region Fields

		protected RenderQueueGroupID queueID;
		protected CompositorRenderSystemOperation operation;

		#endregion Fields

		#region Constructor

		public QueueIDAndOperation(RenderQueueGroupID queueID, CompositorRenderSystemOperation operation) {
			this.queueID = queueID;
			this.operation = operation;
		}

		#endregion Constructor

		#region Properties

		public RenderQueueGroupID QueueID {
			get { return queueID; }
			set { queueID = value; }
		}

		public CompositorRenderSystemOperation Operation {
			get { return operation; }
			set { operation = value; }
		}

		#endregion Properties
	}

	///<summary>
	///    Operation setup for a RenderTarget (collected).
	///</summary>
    public class CompositorTargetOperation {
	
		#region Fields

		///<summary>
		///    Target
		///</summary>
		protected RenderTarget target;
		///<summary>
		///    Current group ID
		///</summary>
		protected RenderQueueGroupID currentQueueGroupID;
		///<summary>
		///    RenderSystem operations to queue into the scene manager
		///</summary>
		protected List<QueueIDAndOperation> renderSystemOperations;
		///<summary>
		///    Scene visibility mask
		///    If this is 0, the scene is not rendered at all
		///</summary>
		protected uint visibilityMask;
		///<summary>
		///    LOD offset. This is multiplied with the camera LOD offset
		///    1.0 is default, lower means lower detail, higher means higher detail
		///</summary>
		protected float lodBias;
		///<summary>
		///    A set of render queues to either include or exclude certain render queues.
		///</summary>
		protected BitArray renderQueues = new BitArray((int)RenderQueueGroupID.Count);
		///<summary>
		///    @see CompositionTargetPass::mOnlyInitial
		///</summary>
		protected bool onlyInitial;
		///<summary>
		///    "Has been rendered" flag; used in combination with
		///    onlyInitial to determine whether to skip this target operation.
		///</summary>
		protected bool hasBeenRendered;
		///<summary>
		///    Whether this op needs to find visible scene objects or not 
		///</summary>
		protected bool findVisibleObjects;
		///<summary>
		///    Which material scheme this op will use */
		///</summary>
		protected string materialScheme;

		#endregion Fields


		#region Constructors

		public CompositorTargetOperation(RenderTarget target) {
			this.target = target;
			currentQueueGroupID = 0;
            renderSystemOperations = new List<QueueIDAndOperation>();
            visibilityMask = 0xFFFFFFFF;
			lodBias = 1.0f;
			onlyInitial = false;
			hasBeenRendered = false;
			findVisibleObjects = false;
		}

		#endregion Constructors

		#region Properties

		public RenderTarget Target {
			get { return target; }
			set { target = value; }
		}
		
		public RenderQueueGroupID CurrentQueueGroupID {
			get { return currentQueueGroupID; }
			set { currentQueueGroupID = value; }
        }

		public List<QueueIDAndOperation> RenderSystemOperations {
			get { return renderSystemOperations; }
		}

		public uint VisibilityMask {
			get { return visibilityMask; }
			set { visibilityMask = value; }
		}

		public float LodBias {
			get { return lodBias; }
			set { lodBias = value; }
		}

		public BitArray RenderQueues {
			get { return renderQueues; }
			set { renderQueues = value; }
		}

		public bool OnlyInitial {
			get { return onlyInitial; }
			set { onlyInitial = value; }
		}

		public bool HasBeenRendered {
			get { return hasBeenRendered; }
			set { hasBeenRendered = value; }
		}

		public bool FindVisibleObjects {
			get { return findVisibleObjects; }
			set { findVisibleObjects = value; }
		}

		public string MaterialScheme {
			get { return materialScheme; }
			set { materialScheme = value; }
		}

		#endregion Properties

		#region Methods

		public bool RenderQueueBitTest(RenderQueueGroupID id) {
			return renderQueues[(int)id];
		}
		
		#endregion Methods
	}
	
}
