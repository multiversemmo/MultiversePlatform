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
using System.Diagnostics;
using System.IO;
using System.Text;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Media; 

namespace Multiverse.Tools.TerrainGenerator
{
    public class EditableHeightField : SimpleRenderable
    {
        private int width;
        private int height;
        private float[] heightData = null;
        private float xzScale = 128000.0f;
        private float yScale = 300000.0f;
        private bool rebuildVertex = true;
        private bool rebuildIndex = true;
        private float minHeight;
        private float maxHeight;
        private float offsetX;
        private float offsetZ;
        private int cursorX = 0;
        private int cursorZ = 0;

        private BrushStyle brushShape = BrushStyle.Square;
        private int brushWidth = 1;
        private int brushTaper = 100;
        private bool updateBrush = true;
        private bool updateTaper = true;

        private float[,] brush;
        private float[,] taper;

        private bool showBrush = false;

        private Texture brushTexture;

        #region Static Brush Definitions

        private static float[,] brushMask_1 = { { 255f/255f } };

        #endregion Static Brush Definitions

        public EditableHeightField(int w, int h)
            : this(new float[w * h], w, h)
        {
        }

        public EditableHeightField(int w, int h, float startingHeight) : this(w,h)
        {
            for (int i = 0; i < (w * h); i++)
            {
                heightData[i] = startingHeight;
            }
        }

        public EditableHeightField(float[] map, int w, int h)
        {
            Debug.Assert(map.Length == (w * h));

            width = w;
            height = h;

            heightData = map;

            InitMaterial();
            ComputeBounds();
        }

        public EditableHeightField(string filename, out int bitsPerSample)
        {
            bool docsv = false;
            bitsPerSample = 0;
			try
            {
                LoadHeightMapImage(filename, out bitsPerSample);
			}
            catch (Exception)
            {
                docsv = true;
            }

            if (docsv)
            {
                LoadCSV(filename);
            }

            InitMaterial();
            ComputeBounds();
        }

        private void InitMaterial()
        {
            material = MaterialManager.Instance.GetByName("HMETerrain");
            material.Load();
            material.Lighting = true;

            ValidateBrush();
        }

        private void UpdateMaterialBrush()
        {
            
            // build the brush texture with a 1 pixel wide border around it so that
            // we can use texture address clamping to constrain the texture to just
            // the brush area.
            byte[] tmpBrush = new byte[32 * 32];
            //byte[] tmpBrush = new byte[( brushWidth + 2 ) * ( brushWidth + 2 )];

            for (int z = 0; z < brushWidth; z++)
            {
                for (int x = 0; x < brushWidth; x++)
                {
                    tmpBrush[x + 1 + ( ( z + 1 ) * 32 )] = (byte)Math.Round(brush[x,z] * taper[x,z] * 255f);
                }
            }

            Image brushImg = Image.FromDynamicImage(tmpBrush, 32, 32, PixelFormat.A8);

            if (brushTexture != null)
            {
                brushTexture.Dispose();
            }
            brushTexture = TextureManager.Instance.LoadImage("HEDBrushTexture", brushImg);
            material.GetTechnique(0).GetPass(1).GetTextureUnitState(1).SetTextureName("HEDBrushTexture");
        }

        private void ComputeBounds()
        {
            maxHeight = 0f;
            minHeight = 1f;

            for (int i = 0; i < width * height; i++)
            {
                float h = heightData[i];
                if (h < minHeight)
                {
                    minHeight = h;
                }
                if (h > maxHeight)
                {
                    maxHeight = h;
                }
            }

            UpdateBounds();
        }

        private void UpdateBounds()
        {
            // set bounding box
            box = new AxisAlignedBox(new Vector3(0 + offsetX, minHeight * yScale, 0 + offsetZ),
                new Vector3(width * xzScale + offsetX, maxHeight * yScale, height * xzScale + offsetZ));

            // set bounding sphere
            worldBoundingSphere.Center = box.Center;
            worldBoundingSphere.Radius = box.Maximum.Length;
        }

        public void LoadHeightMapImage(string filename, out int bitsPerSample)
        {
			Stream s = new FileStream(filename, FileMode.Open);
            Image img;
            try
            {
                img = Image.FromStream(s, "png");
            }
            finally
            {
                s.Close();
            }

            width = img.Width;
            height = img.Height;
            bitsPerSample = 0;
            heightData = new float[width * height];
			if ((img.Format == PixelFormat.A8) || (img.Format == PixelFormat.L8))
            {
                bitsPerSample = 8;
				for (int i = 0; i < width * height; i++)
                {
                    heightData[i] = img.Data[i] / 256.0f;
                }
            }
            else if ((img.Format == PixelFormat.B8G8R8) || (img.Format == PixelFormat.R8G8B8))
            {
                bitsPerSample = 8;
                for (int i = 0; i < width * height; i++)
                {
                    heightData[i] = img.Data[i*3] / 256.0f;
                }
            }
			else if (img.Format == PixelFormat.L16)
			{
                bitsPerSample = 16;
                float divisor = 1.0f / (256.0f * 256.0f);
				for (int i = 0; i < width * height; i++)
                {
					int j = i * 2;
					// ??? Is this the right order?
					heightData[i] = (img.Data[j] + (img.Data[j+1] << 8)) * divisor;
                }
			}
            //else if (img.Format == PixelFormat.L24)
            //{
            //    bitsPerSample = 24;
            //    float divisor = 1.0f / (256.0f * 256.0f * 256.0f);
            //    for (int i = 0; i < width * height; i++)
            //    {
            //        int j = i * 3;
            //        // ??? Is this the right order?
            //        heightData[i] = ((float)(img.Data[j] + (img.Data[j+1] << 8) + (img.Data[j+2] << 16))) * divisor;
            //    }
            //}
        }

        // helper function to read a comma separated value text file (from excel) into a seed map
        public void LoadCSV(String filename)
        {
            StreamReader r = new StreamReader(filename);

            String line;
            int w;
            int h;

            // first line is the number of values per line
            line = r.ReadLine();
            w = int.Parse(line);

            // second line is the number of lines
            line = r.ReadLine();
            h = int.Parse(line);

            // If we have not already set the size and allocated the height field, then do it now.
            // This is used by the constuctor that creates the object from a filename
            if (heightData == null)
            {
                width = w;
                height = h;
                heightData = new float[width * height];
            }
            else if (h != height || w != width)
            {
                //XXX - should do something here

                r.Close();
                return;
            }

            int j = 0;
            while ((line = r.ReadLine()) != null)
            {
                for (int i = 0; i < w; i++)
                {
                    // every other character is a value (rest are commas)
                    char c = line[i << 1];
                    int val = c - '0';
                    heightData[j*width+i] = val / 9f;
                }
                j++;
            }

            r.Close();

            return;
        }

        private VertexData InitVertexBuffer(int numVerts)
        {
            vertexData = new VertexData();

            vertexData.vertexCount = numVerts;
            vertexData.vertexStart = 0;

            // set up the vertex declaration
            int vDecOffset = 0;
            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            vertexData.vertexDeclaration.AddElement(0, vDecOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords);
            vDecOffset += VertexElement.GetTypeSize(VertexElementType.Float2);

            // create the hardware vertex buffer and set up the buffer binding
            HardwareVertexBuffer hvBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                vertexData.vertexDeclaration.GetVertexSize(0), vertexData.vertexCount,
                BufferUsage.StaticWriteOnly, false);

            vertexData.vertexBufferBinding.SetBinding(0, hvBuffer);

            return vertexData;
        }

        public Vector3 HeightfieldCoordToWorldCoord(int x, int z)
        {
            return new Vector3(x * xzScale + offsetX, heightData[z * width + x] * yScale, z * xzScale + offsetZ);
        }

        private void buildVertexData()
        {
            if (vertexData != null)
            {
                vertexData.vertexBufferBinding.GetBuffer(0).Dispose();
            }
            vertexData = InitVertexBuffer(width * height);

            HardwareVertexBuffer hvBuffer = vertexData.vertexBufferBinding.GetBuffer(0);

            // lock the vertex buffer
            IntPtr ipBuf = hvBuffer.Lock(BufferLocking.Discard);

            int bufferOff = 0;

            unsafe
            {
                float* buffer = (float*)ipBuf.ToPointer();

                for (int zIndex = 0; zIndex < height; zIndex++)
                {
                    for (int xIndex = 0; xIndex < width; xIndex++)
                    {
                        // Position
                        buffer[bufferOff++] = xIndex * xzScale + offsetX;
                        buffer[bufferOff++] = heightData[zIndex*width + xIndex] * yScale;
                        buffer[bufferOff++] = zIndex * xzScale + offsetZ;

                        // normals
                        // XXX - pointing up for now
                        Vector3 norm = Vector3.UnitY;

                        buffer[bufferOff++] = norm.x;
                        buffer[bufferOff++] = norm.y;
                        buffer[bufferOff++] = norm.z;

                        // Texture
                        // XXX - assumes one unit of texture space is one page.
                        //   how does the vertex shader deal with texture coords?
                        buffer[bufferOff++] = xIndex;
                        buffer[bufferOff++] = zIndex;
                    }
                }

            }
            hvBuffer.Unlock();

            return;
        }

        private void buildIndexData()
        {
            indexData = new IndexData();

            int bufLength = width * height * 6;

            IndexType idxType = IndexType.Size32;

            if ((width * height) <= ushort.MaxValue)
                idxType = IndexType.Size16;

            indexData.indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
                    idxType, bufLength, BufferUsage.StaticWriteOnly);

            IntPtr indexBufferPtr = indexData.indexBuffer.Lock(0, indexData.indexBuffer.Size, BufferLocking.Discard);
            int indexCount = 0;

            int pos = 0;
            int i = 0;
            unsafe
            {
                if (idxType == IndexType.Size16) {
                    ushort* indexBuffer = (ushort*)indexBufferPtr.ToPointer();
                    for (int z = 0; z < height - 1; z++) {
                        for (int x = 0; x < width - 1; x++) {
                            indexBuffer[pos++] = (ushort)i;
                            indexBuffer[pos++] = (ushort)(i + width);
                            indexBuffer[pos++] = (ushort)(i + 1);

                            indexBuffer[pos++] = (ushort)(i + width);
                            indexBuffer[pos++] = (ushort)(i + 1 + width);
                            indexBuffer[pos++] = (ushort)(i + 1);

                            i++;
                            indexCount += 6;
                        }

                        i++;
                    }
                } else {
                    uint* indexBuffer = (uint*)indexBufferPtr.ToPointer();
                    for (int z = 0; z < height - 1; z++) {
                        for (int x = 0; x < width - 1; x++) {
                            indexBuffer[pos++] = (uint)i;
                            indexBuffer[pos++] = (uint)(i + width);
                            indexBuffer[pos++] = (uint)(i + 1);

                            indexBuffer[pos++] = (uint)(i + width);
                            indexBuffer[pos++] = (uint)(i + 1 + width);
                            indexBuffer[pos++] = (uint)(i + 1);

                            i++;
                            indexCount += 6;
                        }

                        i++;
                    }
                }
            }

            indexData.indexBuffer.Unlock();
            indexData.indexCount = indexCount;
            indexData.indexStart = 0;

            return;
        }

        private bool Inside(Vector3 pt)
        {
            return ((pt.x >= 0) && (pt.x < width) && (pt.z >= 0) && (pt.z < height));
        }

        public void SetCursorLoc(int x, int z)
        {
            cursorX = x;
            cursorZ = z;
            float brushOff = 1 + BrushWidth / 2.0f;
            SetCustomParameter(0, new Vector4(-x + brushOff, 0, -z + brushOff, 0));
        }

        public bool RayIntersection(Ray ray, out int x, out int z)
        {
            // generate a bounding box for the heightmap in its local space
            AxisAlignedBox axisAlignedBox = new AxisAlignedBox(new Vector3(0, minHeight, 0),
                    new Vector3(width, maxHeight, height));

            Vector3 rayLoc = new Vector3((ray.Origin.x - offsetX) / xzScale,
                                        ray.Origin.y / yScale,
                                        (ray.Origin.z - offsetZ) / xzScale);


            Vector3 rayDir = new Vector3(ray.Direction.x / xzScale, ray.Direction.y / yScale, ray.Direction.z / xzScale);
            rayDir.Normalize();

            // convert the ray to local heightmap space
            Ray tmpRay = new Ray(rayLoc, rayDir);

            // see if the ray intersects with the bounding box
            IntersectResult result = tmpRay.Intersects(axisAlignedBox);

            if (result.Hit)
            {
                // move rayLoc up to just before the point of intersection
                rayLoc = rayLoc + rayDir * ( result.Distance - 1 );

                //
                // deal with edge case where ray is coming from outside the heightmap
                // and is very near edge of map at intersection.
                //
                int insideCounter = 20;
                while ((!Inside(rayLoc)) && (insideCounter > 0))
                {
                    rayLoc += ( rayDir * 0.1f );
                    insideCounter--;
                }
                if (insideCounter == 0)
                {
                    x = 0;
                    z = 0;
                    return false;
                }

                x = (int)Math.Round(rayLoc.x);
                z = (int)Math.Round(rayLoc.z);

                if (x < 0)
                {
                    x = 0;
                }
                if (x >= width)
                {
                    x = width - 1;
                }
                if (z < 0)
                {
                    z = 0;
                }
                if (z >= height)
                {
                    z = height - 1;
                }

                bool above = rayLoc.y > heightData[x + z * width];

                while (Inside(rayLoc))
                {
                    // increment the ray
                    rayLoc += rayDir;

                    x = (int)Math.Round(rayLoc.x);
                    z = (int)Math.Round(rayLoc.z);

                    if (x < 0)
                    {
                        x = 0;
                    }
                    if (x >= width)
                    {
                        x = width - 1;
                    }
                    if (z < 0)
                    {
                        z = 0;
                    }
                    if (z >= height)
                    {
                        z = height - 1;
                    }

                    if (above != (rayLoc.y > heightData[x + z * width]))
                    {
                        // we found a hit
                        return true;
                    }
                }
            }
            x = 0;
            z = 0;
            return false;
        }

        public void AdjustPointWithBrush(int xIn, int zIn, float incr)
        {
            ValidateBrush();
            
            int x1 = xIn - (brushWidth >> 1);
            int z1 = zIn - (brushWidth >> 1);

            for (int z = 0; z < brushWidth; z++)
            {
                bool zValid = ((z + z1) > 0) && ((z + z1) < height);
                for (int x = 0; x < brushWidth; x++)
                {
                    bool xValid = ((x + x1) > 0) && ((x + x1) < width);

                    if (zValid && xValid)
                    {
                        float curIncr = incr * brush[x, z] * taper[x, z];

                        AdjustPoint(x + x1, z + z1, curIncr);
                    }
                }
            }
            ComputeBounds();
        }

        protected void AdjustPoint(int x, int z, float incr)
        {
            float cur = heightData[x + z * width];

            cur = cur + incr;
            if (cur > 1f)
            {
                cur = 1f;
            }
            if (cur < 0f)
            {
                cur = 0f;
            }
            heightData[x + z * width] = cur;
            rebuildVertex = true;
            return;
        }

        public float GetHeight(int x, int z)
        {
            return heightData[x + z * width];
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public float[] Map
        {
            get
            {
                return heightData;
            }
        }

        public float XZScale
        {
            set
            {
                xzScale = value;
                rebuildVertex = true;
            }
        }

        public float OffsetX
        {
            get
            {
                return offsetX;
            }
            set
            {
                offsetX = value;
                rebuildVertex = true;
            }
        }

        public float OffsetZ
        {
            get
            {
                return offsetZ;
            }
            set
            {
                offsetZ = value;
                rebuildVertex = true;
            }
        }

        public float YScale
        {
            set
            {
                yScale = value;
                rebuildVertex = true;
            }
        }

        public BrushStyle BrushShape
        {
            get
            {
                return brushShape;
            }
            set
            {
                brushShape = value;
                updateBrush = true;
            }
        }

        public int BrushWidth
        {
            get
            {
                return brushWidth;
            }
            set
            {
                brushWidth = value;
                updateBrush = true;
                updateTaper = true;
            }
        }

        public int BrushTaper
        {
            get
            {
                return brushTaper;
            }
            set
            {
                brushTaper = value;
                updateTaper = true;
            }
        }

        public bool ShowBrush
        {
            get
            {
                return showBrush;
            }
            set
            {
                showBrush = value;
                if (showBrush)
                {
                    updateBrush = true;
                    updateTaper = true;
                }
            }
        }

        private void DumpTaper()
        {
            LogManager.Instance.Write("New Taper");
            for (int z = 0; z < brushWidth; z++)
            {
                StringBuilder sb = new StringBuilder();

                for (int x = 0; x < brushWidth; x++)
                {
                    sb.Append(taper[x, z]);
                    sb.Append(", ");
                }

                LogManager.Instance.Write(sb.ToString());
            }
        }

        private void DumpBrush()
        {
            LogManager.Instance.Write("New Brush: {0}:", BrushStyleToString(brushShape));
            for (int z = 0; z < brushWidth; z++)
            {
                StringBuilder sb = new StringBuilder();

                for (int x = 0; x < brushWidth; x++)
                {
                    sb.Append(brush[x, z]);
                    sb.Append(", ");
                }

                LogManager.Instance.Write(sb.ToString());
            }
        }

        private void ValidateBrush()
        {

            bool updated = false;

            if (showBrush)
            {

                if (updateTaper)
                {
                    taper = new float[brushWidth, brushWidth];
                    int radius = (brushWidth - 1) / 2;
                    float taperStep = (100f - brushTaper) / 100f / radius;
                    float currentTaper = 1f - taperStep;

                    int evenOddMod = (brushWidth & 0x1) == 0 ? 1 : 0;

                    taper[radius, radius] = 1.0f;
                    if (evenOddMod == 1)
                    {
                        taper[radius, radius + 1] = 1f;
                        taper[radius + 1, radius] = 1f;
                        taper[radius + 1, radius + 1] = 1f;
                    }

                    // The evenOddMod has the effect of drawing a 1 index larger box
                    // Examples:
                    //
                    // r = 3     r = 4
                    //
                    //             3 x x x x
                    //   2 x x x   2 x # # x
                    //   1 x * x   1 x * # x
                    // Z=0 x x x   0 x x x x
                    //   X=0 1 2     0 1 2 3
                    //
                    // * = Center as defined by the radius
                    // # = Padded center for even sized shapes (which evenOddMod is used for)
                    // x = Values populated by loop below with currentTaper
                    // Z/X = Axis labels for the array

                    for (int r = 1; r <= radius; r++)
                    {
                        for (int i = radius - r; i <= radius + r + evenOddMod; i++)
                        {
                            taper[i, radius - r] = currentTaper;
                            taper[i, radius + r + evenOddMod] = currentTaper;
                            taper[radius - r, i] = currentTaper;
                            taper[radius + r + evenOddMod, i] = currentTaper;
                        }

                        currentTaper -= taperStep;
                    }

                    updateTaper = false;
                    updated = true;
                    // DumpTaper();
                }

                if (updateBrush)
                {
                    switch (brushShape)
                    {
                        case BrushStyle.Square:
                            brush = new float[brushWidth, brushWidth];
                            for (int x = 0; x < brushWidth; x++)
                            {
                                for (int z = 0; z < brushWidth; z++)
                                {
                                    brush[x, z] = 1f;
                                }
                            }
                            break;
                        case BrushStyle.Diamond:
                            if (brushWidth == 1)
                            {
                                brush = brushMask_1;
                            }
                            else
                            {
                                brush = new float[brushWidth, brushWidth];
                                int radius = brushWidth >> 1;
                                int midSpans = (brushWidth - 3) >> 1;
                                bool even = (brushWidth & 0x1) == 0;
                                if (even)
                                {
                                    radius--;
                                }

                                // top and bottom
                                brush[radius, 0] = 0.25f;
                                brush[radius, brushWidth - 1] = 0.25f;
                                if (even)
                                {
                                    brush[radius+1, 0] = 0.25f;
                                    brush[radius+1, brushWidth - 1] = 0.25f;                                    
                                }

                                // middle
                                brush[0, radius] = 0.25f;
                                brush[brushWidth - 1, radius] = 0.25f;
                                if (even)
                                {
                                    brush[0, radius+1] = 0.25f;
                                    brush[brushWidth - 1, radius+1] = 0.25f;                                    
                                }

                                for (int i = 1; i < brushWidth - 1; i++)
                                {
                                    brush[i, radius] = 1f;
                                    if (even)
                                    {
                                        brush[i, radius+1] = 1f;                                        
                                    }
                                }

                                // midspans (rows between top & middle and middle & bottom)
                                for (int span = 0; span < midSpans; span++)
                                {
                                    int fromX = radius - span;
                                    int toX = radius + span + (even ? 1 : 0);
                                    
                                    // ends of span (top and bottom)

                                    brush[fromX - 1, span + 1] = 0.5f;
                                    brush[toX + 1, span + 1] = 0.5f;
                                    brush[fromX - 1, brushWidth - span - 2] = 0.5f;
                                    brush[toX + 1, brushWidth - span - 2] = 0.5f;

                                    // middle of span (top and bottom)
                                    for (int x = fromX; x <= toX; x++)
                                    {
                                        brush[x, span + 1] = 1f;
                                        brush[x, brushWidth - span - 2] = 1f;
                                    }
                                }
                            }

                            break;
                        case BrushStyle.Circle:
                            float[,] circleBrushMask = CreateCircleBrush(brushWidth);
                            float[,] guassianBrush = CreateGaussianCircleBrush(brushWidth);
                            brush = CombineBrushes(guassianBrush, circleBrushMask);
                            break;
                        case BrushStyle.Slash:
                            if (brushWidth == 1)
                            {
                                brush = brushMask_1;
                            }
                            else
                            {
                                brush = new float[brushWidth, brushWidth];

                                // top
                                brush[brushWidth - 1, 0] = 1f;
                                brush[brushWidth - 2, 0] = 0.5f;

                                // bottom
                                brush[0, brushWidth - 1] = 1f;
                                brush[1, brushWidth - 1] = 0.5f;

                                for (int i = 1; i < brushWidth - 1; i++)
                                {
                                    brush[brushWidth - 1 - i, i] = 1f;
                                    brush[brushWidth - 2 - i, i] = 0.5f;
                                    brush[brushWidth - i, i] = 0.5f;
                                }
                            }
                            break;
                        case BrushStyle.Backslash:
                            if (brushWidth == 1)
                            {
                                brush = brushMask_1;
                            }
                            else
                            {
                                brush = new float[brushWidth, brushWidth];

                                // top
                                brush[0, 0] = 1f;
                                brush[1, 0] = 0.5f;

                                // bottom
                                brush[brushWidth - 1, brushWidth - 1] = 1f;
                                brush[brushWidth - 2, brushWidth - 1] = 0.5f;

                                for (int i = 1; i < brushWidth - 1; i++)
                                {
                                    brush[i, i] = 1f;
                                    brush[i - 1, i] = 0.5f;
                                    brush[i + 1, i] = 0.5f;
                                }
                            }
                            break;
                        case BrushStyle.HBar:
                            if (brushWidth == 1)
                            {
                                brush = brushMask_1;
                            }
                            else
                            {
                                brush = new float[brushWidth, brushWidth];
                                int radius = (brushWidth >> 1);
                                for (int i = 0; i < brushWidth; i++)
                                {
                                    brush[i, radius] = 1f;
                                    brush[i, radius - 1] = 0.5f;
                                    brush[i, radius + 1] = 0.5f;
                                }
                            }
                            break;

                        case BrushStyle.VBar:
                            if (brushWidth == 1)
                            {
                                brush = brushMask_1;
                            }
                            else
                            {
                                brush = new float[brushWidth, brushWidth];
                                int radius = (brushWidth >> 1);
                                for (int i = 0; i < brushWidth; i++)
                                {
                                    brush[radius, i] = 1f;
                                    brush[radius - 1, i] = 0.5f;
                                    brush[radius + 1, i] = 0.5f;
                                }
                            }
                            break;
                    }

                    DumpBrush();

                    updateBrush = false;
                    updated = true;

                    SetCursorLoc(cursorX, cursorZ);
                }

            }
            else
            {
                brush = new float[brushWidth, brushWidth];
                taper = new float[brushWidth, brushWidth];
                updated = true;
            }
            
            if (updated)
            {
                UpdateMaterialBrush();
            }

        }

        public override void GetRenderOperation(RenderOperation op)
        {
            ValidateBrush();
            op.useIndices = true;
            op.operationType = OperationType.TriangleList;
            if (rebuildVertex)
            {
                buildVertexData();
                rebuildVertex = false;
            }
            op.vertexData = vertexData;
            if (rebuildIndex)
            {
                buildIndexData();
                rebuildIndex = false;
            }
            op.indexData = indexData;
        }

        public override float GetSquaredViewDepth(Camera camera)
        {
            // Use squared length to avoid square root
            return (ParentNode.DerivedPosition - camera.DerivedPosition).LengthSquared;
        }

        public override float BoundingRadius
        {
            get
            {
                return 0;
            }
        }

        public static string BrushStyleToString(BrushStyle style)
        {
            string s = "";

            switch (style)
            {
                case BrushStyle.Square:
                    s = "Square";
                    break;
                case BrushStyle.Diamond:
                    s = "Diamond";
                    break;
                case BrushStyle.Circle:
                    s = "Circle";
                    break;
                case BrushStyle.VBar:
                    s = "VBar";
                    break;
                case BrushStyle.HBar:
                    s = "HBar";
                    break;
                case BrushStyle.Slash:
                    s = "Slash";
                    break;
                case BrushStyle.Backslash:
                    s = "Backslash";
                    break;
            }
            return s;
        }

        public static BrushStyle StringToBrushStyle(string s)
        {
            BrushStyle ret;

            switch (s)
            {
                case "Square":
                default:
                    ret = BrushStyle.Square;
                    break;
                case "Diamond":
                    ret = BrushStyle.Diamond;
                    break;
                case "Circle":
                    ret = BrushStyle.Circle;
                    break;
                case "VBar":
                    ret = BrushStyle.VBar;
                    break;
                case "HBar":
                    ret = BrushStyle.HBar;
                    break;
                case "Slash":
                    ret = BrushStyle.Slash;
                    break;
                case "Backslash":
                    ret = BrushStyle.Backslash;
                    break;

            }

            return ret;
        }

        private static float[,] CreateCircleBrush(int diameter)
        {
            // Create a new brush using Bresenham's alogrithm
            // to fill an inner filled circle (size = diameter)
            // and then fill the corners outside of the circle
            // with diminishing strength.
            float[,] localBrush = new float[diameter, diameter];

            // Special case for degenerate circle when brushWidth==3
            if (diameter == 3)
            {
                localBrush = new float[,] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };
                return localBrush;
            }

            int radius = diameter/2;
            int originX = radius;
            int originZ = radius;

            // If the diameter is an even number, then we'll
            // actually have four origins - one for each quadrant
            bool even = (diameter & 0x1) == 0;

            // The quadrants are:
            //   2 | 1
            //   -----
            //   3 | 4
            //
            // If the diameter is odd, then the origin is at the center.
            // Otherwise, the origin needs to be offcenter to get four
            // way symmetry between the quadrants.
            //
            // So we'll define the origins for each quadrant here.  Keep 
            // in mind that X increases from left to right starting at 0
            // and Z increased from top to bottom.
            int originX1 = even ? originX - 1 : originX;
            int originX2 = originX;
            int originX3 = originX;
            int originX4 = even ? originX - 1 : originX;

            int originZ1 = even ? originZ - 1 : originZ;
            int originZ2 = even ? originZ - 1 : originZ;
            int originZ3 = originZ;
            int originZ4 = originZ;

            int x = -1;
            int z = radius;
            int distance = 1 - radius;
            int delta_e = -1;
            int delta_se = (-radius << 1) + 3;

            while (z > x)
            {
                delta_e += 2;
                x++;

                if (distance < 0)
                {
                    distance += delta_e;
                    delta_se += 2;
                }
                else
                {
                    distance += delta_se;
                    delta_se += 4;
                    z--;
                }

                // Fill out quadrants

                // +X/+Z quadrant (quadrant 1)
                FillInCircleBrushLine(localBrush, originX1, x, originZ1 + z);
                FillInCircleBrushLine(localBrush, originX1, z, originZ1 + x);

                // -X/+Z quadrant (quadrant 2)
                FillInCircleBrushLine(localBrush, originX2, - x, originZ2 + z);
                FillInCircleBrushLine(localBrush, originX2, - z, originZ2 + x);

                // -X/-Z quadrant (quadrant 3)
                FillInCircleBrushLine(localBrush, originX3, - z, originZ3 - x);
                FillInCircleBrushLine(localBrush, originX3, - x, originZ3 - z);

                // +X/-Z quadrant (quadrant 4)
                FillInCircleBrushLine(localBrush, originX4, + z, originZ4 - x);
                FillInCircleBrushLine(localBrush, originX4, + x, originZ4 - z);
            }

            return localBrush;
        }

        private static void FillInCircleBrushLine(float[,] circleBrush, int originX, int relX, int z)
        {
            // Special case for degenerate circle (brushWidth==1)
            if (z < 0 || z >= circleBrush.GetLength(1))
            {
                return;
            }

            int startX = (originX < originX + relX) ? originX : originX + relX;
            int endX =   (originX > originX + relX) ? originX : originX + relX; 

            // Handle special case for degenerate circle (brushWidth==1)
            if (startX < 0)
            {
                startX = 0;
            }

            if (endX >= circleBrush.GetLength(0))
            {
                endX = circleBrush.GetLength(0) - 1;
            }

            // Go from startX to endX inclusive
            for (int x = startX; x <= endX; x++)
            {
                circleBrush[x, z] = 1f;
            }
        }

        private static float[,] CreateGaussianCircleBrush(int diameter)
        {
            float[,] localBrush = new float[diameter,diameter];

            // If the diameter is an even number, then we'll
            // actually have four origins - one for each quadrant

            int radius = diameter / 2;
            bool even = (diameter & 0x1) == 0;

            // The quadrants are:
            //   2 | 1
            //   -----
            //   3 | 4
            //
            // If the diameter is odd, then the origin is at the center.
            // Otherwise, the origin needs to be offcenter to get four
            // way symmetry between the quadrants.
            //
            // So we'll define the origins for each quadrant here.  Keep 
            // in mind that X increases from left to right starting at 0
            // and Z increased from top to bottom.
            int originX = radius;
            int originZ = radius;
            int originEvenX = even ? originX - 1 : originX;
            int originEvenZ = even ? originZ - 1 : originZ;

            double spread = 2d*(radius*0.8d)*(radius*0.8d);

            for (int x = 0; x < diameter; x++)
            {
                double xo = (x < radius) ? originEvenX : originX;

                for (int z = 0; z < diameter; z++)
                {
                    double zo = (z < radius) ? originEvenZ : originZ;

                    // hardness(x,y) = e^-( ((x-xo)^2) / 2*(radius^2)) 
                    //                     +((y-yo)^2) / 2*(radius^2))
                    //                    )
                    double posXsquared = (x - xo)*(x - xo);
                    double posZsquared = (z - zo)*(z - zo);

                    // If the diameter of the circle is 1, then then spread will
                    // equal 0, so we need to prevent a divide by 0 case.  We'll
                    // set the hardness to one in this case.
                    double hardness = spread == 0f ? 1f : Math.Exp(-(posXsquared + posZsquared)/spread);
                    localBrush[x, z] = (float) hardness;
                }
            }

            return localBrush;
        }

        private static float[,] CombineBrushes(float[,] brush1, float[,] brush2)
        {
            if (brush1.GetLength(0) != brush2.GetLength(0) ||
                brush1.GetLength(1) != brush2.GetLength(1))
            {
                throw new ArgumentException("Brushes must be the same size");
            }

            float[,] combined = new float[brush1.GetLength(0),brush1.GetLength(1)];
            for (int x=0; x < combined.GetLength(0); x++)
            {
                for (int z=0; z < combined.GetLength(1); z++)
                {
                    combined[x, z] = brush1[x, z]*brush2[x, z];
                }
            }

            return combined;
        }
    }

    public enum BrushStyle
    {
        Square,
        Diamond,
        Circle,
        VBar,
        HBar,
        Slash,
        Backslash
    }
}
