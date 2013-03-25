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

namespace TUVienna.CS_Lex
{
	/// <summary>
	/// Summary description for CInput.
	/// </summary>
    /***************************************************************
  Class: CInput
  Description: 
 **************************************************************/
   public  class CInput
    {
        /***************************************************************
          Member Variables
          **************************************************************/
        private TextReader m_input; /* JLex specification file. */

        public bool m_eof_reached; /* Whether EOF has been encountered. */
        public bool m_pushback_line; 

        public char[] m_line; /* Line buffer. */
        public int m_line_read; /* Number of bytes read into line buffer. */
        public int m_line_index; /* Current index into line buffer. */

        public int m_line_number; /* Current line number. */

        /***************************************************************
          Constants
          **************************************************************/
       public  const bool EOF = true;
        public const bool NOT_EOF = false;
  
        /***************************************************************
          Function: CInput
          Description: 
          **************************************************************/
       public  CInput
            (
            TextReader input
            )
        {
            if (CUtility.DEBUG)
            {
                CUtility.ASSERT(null != input);
            }

            /* Initialize input stream. */
            m_input = input;

            /* Initialize buffers and index counters. */
            m_line = null;
            m_line_read = 0;
            m_line_index = 0;

            /* Initialize state variables. */
            m_eof_reached = false;
            m_line_number = 0;
            m_pushback_line = false;
        }

        /***************************************************************
          Function: getLine
          Description: Returns true on EOF, false otherwise.
          Guarantees not to return a blank line, or a line
          of zero length.
          **************************************************************/
       public  bool getLine 
            (
            )
        {
            string lineStr;
            int elem;
	
            /* Has EOF already been reached? */
            if (m_eof_reached)
            {
                return EOF;
            }
	
            /* Pushback current line? */
            if (m_pushback_line)
            {
                m_pushback_line = false;

                /* Check for empty line. */
                for (elem = 0; elem < m_line_read; ++elem)
                {
                    if (false == CUtility.isspace(m_line[elem]))
                    {
                        break;
                    }
                }

                /* Nonempty? */
                if (elem < m_line_read)
                {
                    m_line_index = 0;
                    return NOT_EOF;
                }
            }

            while (true)
            {
                if (null == (lineStr = m_input.ReadLine()))
                {
                    m_eof_reached = true;
                    m_line_index = 0;
                    return EOF;
                }
                m_line = (lineStr + "\n").ToCharArray();
                m_line_read=m_line.Length;
                ++m_line_number;
	    
                /* Check for empty lines and discard them. */
                elem = 0;
                while (CUtility.isspace(m_line[elem])) 
                {
                    ++elem;
                    if (elem == m_line_read)
                    {
                        break;
                    }
                }
	    
                if (elem < m_line_read)
                {
                    break;
                }
            }

            m_line_index = 0;
            return NOT_EOF;
        }
    }

}
