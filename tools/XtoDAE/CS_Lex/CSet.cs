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
	/// Summary description for CSet.
	/// </summary>
    /********************************************************
      Class: CSet
      *******************************************************/
   public  class CSet 
    {
        /********************************************************
          Member Variables
          *******************************************************/
        private SparseBitSet m_set;
        private bool m_complement;

        /********************************************************
          Function: CSet
          *******************************************************/
       public  CSet
            (
            )
        {
            m_set = new SparseBitSet();
            m_complement = false;
        }

        /********************************************************
          Function: complement
          *******************************************************/
       public  void complement
            (
            )
        {
            m_complement = true;
        }

        /********************************************************
          Function: add
          *******************************************************/
       public  void add
            (
            int i
            )
        {
            m_set.Set(i);
        }

        /********************************************************
          Function: addncase
          *******************************************************/
      public   void addncase // add, ignoring case.
            (
            char c
            )
        {
            /* Do this in a Unicode-friendly way. */
            /* (note that duplicate adds have no effect) */
            add(c);
            /*add(Character.toLowerCase(c));
            add(Character.toTitleCase(c));
            add(Character.toUpperCase(c));*/
          add(Char.ToLower(c));
          add(Char.ToUpper(c));
          add(Char.ToUpper(c));
        }
  
        /********************************************************
          Function: contains
          *******************************************************/
       public  bool contains
            (
            int i
            )
        {
            bool result;
	
            result = m_set.Get(i);
	
            if (m_complement)
            {
                return (false == result);
            }
	
            return result;
        }

        /********************************************************
          Function: mimic
          *******************************************************/
       public  void mimic
            (
            CSet Set
            )
        {
            m_complement = Set.m_complement;
            m_set = (SparseBitSet) Set.m_set.Clone();
        } 

        /** Map Set using character classes [CSA] */
        public void map(CSet Set, int[] mapping) 
        {
            m_complement = Set.m_complement;
            m_set.clearAll();
            IEnumerator e=Set.m_set.elements();
            while (e.MoveNext() ) 
            {
                int old_value =(int)e.Current;
                if (old_value<mapping.Length) // skip unmapped characters
                    m_set.Set(mapping[old_value]);
            }
        }
       public override string ToString()
       {
           return m_set.ToString();
       }
}
}
