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
using System.Diagnostics;
using Axiom.Graphics;

namespace Axiom.Core {
	/// <summary>
	///		Summary description for Viewport.
	///		An abstraction of a viewport, i.e. a rendering region on a render
	///		target.
	///	</summary>
	///	<remarks>
	///		A viewport is the meeting of a camera and a rendering surface -
	///		the camera renders the scene from a viewpoint, and places its
	///		results into some subset of a rendering target, which may be the
	///		whole surface or just a part of the surface. Each viewport has a
	///		single camera as source and a single target as destination. A
	///		camera only has 1 viewport, but a render target may have several.
	///		A viewport also has a Z-order, i.e. if there is more than one
	///		viewport on a single render target and they overlap, one must
	///		obscure the other in some predetermined way.
	///	</remarks>
	public sealed class Viewport {
		#region Fields
		
		/// <summary>
		///		Camera that this viewport is attached to.
		/// </summary>
		private Camera camera;
		/// <summary>
		///		Render target that is using this viewport.
		/// </summary>
		private RenderTarget target;
		/// <summary>
		///		Relative left [0.0, 1.0].
		/// </summary>
		private float relativeLeft;
		/// <summary>
		///		Relative top [0.0, 1.0].
		/// </summary>
		private float relativeTop;
		/// <summary>
		///		Relative width [0.0, 1.0].
		/// </summary>
		private float relativeWidth;
		/// <summary>
		///		Relative height [0.0, 1.0].
		/// </summary>
		private float relativeHeight;
		/// <summary>
		///		Absolute left edge of the viewport (in pixels).
		/// </summary>
		private int actualLeft;
		/// <summary>
		///		Absolute top edge of the viewport (in pixels).
		/// </summary>
		private int actualTop;
		/// <summary>
		///		Absolute width of the viewport (in pixels).
		/// </summary>
		private int actualWidth;
		/// <summary>
		///		Absolute height of the viewport (in pixels).
		/// </summary>
		private int actualHeight;
		/// <summary>
		///		Depth order of the viewport, for sorting.
		/// </summary>
		private int zOrder;
		/// <summary>
		///		Background color of the viewport.
		/// </summary>
		private ColorEx backColor;
		/// <summary>
		///		Should this viewport be cleared very frame?
		/// </summary>
		private bool clearEveryFrame;
        /// <summary>
        ///		A bitvector giving the buffers to be cleared
        /// </summary>
        private FrameBuffer clearBuffers;
		/// <summary>
		///		Has this viewport been updated?
		/// </summary>
		private bool isUpdated;
		/// <summary>
		///		Should we show overlays on this viewport?
		/// </summary>
		private bool showOverlays;
		
		/// <summary>
		///		Should we show sky boxes on this viewport?
		/// </summary>
		private bool showSkies;
		
		/// <summary>
		///		Should we show shadows on this viewport?
		/// </summary>
		private bool showShadows;

        /// <summary>
        ///		Material scheme
        /// </summary>
        private string materialSchemeName;

		#endregion Fields

		#region Constructor

		/// <summary>
		///		The constructor. Dimensions of the viewport are expressed as a pecentage between
		///		0 and 100. This allows the dimensions to apply irrespective of
		///		changes in the target's size: e.g. to fill the whole area,
		///		values of 0,0,100,100 are appropriate.
		/// </summary>
		/// <param name="camera">Reference to the camera to be the source for the image.</param>
		/// <param name="target">Reference to the render target to be the destination for the rendering.</param>
		/// <param name="left">Left</param>
		/// <param name="top">Top</param>
		/// <param name="width">Width</param>
		/// <param name="height">Height</param>
		/// <param name="zOrder">Relative Z-order on the target. Lower = further to the front.</param>
		public Viewport(Camera camera, RenderTarget target, float left, float top, float width, float height, int zOrder) {
			Debug.Assert(camera != null, "Cannot use a null Camera to create a viewport.");
			Debug.Assert(target != null, "Cannor use a null RenderTarget to create a viewport.");

            LogManager.Instance.Write("Creating viewport rendering from camera '{0}', relative dimensions L:{1},T:{2},W:{3},H:{4}, Z-Order:{5}",
                camera.Name, left, top, width, height, zOrder);

            this.camera = camera;
			this.target = target;
			this.zOrder = zOrder;

			relativeLeft = left;
			relativeTop = top;
			relativeWidth = width;
			relativeHeight = height;

			backColor = ColorEx.Black;
			clearEveryFrame = true;
            clearBuffers = FrameBuffer.Color | FrameBuffer.Depth;

			// Calculate actual dimensions
			UpdateDimensions();

			isUpdated = true;
			showOverlays = true;
			showSkies = true;
			showShadows = true;

			materialSchemeName = MaterialManager.DefaultSchemeName;
            
            // notify camera
			camera.NotifyViewport(this);
		}

		#endregion

		#region Internal engine methods

		/// <summary>
		///		Notifies the viewport of a possible change in dimensions.
		/// </summary>
		///	<remarks>
		///		Used by the target to update the viewport's dimensions
		///		(usually the result of a change in target size).
		///	</remarks>
		public void UpdateDimensions() {
			float height = (float)target.Height;
			float width = (float)target.Width;

			actualLeft = (int)(relativeLeft * width);
			actualTop = (int)(relativeTop * height);
			actualWidth = (int)(relativeWidth * width);
			actualHeight = (int)(relativeHeight * height);

			// This will check if  the cameras getAutoAspectRation() property is set.
			// If it's true its aspect ratio is fit to the current viewport
			// If it's false the camera remains unchanged.
			// This allows cameras to be used to render to many viewports,
			// which can have their own dimensions and aspect ratios.
			if (camera.AutoAspectRatio) {
				camera.AspectRatio = actualWidth / actualHeight;
			}

            LogManager.Instance.Write("Viewport for camera '{0}' - actual dimensions L:{1},T:{2},W:{3},H:{4}",
                camera.Name, actualLeft, actualTop, actualWidth, actualHeight);

            isUpdated = true;
		}

		#endregion

		#region Public properties

		/// <summary>
		///		Retrieves a reference to the render target for this viewport.
		/// </summary>
		public RenderTarget Target {
			get { 
				return target; 
			}
			set { 
				target = value; 
			}
		}

		/// <summary>
		///		Retrieves a reference to the camera for this viewport.
		/// </summary>
		public Camera Camera {
			get { 
				return camera; 
			}
			set { 
				camera = value; 
			}
		}

		/// <summary>
		///		Gets/Sets the background color which will be used to clear the screen every frame.
		/// </summary>
		public ColorEx BackgroundColor {
			get { 
				return backColor; 
			}
			set { 
				backColor = value; 
			}
		}
		/// <summary>
		///		Gets the relative top edge of the viewport, a value between 0.0 and 1.0.
		/// </summary>
		public float Top {
			get { 
				return relativeTop; 
			}
		}

		/// <summary>
		///		Gets the relative left edge of the viewport, a value between 0.0 and 1.0.
		/// </summary>
		public float Left {
			get { 
				return relativeLeft; 
			}
		}

		/// <summary>
		///		Gets the relative width of the viewport, a value between 0.0 and 1.0.
		/// </summary>
		public float Width {
			get { 
				return relativeWidth; 
			}
		}

		/// <summary>
		///		Gets the relative height of the viewport, a value between 0.0 and 1.0.
		/// </summary>
		public float Height {
			get { 
				return relativeHeight; 
			}
		}

		/// <summary>
		///		Gets the ZOrder of this viewport.
		/// </summary>
		public int ZOrder {
			get { 
				return zOrder; 
			}
		}

		/// <summary>
		///		Gets the actual top edge of the viewport, a value in pixels.
		/// </summary>
		public int ActualTop {
			get { 
				return actualTop; 
			}
		}

		/// <summary>
		///		Gets the actual left edge of the viewport, a value in pixels.
		/// </summary>
		public int ActualLeft {
			get { 
				return actualLeft; 
			}
		}

		/// <summary>
		///		Gets the actual width of the viewport, a value in pixels.
		/// </summary>
		public int ActualWidth {
			get { 
				return actualWidth; 
			}
		}

		/// <summary>
		///		Gets the actual height of the viewport, a value in pixels.
		/// </summary>
		public int ActualHeight {
			get { 
				return actualHeight; 
			}
		}

		/// <summary>
		///		Determines whether to clear the viewport before rendering.
		/// </summary>
		/// <remarks>
		///		If you expecting every pixel on the viewport to be redrawn
		///		every frame, you can save a little time by not clearing the
		///		viewport before every frame. Do so by passing 'false' to this
		///		method (the default is to clear every frame).
		///	</remarks>
		public bool ClearEveryFrame {
			get { 
				return clearEveryFrame; 
			}
			set { 
				SetClearEveryFrame(value); 
			}
		}

        /// <summary>
        ///		A bitvector giving the buffers to be cleared
        /// </summary>
        public FrameBuffer ClearBuffers {
            get {
                return clearBuffers;
            }
            set {
                clearBuffers = value;
            }
        }

		/// <summary>
		///		Tells this viewport whether it should display Overlay objects.
		///	</summary>
		///	<remarks>
		///		Overlay objects are layers which appear on top of the scene. They are created via
		///		SceneManager.CreateOverlay and every viewport displays these by default.
		///		However, you probably don't want this if you're using multiple viewports,
		///		because one of them is probably a picture-in-picture which is not supposed to
		///		have overlays of it's own. In this case you can turn off overlays on this viewport
		///		by calling this method.
		public bool OverlaysEnabled {
			get { 
				return showOverlays; 
			}
			set { 
				showOverlays = value; 
			}
		}

		/// <summary>
		///		Tells this viewport whether it should display sky boxes.
		///	</summary>
		public bool SkiesEnabled {
			get { 
				return showSkies; 
			}
			set { 
				showSkies = value; 
			}
		}

		/// <summary>
		///		Tells this viewport whether it should display shadows.
		///	</summary>
		public bool ShadowsEnabled {
			get { 
				return showShadows; 
			}
			set { 
				showShadows = value; 
			}
		}

		/// <summary>
		///		Returns the number of faces rendered to this viewport during the last frame.
		/// </summary>
		public int RenderedFaceCount {
			get { 
				return camera.RenderedFaceCount; 
			}
		}

		/// <summary>
		///		Gets/Sets the IsUpdated value.
		/// </summary>
		public bool IsUpdated {
			get { 
				return isUpdated; 
			}
			set { 
				isUpdated = value; 
			}
		}

        /// <summary>
        ///		Gets/Sets the material scheme name.
        /// </summary>
        public string MaterialScheme {
            get {
                return materialSchemeName;
            }
            set {
                materialSchemeName = value;
            }
        }

		#endregion

		#region Public methods

        /// <summary>
        ///		Determines whether to clear the viewport before rendering.
        /// </summary>
        ///<remarks>
        ///    You can use this method to set which buffers are cleared
        ///    (if any) before rendering every frame.
        ///</remarks>
        ///<param name="clear">Whether or not to clear any buffers</param>
        ///<param name="buffers">One or more values from FrameBuffer denoting
        ///    which buffers to clear, if clear is set to true. Note you should
        ///    not clear the stencil buffer here unless you know what you're doing.
        ///</param>
        public void SetClearEveryFrame(bool clear, FrameBuffer buffers) {
            clearEveryFrame = clear;
            clearBuffers = buffers;
        }

        public void SetClearEveryFrame(bool clear) {
            clearEveryFrame = clear;
            clearBuffers = FrameBuffer.Color | FrameBuffer.Depth;
        }

		/// <summary>
		///		Instructs the viewport to updates its contents from the viewpoint of
		///		the current camera.
		/// </summary>
		public void Update() {
			if(camera != null) {
				camera.RenderScene(this, showOverlays);
			}
		}

		/// <summary>
		///		Allows setting the dimensions of the viewport (after creation).
		/// </summary>
		/// <remarks>
		///		Dimensions relative to the size of the target,
		///		represented as real values between 0 and 1. i.e. the full
		///		target area is 0, 0, 1, 1.
		/// </remarks>
		/// <param name="left">Left edge of the viewport ([0.0, 1.0]).</param>
		/// <param name="top">Top edge of the viewport ([0.0, 1.0]).</param>
		/// <param name="width">Width of the viewport ([0.0, 1.0]).</param>
		/// <param name="height">Height of the viewport ([0.0, 1.0]).</param>
		public void SetDimensions(float left, float top, float width, float height) {
			relativeLeft = left;
			relativeTop = top;
			relativeWidth = width;
			relativeHeight = height;
			
			UpdateDimensions();
		}

		/// <summary>
		///		Access to actual dimensions (based on target size).
		/// </summary>
		/// <param name="left">Left edge of the viewport (in pixels).</param>
		/// <param name="top">Top edge of the viewport (in pixels).</param>
		/// <param name="width">Width of the viewport (in pixels).</param>
		/// <param name="height">Height of the viewport (in pixels).</param>
		public void GetActualDimensions(out int left, out int top, out int width, out int height) {
			left = actualLeft;
			top = actualTop;
			width = actualWidth;
			height = actualHeight;
		}

		#endregion
	}
}
