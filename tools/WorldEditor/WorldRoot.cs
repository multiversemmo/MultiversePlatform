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
using System.Windows.Forms;
using System.Xml;
using System.ComponentModel;
using System.IO;
using Axiom.MathLib;
using Axiom.Core;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    public class WorldRoot : IWorldObject, IWorldContainer, IObjectCollectionParent
    {
        protected String name;
        protected static WorldRoot instance = null;
        protected MultiSelectTreeView treeView;
        protected WorldTreeNode node = null;
        protected WorldTreeNode terrainNode = null;
        protected WorldEditor app;

        protected WorldTerrain worldTerrain;
        protected Ocean ocean;
        protected Skybox skybox;
        protected GlobalFog fog = null;
        protected GlobalAmbientLight ambientLight = null;
        protected GlobalDirectionalLight directionalLight = null;
        protected PathObjectTypeContainer pathObjectTypes = null;
        protected List<WorldObjectCollection> worldCollections;

        protected bool inTree = false;
        protected bool inScene = false;

        protected string worldFilePath = null;

        protected Vector3 cameraPosition = Vector3.Zero;
        protected Quaternion cameraOrientation = Quaternion.Identity;

        protected List<ToolStripButton> buttonBar;
        protected string path;
        protected bool loadCollections;

        public WorldRoot(String worldName, MultiSelectTreeView tree, WorldEditor worldEditor)
        {
            instance = this;
            name = worldName;
            treeView = tree;
            app = worldEditor;

            worldTerrain = new WorldTerrain(app);
            ocean = new Ocean(this, app);
            skybox = new Skybox(this, app);
            fog = new GlobalFog(this, app);
            ambientLight = new GlobalAmbientLight(this, app);
            directionalLight = new GlobalDirectionalLight(this, app);
            worldCollections = new List<WorldObjectCollection>();
            pathObjectTypes = new PathObjectTypeContainer(this, app);
        }

        public WorldRoot(XmlReader r, String worldFilename, MultiSelectTreeView tree, WorldEditor worldEditor, bool loadCollections)
        {
            instance = this;
            treeView = tree;
            app = worldEditor;
            worldFilePath = worldFilename;
            this.loadCollections = loadCollections;

            worldCollections = new List<WorldObjectCollection>();

            if (loadCollections)
            {
                FromXml(r);
            }
            else
            {
                FromXml(r, loadCollections);
            }

            // if the XML doesn't have ocean in it, then add it here
            if (ocean == null)
            {
                ocean = new Ocean(this, app);
            }
            if (skybox == null)
            {
                skybox = new Skybox(this, app);
            }
            if (fog == null)
            {
                fog = new GlobalFog(this, app);
            }
            if (ambientLight == null)
            {
                ambientLight = new GlobalAmbientLight(this, app);
            }
            if (directionalLight == null)
            {
                directionalLight = new GlobalDirectionalLight(this, app);
            }
            if (pathObjectTypes == null)
            {
                pathObjectTypes = new PathObjectTypeContainer(this, app);
            }
        }

        [DescriptionAttribute("The name of this world."), CategoryAttribute("Miscellaneous")]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                UpdateNode();
            }
        }

        [BrowsableAttribute(false)]
        public static WorldRoot Instance
        {
            get
            {
                return instance;
            }
        }
        
        protected void UpdateNode()
        {
            if (inTree)
            {
                node.Text = NodeName;
            }
        }

        [BrowsableAttribute(false)]
        protected string NodeName
        {
            get
            {
                string ret;
                if (app.Config.ShowTypeLabelsInTreeView)
                {
                    ret = string.Format("{0}: {1}", ObjectType, name);
                }
                else
                {
                    ret = name;
                }

                return ret;
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 CameraPosition
        {
            get
            {
                return cameraPosition;
            }
            set
            {
                cameraPosition = value;
            }
        }

        [BrowsableAttribute(false)]
        public Quaternion CameraOrientation
        {
            get
            {
                return cameraOrientation;
            }
            set
            {
                cameraOrientation = value;
            }
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "World";
            }
        }

        [BrowsableAttribute(false)]
        public WorldTerrain Terrain
        {
            get
            {
                return worldTerrain;
            }
        }

        [BrowsableAttribute(false)]
        public bool IsGlobal
        {
            get
            {
                return true;
            }
        }

        [BrowsableAttribute(false)]
        public bool IsTopLevel
        {
            get
            {
                return false;
            }
        }

        [BrowsableAttribute(false)]
        public bool AcceptObjectPlacement
        {
            get
            {
                return false;
            }
            set
            {
                //not implemented for this type of object
            }
        }

        public void Clone(IWorldContainer copyParent)
        {
        }

        protected void FromXml(XmlReader r)
        {
            string filename="";
            string baseName = worldFilePath.Substring(0, worldFilePath.LastIndexOf('\\'));
            

            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                switch (r.Name)
                {
                    case "Name":
                        name = r.Value;
                        break;
                }
            }
            r.MoveToElement();

            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if (r.NodeType == XmlNodeType.Element)
                {
                    switch (r.Name)
                    {
                        case "CameraPosition":
                            cameraPosition = XmlHelperClass.ParseVectorAttributes(r);
                            break;
                        case "CameraOrientation":
                            cameraOrientation = XmlHelperClass.ParseQuaternion(r);
                            break;
                        case "Terrain":
                            worldTerrain = new WorldTerrain(app, r);
                            break;
                        case "TerrainDisplay":
                            worldTerrain.DisplayParamsFromXml(r);
                            break;
                        case "Ocean":
                            ocean = new Ocean(this, app, r);
                            break;
                        case "Skybox":
                            skybox = new Skybox(this, app, r);
                            break;
                        case "GlobalFog":
                            fog = new GlobalFog(this, app, r);
                            break;
                        case "GlobalAmbientLight":
                            ambientLight = new GlobalAmbientLight(this, app, app.Scene, r);
                            break;
                        case "GlobalDirectionalLight":
                            directionalLight = new GlobalDirectionalLight(this, app, r);
                            break;
                        case "PathObjectTypes":
                            pathObjectTypes = new PathObjectTypeContainer(app, this, r);
                            break;
                        case "WorldCollection":
                            string collectionName = null;
                            filename = "";
                            for (int i = 0; i < r.AttributeCount; i++)
                            {
                                r.MoveToAttribute(i);
                                switch (r.Name)
                                {
                                    case "Name":
                                        collectionName = r.Value;
                                        break;
                                    case "Filename":
                                        filename = r.Value;
                                        break;
                                }
                            }
                            if (filename != "")
                            {
                                string filepath = String.Format("{0}\\{1}", baseName, filename);
                                if (filename.EndsWith("~.mwc"))
                                {
                                    string autofilepath = String.Format("{0}\\{1}", baseName, filename);
                                    string normalfilepath = String.Format("{0}\\{1}", baseName, filename.Remove(filename.LastIndexOf("~"), 1));
                                    if ((File.Exists(autofilepath) && File.Exists(normalfilepath) &&
                                        (new FileInfo(autofilepath)).LastWriteTime < (new FileInfo(normalfilepath).LastWriteTime))
                                        || (!File.Exists(autofilepath) && File.Exists(normalfilepath)))
                                    {
                                        filename = filename.Remove(filename.LastIndexOf("~"), 1);
                                        filepath = normalfilepath;
                                    }
                                    else
                                    {
                                        filepath = autofilepath;
                                    }
                                }
                                XmlReader childReader = XmlReader.Create(filepath, app.XMLReaderSettings);
                                WorldObjectCollection collection = new WorldObjectCollection(childReader, collectionName, this, app, baseName);
                                collection.Filename = filename;
                                while(collection.Filename.Contains("~"))
                                {
                                    collection.Filename = collection.Filename.Remove(collection.Filename.LastIndexOf("~"), 1);
                                }
                                Add(collection);
                                childReader.Close();
                            }
                            else
                            {
                                XmlReader childReader = XmlReader.Create(String.Format("{0}\\{1}.mwc", baseName, collectionName), app.XMLReaderSettings);
                                WorldObjectCollection collection = new WorldObjectCollection(childReader, collectionName, this, app, baseName);
                                collection.Filename = filename;
                                while(collection.Filename.Contains("~"))
                                {
                                    collection.Filename = collection.Filename.Remove(collection.Filename.LastIndexOf("~"), 1);
                                }
                                Add(collection);
                                childReader.Close();
                            }

                            r.MoveToElement();

                            break;
                    }
                }
            }
        }

        protected void FromXml(XmlReader r, bool loadCollections)
        {
            string filename = "";
            string baseName = worldFilePath.Substring(0, worldFilePath.LastIndexOf('\\'));
            bool loadColl = loadCollections;


            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                switch (r.Name)
                {
                    case "Name":
                        name = r.Value;
                        break;
                }
            }
            r.MoveToElement();

            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if (r.NodeType == XmlNodeType.Element)
                {
                    switch (r.Name)
                    {
                        case "CameraPosition":
                            cameraPosition = XmlHelperClass.ParseVectorAttributes(r);
                            break;
                        case "CameraOrientation":
                            cameraOrientation = XmlHelperClass.ParseQuaternion(r);
                            break;
                        case "Terrain":
                            worldTerrain = new WorldTerrain(app, r);
                            break;
                        case "TerrainDisplay":
                            worldTerrain.DisplayParamsFromXml(r);
                            break;
                        case "Ocean":
                            ocean = new Ocean(this, app, r);
                            break;
                        case "Skybox":
                            skybox = new Skybox(this, app, r);
                            break;
                        case "GlobalFog":
                            fog = new GlobalFog(this, app, r);
                            break;
                        case "GlobalAmbientLight":
                            ambientLight = new GlobalAmbientLight(this, app, app.Scene, r);
                            break;
                        case "GlobalDirectionalLight":
                            directionalLight = new GlobalDirectionalLight(this, app, r);
                            break;
                        case "PathObjectTypes":
                            pathObjectTypes = new PathObjectTypeContainer(app, this, r);
                            break;
                        case "WorldCollection":
                            string collectionName = null;
                            filename = "";
                            for (int i = 0; i < r.AttributeCount; i++)
                            {
                                r.MoveToAttribute(i);
                                switch (r.Name)
                                {
                                    case "Name":
                                        collectionName = r.Value;
                                        break;
                                    case "Filename":
                                        filename = r.Value;
                                        break;
                                }
                            }
                            string filepath = String.Format("{0}\\{1}", baseName, filename);
                            if (filename != "")
                            {
                                if (filename.EndsWith("~.mwc"))
                                {
                                    string autofilepath = String.Format("{0}\\{1}", baseName, filename);
                                    string normalfilepath = String.Format("{0}\\{1}", baseName, filename.Remove(filename.LastIndexOf("~"), 1));
                                    if ((File.Exists(autofilepath) && File.Exists(normalfilepath) &&
                                        (new FileInfo(autofilepath)).LastWriteTime < (new FileInfo(normalfilepath).LastWriteTime))
                                        || (!File.Exists(autofilepath) && File.Exists(normalfilepath)))
                                    {
                                        filename = filename.Remove(filename.LastIndexOf("~"), 1);
                                        filepath = normalfilepath;
                                    }
                                    else
                                    {
                                        filepath = autofilepath;
                                    }
                                }
                                XmlReader childReader = XmlReader.Create(filepath, app.XMLReaderSettings);
                                WorldObjectCollection collection = new WorldObjectCollection(childReader, collectionName, this, app, baseName, loadColl);
                                collection.Filename = filename;
                                while (collection.Filename.Contains("~"))
                                {
                                    collection.Filename = collection.Filename.Remove(collection.Filename.LastIndexOf("~"), 1);
                                }
                                Add(collection);
                                childReader.Close();

                            }
                            else
                            {
                                XmlReader childReader = XmlReader.Create(String.Format("{0}\\{1}.mwc", baseName, collectionName), app.XMLReaderSettings);
                                WorldObjectCollection collection = new WorldObjectCollection(childReader, collectionName, this, app, baseName, loadColl);
                                collection.Filename = filename;
                                Add(collection);
                                while (collection.Filename.Contains("~"))
                                {
                                    collection.Filename = collection.Filename.Remove(collection.Filename.LastIndexOf("~"), 1);
                                }
                                childReader.Close();
                            }

                            r.MoveToElement();

                            break;
                    }
                }
            }
        }



        #region IObjectCollectionParent
        [BrowsableAttribute(false)]
        public string WorldFilePath
        {
            get
            {
                return worldFilePath;
            }
            set
            {
                worldFilePath = value;
                string temp = worldFilePath.Substring(0, worldFilePath.LastIndexOf("\\") + 1);
                Path = temp;
            }
        }


        [BrowsableAttribute(false)]
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
            }

        }

        [BrowsableAttribute(false)]
        public List<WorldObjectCollection> CollectionList
        {
            get
            {
                List<WorldObjectCollection>list = new List<WorldObjectCollection>();
                foreach (WorldObjectCollection col in worldCollections)
                {
                    list.Add(col);
                }
                return list;
            }
        }

        #endregion IObjectCollectionParent

        [BrowsableAttribute(false)]
        public PathObjectTypeContainer PathObjectTypes
        {
            get
            {
                return pathObjectTypes;
            }
        }
        

        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {            
            // add the world node
            node = app.MakeTreeNode(this, NodeName);
            treeView.Nodes.Add(node);

            // build the menu
            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Create Object Collection", new CreateWorldCollectionCommandFactory(app,((IObjectCollectionParent) this)), app.DefaultCommandClickHandler);
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "World_Root", app.HelpClickHandler);
            node.ContextMenuStrip = menuBuilder.Menu;

            // traverse children
            worldTerrain.AddToTree(node);
            ocean.AddToTree(node);
            skybox.AddToTree(node);
            fog.AddToTree(node);
            ambientLight.AddToTree(node);
            directionalLight.AddToTree(node);

//             pathObjectTypes.AddToTree(node);
            foreach (WorldObjectCollection child in worldCollections)
            {
                child.AddToTree(node);
            }

            inTree = true;
            buttonBar = menuBuilder.ButtonBar;
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", NodeName);
                objString +=  "\r\n";
                return objString;
            }
        }

        [BrowsableAttribute(false)]
        public List<ToolStripButton> ButtonBar
        {
            get
            {
                return buttonBar;
            }
        }

        public void RemoveFromTree()
        {
            if (node.IsSelected)
            {
                node.UnSelect();
            }
            worldTerrain.RemoveFromTree();
            ocean.RemoveFromTree();
            skybox.RemoveFromTree();
            pathObjectTypes.RemoveFromTree();
            foreach (WorldObjectCollection child in worldCollections)
            {
                child.RemoveFromTree();
            }

            treeView.Nodes.Remove(node);
            node = null;

            inTree = false;
        }

        public void AddToScene()
        {
            inScene = true;

            worldTerrain.AddToScene();
            ocean.AddToScene();
            skybox.AddToScene();
            fog.AddToScene();
            ambientLight.AddToScene();
            directionalLight.AddToScene();


            // Iterate all children and have them add themselves to the tree
            foreach (IWorldObject child in worldCollections)
            {
                child.AddToScene();
            }
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            foreach (WorldObjectCollection objCol in this.worldCollections)
            {
                objCol.UpdateScene(type, hint);
            }
        }

        public void RemoveFromScene()
        {
            inScene = false;

            worldTerrain.RemoveFromScene();
            ocean.RemoveFromScene();
            skybox.RemoveFromScene();

            // Iterate all children and have them add themselves to the tree
            foreach (IWorldObject child in worldCollections)
            {
                child.RemoveFromScene();
            }
        }

        public void RemoveCollectionsFromScene()
        {
            foreach (IWorldObject child in worldCollections)
            {
                child.RemoveFromScene();
            }
        }


        public void CheckAssets()
        {
            worldTerrain.CheckAssets();
            ocean.CheckAssets();
            skybox.CheckAssets();
            fog.CheckAssets();
            ambientLight.CheckAssets();
            directionalLight.CheckAssets();

            foreach (IWorldObject child in worldCollections)
            {
                child.CheckAssets();
            }
        }

		public void ToXml(XmlWriter w)
        {
            int pathEnd = worldFilePath.LastIndexOf('\\');
            string pathName = worldFilePath.Substring(0, pathEnd);
            
            w.WriteStartElement("World");
            w.WriteAttributeString("Name", name);
            w.WriteAttributeString("Version", app.Config.XmlSaveFileVersion.ToString());

            w.WriteStartElement("CameraPosition");
            w.WriteAttributeString("x", cameraPosition.x.ToString());
            w.WriteAttributeString("y", cameraPosition.y.ToString());
            w.WriteAttributeString("z", cameraPosition.z.ToString());
            w.WriteEndElement();

            w.WriteStartElement("CameraOrientation");
            w.WriteAttributeString("x", cameraOrientation.x.ToString());
            w.WriteAttributeString("y", cameraOrientation.y.ToString());
            w.WriteAttributeString("z", cameraOrientation.z.ToString());
            w.WriteAttributeString("w", cameraOrientation.w.ToString());
            w.WriteEndElement();

            worldTerrain.ToXml(w);
            ocean.ToXml(w);
            skybox.ToXml(w);
            fog.ToXml(w);
            ambientLight.ToXml(w);
            directionalLight.ToXml(w);
            pathObjectTypes.ToXml(w);
            
            foreach (WorldObjectCollection worldCollection in worldCollections)
            {
                // write the XML into the top level world file
                w.WriteStartElement("WorldCollection");
                worldCollection.Path = pathName;
                if (worldCollection.Filename == "")
                {
                    int startIndex = pathEnd + 1;
                    worldCollection.Path = pathName;
                    int subStringLength = worldFilePath.LastIndexOf(".mvw") - startIndex + 4;
                    worldCollection.Filename = String.Format("{0}-{1}.mwc", worldFilePath.Substring(startIndex,
                        subStringLength), worldCollection.Name);
                }
                w.WriteAttributeString("Name", worldCollection.Name);
                w.WriteAttributeString("Filename", worldCollection.Filename);
                w.WriteEndElement();

                // create a file for the collection
                if (worldCollection.Loaded)
                {
                    try
                    {
                        XmlWriter childWriter = XmlWriter.Create(String.Format("{0}\\{1}", pathName,
                            worldCollection.Filename), app.XMLWriterSettings);
                        worldCollection.ToXml(childWriter);
                        childWriter.Close();
                    }
                    catch (Exception e)
                    {
                        LogManager.Instance.Write(e.ToString());
                        MessageBox.Show(String.Format("Unable to open the file for writing.  Use file menu \"Save As\" to save to another location.  Error: {0}", e.Message), "Error Saving File", MessageBoxButtons.OK);
                    }
                }
            }
            w.WriteEndElement();
        }

        public void ToXml(XmlWriter w, bool backup)
        {
            if (!backup)
            {
                ToXml(w);
            }
            int pathEnd = worldFilePath.LastIndexOf('\\');
            string pathName = worldFilePath.Substring(0, pathEnd);

            w.WriteStartElement("World");
            w.WriteAttributeString("Name", name);
            w.WriteAttributeString("Version", app.Config.XmlSaveFileVersion.ToString());

            w.WriteStartElement("CameraPosition");
            w.WriteAttributeString("x", cameraPosition.x.ToString());
            w.WriteAttributeString("y", cameraPosition.y.ToString());
            w.WriteAttributeString("z", cameraPosition.z.ToString());
            w.WriteEndElement();

            w.WriteStartElement("CameraOrientation");
            w.WriteAttributeString("x", cameraOrientation.x.ToString());
            w.WriteAttributeString("y", cameraOrientation.y.ToString());
            w.WriteAttributeString("z", cameraOrientation.z.ToString());
            w.WriteAttributeString("w", cameraOrientation.w.ToString());
            w.WriteEndElement();

            worldTerrain.ToXml(w);
            ocean.ToXml(w);
            skybox.ToXml(w);
            fog.ToXml(w);
            ambientLight.ToXml(w);
            directionalLight.ToXml(w);
            pathObjectTypes.ToXml(w);

            foreach (WorldObjectCollection worldCollection in worldCollections)
            {
                if (worldCollection.Filename == "")
                {
                    int startIndex = pathEnd + 1;
                    int subStringLength = worldFilePath.LastIndexOf(".mvw") - startIndex + 4;
                    worldCollection.Filename = String.Format("{0}-{1}.mwc", worldFilePath.Substring(startIndex,
                        subStringLength), worldCollection.Name);
                }
                // write the XML into the top level world file
                w.WriteStartElement("WorldCollection");
                w.WriteAttributeString("Name", worldCollection.Name);
                string filename = worldCollection.Filename.Insert((worldCollection.Filename.LastIndexOf(".")),"~");
                w.WriteAttributeString("Filename", filename);
                w.WriteEndElement();

                // create a file for the collection
                if (worldCollection.Loaded)
                {
                    try
                    {
                        XmlWriter childWriter = XmlWriter.Create(String.Format("{0}\\{1}", pathName,
                            filename), app.XMLWriterSettings);
                        worldCollection.ToXml(childWriter, backup);
                        childWriter.Close();
                    }
                    catch (Exception e)
                    {
                        LogManager.Instance.Write(e.ToString());
                        MessageBox.Show(String.Format("Unable to open the file for writing.  Use file menu \"Save As\" to save to another location.  Error: {0}", e.Message), "Error Saving File", MessageBoxButtons.OK);
                    }
                }
                int index = worldCollection.Filename.LastIndexOf('~');
                if (index > 0)
                {
                    worldCollection.Filename = worldCollection.Filename.Remove(worldCollection.Filename.LastIndexOf('~'),1);
                }
            }
            w.WriteEndElement();
        }


        [BrowsableAttribute(false)]
        public bool WorldViewSelectable
        {
            get
            {
                return false;
            }
            set
            {
                // this property is not applicable to this object
            }
        }


        [BrowsableAttribute(false)]
        public Vector3 FocusLocation
        {
            get
            {
                Vector3 v = Vector3.Zero;

                if (worldCollections.Count != 0)
                {
                    foreach (WorldObjectCollection worldCollection in worldCollections)
                    {
                        v += worldCollection.FocusLocation;
                    }

                    v = v / worldCollections.Count;
                }

                return v;
            }
        }

        [BrowsableAttribute(false)]
        public bool Highlight
        {
            get
            {
                return false;
            }
            set
            {
                // do nothing
            }
        }

		[BrowsableAttribute(false)]
		public WorldTreeNode Node
		{
			get
			{
				return node;
			}
		}


        public void ToManifest(System.IO.StreamWriter w)
        {
            if (worldCollections != null)
            {
                foreach (IWorldObject child in worldCollections)
                {
                    child.ToManifest(w);
                }
            }

            // traverse children
            worldTerrain.ToManifest(w);
            ocean.ToManifest(w);
            skybox.ToManifest(w);
            fog.ToManifest(w);
        }

        #endregion

        [BrowsableAttribute(false)]
        public List<WorldObjectCollection> WorldObjectCollections
        {
            get
            {
                return this.worldCollections;
            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            worldCollections.Clear();
        }

        #endregion

        #region ICollection<IWorldObject> Members

        public void Add(IWorldObject item)
        {
            worldCollections.Add(item as WorldObjectCollection);
            if (inTree)
            {
                item.AddToTree(node);
            }
            if (inScene)
            {
                item.AddToScene();
            }
        }

        public void Clear()
        {
            worldCollections.Clear();
        }

        public bool Contains(IWorldObject item)
        {
            return worldCollections.Contains(item as WorldObjectCollection);
        }

        public void CopyTo(IWorldObject[] array, int arrayIndex)
        {
            worldCollections.CopyTo(array as WorldObjectCollection[], arrayIndex);
        }

        [BrowsableAttribute(false)]
        public bool DisplayOcean
        {
            get
            {
                return ocean.DisplayOcean;
            }
        }

        [BrowsableAttribute(false)]
        public int Count
        {
            get
            {
                return worldCollections.Count;
            }
        }

        [BrowsableAttribute(false)]
        public bool IsReadOnly
        {
            get 
            {
                return false;
            }
        }

        public bool Remove(IWorldObject item)
        {

            if (inTree)
            {
                item.RemoveFromTree();
            }
            if (inScene)
            {
                item.RemoveFromScene();
            }
            return worldCollections.Remove(item as WorldObjectCollection);
        }

        #endregion

        #region IEnumerable<IWorldObject> Members

        public IEnumerator<IWorldObject> GetEnumerator()
        {

            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
