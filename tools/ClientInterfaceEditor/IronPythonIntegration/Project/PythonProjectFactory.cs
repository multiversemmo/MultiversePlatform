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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.Samples.VisualStudio.IronPythonProject
{
    /// <summary>
    /// Creates Python Projects
    /// </summary>
    [Guid(GuidList.guidPythonProjectFactoryString)]
    public class PythonProjectFactory : Microsoft.VisualStudio.Package.ProjectFactory
    {
        #region ctor
        /// <summary>
        /// Constructor for PythonProjectFactory
        /// </summary>
        /// <param name="package">the package who created this object</param>
        public PythonProjectFactory(PythonProjectPackage package)
            : base(package)
        {
        }
        #endregion

        #region overridden methods
        /// <summary>
        /// Creates the Python Project node
        /// </summary>
        /// <returns>the new instance of the Python Project node</returns>
        protected override Microsoft.VisualStudio.Package.ProjectNode CreateProject()
        {
            PythonProjectNode project = new PythonProjectNode(this.Package as PythonProjectPackage);
            project.SetSite((IOleServiceProvider)((IServiceProvider)this.Package).GetService(typeof(IOleServiceProvider)));
            return project;
        }
        #endregion
    }

    /// <summary>
    /// This class is a 'fake' project factory that is used by WAP to register WAP specific information about
    /// IronPython projects.
    /// </summary>
    [GuidAttribute("0C1E5196-4828-499e-9F72-98268B955B28")]
    public class WAPythonProjectFactory { }


}
