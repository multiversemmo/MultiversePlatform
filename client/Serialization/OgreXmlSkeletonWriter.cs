/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

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
using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Generic;

using Axiom.Animating;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;

namespace Multiverse.Serialization
{
	/// <summary>
	/// 	Class to write out a skeleton in Ogre's xml format
	/// </summary>
	public class OgreXmlSkeletonWriter
	{
		#region Member variables

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OgreXmlSkeletonWriter));
        
        protected Skeleton skeleton;

		protected Stream stream;

		protected XmlDocument document;

		protected Matrix4 exportTransform = Matrix4.Identity;
		protected float exportScale = 1.0f;

		#endregion

		#region Constructors

		public OgreXmlSkeletonWriter(Stream data) {
			stream = data;
		}

		#endregion

		#region Methods

		public void Export(Skeleton skeleton, Matrix4 exportTransform) {
			this.exportTransform = exportTransform;
			float det = exportTransform.Determinant;
			this.exportScale = (float)Math.Pow(det, 1 / 3.0f);
			Export(skeleton);
		}

		public void Export(Skeleton skeleton) {
			// store a local reference to the mesh for modification
			this.skeleton = skeleton;
			this.document = new XmlDocument();
			XmlNode skeletonNode = WriteSkeleton();
			document.AppendChild(skeletonNode);
			document.Save(stream);
		}

		protected XmlNode WriteSkeleton() {
			XmlElement node = document.CreateElement("skeleton");

			XmlNode childNode;

			// Write the bones
			childNode = WriteBones();
			node.AppendChild(childNode);

			// Next write bone hierarchy
			childNode = WriteBoneHierarchy();
			node.AppendChild(childNode);

			// Next write animations
			childNode = WriteAnimations();
			node.AppendChild(childNode);
			
			return node;
		}

		protected XmlElement WriteBones() {
			XmlElement node = document.CreateElement("bones");
			if (exportTransform != Matrix4.Identity) {
				Vector3 tmpTranslate = exportTransform.Translation;
				Quaternion tmpRotate = GetRotation(exportTransform);
				TransformSkeleton(exportTransform);
			}
			for (ushort i = 0; i < skeleton.BoneCount; ++i) {
				Bone bone = skeleton.GetBone(i);
				XmlElement childNode = WriteBone(bone);
				node.AppendChild(childNode);
			}

			return node;
		}

		protected XmlElement WriteBoneHierarchy() {
			XmlElement node = document.CreateElement("bonehierarchy");
			for (ushort i = 0; i < skeleton.BoneCount; ++i) {
				Bone bone = skeleton.GetBone(i);
				if (bone.Parent != null) {
					XmlElement subNode = document.CreateElement("boneparent");
					XmlAttribute attr;
					attr = document.CreateAttribute("bone");
					attr.Value = bone.Name;
					subNode.Attributes.Append(attr);
					attr = document.CreateAttribute("parent");
					attr.Value = bone.Parent.Name;
					subNode.Attributes.Append(attr);
					node.AppendChild(subNode);
				}
			}
			return node;
		}


		protected void TransformSkeleton(Matrix4 exportTransform) {
			Matrix4 invExportTransform = exportTransform.Inverse();
			Dictionary<string, Matrix4> fullInverseBoneTransforms = new Dictionary<string, Matrix4>();
			Skeleton newSkeleton = new Skeleton(skeleton.Name);
			// Construct new versions of the bones, and build
			// the inverse bind matrix that will be needed.
			for (ushort i = 0; i < skeleton.BoneCount; ++i) {
				Bone bone = skeleton.GetBone(i);
				Bone newBone = newSkeleton.CreateBone(bone.Name, bone.Handle);
				fullInverseBoneTransforms[bone.Name] = bone.BindDerivedInverseTransform;
			}
			//  Build the parenting relationship for the new skeleton
			for (ushort i = 0; i < skeleton.BoneCount; ++i) {
				Bone bone = skeleton.GetBone(i);
				Bone newBone = newSkeleton.GetBone(i);
				Bone parentBone = (Bone)bone.Parent;
				if (parentBone != null) {
					Bone newParentBone = newSkeleton.GetBone(parentBone.Handle);
					newParentBone.AddChild(newBone);
				}
			}
			// Set the orientation and position for the various bones
			// B' = T * B * Tinv
			for (ushort i = 0; i < newSkeleton.BoneCount; ++i) {
				Bone bone = skeleton.GetBone(i);
				string boneName = bone.Name;
				string parentName = (bone.Parent == null) ? null : bone.Parent.Name;
				Matrix4 transform = GetLocalBindMatrix(fullInverseBoneTransforms, boneName, parentName, true);
				transform = exportTransform * transform * invExportTransform;
				Quaternion orientation = GetRotation(transform);
				Bone newBone = newSkeleton.GetBone(i);
				newBone.Orientation = orientation;
				newBone.Position = transform.Translation;
                //if (newBone.Name == "Lower_Torso_BIND_jjj") {
                //    log.DebugFormat("New Bone Position: {0}", transform.Translation);
                //}

			}
			newSkeleton.SetBindingPose();
			for (int i = 0; i < skeleton.AnimationCount; ++i) {
				Animation anim = skeleton.GetAnimation(i);
				Animation newAnim = newSkeleton.CreateAnimation(anim.Name, anim.Length);
				TransformAnimation(exportTransform, newAnim, anim, newSkeleton);
			}
			skeleton = newSkeleton;
		}

		protected void TransformAnimation(Matrix4 exportTransform, 
										  Animation newAnim, Animation anim,
										  Skeleton newSkeleton) {
			foreach (NodeAnimationTrack track in anim.NodeTracks.Values) {
				NodeAnimationTrack newTrack = newAnim.CreateNodeTrack(track.Handle);
				Bone targetBone = (Bone)track.TargetNode;
				newTrack.TargetNode = newSkeleton.GetBone(targetBone.Handle);
				TransformTrack(exportTransform, newTrack, track, targetBone);
			}
		}

		protected void TransformTrack(Matrix4 exportTransform,
									  NodeAnimationTrack newTrack, 
									  NodeAnimationTrack track,
									  Bone bone) {
			Matrix4 invExportTransform = exportTransform.Inverse();
			Bone oldNode = (Bone)track.TargetNode;
			Bone newNode = (Bone)newTrack.TargetNode;
			for (int i = 0; i < track.KeyFrames.Count; ++i) {
				TransformKeyFrame keyFrame = track.GetTransformKeyFrame(i);
				TransformKeyFrame newKeyFrame = newTrack.CreateNodeKeyFrame(keyFrame.Time);
				Quaternion oldOrientation = oldNode.Orientation * keyFrame.Rotation;
				Vector3 oldTranslation = oldNode.Position + keyFrame.Translate;
                Matrix4 oldTransform = Multiverse.MathLib.MathUtil.GetTransform(oldOrientation, oldTranslation);
				Matrix4 newTransform = exportTransform * oldTransform * invExportTransform;
				Quaternion newOrientation = GetRotation(newTransform);
				Vector3 newTranslation = newTransform.Translation;
				newKeyFrame.Rotation = newNode.Orientation.Inverse() * newOrientation;
				newKeyFrame.Translate = newTranslation - newNode.Position;
                //if (oldNode.Name == "Lower_Torso_BIND_jjj") {
                //    log.DebugFormat("New translation: {0}; New Position: {1}", newTranslation, newNode.Position);
                //}
			}
		}

#if COMPLEX_WAY
		protected void TransformSkeleton(Matrix4 unscaledTransform, float scale) {
			Matrix4 invExportTransform = unscaledTransform.Inverse();
			Dictionary<string, Matrix4> fullInverseBoneTransforms = new Dictionary<string, Matrix4>();
			Skeleton newSkeleton = new Skeleton(skeleton.Name);
			// Construct new versions of the bones, and build
			// the inverse bind matrix that will be needed.
			for (ushort i = 0; i < skeleton.BoneCount; ++i) {
				Bone bone = skeleton.GetBone(i);
				Bone newBone = newSkeleton.CreateBone(bone.Name, bone.Handle);
				fullInverseBoneTransforms[bone.Name] = 
					bone.BindDerivedInverseTransform * invExportTransform;
			}
			//  Build the parenting relationship for the new skeleton
			for (ushort i = 0; i < skeleton.BoneCount; ++i) {
				Bone bone = skeleton.GetBone(i);
				Bone newBone = newSkeleton.GetBone(i);
				Bone parentBone = (Bone)bone.Parent;
				if (parentBone != null) {
					Bone newParentBone = newSkeleton.GetBone(parentBone.Handle);
					newParentBone.AddChild(newBone);
				}
			}
			// Set the orientation and position for the various bones
			for (ushort i = 0; i < newSkeleton.BoneCount; ++i) {
				Bone bone = skeleton.GetBone(i);
				string boneName = bone.Name;
				string parentName = (bone.Parent == null) ? null : bone.Parent.Name;
				Matrix4 transform = GetLocalBindMatrix(fullInverseBoneTransforms, boneName, parentName, true);
				Quaternion orientation = GetRotation(transform);
				Bone newBone = newSkeleton.GetBone(i);
				newBone.Orientation = orientation;
				// newBone.Scale = transform.Scale;
				newBone.Position = scale * transform.Translation;
			}
			newSkeleton.SetBindingPose();
			for (int i = 0; i < skeleton.AnimationCount; ++i) {
				Animation anim = skeleton.GetAnimation(i);
				Animation newAnim = newSkeleton.CreateAnimation(anim.Name, anim.Length);
				TransformAnimation(unscaledTransform, scale, newAnim, anim, newSkeleton);
			}
			skeleton = newSkeleton;
		}
		
		public List<Bone> GetOrderedBoneList(Bone parentBone, Skeleton skel) {
			List<Bone> rv = new List<Bone>();
			// Get all the bones that are my children
			List<Bone> childBones = new List<Bone>();
			for (ushort i = 0; i < skel.BoneCount; ++i) {
				Bone bone = skel.GetBone(i);
				if (bone.Parent == parentBone)
					childBones.Add(bone);
			}
			rv.AddRange(childBones);
			// For each of those bones, get their descendents
			foreach (Bone childBone in childBones) {
				List<Bone> bones = GetOrderedBoneList(childBone, skel);
				rv.AddRange(bones);
			}
			return rv;
		}
			
		protected void TransformAnimation(Matrix4 unscaledTransform, float scale,
										  Animation newAnim, Animation anim,
										  Skeleton newSkeleton) {
			// With the new idea I had for transforming these, I need the tracks
			// set up for the parent bones before I can handle the child bones.
			for (int i = 0; i < anim.Tracks.Count; ++i) {
				AnimationTrack track = anim.Tracks[i];
				AnimationTrack newTrack = newAnim.CreateTrack(track.Handle);
				Bone targetBone = (Bone)track.TargetNode;
				newTrack.TargetNode = newSkeleton.GetBone(targetBone.Handle);
			}
			// This gets the ordered bone list, and transforms the tracks in 
			// that order instead.
			List<Bone> orderedBoneList = GetOrderedBoneList(null, newSkeleton);
			foreach (Bone bone in orderedBoneList)
				TransformTrack(unscaledTransform, scale, newAnim, anim, bone);
		}

		protected AnimationTrack GetBoneTrack(Animation anim, ushort boneHandle) {
			for (int i = 0; i < anim.Tracks.Count; ++i) {
				AnimationTrack track = anim.Tracks[i];
				Bone bone = (Bone)track.TargetNode;
				if (bone.Handle == boneHandle)
					return track;
			}
			return null;
		}

		protected void GetCompositeTransform(ref Quaternion orientation,
											 ref Vector3 translation,
											 Bone bone, Animation anim, int keyFrameIndex) {
			if (bone == null)
				return;
			Quaternion tmpOrient = Quaternion.Identity;
			Vector3 tmpTranslate = Vector3.Zero;
			GetCompositeTransform(ref tmpOrient, ref tmpTranslate, (Bone)bone.Parent, anim, keyFrameIndex);
			AnimationTrack track = GetBoneTrack(anim, bone.Handle);
			KeyFrame keyFrame = track.KeyFrames[keyFrameIndex];
			orientation = tmpOrient * bone.Orientation * keyFrame.Rotation;
			translation = tmpTranslate + bone.Position + keyFrame.Translate;
		}


		protected void TransformTrack(Matrix4 unscaledTransform, float scale,
									  Animation newAnim, Animation anim, Bone bone) {
			AnimationTrack track = GetBoneTrack(anim, bone.Handle);
			AnimationTrack newTrack = GetBoneTrack(newAnim, bone.Handle);
			Bone oldNode = (Bone)track.TargetNode;
			Bone newNode = (Bone)newTrack.TargetNode;
			Quaternion exportRotation = GetRotation(unscaledTransform);
			Vector3 exportTranslation = unscaledTransform.Translation;
			for (int i = 0; i < track.KeyFrames.Count; ++i) {
				KeyFrame keyFrame = track.KeyFrames[i];
				Quaternion oldOrientation = Quaternion.Identity;
				Vector3 oldTranslation = Vector3.Zero;
				// Now build the composite transform for the old node
				GetCompositeTransform(ref oldOrientation, ref oldTranslation, oldNode, anim, i);
				Quaternion targetOrientation = exportRotation * oldOrientation;
				Vector3 targetTranslation = exportTranslation + scale * (exportRotation * oldTranslation);
				KeyFrame newKeyFrame = newTrack.CreateKeyFrame(keyFrame.Time);
				// we have a parent - where is it?
				Quaternion parentOrientation = Quaternion.Identity;
				Vector3 parentTranslation = Vector3.Zero;
				GetCompositeTransform(ref parentOrientation, ref parentTranslation, (Bone)newNode.Parent, newAnim, i);
				newKeyFrame.Rotation = newNode.Orientation.Inverse() * parentOrientation.Inverse() * targetOrientation;
				newKeyFrame.Translate = (-1 * newNode.Position) + (-1 * parentTranslation) + targetTranslation;
			}
		}
#endif

		public static Matrix4 ScaleMatrix(Matrix4 transform, float scale) {
			Matrix4 rv = transform;
			for (int row = 0; row < 3; ++row) {
				for (int col = 0; col < 3; ++col) {
					rv[row, col] *= scale;
				}
			}
			return rv;
		}

		public static float GetScale(Matrix4 transform) {
			Matrix3 tmp =
				new Matrix3(transform.m00, transform.m01, transform.m02,
							transform.m10, transform.m11, transform.m12,
							transform.m20, transform.m21, transform.m22);
			return (float)Math.Pow(tmp.Determinant, 1 / 3.0f);
		}


		public static Quaternion GetRotation(Matrix4 transform) {
			Matrix3 tmp =
				new Matrix3(transform.m00, transform.m01, transform.m02,
						    transform.m10, transform.m11, transform.m12,
						    transform.m20, transform.m21, transform.m22);
			float scale = (float)Math.Pow(tmp.Determinant, 1 / 3.0f);
			tmp = tmp * scale;
			Quaternion rv = Quaternion.Identity;
			rv.FromRotationMatrix(tmp);
			return rv;
		}

		/// <summary>
		///   Get the transform of the child bone at bind pose relative to 
		///   the parent bone at bind pose.  Note that this could be different
		///   for a different controller, but that I assume that it will be
		///   the same for now, and just assert that all skinned objects use
		///   the same bind pose for the skeleton.
		///   Since the xml skeleton format does not support scale, these
		///   local bind matrices all contain the export scale.
		/// </summary>
		/// <param name="boneName">Name of the bone</param>
		/// <param name="parentName">Name of the parent bone</param>
		/// <param name="debug">Set this to true to dump the matrices</param>
		/// <returns></returns>
		protected Matrix4 GetLocalBindMatrix(Dictionary<string, Matrix4> invBindMatrices,
											 string boneName,
											 string parentName,
											 bool debug) {
			if (!invBindMatrices.ContainsKey(boneName)) {
				log.WarnFormat("No skin seems to use bone: {0}", boneName);
				return Matrix4.Identity;
			}
			Matrix4 transform = invBindMatrices[boneName].Inverse();
			log.DebugFormat("BIND_MATRIX[{0}] = \n{1}", boneName, transform);
			if (parentName != null) {
				Matrix4 parInvTrans = invBindMatrices[parentName];
				parInvTrans = parInvTrans * (1 / parInvTrans.Determinant);
				transform = parInvTrans * transform;
			}
            log.DebugFormat("LOCAL_BIND_MATRIX[{0}] = \n{1}", boneName, transform);
			return transform;
		}

		protected XmlElement WriteBone(Bone bone) {
			XmlElement node = document.CreateElement("bone");
			XmlAttribute attr;
			attr = document.CreateAttribute("id");
			attr.Value = bone.Handle.ToString();
			node.Attributes.Append(attr);
			attr = document.CreateAttribute("name");
			attr.Value = bone.Name;
			node.Attributes.Append(attr);

			XmlElement childNode;
			childNode = WritePosition(bone.Position);
			node.AppendChild(childNode);
			childNode = WriteRotation(bone.Orientation);
			node.AppendChild(childNode);
		
			return node;
		}

		protected XmlElement WritePosition(Vector3 pos) {
			return WriteVector3("position", pos);
		}

		protected XmlElement WriteRotation(Quaternion rot) {
			return WriteQuaternion("rotation", rot);
		}

		protected XmlElement WriteRotate(Quaternion rot) {
			return WriteQuaternion("rotate", rot);
		}

		protected XmlElement WriteQuaternion(string elementName, Quaternion rot) {
			Vector3 axis = new Vector3();
			float angle = 0;
			rot.ToAngleAxis(ref angle, ref axis);
			XmlElement node = document.CreateElement(elementName);
			XmlAttribute attr;
			attr = document.CreateAttribute("angle");
			if (angle >= Math.PI &&
				2 * (float)Math.PI - angle < (float)Math.PI) 
			{
				angle = 2 * (float)Math.PI - angle;
				axis = -1 * axis;
				Debug.Assert(angle < Math.PI);
			}
			Debug.Assert(angle < Math.PI + .0001);
			attr.Value = angle.ToString();
			node.Attributes.Append(attr);
			XmlElement childNode = WriteAxis(axis);
			node.AppendChild(childNode);
			return node;
		}

		protected XmlElement WriteAxis(Vector3 axis) {
			return WriteVector3("axis", axis);
		}

		protected XmlElement WriteTranslate(Vector3 axis) {
			return WriteVector3("translate", axis);
		}

		protected XmlElement WriteVector3(string elementName, Vector3 vec) {
			XmlElement node = document.CreateElement(elementName);
			XmlAttribute attr;
			attr = document.CreateAttribute("x");
			attr.Value = vec.x.ToString();
			node.Attributes.Append(attr);
			attr = document.CreateAttribute("y");
			attr.Value = vec.y.ToString();
			node.Attributes.Append(attr);
			attr = document.CreateAttribute("z");
			attr.Value = vec.z.ToString();
			node.Attributes.Append(attr);
			return node;
		}

		protected XmlElement WriteAnimations() {
			XmlElement node = document.CreateElement("animations");
			for (int i = 0; i < skeleton.AnimationCount; ++i) {
				Animation anim = skeleton.GetAnimation(i);
				XmlElement animNode = WriteAnimation(anim);
				node.AppendChild(animNode);
			}
			return node;
		}

		protected XmlElement WriteAnimation(Animation anim) {
			XmlElement node = document.CreateElement("animation");
			XmlAttribute attr;
			attr = document.CreateAttribute("name");
			attr.Value = anim.Name;
			node.Attributes.Append(attr);
			attr = document.CreateAttribute("length");
			attr.Value = anim.Length.ToString();
			node.Attributes.Append(attr);
			XmlElement tracksNode = document.CreateElement("tracks");
			foreach (NodeAnimationTrack track in anim.NodeTracks.Values) {
				XmlElement trackNode = WriteTrack(track);
				tracksNode.AppendChild(trackNode);
			}
			node.AppendChild(tracksNode);
			return node;
		}

		protected XmlElement WriteTrack(NodeAnimationTrack track) {
			XmlElement node = document.CreateElement("track");
			XmlAttribute attr;
			attr = document.CreateAttribute("bone");
			attr.Value = track.TargetNode.Name;
			node.Attributes.Append(attr);
			XmlElement keyFramesNode = document.CreateElement("keyframes");
			foreach (KeyFrame baseKeyFrame in track.KeyFrames) {
                TransformKeyFrame keyFrame = (TransformKeyFrame)baseKeyFrame;
				//if (track.TargetNode.Parent != null)
				//    Debug.Assert(keyFrame.Translate.LengthSquared < .01);
				XmlElement keyFrameNode = WriteKeyFrame(keyFrame);
				keyFramesNode.AppendChild(keyFrameNode);
			}
			node.AppendChild(keyFramesNode);
			return node;
		}

		protected XmlElement WriteKeyFrame(TransformKeyFrame keyFrame) {
			XmlElement node = document.CreateElement("keyframe");
			XmlAttribute attr;
			attr = document.CreateAttribute("time");
			attr.Value = keyFrame.Time.ToString();
			node.Attributes.Append(attr);
			XmlElement translate = WriteTranslate(keyFrame.Translate);
			node.AppendChild(translate);
			XmlElement rotate = WriteRotate(keyFrame.Rotation);
			node.AppendChild(rotate);
			return node;
		}
		
		#endregion

		#region Properties

		#endregion
	}
}
