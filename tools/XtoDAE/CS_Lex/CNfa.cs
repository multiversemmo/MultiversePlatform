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
	/// Summary description for CNfa.
	/// </summary>
	public class CNfa
	{
        /********************************************************
          Member Variables
          *******************************************************/
       public  int m_edge;  /* Label for edge type:
			 character code, 
			 CCL (character class), 
			 [STATE,
			 SCL (state class),]
			 EMPTY, 
			 EPSILON. */
  
       public  CSet m_set;  /* Set to store character classes. */
        public CNfa m_next;  /* Next state (or null if none). */
  
        public CNfa m_next2;  /* Another state with type == EPSILON
			   and null if not used.  
			   The NFA construction should result in two
			   outgoing edges only if both are EPSILON edges. */
  
        public CAccept m_accept;  /* Set to null if nonaccepting state. */
        public int m_anchor;  /* Says if and where pattern is anchored. */

      public   int m_label;

        SparseBitSet m_states;

        /********************************************************
          Constants
          *******************************************************/
       public  const int NO_LABEL = -1;

        /********************************************************
          Constants: Edge Types
          Note: Edge transitions on one specific character
          are labelled with the character Ascii (Unicode)
          codes.  So none of the constants below should
          overlap with the natural character codes.
          *******************************************************/
       public  const int CCL = -1;
       public  const int EMPTY = -2;
       public  const int EPSILON = -3;
   
        /********************************************************
          Function: CNfa
          *******************************************************/
       public  CNfa
            (
            )
        {
            m_edge = EMPTY;
            m_set = null;
            m_next = null;
            m_next2 = null;
            m_accept = null;
            m_anchor = CSpec.NONE;
            m_label = NO_LABEL;
            m_states = null;
        }

        /********************************************************
          Function: mimic
          Description: Converts this NFA state into a copy of
          the input one.
          *******************************************************/
        public void mimic
            (
            CNfa nfa
            )
        {
            m_edge = nfa.m_edge;
	
            if (null != nfa.m_set)
            {
                if (null == m_set)
                {
                    m_set = new CSet();
                }
                m_set.mimic(nfa.m_set);
            }
            else
            {
                m_set = null;
            }

            m_next = nfa.m_next;
            m_next2 = nfa.m_next2;
            m_accept = nfa.m_accept;
            m_anchor = nfa.m_anchor;

            if (null != nfa.m_states)
            {
                m_states = (SparseBitSet) nfa.m_states.Clone();
            }
            else
            {
                m_states = null;
            }
        }
    }
	}
