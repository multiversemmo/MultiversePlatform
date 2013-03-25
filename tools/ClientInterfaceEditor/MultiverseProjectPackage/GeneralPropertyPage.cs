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
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;

namespace Microsoft.MultiverseInterfaceStudio
{
    /// <summary>
    /// General property settings page for World of Warcraft projects.
    /// </summary>
    [ComVisible(true)]
    [Guid(GuidStrings.GeneralPropertyPage)]
    public class GeneralPropertyPage : SettingsPage, ICustomTypeDescriptor
    {
        private string wowPath;
        private string interfaceVersion;
        private string addonTitle;
        private string addonNotes;
        private string dependencies;
        private string savedVariables;
        private string savedVariablesPerCharacter;
        private string author;
        private string addonVersion;
        private string authorEmail;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralPropertyPage"/> class.
        /// </summary>
        public GeneralPropertyPage()
        {
            // Get the localized general caption for the page's name
            this.Name = Resources.GetString(Resources.GeneralCaption);
        }

        #region Project Properties
        /// <summary>
        /// Gets or sets the path to the local World of Warcraft installation.
        /// </summary>
        [LocalizedCategory(Resources.MultiverseInterface)]
        [LocalizedDisplayName("WowPath_Name")]
        [LocalizedDescription("WowPath_Description")]
        public string WowPath
        {
            get { return this.wowPath; }
            set { this.wowPath = value; this.IsDirty = true; }
        }

        [LocalizedCategory("Category_Details")]
        [LocalizedDisplayName("InterfaceVersion_Name")]
        [LocalizedDescription("InterfaceVersion_Description")]
        public string InterfaceVersion
        {
            get { return this.interfaceVersion; }
            set { this.interfaceVersion = value; this.IsDirty = true; }
        }

        [LocalizedCategory("Category_AddonInfo")]
        [LocalizedDisplayName("AddonTitle_Name")]
        [LocalizedDescription("AddonTitle_Description")]
        public string AddonTitle
        {
            get { return this.addonTitle; }
            set { this.addonTitle = value; this.IsDirty = true; }
        }

        [LocalizedCategory("Category_AddonInfo")]
        [LocalizedDisplayName("AddonNotes_Name")]
        [LocalizedDescription("AddonNotes_Description")]
        public string AddonNotes
        {
            get { return this.addonNotes; }
            set { this.addonNotes = value; this.IsDirty = true; }
        }

        [LocalizedCategory("Category_Details")]
        [LocalizedDisplayName("Dependencies_Name")]
        [LocalizedDescription("Dependencies_Description")]
        public string Dependencies
        {
            get { return this.dependencies; }
            set { this.dependencies = value; this.IsDirty = true; }
        }

        [LocalizedCategory("Category_Details")]
        [LocalizedDisplayName("SavedVariables_Name")]
        [LocalizedDescription("SavedVariables_Description")]
        public string SavedVariables
        {
            get { return this.savedVariables; }
            set { this.savedVariables = value; this.IsDirty = true; }
        }

        [LocalizedCategory("Category_Details")]
        [LocalizedDisplayName("SavedVariablesPerCharacter_Name")]
        [LocalizedDescription("SavedVariablesPerCharacter_Description")]
        public string SavedVariablesPerCharacter
        {
            get { return this.savedVariablesPerCharacter; }
            set { this.savedVariablesPerCharacter = value; this.IsDirty = true; }
        }

        [LocalizedCategory("Category_AddonInfo")]
        [LocalizedDisplayName("Author_Name")]
        [LocalizedDescription("Author_Description")]
        public string Author
        {
            get { return this.author; }
            set { this.author = value; this.IsDirty = true; }
        }

        [LocalizedCategory("Category_AddonInfo")]
        [LocalizedDisplayName("AddonVersion_Name")]
        [LocalizedDescription("AddonVersion_Description")]
        public string AddonVersion
        {
            get { return this.addonVersion; }
            set { this.addonVersion = value; this.IsDirty = true; }
        }

        [LocalizedCategory("Category_AddonInfo")]
        [LocalizedDisplayName("AuthorEmail_Name")]
        [LocalizedDescription("AuthorEmail_Description")]
        public string AuthorEmail
        {
            get { return this.authorEmail; }
            set { this.authorEmail = value; this.IsDirty = true; }
        }

        #endregion

        /// <summary>
        /// Returns the name of the class.
        /// </summary>
        public override string GetClassName()
        {
            return this.GetType().FullName;
        }

        /// <summary>
        /// Binds the properties.
        /// </summary>
        protected override void BindProperties()
        {
            if (this.ProjectMgr == null)
                return;

            this.wowPath = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.MultiversePath.ToString(), false);

            this.interfaceVersion = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.InterfaceVersion.ToString(), false);
            this.addonTitle = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.AddonTitle.ToString(), false);
            this.addonNotes = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.AddonNotes.ToString(), false);
            this.addonVersion = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.AddonVersion.ToString(), false);
            this.author = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.Author.ToString(), false);
            this.authorEmail = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.AuthorEmail.ToString(), false);

            this.dependencies = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.Dependencies.ToString(), false);

            this.savedVariables = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.SavedVariables.ToString(), false);
            this.savedVariablesPerCharacter = this.ProjectMgr.GetProjectProperty(GeneralPropertyPageTag.SavedVariablesPerCharacter.ToString(), false);
        }

        /// <summary>
        /// Apply the changes made to the project.
        /// </summary>
        protected override int ApplyChanges()
        {
            if (this.ProjectMgr == null)
                return VSConstants.E_INVALIDARG;

            this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.MultiversePath.ToString(), this.wowPath);
            this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.InterfaceVersion.ToString(), this.interfaceVersion);
            this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.AddonTitle.ToString(), this.addonTitle);
            this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.AddonNotes.ToString(), this.addonNotes);
            this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.Dependencies.ToString(), this.dependencies);
            this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.SavedVariables.ToString(), this.savedVariables);
            this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.SavedVariablesPerCharacter.ToString(), this.savedVariablesPerCharacter);
            this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.Author.ToString(), this.author);
            this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.AddonVersion.ToString(), this.addonVersion);
            this.ProjectMgr.SetProjectProperty(GeneralPropertyPageTag.AuthorEmail.ToString(), this.authorEmail);

            this.IsDirty = false;

            return VSConstants.S_OK;
        }
    }
}
