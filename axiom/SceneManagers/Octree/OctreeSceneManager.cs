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
using System.Collections.Generic;
using Axiom;
using Axiom.Collections;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;

namespace Axiom.SceneManagers.Octree {
    /// <summary>
    /// Summary description for OctreeSceneManager.
    /// </summary>
    public class OctreeSceneManager : SceneManager {
        #region Member Variables
        protected System.Collections.ArrayList boxList = new ArrayList();
        protected System.Collections.ArrayList colorList = new ArrayList();
        //NOTE: "visible" was a Nodelist...could be a custom collection
        protected System.Collections.ArrayList visible = new ArrayList();
        public System.Collections.Hashtable options = new Hashtable();
        protected static long white = 0xFFFFFFFF;
        protected ushort[] indexes = {0,1,1,2,2,3,3,0,0,6,6,5,5,1,3,7,7,4,4,2,6,7,5,4};
        protected long[] colors = {white, white, white, white, white, white, white, white };
        protected float[] corners;
        protected Matrix4 scaleFactor;
        protected int intersect = 0;
        protected int maxDepth;
        protected bool cullCamera;
        protected float worldSize;
        protected int numObjects;
        protected bool looseOctree;
        //protected bool showBoxes;
        protected Octree octree;

        public enum Intersection {
            Outside,
            Inside,
            Intersect
        }

        #endregion

        #region Properties

        public long White {
            get {
                return white;
            }
        }

        #endregion

        public OctreeSceneManager() : base("OcttreeSM") {
            Vector3 Min = new Vector3(-500f,-500f,-500f);
            Vector3 Max = new Vector3(500f,500f,500f);
            int depth = 5; 

            AxisAlignedBox box = new AxisAlignedBox(Min, Max);
			
            Init(box, depth);
        }	

        public OctreeSceneManager(string name, AxisAlignedBox box, int max_depth) : base(name) {
            Init(box, max_depth);
        }

        public Intersection Intersect(AxisAlignedBox box1, AxisAlignedBox box2) {
            intersect++;			
            Vector3[] outside = box1.Corners;
            Vector3[] inside = box2.Corners;

            if(inside[4].x < outside[0].x ||
                inside[4].y < outside[0].y ||
                inside[4].z < outside[0].z ||
                inside[0].x > outside[4].x ||
                inside[0].y > outside[4].y ||
                inside[0].z > outside[4].z ) {

                return Intersection.Outside;
            }

            if(inside[0].x > outside[0].x &&
                inside[0].y > outside[0].y &&
                inside[0].z > outside[0].z &&
                inside[4].x < outside[4].x &&
                inside[4].y < outside[4].y &&
                inside[4].z < outside[4].z ) {

                return Intersection.Inside;
            }
            else {
                return Intersection.Intersect;
            }
        }

        public Intersection Intersect(Sphere sphere, AxisAlignedBox box) {
            intersect++;
            float Radius = sphere.Radius;
            Vector3 Center = sphere.Center;
            Vector3[] Corners = box.Corners;
            float s = 0;
            float d = 0;
            int i;
            bool Partial;

            Radius *= Radius;

            Vector3 MinDistance = (Corners[0] - Center);
            Vector3 MaxDistance = (Corners[4] - Center);

            if((MinDistance.LengthSquared < Radius) && (MaxDistance.LengthSquared < Radius)) {
                return Intersection.Inside;
            }

            //find the square of the distance
            //from the sphere to the box
            for(i=0;i<3;i++) {
                if ( Center[i] < Corners[0][i] ) {
                    s = Center[i] - Corners[0][i];
                    d += s * s;
                }

                else if ( Center[i] > Corners[4][i] ) {
                    s = Center[i] - Corners[4][i];
                    d += s * s;
                }
            }

            Partial = (d <= Radius);

            if(!Partial) {
                return Intersection.Outside;
            }
            else {
                return Intersection.Intersect;
            }
        }

        public void Init(AxisAlignedBox box, int depth) {
            rootSceneNode = new OctreeNode(this, "SceneRoot");

            maxDepth = depth;

            octree = new Octree(null);

            octree.Box = box;

            Vector3 Min = box.Minimum;
            Vector3 Max = box.Maximum;

            octree.HalfSize = (Max - Min) / 2;

            numObjects = 0;

            Vector3 scalar = new Vector3(1.5f,1.5f,1.5f);

            scaleFactor.Scale = scalar;
        }

        public override SceneNode CreateSceneNode() {
            OctreeNode node = new OctreeNode(this);
            sceneNodeList[node.Name] = node;
            return node;
        }

        public override SceneNode CreateSceneNode(string name) {
            OctreeNode node = new OctreeNode(this, name);
            sceneNodeList[node.Name] = node;
            return node;
        }

        public override Camera CreateCamera(string name) {
            Camera cam = new OctreeCamera(name, this);
            cameraList.Add(name, cam);
            // create visible bounds aabb map entry
            camVisibleObjectsMap[cam] = new VisibleObjectsBoundsInfo();
            return cam;
        }

        protected override void UpdateSceneGraph(Camera cam) {
            base.UpdateSceneGraph(cam);
        }

        public override void FindVisibleObjects(Camera cam,  VisibleObjectsBoundsInfo visibleBounds, bool onlyShadowCasters) {
            GetRenderQueue().Clear();
            boxList.Clear();
            visible.Clear();

            if(cullCamera) {
                Camera c = cameraList["CullCamera"];

                if(c != null) {
                    cameraInProgress = cameraList["CullCamera"];
                }
            }

            numObjects = 0;

            //walk the octree, adding all visible Octreenodes nodes to the render queue.
            WalkOctree((OctreeCamera)cam, GetRenderQueue(), octree, visibleBounds, onlyShadowCasters, false);

            // Show the octree boxes & cull camera if required
            if(this.ShowBoundingBoxes || cullCamera) {
                if(this.ShowBoundingBoxes) {
                    for(int i = 0; i < boxList.Count; i++) {
                        WireBoundingBox box = (WireBoundingBox)boxList[i];
						
                        GetRenderQueue().AddRenderable(box);
                    }
                }
				
                if(cullCamera) {
                    OctreeCamera c = (OctreeCamera)GetCamera("CullCamera");

                    if(c != null) {
                        GetRenderQueue().AddRenderable(c);
                    }
                }
            }
        }

        /** Alerts each unculled object, notifying it that it will be drawn.
        * Useful for doing calculations only on nodes that will be drawn, prior
        * to drawing them...
        */
        public void AlertVisibleObjects() {
            int i;

            for(i=0;i<visible.Count;i++) {
                OctreeNode node = (OctreeNode)visible[i];
                //TODO: looks like something is missing here
            }
        }

        /** Walks through the octree, adding any visible objects to the render queue.
        @remarks
        If any octant in the octree if completely within the the view frustum,
        all subchildren are automatically added with no visibility tests.
        */
        public void WalkOctree(OctreeCamera camera, RenderQueue queue, Octree octant, 
                               VisibleObjectsBoundsInfo visibleBounds, bool onlyShadowCasters, bool foundVisible) {
            //return immediately if nothing is in the node.
            if(octant.NumNodes == 0) {
                return;
            }

            Visibility v = Visibility.None;
 
            if(foundVisible) {
                v = Visibility.Full;
            }
            else if(octant == octree) {
                v = Visibility.Partial;
            }
            else {
                AxisAlignedBox box = octant.CullBounds;
                v = camera.GetVisibility(box);
            }

            // if the octant is visible, or if it's the root node...
            if(v != Visibility.None) {
                if(this.ShowBoundingBoxes) {
                    // TODO: Implement Octree.WireBoundingBox
                    //boxList.Add(octant.WireBoundingBox); 
                }

                bool vis = true;

                for(int i = 0; i < octant.NodeList.Count; i++) {
                    OctreeNode node = (OctreeNode)octant.NodeList[i];
					
                    // if this octree is partially visible, manually cull all
                    // scene nodes attached directly to this level.

                    if(v == Visibility.Partial) {
                        vis = camera.IsObjectVisible(node.WorldAABB);
                    }

                    if(vis)  {
                        numObjects++;
                        node.AddToRenderQueue(camera, queue, onlyShadowCasters, visibleBounds);
                        visible.Add(node);

                        if(DisplayNodes) {
                            GetRenderQueue().AddRenderable(node);
                        }

                        // check if the scene manager or this node wants the bounding box shown.
                        if(node.ShowBoundingBox || this.ShowBoundingBoxes) {
                            node.AddBoundingBoxToQueue(queue);
                        }
                    }
                }
				
                if(octant.Children[0,0,0] != null ) WalkOctree(camera, queue, octant.Children[0,0,0], visibleBounds, onlyShadowCasters, ( v == Visibility.Full ) );

                if(octant.Children[1,0,0] != null ) WalkOctree(camera, queue, octant.Children[1,0,0], visibleBounds, onlyShadowCasters, ( v == Visibility.Full ) );

                if(octant.Children[0,1,0] != null ) WalkOctree(camera, queue, octant.Children[0,1,0], visibleBounds, onlyShadowCasters, ( v == Visibility.Full ) );

                if(octant.Children[1,1,0] != null ) WalkOctree(camera, queue, octant.Children[1,1,0], visibleBounds, onlyShadowCasters, ( v == Visibility.Full ) );

                if(octant.Children[0,0,1] != null ) WalkOctree(camera, queue, octant.Children[0,0,1], visibleBounds, onlyShadowCasters, ( v == Visibility.Full ) );

                if(octant.Children[1,0,1] != null ) WalkOctree(camera, queue, octant.Children[1,0,1], visibleBounds, onlyShadowCasters, ( v == Visibility.Full ) );

                if(octant.Children[0,1,1] != null ) WalkOctree(camera, queue, octant.Children[0,1,1], visibleBounds, onlyShadowCasters, ( v == Visibility.Full ) );

                if(octant.Children[1,1,1] != null ) WalkOctree(camera, queue, octant.Children[1,1,1], visibleBounds, onlyShadowCasters, ( v == Visibility.Full ) );
            }			
        }

        /** Checks the given OctreeNode, and determines if it needs to be moved
        * to a different octant.
        */
        public void UpdateOctreeNode(OctreeNode node) {
            AxisAlignedBox box = node.WorldAABB;
        
            if(box.IsNull) {
                return;
            }

            if(node.Octant == null) {
                //if outside the octree, force into the root node.
                if(!node.IsInBox(octree.Box)) {
                    octree.AddNode(node);
                }
                else {
                    AddOctreeNode(node, octree);
                    return;
                }
            }

            if(!node.IsInBox(node.Octant.Box)) {
                RemoveOctreeNode(node);

                //if outside the octree, force into the root node.
                if(!node.IsInBox(octree.Box)) {
                    octree.AddNode(node);
                }
                else {
                    AddOctreeNode(node,octree);
                }
            }
        }

        /*public void RemoveOctreeNode(OctreeNode node, Octree tree, int depth)
        {

        }*/

        /** Only removes the node from the octree.  It leaves the octree, even if it's empty.
        */
        public void RemoveOctreeNode(OctreeNode node) {
            Octree tree = node.Octant;

            if(tree != null) {
                tree.RemoveNode(node);
            }
        }

        public override void DestroySceneNode(string name) {
            OctreeNode node = (OctreeNode)GetSceneNode(name);

            if(node != null) {
                RemoveOctreeNode(node);
            }

            base.DestroySceneNode(name);
        }

        public void AddOctreeNode(OctreeNode node, Octree octant) {
            AddOctreeNode(node, octant, 0);
        }

        public void AddOctreeNode(OctreeNode node, Octree octant, int depth) {
            AxisAlignedBox box = node.WorldAABB;

            //if the octree is twice as big as the scene node,
            //we will add it to a child.
            if((depth < maxDepth) && octant.IsTwiceSize(box)) {
                int x,y,z;

                octant.GetChildIndexes(box, out x, out y, out z);

                if(octant.Children[x,y,z] == null) {
                    octant.Children[x,y,z] = new Octree(octant);

                    Vector3[] corners = octant.Box.Corners;

                    Vector3 min, max;

                    if(x == 0) {
                        min.x = corners[0].x;
                        max.x = (corners[0].x + corners[4].x) / 2;
                    }
                    else {
                        min.x = (corners[0].x + corners[4].x) / 2;
                        max.x = corners[4].x;
                    }

                    if ( y == 0 ) {
                        min.y = corners[ 0 ].y;
                        max.y = ( corners[ 0 ].y + corners[ 4 ].y ) / 2;
                    }

                    else {
                        min.y = ( corners[ 0 ].y + corners[ 4 ].y ) / 2;
                        max.y = corners[ 4 ].y;
                    }

                    if ( z == 0 ) {
                        min.z = corners[ 0 ].z;
                        max.z = ( corners[ 0 ].z + corners[ 4 ].z ) / 2;
                    }

                    else {
                        min.z = ( corners[ 0 ].z + corners[ 4 ].z ) / 2;
                        max.z = corners[ 4 ].z;
                    }

                    octant.Children[x,y,z].Box.SetExtents(min,max);
                    octant.Children[x,y,z].HalfSize = (max - min) / 2;

                }

                AddOctreeNode(node, octant.Children[x,y,z], ++depth);
            }
            else {
                octant.AddNode(node);
            }
        }

        /*public void AddOctreeNode(OctreeNode node, Octree octree)
        {


        }*/

        /** Resizes the octree to the given size */
        public void Resize(AxisAlignedBox box) {
            List<SceneNode> nodes = new List<SceneNode>();

            FindNodes(this.octree.Box, nodes, null, true, this.octree);

            octree = new Octree(null);
            octree.Box = box;

            foreach (OctreeNode node in nodes) {
                node.Octant = null;
                UpdateOctreeNode(node);
            }
        }

        public void FindNodes(AxisAlignedBox box, List<SceneNode> sceneNodeList, SceneNode exclude, bool full, Octree octant) {
            System.Collections.ArrayList localList = new System.Collections.ArrayList();
            if(octant == null) {
                octant = this.octree;
            }

            if(!full) {
                AxisAlignedBox obox = octant.CullBounds;

                Intersection isect = this.Intersect(box,obox);

                if(isect == Intersection.Outside) {
                    return;
                }

                full = (isect == Intersection.Inside);
            }

            for(int i=0;i<octant.NodeList.Count;i++) {
                OctreeNode node = (OctreeNode)octant.NodeList[i];

                if(node != exclude) {
                    if(full) {
                        localList.Add(node);
                    }
                    else {
                        Intersection nsect = this.Intersect(box,node.WorldAABB);

                        if(nsect != Intersection.Outside) {
                            localList.Add(node);
                        }
                    }
                }
            }

            if ( octant.Children[0,0,0] != null ) FindNodes( box, sceneNodeList, exclude, full, octant.Children[0,0,0]);

            if ( octant.Children[1,0,0] != null ) FindNodes( box, sceneNodeList, exclude, full, octant.Children[1,0,0]);

            if ( octant.Children[0,1,0] != null ) FindNodes( box, sceneNodeList, exclude, full, octant.Children[0,1,0] );

            if ( octant.Children[1,1,0] != null ) FindNodes( box, sceneNodeList, exclude, full, octant.Children[1,1,0]);

            if ( octant.Children[0,0,1] != null ) FindNodes( box, sceneNodeList, exclude, full, octant.Children[0,0,1]);

            if ( octant.Children[1,0,1] != null ) FindNodes( box, sceneNodeList, exclude, full, octant.Children[1,0,1]);

            if ( octant.Children[0,1,1] != null ) FindNodes( box, sceneNodeList, exclude, full, octant.Children[0,1,1]);

            if ( octant.Children[1,1,1] != null ) FindNodes( box, sceneNodeList, exclude, full, octant.Children[1,1,1]);

        }

        public void FindNodes(Sphere sphere, List<SceneNode> sceneNodeList, SceneNode exclude, bool full, Octree octant) {
            //TODO: Implement
        }

        public bool SetOption(string key,object val) {
            bool ret = false;
            switch(key) {
                case "Size":
                    Resize((AxisAlignedBox)val);
                    ret = true;
                    break;
                case "Depth":
                    maxDepth = (int)val;
                    Resize(this.octree.Box);
                    ret = true;
                    break;
                case "ShowOctree":
                    this.ShowBoundingBoxes = (bool)val;
                    ret = true;
                    break;
                case "CullCamera":
                    cullCamera = (bool)val;
                    ret = true;
                    break;
            }

            return ret;
        }

        public bool GetOption() {
            return true;//TODO: Implement
        }
    }
}
