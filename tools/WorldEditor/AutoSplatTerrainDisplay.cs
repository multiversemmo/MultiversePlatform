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
    public class AutoSplatTerrainDisplay : IWorldObject
    {
        protected WorldTerrain parent;
        protected WorldEditor app;

        protected WorldTreeNode node;
        protected WorldTreeNode parentNode;

        protected float sandToGrassHeight = 50000;
        protected float grassToRockHeight = 250000;
        protected float rockToSnowHeight = 450000;

        protected float textureTileSize = 5;

        protected string sandTextureName = "splatting_sand.dds";
        protected string grassTextureName = "splatting_grass.dds";
        protected string rockTextureName = "splatting_rock.dds";
        protected string snowTextureName = "splatting_snow.dds";
        protected string shadeMaskTextureName = "";

        protected bool useParams = true;
        protected bool useGeneratedShadeMask = true;

        protected bool inScene = false;
        protected bool inTree = false;
        protected List<ToolStripButton> buttonBar;

        public AutoSplatTerrainDisplay(WorldTerrain parent, WorldEditor worldEditor)
        {
            this.parent = parent;
            this.app = worldEditor;
        }

        public AutoSplatTerrainDisplay(WorldTerrain parent, WorldEditor worldEditor, XmlReader r)
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

                AutoSplatConfig terrainConfig;
                terrainConfig = TerrainManager.Instance.TerrainMaterialConfig as AutoSplatConfig;
                terrainConfig.UseParams = useParams;
            }
        }

        [DescriptionAttribute("If true, the engine will generate a detail map that makes the terrain lighter and darker in different areas. Set to false to specify your own shade mask with the ShadeMaskTextureName property."), CategoryAttribute("Display Parameters")]
        public bool UseGeneratedShadeMask
        {
            get
            {
                return useGeneratedShadeMask;
            }
            set
            {
                useGeneratedShadeMask = value;

                AutoSplatConfig terrainConfig;
                terrainConfig = TerrainManager.Instance.TerrainMaterialConfig as AutoSplatConfig;
                terrainConfig.UseGeneratedShadeMask = useGeneratedShadeMask;
            }
        }

        [DescriptionAttribute("Altitude at which terrain has grass texture. Terrain below this altitude has a mixture of grass and sand based on altitude."), CategoryAttribute("Splatting Altitudes")]
        public float SandToGrassHeight
        {
            get
            {
                return sandToGrassHeight;
            }
            set
            {
                sandToGrassHeight = value;
                if (inScene)
                {
                    AutoSplatConfig terrainConfig;
                    terrainConfig = TerrainManager.Instance.TerrainMaterialConfig as AutoSplatConfig;
                    terrainConfig.SandToGrassHeight = sandToGrassHeight;
                }
            }
        }

        [DescriptionAttribute("Altitude at which terrain has rock texture. Terrain below this altitude has a mixture of grass and rock textures, and terrain above this altitude has a mixture of rock and snow textures."), CategoryAttribute("Splatting Altitudes")]
        public float GrassToRockHeight
        {
            get
            {
                return grassToRockHeight;
            }
            set
            {
                grassToRockHeight = value;
                if (inScene)
                {
                    AutoSplatConfig terrainConfig;
                    terrainConfig = TerrainManager.Instance.TerrainMaterialConfig as AutoSplatConfig;
                    terrainConfig.GrassToRockHeight = grassToRockHeight;
                }
            }
        }

        [DescriptionAttribute("Altitude at or above this level has the snow texture. Terrain below this altitude has a mixture of rock and snow textures."), CategoryAttribute("Splatting Altitudes")]
        public float RockToSnowHeight
        {
            get
            {
                return rockToSnowHeight;
            }
            set
            {
                rockToSnowHeight = value;
                if (inScene)
                {
                    AutoSplatConfig terrainConfig;
                    terrainConfig = TerrainManager.Instance.TerrainMaterialConfig as AutoSplatConfig;
                    terrainConfig.RockToSnowHeight = rockToSnowHeight;
                }
            }
        }

        [DescriptionAttribute("Length of sides (in meters) of the square covered by one terrain texture tile."), CategoryAttribute("Display Parameters")]
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
                    AutoSplatConfig terrainConfig;
                    terrainConfig = TerrainManager.Instance.TerrainMaterialConfig as AutoSplatConfig;
                    terrainConfig.TextureTileSize = textureTileSize;
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of file in asset repository to use for \"sand\" texture."), CategoryAttribute("Textures")]
        public string SandTextureName
        {
            get
            {
                return sandTextureName;
            }
            set
            {
                sandTextureName = value;
                if (inScene)
                {
                    AutoSplatConfig terrainConfig;
                    terrainConfig = TerrainManager.Instance.TerrainMaterialConfig as AutoSplatConfig;
                    terrainConfig.SandTextureName = sandTextureName;
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of file in asset repository to use for \"grass\" texture."), CategoryAttribute("Textures")]
        public string GrassTextureName
        {
            get
            {
                return grassTextureName;
            }
            set
            {
                grassTextureName = value;
                if (inScene)
                {
                    AutoSplatConfig terrainConfig;
                    terrainConfig = TerrainManager.Instance.TerrainMaterialConfig as AutoSplatConfig;
                    terrainConfig.GrassTextureName = grassTextureName;
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of file in asset repository to use for \"rock\" texture."), CategoryAttribute("Textures")]
        public string RockTextureName
        {
            get
            {
                return rockTextureName;
            }
            set
            {
                rockTextureName = value;
                if (inScene)
                {
                    AutoSplatConfig terrainConfig;
                    terrainConfig = TerrainManager.Instance.TerrainMaterialConfig as AutoSplatConfig;
                    terrainConfig.RockTextureName = rockTextureName;
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of file in asset repository to use for \"snow\" texture."), CategoryAttribute("Textures")]
        public string SnowTextureName
        {
            get
            {
                return snowTextureName;
            }
            set
            {
                snowTextureName = value;
                if (inScene)
                {
                    AutoSplatConfig terrainConfig;
                    terrainConfig = TerrainManager.Instance.TerrainMaterialConfig as AutoSplatConfig;
                    terrainConfig.SnowTextureName = snowTextureName;
                }
            }
        }

        [EditorAttribute(typeof(TextureSelectorTypeEditor), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name of the texture file in asset repository used as the shade mask."), CategoryAttribute("Textures")]
        public string ShadeMaskTextureName
        {
            get
            {
                return shadeMaskTextureName;
            }
            set
            {
                shadeMaskTextureName = value;
                if (inScene)
                {
                    AutoSplatConfig terrainConfig;
                    terrainConfig = TerrainManager.Instance.TerrainMaterialConfig as AutoSplatConfig;
                    terrainConfig.ShadeMaskTextureName = shadeMaskTextureName;
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
            menuBuilder.Add("Help", "Auto_Splat_Terrain_Display", app.HelpClickHandler);

            node.ContextMenuStrip = menuBuilder.Menu;
            inTree = true;
            buttonBar = menuBuilder.ButtonBar;
        }

        public void Clone(IWorldContainer copyParent)
        {
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
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n",ObjectType);
                objString +=  String.Format("\tUseParams={0}\r\n", UseParams);
                objString +=  String.Format("\tUseGeneratedShadeMask={0}\r\n", UseGeneratedShadeMask);
                objString +=  String.Format("\tSandToGrassHeight={0}\r\n", SandToGrassHeight);
                objString +=  String.Format("\tGrassToRockHeight={0}\r\n", GrassToRockHeight);
                objString +=  String.Format("\tRockToSnowHeight={0}\r\n", RockToSnowHeight);
                objString +=  String.Format("\tTextureTileSize={0}\r\n",TextureTileSize);
                objString +=  String.Format("\tSandTextureName={0}\r\n",SandTextureName);
                objString +=  String.Format("\tGrassTextureName={0}\r\n",GrassTextureName);
                objString +=  String.Format("\tRockTextureName={0}\r\n",RockTextureName);
                objString +=  String.Format("\tSnowTextureNam={0}\r\n", SnowTextureName);
                objString +=  String.Format("\tShadeMaskTextureName={0}\r\n", ShadeMaskTextureName);
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
            AutoSplatConfig terrainConfig;

            terrainConfig = new AutoSplatConfig();

            terrainConfig.UseParams = useParams;
            terrainConfig.UseGeneratedShadeMask = useGeneratedShadeMask;

            terrainConfig.SandToGrassHeight = sandToGrassHeight;
            terrainConfig.GrassToRockHeight = grassToRockHeight;
            terrainConfig.RockToSnowHeight = rockToSnowHeight;

            terrainConfig.TextureTileSize = textureTileSize;

            terrainConfig.SandTextureName = sandTextureName;
            terrainConfig.GrassTextureName = grassTextureName;
            terrainConfig.RockTextureName = rockTextureName;
            terrainConfig.SnowTextureName = snowTextureName;
            terrainConfig.ShadeMaskTextureName = shadeMaskTextureName;

            TerrainManager.Instance.TerrainMaterialConfig = terrainConfig;
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
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

        public void CheckAssets()
        {
            CheckAsset(sandTextureName);
            CheckAsset(grassTextureName);
            CheckAsset(rockTextureName);
            CheckAsset(snowTextureName);
            CheckAsset(shadeMaskTextureName);
        }

        public void ToXml(System.Xml.XmlWriter w)
        {
            w.WriteStartElement("TerrainDisplay");
            w.WriteAttributeString("Type", "AutoSplat");
            w.WriteAttributeString("UseParams", useParams.ToString());
            w.WriteAttributeString("UseGeneratedShadeMask", useGeneratedShadeMask.ToString());
            w.WriteAttributeString("TextureTileSize", textureTileSize.ToString());

            w.WriteAttributeString("SandToGrassHeight", sandToGrassHeight.ToString());
            w.WriteAttributeString("GrassToRockHeight", grassToRockHeight.ToString());
            w.WriteAttributeString("RockToSnowHeight", rockToSnowHeight.ToString());

            w.WriteAttributeString("SandTextureName", sandTextureName);
            w.WriteAttributeString("GrassTextureName", grassTextureName);
            w.WriteAttributeString("RockTextureName", rockTextureName);
            w.WriteAttributeString("SnowTextureName", snowTextureName);
            w.WriteAttributeString("ShadeMaskTextureName", shadeMaskTextureName);

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
                    case "UseGeneratedShadeMask":
                        useGeneratedShadeMask = (r.Value == "True");
                        break;
                    case "TextureTileSize":
                        textureTileSize = float.Parse(r.Value);
                        break;
                    case "SandToGrassHeight":
                        sandToGrassHeight = float.Parse(r.Value);
                        break;
                    case "GrassToRockHeight":
                        grassToRockHeight = float.Parse(r.Value);
                        break;
                    case "RockToSnowHeight":
                        rockToSnowHeight = float.Parse(r.Value);
                        break;
                    case "SandTextureName":
                        sandTextureName = r.Value;
                        break;
                    case "GrassTextureName":
                        grassTextureName = r.Value;
                        break;
                    case "RockTextureName":
                        rockTextureName = r.Value;
                        break;
                    case "SnowTextureName":
                        snowTextureName = r.Value;
                        break;
                    case "ShadeMaskTextureName":
                        shadeMaskTextureName = r.Value;
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

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public String ObjectType
        {
            get
            {
                return "AutoSplatTerrainDisplay";
            }
        }

        protected void WriteManifestIfNotBlank(System.IO.StreamWriter w, string assetType, string asset)
        {
            if ((asset != null) && (asset != ""))
            {
                w.WriteLine("{0}:{1}", assetType, asset);
            }
        }

        public void ToManifest(System.IO.StreamWriter w)
        {
            WriteManifestIfNotBlank(w, "Texture", sandTextureName);
            WriteManifestIfNotBlank(w, "Texture", grassTextureName);
            WriteManifestIfNotBlank(w, "Texture", rockTextureName);
            WriteManifestIfNotBlank(w, "Texture", snowTextureName);
            WriteManifestIfNotBlank(w, "Texture", shadeMaskTextureName);
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
