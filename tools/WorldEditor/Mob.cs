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
using Axiom.MathLib;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
	class SpawnGen : IWorldObject, IObjectDelete
	{
		protected WorldEditor app;
		protected IWorldContainer parent;
		protected int respawnTime;
		protected uint numSpawn;
		protected string templateName;
		protected float spawnRadius;
        protected NameValueObject nameValuePairs = new NameValueObject();
		
		protected WorldTreeNode node = null;
		protected WorldTreeNode parentNode = null;
		
		protected bool inTree = false;
		protected bool inScene = false;
        protected List<ToolStripButton> buttonBar;

		public SpawnGen(WorldEditor appin, IWorldContainer parentin, int respawnTimein, uint numberOfSpawnsin, string templateNamein)
		{
			this.app = appin;
			this.parent = parentin;
			this.respawnTime = respawnTimein;
			this.numSpawn = numberOfSpawnsin;
			this.templateName = templateNamein;
            this.nameValuePairs = new NameValueObject();
		}

		public SpawnGen(WorldEditor appin, IWorldContainer parentin, int respawnTimein, uint numberOfSpawnsin, string templateNamein, float spawnRadiusin)
		{
			this.app = appin;
			this.parent = parentin;
			this.respawnTime = respawnTimein;
			this.numSpawn = numberOfSpawnsin;
			this.templateName = templateNamein;
			this.spawnRadius = spawnRadiusin;
            this.nameValuePairs = new NameValueObject();
		}

        public SpawnGen(XmlReader r, WorldEditor appin, IWorldContainer parentin)
		{
			this.parent = parentin;
			this.app = appin;
			FromXml(r);
        }

        [DescriptionAttribute("The name of the server mob template to use."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
		public String TemplateName
		{
			get
			{
				return templateName;
			}
			set
			{
				templateName = value;
			}
		}

        [DescriptionAttribute("How long to wait (in milliseconds) to re-spawn a mob after it is killed."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
		public int RespawnTime
		{
			get
			{
				return respawnTime;
			}
			set
			{
				respawnTime = value;
			}
		}

        [DescriptionAttribute("Number of mobs to spawn."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
		public uint NumSpawn
		{
			get
			{
				return numSpawn;
			}
			set
			{
				numSpawn = value;
			}
		}

        [DescriptionAttribute("Only applies to spawn markers (not regions). Distance (in mm) within which mobs are spawned."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
		public float SpawnRadius
		{
			get
			{
				return spawnRadius;
			}
			set
			{
				spawnRadius = value;
			}
		}


		#region IWorldObject Members

		public void AddToTree(WorldTreeNode parentNode)
		{
			this.parentNode = parentNode;
			inTree = true;

			// create a node for the collection and add it to the parent
            this.node = app.MakeTreeNode(this, "Spawn Generator");

			parentNode.Nodes.Add(node);

			// build the menu
			CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Spawn_Generator", app.HelpClickHandler);
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
            SpawnGen clone = new SpawnGen(app, copyParent, respawnTime, numSpawn, templateName, spawnRadius);
            clone.NameValue = new NameValueObject(this.NameValue);
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n",ObjectType);
                objString +=  String.Format("\tTemplateName={0}\r\n",TemplateName);
                objString +=  String.Format("\tRespawnTime={0}\r\n", RespawnTime);
                objString +=  String.Format("\tNumSpawn={0}\r\n", NumSpawn);
                objString +=  String.Format("\tSpawnRadius={0}\r\n", SpawnRadius);
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
            if (node != null && inTree)
            {
                if (node.IsSelected)
                {
                    node.UnSelect();
                }
                parentNode.Nodes.Remove(node);
                node = null;
                parentNode = null;

                inTree = false;
            }
		}

		public void AddToScene()
		{
			return;
		}

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
        }

		public void RemoveFromScene()
		{
			return;
		}

        public void CheckAssets()
        {
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

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
		public string ObjectType
		{
			get
			{
				return "SpawnGenerator";
			}
		}

		public void ToXml(XmlWriter w)
		{
			w.WriteStartElement("SpawnGen");
			w.WriteAttributeString("TemplateName", TemplateName);
			w.WriteAttributeString("RespawnTime", RespawnTime.ToString());
			w.WriteAttributeString("NumSpawns", NumSpawn.ToString());

            // Server will ignore SpawnRadius
		    w.WriteAttributeString("SpawnRadius", SpawnRadius.ToString());

            if (this.nameValuePairs != null && this.nameValuePairs.Count > 0)
            {
                nameValuePairs.ToXml(w);
            }
			w.WriteEndElement();
		}

		public void FromXml(XmlReader r)
		{
		    // first parse the attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                // set the field in this object based on the element we just read
                {
                    switch (r.Name)
                    {
                        case "TemplateName":
                            this.templateName = r.Value;
                            break;
                        case "RespawnTime":
                            this.respawnTime = int.Parse(r.Value);
                            break;
                        case "NumSpawns":
                            this.numSpawn = uint.Parse(r.Value);
                            break;
                        case "SpawnRadius":
                            this.spawnRadius = float.Parse(r.Value);
                            break;
                    }

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
                    if (r.NodeType == XmlNodeType.Whitespace)
                    {
                        continue;
                    }
                    if (r.NodeType == XmlNodeType.Element)
                    {
                        switch (r.Name)
                        {
                            case "NameValuePairs":
                                this.nameValuePairs = new NameValueObject(r);
                                break;
                        }
                    }
                }
            }
        }

		[BrowsableAttribute(false)]
		public Axiom.MathLib.Vector3 FocusLocation
		{
			get
			{
                IWorldObject parentObj = parent as IWorldObject;
                if (parentObj != null)
                {
                    return parentObj.FocusLocation;
                }

                return new Vector3(0, 0, 0);
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
                ;
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

        [EditorAttribute(typeof(NameValueUITypeEditorMob), typeof(System.Drawing.Design.UITypeEditor)),
      DescriptionAttribute("Arbitrary Name/Value pair used to pass information about an object to server scripts and plug-ins. Click [...] to add or edit name/value pairs."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
        public NameValueObject NameValue
        {
            get
            {
                return nameValuePairs;
            }
            set
            {
                nameValuePairs = value;
            }
        }
        public void ToManifest(System.IO.StreamWriter w)
        {
        }
        #endregion IWorldObject Members



		#region IDisposable Members

		public void Dispose()
		{
			throw new Exception("The method or operation is not implemented.");
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

    }
}
