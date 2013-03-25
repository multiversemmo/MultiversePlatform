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
using System.IO;
using Axiom.Core;
using Axiom.Collections;
using Axiom.Media;
using Axiom.Utility;

namespace Axiom.Graphics {

	#region Delegate/EventArg Declarations

	/// <summary>
	///    Delegate for RenderTarget update events.
	/// </summary>
	public delegate void RenderTargetUpdateEventHandler(object sender, RenderTargetUpdateEventArgs e);

	/// <summary>
	///    Delegate for Viewport update events.
	/// </summary>
	public delegate void ViewportUpdateEventHandler(object sender, ViewportUpdateEventArgs e);

	/// <summary>
	///    Event arguments for render target updates.
	/// </summary>
	public class RenderTargetUpdateEventArgs : EventArgs {
	}

	/// <summary>
	///    Event arguments for viewport updates through while processing a RenderTarget.
	/// </summary>
	public class ViewportUpdateEventArgs : EventArgs {
		internal Viewport viewport;

		public Viewport Viewport {
			get {
				return viewport;
			}
		}
	}

	#endregion Delegate/EventArg Declarations

	/// <summary>
	///		A 'canvas' which can receive the results of a rendering operation.
	/// </summary>
	/// <remarks>
	///		This abstract class defines a common root to all targets of rendering operations. A
	///		render target could be a window on a screen, or another
	///		offscreen surface like a render texture.
	///	</remarks>
	public abstract class RenderTarget : IDisposable {
		#region Fields

		/// <summary>
		///    Height of this render target.
		/// </summary>
		protected int height;
		/// <summary>
		///    Width of this render target.
		/// </summary>
		protected int width;
        /// <summary>
        ///     Color depth of this render target.
        /// </summary>
		protected int colorDepth;
        /// <summary>
        ///     Is there a depth buffer for this render target?
        /// </summary>
        protected bool isDepthBuffered;
		/// <summary>
		///    Indicates the priority of this render target.  Higher priority targets will get processed first.
		/// </summary>
		protected RenderTargetPriority priority;
		/// <summary>
		///    Unique name assigned to this render target.
		/// </summary>
		protected string name;
		/// <summary>
		///    Optional debug text that can be display on this render target.  May not be relevant for all targets.
		/// </summary>
		protected string debugText;
		/// <summary>
		///    Number of faces rendered during the last update to this render target.
		/// </summary>
		protected int numFaces;
		/// <summary>
		///    Custom attributes that can be assigned to this target.
		/// </summary>
		protected Hashtable customAttributes = new Hashtable();
		/// <summary>
		///    Flag that states whether this target is active or not.
		/// </summary>
		protected bool isActive = true;
        /// <summary>
        ///     Is this render target updated automatically each frame?
        /// </summary>
		protected bool isAutoUpdated = true;
        /// <summary>
        ///    Collection of viewports attached to this render target.
        /// </summary>
        protected ViewportCollection viewportList;

		#endregion Fields

		#region Constructor

        /// <summary>
        ///     Default constructor.
        /// </summary>
		public RenderTarget() {
			this.viewportList = new ViewportCollection(this);

			numFaces = 0;
		}

		#endregion Constructor

		#region Event handling

		/// <summary>
		///    Gets fired before this RenderTarget is going to update.  Handling this event is ideal
		///    in situation, such as RenderTextures, where before rendering the scene to the texture,
		///    you would like to show/hide certain entities to avoid rendering more than was necessary
		///    to reduce processing time.
		/// </summary>
		public event RenderTargetUpdateEventHandler BeforeUpdate;

		/// <summary>
		///    Gets fired right after this RenderTarget has been updated each frame.  If the scene has been modified
		///    in the BeforeUpdate event (such as showing/hiding objects), this event can be handled to set everything 
		///    back to normal.
		/// </summary>
		public event RenderTargetUpdateEventHandler AfterUpdate;

		/// <summary>
		///    Gets fired before rendering the contents of each viewport attached to this RenderTarget.
		/// </summary>
		public event ViewportUpdateEventHandler BeforeViewportUpdate;

		/// <summary>
		///    Gets fired after rendering the contents of each viewport attached to this RenderTarget.
		/// </summary>
		public event ViewportUpdateEventHandler AfterViewportUpdate;

		protected virtual void OnBeforeUpdate() {
			if(BeforeUpdate != null) {
				BeforeUpdate(this, new RenderTargetUpdateEventArgs());
			}
		}

		protected virtual void OnAfterUpdate() {
			if(AfterUpdate != null) {
				AfterUpdate(this, new RenderTargetUpdateEventArgs());
			}
		}

		protected virtual void OnBeforeViewportUpdate(Viewport viewport) {
			if(BeforeViewportUpdate != null) {
				ViewportUpdateEventArgs e = new ViewportUpdateEventArgs();
				e.viewport = viewport;
				BeforeViewportUpdate(this, e);
			}
		}

		protected virtual void OnAfterViewportUpdate(Viewport viewport) {
			if(AfterViewportUpdate != null) {
				ViewportUpdateEventArgs e = new ViewportUpdateEventArgs();
				e.viewport = viewport;
				AfterViewportUpdate(this, e);
			}
		}

		#endregion

		#region Public properties

		/// <summary>
		///    Gets/Sets the name of this render target.
		/// </summary>
		public string Name {
			get { 
				return this.name; 
			}
			set { 
				this.name = value; 
			}
		}

		/// <summary>
		///    Gets/Sets whether this RenderTarget is active or not.  When inactive, it will be skipped
		///    during processing each frame.
		/// </summary>
		public virtual bool IsActive {
			get {
				return isActive;
			}
			set {
				isActive = value;
			}
		}


		/// <summary>
		///    Gets/Sets whether this target should be automatically updated if Axiom's rendering
		///    loop or Root.UpdateAllRenderTargets is being used.
		/// </summary>
		/// <remarks>
		///		By default, if you use Axiom's own rendering loop (Root.StartRendering)
		///		or call Root.UpdateAllRenderTargets, all render targets are updated
		///		automatically. This method allows you to control that behaviour, if 
		///		for example you have a render target which you only want to update periodically.
		/// </remarks>
		public virtual bool IsAutoUpdated {
			get {
				return isAutoUpdated;
			}
			set {
				isAutoUpdated = value;
			}
		}
		
		/// <summary>
		///    Gets the priority of this render target.  Higher priority targets will get processed first.
		/// </summary>
		public RenderTargetPriority Priority {
			get {
				return priority;
			}
		}

		/// <summary>
		/// Gets/Sets the debug text of this render target.
		/// </summary>
		public string DebugText {
			get {
				return this.debugText;
			}
			set {
				this.debugText = value;
			}
		}

		/// <summary>
		/// Gets/Sets the width of this render target.
		/// </summary>
		public int Width {
			get { 
				return this.width; 
			}
			set { 
				this.width = value; 
			}
		}

		/// <summary>
		/// Gets/Sets the height of this render target.
		/// </summary>
		public int Height {
			get { 
				return this.height; 
			}
			set { 
				this.height = value; 
			}
		}

		/// <summary>
		/// Gets/Sets the color depth of this render target.
		/// </summary>
		public int ColorDepth {
			get { 
				return this.colorDepth; 
			}
			set { 
				this.colorDepth = value; 
			}
		} 

		/// <summary>
		///     Gets the number of viewports attached to this render target.
		/// </summary>
		public int NumViewports {
			get {
				return viewportList.Count;
			}
		}

		/// <summary>
		///     Signals whether textures should be flipping before this target
		///     is updated.  Required for render textures in some API's.
		/// </summary>
		public virtual bool RequiresTextureFlipping {
			get {
				return false;
			}
		}

		#endregion

		#region Methods

		private static TimingMeter updateTargetMeter = MeterManager.GetMeter("Update Target", "Update Target");
		private static TimingMeter beforeUpdateMeter = MeterManager.GetMeter("Before Update", "Update Target");
		private static TimingMeter afterUpdateMeter = MeterManager.GetMeter("After Update", "Update Target");
		private static TimingMeter beforeViewPortUpdateMeter = MeterManager.GetMeter("Before Viewport Update", "Update Target");
		private static TimingMeter afterViewPortUpdateMeter = MeterManager.GetMeter("After Viewport Update", "Update Target");
		private static TimingMeter viewPortUpdateMeter = MeterManager.GetMeter("Viewport Update", "Update Target");
		/// <summary>
		///		Tells the target to update it's contents.
		/// </summary>
		/// <remarks>
		///		If the engine is not running in an automatic rendering loop
		///		(started using RenderSystem.StartRendering()),
		///		the user of the library is responsible for asking each render
		///		target to refresh. This is the method used to do this. It automatically
		///		re-renders the contents of the target using whatever cameras are assigned to render to it (using Camera.RenderTarget).
		///	
		///		This allows the engine to be used in multi-windowed utilities
		///		and for contents to be refreshed only when required, rather than
		///		constantly as with the automatic rendering loop.
		///	</remarks>
		public virtual void Update() {
			updateTargetMeter.Enter();
			numFaces = 0;

			// notify event handlers that this RenderTarget is about to be updated
			beforeUpdateMeter.Enter();
			OnBeforeUpdate();
			beforeUpdateMeter.Exit();

			// Go through viewportList in Z-order
			// Tell each to refresh
			for(int i = 0; i < viewportList.Count; i++) {
				Viewport viewport = viewportList[i];

				// notify listeners (pre)
				beforeViewPortUpdateMeter.Enter();
				OnBeforeViewportUpdate(viewport);                
				beforeViewPortUpdateMeter.Exit();

				viewPortUpdateMeter.Enter();
				viewportList[i].Update();
				viewPortUpdateMeter.Exit();
				numFaces += viewportList[i].Camera.RenderedFaceCount;

				// notify event handlers the the viewport is updated
				afterViewPortUpdateMeter.Enter();
				OnAfterViewportUpdate(viewport);
				afterViewPortUpdateMeter.Exit();
			}

			// notify event handlers that this target update is complete
			afterUpdateMeter.Enter();
			OnAfterUpdate();
			afterUpdateMeter.Exit();

			updateTargetMeter.Exit();
		}

		/// <summary>
		///		Adds a viewport to the rendering target.
		/// </summary>
		/// <remarks>
		///		A viewport is the rectangle into which rendering output is sent. This method adds
		///		a viewport to the render target, rendering from the supplied camera. The
		///		rest of the parameters are only required if you wish to add more than one viewport
		///		to a single rendering target. Note that size information passed to this method is
		///		passed as a parametric, i.e. it is relative rather than absolute. This is to allow
		///		viewports to automatically resize along with the target.
		/// </remarks>
		/// <param name="camera">The camera from which the viewport contents will be rendered (mandatory)</param>
		/// <param name="left">The relative position of the left of the viewport on the target, as a value between 0 and 1.</param>
		/// <param name="top">The relative position of the top of the viewport on the target, as a value between 0 and 1.</param>
		/// <param name="width">The relative width of the viewport on the target, as a value between 0 and 1.</param>
		/// <param name="height">The relative height of the viewport on the target, as a value between 0 and 1.</param>
		/// <param name="zOrder">The relative order of the viewport with others on the target (allows overlapping
		///		viewports i.e. picture-in-picture). Higher ZOrders are on top of lower ones. The actual number
		///		is irrelevant, only the relative ZOrder matters (you can leave gaps in the numbering)</param>
		/// <returns></returns>
		public virtual Viewport AddViewport(Camera camera, float left, float top, float width, float height, int zOrder) {
			// create a new camera and add it to our internal collection
			Viewport viewport = new Viewport(camera, this, left, top, width, height, zOrder);
			this.viewportList.Add(viewport);

			return viewport;
		}

		public virtual void RemoveViewport(Viewport viewport) 
		{
			viewportList.Remove(viewport);
		}
		
		public virtual void RemoveViewport(int index) 
		{
			viewportList.RemoveAt(index);
		}

        /// <summary>
        ///     Adds a viewport to the rendering target.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
		public Viewport AddViewport(Camera camera) {
			return AddViewport(camera, 0.0f, 0.0f, 1.0f, 1.0f, 0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Viewport GetViewport(int index) {
			Debug.Assert(index >= 0 && index < viewportList.Count);

			return viewportList[index];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public virtual object GetCustomAttribute(string attribute) {
			Debug.Assert(customAttributes.ContainsKey(attribute));

			return customAttributes[attribute];
		}

		/// <summary>
		///		Utility method to notify a render target that a camera has been removed, 
		///		incase it was referring to it as a viewer.
		/// </summary>
		/// <param name="camera"></param>
		internal void NotifyCameraRemoved(Camera camera) {
			for(int i = 0; i < viewportList.Count; i++) {
				Viewport viewport = viewportList[i];

				// remove the link to this camera
				if(viewport.Camera == camera) {
					viewport.Camera = null;
				}
			}
		}

        public virtual void ResetStatistics() {
            // TODO: Implement RenderTarget.ResetStatistics
        }

		/// <summary>
		///		Saves window contents to file (i.e. screenshot);
		/// </summary>
        public void Save(string fileName)
        {
            Save(fileName, PixelFormat.BYTE_RGB);
        }

        public void Save(string fileName, PixelFormat requestedFormat)
        {
            // create a memory stream, setting the initial capacity
            MemoryStream bufferStream = new MemoryStream(width * height * 3);

            // save the data to the memory stream
            Save(bufferStream, requestedFormat);

            int pos = fileName.LastIndexOf('.');

            // grab the file extension
            string extension = fileName.Substring(pos + 1);

            // grab the codec for the requested file extension
            ICodec codec = CodecManager.Instance.GetCodec(extension);

            // setup the image file information
            ImageCodec.ImageData imageData = new ImageCodec.ImageData();
            imageData.width = width;
            imageData.height = height;
            imageData.format = requestedFormat;

            // reset the stream position
            bufferStream.Position = 0;

            // finally, save to file as an image
            codec.EncodeToFile(bufferStream, fileName, imageData);

            bufferStream.Close();
        }

		/// <summary>
		///		Saves the contents of this render target to the specified stream.
		/// </summary>
		/// <param name="stream">Stream to write the contents of this render target to.</param>
        /// <param name="requestedFormat">Allows caller to request a format other than BYTE_RGB</param>
		public abstract void Save(Stream stream, PixelFormat requestedFormat);

		#endregion Methods

        #region IDisposable Members

        /// <summary>
        ///     Called when a render target is being destroyed.
        /// </summary>
        public virtual void Dispose() {
            // TODO: Track stats per render target and report on shutdown
            // Write final performance stats
            //LogManager.Instance.Write("Final Stats:");
            //LogManager.Instance.Write("Axiom Framerate Average FPS: " + averageFPS.ToString("0.000000") + " Best FPS: " + highestFPS.ToString("0.000000") + " Worst FPS: " + lowestFPS.ToString("0.000000"));
        }

#endregion
    }
}
