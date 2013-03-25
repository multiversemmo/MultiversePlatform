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
using System.Windows.Forms;

namespace Multiverse.Tools.WorldEditor
{
    public class AddObjectCommand : ICommand
    {
        private WorldEditor app;
        private IWorldContainer parent;
        private String name;
        private String meshName;
        private float rotation = 0;
        private float scale = 1;
        private bool placing;
        private Vector3 location;
        private StaticObject staticObject;
        private DisplayObject dragObject;
        private bool cancelled = false;
        private bool placeMultiple;
        private bool randomRotation;
        private bool randomScale;
        private float minScale;
        private float maxScale;
        private int objectNumber = 1;

        public AddObjectCommand(WorldEditor worldEditor, IWorldContainer parentObject, String objectName, 
            String meshName, bool randomRotation, bool randomScale, float minScale, float maxScale,
            bool multiPlacement)
        {
            this.app = worldEditor;
            this.parent = parentObject;
            this.name = objectName;
            this.meshName = meshName;
            placeMultiple = multiPlacement;
            this.randomRotation = randomRotation;
            this.randomScale = randomScale;
            this.minScale = minScale;
            this.maxScale = maxScale;


            // apply random scale and rotation
            if (randomRotation)
            {
                rotation = (float)app.Random.NextDouble() * 360f;
            }
            if (randomScale)
            {
                float scaleRange = maxScale - minScale;

                scale = minScale + (float)app.Random.NextDouble() * scaleRange;
            }

            
            placing = true;
        }

        public AddObjectCommand(WorldEditor worldEditor, IWorldContainer parentObject, string objectName,
            string meshName, bool randomRotation, bool randomScale, float minScale, float maxScale,
            Vector3 position)
        {
            this.app = worldEditor;
            this.parent = parentObject;
            this.name = objectName;
            this.meshName = meshName;
            this.location = position;
            this.randomRotation = randomRotation;
            this.randomScale = randomScale;
            this.minScale = minScale;
            this.maxScale = maxScale;

            // apply random scale and rotation
            if (randomRotation)
            {
                rotation = (float)app.Random.NextDouble() * 360f;
            }
            if (randomScale)
            {
                float scaleRange = maxScale - minScale;

                scale = minScale + (float)app.Random.NextDouble() * scaleRange;
            }

            placing = false;
        }

        #region ICommand Members

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            if (!cancelled)
            {
                Vector3 scaleVec = new Vector3(scale, scale, scale);
                Vector3 rotVec = new Vector3(0, rotation, 0);
                if (placing)
                {
                    // we need to place the object (which is handled asynchronously) before we can create it
                    dragObject = new DisplayObject(name, app, "Drag", app.Scene, meshName, location, scaleVec, rotVec, null);

                    if (placeMultiple)
                    {
                        new MultiPointPlacementHelper(app, ObjectValidate, dragObject, ObjectPlacementComplete);
                        cancelled = true;
                    }
                    else
                    {
                        new DragHelper(app, new DragComplete(DragCallback), dragObject, false);
                    }
                }
                else
                {
                    // object has already been placed, so create it now
                    // only create it if it doesn't exist already
                    if (staticObject == null)
                    {
                        staticObject = new StaticObject(name, parent, app, meshName, location, scaleVec, rotVec);
                    }
                    parent.Add(staticObject);
                    for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
                    {
                        if (app.SelectedObject[i] != null && app.SelectedObject[i].Node != null)
                        {
                            app.SelectedObject[i].Node.UnSelect();
                        }
                    }
                    if (staticObject.Node != null)
                    {
                        staticObject.Node.Select();
                    }
                }
            }
        }

        public void UnExecute()
        {
            if (!cancelled)
            {
                parent.Remove(staticObject);
            }
        }

        #endregion

        private bool DragCallback(bool accept, Vector3 loc)
        {
            placing = false;

            dragObject.Dispose();

            if (accept)
            {
                location = loc;
                Vector3 scaleVec = new Vector3(scale, scale, scale);
                Vector3 rotVec = new Vector3(0, rotation, 0);
                staticObject = new StaticObject(name, parent, app, meshName, location, scaleVec, rotVec);
                parent.Add(staticObject);
                for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
                {
                    app.SelectedObject[i].Node.UnSelect();
                }
                if (staticObject.Node != null)
                {
                    staticObject.Node.Select();
                }
            }
            else
            {
                cancelled = true;
            }

            return true;
        }

        protected void ObjectPlacementComplete(List<Vector3> points)
        {
            dragObject.Dispose();
        }

        protected bool ObjectValidate(List<Vector3> points, Vector3 location)
        {
            Vector3 scaleVec = new Vector3(scale, scale, scale);
            Vector3 rotVec = new Vector3(0, rotation, 0);
            AddObjectCommand addCmd = new AddObjectCommand(app, parent, String.Format("{0}-{1}", name, objectNumber), meshName, randomRotation, randomScale, minScale, maxScale, location);
            app.ExecuteCommand(addCmd);
            objectNumber++;
            return true;
        }

    }
}
