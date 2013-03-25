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
    public class DragObjectsFromMenuCommand : ICommand
    {
        protected WorldEditor app;
        protected List<IWorldObject> list = new List<IWorldObject>();
        protected List<IObjectDrag> dragObject = new List<IObjectDrag>();
        protected List<Vector3> dragOffset = new List<Vector3>();
        protected List<Vector3> origPosition = new List<Vector3>();
        protected List<float> terrainOffset = new List<float>();
        protected List<Vector3> placedPosition = new List<Vector3>();
        protected Vector3 location;
        protected DragCollection dragObjects = new DragCollection();
        protected IWorldObject hitObject;
        protected bool placing = true;

        public DragObjectsFromMenuCommand(WorldEditor worldEditor, List<IWorldObject> list, IWorldObject hit)
        {
            this.app = worldEditor;
            this.hitObject = hit;
            foreach (IWorldObject obj in list)
            {
                this.list.Add(obj);
                this.dragObject.Add(obj as IObjectDrag);
                origPosition.Add(new Vector3((obj as IObjectDrag).Position.x,(obj as IObjectDrag).Position.y, (obj as IObjectDrag).Position.z));
            }
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
                foreach (IObjectDrag obj in dragObject)
                {
                    dragOffset.Add((obj as IObjectDrag).Position - (hitObject as IObjectDrag).Position);
                    switch (obj.ObjectType)
                    {
                        case "Road":
                            (obj as RoadObject).Points.Clone(dragObjects as IWorldContainer);
                            terrainOffset.Add(0f);
                            (obj as IWorldObject).RemoveFromScene();
                            break;
                        case "Region":
                            (obj as Boundary).Points.Clone(dragObjects as IWorldContainer);
                            terrainOffset.Add(0f);
                            (obj as IWorldObject).RemoveFromScene();
                            break;
                        case "TerrainDecal":
                            dragObjects.Add(obj as IWorldObject);
                            terrainOffset.Add(0f);
                            break;
                        case "Marker":
                        case "Object":
                        case "PointLight":
                            dragObjects.Add(obj as IWorldObject);
                            if (obj.AllowAdjustHeightOffTerrain)
                            {
                                terrainOffset.Add((obj as IObjectDrag).TerrainOffset);
                            }
                            else
                            {
                                terrainOffset.Add(0f);
                            }
                            break;

                    }
                }
                new DragHelper(app, dragObjects.DragList, dragOffset, terrainOffset, DragCallback, false);
            }
            else
            {
                if (dragObject.Count > 0 && placedPosition.Count > 0)
                {
                    int i = 0;
                    foreach (IObjectDrag obj in dragObject)
                    {
                        obj.Position = placedPosition[i];
                        //if (obj.AllowAdjustHeightOffTerrain)
                        //{
                        //    position.y = ((app.GetTerrainHeight(obj.Position.x, obj.Position.z)) + terrainOffset[i]);
                        //}
                        //else
                        //{
                        //    position.y = app.GetTerrainHeight(obj.Position.x, obj.Position.z);
                        //}
                        //obj.Position = position;
                        i++;
                    }
                }
            }
        }


        public void UnExecute()
        {
            int i = 0;
            foreach (IWorldObject obj in list)
            {
                (obj as IObjectDrag).Position = origPosition[i];
                i++;
            }
        }

        #endregion ICommand Members

        private bool DragCallback(bool accept, Vector3 loc)
        {
            placing = false;
            int i = 0;
            foreach (IObjectDrag obj in dragObjects.DragList)
            {
                dragObject[i].Position = obj.Position;
                placedPosition.Add(obj.Position);
                switch (dragObject[i].ObjectType)
                {
                    case "Road":
                    case "Region":
                        (dragObject[i] as IWorldObject).AddToScene();
                        (obj as PointCollection).Dispose();
                        break;
                }
                i++;
            }
            app.MouseDragEvent = false;
            return true;
        }

    }
}
