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

#region BSD License
/*
Copyright (c) 2004-2005 Matthew Holmes (matthew@wildfiregames.com), Dan Moorehead (dan05a@gmail.com)

Redistribution and use in source and binary forms, with or without modification, are permitted
provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions 
  and the following disclaimer. 
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions 
  and the following disclaimer in the documentation and/or other materials provided with the 
  distribution. 
* The name of the author may not be used to endorse or promote products derived from this software 
  without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

#region CVS Information
/*
 * $Source$
 * $Author: mccollum $
 * $Date: 2006-09-08 17:12:07 -0700 (Fri, 08 Sep 2006) $
 * $Revision: 6434 $
 */
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;

namespace DNPreBuild.Core.Util
{    
	/// <summary>
	/// The CommandLine class parses and interprets the command-line arguments passed to
	/// dnpb.
	/// </summary>
	public class CommandLine : IEnumerable 
	{
		#region Fields

		// The raw OS arguments
		private string[] m_RawArgs = null;

		// Command-line argument storage
		private Hashtable m_Arguments = null;
        
		#endregion
        
		#region Constructors
        
		/// <summary>
		/// Create a new CommandLine instance and set some internal variables.
		/// </summary>
		public CommandLine(string[] args) 
		{
			m_RawArgs = args;
			m_Arguments = new Hashtable();
            
			Parse();
		}

		#endregion

		#region Private Methods

		private void Parse() 
		{
			if(m_RawArgs.Length < 1)
				return;

			int idx = 0;
			string arg = null, lastArg = null;

			while(idx <m_RawArgs.Length) 
			{
				arg = m_RawArgs[idx];

				if(arg.Length > 2 && arg[0] == '/') 
				{
					arg = arg.Substring(1);
					lastArg = arg;
					m_Arguments[arg] = "";
				} 
				else 
				{
					if(lastArg != null)
					{
						m_Arguments[lastArg] = arg;
						lastArg = null;
					}
				}

				idx++;
			}
		}

		#endregion

		#region Public Methods

		public bool WasPassed(string arg)
		{
			return (m_Arguments.ContainsKey(arg));
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the parameter associated with the command line option
		/// </summary>
		/// <remarks>Returns null if option was not specified,
		/// null string if no parameter was specified, and the value if a parameter was specified</remarks>
		public string this[string idx] 
		{
			get 
			{
				if(m_Arguments.ContainsKey(idx))
					return (string)(m_Arguments[idx]);
				else
					return null;
			}
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator() 
		{
			return m_Arguments.Keys.GetEnumerator();
		}

		#endregion
	}
}
