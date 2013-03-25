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
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService {

    internal partial class PythonViewFilter : ViewFilter {

        public PythonViewFilter(CodeWindowManager mgr, IVsTextView view)
            : base(mgr, view) {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override void Dispose() {
            try {
                this.bufferCoordinator = null;
                base.Dispose();
            } catch(Exception) {
            }
        }

        protected override int QueryCommandStatus(ref Guid guidCmdGroup, uint nCmdId) {
            if (guidCmdGroup == VSConstants.VSStd2K) {
                if (nCmdId == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET || 
                    nCmdId == (uint)VSConstants.VSStd2KCmdID.SURROUNDWITH) {
                    return (int)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                }
            }

            return base.QueryCommandStatus(ref guidCmdGroup, nCmdId);
        }

        public override bool HandlePreExec(ref Guid guidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            if (guidCmdGroup == VSConstants.VSStd2K) {
                if (nCmdId == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET) {
                    ExpansionProvider ep = this.GetExpansionProvider();
                    if (this.TextView != null && ep != null) {
                        ep.DisplayExpansionBrowser(this.TextView, Resources.InsertSnippet, null, false, null, false);
                    }
                    return true;   // Handled the command.
                } else if (nCmdId == (uint)VSConstants.VSStd2KCmdID.SURROUNDWITH) {
                    ExpansionProvider ep = this.GetExpansionProvider();
                    if (this.TextView != null && ep != null) {
                        ep.DisplayExpansionBrowser(this.TextView, Resources.SurroundWith, null, false, null, false);
                    }
                    return true;   // Handled the command.
                }
            }

            // Base class handled the command.  Do nothing more here.
            return base.HandlePreExec(ref guidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
        }

    }
}
