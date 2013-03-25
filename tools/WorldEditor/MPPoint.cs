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
using System.Xml;
using System.ComponentModel;
using Axiom.MathLib;

namespace Multiverse.Tools.WorldEditor
{
    public enum MPPointType { Boundary, Road };

    public class MPPoint : IWorldObject, IObjectPosition, IObjectDrag, IObjectDelete, IObjectCameraLockable
    {
        protected DisplayObject displayObject = null;
        protected PointCollection parent;
        protected WorldEditor app;
        protected WorldTreeNode node = null;
        protected WorldTreeNode parentNode = null;

        protected String meshName;
        protected String meshMaterial;
        protected bool highlight = false;
        protected bool inTree = false;
        protected bool inScene = false;
        protected int pointNum;
        protected Vector3 position;
        protected List<ToolStripButton> buttonBar;
        protected MPPointType type;
        protected float terrainOffset = 0;
        protected Vector2 focusOffset;
        protected bool worldViewSelectable = true;

        public MPPoint(int pointNum, PointCollection parent, WorldEditor worldEditor, string meshName, string meshMaterial, Vector3 position, MPPointType type)
        {
            this.PointNum = pointNum;
            this.parent = parent;
            this.app = worldEditor;
            this.meshName = meshName;
            this.meshMaterial = meshMaterial;
            this.position = position;
            this.type = type;
        }

		public MPPoint(XmlTextReader r, int pointNum, PointCollection parent, WorldEditor worldEditor, string meshName, string meshMaterial, MPPointType type)
		{
            this.PointNum = pointNum;
            this.parent = parent;
            this.app = worldEditor;
            this.meshName = meshName;
            this.meshMaterial = meshMaterial;
            this.type = type;

            position = parseVectorAttributes(r);
		}

        public MPPoint(int pointNum, PointCollection parent, WorldEditor worldEditor, string meshName, string meshMaterial, Vector3 position, MPPointType type, Vector2 focusOffset)
        {
            this.PointNum = pointNum;
            this.parent = parent;
            this.app = worldEditor;
            this.meshName = meshName;
            this.meshMaterial = meshMaterial;
            this.position = position;
            this.type = type;
            this.focusOffset = focusOffset;
        }

		protected Vector3 parseVectorAttributes(XmlTextReader r)
		{
			float x = 0;
			float y = 0;
			float z = 0;

			for (int i = 0; i < r.AttributeCount; i++)
			{
				r.MoveToAttribute(i);

				// set the field in this object based on the element we just read
				switch (r.Name)
				{
                    case "WorldViewSelect":
                        worldViewSelectable = bool.Parse(r.Value);
                        break;
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

			return new Vector3(x, y, z);
		}

        public string Name
        {
            get
            {
                return String.Format("Point{0}", pointNum);
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
        public IWorldContainer Parent
        {
            get
            {
                return parent;
            }
        }

        [BrowsableAttribute(false)]
        public int PointNum
        {
            get
            {
                return pointNum;
            }
            set
            {
                if (pointNum != value)
                {
                    pointNum = value;
                    OnPointNumChanged();
                }
            }
        }

        [BrowsableAttribute(false)]
        public MPPointType Type
        {
            get
            {
                return type;
            }
        }

        public void UpdateFocus(Vector2 focusLoc)
        {
            focusOffset.x = position.x - focusLoc.x;
            focusOffset.y = position.z - focusLoc.y;
        }

        public void SetFocus(Vector2 focusLoc)
        {
            position.x = focusLoc.x + focusOffset.x;
            position.z = focusLoc.y + focusOffset.y;
            position.y = app.GetTerrainHeight(position.x, position.z);
            if (inScene)
            {
                this.displayObject.Position = position;
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
                return 0;
            }
            set
            {
                terrainOffset = 0;
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                float y = app.GetTerrainHeight(value.x, value.z);
                position = new Vector3(value.x, y, value.z);

                if (inScene)
                {
                    displayObject.Position = position;
                }
                if (!parent.ReFocus)
                {
                    parent.OnPointsChanged();
                }
            }
        }

        protected void OnPointNumChanged()
        {
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
        public float Radius
        {
            get
            {
                return displayObject.Radius;
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 Center
        {
            get
            {
                if (inScene && displayObject != null)
                {
                    return displayObject.Center;
                }
                else
                {
                    return Vector3.Zero;
                }
            }

        }

        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;
            if (!inTree)
            {
                inTree = true;

                // create a node for the collection and add it to the parent
                node = app.MakeTreeNode(this, Name);
                parentNode.Nodes.Add(this.Node);

                // build the menu
                CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
                menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
                menuBuilder.Add("Drag Point", new DragMPPointCommandFactory(app, this), app.DefaultCommandClickHandler);
                menuBuilder.Add("Insert new points", new InsertPointsCommandFactory(app, (IWorldContainer)parent, this.PointNum), app.DefaultCommandClickHandler);
                menuBuilder.Add("Delete", new DeletePointCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                node.ContextMenuStrip = menuBuilder.Menu;
                buttonBar = menuBuilder.ButtonBar;
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
        public List<ToolStripButton> ButtonBar
        {
            get
            {
                return buttonBar;
            }
        }

        [CategoryAttribute("Miscellaneous"), DescriptionAttribute("Sets if the point may be selected in the world view")]
        public bool WorldViewSelectable
        {
            get
            {
                return true;
            }
            set
            {
                worldViewSelectable = value;
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


        public void Clone(IWorldContainer copyParent)
        {
            MPPoint clone = new MPPoint(pointNum, copyParent as PointCollection, app, meshName, meshMaterial, position, type, focusOffset);
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                
                string objString = String.Format("Name:{0}:{1}\r\n", ObjectType, Name);
                objString += String.Format("\tAllowAdjustHeightOffTerrain={0}\r\n", AllowAdjustHeightOffTerrain);
                objString += String.Format("\tWorldViewSelectable={0}\r\n", worldViewSelectable);
                objString += String.Format("\tPosition=({0},{1},{2})\r\n", position.x, position.y, position.z);
                objString += "\r\n";
                return objString;
            }
        }

        [BrowsableAttribute(false)]
        public Vector2 FocusOffset
        {
            get
            {
                return focusOffset;
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
                if (node != null)
                {
                    parentNode.Nodes.Remove(node);
                }
                parentNode = null;
                node = null;
                inTree = false;
            }
        }

        public void AddToScene()
        {
            if ((type == MPPointType.Boundary && app.DisplayBoundaryMarkers) || (type == MPPointType.Road && app.DisplayRoadMarkers))
            {
                if (!inScene)
                {

                    Vector3 scaleVec = new Vector3(1, 1, 1);
                    Vector3 rotVec = new Vector3(0, 0, 0);
                    displayObject = new DisplayObject(this, app, Name, "MultiPoint", app.Scene, meshName, position, scaleVec, rotVec, null);
                    displayObject.MaterialName = meshMaterial;
                    displayObject.TerrainOffset = this.terrainOffset;

                    if (displayObject.Entity.Mesh.TriangleIntersector == null)
                        displayObject.Entity.Mesh.CreateTriangleIntersector();
                    inScene = true;
                }
            }
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            if ((type == UpdateTypes.Regions || type == UpdateTypes.All) && hint == UpdateHint.DisplayMarker)
            {
                if (inScene == app.DisplayBoundaryMarkers)
                {
                    return;
                }
                else
                {
                    if (inScene)
                    {
                        this.RemoveFromScene();
                    }
                    else
                    {
                        this.AddToScene();
                    }
                }
            }
            if ((type == UpdateTypes.Road || type == UpdateTypes.All) && hint == UpdateHint.DisplayMarker)
            {
                if (inScene == app.DisplayRoadMarkers)
                {
                    return;
                }
                else
                {
                    if (inScene)
                    {
                        this.RemoveFromScene();
                    }
                    else
                    {
                        this.AddToScene();
                    }
                }
            }
            if ((type == UpdateTypes.All || type == UpdateTypes.Point) && hint == UpdateHint.TerrainUpdate)
            {
                this.position.y = app.GetTerrainHeight(position.x, position.z) + terrainOffset; 
            }
        }

        public void RemoveFromScene()
        {
            if (inScene || displayObject != null)
            {
                if (displayObject != null)
                {
                    displayObject.Dispose();
                    displayObject = null;
                }
            }
            inScene = false;
        }

        public void CheckAssets()
        {
        }

        public void ToXml(XmlWriter w)
        {
            w.WriteStartElement("Point");
            w.WriteAttributeString("WorldViewSelect", worldViewSelectable.ToString());
			w.WriteAttributeString("x", this.position.x.ToString());
			w.WriteAttributeString("y", this.position.y.ToString());
			w.WriteAttributeString("z", this.position.z.ToString());
			w.WriteEndElement();
        }

        [BrowsableAttribute(false)]
        public Vector3 FocusLocation
        {
            get
            {
                return position;
            }
        }

        [BrowsableAttribute(false)]
        public bool Highlight
        {
            get
            {
                return highlight;
            }
            set
            {
                if (highlight != value)
                {
                    highlight = value;
                    if (inScene)
                    {
                        displayObject.Highlight = value;
                    }
                }
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
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            RemoveFromScene();
        }

        #endregion

        #region IOjbectDrag Members

        [BrowsableAttribute(false)]
        public DisplayObject Display
        {
            get
            {
                return displayObject;
            }
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "Point";
            }
        }

        #endregion IOjbectDrag Members
    }
}
