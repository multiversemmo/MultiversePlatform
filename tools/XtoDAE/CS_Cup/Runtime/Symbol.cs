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

namespace TUVienna.CS_CUP.Runtime
{

	/**
	 * Defines the Symbol class, which is used to represent all terminals
	 * and nonterminals while parsing.  The lexer should pass CUP Symbols 
	 * and CUP returns a Symbol.
	 *
	 * @version last updated: 7/3/96
	 * @author  Frank Flannery
     * translated to C# 08.09.2003 by Samuel Imriska
	 */

	/* ****************************************************************
	  Class Symbol
	  what the parser expects to receive from the lexer. 
	  the token is identified as follows:
	  sym:    the symbol type
	  parse_state: the parse state.
	  value:  is the lexical value of type Object
	  left :  is the left position in the original input file
	  right:  is the right position in the original input file
	******************************************************************/

	public class Symbol 
	{

		/*******************************
		  Constructor for l,r values
		 *******************************/

		public Symbol(int id, int l, int r, object o) :this(id)
		{
			left = l;
			right = r;
			value = o;
		}

		/*******************************
		  Constructor for no l,r values
		********************************/

		public Symbol(int id, object o) :this(id,-1,-1,o){	}

		/*****************************
		  Constructor for no value
		  ***************************/

		public Symbol(int id, int l, int r) : this(id,l,r,null){	}

		/***********************************
		  Constructor for no value or l,r
		***********************************/

		public Symbol(int sym_num) : this(sym_num,-1)
		{
			left = -1;
			right = -1;
			value = null;
		}

		/***********************************
		  Constructor to give a start state
		***********************************/
		public Symbol(int sym_num, int state)
		{
			sym = sym_num;
			parse_state = state;
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** The symbol number of the terminal or non terminal being represented */
		public int sym;

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** The parse state to be recorded on the parse stack with this symbol.
		 *  This field is for the convenience of the parser and shouldn't be 
		 *  modified except by the parser. 
		 */
		public int parse_state;
		/** This allows us to catch some errors caused by scanners recycling
		 *  symbols.  For the use of the parser only. [CSA, 23-Jul-1999] */
		public bool used_by_parser = false;

		/*******************************
		  The data passed to parser
		 *******************************/

		public int left, right;
		public object value;

		/*****************************
		  Printing this token out. (Override for pretty-print).
		  ****************************/
		public override string ToString() { return "#"+sym; }
	}
}





