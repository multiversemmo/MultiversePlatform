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
using System.Xml;
using System.IO;

using Axiom.Core;
using Axiom.FileSystem;

namespace Multiverse.Gui
{
    public class AssetConfig
    {
        List<AssetEntry> assetEntries;

        public struct AssetEntry
        {
            public string archiveType;
            public string archiveSource;
            public string resourceType;
        }

        public AssetConfig()
        {
            assetEntries = new List<AssetEntry>();
        }
        public void ReadXml(string xmlFile)
        {
            Stream stream = new FileStream(xmlFile, FileMode.Open);
            XmlDocument document = new System.Xml.XmlDocument();
            document.Load(stream);
            foreach (XmlNode childNode in document.ChildNodes)
            {
                if (childNode.Name == "AssetConfig")
                    ReadAssetConfig(childNode);
            }
            stream.Close();
        }

        void ReadAssetConfig(XmlNode node)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "AssetPath")
                    ReadAssetPath(childNode);
            }
        }
        void ReadAssetPath(XmlNode node)
        {
            AssetEntry entry = new AssetEntry();
            entry.archiveType = node.Attributes["type"].Value;
            entry.archiveSource = node.Attributes["src"].Value;
            if (node.Attributes["resource"] != null)
                entry.resourceType = node.Attributes["resource"].Value;
            else
                entry.resourceType = "Common";
            assetEntries.Add(entry);
        }

        public List<AssetEntry> AssetEntries
        {
            get
            {
                return assetEntries;
            }
        }
    }

    public class MVResourceManager : ResourceManager
    {
        public override Resource Create(string name, bool isManual)
        {
            throw new NotImplementedException();
        }

        public List<Archive> GetArchives()
        {
            List<Archive> rv = new List<Archive>();
            foreach (Archive archive in archives)
            {
                rv.Add(archive);
            }
            return rv;
        }
    }
    public class AssetManager
    {
        public static AssetManager instance = new AssetManager();

        // Mapping from type to list of archives
        Dictionary<string, MVResourceManager> archiveDict =
            new Dictionary<string, MVResourceManager>();

        public void AddArchive(string assetType, string assetPath, string archiveType)
        {
            if (assetType == "Common")
            {
                ResourceManager.AddCommonArchive(assetPath, archiveType);
                return;
            }
            if (!archiveDict.ContainsKey(assetType))
                archiveDict[assetType] = new MVResourceManager();
            MVResourceManager manager = archiveDict[assetType];
            manager.AddArchive(assetPath, archiveType);
        }

        public void AddArchive(string assetType, List<string> assetPathList, string archiveType)
        {
            if (assetType == "Common")
            {
                ResourceManager.AddCommonArchive(assetPathList, archiveType);
                return;
            }
            if (!archiveDict.ContainsKey(assetType))
                archiveDict[assetType] = new MVResourceManager();
            MVResourceManager manager = archiveDict[assetType];
            manager.AddArchive(assetPathList, archiveType);
        }

        public string ResolveResourceData(string assetType, string fileName)
        {
            if (assetType == "Common")
                throw new NotImplementedException();
            if (!archiveDict.ContainsKey(assetType))
                return null;
            return archiveDict[assetType].ResolveResourceData(fileName);
        }

        public System.IO.Stream FindResourceData(string assetType, string fileName)
        {
            if (assetType == "Common")
                return ResourceManager.FindCommonResourceData(fileName);
            if (!archiveDict.ContainsKey(assetType))
                return null;
            return archiveDict[assetType].FindResourceData(fileName);
        }

        public MVResourceManager GetResourceManager(string assetType)
        {
            if (!archiveDict.ContainsKey(assetType))
                return null;
            return archiveDict[assetType];
        }

        public static AssetManager Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
