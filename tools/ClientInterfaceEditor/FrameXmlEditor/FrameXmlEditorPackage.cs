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

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;

using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Controls;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the registration utility (regpkg.exe) that this class needs
    // to be registered as package.
    [PackageRegistration(UseManagedResourcesOnly = true)]

    // A Visual Studio component can be registered under different registry roots; for instance
    // when you debug your package you want to register it in the experimental hive. This
    // attribute specifies the registry root to use if no one is provided to regpkg.exe with
    // the /root switch.
    [DefaultRegistryRoot(@"Software\Microsoft\AppEnv\9.0\Apps\MultiverseInterfaceStudio")]

    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration(false, "#100", "#101", "1.0", IconResourceID = 400)]

    // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
    // package needs to have a valid load key (it can be requested at 
    // http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
    // package has a load key embedded in its resources.
    [ProvideLoadKey("Standard", "1.0", "Multiverse Interface Studio", "MultiverseInterfaceStudio", 102)]

    [ProvideEditorExtension(typeof(FrameXmlEditorFactory), ".xml", 50, DefaultName = "Frame XML Editor")]
    [ProvideEditorLogicalView(typeof(FrameXmlEditorFactory), "{7651a702-06e5-11d1-8ebd-00a0c90f26ea}")]  // LOGVIEWID_Designer
    [ProvideEditorLogicalView(typeof(FrameXmlEditorFactory), "{7651a701-06e5-11d1-8ebd-00a0c90f26ea}")]  // LOGVIEWID_Code
    [Guid(GuidStrings.FrameXmlEditorPackage)]
    public sealed class FrameXmlEditorPackage : Package
    {

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public FrameXmlEditorPackage()
        {
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
			base.RegisterEditorFactory(new FrameXmlEditorFactory(this));
			this.RegisterSelectionEvents(SelectionEventsMonitor.Instance);
		}

		private void RegisterSelectionEvents(IVsSelectionEvents selectionEvents)
		{
			IVsMonitorSelection monitorSelection = (IVsMonitorSelection)this.GetService(typeof(SVsShellMonitorSelection));
			uint cookie;
			int result = monitorSelection.AdviseSelectionEvents(selectionEvents, out cookie);
		}
    }
}
