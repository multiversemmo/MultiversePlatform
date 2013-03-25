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
using System.IO;
using System.Collections;
using System.Text;

namespace TUVienna.CS_Lex
{
	/// <summary>
	/// Summary description for CEmit.
	/// </summary>
    public class CEmit
    {
        /***************************************************************
          Member Variables
          **************************************************************/
        private CSpec m_spec;
        private TextWriter m_outstream;

        /***************************************************************
          Constants: Anchor Types
          **************************************************************/
        private const int START = 1;
        private const int END = 2;
        private const int NONE = 4;

        /***************************************************************
          Constants
          **************************************************************/
        private const bool EDBG = true;
        private const bool NOT_EDBG = false;

        /***************************************************************
          Function: CEmit
          Description: Constructor.
          **************************************************************/
        public CEmit
            (
            )
        {
            reset();
        }

        /***************************************************************
          Function: reset
          Description: Clears member variables.
          **************************************************************/
        private void reset
            (
            )
        {
            m_spec = null;
            m_outstream = null;
        }

        /***************************************************************
          Function: Set
          Description: Initializes member variables.
          **************************************************************/
        private void Set
            (
            CSpec spec,
            TextWriter outstream
            )
        {
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != spec);
                CUtility.ASSERT(null != outstream);
            }

            m_spec = spec;
            m_outstream = outstream;
        }

  
        /***************************************************************
          Function: print_details
          Description: Debugging output.
          **************************************************************/
        private void print_details
            (
            )
        {
            int i;
            int j;
            int next;
            int state;
            CDTrans dtrans;
            CAccept accept;
            bool tr;

            System.Console.WriteLine("---------------------- Transition Table " 
                + "----------------------");
	
            for (i = 0; i < m_spec.m_row_map.Length; ++i)
            {
                System.Console.Write("State " + i);
	    
                accept = (CAccept) m_spec.m_accept_vector.elementAt(i);
                if (null == accept)
                {
                    System.Console.WriteLine(" [nonaccepting]");
                }
                else
                {
                    System.Console.WriteLine(" [accepting, line "
                        + accept.m_line_number 
                        + " <"
                        + (new string(accept.m_action,0,accept.m_action_read))
                        + ">]");
                }
                dtrans = (CDTrans) m_spec.m_dtrans_vector.elementAt(m_spec.m_row_map[i]);
	    
                tr = false;
                state = dtrans.m_dtrans[m_spec.m_col_map[0]];
                if (CDTrans.F != state)
                {
                    tr = true;
                    System.Console.Write("\tgoto " + state + " on [" + ((char) 0));
                }
                for (j = 1; j < m_spec.m_dtrans_ncols; ++j)
                {
                    next = dtrans.m_dtrans[m_spec.m_col_map[j]];
                    if (state == next)
                    {
                        if (CDTrans.F != state)
                        {
                            System.Console.Write((char) j);
                        }
                    }
                    else
                    {
                        state = next;
                        if (tr)
                        {
                            System.Console.WriteLine("]");
                            tr = false;
                        }
                        if (CDTrans.F != state)
                        {
                            tr = true;
                            System.Console.Write("\tgoto " + state + " on [" + ((char) j));
                        }
                    }
                }
                if (tr)
                {
                    System.Console.WriteLine("]");
                }
            }

            System.Console.WriteLine("---------------------- Transition Table " 
                + "----------------------");
        }

        /***************************************************************
          Function: emit
          Description: High-level access function to module.
          **************************************************************/
      public   void emit
            (
            CSpec spec,
            System.IO.TextWriter outstream
            )    
        {
            Set(spec,outstream);
	  
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != m_spec);
                CUtility.ASSERT(null != m_outstream);
            }
	  
            if (CUtility.OLD_DEBUG) 
            {
                print_details();
            }

            emit_header();
            emit_construct();
            emit_helpers();
            emit_driver();
            emit_footer();
	  
            reset();
        }

        /***************************************************************
          Function: emit_construct
          Description: Emits constructor, member variables,
          and constants.
          **************************************************************/
        private void emit_construct
            (
            )
        {
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != m_spec);
                CUtility.ASSERT(null != m_outstream);
            }
	  
            /* Constants */
            m_outstream.WriteLine("\tprivate const int YY_BUFFER_SIZE = 512;");

            m_outstream.WriteLine("\tprivate const int YY_F = -1;");
            m_outstream.WriteLine("\tprivate const int YY_NO_STATE = -1;");

            m_outstream.WriteLine("\tprivate const int YY_NOT_ACCEPT = 0;");
            m_outstream.WriteLine("\tprivate const int YY_START = 1;");
            m_outstream.WriteLine("\tprivate const int YY_END = 2;");
            m_outstream.WriteLine("\tprivate const int YY_NO_ANCHOR = 4;");

            // internal
            m_outstream.WriteLine("\tprivate const int YY_BOL = "+m_spec.BOL+";");
            m_outstream.WriteLine("\tprivate const int YY_EOF = "+m_spec.EOF+";");
            // external
            if (m_spec.m_integer_type || true == m_spec.m_yyeof)
                m_outstream.WriteLine("\tpublic const int YYEOF = -1;");
	  
            /* User specified class code. */
            if (null != m_spec.m_class_code)
            {
                m_outstream.Write(new string(m_spec.m_class_code,0,
                    m_spec.m_class_read));
            }

            /* Member Variables */
            m_outstream.WriteLine("\tprivate System.IO.TextReader yy_reader;");
            m_outstream.WriteLine("\tprivate int yy_buffer_index;");
            m_outstream.WriteLine("\tprivate int yy_buffer_read;");
            m_outstream.WriteLine("\tprivate int yy_buffer_start;");
            m_outstream.WriteLine("\tprivate int yy_buffer_end;");
            m_outstream.WriteLine("\tprivate char[] yy_buffer;");
            if (m_spec.m_count_chars)
            {
                m_outstream.WriteLine("\tprivate int yychar;");
            }
            if (m_spec.m_count_lines)
            {
                m_outstream.WriteLine("\tprivate int yyline;");
            }
            m_outstream.WriteLine("\tprivate bool yy_at_bol;");
            m_outstream.WriteLine("\tprivate int yy_lexical_state;");
            /*if (m_spec.m_count_lines || true == m_spec.m_count_chars)
              {
                m_outstream.WriteLine("\tprivate int yy_buffer_prev_start;");
              }*/
            m_outstream.WriteLine();

	  
            /* Function: first constructor (Reader) */
            m_outstream.Write("\t");
                m_outstream.Write("public ");
            m_outstream.Write(new string(m_spec.m_class_name));
            m_outstream.Write(" (System.IO.TextReader yy_reader1) : this()");
	  
            //SI:
            /*if (null != m_spec.m_init_throw_code)
            {
                m_outstream.WriteLine(); 
                m_outstream.Write("\t\tthrows "); 
                m_outstream.Write(new string(m_spec.m_init_throw_code,0,
                    m_spec.m_init_throw_read));
                m_outstream.WriteLine();
                m_outstream.WriteLine("\t\t{");
            }
            else
            {*/
                m_outstream.WriteLine(" {");
           

         //SI:   m_outstream.WriteLine("\t\tthis ();");	  
            m_outstream.WriteLine("\t\tif (null == yy_reader1) {");
            m_outstream.WriteLine("\t\t\tthrow (new System.Exception(\"Error: Bad input "
                + "stream initializer.\"));");
            m_outstream.WriteLine("\t\t}");
            m_outstream.WriteLine("\t\tyy_reader = yy_reader1;");
            m_outstream.WriteLine("\t}");
            m_outstream.WriteLine();


            /* 
            m_outstream.Write("\t");
            if (true == m_spec.m_public) 
            {
                m_outstream.Write("public ");
            }
            m_outstream.Write(new string(m_spec.m_class_name));
            m_outstream.Write(" (java.io.InputStream instream)");
	  
            if (null != m_spec.m_init_throw_code)
            {
                m_outstream.WriteLine(); 
                m_outstream.Write("\t\tthrows "); 
                m_outstream.WriteLine(new string(m_spec.m_init_throw_code,0,
                    m_spec.m_init_throw_read));
                m_outstream.WriteLine("\t\t{");
            }
            else
            {
                m_outstream.WriteLine(" {");
            }
	  
            m_outstream.WriteLine("\t\tthis ();");	  
            m_outstream.WriteLine("\t\tif (null == instream) {");
            m_outstream.WriteLine("\t\t\tthrow (new Error(\"Error: Bad input "
                + "stream initializer.\"));");
            m_outstream.WriteLine("\t\t}");
            m_outstream.WriteLine("\t\tyy_reader = new java.io.BufferedReader(new java.io.InputStreamReader(instream));");
            m_outstream.WriteLine("\t}");
            m_outstream.WriteLine();
        */

            /* Function: third, private constructor - only for internal use */
            m_outstream.Write("\tprivate ");
            m_outstream.Write(new string(m_spec.m_class_name));
            m_outstream.Write(" ()");
	  
            //SI:throw code not necceasary
            /*if (null != m_spec.m_init_throw_code)
            {
                m_outstream.WriteLine(); 
                m_outstream.Write("\t\tthrows "); 
                m_outstream.WriteLine(new string(m_spec.m_init_throw_code,0,
                    m_spec.m_init_throw_read));
                m_outstream.WriteLine("\t\t{");
            }
            else
            {*/
            m_outstream.WriteLine(" {");
            
	  
            m_outstream.WriteLine("\t\tyy_buffer = new char[YY_BUFFER_SIZE];");
            m_outstream.WriteLine("\t\tyy_buffer_read = 0;");
            m_outstream.WriteLine("\t\tyy_buffer_index = 0;");
            m_outstream.WriteLine("\t\tyy_buffer_start = 0;");
            m_outstream.WriteLine("\t\tyy_buffer_end = 0;");
            if (m_spec.m_count_chars)
            {
                m_outstream.WriteLine("\t\tyychar = 0;");
            }
            if (m_spec.m_count_lines)
            {
                m_outstream.WriteLine("\t\tyyline = 0;");
            }
            m_outstream.WriteLine("\t\tyy_at_bol = true;");
            m_outstream.WriteLine("\t\tyy_lexical_state = YYINITIAL;");
            /*if (m_spec.m_count_lines || true == m_spec.m_count_chars)
              {
                m_outstream.WriteLine("\t\tyy_buffer_prev_start = 0;");
              }*/

            /* User specified constructor code. */
            if (null != m_spec.m_init_code)
            {
                m_outstream.Write(new string(m_spec.m_init_code,0,
                    m_spec.m_init_read));
            }

            m_outstream.WriteLine("\t}");
            m_outstream.WriteLine();

        }

        /***************************************************************
          Function: emit_states
          Description: Emits constants that serve as lexical states,
          including YYINITIAL.
          **************************************************************/
        private void emit_states
            (
            )
        {
            IEnumerator states;
            string state;
            int index;

            states = m_spec.m_states.Keys.GetEnumerator();
            /*index = 0;*/
            while (states.MoveNext())
            {
                state = (string) states.Current;
	      
                if (CUtility.DEBUG)
                {
                    CUtility.ASSERT(null != state);
                }
	      
                m_outstream.WriteLine("\tprivate const int " 
                    + state 
                    + " = " 
                    + (m_spec.m_states[state]).ToString() 
                    + ";");
                /*++index;*/
            }

            m_outstream.WriteLine("\tprivate static readonly int[] yy_state_dtrans =new int[] {");
            for (index = 0; index < m_spec.m_state_dtrans.Length; ++index)
            {
                m_outstream.Write("\t\t" + m_spec.m_state_dtrans[index]);
                if (index < m_spec.m_state_dtrans.Length - 1)
                {
                    m_outstream.WriteLine(",");
                }
                else
                {
                    m_outstream.WriteLine();
                }
            }
            m_outstream.WriteLine("\t};");
        }

        /***************************************************************
          Function: emit_helpers
          Description: Emits helper functions, particularly 
          error handling and input buffering.
          **************************************************************/
        private void emit_helpers
            (
            )
        {
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != m_spec);
                CUtility.ASSERT(null != m_outstream);
            }

            /* Function: yy_do_eof */
            m_outstream.WriteLine("\tprivate bool yy_eof_done = false;");
            if (null != m_spec.m_eof_code)
            {
                m_outstream.Write("\tprivate void yy_do_eof ()");

                //SI:throw code not necessary
              /*  if (null != m_spec.m_eof_throw_code)
                {
                    m_outstream.WriteLine(); 
                    m_outstream.Write("\t\tthrows "); 
                    m_outstream.WriteLine(new string(m_spec.m_eof_throw_code,0,
                        m_spec.m_eof_throw_read));
                    m_outstream.WriteLine("\t\t{");
                }
                else
                {*/
                m_outstream.WriteLine(" {");
                

                m_outstream.WriteLine("\t\tif (false == yy_eof_done) {");
                m_outstream.Write(new string(m_spec.m_eof_code,0,
                    m_spec.m_eof_read));
                m_outstream.WriteLine("\t\t}");
                m_outstream.WriteLine("\t\tyy_eof_done = true;");
                m_outstream.WriteLine("\t}");
            }

            emit_states();
	
            /* Function: yybegin */
            m_outstream.WriteLine("\tprivate void yybegin (int state) {");
            m_outstream.WriteLine("\t\tyy_lexical_state = state;");
            m_outstream.WriteLine("\t}");


            /* Function: yy_advance */
            m_outstream.WriteLine("\tprivate int yy_advance ()");
       //SI:     m_outstream.WriteLine("\t\tthrows java.io.IOException {");
            /*m_outstream.WriteLine("\t\t{");*/
            m_outstream.WriteLine("\t{");
            m_outstream.WriteLine("\t\tint next_read;");
            m_outstream.WriteLine("\t\tint i;");
            m_outstream.WriteLine("\t\tint j;");
            m_outstream.WriteLine();

            m_outstream.WriteLine("\t\tif (yy_buffer_index < yy_buffer_read) {");
            m_outstream.WriteLine("\t\t\treturn yy_buffer[yy_buffer_index++];");
            /*m_outstream.WriteLine("\t\t\t++yy_buffer_index;");*/
            m_outstream.WriteLine("\t\t}");
            m_outstream.WriteLine();

            m_outstream.WriteLine("\t\tif (0 != yy_buffer_start) {");
            m_outstream.WriteLine("\t\t\ti = yy_buffer_start;");
            m_outstream.WriteLine("\t\t\tj = 0;");
            m_outstream.WriteLine("\t\t\twhile (i < yy_buffer_read) {");
            m_outstream.WriteLine("\t\t\t\tyy_buffer[j] = yy_buffer[i];");
            m_outstream.WriteLine("\t\t\t\t++i;");
            m_outstream.WriteLine("\t\t\t\t++j;");
            m_outstream.WriteLine("\t\t\t}");
            m_outstream.WriteLine("\t\t\tyy_buffer_end = yy_buffer_end - yy_buffer_start;");
            m_outstream.WriteLine("\t\t\tyy_buffer_start = 0;");
            m_outstream.WriteLine("\t\t\tyy_buffer_read = j;");
            m_outstream.WriteLine("\t\t\tyy_buffer_index = j;");
            m_outstream.WriteLine("\t\t\tnext_read = yy_reader.Read(yy_buffer,");
            m_outstream.WriteLine("\t\t\t\t\tyy_buffer_read,");
            m_outstream.WriteLine("\t\t\t\t\tyy_buffer.Length - yy_buffer_read);");
            m_outstream.WriteLine("\t\t\tif ( next_read<=0) {");
            m_outstream.WriteLine("\t\t\t\treturn YY_EOF;");
            m_outstream.WriteLine("\t\t\t}");
            m_outstream.WriteLine("\t\t\tyy_buffer_read = yy_buffer_read + next_read;");
            m_outstream.WriteLine("\t\t}");
            m_outstream.WriteLine();

            m_outstream.WriteLine("\t\twhile (yy_buffer_index >= yy_buffer_read) {");
            m_outstream.WriteLine("\t\t\tif (yy_buffer_index >= yy_buffer.Length) {");
            m_outstream.WriteLine("\t\t\t\tyy_buffer = yy_double(yy_buffer);");
            m_outstream.WriteLine("\t\t\t}");
            m_outstream.WriteLine("\t\t\tnext_read = yy_reader.Read(yy_buffer,");
            m_outstream.WriteLine("\t\t\t\t\tyy_buffer_read,");
            m_outstream.WriteLine("\t\t\t\t\tyy_buffer.Length - yy_buffer_read);");
            m_outstream.WriteLine("\t\t\tif ( next_read<=0) {");
            m_outstream.WriteLine("\t\t\t\treturn YY_EOF;");
            m_outstream.WriteLine("\t\t\t}");
            m_outstream.WriteLine("\t\t\tyy_buffer_read = yy_buffer_read + next_read;");
            m_outstream.WriteLine("\t\t}");

            m_outstream.WriteLine("\t\treturn yy_buffer[yy_buffer_index++];");
            m_outstream.WriteLine("\t}");
	
            /* Function: yy_move_end */
            m_outstream.WriteLine("\tprivate void yy_move_end () {");
            m_outstream.WriteLine("\t\tif (yy_buffer_end > yy_buffer_start &&");
            m_outstream.WriteLine("\t\t    '\\n' == yy_buffer[yy_buffer_end-1])");
            m_outstream.WriteLine("\t\t\tyy_buffer_end--;");
            m_outstream.WriteLine("\t\tif (yy_buffer_end > yy_buffer_start &&");
            m_outstream.WriteLine("\t\t    '\\r' == yy_buffer[yy_buffer_end-1])");
            m_outstream.WriteLine("\t\t\tyy_buffer_end--;");
            m_outstream.WriteLine("\t}");

            /* Function: yy_mark_start */
            m_outstream.WriteLine("\tprivate bool yy_last_was_cr=false;");
            m_outstream.WriteLine("\tprivate void yy_mark_start () {");
            if (m_spec.m_count_lines || true == m_spec.m_count_chars)
            {
                if (m_spec.m_count_lines)
                {
                    m_outstream.WriteLine("\t\tint i;");
                    m_outstream.WriteLine("\t\tfor (i = yy_buffer_start; " 
                        + "i < yy_buffer_index; ++i) {");
                    m_outstream.WriteLine("\t\t\tif ('\\n' == yy_buffer[i] && !yy_last_was_cr) {");
                    m_outstream.WriteLine("\t\t\t\t++yyline;");
                    m_outstream.WriteLine("\t\t\t}");
                    m_outstream.WriteLine("\t\t\tif ('\\r' == yy_buffer[i]) {");
                    m_outstream.WriteLine("\t\t\t\t++yyline;");
                    m_outstream.WriteLine("\t\t\t\tyy_last_was_cr=true;");
                    m_outstream.WriteLine("\t\t\t} else yy_last_was_cr=false;");
                    m_outstream.WriteLine("\t\t}");
                }
                if (m_spec.m_count_chars)
                {
                    m_outstream.WriteLine("\t\tyychar = yychar"); 
                    m_outstream.WriteLine("\t\t\t+ yy_buffer_index - yy_buffer_start;");
                }
            }
            m_outstream.WriteLine("\t\tyy_buffer_start = yy_buffer_index;");
            m_outstream.WriteLine("\t}");

            /* Function: yy_mark_end */
            m_outstream.WriteLine("\tprivate void yy_mark_end () {");
            m_outstream.WriteLine("\t\tyy_buffer_end = yy_buffer_index;");
            m_outstream.WriteLine("\t}");

            /* Function: yy_to_mark */
            m_outstream.WriteLine("\tprivate void yy_to_mark () {");
            m_outstream.WriteLine("\t\tyy_buffer_index = yy_buffer_end;");
            m_outstream.WriteLine("\t\tyy_at_bol = "+
                "(yy_buffer_end > yy_buffer_start) &&");
            m_outstream.WriteLine("\t\t            "+
                "('\\r' == yy_buffer[yy_buffer_end-1] ||");
            m_outstream.WriteLine("\t\t            "+
                " '\\n' == yy_buffer[yy_buffer_end-1] ||");
            m_outstream.WriteLine("\t\t            "+ /* unicode LS */
                " 2028/*LS*/ == yy_buffer[yy_buffer_end-1] ||");
            m_outstream.WriteLine("\t\t            "+ /* unicode PS */
                " 2029/*PS*/ == yy_buffer[yy_buffer_end-1]);");
            m_outstream.WriteLine("\t}");

            /* Function: yytext */
            m_outstream.WriteLine("\tprivate string yytext () {");
            m_outstream.WriteLine("\t\treturn (new string(yy_buffer,");
            m_outstream.WriteLine("\t\t\tyy_buffer_start,");
            m_outstream.WriteLine("\t\t\tyy_buffer_end - yy_buffer_start));");
            m_outstream.WriteLine("\t}");

            /* Function: yylength */
            m_outstream.WriteLine("\tprivate int yylength () {");
            m_outstream.WriteLine("\t\treturn yy_buffer_end - yy_buffer_start;");
            m_outstream.WriteLine("\t}");

            /* Function: yy_double */
            m_outstream.WriteLine("\tprivate char[] yy_double (char[] buf) {");
            m_outstream.WriteLine("\t\tint i;");
            m_outstream.WriteLine("\t\tchar[] newbuf;");
            m_outstream.WriteLine("\t\tnewbuf = new char[2*buf.Length];");
            m_outstream.WriteLine("\t\tfor (i = 0; i < buf.Length; ++i) {");
            m_outstream.WriteLine("\t\t\tnewbuf[i] = buf[i];");
            m_outstream.WriteLine("\t\t}");
            m_outstream.WriteLine("\t\treturn newbuf;");
            m_outstream.WriteLine("\t}");

            /* Function: yy_error */
            m_outstream.WriteLine("\tprivate const int YY_E_INTERNAL = 0;");
            m_outstream.WriteLine("\tprivate const int YY_E_MATCH = 1;");
            m_outstream.WriteLine("\tprivate string[] yy_error_string = {");
            m_outstream.WriteLine("\t\t\"Error: Internal error.\\n\",");
            m_outstream.WriteLine("\t\t\"Error: Unmatched input.\\n\"");
            m_outstream.WriteLine("\t};");
            m_outstream.WriteLine("\tprivate void yy_error (int code,bool fatal) {");
            m_outstream.WriteLine("\t\t System.Console.Write(yy_error_string[code]);");
            m_outstream.WriteLine("\t\t System.Console.Out.Flush();");
            m_outstream.WriteLine("\t\tif (fatal) {");
            m_outstream.WriteLine("\t\t\tthrow new System.Exception(\"Fatal Error.\\n\");");
            m_outstream.WriteLine("\t\t}");
            m_outstream.WriteLine("\t}");

           

            // Function: private int [][] unpackFromString(int size1, int size2, string st)
            // Added 6/24/98 Raimondas Lencevicius
            // May be made more efficient by replacing string operations
            // Assumes correctly formed input string. Performs no error checking
            m_outstream.WriteLine("\tprivate static int[][] unpackFromString"+
                "(int size1, int size2, string st) {");
            m_outstream.WriteLine("\t\tint colonIndex = -1;");
            m_outstream.WriteLine("\t\tstring lengthString;");
            m_outstream.WriteLine("\t\tint sequenceLength = 0;");
            m_outstream.WriteLine("\t\tint sequenceInteger = 0;");
            m_outstream.WriteLine();
            m_outstream.WriteLine("\t\tint commaIndex;");
            m_outstream.WriteLine("\t\tstring workString;");
            m_outstream.WriteLine();
            m_outstream.WriteLine("\t\tint[][] res = new int[size1][];");
            m_outstream.WriteLine("\t\tfor(int i=0;i<size1;i++) res[i]=new int[size2];");
            m_outstream.WriteLine("\t\tfor (int i= 0; i < size1; i++) {");
            m_outstream.WriteLine("\t\t\tfor (int j= 0; j < size2; j++) {");
            m_outstream.WriteLine("\t\t\t\tif (sequenceLength != 0) {");
            m_outstream.WriteLine("\t\t\t\t\tres[i][j] = sequenceInteger;");
            m_outstream.WriteLine("\t\t\t\t\tsequenceLength--;");
            m_outstream.WriteLine("\t\t\t\t\tcontinue;");
            m_outstream.WriteLine("\t\t\t\t}");
            m_outstream.WriteLine("\t\t\t\tcommaIndex = st.IndexOf(',');");
            m_outstream.WriteLine("\t\t\t\tworkString = (commaIndex==-1) ? st :");
            m_outstream.WriteLine("\t\t\t\t\tst.Substring(0, commaIndex);");
            m_outstream.WriteLine("\t\t\t\tst = st.Substring(commaIndex+1);");  
            m_outstream.WriteLine("\t\t\t\tcolonIndex = workString.IndexOf(':');");
            m_outstream.WriteLine("\t\t\t\tif (colonIndex == -1) {");
            m_outstream.WriteLine("\t\t\t\t\tres[i][j]=System.Int32.Parse(workString);");
            m_outstream.WriteLine("\t\t\t\t\tcontinue;");
            m_outstream.WriteLine("\t\t\t\t}");
            m_outstream.WriteLine("\t\t\t\tlengthString =");
            m_outstream.WriteLine("\t\t\t\t\tworkString.Substring(colonIndex+1);");
            m_outstream.WriteLine("\t\t\t\tsequenceLength="+
                "System.Int32.Parse(lengthString);");
            m_outstream.WriteLine("\t\t\t\tworkString="+
                "workString.Substring(0,colonIndex);");
            m_outstream.WriteLine("\t\t\t\tsequenceInteger="+
                "System.Int32.Parse(workString);");
            m_outstream.WriteLine("\t\t\t\tres[i][j] = sequenceInteger;");
            m_outstream.WriteLine("\t\t\t\tsequenceLength--;");
            m_outstream.WriteLine("\t\t\t}");
            m_outstream.WriteLine("\t\t}");
            m_outstream.WriteLine("\t\treturn res;");
            m_outstream.WriteLine("\t}");
        }

        /***************************************************************
          Function: emit_header
          Description: Emits class header.
          **************************************************************/
        private void emit_header
            (
            )
        {
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != m_spec);
                CUtility.ASSERT(null != m_outstream);
            }

            m_outstream.WriteLine();
            m_outstream.WriteLine();
            if (true == m_spec.m_public) 
            {
                m_outstream.Write("public ");
            }
            m_outstream.Write("class ");
            m_outstream.Write(new string(m_spec.m_class_name,0,
                m_spec.m_class_name.Length));
            if (m_spec.m_implements_name.Length > 0) 
            {
                m_outstream.Write(" : ");	
                m_outstream.Write(new string(m_spec.m_implements_name,0,
                    m_spec.m_implements_name.Length));
            }	  
            m_outstream.WriteLine(" {");
        }

        /***************************************************************
          Function: emit_table
          Description: Emits transition table.
          **************************************************************/
        private void emit_table
            (
            )
        {
            int i;
            int elem;
            int size;
            CDTrans dtrans;
            bool is_start;
            bool is_end;
            CAccept accept;

            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != m_spec);
                CUtility.ASSERT(null != m_outstream);
            }

            m_outstream.WriteLine("\tprivate int[] yy_acpt = {");
            size = m_spec.m_accept_vector.Count;
            for (elem = 0; elem < size; ++elem)
            {
                accept = (CAccept) m_spec.m_accept_vector.elementAt(elem);
	    
                m_outstream.Write("\t\t/* "+elem+" */ ");
                if (null != accept)
                {
                    is_start = (0 != (m_spec.m_anchor_array[elem] & CSpec.START));
                    is_end = (0 != (m_spec.m_anchor_array[elem] & CSpec.END));
		
                    if (is_start && true == is_end)
                    {
                        m_outstream.Write("YY_START | YY_END");
                    }
                    else if (is_start)
                    {
                        m_outstream.Write("YY_START");
                    }
                    else if (is_end)
                    {
                        m_outstream.Write("YY_END");
                    }
                    else
                    {
                        m_outstream.Write("YY_NO_ANCHOR");
                    }
                }
                else 
                {
                    m_outstream.Write("YY_NOT_ACCEPT");
                }
	    
                if (elem < size - 1)
                {
                    m_outstream.Write(",");
                }
	    
                m_outstream.WriteLine();
            }
            m_outstream.WriteLine("\t};");

            // CSA: modified yy_cmap to use string packing 9-Aug-1999
            int[] yy_cmap = new int[m_spec.m_ccls_map.Length];
            for (i = 0; i < m_spec.m_ccls_map.Length; ++i)
                yy_cmap[i] = m_spec.m_col_map[m_spec.m_ccls_map[i]];
            m_outstream.Write("\tprivate int[] yy_cmap = unpackFromString(");
            emit_table_as_string(new int[][] { yy_cmap });
            m_outstream.WriteLine(")[0];");
            m_outstream.WriteLine();

            // CSA: modified yy_rmap to use string packing 9-Aug-1999
            m_outstream.Write("\tprivate int[] yy_rmap = unpackFromString(");
            emit_table_as_string(new int[][] { m_spec.m_row_map });
            m_outstream.WriteLine(")[0];");
            m_outstream.WriteLine();

            // 6/24/98 Raimondas Lencevicius
            // modified to use
            //    int[][] unpackFromString(int size1, int size2, string st)
            size = m_spec.m_dtrans_vector.size();
            int[][] yy_nxt = new int[size][];
            for (elem=0; elem<size; elem++) 
            {
                dtrans = (CDTrans) m_spec.m_dtrans_vector.elementAt(elem);
                CUtility.ASSERT(dtrans.m_dtrans.Length==m_spec.m_dtrans_ncols);
                yy_nxt[elem] = dtrans.m_dtrans;
            }
            m_outstream.Write
                ("\tprivate int[][] yy_nxt = unpackFromString(");
            emit_table_as_string(yy_nxt);
            m_outstream.WriteLine(");");
            m_outstream.WriteLine();
        }

        /***************************************************************
          Function: emit_driver
          Description: Output an integer table as a string.  Written by
          Raimondas Lencevicius 6/24/98; reorganized by CSA 9-Aug-1999.
          From his original comments:
             yy_nxt[][] values are coded into a string
             by printing integers and representing
             integer sequences as "value:length" pairs.
          **************************************************************/
        private void emit_table_as_string(int[][] ia) 
        {
            int sequenceLength = 0; // RL - length of the number sequence
            bool sequenceStarted = false; // RL - has number sequence started?
            int previousInt = -20; // RL - Bogus -20 state.
	
            // RL - Output matrix size
            m_outstream.Write(ia.Length);
            m_outstream.Write(",");
            m_outstream.Write(ia.Length>0?ia[0].Length:0);
            m_outstream.WriteLine(",");

            System.Text.StringBuilder outstr = new System.Text.StringBuilder();

            //  RL - Output matrix 
            for (int elem = 0; elem < ia.Length; ++elem)
            {
                for (int i = 0; i < ia[elem].Length; ++i)
                {
                    int writeInt = ia[elem][i];
                    if (writeInt == previousInt) // RL - sequence?
                    {
                        if (sequenceStarted)
                        {
                            sequenceLength++;
                        }
                        else
                        {
                            outstr.Append(writeInt);
                            outstr.Append(":");
                            sequenceLength = 2;
                            sequenceStarted = true;
                        }
                    }
                    else // RL - no sequence or end sequence
                    {
                        if (sequenceStarted)
                        {
                            outstr.Append(sequenceLength);
                            outstr.Append(",");
                            sequenceLength = 0;
                            sequenceStarted = false;
                        }
                        else
                        {
                            if (previousInt != -20)
                            {
                                outstr.Append(previousInt);
                                outstr.Append(",");
                            }
                        }
                    }
                    previousInt = writeInt;
                    // CSA: output in 75 character chunks.
                    if (outstr.Length > 75) 
                    {
                        string s = outstr.ToString();
                        m_outstream.WriteLine("\""+s.Substring(0,75)+"\" +");
                        outstr = new StringBuilder(s.Substring(75));
                    }
                }
            }
            if (sequenceStarted)
            {
                outstr.Append(sequenceLength);
            }
            else
            {
                outstr.Append(previousInt);
            }    
            // CSA: output in 75 character chunks.
            if (outstr.Length > 75) 
            {
                string s = outstr.ToString();
                m_outstream.WriteLine("\""+s.Substring(0,75)+"\" +");
                outstr = new StringBuilder(s.Substring(75));
            }
            m_outstream.Write("\""+outstr+"\"");
        }

        /***************************************************************
          Function: emit_driver
          Description: 
          **************************************************************/
        private void emit_driver
            (
            )
        {
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != m_spec);
                CUtility.ASSERT(null != m_outstream);
            }
	  
            emit_table();

            if (m_spec.m_integer_type)
            {
                m_outstream.Write("\tpublic int ");
                m_outstream.Write(new string(m_spec.m_function_name));
                m_outstream.WriteLine(" ()");
            }
            else if (m_spec.m_intwrap_type)
            {
                m_outstream.Write("\tpublic int ");
                m_outstream.Write(new string(m_spec.m_function_name));
                m_outstream.WriteLine(" ()");
            }
            else
            {
                m_outstream.Write("\tpublic ");
                m_outstream.Write(new string(m_spec.m_type_name));
                m_outstream.Write(" ");
                m_outstream.Write(new string(m_spec.m_function_name));
                m_outstream.WriteLine(" ()");
            }

            /*m_outstream.WriteLine("\t\tthrows java.io.IOException {");*/
       //     m_outstream.Write("\t\tthrows java.io.IOException");
            if (null != m_spec.m_yylex_throw_code)
            {
                m_outstream.Write(", "); 
                m_outstream.Write(new string(m_spec.m_yylex_throw_code,0,
                    m_spec.m_yylex_throw_read));
                m_outstream.WriteLine();
                m_outstream.WriteLine("\t\t{");
            }
            else
            {
                m_outstream.WriteLine(" {");
            }

            m_outstream.WriteLine("\t\tint yy_lookahead;");
            m_outstream.WriteLine("\t\tint yy_anchor = YY_NO_ANCHOR;");
            /*m_outstream.WriteLine("\t\tint yy_state "
              + "= yy_initial_dtrans(yy_lexical_state);");*/
            m_outstream.WriteLine("\t\tint yy_state " 
                + "= yy_state_dtrans[yy_lexical_state];");
            m_outstream.WriteLine("\t\tint yy_next_state = YY_NO_STATE;");
            /*m_outstream.WriteLine("\t\tint yy_prev_stave = YY_NO_STATE;");*/
            m_outstream.WriteLine("\t\tint yy_last_accept_state = YY_NO_STATE;");
            m_outstream.WriteLine("\t\tbool yy_initial = true;");
            m_outstream.WriteLine("\t\tint yy_this_accept;");
            m_outstream.WriteLine();

            m_outstream.WriteLine("\t\tyy_mark_start();");
            /*m_outstream.WriteLine("\t\tyy_this_accept = yy_accept(yy_state);");*/
            m_outstream.WriteLine("\t\tyy_this_accept = yy_acpt[yy_state];");
            m_outstream.WriteLine("\t\tif (YY_NOT_ACCEPT != yy_this_accept) {");
            m_outstream.WriteLine("\t\t\tyy_last_accept_state = yy_state;");
            m_outstream.WriteLine("\t\t\tyy_mark_end();");
            m_outstream.WriteLine("\t\t}");

            if (NOT_EDBG)
            {
                m_outstream.WriteLine("\t\tSystem.Console.WriteLine(\"Begin\");");
            }

            m_outstream.WriteLine("\t\twhile (true) {");

            m_outstream.WriteLine("\t\t\tif (yy_initial && yy_at_bol) "+
                "yy_lookahead = YY_BOL;");
            m_outstream.WriteLine("\t\t\telse yy_lookahead = yy_advance();");
            m_outstream.WriteLine("\t\t\tyy_next_state = YY_F;");
            /*m_outstream.WriteLine("\t\t\t\tyy_next_state = "
                       + "yy_next(yy_state,yy_lookahead);");*/
            m_outstream.WriteLine("\t\t\tyy_next_state = "
                + "yy_nxt[yy_rmap[yy_state]][yy_cmap[yy_lookahead]];");

            if (NOT_EDBG)
            {
                m_outstream.WriteLine("System.Console.WriteLine(\"Current state: \"" 
                    + " + yy_state");
                m_outstream.WriteLine("+ \"\tCurrent input: \""); 
                m_outstream.WriteLine(" + ((char) yy_lookahead));");
            }
            if (NOT_EDBG)
            {
                m_outstream.WriteLine("\t\t\tSystem.Console.WriteLine(\"State = \"" 
                    + "+ yy_state);");
                m_outstream.WriteLine("\t\t\tSystem.Console.WriteLine(\"Accepting status = \"" 
                    + "+ yy_this_accept);");
                m_outstream.WriteLine("\t\t\tSystem.Console.WriteLine(\"Last accepting state = \"" 
                    + "+ yy_last_accept_state);");
                m_outstream.WriteLine("\t\t\tSystem.Console.WriteLine(\"Next state = \"" 
                    + "+ yy_next_state);");
                m_outstream.WriteLine("\t\t\tSystem.Console.WriteLine(\"Lookahead input = \"" 
                    + "+ ((char) yy_lookahead));");
            }

            // handle bare EOF.
            m_outstream.WriteLine("\t\t\tif (YY_EOF == yy_lookahead " 
                + "&& true == yy_initial) {");
            if (null != m_spec.m_eof_code)
            {
                m_outstream.WriteLine("\t\t\t\tyy_do_eof();");
            }
            if (true == m_spec.m_integer_type)
            {
                m_outstream.WriteLine("\t\t\t\treturn YYEOF;");
            }
            else if (null != m_spec.m_eof_value_code) 
            {
                m_outstream.Write(new string(m_spec.m_eof_value_code,0,
                    m_spec.m_eof_value_read));
            }
            else
            {
                m_outstream.WriteLine("\t\t\t\treturn null;");
            }
            m_outstream.WriteLine("\t\t\t}");

            m_outstream.WriteLine("\t\t\tif (YY_F != yy_next_state) {");
            m_outstream.WriteLine("\t\t\t\tyy_state = yy_next_state;");
            m_outstream.WriteLine("\t\t\t\tyy_initial = false;");
            /*m_outstream.WriteLine("\t\t\t\tyy_this_accept = yy_accept(yy_state);");*/
            m_outstream.WriteLine("\t\t\t\tyy_this_accept = yy_acpt[yy_state];");
            m_outstream.WriteLine("\t\t\t\tif (YY_NOT_ACCEPT != yy_this_accept) {");
            m_outstream.WriteLine("\t\t\t\t\tyy_last_accept_state = yy_state;");
            m_outstream.WriteLine("\t\t\t\t\tyy_mark_end();");
            m_outstream.WriteLine("\t\t\t\t}");
            /*m_outstream.WriteLine("\t\t\t\tyy_prev_state = yy_state;");*/
            /*m_outstream.WriteLine("\t\t\t\tyy_state = yy_next_state;");*/
            m_outstream.WriteLine("\t\t\t}");

            m_outstream.WriteLine("\t\t\telse {");
	  
            m_outstream.WriteLine("\t\t\t\tif (YY_NO_STATE == yy_last_accept_state) {");

            m_outstream.WriteLine("\t\t\t\t\tthrow (new System.Exception(\"Lexical Error: Unmatched Input.\"));");
            m_outstream.WriteLine("\t\t\t\t}");

            m_outstream.WriteLine("\t\t\t\telse {");

            m_outstream.WriteLine("\t\t\t\t\tyy_anchor = yy_acpt[yy_last_accept_state];");
            m_outstream.WriteLine("\t\t\t\t\tif (0 != (YY_END & yy_anchor)) {");
            m_outstream.WriteLine("\t\t\t\t\t\tyy_move_end();");
            m_outstream.WriteLine("\t\t\t\t\t}");
            m_outstream.WriteLine("\t\t\t\t\tyy_to_mark();");

            m_outstream.WriteLine("\t\t\t\t\tswitch (yy_last_accept_state) {");

            emit_actions("\t\t\t\t\t");

            m_outstream.WriteLine("\t\t\t\t\tdefault:");
            //SI:break added
            m_outstream.WriteLine("\t\t\t\t\t\tyy_error(YY_E_INTERNAL,false);break;");
            /*m_outstream.WriteLine("\t\t\t\t\t\treturn null;");*/
            //SI:removed
           // m_outstream.WriteLine("\t\t\t\t\tcase -1:");
            m_outstream.WriteLine("\t\t\t\t\t}");
            
	  
            m_outstream.WriteLine("\t\t\t\t\tyy_initial = true;");
            m_outstream.WriteLine("\t\t\t\t\tyy_state "
                + "= yy_state_dtrans[yy_lexical_state];");
            m_outstream.WriteLine("\t\t\t\t\tyy_next_state = YY_NO_STATE;");
            /*m_outstream.WriteLine("\t\t\t\t\tyy_prev_state = YY_NO_STATE;");*/
            m_outstream.WriteLine("\t\t\t\t\tyy_last_accept_state = YY_NO_STATE;");

            m_outstream.WriteLine("\t\t\t\t\tyy_mark_start();");

            /*m_outstream.WriteLine("\t\t\t\t\tyy_this_accept = yy_accept(yy_state);");*/
            m_outstream.WriteLine("\t\t\t\t\tyy_this_accept = yy_acpt[yy_state];");
            m_outstream.WriteLine("\t\t\t\t\tif (YY_NOT_ACCEPT != yy_this_accept) {");
            m_outstream.WriteLine("\t\t\t\t\t\tyy_last_accept_state = yy_state;");
            m_outstream.WriteLine("\t\t\t\t\t\tyy_mark_end();");
            m_outstream.WriteLine("\t\t\t\t\t}");

            m_outstream.WriteLine("\t\t\t\t}");	  
            m_outstream.WriteLine("\t\t\t}");
            m_outstream.WriteLine("\t\t}");
            m_outstream.WriteLine("\t}");
        }
  
        /***************************************************************
          Function: emit_actions
          Description:     
          **************************************************************/
        private void emit_actions 
            (
            string tabs
            )
        {
            int elem;
            int size;
            int bogus_index;
            CAccept accept;
	  
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(m_spec.m_accept_vector.size() 
                    == m_spec.m_anchor_array.Length);
            }

            bogus_index = -2;
            size = m_spec.m_accept_vector.size();
            for (elem = 0; elem < size; ++elem)
            {
                accept = (CAccept) m_spec.m_accept_vector.elementAt(elem);
                if (null != accept) 
                {
                    m_outstream.WriteLine(tabs + "case " + elem 
                        + ":");
                    m_outstream.Write(tabs + "\t");
                    string tmp=new string(accept.m_action,0, accept.m_action_read);
                    //Added by SI:
                    if (tmp.Equals("{ }")) tmp="break;";
                    //Added by SI:
                    if (tmp.Equals("")) tmp="break;";
                    m_outstream.Write(tmp);
                    m_outstream.WriteLine();
                    m_outstream.WriteLine(tabs + "case " + bogus_index + ":");
                    m_outstream.WriteLine(tabs + "\tbreak;");
                    --bogus_index;
                }
            }
        }
  
        /***************************************************************
          Function: emit_footer
          Description:     
          **************************************************************/
        private void emit_footer
            (
            )
        {
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != m_spec);
                CUtility.ASSERT(null != m_outstream);
            }

            m_outstream.WriteLine("}");
        }
    }

}
