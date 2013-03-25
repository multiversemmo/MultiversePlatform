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

namespace Multiverse.Tools.WorldEditor
{
    public class WorldTerrain : IWorldObject
    {
        protected WorldEditor app;
        protected WorldTreeNode node;
        protected WorldTreeNode parentNode;

        protected IWorldObject terrainDisplay;

        protected bool inScene = false;
        protected bool inTree = false;

        protected Axiom.SceneManagers.Multiverse.ITerrainGenerator terrainGenerator;

        protected string displayType = "Auto Splat";
        protected List<ToolStripButton> buttonBar;

        protected Axiom.SceneManagers.Multiverse.ITerrainGenerator DefaultTerrainGenerator()
        {
            Multiverse.Generator.FractalTerrainGenerator gen = new Multiverse.Generator.FractalTerrainGenerator();
            gen.HeightFloor = 20;
            gen.HeightScale = 0;

            return gen;
        }

        public WorldTerrain(WorldEditor worldEditor)
        {
            app = worldEditor;

            terrainDisplay = new AutoSplatTerrainDisplay(this, app);

            terrainGenerator = DefaultTerrainGenerator();
        }

        public WorldTerrain(WorldEditor worldEditor, XmlReader r)
            : this(worldEditor)
        {
            FromXml(r);
        }

        protected void FromXml(XmlReader r)
        {
            // search attributes to see if Type is specified
            string type = null;
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                if (r.Name == "Type")
                {
                    type = r.Value;
                    break;
                }
            }
            r.MoveToElement();

            if (type == "HeightfieldMosaic")
            {
                terrainGenerator = new Multiverse.Lib.HeightfieldGenerator.HeightfieldTerrainGenerator(r);
            }
            else
            {
                Multiverse.Generator.FractalTerrainGenerator gen = new Multiverse.Generator.FractalTerrainGenerator();

                gen.FromXML(r);

                terrainGenerator = gen;
            }
        }

        public void LoadTerrainFile(string filename)
        {
            int dot = filename.LastIndexOf('.');
            string fileExt = filename.Substring(dot + 1);

            switch (fileExt)
            {
                case "mvt":
                    XmlReader r = XmlReader.Create(filename, app.XMLReaderSettings);

                    // read until we find the start of the world description
                    while (r.Read())
                    {
                        // look for the start of the terrain description
                        if (r.NodeType == XmlNodeType.Element)
                        {
                            if (r.Name == "Terrain")
                            {

                                LoadTerrain(r);
                                break;
                            }
                        }
                    }
                    r.Close();
                    break;
                case "mmf":
                    int lastslash = filename.LastIndexOf('\\');
                    string baseName = filename.Substring(lastslash + 1, dot - lastslash - 1);

                    try
                    {
                        terrainGenerator = new Multiverse.Lib.HeightfieldGenerator.HeightfieldTerrainGenerator(baseName, 1, 0);
                        app.TerrainGenerator = terrainGenerator;
                    }
                    catch (Axiom.Core.AxiomException ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.Message);
                    }

                    break;
            }
        }

        public void LoadTerrain(XmlReader r)
        {
            FromXml(r);

            app.TerrainGenerator = terrainGenerator;
        }

        public void LoadTerrain(string s)
        {
            XmlReader r = XmlReader.Create(new StringReader(s), app.XMLReaderSettings);

            while (r.Read())
            {
                // look for the start of the terrain description
                if (r.NodeType == XmlNodeType.Element)
                {
                    if (r.Name == "Terrain")
                    {

                        LoadTerrain(r);
                        break;
                    }
                }
            }
            r.Close();
        }

        [TypeConverter(typeof(TerrainDisplayTypeConverter)),CategoryAttribute("Miscellaneous"), DescriptionAttribute("Selects which of the available terrain texturing methods to use, Auto Splat or Alpha Splat.")]
        public string DisplayType
        {
            get
            {
                return displayType;
            }
            set
            {
                if (displayType != value)
                {
                    displayType = value;

                    // remove old terrainDisplay from scene and tree
                    if (inScene)
                    {
                        terrainDisplay.RemoveFromScene();
                    }

                    if (inTree)
                    {
                        terrainDisplay.RemoveFromTree();
                    }

                    //terrainDisplay.Dispose();

                    // create new terrainDisplay
                    switch (displayType)
                    {
                        case "Auto Splat":
                            terrainDisplay = new AutoSplatTerrainDisplay(this, app);
                            break;
                        case "Alpha Splat":
                            terrainDisplay = new AlphaSplatTerrainDisplay(this, app);
                            break;
                    }

                    // add new ont to tree and scene
                    if (inTree)
                    {
                        terrainDisplay.AddToTree(node);
                    }
                    if (inScene)
                    {
                        terrainDisplay.AddToScene();
                    }
                }
            }
        }

        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;

            // add the terrain node
            node = app.MakeTreeNode(this, "Terrain");
            parentNode.Nodes.Add(node);

            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();

            menuBuilder.Add("Load Terrain", new LoadTerrainCommandFactory(app), app.DefaultCommandClickHandler);
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Terrain", app.HelpClickHandler);

            node.ContextMenuStrip = menuBuilder.Menu;

            terrainDisplay.AddToTree(node);

            inTree = true;
            buttonBar = menuBuilder.ButtonBar;
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
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString += String.Format("\tDisplayType={0}\r\n",DisplayType);
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

        public void RemoveFromTree()
        {
            if (node.IsSelected)
            {
                node.UnSelect();
            }
            terrainDisplay.RemoveFromTree();

            parentNode.Nodes.Remove(node);
            parentNode = null;
            node = null;
            inTree = false;
        }

        public void Clone(IWorldContainer copyParent)
        {
        }

        public void AddToScene()
        {
            inScene = true;

            // tell the app to use our terrain generator
            app.TerrainGenerator = terrainGenerator;

            terrainDisplay.AddToScene();
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
        }

        public void RemoveFromScene()
        {
            terrainDisplay.RemoveFromScene();
            inScene = false;
        }


        public void CheckAssets()
        {
            terrainDisplay.CheckAssets();
        }

		public void ToXml(XmlWriter w)
        {
            terrainGenerator.ToXml(w);

            terrainDisplay.ToXml(w);
        }

        public void DisplayParamsFromXml(XmlReader r)
        {
            string type = null;
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                if (r.Name == "Type")
                {
                    type = r.Value;
                    break;
                }
            }
            r.MoveToElement();

            if (type == "AutoSplat")
            {
                terrainDisplay = new AutoSplatTerrainDisplay(this, app, r);
                displayType = "Auto Splat";
            }
            else if (type == "AlphaSplat")
            {
                terrainDisplay = new AlphaSplatTerrainDisplay(this, app, r);
                displayType = "Alpha Splat";
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 FocusLocation
        {
            get
            {
                return Vector3.Zero;
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

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "WorldTerrain";
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

        public void ToManifest(System.IO.StreamWriter w)
        {
            if (terrainGenerator is Multiverse.Lib.HeightfieldGenerator.HeightfieldTerrainGenerator)
            {
                Multiverse.Lib.HeightfieldGenerator.HeightfieldTerrainGenerator tmpGen = terrainGenerator as Multiverse.Lib.HeightfieldGenerator.HeightfieldTerrainGenerator;

                w.WriteLine("Mosaic:{0}", tmpGen.HeightfieldName);
            }

            terrainDisplay.ToManifest(w);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
