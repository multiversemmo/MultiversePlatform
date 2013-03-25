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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using IronPython.CodeDom;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VSConstants = Microsoft.VisualStudio.VSConstants;

using Microsoft.Samples.VisualStudio.CodeDomCodeModel;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService {
    /// <summary>
    /// This class is used to store the information about one specific instance
    /// of EnvDTE.FileCodeModel.
    /// </summary>
    internal class FileCodeModelInfo {
        private EnvDTE.FileCodeModel codeModel;
        private uint itemId;
        internal FileCodeModelInfo(EnvDTE.FileCodeModel codeModel, uint itemId) {
            this.codeModel = codeModel;
            this.itemId = itemId;
        }

        internal EnvDTE.FileCodeModel FileCodeModel {
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            get { return codeModel; }
        }

        internal uint ItemId {
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            get { return itemId; }
        }
    }

    /// <summary>
    /// This class implements an intellisense provider for Web projects.
    /// An intellisense provider is a specialization of a language service where it is able to
    /// run as a contained language. The main language service and main editor is usually the
    /// HTML editor / language service, but a specific contained language is hosted for fragments
    /// like the "script" tags.
    /// This object must be COM visible because it will be Co-Created by the host language.
    /// </summary>
    [ComVisible(true)]
    [Guid(PythonConstants.intellisenseProviderGuidString)]
    public sealed class PythonIntellisenseProvider : IVsIntellisenseProject, IDisposable {

        private class SourceFileInfo {
            private string fileName;
            private uint itemId;
            private IVsIntellisenseProjectHost hostProject;
            private EnvDTE.FileCodeModel fileCode;
            private CodeDomProvider codeProvider;

            public SourceFileInfo(string name, uint id) {
                this.fileName = name;
                this.itemId = id;
            }

            public IVsIntellisenseProjectHost HostProject {
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                get { return hostProject; }
                set {
                    if (this.hostProject != value) {
                        fileCode = null;
                    }
                    this.hostProject = value;
                }
            }

            public CodeDomProvider CodeProvider {
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                get { return codeProvider; }
                set {
                    if (value != codeProvider) {
                        fileCode = null;
                    }
                    codeProvider = value;
                }
            }

            public EnvDTE.FileCodeModel FileCodeModel {
                get {
                    // Don't build the object more than once.
                    if (null != fileCode) {
                        return fileCode;
                    }
                    // Verify that the host project is set.
                    if (null == hostProject) {
                        throw new System.InvalidOperationException();
                    }

                    // Get the hierarchy from the host project.
                    object propValue;
                    ErrorHandler.ThrowOnFailure(hostProject.GetHostProperty((uint)HOSTPROPID.HOSTPROPID_HIERARCHY, out propValue));
                    IVsHierarchy hierarchy = propValue as IVsHierarchy;
                    if (null == hierarchy) {
                        return null;
                    }

                    // Try to get the extensibility object for the item.
                    // NOTE: here we assume that the __VSHPROPID.VSHPROPID_ExtObject property returns a VSLangProj.VSProjectItem
                    // or a EnvDTE.ProjectItem object. No other kind of extensibility is supported.
                    propValue = null;
                    ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out propValue));
                    EnvDTE.ProjectItem projectItem = null;
                    VSLangProj.VSProjectItem vsprojItem = propValue as VSLangProj.VSProjectItem;
                    if (null == vsprojItem) {
                        projectItem = propValue as EnvDTE.ProjectItem;
                    } else {
                        projectItem = vsprojItem.ProjectItem;
                    }
                    if (null == projectItem) {
                        return null;
                    }

                    fileCode = new CodeDomFileCodeModel(projectItem.DTE, projectItem, codeProvider, fileName);

                    return fileCode;
                }
            }

            public uint ItemId {
                get { return this.itemId; }
            }

            public string Name {
                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
                get { return fileName; }
            }
        }

        private class FileCodeModelEnumerator : IEnumerable<FileCodeModelInfo> {
            private List<SourceFileInfo> filesInfo;
            private IVsIntellisenseProjectHost host;
            private CodeDomProvider codeProvider;
            public FileCodeModelEnumerator(IEnumerable<SourceFileInfo> files, IVsIntellisenseProjectHost hostProject, CodeDomProvider provider) {
                filesInfo = new List<SourceFileInfo>(files);
                host = hostProject;
                codeProvider = provider;
            }

            public IEnumerator<FileCodeModelInfo> GetEnumerator() {
                foreach (SourceFileInfo info in filesInfo) {
                    info.HostProject = host;
                    info.CodeProvider = codeProvider;
                    FileCodeModelInfo codeInfo = new FileCodeModelInfo(info.FileCodeModel, info.ItemId);
                    yield return codeInfo;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return (System.Collections.IEnumerator)this.GetEnumerator();
            }
        }

        private IVsIntellisenseProjectHost hostProject;
        private PythonContainedLanguageFactory languageFactory;
        private List<string> references;
        private Dictionary<uint, SourceFileInfo> files;
        private PythonProvider pythonProvider;

        public PythonIntellisenseProvider() {
            System.Diagnostics.Debug.WriteLine("\n\tPythonIntellisenseProvider created\n");
            references = new List<string>();
            files = new Dictionary<uint, SourceFileInfo>();
        }

        public int AddAssemblyReference(string bstrAbsPath) {
            string path = bstrAbsPath.ToUpper(CultureInfo.CurrentCulture);
            if (!references.Contains(path)) {
                references.Add(path);
                if (null != pythonProvider) {
                    pythonProvider.AddReference(path);
                }
            }
            return VSConstants.S_OK;
        }

        public void Dispose() {
            Dispose(true);
        }

        private void Dispose(bool disposing) {
            if (disposing) {
                Close();
            }
            GC.SuppressFinalize(this);
        }

        public int AddFile(string bstrAbsPath, uint itemid) {
            if (VSConstants.S_OK != IsCompilableFile(bstrAbsPath)) {
                return VSConstants.S_OK;
            }
            if (!files.ContainsKey(itemid)) {
                files.Add(itemid, new SourceFileInfo(bstrAbsPath, itemid));
            }
            return VSConstants.S_OK;
        }

        public int AddP2PReference(object pUnk) {
            // Not supported.
            return VSConstants.E_NOTIMPL;
        }

        public int Close() {
            this.hostProject = null;
            if (null != pythonProvider) {
                pythonProvider.Dispose();
                pythonProvider = null;
            }
            return VSConstants.S_OK;
        }

        public int GetCodeDomProviderName(out string pbstrProvider) {
            pbstrProvider = PythonConstants.pythonCodeDomProviderName;
            return VSConstants.S_OK;
        }

        public int GetCompilerReference(out object ppCompilerReference) {
            ppCompilerReference = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetContainedLanguageFactory(out IVsContainedLanguageFactory ppContainedLanguageFactory) {
            if (null == languageFactory) {
                languageFactory = new PythonContainedLanguageFactory(this);
            }
            ppContainedLanguageFactory = languageFactory;
            return VSConstants.S_OK;
        }

        public int GetExternalErrorReporter(out IVsReportExternalErrors ppErrorReporter) {
            // TODO: Handle the error reporter
            ppErrorReporter = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetFileCodeModel(object pProj, object pProjectItem, out object ppCodeModel) {
            ppCodeModel = null;
            return VSConstants.E_NOTIMPL;
            //EnvDTE.ProjectItem projectItem = pProjectItem as EnvDTE.ProjectItem;
            //if (null == projectItem) {
            //    throw new System.ArgumentException();
            //}
            //EnvDTE.Property prop = projectItem.Properties.Item("FullPath");
            //if (null == prop) {
            //    throw new System.InvalidOperationException();
            //}
            //string itemPath = prop.Value as string;
            //if (string.IsNullOrEmpty(itemPath)) {
            //    throw new System.InvalidOperationException();
            //}
            //SourceFileInfo fileInfo = null;
            //foreach (SourceFileInfo sourceInfo in files.Values) {
            //    if (0 == string.Compare(sourceInfo.Name, itemPath, StringComparison.OrdinalIgnoreCase)) {
            //        fileInfo = sourceInfo;
            //        break;
            //    }
            //}
            //if (null == fileInfo) {
            //    throw new System.InvalidOperationException();
            //}
            //fileInfo.HostProject = hostProject;
            //fileInfo.CodeProvider = CodeDomProvider;
            //ppCodeModel = fileInfo.FileCodeModel;
            //return VSConstants.S_OK;
        }

        public int GetProjectCodeModel(object pProj, out object ppCodeModel) {
            ppCodeModel = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Init(IVsIntellisenseProjectHost pHost) {
            this.hostProject = pHost;
            return VSConstants.S_OK;
        }

        public int IsCompilableFile(string bstrFileName) {
            string ext = System.IO.Path.GetExtension(bstrFileName);
            if (0 == string.Compare(ext, PythonConstants.pythonFileExtension, StringComparison.OrdinalIgnoreCase)) {
                return VSConstants.S_OK;
            }
            return VSConstants.S_FALSE;
        }

        public int IsSupportedP2PReference(object pUnk) {
            // P2P references are not supported.
            return VSConstants.S_FALSE;
        }

        public int IsWebFileRequiredByProject(out int pbReq) {
            pbReq = 0;
            return VSConstants.S_OK;
        }

        public int RefreshCompilerOptions() {
            return VSConstants.S_OK;
        }

        public int RemoveAssemblyReference(string bstrAbsPath) {
            string path = bstrAbsPath.ToUpper(CultureInfo.CurrentCulture);
            if (references.Contains(path)) {
                references.Remove(path);
                pythonProvider = null;
            }
            return VSConstants.S_OK;
        }

        public int RemoveFile(string bstrAbsPath, uint itemid) {
            if (files.ContainsKey(itemid)) {
                files.Remove(itemid);
            }
            return VSConstants.S_OK;
        }

        public int RemoveP2PReference(object pUnk) {
            return VSConstants.S_OK;
        }

        public int RenameFile(string bstrAbsPath, string bstrNewAbsPath, uint itemid) {
            return VSConstants.S_OK;
        }

        public int ResumePostedNotifications() {
            // No-Op
            return VSConstants.S_OK;
        }

        public int StartIntellisenseEngine() {
            // No-Op
            return VSConstants.S_OK;
        }

        public int StopIntellisenseEngine() {
            // No-Op
            return VSConstants.S_OK;
        }

        public int SuspendPostedNotifications() {
            // No-Op
            return VSConstants.S_OK;
        }

        public int WaitForIntellisenseReady() {
            // No-Op
            return VSConstants.S_OK;
        }

        internal PythonProvider CodeDomProvider {
            get {
                if (null == pythonProvider) {
                    pythonProvider = new PythonProvider();
                    foreach (string assembly in references) {
                        pythonProvider.AddReference(assembly);
                    }
                }
                return pythonProvider;
            }
        }

        internal IEnumerable<FileCodeModelInfo> FileCodeModels {
            get {
                List<SourceFileInfo> allFiles = new List<SourceFileInfo>(files.Values);
                return new FileCodeModelEnumerator(allFiles, hostProject, CodeDomProvider); 
            }
        }
    }
}
