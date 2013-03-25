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
using Axiom.MathLib;

namespace Multiverse.Tools.WorldEditor
{
	public class AddSoundCommandFactory : ICommandFactory
	{
		protected WorldEditor app;
        protected IWorldContainer parent;
		protected string filename = "";
		protected Sound sound;
		protected string anchor = "Sound";


		public AddSoundCommandFactory(WorldEditor worldEditor, IWorldContainer parentObject)
		{
			this.app = worldEditor;
			this.parent = parentObject;
		}

        public AddSoundCommandFactory(WorldEditor worldEditor, IWorldContainer parentObject, string filename)
        {
            this.app = worldEditor;
            this.parent = parentObject;
            this.filename = filename;
        }

		public ICommand CreateCommand()
		{
			List<AssetDesc> assets = app.Assets.Select("Sound");
            ICommand ret = null;
			// bool add = false;

			// add = true;
			string title = "Add Sound";
            if (String.Equals(filename, ""))
            {
                using (comboBoxPrompt dlg = new comboBoxPrompt(assets, this.filename, title, anchor, "&Add"))
                {

                    DialogResult result = dlg.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        this.filename = (app.Assets.assetFromName(dlg.ComboBoxSelectedItemAsString)).AssetName;
                    }
                    if (result == DialogResult.Cancel)
                    {
                        return null;
                    }
                }
            }
            ret = new AddSoundCommand(app, parent, filename);
			return ret;
		}
	}
}

