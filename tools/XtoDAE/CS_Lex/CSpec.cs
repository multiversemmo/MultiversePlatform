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
	/// Summary description for CSpec.
	/// </summary>
    public class CSpec
    {
        /***************************************************************
          Member Variables
          **************************************************************/
    
        /* Lexical States. */
        public Hashtable m_states; /* Hashtable taking state indices (Integer) 
			 to state name (string). */

        /* Regular Expression Macros. */ 
        public Hashtable m_macros; /* Hashtable taking macro name (string)
				to corresponding char buffer that
				holds macro definition. */

        /* NFA Machine. */
        public CNfa m_nfa_start; /* Start state of NFA machine. */
        public Vector m_nfa_states; /* Vector of states, with index
				 corresponding to label. */
  
        public Vector[] m_state_rules; /* An array of Vectors of Integers.
				    The ith Vector represents the lexical state
				    with index i.  The contents of the ith 
				    Vector are the indices of the NFA start
				    states that can be matched while in
				    the ith lexical state. */
				    

        public int[] m_state_dtrans;

        /* DFA Machine. */
        public Vector m_dfa_states; /* Vector of states, with index
				 corresponding to label. */
        public Hashtable m_dfa_sets; /* Hashtable taking Set of NFA states
				  to corresponding DFA state, 
				  if the latter exists. */
  
        /* Accept States and Corresponding Anchors. */
        public Vector m_accept_vector;
        public int[] m_anchor_array;

        /* Transition Table. */
       public  Vector m_dtrans_vector;
       public  int m_dtrans_ncols;
        public int[] m_row_map;
       public  int[] m_col_map;

        /* Special pseudo-characters for beginning-of-line and end-of-file. */
        public const int NUM_PSEUDO=2;
        public int BOL; // beginning-of-line
        public int EOF; // end-of-line

        /** NFA character class minimization map. */
        public int[] m_ccls_map;

        /* Regular expression token variables. */
        public int m_current_token;
        public char m_lexeme;
        public bool m_in_quote;
        public bool m_in_ccl;

        /* Verbose execution flag. */
        public bool m_verbose;

        /* JLex directives flags. */
        public bool m_integer_type;
        public bool m_intwrap_type;
        public bool m_yyeof;
       public  bool m_count_chars;
       public  bool m_count_lines;
      public   bool m_cup_compatible;
       public  bool m_unix;
        public bool m_public;
        public bool m_ignorecase;

        public char[] m_init_code;
        public int m_init_read;

        public char[] m_init_throw_code;
        public int m_init_throw_read;

      public   char[] m_class_code;
       public  int m_class_read;

        public char[] m_eof_code;
        public int m_eof_read;

        public char[] m_eof_value_code;
       public  int m_eof_value_read;

       public  char[] m_eof_throw_code;
        public int m_eof_throw_read;

       public  char[] m_yylex_throw_code;
       public  int m_yylex_throw_read;

        /* Class, function, type names. */
       public  char[] m_class_name = {          
                                  'Y', 'y', 'l', 
                                  'e', 'x' 
                              };
        public char[] m_implements_name = {};
        public char[] m_function_name = {
                                     'y', 'y', 'l', 
                                     'e', 'x' 
                                 };
        public char[] m_type_name = {
                                 'Y', 'y', 't', 
                                 'o', 'k', 'e',
                                 'n'
                             };

        /* Lexical Generator. */
        private CLexGen m_lexGen;

        /***************************************************************
          Constants
          ***********************************************************/
       public  const int NONE = 0;
        public const int START = 1;
        public const int END = 2;
  
        /***************************************************************
          Function: CSpec
          Description: Constructor.
          **************************************************************/
        public CSpec
            (
            CLexGen lexGen
            )
        {
            m_lexGen = lexGen;

            /* Initialize regular expression token variables. */
            m_current_token = CLexGen.EOS;
            m_lexeme = '\0';
            m_in_quote = false;
            m_in_ccl = false;

            /* Initialize hashtable for lexer states. */
            m_states = new Hashtable();
            m_states.Add("YYINITIAL",m_states.Count);

            /* Initialize hashtable for lexical macros. */
            m_macros = new Hashtable();

            /* Initialize variables for lexer options. */
            m_integer_type = false;
            m_intwrap_type = false;
            m_count_lines = false;
            m_count_chars = false;
            m_cup_compatible = false;
            m_unix = true;
            m_public = false;
            m_yyeof = false;
            m_ignorecase = false;

            /* Initialize variables for JLex runtime options. */
            m_verbose = true;

            m_nfa_start = null;
            m_nfa_states = new Vector();
	
            m_dfa_states = new Vector();
            m_dfa_sets = new Hashtable();

            m_dtrans_vector = new Vector();
            m_dtrans_ncols = CUtility.MAX_SEVEN_BIT + 1;
            m_row_map = null;
            m_col_map = null;

            m_accept_vector = null;
            m_anchor_array = null;

            m_init_code = null;
            m_init_read = 0;

            m_init_throw_code = null;
            m_init_throw_read = 0;

            m_yylex_throw_code = null;
            m_yylex_throw_read = 0;

            m_class_code = null;
            m_class_read = 0;

            m_eof_code = null;
            m_eof_read = 0;

            m_eof_value_code = null;
            m_eof_value_read = 0;

            m_eof_throw_code = null;
            m_eof_throw_read = 0;

            m_state_dtrans = null;

            m_state_rules = null;
        }
    }

}
