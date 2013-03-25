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

	/** This class represents one part (either a symbol or an action) of a 
	 *  production.  In this base class it contains only an optional label 
	 *  string that the user can use to refer to the part within actions.<p>
	 *
	 *  This is an abstract class.
	 *
	 * @see     java_cup.production
	 * @version last updated: 11/25/95
	 * @author  Scott Hudson
     * translated to C# 08.09.2003 by Samuel Imriska
	 */
	public abstract class production_part 
	{

		/*-----------------------------------------------------------*/
		/*--- Constructor(s) ----------------------------------------*/
		/*-----------------------------------------------------------*/
       
		/** Simple constructor. */
		public production_part(string lab)
		{
			_label = lab;
		}

		/*-----------------------------------------------------------*/
		/*--- (Access to) Instance Variables ------------------------*/
		/*-----------------------------------------------------------*/
       
		/** Optional label for referring to the part within an action (null for 
		 *  no label). 
		 */
		protected string _label;

		/** Optional label for referring to the part within an action (null for 
		 *  no label). 
		 */
		public string label() {return _label;}

		/*-----------------------------------------------------------*/
		/*--- General Methods ---------------------------------------*/
		/*-----------------------------------------------------------*/
       
		/** Indicate if this is an action (rather than a symbol).  Here in the 
		 * base class, we don't this know yet, so its an abstract method.
		 */
		public abstract bool is_action();

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Equality comparison. */
		public bool Equals(production_part other)
		{
			if (other == null) return false;

			/* compare the labels */
			if (label() != null)
				return label().Equals(other.label());
			else
				return other.label() == null;
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Generic equality comparison. */
		public override bool Equals(object other)
		{
			if (other.GetType()!=typeof(production_part))
													   return false;
			else
				return Equals((production_part)other);
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Produce a hash code. */
		public override int GetHashCode()
		{
			return label()==null ? 0 : label().GetHashCode();
		}

		/*. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .*/

		/** Convert to a string. */
		 public  override string ToString()
		{
			if (label() != null)
				return label() + ":";
			else
				return " ";
		}

		/*-----------------------------------------------------------*/

	}
}
