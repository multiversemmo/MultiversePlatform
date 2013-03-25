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

/**************************************************************
  JLex: A Lexical Analyzer Generator for Java(TM)
  Written by Elliot Berk <ejberk@cs.princeton.edu>. Copyright 1996.
  Maintained by C. Scott Ananian <cananian@alumni.princeton.edu>.
  See below for copyright notice, license, and disclaimer.
  New releases from http://www.cs.princeton.edu/~appel/modern/java/JLex/

  Version 1.2.6, 2/7/03, [C. Scott Ananian]
   Renamed 'assert' function 'ASSERT' to accomodate Java 1.4's new keyword.
   Fixed a bug which certain forms of comment in the JLex directives section
     (which are not allowed) to be incorrectly parsed as macro definitions.
  Version 1.2.5, 7/25/99-5/16/00, [C. Scott Ananian]
   Stomped on one more 8-bit character bug.  Should work now (really!).
   Added unicode support, including unicode escape sequences.
   Rewrote internal JavaLexBitSet class as SparseBitSet for efficient
     unicoding.
   Added an NFA character class simplification pass for unicode efficiency.
   Changed byte- and stream-oriented I/O routines to use characters and
     java.io.Reader and java.io.Writer instead --- which means we read in
     unicode specifications correctly and write out a proper unicode java
     source file.  As a happy side-effect, the output java file is written
     with your platform's preferred newline character(s).
   Rewrote CInput to fix bugs with line-counting in the specification file
     and "unusual behaviour" when the last line of the specification wasn't
     terminated with a newline. Thanks to Matt Hanna <mhanna@cs.caltech.edu>
     for pointing out the bug.
   Fixed a bug that would cause JLex not to terminate given certain input
     specifications.  Thanks to Mark Greenstreet <mrg@cs.ubc.ca> and
     Frank B. Brokken <frank@suffix.icce.rug.nl> for reporting this.
   CUP parser integration improved according to suggestions made by
     David MacMahon <davidm@smartsc.com>.  The %cup directive now tells
     JLex to generate a parser conforming to the java_cup.runtime.Scanner
     interface; see manual for more details.
   Fixed bug with null string literals ("") in regexps.  Reported by
     Charles Fischer <fischer@cs.wisc.edu>.
   Rewrote start-of-line and end-of-line handling, closing active bug #5.
     Also fixed line-counting code, closing active bug #12.  All
     new-line handling is now platform-independent.
   Used unpackFromString more extensively to allow larger cmap, etc,
     tables.  This helps unicode support work reliably.  It's also
     prettier now if you happen to read the source to the generated
     lexer.
   Generated lexer now accepts unicode LS (U+2028) and PS (U+2029) as
     line separators for strict unicode compliance; see
     http://www.unicode.org/unicode/reports/tr18/
   Fixed bug with character constants in action strings.  Reported by
     Andrew Appel against 1.2.5b3.
   Fixed bug with illegal \^C-style escape sequences.  Reported by
     Toshiya Iwai <iwai@isdnet.co.jp> against 1.2.5b4.
   Fixed "newline in quoted string" error when unpaired single- or
     double-quotes were present in comments in the action phrase.
     Reported by Stephen Ostermiller <1010JLex@ostermiller.com>
     against 1.2.5b4.  Reported by Eric Esposito <eric.esposito@unh.edu>
     against 1.2.4 and 1.2.5b2.
   Fixed "newline in quoted string" error when /* or // appeared
     in quoted strings in the action phrase.  Reported by
     David Eichmann <david-eichmann@uiowa.edu> against 1.2.5b5.
   Fixed 'illegal constant' errors in case statements caused by
     Sun's JDK 1.3 more closely adhering to the Java Language
     Specification.  Reported by a number of people, but 
     Harold Grovesteen <hgrovesteen@home.com> was the first to direct me to
     a Sun bug report (4119776) which quoted the relevant section of the
     JLS (15.27) to convince me that the JLex construction actually was
     illegal.  Reported against 1.2.5b6, but this bit of code has been
     present since the very first version of JLex (1.1.1).

  Version 1.2.4, 7/24/99, [C. Scott Ananian]
   Correct the parsing of '-' in character classes, closing active 
     bug #1.  Behaviour follows egrep: leading and trailing dashes in
     a character class lose their special meaning, so [-+] and [+-] do
     what you would expect them to.
   New %ignorecase directive for generating case-insensitive lexers by
     expanding matched character classes in a unicode-friendly way.
   Handle unmatched braces in quoted strings or comments within
     action code blocks.
   Fixed input lexer to allow whitespace in character classes, closing
     active bug #9.  Whitespace in quotes had been previously fixed.
   Made Yylex.YYEOF and %yyeof work like the manual says they should.

  Version 1.2.3, 6/26/97, [Raimondas Lencevicius]
   Fixed the yy_nxt[][] assignment that has generated huge code
   exceeding 64K method size limit. Now the assignment
   is handled by unpacking a string encoding of integer array.
   To achieve that, added
   "private int [][] unpackFromString(int size1, int size2, string st)"
   function and coded the yy_nxt[][] values into a string
   by printing integers into a string and representing
   integer sequences as "value:length" pairs.
   Improvement: generated .java file reduced 2 times, .class file
     reduced 6 times for sample grammar. No 64K errors.
   Possible negatives: Some editors and OSs may not be able to handle 
     the huge one-line generated string. string unpacking may be slower
     than direct array initialization.

  Version 1.2.2, 10/24/97, [Martin Dirichs]
  Notes:
    Changed yy_instream to yy_reader of type BufferedReader. This reflects
     the improvements in the JDK 1.1 concerning InputStreams. As a
     consequence, changed yy_buffer from byte[] to char[].
     The lexer can now be initialized with either an InputStream
     or a Reader. A third, private constructor is called by the other
     two to execute user specified constructor code.

  Version 1.2.1, 9/15/97 [A. Appel]
   Fixed bugs 6 (character codes > 127) and 10 (deprecated string constructor).

  Version 1.2, 5/5/97, [Elliot Berk]
  Notes:
    Simply changed the name from JavaLex to JLex.  No other changes.

  Version 1.1.5, 2/25/97, [Elliot Berk]
  Notes:
    Simple optimization to the creation of the source files.
     Added a BufferedOutputStream in the creation of the DataOutputStream
     field m_outstream of the class CLexGen.  This helps performance by
     doing some buffering, and was suggested by Max Hailperin,
     Associate Professor of Computer Science, Gustavus Adolphus College.

  Version 1.1.4, 12/12/96, [Elliot Berk]
  Notes:
    Added %public directive to make generated class public.

  Version 1.1.3, 12/11/96, [Elliot Berk]
  Notes:
    Converted assertion failure on invalid character class 
     when a dash '-' is not preceded with a start-of-range character.
     Converted this into parse error E_DASH.

  Version 1.1.2, October 30, 1996 [Elliot Berk]
    Fixed BitSet bugs by installing a BitSet class of my own,
     called JavaLexBitSet.  Fixed support for '\r', non-UNIX 
     sequences.  Added try/catch block around lexer generation
     in main routine to moderate error information presented 
     to user.  Fixed macro expansion, so that macros following 
     quotes are expanded correctly in regular expressions.
     Fixed dynamic reallocation of accept action buffers.

  Version 1.1.1, September 3, 1996 [Andrew Appel]
    Made the class "Main" instead of "JavaLex", 
     improved the installation instructions to reflect this.

  Version 1.1, August 15, 1996  [Andrew Appel]
    Made yychar, yyline, yytext global to the lexer so that
     auxiliary functions can access them.
  **************************************************************/

/***************************************************************
       JLEX COPYRIGHT NOTICE, LICENSE, AND DISCLAIMER
  Copyright 1996-2000 by Elliot Joel Berk and C. Scott Ananian 

  Permission to use, copy, modify, and distribute this software and its
  documentation for any purpose and without fee is hereby granted,
  provided that the above copyright notice appear in all copies and that
  both the copyright notice and this permission notice and warranty
  disclaimer appear in supporting documentation, and that the name of
  the authors or their employers not be used in advertising or publicity
  pertaining to distribution of the software without specific, written
  prior permission.

  The authors and their employers disclaim all warranties with regard to
  this software, including all implied warranties of merchantability and
  fitness. In no event shall the authors or their employers be liable
  for any special, indirect or consequential damages or any damages
  whatsoever resulting from loss of use, data or profits, whether in an
  action of contract, negligence or other tortious action, arising out
  of or in connection with the use or performance of this software.
  **************************************************************/

/***************************************************************
  Package Declaration
  **************************************************************/
namespace TUVienna.CS_Lex
{

    /***************************************************************
      Imported Packages
      **************************************************************/
    using System.Collections;


    /******************************
      Questions:
      2) How should I use the Java package system
      to make my tool more modularized and
      coherent?

      Unimplemented:
      !) Fix BitSet issues -- expand only when necessary.
      2) Repeated accept rules.
      6) Clean up the CAlloc class and use buffered
      allocation.
      9) Add to spec about extending character set.
      11) m_verbose -- what should be done with it?
      12) turn lexical analyzer into a coherent
      Java package
      13) turn lexical analyzer generator into a
      coherent Java package
      16) pretty up generated code
      17) make it possible to have white space in
      regular expressions
      18) clean up all of the class files the lexer
      generator produces when it is compiled,
      and reduce this number in some way.
      24) character format to and from file: writeup
      and implementation
      25) Debug by testing all arcane regular expression cases.
      26) Look for and fix all UNDONE comments below.
      27) Fix package system.
      28) Clean up unnecessary classes.
      *****************************/

    /***************************************************************
      Class: Main
      Description: Top-level lexical analyzer generator function.
     **************************************************************/
    public class Main
    {
        /***************************************************************
          Function: main
          **************************************************************/
        public static void main
            (
            string[] arg
            )
        {
            CLexGen lg;
       
            if (arg.Length < 1)
            {
                System.Console.WriteLine("Usage: JLex.Main <filename>");
                return;
            }

            /* Note: For debuging, it may be helpful to remove the try/catch
               block and permit the Exception to propagate to the top level. 
               This gives more information. */
            try 
            {	
                lg = new CLexGen(arg[0]);
                lg.generate();
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
    }    
}







    

/************************************************************************
  JLEX COPYRIGHT NOTICE, LICENSE AND DISCLAIMER.
  
  Copyright 1996 by Elliot Joel Berk
  
  Permission to use, copy, modify, and distribute this software and its
  documentation for any purpose and without fee is hereby granted,
  provided that the above copyright notice appear in all copies and that
  both the copyright notice and this permission notice and warranty
  disclaimer appear in supporting documentation, and that the name of
  Elliot Joel Berk not be used in advertising or publicity pertaining 
  to distribution of the software without specific, written prior permission.
  
  Elliot Joel Berk disclaims all warranties with regard to this software, 
  including all implied warranties of merchantability and fitness.  In no event
  shall Elliot Joel Berk be liable for any special, indirect or consequential
  damages or any damages whatsoever resulting from loss of use, data or
  profits, whether in an action of contract, negligence or other
  tortious action, arising out of or in connection with the use or
  performance of this software.
  ***********************************************************************/
// set emacs indentation
// Local Variables:
// c-basic-offset:2
// End:
