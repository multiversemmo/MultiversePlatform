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
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

using DNPreBuild.Core.Attributes;
using DNPreBuild.Core.Interfaces;
using DNPreBuild.Core.Util;

namespace DNPreBuild.Core.Nodes
{
	[DataNode("Match")]
	public class MatchNode : DataNode
	{
		#region Fields

		private StringCollection m_Files = null;
		private Regex m_Regex = null;
		private BuildAction m_BuildAction = BuildAction.Compile;

		#endregion

		#region Constructors

		public MatchNode()
		{
			m_Files = new StringCollection();
		}

		#endregion

		#region Properties

		public StringCollection Files
		{
			get
			{
				return m_Files;
			}
		}

		public BuildAction BuildAction
		{
			get
			{
				return m_BuildAction;
			}
		}

		#endregion

		#region Private Methods

		public void RecurseDirs(string path, string pattern, bool recurse, bool useRegex)
		{
			try
			{
				string[] files;

				if(!useRegex)
				{
					files = Directory.GetFiles(path, pattern);
					if(files != null)
						m_Files.AddRange(files);
					else
						return;
				}
				else
				{
					Match match;
					files = Directory.GetFiles(path);
					foreach(string file in files)
					{
						match = m_Regex.Match(file);
						if(match.Success)
							m_Files.Add(file);
					}
				}
                
				if(recurse)
				{
					string[] dirs = Directory.GetDirectories(path);
					if(dirs != null && dirs.Length > 0)
					{
						foreach(string str in dirs)
							RecurseDirs(Helper.NormalizePath(str), pattern, recurse, useRegex);
					}
				}
			}
			catch(DirectoryNotFoundException)
			{
				return;
			}
			catch(ArgumentException)
			{
				return;
			}
		}

		#endregion

		#region Public Methods

		public override void Parse(XmlNode node)
		{
			string path = Helper.AttributeValue(node, "path", ".");
			string pattern = Helper.AttributeValue(node, "pattern", "*");
			bool recurse = (bool)Helper.TranslateValue(typeof(bool), Helper.AttributeValue(node, "recurse", "false"));
			bool useRegex = (bool)Helper.TranslateValue(typeof(bool), Helper.AttributeValue(node, "useRegex", "false"));
			m_BuildAction = (BuildAction)Enum.Parse(typeof(BuildAction), 
				Helper.AttributeValue(node, "buildAction", m_BuildAction.ToString()));

			if(path == null || path == string.Empty)
				path = ".";//use current directory
			//throw new WarningException("Match must have a 'path' attribute");

			if(pattern == null)
				throw new WarningException("Match must have a 'pattern' attribute");

			path = Helper.NormalizePath(path);
			if(!Directory.Exists(path))
				throw new WarningException("Match path does not exist: {0}", path);

			try
			{
				if(useRegex)
					m_Regex = new Regex(pattern);
			}
			catch(ArgumentException ex)
			{
				throw new WarningException("Could not compile regex pattern: {0}", ex.Message);
			}

			RecurseDirs(path, pattern, recurse, useRegex);
			if(m_Files.Count < 1)
				throw new WarningException("Match returned no files: {0}{1}", Helper.EndPath(path), pattern);

			m_Regex = null;
		}

		#endregion
	}
}
