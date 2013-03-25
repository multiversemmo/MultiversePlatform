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
	public class Tree : IWorldObject, IObjectDelete
	{
		protected string name;
		protected string descriptionFilename;
		protected float scale;
		protected float scaleVariance;
		protected uint instances;
		protected WorldEditor app;
		protected Forest parent;
		protected Axiom.SceneManagers.Multiverse.TreeType sceneTree;
		protected bool inScene = false;
		protected bool inTree = false;
		protected WorldTreeNode node = null;
		protected WorldTreeNode parentNode = null;
        protected List<ToolStripButton> buttonBar;

		public Tree(string name, string descriptionFilename, float scale, float scalevariance, uint instances, Forest parent, WorldEditor app)
		{
			this.app = app;
			this.name = name;
			this.parent = parent;
			this.descriptionFilename = descriptionFilename;
			this.scale = scale;
			this.scaleVariance = scalevariance;
			this.instances = instances;
		}

        [BrowsableAttribute(false)]
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
                if (inTree)
                {
                    node.Text = name;
                }
			}
		}


        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
		public string ObjectType
		{
			get
			{
				return "Tree";
			}
		}

        [TypeConverter(typeof(TreeDescriptionFilenameUITypeEditor)), CategoryAttribute("Miscellaneous"), DescriptionAttribute("Name of the selected SpeedTree file.")]
		public string TreeDescriptionFilename
		{
			get
			{
				return app.Assets.assetFromAssetName(descriptionFilename).Name;
			}
			set
			{
				descriptionFilename = app.Assets.assetFromName(value).AssetName;
				if (inScene)
				{
					this.sceneTree.filename = descriptionFilename;
					parent.ForestSemantic.UpdateTreeType();
				}
			}
		}

        [BrowsableAttribute(true), CategoryAttribute("Miscellaneous"), DescriptionAttribute("Base size of the trees.")]
		public float Scale
		{
			get
			{
				return this.scale;
			}
			set
			{
				this.scale = value;
                if (inScene)
                {
                    sceneTree.size = value;
                    parent.ForestSemantic.UpdateTreeType();
                }
			}
		}

        [BrowsableAttribute(true), CategoryAttribute("Miscellaneous"), DescriptionAttribute("Variance of tree size from the base scale (mm). Tree sizes vary randomly between (Scale - Variance) to (Scale + Variance).")]
		public float ScaleVariance
		{
			get
			{
				return this.scaleVariance;
			}
			set
			{
				this.scaleVariance = value;
                if (inScene)
                {
                    sceneTree.sizeVariance = value;
                    parent.ForestSemantic.UpdateTreeType();
                }
			}
		}

        [BrowsableAttribute(true), CategoryAttribute("Miscellaneous"), DescriptionAttribute("Number of trees of this type in the forest.")]
		public uint Instances
		{
			get
			{
				return this.instances;
			}
			set
			{
				this.instances = value;
                if (inScene)
                {
                    sceneTree.numInstances = value;
                    parent.ForestSemantic.UpdateTreeType();
                }
			}
		}

        public void ToXml(XmlWriter w)
        {
			w.WriteStartElement("Tree");
            w.WriteAttributeString("Name", name);
			w.WriteAttributeString("Filename", descriptionFilename);
			w.WriteAttributeString("Scale", scale.ToString());
			w.WriteAttributeString("ScaleVariance", scaleVariance.ToString());
			w.WriteAttributeString("Instances", instances.ToString());
			w.WriteEndElement(); // end Tree
        }

		public Tree(XmlReader r, Forest parent, WorldEditor worldEditor)
		{
            this.app = worldEditor;
            this.parent = parent;

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
                    case "Name":
                        this.name = r.Value;
                        break;
                    case "Filename":
                        this.descriptionFilename = r.Value;
                        break;
                    case "Scale":
                        this.scale = float.Parse(r.Value);
                        break;
                    case "ScaleVariance":
                        this.scaleVariance = float.Parse(r.Value);
                        break;
                    case "Instances":
                        this.instances = uint.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.
        }

		public void AddToScene()
		{
            if (!inScene)
            {
                inScene = true;

                sceneTree = new Axiom.SceneManagers.Multiverse.TreeType(descriptionFilename, scale, scaleVariance, instances);
                this.parent.ForestSemantic.AddTreeType(sceneTree);
            }
		}

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
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
                // This property is not relevent for this object.
            }
        }


		public void RemoveFromScene()
		{
            if (inScene)
            {
                if (this.parent.ForestSemantic != null && this.sceneTree != null)
                {
                    this.parent.ForestSemantic.DeleteTreeType(this.sceneTree);
                }
            }
            inScene = false;
		}


		public void AddToTree(WorldTreeNode parentNode)
		{
			this.parentNode = parentNode;
            if (!inTree)
            {

                // create a node for the collection and add it to the parent
                this.node = app.MakeTreeNode(this, name);

                parentNode.Nodes.Add(node);

                // build the menu
                CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
                menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
                menuBuilder.Add("Help", "Tree_Type", app.HelpClickHandler);
                menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                node.ContextMenuStrip = menuBuilder.Menu;
                buttonBar = menuBuilder.ButtonBar;
                inTree = true;
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

        public void Clone(IWorldContainer copyParent)
        {
            Tree clone = new Tree(name, descriptionFilename, scale, scaleVariance, instances, copyParent as Forest, app);
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}: {1}\r\n", ObjectType, Name);
                objString += String.Format("\tTreeDescriptionFilename={0}\r\n",TreeDescriptionFilename);
                objString += String.Format("\tScale={0}\r\n",Scale);
                objString += String.Format("\tScaleVariance={0}\r\n",ScaleVariance);
                objString += String.Format("\tInstances={0}\r\n",Instances);
                objString += "\r\n";
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
            if (node != null && inTree)
            {
                if (node.IsSelected)
                {
                    node.UnSelect();
                }
                parentNode.Nodes.Remove(node);
                parentNode = null;
                node = null;
            }
            inTree = false;
        }

        public void CheckAssets()
        {
            bool missing = false;
            if (!app.CheckAssetFileExists(descriptionFilename))
            {
                int extOff = descriptionFilename.LastIndexOf('.');
                string treName = string.Format("{0}.tre", descriptionFilename.Substring(0, extOff));
                if (descriptionFilename != treName)
                {
                    if (!app.CheckAssetFileExists(treName))
                    {
                        missing = true;
                    }
                }
                else
                {
                    missing = true;
                }
            }
            if (missing)
            {
                app.AddMissingAsset(descriptionFilename);
            }
        }

        [BrowsableAttribute(false)]
		public Vector3 FocusLocation
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

        public void ToManifest(System.IO.StreamWriter w)
        {
            int extIndex = descriptionFilename.LastIndexOf('.');
            string basename = descriptionFilename.Substring(0, extIndex);
            w.WriteLine("SpeedTree:{0}", basename);
        }

		public void Dispose()
		{
			RemoveFromScene();
		}
	}
}
