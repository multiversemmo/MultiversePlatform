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
using System.ComponentModel;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.IO;
using System.Drawing;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	[ToolboxItem(false)]
    public class Ui : UserControl, ISerializableControl, ICustomTypeDescriptor
    {
        private string backgroundImagePath;

        public Ui()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.Dock = DockStyle.Fill;
        }

        #region ISerializableControl Members

        /// <summary>
        /// Hosted Serialization object
        /// </summary>
        private Serialization.Ui uiObject = new Serialization.Ui();

        /// <summary>
        /// Gets or sets the serialization object.
        /// </summary>
        /// <value>The serialization object.</value>
        [Browsable(false)]
        public Serialization.SerializationObject SerializationObject
        {
            get
            {
                return uiObject;
            }
            set
            {
                uiObject = (Serialization.Ui) value;
            }
        }

		[Browsable(false)]
		public Serialization.Ui TypedSerializationObject
		{
			get { return uiObject; }
		}

		[Browsable(false)]
		public FrameXmlDesignerLoader DesignerLoader { get; set; }

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

		/// <summary>
		/// Repositions all child controls of the Ui control.
		/// </summary>
		/// <param name="ui">The Ui control.</param>
		public void RepositionUi()
		{
			foreach (BaseControl childControl in this.Controls.OfType<BaseControl>())
			{
				childControl.ChangeLayout();
			}
		}

		[Browsable(false)]
		public IEnumerable<BaseControl> BaseControls
		{
			get
			{
				return this.Controls.OfType<BaseControl>();
			}
		}

		[Browsable(false)]
		public IEnumerable<BaseControl> SortedControls
		{
			get
			{
				return this.BaseControls.SortControls();
			}
		}

		[Browsable(false)]
        public string BackgroundImagePath
        {
            get
            {
                return backgroundImagePath;
            }
            set
            {
                backgroundImagePath = value;
                this.InitializeBackground();
            }
        }

        private void InitializeBackground()
        {
            if (File.Exists(backgroundImagePath))
            {
                try
                {
                    this.BackgroundImage = Image.FromFile(backgroundImagePath);
                    this.BackgroundImageLayout = ImageLayout.None;
                }
                catch (FileNotFoundException)
                {
                }
                catch (OutOfMemoryException)
                {
                }
            }
        }

		protected override void OnControlAdded(ControlEventArgs e)
		{
			if (!(e.Control is BaseControl))
				throw new ArgumentException("Only World of Warcraft controls can be hosted!");

			base.OnControlAdded(e);
		}

		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			this.DesignerLoader.OnPropertyChanged(this, e);
		}

		protected override void OnControlRemoved(ControlEventArgs e)
		{
			this.DesignerLoader.NotifyControlRemoval(e.Control);

			base.OnControlRemoved(e);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			if (DesignerLoader != null && DesignerLoader.RootControl != null && !DesignerLoader.IsLoading)
			{
				DesignerLoader.RootControl.RepositionUi();
			}
		}
	}
}
