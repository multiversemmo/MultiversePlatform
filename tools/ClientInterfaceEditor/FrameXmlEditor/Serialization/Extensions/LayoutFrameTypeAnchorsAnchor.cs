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
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Controls;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	public partial class LayoutFrameTypeAnchorsAnchor
	{
		[Category("Layout")]
		[DisplayName("relativePoint")]
		[XmlIgnore]
		public FRAMEPOINT? RelativePoint
		{
			get
			{
				return relativePointSpecified ?
					(FRAMEPOINT?)relativePoint : null;
			}
			set
			{
				relativePointSpecified = value.HasValue;
				if (value.HasValue)
					relativePoint = value.Value;
			}
		}

		public override string ToString()
		{
			string text = this.point.ToString();
			if (this.Offset != null)
				text += ' ' + this.Offset.ToString();

			if (!String.IsNullOrEmpty(this.relativeTo))
				text += String.Format(" ({0})", this.relativeTo);

			return text;
		}

		public Point GetRelativePoint(Control parent)
		{
			// returns parent if relativeTo doesn't point to an existing control
			// WoW will not show such controls.
			// TODO: verify exception handling

			ISerializableControl baseParent = parent as ISerializableControl;

			Control relativeControl = baseParent != null ?
				baseParent.DesignerLoader.BaseControls[this.relativeTo, parent] :
				null;

			if (relativeControl == null)
				relativeControl = parent;

			FRAMEPOINT relativePoint = this.relativePointSpecified ?
				this.relativePoint :
				this.point;

			Point anchorPoint = Point.Empty;
			string relativePointText = relativePoint.ToStringValue();

			if (relativePointText.StartsWith("BOTTOM"))
				anchorPoint.Y = relativeControl.Height;
			if (relativePointText.StartsWith("CENTER"))
				anchorPoint.Y = relativeControl.Height / 2;

			if (relativePointText.EndsWith("RIGHT"))
				anchorPoint.X = relativeControl.Width;
			if (relativePointText.EndsWith("CENTER"))
				anchorPoint.X = relativeControl.Width / 2;

			if (relativeControl != parent)
			{
				anchorPoint = relativeControl.PointToScreen(anchorPoint);
				anchorPoint = parent.PointToClient(anchorPoint);
			}
			return anchorPoint;
		}
	}
}
