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

    public enum ClipboardState { cut, copy, clear };

    public class ClipboardObject : IWorldContainer
    {

        protected ClipboardState state = ClipboardState.clear;
        protected List<IWorldObject> clipboard = new List<IWorldObject>();
        protected List<IWorldContainer> parents = new List<IWorldContainer>();
        protected int numPaste = 0;

        public ClipboardObject()
        {
        }

        public int NumPaste
        {
            get
            {
                return numPaste;
            }
        }

        public void IncrementNumPaste()
        {
            numPaste++;
        }

        public List<IWorldObject> Clipboard
        {
            get
            {
                return clipboard;
            }
        }

        public List<IWorldContainer> Parents
        {
            get
            {
                return parents;
            }
        }

        public ClipboardState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        #region IWorldContainer Members

        public void Add(IWorldObject item)
        {
            clipboard.Add(item);
        }

        public bool Remove(IWorldObject item)
        {
            return clipboard.Remove(item);
        }

        #endregion

        #region ICollection<IWorldObject> Members


        public void Clear()
        {
            clipboard.Clear();
            parents.Clear();
            this.numPaste = 0;
            this.state = ClipboardState.clear;
        }

        public bool Contains(IWorldObject item)
        {
            foreach (IWorldObject obj in clipboard)
            {
                if (ReferenceEquals(item, obj))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(IWorldObject[] array, int arrayIndex)
        {
            clipboard.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return clipboard.Count;
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
            return clipboard.GetEnumerator();
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
