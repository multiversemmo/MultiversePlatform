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
using Multiverse.AssetRepository;
using System.Security.Permissions;
using System.Threading;
using System.Globalization;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, ControlThread = true)]

namespace AssetImporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Changes the CurrentCulture of the current thread to the invariant culture.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("", false);
            if (args.Length > 0) {
				ProcessArgs(args);
				return;
			}
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Importer());
        }

		// Legal args are:
		// --generate_asset_definitions      Optional; says to generate the definitions
		// --generate_asset_kind kind        Optional; says only generate this kind of asset
		// --generate_asset_definitions      Required; says to generate the defs 
		// --repository_directory directory  Required if the registry key for the 
		//                                   repository is not set.
		// --output_file outputfile          File containing warnings produced by
		//                                   the asset generation process.
		static void ProcessArgs(string [] args)
		{
            bool generateAssetDefinitions = false;
			string generateAssetKind = "";
			string repositoryDirectory = "";
			string outputFile = "";

			for (int i = 0; i < args.Length; ++i) {
                switch (args[i]) {
				case "--generate_asset_definitions":
					generateAssetDefinitions = true;
					break;
				case "--generate_asset_kind":
					generateAssetKind = args[++i];
					break;
				case "--repository_directory":
					repositoryDirectory = args[++i];
					break;
				case "--output_file":
					outputFile = args[++i];
					break;
				default:
					Barf(string.Format("Unrecognized command-line argument '{0}'; exiting", args[i]));
					return;
				}
			}
            if (generateAssetDefinitions || generateAssetKind != "")
            {
                List<string> log;
                if (repositoryDirectory != "") {
                    List<string> directories = new List<string>();
                    directories.Add(repositoryDirectory);
                    log = RepositoryClass.Instance.InitializeRepository(directories);
                } else
                    log = RepositoryClass.Instance.InitializeRepository();
                CheckLogAndMaybeExit(log);
				RepositoryClass.Instance.BuildAllAssetDefinitions(outputFile, generateAssetKind);
            }
		}
		
		private static void CheckLogAndMaybeExit(List<string> log)
		{
            if (log.Count > 0) {
                string lines = "";
                foreach (string s in log)
                    lines += s + "\n";
                if (MessageBox.Show("Error(s) initializing asset repository:\n" + lines,
                                    "Errors Initializing Asset Repository.  Click Cancel To Exit",
                                    MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    Environment.Exit(-1);
            }
		}
		
		static void Barf(string message)
		{
			Usage();
			Console.WriteLine(message);
		}
		
		static void Usage()
		{
			Console.WriteLine(
		"--generate_asset_definitions      Optional; says to generate the definitions\n" +
		"--generate_asset_kind kind        Optional; says only generate this kind of asset\n" + 
		"--repository_directory directory  Required if the registry key for the\n" +
        "                                  repository is not set.\n" +
		"--output_file outputfile          File containing warnings produced by\n" +
		"                                  the asset generation process.");
		}
    }
}
