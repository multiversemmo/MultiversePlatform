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

	/** This class represents a shift/reduce nonassociative error within the 
	 *  parse table.  If action_table element is assign to type
	 *  nonassoc_action, it cannot be changed, and signifies that there 
	 *  is a conflict between shifting and reducing a production and a
	 *  terminal that shouldn't be next to each other.
	 *
	 * @version last updated: 7/2/96
	 * @author  Frank Flannery
     * translated to C# 08.09.2003 by Samuel Imriska
	 */
	public class nonassoc_action : parse_action 
	{
 
		/*-----------------------------------------------------------*/
		/*--- Constructor(s) ----------------------------------------*/
		/*-----------------------------------------------------------*/

		/** Simple constructor. 
		 */
		public nonassoc_action() 
								 {
									 /* don't need to Set anything, since it signifies error */
								 }

		/*-----------------------------------------------------------*/
		/*--- General Methods ---------------------------------------*/
		/*-----------------------------------------------------------*/

		/** Quick access to type of action. */
		  public override int kind() {return NONASSOC;}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Equality test. */
		 new  public  bool Equals(parse_action other)
		{
			return other != null && other.kind() == NONASSOC;
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Generic equality test. */
		 public override bool Equals(object other)
		{
			if (other.GetType()==typeof(parse_action))
												 return Equals((parse_action)other);
			else
				return false;
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Compute a hash code. */
		public override  int GetHashCode()
		{
			/* all objects of this class hash together */
			return 0xCafe321;
		}



		/** Convert to string. */
		  public override  string ToString() 
		{
			return "NONASSOC";
		}

		/*-----------------------------------------------------------*/

	}
}
