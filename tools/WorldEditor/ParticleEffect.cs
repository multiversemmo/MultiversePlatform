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
    public class ParticleEffect : IWorldObject, IObjectDelete
    {
        protected IWorldObject parent;
        protected WorldEditor app;
        protected string particleEffectName;
        protected float particleScale;
        protected float velocityScale;
        protected string attachmentPointName = null;
        protected Quaternion orientation;

        protected bool inScene = false;
        protected bool inTree = false;

        protected WorldTreeNode node = null;
        protected WorldTreeNode parentNode = null;

        protected DisplayParticleSystem displayParticle;

        protected List<ToolStripButton> buttonBar;

        public ParticleEffect(IWorldObject parent, WorldEditor app, string particleEffectName, float particleScale, float velocityScale, Quaternion orientation)
        {
            this.parent = parent;
            this.app = app;
            this.orientation = orientation;

            this.particleEffectName = particleEffectName;
            this.particleScale = particleScale;
            this.velocityScale = velocityScale;
        }
    
        public ParticleEffect(IWorldObject parent, WorldEditor app, string particleEffectName, float particleScale, float velocityScale, string attachmentPointName, Quaternion orientation)
            : this(parent, app, particleEffectName, particleScale, velocityScale, orientation)
        {
            this.attachmentPointName = attachmentPointName;
        }

        public ParticleEffect(XmlReader r, IWorldObject parent, WorldEditor app)
        {
            this.parent = parent;
            this.app = app;

            FromXml(r);
        }

        protected void FromXml(XmlReader r)
        {            // first parse name and mesh, which are attributes
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);

                // set the field in this object based on the element we just read
                switch (r.Name)
                {
                    case "ParticleEffectName":
                        this.particleEffectName = r.Value;
                        break;
                    case "VelocityScale":
                        this.velocityScale = float.Parse(r.Value);
                        break;
                    case "ParticleScale":
                        this.particleScale = float.Parse(r.Value);
                        break;
                    case "AttachmentPoint":
                        this.attachmentPointName = r.Value;
                        break;
                }
            }
            r.MoveToElement(); //Moves the reader back to the element node.

            
        }

        public Quaternion Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                orientation = value;
                displayParticle.Orientation = orientation;
            }
        }

        [DescriptionAttribute("The name of this particle effect asset.")]
        [TypeConverter(typeof(ParticleEffectNameUITypeEditor)), CategoryAttribute("Miscellaneous")]
        public String Name
        {
            get
            {
				return app.Assets.assetFromAssetName(particleEffectName).Name;
            }
			set
			{
				RemoveFromScene();
				particleEffectName = app.Assets.assetFromName(value).AssetName;
				AddToScene();
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
                    ret = string.Format("{0}: {1}", ObjectType, Name);
                }
                else
                {
                    ret = Name;
                }

                return ret;
            }
        }

        [DescriptionAttribute("The type of this object."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
        public string ObjectType
        {
            get
            {
                return "ParticleEffect";
            }
        }

        [DescriptionAttribute("Amount to scale the velocity of the particles."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
        public float VelocityScale
        {
            get
            {
                return velocityScale;
            }
            set
            {
                velocityScale = value;
                if (inScene)
                {
                    displayParticle.VelocityScale = velocityScale;
                }
            }
        }

        [DescriptionAttribute("Amount to scale the size of the individual particle billboards."), CategoryAttribute("Miscellaneous"), BrowsableAttribute(true)]
        public float ParticleScale
        {
            get
            {
                return particleScale;
            }
            set
            {
                particleScale = value;
                if (inScene)
                {
                    displayParticle.ParticleScale = particleScale;
                }
            }
        }

        protected Vector3 ParentPosition
        {
            get
            {
                IObjectPosition posParent = parent as IObjectPosition;

                if (parent == null)
                {
                    return Vector3.Zero;
                }

                return posParent.Position;
            }
        }

        public void PositionUpdate()
        {
            if (inScene)
            {
                displayParticle.Position = ParentPosition;
            }
        }

        #region IWorldObject Members

        public void AddToTree(WorldTreeNode parentNode)
        {
            this.parentNode = parentNode;
            inTree = true;

            // create a node for the collection and add it to the parent
            node = app.MakeTreeNode(this, NodeName);
            parentNode.Nodes.Add(node);

            // build the menu
            CommandMenuBuilder menuBuilder = new CommandMenuBuilder();
            menuBuilder.Add("Copy Description", "", app.copyToClipboardMenuButton_Click);
            menuBuilder.Add("Help", "Particle_Effect", app.HelpClickHandler);
            menuBuilder.Add("Delete", new DeleteObjectCommandFactory(app, parent as IWorldContainer, this), app.DefaultCommandClickHandler);
            node.ContextMenuStrip = menuBuilder.Menu;
            buttonBar = menuBuilder.ButtonBar;
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

        public void Clone(IWorldContainer copyParent)
        {
            ParticleEffect clone = new ParticleEffect(copyParent as IWorldObject, app, particleEffectName, particleScale,
                velocityScale, attachmentPointName, orientation);
            copyParent.Add(clone);
        }


        [BrowsableAttribute(false)]
        public string ObjectAsString
        {
            get
            {
                string objString = String.Format("Name:{0}:{1}\r\n",ObjectType, Name);
                objString +=  String.Format("VelocityScale={0}\r\n",VelocityScale);
                objString +=  String.Format("ParticleScale={0}\r\n", ParticleScale);
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
            if (inTree)
            {
                if (node.IsSelected)
                {
                    node.UnSelect();
                }
                if (node.IsSelected)
                {
                    node.UnSelect();
                }
                parentNode.Nodes.Remove(node);
                node = null;
                parentNode = null;
            }
            inTree = false;
        }

        public void AddToScene()
        {
            if (app.DisplayParticleEffects && !inScene)
            {
                if (attachmentPointName != null)
                {
                    DisplayObject parentObj = (parent as StaticObject).DisplayObject;
                    displayParticle = new DisplayParticleSystem(particleEffectName, app.Scene, particleEffectName, velocityScale, particleScale, parentObj, attachmentPointName);
                    displayParticle.Orientation = this.Orientation;
                }
                else
                {
                    Vector3 scale = new Vector3(1, 1, 1);
                    Vector3 rotation = Vector3.Zero;
                    displayParticle = new DisplayParticleSystem(particleEffectName, app.Scene, particleEffectName, ParentPosition, scale, rotation, velocityScale, particleScale);
                    displayParticle.Orientation = this.Orientation;
                }
                inScene = true;

                // displayParticle.Highlight = Highlight;
            }
        }

        public void UpdateScene(UpdateTypes type, UpdateHint hint)
        {
            if ((type == UpdateTypes.ParticleEffect || type == UpdateTypes.All) && hint == UpdateHint.Display)
            {
                if (inScene == app.DisplayParticleEffects)
                {
                    return;
                }
                else
                {
                    if (inScene)
                    {
                        this.RemoveFromScene();
                    }
                    else
                    {
                        this.AddToScene();
                    }
                }
            }
        }

        public void RemoveFromScene()
        {
            if (inScene)
            {
                if (displayParticle != null)
                {
                    displayParticle.Dispose();
                    displayParticle = null;
                }
            }
            inScene = false;
        }

        public void CheckAssets()
        {
            if (!Axiom.ParticleSystems.ParticleSystemManager.Instance.SystemTemplateList.ContainsKey(particleEffectName))
            {
                app.AddMissingAsset(string.Format("Particle Effect:{0}", particleEffectName));
            }
        }

        public void ToXml(System.Xml.XmlWriter w)
        {
            w.WriteStartElement("ParticleEffect");
            w.WriteAttributeString("ParticleEffectName", particleEffectName);
            w.WriteAttributeString("VelocityScale", velocityScale.ToString());
            w.WriteAttributeString("ParticleScale", particleScale.ToString());
            if (attachmentPointName != null)
            {
                w.WriteAttributeString("AttachmentPoint", attachmentPointName);
            }
            w.WriteEndElement(); // ParticleEffect end
        }

		[BrowsableAttribute(false)]
        public Axiom.MathLib.Vector3 FocusLocation
        {
            get
            {
                return parent.FocusLocation;
            }
        }

		[BrowsableAttribute(false)]
        public bool Highlight
        {
            get
            {
                return parent.Highlight;
            }
            set
            {
                parent.Highlight = value;
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
		public WorldTreeNode Node
		{
			get
			{
				return node;
			}
		}

        public void ToManifest(System.IO.StreamWriter w)
        {
            w.WriteLine("ParticleEffect:{0}", particleEffectName);
        }

        #endregion

        #region IWorldDelete

        [BrowsableAttribute(false)]
        public IWorldContainer Parent
        {
            get
            {
                return parent as IWorldContainer;
            }
        }
        #endregion IWorldDelete

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

    }
}
