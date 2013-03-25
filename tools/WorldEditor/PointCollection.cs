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
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Axiom.MathLib;
using Axiom.Core;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    public delegate void PointsChangedEventHandler(object sender, EventArgs args);

    public class PointCollection : IWorldObject, IWorldContainer, IObjectDrag
    {
        protected List<MPPoint> points;
        protected bool inTree = false;
        protected bool inScene = false;
        protected WorldTreeNode node;
        protected WorldTreeNode parentNode;
        protected IWorldContainer parent;
        protected WorldEditor app;
        protected bool noIntersect;
        protected bool displayMarkers;
        protected string markerMeshName;
        protected string markerMaterialName;
        protected MPPointType type;
        protected List<ToolStripButton> buttonBar;
        protected bool reFocus = false;

        public PointCollection(IWorldContainer parent, WorldEditor worldEditor, bool noIntersect, bool displayMarkers, string markerMeshName, string markerMaterialName, MPPointType type)
        {
            this.parent = parent;
            this.app = worldEditor;
            this.noIntersect = noIntersect;
            this.displayMarkers = true;
            this.markerMeshName = markerMeshName;
            this.markerMaterialName = markerMaterialName;
            points = new List<MPPoint>();
            this.type = type;
        }


        public PointCollection(IWorldContainer parent, WorldEditor worldEditor, bool noIntersect, bool displayMarkers, string markerMeshName, string markerMaterialName, MPPointType type, XmlReader r)
            :
            this(parent, worldEditor, noIntersect, displayMarkers, markerMeshName, markerMaterialName, type)
        {
            //
            // don't do the intersection test when adding points from xml
            //
            this.noIntersect = false;

            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if (r.NodeType == XmlNodeType.Element)
                {
                    if (String.Equals(r.Name, "Point"))
                    {
                        Vector3 ptLoc = XmlHelperClass.ParseVectorAttributes(r);
                        int pointNum;
                        AddPoint(ptLoc, out pointNum);
                    }
                }

            }

            this.noIntersect = noIntersect;
        }

        protected void ReNumberPoints()
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].PointNum = i;
            }
        }

        public event PointsChangedEventHandler PointsChanged;

        public void OnPointsChanged()
        {
            if (!reFocus)
            {
                ReNumberPoints();
                PointsChangedEventHandler e = PointsChanged;
                if (e != null)
                {
                    e(this, new EventArgs());
                }
            }
        }

        [BrowsableAttribute(false)]
        public List<Vector3> VectorList
        {
            get
            {
                List<Vector3> list = new List<Vector3>();
                foreach (MPPoint pt in points)
                {
                    list.Add(pt.Position);
                }
                return list;
            }
        }

        private Vector2 MinMaxX
        {
            get
            {
                float minX = 0f;
                float maxX = 0f;
                int i = 0;
                foreach (Vector3 vec in VectorList)
                {
                    if (i == 0 || vec.x > maxX )
                    {
                        maxX = vec.x;
                    }
                    if (i == 0 || vec.x < minX )
                    {
                        minX = vec.x;
                    }
                    i++;
                }
                return new Vector2(minX, maxX);
            }
        }

        private Vector2 MinMaxZ
        {
            get
            {
                float minZ = 0f;
                float maxZ = 0f;
                int i = 0;
                foreach (Vector3 vec in VectorList)
                {
                    if (i == 0 || vec.z > maxZ)
                    {
                        maxZ = vec.z;
                    }
                    if (i == 0 || vec.z < minZ)
                    {
                        minZ = vec.z;
                    }
                    i++;
                }
                return new Vector2(minZ, maxZ);
            }
        }

        [BrowsableAttribute(false)]
        public float Radius
        {
            get
            {
                float a = (MinMaxZ.y - MinMaxZ.x) / 2;
                float b = (MinMaxX.y - MinMaxX.x) / 2;
                float c = (float)(Math.Sqrt((Math.Pow((double)a, 2.0) + Math.Pow((double)b, 2.0))));
                return c;
            }
        }

        [BrowsableAttribute(false)]
        public bool AllowAdjustHeightOffTerrain
        {
            get
            {
                return false;
            }
        }


        [BrowsableAttribute(false)]
        public IWorldContainer Parent
        {
            get
            {
                return (IWorldContainer) this.parent;
            }
            set
            {
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 Position
        {
            get
            {
                return FocusLocation;
            }
            set
            {
                SetFocus(value.x, value.z);
            }
        }

        [BrowsableAttribute(false)]
        public bool AllowYChange
        {
            get
            {
                return false;
            }
        }

        [BrowsableAttribute(false)]
        public float TerrainOffset
        {
            get
            {
                return 0f;
            }
            set
            {
            }
        }

        [BrowsableAttribute(false)]
        public DisplayObject Display
        {
            get
            {
                return null;
            }
        }

        [BrowsableAttribute(false)]
        public bool InScene
        {
            get
            {
                return inScene;
            }
        }


        [BrowsableAttribute(false)]
        public List<IWorldObject> Children
        {
            get
            {
                List<IWorldObject> children = new List<IWorldObject>();
                foreach (IWorldObject point in points)
                {
                    children.Add(point);
                }
                return children;
            }
        }   

        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            if (!inTree)
            {
                // save parentNode for use later when removing from the tree
                this.parentNode = parentNode;

                

                // create a node for the collection and add it to the parent
                node = app.MakeTreeNode(this, "Points");
                CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
                parentNode.Nodes.Add(node); menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
                menuBuilder.Add("Help", "Plant_Type", app.HelpClickHandler);
                node.ContextMenuStrip = menuBuilder.Menu;
                buttonBar = menuBuilder.ButtonBar;// mark this object as being in the tree
                inTree = true;
            }
            // add any children to the tree
            foreach (MPPoint pt in points)
            {
                pt.AddToTree(node);
            }
        }

        [BrowsableAttribute(false)]
        public bool IsGlobal
        {
            get
            {
                return false;
            }
        }

        [BrowsableAttribute(false)]
        public bool IsTopLevel
        {
            get
            {
                return false;
            }
        }

        [BrowsableAttribute(false)]
        public bool WorldViewSelectable
        {
            get
            {
                return false;
            }
            set
            {
                // This property is not relevent for this object.
            }
        }

        public void Clone(IWorldContainer copyParent)
        {
            PointCollection clone = new PointCollection(parent, app, noIntersect, false, markerMeshName, markerMaterialName, type);
            foreach (IWorldObject child in Children)
            {
                child.Clone(clone);
            }
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString +=  "\r\n";
                return objString;
            }
        }

        [BrowsableAttribute(false)]
        public List<ToolStripButton> ButtonBar
        {
            get
            {
                return buttonBar;
            }
        }
        

        public void RemoveFromTree()
        {
            if (inTree)
            {
                if (node.IsSelected)
                {
                    node.UnSelect();
                }


                // remove this object from the tree
                parentNode.Nodes.Remove(node);

                parentNode = null;
                node = null;
                inTree = false;
            }
            // remove all children from the tree
            foreach (MPPoint pt in points)
            {
                pt.RemoveFromTree();
            }
        }

        public void AddToScene()
        {
            if (!inScene)
            {
                inScene = true;
            }
            if (DisplayMarkers)
            {
                AddMarkersToScene();
            }
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            foreach (IWorldObject obj in Children)
            {
                obj.UpdateScene(type, hint);
            }
        }

        protected void AddMarkersToScene()
        {
            foreach (MPPoint pt in points)
            {
                if (!pt.InScene)
                {
                    pt.AddToScene();
                }
            }
        }

        public void RemoveFromScene()
        {
            
            RemoveMarkersFromScene();
            if (inScene)
            {
                inScene = false;
            }
        }

        public void CheckAssets()
        {
            foreach (MPPoint pt in points)
            {
                pt.CheckAssets();
            }
        }

        protected void RemoveMarkersFromScene()
        {
            foreach (MPPoint pt in points)
            {
                pt.RemoveFromScene();
            }
        }

        public void ToXml(XmlWriter w)
        {
			w.WriteStartElement("PointCollection");
			foreach(MPPoint point in points)
			{
				point.ToXml(w);
			}
			w.WriteEndElement();
        }

		[BrowsableAttribute(false)]
        public Vector3 FocusLocation
        {
            get
            {
                Vector3 v = Vector3.Zero;

                if (points.Count != 0)
                {
                    foreach (MPPoint p in points)
                    {
                        v += p.Position;
                    }

                    v = v / points.Count;
                }

                return v;
            }
        }


        public void SetFocus(float x, float z)
        {
            reFocus = true;
            Vector2 focus = new Vector2(x, z);
            foreach (MPPoint point in points)
            {
                point.SetFocus(focus);
            }

            reFocus = false;
            OnPointsChanged();
        }

        [BrowsableAttribute(false)]
        public bool ReFocus
        {
            get
            {
                return reFocus;
            }
        }

        public void UpdateFocus(Vector3 focusPos)
        {
            Vector2 focusLoc = new Vector2(focusPos.x, focusPos.z);
            foreach (MPPoint point in points)
            {
                point.UpdateFocus(focusLoc);
            }
        }

		[BrowsableAttribute(false)]
        public bool Highlight
        {
            get
            {
                return false;
            }
            set
            {
                // do nothing for now
            }
        }

		[BrowsableAttribute(false)]
		public WorldTreeNode Node
		{
			get
			{
				return node;
			}
		}

        public void ToManifest(System.IO.StreamWriter w)
        {
            if (Children != null)
            {
                foreach (IWorldObject child in Children)
                {
                    child.ToManifest(w);
                }
            }
        }

        public int IndexOf(MPPoint point)
        {
            return points.IndexOf(point);
        }

        #endregion

        [BrowsableAttribute(false)]
        public bool DisplayMarkers
        {
            get
            {
                if ((app.DisplayBoundaryMarkers && this.type == MPPointType.Boundary) || (app.DisplayRoadMarkers && this.type == MPPointType.Road))
                {
                    return true;
                }
                return false;
            }
            set
            {
                if (value != displayMarkers)
                {
                    displayMarkers = value;
                    if (inScene)
                    {
                        if (displayMarkers)
                        {
                            AddMarkersToScene();
                        }
                        else
                        {
                            RemoveMarkersFromScene();
                        }
                    }
                }
            }
        }

		[BrowsableAttribute(false)]
		public bool NoIntersect
		{
			get
			{
				return noIntersect;
			}

		}

        public bool AddPoint(Vector3 location, out int index)
		{
			bool ret = true;
            index = 0;

			if (noIntersect)
		    {
				if (IntersectionHelperClass.BoundaryIntersectionSearch(this.VectorList, location, points.Count))
				{
                    ErrorHelper.SendUserError("Adding point to region failed", "Region", app.Config.ErrorDisplayTimeDefault, true, this, app);
					return false;
				}
			}
            MPPoint pt = new MPPoint(Count, this, app, markerMeshName, markerMaterialName, location, this.type);

            Add(pt);
			index = pt.PointNum;
            this.UpdateFocus(FocusLocation);
            return ret;
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (MPPoint point in points)
            {
                point.Dispose();
            }
            points.Clear();
        }

        #endregion


        public bool Insert(int index, MPPoint item)
        {
            int i = index + 1;

            foreach (MPPoint pt in points)
            {
                if (pt.PointNum >= i)
                {
                    pt.PointNum = pt.PointNum + 1;
                }
                if (inTree)
                {
                    pt.RemoveFromTree();
                }
            }
            points.Insert(i, item);
            if (inScene)
            {
                item.AddToScene();
            }
            foreach (MPPoint pt in points)
            {
                if (inTree)
                {
                    pt.AddToTree(node);
                }
            }
            OnPointsChanged();
            this.UpdateFocus(FocusLocation);
            return true;
        }


        public void Clear()
        {
            points.Clear();

            OnPointsChanged();
        }

        [BrowsableAttribute(false)]
        public int Count
        {
            get
            {
                return points.Count;
            }
        }

		[BrowsableAttribute(false)]
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
		public string ObjectType
		{
			get
			{
				return "Points";
			}
		}

        [BrowsableAttribute(false)]
        public bool AcceptObjectPlacement
        {
            get
            {
                return false;
            }
            set
            {
                //not implemented for this type of object
            }
        }


        public bool Remove(int pointNum)
        {
            MPPoint item = points[pointNum];
            bool rv = Remove(item);
            UpdateFocus(FocusLocation);
            return rv;
        }

        #region IEnumerable<MPPoint> Members

        public IEnumerator<MPPoint> GetEnumerator()
        {
            return points.GetEnumerator();
        }

        #endregion // IEnumerable<MPPoint> Members

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion  //IEnumerable Members

        #region ICollection<IWorldObject> Members


        public void Add(IWorldObject item)
        {
            points.Add((MPPoint) item);

            OnPointsChanged();

            if (inTree)
            {
                item.AddToTree(node);
            }
            if (inScene)
            {
                item.AddToScene();
            }
        }

        public bool Contains(IWorldObject item)
        {
            return points.Contains((MPPoint)(item));
        }

        public void CopyTo(IWorldObject[] array, int arrayIndex)
		{
			Children.CopyTo(array, arrayIndex);
		}

        public bool Remove(IWorldObject item)
        {
            bool ret;

            ret = points.Remove((MPPoint)(item));
            int index = ((MPPoint)item).PointNum;
            foreach (MPPoint pt in points)
            {
                if (pt.PointNum >= index)
                {
                    pt.PointNum--;
                }
            }
            OnPointsChanged();

            if (inTree)
            {
                item.RemoveFromTree();
            }
            if (inScene)
            {
                item.RemoveFromScene();
            }

            return ret;
        }

        #endregion //ICollection<IWorldObject> Members

        #region IEnumerable<IWorldObject> Members

        IEnumerator<IWorldObject> IEnumerable<IWorldObject>.GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        #endregion
    }
}
