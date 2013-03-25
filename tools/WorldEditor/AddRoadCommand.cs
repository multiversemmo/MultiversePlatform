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
    public class AddRoadCommand : ICommand
    {
        #region ICommand Members

        private WorldEditor app;
        private IWorldContainer parent;
        private String name;
        private String meshName;
        private bool placing;
        // private Vector3 location;
        private DisplayObject dragMarker;
        // private bool cancelled = false;
        private int halfWidth;

        private RoadObject roadObject;

        public AddRoadCommand(WorldEditor worldEditor, IWorldContainer parentObject, String roadName, int halfWidth)
        {
            this.app = worldEditor;
            this.parent = parentObject;
            this.name = roadName;
            this.meshName = app.Config.RoadPointMeshName;
            this.halfWidth = halfWidth;

            placing = true;
        }

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            if (roadObject == null)
            {
                roadObject = new RoadObject(name, parent, app, halfWidth);
            }
            parent.Add(roadObject);
            for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
            {
                app.SelectedObject[i].Node.UnSelect();
            }
            if (roadObject.Node != null)
            {
                roadObject.Node.Select();
            }

            if (placing)
            {
                dragMarker = new DisplayObject("RoadDrag", app, "Dragging", app.Scene, app.Config.RoadPointMeshName, Vector3.Zero, Vector3.UnitScale, Vector3.Zero, null);
                dragMarker.MaterialName = app.Config.RoadPointMaterial;
                dragMarker.ScaleWithCameraDistance = true;

                new MultiPointPlacementHelper(app, dragMarker, new MultiPointValidate(PointValidate), new MultiPointComplete(PointPlacementComplete));

                placing = false;
            }



        }

        public void UnExecute()
        {
            parent.Remove(roadObject);
        }

        #endregion

        protected void PointPlacementComplete(List<Vector3> points)
        {
            dragMarker.Dispose();
        }

        protected bool PointValidate(List<Vector3> points, Vector3 location)
        {
            AddPointCommand addCmd = new AddPointCommand(roadObject.Points, location);
            app.ExecuteCommand(addCmd);
            
            return true;
        }
    }
}
