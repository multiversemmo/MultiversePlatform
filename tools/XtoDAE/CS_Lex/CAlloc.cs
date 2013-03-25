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

namespace TUVienna.CS_Lex
{
	/// <summary>
	/// Summary description for CAlloc.
	/// </summary>
  public   class CAlloc
    {
        /***************************************************************
          Function: newCDfa
          **************************************************************/
        public static CDfa newCDfa
            (
            CSpec spec
            )
        {
            CDfa dfa;
	
            dfa = new CDfa(spec.m_dfa_states.size());
            spec.m_dfa_states.addElement(dfa);

            return dfa;
        }

        /***************************************************************
          Function: newCNfaPair
          Description: 
          **************************************************************/
        public static CNfaPair newCNfaPair
            (
            )
        {
            CNfaPair pair = new CNfaPair();
	
            return pair;
        }
        /***************************************************************
          Function: newNLPair
          Description: return a new CNfaPair that matches a new 
                       line: (\r\n?|[\n\uu2028\uu2029])
               Added by CSA 8-Aug-1999, updated 10-Aug-1999
          **************************************************************/
       public  static CNfaPair newNLPair(CSpec spec) 
        {
            CNfaPair pair = newCNfaPair();
            pair.m_end=newCNfa(spec); // newline accepting state
            pair.m_start=newCNfa(spec); // new state with two epsilon edges
            pair.m_start.m_next = newCNfa(spec);
            pair.m_start.m_next.m_edge = CNfa.CCL;
            pair.m_start.m_next.m_set = new CSet();
            pair.m_start.m_next.m_set.add('\n');
            if (spec.m_dtrans_ncols-CSpec.NUM_PSEUDO > 2029) 
            {
                pair.m_start.m_next.m_set.add(2028); /*U+2028 is LS, the line separator*/
                pair.m_start.m_next.m_set.add(2029); /*U+2029 is PS, the paragraph sep.*/
            }
            pair.m_start.m_next.m_next = pair.m_end; // accept '\n', U+2028, or U+2029
            pair.m_start.m_next2 = newCNfa(spec);
            pair.m_start.m_next2.m_edge = '\r';
            pair.m_start.m_next2.m_next = newCNfa(spec);
            pair.m_start.m_next2.m_next.m_next = pair.m_end; // accept '\r';
            pair.m_start.m_next2.m_next.m_next2 = newCNfa(spec);
            pair.m_start.m_next2.m_next.m_next2.m_edge = '\n';
            pair.m_start.m_next2.m_next.m_next2.m_next = pair.m_end; // accept '\r\n';
            return pair;
        }

        /***************************************************************
          Function: newCNfa
          Description: 
          **************************************************************/
        public static CNfa newCNfa
            (
            CSpec spec
            )
        {
            CNfa p;

            /* UNDONE: Buffer this? */

            p = new CNfa();
	
            /*p.m_label = spec.m_nfa_states.size();*/
            spec.m_nfa_states.addElement(p);
            p.m_edge = CNfa.EPSILON;
	
            return p;
        }
    }

}
