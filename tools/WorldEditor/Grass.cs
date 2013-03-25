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
using System.ComponentModel;
using System.Windows.Forms;
using Axiom.MathLib;


namespace Multiverse.Tools.WorldEditor
{
	public class Grass : IWorldObject, IWorldContainer, IObjectDelete
	{
		protected string name;
		protected WorldEditor app;
		protected Boundary parent;
		protected bool highlight;
		protected bool inTree = false;
		protected bool inScene = false;
		protected WorldTreeNode node = null;
		protected WorldTreeNode parentNode = null;
		protected Axiom.SceneManagers.Multiverse.VegetationSemantic vegieSemantic;
		protected List<IWorldObject> plantList;
        protected List<ToolStripButton> buttonBar;

		public Grass(Boundary parent, WorldEditor app, String name)
		{
			this.parent = parent;
			this.app = app;
			this.name = name;
			this.plantList = new List<IWorldObject>();
		}

		public void ToXml(XmlWriter w)
		{
			w.WriteStartElement("Grass");
			w.WriteAttributeString("Name", this.name);
			foreach(PlantType plant in plantList)
			{
				plant.ToXml(w);
			}
			w.WriteEndElement(); //end Grass
		}

		public Grass(XmlReader r, Boundary parent, WorldEditor app)
		{
            this.parent = parent;
            this.app = app;
            this.plantList = new List<IWorldObject>();

            FromXml(r);
		}


        public void FromXml(XmlReader r)
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
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.

            if (!r.IsEmptyElement)
            {
                while (r.Read())
                {
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
                        switch (r.Name)
                        {
                            case "PlantType":
                                PlantType plant = new PlantType(r, this, app);
                                Add(plant);
                                break;
                        }
                    }
                }
            }
        }

		[BrowsableAttribute(false)]
		public Axiom.SceneManagers.Multiverse.VegetationSemantic VegieSemantic
		{
			get
			{
				return vegieSemantic;
			}
		}

		public void Dispose()
		{
			RemoveFromScene();
        }

        [DescriptionAttribute("The name of this Vegetation."), BrowsableAttribute(true), CategoryAttribute("Miscellaneous")]
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

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "Grass";
            }
        }

        #region IWorldObject Members

        public void AddToScene()
		{
			inScene = true;
			vegieSemantic = new Axiom.SceneManagers.Multiverse.VegetationSemantic(this.name, this.parent.SceneBoundary);
			parent.SceneBoundary.AddSemantic(this.vegieSemantic);

			foreach (IWorldObject plant in plantList)
			{
				plant.AddToScene();
			}
		}

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", NodeName );
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
                // this property is not applicable to this object
            }
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            foreach (IWorldObject child in plantList)
            {
                child.UpdateScene(type, hint);
            }
        }

		public void AddToTree(WorldTreeNode parentNode)
		{
			this.parentNode = parentNode;
			inTree = true;

			// create a node for the collection and add it to the parent
            node = app.MakeTreeNode(this, NodeName);

			parentNode.Nodes.Add(node);
			Grass grass = this;
			CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
			menuBuilder.Add("Add Plant Type", new AddPlantTypeCommandFactory(app, grass), app.DefaultCommandClickHandler);
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Grass", app.HelpClickHandler);
            menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
			node.ContextMenuStrip = menuBuilder.Menu;

			foreach (IWorldObject plant in plantList)
			{
				plant.AddToTree(node);
            }
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


        public void Clone(IWorldContainer copyParent)
        {
            Grass clone = new Grass(copyParent as Boundary, app, name);
            foreach (IWorldObject child in plantList)
            {
                child.Clone(clone);
            }
            copyParent.Add(clone);
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
            foreach (IWorldObject plant in plantList)
            {
                plant.RemoveFromTree();
            }
			parentNode.Nodes.Remove(node);
			parentNode = null;
			node = null;
		}

		public void RemoveFromScene()
		{
            foreach (IWorldObject plant in plantList)
            {
                plant.RemoveFromScene();
            }
			if (this.vegieSemantic != null && parent != null && parent.SceneBoundary != null)
			{
				parent.SceneBoundary.RemoveSemantic(this.vegieSemantic);
			}
		}


        public void CheckAssets()
        {
            foreach (IWorldObject child in plantList)
            {
                child.CheckAssets();
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
            if (plantList != null)
            {
                foreach (IWorldObject child in plantList)
                {
                    child.ToManifest(w);
                }
            }
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

        #region ICollection<IWorldObject> Members

        public void Add(IWorldObject item)
		{
			plantList.Add(item as PlantType);
			if (inTree)
			{
				item.AddToTree(node);
			}
			if (inScene)
			{
				item.AddToScene();
			}
		}

		public void Clear()
		{
			plantList.Clear();
		}

		public bool Contains(IWorldObject item)
		{
			return plantList.Contains(item as PlantType);
		}

		public void CopyTo(IWorldObject[] array, int arrayIndex)
		{
			plantList.CopyTo(array, arrayIndex);
		}

        [BrowsableAttribute(false)]
		public int Count
		{
			get
			{
				return plantList.Count;
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
            if (inTree)
            {
                item.RemoveFromTree();
            }
            if (inScene)
            {
                item.RemoveFromScene();
            }

			return plantList.Remove(item);
		}

		#endregion

		#region IEnumerable<IWorldObject> Members

		public IEnumerator<IWorldObject> GetEnumerator()
		{
			return plantList.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
