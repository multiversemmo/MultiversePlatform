using System;
using System.IO;
using System.Text;
using Axiom.Animating;
using Axiom.MathLib;

namespace Axiom.Serialization {
    /// <summary>
    /// 	Summary description for OgreSkeletonSerializer.
    /// </summary>
    public class OgreSkeletonSerializer : Serializer {
        #region Member variables
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OgreSkeletonSerializer));

        private Skeleton skeleton;

        #endregion
		
        #region Constructors

        public OgreSkeletonSerializer() {
            version = "[Serializer_v1.10]";
        }
		
        #endregion
		
        #region Methods

        public void ImportSkeleton(Stream stream, Skeleton skeleton) {
            // store a local reference to the mesh for modification
            this.skeleton = skeleton;

            BinaryMemoryReader reader = new BinaryMemoryReader(stream, System.Text.Encoding.ASCII);

            // start off by taking a look at the header
            ReadFileHeader(reader);

            SkeletonChunkID chunkID = 0;

            while (!IsEOF(reader)) {
                chunkID = ReadChunk(reader);

                switch (chunkID) {
                    case SkeletonChunkID.Bone:
                        ReadBone(reader);
                        break;

                    case SkeletonChunkID.BoneParent:
                        ReadBoneParent(reader);
                        break;

                    case SkeletonChunkID.Animation:
                        ReadAnimation(reader);
                        break;

                    case SkeletonChunkID.AttachmentPoint:
                        ReadAttachmentPoint(reader);
                        break;

                    default:
                        log.Warn("Can only parse bones, parents, and animations at the top level during skeleton loading.");
                        log.Warn("Unexpected chunk: " + chunkID.ToString());
                        break;
                } // switch
            } // while

            // assume bones are stored in binding pose
            skeleton.SetBindingPose();
        }

        protected SkeletonChunkID ReadChunk(BinaryMemoryReader reader) {
            return (SkeletonChunkID)ReadFileChunk(reader);
        }

        /// <summary>
        ///    Reads animation information from the file.
        /// </summary>
        protected void ReadAnimation(BinaryMemoryReader reader) {
            // name of the animation
            string name = ReadString(reader);

            // length in seconds of the animation
            float length = ReadFloat(reader);

            // create an animation from the skeleton
            Animation anim = skeleton.CreateAnimation(name, length);

            // keep reading all keyframes for this track
            if (!IsEOF(reader)) {
                SkeletonChunkID chunkID = ReadChunk(reader);
                while (!IsEOF(reader) && (chunkID == SkeletonChunkID.AnimationTrack)) {
                    // read the animation track
                    ReadAnimationTrack(reader, anim);
                    // read the next chunk id
                    // If we're not end of file get the next chunk ID
                    if (!IsEOF(reader)) {
                        chunkID = ReadChunk(reader);
                    }
                }
                // backpedal to the start of the chunk
                if (!IsEOF(reader)) {
                    Seek(reader, -ChunkOverheadSize);
                }
            }
        }

        /// <summary>
        ///    Reads an animation track.
        /// </summary>
        protected void ReadAnimationTrack(BinaryMemoryReader reader, Animation anim) {
            // read the bone handle to apply this track to
            ushort boneHandle = ReadUShort(reader);

            // get a reference to the target bone
            Bone targetBone = skeleton.GetBone(boneHandle);

            // create an animation track for this bone
            NodeAnimationTrack track = anim.CreateNodeTrack(boneHandle, targetBone);

            // keep reading all keyframes for this track
            if (!IsEOF(reader)) {
                SkeletonChunkID chunkID = ReadChunk(reader);
                while (!IsEOF(reader) && (chunkID == SkeletonChunkID.KeyFrame)) {
                    // read the key frame
                    ReadKeyFrame(reader, track);
                    // read the next chunk id
                    // If we're not end of file get the next chunk ID
                    if (!IsEOF(reader)) {
                        chunkID = ReadChunk(reader);
                    }
                }
                // backpedal to the start of the chunk
                if (!IsEOF(reader)) {
                    Seek(reader, -ChunkOverheadSize);
                }
            }
        }

        /// <summary>
        ///    Reads bone information from the file.
        /// </summary>
        protected void ReadBone(BinaryMemoryReader reader) {
            // bone name
            string name = ReadString(reader);

            ushort handle = ReadUShort(reader);

            // create a new bone
            Bone bone = skeleton.CreateBone(name, handle);

            // read and set the position of the bone
            Vector3 position = ReadVector3(reader);
            bone.Position = position;

            // read and set the orientation of the bone
            Quaternion q = ReadQuat(reader);
            bone.Orientation = q;
        }

        /// <summary>
        ///    Reads bone information from the file.
        /// </summary>
        protected void ReadBoneParent(BinaryMemoryReader reader) {
            // all bones should have been created by this point, so this establishes the heirarchy
            Bone child, parent;
            ushort childHandle, parentHandle;

            // child bone
            childHandle = ReadUShort(reader);

            // parent bone
            parentHandle = ReadUShort(reader);

            // get references to father and son bones
            parent = skeleton.GetBone(parentHandle);
            child = skeleton.GetBone(childHandle);

            // attach the child to the parent
            parent.AddChild(child);
        }

        /// <summary>
        ///    Reads an animation track section.
        /// </summary>
        /// <param name="track"></param>
        protected void ReadKeyFrame(BinaryMemoryReader reader, NodeAnimationTrack track) {
            float time = ReadFloat(reader);

            // create a new keyframe with the specified length
            TransformKeyFrame keyFrame = track.CreateNodeKeyFrame(time);

            // read orientation
            Quaternion rotate = ReadQuat(reader);
            keyFrame.Rotation = rotate;

			// read translation
			Vector3 translate = ReadVector3(reader);
			keyFrame.Translate = translate;

			// read scale if it is in there
			if (currentChunkLength >= 50) {
				Vector3 scale = ReadVector3(reader);
				keyFrame.Scale = scale;
			} else {
				keyFrame.Scale = Vector3.UnitScale;
			}
		}
        
        /// <summary>
        ///    Reads bone information from the file.
        /// </summary>
        protected void ReadAttachmentPoint(BinaryMemoryReader reader) {
            // bone name
            string name = ReadString(reader);

            ushort boneHandle = ReadUShort(reader);

            // read and set the position of the bone
            Vector3 position = ReadVector3(reader);

            // read and set the orientation of the bone
            Quaternion q = ReadQuat(reader);

            // create the attachment point
            AttachmentPoint ap = skeleton.CreateAttachmentPoint(name, boneHandle, q, position);
        }

        public void ExportSkeleton(Skeleton skeleton, string fileName) {
            this.skeleton = skeleton;
            FileStream stream = new FileStream(fileName, FileMode.Create);
            try {
                BinaryWriter writer = new BinaryWriter(stream);
                WriteFileHeader(writer, version);
                WriteSkeleton(writer);
            } finally {
                if (stream != null)
                    stream.Close();
            }
        }

        protected void WriteSkeleton(BinaryWriter writer) {
            for (ushort i = 0; i < skeleton.BoneCount; ++i) {
                Bone bone = skeleton.GetBone(i);
                WriteBone(writer, bone);
            }

            for (ushort i = 0; i < skeleton.BoneCount; ++i) {
                Bone bone = skeleton.GetBone(i);
                if (bone.Parent != null)
                    WriteBoneParent(writer, bone, (Bone)bone.Parent);
            }

            for (int i = 0; i < skeleton.AnimationCount; ++i) {
                Animation anim = skeleton.GetAnimation(i);
                WriteAnimation(writer, anim);
            }

            for (int i = 0; i < skeleton.AttachmentPoints.Count; ++i) {
                AttachmentPoint ap = skeleton.AttachmentPoints[i];
                WriteAttachmentPoint(writer, ap, skeleton.GetBone(ap.ParentBone));
            }
        }

        protected void WriteBone(BinaryWriter writer, Bone bone) {
            long start_offset = writer.Seek(0, SeekOrigin.Current);
            WriteChunk(writer, SkeletonChunkID.Bone, 0);

            WriteString(writer, bone.Name);
            WriteUShort(writer, bone.Handle);
            WriteVector3(writer, bone.Position);
            WriteQuat(writer, bone.Orientation);
            if (bone.ScaleFactor != Vector3.UnitScale)
                WriteVector3(writer, bone.ScaleFactor);

            long end_offset = writer.Seek(0, SeekOrigin.Current);
            writer.Seek((int)start_offset, SeekOrigin.Begin);
            WriteChunk(writer, SkeletonChunkID.Bone, (int)(end_offset - start_offset));
            writer.Seek((int)end_offset, SeekOrigin.Begin);
        }

        protected void WriteBoneParent(BinaryWriter writer, Bone bone, Bone parent) {
            long start_offset = writer.Seek(0, SeekOrigin.Current);
            WriteChunk(writer, SkeletonChunkID.BoneParent, 0);

            WriteUShort(writer, bone.Handle);
            WriteUShort(writer, parent.Handle);

            long end_offset = writer.Seek(0, SeekOrigin.Current);
            writer.Seek((int)start_offset, SeekOrigin.Begin);
            WriteChunk(writer, SkeletonChunkID.BoneParent, (int)(end_offset - start_offset));
            writer.Seek((int)end_offset, SeekOrigin.Begin);
        }

        protected void WriteAnimation(BinaryWriter writer, Animation anim) {
            long start_offset = writer.Seek(0, SeekOrigin.Current);
            WriteChunk(writer, SkeletonChunkID.Animation, 0);

            WriteString(writer, anim.Name);
            WriteFloat(writer, anim.Length);

            foreach (NodeAnimationTrack track in anim.NodeTracks.Values)
                WriteAnimationTrack(writer, track);

            long end_offset = writer.Seek(0, SeekOrigin.Current);
            writer.Seek((int)start_offset, SeekOrigin.Begin);
            WriteChunk(writer, SkeletonChunkID.Animation, (int)(end_offset - start_offset));
            writer.Seek((int)end_offset, SeekOrigin.Begin);
        }

        protected void WriteAnimationTrack(BinaryWriter writer, NodeAnimationTrack track) {
            long start_offset = writer.Seek(0, SeekOrigin.Current);
            WriteChunk(writer, SkeletonChunkID.AnimationTrack, 0);

            WriteUShort(writer, (ushort)track.Handle);
            for (ushort i = 0; i < track.KeyFrames.Count; i++) {
                TransformKeyFrame keyFrame = track.GetNodeKeyFrame(i);
                WriteKeyFrame(writer, keyFrame);
            }
            long end_offset = writer.Seek(0, SeekOrigin.Current);
            writer.Seek((int)start_offset, SeekOrigin.Begin);
            WriteChunk(writer, SkeletonChunkID.AnimationTrack, (int)(end_offset - start_offset));
            writer.Seek((int)end_offset, SeekOrigin.Begin);
        }

        protected void WriteKeyFrame(BinaryWriter writer, TransformKeyFrame keyFrame) {
            long start_offset = writer.Seek(0, SeekOrigin.Current);
            WriteChunk(writer, SkeletonChunkID.KeyFrame, 0);

            WriteFloat(writer, keyFrame.Time);
            WriteQuat(writer, keyFrame.Rotation);
            WriteVector3(writer, keyFrame.Translate);
            if (keyFrame.Scale != Vector3.UnitScale)
                WriteVector3(writer, keyFrame.Scale);

            long end_offset = writer.Seek(0, SeekOrigin.Current);
            writer.Seek((int)start_offset, SeekOrigin.Begin);
            WriteChunk(writer, SkeletonChunkID.KeyFrame, (int)(end_offset - start_offset));
            writer.Seek((int)end_offset, SeekOrigin.Begin);
        }

        protected void WriteAttachmentPoint(BinaryWriter writer, AttachmentPoint ap, Bone bone) {
            long start_offset = writer.Seek(0, SeekOrigin.Current);
            WriteChunk(writer, SkeletonChunkID.AttachmentPoint, 0);

            WriteString(writer, ap.Name);
            WriteUShort(writer, bone.Handle);
            WriteVector3(writer, ap.Position);
            WriteQuat(writer, ap.Orientation);

            long end_offset = writer.Seek(0, SeekOrigin.Current);
            writer.Seek((int)start_offset, SeekOrigin.Begin);
            WriteChunk(writer, SkeletonChunkID.AttachmentPoint, (int)(end_offset - start_offset));
            writer.Seek((int)end_offset, SeekOrigin.Begin);
        }

        #endregion Methods
    }

    /// <summary>
    ///    Chunk ID's that can be found within the Ogre .skeleton format.
    /// </summary>
    public enum SkeletonChunkID {
        Header                 = 0x1000,
        Bone                   = 0x2000,
        BoneParent             = 0x3000,
        Animation              = 0x4000,
        AnimationTrack         = 0x4100,
        KeyFrame               = 0x4110,
        // TODO: AnimationLink = 0x5000,
        // Multiverse Addition
        AttachmentPoint        = 0x6000,
    }
}
