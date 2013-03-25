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
    public class DragMPPointCommand : ICommand
    {
        protected WorldEditor app;
        protected MPPoint obj;
        protected Vector3 origPosition;
        protected Vector3 placedPosition;
        protected MPPointType type;
        protected bool placing = true;
        protected int pointNum;

        public DragMPPointCommand(WorldEditor worldEditor, MPPoint point)
        {
            this.app = worldEditor;
            this.obj = point;
            this.origPosition = point.Position;
            this.type = point.Type;
            this.pointNum = point.PointNum;
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
                new DragHelper(app, obj.Display, DragCallback, true);
            }
            else
            {
                obj.Position = placedPosition;
            }
        }


        public void UnExecute()
        {
            obj.Position = origPosition;
        }

        #endregion ICommand Members

        public bool DragCallback(bool accept, Vector3 loc)
        {
            placing = false;
            placedPosition = loc;
            Vector3 location = loc;
            if (accept)
            {
                if (type == MPPointType.Boundary)
                {
                    if ((((obj).Parent) as PointCollection).NoIntersect)
                    {
                        PointCollection pc = obj.Parent as PointCollection;
                        int i = 0;
                        List<Vector3> list = new List<Vector3>();
                        foreach (Vector3 point in pc.VectorList)
                        {
                            if (i == obj.PointNum)
                            {
                                list.Add(location);
                            }
                            else
                            {
                                list.Add(point);
                            }
                            i++;
                        }
                        if (IntersectionHelperClass.BoundaryIntersectionSearch(obj.PointNum, list))
                        {
                            ErrorHelper.SendUserError("Unable to move point to that Position", "Region", app.Config.ErrorDisplayTimeDefault, true, (object)obj, app);
                            obj.Position = origPosition;
                            app.MouseDragEvent = false;
                            return true;
                        }
                    }
                }
                loc.y = app.GetTerrainHeight(loc.x, loc.z);
                placedPosition = loc;
                obj.Position = loc;
                
                app.MouseDragEvent = false;
                return true;
            }
            obj.Position = origPosition;
            app.MouseDragEvent = false;
            return false;
        }
    }
}
