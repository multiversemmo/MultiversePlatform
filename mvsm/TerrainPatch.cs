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
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using System.Diagnostics;

namespace Axiom.SceneManagers.Multiverse
{
    public class TerrainPatch
    {
        // size of patch in samples, along each axis
        private int numSamples;

        // size of patch in meters
        private int patchSize;

        private int metersPerSample;
        private int startX;
        private int startZ;
        private PageHeightMap pageHeightMap;

        private TerrainPage terrainPage;

        Vector3 patchLoc;
        int southMPS = 0;
        int eastMPS = 0;

        int southSamples = 0;
        int eastSamples = 0;
        int stitchVerts = 0;
        int stitchTriangleCount = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">size of the terrain patch in meters</param>
        /// <param name="metersPerSample">meters per sample</param>
        /// <param name="pageHeightMap">which page heightmap to use to build the patch</param>
        /// <param name="startX">starting X offset of the patch within the page (in meters)</param>
        /// <param name="startZ">starting Z offset of the patch within the page (in meters)</param>
        public TerrainPatch(TerrainPage terrainPage, int size, int metersPerSample, int startX, int startZ)
        {
            this.terrainPage = terrainPage;
            patchSize = size;

            // save patch params
            this.pageHeightMap = terrainPage.PageHeightMap;
            this.metersPerSample = metersPerSample;
            this.startX = startX;
            this.startZ = startZ;

            patchLoc = new Vector3(
                terrainPage.Location.x + startX * TerrainManager.oneMeter,
                0,
                terrainPage.Location.z + startZ * TerrainManager.oneMeter);

            numSamples = patchSize / metersPerSample;

            pageHeightMap.SetPatchLOD(startX, startZ, patchSize, metersPerSample);

            ComputeStitchSize();

        }

        private void fillTopBotHeights(int xSample, int zSample, float[] heightBuffer)
        {
            if (zSample < 0 || zSample >= TerrainManager.Instance.PageSize)
            { // the row is outside the top or bottom of the page
                xSample = xSample - metersPerSample;
                // fill the middle samples
                for (int x = 0; x < (numSamples + 2); x++)
                {
                    heightBuffer[x] = pageHeightMap.GenHeight(xSample, zSample);

                    xSample += metersPerSample;
                }
            }
            else
            { // the row is within the page
                // fill the first sample
                if ((xSample - metersPerSample) < 0)
                {
                    heightBuffer[0] = pageHeightMap.GenHeight(xSample - metersPerSample, zSample);
                }
                else
                {
                    heightBuffer[0] = pageHeightMap.GetHeight(xSample - metersPerSample, zSample);
                }

                // fill the middle samples
                for (int x = 0; x < numSamples; x++)
                {
                    heightBuffer[x + 1] = pageHeightMap.GetHeight(xSample, zSample);

                    xSample += metersPerSample;
                }

                // fill the end sample
                if (xSample >= TerrainManager.Instance.PageSize)
                {
                    heightBuffer[numSamples + 1] = pageHeightMap.GenHeight(xSample, zSample);
                }
                else
                {
                    heightBuffer[numSamples + 1] = pageHeightMap.GetHeight(xSample, zSample);
                }
            }
        }

        private void fillHeights(int xSample, int zSample, float[] heightBuffer)
        {
            // fill the first sample
            if ((xSample - metersPerSample) < 0)
            {
                heightBuffer[0] = pageHeightMap.GenHeight(xSample - metersPerSample, zSample);
            }
            else
            {
                heightBuffer[0] = pageHeightMap.GetHeight(xSample - metersPerSample, zSample);
            }

            // fill the middle samples
            for (int x = 0; x < numSamples; x++)
            {
                heightBuffer[x + 1] = pageHeightMap.GetHeight(xSample, zSample);

                xSample += metersPerSample;
            }

            // fill the end sample
            if (xSample >= TerrainManager.Instance.PageSize)
            {
                heightBuffer[numSamples + 1] = pageHeightMap.GenHeight(xSample, zSample);
            }
            else
            {
                heightBuffer[numSamples + 1] = pageHeightMap.GetHeight(xSample, zSample);
            }
        }

        private Vector3 ComputeNormal(float x1, float x2, float z1, float z2)
        {
            float unitsPerSample = metersPerSample * TerrainManager.oneMeter;

            // computer the normal
            Vector3 v = new Vector3(x1 - x2,
                2.0f * unitsPerSample,
                z1 - z2);
            v.Normalize();

            return v;
        }

        public void BuildVertexIndexData(IntPtr vertexBufferPtr, int vertexOff, IntPtr indexBufferPtr, int indexOff )
        {
            float[][] lineBuffers = new float[3][];

            lineBuffers[0] = new float[numSamples + 2];
            lineBuffers[1] = new float[numSamples + 2];
            lineBuffers[2] = new float[numSamples + 2];

            fillTopBotHeights(startX, startZ - metersPerSample, lineBuffers[1]);
            fillHeights(startX, startZ, lineBuffers[2]);

            int baseVertex = vertexOff / TerrainPage.VertexSize;

            unsafe
            {
                //
                // Build vertex buffer for main tile
                //
                float* buffer = (float*)vertexBufferPtr.ToPointer();

                for (int zIndex = 0; zIndex < numSamples; zIndex++)
                {
                    int zSample = zIndex * metersPerSample + startZ;
                    float z = zSample * TerrainManager.oneMeter;

                    float[] tmpLine;

                    tmpLine = lineBuffers[0];
                    lineBuffers[0] = lineBuffers[1];
                    lineBuffers[1] = lineBuffers[2];
                    lineBuffers[2] = tmpLine;

                    // fill the next line
                    if (zIndex < (numSamples - 1))
                    {
                        fillHeights(startX, zSample + metersPerSample, lineBuffers[2]);
                    }
                    else
                    {
                        fillTopBotHeights(startX, zSample + metersPerSample, lineBuffers[2]);
                    }

                    for (int xIndex = 0; xIndex < numSamples; xIndex++)
                    {
                        int xSample = xIndex * metersPerSample + startX;
                        float x = xSample * TerrainManager.oneMeter;

                        float height = pageHeightMap.GetHeight(xSample, zSample);

                        // Position
                        buffer[vertexOff++] = x;
                        buffer[vertexOff++] = lineBuffers[1][xIndex + 1];
                        buffer[vertexOff++] = z;

                        // normals
                        // XXX - this can be optimized quite a bit
                        // XXX - normals point up right now
                        //Vector3 norm = tile.GetNormalAt(new Vector3(x + tile.Location.x, height, z + tile.Location.z));
                        Vector3 norm = ComputeNormal(lineBuffers[1][xIndex], lineBuffers[1][xIndex + 2],
                            lineBuffers[0][xIndex + 1], lineBuffers[2][xIndex + 1]);
                        //Vector3 norm = pageHeightMap.GetNormal(xSample, zSample);
                        //Vector3 norm = Vector3.UnitY;

                        buffer[vertexOff++] = norm.x;
                        buffer[vertexOff++] = norm.y;
                        buffer[vertexOff++] = norm.z;

                        // Texture
                        // XXX - assumes one unit of texture space is one page.
                        //   how does the vertex shader deal with texture coords?
                        buffer[vertexOff++] = x / (TerrainManager.Instance.PageSize * TerrainManager.oneMeter);
                        buffer[vertexOff++] = z / (TerrainManager.Instance.PageSize * TerrainManager.oneMeter);
                    }
                }

            }

            //
            // build index buffer for main tile
            //
            int i = baseVertex;
            unsafe
            {
                ushort* indexBuffer = (ushort*)indexBufferPtr.ToPointer();
                for (int z = 0; z < numSamples - 1; z++)
                {
                    for (int x = 0; x < numSamples - 1; x++)
                    {
                        indexBuffer[indexOff++] = (ushort)i;
                        indexBuffer[indexOff++] = (ushort)(i + numSamples);
                        indexBuffer[indexOff++] = (ushort)(i + 1);

                        indexBuffer[indexOff++] = (ushort)(i + numSamples);
                        indexBuffer[indexOff++] = (ushort)(i + 1 + numSamples);
                        indexBuffer[indexOff++] = (ushort)(i + 1);

                        i++;
                    }
                    i++;
                }
            }

            BuildStitch(vertexBufferPtr, vertexOff, indexBufferPtr, indexOff);

            return;
        }

        /// <summary>
        /// Determine whether the adjacent tile has changed LOD
        /// Returns true if the stitch is ok.
        /// </summary>
        public bool ValidateStitch()
        {
            Vector3 patchLoc = new Vector3(
                terrainPage.Location.x + startX * TerrainManager.oneMeter,
                0,
                terrainPage.Location.z + startZ * TerrainManager.oneMeter);

            // compute starting locations of south and east edges of adjacent patches
            Vector3 southLoc = new Vector3(patchLoc.x, 0, patchLoc.z + patchSize * TerrainManager.oneMeter);
            Vector3 eastLoc = new Vector3(patchLoc.x + patchSize * TerrainManager.oneMeter, 0, patchLoc.z);

            // compute meters per sample of adjacent patches
            int south = TerrainManager.Instance.MetersPerSample(southLoc);
            int east = TerrainManager.Instance.MetersPerSample(eastLoc);

            return (south == southMPS) && (east == eastMPS);
        }

        // compute the number of vertices for the stitching
        private void ComputeStitchSize()
        {

            // compute starting locations of south and east edges of adjacent patches
            Vector3 southLoc = new Vector3(patchLoc.x, 0, patchLoc.z + patchSize * TerrainManager.oneMeter);
            Vector3 eastLoc = new Vector3(patchLoc.x + patchSize * TerrainManager.oneMeter, 0, patchLoc.z);

            // compute meters per sample of adjacent patches
            southMPS = TerrainManager.Instance.MetersPerSample(southLoc);
            eastMPS = TerrainManager.Instance.MetersPerSample(eastLoc);

            // if neighbors are off the edge of the map, just use the same meters per sample as the main patch
            if (southMPS == 0)
            {
                southMPS = metersPerSample;
            }
            if (eastMPS == 0)
            {
                eastMPS = metersPerSample;
            }

            // make sure current and neighbor patches are only off by one level of detail
            Debug.Assert((southMPS == metersPerSample) || (southMPS == (metersPerSample * 2)) || ((southMPS * 2) == metersPerSample));
            Debug.Assert((eastMPS == metersPerSample) || (eastMPS == (metersPerSample * 2)) || ((eastMPS * 2) == metersPerSample));

            southSamples = patchSize / southMPS;
            eastSamples = patchSize / eastMPS;

            // compute the total number of verts for the stitch.
            stitchVerts = ( numSamples + numSamples + southSamples + eastSamples );

            stitchTriangleCount = 0;

            // count north triangles
            if (southSamples != 0)
            {
                if (southSamples == numSamples)
                {
                    stitchTriangleCount += ((numSamples - 1) * 2);
                }
                else if (southSamples > numSamples)
                {
                    stitchTriangleCount += ((numSamples - 1) * 3);
                }
                else
                {
                    stitchTriangleCount += ((southSamples - 1) * 3);
                }

                // count northeast triangles
                if (eastSamples != 0)
                {
                    if (eastSamples == southSamples)
                    {
                        if (eastSamples == numSamples)
                        {
                            stitchTriangleCount += 2;
                        }
                        else
                        {
                            stitchTriangleCount += 4;
                        }
                    }
                    else
                    {
                        stitchTriangleCount += 3;
                    }
                }
            }

            // count east triangles
            if (eastSamples != 0)
            {
                if (eastSamples == numSamples)
                {
                    stitchTriangleCount += ((numSamples - 1) * 2);
                }
                else if (eastSamples > numSamples)
                {
                    stitchTriangleCount += ((numSamples - 1) * 3);
                }
                else
                {
                    stitchTriangleCount += ((eastSamples - 1) * 3);
                }
            }
        }


        private void BuildStitch(IntPtr vertexBuf, int vertexOff, IntPtr indexBuf, int indexOff)
        {

            int startVertex = vertexOff / TerrainPage.VertexSize;

            unsafe
            {
                float* buffer = (float*)vertexBuf.ToPointer();

                int xSample;
                int zSample;
                float x;
                float z;

                // fill the buffer with the samples from the south edge of the patch
                zSample = patchSize - metersPerSample + startZ;
                z = zSample * TerrainManager.oneMeter;

                for (int xIndex = 0; xIndex < numSamples; xIndex++)
                {
                    xSample = xIndex * metersPerSample + startX;
                    x = xSample * TerrainManager.oneMeter;

                    float height = pageHeightMap.GetHeight(xSample, zSample);

                    // Position
                    buffer[vertexOff++] = x;
                    buffer[vertexOff++] = height;
                    buffer[vertexOff++] = z;

                    // normals
                    // XXX - this can be optimized quite a bit
                    // XXX - normals point up right now
                    //Vector3 norm = tile.GetNormalAt(new Vector3(x + tile.Location.x, height, z + tile.Location.z));
                    Vector3 norm = pageHeightMap.GetNormal(xSample, zSample);
                    //Vector3 norm = Vector3.UnitY;

                    buffer[vertexOff++] = norm.x;
                    buffer[vertexOff++] = norm.y;
                    buffer[vertexOff++] = norm.z;

                    // Texture
                    // XXX - assumes one unit of texture space is one page.
                    //   how does the vertex shader deal with texture coords?
                    buffer[vertexOff++] = x / (TerrainManager.Instance.PageSize * TerrainManager.oneMeter);
                    buffer[vertexOff++] = z / (TerrainManager.Instance.PageSize * TerrainManager.oneMeter);
                }

                // fill the buffer with the samples from the east edge of the patch
                xSample = patchSize - metersPerSample + startX;
                x = xSample * TerrainManager.oneMeter;

                for (int zIndex = numSamples - 2; zIndex >= 0; zIndex--)
                {
                    zSample = zIndex * metersPerSample + startZ;
                    z = zSample * TerrainManager.oneMeter;

                    float height = pageHeightMap.GetHeight(xSample, zSample);

                    // Position
                    buffer[vertexOff++] = x;
                    buffer[vertexOff++] = height;
                    buffer[vertexOff++] = z;

                    // normals
                    // XXX - this can be optimized quite a bit
                    // XXX - normals point up right now
                    //Vector3 norm = tile.GetNormalAt(new Vector3(x + tile.Location.x, height, z + tile.Location.z));
                    Vector3 norm = pageHeightMap.GetNormal(xSample, zSample);
                    //Vector3 norm = Vector3.UnitY;

                    buffer[vertexOff++] = norm.x;
                    buffer[vertexOff++] = norm.y;
                    buffer[vertexOff++] = norm.z;

                    // Texture
                    // XXX - assumes one unit of texture space is one page.
                    //   how does the vertex shader deal with texture coords?
                    buffer[vertexOff++] = x / (TerrainManager.Instance.PageSize * TerrainManager.oneMeter);
                    buffer[vertexOff++] = z / (TerrainManager.Instance.PageSize * TerrainManager.oneMeter);
                }

                zSample = patchSize + startZ;
                z = zSample * TerrainManager.oneMeter;
                for (int xIndex = 0; xIndex <= southSamples; xIndex++)
                {
                    xSample = xIndex * southMPS + startX;
                    x = xSample * TerrainManager.oneMeter;

                    float height = TerrainManager.Instance.TerrainGenerator.GenerateHeightPointMM(new Vector3(x + terrainPage.Location.x, 0, z + terrainPage.Location.z));

                    // Position
                    buffer[vertexOff++] = x;
                    buffer[vertexOff++] = height;
                    buffer[vertexOff++] = z;

                    // normals
                    // XXX - this can be optimized quite a bit
                    // XXX - normals point up right now
                    //Vector3 norm = tile.GetNormalAt(new Vector3(x + tile.Location.x, height, z + tile.Location.z));
                    Vector3 norm = pageHeightMap.GetNormal(xSample, zSample);
                    //Vector3 norm = Vector3.UnitY;

                    buffer[vertexOff++] = norm.x;
                    buffer[vertexOff++] = norm.y;
                    buffer[vertexOff++] = norm.z;

                    // Texture
                    // XXX - assumes one unit of texture space is one page.
                    //   how does the vertex shader deal with texture coords?
                    buffer[vertexOff++] = x / (TerrainManager.Instance.PageSize * TerrainManager.oneMeter);
                    buffer[vertexOff++] = z / (TerrainManager.Instance.PageSize * TerrainManager.oneMeter);
                }

                // keep x from previous loop
                for (int zIndex = eastSamples - 1; zIndex >= 0; zIndex--)
                {
                    zSample = zIndex * eastMPS + startZ;
                    z = zSample * TerrainManager.oneMeter;

                    float height = TerrainManager.Instance.TerrainGenerator.GenerateHeightPointMM(new Vector3(x + terrainPage.Location.x, 0, z + terrainPage.Location.z));

                    // Position
                    buffer[vertexOff++] = x;
                    buffer[vertexOff++] = height;
                    buffer[vertexOff++] = z;

                    // normals
                    // XXX - this can be optimized quite a bit
                    // XXX - normals point up right now
                    //Vector3 norm = tile.GetNormalAt(new Vector3(x + tile.Location.x, height, z + tile.Location.z));
                    Vector3 norm = pageHeightMap.GetNormal(xSample, zSample);
                    //Vector3 norm = Vector3.UnitY;

                    buffer[vertexOff++] = norm.x;
                    buffer[vertexOff++] = norm.y;
                    buffer[vertexOff++] = norm.z;

                    // Texture
                    // XXX - assumes one unit of texture space is one page.
                    //   how does the vertex shader deal with texture coords?
                    buffer[vertexOff++] = x / (TerrainManager.Instance.PageSize * TerrainManager.oneMeter);
                    buffer[vertexOff++] = z / (TerrainManager.Instance.PageSize * TerrainManager.oneMeter);
                }
            }

            BuildStitchIndexBuffer(numSamples, southSamples, eastSamples, startVertex, indexBuf, indexOff);
           
        }


        public void BuildStitchIndexBuffer(int numSamples, int northSamples, int eastSamples, int startVertex, IntPtr indexBufferPtr, int indexOff)
        {
            string name = String.Format("{0},{1},{2}", numSamples, northSamples, eastSamples);

            int pos = indexOff;
            unsafe
            {
                ushort* indexBuffer = (ushort*)indexBufferPtr.ToPointer();

                int neighborOff;
                int innerCornerOff = 0;
                int outerCornerOff = 0;

                if (northSamples == 0)
                {
                    // there is no north side
                    neighborOff = numSamples;
                    outerCornerOff = numSamples;
                }
                else if (eastSamples == 0)
                {
                    // there is no east side
                    neighborOff = numSamples;
                }
                else
                {
                    neighborOff = (numSamples * 2) - 1;
                    innerCornerOff = numSamples - 1;
                    outerCornerOff = neighborOff + northSamples;
                }

                int eastTOff = 0;
                int eastNOff = 0;

                // generate north triangles
                if (northSamples != 0)
                {
                    int tOff = 0;
                    int nOff = neighborOff;

                    if (northSamples == numSamples)
                    {
                        for (int i = 0; i < (numSamples - 1); i++)
                        {
                            indexBuffer[pos++] = (ushort)(tOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);

                            indexBuffer[pos++] = (ushort)(nOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);
                            nOff++;
                            tOff++;
                        }
                    }
                    else if (northSamples > numSamples)
                    {
                        for (int i = 0; i < (numSamples - 1); i++)
                        {
                            indexBuffer[pos++] = (ushort)(tOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);

                            indexBuffer[pos++] = (ushort)(tOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);

                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 2 + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);
                            nOff += 2;
                            tOff++;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < (northSamples - 1); i++)
                        {
                            indexBuffer[pos++] = (ushort)(tOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);

                            indexBuffer[pos++] = (ushort)(nOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);

                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 2 + startVertex);
                            nOff++;
                            tOff += 2;
                        }
                    }

                    // draw northeast triangles
                    if (eastSamples != 0)
                    {
                        if (eastSamples == northSamples)
                        {
                            if (eastSamples == numSamples)
                            { // neighbors same lod as tile
                                indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);
                                indexBuffer[pos++] = (ushort)(outerCornerOff - 1 + startVertex);
                                indexBuffer[pos++] = (ushort)(outerCornerOff + startVertex);

                                indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);
                                indexBuffer[pos++] = (ushort)(outerCornerOff + startVertex);
                                indexBuffer[pos++] = (ushort)(outerCornerOff + 1 + startVertex);

                                eastTOff = innerCornerOff;
                                eastNOff = outerCornerOff + 1;
                            }
                            else
                            { // neighbors lesser LOD
                                indexBuffer[pos++] = (ushort)(innerCornerOff - 1 + startVertex);
                                indexBuffer[pos++] = (ushort)(outerCornerOff - 1 + startVertex);
                                indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);

                                indexBuffer[pos++] = (ushort)(outerCornerOff - 1 + startVertex);
                                indexBuffer[pos++] = (ushort)(outerCornerOff + startVertex);
                                indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);

                                indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);
                                indexBuffer[pos++] = (ushort)(outerCornerOff + startVertex);
                                indexBuffer[pos++] = (ushort)(outerCornerOff + 1 + startVertex);

                                indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);
                                indexBuffer[pos++] = (ushort)(outerCornerOff + 1 + startVertex);
                                indexBuffer[pos++] = (ushort)(innerCornerOff + 1 + startVertex);

                                eastTOff = innerCornerOff + 1;
                                eastNOff = outerCornerOff + 1;
                            }
                        }
                        else
                        { // both neighbors exist.  one is the same lod as tile.
                            if (northSamples == numSamples)
                            {
                                // LOD change is to the east
                                if (eastSamples < numSamples)
                                {
                                    // lower LOD to east
                                    indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff - 1 + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + startVertex);

                                    indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + 1 + startVertex);

                                    indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + 1 + startVertex);
                                    indexBuffer[pos++] = (ushort)(innerCornerOff + 1 + startVertex);

                                    eastTOff = innerCornerOff + 1;
                                    eastNOff = outerCornerOff + 1;
                                }
                                else
                                {
                                    // higher LOD to east
                                    indexBuffer[pos++] = (ushort)(outerCornerOff - 1 + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + 1 + startVertex);

                                    indexBuffer[pos++] = (ushort)(outerCornerOff - 1 + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + 1 + startVertex);
                                    indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);

                                    indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + 1 + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + 2 + startVertex);

                                    eastTOff = innerCornerOff;
                                    eastNOff = outerCornerOff + 2;
                                }
                            }
                            else
                            {
                                // LOD Change is to the north
                                if (northSamples < numSamples)
                                {
                                    // lower LOD to north
                                    indexBuffer[pos++] = (ushort)(innerCornerOff - 1 + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff - 1 + startVertex);
                                    indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);

                                    indexBuffer[pos++] = (ushort)(outerCornerOff - 1 + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);

                                    indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + 1 + startVertex);

                                    eastTOff = innerCornerOff;
                                    eastNOff = outerCornerOff + 1;
                                }
                                else
                                {
                                    // higher LOD to north
                                    indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff - 2 + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff - 1 + startVertex);

                                    indexBuffer[pos++] = (ushort)(innerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff - 1 + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + 1 + startVertex);

                                    indexBuffer[pos++] = (ushort)(outerCornerOff - 1 + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + startVertex);
                                    indexBuffer[pos++] = (ushort)(outerCornerOff + 1 + startVertex);

                                    eastTOff = innerCornerOff;
                                    eastNOff = outerCornerOff + 1;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // no north neighbor
                    eastTOff = 0;
                    eastNOff = numSamples;
                }

                // draw east triangles
                if (eastSamples != 0)
                {
                    int tOff = eastTOff;
                    int nOff = eastNOff;

                    if (eastSamples == numSamples)
                    {
                        for (int i = 0; i < (numSamples - 1); i++)
                        {
                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);

                            indexBuffer[pos++] = (ushort)(tOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);

                            tOff++;
                            nOff++;
                        }
                    }
                    else if (eastSamples > numSamples)
                    {
                        for (int i = 0; i < (numSamples - 1); i++)
                        {
                            indexBuffer[pos++] = (ushort)(tOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);

                            indexBuffer[pos++] = (ushort)(tOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);

                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 2 + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);
                            nOff += 2;
                            tOff++;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < (eastSamples - 1); i++)
                        {
                            indexBuffer[pos++] = (ushort)(tOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);

                            indexBuffer[pos++] = (ushort)(nOff + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);

                            indexBuffer[pos++] = (ushort)(tOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(nOff + 1 + startVertex);
                            indexBuffer[pos++] = (ushort)(tOff + 2 + startVertex);

                            nOff++;
                            tOff += 2;
                        }
                    }
                }
            }
            return ;
        }

        public void DumpLOD()
        {
            LogManager.Instance.Write("    MPS: {0}, east: {1}, south: {2}", metersPerSample, eastMPS, southMPS);
        }

        public int MetersPerSample
        {
            get
            {
                return metersPerSample;
            }
        }

        public int NumVerts
        {
            get
            {
                return stitchVerts + numSamples * numSamples;
            }
        }

        public int NumTriangles
        {
            get
            {
                return stitchTriangleCount + ((numSamples - 1) * (numSamples - 1) * 2);
            }
        }

        public void ResetHeightMaps()
        {
            pageHeightMap.ResetHeightMaps();
        }
    }
}
