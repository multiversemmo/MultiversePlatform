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
using Axiom.MathLib;
using System.Windows.Forms;

namespace Multiverse.Tools.WorldEditor
{
    public class AddTerrainDecalCommandFactory : ICommandFactory
    {
        WorldEditor app;
        IWorldContainer parent;

        public AddTerrainDecalCommandFactory(WorldEditor worldEditor, IWorldContainer parent)
        {
            this.app = worldEditor;
            this.parent = parent;

        }

        public ICommand CreateCommand()
        {

            using (AddTerrainDecalDialog dlg = new AddTerrainDecalDialog())
            {
                bool showAgain = false;
                ICommand ret = null;
                DialogResult result;
                do
                {
                    result = dlg.ShowDialog();
                    showAgain = false;
                    if (result == DialogResult.OK)
                    {
                        // do validation here
                        // if validation fails, set showAgain to true
                        showAgain = ((result == DialogResult.OK) && (!dlg.okButton_validating()));
                    }
                } while (showAgain);
                if (result == DialogResult.OK)
                {
                    Vector2 size = new Vector2(dlg.SizeX,dlg.SizeZ);
                    ret = new AddTerrainDecalCommand(app, parent, dlg.ObjectName, dlg.Filename, size, dlg.Priority);
                }
                return ret;
            }
        }
    }
}
