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
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;


namespace Axiom.SceneManagers.Multiverse
{
    public class AutoSplatMaterial : ITerrainMaterial
    {
        protected TerrainPage.PageHilightType highlightType = TerrainPage.PageHilightType.None;
        protected Texture highlightMask;
        protected Texture highlightTexture;
        protected Material material;
        protected string materialName;
        protected bool dirty = true;
        protected bool typeChange = true;
        protected AutoSplatConfig config;
        protected int pageX;
        protected int pageZ;

        public AutoSplatMaterial(AutoSplatConfig config, int pageX, int pageZ)
        {
            this.config = config;

            this.pageX = pageX;
            this.pageZ = pageZ;

            //LogManager.Instance.Write("Create AutoSplatMaterial ({0}, {1})", pageX, pageZ);

            config.ConfigChange += ConfigChange;
        }

        private static String HighlightMaterialName(TerrainPage.PageHilightType type)
        {
            switch (type)
            {
                case TerrainPage.PageHilightType.None:
                default:
                    return "MVSMTerrain";
                case TerrainPage.PageHilightType.Colorized:
                    return "MVSMTerrainColorizedHilight";
                case TerrainPage.PageHilightType.EdgeBlend:
                    return "MVSMTerrainEdgeBlendHilight";
                case TerrainPage.PageHilightType.EdgeSharpBlend:
                    return "MVSMTerrainEdgeSharpBlendHilight";
                case TerrainPage.PageHilightType.EdgeSharp:
                    return "MVSMTerrainEdgeSharpHilight";
            }
        }

        private String HighlightTypeString
        {
            get
            {
                switch (highlightType)
                {
                    case TerrainPage.PageHilightType.None:
                    default:
                        return "None";
                    case TerrainPage.PageHilightType.Colorized:
                        return "Colorized";
                    case TerrainPage.PageHilightType.EdgeBlend:
                        return "EdgeBlend";
                    case TerrainPage.PageHilightType.EdgeSharpBlend:
                        return "EdgeSharpBlend";
                    case TerrainPage.PageHilightType.EdgeSharp:
                        return "EdgeSharp";
                }
            }
        }

        protected void BuildMaterial()
        {

            // If the highlight type changes, we need to use a different shader, so we load a
            // different material to go with it.
            if (typeChange)
            {
                if (material != null)
                {
                    //LogManager.Instance.Write("Free Material: {0}", material.Name);

                    MaterialManager.Instance.Unload(material);
                    material.Dispose();
                    material = null;
                }

                materialName = String.Format("AutoSplat-{0}-{1}-{2}", pageX, pageZ, HighlightTypeString);

                //LogManager.Instance.Write("Create Material: {0}", materialName);

                Material tmpMaterial = MaterialManager.Instance.GetByName(HighlightMaterialName(highlightType));
                material = tmpMaterial.Clone(materialName);

                typeChange = false;
            }

            Technique tech = material.GetTechnique(0);
            material.Load();

            // Only do this stuff if we are using technique 0.  Otherwise we are using the
            // fixed function fallback technique, so we don't want to do any shader stuff.
            if (tech.IsSupported)
            {
                // set the highlight mask texture
                int size = TerrainManager.Instance.PageSize;
                tech.GetPass(0).VertexProgramParameters.SetNamedConstant("pageSize", new Vector3(size * TerrainManager.oneMeter, size * TerrainManager.oneMeter, size * TerrainManager.oneMeter));
                if (highlightType != TerrainPage.PageHilightType.None)
                {
                    tech.GetPass(0).GetTextureUnitState(5).SetTextureName(highlightMask.Name);
                }
                //Page.SetShadeMask(hilightMaterial, 4);

                config.UpdateMaterial(material);
            }

            material.Load();
            material.Lighting = true;

            dirty = false;
        }

        protected void ConfigChange(object sender, EventArgs args)
        {
            dirty = true;
        }

        #region ITerrainMaterial Members

        public Material Material
        {
            get
            {
                if (dirty)
                {
                    BuildMaterial();
                }
                return material;
            }
        }

        public TerrainPage.PageHilightType HighlightType
        {
            get
            {
                return highlightType;
            }
            set
            {
                if (highlightType != value)
                {
                    highlightType = value;
                    dirty = true;
                    typeChange = true;
                }
            }
        }

        public Texture HighlightMask
        {
            get
            {
                return highlightMask;
            }
            set
            {
                highlightMask = value;
                dirty = true;
            }
        }

        public Texture HighlightTexture
        {
            get
            {
                return highlightTexture;
            }
            set
            {
                highlightTexture = value;
                dirty = true;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //LogManager.Instance.Write("Dispose AutoSplatMaterial ({0}, {1})", pageX, pageZ);
            if (material != null)
            {
                //LogManager.Instance.Write("Free Material(Dispose): {0}", material.Name);

                MaterialManager.Instance.Unload(material);
                material.Dispose();
                material = null;
            }
        }

        #endregion
    }
}
