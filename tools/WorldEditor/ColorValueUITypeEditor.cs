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
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Collections.Specialized;
using Axiom.Core;

namespace Multiverse.Tools.WorldEditor
{
	public class ColorValueUITypeEditor : UITypeEditor
	{
		public string type;

		private Color ColorExToColor(ColorEx cx)
		{
			return Color.FromArgb((int)cx.ToARGB());
		}

		private ColorEx ColorToColorEx(Color c)
		{
			return new ColorEx(c.A / 255f, c.R / 255f, c.G / 255f, c.B / 255f);
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			ColorEx color = (ColorEx) value;
			using (ColorDialog colorDialog = new ColorDialog())
			{
				colorDialog.AllowFullOpen = true;
				colorDialog.AnyColor = true;
				colorDialog.FullOpen = true;
				colorDialog.ShowHelp = true;
				colorDialog.Color = ColorExToColor(color);
                int colorabgr = (int)color.ToABGR();
				colorabgr &= 0x00ffffff;
				int[] colorsabgr = new int[1];
				colorsabgr[0] = colorabgr;
				colorDialog.CustomColors = colorsabgr;
				DialogResult result = colorDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					color = ColorToColorEx(colorDialog.Color);
				}
				colorDialog.Color = ColorExToColor(color);
			}
			return color;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);
		}

		public class ColorValueUITypeEditorFog : ColorValueUITypeEditor
		{
			public ColorValueUITypeEditorFog()
			{
				type = "Fog";
			}
		}

		public class ColorValueUITypeEditorPlantType : ColorValueUITypeEditor
		{
			public ColorValueUITypeEditorPlantType()
			{
				type = "Plant";
			}
		}

        public class ColorValueUITypeEditorGlobalDirectionalLight : ColorValueUITypeEditor
        {
            public ColorValueUITypeEditorGlobalDirectionalLight()
            {
                type = "GlobalDirectionalLight";
            }
        }

        public class ColorValueUITypeEditorGlobalFog : ColorValueUITypeEditor
        {
            public ColorValueUITypeEditorGlobalFog()
            {
                type = "GlobalFog";
            }
        }

        public class ColorValueUITypeEditorPointLight : ColorValueUITypeEditor
        {
            public ColorValueUITypeEditorPointLight()
            {
                type = "PointLight";
            }
        }
	}
}
