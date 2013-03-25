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
using System.Xml;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    class DirectionalLight : IWorldObject, IObjectOrientation, IObjectDelete
    {
        protected WorldEditor app;
        protected Boundary parent;
        protected Vector3 lightDirection;
        protected ColorEx diffuse;
        protected ColorEx specular;
        protected WorldTreeNode parentNode;
        protected WorldTreeNode node;
        protected DisplayObject displayObject;
        protected string name;
        protected float azimuth;
        protected float zenith;
        protected float origAzimuth;
        protected float origZenith;
        protected bool inScene = false;
        protected List<ToolStripButton> buttonBar;
        protected Quaternion orientation;

        public DirectionalLight(WorldEditor worldEditor, Boundary parent, Vector3 lightDir, ColorEx diff, ColorEx spec)
        {
            this.app = worldEditor;
            this.parent = parent;
            this.lightDirection = lightDir;
            this.diffuse = diff;
            this.specular = spec;
            this.name = String.Format("{0}-{1}", this.parent.Name, "DirectionalLight");
        }

        public DirectionalLight(WorldEditor worldEditor, Boundary parent, XmlReader r)
        {
            this.app = worldEditor;
            this.parent = parent;
            fromXml(r);
        }


        #region IObjectOrientation members

        protected void UpdateOrientation()
        {
            Quaternion azimuthRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(azimuth), Vector3.UnitY);
            Quaternion zenithRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(-zenith), Vector3.UnitX);
            Matrix3 lightMatrix = (azimuthRotation * zenithRotation).ToRotationMatrix(); // Compute "position" of light (actually just reverse direction)
            Vector3 relativeLightPos = lightMatrix * Vector3.UnitZ;

            relativeLightPos.Normalize();

            this.lightDirection = -relativeLightPos;
            Quaternion displayZenithRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(-Zenith + 90), Vector3.UnitX);
            this.orientation = (azimuthRotation * displayZenithRotation);

            if (inScene)
            {
                this.displayObject.SetOrientation(orientation);
            }
        }

        public void SetDirection(float lightAzimuth, float lightZenith)
        {
            this.azimuth = lightAzimuth;
            this.zenith = lightZenith;

            UpdateOrientation();
        }

        [BrowsableAttribute(false)]
        public Quaternion Orientation
        {
            get
            {
                // XXX - This is wrong, will need to update it at some point
                return orientation;
            }
        }


        [BrowsableAttribute(false)]
        public float Azimuth
        {
            get
            {
                return azimuth;
            }
            set
            {
                azimuth = value;
                UpdateOrientation();
            }
        }

        [BrowsableAttribute(false)]
        public float Zenith
        {
            get
            {
                return zenith;
            }
            set
            {
                zenith = value;
                UpdateOrientation();
            }
        }

        #endregion //IOjectOrientation Members


        [BrowsableAttribute(false)]
        public float OrigAzimuth
        {
            get
            {
                return origAzimuth;
            }
            set
            {
                origAzimuth = value;
            }
        }

        [BrowsableAttribute(false)]
        public float OrigZenith
        {
            get
            {
                return origZenith;
            }
            set
            {
                origZenith = value;
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 LightDir
        {
            get
            {
                return lightDirection;
            }
            set
            {
                lightDirection = value;
            }
        }


        [EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)),
DescriptionAttribute("Diffuse color of the light (click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Color")]
        public ColorEx Diffuse
        {
            get
            {
                return diffuse;
            }
            set
            {
                diffuse = value;
            }
        }

        [EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)),
DescriptionAttribute("Specular color of the light (click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Color")]
        public ColorEx Specular
        {
            get
            {
                return specular;
            }
            set
            {
                specular = value;
            }
        }

        [DescriptionAttribute("Name of this directional light."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }


        #region IWorldObject Members
        
        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;

            // add the Fog node
            node = app.MakeTreeNode(this, "Directional Light");
            parentNode.Nodes.Add(node);

            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Directional_Light", app.HelpClickHandler);
            menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
            node.ContextMenuStrip = menuBuilder.Menu;
            buttonBar = menuBuilder.ButtonBar;
        }


        public void Clone(IWorldContainer copyParent)
        {
            DirectionalLight clone = new DirectionalLight(app, copyParent as Boundary, lightDirection, diffuse, specular);
            clone.Azimuth = azimuth;
            clone.Zenith = zenith;
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString +=  String.Format("\tSpecular:\r\n");
                objString +=  String.Format("\t\tR={0}\r\n", Specular.r);
                objString +=  String.Format("\t\tG={0}\r\n", Specular.g);
                objString +=  String.Format("\t\tB={0}\r\n", Specular.b);
                objString +=  String.Format("\tDiffuse:\r\n");
                objString +=  String.Format("\t\tR={0}\r\n", Diffuse.r);
                objString +=  String.Format("\t\tG={0}\r\n", Diffuse.g);
                objString +=  String.Format("\t\tB={0}\r\n", Diffuse.b);
                objString +=  "\r\n";
                return objString;
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


        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
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

        
        [BrowsableAttribute(false)]
        public DisplayObject Display
        {
            get
            {
                return null;
            }
        }

        public void RemoveFromTree()
        {
            if (node.IsSelected)
            {
                node.UnSelect();
            }
            parentNode.Nodes.Remove(node);
            parentNode = null;
            node = null;
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


        public void AddToScene()
        {
            float x = parent.FocusLocation.x;
            float z = parent.FocusLocation.z;

            Vector3 location = new Vector3(x, app.GetTerrainHeight(x, z) + 2000, z);
            displayObject = new DisplayObject(String.Format("{0}-{1}", parent.Name, "DirectionalLight"), app,"DirectionalLight", app.Scene, (app.Assets.assetFromName(app.Config.DirectionalLightMeshName)).AssetName, location, new Vector3(1, 1, 1), this.Orientation, null);
            inScene = true;

            UpdateOrientation();

            if (displayObject.Entity.Mesh.TriangleIntersector == null)
                displayObject.Entity.Mesh.CreateTriangleIntersector();
        }

        public void RemoveFromScene()
        {
            app.GlobalDirectionalLight = null;
            if (displayObject != null)
            {
                displayObject.Dispose();
            }
            displayObject = null;

            inScene = false;
        }

        public void CheckAssets()
        {
            CheckAsset(app.Assets.assetFromName(app.Config.DirectionalLightMeshName).AssetName);
        }

        protected void CheckAsset(string name)
        {
            if ((name != null) && (name != ""))
            {
                if (!app.CheckAssetFileExists(name))
                {
                    app.AddMissingAsset(name);
                }
            }
        }

        public void ToXml(System.Xml.XmlWriter w)
        {
            w.WriteStartElement("DirectionalLight");
            w.WriteAttributeString("Azimuth", this.azimuth.ToString());
            w.WriteAttributeString("Zenith", this.zenith.ToString());
            w.WriteStartElement("Direction");
            w.WriteAttributeString("x", this.lightDirection.x.ToString());
            w.WriteAttributeString("y", this.lightDirection.y.ToString());
            w.WriteAttributeString("z", this.lightDirection.z.ToString());
            w.WriteEndElement(); // End Direction
            w.WriteStartElement("Diffuse");
            w.WriteAttributeString("R", this.diffuse.r.ToString());
            w.WriteAttributeString("G", this.diffuse.g.ToString());
            w.WriteAttributeString("B", this.diffuse.b.ToString());
            w.WriteEndElement(); // End diffuse
            w.WriteStartElement("Specular");
            w.WriteAttributeString("R", this.specular.r.ToString());
            w.WriteAttributeString("G", this.specular.g.ToString());
            w.WriteAttributeString("B", this.specular.b.ToString());
            w.WriteEndElement(); // End Specular
            w.WriteEndElement(); // End GlobalAmbientLight
        }


        private void parseOrientation(XmlReader r)
        {
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Azimuth":
                        azimuth = float.Parse(r.Value);
                        break;
                    case "Zenith":
                        zenith = float.Parse(r.Value);
                        break;
                }
            }
        }

        private void fromXml(XmlReader r)
        {
            // Parse sub-elements
            parseOrientation(r);
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    // save the name of the element
                    string elementName = r.Name;
                    switch (elementName)
                    {
                        case "Direction":
                            this.lightDirection = XmlHelperClass.ParseVectorAttributes(r);
                            break;
                        case "Diffuse":
                            diffuse = XmlHelperClass.ParseColorAttributes(r);
                            break;
                        case "Specular":
                            specular = XmlHelperClass.ParseColorAttributes(r);
                            break;
                    }
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 FocusLocation
        {
            get
            {
                return parent.FocusLocation;
            }
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "DirectionalLight";
            }
        }

        [BrowsableAttribute(false)]
        public bool Highlight
        {
            get
            {
                return displayObject.Highlight;
            }
            set
            {
                displayObject.Highlight = value;
            }
        }

        [BrowsableAttribute(false)]
        public WorldTreeNode Node
        {
            get
            {
                return node;
            }
            set
            {
                node = value;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IWorldObject Members


        public void ToManifest(System.IO.StreamWriter w)
        {
        }

        #region IWorldDelete

        [BrowsableAttribute(false)]
        public IWorldContainer Parent
        {
            get
            {
                return parent;
            }
        }

        #endregion IWorldDelete

        #endregion
    }
}
