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
    public delegate bool DragComplete(bool accept, Vector3 location);

    public class DragHelper : IDisposable
    {
        protected DragComplete dragCallback;
        protected WorldEditor app;
        protected DisplayObject dragObject;
        protected List<DisplayObject> dragObjs = new List<DisplayObject>();
        protected List<IObjectDrag> dragObjects = new List<IObjectDrag>();
        protected List<Vector3> dragOffset = new List<Vector3>();
        protected List<float> terrainOffset = new List<float>();
        protected bool disposeDragObject;
        protected bool dragging = true;
        protected bool stopOnUp = false;
        protected string imageName;
        protected Vector2 size;
        protected TerrainDecal decal;
        protected string name;
        protected List<IObjectCutCopy> origObjs = new List<IObjectCutCopy>();
        protected MouseButtons but;

        protected Vector3 location = new Vector3(0, 0, 0);
        protected List<Vector3> dragOffsets;

        /// <summary>
        /// Use this constructor when dragging an existing object, or one that needs rotation or scaling.
        /// </summary>
        /// <param name="worldEditor"></param>
        /// <param name="displayObject"></param>
        /// <param name="callback"></param>
        public DragHelper(WorldEditor worldEditor, DisplayObject displayObject, DragComplete callback)
        {
            app = worldEditor;
            dragObject = displayObject;
            dragCallback = callback;
            disposeDragObject = false;
            but = app.MouseSelectButton;

            // set up mouse capture and callbacks for placing the object
            app.InterceptMouse(new MouseMoveIntercepter(DragMove), new MouseButtonIntercepter(DragButtonDown), new MouseButtonIntercepter(DragButtonUp), new MouseCaptureLost(DragCaptureLost), true);
        }

        /// <summary>
        /// Use this constructor when placing point lights and markers to allow them to be placed on other objects.
        /// </summary>
        /// <param name="worldEditor"></param>
        /// <param name="callback"></param>
        /// <param name="displayObject"></param>
        public DragHelper(WorldEditor worldEditor, DragComplete callback, DisplayObject displayObject)
        {
            app = worldEditor;
            dragObject = displayObject;
            dragCallback = callback;
            disposeDragObject = false;
            but = app.MouseSelectButton;

            // set up mouse capture and callbacks for placing the object
            app.InterceptMouse(new MouseMoveIntercepter(DragMoveAllowObject), new MouseButtonIntercepter(DragButtonDownAllowObject), new MouseButtonIntercepter(DragButtonUp), new MouseCaptureLost(DragCaptureLost), true);
        }


        /// <summary>
        /// Use this constructor when you want to choose to stop on the mouse up or down.  stopOnUP should 
        /// be false for stoping with mouse down and true for stopping on the mouse being up.
        /// This will preserve the objects scale and rotation.
        /// 
        /// </summary>
        /// <param name="worldEditor"></param>
        /// <param name="displayObject"></param>
        /// <param name="callback"></param>
        /// <param name="stopOnUp"></param>
        public DragHelper(WorldEditor worldEditor, DisplayObject displayObject, DragComplete callback, bool stopOnUp)
        {
            app = worldEditor;
            dragObject = displayObject;
            dragCallback = callback;
            disposeDragObject = false;
            this.stopOnUp = stopOnUp;
            but = app.MouseSelectButton;


            // set up mouse capture and callbacks for placing the object
            app.InterceptMouse(new MouseMoveIntercepter(DragMove), new MouseButtonIntercepter(DragButtonDown), new MouseButtonIntercepter(DragButtonUp), new MouseCaptureLost(DragCaptureLost), true);
        }

        /// <summary>
        /// This is used when placing a single static object it preserves the object rotation and scale and allows placement on another object.
        /// 
        /// </summary>
        /// <param name="worldEditor"></param>
        /// <param name="displayObject"></param>
        /// <param name="callback"></param>
        /// <param name="stopOnUp"></param>
        public DragHelper(WorldEditor worldEditor, DragComplete callback, DisplayObject displayObject, bool stopOnUp)
        {
            app = worldEditor;
            dragObject = displayObject;
            dragCallback = callback;
            disposeDragObject = false;
            this.stopOnUp = stopOnUp;
            but = app.MouseSelectButton;


            // set up mouse capture and callbacks for placing the object
            app.InterceptMouse(new MouseMoveIntercepter(DragMoveAllowObject), new MouseButtonIntercepter(DragButtonDownAllowObject), new MouseButtonIntercepter(DragButtonUp), new MouseCaptureLost(DragCaptureLost), true);
        }




        /// <summary>
        /// Use this constructor when you want the DragHelper to create the displayObject for you
        /// based on the meshName.
        /// </summary>
        /// <param name="worldEditor"></param>
        /// <param name="meshName"></param>
        /// <param name="callback"></param>
        public DragHelper(WorldEditor worldEditor, String meshName, DragComplete callback)
        {
            app = worldEditor;
            dragObject = new DisplayObject(meshName, app, "Drag", app.Scene, meshName, location, 
                new Vector3(1,1,1), new Vector3(0,0,0), null);
            dragCallback = callback;
            disposeDragObject = true;
            but = app.MouseSelectButton;

            // set up mouse capture and callbacks for placing the object
            app.InterceptMouse(new MouseMoveIntercepter(DragMove), new MouseButtonIntercepter(DragButtonDown), new MouseButtonIntercepter(DragButtonUp), new MouseCaptureLost(DragCaptureLost), true);            
        }


        public DragHelper(WorldEditor worldEditor, string meshName, DragComplete callback, bool accept, Vector3 loc, int index)
        {
            app = worldEditor;
            dragCallback = callback;
            disposeDragObject = true;
            this.location = loc;
            but = app.MouseSelectButton;
            // set up mouse capture and callbacks for placing the object
            app.InterceptMouse(new MouseMoveIntercepter(DecalDragMove), new MouseButtonIntercepter(DragButtonDown), new MouseButtonIntercepter(DragButtonUp), new MouseCaptureLost(DragCaptureLost), true);            
        }


        /// <summary>
        /// Use this constructor when you want to drag a TerrainDecal and have the drag stopped on a left button mouse down. Creates its own decal and disposes it.
        /// Usually used to place new decals.
        /// </summary>
        /// <param name="worldEditor"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="callback"></param>
        public DragHelper(WorldEditor worldEditor, IWorldContainer parent, string name, string fname, Vector2 size, DragComplete callback)
        {
            this.app = worldEditor;
            this.imageName = fname;
            this.size = size;
            this.name = name;
            stopOnUp = false;
            disposeDragObject = true;
            decal = new TerrainDecal(app, parent, name, new Vector2(0f, 0f), size, imageName, 1);
            decal.AddToScene();
            dragCallback = callback;
            but = app.MouseSelectButton;

            app.InterceptMouse(new MouseMoveIntercepter(DecalDragMove), new MouseButtonIntercepter(DecalDragButtonDown), new MouseButtonIntercepter(DecalDragButtonUp), new MouseCaptureLost(DragCaptureLost), true);
        }


        /// <summary>
        /// Use this constructor to drag top level objects.  Used when placing objects from the clipboard in to the world and when top level objects (not MPPoints)
        /// are being dragged.  Not used for doing the original placement of points, objects, markers, point lights, or decals.
        /// </summary>
        /// <param name="worldEditor"></param>
        /// <param name="displayObject"></param>
        /// <param name="callback"></param>
        /// <param name="stopOnUp"></param>
        public DragHelper(WorldEditor worldEditor, List<IObjectDrag> dragObjes, List<Vector3> dragOffsets, List<float> terrainOffset, DragComplete callback, bool stopOnUp)
        {
            app = worldEditor;
            dragCallback = callback;
            disposeDragObject = false;
            this.stopOnUp = stopOnUp;
            Vector3 baseObjPosition = dragObjes[0].Position;
            int i = 0;
            but = app.MouseSelectButton;
            
            foreach (IObjectDrag obj in dragObjes)
            {
                this.dragObjects.Add(obj);
                this.dragOffset.Add(dragOffsets[i]);
                this.terrainOffset.Add(terrainOffset[i]);
                i++;
            }
            app.InterceptMouse(new MouseMoveIntercepter(MultipleDragMove), new MouseButtonIntercepter(MultipleDragButtonDown), new MouseButtonIntercepter(MultipleDragButtonUp), new MouseCaptureLost(DragCaptureLost), true);
        }




        protected void StopDrag(bool accept)
        {
            bool valid = dragCallback(accept, location);

            // if the callback accepts the placement, then stop the drag and clean up
            if (valid)
            {
                app.ReleaseMouse();
                if (disposeDragObject)
                {
                    dragObject.Dispose();
                }

                dragging = false;
            }
        }

        protected void DecalStopDrag(bool accept)
        {
            bool valid = dragCallback(accept, location);
            if (valid)
            {
                app.ReleaseMouse();
                if (disposeDragObject)
                {
                    decal.Dispose();
                }
            }
        }

        public void DragButtonDown(WorldEditor app, MouseButtons button, int x, int y)
        {
            if (dragging)
            {
                DragMove(app, x, y);
                if (button == but)
                {
                    StopDrag(true);
                }
                else
                {
                    StopDrag(false);
                }
            }
        }

        public void DragButtonDownAllowObject(WorldEditor app, MouseButtons button, int x, int y)
        {
            if (dragging)
            {
                DragMoveAllowObject(app, x, y);
                if (button == but)
                {
                    StopDrag(true);
                }
                else
                {
                    StopDrag(false);
                }
            }
        }

        public void DragButtonUp(WorldEditor app, MouseButtons button, int x, int y)
        {
            if (dragging && stopOnUp)
            {
                DragMove(app, x, y);
                if (button == but)
                {
                    StopDrag(true);
                    stopOnUp = false;
                }
                else
                {
                    StopDrag(false);
                }
            }
        }

        public void DecalDragButtonUp(WorldEditor app, MouseButtons button, int x, int y)
        {
            DecalDragMove(app, x, y);
            if (dragging && stopOnUp)
            {
                if (button == but)
                {
                    DecalStopDrag(true);
                    stopOnUp = false;
                }

            }
            else
            {
                DecalStopDrag(false);
            }
        }

        public void DecalDragButtonDown(WorldEditor app, MouseButtons button, int x, int y)
        {

            DecalDragMove(app, x, y);
            if (dragging && !stopOnUp)
            {
                if (!stopOnUp && button == but)
                {
                    DecalStopDrag(true);
                }
            }
            else
            {
                DecalStopDrag(false);
            }
        }


        public void MultipleDragButtonDown(WorldEditor app, MouseButtons button, int x, int y)
        {
            if (dragging)
            {
                MultipleDragMove(app, x, y);
                if (button == but  && !stopOnUp)
                {
                    StopDrag(true);
                }
                else
                {
                    StopDrag(false);
                }
            }
        }


        public void MultipleDragButtonUp(WorldEditor app, MouseButtons button, int x, int y)
        {
            if (dragging && stopOnUp)
            {
                MultipleDragMove(app, x, y);
                if (button == but)
                {
                    StopDrag(true);
                    stopOnUp = false;
                }
                else
                {
                    StopDrag(false);
                }
            }
        }

        public void DragMoveAllowObject(WorldEditor app, int x, int y)
        {
            if (dragging)
            {
                location = app.ObjectPlacementLocation(x, y);
                location.y = location.y + dragObject.TerrainOffset;
                dragObject.Position = location;
            }
        }

        public void DragMove(WorldEditor app, int x, int y)
        {
            if (dragging)
            {
                location = app.PickTerrain(x, y);
                location.y = app.GetTerrainHeight(location.x, location.z) + dragObject.TerrainOffset;
                dragObject.Position = location;
            }
        }

        public void DecalDragMove(WorldEditor app, int x, int y)
        {
            if (dragging)
            {
                location = app.PickTerrain(x, y);
                decal.Position = location;
            }
        }

        public void MultipleDragMove(WorldEditor app, int x, int y)
        {
            int i = 0;
            location = app.PickTerrain(x, y);
            Vector3 position;
            foreach (IObjectDrag disObject in dragObjects)
            {
                switch (disObject.ObjectType)
                {
                    case "PointLight":
                    case "Marker":
                    case "Object":
                        position = location + dragOffset[i];
                        if (i == 0)
                        {
                            if (disObject.AllowAdjustHeightOffTerrain)
                            {
                                position = app.ObjectPlacementLocation(x, y) + new Vector3(0, terrainOffset[i], 0);
                            }
                            else
                            {
                                position = app.ObjectPlacementLocation(x, y);
                            }
                            disObject.Position = position;
                            break;
                        }
                        else
                        {
                            if (disObject.AllowAdjustHeightOffTerrain)
                            {
                                position = app.ObjectPlacementLocation(location + dragOffset[i]) + new Vector3(0, terrainOffset[i], 0);
                            }
                            else
                            {
                                position = app.ObjectPlacementLocation(location + dragOffset[i]);
                            }
                            disObject.Position = position;
                            break;
                        }
                    default:
                        position = location + dragOffset[i];
                        position.y = app.GetTerrainHeight(location.x, location.z);
                        disObject.Position = position;
                        if (String.Equals(disObject.ObjectType, "Points") && (disObject as PointCollection).DisplayMarkers != true)
                        {
                            (disObject as PointCollection).DisplayMarkers = true;
                        }
                        break;
                }
                if (!disObject.InScene)
                {
                    (disObject as IWorldObject).AddToScene();
                }
                i++;
            }
        }

        public void DragCaptureLost(WorldEditor app)
        {
            // It looks like we dont need to worry about capture lost
            //if (dragging)
            //{
            //    StopDrag(false);
            //}
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (dragging)
            {
                StopDrag(false);
            }
        }

        #endregion
    }
}
