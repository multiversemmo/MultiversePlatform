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
using Axiom.Core;

namespace Multiverse.Tools.WorldEditor
{
	public class AddPlantTypeCommandFactory : ICommandFactory
	{
		protected WorldEditor app;
		protected Grass parent;
		protected Axiom.SceneManagers.Multiverse.PlantType sceneType;

		public AddPlantTypeCommandFactory(WorldEditor worldEditorin, Grass parentin)
		{
			this.app = worldEditorin;
			this.parent = parentin;
		}

		public ICommand CreateCommand()
		{
			ICommand ret = null;
			List<AssetDesc> assetList = app.Assets.Select("Vegitation");
			string title;
			// bool add = false; (unused)

			// add = true;
			title = "Add A PlantType";

			using (AddPlantTypeDialog dlg = new AddPlantTypeDialog(title, assetList, app))
			{
				dlg.Instances = app.Config.InstancesDefault;
				dlg.MinimumWidthScale = app.Config.ScaleWidthLowDefault;
				dlg.MaximumWidthScale = app.Config.ScaleWidthHiDefault;
				dlg.MinimumHeightScale = app.Config.ScaleHeightLowDefault;
				dlg.MaximumHeightScale = app.Config.ScaleHeightHiDefault;
				dlg.ColorMultLow = app.Config.ColorMultLowDefault;
				dlg.ColorMultHi = app.Config.ColorMultHiDefault;
                dlg.WindMagnitude = app.Config.WindMagnitudeDefault;

				bool showAgain = false;
				DialogResult result;
				do {
					result = dlg.ShowDialog();
					showAgain = false;
					if ( result == DialogResult.OK) {
						// do validation here
						// if validation fails, set showAgain to true
						showAgain = ((result == DialogResult.OK) && (!dlg.okButton_validating()));
					}

				} while (showAgain);
				if (result == DialogResult.OK)
				{
                    ret = new AddPlantTypeCommand(app, parent, dlg.Instances, dlg.ComboBoxSelected, app.Assets.assetFromName(dlg.ComboBoxSelected).AssetName, dlg.MaximumWidthScale, dlg.MinimumWidthScale, dlg.MaximumHeightScale, dlg.MinimumHeightScale, dlg.ColorRGB, dlg.ColorMultHi, dlg.ColorMultLow, dlg.WindMagnitude);
				}
			}
			return ret;
		}
	}
}
