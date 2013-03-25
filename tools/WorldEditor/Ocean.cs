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
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    public class Ocean : IWorldObject
    {
        protected IWorldContainer parent;
        protected WorldEditor app;

        protected float seaLevel;
        protected bool displayOcean;
        protected float waveHeight;
        protected ColorEx deepColor;
        protected ColorEx shallowColor;
        protected float bumpScale;
        protected float textureScaleX;
        protected float textureScaleZ;
        protected float bumpSpeedX;
        protected float bumpSpeedZ;
        protected bool useParams = true;

        protected WorldTreeNode node;
        protected WorldTreeNode parentNode;

        protected bool inScene = false;

        protected List<ToolStripButton> buttonBar;

        public Ocean(IWorldContainer parentContainer, WorldEditor worldEditor)
        {
            this.parent = parentContainer;
            this.app = worldEditor;

            // set up default parameters
            this.displayOcean = app.Config.DefaultDisplayOcean;
            this.seaLevel = app.Config.DefaultSeaLevel;
            this.waveHeight = app.Config.DefaultWaveHeight;
            this.deepColor = app.Config.DefaultOceanDeepColor;
            this.shallowColor = app.Config.DefaultOceanShallowColor;
            this.bumpScale = app.Config.DefaultOceanBumpScale;
            this.textureScaleX = app.Config.DefaultOceanTextureScaleX;
            this.textureScaleZ = app.Config.DefaultOceanTextureScaleZ;
            this.bumpSpeedX = app.Config.DefaultOceanBumpSpeedX;
            this.bumpSpeedZ = app.Config.DefaultOceanBumpSpeedZ;
            this.useParams = app.Config.DefaultOceanUseParams;
        }

        public Ocean(IWorldContainer parentContainer, WorldEditor worldEditor, XmlReader r) :
            this(parentContainer, worldEditor)
        {
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
                    case "DisplayOcean":
                        displayOcean = (r.Value == "True");
                        break;
                    case "UseParams":
                        useParams = (r.Value == "True");
                        break;
                    case "WaveHeight":
                        waveHeight = float.Parse(r.Value);
                        break;
                    case "SeaLevel":
                        seaLevel = float.Parse(r.Value);
                        break;
                    case "BumpScale":
                        bumpScale = float.Parse(r.Value);
                        break;
                    case "BumpSpeedX":
                        bumpSpeedX = float.Parse(r.Value);
                        break;
                    case "BumpSpeedZ":
                        bumpSpeedZ = float.Parse(r.Value);
                        break;
                    case "TextureScaleX":
                        textureScaleX = float.Parse(r.Value);
                        break;
                    case "TextureScaleZ":
                        textureScaleZ = float.Parse(r.Value);
                        break;
                    default:
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.

            if (!r.IsEmptyElement)
            {
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
                            case "DeepColor":
                                deepColor = XmlHelperClass.ParseColorAttributes(r);
                                break;
                            case "ShallowColor":
                                shallowColor = XmlHelperClass.ParseColorAttributes(r);
                                break;
                        }
                    }
                    else if (r.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                }
            }
            if (!displayOcean)
            {
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShowOcean = displayOcean;
            }
        }

        [DescriptionAttribute("Whether to display the automatically-generated ocean."), CategoryAttribute("Ocean Properties")]
        public bool DisplayOcean
        {
            get
            {
                return displayOcean;
            }
            set
            {
                displayOcean = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShowOcean = displayOcean;
            }
        }

        [DescriptionAttribute("Set this property to false if you are using your own ocean material that uses different vertex and pixel shader parameters than the default ocean shaders provided by Multiverse."), CategoryAttribute("Ocean Properties")]
        public bool UseParams
        {
            get
            {
                return useParams;
            }
            set
            {
                useParams = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.OceanConfig.UseParams = useParams;
            }
        }

        [DescriptionAttribute("Amplitude of the ocean waves (in millimeters)."), CategoryAttribute("Ocean Properties")]
        public float WaveHeight
        {
            get
            {
                return waveHeight;
            }
            set
            {
                waveHeight = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.OceanConfig.WaveHeight = waveHeight;
            }
        }

        [DescriptionAttribute("Average level of the ocean (in millimeters)."), CategoryAttribute("Ocean Properties")]
        public float SeaLevel
        {
            get
            {
                return seaLevel;
            }
            set
            {
                seaLevel = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.OceanConfig.SeaLevel = seaLevel;
            }
        }

        [EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)),
DescriptionAttribute("Predominant color when looking directly into the water. Used to simulate a deep water effect. (click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Ocean Color Properties")]
        public ColorEx DeepColor
        {
            get
            {
                return deepColor;
            }
            set
            {
                deepColor = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.OceanConfig.DeepColor = deepColor;
            }
        }

        [EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)),
DescriptionAttribute("Predominant color when looking at the water from an acute angle. Used to simulate a shallow water effect.(click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Ocean Color Properties")]
        public ColorEx ShallowColor
        {
            get
            {
                return shallowColor;
            }
            set
            {
                shallowColor = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.OceanConfig.ShallowColor = shallowColor;
            }
        }

        [DescriptionAttribute("Scale factor for the normal mapping effect used to simulate smaller waves. Larger values make the waves look taller."), CategoryAttribute("Ocean Detail Properties")]
        public float BumpScale
        {
            get
            {
                return bumpScale;
            }
            set
            {
                bumpScale = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.OceanConfig.BumpScale = bumpScale;
            }
        }

        [DescriptionAttribute("Scale factor for the speed of the small wave bump effect, along the X axis. Larget values make the waves move faster in the X direction."), CategoryAttribute("Ocean Detail Properties")]
        public float BumpSpeedX
        {
            get
            {
                return bumpSpeedX;
            }
            set
            {
                bumpSpeedX = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.OceanConfig.BumpSpeedX = bumpSpeedX;
            }
        }
        [DescriptionAttribute("Scale factor for the speed of the small wave bump effect, along the Z axis. ILarget values make the waves move faster in the Z direction."), CategoryAttribute("Ocean Detail Properties")]
        public float BumpSpeedZ
        {
            get
            {
                return bumpSpeedZ;
            }
            set
            {
                bumpSpeedZ = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.OceanConfig.BumpSpeedZ = bumpSpeedZ;
            }
        }
        [DescriptionAttribute("Scale factor for the normal map texture used to generate the small wave bump effect, along the X axis."), CategoryAttribute("Ocean Detail Properties")]
        public float TextureScaleX
        {
            get
            {
                return textureScaleX;
            }
            set
            {
                textureScaleX = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.OceanConfig.TextureScaleX = textureScaleX;
            }
        }

        [DescriptionAttribute("Scale factor for the normal map texture used to generate the small wave bump effect, along the Z axis."), CategoryAttribute("Ocean Detail Properties")]
        public float TextureScaleZ
        {
            get
            {
                return textureScaleZ;
            }
            set
            {
                textureScaleZ = value;
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.OceanConfig.TextureScaleZ = textureScaleZ;
            }
        }

        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;

            // add the ocean node
            node = app.MakeTreeNode(this, "Ocean");
            parentNode.Nodes.Add(node);

            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Ocean", app.HelpClickHandler);

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
                // This property is not relevent for this object.
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
        //    Ocean oc = new Ocean(copyParent, app);
        //    oc.BumpScale = this.bumpScale;
        //    oc.BumpSpeedX = this.bumpSpeedX;
        //    oc.BumpSpeedZ = this.bumpSpeedZ;
        //    oc.DisplayOcean = this.DisplayOcean;
        //    oc.SeaLevel = this.seaLevel;
        //    oc.ShallowColor = this.shallowColor;
        //    oc.TextureScaleX = this.textureScaleX;
        //    oc.TextureScaleZ = this.TextureScaleZ;
        //    oc.UseParams = this.useParams;
        //    oc.WaveHeight = this.waveHeight;
        }



        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString +=  String.Format("\tDisplayOcean={0}\r\n",DisplayOcean);
                objString +=  String.Format("\tUseParams={0}\r\n",UseParams);
                objString +=  String.Format("\tWaveHeight={0}\r\n",WaveHeight);
                objString +=  String.Format("\t=SeaLevel={0}\r\n",SeaLevel);
                objString +=  String.Format("\t=DeepColor:\r\n");
                objString +=  String.Format("\t\tR={0}\r\n",DeepColor.r);
                objString +=  String.Format("\t\tG={0}\r\n",DeepColor.g);
                objString +=  String.Format("\t\tB={0}\r\n",DeepColor.b);
                objString +=  String.Format("\tShallowColor:\r\n");
                objString +=  String.Format("\t\tR={0}\r\n",ShallowColor.r);
                objString +=  String.Format("\t\tG={0}\r\n",ShallowColor.g);
                objString +=  String.Format("\t\tB={0}\r\n",ShallowColor.b);
                objString +=  String.Format("\tBumpScale={0}\r\n",BumpScale);
                objString +=  String.Format("\tBumpSpeedX={0}\r\n",BumpSpeedX);
                objString +=  String.Format("\tBumpSpeedZ={0}\r\n",BumpSpeedZ);
                objString +=  String.Format("\tTextureScaleX={0}\r\n",TextureScaleX);
                objString +=  String.Format("\tDisplayOcean={0}\r\n",DisplayOcean);
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
            inScene = true;
            Axiom.SceneManagers.Multiverse.TerrainManager.Instance.ShowOcean = displayOcean;
            Axiom.SceneManagers.Multiverse.OceanConfig oceanConfig = Axiom.SceneManagers.Multiverse.TerrainManager.Instance.OceanConfig;
            oceanConfig.UseParams = useParams;
            oceanConfig.WaveHeight = waveHeight;
            oceanConfig.SeaLevel = seaLevel;
            oceanConfig.DeepColor = deepColor;
            oceanConfig.ShallowColor = shallowColor;
            oceanConfig.BumpScale = bumpScale;
            oceanConfig.BumpSpeedX = bumpSpeedX;
            oceanConfig.BumpSpeedZ = bumpSpeedZ;
            oceanConfig.TextureScaleX = textureScaleX;
            oceanConfig.TextureScaleZ = textureScaleZ;
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
        }

        public void RemoveFromScene()
        {
            inScene = false;
            // dont do anything here
        }

        public void CheckAssets()
        {
        }

        public void ToXml(System.Xml.XmlWriter w)
        {
            w.WriteStartElement("Ocean");
            w.WriteAttributeString("DisplayOcean", displayOcean.ToString());
            w.WriteAttributeString("UseParams", useParams.ToString());
            w.WriteAttributeString("WaveHeight", waveHeight.ToString());
            w.WriteAttributeString("SeaLevel", seaLevel.ToString());
            w.WriteAttributeString("BumpScale", bumpScale.ToString());
            w.WriteAttributeString("BumpSpeedX", bumpSpeedX.ToString());
            w.WriteAttributeString("BumpSpeedZ", bumpSpeedZ.ToString());
            w.WriteAttributeString("TextureScaleX", textureScaleX.ToString());
            w.WriteAttributeString("TextureScaleZ", textureScaleZ.ToString());
            w.WriteStartElement("DeepColor");
            w.WriteAttributeString("R", deepColor.r.ToString());
            w.WriteAttributeString("G", deepColor.g.ToString());
            w.WriteAttributeString("B", deepColor.b.ToString());
            w.WriteEndElement(); // DeepColor
            w.WriteStartElement("ShallowColor");
            w.WriteAttributeString("R", shallowColor.r.ToString());
            w.WriteAttributeString("G", shallowColor.g.ToString());
            w.WriteAttributeString("B", shallowColor.b.ToString());
            w.WriteEndElement(); // ShallowColor
            w.WriteEndElement(); // Ocean
        }

        [BrowsableAttribute(false)]
        public Axiom.MathLib.Vector3 FocusLocation
        {
            get
            {
                return Vector3.Zero;
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
                return "Ocean";
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
