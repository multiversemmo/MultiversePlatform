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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VsSDK.UnitTestLibrary;

namespace PythonProject.UnitTest
{
    /// <summary>
    /// Help code to create ILocalRegistry3 mock
    /// </summary>
    internal static class MockILocalRegistry3
    {
        internal static string LocalRegistryRoot = string.Empty;

        internal static BaseMock GetInstance()
        {
            //Create a base mock
            GenericMockFactory factory = new GenericMockFactory("ILocalRegistry3", new Type[] { typeof(ILocalRegistry3) });
            BaseMock mockObj = factory.GetInstance();

            //Add method call back for GetLocalRegistryRoot
            string methodName = string.Format("{0}.{1}", typeof(ILocalRegistry3).FullName, "GetLocalRegistryRoot");
            mockObj.AddMethodCallback(methodName, new EventHandler<CallbackArgs>(GetLocalRegistryRootCallBack));

            return mockObj;
        }

        #region Callbacks
        private static void GetLocalRegistryRootCallBack(object caller, CallbackArgs arguments)
        {
            arguments.SetParameter(0, MockILocalRegistry3.LocalRegistryRoot);
            arguments.ReturnValue = VSConstants.S_OK;
        }
        #endregion
    }
}
