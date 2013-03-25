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
    public class AddTerrainDecalCommand : ICommand
    {
        protected WorldEditor app;
        protected IWorldContainer parent;
        protected string name;
        protected string filename;
        protected Vector2 size;
        protected int priority;
        protected Vector3 location;
        protected bool placing;
        protected bool cancelled;
        protected TerrainDecal decal;


        public AddTerrainDecalCommand(WorldEditor worldEditor, IWorldContainer parent, string name, string filename, Vector2 size, int pri)
        {
            this.app = worldEditor;
            this.name = name;
            this.filename = filename;
            this.size = new Vector2(size.x, size.y);
            this.priority = pri;
            this.placing = true;
            this.parent = parent;
        }

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            if (placing)
            {
                new DragHelper(app, parent, name, filename, size, new DragComplete(DragCallback));
            }
            else
            {
                decal = new TerrainDecal(app, parent, name, new Vector2(location.x, location.z), size, filename, priority);
                parent.Add(decal);
                for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
                {
                    app.SelectedObject[i].Node.UnSelect();
                }
                decal.Node.Select();
            }
            
        }

        public void UnExecute()
        {
            parent.Remove(decal);
        }

        private bool DragCallback(bool accept, Vector3 loc)
        {
            if (accept)
            {
                this.location = loc;
                decal = new TerrainDecal(app, parent, name, new Vector2(location.x, location.z), size, filename, priority);
                parent.Add(decal);
                for (int i = app.SelectedObject.Count - 1; i >= 0; i--)
                {
                    app.SelectedObject[i].Node.UnSelect();
                }
                if (decal.Node != null)
                {
                    decal.Node.Select();
                }

                placing = false;
            }
            else
            {
                cancelled = true;
            }
            return true;
        }
    }
}
