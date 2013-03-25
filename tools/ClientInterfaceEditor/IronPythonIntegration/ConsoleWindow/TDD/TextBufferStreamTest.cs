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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

// Visual Studio Platform
using Microsoft.VisualStudio.TextManager.Interop;

// Unit test infrastructure.
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// The namespace of the classes to test.
using Microsoft.Samples.VisualStudio.IronPythonConsole;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    public class TestStreamException : Exception
    {
    }

    [TestClass]
    public class TextBufferStreamTest
    {
        private static Type streamType;
        private static ConstructorInfo streamConstructor;
        private static Stream CreateTextBufferStream(IVsTextLines textLines)
        {
            if (null == streamConstructor)
            {
                if (null == streamType)
                {
                    Assembly asm = typeof(PythonConsolePackage).Assembly;
                    streamType = asm.GetType("Microsoft.Samples.VisualStudio.IronPythonConsole.TextBufferStream", true);
                }
                streamConstructor = streamType.GetConstructor(new Type[] { typeof(IVsTextLines) });
            }
            return (Stream)streamConstructor.Invoke(new object[] { textLines });
        }
        private static int BufferSize
        {
            get
            {
                if (null == streamType)
                {
                    Assembly asm = typeof(PythonConsolePackage).Assembly;
                    streamType = asm.GetType("Microsoft.Samples.VisualStudio.IronPythonConsole.TextBufferStream", true);
                }
                FieldInfo bufferSizeInfo = streamType.GetField("bufferSize", BindingFlags.NonPublic | BindingFlags.Static);
                return (int)bufferSizeInfo.GetRawConstantValue();
            }
        }

        // Check that the constructor can handle a null buffer.
        [TestMethod]
        public void StreamConstructorNullBuffer()
        {
            bool exceptionThrown = false;
            try
            {
                Stream stream = CreateTextBufferStream(null);
                stream.Dispose();
            }
            catch (ArgumentNullException)
            {
                exceptionThrown = true;
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                ArgumentNullException inner = e.InnerException as ArgumentNullException;
                if (null != inner)
                    exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown);
        }

        // Check the value of the standard properties of the stream.
        [TestMethod]
        public void StreamProperties()
        {
            BaseMock mockBuffer = MockFactories.CreateBufferWithMarker();
            mockBuffer.AddMethodReturnValues(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetSize"),
                                             new object[] { 0, 42 });
            mockBuffer.AddMethodReturnValues(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetPositionOfLineIndex"),
                                             new object[] { 0, 0, 0, 21 });
            using (Stream stream = CreateTextBufferStream((IVsTextLines)mockBuffer))
            {
                Assert.IsFalse(stream.CanRead);
                Assert.IsFalse(stream.CanSeek);
                Assert.IsTrue(stream.CanWrite);
                Assert.IsTrue(42 == stream.Length);
                Assert.IsTrue(21 == stream.Position);
                bool exceptionThrown = false;
                try
                {
                    stream.Position = 11;
                }
                catch (NotImplementedException)
                {
                    exceptionThrown = true;
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    NotImplementedException inner = e.InnerException as NotImplementedException;
                    if (null != inner)
                        exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);
            }
        }

        [TestMethod]
        public void TestRead()
        {
            IVsTextLines textLines = (IVsTextLines)MockFactories.CreateBufferWithMarker();
            using (Stream stream = CreateTextBufferStream(textLines))
            {
                bool exceptionThrown = false;
                try
                {
                    byte[] buffer = new byte[2];
                    stream.Read(buffer, 0, 2);
                }
                catch (NotImplementedException)
                {
                    exceptionThrown = true;
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    NotImplementedException inner = e.InnerException as NotImplementedException;
                    if (null != inner)
                        exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);
            }
        }

        [TestMethod]
        public void TestSeek()
        {
            IVsTextLines textLines = (IVsTextLines)MockFactories.CreateBufferWithMarker();
            using (Stream stream = CreateTextBufferStream(textLines))
            {
                bool exceptionThrown = false;
                try
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
                catch (NotImplementedException)
                {
                    exceptionThrown = true;
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    NotImplementedException inner = e.InnerException as NotImplementedException;
                    if (null != inner)
                        exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);
            }
        }

        [TestMethod]
        public void TestSetLength()
        {
            IVsTextLines textLines = (IVsTextLines)MockFactories.CreateBufferWithMarker();
            using (Stream stream = CreateTextBufferStream(textLines))
            {
                bool exceptionThrown = false;
                try
                {
                    stream.SetLength(56);
                }
                catch (NotImplementedException)
                {
                    exceptionThrown = true;
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    NotImplementedException inner = e.InnerException as NotImplementedException;
                    if (null != inner)
                        exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);
            }
        }

        [TestMethod]
        public void WriteNullBuffer()
        {
            IVsTextLines textLines = (IVsTextLines)MockFactories.CreateBufferWithMarker();
            using (Stream stream = CreateTextBufferStream(textLines))
            {
                bool exceptionThrown = false;
                try
                {
                    stream.Write(null, 0, 1);
                }
                catch (ArgumentNullException)
                {
                    exceptionThrown = true;
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    ArgumentNullException inner = e.InnerException as ArgumentNullException;
                    if (null != inner)
                        exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);
            }
        }

        private bool IsWriteOutOfRange(Stream stream, byte[] buffer, int offset, int count)
        {
            bool exceptionThrown = false;
            try
            {
                stream.Write(buffer, offset, count);
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

        [TestMethod]
        public void WriteInvalidArguments()
        {
            IVsTextLines textLines = (IVsTextLines)MockFactories.CreateBufferWithMarker();
            using (Stream stream = CreateTextBufferStream(textLines))
            {
                byte[] buffer = new byte[2];
                Assert.IsTrue(IsWriteOutOfRange(stream, buffer, -1, 1));
                Assert.IsTrue(IsWriteOutOfRange(stream, buffer, 2, 1));
                Assert.IsTrue(IsWriteOutOfRange(stream, buffer, 0, -1));
                Assert.IsTrue(IsWriteOutOfRange(stream, buffer, 1, 2));
                Assert.IsFalse(IsWriteOutOfRange(stream, buffer, 1, 1));
            }
        }

        [TestMethod]
        public void WriteSmallBuffer()
        {
            BaseMock mockBuffer = MockFactories.CreateBufferWithMarker();
            string bufferWriteFunction = string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines");
            using (Stream stream = CreateTextBufferStream((IVsTextLines)mockBuffer))
            {
                // Verify that writing a small buffer will not cause a change in the
                // buffer (so Flush is not called).
                int writeSize = 10;
                byte[] buffer = new byte[writeSize];
                stream.Write(buffer, 0, writeSize);
                Assert.IsTrue(0 == mockBuffer.FunctionCalls(bufferWriteFunction));

                // Now write anothor buffer big enough to leave only 1 not used spot in the
                // internal buffer of the stream.
                writeSize = BufferSize - writeSize - 1;
                buffer = new byte[writeSize];
                stream.Write(buffer, 0, writeSize);
                Assert.IsTrue(0 == mockBuffer.FunctionCalls(bufferWriteFunction));

                // Verify that writing another byte will cause the data to be written on the
                // text buffer.
                stream.Write(buffer, 0, 1);
                Assert.IsTrue(1 == mockBuffer.FunctionCalls(bufferWriteFunction));
            }
            int lockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "LockBuffer"));
            int unlockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "UnlockBuffer"));
            Assert.IsTrue(lockCount == unlockCount);
        }

        [TestMethod]
        public void WriteBigBuffer()
        {
            BaseMock mockBuffer = MockFactories.CreateBufferWithMarker();
            string bufferWriteFunction = string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines");
            using (Stream stream = CreateTextBufferStream((IVsTextLines)mockBuffer))
            {
                int writeSize = BufferSize + BufferSize / 2;
                byte[] buffer = new byte[writeSize];
                stream.Write(buffer, 0, writeSize);
                Assert.IsTrue(1 == mockBuffer.FunctionCalls(bufferWriteFunction));
                stream.Write(buffer, 0, writeSize);
                Assert.IsTrue(3 == mockBuffer.FunctionCalls(bufferWriteFunction));
            }
            int lockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "LockBuffer"));
            int unlockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "UnlockBuffer"));
            Assert.IsTrue(lockCount == unlockCount);
        }

        [TestMethod]
        public void FlushEmptyBuffer()
        {
            BaseMock mockBuffer = MockFactories.CreateBufferWithMarker();
            string bufferWriteFunction = string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines");
            using (Stream stream = CreateTextBufferStream((IVsTextLines)mockBuffer))
            {
                stream.Flush();
                Assert.IsTrue(0 == mockBuffer.FunctionCalls(bufferWriteFunction));
            }
            int lockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "LockBuffer"));
            int unlockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "UnlockBuffer"));
            Assert.IsTrue(lockCount == unlockCount);
        }

        private static void ReplaceLinesCallback(object sender, CallbackArgs args)
        {
            Assert.IsTrue(11 == (int)args.GetParameter(0));
            Assert.IsTrue(42 == (int)args.GetParameter(1));
            Assert.IsTrue(11 == (int)args.GetParameter(2));
            Assert.IsTrue(42 == (int)args.GetParameter(3));
            IntPtr stringPointer = (IntPtr)args.GetParameter(4);
            int stringLen = (int)args.GetParameter(5);
            Assert.IsTrue(IntPtr.Zero != stringPointer);
            Assert.IsTrue(stringLen > 0);
            string newText = Marshal.PtrToStringAuto(stringPointer, stringLen);
            BaseMock mock = (BaseMock)sender;
            mock["Text"] = (string)mock["Text"] + newText;
            args.ReturnValue = Microsoft.VisualStudio.VSConstants.S_OK;
        }
        [TestMethod]
        public void TestFlushFromASCII()
        {
            BaseMock mockBuffer = MockFactories.CreateBufferWithMarker();
            mockBuffer.AddMethodReturnValues(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLastLineIndex"),
                                             new object[] { 0, 11, 42 });
            mockBuffer["Text"] = "";
            mockBuffer.AddMethodCallback(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines"),
                                         new EventHandler<CallbackArgs>(ReplaceLinesCallback));
            using (Stream stream = CreateTextBufferStream((IVsTextLines)mockBuffer))
            {
                string test = "� Test �";
                using (StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.ASCII))
                {
                    writer.Write(test);
                    writer.Flush();
                    // There is no ASCII translation for �, so the standard replacement is used.
                    Assert.IsTrue((string)mockBuffer["Text"] == "? Test ?");
                }
            }
            int lockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "LockBuffer"));
            int unlockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "UnlockBuffer"));
            Assert.IsTrue(lockCount == unlockCount);
        }

        [TestMethod]
        public void TestFlushFromUnicode()
        {
            BaseMock mockBuffer = MockFactories.CreateBufferWithMarker();
            mockBuffer.AddMethodReturnValues(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLastLineIndex"),
                                             new object[] { 0, 11, 42 });
            mockBuffer["Text"] = "";
            mockBuffer.AddMethodCallback(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines"),
                                         new EventHandler<CallbackArgs>(ReplaceLinesCallback));
            using (Stream stream = CreateTextBufferStream((IVsTextLines)mockBuffer))
            {
                string test = "� Test �";
                using (StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.Unicode))
                {
                    writer.Write(test);
                    writer.Flush();
                    Assert.IsTrue((string)mockBuffer["Text"] == test);
                }
            }
            int lockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "LockBuffer"));
            int unlockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "UnlockBuffer"));
            Assert.IsTrue(lockCount == unlockCount);
        }

        [TestMethod]
        public void TestFlushFromUTF8()
        {
            BaseMock mockBuffer = MockFactories.CreateBufferWithMarker();
            mockBuffer.AddMethodReturnValues(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "GetLastLineIndex"),
                                             new object[] { 0, 11, 42 });
            mockBuffer["Text"] = "";
            mockBuffer.AddMethodCallback(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines"),
                                         new EventHandler<CallbackArgs>(ReplaceLinesCallback));
            using (Stream stream = CreateTextBufferStream((IVsTextLines)mockBuffer))
            {
                string test = "� Test �";
                using (StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.UTF8))
                {
                    writer.Write(test);
                    writer.Flush();
                    Assert.IsTrue((string)mockBuffer["Text"] == test);
                }
            }
            int lockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "LockBuffer"));
            int unlockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "UnlockBuffer"));
            Assert.IsTrue(lockCount == unlockCount);
        }

        private static void ReplaceLinesThrow(object sender, CallbackArgs args)
        {
            throw new TestStreamException();
        }
        [TestMethod]
        public void TestFlushWithException()
        {
            BaseMock mockBuffer = MockFactories.CreateBufferWithMarker();
            mockBuffer.AddMethodCallback(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "ReplaceLines"),
                                         new EventHandler<CallbackArgs>(ReplaceLinesThrow));
            bool exceptionThrown = false;
            try
            {
                using (Stream stream = CreateTextBufferStream((IVsTextLines)mockBuffer))
                {
                    string test = "Test Line";
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(test);
                        writer.Flush();
                    }
                }
            }
            catch (TestStreamException)
            {
                exceptionThrown = true;
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                TestStreamException inner = e.InnerException as TestStreamException;
                if (null != inner)
                    exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown);
            int lockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "LockBuffer"));
            int unlockCount = mockBuffer.FunctionCalls(string.Format("{0}.{1}", typeof(IVsTextLines).FullName, "UnlockBuffer"));
            Assert.IsTrue(lockCount == unlockCount);
        }
    }
}
