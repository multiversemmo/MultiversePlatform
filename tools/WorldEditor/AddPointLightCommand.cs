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
    public class AddPointLightCommand : ICommand
    {
        private WorldEditor app;
        private IWorldContainer parent;
        private SceneManager scene;
        private String name;
        private String meshName;
        private float rotation = 0;
        private float scale = 1;
        private bool placing;
        private Vector3 location;
        private PointLight pointLight;
        private DisplayObject dragObject;
        private bool cancelled = false;
        private ColorEx specular;
        private ColorEx diffuse;

        public AddPointLightCommand(WorldEditor worldEditor, IWorldContainer parentObject, String objectName, string meshName, ColorEx specular, ColorEx diffuse)
        {
            this.app = worldEditor;
            this.parent = parentObject;
            this.name = objectName;
            this.meshName = meshName;
            this.specular = specular;
            this.diffuse = diffuse;
            this.scene = app.Scene;
            placing = true;
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
                if (placing)
                {
                    // we need to place the object (which is handled asynchronously) before we can create it
                    dragObject = new DisplayObject(name, app, "Drag", app.Scene, app.Assets.assetFromName(app.Config.PointLightMeshName).AssetName, location, new Vector3(1,1,1), new Vector3(0,0,0), null);
                    dragObject.TerrainOffset = app.Config.DefaultPointLightHeight;

                    new DragHelper(app, new DragComplete(DragCallback), dragObject);
                }
                else
                {
                    // object has already been placed, so create it now
                    // only create it if it doesn't exist already
                    if (pointLight == null)
                    {
                        pointLight = new PointLight(app, parent, scene, name, specular, diffuse, location);
                    }
                    parent.Add(pointLight);
                    for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
                    {
                        app.SelectedObject[i].Node.UnSelect();
                    }
                    if (pointLight.Node != null)
                    {
                        pointLight.Node.Select();
                    }
                }
            }
        }

        public void UnExecute()
        {
            parent.Remove(pointLight);
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
                pointLight = new PointLight(app, parent, scene, name, specular, diffuse, new Vector3(location.x, location.y, location.z));
                parent.Add(pointLight);
                for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
                {
                    app.SelectedObject[i].Node.UnSelect();
                }
                if (pointLight.Node != null)
                {
                    pointLight.Node.Select();
                }
            }
            else
            {
                cancelled = true;
            }

            return true;
        }
    }
}
