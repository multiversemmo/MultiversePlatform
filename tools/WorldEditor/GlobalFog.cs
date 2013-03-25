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
using System.Xml;
using System.Text;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    public class GlobalFog : IWorldObject
    {
        protected float far;
        protected float near;
        protected ColorEx color;
        protected IWorldContainer parent;
        protected WorldEditor app;
        protected WorldTreeNode parentNode;
        protected WorldTreeNode node;
        protected SceneManager scene;
        protected List<ToolStripButton> buttonBar;

        public GlobalFog(IWorldContainer parentContainer, WorldEditor worldEditor)
        {
            this.parent = parentContainer;
            this.app = worldEditor;
            this.scene = app.Scene;
            this.color = app.Config.FogColorDefault;
            this.far = app.Config.FogFarDefault;
            this.near = app.Config.FogNearDefault;
        }

        public GlobalFog(IWorldContainer parentContainer, WorldEditor worldEditor, XmlReader r):
            this(parentContainer, worldEditor)
        {
            fromXml(r);
        }
            

        [EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)),
DescriptionAttribute("Color of the fog (red, green, and blue values from 0 to 255).(click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Miscellaneous")]
        public ColorEx Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        [BrowsableAttribute(true), DescriptionAttribute("Distance (in millimeters) from the camera where the fog effect reaches its maximum. Objects farther from the camera than this distance will be completely obscured (in this color)."), CategoryAttribute("Distance")]
        public float Far
        {
            get
            {
                return far;
            }
            set
            {
                far = value;
            }
        }

        [DescriptionAttribute("Distance (in millimeters) from the camera where the fog effect begins. Closer than this distance, there will be no fog effects."), CategoryAttribute("Distance")]
        public float Near
        {
            get
            {
                return near;
            }
            set
            {
                near = value;
            }
        }

        #region IWorldObject Members
        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;

            // add the Fog node
            node = app.MakeTreeNode(this, "Global Fog");
            parentNode.Nodes.Add(node);

            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();

            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Global_Fog", app.HelpClickHandler);

            node.ContextMenuStrip = menuBuilder.Menu;
            buttonBar = menuBuilder.ButtonBar;
        }

        [BrowsableAttribute(false)]
        public bool IsGlobal
        {
            get
            {
                return true;
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
                // this property is not applicable to this object
            }
        }

        public void Clone(IWorldContainer copyParent)
        {
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString +=  String.Format("\tColor:\r\n");
                objString +=  String.Format("\t\tR={0}\r\n",Color.r);
                objString +=  String.Format("\t\tG={0}\r\n",Color.g);
                objString +=  String.Format("\t\tB={0}\r\n",Color.b);
                objString +=  String.Format("\tNear={0}\r\n",Near);
                objString +=  String.Format("\tFar={0}\r\n",Far);
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

        public void AddToScene()
        {
            app.GlobalFog = this;
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
        }

        public void RemoveFromScene()
        {
            app.GlobalFog = null;
        }


        public void CheckAssets()
        {
        }

        public void ToXml(XmlWriter w)
        {
            w.WriteStartElement("GlobalFog");
            w.WriteAttributeString("Far", this.far.ToString());
            w.WriteAttributeString("Near", this.near.ToString());
            w.WriteStartElement("Color");
            w.WriteAttributeString("R", this.color.r.ToString());
            w.WriteAttributeString("G", this.color.g.ToString());
            w.WriteAttributeString("B", this.color.b.ToString());
            w.WriteEndElement(); // end color
            w.WriteEndElement(); // end Fog
        }

        private void fromXml(XmlReader r)
        {
            // first parse the attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Far":
                        this.far = float.Parse(r.Value);
                        break;
                    case "Near":
                        this.near = float.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.

            // now parse the sub-elements
            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Element)
                {
                    // parse that element
                    // save the name of the element
                    string elementName = r.Name;
                    switch (elementName)
                    {
                        case "Color":
                            color = XmlHelperClass.ParseColorAttributes(r);
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
        public Axiom.MathLib.Vector3 FocusLocation
        {
            get
            {
                return Vector3.Zero;
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
            }
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "GlobalFog";
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

        public void ToManifest(System.IO.StreamWriter w)
        {
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
