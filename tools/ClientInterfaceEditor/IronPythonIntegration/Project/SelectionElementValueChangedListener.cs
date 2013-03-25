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
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Package;
using Microsoft.Win32;
using EnvDTE;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Microsoft.Samples.VisualStudio.IronPythonLanguageService;

namespace Microsoft.Samples.VisualStudio.IronPythonProject
{
	public class SelectionElementValueChangedListener : SelectionListener
    {
        #region fileds
        private ProjectNode projMgr;
        #endregion
        #region ctors
        public SelectionElementValueChangedListener(ServiceProvider serviceProvider, ProjectNode proj)
            : base(serviceProvider)
		{
            projMgr = proj;
		}
		#endregion

        #region overridden methods
        public override int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            int hr = VSConstants.S_OK;
            if (elementid == VSConstants.DocumentFrame)
            {
                
                IVsWindowFrame pWindowFrame = varValueOld as IVsWindowFrame;
                if(pWindowFrame != null)
                {
                    object document;
                    // Get the name of the document associated with the old window frame
                    hr = pWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out document);
                    if (ErrorHandler.Succeeded(hr))
                    {
                        uint itemid;
                        IVsHierarchy hier = projMgr as IVsHierarchy;
                        hr = hier.ParseCanonicalName((string)document, out itemid);
                        PythonFileNode node = projMgr.NodeFromItemId(itemid) as PythonFileNode;
                        if (null != node)
                        {
                            node.RunGenerator();
                        }
                    }
                 }
            }

            return hr;
        }
        #endregion

    }
}
