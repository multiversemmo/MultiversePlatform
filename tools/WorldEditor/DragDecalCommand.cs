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
    public class DragDecalCommand : ICommand
    {
        WorldEditor app;
        IWorldContainer parent;
        TerrainDecal decal;
        Vector2 oldPosition;
        Vector2 newPosition;
        Vector3 location;
        bool placing;
        bool canceled;

        public DragDecalCommand(WorldEditor worldEditor, IWorldContainer parent, TerrainDecal tDecal)
        {
            this.app = worldEditor;
            this.parent = parent;
            this.decal = tDecal;
            this.placing = true;
            oldPosition = new Vector2(decal.Position.x,decal.Position.z);
        }

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            if (placing)
            {
                canceled = false;
                new DragHelper(app, parent, decal, DragCallback);
            }
            else
            {
                decal.Position = location;
            }
        }

        public void UnExecute()
        {
            decal.Position = new Vector3(oldPosition.x, 0f, oldPosition.y);
        }

        private bool DragCallback(bool accept, Vector3 loc)
        {
            if (accept)
            {
                this.location = loc;
                this.decal.Position = loc;
                placing = false;
            }
            else
            {
                canceled = true;
                this.decal.Position = new Vector3(oldPosition.x, 0f, oldPosition.y);
            }
            return true;
        }
    }
}
