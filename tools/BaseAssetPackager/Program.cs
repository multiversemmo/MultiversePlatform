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
using System.Text;
using System.Security.Permissions;
using System.Threading;
using System.Globalization;
using System.IO;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, ControlThread = true)]

namespace BaseAssetPackager {
    class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            // Changes the CurrentCulture of the current thread to the invariant culture.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("", false);
            ProcessArgs(args);
        }
        static void ProcessArgs(string[] args) {
            string worldAssetFile = "";
            List<string> addonFiles = new List<string>();
            string destRepository = "";
            bool copyAssetDefs = true;
            string sourceRepository = "";
            string worldName = "";
            Console.OpenStandardOutput();
            for (int i = 0; i < args.Length; ++i) {
                switch (args[i]) {
                    case "--worldassets_file":
                        worldAssetFile = args[++i];
                        break;
                    case "--dest_repository":
                        destRepository = args[++i];
                        break;
                    case "--assetlist_file":
                        addonFiles.Add(args[++i]);
                        break;
                    case "--dont_copy_asset_defs":
                        copyAssetDefs = false;
                        break;
                    case "--source_repository":
                        sourceRepository = args[++i];
                        break;
                    case "--world_name":
                        worldName = args[++i];
                        break;
                    default:
                        Barf(string.Format("Unrecognized command-line argument '{0}'; exiting", args[i]));
                        return;
                }
            }
            if (sourceRepository == "") {
                Barf("The source repository must be specified");
                return;
            }
            if (worldAssetFile == "" && addonFiles.Count == 0) {
                Barf("Neither the worldassets_file argument was supplied, nor any assetlist_file arguments; exiting");
                return;
            }
            if (destRepository == "") {
                Barf("The dest_repository command-line argument was not supplied; exiting");
                return;
            }
            if (!File.Exists(worldAssetFile)) {
                Barf(string.Format("The world asset file '{0}' does not exist!", worldAssetFile));
                return;
            }

            try {
                if (sourceRepository != "") {
                    if (!Directory.Exists(destRepository))
                        Directory.CreateDirectory(destRepository);
                    List<string> errors = BaseRepositoryClass.Instance.InitializeRepository(sourceRepository, worldName);
                    if (errors.Count != 0) {
                        foreach (string s in errors)
                            Console.WriteLine(s);
                    }
                    List<string> worldAssetLines = BaseRepositoryClass.ReadFileLines(worldAssetFile.Trim());
                    BaseRepositoryClass.Instance.GenerateAndCopyMediaTree(worldName, worldAssetLines, addonFiles,
                                                                          destRepository, copyAssetDefs);
                    List<string> log = BaseRepositoryClass.Instance.ErrorLog;
                    foreach (string s in log)
                        Console.WriteLine(s);
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        static void Barf(string message) {
            Usage();
            Console.WriteLine(message);
        }

        static void Usage() {
            Console.WriteLine(
         "--source_repository             Required; the directory that holds the source\n" +
         "                                asset repository\n" +
         "--dest_repository               Required; the directory that holds the new\n" +
         "                                asset repository\n" +
         "--worldassets_file file         Optional, but nearly always supplied.\n" +
         "--assetlist_file file           Optional, but if not supplied, worldassets_file\n" +
         "                                must be supplied and vice versa.  Can have\n" +
         "                                more than 1 assetlist_file argument pair\n" +
         "--dont_copy_asset_defs          Don't copy the the asset definition\n" +
         "                                files themselves.\n" +
         "--world_name name               If present, used to find the optional subdirectory\n" +
         "                                of the source media tree which contains world-specific files to\n" +
         "                                substitute for files referenced by asset definitions.");
        }
		
    }
}
