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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Drawing.Design;
using System.Windows.Forms;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

using Microsoft.MultiverseInterfaceStudio.Services;

using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.MultiverseInterfaceStudio
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

    // A Visual Studio component can be registered under different regitry roots; for instance
    // when you debug your package you want to register it in the experimental hive. This
    // attribute specifies the registry root to use if no one is provided to regpkg.exe with
    // the /root switch.
    [DefaultRegistryRoot(@"Software\Microsoft\AppEnv\9.0\Apps\MultiverseInterfaceStudio")]

    // Register the package and use only managed resources
    [PackageRegistration(UseManagedResourcesOnly = true)]

    // Provides a project factory so that addon projects can be created.
    [ProvideProjectFactory(typeof(MultiverseInterfaceProjectFactory), "Multiverse Interface", "Multiverse Network Interface Project Files (*.mulproj);*.mulproj", "mulproj", "mulproj", @".\NullPath",
                           LanguageVsTemplate = "MultiverseInterface")]

    // Provides project items
    [ProvideProjectItem(typeof(MultiverseInterfaceProjectFactory), "Mulitverse Interface Items", @".\NullPath", 500)]

    // Provides the property pages
    [ProvideObject(typeof(GeneralPropertyPage))]

    // Provides the option page
    [ProvideOptionPage(typeof(FrameXmlDesignerGeneralOptionPage), "FrameXMLDesigner", "Miscellaneous", 110, 111, true)]

    // Provides the Settings service
    [ProvideService(typeof(IInterfaceStudioSettings), ServiceName = "Multiverse Interface Studio Project Settings")]

    // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
    // package needs to have a valid load key (it can be requested at 
    // http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
    // package has a load key embedded in its resources.
    [ProvideLoadKey("Standard", "1.0", "Interface Studio for Multiverse Interface", "Multiverse Interface", 102)]

    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration(false, "#100", "#101", "1.0", IconResourceID = 400)]

    // Provides the MSBuild targets that are used by the projects
    [ProvideMSBuildTargets("MultiverseInterfaceStudio_1.0", @"%ProgramFiles%\MSBuild\Microsoft\MultiverseInterfaceStudio\1.0\MultiverseInterfaceStudio.targets")]

    // Provides toolbox items
    [ProvideToolboxItems(1)]

    // Provide menu items
    [ProvideMenuResource(1000, 1)]

    // The Guid of the WowProjectPackage
    [Guid(GuidStrings.MultiverseInterfaceProjectPackage)]
    public sealed class MultiverseInterfaceProjectPackage : ProjectPackage
    {
        private const string tutorialsRelativePath = @"Tutorials\AddOn Studio for Multiverse Tutorial.html";
        private const string MultiverseInterfaceStudioFilterName = "MultiverseInterfaceStudioFilter";
        private const string toolboxTabName = "Multiverse Interface";

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Remove SolutionListenerForProjectUpdates from collection of SolutionListeners as we do not support references
            SolutionListenerForProjectReferenceUpdate referenceUpdateListener = SolutionListeners.OfType<SolutionListenerForProjectReferenceUpdate>().SingleOrDefault();
            if (referenceUpdateListener != null)
            {
                referenceUpdateListener.Dispose();
                SolutionListeners.Remove(referenceUpdateListener);
            }

            // Find SolutionListenerForProjectEvents and subscribe to OnBeforeProjectClosed
            SolutionListenerForProjectEvents projectEventListener = SolutionListeners.OfType<SolutionListenerForProjectEvents>().SingleOrDefault();
            if (projectEventListener != null)
            {
                projectEventListener.BeforeProjectFileClosed += new EventHandler<BeforeProjectFileClosedEventArgs>(OnBeforeProjectFileClosed);
            }

            // Hook designer event services
            IDesignerEventService eventService = (IDesignerEventService)GetService(typeof(IDesignerEventService));
            eventService.DesignerCreated += new DesignerEventHandler(OnDesignerCreated);

            // Hook ToolboxInitialized event
            this.ToolboxInitialized += new EventHandler(OnToolboxInitialized);
            this.ToolboxUpgraded += new EventHandler(OnToolboxUpgraded);

            // Add menu handlers
            this.AddMenuHandlers();

            // Create settings service
            AddonStudioSettings settings = new AddonStudioSettings(this);

            // Add service
            this.AddService(typeof(IInterfaceStudioSettings), settings);

            // Register the WoW project factory
            base.RegisterProjectFactory(new MultiverseInterfaceProjectFactory(this));
        }

        public new object GetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }

        public T GetDialogPage<T>() where T:class
        {
            return base.GetDialogPage(typeof(T)) as T;
        }

        private void AddService(Type serviceType, object serviceInstance)
        {
            IServiceContainer serviceContainer = this as IServiceContainer;
            if (serviceContainer != null)
                serviceContainer.AddService(serviceType, serviceInstance, true);
        }

        private void AddMenuHandlers()
        {
            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (menuCommandService != null)
            {
                //// Add menu handlers for the menu items
                this.AddMenuHandler(menuCommandService, Guids.MultiverseInterfaceProjectCmdSet, Commands.TutorialCreateAddon, OnTutorialCreateAddOn);
                this.AddMenuHandler(menuCommandService, Guids.MultiverseInterfaceProjectCmdSet, Commands.TutorialDeployAddon, OnTutorialDeployAddOn);
                this.AddMenuHandler(menuCommandService, Guids.MultiverseInterfaceProjectCmdSet, Commands.TutorialTestAddon, OnTutorialTestAddOn);
                this.AddMenuHandler(menuCommandService, Guids.MultiverseInterfaceProjectCmdSet, Commands.TutorialCreateAce2Addon, OnTutorialCreateAce2AddOn);

                this.AddMenuHandler(menuCommandService, Guids.MultiverseInterfaceProjectCmdSet, Commands.HelpWowAceWiki, OnHelpWowAceWiki);
                this.AddMenuHandler(menuCommandService, Guids.MultiverseInterfaceProjectCmdSet, Commands.HelpWowUiWiki, OnHelpWowUiWiki);
            }
        }

        private void AddMenuHandler(OleMenuCommandService menuCommandService, Guid menuGroup, int command, EventHandler handler)
        {
            CommandID commandId = new CommandID(menuGroup, command);
            MenuCommand menuItem = new MenuCommand(handler, commandId);
            menuCommandService.AddCommand(menuItem);
        }

        private void OnTutorialCreateAddOn(object sender, EventArgs e)
        {
            this.NavigateTutorialAnchor("CreatingAddon");
        }

        private void OnTutorialDeployAddOn(object sender, EventArgs e)
        {
            this.NavigateTutorialAnchor("DeployingAddon");
        }

        private void OnTutorialTestAddOn(object sender, EventArgs e)
        {
            this.NavigateTutorialAnchor("TestingAddon");
        }

        private void OnTutorialCreateAce2AddOn(object sender, EventArgs e)
        {
            this.NavigateTutorialAnchor("CreatingAce2Addon");
        }

        private void OnHelpWowAceWiki(object sender, EventArgs e)
        {
            this.Navigate("http://www.wowace.com");
        }

        private void OnHelpWowUiWiki(object sender, EventArgs e)
        {
            this.Navigate("http://www.wowwiki.com");
        }

        private void Navigate(string url)
        {
            IVsWebBrowsingService webBrowsingService = this.GetService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService;

            if (webBrowsingService != null)
            {
                IVsWindowFrame frame;
                ErrorHandler.ThrowOnFailure(webBrowsingService.Navigate(url, 0, out frame));
            }
        }

        private void NavigateTutorialAnchor(string anchorName)
        {
            IVsShell shell = (IVsShell)GetService(typeof(SVsShell));

            if (shell != null)
            {
                object value;
                shell.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out value);

                string installDir = (string)value;
                string tutorialsPath = Path.Combine(installDir, tutorialsRelativePath);

                if (File.Exists(tutorialsPath))
                {
                    if (!String.IsNullOrEmpty(anchorName))
                        this.Navigate(tutorialsPath + "#" + anchorName);
                    else
                        this.Navigate(tutorialsPath);
                }
                else
                    MessageBox.Show("The tutorials are missing from your installation. Please reinstall AddOn Studio for Multiverse Interface.", "AddOn Studio for Multiverse Interface", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDesignerCreated(object sender, DesignerEventArgs e)
        {
            e.Designer.LoadComplete += new EventHandler(OnDesignerLoadComplete);
        }

        private void OnDesignerLoadComplete(object sender, EventArgs e)
        {
            IDesignerHost host = (IDesignerHost)sender;

            TypeDescriptor.AddAttributes(host.GetDesigner(host.RootComponent), new Attribute[] { new ToolboxItemFilterAttribute(MultiverseInterfaceStudioFilterName, ToolboxItemFilterType.Require) });
            IToolboxService toolboxService = (IToolboxService)GetService(typeof(IToolboxService));
            toolboxService.Refresh();
        }

        private void OnToolboxInitialized(object sender, EventArgs e)
        {
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(String.Format("{0}\\{1}", Application.ExecutablePath.Substring(0,Application.ExecutablePath.LastIndexOf("\\")), "PackagesToLoad\\Microsoft.MultiverseInterfaceStudio.FrameXmlEditor.dll"));
            IToolboxService toolboxService = (IToolboxService)GetService(typeof(IToolboxService));

            foreach (ToolboxItem item in ToolboxService.GetToolboxItems(assemblyName))
            {
                toolboxService.AddToolboxItem(item, toolboxTabName);
            }
        }

        private void OnToolboxUpgraded(object sender, EventArgs e)
        {
            IVsToolbox toolbox = this.GetService(typeof(SVsToolbox)) as IVsToolbox;
            if (toolbox == null)
                throw new InvalidOperationException();

            toolbox.RemoveTab(toolboxTabName);
            OnToolboxInitialized(sender, e);
        }

        private void OnBeforeProjectFileClosed(object sender, BeforeProjectFileClosedEventArgs e)
        {
            IPythonLanguageService languageService = (IPythonLanguageService)this.GetService(typeof(IPythonLanguageService));

            if (languageService != null)
                languageService.Clear();
        }
    }
}
