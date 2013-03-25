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
	/// Summary description for CMakeNfa.
	/// </summary>
    public class CMakeNfa
    {
        /***************************************************************
          Member Variables
          **************************************************************/
        private CSpec m_spec;
        private CLexGen m_lexGen;
        private CInput m_input;

        /***************************************************************
          Function: CMakeNfa
          Description: Constructor.
          **************************************************************/
       public  CMakeNfa
            (
            )
        {
            reset();
        }

        /***************************************************************
          Function: reset
          Description: Resets CMakeNfa member variables.
          **************************************************************/
        private void reset
            (
            )
        {
            m_input = null;
            m_lexGen = null;
            m_spec = null;
        }

        /***************************************************************
          Function: Set
          Description: Sets CMakeNfa member variables.
          **************************************************************/
        private void Set
            (
            CLexGen lexGen,
            CSpec spec,
            CInput input
            )
        {
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != input);
                CUtility.ASSERT(null != lexGen);
                CUtility.ASSERT(null != spec);
            }

            m_input = input;
            m_lexGen = lexGen;
            m_spec = spec;
        }

        /***************************************************************
          Function: allocate_BOL_EOF
          Description: Expands character class to include special BOL and
          EOF characters.  Puts numeric index of these characters in
          input CSpec.
          **************************************************************/
       public  void allocate_BOL_EOF
            (
            CSpec spec
            )
        {
            CUtility.ASSERT(CSpec.NUM_PSEUDO==2);
            spec.BOL = spec.m_dtrans_ncols++;
            spec.EOF = spec.m_dtrans_ncols++;
        }

        /***************************************************************
          Function: thompson
          Description: High level access function to module.
          Deposits result in input CSpec.
          **************************************************************/
       public  void thompson
            (
            CLexGen lexGen,
            CSpec spec,
            CInput input
            )    
        {
            int i;
            CNfa elem;
            int size;

            /* Set member variables. */
            reset();
            Set(lexGen,spec,input);

            size = m_spec.m_states.Count;
            m_spec.m_state_rules = new Vector[size];
            for (i = 0; i < size; ++i)
            {
                m_spec.m_state_rules[i] = new Vector();
            }

            /* Initialize current token variable 
               and create nfa. */
            /*m_spec.m_current_token = m_lexGen.EOS;
            m_lexGen.advance();*/

            m_spec.m_nfa_start = machine();
	  
            /* Set labels in created nfa machine. */
            size = m_spec.m_nfa_states.size();
            for (i = 0; i < size; ++i)
            {
                elem = (CNfa) m_spec.m_nfa_states.elementAt(i);
                elem.m_label = i;
            }

            /* Debugging output. */
            if (CUtility.DO_DEBUG)
            {
                m_lexGen.print_nfa();
            }

            if (m_spec.m_verbose)
            {
                System.Console.WriteLine("NFA comprised of " 
                    + (m_spec.m_nfa_states.Count + 1) 
                    + " states.");
            }

            reset();
        }
     
        /***************************************************************
          Function: discardCNfa
          Description: 
          **************************************************************/
        private void discardCNfa
            (
            CNfa nfa
            )
        {
            m_spec.m_nfa_states.removeElement(nfa);
        }

        /***************************************************************
          Function: processStates
          Description:
          **************************************************************/
        private void processStates
            (
            SparseBitSet states,
            CNfa current
            )
        {
            int size;
            int i;
	
            size = m_spec.m_states.Count;
            for (i = 0; i <  size; ++i)
            {
                if (states.Get(i))
                {
                    m_spec.m_state_rules[i].addElement(current);
                }
            }
        }

        /***************************************************************
          Function: machine
          Description: Recursive descent regular expression parser.
          **************************************************************/
        private CNfa machine
            (
            )
        {
            CNfa start;
            CNfa p;
            SparseBitSet states;

            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.enter("machine",m_spec.m_lexeme,m_spec.m_current_token);
            }

            start = CAlloc.newCNfa(m_spec);
            p = start;
	    
            states = m_lexGen.getStates();

            /* Begin: Added for states. */
            m_spec.m_current_token = CLexGen.EOS;
            m_lexGen.advance();
            /* End: Added for states. */
	
            if (CLexGen.END_OF_INPUT != m_spec.m_current_token) // CSA fix.
            {
                p.m_next = rule();
	    
                processStates(states,p.m_next);
            }

            while (CLexGen.END_OF_INPUT != m_spec.m_current_token)
            {
                /* Make state changes HERE. */
                states = m_lexGen.getStates();
	
                /* Begin: Added for states. */
                m_lexGen.advance();
                if (CLexGen.END_OF_INPUT == m_spec.m_current_token)
                { 
                    break;
                }
                /* End: Added for states. */
	    
                p.m_next2 = CAlloc.newCNfa(m_spec);
                p = p.m_next2;
                p.m_next = rule();
	    
                processStates(states,p.m_next);
            }

            // CSA: add pseudo-rules for BOL and EOF
            SparseBitSet all_states = new SparseBitSet();
            for (int i = 0; i < m_spec.m_states.Count; ++i)
                all_states.Set(i);
            p.m_next2 = CAlloc.newCNfa(m_spec);
            p = p.m_next2;
            p.m_next = CAlloc.newCNfa(m_spec);
            p.m_next.m_edge = CNfa.CCL;
            p.m_next.m_next = CAlloc.newCNfa(m_spec);
            p.m_next.m_set = new CSet();
            p.m_next.m_set.add(m_spec.BOL);
            p.m_next.m_set.add(m_spec.EOF);
            p.m_next.m_next.m_accept = // do-nothing accept rule
                new CAccept(new char[0], 0, m_input.m_line_number+1);
            processStates(all_states,p.m_next);
            // CSA: done. 

            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.leave("machine",m_spec.m_lexeme,m_spec.m_current_token);
            }

            return start;
        }
  
        /***************************************************************
          Function: rule
          Description: Recursive descent regular expression parser.
          **************************************************************/
        private CNfa rule
            (
            )
        {
            CNfaPair pair; 
            //CNfa p;
            CNfa start = null;
            CNfa end = null;
            int anchor = CSpec.NONE;

            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.enter("rule",m_spec.m_lexeme,m_spec.m_current_token);
            }

            pair = CAlloc.newCNfaPair();

            if (CLexGen.AT_BOL == m_spec.m_current_token)
            {
                anchor = anchor | CSpec.START;
                m_lexGen.advance();
                expr(pair);

                // CSA: fixed beginning-of-line operator. 8-aug-1999
                start = CAlloc.newCNfa(m_spec);
                start.m_edge = m_spec.BOL;
                start.m_next = pair.m_start;
                end = pair.m_end;
            }
            else
            {
                expr(pair);
                start = pair.m_start;
                end = pair.m_end;
            }

            if (CLexGen.AT_EOL == m_spec.m_current_token)
            {
                m_lexGen.advance();
                // CSA: fixed end-of-line operator. 8-aug-1999
                CNfaPair nlpair = CAlloc.newNLPair(m_spec);
                end.m_next = CAlloc.newCNfa(m_spec);
                end.m_next.m_next = nlpair.m_start;
                end.m_next.m_next2 = CAlloc.newCNfa(m_spec);
                end.m_next.m_next2.m_edge = m_spec.EOF;
                end.m_next.m_next2.m_next = nlpair.m_end;
                end = nlpair.m_end;
                anchor = anchor | CSpec.END;
            }

            /* Check for null rules. Charles Fischer found this bug. [CSA] */
            if (end==null)
                CError.parse_error(CError.E_ZERO, m_input.m_line_number);

            /* Handle end of regular expression.  See page 103. */
            end.m_accept = m_lexGen.packAccept();
            end.m_anchor = anchor;

            /* Begin: Removed for states. */
            /*m_lexGen.advance();*/
            /* End: Removed for states. */

            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.leave("rule",m_spec.m_lexeme,m_spec.m_current_token);
            }

            return start;
        }
	    
        /***************************************************************
          Function: expr
          Description: Recursive descent regular expression parser.
          **************************************************************/
        private void expr
            (
            CNfaPair pair
            )
        {
            CNfaPair e2_pair;
            CNfa p;
	
            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.enter("expr",m_spec.m_lexeme,m_spec.m_current_token);
            }

            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != pair);
            }

            e2_pair = CAlloc.newCNfaPair();

            cat_expr(pair);
	
            while (CLexGen.OR == m_spec.m_current_token)
            {
                m_lexGen.advance();
                cat_expr(e2_pair);

                p = CAlloc.newCNfa(m_spec);
                p.m_next2 = e2_pair.m_start;
                p.m_next = pair.m_start;
                pair.m_start = p;
	    
                p = CAlloc.newCNfa(m_spec);
                pair.m_end.m_next = p;
                e2_pair.m_end.m_next = p;
                pair.m_end = p;
            }

            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.leave("expr",m_spec.m_lexeme,m_spec.m_current_token);
            }
        }
	    
        /***************************************************************
          Function: cat_expr
          Description: Recursive descent regular expression parser.
          **************************************************************/
        private void cat_expr
            (
            CNfaPair pair
            )
        {
            CNfaPair e2_pair;

            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.enter("cat_expr",m_spec.m_lexeme,m_spec.m_current_token);
            }

            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != pair);
            }
	
            e2_pair = CAlloc.newCNfaPair();
	
            if (first_in_cat(m_spec.m_current_token))
            {
                factor(pair);
            }

            while (first_in_cat(m_spec.m_current_token))
            {
                factor(e2_pair);

                /* Destroy */
                pair.m_end.mimic(e2_pair.m_start);
                discardCNfa(e2_pair.m_start);
	    
                pair.m_end = e2_pair.m_end;
            }

            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.leave("cat_expr",m_spec.m_lexeme,m_spec.m_current_token);
            }
        }
  
        /***************************************************************
          Function: first_in_cat
          Description: Recursive descent regular expression parser.
          **************************************************************/
        private bool first_in_cat
            (
            int token
            )
        {
            switch (token)
            {
                case CLexGen.CLOSE_PAREN:
                case CLexGen.AT_EOL:
                case CLexGen.OR:
                case CLexGen.EOS:
                    return false;
	    
                case CLexGen.CLOSURE:
                case CLexGen.PLUS_CLOSE:
                case CLexGen.OPTIONAL:
                    CError.parse_error(CError.E_CLOSE,m_input.m_line_number);
                    return false;

                case CLexGen.CCL_END:
                    CError.parse_error(CError.E_BRACKET,m_input.m_line_number);
                    return false;

                case CLexGen.AT_BOL:
                    CError.parse_error(CError.E_BOL,m_input.m_line_number);
                    return false;

                default:
                    break;
            }

            return true;
        }

        /***************************************************************
          Function: factor
          Description: Recursive descent regular expression parser.
          **************************************************************/
        private void factor
            (
            CNfaPair pair
            )
        {
            CNfa start = null;
            CNfa end = null;

            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.enter("factor",m_spec.m_lexeme,m_spec.m_current_token);
            }

            term(pair);

            if (CLexGen.CLOSURE == m_spec.m_current_token
                || CLexGen.PLUS_CLOSE == m_spec.m_current_token
                || CLexGen.OPTIONAL == m_spec.m_current_token)
            {
                start = CAlloc.newCNfa(m_spec);
                end = CAlloc.newCNfa(m_spec);
	    
                start.m_next = pair.m_start;
                pair.m_end.m_next = end;

                if (CLexGen.CLOSURE == m_spec.m_current_token
                    || CLexGen.OPTIONAL == m_spec.m_current_token)
                {
                    start.m_next2 = end;
                }
	    
                if (CLexGen.CLOSURE == m_spec.m_current_token
                    || CLexGen.PLUS_CLOSE == m_spec.m_current_token)
                {
                    pair.m_end.m_next2 = pair.m_start;
                }
	    
                pair.m_start = start;
                pair.m_end = end;
                m_lexGen.advance();
            }

            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.leave("factor",m_spec.m_lexeme,m_spec.m_current_token);
            }
        }
      
        /***************************************************************
          Function: term
          Description: Recursive descent regular expression parser.
          **************************************************************/
        private void term
            (
            CNfaPair pair
            )
        {
            CNfa start;
            bool isAlphaL;
           // int c;

            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.enter("term",m_spec.m_lexeme,m_spec.m_current_token);
            }

            if (CLexGen.OPEN_PAREN == m_spec.m_current_token)
            {
                m_lexGen.advance();
                expr(pair);

                if (CLexGen.CLOSE_PAREN == m_spec.m_current_token)
                {
                    m_lexGen.advance();
                }
                else
                {
                    CError.parse_error(CError.E_SYNTAX,m_input.m_line_number);
                }
            }
            else
            {
                start = CAlloc.newCNfa(m_spec);
                pair.m_start = start;

                start.m_next = CAlloc.newCNfa(m_spec);
                pair.m_end = start.m_next;

                if (CLexGen.L == m_spec.m_current_token &&
                    Char.IsLetter(m_spec.m_lexeme)) 
                {
                    isAlphaL = true;
                } 
                else 
                {
                    isAlphaL = false;
                }
                if (false == (CLexGen.ANY == m_spec.m_current_token
                    || CLexGen.CCL_START == m_spec.m_current_token
                    || (m_spec.m_ignorecase && isAlphaL)))
                {
                    start.m_edge = m_spec.m_lexeme;
                    m_lexGen.advance();
                }
                else
                {
                    start.m_edge = CNfa.CCL;
		
                    start.m_set = new CSet();

                    /* Match case-insensitive letters using character class. */
                    if (m_spec.m_ignorecase && isAlphaL) 
                    {
                        start.m_set.addncase(m_spec.m_lexeme);
                    }
                        /* Match dot (.) using character class. */
                    else if (CLexGen.ANY == m_spec.m_current_token)
                    {
                        start.m_set.add('\n');
                        start.m_set.add('\r');
                        // CSA: exclude BOL and EOF from character classes
                        start.m_set.add(m_spec.BOL);
                        start.m_set.add(m_spec.EOF);
                        start.m_set.complement();
                    }
                    else
                    {
                        m_lexGen.advance();
                        if (CLexGen.AT_BOL == m_spec.m_current_token)
                        {
                            m_lexGen.advance();

                            // CSA: exclude BOL and EOF from character classes
                            start.m_set.add(m_spec.BOL);
                            start.m_set.add(m_spec.EOF);
                            start.m_set.complement();
                        }
                        if (false == (CLexGen.CCL_END == m_spec.m_current_token))
                        {
                            dodash(start.m_set);
                        }
                        /*else
                          {
                        for (c = 0; c <= ' '; ++c)
                          {
                            start.m_set.add((byte) c);
                          }
                          }*/
                    }
                    m_lexGen.advance();
                }
            }

            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.leave("term",m_spec.m_lexeme,m_spec.m_current_token);
            }
        }

        /***************************************************************
          Function: dodash
          Description: Recursive descent regular expression parser.
          **************************************************************/
        private void dodash
            (
            CSet Set
            )
        {
            int first = -1;
	  
            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.enter("dodash",m_spec.m_lexeme,m_spec.m_current_token);
            }
	  
            while (CLexGen.EOS != m_spec.m_current_token 
                && CLexGen.CCL_END != m_spec.m_current_token)
            {
                // DASH loses its special meaning if it is first in class.
                if (CLexGen.DASH == m_spec.m_current_token && -1 != first)
                {
                    m_lexGen.advance();
                    // DASH loses its special meaning if it is last in class.
                    if (m_spec.m_current_token == CLexGen.CCL_END)
                    {
                        // 'first' already in Set.
                        Set.add('-');
                        break;
                    }
                    for ( ; first <= m_spec.m_lexeme; ++first)
                    {
                        if (m_spec.m_ignorecase) 
                            Set.addncase((char)first);
                        else
                            Set.add(first);
                    }  
                }
                else
                {
                    first = m_spec.m_lexeme;
                    if (m_spec.m_ignorecase)
                        Set.addncase(m_spec.m_lexeme);
                    else
                        Set.add(m_spec.m_lexeme);
                }

                m_lexGen.advance();
            }
	  
            if (CUtility.DESCENT_DEBUG)
            {
                CUtility.leave("dodash",m_spec.m_lexeme,m_spec.m_current_token);
            }
        }
    }

}
