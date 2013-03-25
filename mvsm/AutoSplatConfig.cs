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
using System.Xml;
using Axiom.Graphics;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Multiverse
{
    public class AutoSplatConfig : ITerrainMaterialConfig
    {
        public event ConfigChangeHandler ConfigChange;

        protected bool useParams = true;
        protected bool useGeneratedShadeMask = true;

        protected float sandToGrassHeight = 50 * TerrainManager.oneMeter;
        protected float grassToRockHeight = 250 * TerrainManager.oneMeter;
        protected float rockToSnowHeight = 450 * TerrainManager.oneMeter;

        protected float textureTileSize = 5;

        protected string sandTextureName = "splatting_sand.dds";
        protected string grassTextureName = "splatting_grass.dds";
        protected string rockTextureName = "splatting_rock.dds";
        protected string snowTextureName = "splatting_snow.dds";
        protected string shadeMaskTextureName = "";

        public AutoSplatConfig()
        {
        }

        public AutoSplatConfig(XmlReader r)
        {
            FromXml(r);
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
        protected void OnConfigChange()
        {
            ConfigChangeHandler handler = ConfigChange;
            if (handler != null)
            {
                handler(null, new EventArgs());
            }
        }

        public bool UseParams
        {
            get
            {
                return useParams;
            }
            set
            {
                if (useParams != value)
                {
                    useParams = value;
                    OnConfigChange();
                }
            }
        }

        public bool UseGeneratedShadeMask
        {
            get
            {
                return useGeneratedShadeMask;
            }
            set
            {
                if (useGeneratedShadeMask != value)
                {
                    useGeneratedShadeMask = value;
                    OnConfigChange();
                }
            }
        }

        public float SandToGrassHeight
        {
            get
            {
                return sandToGrassHeight;
            }
            set
            {
                if (sandToGrassHeight != value)
                {
                    sandToGrassHeight = value;
                    OnConfigChange();
                }
            }
        }

        public float GrassToRockHeight
        {
            get
            {
                return grassToRockHeight;
            }
            set
            {
                if (grassToRockHeight != value)
                {
                    grassToRockHeight = value;
                    OnConfigChange();
                }
            }
        }


        public float RockToSnowHeight
        {
            get
            {
                return rockToSnowHeight;
            }
            set
            {
                if (rockToSnowHeight != value)
                {
                    rockToSnowHeight = value;
                    OnConfigChange();
                }
            }
        }

        public float TextureTileSize
        {
            get
            {
                return textureTileSize;
            }
            set
            {
                if (textureTileSize != value)
                {
                    textureTileSize = value;
                    OnConfigChange();
                }
            }
        }

        public string SandTextureName
        {
            get
            {
                return sandTextureName;
            }
            set
            {
                if (sandTextureName != value)
                {
                    sandTextureName = value;
                    OnConfigChange();
                }
            }
        }

        public string GrassTextureName
        {
            get
            {
                return grassTextureName;
            }
            set
            {
                if (grassTextureName != value)
                {
                    grassTextureName = value;
                    OnConfigChange();
                }
            }
        }

        public string RockTextureName
        {
            get
            {
                return rockTextureName;
            }
            set
            {
                if (rockTextureName != value)
                {
                    rockTextureName = value;
                    OnConfigChange();
                }
            }
        }

        public string SnowTextureName
        {
            get
            {
                return snowTextureName;
            }
            set
            {
                if (snowTextureName != value)
                {
                    snowTextureName = value;
                    OnConfigChange();
                }
            }
        }

        public string ShadeMaskTextureName
        {
            get
            {
                return shadeMaskTextureName;
            }
            set
            {
                if (shadeMaskTextureName != value)
                {
                    shadeMaskTextureName = value;
                    OnConfigChange();
                }
            }
        }

        #region ITerrainMaterialConfig Members

        public ITerrainMaterial NewTerrainMaterial(int pageX, int pageZ)
        {
            return new AutoSplatMaterial(this, pageX, pageZ);
        }

        public void UpdateMaterial(Material material)
        {
            if (useParams)
            {
                Pass pass = material.GetTechnique(0).GetPass(0);
                pass.GetTextureUnitState(0).SetTextureName(sandTextureName);
                pass.GetTextureUnitState(1).SetTextureName(grassTextureName);
                pass.GetTextureUnitState(2).SetTextureName(rockTextureName);
                pass.GetTextureUnitState(3).SetTextureName(snowTextureName);
                GpuProgramParameters vertexParams = pass.VertexProgramParameters;
                vertexParams.SetNamedConstant("splatConfig", new Vector3(sandToGrassHeight, grassToRockHeight, rockToSnowHeight));
                vertexParams.SetNamedConstant("textureTileSize", new Vector3(textureTileSize, 0, 0));

                if (useGeneratedShadeMask)
                {
                    Page.SetShadeMask(material, 4);
                }
                else
                {
                    pass.GetTextureUnitState(4).SetTextureName(shadeMaskTextureName);
                }
            }
        }

        #endregion
    }
}
