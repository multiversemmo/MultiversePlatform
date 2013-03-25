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
	///		IRenderables in the queue grouped by priority.
	/// </summary>
	/// <remarks>
	///		This class simply groups renderables for rendering. All the 
	///		renderables contained in this class are destined for the same
	///		RenderQueueGroup (coarse groupings like those between the main
	///		scene and overlays) and have the same priority (fine groupings
	///		for detailed overlap control).
	/// </remarks>
	public class RenderPriorityGroup 
	{
		#region Fields
			
		protected internal ArrayList transparentPasses = new ArrayList();
		/// <summary>
		///		Solid pass list, used when no shadows, modulative shadows, or ambient passes for additive.
		/// </summary>
		protected internal SortedList solidPasses;
		/// <summary>
		///		Solid per-light pass list, used with additive shadows.
		/// </summary>
		protected internal SortedList solidPassesDiffuseSpecular;
		/// <summary>
		///		Solid decal (texture) pass list, used with additive shadows.
		/// </summary>
		protected internal SortedList solidPassesDecal;
		/// <summary>
		///		Solid pass list, used when shadows are enabled but shadow receive is turned off for these passes.
		/// </summary>
		protected internal SortedList solidPassesNoShadow;
		/// <summary>
		///		Should passes be split by their lighting stage?
		/// </summary>
		protected bool splitPassesByLightingType;
		protected bool splitNoShadowPasses;
        protected bool shadowCastersCannotBeReceivers;

		#endregion Fields

		#region Constructor

		/// <summary>
		///    Default constructor.
		/// </summary>
		internal RenderPriorityGroup(bool splitPassesByLightingType, bool splitNoShadowPasses,
									 bool shadowCastersCannotBeReceivers) 
		{
			// sorted list, using Pass as a key (sorted based on hashcode), and IRenderable as the value
			solidPasses = new SortedList(new SolidSort(), 50);
			solidPassesDiffuseSpecular = new SortedList(new SolidSort(), 50);
			solidPassesDecal = new SortedList(new SolidSort(), 50);
			solidPassesNoShadow = new SortedList(new SolidSort(), 50);
			this.splitPassesByLightingType = splitPassesByLightingType;
			this.splitNoShadowPasses = splitNoShadowPasses;
            this.shadowCastersCannotBeReceivers = shadowCastersCannotBeReceivers;
		}

		#endregion Constructor

		#region Methods

		/// <summary>
		///		Add a renderable to this group.
		/// </summary>
		/// <param name="renderable">Renderable to add to the queue.</param>
		public void AddRenderable(IRenderable renderable) 
		{
			Technique t = null;
                
			// Check material & technique supplied (the former since the default implementation
			// of Technique is based on it for backwards compatibility
			if(renderable.Material == null || renderable.Technique == null) 
			{
				// use default if not found
				t = MaterialManager.Instance.GetByName("BaseWhite").GetTechnique(0);
			}
			else 
			{
				t = renderable.Technique;
			}

			// Transparent and depth settings mean depth sorting is required?
			if(t.IsTransparent && !(t.DepthWrite && t.DepthCheck) ) 
			{
				AddTransparentRenderable(t, renderable);
			}
			else 
			{
				if(splitNoShadowPasses && 
				   (!t.Parent.ReceiveShadows ||
					renderable.CastsShadows && shadowCastersCannotBeReceivers))
				{
					// Add solid renderable and add passes to no-shadow group
					AddSolidRenderable(t, renderable, true);
				}
				else 
				{
					if(splitPassesByLightingType)
					{
						AddSolidRenderableSplitByLightType(t, renderable);
					}
					else 
					{
						AddSolidRenderable(t, renderable, false);
					}
				}
			}
		}

		/// <summary>
		///		Internal method for adding a solid renderable
		/// </summary>
		/// <param name="technique">Technique to use for this renderable.</param>
		/// <param name="renderable">Renderable to add to the queue.</param>
		/// <param name="noShadows">True to add to the no shadow group, false otherwise.</param>
		protected void AddSolidRenderable(Technique technique, IRenderable renderable, bool noShadows) 
		{
			SortedList passMap = null;

			if(noShadows) 
			{
				passMap = solidPassesNoShadow;
			}
			else 
			{
				passMap = solidPasses;
			}

			for(int i = 0; i < technique.NumPasses; i++) 
			{
				Pass pass = technique.GetPass(i);

				if(passMap[pass] == null) 
				{
					// add a new list to hold renderables for this pass
					passMap.Add(pass, new RenderableList());
				}

				// add to solid list for this pass
				RenderableList solidList = (RenderableList)passMap[pass];

				solidList.Add(renderable);
			}
		}

		/// <summary>
		///		Internal method for adding a solid renderable ot the group based on lighting stage.
		/// </summary>
		/// <param name="technique">Technique to use for this renderable.</param>
		/// <param name="renderable">Renderable to add to the queue.</param>
		protected void AddSolidRenderableSplitByLightType(Technique technique, IRenderable renderable) 
		{
			// Divide the passes into the 3 categories
			for (int i = 0; i < technique.IlluminationPassCount; i++) 
			{
				// Insert into solid list
				IlluminationPass illpass = technique.GetIlluminationPass(i);
				SortedList passMap = null;

				switch(illpass.Stage) 
				{
					case IlluminationStage.Ambient:
						passMap = solidPasses;
						break;
					case IlluminationStage.PerLight:
						passMap = solidPassesDiffuseSpecular;
						break;
					case IlluminationStage.Decal:
						passMap = solidPassesDecal;
						break;
				}

				RenderableList solidList = (RenderableList)passMap[illpass.Pass];

				if(solidList == null) 
				{
					// add a new list to hold renderables for this pass
					solidList = new RenderableList();
					passMap.Add(illpass.Pass, solidList);
				}

				solidList.Add(renderable);
			}
		}

		/// <summary>
		///		Internal method for adding a transparent renderable.
		/// </summary>
		/// <param name="technique">Technique to use for this renderable.</param>
		/// <param name="renderable">Renderable to add to the queue.</param>
		protected void AddTransparentRenderable(Technique technique, IRenderable renderable) 
		{
			for(int i = 0; i < technique.NumPasses; i++) 
			{
				// add to transparent list
				transparentPasses.Add(new RenderablePass(renderable, technique.GetPass(i)));
			}
		}

		/// <summary>
		///		Clears all the internal lists.
		/// </summary>
		public void Clear() 
		{
            lock (Pass.PassLock)
            {
                PassList graveyardList = Pass.GraveyardList;

                // Delete queue groups which are using passes which are to be
                // deleted, we won't need these any more and they clutter up 
                // the list and can cause problems with future clones
                for (int i = 0; i < graveyardList.Count; i++)
                {
                    RemoveSolidPassEntry((Pass)graveyardList[i]);
                }

                // Now remove any dirty passes, these will have their hashes recalculated
                // by the parent queue after all groups have been processed
                // If we don't do this, the std::map will become inconsistent for new insterts
                PassList dirtyList = Pass.DirtyList;

                // Delete queue groups which are using passes which are to be
                // deleted, we won't need these any more and they clutter up 
                // the list and can cause problems with future clones
                for (int i = 0; i < dirtyList.Count; i++)
                {
                    RemoveSolidPassEntry((Pass)dirtyList[i]);
                }

                // We do NOT clear the graveyard or the dirty list here, because 
                // it needs to be acted on for all groups, the parent queue takes 
                // care of this afterwards

                // We do not clear the unchanged solid pass maps, only the contents of each list
                // This is because we assume passes are reused a lot and it saves resorting
                ClearSolidPassMap(solidPasses);
                ClearSolidPassMap(solidPassesDiffuseSpecular);
                ClearSolidPassMap(solidPassesDecal);
                ClearSolidPassMap(solidPassesNoShadow);

                // Always empty the transparents list
                transparentPasses.Clear();
            }
		}

		public void ClearSolidPassMap(SortedList list) 
		{
			// loop through and clear the renderable containers for the stored passes
			for(int i = 0; i < list.Count; i++) 
			{
				((RenderableList)list.GetByIndex(i)).Clear();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Pass GetSolidPass(int index) 
		{
			Debug.Assert(index < solidPasses.Count, "index < solidPasses.Count");
			return (Pass)solidPasses.GetKey(index);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public RenderableList GetSolidPassRenderables(int index) 
		{
			Debug.Assert(index < solidPasses.Count, "index < solidPasses.Count");
			return (RenderableList)solidPasses.GetByIndex(index);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public RenderablePass GetTransparentPass(int index) 
		{
			Debug.Assert(index < transparentPasses.Count, "index < transparentPasses.Count");
			return (RenderablePass)transparentPasses[index];
		}

		/// <summary>
		///    Sorts the objects which have been added to the queue; transparent objects by their 
		///    depth in relation to the passed in Camera, solid objects in order to minimize
		///    render state changes.
		/// </summary>
		/// <remarks>
		///    Solid passes are already stored in a sorted structure, so nothing extra needed here.
		/// </remarks>
		/// <param name="camera">Current camera to use for depth sorting.</param>
		public void Sort(Camera camera) 
		{
			// sort the transparent objects using the custom IComparer
			transparentPasses.Sort(new TransparencySort(camera));
		}

		/// <summary>
		///		Remove a pass entry from all solid pass maps
		/// </summary>
		/// <param name="pass">Reference to the pass to remove.</param>
		public void RemoveSolidPassEntry(Pass pass) 
		{
			if(solidPasses[pass] != null) 
			{
				solidPasses.Remove(pass);
			}

			if(solidPassesDecal[pass] != null) 
			{
				solidPassesDecal.Remove(pass);
			}

			if(solidPassesDiffuseSpecular[pass] != null) 
			{
				solidPassesDiffuseSpecular.Remove(pass);
			}

			if(solidPassesNoShadow[pass] != null) 
			{
				solidPassesNoShadow.Remove(pass);
			}
		}

		#endregion

		#region Properties

        public SortedList SolidPasses
        {
            get
            {
                return solidPasses;
            }
        }

		/// <summary>
		///    Gets the number of non-transparent passes for this priority group.
		/// </summary>
		public int NumSolidPasses 
		{
			get 
			{
				return solidPasses.Count;
			}
		}

		/// <summary>
		///    Gets the number of transparent passes for this priority group.
		/// </summary>
		public int NumTransparentPasses 
		{
			get 
			{
				return transparentPasses.Count;
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
			}
		}
		
		#endregion

		#region Internal classes

		/// <summary>
		/// 
		/// </summary>
		class SolidSort : IComparer 
		{
			#region IComparer Members

			public int Compare(object x, object y) 
			{
				// if they are the same, return 0
				if(x == y)
					return 0;

				Pass a = x as Pass;
				Pass b = y as Pass;

                if (a == null || b == null)
                    return 0;

				// sorting by pass hash
                if (a.GetHashCode() == b.GetHashCode())
                    return (a.passId < b.passId) ? -1 : 1;
                return (a.GetHashCode() < b.GetHashCode()) ? -1 : 1;
			}

			#endregion            
		}

		/// <summary>
		///		Nested class that implements IComparer for transparency sorting.
		/// </summary>
		class TransparencySort : IComparer 
		{
			private Camera camera;

			public TransparencySort(Camera camera) 
			{
				this.camera = camera;
			}

			#region IComparer Members

			public int Compare(object x, object y) 
			{
				if(x == null  || y == null)
					return 0;

				// if they are the same, return 0
				if(x == y)
					return 0;

				RenderablePass a = x as RenderablePass;
				RenderablePass b = y as RenderablePass;

				float adepth = a.renderable.GetSquaredViewDepth(camera);
				float bdepth = b.renderable.GetSquaredViewDepth(camera);

				if(adepth == bdepth) 
				{
					if(a.pass.GetHashCode() < b.pass.GetHashCode()) 
					{
						return 1;
					}
					else 
					{
						return -1;
					}
				}
				else 
				{
					// sort descending by depth, meaning further objects get drawn first
					if(adepth > bdepth)
						return 1;
					else
						return -1;
				}
			}

			#endregion
		}

		#endregion
	}

}
