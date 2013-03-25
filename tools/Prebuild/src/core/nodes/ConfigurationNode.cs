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
using System.Xml;

using DNPreBuild.Core.Attributes;
using DNPreBuild.Core.Interfaces;
using DNPreBuild.Core.Util;

namespace DNPreBuild.Core.Nodes
{
	[DataNode("Configuration")]
	public class ConfigurationNode : DataNode, ICloneable
	{
		#region Fields

		private string m_Name = "unknown";
		private OptionsNode m_Options = null;

		#endregion

		#region Constructors

		public ConfigurationNode()
		{
			m_Options = new OptionsNode();
		}

		#endregion

		#region Properties

		public override IDataNode Parent
		{
			get
			{
				return base.Parent;
			}
			set
			{
				base.Parent = value;
				if(base.Parent is SolutionNode)
				{
					SolutionNode node = (SolutionNode)base.Parent;
					if(node != null && node.Options != null)
						node.Options.CopyTo(m_Options);
				}
			}
		}

		public string Name
		{
			get
			{
				return m_Name;
			}
		}

		public OptionsNode Options
		{
			get
			{
				return m_Options;
			}
			set
			{
				m_Options = value;
			}
		}

		#endregion

		#region Public Methods

		public override void Parse(XmlNode node)
		{
			m_Name = Helper.AttributeValue(node, "name", m_Name);
			foreach(XmlNode child in node.ChildNodes)
			{
				IDataNode dataNode = Kernel.Instance.ParseNode(child, this);
				if(dataNode is OptionsNode)
					((OptionsNode)dataNode).CopyTo(m_Options);
			}
		}

		public void CopyTo(ConfigurationNode conf)
		{
			m_Options.CopyTo(conf.m_Options);
		}

		#endregion

		#region ICloneable Members

		public object Clone()
		{
			ConfigurationNode ret = new ConfigurationNode();
			ret.m_Name = m_Name;
			m_Options.CopyTo(ret.m_Options);
			return ret;
		}

		#endregion
	}
}
