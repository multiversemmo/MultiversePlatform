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
using Axiom;
using Axiom.Collections;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Octree {
    /// <summary>
    /// Summary description for Octree.
    /// </summary>
    public class Octree {
        #region Member Variables

        /** Returns the number of scene nodes attached to this octree
        */
        protected int numNodes;

        /** Public list of SceneNodes attached to this particular octree
        */
        protected NodeCollection nodeList = new NodeCollection();

        /** The bounding box of the octree
        @remarks
        This is used for octant index determination and rendering, but not culling
        */
        protected AxisAlignedBox box = new AxisAlignedBox();
        /** Creates the wire frame bounding box for this octant
        */
        protected WireBoundingBox wireBoundingBox;
		
        /** Vector containing the dimensions of this octree / 2
        */
        protected Vector3 halfSize;

        /** 3D array of children of this octree.
        @remarks
        Children are dynamically created as needed when nodes are inserted in the Octree.
        If, later, the all the nodes are removed from the child, it is still kept arround.
        */
        public Octree[,,] Children = new Octree[8,8,8];

        protected Octree parent = null;
		
        #endregion

        #region Properties
        public int NumNodes {
            get{return numNodes;}
            set{numNodes = value;}
        }

        public NodeCollection NodeList {
            get{return nodeList;}
            //set{nodeList = value;}
        }

        public WireBoundingBox BoundingBox {
            get{
                // Create a WireBoundingBox if needed
                if(this.wireBoundingBox == null) {
                    this.wireBoundingBox = new WireBoundingBox();
                }
					
                this.wireBoundingBox.InitAABB(this.box);
                return this.wireBoundingBox;
            }

            set{wireBoundingBox = value;}
        }

        public Vector3 HalfSize {
            get{return halfSize;}
            set{halfSize = value;}
        }

        public AxisAlignedBox Box {
            get{return box;}
            set{box = value;}
        }

        #endregion 
        public Octree(Octree parent) {
            this.wireBoundingBox = null;
            this.HalfSize = new Vector3();

            this.parent = parent;
            this.NumNodes = 0;
        }

        public void AddNode(OctreeNode node) {
			// TODO: Att some points, some nodes seemed to be added if they already existed.  Investigate.
            nodeList[node.Name] = node;
            node.Octant = this;
            Ref();
        }

        public void RemoveNode(OctreeNode node) {
            OctreeNode check;
            int i;
            int Index;

            Index = NodeList.Count - 1;

            for(i=Index;i>0;i--) {
                check = (OctreeNode)NodeList[i];

                if(check == node) {
                    node.Octant = null;
                    NodeList.RemoveAt(i);
                    UnRef();
                }
            }
        }

        /// <summary>
        ///  Determines if this octree is twice as big as the given box.
        ///@remarks
        ///	This method is used by the OctreeSceneManager to determine if the given
        ///	box will fit into a child of this octree.
        /// </summary>

        public bool IsTwiceSize(AxisAlignedBox box) {
            Vector3[] pts1 = this.box.Corners;
            Vector3[] pts2 = box.Corners;

            return ( ( pts2[4].x -pts2[0].x ) <= ( pts1[4].x - pts1[0].x ) / 2 ) &&
                ( ( pts2[4].y - pts2[0].y ) <= ( pts1[4].y - pts1[0].y ) / 2 ) &&
                ( ( pts2[4].z - pts2[0].z ) <= ( pts1[4].z - pts1[0].z ) / 2 ) ;

        }

        /// <summary>
        /// Returns the appropriate indexes for the child of this octree into which the box will fit.
        ///@remarks
        ///	This is used by the OCtreeSceneManager to determine which child to traverse next when
        ///finding the appropriate octree to insert the box.  Since it is a loose octree, only the
        ///center of the box is checked to determine the octant.
        /// </summary>
        public void GetChildIndexes(AxisAlignedBox aabox, out int x, out int y, out int z) {

            Vector3 max = this.box.Maximum;
            Vector3 min = aabox.Minimum;

            Vector3 Center = this.box.Maximum.MidPoint(this.box.Minimum);
            Vector3 CheckCenter = aabox.Maximum.MidPoint(aabox.Minimum);

            if(CheckCenter.x > Center.x) {
                x = 1;
            }
            else {
                x = 0;
            }

			
            if(CheckCenter.y > Center.y) {
                y = 1;
            }
            else {
                y = 0;
            }

			
            if(CheckCenter.z > Center.z) {
                z = 1;
            }
            else {
                z = 0;
            }
        }

        /// <summary>
        ///  Creates the AxisAlignedBox used for culling this octree.
        /// </summary>
        /// <remarks>
        ///     Since it's a loose octree, the culling bounds can be different than the actual bounds of the octree.
        /// </remarks>
        public AxisAlignedBox CullBounds {
            get {
                Vector3[] Corners = this.box.Corners;
                box.SetExtents(Corners[0] - this.HalfSize, Corners[4] + this.HalfSize);

                return box;
            }
        }

        public void Ref() {
            numNodes++;

            if(parent != null) {
                parent.Ref();
            }
        }

        public void UnRef() {
            numNodes--;

            if(parent != null) {
                parent.UnRef();
            }
        }

    }
}
