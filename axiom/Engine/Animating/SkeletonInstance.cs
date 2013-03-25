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
using Axiom.MathLib;

namespace Axiom.Animating {
	/// <summary>
	///		A SkeletonInstance is a single instance of a Skeleton used by a world object.
	/// </summary>
	/// <remarks>
	///		The difference between a Skeleton and a SkeletonInstance is that the
	///		Skeleton is the 'master' version much like Mesh is a 'master' version of
	///		Entity. Many SkeletonInstance objects can be based on a single Skeleton, 
	///		and are copies of it when created. Any changes made to this are not
	///		reflected in the master copy. The exception is animations; these are
	///		shared on the Skeleton itself and may not be modified here.
	/// </remarks>
	public class SkeletonInstance : Skeleton {

		#region Fields

		/// <summary>
		///		Reference to the master Skeleton.
		/// </summary>
		protected Skeleton skeleton;
		/// <summary>
		///		Used for auto generated tag point handles to ensure they are unique.
		///	</summary>
		protected internal ushort nextTagPointAutoHandle;
		protected Hashtable tagPointList = new Hashtable();

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Constructor, don't call directly, this will be created automatically
		///		when you create an <see cref="Entity"/> based on a skeletally animated Mesh.
		/// </summary>
		/// <param name="masterCopy"></param>
		public SkeletonInstance(Skeleton masterCopy) : base("") {
			this.skeleton = masterCopy;
		}

		#endregion Constructor

		#region Properties

		/// <summary>
		///		Gets the number of animations on this skeleton.
		/// </summary>
		public override int AnimationCount {
			get {
				return skeleton.AnimationCount;
			}
		}

        public Skeleton MasterSkeleton {
            get {
                return skeleton;
            }
        }

		#endregion Properties

		#region Methods

		/// <summary>
		///		Clones bones, for use in cloning the master skeleton to make this a unique 
		///		skeleton instance.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="parent"></param>
		protected void CloneBoneAndChildren(Bone source, Bone parent) {
			Bone newBone;

			if(source.Name == "") {
				newBone = CreateBone(source.Handle);
			}
			else {
				newBone = CreateBone(source.Name, source.Handle);
			}

			newBone.Orientation = source.Orientation;
			newBone.Position = source.Position;
			newBone.ScaleFactor = source.ScaleFactor;

			if(parent == null) {
				rootBones.Add(newBone);
			}
			else {
				parent.AddChild(newBone);
			}

			// process children
			foreach (Bone child in source.Children) {
				CloneBoneAndChildren(child, newBone);
			}
		}

		public TagPoint CreateTagPointOnBone(Bone bone) {
			return CreateTagPointOnBone(bone, Quaternion.Identity);
		}

		public TagPoint CreateTagPointOnBone(Bone bone, Quaternion offsetOrientation) {
			return CreateTagPointOnBone(bone, Quaternion.Identity, Vector3.Zero);
		}

		public TagPoint CreateTagPointOnBone(Bone bone, Quaternion offsetOrientation, Vector3 offsetPosition) {
			TagPoint tagPoint = new TagPoint(++nextTagPointAutoHandle, this);
			tagPointList[nextTagPointAutoHandle] = tagPoint;

			tagPoint.Translate(offsetPosition);
			tagPoint.Rotate(offsetOrientation);
			tagPoint.SetBindingPose();
			bone.AddChild(tagPoint);

			return tagPoint;
		}

		public void RemoveTagPointFromBone(Bone bone, TagPoint tagPoint) {
			bone.RemoveChild(tagPoint);
			tagPointList.Remove(tagPoint);
		}

		#endregion Methods

		#region Skeleton Members

		/// <summary>
		///		Creates a new Animation object for animating this skeleton.
		/// </summary
		/// <remarks>
		///		This method updates the reference skeleton, not just this instance!
		/// </remarks>
		/// <param name="name">The name of this animation.</param>
		/// <param name="length">The length of the animation in seconds.</param>
		/// <returns></returns>
		public override Animation CreateAnimation(string name, float length) {
			return skeleton.CreateAnimation(name, length);
		}

		/// <summary>
		///		Returns the <see cref="Animation"/> object with the specified name.
		/// </summary>
		/// <param name="name">Name of the animation to retrieve.</param>
		/// <returns>Animation with the specified name, or null if none exists.</returns>
		public override Animation GetAnimation(string name) {
			return skeleton.GetAnimation(name);
		}

		/// <summary>
		///		Returns the <see cref="Animation"/> object at the specified index.
		/// </summary>
		/// <param name="index">Index of the animation to retrieve.</param>
		/// <returns>Animation at the specified index, or null if none exists.</returns>
		public override Animation GetAnimation(int index) {
			return skeleton.GetAnimation(index);
		}

        public override bool ContainsAnimation(string name) {
            return skeleton.ContainsAnimation(name);
        }

		/// <summary>
		///		Removes an <see cref="Animation"/> from this skeleton.
		/// </summary>
		/// <param name="name">Name of the animation to remove.</param>
		public override void RemoveAnimation(string name) {
			skeleton.RemoveAnimation(name);
		}

		#endregion Methods

		#region Resource Members

		/// <summary>
		///		Overriden to copy/clone the bones of the master skeleton.
		/// </summary>
		protected override void LoadImpl() {
			nextAutoHandle = skeleton.nextAutoHandle;
			nextTagPointAutoHandle = 0;

			this.blendMode = skeleton.BlendMode;

			// copy bones starting at the roots
			for(int i = 0; i < skeleton.RootBoneCount; i++) {
				Bone rootBone = skeleton.GetRootBone(i);
				CloneBoneAndChildren(rootBone, null);
				rootBone.Update(true, false);
			}

			SetBindingPose();

            // Clone the attachment points
            for (int i = 0; i < skeleton.AttachmentPoints.Count; i++) {
                AttachmentPoint ap = skeleton.AttachmentPoints[i];
                Bone parentBone = this.GetBone(ap.ParentBone);
                this.CreateAttachmentPoint(ap.Name, parentBone.Handle, ap.Orientation, ap.Position);
            }
		}

        /// <summary>
        ///		Overriden to unload the skeleton and clear the tagpoint list.
        /// </summary>
        protected override void UnloadImpl() {
			base.UnloadImpl();

			// clear all tag points
			tagPointList.Clear();
		}

		#endregion Resource Members
	}
}
