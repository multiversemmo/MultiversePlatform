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


/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.ComponentModel;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Package.Automation;
using Microsoft.Win32;
using EnvDTE;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.Samples.VisualStudio.IronPythonProject
{
	[ComVisible(true), CLSCompliant(false)]
	[Guid("BF389FD8-F382-41b1-B502-63CB11254137")]
	public class PythonFileNodeProperties : SingleFileGeneratorNodeProperties
	{
		#region ctors
		public PythonFileNodeProperties(HierarchyNode node)
			: base(node)
		{
		}
		#endregion

		#region properties
		[Browsable(false)]
		public string Url
		{
			get
			{
				return "file:///" + this.Node.Url;
			}
		}
		[Browsable(false)]
		public string SubType
		{
			get
			{
				return ((PythonFileNode)this.Node).SubType;
			}
			set
			{
				((PythonFileNode)this.Node).SubType = value;
			}
		}

		[Microsoft.VisualStudio.Package.SRCategoryAttribute(Microsoft.VisualStudio.Package.SR.Advanced)]
		[Microsoft.VisualStudio.Package.LocDisplayName(Microsoft.VisualStudio.Package.SR.BuildAction)]
		[Microsoft.VisualStudio.Package.SRDescriptionAttribute(Microsoft.VisualStudio.Package.SR.BuildActionDescription)]
		public virtual PythonBuildAction PythonBuildAction
		{
			get
			{
				string value = this.Node.ItemNode.ItemName;
				if (value == null || value.Length == 0)
				{
					return PythonBuildAction.None;
				}
				return (PythonBuildAction)Enum.Parse(typeof(PythonBuildAction), value);
			}
			set
			{
				this.Node.ItemNode.ItemName = value.ToString();
			}
		}

		[Browsable(false)]
		public override BuildAction BuildAction
		{
			get
			{
				switch(this.PythonBuildAction)
				{
					case PythonBuildAction.ApplicationDefinition:
					case PythonBuildAction.Page:
					case PythonBuildAction.Resource:
						return BuildAction.Compile;
					default:
						return (BuildAction)Enum.Parse(typeof(BuildAction), this.PythonBuildAction.ToString());
				}
			}
			set
			{
				this.PythonBuildAction = (PythonBuildAction)Enum.Parse(typeof(PythonBuildAction), value.ToString());
			}
		}
		#endregion
	}

	public enum PythonBuildAction { None, Compile, Content, EmbeddedResource, ApplicationDefinition, Page, Resource };
}
