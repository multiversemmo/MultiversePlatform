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
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

using System.ComponentModel.Design;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService {

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideLoadKey(PythonConstants.PLKMinEdition, PythonConstants.PLKProductVersion, PythonConstants.PLKProductName, PythonConstants.PLKCompanyName, PythonConstants.PLKResourceID)]
    [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\9.0Exp")]
    [ProvideService(typeof(PythonLanguage), ServiceName = "Python")]
    [ProvideService(typeof(IPythonLibraryManager))]
    [ProvideLanguageService(typeof(PythonLanguage), "Python", 100,
        CodeSense = true,
        DefaultToInsertSpaces = true,
        EnableCommenting = true,
        MatchBraces = true,
        ShowCompletion = true,
        ShowMatchingBrace = true)]
    [ProvideLanguageExtension(typeof(PythonLanguage), PythonConstants.pythonFileExtension)]
    [ProvideIntellisenseProvider(typeof(PythonIntellisenseProvider), "IronPythonCodeProvider", "Iron Python", ".py", "IronPython;Python", "IronPython")]
    [ProvideObject(typeof(PythonIntellisenseProvider))]
    [Guid(PythonConstants.packageGuidString)]
    [RegisterSnippetsAttribute(PythonConstants.languageServiceGuidString, false, 131, "Python", @"CodeSnippets\SnippetsIndex.xml", @"CodeSnippets\Snippets\", @"CodeSnippets\Snippets\")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class PythonPackage : Package, IOleComponent {
        private uint componentID;
        private PythonLibraryManager libraryManager;

        public PythonPackage() {
            IServiceContainer container = this as IServiceContainer;
            ServiceCreatorCallback callback = new ServiceCreatorCallback(CreateService);
            container.AddService(typeof(PythonLanguage), callback, true);
            container.AddService(typeof(IPythonLibraryManager), callback, true);
        }

        private void RegisterForIdleTime() {
            IOleComponentManager mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
            if (componentID == 0 && mgr != null) {
                OLECRINFO[] crinfo = new OLECRINFO[1];
                crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime |
                                              (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal |
                                              (uint)_OLECADVF.olecadvfRedrawOff |
                                              (uint)_OLECADVF.olecadvfWarningsOff;
                crinfo[0].uIdleTimeInterval = 1000;
                int hr = mgr.FRegisterComponent(this, crinfo, out componentID);
            }
        }

        protected override void Dispose(bool disposing) {
            try {
                if (componentID != 0) {
                    IOleComponentManager mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
                    if (mgr != null) {
                        mgr.FRevokeComponent(componentID);
                    }
                    componentID = 0;
                }
                if (null != libraryManager) {
                    libraryManager.Dispose();
                    libraryManager = null;
                }
            } finally {
                base.Dispose(disposing);
            }
        }

        private object CreateService(IServiceContainer container, Type serviceType) {
            object service = null;
            if (typeof(PythonLanguage) == serviceType) {
                PythonLanguage language = new PythonLanguage();
                language.SetSite(this);
                RegisterForIdleTime();
                service = language;
            } else if (typeof(IPythonLibraryManager) == serviceType) {
                libraryManager = new PythonLibraryManager(this);
                service = libraryManager as IPythonLibraryManager;
            }
            return service;
        }

        #region IOleComponent Members

        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked) {
            return 1;
        }

        public int FDoIdle(uint grfidlef) {
            PythonLanguage pl = GetService(typeof(PythonLanguage)) as PythonLanguage;
            if (pl != null) {
                pl.OnIdle((grfidlef & (uint)_OLEIDLEF.oleidlefPeriodic) != 0);
            }
            if (null != libraryManager) {
                libraryManager.OnIdle();
            }
            return 0;
        }

        public int FPreTranslateMessage(MSG[] pMsg) {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser) {
            return 1;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam) {
            return 1;
        }

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved) {
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved) {
        }

        public void OnAppActivate(int fActive, uint dwOtherThreadID) {
        }

        public void OnEnterState(uint uStateID, int fEnter) {
        }

        public void OnLoseActivation() {
        }

        public void Terminate() {
        }

        #endregion
    }
}
