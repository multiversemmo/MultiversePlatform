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

#region Using directives

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Xml;
using System.Runtime.InteropServices;

using log4net;

using Axiom.Core;
using Axiom.Input;
using Axiom.Utility;

using Multiverse.Gui;

using FontFamily = System.Drawing.FontFamily;

using Multiverse.Web;
using Multiverse.Lib.LogUtil;

#endregion

// These classes are intended to capture the functionality exposed
// by the customizable xml based ui used by World of Warcraft.
// The real work of setting up and drawing the visible objects is
// generally handled by a couple underlying core widgets.
namespace Multiverse.Interface
{
    /// <summary>
    ///   Delegate method used when copying elements of a frame when
    ///   inheriting from an existing template
    /// </summary>
    /// <param name="dst"></param>
    /// <param name="src"></param>
    /// <param name="dst_element"></param>
    /// <param name="src_element"></param>
    public delegate void FrameElementHandler(Frame dst, Frame src, InterfaceLayer dst_element, InterfaceLayer src_element);

	public class InterfaceLayer : IDisposable {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(InterfaceLayer));

		protected Region uiParent;
		protected string parentName;
        protected FrameStrata frameStrata = FrameStrata.Unknown;
        protected int frameLevel = 0;
        protected LayerLevel layerLevel = LayerLevel.Unknown;
        protected int layerOffset = 0;

		#region Methods

        #region Xml Parsing Methods
        public static LayerLevel GetLayerLevel(string val) {
            switch (val) {
                case "BACKGROUND":
                    return LayerLevel.Background;
                case "ARTWORK":
                    return LayerLevel.Artwork;
                case "BORDER":
                    return LayerLevel.Border;
                case "OVERLAY":
                    return LayerLevel.Overlay;
                default:
                    throw new Exception("Invalid layer level: " + val);
            }
        }
        public static string GetLayerLevel(LayerLevel val) {
            switch (val) {
                case LayerLevel.Background:
                    return "BACKGROUND";
                case LayerLevel.Artwork:
                    return "ARTWORK";
                case LayerLevel.Border:
                    return "BORDER";
                case LayerLevel.Overlay:
                    return "OVERLAY";
                default:
                    throw new Exception("Invalid layer level: " + val);
            }
        }
        public static FrameStrata GetFrameStrata(string val) {
            switch (val) {
                case "BACKGROUND":
                    return FrameStrata.Background;
                case "LOW":
                    return FrameStrata.Low;
                case "MEDIUM":
                    return FrameStrata.Medium;
                case "HIGH":
                    return FrameStrata.High;
                case "DIALOG":
                    return FrameStrata.Dialog;
                case "FULLSCREEN":
                    return FrameStrata.Fullscreen;
                case "FULLSCREEN_DIALOG":
                    return FrameStrata.FullscreenDialog;
                case "TOOLTIP":
                    return FrameStrata.Tooltip;
                default:
                    throw new Exception("Invalid frame strata: " + val);
            }
        }
        public static string GetFrameStrata(FrameStrata val) {
            switch (val) {
                case FrameStrata.Background:
                    return "BACKGROUND";
                case FrameStrata.Low:
                    return "LOW";
                case FrameStrata.Medium:
                    return "MEDIUM";
                case FrameStrata.High:
                    return "HIGH";
                case FrameStrata.Dialog:
                    return "DIALOG";
                case FrameStrata.Fullscreen:
                    return "FULLSCREEN";
                case FrameStrata.FullscreenDialog:
                    return "FULLSCREEN_DIALOG";
                case FrameStrata.Tooltip:
                    return "TOOLTIP";
                default:
                    throw new Exception("Invalid frame strata: " + val);
            }
        }
        protected virtual bool HandleElement(XmlElement node) {
			throw new NotImplementedException();
		}
		protected virtual bool HandleAttribute(XmlAttribute attr) {
			throw new NotImplementedException();
		}
        protected virtual void HandleInheritAttribute(XmlAttribute attr) {
        }
        #endregion
        protected static void CopyInterfaceLayer(InterfaceLayer dst, InterfaceLayer src) {
            dst.FrameStrata = src.FrameStrata;
            // we need to copy the underived setting, because we
            // may be copying from a virtual interface layer
            dst.FrameLevel = src.frameLevel;
            dst.LayerLevel = src.layerLevel;
            dst.LayerOffset = src.layerOffset;
        }

        public virtual void Prepare(Window topWindow) {
			throw new NotImplementedException();
		}
		public virtual void SetUiParent(Region parent) {
			throw new NotImplementedException();
		}
		public virtual void ResolveParentStrings() {
			throw new NotImplementedException();
		}
		public virtual InterfaceLayer Clone() {
            throw new NotImplementedException();
        }
        /// <summary>
        ///   Apply the visibility information to this widget and its 
        ///   descendants, recursing as needed.
        /// </summary>
        public virtual void UpdateVisibility() {
            // noop
        }
        /// <summary>
        ///   Change this widget to be flagged as visible, and update its
        ///   descendants as needed.
        /// </summary>
        public void Show() {
            this.Hidden = false;
            UpdateVisibility();
		}
        /// <summary>
        ///   Change this widget to be flagged as hidden, and update its
        ///   descendants as needed.
        /// </summary>
        public void Hide() {
            this.Hidden = true;
            UpdateVisibility();
		}
		/// <summary>
		///   Recursively determine if this widget is hidden.  The widget is
		///   hidden if it has its hidden flag set, or if any of its 
		///   ancestors have their hidden flag set.
		/// </summary>
		public virtual bool IsHidden {
			get {
				if (uiParent == null)
					return false;
				else
					return uiParent.IsHidden;
			}
		}
        /// <summary>
        ///   Return whether the widget is tagged to be visible.  This does 
        ///   not consider the widget's ancestors.
        /// </summary>
        /// <returns></returns>
		public bool IsVisible() {
			return !this.Hidden;
		}
		public virtual bool Hidden {
			get {
				return false;
			}
			set {
				throw new NotImplementedException();
			}
		}
		// Push down possible visibility changes to the windows of
		// our child widgets.
		internal virtual void ShowWindows() {
			throw new NotImplementedException();
		}
		// Push down possible visibility changes to the windows of
		// our child widgets.
		internal virtual void HideWindows()	{
			throw new NotImplementedException();
		}
		// Push down alpha changes to the windows of our child widgets.
		internal virtual void CascadeAlpha(float derivedAlpha) {
			throw new NotImplementedException();
		}
		public void ReadNode(XmlNode node) {
            if (node == null)
                return;
            
            // First pass (where we handle the inherits tag)
            if (this is Region) {
                Region region = (Region)this;
                foreach (XmlAttribute attr in node.Attributes)
                    if (attr.Name == "inherits")
                        region.HandleInheritAttribute(attr);
            }

            // Second pass (where we handle all the tags except inherits)
            foreach (XmlAttribute attr in node.Attributes)
                if (!HandleAttribute(attr))
                    log.WarnFormat("Unhandled attribute: {0}", attr.Name);

			foreach (XmlNode childNode in node.ChildNodes) {
				if (!(childNode is XmlElement))
                    log.InfoFormat("Ignoring non-element child: {0}", childNode.Name);
				else
					if (!HandleElement(childNode as XmlElement))
						log.WarnFormat("Unhandled element: {0}", childNode.Name);
			}
		}
		public virtual void SetupWindowPosition() {
			throw new NotImplementedException();
		}

		public void Dispose() {
			Dispose(true);
		}

		public virtual void Dispose(bool removeFromMap) {
			// noop
		}
		#endregion

		#region Properties
		public Region UiParent {
			get {
				return uiParent;
			}
			set {
				uiParent = value;
			}
		}
        public virtual FrameStrata FrameStrata {
            get {
                return frameStrata;
            }
            set {
                frameStrata = value;
            }
        }
        public virtual int FrameLevel {
            get {
                if (frameLevel != 0)
                    return frameLevel;
                else if (uiParent != null)
                    return UiParent.FrameLevel;
                else
                    Debug.Assert(false, "Unexpected frame level");
                return frameLevel;
            }
            set {
                frameLevel = value;
            }
        }
        public virtual LayerLevel LayerLevel {
            get {
                return layerLevel;
            }
            set {
                layerLevel = value;
            }
        }
        public virtual int LayerOffset {
            get {
                return layerOffset;
            }
            set {
                layerOffset = value;
            }
        }
		#endregion
	}

	public class Layer : InterfaceLayer {
		List<InterfaceLayer> elements = new List<InterfaceLayer>();

		public override void Dispose(bool removeFromMap) {
			foreach (InterfaceLayer element in elements)
				element.Dispose(removeFromMap);
			base.Dispose(removeFromMap);
		}

		public override void ResolveParentStrings() {
			foreach (InterfaceLayer element in elements)
				element.ResolveParentStrings();
		}

		public override void SetUiParent(Region parentFrame) {
			foreach (InterfaceLayer element in elements)
				element.SetUiParent(parentFrame);
		}

        /// <summary>
        ///   Apply the visibility information to this widget and its 
        ///   descendants, recursing as needed.
        /// </summary>
        public override void UpdateVisibility() {
            foreach (InterfaceLayer element in elements)
                element.UpdateVisibility();
        }

		public override InterfaceLayer Clone() {
			Layer rv = new Layer();
            InterfaceLayer.CopyInterfaceLayer(rv, this);
			foreach (InterfaceLayer element in elements) {
				InterfaceLayer newFrame = element.Clone();
				rv.elements.Add(newFrame);
			}
			return rv;
		}
		
		protected override bool HandleElement(XmlElement node) {
			switch (node.Name) {
				case "Texture":
				case "FontString":
					UiSystem.ReadFrame(node, uiParent, this);
					return true;
				default:
					return false;
			}
		}

		protected override bool HandleAttribute(XmlAttribute attr) {
			switch (attr.Name) {
				case "level":
					layerLevel = InterfaceLayer.GetLayerLevel(attr.Value);
					return true;
				default:
					return false;
			}
		}

		public override void Prepare(Window parent) {
			// UiSystem.debugIndent += 1;
			foreach (InterfaceLayer element in elements) {
                element.Prepare(parent);
			}
			// UiSystem.debugIndent -= 1;
		}

		public override void SetupWindowPosition() {
			foreach (InterfaceLayer element in elements)
				element.SetupWindowPosition();
		}


		internal void AddElement(Region element) {
            Debug.Assert(!(element is Frame));
            if (element.LayerLevel == LayerLevel.Unknown)
				element.LayerLevel = this.LayerLevel;
            // Reference the underived frameLevel.  If we 
            // are still set to zero, the element will derive
            // frame level by walking up the hierarchy.
            element.FrameLevel = this.frameLevel;
            if (element.FrameStrata == FrameStrata.Unknown)
                element.FrameStrata = this.FrameStrata;
            elements.Add(element);
		}

		internal List<InterfaceLayer> Elements {
			get {
				return elements;
			}
		}

		internal override void ShowWindows() {
		}

		internal override void HideWindows() {
		}

		internal override void CascadeAlpha(float parentAlpha) {
			foreach (InterfaceLayer element in elements)
				element.CascadeAlpha(parentAlpha);
		}


        public override LayerLevel LayerLevel {
            set {
                base.LayerLevel = value;
                foreach (InterfaceLayer element in elements)
                    if (element.LayerLevel == LayerLevel.Unknown)
                        element.LayerLevel = value;

            }
        }
        public override FrameStrata FrameStrata {
            set {
                base.FrameStrata = value;
                foreach (InterfaceLayer element in elements)
                    if (element.FrameStrata == FrameStrata.Unknown)
                        element.FrameStrata = value;
            }
        }

	}

	public class Region : InterfaceLayer, IRegion {

        public enum Orientation {
            Horizontal,
            Vertical
        }

        public class Anchor : IComparable<Anchor> {

            public enum Framepoint {
                TopLeft,
                TopRight,
                BottomLeft,
                BottomRight,
                Top,
                Bottom,
                Left,
                Right,
                Center
            }

            public bool IsLeftFramepoint() {
                switch (point) {
                    case Framepoint.TopLeft:
                    case Framepoint.Left:
                    case Framepoint.BottomLeft:
                        return true;
                    default:
                        return false;
                }
            }

            public bool IsRightFramepoint() {
                switch (point) {
                    case Framepoint.TopRight:
                    case Framepoint.Right:
                    case Framepoint.BottomRight:
                        return true;
                    default:
                        return false;
                }
            }

            public bool IsTopFramepoint() {
                switch (point) {
                    case Framepoint.TopLeft:
                    case Framepoint.Top:
                    case Framepoint.TopRight:
                        return true;
                    default:
                        return false;
                }
            }

            public bool IsBottomFramepoint() {
                switch (point) {
                    case Framepoint.BottomLeft:
                    case Framepoint.Bottom:
                    case Framepoint.BottomRight:
                        return true;
                    default:
                        return false;
                }
            }

            public static Framepoint GetFramepoint(string val) {
                switch (val) {
                    case "TOPLEFT":
                        return Framepoint.TopLeft;
                    case "TOPRIGHT":
                        return Framepoint.TopRight;
                    case "BOTTOMLEFT":
                        return Framepoint.BottomLeft;
                    case "BOTTOMRIGHT":
                        return Framepoint.BottomRight;
                    case "TOP":
                        return Framepoint.Top;
                    case "BOTTOM":
                        return Framepoint.Bottom;
                    case "LEFT":
                        return Framepoint.Left;
                    case "RIGHT":
                        return Framepoint.Right;
                    case "CENTER":
                        return Framepoint.Center;
                    default:
                        throw new Exception("Invalid framepoint: " + val);
                }
            }

            // The ui element to which this anchor applies
            private Region uiElement = null;
            private Framepoint point = Framepoint.TopLeft;
            private Framepoint relativePoint = Framepoint.TopLeft;
            private string relativeTo = null; // null => parent
            private Region relativeElement = null;
            private PointF offset = new PointF(0, 0);
            private bool isImplied = false;

            public Anchor(Region element) {
                uiElement = element;
            }

            public Anchor(Region element, Framepoint framepoint) {
                uiElement = element;
                point = framepoint;
                relativePoint = framepoint;
            }

            public Anchor(Region element, bool implied) {
                uiElement = element;
                isImplied = implied;
            }

            public Anchor(Anchor other, Region element) {
                uiElement = element;
                point = other.point;
                relativePoint = other.relativePoint;
                relativeTo = other.relativeTo;
                relativeElement = other.relativeElement;
                offset = other.offset;
                isImplied = other.isImplied;
            }

            #region Methods

            public bool Equals(Anchor other)
            {
                return CompareTo(other) == 0;
            }

            public int CompareTo(Anchor other)
            {
                if (other == null)
                    return 1;
                int rv;
                if (uiElement != null && other.uiElement == null)
                    return 1;
                else if (uiElement == null && other.uiElement != null)
                    return -1;
                else if (uiElement != null && other.uiElement != null) {
                    rv = uiElement.Name.CompareTo(other.uiElement.Name);
                    if (rv != 0)
                        return rv;
                }
                rv = point.CompareTo(other.point);
                if (rv != 0)
                    return rv;
                rv = relativePoint.CompareTo(other.relativePoint);
                if (rv != 0)
                    return rv;
                if (relativeTo != null && other.relativeTo == null)
                    return 1;
                else if (relativeTo == null && other.relativeTo != null)
                    return -1;
                else if (relativeTo != null && other.relativeTo != null)
                {
                    rv = relativeTo.CompareTo(other.relativeTo);
                    if (rv != 0)
                        return rv;
                }
                if (relativeElement != null && other.relativeElement == null)
                    return 1;
                else if (relativeElement == null && other.relativeElement != null)
                    return -1;
                else if (relativeElement != null && other.relativeElement != null)
                {
                    rv = relativeElement.Name.CompareTo(other.relativeElement.Name);
                    if (rv != 0)
                        return rv;
                }
                rv = offset.X.CompareTo(other.offset.X);
                if (rv != 0)
                    return rv;
                rv = offset.Y.CompareTo(other.offset.Y);
                if (rv != 0)
                    return rv;
                return isImplied.CompareTo(other.isImplied);
            }

            public virtual void ReadNode(XmlNode node) {
                XmlAttribute attr;
                attr = node.Attributes["point"];
                point = Anchor.GetFramepoint(attr.Value);
                attr = node.Attributes["relativePoint"];
                if (attr != null)
                    relativePoint = GetFramepoint(attr.Value);
                else
                    relativePoint = point;
                attr = node.Attributes["relativeTo"];
                if (attr != null)
                    relativeTo = attr.Value;
                if (relativeTo == "UIParent")
                    relativeTo = null;

                foreach (XmlNode childNode in node.ChildNodes) {
                    switch (childNode.Name) {
                        case "Offset":
                            SizeF dim = XmlUi.ReadDimension(childNode);
                            // Since WoW uses +y for up, invert it
                            offset = new PointF(dim.Width, -dim.Height);
                            break;
                        default:
                            log.WarnFormat("Invalid element found in Anchor: {0}", childNode.Name);
                            break;
                    }
                }
            }

            public override string ToString() {
                return string.Format("[Point: {0}, RelativePoint: {1}, RelativeTo: {2}, Offset: {3}]",
                                     point, relativePoint, relativeTo, offset);
            }

            public PointF GetDelta(SizeF windowSize) {
                return Anchor.GetDelta(point, windowSize);
            }

            public PointF GetRelativeDelta(SizeF windowSize) {
                return Anchor.GetDelta(relativePoint, windowSize);
            }

            // public float GetWidth(Point p1, Point p2);
            private static PointF GetDelta(Framepoint p, SizeF windowSize) {
                PointF delta = new PointF(0, 0);
                switch (p) {
                    case Framepoint.TopRight:
                    case Framepoint.Right:
                    case Framepoint.BottomRight:
                        delta.X += windowSize.Width;
                        break;
                    case Framepoint.Top:
                    case Framepoint.Center:
                    case Framepoint.Bottom:
                        delta.X += windowSize.Width / 2;
                        break;
                    default:
                        break;
                }
                switch (p) {
                    case Framepoint.BottomLeft:
                    case Framepoint.Bottom:
                    case Framepoint.BottomRight:
                        delta.Y += windowSize.Height;
                        break;
                    case Framepoint.Left:
                    case Framepoint.Center:
                    case Framepoint.Right:
                        delta.Y += windowSize.Height / 2;
                        break;
                    default:
                        break;
                }
                return delta;
            }

            /// <summary>
            ///   For anchors, $parent really means the parent of the ui element
            ///   that the anchor is applied to.
            /// </summary>
            public void ResolveParentStrings() {
                if (relativeTo == null || uiElement == null ||
                    uiElement.uiParent == null || uiElement.uiParent.Name == null) {
                    return;
                }
                relativeTo = relativeTo.Replace("$parent", uiElement.uiParent.Name);
            }

            public bool ResolveElement() {
                if (relativeTo == null || relativeTo == "UIParent") {
                    relativeElement = uiElement.uiParent;
                    return true;
                } else if (UiSystem.FrameMap.ContainsKey(relativeTo)) {
                    relativeElement = UiSystem.FrameMap[relativeTo];
                    return true;
                }
                return false;
            }

            public Region GetRelativeElement() {
                if (relativeTo == null || relativeTo == "UIParent")
                    return uiElement.uiParent;
                else if (UiSystem.FrameMap.ContainsKey(relativeTo))
                    return UiSystem.FrameMap[relativeTo];
                else
                    log.ErrorFormat("Invalid relative element: '{0}'", relativeTo);
                return null;
            }

            public Region GetElement() {
                return uiElement;
            }

            #endregion

            #region Properties

            public Framepoint Point {
                get { return point; }
                set { point = value; }
            }

            public Framepoint RelativePoint {
                get { return relativePoint; }
                set { relativePoint = value; }
            }

            public string RelativeTo {
                get {
                    if (relativeTo != null)
                        return relativeTo;
                    else if (uiElement != null && uiElement.uiParent != null)
                        return uiElement.uiParent.Name;
                    else
                        return null;
                }
                set {
                    relativeTo = value;
                }
            }

            public PointF Offset {
                get { return offset; }
                set { offset = value; }
            }

            public bool IsImplied {
                get { return isImplied; }
            }

            #endregion

        }

		#region Fields

        public Window window;
        protected Window parentWindow;
        
		protected string name;
		protected string generatedName;
		protected SizeF size;
		protected SizeF specifiedSize;
		protected PointF position;
        protected PointF scrollOffset = new PointF(0, 0);
        /// <summary>
        ///   Flag that indicates whether this region should be hidden.  
        ///   This flag does not take into account any ancestors
        /// </summary>
		protected bool isHidden = false;
        /// <summary>
        ///   Flag that indicates whether this frame is already shown
        /// </summary>
        protected bool isShown = false;
		protected bool isVirtual = false;
        protected bool isScrollChild = false;
		protected bool setAllPoints = false;
		// Set these to true if you want to honor the current size
		protected bool useHeight = false;
		protected bool useWidth = false;
        // Set my position and size based on the anchors
		protected List<Anchor> anchors = new List<Anchor>();
		protected float widgetAlpha = 1.0f;
        // The region whose window we (and our children) will clip to
        protected Region clipper;

		protected UiSystem parentUiSystem;

		#endregion

        #region Methods

        public static Orientation GetOrientation(string val) {
            switch (val) {
                case "HORIZONTAL":
                    return Orientation.Horizontal;
                case "VERTICAL":
                    return Orientation.Vertical;
                default:
                    throw new Exception("Invalid orientation: " + val);
            }
        }

        public static string GetOrientation(Orientation val) {
            switch (val) {
                case Orientation.Horizontal:
                    return "HORIZONTAL";
                case Orientation.Vertical:
                    return "VERTICAL";
                default:
                    throw new Exception("Invalid orientation: " + val);
            }
        }

        internal virtual string GetInterfaceName() {
            return "IRegion";
        }

        protected static void DisposeWindow(Window window) {
            if (window == null)
                return;
            while (window.ChildCount > 0) {
                Window childWindow = window.GetChildAtIndex(0);
                window.RemoveChild(childWindow);
            }
            if (window.Name != null)
                WindowManager.Instance.DestroyWindow(window.Name);
        }

        protected virtual void DisposeWindows() {
        }

        public override void Dispose(bool removeFromMap) {
            DisposeWindows();
            if (removeFromMap && this.Name != null) {
                if (this.IsVirtual) {
                    if (!UiSystem.VirtualFrameMap.Remove(this.Name))
                        log.WarnFormat("Couldn't find {0} in virtual frame map", this.Name);
                } else {
                    if (!UiSystem.FrameMap.Remove(this.Name))
                        log.WarnFormat("Couldn't find {0} in frame map", this.Name);
                }
            }
        }

        public virtual bool CheckHit(PointF pt) {
            if (position.X > pt.X ||
                position.Y > pt.Y ||
                position.X + size.Width < pt.X ||
                position.Y + size.Height < pt.Y ||
                this.IsHidden)
                return false;
            return true;
        }

        public override void ResolveParentStrings() {
            if (isVirtual)
                return;
            if (uiParent == null)
                return;
            if (name != null && name.StartsWith("$parent")) {
                Region tmp = uiParent;
                while (tmp != null) {
                    if (tmp.name != null)
                        break;
                    tmp = tmp.uiParent;
                }
                // either we found an ancestor with a name, or ran out of ancestors
                if (tmp.name == null)
                    return;
                name = name.Replace("$parent", tmp.name);
                Debug.Assert(!UiSystem.FrameMap.ContainsKey(name),
                             string.Format("Newly generated name '{0}' conflicts with existing name", name));
                log.InfoFormat("Adding new element to ui frames: {0}", name);
                UiSystem.FrameMap[name] = this;
            }
            foreach (Anchor anchor in anchors)
                anchor.ResolveParentStrings();
        }

        public virtual void ResolveAnchors() {
            foreach (Anchor anchor in anchors) {
                bool status = anchor.ResolveElement();
                if (!status)
                    log.ErrorFormat("Invalid relative element '{0}' from '{1}'", anchor.RelativeTo, this.Name);
            }
        }

        // Internal method to set the name when we cannot read it from the xml
        internal void SetName(string name) {
            this.name = name;
        }

        protected static void SetWindowProperties(Window window, SizeF size, PointF position) {
            window.Size = size;
            window.DerivedPosition = position;
        }

        public virtual void SetWindowProperties() {
        }

        public void AddWindows(Window parentWindow) {
            parentWindow.AddChild(window);
            if (window.Name != null)
                WindowManager.Instance.AttachWindow(window);
        }

        public override void Prepare(Window parent) {
            if (this.IsVirtual)
                return;
            this.parentWindow = parent;

            GetWindows(); // create any windows we need
            AddImpliedAnchors(); // find out where to put the windows
            ComputePlacement();
            SetWindowProperties(); // put the windows there
            AddWindows(parent); // add our window to our parent
        }

        protected float GetDerivedAlpha() {
            float derivedAlpha = 1.0f;
            if (uiParent != null)
                derivedAlpha = uiParent.GetDerivedAlpha();
            derivedAlpha *= widgetAlpha;
            return derivedAlpha;
        }

        public virtual void GetWindows() {
            //Imageset imageset = ImagesetManager.Instance.GetImageset("MultiverseInterface");
            //Image tmp = imageset.GetImage("default");
            //LayeredStaticImage imageWindow = new LayeredStaticImage(name);
            //imageWindow.Layer = this.Layer;
            //imageWindow.SetImage(tmp);

            //imageWindow.Alpha = 1.0f;
            //imageWindow.MetricsMode = MetricsMode.Absolute;
            //imageWindow.MinimumSize = new Size(0, 0);
            //imageWindow.MaximumSize = topWindow.MaximumSize;

            //window = imageWindow;
        }

        /// <summary>
        ///   Get the anchor for the left side
        /// </summary>
        /// <param name="impliedOk">if impliedOk is true, this will return implied anchors</param>
        /// <returns></returns>
        protected Anchor GetLeftAnchor(bool impliedOk) {
            foreach (Anchor anchor in anchors)
                if (anchor.IsLeftFramepoint())
                    if (impliedOk || !anchor.IsImplied)
                        return anchor;
            return null;
        }
        protected Anchor GetRightAnchor(bool impliedOk) {
            foreach (Anchor anchor in anchors)
                if (anchor.IsRightFramepoint())
                    if (impliedOk || !anchor.IsImplied)
                        return anchor;
            return null;
        }
        protected Anchor GetTopAnchor(bool impliedOk) {
            foreach (Anchor anchor in anchors)
                if (anchor.IsTopFramepoint())
                    if (impliedOk || !anchor.IsImplied)
                        return anchor;
            return null;
        }
        protected Anchor GetBottomAnchor(bool impliedOk) {
            foreach (Anchor anchor in anchors)
                if (anchor.IsBottomFramepoint())
                    if (impliedOk || !anchor.IsImplied)
                        return anchor;
            return null;
        }

        private void AnchorToParentPoint(Anchor.Framepoint point) {
            // Insert an anchor to ui parent
            Anchor anchor = new Anchor(this, point);
            anchors.Add(anchor);
        }

        public void AnchorToParent(Anchor.Framepoint point) {
            // If we are already anchored, we can skip this
            if (anchors.Count != 0)
                return;
            // Insert an anchor to ui parent
            AnchorToParentPoint(point);
        }

        public void AnchorToParentFull() {
            // If we are already anchored, we can skip this
            if (anchors.Count != 0)
                return;
            // Insert a center anchor to ui parent (text is centered)
            AnchorToParentPoint(Anchor.Framepoint.TopLeft);
            AnchorToParentPoint(Anchor.Framepoint.BottomRight);
        }

        public void AnchorToParentCenter() {
            // If we are already anchored, we can skip this
            if (anchors.Count != 0)
                return;
            // Insert a center anchor to ui parent (text is centered)
            AnchorToParentPoint(Anchor.Framepoint.Center);
        }

        /// <summary>
        ///   Determines the position and size of the layout frame.
        ///   This does not actually update any of the window objects,
        ///   but is useful to determine their parameters.
        ///   If we are using the widget size, or have a fully specified size, 
        ///   we only need one anchor otherwise, we will need more.
        /// </summary>
        protected void AddImpliedAnchors() {
            List<Anchor> origImpliedAnchors = anchors.FindAll(new Predicate<Anchor>(this.IsImplied));
            List<Anchor> impliedAnchors = new List<Anchor>();

            Anchor left = GetLeftAnchor(false);
            Anchor right = GetRightAnchor(false);
            Anchor top = GetTopAnchor(false);
            Anchor bottom = GetBottomAnchor(false);

            if (anchors.Count - origImpliedAnchors.Count == 0) {
                // Insert a top left anchor to ui parent
                // Logger.Log(0, "Adding implied top left anchor");
                Anchor topLeft = new Anchor(this, true);
                left = top = topLeft;
                impliedAnchors.Add(topLeft);
            }

            if (!this.IsSizeSpecified) {
                if (!useWidth && specifiedSize.Width == 0) {
                    // Add any implied anchors that we need
                    if (left == null) {
                        left = new Anchor(this, true);
                        left.Point = Anchor.Framepoint.Left;
                        left.RelativePoint = Anchor.Framepoint.Left;
                        impliedAnchors.Add(left);
                    }
                    if (right == null) {
                        right = new Anchor(this, true);
                        right.Point = Anchor.Framepoint.Right;
                        right.RelativePoint = Anchor.Framepoint.Right;
                        impliedAnchors.Add(right);
                    }
                }
                if (!useHeight && specifiedSize.Height == 0) {
                    if (top == null) {
                        top = new Anchor(this, true);
                        top.Point = Anchor.Framepoint.Top;
                        top.RelativePoint = Anchor.Framepoint.Top;
                        impliedAnchors.Add(top);
                    }
                    if (bottom == null) {
                        bottom = new Anchor(this, true);
                        bottom.Point = Anchor.Framepoint.Bottom;
                        bottom.RelativePoint = Anchor.Framepoint.Bottom;
                        impliedAnchors.Add(bottom);
                    }
                }
            }

            // See if we changed our implied anchors
            bool needAnchorUpdate = false;
            if (impliedAnchors.Count != origImpliedAnchors.Count)
                needAnchorUpdate = true;
            else {
                foreach (Anchor impliedAnchor in impliedAnchors)
                {
                    if (origImpliedAnchors.Contains(impliedAnchor))
                        continue;
                    needAnchorUpdate = true;
                    break;
                }
            }

            if (needAnchorUpdate)
            {
                // Remove any implied anchors, since they are being replaced
                anchors.RemoveAll(new Predicate<Anchor>(this.IsImplied));
                anchors.AddRange(impliedAnchors);
                if (this.Ui != null)
                    this.Ui.NotifyAnchorsChanged(this);
            }
        }

        /// <summary>
        ///    Compute the size and position of the widget based on the set 
        ///    of anchors.
        /// </summary>
        public void ComputePlacement() {
            // We may be called before being initialized.  Once we are fully 
            // initialized, topWindow will be set.
            //if (!this.IsPrepared)
            //    return;

            bool heightDetermined = false;
            bool widthDetermined = false;

            SizeF tmpSize = new SizeF(0, 0); // empty
            if (useHeight) {
                heightDetermined = true;
                tmpSize.Height = this.Size.Height;
            }
            if (useWidth) {
                widthDetermined = true;
                tmpSize.Width = this.Size.Width;
            }
#if EXPAND_TO_FILL
			// Handle the case where all we have is a center anchor
			// and no size information.
			if ((!widthDetermined || !heightDetermined) &&
				anchors.Count == 1 &&
				anchors[0].Point == Anchor.Framepoint.Center) {
				// In this case, our size is determined based on our parent, 
				// so that we are the largest frame that would be completely 
				// contained by the parent window, but would still be centered.

				Anchor center = anchors[0];
				LayoutFrame anchorTarget = center.GetRelativeElement();
				if (!heightDetermined) {
					tmpSize.height = anchorTarget.Size.height - 2 * Math.Abs(center.Offset.y);
					if (tmpSize.width < 0)
						tmpSize.width = 0;
					heightDetermined = true;
				}
				if (!widthDetermined) {
					tmpSize.width = anchorTarget.Size.width - 2 * Math.Abs(center.Offset.x);
					if (tmpSize.width < 0)
						tmpSize.width = 0;
					widthDetermined = true;
				}

				Logger.Log(1, "Anchor target for {0} = {1}", name, anchorTarget.Name);
			}
#endif
            Anchor left = GetLeftAnchor(true);
            Anchor right = GetRightAnchor(true);
            Anchor top = GetTopAnchor(true);
            Anchor bottom = GetBottomAnchor(true);

            // If we have a Top and a Bottom anchor, we can infer the height
            // If we have a Left and a Right anchor, we can infer the width
            if (!widthDetermined && left != null && right != null) {
                PointF basePoint1 = GetBasePoint(uiParent, parentWindow, left);
                PointF basePoint2 = GetBasePoint(uiParent, parentWindow, right);
                tmpSize.Width = basePoint2.X - basePoint1.X;
                widthDetermined = true;
            }
            if (!heightDetermined && top != null && bottom != null) {
                PointF basePoint1 = GetBasePoint(uiParent, parentWindow, top);
                PointF basePoint2 = GetBasePoint(uiParent, parentWindow, bottom);
                tmpSize.Height = basePoint2.Y - basePoint1.Y;
                heightDetermined = true;
            }
            // If we couldn't get the width from the anchors, see if we can get it
            // from the specified size
            if (!heightDetermined && specifiedSize.Height != 0) {
                tmpSize.Height = specifiedSize.Height; // adjust to what is in the xml
                heightDetermined = true;
            }
            if (!widthDetermined && specifiedSize.Width != 0) {
                tmpSize.Width = specifiedSize.Width; // adjust to what is in the xml
                widthDetermined = true;
            }

            Debug.Assert(anchors.Count > 0, "Must have at least one anchor");

            if (!widthDetermined || !heightDetermined)
                log.WarnFormat("Failed to determine widget dimensions for {0}", this.Name);

            //Debug.Assert(this.Name != "QuestLogObjective1");
            //Debug.Assert(this.Name != "QuestLogTimerText");

            // Now get a basepoint for the frame based on the first anchor
            PointF basePoint = GetBasePoint(uiParent, parentWindow, anchors[0]);

            // Set the properties
            size = tmpSize;
            if (size.Width < 0)
                size.Width = 0;
            if (size.Height < 0)
                size.Height = 0;
            position = basePoint;
            PointF delta = anchors[0].GetDelta(size);
            position.X -= delta.X;
            position.Y -= delta.Y;
            log.DebugFormat("Called compute placement on {0}; size = [{1}, {2}]; position = [{3}, {4}]",
                            this.Name, size.Width, size.Height, position.X, position.Y);
        }

        protected static Region GetAnchorTarget(Region uiParent, Anchor anchor) {
            Region anchorFrame = uiParent;
            if (anchor.RelativeTo != null &&
                !UiSystem.FrameMap.ContainsKey(anchor.RelativeTo)) {
                Dictionary<string, Region>.KeyCollection keys = UiSystem.FrameMap.Keys;
                Debug.Assert(false, string.Format("RelativeTo target: {0} not handled", anchor.RelativeTo));
            }
            if (anchor.RelativeTo != null)
                anchorFrame = UiSystem.FrameMap[anchor.RelativeTo];
            return anchorFrame;
        }

        protected static PointF GetBasePoint(Region uiParent, Window parentWindow, Anchor anchor) {
            Region anchorFrame = anchor.GetRelativeElement();
            SizeF anchorFrameSize = parentWindow.Size;
            PointF anchorFramePosition = parentWindow.DerivedPosition;
            if (anchorFrame != null) {
                // Logger.Log(0, "Anchored to " + anchorFrame.Name);
                anchorFrameSize = anchorFrame.SizeForAnchor;
                anchorFramePosition = anchorFrame.PositionForAnchor;
            } else if (uiParent != null) {
                // Logger.Log(0, "Anchor not found");
                anchorFrameSize = uiParent.SizeForAnchor;
                anchorFramePosition = uiParent.PositionForAnchor;
            } else {
                // Logger.Log(0, "Anchor not found and uiParent is null");
            }
            PointF relativeDelta = anchor.GetRelativeDelta(anchorFrameSize);
            PointF offset = anchor.Offset;
            PointF rv = anchorFramePosition;
            rv.X += relativeDelta.X + offset.X;
            rv.Y += relativeDelta.Y + offset.Y;
            return rv;
        }

        public override void SetUiParent(Region parentFrame) {
			uiParent = parentFrame;
		}

        /// <summary>
        ///   Returns the y coordinate of the top of this frame, 
        ///   but inverted so that 0 is the bottom of the screen
        /// </summary>
        /// <returns></returns>
        public int GetTop()
        {
            return (int)(parentUiSystem.Window.Height - position.Y);
        }
        public int GetBottom()
        {
            return (int)(parentUiSystem.Window.Height - (position.Y + size.Height));
        }
        public int GetLeft()
        {
            return (int)(position.X);
        }
        public int GetRight()
        {
            return (int)(position.X + size.Width);
        }

        public IRegion GetParent()
        {
            return uiParent;
        }

        #endregion

		#region Copy Methods

		public override InterfaceLayer Clone() {
			Region rv = new Region();
			CopyTo(rv);
			return rv;
		}

		public virtual bool CopyTo(object obj) {
			if (!(obj is Region))
				return false;
			Region dst = (Region)obj;
			Region.CopyRegion(dst, this);
			return true;
		}

		public void GenerateName(string basename) {
			if (basename == null)
				basename = string.Empty;
			generatedName = UiSystem.GenerateWindowName(basename);
		}

		protected static void CopyRegion(Region dst, Region src) {
			// Set the name if it has not already been set.
			if (dst.name == null)
				if (src.name != null && src.Name.StartsWith("$parent"))
					// copy over the name, since it will be resolved to something else
					dst.name = src.name;
				else
					dst.GenerateName(src.name);
            InterfaceLayer.CopyInterfaceLayer(dst, src);
            // Skip uiParent, since this will not be correct, and
			// we will set it when we are done with the copy.
			// dst.uiParent = src.uiParent;
			// Skip isVirtual as well, since inheriting from a 
			// virtual object should not make you virtual
			// dst.isVirtual = src.isVirtual;
			// Also skip window, since this field will be set in Prepare
			// dst.window = src.window;
            // Also skip parentUiSystem since this field will be set in Prepare
            // dst.parentUiSystem = src.parentUiSystem;
            dst.size = src.size;
			dst.specifiedSize = src.specifiedSize;
			dst.position = src.position;
			dst.isHidden = src.isHidden;
			dst.setAllPoints = src.setAllPoints;
			dst.widgetAlpha = src.widgetAlpha;
			dst.anchors = new List<Anchor>();
			foreach (Anchor anchor in src.anchors) {
				Anchor newAnchor = new Anchor(anchor, dst);
				dst.anchors.Add(newAnchor);
			}
			// We don't need to do this, since we aren't concrete yet
			// dst.Ui.NotifyAnchorsChanged(dst);
		}

		#endregion

		/// <summary>
		///   Sets the position of the layout frame and of the frame's windows.
		///   This has to be public, because I want a Frame object to be able 
		///   to call this method on other LayoutFrame objects.
		/// </summary>
		public override void SetupWindowPosition() {
			// We may not be ready to set up the window position
			//if (!this.IsPrepared)
			//    return;

			AddImpliedAnchors();
			ComputePlacement();
			if (this.Ui != null)
				this.Ui.NotifyAnchored(this);
			//foreach (LayoutFrame frame in anchoredWidgets)
			//    frame.SetupWindowPosition();
			//window.MetricsMode = MetricsMode.Absolute;
			//window.Size = size;
			//window.Position = position;
		}

        internal void SetInheritTarget(string inherits) {
            if (UiSystem.FrameMap.ContainsKey(inherits)) {
                Region baseNode = UiSystem.FrameMap[inherits];
                baseNode.CopyTo(this);
                // Recursively update the ui parent
                this.SetUiParent(uiParent);
            } else if (UiSystem.VirtualFrameMap.ContainsKey(inherits)) {
                Region baseNode = UiSystem.VirtualFrameMap[inherits];
                baseNode.CopyTo(this);
                // Recursively update the ui parent
                this.SetUiParent(uiParent);
            } else {
                log.ErrorFormat("Invalid inherit target: {0}", inherits);
            }
        }

        /// <summary>
        ///   Sets up the anchor for our widget.  This will cause all other 
        ///   anchors for this widget to be removed, but the actual layout
        ///   work won't happen here.  The elements anchored to this widget
        ///   will still need to be notified if we move, and we still need
        ///   to call SetupWindowPosition.  Neither of these can happen until
        ///   the UI is fully loaded, so do not do it here.
        /// </summary>
        /// <param name="pointStr"></param>
        /// <param name="relativeTo"></param>
        /// <param name="relativePointStr"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal void SetPrimaryAnchor(string pointStr, string relativeTo, string relativePointStr, int x, int y) {
            // Remove any existing anchors
            anchors.Clear();
            SetupAnchor(pointStr, relativeTo, relativePointStr, x, y);
        }

        /// <summary>
        ///   Helper method to construct the anchor that matches these parameters.
        /// </summary>
        /// <param name="pointStr"></param>
        /// <param name="relativeTo"></param>
        /// <param name="relativePointStr"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        protected void SetupAnchor(string pointStr, string relativeTo, string relativePointStr, int x, int y) {
            Anchor.Framepoint point = Anchor.GetFramepoint(pointStr);
            Anchor.Framepoint relativePoint = Anchor.GetFramepoint(relativePointStr);
            Anchor newAnchor = null;
            foreach (Anchor anchor in anchors) {
                if (anchor.Point == point) {
                    newAnchor = anchor;
                    break;
                }
            }
            if (newAnchor == null) {
                newAnchor = new Anchor(this);
                anchors.Add(newAnchor);
            }

            newAnchor.Point = point;
            newAnchor.RelativePoint = relativePoint;
            newAnchor.RelativeTo = relativeTo;
            // WoW inverts y
            newAnchor.Offset = new PointF(x, -y);
        }
        internal void SetWidth(int width, bool useWidth) {
            size.Width = width;
            this.useWidth = useWidth;
            SetupWindowPosition();
        }

        internal void SetHeight(int height, bool useHeight) {
            size.Height = height;
            this.useHeight = useHeight;
            SetupWindowPosition();
        }


		#region IRegion Methods

		public string GetName() {
			return name;
		}

		public virtual int GetHeight()
		{
			return (int)size.Height;
		}
		public int GetSpecifiedHeight() {
			return (int)specifiedSize.Height;
		}
        /// <summary>
        ///   Sets the height of this widget.  If the height is 0, the effect
        ///   is to clear the flag that tells us to honor the height value.
        /// </summary>
        /// <param name="height"></param>
        public void SetHeight(int height) {
            SetHeight(height, (height != 0));
        }

        public virtual int GetWidth() {
			return (int)size.Width;
		}
		public int GetSpecifiedWidth() {
			return (int)specifiedSize.Width;
		}
        /// <summary>
        ///   Sets the width of this widget.  If the width is 0, the effect
        ///   is to clear the flag that tells us to honor the width value.
        /// </summary>
        /// <param name="width"></param>
        public void SetWidth(int width) {
            SetWidth(width, (width != 0));
        }


		//public override bool IsVisible() {
		//    return !this.Hidden;
		//}

		public float GetAlpha()
		{
			return widgetAlpha;
		}
		public virtual void SetAlpha(float alpha)
		{
			widgetAlpha = alpha;
			float derivedAlpha = GetDerivedAlpha();
			// apply the new alpha to any children we may have
			CascadeAlpha(derivedAlpha);
		}

        /// <summary>
        ///   This should not be called until the UI has been set on the object.
        ///   Once the UI is fully loaded, this is acceptable.
        /// </summary>
		public void ClearAllPoints() {
			anchors = new List<Anchor>();
            this.Ui.NotifyAnchorsChanged(this);
            SetupWindowPosition();
		}
		public void SetPoint(string pointStr, string relativeTo, string relativePointStr)
		{
			SetPoint(pointStr, relativeTo, relativePointStr, 0, 0);
		}
		public void SetPoint(string pointStr, string relativeTo, string relativePointStr, int x, int y)
		{
			// Remove any implied anchors, since the need for these may go away
			anchors.RemoveAll(new Predicate<Anchor>(this.IsImplied));

            // Create a new anchor (or update the existing one)
            SetupAnchor(pointStr, relativeTo, relativePointStr, x, y);

			this.Ui.NotifyAnchorsChanged(this);

            log.DebugFormat("In Region.SetPoint for {0}", this.Name);
			SetupWindowPosition();
		}

		#endregion

		#region Xml Parsing Methods

        protected override void HandleInheritAttribute(XmlAttribute attr) {
            SetInheritTarget(attr.Value);
        }

		protected override bool HandleAttribute(XmlAttribute attr) {
			switch (attr.Name) {
				case "name":
					name = attr.Value;
					return true;
				case "virtual":
					this.isVirtual = bool.Parse(attr.Value);
					return true;
				case "setAllPoints":
					// TODO: Handle these => what is this supposed to do?
					this.setAllPoints = bool.Parse(attr.Value);
					return true;
				case "hidden":
					this.isHidden = bool.Parse(attr.Value);
					return true;
                case "inherits":
                    // We have already handled this
                    return true;
				default:
					return false;
			}
		}

		private void ReadAnchors(XmlElement node) {
			anchors.Clear();
			foreach (XmlNode childNode in node.ChildNodes) {
				if (!(childNode is XmlElement))
					log.InfoFormat("Ignoring non-element child: {0}", childNode.Name);
				else {
					Anchor anchor = new Anchor(this);
					anchor.ReadNode(childNode);
					anchors.Add(anchor);
					// TODO: Register with the anchor target here?
				}
			}		
		}

		protected override bool HandleElement(XmlElement node) {
			switch (node.Name) {
				case "Size":
					this.SpecifiedSize = XmlUi.ReadDimension(node);
					return true;
				case "Anchors":
					ReadAnchors(node);
					return true;
				default:
					return false;
			}
		}


		#endregion

		#region Methods

		/// <summary>
		///   If this layout frame and its parents are all visible,
		///   then set the graphic window to be visible
		/// </summary>
		internal override void ShowWindows() {
		}

		internal override void HideWindows() {
		}

		internal override void CascadeAlpha(float parentAlpha) {
		}

		public virtual void NotifyChanged() {
			// OnSized();
		}

        public override void UpdateVisibility() {
            bool desiredVisibility = !this.IsHidden;
            bool currentVisibility = isShown;
            if (desiredVisibility != currentVisibility) {
                if (desiredVisibility && !currentVisibility) {
                    ShowWindows(); // this will show the windows
                    isShown = true;
                } else if (currentVisibility && !desiredVisibility) {
                    HideWindows(); // this will hide the windows
                    isShown = false;
                }
            }
        }

		protected bool IsImplied(Anchor anchor) {
			return anchor.IsImplied;
		}

        internal void SetClipper(Region region) {
            clipper = region;
        }
		
		#endregion

		#region Event Handlers
		/// <summary>
		///    Event trigger method for the <see cref="Sized"/> event.  
		///    Note that this is the internal version.  Most of the time,
		///    you will want to set up your handler as an OnSizeChanged
		///    event instead.
		/// </summary>
		/// <param name="e">Event information.</param>
		public virtual void OnSized(GuiEventArgs e) {
			if (this.Ui != null)
				this.Ui.NotifyAnchored(this);
		}
		
		#endregion

		#region Properties

		public string Name {
			get {
				if (name != null)
					return name;
				return generatedName;
			}
		}
		public bool IsVirtual {
			get {
				if (isVirtual)
					return true;
				else if (uiParent == null)
					return false;
				else
					return uiParent.IsVirtual;
			}
			set {
				isVirtual = value;
			}
		}
		public override bool Hidden {
			get {
				return isHidden;
			}
			set {
				isHidden = value;
			}
		}
		public bool IsSizeSpecified {
			get { 
				return specifiedSize.Height != 0 && specifiedSize.Width != 0;
			}
		}
        public SizeF Size {
            get {
                return size;
            }
            set {
                if (size.Height != value.Height || size.Width != value.Width) {
                    size = value;
                    OnSized(null);
                }
            }
        }
        public SizeF SizeForAnchor {
            get {
                if (!isScrollChild || uiParent == null)
                    return size;
                return new SizeF(uiParent.ScrollOffset.X + size.Width, uiParent.ScrollOffset.Y + size.Height);
            }
        }
		public SizeF SpecifiedSize {
			get {
				return specifiedSize;
			}
			set {
				specifiedSize = value;
			}
		}
		public PointF Position {
			get { return position; }
		}
        public PointF PositionForAnchor {
            get {
                if (!isScrollChild || uiParent == null)
                    return position;
                PointF rv = position;
                PointF offset = uiParent.ScrollOffset;
                rv.X -= offset.X;
                rv.Y -= offset.Y;
                return rv;
            }
        }
        public PointF ScrollOffset {
            get {
                return scrollOffset;
            }
            set {
                scrollOffset = value;
            }
        }
        public bool IsScrollChild {
            get {
                return isScrollChild;
            }
            set {
                isScrollChild = true;
            }
        }

		/// <summary>
		///   Recursively determine if this widget is hidden.  The widget is
		///   hidden if it has its hidden flag set, or if any of its 
		///   ancestors have their hidden flag set.
		/// </summary>
		public override bool IsHidden {
			get {
				if (isHidden)
					return true;
				else if (uiParent == null)
					return false;
				else
					return uiParent.IsHidden;
			}
		}

		public bool IsPrepared {
			get {
				return parentWindow != null;
			}
		}

		public UiSystem Ui {
			get { return parentUiSystem; }
			set { parentUiSystem = value; }
		}

		public List<Anchor> Anchors {
			get {
				return anchors;
			}
		}

        protected Window ClipWindow {
            get {
                if (clipper != null)
                    return clipper.window;
                if (uiParent!= null)
                    return uiParent.ClipWindow;
                return null;
            }
        }
		#endregion
	}

    public class LayeredRegion : Region, ILayeredRegion {
        protected ColorRect colorRect;
        protected ColorEx vertexColor = ColorEx.Black;

        internal override string GetInterfaceName() {
            return "ILayeredRegion";
        }

        public override InterfaceLayer Clone() {
            LayeredRegion rv = new LayeredRegion();
            CopyTo(rv);
            return rv;
        }

        public override bool CopyTo(object obj) {
            if (!(obj is LayeredRegion))
                return false;
            LayeredRegion dst = (LayeredRegion)obj;
            LayeredRegion.CopyLayeredRegion(dst, this);
            return true;
        }

        public string GetDrawLayer() {
            return GetLayerLevel(layerLevel);
        }
        public void SetDrawLayer(string layer) {
            layerLevel = GetLayerLevel(layer);
        }
        public void SetVertexColor(float r, float g, float b) {
            SetVertexColor(r, g, b, 1.0f);
        }

        public virtual void SetVertexColor(float r, float g, float b, float a) {
            ColorEx newColor = new ColorEx(a, r, g, b);
            if (newColor.CompareTo(vertexColor) != 0) {
                SetAlpha(a);
                if (colorRect == null)
                    colorRect = new ColorRect();
                colorRect.TopLeft = new ColorEx(a, r, g, b);
                colorRect.BottomLeft = new ColorEx(a, r, g, b);
                colorRect.TopRight = new ColorEx(a, r, g, b);
                colorRect.BottomRight = new ColorEx(a, r, g, b);
                vertexColor = newColor;
                window.Dirty = true;
            }
        }

        protected static void CopyLayeredRegion(LayeredRegion dst, LayeredRegion src) {
            CopyRegion(dst, src);
            dst.colorRect = 
                (src.colorRect == null) ? null : src.colorRect.Clone();
        }

    }

	public class FontString : LayeredRegion, IFontString {

		protected int fontHeight;
		protected string fontFace;
		protected string initialText = string.Empty;
		protected HorizontalTextFormat justifyH = HorizontalTextFormat.WordWrapCentered;
		protected VerticalTextFormat justifyV = VerticalTextFormat.Centered;
        protected string characterSet = null;
		protected bool nonSpaceWrap = false;
		protected PointF shadowOffset;
		protected TextStyle style = new TextStyle();
        protected Rect inset = new Rect(0, 0, 0, 0);
		// editable font strings do not support multiple styles, and are less
		// efficient, but do support edits.
		protected bool editable = false;
        // This is a non-standard flag, that tells us that we need gdi to do 
        // the text layout
        protected bool complex = false;
        // This is a non-standard flag that tells us to write to the texture
        // instead of an image that is copied.  This saves about 30ms whenever
        // we update the text, but it means we must have an opaque background
        // for the text (since alpha is not supported by gdi talking to the
        // video card textures).
        protected bool dynamic = true;
        // This is a non-standard flag that tells us that our general strategy
        // for laying out this text is right-to-left.  This will be ignored
        // unless the complex member is also set.  This is really only needed
        // in some corner cases where it isn't clear what way to display the
        // text from the sequence of characters (e.g. initial digits in arabic).
        protected bool rightToLeft = false;

		internal override string GetInterfaceName() {
			return "IFontString";
		}

		public override void SetWindowProperties() {
			SetWindowProperties(window, size, position);
		}

        internal override void CascadeAlpha(float parentAlpha) {
            // apply the new alpha to our window
            window.Alpha = widgetAlpha * parentAlpha;
        }

		internal override void ShowWindows() {
			if (!this.IsHidden)
				window.Visible = true;
		}

		internal override void HideWindows() {
			window.Visible = false;
		}

		protected override void DisposeWindows() {
			DisposeWindow(window);
			window = null;
		}

		public override void Prepare(Window parent) {
            base.Prepare(parent);
			if (this.IsVirtual)
				return;
			SetText(initialText);
		}


		#region Copy Methods

		public override InterfaceLayer Clone() {
			FontString rv = new FontString();
			CopyTo(rv);
			return rv;
		}

		public override bool CopyTo(object obj) {
			if (!(obj is FontString))
				return false;
			FontString dst = (FontString)obj;
			FontString.CopyFontString(dst, this);
			return true;
		}

		protected static void CopyFontString(FontString dst, FontString src) {
			CopyLayeredRegion(dst, src);
			dst.fontHeight = src.fontHeight;
			dst.fontFace = src.fontFace;
			dst.initialText = src.initialText;
			dst.justifyH = src.justifyH;
			dst.justifyV = src.justifyV;
            dst.characterSet = src.characterSet;
			dst.nonSpaceWrap = src.nonSpaceWrap;
			dst.shadowOffset = src.shadowOffset;
			dst.inset = src.inset;
			dst.style = new TextStyle(src.style);
			dst.editable = src.editable;
            // These are our own flags, that aren't standard
            dst.complex = src.complex;
            dst.dynamic = src.dynamic;
            dst.rightToLeft = src.rightToLeft;
		}

		#endregion

        /// <summary>
        ///   Resolve the character set based on the xml attribute value for
        ///   "bytes".  If this is a number (e.g. 256), we will use the 
        ///   character set [0,255].  We differ from the standard here by 
        ///   using the string as the set of characters if the string is not
        ///   a number.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static string GetCharacterSet(string attr) {
            try {
                int numChars = int.Parse(attr);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < numChars; ++i)
                    sb.Append((char)i);
                return sb.ToString();
            } catch (FormatException) {
                log.WarnFormat("Invalid bytes attribute: {0}", attr);
                return attr;
            }
        }
		private static HorizontalTextFormat GetJustifyH(string val) {
			switch (val) {
				case "LEFT":
					return HorizontalTextFormat.WordWrapLeft;
				case "CENTER":
					return HorizontalTextFormat.WordWrapCentered;
				case "RIGHT":
					return HorizontalTextFormat.WordWrapRight;
				default:
					throw new Exception("Invalid justifyH: " + val);
			}
		}

		private static VerticalTextFormat GetJustifyV(string val) {
			switch (val) {
				case "TOP":
					return VerticalTextFormat.Top;
                case "MIDDLE":
                    return VerticalTextFormat.Centered;
				case "BOTTOM":
					return VerticalTextFormat.Bottom;
				default:
					throw new Exception("Invalid justifyV: " + val);
			}
		}

		#region Xml Parsing Methods

		protected override bool HandleElement(XmlElement node) {
			switch (node.Name) {
				case "FontHeight":
					fontHeight = (int)XmlUi.ReadValue(node);
					return true;
                case "Color":
                    style.textColor = XmlUi.ReadColor(node);
                    return true;
				case "Shadow":
					if (!HandleShadow(node))
						return false;
					style.shadowEnabled = true;
					return true;
                case "BackgroundColor":
                    // This is a non-standard option
                    style.bgColor = XmlUi.ReadColor(node);
                    style.bgEnabled = true;
                    return true;
				default:
					return base.HandleElement(node);
			}
		}
		protected override bool HandleAttribute(XmlAttribute attr) {
			switch (attr.Name) {
				case "font":
					fontFace = attr.Value;
					if (fontFace.Contains("\\"))
						fontFace = fontFace.Substring(fontFace.LastIndexOf('\\') + 1);
					return true;
				case "text":
					if (UiSystem.StringMap.ContainsKey(attr.Value))
						initialText = UiSystem.StringMap[attr.Value];
					else
						initialText = attr.Value;
					return true;
				case "nonspacewrap":
					nonSpaceWrap = Boolean.Parse(attr.Value);
					return true;
				case "bytes":
                    characterSet = FontString.GetCharacterSet(attr.Value);
                    return true;
				case "spacing":
				case "outline":
				case "monochrome":
				case "maxLines":
					// TODO: Handle these
					return false;
				case "justifyV":
                    try {
                        justifyV = FontString.GetJustifyV(attr.Value);
                    } catch (Exception e) {
                        LogUtil.ExceptionLog.WarnFormat("Invalid attribute value: {0}", e);
                    }
					return true;
				case "justifyH":
                    try {
					    justifyH = FontString.GetJustifyH(attr.Value);
                    } catch (Exception e) {
                        LogUtil.ExceptionLog.WarnFormat("Invalid attribute value: {0}", e);
                    }
					return true;
                case "complex":
                    // This is not standard, but I use it for some text tricks
                    try {
                        complex = bool.Parse(attr.Value);
                    } catch (Exception e) {
                        LogUtil.ExceptionLog.WarnFormat("Invalid attribute value: {0}", e);
                    }
                    return true;
                case "dynamic":
                    // This is not standard, but I use it for some text tricks
                    try {
                        dynamic = bool.Parse(attr.Value);
                    } catch (Exception e) {
                        LogUtil.ExceptionLog.WarnFormat("Invalid attribute value: {0}", e);
                    }
                    return true;
                case "rightToLeft":
                    // This is not standard, but I use it for some text tricks
                    try {
                        rightToLeft = bool.Parse(attr.Value);
                    } catch (Exception e) {
                        LogUtil.ExceptionLog.WarnFormat("Invalid attribute value: {0}", e);
                    }
                    return true;
                default:
					return base.HandleAttribute(attr);
			}
		}
		protected bool HandleShadow(XmlElement node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				if (!(childNode is XmlElement))
					continue;
				switch (childNode.Name) {
					case "Offset": {
							SizeF tmp = XmlUi.ReadDimension(childNode);
							// invert y
							shadowOffset = new PointF(tmp.Width, -1 * tmp.Height);
						}
						break;
					case "Color":
						style.shadowColor = XmlUi.ReadColor(childNode);
						break;
					default:
						return false;
				}
			}
			return true;
		}

		#endregion

        //public override void  NotifyChanged() {
        //    base.NotifyChanged();
        //    string fontName = string.Format("{0}-{1}", fontFace, fontHeight);
        //    if (!FontManager.Instance.ContainsKey(fontName)) {
        //        FontFamily family = UiSystem.GetFontFamily(fontFace);
        //        FontManager.Instance.CreateFont(fontName, family, fontHeight, 0, characterSet);
        //    }
        //    font = FontManager.Instance.GetFont(fontName);
        //}

		/// <summary>
		///   Sets the position of the layout frame and of the frame's windows
		/// </summary>
		public override void SetupWindowPosition() {
			base.SetupWindowPosition();

			SizeF tmpSize = this.Size;
			// Logger.Log(0, "Pre-Inset size: {0}", this.Size);
			tmpSize.Width -= (inset.Left + inset.Right);
			tmpSize.Height -= (inset.Top + inset.Bottom);

			// Logger.Log(0, "Post-Inset size: {0}; Inset = {1}", tmpSize, inset);

			PointF tmpPos = this.Position;
			tmpPos.X += inset.Left;
			tmpPos.Y += inset.Top;

			// textWindow.MetricsMode = MetricsMode.Absolute;
			window.Size = tmpSize;
            window.DerivedPosition = tmpPos;

            ((LayeredText)window).HandleAreaChanged();
		}

		internal void SetTextInsets(Rect inset) {
			this.inset = inset;
			SetupWindowPosition();
		}

		protected void UpdateFontString() {
            UpdateFontString((LayeredText)window);
		}

		protected void UpdateFontString(LayeredText textWindow) {
            textWindow.SetFont(fontFace, fontHeight, characterSet);
			textWindow.HorizontalFormat = justifyH;
			textWindow.VerticalFormat = justifyV;
			textWindow.ShadowOffset = shadowOffset;

            textWindow.HandleFontChanged();
            textWindow.HandleFormatChanged();
		}

        //protected void UpdateText() {
        //    // textWindow.SetText(textString);
        //    int fullHeight = (int)Math.Ceiling(textWindow.GetTextHeight(false) + inset.Bottom + inset.Top);
        //    // Logger.Log(0, "full height of " + fullHeight + " for " + textString + "; current height = " + GetHeight() + "; current specified heigth = " + GetSpecifiedHeight());
        //    // If they didn't specify a height, set it here based on the amount needed for the text
        //    if (GetSpecifiedHeight() == 0 && fullHeight != GetHeight()) {
        //        // If they have anchors that determine the correct height, 
        //        // use those.  Otherwise set the height to the text height.
        //        bool foundTop = false;
        //        bool foundBottom = false;
        //        foreach (Anchor anchor in anchors) {
        //            if (anchor.IsImplied)
        //                continue;
        //            if (anchor.IsTopFramepoint())
        //                foundTop = true;
        //            else if (anchor.IsBottomFramepoint())
        //                foundBottom = true;
        //        }
        //        if (!foundTop || !foundBottom) {
        //            Debug.Assert(uiParent.Name != "MvChatFrameOutputScrollingMessageFrame0");
        //            SetHeight(fullHeight);
        //        }
        //    }
        //    textWindow.HandleTextChanged();
        //}

		public override void GetWindows()
		{
			TextureAtlas imageset = AtlasManager.Instance.GetTextureAtlas("MultiverseInterface");
			TextureInfo bgImage = imageset.GetTextureInfo("white"); // for highlight backdrop
            TextureInfo caret = imageset.GetTextureInfo("caret"); // for the cursor/caret
			if (editable) {
				LayeredEditBox editWindow = new LayeredEditBox(this.Name, this.ClipWindow);
				editWindow.Caret = caret;
				window = editWindow;
			} else {
                if (!complex)
                    window = new LayeredStaticText(this.Name, this.ClipWindow);
                else
                    window = new LayeredComplexText(this.Name, this.ClipWindow, dynamic, rightToLeft);
			}
			window.Initialize();
            LayeredText textWindow = (LayeredText)window;
            textWindow.FrameLevel = this.FrameLevel;
            textWindow.FrameStrata = this.FrameStrata;
            textWindow.LayerLevel = this.LayerLevel;
            textWindow.Alpha = 1.0f;
			// textWindow.MetricsMode = MetricsMode.Absolute;
			// textWindow.MinimumSize = new Size(0, 0);
            textWindow.MaximumSize = parentWindow.MaximumSize;
            textWindow.NormalTextStyle = style;
            textWindow.BackgroundImage = bgImage;

            UpdateFontString(textWindow);
        }

		internal void SetJustifyH(HorizontalTextFormat fmt) {
			justifyH = fmt;
		}
		internal void SetJustifyV(VerticalTextFormat fmt) {
			justifyV = fmt;
		}

        public override int GetHeight() {
            if (useHeight)
                return (int)size.Height;
            if (useWidth)
                return (int)((LayeredText)window).GetTextHeight(false);
            return fontHeight;
        }

        public override int GetWidth() {
            if (useWidth)
                return (int)size.Width;
            return GetStringWidth();
        }

		#region IFontString methods

		public int GetStringWidth() {
            return ((LayeredText)window).GetStringWidth();
		}

		public void SetText(string textString)
		{
            LayeredText textWindow = (LayeredText)window;
//             log.DebugFormat("FontString.SetText: textWindow {0}, dirty {1}, textWindow.Text {2}, textString {3}",
//                 textWindow.Name, textWindow.Dirty, textWindow.Text, textString);
//             if (!textWindow.Dirty && textWindow.Text == textString)
//                 return;
            // UpdateFontString();
            // LayeredStaticText has funny behavior, because unless the size is specified, we 
            // will resize the widget based on the text in the widget.  We don't do this resize
            // behavior for the LayeredEditText.
            if (!(textWindow is LayeredEditBox)) {
                // pretend we have the whole canvas, and set the text window
                // size accordingly.  this allows us to layout or text, find
                // out how much space we want, then shrink back down to the 
                // smallest area that will fit the text.
                if (this.IsSizeSpecified)
                    // we can use the whole area we specified
                    textWindow.Size = new SizeF(this.Size.Width - inset.Left - inset.Right,
                                                this.Size.Height - inset.Top - inset.Bottom);
                else if (this.UiParent != null)
                    // pretend we can fill our parent
                    textWindow.Size = uiParent.Size;
            }
            textWindow.SetText(textString);
            if (!(textWindow is LayeredEditBox)) {
                // If we grew our text render area, shrink it back to the
                // smallest area we need.  If the size was specified, leave
                // things as they were.
                if (!this.IsSizeSpecified && this.UiParent != null) {
                    SizeF textSize = new SizeF(textWindow.GetTextWidth(), textWindow.GetTextHeight(false));
                    // Size our widget to match the area used by the text
                    this.Size = new SizeF(textSize.Width + inset.Left + inset.Right, textSize.Height + inset.Top + inset.Bottom);
                    // Now set the text size to the smallest size that will 
                    // contain all of the text, so that centering is updated.
                    textWindow.Size = textSize;
                }
            }
		}

		public void AddText(string textString)
		{
			UpdateFontString();
            LayeredText textWindow = (LayeredText)window;
            textWindow.AddText(textString, style);
            textWindow.HandleTextChanged();
		}

		/// <summary>
		///   In the context of editable text, we want the text that has been
		///   entered.  In the context of non-editable text, we want the text
		///   that is being displayed.
		/// </summary>
		/// <returns></returns>
		public string GetText() {
            LayeredText textWindow = (LayeredText)window;
            if (textWindow is LayeredEditBox)
                // FIXME: We should return the text that has been entered
                //        but we return the text that is displayed instead
                return textWindow.Text;
			else
                return textWindow.Text;
		}

		public void SetJustifyH(string justifyStr) {
			justifyH = FontString.GetJustifyH(justifyStr);
			UpdateFontString();
		}

		public void SetJustifyV(string justifyStr) {
			justifyV = FontString.GetJustifyV(justifyStr);
			UpdateFontString();
		}

        public void SetTextColor(float r, float g, float b) {
            SetTextColor(r, g, b, 1.0f);
        }
   		public void SetTextColor(float r, float g, float b, float a) {
            style.textColor = new ColorEx(a, r, g, b);
			UpdateFontString();
            LayeredText textWindow = (LayeredText)window;
			// Apply the modified text style to all the text
            textWindow.ApplyStyle(style);
		}

		public void SetTextHeight(int pixelHeight) {
			fontHeight = pixelHeight;
			UpdateFontString();
		}

		public override void SetVertexColor(float r, float g, float b, float a) {
            base.SetVertexColor(r, g, b, a);
			UpdateFontString();
		}

		#endregion

        /// <summary>
        ///   Normally, this method wouldn't be exposed (since it isn't part 
        ///   of the IFontString interface), but this is useful for scaling 
        ///   fonts to work well.
        /// </summary>
        /// <returns></returns>
		public int GetFontHeight() {
            return fontHeight;
		}

		// DEBUG
		internal ColorEx GetTextColor() {
			return style.textColor;
		}

		// DEBUG
		internal ColorRect GetColorRect() {
			return colorRect;
		}

		public void SetTextStyleColor(float r, float g, float b)
		{
			style.textColor = new ColorEx(r, g, b);
        }

        #region Properties

        // this should only be called before the font string is prepared
		internal bool EditEnabled {
			get { return editable; }
			set { editable = value; }
		}

        public override FrameStrata FrameStrata {
            get {
                return base.FrameStrata;
            }
            set {
                base.FrameStrata = value;
                LayeredText textWindow = (LayeredText)window;
                if (textWindow != null)
                    textWindow.FrameStrata = value;
            }
        }
        public override int FrameLevel {
            get {
                return base.FrameLevel;
            }
            set {
                base.FrameLevel = value;
                LayeredText textWindow = (LayeredText)window;
                if (textWindow != null)
                    textWindow.FrameLevel = value;
            }
        }
        public override LayerLevel LayerLevel {
            get {
                return base.LayerLevel;
            }
            set {
                base.LayerLevel = value;
                LayeredText textWindow = (LayeredText)window;
                if (textWindow != null)
                    textWindow.LayerLevel = value;
            }
        }

        #endregion
	}

	public class Texture : LayeredRegion, ITexture {

		#region Fields

        List<TextureAtlas> createdImagesets = new List<TextureAtlas>();
		// Rect texCoords = new Rect(0.0f, 1.0f, 0.0f, 1.0f);
        PointF[] texCoords;
		AlphaMode alphaMode = AlphaMode.Blend;
		string file;

		#endregion

		public enum AlphaMode {
            // Hurray for WowWiki
            //* "DISABLE" - opaque texture
            //* "BLEND" - normal painting on top of the background, obeying alpha channels if set (?)
            //* "ALPHAKEY" - 1-bit alpha
            //* "ADD" - additive blend
            //* "MOD" - modulating blend 
			// Warcraft's UI uses Blend and Add.  I'm not sure what the other two should do.
			Disable,
			Blend,
			AlphaKey,
			Add
		}

        public Texture() {
            texCoords = new PointF[4];
            texCoords[0].X = 0;
            texCoords[0].Y = 0;
            texCoords[1].X = 1;
            texCoords[1].Y = 0;
            texCoords[2].X = 0;
            texCoords[2].Y = 1;
            texCoords[3].X = 1;
            texCoords[3].Y = 1;
        }

		private static AlphaMode GetAlphablend(string val) {
			switch (val) {
				case "DISABLE":
					return AlphaMode.Disable;
				case "BLEND":
					return AlphaMode.Blend;
				case "ALPHAKEY":
					return AlphaMode.AlphaKey;
				case "ADD":
					return AlphaMode.Add;
				default:
					throw new Exception("Invalid alphablend: " + val);
			}
		}
		public override void SetupWindowPosition() {
			base.SetupWindowPosition();

			// window.MetricsMode = MetricsMode.Absolute;
			window.Size = size;
			window.DerivedPosition = position;

			log.DebugFormat("Put texture {0} at {1}", this.Name, position);
		}

		public override void SetWindowProperties() {
			SetWindowProperties(window, size, position);
		}

        internal override void CascadeAlpha(float parentAlpha) {
            // apply the new alpha to our window
            window.Alpha = widgetAlpha * parentAlpha;
        }

		internal override void ShowWindows() {
			if (!this.IsHidden)
				window.Visible = true;
		}

		internal override void HideWindows() {
			window.Visible = false;
		}

		public override void Dispose(bool removeFromMap) {
			foreach (TextureAtlas imageset in createdImagesets)
				AtlasManager.Instance.DestroyAtlas(imageset);
			base.Dispose(removeFromMap);
		}

		protected override void DisposeWindows() {
			DisposeWindow(window);
			window = null;
		}	

		internal override string GetInterfaceName() {
			return "ITexture";
		}


		#region Copy Methods

		public override InterfaceLayer Clone() {
			Texture rv = new Texture();
			CopyTo(rv);
			return rv;
		}

		public override bool CopyTo(object obj) {
			if (!(obj is Texture))
				return false;
			Texture dst = (Texture)obj;
			Texture.CopyTexture(dst, this);
			return true;
		}

		protected static void CopyTexture(Texture dst, Texture src) {
            LayeredRegion.CopyLayeredRegion(dst, src);
			dst.texCoords = src.texCoords;
			dst.alphaMode = src.alphaMode;
			dst.file = src.file;
		}

		#endregion

		#region Xml Parsing Methods

		protected override bool HandleElement(XmlElement node) {
			switch (node.Name) {
				case "TexCoords":
					texCoords = XmlUi.ReadTexCoords(node);
					return true;
				case "Color":
					colorRect = new ColorRect(XmlUi.ReadColor(node));
					return true;
				case "Gradient":
					colorRect = XmlUi.ReadGradient(node);
					return true;
				default:
					return base.HandleElement(node);
			}
		}

		protected override bool HandleAttribute(XmlAttribute attr) {
			switch (attr.Name) {
				case "file":
					// In my case, the file attribute refers to an imageset and image
					file = attr.Value;
					return true;
				case "alphaMode":
					alphaMode = Texture.GetAlphablend(attr.Value);
					return true;
				default:
					return base.HandleAttribute(attr);
			}
		}

		#endregion

		//public override void NotifyChanged() {
		//    base.NotifyChanged();
		//    char[] delims = {'/', '\\'};
		//    string[] vals = file.Split(delims);
		//    if (vals.Length != 2)
		//        throw new Exception("Invalid file: " + file);
		//    string imagesetName = vals[0];
		//    string imageName = vals[1];
		//    Imageset imageset = ImagesetManager.Instance.GetImageset(imagesetName);
		//    image = imageset.GetImage(imageName);
		//}
	
		#region ITexture methods

        public void SetTexCoord(float x0, float x1, float y0, float y1) {
            if (texCoords[0].X != x0 || texCoords[0].Y != y0 || 
                texCoords[1].X != x1 || texCoords[0].Y != y0 ||
                texCoords[2].X != x0 || texCoords[0].Y != y1 || 
                texCoords[3].X != x1 || texCoords[0].Y != y1) {

                texCoords[0] = new PointF(x0, y0);
                texCoords[1] = new PointF(x1, y0);
                texCoords[2] = new PointF(x0, y1);
                texCoords[3] = new PointF(x1, y1);
                UpdateTexture();
            }
        }

        public void SetTexCoord(float ul_x, float ul_y, float ll_x, float ll_y,
                                float ur_x, float ur_y, float lr_x, float lr_y) {
            if (texCoords[0].X != ul_x || texCoords[0].Y != ul_y || 
                texCoords[1].X != ur_x || texCoords[0].Y != ur_y ||
                texCoords[2].X != ll_x || texCoords[0].Y != ll_y || 
                texCoords[3].X != lr_x || texCoords[0].Y != lr_y) {

                texCoords[0] = new PointF(ul_x, ul_y);
                texCoords[1] = new PointF(ur_x, ur_y);
                texCoords[2] = new PointF(ll_x, ll_y);
                texCoords[3] = new PointF(lr_x, lr_y);
                UpdateTexture();
            }
        }

        // TODO: Add support for the 8 argument version of SetTexCoord

		public void SetTexture(string textureFile) {
			file = textureFile;
			UpdateTexture();
		}

		public override void SetVertexColor(float r, float g, float b, float a) {
			base.SetVertexColor(r, g, b, a);
            UpdateTexture();
		}

		#endregion

		/// <summary>
		///   Apply file, texCoords and colorRect to the texture window
		/// </summary>
		protected void UpdateTexture() {
			window.Dirty = true;
            LayeredStaticImage imageWindow = (LayeredStaticImage)window;
			TextureAtlas imageset = null;
			TextureInfo image = null;
			if (UiSystem.GetImage(file, out imageset, out image))
				createdImagesets.Add(imageset);
			if (image == null) {
				log.InfoFormat("Using transparent texture instead of {1} for widget {0}", name, file);
                imageset = AtlasManager.Instance.GetTextureAtlas("MultiverseInterface");
				image = imageset.GetTextureInfo("default");
            } else if (texCoords[0].X != 0.0f || texCoords[0].Y != 0.0f ||
                       texCoords[1].X != 1.0f || texCoords[1].Y != 0.0f ||
                       texCoords[2].X != 0.0f || texCoords[2].Y != 1.0f ||
                       texCoords[3].X != 1.0f || texCoords[3].Y != 1.0f) {
                string newName = string.Format("_{0}_[{1},{2},{3},{4},{5},{6}]", this.Name,
                                               texCoords[0].X, texCoords[0].Y,
                                               texCoords[1].X, texCoords[1].Y,
                                               texCoords[2].X, texCoords[2].Y,
                                               texCoords[3].X, texCoords[3].Y);
                if (!imageset.ContainsKey(newName)) {
                    // Convert the relative texture coordinates into absolute ones
                    PointF[] newPoints = new PointF[4];
                    newPoints[0] = new PointF(image.Left + texCoords[0].X * image.Width,
                                             image.Top + texCoords[0].Y * image.Height);
                    newPoints[1] = new PointF(image.Left + texCoords[1].X * image.Width,
                                             image.Top + texCoords[1].Y * image.Height);
                    newPoints[2] = new PointF(image.Left + texCoords[2].X * image.Width,
                                             image.Top + texCoords[2].Y * image.Height);
                    newPoints[3] = new PointF(image.Left + texCoords[3].X * image.Width,
                                             image.Top + texCoords[3].Y * image.Height);
                    imageset.DefineImage(newName, newPoints);
                }
				// Logger.Log(0, "Defining new image {0} in {1} at {2}", newName, imageset.Name, newArea);
				image = imageset.GetTextureInfo(newName);
			}
			imageWindow.SetImage(image);
			if (colorRect != null) // TODO: Should I be using SetImageColors?
				imageWindow.SetImageColors(colorRect);
		}

		public override void GetWindows() {
			LayeredStaticImage imageWindow = new LayeredStaticImage(this.Name, this.ClipWindow);
			imageWindow.Initialize();
            imageWindow.FrameStrata = this.FrameStrata;
            imageWindow.FrameLevel = this.FrameLevel;
            imageWindow.LayerLevel = this.LayerLevel;
            imageWindow.LevelOffset = this.LayerOffset;
            imageWindow.Alpha = 1.0f;
			// imageWindow.MetricsMode = MetricsMode.Absolute;
			// imageWindow.MinimumSize = new Size(0, 0);
            imageWindow.MaximumSize = parentWindow.MaximumSize;

			window = imageWindow;

			UpdateTexture();
		}

        #region Properties

        public override FrameStrata FrameStrata {
            get {
                return base.FrameStrata;
            }
            set {
                base.FrameStrata = value;
                LayeredStaticImage imageWindow = window as LayeredStaticImage;
                if (imageWindow != null)
                    imageWindow.FrameStrata = value;
            }
        }
        public override int FrameLevel {
            get {
                return base.FrameLevel;
            }
            set {
                base.FrameLevel = value;
                LayeredStaticImage imageWindow = window as LayeredStaticImage;
                if (imageWindow != null)
                    imageWindow.FrameLevel = value;
            }
        }
        public override LayerLevel LayerLevel {
            get {
                return base.LayerLevel;
            }
            set {
                base.LayerLevel = value;
                LayeredStaticImage imageWindow = window as LayeredStaticImage;
                if (imageWindow != null)
                    imageWindow.LayerLevel = value;
            }
        }
        public override int LayerOffset {
            get {
                return base.LayerOffset;
            }
            set {
                base.LayerOffset = value;
                LayeredStaticImage imageWindow = window as LayeredStaticImage;
                if (imageWindow != null)
                    imageWindow.LevelOffset = value;
            }
        }

        #endregion
    }

	public class Frame : Region, IFrame {
		// TitleRegion, ResizeBounds, Backdrop, HitRectInsets, Layers, Frames, Scripts
		// alpha, parent, toplevel, movable, resizable, frameStrata, frameLevel, id, enableMouse, enableKeyboard

        // OnClick (in Button)
        // OnValueChanged (in Slider and StatusBar)
        // OnUpdateModel (in Model)
		/*
				<xs:element name="OnLoad" type="xs:string"/>
				<xs:element name="OnSizeChanged" type="xs:string"/>
				<xs:element name="OnEvent" type="xs:string"/>
				<xs:element name="OnUpdate" type="xs:string"/>
				<xs:element name="OnShow" type="xs:string"/>
				<xs:element name="OnHide" type="xs:string"/>
				<xs:element name="OnEnter" type="xs:string"/>
				<xs:element name="OnLeave" type="xs:string"/>
				<xs:element name="OnMouseDown" type="xs:string"/>
				<xs:element name="OnMouseUp" type="xs:string"/>
				<xs:element name="OnMouseWheel" type="xs:string"/>
				<xs:element name="OnDragStart" type="xs:string"/>
				<xs:element name="OnDragStop" type="xs:string"/>
				<xs:element name="OnReceiveDrag" type="xs:string"/>
				<xs:element name="OnClick" type="xs:string"/>
				<xs:element name="OnValueChanged" type="xs:string"/>
				<xs:element name="OnUpdateModel" type="xs:string"/>
				<xs:element name="OnAnimFinished" type="xs:string"/>
				<xs:element name="OnEnterPressed" type="xs:string"/>
				<xs:element name="OnEscapePressed" type="xs:string"/>
				<xs:element name="OnSpacePressed" type="xs:string"/>
				<xs:element name="OnTabPressed" type="xs:string"/>
				<xs:element name="OnTextChanged" type="xs:string"/>
				<xs:element name="OnTextSet" type="xs:string"/>
				<xs:element name="OnEditFocusGained" type="xs:string"/>
				<xs:element name="OnEditFocusLost" type="xs:string"/>
				<xs:element name="OnHorizontalScroll" type="xs:string"/>
				<xs:element name="OnVerticalScroll" type="xs:string"/>
				<xs:element name="OnScrollRangeChanged" type="xs:string"/>
				<xs:element name="OnChar" type="xs:string"/>
				<xs:element name="OnKeyDown" type="xs:string"/>
				<xs:element name="OnKeyUp" type="xs:string"/>
				<xs:element name="OnColorSelect" type="xs:string"/>
				<xs:element name="OnHyperlinkEnter" type="xs:string"/>
				<xs:element name="OnHyperlinkLeave" type="xs:string"/>
				<xs:element name="OnHyperlinkClick" type="xs:string"/>
				<xs:element name="OnMessageScrollChanged" type="xs:string"/>
				<xs:element name="OnMovieFinished" type="xs:string"/>
				<xs:element name="OnDoubleClick" type="xs:string"/>
		*/

        #region Events

        public event KeyboardEventHandler CharEvent;
        public event EventHandler DragStartEvent;
        public event EventHandler DragStopEvent;
        public event MouseEventHandler EnterEvent;
        public event GenericEventHandler EventEvent;
        public event EventHandler HideEvent;
        public event KeyboardEventHandler KeyDownEvent;
        public event KeyboardEventHandler KeyUpEvent;
        public event MouseEventHandler LeaveEvent;
        public event EventHandler LoadEvent;
        public event MouseEventHandler MouseDownEvent;
        public event MouseEventHandler MouseUpEvent;
        public event FloatEventHandler MouseWheelEvent;
        public event EventHandler ReceiveDragEvent;
        public event EventHandler ShowEvent;
        public event EventHandler SizeChangedEvent;
        public event FloatEventHandler UpdateEvent;

        // Internal method (not tied to script events)
        // This is used by ColorPicker, EditBox and Slider
        public event MouseEventHandler MouseMoveEvent;

        #endregion

        protected int id = 0;
        protected bool enableMouse = false;
        protected bool enableKeyboard = false;
		protected Rect hitRectInsets = new Rect(0, 0, 0, 0);
		protected Rect bgInsets = new Rect(0, 0, 0, 0);
		protected int bgTileSize;
		protected int edgeSize;
		protected bool bgTile;
		protected string bgFile;
		protected string edgeFile;

        protected LayeredStaticImage bgWindow;
		protected LayeredStaticImage[] edgeWindows;
		protected string[] edgeNames = { "Left", "Right", "Top", "Bottom", "TopLeft", "TopRight", "BottomLeft", "BottomRight" };
		//protected Window leftWindow;
		//protected Window rightWindow;
		//protected Window topWindow;
		//protected Window bottomWindow;
		//protected Window topLeftWindow;
		//protected Window topRightWindow;
		//protected Window bottomLeftWindow;
		//protected Window bottomRightWindow;

		// List of imagesets that were created for this widget
        List<TextureAtlas> createdImagesets = new List<TextureAtlas>();

		// Code for the class's event handlers
		protected List<int> eventScripts = new List<int>();
        
        // Mapping from the script name to the script delegate 
        // (e.g. OnClick to the MouseEventHandler that does the work).
        protected Dictionary<string, object> scriptEventHandlers = 
            new Dictionary<string, object>();
		
		// Sub elements
		protected List<InterfaceLayer> elements = new List<InterfaceLayer>();

        // Properties dictionary that allows the script to set
        // user-defined properties on the frame object.  This
        // is used for things like storing the unit with a health bar.
        protected Dictionary<string, object> properties = 
            new Dictionary<string, object>();

		public override void Dispose(bool removeFromMap) {
			foreach (InterfaceLayer element in elements)
				element.Dispose(removeFromMap);
			base.Dispose(removeFromMap);
		}

		internal override string GetInterfaceName() {
			return "IFrame";
		}

		public override void SetUiParent(Region parentFrame) {
			uiParent = parentFrame;
			foreach (InterfaceLayer element in elements)
				element.SetUiParent(this);
		}

#if NOT
        public void SetScript(string handler, MouseEventHandler method) {
            SetScriptInternal(handler, method);
        }

        public void SetScript(string handler, KeyboardEventHandler method) {
            SetScriptInternal(handler, method);
        }

        public void SetScript(string handler, EventHandler method) {
            SetScriptInternal(handler, method);
        }

        public void SetScript(string handler, FloatEventHandler method) {
            SetScriptInternal(handler, method);
        }

        public void SetScript(string handler, GenericEventHandler method) {
            SetScriptInternal(handler, method);
        }
#endif

        public void SetScript(string handler, object method) {
            if (method is IronPython.Runtime.Calls.PythonFunction)
                method = GetScriptDelegate(handler, (IronPython.Runtime.Calls.PythonFunction)method);
            SetScriptInternal(handler, method);
        }

        public void SetScriptInternal(string handler, object method) {
            // Clear out the old script
            object old_method = null;
            scriptEventHandlers.TryGetValue(handler, out old_method);
            SetScriptHelper(handler, old_method, method);
            scriptEventHandlers[handler] = method;
        }

        public virtual bool HasScript(string handler) {
            switch (handler) {
                case "OnChar":
                case "OnDragStart":
                case "OnDragStop":
                case "OnEnter":
                case "OnEvent":
                case "OnHide":
                case "OnKeyDown":
                case "OnKeyUp":
                case "OnLeave":
                case "OnLoad":
                case "OnMouseDown":
                case "OnMouseUp":
                case "OnMouseWheel":
                case "OnReceiveDrag":
                case "OnShow":
                case "OnSizeChanged":
                case "OnUpdate":
                    return true;
            }
            return false;
        }

        protected virtual object GetScriptDelegate(string handler, IronPython.Runtime.Calls.PythonFunction method) {
            switch (handler) {
                case "OnChar":
                    return UiScripting.SetupDelegate<KeyboardEventHandler>(method.Name);
                case "OnDragStart":
                case "OnDragStop":
                    return UiScripting.SetupDelegate<EventHandler>(method.Name);
                case "OnEnter":
                    return UiScripting.SetupDelegate<MouseEventHandler>(method.Name);
                case "OnEvent":
                    return UiScripting.SetupDelegate<GenericEventHandler>(method.Name);
                case "OnHide":
                    return UiScripting.SetupDelegate<EventHandler>(method.Name);
                case "OnKeyDown":
                case "OnKeyUp":
                    return UiScripting.SetupDelegate<KeyboardEventHandler>(method.Name);
                case "OnLeave":
                    return UiScripting.SetupDelegate<MouseEventHandler>(method.Name);
                case "OnLoad":
                    return UiScripting.SetupDelegate<EventHandler>(method.Name);
                case "OnMouseDown":
                case "OnMouseUp":
                    return UiScripting.SetupDelegate<MouseEventHandler>(method.Name);
                case "OnMouseWheel":
                    return UiScripting.SetupDelegate<FloatEventHandler>(method.Name);
                case "OnReceiveDrag":
                case "OnShow":
                case "OnSizeChanged":
                    return UiScripting.SetupDelegate<EventHandler>(method.Name);
                case "OnUpdate":
                    return UiScripting.SetupDelegate<FloatEventHandler>(method.Name);
                default:
                    return null;
            }
        }

        protected virtual void SetScriptHelper(string handler, object old_method, object new_method) {
            switch (handler) {
                case "OnChar":
                    if (old_method != null) {
                        KeyboardEventHandler eventHandler = old_method as KeyboardEventHandler;
                        this.CharEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        KeyboardEventHandler eventHandler = new_method as KeyboardEventHandler;
                        this.CharEvent += eventHandler;
                    }
                    break;
                case "OnDragStart":
                    if (old_method != null) {
                        EventHandler eventHandler = old_method as EventHandler;
                        this.DragStartEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        EventHandler eventHandler = new_method as EventHandler;
                        this.DragStartEvent += eventHandler;
                    }
                    break;
                case "OnDragStop":
                    if (old_method != null) {
                        EventHandler eventHandler = old_method as EventHandler;
                        this.DragStopEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        EventHandler eventHandler = new_method as EventHandler;
                        this.DragStopEvent += eventHandler;
                    }
                    break;
                case "OnEnter":
                    if (old_method != null) {
                        MouseEventHandler eventHandler = old_method as MouseEventHandler;
                        this.EnterEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        MouseEventHandler eventHandler = new_method as MouseEventHandler;
                        this.EnterEvent += eventHandler;
                    }
                    break;
                case "OnEvent":
                    if (old_method != null) {
                        GenericEventHandler eventHandler = old_method as GenericEventHandler;
                        this.EventEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        GenericEventHandler eventHandler = new_method as GenericEventHandler;
                        this.EventEvent += eventHandler;
                    }
                    break;
                case "OnHide":
                    if (old_method != null) {
                        EventHandler eventHandler = old_method as EventHandler;
                        this.HideEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        EventHandler eventHandler = new_method as EventHandler;
                        this.HideEvent += eventHandler;
                    }
                    break;
                case "OnKeyDown":
                    if (old_method != null) {
                        KeyboardEventHandler eventHandler = old_method as KeyboardEventHandler;
                        this.KeyDownEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        KeyboardEventHandler eventHandler = new_method as KeyboardEventHandler;
                        this.KeyDownEvent += eventHandler;
                    }
                    break;
                case "OnKeyUp":
                    if (old_method != null) {
                        KeyboardEventHandler eventHandler = old_method as KeyboardEventHandler;
                        this.KeyUpEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        KeyboardEventHandler eventHandler = new_method as KeyboardEventHandler;
                        this.KeyUpEvent += eventHandler;
                    }
                    break;
                case "OnLeave":
                    if (old_method != null) {
                        MouseEventHandler eventHandler = old_method as MouseEventHandler;
                        this.LeaveEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        MouseEventHandler eventHandler = new_method as MouseEventHandler;
                        this.LeaveEvent += eventHandler;
                    }
                    break;
                case "OnLoad":
                    if (old_method != null) {
                        EventHandler eventHandler = old_method as EventHandler;
                        this.LoadEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        EventHandler eventHandler = new_method as EventHandler;
                        this.LoadEvent += eventHandler;
                    }
                    break;
                case "OnMouseDown":
                    if (old_method != null) {
                        MouseEventHandler eventHandler = old_method as MouseEventHandler;
                        this.MouseDownEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        MouseEventHandler eventHandler = new_method as MouseEventHandler;
                        this.MouseDownEvent += eventHandler;
                    }
                    break;
                case "OnMouseUp":
                    if (old_method != null) {
                        MouseEventHandler eventHandler = old_method as MouseEventHandler;
                        this.MouseUpEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        MouseEventHandler eventHandler = new_method as MouseEventHandler;
                        this.MouseUpEvent += eventHandler;
                    }
                    break;
                case "OnMouseWheel":
                    if (old_method != null) {
                        FloatEventHandler eventHandler = old_method as FloatEventHandler;
                        this.MouseWheelEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        FloatEventHandler eventHandler = new_method as FloatEventHandler;
                        this.MouseWheelEvent += eventHandler;
                    }
                    break;
                case "OnReceiveDrag":
                    if (old_method != null) {
                        EventHandler eventHandler = old_method as EventHandler;
                        this.ReceiveDragEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        EventHandler eventHandler = new_method as EventHandler;
                        this.ReceiveDragEvent += eventHandler;
                    }
                    break;
                case "OnShow":
                    if (old_method != null) {
                        EventHandler eventHandler = old_method as EventHandler;
                        this.ShowEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        EventHandler eventHandler = new_method as EventHandler;
                        this.ShowEvent += eventHandler;
                    }
                    break;
                case "OnSizeChanged":
                    if (old_method != null) {
                        EventHandler eventHandler = old_method as EventHandler;
                        this.SizeChangedEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        EventHandler eventHandler = new_method as EventHandler;
                        this.SizeChangedEvent += eventHandler;
                    }
                    break;
                case "OnUpdate":
                    if (old_method != null) {
                        FloatEventHandler eventHandler = old_method as FloatEventHandler;
                        this.UpdateEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        FloatEventHandler eventHandler = new_method as FloatEventHandler;
                        this.UpdateEvent += eventHandler;
                    }
                    break;
            }
        }

        public virtual object GetScript(string handler) {
            object method = null;
            scriptEventHandlers.TryGetValue(handler, out method);
            return method;
        }

		public virtual void SetupEventScripts() {
			foreach (int scriptId in eventScripts) {
				EventScript eventScript = UiSystem.EventScripts[scriptId];
                if (HasScript(eventScript.eventName))
                    SetScriptInternal(eventScript.eventName, eventScript.EventHandler);
			}
		}

		#region Copy Methods

		public override InterfaceLayer Clone() {
			Frame rv = new Frame();
			CopyTo(rv);
			return rv;
		}

		public override bool CopyTo(object obj) {
			if (!(obj is Frame))
				return false;
			Frame dst = (Frame)obj;
			Frame.CopyFrame(dst, this);
			return true;
		}

		protected static void CopyFrame(Frame dst, Frame src) {
			Frame.CopyFrame(dst, src, null);
		}

		/// <summary>
		///   Copy the elements from src frame to dst frame, calling method 
		///   on each of the elements.
		///   This is used so that the classes that extend frame have a hook
		///   into the hideous copy system.
		/// </summary>
		/// <param name="dst">the destination frame</param>
		/// <param name="src">the source frame</param>
		/// <param name="method">the method that should be called for each copied element</param>
		protected static void CopyFrame(Frame dst, Frame src, FrameElementHandler method) {
			Region.CopyRegion(dst, src);
			dst.elements = new List<InterfaceLayer>();
			foreach (InterfaceLayer element in src.elements) {
				InterfaceLayer newFrame = element.Clone();
                newFrame.UiParent = dst;
				dst.elements.Add(newFrame);
				if (method != null)
					method(dst, src, newFrame, element);
			}
            dst.id = src.id;
            dst.enableKeyboard = src.enableKeyboard;
            dst.enableMouse = src.enableMouse;
            dst.hitRectInsets = src.hitRectInsets;
			dst.eventScripts = new List<int>(src.eventScripts);
			dst.bgInsets = src.bgInsets;
			dst.bgTileSize = src.bgTileSize;
			dst.edgeSize = src.edgeSize;
			dst.bgTile = src.bgTile;
			dst.bgFile = src.bgFile;
			dst.edgeFile = src.edgeFile;
		}

		#endregion

        /// <summary>
        ///   Check to see if the point is within the hit rectangle of this 
        ///   frame.
        ///   FIXME: A frame may be in a scroll frame, in which case, this
        ///   check needs to walk up the tree to make sure that we are not
        ///   in the clipped area. Bug #622
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
		public override bool CheckHit(PointF pt) {
			if (position.X + hitRectInsets.Left > pt.X ||
				position.Y + hitRectInsets.Top > pt.Y ||
				position.X - hitRectInsets.Right + size.Width < pt.X ||
				position.Y - hitRectInsets.Bottom + size.Height < pt.Y ||
				this.IsHidden)
				return false;
			return true;
		}
		public override void ResolveParentStrings() {
			base.ResolveParentStrings();
			foreach (InterfaceLayer element in elements)
				element.ResolveParentStrings();
		}

		internal void AddElement(InterfaceLayer element) {
            if (element.FrameStrata == FrameStrata.Unknown)
                element.FrameStrata = this.FrameStrata;
            if (element.LayerLevel == LayerLevel.Unknown)
                element.LayerLevel = this.LayerLevel;
			elements.Add(element);
		}

		#region Xml Parsing Methods

		protected override bool HandleAttribute(XmlAttribute attr) {
			switch (attr.Name) {
				case "parent":
					if (attr.Value == "UIParent")
						uiParent = null;
					else
						uiParent = UiSystem.FrameMap[attr.Value];
					return true;
				case "alpha":
					widgetAlpha = float.Parse(attr.Value);
					return true;
				case "id":
					id = int.Parse(attr.Value);
					return true;
                case "frameStrata":
                    this.FrameStrata = InterfaceLayer.GetFrameStrata(attr.Value);
                    return true;
                case "frameLevel":
                    this.FrameLevel = int.Parse(attr.Value);
                    return true;
                case "enableMouse":
                    enableMouse = bool.Parse(attr.Value);
                    return true;
                case "enableKeyboard":
                    // FIXME: For 1.1 or 1.01, we should special case this, 
                    // and not throw an exception, since we included the bad
                    // file in our sample assets.
                    if (attr.Value == "fase") {
                        log.Error("Invalid attribute value for enableKeyboard: fase");
                        enableKeyboard = false;
                    } else
                        enableKeyboard = bool.Parse(attr.Value);
                    return true;
                case "toplevel":
				case "movable":
				case "resizable":
                    log.InfoFormat("Frame attribute: {0} is not yet supported.", attr.Name);
                    return true;
				default:
					return base.HandleAttribute(attr);
			}
		}

		protected override bool HandleElement(XmlElement node) {
			switch (node.Name) {
				case "Frames":
					ReadFrames(node);
					return true;
				case "Layers":
					ReadLayers(node);
					return true;
				case "TitleRegion":
				case "ResizeBounds":
					// TODO: Handle these
					return true;
				case "HitRectInsets":
					hitRectInsets = XmlUi.ReadInset(node);
					return true;
				case "Backdrop":
					return HandleBackdropElement(node);
				case "Scripts":
					return HandleScriptsElement(node);
				default:
					return base.HandleElement(node);
			}
		}

		public void ReadFrames(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				if (!(childNode is XmlElement))
					continue;
				UiSystem.ReadFrame(childNode, this, null);
			}
		}

		public void ReadLayers(XmlNode node) {
			foreach (XmlNode childNode in node.ChildNodes) {
				if (!(childNode is XmlElement))
					continue;
				if (childNode.Name != "Layer")
					continue;
				Layer layer = new Layer();
				layer.UiParent = this;
				layer.ReadNode(childNode);
                // Default level for layers under frames is ARTWORK
                if (layer.LayerLevel == LayerLevel.Unknown)
                    layer.LayerLevel = LayerLevel.Artwork;
                AddElement(layer);
			}
		}

		protected bool HandleScriptsElement(XmlElement node) {
			if (!node.HasAttribute("language"))
				// Ignore unhandled scripts tag.
				return true;
			if (node.Attributes["language"].Value != "python")
				return true;
			foreach (XmlNode childNode in node.ChildNodes) {
                if (HasScript(childNode.Name)) {
                    int scriptId = UiSystem.AddEventScript(childNode.Name, childNode.InnerXml);
                    eventScripts.Add(scriptId);
                }
            }
			return true;
		}

		protected bool HandleBackdropElement(XmlElement node) {
			foreach (XmlAttribute attr in node.Attributes) {
				switch (attr.Name) {
					case "bgFile":
						bgFile = attr.Value;
						break;
					case "edgeFile":
						edgeFile = attr.Value;
						break;
					case "tile":
						bgTile = bool.Parse(attr.Value);
						break;
					default:
						log.ErrorFormat("Unhandled attribute: {0}", attr.Name);
						break;
				}
			}

			foreach (XmlNode childNode in node.ChildNodes) {
				if (!(childNode is XmlElement))
					continue;
				switch (childNode.Name) {
					case "TileSize":
						bgTileSize = (int)XmlUi.ReadValue(childNode);
						break;
					case "EdgeSize":
						edgeSize = (int)XmlUi.ReadValue(childNode);
						break;
					case "BackgroundInsets":
						bgInsets = XmlUi.ReadInset(childNode);
						break;
					default:
                        log.ErrorFormat("Unhandled element: {0}", childNode.Name);
						break;
				}
			}

			return true;
		}

		#endregion

        public override void Prepare(Window parent) {
            base.Prepare(parent);
			if (this.IsVirtual)
				return;
			UiSystem.debugIndent += 1;
			foreach (InterfaceLayer element in elements)
				element.Prepare(window);
			UiSystem.debugIndent -= 1;
		}

		public override void SetWindowProperties() {
            SetWindowProperties(window, size, position);
		}

		public override void SetupWindowPosition() {
			base.SetupWindowPosition();

			// window.MetricsMode = MetricsMode.Absolute;
			window.Size = size;
            window.DerivedPosition = position;

			if (bgWindow != null) {
				float innerWidth = (float)Math.Max(size.Width - bgInsets.Left - bgInsets.Right, 0);
				float innerHeight = (float)Math.Max(size.Height - bgInsets.Top - bgInsets.Bottom, 0);
				// bgWindow.MetricsMode = MetricsMode.Absolute;
				bgWindow.Size = new SizeF(innerWidth, innerHeight);
				bgWindow.Position = new PointF(bgInsets.Left, bgInsets.Top);
			}
			if (edgeWindows != null) {
				float innerWidth = (float)Math.Max(size.Width - 2 * edgeSize, 0);
				float innerHeight = (float)Math.Max(size.Height - 2 * edgeSize, 0);
				// left
				// edgeWindows[0].MetricsMode = MetricsMode.Absolute;
				edgeWindows[0].Size = new SizeF(edgeSize, innerHeight);
				edgeWindows[0].Position = new PointF(0, edgeSize);
				// right
				// edgeWindows[1].MetricsMode = MetricsMode.Absolute;
				edgeWindows[1].Size = new SizeF(edgeSize, innerHeight);
				edgeWindows[1].Position = new PointF(edgeSize + innerWidth, edgeSize);
				// top
				// edgeWindows[2].MetricsMode = MetricsMode.Absolute;
				edgeWindows[2].Size = new SizeF(innerWidth, edgeSize);
				edgeWindows[2].Position = new PointF(edgeSize, 0);
				// bottom
				// edgeWindows[3].MetricsMode = MetricsMode.Absolute;
				edgeWindows[3].Size = new SizeF(innerWidth, edgeSize);
				edgeWindows[3].Position = new PointF(edgeSize, edgeSize + innerHeight);
				// topleft
				// edgeWindows[4].MetricsMode = MetricsMode.Absolute;
				edgeWindows[4].Size = new SizeF(edgeSize, edgeSize);
				edgeWindows[4].Position = new PointF(0, 0);
				// topright
				// edgeWindows[5].MetricsMode = MetricsMode.Absolute;
				edgeWindows[5].Size = new SizeF(edgeSize, edgeSize);
				edgeWindows[5].Position = new PointF(edgeSize + innerWidth, 0);
				// bottomleft
				// edgeWindows[6].MetricsMode = MetricsMode.Absolute;
				edgeWindows[6].Size = new SizeF(edgeSize, edgeSize);
				edgeWindows[6].Position = new PointF(0, edgeSize + innerHeight);
				// bottomright
				// edgeWindows[7].MetricsMode = MetricsMode.Absolute;
				edgeWindows[7].Size = new SizeF(edgeSize, edgeSize);
				edgeWindows[7].Position = new PointF(edgeSize + innerWidth, edgeSize + innerHeight);
			}
			foreach (InterfaceLayer element in elements)
				element.SetupWindowPosition();
		}

        private void BuildImageWindow(TextureAtlas imageset, TextureInfo image, int index,
									  bool rotated, bool vtile, bool htile) {
			float xOffset = image.Left + index * edgeSize;
			float yOffset = image.Top;
			PointF[] imagePoints = new PointF[4];
			string newName = string.Format("_{0}_{1}", this.Name, edgeNames[index]);
			if (!rotated) {
				imagePoints[0] = new PointF(xOffset, yOffset);
				imagePoints[1] = new PointF(xOffset + edgeSize, yOffset);
				imagePoints[2] = new PointF(xOffset, yOffset + edgeSize);
				imagePoints[3] = new PointF(xOffset + edgeSize, yOffset + edgeSize);
			} else {
				imagePoints[0] = new PointF(xOffset, yOffset + edgeSize);
				imagePoints[1] = new PointF(xOffset, yOffset);
				imagePoints[2] = new PointF(xOffset + edgeSize, yOffset + edgeSize);
				imagePoints[3] = new PointF(xOffset + edgeSize, yOffset);
			}
			imageset.DefineImage(newName, imagePoints);
			edgeWindows[index].SetImage(imageset.GetTextureInfo(newName));
			if (vtile)
				edgeWindows[index].VerticalFormat = VerticalImageFormat.Tiled;
			if (htile)
				edgeWindows[index].HorizontalFormat = HorizontalImageFormat.Tiled;
		}


		protected override void DisposeWindows() {
			DisposeWindow(window);
			window = null;
		}	

		/// <summary>
		///   Apply file, texCoords and colorRect to the texture window
		/// </summary>
		protected void UpdateTextures() {
			if (bgWindow != null) {
				LayeredStaticImage imageWindow;
				TextureAtlas imageset = null;
				TextureInfo image = null;
				if (UiSystem.GetImage(bgFile, out imageset, out image))
					createdImagesets.Add(imageset);
				if (image == null) {
                    log.WarnFormat("Using transparent texture instead of {1} for widget {0}", name, bgFile);
					imageset = AtlasManager.Instance.GetTextureAtlas("MultiverseInterface");
					image = imageset.GetTextureInfo("default");
				}
				imageWindow = bgWindow;
				imageWindow.SetImage(image);
				if (bgTile) {
					imageWindow.VerticalFormat = VerticalImageFormat.Tiled;
					imageWindow.HorizontalFormat = HorizontalImageFormat.Tiled;
				}
			}
			if (edgeWindows != null) {
                TextureAtlas imageset = null;
                TextureInfo image = null;
				if (UiSystem.GetImage(edgeFile, out imageset, out image))
					createdImagesets.Add(imageset);
				if (image == null) {
					log.WarnFormat("Using transparent texture instead of {1} for widget {0}", name, bgFile);
                    imageset = AtlasManager.Instance.GetTextureAtlas("MultiverseInterface");
					image = imageset.GetTextureInfo("default");
				}
				for (int i = 0; i < edgeWindows.Length; ++i) {
					bool rotate = (i == 2 || i == 3);
					bool vtile = bgTile && (i == 0 || i == 1);
					bool htile = bgTile && (i == 2 || i == 3);
					BuildImageWindow(imageset, image, i, rotate, vtile, htile);
				}
			}
		}

		private void SetupWindowPart(Window frameWindow, Window subWindow) {
			LayeredStaticImage imageWindow = (LayeredStaticImage)subWindow;
			imageWindow.Initialize();
            imageWindow.FrameStrata = this.FrameStrata;
            imageWindow.FrameLevel = this.FrameLevel;
            // this is only called for window background and edges
            // imageWindow.Level = this.level;
            imageWindow.Alpha = 1.0f;
			// imageWindow.MetricsMode = MetricsMode.Absolute;
			// imageWindow.MinimumSize = new Size(0, 0);
			imageWindow.MaximumSize = frameWindow.MaximumSize;
			frameWindow.AddChild(imageWindow);
		}

		public override void GetWindows() {
			Window frameWindow = new Window(this.Name);
			frameWindow.Initialize();
			frameWindow.Alpha = 1.0f;
			// frameWindow.MetricsMode = MetricsMode.Absolute;
			// frameWindow.MinimumSize = new Size(0, 0);
            frameWindow.MaximumSize = parentWindow.MaximumSize;
			if (bgFile != null) {
				bgWindow = new LayeredStaticImage("_" + this.Name + "_Background", this.ClipWindow);
                bgWindow.LayerLevel = LayerLevel.Background; // behind the artwork
				SetupWindowPart(frameWindow, bgWindow);
			}
			if (edgeFile != null) {
				edgeWindows = new LayeredStaticImage[8];

				for (int i = 0; i < edgeWindows.Length; ++i) {
                    edgeWindows[i] = new LayeredStaticImage("_" + this.Name + "_" + edgeNames[i], this.ClipWindow);
                    edgeWindows[i].LayerLevel = LayerLevel.Border; // in front of the artwork
					SetupWindowPart(frameWindow, edgeWindows[i]);
				}
			}
			window = frameWindow;
			UpdateTextures();
		}

		internal List<InterfaceLayer> Elements {
			get {
				return elements;
			}
		}

        public override int FrameLevel {
            get {
                if (frameLevel != 0)
                    return frameLevel;
                else if (uiParent != null)
                    return UiParent.FrameLevel + 1;
                else
                    return 1;
            }
            set {
                frameLevel = value;
            }
        }
        public override LayerLevel LayerLevel {
            set {
                base.LayerLevel = value;
                foreach (InterfaceLayer element in elements)
                    if (element.LayerLevel == LayerLevel.Unknown)
                        element.LayerLevel = layerLevel;
            }
        }
        public override FrameStrata FrameStrata {
            set {
                base.FrameStrata = value;
                if (bgWindow != null)
                    bgWindow.FrameStrata = frameStrata;
                if (edgeWindows != null)
    				for (int i = 0; i < edgeWindows.Length; ++i)
                        edgeWindows[i].FrameStrata = frameStrata;
                foreach (InterfaceLayer element in elements)
                    if (element.FrameStrata == FrameStrata.Unknown)
                        element.FrameStrata = frameStrata;
            }
        }

        public void CaptureMouse(bool val) {
            if (val)
                UiSystem.CaptureFrame = this;
            else if (UiSystem.CaptureFrame == this)
                UiSystem.CaptureFrame = null;
        }

		#region IFrame Methods

		public int GetID() {
			return id;
		}
		public void SetID(int val) {
			id = val;
		}
        public void RegisterEvent(string eventName) {
            UiSystem.RegisterEvent(eventName, this);
        }
        public void UnregisterEvent(string eventName) {
            UiSystem.RegisterEvent(eventName, this);
        }
        public override void UpdateVisibility()
        {
            bool desiredVisibility = !this.IsHidden;
            bool currentVisibility = isShown;
            if (desiredVisibility != currentVisibility) {
                foreach (InterfaceLayer element in elements)
                    element.UpdateVisibility();
                if (desiredVisibility && !currentVisibility) {
                    ShowWindows(); // this will show the windows
                    OnShow(new EventArgs());
                    log.DebugFormat("Setting isShown true for {0}/{1}", this.Name, this.window == null ? null : this.window.Name);
                    isShown = true;
                } else if (currentVisibility && !desiredVisibility) {
                    HideWindows(); // this will hide the windows
                    OnHide(new EventArgs());
                    isShown = false;
                }
            }
        }
        public void SetBackdrop(string bgFile, string edgeFile, bool bgTile, int bgTileSize, int edgeSize) {
            this.bgFile = bgFile;
            this.bgTile = bgTile;
            this.bgTileSize = bgTileSize;
            this.edgeFile = edgeFile;
            this.edgeSize = edgeSize;
            UpdateTextures();
            SetupWindowPosition();
        }
        public void SetBackdrop(string bgFile, string edgeFile, bool bgTile, int bgTileSize, int edgeSize,
                                int insetLeft, int insetRight, int insetTop, int insetBottom) {
            SetBackdrop(bgFile, edgeFile, bgTile, bgTileSize, edgeSize);
        }
        public void SetBackdropColor(float r, float g, float b) {
            if (bgWindow == null)
                return;
            ColorRect colors = new ColorRect(new ColorEx(r, g, b));
            LayeredStaticImage imageWindow = bgWindow;
            imageWindow.SetImageColors(colors);
        }
        public void SetBackdropBorderColor(float r, float g, float b) {
            if (edgeWindows == null)
                return;
            ColorRect colors = new ColorRect(new ColorEx(r, g, b));
			for (int i = 0; i < edgeWindows.Length; ++i)
                edgeWindows[i].SetImageColors(colors);
        }
        public int GetFrameLevel() {
            return this.FrameLevel;
        }
        public string GetFrameStrata() {
            return InterfaceLayer.GetFrameStrata(this.FrameStrata);
        }
        public void SetFrameLevel(int level) {
            this.FrameLevel = level;
        }
        public void SetFrameStrata(string strata) {
            this.FrameStrata = InterfaceLayer.GetFrameStrata(strata);
        }
        public bool IsKeyboardEnabled() {
            return enableKeyboard || this.KeyDownEvent != null || this.KeyUpEvent != null;
        }
        public virtual bool IsMouseEnabled() {
            return enableMouse || this.MouseDownEvent != null || this.MouseUpEvent != null;
        }
        // Not exactly a normal IFrame member, but we need a property list
        public Dictionary<string, object> Properties {
            get {
                return properties;
            }
        }
		#endregion

        #region Event Helper Methods
        public virtual void OnChar(KeyEventArgs args) {
            if (CharEvent != null)
                CharEvent(this, args);
        }
        public virtual void OnDragStart(EventArgs args) {
            if (DragStartEvent != null)
                DragStartEvent(this, args);
        }
        public virtual void OnDragStop(EventArgs args) {
            if (DragStopEvent != null)
                DragStopEvent(this, args);
        }
        public virtual void OnEnter(MouseEventArgs args) {
            if (EnterEvent != null)
                EnterEvent(this, args);
        }
        public virtual void OnEvent(GenericEventArgs args) {
            if (EventEvent != null)
                EventEvent(this, args);
        }
        public virtual void OnHide(EventArgs args) {
            if (HideEvent != null)
                HideEvent(this, args);
        }
        public virtual void OnKeyDown(KeyEventArgs args) {
            if (KeyDownEvent != null)
                KeyDownEvent(this, args);
        }
        public virtual void OnKeyUp(KeyEventArgs args) {
            if (KeyUpEvent != null)
                KeyUpEvent(this, args);
        }
        public virtual void OnLeave(MouseEventArgs args) {
            if (LeaveEvent != null)
                LeaveEvent(this, args);
        }
        public virtual void OnLoad(EventArgs args) {
            if (LoadEvent != null)
                LoadEvent(this, args);
        }
        public virtual void OnMouseDown(MouseEventArgs args) {
            if (MouseDownEvent != null)
                MouseDownEvent(this, args);
        }
        public virtual void OnMouseUp(MouseEventArgs args) {
            if (MouseUpEvent != null)
                MouseUpEvent(this, args);
        }
        public virtual void OnMouseWheel(FloatEventArgs args) {
            if (MouseWheelEvent != null)
                MouseWheelEvent(this, args);
        }
        public virtual void OnReceiveDrag(EventArgs args) {
            if (ReceiveDragEvent != null)
                ReceiveDragEvent(this, args);
        }
        public virtual void OnShow(EventArgs args) {
            if (ShowEvent != null)
                ShowEvent(this, args);
        }
        public virtual void OnSizeChanged(EventArgs args) {
            if (SizeChangedEvent != null)
                SizeChangedEvent(this, args);
        }
        
        private static TimingMeter onUpdateFrameMeter = MeterManager.GetMeter("Non-null OnUpdate", "UiCompoent Frame");

        public virtual void OnUpdate(FloatEventArgs args) {
            if (UpdateEvent != null) {
                onUpdateFrameMeter.Enter();
                MeterManager.AddInfoEvent("Frame Update Event: {0}", GetName());
                UpdateEvent(this, args);
                onUpdateFrameMeter.Exit();
            }
        }

        /// <summary>
        ///   This method is not exposed because of scripting, but because
        ///   we expect to be able to drag a slider, drag to select text in
        ///   an edit box, and move the indicator around on a colorselect
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseMove(MouseEventArgs e) {
            if (MouseMoveEvent != null)
                MouseMoveEvent(this, e);
        }

        /// <summary>
        ///   This method is so that the LayeredEditBox can dispatch char 
        ///   events to the frame.  In the future, this should be reworked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleChar(object sender, KeyEventArgs e) {
            OnChar(e);
        }

        internal bool HasEnterEvent {
            get {
                return EnterEvent != null;
            }
        }
        internal bool HasLeaveEvent {
            get {
                return LeaveEvent != null;
            }
        }
        internal bool HasMouseDownEvent {
            get {
                return MouseDownEvent != null;
            }
        }
        internal bool HasMouseUpEvent {
            get {
                return MouseUpEvent != null;
            }
        }
        internal bool HasMouseWheelEvent {
            get {
                return MouseWheelEvent != null;
            }
        }

        // to support the internal MouseMoveEvent
        internal bool HasMouseMoveEvent {
            get {
                return MouseMoveEvent != null;
            }
        }

        #endregion

        internal override void ShowWindows() {
			base.ShowWindows();
			if (!this.IsHidden) {
				window.Visible = true;
				if (bgWindow != null)
					bgWindow.Visible = true;
				if (edgeWindows != null)
					for (int i = 0; i < edgeWindows.Length; ++i)
						edgeWindows[i].Visible = true;
			}
		}

		internal override void HideWindows() {
			base.HideWindows();
			window.Visible = false;
			if (bgWindow != null)
				bgWindow.Visible = false;
			if (edgeWindows != null)
				for (int i = 0; i < edgeWindows.Length; ++i)
					edgeWindows[i].Visible = false;
		}

		internal override void CascadeAlpha(float parentAlpha) {
			foreach (InterfaceLayer element in elements)
				element.CascadeAlpha(widgetAlpha * parentAlpha);
            if (bgWindow != null)
                bgWindow.Alpha = widgetAlpha * parentAlpha;
            if (edgeWindows != null)
				for (int i = 0; i < edgeWindows.Length; ++i)
					edgeWindows[i].Alpha = widgetAlpha * parentAlpha;
		}
    }

	public class Button : Frame, IButton {
		// NormalTexture, PushedTexture, DisabledTexture, HighlightTexture, NormalText, HighlightText, DisabledText, PushedTextOffset
		// text
        // OnClick, OnDoubleClick
		protected Texture normalTexture;
		protected Texture pushedTexture;
		protected Texture disabledTexture;
		protected Texture highlightTexture;

		protected FontString normalText;
		protected FontString highlightText;
		protected FontString disabledText;

		protected PointF pushedTextOffset;

        #region Events
        public event MouseEventHandler ClickEvent;
        public event MouseEventHandler DoubleClickEvent; // never invoked right now
        #endregion

        // Default text
		protected string text = null;

        // The priority here is highlight, then disabled, then pushed
        protected bool highlight = false;
        protected bool enabled = true;
        protected bool pushed = false;
        protected bool pushed_locked = false;


        internal override string GetInterfaceName() {
			return "IButton";
		}

		#region Copy Methods

		public override bool CopyTo(object obj) {
			if (!(obj is Button))
				return false;
			Button dst = (Button)obj;
			Button.CopyButton(dst, this);
			return true;
		}

		protected static void CopyButtonElement(Frame dst, Frame src,
										        InterfaceLayer dst_element, 
										        InterfaceLayer src_element) {
			if (!(src is Button) || !(dst is Button))
				return;
			Button src_button = src as Button;
			Button dst_button = dst as Button;
			// Handle the textures
			if (src_element == src_button.normalTexture)
				dst_button.normalTexture = (Texture)dst_element;
			if (src_element == src_button.pushedTexture)
				dst_button.pushedTexture = (Texture)dst_element;
			if (src_element == src_button.disabledTexture)
				dst_button.disabledTexture = (Texture)dst_element;
			if (src_element == src_button.highlightTexture)
				dst_button.highlightTexture = (Texture)dst_element;

			// Handle the font strings
			if (src_element == src_button.normalText)
				dst_button.normalText = (FontString)dst_element;
			if (src_element == src_button.highlightText)
				dst_button.highlightText = (FontString)dst_element;
			if (src_element == src_button.disabledText)
				dst_button.disabledText = (FontString)dst_element;
		}

		protected static void CopyButton(Button dst, Button src) {
			Frame.CopyFrame(dst, src,
                            new FrameElementHandler(Button.CopyButtonElement));

			dst.text = src.text;
			dst.enabled = src.enabled;
			dst.pushedTextOffset = src.pushedTextOffset;
		}

		public override InterfaceLayer Clone() {
			Button rv = new Button();
			CopyTo(rv);
			return rv;
		}

		#endregion

		#region Xml Parsing Methods

		protected override bool HandleAttribute(XmlAttribute attr) {
			switch (attr.Name) {
				case "text":
					if (UiSystem.StringMap.ContainsKey(attr.Value))
						text = UiSystem.StringMap[attr.Value];
					else
						text = attr.Value;
					return true;
				default:
					return base.HandleAttribute(attr);
			}
		}

		protected override bool HandleElement(XmlElement node) {
			switch (node.Name) {
				case "NormalTexture":
					normalTexture = new Texture();
					UiSystem.ReadFrame(normalTexture, node, this, null);
					normalTexture.AnchorToParentFull();
					return true;
				case "PushedTexture":
					pushedTexture = new Texture();
					UiSystem.ReadFrame(pushedTexture, node, this, null);
					pushedTexture.AnchorToParentFull();
					return true;
				case "DisabledTexture":
					disabledTexture = new Texture();
					UiSystem.ReadFrame(disabledTexture, node, this, null);
					disabledTexture.AnchorToParentFull();
					return true;
				case "HighlightTexture":
                    highlightTexture = new Texture();
                    UiSystem.ReadFrame(highlightTexture, node, this, null);
                    highlightTexture.LayerOffset = 1;
                    highlightTexture.AnchorToParentFull();
					return true;
				case "NormalText":
					normalText = new FontString();
					UiSystem.ReadFrame(normalText, node, this, null);
					normalText.AnchorToParentCenter();
					return true;
				case "HighlightText":
					highlightText = new FontString();
					UiSystem.ReadFrame(highlightText, node, this, null);
					highlightText.AnchorToParentCenter();
					return true;
				case "DisabledText":
					disabledText = new FontString();
					UiSystem.ReadFrame(disabledText, node, this, null);
					disabledText.AnchorToParentCenter();
					return true;
				case "PushedTextOffset":
					SizeF dim = XmlUi.ReadDimension(node);
					// Since WoW uses +y for up, invert it
					pushedTextOffset = new PointF(dim.Width, -dim.Height);
					return true;
				default:
					return base.HandleElement(node);
			}
		}

		#endregion

        #region Event Helper Methods

        public override bool HasScript(string handler) {
            switch (handler) {
                case "OnClick":
                case "OnDoubleClick":
                    return true;
                default:
                    return base.HasScript(handler);
            }
        }

        protected override object GetScriptDelegate(string handler, IronPython.Runtime.Calls.PythonFunction method) {
            switch (handler) {
                case "OnClick":
                case "OnDoubleClick":
                    return UiScripting.SetupDelegate<MouseEventHandler>(method.Name);
                default:
                    return base.GetScriptDelegate(handler, method);
            }
        }

        protected override void SetScriptHelper(string handler, object old_method, object new_method) {
            switch (handler) {
                case "OnClick":
                    if (old_method != null) {
                        MouseEventHandler eventHandler = old_method as MouseEventHandler;
                        this.ClickEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        MouseEventHandler eventHandler = new_method as MouseEventHandler;
                        this.ClickEvent += eventHandler;
                    }
                    break;
                case "OnDoubleClick":
                    if (old_method != null) {
                        MouseEventHandler eventHandler = old_method as MouseEventHandler;
                        this.DoubleClickEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        MouseEventHandler eventHandler = new_method as MouseEventHandler;
                        this.DoubleClickEvent += eventHandler;
                    }
                    break;
                default:
                    base.SetScriptHelper(handler, old_method, new_method);
                    break;
            }
        }

        public virtual void OnClick(MouseEventArgs e) {
            if (ClickEvent != null) 
                ClickEvent(this, e);
        }

        public virtual void OnDoubleClick(MouseEventArgs e) {
            if (DoubleClickEvent != null) 
                DoubleClickEvent(this, e);
        }

        internal bool HasClickEvent {
            get {
                return this.ClickEvent != null;
            }
        }
        #endregion

        public override void Prepare(Window parent) {
            base.Prepare(parent);
			if (this.IsVirtual)
				return;
			// Set up anchors for my textures that cause them to fill my size
            this.MouseDownEvent += button_OnMouseDown;
            this.MouseUpEvent += button_OnMouseUp;
            this.LeaveEvent += button_OnMouseLeave;
			this.Enable();
            // Set the text on the child widgets that haven't overridden 
            // the button text with their own.  This doesn't correctly handle 
            // the situation where they set a sub-text to the empty string.
            if (normalText != null && normalText.GetText() == string.Empty)
                normalText.SetText(text);
            if (highlightText != null && highlightText.GetText() == string.Empty)
                highlightText.SetText(text);
            if (disabledText != null && disabledText.GetText() == string.Empty)
                disabledText.SetText(text);
		}

		public void DebugDump() {
            log.InfoFormat("Position: {0}; Size: {1}", position, size);
            log.InfoFormat("normal texture - Position: {0}; Size: {1}", normalTexture.Position, normalTexture.Size);
            log.InfoFormat("normal texture window - Position: {0}; Size: {1}", normalTexture.window.Position, normalTexture.window.Size);
            log.InfoFormat("normal text - Position: {0}; Size: {1}", normalText.Position, normalText.Size);
            log.InfoFormat("normal text window - Position: {0}; Size: {1}", normalText.window.Position, normalText.window.Size);
            LayeredText textWindow = (LayeredText)(normalText.window);
            log.InfoFormat("normal text window text/alltext: {0}, {1}", textWindow.Text, textWindow.GetAllText());
            log.InfoFormat("normal text window color: {0}; {1}; {2}", textWindow.NormalTextStyle, normalText.GetTextColor(), normalText.GetColorRect());
            List<TextChunk> textChunks = textWindow.TextChunks;
			foreach (TextChunk chunk in textChunks)
                log.InfoFormat("normal text chunk: {0} ({1})", text.Substring(chunk.range.start, chunk.range.end - chunk.range.start), chunk.style);
		}

        public override bool IsMouseEnabled() {
            return base.IsMouseEnabled() || this.ClickEvent != null || this.DoubleClickEvent != null;
        }

        public virtual void UpdateButton() {
            // Update the image portion
            if (highlight && highlightTexture != null) {
                if (normalTexture != null)
                    normalTexture.Show();
                if (pushedTexture != null)
                    pushedTexture.Hide();
                if (disabledTexture != null)
                    disabledTexture.Hide();
                if (highlightTexture != null)
                    highlightTexture.Show();
            } else if (!enabled && disabledTexture != null) {
                if (normalTexture != null)
                    normalTexture.Hide();
                if (pushedTexture != null)
                    pushedTexture.Hide();
                if (disabledTexture != null)
                    disabledTexture.Show();
                if (highlightTexture != null)
                    highlightTexture.Hide();
            } else if (pushed && pushedTexture != null) {
                if (normalTexture != null)
                    normalTexture.Hide();
                if (pushedTexture != null)
                    pushedTexture.Show();
                if (disabledTexture != null)
                    disabledTexture.Hide();
                if (highlightTexture != null)
                    highlightTexture.Hide();
            } else {
                if (normalTexture != null)
                    normalTexture.Show();
                if (pushedTexture != null)
                    pushedTexture.Hide();
                if (disabledTexture != null)
                    disabledTexture.Hide();
                if (highlightTexture != null)
                    highlightTexture.Hide();
            }

            // Update the text portion
            if (highlight && highlightText != null) {
                if (normalText != null)
                    normalText.Hide();
                if (highlightText != null)
                    highlightText.Show();
                if (disabledText != null)
                    disabledText.Hide();
            } else if (!enabled && disabledText != null) {
                if (normalText != null)
                    normalText.Hide();
                if (highlightText != null)
                    highlightText.Hide();
                if (disabledText != null)
                    disabledText.Show();
            } else {
                if (normalText != null)
                    normalText.Show();
                if (highlightText != null)
                    highlightText.Hide();
                if (disabledText != null)
                    disabledText.Hide();
            }
        }

		#region IButton Methods
		public void Click() {
			if (this.HasClickEvent) {
                MouseEventArgs eventArgs = GuiSystem.Instance.CreateMouseEventArgs();
				OnClick(eventArgs);
			}
		}

		public virtual void Enable() {
			enabled = true;
            UpdateButton();
		}
		public virtual void Disable() {
			enabled = false;
            UpdateButton();
		}
        public string GetButtonState() {
            if (pushed)
                return "PUSHED";
            return "NORMAL";
        }
		public int GetTextHeight() {
			if (normalText != null)
				return normalText.GetFontHeight();
			return 0;
		}

		public int GetTextWidth() {
			if (normalText != null)
				return normalText.GetStringWidth();
			return 0;
		}
		public string GetText() {
			return text;
		}
		public bool IsEnabled() {
			return enabled;
		}
		public virtual void LockHighlight() {
            highlight = true;
            UpdateButton();
		}

		public void SetText(string str) {
			text = str;
			if (normalText != null)
				normalText.SetText(text);
			if (highlightText != null)
				highlightText.SetText(text);
			if (disabledText != null)
				disabledText.SetText(text);
		}
		public void SetTextColor(float r, float g, float b) {
			normalText.SetTextColor(r, g, b);
		}
        public void SetHighlightTextColor(float r, float g, float b) {
            highlightText.SetTextColor(r, g, b);
        }
        public void SetHighlightTextColor(float r, float g, float b, float a) {
            highlightText.SetTextColor(r, g, b, a);
        }
        public void SetDisabledTextColor(float r, float g, float b) {
            disabledText.SetTextColor(r, g, b);
        }
        public void SetDisabledTextColor(float r, float g, float b, float a) {
            disabledText.SetTextColor(r, g, b, a);
        }
        // TODO: I don't think this will work if they were not
		//       defined in xml, but are called from scripts.
        public void SetDisabledTexture(string texture) {
            disabledTexture.SetTexture(texture);
		}
        public void SetDisabledTexture(ITexture texture) {
            disabledTexture = (Texture)texture;
            UpdateButton();
        }
		public void SetHighlightTexture(string texture) {
			highlightTexture.SetTexture(texture);
		}
        public void SetHighlightTexture(ITexture texture) {
            highlightTexture = (Texture)texture;
            UpdateButton();
        }
		public void SetNormalTexture(string texture) {
			normalTexture.SetTexture(texture);
		}
        public void SetNormalTexture(ITexture texture) {
            normalTexture = (Texture)texture;
            UpdateButton();
        }
		public void SetPushedTexture(string texture) {
			pushedTexture.SetTexture(texture);
		}
        public void SetPushedTexture(ITexture texture) {
            pushedTexture = (Texture)texture;
            UpdateButton();
        }
        public void SetButtonState(string state) {
            SetButtonState(state, false);
        }
        public void SetButtonState(string state, int locked) {
            SetButtonState(state, locked != 0);
        }
        public void SetButtonState(string state, bool locked) {
            switch (state) {
                case "PUSHED":
                    pushed = true;
                    pushed_locked = locked;
                    break;
                case "NORMAL":
                    pushed = false;
                    pushed_locked = false;
                    break;
            }
            UpdateButton();
        }
		public void UnlockHighlight() {
            highlight = false;
            UpdateButton();
		}
		#endregion

        protected void button_OnMouseDown(object sender, MouseEventArgs e) {
            if (enabled && !pushed && e.Button == MouseButtons.Left) {
                pushed = true;
                UpdateButton();
            }
        }
        protected void button_OnMouseUp(object sender, MouseEventArgs e) {
            if (pushed && !pushed_locked && e.Button == MouseButtons.Left) {
                pushed = false;
                UpdateButton();
            }
        }
        protected void button_OnMouseLeave(object sender, MouseEventArgs e) {
            if (pushed && !pushed_locked) {
                pushed = false;
                UpdateButton();
            }
        }
    }

	public class CheckButton : Button, ICheckButton {
		// CheckedTexture, DisabledCheckedTexture
		// checked
        protected Texture checkedTexture;
        protected Texture disabledCheckedTexture;
        protected bool isChecked;

        internal override string GetInterfaceName() {
            return "ICheckButton";
        }

        public override InterfaceLayer Clone() {
			CheckButton rv = new CheckButton();
			CopyTo(rv);
			return rv;
		}
        
		public override bool CopyTo(object obj) {
			if (!(obj is CheckButton))
				return false;
			CheckButton dst = (CheckButton)obj;
			CheckButton.CopyCheckButton(dst, this);
			return true;
		}

		protected static void CopyCheckButtonElement(Frame dst, Frame src,
										             InterfaceLayer dst_element, 
										             InterfaceLayer src_element) {
            Button.CopyButtonElement(dst, src, dst_element, src_element);
			if (!(src is CheckButton) || !(dst is CheckButton))
				return;
			CheckButton src_button = src as CheckButton;
			CheckButton dst_button = dst as CheckButton;
			// Handle the textures
			if (src_element == src_button.checkedTexture)
				dst_button.checkedTexture = (Texture)dst_element;
            if (src_element == src_button.disabledCheckedTexture)
                dst_button.disabledCheckedTexture = (Texture)dst_element;
		}

        protected static void CopyCheckButton(CheckButton dst, CheckButton src) {
            Frame.CopyFrame(dst, src,
                            new FrameElementHandler(CheckButton.CopyCheckButtonElement));

            dst.text = src.text;
            dst.enabled = src.enabled;
            dst.pushedTextOffset = src.pushedTextOffset;
			dst.isChecked = src.isChecked;
		}

		#region Xml Parsing Methods

		protected override bool HandleAttribute(XmlAttribute attr) {
			switch (attr.Name) {
				case "checked":
					isChecked = bool.Parse(attr.Value);
                    return true;
				default:
					return base.HandleAttribute(attr);
			}
		}

		protected override bool HandleElement(XmlElement node) {
			switch (node.Name) {
				case "CheckedTexture":
					checkedTexture = new Texture();
                    UiSystem.ReadFrame(checkedTexture, node, this, null);
                    checkedTexture.LayerOffset = 1; // We need the check to be in front of the rest of the button
                    checkedTexture.AnchorToParentFull();
					return true;
				case "DisabledCheckedTexture":
                    disabledCheckedTexture = new Texture();
					UiSystem.ReadFrame(disabledCheckedTexture, node, this, null);
                    disabledCheckedTexture.LayerOffset = 1; // We need the check to be in front of the rest of the button
                    disabledCheckedTexture.AnchorToParentFull();
					return true;
				default:
					return base.HandleElement(node);
			}
		}

		#endregion


        #region ICheckButton Methods

        public bool GetChecked() {
            return isChecked;
        }

        public void SetChecked() {
            SetChecked(true);
        }

        public void SetChecked(bool state) {
            isChecked = state;
            UpdateButton();
        }

        #endregion

        public override void UpdateButton() {
            if (isChecked && enabled) {
                if (checkedTexture != null)
                    checkedTexture.Show();
                if (disabledCheckedTexture != null)
                    disabledCheckedTexture.Hide();
            } else if (isChecked) {
                if (checkedTexture != null)
                    checkedTexture.Hide();
                if (disabledCheckedTexture != null)
                    disabledCheckedTexture.Show();
            } else {
                if (checkedTexture != null)
                    checkedTexture.Hide();
                if (disabledCheckedTexture != null)
                    disabledCheckedTexture.Hide();
            }
            base.UpdateButton();
        }
	}

	//public class LootButton : Button {
	//}

	public class ColorSelect : Frame, IColorSelect {
		// ColorWheelTexture, ColorWheelThumbTexture, ColorValueTexture, ColorValueThumbTexture
		public override InterfaceLayer Clone() {
			ColorSelect rv = new ColorSelect();
			CopyTo(rv);
			return rv;
		}
	}

	public class EditBox : Frame, IEditBox {
		// FontString, HighlightColor, TextInsets
		// letters, blinkSpeed, numeric, password, multiLine, historyLines, autoFocus, ignoreArrows
        // OnEnterPressed, OnEscapePressed, OnSpacePressed, OnTabPressed 
        FontString fontString;
		// TODO: Handle the textInsets
		Rect textInsets = new Rect(0, 0, 0, 0);
		bool textMasked = false;
		ColorEx highlightColor = new ColorEx(1.0f, 0.5f, 0.5f, 0.5f);
		int maxHistoryLines = 0;
		List<string> historyLines = new List<string>();

        #region Events      

        public event EventHandler EnterPressedEvent;
        public event EventHandler EscapePressedEvent;
        public event EventHandler SpacePressedEvent;
        public event EventHandler TabPressedEvent;

        // OnEditFocusGained 
        // OnEditFocusLost
        // OnCursorChanged
        // OnInputLanguageChanged
        // OnTextChanged 
        // OnTextSet
        #endregion

        public override void Prepare(Window parent) {
            base.Prepare(parent);
			if (this.isVirtual)
				return;
			fontString.SetTextInsets(textInsets);
			LayeredEditBox editWindow = fontString.window as LayeredEditBox;
			editWindow.TextMasked = textMasked;
			editWindow.SelectedTextStyle = new TextStyle(editWindow.NormalTextStyle);
			editWindow.SelectedTextStyle.bgEnabled = true;
			editWindow.SelectedTextStyle.bgColor = highlightColor;
			editWindow.SetHistory(historyLines);
            
			// this.Click += this.HandleClick;
            editWindow.EnterPressed += this.HandleEnterPressed;
            editWindow.EscapePressed += this.HandleEscapePressed;
            editWindow.SpacePressed += this.HandleSpacePressed;
            editWindow.TabPressed += this.HandleTabPressed;
            editWindow.CharEvent += this.HandleChar;

            this.MouseDownEvent += this.HandleMouseDown;
            this.MouseUpEvent += this.HandleMouseUp;
        }
		

		#region Copy methods

		public override InterfaceLayer Clone() {
			EditBox rv = new EditBox();
			CopyTo(rv);
			return rv;
		}

		public override bool CopyTo(object obj) {
			if (!(obj is EditBox))
				return false;
			EditBox dst = (EditBox)obj;
			EditBox.CopyEditBox(dst, this);
			return true;
		}

		protected static void CopyEditBox(EditBox dst, EditBox src) {
			Frame.CopyFrame(dst, src,
							new FrameElementHandler(EditBox.CopyEditBoxElement));
			dst.textInsets = src.textInsets;
			dst.textMasked = src.textMasked;
			dst.highlightColor = src.highlightColor;
			dst.maxHistoryLines = src.maxHistoryLines;
			// nothing is in the history when we copy these.
			// dst.historyLines = new List<string>(src.historyLines);
		}

		protected static void CopyEditBoxElement(Frame dst, Frame src,
										         InterfaceLayer dst_element,
										         InterfaceLayer src_element) {
			if (!(src is EditBox) || !(dst is EditBox))
				return;
			EditBox src_frame = src as EditBox;
			EditBox dst_frame = dst as EditBox;
			// Handle the font string
			if (src_element == src_frame.fontString)
				dst_frame.fontString = (FontString)dst_element;
		}

		#endregion

		#region Xml Parsing Methods

		protected override bool HandleElement(XmlElement node) {
			switch (node.Name) {
				case "FontString":
					fontString = new FontString();
					UiSystem.ReadFrame(fontString, node, this, null);
					fontString.AnchorToParent(Anchor.Framepoint.Top);
					fontString.EditEnabled = true;
					fontString.SetJustifyH(HorizontalTextFormat.Left);
					fontString.SetJustifyV(VerticalTextFormat.Top);
					return true;
				case "TextInsets":
					textInsets = XmlUi.ReadInset(node);
					return true;
				case "HighlightColor":
					highlightColor = XmlUi.ReadColor(node);
					return true;
				default:
					return base.HandleElement(node);
			}
		}

		protected override bool HandleAttribute(XmlAttribute attr) {
			switch (attr.Name) {
				case "password":
					textMasked = bool.Parse(attr.Value);
					return true;
				case "historyLines":
					maxHistoryLines = int.Parse(attr.Value);
					return true;
				case "letters":
				case "blinkSpeed":
				case "numeric":
				case "multiLine":
				case "autoFocus":
				case "ignoreArrows":
					// TODO: Handle these
					return true;
				default:
					return base.HandleAttribute(attr);
			}
		}

		#endregion

        #region Event Helper Methods

        public override bool HasScript(string handler) {
            switch (handler) {
                case "OnEnterPressed":
                case "OnEscapePressed":
                case "OnSpacePressed":
                case "OnTabPressed":
                    return true;
                default:
                    return base.HasScript(handler);
            }
        }

        protected override object GetScriptDelegate(string handler, IronPython.Runtime.Calls.PythonFunction method) {
            switch (handler) {
                case "OnEnterPressed":
                case "OnEscapePressed":
                case "OnSpacePressed":
                case "OnTabPressed":
                    return UiScripting.SetupDelegate<EventHandler>(method.Name);
                default:
                    return base.GetScriptDelegate(handler, method);
            }
        }

        protected override void SetScriptHelper(string handler, object old_method, object new_method) {
            switch (handler) {
                case "OnEnterPressed":
                    if (old_method != null) {
                        EventHandler eventHandler = old_method as EventHandler;
                        this.EnterPressedEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        EventHandler eventHandler = new_method as EventHandler;
                        this.EnterPressedEvent += eventHandler;
                    }
                    break;
                case "OnEscapePressed":
                    if (old_method != null) {
                        EventHandler eventHandler = old_method as EventHandler;
                        this.EscapePressedEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        EventHandler eventHandler = new_method as EventHandler;
                        this.EscapePressedEvent += eventHandler;
                    }
                    break;
                case "OnSpacePressed":
                    if (old_method != null) {
                        EventHandler eventHandler = old_method as EventHandler;
                        this.SpacePressedEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        EventHandler eventHandler = new_method as EventHandler;
                        this.SpacePressedEvent += eventHandler;
                    }
                    break;
                case "OnTabPressed":
                    if (old_method != null) {
                        EventHandler eventHandler = old_method as EventHandler;
                        this.TabPressedEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        EventHandler eventHandler = new_method as EventHandler;
                        this.TabPressedEvent += eventHandler;
                    }
                    break;
                default:
                    base.SetScriptHelper(handler, old_method, new_method);
                    break;
            }
        }

        public virtual void OnEnterPressed(EventArgs e) {
            if (EnterPressedEvent != null)
                EnterPressedEvent(this, e);
        }
        public virtual void OnEscapePressed(EventArgs e) {
            if (EscapePressedEvent != null)
                EscapePressedEvent(this, e);
        }
        public virtual void OnSpacePressed(EventArgs e) {
            if (SpacePressedEvent != null)
                SpacePressedEvent(this, e);
        }
        public virtual void OnTabPressed(EventArgs e) {
            if (TabPressedEvent != null)
                TabPressedEvent(this, e);
        }

        #endregion

		#region Event handlers

		public void HandleClick(object sender, MouseEventArgs args) {
			PointF screenPoint = GuiSystem.Instance.MousePosition;
			LayeredEditBox editWindow = fontString.window as LayeredEditBox;
			PointF windowPoint = editWindow.ScreenToWindow(screenPoint);
		}

        public void HandleMouseDown(object sender, MouseEventArgs args) {
            // These events get dispatched to our frame, but I then want to 
            // send the data to the window object.
            GuiSystem.Instance.InjectMouseDown(args.Button, fontString.window);
            CaptureMouse(true);
        }
        public void HandleMouseUp(object sender, MouseEventArgs args) {
            // These events get dispatched to our frame, but I then want to 
            // send the data to the window object.
            GuiSystem.Instance.InjectMouseUp(args.Button, fontString.window);
            CaptureMouse(false);
        }


        protected void HandleEnterPressed(object sender, EventArgs e) {
            OnEnterPressed(e);
        }
        protected void HandleEscapePressed(object sender, EventArgs e) {
            OnEscapePressed(e); 
        }
        protected void HandleSpacePressed(object sender, EventArgs e) {
            OnSpacePressed(e);
        }
        protected void HandleTabPressed(object sender, EventArgs e) {
            OnTabPressed(e);
        }

        #endregion

    	//public LayeredEditBox GetWindow() {
		//    LayeredEditBox editWindow = fontString.window as LayeredEditBox;
		//    return editWindow;
		//}

		public void Select(int start, int end) {
			LayeredEditBox editWindow = fontString.window as LayeredEditBox;
			editWindow.SelectionStartIndex = start;
			editWindow.SelectionEndIndex = end;
		}

        //public string WidgetText {
        //    get {
        //        return fontString.textWindow.Text;
        //    }
        //    set {
        //        // FIXME - this will be all text, but I'm not sure if we want the real text or not
        //        fontString.textWindow.Text = value;
        //    }
        //}

		internal override string GetInterfaceName() {
			return "IEditBox";
		}

		#region IEditBox Methods

		public void AddHistoryLine(string text) {
			if (maxHistoryLines == 0)
				return;
			if (historyLines.Count == maxHistoryLines)
				historyLines.RemoveAt(0);
			historyLines.Add(text);
		}

		public string GetText() {
			return fontString.GetText();
		}

		public void SetText(string text) {
			fontString.SetText(text);
		}

		public void SetTextInsets(int l, int r, int t, int b) {
			fontString.SetTextInsets(new Rect(l, r, t, b));
		}

		public void SetTextColor(float r, float g, float b) {
			fontString.SetTextColor(r, g, b);
		}

        public void SetFocus() {
            LayeredEditBox editWindow = fontString.window as LayeredEditBox;
            editWindow.CaptureInput();
            parentUiSystem.FocusedFrame = this;
        }

        public void ClearFocus() {
            LayeredEditBox editWindow = fontString.window as LayeredEditBox;
            editWindow.ReleaseInput();
            if (parentUiSystem.FocusedFrame == this)
                parentUiSystem.FocusedFrame = null;
        }
		#endregion

		#region Properties

		public IFontString FontString {
			get { return fontString; }
		}

		public int CaretIndex {
			get {
				LayeredEditBox editWindow = fontString.window as LayeredEditBox;
				return editWindow.CaretIndex;
			}
			set {
				LayeredEditBox editWindow = fontString.window as LayeredEditBox;
				editWindow.CaretIndex = value;
			}
		}
		#endregion
	}

	public class GameTooltip : Frame, IGameTooltip {
		public override InterfaceLayer Clone() {
			GameTooltip rv = new GameTooltip();
			CopyTo(rv);
			return rv;
		}
		public void SetText(string text) {
			// TODO: Implement me
		}

		public void SetOwner(IRegion target, string anchor) {
			Debug.Assert(target is Frame);
			Frame frame = (Frame)target;
			switch (anchor) {
				case "ANCHOR_TOPRIGHT":
					this.SetPoint("BOTTOMRIGHT", frame.Name, "TOPRIGHT");
					break;
				case "ANCHOR_RIGHT":
					this.SetPoint("BOTTOMLEFT", frame.Name, "TOPRIGHT");
					break;
				case "ANCHOR_BOTTOMRIGHT":
					this.SetPoint("TOPLEFT", frame.Name, "BOTTOMRIGHT");
					break;
				case "ANCHOR_TOPLEFT":
					this.SetPoint("BOTTOMLEFT", frame.Name, "TOPLEFT");
					break;
				case "ANCHOR_LEFT":
					this.SetPoint("BOTTOMRIGHT", frame.Name, "TOPLEFT");
					break;
				case "ANCHOR_BOTTOMLEFT":
					this.SetPoint("TOPRIGHT", frame.Name, "BOTTOMLEFT");
					break;
				default:
					log.Warn("Invalid anchor argument to GameTooltip.SetOwner");
					break;
			}
		}
	}

	public class MessageFrame : Frame, IMessageFrame {
		// FontString, TextInsets
		// fadeDuration, insertMode
		public override InterfaceLayer Clone() {
			MessageFrame rv = new MessageFrame();
			CopyTo(rv);
			return rv;
		}

		protected override bool HandleAttribute(XmlAttribute attr) {
			switch (attr.Name) {
				case "fadeDuration":
				case "insertMode":
					// TODO: Handle these
					return true;
				default:
					return base.HandleAttribute(attr);
			}
		}

	}

	public class Minimap : Frame, IMinimap {
	}

	public class Model : Frame, IModel {
		// FogColor
		// file, scale, fogNear, fogFar
		public override InterfaceLayer Clone() {
			Model rv = new Model();
			CopyTo(rv);
			return rv;
		}
	}

	public class MovieFrame : Frame, IMovieFrame {
		public override InterfaceLayer Clone() {
			MovieFrame rv = new MovieFrame();
			CopyTo(rv);
			return rv;
		}
	}

	//public class PlayerModel : Model {
	//}

	//public class TabardModel : Model {
	//}

	public class ScrollingMessageFrame : Frame, IScrollingMessageFrame {
		// FontString, TextInsets
		// fade, fadeDuration, displayDuration, maxLines
		FontString fontString;
		// TODO: Handle the textInsets
		Rect textInsets = new Rect(0, 0, 0, 0);
		int maxLines = 100;
		bool scrollFromBottom = true;

		internal override string GetInterfaceName() {
			return "IScrollingMessageFrame";
		}

        public override void Prepare(Window parent) {
            base.Prepare(parent);
			if (this.IsVirtual)
				return;
			fontString.SetTextInsets(textInsets);
            LayeredText textWindow = fontString.window as LayeredText;
			textWindow.ScrollFromBottom = scrollFromBottom;
		}

		#region Copy methods

		public override InterfaceLayer Clone() {
			ScrollingMessageFrame rv = new ScrollingMessageFrame();
			CopyTo(rv);
			return rv;
		}

		public override bool CopyTo(object obj) {
			if (!(obj is ScrollingMessageFrame))
				return false;
			ScrollingMessageFrame dst = (ScrollingMessageFrame)obj;
			ScrollingMessageFrame.CopyScrollingMessageFrame(dst, this);
			return true;
		}

		protected static void CopyScrollingMessageFrame(ScrollingMessageFrame dst, ScrollingMessageFrame src) {
			Frame.CopyFrame(dst, src,
							new FrameElementHandler(ScrollingMessageFrame.CopyScrollingMessageFrameElement));
			dst.textInsets = src.textInsets;
			dst.maxLines = src.maxLines;
			dst.scrollFromBottom = src.scrollFromBottom;
		}

        protected static void CopyScrollingMessageFrameElement(Frame dst, Frame src,
										                       InterfaceLayer dst_element, 
										                       InterfaceLayer src_element) {
			if (!(src is ScrollingMessageFrame) || !(dst is ScrollingMessageFrame))
				return;
			ScrollingMessageFrame src_frame = src as ScrollingMessageFrame;
			ScrollingMessageFrame dst_frame = dst as ScrollingMessageFrame;
			// Handle the font string
			if (src_element == src_frame.fontString)
				dst_frame.fontString = (FontString)dst_element;
		}

		#endregion

		#region Xml Parsing Methods

		protected override bool HandleElement(XmlElement node) {
			switch (node.Name) {
				case "FontString":
					fontString = new FontString();
					UiSystem.ReadFrame(fontString, node, this, null);
					fontString.AnchorToParentFull();
					fontString.SetJustifyH(HorizontalTextFormat.WordWrapLeft);
					fontString.SetJustifyV(VerticalTextFormat.Top);
					return true;
				case "TextInsets":
					textInsets = XmlUi.ReadInset(node);
					return true;
				default:
					return base.HandleElement(node);
			}
		}

		protected override bool HandleAttribute(XmlAttribute attr) {
			switch (attr.Name) {
                case "font":
				case "fade":
				case "fadeDuration":
				case "displayDuration":
					// TODO: Handle these
					return true;
				case "maxLines":
					maxLines = Int32.Parse(attr.Value);
					return true;
				default:
					return base.HandleAttribute(attr);
			}
		}

		#endregion

		#region IScrollingMessageFrame Methods

		public void AddMessage(string text, float r, float g, float b, int id) {
			ColorEx curColor = fontString.GetTextColor();
			fontString.SetTextStyleColor(r, g, b);
			if (fontString.GetText().Length == 0)
				fontString.AddText(text);
			else
				fontString.AddText("\n" + text);
			fontString.SetTextStyleColor(curColor.r, curColor.g, curColor.b);
		}

		public int GetFontHeight() {
			return fontString.GetFontHeight();
		}

		public void SetFontHeight(int fontHeight) {
			fontString.SetTextHeight(fontHeight);
		}

		public int GetNumLinesDisplayed() {
			return GetHeight() / GetFontHeight();
		}

		public void SetScrollFromBottom(bool val) {
			scrollFromBottom = val;
            LayeredText textWindow = fontString.window as LayeredText;
			textWindow.ScrollFromBottom = val;
		}

		public void ScrollUp() {
            LayeredText textWindow = fontString.window as LayeredText;
			textWindow.ScrollUp();
		}

		public void ScrollDown() {
            LayeredText textWindow = fontString.window as LayeredText;
			textWindow.ScrollDown();
		}

		public void ScrollToTop() {
            LayeredText textWindow = fontString.window as LayeredText;
			textWindow.ScrollToTop();
		}

		public void ScrollToBottom() {
            LayeredText textWindow = fontString.window as LayeredText;
			textWindow.ScrollToBottom();
		}


		#endregion

		#region Properties

		public IFontString FontString {
			get { return fontString; }
		}

		#endregion

	}

	public class ScrollFrame : Frame, IScrollFrame {
        // OnHorizontalScroll, OnScrollRangeChanged, OnVerticalScroll
        float horizontalOffset = 0.0f;
        float horizontalRange = 0.0f;
        float verticalOffset = 0.0f;
        float verticalRange = 0.0f;

        List<Frame> scrollFrames = new List<Frame>();

        #region Events

        public event ExtendedEventHandler ScrollRangeChangedEvent;
        public event FloatEventHandler HorizontalScrollEvent;
        public event FloatEventHandler VerticalScrollEvent;

        #endregion

        public override void Prepare(Window parent) {
            base.Prepare(parent);
            if (this.IsVirtual)
                return;
            UpdateScrollChildRect();
        }

        #region Copy Methods

        public override bool CopyTo(object obj) {
            if (!(obj is ScrollFrame))
                return false;
            ScrollFrame dst = (ScrollFrame)obj;
            ScrollFrame.CopyScrollFrame(dst, this);
            return true;
        }

        protected static void CopyScrollFrameElement(Frame dst, Frame src,
                                                     InterfaceLayer dst_element,
                                                     InterfaceLayer src_element) {
            if (!(src is ScrollFrame) || !(dst is ScrollFrame))
                return;
            ScrollFrame src_frame = src as ScrollFrame;
            ScrollFrame dst_frame = dst as ScrollFrame;
            // If an element is one of my scroll frames, the dst version
            // should also be one of the dst object's scroll frames.
            if (src_frame.scrollFrames.Contains((Frame)src_element))
                dst_frame.scrollFrames.Add((Frame)dst_element);
        }

        protected static void CopyScrollFrame(ScrollFrame dst, ScrollFrame src) {
            Frame.CopyFrame(dst, src,
                            new FrameElementHandler(ScrollFrame.CopyScrollFrameElement));
            dst.horizontalOffset = src.horizontalOffset;
            dst.horizontalRange = src.horizontalRange;
            dst.verticalOffset = src.verticalOffset;
            dst.verticalRange = src.verticalRange;
        }

        public override InterfaceLayer Clone() {
            ScrollFrame rv = new ScrollFrame();
            CopyTo(rv);
            return rv;
        }

        #endregion

        #region Xml Parsing Methods

        protected override bool HandleElement(XmlElement node) {
            switch (node.Name) {
                case "ScrollChild":
                    Frame scrollChild = new Frame();
                    UiSystem.ReadFrame(scrollChild, null, this, null);
                    scrollFrames.Add(scrollChild);
                    scrollChild.SetClipper(this);
                    foreach (XmlNode childNode in node.ChildNodes) {
                        if (!(childNode is XmlElement))
                            continue;
                        Region region = UiSystem.ReadFrame(childNode, scrollChild, null);
                        region.IsScrollChild = true;
                    }
                    return true;
                default:
                    return base.HandleElement(node);
            }
        }

        #endregion


        #region IScrollFrame Methods

        public float GetHorizontalScroll() {
            return horizontalOffset;
        }
        public void SetHorizontalScroll(float offset) {
            if (horizontalOffset == offset)
                return;
            horizontalOffset = offset;
            UpdateScrollChildRect();
            FloatEventArgs args = new FloatEventArgs();
            args.data = offset;
            OnHorizontalScroll(args);
        }
        public float GetHorizontalScrollRange() {
            return horizontalRange;
        }
        public IFrame GetScrollChild() {
            if (scrollFrames.Count > 0)
                return scrollFrames[0];
            return null;
        }
        public void SetScrollChild(string childName) {
            IFrame child = (IFrame)UiSystem.FrameMap[childName];
            SetScrollChild(child);
        }
        public void SetScrollChild(IFrame child) {
            scrollFrames.Clear();
            scrollFrames.Add((Frame)child);
            UpdateScrollChildRect();
        }
        public float GetVerticalScroll() {
            return verticalOffset;
        }
        public void SetVerticalScroll(float offset) {
            if (verticalOffset == offset)
                return;
            verticalOffset = offset;
            UpdateScrollChildRect();
            FloatEventArgs args = new FloatEventArgs();
            args.data = offset;
            OnVerticalScroll(args);
        }
        public float GetVerticalScrollRange() {
            return verticalRange;
        }
        public void UpdateScrollChildRect() {
            Rect rect = null;
            foreach (Frame scrollChild in scrollFrames) {
                foreach (InterfaceLayer layer in scrollChild.Elements) {
                    Region region = layer as Region;
                    if (region == null)
                        continue;
                    if (rect == null) {
                        rect = new Rect();
                        rect.Top = region.PositionForAnchor.Y;
                        rect.Bottom = region.PositionForAnchor.Y + region.GetHeight();
                        rect.Left = region.PositionForAnchor.X;
                        rect.Right = region.PositionForAnchor.X + region.GetWidth();
                    }
                    ExpandToContain(ref rect, region);
                }
            }
            if (rect == null)
                rect = new Rect(0, 0, 0, 0);
            float oldHorizontalRange = horizontalRange;
            float oldVerticalRange = verticalRange;
            if (this.Size.Height > rect.Size.Height)
                verticalRange = 0;
            else
                verticalRange = rect.Size.Height - this.Size.Height;
            if (verticalOffset > verticalRange)
                verticalOffset = verticalRange;
            else if (verticalOffset < 0)
                verticalOffset = 0;
            if (this.Size.Width > rect.Size.Width)
                horizontalRange = 0;
            else
                horizontalRange = rect.Size.Width - this.Size.Width;
            if (horizontalOffset > horizontalRange) 
                horizontalOffset = horizontalRange;
            else if (horizontalOffset < 0)
                horizontalOffset = 0;

            // Resize the scrollChild frame(s) to fit their contents
            foreach (Frame scrollChild in scrollFrames) {
                scrollChild.ScrollOffset = new PointF(horizontalOffset, verticalOffset);
                foreach (InterfaceLayer element in scrollChild.Elements) {
                    if (element is Region)
                        element.SetupWindowPosition();
                }
            }
            if (oldHorizontalRange != horizontalRange ||
                oldVerticalRange != verticalRange) {
                ExtendedEventArgs e = new ExtendedEventArgs();
                e.data = new object[2];
                e.data[0] = horizontalRange;
                e.data[1] = verticalRange;
                OnScrollRangeChanged(e);
            }
        }

        #endregion
        
        protected void ExpandToContain(ref Rect rect, Region region) {
            if (region.Position.Y < rect.Top)
                rect.Top = region.PositionForAnchor.Y;
            if (region.Position.Y + region.GetHeight() > rect.Bottom)
                rect.Bottom = region.PositionForAnchor.Y + region.GetHeight();
            if (region.Position.X < rect.Left)
                rect.Left = region.PositionForAnchor.X;
            if (region.Position.X + region.GetWidth() > rect.Right)
                rect.Right = region.PositionForAnchor.X + region.GetWidth();
            Frame frame = region as Frame;
            if (frame == null)
                return;
            foreach (InterfaceLayer layer in frame.Elements) {
                Region subRegion = layer as Region;
                if (subRegion == null)
                    continue;
                ExpandToContain(ref rect, subRegion);
            }
        }

        #region Event Helper Methods

        public override bool HasScript(string handler) {
            switch (handler) {
                case "OnScrollRangeChanged":
                case "OnHorizontalScroll":
                case "OnVerticalScroll":
                    return true;
                default:
                    return base.HasScript(handler);
            }
        }

        protected override object GetScriptDelegate(string handler, IronPython.Runtime.Calls.PythonFunction method) {
            switch (handler) {
                case "OnScrollRangeChanged":
                    return UiScripting.SetupDelegate<ExtendedEventHandler>(method.Name);
                case "OnHorizontalScroll":
                case "OnVerticalScroll":
                    return UiScripting.SetupDelegate<FloatEventHandler>(method.Name);
                default:
                    return base.GetScriptDelegate(handler, method);
            }
        }

        protected override void SetScriptHelper(string handler, object old_method, object new_method) {
            switch (handler) {
                case "OnScrollRangeChanged":
                    if (old_method != null) {
                        ExtendedEventHandler eventHandler = old_method as ExtendedEventHandler;
                        this.ScrollRangeChangedEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        ExtendedEventHandler eventHandler = new_method as ExtendedEventHandler;
                        this.ScrollRangeChangedEvent += eventHandler;
                    }
                    break;
                case "OnHorizontalScroll":
                    if (old_method != null) {
                        FloatEventHandler eventHandler = old_method as FloatEventHandler;
                        this.HorizontalScrollEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        FloatEventHandler eventHandler = new_method as FloatEventHandler;
                        this.HorizontalScrollEvent += eventHandler;
                    }
                    break;
                case "OnVerticalScroll":
                    if (old_method != null) {
                        FloatEventHandler eventHandler = old_method as FloatEventHandler;
                        this.VerticalScrollEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        FloatEventHandler eventHandler = new_method as FloatEventHandler;
                        this.VerticalScrollEvent += eventHandler;
                    }
                    break;
                default:
                    base.SetScriptHelper(handler, old_method, new_method);
                    break;
            }
        }

        public virtual void OnHorizontalScroll(FloatEventArgs e) {
            if (HorizontalScrollEvent != null)
                HorizontalScrollEvent(this, e);
        }
        public virtual void OnVerticalScroll(FloatEventArgs e) {
            if (VerticalScrollEvent != null)
                VerticalScrollEvent(this, e);
        }
        public virtual void OnScrollRangeChanged(ExtendedEventArgs e) {
            if (ScrollRangeChangedEvent != null)
                ScrollRangeChangedEvent(this, e);
        }
        #endregion
    }

	public class SimpleHTML : Frame, ISimpleHTML {
		// FontString, FontStringHeader1, FontStringHeader2, FontStringHeader3
		// files
		public override InterfaceLayer Clone() {
			SimpleHTML rv = new SimpleHTML();
			CopyTo(rv);
			return rv;
		}
	}

	public class Slider : Frame, ISlider {
        // ThumbTexture
        // drawLayer, minValue, maxValue, defaultValue, valueStep, orientation
        // OnValueChanged
        
        float minValue = 0.0f;
        float maxValue = 1.0f;
        float curValue = 0.0f;
        float valueStep = 0.0f;
        Texture thumbTexture;
        Orientation orientation = Orientation.Vertical;
		
        #region Events

        public event FloatEventHandler ValueChangedEvent;

        #endregion
        
        public Slider() {
            layerLevel = LayerLevel.Overlay;
        }

        internal override string GetInterfaceName() {
            return "ISlider";
        }

        public override void Prepare(Window topWindow) {
            base.Prepare(topWindow);
            if (this.IsVirtual)
                return;
            if (thumbTexture != null)
                thumbTexture.Show();
            UpdateSlider();
            this.MouseDownEvent += HandleMouseDown;
            this.MouseUpEvent += HandleMouseUp;
            this.MouseMoveEvent += HandleMouseMove;
        }

        /// <summary>
        ///   Handle the mouse down event.  This sets the slider thumb position
        ///   so that the mouse position matches the center of the thumb.
        ///   This means that offset = relativeMousePosition - .5 * thumbSize
        ///   This also captures the mouse so that we will get mousemove events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleMouseDown(object sender, MouseEventArgs e) {
            CaptureMouse(true);
            int offset = 0;
            float thumbSize = 0;
            switch (orientation) {
                case Orientation.Vertical:
                    if (thumbTexture != null)
                        thumbSize = thumbTexture.Size.Height;
                    offset = (int)(e.Y - window.DerivedPosition.Y - .5 * thumbSize);
                    break;
                case Orientation.Horizontal:
                    if (thumbTexture != null)
                        thumbSize = thumbTexture.Size.Width;
                    offset = (int)(e.X - window.DerivedPosition.X - .5 * thumbSize);
                    break;
            }
            SetValue(OffsetToValue(offset));
        }

        /// <summary>
        ///   Release the capture of the mouse (so we won't get mousemove events)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleMouseUp(object sender, MouseEventArgs e) {
            CaptureMouse(false);
        }

        /// <summary>
        ///   Handle the mouse move event.  This sets the slider thumb position
        ///   so that the mouse position matches the center of the thumb.
        ///   This means that offset = relativeMousePosition - .5 * thumbSize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleMouseMove(object sender, MouseEventArgs e) {
            // I'm only interested in mouse movement if the left mouse button down
            int offset = 0;
            float thumbSize = 0;
            switch (orientation) {
                case Orientation.Vertical:
                    if (thumbTexture != null)
                        thumbSize = thumbTexture.Size.Height;
                    offset = (int)(e.Y - window.DerivedPosition.Y - .5 * thumbSize);
                    break;
                case Orientation.Horizontal:
                    if (thumbTexture != null)
                        thumbSize = thumbTexture.Size.Width;
                    offset = (int)(e.X - window.DerivedPosition.X - .5 * thumbSize);
                    break;
            }
            SetValue(OffsetToValue(offset));
        }


        #region Copy Methods

        public override bool CopyTo(object obj) {
            if (!(obj is Slider))
                return false;
            Slider dst = (Slider)obj;
            Slider.CopySlider(dst, this);
            return true;
        }

        protected static void CopySliderElement(Frame dst, Frame src,
                                          InterfaceLayer dst_element,
                                          InterfaceLayer src_element) {
            if (!(src is Slider) || !(dst is Slider))
                return;
            Slider src_slider = src as Slider;
            Slider dst_slider = dst as Slider;
            // Handle the textures
            if (src_element == src_slider.thumbTexture)
                dst_slider.thumbTexture = (Texture)dst_element;
        }

        protected static void CopySlider(Slider dst, Slider src) {
            Frame.CopyFrame(dst, src,
                            new FrameElementHandler(Slider.CopySliderElement));
            dst.minValue = src.minValue;
            dst.maxValue = src.maxValue;
            dst.curValue = src.curValue;
            dst.valueStep = src.valueStep;
            dst.orientation = src.orientation;
        }

        public override InterfaceLayer Clone() {
			Slider rv = new Slider();
			CopyTo(rv);
			return rv;
        }
        
        #endregion

        #region Xml Parsing Methods

        protected override bool HandleAttribute(XmlAttribute attr) {
            switch (attr.Name) {
                case "minValue":
                    minValue = float.Parse(attr.Value);
                    return true;
                case "maxValue":
                    maxValue = float.Parse(attr.Value);
                    return true;
                case "defaultValue":
                    curValue = float.Parse(attr.Value);
                    return true;
                case "orientation":
                    orientation = Region.GetOrientation(attr.Value);
                    return true;
                case "drawLayer":
                    layerLevel = InterfaceLayer.GetLayerLevel(attr.Value);
                    return true;
                case "valueStep":
                    this.valueStep = float.Parse(attr.Value);
                    return true;
                default:
                    return base.HandleAttribute(attr);
            }
        }

        protected override bool HandleElement(XmlElement node) {
            switch (node.Name) {
                case "ThumbTexture":
                    thumbTexture = new Texture();
                    UiSystem.ReadFrame(thumbTexture, node, this, null);
                    // TODO: I don't really know what is appropriate for thumbTextures
                    thumbTexture.FrameStrata = this.FrameStrata;
                    thumbTexture.LayerLevel = LayerLevel.Overlay;
                    thumbTexture.AnchorToParent(Anchor.Framepoint.TopLeft);
                    return true;
                default:
                    return base.HandleElement(node);
            }
        }

        #endregion

        protected float GetOffsetRange() {
            float range = 0;
            switch (orientation) {
                case Orientation.Vertical:
                    range = this.Size.Height;
                    if (thumbTexture != null)
                        range -= thumbTexture.Size.Height;
                    break;
                case Orientation.Horizontal:
                    range = this.Size.Width;
                    if (thumbTexture != null)
                        range -= thumbTexture.Size.Width;
                    break;
            }
            if (range < 0)
                range = 0;
            return range;
        }
        protected float OffsetToValue(int offset) {
            float range = GetOffsetRange();
            if (range == 0)
                return minValue;
            float thumbSpan = offset / range;
            return minValue + thumbSpan * (maxValue - minValue);
        }
        protected int ValueToOffset(float val) {
            float thumbSpan;
            if (val >= maxValue || maxValue <= minValue)
                thumbSpan = 1.0f;
            else if (val <= minValue)
                thumbSpan = 0.0f;
            else
                thumbSpan = (val - minValue) / (maxValue - minValue);
            float range = GetOffsetRange();
            return (int)(thumbSpan * range);
        }

        protected void UpdateSlider() {
            // portion of the bar that is filled in
            if (thumbTexture != null) {
                int offset = ValueToOffset(curValue);
                switch (orientation) {
                    case Orientation.Vertical:
                        thumbTexture.SetPoint("TOPLEFT", "UIParent", "TOPLEFT", 0, -offset);
                        break;
                    case Orientation.Horizontal:
                        thumbTexture.SetPoint("TOPLEFT", "UIParent", "TOPLEFT", offset, 0);
                        break;
                }
                thumbTexture.Show();
            }
        }

        #region Event Helper Methods
        
        public override bool HasScript(string handler) {
            switch (handler) {
                case "OnValueChanged":
                    return true;
                default:
                    return base.HasScript(handler);
            }
        }

        protected override object GetScriptDelegate(string handler, IronPython.Runtime.Calls.PythonFunction method) {
            switch (handler) {
                case "OnValueChanged":
                    return UiScripting.SetupDelegate<FloatEventHandler>(method.Name);
                default:
                    return base.GetScriptDelegate(handler, method);
            }
        }

        protected override void SetScriptHelper(string handler, object old_method, object new_method) {
            switch (handler) {
                case "OnValueChanged":
                    if (old_method != null) {
                        FloatEventHandler eventHandler = old_method as FloatEventHandler;
                        this.ValueChangedEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        FloatEventHandler eventHandler = new_method as FloatEventHandler;
                        this.ValueChangedEvent += eventHandler;
                    }
                    break;
                default:
                    base.SetScriptHelper(handler, old_method, new_method);
                    break;
            }
        }

        public virtual void OnValueChanged(FloatEventArgs e) {
            if (ValueChangedEvent != null)
                ValueChangedEvent(this, e);
        }

        #endregion

        #region ISlider Methods

        public float GetValue() {
            return curValue;
        }
        public void SetValue(float val) {
            if (curValue == val)
                return;
            if (val < minValue)
                val = minValue;
            else if (val > maxValue)
                val = maxValue;
            curValue = val;
            UpdateSlider();
            FloatEventArgs args = new FloatEventArgs();
            args.data = val;
            OnValueChanged(args);
        }
        public void SetMinMaxValues(float min, float max) {
            minValue = min;
            maxValue = max;
            UpdateSlider();
        }
        public float[] GetMinMaxValues() {
            float[] rv = new float[2];
            rv[0] = minValue;
            rv[1] = maxValue;
            return rv;
        }
        public string GetOrientation() {
            return Region.GetOrientation(orientation);
        }
        public void SetOrientation(string orient) {
            orientation = Region.GetOrientation(orient);
            UpdateSlider();
        }
        public float GetValueStep() {
            return valueStep;
        }
        public void SetValueStep(float step) {
            valueStep = step;
        }
        #endregion

    }

	public class StatusBar : Frame, IStatusBar {
		// BarTexture, BarColor
		// drawLayer, minValue, maxValue, defaultValue
        // OnValueChanged
		#region Events

		public event FloatEventHandler ValueChangedEvent;

		#endregion

		Texture barTexture;
		float minValue = 0.0f;
		float maxValue = 1.0f;
		float curValue = 0.0f;
        Orientation orientation = Orientation.Horizontal;
		ColorEx barColor = new ColorEx(1.0f, 1.0f, 1.0f, 1.0f);

		internal override string GetInterfaceName() {
            return "IStatusBar";
		}

        public StatusBar() {
            layerLevel = LayerLevel.Artwork;
        }

		public override void Prepare(Window topWindow) {
			base.Prepare(topWindow);
			if (this.IsVirtual)
				return;
			if (barTexture != null)
				barTexture.Show();
			UpdateStatusBar();
		}


		#region Copy Methods

		public override bool CopyTo(object obj) {
			if (!(obj is StatusBar))
				return false;
			StatusBar dst = (StatusBar)obj;
			StatusBar.CopyStatusBar(dst, this);
			return true;
		}

		protected static void CopyStatusBarElement(Frame dst, Frame src,
										           InterfaceLayer dst_element, 
										           InterfaceLayer src_element) {
			if (!(src is StatusBar) || !(dst is StatusBar))
				return;
			StatusBar src_bar = src as StatusBar;
			StatusBar dst_bar = dst as StatusBar;
			// Handle the textures
			if (src_element == src_bar.barTexture)
				dst_bar.barTexture = (Texture)dst_element;
		}

		protected static void CopyStatusBar(StatusBar dst, StatusBar src) {
			Frame.CopyFrame(dst, src,
                            new FrameElementHandler(StatusBar.CopyStatusBarElement));
			dst.barColor = src.barColor;
			dst.minValue = src.minValue;
			dst.maxValue = src.maxValue;
			dst.curValue = src.curValue;
            dst.orientation = src.orientation;
		}

		public override InterfaceLayer Clone() {
			StatusBar rv = new StatusBar();
			CopyTo(rv);
			return rv;
		}

		#endregion

		#region Xml Parsing Methods

		protected override bool HandleAttribute(XmlAttribute attr) {
			switch (attr.Name) {
				case "minValue":
					minValue = float.Parse(attr.Value);
					return true;
				case "maxValue":
					maxValue = float.Parse(attr.Value);
					return true;
				case "defaultValue":
					curValue = float.Parse(attr.Value);
					return true;
				case "drawLayer":
                    layerLevel = InterfaceLayer.GetLayerLevel(attr.Value);
                    return true;
                case "orientation":
                    orientation = Region.GetOrientation(attr.Value);
                    return true;
				default:
					return base.HandleAttribute(attr);
			}
		}

		protected override bool HandleElement(XmlElement node) {
			switch (node.Name) {
				case "BarTexture":
					barTexture = new Texture();
					UiSystem.ReadFrame(barTexture, node, this, null);
					// status bars are even behind the background
                    barTexture.FrameStrata = this.FrameStrata;
                    // FIXME: This won't really work.  The way we use frame levels, 
                    // child objects are automatically nested.  When I inherit a 
                    // template that uses this, I will inherit the frame level,
                    // which is wrong
                    barTexture.FrameLevel = this.FrameLevel;
                    barTexture.LayerLevel = LayerLevel.Zero;
                    // Anchor the bottom left.  This means we will expand to the right
                    // or up, depending on our orientation.
                    barTexture.SetPrimaryAnchor("BOTTOMLEFT", null, "BOTTOMLEFT", 0, 0);
					return true;
				case "BarColor":
					barColor = XmlUi.ReadColor(node);
					return true;
				default:
					return base.HandleElement(node);
			}
		}

		#endregion

		protected void UpdateStatusBar() {
			// portion of the bar that is filled in
			float barSpan;
			if (curValue >= maxValue || maxValue <= minValue)
				barSpan = 1.0f;
			else if (curValue <= minValue)
				barSpan = 0.0f;
			else 
				barSpan = (curValue - minValue) / (maxValue - minValue);
            switch (orientation) {
                case Orientation.Horizontal:
                    barTexture.SetHeight((int)this.Size.Height);
                    barTexture.SetWidth((int)(barSpan * this.Size.Width), true);
                    break;
                case Orientation.Vertical:
                    barTexture.SetHeight((int)(barSpan * this.Size.Height), true);
                    barTexture.SetWidth((int)this.Size.Width);
                    break;
            }
			barTexture.SetVertexColor(barColor.r, barColor.g, barColor.b, barColor.a);
			barTexture.Show();
        }

        #region Event Helper Methods

        public override bool HasScript(string handler) {
            switch (handler) {
                case "OnValueChanged":
                    return true;
                default:
                    return base.HasScript(handler);
            }
        }

        protected override object GetScriptDelegate(string handler, IronPython.Runtime.Calls.PythonFunction method) {
            switch (handler) {
                case "OnValueChanged":
                    return UiScripting.SetupDelegate<FloatEventHandler>(method.Name);
                default:
                    return base.GetScriptDelegate(handler, method);
            }
        }

        protected override void SetScriptHelper(string handler, object old_method, object new_method) {
            switch (handler) {
                case "OnValueChanged":
                    if (old_method != null) {
                        FloatEventHandler eventHandler = old_method as FloatEventHandler;
                        this.ValueChangedEvent -= eventHandler;
                    }
                    if (new_method != null) {
                        FloatEventHandler eventHandler = new_method as FloatEventHandler;
                        this.ValueChangedEvent += eventHandler;
                    }
                    break;
                default:
                    base.SetScriptHelper(handler, old_method, new_method);
                    break;
            }
        }

        public virtual void OnValueChanged(FloatEventArgs e) {
            if (ValueChangedEvent != null)
                ValueChangedEvent(this, e);
        }

        #endregion

		#region IStatusBar Methods

		public float GetValue() {
			return curValue;
		}
		public void SetValue(float val) {
            if (curValue == val)
                return;
			curValue = val;
			UpdateStatusBar();
            FloatEventArgs args = new FloatEventArgs();
            args.data = val;
            OnValueChanged(args);
		}
		public void SetMinMaxValues(float min, float max) {
			minValue = min;
			maxValue = max;
			UpdateStatusBar();
		}

		public void SetStatusBarColor(float r, float g, float b, float a) {
			barColor = new ColorEx(a, r, g, b);
			barTexture.SetVertexColor(barColor.r, barColor.g, barColor.b, barColor.a);
		}

		public void SetStatusBarColor(float r, float g, float b) {
			SetStatusBarColor(r, g, b, 1.0f);
		}
        
        public string GetOrientation() {
            return Region.GetOrientation(orientation);
        }
        public void SetOrientation(string orient) {
            orientation = Region.GetOrientation(orient);
            UpdateStatusBar();
        }
        public float[] GetMinMaxValues() {
            float[] rv = new float[2];
            rv[0] = minValue;
            rv[1] = maxValue;
            return rv;
        }

		#endregion
	}

    /// <summary>
    ///   This attribute isn't really needed, but I have it here just for annotation.
    ///   This object needs to be visible to COM so that IE can access it.
    /// </summary>
    [ComVisibleAttribute(true)]
    public class ScriptingDictionary
    {
        private Dictionary<string, object> dict = new Dictionary<string, object>();
        private Dictionary<string, List<EventHandler>> handlerDict = new Dictionary<string, List<EventHandler>>();
        public class ScriptingEventArgs : EventArgs
        {
            public string eventName;
            public object eventData;
        }

        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }
        public object this[string key]
        {
            get { return dict[key]; }
            set { dict[key] = value; }
        }
        public object GetValue(string arg)
        {
            return dict[arg];
        }
        public void SetValue(string arg, object obj)
        {
            dict[arg] = obj;
        }
        /// <summary>
        ///   This method generates an event that is then dispatched to the handlers.
        ///   If the obj argument is of type EventArgs, we will directly dispatch the event.
        ///   If the obj is not of type EventArgs, we will create a ScriptingEventArgs object
        ///   that contains both the eventName and the passed object.
        ///   While this design is somewhat awkward, it allows us to use it in a simple way,
        ///   where we just pass an object, or a more complex way, where we construct a real
        ///   EventArgs object.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="obj"></param>
        public void OnEvent(string eventName, object obj)
        {
            if (obj is EventArgs)
            {
                EventHandler(eventName, obj as EventArgs);
            }
            else
            {
                ScriptingEventArgs args = new ScriptingEventArgs();
                args.eventName = eventName;
                args.eventData = obj;
                EventHandler(eventName, args);
            }
        }
        protected void EventHandler(string eventName, EventArgs eventArgs)
        {
            List<EventHandler> handlers;
            bool found = handlerDict.TryGetValue(eventName, out handlers);
            if (!found)
                return;
            foreach (EventHandler handler in handlers)
            {
                handler(this, eventArgs);
            }
        }
        public void RegisterEventHandler(string eventName, EventHandler handler)
        {
            List<EventHandler> handlers;
            bool found = handlerDict.TryGetValue(eventName, out handlers);
            if (!found)
            {
                handlers = new List<EventHandler>();
                handlerDict[eventName] = handlers;
            }
            handlers.Add(handler);
        }
        public void RemoveEventHandler(string eventName, EventHandler handler)
        {
            List<EventHandler> handlers;
            bool found = handlerDict.TryGetValue(eventName, out handlers);
            if (!found)
                return;
            handlers.Remove(handler);
        }
    }

    public class Browser : Frame, IWebBrowser
    {
        // XXXMLM - GuiInputHandler.ClickToleranceTime and Space are protected
        public const int ClickToleranceTime = 3000;
        public const int ClickToleranceSpace = 10;

        private Multiverse.Web.Browser browser = null;
        private string url = null;
        private bool scrollbars = false;
        private int line = 0;
        private bool browsererrors = false;
        private string captureframe = null;

        // The default scripting object.  It's a little tricky to construct a valid
        // object that we can use, so I have a dictionary object that can be used.
        private ScriptingDictionary defaultScriptingObj = new ScriptingDictionary();
        private object scriptingObj;
        // private MouseEventArgs mouseEvent = null;

        internal override string GetInterfaceName()
        {
            return "IWebBrowser";
        }
		public override void Prepare(Window parent) {
			base.Prepare(parent);
			if (this.isVirtual) {
				return;
			}
            scriptingObj = defaultScriptingObj;
		}
		public void HandleKeyDown(object sender, KeyEventArgs args) {
			// do nothing -- suppresses key handling
			args.Handled = true;
		}
		public void HandleMouseDown(object sender, MouseEventArgs args) {
			if ((args.X < position.X) || (args.X > (position.X + size.Width )) || (args.Y < position.Y) || (args.Y > (position.Y + size.Height))) {
				// clicked outside the frame
				ClearFocus();
			} else {
				SetFocus();
			}
		}
		public void SetFocus() {
			// capture mouse so we can release control when user clicks outside our frame
			CaptureMouse(true);

			// tell brower to handle keyboard events
			browser.GetFocus();

			// capture keyboard input so we can disable key handling outside this frame
			GuiSystem.Instance.CaptureInput(this.window);
		}
		public void ClearFocus() {
			// see comment in SetFocus()
			CaptureMouse(false);

			// tell browser to ignore keyboard events
			browser.Losefocus();

			// see comment in SetFocus()
			GuiSystem.Instance.ReleaseInput(this.window);
		}
        internal override void ShowWindows()
        {
            base.ShowWindows();
            if (browser != null)
            {
                browser.Hide();
                browser.Dispose();
                browser = null;
            }
            browser = new Multiverse.Web.Browser();
            browser.BrowserSize = System.Drawing.Size.Round(this.Size);
            browser.BrowserLocation = Point.Round(this.Position);
            browser.BrowserScrollbars = scrollbars;
            browser.BrowserLine = line;
            browser.BrowserErrors = browsererrors;
            browser.Open(url);
            browser.ObjectForScripting = scriptingObj;
            browser.Show();

			this.MouseDownEvent += HandleMouseDown;
			this.window.KeyDown += this.HandleKeyDown; // -- this doesn't work --> // this.KeyDownEvent += this.HandleKeyDown;

			SetFocus();
        }

        internal override void HideWindows()
        {
			ClearFocus();
			this.window.KeyDown -= this.HandleKeyDown; // -- this doesn't work --> // this.KeyDownEvent -= this.HandleKeyDown;
			this.MouseDownEvent -= HandleMouseDown;

			// dispose must happen last
            base.HideWindows();
            browser.Hide();
            browser.Dispose();
            browser = null;
        }

        public override void SetupWindowPosition()
        {
            base.SetupWindowPosition();
            if (browser != null)
            {
                browser.BrowserSize = System.Drawing.Size.Round(this.Size);
                browser.BrowserLocation = Point.Round(this.Position);
                browser.PositionBrowser();
            }
        }

        #region Xml parsing methods
        protected override bool HandleAttribute(XmlAttribute attr)
        {
            switch (attr.Name)
            {
                case "url":
                    url = attr.Value;
                    return true;
                case "scrollbars":
                    SetScrollbarsEnabled(attr.Value.ToLower() == "true");
                    return true;
                case "line":
                    SetBrowserLine(Int32.Parse(attr.Value));
                    return true;
                case "errors":
                    SetBrowserErrors(attr.Value.ToLower() == "true");
                    return true;
                case "capture":
                    captureframe = attr.Value;
                    if (captureframe.StartsWith("$parent"))
                    {
                        Region tmp = UiParent;
                        while (tmp != null)
                        {
                            if (tmp.Name != null)
                                break;
                            tmp = tmp.UiParent;
                        }
                        // either we found an ancestor with a name, or ran out of ancestors
                        if ((tmp != null) && (tmp.Name != null))
                        {
                            captureframe = captureframe.Replace("$parent", tmp.Name);
                        }
                        else
                        {
                            log.WarnFormat("Did not find parent for \"{0}\", continuing anyway", captureframe);
                        }
                    }
                    return true;
                default:
                    return base.HandleAttribute(attr);
            }
        }
        #endregion

        #region IWebBrowser methods
        public string GetURL()
        {
            if ((browser != null) && (browser.URL != null))
            {
                return browser.URL;
            }
            return url;
        }
        public void SetURL(string u)
        {
            if (browser != null)
            {
                browser.Open(u);
            }
            url = u;
        }
        public bool GetScrollbarsEnabled()
        {
            return scrollbars;
        }
        public void SetScrollbarsEnabled()
        {
            SetScrollbarsEnabled(true);
        }
        public void SetScrollbarsEnabled(bool enabled)
        {
            scrollbars = enabled;
        }
        public int GetBrowserLine()
        {
            return line;
        }
        public void SetBrowserLine(int l)
        {
            line = l;
        }
        public bool GetBrowserErrors()
        {
            return browsererrors;
        }
        public void SetBrowserErrors()
        {
            SetBrowserErrors(true);
        }
        public void SetBrowserErrors(bool enabled)
        {
            browsererrors = enabled;
        }
        public void SetObjectForScripting(object obj)
        {
            scriptingObj = obj;
            if (browser != null)
                browser.ObjectForScripting = obj;
        }
        public object GetObjectForScripting()
        {
            return scriptingObj;
        }
        /// <summary>
        ///   This method lets us invoke a javascript method in the browser control.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object InvokeScript(string method, IEnumerable<object> args)
        {
            return browser.InvokeScript(method, args);
        }

        #endregion
    }

	//public class TaxiRouteFrame : Frame {
	//}

	//public class WorldFrame : Frame {
	//}

}
