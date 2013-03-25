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
using System.Diagnostics.CodeAnalysis;
using EnvDTE;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    [System.Runtime.InteropServices.ComVisible(true)]
    [SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable")]
    public sealed class PythonProjectCodeModel : CodeModel {
        private Project projectItem;
        internal PythonProjectCodeModel(Project project) {
            this.projectItem = project;
        }

        #region CodeModel interface
        public CodeAttribute AddAttribute(string Name, object Location, string Value, object Position) {
            throw new NotImplementedException();
        }
        public CodeClass AddClass(string Name, object Location, object Position, object Bases, object ImplementedInterfaces, vsCMAccess Access) {
            throw new NotImplementedException();
        }
        public CodeDelegate AddDelegate(string Name, object Location, object Type, object Position, vsCMAccess Access) {
            throw new NotImplementedException();
        }
        public CodeEnum AddEnum(string Name, object Location, object Position, object Bases, vsCMAccess Access) {
            throw new NotImplementedException();
        }
        public CodeFunction AddFunction(string Name, object Location, vsCMFunction Kind, object Type, object Position, vsCMAccess Access) {
            throw new NotImplementedException();
        }
        public CodeInterface AddInterface(string Name, object Location, object Position, object Bases, vsCMAccess Access) {
            throw new NotImplementedException();
        }
        public CodeNamespace AddNamespace(string Name, object Location, object Position) {
            throw new NotImplementedException();
        }
        public CodeStruct AddStruct(string Name, object Location, object Position, object Bases, object ImplementedInterfaces, vsCMAccess Access) {
            throw new NotImplementedException();
        }
        public CodeVariable AddVariable(string Name, object Location, object Type, object Position, vsCMAccess Access) {
            throw new NotImplementedException();
        }
        public CodeType CodeTypeFromFullName(string Name) {
            throw new NotImplementedException();
        }
        public CodeTypeRef CreateCodeTypeRef(object Type) {
            throw new NotImplementedException();
        }
        public bool IsValidID(string Name) {
            throw new NotImplementedException();
        }
        public void Remove(object Element) {
            throw new NotImplementedException();
        }
        public CodeElements CodeElements {
            get { throw new NotImplementedException(); }
        }
        public DTE DTE {
            get {
                return projectItem.DTE;
            }
        }
        public bool IsCaseSensitive {
            get { return true; }
        }
        public string Language {
            get { return Microsoft.Samples.VisualStudio.IronPythonLanguageService.PythonConstants.pythonCodeDomProviderName; }
        }
        public Project Parent {
            get { return projectItem; }
        }
        #endregion
    }
}
