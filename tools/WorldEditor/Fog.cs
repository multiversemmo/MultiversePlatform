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
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Xml;
using Axiom.Core;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
	public class Fog : IWorldObject, IObjectDelete
	{
		protected WorldEditor app;
		protected WorldTreeNode parentNode;
		protected WorldTreeNode node;
		protected float near;
		protected float far;
		protected Boundary parent;
		protected bool inTree;
		protected ColorEx cx;
        protected List<ToolStripButton> buttonBar;


		public Fog(WorldEditor app, Boundary parent, ColorEx color, float nearin, float farin)
		{
			this.app = app;
			this.parentNode = null;
			this.parent = parent;
			this.cx = color;
			this.near = nearin;
			this.far = farin;
		}

        public Fog(XmlReader r, Boundary parent, WorldEditor app)
        {
            this.app = app;
            this.parentNode = null;
            this.parent = parent;

            FromXml(r);
        }

		public void ToXml(System.Xml.XmlWriter w)
		{
			w.WriteStartElement("Fog");
            w.WriteAttributeString("Far", this.far.ToString());
            w.WriteAttributeString("Near", this.near.ToString());
            w.WriteStartElement("Color");
			w.WriteAttributeString("R", this.cx.r.ToString());
			w.WriteAttributeString("G", this.cx.g.ToString());
			w.WriteAttributeString("B", this.cx.b.ToString());
            w.WriteEndElement(); // end color
			w.WriteEndElement(); // end Fog
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
                            cx = XmlHelperClass.ParseColorAttributes(r);
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

		private System.Drawing.Color ColorExToColor(ColorEx color)
		{
			return System.Drawing.Color.FromArgb((int)cx.ToARGB());
		}

		private ColorEx ColorToColorEx(Color c)
		{
			return new ColorEx(c.A / 255f, c.R / 255f, c.G / 255f, c.B / 255f);
		}

		[BrowsableAttribute(false)]
		public float Red
		{
			get
			{
				return cx.r;
			}
			set
			{
				cx.r = value;
			}
		}

		[BrowsableAttribute(false)]
		public float Green
		{
			get
			{
				return cx.g;
			}
			set
			{
				cx.g = value;
			}
		}

		[BrowsableAttribute(false)]
		public float Blue 
		{
			get
			{
				return cx.b;
			}
			set
			{
				cx.b = value;
			}
		}

        [EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)),
DescriptionAttribute("Color of this Fog. (Click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Miscellaneous")] 
        public ColorEx Color
        {
            get
            {
                return Cx;
            }
            set
            {
                cx = value;
            }

        }

        [BrowsableAttribute(false)]
		public ColorEx Cx
		{
			get
			{
				return cx;
			}
			set
			{
				cx = value;
			}
		}

        [CategoryAttribute("Distance"), DescriptionAttribute("Distance (in millimeters) from the camera where the fog effect begins. Closer than this distance, there will be no fog effects."), BrowsableAttribute(true)]
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

        [CategoryAttribute("Distance"),DescriptionAttribute("Distance (in millimeters) from the camera where the fog effect reaches its maximum. Objects farther from the camera will be completely obscured by the fog."), BrowsableAttribute(true)]
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

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
		public string ObjectType
		{
			get
			{
				return "Fog";
			}
		}

		#region IWorldObject Members

		public void AddToTree(WorldTreeNode parentNode)
		{
			this.parentNode = parentNode;
			inTree = true;

			// create a node for the collection and add it to the parent
            this.node = app.MakeTreeNode(this, "Fog");
			parentNode.Nodes.Add(node);

            // build the menu
            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Fog", app.HelpClickHandler);
            menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
            node.ContextMenuStrip = menuBuilder.Menu;
            buttonBar = menuBuilder.ButtonBar;
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
                // this property is not applicable to this object
            }
        }

        public void Clone(IWorldContainer copyParent)
        {
            Fog clone = new Fog(app, copyParent as Boundary, cx, near, far);
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString +=  String.Format("\tColor:\r\n");
                objString +=  String.Format("\t\tR:{0}\r\n",Cx.r);
                objString +=  String.Format("\t\tG:{0}\r\n",Cx.g);
                objString +=  String.Format("\t\tB:{0}\r\n",Cx.b);
                objString +=  String.Format("\tNear:{0})\r\n", Near);
                objString +=  String.Format("\tFar:{0})\r\n", Far);
                objString +=  "\r\n";
                return objString;
            }
        }


        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
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
			return;
		}

		public void RemoveFromScene()
		{
			return;
		}

        public void CheckAssets()
        {
        }

		[BrowsableAttribute(false)]
		public Axiom.MathLib.Vector3 FocusLocation
		{
			get
			{
				return this.parent.FocusLocation;
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
                return parent as IWorldContainer;
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
