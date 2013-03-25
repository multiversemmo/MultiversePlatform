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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.Build.BuildEngine;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Package.Automation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

using Microsoft.MultiverseInterfaceStudio.Services;

using Utilities = Microsoft.VisualStudio.Package.Utilities;

namespace Microsoft.MultiverseInterfaceStudio
{
    /// <summary>
    /// Represents a project node in the Solution Explorer and controls its behavior.
    /// </summary>
    [Guid(GuidStrings.MultiverseInterfaceProjectNode)]
    public class MultiverseInterfaceProjectNode : ProjectNode
    {
        private const string MultiverseInterfacePathSubKey = @"SOFTWARE\MultiverseClient\Interface";
        private const string MultiverseInterfacePathName = "InstallPath";
        private const string MultiverseInterfacePathToken = "$multiversepath$";

        private static ImageList multiverseImageList;
        private static int multiverseImageOffset;

        private MultiverseInterfaceProjectPackage package;

        static MultiverseInterfaceProjectNode()
        {
            // Get the image list from the embedded resource used by the Multiverse Interface project
            multiverseImageList = Utilities.GetImageList(typeof(MultiverseInterfaceProjectNode).Assembly.GetManifestResourceStream("Resources.multiverseImageList.bmp"));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiverseInterfaceProjectNode"/> class.
        /// </summary>
        /// <param name="package">The package the project type resides in.</param>
        public MultiverseInterfaceProjectNode(MultiverseInterfaceProjectPackage package)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            this.package = package;

            // File nodes can have children (!) (think Frame XML has a Lua codebehind file)
            this.CanFileNodesHaveChilds = true;

            // Support the Project Designer Editor
            this.SupportsProjectDesigner = true;

            // Allow deleting items
            this.CanProjectDeleteItems = true;
            
            // Store the number of images before we add our own so we know the offset where we start
            multiverseImageOffset = base.ImageHandler.ImageList.Images.Count;

            // Add all images
            foreach (Image image in multiverseImageList.Images)
            {
                base.ImageHandler.AddImage(image);                
            }
        }

        /// <summary>
        /// Gets the image list that contains the WoW icons for the nodes.
        /// </summary>
        public static ImageList MultiverseImageList
        {
            get { return multiverseImageList; }
        }

        /// <summary>
        /// Gets the index of the image to show for the project node.
        /// </summary>
        public override int ImageIndex
        {
            get
            {
                // We need to add the offset as we appended our own images to an existing image list.
                return multiverseImageOffset + (int)MultiverseInterfaceImage.MultiverseInterfaceProject;
            }
        }
        /// <summary>
        /// Gets the Guid of the project factory.
        /// </summary>
        public override Guid ProjectGuid
        {
            get { return typeof(MultiverseInterfaceProjectFactory).GUID; }
        }

        /// <summary>
        /// Gets the type of the project.
        /// </summary>
        public override string ProjectType
        {
            get { return this.GetType().Name; }
        }

        /// <summary>
        /// Loads a project file. Called from the factory CreateProject to load the project.
        /// </summary>
        /// <param name="fileName">File name of the project that will be created. </param>
        /// <param name="location">Location where the project will be created.</param>
        /// <param name="name">If applicable, the name of the template to use when cloning a new project.</param>
        /// <param name="flags">Set of flag values taken from the VSCREATEPROJFLAGS enumeration.</param>
        /// <param name="iidProject">Identifier of the interface that the caller wants returned. </param>
        /// <param name="canceled">An out parameter specifying if the project creation was canceled</param>
        public override void Load(string fileName, string location, string name, uint flags, ref Guid iidProject, out int canceled)
        {
            // Let MPF first deal with the tasks at hand
            base.Load(fileName, location, name, flags, ref iidProject, out canceled);

            // If this project was just created, set WoW path based on the registry
            if ((flags & (uint)__VSCREATEPROJFLAGS.CPF_CLONEFILE) == (uint)__VSCREATEPROJFLAGS.CPF_CLONEFILE)
            {
                this.SetProjectProperty(GeneralPropertyPageTag.MultiversePath.ToString(), this.GetMultiversePath());
            }
        }

         /// <summary>
        /// Creates a file node for a project element.
        /// </summary>
        /// <param name="element">The project element.</param>
        /// <returns>An instance of the <see cref="FileNode"/> class.</returns>
        public override FileNode CreateFileNode(ProjectElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            MultiverseInterfaceFileNode node = null;

            // Get the SubType for the project element.
            string subType = element.GetMetadata(ProjectFileConstants.SubType);

            switch (subType)
            {
                case MultiverseInterfaceSubType.Code:
                    node = new MultiverseInterfacePythonFileNode(this, element);
                    break;
                case MultiverseInterfaceSubType.Frame:
                    node = new MultiverseInterfaceXmlFileNode(this, element);
                    break;
                case MultiverseInterfaceSubType.TableOfContents:
                    node = new MultiverseInterfaceTocFileNode(this, element);
                    break;
                default:
                    // We could not recognize the file subtype, just create a WowFileNode
                    node = new MultiverseInterfaceFileNode(this, element);
                    break;
            }

            // Check whether this file should be added to the language service
            if (subType == MultiverseInterfaceSubType.Frame || subType == MultiverseInterfaceSubType.Code)
            {
                IPythonLanguageService languageService = (IPythonLanguageService)this.GetService(typeof(IPythonLanguageService));

                // Make sure the language service is available
                if (languageService != null)
                {
                    switch (subType)
                    {
                        case MultiverseInterfaceSubType.Frame:
                            languageService.AddFrameXmlFile(node.GetMkDocument());
                            break;
                        case MultiverseInterfaceSubType.Code:
                            languageService.AddPythonFile(node.GetMkDocument());
                            break;
                    }
                }
            }

            return node;
        }

        /// <summary>
        /// Creates a file node for a project element.
        /// </summary>
        /// <param name="element">The project element.</param>
        /// <returns>An instance of the <see cref="DependentFileNode"/> class.</returns>
        public override DependentFileNode CreateDependentFileNode(ProjectElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            string subType = element.GetMetadata(ProjectFileConstants.SubType);

            if (subType == MultiverseInterfaceSubType.Code)
            {
                DependentFileNode node = new MultiverseInterfaceIronPythonDependentFileNode(this, element);

                IPythonLanguageService languageService = (IPythonLanguageService)this.GetService(typeof(IPythonLanguageService));

                // Make sure the language service is available
                if (languageService != null)
                    languageService.AddPythonFile(node.GetMkDocument());

                return node;
            }

            return base.CreateDependentFileNode(element);
        }

        /// <summary>
        /// Creates the reference container node.
        /// </summary>
        /// <returns></returns>
        protected override ReferenceContainerNode CreateReferenceContainerNode()
        {
            // Return null as we do not support references
            return null;
        }

        /// <summary>
        /// Removes a node from the hierarchy.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        public override void RemoveChild(HierarchyNode node)
        {
            if (node is MultiverseInterfaceXmlFileNode || node is MultiverseInterfacePythonFileNode)
            {
                IPythonLanguageService languageService = (IPythonLanguageService)this.GetService(typeof(IPythonLanguageService));

                if (languageService != null)
                {
                    if (node is MultiverseInterfaceXmlFileNode)
                        languageService.RemoveFrameXmlFile(node.GetMkDocument());

                    if (node is MultiverseInterfacePythonFileNode)
                        languageService.RemovePythonFile(node.GetMkDocument());
                }
            }
            
            base.RemoveChild(node);
        }

        /// <summary>
        /// Adds the new file node to hierarchy.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="fileName">Name of the file.</param>
        protected override void AddNewFileNodeToHierarchy(HierarchyNode parentNode, string path)
        {
            // If a lua file is being added, try to find a related FrameXML node
            if (MultiverseInterfaceProjectNode.IsPythonFile(path))
            {
                // Try to find a FrameXML node with a matching relational name
                string fileName = Path.GetFileNameWithoutExtension(path);
                HierarchyNode childNode = this.FirstChild;

                // Iterate through the children
                while (childNode != null)
                {
                    // If this child is an XML node and its relational name matches, break out of the loop
                    if (childNode is MultiverseInterfaceXmlFileNode && String.Compare(childNode.GetRelationalName(), fileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        parentNode = childNode;
                        break;
                    }

                    // Move over to the next sibling
                    childNode = childNode.NextSibling;
                }
            }

            HierarchyNode child;

            if (this.CanFileNodesHaveChilds && (parentNode is FileNode || parentNode is DependentFileNode))
            {
                child = this.CreateDependentFileNode(path);
                child.ItemNode.SetMetadata(ProjectFileConstants.DependentUpon, parentNode.ItemNode.GetMetadata(ProjectFileConstants.Include));

                // Make sure to set the HasNameRelation flag on the dependent node if it is related to the parent by name
                if (String.Compare(child.GetRelationalName(), parentNode.GetRelationalName(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    child.HasParentNodeNameRelation = true;
                }
            }
            else
            {
                //Create and add new filenode to the project
                child = this.CreateFileNode(path);
            }

            parentNode.AddChild(child);

            this.Tracker.OnItemAdded(path, VSADDFILEFLAGS.VSADDFILEFLAGS_NoFlags);
        }

        /// <summary>
        /// Adds the file from template.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public override void AddFileFromTemplate(string source, string target)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");
            if (!File.Exists(source))
                throw new FileNotFoundException(Resources.GetString(Resources.FailedToLoadTemplateFileMessage, source));

            // We assume that there is no token inside the file because the only
            // way to add a new element should be through the template wizard that
            // take care of expanding and replacing the tokens.
            // The only task to perform is to copy the source file in the
            // target location.
            string targetFolder = Path.GetDirectoryName(target);

            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            if (File.Exists(target))
                File.Delete(target);

            File.Copy(source, target);
        }

        /// <summary>
        /// Creates an MSBuild ProjectElement for a file.
        /// </summary>
        /// <param name="file">The path to the file.</param>
        /// <returns>An instance of the <see cref="ProjectElement"/> class.</returns>
        protected override ProjectElement AddFileToMsBuild(string file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            string itemPath = PackageUtilities.MakeRelativeIfRooted(file, this.BaseURI);

            if (Path.IsPathRooted(itemPath))
                throw new ArgumentException("Cannot add item with full path.", "file");

            ProjectElement newItem = this.CreateMsBuildFileItem(itemPath, ProjectFileConstants.Content);

            if (MultiverseInterfaceProjectNode.IsPythonFile(itemPath))
            {
                newItem.SetMetadata(ProjectFileConstants.SubType, MultiverseInterfaceSubType.Code);
            }
            else if (MultiverseInterfaceProjectNode.IsFrameXmlFile(itemPath))
            {
                newItem.SetMetadata(ProjectFileConstants.SubType, MultiverseInterfaceSubType.Frame);
            }
            else if (MultiverseInterfaceProjectNode.IsTableOfContentsFile(itemPath))
            {
                newItem.SetMetadata(ProjectFileConstants.SubType, MultiverseInterfaceSubType.TableOfContents);
            }

            return newItem;
        }

        /// <summary>
        /// Gets the configuration independent property pages.
        /// </summary>
        /// <returns></returns>
        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(GeneralPropertyPage).GUID;
            return result;
        }

        public override int GetFormatList(out string formatList)
        {
            formatList = Resources.GetString(Resources.FormatList, "\0");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called when a Add References.. is pressed.
        /// </summary>
        /// <returns>Returns a value of the <see cref="VSConstants"/> enumeration.</returns>
        public override int AddProjectReference()
        {
            return VSConstants.S_OK;
        }

        private static bool IsPythonFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return false;

            return (String.Compare(Path.GetExtension(fileName), ".py", StringComparison.OrdinalIgnoreCase) == 0);
        }

        private static bool IsFrameXmlFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return false;

            // TODO: Also look into the file, might be a normal XML file

            return (String.Compare(Path.GetExtension(fileName), ".xml", StringComparison.OrdinalIgnoreCase) == 0);
        }

        private static bool IsTableOfContentsFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return false;

            return (String.Compare(Path.GetExtension(fileName), ".toc", StringComparison.OrdinalIgnoreCase) == 0);
        }

        private string GetMultiversePath()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(MultiverseInterfacePathSubKey))
            {
                if (key != null)
                {
                    return (string)key.GetValue(MultiverseInterfacePathName, String.Empty);
                }
            }

            return String.Empty;
		}

		#region Folder node

		protected internal override FolderNode CreateFolderNode(string path, ProjectElement element)
		{
			FolderNode folderNode = new MultiverseInterfaceFolderNode(this, path, element);
			return folderNode;
		}

		#endregion
	}
}
