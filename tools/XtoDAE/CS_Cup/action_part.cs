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

	/** 
	 * This class represents a part of a production which contains an
	 * action.  These are eventually eliminated from productions and converted
	 * to trailing actions by factoring out with a production that derives the
	 * empty string (and ends with this action).
	 *
	 * @see java_cup.production
	 * @version last update: 11/25/95
	 * @author Scott Hudson
     * translated to C# 08.09.2003 by Samuel Imriska
	 */

	public class action_part: production_part 
	{

		/*-----------------------------------------------------------*/
		/*--- Constructors ------------------------------------------*/
		/*-----------------------------------------------------------*/

		/** Simple constructor. 
		 * @param code_str string containing the actual user code.
		 */
		public action_part(string code_str):base(null)
		{
			_code_string = code_str;
		}

		/*-----------------------------------------------------------*/
		/*--- (Access to) Instance Variables ------------------------*/
		/*-----------------------------------------------------------*/

		/** string containing code for the action in question. */
		protected string _code_string;

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** string containing code for the action in question. */
		public string code_string() {return _code_string;}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Set the code string. */
		public void set_code_string(string new_str) {_code_string = new_str;}

		/*-----------------------------------------------------------*/
		/*--- General Methods ---------------------------------------*/
		/*-----------------------------------------------------------*/

		/** Override to report this object as an action. */
		 public override bool is_action() { return true; }

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Equality comparison for properly typed object. */
		public bool Equals(action_part other)
		{
			/* compare the strings */
			return other != null && base.Equals(other) && 
				other.code_string().Equals(code_string());
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Generic equality comparison. */
		 public override bool Equals(object other)
		{
			if (other.GetType()!=typeof(action_part)) 
				return false;
			else
				return Equals((action_part)other);
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Produce a hash code. */
		 public override int GetHashCode()
		{
			return base.GetHashCode() ^ 
				(code_string()==null ? 0 : code_string().GetHashCode());
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Convert to a string.  */
		 public override  string ToString()
		{
			return base.ToString() + "{" + code_string() + "}";
		}

		/*-----------------------------------------------------------*/
	}
}
