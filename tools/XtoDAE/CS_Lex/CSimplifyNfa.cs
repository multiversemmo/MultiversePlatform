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
	/// Summary description for CSimplifyNfa.
	/// </summary>
   public  class CSimplifyNfa
    {
        private int[] ccls; // character class mapping.
        private int original_charset_size; // original charset size
        private int mapped_charset_size; // reduced charset size

      public   void simplify(CSpec m_spec) 
        {
            computeClasses(m_spec); // initialize fields.
    
            // now rewrite the NFA using our character class mapping.
          IEnumerator e=m_spec.m_nfa_states.elements();
            while ( e.MoveNext() ) 
            {
                CNfa nfa = (CNfa) e.Current;
                if (nfa.m_edge==CNfa.EMPTY || nfa.m_edge==CNfa.EPSILON)
                    continue; // no change.
                if (nfa.m_edge==CNfa.CCL) 
                {
                    CSet ncset = new CSet();
                    ncset.map(nfa.m_set, ccls); // map it.
                    nfa.m_set = ncset;
                } 
                else 
                { // single character
                    nfa.m_edge = ccls[nfa.m_edge]; // map it.
                }
            }

            // now update m_spec with the mapping.
            m_spec.m_ccls_map = ccls;
            m_spec.m_dtrans_ncols = mapped_charset_size;
        }
        /** Compute minimum Set of character classes needed to disambiguate
         *  edges.  We optimistically assume that every character belongs to
         *  a single character class, and then incrementally split classes
         *  as we see edges that require discrimination between characters in
         *  the class. [CSA, 25-Jul-1999] */
        private void computeClasses(CSpec m_spec) 
        {
            this.original_charset_size = m_spec.m_dtrans_ncols;
            this.ccls = new int[original_charset_size]; // initially all zero.

            int nextcls = 1;
            SparseBitSet clsA = new SparseBitSet(), clsB = new SparseBitSet();
            Hashtable h = new Hashtable();
    
            System.Console.Write("Working on character classes.");
            IEnumerator e=m_spec.m_nfa_states.elements();
            while ( e.MoveNext() ) 
            {
                CNfa nfa = (CNfa) e.Current;
                if (nfa.m_edge==CNfa.EMPTY || nfa.m_edge==CNfa.EPSILON)
                    continue; // no discriminatory information.
                clsA.clearAll(); clsB.clearAll();
                for (int i=0; i<ccls.Length; i++)
                    if (nfa.m_edge==i || // edge labeled with a character
                        nfa.m_edge==CNfa.CCL && nfa.m_set.contains(i)) // Set of characters
                        clsA.Set(ccls[i]);
                    else
                        clsB.Set(ccls[i]);
                // now figure out which character classes we need to split.
                clsA.and(clsB); // split the classes which show up on both sides of edge
                System.Console.Write(clsA.size()==0?".":":");
                if (clsA.size()==0) continue; // nothing to do.
                // and split them.
                h.Clear(); // h will map old to new class name
                for (int i=0; i<ccls.Length; i++)
                    if (clsA.Get(ccls[i])) // a split class
                        if (nfa.m_edge==i ||
                            nfa.m_edge==CNfa.CCL && nfa.m_set.contains(i)) 
                        { // on A side
                            int split = ccls[i];
                            if (!h.ContainsKey(split))
                                h.Add(split, (nextcls++)); // make new class
                            ccls[i] = (int)h[split];
                        }
            }
            System.Console.WriteLine();
            System.Console.WriteLine("NFA has "+nextcls+" distinct character classes.");
    
            this.mapped_charset_size = nextcls;
        }
    }

}
