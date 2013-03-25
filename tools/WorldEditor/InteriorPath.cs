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

namespace Multiverse.Tools.WorldEditor
{

	public class InteriorPath : IWorldObject
	{
        protected WorldEditor app;
        protected IWorldContainer parent;
		protected PointCollection points;
		string meshName;
        protected WorldTreeNode node = null;
		protected WorldTreeNode parentNode = null;
		protected NameValueObject nameValuePairs;
        protected bool inScene = false;
		protected bool inTree = false;
        protected bool highlight = false;
		protected List<SceneRod> rods = new List<SceneRod>();
		protected static int nameCounter = 0;
        public event InSceneChangedEventHandler Changed;


		public InteriorPath(IWorldContainer parentContainer, WorldEditor worldEditor, string meshName)
		{
			// initialize data members
            parent = parentContainer;
			app = worldEditor;
			this.meshName = meshName;
            points = new PointCollection(this, worldEditor, true, worldEditor.Config.DisplayRegionPoints, worldEditor.Config.RegionPointMeshName, worldEditor.Config.RegionPointMaterial);
			nameValuePairs = new NameValueObject();
			points.PointsChanged += new PointsChangedEventHandler(PointsChangedHandler);
            Changed += new InSceneChangedEventHandler(app.SceneChangedHandler);
		}

		public InteriorPath(WorldEditor worldEditor, IWorldContainer parentContainer, string meshName, InteriorPathContents path)
		{
            parent = parentContainer;
			app = worldEditor;
            points = new PointCollection(this, worldEditor, true, worldEditor.Config.DisplayRegionPoints, worldEditor.Config.RegionPointMeshName, worldEditor.Config.RegionPointMaterial);
			nameValuePairs = new NameValueObject();
			points.PointsChanged += new PointsChangedEventHandler(PointsChangedHandler);
            Changed += new InSceneChangedEventHandler(app.SceneChangedHandler);
			foreach (Vector3 position in path.Points)
			{
				int index;
				points.AddPoint(position, out index);
			}
		}
		
		public InteriorPath(XmlReader r, IWorldContainer parentContainer, WorldEditor worldEditor)
        {
            app = worldEditor;
            this.parent = parentContainer;
            FromXml(r);
		}
			
		public void ToXml(XmlWriter w)
		{
			w.WriteStartElement("InteriorPath");
			w.WriteAttributeString("mesh", meshName);
			if (points != null)
			{
				points.ToXml(w);
			}
			if (nameValuePairs != null)
			{
				nameValuePairs.ToXml(w);
			}
            w.WriteEndElement();
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
                    case "mesh":
                        this.meshName = r.Value;
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
                                this.points = new PointCollection(this, app, true, app.Config.DisplayRegionPoints, this.app.Config.RegionPointMeshName, this.app.Config.RegionPointMaterial, r);
                                points.PointsChanged += new PointsChangedEventHandler(PointsChangedHandler);
                            }
                            break;
                        case "NameValuePairs":
                            this.nameValuePairs = new NameValueObject(r);
                            break;
                    }
                }
            }
            if (points == null)
            {
                this.points = new PointCollection(this, app, true, app.Config.DisplayRegionPoints, this.app.Config.RegionPointMeshName, this.app.Config.RegionPointMaterial);
                points.PointsChanged += new PointsChangedEventHandler(PointsChangedHandler);
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

		[EditorAttribute(typeof(NameValueUITypeEditorMarker), typeof(System.Drawing.Design.UITypeEditor)), DescriptionAttribute("Name/Value attributes for this Region.")]
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
			return true;
		}

		public void AddToTree(WorldTreeNode parentNode)
		{
			this.parentNode = parentNode;
			inTree = true;

            // create a node for the collection and add it to the parent
            node = new WorldTreeNode(this, NodeName);

			parentNode.Nodes.Add(node);

			points.AddToTree(node);
            // build the menu
            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Delete Path", new DeleteInteriorPathCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
			node.ContextMenuStrip = menuBuilder.Menu;
        }

        public void RemoveFromTree()
        {
            parentNode.Nodes.Remove(node);
            parentNode = null;
            node = null;
        }

		public void AddToScene()
		{
			if (highlight)
				AddRodsToScene(false);
		}

		public Vector3 TransformLocation(Vector3 p)
		{
			SceneNode s = ((StaticObject)parent).DisplayObject.SceneNode;
			Matrix4 inverseTransform = s.FullTransform.Inverse();
			return inverseTransform * p;
		}
		
// 		List<Vector3> GetTransformedPoints()
// 		{
// 			List<Vector3> positions = points.VectorList;
// 			SceneNode s = ((StaticObject)parent).DisplayObject.SceneNode;
// 			Matrix4 inverseTransform = s.FullTransform.Inverse();
// 			for (int i=0; i<positions.Count; i++)
// 				positions[i] = inverseTransform * positions[i];
// 			return positions;
// 		}
		
		public void AddRodsToScene(bool incremental)
		{
			if (points.Count >= 2) {
				if (incremental)
				{
					for (int i=1; i<points.Count; i++)
						NewSceneCapsule(points.GetPoint(i-1).Position, points.GetPoint(i).Position);
				}
				else {
					Vector3 previousPosition = points.GetPoint(points.Count - 1).Position;
					foreach (MPPoint p in points) {
						NewSceneCapsule(previousPosition, p.Position);
						previousPosition = p.Position;
					}
				}
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
			}
		}

        protected void BlastSceneNode(SceneNode node) {
            node.Creator.DestroySceneNode(node.Name);
        }

		public void RemoveFromScene()
		{
			RemoveRodsFromScene();
			
			// remove boundary point markers from scene
			points.RemoveFromScene();

			inScene = false;
		}

        protected void RemoveRodsFromScene()
		{
			if (rods.Count > 0) 
			{
				foreach (SceneRod rod in rods) 
				{
					BlastSceneNode(rod.end1);
					BlastSceneNode(rod.end2);
					BlastSceneNode(rod.cylinder);
				}
			}
			rods.Clear();
		}
		
		public void CheckAssets()
        {
        }

		protected void PointsChangedHandler(object sender, EventArgs args)
		{
			RefreshPoints();
		}

        protected string NodeName
        {
            get
            {
                return "Interior Path";
            }
        }

        [DescriptionAttribute("The type of this object.")]
        public string ObjectType
        {
            get
            {
                return "Interior Path";
            }
        }

        [BrowsableAttribute(false)]
        public bool Highlight
        {
            get
            {
				return this.highlight;
            }
            set
            {
                if (!value && this.highlight)
					RemoveRodsFromScene();
				if (value && !this.highlight)
					AddRodsToScene(false);
				this.highlight = value;
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
        }

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
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

		public void DisplayRods(bool incremental) 
		{
			RemoveRodsFromScene();
			if (highlight)
				AddRodsToScene(incremental);
		}
		
		private SceneNode UnscaledSceneObject(string meshName, Vector3 position)
		{
			nameCounter++;
            string name = "Rod-" + nameCounter;
			Entity entity = app.Scene.CreateEntity(name, meshName);
			SceneNode node = ((StaticObject)parent).DisplayObject.SceneNode.CreateChildSceneNode(name);
			node.AttachObject(entity);
			node.Position = position;
			node.ScaleFactor = Vector3.UnitScale;
			node.Orientation = Quaternion.Identity;
			return node;
		}

		private SceneNode NewSceneObject(string meshName, Vector3 position, float scale)
		{
			SceneNode node = UnscaledSceneObject(meshName, position);
			node.ScaleFactor = Vector3.UnitScale * scale;
			return node;
		}

		private SceneNode NewSceneObject(string meshName, Vector3 position, 
										 Vector3 scale, Quaternion orientation)
		{
			SceneNode node = UnscaledSceneObject(meshName, position);
			node.ScaleFactor = scale;
			node.Orientation = orientation;
			return node;
		}
	
		private SceneNode NewSceneSphere(Vector3 center, float radius)
		{
			return NewSceneObject("unit_sphere.mesh", center, radius);
		}

		protected void NewSceneCapsule(Vector3 from, Vector3 to) {
			float radius = 5f / 100f;
			// Place the rods so they sit on the triangles
			from.y += radius;
			to.y += radius;
			SceneNode end1 = NewSceneSphere(from, radius);
			SceneNode end2 = NewSceneSphere(to, radius);
			Vector3 seg = (from - to);
			rods.Add(new SceneRod(end1,
								  end2,
								  NewSceneObject("unit_cylinder.mesh",
												 to + seg * 0.5f,
												 new Vector3(radius, seg.Length / 1000f, radius),
												 new Vector3(0f, 1f, 0f).GetRotationTo(seg))));
		}
	}

	public class SceneRod {
		public SceneNode end1;
		public SceneNode end2;
		public SceneNode cylinder;
		
		public SceneRod(SceneNode end1, SceneNode end2, SceneNode cylinder) 
		{
			this.end1 = end1;
			this.end2 = end2;
			this.cylinder = cylinder;
		}

	}

	internal struct InteriorPathSetLookup
	{
		internal bool found;
		internal InteriorPathSet paths;
		internal InteriorPathSetLookup(bool found, InteriorPathSet paths)
		{
			this.found = found;
			this.paths = paths;
		}
	}
	
	// This class holds all the InteriorPathContents instances read
	// from a .modelpaths file associated with a mesh model.  It has
	// no connection with any UI objects.
	public class InteriorPathSet
	{
		protected string meshName;
		protected List<InteriorPathContents> pathSet = new List<InteriorPathContents>();

        internal static Dictionary<string, InteriorPathSetLookup> pathSetDictionary = 
                new Dictionary<string, InteriorPathSetLookup>();

		// This constructor is used when saving a collection of
		// InteriorPaths from a UI model object.
		public InteriorPathSet(string meshName)
		{
			this.meshName = meshName;
			pathSetDictionary[meshName] = new InteriorPathSetLookup(true, this);
		}

		// This constructor reads the xml from the file, filling in
		// the instance, and adding it to the dictionary mapping mesh
		// name to InteriorPathSet instance.
		public InteriorPathSet(Stream filestream, WorldEditor app, string meshName)
		{
            this.meshName = meshName;
            XmlReader r = XmlReader.Create(filestream, app.XMLReaderSettings);
			FromXml(r, app);
			pathSetDictionary[meshName] = new InteriorPathSetLookup(true, this);
		}
		
		// Look up the path set associated with a particular mesh
		public static InteriorPathSet FindPathsForMesh(WorldEditor app, string meshName)
		{
			InteriorPathSetLookup result;
			if (pathSetDictionary.TryGetValue(meshName, out result))
				return result.paths;
			else {
                string meshNameWithoutExtension = Path.GetFileNameWithoutExtension(meshName);
				string modelPathFile = meshNameWithoutExtension + ".modelpaths";
                try {
                    Stream stream = ResourceManager.FindCommonResourceData(modelPathFile);
					InteriorPathSet pathset = new InteriorPathSet(stream, app, meshNameWithoutExtension);
                    return pathset;
				}
				catch (Exception e) {
					LogManager.Instance.Write("Could not read '{0}'; error was '{1}'",
											  modelPathFile, e.Message);
					pathSetDictionary[meshNameWithoutExtension] = new InteriorPathSetLookup(false, null);
					return null;
				}
			}
		}
		
		public List<InteriorPathContents> PathSet
		{
			get {
				return pathSet;
			}
		}
		
		protected void FromXml(XmlReader r, WorldEditor app)
        {
            do
            {
                r.Read();
                if (r.EOF)
                    throw new Exception("Ill-formed .modelpaths file ");
            } while ((r.NodeType != XmlNodeType.Element) || !(String.Equals(r.Name, "InteriorPathSet")));


			// first parse the attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "mesh":
                        this.meshName = r.Value;
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
                        case "InteriorPath":
							pathSet.Add(new InteriorPathContents(r));
							break;
					}
				}
			}
		}
		
		public void ToXml(XmlWriter w, WorldEditor app)
		{
			w.WriteStartElement("InteriorPathSet");
            w.WriteAttributeString("Version", app.Config.XmlSaveFileVersion.ToString());
			w.WriteAttributeString("mesh", meshName);
			foreach(InteriorPathContents path in pathSet)
				path.ToXml(w);
            w.WriteEndElement();
		}

	}
	
	// This class represents the state of an InteriorPath as
	// read from a .modelpaths file.  It has no connection with
	// any UI objects.
	public class InteriorPathContents
	{
		protected List<Vector3> points = new List<Vector3>();
		protected NameValueObject nameValuePairs;

		public InteriorPathContents()
		{
		}

		public InteriorPathContents(InteriorPath path)
		{
			points = path.Points.VectorList;
		}
		
		public InteriorPathContents(XmlReader r)
		{
			FromXml(r);
		}
		
		public void ToXml(XmlWriter w)
		{
			w.WriteStartElement("InteriorPath");
			if (points != null)
			{
				w.WriteStartElement("PointCollection");
				foreach(Vector3 point in points)
				{
					w.WriteStartElement("Point");
					w.WriteAttributeString("x", point.x.ToString());
					w.WriteAttributeString("y", point.y.ToString());
					w.WriteAttributeString("z", point.z.ToString());
					w.WriteEndElement();
				}
				w.WriteEndElement();
			}
			if (nameValuePairs != null)
			{
				nameValuePairs.ToXml(w);
			}
            w.WriteEndElement();
		}

        protected void FromXml(XmlReader r)
        {
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
						while (r.Read())
						{
							// look for the start of an element
							if (r.NodeType == XmlNodeType.Element)
                            {
                                string elementName = r.Name;
                                switch (elementName)
                                {
                                case "Point":
                                    points.Add(XmlHelperClass.ParseVectorAttributes(r));
                                    break;
                                }
                            }
							else if (r.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
						}
						r.MoveToElement(); //Moves the reader back to the element node.
						break;

				    case "NameValuePairs":
                            this.nameValuePairs = new NameValueObject(r);
                            break;
					}
				}
			}
		}

		public List<Vector3> Points
		{
			get {
				return points;
			}
		}
		
	}
	
}
