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
using System.Text;
using System.Xml;
using Axiom.MathLib;



using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Multiverse.Tools.WorldEditor
{
	public class Water : IWorldObject, IObjectDelete
	{
		protected float height;
		protected Axiom.SceneManagers.Multiverse.WaterPlane waterSemantic;
		protected Boundary parent;
		protected WorldEditor app;
		protected WorldTreeNode node = null;
        protected WorldTreeNode parentNode = null;
        protected List<ToolStripButton> buttonBar;
		bool inTree = false;
        bool inScene = false;

		public Water(float height, Boundary parent, WorldEditor app)
		{
			this.parent = parent;
			this.app = app;
			this.height = height;
		}

        [BrowsableAttribute(true), CategoryAttribute("Miscellaneous"), DescriptionAttribute("Altitude (in millimeters) of the water plane.")]
		public float Height
		{
			get
			{
				return height;
			}
			set
			{
				height = value;
                if (inScene)
                {
                    waterSemantic.Height = height;
                }
			}

		}

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
		public string ObjectType
		{
			get
			{
				return "Water";
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

		public void ToXml(XmlWriter w)
		{
			w.WriteStartElement("Water");
			w.WriteAttributeString("Height", this.Height.ToString());
			w.WriteEndElement();
			return;
		}

		public Water(XmlReader r, Boundary parent, WorldEditor app)
		{
			this.parent = parent;
			this.app = app;
            FromXml(r);
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
                    case "Height":
                        this.height = float.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.
        }

		#region IWorldObject Members


		public void AddToScene()
		{
            if (!inScene)
            {
                inScene = true;
                this.waterSemantic = new Axiom.SceneManagers.Multiverse.WaterPlane(this.Height, WorldEditor.GetUniqueName((parent as Boundary).Name, "BoundaryWaterSemantic"), null);
                parent.SceneBoundary.AddSemantic(this.waterSemantic);
            }
		}

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
        }

		public void AddToTree(WorldTreeNode parentNode)
		{

            this.parentNode = parentNode;
            if (!inTree)
            {
                inTree = true;

                // create a node for the collection and add it to the parent
                node = app.MakeTreeNode(this, "Water");
                parentNode.Nodes.Add(node);

                // build the menu
                CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
                menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
                menuBuilder.Add("Help", "Water", app.HelpClickHandler);
                menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                node.ContextMenuStrip = menuBuilder.Menu;
                buttonBar = menuBuilder.ButtonBar;
            }
        }

        public void Clone(IWorldContainer copyParent)
        {
            Water clone = new Water(height, copyParent as Boundary, app);
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString = String.Format("\tHeight={0}\r\n", Height);
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
                parentNode.Nodes.Remove(node);
                parentNode = null;
                node = null;

                inTree = false;
            }
        }

		public void RemoveFromScene()
		{
            if (inScene)
            {
                if (parent != null && parent.SceneBoundary != null && this.waterSemantic != null)
                {
                    parent.SceneBoundary.RemoveSemantic(this.waterSemantic);
                }
            }
            inScene = false;
		}

        public void CheckAssets()
        {
        }

		[BrowsableAttribute(false)]
		public Vector3 FocusLocation
		{
			get
			{
				return parent.FocusLocation;
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
			RemoveFromScene();
		}

		#endregion
	}
}
