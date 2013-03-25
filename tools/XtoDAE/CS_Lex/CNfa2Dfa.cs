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
	/// Summary description for CNfa2Dfa.
	/// </summary>
    public class CNfa2Dfa
    {
        /***************************************************************
          Member Variables
          **************************************************************/
        private CSpec m_spec;
        private int m_unmarked_dfa;
        private CLexGen m_lexGen;

        /***************************************************************
          Constants
          **************************************************************/
        private const int NOT_IN_DSTATES = -1;

        /***************************************************************
          Function: CNfa2Dfa
          **************************************************************/
        public CNfa2Dfa
            (
            )
        {
            reset();
        }

        /***************************************************************
          Function: Set 
          Description: 
          **************************************************************/
        private void Set
            (
            CLexGen lexGen,
            CSpec spec
            )
        {
            m_lexGen = lexGen;
            m_spec = spec;
            m_unmarked_dfa = 0;
        }

        /***************************************************************
          Function: reset 
          Description: 
          **************************************************************/
        private void reset
            (
            )
        {
            m_lexGen = null;
            m_spec = null;
            m_unmarked_dfa = 0;
        }

        /***************************************************************
          Function: make_dfa
          Description: High-level access function to module.
          **************************************************************/
        public void make_dfa
            (
            CLexGen lexGen,
            CSpec spec
            )
        {
          //  int i;

            reset();
            Set(lexGen,spec);

            make_dtrans();
            free_nfa_states();

            if (m_spec.m_verbose && true == CUtility.OLD_DUMP_DEBUG)
            {
                System.Console.WriteLine(m_spec.m_dfa_states.size()
                    + " DFA states in original machine.");
            }

            free_dfa_states();
        }     

        /***************************************************************
         Function: make_dtrans
         Description: Creates uncompressed CDTrans transition table.
         **************************************************************/
        private void make_dtrans
            (
            )
            /* throws java.lang.CloneNotSupportedException*/
        {
            //CDfa next;
            CDfa dfa;
            CBunch bunch;
            int i;
            int nextstate;
            int size;
            CDTrans dtrans;
            CNfa nfa;
            int istate;
            int nstates;
	
            System.Console.Write("Working on DFA states.");

            /* Reference passing type and initializations. */
            bunch = new CBunch();
            m_unmarked_dfa = 0;

            /* Allocate mapping array. */
            nstates = m_spec.m_state_rules.Length;
            m_spec.m_state_dtrans = new int[nstates];

            for (istate = 0; nstates > istate; ++istate)
            {
                /* CSA bugfix: if we skip all zero size rules, then
                   an specification with no rules produces an illegal
                   lexer (0 states) instead of a lexer that rejects
                   everything (1 nonaccepting state). [27-Jul-1999]
                if (0 == m_spec.m_state_rules[istate].size())
                  {
                m_spec.m_state_dtrans[istate] = CDTrans.F;
                continue;
                  }
                */
		
                /* Create start state and initialize fields. */
                //SI:testing
                //bunch.m_nfa_set = (Vector) m_spec.m_state_rules[istate].Clone();
                bunch.m_nfa_set =(Vector) ((m_spec.m_state_rules[istate]).Clone());
                sortStates(bunch.m_nfa_set);
	    
                bunch.m_nfa_bit = new SparseBitSet();
	    
                /* Initialize bit Set. */
                size = bunch.m_nfa_set.size();
                for (i = 0; size > i; ++i)
                {
                    nfa = (CNfa) bunch.m_nfa_set.elementAt(i);
                    bunch.m_nfa_bit.Set(nfa.m_label);
                }
	    
                bunch.m_accept = null;
                bunch.m_anchor = CSpec.NONE;
                bunch.m_accept_index = CUtility.INT_MAX;
	    
                e_closure(bunch);
                add_to_dstates(bunch);
	    
                m_spec.m_state_dtrans[istate] = m_spec.m_dtrans_vector.size();

                /* Main loop of CDTrans creation. */
                while (null != (dfa = get_unmarked()))
                {
                    System.Console.Write(".");
                    System.Console.Out.Flush();
		
                    if (CUtility.DEBUG)
                    {
                        CUtility.ASSERT(false == dfa.m_mark);
                    }

                    /* Get first unmarked node, then mark it. */
                    dfa.m_mark = true;
		
                    /* Allocate new CDTrans, then initialize fields. */
                    dtrans = new CDTrans(m_spec.m_dtrans_vector.size(),m_spec);
                    dtrans.m_accept = dfa.m_accept;
                    dtrans.m_anchor = dfa.m_anchor;
		
                    /* Set CDTrans array for each character transition. */
                    for (i = 0; i < m_spec.m_dtrans_ncols; ++i)
                    {
                        if (CUtility.DEBUG)
                        {
                            CUtility.ASSERT(0 <= i);
                            CUtility.ASSERT(m_spec.m_dtrans_ncols > i);
                        }
		    
                        /* Create new dfa Set by attempting character transition. */
                        move(dfa.m_nfa_set,dfa.m_nfa_bit,i,bunch);
                        if (null != bunch.m_nfa_set)
                        {
                            e_closure(bunch);
                        }
		    
                        if (CUtility.DEBUG)
                        {
                            CUtility.ASSERT((null == bunch.m_nfa_set 
                                && null == bunch.m_nfa_bit)
                                || (null != bunch.m_nfa_set 
                                && null != bunch.m_nfa_bit));
                        }
		    
                        /* Create new state or Set state to empty. */
                        if (null == bunch.m_nfa_set)
                        {
                            nextstate = CDTrans.F;
                        }
                        else 
                        {
                            nextstate = in_dstates(bunch);
			
                            if (NOT_IN_DSTATES == nextstate)
                            {
                                nextstate = add_to_dstates(bunch);
                            }
                        }
		    
                        if (CUtility.DEBUG)
                        {
                            CUtility.ASSERT(nextstate < m_spec.m_dfa_states.Count);
                        }
		    
                        dtrans.m_dtrans[i] = nextstate;
                    }
		
                    if (CUtility.DEBUG)
                    {
                        CUtility.ASSERT(m_spec.m_dtrans_vector.size() == dfa.m_label);
                    }
		
                    m_spec.m_dtrans_vector.addElement(dtrans);
                }
            }

            System.Console.WriteLine();
        }

        /***************************************************************
          Function: free_dfa_states
          **************************************************************/  
        private void free_dfa_states
            (
            )
        {
            m_spec.m_dfa_states = null;
            m_spec.m_dfa_sets = null;
        }

        /***************************************************************
          Function: free_nfa_states
          **************************************************************/  
        private void free_nfa_states
            (
            )
        {
            /* UNDONE: Remove references to nfas from within dfas. */
            /* UNDONE: Don't free CAccepts. */

            m_spec.m_nfa_states = null;
            m_spec.m_nfa_start = null;
            m_spec.m_state_rules = null;
        }

        /***************************************************************
          Function: e_closure
          Description: Alters and returns input Set.
          **************************************************************/
        private void e_closure
            (
            CBunch bunch
            )
        {
            Stack nfa_stack;
            int size;
            int i;
            CNfa state;

            /* Debug checks. */
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != bunch);
                CUtility.ASSERT(null != bunch.m_nfa_set);
                CUtility.ASSERT(null != bunch.m_nfa_bit);
            }

            bunch.m_accept = null;
            bunch.m_anchor = CSpec.NONE;
            bunch.m_accept_index = CUtility.INT_MAX;
	
            /* Create initial stack. */
            nfa_stack = new Stack();
            size = bunch.m_nfa_set.size();
            for (i = 0; i < size; ++i)
            {
                state = (CNfa) bunch.m_nfa_set.elementAt(i);
	    
                if (CUtility.DEBUG)
                {
                    CUtility.ASSERT(bunch.m_nfa_bit.Get(state.m_label));
                }

                nfa_stack.Push(state);
            }

            /* Main loop. */
            //while (false == nfa_stack.empty())
            while ( nfa_stack.Count>0)
            {
                state = (CNfa) nfa_stack.Pop();
	    
                if (CUtility.OLD_DUMP_DEBUG)
                {
                    if (null != state.m_accept)
                    {
                        System.Console.WriteLine("Looking at accepting state " + state.m_label
                            + " with <"
                            + (new string(state.m_accept.m_action,0,
                            state.m_accept.m_action_read))
                            + ">");
                    }
                }

                if (null != state.m_accept 
                    && state.m_label < bunch.m_accept_index)
                {
                    bunch.m_accept_index = state.m_label;
                    bunch.m_accept = state.m_accept;
                    bunch.m_anchor = state.m_anchor;

                    if (CUtility.OLD_DUMP_DEBUG)
                    {
                        System.Console.WriteLine("Found accepting state " + state.m_label
                            + " with <"
                            + (new string(state.m_accept.m_action,0,
                            state.m_accept.m_action_read))
                            + ">");
                    }

                    if (CUtility.DEBUG)
                    {
                        CUtility.ASSERT(null != bunch.m_accept);
                        CUtility.ASSERT(CSpec.NONE == bunch.m_anchor
                            || 0 != (bunch.m_anchor & CSpec.END)
                            || 0 != (bunch.m_anchor & CSpec.START));
                    }
                }

                if (CNfa.EPSILON == state.m_edge)
                {
                    if (null != state.m_next)
                    {
                        if (false == bunch.m_nfa_set.contains(state.m_next))
                        {
                            if (CUtility.DEBUG)
                            {
                                CUtility.ASSERT(false == bunch.m_nfa_bit.Get(state.m_next.m_label));
                            }
			
                            bunch.m_nfa_bit.Set(state.m_next.m_label);
                            bunch.m_nfa_set.addElement(state.m_next);
                            nfa_stack.Push(state.m_next);
                        }
                    }

                    if (null != state.m_next2)
                    {
                        if (false == bunch.m_nfa_set.contains(state.m_next2))
                        {
                            if (CUtility.DEBUG)
                            {
                                CUtility.ASSERT(false == bunch.m_nfa_bit.Get(state.m_next2.m_label));
                            }
			
                            bunch.m_nfa_bit.Set(state.m_next2.m_label);
                            bunch.m_nfa_set.addElement(state.m_next2);
                            nfa_stack.Push(state.m_next2);
                        }
                    }
                }
            }

            if (null != bunch.m_nfa_set)
            {
                sortStates(bunch.m_nfa_set);
            }

            return;
        }

        /***************************************************************
          Function: move
          Description: Returns null if resulting NFA Set is empty.
          **************************************************************/
        void move
            (
            Vector nfa_set,
            SparseBitSet nfa_bit,
            int b,
            CBunch bunch
            )
        {
            int size;
            int index;
            CNfa state;
	
            bunch.m_nfa_set = null;
            bunch.m_nfa_bit = null;

            size = nfa_set.Count;
          //  System.Console.WriteLine(size);
            for (index = 0; index < size; ++index)
            {
                state = (CNfa) nfa_set.elementAt(index);
             //   System.Console.WriteLine(index+" "+state.m_set);
	    
                if (b == state.m_edge
                    || (CNfa.CCL == state.m_edge
                    && true == state.m_set.contains(b)))
                {
               //     System.Console.WriteLine(state.m_edge+" "+b);
                    if (null == bunch.m_nfa_set)
                    {
                        if (CUtility.DEBUG)
                        {
                            CUtility.ASSERT(null == bunch.m_nfa_bit);
                        }
		    
                        bunch.m_nfa_set = new Vector();
                        /*bunch.m_nfa_bit 
                        = new SparseBitSet(m_spec.m_nfa_states.size());*/
                        bunch.m_nfa_bit = new SparseBitSet();
                    }

                    bunch.m_nfa_set.addElement(state.m_next);
                    /*System.Console.WriteLine("Size of bitset: " + bunch.m_nfa_bit.size());
                    System.Console.WriteLine("Reference index: " + state.m_next.m_label);
                    System.out.flush();*/
                    bunch.m_nfa_bit.Set(state.m_next.m_label);
                }
            }
	
            if (null != bunch.m_nfa_set)
            {
                if (CUtility.DEBUG)
                {
                    CUtility.ASSERT(null != bunch.m_nfa_bit);
                }
	    
                sortStates(bunch.m_nfa_set);
            }

            return;
        }

        /***************************************************************
          Function: sortStates
          **************************************************************/
        private void sortStates
            (
            Vector nfa_set
            )
        {
            CNfa elem;
            int begin;
            int size;
            int index;
            int value;
            int smallest_index;
            int smallest_value;
            CNfa begin_elem;

            size = nfa_set.size();
            for (begin = 0; begin < size; ++begin)
            {
                elem = (CNfa) nfa_set.elementAt(begin);
                smallest_value = elem.m_label;
                smallest_index = begin;

                for (index = begin + 1; index < size; ++index)
                {
                    elem = (CNfa) nfa_set.elementAt(index);
                    value = elem.m_label;

                    if (value < smallest_value)
                    {
                        smallest_index = index;
                        smallest_value = value;
                    }
                }

                begin_elem = (CNfa) nfa_set.elementAt(begin);
                elem = (CNfa) nfa_set.elementAt(smallest_index);
                nfa_set.setElementAt(elem,begin);
                nfa_set.setElementAt(begin_elem,smallest_index);
            }

            if (CUtility.OLD_DEBUG)
            {
                System.Console.Write("NFA vector indices: ");  
	    
                for (index = 0; index < size; ++index)
                {
                    elem = (CNfa) nfa_set.elementAt(index);
                    System.Console.Write(elem.m_label + " ");
                }
                System.Console.WriteLine();
            }	

            return;
        }

        /***************************************************************
          Function: get_unmarked
          Description: Returns next unmarked DFA state.
          **************************************************************/
        private CDfa get_unmarked
            (
            )
        {
            int size;
            CDfa dfa;

            size = m_spec.m_dfa_states.Count;
            while (m_unmarked_dfa < size)
            {
                dfa = (CDfa) m_spec.m_dfa_states.elementAt(m_unmarked_dfa);

                if (false == dfa.m_mark)
                {
                    if (CUtility.OLD_DUMP_DEBUG)
                    {
                        System.Console.Write("*");
                        System.Console.Out.Flush();
                    }

                    if (m_spec.m_verbose && true == CUtility.OLD_DUMP_DEBUG)
                    {
                        System.Console.WriteLine("---------------");
                        System.Console.Write("working on DFA state " 
                            + m_unmarked_dfa
                            + " = NFA states: ");
                        m_lexGen.print_set(dfa.m_nfa_set);
                        System.Console.WriteLine();
                    }

                    return dfa;
                }

                ++m_unmarked_dfa;
            }

            return null;
        }
  
        /***************************************************************
          function: add_to_dstates
          Description: Takes as input a CBunch with details of
          a dfa state that needs to be created.
          1) Allocates a new dfa state and saves it in 
          the appropriate CSpec vector.
          2) Initializes the fields of the dfa state
          with the information in the CBunch.
          3) Returns index of new dfa.
          **************************************************************/
        private int add_to_dstates
            (
            CBunch bunch
            )
        {
            CDfa dfa;
	
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != bunch.m_nfa_set);
                CUtility.ASSERT(null != bunch.m_nfa_bit);
                CUtility.ASSERT(null != bunch.m_accept 
                    || CSpec.NONE == bunch.m_anchor);
            }

            /* Allocate, passing CSpec so dfa label can be Set. */
            dfa = CAlloc.newCDfa(m_spec);
	
            /* Initialize fields, including the mark field. */
            dfa.m_nfa_set = (Vector) bunch.m_nfa_set.Clone();
            dfa.m_nfa_bit = (SparseBitSet) bunch.m_nfa_bit.Clone();
            dfa.m_accept = bunch.m_accept;
            dfa.m_anchor = bunch.m_anchor;
            dfa.m_mark = false;
	
            /* Register dfa state using BitSet in CSpec Hashtable. */
            m_spec.m_dfa_sets.Add(dfa.m_nfa_bit,dfa);
            /*registerCDfa(dfa);*/

            if (CUtility.OLD_DUMP_DEBUG)
            {
                System.Console.Write("Registering Set : ");
                m_lexGen.print_set(dfa.m_nfa_set);
                System.Console.WriteLine();
            }

            return dfa.m_label;
        }

        /***************************************************************
          Function: in_dstates
          **************************************************************/
        private int in_dstates
            (
            CBunch bunch
            )
        {
            CDfa dfa;
	
            if (CUtility.OLD_DEBUG)
            {
                System.Console.Write("Looking for Set : ");
                m_lexGen.print_set(bunch.m_nfa_set);
            }

            dfa = (CDfa) m_spec.m_dfa_sets[bunch.m_nfa_bit];

            if (null != dfa)
            {
                if (CUtility.OLD_DUMP_DEBUG)
                {
                    System.Console.WriteLine(" FOUND!");
                }
	    
                return dfa.m_label;
            }

            if (CUtility.OLD_DUMP_DEBUG)
            {
                System.Console.WriteLine(" NOT FOUND!");
            }
            return NOT_IN_DSTATES;
        }

    }

}
