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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Text;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using System.Diagnostics;
using System.Windows.Forms;
using Multiverse.AssetRepository;

namespace Multiverse.Tools.WorldEditor
{
	public class SaveModelInteriorPathsCommandFactory : ICommandFactory
    {
        WorldEditor app;
        IWorldContainer parent;
		string meshName;

        public SaveModelInteriorPathsCommandFactory(WorldEditor worldEditor, IWorldContainer parentObject, string meshName)
        {
            app = worldEditor;
            parent = parentObject;
			this.meshName = meshName;
        }

        #region ICommandFactory Members

        public ICommand CreateCommand()
        {
            ICommand cmd = new SaveModelInteriorPathsCommand(app, parent, meshName);

            return cmd;
        }

		

        #endregion
    }

	public class SaveModelInteriorPathsCommand : ICommand
	{
        private WorldEditor app;
        private IWorldContainer parent;
		private string meshName;
		private System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog();

		public SaveModelInteriorPathsCommand(WorldEditor worldEditor, IWorldContainer parentObject, string meshName)
        {
            this.app = worldEditor;
            this.parent = parentObject;
			this.meshName = meshName;
            this.saveDialog.DefaultExt = "modelpaths";
            this.saveDialog.Filter = "Model Interior Path Files (*.modelpaths)|*.modelpaths";
            this.saveDialog.Title = "Save Model Interior Paths";
        }

        public bool Undoable()
        {
            return false;
        }

        public void Execute()
        {
			saveDialog.FileName = RepositoryClass.Instance.RepositoryPath + "\\Physics\\" + 
				Path.GetFileNameWithoutExtension(meshName) + ".modelpaths";
			if (saveDialog.ShowDialog() == DialogResult.OK) {
				StaticObject so = (StaticObject)parent;
				InteriorPathSet pathset = so.WriteModelInteriorPaths(app, saveDialog.FileName);
				so.PathsPerInstance = false;
				foreach (WorldObjectCollection collection in app.WorldRoot.WorldObjectCollections)
				{
					foreach (IWorldObject obj in collection)
					{
						if (obj is StaticObject)
						{
							StaticObject aso = (StaticObject)obj;
							if (aso != so && !aso.PathsPerInstance && aso.MeshName == so.MeshName)
								aso.AddChildPaths(pathset);
						}
					}
				}
			}
		}
		
        public void UnExecute()
        {
        }
	}
}
