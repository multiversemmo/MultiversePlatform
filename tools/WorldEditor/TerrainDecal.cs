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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Axiom.SceneManagers.Multiverse;
using Axiom.MathLib;
using System.ComponentModel;
using System.Windows.Forms;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    public class TerrainDecal : IWorldObject, IObjectDrag, IObjectChangeCollection, IObjectPosition, IObjectRotation, IObjectCutCopy, IObjectCameraLockable
    {
        protected WorldEditor app;
        protected IWorldContainer parent;
        protected string name;
        protected DecalElement decal;
        protected Vector2 position;
        protected Vector2 size;
        protected string imageName;
        protected float rotation;
        protected int priority;
        protected bool inScene;
        protected bool inTree;
        protected WorldTreeNode node;
        protected WorldTreeNode parentNode;
        protected List<ToolStripButton> buttonBar;
        protected float perceptionRadius = 0;

        public TerrainDecal(WorldEditor worldEditor, IWorldContainer parent, string name, Vector2 position, Vector2 size, string image, int priority)
        {
            this.app = worldEditor;
            this.name = name;
            this.parent = parent;
            this.position = position;
            this.size = size;
            this.imageName = image;
            this.rotation = 0f;
            this.priority = priority;
        }

        public TerrainDecal(WorldEditor worldEditor, IWorldContainer parent, XmlReader r)
        {
            this.app = worldEditor;
            this.parent = parent;
            FromXml(r);
        }

        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            if (!inTree && parentNode != null)
            {
                this.parentNode = parentNode;

                // add the terrain node
                node = app.MakeTreeNode(this, NodeName);
                parentNode.Nodes.Add(node);

                CommandMenuBuilder menuBuilder = new CommandMenuBuilder();

                menuBuilder.Add("Drag Decal", new DragObjectsFromMenuCommandFactory(app), app.DefaultCommandClickHandler);
                menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
                menuBuilder.AddDropDown("Move to Collection", menuBuilder.ObjectCollectionDropDown_Opening);
                menuBuilder.Add("Help", ObjectType, app.HelpClickHandler);
                menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                node.ContextMenuStrip = menuBuilder.Menu;
                inTree = true;
                buttonBar = menuBuilder.ButtonBar;
            }
            else
            {
                inTree = false;
            }
        }

        public void RemoveFromTree()
        {
            if (node != null && node.IsSelected)
            {
                node.UnSelect();
            }
            if (inTree)
            {
                parentNode.Nodes.Remove(node);
                parentNode = null;
                node = null;

                inTree = false;
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
        public float Radius
        {
            get
            {
                float a = SizeX / 2;
                float b = SizeZ / 2;
                float c = (float)(Math.Sqrt((Math.Pow(((double)a),2.0) + (Math.Pow(((double) b),2.0)))));
                return c;
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 Center
        {
            get
            {
                return new Vector3(position.x, app.GetTerrainHeight(position.x, position.y), position.y);
            }
        }


        [DescriptionAttribute("This allows you to set the distance at which the object can be seen, in millimeters. 0 means it will be displayed out to the normal perception radius of the player."), CategoryAttribute("Display Propeties")]
        public float PerceptionRadius
        {
            get
            {
                return perceptionRadius;
            }
            set
            {
                perceptionRadius = value;
            }
        }


        public void AddToScene()
        {
            if (!inScene)
            {
                decal = TerrainManager.Instance.TerrainDecalManager.CreateDecalElement(imageName, position.x,
                    position.y, size.x, size.y, rotation, 0f, 0f, priority);
                inScene = true;
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

        public void RemoveFromScene()
        {
            if (inScene)
            {
                TerrainManager.Instance.TerrainDecalManager.RemoveDecalElement(decal);
                inScene = false;
            }
        }

        public void CheckAssets()
        {
            if (imageName != null)
            {
                if (!app.CheckAssetFileExists(imageName))
                {
                    app.AddMissingAsset(imageName);
                }
            }
        }


        [BrowsableAttribute(false)]
        public DecalElement Decal
        {
            get
            {
                return decal;
            }
        }

        public void ToXml(System.Xml.XmlWriter w)
        {
            w.WriteStartElement(ObjectType);
            w.WriteAttributeString("Name", name);
            w.WriteAttributeString("ImageName", imageName);
            w.WriteAttributeString("PositionX", position.x.ToString());
            w.WriteAttributeString("PositionZ", position.y.ToString());
            w.WriteAttributeString("SizeX", size.x.ToString());
            w.WriteAttributeString("SizeZ", size.y.ToString());
            w.WriteAttributeString("Rotation", rotation.ToString());
            w.WriteAttributeString("Priority", priority.ToString());
            w.WriteAttributeString("PerceptionRadius", perceptionRadius.ToString());
            w.WriteEndElement();
        }

        public void ToManifest(System.IO.StreamWriter w)
        {
            w.WriteLine("Texture:{0}", imageName);
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            if ((type == UpdateTypes.TerrainDecal || type == UpdateTypes.All) && hint == UpdateHint.DisplayDecal)
            {
                if (inScene == app.DisplayTerrainDecals)
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

        public void UpdateNode()
        {
            if (inTree)
            {
                node.Text = NodeName;
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 FocusLocation
        {
            get
            {
                return new Vector3(position.x, app.GetTerrainHeight(position.x,position.y), position.y); 
            }
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "TerrainDecal";
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
                // do nothing
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

        [BrowsableAttribute(false)]
        public List<ToolStripButton> ButtonBar
        {
            get
            {
                return buttonBar;
            }
        }

        public void Clone(IWorldContainer copyParent)
        {
            TerrainDecal clone = new TerrainDecal(app, copyParent, name, position, size, imageName, priority);
            clone.Rotation = rotation;
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", NodeName);
                objString += String.Format("\tImageName={0}\r\n",imageName);
                objString += String.Format("\tSizeX:{0}\r\n", size.x);
                objString += String.Format("\tSizeZ:{0}\r\n", size.y);
                objString += String.Format("\tPriority:{0}\r\n", priority);
                objString += String.Format("\tPerceptionRadius:{0}\r\n", perceptionRadius);
                objString += String.Format("\tPosition:({0},{1},{2})\r\n", FocusLocation.x, FocusLocation.y, FocusLocation.z);
                objString += String.Format("\tRotation:{0} degrees\r\n", rotation);
                objString += "\r\n";
                return objString;
            }
        }



        #endregion IWorldObjectMembers

        public void FromXml(XmlReader r)
        {
            position = new Vector2();
            size = new Vector2();
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                switch (r.Name)
                {
                    case "Name":
                        name = r.Value;
                        break;
                    case "ImageName":
                        imageName = r.Value;
                        break;
                    case "PositionX":
                        position.x= float.Parse(r.Value);
                        break;
                    case "PositionZ":
                        position.y = float.Parse(r.Value);
                        break;
                    case "SizeX":
                        size.x = float.Parse(r.Value);
                        break;
                    case "SizeZ":
                        size.y = float.Parse(r.Value);
                        break;
                    case "Rotation":
                        rotation = float.Parse(r.Value);
                        break;
                    case "Priority":
                        priority = int.Parse(r.Value);
                        break;
                    case "PerceptionRadius":
                        perceptionRadius = float.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement();
        }

        #region IDisposable Members

        public void Dispose()
        {
            TerrainManager.Instance.TerrainDecalManager.RemoveDecalElement(decal);
        }

        #endregion

        #region IObjectDrag Members

        [BrowsableAttribute(false)]
        public DisplayObject Display
        {
            get
            {
                return null;
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

        #endregion

        #region IObjectPosition Members


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
                // do nothing
            }
        }

        #endregion

        #region IObjectRotation Members

        [BrowsableAttribute(false)]
        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                if (decal != null)
                {
                    decal.Rot = rotation;
                }
            }
        }

        #endregion

        #region IObjectPosition Members


        [BrowsableAttribute(false)]
        public Vector3 Position
        {
            get
            {
                return FocusLocation;
            }
            set
            {
                position.x = value.x;
                position.y = value.z;
                if (inScene)
                {
                    decal.PosX = position.x;
                    decal.PosZ = position.y;
                }
            }
        }
        #endregion

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), BrowsableAttribute(true), CategoryAttribute("Miscellaneous"), DescriptionAttribute("The texture file to be applied to the terrain")]
        public string Filename
        {
            get
            {
                return imageName;
            }
            set
            {
                if(ValidityHelperClass.assetExists(value))
                {
                    imageName = value;
                    if (inScene)
                    {
                        RemoveFromScene();
                        AddToScene();
                    }                    
                }
            }
        }

        [BrowsableAttribute(false)]
        public Vector2 Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        [BrowsableAttribute(true), CategoryAttribute("Miscellaneous"), DescriptionAttribute("The name of the Terrain Decal")]
        public string Name
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


        [BrowsableAttribute(true), CategoryAttribute("Display Properties"), DescriptionAttribute("Sets the order in which the Terrain Decal is drawn.  Lower will be drawn first")]
        public int Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
                decal.Priority = priority;
            }
        }

        [BrowsableAttribute(true), CategoryAttribute("Size"), DescriptionAttribute("The size of the decal along the x axis")]
        public float SizeX
        {
            get
            {
                return size.x;
            }
            set
            {
                size.x = value;
                decal.SizeX = value;
            }
        }

        [BrowsableAttribute(true), CategoryAttribute("Size"), DescriptionAttribute("The size of the decal along the x axis")]
        public float SizeZ
        {
            get
            {
                return size.y;
            }
            set
            {
                size.y = value;
                decal.SizeZ = value;
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
    }
}
