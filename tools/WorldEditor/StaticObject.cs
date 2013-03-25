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
using Axiom.MathLib;
using Axiom.Core;
using Axiom.Animating;
using System.ComponentModel;
using System.Xml;
using Multiverse.CollisionLib;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    public class StaticObject : IWorldObject, IObjectPosition, IObjectScale, IWorldContainer, IObjectDrag, IObjectChangeCollection, IObjectDelete, IObjectCutCopy, IObjectCameraLockable, IObjectOrientation
    {
        protected DisplayObject displayObject;
        protected String name;
        protected IWorldContainer parent;
        protected WorldEditor app;
        protected WorldTreeNode node = null;
        protected WorldTreeNode parentNode = null;
        protected List<IWorldObject> children;

        protected Vector3 rotation;
        protected Vector3 direction;
        protected Vector3 scale;
        protected Vector3 location;
        protected Quaternion orientation;

        protected bool locationDirty = true;
        protected PathData pathData;
        
        protected String meshName;
        protected bool highlight = false;
        protected bool inTree = false;
        protected bool inScene = false;
        protected SubMeshCollection subMeshes;
        protected NameValueObject nameValuePairs;
        protected float terrainOffset = 0;
        protected bool allowAdjustHeightOffTerrain = true;
        protected bool offsetFound = false;
        protected bool castShadows = true;
        protected bool receiveShadows = true;
        protected bool acceptObjectPlacement = false;
        protected bool worldViewSelectable = true;
        protected float azimuth;
        protected float zenith;
        protected bool targetable = false;

        protected string soundAssetName = null;

        protected List<ToolStripButton> buttonBar;

        protected float perceptionRadius = 0;


        public StaticObject(String objectName, IWorldContainer parentContainer, WorldEditor worldEditor, string meshName, Vector3 position, Vector3 scale, Vector3 rotation)
        {
            name = objectName;
            parent = parentContainer;
            app = worldEditor;

            children = new List<IWorldObject>();
            this.SetDirection(rotation.y, 90f);
            this.scale = scale;
            this.location = position;
            this.meshName = meshName;
			this.nameValuePairs = new NameValueObject();
            this.terrainOffset = 0f;
            

            displayObject = null;

            subMeshes = new SubMeshCollection(meshName);
		}

        public StaticObject(IWorldContainer parentContainer, WorldEditor worldEditor, XmlReader r)
        {
            parent = parentContainer;
            app = worldEditor;

            children = new List<IWorldObject>();
            FromXml(r);

            if (nameValuePairs == null)
            {
                nameValuePairs = new NameValueObject();
            }
        }

        protected void FromXml(XmlReader r)
        {
            bool adjustHeightFound = false;
            bool offsetFound = false;
            bool pRFound = false;
            // first parse name and mesh, which are attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Name":
                        this.name = r.Value;
                        break;
                    case "Mesh":
                        this.meshName = r.Value;
                        break;
                    case "Sound":
                        string filename = r.Value;
                        if (!String.Equals(filename, ""))
                        {
                            ICommandFactory ret = new AddSoundCommandFactory(app, this, r.Value);
                            ICommand com = ret.CreateCommand();
                            com.Execute();
                        }
                        break;
                    case "TerrainOffset":
                        terrainOffset = float.Parse(r.Value);
                        offsetFound = true;
                        break;
                    case "AllowHeightAdjustment":
                        if (String.Equals(r.Value.ToLower(), "false"))
                        {
                            allowAdjustHeightOffTerrain = false;
                        }
                        break;
                    case "AcceptObjectPlacement":
                        acceptObjectPlacement = bool.Parse(r.Value);
                        break;
                    case "PerceptionRadius":
                        pRFound = true;
                        perceptionRadius = float.Parse(r.Value);
                        break;
                    case "CastShadows":
                        castShadows = bool.Parse(r.Value);
                        break;
                    case "ReceiveShadows":
                        receiveShadows = bool.Parse(r.Value);
                        break;
                    case "Azimuth":
                        azimuth = float.Parse(r.Value);
                        break;
                    case "Zenith":
                        zenith = float.Parse(r.Value);
                        break;
                    case "WorldViewSelect":
                        worldViewSelectable = bool.Parse(r.Value);
                        break;
                    case "Targetable":
                        targetable = bool.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.

            // now parse the sub-elements
			while (r.Read())
			{
				// look for the start of an element
				if (r.NodeType == XmlNodeType.Element)
				{
					// parse that element
					// save the name of the element
					string elementName = r.Name;
					switch (elementName)
					{
						case "Position":
                            location = XmlHelperClass.ParseVectorAttributes(r);
							break;
						case "Scale":
                            scale = XmlHelperClass.ParseVectorAttributes(r);
							break;
						case "Rotation":
                            Vector3 rotation = XmlHelperClass.ParseVectorAttributes(r);
                            // force rotation to be between -180 and 180
                            while (rotation.y < -180)
                            {
                                rotation.y += 360;
                            }
                            while (rotation.y > 180)
                            {
                                rotation.y -= 360;
                            }
                            SetDirection(rotation.y, 90f);
							break;
                        case "Orientation":
                            orientation = XmlHelperClass.ParseQuaternion(r);
                            break;
						case "SubMeshes":
							subMeshes = new SubMeshCollection(r);
                            if (!subMeshes.CheckValid(app, meshName))
                            {
                                app.AddPopupMessage(string.Format("Some submesh names in {0} changed.  Submesh display and material parameters for this object were reset.", meshName));

                                // if the check fails, then reset the subMeshes from the mesh
                                subMeshes = new SubMeshCollection(meshName);
                            }
							break;
                        case "NameValuePairs":
                            nameValuePairs = new NameValueObject(r);
                            break;
                        case "ParticleEffect":
                            ParticleEffect particle = new ParticleEffect(r, this, app);
                            Add(particle);
                            break;
                        case "PathData":
                            pathData = new PathData(r);
                            locationDirty = pathData.Version != pathData.CodeVersion;
                            break;
                        case "Sound":
                            Sound sound = new Sound(r, this, app);
                            Add(sound);
                            break;
					}
				}
				else if (r.NodeType == XmlNodeType.EndElement)
				{
					break;
				}
			}
            if (!adjustHeightFound)
            {
                allowAdjustHeightOffTerrain = true;
            }
            if (!offsetFound)
            {
                terrainOffset = location.y - app.GetTerrainHeight(location.x, location.z);
            }
            if (!pRFound && nameValuePairs != null)
            {
                valueItem value = nameValuePairs.LookUp("perceptionRadius");
                if (value != null && ValidityHelperClass.isFloat(value.value))
                {
                    perceptionRadius = float.Parse(value.value);
                }
            }
			return;
		}

        [BrowsableAttribute(false)]
        public float Radius
        {
            get
            {
                if (inScene && displayObject != null)
                {
                    return displayObject.Radius;
                }
                else
                {
                    return 0f;
                }
            }
        }


        [BrowsableAttribute(false)]
        public Vector3 Center
        {
            get
            {
                if (inScene && displayObject != null)
                {
                    return displayObject.Center;
                }
                else
                {
                    return Vector3.Zero;
                }
            }
        }

        [BrowsableAttribute(false)]
        public List<string> AttachmentPoints        
        {
            get
            {
                List<string> attachmentPoints = new List<string>();
                Mesh mesh = MeshManager.Instance.Load(meshName);

                for (int i = 0; i < mesh.AttachmentPoints.Count; i++)
                {
                    AttachmentPoint attachmentPoint = mesh.AttachmentPoints[i] as AttachmentPoint;

                    attachmentPoints.Add(attachmentPoint.Name);
                }

                if (mesh.Skeleton != null)
                {
                    for (int i = 0; i < mesh.Skeleton.AttachmentPoints.Count; i++)
                    {
                        AttachmentPoint attachmentPoint = mesh.Skeleton.AttachmentPoints[i] as AttachmentPoint;

                        attachmentPoints.Add(attachmentPoint.Name);
                    }
                }

                //mesh.Dispose();

                return attachmentPoints;
            }
        }

        [BrowsableAttribute(false)]
        public DisplayObject DisplayObject
        {
            get
            {
                return displayObject;
            }
        }

        [BrowsableAttribute(false)]
        public DisplayObject Display
        {
            get
            {
                return displayObject;
            }
        }

        public void SetCamera(Camera cam, CameraDirection dir)
        {
            AxisAlignedBox boundingBox = displayObject.BoundingBox;

            // find the size along the largest axis
            float size = Math.Max(Math.Max(boundingBox.Size.x, boundingBox.Size.y), boundingBox.Size.z);
            Vector3 dirVec;

            switch (dir)
            {
                default:
                case CameraDirection.Above:
                    // for some reason axiom messes up the camera matrix when you point
                    // the camera directly down, so this vector is ever so slightly off
                    // from negative Y.
                    dirVec = new Vector3(0.0001f, -1f, 0f);
                    dirVec.Normalize();
                    break;
                case CameraDirection.North:
                    dirVec = Vector3.UnitZ;
                    break;
                case CameraDirection.South:
                    dirVec = Vector3.NegativeUnitZ;
                    break;
                case CameraDirection.West:
                    dirVec = Vector3.UnitX;
                    break;
                case CameraDirection.East:
                    dirVec = Vector3.NegativeUnitX;
                    break;
            }

            cam.Position = boundingBox.Center + (size * 2 * (-dirVec));

            cam.Direction = dirVec;
        }

        [BrowsableAttribute(true), DescriptionAttribute("Whether the object preserves the height above terrain when the terrain is changed or the object is dragged or pasted to another location."), CategoryAttribute("Miscellaneous")]
        public bool AllowAdjustHeightOffTerrain
        {
            get
            {
                return allowAdjustHeightOffTerrain;
            }
            set
            {
                allowAdjustHeightOffTerrain = value;
            }
        }

        [BrowsableAttribute(true), DescriptionAttribute("This allows you to select if an object casts shadows."), CategoryAttribute("Display Propeties")]
        public bool CastShadows
        {
            get
            {
                return castShadows;
            }
            set
            {
                castShadows = value;
                if (inScene && displayObject != null)
                {
                    displayObject.CastShadows = value;
                }
            }
        }

        [BrowsableAttribute(false)]
        public bool ReceiveShadows
        {
            get
            {
                return receiveShadows;
            }
            set
            {
                receiveShadows = value;
            }
        }

        [BrowsableAttribute(true), DescriptionAttribute("Used by the Multiverse World Browser to determine if an object is targetable."), CategoryAttribute("Miscellaneous")]
        public bool Targetable
        {
            get
            {
                return targetable;
            }
            set
            {
                targetable = value;
            }
        }

		#region IWorldObject Members

		public void AddToTree(WorldTreeNode parentNode)
        {
            if (parentNode != null)
            {
                this.parentNode = parentNode;
                inTree = true;

                // create a node for the collection and add it to the parent
                node = app.MakeTreeNode(this, NodeName);
                parentNode.Nodes.Add(node);

                // build the menu
                CommandMenuBuilder menuBuilder = new CommandMenuBuilder();

                if (AttachmentPoints.Count > 0)
                {
                    menuBuilder.Add("Attach Particle Effect", new AddObjectParticleEffectCommandFactory(app, this), app.DefaultCommandClickHandler);
                }
                menuBuilder.Add("Add Sound", new AddSoundCommandFactory(app, this), app.DefaultCommandClickHandler);
                menuBuilder.AddDropDown("View From");
                menuBuilder.Add("Above", new DirectionAndObject(CameraDirection.Above, this), app.CameraObjectDirClickHandler);
                menuBuilder.Add("North", new DirectionAndObject(CameraDirection.North, this), app.CameraObjectDirClickHandler);
                menuBuilder.Add("South", new DirectionAndObject(CameraDirection.South, this), app.CameraObjectDirClickHandler);
                menuBuilder.Add("West", new DirectionAndObject(CameraDirection.West, this), app.CameraObjectDirClickHandler);
                menuBuilder.Add("East", new DirectionAndObject(CameraDirection.East, this), app.CameraObjectDirClickHandler);
                menuBuilder.FinishDropDown();
                menuBuilder.Add("Drag Object", new DragObjectsFromMenuCommandFactory(app), app.DefaultCommandClickHandler);
                menuBuilder.AddDropDown("Move to Collection", menuBuilder.ObjectCollectionDropDown_Opening);
                menuBuilder.FinishDropDown();
                menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
                menuBuilder.Add("Help", "Object", app.HelpClickHandler);
                menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                //             menuBuilder.Add("Generate Model Paths", new GenerateModelPathsCommandFactory(this), app.DefaultCommandClickHandler);
                node.ContextMenuStrip = menuBuilder.Menu;

                foreach (IWorldObject child in children)
                {
                    child.AddToTree(node);
                }
                buttonBar = menuBuilder.ButtonBar;
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
                return true;
            }
        }

        [CategoryAttribute("Miscellaneous"), DescriptionAttribute("Sets if the object may be selected in the world view")]
        public bool WorldViewSelectable
        {
            get
            {
                return worldViewSelectable;
            }
            set
            {
                worldViewSelectable = value;
            }
        }

        public void Clone(IWorldContainer copyParent)
        {
            StaticObject clone = new StaticObject(name, copyParent, app, meshName, Position, scale, rotation);
            clone.ReceiveShadows = receiveShadows;
            clone.CastShadows = castShadows;
            clone.AllowAdjustHeightOffTerrain = allowAdjustHeightOffTerrain;
            clone.SubMeshes = new SubMeshCollection(this.SubMeshes);
            clone.NameValue = new NameValueObject(this.NameValue);
            clone.TerrainOffset = terrainOffset;
            foreach (IWorldObject child in children)
            {
                child.Clone(clone);
            }
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", this.NodeName);
                objString += String.Format("\tAllowAdjustHeightOffTerrain={0}\r\n", AllowAdjustHeightOffTerrain);
                objString += String.Format("\tWorldViewSelectable = {0}\r\n", worldViewSelectable.ToString());
                objString += String.Format("\tAcceptObjectPlacement = {0}\r\n", acceptObjectPlacement.ToString());
                objString += String.Format("\tPerceptionRadius={0}\r\n", PerceptionRadius);
                objString += String.Format("\tMeshName={0}\r\n", MeshName);
                objString += String.Format("\tCastShadows={0}\r\n", castShadows);
                objString += String.Format("\tPosition=({0},{1},{2})\r\n", location.x, location.y, location.z);
                objString += String.Format("\tOrientation=({0},{1},{2},{3})\r\n", orientation.w, orientation.x, orientation.y, orientation.z);
                objString += String.Format("\tTargetable={0}\r\n", targetable);
                //objString += String.Format("\tReceiveShadows={0}", recieveShadows);
                objString += "\r\n";
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
            if (node != null && parentNode != null)
            {
                if (node != null && node.IsSelected)
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

        [BrowsableAttribute(true), DescriptionAttribute("This allows you to set the distance at which the object can be seen, in millimeters. 0 means it will be displayed out to the normal perception radius of the player."), CategoryAttribute("Display Propeties")]
        public float PerceptionRadius
        {
            get
            {
                return perceptionRadius;
            }
            set
            {
                perceptionRadius = value;
            }
        }

        public void AddToScene()
        {
            if (!inScene)
            {
                if (!offsetFound)
                {
                    terrainOffset = location.y - app.GetTerrainHeight(location.x, location.z);
                    offsetFound = true;
                }
                inScene = true;
                displayObject = new DisplayObject(this, name, app, "StaticObject", app.Scene, meshName, location, scale, orientation, subMeshes);
                displayObject.TerrainOffset = this.terrainOffset;
                displayObject.Highlight = highlight;
                displayObject.CastShadows = castShadows;

                // Create the list of triangles used to query mouse hits
                if (displayObject.Entity.Mesh.TriangleIntersector == null)
                    displayObject.Entity.Mesh.CreateTriangleIntersector();
            }
            foreach (IWorldObject child in children)
            {
                child.AddToScene();
                if (child is ParticleEffect)
                {
                    (child as ParticleEffect).Orientation = orientation;
                }
            }
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            foreach (IWorldObject child in children)
            {
                child.UpdateScene(type, hint);
            }
            if ((type == UpdateTypes.All || type == UpdateTypes.Object) && hint == UpdateHint.TerrainUpdate && allowAdjustHeightOffTerrain)
            {
                this.location.y = app.GetTerrainHeight(location.x, location.z) + terrainOffset;
            }
        }

        public void RemoveFromScene()
        {
            if (inScene)
            {
                inScene = false;
                foreach (IWorldObject child in children)
                {
                    child.RemoveFromScene();
                }

                displayObject.Dispose();
                displayObject = null;
            }
        }

        public void CheckAssets()
        {
            if (!app.CheckAssetFileExists(meshName))
            {
                app.AddMissingAsset(meshName);
            }

            foreach (IWorldObject child in children)
            {
                child.CheckAssets();
            }
        }

        protected void MaybeGeneratePathData() 
        {
            PathObjectTypeContainer pathObjectTypes = WorldRoot.Instance.PathObjectTypes;
            if (pathObjectTypes.Count == 0)
                PathData = null;
            else if (WorldRoot.Instance.PathObjectTypes.AllObjectsDirty || locationDirty) {
                List<CollisionShape> meshShapes = WorldEditor.Instance.FindMeshCollisionShapes(meshName, displayObject.Entity);
                if (meshShapes.Count == 0)
                    PathData = null;
                else {
                    PathData = new PathData();
                    float terrainHeight = WorldEditor.Instance.GetTerrainHeight(location.x, location.z) - location.y;
                    for (int i=0; i<pathObjectTypes.Count; i++) {
                        PathObjectType type = pathObjectTypes.GetType(i);
                        try {
                            PathData.AddModelPathObject(WorldEditor.Instance.LogPathGeneration, type, name, location,
                                displayObject.SceneNode.FullTransform, meshShapes, terrainHeight);
                        }
                        catch (Exception e) {
                            MessageBox.Show(string.Format("An exception was raised when generating pathing information for " +
                                    "model {0}, for path object type {1}.  The exception message was '{2}'.  Please run the World " +
                                    "Editor again, with the command-line option --log_paths, save the world file to generate " +
                                    "pathing information, and zip up the generated file " +
                                    "PathGenerationLog.txt, found in the World Editor's bin directory, and send it to Multiverse, " +
                                    "along with the {3} file so the problem can be identified and fixed.",
                                    name, type.name, e.Message, meshName),
                                "Error Generating Path Information For Model " + name + "!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        public void ToXml(XmlWriter w)
		{
			MaybeGeneratePathData();
            w.WriteStartElement("StaticObject");
            w.WriteAttributeString("Name", this.Name);
            w.WriteAttributeString("Mesh", this.meshName);
            w.WriteAttributeString("TerrainOffest", this.terrainOffset.ToString());
            w.WriteAttributeString("AllowHeightAdjustment", this.AllowAdjustHeightOffTerrain.ToString());
            w.WriteAttributeString("WorldViewSelect", worldViewSelectable.ToString());
            w.WriteAttributeString("AcceptObjectPlacement", acceptObjectPlacement.ToString());
            w.WriteAttributeString("PerceptionRadius", this.perceptionRadius.ToString());
            w.WriteAttributeString("CastShadows", this.castShadows.ToString());
            w.WriteAttributeString("Azimuth", this.azimuth.ToString());
            w.WriteAttributeString("Zenith", this.zenith.ToString());
            w.WriteAttributeString("Targetable", this.targetable.ToString());
            //w.WriteAttributeString("ReceiveShadows", this.receiveShadows.ToString());
			w.WriteStartElement("Position");
			w.WriteAttributeString("x", this.Position.x.ToString());
			w.WriteAttributeString("y", this.Position.y.ToString());
			w.WriteAttributeString("z", this.Position.z.ToString());
			w.WriteEndElement(); // Position end
			w.WriteStartElement("Scale");
			w.WriteAttributeString("x", this.scale.x.ToString());
			w.WriteAttributeString("y", this.scale.y.ToString());
			w.WriteAttributeString("z", this.scale.z.ToString());
			w.WriteEndElement(); // Scale end
            w.WriteStartElement("Orientation");
            w.WriteAttributeString("x", Orientation.x.ToString());
            w.WriteAttributeString("y", Orientation.y.ToString());
            w.WriteAttributeString("z", Orientation.z.ToString());
            w.WriteAttributeString("w", Orientation.w.ToString());
            w.WriteEndElement(); // Orientation end
            //w.WriteStartElement("Rotation");
            //w.WriteAttributeString("x", this.rotation.x.ToString());
            //w.WriteAttributeString("y", this.rotation.y.ToString());
            //w.WriteAttributeString("z", this.rotation.z.ToString());
            //w.WriteEndElement(); // Rotation end
			subMeshes.ToXml(w);
			nameValuePairs.ToXml(w);
            if (pathData != null)
                pathData.ToXml(w);
            foreach (IWorldObject child in children)
            {
                child.ToXml(w);
            }
			w.WriteEndElement(); // StaticObject end;
        }

        [BrowsableAttribute(false)]
		public float Rotation
		{
			get
			{
				return rotation.y;
			}
			set
			{
                if (value != rotation.y)
                {
                    locationDirty = true;
                    rotation = new Vector3(0, value, 0);
                    if (inScene)
                    {
                        displayObject.SetRotation(value);
                    }
                }
			}
		}

		[BrowsableAttribute(false)]
		public float Scale
		{
			get
			{
				return scale.y;
			}
			set
			{
                if (scale.x != value)
                {
                    locationDirty = true;
                    scale = new Vector3(value, value, value);
                    if (inScene)
                    {
                        displayObject.Scale = scale;
                    }
                }
			}
		}

        [BrowsableAttribute(false)]
        public Vector3 FocusLocation
        {
            get
            {
                return Position;
            }
        }

        [BrowsableAttribute(false)]
        public bool Highlight
        {
            get
            {
                return highlight;
            }
            set
            {
                highlight = value;
                if (displayObject != null)
                {
                    displayObject.Highlight = highlight;
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

        [DescriptionAttribute("The name of this object."), CategoryAttribute("Miscellaneous")]
        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                locationDirty = true;
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


        [BrowsableAttribute(true), DescriptionAttribute("This shows the mesh name of the model"), CategoryAttribute("Display Propeties")]
        public string MeshName
        {
            get
            {
                return meshName;
            }
        }

        [EditorAttribute(typeof(SubmeshUITypeEditor),typeof(System.Drawing.Design.UITypeEditor)),
      DescriptionAttribute("Which submeshes of a model are displayed. Click [...] to view the submeshes and assign different materials to them."), CategoryAttribute("Display Propeties")]
        public SubMeshCollection SubMeshes
        {
            get
            {
                return subMeshes;
            }
            set
            {
                subMeshes = value;
                if (inScene)
                {
                    displayObject.SubMeshCollection = subMeshes;
                }
            }
        }

		[EditorAttribute(typeof(NameValueUITypeEditorObject), typeof(System.Drawing.Design.UITypeEditor)),
DescriptionAttribute("Arbitrary Name/Value pair used to pass information about an object to server scripts and plug-ins. Click [...] to add or edit name/value pairs."), CategoryAttribute("Miscellaneous")]
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

        public void ToManifest(System.IO.StreamWriter w)
        {
            w.WriteLine("Mesh:{0}", meshName);

            // output any materials that have been set manually
            subMeshes.ToManifest(w, meshName);

            if (children != null)
            {
                foreach (IWorldObject child in children)
                {
                    child.ToManifest(w);
                }
            }
        }

        #endregion IWorldObject


        #region IObjectPosition
        [BrowsableAttribute(false)]
        public bool AllowYChange
        {
            get
            {
                return true;
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

        [BrowsableAttribute(false)]
        public Vector3 Position
        {
            get
            {
                return location;
            }
            set
            {
                Vector3 position = value;
                terrainOffset = position.y - app.GetTerrainHeight(position.x, position.z);
                if ((location - value).LengthSquared > 0.0001f)
                {
                    locationDirty = true;
                    location = value;
                    if (inScene)
                    {
                        displayObject.Position = location;
                        displayObject.TerrainOffset = terrainOffset;
                    }
                }
            }
        }

        [DescriptionAttribute("If true allows other objects to be placed on the object."), CategoryAttribute("Miscellaneous")]
        public bool AcceptObjectPlacement
        {
            get
            {
                return acceptObjectPlacement;
            }
            set
            {
                acceptObjectPlacement = value;
            }
        }

        [BrowsableAttribute(false)]
        public float TerrainOffset
        {
            get
            {
                return terrainOffset;
            }
            set
            {
                terrainOffset = value;
                if (inScene)
                {
                    this.DisplayObject.TerrainOffset = terrainOffset;
                }
            }
        }

        #endregion IObjectPosition


        #region IDisposable Members

        public void Dispose()
        {
            if (displayObject != null)
            {
                displayObject.Dispose();
                displayObject = null;
            }
        }

        #endregion
        #region IObjectDrag Members


        [BrowsableAttribute(true), DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "Object";
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

        #endregion IObjectDrag Members

        #region ICollection<IWorldObject> Members

        public void Add(IWorldObject item)
        {
            children.Add(item);
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
                return children.Count;
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

        [BrowsableAttribute(false)]
        public bool LocationDirty
        {
            get
            {
                return locationDirty;
            }
            set
            {
                locationDirty = value;
            }
        }

        [BrowsableAttribute(false)] 
        public PathData PathData
        {
            get
            {
                return pathData;
            }
            set
            {
                pathData = value;
            }
        }


        [BrowsableAttribute(false)] 
        public float MeshHeight
        {
            get
            {
                return 0f;
            }
        }

        public bool Remove(IWorldObject item)
        {
            item.RemoveFromTree();
            item.RemoveFromScene();
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

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IObjectOrientation Members
        [DescriptionAttribute("The Orientation of this marker in the world"), BrowsableAttribute(false)]
        public Quaternion Orientation
        {
            get
            {
                return orientation;
            }
        }

        public void SetDirection(float lightAzimuth, float lightZenith)
        {
            this.azimuth = lightAzimuth;
            this.zenith = lightZenith;

            UpdateOrientation();
        }

        [BrowsableAttribute(false)]
        public float Azimuth
        {
            get
            {
                return azimuth;
            }
            set
            {
                azimuth = value;
                UpdateOrientation();
            }
        }

        [BrowsableAttribute(false)]
        public float Zenith
        {
            get
            {
                return zenith;
            }
            set
            {
                zenith = value;
                UpdateOrientation();
            }
        }

        protected void UpdateOrientation()
        {
            Quaternion azimuthRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(azimuth), Vector3.UnitY);
            Quaternion zenithRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(-zenith), Vector3.UnitX);
            Quaternion displayZenithRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(-Zenith + 90), Vector3.UnitX);
            orientation = azimuthRotation * displayZenithRotation;

            if (inScene)
            {
                this.displayObject.SetOrientation(orientation);
            }

            foreach (IWorldObject child in children)
            {
                if (child is ParticleEffect)
                {
                    (child as ParticleEffect).Orientation = this.orientation;
                }
            }
        }

        #endregion IObjectOrientation Members

    }

    public enum CameraDirection
    {
        Above,
        North,
        South,
        East,
        West
    }

    /// <summary>
    /// This class is used to hold a direction and object, and set a camera based on them.
    /// </summary>
    public class DirectionAndObject
    {
        CameraDirection dir;
        StaticObject obj;

        public DirectionAndObject(CameraDirection dir, StaticObject obj)
        {
            this.dir = dir;
            this.obj = obj;
        }

        public void SetCamera(Camera cam)
        {
            obj.SetCamera(cam, dir);
        }
    }
}
