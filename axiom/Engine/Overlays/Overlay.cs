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
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;

namespace Axiom.Overlays {
    /// <summary>
    ///    Represents a layer which is rendered on top of the 'normal' scene contents.
    /// </summary>
    /// <remarks>
    ///    An overlay is a container for visual components (2D and 3D) which will be 
    ///    rendered after the main scene in order to composite heads-up-displays, menus
    ///    or other layers on top of the contents of the scene.
    ///    <p/>
    ///    An overlay always takes up the entire size of the viewport, although the 
    ///    components attached to it do not have to. An overlay has no visual element
    ///    in itself, it it merely a container for visual elements.
    ///    <p/>
    ///    Overlays are created by calling SceneManager.CreateOverlay, or by defining them
    ///    in special text scripts (.overlay files). As many overlays
    ///    as you like can be defined; after creation an overlay is hidden i.e. not
    ///    visible until you specifically enable it by calling Show(). This allows you to have multiple
    ///    overlays predefined (menus etc) which you make visible only when you want.
    ///    It is possible to have multiple overlays enabled at once; in this case the
    ///    relative ZOrder parameter of the overlays determine which one is displayed
     ///    on top.
     ///    <p/>
     ///    By default overlays are rendered into all viewports. This is fine when you only
     ///    have fullscreen viewports, but if you have picture-in-picture views, you probably
     ///    don't want the overlay displayed in the smaller viewports. You turn this off for 
     ///    a specific viewport by calling the Viewport.DisplayOverlays property.
    /// </remarks>
    public class Overlay : Resource {
        #region Member variables

        protected int zOrder;
        protected bool isVisible;
        protected SceneNode rootNode;
        /// <summary>2D element list.</summary>
        protected ArrayList elementList = new ArrayList();
        protected Hashtable elementLookup = new Hashtable();
        /// <summary>Degrees of rotation around center.</summary>
        protected float rotate;
        /// <summary>Scroll values, offsets.</summary>
        protected float scrollX, scrollY;
        /// <summary>Scale values.</summary>
        protected float scaleX, scaleY;
        protected Matrix4 transform = Matrix4.Identity;
        protected bool isTransformOutOfDate;
   
        #endregion Member variables

        #region Constructors
        
        /// <summary>
        ///    Constructor: do not call direct, use SceneManager.CreateOverlay
        /// </summary>
        /// <param name="name"></param>
        internal Overlay(string name) {
            this.name = name;
            this.scaleX = 1.0f;
            this.scaleY = 1.0f;
            this.isTransformOutOfDate = true;
            this.zOrder = 100;
            rootNode = new SceneNode(null);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///    Adds a 2d element to this overlay.
        /// </summary>
        /// <remarks>
        ///    Containers are created and managed using the GuiManager. A container
        ///    could be as simple as a square panel, or something more complex like
        ///    a grid or tree view. Containers group collections of other elements,
        ///    giving them a relative coordinate space and a common z-order.
        ///    If you want to attach a gui widget to an overlay, you have to do it via
        ///    a container.
        /// </remarks>
        /// <param name="element"></param>
        public void AddElement(OverlayElementContainer element) {
            elementList.Add(element);
            elementLookup.Add(element.Name, element);

            // notify the parent
            element.NotifyParent(null, this);

            // Set Z order, scaled to separate overlays
            // max 100 container levels per overlay, should be plenty
            element.NotifyZOrder(zOrder * 100);
        }

        /// <summary>
        ///    Adds a node capable of holding 3D objects to the overlay.
        /// </summary>
        /// <remarks>
        ///    Although overlays are traditionally associated with 2D elements, there 
        ///    are reasons why you might want to attach 3D elements to the overlay too.
        ///    For example, if you wanted to have a 3D cockpit, which was overlaid with a
        ///    HUD, then you would create 2 overlays, one with a 3D object attached for the
        ///    cockpit, and one with the HUD elements attached (the zorder of the HUD 
        ///    overlay would be higher than the cockpit to ensure it was always on top).
        ///    <p/>
        ///    A SceneNode can have any number of 3D objects attached to it. SceneNodes
        ///    are created using SceneManager.CreateSceneNode, and are normally attached 
        ///    (directly or indirectly) to the root node of the scene. By attaching them
        ///    to an overlay, you indicate that:<OL>
        ///    <LI>You want the contents of this node to only appear when the overlay is active</LI>
        ///    <LI>You want the node to inherit a coordinate space relative to the camera,
        ///    rather than relative to the root scene node</LI>
        ///    <LI>You want these objects to be rendered after the contents of the main scene
        ///    to ensure they are rendered on top</LI>
        ///    </OL>
        ///    One major consideration when using 3D objects in overlays is the behavior of 
        ///    the depth buffer. Overlays are rendered with depth checking off, to ensure
        ///    that their contents are always displayed on top of the main scene (to do 
        ///    otherwise would result in objects 'poking through' the overlay). The problem
        ///    with using 3D objects is that if they are concave, or self-overlap, then you
        ///    can get artifacts because of the lack of depth buffer checking. So you should 
        ///    ensure that any 3D objects you us in the overlay are convex and don't overlap
        ///    each other. If they must overlap, split them up and put them in 2 overlays.
        /// </remarks>
        /// <param name="node"></param>
        public void AddElement(SceneNode node) {
            // add the scene node as a child of the root node
            rootNode.AddChild(node);
        }

        /// <summary>
        ///    Clears the overlay of all attached items.
        /// </summary>
        public void Clear() {
            rootNode.Clear();
            elementList.Clear();
        }

        /// <summary>
        ///    Internal method to put the overlay contents onto the render queue.
        /// </summary>
        /// <param name="camera">Current camera being used in the render loop.</param>
        /// <param name="queue">Current render queue.</param>
        public void FindVisibleObjects(Camera camera, RenderQueue queue)
        {
            if(!isVisible) {
                return;
            }

            // add 3d elements
            rootNode.Position = camera.DerivedPosition;
            rootNode.Orientation = camera.DerivedOrientation;
            rootNode.Update(true, false);

            // set up the default queue group for the objects about to be added
            RenderQueueGroupID oldGroupID = queue.DefaultRenderGroup;
            queue.DefaultRenderGroup = RenderQueueGroupID.Overlay;
            rootNode.FindVisibleObjects(camera, queue, null, true, false);
            // reset the group
            queue.DefaultRenderGroup = oldGroupID;

            // add 2d elements
            for(int i = 0; i < elementList.Count; i++) {
                OverlayElementContainer container = (OverlayElementContainer)elementList[i];
                container.Update();
                container.UpdateRenderQueue(queue);
            }
        }

        /// <summary>
        ///    Gets a child container of this overlay by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public OverlayElementContainer GetChild(string name) {
            return (OverlayElementContainer)elementLookup[name];
        }

        /// <summary>
        ///    Used to transform the overlay when scrolling, scaling etc.
        /// </summary>
        /// <param name="xform">Array of Matrix4s to populate with the world 
        ///    transforms of this overlay.
        /// </param>
        public void GetWorldTransforms(Matrix4[] xform) {
            if(isTransformOutOfDate) {
                UpdateTransforms();
            }

            xform[0] = transform;
        }

        /// <summary>
        ///    Hides this overlay if it is currently being displayed.
        /// </summary>
        public void Hide() {
            isVisible = false;
        }

        /// <summary>
        ///    Adds the passed in angle to the rotation applied to this overlay.
        /// </summary>
        /// <param name="degress"></param>
        public void Rotate(float degrees) {
            this.Rotation = (rotate += degrees);
        }

        /// <summary>
        ///    Scrolls the overlay by the offsets provided.
        /// </summary>
        /// <remarks>
        ///    This method moves the overlay by the amounts provided. As with
        ///    other methods on this object, a full screen width / height is represented
        ///    by the value 1.0.
        /// </remarks>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        public void Scroll(float xOffset, float yOffset) {
            scrollX += xOffset;
            scrollY += yOffset;
            isTransformOutOfDate = true;
        }

        /// <summary>
        ///    Sets the scaling factor of this overlay.
        /// </summary>
        /// <remarks>
        ///    You can use this to set an scale factor to be used to zoom an overlay.
        /// </remarks>
        /// <param name="x">Horizontal scale value, where 1.0 = normal, 0.5 = half size etc</param>
        /// <param name="y">Vertical scale value, where 1.0 = normal, 0.5 = half size etc</param>
        public void SetScale(float x, float y) {
            scaleX = x;
            scaleY = y;
            isTransformOutOfDate = true;
        }

        /// <summary>
        ///    Sets the scrolling factor of this overlay.
        /// </summary>
        /// <remarks>
        ///    You can use this to set an offset to be used to scroll an 
        ///    overlay around the screen.
        /// </remarks>
        /// <param name="x">
        ///    Horizontal scroll value, where 0 = normal, -0.5 = scroll so that only
        ///    the right half the screen is visible etc
        /// </param>
        /// <param name="y">
        ///    Vertical scroll value, where 0 = normal, 0.5 = scroll down by half 
        ///    a screen etc.
        /// </param>
        public void SetScroll(float x, float y) {
            scrollX = x;
            scrollY = y;
            isTransformOutOfDate = true;
        }

        /// <summary>
        ///    Shows this overlay if it is not already visible.
        /// </summary>
        public void Show() {
            isVisible = true;
        }

        /// <summary>
        ///    Internal lazy update method.
        /// </summary>
        protected void UpdateTransforms() {
            // Ordering:
            //    1. Scale
            //    2. Rotate
            //    3. Translate
            Matrix3 rot3x3 = Matrix3.Identity;
            Matrix3 scale3x3 = Matrix3.Zero;

            rot3x3.FromEulerAnglesXYZ(0, 0, MathUtil.DegreesToRadians(rotate));
            scale3x3.m00 = scaleX;
            scale3x3.m11 = scaleY;
            scale3x3.m22 = 1.0f;

            transform = Matrix4.Identity;
            transform = rot3x3 * scale3x3;
            transform.Translation = new Vector3(scrollX, scrollY, 0);

            isTransformOutOfDate = false;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        ///    Gets whether this overlay is being displayed or not.
        /// </summary>
        public bool IsVisible {
            get {
                return isVisible;
            }
        }

        /// <summary>
        ///    Gets/Sets the rotation applied to this overlay, in degrees.
        /// </summary>
        public float Rotation {
            get {
                return rotate;
            }
            set {
                rotate = value;
                isTransformOutOfDate = true;
            }
        }

        /// <summary>
        ///    Gets the current x scale value.
        /// </summary>
        public float ScaleX {
            get {
                return scaleX;
            }
        }

        /// <summary>
        ///    Gets the current y scale value.
        /// </summary>
        public float ScaleY {
            get {
                return scaleY;
            }
        }

        /// <summary>
        ///    Gets the current x scroll value.
        /// </summary>
        public float ScrollX {
            get {
                return scrollX;
            }
        }

        /// <summary>
        ///    Gets the  current y scroll value.
        /// </summary>
        public float ScrollY {
            get {
                return scrollY;
            }
        }

        /// <summary>
        ///    Z ordering of this overlay. Valid values are between 0 and 600.
        /// </summary>
        public int ZOrder {
            get {
                return zOrder;
            }
            set {
                zOrder = value;

                // notify attached 2d elements
                for(int i = 0; i < elementList.Count; i++) {
                    ((OverlayElementContainer)elementList[i]).NotifyZOrder(zOrder);
                }
            }
        }

        public Quaternion DerivedOrientation {
            get {
                return Quaternion.Identity;
            }
        }

        public Vector3 DerivedPosition {
            get {
                return Vector3.Zero;
            }
        }

        #endregion Properties

        #region Implementation of Resource

        /// <summary>
        ///		
        /// </summary>
        public override void Preload() {
            // do nothing
        }

        /// <summary>
        ///		
        /// </summary>
        protected override void LoadImpl() {
            // do nothing
        }

        /// <summary>
        ///		
        /// </summary>
        protected override void UnloadImpl() {
		    // do nothing
        }

        /// <summary>
        ///		
        /// </summary>
        public override void Dispose() {
            // do nothing
        }

        #endregion
    }
}
