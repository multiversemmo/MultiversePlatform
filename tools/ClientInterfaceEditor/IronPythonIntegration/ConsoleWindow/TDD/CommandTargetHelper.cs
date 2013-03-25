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
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.OLE.Interop;

using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    internal class CommandTargetHelper
    {
        private IOleCommandTarget target;
        public CommandTargetHelper(IOleCommandTarget commandTarget)
        {
            if (null == commandTarget)
            {
                throw new ArgumentNullException("commandTarget");
            }
            this.target = commandTarget;
        }

        public bool IsCommandSupported(Guid commandGuid, uint commandId, out uint flags)
        {
            flags = 0;
            OLECMD[] cmd = new OLECMD[1];
            cmd[0].cmdID = commandId;
            OLECMDTEXT commandText = new OLECMDTEXT();
            int hr;
            GCHandle handle = GCHandle.Alloc(commandText, GCHandleType.Pinned);
            try
            {
                hr = target.QueryStatus(ref commandGuid, 1, cmd, handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
            if ((int)OleConstants.OLECMDERR_E_NOTSUPPORTED == hr)
            {
                return false;
            }
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            flags = cmd[0].cmdf;
            return (0 != (flags & (uint)OLECMDF.OLECMDF_SUPPORTED));
        }

        public void ExecCommand(Guid commandGuid, uint commandId)
        {
            uint flags = (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT;
            int hr = target.Exec(ref commandGuid, commandId, flags, IntPtr.Zero, IntPtr.Zero);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
        }
    }
}
