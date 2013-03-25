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
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Samples.VisualStudio.IronPythonProject.WPFProviders;

namespace Microsoft.Samples.VisualStudio.IronPythonProject
{
    [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\9.0Exp")]
    //Set the projectsTemplatesDirectory to a non-existant path to prevent VS from including the working directory as a valid template path
    [ProvideProjectFactory(typeof(PythonProjectFactory), "IronPython", "IronPython Project Files (*.pyproj);*.pyproj", "pyproj", "pyproj", ".\\NullPath", LanguageVsTemplate = "IronPython")]
    //Register the WPF Python Factory
    [ProvideProjectFactory(typeof(PythonWPFProjectFactory), null, null, null, null, null, LanguageVsTemplate = "IronPython", TemplateGroupIDsVsTemplate = "WPF", ShowOnlySpecifiedTemplatesVsTemplate = false)]
    [SingleFileGeneratorSupportRegistrationAttribute(typeof(PythonProjectFactory))]
    [WebSiteProject("IronPython", "Iron Python")]
    [ProvideObject(typeof(GeneralPropertyPage))]
    [ProvideObject(typeof(IronPythonBuildPropertyPage))]
    [ProvideMenuResource(1000, 1)]
    [ProvideEditorExtensionAttribute(typeof(EditorFactory), ".py", 32)]
    [ProvideEditorLogicalView(typeof(EditorFactory), "{7651a702-06e5-11d1-8ebd-00a0c90f26ea}")]  //LOGVIEWID_Designer
    [ProvideEditorLogicalView(typeof(EditorFactory), "{7651a701-06e5-11d1-8ebd-00a0c90f26ea}")]  //LOGVIEWID_Code
    [Microsoft.VisualStudio.Shell.PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideLoadKey("standard", "1.0", "Visual Studio Integration of IronPython Project System", "Microsoft Corporation", 1)]
    [Guid(GuidList.guidPythonProjectPkgString)]
    //The following attributes are specific to supporting Web Application Projects
    [WAProvideProjectFactory(typeof(WAPythonProjectFactory), "IronPython Web Application Project Templates", "IronPython", false, "Web", null)]
    [WAProvideProjectFactoryTemplateMapping("{" + GuidList.guidPythonProjectFactoryString + "}", typeof(WAPythonProjectFactory))]
    [WAProvideLanguageProperty(typeof(WAPythonProjectFactory), "CodeFileExtension", ".py")]
    [WAProvideLanguageProperty(typeof(WAPythonProjectFactory), "TemplateFolder", "IronPython")]
    //[WAProvideLanguageProperty(typeof(WAPythonProjectFactory), "RootIcon", "#8001")]
    [InstalledProductRegistration(true, null, null, null)]
    // IronPython does not need a CodeBehindCodeGenerator because all the code should be inline, so we disable
    // it setting a null GUID for the class that is supposed to implement it.
    [WAProvideLanguageProperty(typeof(WAPythonProjectFactory), "CodeBehindCodeGenerator", "{00000000-0000-0000-0000-000000000000}")]
    //The following value would be the guid of the Debug property page for IronPython (if it existed). The reason this guid is needed is so
    //WAP can hide it from the user.
    //[WAProvideLanguageProperty(typeof(WAPythonProjectFactory), "DebugPageGUID", "{00000000-1008-4FB2-A715-3A4E4F27E610}")]
    // Register the targets file used by the IronPython project system.
    [ProvideMSBuildTargets("IronPython_1.0", @"%ProgramFiles%\MSBuild\Microsoft\IronPython\1.0\IronPython.targets")]
    public class PythonProjectPackage : ProjectPackage, IVsInstalledProduct
    {
        protected override void Initialize()
        {
            base.Initialize();
            this.RegisterProjectFactory(new PythonProjectFactory(this));
            this.RegisterProjectFactory(new PythonWPFProjectFactory(this as IServiceProvider));
            this.RegisterEditorFactory(new EditorFactory(this));
        }

        #region IVsInstalledProduct Members
        /// <summary>
        /// This method is called during Devenv /Setup to get the bitmap to
        /// display on the splash screen for this package.
        /// </summary>
        /// <param name="pIdBmp">The resource id corresponding to the bitmap to display on the splash screen</param>
        /// <returns>HRESULT, indicating success or failure</returns>
        public int IdBmpSplash(out uint pIdBmp)
        {
            pIdBmp = 300;

            return VSConstants.S_OK;
        }

        /// <summary>
        /// This method is called to get the icon that will be displayed in the
        /// Help About dialog when this package is selected.
        /// </summary>
        /// <param name="pIdIco">The resource id corresponding to the icon to display on the Help About dialog</param>
        /// <returns>HRESULT, indicating success or failure</returns>
        public int IdIcoLogoForAboutbox(out uint pIdIco)
        {
            pIdIco = 400;

            return VSConstants.S_OK;
        }

        /// <summary>
        /// This methods provides the product official name, it will be
        /// displayed in the help about dialog.
        /// </summary>
        /// <param name="pbstrName">Out parameter to which to assign the product name</param>
        /// <returns>HRESULT, indicating success or failure</returns>
        public int OfficialName(out string pbstrName)
        {
            pbstrName = GetResourceString("@ProductName");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This methods provides the product description, it will be
        /// displayed in the help about dialog.
        /// </summary>
        /// <param name="pbstrProductDetails">Out parameter to which to assign the description of the package</param>
        /// <returns>HRESULT, indicating success or failure</returns>
        public int ProductDetails(out string pbstrProductDetails)
        {
            pbstrProductDetails = GetResourceString("@ProductDetails");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This methods provides the product version, it will be
        /// displayed in the help about dialog.
        /// </summary>
        /// <param name="pbstrPID">Out parameter to which to assign the version number</param>
        /// <returns>HRESULT, indicating success or failure</returns>
        public int ProductID(out string pbstrPID)
        {
            pbstrPID = GetResourceString("@ProductID");
            return VSConstants.S_OK;
        }

        #endregion

        /// <summary>
        /// This method loads a localized string based on the specified resource.
        /// </summary>
        /// <param name="resourceName">Resource to load</param>
        /// <returns>String loaded for the specified resource</returns>
        public string GetResourceString(string resourceName)
        {
            string resourceValue;
            IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
            }
            Guid packageGuid = this.GetType().GUID;
            int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            return resourceValue;
        }
    }
}
