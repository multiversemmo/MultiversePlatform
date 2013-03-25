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

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService {
    internal class TextLineEventListener : IVsTextLinesEvents, IDisposable {
        private const int defaultDelay = 2000;

        private string fileName;
        private ModuleId fileId;
        private IVsTextLines buffer;
        private bool isDirty;

        private IConnectionPoint connectionPoint;
        private uint connectionCookie;

        public TextLineEventListener(IVsTextLines buffer, string fileName, ModuleId id)
        {
            this.buffer = buffer;
            this.fileId = id;
            this.fileName = fileName;
            IConnectionPointContainer container = buffer as IConnectionPointContainer;
            if (null != container) {
                Guid eventsGuid = typeof(IVsTextLinesEvents).GUID;
                container.FindConnectionPoint(ref eventsGuid, out connectionPoint);
                connectionPoint.Advise(this as IVsTextLinesEvents, out connectionCookie);
            }
        }

        #region Properties
        public ModuleId FileID {
            get { return fileId; }
        }
        public string FileName {
            get { return fileName; }
            set { fileName = value; }
        }
        #endregion

        #region Events
        private EventHandler<HierarchyEventArgs> onFileChanged;
        public event EventHandler<HierarchyEventArgs> OnFileChanged {
            add { onFileChanged += value; }
            remove { onFileChanged -= value; }
        }

        public event TextLineChangeEvent OnFileChangedImmediate;

        #endregion

        #region IVsTextLinesEvents Members
        void IVsTextLinesEvents.OnChangeLineAttributes(int iFirstLine, int iLastLine) {
            // Do Nothing
        }

        void IVsTextLinesEvents.OnChangeLineText(TextLineChange[] pTextLineChange, int fLast) {
            TextLineChangeEvent eh = OnFileChangedImmediate;
            if (null != eh) {
                eh(this, pTextLineChange, fLast);
            }

            isDirty = true;
        }
        #endregion

        #region IDisposable Members
        public void Dispose() {
            if ((null != connectionPoint) && (0 != connectionCookie)) {
                connectionPoint.Unadvise(connectionCookie);
                System.Diagnostics.Debug.WriteLine("\n\tUnadvised from TextLinesEvents\n");
            }
            connectionCookie = 0;
            connectionPoint = null;

            this.buffer = null;
            this.fileId = null;
        }
        #endregion

        #region Idle time processing
        public void OnIdle() {
            if (!isDirty) {
                return;
            }
            if (null != onFileChanged) {
                HierarchyEventArgs args = new HierarchyEventArgs(fileId.ItemID, fileName);
                args.TextBuffer = buffer;
                onFileChanged(fileId.Hierarchy, args);
            }

            isDirty = false;
        }
        #endregion
    }
}
