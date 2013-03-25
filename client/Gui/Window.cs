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

using Axiom.MathLib;
using Axiom.Core;
using Axiom.Input;

namespace Multiverse.Gui {
    /// <summary>
    ///   Gui window
    /// </summary>
    public class Window {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Window));
        protected string name;
        protected float alpha = 1.0f;
        protected Window parent;
        protected Window clipParent;
        protected List<Window> children = new List<Window>();
        protected Rect area = new Rect();
        protected ColorRect colors;
        protected SizeF maxSize;
        protected bool visible = false;
        protected float zValue = 0;
        protected bool isActive = false;

        /// <summary>
        ///   The list of QuadBuckets for this window, created when
        ///   the window was last dirtied
        /// </summary>
        protected List<QuadBucket> quadBuckets = new List<QuadBucket>();

        /// <summary>
        ///   The frame number when the window was last dirtied.
        ///   Windows that have been recently dirtied are kept
        ///   separate from windows that were dirtied some time ago,
        ///   so we don't have to constantly regenerate the batches
        ///   that contain windows that were not recently dirtied
        /// </summary>
        public int rebuildFrame = 0;

        /// <summary>
        ///   Set when anyone changes the window, it's properties or
        ///   where it's displayed.
        /// </summary>
        private bool dirty = true;

        public event EventHandler Activated;
        public event EventHandler Deactivated;

        // Basic mouse events
        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseMoved;
        public event MouseEventHandler MouseUp;
        // More complex events (inserted by GuiSystem)
        public event MouseEventHandler MouseClicked;
        public event MouseEventHandler MouseDoubleClicked;

        // Basic key events
        public event KeyboardEventHandler KeyUp;
        public event KeyboardEventHandler KeyDown;
        // More complex key events (inserted by GuiSystem)
        public event KeyboardEventHandler KeyPress;

        // Capture events - inserted by GuiSystem?
        public event EventHandler CaptureGained;
        public event EventHandler CaptureLost;

        public Window(string name) {
            this.name = name;
        }

        public Window() {
            this.name = null;
        }

        // FIXME -- implement Window.Initialize()
        public virtual void Initialize() {
        }
        
        public void AddChild(Window child) {
            // Add the child to the front of the queue
            child.parent = this;
            if (clipParent != null)
                child.clipParent = clipParent;
            children.Insert(0, child);
        }

        public void RemoveChild(Window child) {
            children.Remove(child);
            child.parent = null;
            child.clipParent = null;
        }

        public void RemoveChild(int index) {
            Window child = children[index];
            RemoveChild(child);
        }

        public Window GetChildAtIndex(int index) {
            return children[index];
        }

        public void MoveToFront() {
            Dirty = true;
            if (parent != null)
                parent.MoveToFront();
            // Deactivate any siblings, and activate ourself.
            Window activeSibling = null;
            if (parent != null) {
                for (int i = 0; i < parent.ChildCount; ++i) {
                    Window tmp = parent.GetChildAtIndex(i);
                    if (tmp.IsActive) {
                        activeSibling = tmp;
                        break;
                    }
                }
            }
            if (activeSibling != this) {
                if (parent != null) {
                    Window currentParent = parent;
                    Window currentClipParent = clipParent;
                    // Add and remove our window, so that we are at the front
                    currentParent.RemoveChild(this);
                    currentParent.AddChild(this);
                    // Restore our clipParent
                    clipParent = currentClipParent;
                }
                // Deactivate our sibling, and then activate ourself.
                EventArgs args = new EventArgs();
                if (activeSibling != null)
                    activeSibling.OnDeactivated(args);
                this.OnActivated(args);
            }
        }

        public void Activate() {
            MoveToFront();
        }

        ///<summary>
        ///   Find the suitable bucket for the quad in the list of
        ///   buckets.  If create is false and you don't find it in
        ///   the list, create the bucket and add it to the list.
        ///   If create is false and the bucket doesn't exist, return
        ///   null.
        ///</summary>
        public QuadBucket FindBucketForQuad(QuadInfo quad, List<QuadBucket> buckets, bool create) {
            float z = quad.z;
            int textureHandle = quad.texture.Handle;
            int hashCode = (quad.clipRect != null ? quad.clipRect.GetHashCode() : 0);
            QuadBucket quadBucket = null;
            foreach (QuadBucket bucket in quadBuckets) {
                if (bucket.z == z && bucket.textureHandle == textureHandle && bucket.clipRectHash == hashCode) {
                    quadBucket = bucket;
                    break;
                }
            }
            if (create && quadBucket == null) {
                quadBucket = new QuadBucket(z, textureHandle, hashCode, this);
                buckets.Add(quadBucket);
            }
            return quadBucket;
        }
        
        ///<summary>
        ///   The TextureAndClipRect value is interned, so we can just
        ///   look it up, and don't have to iterate.
        ///</summary>
        public void AddQuad(QuadInfo quad) {
            QuadBucket quadBucket = FindBucketForQuad(quad, quadBuckets, true);
            quadBucket.quads.Add(quad);
        }
                
        public void SetChildrenDirty() {
            foreach (Window child in children) {
                child.Dirty = true;
                child.SetChildrenDirty();
            }
        }

        // For this window and all its children, mark the
        // RenderBatches referenced by QuadBuckets to be rebuilt, and
        // sever the connection between the QuadQueue and any
        // RenderBatches it might have contributed to.
        public void RequireRenderBatchRebuilds() {
            foreach (QuadBucket quadBucket in quadBuckets)
                quadBucket.RequireRenderBatchRebuilds();
            foreach (Window child in children)
                child.RequireRenderBatchRebuilds();
        }
        
        public void ClearQuadBuckets() {
            foreach (QuadBucket quadBucket in quadBuckets)
                quadBucket.RequireRenderBatchRebuilds();
            quadBuckets.Clear();
        }

        public void Draw() {
            if (!visible)
                return;
            Renderer.Instance.CurrentWindow = this;
            if (Dirty) {
                ClearQuadBuckets();
                rebuildFrame = Renderer.Instance.FrameCounter;
                DrawSelf(0);
                Dirty = false;
            }
            Renderer.Instance.AddQuadBuckets(quadBuckets);
            foreach (Window child in children)
                child.Draw();
        }

        protected virtual void DrawSelf(float z) {
            DrawImpl(area, null, z);
        }

        protected virtual void DrawSelf(Vector3 pos, Rect clipRect) {
            DrawImpl(area, clipRect, pos.z);
        }

        // FIXME - Actually queue quads as needed for Window.DrawImpl
        protected virtual void DrawImpl(Rect drawArea, Rect clipArea, float z) {
            // Trace.TraceInformation("Drawing window: " + this.Name);
        }

        // Mouse events
        protected internal virtual void OnMouseDown(MouseEventArgs args) {
            if (this.MouseDown != null)
                MouseDown(this, args);
        }
        protected internal virtual void OnMouseUp(MouseEventArgs args) {
            if (this.MouseUp != null)
                MouseUp(this, args);
        }

        protected internal virtual void OnMouseMoved(MouseEventArgs args) {
            if (this.MouseMoved != null)
                MouseMoved(this, args);
        }
        // Higher level mouse events
        protected internal virtual void OnMouseClicked(MouseEventArgs args) {
            if (this.MouseClicked != null)
                MouseClicked(this, args);
        }
        protected internal virtual void OnMouseDoubleClicked(MouseEventArgs args) {
            if (this.MouseDoubleClicked != null)
                MouseDoubleClicked(this, args);
        }
        // Key events
        protected internal virtual void OnKeyUp(KeyEventArgs args) {
            if (this.KeyUp != null)
                KeyUp(this, args);
        }
        protected internal virtual void OnKeyDown(KeyEventArgs args) {
            if (this.KeyDown != null)
                KeyDown(this, args);
        }
        // Higher level key events
        protected internal virtual void OnKeyPress(KeyEventArgs args) {
            if (this.KeyPress != null)
                KeyPress(this, args);
        }

        protected internal virtual void OnActivated(EventArgs args) {
            isActive = true;
            if (this.Activated != null)
                Activated(this, args);
        }
        protected internal virtual void OnDeactivated(EventArgs args) {
            isActive = false;
            foreach (Window child in children) {
                if (child.IsActive)
                    child.OnDeactivated(args);
            }
            if (this.Deactivated != null)
                Deactivated(this, args);
        }

        // Do these make sense?
        protected internal virtual void OnCaptureGained(EventArgs args) {
            if (this.CaptureGained != null)
                CaptureGained(this, args);
        }
        protected internal virtual void OnCaptureLost(EventArgs args) {
            if (this.CaptureLost != null)
                CaptureLost(this, args);
        }
        public void CaptureInput() {
            GuiSystem.Instance.CaptureInput(this);
        }
        public void ReleaseInput() {
            GuiSystem.Instance.ReleaseInput(this);
        }
        // FIXME - implement Window.RequestRedraw
        protected void RequestRedraw() {
        }

        // Convert a point (in screen pixels), to the offset
        // for that same point as a child window.
        public PointF ScreenToWindow(PointF pt) {
            PointF derivedPos = this.DerivedPosition;
            pt.X -= derivedPos.X;
            pt.Y -= derivedPos.Y;
            return pt;
        }

        // Rectangle that this window would cover
        //public virtual Rect UnclippedPixelRect {
        //    get { return DerivedArea; }
        //}
        // Clipped variant of the above
        public virtual Rect PixelRect {
            get {
                if (clipParent != null)
                    return clipParent.PixelRect.GetIntersection(DerivedArea);
                return DerivedArea;
            }
        }
        // Rectangle into which children can be drawn without 
        // interfering with a frame
        public virtual Rect UnclippedInnerRect {
            get { return DerivedArea; }
        }

        public virtual void SetAreaRect(Rect rect) {
            area = rect;
        }

        public virtual float EffectiveAlpha {
            get {
                if (parent == null)
                    return alpha;
                return alpha * parent.EffectiveAlpha;
            }
        }

        /// <summary>
        ///   Convert a point coordinate within our window (where 0,0 is the
        ///   top left of our window) to an absolute screen position.
        /// </summary>
        /// <param name="pt">the coordinates within our window (in pixels)</param>
        /// <returns>the coordinates in screen space (in pixels)</returns>
        public PointF RelativeToAbsolute(PointF pt) {
            PointF derivedPos = this.DerivedPosition;
            pt.X += derivedPos.X;
            pt.Y += derivedPos.Y;
            return pt;
        }

        /// <summary>
        ///   Sets or gets the position of the top left corner of this widget
        ///   This is relative to the parent window.
        /// </summary>
        public PointF Position {
            get {
                return new PointF(area.Left, area.Top);
            }
            set {
                Rect p = new Rect(value.X, value.X + area.Width, value.Y, value.Y + area.Height);
                if (p != area) {
                    Dirty = true;
                    area = p;
                }
            }
        }

        public PointF DerivedPosition {
            get {
                if (parent == null)
                    return this.Position;
                PointF derivedPos = parent.DerivedPosition;
                PointF pos = this.Position;
                pos.X += derivedPos.X;
                pos.Y += derivedPos.Y;
                return pos;
            }
            set {
                PointF newPosition;
                if (parent == null)
                    newPosition = value;
                else
                {
                    PointF derivedPos = parent.DerivedPosition;
                    PointF pos = value;
                    pos.X -= derivedPos.X;
                    pos.Y -= derivedPos.Y;
                    newPosition = pos;
                }
                if (this.Position != newPosition) {
                    Dirty = true;
                    this.Position = newPosition;
                }
            }
        }

        public Rect DerivedArea {
            get {
                if (parent == null)
                    return new Rect(area.Left, area.Right, area.Top, area.Bottom);
                PointF p = parent.DerivedPosition;
                return new Rect(p.X + area.Left, 
                                p.X + area.Right,
                                p.Y + area.Top, 
                                p.Y + area.Bottom);
            }
        }

        public SizeF Size {
            get {
                return new SizeF(area.Right - area.Left, area.Bottom - area.Top);
            }
            set {
                Rect newArea = new Rect(area.Left, area.Left + value.Width, area.Top, area.Top + value.Height);
                if (newArea != area) {
                    Dirty = true;
                    area = newArea;
                }
            }
        }
        public float Height {
            get {
                return area.Height;
            }
            set
            {
                if (area.Height != value) {
                    Dirty = true;
                    area.Height = value;
                }
            }
        }
        public float Width {
            get {
                return area.Width;
            }
            set
            {
                if (area.Width != value) {
                    Dirty = true;
                    area.Width = value;
                }
            }
        }

        public SizeF MaximumSize {
            get {
                return maxSize;
            }
            set {
                if (maxSize != value) {
                    Dirty = true;
                    maxSize = value;
                }
            }
        }

        public bool Visible {
            get {
                return visible;
            }
            set {
                if (visible != value) {
                    visible = value;
                    // If we're now being made invisible, we must mark
                    // the RenderBatches of which we're a component to
                    // be rebuilt.
                    if (visible == false)
                        RequireRenderBatchRebuilds();
                    else
                        dirty = true;
                }
            }
        }

        public string Name {
            get { return name; }
        }

        public int ChildCount {
            get {
                return children.Count;
            }
        }

        public float Alpha {
            get {
                return alpha;
            }
            set {
                if (alpha != value) {
                    Dirty = true;
                    alpha = value;
                }
            }
        }

        public Window Parent {
            get {
                return parent;
            }
        }

        public float ZValue {
            get {
                return zValue;
            }
            set {
                if (zValue != value) {
                    Dirty = true;                    
                    zValue = value;
                }
            }
        }
        public bool IsActive {
            get {
                return isActive;
            }
        }
        /// <summary>
        ///   Get the deepest descendent window that is active
        /// </summary>
        public Window ActiveChild {
            get {
                if (!this.IsActive)
                    return null;
                foreach (Window child in children)
                    if (child.IsActive)
                        return child.ActiveChild;
                return this;
            }
        }
        
        public bool Dirty {
            get {
                return dirty;
            }
            set {
                if (dirty != value && !dirty) {
                    log.DebugFormat("Window.Dirty: Setting {0} dirty", name);
                    //log.DebugFormat("Window.Dirty: Setting {0} dirty\nStack Trace: {1}", name, new StackTrace(true));
                }
                dirty = value;
            }
        }
        
    }
}
