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
using Axiom.Core;

namespace Multiverse.Tools.WorldEditor
{
	class AddPlantTypeCommand : ICommand
	{
		protected WorldEditor app;
		protected Grass parent;
		protected uint instances;
		protected string imageName;
        protected string name;
		protected float scaleWidthLow;
		protected float scaleWidthHi;
		protected float scaleHeightLow;
		protected float scaleHeightHi;
		protected ColorEx colorRGB;
		protected float colorMultLow;
		protected float colorMultHi;
		protected float windMagnitude;
		// protected Axiom.SceneManagers.Multiverse.PlantType sceneType;
		protected bool inScene = false;
		protected bool inTree = false;
		protected WorldTreeNode node = null;
		protected WorldTreeNode parentNode = null;
		protected PlantType plant = null;

		public AddPlantTypeCommand(WorldEditor worldEditorin, Grass parentin, uint instancesin, string namein,
			string imageNamein, float scaleWidthHiin, float scaleWidthLowin, float scaleHeightHiin, float scaleHeightLowin,
			ColorEx colorin, float colorMultHiin, float colorMultLowin, float windMagnitudein)
		{
			this.app = worldEditorin;
			this.parent = parentin;
			this.instances = instancesin;
            this.name = namein;
			this.imageName = imageNamein;
			this.scaleWidthHi = scaleWidthHiin;
			this.scaleWidthLow = scaleWidthLowin;
			this.scaleHeightHi = scaleHeightHiin;
			this.scaleHeightLow = scaleHeightLowin;
			this.colorRGB = colorin;
			this.colorMultHi = colorMultHiin;
			this.colorMultLow = colorMultLowin;
			this.windMagnitude = windMagnitudein;
		}


		#region ICommand Members
		public bool Undoable()
		{
			return true;
		}
		public void Execute()
		{
			if (plant == null)
			{
				plant = new PlantType(app, parent, instances, name, imageName, scaleWidthLow, scaleWidthHi, scaleHeightLow, scaleHeightHi, colorRGB, colorMultLow, colorMultHi, windMagnitude);
			}
			this.parent.Add(plant);
            for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
            {
                app.SelectedObject[i].Node.UnSelect();
            }
            if (plant.Node != null)
            {
                plant.Node.Select();
            }
		}

		public void UnExecute()
		{
			parent.Remove(plant);
		}

		#endregion
	}
}

