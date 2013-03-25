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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.ComponentModel;
using System.IO;
using Axiom.MathLib;
using Axiom.Core;

namespace Multiverse.Tools.WorldEditor
{
    public class WorldObjectCollection : IWorldObject, IWorldContainer, IObjectCollectionParent, IObjectChangeCollection, IObjectDelete
    {
        protected List<IWorldObject> objectList;
        protected String name;
        protected String filename = "";
        protected IWorldContainer parent;
        protected WorldTreeNode node = null;
        protected WorldEditor app;
        protected bool inTree = false;
        protected bool inScene = false;
        protected WorldTreeNode parentNode;
        protected List<ToolStripButton> buttonBar;
        protected List<WorldObjectCollection> worldCollections = new List<WorldObjectCollection>();
        protected String path;
        protected bool loaded;

        public WorldObjectCollection(String collectionName, IWorldContainer parentContainer, WorldEditor worldEditor)
        {
            name = collectionName;
            parent = parentContainer;
            objectList = new List<IWorldObject>();
            app = worldEditor;
            loaded = true;
        }

        public WorldObjectCollection(XmlReader r, String collectionName, String collectionFilename, IWorldContainer parentContainer, WorldEditor worldEditor, string path, bool load)
        {
            name = collectionName;
            filename = collectionFilename;
            parent = parentContainer;
            objectList = new List<IWorldObject>();
            app = worldEditor;
            this.path = path;
            if (load)
            {
                FromXml(r, false);
                Loaded = true;
            }
        }


        public WorldObjectCollection(XmlReader r, String collectionName, String collectionFilename, IWorldContainer parentContainer, WorldEditor worldEditor, string path)
        {
            name = collectionName;
            filename = collectionFilename;
            parent = parentContainer;
            objectList = new List<IWorldObject>();
            app = worldEditor;
            this.path = path;
            FromXml(r);
            Loaded = true;
        }

        public WorldObjectCollection(XmlReader r, String collectionName, IWorldContainer parentContainer,  WorldEditor worldEditor, string path, bool load)
        {
            name = collectionName;
            parent = parentContainer;
            objectList = new List<IWorldObject>();
            app = worldEditor;
            this.path = path;
            if (load)
            {
                FromXml(r, false);
                Loaded = true;
            }
        }

        public WorldObjectCollection(XmlReader r, String collectionName, IWorldContainer parentContainer, WorldEditor worldEditor, string path)
        {
            name = collectionName;
            parent = parentContainer;
            objectList = new List<IWorldObject>();
            app = worldEditor;
            this.path = path;
            FromXml(r);
            Loaded = true;
        }

        public void LoadCollection(XmlReader r)
        {
            Loaded = true;
            FromXml(r, false);
        }

        public void LoadAll(XmlReader r)
        {
            FromXml(r);
            Loaded = true;
        }

        public void UnloadCollection()
        {
            Loaded = false;
            int count = objectList.Count;
            for(int i = count - 1; i >= 0; i--)
            {
                if (objectList[i] is WorldObjectCollection)
                {
                    (objectList[i] as WorldObjectCollection).UnloadCollection();
                    this.Remove(objectList[i]);
                }
                else
                {
                    this.Remove(objectList[i]);
                }
            }
        }

        [BrowsableAttribute(false)]
        public bool Loaded
        {
            get
            {
                return loaded;
            }
            set
            {
                if (value != loaded)
                {
                    loaded = value;
                    if (inTree)
                    {
                        buildMenu(this.node);
                    }
                }
            }
        }

        [BrowsableAttribute(false)]
        public bool InScene
        {
            get
            {
                return inScene;
            }
        }

        protected void FromXml(XmlReader r)
        {
            string colfilename = "";
            string baseName = WorldFilePath.Substring(0, WorldFilePath.LastIndexOf('\\'));
            do
            {
                r.Read();
            } while ((r.NodeType != XmlNodeType.Element) || !(String.Equals(r.Name, "WorldObjectCollection")));

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
                        case "Road":
                            RoadObject road = new RoadObject(r, this, app);
                            Add(road);
                            break;

                        case "StaticObject":
                            StaticObject obj = new StaticObject(this, app, r);
                            Add(obj);
                            break;
                        case "Waypoint":
                            Waypoint wp = new Waypoint(r, this, app);
                            Add(wp);
                            break;
                        case "Boundary":
                            Boundary b = new Boundary(r, this, app);
                            Add(b);
                            break;
                        case "PointLight":
                            PointLight pl = new PointLight(app, this, app.Scene, r);
                            Add(pl);
                            break;
                        case "TerrainDecal":
                            TerrainDecal d = new TerrainDecal(app, this, r);
                            Add(d);
                            break;
                        case "WorldCollection":
                            string collectionName = null;
                            colfilename = "";
                            for (int i = 0; i < r.AttributeCount; i++)
                            {
                                r.MoveToAttribute(i);
                                switch (r.Name)
                                {
                                    case "Name":
                                        collectionName = r.Value;
                                        break;
                                    case "Filename":
                                        colfilename = r.Value;
                                        break;
                                }
                            }
                            baseName = this.Path;

                            if (colfilename != "")
                            {
                                string filepath = String.Format("{0}\\{1}", baseName, colfilename);
                                if (colfilename.EndsWith("~.mwc"))
                                {
                                    string autofilepath = String.Format("{0}\\{1}", baseName, colfilename);
                                    string normalfilepath = String.Format("{0}\\{1}", baseName, colfilename.Remove(colfilename.LastIndexOf("~"), 1));
                                    bool autofilepathExists = File.Exists(autofilepath);
                                    bool normalfilepathExists = File.Exists(normalfilepath);
                                    DateTime autoFileTime = (new FileInfo(autofilepath)).LastWriteTime;
                                    DateTime normalFileTime = (new FileInfo(normalfilepath)).LastWriteTime;
                                    bool normalNewer = autoFileTime < normalFileTime;
                                    if ((File.Exists(autofilepath) && File.Exists(normalfilepath) &&
                                        (new FileInfo(autofilepath)).LastWriteTime < (new FileInfo(normalfilepath).LastWriteTime))
                                        || (!File.Exists(autofilepath) && File.Exists(normalfilepath)))
                                    {
                                        colfilename = colfilename.Remove(colfilename.LastIndexOf("~"), 1);
                                        filepath = String.Format("{0}\\{1}", baseName, colfilename);
                                    }
                                    else
                                    {
                                        filepath = autofilepath;
                                    }
                                }
                                XmlReader childReader = XmlReader.Create(String.Format("{0}\\{1}", baseName, colfilename), app.XMLReaderSettings);
                                WorldObjectCollection col = new WorldObjectCollection(childReader, collectionName, this, app, baseName);
                                while(colfilename.Contains("~"))
                                {
                                    colfilename = colfilename.Remove(colfilename.IndexOf("~"), 1);
                                }
                                col.Filename = colfilename;
                                Add(col);
                                childReader.Close();
                            }
                            else
                            {
                                XmlReader childReader = XmlReader.Create(String.Format("{0}\\{1}.mwc", baseName, collectionName), app.XMLReaderSettings);
                                WorldObjectCollection col = new WorldObjectCollection(childReader, collectionName, this, app, baseName);
                                col.Filename = collectionName + ".mwc";
                                while(colfilename.Contains("~"))
                                {
                                    colfilename = colfilename.Remove(colfilename.IndexOf("~"), 1);
                                }
                                Add(col);
                                childReader.Close();
                            }
                            r.MoveToElement();
                            break;
                    }
                }
            }
        }



        protected void FromXml(XmlReader r, bool loadall)
        {
            string colfilename = "";
            string baseName = WorldFilePath.Substring(0, WorldFilePath.LastIndexOf('\\'));
            do
            {
                r.Read();
            } while ((r.NodeType != XmlNodeType.Element) || !(String.Equals(r.Name, "WorldObjectCollection")));

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
                        case "Road":
                            RoadObject road = new RoadObject(r, this, app);
                            Add(road);
                            break;

                        case "StaticObject":
                            StaticObject obj = new StaticObject(this, app, r);
                            Add(obj);
                            break;
                        case "Waypoint":
                            Waypoint wp = new Waypoint(r, this, app);
                            Add(wp);
                            break;
                        case "Boundary":
                            Boundary b = new Boundary(r, this, app);
                            Add(b);
                            break;
                        case "PointLight":
                            PointLight pl = new PointLight(app, this, app.Scene, r);
                            Add(pl);
                            break;
                        case "TerrainDecal":
                            TerrainDecal d = new TerrainDecal(app, this, r);
                            Add(d);
                            break;
                        case "WorldCollection":
                            string collectionName = null;
                            colfilename = "";
                            for (int i = 0; i < r.AttributeCount; i++)
                            {
                                r.MoveToAttribute(i);
                                switch (r.Name)
                                {
                                    case "Name":
                                        collectionName = r.Value;
                                        break;
                                    case "Filename":
                                        colfilename = r.Value;
                                        break;
                                }
                            }
                            baseName = this.Path;
                            if (!loadall)
                            {
                                if (colfilename != "")
                                {
                                    if (colfilename.EndsWith("~.mwc"))
                                    {
                                        string autofilepath = String.Format("{0}\\{1}", baseName, colfilename);
                                        string normalfilepath = String.Format("{0}\\{1}", baseName, colfilename.Remove(colfilename.LastIndexOf("~"), 1));
                                        if ((File.Exists(autofilepath) && File.Exists(normalfilepath) &&
                                            (new FileInfo(autofilepath)).LastWriteTime < (new FileInfo(normalfilepath).LastWriteTime))
                                            || (!File.Exists(autofilepath) && File.Exists(normalfilepath)))
                                        {
                                            colfilename = colfilename.Remove(filename.LastIndexOf("~"), 1);
                                        }
                                    }
                                    XmlReader childReader = XmlReader.Create(String.Format("{0}\\{1}", baseName, colfilename), app.XMLReaderSettings);
                                    WorldObjectCollection coll = new WorldObjectCollection(childReader, collectionName, this, app, baseName, false);
                                    while (colfilename.Contains("~"))
                                    {
                                        colfilename = colfilename.Remove(colfilename.LastIndexOf("~"),1);
                                    }
                                    coll.Filename = colfilename;
                                    Add(coll);
                                    childReader.Close();
                                }
                                else
                                {
                                    XmlReader childReader = XmlReader.Create(String.Format("{0}\\{1}.mwc", baseName, collectionName), app.XMLReaderSettings);
                                    WorldObjectCollection coll = new WorldObjectCollection(childReader, collectionName, this, app, baseName, false);
                                    coll.Filename = collectionName + ".mwc";
                                    Add(coll);
                                    childReader.Close();
                                }
                            }
                            else
                            {
                                if (colfilename != "")
                                {
                                    if (colfilename.EndsWith("~.mwc"))
                                    {
                                        string autofilepath = String.Format("{0}\\{1}", baseName, colfilename);
                                        string normalfilepath = String.Format("{0}\\{1}", baseName, colfilename.Remove(filename.LastIndexOf("~"), 1));
                                        if ((File.Exists(autofilepath) && File.Exists(normalfilepath) &&
                                            (new FileInfo(autofilepath)).LastWriteTime < (new FileInfo(normalfilepath).LastWriteTime))
                                            || (!File.Exists(autofilepath) && File.Exists(normalfilepath)))
                                        {
                                            colfilename = colfilename.Remove(colfilename.LastIndexOf("~"), 1);
                                        }
                                    }
                                    XmlReader childReader = XmlReader.Create(String.Format("{0}\\{1}", baseName, colfilename), app.XMLReaderSettings);
                                    WorldObjectCollection coll = new WorldObjectCollection(childReader, collectionName, this, app, baseName);
                                    while (colfilename.Contains("~"))
                                    {
                                        colfilename = colfilename.Remove(colfilename.LastIndexOf("~"), 1);
                                    }
                                    coll.Filename = colfilename;
                                    Add(coll);
                                    childReader.Close(); 
                                }
                                else
                                {
                                    XmlReader childReader = XmlReader.Create(String.Format("{0}\\{1}.mwc", baseName, collectionName), app.XMLReaderSettings);
                                    WorldObjectCollection coll = new WorldObjectCollection(childReader, collectionName, this, app, baseName);
                                    coll.Filename = collectionName + ".mwc";
                                    Add(coll);
                                    childReader.Close();
                                }
                            }

                            r.MoveToElement();

                            break;
                    }
                }
            }

            while(filename.Contains("~"))
            {
                filename = filename.Remove(filename.IndexOf("~"), 1);
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
            WorldObjectCollection clone = new WorldObjectCollection(name, copyParent, app);
            foreach (IWorldObject child in objectList)
            {
                child.Clone(clone);
            }
            copyParent.Add(clone);
        }

        [DescriptionAttribute("The name of this collection."), CategoryAttribute("Miscellaneous")]
        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                foreach (WorldObjectCollection col in ((IObjectCollectionParent)parent).CollectionList)
                {
                    if (String.Equals(value, col.Name))
                    {
                        return;
                    }
                }
                name = value;
                UpdateNode();
            }
        }

        protected void UpdateNode()
        {
            if (inTree)
            {
                node.Text = NodeName;
            }
        }

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
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }

        #region IObjectCollectionParent
        [BrowsableAttribute(false)]
        public string WorldFilePath
        {
            get
            {
                return String.Format("{0}\\{1}", path, filename);
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
                List<WorldObjectCollection> list = new List<WorldObjectCollection>();
                foreach (IWorldObject obj in objectList)
                {
                    if (String.Equals(obj.ObjectType, "Collection"))
                    {
                        list.Add((WorldObjectCollection)obj);
                    }
                }
                return list;
            }
        }
        #endregion IObjectCollectionParent

        [BrowsableAttribute(false)]
        public IWorldContainer Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }



        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "Collection";
            }
        }

        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {           
            this.parentNode = parentNode;

            // create a node for the collection and add it to the parent
            node = app.MakeTreeNode(this, NodeName);
            parentNode.Nodes.Add(node);
            // build the menu
            inTree = true;
            buildMenu(node);
            if (loaded)
            {
                // Iterate all children and have them add themselves to the tree
                foreach (IWorldObject child in objectList)
                {
                    child.AddToTree(node);
                }
            }
        }

        private void buildMenu(WorldTreeNode node)
        {
            if (inTree && node != null)
            {
                CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
                if (loaded)
                {
                    menuBuilder.Add("Create Object Collection", new CreateWorldCollectionCommandFactory(app, (IObjectCollectionParent)this), app.DefaultCommandClickHandler);
                    menuBuilder.Add("Add Object", new AddObjectCommandFactory(app, this), app.DefaultCommandClickHandler);
                    menuBuilder.Add("Add Road", new AddRoadCommandFactory(app, this), app.DefaultCommandClickHandler);
                    menuBuilder.Add("Add Marker", new AddMarkerCommandFactory(app, this), app.DefaultCommandClickHandler);
                    menuBuilder.Add("Add Marker at Camera", new AddMarkerAtCameraCommandFactory(app, this), app.DefaultCommandClickHandler);
                    menuBuilder.Add("Add Region", new AddRegionCommandFactory(app, this), app.DefaultCommandClickHandler);
                    menuBuilder.Add("Add Point Light", new AddPointLightCommandFactory(app, this), app.DefaultCommandClickHandler);
                    menuBuilder.Add("Add Terrain Decal", new AddTerrainDecalCommandFactory(app, this), app.DefaultCommandClickHandler);
                    menuBuilder.AddDropDown("Move to Collection", menuBuilder.ObjectCollectionMoveDropDown_Opening);
                    menuBuilder.FinishDropDown();
                    menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
                    menuBuilder.Add("Unload Collection", new UnloadCollectionCommandFactory(app, this), app.DefaultCommandClickHandler);
                    if (inScene)
                    {
                        menuBuilder.Add("Remove From Scene", this, app.RemoveCollectionFromSceneClickHandler);
                        node.ImageIndex = 3;
                        node.SelectedImageIndex = 3;
                    }
                    else
                    {
                        menuBuilder.Add("Add To Scene", this, app.AddCollectionToSceneClickHandler);
                        node.ImageIndex = 2;
                        node.SelectedImageIndex = 2;
                    }
                    menuBuilder.Add("Help", "Collection", app.HelpClickHandler);
                    menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                    node.ContextMenuStrip = menuBuilder.Menu;
                    buttonBar = menuBuilder.ButtonBar;
                }
                else
                {
                    if (buttonBar != null)
                    {
                        buttonBar.Clear();
                    }
                    menuBuilder.Add("Load Collection", new LoadCollectionCommandFactory(app, this), app.DefaultCommandClickHandler);
                    node.ContextMenuStrip = menuBuilder.Menu;
                    node.ImageIndex = 1;
                    node.SelectedImageIndex = 1;
                    buttonBar = menuBuilder.ButtonBar;
                }
                app.UpdateButtonBar();
            }
        }

        [BrowsableAttribute(false)]
        public bool IsGlobal
        {
            get
            {
                return false;
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

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            foreach (IWorldObject obj in objectList)
            {
                obj.UpdateScene(type, hint);
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
            parentNode.Nodes.Remove(node);

            parentNode = null;
            node = null;
        }

        public void AddToScene()
        {
            inScene = true;
            // Iterate all children and have them add themselves to the tree
            foreach (IWorldObject child in objectList)
            {
                child.AddToScene();
            }
            if (node != null)
            {
                buildMenu(node);
            }
        }

        public void RemoveFromScene()
        {
            inScene = false;
            // Iterate all children and have them add themselves to the tree
            foreach (IWorldObject child in objectList)
            {
                child.RemoveFromScene();
            }
            if (node != null)
            {
                buildMenu(node);
            }
        }


        public void CheckAssets()
        {
            if (loaded)
            {
                foreach (IWorldObject child in objectList)
                {
                    child.CheckAssets();
                }
            }
        }

		public void ToXml(XmlWriter w)
        {
            if (loaded)
            {
                int pathEnd = ((IObjectCollectionParent)parent).WorldFilePath.LastIndexOf('\\');
                string pathName = ((IObjectCollectionParent)parent).WorldFilePath.Substring(0, pathEnd);
                w.WriteStartElement("WorldObjectCollection");
                w.WriteAttributeString("Version", app.Config.XmlSaveFileVersion.ToString());
                foreach (IWorldObject obj in objectList)
                {
                    if (String.Equals(obj.ObjectType, "Collection"))
                    {
                        WorldObjectCollection worldCollection = (WorldObjectCollection)obj;
                        worldCollection.Path = pathName;
                        // write the XML into the top level world file
                        w.WriteStartElement("WorldCollection");
                        if (worldCollection.Filename == "")
                        {
                            int startIndex = pathEnd + 1;
                            int subStringLength = WorldFilePath.LastIndexOf(".mwc") - startIndex;
                            worldCollection.Filename = String.Format("{0}-{1}.mwc", WorldFilePath.Substring(startIndex,
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
                    else
                    {
                        obj.ToXml(w);
                    }
                }
                w.WriteEndElement();
            }
        }

        public void ToXml(XmlWriter w, bool backup)
        {
            if (!backup)
            {
                ToXml(w);
            }

            if (loaded)
            {
                int pathEnd = ((IObjectCollectionParent)parent).WorldFilePath.LastIndexOf('\\');
                string pathName = ((IObjectCollectionParent)parent).WorldFilePath.Substring(0, pathEnd);
                w.WriteStartElement("WorldObjectCollection");
                w.WriteAttributeString("Version", app.Config.XmlSaveFileVersion.ToString());
                foreach (IWorldObject obj in objectList)
                {
                    if (String.Equals(obj.ObjectType, "Collection"))
                    {
                        WorldObjectCollection worldCollection = (WorldObjectCollection)obj;
                        worldCollection.Path = pathName;
                        // write the XML into the top level world file
                        w.WriteStartElement("WorldCollection");
                        if (worldCollection.Filename == "")
                        {
                            int startIndex = pathEnd + 1;
                            int subStringLength = WorldFilePath.LastIndexOf(".mwc") - startIndex;
                            worldCollection.Filename = String.Format("{0}-{1}.mwc", WorldFilePath.Substring(startIndex,
                                subStringLength), worldCollection.Name);
                        }
                        w.WriteAttributeString("Name", worldCollection.Name);
                        string filename = worldCollection.Filename.Insert((worldCollection.Filename.LastIndexOf(".")), "~");
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
                    }
                    else
                    {
                        obj.ToXml(w);
                    }
                }
                w.WriteEndElement();
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 FocusLocation
        {
            get
            {
                Vector3 v = Vector3.Zero;
                if (objectList.Count != 0)
                {
                    foreach (IWorldObject obj in objectList)
                    {
                        v += obj.FocusLocation;
                    }

                    v = v / objectList.Count;
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
            if (objectList != null)
            {
                foreach (IWorldObject child in objectList)
                {
                    child.ToManifest(w);
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region ICollection<IWorldObject> Members

        public void Add(IWorldObject item)
        {
            objectList.Add(item);
            if (inTree && Loaded)
            {
                item.AddToTree(node);
            }
            if (inScene && Loaded)
            {
                item.AddToScene();
            }
        }

        public void Clear()
        {
            objectList.Clear();
        }

        public bool Contains(IWorldObject item)
        {
            return objectList.Contains(item);
        }

        public void CopyTo(IWorldObject[] array, int arrayIndex)
        {
            objectList.CopyTo(array, arrayIndex);
        }

        [BrowsableAttribute(false)]
        public int Count
        {
            get 
            {
                return objectList.Count;
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
            return objectList.Remove(item);

        }

        #endregion

        #region IEnumerable<IWorldObject> Members

        public IEnumerator<IWorldObject> GetEnumerator()
        {
            return objectList.GetEnumerator();
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
