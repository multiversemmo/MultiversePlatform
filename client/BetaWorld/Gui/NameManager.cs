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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Animating;

using Multiverse.Base;
using Multiverse.Config;
using Multiverse.Gui;
using Multiverse.Interface;

#endregion


namespace Multiverse.BetaWorld.Gui {

	/// <summary>
	///   Class that represents a static text widget attached to an
	///   Axiom node and associated with a Multiverse world object.
	/// </summary>
	public class AttachedWidget : IDisposable
	{
		long oid;
		Node node;
        protected Window widget;
		bool nodeVisible = false;
		bool drawWidget = false;

        public AttachedWidget(long oid, Window widget)
		{
			this.oid = oid;
			this.widget = widget;
			WindowManager.Instance.AttachWindow(widget);
		}

		public virtual void Dispose() {
			WindowManager.Instance.DestroyWindow(widget);
		}

		public virtual void SetPosition(float screenX, float screenY, float screenZ) {
			screenX = (float)Math.Floor(screenX - widget.Width / 2);
			screenY = (float)Math.Floor(screenY - widget.Height / 2);
			widget.Position = new Point(screenX, screenY);
			widget.ZValue = screenZ;
		}

		public long Oid {
			get {
				return oid;
			}
		}
		public string Name {
			get {
				return widget.Name;
			}
		}
		public Node Node {
			get {
				return node;
			}
			set {
				node = value;
			}
		}
		public bool NodeVisible {
			get {
				return nodeVisible;
			}
			set {
				nodeVisible = value;
				widget.Visible = drawWidget && nodeVisible;
			}
		}
		public bool Visible {
			get {
				return drawWidget && nodeVisible;
			}
			set {
				drawWidget = value;
				widget.Visible = drawWidget && nodeVisible;
			}
		}
		public Window Widget {
			get {
				return widget;
			}
		}
	}

	public class BubbleTextNode : AttachedWidget
	{
		long expireTime;

		public BubbleTextNode(long oid) : 
			base(oid, new WLBubbleText("bubblechat." + oid))
		{
			widget.Position = new Point(0, 0);
			widget.MaximumSize = new Size(200, 200);
			widget.Alpha = 0.9f;
            //widget.BackgroundColors = new ColorRect(ColorEx.White);
            //widget.FrameEnabled = true;
            //widget.BackgroundEnabled = true;
			widget.Visible = true;
		}

		public bool IsExpired(long now) {
			// return false;
			return (now > expireTime);
		}

		public void SetText(string text, long expire) {
			widget.Visible = true;
            if (widget is WLBubbleText) {
                WLBubbleText bubbleTextWidget = (WLBubbleText)widget;
                bubbleTextWidget.SetText(text);
            }
        	expireTime = expire;
        }
        public void SetFont(Font font) {
            if (widget is WLBubbleText) {
                WLBubbleText bubbleTextWidget = (WLBubbleText)widget;
                bubbleTextWidget.SetFont(font);
            }
        }
	}

    public class NameNode : AttachedWidget {
        public NameNode(long oid)
            :
            base(oid, new WLNameText("name." + oid)) {
            widget.Position = new Point(0, 0);
            widget.Alpha = 1.0f;
            //widget.FrameEnabled = false;
            //widget.BackgroundEnabled = false;
            widget.Visible = false;
            // FIXME
            //ZOrderedStaticText textWidget = (ZOrderedStaticText)widget;
            //textWidget.SetTextColor(ColorEx.Cyan);
            //textWidget.HorizontalFormat = HorizontalTextFormat.Centered;
        }

        public void SetText(string text) {
            // widget.Visible = true;
            if (widget is WLNameText) {
                WLNameText nameTextWidget = (WLNameText)widget;
                nameTextWidget.SetText(text);
            }
        }

        public void SetFont(Font font) {
            if (widget is WLNameText)
                ((WLNameText)widget).SetFont(font);
        }
    }

	/// <summary>
	///   Scene object that wraps the widget for use in visibility tests and 
	///   attachment to other objects.
	/// </summary>
	public class WidgetSceneObject : MovableObject
	{
		// AxisAlignedBox box = new AxisAlignedBox(Vector3.Zero, Vector3.Zero);

		AttachedWidget widgetNode;
        AxisAlignedBox boundingBox;

		public WidgetSceneObject() {
            boundingBox = new AxisAlignedBox();
            boundingBox.IsNull = false;
		}

		public override void NotifyCurrentCamera(Camera camera) {
			if (!camera.IsObjectVisible(GetWorldBoundingBox(true))) {
				widgetNode.NodeVisible = false;
			} else {
				widgetNode.NodeVisible = true;
            }
		}

		public override void UpdateRenderQueue(RenderQueue queue) {
		}

        public override AxisAlignedBox GetWorldBoundingBox(bool derive) {
            return base.GetWorldBoundingBox(derive);
        }

		public AttachedWidget WidgetNode {
			get {
				return widgetNode;
			}
			set {
				widgetNode = value;
				this.Name = widgetNode.Name;
			}
		}

        /// <summary>
        ///   Get the bounding box of this MovableObject.
        /// </summary>
        /// <remarks>
        ///   The returned bounding box will be modified, so be sure
        ///   to return a new object, instead of our own.
        /// </remarks>
		public override AxisAlignedBox BoundingBox {
			get {
                return (AxisAlignedBox)boundingBox.Clone();
			}
		}

		public override float BoundingRadius {
			get {
				return 1.0f;
			}
		}
	}
#if TEMP_DISABLED
	public class DummyScrollbar : Scrollbar {

		public DummyScrollbar(string name) : base(name) {
		}

		/// <summary>
		///		Create a <see cref="PushButton"/> based widget to use as the decreaseButton button for this scroll bar.
		/// </summary>
		/// <returns>A custom PushButton implementation.</returns>
		protected override PushButton CreateDecreaseButton() {
			throw new NotImplementedException();
		}
		
		/// <summary>
		///		Create a <see cref="PushButton"/> based widget to use as the increaseButton button for this scroll bar.
		/// </summary>
		/// <returns>A custom PushButton implementation.</returns>
		protected override PushButton CreateIncreaseButton() {
			throw new NotImplementedException();
		}

		/// <summary>
		///		Create a <see cref="Thumb"/> based widget to use as the thumb for this scroll bar.
		/// </summary>
		/// <returns>A custom thumb implementation.</returns>
		protected override Thumb CreateThumb() {
			throw new NotImplementedException();
		}

		/// <summary>
		///		Given window location <paramref name="point"/>, return a value indicating what change should be made to the scroll bar.
		/// </summary>
		/// <param name="point">Point object describing a pixel position in window space.</param>
		/// <returns>
		///		- -1 to indicate scroll bar position should be moved to a lower value.
		///		-  0 to indicate scroll bar position should not be changed.
		///		- +1 to indicate scroll bar position should be moved to a higher value.
		/// </returns>
		protected override float GetAdjustDirectionFromPoint(CrayzEdsGui.Base.Point point) {
			throw new NotImplementedException();
		}

		/// <summary>
		///		Return the value that best represents current scroll bar position given the current location of the thumb.
		/// </summary>
		/// <returns>float value that, given the thumb widget position, best represents the current position for the scroll bar.</returns>
		protected override float GetPositionFromThumb() {
			throw new NotImplementedException();
		}

		/// <summary>
		///		Layout the scroll bar component widgets
		/// </summary>
		protected override void LayoutComponentWidgets() {
			// noop
		}

		/// <summary>
		///		Update the size and location of the thumb to properly represent the current state of the scroll bar.
		/// </summary>
		protected override void UpdateThumb() {
			// noop
		}

		protected override void DrawSelf(float z) {
			// noop
		}
	}
#endif

#if DISABLED
	/// <summary>
	///   Class that acts like a static text widget, but handles its quads
	///   by creating a billboard attached to an Axiom Node instead of 
	///   just drawing them to the screen.
	/// </summary>
	public class NameWidget : StaticText
	{
		MultiverseRenderer renderer;
		TextBillboardSet textBillboard;
		SceneNode node;

		public NameWidget(SceneNode node, string name) : base(name) {
			this.node = node;
			textBillboard = new TextBillboardSet("NameBar/" + name);
			node.AttachObject(textBillboard);
			Renderer tmp = GuiSystem.Instance.Renderer;
			Debug.Assert(tmp is MultiverseRenderer, "Not using multiverse renderer");
			renderer = (MultiverseRenderer)tmp;
		}

		protected override void DrawSelf(float z) {
			renderer.SetSceneObject(textBillboard);
			try {
				MetricsMode tmp = this.MetricsMode;
				this.MetricsMode = MetricsMode.Absolute;
				renderer.SetWidgetSize(this.Size);
				this.MetricsMode = tmp;
				Logger.Log(0, "Calling DrawSelf: node = " + node);
				// base.DrawSelf(z);
				Logger.Log(0, "Done calling DrawSelf");
			} finally {
				renderer.SetSceneObject(null);
			}
		}
		
		#region Methods for Scrollbars

		/// <summary>
		///   Create and return a pointer to a Scrollbar widget for use as
		///   vertical scroll bar
		/// </summary>
		/// <returns></returns>
		protected override Scrollbar CreateVertScrollbar() {
			return new DummyScrollbar("dummy");
		}

		/// <summary>
		///   Create and return a pointer to a Scrollbar widget for use as
		///   horizontal scroll bar
		/// </summary>
		/// <returns></returns>
		protected override Scrollbar CreateHorzScrollbar() {
			return new DummyScrollbar("dummy");
		}

		#endregion

		public TextBillboardSet TextBillboard {
			get {
				return textBillboard;
			}
			set {
				textBillboard = value;
			}
		}
	}

#endif

	public abstract class WidgetManager
	{
		protected Client client;
		protected WorldManager worldManager;
		protected Window window;
		private bool visible = false;

		public WidgetManager(Window rootWindow, Client client,
							 WorldManager worldManager) {
			this.client = client;
			this.worldManager = worldManager;
			this.window = rootWindow;
		}

		protected bool SetWidgetPosition(AttachedWidget widgetNode,
										 Vector3 widgetOffset, Point pixelOffset) {
			Vector3 widgetPosition = widgetNode.Node.DerivedPosition + widgetOffset;
			float screenX, screenY, screenZ;
			if (!client.GetScreenPosition(widgetPosition, out screenX, out screenY, out screenZ)) {
				widgetNode.Visible = false;
				return false;
			}
			screenX = screenX * window.Width + pixelOffset.x;
			screenY = screenY * window.Height + pixelOffset.y;
			if ((screenX > window.Width) || (screenX < 0.0f) ||
				(screenY > window.Height) || (screenY < 0.0f)) {
				widgetNode.Visible = false;
				return false;
			}
			widgetNode.SetPosition(screenX, screenY, screenZ);
			return true;
		}

		protected bool SetWidgetPosition(AttachedWidget widgetNode, Vector3 widgetOffset) {
			Point zero = new Point(0, 0);
			return SetWidgetPosition(widgetNode, widgetOffset, zero);
		}

		public bool Visible {
			get {
				return visible;
			}
			set {
				visible = value;
			}
		}
	}


	// FIXME: Locking
	public class NameManager : WidgetManager
	{
		private Font font;

		private Dictionary<long, NameNode> nameDictionary;

		public NameManager(Window rootWindow, Client client, 
						   WorldManager worldManager) :
			base(rootWindow, client, worldManager)
		{
			// Renderer that can be used to render stuff to a mesh
			// meshRenderer = new MultiverseRenderer(rootWindow);
			// font = new MultiverseFont("MV-Tahoma-30", "Tahoma", 30, meshRenderer,
			// 				          FontFlags.None, (char)32, (char)127);

            // fontMaterial = (Material)MaterialManager.Instance.Create("font-material");
            // SetupFontMaterial();

			nameDictionary = new Dictionary<long, NameNode>();

			if (FontManager.Instance.ContainsKey("NameFont"))
				font = FontManager.Instance.GetFont("NameFont");
			else
				font = FontManager.Instance.CreateFont2("NameFont", "Verdana", 10);
		}


#if DISABLED
        private void SetupFontMaterial() {
            Technique technique = fontMaterial.CreateTechnique();
            Pass pass = technique.CreatePass();
            TextureUnitState texUnitState = pass.CreateTextureUnitState();
            texUnitState.SetTextureName(font.TextureName);
            // texUnitState.SetAlphaOperation(LayerBlendOperation.AlphaBlend);
            // texUnitState.SetTextureFiltering(FilterOptions.Linear);
            texUnitState.TextureAddressing = TextureAddressing.Clamp;
            texUnitState.TextureMatrix = Matrix4.Identity;
            texUnitState.TextureCoordSet = 0;

//			renderSystem.SetTextureCoordCalculation( 0, TexCoordCalcMethod.None );
//			renderSystem.SetTextureUnitFiltering(0, FilterOptions.Linear, FilterOptions.Linear, FilterOptions.Point);
//			renderSystem.SetAlphaRejectSettings(0, CompareFunction.AlwaysPass, 0);
//			renderSystem.SetTextureBlendMode( 0, unitState.ColorBlendMode );
//			renderSystem.SetTextureBlendMode( 0, unitState.AlphaBlendMode );
//
//			// enable alpha blending
//			renderSystem.SetSceneBlending(SceneBlendFactor.SourceAlpha, SceneBlendFactor.OneMinusSourceAlpha);

        }
#endif

		public void Tick(float timeSinceLastFrame, long now) {
			lock (nameDictionary) {
				// Draw the name elements
				foreach (AttachedWidget node in nameDictionary.Values)
					UpdateNode(node, now);
			}
		}

		private bool IsTargeted(long oid) {
			return (client.Target != null && client.Target.Oid == oid);
		}

		/// <summary>
		///   Lock on the name dictionary is held already
		/// </summary>
		/// <param name="widgetNode"></param>
		/// <param name="now"></param>
		private void UpdateNode(AttachedWidget widgetNode, long now) {
			const float MaxFadeRange = 40 * Client.OneMeter;
			const float MinFadeRange = 20 * Client.OneMeter;
			const float MaxFadeRangeSquared = MaxFadeRange * MaxFadeRange;
			const float MinFadeRangeSquared = MinFadeRange * MinFadeRange;

			ColorEx[] SelectedColors = new ColorEx[4];
			SelectedColors[0] = new ColorEx(1, 0, 1.0f, 1.0f);
			SelectedColors[1] = new ColorEx(1, 0, 0.9f, 0.9f);
			SelectedColors[2] = new ColorEx(1, 0, 0.8f, 0.8f);
			SelectedColors[3] = new ColorEx(1, 0, 0.7f, 0.7f);

			ColorEx StandardColor = new ColorEx(1, 0, 0.9f, 0.9f);
			
			Axiom.MathLib.Vector3 ray = 
				widgetNode.Node.DerivedPosition - client.Camera.DerivedPosition;

			if (!IsTargeted(widgetNode.Oid)) {
				// Don't show if they are too far away or if they are the player, 
				// unless they are selected
				if (widgetNode.Oid == client.PlayerId || ray.LengthSquared > MaxFadeRangeSquared) {
					widgetNode.Visible = false;
					return;
				}
			}

			// Put the name widget about one foot above the mob's head
            if (!SetWidgetPosition(widgetNode, Vector3.Zero))
            // if (!SetWidgetPosition(widgetNode, new Vector3(0, 0.3f * Client.OneMeter, 0)))
                return;

			if (ray.LengthSquared < MinFadeRangeSquared || IsTargeted(widgetNode.Oid))
				widgetNode.Widget.Alpha = 1.0f;
			else
				widgetNode.Widget.Alpha = 1.0f - (ray.Length - MinFadeRange) / (MaxFadeRange - MinFadeRange);
#if OLD_CODE
			ZOrderedStaticText textWidget = (ZOrderedStaticText)widgetNode.Widget;
			if (IsTargeted(widgetNode.Oid)) {
				const int Period = 500;
				// Blink the name with a 500ms period.
				float tmp = now % Period;
				tmp /= Period;
				int i = (int)Math.Min(3, 4 * Math.Sin(Math.PI * tmp));
				textWidget.SetTextColor(SelectedColors[i]);
			} else
				textWidget.SetTextColor(StandardColor);
#endif
			if (this.Visible)
				widgetNode.Visible = true;
		}

        /// <summary>
        ///   Inform the name manager about an object node for which the name
        ///   should be displayed.
        /// </summary>
        /// <param name="oid">the object id of the node (used as the key for lookups)</param>
        /// <param name="objNode">the object node whose name should be displayed</param>
        public void AddNode(long oid, ObjectNode objNode) {
			NameNode widgetNode = new NameNode(oid);
            Node attachNode = null;
            WidgetSceneObject attachObj = new WidgetSceneObject();
            attachObj.WidgetNode = widgetNode;
            AttachmentPoint ap = objNode.GetAttachmentPoint("name-disabled");
            if (ap == null) {
                // Default to a bit larger than the height of the bounding box
                float objectHeight = objNode.Entity.BoundingBox.Size.y * 1.02f;
                ap = new AttachmentPoint("name-disabled", null, Quaternion.Identity, Vector3.UnitY * objectHeight);
            }
            attachNode = objNode.AttachLocalObject(ap, attachObj);
            if (attachNode == null) {
				widgetNode.NodeVisible = true; 
				widgetNode.Node = objNode.SceneNode;
			} else {
				// The node visible will be set by the attachObj
                widgetNode.Node = attachNode;
			}

            // FIXME
            //widgetNode.Widget.Text = objNode.Name;
            //widgetNode.Widget.Font = font;
			window.AddChild(widgetNode.Widget);
            widgetNode.Widget.Initialize();
            widgetNode.SetFont(font);
            widgetNode.SetText(objNode.Name);
            lock (nameDictionary) {
                nameDictionary[oid] = widgetNode;
            }
		}

		public void RemoveNode(long oid) {
			lock (nameDictionary) {
				if (!nameDictionary.ContainsKey(oid))
					return;
				NameNode node = nameDictionary[oid];
				window.RemoveChild(node.Widget);
				node.Dispose();
				nameDictionary.Remove(oid);
			}
		}

		public void ClearNodes() {
			lock (nameDictionary) {
				foreach (long oid in nameDictionary.Keys) {
					NameNode node = nameDictionary[oid];
					window.RemoveChild(node.Widget);
					node.Dispose();
				}
				nameDictionary.Clear();
			}
		}
#if DISABLED
		public void AddNode(int oid, ObjectNode objNode) {
			// Create a namebar scene node and entity to handle names
			Axiom.MathLib.Vector3 offset = new Axiom.MathLib.Vector3(0, 2 * Client.OneMeter, 0);
			SceneNode sceneNode = objNode.SceneNode.CreateChildSceneNode("namebar." + oid, offset);
			TexturedBillboardSet widget = new TexturedBillboardSet("billboard." + oid);
            widget.MaterialName = "font-material";
            widget.BillboardType = BillboardType.Point;
            widget.CommonDirection = Axiom.MathLib.Vector3.NegativeUnitZ;
            sceneNode.AttachObject(widget);

            // Set the target mesh for methods like Font.DrawText
			meshRenderer.BeginRender(widget);
			Rect dummyRect = new Rect();
			dummyRect.left = 0;
			dummyRect.top = 0;
			dummyRect.Height = 150;
			dummyRect.Width = 600;
			font.DrawText("This is a test", dummyRect, 0);
            meshRenderer.EndRender();
        }

		public void AddNode(int oid, ObjectNode objNode) {
			NameNode nameNode = new NameNode();
//			nameNode.nameBar = (StaticText)WindowManager.Instance.CreateWindow(
//								"WindowsLook.WLStaticText", "Window/LabelText-" + oid);
//			nameNode.nameBar.Font = font;
//			nameNode.nameBar.Text = "Name: " + objNode.Name;
//			nameNode.nameBar.HorizontalFormat = HorizontalTextFormat.Center;
//			nameNode.nameBar.VerticalFormat = VerticalTextFormat.Top;
//			nameNode.nameBar.SetTextColor(new Color(1, 0, 0, 0));
//			nameNode.nameBar.MetricsMode = MetricsMode.Absolute;
//			float chromeHeight = 
//				nameNode.nameBar.UnclippedPixelRect.Height - nameNode.nameBar.UnclippedInnerRect.Height;
//			Logger.Log(0, "chromeHeight: {0} Line Spacing: {1}", chromeHeight, font.LineSpacing);
//			nameNode.nameBar.Size = new Size(60, chromeHeight + font.LineSpacing + 5); // FIXME (why do i need +5)
			nameDictionary[oid] = nameNode;
//			window.AddChild(nameNode.nameBar);
//			nameNode.nameBar.MetricsMode = MetricsMode.Relative;

			// Console.WriteLine("Adding node for: " + objNode.Name);
			NameWidget widget = new NameWidget(objNode.SceneNode, objNode.Name);
			widget.Initialize();
			// Object will be clipped to this size
			widget.MetricsMode = MetricsMode.Relative;
			widget.Size = new Size(1.0f, 1.0f);
			widget.Position = new Point(0.0f, 0.0f);
			widget.Text = objNode.Name;
			widget.HorizontalFormat = HorizontalTextFormat.Center;
			widget.VerticalFormat = VerticalTextFormat.Centered;
			widget.Visible = true;
			window.AddChild(widget);
		}
#endif
	}

	public class BubbleTextManager : WidgetManager
	{
		// protected const int VertexBufferCapacity = 4096;

		// private MultiverseFont font;
		// private Material fontMaterial;
		private Font font;

		// private MultiverseRenderer meshRenderer;

		private Dictionary<long, BubbleTextNode> bubbleDictionary;

		public BubbleTextManager(Window rootWindow, Client client,
								 WorldManager worldManager) :
			base(rootWindow, client, worldManager)
		{
			// Renderer that can be used to render stuff to a mesh
			// meshRenderer = new MultiverseRenderer(rootWindow);
			// font = new MultiverseFont("MV-Tahoma-30", "Tahoma", 30, meshRenderer,
			// 				          FontFlags.None, (char)32, (char)127);

			// fontMaterial = (Material)MaterialManager.Instance.Create("font-material");
			// SetupFontMaterial();

			bubbleDictionary = new Dictionary<long, BubbleTextNode>();

			if (FontManager.Instance.ContainsKey("BubbleTextFont"))
				font = FontManager.Instance.GetFont("BubbleTextFont");
			else
				font = FontManager.Instance.CreateFont2("BubbleTextFont", "Verdana", 10);
		}

#if DISABLED
        private void SetupFontMaterial() {
            Technique technique = fontMaterial.CreateTechnique();
            Pass pass = technique.CreatePass();
            TextureUnitState texUnitState = pass.CreateTextureUnitState();
            texUnitState.SetTextureName(font.TextureName);
            // texUnitState.SetAlphaOperation(LayerBlendOperation.AlphaBlend);
            // texUnitState.SetTextureFiltering(FilterOptions.Linear);
            texUnitState.TextureAddressing = TextureAddressing.Clamp;
            texUnitState.TextureMatrix = Matrix4.Identity;
            texUnitState.TextureCoordSet = 0;

//			renderSystem.SetTextureCoordCalculation( 0, TexCoordCalcMethod.None );
//			renderSystem.SetTextureUnitFiltering(0, FilterOptions.Linear, FilterOptions.Linear, FilterOptions.Point);
//			renderSystem.SetAlphaRejectSettings(0, CompareFunction.AlwaysPass, 0);
//			renderSystem.SetTextureBlendMode( 0, unitState.ColorBlendMode );
//			renderSystem.SetTextureBlendMode( 0, unitState.AlphaBlendMode );
//
//			// enable alpha blending
//			renderSystem.SetSceneBlending(SceneBlendFactor.SourceAlpha, SceneBlendFactor.OneMinusSourceAlpha);

        }
#endif

        /// <summary>
        ///   Called periodically to update the attached gui widgets to match
        ///   the position of the underlying object.
        /// </summary>
        /// <param name="timeSinceLastFrame"></param>
        /// <param name="now"></param>
		public void Tick(float timeSinceLastFrame, long now) {
			// Draw the name elements
			lock (bubbleDictionary) {
				foreach (AttachedWidget node in bubbleDictionary.Values)
					UpdateNode(node, now);
			}
		}

		/// <summary>
		///   Lock on the bubble dictionary is held already
		/// </summary>
		/// <param name="widgetNode"></param>
		/// <param name="now"></param>
		private void UpdateNode(AttachedWidget widgetNode, long now) {
			const float MaxFadeRange = 40 * Client.OneMeter;
			const float MinFadeRange = 20 * Client.OneMeter;
			const float MaxFadeRangeSquared = MaxFadeRange * MaxFadeRange;
			const float MinFadeRangeSquared = MinFadeRange * MinFadeRange;

			Axiom.MathLib.Vector3 ray =
				widgetNode.Node.DerivedPosition - client.Camera.DerivedPosition;
			
			// Don't show if they are too far away
			if (ray.LengthSquared > MaxFadeRangeSquared) {
				widgetNode.Visible = false;
				return;
			}

			BubbleTextNode bubbleNode = (BubbleTextNode)widgetNode;
			if (bubbleNode.IsExpired(now)) {
				widgetNode.Visible = false;
				return;
			}

			Vector3 widgetOffset = new Vector3(0, 0.3f * Client.OneMeter, 0);
			Point pixelOffset = new Point(0, -50);

            // if (!SetWidgetPosition(widgetNode, widgetOffset, pixelOffset))
            if (!SetWidgetPosition(widgetNode, Vector3.Zero, pixelOffset))
				return;

			if (ray.LengthSquared < MinFadeRangeSquared)
				widgetNode.Widget.Alpha = 1.0f;
			else
				widgetNode.Widget.Alpha = 1.0f - (ray.Length - MinFadeRange) / (MaxFadeRange - MinFadeRange);
			if (this.Visible)
				widgetNode.Visible = true;
		}

		public void SetBubbleText(long oid, string text, long now) {
			lock (bubbleDictionary) {
				if (bubbleDictionary.ContainsKey(oid)) {
					BubbleTextNode node = bubbleDictionary[oid];
					// expire in 5 seconds, plus one second for each 5 chars
					long expire = now + 5000 + (1000 * text.Length) / 5;
					node.SetText(text, expire);
					UpdateNode(node, now);
				}
			}
		}

		// FIXME: Thread safety
		public void AddNode(long oid, ObjectNode objNode) {
			BubbleTextNode widgetNode = new BubbleTextNode(oid);
			Node attachNode = null;
            WidgetSceneObject attachObj = new WidgetSceneObject();
            attachObj.WidgetNode = widgetNode;
            AttachmentPoint ap = objNode.GetAttachmentPoint("bubble-disabled");
            if (ap == null) {
                // Default to a bit larger than the height of the bounding box
                float objectHeight = objNode.Entity.BoundingBox.Size.y * 1.02f;
                ap = new AttachmentPoint("bubble-disabled", null, Quaternion.Identity, Vector3.UnitY * objectHeight);
            }
            attachNode = objNode.AttachLocalObject(ap, attachObj);
            if (attachNode == null) {
				widgetNode.NodeVisible = true; 
				widgetNode.Node = objNode.SceneNode;
			} else {
				// The node visible will be set by the attachObj
                widgetNode.Node = attachNode;
			}

            // FIXME
			// widgetNode.Widget.Font = font;
            window.AddChild(widgetNode.Widget);
            widgetNode.Widget.Initialize();
            widgetNode.SetFont(font);
            lock (bubbleDictionary) {
                bubbleDictionary[oid] = widgetNode;
            }
		}

		public void RemoveNode(long oid) {
			lock (bubbleDictionary) {
				if (!bubbleDictionary.ContainsKey(oid))
					return;
				BubbleTextNode node = bubbleDictionary[oid];
				window.RemoveChild(node.Widget);
				bubbleDictionary.Remove(oid);
				node.Dispose();
			}
		}

		public void ClearNodes() {
			lock (bubbleDictionary) {
				foreach (long oid in bubbleDictionary.Keys) {
					BubbleTextNode node = bubbleDictionary[oid];
					window.RemoveChild(node.Widget);
					node.Dispose();
				}
				bubbleDictionary.Clear();
			}
		}
    }
}
