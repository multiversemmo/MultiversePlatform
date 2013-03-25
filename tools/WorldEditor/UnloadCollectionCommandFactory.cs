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

namespace Multiverse.Tools.WorldEditor
{
    class UnloadCollectionCommandFactory: ICommandFactory
    {
        protected WorldEditor app;
        protected WorldObjectCollection topCollection;

        public UnloadCollectionCommandFactory(WorldEditor worldEditor, WorldObjectCollection col)
        {
            app = worldEditor;
            topCollection = col;
        }

        #region ICommandFactory Members

        public ICommand CreateCommand()
        {
            if (app.WorldDirty)
            {
                DialogResult dlgRes = MessageBox.Show("You have unsaved changes.  You will have to save the file or cancel", "Save Changes?", MessageBoxButtons.OKCancel , MessageBoxIcon.Information);
                switch (dlgRes)
                {
                    case DialogResult.OK:
                        EventArgs e = new EventArgs();
                        if (app.WorldRoot != null && app.WorldRoot.WorldFilePath != null && !String.Equals(app.WorldRoot.WorldFilePath, ""))
                        {
                            app.SaveWorld(app.WorldRoot.WorldFilePath);
                            app.ResetDirtyWorld();
                        }
                        else
                        {
                            using (SaveFileDialog dlg = new SaveFileDialog())
                            {
                                string filename = "";

                                dlg.Title = "Save World";
                                dlg.DefaultExt = "mvw";
                                if (app.WorldRoot != null && app.WorldRoot.WorldFilePath != null)
                                {
                                    filename = app.WorldRoot.WorldFilePath;
                                    dlg.FileName = app.WorldRoot.WorldFilePath;
                                    foreach (WorldObjectCollection obj in app.WorldRoot.WorldObjectCollections)
                                    {
                                        obj.Filename = "";
                                    }
                                }
                                else
                                {
                                    if (app.WorldRoot == null)
                                    {
                                        return null;
                                    }
                                }
                                dlg.Filter = "Multiverse World files (*.mvw)|*.mvw|xml files (*.xml)|*.xml|All files (*.*)|*.*";
                                dlg.RestoreDirectory = true;
                                if (dlg.ShowDialog() == DialogResult.OK)
                                {
                                    app.WorldRoot.WorldFilePath = dlg.FileName;
                                    string title = String.Format("World Editor : {0}", dlg.FileName.Substring(dlg.FileName.LastIndexOf("\\") + 1));
                                    app.SaveWorld(dlg.FileName);
                                    app.ResetDirtyWorld();
                                }
                            }
                        }
                        break;
                    case DialogResult.Cancel:
                        return null;
                }
            }
            ICommand cmd = new UnloadCollectionCommand(app, topCollection);
            return cmd;
        }

        #endregion
    }
}
