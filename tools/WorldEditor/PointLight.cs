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
using System.ComponentModel;
using System.Text;
using System.Xml;
using Axiom.Core;
using Axiom.MathLib;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{
    class PointLight : IWorldObject, IObjectDrag, IObjectPosition, IObjectChangeCollection, IObjectDelete, IObjectCutCopy, IObjectCameraLockable
    {
        protected WorldEditor app;
        protected IWorldContainer parent;
        protected SceneManager scene;
        protected bool inScene = false;
        protected bool inTree = false;
        protected String name;
        protected Light pLight;
        protected ColorEx specular;
        protected ColorEx diffuse;
        protected Vector3 position;
        protected DisplayObject displayObject = null;
        protected WorldTreeNode parentNode;
        protected WorldTreeNode node;
        protected bool highlight = false;
        protected float attenuationRange = 1000000;
        protected float attenuationConstant = 1;
        protected float attenuationLinear = 0.00001f;
        protected float attenuationQuadratic = 0;
        protected List<ToolStripButton> buttonBar;
        protected float terrainOffset;
        protected bool allowAdjustHeightOffTerrain = true;
        protected bool foundOffset = false;
        protected bool worldViewSelectable = true;
        protected bool showCircles = false;
        protected DisplayObject halfCircleObject = null;
        protected DisplayObject quarterCircleObject = null;
        protected DisplayObject maxCircleObject = null;

        public PointLight(WorldEditor worldEditor, IWorldContainer parent, SceneManager scene, string name, ColorEx specular, ColorEx diffuse, Vector3 position)
        {
            this.app = worldEditor;
            this.parent = parent;
            this.scene = scene;
            this.name = name;
            this.position = position;
            this.specular = specular;
            this.diffuse = diffuse;
            this.terrainOffset = app.Config.DefaultPointLightHeight;
        }

        public PointLight(WorldEditor worldEditor, IWorldContainer parent, SceneManager scene, XmlReader r)
        {
            this.app = worldEditor;
            this.parent = parent;
            this.scene = scene;
            fromXML(r);
        }

        [EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)), 
        DescriptionAttribute("Specular Color of this Light. (Click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Colors")]
        public ColorEx Specular
        {
            get
            {
                return specular;
            }
            set
            {
                specular = value;
                if (inScene)
                {
                    pLight.Specular = specular;
                }
            }
        }


        [EditorAttribute(typeof(ColorValueUITypeEditor), typeof(System.Drawing.Design.UITypeEditor)), 
        DescriptionAttribute("Diffuse Color of this Light. (Click [...] to use the color picker dialog to select a color)."), CategoryAttribute("Colors")]
        public ColorEx Diffuse
        {
            get
            {
                return diffuse;
            }
            set
            {
                diffuse = value;
                if (inScene)
                {
                    pLight.Diffuse = diffuse;
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

        protected void UpdateAttenuation()
        {
            if (inScene)
            {
                pLight.SetAttenuation(attenuationRange, attenuationConstant, attenuationLinear, attenuationQuadratic);
                UpdateShowCircles();
                
            }
            app.UpdatePropertyGrid();
        }

        [DescriptionAttribute("Maximum distance (in millimeters) that this light will affect. See documentation for further information"),
        CategoryAttribute("Light Attenuation"), BrowsableAttribute(true)]
        public float AttenuationRange
        {
            get
            {
                return attenuationRange;
            }
            set
            {
                attenuationRange = value;
                UpdateAttenuation();
            }
        }

        [DescriptionAttribute("Constant attenuation term."), CategoryAttribute("Light Attenuation")]
        public float AttenuationConstant
        {
            get
            {
                return attenuationConstant;
            }
            set
            {
                attenuationConstant = value;
                UpdateAttenuation();
            }
        }

        [DescriptionAttribute("Linear attenuation term."), CategoryAttribute("Light Attenuation")]
        public float AttenuationLinear
        {
            get
            {
                return attenuationLinear;
            }
            set
            {
                attenuationLinear = value;
                UpdateAttenuation();
            }
        }

        [DescriptionAttribute("Quadratic attenuation term."), CategoryAttribute("Light Attenuation")]
        public float AttenuationQuadratic
        {
            get
            {
                return attenuationQuadratic;
            }
            set
            {
                attenuationQuadratic = value;
                UpdateAttenuation();
            }
        }

        [DescriptionAttribute("The Name of this PointLight"), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
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

        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            if (parentNode != null)
            {
                this.parentNode = parentNode;
                inTree = true;

                // create a node for the collection and add it to the parent
                this.node = app.MakeTreeNode(this, NodeName);
                parentNode.Nodes.Add(node);

                // build the menu
                CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
                menuBuilder.Add("Drag Point Light", new DragObjectsFromMenuCommandFactory(app), app.DefaultCommandClickHandler);
                menuBuilder.AddDropDown("Move to Collection", menuBuilder.ObjectCollectionDropDown_Opening);
                menuBuilder.FinishDropDown();
                menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
                menuBuilder.Add("Help", "Point_Light", app.HelpClickHandler);
                menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent, this), app.DefaultCommandClickHandler);
                node.ContextMenuStrip = menuBuilder.Menu;
                buttonBar = menuBuilder.ButtonBar;
            }
            else
            {
                inTree = false;
            }
        }

        [CategoryAttribute("Miscellaneous"), DescriptionAttribute("Sets if the point light may be selected in the world view")]
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
            PointLight clone = new PointLight(app, copyParent, scene, name, specular, diffuse, position);
            clone.AttenuationConstant = attenuationConstant;
            clone.AttenuationLinear = attenuationLinear;
            clone.AttenuationQuadratic = attenuationQuadratic;
            clone.AttenuationRange = attenuationRange;
            clone.TerrainOffset = terrainOffset;
            clone.AllowAdjustHeightOffTerrain = allowAdjustHeightOffTerrain;
            copyParent.Add(clone);
        }

        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}\r\n", NodeName);
                objString += String.Format("\tAllowAdjustHeightOffTerrain={0}\r\n", AllowAdjustHeightOffTerrain);
                objString += String.Format("\tWorldViewSelectable = {0}", worldViewSelectable.ToString());
                objString += String.Format("\tSpecular:\r\n");
                objString += String.Format("\t\tR={0}\r\n", Specular.r);
                objString += String.Format("\t\tG={0}\r\n", Specular.g);
                objString += String.Format("\t\tB={0}\r\n", Specular.b);
                objString += String.Format("\tDiffuse:\r\n");
                objString += String.Format("\t\tR={0}\r\n", Diffuse.r);
                objString += String.Format("\t\tG={0}\r\n", Diffuse.g);
                objString += String.Format("\t\tB={0}\r\n", Diffuse.b);
                objString += String.Format("\tAttenuationRange={0}\r\n", AttenuationRange);
                objString += String.Format("\tAttenuationConstant={0}\r\n", AttenuationConstant);
                objString += String.Format("\tAttenuationLinear={0}\r\n", AttenuationLinear);
                objString += String.Format("\tAttenuationQuadratic={0}\r\n", AttenuationQuadratic);
                objString += String.Format("\tPosition:({0},{1},{2})", position.x, position.y, position.z);
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
                parentNode.Nodes.Remove(node);
                parentNode = null;
                node = null;
            }
            inTree = false;
        }

        public void AddToScene()
        {

            if (!foundOffset)
            {
                terrainOffset = position.y - app.GetTerrainHeight(Position.x, Position.z);
                foundOffset = true;
            }
            if (app.DisplayPointLightMarker)
            {
                this.DisplayPointLightMarker();
                if (displayObject.Entity.Mesh.TriangleIntersector == null)
                    displayObject.Entity.Mesh.CreateTriangleIntersector();
            }
            string uniqueName = WorldEditor.GetUniqueName(ObjectType, name);
            pLight = this.scene.CreateLight(uniqueName);
            pLight.Type = Axiom.Graphics.LightType.Point;
            pLight.Position = position;
            pLight.Specular = specular;
            pLight.Diffuse = diffuse;
            pLight.IsVisible = true;
            pLight.SetAttenuation(attenuationRange, attenuationConstant, attenuationLinear, attenuationQuadratic);
            inScene = true;
            if (app.DisplayPointLightCircles)
            {
                UpdateShowCircles();
            }
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            if ((type == UpdateTypes.PointLight || type == UpdateTypes.All) && hint == UpdateHint.DisplayMarker)
            {
                if (displayObject != null && app.DisplayPointLightMarker)
                {
                    return;
                }
                else
                {
                    if (displayObject != null)
                    {
                        this.RemovePointLightMarker();
                    }
                    else
                    {
                        this.DisplayPointLightMarker();
                    }
                }
            }
            if ((type == UpdateTypes.All || type == UpdateTypes.PointLight) && hint == UpdateHint.TerrainUpdate && allowAdjustHeightOffTerrain)
            {
                this.position.y = app.GetTerrainHeight(position.x, position.z) + terrainOffset;
            }
            if ((type == UpdateTypes.All || type == UpdateTypes.PointLight) && hint == UpdateHint.DisplayLight)
            {
                if (pLight.IsVisible && app.DisplayLights)
                {
                    return;
                }
                else
                {
                    if (pLight.IsVisible)
                    {
                        this.RemoveLightFromScene();
                    }
                    else
                    {
                        this.AddLightToScene();
                    }
                }
            }
            if (type == UpdateTypes.PointLight && hint == UpdateHint.DisplayPointLightCircles)
            {
                if (this.showCircles && app.DisplayPointLightCircles)
                {
                    return;
                }
                else
                {
                    UpdateShowCircles();
                }
            }
        }

        private void RemovePointLightMarker()
        {
            if (displayObject != null)
            {
                displayObject.Dispose();
                displayObject = null;
            }
        }

        private void DisplayPointLightMarker()
        {
            displayObject = new DisplayObject((IWorldObject)this, app, name, "Point Light", scene, app.Assets.assetFromName(app.Config.PointLightMeshName).AssetName, position, new Vector3(1, 1, 1), new Vector3(0, 0, 0), null);
            displayObject.TerrainOffset = this.terrainOffset;
        }

        private void RemoveLightFromScene()
        {
            if (pLight != null)
            {
                pLight.IsVisible = false;
                //scene.RemoveLight(pLight);
            }
        }

        private void AddLightToScene()
        {
            if (pLight == null || !pLight.IsVisible)
            {
                //pLight = this.scene.CreateLight(uniqueName);
                //pLight.Type = Axiom.Graphics.LightType.Point;
                //pLight.Position = position;
                //pLight.Specular = specular;
                //pLight.Diffuse = diffuse;
                pLight.IsVisible = true;
                //pLight.SetAttenuation(attenuationRange, attenuationConstant, attenuationLinear, attenuationQuadratic);
            }
        }

        public void RemoveFromScene()
        {
            if (inScene)
            {
                pLight.IsVisible = false;
                scene.RemoveLight(pLight);
                RemovePointLightMarker();
                inScene = false;
                UpdateShowCircles();
            }
        }

        public void CheckAssets()
        {
            string textureName;
            string materialName;
            CheckAsset(app.Assets.assetFromName(app.Config.PointLightMeshName).AssetName);
            materialName = "world_editor_point_light_marker.material";
            CheckAsset(materialName);
            textureName = "world_editor_point_light_marker.dds";
            CheckAsset(textureName);
            CheckAsset(app.Assets.assetFromName(app.Config.PointLightCircleMeshName).AssetName);
            materialName = "world_editor_light_ring.material";
            CheckAsset(materialName);
            textureName = "red-circle-gradient.png";
            CheckAsset(textureName);
            textureName = "yellow-circle-gradient.png";
            CheckAsset(textureName);
            textureName = "green-circle-gradient.png";
            CheckAsset(textureName);
        }

        protected void CheckAsset(string name)
        {
            if ((name != null) && (name != ""))
            {
                if (!app.CheckAssetFileExists(name))
                {
                    app.AddMissingAsset(name);
                }
            }
        }

        public void ToXml(XmlWriter w)
        {
            w.WriteStartElement("PointLight");
            w.WriteAttributeString("Name", name);
            w.WriteAttributeString("AttenuationRange", attenuationRange.ToString());
            w.WriteAttributeString("AttenuationConstant", attenuationConstant.ToString());
            w.WriteAttributeString("AttenuationLinear", attenuationLinear.ToString());
            w.WriteAttributeString("AttenuationQuadratic", attenuationQuadratic.ToString());
            w.WriteAttributeString("TerrainOffset", terrainOffset.ToString());
            w.WriteAttributeString("AllowHeightAdjustment", this.AllowAdjustHeightOffTerrain.ToString());
            w.WriteAttributeString("WorldViewSelect", worldViewSelectable.ToString());
            w.WriteStartElement("Position");
            w.WriteAttributeString("x", position.x.ToString());
            w.WriteAttributeString("y", position.y.ToString());
            w.WriteAttributeString("z", position.z.ToString());
            w.WriteEndElement(); // Position end
            w.WriteStartElement("Specular");
            w.WriteAttributeString("R", specular.r.ToString());
            w.WriteAttributeString("G", specular.g.ToString());
            w.WriteAttributeString("B", specular.b.ToString());
            w.WriteEndElement();
            w.WriteStartElement("Diffuse");
            w.WriteAttributeString("R", diffuse.r.ToString());
            w.WriteAttributeString("G", diffuse.g.ToString());
            w.WriteAttributeString("B", diffuse.b.ToString());
            w.WriteEndElement();
            w.WriteEndElement();
        }

        protected void fromXML(XmlReader r)
        {
            bool offsetFound = false;
            bool adjustHeightFound = false;

            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                switch (r.Name)
                {
                    case "Name":
                        name = r.Value;
                        break;
                    case "AttenuationRange":
                        attenuationRange = float.Parse(r.Value);
                        break;
                    case "AttenuationConstant":
                        attenuationConstant = float.Parse(r.Value);
                        break;
                    case "AttenuationLinear":
                        attenuationLinear = float.Parse(r.Value);
                        break;
                    case "AttenuationQuadratic":
                        attenuationQuadratic = float.Parse(r.Value);
                        break;
                    case "TerrainOffset":
                        offsetFound = true;
                        terrainOffset = float.Parse(r.Value);
                        offsetFound = true;
                        break;
                    case "AllowHeightAdjustment":
                        adjustHeightFound = true;
                        if (String.Equals(r.Value.ToLower(), "false"))
                        {
                            allowAdjustHeightOffTerrain = false;
                        }
                        break;
                    case "WorldViewSelect":
                        worldViewSelectable = bool.Parse(r.Value);
                        break;
                }
            }
            r.MoveToElement();
            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                switch (r.Name)
                {
                    case "Position":
                        position = XmlHelperClass.ParseVectorAttributes(r);
                        break;
                    case "Specular":
                        specular = XmlHelperClass.ParseColorAttributes(r);
                        break;
                    case "Diffuse":
                        diffuse = XmlHelperClass.ParseColorAttributes(r);
                        break;
                }
            }
            if (!adjustHeightFound)
            {
                allowAdjustHeightOffTerrain = true;
            }
            if (!offsetFound)
            {
                terrainOffset = position.y - app.GetTerrainHeight(position.x, position.z);
            }
        }

        [DescriptionAttribute("The radius in millimeters at which the light attenuates to half of its brightness."), CategoryAttribute("Light Attenuation")]
        public float HalfAttenuationRadius
        {
            get
            {
                if (attenuationQuadratic == 0)
                {
                    if (attenuationLinear == 0)
                    {
                        return 0f;
                    }
                    else
                    {
                        return 1f / attenuationLinear;
                    }
                }
                else
                {
                    return (float)((Math.Sqrt((Math.Pow(attenuationLinear, 2f) + (4 * attenuationQuadratic * (2 - attenuationConstant)))) - attenuationLinear ) / (2 * attenuationQuadratic));
                }
            }
            //set
            //{

            //}
        }

        [DescriptionAttribute("The radius in millimeters at which the light attenuates a quarter of its brightness."), CategoryAttribute("Light Attenuation")]
        public float QuarterAttenuationRadius
        {
            get
            {
                if (attenuationQuadratic == 0)
                {
                    if (attenuationLinear == 0)
                    {
                        return 0f;
                    }
                    else
                    {
                        return 3f / attenuationLinear;
                    }
                }
                return (float)((Math.Sqrt((Math.Pow(attenuationLinear, 2f) + (4 * attenuationQuadratic * (4 - attenuationConstant)))) - attenuationLinear ) / (2 * attenuationQuadratic));
            }
            //set
            //{

            //}
        }


        [BrowsableAttribute(false)]
        public Vector3 FocusLocation
        {
            get
            {
                return position;
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

        public void ToManifest(System.IO.StreamWriter w)
        {
        }

        #endregion

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

        [BrowsableAttribute(false)]
        public DisplayObject Display
        {
            get
            {
                return displayObject;
            }
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous")]
        public string ObjectType
        {
            get
            {
                return "PointLight";
            }
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
                    return this.displayObject.Center;
                }
                else
                {
                    return Vector3.Zero;
                }
            }
        }


        private void UpdateShowCircles()
        {
            if (app.DisplayPointLightCircles)
            {

                if (HalfAttenuationRadius < AttenuationRange)
                {
                    if (halfCircleObject == null)
                    {
                        halfCircleObject = new DisplayObject(String.Format("{0}-halfAttenuationCircle", this.Name), app, "PointLightCircle", scene, app.Assets.assetFromName(app.Config.PointLightCircleMeshName).AssetName, this.Position, Vector3.UnitScale, Vector3.Zero, null);
                        halfCircleObject.MaterialName = "world_editor_light_ring.lightRing.green";
                    }
                }
                else
                {
                    if (halfCircleObject != null)
                    {
                        halfCircleObject.Dispose();
                        halfCircleObject = null;
                    }
                }
                if (QuarterAttenuationRadius < AttenuationRange)
                {
                    if (quarterCircleObject == null)
                    {
                        quarterCircleObject = new DisplayObject(String.Format("{0}-quarterAttenutationCircle", this.Name), app, "PointLightCircle", scene, app.Assets.assetFromName(app.Config.PointLightCircleMeshName).AssetName, this.Position, Vector3.UnitScale, Vector3.Zero, null);
                        quarterCircleObject.MaterialName = "world_editor_light_ring.lightRing.yellow";
                    }
                }
                else
                {
                    if (quarterCircleObject != null)
                    {
                        quarterCircleObject.Dispose();
                        quarterCircleObject = null;
                    }
                }

                if (maxCircleObject == null)
                {
                    maxCircleObject = new DisplayObject(String.Format("{0}-maxAttenuationCircle", this.Name), app, "PointLightCircle", scene, app.Assets.assetFromName(app.Config.PointLightCircleMeshName).AssetName, this.Position, Vector3.UnitScale, Vector3.Zero, null);
                    maxCircleObject.MaterialName = "world_editor_light_ring.lightRing.red";

                }
                if (halfCircleObject != null)
                {
                    halfCircleObject.Scale = new Vector3(HalfAttenuationRadius, HalfAttenuationRadius, HalfAttenuationRadius);
                }
                if (quarterCircleObject != null)
                {
                    quarterCircleObject.Scale = new Vector3(QuarterAttenuationRadius, QuarterAttenuationRadius, QuarterAttenuationRadius);
                }
                maxCircleObject.Scale = new Vector3(attenuationRange, attenuationRange, attenuationRange);

            }
            else
            {
                showCircles = false;
                if (halfCircleObject != null)
                {
                    halfCircleObject.Dispose();
                    halfCircleObject = null;
                }
                if (quarterCircleObject != null)
                {
                    quarterCircleObject.Dispose();
                    quarterCircleObject = null;
                }
                if (maxCircleObject != null)
                {
                    maxCircleObject.Dispose();
                    maxCircleObject = null;
                }

            }
        }


        private void UpdateCirclesPosition()
        {
            if (halfCircleObject != null)
            {
                halfCircleObject.Position = this.Position;
            }
            if (quarterCircleObject != null)
            {
                quarterCircleObject.Position = this.Position;
            }
            if (maxCircleObject != null)
            {
                maxCircleObject.Position = this.Position;
            }
        }


        #endregion

        #region IObjectPosition Members

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
                if (displayObject != null)
                {
                    displayObject.Position = value;
                    pLight.Position = value;
                }
                terrainOffset = value.y - app.GetTerrainHeight(value.x, value.z);
                UpdateCirclesPosition();
            }
        }

        [BrowsableAttribute(false)]
        public bool AllowYChange
        {
            get
            {
                return true;
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
                    displayObject.TerrainOffset = terrainOffset;
                }
            }
        }

        [CategoryAttribute("Miscellaneous"), Description("The height in millimeters that Point Light is above the terrain")]
        public float HeightOffTerrain
        {
            get
            {
                return terrainOffset;
            }
        }

        #endregion IObjectPosition Members

        #region IObjectDrag Members


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

        #endregion
    }
}
