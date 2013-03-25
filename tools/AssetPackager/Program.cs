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
using System.IO;
using Multiverse.AssetRepository;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, ControlThread = true)]

namespace AssetPackager
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
            bool runWindow = args.Length == 0 || (args.Length == 2 && args[0] == "--world_name");
            if (runWindow) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new assetPackagerForm(args.Length > 0 ? args[1] : ""));
            }
            else
				ProcessArgs(args);
        }

		// Legal args are:
        // --source_repository             Optional.  If supplied, the arg is the
        //                                 the directory that holds the asset repository
        //                                 from which files will be copied.
        // --set_default_repository        Optional.  If supplied, the arg is the
		//                                 new default repository directory.  Sets the
		//                                 key and exits immediately.
		// --worldassets_file file         Optional, but nearly always
		//                                 supplied.  You can have as
		//                                 many of these as you want.
		// --assetlist_file file           Optional, but if not supplied,
		//                                 worldassets_file must be supplied
		//                                 and vice versa.  Can have more
		//                                 than 1 assetlist_file argument pair
		// --new_asset_repository          Required; the directory that
		//                                 holds the new asset repository
		// --dont_copy_asset_defs          Don't copy the the asset definition
		//                                 files themselves.
		// --world_name name               If present, used to find
		//                                 the optional subdirectory
		//                                 of the source media tree
		//                                 which contains world-specific files to
		//                                 substitute for files referenced by asset
        //                                 definitions
        static void ProcessArgs(string [] args)
		{
            List<string> sourceRepositories = new List<string>();
            List<string> worldAssetsFiles = new List<string>();
			List<string> addonFiles = new List<string>();
			string newAssetRepository = "";
			bool copyAssetDefs = true;
			List<string> defaultRepositories = new List<string>();
			string worldName = "";
            Console.OpenStandardOutput();
            for (int i = 0; i < args.Length; ++i) {
                switch (args[i]) {
				case "--source_repository":
                    sourceRepositories.Add(args[++i]);
                    break;
                case "--worldassets_file":
					worldAssetsFiles.Add(args[++i]);
					break;
				case "--assetlist_file":
					addonFiles.Add(args[++i]);
					break;
				case "--new_asset_repository":
					newAssetRepository = args[++i];
					break;
				case "--dont_copy_asset_defs":
					copyAssetDefs = false;
					break;
				case "--set_default_repository":
					defaultRepositories.Add(args[++i]);
					break;
				case "--world_name":
					worldName = args[++i];
					break;
				default:
					Barf(string.Format("Unrecognized command-line argument '{0}'; exiting", args[i]));
					return;
				}
			}
			if (defaultRepositories.Count == 0 && worldAssetsFiles.Count == 0 && addonFiles.Count == 0) {
				Barf("Neither the worldassets_file argument was supplied, nor any assetlist_file arguments; exiting");
				return;
			}
            if (defaultRepositories.Count == 0 && newAssetRepository == "") {
				Barf("The new_asset_repository command-line argument was not supplied; exiting");
				return;
			}
			foreach (string worldAssetsFile in worldAssetsFiles) {
                if (!File.Exists(worldAssetsFile)) {
                    Barf(string.Format("The world asset file '{0}' does not exist!", worldAssetsFile));
                    return;
                }
            }
            
            try {
				if (defaultRepositories.Count > 0) {
					RepositoryClass.Instance.SetRepositoryDirectoriesInRegistry(defaultRepositories);
                    Console.WriteLine(string.Format("Set default repository to '{0}'", RepositoryClass.Instance.RepositoryDirectoryList));
					return;
				}
				else {
                    if (!Directory.Exists(newAssetRepository))
                        Directory.CreateDirectory(newAssetRepository);
                    List<string> directories = new List<string>();
                    if (sourceRepositories.Count > 0) {
                        directories = sourceRepositories;
                    } else {
                        RepositoryClass.Instance.InitializeRepositoryPath();
                        directories.AddRange(RepositoryClass.Instance.RepositoryDirectoryList);
                        if (worldName != "")
                            directories.Insert(0, Path.Combine(directories[directories.Count - 1], worldName));
                    }
                    RepositoryClass.Instance.InitializeRepository(directories);
					string summary = string.Format("world_name='{0}', source_repositories='{1}', new_asset_repository='{2}', dont_copy_asset_defs={3}",
                        worldName, sourceRepositories, newAssetRepository, !copyAssetDefs);
                    List<string> worldAssetLines = new List<string>();
                    foreach (string worldAssetsFile in worldAssetsFiles) {
                        summary += string.Format(", worldassets_file='{0}'", worldAssetsFile);
                        List<string> lines = (worldAssetsFile == "" ? new List<string>() :
                            RepositoryClass.ReadFileLines(worldAssetsFile.Trim()));
                        worldAssetLines.AddRange(lines);
                    }
                    foreach (string addonFile in addonFiles)
                        summary += string.Format(", assetlist_file='{0}'", addonFile);
                    Console.WriteLine("Command-line arguments: " + summary);
                    RepositoryClass.Instance.GenerateAndCopyMediaTree(worldAssetLines, addonFiles, 
																	  newAssetRepository, copyAssetDefs);
					List<string> log = RepositoryClass.Instance.ErrorLog;
					foreach (string s in log)
						Console.WriteLine(s);
                }
			}
			catch (Exception e) {
                Console.WriteLine(e.Message);
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
         "--source_repository             Optional.  If supplied, the arg is the \n" +
         "                                the directory that holds the asset repository \n" +
         "                                from which files will be copied.\n" +
         "--set_default_repository        Optional.  If supplied, the arg is the \n" +
		 "                                new default repository directory.  Sets the \n" +
		 "                                key and exits immediately.\n" +
         "--worldassets_file file         Optional, but nearly always supplied.  You can have\n" +
         "                                many of these.\n" +
         "--assetlist_file file           Optional, but if not supplied, worldassets_file\n" +
         "                                must be supplied and vice versa.  Can have\n" +
         "                                more than 1 assetlist_file argument pair\n" +
         "--new_asset_repository          Required; the directory that holds the new\n" +
         "                                asset repository\n" +
		 "--dont_copy_asset_defs          Don't copy the the asset definition\n" +
		 "                                files themselves.\n" +
		 " --world_name name              If present, used to find the optional subdirectory\n" + 
		 "                                of the source media tree which contains world-specific files to\n" +
		 "                                substitute for files referenced by asset definitions.");
		}
		
    }
}
