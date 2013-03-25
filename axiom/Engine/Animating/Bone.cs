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
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Animating {
    /// <summary>
    ///    A bone in a skeleton.
    /// </summary>
    /// <remarks>
    ///    See Skeleton for more information about the principles behind skeletal animation.
    ///    This class is a node in the joint hierarchy. Mesh vertices also have assignments
    ///    to bones to define how they move in relation to the skeleton.
    /// </remarks>
    public class Bone : Node {
        #region Fields

        /// <summary>Numeric handle of this bone.</summary>
        protected ushort handle;
        /// <summary>Bones set as manuallyControlled are not reseted in Skeleton.Reset().</summary>
        public bool isManuallyControlled;
        /// <summary>The skeleton that created this bone.</summary>
        protected Skeleton creator;
        /// <summary>The inverse derived transform of the bone in the binding pose.</summary>
        public Matrix4 bindDerivedInverseTransform;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///    Constructor, not to be used directly (use Bone.CreateChild or Skeleton.CreateBone)
        /// </summary>
        public Bone(ushort handle, Skeleton creator) : base() {
            this.handle = handle;
            this.isManuallyControlled = false;
            this.creator = creator;
        }

        /// <summary>
        ///    Constructor, not to be used directly (use Bone.CreateChild or Skeleton.CreateBone)
        /// </summary>
        public Bone(string name, ushort handle, Skeleton creator) : base(name) {
            this.handle = handle;
            this.isManuallyControlled = false;
            this.creator = creator;
        }

        #endregion

        #region Methods

        /// <summary>
        ///    Creates a new Bone as a child of this bone.
        /// </summary>
        /// <returns></returns>
        protected override Node CreateChildImpl() {
            return creator.CreateBone();
        }

        /// <summary>
        ///    Creates a new Bone as a child of this bone.
        /// </summary>
        /// <param name="name">Name of the bone to create.</param>
        /// <returns></returns>
        protected override Node CreateChildImpl(string name) {
            return creator.CreateBone(name);
        }

        /// <summary>
        ///    Overloaded method.  Passes in Zero and Identity for the last 2 params.
        /// </summary>
        /// <param name="handle">The numeric handle to give the new bone; must be unique within the Skeleton.</param>
        /// <returns></returns>
        public Bone CreateChild(ushort handle) {
            return CreateChild(handle, Vector3.Zero, Quaternion.Identity);
        }

        /// <summary>
        ///    Creates a new Bone as a child of this bone.
        /// </summary>
        /// <param name="handle">The numeric handle to give the new bone; must be unique within the Skeleton.</param>
        /// <param name="translate">Initial translation offset of child relative to parent.</param>
        /// <param name="rotate">Initial rotation relative to parent.</param>
        /// <returns></returns>
        public Bone CreateChild(ushort handle, Vector3 translate, Quaternion rotate) {
            Bone bone = creator.CreateBone(handle);
            bone.Translate(translate);
            bone.Rotate(rotate);
            this.AddChild(bone);

            return bone;
        }

        /// <summary>
        ///    Resets the position and orientation of this Bone to the original binding position.
        /// </summary>
        /// <remarks>
        ///    Bones are bound to the mesh in a binding pose. They are then modified from this
        ///    position during animation. This method returns the bone to it's original position and
        ///    orientation.
        /// </remarks>
        public void Reset() {
            ResetToInitialState();
        }

        /// <summary>
        ///    Sets the current position / orientation to be the 'binding pose' ie the layout in which 
        ///    bones were originally bound to a mesh.
        /// </summary>
        public void SetBindingPose() {
            SetInitialState();

            // save inverse derived, used for mesh transform later (assumes Update has been called by Skeleton
            MakeInverseTransform(
                this.DerivedPosition, 
                Vector3.UnitScale, 
                this.DerivedOrientation, 
                ref bindDerivedInverseTransform);
        }

        #endregion

        #region Properties

        /// <summary>
        ///		Determines whether this bone is controlled at runtime.
        /// </summary>
        public bool IsManuallyControlled {
            get { return isManuallyControlled; }
            set {
                isManuallyControlled = value;
            }
        }

        /// <summary>
        ///    Gets the inverse transform which takes bone space to origin from the binding pose. 
        /// </summary>
        public Matrix4 BindDerivedInverseTransform {
            get {
                return bindDerivedInverseTransform;
            }
        }

        /// <summary>
        ///    Gets the numeric handle of this bone.
        /// </summary>
        public ushort Handle {
            get {
                return handle;
            }
        }

        #endregion
    }

    /// <summary>
    ///		Records the assignment of a single vertex to a single bone with the corresponding weight.
    ///	 </summary>
    ///	 <remarks>
    ///		This simple struct simply holds a vertex index, bone index and weight representing the
    ///		assignment of a vertex to a bone for skeletal animation. There may be many of these
    ///		per vertex if blended vertex assignments are allowed.
    ///		This is a class because we need it as a reference type to allow for modification
    ///		in places where we would only have a copy of the data if it were a struct. 
    /// </remarks>
    public class VertexBoneAssignment {
        public int vertexIndex;
        public ushort boneIndex;
        public float weight;

        public VertexBoneAssignment() {
        }
        public VertexBoneAssignment(VertexBoneAssignment other) {
            vertexIndex = other.vertexIndex;
            boneIndex = other.boneIndex;
            weight = other.weight;
        }
    }

    public class VertexBoneAssignmentWeightComparer : System.Collections.Generic.IComparer<VertexBoneAssignment>
	{
		/// <summary>Compares two objects and returns a value indicating whether one is less than, equal to or greater than the other.</summary>
		/// <returns>Value Condition Less than zero x is less than y. Zero x equals y. Greater than zero x is greater than y. </returns>
		/// <param name="y">Second object to compare. </param>
		/// <param name="x">First object to compare. </param>
		/// <filterpriority>2</filterpriority>
		public int Compare(VertexBoneAssignment xVba, VertexBoneAssignment yVba) {
			if (xVba == null && yVba == null)
				return 0;
			else if (xVba == null)
				return -1;
			else if (yVba == null)
				return 1;
			else if (xVba.weight == yVba.weight)
				return 0;
			else if (xVba.weight < yVba.weight)
				return -1;
			else // if (xVba.weight > yVba.weight)
				return 1;
		}
		
	}
}
