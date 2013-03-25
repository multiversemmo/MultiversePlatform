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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    internal static class CommandWindowHelper
    {
        // Creation of an instance of the console window.
        // The class that we want to instanziate is defined as internal in the 
        // target assembly, so we can not use 'new' to create an object, but we have
        // to use reflection to load the type, find the constructor and call it
        // to get an instance of the class.
        private static Type consoleType;
        public static Type ConsoleType
        {
            get
            {
                if (null == consoleType)
                {
                    Assembly asm = typeof(PythonConsolePackage).Assembly;
                    consoleType = asm.GetType("Microsoft.Samples.VisualStudio.IronPythonConsole.ConsoleWindow", true);
                }
                return consoleType;
            }
        }

        private static ConstructorInfo consoleConstructorWithProvider;
        private static ConstructorInfo standardConsoleConstructor;
        public static object CreateConsoleWindow(System.IServiceProvider provider)
        {
            if (null == consoleConstructorWithProvider)
            {
                consoleConstructorWithProvider = ConsoleType.GetConstructor(new Type[] { typeof(System.IServiceProvider) });
            }
            return consoleConstructorWithProvider.Invoke(new object[] { provider });
        }
        public static object CreateConsoleWindow()
        {
            if (null == standardConsoleConstructor)
            {
                standardConsoleConstructor = ConsoleType.GetConstructor(new Type[] { });
            }
            return standardConsoleConstructor.Invoke(new object[] { });
        }

        // Get the internal buffer used to store the lines added by the user and not yet passed to
        // the engine.
        private static FieldInfo inputBufferInfo;
        public static int LinesInInputBuffer(object consoleInstance)
        {
            if (null == inputBufferInfo)
            {
                inputBufferInfo = ConsoleType.GetField("inputBuffer", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            CommandBuffer buffer = (CommandBuffer)inputBufferInfo.GetValue(consoleInstance);
            string bufferText = buffer.Text;
            if (string.IsNullOrEmpty(bufferText))
            {
                return 0;
            }
            string[] lines = bufferText.Split('\n');
            return lines.Length;
        }

        // Gets the stream that writes on the console's text view.
        private static FieldInfo textStreamInfo;
        public static System.IO.Stream ConsoleStream(object consoleInstance)
        {
            if (null == textStreamInfo)
            {
                textStreamInfo = ConsoleType.GetField("textStream", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            return (System.IO.Stream)textStreamInfo.GetValue(consoleInstance);
        }

        // Make sure that the text marker for the read only reagion is created.
        public static void EnsureConsoleTextMarker(object consoleInstance)
        {
            System.IO.Stream consoleStream = ConsoleStream(consoleInstance);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(consoleStream);
            writer.Write("");
            writer.Flush();
        }

        private static MethodInfo supportCommandOnInputPosition;
        public static void ExecuteSupportCommandOnInputPosition(object consoleInstance, object sender)
        {
            if (null == supportCommandOnInputPosition)
            {
                supportCommandOnInputPosition = ConsoleType.GetMethod("SupportCommandOnInputPosition", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            supportCommandOnInputPosition.Invoke(consoleInstance, new object[] { sender, EventArgs.Empty });
        }

        private static MethodInfo onBeforeMeveLeftInfo;
        public static void ExecuteOnBeforeMoveLeft(object consoleInstance, object sender)
        {
            if (null == onBeforeMeveLeftInfo)
            {
                onBeforeMeveLeftInfo = ConsoleType.GetMethod("OnBeforeMoveLeft", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            onBeforeMeveLeftInfo.Invoke(consoleInstance, new object[] { sender, EventArgs.Empty });
        }

    }
}
