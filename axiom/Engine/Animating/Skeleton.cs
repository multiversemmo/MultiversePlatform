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
using System.IO;

using Axiom.Collections;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Serialization;
using Axiom.Utility;

namespace Axiom.Animating {
    /// <summary>
    ///		A collection of Bone objects used to animate a skinned mesh.
    ///	 </summary>
    ///	 <remarks>
    ///		Skeletal animation works by having a collection of 'bones' which are 
    ///		actually just joints with a position and orientation, arranged in a tree structure.
    ///		For example, the wrist joint is a child of the elbow joint, which in turn is a
    ///		child of the shoulder joint. Rotating the shoulder automatically moves the elbow
    ///		and wrist as well due to this hierarchy.
    ///		<p/>
    ///		So how does this animate a mesh? Well every vertex in a mesh is assigned to one or more
    ///		bones which affects it's position when the bone is moved. If a vertex is assigned to 
    ///		more than one bone, then weights must be assigned to determine how much each bone affects
    ///		the vertex (actually a weight of 1.0 is used for single bone assignments). 
    ///		Weighted vertex assignments are especially useful around the joints themselves
    ///		to avoid 'pinching' of the mesh in this region. 
    ///		<p/>
    ///		Therefore by moving the skeleton using preset animations, we can animate the mesh. The
    ///		advantage of using skeletal animation is that you store less animation data, especially
    ///		as vertex counts increase. In addition, you are able to blend multiple animations together
    ///		(e.g. walking and looking around, running and shooting) and provide smooth transitions
    ///		between animations without incurring as much of an overhead as would be involved if you
    ///		did this on the core vertex data.
    ///		<p/>
    ///		Skeleton definitions are loaded from datafiles, namely the .xsf file format. They
    ///		are loaded on demand, especially when referenced by a Mesh.
    /// </remarks>
    public class Skeleton : Resource {

        #region Member variables

        protected static TimingMeter skeletonLoadMeter = MeterManager.GetMeter("Skeleton Load", "Skeleton");

        /// <summary>Mode of animation blending to use.</summary>
        protected SkeletalAnimBlendMode blendMode;
        /// <summary>Internal list of bones attached to this skeleton, indexed by handle.</summary>
        protected Dictionary<int, Bone> boneList = new Dictionary<int, Bone>();
        /// <summary>Internal list of bones attached to this skeleton, indexed by name.</summary>
        protected Dictionary<string, Bone> namedBoneList = new Dictionary<string, Bone>();
        /// <summary>The entity that is currently updating this skeleton.</summary>
        protected Entity currentEntity;
        /// <summary>Reference to the root bone of this skeleton.</summary>
        protected List<Bone> rootBones = new List<Bone>();
        /// <summary>Used for auto generated handles to ensure they are unique.</summary>
        protected internal ushort nextAutoHandle;
        /// <summary>Lookup table for animations related to this skeleton.</summary>
        protected AnimationCollection animationList = new AnimationCollection();
        /// <summary>Internal list of bones attached to this skeleton, indexed by handle.</summary>
        protected List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();

        public ICollection<Bone> Bones { get { return boneList.Values; } }

        public List<AttachmentPoint> AttachmentPoints { get { return attachmentPoints; } }

        #endregion Member variables

        #region Constants

        /// <summary>Maximum total available bone matrices that are available during blending.</summary>
        public const int MAX_BONE_COUNT = 256;

        #endregion Constants

        #region Constructors

        /// <summary>
        ///    Default constructor.
        /// </summary>
        public Skeleton(string name) {
            this.name = name;

            // default to weighted blending
            blendMode = SkeletalAnimBlendMode.Average;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///    Creates a new Animation object for animating this skeleton.
        /// </summary>
        /// <param name="name">The name of this animation</param>
        /// <param name="length">The length of the animation in seconds</param>
        /// <returns></returns>
        public virtual Animation CreateAnimation(string name, float length) {
            // Check name not used
            if (animationList.ContainsKey(name)) {
                throw new Exception("An animation with the name already exists");
            }

            Animation anim = new Animation(name, length);

            animationList.Add(name, anim);

            return anim;
        }

        /// <summary>
        ///    Creates a brand new Bone owned by this Skeleton. 
        /// </summary>
        /// <remarks>
        ///    This method creates an unattached new Bone for this skeleton. Unless this is to
        ///    be the root bone (there must only be one of these), you must
        ///    attach it to another Bone in the skeleton using addChild for it to be any use. 
        ///    For this reason you will likely be better off creating child bones using the
        ///    Bone.CreateChild method instead, once you have created the root bone. 
        ///    <p/>
        ///    Note that this method automatically generates a handle for the bone, which you
        ///    can retrieve using Bone.Handle. If you wish the new Bone to have a specific
        ///    handle, use the alternate form of this method which takes a handle as a parameter,
        ///    although you should note the restrictions.
        /// </remarks>
        public Bone CreateBone() {
            return CreateBone(nextAutoHandle++);
        }

        /// <summary>
        ///    Creates a brand new Bone owned by this Skeleton. 
        /// </summary>
        /// <remarks>
        ///    This method creates an unattached new Bone for this skeleton. Unless this is to
        ///    be the root bone (there must only be one of these), you must
        ///    attach it to another Bone in the skeleton using addChild for it to be any use. 
        ///    For this reason you will likely be better off creating child bones using the
        ///    Bone.CreateChild method instead, once you have created the root bone. 
        /// </remarks>
        /// <param name="name">
        ///    The name to give to this new bone - must be unique within this skeleton. 
        ///    Note that the way the engine looks up bones is via a numeric handle, so if you name a
        ///    Bone this way it will be given an automatic sequential handle. The name is just
        ///    for your convenience, although it is recommended that you only use the handle to 
        ///    retrieve the bone in performance-critical code.
        /// </param>
        public virtual Bone CreateBone(string name) {
            if(boneList.Count == MAX_BONE_COUNT) {
                throw new Exception("Skeleton exceeded the maximum amount of bones.");
            }

            // create the new bone, and add it to both lookup lists
            Bone bone = new Bone(name, nextAutoHandle++, this);
            boneList.Add(bone.Handle, bone);
            namedBoneList.Add(bone.Name, bone);

            return bone;
        }

        /// <summary>
        ///    Creates a brand new Bone owned by this Skeleton. 
        /// </summary>
        /// <param name="handle">
        ///    The handle to give to this new bone - must be unique within this skeleton. 
        ///    You should also ensure that all bone handles are eventually contiguous (this is to simplify
        ///    their compilation into an indexed array of transformation matrices). For this reason
        ///    it is advised that you use the simpler createBone method which automatically assigns a
        ///    sequential handle starting from 0.
        /// </param>
        public virtual Bone CreateBone(ushort handle) {
            if(boneList.Count == MAX_BONE_COUNT) {
                throw new Exception("Skeleton exceeded the maximum amount of bones.");
            }

            // create the new bone, and add it to both lookup lists
            Bone bone = new Bone(nextAutoHandle++, this);
            boneList.Add(bone.Handle, bone);
            namedBoneList.Add(bone.Name, bone);

            return bone;
        }

        /// <summary>
        ///    Creates a brand new Bone owned by this Skeleton. 
        /// </summary>
        /// <param name="name">
        ///    The name to give to this new bone - must be unique within this skeleton. 
        ///    Note that the way the engine looks up bones is via a numeric handle, so if you name a
        ///    Bone this way it will be given an automatic sequential handle. The name is just
        ///    for your convenience, although it is recommended that you only use the handle to 
        ///    retrieve the bone in performance-critical code.
        /// </param>
        /// <param name="handle">
        ///    The handle to give to this new bone - must be unique within this skeleton. 
        ///    You should also ensure that all bone handles are eventually contiguous (this is to simplify
        ///    their compilation into an indexed array of transformation matrices). For this reason
        ///    it is advised that you use the simpler createBone method which automatically assigns a
        ///    sequential handle starting from 0.
        /// </param>
        public virtual Bone CreateBone(string name, ushort handle) {
            if(boneList.Count == MAX_BONE_COUNT) {
                throw new Exception("Skeleton exceeded the maximum amount of bones.");
            }

            // create the new bone, and add it to both lookup lists
            Bone bone = new Bone(name, handle, this);
            boneList.Add(bone.Handle, bone);
            namedBoneList.Add(bone.Name, bone);

            return bone;
        }

        /// <summary>
        ///    Internal method which parses the bones to derive the root bone.
        /// </summary>
        protected void DeriveRootBone() {
            if(boneList.Count == 0) {
                throw new Exception("Cannot derive the root bone for a skeleton that has no bones.");
            }

			rootBones.Clear();

            // get the first bone in the list
            Bone currentBone = boneList[0];
            
			// all bones without a parent are root bones
			for(int i = 0; i < boneList.Count; i++) {
				Bone bone = boneList[i];

				if(bone.Parent == null) {
					rootBones.Add(bone);
				}
			}
        }

        /// <summary>
        ///    Returns the animation with the specified index.
        /// </summary>
        /// <param name="index">Index of the animation to retrieve.</param>
        /// <returns></returns>
        public virtual Animation GetAnimation(int index) {
            Debug.Assert(index < animationList.Count, "index < animationList.Count");

            return animationList[index];
        }

        /// <summary>
        ///    Returns the animation with the specified name.
        /// </summary>
        /// <param name="name">Name of the animation to retrieve.</param>
        /// <returns></returns>
        public virtual Animation GetAnimation(string name) {
            if(!animationList.ContainsKey(name)) {
                return null;
                // throw new Exception("Animation named '" + name + "' is not part of this skeleton.");
            }

            return animationList[name];
        }

        public virtual bool ContainsAnimation(string name) {
            return animationList.ContainsKey(name);
        }

        /// <summary>
        ///    Gets a bone by its handle.
        /// </summary>
        /// <param name="handle">Handle of the bone to retrieve.</param>
        /// <returns></returns>
        public virtual Bone GetBone(ushort handle) {
            if(!boneList.ContainsKey(handle)) {
                throw new Exception("Bone with the handle " + handle + " not found.");
            }

            return (Bone)boneList[handle];
        }

        /// <summary>
        ///    Gets a bone by its name.
        /// </summary>
        /// <param name="name">Name of the bone to retrieve.</param>
        /// <returns></returns>
        public virtual Bone GetBone(string name) {
            if(!namedBoneList.ContainsKey(name)) {
                throw new Exception("Bone with the name '" + name + "' not found.");
            }

            return (Bone)namedBoneList[name];
        }

		/// <summary>
		///    Checks to see if a bone exists
		/// </summary>
		/// <param name="name">Name of the bone to check.</param>
		/// <returns></returns>
		public virtual bool ContainsBone(string name) {
            return namedBoneList.ContainsKey(name);
		}

		/// <summary>
		///		Gets the root bone at the specified index.
		/// </summary>
		/// <param name="index">Index of the root bone to return.</param>
		/// <returns>Root bone at the specified index, or null if the index is out of bounds.</returns>
		public virtual Bone GetRootBone(int index) {
			if(index < rootBones.Count) {
				return rootBones[index];
			}

			return null;
		}

        /// <summary>
        ///    Populates the passed in array with the bone matrices based on the current position.
        /// </summary>
        /// <remarks>
        ///    Internal use only. The array passed in must
        ///    be at least as large as the number of bones.
        ///    Assumes animation has already been updated.
        /// </remarks>
        /// <param name="matrices"></param>
        internal virtual void GetBoneMatrices(Matrix4[] matrices) {
            // update derived transforms
            this.RootBone.Update(true, false);

            /* 
                Calculating the bone matrices
                -----------------------------
                Now that we have the derived orientations & positions in the Bone nodes, we have
                to compute the Matrix4 to apply to the vertices of a mesh.
                Because any modification of a vertex has to be relative to the bone, we must first
                reverse transform by the Bone's original derived position/orientation, then transform
                by the new derived position / orientation.
            */
            Matrix4 temp = Matrix4.Zero;
            for(int i = 0; i < boneList.Count; i++) {
                Bone bone = boneList[i];
                bone.GetFullTransform(ref temp);
                Matrix4.MultiplyRef(ref matrices[i], ref temp, ref bone.bindDerivedInverseTransform);
            }
        }

        /// <summary>
        ///    Initialise an animation set suitable for use with this mesh. 
        /// </summary>
        /// <remarks>
        ///    Only recommended for use inside the engine, not by applications.
        /// </remarks>
        /// <param name="animSet"></param>
        public virtual void InitAnimationState(AnimationStateSet animSet) {
            animSet.RemoveAllAnimationStates();

            // loop through all the internal animations and add new animation states to the passed in
            // collection
            for(int i = 0; i < animationList.Count; i++) {
                Animation anim = animationList[i];

                animSet.CreateAnimationState(anim.Name, 0, anim.Length);
            }
        }

        /// <summary>
        ///    Removes the animation with the specified name from this skeleton.
        /// </summary>
        /// <param name="name">Name of the animation to remove.</param>
        /// <returns></returns>
        public virtual void RemoveAnimation(string name) {
            animationList.Remove(animationList[name]);
        }

        /// <summary>
        ///    Resets the position and orientation of all bones in this skeleton to their original binding position.
        /// </summary>
        /// <remarks>
        ///    A skeleton is bound to a mesh in a binding pose. Bone positions are then modified from this
        ///    position during animation. This method returns all the bones to their original position and
        ///    orientation.
        /// </remarks>
        public void Reset() {
            Reset(false);
        }

		/// <summary>
		///    Resets the position and orientation of all bones in this skeleton to their original binding position.
		/// </summary>
		/// <remarks>
		///    A skeleton is bound to a mesh in a binding pose. Bone positions are then modified from this
		///    position during animation. This method returns all the bones to their original position and
		///    orientation.
		/// </remarks>
		public virtual void Reset(bool resetManualBones) {
			// set all bones back to their binding pose
			for(int i = 0; i < boneList.Count; i++) {
				if(!boneList[i].isManuallyControlled || resetManualBones) {
					boneList[i].Reset();
				}
			}
		}

        /// <summary>
        ///    
        /// </summary>
        /// <param name="animSet"></param>
        public virtual void SetAnimationState(AnimationStateSet animSet) {
            /* 
        Algorithm:
          1. Reset all bone positions
          2. Iterate per AnimationState, if enabled get Animation and call Animation::apply
            */

            // reset bones
            Reset();

            // per animation state
            foreach(AnimationState animState in animSet.EnabledAnimationStates) {
                Animation anim = GetAnimation(animState.Name);
                // tolerate state entries for animations we're not aware of
                if (anim != null) {
                    anim.Apply(this, animState.Time, animState.Weight, blendMode == SkeletalAnimBlendMode.Cumulative, 1.0f);
                }
            } // foreach
        }

        /// <summary>
        ///    Sets the current position / orientation to be the 'binding pose' ie the layout in which 
        ///    bones were originally bound to a mesh.
        /// </summary>
        public virtual void SetBindingPose() {
			// update the derived transforms
            UpdateTransforms();

            // set all bones back to their binding pose
            for(int i = 0; i < boneList.Count; i++) {
                boneList[i].SetBindingPose();
            }
        }

		/// <summary>
		///		Updates all the derived transforms in the skeleton.
		/// </summary>
		public virtual void UpdateTransforms() {
			for(int i = 0; i < rootBones.Count; i++) {
				rootBones[i].Update(true, false);
			}
		}

        /// <summary>
        ///   TODO: should this replace an existing attachment point with the same name?
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentHandle"></param>
        /// <param name="rotation"></param>
        /// <param name="translation"></param>
        /// <returns></returns>
        public virtual AttachmentPoint CreateAttachmentPoint(string name, ushort parentHandle, 
                                                             Quaternion rotation, Vector3 translation) {
            Bone parentBone = boneList[parentHandle];
            AttachmentPoint ap = new AttachmentPoint(name, parentBone.Name, rotation, translation);
            attachmentPoints.Add(ap);
            return ap;
        }


        #endregion Methods

        #region Properties

        /// <summary>
        ///    Gets the number of animations associated with this skeleton.
        /// </summary>
        public virtual int AnimationCount {
            get {
                return animationList.Count;
            }
        }

        /// <summary>
        ///    Gets/Sets the animation blending mode which this skeleton will use.
        /// </summary>
        public SkeletalAnimBlendMode BlendMode {
            get {
                return blendMode;
            }
            set {
                blendMode = value;
            }
        }

        /// <summary>
        ///    Gets the number of bones in this skeleton.
        /// </summary>
        public int BoneCount {
            get {
                return boneList.Count;
            }
        }

        /// <summary>
        ///    Get/Set the entity that is currently updating this skeleton.
        /// </summary>
        public Entity CurrentEntity {
            get {
                return currentEntity;
            }
            set {
                currentEntity = value;
            }
        }

        /// <summary>
        ///    Gets the root bone of the skeleton.
        /// </summary>
        /// <remarks>
        ///    The system derives the root bone the first time you ask for it. The root bone is the
        ///    only bone in the skeleton which has no parent. The system locates it by taking the
        ///    first bone in the list and going up the bone tree until there are no more parents,
        ///    and saves this top bone as the root. If you are building the skeleton manually using
        ///    CreateBone then you must ensure there is only one bone which is not a child of 
        ///    another bone, otherwise your skeleton will not work properly. If you use CreateBone
        ///    only once, and then use Bone.CreateChild from then on, then inherently the first
        ///    bone you create will by default be the root.
        /// </remarks>
        public Bone RootBone {
            get {
                if(rootBones.Count == 0) {
                    DeriveRootBone();
                }

                return rootBones[0];
            }
        }

		/// <summary>
		///		Gets the number of root bones in this skeleton.
		/// </summary>
		public int RootBoneCount {
			get {
				if(rootBones.Count == 0) {
					DeriveRootBone();
				}

				return rootBones.Count;
			}
		}

        #endregion Properties

        #region Implementation of Resource

        public override void Preload() {

        }

        /// <summary>
        ///    Generic load, called by SkeletonManager.
        /// </summary>
        protected override void LoadImpl() {
            LogManager.Instance.Write("Skeleton: Loading '{0}'...", name);
            skeletonLoadMeter.Enter();

            // load the skeleton file
            Stream data = SkeletonManager.Instance.FindResourceData(name);
            
            OgreSkeletonSerializer reader = new OgreSkeletonSerializer();
            reader.ImportSkeleton(data, this);
            // TODO: linkedSkeletonAnimSourceList
			data.Close();
            skeletonLoadMeter.Exit();
        }

        /// <summary>
        ///    Generic unload, called by SkeletonManager.
        /// </summary>
        protected override void UnloadImpl() {
            // destroy bones
            boneList.Clear();
            namedBoneList.Clear();
            rootBones.Clear();
            // manualBones.Clear();
            // manualBonesDirty = false;

            // Destroy animations
            animationList.Clear();

            // Remove all linked skeletons
            // linkedSkeletonAnimSourceList.Clear();
        }

        #endregion Implementation of Resource

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void DumpContents(string fileName) {
            FileStream fs = File.Open(fileName, FileMode.Create);
            StreamWriter writer = new StreamWriter(fs);
            writer.AutoFlush = true;
            
            writer.WriteLine("-= Debug output of skeleton  {0} =-", this.name);
            writer.WriteLine("");
            writer.WriteLine("== Bones ==");
            writer.WriteLine("Number of bones: {0}", boneList.Count);

            Quaternion q = new Quaternion();
            float angle = 0;
            Vector3 axis = new Vector3();

            // write each bone out
            foreach (Bone bone in boneList.Values) {
                writer.WriteLine("-- Bone {0} --", bone.Handle);
                writer.Write("Position: {0}", bone.Position);
                q = bone.Orientation;
                writer.Write("Rotation: {0}", q);
                q.ToAngleAxis(ref angle, ref axis);
                writer.Write(" = {0} radians around axis {1}", angle, axis);
                writer.WriteLine(""); writer.WriteLine("");
            }

            writer.WriteLine("== Animations ==");
            writer.WriteLine("Number of animations: {0}", animationList.Count);

            // animations
            foreach(Animation anim in animationList) {
                writer.WriteLine("-- Animation '{0}' (length {1}) --", anim.Name, anim.Length);
                writer.WriteLine("Number of tracks: {0}", anim.NodeTracks.Count);

                // tracks
                foreach(NodeAnimationTrack track in anim.NodeTracks.Values) {
                    writer.WriteLine("  -- AnimationTrack {0} --", track.Handle);
                    writer.WriteLine("  Affects bone: {0}", ((Bone)track.TargetNode).Handle);
                    writer.WriteLine("  Number of keyframes: {0}", track.KeyFrames.Count);

                    // key frames
                    int kf = 0;
                    for(ushort i=0; i<track.KeyFrames.Count; i++) {
                        TransformKeyFrame keyFrame = track.GetNodeKeyFrame(i);
                        writer.WriteLine("    -- KeyFrame {0} --", kf++);
                        writer.Write("    Time index: {0}", keyFrame.Time);
                        writer.WriteLine("    Translation: {0}", keyFrame.Translate);
                        q = keyFrame.Rotation;
                        writer.Write("    Rotation: {0}", q);
                        q.ToAngleAxis(ref angle, ref axis);
                        writer.WriteLine(" = {0} radians around axis {1}", angle, axis);
                    }
                }
            }

            writer.Close();
            fs.Close();
        }

    }
}
