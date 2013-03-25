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
using Axiom.SceneManagers.Multiverse;

namespace Multiverse.Tools.WorldEditor
{
    public class AlphaSplatTerrainDisplay : IWorldObject
    {
        protected WorldTerrain parent;
        protected WorldEditor app;

        protected WorldTreeNode node;
        protected WorldTreeNode parentNode;

        protected float textureTileSize = 5;

        protected bool useParams = true;

        protected string alpha0MosaicName;
        protected string alpha1MosaicName;

        protected string l1TextureName = "";
        protected string l2TextureName = "";
        protected string l3TextureName = "";
        protected string l4TextureName = "";
        protected string l5TextureName = "";
        protected string l6TextureName = "";
        protected string l7TextureName = "";
        protected string l8TextureName = "";
        protected string detailTextureName = "";

        protected bool inScene = false;
        protected bool inTree = false;

        protected AlphaSplatTerrainConfig terrainConfig;
        protected List<ToolStripButton> buttonBar;

        public AlphaSplatTerrainDisplay(WorldTerrain parent, WorldEditor worldEditor)
        {
            this.parent = parent;
            this.app = worldEditor;
        }

        public AlphaSplatTerrainDisplay(WorldTerrain parent, WorldEditor worldEditor, XmlReader r)
        {
            this.parent = parent;
            this.app = worldEditor;

            FromXml(r);
        }

        [DescriptionAttribute("Set this property to false if you are using your own terrain material that uses different vertex and pixel shader parameters than the default terrain shaders provided by Multiverse."), CategoryAttribute("Display Parameters")]
        public bool UseParams
        {
            get
            {
                return useParams;
            }
            set
            {
                useParams = value;
                if (inScene)
                {
                    terrainConfig.UseParams = useParams;
                }
            }
        }

        [DescriptionAttribute("Area (in meters) covered by one terrain texture tile."), CategoryAttribute("Display Parameters")]
        public float TextureTileSize
        {
            get
            {
                return textureTileSize;
            }
            set
            {
                textureTileSize = value;
                if (inScene)
                {
                    terrainConfig.TextureTileSize = textureTileSize;
                }
            }
        }

        [BrowsableAttribute(true), CategoryAttribute("Alpha Map Mosaics"), DescriptionAttribute("Name of the texture mosaic file in the asset repository to use for layers 1-4.")]
        public string Alpha0MosaicName
        {
            get
            {
                return alpha0MosaicName;
            }
            set
            {
                alpha0MosaicName = value;
                if (inScene)
                {
                    terrainConfig.SetAlphaMapName(0, alpha0MosaicName);
                }
            }
        }

        [BrowsableAttribute(true), CategoryAttribute("Alpha Map Mosaics"), DescriptionAttribute("Name of the texture mosaic file in the asset repository to use for layers 5-8.")]
        public string Alpha1MosaicName
        {
            get
            {
                return alpha1MosaicName;
            }
            set
            {
                alpha1MosaicName = value;

                if (inScene)
                {
                    terrainConfig.SetAlphaMapName(1, alpha1MosaicName);
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of the texture file in the asset repository to use for layer 1. This texture corresponds with the red channel of Alpha map 0."), CategoryAttribute("Display Parameters")]
        public string Layer1TextureName
        {
            get
            {
                return l1TextureName;
            }
            set
            {
                l1TextureName = value;
                if (inScene)
                {
                    terrainConfig.SetLayerTextureName(0, l1TextureName);
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of the texture file in the asset repository to use for layer 2. This texture corresponds with the green channel of Alpha map 0."), CategoryAttribute("Display Parameters")]
        public string Layer2TextureName
        {
            get
            {
                return l2TextureName;
            }
            set
            {
                l2TextureName = value;
                if (inScene)
                {
                    terrainConfig.SetLayerTextureName(1, l2TextureName);
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of the texture file in the asset repository to use for layer 3. This texture corresponds with the blue channel of Alpha map 0."), CategoryAttribute("Display Parameters")]
        public string Layer3TextureName
        {
            get
            {
                return l3TextureName;
            }
            set
            {
                l3TextureName = value;
                if (inScene)
                {
                    terrainConfig.SetLayerTextureName(2, l3TextureName);
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of the texture file in the asset repository to use for layer 4. This texture corresponds with the alpha channel of Alpha map 0."), CategoryAttribute("Display Parameters")]
        public string Layer4TextureName
        {
            get
            {
                return l4TextureName;
            }
            set
            {
                l4TextureName = value;
                if (inScene)
                {
                    terrainConfig.SetLayerTextureName(3, l4TextureName);
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of the texture file in the asset repository to use for layer 5. This texture corresponds with the red channel of Alpha map 1."), CategoryAttribute("Display Parameters")]
        public string Layer5TextureName
        {
            get
            {
                return l5TextureName;
            }
            set
            {
                l5TextureName = value;
                if (inScene)
                {
                    terrainConfig.SetLayerTextureName(4, l5TextureName);
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of the texture file in the asset repository to use for layer 6. This texture corresponds with the green channel of Alpha map 1."), CategoryAttribute("Display Parameters")]
        public string Layer6TextureName
        {
            get
            {
                return l6TextureName;
            }
            set
            {
                l6TextureName = value;
                if (inScene)
                {
                    terrainConfig.SetLayerTextureName(5, l6TextureName);
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of the texture file in the asset repository to use for layer 7. This texture corresponds with the blue channel of Alpha map 1."), CategoryAttribute("Display Parameters")]
        public string Layer7TextureName
        {
            get
            {
                return l7TextureName;
            }
            set
            {
                l7TextureName = value;
                if (inScene)
                {
                    terrainConfig.SetLayerTextureName(6, l7TextureName);
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of the texture file in the asset repository to use for layer 8. This texture corresponds with the alpha channel of Alpha map 1."), CategoryAttribute("Display Parameters")]
        public string Layer8TextureName
        {
            get
            {
                return l8TextureName;
            }
            set
            {
                l8TextureName = value;
                if (inScene)
                {
                    terrainConfig.SetLayerTextureName(7, l8TextureName);
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of the texture file in the asset repository to use as a detail map."), CategoryAttribute("Display Parameters")]
        public string DetailTextureName
        {
            get
            {
                return detailTextureName;
            }
            set
            {
                detailTextureName = value;
                if (inScene)
                {
                    terrainConfig.DetailTextureName = detailTextureName;
                }
            }
        }



        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;

            // add the terrain node
            node = app.MakeTreeNode(this, "Terrain Display");
            parentNode.Nodes.Add(node);

            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();

            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Alpha_Splat_Terrain_Display", app.HelpClickHandler);

            node.ContextMenuStrip = menuBuilder.Menu;
            inTree = true;
            buttonBar = menuBuilder.ButtonBar;
        }

        [BrowsableAttribute(false)]
        public List<ToolStripButton> ButtonBar
        {
            get
            {
                return buttonBar;
            }
        }

        public void Clone(IWorldContainer copyParent)
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
                // this property is not applicable to this object
            }
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString +=  String.Format("\tUseParams={0}\r\n",UseParams);
                objString +=  String.Format("\tTextureTileSize:{0}\r\n",TextureTileSize);
                objString +=  String.Format("\tAlpha0MosaicName:{0}\r\n",Alpha0MosaicName);
                objString +=  String.Format("\tAlpha1MosaicName:{0}\r\n",Alpha1MosaicName);
                objString +=  String.Format("\tLayer1TextureName:{0}\r\n",Layer1TextureName);
                objString +=  String.Format("\tLayer2TextureName:{0}\r\n",Layer2TextureName);
                objString +=  String.Format("\tLayer3TextureName:{0}\r\n",Layer3TextureName);
                objString +=  String.Format("\tLayer4TextureName:{0}\r\n",Layer4TextureName);
                objString +=  String.Format("\tLayer5TextureName:{0}\r\n",Layer5TextureName);
                objString +=  String.Format("\tLayer6TextureName:{0}\r\n",Layer6TextureName);
                objString +=  String.Format("\tLayer7TextureName:{0}\r\n",Layer7TextureName);
                objString +=  String.Format("\tLayer8TextureName:{0}\r\n",Layer8TextureName);
                objString +=  String.Format("\tDetailTextureName:{0}\r\n",DetailTextureName);
                objString +=  "\r\n";
                return objString;
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
            parentNode.Nodes.Remove(node);
            parentNode = null;
            node = null;

            inTree = false;
        }

        public void AddToScene()
        {
            inScene = true;
            terrainConfig = new AlphaSplatTerrainConfig();

            terrainConfig.UseParams = useParams;
            terrainConfig.TextureTileSize = textureTileSize;
            terrainConfig.SetAlphaMapName(0, alpha0MosaicName);
            terrainConfig.SetAlphaMapName(1, alpha1MosaicName);
            terrainConfig.SetLayerTextureName(0, l1TextureName);
            terrainConfig.SetLayerTextureName(1, l2TextureName);
            terrainConfig.SetLayerTextureName(2, l3TextureName);
            terrainConfig.SetLayerTextureName(3, l4TextureName);
            terrainConfig.SetLayerTextureName(4, l5TextureName);
            terrainConfig.SetLayerTextureName(5, l6TextureName);
            terrainConfig.SetLayerTextureName(6, l7TextureName);
            terrainConfig.SetLayerTextureName(7, l8TextureName);
            terrainConfig.DetailTextureName = detailTextureName;

            TerrainManager.Instance.TerrainMaterialConfig = terrainConfig;
        }

        public void RemoveFromScene()
        {
            inScene = false;
        }

        protected void CheckAsset(string name)
        {
            if ((name != null) && (name != ""))
            {
                if (!app.CheckAssetFileExists(name))
                {
                    app.AddMissingAsset(name);
                }
            }
        }

        [BrowsableAttribute(false)]
        public bool IsGlobal
        {
            get
            {
                return true;
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

        public void CheckAssets()
        {
            if ((alpha0MosaicName != null) && (alpha0MosaicName != ""))
            {
                CheckAsset(string.Format("{0}.mmf", alpha0MosaicName));
            }
            if ((alpha1MosaicName != null) && (alpha1MosaicName != ""))
            {
                CheckAsset(string.Format("{0}.mmf", alpha1MosaicName));
            }
            CheckAsset(l1TextureName);
            CheckAsset(l2TextureName);
            CheckAsset(l3TextureName);
            CheckAsset(l4TextureName);
            CheckAsset(l5TextureName);
            CheckAsset(l6TextureName);
            CheckAsset(l7TextureName);
            CheckAsset(l8TextureName);
            CheckAsset(detailTextureName);
        }

        public void ToXml(System.Xml.XmlWriter w)
        {
            w.WriteStartElement("TerrainDisplay");
            w.WriteAttributeString("Type", "AlphaSplat");
            w.WriteAttributeString("UseParams", useParams.ToString());
            w.WriteAttributeString("TextureTileSize", textureTileSize.ToString());
            w.WriteAttributeString("Alpha0MosaicName", alpha0MosaicName);
            w.WriteAttributeString("Alpha1MosaicName", alpha1MosaicName);
            w.WriteAttributeString("Layer1TextureName", l1TextureName);
            w.WriteAttributeString("Layer2TextureName", l2TextureName);
            w.WriteAttributeString("Layer3TextureName", l3TextureName);
            w.WriteAttributeString("Layer4TextureName", l4TextureName);
            w.WriteAttributeString("Layer5TextureName", l5TextureName);
            w.WriteAttributeString("Layer6TextureName", l6TextureName);
            w.WriteAttributeString("Layer7TextureName", l7TextureName);
            w.WriteAttributeString("Layer8TextureName", l8TextureName);
            w.WriteAttributeString("DetailTextureName", detailTextureName);

            w.WriteEndElement();
        }

        public void FromXml(XmlReader r)
        {
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                switch (r.Name)
                {
                    case "Type":
                        break;
                    case "UseParams":
                        useParams = (r.Value == "True");
                        break;
                    case "TextureTileSize":
                        textureTileSize = float.Parse(r.Value);
                        break;
                    case "Alpha0MosaicName":
                        alpha0MosaicName = r.Value;
                        break;
                    case "Alpha1MosaicName":
                        alpha1MosaicName = r.Value;
                        break;
                    case "Layer1TextureName":
                        l1TextureName = r.Value;
                        break;
                    case "Layer2TextureName":
                        l2TextureName = r.Value;
                        break;
                    case "Layer3TextureName":
                        l3TextureName = r.Value;
                        break;
                    case "Layer4TextureName":
                        l4TextureName = r.Value;
                        break;
                    case "Layer5TextureName":
                        l5TextureName = r.Value;
                        break;
                    case "Layer6TextureName":
                        l6TextureName = r.Value;
                        break;
                    case "Layer7TextureName":
                        l7TextureName = r.Value;
                        break;
                    case "Layer8TextureName":
                        l8TextureName = r.Value;
                        break;
                    case "DetailTextureName":
                        detailTextureName = r.Value;
                        break;
                }
            }
            r.MoveToElement();
        }

        [BrowsableAttribute(false)]
        public Vector3 FocusLocation
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
                // do nothing
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
            w.WriteLine("Mosaic:{0}", alpha0MosaicName);
            w.WriteLine("Mosaic:{0}", alpha1MosaicName);
            w.WriteLine("Texture:{0}", Layer1TextureName);
            w.WriteLine("Texture:{0}", Layer2TextureName);
            w.WriteLine("Texture:{0}", Layer3TextureName);
            w.WriteLine("Texture:{0}", Layer4TextureName);
            w.WriteLine("Texture:{0}", Layer5TextureName);
            w.WriteLine("Texture:{0}", Layer6TextureName);
            w.WriteLine("Texture:{0}", Layer7TextureName);
            w.WriteLine("Texture:{0}", Layer8TextureName);
            w.WriteLine("Texture:{0}", DetailTextureName);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IWorldObject Members

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "AlphaSplatTerrainDisplay";
            }
        }

        #endregion
    }
}
