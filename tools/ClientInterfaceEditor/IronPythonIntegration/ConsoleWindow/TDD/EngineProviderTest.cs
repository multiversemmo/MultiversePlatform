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
using System.Reflection;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

// Unit test infrastructure.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VsSDK.UnitTestLibrary;

// The namespace of the classes to test.
using Microsoft.Samples.VisualStudio.IronPythonInterfaces;
using Microsoft.Samples.VisualStudio.IronPythonConsole;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    [TestClass()]
    public class EngineProviderTest
    {
        private static ConstructorInfo engineProviderCtr;
        private static IPythonEngineProvider CreateEngineProvider(IOleServiceProvider provider)
        {
            if (null == engineProviderCtr)
            {
                Assembly asm = typeof(PythonConsolePackage).Assembly;
                Type engineProviderType = asm.GetType("Microsoft.Samples.VisualStudio.IronPythonConsole.IronPythonEngineProvider", true);
                engineProviderCtr = engineProviderType.GetConstructor(new Type[] { typeof(IOleServiceProvider) });
            }
            object obj = engineProviderCtr.Invoke(new object[] { provider });
            Assert.IsNotNull(obj);
            return (IPythonEngineProvider)obj;
        }

        [TestMethod()]
        public void SharedEngineTest()
        {
            IPythonEngineProvider engineProvider = CreateEngineProvider(null);
            // Verify that the shared engine is created.
            IEngine engineObj = engineProvider.GetSharedEngine();
            Assert.IsNotNull(engineObj);

            // Verify that the object returned by GetSharedEngine is always the same.
            IEngine newEngine = engineProvider.GetSharedEngine();
            Assert.IsTrue(newEngine.Equals(engineObj));
        }

        [TestMethod]
        public void SharedEngineVariables()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                // Create an engine provider with an empty service provider.
                IPythonEngineProvider engineProvider = CreateEngineProvider(provider);
                // Get the shared engine.
                IEngine engine = engineProvider.GetSharedEngine();
                // The "engine" variable should always be defined.
                object engObj = engine.Evaluate("engine");
                Assert.IsNotNull(engObj);
                // The "dte" variable should be set to null.
                object dte = engine.Evaluate("dte");
                Assert.IsNull(dte);

                // Now add the EnvDTE.DTE service to the service provider.
                BaseMock dteMock = MockFactories.DTEFactory.GetInstance();
                provider.AddService(typeof(EnvDTE.DTE), dteMock, false);

                // Create a new engine provider.
                engineProvider = CreateEngineProvider(provider);
                // Get the shared engine.
                engine = engineProvider.GetSharedEngine();
                // The "engine" variable should always be defined.
                engObj = engine.Evaluate("engine");
                Assert.IsNotNull(engObj);
                // Now the "dte" variable should be set.
                dte = engine.Evaluate("dte");
                Assert.IsNotNull(dte);
            }
        }

        [TestMethod()]
        public void CreateNewEngineTest()
        {
            IPythonEngineProvider engineProvider = CreateEngineProvider(null);
            // Verify that the engine is created.
            object firstEngine = engineProvider.CreateNewEngine();
            Assert.IsNotNull(firstEngine);
            // Verify that every call to CreateNewEngine returns a different object.
            object secondEngine = engineProvider.CreateNewEngine();
            Assert.IsNotNull(secondEngine);
            Assert.IsFalse(firstEngine.Equals(secondEngine));
        }
    }
}
