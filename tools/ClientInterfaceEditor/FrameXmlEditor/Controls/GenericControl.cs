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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.Drawing.Design;
using System.Collections;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
    /// <summary>
    /// Generic base class for all WoW controls.
    /// </summary>
    /// <typeparam name="TS">The type of the Serialization Object.</typeparam>
    public partial class GenericControl<TS> : BaseControl, ICustomTypeDescriptor
        where TS: LayoutFrameType, new()
    {
        #region Control support

		[Browsable(false)]
		public Control InnerControl { get; private set; }

		private StringFormat stringFormat = new StringFormat();

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericControl&lt;TS&gt;"/> class.
		/// </summary>
		/// <remarks>The class has a non-default constructor. Don't remove this empty default constructor.</remarks>
        public GenericControl()
        {
			Initialize();
		}

		private void Initialize()
		{
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

			// used for displaying the type name in the center of the control
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericControl&lt;TS&gt;"/> class.
		/// </summary>
		/// <param name="control">The inner control.</param>
		public GenericControl(Control control)
		{
			this.InnerControl = control;
			if (InnerControl != null)
			{
				InnerControl.Parent = this;
				InnerControl.Dock = DockStyle.Fill;
			}
			Initialize();
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance has border.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has border; otherwise, <c>false</c>.
		/// </value>
		protected bool HasBorder { get; set;}

		protected virtual bool DrawName
		{
			get { return true; }
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (this.DrawName)
			{
				// draw the control name in the middle. If there are specific inner controls, this text might be hidden.
				e.Graphics.DrawString(this.Name, Font, Brushes.Black, this.ClientRectangle, stringFormat);
			}

            base.OnPaint(e);

			// draw the border if necessary
			if (this.HasBorder)
			{
				Rectangle rect = this.ClientRectangle;
				if (rect.Width > 0) rect.Width--;
				if (rect.Height > 0) rect.Height--;
				e.Graphics.DrawRectangle(Pens.Black, rect);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.ControlAdded"/> event.
		/// Brings the newly added control in front.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ControlEventArgs"/> that contains the event data.</param>
        protected override void OnControlAdded(ControlEventArgs e)
        {
			if (e.Control != this.InnerControl && !(this is GenericFrameControl<TS>))
				throw new ArgumentException("This control cannot host further controls.");

			base.OnControlAdded(e);
			e.Control.BringToFront();
        }

		[Category("Design")]
		public string name
		{
			get { return this.LayoutFrameType.name; }
			set
			{
				string oldName = this.Name;
				// TODO: verify control name uniqueness
				this.Name = value;
				this.LayoutFrameType.name = value;
				this.Site.Name = this.LayoutFrameType.ExpandedName;
			}
		}

        #endregion

        #region Serialization Support

        private TS typedSerializationObject = null;

        [Browsable(false)]
        public TS TypedSerializationObject
        {
            get
            {
				if (typedSerializationObject == null)
				{
					typedSerializationObject = new TS();
					OnUpdateControl();
				}

                return typedSerializationObject;
            }
            set
            {
                typedSerializationObject = value;
				OnUpdateControl();
            }
        }

        [Browsable(false)]
        public override SerializationObject SerializationObject
        {
            get
            {
                return TypedSerializationObject;
            }
            set
            {
                TypedSerializationObject = (TS)value;
            }
        }

        #endregion

        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptorHelper.GetAttributes(this);
        }

        public string GetClassName()
        {
            return TypeDescriptorHelper.GetClassName(this);
        }

        public string GetComponentName()
        {
            return TypeDescriptorHelper.GetComponentName(this);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptorHelper.GetConverter(this);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptorHelper.GetDefaultEvent(this);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptorHelper.GetDefaultProperty(this);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptorHelper.GetEditor(this, editorBaseType);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptorHelper.GetEvents(this);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptorHelper.GetEvents(this, attributes);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptorHelper.GetProperties(this);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return TypeDescriptorHelper.GetProperties(this, attributes);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return TypeDescriptorHelper.GetPropertyOwner(this, pd);
        }

        #endregion

		#region layouting support

		protected override void OnMove(EventArgs e)
		{
			base.OnMove(e);
			if (this.SuspendLayouting)
				return;

			if (this.ControlAnchors.Left != null)
			{
				this.ControlAnchors.Left.SetX(this.Left, this.Parent);
			}

			if (this.ControlAnchors.Top != null)
			{
				this.ControlAnchors.Top.SetY(this.Top, this.Parent);
			}

			if (this.ControlAnchors.Right != null)
			{
				this.ControlAnchors.Right.SetX(this.Right, this.Parent);
			}

			if (this.ControlAnchors.Bottom != null)
			{
				this.ControlAnchors.Bottom.SetY(this.Bottom, this.Parent);
			}

			if ((this.ControlAnchors.Left != null || this.ControlAnchors.Right != null) 
				&& this.ControlAnchors.Top == null && this.ControlAnchors.Bottom == null)
			{
				ControlAnchors.SideAnchor sideAnchor = this.ControlAnchors.Left != null ?
					this.ControlAnchors.Left :
					this.ControlAnchors.Right;
				sideAnchor.SetY(this.Top + this.Height / 2, this.Parent);
			}

			if ((this.ControlAnchors.Top != null || this.ControlAnchors.Bottom != null)
				&& this.ControlAnchors.Left == null && this.ControlAnchors.Right == null)
			{
				ControlAnchors.SideAnchor sideAnchor = this.ControlAnchors.Top != null ?
					this.ControlAnchors.Top :
					this.ControlAnchors.Bottom;
				sideAnchor.SetX(this.Left + this.Width / 2, this.Parent);
			}

			if (this.ControlAnchors.Center != null)
			{
				this.ControlAnchors.Center.SetX((this.Right + this.Left) / 2, this.Parent);
				this.ControlAnchors.Center.SetY((this.Top + this.Bottom) / 2, this.Parent);
			}

			ChangeLayoutOfDependencies();
            this.OnPropertyChanged(new PropertyChangedEventArgs("anchors"));
		}

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Resize"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (this.SuspendLayouting)
				return;

			if (this.ControlAnchors.Left != null && this.ControlAnchors.Right != null)
			{
				this.ControlAnchors.Left.SetX(this.Left, this.Parent);
				this.ControlAnchors.Right.SetX(this.Right, this.Parent);
			}

			if (this.ControlAnchors.Top != null && this.ControlAnchors.Bottom != null)
			{
				this.ControlAnchors.Top.SetY(this.Top, this.Parent);
				this.ControlAnchors.Bottom.SetY(this.Bottom, this.Parent);
			}

			if (this.ControlAnchors.Center != null)
			{
				this.ControlAnchors.Center.SetX((this.Right + this.Left) / 2, this.Parent);
				this.ControlAnchors.Center.SetY((this.Top + this.Bottom) / 2, this.Parent);
			}

			Dimension.Size dimension = Dimension.Clone<Dimension.Size>(this.TypedSerializationObject.SizeDimension);
			if (dimension == null)
			{
				dimension = new Dimension.Size();
			}

			dimension.Update(this.Width, this.Height);
			this.TypedSerializationObject.SizeDimension = dimension;

			ChangeLayoutOfDependencies();
            this.OnPropertyChanged(new PropertyChangedEventArgs("size"));
        }

		#endregion

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			ISerializableControl parent = (this.Parent as ISerializableControl);
			if ((parent == null) || (parent.DesignerLoader == null))
			{
				this.DesignerLoader = null;
			}
			else
			{
				this.DesignerLoader = parent.DesignerLoader;

				if (string.IsNullOrEmpty(this.name))
				{
					string typeName = typeof(TS).Name;
					if (typeName.EndsWith("Type"))
					{
						typeName = typeName.Substring(0, typeName.Length - 4);
					}

					var names = from layoutFrame in this.DesignerLoader.LayoutFrames
								select layoutFrame.ExpandedName;
					this.name = UniqueName.GetUniqueName(this.name, typeName, names);
				}
				this.LayoutFrameType.Parent = parent.SerializationObject as LayoutFrameType;
				this.Site.Name = this.LayoutFrameType.ExpandedName;
			}
		}

        /// <summary>
        /// Called after the control has been added to another container.
        /// </summary>
        /// <remarks>Adds the TOPLEFT anchor</remarks>
        protected override void InitLayout()
        {
            base.InitLayout();

            // set ControlFactory
            ISerializableControl serializableParent = this.Parent as ISerializableControl;
            if (serializableParent == null)
                return;

			this.DesignerLoader = serializableParent.DesignerLoader;
            
            if (this.SuspendLayouting)
                return;

            if (this.LayoutFrameType.SizeDimension == null)
                this.LayoutFrameType.SizeDimension = Dimension.FromSize<Dimension.Size>(this.Size);

            LayoutFrameTypeAnchors anchors;
            if (this.LayoutFrameType.AnchorsCollection.Count == 0)
            {
                anchors = new LayoutFrameTypeAnchors();
                this.LayoutFrameType.AnchorsCollection.Add(anchors);
            }
            else
            {
                anchors = this.LayoutFrameType.AnchorsCollection[0];
            }
            if (anchors.Anchor.Count == 0)
            {
                LayoutFrameTypeAnchorsAnchor anchor = new LayoutFrameTypeAnchorsAnchor();
                anchor.point = FRAMEPOINT.TOPLEFT;
                anchor.Offset = new Dimension();
                anchor.Offset.Update(this.Left, this.Top);
                anchors.Anchor.Add(anchor);
                this.DoChangeLayout();
            }
        }

        /// <summary>
        /// Gets or sets the size of the control.
        /// </summary>
        /// <value>The size dimension.</value>
        /// <remarks>
        /// Appears as "Size" in the property grid.
        /// </remarks>
        [Category("Layout")]
        [DisplayName("Size")]
        [XmlIgnore]
        public Dimension.Size SizeDimension
        {
            get { return TypedSerializationObject.SizeDimension; }
            set { TypedSerializationObject.SizeDimension = value; }
        }

        [XmlIgnore]
        [TypeConverter(typeof(InheritsTypeConverter))]
        [Category("Appearance")]
        public string inherits
        {
            get { return TypedSerializationObject.inherits; }
            set { TypedSerializationObject.inherits = value; }
		}
	}
}
