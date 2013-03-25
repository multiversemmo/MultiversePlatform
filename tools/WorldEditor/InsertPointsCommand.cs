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
    public class InsertPointsCommand : ICommand
    {
        WorldEditor app;
        PointCollection parent;
        Vector3 newPoint;
        int index;
        MPPoint pt = null;
        private bool placing;
        // private bool canceled = false;
        private DisplayObject dragObject;


        public InsertPointsCommand(WorldEditor worldEditor, IWorldContainer parent, Vector3 newPoint, int index)
        {
            this.app = worldEditor;
            this.parent = (PointCollection) parent;
            this.newPoint = newPoint;
            this.index = index;
            this.placing = true;
        }
        
        #region ICommand Members
		public bool Undoable()
		{
			return true;
		}

		public void Execute()
		{

            if (placing)
            {
                switch (((IObjectInsert)(parent.Parent)).ObjectType)
                {
                    case "Road":
                        dragObject = new DisplayObject("RoadDrag", app, "Dragging", app.Scene, app.Config.RoadPointMeshName, Vector3.Zero, Vector3.UnitScale, Vector3.Zero, null);
                        dragObject.MaterialName = app.Config.RoadPointMaterial;
                        break;
                    case "Region":
                        dragObject = new DisplayObject("RegionDrag", app, "Dragging", app.Scene, app.Config.RegionPointMeshName, Vector3.Zero, Vector3.UnitScale, Vector3.Zero, null);
                        dragObject.MaterialName = app.Config.RegionPointMaterial;
                        break;
                }
                new MultiPointInsertHelper(app, dragObject, new MultiPointInsertValidate(PointValidate), new MultiPointInsertComplete(PointPlacementComplete), parent.VectorList, index);


                placing = false;
            }
		}

		public void UnExecute()
		{
			parent.Remove(pt);
		}

        protected void PointPlacementComplete(List<Vector3> list)
        {
            dragObject.Dispose();
        }

        protected bool PointValidate(List<Vector3> points, Vector3 location, int index)
        {
            if (parent.NoIntersect)
            {
                if (!IntersectionHelperClass.BoundaryIntersectionSearch(points, location, index + 1))
                {
                    parent.Insert(index, new MPPoint(index + 1, parent, app, dragObject.MeshName, dragObject.MaterialName, location, MPPointType.Boundary));
                    return true;
                }
                else
                {
                    ErrorHelper.SendUserError("Add region point failed", "Region", app.Config.ErrorDisplayTimeDefault, true, this, app);
                    return false;
                }
            }
            else
            {
                parent.Insert(index, new MPPoint(index + 1, parent, app, dragObject.MeshName, dragObject.MaterialName, location, MPPointType.Road));
                return true;
            }
        }

		#endregion
    }
}
