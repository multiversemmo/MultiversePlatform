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
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Scripting;
using Axiom.Animating;

namespace Axiom.Core {
	/// <summary>
	///		Abstract class definining a movable object in a scene.
	/// </summary>
	/// <remarks>
	///		Instances of this class are discrete, relatively small, movable objects
	///		which are attached to SceneNode objects to define their position.						  
	/// </remarks>
	public abstract class MovableObject : ShadowCaster, IAnimableObject {
		#region Fields

		/// <summary>
		///    Node that this node is attached to.
		/// </summary>
		protected Node parentNode;
		/// <summary>
		///    Is this object visible?
		/// </summary>
		protected bool isVisible;
		/// <summary>
		///    Name of this object.
		/// </summary>
		protected string name;
		/// <summary>
		///    The render queue to use when rendering this object.
		/// </summary>
		protected RenderQueueGroupID renderQueueID;
        protected bool renderQueueIDSet = false;
		/// <summary>
		///    Flags determining whether this object is included/excluded from scene queries.
		/// </summary>
		protected ulong queryFlags;
		/// <summary>
		///    Cached world bounding box of this object.
		/// </summary>
		protected AxisAlignedBox worldAABB;
		/// <summary>
		///    Cached world bounding spehere.
		/// </summary>
		protected Sphere worldBoundingSphere = new Sphere();
		/// <summary>
		///    A link back to a GameObject (or subclass thereof) that may be associated with this SceneObject.
		/// </summary>
		protected object userData;
		/// <summary>
		///		Flag which indicates whether this objects parent is a <see cref="TagPoint"/>.
		/// </summary>
		protected bool parentIsTagPoint;
		/// <summary>
		///		World space AABB of this object's dark cap.
		/// </summary>
		protected AxisAlignedBox worldDarkCapBounds = AxisAlignedBox.Null;
		/// <summary>
		///		Does this object cast shadows?
		/// </summary>
		protected bool castShadows;

		protected ShadowRenderableList dummyList = new ShadowRenderableList();

		#endregion Fields

		#region Constructors

		/// <summary>
		///		Default constructor.
		/// </summary>
		public MovableObject() {
			isVisible = true;
			// set default RenderQueueGroupID for this movable object
			renderQueueID = RenderQueueGroupID.Main;
			queryFlags = unchecked(0xffffffff);
			worldAABB = AxisAlignedBox.Null;
			castShadows = true;
		}

        public MovableObject(string name) {
            this.Name = name;
            
            isVisible = true;
            // set default RenderQueueGroupID for this movable object
            renderQueueID = RenderQueueGroupID.Main;
            queryFlags = unchecked(0xffffffff);
            worldAABB = AxisAlignedBox.Null;
            castShadows = true;
        }

		#endregion Constructors

		#region Properties

		/// <summary>
		///		An abstract method required by subclasses to return the bounding box of this object in local coordinates.
		/// </summary>
		public abstract AxisAlignedBox BoundingBox {
			get;
		}

		/// <summary>
		///		An abstract method required by subclasses to return the bounding box of this object in local coordinates.
		/// </summary>
		public abstract float BoundingRadius {
			get;
		}

		/// <summary>
		///     Get/Sets a link back to a GameObject (or subclass thereof, such as Entity) that may be associated with this SceneObject.
		/// </summary>
		public object UserData {
			get {
				return userData;
			}
			set {
				userData = value;
			}
		}

		/// <summary>
		///		Gets the parent node that this object is attached to.
		/// </summary>
		public Node ParentNode {
			get {
				return parentNode;
			}
		}

		/// <summary>
		///		See if this object is attached to another node.
		/// </summary>
		public bool IsAttached {
			get {
				return (parentNode != null);
			}
		}

		/// <summary>
		///		States whether or not this object should be visible.
		/// </summary>
		public virtual bool IsVisible {
			get {
				return isVisible;
			}
			set {
				isVisible = value;
			}
		}

		/// <summary>
		///		Name of this SceneObject.
		/// </summary>
		public virtual string Name {
			get { 
				return name;
			}
			set { 
				name = value;
			}
		}

		/// <summary>
		///    		Returns the full transformation of the parent SceneNode or the attachingPoint node
		/// </summary>
		public virtual Matrix4 ParentFullTransform {
			get {
				if(parentNode != null)
					return parentNode.FullTransform;
                
				// identity if no parent
				return Matrix4.Identity;
			}
		}

		/// <summary>
		///		Gets the full transformation of the parent SceneNode or TagPoint.
		/// </summary>
		public virtual Matrix4 ParentNodeFullTransform {
			get {
				if(parentNode != null) {
					// object is attached to a node, so return the nodes transform
					return parentNode.FullTransform;
				}

				// fallback
				return Matrix4.Identity;
			}
		}

		/// <summary>
		///		Gets/Sets the query flags for this object.
		/// </summary>
		/// <remarks>
		///		When performing a scene query, this object will be included or excluded according
		///		to flags on the object and flags on the query. This is a bitwise value, so only when
		///		a bit on these flags is set, will it be included in a query asking for that flag. The
		///		meaning of the bits is application-specific.
		/// </remarks>
		public ulong QueryFlags {
			get {
				return queryFlags;
			}
			set {
				queryFlags = value;
			}
		}

		/// <summary>
		///    Allows showing the bounding box of an invidual SceneObject.
		/// </summary>
		/// <remarks>
		///    This shows the bounding box of the SceneNode that the SceneObject is currently attached to.
		/// </remarks>
		public bool ShowBoundingBox {
			get {
				return ((SceneNode)parentNode).ShowBoundingBox;
			}
			set {
				((SceneNode)parentNode).ShowBoundingBox = value;
			}
		}

		/// <summary>
		///		Gets/Sets the render queue group this entity will be rendered through.
		/// </summary>
		/// <remarks>
		///		Render queues are grouped to allow you to more tightly control the ordering
		///		of rendered objects. If you do not call this method, all Entity objects default
		///		to <see cref="RenderQueueGroupID.Main"/> which is fine for most objects. You may want to alter this
		///		if you want this entity to always appear in front of other objects, e.g. for
		///		a 3D menu system or such.
		/// </remarks>
		public RenderQueueGroupID RenderQueueGroup {
			get {
				return renderQueueID;
			}
			set {
				renderQueueID = value;
                renderQueueIDSet = true;
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>
		///		Appends the specified flags to the current flags for this object.
		/// </summary>
		/// <param name="flags"></param>
		public void AddQueryFlags(ulong flags) {
			queryFlags |= flags;
		}

		/// <summary>
		///    Retrieves the axis-aligned bounding box for this object in world coordinates.
		/// </summary>
		/// <returns></returns>
		public override AxisAlignedBox GetWorldBoundingBox(bool derive) {
			if(derive) {
				worldAABB = this.BoundingBox;
				worldAABB.Transform(this.ParentFullTransform);
                ComputeContainedWorldAABBs();
			}

			return worldAABB;
		}

		/// <summary>
		///    Provide a way for subclasses to recompute the world
		///    AABBs of contained entities, i.e., SubEntities.
		/// </summary>
        public virtual void ComputeContainedWorldAABBs() {
        }

		/// <summary>
		///    Overloaded method.  Calls the overload with a default of not deriving the transform.
		/// </summary>
		/// <returns></returns>
		public Sphere GetWorldBoundingSphere() {
			return GetWorldBoundingSphere(false);
		}

		/// <summary>
		///    Retrieves the worldspace bounding sphere for this object.
		/// </summary>
		/// <param name="derive">Whether or not to derive from parent transforms.</param>
		/// <returns></returns>
		public virtual Sphere GetWorldBoundingSphere(bool derive) {
			if(derive) {
				worldBoundingSphere.Radius = this.BoundingRadius;
				worldBoundingSphere.Center = parentNode.DerivedPosition;
			}

			return worldBoundingSphere;
		}

		/// <summary>
		///		Removes the specified flags from the current flags for this object.
		/// </summary>
		/// <param name="flags"></param>
		public void RemoveQueryFlags(ulong flags) {
			queryFlags ^= flags;
		}

		#endregion Methods

		#region ShadowCaster Members

		/// <summary>
		///		Overridden.
		/// </summary>
		public override bool CastShadows {
			get {
				return castShadows;
			}
			set {
				castShadows = value;
			}
		}

		public override AxisAlignedBox GetLightCapBounds() {
			// same as original bounds
			return GetWorldBoundingBox();
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="light"></param>
		/// <param name="extrusionDistance"></param>
		/// <returns></returns>
		public override AxisAlignedBox GetDarkCapBounds(Light light, float extrusionDistance) {
			// Extrude own light cap bounds
			// need a clone to avoid modifying the original bounding box
			worldDarkCapBounds = (AxisAlignedBox)GetLightCapBounds().Clone();

			ExtrudeBounds(worldDarkCapBounds, light.GetAs4DVector(), extrusionDistance);

			return worldDarkCapBounds;
		}

        /// <summary>
        ///		Overridden.  Returns null by default.
        /// </summary>
        public override EdgeData GetEdgeList(int lodIndex) {
            return null;
        }

        public override IEnumerator GetShadowVolumeRenderableEnumerator(ShadowTechnique technique, Light light, 
			HardwareIndexBuffer indexBuffer, bool extrudeVertices, float extrusionDistance, int flags) {

			return dummyList.GetEnumerator();
		}

		public override IEnumerator GetLastShadowVolumeRenderableEnumerator() {
			return dummyList.GetEnumerator();
		}

		/// <summary>
		///		Get the distance to extrude for a point/spot light
		/// </summary>
		/// <param name="light"></param>
		/// <returns></returns>
		public override float GetPointExtrusionDistance(Light light) {
			if (parentNode != null) {
				return GetExtrusionDistance(parentNode.DerivedPosition, light);
			}
			else {
				return 0;
			}
		}

		#endregion ShadowCaster Members

        #region IAnimable methods
		
		/// <summary>
		///     Part of the IAnimableObject interface.
		///		The implementation of this property just returns null; descendents
		///     are free to override this.
		/// </summary>
		public virtual string[] AnimableValueNames {
			get { 
				return null;
			}
		}
		
		/// <summary>
		///     Part of the IAnimableObject interface.
		///		Create an AnimableValue for the attribute with the given name, or 
		///     throws an exception if this object doesn't support creating them.
		/// </summary>
        public virtual AnimableValue CreateAnimableValue(string valueName) {
			throw new Exception("This object has no AnimableValue attributes");
		}

		#endregion IAnimable methods

		#region Internal engine methods

		/// <summary>
		///		Internal method called to notify the object that it has been attached to a node.
		/// </summary>
		/// <param name="node">Scene node to notify.</param>
		internal virtual void NotifyAttached(Node node) {
			NotifyAttached(node, false);
		}

		/// <summary>
		///		Internal method called to notify the object that it has been attached to a node.
		/// </summary>
		/// <param name="node">Scene node to notify.</param>
		internal virtual void NotifyAttached(Node node, bool isTagPoint) {
			parentNode = node;
			parentIsTagPoint = isTagPoint;
		}

		/// <summary>
		///		Internal method to notify the object of the camera to be used for the next rendering operation.
		/// </summary>
		/// <remarks>
		///		Certain objects may want to do specific processing based on the camera position. This method notifies
		///		them incase they wish to do this.
		/// </remarks>
		/// <param name="camera">Reference to the Camera being used for the current rendering operation.</param>
		public abstract void NotifyCurrentCamera(Camera camera);

		/// <summary>
		///		An abstract method that causes the specified RenderQueue to update itself.  
		/// </summary>
		/// <remarks>This is an internal method used by the engine assembly only.</remarks>
		/// <param name="queue">The render queue that this object should be updated in.</param>
		public abstract void UpdateRenderQueue(RenderQueue queue);

		#endregion Internal engine methods

	}
}
