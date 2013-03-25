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
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.InteropServices;

// Platform references
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;

// IronPython namespaces.
using Microsoft.Samples.VisualStudio.IronPythonLanguageService;
using IronPython.Hosting;

// Unit test framework.
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// Namespace of the class to test
using Microsoft.Samples.VisualStudio.IronPythonInterfaces;
using Microsoft.Samples.VisualStudio.IronPythonConsole;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    public class TestConsoleException : Exception
    {
    }

    /// <summary>
    /// Summary description for ConsoleWindowTest
    /// </summary>
    [TestClass]
    public class ConsoleWindowTest
    {

        [TestMethod]
        public void WindowConstructorNullProvider()
        {
            // Verify that the constructor throws a ArgumentNull exception if the
            // service provider is not set.
            bool exceptionThrown = false;
            try
            {
                object consoleWindow = CommandWindowHelper.CreateConsoleWindow(null);
            }
            catch (ArgumentNullException)
            {
                exceptionThrown = true;
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                ArgumentNullException inner = e.InnerException as ArgumentNullException;
                if (null != inner)
                    exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public void WindowConstructor()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock object for the text buffer.
                BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();
                // Create a new local registry class.
                LocalRegistryMock mockRegistry = new LocalRegistryMock();
                // Add the text buffer to the list of the classes that local registry can create.
                mockRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);
                provider.AddService(typeof(SLocalRegistry), mockRegistry, false);

                // Now create the object and verify that the constructor sets the site for the text buffer.
                using (IDisposable consoleObject = CommandWindowHelper.CreateConsoleWindow(provider) as IDisposable)
                {
                    Assert.IsNotNull(consoleObject);
                    Assert.IsTrue(0 < textLinesMock.FunctionCalls(string.Format("{0}.{1}", typeof(IObjectWithSite).FullName, "SetSite")));
                }
            }
        }

        [TestMethod]
        public void StandardConstructor()
        {
            using (OleServiceProvider provider = OleServiceProvider.CreateOleServiceProviderWithBasicServices())
            {
                IVsPackage package = null;
                try
                {
                    // Create a mock object for the text buffer.
                    BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();
                    // Create a new local registry class.
                    LocalRegistryMock mockRegistry = new LocalRegistryMock();
                    // Add the text buffer to the list of the classes that local registry can create.
                    mockRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);
                    provider.AddService(typeof(SLocalRegistry), mockRegistry, false);

                    // Now create a package object and site it.
                    package = new PythonConsolePackage() as IVsPackage;
                    package.SetSite(provider);

                    // Create a console window using the standard constructor and verify that the 
                    // text buffer is created and sited.
                    using (IDisposable consoleObject = CommandWindowHelper.CreateConsoleWindow() as IDisposable)
                    {
                        Assert.IsTrue(0 < textLinesMock.FunctionCalls(string.Format("{0}.{1}", typeof(IObjectWithSite).FullName, "SetSite")));
                    }
                }
                finally
                {
                    if (null != package)
                    {
                        package.SetSite(null);
                        package.Close();
                    }
                }
            }
        }

        [TestMethod]
        public void WindowPaneImplementation()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the tool window.
                using (IDisposable disposableObject = CommandWindowHelper.CreateConsoleWindow(provider) as IDisposable)
                {
                    IVsWindowPane windowPane = disposableObject as IVsWindowPane;
                    Assert.IsNotNull(windowPane);

                    // Now call the IVsWindowPane's methods and check that they are redirect to
                    // the implementation provided by the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        windowPane.CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));
                    Assert.IsTrue(1 == textViewMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsWindowPane).FullName, "CreatePaneWindow")));

                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        windowPane.GetDefaultSize(null)));
                    Assert.IsTrue(1 == textViewMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsWindowPane).FullName, "GetDefaultSize")));

                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        windowPane.LoadViewState(null)));
                    Assert.IsTrue(1 == textViewMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsWindowPane).FullName, "LoadViewState")));

                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        windowPane.SaveViewState(null)));
                    Assert.IsTrue(1 == textViewMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsWindowPane).FullName, "SaveViewState")));

                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        windowPane.SetSite(null)));
                    Assert.IsTrue(1 == textViewMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsWindowPane).FullName, "SetSite")));

                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        windowPane.TranslateAccelerator(null)));
                    Assert.IsTrue(1 == textViewMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsWindowPane).FullName, "TranslateAccelerator")));

                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        windowPane.ClosePane()));
                    Assert.IsTrue(1 == textViewMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsWindowPane).FullName, "ClosePane")));
                }
                // Verify that the text view is closed after Dispose is called on the window pane.
                Assert.IsTrue(1 == textViewMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextView).FullName, "CloseView")));
            }
        }

        private static void TextViewSetSiteCallback(object sender, CallbackArgs args)
        {
            Assert.IsNotNull(args.GetParameter(0));
        }
        private static void TextViewInitializeCallback(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            // Verify that the view is sited and that a text buffer is provided.
            Assert.IsTrue(1 == mock.FunctionCalls(string.Format("{0}.{1}", typeof(IObjectWithSite), "SetSite")));
            IVsTextLines textLines = args.GetParameter(0) as IVsTextLines;
            Assert.IsNotNull(textLines);
            // This text view is not supposed to be initialized using a parent window.
            Assert.IsTrue(IntPtr.Zero == (IntPtr)args.GetParameter(1));
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        [TestMethod]
        public void TextViewCreation()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                textViewMock.AddMethodCallback(string.Format("{0}.{1}", typeof(IObjectWithSite).FullName, "SetSite"),
                                               new EventHandler<CallbackArgs>(TextViewSetSiteCallback));
                textViewMock.AddMethodCallback(string.Format("{0}.{1}", typeof(IVsTextView).FullName, "Initialize"),
                                               new EventHandler<CallbackArgs>(TextViewInitializeCallback));
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the tool window.
                using (IDisposable disposableObject = CommandWindowHelper.CreateConsoleWindow(provider) as IDisposable)
                {
                    IVsWindowPane windowPane = disposableObject as IVsWindowPane;
                    Assert.IsNotNull(windowPane);

                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        windowPane.CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Verify that the text view was used as expected.
                    Assert.IsTrue(1 == textViewMock.FunctionCalls(string.Format("{0}.{1}", typeof(IObjectWithSite), "SetSite")));
                    Assert.IsTrue(1 == textViewMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextView), "Initialize")));
                }
            }
        }

        [TestMethod]
        public void ViewCreationWithLanguage()
        {
            using (OleServiceProvider provider = OleServiceProvider.CreateOleServiceProviderWithBasicServices())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();
                // The buffer have to handle a few of connection points in order to enable the
                // creation of a Source object from the language service.
                ConnectionPointHelper.AddConnectionPointsToContainer(
                    textLinesMock,
                    new Type[] { typeof(IVsFinalTextChangeCommitEvents), typeof(IVsTextLinesEvents), typeof(IVsUserDataEvents) });

                // Create the local registry mock and add the text buffer to it.
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                // Create the connection point for IVsTextViewEvents (needed for the language service).
                ConnectionPointHelper.AddConnectionPointsToContainer(textViewMock, new Type[] { typeof(IVsTextViewEvents) });

                // Add the text view to the local registry.
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                MockPackage package = new MockPackage();
                ((IVsPackage)package).SetSite(provider);
                provider.AddService(typeof(Microsoft.VisualStudio.Shell.Package), package, true);

                // Create the language service and add it to the list of services.
                PythonLanguage language = new MockLanguage();
                provider.AddService(typeof(PythonLanguage), language, true);
                language.SetSite(provider);

                // We need to add a method tip window to the local registry in order to create
                // a Source object.
                IVsMethodTipWindow methodTip = MockFactories.MethodTipFactory.GetInstance() as IVsMethodTipWindow;
                mockLocalRegistry.AddClass(typeof(VsMethodTipWindowClass), methodTip);

                // Create a mock expansion manager that is needed for the language service.
                BaseMock expansionManager = MockFactories.ExpansionManagerFactory.GetInstance();
                ConnectionPointHelper.AddConnectionPointsToContainer(expansionManager, new Type[] { typeof(IVsExpansionEvents) });
                Assembly asm = typeof(Microsoft.VisualStudio.Package.LanguageService).Assembly;
                Type expMgrType = asm.GetType("Microsoft.VisualStudio.Package.SVsExpansionManager");
                provider.AddService(expMgrType, expansionManager, false);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    Assert.IsNotNull(windowPane);

                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Verify that the language service contains a special view for this text view.
                    FieldInfo specialSourcesField = typeof(PythonLanguage).GetField("specialSources", BindingFlags.Instance | BindingFlags.NonPublic);
                    Assert.IsNotNull(specialSourcesField);
                    Dictionary<IVsTextView, PythonSource> specialSources =
                        (Dictionary<IVsTextView, PythonSource>)specialSourcesField.GetValue(language);
                    PythonSource source;
                    Assert.IsTrue(specialSources.TryGetValue(textViewMock as IVsTextView, out source));
                    Assert.IsNotNull(source);
                    // Set ColorState to null so that Dispose will not call Marshal.ReleaseComObject on it.
                    source.ColorState = null;
                }
            }
        }

        [TestMethod]
        public void EngineInitialization()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create a mock engine provider.
                BaseMock mockEngineProvider = MockFactories.EngineProviderFactory.GetInstance();
                // Create a mock engine.
                BaseMock mockEngine = MockFactories.CreateStandardEngine();
                // Set this engine as the one returned from the GetSharedEngine of the engine provider.
                mockEngineProvider.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IPythonEngineProvider), "GetSharedEngine"),
                    new object[] { (IEngine)mockEngine });
                // Add the engine provider to the list of the services.
                provider.AddService(typeof(IPythonEngineProvider), mockEngineProvider, false);

                // Create the console window
                using (IDisposable disposableObject = CommandWindowHelper.CreateConsoleWindow(provider) as IDisposable)
                {
                    IVsWindowPane windowPane = disposableObject as IVsWindowPane;
                    Assert.IsNotNull(windowPane);

                    // Verify that the shared engine was get.
                    Assert.IsTrue(1 == mockEngineProvider.FunctionCalls(string.Format("{0}.{1}", typeof(IPythonEngineProvider), "GetSharedEngine")));
                    Assert.IsTrue(1 == mockEngine.FunctionCalls(string.Format("{0}.{1}", typeof(IEngine), "set_StdErr")));
                    Assert.IsTrue(1 == mockEngine.FunctionCalls(string.Format("{0}.{1}", typeof(IEngine), "set_StdOut")));
                }
            }
        }

        private static void SetEngineStdErr(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            mock["StdErr"] = args.GetParameter(0);
        }
        private static void SetEngineStdOut(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            mock["StdOut"] = args.GetParameter(0);
        }
        private static void ReplaceLinesCallback(object sender, CallbackArgs args)
        {
            IntPtr stringPointer = (IntPtr)args.GetParameter(4);
            int stringLen = (int)args.GetParameter(5);
            Assert.IsTrue(IntPtr.Zero != stringPointer);
            Assert.IsTrue(stringLen > 0);
            string newText = Marshal.PtrToStringAuto(stringPointer, stringLen);
            BaseMock mock = (BaseMock)sender;
            mock["Text"] = (string)mock["Text"] + newText;
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        [TestMethod]
        public void EngineStreams()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();
                textLinesMock["Text"] = "";
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create a mock engine provider.
                BaseMock mockEngineProvider = MockFactories.EngineProviderFactory.GetInstance();
                // Create a mock engine.
                BaseMock mockEngine = MockFactories.CreateStandardEngine();
                // Add the callbacks for the setter methods of stderr and stdout
                mockEngine.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IEngine).FullName, "set_StdErr"),
                    new EventHandler<CallbackArgs>(SetEngineStdErr));
                mockEngine.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IEngine).FullName, "set_StdOut"),
                    new EventHandler<CallbackArgs>(SetEngineStdOut));
                // Set this engine as the one returned from the GetSharedEngine of the engine provider.
                mockEngineProvider.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IPythonEngineProvider), "GetSharedEngine"),
                    new object[] { (IEngine)mockEngine });
                // Add the engine provider to the list of the services.
                provider.AddService(typeof(IPythonEngineProvider), mockEngineProvider, false);

                // Create the console window.
                using (IDisposable disposableObject = CommandWindowHelper.CreateConsoleWindow(provider) as IDisposable)
                {
                    IVsWindowPane windowPane = disposableObject as IVsWindowPane;
                    Assert.IsNotNull(windowPane);
                    Assert.IsNotNull(mockEngine["StdErr"]);
                    Assert.IsNotNull(mockEngine["StdOut"]);

                    // Set the callback for the text buffer.
                    textLinesMock.AddMethodCallback(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines"),
                        new EventHandler<CallbackArgs>(ReplaceLinesCallback));

                    // Verify that the standard error stream is associated with the text buffer.
                    System.IO.Stream stream = (System.IO.Stream)mockEngine["StdErr"];
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream))
                    {
                        writer.Write("Test String");
                        writer.Flush();
                        Assert.IsTrue((string)textLinesMock["Text"] == "Test String");
                        textLinesMock["Text"] = "";
                    }

                    // Verify the standard output.
                    stream = (System.IO.Stream)mockEngine["StdOut"];
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream))
                    {
                        writer.Write("Test String");
                        writer.Flush();
                        Assert.IsTrue((string)textLinesMock["Text"] == "Test String");
                        textLinesMock["Text"] = "";
                    }
                }
            }
        }

        private static void AddCommandFilterCallback(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            mock["CommandFilter"] = (IOleCommandTarget)args.GetParameter(0);
            args.SetParameter(1, (IOleCommandTarget)mock["OriginalFilter"]);
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        [TestMethod]
        public void VerifyCommandFilter()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                textViewMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IVsTextView).FullName, "AddCommandFilter"),
                    new EventHandler<CallbackArgs>(AddCommandFilterCallback));
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Create a command target that handles some random command
                OleMenuCommandService commandService = new OleMenuCommandService(provider);
                Guid newCommandGroup = Guid.NewGuid();
                uint newCommandId = 42;
                CommandID id = new CommandID(newCommandGroup, (int)newCommandId);
                OleMenuCommand cmd = new OleMenuCommand(null, id);
                commandService.AddCommand(cmd);
                textViewMock["OriginalFilter"] = (IOleCommandTarget)commandService;

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the window.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    Assert.IsNotNull(windowPane);

                    // Verify that the command specific to the text view are not handled yet.
                    CommandTargetHelper commandHelper = new CommandTargetHelper((IOleCommandTarget)windowPane);
                    uint flags;
                    Assert.IsFalse(commandHelper.IsCommandSupported(
                                        typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                        (int)(int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.RETURN,
                                        out flags));
                    Assert.IsFalse(commandHelper.IsCommandSupported(
                                        typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                        (int)(int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UP,
                                        out flags));
                    Assert.IsFalse(commandHelper.IsCommandSupported(
                                        typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                        (int)(int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.DOWN,
                                        out flags));
                    Assert.IsFalse(commandHelper.IsCommandSupported(
                                        typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                        (int)(int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU,
                                        out flags));
                    // Verify that also the command that we have defined here is not supported.
                    Assert.IsFalse(commandHelper.IsCommandSupported(newCommandGroup, newCommandId, out flags));

                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    // Now the command filter should be set for the text view
                    Assert.IsNotNull(textViewMock["CommandFilter"]);
                    // The command target for the window pane should also be able to support
                    // the text view specific command that we have installed.
                    // Verify only two commands that are always supported
                    Assert.IsTrue(commandHelper.IsCommandSupported(
                                        typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                        (int)(int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.RETURN,
                                        out flags));
                    Assert.IsTrue(commandHelper.IsCommandSupported(
                                        typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                        (int)(int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL,
                                        out flags));
                    Assert.IsTrue(commandHelper.IsCommandSupported(
                                        typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                        (int)(int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU,
                                        out flags));
                    // Verify that also the commands supported by the original command target are
                    // supported by the new one.
                    Assert.IsTrue(commandHelper.IsCommandSupported(newCommandGroup, newCommandId, out flags));
                }
            }
        }

        [TestMethod]
        public void ReadOnlyRegionAfterWrite()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();

                // Add the buffer to the local registry.
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console window.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Get the stream from the window pane.
                    System.IO.Stream consoleStream = CommandWindowHelper.ConsoleStream(windowPane);
                    Assert.IsNotNull(consoleStream);

                    // Set a return value for GetLastLineIndex
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLastLineIndex"),
                        new object[] { 0, 12, 35 });

                    // Write some text on the stream.
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(consoleStream);
                    writer.Write("");
                    writer.Flush();

                    // Verify that the ResetSpan method for the text marker was called and that
                    // the span is set to cover all the current buffer.
                    BaseMock markerMock = (BaseMock)textLinesMock["LineMarker"];
                    Assert.IsTrue(1 == markerMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLineMarker).FullName, "ResetSpan")));
                    TextSpan span = (TextSpan)markerMock["Span"];
                    Assert.IsTrue(0 == span.iStartLine);
                    Assert.IsTrue(0 == span.iStartIndex);
                    Assert.IsTrue(12 == span.iEndLine);
                    Assert.IsTrue(35 == span.iEndIndex);

                    // Change the end point of the buffer and try again.
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLastLineIndex"),
                        new object[] { 0, 15, 3 });
                    writer.Write("abc");
                    writer.Flush();
                    Assert.IsTrue(2 == markerMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLineMarker).FullName, "ResetSpan")));
                    span = (TextSpan)markerMock["Span"];
                    Assert.IsTrue(0 == span.iStartLine);
                    Assert.IsTrue(0 == span.iStartIndex);
                    Assert.IsTrue(15 == span.iEndLine);
                    Assert.IsTrue(3 == span.iEndIndex);
                }
            }
        }

        private static void GetLineTextCallbackForConsoleTextOfLine(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            int expectedLine = (int)mock["ExpectedLine"];
            Assert.IsTrue(expectedLine == (int)args.GetParameter(0));
            Assert.IsTrue(expectedLine == (int)args.GetParameter(2));

            int expectedStart = (int)mock["ExpectedStart"];
            Assert.IsTrue(expectedStart == (int)args.GetParameter(1));

            int expectedEnd = (int)mock["ExpectedEnd"];
            Assert.IsTrue(expectedEnd == (int)args.GetParameter(3));

            args.SetParameter(4, (string)mock["LineText"]);

            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        [TestMethod]
        public void ConsoleTextOfLineNoMarker()
        {
            string testString = "Test";
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();
                textLinesMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineText"),
                    new EventHandler<CallbackArgs>(GetLineTextCallbackForConsoleTextOfLine));
                textLinesMock["LineText"] = testString;
                textLinesMock["ExpectedLine"] = 1;
                textLinesMock["ExpectedStart"] = 0;
                textLinesMock["ExpectedEnd"] = 10;

                // Create a new local registry class.
                LocalRegistryMock mockRegistry = new LocalRegistryMock();
                // Add the text buffer to the list of the classes that local registry can create.
                mockRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Add the local registry to the service provider.
                provider.AddService(typeof(SLocalRegistry), mockRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    IConsoleText consoleText = windowPane as IConsoleText;
                    Assert.IsNull(consoleText.TextOfLine(1, -1, true));
                    Assert.IsNull(consoleText.TextOfLine(1, -1, false));
                    string text = consoleText.TextOfLine(1, 10, false);
                    Assert.IsTrue(testString == text);
                }
            }
        }

        [TestMethod]
        public void ConsoleTextOfLineWithMarker()
        {
            string testString1 = "Test 1";
            string testString2 = "Test 2";
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();
                textLinesMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineText"),
                    new EventHandler<CallbackArgs>(GetLineTextCallbackForConsoleTextOfLine));

                // Create a new local registry class.
                LocalRegistryMock mockRegistry = new LocalRegistryMock();
                // Add the text buffer to the list of the classes that local registry can create.
                mockRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Add the local registry to the service provider.
                provider.AddService(typeof(SLocalRegistry), mockRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Make sure that the text marker is created.
                    CommandWindowHelper.EnsureConsoleTextMarker(windowPane);

                    // Set the span of the marker.
                    TextSpan span = new TextSpan();
                    span.iStartLine = 0;
                    span.iStartIndex = 0;
                    span.iEndLine = 3;
                    span.iEndIndex = 5;
                    BaseMock markerMock = (BaseMock)textLinesMock["LineMarker"];
                    markerMock["Span"] = span;

                    IConsoleText consoleText = windowPane as IConsoleText;

                    // Verify the case that the requested line is all inside the
                    // read only region.
                    textLinesMock["LineText"] = testString1;
                    textLinesMock["ExpectedLine"] = 1;
                    textLinesMock["ExpectedStart"] = 0;
                    textLinesMock["ExpectedEnd"] = 10;
                    Assert.IsNull(consoleText.TextOfLine(1, 10, true));
                    string text = consoleText.TextOfLine(1, 10, false);
                    Assert.IsTrue(text == testString1);

                    // Now ask for some text inside the read-only region, but on its last line.
                    textLinesMock["LineText"] = testString2;
                    textLinesMock["ExpectedLine"] = 3;
                    textLinesMock["ExpectedStart"] = 0;
                    textLinesMock["ExpectedEnd"] = 4;
                    Assert.IsNull(consoleText.TextOfLine(3, 4, true));
                    text = consoleText.TextOfLine(3, 4, false);
                    Assert.IsTrue(text == testString2);

                    // Now the text is part inside and part outside the read-only region.
                    textLinesMock["LineText"] = testString1;
                    textLinesMock["ExpectedLine"] = 3;
                    textLinesMock["ExpectedStart"] = 5;
                    textLinesMock["ExpectedEnd"] = 10;
                    text = consoleText.TextOfLine(3, 10, true);
                    Assert.IsTrue(testString1 == text);
                    textLinesMock["LineText"] = testString2;
                    textLinesMock["ExpectedLine"] = 3;
                    textLinesMock["ExpectedStart"] = 0;
                    textLinesMock["ExpectedEnd"] = 10;
                    text = consoleText.TextOfLine(3, 10, false);
                    Assert.IsTrue(text == testString2);

                    // Now the line has no intersection with the read-only region.
                    textLinesMock["LineText"] = testString1;
                    textLinesMock["ExpectedLine"] = 4;
                    textLinesMock["ExpectedStart"] = 0;
                    textLinesMock["ExpectedEnd"] = 10;
                    text = consoleText.TextOfLine(4, 10, true);
                    Assert.IsTrue(testString1 == text);
                    textLinesMock["LineText"] = testString2;
                    textLinesMock["ExpectedLine"] = 4;
                    textLinesMock["ExpectedStart"] = 0;
                    textLinesMock["ExpectedEnd"] = 10;
                    text = consoleText.TextOfLine(4, 10, false);
                    Assert.IsTrue(text == testString2);
                }
            }
        }
    }
}
