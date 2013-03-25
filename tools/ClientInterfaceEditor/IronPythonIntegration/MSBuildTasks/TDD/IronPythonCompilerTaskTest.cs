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
using System.Reflection.Emit;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Samples.VisualStudio.IronPythonTasks;

namespace Microsoft.Samples.VisualStudio.IronPythonTasks.UnitTest
{
	[TestClass()]
	public class IronPythonCompilerTaskTest
	{
		[TestMethod()]
		public void Instantiation1()
		{
			IronPythonCompilerTask task = new IronPythonCompilerTask();
			Assert.IsNotNull(task);
		}

		[TestMethod()]
		public void SourceFiles()
		{
			IronPythonCompilerTask task = new IronPythonCompilerTask();
			// Set item
			string[] sourceFiles = new string[] { "Foo.py", "bar.py" };
			task.SourceFiles = sourceFiles;

			Assert.AreEqual(sourceFiles, task.SourceFiles, "Source files not persisted");
		}

		[TestMethod()]
		public void OutputAssembly()
		{
			IronPythonCompilerTask task = new IronPythonCompilerTask();
			// Set item
			string outputAssembly = "Something.dll";
			task.OutputAssembly = outputAssembly;
			// Verify what we added is preserved
			Assert.AreEqual(outputAssembly, task.OutputAssembly, "OutputAssembly not preserved");
		}

		[TestMethod()]
		public void MainFile()
		{
			IronPythonCompilerTask task = new IronPythonCompilerTask();
			// Set item
			string mainFile = "BlaFile";
			task.MainFile = mainFile;
			// Verify what we added is preserved
			Assert.AreEqual(mainFile, task.MainFile, "MainFile not preserved");
		}

		[TestMethod()]
		public void TargetKind()
		{
			IronPythonCompilerTask task = new IronPythonCompilerTask();
			// Set item
			string peKind = "library";
			task.TargetKind = peKind;
			// Verify what we added is preserved
			Assert.AreEqual(peKind, task.TargetKind, "TargetKind not preserved");
		}

		[TestMethod()]
		public void DebugSymbols()
		{
			IronPythonCompilerTask task = new IronPythonCompilerTask();
			// Set item
			bool debugInformation = true;
			task.DebugSymbols = debugInformation;
			// Verify what we added is preserved
			Assert.AreEqual(debugInformation, task.DebugSymbols, "DebugSymbols not preserved");
		}

		[TestMethod()]
		public void ReferencedAssemblies()
		{
			IronPythonCompilerTask task = new IronPythonCompilerTask();
			// Set item
			task.ReferencedAssemblies = null;
			// Verify what we added is preserved
			Assert.IsNotNull(task.ReferencedAssemblies, "References should not be null");
			Assert.AreEqual(0, task.ReferencedAssemblies.Length, "References should be empty");
		}

		[TestMethod()]
		public void Execute()
		{
			// Get a mock compiler
			ICompiler compiler = CreateMockCompiler();

			IronPythonCompilerTask task = new IronPythonCompilerTask(compiler);
			// Create a fake engine as the logger will call into it
			Type engineType = GenericMockFactory.CreateType("MockBuildEngine", new Type[] { typeof(Microsoft.Build.Framework.IBuildEngine) });
			BaseMock mockEngine = (BaseMock)Activator.CreateInstance(engineType);
			task.BuildEngine = (Microsoft.Build.Framework.IBuildEngine)mockEngine;

			// Set parameters
			task.SourceFiles = new string[] { "Foo.py", "bar.py" };
			task.TargetKind = "exe";
			task.MainFile = "Foo.py";
			task.ResourceFiles = null;
			Microsoft.Build.Framework.ITaskItem[] resources = new Microsoft.Build.Framework.ITaskItem[1];
			resources[0] = new Microsoft.Build.Utilities.TaskItem(@"obj\i386\form1.resources");
			task.ResourceFiles = resources;
			// Execute
			bool result = task.Execute();

			// Validation
			Assert.IsTrue(result);
			BaseMock mock = (BaseMock)compiler;
			Assert.AreEqual(PEFileKinds.ConsoleApplication, mock["TargetKind"]);
			Assert.AreEqual(task.MainFile, mock["MainFile"]);
		}

		private static ICompiler CreateMockCompiler()
		{
			Type compilerType = GenericMockFactory.CreateType("MockCompiler", new Type[] { typeof(ICompiler) });
			BaseMock mockCompiler = (BaseMock)Activator.CreateInstance(compilerType);
			string name = string.Format("{0}.{1}", typeof(ICompiler).FullName, "set_SourceFiles");
			mockCompiler.AddMethodCallback(name, new EventHandler<CallbackArgs>(SourceFilesCallBack));
			name = string.Format("{0}.{1}", typeof(ICompiler).FullName, "set_OutputAssembly");
			mockCompiler.AddMethodCallback(name, new EventHandler<CallbackArgs>(OutputAssemblyCallBack));
			name = string.Format("{0}.{1}", typeof(ICompiler).FullName, "set_ReferencedAssemblies");
			mockCompiler.AddMethodCallback(name, new EventHandler<CallbackArgs>(ReferencedAssembliesCallBack));
			name = string.Format("{0}.{1}", typeof(ICompiler).FullName, "set_MainFile");
			mockCompiler.AddMethodCallback(name, new EventHandler<CallbackArgs>(MainFileCallBack));
			name = string.Format("{0}.{1}", typeof(ICompiler).FullName, "set_IncludeDebugInformation");
			mockCompiler.AddMethodCallback(name, new EventHandler<CallbackArgs>(IncludeDebugInformationCallBack));
			name = string.Format("{0}.{1}", typeof(ICompiler).FullName, "set_TargetKind");
			mockCompiler.AddMethodCallback(name, new EventHandler<CallbackArgs>(TargetKindCallBack));

			return (ICompiler)mockCompiler;
		}

		private static void SourceFilesCallBack(object caller, CallbackArgs arguments)
		{
			BaseMock compiler = (BaseMock)caller;
			compiler["SourceFiles"] = arguments.GetParameter(0);
		}
		private static void OutputAssemblyCallBack(object caller, CallbackArgs arguments)
		{
			BaseMock compiler = (BaseMock)caller;
			compiler["OutputAssembly"] = arguments.GetParameter(0);
		}
		private static void ReferencedAssembliesCallBack(object caller, CallbackArgs arguments)
		{
			BaseMock compiler = (BaseMock)caller;
			compiler["ReferencedAssemblies"] = arguments.GetParameter(0);
		}
		private static void MainFileCallBack(object caller, CallbackArgs arguments)
		{
			BaseMock compiler = (BaseMock)caller;
			compiler["MainFile"] = arguments.GetParameter(0);
		}
		private static void IncludeDebugInformationCallBack(object caller, CallbackArgs arguments)
		{
			BaseMock compiler = (BaseMock)caller;
			compiler["IncludeDebugInformation"] = arguments.GetParameter(0);
		}
		private static void TargetKindCallBack(object caller, CallbackArgs arguments)
		{
			BaseMock compiler = (BaseMock)caller;
			compiler["TargetKind"] = arguments.GetParameter(0);
		}
	}
}
