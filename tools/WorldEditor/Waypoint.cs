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
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Multiverse.ToolBox;


namespace Multiverse.Tools.WorldEditor
{
	/// <summary>
	/// Summary description for Waypoint
	/// </summary>
    public class Waypoint : IWorldObject, IWorldContainer, IObjectPosition, IObjectOrientation, IObjectDrag, IObjectChangeCollection, IObjectDelete, IObjectCutCopy, IObjectCameraLockable
    {
        protected IWorldContainer parent;
        protected WorldEditor app;
        protected string name;
        protected Quaternion orientation;
        protected DisplayObject disp;
        protected NameValueObject nameValuePairs;
        protected bool highlight = false;
        protected Vector3 position;
        protected WorldTreeNode node = null;
        protected WorldTreeNode parentNode = null;
        protected List<IWorldObject> children;
        protected Vector3 direction;
        protected Vector3 rotation;
        protected float azimuth;
        protected float zenith;
        protected float terrainOffset;
        protected bool allowAdjustHeightOffTerrain = true;
        protected bool offsetFound = false;
        protected bool updating = false;
        

        protected bool inScene = false;
        protected bool inTree = false;
        protected bool worldViewSelectable = true;
        protected bool customColor = false;
        protected string customMaterialName;
        protected ColorEx color = ColorEx.Black;

        const float defaultScale = 4f;
        const float defaultRot = 0f;

        //protected string soundAssetName = null;

        protected List<ToolStripButton> buttonBar;
        
        public Waypoint(string name, IWorldContainer parent, WorldEditor app, Vector3 location, Vector3 rotation)
        {
            this.parent = parent;
            this.app = app;
            this.position = location;
            this.name = name;
            this.nameValuePairs = new NameValueObject();
            this.children = new List<IWorldObject>();
            this.orientation = new Quaternion(0, 0, 0, 0);
//            this.orientation = Quaternion.FromAngleAxis(defaultRot * MathUtil.RADIANS_PER_DEGREE, Vector3.UnitY);
            SetDirection(rotation.y,90);
        }

        public Waypoint(string name, IWorldContainer parent, WorldEditor app, Vector3 location, Quaternion orientation)
        {
            this.azimuth = 0f;
            this.zenith = 0f;
            this.name = name;
            this.app = app;
            this.orientation = new Quaternion(orientation.w, orientation.x, orientation.y, orientation.z);
            this.position = location;
            this.parent = parent;
            this.nameValuePairs = new NameValueObject();
            this.children = new List<IWorldObject>();
        }

        public Waypoint(XmlReader r, IWorldContainer parent, WorldEditor app)
        {
            this.parent = parent;
            this.app = app;
            this.children = new List<IWorldObject>();
            FromXml(r);

            if (nameValuePairs == null)
            {
                nameValuePairs = new NameValueObject();
            }
        }

        protected void FromXml(XmlReader r)
        {
            // first parse the attributes

            bool adjustHeightFound = false;
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "Name":
                        this.name = r.Value;
                        break;
                    //case "Sound":
                    //    this.soundAssetName = r.Value;
                    //    break;
                    case "TerrainOffset":
                        offsetFound = true;
                        terrainOffset = float.Parse(r.Value);
                        break;
                    case "AllowHeightAdjustment":
                        adjustHeightFound = true;
                        if (String.Equals(r.Value.ToLower(), "false"))
                        {
                            allowAdjustHeightOffTerrain = false;
                        }
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
                }
            }
//            this.nameValuePairs = new NameValueObject();
            r.Read();
            do
            {
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                if (r.NodeType == XmlNodeType.Element)
                {
                    switch (r.Name)
                    {
                        case "Position":
                            this.position = XmlHelperClass.ParseVectorAttributes(r);
                            break;
                        case "Orientation":
                            orientation = XmlHelperClass.ParseQuaternion(r);
                            break;
                        case "NameValuePairs":
                            this.nameValuePairs = new NameValueObject(r);
                            break;
                        case "ParticleEffect":
                            ParticleEffect particle = new ParticleEffect(r, this, app);
                            Add(particle);
                            break;
                        case "SpawnGen":
                            SpawnGen mob = new SpawnGen(r, app, this);
                            Add(mob);
                            break;
                        case "Sound":
                            Sound sound = new Sound(r, this, app);
                            Add(sound);
                            break;
                        case "Color":
                            Color = XmlHelperClass.ParseColorAttributes(r);
                            break;
                    }
                }
                else if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            } while (r.Read());
            if (!adjustHeightFound)
            {
                allowAdjustHeightOffTerrain = true;
            }
            if (!offsetFound)
            {
                terrainOffset = this.Position.y - app.GetTerrainHeight(position.x, position.z);
            }
            
            if (orientation != null && disp != null)
            {
                disp.SetOrientation(orientation);
                foreach (IWorldObject obj in children)
                {
                    if (obj is ParticleEffect)
                    {
                        (obj as ParticleEffect).Orientation = this.orientation;
                    }
                }
            }
        }

        public void AddToScene()
        {
            if (app.DisplayMarkerPoints)
            {
                if (!offsetFound)
                {
                    float terrainHeight = app.GetTerrainHeight(Position.x, Position.z);
                    terrainOffset = position.y - terrainHeight;
                }
                Vector3 scaleVec = new Vector3(app.Config.MarkerPointScale, app.Config.MarkerPointScale, app.Config.MarkerPointScale);
                this.disp = new DisplayObject(this, name, app, "waypoint", app.Scene, app.Config.MarkerPointMeshName, position, scaleVec, this.orientation, null);
                this.disp.TerrainOffset = this.terrainOffset;
                if (customColor)
                {
                    this.disp.MaterialName = customMaterialName;
                }
                else
                {
                    this.disp.MaterialName = app.Config.MarkerPointMaterial;
                }

                foreach (IWorldObject child in children)
                {
                    child.AddToScene();
                    if (child is ParticleEffect)
                    {
                        (child as ParticleEffect).Orientation = orientation;
                    }
                }
                if (disp.Entity.Mesh.TriangleIntersector == null)
                    disp.Entity.Mesh.CreateTriangleIntersector();
                
                inScene = true;
            }
            else
            {
                foreach (IWorldObject child in children)
                {
                    if (app.DisplayParticleEffects && child is ParticleEffect && app.WorldRoot != null)
                    {
                        child.AddToScene();
                        (child as ParticleEffect).Orientation = orientation;
                    }
                }
            }
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            foreach (IWorldObject child in children)
            {
                child.UpdateScene(type, hint);
            }
            if ((type == UpdateTypes.Markers || type == UpdateTypes.All) && hint == UpdateHint.DisplayMarker)
            {
                if (inScene == app.DisplayMarkerPoints)
                {
                    return;
                }
                else
                {
                    if (inScene)
                    {
                        this.updating = true;
                        this.RemoveFromScene();
                        this.updating = false;
                    }
                    else
                    {
                        this.AddToScene();
                    }
                }
            }
            if ((type == UpdateTypes.All || type == UpdateTypes.Markers) && hint == UpdateHint.TerrainUpdate && allowAdjustHeightOffTerrain)
            {
                this.position.y = app.GetTerrainHeight(position.x, position.z) + terrainOffset;
            }
        }

        public void RemoveFromScene()
        {
            if (inScene)
            {
                inScene = false;
                disp.Dispose();
                disp = null;
            }
            foreach (IWorldObject child in children)
            {
                if (child is ParticleEffect && app.DisplayParticleEffects && updating && (parent as WorldObjectCollection).InScene)
                {
                    continue;
                }
                child.RemoveFromScene();
            }
        }

        [DescriptionAttribute("The name of this marker."), CategoryAttribute("Miscellaneous")]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;

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


        [DescriptionAttribute("The Rotation around the Y Axis(runs verticle) of this marker in the world")]
        [BrowsableAttribute(false)]
        public float Rotation
        {
            get
            {
                return rotation.y;
            }
            set
            {
                rotation = new Vector3(0, value, 0);
                if (inScene)
                {
                    disp.SetRotation(value);
                }
            }
        }


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
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                terrainOffset = position.y - app.GetTerrainHeight(position.x, position.z);
                if (inScene)
                {
                    disp.Position = position;
                    disp.TerrainOffset = terrainOffset;
                }
                foreach (IWorldObject child in children)
                {
                    if (child is ParticleEffect)
                    {
                        ParticleEffect particle = child as ParticleEffect;
                        particle.PositionUpdate();
                    }
                }
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
                    disp.TerrainOffset = terrainOffset;
                }
            }
        }

        #endregion IObjectPosition

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
                if (disp != null)
                {
                    disp.Highlight = highlight;
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


        [EditorAttribute(typeof(NameValueUITypeEditorMarker), typeof(System.Drawing.Design.UITypeEditor)),
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

        [EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)),
DescriptionAttribute("Color of this Marker Point. (click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Colors")]
        public ColorEx Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                Material eMat;
                if (!customColor)
                {
                    customColor = true;
                    Material mat = MaterialManager.Instance.GetByName(app.Config.MarkerPointCustomMaterial);
                    customMaterialName = Guid.NewGuid().ToString();
                    eMat = mat.Clone(customMaterialName);
                    eMat.Load();
                }
                else
                {
                    eMat = MaterialManager.Instance.GetByName(customMaterialName);
                }
                eMat.GetBestTechnique().GetPass(0).GetTextureUnitState(0).SetColorOperationEx(
                    LayerBlendOperationEx.Modulate, LayerBlendSource.Manual, LayerBlendSource.Diffuse, color);
                if (InScene)
                {
                    disp.MaterialName = customMaterialName;
                }
            }
        }

        public void AddToTree(WorldTreeNode parentNode)
        {
            if (parentNode != null && !inTree)
            {
                this.parentNode = parentNode;
                inTree = true;

                // create a node for the collection and add it to the parent
                node = app.MakeTreeNode(this, NodeName);
                parentNode.Nodes.Add(node);

                // build the menu
                CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
                menuBuilder.Add("Attach Particle Effect", new AddWaypointParticleEffectCommandFactory(app, this), app.DefaultCommandClickHandler);
                menuBuilder.Add("Add Spawn Generator", new AddSpawnGenToMarkerCommandFactory(app, this), app.DefaultCommandClickHandler);
                menuBuilder.Add("Add Sound", new AddSoundCommandFactory(app, this), app.DefaultCommandClickHandler);
                menuBuilder.AddDropDown("View From");
                menuBuilder.Add("Above", new DirectionAndMarker(CameraDirection.Above, this), app.CameraMarkerDirClickHandler);
                menuBuilder.Add("North", new DirectionAndMarker(CameraDirection.North, this), app.CameraMarkerDirClickHandler);
                menuBuilder.Add("South", new DirectionAndMarker(CameraDirection.South, this), app.CameraMarkerDirClickHandler);
                menuBuilder.Add("West", new DirectionAndMarker(CameraDirection.West, this), app.CameraMarkerDirClickHandler);
                menuBuilder.Add("East", new DirectionAndMarker(CameraDirection.East, this), app.CameraMarkerDirClickHandler);
                menuBuilder.FinishDropDown();
                menuBuilder.Add("Drag Marker", new DragObjectsFromMenuCommandFactory(app), app.DefaultCommandClickHandler);
                menuBuilder.AddDropDown("Move to Collection", menuBuilder.ObjectCollectionDropDown_Opening);
                menuBuilder.FinishDropDown();
                menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
                menuBuilder.Add("Help", "Marker", app.HelpClickHandler);
                menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                node.ContextMenuStrip = menuBuilder.Menu;

                foreach (IWorldObject child in children)
                {
                    child.AddToTree(node);
                }
                buttonBar = menuBuilder.ButtonBar;
            }
            else
            {
                inTree = false;
            }
        }

        [CategoryAttribute("Miscellaneous"), DescriptionAttribute("Sets if the marker may be selected in the world view")]
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

        public void Clone(IWorldContainer copyParent)
        {
            Waypoint clone = new Waypoint(name, copyParent, app, position, rotation);
            clone.Azimuth = azimuth;
            clone.Zenith = zenith;
            clone.TerrainOffset = terrainOffset;
            clone.AllowAdjustHeightOffTerrain = allowAdjustHeightOffTerrain;
            clone.NameValue = new NameValueObject(this.NameValue);
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
                string objString = String.Format("Name:{0}\r\n", NodeName);
                objString += String.Format("\tAllowAdjustHeightOffTerrain={0}\r\n", AllowAdjustHeightOffTerrain);
                objString += String.Format("\tWorldViewSelectable={0}", worldViewSelectable);
                objString += String.Format("\tPosition=({0},{1},{2})\r\n", position.x, position.y, position.z);
                objString += String.Format("\tOrientation=({0},{1},{2},{3})\r\n", orientation.w, orientation.x, orientation.y, orientation.z);
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
        public bool InScene
        {
            get
            {
                return inScene;
            }
        }

        [CategoryAttribute("Miscellaneous"), BrowsableAttribute(true), DescriptionAttribute("Whether the object preserves the height above terrain when the terrain is changed or the object is dragged or pasted to another location.")]
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
                node = null;
                parentNode = null;

                inTree = false;
            }
        }

        public void CheckAssets()
        {
            if (!app.CheckAssetFileExists(app.Config.MarkerPointMeshName))
            {
                app.AddMissingAsset(app.Config.MarkerPointMeshName);
            }
            string textureFile = "directional_marker_orange.dds";
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

        public void ToXml(XmlWriter w)
        {
            w.WriteStartElement("Waypoint");
            w.WriteAttributeString("Name", name);
            //w.WriteAttributeString("Sound", soundAssetName);
            w.WriteAttributeString("TerrainOffset", terrainOffset.ToString());
            w.WriteAttributeString("AllowHeightAdjustment", this.AllowAdjustHeightOffTerrain.ToString());
            w.WriteAttributeString("WorldViewSelect", worldViewSelectable.ToString());
            w.WriteAttributeString("Azimuth", azimuth.ToString());
            w.WriteAttributeString("Zenith", zenith.ToString());
            w.WriteStartElement("Position");
            w.WriteAttributeString("x", Position.x.ToString());
            w.WriteAttributeString("y", Position.y.ToString());
            w.WriteAttributeString("z", Position.z.ToString());
            w.WriteEndElement(); // Position end
            w.WriteStartElement("Orientation");
            w.WriteAttributeString("x", Orientation.x.ToString());
            w.WriteAttributeString("y", Orientation.y.ToString());
            w.WriteAttributeString("z", Orientation.z.ToString());
            w.WriteAttributeString("w", Orientation.w.ToString());
            w.WriteEndElement(); // Orientation end
            if (customColor)
            {
                w.WriteStartElement("Color");
                w.WriteAttributeString("R", this.color.r.ToString());
                w.WriteAttributeString("G", this.color.g.ToString());
                w.WriteAttributeString("B", this.color.b.ToString());
                w.WriteEndElement(); // End Color
            }

            if (this.nameValuePairs != null && this.nameValuePairs.Count > 0)
            {
                nameValuePairs.ToXml(w);
            }
            foreach (IWorldObject child in children)
            {
                child.ToXml(w);
            }
            w.WriteEndElement(); // Waypoint end
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
            if (disp != null)
            {
                disp.Dispose();
                disp = null;
            }
        }

        #region ICollection<IWorldObject> Members

        public void Add(IWorldObject item)
        {
            children.Add(item);
            if (inTree)
            {
                item.AddToTree(node);
            }
            if (item is ParticleEffect && app.DisplayParticleEffects && app.WorldRoot != null && !(parent is PasteFromClipboardCommand) && !(parent is ClipboardObject) && (parent as WorldObjectCollection).InScene)
            {
                item.AddToScene();
            }
            else
            {
                if (inScene)
                {
                    item.AddToScene();
                }
            }
        }

        public void Clear()
        {
            children.Clear();
        }

        public bool Contains(IWorldObject item)
        {
            return children.Contains(item as ParticleEffect);
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

        public bool Remove(IWorldObject item)
        {
            if (inTree)
            {
                item.RemoveFromTree();
            }
            if (inScene || (item is ParticleEffect && app.DisplayParticleEffects))
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

        #region IOjbectDrag Members

        [BrowsableAttribute(false)]
        public DisplayObject Display
        {
            get
            {
                return disp;
            }
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "Marker";
            }
        }

        [BrowsableAttribute(false)]
        public float Radius
        {
            get
            {
                if (inScene && disp != null)
                {
                    return disp.Radius;
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
                if (inScene && disp != null)
                {
                    return Display.Center;
                }
                else
                {
                    return Vector3.Zero;
                }
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

        #endregion IOjbectDrag Members

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
                
                this.disp.SetOrientation(orientation);
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
        public void SetCamera(Camera cam, CameraDirection dir)
        {
            AxisAlignedBox boundingBox = disp.BoundingBox;

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
    }

    /// <summary>
    /// This class is used to hold a direction and object, and set a camera based on them.
    /// </summary>
    public class DirectionAndMarker
    {
        CameraDirection dir;
        Waypoint obj;

        public DirectionAndMarker(CameraDirection dir, Waypoint obj)
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
