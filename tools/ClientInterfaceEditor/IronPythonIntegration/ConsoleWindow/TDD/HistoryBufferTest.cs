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

// System references
using System;
using System.Reflection;

// The namespace to test.
using Microsoft.Samples.VisualStudio.IronPythonConsole;

// Unit test framework.
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    [TestClass]
    public class HistoryBufferTest
    {
        private static Type historyBufferType;
        private static ConstructorInfo historyBufferCtrInfo;
        private static object CreateBuffer()
        {
            if (null == historyBufferCtrInfo)
            {
                if (null == historyBufferType)
                {
                    Assembly asm = typeof(PythonConsolePackage).Assembly;
                    historyBufferType = asm.GetType("Microsoft.Samples.VisualStudio.IronPythonConsole.HistoryBuffer", true);
                }
                historyBufferCtrInfo = historyBufferType.GetConstructor(new Type[] { });
            }
            return historyBufferCtrInfo.Invoke(new object[] { });
        }

        private static ConstructorInfo historyBufferCtrSizeInfo;
        private static object CreateBuffer(int size)
        {
            if (null == historyBufferCtrSizeInfo)
            {
                if (null == historyBufferType)
                {
                    Assembly asm = typeof(PythonConsolePackage).Assembly;
                    historyBufferType = asm.GetType("Microsoft.Samples.VisualStudio.IronPythonConsole.HistoryBuffer", true);
                }
                historyBufferCtrSizeInfo = historyBufferType.GetConstructor(new Type[] { typeof(int) });
            }
            return historyBufferCtrSizeInfo.Invoke(new object[] { size });
        }

        private static MethodInfo previousEntryInfo;
        private static string PreviousEntry(object buffer)
        {
            if (null == previousEntryInfo)
            {
                previousEntryInfo = historyBufferType.GetMethod("PreviousEntry");
            }
            return (string)previousEntryInfo.Invoke(buffer, new object[] { });
        }

        private static MethodInfo nextEntryInfo;
        private static string NextEntry(object buffer)
        {
            if (null == nextEntryInfo)
            {
                nextEntryInfo = historyBufferType.GetMethod("NextEntry");
            }
            return (string)nextEntryInfo.Invoke(buffer, new object[] { });
        }

        private static MethodInfo addEntryInfo;
        private static string AddEntry(object buffer, string entry)
        {
            if (null == addEntryInfo)
            {
                addEntryInfo = historyBufferType.GetMethod("AddEntry");
            }
            return (string)addEntryInfo.Invoke(buffer, new object[] { entry });
        }

        private static bool HasConstructorSucceeded(int size)
        {
            bool exceptionThrown = false;
            try
            {
                object buffer = CreateBuffer(size);
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
            return !exceptionThrown;
        }

        [TestMethod]
        public void VerifySizeOnConstructor()
        {
            Assert.IsFalse(HasConstructorSucceeded(-3));
            Assert.IsFalse(HasConstructorSucceeded(0));
            Assert.IsTrue(HasConstructorSucceeded(3));
        }

        [TestMethod]
        public void PreviousOnEmptyBuffer()
        {
            object buffer = CreateBuffer();
            Assert.IsNull(PreviousEntry(buffer));
        }

        [TestMethod]
        public void NextOnEmptyBuffer()
        {
            object buffer = CreateBuffer();
            Assert.IsNull(NextEntry(buffer));
        }

        [TestMethod]
        public void AddOneItem()
        {
            object buffer = CreateBuffer();
            string newEntry = "New Entry";
            AddEntry(buffer, newEntry);
            Assert.IsNull(NextEntry(buffer));
            Assert.IsTrue(newEntry == PreviousEntry(buffer));
            Assert.IsNull(PreviousEntry(buffer));
            Assert.IsNull(NextEntry(buffer));
        }

        [TestMethod]
        public void AddOverCapacity()
        {
            object buffer = CreateBuffer(3);
            AddEntry(buffer, "String 1");
            AddEntry(buffer, "String 2");
            AddEntry(buffer, "String 3");
            AddEntry(buffer, "String 4");
            Assert.IsTrue("String 4" == PreviousEntry(buffer));
            Assert.IsTrue("String 3" == PreviousEntry(buffer));
            Assert.IsTrue("String 2" == PreviousEntry(buffer));
            Assert.IsNull(PreviousEntry(buffer));
            Assert.IsTrue("String 3" == NextEntry(buffer));
            Assert.IsTrue("String 4" == NextEntry(buffer));
            Assert.IsNull(NextEntry(buffer));
        }

        [TestMethod]
        public void AddExistingItem()
        {
            object buffer = CreateBuffer();
            AddEntry(buffer, "String 1");
            AddEntry(buffer, "String 2");
            AddEntry(buffer, "String 3");
            AddEntry(buffer, "String 4");
            AddEntry(buffer, "String 2");
            Assert.IsTrue("String 2" == PreviousEntry(buffer));
            Assert.IsTrue("String 1" == PreviousEntry(buffer));
            Assert.IsNull(PreviousEntry(buffer));
            Assert.IsTrue("String 2" == NextEntry(buffer));
            Assert.IsTrue("String 3" == NextEntry(buffer));
            Assert.IsTrue("String 4" == NextEntry(buffer));
            Assert.IsNull(NextEntry(buffer));
        }

        [TestMethod]
        public void AddExistingItemNavigateDown()
        {
            object buffer = CreateBuffer();
            AddEntry(buffer, "String 1");
            AddEntry(buffer, "String 2");
            AddEntry(buffer, "String 3");
            AddEntry(buffer, "String 4");
            AddEntry(buffer, "String 2");
            Assert.IsTrue("String 3" == NextEntry(buffer));
            Assert.IsTrue("String 2" == PreviousEntry(buffer));
            Assert.IsTrue("String 3" == NextEntry(buffer));
            Assert.IsTrue("String 4" == NextEntry(buffer));
            Assert.IsNull(NextEntry(buffer));
        }

        [TestMethod]
        public void Nativage()
        {
            object buffer = CreateBuffer();
            AddEntry(buffer, "String 1");
            AddEntry(buffer, "String 2");
            AddEntry(buffer, "String 3");
            Assert.IsNull(NextEntry(buffer));
            Assert.IsTrue("String 3" == PreviousEntry(buffer));
            Assert.IsTrue("String 2" == PreviousEntry(buffer));
            Assert.IsTrue("String 3" == NextEntry(buffer));
            Assert.IsTrue("String 2" == PreviousEntry(buffer));
        }
    }
}
