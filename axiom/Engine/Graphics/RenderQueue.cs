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
using System.Diagnostics;
using Axiom.Collections;
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.Graphics {
	/// <summary>
	///		Class to manage the scene object rendering queue.
	/// </summary>
	/// <remarks>
	///		Objects are grouped by material to minimize rendering state changes. The map from
	///		material to renderable object is wrapped in a class for ease of use.
	///		<p/>
	///		This class includes the concept of 'queue groups' which allows the application
	///		adding the renderable to specifically schedule it so that it is included in 
	///		a discrete group. Good for separating renderables into the main scene,
	///		backgrounds and overlays, and also could be used in the future for more
	///		complex multipass routines like stenciling.
	/// </remarks>
	public class RenderQueue {
		#region Fields

		/// <summary>
		///		Cached list of render groups, indexed by RenderQueueGroupID.
		///	</summary>
		protected SortedList renderGroups = new SortedList();
		/// <summary>
		///		Default render group for this queue.
		///	</summary>
		protected RenderQueueGroupID defaultGroup;
		/// <summary>
		///		Should passes be split by their lighting stage?
		/// </summary>
		protected bool splitPassesByLightingType;
		/// <summary>
		/// 
		/// </summary>
		protected bool splitNoShadowPasses;
		/// <summary>
		/// 
		/// </summary>
		protected bool shadowCastersCannotBeReceivers;

		/// <summary>
		///		Default priority of items added to the render queue.
		///	</summary>
		public const int DEFAULT_PRIORITY = 100;

		#endregion Fields

		#region Constructors

		/// <summary>
		///		Default constructor.
		/// </summary>
		public RenderQueue() {
			// set the default queue group for this queue
			defaultGroup = RenderQueueGroupID.Main;

			// create the main queue group up front
			renderGroups.Add(
				RenderQueueGroupID.Main, 
				new RenderQueueGroup(this, splitPassesByLightingType, 
									 splitNoShadowPasses, 
									 shadowCastersCannotBeReceivers));
		}

		#endregion

		#region Properties

		/// <summary>
		///		Gets/Sets the default priority for rendering objects in the queue.
		/// </summary>
		public RenderQueueGroupID DefaultRenderGroup {
			get { 
				return defaultGroup; 
			}
			set { 
				defaultGroup = value; 
			}
		}

		/// <summary>
		///    Gets the number of render queue groups contained within this queue.
		/// </summary>
		public int NumRenderQueueGroups {
			get {
				return renderGroups.Count;
			}
		}

		/// <summary>
		///		Gets/Sets whether or not the queue will split passes by their lighting type,
		///		ie ambient, per-light and decal. 
		/// </summary>
		public bool SplitPassesByLightingType {
			get {
				return splitPassesByLightingType;
			}
			set {
				splitPassesByLightingType = value;

				// set the value for all render groups as well
				for(int i = 0; i < renderGroups.Count; i++) {
					GetQueueGroupByIndex(i).SplitPassesByLightingType = splitPassesByLightingType;
				}
			}
		}

		/// <summary>
		///		Gets/Sets whether or not the queue will split passes which have shadow receive
		///		turned off (in their parent material), which is needed when certain shadow
		///		techniques are used.
		/// </summary>
		public bool SplitNoShadowPasses {
			get {
				return splitNoShadowPasses;
			}
			set {
				splitNoShadowPasses = value;

				// set the value for all render groups as well
				for(int i = 0; i < renderGroups.Count; i++) {
					GetQueueGroupByIndex(i).SplitNoShadowPasses = splitNoShadowPasses;
				}
			}
		}

		/// <summary>
		///		Gets/Sets whether or not the queue will split passes which have shadow receive
		///		turned off (in their parent material), which is needed when certain shadow
		///		techniques are used.
		/// </summary>
		public bool ShadowCastersCannotBeReceivers {
			get {
				return shadowCastersCannotBeReceivers;
			}
			set {
				shadowCastersCannotBeReceivers = value;

				// set the value for all render groups as well
				for(int i = 0; i < renderGroups.Count; i++) {
					GetQueueGroupByIndex(i).ShadowCastersCannotBeReceivers = shadowCastersCannotBeReceivers;
				}
			}
		}

		#endregion

		#region Public methods

		/// <summary>
		///		Adds a renderable item to the queue.
		/// </summary>
		/// <param name="item">IRenderable object to add to the queue.</param>
		/// <param name="groupID">Group to add the item to.</param>
		/// <param name="priority"></param>
		public void AddRenderable(IRenderable renderable, ushort priority, RenderQueueGroupID groupID) {
			RenderQueueGroup group = GetQueueGroup(groupID);

			// let the material know it has been used, which also forces a recompile if required
			if(renderable.Material != null) {
				renderable.Material.Touch();
			}

			// add the renderable to the appropriate group
			group.AddRenderable(renderable, priority);
		}

		/// <summary>
		///		Overloaded method.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="groupID"></param>
		public void AddRenderable(IRenderable item, ushort priority) {
			AddRenderable(item, priority, defaultGroup);
		}

		/// <summary>
		///		Overloaded method.
		/// </summary>
		/// <param name="item"></param>
		public void AddRenderable(IRenderable item) {
			AddRenderable(item, DEFAULT_PRIORITY);
		}

		/// <summary>
		///		Clears all 
		/// </summary>
		public void Clear() {
			// loop through each queue and clear it's items.  We don't wanna clear the group
			// list because it probably won't change frame by frame.
			for(int i = 0; i < renderGroups.Count; i++) {
				RenderQueueGroup group = (RenderQueueGroup)renderGroups.GetByIndex(i);

				// clear the RenderQueueGroup
				group.Clear();
			}

			// trigger the pending pass updates
			Pass.ProcessPendingUpdates();
		}

		/// <summary>
		///		Get a render queue group.
		/// </summary>
		/// <remarks>
		///		New queue groups are registered as they are requested, 
		///		therefore this method will always return a valid group.
		/// </remarks>
		/// <param name="queueID">ID of the queue group to retreive.</param>
		/// <returns></returns>
		public RenderQueueGroup GetQueueGroup(RenderQueueGroupID queueID) {
			RenderQueueGroup group = null;

			// see if there is a current queue group for this group id
			if(renderGroups[queueID] == null) {
				// create a new queue group for this group id
				group = new RenderQueueGroup(this, splitPassesByLightingType, 
											 splitNoShadowPasses,
											 shadowCastersCannotBeReceivers);

				// add the new group to cached render group
				renderGroups.Add(queueID, group);
			}
			else {
				// retreive the existing queue group
				group = (RenderQueueGroup)renderGroups[queueID];
			}

			return group;
		}

		/// <summary>
		///    
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal RenderQueueGroup GetQueueGroupByIndex(int index) {
			Debug.Assert(index < renderGroups.Count, "index < renderGroups.Count");

			return (RenderQueueGroup)renderGroups.GetByIndex(index);
		}

		internal RenderQueueGroupID GetRenderQueueGroupID(int index) {
			Debug.Assert(index < renderGroups.Count, "index < renderGroups.Count");

			return (RenderQueueGroupID)renderGroups.GetKey(index);
		}

		#endregion
	}


	/// <summary>
	///    Internal structure reflecting a single Pass for a Renderable
	/// </summary>
	public class RenderablePass {
		public IRenderable renderable;
		public Pass pass;

		public RenderablePass(IRenderable renderable, Pass pass) {
			this.renderable = renderable;
			this.pass = pass;
		}
	}
}
