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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Text;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using System.Diagnostics;
using System.Windows.Forms;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    public enum BoundaryType
    {
        Water,
        Forest,
        Fog
    }

    public delegate void InSceneChangedEventHandler(Object sender, EventArgs args);

    public class Boundary : IWorldObject, IWorldContainer, IObjectInsert, IObjectChangeCollection, IObjectDelete, IObjectCutCopy, IObjectDrag, IObjectCameraLockable
	{
        protected IWorldContainer parent;
        protected WorldEditor app;
        protected WorldTreeNode node = null;
		protected WorldTreeNode parentNode = null;
		protected string name;
		protected Axiom.SceneManagers.Multiverse.Boundary sceneBoundary;
		protected List<IWorldObject> children;
		protected const float defaultScale = 4f;
		protected NameValueObject nameValuePairs;
		protected PointCollection points;
        protected bool inScene = false;
		protected bool inTree = false;
        protected int priority;
        protected List<ToolStripButton> buttonBar;
        public event InSceneChangedEventHandler Changed;


		public Boundary(IWorldContainer parentContainer, WorldEditor worldEditor, string namein, int priority)
		{
			// initialize data members
            parent = parentContainer;
			app = worldEditor;
			name = namein;
            
            this.priority = priority;
			nameValuePairs = new NameValueObject();
            children = new List<IWorldObject>();
            points = new PointCollection(this, worldEditor, true, worldEditor.Config.DisplayRegionPoints, worldEditor.Config.RegionPointMeshName, worldEditor.Config.RegionPointMaterial, MPPointType.Boundary);
            Add(points);
			points.PointsChanged += new PointsChangedEventHandler(PointsChangedHandler);
            Changed += new InSceneChangedEventHandler(app.SceneChangedHandler);
		}

        public Boundary(XmlReader r, IWorldContainer parentContainer, WorldEditor worldEditor)
        {
            // initialize data members
            parent = parentContainer;
            app = worldEditor;

            children = new List<IWorldObject>();

            FromXml(r);

            if (nameValuePairs == null)
            {
                nameValuePairs = new NameValueObject();
            }
            Changed += new InSceneChangedEventHandler(app.SceneChangedHandler);
        }

        public Boundary(WorldEditor worldEditor, IWorldContainer parentContainer, string namein, int priority)
        {
            // initialize data members
            parent = parentContainer;
            app = worldEditor;
            name = namein;

            this.priority = priority;
            nameValuePairs = new NameValueObject();
            children = new List<IWorldObject>();
            points = null;
            Changed += new InSceneChangedEventHandler(app.SceneChangedHandler);
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
                        this.name = r.Value;
                        break;
                    case "Priority":
                        this.priority = int.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.

            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if (r.NodeType == XmlNodeType.Element)
                {
                    switch (r.Name)
                    {
                        case "PointCollection":
                            if (!r.IsEmptyElement)
                            {
                                this.points = new PointCollection(this, app, true, app.Config.DisplayRegionPoints, this.app.Config.RegionPointMeshName, this.app.Config.RegionPointMaterial,MPPointType.Boundary, r);
                                Add(points);
                                points.PointsChanged += new PointsChangedEventHandler(PointsChangedHandler);
                            }
                            break;
                        case "NameValuePairs":
                            this.nameValuePairs = new NameValueObject(r);
                            break;
                        case "Forest":
                            Forest forest = new Forest(r, this, app);
                            Add(forest);
                            break;
                        case "Fog":
                            Fog fog = new Fog(r, this, app);
                            Add(fog);
                            break;
                        case "Water":
                            Water water = new Water(r, this, app);
                            Add(water);
                            break;
                        case "Sound":
                            Sound sound = new Sound(r, (IWorldContainer)this, app);
                            Add(sound);
                            break;
                        case "Grass":
                            Grass grass = new Grass(r, this, app);
                            Add(grass);
                            break;
                        case "SpawnGen":
                            SpawnGen mob = new SpawnGen(r, app, this);
                            Add(mob);
                            break;
                        case "AmbientLight":
                            AmbientLight ambientLight = new AmbientLight(app, this, r);
                            Add(ambientLight);
                            break;
                        case "DirectionalLight":
                            DirectionalLight directionalLight = new DirectionalLight(app, this, r);
                            Add(directionalLight);
                            break;
                    }
                }
            }
            if (points == null)
            {
                this.points = new PointCollection(this, app, true, app.Config.DisplayRegionPoints, this.app.Config.RegionPointMeshName, this.app.Config.RegionPointMaterial, MPPointType.Boundary);
                Add(points);
                points.PointsChanged += new PointsChangedEventHandler(PointsChangedHandler);
            }
        }

        [BrowsableAttribute(false)]
        public List<IWorldObject> Children
        {
            get
            {
                return children;
            }
        }

        [BrowsableAttribute(false)]
		public Vector3 FocusLocation
		{
			get
			{
				return points.FocusLocation;
			}
		}

        [EditorAttribute(typeof(NameValueUITypeEditorBoundary), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Arbitrary Name/Value pair used to pass information about an object to server scripts and plug-ins. Click [...] to add or edit name/value pairs."), CategoryAttribute("Miscellaneous")]
		public NameValueObject NameValue
		{
			get
			{
				return nameValuePairs;
			}
			set
			{
				nameValuePairs = value;
			}
		}

		[BrowsableAttribute(false)]
		public PointCollection Points
		{
            get
            {
                return points;
            }
		}

        public bool PointIn(Vector3 point)
        {
            if (this.Count < 3)
            {
                return false;
            }
            else
            {
                if (inScene)
                {
                    return SceneBoundary.PointIn(point);
                }
            }
            return false;
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
		public Axiom.SceneManagers.Multiverse.Boundary SceneBoundary
		{
			get
			{
				return sceneBoundary;
			}
		}

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

		public void AddToTree(WorldTreeNode parentNode)
		{
            if (parentNode != null)
            {
                this.parentNode = parentNode;
                inTree = true;

                // create a node for the collection and add it to the parent
                node = app.MakeTreeNode(this, NodeName);

                parentNode.Nodes.Add(node);
                CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
                menuBuilder.Add("Add Forest", new AddForestCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                menuBuilder.Add("Add Water", new AddWaterCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                menuBuilder.Add("Add Fog", new AddFogCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                menuBuilder.Add("Add Sound", new AddSoundCommandFactory(app, (IWorldContainer)this), app.DefaultCommandClickHandler);
                menuBuilder.Add("Add Vegetation", new AddGrassCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                menuBuilder.Add("Add Spawn Generator", new AddSpawnGenToRegionCommandFactory(app, this), app.DefaultCommandClickHandler);
                menuBuilder.Add("Add Ambient Light", new AddAmbientLightCommandFactory(app, this), app.DefaultCommandClickHandler);
                menuBuilder.Add("Add Directional Light", new AddDirectionalLightCommandFactory(app, this), app.DefaultCommandClickHandler);
                menuBuilder.Add("Drag Region", new DragObjectsFromMenuCommandFactory(app), app.DefaultCommandClickHandler);
                menuBuilder.AddDropDown("Move to Collection", menuBuilder.ObjectCollectionDropDown_Opening);
                menuBuilder.FinishDropDown();
                menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
                menuBuilder.Add("Help", "Region", app.HelpClickHandler);
                menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);

                node.ContextMenuStrip = menuBuilder.Menu;

                //			points.AddToTree(node);

                foreach (IWorldObject child in children)
                {
                    child.AddToTree(node);
                }
                buttonBar = menuBuilder.ButtonBar;
            }
        }


        public void Clone(IWorldContainer copyParent)
        {
            Boundary clone = new Boundary(app, copyParent, name, priority);
            foreach (IWorldObject child in children)
            {
                child.Clone(clone);
            }
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public bool InScene
        {
            get
            {
                return inScene;
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


        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", NodeName);
                objString += String.Format("\tPriority={0}\r\n", priority);
                objString += String.Format("\tCenter=({0},{1},{2}\r\n", FocusLocation.x, FocusLocation.y, FocusLocation.z);
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
            if (node != null && inTree)
            {
                if (node.IsSelected)
                {
                    node.UnSelect();
                }
                foreach (IWorldObject child in children)
                {
                    child.RemoveFromTree();
                }

                parentNode.Nodes.Remove(node);
                parentNode = null;
                node = null;
            }
        }

		public void AddToScene()
		{
            if (!inScene)
            {
                inScene = true;


                // create scene manager boundary
                sceneBoundary = new Axiom.SceneManagers.Multiverse.Boundary(WorldEditor.GetUniqueName("Boundary", name));
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.AddBoundary(sceneBoundary);
            }
			// update points list in scene manager
			RefreshPoints();

			// add all children to the scene
			foreach (IWorldObject child in children)
			{
				child.AddToScene();
			}
            OnSceneChange();
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
                return true;
            }
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            foreach (IWorldObject obj in children)
            {
                obj.UpdateScene(type, hint);
            }
        }

        public void OnSceneChange()
        {
            InSceneChangedEventHandler e = Changed;
            if (e != null)
            {
                e(this, new EventArgs());
            }
        }

		protected void RefreshPoints()
		{
			if (inScene)
			{

				List<Vector3> tmpPts = new List<Vector3>(points.Count);
				foreach (MPPoint pt in points)
				{
					tmpPts.Add(pt.Position);
				}

				this.sceneBoundary.SetPoints(tmpPts);
			}
		}


		public void RemoveFromScene()
		{
			// remove children from scene
            if (Highlight)
            {
                Highlight = false;
            }
			foreach (IWorldObject child in children)
			{
				child.RemoveFromScene();
			}

			// remove boundary point markers from scene
            //points.RemoveFromScene();

			// remove boundary from the scene
            if (sceneBoundary != null)
            {
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.RemoveBoundary(sceneBoundary);
            }
			sceneBoundary = null;

			inScene = false;
            OnSceneChange();
		}

        public void CheckAssets()
        {
            if (!app.CheckAssetFileExists(app.Config.RegionPointMeshName))
            {
                app.AddMissingAsset(app.Config.RegionPointMeshName);
            }
            string textureFile = "directional_marker_red.dds";
            if (!app.CheckAssetFileExists(textureFile))
            {
                app.AddMissingAsset(textureFile);
            }
            string materialFile = "directional_marker.material";
            if (!app.CheckAssetFileExists(materialFile))
            {
                app.AddMissingAsset(materialFile);
            }
            foreach (IWorldObject child in children)
            {
                child.CheckAssets();
            }
        }

        [DescriptionAttribute("Priority of the region. When regions containing fog or lights overlap, the one with the lowest priority value will override others."), BrowsableAttribute(true), CategoryAttribute("Miscellaneous")]
        public int Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
            }
        }

        [BrowsableAttribute(false)]
        public DisplayObject Display
        {
            get
            {
                return null;
            }
        }

        [BrowsableAttribute(false)]
        public bool AllowYChange
        {
            get
            {
                return false;
            }
        }

        [BrowsableAttribute(false)]
        public float TerrainOffset
        {
            get
            {
                return 0f;
            }
            set
            {
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 Position
        {
            get
            {
                return FocusLocation;
            }
            set
            {
                points.SetFocus(value.x, value.z);
            }
        }


		protected void PointsChangedHandler(object sender, EventArgs args)
		{
			RefreshPoints();
		}

		public void ToXml(XmlWriter w)
		{
			w.WriteStartElement("Boundary");
			w.WriteAttributeString("Name", name);
            w.WriteAttributeString("Priority", this.priority.ToString());
            //if (points != null)
            //{
            //    points.ToXml(w);
            //}
			if (nameValuePairs != null)
			{
				nameValuePairs.ToXml(w);
			}
			foreach (IWorldObject child in children)
			{
				child.ToXml(w);
			}
            w.WriteEndElement(); // Boundary end
		}

        [DescriptionAttribute("The name of this region."), BrowsableAttribute(true), CategoryAttribute("Miscellaneous")]
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

        [DescriptionAttribute("The type of this object."), BrowsableAttribute(true), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "Region";
            }
        }

        [BrowsableAttribute(false)]
        public bool Highlight
        {
            get
            {
                if (this.SceneBoundary != null)
                {
                    return this.sceneBoundary.Hilight;
                }
                return false;
            }
            set
            {
                if (this.SceneBoundary != null)
                {
                    this.sceneBoundary.Hilight = value;
                }
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
            if (children != null)
            {
                foreach (IWorldObject child in children)
                {
                    child.ToManifest(w);
                }
            }
        }

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }


		#region ICollection<IWorldObject> Members

		public void Add(IWorldObject item)
		{
			children.Add(item);
            if (String.Equals(item.ObjectType, "Points") && points == null)
            {
                this.points = (item as PointCollection);
                points.PointsChanged += new PointsChangedEventHandler(PointsChangedHandler);
            }
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
			children.Clear();
		}

		public bool Contains(IWorldObject item)
		{
			return children.Contains(item);
		}

		public void CopyTo(IWorldObject[] array, int arrayIndex)
		{
			children.CopyTo(array, arrayIndex);
		}

        [BrowsableAttribute(false)]
		public int Count
		{
			get
			{
				return points.Count;
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
			return children.Remove(item);
		}

		#endregion

		#region IEnumerable<IWorldObject> Members

		public IEnumerator<IWorldObject> GetEnumerator()
		{
			return children.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

        [BrowsableAttribute(false)]
        public bool AllowAdjustHeightOffTerrain
        {
            get
            {
                return false;
            }
        }

        [BrowsableAttribute(false)]
        public float Radius
        {
            get
            {
                return points.Radius;
            }
        }

        [BrowsableAttribute(false)]
        public Vector3 Center
        {
            get
            {
                return FocusLocation;
            }
        }

	}
}
