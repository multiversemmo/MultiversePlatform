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
using System.Drawing;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	/// <summary>
	/// Collects anchor information of the controls.
	/// </summary>
    public class ControlAnchors
    {
		public class SideAnchor
		{
			public LayoutFrameTypeAnchorsAnchor Anchor { get; set; }
			public Point Offset { get; set; }
			public bool Inherited { get; set; }

			public void SetX(int x, Control parent)
			{
				Point anchorPoint = Anchor.GetRelativePoint(parent);
				x -= anchorPoint.X;
				if (Anchor.Offset == null)
					Anchor.Offset = new Dimension();

				Anchor.Offset.Update(x, null);
				Offset = new Point(x, Offset.Y);
			}

			public void SetY(int y, Control parent)
			{
				Point anchorPoint = Anchor.GetRelativePoint(parent);
				y = anchorPoint.Y - y;
				if (Anchor.Offset == null)
					Anchor.Offset = new Dimension();

				Anchor.Offset.Update(null, y);
				Offset = new Point(Offset.X, y);
			}
		}

		private static void SetOnce(ref SideAnchor leftValue, SideAnchor value)
		{
			if (leftValue == null)
				leftValue = value;
		}

		private SideAnchor left = null;
		public SideAnchor Left
		{
			get { return left; }
			set { SetOnce(ref left, value); }
		}

		private SideAnchor top = null;
		public SideAnchor Top
		{
			get { return top; }
			set { SetOnce(ref top, value); }
		}

		private SideAnchor right;
        public SideAnchor Right 
		{
			get { return this.right; }
			set { SetOnce(ref right, value); }
		}

		private SideAnchor bottom;
        public SideAnchor Bottom 
		{
			get { return this.bottom; }
			set { SetOnce(ref bottom, value); }
		}

		private SideAnchor center;
		public SideAnchor Center
		{
			get { return this.center; }
			set { SetOnce(ref center, value); }
		}
	}
}
