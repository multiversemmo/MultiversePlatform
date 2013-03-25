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
using System.Windows.Forms;
using System.Text;
using Axiom.Core;

namespace Multiverse.Tools.WorldEditor
{
    class AddAmbientLightCommandFactory : ICommandFactory
    {
		WorldEditor app;
		Boundary parent;
		// ColorEx cx;


		public AddAmbientLightCommandFactory(WorldEditor worldEditor, Boundary bound)
		{
			this.app = worldEditor;
			this.parent = bound;
		}

		public ICommand CreateCommand()
		{
			ICommand ret = null;
			// bool add = false;

			// add = true;
			// string title = "Add An Ambient Light";

			using (AddAmbientLightDialog dlg = new AddAmbientLightDialog())
			{
				dlg.Cx = app.Config.DefaultAmbientLightColor;
				DialogResult result;
				result = dlg.ShowDialog();
				if (result == DialogResult.OK)
				{
					ret = new AddAmbientLightCommand(app, parent, dlg.Cx);
				}
				return ret;
			}
		}
    }
}
