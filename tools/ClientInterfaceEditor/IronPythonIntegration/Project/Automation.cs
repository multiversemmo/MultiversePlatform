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
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using System.Reflection;
using IServiceProvider = System.IServiceProvider;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Package.Automation;
using Microsoft.VisualStudio.Designer.Interfaces;

using Microsoft.Samples.VisualStudio.CodeDomCodeModel;

namespace Microsoft.Samples.VisualStudio.IronPythonProject
{
	/// <summary>
	/// Add support for automation on py files.
	/// </summary>
    [ComVisible(true)]
	[Guid("CCD70EB5-E3FE-454f-AD14-C945E9F04250")]
	public class OAIronPythonFileItem : OAFileItem
    {
        #region variables
        private EnvDTE.FileCodeModel codeModel;
        #endregion

        #region ctors
        public OAIronPythonFileItem(OAProject project, FileNode node)
			: base(project, node)
		{
		}
		#endregion

		#region overridden methods
        public override EnvDTE.FileCodeModel FileCodeModel
        {
            get
            {
                if (null != codeModel)
                {
                    return codeModel;
                }
                if ((null == this.Node) || (null == this.Node.OleServiceProvider))
                {
                    return null;
                }
                ServiceProvider sp = new ServiceProvider(this.Node.OleServiceProvider);
                IVSMDCodeDomProvider smdProvider = sp.GetService(typeof(SVSMDCodeDomProvider)) as IVSMDCodeDomProvider;
                if (null == smdProvider)
                {
                    return null;
                }
                CodeDomProvider provider = smdProvider.CodeDomProvider as CodeDomProvider;
                codeModel = PythonCodeModelFactory.CreateFileCodeModel(this as EnvDTE.ProjectItem, provider, this.Node.Url);
                return codeModel;
            }
        }
        
        public override EnvDTE.Window Open(string viewKind)
		{
			if (string.Compare(viewKind, EnvDTE.Constants.vsViewKindPrimary) == 0)
			{
				// Get the subtype and decide the viewkind based on the result
				if (((PythonFileNode)this.Node).IsFormSubType)
				{
					return base.Open(EnvDTE.Constants.vsViewKindDesigner);
				}
			}
			return base.Open(viewKind);
		}
		#endregion
	}

    [ComVisible(true)]
    public class OAIronPythonProject : OAProject
    {
        public OAIronPythonProject(PythonProjectNode pythonProject)
            : base(pythonProject)
        {
        }

        public override EnvDTE.CodeModel CodeModel
        {
            get 
            {
                return PythonCodeModelFactory.CreateProjectCodeModel(this);
            }
        }
    }

}
