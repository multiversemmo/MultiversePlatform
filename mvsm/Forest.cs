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
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Multiverse;
using Axiom.MathLib;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Utility;
using Multiverse.CollisionLib;

namespace Axiom.SceneManagers.Multiverse
{
	/// <summary>
	/// Summary description for Forest.
	/// </summary>
    public class Forest : SimpleRenderable, IBoundarySemantic, IDisposable
    {
        private List<TreeGroup> groups = new List<TreeGroup>();

        private List<TreeType> treeTypes;
        private List<TreeType> pendingTreeTypes;

        // for now only one speedWind object per forest
        private SpeedWindWrapper speedWind;
        private float windStrength = 0.5f;
        private Vector3 windDirection;
        private String windFilename;

        private Random rand;
        private float sizeX;
        private float sizeZ;
        private float offX;
        private float offZ;
        private Boundary boundary = null;

        private int seed;

        private AxisAlignedBox bounds = null;

        private Vector3 lastCameraDirection = new Vector3();
        private Vector3 lastCameraLocation = new Vector3();

        private bool inBoundary = false;

        private SceneNode parentSceneNode;

        private static int uniqueNum = 0;

        public Forest(int seed, String name, SceneNode parentSceneNode)
        {

            this.seed = seed;
            this.name = name;

            // create a scene node
            if (parentSceneNode == null)
            {
                parentSceneNode = TerrainManager.Instance.RootSceneNode;
            }
            this.parentSceneNode = parentSceneNode;
            parentSceneNode.AttachObject(this);

            box = AxisAlignedBox.Null;

            CastShadows = true;
        }

        public Forest(SceneNode parentSceneNode, XmlTextReader r)
        {

            FromXML(r);

            // create a scene node
            if (parentSceneNode == null)
            {
                parentSceneNode = TerrainManager.Instance.RootSceneNode;
            }

            this.parentSceneNode = parentSceneNode;

            parentSceneNode.AttachObject(this);
            box = AxisAlignedBox.Null;

            CastShadows = true;
        }

        public string Type
        {
            get
            {
                return "SpeedTreeForest";
            }
        }

        public void AddToBoundary(Boundary boundary)
        {
            inBoundary = true;
            this.boundary = boundary;

            ResetBounds();
        }

        public void RemoveFromBoundary()
        {
            inBoundary = false;
            boundary = null;

            ClearBounds();
        }

        private void SaveBoundaryBounds()
        {
            sizeX = boundary.Bounds.Maximum.x - boundary.Bounds.Minimum.x;
            sizeZ = boundary.Bounds.Maximum.z - boundary.Bounds.Minimum.z;

            offX = boundary.Bounds.Minimum.x;
            offZ = boundary.Bounds.Minimum.z;
        }

        /// <summary>
        /// Create a new wind object and add it to the list
        /// </summary>
        /// <param name="filename">The name of the SpeedWind .ini file to load for this wind object</param>
        public String WindFilename
        {
            get
            {
                return windFilename;
            }
            set
            {
                windFilename = value;
                speedWind = new SpeedWindWrapper();

                Stream s = ResourceManager.FindCommonResourceData(windFilename);
                FileStream fs = s as FileStream;

                String name = fs.Name;
                fs.Close();

                bool ret = speedWind.Load(name);

                // force reload of the trees so they get the new speedwind object
                BoundaryChange();
            }
        }

        public int Seed
        {
            get
            {
                return seed;
            }
            set
            {
                seed = value;

                BoundaryChange();
            }
        }

        /// <summary>
        /// Select a random location within the boundary area for a tree.  First we pick a random spot
        /// within the 2D rectangular bounds of the boundary, and then perform an intersection test of
        /// the point against the actual boundary shape.  If the selected point is not in the boundary
        /// area, then we pick another random number and try again.
        /// </summary>
        /// <returns></returns>
        private Vector3 RandomLocation()
        {
            float x;
            float z;
            Vector3 v;

            do
            {
                x = ((float)rand.NextDouble()) * sizeX + offX;
                z = ((float)rand.NextDouble()) * sizeZ + offZ;

                v = new Vector3(x, 0, z);

            } while (!boundary.PointIn(v));

            return new Vector3(x, TerrainManager.Instance.GetTerrainHeight(v, GetHeightMode.Interpolate, GetHeightLOD.MaxLOD), z);
        }

        /// <summary>
        /// Add a new type of tree to the boundary.  The number of instances indicated are randomly placed
        /// in the boundary.
        /// 
        /// NOTE - the request is queued here, and executed later.  This is due to a probable
        /// compiler or runtime bug that was causing it to fail when done here.
        /// </summary>
        /// <param name="filename">name of the speed tree description file for this tree species</param>
        /// <param name="size">size of the tree</param>
        /// <param name="sizeVariance">how much the size should vary</param>
        /// <param name="numInstances">number of instances of this tree type to make</param>
        public void AddTreeType(String filename, float size, float sizeVariance, uint numInstances)
        {
            TreeType type = new TreeType(filename, size, sizeVariance, numInstances);

            AddTreeType(type);
        }

        public void AddTreeType(TreeType type)
        {
            if (pendingTreeTypes == null)
            {
                pendingTreeTypes = new List<TreeType>();
            }
            if (treeTypes == null)
            {
                treeTypes = new List<TreeType>();
            }
            pendingTreeTypes.Add(type);
            treeTypes.Add(type);
        }

        public void UpdateTreeType()
        {
            BoundaryChange();
        }

        public void EditTreeType(int index, String filename, float size, float sizeVariance, uint numInstances)
        {
            TreeType type = new TreeType(filename, size, sizeVariance, numInstances);
            treeTypes[index] = type;

            BoundaryChange();
        }

        public void DeleteTreeType(int index)
        {
            treeTypes.RemoveAt(index);
            BoundaryChange();
        }

        public void DeleteTreeType(TreeType type)
        {
            treeTypes.Remove(type);
            BoundaryChange();
        }

        private void ProcessTreeTypes()
        {
            if (pendingTreeTypes != null)
            {
                foreach (TreeType tt in pendingTreeTypes)
                {
                    RealAddTreeType(tt.filename, tt.size, tt.sizeVariance, tt.numInstances);
                }
                pendingTreeTypes = null;
            }
        }

        /// <summary>
        /// (possibly) increase the bounding box of the forest to include the area of the bounding
        /// box.
        /// </summary>
        /// <param name="treeBox">The AxisAlignedBox that describes the tree or tree group's bounding box</param>
        private void AddBounds(AxisAlignedBox treeBox)
        {
            if (bounds == null)
            {
                bounds = new AxisAlignedBox();
            }

            bounds.Merge(treeBox);

            box = bounds;
        }

        public void RealAddTreeType(String filename, float size, float sizeVariance, uint numInstances)
        {
            List<Vector3> locations = new List<Vector3>();

            // create random locations for trees
            for (int i = 0; i < numInstances; i++)
            {
                locations.Add(RandomLocation());
            }
            TreeGroup group = new TreeGroup(filename, size, sizeVariance, speedWind, this, locations);
            AddBounds(group.Bounds);

            group.WindStrength = WindStrength;

            groups.Add(group);
        }

        public void PerFrameProcessing(float time, Camera camera)
        {
            Debug.Assert(inBoundary);
            bool updateVisibility = false;

            if (pendingTreeTypes != null)
            {
                ProcessTreeTypes();
                TerrainManager.Instance.RecreateCollisionTiles();
                updateVisibility = true;
            }
            SpeedTreeWrapper.Time = time;

            if (bounds != null && camera.IsObjectVisible(bounds))
            { // this stuff only needs to be done if the forest is visible
                // process wind
                speedWind.Advance(time, windStrength, SpeedTreeUtil.ToSpeedTree(windDirection));

                // determine whether the camera changed direction or location since the last frame
                if (camera.Direction != lastCameraDirection || camera.Position != lastCameraLocation)
                {
                    updateVisibility = true;
                    lastCameraLocation = camera.Position;
                    lastCameraDirection = camera.Direction;
                }

                // if the camera changed position or direction, or new trees were added, then recompute the visibility of this tree
                if (updateVisibility)
                {
                    foreach (TreeGroup group in groups)
                    {
                        group.CameraChange(camera);
                    }
                }
                foreach (TreeGroup group in groups)
                {
                    group.UpdateMaterials();
                }
            }
        }

        public void PageShift()
        {
        }

        public void FindObstaclesInBox(AxisAlignedBox box,
                                       CollisionTileManager.AddTreeObstaclesCallback callback)
        {
            foreach (TreeGroup group in groups)
            {
                group.FindObstaclesInBox(box, callback);
            }
        }

        public void ToXML(XmlTextWriter w)
        {
            w.WriteStartElement("boundarySemantic");
            w.WriteAttributeString("type", "SpeedTreeForest");
            w.WriteElementString("seed", seed.ToString());
            w.WriteElementString("name", name);
            w.WriteElementString("windFilename", windFilename);
            w.WriteElementString("windStrength", windStrength.ToString());
            w.WriteStartElement("windDirection");
            w.WriteAttributeString("x", windDirection.x.ToString());
            w.WriteAttributeString("y", windDirection.y.ToString());
            w.WriteAttributeString("z", windDirection.z.ToString());
            w.WriteEndElement();

            foreach (TreeType tt in treeTypes)
            {
                w.WriteStartElement("treeType");
                w.WriteAttributeString("filename", tt.filename);
                w.WriteAttributeString("size", tt.size.ToString());
                w.WriteAttributeString("sizeVariance", tt.sizeVariance.ToString());
                w.WriteAttributeString("numInstances", tt.numInstances.ToString());
                w.WriteEndElement();
            }

            w.WriteEndElement();
        }

        protected void ParseElement(XmlTextReader r)
        {
            bool readEnd = true;

            // set the field in this object based on the element we just read
            switch (r.Name)
            {
                case "seed":
                    // read the value
                    r.Read();
                    if (r.NodeType != XmlNodeType.Text)
                    {
                        return;
                    }

                    seed = int.Parse(r.Value);
                    break;

                case "name":
                    // read the value
                    r.Read();
                    if (r.NodeType != XmlNodeType.Text)
                    {
                        return;
                    }

                    name = string.Format("{0} - unique - {1}", r.Value, uniqueNum.ToString());
                    uniqueNum++;

                    break;
                case "windFilename":
                    // read the value
                    r.Read();
                    if (r.NodeType != XmlNodeType.Text)
                    {
                        return;
                    }
                    WindFilename = r.Value;

                    break;
                case "windStrength":
                    // read the value
                    r.Read();
                    if (r.NodeType != XmlNodeType.Text)
                    {
                        return;
                    }
                    WindStrength = float.Parse(r.Value);

                    break;
                case "windDirection":
                    float x = 0, y = 0, z = 0;
                    for (int i = 0; i < r.AttributeCount; i++)
                    {

                        r.MoveToAttribute(i);

                        // set the field in this object based on the element we just read
                        switch (r.Name)
                        {
                            case "x":
                                x = float.Parse(r.Value);
                                break;
                            case "y":
                                y = float.Parse(r.Value);
                                break;
                            case "z":
                                z = float.Parse(r.Value);
                                break;
                        }
                    }
                    r.MoveToElement(); //Moves the reader back to the element node.

                    WindDirection = new Vector3(x, y, z);

                    readEnd = false;

                    break;
                case "treeType":
                    String filename = null;
                    float size = 0;
                    float sizeVariance = 0;
                    uint numInstances = 0;

                    for (int i = 0; i < r.AttributeCount; i++)
                    {

                        r.MoveToAttribute(i);

                        // set the field in this object based on the element we just read
                        switch (r.Name)
                        {
                            case "filename":
                                filename = r.Value;
                                break;
                            case "size":
                                size = float.Parse(r.Value);
                                break;
                            case "sizeVariance":
                                sizeVariance = float.Parse(r.Value);
                                break;
                            case "numInstances":
                                numInstances = uint.Parse(r.Value);
                                break;
                        }
                    }
                    r.MoveToElement(); //Moves the reader back to the element node.

                    AddTreeType(filename, size, sizeVariance, numInstances);
                    readEnd = false;
                    break;

            }

            if (readEnd)
            {
                // error out if we dont see an end element here
                r.Read();
                if (r.NodeType != XmlNodeType.EndElement)
                {
                    return;
                }
            }
        }

        private void FromXML(XmlTextReader r)
        {
            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    ParseElement(r);
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    // if we found an end element, it means we are at the end of the terrain description
                    return;
                }
            }
        }

        public float WindStrength
        {
            get
            {
                return windStrength;
            }
            set
            {
                windStrength = value;

                foreach (TreeGroup group in groups)
                {
                    group.WindStrength = windStrength;
                }
            }
        }

        public Vector3 WindDirection
        {
            get
            {
                return windDirection;
            }
            set
            {
                windDirection = value;
            }
        }

        private TimingMeter renderAllBranchesMeter = MeterManager.GetMeter("Render All Branches", "Forest");
        private TimingMeter renderAllFrondsMeter = MeterManager.GetMeter("Render All Fronds", "Forest");
        private TimingMeter renderAllLeavesMeter = MeterManager.GetMeter("Render All Leaves", "Forest");
        private TimingMeter renderAllBillboardsMeter = MeterManager.GetMeter("Render All Billboards", "Forest");

        /// <summary>
        /// return the forest to its initial state when no boundary is bound to it
        /// </summary>
        private void ClearBounds()
        {
            foreach (TreeGroup group in groups)
            {
                group.Dispose();
            }
            groups.Clear();

            rand = null;
            bounds = null;
            box = AxisAlignedBox.Null;
        }

        /// <summary>
        /// Reset the state of the forest when a boundary is added or changed
        /// </summary>
        private void ResetBounds()
        {
            // create a new Random object so that the generator is reset from the seed
            rand = new Random(seed);

            // compute values based on boundary bounds
            SaveBoundaryBounds();

            // reset the bounds
            bounds = null;
            box = AxisAlignedBox.Null;
        }

        public void BoundaryChange()
        {
            if (inBoundary)
            {

                ClearBounds();
                ResetBounds();

                // add back all the trees
                if (treeTypes != null && treeTypes.Count > 0)
                {
                    foreach (TreeType tt in treeTypes)
                    {
                        RealAddTreeType(tt.filename, tt.size, tt.sizeVariance, tt.numInstances);
                    }
                }

                // mess with these values, so that next time through per frame processing we will think
                // the camera changed
                lastCameraDirection = new Vector3();
                lastCameraLocation = new Vector3();
            }
        }

        public void Dispose()
        {
            parentSceneNode.DetachObject(this);
            ClearBounds();
        }

        public override void GetRenderOperation(RenderOperation op)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void UpdateRenderQueue(RenderQueue queue)
        {
            return;
        }

        public override float GetSquaredViewDepth(Camera camera)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override float BoundingRadius
        {
            get
            {
                return 0f;
            }
        }
    }

    public class TreeType
    {
        public String filename;
        public float size;
        public float sizeVariance;
        public uint numInstances;

        public TreeType(String filename, float size, float sizeVariance, uint numInstances)
        {
            this.filename = filename;
            this.size = size;
            this.sizeVariance = sizeVariance;
            this.numInstances = numInstances;
        }
    }
}
