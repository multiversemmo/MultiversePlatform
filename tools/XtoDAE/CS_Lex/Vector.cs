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
using System.Collections;

namespace TUVienna.CS_Lex
{
    /// <summary>
    /// Summary description for Vector.
    /// </summary>
    public class Vector
    {
        ArrayList parent;
        public Vector()
        {
            parent=new ArrayList();
            //
            // TODO: Add constructor logic here
            //
        }
        public Vector(ArrayList mPar)
        {
            parent=mPar;
        }
    
        public int size()
        {
            return parent.Count;
        }

        public int Count
        {
            get
            {
                return parent.Count;
            }
        }

        public object elementAt(int i)
        {
            return parent[i];
        }
        public IEnumerator elements()
        {
            return parent.GetEnumerator();
        }

        public int indexOf(object elem)
        {
            return parent.IndexOf(elem);
        }
        public void addElement(object elem)
        {
            parent.Add(elem);
        }

        public void removeElement(object elem)
        {
            parent.Remove(elem);
        }
        public void removeElementAt(int i)
        {
            parent.RemoveAt(i);
        }
        public void setElementAt(object obj,int pos)
        {
            parent[pos]=obj;
        }

        public bool contains(object elem)
        {
            return parent.Contains(elem);
        }

        public void setSize(int i)
        {
            if (parent.Count<=i)
            {
                parent.Capacity=i;
            }
            else
            {
                if (parent.Count>i)
                    parent.RemoveRange(i,parent.Count-i);
                parent.Capacity=i;
            }
        }

        public  object Clone()
        {
            return new Vector((ArrayList)parent.Clone());
        }

    }
}
