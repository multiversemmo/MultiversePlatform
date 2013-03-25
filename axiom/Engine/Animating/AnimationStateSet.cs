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
    public class AnimationStateSet {

		#region Protected Fields

		/// <summary>
		///		Mapping from string to AnimationState
		/// </summary>
		protected Dictionary<string, AnimationState> stateSet = new Dictionary<string, AnimationState>();
		/// <summary>
		///		
		/// </summary>
		protected int dirtyFrameNumber;
		/// <summary>
		///		A list of enabled animation states
		/// </summary>
		protected List<AnimationState> enabledAnimationStates = new List<AnimationState>();

		#endregion Protected Fields
		
		#region Constructors
		
		public AnimationStateSet() {
			this.dirtyFrameNumber = int.MaxValue;
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		///     Get the latest animation state been altered frame number
		/// </summary>
		public int DirtyFrameNumber {
			get { return dirtyFrameNumber; }
            set { dirtyFrameNumber = value; }
		}

		/// <summary>
		///     Get the dictionary of states
		/// </summary>
		public Dictionary<string, AnimationState> AllAnimationStates {
			get { return stateSet; }
		}
		
		/// <summary>
		///     Get the list of enabled animation states
		/// </summary>

		/// <summary>
		///     Get the list of all animation states
		/// </summary>
        public List<AnimationState> EnabledAnimationStates {
			get { return enabledAnimationStates; }
		}

        public ICollection<AnimationState> Values {
            get { return (ICollection<AnimationState>)stateSet.Values; }
        }
		
		#endregion Properties
		
		#region Methods

		/// <summary>
        ///     Create a copy of the AnimationStateSet instance. 
        /// </summary>
        public AnimationStateSet Clone() {
			AnimationStateSet newSet = new AnimationStateSet();
			
			foreach(AnimationState animationState in stateSet.Values)
				new AnimationState(newSet, animationState);

			// Clone enabled animation state list
			foreach (AnimationState animationState in enabledAnimationStates)
				newSet.EnabledAnimationStates.Add(newSet.GetAnimationState(animationState.Name));
            return newSet;
		}
		
 		/// <summary>
        ///     Create a new AnimationState instance. 
        /// </summary>
        /// <param name="animName" The name of the animation</param>
        /// <param name="timePos Starting time position</param>
        /// <param name="length Length of the animation to play</param>
        public AnimationState CreateAnimationState(string name, float time, float length) {
            return CreateAnimationState(name, time, length, 1.0f, false);
        }

		/// <summary>
		///     Create a new AnimationState instance. 
		/// </summary>
		/// <param name="animName" The name of the animation</param>
		/// <param name="timePos Starting time position</param>
		/// <param name="length Length of the animation to play</param>
		/// <param name="weight Weight to apply the animation with</param>
		/// <param name="enabled Whether the animation is enabled</param>
		public AnimationState CreateAnimationState(string name, float time, float length,
												   float weight, bool enabled) {
			if (stateSet.ContainsKey(name))
				throw new Exception("State for animation named '" + name + "' already exists, " +
									"in AnimationStateSet.CreateAnimationState");
			AnimationState newState = new AnimationState(name, this, time, 
														 length, weight, enabled);
			stateSet[name] = newState;
			return newState;
		}
		
		/// <summary>
		///     Get an animation state by the name of the animation
		/// </summary>
		public AnimationState GetAnimationState(string name) {
			if (!stateSet.ContainsKey(name))
				throw new Exception("No state found for animation named '" + name + "', " +
									"in AnimationStateSet.CreateAnimationState");
			return stateSet[name];
		}
			
		/// <summary>
		///     Tests if state for the named animation is present
		/// </summary>
		public bool HasAnimationState(string name) {
			return stateSet.ContainsKey(name);
		}
			
		/// <summary>
		///     Remove animation state with the given name
		/// </summary>
		public void RemoveAnimationState(string name) {
			if (stateSet.ContainsKey(name)) {
				enabledAnimationStates.Remove(stateSet[name]);
				stateSet.Remove(name);
			}
		}
			
		/// <summary>
		///     Remove all animation states
		/// </summary>
		public void RemoveAllAnimationStates() {
		    stateSet.Clear();
			enabledAnimationStates.Clear();
		}

		/// <summary>
		///     Copy the state of any matching animation states from this to another
		/// </summary>
		public void CopyMatchingState(AnimationStateSet target) {
			foreach(KeyValuePair<string, AnimationState> pair in target.AllAnimationStates) {
				AnimationState result;
				if (!stateSet.TryGetValue(pair.Key, out result))
					throw new Exception("No animation entry found named '" + pair.Key + "', in " + 
										"AnimationStateSet.CopyMatchingState");
				else
					result.CopyTo(pair.Value);
			}

			// Copy matching enabled animation state list
			target.EnabledAnimationStates.Clear();
			foreach(AnimationState state in enabledAnimationStates)
				target.EnabledAnimationStates.Add(target.AllAnimationStates[state.Name]);

			target.DirtyFrameNumber = dirtyFrameNumber;
		}
			
		/// <summary>
		///     Set the dirty flag and dirty frame number on this state set
		/// </summary>
		public void NotifyDirty() {
			++dirtyFrameNumber;
		} 

		/// <summary>
        ///     Internal method respond to enable/disable an animation state
		/// </summary>
		public void NotifyAnimationStateEnabled(AnimationState target, bool enabled) {
			// Remove from enabled animation state list first
			enabledAnimationStates.Remove(target);

			// Add to enabled animation state list if need
			if (enabled)
				enabledAnimationStates.Add(target);

			// Set the dirty frame number
			NotifyDirty();
		}

        #endregion Methods
	}
}
