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
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	[ToolboxBitmap(typeof(System.Windows.Forms.PictureBox), "PictureBox.bmp")]
    [ToolboxItemFilter("MultiverseInterfaceStudioFilter", ToolboxItemFilterType.Require)]
    public class Texture : GenericControl<TextureType>, ILayerable
	{
		public Texture()
		{
            this.BackColor = Color.Transparent;
            this.HasBorder = true;
            this.LayerLevel = DRAWLAYER.ARTWORK;
		}

		[Category("Appearance")]
		[TypeConverter(typeof(ImageNameTypeConverter))]
		public string ImageFileName
		{
			get
			{
				string file = TypedSerializationObject.file;
                if (String.IsNullOrEmpty(file))
                    return null;

				string[] elements = file.Split('\\');
				if (elements.Length > 0)
					return elements[elements.Length - 1];
				return null;
			}
			set
			{
				string file = String.Format(@"Interface\{0}\{1}",
					Path.GetFileName(Path.GetDirectoryName(DesignerLoader.DocumentMoniker)),
					Path.GetFileNameWithoutExtension(value));

				TypedSerializationObject.file = file;
			}
		}

        public override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case "file":
                case "ImageFileName":
                    this.BackgroundImage = TargaImage.LookupFile(this, TypedSerializationObject.file);
                    break;
            }
        }

		protected override void OnUpdateControl()
		{
			base.OnUpdateControl();
			
			this.BackgroundImage = TargaImage.LookupFile(this, TypedSerializationObject.file);
		}

        protected override System.Drawing.Size DefaultSize
        {
            get
            {
                return new System.Drawing.Size(128,128);
            }
        }

		#region ILayerable Members

        [Category("Layout")]
        public DRAWLAYER LayerLevel { get; set; }

		#endregion


	}
}
