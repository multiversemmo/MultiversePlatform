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
	///    Chain of compositor effects applying to one viewport.
	///</summary>
	public class CompositorChain {

        public class RQListener {
            ///<summary>
            /// Fields that are treated as temps by queue started/ended events
            ///</summary>
            CompositorTargetOperation operation;
            ///<summary>
            ///    The scene manager instance
            ///</summary>
            SceneManager sceneManager;
            ///<summary>
            ///    The render system
            ///</summary>
            RenderSystem renderSystem;
            ///<summary>
            ///    The view port
            ///</summary>
            Viewport viewport;
            ///<summary>
            ///    The number of the first render system op to be processed by the event
            ///</summary>
            int currentOp;
            ///<summary>
            ///    The number of the last render system op to be processed by the event
            ///</summary>
            int lastOp;

            ///<summary>
            ///    Set current operation and target */
            ///</summary>
            public void SetOperation(CompositorTargetOperation op, SceneManager sm, RenderSystem rs) {
                operation = op;
                sceneManager = sm;
                renderSystem = rs;
                currentOp = 0;
                lastOp = op.RenderSystemOperations.Count;
            }

            ///<summary>
            ///    Notify current destination viewport
            ///</summary>
            public void NotifyViewport(Viewport vp) {
                viewport = vp;
            }

            ///<summary>
            ///    @copydoc RenderQueueListener::renderQueueStarted
            ///</summary>
            public bool OnRenderQueueStarted(RenderQueueGroupID id) {
                // Skip when not matching viewport
                // shadows update is nested within main viewport update
                if (sceneManager.CurrentViewport != viewport)
                    return false;

                FlushUpTo(id);
                /// If noone wants to render this queue, skip it
                /// Don't skip the OVERLAY queue because that's handled seperately
                if (!operation.RenderQueueBitTest(id) && id != RenderQueueGroupID.Overlay)
                    return true;
                return false;
            }

            public bool OnRenderQueueEnded(RenderQueueGroupID id) {
                return false;
            }

            ///<summary>
            ///    Flush remaining render system operations
            ///</summary>
            public void FlushUpTo(RenderQueueGroupID id) {
                /// Process all RenderSystemOperations up to and including render queue id.
                /// Including, because the operations for RenderQueueGroup x should be executed
                /// at the beginning of the RenderQueueGroup render for x.
                while (currentOp != lastOp &&
                       ((int)operation.RenderSystemOperations[currentOp].QueueID < (int)id)) {
                    operation.RenderSystemOperations[currentOp].Operation.Execute(sceneManager, renderSystem);
                    currentOp++;
                }
            }
        }

		#region Fields

        ///<summary>
        ///    Viewport affected by this CompositorChain
		///</summary>
        protected Viewport viewport;
        ///<summary>
        ///    Plainly renders the scene; implicit first compositor in the chain.
		///</summary>
        protected CompositorInstance originalScene;
        ///<summary>
        ///    Postfilter instances in this chain
		///</summary>
        protected List<CompositorInstance> instances;
        ///<summary>
        ///    State needs recompile
		///</summary>
        protected bool dirty;
        ///<summary>
        ///    Any compositors enabled?
		///</summary>
		protected bool anyCompositorsEnabled;
        ///<summary>
        ///    Compiled state (updated with _compile)
		///</summary>
        protected List<CompositorTargetOperation> compiledState;
        protected CompositorTargetOperation outputOperation;
        /// <summary>
        ///    Render System operations queued by last compile, these are created by this
        ///    instance thus managed and deleted by it. The list is cleared with 
        ///    ClearCompilationState()
        /// </summary>
        protected List<CompositorRenderSystemOperation> renderSystemOperations;
        ///<summary>
        ///    Old viewport settings
		///</summary>
		protected FrameBuffer oldClearEveryFrameBuffers;
        ///<summary>
        ///    Store old scene visibility mask
		///</summary>
		protected uint oldVisibilityMask;
        ///<summary>
        ///    Store old find visible objects
		///</summary>
		protected bool oldFindVisibleObjects;
        ///<summary>
        ///    Store old camera LOD bias
		///</summary>
        protected float oldLodBias;     
        ///<summary>
        ///    Store old viewport material scheme
		///</summary>
		protected string oldMaterialScheme;
        /// <summary>
        ///   The class that will handle the callbacks from the RenderQueue
        /// </summary>
        protected RQListener listener;

        ///<summary>
        ///    Identifier for "last" compositor in chain
		///</summary>
        protected static int lastCompositor = int.MaxValue;
        ///<summary>
        ///    Identifier for best technique
		///</summary>
		protected static int bestCompositor = 0;

		#endregion Fields


		#region Constructor

		public CompositorChain(Viewport vp) {
			this.viewport = vp;
			originalScene = null;
			instances = new List<CompositorInstance>();
			dirty = true;
			anyCompositorsEnabled = false;
			compiledState = new List<CompositorTargetOperation>();
			outputOperation = null;
			oldClearEveryFrameBuffers = viewport.ClearBuffers;
            renderSystemOperations = new List<CompositorRenderSystemOperation>();
            listener = new RQListener();
			Debug.Assert(viewport != null);
		}

        #endregion Constructor


        #region Properties

		public bool Dirty {
			get { return dirty; }
			set { dirty = value; }
		}
				
		public Viewport Viewport {
			get { return viewport; }
		}
				
		public CompositorInstance OriginalScene {
			get { return originalScene; }
		}
				
		public List<CompositorInstance> Instances {
			get { return instances; }
		}
				
		public static int LastCompositor {
			get { return lastCompositor; }
		}
				
		public static int BestCompositor {
			get { return BestCompositor; }
		}

        internal List<CompositorRenderSystemOperation> RenderSystemOperations {
            get {
                return renderSystemOperations;
            }
        }
				
		#endregion Properties


		#region Methods

        ///<summary>
        ///    destroy internal resources
		///</summary>
		protected void DestroyResources() {
            ClearCompiledState();

			if (viewport != null) {
				RemoveAllCompositors();
                viewport.Target.BeforeUpdate -= BeforeRenderTargetUpdate;
                // viewport.Target.AfterUpdate -= AfterRenderTargetUpdate;
				viewport.Target.BeforeViewportUpdate -= BeforeViewportUpdate;
				viewport.Target.AfterViewportUpdate -= AfterViewportUpdate;
				/// Destroy "original scene" compositor instance
				originalScene.Technique.DestroyInstance(originalScene);
				viewport = null;
			}
		}
			
        ///<summary>
        ///    Apply a compositor. Initially, the filter is enabled.
		///</summary>
		///<param name="filter">Filter to apply</param>
		///<param name="addPosition">Position in filter chain to insert this filter at; defaults to the end (last applied filter)</param>
		///<param name="technique">Technique to use; CompositorChain::BEST (default) chooses to the best one 
		///                        available (first technique supported)
		///</param>
		CompositorInstance AddCompositor(Compositor filter, int addPosition, int technique) {
			// Init on demand
			if (originalScene == null) {
                viewport.Target.BeforeUpdate += BeforeRenderTargetUpdate;
                // viewport.Target.AfterUpdate += AfterRenderTargetUpdate;
                viewport.Target.BeforeViewportUpdate += BeforeViewportUpdate;
				viewport.Target.AfterViewportUpdate += AfterViewportUpdate;
				/// Create base "original scene" compositor
				Compositor baseCompositor = (Compositor)CompositorManager.Instance.LoadExisting("Ogre/Scene");
				originalScene = baseCompositor.GetSupportedTechnique(0).CreateInstance(this);
			}


			filter.Touch();
			if (technique >= filter.SupportedTechniques.Count) {
				/// Warn user
				LogManager.Instance.Write("CompositorChain: Compositor " + filter.Name + " has no supported techniques.");
				return null;
			}
			CompositionTechnique tech = filter.GetSupportedTechnique(technique);
			CompositorInstance t = tech.CreateInstance(this);

			if (addPosition == lastCompositor)
				instances.Add(t);
			else {
				Debug.Assert(addPosition <= instances.Count);
				instances.Insert(addPosition, t);
			}
			
			dirty = true;
			anyCompositorsEnabled = true;
			return t;
		}
			
		public CompositorInstance AddCompositor(Compositor filter) {
			return AddCompositor(filter, lastCompositor, bestCompositor);
		}
		
		public CompositorInstance AddCompositor(Compositor filter, int addPosition) {
			return AddCompositor(filter, addPosition, bestCompositor);
		}
		

        ///<summary>
        ///    Remove a compositor.
		///</summary>
		///<param name="position">Position in filter chain of filter to remove</param>
        public void RemoveCompositor(int position) {
			CompositorInstance instance = instances[position];
			instances.RemoveAt(position);
			instance.Technique.DestroyInstance(instance);
			dirty = true;
		}

        public void RemoveCompositor() {
			RemoveCompositor(lastCompositor);
		}
		
        ///<summary>
        ///    Remove all compositors.
		///</summary>
		public void RemoveAllCompositors() {
			foreach (CompositorInstance instance in instances)
				instance.Technique.DestroyInstance(instance);
			instances.Clear();
			dirty = true;
		}

        ///<summary>
        ///    Remove a compositor by pointer. This is internally used by CompositionTechnique to
        ///    "weak" remove any instanced of a deleted technique.
		///</summary>
		public void RemoveInstance(CompositorInstance instance) {
			instances.Remove(instance);
			instance.Technique.DestroyInstance(instance);
		}

        ///<summary>
        ///    Get compositor instance by position.
		///</summary>
		public CompositorInstance GetCompositor(int index) {
			return instances[index];
		}

        ///<summary>
        ///    Enable or disable a compositor, by position. Disabling a compositor stops it from rendering
        ///    but does not free any resources. This can be more efficient than using removeCompositor and 
        ///    addCompositor in cases the filter is switched on and off a lot.
		///</summary>
		///<param name="position">Position in filter chain of filter</param>
        public void SetCompositorEnabled(int position, bool state) {
			GetCompositor(position).Enabled = state;
		}
    
        ///<summary>
        ///    @see RenderTargetListener.PreRenderTargetUpdate
		///</summary>
		public void BeforeRenderTargetUpdate(object sender, RenderTargetUpdateEventArgs evt) {
			/// Compile if state is dirty
			if(dirty)
				Compile();

			// Do nothing if no compositors enabled
			if (!anyCompositorsEnabled)
				return;

			/// Update dependent render targets; this is done in the preRenderTarget 
			/// and not the preViewportUpdate for a reason: at this time, the
			/// target Rendertarget will not yet have been set as current. 
			/// ( RenderSystem::setViewport(...) ) if it would have been, the rendering
			/// order would be screwed up and problems would arise with copying rendertextures.
			Camera cam = viewport.Camera;
			/// Iterate over compiled state
			foreach (CompositorTargetOperation op in compiledState) {
				/// Skip if this is a target that should only be initialised initially
				if(op.OnlyInitial && op.HasBeenRendered)
					continue;
				op.HasBeenRendered = true;
				/// Setup and render
				PreTargetOperation(op, op.Target.GetViewport(0), cam);
				op.Target.Update();
				PostTargetOperation(op, op.Target.GetViewport(0), cam);
			}
		}

        ///<summary>
        ///    @see RenderTargetListener.PreViewportUpdate
		///</summary>
        public virtual void BeforeViewportUpdate(object sender, ViewportUpdateEventArgs evt) {
			// Only set up if there is at least one compositor enabled, and it's this viewport
			if(evt.Viewport != viewport || !anyCompositorsEnabled)
				return;

			// set original scene details from viewport
			CompositionPass pass = originalScene.Technique.OutputTarget.GetPass(0);
			if (pass.ClearBuffers != viewport.ClearBuffers ||
				pass.ClearColor != viewport.BackgroundColor) {
				pass.ClearBuffers = viewport.ClearBuffers;
				pass.ClearColor = viewport.BackgroundColor;
				Compile();
			}

			/// Prepare for output operation
			PreTargetOperation(outputOperation, viewport, viewport.Camera);
		}

        ///<summary>
        ///    Prepare a viewport, the camera and the scene for a rendering operation
		///</summary>
        protected void PreTargetOperation(CompositorTargetOperation op, Viewport vp, Camera cam) {
			SceneManager sm = cam.SceneManager;
			/// Set up render target listener
			listener.SetOperation(op, sm, sm.TargetRenderSystem);
            listener.NotifyViewport(vp);
			/// Register it
            sm.QueueStarted += listener.OnRenderQueueStarted;
            sm.QueueEnded += listener.OnRenderQueueEnded;
            /// Set visiblity mask
			oldVisibilityMask = sm.VisibilityMask;
			sm.VisibilityMask = op.VisibilityMask;
			/// Set whether we find visibles
			oldFindVisibleObjects = sm.FindVisibleObjectsBool;
			sm.FindVisibleObjectsBool = op.FindVisibleObjects;
			/// Set LOD bias level
			oldLodBias = cam.LodBias;
			cam.LodBias = cam.LodBias * op.LodBias;
			/// Set material scheme 
			oldMaterialScheme = vp.MaterialScheme;
			vp.MaterialScheme = op.MaterialScheme;
			/// XXX TODO
			//vp->setClearEveryFrame( true );
			//vp->setOverlaysEnabled( false );
			//vp->setBackgroundColour( op.clearColour );
		}
        

        ///<summary>
		///    Notify current destination viewport
		///</summary>
		public void NotifyViewport(Viewport vp) {
			viewport = vp; 
		}

        ///<summary>
        ///    Restore a viewport, the camera and the scene after a rendering operation
		///</summary>
        protected void PostTargetOperation(CompositorTargetOperation op, Viewport vp, Camera cam) {
			SceneManager sm = cam.SceneManager;
			/// Unregister our listener
            sm.QueueStarted -= listener.OnRenderQueueStarted;
            sm.QueueEnded -= listener.OnRenderQueueEnded;
            /// Flush remaing operations
			listener.FlushUpTo(RenderQueueGroupID.Count);
			/// Restore default scene and camera settings
			sm.VisibilityMask = oldVisibilityMask;
			sm.FindVisibleObjectsBool = oldFindVisibleObjects;
			cam.LodBias = oldLodBias;
			vp.MaterialScheme = oldMaterialScheme;
		}

        ///<summary>
        ///    @see RenderTargetListener.PostViewportUpdate
		///</summary>
        public virtual void AfterViewportUpdate(object sender, ViewportUpdateEventArgs evt) {
			// Only tidy up if there is at least one compositor enabled, and it's this viewport
			if(evt.Viewport != viewport || !anyCompositorsEnabled)
				return;

			PostTargetOperation(outputOperation, viewport, viewport.Camera);
		}

        ///<summary>
        ///    @see RenderTargetListener.ViewportRemoved
		///</summary>
		public virtual void OnViewportRemoved(object sender, ViewportUpdateEventArgs evt) {
			// this chain is now orphaned
			// can't delete it since held from outside, but release all resources being used
			DestroyResources();
		}
        
        ///<summary>
        ///    Compile this Composition chain into a series of RenderTarget operations.
		///</summary>
		protected void Compile() {
            ClearCompiledState();

			bool compositorsEnabled = false;

			/// Set previous CompositorInstance for each compositor in the list
			CompositorInstance lastComposition = originalScene;
			originalScene.PreviousInstance = null;
            CompositionPass pass = originalScene.Technique.OutputTarget.GetPass(0);
            pass.ClearBuffers = viewport.ClearBuffers;
            pass.ClearColor = viewport.BackgroundColor;
			foreach (CompositorInstance instance in instances) {
				if (instance.Enabled) {
					compositorsEnabled = true;
					instance.PreviousInstance = lastComposition;
					lastComposition = instance;
				}
			}

			/// Compile misc targets
			lastComposition.CompileTargetOperations(compiledState);

			/// Final target viewport (0)
            outputOperation.RenderSystemOperations.Clear();
            lastComposition.CompileOutputOperation(outputOperation);

			// Deal with viewport settings
			if (compositorsEnabled != anyCompositorsEnabled) {
				anyCompositorsEnabled = compositorsEnabled;
				if (anyCompositorsEnabled) {
					// Save old viewport clearing options
					oldClearEveryFrameBuffers = viewport.ClearBuffers;
					// Don't clear anything every frame since we have our own clear ops
					viewport.SetClearEveryFrame(false);
				} else {
					// Reset clearing options
					viewport.SetClearEveryFrame(oldClearEveryFrameBuffers > 0, 
												oldClearEveryFrameBuffers);
				}
			}
			dirty = false;
		}

        protected void ClearCompiledState() {
            renderSystemOperations.Clear();
            compiledState.Clear();
            outputOperation = new CompositorTargetOperation(null);
        }


		
        #endregion Methods
		
    }

}

