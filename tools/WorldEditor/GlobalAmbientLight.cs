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
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    public class GlobalAmbientLight : IWorldObject
    {
        protected ColorEx color;
        protected SceneManager scene;
        protected WorldEditor app;
        protected IWorldContainer parent;
        protected WorldTreeNode parentNode;
        protected WorldTreeNode node;
        protected List<ToolStripButton> buttonBar;


        public GlobalAmbientLight(IWorldContainer parentContainer, WorldEditor worldEditor)
        {
            this.app = worldEditor;
            this.parent = parentContainer;
            this.scene = app.Scene;
            this.color = app.Config.DefaultAmbientLightColor;
        }

        public GlobalAmbientLight(IWorldContainer parentContainer,WorldEditor worldEditor, SceneManager sceneManager, ColorEx lightColor)
        {
            this.parent = parentContainer;
            this.app = worldEditor;
            this.scene = sceneManager;
            this.color = lightColor;
        }

        public GlobalAmbientLight(IWorldContainer parentContainer, WorldEditor worldEditor, SceneManager sceneManager,  XmlReader r)
        {
            this.app = worldEditor;
            this.parent = parentContainer;
            this.scene = sceneManager;

            fromXml(r);
        }


        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;

            // add the Fog node
            node = app.MakeTreeNode(this, "Global Ambient Light");
            parentNode.Nodes.Add(node);

            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Global_Ambient_Light", app.HelpClickHandler);

            node.ContextMenuStrip = menuBuilder.Menu;
            buttonBar = menuBuilder.ButtonBar;
        }

        public void Clone(IWorldContainer copyParent)
        {
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
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString +=  String.Format("\tColor:\r\n");
                objString +=  String.Format("\t\tR={0}\r\n",color.r);
                objString +=  String.Format("\t\tG={0}\r\n", color.g);
                objString +=  String.Format("\t\tB={0}\r\n", color.b);
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
            app.GlobalAmbientLight = this;
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
        }

        public void CheckAssets()
        {
        }

        [EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)),
DescriptionAttribute("Color of the global ambient light (click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Miscellaneous")]
        public ColorEx Color
        {
            get
            {
                return this.color;
            }
            set
            {
                this.color = value;
            }
        }

        public void RemoveFromScene()
        {
            app.GlobalAmbientLight = null;
        }

        public void ToXml(System.Xml.XmlWriter w)
        {

            w.WriteStartElement("GlobalAmbientLight");
            w.WriteStartElement("Color");
            w.WriteAttributeString("R", this.color.r.ToString());
            w.WriteAttributeString("G", this.color.g.ToString());
            w.WriteAttributeString("B", this.color.b.ToString());
            w.WriteEndElement(); // end color
            w.WriteEndElement(); // end GlobalAmbientLight
        }

        private void fromXml(System.Xml.XmlReader r)
        {
            // Parse sub-elements
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

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "GlobalAmbientLight";
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
