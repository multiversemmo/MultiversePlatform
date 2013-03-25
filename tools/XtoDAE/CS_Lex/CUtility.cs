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
	/// Summary description for CUtility.
	/// </summary>
    /********************************************************
  Class: Utility
  *******************************************************/
    public class CUtility 
    {
        /********************************************************
          Constants
          *******************************************************/
       public  const bool DEBUG = true;
        public const bool SLOW_DEBUG = true;
        public const bool DUMP_DEBUG = true;
        /*const bool DEBUG = false;
        const bool SLOW_DEBUG = false;
        const bool DUMP_DEBUG = false;*/
        public const bool DESCENT_DEBUG = false;
        public const bool OLD_DEBUG = false;
        public const bool OLD_DUMP_DEBUG = false;
        public const bool FOODEBUG = false;
        public const bool DO_DEBUG = false;
  
        /********************************************************
          Constants: Integer Bounds
          *******************************************************/
        public const int INT_MAX = 2147483647;

        public const int MAX_SEVEN_BIT = 127;
        public const int MAX_EIGHT_BIT = 255;
        public const int MAX_SIXTEEN_BIT=65535;

        /********************************************************
          Function: enter
          Description: Debugging routine.
          *******************************************************/
        public static void enter
            (
            string descent,
            char lexeme,
            int token
            )
        {
            System.Console.WriteLine("Entering " + descent 
                + " [lexeme: " + lexeme 
                + "] [token: " + token + "]");
        }

        /********************************************************
          Function: leave
          Description: Debugging routine.
          *******************************************************/
        public static void leave
            (
            string descent,
            char lexeme,
            int token
            )
        {
            System.Console.WriteLine("Leaving " + descent 
                + " [lexeme:" + lexeme 
                + "] [token:" + token + "]");
        }

        /********************************************************
          Function: ASSERT
          Description: Debugging routine.
          *******************************************************/
       public  static void ASSERT
            (
            bool expr
            )
        {
            if (DEBUG && false == expr)
            {
                System.Console.WriteLine("Assertion Failed");
                throw new System.Exception("Assertion Failed.");
            }
        }

        /***************************************************************
          Function: doubleSize
          **************************************************************/
       public  static char[] doubleSize
            (
            char[] oldBuffer
            )
        {
            char[] newBuffer = new char[2 * oldBuffer.Length];
            int elem;

            for (elem = 0; elem < oldBuffer.Length; ++elem)
            {
                newBuffer[elem] = oldBuffer[elem];
            }

            return newBuffer;
        }

        /***************************************************************
          Function: doubleSize
          **************************************************************/
      public   static byte[] doubleSize
            (
            byte[] oldBuffer
            )
        {
            byte[] newBuffer = new byte[2 * oldBuffer.Length];
            int elem;

            for (elem = 0; elem < oldBuffer.Length; ++elem)
            {
                newBuffer[elem] = oldBuffer[elem];
            }

            return newBuffer;
        }

        /********************************************************
          Function: hex2bin
          *******************************************************/
      public   static char hex2bin
            (
            char c
            )
        {
            if ('0' <= c && '9' >= c)
            {
                return (char) (c - '0');
            }
            else if ('a' <= c && 'f' >= c)
            {
                return (char) (c - 'a' + 10);
            }	    
            else if ('A' <= c && 'F' >= c)
            {
                return (char) (c - 'A' + 10);
            }
	
            CError.impos("Bad hexidecimal digit" + c);
            return (char)0;
        }

        /********************************************************
          Function: ishexdigit
          *******************************************************/
      public   static bool ishexdigit
            (
            char c
            )
        {
            if (('0' <= c && '9' >= c)
                || ('a' <= c && 'f' >= c)
                || ('A' <= c && 'F' >= c))
            {
                return true;
            }

            return false;
        }

        /********************************************************
          Function: oct2bin
          *******************************************************/
      public   static char oct2bin
            (
            char c
            )
        {
            if ('0' <= c && '7' >= c)
            {
                return (char) (c - '0');
            }
	
            CError.impos("Bad octal digit " + c);
            return (char)0;
        }

        /********************************************************
          Function: isoctdigit
          *******************************************************/
      public   static bool isoctdigit
            (
            char c
            )
        {
            if ('0' <= c && '7' >= c)
            {
                return true;
            }

            return false;
        }
	
        /********************************************************
          Function: isspace
          *******************************************************/
        public static bool isspace
            (
            char c
            )
        {
            if ('\b' == c 
                || '\t' == c
                || '\n' == c
                || '\f' == c
                || '\r' == c
                || ' ' == c)
            {
                return true;
            }
	
            return false;
        }

        /********************************************************
          Function: isnewline
          *******************************************************/
       public  static bool isnewline
            (
            char c
            )
        {
            if ('\n' == c
                || '\r' == c)
            {
                return true;
            }
	
            return false;
        }

        /********************************************************
          Function: bytencmp
          Description: Compares up to n elements of 
          byte array a[] against byte array b[].
          The first byte comparison is made between 
          a[a_first] and b[b_first].  Comparisons continue
          until the null terminating byte '\0' is reached
          or until n bytes are compared.
          Return Value: Returns 0 if arrays are the 
          same up to and including the null terminating byte 
          or up to and including the first n bytes,
          whichever comes first.
          *******************************************************/
      public   static int bytencmp
            (
            byte[] a,
            int a_first,
            byte[] b,
            int b_first,
            int n
            )
        {
            int elem;

            for (elem = 0; elem < n; ++elem)
            {
                /*System.Console.Write((char) a[a_first + elem]);
                System.Console.Write((char) b[b_first + elem]);*/
			     
                if ('\0' == a[a_first + elem] && '\0' == b[b_first + elem])
                {
                    /*System.Console.WriteLine("return 0");*/
                    return 0;
                }
                if (a[a_first + elem] < b[b_first + elem])
                {
                    /*System.Console.WriteLine("return 1");*/
                    return 1;
                }
                else if (a[a_first + elem] > b[b_first + elem])
                {
                    /*System.Console.WriteLine("return -1");*/
                    return -1;
                }
            }

            /*System.Console.WriteLine("return 0");*/
            return 0;
        }

        /********************************************************
          Function: charncmp
          *******************************************************/
      public   static int charncmp
            (
            char[] a,
            int a_first,
            char[] b,
            int b_first,
            int n
            )
        {
            int elem;

            for (elem = 0; elem < n; ++elem)
            {
                if ('\0' == a[a_first + elem] && '\0' == b[b_first + elem])
                {
                    return 0;
                }
                if (a[a_first + elem] < b[b_first + elem])
                {
                    return 1;
                }
                else if (a[a_first + elem] > b[b_first + elem])
                {
                    return -1;
                }
            }

            return 0;
        }
    }

}
