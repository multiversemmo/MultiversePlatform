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
using System.Collections.Generic;
using System.Diagnostics;
using Axiom.Collections;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;

namespace Axiom.Core {
	#region Delegates
	/// <summary>
	///    Signature for the Node.UpdatedFromParent event which provides the newly-updated derived properties for syncronization in a physics engine for instance
	/// </summary>
	public delegate void NodeUpdateHandler(Vector3 derivedPosition, Quaternion derivedOrientation, Vector3 derivedScale);

	/// <summary>
	///    Signature for the events on the node whose 
	/// </summary>
    public delegate void NodeEventDelegate(Node node);

	#endregion

	/// <summary>
	///		Class representing a general-purpose node an articulated scene graph.
	/// </summary>
	/// <remarks>
	///		A node in the scene graph is a node in a structured tree. A node contains
	///		information about the transformation which will apply to
	///		it and all of its children. Child nodes can have transforms of their own, which
	///		are combined with their parent's transformations.
	///		
	///		This is an abstract class - concrete classes are based on this for specific purposes,
	///		e.g. SceneNode, Bone
	///	</remarks>
	///	<ogre headerVersion="1.39" sourceVersion="1.53" />
	public abstract class Node : IRenderable 
	{
		#region Events
		/// <summary>
		/// Event which provides the newly-updated derived properties for syncronization in a physics engine for instance
		/// </summary>
		public event NodeUpdateHandler UpdatedFromParent;

        /// <summary>
        ///    Called when a node gets updated.
        /// </summary>
        /// <remarks>
        ///    Note that this happens when the node's derived update happens,
        ///    not every time a method altering it's state occurs. There may 
        ///    be several state-changing calls but only one of these calls, 
        ///    when the node graph is fully updated.
        /// </remarks>
        public event NodeEventDelegate NodeUpdatedEvent;
        
        /// <summary>
        ///    Node is being destroyed
        /// </summary>
        public event NodeEventDelegate NodeDestroyedEvent;

        /// <summary>
        ///    Node has been attached to a parent
        /// </summary>
        public event NodeEventDelegate NodeAttachedEvent;

        /// <summary>
        ///    Node has been detached from a parent
        /// </summary>
        public event NodeEventDelegate NodeDetachedEvent;

		#endregion

		#region Protected member variables

		/// <summary>Name of this node.</summary>
		protected string name;
		/// <summary>Parent node (if any)</summary>
		protected Node parent;
		/// <summary>Collection of this nodes child nodes.</summary>
		protected Dictionary<string, Node> childNodes;
		public ICollection<Node> Children { get { return childNodes.Values; } }
		/// <summary>Collection of this nodes child nodes.</summary>
		protected List<Node> childrenToUpdate;
		/// <summary>Flag to indicate own transform from parent is out of date.</summary>
		protected bool needParentUpdate;
		/// <summary>Flag to indicate all children need to be updated.</summary>
		protected bool needChildUpdate;
		/// <summary>Flag indicating that parent has been notified about update request.</summary>
		protected bool isParentNotified;
		/// <summary>Orientation of this node relative to its parent.</summary>
		protected Quaternion orientation;
		/// <summary>World orientation of this node based on parents orientation.</summary>
		protected Quaternion derivedOrientation;
		/// <summary>Original orientation of this node, used for resetting to original.</summary>
		protected Quaternion initialOrientation;
		/// <summary></summary>
		protected Quaternion rotationFromInitial;
		/// <summary>Position of this node relative to its parent.</summary>
		protected Vector3 position;
		/// <summary></summary>
		protected Vector3 derivedPosition;
		/// <summary></summary>
		protected Vector3 initialPosition;
		/// <summary></summary>
		protected Vector3 translationFromInitial;
		/// <summary></summary>
		protected Vector3 scale;
		/// <summary></summary>
		protected Vector3 derivedScale;
		/// <summary></summary>
		protected Vector3 initialScale;
		/// <summary></summary>
		protected Vector3 scaleFromInitial;
		/// <summary></summary>
		protected bool inheritsScale;
		/// <summary>Weight of applied animations so far, used for blending.</summary>
		protected float accumAnimWeight;
		/// <summary>Cached derived transform as a 4x4 matrix.</summary>
		protected Matrix4 cachedTransform;
		/// <summary>Cached relative transform as a 4x4 matrix.</summary>
		protected Matrix4 cachedRelativeTransform;
		/// <summary></summary>
		protected bool needTransformUpdate;
		/// <summary></summary>
		protected bool needRelativeTransformUpdate;
		/// <summary>Material to be used is this node itself will be rendered (axes, or bones).</summary>
		protected Material nodeMaterial;
		/// <summary>SubMesh to be used is this node itself will be rendered (axes, or bones).</summary>
		protected SubMesh nodeSubMesh;

        protected Dictionary<int, object> customParams = new Dictionary<int, object>();

        protected bool suppressUpdateEvent = false;

        protected bool queuedForUpdate = false;
        
        protected static List<Node> queuedUpdates = new List<Node>();

		#endregion

		#region Static member variables
		
		protected static Material material = null;
		protected static SubMesh subMesh = null;
		protected static long nextUnnamedNodeExtNum = 1;
		/// <summary>
		///    Empty list of lights to return for IRenderable.Lights, since nodes are not lit.
		/// </summary>
        private List<Light> emptyLightList = new List<Light>();
		
		#endregion

		#region Constructors

		/// <summary>
		/// 
		/// </summary>
		public Node() {
			this.name = "Unnamed_" + nextUnnamedNodeExtNum++;

			parent = null;

			// initialize objects
			orientation = initialOrientation = derivedOrientation = Quaternion.Identity;
			position = initialPosition = derivedPosition = Vector3.Zero;
			scale = initialScale = derivedScale = Vector3.UnitScale;
			cachedTransform = Matrix4.Identity;

			inheritsScale = true;

			accumAnimWeight = 0.0f;

			childNodes = new Dictionary<string, Node>();
            childrenToUpdate = new List<Node>();

			NeedUpdate();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		public Node(string name) {
			this.name = name;

			// initialize objects
			orientation = initialOrientation = derivedOrientation = Quaternion.Identity;
			position = initialPosition = derivedPosition = Vector3.Zero;
			scale = initialScale = derivedScale = Vector3.UnitScale;
			cachedTransform = Matrix4.Identity;

			inheritsScale = true;

			accumAnimWeight = 0.0f;

			childNodes = new Dictionary<string, Node>();
			childrenToUpdate = new List<Node>();

			NeedUpdate();
		}


		#endregion

		#region Public methods
		public void RemoveFromParent() 
		{
			if(parent != null)
				parent.RemoveChild(name);//if this errors, then the parent is out of sync with the child
		}

		/// <summary>
		///    Adds a node to the list of children of this node.
		/// </summary>
		/// <param name="node"></param>
		public void AddChild(Node child) {
			string childName = child.Name;
			if(child == this)
				throw new ArgumentException(string.Format("Node '{0}' cannot be added as a child of itself.",childName));
			if(childNodes.ContainsKey(childName))
				throw new ArgumentException(string.Format("Node '{0}' already has a child node with the name '{1}'.",this.name, childName));
			
			child.RemoveFromParent();
			childNodes.Add(childName, child);

			child.NotifyOfNewParent(this);
		}

		/// <summary>
		///    Removes all child nodes from this node.
		/// </summary>
		public virtual void Clear() {
			childNodes.Clear();
		}
		

		/// <summary>
		///		Removes all child Nodes attached to this node.
		/// </summary>
		public virtual void RemoveAllChildren() 
		{
			Clear();
		}

		public bool HasChild(Node node) 
		{
			return childNodes.ContainsValue(node);
		}
		
		public bool HasChild(string name) 
		{
			return childNodes.ContainsKey(name);
		}

		/// <summary>
		///    Gets a child node by index.
		/// </summary>
		/// <param name="index"></param>
        //public Node GetChild(int index) {
        //    return childNodes[index];
        //}

		/// <summary>
		///    Gets a child node by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Node GetChild(string name) {
			return childNodes[name];
		}

		/// <summary>
		///    Removes the specifed node as a child of this node.
		/// </summary>
		/// <param name="child"></param>
		public virtual void RemoveChild(Node child) {
            if (child != null) {
                CancelUpdate(child);
                child.NotifyOfNewParent(null);
            }
            childNodes.Remove(child.Name);
		}


		/// <summary>
		///     Removes the child node with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual Node RemoveChild(string name) {
            Node child;
            if (childNodes.TryGetValue(name, out child))
                RemoveChild(child);
            return child;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
        //public virtual Node RemoveChild(int index) {
        //    if(index < 0 || index >= childNodes.Count)
        //        throw new ArgumentOutOfRangeException(string.Format("The index must be greater then or equal to 0 and less then {0}, the number of items.",childNodes.Count));
        //    Node child = childNodes[index];
        //    RemoveChild(child, index);			
        //    return child;
        //}

		
        //protected virtual void RemoveChild(Node child, int index) 
        //{
        //    CancelUpdate(child);
        //    child.NotifyOfNewParent(null);
        //    childNodes.RemoveAt(index);
        //}

		/// <summary>
		/// Scales the node, combining its current scale with the passed in scaling factor. 
		/// </summary>
		/// <remarks>
		///	This method applies an extra scaling factor to the node's existing scale, (unlike setScale
		///	which overwrites it) combining its current scale with the new one. E.g. calling this 
		///	method twice with Vector3(2,2,2) would have the same effect as setScale(Vector3(4,4,4)) if
		/// the existing scale was 1.
		/// 
		///	Note that like rotations, scalings are oriented around the node's origin.
		///</remarks>
		/// <param name="scale"></param>
		public virtual void Scale(Vector3 factor) {
			scale = scale * factor;
			NeedUpdate();
		}

		/// <summary>
		/// Moves the node along the cartesian axes.
		///
		///	This method moves the node by the supplied vector along the
		///	world cartesian axes, i.e. along world x,y,z
		/// </summary>
		/// <param name="scale">Vector with x,y,z values representing the translation.</param>
		public virtual void Translate(Vector3 translate) {
			Translate(translate, TransformSpace.Parent);
		}
		
		/// <summary>
		/// Moves the node along the cartesian axes.
		///
		///	This method moves the node by the supplied vector along the
		///	world cartesian axes, i.e. along world x,y,z
		/// </summary>
		/// <param name="scale">Vector with x,y,z values representing the translation.</param>
		public virtual void Translate(Vector3 translate, TransformSpace relativeTo) {
			switch(relativeTo) {
				case TransformSpace.Local:
					// position is relative to parent so transform downwards
					position += orientation * translate;
					break;

				case TransformSpace.World:
					if(parent != null) {
						position += parent.DerivedOrientation.Inverse() * translate;
					}
					else {
						position += translate;
					}

					break;

				case TransformSpace.Parent:
					position = position + translate;
					break;
			}

			NeedUpdate();
		}

		/// <summary>
		/// Moves the node along arbitrary axes.
		/// </summary>
		/// <remarks>
		///	This method translates the node by a vector which is relative to
		///	a custom set of axes.
		///	</remarks>
		/// <param name="axes">3x3 Matrix containg 3 column vectors each representing the
		///	X, Y and Z axes respectively. In this format the standard cartesian axes would be expressed as:
		///		1 0 0
		///		0 1 0
		///		0 0 1
		///		i.e. The Identity matrix.
		///	</param>
		/// <param name="move">Vector relative to the supplied axes.</param>
		public virtual void Translate(Matrix3 axes, Vector3 move) {
			Vector3 derived = axes * move;
			Translate(derived, TransformSpace.Parent);
		}

		/// <summary>
		/// Moves the node along arbitrary axes.
		/// </summary>
		/// <remarks>
		///	This method translates the node by a vector which is relative to
		///	a custom set of axes.
		///	</remarks>
		/// <param name="axes">3x3 Matrix containg 3 column vectors each representing the
		///	X, Y and Z axes respectively. In this format the standard cartesian axes would be expressed as:
		///		1 0 0
		///		0 1 0
		///		0 0 1
		///		i.e. The Identity matrix.
		///	</param>
		/// <param name="move">Vector relative to the supplied axes.</param>
		public virtual void Translate(Matrix3 axes, Vector3 move, TransformSpace relativeTo) {
			Vector3 derived = axes * move;
			Translate(derived, relativeTo);
		}

		/// <summary>
		/// Rotate the node around the X-axis.
		/// </summary>
		/// <param name="degrees"></param>
		public virtual void Pitch(float degrees, TransformSpace relativeTo) {
			Rotate(Vector3.UnitX, degrees, relativeTo);
		}

		/// <summary>
		/// Rotate the node around the X-axis.
		/// </summary>
		/// <param name="degrees"></param>
		public virtual void Pitch(float degrees) {
			Rotate(Vector3.UnitX, degrees, TransformSpace.Local);
		}		
		
		/// <summary>
		/// Rotate the node around the Z-axis.
		/// </summary>
		/// <param name="degrees"></param>
		public virtual void Roll(float degrees, TransformSpace relativeTo) {
			Rotate(Vector3.UnitZ, degrees, relativeTo);
		}

		/// <summary>
		/// Rotate the node around the Z-axis.
		/// </summary>
		/// <param name="degrees"></param>
		public virtual void Roll(float degrees) {
			Rotate(Vector3.UnitZ, degrees, TransformSpace.Local);
		}

		/// <summary>
		/// Rotate the node around the Y-axis.
		/// </summary>
		/// <param name="degrees"></param>
		public virtual void Yaw(float degrees, TransformSpace relativeTo) {
			Rotate(Vector3.UnitY, degrees, relativeTo);
		}

		/// <summary>
		/// Rotate the node around the Y-axis.
		/// </summary>
		/// <param name="degrees"></param>
		public virtual void Yaw(float degrees) {
			Rotate(Vector3.UnitY, degrees, TransformSpace.Local);
		}

		/// <summary>
		/// Rotate the node around an arbitrary axis.
		/// </summary>
		public virtual void Rotate(Vector3 axis, float degrees, TransformSpace relativeTo) {
			Quaternion q = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(degrees), axis);
			Rotate(q, relativeTo);
		}

		/// <summary>
		/// Rotate the node around an arbitrary axis.
		/// </summary>
		public virtual void Rotate(Vector3 axis, float degrees) {
			Rotate(axis, degrees, TransformSpace.Local);
		}

		/// <summary>
		/// Rotate the node around an arbitrary axis using a Quaternion.
		/// </summary>
		public virtual void Rotate(Quaternion rotation, TransformSpace relativeTo) {
			switch(relativeTo) {
				case TransformSpace.Parent:
					// Rotations are normally relative to local axes, transform up
					orientation = rotation * orientation;
					break;

				case TransformSpace.World:
					orientation = orientation * DerivedOrientation.Inverse() * rotation * DerivedOrientation;
					break;

				case TransformSpace.Local:
					// Note the order of the mult, i.e. q comes after
					orientation = orientation * rotation;
					break;
			}

			NeedUpdate();
		}

		/// <summary>
		/// Rotate the node around an arbitrary axis using a Quaternion.
		/// </summary>
		public virtual void Rotate(Quaternion rotation) {
			Rotate(rotation, TransformSpace.Local);
		}

		/// <summary>
		/// Resets the nodes orientation (local axes as world axes, no rotation).
		/// </summary>
		public virtual void ResetOrientation() {
			orientation = Quaternion.Identity;
			NeedUpdate();
		}
		
		/// <summary>
		/// Resets the position / orientation / scale of this node to its initial state, see SetInitialState for more info.
		/// </summary>
		public virtual void ResetToInitialState() {
            position = initialPosition;
			orientation = initialOrientation;
			scale = initialScale;

			// Reset weights
			accumAnimWeight = 0.0f;
			translationFromInitial.x = 0f;
			translationFromInitial.y = 0f;
			translationFromInitial.z = 0f;
			rotationFromInitial.w = 1f;
			rotationFromInitial.x = 0f;
			rotationFromInitial.y = 0f;
			rotationFromInitial.z = 0f;
			scaleFromInitial.x = 1f;
			scaleFromInitial.y = 1f;
			scaleFromInitial.z = 1f;
			NeedUpdate();
		}

		/// <summary>
		/// Sets the current transform of this node to be the 'initial state' ie that
		///	position / orientation / scale to be used as a basis for delta values used
		/// in keyframe animation.
		/// </summary>
		/// <remarks>
		///	You never need to call this method unless you plan to animate this node. If you do
		///	plan to animate it, call this method once you've loaded the node with its base state,
		///	ie the state on which all keyframes are based.
		///
		///	If you never call this method, the initial state is the identity transform (do nothing) and a position of zero
		/// </remarks>
		public virtual void SetInitialState() {
			initialOrientation = orientation;
			initialPosition = position;
			initialScale = scale;
		}

		/// <summary>
		///    Creates a new name child node.
		/// </summary>
		/// <param name="name"></param>
		public virtual Node CreateChild(string name) {
			return CreateChild(name, Vector3.Zero, Quaternion.Identity);
		}

		/// <summary>
		///    Creates a new named child node.
		/// </summary>
		/// <param name="name">Name of the node.</param>
		/// <param name="translate">A vector to specify the position relative to the parent.</param>
		/// <returns></returns>
		public virtual Node CreateChild(string name, Vector3 translate) {
			return CreateChild(name, translate, Quaternion.Identity);
		}

		/// <summary>
		///    Creates a new named child node.
		/// </summary>
		/// <param name="name">Name of the node.</param>
		/// <param name="translate">A vector to specify the position relative to the parent.</param>
		/// <param name="rotate">A quaternion to specify the orientation relative to the parent.</param>
		/// <returns></returns>
		public virtual Node CreateChild(string name, Vector3 translate, Quaternion rotate) {
			Node newChild = CreateChildImpl(name);
			newChild.Translate(translate);
			newChild.Rotate(rotate);
			AddChild(newChild);

			return newChild;
		}

		/// <summary>
		///    Creates a new Child node.
		/// </summary>
		public virtual Node CreateChild() {
			return CreateChild(Vector3.Zero, Quaternion.Identity);
		}

		/// <summary>
		///    Creates a new child node.
		/// </summary>
		/// <param name="translate">A vector to specify the position relative to the parent.</param>
		/// <returns></returns>
		public virtual Node CreateChild(Vector3 translate) {
			return CreateChild(translate, Quaternion.Identity);
		}

		/// <summary>
		///    Creates a new child node.
		/// </summary>
		/// <param name="translate">A vector to specify the position relative to the parent.</param>
		/// <param name="rotate">A quaternion to specify the orientation relative to the parent.</param>
		/// <returns></returns>
		public virtual Node CreateChild(Vector3 translate, Quaternion rotate) {
			Node newChild = CreateChildImpl();
			newChild.Translate(translate);
			newChild.Rotate(rotate);
			AddChild(newChild);

			return newChild;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="camera"></param>
		/// <returns></returns>
		public float GetSquaredViewDepth(Camera camera) {
			Vector3 difference = this.DerivedPosition - camera.DerivedPosition;

			// return squared length to avoid doing a square root when it is not imperative
			return difference.LengthSquared;
		}

		/// <summary>
		///    
		/// </summary>
		/// <param name="matrices"></param>
		public void GetWorldTransforms(Matrix4[] matrices) {
			MaybeComputeFullTransform();
            matrices[0] = this.cachedTransform;
		}

		/// <summary>
		///		To be called in the event of transform changes to this node that require its recalculation.
		/// </summary>
		/// <remarks>
		///		This not only tags the node state as being 'dirty', it also requests its parent to 
		///		know about its dirtiness so it will get an update next time.
		/// </remarks>
		public virtual void NeedUpdate() {
			needParentUpdate = true;
			needChildUpdate = true;
			needTransformUpdate = true;
			needRelativeTransformUpdate = true;

			// make sure we are not the root node
			if(parent != null && !isParentNotified) {
				parent.RequestUpdate(this);
				isParentNotified = true;
			}

			// all children will be updated shortly
			childrenToUpdate.Clear();
		}


		/// <summary>
		///		Called by children to notify their parent that they need an update.
		/// </summary>
		/// <param name="child"></param>
		public virtual void RequestUpdate(Node child) {
			// if we are already going to update everything, this wont matter
			if(needChildUpdate)
				return;

			// add to the list of children that need updating
			if(!childrenToUpdate.Contains(child))
				childrenToUpdate.Add(child);

			// request to update me
			if(parent != null && !isParentNotified) {
				parent.RequestUpdate(this);
				isParentNotified = true;
			}
		}

		/// <summary>
		///		Called by children to notify their parent that they no longer need an update.
		/// </summary>
		/// <param name="child"></param>
		public virtual void CancelUpdate(Node child) {
			// remove this from the list of children to update
			childrenToUpdate.Remove(child);

			// propogate this changed if we are done
			if(childrenToUpdate.Count == 0 && parent != null && !needChildUpdate) {
				parent.CancelUpdate(this);
				isParentNotified = false;
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		///		Gets the number of children attached to this node.
		/// </summary>
		public int ChildCount {
			get {
				return childNodes.Count;
			}
		}

		/// <summary>
		/// Gets or sets the name of this Node object.
		/// </summary>
		/// <remarks>This is autogenerated initially, so setting it is optional.</remarks>
		public string Name {
			get { 
				return name;	
			}
			set {
				if(value == name)
					return;
				string oldName = name;
				name = value;
				if(parent != null) 
				{
					//ensure that it is keyed under this new name in its parent's collection
					parent.RemoveChild(oldName);
					parent.AddChild(this);
				}
				OnRename(oldName);

			}
		}

		/// <summary>
		/// Can be overriden in derived classes to fire an event or rekey this node in the collections which contain it
		/// </summary>
		/// <param name="oldName"></param>
		protected virtual void OnRename(string oldName) {}

        protected virtual void ParentChanged(Node oldParent, Node newParent) {
            if(oldParent != null) {
                oldParent.RemoveChild(this);
                if (NodeDetachedEvent != null)
                    NodeDetachedEvent(this);
            }
            if (newParent != null) {
                newParent.AddChild(this); 
                if (NodeAttachedEvent != null)
                    NodeAttachedEvent(this);
            }
        }

		/// <summary>
		/// Get the Parent Node of the current Node.
		/// </summary>
		public virtual Node Parent {
			get { 
				return parent; 
			}
			set {
				if(parent != value) {
					ParentChanged(parent, value);
                    parent = value;
                }
			}
		}

		protected virtual void NotifyOfNewParent(Node newParent) 
		{			
			parent = newParent;
			isParentNotified = false;
			NeedUpdate();
		}

        public static void QueueNeedUpdate(Node node) {
            // Don't queue the node more than once
            if (!node.queuedForUpdate) {
                node.queuedForUpdate = true;
                queuedUpdates.Add(node);
            }
        }

        public static void ProcessQueuedUpdates() {
            if (queuedUpdates != null) {
                foreach (Node node in queuedUpdates) {
                    // Update, and force parent update since chances are we've ended
                    // up with some mixed state in there due to re-entrancy
                    node.queuedForUpdate = false;
                    node.NeedUpdate();
                }
                queuedUpdates.Clear();
            }
        }

		/// <summary>
		///    A Quaternion representing the nodes orientation.
		/// </summary>
		public virtual Quaternion Orientation {
			get { 
				return orientation; 
			}
			set { 
				orientation = value; 
				NeedUpdate();
			}
		}

		/// <summary>
		/// The position of the node relative to its parent.
		/// </summary>
		public virtual Vector3 Position {
			get { 
				return position; 
			}
			set {	
				position = value;  
				NeedUpdate();
			}
		}

		/// <summary>
		/// The scaling factor applied to this node.
		/// </summary>
		/// <remarks>
		///	Scaling factors, unlike other transforms, are not always inherited by child nodes. 
		///	Whether or not scalings affect both the size and position of the child nodes depends on
		///	the setInheritScale option of the child. In some cases you want a scaling factor of a parent node
		///	to apply to a child node (e.g. where the child node is a part of the same object, so you
		///	want it to be the same relative size and position based on the parent's size), but
		///	not in other cases (e.g. where the child node is just for positioning another object,
		///	you want it to maintain its own size and relative position). The default is to inherit
		///	as with other transforms.
		///
		///	Note that like rotations, scalings are oriented around the node's origin.
		///	</remarks>
		public virtual Vector3 ScaleFactor {
			get { 
				return scale; 
			}
			set { 
				scale = value; 
				NeedUpdate();  
			}
		}

		/// <summary>
		/// Tells the node whether it should inherit scaling factors from its parent node.
		/// </summary>
		/// <remarks>
		///	Scaling factors, unlike other transforms, are not always inherited by child nodes. 
		///	Whether or not scalings affect both the size and position of the child nodes depends on
		///	the setInheritScale option of the child. In some cases you want a scaling factor of a parent node
		///	to apply to a child node (e.g. where the child node is a part of the same object, so you
		///	want it to be the same relative size and position based on the parent's size), but
		///	not in other cases (e.g. where the child node is just for positioning another object,
		///	you want it to maintain its own size and relative position). The default is to inherit
		///	as with other transforms.
		///	If true, this node's scale and position will be affected by its parent's scale. If false,
		///	it will not be affected.
		///</remarks>
		public virtual bool InheritScale {
			get { 
				return inheritsScale; 
			}
			set { 
				inheritsScale = value; 
				NeedUpdate();  
			}
		}

		/// <summary>
		/// Gets a matrix whose columns are the local axes based on
		/// the nodes orientation relative to its parent.
		/// </summary>
		public virtual Matrix3 LocalAxes {
			get {	
				// get the 3 unit Vectors
				Vector3 xAxis = Vector3.UnitX;
				Vector3 yAxis = Vector3.UnitY;
				Vector3 zAxis = Vector3.UnitZ;

				// multpliy each times the current orientation
				xAxis = orientation * xAxis;
				yAxis = orientation * yAxis;
				zAxis = orientation * zAxis;

				return new Matrix3(xAxis, yAxis, zAxis);
			}
		}

		#endregion

		#region Protected methods
		/// <summary>
		///	Triggers the node to update its combined transforms.
		///
		///	This method is called internally by the engine to ask the node
		///	to update its complete transformation based on its parents
		///	derived transform.
		/// </summary>
		// TODO: This was previously protected.  Was made internal to allow access to custom collections.
		virtual internal void UpdateFromParent() {
			if(parent != null) {
				if(parent.needParentUpdate) {
					parent.UpdateFromParent();
					parent.needParentUpdate = false;
				}
				// combine local orientation with parents
				Quaternion parentOrientation = parent.derivedOrientation;
                derivedOrientation.w = parentOrientation.w * orientation.w - parentOrientation.x * orientation.x - parentOrientation.y * orientation.y - parentOrientation.z * orientation.z;
                derivedOrientation.x = parentOrientation.w * orientation.x + parentOrientation.x * orientation.w + parentOrientation.y * orientation.z - parentOrientation.z * orientation.y;
                derivedOrientation.y = parentOrientation.w * orientation.y + parentOrientation.y * orientation.w + parentOrientation.z * orientation.x - parentOrientation.x * orientation.z;
                derivedOrientation.z = parentOrientation.w * orientation.z + parentOrientation.z * orientation.w + parentOrientation.x * orientation.y - parentOrientation.y * orientation.x;

				// change position vector based on parent's orientation
				Quaternion.MultiplyRef(ref derivedPosition, ref parentOrientation, ref position);

				// update scale
				if(inheritsScale) {
					// set out own position by parent scale
					derivedPosition.x *= parent.derivedScale.x;
					derivedPosition.y *= parent.derivedScale.y;
					derivedPosition.z *= parent.derivedScale.z;

					// set own scale, just combine as equivalent axes, no shearing
					derivedScale.x = scale.x * parent.derivedScale.x;
					derivedScale.y = scale.y * parent.derivedScale.y;
					derivedScale.z = scale.z * parent.derivedScale.z;
				}
				else {
					// do not inherit parents scale
					derivedScale = scale;
				}

				// add parents positition to local altered position
                derivedPosition.x += parent.derivedPosition.x;
                derivedPosition.y += parent.derivedPosition.y;
                derivedPosition.z += parent.derivedPosition.z;
			}
			else {
				// Root node, no parent
				derivedOrientation = orientation;
				derivedPosition = position;
				derivedScale = scale;
			}

			needTransformUpdate = true;
			needRelativeTransformUpdate = true;
            needParentUpdate = false;
            if (suppressUpdateEvent == false)
            {
                OnUpdatedFromParent();
            }
		}

        public void OnUpdatedFromParent() {
            if (UpdatedFromParent != null)
                UpdatedFromParent(derivedPosition, derivedOrientation, derivedScale);
            // Call listener if there is one
            if (NodeUpdatedEvent != null)
                NodeUpdatedEvent(this);
        }

        public void OnNodeDestroyed() {
            if (NodeDestroyedEvent != null)
                NodeDestroyedEvent(this);
        }

        public void OnNodeAttacheded() {
            if (NodeAttachedEvent != null)
                NodeAttachedEvent(this);
        }

        public void OnNodeDetacheded() {
            if (NodeDetachedEvent != null)
                NodeDetachedEvent(this);
        }

		/// <summary>
		/// Internal method for building a Matrix4 from orientation / scale / position. 
		/// </summary>
		/// <remarks>
		///	Transform is performed in the order scale, rotate, translation, i.e. translation is independent
		///	of orientation axes, scale does not affect size of translation, rotation and scaling are always
		///	centered on the origin.
		///	</remarks>
		/// <param name="position"></param>
		/// <param name="scale"></param>
		/// <param name="orientation"></param>
		/// <returns></returns>
		protected void MakeTransform(Vector3 position, Vector3 scale, Quaternion orientation, ref Matrix4 destMatrix) {
			Quaternion.ToRotationMatrixPlusTranslationRef(ref destMatrix, ref orientation, ref scale, ref position);
		}

		/// <summary>
		/// Internal method for building an inverse Matrix4 from orientation / scale / position. 
		/// </summary>
		/// <remarks>
		///	As makeTransform except it build the inverse given the same data as makeTransform, so
		///	performing -translation, 1/scale, -rotate in that order.
		/// </remarks>
		/// <param name="position"></param>
		/// <param name="scale"></param>
		/// <param name="orientation"></param>
		/// <returns></returns>
		protected void MakeInverseTransform(Vector3 position, Vector3 scale, Quaternion orientation, ref Matrix4 destMatrix) {
			destMatrix = Matrix4.Identity;

			// Invert the parameters
			Vector3 invTranslate = -position;
			Vector3 invScale = Vector3.Zero;

			invScale.x = 1.0f / scale.x;
			invScale.y = 1.0f / scale.y;
			invScale.z = 1.0f / scale.z;

			Quaternion invRot = orientation.Inverse();
        
			// Because we're inverting, order is translation, rotation, scale
			// So make translation relative to scale & rotation
			invTranslate.x *= invScale.x; // scale
			invTranslate.y *= invScale.y; // scale
			invTranslate.z *= invScale.z; // scale
			invTranslate = invRot * invTranslate; // rotate

			// Next, make a 3x3 rotation matrix and apply inverse scale
			Matrix3 rot3x3 = invRot.ToRotationMatrix();
			Matrix3 scale3x3= Matrix3.Zero;

			scale3x3.m00 = invScale.x;
			scale3x3.m11 = invScale.y;
			scale3x3.m22 = invScale.z;

			// Set up final matrix with scale & rotation
			destMatrix = scale3x3 * rot3x3;

			destMatrix.Translation = invTranslate;
		}		

		/// <summary>
		/// Must be overridden in subclasses.  Specifies how a Node is created.  CreateChild uses this to create a new one and add it
		/// to the list of child nodes.  This allows subclasses to not have to override CreateChild and duplicate all its functionality.
		/// </summary>
		protected abstract Node CreateChildImpl();

		/// <summary>
		/// Must be overridden in subclasses.  Specifies how a Node is created.  CreateChild uses this to create a new one and add it
		/// to the list of child nodes.  This allows subclasses to not have to override CreateChild and duplicate all its functionality.
		/// </summary>
		/// <param name="name">The name of the node to add.</param>
		protected abstract Node CreateChildImpl(string name);

		#endregion

		#region Internal engine properties

		/// <summary>
		/// Gets the orientation of the node as derived from all parents.
		/// </summary>
		public virtual Quaternion DerivedOrientation {
			get { 
				if(needParentUpdate) {
					UpdateFromParent();
					needParentUpdate = false;
				}

				return derivedOrientation;
			}
		}

		/// <summary>
		/// Gets the position of the node as derived from all parents.
		/// </summary>
		public virtual Vector3 DerivedPosition {
			get { 
				if(needParentUpdate) {
					UpdateFromParent();
					needParentUpdate = false;
				}
				
				return derivedPosition;
			}
		}

		
		/// <summary>
		/// 
		/// </summary>
		Quaternion IRenderable.WorldOrientation 
		{
			get 
			{
				return this.DerivedOrientation;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		Vector3 IRenderable.WorldPosition 
		{
			get 
			{
				return this.DerivedPosition;
			}
		}

		/// <summary>
		/// Gets the scaling factor of the node as derived from all parents.
		/// </summary>
		public virtual Vector3 DerivedScale {
			get { 
				if(needParentUpdate) {
					UpdateFromParent();
					needParentUpdate = false;
				}

				return derivedScale; 
			}
		}

		protected void MaybeComputeFullTransform() {
            //if needs an update from parent or it has been updated from parent
            //yet this hasn't been called after that yet
            if(needTransformUpdate) {
                if(needParentUpdate) {
                    UpdateFromParent();
                    needParentUpdate = false;
                }
                Quaternion.ToRotationMatrixPlusTranslationRef(ref cachedTransform, ref this.derivedOrientation, ref this.derivedScale, ref this.derivedPosition);

                //dont need to update this again until next invalidation
                needTransformUpdate = false;
            }
        }
        
        public virtual void GetFullTransform(ref Matrix4 destMatrix) {
            MaybeComputeFullTransform();
            destMatrix = cachedTransform;
        }
        
        /// <summary>
		///	Gets the full transformation matrix for this node.
		/// </summary>
		/// <remarks>
		/// This method returns the full transformation matrix
		/// for this node, including the effect of any parent node
		/// transformations, provided they have been updated using the Node.Update() method.
		/// This should only be called by a SceneManager which knows the
		/// derived transforms have been updated before calling this method.
		/// Applications using the engine should just use the relative transforms.
		/// </remarks>
		public virtual Matrix4 FullTransform {
			get {
                MaybeComputeFullTransform();
				return cachedTransform;
			}
		}

		protected void MaybeComputeRelativeTransform() {
            //if needs an update from parent or it has been updated from parent
            //yet this hasn't been called after that yet
            if(needRelativeTransformUpdate) 
            {
                //derived properties may call Update() if needsParentUpdate is true and this will set needTransformUpdate to true
                Quaternion.ToRotationMatrixPlusTranslationRef(ref cachedRelativeTransform, ref this.orientation, ref this.scale, ref this.position);
                //dont need to update this again until next invalidation
                needRelativeTransformUpdate = false;
            }
        }
        
		/// <summary>
		///	Gets the full transformation matrix for this node.
		/// </summary>
		/// <remarks>
		/// This method returns the full transformation matrix
		/// for this node, including the effect of any parent node
		/// transformations, provided they have been updated using the Node.Update() method.
		/// This should only be called by a SceneManager which knows the
		/// derived transforms have been updated before calling this method.
		/// Applications using the engine should just use the relative transforms.
		/// </remarks>
		public virtual Matrix4 RelativeTransform 
		{
			get 
			{
                MaybeComputeRelativeTransform();
				return cachedRelativeTransform;
			}
		}

		#endregion

		#region Internal engine methods
		/// <summary>
		/// Internal method to update the Node.
		/// Updates this node and any relevant children to incorporate transforms etc.
		///	Don't call this yourself unless you are writing a SceneManager implementation.
		/// </summary>
		/// <param name="updateChildren">If true, the update cascades down to all children. Specify false if you wish to
		/// update children separately, e.g. because of a more selective SceneManager implementation.</param>
		/// <param name="hasParentChanged">if true then this will update its derived properties (scale, orientation, position) accoarding to the parent's</param>
		protected internal virtual void Update(bool updateChildren, bool hasParentChanged) {
			isParentNotified = false;

			// skip update if not needed
			if(!updateChildren && !needParentUpdate && !needChildUpdate && !hasParentChanged)
				return;

			// see if need to process everyone
			if(needParentUpdate || hasParentChanged) {
				// update transforms from parent
				UpdateFromParent();
				needParentUpdate = false;
			}

			// see if we need to process all
			if(needChildUpdate || hasParentChanged) {
				// update all children
				foreach (Node child in childNodes.Values) {
					child.Update(true, true);
				}

				childrenToUpdate.Clear();
			}
			else {
				// just update selected children
				foreach (Node child in childrenToUpdate) {
					child.Update(true, false);
				}

				// clear the list
				childrenToUpdate.Clear();
			}

			// reset the flag
			needChildUpdate = false;
		}

		/// <summary>
		/// This method transforms a Node by a weighted amount from its
		///	initial state. If weighted transforms have already been applied, 
		///	the previous transforms and this one are blended together based
		/// on their relative weight. This method should not be used in
		///	combination with the unweighted rotate, translate etc methods.
		/// </summary>
		/// <param name="weight"></param>
		/// <param name="translate"></param>
		/// <param name="rotate"></param>
		/// <param name="scale"></param>
		internal virtual void WeightedTransform(float weight, ref Vector3 translate, ref Quaternion rotate, ref Vector3 scale) {
			WeightedTransform(weight, ref translate, ref rotate, ref scale, false);
        }
        
		/// <summary>
		/// This method transforms a Node by a weighted amount from its
		///	initial state. If weighted transforms have already been applied, 
		///	the previous transforms and this one are blended together based
		/// on their relative weight. This method should not be used in
		///	combination with the unweighted rotate, translate etc methods.
		/// </summary>
		/// <param name="weight"></param>
		/// <param name="translate"></param>
		/// <param name="rotate"></param>
		/// <param name="scale"></param>
		internal virtual void WeightedTransform(float weight, ref Vector3 translate, ref Quaternion rotate, ref Vector3 scale, bool lookInMovementDirection) {
			// If no previous transforms, we can just apply
			if (accumAnimWeight == 0.0f) {
				rotationFromInitial = rotate;
				translationFromInitial = translate;
				scaleFromInitial = scale;
				accumAnimWeight = weight;
			}
			else {
				// Blend with existing
				float factor = weight / (accumAnimWeight + weight);

				// translationFromInitial += (translate - translationFromInitial) * factor;
				Vector3 tmp = Vector3.Zero;
                translationFromInitial.x += (translate.x - translationFromInitial.x) * factor;
                translationFromInitial.y += (translate.y - translationFromInitial.y) * factor;
                translationFromInitial.z += (translate.z - translationFromInitial.z) * factor;

				Quaternion result = Quaternion.Zero;
                Quaternion.SlerpRef(ref result, factor, ref rotationFromInitial, ref rotate, false);
                rotationFromInitial = result;

				// For scale, find delta from 1.0, factor then add back before applying
				scaleFromInitial.x *= 1.0f + (scale.x - 1.0f) * factor;
				scaleFromInitial.y *= 1.0f + (scale.y - 1.0f) * factor;
				scaleFromInitial.z *= 1.0f + (scale.z - 1.0f) * factor;
                accumAnimWeight += weight;
			}

			// Update final based on bind position + offsets
			// orientation = initialOrientation * rotationFromInitial;
			Quaternion.MultiplyRef(ref orientation, ref initialOrientation, ref rotationFromInitial);
            // position = initialPosition + translationFromInitial;
			position.x = initialPosition.x + translationFromInitial.x;
			position.y = initialPosition.y + translationFromInitial.y;
			position.z = initialPosition.z + translationFromInitial.z;
            // scale = initialScale * scaleFromInitial;
			scale.x = initialScale.x * scaleFromInitial.x;
			scale.y = initialScale.y * scaleFromInitial.y;
			scale.z = initialScale.z * scaleFromInitial.z;
            if(lookInMovementDirection)
				orientation = -Vector3.UnitX.GetRotationTo(translate.ToNormalized());
			
			NeedUpdate();
		}
		#endregion
	
		#region IRenderable implementation

		public bool CastsShadows {
			get {
				return false;
			}
		}
		
		/// <summary>
		///		This is only used if the SceneManager chooses to render the node. This option can be set
		///		for SceneNodes at SceneManager.DisplaySceneNodes, and for entities based on skeletal 
		///		models using Entity.DisplaySkeleton = true.
		///	 </summary>
		public void GetRenderOperation(RenderOperation op) {
			if(nodeSubMesh == null) {
				Mesh nodeMesh = MeshManager.Instance.Load("axes.mesh");
				nodeSubMesh = nodeMesh.GetSubMesh(0);
			}
			// return the render operation of the submesh itself
			nodeSubMesh.GetRenderOperation(op);
		}		
	
		/// <summary>
		///		
		/// </summary>
		/// <remarks>
		///		This is only used if the SceneManager chooses to render the node. This option can be set
		///		for SceneNodes at SceneManager.DisplaySceneNodes, and for entities based on skeletal 
		///		models using Entity.DisplaySkeleton = true.
		/// </remarks>
		public Material Material {
			get {
				if(nodeMaterial == null) {
					nodeMaterial = MaterialManager.Instance.GetByName("Core/NodeMaterial");
                    
					if(nodeMaterial == null) {
						throw new Exception("Could not find material 'Core/NodeMaterial'");
					}

					// load, will ignore if already loaded
					nodeMaterial.Load();
				}

				return nodeMaterial;
			}
		}

		public bool NormalizeNormals {
			get {
				return false;
			}
		}

		public Technique Technique {
			get {
				return this.Material.GetBestTechnique();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		public ushort NumWorldTransforms {
			get { 
				return 1; 
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool UseIdentityProjection {
			get { 
				return false; 
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool UseIdentityView {
			get { 
				return false; 
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public SceneDetailLevel RenderDetail {
			get { 
				return SceneDetailLevel.Solid;	
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual List<Light> Lights {
			get {
				return emptyLightList;
			}
		}

		public Vector4 GetCustomParameter(int index) {
			if(customParams[index] == null) {
				throw new Exception("A parameter was not found at the given index");
			}
			else {
				return (Vector4)customParams[index];
			}
		}

		public void SetCustomParameter(int index, Vector4 val) {
			customParams[index] = val;
		}

		public void UpdateCustomGpuParameter(GpuProgramParameters.AutoConstantEntry entry, GpuProgramParameters gpuParams) {
			if(customParams[entry.data] != null) {
				gpuParams.SetConstant(entry.index, (Vector4)customParams[entry.data]);
			}
		}

		#endregion
	}

}
