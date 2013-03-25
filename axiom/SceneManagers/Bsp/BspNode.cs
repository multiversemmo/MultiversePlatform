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

using Axiom;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Collections;
using Axiom.MathLib.Collections;

namespace Axiom.SceneManagers.Bsp
{
	/// <summary>
	///    Encapsulates a node in a BSP tree.
	/// </summary>
	/// <remarks>
	///    A BSP tree represents space partitioned by planes . The space which is
	///    partitioned is either the world (in the case of the root node) or the space derived
	///    from their parent node. Each node can have elements which are in front or behind it, which are
	///    it's children and these elements can either be further subdivided by planes,
	///    or they can be undivided spaces or 'leaf nodes' - these are the nodes which actually contain
	///    objects and world geometry.The leaves of the tree are the stopping point of any tree walking algorithm,
	///    both for rendering and collision detection etc.<p/>
	///    We choose not to represent splitting nodes and leaves as separate structures, but to merge the two for simplicity
	///    of the walking algorithm. If a node is a leaf, the IsLeaf property returns true and both GetFront() and
	///    GetBack() return null references. If the node is a partitioning plane IsLeaf returns false and GetFront()
	///    and GetBack() will return the corresponding BspNode objects.
	/// </remarks>
	public class BspNode
	{
		#region Protected members
		protected bool isLeaf;
		protected BspNode frontNode;
		protected BspNode backNode;

		protected Plane splittingPlane = new Plane();
		protected AxisAlignedBox boundingBox = new AxisAlignedBox();
		protected MovableObjectCollection objectList = new MovableObjectCollection();
		
		protected int numFaceGroups;
		protected int faceGroupStart;
		protected BspLevel owner;
		protected BspBrush[] solidBrushes;

		/// <summary>
		///		The cluster number of this leaf.
		/// </summary>
		/// <remarks>
		///		Leaf nodes are assigned to 'clusters' of nodes, which are used to group nodes together for
		///		visibility testing. There is a lookup table which is used to determine if one cluster of leaves
		///		is visible from another cluster. Whilst it would be possible to expand all this out so that
		///		each node had a list of pointers to other visible nodes, this would be very expensive in terms
		///		of storage (using the cluster method there is a table which is 1-bit squared per cluster, rounded
		///		up to the nearest byte obviously, which uses far less space than 4-bytes per linked node per source
		///		node). Of course the limitation here is that you have to each leaf in turn to determine if it is visible
		///		rather than just following a list, but since this is only done once per frame this is not such a big
		///		overhead.
		/// </remarks>
		protected int visCluster;
		#endregion

		#region Public properties
		public BspLevel Owner
		{
			get { return owner; }
			set { owner = value; }
		}

		/// <summary>
		///		Returns true if this node is a leaf (i.e. contains geometry) or false if it is a splitting plane.
		///	</summary>
		///	<remarks>
		///		A BspNode can either be a splitting plane (the typical representation of a BSP node) or an undivided
		///		region contining geometry (a leaf node). Ogre represents both using the same class for simplicity
		///		of tree walking. However it is important that you use this method to determine which type you are dealing
		///		with, since certain methods are only supported with one of the subtypes. Details are given in the individual methods.
		///		Note that I could have represented splitting / leaf nodes as a class hierarchy but the
		///		virtual methods / run-time type identification would have a performance hit, and it would not make the
		///		code much (any?) simpler anyway. I think this is a fair trade-off in this case.
		/// </remarks>
		public bool IsLeaf
		{
			get { return isLeaf; }
			set { isLeaf = value; }
		}

		/// <summary>
		///		Gets a reference to a <see cref="Plugin_BSPSceneManager.BspSceneNode"/> containing the subspace on the 
		///		positive side of the splitting plane.
		/// </summary>
		/// <remarks>
		///		This method should only be called on a splitting node, i.e. where <see cref="Plugin_BSPSceneManager.BspSceneNode"/> returns false. 
		///		Calling this method on a leaf node will throw an AxiomException.
		/// </remarks>
		public BspNode FrontNode
		{
			get 
			{ 
				if(IsLeaf)
					throw new AxiomException("This property is not valid on a leaf node.");

				return frontNode; 
			}
			set
			{
				if(IsLeaf)
					throw new AxiomException("This property is not valid on a leaf node.");

				frontNode = value;
			}
		}

		/// <summary>
		///		Returns a pointer to a BspNode containing the subspace on the negative side of the splitting plane.
		///	</summary>
		///	<remarks>
		///		This method should only be called on a splitting node, i.e. where <see cref="Plugin_BSPSceneManager.BspSceneNode"/> returns false. 
		///		Calling this method on a leaf node will throw an AxiomException.
		/// </remarks>
		public BspNode BackNode
		{
			get 
			{ 
				if(IsLeaf)
					throw new AxiomException("This property is not valid on a leaf node.");

				return backNode; 
			}
			set
			{
				if(IsLeaf)
					throw new AxiomException("This property is not valid on a leaf node.");

				backNode = value;
			}
		}

		/// <summary>
		///		Returns details of the plane which is used to subdivide the space of his node's children.
		/// </summary>
		/// <remarks>
		///		This method should only be called on a splitting node, i.e. where <see cref="Plugin_BSPSceneManager.BspSceneNode"/> returns false. 
		///		Calling this method on a leaf node will throw an AxiomException.
		/// </remarks>
		public Plane SplittingPlane
		{
			get 
			{ 
				/*if(IsLeaf)
					throw new AxiomException("This property is not valid on a leaf node.");*/

				return splittingPlane; 
			}
			set
			{
				splittingPlane = value;
			}
		}

		/// <summary>
		///		Returns the axis-aligned box which contains this node if it is a leaf.
		///	</summary>
		///	<remarks>
		///		This method should only be called on a leaf node. It returns a box which can be used in calls like
		///		<see cref="Camera.IsVisible"/> to determine if the leaf node is visible in the view.
		/// </remarks>
		public AxisAlignedBox BoundingBox
		{
			get 
			{ 
				if(!IsLeaf)
					throw new AxiomException("This property is only valid on a leaf node.");

				return boundingBox; 
			}
			set
			{
				boundingBox = value;
			}
		}

		/// <summary>
		///		Returns the number of faces contained in this leaf node.
		/// </summary>
		/// <remarks>
		///		Should only be called on a leaf node.
		/// </remarks>
		public int NumFaceGroups
		{
			get 
			{ 
				if(!IsLeaf)
					throw new AxiomException("This property is only valid on a leaf node.");

				return numFaceGroups; 
			}
			set
			{
				numFaceGroups = value;
			}
		}

		/// <summary>
		///		Returns the index to the face group index list for this leaf node.
		/// </summary>
		/// <remarks>
		///		The contents of this buffer is a list of indexes which point to the
		///		actual face groups held in a central buffer in the BspLevel class (in
		///		actual fact for efficency the indexes themselves are also held in a single
		///		buffer in BspLevel too). The reason for this indirection is that the buffer
		///		of indexes to face groups is organised in chunks relative to nodes, whilst the
		///		main buffer of face groups may not be.
		///		Should only be called on a leaf node.
		/// </remarks>
		public int FaceGroupStart
		{
			get 
			{ 
				if(!IsLeaf)
					throw new AxiomException("This property is only valid on a leaf node.");

				return faceGroupStart; 
			}
			set
			{
				faceGroupStart = value;
			}
		}

		public MovableObjectCollection Objects
		{
			get { return objectList; }
		}

		/// <summary>
		///		Get the list of solid Brushes for this node.	
		/// </summary>
		/// <remarks>
		///		Only applicable for leaf nodes. 
		/// </remarks>
		public BspBrush[] SolidBrushes
		{
			get { return solidBrushes; }
			set { solidBrushes = value; }
		}

		public int VisCluster
		{
			get { return visCluster; }
			set { visCluster = value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		///		Constructor, only to be used by BspLevel.
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="isLeaf"></param>
		public BspNode(BspLevel owner, bool isLeaf)
		{
			this.owner = owner;
			this.isLeaf = isLeaf;
		}

		public BspNode()
		{
		}
		#endregion
		
		#region Methods
		/// <summary>
		///		Determines which side of the splitting plane a worldspace point is.
		///	</summary>
		///	<remarks>
		///		This method should only be called on a splitting node, i.e. where <see cref="Plugin_BSPSceneManager.BspSceneNode"/> returns false. 
		///		Calling this method on a leaf node will throw an exception.
		/// </remarks>
		public PlaneSide GetSide(Vector3 point)
		{
			if(IsLeaf)
				throw new Exception("This property is not valid on a leaf node.");

			 return splittingPlane.GetSide(point);
		}

		/// <summary>
		///		Gets the next node down in the tree, with the intention of locating the leaf containing the given point.
		/// </summary>
		/// <remarks>
		///		This method should only be called on a splitting node, i.e. where <see cref="Plugin_BSPSceneManager.BspSceneNode"/> returns false. 
		///		Calling this method on a leaf node will throw an exception.
		/// </remarks>
		public BspNode GetNextNode(Vector3 point)
		{
			if(IsLeaf)
				throw new Exception("This property is not valid on a leaf node.");

			PlaneSide sd = GetSide(point);
			        
			if(sd == PlaneSide.Negative)
				return this.BackNode;
			else 
				return this.FrontNode;
		}
		
		/// <summary>
		///		Determines if the passed in node (must also be a leaf) is visible from this leaf.
		///	</summary>
		///	<remarks>
		///		Must only be called on a leaf node, and the parameter must also be a leaf node. If
		///		this method returns true, then the leaf passed in is visible from this leaf.
		///		Note that internally this uses the Potentially Visible Set (PVS) which is precalculated
		///		and stored with the BSP level.
		///	</remarks>
		public bool IsLeafVisible(BspNode leaf)
		{
			return owner.IsLeafVisible(this, leaf);
		}

		/// <summary>
		///		Internal method for telling the node that a movable intersects it.
		/// </summary>
		/// <param name="?"></param>
		public void AddObject(MovableObject obj)
		{
			objectList.Add(obj);
		}
		
		/// <summary>
		///		Internal method for telling the node that a movable no longer intersects it.
		///	</summary>
		public void RemoveObject(MovableObject obj)
		{
			objectList.Remove(obj);
		}

		/// <summary>
		///		Gets the signed distance to the dividing plane.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public float GetDistance(Vector3 pos)
		{
			if(IsLeaf)
				throw new Exception("This property is not valid on a leaf node.");

			return splittingPlane.GetDistance(pos);
		}
		#endregion
	}

	public class BspBrush
	{
		private PlaneList planes;
		private SceneQuery.WorldFragment fragment;

		public PlaneList Planes
		{
			get { return planes; }
			set { planes = value; }
		}

		public SceneQuery.WorldFragment Fragment
		{
			get { return fragment; }
			set { fragment = value; }
		}

		public BspBrush()
		{
			planes = new PlaneList();
			fragment = new SceneQuery.WorldFragment();
		}

		public BspBrush(PlaneList planes, SceneQuery.WorldFragment fragment)
		{
			this.planes = planes;
			this.fragment = fragment;
		}
	}
}