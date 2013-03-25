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
using System.Text;
using System.ComponentModel;
using System.Xml;
using Axiom.SceneManagers.Multiverse;
using Axiom.MathLib;
using Multiverse.ToolBox;


namespace Multiverse.Tools.WorldEditor
{
    public class RoadObject : IWorldObject, IObjectInsert, IWorldContainer, IObjectChangeCollection, IObjectDelete, IObjectCutCopy, IObjectDrag, IObjectCameraLockable
    {
        protected String name;
        protected IWorldContainer parent;
        protected List<IWorldObject> children;
        protected WorldEditor app;
        protected WorldTreeNode node = null;
        protected WorldTreeNode parentNode = null;

        protected int halfWidth;
        protected Road road;

        protected bool highlight = false;
        protected bool inTree = false;
		protected bool inScene = false;
		public NameValueObject nameValuePairs;

        protected PointCollection points;

        protected List<ToolStripButton> buttonBar;
        
        public RoadObject(String objectName, IWorldContainer parentContainer, WorldEditor worldEditor, int halfWidth)
        {
            name = objectName;
            parent = parentContainer;
            app = worldEditor;
            children = new List<IWorldObject>();

            this.HalfWidth = halfWidth;
			this.nameValuePairs = new NameValueObject();

            points = new PointCollection(this, app, false, true, app.Config.RoadPointMeshName, app.Config.RoadPointMaterial, MPPointType.Road);
            Add(points);
            points.PointsChanged += new PointsChangedEventHandler(PointsChangedHandler);
        }

		public RoadObject(XmlReader r, IWorldContainer parentContainer, WorldEditor worldEditor)
		{
            parent = parentContainer;
            app = worldEditor;
            children = new List<IWorldObject>();

            FromXml(r);
        }

        public RoadObject(WorldEditor worldEditor, String objectName, IWorldContainer parentContainer, int halfWidth)
        {
            name = objectName;
            parent = parentContainer;
            app = worldEditor;
            children = new List<IWorldObject>();

            this.HalfWidth = halfWidth;
            this.nameValuePairs = new NameValueObject();

            points = null;
        }

        protected void FromXml(XmlReader r)
        {
            // first parse the attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Name":
                        this.name = r.Value;
                        break;
                    case "HalfWidth":
                        this.HalfWidth = int.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.

			while (r.Read())
			{
                if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }

				if (r.NodeType == XmlNodeType.Element)
				{
					switch (r.Name)
					{
						case "PointCollection":
                            if (!r.IsEmptyElement)
                            {
                                this.points = new PointCollection(this, app, false, true, this.app.Config.RoadPointMeshName, this.app.Config.RoadPointMaterial, MPPointType.Road, r);
                                points.PointsChanged += new PointsChangedEventHandler(PointsChangedHandler);
                                Add(points);
                            }
							break;
						case "NameValuePairs":
							this.nameValuePairs = new NameValueObject(r);
                            break;
					}
				}

			}
			if (this.nameValuePairs == null)
			{
				this.nameValuePairs = new NameValueObject();
			}
            if (this.points == null)
            {
                this.points = new PointCollection(this, app, false, true, this.app.Config.RoadPointMeshName, this.app.Config.RoadPointMaterial, MPPointType.Road);
                Add(points);
            }
		}

        protected void PointsChangedHandler(object sender, EventArgs args)
        {
            RefreshPoints();
        }

        protected void RefreshPoints()
        {
            if (inScene)
            {

                List<Vector3> tmpPts = new List<Vector3>(points.Count);
                foreach (MPPoint pt in points)
                {
                    tmpPts.Add(pt.Position);
                }
              
                road.SetPoints(tmpPts);
            }
        }

        [BrowsableAttribute(false)]
        public PointCollection Points
        {
            get
            {
                return points;
            }
        }

        [BrowsableAttribute(true), DescriptionAttribute("Half of the width of the road (meters)."), CategoryAttribute("Miscellaneous")]
        public int HalfWidth
        {
            get
            {
                return halfWidth;
            }
            set
            {
                halfWidth = value;
                if (road != null)
                {
                    road.HalfWidth = halfWidth;
                }
            }
        }

        [DescriptionAttribute("The name of this road."), BrowsableAttribute(true), CategoryAttribute("Miscellaneous")]
        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                UpdateNode();
            }
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
		public string ObjectType
		{
			get
			{
				return "Road";
			}
		}

        protected void UpdateNode()
        {
            if (inTree)
            {
                node.Text = NodeName;
            }
        }

        protected string NodeName
        {
            get
            {
                string ret;
                if (app.Config.ShowTypeLabelsInTreeView)
                {
                    ret = string.Format("{0}: {1}", ObjectType, name);
                }
                else
                {
                    ret = name;
                }

                return ret;
            }
        }



        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            if (parentNode != null)
            {
                this.parentNode = parentNode;

                inTree = true;

                // create a node for the collection and add it to the parent
                node = app.MakeTreeNode(this, NodeName);
                parentNode.Nodes.Add(node);

                foreach (IWorldObject obj in children)
                {
                    obj.AddToTree(node);
                }

                // build the menu
                CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
                menuBuilder.Add("Drag Road", new DragObjectsFromMenuCommandFactory(app), app.DefaultCommandClickHandler);
                menuBuilder.AddDropDown("Move to Collection", menuBuilder.ObjectCollectionDropDown_Opening);
                menuBuilder.FinishDropDown();
                menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
                menuBuilder.Add("Help", "Road", app.HelpClickHandler);
                menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                node.ContextMenuStrip = menuBuilder.Menu;
                buttonBar = menuBuilder.ButtonBar;
            }
            else
            {
                inTree = false;
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
                return true;
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

        [BrowsableAttribute(false)]
        public bool WorldViewSelectable
        {
            get
            {
                return false;
            }
            set
            {
                // this property is not applicable to this object
            }
        }

        public void Clone(IWorldContainer copyParent)
        {
            RoadObject clone = new RoadObject(app, name, copyParent, halfWidth);
            clone.NameValue = new NameValueObject(nameValuePairs);
            foreach (IWorldObject child in children)
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
                string objString = String.Format("Name:{0}\r\n", NodeName);
                objString += String.Format("\tHalfWidth={0}\r\n", HalfWidth);
                objString += String.Format("\tCenter=({0},{1},{2})\r\n", FocusLocation.x, FocusLocation.y, FocusLocation.z);
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
            if (inTree && node != null && parentNode != null)
            {
                if (node.IsSelected)
                {
                    node.UnSelect();
                }
                points.RemoveFromTree();

                parentNode.Nodes.Remove(node);
            }
            parentNode = null;
            node = null;
            inTree = false;
        }

        public void AddToScene()
        {
            if (!inScene)
            {
                inScene = true;

                // set up the road in the scene manager
                road = Axiom.SceneManagers.Multiverse.TerrainManager.Instance.CreateRoad(name);
                road.HalfWidth = halfWidth;
                RefreshPoints();

                // display the markers
                foreach (IWorldObject child in children)
                {
                    child.AddToScene();
                }
            }
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            foreach (IWorldObject child in children)
            {
                child.UpdateScene(type, hint);
            }
        }

        [BrowsableAttribute(false)]
        public IWorldContainer Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        public void RemoveFromScene()
        {
            if (inScene)
            {
                inScene = false;

                points.RemoveFromScene();

                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.RemoveRoad(road);

                road.Dispose();
                road = null;
            }
        }

        public void CheckAssets()
        {
            if (!app.CheckAssetFileExists(app.Config.RoadPointMeshName))
            {
                app.AddMissingAsset(app.Config.RoadPointMeshName);
            }
            string textureFile = "directional_marker_yellow.dds";
            if (!app.CheckAssetFileExists(textureFile))
            {
                app.AddMissingAsset(textureFile);
            }
            string materialFile = "directional_marker.material";
            if (!app.CheckAssetFileExists(materialFile))
            {
                app.AddMissingAsset(materialFile);
            }
            foreach (IWorldObject child in children)
            {
                child.CheckAssets();
            }
        }

        public void ToXml(XmlWriter w)
        {
			w.WriteStartElement("Road");
			w.WriteAttributeString("Name", name);
			w.WriteAttributeString("HalfWidth", this.halfWidth.ToString());
            if (nameValuePairs != null)
            {
                nameValuePairs.ToXml(w);
            }
            foreach(IWorldObject child in children)
            {
                child.ToXml(w);
            }
			w.WriteEndElement(); // Road end
		}

        [BrowsableAttribute(false)]
        public Vector3 FocusLocation
        {
            get
            {
                return points.FocusLocation;
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 Position
        {
            get
            {
                return points.FocusLocation;
            }
            set
            {
                points.SetFocus(value.x, value.z);
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
        public bool Highlight
        {
            get
            {
                return false;
            }
            set
            {
                // does nothing for now
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
            if (children != null)
            {
                foreach (IWorldObject child in children)
                {
                    child.ToManifest(w);
                }
            }
        }

        #endregion

		[EditorAttribute(typeof(NameValueUITypeEditorRoad), typeof(System.Drawing.Design.UITypeEditor)), CategoryAttribute("Miscellaneous"),
DescriptionAttribute("Arbitrary Name/Value pair used to pass information about an object to server scripts and plug-ins. Click [...] to add or edit name/value pairs.")]
		public NameValueObject NameValue
		{
			get
			{
				return nameValuePairs;
			}
			set
			{
				nameValuePairs = value;
			}
		}

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region ICollection<IWorldObject> Members

        public void Add(IWorldObject item)
        {
            children.Add(item);
            if (String.Equals(item.ObjectType, "Points") && points == null)
            {
                points = (item as PointCollection);
            }
            if (inScene)
            {
                item.AddToScene();
            }
            if (inTree)
            {
                item.AddToTree(node);
            }
        }

        public void Clear()
        {
            children.Clear();
        }

        public bool Contains(IWorldObject item)
        {
            return this.children.Contains(item);
        }

        public void CopyTo(IWorldObject[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
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
        public int Count
        {
            get
            {
                return this.children.Count;
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

        public bool Remove(IWorldObject item)
        {
            points.Remove((PointCollection)item);
            children.Remove(item);
            return true;
        }

        #endregion

        #region IEnumerable<IWorldObject> Members

        public IEnumerator<IWorldObject> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        [BrowsableAttribute(false)]
        public bool AllowAdjustHeightOffTerrain
        {
            get
            {
                return false;
            }
        }

        [BrowsableAttribute(false)]
        public float Radius
        {
            get
            {
                return points.Radius;
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 Center
        {
            get
            {
                return FocusLocation;
            }
        }

    }
}
