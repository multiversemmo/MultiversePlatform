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

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

using Axiom.Animating;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;

namespace Multiverse.Serialization {
    /// <summary>
    /// 	Summary description for OgreXmlSkeletonReader.
    /// </summary>
    public class OgreXmlSkeletonReader {
        #region Member variables

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OgreXmlSkeletonReader));
		
        private Skeleton skeleton;
		protected Stream stream;

		#endregion
		
        #region Constructors
		
        public OgreXmlSkeletonReader(Stream data) {
			stream = data;
        }
		
        #endregion
		
        #region Methods

		protected void DebugMessage(XmlNode node) {
			if (node.NodeType == XmlNodeType.Comment)
				return;
            log.InfoFormat("Unhandled node type: {0} with parent of {1}", node.Name, node.ParentNode.Name);
        }

		protected void DebugMessage(XmlNode node, XmlAttribute attr) {
            log.InfoFormat("Unhandled node attribute: {0} with parent node of {1}", attr.Name, node.Name);
		}

        public void Import(Skeleton skeleton) {
			// store a local reference to the skeleton for modification
			this.skeleton = skeleton;

			XmlDocument document = new XmlDocument();
			document.Load(stream);
			foreach (XmlNode childNode in document.ChildNodes) {
				switch (childNode.Name) {
					case "skeleton":
						ReadSkeleton(childNode);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
			skeleton.SetBindingPose();
		}

        /// <summary>
        ///    Reads bone information from the file.
        /// </summary>
		protected void ReadSkeleton(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "bones":
						ReadBones(childNode);
						break;
					case "bonehierarchy":
						ReadBoneHierarchy(childNode);
						break;
					case "animations":
						ReadAnimations(childNode);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadBones(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "bone":
						ReadBone(childNode);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadBoneHierarchy(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "boneparent":
						ReadBoneParent(childNode);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadAnimations(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "animation":
						ReadAnimation(childNode);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		/// <summary>
        ///    Reads bone information from the file.
        /// </summary>
        protected void ReadBone(XmlNode node) {
            // bone name
			string name = node.Attributes["name"].Value;
			ushort handle = ushort.Parse(node.Attributes["id"].Value);

            // create a new bone
            Bone bone = skeleton.CreateBone(name, handle);

			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "position":
						ReadPosition(childNode, bone);
						break;
					case "rotation":
						ReadRotation(childNode, bone);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

        /// <summary>
        ///    Reads bone parent information from the file.
        /// </summary>
		protected void ReadBoneParent(XmlNode node) {
			string childName = node.Attributes["bone"].Value;
			string parentName = node.Attributes["parent"].Value;
			// get references to father and son bones
			Bone child = skeleton.GetBone(childName);
			Bone parent = skeleton.GetBone(parentName);
			// attach the child to the parent
			parent.AddChild(child);
		}

		protected void ReadAnimation(XmlNode node) {
			string name = node.Attributes["name"].Value;
			float length = float.Parse(node.Attributes["length"].Value);
			// create an animation from the skeleton
			Animation anim = skeleton.CreateAnimation(name, length);

			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "tracks":
						ReadTracks(childNode, anim);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected Vector3 ReadVector3(XmlNode node) {
			Vector3 vec;
			vec.x = float.Parse(node.Attributes["x"].Value);
			vec.y = float.Parse(node.Attributes["y"].Value);
			vec.z = float.Parse(node.Attributes["z"].Value);
			return vec;
		}

		protected void ReadPosition(XmlNode node, Bone bone) {
			bone.Position = ReadVector3(node);
		}

		protected void ReadRotation(XmlNode node, Bone bone) {
			float angle = float.Parse(node.Attributes["angle"].Value);
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "axis":
						Vector3 axis = ReadVector3(childNode);
						bone.Orientation = Quaternion.FromAngleAxis(angle, axis);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadTracks(XmlNode node, Animation anim) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "track":
						ReadTrack(childNode, anim);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadTrack(XmlNode node, Animation anim) {
			string boneName = node.Attributes["bone"].Value;
			// get a reference to the target bone
			Bone targetBone = skeleton.GetBone(boneName);
			// create an animation track for this bone
			NodeAnimationTrack track = anim.CreateNodeTrack(targetBone.Handle, targetBone);

			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "keyframes":
						ReadKeyFrames(childNode, track);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadKeyFrames(XmlNode node, AnimationTrack track) {
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "keyframe":
						ReadKeyFrame(childNode, track);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadKeyFrame(XmlNode node, AnimationTrack track) {
			float time = float.Parse(node.Attributes["time"].Value);
			// create a new keyframe with the specified length
			TransformKeyFrame keyFrame = (TransformKeyFrame)track.CreateKeyFrame(time);

			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "translate":
						keyFrame.Translate = ReadVector3(childNode);
						break;
					case "rotate":
						ReadRotate(childNode, keyFrame);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}

		protected void ReadRotate(XmlNode node, TransformKeyFrame keyFrame) {
			float angle = float.Parse(node.Attributes["angle"].Value);
			foreach (XmlNode childNode in node.ChildNodes) {
				switch (childNode.Name) {
					case "axis":
						Vector3 axis = ReadVector3(childNode);
						keyFrame.Rotation = Quaternion.FromAngleAxis(angle, axis);
						break;
					default:
						DebugMessage(childNode);
						break;
				}
			}
		}


        #endregion Methods
    }
}

