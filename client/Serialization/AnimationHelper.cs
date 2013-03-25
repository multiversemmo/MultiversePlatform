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
using System.Collections.Generic;
using System.Text;

using Axiom.MathLib;
using Axiom.Animating;

namespace Multiverse.Serialization {
    public class AnimationHelper {
        // If two scale vectors are this close, consider them to be the same
        public const float ScaleEpsilon = 0.0001f;
        // If the angle between two vectors is this small, consider it zero
        public const float RotateEpsilon = 0.0001f;
        // If two translate vectors are this close, consider them to be the same
        public const float TranslateEpsilon = 0.0001f;


        public static void DuplicateKeyFrame(NodeAnimationTrack track, TransformKeyFrame orig) {
            TransformKeyFrame newKeyFrame = (TransformKeyFrame)track.CreateKeyFrame(orig.Time);
            newKeyFrame.Scale = orig.Scale;
            newKeyFrame.Rotation = orig.Rotation;
            newKeyFrame.Translate = orig.Translate;
        }

        public static bool CompareKeyFrames(TransformKeyFrame keyFrame1, TransformKeyFrame keyFrame2) {
            Vector3 delta = keyFrame1.Translate - keyFrame2.Translate;
            if (delta.LengthSquared > (TranslateEpsilon * TranslateEpsilon))
                return false;
            Quaternion rot = keyFrame1.Rotation * keyFrame2.Rotation.Inverse();
            Vector3 axis = Vector3.Zero;
            float angle = 0;
            rot.ToAngleAxis(ref angle, ref axis);
            if (Math.Abs(angle) > RotateEpsilon)
                return false;
            delta = keyFrame1.Scale - keyFrame2.Scale;
            if (delta.LengthSquared > (ScaleEpsilon * ScaleEpsilon))
                return false;
            return true;
        }

        public static void CleanupAnimation(Skeleton skel, string anim_name) {
            Animation anim = skel.GetAnimation(anim_name);
            CleanupAnimation(skel, anim);
        }

        public static void CleanupAnimation(Skeleton skel, Animation anim) {
            Animation newAnim = skel.CreateAnimation("_replacement", anim.Length);
            Animation tmpAnim = skel.CreateAnimation("_temporary", anim.Length);
            foreach (NodeAnimationTrack track in anim.NodeTracks.Values) {
                Bone bone = skel.GetBone((ushort)track.Handle);
                NodeAnimationTrack newTrack = newAnim.CreateNodeTrack(track.Handle, bone);
                NodeAnimationTrack tmpTrack = tmpAnim.CreateNodeTrack(track.Handle, bone);
                int maxFrame = track.KeyFrames.Count;
                int lastKeyFrame = -1;
                for (int keyFrameIndex = 0; keyFrameIndex < track.KeyFrames.Count; ++keyFrameIndex) {
                    if (anim.InterpolationMode == InterpolationMode.Linear) {
                        // Linear is based on one point before and one after.
                        TransformKeyFrame cur = track.GetTransformKeyFrame(keyFrameIndex);
                        if (keyFrameIndex == 0 ||
                            keyFrameIndex == (track.KeyFrames.Count - 1)) {
                            // Add the key frame if it is the first or last keyframe.
                            lastKeyFrame = keyFrameIndex;
                            DuplicateKeyFrame(newTrack, cur);
                        } else {
                            // Make sure tmpTrack is clean.. we just use it for interpolation
                            tmpTrack.RemoveAllKeyFrames();
                            TransformKeyFrame prior = track.GetTransformKeyFrame(lastKeyFrame);
                            TransformKeyFrame next = track.GetTransformKeyFrame(keyFrameIndex + 1);
                            DuplicateKeyFrame(tmpTrack, prior);
                            DuplicateKeyFrame(tmpTrack, next);
                            // Check to see if removing this last keyframe will throw off
                            // any of the other keyframes that were considered redundant.
                            bool needKeyFrame = false;
                            for (int i = lastKeyFrame + 1; i <= keyFrameIndex; ++i) {
                                TransformKeyFrame orig = track.GetTransformKeyFrame(i);
                                TransformKeyFrame interp = new TransformKeyFrame(tmpTrack, orig.Time);
                                tmpTrack.GetInterpolatedKeyFrame(orig.Time, interp);
                                // Is this interpolated frame useful or redundant?
                                if (!CompareKeyFrames(interp, cur)) {
                                    needKeyFrame = true;
                                    break;
                                }
                            }
                            if (needKeyFrame) {
                                lastKeyFrame = keyFrameIndex;
                                DuplicateKeyFrame(newTrack, cur);
                            }
                        }
                    } else if (anim.InterpolationMode == InterpolationMode.Spline) {
                        // Spline is based on two points before and two after.
                        TransformKeyFrame cur = track.GetTransformKeyFrame(keyFrameIndex);
#if DISABLED_CODE
                        if (keyFrameIndex == 0 ||
                            keyFrameIndex == 1 ||
                            keyFrameIndex == (track.KeyFrames.Count - 1) ||
                            keyFrameIndex == (track.KeyFrames.Count - 2)) {
                            // Add the key frame if it is the first, second, last or second to last keyframe.
                            DuplicateKeyFrame(newTrack, cur);
                        } else {
                            // Make sure tmpTrack is clean.. we just use it for interpolation
                            tmpTrack.RemoveAllKeyFrames();
                            TransformKeyFrame prior1 = track.GetTransformKeyFrame(keyFrameIndex - 2);
                            TransformKeyFrame prior2 = track.GetTransformKeyFrame(keyFrameIndex - 1);
                            TransformKeyFrame next1 = track.GetTransformKeyFrame(keyFrameIndex + 1);
                            TransformKeyFrame next2 = track.GetTransformKeyFrame(keyFrameIndex + 2);
                            DuplicateKeyFrame(tmpTrack, prior1);
                            DuplicateKeyFrame(tmpTrack, prior2);
                            DuplicateKeyFrame(tmpTrack, next1);
                            DuplicateKeyFrame(tmpTrack, next2);
                            TransformKeyFrame interp = new TransformKeyFrame(tmpTrack, cur.Time);
                            tmpTrack.GetInterpolatedKeyFrame(cur.Time, interp);
                            // Is this interpolated frame useful or redundant?
                            if (!CompareKeyFrames(interp, cur))
                                DuplicateKeyFrame(newTrack, cur);
                        }
#else
                        DuplicateKeyFrame(newTrack, cur);
#endif
                    } else {
                        System.Diagnostics.Debug.Assert(false, "Invalid InterpolationMode: " + anim.InterpolationMode);
                    }
                }
            }
            skel.RemoveAnimation(tmpAnim.Name);
            skel.RemoveAnimation(newAnim.Name);
            skel.RemoveAnimation(anim.Name);
            
            // Recreate the animation with the proper name (awkward)
            anim = skel.CreateAnimation(anim.Name, anim.Length);
            foreach (NodeAnimationTrack track in newAnim.NodeTracks.Values) {
                Bone bone = skel.GetBone((ushort)track.Handle);
                NodeAnimationTrack newTrack = anim.CreateNodeTrack(track.Handle, bone);
                foreach (KeyFrame keyFrame in track.KeyFrames)
                    DuplicateKeyFrame(newTrack, (TransformKeyFrame)keyFrame);
            }
        }
    }
}
