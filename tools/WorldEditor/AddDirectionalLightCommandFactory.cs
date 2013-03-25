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
using Axiom.Core;

namespace Multiverse.Tools.WorldEditor
{
    class AddDirectionalLightCommandFactory : ICommandFactory
    {
		WorldEditor app;
        Boundary parent;


		public AddDirectionalLightCommandFactory(WorldEditor worldEditor, IWorldContainer parentObject)
		{
			this.app = worldEditor;
            this.parent = (Boundary)parentObject;
        }

        #region ICommandFactory Members
        
        public ICommand CreateCommand()
		{
			ICommand ret = null;

			using (AddDirectionalLightDialog dlg = new AddDirectionalLightDialog(app.Config.DefaultBoundaryDirectionalLightSpecular, app.Config.DefaultBoundaryDirectionalLightDiffuse))
			{
				DialogResult result;

                result = dlg.ShowDialog();
				if (result == DialogResult.OK)
				{
                    ret = new AddDirectionalLightCommand(app, (IWorldContainer)parent, String.Format("{0}-{1}",parent.Name, "DirectionalLight"),app.Assets.assetFromName(app.Config.PointLightMeshName).AssetName, dlg.Specular, dlg.Diffuse);
				}
				return ret;
			}
		}
        #endregion
    }
}
