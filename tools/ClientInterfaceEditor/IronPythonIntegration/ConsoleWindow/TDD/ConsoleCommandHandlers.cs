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
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

// Platform references
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;

// IronPython namespaces.
using IronPython.Hosting;

// Unit test framework.
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// Namespace of the class to test
using Microsoft.Samples.VisualStudio.IronPythonInterfaces;
using Microsoft.Samples.VisualStudio.IronPythonConsole;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    [TestClass]
    public class ConsoleCommandHandlers
    {
        // Callback function used to define the OleMenuCommand objects needed to
        // verify the command handler function.
        private static void EmptyMenuCallback(object sender, EventArgs e)
        {
        }

        [TestMethod]
        public void SupportCommandOnInputPositionVerifySender()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Verify OnBeforeHistory can handle a null sender
                    CommandWindowHelper.ExecuteSupportCommandOnInputPosition(windowPane, null);
                    // Verify OnBeforeHistory can handle a sender of unexpected type.
                    CommandWindowHelper.ExecuteSupportCommandOnInputPosition(windowPane, "");
                }
            }
        }

        /// <summary>
        /// Verify that the commands that are supposed to be supported only when the cursor is in
        /// a input position are actually supported / unsupported.
        /// </summary>
        [TestMethod]
        public void InputPositionCommand()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();
                BaseMock lineMarkerMock = (BaseMock)textLinesMock["LineMarker"];

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view and add it to the local registry.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated and this will add the
                    // command handling for the return key.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    // Make sure that the text marker is created.
                    CommandWindowHelper.EnsureConsoleTextMarker(windowPane);

                    // Reset the span of the marker.
                    TextSpan markerSpan = new TextSpan();
                    markerSpan.iStartLine = 0;
                    markerSpan.iStartIndex = 0;
                    markerSpan.iEndLine = 4;
                    markerSpan.iEndIndex = 3;
                    lineMarkerMock["Span"] = markerSpan;

                    // Create the helper class to handle the command target implemented
                    // by the console.
                    CommandTargetHelper helper = new CommandTargetHelper((IOleCommandTarget)windowPane);

                    // Simulate the fact that the cursor is after the end of the marker.
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 4, 7 });
                    // Verify that the commands are supported.
                    uint flags;
                    Assert.IsTrue(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UP,
                            out flags));
                    Assert.IsTrue(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.DOWN,
                            out flags));
                    Assert.IsTrue(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL_EXT,
                            out flags));

                    // Simulate the cursor on the last line, but before the end of the marker.
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 4, 2 });
                    Assert.IsFalse(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UP,
                            out flags));
                    Assert.IsFalse(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.DOWN,
                            out flags));
                    Assert.IsFalse(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL_EXT,
                            out flags));

                    // Simulate the cursor on a line before the end of the marker.
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 1, 7 });
                    Assert.IsFalse(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UP,
                            out flags));
                    Assert.IsFalse(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.DOWN,
                            out flags));
                    Assert.IsFalse(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL_EXT,
                            out flags));

                    // Simulate the cursor on a line after the last.
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 5, 7 });
                    Assert.IsTrue(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UP,
                            out flags));
                    Assert.IsTrue(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.DOWN,
                            out flags));
                    Assert.IsTrue(
                        helper.IsCommandSupported(
                            typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                            (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL_EXT,
                            out flags));
                }
            }
        }

        [TestMethod]
        public void VerifyHistoryEmpty()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view and add it to the local registry.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated and this will add the
                    // commands handling functions.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    // Make sure that the text marker is created.
                    CommandWindowHelper.EnsureConsoleTextMarker(windowPane);

                    // The history should be empty, so no function on the text buffer or text marker
                    // should be called. Reset all the function calls on these objects to verify.
                    BaseMock markerMock = (BaseMock)textLinesMock["LineMarker"];
                    markerMock.ResetAllFunctionCalls();
                    textLinesMock.ResetAllFunctionCalls();

                    // Create the command target helper.
                    CommandTargetHelper helper = new CommandTargetHelper(windowPane as IOleCommandTarget);

                    // Call the command handler for the UP arrow.
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UP);
                    Assert.IsTrue(0 == markerMock.TotalCallsAllFunctions());
                    Assert.IsTrue(0 == textLinesMock.TotalCallsAllFunctions());

                    // Call the command handler for the DOWN arrow.
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.DOWN);
                    Assert.IsTrue(0 == markerMock.TotalCallsAllFunctions());
                    Assert.IsTrue(0 == textLinesMock.TotalCallsAllFunctions());
                }
            }
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
        public void VerifyHistoryOneElement()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view and add it to the local registry.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated and this will add the
                    // commands handling functions.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    // Make sure that the text marker is created.
                    CommandWindowHelper.EnsureConsoleTextMarker(windowPane);

                    // Create the command target helper.
                    CommandTargetHelper helper = new CommandTargetHelper(windowPane as IOleCommandTarget);

                    // Add an element to the history executing a command.
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineCount"),
                        new object[] { 0, 3 });
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 2, 4 });
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLengthOfLine"),
                        new object[] { 0, 3, 10 });
                    BaseMock markerMock = (BaseMock)textLinesMock["LineMarker"];
                    TextSpan span = new TextSpan();
                    span.iEndLine = 2;
                    span.iEndIndex = 4;
                    markerMock["Span"] = span;
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineText"),
                        new object[] { 0, 2, 4, 2, 10, "Line 1" });
                    // Execute the OnReturn handler.
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.RETURN);

                    // Now there should be one element in the history buffer.
                    // Verify that DOWN key does nothing.
                    markerMock.ResetAllFunctionCalls();
                    textLinesMock.ResetAllFunctionCalls();
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.DOWN);
                    Assert.IsTrue(0 == markerMock.TotalCallsAllFunctions());
                    Assert.IsTrue(0 == textLinesMock.TotalCallsAllFunctions());

                    // The UP key should force the "Line 1" text in the last line of the text buffer.
                    textLinesMock.AddMethodCallback(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines"),
                        new EventHandler<CallbackArgs>(ReplaceLinesCallback));
                    textLinesMock["Text"] = "";
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UP);
                    Assert.IsTrue(1 == textLinesMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines")));
                    Assert.IsTrue("Line 1" == (string)textLinesMock["Text"]);
                }
            }
        }

        private static void SetCaretPosCallback(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            mock["CaretLine"] = (int)args.GetParameter(0);
            mock["CaretColumn"] = (int)args.GetParameter(1);
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        [TestMethod]
        public void VerifyOnHome()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view and add it to the local registry.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                textViewMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IVsTextView).FullName, "SetCaretPos"),
                    new EventHandler<CallbackArgs>(SetCaretPosCallback));
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Make sure that the text marker is created.
                    CommandWindowHelper.EnsureConsoleTextMarker(windowPane);

                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated and this will add the
                    // command handling for the return key.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    CommandTargetHelper helper = new CommandTargetHelper(windowPane as IOleCommandTarget);

                    // Simulate the fact that the cursor is on the last line of the buffer.
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineCount"),
                        new object[] { 0, 6 });
                    // Simulate a cursor 3 chars long.
                    BaseMock markerMock = (BaseMock)textLinesMock["LineMarker"];
                    TextSpan span = new TextSpan();
                    span.iEndLine = 5;
                    span.iEndIndex = 3;
                    markerMock["Span"] = span;
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 5, 7 });
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL);
                    Assert.IsTrue(5 == (int)textViewMock["CaretLine"]);
                    Assert.IsTrue(3 == (int)textViewMock["CaretColumn"]);

                    // Simulate the fact that the cursor is before last line of the buffer.
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineCount"),
                        new object[] { 0, 6 });
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 3, 7 });
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL);
                    Assert.IsTrue(3 == (int)textViewMock["CaretLine"]);
                    Assert.IsTrue(0 == (int)textViewMock["CaretColumn"]);

                    // Simulate the fact that the cursor is after last line of the buffer.
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineCount"),
                        new object[] { 0, 6 });
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 8, 7 });
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL);
                    Assert.IsTrue(8 == (int)textViewMock["CaretLine"]);
                    Assert.IsTrue(0 == (int)textViewMock["CaretColumn"]);
                }
            }
        }

        private static void SetSelectionCallback(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            mock["StartLine"] = (int)args.GetParameter(0);
            mock["StartColumn"] = (int)args.GetParameter(1);
            mock["EndLine"] = (int)args.GetParameter(2);
            mock["EndColumn"] = (int)args.GetParameter(3);
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        [TestMethod]
        public void OnShiftHomeTest()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();
                BaseMock lineMarkerMock = (BaseMock)textLinesMock["LineMarker"];

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view and add it to the local registry.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                textViewMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IVsTextView).FullName, "SetSelection"),
                    new EventHandler<CallbackArgs>(SetSelectionCallback));
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Make sure that the text marker is created.
                    CommandWindowHelper.EnsureConsoleTextMarker(windowPane);

                    // Reset the span of the marker.
                    TextSpan markerSpan = new TextSpan();
                    markerSpan.iStartLine = 0;
                    markerSpan.iStartIndex = 0;
                    markerSpan.iEndLine = 4;
                    markerSpan.iEndIndex = 3;
                    lineMarkerMock["Span"] = markerSpan;

                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated and this will add the
                    // command handling for the return key.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    CommandTargetHelper helper = new CommandTargetHelper(windowPane as IOleCommandTarget);

                    // Set the cursor after the end of the marker, but on the same line.
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 4, 7 });
                    helper.ExecCommand(
                        typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                        (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL_EXT);
                    Assert.IsTrue(4 == (int)textViewMock["StartLine"]);
                    Assert.IsTrue(4 == (int)textViewMock["EndLine"]);
                    Assert.IsTrue(7 == (int)textViewMock["StartColumn"]);
                    Assert.IsTrue(3 == (int)textViewMock["EndColumn"]);

                    // Set the cursor before the end of the marker, but on the same line.
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 4, 2 });
                    helper.ExecCommand(
                        typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                        (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL_EXT);
                    Assert.IsTrue(4 == (int)textViewMock["StartLine"]);
                    Assert.IsTrue(4 == (int)textViewMock["EndLine"]);
                    Assert.IsTrue(2 == (int)textViewMock["StartColumn"]);
                    Assert.IsTrue(0 == (int)textViewMock["EndColumn"]);

                    // Set the cursor before the end of the marker, on a different line.
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 2, 8 });
                    helper.ExecCommand(
                        typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                        (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL_EXT);
                    Assert.IsTrue(2 == (int)textViewMock["StartLine"]);
                    Assert.IsTrue(2 == (int)textViewMock["EndLine"]);
                    Assert.IsTrue(8 == (int)textViewMock["StartColumn"]);
                    Assert.IsTrue(0 == (int)textViewMock["EndColumn"]);

                    // Set the cursor after the end of the marker, on a different line.
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 9, 12 });
                    helper.ExecCommand(
                        typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                        (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL_EXT);
                    Assert.IsTrue(9 == (int)textViewMock["StartLine"]);
                    Assert.IsTrue(9 == (int)textViewMock["EndLine"]);
                    Assert.IsTrue(12 == (int)textViewMock["StartColumn"]);
                    Assert.IsTrue(0 == (int)textViewMock["EndColumn"]);
                }
            }
        }

        [TestMethod]
        public void OnBeforeMoveLeftVerifySender()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Verify OnBeforeMoveLeft can handle a null sender
                    CommandWindowHelper.ExecuteOnBeforeMoveLeft(windowPane, null);
                    // Verify OnBeforeMoveLeft can handle a sender of unexpected type.
                    CommandWindowHelper.ExecuteOnBeforeMoveLeft(windowPane, "");
                }
            }
        }

        [TestMethod]
        public void VerifyOnBeforeMoveLeft()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view and add it to the local registry.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated and this will add the
                    // command handling for the return key.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    // Make sure that the text marker is created.
                    CommandWindowHelper.EnsureConsoleTextMarker(windowPane);
                    BaseMock markerMock = (BaseMock)textLinesMock["LineMarker"];

                    // Create a OleMenuCommand to use to call OnBeforeHistory.
                    OleMenuCommand cmd = new OleMenuCommand(new EventHandler(EmptyMenuCallback), new CommandID(Guid.Empty, 0));

                    // Simulate the fact that the cursor is on the last line of the buffer and after the
                    // end of the prompt.
                    cmd.Supported = true;
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineCount"),
                        new object[] { 0, 5 });
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 4, 7 });
                    TextSpan span = new TextSpan();
                    span.iEndIndex = 4;
                    span.iEndLine = 3;
                    markerMock["Span"] = span;
                    CommandWindowHelper.ExecuteOnBeforeMoveLeft(windowPane, cmd);
                    Assert.IsFalse(cmd.Supported);

                    // Simulate the cursor over the prompt.
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 4, 3 });
                    CommandWindowHelper.ExecuteOnBeforeMoveLeft(windowPane, cmd);
                    Assert.IsTrue(cmd.Supported);

                    // Simulate the cursor right after the prompt.
                    cmd.Supported = false;
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 4, 4 });
                    CommandWindowHelper.ExecuteOnBeforeMoveLeft(windowPane, cmd);
                    Assert.IsTrue(cmd.Supported);

                    // Simulate the cursor on a line before the last.
                    cmd.Supported = true;
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 3, 7 });
                    CommandWindowHelper.ExecuteOnBeforeMoveLeft(windowPane, cmd);
                    Assert.IsFalse(cmd.Supported);

                    // Simulate the cursor on a line before the last but over the prompt.
                    cmd.Supported = true;
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 3, 2 });
                    CommandWindowHelper.ExecuteOnBeforeMoveLeft(windowPane, cmd);
                    Assert.IsFalse(cmd.Supported);

                    // Simulate the cursor on a line after the last.
                    cmd.Supported = true;
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 5, 7 });
                    CommandWindowHelper.ExecuteOnBeforeMoveLeft(windowPane, cmd);
                    Assert.IsFalse(cmd.Supported);

                    // Simulate the cursor on a line after the last, but over the prompt.
                    cmd.Supported = true;
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 5, 0 });
                    CommandWindowHelper.ExecuteOnBeforeMoveLeft(windowPane, cmd);
                    Assert.IsFalse(cmd.Supported);
                }
            }
        }

        private static void GetLineTextCallback(object sender, CallbackArgs args)
        {
            int startLine = (int)args.GetParameter(0);
            Assert.IsTrue(12 == startLine);
            int startPos = (int)args.GetParameter(1);
            Assert.IsTrue(4 == startPos);
            int endLine = (int)args.GetParameter(2);
            Assert.IsTrue(endLine == startLine);
            int endPos = (int)args.GetParameter(3);
            Assert.IsTrue(13 == endPos);
            args.SetParameter(4, "Test Line");
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        private static void ExecuteToConsoleCallback(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            mock["ExecutedCommand"] = args.GetParameter(0);
        }
        [TestMethod]
        public void VerifyOnReturn()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();
                textLinesMock.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLastLineIndex"),
                    new object[] { 0, 0, 4 });
                textLinesMock.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLengthOfLine"),
                    new object[] { 0, 0, 13 });
                textLinesMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineText"),
                    new EventHandler<CallbackArgs>(GetLineTextCallback));
                textLinesMock.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineCount"),
                    new object[] { 0, 13 });

                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create a mock engine provider.
                BaseMock mockEngineProvider = MockFactories.EngineProviderFactory.GetInstance();
                // Create a mock engine.
                BaseMock mockEngine = MockFactories.CreateStandardEngine();
                // Set this engine as the one returned from the GetSharedEngine of the engine provider.
                mockEngineProvider.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IPythonEngineProvider), "GetSharedEngine"),
                    new object[] { (IEngine)mockEngine });
                // Set the callback function for the ExecuteToConsole method of the engine.
                mockEngine.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IEngine).FullName, "ExecuteToConsole"),
                    new EventHandler<CallbackArgs>(ExecuteToConsoleCallback));
                // Add the engine provider to the list of the services.
                provider.AddService(typeof(IPythonEngineProvider), mockEngineProvider, false);

                // Create the console window.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Make sure that the text marker is created.
                    CommandWindowHelper.EnsureConsoleTextMarker(windowPane);

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated and this will add the
                    // command handling for the return key.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    // Simulate the cursor on a line different from the last one.
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 5, 8 });

                    // Execute the command handler for the RETURN key.
                    CommandTargetHelper helper = new CommandTargetHelper(windowPane as IOleCommandTarget);
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.RETURN);

                    // In this case nothing should happen because we are not on the input line.
                    Assert.IsTrue(0 == CommandWindowHelper.LinesInInputBuffer(windowPane));

                    // Now simulate the cursor on the input line.
                    textViewMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextView).FullName, "GetCaretPos"),
                        new object[] { 0, 12, 1 });

                    // Make sure that the mock engine can execute the command.
                    mockEngine.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IEngine).FullName, "ParseInteractiveInput"),
                        new object[] { true });
                    // Execute the command.
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.RETURN);

                    // The input buffer should not contain any text because the engine should have
                    // executed it.
                    Assert.AreEqual<int>(0, CommandWindowHelper.LinesInInputBuffer(windowPane));
                    Assert.AreEqual<string>("Test Line", (string)mockEngine["ExecutedCommand"]);

                    // Now change the length of the line so that it is shorter than the
                    // console's prompt.
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLengthOfLine"),
                        new object[] { 0, 0, 3 });

                    // Reset the count of the calls to GetLineText so that we can verify
                    // if it is called.
                    textLinesMock.ResetFunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineText"));
                    // Do the same for the ParseInteractiveInput method of the engine.
                    mockEngine.ResetFunctionCalls(string.Format("{0}.{1}", typeof(IEngine).FullName, "ParseInteractiveInput"));
                    // Simulate a partial statment.
                    mockEngine.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IEngine).FullName, "ParseInteractiveInput"),
                        new object[] { false });

                    // Execute again the command handler.
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.RETURN);

                    // Verify that GetLineText was not called.
                    Assert.IsTrue(0 == textLinesMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLineText")));
                    // Verify that the engine was not called to run an interactive command.
                    Assert.IsTrue(1 == mockEngine.FunctionCalls(string.Format("{0}.{1}", typeof(IEngine).FullName, "ParseInteractiveInput")));
                    // Verify that the console's buffer contains an empty string.
                    Assert.IsTrue(0 == CommandWindowHelper.LinesInInputBuffer(windowPane));
                }
            }
        }

        private void ReplaceLinesCallback_ClearPane(object sender, CallbackArgs args)
        {
            const int arraySize = 4;
            BaseMock mock = (BaseMock)sender;
            int callCount = (int)mock["CallCount"];
            int[] expected = (int[])mock["ReplaceRegion"];
            for (int i = 0; i < arraySize; i++)
            {
                Assert.IsTrue(expected[arraySize * callCount + i] == (int)args.GetParameter(i));
            }
            mock["CallCount"] = callCount + 1;
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        [TestMethod]
        public void OnClearPaneOnlyOneLine()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();
                BaseMock lineMarkerMock = (BaseMock)textLinesMock["LineMarker"];

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view and add it to the local registry.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Make sure that the text marker is created.
                    CommandWindowHelper.EnsureConsoleTextMarker(windowPane);

                    // Reset the span of the marker.
                    TextSpan markerSpan = new TextSpan();
                    markerSpan.iStartLine = 0;
                    markerSpan.iStartIndex = 0;
                    markerSpan.iEndLine = 0;
                    markerSpan.iEndIndex = 3;
                    lineMarkerMock["Span"] = markerSpan;

                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated and this will add the
                    // command handling for the return key.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    CommandTargetHelper helper = new CommandTargetHelper(windowPane as IOleCommandTarget);

                    // Set the last index of the buffer before the end of the line marker.
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLastLineIndex"),
                        new object[] { 0, 0, 2 });
                    // Reset the counters of function calls for the text buffer.
                    textLinesMock.ResetAllFunctionCalls();
                    // Execute the "Clear" command.
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd97CmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd97CmdID.ClearPane);
                    // Verify that ReplaceLines wan never called.
                    Assert.IsTrue(0 == textLinesMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines")));

                    // Set the last index of the buffer after the end of the line marker.
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLastLineIndex"),
                        new object[] { 0, 2, 1 });
                    textLinesMock.AddMethodCallback(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines"),
                        new EventHandler<CallbackArgs>(ReplaceLinesCallback_ClearPane));
                    textLinesMock["ReplaceRegion"] = new int[] { 0, 3, 2, 1 };
                    textLinesMock["CallCount"] = 0;
                    // Reset the counters of function calls for the text buffer.
                    textLinesMock.ResetAllFunctionCalls();
                    // Execute the "Clear" command.
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd97CmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd97CmdID.ClearPane);
                    // Verify that ReplaceLines wan called only once.
                    Assert.IsTrue(1 == textLinesMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines")));
                }
            }
        }

        [TestMethod]
        public void OnClearPaneMultipleLines()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.CreateBufferWithMarker();
                textLinesMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines"),
                    new EventHandler<CallbackArgs>(ReplaceLinesCallback_ClearPane));
                BaseMock lineMarkerMock = (BaseMock)textLinesMock["LineMarker"];

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view and add it to the local registry.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Make sure that the text marker is created.
                    CommandWindowHelper.EnsureConsoleTextMarker(windowPane);

                    // Reset the span of the marker.
                    TextSpan markerSpan = new TextSpan();
                    markerSpan.iStartLine = 0;
                    markerSpan.iStartIndex = 0;
                    markerSpan.iEndLine = 2;
                    markerSpan.iEndIndex = 3;
                    lineMarkerMock["Span"] = markerSpan;

                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated and this will add the
                    // command handling for the return key.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    CommandTargetHelper helper = new CommandTargetHelper(windowPane as IOleCommandTarget);

                    // Set the last index of the buffer after the end of the line marker.
                    textLinesMock.AddMethodReturnValues(
                        string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLastLineIndex"),
                        new object[] { 0, 2, 8 });
                    textLinesMock["ReplaceRegion"] = new int[] { 0, 0, 2, 0, 0, 3, 2, 8 };
                    textLinesMock["CallCount"] = 0;
                    // Reset the counters of function calls for the text buffer.
                    textLinesMock.ResetAllFunctionCalls();
                    // Execute the "Clear" command.
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd97CmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd97CmdID.ClearPane);
                    // Verify that ReplaceLines wan called 2 times.
                    Assert.IsTrue(2 == textLinesMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines")));
                    // Verify that the marker was resized.
                    markerSpan = (TextSpan)lineMarkerMock["Span"];
                    Assert.IsTrue(2 == markerSpan.iStartLine);
                    Assert.IsTrue(0 == markerSpan.iStartIndex);
                    Assert.IsTrue(2 == markerSpan.iEndLine);
                    Assert.IsTrue(3 == markerSpan.iEndIndex);
                }
            }
        }

        [TestMethod]
        public void ContextMenuNoUIShell()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view and add it to the local registry.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated and this will add the
                    // command handlers to the console window.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    CommandTargetHelper helper = new CommandTargetHelper(windowPane as IOleCommandTarget);

                    // Verify that the "ShowContextMenu" command handler does not crashes in
                    // case of missing SVsUIShell service.
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU);
                }
            }
        }

        [TestMethod]
        public void ContextMenu()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create a mock text buffer for the console.
                BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();

                // Add the text buffer to the local registry
                LocalRegistryMock mockLocalRegistry = new LocalRegistryMock();
                mockLocalRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view and add it to the local registry.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                mockLocalRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry to the list of services.
                provider.AddService(typeof(SLocalRegistry), mockLocalRegistry, false);

                // Create a mock UIShell.
                BaseMock uiShellMock = MockFactories.UIShellFactory.GetInstance();
                provider.AddService(typeof(SVsUIShell), uiShellMock, false);

                // Create the console.
                using (ToolWindowPane windowPane = CommandWindowHelper.CreateConsoleWindow(provider) as ToolWindowPane)
                {
                    // Call the CreatePaneWindow method that will force the creation of the text view.
                    IntPtr newHwnd;
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(
                        ((IVsWindowPane)windowPane).CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out newHwnd)));

                    // Now we have to set the frame property on the ToolWindowFrame because
                    // this will cause the execution of OnToolWindowCreated and this will add the
                    // command handlers to the console window.
                    windowPane.Frame = (IVsWindowFrame)MockFactories.WindowFrameFactory.GetInstance();

                    CommandTargetHelper helper = new CommandTargetHelper(windowPane as IOleCommandTarget);

                    // Verify that the "ShowContextMenu" command handler calls the
                    // ShowContextMenu method of IVsUIShell.
                    uiShellMock.ResetFunctionCalls(string.Format("{0}.{1}", typeof(IVsUIShell).FullName, "ShowContextMenu"));
                    helper.ExecCommand(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                       (uint)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU);
                    Assert.IsTrue(1 == uiShellMock.FunctionCalls(string.Format("{0}.{1}", typeof(IVsUIShell).FullName, "ShowContextMenu")));
                }
            }
        }

    }
}
