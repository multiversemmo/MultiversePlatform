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

namespace Axiom.Graphics
{
	
	/// <summary>
	///		A grouping level underneath RenderQueue which groups renderables
	///		to be issued at coarsely the same time to the renderer.	
	/// </summary>
	/// <remarks>
	///		Each instance of this class itself hold RenderPriorityGroup instances, 
	///		which are the groupings of renderables by priority for fine control
	///		of ordering (not required for most instances).
	/// </remarks>
	public class RenderQueueGroup 
	{
		#region Fields

		/// <summary>
		///		Render queue that this queue group belongs to.
		/// </summary>
		protected RenderQueue parent;
		/// <summary>
		///		Should passes be split by their lighting stage?
		/// </summary>
		protected bool splitPassesByLightingType;
		protected bool splitNoShadowPasses;
        protected bool shadowCastersCannotBeReceivers;

		/// <summary>
		///		List of priority groups.
		/// </summary>
		protected HashList priorityGroups = new HashList();
		/// <summary>
		///		Are shadows enabled for this group?
		/// </summary>
		protected bool shadowsEnabled;

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Default constructor.
		/// </summary>
		/// <param name="parent">Render queue that owns this group.</param>
		/// <param name="splitPassesByLightingType">Split passes based on lighting stage?</param>
		/// <param name="splitNoShadowPasses"></param>
		public RenderQueueGroup(RenderQueue parent, bool splitPassesByLightingType,
                                bool splitNoShadowPasses, bool shadowCastersCannotBeReceivers) 
		{
			// shadows enabled by default
			shadowsEnabled = true;

			this.splitPassesByLightingType = splitPassesByLightingType;
			this.splitNoShadowPasses = splitNoShadowPasses;
            this.shadowCastersCannotBeReceivers = shadowCastersCannotBeReceivers;
			this.parent = parent;
		}

		#endregion

		#region Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <param name="priority"></param>
		public void AddRenderable(IRenderable item, ushort priority) 
		{
			RenderPriorityGroup group = null;

			// see if there is a current queue group for this group id
			if(!priorityGroups.ContainsKey(priority)) 
			{
				// create a new queue group for this group id
				group = new RenderPriorityGroup(splitPassesByLightingType, splitNoShadowPasses,
												splitPassesByLightingType);

				// add the new group to cached render group
				priorityGroups.Add(priority, group);
			}
			else 
			{
				// retreive the existing queue group
				group = (RenderPriorityGroup)priorityGroups.GetByKey(priority);
			}

			// add the renderable to the appropriate group
			group.AddRenderable(item);			
		}

		/// <summary>
		///		Clears all the priority groups within this group.
		/// </summary>
		public void Clear() 
		{
			// loop through each priority group and clear it's items.  We don't wanna clear the group
			// list because it probably won't change frame by frame.
			for(int i = 0; i < priorityGroups.Count; i++) 
			{
				RenderPriorityGroup group = (RenderPriorityGroup)priorityGroups[i];

				// clear the RenderPriorityGroup
				group.Clear();
			}
		}

		/// <summary>
		///    Gets the hashlist entry for the priority group at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public RenderPriorityGroup GetPriorityGroup(int index) 
		{
			Debug.Assert(index < priorityGroups.Count, "index < priorityGroups.Count");

			return (RenderPriorityGroup)priorityGroups[index];
		}

		#endregion

		#region Properties

		/// <summary>
		///    Gets the number of priority groups within this queue group.
		/// </summary>
		public int NumPriorityGroups 
		{
			get 
			{
				return priorityGroups.Count;
			}
		}

		/// <summary>
		///		Indicate whether a given queue group will be doing any shadow setup.
		/// </summary>
		/// <remarks>
		///		This method allows you to inform the queue about a queue group, and to 
		///		indicate whether this group will require shadow processing of any sort.
		///		In order to preserve rendering order, Axiom/Ogre has to treat queue groups
		///		as very separate elements of the scene, and this can result in it
		///		having to duplicate shadow setup for each group. Therefore, if you
		///		know that a group which you are using will never need shadows, you
		///		should preregister the group using this method in order to improve
		///		the performance.
		/// </remarks>
		public bool ShadowsEnabled 
		{
			get 
			{
				return shadowsEnabled;
			}
			set 
			{
				shadowsEnabled = value;
			}
		}

		/// <summary>
		///		Gets/Sets whether or not the queue will split passes by their lighting type,
		///		ie ambient, per-light and decal. 
		/// </summary>
		public bool SplitPassesByLightingType 
		{
			get 
			{
				return splitPassesByLightingType;
			}
			set 
			{
				splitPassesByLightingType = value;

				// set the value for all priority groups as well
				for(int i = 0; i < priorityGroups.Count; i++) 
				{
					GetPriorityGroup(i).SplitPassesByLightingType = splitPassesByLightingType;
				}
			}
		}

		/// <summary>
		///		Gets/Sets whether or not the queue will split passes which have shadow receive
		///		turned off (in their parent material), which is needed when certain shadow
		///		techniques are used.
		/// </summary>
		public bool SplitNoShadowPasses 
		{
			get 
			{
				return splitNoShadowPasses;
			}
			set 
			{
				splitNoShadowPasses = value;

				// set the value for all priority groups as well
				for(int i = 0; i < priorityGroups.Count; i++) 
				{
					GetPriorityGroup(i).SplitNoShadowPasses = splitNoShadowPasses;
				}
			}
		}

		/// <summary>
		///		Gets/Sets whether or not the queue will disallow receivers when certain shadow
		///		techniques are used.
		/// </summary>
		public bool ShadowCastersCannotBeReceivers 
		{
			get 
			{
				return shadowCastersCannotBeReceivers;
			}
			set 
			{
				shadowCastersCannotBeReceivers = value;

				// set the value for all priority groups as well
				for(int i = 0; i < priorityGroups.Count; i++) 
				{
					GetPriorityGroup(i).ShadowCastersCannotBeReceivers = shadowCastersCannotBeReceivers;
				}
			}
		}

		#endregion
	}

}
