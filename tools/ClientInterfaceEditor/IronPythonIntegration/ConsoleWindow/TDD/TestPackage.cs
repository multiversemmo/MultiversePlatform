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
using System.Reflection;
using System.Runtime.InteropServices;

// Visual Studio platform.
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

using IServiceProvider = System.IServiceProvider;

// Unit test infrastructure.
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// The namespace of the classes to test.
using Microsoft.Samples.VisualStudio.IronPythonConsole;
using Microsoft.Samples.VisualStudio.IronPythonInterfaces;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    [TestClass()]
    public class ConsolePackageTest
    {
        /// <summary>
        /// Verify that the package exposes the services that it is supposed
        /// to expose. The verification is done for:
        /// 1. The IronPython engine provider.
        /// </summary>
        [TestMethod()]
        public void VerifyServices()
        {
            // Create the package.
            IVsPackage consolePackage = new PythonConsolePackage() as IVsPackage;
            Assert.IsNotNull(consolePackage);
            // Get the service provider implemented by the package.
            IServiceProvider packageProvider = consolePackage as IServiceProvider;
            Assert.IsNotNull(packageProvider);
            // Verify that the package exposes the services.
            Assert.IsNotNull(packageProvider.GetService(typeof(IPythonEngineProvider)));
        }

        [TestMethod()]
        public void PackageCreation()
        {
            // Create a service provider with basic services to site the package.
            using (OleServiceProvider provider = OleServiceProvider.CreateOleServiceProviderWithBasicServices())
            {
                IVsPackage package = null;
                try
                {
                    // Create the package and verify that implements IVsPackage.
                    package = new PythonConsolePackage() as IVsPackage;
                    Assert.IsNotNull(package);
                    // Verify that SetSite succeeded.
                    int hr = package.SetSite(provider);
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(hr));
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
        public void PackageCommands()
        {
            using (OleServiceProvider provider = OleServiceProvider.CreateOleServiceProviderWithBasicServices())
            {
                IVsPackage package = null;
                try
                {
                    // Create and site the package.
                    package = new PythonConsolePackage() as IVsPackage;
                    Assert.IsNotNull(package);
                    int hr = package.SetSite(provider);
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(hr));

                    // Get the command target from the package.
                    IOleCommandTarget target = package as IOleCommandTarget;
                    Assert.IsNotNull(target);

                    CommandTargetHelper helper = new CommandTargetHelper(target);
                    uint flags;
                    Assert.IsTrue(helper.IsCommandSupported(GuidList.guidIronPythonConsoleCmdSet, PkgCmdIDList.cmdidIronPythonConsole, out flags));
                    Assert.IsTrue(0 != ((uint)OLECMDF.OLECMDF_SUPPORTED & flags));
                    Assert.IsTrue(0 != ((uint)OLECMDF.OLECMDF_ENABLED & flags));
                    Assert.IsTrue(0 == ((uint)OLECMDF.OLECMDF_INVISIBLE & flags));
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

        private static void CreateToolwindowCallback(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            IVsWindowFrame frame = mock["Frame"] as IVsWindowFrame;
            Assert.IsInstanceOfType(args.GetParameter(2), CommandWindowHelper.ConsoleType);
            IVsWindowPane pane = args.GetParameter(2) as IVsWindowPane;
            IntPtr hwnd;
            pane.CreatePaneWindow(IntPtr.Zero, 0, 0, 0, 0, out hwnd);
            args.SetParameter(9, frame);
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        [TestMethod]
        public void ConsoleCreation()
        {
            using (OleServiceProvider provider = OleServiceProvider.CreateOleServiceProviderWithBasicServices())
            {
                // In order to create a console window we have to add the text buffer to the
                // local registry.

                // Create a mock object for the text buffer.
                BaseMock textLinesMock = MockFactories.TextBufferFactory.GetInstance();
                // Create a new local registry class.
                LocalRegistryMock mockRegistry = new LocalRegistryMock();
                // Add the text buffer to the list of the classes that local registry can create.
                mockRegistry.AddClass(typeof(VsTextBufferClass), textLinesMock);

                // Define the mock object for the text view.
                BaseMock textViewMock = MockFactories.TextViewFactory.GetInstance();
                mockRegistry.AddClass(typeof(VsTextViewClass), textViewMock);

                // Add the local registry mock to the service provider.
                provider.AddService(typeof(SLocalRegistry), mockRegistry, false);

                // Create a mock UIShell to be able to create the tool window.
                BaseMock uiShell = MockFactories.UIShellFactory.GetInstance();
                uiShell["Frame"] = MockFactories.WindowFrameFactory.GetInstance() as IVsWindowFrame;
                uiShell.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IVsUIShell), "CreateToolWindow"),
                    new EventHandler<CallbackArgs>(CreateToolwindowCallback));
                provider.AddService(typeof(SVsUIShell), uiShell, false);

                IVsPackage package = null;
                try
                {
                    // Create the package.
                    package = new PythonConsolePackage() as IVsPackage;
                    Assert.IsNotNull(package);

                    // Make sure that the static variable about the global service provider is null;
                    FieldInfo globalProvider = typeof(Microsoft.VisualStudio.Shell.Package).GetField("_globalProvider", BindingFlags.Static | BindingFlags.NonPublic);
                    globalProvider.SetValue(null, null);

                    // Site it.
                    int hr = package.SetSite(provider);
                    Assert.IsTrue(Microsoft.VisualStudio.ErrorHandler.Succeeded(hr));

                    // Get the command target from the package.
                    IOleCommandTarget target = package as IOleCommandTarget;
                    Assert.IsNotNull(target);

                    CommandTargetHelper helper = new CommandTargetHelper(target);
                    helper.ExecCommand(GuidList.guidIronPythonConsoleCmdSet, PkgCmdIDList.cmdidIronPythonConsole);
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
    }
}
