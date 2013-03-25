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

using Microsoft.VisualStudio.TextManager.Interop;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService
{
    internal class SnippetsEnumerator : IEnumerable<VsExpansion> {
        /// <summary>
        /// This structure is used to facilitate the interop calls to the method
        /// exposed by IVsExpansionEnumeration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ExpansionBuffer {
            public IntPtr pathPtr;
            public IntPtr titlePtr;
            public IntPtr shortcutPtr;
            public IntPtr descriptionPtr;
        }

        private IVsTextManager2 textManager;
        private Guid languageGuid;
        private bool shortcutOnly;

        public SnippetsEnumerator(IVsTextManager2 textManager, Guid languageGuid) {
            if (null == textManager) {
                throw new ArgumentNullException("textManager");
            }
            this.textManager = textManager;
            this.languageGuid = languageGuid;
        }

        public bool ShortcutOnly {
            get { return this.shortcutOnly; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            set { this.shortcutOnly = value; }
        }

        #region IEnumerable<VsExpansion> Members
        public IEnumerator<VsExpansion> GetEnumerator() {
            IVsExpansionManager expansionManager;
            ErrorHandler.ThrowOnFailure(textManager.GetExpansionManager(out expansionManager));

            IVsExpansionEnumeration enumerator;
            int onlyShortcut = (this.ShortcutOnly ? 1 : 0);
            ErrorHandler.ThrowOnFailure(expansionManager.EnumerateExpansions(languageGuid, onlyShortcut, null, 0, 0, 0, out enumerator));

            ExpansionBuffer buffer = new ExpansionBuffer();
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try {
                int hr = VSConstants.S_OK;
                uint fetched;
                while (VSConstants.S_OK == (hr = enumerator.Next(1, new IntPtr[] { handle.AddrOfPinnedObject() }, out fetched))) {
                    buffer = (ExpansionBuffer)handle.Target;
                    try {
                        handle.Free();
                        if (IntPtr.Zero != buffer.shortcutPtr) {
                            VsExpansion expansion = new VsExpansion();
                            expansion.shortcut = Marshal.PtrToStringBSTR(buffer.shortcutPtr);
                            if (IntPtr.Zero != buffer.descriptionPtr) {
                                expansion.description = Marshal.PtrToStringBSTR(buffer.descriptionPtr);
                            }
                            if (IntPtr.Zero != buffer.pathPtr) {
                                expansion.path = Marshal.PtrToStringBSTR(buffer.pathPtr);
                            }
                            if (IntPtr.Zero != buffer.titlePtr) {
                                expansion.title = Marshal.PtrToStringBSTR(buffer.titlePtr);
                            }
                            yield return expansion;
                            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                        }
                    } finally {
                        if (IntPtr.Zero != buffer.descriptionPtr) {
                            Marshal.FreeBSTR(buffer.descriptionPtr);
                            buffer.descriptionPtr = IntPtr.Zero;
                        }
                        if (IntPtr.Zero != buffer.pathPtr) {
                            Marshal.FreeBSTR(buffer.pathPtr);
                            buffer.pathPtr = IntPtr.Zero;
                        }
                        if (IntPtr.Zero != buffer.shortcutPtr) {
                            Marshal.FreeBSTR(buffer.shortcutPtr);
                            buffer.shortcutPtr = IntPtr.Zero;
                        }
                        if (IntPtr.Zero != buffer.titlePtr) {
                            Marshal.FreeBSTR(buffer.titlePtr);
                            buffer.titlePtr = IntPtr.Zero;
                        }
                    }
                }
                ErrorHandler.ThrowOnFailure(hr);
            } finally {
                if (handle.IsAllocated) {
                    handle.Free();
                }
            }
        }
        #endregion

        #region IEnumerable Members
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
        #endregion
    }
}
