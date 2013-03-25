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
	 * Defines the Scanner interface, which CUP uses in the default
	 * implementation of <code>lr_parser.scan()</code>.  Integration
	 * of scanners implementing <code>Scanner</code> is facilitated.
	 *
	 * @version last updated 23-Jul-1999
	 * @author David MacMahon <davidm@smartsc.com>
     * translated to C# 08.09.2003 by Samuel Imriska
	 */

	/* *************************************************
	  Interface Scanner
  
	  Declares the next_token() method that should be
	  implemented by scanners.  This method is typically
	  called by lr_parser.scan().  End-of-file can be
	  indicated either by returning
	  <code>new Symbol(lr_parser.EOF_sym())</code> or
	  <code>null</code>.
	 ***************************************************/
	public interface Scanner 
	{
		/** Return the next token, or <code>null</code> on end-of-file. */
		 Symbol next_token();
	}
}
