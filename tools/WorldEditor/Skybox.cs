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
using Axiom.MathLib;

namespace Multiverse.Tools.WorldEditor
{
    public class Skybox : IWorldObject
    {
        protected IWorldContainer parent;
        protected WorldEditor app;

        protected WorldTreeNode node;
        protected WorldTreeNode parentNode;

        protected bool inScene = false;
        protected bool inTree = false;

        protected string skyBoxName = null;

        protected List<ToolStripButton> buttonBar;

        public Skybox(IWorldContainer parentContainer, WorldEditor worldEditor)
        {
            this.parent = parentContainer;
            this.app = worldEditor;
        }

        public Skybox(IWorldContainer parentContainer, WorldEditor worldEditor, XmlReader r)
        {
            this.parent = parentContainer;
            this.app = worldEditor;

            FromXml(r);
        }

        protected void FromXml(XmlReader r)
        {
            // first parse the attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Name":
                        skyBoxName = r.Value;
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.
        }

        [TypeConverter(typeof(SkyboxAssetListConverter)), DescriptionAttribute("Name of the skybox as defined in the asset repository."), CategoryAttribute("Miscellaneous")]
        public string SkyboxName
        {
            get
            {
                if (skyBoxName == null || skyBoxName == "")
                {
                    return "None";
                }
                else
                {
                    return app.Assets.assetFromAssetName(skyBoxName).Name;
                }
            }
            set
            {
                if (value == "None")
                {
                    skyBoxName = null;
                    app.SetSkybox(false, null);
                }
                else
                {
                    skyBoxName = app.Assets.assetFromName(value).AssetName;
                    app.SetSkybox(true, skyBoxName);
                }
            }
        }

        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;

            // add the ocean node
            node = app.MakeTreeNode(this, "Skybox");
            parentNode.Nodes.Add(node);

            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Skybox", app.HelpClickHandler);

            node.ContextMenuStrip = menuBuilder.Menu;

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

        public void Clone(IWorldContainer copyParent)
        {
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
                // This property is not relevent for this object.
            }
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", ObjectType);
                objString += String.Format("\tSkyboxName={0}\r\n", SkyboxName);
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
            parentNode.Nodes.Remove(node);
            parentNode = null;
            node = null;

            inTree = false;
        }

        public void AddToScene()
        {
            if ((skyBoxName == null) || skyBoxName == "")
            {
                app.SetSkybox(false, null);
            }
            else
            {
                app.SetSkybox(true, skyBoxName);
            }
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
        }

        public void CheckAssets()
        {
            if (!app.CheckMaterialExists(skyBoxName))
            {
                app.AddMissingAsset(string.Format("Material: {0}", skyBoxName));
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

        public void RemoveFromScene()
        {
            app.SetSkybox(false, null);
        }

        public void ToXml(XmlWriter w)
        {
            if ((skyBoxName != null) && skyBoxName != "")
            {
                w.WriteStartElement("Skybox");
                w.WriteAttributeString("Name", skyBoxName);
                w.WriteEndElement();
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

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "Skybox";
            }
        }

        public void ToManifest(System.IO.StreamWriter w)
        {
            if ((skyBoxName != null) && (skyBoxName != ""))
            {
                w.WriteLine("Material:{0}", skyBoxName);
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

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
