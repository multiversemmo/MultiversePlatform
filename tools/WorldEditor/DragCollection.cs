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
    public class DragCollection : IWorldContainer
    {
        protected List<IWorldObject> worldObjects = new List<IWorldObject>();
        protected List<IObjectDrag> dragObjects = new List<IObjectDrag>();

        public DragCollection()
        {
        }


        #region IWorldContainer Members

        public List<IObjectDrag> DragList
        {
            get
            {
                return dragObjects;
            }
        }

        public void Add(IWorldObject item)
        {
            dragObjects.Add(item as IObjectDrag);
            worldObjects.Add(item);
        }

        public bool Remove(IWorldObject item)
        {
            if (dragObjects.Remove(item as IObjectDrag) && worldObjects.Remove(item))
            {
                return true;
            }
            return false;
        }

        #endregion

        #region ICollection<IWorldObject> Members


        public void Clear()
        {
            dragObjects.Clear();
            worldObjects.Clear();
            
        }

        public bool Contains(IWorldObject item)
        {
            if(worldObjects.Contains(item) && dragObjects.Contains(item as IObjectDrag))
            {
                return true;
            }
            return false;
        }

        public void CopyTo(IWorldObject[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count
        {
            get
            {
                return dragObjects.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region IEnumerable<IWorldObject> Members

        public IEnumerator<IWorldObject> GetEnumerator()
        {
            return worldObjects.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

    }
}
