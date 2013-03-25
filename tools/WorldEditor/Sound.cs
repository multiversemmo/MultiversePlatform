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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml;
using Axiom.MathLib;


namespace Multiverse.Tools.WorldEditor
{
	public class Sound : IWorldObject , IObjectDelete
	{
        public enum SoundType { Ambient, Positional };
        protected SoundType type;
		protected string filename;
        private IWorldContainer parent; 
		private WorldEditor app;
		protected WorldTreeNode node = null;
		protected WorldTreeNode parentNode = null;
        protected bool inTree = false;
        protected List<ToolStripButton> buttonBar;
        protected float minAttenuationDistance;
        protected float maxAttenuationDistance;
        protected float gain;
        protected bool loop;

		public Sound(string filenamein, IWorldContainer parent, WorldEditor app)
		{
			this.filename = filenamein;
			this.parent = parent;
			this.app = app;
            this.loop = app.Config.DefaultSoundLooping;
            this.gain = app.Config.DefaultSoundGain;

            if (parent is Boundary)
            {
                this.type = SoundType.Ambient;
            }
            else
            {
                this.type = SoundType.Positional;
                minAttenuationDistance = app.Config.DefaultSoundMinAttenuationDistance;
                maxAttenuationDistance = app.Config.DefaultSoundMaxAttenuationDistance;
            }
		}

		public void ToXml(XmlWriter w)
		{
			w.WriteStartElement("Sound");
			w.WriteAttributeString("Filename",this.filename);
            w.WriteAttributeString("Type", type.ToString());
            w.WriteAttributeString("Gain", gain.ToString());
            w.WriteAttributeString("Loop", loop.ToString());
            if (type == SoundType.Positional)
            {
                w.WriteAttributeString("MinAttenuationDistance", minAttenuationDistance.ToString());
                w.WriteAttributeString("MaxAttenuationDistance", maxAttenuationDistance.ToString());
            }
			w.WriteEndElement(); //end Sound
			return;
		}

        public Sound(XmlReader r, IWorldContainer parent, WorldEditor worldEditor)
        {
            this.parent = parent;
            this.app = worldEditor;

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
                    case "Filename":
                        this.filename = r.Value;
                        break;
                    case "Type":
                        this.type = (SoundType)Enum.Parse(type.GetType(), r.Value);
                        break;
                    case "Gain":
                        this.gain = float.Parse(r.Value);
                        break;
                    case "Loop":
                        this.loop = bool.Parse(r.Value);
                        break;
                    case "MinAttenuationDistance":
                        this.minAttenuationDistance = float.Parse(r.Value);
                        break;
                    case "MaxAttenuationDistance":
                        this.maxAttenuationDistance = float.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.
            if (type == SoundType.Ambient)
            {
                this.minAttenuationDistance = 0f;
                this.maxAttenuationDistance = 0f;
            }
        }

		#region IWorldObject Members

        public void Clone(IWorldContainer copyParent)
        {
            Sound clone = new Sound(filename, copyParent, app);
            clone.Gain = gain;
            clone.MaxAttenuationDistance = maxAttenuationDistance;
            clone.MinAttenuationDistance = minAttenuationDistance;
            clone.Loop = loop;
            copyParent.Add(clone);
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
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString += String.Format("\tFilename={0}\r\n", File);
                objString += "\r\n";
                return objString;
            }
        }

        [DescriptionAttribute("Whether the sound is an ambient or positional sound."), BrowsableAttribute(true), CategoryAttribute("Sound Properties")]
        public SoundType Type
        {
            get
            {
                return type;
            }
        }

        [DescriptionAttribute("Base sound level (between 0 and 1)."), BrowsableAttribute(true), CategoryAttribute("Sound Properties")]
        public float Gain
        {
            get
            {
                return gain;
            }
            set
            {
                float original = gain;
                if (0 <= value && 1 >= value)
                {
                    gain = value;
                }
                else
                {
                    gain = original;
                }
            }
        }

        [DescriptionAttribute("Distance at which volume of a point sound starts to diminish"), CategoryAttribute("Attenuation"), BrowsableAttribute(true)]
        public float MinAttenuationDistance
        {
            get
            {
                if (type == SoundType.Positional)
                {
                    return minAttenuationDistance;
                }
                else
                {
                    return 0f;
                }
            }
            set
            {
                if (type == SoundType.Positional)
                {
                    minAttenuationDistance = value;
                }
                else
                {
                    minAttenuationDistance = 0f;
                }
            }
        }

        [DescriptionAttribute("Distance at which a point sound can no longer be heard."), BrowsableAttribute(true), CategoryAttribute("Attenuation")]
        public float MaxAttenuationDistance
        {
            get
            {
                if (type == SoundType.Positional)
                {
                    return maxAttenuationDistance;
                }
                else
                {
                    return 0f;
                }
            }
            set
            {
                if (type == SoundType.Positional)
                {
                    maxAttenuationDistance = value;
                }
                else
                {
                    maxAttenuationDistance = 0f;
                }
            }
        }

        [BrowsableAttribute(true), CategoryAttribute("Sound Properties"), DescriptionAttribute("Whether to loop the sound.")]
        public bool Loop
        {
            get
            {
                return loop;
            }
            set
            {
                loop = value;
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
            if (!String.Equals("",filename) && !app.CheckAssetFileExists(filename))
            {
                app.AddMissingAsset(filename);
            }
        }

		[BrowsableAttribute(false)]
		public Vector3 FocusLocation
		{
			get
			{
                return ((IWorldObject)parent).FocusLocation;
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
		}

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
		public string ObjectType
		{
			get
			{
				return "Sound";
			}
		}

        [TypeConverter(typeof(SoundAssetListConverter)),
       DescriptionAttribute("Name of the sound file to play."), CategoryAttribute("Sound Properties")]
		public string File
		{
			get
			{
				AssetDesc asset = app.Assets.assetFromAssetName(this.filename);
				return asset.Name;
			}
			set
			{
                bool found = false;                
                foreach (AssetDesc asset in  app.Assets.Select("Sound"))
                {
                    if (string.Equals(asset.Name, value))
                    {
                        found = true;
                    }
                }
                if (found)
                {
                    this.filename = app.Assets.assetFromName(value).AssetName;
                }
                else
                {
                    MessageBox.Show("Invalid entry for Sound", "Invalid Entry", MessageBoxButtons.OK);
                    this.filename = "";
                }
			}
		}


		public void AddToTree(WorldTreeNode parentNode)
		{
			this.parentNode = parentNode;
            if (inTree == false)
            {
                inTree = true;
            }

			// create a node for the collection and add it to the parent
            node = app.MakeTreeNode(this, "Sound");
			parentNode.Nodes.Add(node);

            // build the menu
            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Sound", app.HelpClickHandler);
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

        public void ToManifest(System.IO.StreamWriter w)
        {
            w.WriteLine("Sound:{0}", filename);
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
            RemoveFromScene();
        }

        #endregion
	}
}
