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
    public class AlphaSplatTerrainMaterial : ITerrainMaterial
    {
        protected TerrainPage.PageHilightType highlightType = TerrainPage.PageHilightType.None;
        protected Texture highlightMask;
        protected Texture highlightTexture;
        protected Material material;
        protected string materialName;
        protected bool dirty = true;
        protected bool typeChange = true;
        protected AlphaSplatTerrainConfig config;
        protected int pageX;
        protected int pageZ;

        public AlphaSplatTerrainMaterial(AlphaSplatTerrainConfig config, int pageX, int pageZ)
        {
            this.config = config;

            this.pageX = pageX;
            this.pageZ = pageZ;

            //LogManager.Instance.Write("Create AlphaSplatTerrainMaterial ({0}, {1})", pageX, pageZ);

            config.TerrainSplatChanged += On_TerrainSplatChanged;
        }

        private String HighlightMaterialName(TerrainPage.PageHilightType type)
        {
            switch (type)
            {
                case TerrainPage.PageHilightType.None:
                default:
                    return "AlphaSplatTerrain";
                case TerrainPage.PageHilightType.Colorized:
                    return "AlphaSplatTerrainColorizedHighlight";
                case TerrainPage.PageHilightType.EdgeBlend:
                    return "AlphaSplatTerrainEdgeBlendHighlight";
                case TerrainPage.PageHilightType.EdgeSharpBlend:
                    return "AlphaSplatTerrainEdgeSharpBlendHighlight";
                case TerrainPage.PageHilightType.EdgeSharp:
                    return "AlphaSplatTerrainEdgeSharpHighlight";
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

                materialName = String.Format("AlphaSplat-{0}-{1}-{2}", pageX, pageZ, HighlightTypeString);

                //LogManager.Instance.Write("Create Material: {0}", materialName);

                Material tmpMaterial = MaterialManager.Instance.GetByName(HighlightMaterialName(highlightType));
                material = tmpMaterial.Clone(materialName);

                typeChange = false;
            }

            Technique tech = material.GetTechnique(0);
            material.Load();
            if (tech.IsSupported)
            {
                Pass pass = tech.GetPass(0);
                // set the highlight mask texture
                int size = TerrainManager.Instance.PageSize;
                pass.VertexProgramParameters.SetNamedConstant("pageSize", new Vector3(size * TerrainManager.oneMeter, size * TerrainManager.oneMeter, size * TerrainManager.oneMeter));
                if (highlightType != TerrainPage.PageHilightType.None)
                {
                    pass.GetTextureUnitState(11).SetTextureName(highlightMask.Name);
                }
                //Page.SetShadeMask(hilightMaterial, 4);

                int pageSize = TerrainManager.Instance.PageSize;

                for (int index = 0; index < AlphaSplatTerrainConfig.NUM_ALPHA_MAPS; index++)
                {
                    TextureMosaic alphaMap = config.GetAlphaMap(index);
                    if (alphaMap != null)
                    {
                        // pass in the alpha texture names and the texture coord adjustment params.
                        // The coord adjustment params are used to convert from a page relative texture
                        // coordinate to the appropriate coords for the alpha texture.
                        float u1, u2, v1, v2;
                        string alphaTextureName = alphaMap.GetTexture(
                            pageX*pageSize, pageZ*pageSize, pageSize, pageSize,
                            out u1, out v1, out u2, out v2);

                        pass.GetTextureUnitState(index).SetTextureName(alphaTextureName);

                        string coordAdjustParam = "alpha" + index + "TextureCoordAdjust";
                        pass.VertexProgramParameters.SetNamedConstant(
                            coordAdjustParam, new Vector4(u1, u2 - u1, v1, v2 - v1));

                        //todo: Disabled page logging because it slows down terrain edits 
                        // in the terrain editor.  We should probably make this 
                        // logging be conditional. -Trev 9/4/08
//                        LogManager.Instance.Write("page[{0},{1}]: {2} : ({3},{4}) : ({5},{6})", pageX, pageZ,
//                                                  alphaTextureName, u1, v1, u2, v2);
                    }
                }
            }

            config.UpdateMaterial(material);

            material.Load();
            material.Lighting = true;

            dirty = false;
        }

        protected void On_TerrainSplatChanged(ITerrainMaterialConfig cfg, MosaicTile tile, int worldXMeters, int worldZMeters, int sizeXMeters, int sizeZMeters)
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
            //LogManager.Instance.Write("Dispose AlphaSplatTerrainMaterial ({0}, {1})", pageX, pageZ);
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
