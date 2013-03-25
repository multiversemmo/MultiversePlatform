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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	[Designer(typeof(BaseControlDesigner))]
	public abstract class BaseControl : ContainerControl, ISerializableControl
	{
		public BaseControl()
		{
			this.ControlAnchors = new ControlAnchors();
			this.DesignerDefaultValues = new Dictionary<string, object>();
		}

        [Browsable(false)]
		public FrameXmlDesignerLoader DesignerLoader { get; set; }

		protected override Size DefaultSize
		{
			get
			{
				return !base.DefaultSize.IsEmpty ?
					base.DefaultSize :
					new Size(100, 20);
					
			}
		}

		public void SetDefaultSize()
		{
			this.Size = this.DefaultSize;
		}

		/// <summary>
		/// Gets or sets the designer default values.
		/// </summary>
		/// <value>The designer default values.</value>
		protected Dictionary<string, object> DesignerDefaultValues { get; private set; }

		[Browsable(false)]
		public virtual List<string> InheritsList
		{
			get
			{
				var q = from layoutFrame in DesignerLoader.LayoutFrames
						where layoutFrame.GetType() == this.SerializationObject.GetType()
							&& layoutFrame != this.LayoutFrameType
						select layoutFrame.ExpandedName;

				return q.ToList<string>();
			}
		}

		#region ISerializableControl Members

		public abstract SerializationObject SerializationObject { get; set;}

		#endregion

		/// <summary>
		/// Gets or sets the anchors collection.
		/// </summary>
		/// <value>The anchors.</value>
		/// <remarks>
		/// This property is duplicated on the UI because otherwise 
		/// the resize glyph did not moved together with the control.
		/// </remarks>
		[Category("Layout")]
		[DisplayName("Anchors")]
		[TypeConverter(typeof(AnchorsTypeConverter))]
		[XmlIgnore]
		public LayoutFrameTypeAnchorsAnchor[] Anchors
		{
			get { return LayoutFrameType.Anchors; }
			set
			{
				LayoutFrameType.Anchors = value;
				this.ChangeLayout();
			}
		}

		[Browsable(false)]
		public LayoutFrameType LayoutFrameType
		{
			get { return (LayoutFrameType)this.SerializationObject; }
		}

		#region Anchoring

		[Browsable(false)]
		public ControlAnchors ControlAnchors { get; private set; }

		/// <summary>
		/// Changes the control bounds based on anchors.
		/// </summary>
		/// <param name="control">The control.</param>
		/// <param name="layoutFrameType">The layout frame.</param>
		public void DoChangeLayout()
		{
			this.ControlAnchors = new ControlAnchors();

			// process own achors first ...
			foreach (LayoutFrameTypeAnchors anchors in this.LayoutFrameType.AnchorsCollection)
			{
				foreach (LayoutFrameTypeAnchorsAnchor anchor in anchors.Anchor)
				{
					this.SetControlAnchor(anchor, false);
				}
			}

			// ... then the inherited anchors - they will be only in use if there is no own anchor
			LayoutFrameType inheritedLayoutFrame = this.LayoutFrameType.InheritedObject;
			if (inheritedLayoutFrame != null)
			{
				foreach (LayoutFrameTypeAnchors anchors in inheritedLayoutFrame.AnchorsCollection)
				{
					foreach (LayoutFrameTypeAnchorsAnchor anchor in anchors.Anchor)
					{
						this.SetControlAnchor(anchor, true);
					}
				}
			}

			ApplyControlAnchors();
		}

		/// <summary>
		/// Updates the control anchor.
		/// </summary>
		/// <param name="controlAnchor">The control anchor to be updated.</param>
		/// <param name="control">The control.</param>
		/// <param name="anchor">The anchor.</param>
		private void SetControlAnchor(LayoutFrameTypeAnchorsAnchor anchor, bool inherited)
		{
			Point anchorPoint = anchor.GetRelativePoint(this.Parent);

			if (anchor.Offset != null)
			{
				Size offset = anchor.Offset.GetSize();
                // Height is substracted because MultiverseInterface y coordinate increases upwards
				anchorPoint.Offset(offset.Width, -offset.Height);
			}

			string pointText = anchor.point.ToStringValue();

			ControlAnchors.SideAnchor sideAnchor = new ControlAnchors.SideAnchor() { Anchor = anchor, Offset = anchorPoint, Inherited = inherited };

			if (pointText.StartsWith("TOP"))
				this.ControlAnchors.Top = sideAnchor;
			if (pointText.StartsWith("BOTTOM"))
				this.ControlAnchors.Bottom = sideAnchor;
			if (pointText.EndsWith("LEFT"))
				this.ControlAnchors.Left = sideAnchor;
			if (pointText.EndsWith("RIGHT"))
				this.ControlAnchors.Right = sideAnchor;
			if (anchor.point == FRAMEPOINT.CENTER)
				this.ControlAnchors.Center = sideAnchor;
		}

		/// <summary>
		/// Sets the control bounds based on the control anchor information
		/// </summary>
		/// <param name="control">The control to be re-bounded</param>
		private void ApplyControlAnchors()
		{
            this.SuspendLayouting = true;
            try
            {
                if (ControlAnchors.Left != null)
                    this.Left = ControlAnchors.Left.Offset.X;

                if (ControlAnchors.Top != null)
                    this.Top = ControlAnchors.Top.Offset.Y;

                if (ControlAnchors.Right != null)
                {
                    if (ControlAnchors.Left != null)
                        this.Width = ControlAnchors.Right.Offset.X - ControlAnchors.Left.Offset.X;
                    else
                        this.Left = ControlAnchors.Right.Offset.X - this.Width;
                }

                if (ControlAnchors.Bottom != null)
                {
                    if (ControlAnchors.Top != null)
                        this.Height = ControlAnchors.Bottom.Offset.Y - ControlAnchors.Top.Offset.Y;
                    else
                        this.Top = ControlAnchors.Bottom.Offset.Y - this.Height;
                }

                // position this to the middle if there is no vertical anchor
                if ((ControlAnchors.Left != null || ControlAnchors.Right != null)
                    && (ControlAnchors.Top == null && ControlAnchors.Bottom == null))
                {
                    int offsetY = ControlAnchors.Left == null ?
                         ControlAnchors.Right.Offset.Y :
                         ControlAnchors.Left.Offset.Y;
                    this.Top = offsetY - this.Height / 2;
                }

                // position this to the middle if there is no horizontal anchor
                if ((ControlAnchors.Top != null || ControlAnchors.Bottom != null)
                    && (ControlAnchors.Left == null && ControlAnchors.Right == null))
                {
                    int offsetX = ControlAnchors.Top == null ?
                         ControlAnchors.Bottom.Offset.X :
                         ControlAnchors.Top.Offset.X;
                    this.Left = offsetX - this.Width / 2;
                }

                // center has only sense if no other anchor is specified
                if (ControlAnchors.Center != null &&
                    ControlAnchors.Left == null &&
                    ControlAnchors.Right == null &&
                    ControlAnchors.Top == null &&
                    ControlAnchors.Bottom == null)
                {
                    this.Left = ControlAnchors.Center.Offset.X - this.Width / 2;
                    this.Top = ControlAnchors.Center.Offset.Y - this.Height / 2;
                }
            }
            finally
            {
                this.SuspendLayouting = false;
            }
		}

		private bool explicitSuspendLayouting = false;

		protected bool SuspendLayouting
		{
			get
			{
				bool implicitSuspendLayouting = this.DesignerLoader != null ?
					this.DesignerLoader.IsLoading :
					true;
				return
					explicitSuspendLayouting || implicitSuspendLayouting;
			}
			set
			{
				explicitSuspendLayouting = value;
			}
		}

		#endregion

		/// <summary>
		/// Repositions the control and its child controls.
		/// </summary>
		/// <remarks>Recursive method.</remarks>
		public void ChangeLayout()
		{
			this.DoChangeLayout();

			foreach (BaseControl childControl in this.Controls.OfType<BaseControl>().SortControls())
			{
				childControl.ChangeLayout();
			}
		}

		/// <summary>
		/// Changes the layout of dependent controls.
		/// </summary>
		public void ChangeLayoutOfDependencies()
		{
            DesignerLoader.RootControl.RepositionUi();
		}

		/// <summary>
		/// Raises the <see cref="E:PropertyChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		/// <remarks>
		/// Can be overridden in descendants for custom property change handling
		/// </remarks>
        public virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			// notify designer loader about the change
			this.DesignerLoader.OnPropertyChanged(this, e);

			switch (e.PropertyName)
			{
				case "name":
					OnUpdateControl();
					break;
				case "inherits":
					RemoveDesignerDefaults();
					if (DesignerLoader != null && !DesignerLoader.IsLoading && !DesignerLoader.IsSerializing)
						DesignerLoader.ReloadControls();
					OnUpdateControl();
					break;
				case "SizeDimension":
					this.SuspendLayouting = true;
					try
					{
						this.Size = this.LayoutFrameType.SizeInPixels;
						ChangeLayoutOfDependencies();
						this.OnPropertyChanged(new PropertyChangedEventArgs("size"));
					}
					finally
					{
						this.SuspendLayouting = false;
					}
					break;
			}
		}

		private void RemoveDesignerDefaults()
		{
			if (this.Size == this.DefaultSize)
				this.LayoutFrameType.SizeDimension = null;

			foreach (var keyValue in this.DesignerDefaultValues)
			{
				if (LayoutFrameType.Properties.HasValue(keyValue.Key) &&
					LayoutFrameType.Properties[keyValue.Key].Equals(keyValue.Value))
				{
					LayoutFrameType.Properties.Remove(keyValue.Key);
				}
			}
		}

		protected virtual void OnUpdateControl()
		{
			this.Invalidate();
		}

		protected override void OnControlRemoved(ControlEventArgs e)
		{
            if (this.DesignerLoader != null)
			    this.DesignerLoader.NotifyControlRemoval(e.Control);

			base.OnControlRemoved(e);
		}

		[Browsable(false)]
		[XmlIgnore]
		public virtual EventChoice? DefaultEventChoice
		{
			get { return null; }
		}

        [Browsable(false)]
		public bool Inherited { get; set; }

		[Browsable(false)]
		public bool HasActions { get; protected set; }
	}
}
