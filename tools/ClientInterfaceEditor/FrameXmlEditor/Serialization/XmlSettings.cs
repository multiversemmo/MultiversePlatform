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

using System.Reflection;
using System.Xml;
using System.IO;
using Microsoft.Win32;
using System;
using System.Windows.Forms;
namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	/// <summary>
	/// Constants and default settings for Xml serialization.
	/// </summary>
	internal static class XmlSettings
	{
        private const string wowPathSubKey = @"SOFTWARE\Blizzard Entertainment\World of Warcraft";
        private const string wowPathName = "InstallPath";
        private const string wowInterfaceDataFolderPattern = "Blizzard Interface Data*";
        private const string xsdRelativePath = @"FrameXML\UI.xsd";

		/// <summary>
		/// Namespace of xml types defined in the Wow schema.
		/// </summary>
		public const string MultiverseNamespace = "http://www.multiverse.net/ui";

		public static XmlReaderSettings CreateReaderSettings()
		{
			var useSchema = true;
			return CreateReaderSettings(ref useSchema);
		}

		public static XmlReaderSettings CreateReaderSettings(ref bool useSchema)
		{
            XmlReaderSettings readerSettings = new XmlReaderSettings();

			if (useSchema)
			{
				string schemaPath = GetSchemaPath();

				useSchema = File.Exists(schemaPath);
				if (useSchema)
				{
					using (Stream stream = new FileStream(schemaPath, FileMode.Open))
					using (XmlReader reader = XmlReader.Create(stream))
					{
						readerSettings.Schemas.Add(XmlSettings.MultiverseNamespace, reader);
					}

					readerSettings.ValidationType = ValidationType.Schema;
				}
			}

			return readerSettings;
		}

        private static string GetSchemaPath()
        {
            string wowPath = GetWowPath();

            string regPath = "";

            string xsdfile = "";
            Registry.GetValue("HKEY_CURRENT_USER\\Software\\Multiverse\\ClientInterfaceDesinger\\EXEPath", regPath, (object)regPath);
            regPath = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));
            if (Directory.Exists(regPath))
            {
                xsdfile = regPath + "\\MultiverseUI.xsd";
            }
            if (File.Exists(xsdfile))
            {
                return xsdfile;
            }

            //string execDir = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));
            //if(String.IsNullOrEmpty(execDir) && Directory.Exists(execDir))
            //{
            //    xsdfile = execDir + "\\MultiverseUI.xsd";
            //    if (File.Exists(xsdfile))
            //    {
            //        return xsdfile;
            //    }
            //}


            //if (!String.IsNullOrEmpty(wowPath) && Directory.Exists(wowPath))
            //{
            //    // Find the folder we are looking for
            //    string[] directories = Directory.GetDirectories(wowPath, wowInterfaceDataFolderPattern, SearchOption.TopDirectoryOnly);

            //    if (directories.Length > 0)
            //    {
            //        // Construct the path to the XSD schema and return it
            //        return Path.Combine(Path.Combine(wowPath, directories[0]), xsdRelativePath);
            //    }
            //}

            return String.Empty;
        }

        private static string GetWowPath()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(wowPathSubKey))
            {
                if (key != null)
                {
                    return (string)key.GetValue(wowPathName, String.Empty);
                }
            }

            return String.Empty;
        }
	}
}
