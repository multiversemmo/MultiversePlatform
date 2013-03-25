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
using System.Text;
using Microsoft.Build.Utilities;

namespace Microsoft.Samples.VisualStudio.IronPythonTasks
{
	class CompilerErrorSink : IronPython.Hosting.CompilerSink
	{
		private bool buildSucceeded = true;
		private TaskLoggingHelper taskLogger;
		private string projectDirectory = null;

		/// <summary>
		/// This is the directory where the project is located.
		/// If set, it will be used to calculate full paths of files.
		/// </summary>
		public string ProjectDirectory
		{
			get { return projectDirectory; }
			set { projectDirectory = value; }
		}
	
		/// <summary>
		/// Constructor for the error sink
		/// </summary>
		/// <param name="logger">This parameter should be the logger for the task being executed</param>
		public CompilerErrorSink(TaskLoggingHelper logger)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");
			taskLogger = logger;
		}

		public bool BuildSucceeded
		{
			get { return buildSucceeded; }
		}

		/// <summary>
		/// Log Errors/Warnings/Messages when the compiler reports them.
		/// </summary>
		/// <param name="path">Path to the file where the error was found (null/empty if N/A)</param>
		/// <param name="message">Text of the error/warning/message</param>
		/// <param name="startLine">First line of the block containing the error (0 if N/A)</param>
		/// <param name="startColumn">First column of the block containing the error (0 if N/A)</param>
		/// <param name="endLine">Last line of the block containing the error (0 if N/A)</param>
		/// <param name="endColumn">Last column of the block containing the error (0 if N/A)</param>
		/// <param name="errorCode">Code corresponding to the error</param>
		/// <param name="severity">Error/Warning/Message</param>
		public override void AddError(string path, string message, string lineText, IronPython.Hosting.CodeSpan location, int errorCode, IronPython.Hosting.Severity severity)
		{
			if (ProjectDirectory != null && !System.IO.Path.IsPathRooted(path))
				path = System.IO.Path.Combine(ProjectDirectory, path);
			// Based on the type of event (error/warning/message), report the corresponding type of problem to MSBuild
			switch(severity)
			{
				case IronPython.Hosting.Severity.Error:
					{
						buildSucceeded = false;
						taskLogger.LogError(String.Empty, "PY" + errorCode.ToString(), String.Empty, path, location.StartLine, location.StartColumn, location.EndLine, location.EndColumn, message);
						break;
					}
				case IronPython.Hosting.Severity.Warning:
					{
						taskLogger.LogWarning(String.Empty, "PY" + errorCode.ToString(), String.Empty, path, location.StartLine, location.StartColumn, location.EndLine, location.EndColumn, message);
						break;
					}
				case IronPython.Hosting.Severity.Message:
					{
						taskLogger.LogMessage(message);
						break;
					}
			}
		}
	}
}
