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
using System.Drawing;
using System.Collections.Generic;
using System.Text;

using Axiom.Core;

using Multiverse.Gui;

//using CrayzEdsGui.Base;
//using CrayzEdsGui.Base.Widgets;
//using CrayzEdsGui.Renderers.AxiomEngine;
//using Vector3 = CrayzEdsGui.Base.Vector3;
//using EditBox = CrayzEdsGui.Base.Widgets.EditBox;
#endregion

namespace Multiverse.Base
{
    //public class TextWindow
    //{
    //    // constants for the names of the widget types
    //    const string EditBoxType = "WindowsLook.WLEditBox";

    //    public EditBox editBox;
    //    public TextWindow(Size minSize, Size maxSize)
    //    {
    //        editBox = (EditBox)WindowManager.Instance.CreateWindow(EditBoxType, "Window/EditBox");
    //        editBox.MetricsMode = MetricsMode.Absolute;
    //        editBox.Position = new Point(maxSize.width - 350, maxSize.height - 212);
    //        editBox.Size = new Size(300, 20);
    //        editBox.Alpha = .7f;
    //        editBox.MinimumSize = minSize;
    //        editBox.MaximumSize = maxSize;
    //    }
    //}

	public class LoadingScreen
	{
		public ImageWindow loadWindow;

		public LoadingScreen(SizeF size, SizeF maxSize)
		{
			loadWindow = new ImageWindow("Window/LoadWindow");
			loadWindow.Position = new PointF(0, 0);
			loadWindow.MaximumSize = maxSize;
			loadWindow.Colors = new ColorRect(ColorEx.Black);
			loadWindow.Size = size;

			Texture texture = TextureManager.Instance.Load("loadscreen.dds");
            TextureAtlas atlas = new TextureAtlas("_glue", texture);
            atlas.DefineImage("LoadImage1", new PointF(0, 0), new SizeF(1024, 768));

			loadWindow.VerticalFormat = VerticalImageFormat.Stretched;
			loadWindow.HorizontalFormat = HorizontalImageFormat.Stretched;
            loadWindow.SetImage(atlas.GetTextureInfo("LoadImage1"));
			loadWindow.Visible = true;
		}
	}
}
