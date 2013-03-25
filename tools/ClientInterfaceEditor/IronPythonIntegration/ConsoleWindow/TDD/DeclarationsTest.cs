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
using Microsoft.VisualStudio.Package;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Samples.VisualStudio.IronPythonConsole;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    [TestClass]
    public class ConsoleDeclarationsTest
    {
        private static ConstructorInfo declarationsConstructor;
        private static Declarations CreateDeclarations()
        {
            if (null == declarationsConstructor)
            {
                Assembly asm = typeof(PythonConsolePackage).Assembly;
                Type declarationsType = asm.GetType("Microsoft.Samples.VisualStudio.IronPythonConsole.MethodDeclarations");
                declarationsConstructor = declarationsType.GetConstructor(new Type[0]);
            }
            return declarationsConstructor.Invoke(new object[0]) as Declarations;
        }

        private static MethodInfo addMethodInfo;
        private static void AddMethod(Declarations instance, string method)
        {
            if (null == addMethodInfo)
            {
                Assembly asm = typeof(PythonConsolePackage).Assembly;
                Type declarationsType = asm.GetType("Microsoft.Samples.VisualStudio.IronPythonConsole.MethodDeclarations");
                addMethodInfo = declarationsType.GetMethod("AddMethod", BindingFlags.Instance | BindingFlags.Public);
            }
            addMethodInfo.Invoke(instance, new object[] { method });
        }

        [TestMethod]
        public void DeclarationsCountTest()
        {
            Declarations declarations = CreateDeclarations();
            Assert.IsTrue(0 == declarations.GetCount());
            AddMethod(declarations, "Test1");
            Assert.IsTrue(1 == declarations.GetCount());
            AddMethod(declarations, "Test2");
            Assert.IsTrue(2 == declarations.GetCount());
        }

        private static class CheckDeclarationsLimit<T>
        {
            public delegate T GetObjectFunction(int index);
            public static bool CheckIndexException(GetObjectFunction method, int index)
            {
                bool exceptionThrown = false;
                try
                {
                    method(index);
                }
                catch (ArgumentOutOfRangeException)
                {
                    exceptionThrown = true;
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    ArgumentOutOfRangeException inner = e.InnerException as ArgumentOutOfRangeException;
                    if (null != inner)
                        exceptionThrown = true;
                }
                return exceptionThrown;
            }
        }

        [TestMethod]
        public void DeclarationsGetName_BadIndex()
        {
            Declarations declarations = CreateDeclarations();
            CheckDeclarationsLimit<string>.GetObjectFunction getString = 
                new CheckDeclarationsLimit<string>.GetObjectFunction(declarations.GetName);
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, 0));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, -1));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, 1));

            AddMethod(declarations, "Test");
            Assert.IsFalse(CheckDeclarationsLimit<string>.CheckIndexException(getString, 0));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, -1));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, 1));
        }

        [TestMethod]
        public void DeclarationsGetName()
        {
            Declarations declarations = CreateDeclarations();

            // Check that it is possible to add and get back a single string.
            AddMethod(declarations, "Test1");
            Assert.IsTrue("Test1" == declarations.GetName(0));

            // Check that GetName can find the strings on a list that is more
            // than one element long.
            AddMethod(declarations, "Test2");
            Assert.IsTrue("Test2" == declarations.GetName(1));
            Assert.IsTrue("Test1" == declarations.GetName(0));
        }

        [TestMethod]
        public void DeclarationsDisplayText_BadIndex()
        {
            Declarations declarations = CreateDeclarations();
            CheckDeclarationsLimit<string>.GetObjectFunction getString =
                new CheckDeclarationsLimit<string>.GetObjectFunction(declarations.GetDisplayText);
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, 0));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, -1));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, 1));

            AddMethod(declarations, "Test");
            Assert.IsFalse(CheckDeclarationsLimit<string>.CheckIndexException(getString, 0));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, -1));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, 1));
        }

        [TestMethod]
        public void DeclarationsGetDisplayText()
        {
            Declarations declarations = CreateDeclarations();

            // Check that it is possible to add and get back a single string.
            AddMethod(declarations, "Test1");
            Assert.IsTrue("Test1" == declarations.GetDisplayText(0));

            // Check that GetDisplayText can find the strings on a list that is more
            // than one element long.
            AddMethod(declarations, "Test2");
            Assert.IsTrue("Test2" == declarations.GetDisplayText(1));
            Assert.IsTrue("Test1" == declarations.GetDisplayText(0));
        }

        [TestMethod]
        public void DeclarationsGetDescription_BadIndex()
        {
            Declarations declarations = CreateDeclarations();
            CheckDeclarationsLimit<string>.GetObjectFunction getString =
                new CheckDeclarationsLimit<string>.GetObjectFunction(declarations.GetDescription);
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, 0));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, -1));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, 1));

            AddMethod(declarations, "Test");
            Assert.IsFalse(CheckDeclarationsLimit<string>.CheckIndexException(getString, 0));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, -1));
            Assert.IsTrue(CheckDeclarationsLimit<string>.CheckIndexException(getString, 1));
        }

        [TestMethod]
        public void DeclarationsGetDescription()
        {
            Declarations declarations = CreateDeclarations();

            // Check that it is possible to add and get back a single string.
            AddMethod(declarations, "Test1");
            Assert.IsTrue("" == declarations.GetDescription(0));
        }

        [TestMethod]
        public void DeclarationsGetGlyph_BadIndex()
        {
            Declarations declarations = CreateDeclarations();
            CheckDeclarationsLimit<int>.GetObjectFunction getGlyph =
                new CheckDeclarationsLimit<int>.GetObjectFunction(declarations.GetGlyph);
            Assert.IsTrue(CheckDeclarationsLimit<int>.CheckIndexException(getGlyph, 0));
            Assert.IsTrue(CheckDeclarationsLimit<int>.CheckIndexException(getGlyph, -1));
            Assert.IsTrue(CheckDeclarationsLimit<int>.CheckIndexException(getGlyph, 1));

            AddMethod(declarations, "Test");
            Assert.IsFalse(CheckDeclarationsLimit<int>.CheckIndexException(getGlyph, 0));
            Assert.IsTrue(CheckDeclarationsLimit<int>.CheckIndexException(getGlyph, -1));
            Assert.IsTrue(CheckDeclarationsLimit<int>.CheckIndexException(getGlyph, 1));
        }

        [TestMethod]
        public void DeclarationsGetGlyph()
        {
            Declarations declarations = CreateDeclarations();

            // Check that it is possible to add and get back a single string.
            AddMethod(declarations, "Test1");
            Assert.IsTrue(0 == declarations.GetGlyph(0));
        }
    }
}
