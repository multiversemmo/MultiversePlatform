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
using System.Drawing;
using System.Diagnostics;

using log4net;

using Axiom.Core;
using Axiom.Input;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;

using TimeTool = Multiverse.Utility.TimeTool;

namespace Multiverse.Gui {

    //public delegate void CharacterEventHandler(object sender, CharEventArgs e);

    //public class CharEventArgs : InputEventArgs {
    //    char c;

    //    public CharEventArgs(char c, ModifierKeys modifiers) : base(modifiers) {
    //        this.c = c;
    //    }

    //    public char Character {
    //        get {
    //            return c;
    //        }
    //    }
    //}

    // This class is designed along the same principal as the Cegui code.

    public class QuadInfo : IComparable<QuadInfo> {

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(QuadInfo));

        public Texture texture;
        public float z;
        public PointF[] points = new PointF[4];
        public PointF[] texCoords = new PointF[4];
        public uint[] colors = new uint[4];
        public Rect clipRect = null;

        public QuadInfo(Texture texture, float in_z, PointF[] in_points, PointF[] texCoords, uint[] colors, Rect clipRect, Rect displayArea, PointF texelOffset)
        {
            this.texture = texture;
            this.z = -1 + in_z; // convert z coordinates into some subset of the -1 to +1 space that is near the -1
            Debug.Assert(this.z >= -1 && this.z <= 1);
            points = in_points;
            // Convert our points into screen space (where x and y range from -1 to 1)
            for (int i = 0; i < points.Length; ++i) {
//                points[i].X = -1 + (points[i].X) / (displayArea.Width * 0.5f);
//                points[i].Y = -1 + (displayArea.Height - points[i].Y) / (displayArea.Height * 0.5f);
                points[i].X = -1 + (points[i].X + texelOffset.X) / (displayArea.Width * 0.5f);
                points[i].Y = -1 + (displayArea.Height - (points[i].Y + texelOffset.Y)) / (displayArea.Height * 0.5f);
            }
            if (log.IsDebugEnabled && points.Length == 4)
                log.DebugFormat("Created quad: {0}, {1}, {2}, {3}", points[0], points[1], points[2], points[3]);
            this.texCoords = texCoords;
            this.colors = colors;
            this.clipRect = clipRect;
        }

        public void PopulateQuadVertices(QuadVertex[] vertices, int offset) {
            int[] srcIndices = { 2, 3, 0, 3, 1, 0 };
            int dstIndex = offset;
            foreach (int srcIndex in srcIndices) {
                vertices[dstIndex].x = points[srcIndex].X;
                vertices[dstIndex].y = points[srcIndex].Y;
                vertices[dstIndex].z = z;
                vertices[dstIndex].color = colors[srcIndex];
                vertices[dstIndex].u = texCoords[srcIndex].X;
                vertices[dstIndex].v = texCoords[srcIndex].Y;
                dstIndex++;
            }
        }

        public int CompareTo(QuadInfo other) {
            int rv = 0;
            rv = - z.CompareTo(other.z);
            if (rv != 0)
                return rv;
            rv = texture.Handle.CompareTo(other.texture.Handle);
            if (rv != 0)
                return rv;
            if (other.clipRect == null && clipRect == null)
                return 0;
            else if (other.clipRect == null)
                return 1;
            else if (clipRect == null)
                return -1;
            return clipRect.GetHashCode().CompareTo(other.clipRect.GetHashCode());
        }
    }

    ///<summary>
    ///   This class holds the quads for a single Window object which
    ///   have the same z, texture and clip rectangle.
    ///</summary>
    public class QuadBucket : IComparable<QuadBucket> {
        public float z;
        public int textureHandle;
        public int clipRectHash;
        public List<QuadInfo> quads;
        public List<RenderBatch> bucketRenderBatches = new List<RenderBatch>();
        public static int ownsRenderBatchesThreshold = 20;
        public Window window;

        ///<summary>
        ///    If the window has survived without rebuilding for this
        ///    many frames, we consider it to be segregated from those
        ///    that are newer, and not put in the same RenderBatches.
        ///</summary>
        private static int OldAgeThreshold = 10;
        
        public QuadBucket(float z, int textureHandle, int clipRectHash, Window window) {
            this.z = z;
            this.textureHandle = textureHandle;
            this.clipRectHash = clipRectHash;
            this.window = window;
            quads = new List<QuadInfo>();
        }

        public override string ToString() {
            return string.Format("QuadBucket[z {0}, textureHandle {1}, clipRectHash {2}, {3} quads, window {4}",
                z, textureHandle, clipRectHash, quads.Count, window.Name);
        }
        
        public static bool Equals(QuadBucket t1, QuadBucket t2) {
            return t1.z == t2.z && t1.textureHandle == t2.textureHandle && t1.clipRectHash == t2.clipRectHash;
        }

        ///<summary>
        ///   Sort the buckets by z descending, by texture, by
        ///   clipRect and then by when the window was last rebuilt
        ///   ascending, leaving the oldest quads earlier in the list
        ///   than the later ones.
        ///</summary>
        public int CompareTo(QuadBucket other) {
            int rv = -z.CompareTo(other.z);
            if (rv != 0)
                return rv;
            rv = textureHandle.CompareTo(other.textureHandle);
            if (rv != 0)
                return rv;
            rv = clipRectHash.CompareTo(other.clipRectHash);
            if (rv != 0)
                return rv;
            return window.rebuildFrame.CompareTo(other.window.rebuildFrame);
        }

        ///<summary>
        ///   Mark the RenderBatches referenced by this QuadBucket to be
        ///   rebuilt, and sever the connection between the QuadQueue and
        ///   any RenderBatches it might have contributed to.
        ///</summary>
        public void RequireRenderBatchRebuilds() {
            if (bucketRenderBatches.Count > 0) {
                foreach (RenderBatch renderBatch in bucketRenderBatches)
                    renderBatch.NeedsRebuild = true;
                bucketRenderBatches.Clear();
            }
        }

        public RenderBatch FirstRenderBatch {
            get
            {
                if (bucketRenderBatches.Count == 0)
                    return null;
                else
                    return bucketRenderBatches[0];
            }
        }
        
        ///<summary>
        ///   The quad bucket is considered long-lived if it's window
        ///   was last rebuilt earlier that the current frame count -
        ///   OldAgeThreshold
        ///</summary>
        public bool LongLived {
            get {
                return (Renderer.Instance.FrameCounter - window.rebuildFrame) > OldAgeThreshold;
            }
        }
        
        ///<summary>
        ///    Based on the the quad count, should this fellow 
        ///    allocate it's own RenderBatch(es)?
        ///</summary>
        public bool ShouldOwnRenderBatches() {
            return quads.Count > ownsRenderBatchesThreshold;
        }
        
    }

    public struct QuadVertex {
        public float x, y, z;
        public uint color;
        public float u, v;
        public static bool SameVertex(QuadVertex v1, QuadVertex v2) {
            return (v1.x == v2.x) && (v1.y == v2.y) && (v1.z == v2.z) && (v1.color == v2.color) && (v1.u == v2.u) && (v1.v == v2.v);
        }
    }

    public class RenderBatch {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RenderBatch));

        // A set of QuadBuckets that make up this batch
        public Dictionary<QuadBucket, int> quadBucketsRendered = new Dictionary<QuadBucket, int>();
        internal QuadVertex[] lastVertices;
        public HardwareVertexBuffer vertexBuffer;
        public RenderOperation renderOp;
        public Material material;
        public TextureUnitState passTextureUnitState;
        public Rect clipRect;
        public int lastFrameUsed = -1;
        protected bool needsRebuild = false;
        protected int vertexCount = 0;
        protected int sequenceNumber;
        protected static int verticesUnchanged = 0;
        protected static int verticesChanged = 0;
        protected static int sequenceNumberCounter = 0;

        public RenderBatch() {
            sequenceNumber = sequenceNumberCounter++;
        }
        
        internal bool VerticesChanged(QuadVertex[] newVertices) {
            bool changed = false;
            int count = newVertices.Length;
            if (lastVertices == null || lastVertices.Length != count)
                changed = true;
            else {
                for (int i=0; i<count; i++) {
                    if (!QuadVertex.SameVertex(lastVertices[i], newVertices[i])) {
                        changed = true;
                        break;
                    }
                }
            }
            if (changed) {
                lastVertices = new QuadVertex[count];
                for (int i=0; i<count; i++)
                    lastVertices[i] = newVertices[i];
            }
            if (changed)
                verticesChanged++;
            else
                verticesUnchanged++;
            return changed;
        }

        public int VertexCount {
            get {
                return vertexCount;
            }
            set {
                vertexCount = value;
            }
        }
        
        public int SequenceNumber {
            get {
                return sequenceNumber;
            }
        }
        
        public bool NeedsRebuild {
            get {
                return needsRebuild;
            }
            set {
                if (needsRebuild != value) {
                    needsRebuild = value;
                }
            }
        }

    }

    /// <summary>
    ///   This class sets up a vertex buffer, which is populated by the QuadInfo objects.
    /// </summary>
    public class Renderer {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Renderer));

        static Renderer instance;

        /// <summary>
        ///   Value to increment z between parts of a widget
        ///   Smallest delta z between two quads
        /// </summary>
        public const float GuiZLayerStep = 1.0f / (2 << 23);
        /// <summary>
        ///   Value to increment z between widgets -- enough for 16 layers
        /// </summary>
        public const float GuiZElementStep = 16 * GuiZLayerStep;

        /// <summary>
        ///   Size of the vertex buffer
        /// </summary>
        const int VertexBufferCapacity = 1024;
        const int VerticesPerQuad = 6;
        const int QuadVertexSize = 24;

        RenderWindow renderWindow;
        RenderQueueGroupID queueId;
        bool postQueue;

        protected int frameCounter;
        protected Window currentWindow = null;
        
        RenderBatch immediateRenderBatch;

        Viewport viewPort;

        protected int dumpFrameCounter = 1000;
        
        /// <summary>
        ///   All render batches, whether in use or free
        /// </summary>
        protected List<RenderBatch> allRenderBatches = new List<RenderBatch>();
        protected List<RenderBatch> freeRenderBatches = new List<RenderBatch>();
        protected List<RenderBatch> activeRenderBatches = new List<RenderBatch>();
        
        /// <summary>
        ///   Because RenderBatches are supposedly expensive to
        ///   create, we allow this number of unused ones to hang
        ///   around after each frame on the theory that some
        ///   subsequent frame might use them.
        /// </summary>
        protected static int maxFreeRenderBatches = 10;

        // This is not the render queue group id.  It is instead used for 
        // grouping the ui pieces so that they can be independently 
        // updated.  These queues are drawn in order, so that when there
        // are multiple queues, the later queues will be drawn after 
        // earlier ones. This means that z-order will not be safe between
        // different queues.  The most common use for this is that the
        // cursor ends up in a different queue, and is drawn after the
        // other ui objects.
        QuadQueue currentQuadQueue;

        internal class QuadQueue {
            internal int number;
            internal List<QuadBucket> quadBuckets = new List<QuadBucket>();

            internal QuadQueue(int number) {
                this.number = number;
            }
        }

        SortedList<int, QuadQueue> quadQueues = new SortedList<int, QuadQueue>();

        SceneManager sceneManager = null;

        static TimingMeter renderOverlayQueueTimer = MeterManager.GetMeter("Render Overlay", "Multiverse.Gui");
        static TimingMeter renderPrimitiveTimer = MeterManager.GetMeter("Render Primitive", "Multiverse.Gui");
        static TimingMeter prepareBuffersTimer = MeterManager.GetMeter("Prepare Buffers", "Multiverse.Gui");
        static TimingMeter sortQuadsTimer = MeterManager.GetMeter("Sort Quads", "Multiverse.Gui");
        static TimingMeter vbufferUpdateTimer = MeterManager.GetMeter("Vertex Write", "Multiverse.Gui");

        public Renderer(RenderWindow renderWindow, RenderQueueGroupID queueId, bool postQueue) {
            Debug.Assert(instance == null);

            this.renderWindow = renderWindow;
            // For now, just grab the first viewport..
            // TODO: Make this configurable, so we can have UI in multiple windows?
            this.viewPort = null; //  renderWindow.GetViewport(0);
            this.queueId = queueId;
            this.postQueue = postQueue;

            immediateRenderBatch = new RenderBatch();
            InitializeRenderBatch(immediateRenderBatch);

            instance = this;

            this.SetQuadQueue(0);

            Root.Instance.FrameStarted += new FrameEvent(this.OnFrameStarted);
        }

        private RenderBatch CreateRenderBatch() {
            RenderBatch renderBatch = new RenderBatch();
            InitializeRenderBatch(renderBatch);
            allRenderBatches.Add(renderBatch);
            return renderBatch;
        }

        private void InitializeRenderBatch(RenderBatch renderBatch) {
            renderBatch.renderOp = new RenderOperation();
            // Set up my vertex data
            renderBatch.renderOp.vertexData = new VertexData();
            VertexDeclaration decl = renderBatch.renderOp.vertexData.vertexDeclaration;
            int offset = 0;
            VertexElement elem;
            elem = decl.AddElement(0, offset, VertexElementType.Float3, VertexElementSemantic.Position);
            offset += elem.Size;
            elem = decl.AddElement(0, offset, VertexElementType.Color, VertexElementSemantic.Diffuse);
            offset += elem.Size;
            elem = decl.AddElement(0, offset, VertexElementType.Float2, VertexElementSemantic.TexCoords);

            //vertexBuffer =
            //    HardwareBufferManager.Instance.CreateVertexBuffer(decl.GetVertexSize(0), VertexBufferCapacity, BufferUsage.WriteOnly, true);
            renderBatch.vertexBuffer =
                HardwareBufferManager.Instance.CreateVertexBuffer(decl.GetVertexSize(0), VertexBufferCapacity, BufferUsage.DynamicWriteOnly, false);

            renderBatch.renderOp.vertexData.vertexBufferBinding.SetBinding(0, renderBatch.vertexBuffer);
            // This means we need 50% more vertices, but don't need to manage an index buffer
            // TODO: it may be faster to maintain an index buffer.
            renderBatch.renderOp.operationType = OperationType.TriangleList;
            renderBatch.renderOp.useIndices = false;

            renderBatch.material = new Material();
            Technique tech = renderBatch.material.CreateTechnique();
            Pass matPass = tech.CreatePass();
            // TODO: World/View/Projection matrix
            matPass.LightingEnabled = false;
            // Since we probably have translucent widgets, we can't do the 
            // depth test or depth write.
            matPass.DepthCheck = false;
            matPass.DepthWrite = false;
            matPass.CullMode = CullingMode.None;
            matPass.SetFog(true, FogMode.None);
            matPass.SetSceneBlending(SceneBlendFactor.SourceAlpha, SceneBlendFactor.OneMinusSourceAlpha);

            renderBatch.passTextureUnitState = matPass.CreateTextureUnitState();
            renderBatch.passTextureUnitState.TextureAddressing = TextureAddressing.Clamp;

            renderBatch.material.Load();
        }

        public void AddQuad(PointF[] destPoints, float z, Rect clipRect, 
                            Texture texture, PointF[] texturePoints, ColorRect colors) {
            viewPort = renderWindow.GetViewport(0);
            // Do we need to use a scissor test?
            bool allIn = true;
            if (clipRect != null) {
                
                for (int i = 0; i < destPoints.Length; ++i) {
                    if (destPoints[i].X < clipRect.Left || destPoints[i].X > clipRect.Right ||
                        destPoints[i].Y < clipRect.Top || destPoints[i].Y > clipRect.Bottom)
                        allIn = false;
                }
                if (!allIn) {
                    // It's possible that there is no overlap
                    // If so, either all points being added are on the out-side
                    // of the rectangle, or all the points in the rectangle are
                    // on the out-side of one of the edges of my polygon
                    // Flags for whether the given edge can divide us
                    bool left = true;
                    bool right = true;
                    bool bottom = true;
                    bool top = true;
                    for (int i = 0; i < destPoints.Length; ++i) {
                        if (destPoints[i].X > clipRect.Left)
                            left = false;
                        if (destPoints[i].X < clipRect.Right)
                            right = false;
                        if (destPoints[i].Y > clipRect.Top)
                            top = false;
                        if (destPoints[i].Y < clipRect.Bottom)
                            bottom = false;
                    }
                    if (top || bottom || left || right)
                        // our polygon is entirely to one side of the rectangle
                        return; // no need to draw
                    // TODO: If I ever start passing in anything other than a 
                    // rectangle in my list of points, I need to check the 
                    // rectangle's projection onto the edges of my polygon.
                }
            }
            QuadInfo quadInfo;

            RenderSystem renderSystem = sceneManager.TargetRenderSystem;
            uint[] quadColors = new uint[4];
            quadColors[0] = renderSystem.ConvertColor(colors.TopLeft);
            quadColors[1] = renderSystem.ConvertColor(colors.TopRight);
            quadColors[2] = renderSystem.ConvertColor(colors.BottomLeft);
            quadColors[3] = renderSystem.ConvertColor(colors.BottomRight);

            Rect drawRect = new Rect(viewPort.ActualLeft, viewPort.ActualLeft + viewPort.ActualWidth, viewPort.ActualTop, viewPort.ActualTop + viewPort.ActualHeight);
            // TODO: I think I need to do something with the texel offsets, but I'm not sure
            // I'm doing the right thing here.
            // Point texelOffset = new Point(0, 0);
            PointF texelOffset = new PointF(renderSystem.HorizontalTexelOffset, renderSystem.VerticalTexelOffset);
            if (allIn)
                // no need to use scissor test
                quadInfo = new QuadInfo(texture, z, destPoints, texturePoints, quadColors, null, drawRect, texelOffset);
            else
                // needs the scissor test
                quadInfo = new QuadInfo(texture, z, destPoints, texturePoints, quadColors, clipRect, drawRect, texelOffset);
            currentWindow.AddQuad(quadInfo);
        }

        private void PrepareQuads() {
            activeRenderBatches.Clear();
            // Run through the various queues and render the quads
            foreach (QuadQueue quadQueue in quadQueues.Values)
                PrepareQuads(quadQueue);
//             log.DebugFormat("Renderer.PrepareQuads: activeRenderBatches.Count {0}, freeRenderBatches.Count {1}, allRenderBatches.Count {2}",
//                 activeRenderBatches.Count, freeRenderBatches.Count, allRenderBatches.Count);
            foreach (RenderBatch renderBatch in activeRenderBatches)
                renderBatch.lastFrameUsed = frameCounter;
            immediateRenderBatch.lastFrameUsed = frameCounter;
            RemoveExcessRenderBatches();
        }
        
        private void RemoveExcessRenderBatches() {
            List<RenderBatch> onesToRemove = null;
            int i = 0;
            foreach (RenderBatch renderBatch in allRenderBatches) {
                if (freeRenderBatches.Contains(renderBatch))
                    continue;
                if (renderBatch.lastFrameUsed < frameCounter) {
                    log.DebugFormat("Renderer.RemoveExcessRenderBatches: Freeing RenderBatch #{0} {1} QuadBuckets, quad count {2} because lastFrameUsed {3} != frameCounter {4}",
                        renderBatch.SequenceNumber, renderBatch.quadBucketsRendered.Count, renderBatch.VertexCount / VerticesPerQuad, renderBatch.lastFrameUsed, frameCounter);
                    i++;
                    renderBatch.quadBucketsRendered.Clear();
                    if (freeRenderBatches.Count < maxFreeRenderBatches)
                        freeRenderBatches.Add(renderBatch);
                    else {
                        if (onesToRemove == null)
                            onesToRemove = new List<RenderBatch>();
                        onesToRemove.Add(renderBatch);
                    }
                }
            }
            if (onesToRemove != null) {
                foreach (RenderBatch renderBatch in onesToRemove)
                    FreeRenderBatch(renderBatch);
            }
        }

        private void PrepareQuads(QuadQueue quadQueue) {
            List<QuadBucket> quadBuckets = quadQueue.quadBuckets;
//             if (frameCounter == dumpFrameCounter)
//                 DumpQuadBuckets(quadQueue.number, "before sorting", quadBuckets);
            if (quadBuckets.Count == 0)
                return;
            sortQuadsTimer.Enter();
            // sort the quads (descending z)
            quadBuckets.Sort();
            sortQuadsTimer.Exit();
//             if (frameCounter == dumpFrameCounter)
//                 DumpQuadBuckets(quadQueue.number, "after sorting", quadBuckets);
            int start = 0;
            int maxQuads = VertexBufferCapacity / VerticesPerQuad;
            int count;
            
            while (start < quadBuckets.Count) {
                int end = FindLastBucketForRange(quadBuckets, start, maxQuads, out count);
                QuadBucket quadBucket = quadBuckets[start];
                if (quadBucket.ShouldOwnRenderBatches()) {
                    // If it was rebuilt in this frame, create the
                    // required render batches.
                    if (quadBucket.window.rebuildFrame == frameCounter) {
                        int batchCount = (count + maxQuads - 1) / maxQuads;
                        quadBucket.RequireRenderBatchRebuilds();
                        for (int i=0; i<batchCount; i++) {
                            int first = i * maxQuads;
                            int countInBatch = i == batchCount - 1 ? count - first : maxQuads;
                            log.DebugFormat("Renderer.PrepareQuads: countInBatch {0}, count {1}, first {2}, batchCount {3}, start {4}, end {5}",
                                countInBatch, count, first, batchCount, start, end);
                            RenderBatch renderBatch = PrepareGroupedQuads(quadBucket.quads.GetRange(first, countInBatch));
                            quadBucket.bucketRenderBatches.Add(renderBatch);
                        }
                    }
                    else
                        activeRenderBatches.AddRange(quadBucket.bucketRenderBatches);
                }
                else {
                    // None of the buckets in the sequence from start
                    // to end owns batches.  Ask if any of them were
                    // rebuilt; if so free the render batch and remake
                    // it.
                    bool rebuilt = false;
                    RenderBatch sharedBatch = quadBuckets[start].FirstRenderBatch;
                    if (sharedBatch == null || sharedBatch.NeedsRebuild)
                        rebuilt = true;
                    else {
                        for (int i=start; i<=end; i++) {
                            quadBucket = quadBuckets[i];
                            RenderBatch nextBatch = quadBucket.FirstRenderBatch;
                            if (quadBucket.window.rebuildFrame == frameCounter ||
                                sharedBatch != nextBatch) {
                                rebuilt = true;
                                break;
                            }
                        }
                    }
                    if (rebuilt) {
                        // Rebuild the RenderBatch
                        List<QuadInfo> quads = new List<QuadInfo>();
                        for (int i=start; i<=end; i++)
                            quads.AddRange(quadBuckets[i].quads);
                        // Record the mapping from QuadBucket <=> RenderBatch
                        RenderBatch batch = PrepareGroupedQuads(quads);
                        for (int i=start; i<=end; i++) {
                            quadBucket = quadBuckets[i];
                            quadBucket.bucketRenderBatches.Clear();
                            quadBucket.bucketRenderBatches.Add(batch);
                            batch.quadBucketsRendered[quadBucket] = 0;
                        }
                    }
                    else {
                        if (sharedBatch == null)
                            log.ErrorFormat("PrepareQuads: The shared render batch for quadBuckets[{0}] {1} is null!", start, quadBuckets[start]);
                        else {
                            for (int i=start+1; i<=end; i++) { 
                                RenderBatch nextBatch = quadBuckets[i].FirstRenderBatch;
                                if (sharedBatch != nextBatch)
                                    log.ErrorFormat("PrepareQuads: The renderbatch for QuadBucket #{0}, window name {1} is not the same as for the first QuadBucket, window name {2}",
                                        i-start, quadBuckets[i].window.Name, quadBuckets[start].window.Name);
                            }
                            if (!activeRenderBatches.Contains(sharedBatch))
                                activeRenderBatches.Add(sharedBatch);
                        }
                    }
                }
                start = end + 1;
            }
        }

        protected int FindLastBucketForRange(List<QuadBucket> quadBuckets, int start, int maxQuads, out int quadCount) {
            quadCount = 0;
            QuadBucket startBucket = quadBuckets[start];
            bool lookingForLongLived = startBucket.LongLived;
            for (int index = start; index < quadBuckets.Count; ++index) {
                QuadBucket quadBucket = quadBuckets[index];
                int count = quadBucket.quads.Count;
                if (quadBucket.ShouldOwnRenderBatches() ||
                    startBucket.textureHandle != quadBucket.textureHandle ||
                    startBucket.clipRectHash != quadBucket.clipRectHash ||
                    quadCount + count > maxQuads ||
                    (lookingForLongLived != quadBucket.LongLived)) {
                    if (start != index)
                        return index - 1;
                    else {
                        quadCount = count;
                        return start;
                    }
                }
                quadCount += count;
            }
            return quadBuckets.Count - 1;
        }

        ///<summary>
        ///   This routine gets rid of a render batch, disposing it's
        ///   vertex buffer
        ///</summary>
        public void FreeRenderBatch(RenderBatch renderBatch) {
//             log.DebugFormat("Renderer.FreeRenderBatch: Before freeing, activeRenderBatches.Count {0}, freeRenderBatches.Count {1}, allRenderBatches.Count {2}",
//                 activeRenderBatches.Count, freeRenderBatches.Count, allRenderBatches.Count);
            renderBatch.quadBucketsRendered.Clear();
            if (freeRenderBatches.Count <= maxFreeRenderBatches)
                freeRenderBatches.Add(renderBatch);
            else {
                allRenderBatches.Remove(renderBatch);
                freeRenderBatches.Remove(renderBatch);
                activeRenderBatches.Remove(renderBatch);
                renderBatch.vertexBuffer.Dispose();
            }
//             log.DebugFormat("Renderer.FreeRenderBatch: After freeing, activeRenderBatches.Count {0}, freeRenderBatches.Count {1}, allRenderBatches.Count {2}",
//                 activeRenderBatches.Count, freeRenderBatches.Count, allRenderBatches.Count);
        }
        
        public void FreeRenderBatches(List<RenderBatch> renderBatches) {
            foreach (RenderBatch renderBatch in renderBatches)
                FreeRenderBatch(renderBatch);
        }
        
        protected void DumpQuadBuckets(int queueNumber, string when, List<QuadBucket> quadBuckets) {
            log.DebugFormat("Dumping quad buckets for queue {0} at frame {1} {2}", queueNumber, frameCounter, when);
            for (int i=0; i<quadBuckets.Count; i++) {
                QuadBucket quadBucket = quadBuckets[i];
                QuadInfo q = quadBucket.quads[0];
                log.DebugFormat("  {0}: {1} quads, z {2}, texture {3}, clipRect {4}, window {5}, ",
                    i, quadBucket.quads.Count, quadBucket.z, q.texture.Name, ClipRectString(q.clipRect), quadBucket.window.Name);
            }
        }

        protected string ClipRectString(Rect r) {
            if (r == null)
                return "None";
            else
                return string.Format("l {0} t {1} r {2}, b {3}", r.Left, r.Top, r.Right, r.Bottom);
        }
        
        public void AddQuadBuckets(List<QuadBucket> quadBuckets) {
            currentQuadQueue.quadBuckets.AddRange(quadBuckets);
        }
        
        private RenderBatch PrepareGroupedQuads(List<QuadInfo> quadList) {
//             log.DebugFormat("Renderer.PrepareGroupedQuads: Before preparing, activeRenderBatches.Count {0}, freeRenderBatches.Count {1}, allRenderBatches.Count {2}",
//                 activeRenderBatches.Count, freeRenderBatches.Count, allRenderBatches.Count);
            RenderBatch nextBatch = null;            
            if (freeRenderBatches.Count > 0) {
                nextBatch = freeRenderBatches[0];
                freeRenderBatches.RemoveAt(0);
            }
            else
                nextBatch = CreateRenderBatch();
            activeRenderBatches.Add(nextBatch);
            PrepareGroupedQuads(nextBatch, quadList);
            if (nextBatch == null)
                log.Error("Renderer.PrepareGroupedQuads: nextBatch is null!");
//             log.DebugFormat("Renderer.PrepareGroupedQuads: After preparing, activeRenderBatches.Count {0}, freeRenderBatches.Count {1}, allRenderBatches.Count {2}",
//                 activeRenderBatches.Count, freeRenderBatches.Count, allRenderBatches.Count);
            return nextBatch;
        }

        /// <summary>
        ///   Prepare a batch of quads that share a common texture, and
        ///   the same clip parameters.
        ///   The quads in this batch should already be sorted by z order
        ///   (back to front).
        /// </summary>
        /// <param name="quadList"></param>
        private void PrepareGroupedQuads(RenderBatch renderBatch, List<QuadInfo> quadList) {
            int offset = 0;
            QuadVertex[] vertices = new QuadVertex[quadList.Count * VerticesPerQuad];
            for (int i = 0; i < quadList.Count; ++i) {
                QuadInfo quadInfo = quadList[i];
                quadInfo.PopulateQuadVertices(vertices, offset);
                offset += VerticesPerQuad;
            }
            renderBatch.VertexCount = offset;
            PrepareVertices(renderBatch, vertices, quadList[0].texture, quadList[0].clipRect);
        }

        /// <summary>
        ///   Variant of the render quad methods that renders a single
        ///   quad directly (not using the queues or sorting)
        /// </summary>
        /// <param name="quadInfo"></param>
        private void RenderQuadDirect(QuadInfo quadInfo)
        {
            QuadVertex[] vertices = new QuadVertex[6];
            quadInfo.PopulateQuadVertices(vertices, 0);
            PrepareVertices(immediateRenderBatch, vertices, quadInfo.texture, quadInfo.clipRect);
        }
        /// <summary>
        ///   Writes the data into the renderBatch
        /// </summary>
        /// <param name="renderBatch"></param>
        /// <param name="vertices"></param>
        /// <param name="texture"></param>
        /// <param name="clipRect"></param>
        internal void PrepareVertices(RenderBatch renderBatch, QuadVertex[] vertices, Texture texture, Rect clipRect) {
            if (renderBatch.VerticesChanged(vertices)) {
                // vbufferUpdateTimer.Enter();
                renderBatch.vertexBuffer.WriteData(0, vertices.Length * QuadVertexSize, vertices, true);
                // vbufferUpdateTimer.Exit();
            }
            renderBatch.renderOp.vertexData.vertexCount = vertices.Length;

            if (texture != null)
                renderBatch.passTextureUnitState.SetTextureName(texture.Name);
            else
                renderBatch.passTextureUnitState.SetTextureName("white.dds");
            renderBatch.clipRect = clipRect;
        }

        internal void RenderRenderBatch(RenderBatch renderBatch) {
            if (renderBatch.clipRect != null)
                sceneManager.TargetRenderSystem.SetScissorTest(true, 
                                                               (int)renderBatch.clipRect.Left, 
                                                               (int)renderBatch.clipRect.Top, 
                                                               (int)renderBatch.clipRect.Right, 
                                                               (int)renderBatch.clipRect.Bottom);
            renderPrimitiveTimer.Enter();
            sceneManager.ManualRender(renderBatch.renderOp, renderBatch.passTextureUnitState.Parent, viewPort, 
                                      Matrix4.Identity, Matrix4.Identity, Matrix4.Identity);
            renderPrimitiveTimer.Exit();
            if (renderBatch.clipRect != null)
                sceneManager.TargetRenderSystem.SetScissorTest(false);
        }

        public void SetSceneManager(SceneManager scene) {
            if (sceneManager != null) {
                sceneManager.QueueStarted -= new RenderQueueEvent(sceneManager_OnQueueStarted);
                sceneManager.QueueEnded -= new RenderQueueEvent(sceneManager_OnQueueEnded);
            }
            sceneManager = scene;
            if (sceneManager != null) {
                sceneManager.QueueStarted += new RenderQueueEvent(sceneManager_OnQueueStarted);
                sceneManager.QueueEnded += new RenderQueueEvent(sceneManager_OnQueueEnded);
            }
        }

        private void RenderGui()
        {
            renderOverlayQueueTimer.Enter();
            // render each batch
            //Pass matPass = passTextureUnitState.Parent;
            //sceneManager.ManualRender(renderOp, matPass, viewPort, 
            //                          Matrix4.Identity, Matrix4.Identity, Matrix4.Identity);
            RenderBatches();
            // RenderQuads();
            
            // TODO: Render the cursor

            renderOverlayQueueTimer.Exit();
        }

        protected void RenderBatches() {
            foreach (RenderBatch renderBatch in activeRenderBatches)
                RenderRenderBatch(renderBatch);
        }

        public void SetQuadQueue(int quadQueueId) {
            if (!quadQueues.ContainsKey(quadQueueId))
                quadQueues.Add(quadQueueId, new QuadQueue(quadQueueId));
            currentQuadQueue = quadQueues[quadQueueId];
        }

        public void ClearCurrentQueueBuckets() {
            currentQuadQueue.quadBuckets.Clear();
        }

        private void OnFrameStarted(object sender, FrameEventArgs args) {
            try {
                GuiSystem.Instance.PrepareGui();
                prepareBuffersTimer.Enter();
                PrepareQuads();
                prepareBuffersTimer.Exit();
            }
            catch (Exception e) {
                log.WarnFormat("Renderer.OnFrameStarted: Exception {0}, stack trace {1}", e.Message, e.StackTrace);
            }
        }

        private bool sceneManager_OnQueueStarted(RenderQueueGroupID priority) {
            // For optimal efficiency, I should really render all my opaque 
            // widgets before drawing the rest of the scene, then draw the 
            // rest of the scene, then draw my transparent/translucent widgets.
            // For now, I just render them all at the same time.
            if ((queueId == priority) && sceneManager.CurrentViewport.OverlaysEnabled &&
                sceneManager.IlluminationStage != IlluminationRenderStage.RenderToTexture)
                if (!postQueue)
                    RenderGui();
            return false;
        }

        private bool sceneManager_OnQueueEnded(RenderQueueGroupID priority) {
            if ((queueId == priority) && sceneManager.CurrentViewport.OverlaysEnabled &&
                sceneManager.IlluminationStage != IlluminationRenderStage.RenderToTexture)
                if (postQueue)
                    RenderGui();
            return false;
        }

        public static Renderer Instance {
            get {
                return instance;
            }
        }

        public Window CurrentWindow {
            get {
                return currentWindow;
            }
            set {
                currentWindow = value;
            }
        }

        public int FrameCounter {
            get {
                return frameCounter;
            }
            set {
                frameCounter = value;
            }
        }

    }


    public class WindowManager
    {
        Dictionary<string, Window> windows = new Dictionary<string, Window>();
        static WindowManager instance;

        public void AttachWindow(Window win)
        {
            windows[win.Name] = win;
        }
        public void DestroyWindow(string name)
        {
            windows.Remove(name);
        }
        public void DestroyWindow(Window window)
        {
            foreach (KeyValuePair<string, Window> kvp in windows)
            {
                if (kvp.Value == window)
                {
                    windows.Remove(kvp.Key);
                    return;
                }
            }
        }

        public static WindowManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new WindowManager();
                return instance;
            }
        }
    }

    /// <summary>
    ///   This class is responsible for much of the ui system behavior.  
    ///   GuiSystem does not know about the xml ui system, and deals with 
    ///   Window objects instead of Frame objects.
    ///   This class also maintains information about which keys are pressed,
    ///   and handles keeping the cursor position updated.
    ///   Key events are passed to the Window objects, which can handle keys 
    ///   directly.  Frame objects (e.g. EditBox) have some window which gets 
    ///   the events and handles them.
    /// </summary>
    public class GuiSystem
    {
        #region Fields

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(GuiSystem));

        float mouseX;
        float mouseY;

        Window currentCursor;
        Window defaultCursor;
        Window rootWindow;
        Window activeWindow; // this is the window that gets keyboard events
        Renderer renderer;
        bool hardwareCursor = true;

        ModifierKeys modifierKeys = ModifierKeys.None;
        List<KeyCodes> pressedKeys = new List<KeyCodes>();
        KeyCodes lastKey;
        ModifierKeys lastModifierKeys = ModifierKeys.None;
        bool lastKeyDown = false;
        long lastKeyDownTime = 0;
        long lastKeyRepeatTime = 0;

        int ticksBeforeRepeat = 500;
        int ticksPerRepeat = 100;

        KeyboardEventHandler keyUpHandler = null;
        KeyboardEventHandler keyDownHandler = null;
        MouseEventHandler mouseUpHandler = null;
        MouseEventHandler mouseDownHandler = null;

        static GuiSystem instance;

        static TimingMeter prepareOverlayQueueTimer = MeterManager.GetMeter("Prepare Overlay", "Multiverse.Gui");
        static TimingMeter drawRootTimer = MeterManager.GetMeter("Draw Root Window", "Multiverse.Gui");

        #endregion Fields

        public GuiSystem(Renderer renderer) {
            this.renderer = renderer;

            Debug.Assert(instance == null);

            instance = this;
        }

        #region Methods

       
        public static bool PointInArea(PointF pt, Rect area) {
            if (pt.X < area.Left || pt.X > area.Right)
                return false;
            if (pt.Y < area.Top || pt.Y > area.Bottom)
                return false;
            return true;
        }

        public static Window GetWindowAtPoint(Window win, PointF pt) {
            for (int i = 0; i < win.ChildCount; ++i) {
                Window child = win.GetChildAtIndex(i);
                if (PointInArea(pt, child.DerivedArea) && child.Visible)
                    return GetWindowAtPoint(child, pt);
            }
            return win;
        }
        //public static bool IsModifierKeySet(ModifierKeys keys, ModifierKeys check) {
        //    return (keys & check) == check;
        //}
        public static bool IsMouseButtonSet(MouseButtons buttons, MouseButtons check) {
            return (buttons & check) != 0;
        }

        protected void UpdatePressedKeys(KeyCodes k, ModifierKeys mods, bool down) {
            if (down) {
                lastKey = k;
                lastModifierKeys = mods;
                lastKeyDown = true;
                lastKeyDownTime = TimeTool.CurrentTime;
                lastKeyRepeatTime = 0;
            } else if (lastKey == k)
                lastKeyDown = false;

            if (down && !pressedKeys.Contains(k))
                pressedKeys.Add(k);
            else if (!down && pressedKeys.Contains(k))
                pressedKeys.Remove(k);
        }

        protected void UpdateModifierKeys(ModifierKeys key, bool down) {
            switch (key) {
                case ModifierKeys.Alt:
                case ModifierKeys.Shift:
                case ModifierKeys.Control:
                    if (down)
                        modifierKeys |= key;
                    else
                        modifierKeys &= ~key;
                    Debug.Assert(modifierKeys >= 0);
                    break;
            }
        }
        protected void UpdateModifierKeys(KeyCodes k, bool down) {
            switch (k) {
                case KeyCodes.LeftAlt:
                case KeyCodes.RightAlt:
                    UpdateModifierKeys(ModifierKeys.Alt, down);
                    break;
                case KeyCodes.LeftControl:
                case KeyCodes.RightControl:
                    UpdateModifierKeys(ModifierKeys.Control, down);
                    break;
                case KeyCodes.LeftShift:
                case KeyCodes.RightShift:
                    UpdateModifierKeys(ModifierKeys.Shift, down);
                    break;
            }
        }

        public bool InjectMouseUp(MouseButtons b, Window win) {
            if (win != null) {
                MouseEventArgs args = CreateMouseEventArgs(b, modifierKeys);
                while (win != null && !args.Handled) {
                    win.OnMouseUp(args);
                    win = win.Parent;
                }
                return args.Handled;
            }
            return false;
        }
        public bool InjectMouseDown(MouseButtons b, Window win) {
            if (win != null) {
                MouseEventArgs args = CreateMouseEventArgs(b, modifierKeys);
                while (win != null && !args.Handled) {
                    win.OnMouseDown(args);
                    win = win.Parent;
                }
                return args.Handled;
            }
            return false;
        }
        public bool InjectMouseMove(float X, float Y, float relX, float relY)
        {
            mouseX = X;
            mouseY = Y;
            //mouseX += relX;
            //mouseY += relY;
            if (mouseX < 0)
                mouseX = 0;
            else if (mouseX > rootWindow.Width)
                mouseX = rootWindow.Width - 1;
            if (mouseY < 0)
                mouseY = 0;
            else if (mouseY > rootWindow.Height)
                mouseY = rootWindow.Height - 1;
            if (currentCursor != null)
                currentCursor.Position = this.MousePosition;
            return false;
        }

        /// <summary>
        ///   Update our concept of modifier keys and which keys are pressed.
        ///   Dispatch the key down to the activeWindow and its ancestors 
        ///   until it has been handled.
        ///   If it isn't handled by that, dispatch to the widget system
        ///   Typically, the widget system will just apply the bindings.
        ///   If it still isn't handled, generate a key press event.
        /// </summary>
        /// <param name="args"></param>
        public void OnKeyDown(KeyEventArgs args)
        {
            log.DebugFormat("OnKeyDown: {0}; {1}; {2}", args.Key, args.Modifiers, modifierKeys);
            UpdateModifierKeys(args.Key, true);
            UpdatePressedKeys(args.Key, args.Modifiers, true);

            if (activeWindow != null) {
                Window win = activeWindow.ActiveChild;
                while (win != null && !args.Handled) {
                    win.OnKeyDown(args);
                    win = win.Parent;
                }
            }
            // Should we go on and generate an OnKeyPress event?
            bool doKeyPress = true;
            // If our KeyDown event was not consumed by a widget, pass it to 
            // the callback registered by the Interface system
            if (!args.Handled && keyDownHandler != null) {
                // The keyDownHandler code doesn't deal with
                // KeyPress events, so if we handled the KeyDown
                // event, don't go on to pass a KeyPress event.
                keyDownHandler(this, args);
                if (args.Handled)
                    doKeyPress = false;
            }

            // Now generate the corresponding KeyPress event
            if (doKeyPress) {
                KeyEventArgs pressArgs = new KeyEventArgs(args.Key, args.Modifiers);
                OnKeyPress(pressArgs);
                args.Handled |= pressArgs.Handled;
            }
        }

        /// <summary>
        ///   Dispatch the key press to the activeWindow and its ancestors 
        ///   until it has been handled.
        /// </summary>
        /// <param name="args"></param>
        protected void OnKeyPress(KeyEventArgs args) {
            if (activeWindow != null) {
                // Windows has the concept of deciding whether to raise
                // OnKeyPress events based on whether the character 
                // passes the IsInputChar test.
                // Right now, I don't replicate this concept (though the
                // Axiom code has some framework with the GetKeyChar concept.
                // If I used that, I could not get key repeat on special
                // characters such as backspace and arrow keys.

                //char c = InputReader.GetKeyChar(k, modifierKeys);
                //if (c == char.MinValue)
                //    return true;  

                Window win = activeWindow.ActiveChild;
                while (win != null && !args.Handled) {
                    win.OnKeyPress(args);
                    win = win.Parent;
                }
            }
        }

        /// <summary>
        ///   Dispatch the key up to the activeWindow and its ancestors 
        ///   until it has been handled.
        ///   If it isn't handled by that, dispatch to the Interface system
        ///   Typically, the widget system will just apply the bindings.
        ///   Finally update our concept of modifier keys and which keys are 
        ///   pressed (even it has been handled).
        /// </summary>
        /// <param name="args"></param>
        public void OnKeyUp(KeyEventArgs args)
        {
            if (activeWindow != null) {
                Window win = activeWindow.ActiveChild;
                while (win != null && !args.Handled) {
                    win.OnKeyUp(args);
                    win = win.Parent;
                }
            }
            // If our KeyUp event was not consumed by a widget, pass it to 
            // the callback registered by the Interface system
            if (!args.Handled && keyUpHandler != null) 
                keyUpHandler(this, args);

            UpdateModifierKeys(args.Key, false);
            UpdatePressedKeys(args.Key, args.Modifiers, false);
        }

        /// <summary>
        ///   Unlike the key handlers, we don't really do much logic here.
        ///   This is because the key handlers send key events to the window
        ///   that contains the focus, which does not rely on the Interface
        ///   system.  The mouse events only really make sense for the higher
        ///   level objects in the Interface system, so we just dispatch them
        ///   there.  Even the OnClick concept is tied to getting a MouseDown
        ///   followed by a MouseUp where both events are in the area controlled
        ///   by the Interface element, so this needs to be handled in the
        ///   Interface system.
        /// </summary>
        /// <param name="args"></param>
        public void OnMouseDown(MouseEventArgs args) {
            // If our MouseDown event was not consumed by a widget, pass it to 
            // the callback registered by the Interface system
            if (!args.Handled && mouseDownHandler != null) {
                // The mouseDownHandler code doesn't deal with
                // MouseClick events, so if we handled the MouseDown
                // event, don't go on to pass a MouseClick event.
                mouseDownHandler(this, args);
            }
        }

        /// <summary>
        ///   Unlike the key handlers, we don't really do much logic here.
        ///   This is because the key handlers send key events to the window
        ///   that contains the focus, which does not rely on the Interface
        ///   system.  The mouse events only really make sense for the higher
        ///   level objects in the Interface system, so we just dispatch them
        ///   there.  Even the OnClick concept is tied to getting a MouseDown
        ///   followed by a MouseUp where both events are in the area controlled
        ///   by the Interface element, so this needs to be handled in the
        ///   Interface system.
        /// </summary>
        /// <param name="args"></param>
        public void OnMouseUp(MouseEventArgs args) {
            // If our MouseUp event was not consumed by a widget, pass it to 
            // the callback registered by the Interface system
            if (!args.Handled && keyUpHandler != null)
                mouseUpHandler(this, args);
        }

        public void OnUpdate(long tickCount) {
            // Handle key repeat
            if (lastKeyDown) {
                KeyEventArgs args = new KeyEventArgs(lastKey, lastModifierKeys);
                // has enough time passed that we should repeat keys?
                if (lastKeyRepeatTime == 0) {
                    if (lastKeyDownTime + ticksBeforeRepeat < tickCount) {
                        // This is the initial repeat
                        OnKeyPress(args);
                        lastKeyRepeatTime = lastKeyDownTime + ticksBeforeRepeat;
                    } else {
                        // no key repeat yet
                        return;
                    }
                } 
                if (lastKeyRepeatTime + 10 * ticksPerRepeat < tickCount) {
                    // more than 10 key repeats.. this means something is wrong
                    lastKeyRepeatTime = tickCount;
                }
                // we should go into repeat mode
                while (lastKeyRepeatTime + ticksPerRepeat < tickCount) {
                    OnKeyPress(args);
                    lastKeyRepeatTime += ticksPerRepeat;
                }
            }
        }
        
        ///<summary>
        ///    For all dirty windows, regenerate their QuadBuckets.
        ///</summary>
        public void PrepareGui() {
            prepareOverlayQueueTimer.Enter();
            // Draw the main ui
            renderer.SetQuadQueue(0);
            renderer.ClearCurrentQueueBuckets();
            drawRootTimer.Enter();
            rootWindow.Draw();
            drawRootTimer.Exit();
            // Now draw the cursor
            if (currentCursor != null && !hardwareCursor) {
                renderer.SetQuadQueue(1);
                renderer.ClearCurrentQueueBuckets();
                currentCursor.Draw();
            }
            prepareOverlayQueueTimer.Exit();
        }

        public void SetCursorImage(Window image)
        {
            this.currentCursor = image;
        }
        public void SetDefaultCursor(Window image)
        {
            this.defaultCursor = image;
        }
        public Window GetRootWindow()
        {
            return rootWindow;
        }
        public void SetRootWindow(Window window)
        {
            this.rootWindow = window;

            // Center the mouse
            mouseX = rootWindow.Width / 2;
            mouseY = rootWindow.Height / 2;
        }
        public void CaptureInput(Window window) {
            this.activeWindow = window;
            window.OnCaptureGained(new EventArgs());
            window.MoveToFront();
        }
        public void ReleaseInput(Window window) {
            if (this.activeWindow == window) {
                window.OnCaptureLost(new EventArgs());
                this.activeWindow = null;
            }
        }
        public MouseEventArgs CreateMouseEventArgs(MouseButtons button, ModifierKeys keys) {
            PointF pt = this.MousePosition;
            MouseData mouseData = new MouseData();
            mouseData.button = button;
            mouseData.x = pt.X;
            mouseData.y = pt.Y;
            mouseData.z = 0;
            mouseData.relativeX = 0;
            mouseData.relativeY = 0;
            mouseData.relativeZ = 0;
            return new MouseEventArgs(mouseData, keys);
        }
        public MouseEventArgs CreateMouseEventArgs(MouseButtons button) {
            return CreateMouseEventArgs(button, modifierKeys);
        }
        public MouseEventArgs CreateMouseEventArgs() {
            return CreateMouseEventArgs(MouseButtons.None, modifierKeys);
        }

        /// <summary>
        ///   Use the currentCursor and hardwareCursor settings to set up the 
        ///   cursor properties (enabled/disabled/which image)
        /// </summary>
        protected void UpdateCursorProperties() {
            if (hardwareCursor) {
                if (currentCursor != null) {
                    ImageWindow imageWin = currentCursor as ImageWindow;
                    TextureInfo texInfo = imageWin.Image;
                    Root.Instance.RenderSystem.SetCursor(texInfo.Texture, texInfo.Rectangle);
                } else {
                    Root.Instance.RenderSystem.ClearCursor();
                }
                // TODO: handle the case where we need to clear the hardware cursor
            } else {
                // If we aren't using hardware cursor, 
                if (currentCursor != null)
                    currentCursor.Position = this.MousePosition;
            }
        }

        #endregion Methods

        #region Properties

        public static GuiSystem Instance
        {
            get
            {
                return instance;
            }
        }

        public bool KeyboardCaptured {
            get {
                return activeWindow != null;
            }
        }

        public Window DefaultCursor {
            get {
                return defaultCursor;
            }
            set {
                defaultCursor = value;
            }
        }
        public Window CurrentCursor {
            get {
                return currentCursor;
            }
            set {
                if (currentCursor == value)
                    return;
                currentCursor = value;
                UpdateCursorProperties();
            }
        }
        public bool HardwareCursor {
            get {
                return hardwareCursor;
            }
            set {
                if (hardwareCursor == value)
                    return;
                hardwareCursor = value;
                UpdateCursorProperties();
            }
        }
        public PointF MousePosition
        {
            get
            {
                return new PointF(mouseX, mouseY);
            }
        }
        public SizeF CursorSize
        {
            get
            {
                return currentCursor.Size;
            }
            set
            {
                currentCursor.Size = value;
            }
        }
        public SizeF WindowSize {
            get {
                return new SizeF(rootWindow.Width, rootWindow.Height);
            }
        }
        public Window WindowContainingMouse
        {
            get
            {
                return GetWindowAtPoint(rootWindow, this.MousePosition);
            }
        }

        public KeyboardEventHandler KeyUpHandler {
            //get {
            //    return keyUpHandler;
            //}
            set {
                keyUpHandler = value;
            }
        }
        public KeyboardEventHandler KeyDownHandler {
            //get {
            //    return keyDownHandler;
            //}
            set {
                keyDownHandler = value;
            }
        }
        public MouseEventHandler MouseUpHandler {
            //get {
            //    return mouseUpHandler;
            //}
            set {
                mouseUpHandler = value;
            }
        }
        public MouseEventHandler MouseDownHandler {
            //get {
            //    return mouseDownHandler;
            //}
            set {
                mouseDownHandler = value;
            }
        }
        #endregion Properties
    }
}
