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
    class ChangeCollectionCommand : ICommand
    {
        protected List<IObjectChangeCollection> changeObjList;
        protected List<IObjectCollectionParent> fromCollection;
        protected IObjectCollectionParent toCollection;
        protected WorldEditor app = WorldEditor.Instance;

        public ChangeCollectionCommand(List<IObjectChangeCollection> obj, IObjectCollectionParent toCollection)
        {
            this.changeObjList = obj;
            this.fromCollection = new List<IObjectCollectionParent>();
            this.toCollection = toCollection;
            
        }

        #region ICommand Members

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            foreach (IObjectChangeCollection changeObj in changeObjList)
            {
                IWorldContainer parent = changeObj.Parent;
                fromCollection.Add((IObjectCollectionParent)parent);
                parent.Remove(changeObj as IWorldObject);
                toCollection.Add(changeObj as IWorldObject);
                changeObj.Parent = toCollection as IWorldContainer;
                changeObj.Node.Select();
            }
        }

        public void UnExecute()
        {
            int i = 0;
            foreach(IObjectChangeCollection changeObj in changeObjList)
            {
                IWorldObject obj = changeObj as IWorldObject;
                toCollection.Remove(obj);
                IObjectCollectionParent from = fromCollection[i];
                from.Add(obj);
                changeObj.Parent = from as IWorldContainer;
                i++;
            }
        }
        #endregion ICommand Members

    }
}
