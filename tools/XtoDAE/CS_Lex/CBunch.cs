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
	/// Summary description for CBunch.
	/// </summary>
    public class CBunch
    {
        /***************************************************************
          Member Variables
          **************************************************************/
        public Vector m_nfa_set; /* Vector of CNfa states in dfa state. */
       public  SparseBitSet m_nfa_bit; /* BitSet representation of CNfa labels. */
        public CAccept m_accept; /* Accepting actions, or null if nonaccepting state. */
        public int m_anchor; /* Anchors on regular expression. */
        public int m_accept_index; /* CNfa index corresponding to accepting actions. */

        /***************************************************************
          Function: CBunch
          Description: Constructor.
          **************************************************************/
        public CBunch
            (
            )
        {
            m_nfa_set = null;
            m_nfa_bit = null;
            m_accept = null;
            m_anchor = CSpec.NONE;
            m_accept_index = -1;
        }
    }

}
