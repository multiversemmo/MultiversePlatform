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
using System.Collections.Generic;
using System.Text;
using Axiom.Graphics;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Multiverse
{
    public class TerrainDecalManager
    {
        protected int numPages;
        protected PageCoord cameraPage;
        protected List<DecalElement> elements;
        protected PageDecalInfo[,] framePageDecalInfos;
        protected bool sorted = false;

        public TerrainDecalManager(int numPages)
        {
            this.numPages = numPages;
            elements = new List<DecalElement>();
        }

        public DecalElement CreateDecalElement(string imageName,
            float posX, float posZ, float sizeX, float sizeZ, float rot,
            float lifetime, float deleteRadius, int priority)
        {
            DecalElement element = new DecalElement(imageName,
                posX, posZ, sizeX, sizeZ, rot,
                lifetime, deleteRadius, priority, this);

            elements.Add(element);

            sorted = false;

            return element;
        }

        public void RemoveDecalElement(DecalElement element)
        {
            elements.Remove(element);
        }

        public void NeedsSorting()
        {
            sorted = false;
        }

        public PageCoord CameraPage
        {
            get
            {
                return cameraPage;
            }
        }

        public void PerFrameProcessing(Vector3 cameraLocation)
        {
            // free the passes from last frame if there are any
            if (framePageDecalInfos != null)
            {
                FreePasses(framePageDecalInfos);
                framePageDecalInfos = null;
            }


            // compute the camera page
            PageCoord lastCameraPage = cameraPage;
            cameraPage = new PageCoord(cameraLocation, TerrainManager.Instance.PageSize);

            // update the decals
            Update(cameraLocation.x, cameraLocation.z);

            // build the per-page data structures
            framePageDecalInfos = BuildPageInfos();

            // if there are any visible decals, then build the material passes for all affected pages
            if (framePageDecalInfos != null)
            {
                BuildPasses(framePageDecalInfos);
            }
        }

        protected void Update(float cameraX, float cameraZ)
        {
            if (!sorted)
            {
                elements.Sort();
                sorted = true;
            }

            List<DecalElement> removalList = new List<DecalElement>();

            // update current elements, marking those that need to be deleted
            foreach (DecalElement element in elements)
            {
                bool deleteElement = element.Update(cameraX, cameraZ);
                if (deleteElement)
                {
                    removalList.Add(element);
                }
            }

            // process the deletion list
            foreach (DecalElement element in removalList)
            {
                elements.Remove(element);
            }

            return;
        }

        protected PageDecalInfo[,] BuildPageInfos()
        {
            PageDecalInfo[,] pageInfos = new PageDecalInfo[numPages,numPages];

            int pageRadius = numPages >> 1;
            int pageStartX = cameraPage.X - pageRadius;
            int pageStartZ = cameraPage.Z - pageRadius;
            int pageEndX = cameraPage.X + pageRadius;
            int pageEndZ = cameraPage.Z + pageRadius;
            int pageInfoCount = 0;

            foreach (DecalElement element in elements)
            {
                int startX = Math.Max(element.MinPageCoord.X, pageStartX);
                int startZ = Math.Max(element.MinPageCoord.Z, pageStartZ);
                int endX = Math.Min(element.MaxPageCoord.X, pageEndX);
                int endZ = Math.Min(element.MaxPageCoord.Z, pageEndZ);

                if ((startX <= endX) && (startZ <= endZ))
                { // element overlaps the visible pages
                    for (int z = startZ; z <= endZ; z++)
                    {
                        for (int x = startX; x <= endX; x++)
                        {
                            PageDecalInfo pageInfo = pageInfos[x - pageStartX, z - pageStartZ];
                            if ( pageInfo == null )
                            {
                                // this is the first decal on this page, so make a new page info object
                                pageInfo = pageInfos[x - pageStartX, z - pageStartZ] = new PageDecalInfo(new PageCoord(x, z));
                                pageInfoCount++;
                            }

                            // add the decal to this page
                            pageInfo.Decals.Add(element);
                        }
                    }
                }
            }

            if (pageInfoCount == 0)
            {
                // if there are no visible decals, then return null rather than an empty array
                pageInfos = null;
            }

            return pageInfos;
        }

        public void FreePasses(PageDecalInfo[,] pageInfos)
        {
            foreach (PageDecalInfo pageInfo in pageInfos)
            {
                if (pageInfo != null)
                {
                    FreeMaterialPass(pageInfo);
                }
            }
        }

        public void BuildPasses(PageDecalInfo[,] pageInfos)
        {
            foreach (PageDecalInfo pageInfo in pageInfos)
            {
                if (pageInfo != null)
                {
                    BuildMaterialPasses(pageInfo);
                }
            }
        }

        protected Technique FindPageTechnique(PageCoord coord)
        {
            Page page = TerrainManager.Instance.LookupPage(coord);
            Technique t = null;
            if (page != null)
            {
                Material mat = page.TerrainPage.Material;
                t = mat.GetBestTechnique();
            }

            return t;
        }

        public void FreeMaterialPass(PageDecalInfo pageInfo)
        {
            Technique t = FindPageTechnique(pageInfo.Coord);

            if (t != null)
            {
                // remove the passes from the material
                foreach (Pass p in pageInfo.Passes)
                {
                    t.RemovePass(p);
                }

                // clear the list of saved passes
                pageInfo.Passes.Clear();
            }
        }

        public void BuildMaterialPasses(PageDecalInfo pageInfo)
        {
            Technique t = FindPageTechnique(pageInfo.Coord);
            if (t != null)
            {

                float pageX = pageInfo.Coord.X * TerrainManager.Instance.PageSize * TerrainManager.oneMeter;
                float pageZ = pageInfo.Coord.Z * TerrainManager.Instance.PageSize * TerrainManager.oneMeter;

                int availableTexUnits = 0;
                int curTexUnit = 0;
                int texUnitsPerPass = 8;

                Pass p = null;

                foreach (DecalElement element in pageInfo.Decals)
                {
                    // if there are no texture units available, allocate a new pass
                    if (availableTexUnits == 0)
                    {
                        p = t.CreatePass();
                        pageInfo.Passes.Add(p);

                        p.SetSceneBlending(SceneBlendType.TransparentAlpha);
                        // TODO: Unclear what should happen here.  The new Ogre interface
                        // supports SetDepthBias(constantBias, slopeBias), but the units are
                        // different.  Ask Jeff.
                        p.DepthBias = 1;

                        curTexUnit = 0;
                        availableTexUnits = texUnitsPerPass;
                    }

                    TextureUnitState texUnit = p.CreateTextureUnitState(element.ImageName, 0);

                    if (curTexUnit == 0)
                    {
                        texUnit.SetColorOperation(LayerBlendOperation.Replace);
                        texUnit.SetAlphaOperation(LayerBlendOperationEx.Source1, LayerBlendSource.Texture, LayerBlendSource.Current, 0, 0, 0);
                    }
                    else
                    {
                        texUnit.SetColorOperation(LayerBlendOperation.AlphaBlend);
                        texUnit.SetAlphaOperation(LayerBlendOperationEx.AddSmooth, LayerBlendSource.Texture, LayerBlendSource.Current, 0, 0, 0);
                    }
                    texUnit.TextureAddressing = TextureAddressing.Border;
                    texUnit.TextureBorderColor = new ColorEx(0, 0, 0, 0);

                    element.UpdateTextureTransform(texUnit, pageX, pageZ);

                    // bump the counts
                    curTexUnit++;
                    availableTexUnits--;
                }
            }
        }

    }

    public class PageDecalInfo
    {
        protected PageCoord coord;
        protected Material material;
        protected List<DecalElement> decals;
        protected List<Pass> passes;

        public PageDecalInfo(PageCoord coord)
        {
            this.coord = coord;
            decals = new List<DecalElement>();
            passes = new List<Pass>();
        }

        public Material Material
        {
            get
            {
                return material;
            }
            set
            {
                material = value;
            }
        }

        public List<DecalElement> Decals
        {
            get
            {
                return decals;
            }
        }

        public PageCoord Coord
        {
            get
            {
                return coord;
            }
        }

        public List<Pass> Passes
        {
            get
            {
                return passes;
            }
        }
    }
}
