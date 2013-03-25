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

using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService {
    /// <summary>
    /// Class used to identify a module. The module is identify using the hierarchy that
    /// contains it and its item id inside the hierarchy.
    /// </summary>
    internal sealed class ModuleId {
        private IVsHierarchy ownerHierarchy;
        private uint itemId;
        public ModuleId(IVsHierarchy owner, uint id) {
            this.ownerHierarchy = owner;
            this.itemId = id;
        }
        public IVsHierarchy Hierarchy {
            get { return ownerHierarchy; }
        }
        public uint ItemID {
            get { return itemId; }
        }
        public override int GetHashCode() {
            int hash = 0;
            if (null != ownerHierarchy) {
                hash = ownerHierarchy.GetHashCode();
            }
            hash = hash ^ (int)itemId;
            return hash;
        }
        public override bool Equals(object obj) {
            ModuleId other = obj as ModuleId;
            if (null == obj) {
                return false;
            }
            if (!ownerHierarchy.Equals(other.ownerHierarchy)) {
                return false;
            }
            return (itemId == other.itemId);
        }
    }
}
