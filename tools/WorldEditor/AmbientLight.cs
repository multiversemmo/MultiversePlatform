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
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Text;
using Axiom.Core;
using Multiverse.ToolBox;


namespace Multiverse.Tools.WorldEditor
{
    class AmbientLight : IWorldObject, IObjectDelete
    {
        protected WorldEditor app;
        protected Boundary parent;
        protected ColorEx color;
        protected WorldTreeNode parentNode;
        protected WorldTreeNode node;
        protected bool inTree;
        // protected bool inScene;
        protected List<ToolStripButton> buttonBar;
        
        #region IWorldObject Members

        public AmbientLight(WorldEditor worldEditor, Boundary parent, ColorEx lightColor)
        {
            this.app = worldEditor;
            this.parent = parent;
            this.color = lightColor;
        }

        public AmbientLight(WorldEditor worldEditor, Boundary parent)
        {
            this.app = worldEditor;
            this.parent = parent;
            this.color = app.Config.DefaultAmbientLightColor;
        }

        public AmbientLight(WorldEditor worldEditor, Boundary parent, XmlReader r)
        {
            this.app = worldEditor;
            this.parent = parent;
            fromXml(r);
        }

        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;
            inTree = true;

            // create a node for the collection and add it to the parent
            this.node = app.MakeTreeNode(this, "Ambient Light");
            parentNode.Nodes.Add(node);

            // build the menu
            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Ambient_Light", app.HelpClickHandler);
            menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
            node.ContextMenuStrip = menuBuilder.Menu;
            // int count; (unused)
            buttonBar = menuBuilder.ButtonBar;
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
                // This property is not relevent for this object.
            }
        }

        public void Clone(IWorldContainer copyParent)
        {
            AmbientLight clone = new AmbientLight(app, parent, color);
            copyParent.Add(clone);
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
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString +=  String.Format("\tColor:\r\n");
                objString +=  String.Format("\t\tR={0}\r\n", color.r);
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

        [EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)),
DescriptionAttribute("Color of the ambient light (click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Miscellaneous")]
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
            return;
        }

        public void RemoveFromScene()
        {
            return;
        }

        public void CheckAssets()
        {
        }

        public void ToXml(System.Xml.XmlWriter w)
        {
            w.WriteStartElement("AmbientLight");
            w.WriteStartElement("Color");
            w.WriteAttributeString("R", this.color.r.ToString());
            w.WriteAttributeString("G", this.color.g.ToString());
            w.WriteAttributeString("B", this.color.b.ToString());
            w.WriteEndElement(); // end color
            w.WriteEndElement(); // end AmbientLight
        }

        public void fromXml(XmlReader r)
        {
            //parse the sub-elements
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
				return this.parent.FocusLocation;
			}
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
        public string ObjectType
        {
			get
			{
				return "AmbientLight";
			}
        }

        [BrowsableAttribute(false)]
        public bool Highlight
        {
            get
            {
                return parent.Highlight;
            }
            set
            {
                parent.Highlight = value;
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


        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
