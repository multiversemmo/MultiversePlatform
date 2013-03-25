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
using System.Diagnostics;
using Axiom.Core;
using Axiom.ParticleSystems;
using Axiom.MathLib;

namespace Multiverse.Tools.WorldEditor
{
    public class DisplayParticleSystem : IDisposable
    {
        protected string name;
        protected SceneManager scene;
        protected string particleSystemName;
        protected Vector3 scale;
        protected Vector3 rotation;
        protected Quaternion orientation;
        protected Vector3 position;
        protected float velocityScale;
        protected float particleScale;
        protected float baseWidth;
        protected float baseHeight;
        protected bool attached;
        protected Axiom.Animating.TagPoint tagPoint;

        protected DisplayObject displayObject;
        protected string attachmentPointName;

        // Axiom structures for representing the object in the scene
        protected ParticleSystem particleSystem;
        private SceneNode node = null;

        public DisplayParticleSystem(String name, SceneManager scene, string particleSystemName, Vector3 position, Vector3 scale, Vector3 rotation, float velocityScale, float particleScale)
        {
            this.name = name;
            this.scene = scene;
            this.particleSystemName = particleSystemName;
            this.position = position;
            this.scale = scale;
            this.rotation = rotation;
            this.particleScale = particleScale;
            this.velocityScale = velocityScale;
            attached = false;
            AddToScene();
        }

        public DisplayParticleSystem(String name, SceneManager scene, string particleSystemName, float velocityScale, float particleScale, DisplayObject displayObject, string attachmentPointName)
        {
            this.name = name;
            this.scene = scene;
            this.particleSystemName = particleSystemName;
            this.particleScale = particleScale;
            this.velocityScale = velocityScale;
            this.displayObject = displayObject;
            this.attachmentPointName = attachmentPointName;
            attached = true;
            AddToScene();
        }

        public float VelocityScale
        {
            get
            {
                return velocityScale;
            }
            set
            {
                // undo previous scale
                particleSystem.ScaleVelocity(1 / velocityScale);

                // set the scale
                velocityScale = value;
                particleSystem.ScaleVelocity(velocityScale);
            }
        }

        public float ParticleScale
        {
            get
            {
                return particleScale;
            }
            set
            {
                particleScale = value;
                particleSystem.DefaultHeight = baseHeight * particleScale;
                particleSystem.DefaultWidth = baseHeight * particleScale;
            }
        }

        private void AddToScene()
        {
            string sceneName = WorldEditor.GetUniqueName(name, "Particle System");
            particleSystem = ParticleSystemManager.Instance.CreateSystem(sceneName, particleSystemName);
            particleSystem.ScaleVelocity(velocityScale);

            baseHeight = particleSystem.DefaultHeight;
            baseWidth = particleSystem.DefaultWidth;

            ParticleScale = particleScale;

            if (attached)
            {
                Axiom.Animating.AttachmentPoint attachmentPoint = displayObject.GetAttachmentPoint(attachmentPointName);
                if (attachmentPoint == null)
                {
                    attachmentPoint = new Axiom.Animating.AttachmentPoint(attachmentPointName, null, Quaternion.Identity, Vector3.Zero);
                }
                if (attachmentPoint.ParentBone != null)
                {
                    tagPoint = displayObject.Entity.AttachObjectToBone(attachmentPoint.ParentBone, particleSystem, attachmentPoint.Orientation, attachmentPoint.Position);
                    node = null;
                }
                else
                {
                    node = scene.CreateSceneNode();
                    node.Position = attachmentPoint.Position;
                    node.Orientation = attachmentPoint.Orientation;
                    displayObject.SceneNode.AddChild(node);
                    node.AttachObject(particleSystem);
                }

            }
            else
            {
                node = scene.RootSceneNode.CreateChildSceneNode();
                node.AttachObject(particleSystem);

                node.Position = position;
                node.ScaleFactor = scale;
                node.Orientation = Quaternion.FromAngleAxis(rotation.y * MathUtil.RADIANS_PER_DEGREE, Vector3.UnitY);
            }
        }

        private void RemoveFromScene()
        {
            if (tagPoint != null)
            {
                displayObject.Entity.DetachObjectFromBone(tagPoint.Parent.Name, tagPoint);
            }
            else
            {
                // remove the scene node from the scene's list of all nodes, and from its parent in the tree
                node.Creator.DestroySceneNode(node.Name);

                node = null;

                // XXX - remove the entity from the scene
            }
            particleSystem = null;
        }

        /// <summary>
        /// Highlight the object.  Use the axiom bounding box display as a cheap highlight.
        /// </summary>
        public bool Highlight
        {
            get
            {
                return node.ShowBoundingBox;
            }
            set
            {
                node.ShowBoundingBox = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                //Debug.Assert(!attached);
                return node.Position;
            }
            set
            {
                //Debug.Assert(!attached);
                node.Position = value;
            }
        }

        public void AdjustRotation(Vector3 v)
        {
            //Debug.Assert(!attached);
            rotation += v;
            orientation = (Quaternion.FromAngleAxis(rotation.y * MathUtil.RADIANS_PER_DEGREE, Vector3.UnitY));
            return;
        }

        public Quaternion Orientation
        {
            get
            {
                //Debug.Assert(!attached);
                return orientation;
            }
            set
            {
                //Debug.Assert(!attached);
                orientation = value;
                if (node != null)
                {
                    node.Orientation = orientation;
                }
            }
        }

        public void Dispose()
        {
            RemoveFromScene();
        }
    }
}
