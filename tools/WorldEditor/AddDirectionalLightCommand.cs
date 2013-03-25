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
using Axiom.Core;
using Axiom.MathLib;

namespace Multiverse.Tools.WorldEditor
{
    class AddDirectionalLightCommand : ICommand
    {
        private WorldEditor app;
        private Boundary parent;
        private SceneManager scene;
        private String name;
        private String meshName;
        // private float rotation = 0;
        // private float scale = 1;
        // private bool placing;
        // private Vector3 location;
        private DirectionalLight directionalLight;
        // private bool cancelled = false;
        private ColorEx specular;
        private ColorEx diffuse;

        public AddDirectionalLightCommand(WorldEditor worldEditor, IWorldContainer parentObject, String objectName, string meshName, ColorEx specular, ColorEx diffuse)
        {
            this.app = worldEditor;
            this.parent =  (Boundary) parentObject;
            this.name = objectName;
            this.meshName = meshName;
            this.specular = specular;
            this.diffuse = diffuse;
            this.scene = app.Scene;
        }

        #region ICommand Members

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            if (directionalLight == null)
            {
                directionalLight = new DirectionalLight(app, parent, new Vector3(0f,0f,0f), diffuse, specular);
            }
            parent.Add(directionalLight);

            for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
            {
                app.SelectedObject[i].Node.UnSelect();
            }
            if (directionalLight.Node != null)
            {
                directionalLight.Node.Select();
            }
        }

        public void UnExecute()
        {
            parent.Remove(directionalLight);
        }

        #endregion
    }
}
