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

namespace TUVienna.CS_CUP
{

	/** This abstract class serves as the base class for grammar symbols (i.e.,
	 * both terminals and non-terminals).  Each symbol has a name string, and
	 * a string giving the type of object that the symbol will be represented by
	 * on the runtime parse stack.  In addition, each symbol maintains a use count
	 * in order to detect symbols that are declared but never used, and an index
	 * number that indicates where it appears in parse tables (index numbers are
	 * unique within terminals or non terminals, but not across both).
	 *
	 * @see     java_cup.terminal
	 * @see     java_cup.non_terminal
	 * @version last updated: 7/3/96
	 * @author  Frank Flannery
     * translated to C# 08.09.2003 by Samuel Imriska
	 */
	public abstract class symbol 
	{
		/*-----------------------------------------------------------*/
		/*--- Constructor(s) ----------------------------------------*/
		/*-----------------------------------------------------------*/

		/** Full constructor.
		 * @param nm the name of the symbol.
		 * @param tp a string with the type name.
		 */
		public symbol(string nm, string tp)
		{
			/* sanity check */
			if (nm == null) nm = "";

			/* apply default if no type given */
			if (tp == null) tp = "object";

			_name = nm;
			_stack_type = tp;
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Constructor with default type. 
		 * @param nm the name of the symbol.
		 */
		public symbol(string nm) :this(nm,null)
		{
	//		this(nm, null);
		}

		/*-----------------------------------------------------------*/
		/*--- (Access to) Instance Variables ------------------------*/
		/*-----------------------------------------------------------*/

		/** string for the human readable name of the symbol. */
		protected string _name; 
 
		/** string for the human readable name of the symbol. */
		public string name() {return _name;}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** string for the type of object used for the symbol on the parse stack. */
		protected string _stack_type;

		/** string for the type of object used for the symbol on the parse stack. */
		public string stack_type() {return _stack_type;}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Count of how many times the symbol appears in productions. */
		protected int _use_count = 0;

		/** Count of how many times the symbol appears in productions. */
		public int use_count() {return _use_count;}

		/** Increment the use count. */ 
		public void note_use() {_use_count++;}
 
		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/
 
		/** Index of this symbol (terminal or non terminal) in the parse tables.
		 *  Note: indexes are unique among terminals and unique among non terminals,
		 *  however, a terminal may have the same index as a non-terminal, etc. 
		 */
		protected int _index;
 
		/** Index of this symbol (terminal or non terminal) in the parse tables.
		 *  Note: indexes are unique among terminals and unique among non terminals,
		 *  however, a terminal may have the same index as a non-terminal, etc. 
		 */
		public int index() {return _index;}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Indicate if this is a non-terminal.  Here in the base class we
		 *  don't know, so this is abstract.  
		 */
		public abstract bool is_non_term();

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Convert to a string. */
		public override string ToString()
		{
			return name();
		}

		/*-----------------------------------------------------------*/

	}
}
