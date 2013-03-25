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

namespace Multiverse.Tools.WorldEditor
{
    public class DragObjectsFromMenuCommandFactory : ICommandFactory
    {
        protected List<IWorldObject> list = new List<IWorldObject>();
        protected WorldEditor app;
        protected IWorldObject hitObject;

        public DragObjectsFromMenuCommandFactory(WorldEditor worldEditor)
        {
            this.app = worldEditor;
        }
        #region ICommandFactory Members

        public ICommand CreateCommand()
        {
            if (app.SelectedObject.Count > 0)
            {
                this.hitObject = app.SelectedObject[0];
                foreach (IWorldObject obj in app.SelectedObject)
                {
                    if (!(obj is IObjectDrag) || (obj is MPPoint))
                    {
                        return null;
                    }
                    else
                    {
                        list.Add(obj);
                    }

                }
            }
            ICommand cmd = new DragObjectsFromMenuCommand(app, list, hitObject);
            return cmd;
        }

        #endregion ICommandFactory Members
    }
}
