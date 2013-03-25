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

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using log4net;
using Axiom.Core;
using Multiverse.AssetRepository;
using Multiverse.Lib.LogUtil;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, ControlThread = true)]

namespace Multiverse.Tools.WorldEditor
{

    static class Program
    {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
        private static string MyDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string ToolsAppDataFolder = Path.Combine(MyDocumentsFolder, "Multiverse Tools");
        private static string ConfigFolder = Path.Combine(ToolsAppDataFolder, "Config");
        private static string LogFolder = Path.Combine(ToolsAppDataFolder, "Logs");
        private static string FallbackLogfile = Path.Combine(LogFolder, "WorldEditor.log");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                String execDir = Application.ExecutablePath.Substring(0,Application.ExecutablePath.LastIndexOf("\\"));
                Directory.SetCurrentDirectory(execDir);
                // Changes the CurrentCulture of the current thread to the invariant culture.
                Thread.CurrentThread.CurrentCulture = new CultureInfo("", false);
                int processor = 1;
                foreach (ProcessThread thread in Process.GetCurrentProcess().Threads)
                {
                    thread.ProcessorAffinity = (IntPtr)processor;
                }

                if (!Directory.Exists(ConfigFolder))
                    Directory.CreateDirectory(ConfigFolder);
                if (!Directory.Exists(LogFolder))
                    Directory.CreateDirectory(LogFolder);
                LogUtil.InitializeLogging(Path.Combine(ConfigFolder, "LogConfig.xml"), "DefaultLogConfig.xml", FallbackLogfile);

				WorldEditor worldEditor = new WorldEditor();
                List<string> log = RepositoryClass.Instance.CheckForValidRepository();
                if (log.Count != 0)
                    return;


                if (worldEditor.Quit)
                {
                    return;
                }

                if (args.Length > 0) {
                    ProcessArgs(args, worldEditor);
                }

				worldEditor.Show();

				worldEditor.Start(args);
            }
            catch (Exception ex)
            {
                // try logging the error here first, before Root is disposed of
                LogUtil.ExceptionLog.Error(ex.ToString());
                throw;
            }
        }

        private static void ProcessArgs(string [] args, WorldEditor worldEditor)
		{
			for (int i = 0; i < args.Length; ++i) {
                switch (args[i]) {
				case "--log_paths":
					worldEditor.LogPathGeneration = true;
					break;
                }
            }
        }

    }
}
