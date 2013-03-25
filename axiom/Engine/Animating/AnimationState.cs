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
using Axiom.Collections;
using Axiom.Core;
using Axiom.Controllers;

namespace Axiom.Animating {
    /// <summary>
    ///		Represents the state of an animation and the weight of it's influence. 
    /// </summary>
    /// <remarks>
    ///		Other classes can hold instances of this class to store the state of any animations
    ///		they are using.
    ///		This class implements the IControllerValue interface to enable automatic update of
    ///		animation state through controllers.
    /// </remarks>
    public class AnimationState : IControllerValue<float>, IComparable {
        #region Member variables

        /// <summary>Name of this animation track.</summary>
        protected string animationName;
        /// <summary></summary>
        protected float time;
        /// <summary></summary>
        protected float length;
        /// <summary></summary>
        protected float inverseLength;
        /// <summary></summary>
        protected float weight;
        /// <summary></summary>
        protected bool isEnabled;
        /// <summary></summary>
        protected AnimationStateSet parent;

        #endregion

        #region Constructors

        /// <summary>
        ///		
        /// </summary>
        /// <param name="animationName"></param>
        /// <param name="parent">The animation state set parent</param>
        /// <param name="time"></param>
        /// <param name="length"></param>
        /// <param name="weight"></param>
        /// <param name="isEnabled"></param>
        public AnimationState(string animationName, AnimationStateSet parent, float time, float length, float weight, bool isEnabled) {
            this.animationName = animationName;
            this.parent = parent;
            this.time = time;
            this.weight = weight;
            this.isEnabled = isEnabled;

            // set using the Length property
            this.Length = length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationName"></param>
        /// <param name="parent">The animation state set parent</param>
        /// <param name="time"></param>
        /// <param name="length"></param>
		public AnimationState(string animationName, AnimationStateSet animationStates, float time, float length) {
            this.animationName = animationName;
            this.parent = animationStates;
            this.time = time;
            this.length = length;
            this.weight = 1.0f;
            this.isEnabled = false;
        }

        /// <summary>
        ///     The moral equivalent of a copy constructor
        /// </summary>
        /// <param name="parent">The animation state set parent</param>
        /// <param name="source">An animation state to copy from</param>
        public AnimationState(AnimationStateSet parent, AnimationState source) {
			this.parent = parent;
			source.CopyTo(this);
		}
		
		#endregion

        #region Properties

        /// <summary>
        ///		Gets the name of the animation to which this state applies
        /// </summary>
        public string Name {
            get { return animationName; }
            set { animationName = value; }
        }

        /// <summary>
        ///		Gets/Sets the time position for this animation.
        /// </summary>
        public float Time {
            get { return time; }
            set { time = value; }
        }

        /// <summary>
        ///		Gets/Sets the total length of this animation (may be shorter than whole animation)
        /// </summary>
        public float Length {
            get { return length; }
            set { 
                length = value; 

                // update the inverse length of the animation
                if(length != 0)
                    inverseLength = 1.0f / length;
                else
                    inverseLength = 0.0f;
            }
        }

        /// <summary>
        /// Gets/Sets the weight (influence) of this animation
        /// </summary>
        public float Weight {
            get { return weight; }
            set { weight = value; }
        }

        /// <summary>
        ///		Gets/Sets whether this animation is enabled or not.
        /// </summary>
        public bool IsEnabled {
            get { return isEnabled; }
            set { 
				isEnabled = value;
				parent.NotifyAnimationStateEnabled(this, isEnabled);
			}
        }

        /// <summary>
        ///		Gets/Sets the animation state set owning this animation
        /// </summary>
        public AnimationStateSet Parent {
            get { return parent; }
            set { parent = value; }
        }

        #endregion

        #region Public methods

        /// <summary>
        ///		Modifies the time position, adjusting for animation length.
        /// </summary>
        /// <param name="offset">Offset from the current time position.</param>
        public void AddTime(float offset) {
			// TODO: Add MathUtil function for this?
			time = (float)Math.IEEERemainder(time + offset, length);

			if(time < 0) {
				time += length;
			}
        }

        /// <summary>
        ///     Clones this instance of AnimationState.
        /// </summary>
        /// <returns>A copy of this AnimationState object.</returns>
//        public AnimationState Clone() {
//            AnimationState newState = new AnimationState();
//            CopyTo(newState);
//            return newState;
//        }

        /// <summary>
        ///     Copies the details of this AnimationState instance to another instance.
        /// </summary>
        /// <param name="target">Target instance to copy our details to.</param>
        public void CopyTo(AnimationState target) {
            // target.animationName = animationName; 
            target.IsEnabled = isEnabled;
            target.inverseLength = inverseLength;
            target.length = length;
            target.time = time;
            target.weight = weight;
        }

        #endregion

        #region Implementation of IControllerValue

        /// <summary>
        ///		Gets/Sets the value to be used in a ControllerFunction.
        /// </summary>
        public float Value {
            get { return time * inverseLength;	}
            set {	time = value * length; }
        }
        #endregion

        #region Object overloads

        /// <summary>
        ///    Override GetHashCode.
        /// </summary>
        /// <remarks>
        ///    Done mainly to quash warnings, no real need for it.
        /// </remarks>
        /// <returns></returns>
        public override int GetHashCode() {
            return animationName.GetHashCode();
        }

        #endregion Object overloads

        #region IComparable Members

        /// <summary>
        ///    
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>0 if they are the same, -1 otherwise.</returns>
        public int CompareTo(object obj) {
            AnimationState other = obj as AnimationState;

            if (animationName == other.animationName &&
                isEnabled == other.isEnabled &&
                time == other.time &&
                weight == other.weight &&
                length == other.length) {

                return 0;
            }
            else {
                return -1;
            }
        }

        #endregion
    }
}
