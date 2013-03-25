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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.ComponentModel.Design.Serialization;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.Drawing.Imaging;
using System.Xml.Serialization;
using System.IO;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	/// <summary>
	/// Represents the WoW AddOn element "Frame".
	/// </summary>
	[ToolboxBitmap(typeof(System.Windows.Forms.Panel), "Panel.bmp")]
    [ToolboxItemFilter("MultiverseInterfaceStudioFilter", ToolboxItemFilterType.Require)]
	public partial class Frame : GenericFrameControl<FrameType>
	{
		private static Bitmap[] borderBitmaps = new Bitmap[8];
        private static Brush backgroundBrush = new SolidBrush(Color.FromArgb(192, 68, 68, 68));
		private static BackdropType defaultBackdrop;

		static Frame()
		{
			for (int i = 0; i < 8; i++)
			{
				borderBitmaps[i] = new Bitmap(typeof(Frame), String.Format("Resources.UI-DialogBox-Border-{0}.png", i));
			}

			using (Stream stream = typeof(Frame).Assembly.GetManifestResourceStream(typeof(Frame), "Frame.Backdrop.xml"))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(Serialization.Ui));
				try
				{
					Serialization.Ui ui = (Serialization.Ui) serializer.Deserialize(stream);
					defaultBackdrop = ui.Controls[0].Items[0] as BackdropType;
				}
				catch (Exception ex)
				{
					Trace.WriteLine(ex);
				}
			}
		}

		public Frame()
		{
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(12);
            this.MinimumSize = new Size(64, 64);
			this.HasActions = true;

			if (defaultBackdrop != null)
				this.TypedSerializationObject.Items.Add(defaultBackdrop);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

            e.Graphics.FillRectangle(backgroundBrush, 11, 12, this.ClientRectangle.Width - 23, this.ClientRectangle.Height - 23);

			// sides
			e.Graphics.DrawImageTiled(borderBitmaps[0], 0, 32, 32, this.ClientRectangle.Height - 64);
			e.Graphics.DrawImageTiled(borderBitmaps[1], this.ClientRectangle.Width - 32, 32, 32, this.ClientRectangle.Height - 64);
			e.Graphics.DrawImageTiled(borderBitmaps[2], 32, 0, this.ClientRectangle.Width - 64, 32);
			e.Graphics.DrawImageTiled(borderBitmaps[3], 32, this.ClientRectangle.Height - 32, this.ClientRectangle.Width - 64, 32);

			// corners
			e.Graphics.DrawImage(borderBitmaps[4], 0, 0);
			e.Graphics.DrawImage(borderBitmaps[5], this.ClientRectangle.Width - 32, 0);
			e.Graphics.DrawImage(borderBitmaps[6], 0, this.ClientRectangle.Height - 32);
			e.Graphics.DrawImage(borderBitmaps[7], this.ClientRectangle.Width - 32, this.ClientRectangle.Height - 32);
		}

		protected override Size DefaultSize
		{
			get { return new Size(256, 256); }
		}

		public override EventChoice? DefaultEventChoice
		{
			get
			{
				return EventChoice.OnLoad;
			}
		}
	}

	public static class GraphicsHelper
	{
		public static void DrawImageTiled(this Graphics g, Image image, int left, int top, int width, int height)
		{
			Region originalClipRegion = g.Clip;
			g.Clip = new Region(new Rectangle(left, top, width, height));
			for (int x = left; x < left + width; x += image.Width)
			{
				for (int y = top; y < top + height; y += image.Height)
				{
					g.DrawImageUnscaled(image, x, y, width - x, height - y);
				}
			}
			g.Clip = originalClipRegion;
		}
	}
}
