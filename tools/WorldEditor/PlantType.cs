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
using Axiom.Core;

namespace Multiverse.Tools.WorldEditor
{
	public class PlantType : IWorldObject, IObjectDelete
	{
		protected WorldEditor app;
		protected Grass parent;
		protected uint instances;
        protected string name;
		protected string imageName;
		protected float scaleWidthLow;
		protected float scaleWidthHi;
		protected float scaleHeightLow;
		protected float scaleHeightHi;
		protected ColorEx color;
		protected float colorMultLow;
		protected float colorMultHi;
		protected float windMagnitude;
		protected Axiom.SceneManagers.Multiverse.PlantType sceneType;
		protected bool inScene = false;
		protected bool inTree = false;
		protected WorldTreeNode node = null;
		protected WorldTreeNode parentNode = null;
        protected List<ToolStripButton> buttonBar;

		public PlantType(WorldEditor app, Grass parent, uint instances, string name, string imageName, float scaleWidthLow, float scaleWidthHi, float scaleHeightLow, float scaleHeightHi, ColorEx color, float colorMultLow, float colorMultHi, float windMagnitude)
		{
			this.app = app;
			this.parent = parent;
			this.instances = instances;
            this.name = name;
			this.imageName = imageName;
			this.scaleWidthLow = scaleWidthLow;
			this.scaleWidthHi = scaleWidthHi;
			this.scaleHeightLow = scaleHeightLow;
			this.scaleHeightHi = scaleHeightHi;
			this.color = color;
			this.colorMultLow = colorMultLow;
			this.colorMultHi = colorMultHi;
			this.windMagnitude = windMagnitude;
		}

        public PlantType(XmlReader r, Grass parent, WorldEditor app)
        {
            this.app = app;
            this.parent = parent;

            FromXml(r);
        }


        public void ToXml(XmlWriter w)
        {
            w.WriteStartElement("PlantType");
            w.WriteAttributeString("Instances", this.instances.ToString());
            w.WriteAttributeString("ColorMultHi", this.colorMultHi.ToString());
            w.WriteAttributeString("ColorMultLow", this.colorMultLow.ToString());
            w.WriteAttributeString("Name", this.name);
            w.WriteAttributeString("ImageName", this.imageName);
            w.WriteAttributeString("ScaleWidthLow", this.scaleWidthLow.ToString());
            w.WriteAttributeString("ScaleWidthHi", this.scaleWidthHi.ToString());
            w.WriteAttributeString("ScaleHeightLow", this.scaleHeightLow.ToString());
            w.WriteAttributeString("ScaleHeightHi", this.scaleHeightHi.ToString());
			w.WriteAttributeString("WindMagnitude", this.windMagnitude.ToString());
            w.WriteAttributeString("R", color.r.ToString());
            w.WriteAttributeString("G", color.g.ToString());
            w.WriteAttributeString("B", color.b.ToString());
            w.WriteEndElement();
        }

        protected void FromXml(XmlReader r)
        {
            float red = 0;
            float green = 0;
            float blue = 0;

            // first parse the attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Instances":
                        this.instances = uint.Parse(r.Value);
                        break;
                    case "ColorMultHi":
                        this.colorMultHi = float.Parse(r.Value);
                        break;
                    case "ColorMultLow":
                        this.colorMultLow = float.Parse(r.Value);
                        break;
                    case "Name":
                        this.name = r.Value;
                        break;
                    case "ImageName":
                        this.imageName = r.Value;
                        break;
                    case "ScaleWidthLow":
                        this.scaleWidthLow = float.Parse(r.Value);
                        break;
                    case "ScaleWidthHi":
                        this.scaleWidthHi = float.Parse(r.Value);
                        break;
                    case "ScaleHeightLow":
                        this.scaleHeightLow = float.Parse(r.Value);
                        break;
                    case "ScaleHeightHi":
                        this.scaleHeightHi = float.Parse(r.Value);
                        break;
                    case "WindMagnitude":
                        this.windMagnitude = float.Parse(r.Value);
                        break;
					case "WindMagnatude":
						this.windMagnitude = float.Parse(r.Value);
						break;
                    case "R":
                        red = float.Parse(r.Value);
                        break;
                    case "G":
                        green = float.Parse(r.Value);
                        break;
                    case "B":
                        blue = float.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.

            this.color = new ColorEx(red, green, blue);
        }

        [DescriptionAttribute("The name of this plant."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
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

        [TypeConverter(typeof(ImageNameUITypeEditor)), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true), DescriptionAttribute("Type of plant. Must be one of the PlantType assets in your asset repository.")]
		public string ImageName
		{
			get
			{
				return app.Assets.assetFromAssetName(imageName).Name;
			}
			set
			{
				imageName = app.Assets.assetFromName(value).AssetName;
				if (inScene)
				{
					sceneType.ImageName = imageName;
				}
			}
		}

        [CategoryAttribute("Scale"), DescriptionAttribute("Minimum width (in millimeters) of billboards. Billboards are created with a random width between the minimum and maximum width scale."), BrowsableAttribute(true)]
		public float ScaleWidthLow
		{
			get
			{
				return scaleWidthLow;
			}
			set
			{
				scaleWidthLow = value;
				if(inScene)
				{
					sceneType.ScaleWidthLow = value;
				}
			}
		}

        [CategoryAttribute("Scale"), DescriptionAttribute("Maximum width (in millimeters) of billboards. Billboards are created with a random width between the minimum and maximum width scale."), BrowsableAttribute(true)]
		public float ScaleWidthHi
		{
			get
			{
				return scaleWidthHi;
			}
			set
			{
				scaleWidthHi = value;
				if (inScene)
				{
					sceneType.ScaleWidthHi = value;
				}
			}
		}

        [CategoryAttribute("Scale"), DescriptionAttribute("Minimum height (in millimeters) of billboards. Billboards are created with a random height between minimum and maximum scale height."), BrowsableAttribute(true)]
		public float ScaleHeightLow
		{
			get
			{
				return scaleHeightLow;
			}
			set
			{
				scaleHeightLow = value;
				if (inScene)
				{
					sceneType.ScaleHeightLow = value;
				}
			}
		}

        [CategoryAttribute("Scale"), DescriptionAttribute("Maximum height (in millimeters) of billboards. Billboards are created with a random height between minimum and maximum scale height."), BrowsableAttribute(true)]
		public float ScaleHeightHi
		{
			get
			{
				return scaleHeightHi;
			}
			set
			{
				scaleHeightHi = value;
				if (inScene)
				{
					sceneType.ScaleHeightHi = value;
				}
			}
		}

		[EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("For colorized textures, the color to use.  For textures that already contain color, set to white.  This color is multiplied by the texture pixel when texturing the billboard. (click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Color")]
		public ColorEx Color
		{
			get
			{
				return color;
			}
			set
			{
				color = value;
				if (inScene)
				{
					sceneType.Color = value;
				}
			}
		}

        [BrowsableAttribute(true), CategoryAttribute("Color"), DescriptionAttribute("A random value between the low value and high value is multiplied by the texture value when the billboard is being drawn.  This is used to make patches of grass with instances that are slightly lighter and darker.")]
		public float ColorMultLow
		{
			get
			{
				return colorMultLow;
			}
			set
			{
				colorMultLow = value;
				if (inScene)
				{
					sceneType.ColorMultLow = value;
				}
			}
		}

        [BrowsableAttribute(true), CategoryAttribute("Color"), DescriptionAttribute("A random value between the low value and high value is multiplied by the texture value when the billboard is being drawn.  This is used to make patches of grass with instances that are slightly lighter and darker.")]
		public float ColorMultHi
		{
			get
			{
				return colorMultLow;
			}
			set
			{
				colorMultLow = value;
				if (inScene)
				{
					sceneType.ColorMultLow = value;
				}
			}
		}

        [BrowsableAttribute(true), CategoryAttribute("Miscellaneous"), DescriptionAttribute("Maximum displacement (in millimeters) of the tops of the grass billboards due to wind.")]
		public float WindMagnitude
		{
			get
			{
				return windMagnitude;
			}
			set
			{
				windMagnitude = value;
				if (inScene)
				{
					sceneType.WindMagnitude = value;
				}
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
                return "Plant";
            }
        }

        [DescriptionAttribute("Number of instances of this plant type in a grass tile (a square 65 meters on a side)"), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
        public uint Instances
        {
            get
            {
                return instances;
            }
            set
            {
                instances = value;
                if (inScene)
                {
                    sceneType.NumInstances = value;
                }
            }
        }

        #region IWorldObject Members
        public void AddToTree(WorldTreeNode parentNode)
		{
			this.parentNode = parentNode;
			inTree = true;

			// create a node for the collection and add it to the parent
            node = app.MakeTreeNode(this, NodeName);

			parentNode.Nodes.Add(node);
			PlantType plantType = this;

            // build the menu
            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Plant_Type", app.HelpClickHandler);
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
            PlantType clone = new PlantType(app, copyParent as Grass, instances, name,
                imageName, scaleWidthLow, scaleWidthHi, scaleHeightLow, ScaleHeightHi, color, colorMultLow,
                colorMultHi, windMagnitude);
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", NodeName);
                objString +=  String.Format("\tImageName={0}\r\n", ImageName);
                objString +=  String.Format("\tScaleWidthLow={0}\r\n", ScaleWidthLow);
                objString +=  String.Format("\tScaleWidthHi={0}\r\n", ScaleWidthHi);
                objString +=  String.Format("\tScaleHeightLow={0}\r\n", ScaleHeightLow);
                objString +=  String.Format("\tScaleHeightHi={0}\r\n", ScaleHeightHi);
                objString +=  String.Format("\tColor:\r\n");
                objString +=  String.Format("\t\tR={0}\r\n", color.r);
                objString +=  String.Format("\t\tG={0}\r\n", color.g);
                objString +=  String.Format("\t\tB={0}\r\n", color.b);
                objString +=  String.Format("\tColorMultLow={0}\r\n", ColorMultLow);
                objString +=  String.Format("\tColorMultHi={0}\r\n", ColorMultHi);
                objString +=  String.Format("\tWindMagnitude={0}\r\n", WindMagnitude);
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
            inTree = false;
		}

		public void AddToScene()
		{
			this.sceneType = new Axiom.SceneManagers.Multiverse.PlantType(instances,imageName, scaleWidthLow,scaleWidthHi, scaleHeightLow, scaleHeightHi, color, colorMultLow, colorMultHi, windMagnitude);
			this.parent.VegieSemantic.AddPlantType(this.sceneType);
            inScene = true;
		}

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
        }

		public void RemoveFromScene()
		{
            inScene = false;
			this.parent.VegieSemantic.RemovePlantType(this.sceneType);
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
            w.WriteLine("PlantType:{0}", imageName);
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
