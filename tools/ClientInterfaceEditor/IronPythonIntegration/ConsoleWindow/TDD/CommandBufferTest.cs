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

// Unit test framework.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VsSDK.UnitTestLibrary;

// Namespaces to test.
using Microsoft.Samples.VisualStudio.IronPythonInterfaces;
using Microsoft.Samples.VisualStudio.IronPythonConsole;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    [TestClass]
    public class CommandBufferTest
    {
        private class ExecuteException : Exception
        {
        }

        private const string standardPrompt = ">>>";
        private const string multiLinePrompt = "...";

        private static string parseFunctionName = string.Format("{0}.{1}", typeof(IEngine).FullName, "ParseInteractiveInput");

        private static void ExecuteToConsoleCallback(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            mock["ExecutedCommand"] = args.GetParameter(0);
        }
        private static BaseMock CreateDefaultEngine()
        {
            // Create the mock.
            BaseMock mockEngine = MockFactories.EngineFactory.GetInstance();
            // Set the callback function for the ExecuteToConsole method of the engine.
            mockEngine.AddMethodCallback(
                string.Format("{0}.{1}", typeof(IEngine).FullName, "ExecuteToConsole"),
                new EventHandler<CallbackArgs>(ExecuteToConsoleCallback));
            return mockEngine;
        }

        // Verify that the command buffer handle as expected a null engine
        // in the constructor.
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommandBufferNullEngine()
        {
            CommandBuffer buffer = new CommandBuffer(null);
        }

        [TestMethod]
        public void CommandBufferConstructor()
        {
            // Build a command buffer with an engine without in, out and err streams.
            BaseMock mockEngine = MockFactories.EngineFactory.GetInstance();
            CommandBuffer buffer = new CommandBuffer(mockEngine as IEngine);

            // Now set a not empty value for the out and err streams.
            byte[] streamBuffer = new byte[256];
            long usedBytes = 0;
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(streamBuffer, true))
            {
                mockEngine.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IEngine).FullName, "get_StdOut"),
                    new object[] { (System.IO.Stream)stream });
                buffer = new CommandBuffer(mockEngine as IEngine);
                usedBytes = stream.Position;
            }
            // Verify that the prompt (and only the prompt) was written on the stream.
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(streamBuffer, 0, (int)usedBytes))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    string streamText = reader.ReadToEnd();
                    Assert.AreEqual<string>(streamText, standardPrompt);
                }
            }
        }

        [TestMethod]
        public void CommandBufferAddMultiLine()
        {
            BaseMock mockEngine = CreateDefaultEngine();
            CommandBuffer buffer = new CommandBuffer(mockEngine as IEngine);
            mockEngine.AddMethodReturnValues(parseFunctionName, new object[] { false });
            string[] lines = new string[] {
                "Line 1",
                "Line 2",
                "Line 3"
            };
            const string lastLine = "Last Line";
            mockEngine.ResetFunctionCalls(string.Format("{0}.{1}", typeof(IEngine).FullName, "ExecuteToConsole"));
            string expected = string.Empty;
            foreach (string line in lines)
            {
                expected += line;
                buffer.Add(line);
                Assert.AreEqual<string>(expected, buffer.Text);
                Assert.AreEqual<int>(0, mockEngine.FunctionCalls(string.Format("{0}.{1}", typeof(IEngine).FullName, "ExecuteToConsole")));
                expected += System.Environment.NewLine;
            }

            // Now change the return value for ParseInteractiveInput so that the execute 
            // function of the engine is called.
            mockEngine.AddMethodReturnValues(parseFunctionName, new object[] { true });
            expected += lastLine;
            buffer.Add(lastLine);
            // Now the buffer should be cleared and the text should be passed to the engine.
            Assert.IsTrue(string.IsNullOrEmpty(buffer.Text));
            Assert.AreEqual<int>(1, mockEngine.FunctionCalls(string.Format("{0}.{1}", typeof(IEngine).FullName, "ExecuteToConsole")));
            Assert.AreEqual<string>(expected, (string)mockEngine["ExecutedCommand"]);
        }

        [TestMethod]
        public void CommandBufferAddNullLine()
        {
            BaseMock mockEngine = CreateDefaultEngine();
            CommandBuffer buffer = new CommandBuffer(mockEngine as IEngine);
            mockEngine.AddMethodReturnValues(parseFunctionName, new object[] { true });
            mockEngine.ResetFunctionCalls(string.Format("{0}.{1}", typeof(IEngine).FullName, "ExecuteToConsole"));
            buffer.Add(null);
            Assert.AreEqual<int>(1, mockEngine.FunctionCalls(string.Format("{0}.{1}", typeof(IEngine).FullName, "ExecuteToConsole")));
            Assert.IsTrue(string.IsNullOrEmpty((string)mockEngine["ExecutedCommand"]));
        }

        private static void ThrowingExecuteCallback(object sender, CallbackArgs args)
        {
            throw new ExecuteException();
        }
        [TestMethod]
        public void CommandBufferAddWithThrowingExecute()
        {
            // Create the mock engine.
            BaseMock mockEngine = MockFactories.EngineFactory.GetInstance();
            // Set the callback function for the ExecuteToConsole method of the engine to the throwing one.
            mockEngine.AddMethodCallback(
                string.Format("{0}.{1}", typeof(IEngine).FullName, "ExecuteToConsole"),
                new EventHandler<CallbackArgs>(ThrowingExecuteCallback));
            // Make sure that the execute is called.
            mockEngine.AddMethodReturnValues(parseFunctionName, new object[] { true });
            CommandBuffer buffer = new CommandBuffer(mockEngine as IEngine);
            bool exceptionThrown = false;
            try
            {
                buffer.Add("Test Line");
            }
            catch (ExecuteException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(string.IsNullOrEmpty(buffer.Text));
        }

        [TestMethod]
        public void CommandBufferOutput()
        {
            BaseMock mockEngine = CreateDefaultEngine();

            // Make sure that the execute is called.
            mockEngine.AddMethodReturnValues(parseFunctionName, new object[] { true });

            string expected = standardPrompt + System.Environment.NewLine + standardPrompt;

            // Now set a not empty value for the out and err streams.
            byte[] streamBuffer = new byte[256];
            long usedBytes = 0;
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(streamBuffer, true))
            {
                // Set this stream as standard output for the mock engine.
                mockEngine.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IEngine).FullName, "get_StdOut"),
                    new object[] { (System.IO.Stream)stream });
                // Create the command buffer.
                CommandBuffer buffer = new CommandBuffer(mockEngine as IEngine);
                buffer.Add("Test");
                usedBytes = stream.Position;
            }
            // Verify the content of the standard output.
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(streamBuffer, 0, (int)usedBytes))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    string streamText = reader.ReadToEnd();
                    Assert.AreEqual<string>(streamText, expected);
                }
            }

            // Redo the same with a multi-line command.
            usedBytes = 0;
            expected = standardPrompt + System.Environment.NewLine + multiLinePrompt + System.Environment.NewLine + standardPrompt;
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(streamBuffer, true))
            {
                // Set this stream as standard output for the mock engine.
                mockEngine.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IEngine).FullName, "get_StdOut"),
                    new object[] { (System.IO.Stream)stream });
                // Create the command buffer.
                CommandBuffer buffer = new CommandBuffer(mockEngine as IEngine);
                // Force a multi-line execution.
                mockEngine.AddMethodReturnValues(parseFunctionName, new object[] { false });
                buffer.Add("Line 1");
                // Now execute the command.
                mockEngine.AddMethodReturnValues(parseFunctionName, new object[] { true });
                buffer.Add("Line 2");
                usedBytes = stream.Position;
            }
            // Verify the content of the standard output.
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(streamBuffer, 0, (int)usedBytes))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    string streamText = reader.ReadToEnd();
                    Assert.AreEqual<string>(streamText, expected);
                }
            }
        }
    }
}
