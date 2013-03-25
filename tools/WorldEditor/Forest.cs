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
using Multiverse.ToolBox;


namespace Multiverse.Tools.WorldEditor
{
	public class Forest : IWorldObject, IWorldContainer, IObjectDelete
	{
		protected string name;
		protected string filename;
		protected int seed;
		protected float windSpeed;
		protected Vector3 windDirection;
		protected List<IWorldObject> treeTypes;
		protected Axiom.SceneManagers.Multiverse.Forest forestSemantic;
		protected Boundary parent;
		protected WorldEditor app;
		protected bool highlight;
		protected bool inTree = false;
		protected bool inScene = false;
		protected WorldTreeNode node = null;
		protected WorldTreeNode parentNode = null;
        protected List<ToolStripButton> buttonBar;

		public Forest(string filename, float windspeed, Vector3 winddirection, int seed, Boundary parent, WorldEditor app)
		{
			this.parent = parent;
			this.app = app;
			this.name = "Forest";
			this.filename = filename;
			this.windSpeed = windspeed;
			this.windDirection = winddirection;
			this.treeTypes = new List<IWorldObject>();
			this.seed = seed;
		}
         
        public void ToXml(XmlWriter w)
        {
			w.WriteStartElement("Forest");
			w.WriteAttributeString("Name", this.name);
			w.WriteAttributeString("Filename", this.filename);
			w.WriteAttributeString("WindSpeed", this.windSpeed.ToString());
			w.WriteAttributeString("Seed", this.seed.ToString());
			w.WriteStartElement("WindDirection");
			w.WriteAttributeString("x", this.windDirection.x.ToString());
			w.WriteAttributeString("y", this.windDirection.y.ToString());
			w.WriteAttributeString("z", this.windDirection.z.ToString());
			w.WriteEndElement(); //WindDirection End
			foreach (IWorldObject tree in treeTypes)
			{
				tree.ToXml(w);
			}
            w.WriteEndElement(); // Forest
        }

        public Forest(XmlReader r, Boundary parent, WorldEditor worldEditor)
        {
            this.parent = parent;
            this.app = worldEditor;
            this.treeTypes = new List<IWorldObject>();

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
                    case "Filename":
                        this.filename = r.Value;
                        break;
                    case "WindSpeed":
                        this.windSpeed = float.Parse(r.Value);
                        break;
                    case "Seed":
                        this.seed = int.Parse(r.Value);
                        break;
                    case "WindDirection":
                        windDirection = XmlHelperClass.ParseVectorAttributes(r);
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.

            if (!r.IsEmptyElement)
            {
                while (r.Read())
                {
                    if (r.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    if (r.NodeType == XmlNodeType.Element)
                    {
                        switch (r.Name)
                        {
                            case "Tree":
                                Tree t = new Tree(r, this, app);
                                Add(t);
                                break;
                        }
                    }
                }
            }
        }

        [BrowsableAttribute(false)]
		public Axiom.SceneManagers.Multiverse.Forest ForestSemantic
		{
			get
			{
				return forestSemantic;
			}
		}


		[TypeConverter(typeof(SpeedWindFileListUITypeEditor)), CategoryAttribute("Wind"), DescriptionAttribute("Name of the SpeedWind file that contains the wind parameters for the trees in this forest.  Multiverse ships with one SpeedWind file called '''demoWind'''.  Licensees of SpeedTree can use the SpeedTree CAD tool to create other wind files.")]
		public string SpeedWindFilename
		{
			get
			{
				string nam = app.Assets.assetFromAssetName(this.filename).Name;
				return nam;
			}
			set
			{
				filename = app.Assets.assetFromName(value).AssetName;
				this.ForestSemantic.WindFilename = filename;
			}
		}

        [BrowsableAttribute(true), CategoryAttribute("Miscellaneous"), DescriptionAttribute("Random number seed value for tree placement. Must be an integer between -2,147,483,648 and 2,147,483,647.")]
		public int Seed
		{
			get
			{
				return seed;
			}
			set
			{
				seed = value;
				this.ForestSemantic.Seed = Seed;
			}
		}

        [BrowsableAttribute(true), CategoryAttribute("Wind"), DescriptionAttribute("Speed of the wind, which is feed into the SpeedTree library to provide animations for the trees. Must be a floating-point number between zero (0) and one (1).")]
		public float WindSpeed
		{
			get
			{
				return windSpeed;
			}
			set
			{
				windSpeed = value;
				this.ForestSemantic.WindStrength = value;
			}
		}

        [BrowsableAttribute(true), CategoryAttribute("Wind"), DescriptionAttribute("The X component of the wind direction vector")]
		public float WindDirectionX
		{
			get
			{
				return this.windDirection.x;
			}
			set
			{
				this.windDirection.x = value;
				this.ForestSemantic.WindDirection = this.windDirection;
			}
		}

        [BrowsableAttribute(true), CategoryAttribute("Wind"), DescriptionAttribute("The Y component of the wind direction vector")]
		public float WindDirectionY
		{
			get
			{
				return this.windDirection.y;
			}
			set
			{
				this.windDirection.y = value;
				this.ForestSemantic.WindDirection = this.windDirection;
			}
		}

        [BrowsableAttribute(true), CategoryAttribute("Wind"), DescriptionAttribute("The Z component of the wind direction vector")]
		public float WindDirectionZ
		{
			get
			{
				return this.windDirection.z;
			}
			set
			{
				this.windDirection.z = value;
				this.ForestSemantic.WindDirection = this.windDirection;
			}
		}


		public void AddToScene()
		{
            if (!inScene)
            {
                inScene = true;
                forestSemantic = new Axiom.SceneManagers.Multiverse.Forest(this.seed, WorldEditor.GetUniqueName("ForestBoundarySemantic", parent.Name), app.Scene.RootSceneNode);
                parent.SceneBoundary.AddSemantic(this.forestSemantic);
                forestSemantic.WindFilename = filename;
                forestSemantic.WindDirection = windDirection;
                forestSemantic.WindStrength = windSpeed;
            }
            foreach (IWorldObject tree in treeTypes)
            {
                tree.AddToScene();
            }
		}

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            foreach (IWorldObject child in treeTypes)
            {
                child.UpdateScene(type, hint);
            }
        }


		public void AddToTree(WorldTreeNode parentNode)
		{
			this.parentNode = parentNode;
			inTree = true;

			// create a node for the collection and add it to the parent
            node = app.MakeTreeNode(this, name);

			parentNode.Nodes.Add(node);
			Forest forest = this;
			CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
			menuBuilder.Add("Add Tree", new AddTreeCommandFactory(app, forest), app.DefaultCommandClickHandler);
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Forest", app.HelpClickHandler);
            menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
			node.ContextMenuStrip = menuBuilder.Menu;

			foreach (IWorldObject tree in treeTypes)
			{
				tree.AddToTree(node);
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
            Forest clone = new Forest(filename, windSpeed, windDirection, seed, copyParent as Boundary, app);
            foreach(IWorldObject child in treeTypes)
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
                string objString = String.Format("Name:{0}:{1}\r\n", ObjectType, name);
                objString +=  String.Format("\tSpeedWindFilename={0}\r\n",SpeedWindFilename);
                objString +=  String.Format("\tSeed=(0)\r\n",Seed);
                objString +=  String.Format("\tWindSpeed={0}\r\n",WindSpeed);
                objString +=  String.Format("\tWindDirection:{0}\r\n", windDirection);
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
            foreach (IWorldObject tree in treeTypes)
            {
                tree.RemoveFromTree();
            }

            parentNode.Nodes.Remove(node);
            parentNode = null;
            node = null;
        }

		public void RemoveFromScene()
		{
            foreach (IWorldObject tree in treeTypes)
            {
                tree.RemoveFromScene();
            }

            if (inScene)
            {
                if (this.forestSemantic != null && parent != null && parent.SceneBoundary != null)
                {
                    parent.SceneBoundary.RemoveSemantic(this.forestSemantic);
                }
                inScene = false;
            }
		}

        public void CheckAssets()
        {
            if (!app.CheckAssetFileExists(filename))
            {
                app.AddMissingAsset(filename);
            }
            foreach (IWorldObject child in treeTypes)
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

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
		public string ObjectType
		{
			get
			{
				return "Forest";
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
            w.WriteLine("SpeedWind:{0}", filename);
            if (treeTypes != null)
            {
                foreach (IWorldObject child in treeTypes)
                {
                    child.ToManifest(w);
                }
            }
        }

		public void Dispose()
		{
			RemoveFromScene();
		}


		#region ICollection<IWorldObject> Members

		public void Add(IWorldObject item)
		{
			treeTypes.Add(item);
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
			treeTypes.Clear();
		}

		public bool Contains(IWorldObject item)
		{
			return treeTypes.Contains(item);
		}

		public void CopyTo(IWorldObject[] array, int arrayIndex)
		{
			treeTypes.CopyTo(array, arrayIndex);
		}

        [BrowsableAttribute(false)]
		public int Count
		{
			get
			{
				return treeTypes.Count;
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

			return treeTypes.Remove(item);
		}

		#endregion

		#region IEnumerable<IWorldObject> Members

		public IEnumerator<IWorldObject> GetEnumerator()
		{
			return treeTypes.GetEnumerator();
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

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
