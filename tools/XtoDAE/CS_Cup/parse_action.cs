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

	/** This class serves as the base class for entries in a parse action table.  
	 *  Full entries will either be SHIFT(state_num), REDUCE(production), NONASSOC,
	 *  or ERROR. Objects of this base class will default to ERROR, while
	 *  the other three types will be represented by subclasses. 
	 * 
	 * @see     java_cup.reduce_action
	 * @see     java_cup.shift_action
	 * @version last updated: 7/2/96
	 * @author  Frank Flannery
     * translated to C# 08.09.2003 by Samuel Imriska
	 */

	public  class parse_action 
	{

		/*-----------------------------------------------------------*/
		/*--- Constructor(s) ----------------------------------------*/
		/*-----------------------------------------------------------*/

		/** Simple constructor. */
		public parse_action()
		{
			/* nothing to do in the base class */
		}

 
		/*-----------------------------------------------------------*/
		/*--- (Access to) Static (Class) Variables ------------------*/
		/*-----------------------------------------------------------*/

		/** Constant for action type -- error action. */
		public const int ERROR = 0;

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Constant for action type -- shift action. */
		public const int SHIFT = 1;

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Constants for action type -- reduce action. */
		public const int REDUCE = 2;

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Constants for action type -- reduce action. */
		public const int NONASSOC = 3;

		/*-----------------------------------------------------------*/
		/*--- General Methods ---------------------------------------*/
		/*-----------------------------------------------------------*/
	 
		/** Quick access to the type -- base class defaults to error. */
        public virtual int kind() {return ERROR;} 

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Equality test. */
		public bool Equals(parse_action other)
		{
			/* we match all error actions */
			return other != null && other.kind() == kind();
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Generic equality test. */
		public override bool Equals(object other)
		{
			if (other.GetType()==typeof(parse_action))
												 return other != null && ((parse_action)other).kind() == kind();
			else
				return false;
		}
		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Compute a hash code. */
		public override int GetHashCode()
		{
			/* all objects of this class hash together */
			return 0xCafe123;
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Convert to string. */
		public override string ToString() {return "ERROR";}

		/*-----------------------------------------------------------*/
	}
}   
