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

	/** This class represents a part of a production which is a symbol (terminal
	 *  or non terminal).  This simply maintains a reference to the symbol in 
	 *  question.
	 *
	 * @see     java_cup.production
	 * @version last updated: 11/25/95
	 * @author  Scott Hudson
     * translated to C# 08.09.2003 by Samuel Imriska
	 */
	public class symbol_part : production_part 
	{

		/*-----------------------------------------------------------*/
		/*--- Constructor(s) ----------------------------------------*/
		/*-----------------------------------------------------------*/

		/** Full constructor. 
		 * @param sym the symbol that this part is made up of.
		 * @param lab an optional label string for the part.
		 */
		public symbol_part(symbol sym, string lab) : base(lab)
												   {

		if (sym == null)
		throw new internal_error(
				  "Attempt to construct a symbol_part with a null symbol");
		_the_symbol = sym;
	}

	/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

	/** Constructor with no label. 
	 * @param sym the symbol that this part is made up of.
	 */
        public symbol_part(symbol sym): this( sym,null){}

	/*-----------------------------------------------------------*/
	/*--- (Access to) Instance Variables ------------------------*/
	/*-----------------------------------------------------------*/

	/** The symbol that this part is made up of. */
	protected symbol _the_symbol;

	/** The symbol that this part is made up of. */
	public symbol the_symbol() {return _the_symbol;}

	/*-----------------------------------------------------------*/
	/*--- General Methods ---------------------------------------*/
	/*-----------------------------------------------------------*/

	/** Respond that we are not an action part. */
	 public override bool is_action() { return false; }

	/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

	/** Equality comparison. */
	public bool Equals(symbol_part other)
{
	return other != null && base.Equals(other) && 
	the_symbol().Equals(other.the_symbol());
}

	/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

	/** Generic equality comparison. */
	 public override bool Equals(object other)
{
	if (other.GetType()!=typeof(symbol_part))
	return false;
	else
	return Equals((symbol_part)other);
}

	/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

	/** Produce a hash code. */
	 public override int GetHashCode()
{
	return base.GetHashCode() ^ 
	(the_symbol()==null ? 0 : the_symbol().GetHashCode());
}

	/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

	/** Convert to a string. */
	  public  override string ToString()
{
	if (the_symbol() != null)
	return base.ToString() + the_symbol();
	else
	return base.ToString() + "$$MISSING-SYMBOL$$";
}

	/*-----------------------------------------------------------*/

}
}
