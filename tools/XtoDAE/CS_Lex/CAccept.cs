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
	/// Summary description for CAccept.
	/// </summary>
    /***************************************************************
      Class: CAccept
     **************************************************************/
    public class CAccept
    {
        /***************************************************************
          Member Variables
          **************************************************************/
        public char[] m_action;
        public int m_action_read;
        public int m_line_number;

        /***************************************************************
          Function: CAccept
          **************************************************************/
        public CAccept
            (
            char[] action,
            int action_read,
            int line_number
            )
        {
            int elem;

            m_action_read = action_read;

            m_action = new char[m_action_read];
            for (elem = 0; elem < m_action_read; ++elem)
            {
                m_action[elem] = action[elem];
            }

            m_line_number = line_number;
        }

        /***************************************************************
          Function: CAccept
          **************************************************************/
       public  CAccept
            (
            CAccept accept
            )
        {
            int elem;

            m_action_read = accept.m_action_read;
	
            m_action = new char[m_action_read];
            for (elem = 0; elem < m_action_read; ++elem)
            {
                m_action[elem] = accept.m_action[elem];
            }

            m_line_number = accept.m_line_number;
        }

        /***************************************************************
          Function: mimic
          **************************************************************/
        public void mimic
            (
            CAccept accept
            )
        {
            int elem;

            m_action_read = accept.m_action_read;
	
            m_action = new char[m_action_read];
            for (elem = 0; elem < m_action_read; ++elem)
            {
                m_action[elem] = accept.m_action[elem];
            }
        }
    }

}
