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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Package;

using Microsoft.Samples.VisualStudio.IronPythonInterfaces;
using Microsoft.Samples.VisualStudio.IronPythonLanguageService;
using IronPython.Hosting;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole
{
    /// <summary>
    /// This is the class that implements the tool window that will host the console.
    /// The console is implemented as a text view control (provided by the shell) hosted
    /// inside this tool window; the text inside the control is handled by a text buffer
    /// object (also provided by the shell).
    /// </summary>
    [Guid("5f50e2df-8fd8-4a07-ac79-829ee3dc7c7c")]
    internal class ConsoleWindow : ToolWindowPane, IVsWindowPane, IConsoleText
    {
        // The text buffer that stores the text inside the console window
        private IVsTextLines textLines;
        // The Stream object built on top of the text buffer.
        private TextBufferStream textStream;
        // The text view used to visualize the text inside the console
        private IVsTextView textView;
        // The service provider used to access the global services
        private System.IServiceProvider globalProvider;

        // Source object that handle the iteration between the language service and the
        // text view. Note that we should not dispose this object because it is owned by
        // the language service.
        private PythonSource source;

        // This command service is used to hide the one created in the base class because
        // there is no way to add a parent command target to it, so we will have to create
        // a new one and return it in our version of GetService.
        private OleMenuCommandService commandService;

        // List of lines that the user has typed on the console and not returned yet by
        // the ReadLine method
        private CommandBuffer inputBuffer;
        // Buffer of commands for the history.
        private HistoryBuffer history;

        // This is the guid used to set the key binding schema in the text view.
        private static readonly Guid CMDUIGUID_TextEditor = new Guid("{8B382828-6202-11d1-8870-0000F87579D2}");

        // Constants to handle the scrollbars.
        private const int horizontalScrollbar = 0;
        private const int verticalScrollbar = 1;

        /// <summary>
        /// Standard constructor for the console window.
        /// This constructor will use as global service provider the one exposed by the package class
        /// and will use it to create and initialize the text buffer.
        /// </summary>
        public ConsoleWindow() : 
            this(new ServiceProvider((IOleServiceProvider)PythonConsolePackage.GetGlobalService(typeof(IOleServiceProvider))))
        {}

        /// <summary>
        /// Creates a new ConsoleWindow object.
        /// This constructor uses the service provider passed as an argument to create and initialize
        /// the text buffer.
        /// </summary>
        public ConsoleWindow(IServiceProvider provider) :
            base(null)
        {
            if (null == provider)
                throw new ArgumentNullException("provider");
            globalProvider = provider;

            // Create the text buffer.
            textLines = (IVsTextLines)CreateObject(typeof(VsTextBufferClass), typeof(IVsTextLines));
            // Get a reference to the global service provider.
            IOleServiceProvider nativeProvider = (IOleServiceProvider)globalProvider.GetService(typeof(IOleServiceProvider));
            // The text buffer must be sited with the global service provider.
            ((IObjectWithSite)textLines).SetSite(nativeProvider);
            // Set the buffer as read-only. The user should be able to change the content of the
            // buffer only if the engine is started.
            uint flags;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textLines.GetStateFlags(out flags));
            flags |= (uint)Microsoft.VisualStudio.TextManager.Interop.BUFFERSTATEFLAGS.BSF_USER_READONLY;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textLines.SetStateFlags(flags));

            // Set the GUID of the language service that will handle this text buffer
            Guid languageGuid = typeof(PythonLanguage).GUID;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textLines.SetLanguageServiceID(ref languageGuid));

            // Initialize the history
            history = new HistoryBuffer();

            // Create the stream on top of the text buffer.
            textStream = new TextBufferStream(textLines);

            // Initialize the engine.
            InitializeEngine();

            // Set the title of the window.
            this.Caption = Resources.ToolWindowTitle;

            // Set the icon of the toolwindow.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 0;
        }

        /// <summary>
        /// Performs the clean-up operations for this object.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // Dispose the stream.
                    if (null != textStream)
                    {
                        ((IDisposable)textStream).Dispose();
                        textStream = null;
                    }

                    // Close the text view.
                    if (null != textView)
                    {
                        // Remove the command filter.
                        textView.RemoveCommandFilter((IOleCommandTarget)this);
                        // Release the text view.
                        textView.CloseView();
                        textView = null;
                    }

                    // Dispose the command service.
                    if (null != commandService)
                    {
                        commandService.Dispose();
                        commandService = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Thread function used to run the IronPython engine in a different thread.
        /// This thread will run until the IConsole implementation of this window will return
        /// null from the ReadLine method.
        /// </summary>
        private void InitializeEngine()
        {
            // Get the engine provider service to set this console window as the console
            // object associated with the shared engine.
            IPythonEngineProvider engineProvider = (IPythonEngineProvider)globalProvider.GetService(typeof(IPythonEngineProvider));
            if (null != engineProvider)
            {
                IEngine engine = engineProvider.GetSharedEngine();
                engine.StdErr = textStream;
                engine.StdOut = textStream;
                string version = string.Format(System.Globalization.CultureInfo.CurrentCulture,
                                               Resources.EngineVersionFormat,
                                               engine.Version.Major,
                                               engine.Version.Minor,
                                               engine.Version.Build);
                // Remove the read-only flag from the text buffer.
                uint flags;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.GetStateFlags(out flags));
                flags &= ~(uint)Microsoft.VisualStudio.TextManager.Interop.BUFFERSTATEFLAGS.BSF_USER_READONLY;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.SetStateFlags(flags));

                // Write engine version end copyright on the console.
                using (StreamWriter writer = new StreamWriter(textStream as Stream))
                {
                    writer.WriteLine(version);
                    writer.WriteLine(engine.Copyright);
                }

                // Create the buffer that will handle the commands to the engine.
                inputBuffer = new CommandBuffer(engine);
            }
        }

        private IVsTextView TextView
        {
            get
            {
                // Avoid to create the object more than once.
                if (null != textView)
                    return textView;

                // Create the text view object.
                textView = (IVsTextView)CreateObject(typeof(VsTextViewClass), typeof(IVsTextView));
                // Now we have to site the text view using the global service provider.
                // Note that this service provider will be used only before the text view will be
                // used inside CreateToolWindow, because inside this call it will be sited again
                // using a different provider.
                IOleServiceProvider nativeProvider = (IOleServiceProvider)globalProvider.GetService(typeof(IOleServiceProvider));
                ((IObjectWithSite)textView).SetSite(nativeProvider);

                // Now it is possible to initalize the view using the buffer.
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textView.Initialize(textLines, IntPtr.Zero, (uint)TextViewInitFlags.VIF_VSCROLL | (uint)TextViewInitFlags.VIF_HSCROLL, null));

                // Get the language service.
                PythonLanguage language = globalProvider.GetService(typeof(PythonLanguage)) as PythonLanguage;
                if (null != language)
                {
                    // In order to enable intellisense we have to create a Source object and create
                    // a CodeWindowManager on top of it; this way the window manager will install a
                    // special filter to the text view that will intercept some key stroke, call the
                    // language service to get the list of the methods and show the completion window.
                    // In general you don't need to do all this work because for code windows it is
                    // done by the framework, but in this case this window is not a code window, so
                    // we have to do it manually.

                    // Create a new PythonSource object.
                    source = language.CreateSource(textLines) as PythonSource;
                    // Set the ScopeCreator property so that we can use our AuthoringScope
                    // based on the PythonEngine instead of the default one provided by the
                    // language service that is based on the statical analysis of a file.
                    source.ScopeCreator = new ScopeCreatorCallback(ConsoleAuthoringScope.CreateScope);
                    // Set the site for the scope.
                    ConsoleAuthoringScope.Site = globalProvider;
                    // Make sure that the ConsoleAuthoringScope has a reference to the language.
                    ConsoleAuthoringScope.Language = language;
                    ConsoleAuthoringScope.PythonConsole = this;

                    // Now we can create a CodeWindowManager using the source.
                    CodeWindowManager windowManager = language.CreateCodeWindowManager(null, source);
                    // Add the window manager to the language service.
                    language.AddCodeWindowManager(windowManager);
                    // Add the text view to the window manager.
                    windowManager.OnNewView(textView);
                    // Add the source to the list of the special sources.
                    language.AddSpecialSource(source, textView);
                }

                // Set the cursor at the end of the text.
                int line;
                int column;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.GetLastLineIndex(out line, out column));

                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textView.SetCaretPos(line, column));

                return textView;
            }
        }

        /// <summary>
        /// Set the cursor at the end of the current buffer and, if needed, scrolls the text
        /// view so that the cursor is visible.
        /// </summary>
        private void SetCursorAtEndOfBuffer()
        {
            // If the text view is not created, then there is no reason to set the cursor.
            if (null == textView)
            {
                return;
            }
            int lastLine;
            int lastIndex;
            // Get the size of the buffer.
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textLines.GetLastLineIndex(out lastLine, out lastIndex));
            // Set the cursor at the end.
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textView.SetCaretPos(lastLine, lastIndex));

            // Make sure that the last line of the buffer is visible.
            // Note that we will not throw an exception if we can not set the scroll informations.
            int minUnit;
            int maxUnit;
            int visibleUnits;
            int firstVisibleUnit;
            if (Microsoft.VisualStudio.ErrorHandler.Succeeded(
                    textView.GetScrollInfo(verticalScrollbar,
                                           out minUnit, out maxUnit,
                                           out visibleUnits, out firstVisibleUnit)))
            {
                if (maxUnit >= visibleUnits)
                {
                    textView.SetScrollPosition(verticalScrollbar, maxUnit + 1 - visibleUnits);
                }
            }

            // Make sure that the text view is showing the beginning of the new line.
            if (Microsoft.VisualStudio.ErrorHandler.Succeeded(
                    textView.GetScrollInfo(horizontalScrollbar,
                                           out minUnit, out maxUnit,
                                           out visibleUnits, out firstVisibleUnit)))
            {
                textView.SetScrollPosition(horizontalScrollbar, minUnit);
            }
        }

        /// <summary>
        /// Utility function to create an instance of a class from the local registry.
        /// </summary>
        /// <param name="classType">The type of the class to create.</param>
        /// <param name="interfaceType">An interface implemented by the class.</param>
        private object CreateObject(Type classType, Type interfaceType)
        {
            // Get the local registry service that will allow us to create the object
            // using the registration inside the current registry root.
            ILocalRegistry localRegistry = (ILocalRegistry)globalProvider.GetService(typeof(SLocalRegistry));

            // Create the object.
            Guid interfaceGuid = interfaceType.GUID;
            IntPtr interfacePointer;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                localRegistry.CreateInstance(classType.GUID, null, ref interfaceGuid,
                                             (uint)CLSCTX.CLSCTX_INPROC_SERVER, out interfacePointer));
            if (interfacePointer == IntPtr.Zero)
                throw new COMException(Resources.CanNotCreateObject);

            // Get a CLR object from the COM pointer
            object obj = null;
            try
            {
                obj = Marshal.GetObjectForIUnknown(interfacePointer);
            }
            finally
            {
                Marshal.Release(interfacePointer);
            }

            return obj;
        }

        /// <summary>
        /// Return the service of the given type.
        /// This override is needed to be able to use a different command service from the one
        /// implemented in the base class.
        /// </summary>
        protected override object GetService(Type serviceType)
        {
            if ((typeof(IOleCommandTarget) == serviceType) || 
                (typeof(System.ComponentModel.Design.IMenuCommandService) == serviceType))
            {
                if (null != commandService)
                {
                    return commandService;
                }
            }
            return base.GetService(serviceType);
        }

        /// <summary>
        /// The implementation of this abstract method is empty (returns null) because
        /// this class handles the implementation of IVsWindowPane without the help of
        /// the base classes.
        /// </summary>
        public override System.Windows.Forms.IWin32Window Window
        {
            get 
            { 
                return null; 
            }
        }

        /// <summary>
        /// Function called when the window frame is set on this tool window.
        /// </summary>
        public override void OnToolWindowCreated()
        {
            // Call the base class's implementation.
            base.OnToolWindowCreated();

            // Register this object as command filter for the text view so that it will
            // be possible to intercept some command.
            IOleCommandTarget originalFilter;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textView.AddCommandFilter((IOleCommandTarget)this, out originalFilter));
            // Create a command service that will use the previous command target
            // as parent target and will route to it the commands that it can not handle.
            if (null == originalFilter)
            {
                commandService = new OleMenuCommandService(this);
            }
            else
            {
                commandService = new OleMenuCommandService(this, originalFilter);
            }

            // Add the command handler for RETURN.
            CommandID id = new CommandID(
                                typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.RETURN);
            OleMenuCommand cmd = new OleMenuCommand(new EventHandler(OnReturn), id);
            cmd.BeforeQueryStatus += new EventHandler(UnsupportedOnCompletion);
            commandService.AddCommand(cmd);

            // Command handler for UP and DOWN arrows. These commands are needed to implement
            // the history in the console, but at the moment the implementation is empty.
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UP);
            cmd = new OleMenuCommand(new EventHandler(OnHistory), id);
            cmd.BeforeQueryStatus += new EventHandler(SupportCommandOnInputPosition);
            commandService.AddCommand(cmd);
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.DOWN);
            cmd = new OleMenuCommand(new EventHandler(OnHistory), id);
            cmd.BeforeQueryStatus += new EventHandler(SupportCommandOnInputPosition);
            commandService.AddCommand(cmd);

            // Command handler for the LEFT arrow. This command handler is needed in order to
            // avoid that the user uses the left arrow to move to the previous line or over the
            // command prompt.
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.LEFT);
            cmd = new OleMenuCommand(new EventHandler(OnNoAction), id);
            cmd.BeforeQueryStatus += new EventHandler(OnBeforeMoveLeft);
            commandService.AddCommand(cmd);

            // Handle also the HOME command.
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL);
            cmd = new OleMenuCommand(new EventHandler(OnHome), id);
            commandService.AddCommand(cmd);

            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL_EXT);
            cmd = new OleMenuCommand(new EventHandler(OnShiftHome), id);
            cmd.BeforeQueryStatus += new EventHandler(SupportCommandOnInputPosition);
            commandService.AddCommand(cmd);

            // Adding support for "Clear Pane" command.
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd97CmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd97CmdID.ClearPane);
            cmd = new OleMenuCommand(new EventHandler(OnClearPane), id);
            commandService.AddCommand(cmd);

            // Add a command handler for the context menu.
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU);
            cmd = new OleMenuCommand(new EventHandler(ShowContextMenu), id);
            commandService.AddCommand(cmd);

            // Now we set the key binding for this frame to the same value as the text editor
            // so that there will be the same mapping for the commands.
            Guid commandUiGuid = CMDUIGUID_TextEditor;
            ((IVsWindowFrame)Frame).SetGuidProperty((int)__VSFPROPID.VSFPROPID_InheritKeyBindings, ref commandUiGuid);
        }

        /// <summary>
        /// Return true if the user is currently on the input line.
        /// Here we assume that the input line is always the last one.
        /// </summary>
        private bool IsCurrentLineInputLine()
        {
            // Get the total number of lines in the buffer.
            int totalLines;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textLines.GetLineCount(out totalLines));
            // Get the current position of the cursor.
            int currentLine;
            int currentColumn;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textView.GetCaretPos(out currentLine, out currentColumn));
            // Verify whether the current line (that is 0-based) is the last one.
            return (currentLine == totalLines - 1);
        }

        /// <summary>
        /// Returns true if the current position is inside the writable section of the buffer.
        /// </summary>
        private bool IsCurrentPositionInputPosition()
        {
            if ((null == textView) || (null == textStream.ReadOnlyMarker))
            {
                return false;
            }
            TextSpan[] span = new TextSpan[1];
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textStream.ReadOnlyMarker.GetCurrentSpan(span));
            int line;
            int column;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textView.GetCaretPos(out line, out column));
            if (line > span[0].iEndLine)
            {
                return true;
            }
            if ((line == span[0].iEndLine) && (column >= span[0].iEndIndex))
            {
                return true;
            }
            return false;
        }

        public string TextOfLine(int line, int endColumn, bool skipReadOnly)
        {
            string lineText = null;
            lock (textLines)
            {
                int startColumn = 0;
                if (skipReadOnly && (null != textStream.ReadOnlyMarker))
                {
                    TextSpan[] span = new TextSpan[1];
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                        textStream.ReadOnlyMarker.GetCurrentSpan(span));
                    if (line < span[0].iEndLine)
                    {
                        return null;
                    }
                    else if (line == span[0].iEndLine)
                    {
                        startColumn = span[0].iEndIndex;
                    }
                }
                if (startColumn > endColumn)
                {
                    return null;
                }
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.GetLineText(line, startColumn, line, endColumn, out lineText));
            }
            return lineText;
        }

        #region Command Handlers
        /// <summary>
        /// Set the Supported property on the sender command to true if and only if the
        /// current position of the cursor is an input position.
        /// </summary>
        private void SupportCommandOnInputPosition(object sender, EventArgs args)
        {
            // Check if the sender is a MenuCommand.
            MenuCommand command = sender as MenuCommand;
            if (null == command)
            {
                // This should never happen, but let's handle it just in case.
                return;
            }
            // If the completion window is open, then we should not handle commands.
            if ((null != source) && (source.IsCompletorActive))
            {
                command.Supported = false;
            }
            else
            {
                command.Supported = IsCurrentPositionInputPosition();
            }
        }
        /// <summary>
        /// Command handler for the history commands.
        /// The standard implementation of a console has a history function implemented when
        /// the user presses the UP or DOWN key.
        /// </summary>
        private void OnHistory(object sender, EventArgs e)
        {
            // Get the command to figure out from the ID if we have to get the previous or the
            // next element in the history.
            OleMenuCommand command = sender as OleMenuCommand;
            if (null == command ||
                command.CommandID.Guid != typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID)
            {
                return;
            }
            string historyEntry = null;
            if (command.CommandID.ID == (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UP)
            {
                historyEntry = history.PreviousEntry();
            }
            else if (command.CommandID.ID == (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.DOWN)
            {
                historyEntry = history.NextEntry();
            }
            if (string.IsNullOrEmpty(historyEntry))
            {
                return;
            }

            // There is something to write on the console, so replace the current text in the
            // input line with the content of the history.

            lock (textLines)
            {
                // The input line starts by definition at the end of the text marker
                // used to mark the read only region.
                TextSpan[] span = new TextSpan[1];
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textStream.ReadOnlyMarker.GetCurrentSpan(span));

                // Get the last position in the buffer.
                int lastLine;
                int lastIndex;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.GetLastLineIndex(out lastLine, out lastIndex));

                // Now replace all this text with the text returned by the history.
                System.Runtime.InteropServices.GCHandle textHandle = GCHandle.Alloc(historyEntry, GCHandleType.Pinned);
                try
                {
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                        textLines.ReplaceLines(span[0].iEndLine, span[0].iEndIndex, lastLine, lastIndex, textHandle.AddrOfPinnedObject(), historyEntry.Length, null));
                }
                finally
                {
                    // Free the memory inside the finally block to avoid memory leaks.
                    textHandle.Free();
                }
            }
        }

        /// <summary>
        /// Set the status of the command to Unsupported when the completion window is visible.
        /// </summary>
        private void UnsupportedOnCompletion(object sender, EventArgs args)
        {
            MenuCommand command = sender as MenuCommand;
            if (null == command)
            {
                return;
            }
            command.Supported = (null == source) || (!source.IsCompletorActive);
        }

        /// <summary>
        /// Handles the HOME command in two different ways if the current line is the input
        /// line or not.
        /// </summary>
        private void OnHome(object sender, EventArgs e)
        {
            // Get the current line.
            int currentLine;
            int currentColumn;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textView.GetCaretPos(out currentLine, out currentColumn));
            if (IsCurrentLineInputLine() && (null != textStream.ReadOnlyMarker))
            {
                // If we are on the input line, then the 'home' is right after the read only region.
                // Get the span for the read-only region.
                TextSpan[] span = new TextSpan[1];
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textStream.ReadOnlyMarker.GetCurrentSpan(span));
                // Set the cursor right after the text span.
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textView.SetCaretPos(currentLine, span[0].iEndIndex));
            }
            else
            {
                // Otherwise the behaviour is the standard one.
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textView.SetCaretPos(currentLine, 0));
            }
        }

        /// <summary>
        /// Overwrite the default 'Shift' + 'HOME' to limit the selection to the input section
        /// of the buffer.
        /// </summary>
        private void OnShiftHome(object sender, EventArgs args)
        {
            // Get the end of the read-only section.
            TextSpan[] span = new TextSpan[1];
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textStream.ReadOnlyMarker.GetCurrentSpan(span));
            // Get the current position of the cursor.
            int line;
            int endColumn;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textView.GetCaretPos(out line, out endColumn));
            int startColumn = 0;
            if ((line == span[0].iEndLine) && (endColumn >= span[0].iEndIndex))
            {
                startColumn = span[0].iEndIndex;
            }
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textView.SetSelection(line, endColumn, line, startColumn));
        }

        /// <summary>
        /// Determines whether it is possible to move left on the current line.
        /// It is used to avoid a situation where the user moves over the console's prompt.
        /// </summary>
        private void OnBeforeMoveLeft(object sender, EventArgs e)
        {
            // Verify that the sender is of the expected type.
            OleMenuCommand command = sender as OleMenuCommand;
            if (null == command)
            {
                return;
            }
            // As default we don't want to handle this command because it should be handled
            // by the dafault implementation of the text view.
            command.Supported = false;

            // Get the current position of the cursor.
            int line;
            int column;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textView.GetCaretPos(out line, out column));
            // We want to override the standard command only if the cursor is on the last line
            // and right before (or over) the prompt.
            if (IsCurrentLineInputLine())
            {
                if (null != textStream.ReadOnlyMarker)
                {
                    TextSpan[] span = new TextSpan[1];
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                        textStream.ReadOnlyMarker.GetCurrentSpan(span));
                    if (column <= span[0].iEndIndex)
                    {
                        // If the cursor is right before the prompt, then we handle the command to
                        // do nothing, so the user can not move over the prompt or on a previous line.
                        command.Supported = true;
                    }
                }
            }
        }
        /// <summary>
        /// Empty command handler used to overwrite some standard command with an empty action.
        /// </summary>
        private void OnNoAction(object sender, EventArgs e)
        {
            // Do Nothing.
        }
        /// <summary>
        /// Command handler for the RETURN command.
        /// It is called when the user presses the ENTER key inside the console window and
        /// is used to execute the text as an IronPython expression.
        /// </summary>
        private void OnReturn(object sender, EventArgs e)
        {
            lock (textLines)
            {
                // If the user is not on the input line, then this should be a no-action.
                if (!IsCurrentLineInputLine())
                {
                    return;
                }

                ExecuteUserInput();

                SetCursorAtEndOfBuffer();
            }
        }

        private void ExecuteUserInput()
        {
            // Get the position inside the text view.
            int line;
            int column;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                TextView.GetCaretPos(out line, out column));

            // Get the length of the current line.
            int lineLength;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textLines.GetLengthOfLine(line, out lineLength));

            // Get the end of the read-only region.
            TextSpan[] span = new TextSpan[1];
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textStream.ReadOnlyMarker.GetCurrentSpan(span));

            // Check if there is something in this line.
            string text = "";
            if (lineLength > span[0].iEndIndex)
            {
                // Get the text of the line.
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.GetLineText(line, span[0].iEndIndex, line, lineLength, out text));
                // Add the text to the history.
                history.AddEntry(text);
            }

            // Add the text to the buffer. Note that the text is always added to this buffer,
            // but it is added to the history only if it is not empty.
            if (null != inputBuffer)
            {
                inputBuffer.Add(text);
            }
        }

        /// <summary>
        /// Function called when the user select the "Clear Pane" menu item from the context menu.
        /// This will clear the content of the console window leaving only the console cursor and
        /// resizing the read-only region.
        /// </summary>
        private void OnClearPane(object sender, EventArgs args)
        {
            lock (textLines)
            {
                // Clear the content of the read-only region.
                TextSpan[] span = new TextSpan[1];
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textStream.ReadOnlyMarker.GetCurrentSpan(span));
                if (span[0].iEndLine > 0)
                {
                    // Reset the line marker so that only the last line is in it.
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                        textStream.ReadOnlyMarker.ResetSpan(span[0].iEndLine, 0, span[0].iEndLine, span[0].iEndIndex));
                    // Remove all the text before the last line.
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                        textLines.ReplaceLines(0, 0, span[0].iEndLine, 0, IntPtr.Zero, 0, null));
                }

                // Clear the text outside the read-only region.
                int lastLine;
                int lastColumn;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                    textLines.GetLastLineIndex(out lastLine, out lastColumn));
                if ((lastLine > 0) || (lastColumn >= span[0].iEndIndex))
                {
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                        textLines.ReplaceLines(0, span[0].iEndIndex, lastLine, lastColumn, IntPtr.Zero, 0, null));
                }
            }
        }

        private void ShowContextMenu(object sender, EventArgs args)
        {
            // Get a reference to the UIShell.
            IVsUIShell uiShell = globalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (null == uiShell)
            {
                return;
            }

            // Get the position of the cursor.
            System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
            POINTS[] pnts = new POINTS[1];
            pnts[0].x = (short)pt.X;
            pnts[0].y = (short)pt.Y;

            // Show the menu.
            Guid menuGuid = GuidList.guidIronPythonConsoleCmdSet;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                uiShell.ShowContextMenu(0, ref menuGuid, (int)PkgCmdIDList.IPConsoleContextMenu, pnts, textView as IOleCommandTarget));
        }
        #endregion

        #region IVsWindowPane Members

        int IVsWindowPane.ClosePane()
        {
            int hr = Microsoft.VisualStudio.VSConstants.S_OK;
            if (null != textView)
            {
                // Call the implementation provided by the text view.
                hr = ((IVsWindowPane)textView).ClosePane();
            }
            Dispose(true);
            return hr;
        }

        int IVsWindowPane.CreatePaneWindow(IntPtr hwndParent, int x, int y, int cx, int cy, out IntPtr hwnd)
        {
            return ((IVsWindowPane)TextView).CreatePaneWindow(hwndParent, x, y, cx, cy, out hwnd);
        }

        int IVsWindowPane.GetDefaultSize(SIZE[] pSize)
        {
            return ((IVsWindowPane)TextView).GetDefaultSize(pSize);
        }

        int IVsWindowPane.LoadViewState(IStream pStream)
        {
            return ((IVsWindowPane)TextView).LoadViewState(pStream);
        }

        int IVsWindowPane.SaveViewState(IStream pStream)
        {
            return ((IVsWindowPane)TextView).SaveViewState(pStream);
        }

        int IVsWindowPane.SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            return ((IVsWindowPane)TextView).SetSite(psp);
        }

        int IVsWindowPane.TranslateAccelerator(MSG[] lpmsg)
        {
            return ((IVsWindowPane)TextView).TranslateAccelerator(lpmsg);
        }

        #endregion
    }
}
