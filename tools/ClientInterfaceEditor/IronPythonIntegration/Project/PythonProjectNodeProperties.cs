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
	[ComVisible(true), CLSCompliant(false), System.Runtime.InteropServices.ClassInterface(ClassInterfaceType.AutoDual)]
	[Guid("4A383C3B-4734-4d21-B5DF-B4B2CE07AEF4")]
    public class PythonProjectNodeProperties : ProjectNodeProperties
    {
        #region ctors
        public PythonProjectNodeProperties(ProjectNode node)
            : base(node)
        {
        }
        #endregion

        #region properties
        [Browsable(false)]
        public string OutputFileName
        {
            get
            {
                return ((PythonProjectNode)(this.Node.ProjectMgr)).OutputFileName;
            }
        }
        /// <summary>
        /// Returns/Sets the MainFile project property
        /// </summary>
        [Browsable(false)]
        public string MainFile
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty(PythonProjectFileConstants.MainFile, true);
            }
            //set
            //{
            //    // Set relative path to file as mainfile property                
            //    this.Node.ProjectMgr.SetProjectProperty(PythonProjectFileConstants.MainFile, value);
            //}
        }

        [Browsable(false)]
        public string AssemblyName
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty(ProjectFileConstants.AssemblyName);
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty(ProjectFileConstants.AssemblyName, value);
            }
        }

        [Browsable(false)]
        public string DefaultNamespace
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty(ProjectFileConstants.RootNamespace);
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty(ProjectFileConstants.RootNamespace, value);
            }
        }

        [Browsable(false)]
        public string RootNamespace
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty(ProjectFileConstants.RootNamespace);
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty(ProjectFileConstants.RootNamespace, value);
            }
        }

        [Browsable(false)]
        public string OutputType
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty(ProjectFileConstants.OutputType);
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty(ProjectFileConstants.OutputType, value);
            }
        }

        #endregion
    }
}
