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
using System.ComponentModel;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.Windows.Forms;
using System.Drawing;
using System.Xml.Serialization;
using System.Reflection;
using System.Drawing.Design;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
    /// <summary>
    /// Base class for all controls that are frames.
    /// </summary>
    /// <typeparam name="TS">The type of the Serialization Object.</typeparam>
    [Designer(typeof(FrameDesigner))]
    public class GenericFrameControl<TS> : GenericControl<TS>, IFrameControl
        where TS : LayoutFrameType, new()
    {
        private FrameCollection frames;

		private void Initialize()
		{
			frames = new FrameCollection(this);
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="GenericFrameControl&lt;TS&gt;"/> class.
        /// </summary>
        public GenericFrameControl()
        {
			Initialize();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericFrameControl&lt;TS&gt;"/> class.
        /// </summary>
        /// <param name="control">The hosted inner control.</param>
        public GenericFrameControl(Control control) : base(control)
        {
			Initialize();
        }

        /// <summary>
        /// Gets the Frames collection (the child controls that are frames)
        /// </summary>
        /// <value>The frames.</value>
        [Browsable(false)]
		[XmlIgnore]
        public FrameCollection Frames
        {
            get { return frames; }
        }

		[Browsable(false)]
		[XmlIgnore]
		public IEnumerable<ILayerable> Layerables
		{
			get 
			{
				var layerables = this.Controls.OfType<ILayerable>().Where<ILayerable>(layerable => !layerable.Inherited).SortControls(); ;
				return layerables.SortControls();
			}
		}

		#region Events PropertyDescriptors

		private static PropertyDescriptor[] eventDescriptors;

		private static object lockForEventDescriptors = new object();

		public PropertyDescriptor[] GetEventDescriptors()
		{
			const string eventsPropertyName = "EventsArray";

			lock (GenericFrameControl<TS>.lockForEventDescriptors)
			{
				if (GenericFrameControl<TS>.eventDescriptors == null)
				{
					IList<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();

					Type scriptsType = typeof(ScriptsType);
					PropertyInfo eventProperty = scriptsType.GetProperty(eventsPropertyName);
					if (eventProperty == null)
					{
						throw new ApplicationException("ScriptsType must have 'EventsArray' property.");
					}

					XmlElementAttribute[] xmlAttributes =
						(XmlElementAttribute[])eventProperty.GetCustomAttributes(typeof(XmlElementAttribute), true);
					foreach (XmlElementAttribute xmlAttribute in xmlAttributes)
					{
						descriptors.Add(new EventPropertyDescriptor(xmlAttribute.ElementName));
					}

					GenericFrameControl<TS>.eventDescriptors = descriptors.ToArray();

				}
			}

            return GenericFrameControl<TS>.eventDescriptors;

		}

		#endregion

		protected override void OnControlAdded(ControlEventArgs e)
		{
			if (e.Control != this.InnerControl && !(e.Control is BaseControl))
				throw new ArgumentException("Only World of Warcraft controls can be hosted!");

			base.OnControlAdded(e);
		}
	}
}
