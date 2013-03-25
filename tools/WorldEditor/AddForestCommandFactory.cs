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
using System.IO;
using Axiom.MathLib;
using Microsoft.Win32;

namespace Multiverse.Tools.WorldEditor
{
	public class AddForestCommandFactory : ICommandFactory
	{
		WorldEditor app;
		IWorldContainer parent;
		Boundary bound;
		string fileName;
		string windFilename;
		

		public AddForestCommandFactory(WorldEditor worldEditor, IWorldContainer parentObject, Boundary bound)
		{
			app = worldEditor;
			parent = parentObject;
			this.bound = bound;
		}

		public ICommand CreateCommand()
		{
            ICommand ret = null;
			List<AssetDesc> assetList = app.Assets.Select("SpeedWind");
			string title;
			// bool add = false;
            String regSet;

			fileName = app.Config.ForestSpeedWindFileDefault;
			// add = true;
			title = "Add A Forest";
            regSet = (string)Registry.GetValue(app.Config.WorldEditorBaseRegistryKey, "DontShowSpeedTreeLicenseDialog", "False");
            if (String.Equals(regSet, "False"))
            {
                using (SpeedTreeLicenseDialog dlg = new SpeedTreeLicenseDialog())
                {
                    if (dlg.ShowDialog() == DialogResult.Cancel)
                    {
                        return null;
                    }
                    if (dlg.DoNotShowAgain)
                    {
                        Registry.SetValue(app.Config.WorldEditorBaseRegistryKey, "DontShowSpeedTreeLicenseDialog", true);
                    }
                }
            }
			using (ForestDialog dlg = new ForestDialog(assetList, fileName, title))
			{

				dlg.Seed = app.Config.ForestSeedDefault;
				dlg.WindDirection = app.Config.ForestWindDirectionDefault;
				dlg.WindSpeed = app.Config.ForestWindSpeedDefault;


				if (dlg.ShowDialog() == DialogResult.OK)
				{
					bool showAgain = false;
					DialogResult result = DialogResult.OK;
					do
					{
						if (showAgain)
						{
							result = dlg.ShowDialog();
						}
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
						windFilename = app.Assets.assetFromName(dlg.SpeedWindFile).AssetName;

						ret = new AddForestCommand(app, bound, dlg.Seed, dlg.WindSpeed, dlg.WindDirection, windFilename);
					}
				}
				return ret;
			}
		}
	}
}
