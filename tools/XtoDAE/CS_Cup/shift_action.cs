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


	/** This class represents a shift action within the parse table. 
	 *  The action simply stores the state that it shifts to and responds 
	 *  to queries about its type.
	 *
	 * @version last updated: 11/25/95
	 * @author  Scott Hudson
     * translated to C# 08.09.2003 by Samuel Imriska
	 */
	public class shift_action : parse_action 
	{

		/*-----------------------------------------------------------*/
		/*--- Constructor(s) ----------------------------------------*/
		/*-----------------------------------------------------------*/

		/** Simple constructor. 
		 * @param shft_to the state that this action shifts to.
		 */
		public shift_action(lalr_state shft_to) 
												{
													/* sanity check */
													if (shft_to == null)
		throw new internal_error(
				  "Attempt to create a shift_action to a null state");

		_shift_to = shft_to;
	}

	/*-----------------------------------------------------------*/
	/*--- (Access to) Instance Variables ------------------------*/
	/*-----------------------------------------------------------*/

	/** The state we shift to. */
	protected lalr_state _shift_to;

	/** The state we shift to. */
	public lalr_state shift_to() {return _shift_to;}

	/*-----------------------------------------------------------*/
	/*--- General Methods ---------------------------------------*/
	/*-----------------------------------------------------------*/

	/** Quick access to type of action. */
	  public override int kind() {return SHIFT;}

	/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

	/** Equality test. */
	public bool Equals(shift_action other)
{
	return other != null && other.shift_to() == shift_to();
}

	/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

	/** Generic equality test. */
	 public override  bool Equals(object other)
{
	if (other.GetType()==typeof(shift_action))
	return Equals((shift_action)other);
	else
	return false;
}

	/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

	/** Compute a hash code. */
	 public override int GetHashCode()
{
	/* use the hash code of the state we are shifting to */
	return shift_to().GetHashCode();
}

	/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

	/** Convert to a string. */
	  public override  string ToString() {return "SHIFT(to state " + shift_to().index() + ")";}

	/*-----------------------------------------------------------*/

}
}
