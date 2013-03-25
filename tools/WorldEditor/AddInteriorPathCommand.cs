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

namespace Multiverse.Tools.WorldEditor
{
    public class AddInteriorPathCommand : ICommand
    {
        #region ICommand Members

        private WorldEditor app;
        private IWorldContainer parent;
        private String meshName;
        private bool placing;
        private Vector3 location;
        private bool cancelled = false;

        private InteriorPath path;

        public AddInteriorPathCommand(WorldEditor worldEditor, IWorldContainer parentObject)
        {
            this.app = worldEditor;
            this.parent = parentObject;
            this.meshName = app.Config.RegionPointMeshName;

            placing = true;
        }

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
			if (path == null)
			{
				path = new InteriorPath(parent, app, meshName);
				path.Highlight = true;
			}
            parent.Add(path);

			if (placing)
			{
				new MultiPointPlacementHelper(app, null, new MultiPointValidate(PointValidate), 
											  new MultiPointComplete(PointPlacementComplete), DragPickMode.PickEntityTriangle);
				placing = false;
			}
        }

        public void UnExecute()
        {
			parent.Remove(path);
        }

        #endregion

        protected void PointPlacementComplete(List<Vector3> points)
        {
			((StaticObject)parent).PathsPerInstance = true;
			path.DisplayRods(false);
        }

        protected bool PointValidate(List<Vector3> points, Vector3 location)
        {
            if (!IntersectionHelperClass.BoundaryIntersectionSearch(points, location, points.Count))
            {

                AddPointCommand addCmd = new AddPointCommand(path.Points, path.TransformLocation(location));
                app.ExecuteCommand(addCmd);
				path.DisplayRods(true);
			}
            else
            {
                ErrorHelper.SendUserError("Add region point failed","Region", app.Config.ErrorDisplayTimeDefault, true, this, app);
            }
            return true;
        }
    }
}
