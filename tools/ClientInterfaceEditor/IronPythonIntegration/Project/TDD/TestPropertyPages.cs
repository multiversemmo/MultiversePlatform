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
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Samples.VisualStudio.IronPythonProject;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.Package;

namespace PythonProject.UnitTest
{
    /// <summary>
    /// Summary description for TestPropertyPages
    /// </summary>
    [TestClass]
    public class TestPropertyPages
    {
        public TestPropertyPages()
        {
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CreateInstance()
        {
            Assert.IsNotNull(new GeneralPropertyPage());            
        }

        [TestMethod]
        public void VerifyOutPutFileNameProperty()
        {
            GeneralPropertyPage page = new GeneralPropertyPage();

            // Set the assemblyname
            string defaultAssemblyName = "Test";
            FieldInfo assemblyName = typeof(GeneralPropertyPage).GetField("assemblyName", BindingFlags.NonPublic | BindingFlags.Instance);
            assemblyName.SetValue(page, defaultAssemblyName);
            Assert.AreEqual<string>(defaultAssemblyName, assemblyName.GetValue(page).ToString());

            // Test that Output file (DefaultAssemblyName and Library) provides expected result
            AssertOuputFile(page, defaultAssemblyName, OutputType.Library);

            // Test that Output file (DefaultAssemblyName and WinExe) provides expected result
            AssertOuputFile(page, defaultAssemblyName, OutputType.WinExe);

            // Test that Output file (DefaultAssemblyName and Exe) provides expected result
            AssertOuputFile(page, defaultAssemblyName, OutputType.Exe);
        }

        private static void AssertOuputFile(GeneralPropertyPage page, string expectedAssemblyName, OutputType outputType)
        {
            FieldInfo outputTypeInfo = typeof(GeneralPropertyPage).GetField("outputType", BindingFlags.NonPublic | BindingFlags.Instance);
            outputTypeInfo.SetValue(page, outputType);
            Assert.AreEqual<string>(outputType.ToString(), outputTypeInfo.GetValue(page).ToString());
            string expectedOutputFile = expectedAssemblyName + PythonProjectNode.GetOuputExtension(outputType);
            Assert.AreEqual<string>(expectedOutputFile, page.OutputFile);
        }

    }
}
