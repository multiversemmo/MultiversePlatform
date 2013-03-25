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
using Axiom.Graphics;
using Axiom.Core;

using Axiom.MathLib;

namespace Axiom.Animating {
    /// <summary>
    ///		A 'track' in an animation sequence, ie a sequence of keyframes which affect a
    ///		certain type of object that can be animated.
    /// </summary>
    /// <remarks>
    ///		This class is intended as a base for more complete classes which will actually
    ///		animate specific types of object, e.g. a bone in a skeleton to affect
    ///		skeletal animation. An animation will likely include multiple tracks each of which
    ///		can be made up of many KeyFrame instances. Note that the use of tracks allows each animable
    ///		object to have it's own number of keyframes, i.e. you do not have to have the
    ///		maximum number of keyframes for all animable objects just to cope with the most
    ///		animated one.
    ///		<p/>
    ///		Since the most common animable object is a Node, there are options in this class for associating
    ///		the track with a Node which will receive keyframe updates automatically when the 'apply' method
    ///		is called.
    /// </remarks>
    public abstract class AnimationTrack {
        #region Fields

        /// <summary>
        ///		Handle of this animation track.
        ///	</summary>
        protected ushort handle;
        /// <summary>
        ///		Animation that owns this track.
        ///	</summary>
        protected Animation parent;
        /// <summary>
        ///		Maximum keyframe time.
        ///	</summary>
        protected float maxKeyFrameTime;
        /// <summary>
        ///		Collection of key frames in this track.
        ///	</summary>
        protected List<KeyFrame> keyFrameList = new List<KeyFrame>();
        /// <summary>
        ///		Provides fast lookup of frames at a given time.
        ///	</summary>
        protected float[] keyFrameIndex = null;
        /// <summary>
        ///		True if we're supposed to use the keyFrameIndex
        ///	</summary>
        protected bool useKeyFrameIndex = true;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///		Internal constructor, to prevent direction instantiation.  Should be created
        ///		via a call to the CreateTrack method of an Animation.
        /// </summary>
        internal AnimationTrack(Animation parent) : this(parent, 0) {
        }

        internal AnimationTrack(Animation parent, ushort handle) {
            this.parent = parent;
            this.handle = handle;
            maxKeyFrameTime = float.MinValue;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///		The name of this animation track.
        /// </summary>
        public ushort Handle {
            get { 
                return handle; 
            }
            set { 
                handle = value; 
            }
        }

        /// <summary>
        ///		Collection of the KeyFrames present in this AnimationTrack.
        /// </summary>
        public List<KeyFrame> KeyFrames {
            get { 
                return keyFrameList; 
            }
        }

        #endregion Properties

        #region Abstract methods

        /// <summary>
        ///     Gets a KeyFrame object which contains the interpolated transforms at the time index specified.
        /// </summary>
        /// <remarks>
        ///    The KeyFrame objects held by this class are transformation snapshots at 
        ///    discrete points in time. Normally however, you want to interpolate between these
        ///    keyframes to produce smooth movement, and this method allows you to do this easily.
        ///    In animation terminology this is called 'tweening'. 
        /// <param name="time">The time (in relation to the whole animation sequence)</param>
        ///	<param name="kf">Keyframe object to store results </param>
        public abstract KeyFrame GetInterpolatedKeyFrame(float time, KeyFrame kf);

        /// <summary>
        ///		Applies an animation track to the designated target.
        /// </summary>
        /// <param name="time">The time position in the animation to apply.</param>
        /// <param name="weight">The influence to give to this track, 1.0 for full influence, 
        ///	   less to blend with other animations.</param>
        /// <param name="acculumate">Don't make weights relative to overall weights applied,
        ///    make them absolute and just add. </param>          
        /// <param name="scale">The scale to apply to translations and scalings, useful for 
        ///	   adapting an animation to a different size target.</param>
        public abstract void Apply(float time, float weight, bool accumulate, float scale); 

        /// <summary>
        ///		Create a keyframe implementation - must be overridden
        /// </summary>
        public abstract KeyFrame CreateKeyFrameImpl(float time);

        #endregion Abstract methods
        
        #region Public methods

        /// <summary>
        ///     Gets the 2 KeyFrame objects which are active at the time given, and the blend value between them.
        /// </summary>
        /// <remarks>
        ///     At any point in time  in an animation, there are either 1 or 2 keyframes which are 'active',
        ///     1 if the time index is exactly on a keyframe, 2 at all other times i.e. the keyframe before
        ///     and the keyframe after.
        /// </remarks>
        /// <param name="time">The time index in seconds.</param>
        /// <param name="keyFrame1">Receive the keyframe just before or at this time index.</param>
        /// <param name="keyFrame2">Receive the keyframe just after this time index.</param>
        /// <param name="firstKeyIndex">If supplied, will receive the index of the 'from' keyframe incase the caller needs it.</param>
        /// <returns>
        ///     Parametric value indicating how far along the gap between the 2 keyframes the time
        ///    value is, e.g. 0.0 for exactly at 1, 0.25 for a quarter etc. By definition the range of this 
        ///    value is:  0.0 &lt;= returnValue &lt; 1.0 .
        ///</returns>
        public float GetKeyFramesAtTime(float time, out KeyFrame keyFrame1, out KeyFrame keyFrame2, out ushort firstKeyIndex) {
            short firstIndex = -1;
            float totalLength = parent.Length;

            // wrap time
            while(time > totalLength)
                time -= totalLength;

            int i = 0;

            // makes compiler happy so it wont complain about this var being unassigned
            keyFrame1 = null;

            if (useKeyFrameIndex) {
                if (keyFrameIndex == null)
                    rebuildKeyFrameIndex();
            
                int index = Array.BinarySearch<float>(keyFrameIndex, time);
                if (index < 0) {
                    index = ~index - 1;
                    if (index < 0)
                        index = 0;
                }
                firstIndex = (short)index;
                keyFrame1 = keyFrameList[index];
            }
            else {
                // The old linear search lookup
                // find the last keyframe before or on current time
                for(i = 0; i < keyFrameList.Count; i++) {
                    KeyFrame keyFrame = keyFrameList[i];

                    // kick out now if the current frames time is greater than the current time
                    if(keyFrame.time > time)
                        break;

                    keyFrame1 = keyFrame;
                    ++firstIndex;
                }
            }
            
            // trap case where there is no key before this time
            // use the first key anyway and pretend it's time index 0
            if(firstIndex == -1) {
                keyFrame1 = keyFrameList[0];
                ++firstIndex;
            }

            // fill index of the first key
            firstKeyIndex = (ushort)firstIndex;

            // parametric time
            // t1 = time of previous keyframe
            // t2 = time of next keyframe
            float t1, t2;

            // find first keyframe after the time
            // if no next keyframe, wrap back to first
            // TODO: Verify logic
            if(firstIndex == (keyFrameList.Count - 1)) {
                keyFrame2 = keyFrameList[0];
                t2 = totalLength;
            }
            else {
                keyFrame2 = keyFrameList[firstIndex + 1];
                t2 = keyFrame2.Time;
            }

            t1 = keyFrame1.Time;

            if(t1 == t2) {
                // same keyframe
                return 0.0f;
            }
            else {
                return (time - t1) / (t2 - t1);
            }
        }

        /// <summary>
        ///		Creates a new KeyFrame and adds it to this animation at the given time index.
        /// </summary>
        public TransformKeyFrame GetTransformKeyFrame(int index) {
            return (TransformKeyFrame)KeyFrames[index];
        }

        /// <summary>
        ///		Creates a new KeyFrame and adds it to this animation at the given time index.
        /// </summary>
        /// <remarks>
        ///     It is better to create KeyFrames in time order. Creating them out of order can result 
        ///     in expensive reordering processing. Note that a KeyFrame at time index 0.0 is always created
        ///     for you, so you don't need to create this one, just access it using KeyFrames[0];
        /// </remarks>
        /// <param name="time">Time within the animation at which this keyframe will lie.</param>
        /// <returns>A new KeyFrame.</returns>
        public KeyFrame CreateKeyFrame(float time) {
            KeyFrame keyFrame = CreateKeyFrameImpl(time);

            if(time > maxKeyFrameTime || (time == 0 && keyFrameList.Count == 0)) {
                keyFrameList.Add(keyFrame);
                maxKeyFrameTime = time;
            }
            else {
                // search for the correct place to insert the keyframe
                int i = 0;
                KeyFrame kf = keyFrameList[i];

                while(kf.Time < time && i != keyFrameList.Count) {
                    i++;
                }

                keyFrameList.Insert(i, kf);
            }

            // ensure a spline rebuild takes place
            OnKeyFrameDataChanged();

            return keyFrame;
        }

        /// <summary>
        ///     Removes the keyframe at the specified index.
        /// </summary>
        /// <param name="index">Index of the keyframe to remove from this track.</param>
        public void RemoveKeyFrame(int index) {
            Debug.Assert(index < keyFrameList.Count, "Index of of bounds when removing a key frame.");

            keyFrameList.RemoveAt(index);

            // ensure a spline rebuild takes place
            OnKeyFrameDataChanged();
        }

        /// <summary>
        ///     Removes all key frames from this animation track.
        /// </summary>
        public void RemoveAllKeyFrames() {
            keyFrameList.Clear();

            // ensure a spline rebuild takes place
            OnKeyFrameDataChanged();
        }

        /// <summary>
        ///		Overloaded Apply method.  
        /// </summary>
        /// <param name="time"></param>
        public void Apply(float time) {
            // call overloaded method
            Apply(time, 1.0f, false, 1.0f);
        }

        /// <summary>
        ///     Called internally when keyframes belonging to this track are changed, in order to
        ///     trigger a rebuild of the animation splines.
        /// </summary>
        public virtual void OnKeyFrameDataChanged() {
            keyFrameIndex = null;
            // we also need to rebuild the maxKeyFrameTime
            maxKeyFrameTime = float.MinValue;
            foreach (KeyFrame keyFrame in this.KeyFrames)
                if(keyFrame.Time > maxKeyFrameTime)
                    maxKeyFrameTime = keyFrame.Time;
        }
            
        /// <summary>
        ///     Method to determine if this track has any KeyFrames which are
        ///     doing anything useful - can be used to determine if this track
        ///     can be optimised out.
        /// </summary>
        public virtual bool HasNonZeroKeyFrames() {
        return true; 
        }

        /// <summary>
        ///		Optimise the current track by removing any duplicate keyframes.
        /// </summary>
           public virtual void Optimise() {}

        #endregion Public methods

        protected void rebuildKeyFrameIndex() {
            keyFrameIndex = new float[this.KeyFrames.Count];
            int i=0;
            foreach (KeyFrame keyFrame in this.KeyFrames)
                keyFrameIndex[i++] = keyFrame.Time;
        }
        
    }
    
    public class NumericAnimationTrack : AnimationTrack {
        
        protected AnimableValue targetAnimable;

        #region Constructor

        public NumericAnimationTrack(Animation parent) : base(parent) {
        }
        
        public NumericAnimationTrack(Animation parent, ushort handle) : base(parent, handle) {
        }
        
        public NumericAnimationTrack(Animation parent, AnimableValue targetAnimable) : base(parent) {
            this.targetAnimable = targetAnimable;
        }
        
        #endregion Constructor
    
        #region Properties

        /// <summary>
        ///		The aniable value with which this track is associated.
        /// </summary>
        public AnimableValue TargetAnimable {
            get { 
                return targetAnimable; 
            }
            set { 
                targetAnimable = value; 
            }
        }

        #endregion Properties
            
        #region Public methods

        public override KeyFrame CreateKeyFrameImpl(float time) {
            return new NumericKeyFrame(this, time);
        }

        public override KeyFrame GetInterpolatedKeyFrame(float timeIndex, KeyFrame kf) {
            NumericKeyFrame kret = (NumericKeyFrame)kf;

            // Keyframe pointers
            KeyFrame kBase1, kBase2;
            NumericKeyFrame k1, k2;
            ushort firstKeyIndex;

            float t = GetKeyFramesAtTime(timeIndex, out kBase1, out kBase2, out firstKeyIndex);
            k1 = (NumericKeyFrame)kBase1;
            k2 = (NumericKeyFrame)kBase2;

            if (t == 0.0f) {
                // Just use k1
                kret.NumericValue = k1.NumericValue;
            }
            else {
                // Interpolate by t
                kret.NumericValue = AnimableValue.InterpolateValues(t, targetAnimable.Type, 
                                                                    k1.NumericValue, k2.NumericValue);
            }
            return kf;
        }
        
        public override void Apply(float timePos, float weight, bool accumulate, float scale) {
            ApplyToAnimable(targetAnimable, timePos, weight, scale);
        }

        /// <summary> Applies an animation track to a given animable value. </summary>
        /// <param name="anim">The AnimableValue to which to apply the animation </param>
        /// <param name="timePos">The time position in the animation to apply. </param>
        /// <param name="weight">The influence to give to this track, 1.0 for full influence, less to blend with
        ///        other animations. </param>
        /// <param name="scale">The scale to apply to translations and scalings, useful for 
        ///        adapting an animation to a different size target. </param>
        void ApplyToAnimable(AnimableValue anim, float time, float weight, float scale) {
            // Nothing to do if no keyframes
            if (keyFrameList.Count == 0)
                return;

            NumericKeyFrame kf = new NumericKeyFrame(null, time);
            GetInterpolatedKeyFrame(time, kf);
            // add to existing. Weights are not relative, but treated as
            // absolute multipliers for the animation
            float v = weight * scale;
            Object val = AnimableValue.MultiplyFloat(anim.Type, v, kf.NumericValue);

            anim.ApplyDeltaValue(val);

        }

        public NumericKeyFrame CreateNumericKeyFrame(float time) {
            return (NumericKeyFrame)CreateKeyFrame(time);
        }

        public NumericKeyFrame GetNumericKeyFrame(ushort index) {
            return (NumericKeyFrame)keyFrameList[index];
        }
        
        #endregion Public methods

    }
    
    public class NodeAnimationTrack : AnimationTrack {

        #region Fields

        /// <summary>
        ///		Target node to be animated.
        ///	</summary>
        protected Node target;
        /// <summary>
        ///		Flag indicating we need to rebuild the splines next time.
        ///	</summary>
        protected bool isSplineRebuildNeeded = false;
        /// <summary>
        ///		Spline for position interpolation.
        ///	</summary>
        protected PositionalSpline positionSpline = new PositionalSpline();
        /// <summary>
        ///		Spline for scale interpolation.
        ///	</summary>
        protected PositionalSpline scaleSpline = new PositionalSpline();
        /// <summary>
        ///		Spline for rotation interpolation.
        ///	</summary>
        protected RotationalSpline rotationSpline = new RotationalSpline();
        /// <summary>
        ///		Defines if rotation is done using shortest path
        /// </summary>
        protected bool useShortestPath = true;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///		Internal constructor, to prevent direction instantiation.  Should be created
        ///		via a call to the CreateTrack method of an Animation.
        /// </summary>
        public NodeAnimationTrack(Animation parent, Node target) : base(parent) {
            this.target = target;
        }

        public NodeAnimationTrack(Animation parent, ushort handle) : base(parent, handle) {
        }
        
        public NodeAnimationTrack(Animation parent) : base(parent) {
            this.target = null;
        }
        
        #endregion Constructors

        #region Properties

        /// <summary>
        ///		Gets/Sets the target node that this track is associated with.
        /// </summary>
        public Node TargetNode {
            get { 
                return target; 
            }
            set { 
                target = value; 
            }
        }

        #endregion Properties

        #region Public methods

        /// <summary>
        ///		Gets a KeyFrame object which contains the interpolated transforms at the time index specified.
        /// </summary>
        /// <remarks>
        ///		The KeyFrame objects held by this class are transformation snapshots at 
        ///		discrete points in time. Normally however, you want to interpolate between these
        ///		keyframes to produce smooth movement, and this method allows you to do this easily.
        ///		In animation terminology this is called 'tweening'. 
        /// </remarks>
        /// <param name="time">The time (in relation to the whole animation sequence).</param>
        /// <returns>
        ///		A new keyframe object containing the interpolated transforms. Note that the
        ///		position and scaling transforms are linearly interpolated (lerp), whilst the rotation is
        ///		spherically linearly interpolated (slerp) for the most natural result.
        /// </returns>
        public override KeyFrame GetInterpolatedKeyFrame(float time, KeyFrame kf) {
            // note: this is an un-attached keyframe
            TransformKeyFrame result = (TransformKeyFrame)kf;

            // Keyframe pointers
            KeyFrame kBase1, kBase2;
            TransformKeyFrame k1, k2;
            ushort firstKeyIndex;

            float t = GetKeyFramesAtTime(time, out kBase1, out kBase2, out firstKeyIndex);
            k1 = (TransformKeyFrame)kBase1;
            k2 = (TransformKeyFrame)kBase2;

            if(t == 0.0f) {
                // just use k1
                result.Rotation = k1.Rotation;
                result.Translate = k1.Translate;
                result.Scale = k1.Scale;
            }
            else {
                // interpolate by t
                InterpolationMode mode = parent.interpolationMode;
                RotationInterpolationMode rim = parent.rotationInterpolationMode;

                switch(mode) {
                    case InterpolationMode.Linear: {
                        // linear interoplation
                        // Rotation
                        // Interpolate to nearest rotation if mUseShortestPath set
                        if (rim == RotationInterpolationMode.Linear) {
                            Quaternion.NlerpRef(ref result.rotation, t, ref k1.rotation, ref k2.rotation, useShortestPath);
                        }
                        else // RotationInterpolationMode.Spherical
                            result.rotation = Quaternion.Slerp(t, k1.rotation, k2.rotation, useShortestPath);
                        result.translate.x = k1.translate.x + ((k2.translate.x - k1.translate.x) * t);
                        result.translate.y = k1.translate.y + ((k2.translate.y - k1.translate.y) * t);
                        result.translate.z = k1.translate.z + ((k2.translate.z - k1.translate.z) * t);
                        result.scale.x = k1.scale.x + ((k2.scale.x - k1.scale.x) * t);
                        result.scale.y = k1.scale.y + ((k2.scale.y - k1.scale.y) * t);
                        result.scale.z = k1.scale.z + ((k2.scale.z - k1.scale.z) * t);
                        result.Changed();
                    }	break;
                    case InterpolationMode.Spline: {
                        // spline interpolation
                        if(isSplineRebuildNeeded) {
                            BuildInterpolationSplines();
                        }

                        result.Rotation = rotationSpline.Interpolate(firstKeyIndex, t, useShortestPath);
                        result.Translate = positionSpline.Interpolate(firstKeyIndex, t);
                        result.Scale = scaleSpline.Interpolate(firstKeyIndex, t);
                    }	break;

                }
            }

            // return the resulting keyframe
            return result;
        }

        /// <summary>
        ///		Applies an animation track at a certain position to the target node.
        /// </summary>
        /// <remarks>
        ///		When a track has bee associated with a target node, you can eaisly apply the animation
        ///		to the target by calling this method.
        /// </remarks>
        /// <param name="time">The time position in the animation to apply.</param>
        /// <param name="weight">The influence to give to this track, 1.0 for full influence, less to blend with
        ///		other animations.</param>
        /// <param name="accumulate"></param>
        public override void Apply(float time, float weight, bool accumulate, float scale) {
            // call ApplyToNode with our target node
            ApplyToNode(target, time, weight, accumulate, scale);
        }

        /// <summary>
        ///		Same as the Apply method, but applies to a specified Node instead of it's associated node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="time"></param>
        /// <param name="weight"></param>
        /// <param name="accumulate"></param>
        public void ApplyToNode(Node node, float time, float weight, bool accumulate, float scale) {
            TransformKeyFrame kf = new TransformKeyFrame(null, time);
            this.GetInterpolatedKeyFrame(time, kf);

            if(accumulate) {
                // add to existing. Weights are not relative, but treated as absolute multipliers for the animation
                Vector3 translate = kf.Translate * weight * scale;
                node.Translate(translate);

                // interpolate between not rotation and full rotation, to point weight, so 0 = no rotate, and 1 = full rotation
                Quaternion rotate = Quaternion.Slerp(weight, Quaternion.Identity, kf.Rotation);
                node.Rotate(rotate);

                // TODO: not yet sure how to modify scale for cumulative animations
                Vector3 scaleVector = kf.Scale;
                // Not sure how to modify scale for cumulative anims... leave it alone
                //scaleVector = ((Vector3::UNIT_SCALE - kf.getScale()) * weight) + Vector3::UNIT_SCALE;
                if (scale != 1.0f && scaleVector != Vector3.UnitScale)
                    scaleVector = Vector3.UnitScale + (scaleVector - Vector3.UnitScale) * scale;
                node.Scale(scaleVector);
            }
            else {
                // apply using weighted transform method
                node.WeightedTransform(weight, ref kf.translate, ref kf.rotation, ref kf.scale, false);
            }
        }

        #endregion

        #region Protected/Internal methods

        /// <summary>
        ///		Called internally when keyframes belonging to this track are changed, in order to
        ///		trigger a rebuild of the animation splines.
        /// </summary>
        public override void OnKeyFrameDataChanged() {
            isSplineRebuildNeeded = true;
            base.OnKeyFrameDataChanged();
        }

        /// <summary>Used to rebuild the internal interpolation splines for translations, rotations, and scaling.</summary>
        protected void BuildInterpolationSplines() {
            // dont calculate on the fly, wait till the end when we do it manually
            positionSpline.AutoCalculate = false;
            rotationSpline.AutoCalculate = false;
            scaleSpline.AutoCalculate = false;

            positionSpline.Clear();
            rotationSpline.Clear();
            scaleSpline.Clear();

            // add spline control points for each keyframe in the list
            for(int i = 0; i < keyFrameList.Count; i++) {
                TransformKeyFrame keyFrame = (TransformKeyFrame)keyFrameList[i];

                positionSpline.AddPoint(keyFrame.Translate);
                rotationSpline.AddPoint(keyFrame.Rotation);
                scaleSpline.AddPoint(keyFrame.Scale);
            }

            // recalculate all spline tangents now
            positionSpline.RecalculateTangents();
            rotationSpline.RecalculateTangents();
            scaleSpline.RecalculateTangents();

            isSplineRebuildNeeded = false;
        }

        /// <summary>
        ///     Method to determine if this track has any KeyFrames which are
        ///     doing anything useful - can be used to determine if this track
        ///     can be optimised out.
        /// </summary>
        public override bool HasNonZeroKeyFrames() {
            for(int i = 0; i < keyFrameList.Count; i++) {
                KeyFrame keyFrame = keyFrameList[i];
                // look for keyframes which have any component which is non-zero
                // Since exporters can be a little inaccurate sometimes we use a
                // tolerance value rather than looking for nothing
                TransformKeyFrame kf = (TransformKeyFrame)keyFrame;
                Vector3 trans = kf.Translate;
                Vector3 scale = kf.Scale;
                Vector3 axis = Vector3.Zero;
                float angle = 0f;
                kf.Rotation.ToAngleAxis(ref angle, ref axis);
                float tolerance = 1e-3f;
                if (trans.Length > tolerance ||
                    (scale - Vector3.UnitScale).Length > tolerance ||
                    !MathUtil.FloatEqual(MathUtil.DegreesToRadians(angle), 0.0f, tolerance))
                {
                    return true;
                }

            }

            return false;
        }

        /// <summary> Optimise the current track by removing any duplicate keyframes. </summary>
        public override void Optimise()
        {
            // Eliminate duplicate keyframes from 2nd to penultimate keyframe
            // NB only eliminate middle keys from sequences of 5+ identical keyframes
            // since we need to preserve the boundary keys in place, and we need
            // 2 at each end to preserve tangents for spline interpolation
            Vector3 lasttrans = Vector3.Zero;
            Vector3 lastscale = Vector3.Zero;
            Quaternion lastorientation = Quaternion.Zero;
            float tolerance = 1e-3f;
            float quatTolerance = 1e-3f;
            ushort k = 0;
            ushort dupKfCount = 0;
            List<short> removeList = new List<short>();
            for(int i = 0; i < keyFrameList.Count; i++) {
                KeyFrame keyFrame = keyFrameList[i];
                TransformKeyFrame kf = (TransformKeyFrame)keyFrame;
                Vector3 newtrans = kf.Translate;
                Vector3 newscale = kf.Scale;
                Quaternion neworientation = kf.Rotation;
                // Ignore first keyframe; now include the last keyframe as we eliminate
                // only k-2 in a group of 5 to ensure we only eliminate middle keys
                if (i != 0 &&
                    newtrans.DifferenceLessThan(lasttrans, tolerance) &&
                    newscale.DifferenceLessThan(lastscale, tolerance) &&
                    neworientation.Equals(lastorientation, quatTolerance))
                {
                    ++dupKfCount;

                    // 4 indicates this is the 5th duplicate keyframe
                    if (dupKfCount == 4)
                    {
                        // remove the 'middle' keyframe
                        removeList.Add((short)(k-2));
                        --dupKfCount;
                    }
                }
                else
                {
                    // reset
                    dupKfCount = 0;
                    lasttrans = newtrans;
                    lastscale = newscale;
                    lastorientation = neworientation;
                }
            }

            // Now remove keyframes, in reverse order to avoid index revocation
            for (int i = removeList.Count - 1; i>=0; i--)
                RemoveKeyFrame(removeList[i]);
        }

        /// <summary> Specialised keyframe creation </summary>
        public override KeyFrame CreateKeyFrameImpl(float time)
        {
            return new TransformKeyFrame(this, time);
        }

        /// <summary> 
        ///     Creates a new KeyFrame and adds it to this animation at the given time index.
        /// </summary>
        /// <remarks>
        ///    It is better to create KeyFrames in time order. Creating them out of order can result 
        ///    in expensive reordering processing. Note that a KeyFrame at time index 0.0 is always created
        ///    for you, so you don't need to create this one, just access it using getKeyFrame(0);
        /// </remarks>
        /// <param> name="timePos">The time from which this KeyFrame will apply. </param>
        public virtual TransformKeyFrame CreateNodeKeyFrame(float time)
        {
            return (TransformKeyFrame)CreateKeyFrame(time);
        }

        /// <summary> Returns the KeyFrame at the specified index. </summary>
        public virtual TransformKeyFrame GetNodeKeyFrame(ushort index)
        {
            return (TransformKeyFrame)keyFrameList[index];
        }

        #endregion
    }

    public class VertexAnimationTrack : AnimationTrack {
        
        #region Fields

        /// Animation type
        protected VertexAnimationType animationType;
        /// Target to animate
        protected VertexData targetVertexData;
        /// Mode to apply
        protected VertexAnimationTargetMode targetMode;

        #endregion Fields

        #region Constructors

        /// Constructor
        public VertexAnimationTrack(Animation parent, VertexAnimationType animationType) : base(parent) {
            this.animationType = animationType;
        }
            
        public VertexAnimationTrack(Animation parent, ushort handle) : base(parent, handle) {
        }

        public VertexAnimationTrack(Animation parent, ushort handle, VertexAnimationType animationType)
            : base(parent, handle) {
            this.animationType = animationType;
        }
        
        /// Constructor, associates with target VertexData and temp buffer (for software)
        public VertexAnimationTrack(Animation parent, VertexAnimationType animationType,
                                    VertexData targetVertexData, VertexAnimationTargetMode targetMode) : base(parent) {
            this.animationType = animationType;
            this.targetVertexData = targetVertexData;
            this.targetMode = targetMode;
        }

        #endregion Constructors

        #region Properties	

        /// <summary>
        ///		Gets/Sets the vertex animation type that this track is associated with.
        /// </summary>
        public VertexAnimationType AnimationType {
            get { 
                return animationType; 
            }
            set { 
                animationType = value; 
            }
        }

        /// <summary>
        ///		Gets/Sets the target vertex data that this track is associated with.
        /// </summary>
        public VertexData TargetVertexData {
            get { 
                return targetVertexData; 
            }
            set { 
                targetVertexData = value; 
            }
        }

        /// <summary>
        ///		Gets/Sets the target node that this track is associated with.
        /// </summary>
        public VertexAnimationTargetMode TargetMode {
            get { 
                return targetMode; 
            }
            set { 
                targetMode = value; 
            }
        }

        #endregion Properties	

        #region Public Methods

        /// <summary> Creates a new morph KeyFrame and adds it to this animation at the given time index. </summary>
        /// <remarks>
        ///     It is better to create KeyFrames in time order. Creating them out of order can result 
        ///     in expensive reordering processing. Note that a KeyFrame at time index 0.0 is always created
        ///     for you, so you don't need to create this one, just access it using getKeyFrame(0);
        /// </remarks>
        /// <param name="timePos">The time from which this KeyFrame will apply.</param>
        public VertexMorphKeyFrame CreateVertexMorphKeyFrame(float time) {
            if (animationType != VertexAnimationType.Morph)
                throw new Exception("Morph keyframes can only be created on vertex tracks of type morph; in VertexAnimationTrack::createVertexMorphKeyFrame");
            return (VertexMorphKeyFrame)CreateKeyFrame(time);
        }

        public override void Apply(float time, float weight, bool accumulate, float scale) {
            ApplyToVertexData(targetVertexData, time, weight, null);
        }

        /// <summary>
        ///     As the 'apply' method but applies to specified VertexData instead of 
        ///	    associated data.
        /// </summary>
        public void ApplyToVertexData(VertexData data, float time, float weight, List<Pose> poseList) {
            // Nothing to do if no keyframes
            if (keyFrameList.Count == 0)
                return;

            // Get keyframes
            KeyFrame kf1, kf2;
            ushort firstKeyIndex;
            float t = GetKeyFramesAtTime(time, out kf1, out kf2, out firstKeyIndex);

            if (animationType == VertexAnimationType.Morph) {
                VertexMorphKeyFrame vkf1 = (VertexMorphKeyFrame)kf1;
                VertexMorphKeyFrame vkf2 = (VertexMorphKeyFrame)kf2;

                if (targetMode == VertexAnimationTargetMode.Hardware) {
                    // If target mode is hardware, need to bind our 2 keyframe buffers,
                    // one to main pos, one to morph target texcoord
                    Debug.Assert(data.HWAnimationDataList.Count == 0,
                                 "Haven't set up hardware vertex animation elements!");

                    // no use for TempBlendedBufferInfo here btw
                    // NB we assume that position buffer is unshared
                    // VertexDeclaration::getAutoOrganisedDeclaration should see to that
                    VertexElement posElem =
                        data.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);
                    // Set keyframe1 data as original position
                    data.vertexBufferBinding.SetBinding(posElem.Source, vkf1.VertexBuffer);
                    // Set keyframe2 data as derived
                    data.vertexBufferBinding.SetBinding(data.HWAnimationDataList[0].TargetVertexElement.Source,
                                                        vkf2.VertexBuffer);
                    // save T for use later
                    data.HWAnimationDataList[0].Parametric = t;

                }
                else {
                    // If target mode is software, need to software interpolate each vertex

                    Mesh.SoftwareVertexMorph(t, vkf1.VertexBuffer, vkf2.VertexBuffer, data);
                }
            }
            else {
                // Pose

                VertexPoseKeyFrame vkf1 = (VertexPoseKeyFrame)kf1;
                VertexPoseKeyFrame vkf2 = (VertexPoseKeyFrame)kf2;

                // For each pose reference in key 1, we need to locate the entry in
                // key 2 and interpolate the influence
                List<PoseRef> poseRefList1 = vkf1.PoseRefs;
                List<PoseRef> poseRefList2 = vkf2.PoseRefs;
                foreach (PoseRef p1 in poseRefList1) {
                    float startInfluence = p1.Influence;
                    float endInfluence = 0;
                    // Search for entry in keyframe 2 list (if not there, will be 0)
                    foreach (PoseRef p2 in poseRefList2) {
                        if (p1.PoseIndex == p2.PoseIndex) {
                            endInfluence = p2.Influence;
                            break;
                        }
                    }
                    // Interpolate influence
                    float influence = startInfluence + t*(endInfluence - startInfluence);
                    // Scale by animation weight
                    influence = weight * influence;
                    // Get pose
                    Debug.Assert (p1.PoseIndex <= poseList.Count);
                    Pose pose = poseList[p1.PoseIndex];
                    // apply
                    ApplyPoseToVertexData(pose, data, influence);
                }
                // Now deal with any poses in key 2 which are not in key 1
                foreach (PoseRef p2 in  poseRefList2) {
                    bool found = false;
                    foreach (PoseRef p1 in poseRefList1) {
                        if (p1.PoseIndex == p2.PoseIndex) {
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        // Need to apply this pose too, scaled from 0 start
                        float influence = t * p2.Influence;
                        // Scale by animation weight
                        influence = weight * influence;
                        // Get pose
                        Debug.Assert (p2.PoseIndex <= poseList.Count);
                        Pose pose = poseList[p2.PoseIndex];
                        // apply
                        ApplyPoseToVertexData(pose, data, influence);
                    }
                } // key 2 iteration
            } // morph or pose animation
        }

        /// <summary> Creates the single pose KeyFrame and adds it to this animation. </summary>
        public VertexPoseKeyFrame CreateVertexPoseKeyFrame(float time) {
            if (animationType != VertexAnimationType.Pose)
                throw new Exception("Pose keyframes can only be created on vertex tracks of type pose; in VertexAnimationTrack::createVertexPoseKeyFrame");
            return (VertexPoseKeyFrame)CreateKeyFrame(time);
        }

        /// <summary>
        ///     This method in fact does nothing, since interpolation is not performed
        ///  	inside the keyframes for this type of track. 
        /// </summary>
        public override KeyFrame GetInterpolatedKeyFrame(float time, KeyFrame kf) {
            return kf;
        }

        /// <summary> Utility method for applying pose animation </summary>
        public void ApplyPoseToVertexData(Pose pose, VertexData data, float influence) {
            if (targetMode == VertexAnimationTargetMode.Hardware) {
                // Hardware
                // If target mode is hardware, need to bind our pose buffer
                // to a target texcoord
                Debug.Assert(data.HWAnimationDataList.Count == 0,
                             "Haven't set up hardware vertex animation elements!");
                // no use for TempBlendedBufferInfo here btw
                // Set pose target as required
                int hwIndex = data.HWAnimDataItemsUsed++;
                // If we try to use too many poses, ignore extras
                if (hwIndex < data.HWAnimationDataList.Count) {
                    HardwareAnimationData animData = data.HWAnimationDataList[hwIndex];
                    data.vertexBufferBinding.SetBinding(animData.TargetVertexElement.Source,
                                                        pose.GetHardwareVertexBuffer(data.vertexCount));
                    // save final influence in parametric
                    animData.Parametric = influence;
                }
            }
            else {
                // Software
                Mesh.SoftwareVertexPoseBlend(influence, pose.VertexOffsetMap, data);
            }
        }

        /// <summary> Returns the morph KeyFrame at the specified index. </summary>
        public VertexMorphKeyFrame GetVertexMorphKeyFrame(ushort index) {
            if (animationType != VertexAnimationType.Morph)
                throw new Exception("Morph keyframes can only be created on vertex tracks of type morph, in VertexAnimationTrack::getVertexMorphKeyFrame");
            return (VertexMorphKeyFrame)keyFrameList[index];
        }

        /// <summary> Returns the pose KeyFrame at the specified index. </summary>
        public VertexPoseKeyFrame GetVertexPoseKeyFrame(ushort index) {
            if (animationType != VertexAnimationType.Pose)
                throw new Exception("Pose keyframes can only be created on vertex tracks of type pose, in VertexAnimationTrack::getVertexPoseKeyFrame");
            return (VertexPoseKeyFrame)keyFrameList[index];
        }

        public override KeyFrame CreateKeyFrameImpl(float time) {
            switch (animationType) {
                default:
                case VertexAnimationType.Morph:
                    return new VertexMorphKeyFrame(this, time);
                case VertexAnimationType.Pose:
                    return new VertexPoseKeyFrame(this, time);
            }
        }

        /// <summary>
        ///     Method to determine if this track has any KeyFrames which are
        ///     doing anything useful - can be used to determine if this track
        ///     can be optimised out.
        /// </summary>
        public override bool HasNonZeroKeyFrames() {
            if (animationType == VertexAnimationType.Morph)
                return KeyFrames.Count > 0;
            else {
                foreach(KeyFrame kbase in KeyFrames) {
                    // look for keyframes which have a pose influence which is non-zero
                    VertexPoseKeyFrame kf = (VertexPoseKeyFrame)kbase;
                    foreach(PoseRef poseRef in kf.PoseRefs) {
                        if (poseRef.influence > 0.0f)
                            return true;
                    }
                }
                return false;
            }
        }

        /// <summary> Optimise the current track by removing any duplicate keyframes. </summary>
        public override void Optimise() {
            // TODO - remove sequences of duplicate pose references?
        }

        #endregion Public Methods
        
    }
    
}
