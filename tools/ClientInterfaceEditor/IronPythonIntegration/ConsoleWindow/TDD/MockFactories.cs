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

// Visual Studio Platform
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Package;

// Unit Test Framework
using Microsoft.VsSDK.UnitTestLibrary;

using Microsoft.Samples.VisualStudio.IronPythonConsole;
using Microsoft.Samples.VisualStudio.IronPythonInterfaces;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    internal static class MockFactories
    {
        private static GenericMockFactory dteFactory;
        public static GenericMockFactory DTEFactory
        {
            get
            {
                if (null == dteFactory)
                {
                    Type[] dteInterfaces = new Type[]{
                        typeof(EnvDTE._DTE),
                    };
                    dteFactory = new GenericMockFactory("MockDTE", dteInterfaces);
                }
                return dteFactory;
            }
        }

        private static GenericMockFactory engineFactory;
        public static GenericMockFactory EngineFactory
        {
            get
            {
                if (null == engineFactory)
                {
                    Type[] interfaces = new Type[] {
                        typeof(IEngine)
                    };
                    engineFactory = new GenericMockFactory("EngineMock", interfaces);
                }
                return engineFactory;
            }
        }
        public static BaseMock CreateStandardEngine()
        {
            BaseMock mock = EngineFactory.GetInstance();
            mock.AddMethodReturnValues(
                string.Format("{0}.{1}", typeof(IEngine), "get_Version"),
                new object[] { new Version(1, 1, 1) });
            return mock;
        }

        private static GenericMockFactory engineProviderFactory;
        public static GenericMockFactory EngineProviderFactory
        {
            get
            {
                if (null == engineProviderFactory)
                {
                    Type[] interfaces = new Type[] {
                        typeof(IPythonEngineProvider)
                    };
                    engineProviderFactory = new GenericMockFactory("EngineProviderMock", interfaces);
                }
                return engineProviderFactory;
            }
        }

        private static GenericMockFactory textBufferFactory;
        public static GenericMockFactory TextBufferFactory
        {
            get
            {
                if (null == textBufferFactory)
                {
                    Type[] textBufferInterfaces = new Type[] {
                        typeof(IVsTextLines),
                        typeof(IObjectWithSite),
                        typeof(IVsTextColorState),
                        typeof(IVsExpansion),
                        typeof(IConnectionPointContainer)
                    };
                    textBufferFactory = new GenericMockFactory("EmptyTextLinesMock", textBufferInterfaces);
                }
                return textBufferFactory;
            }
        }

        private static void CreateMarkerCallback(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            IVsTextLineMarker[] markers = (IVsTextLineMarker[])args.GetParameter(6);
            BaseMock markerMock = (BaseMock)mock["LineMarker"];
            TextSpan span = new TextSpan();
            span.iStartLine = (int)args.GetParameter(1);
            span.iStartIndex = (int)args.GetParameter(2);
            span.iEndLine = (int)args.GetParameter(3);
            span.iEndIndex = (int)args.GetParameter(4);
            markerMock["Span"] = span;
            markers[0] = (IVsTextLineMarker)markerMock;
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        private static void StandardMarkerResetSpanCallback(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            TextSpan span = new TextSpan();
            span.iStartLine = (int)args.GetParameter(0);
            span.iStartIndex = (int)args.GetParameter(1);
            span.iEndLine = (int)args.GetParameter(2);
            span.iEndIndex = (int)args.GetParameter(3);
            mock["Span"] = span;
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        private static void StandardMarkerGetCurrentSpanCallback(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            TextSpan[] spans = (TextSpan[])args.GetParameter(0);
            spans[0] = (TextSpan)mock["Span"];
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        public static BaseMock CreateBufferWithMarker()
        {
            BaseMock bufferMock = TextBufferFactory.GetInstance();
            BaseMock markerMock = TextLineMarkerFactory.GetInstance();
            markerMock.AddMethodCallback(
                string.Format("{0}.{1}", typeof(IVsTextLineMarker).FullName, "ResetSpan"),
                new EventHandler<CallbackArgs>(StandardMarkerResetSpanCallback));
            markerMock.AddMethodCallback(
                string.Format("{0}.{1}", typeof(IVsTextLineMarker).FullName, "GetCurrentSpan"),
                new EventHandler<CallbackArgs>(StandardMarkerGetCurrentSpanCallback));
            bufferMock["LineMarker"] = markerMock;
            bufferMock.AddMethodCallback(
                string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "CreateLineMarker"),
                new EventHandler<CallbackArgs>(CreateMarkerCallback));
            return bufferMock;
        }

        private static GenericMockFactory textLineMarkerFactory;
        public static GenericMockFactory TextLineMarkerFactory
        {
            get
            {
                if (null == textLineMarkerFactory)
                {
                    textLineMarkerFactory = new GenericMockFactory("EmptyTextMarker", new Type[] { typeof(IVsTextLineMarker) });
                }
                return textLineMarkerFactory;
            }
        }

        private static GenericMockFactory textViewFactory;
        public static GenericMockFactory TextViewFactory
        {
            get
            {
                if (null == textViewFactory)
                {
                    Type[] textViewInterfaces = new Type[] {
                        typeof(IVsTextView),
                        typeof(IVsWindowPane),
                        typeof(IObjectWithSite),
                        typeof(IConnectionPointContainer)
                    };
                    textViewFactory = new GenericMockFactory("EmptyTextViewMock", textViewInterfaces);
                }
                return textViewFactory;
            }
        }

        private static GenericMockFactory uiShellFactory;
        public static GenericMockFactory UIShellFactory
        {
            get
            {
                if (null == uiShellFactory)
                {
                    uiShellFactory = new GenericMockFactory("EmptyUISHellMock", new Type[] { typeof(IVsUIShell) });
                }
                return uiShellFactory;
            }
        }

        private static GenericMockFactory windowFrameFactory;
        public static GenericMockFactory WindowFrameFactory
        {
            get
            {
                if (null == windowFrameFactory)
                {
                    windowFrameFactory = new GenericMockFactory("EmptyWindowFrameMock", new Type[] { typeof(IVsWindowFrame) });
                }
                return windowFrameFactory;
            }
        }

        private static GenericMockFactory consoleTextFactory;
        public static GenericMockFactory ConsoleTextFactory
        {
            get
            {
                if (null == consoleTextFactory)
                {
                    consoleTextFactory = new GenericMockFactory("EmptyConsoleTextMock", new Type[] { typeof(IConsoleText) });
                }
                return consoleTextFactory;
            }
        }

        private static GenericMockFactory scannerFactory;
        public static GenericMockFactory ScannerFactory
        {
            get
            {
                if (null == scannerFactory)
                {
                    scannerFactory = new GenericMockFactory("EmptyScannerMock", new Type[] { typeof(IScanner) });
                }
                return scannerFactory;
            }
        }

        private static GenericMockFactory methodTipFactory;
        public static GenericMockFactory MethodTipFactory
        {
            get
            {
                if (null == methodTipFactory)
                {
                    methodTipFactory = new GenericMockFactory("EmptyMethodTipWindowMock", new Type[] { typeof(IVsMethodTipWindow) });
                }
                return methodTipFactory;
            }
        }

        private static GenericMockFactory expansionManagerFactory;
        public static GenericMockFactory ExpansionManagerFactory
        {
            get
            {
                if (null == expansionManagerFactory)
                {
                    Type[] interfaces = new Type[] {
                        typeof(IVsExpansionManager),
                        typeof(IConnectionPointContainer)
                    };
                    expansionManagerFactory = new GenericMockFactory("EmptyExpansionManager", interfaces);
                }
                return expansionManagerFactory;
            }
        }
    }
}
