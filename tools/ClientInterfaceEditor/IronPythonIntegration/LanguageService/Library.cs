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

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService {

    public class Library : IVsSimpleLibrary2 {
        private Guid guid;
        private _LIB_FLAGS2 capabilities;
        private LibraryNode root;

        public Library(Guid libraryGuid) {
            this.guid = libraryGuid;
            root = new LibraryNode("", LibraryNode.LibraryNodeType.Package);
        }

        public _LIB_FLAGS2 LibraryCapabilities {
            get { return capabilities; }
            set { capabilities = value; }
        }

        internal void AddNode(LibraryNode node) {
            lock (this) {
                root = new LibraryNode(root);
                root.AddNode(node);
            }
        }

        internal void RemoveNode(LibraryNode node) {
            lock (this) {
                root = new LibraryNode(root);
                root.RemoveNode(node);
            }
        }

        #region IVsSimpleLibrary2 Members

        public int AddBrowseContainer(VSCOMPONENTSELECTORDATA[] pcdComponent, ref uint pgrfOptions, out string pbstrComponentAdded) {
            pbstrComponentAdded = null;
            return VSConstants.E_NOTIMPL;
        }

        public int CreateNavInfo(SYMBOL_DESCRIPTION_NODE[] rgSymbolNodes, uint ulcNodes, out IVsNavInfo ppNavInfo) {
            ppNavInfo = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetBrowseContainersForHierarchy(IVsHierarchy pHierarchy, uint celt, VSBROWSECONTAINER[] rgBrowseContainers, uint[] pcActual) {
            return VSConstants.E_NOTIMPL;
        }

        public int GetGuid(out Guid pguidLib) {
            pguidLib = guid;
            return VSConstants.S_OK;
        }

        public int GetLibFlags2(out uint pgrfFlags) {
            pgrfFlags = (uint)LibraryCapabilities;
            return VSConstants.S_OK;
        }

        public int GetList2(uint ListType, uint flags, VSOBSEARCHCRITERIA2[] pobSrch, out IVsSimpleObjectList2 ppIVsSimpleObjectList2) {
            ppIVsSimpleObjectList2 = root as IVsSimpleObjectList2;
            return VSConstants.S_OK;
        }

        public int GetSeparatorStringWithOwnership(out string pbstrSeparator) {
            pbstrSeparator = ".";
            return VSConstants.S_OK;
        }

        public int GetSupportedCategoryFields2(int Category, out uint pgrfCatField) {
            pgrfCatField = (uint)_LIB_CATEGORY2.LC_HIERARCHYTYPE | (uint)_LIB_CATEGORY2.LC_PHYSICALCONTAINERTYPE;
            return VSConstants.S_OK;
        }

        public int LoadState(IStream pIStream, LIB_PERSISTTYPE lptType) {
            return VSConstants.S_OK;
        }

        public int RemoveBrowseContainer(uint dwReserved, string pszLibName) {
            return VSConstants.E_NOTIMPL;
        }

        public int SaveState(IStream pIStream, LIB_PERSISTTYPE lptType) {
            return VSConstants.S_OK;
        }

        public int UpdateCounter(out uint pCurUpdate) {
            return ((IVsSimpleObjectList2)root).UpdateCounter(out pCurUpdate);
        }

        #endregion
    }
}
