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
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Multiverse.CollisionLib;
using System.ComponentModel;

namespace Multiverse.Tools.WorldEditor
{
    /// <summary>
    /// Summary description for DisplayObject.
    /// </summary>
    /// 
    public class DisplayObject : IDisposable
    {
        protected static Dictionary<string, IWorldObject> nameDictionary = new Dictionary<string,IWorldObject>();
        public static CollisionAPI collisionManager = null;
        protected static int oidCounter = 0;
		protected SceneManager scene;
        protected string name;
        protected string type; // what type of object this is.  used for unique name creation
        protected string meshName;
        protected Vector3 scale;
        protected Vector3 rotation;
        protected Quaternion orientation;
        protected SubMeshCollection subMeshCollection;
        protected IWorldObject parent;
        protected string uniName;
        protected bool scaleWithCameraDistance = false;
        protected long oid = 0;
        protected WorldEditor app;
        protected float terrainOffset = 0;
        protected bool castShadows = true;

        // Axiom structures for representing the object in the scene
        private Entity entity = null;
        private SceneNode node = null;

        public DisplayObject(IWorldObject parent, WorldEditor app, string name, string type, SceneManager scene, string meshName, Vector3 position, Vector3 scale, Vector3 rotation, SubMeshCollection subMeshCollection)
        {
            this.name = name;
            this.scene = scene;
            this.meshName = meshName;
            this.type = type;
            this.parent = parent;
			oidCounter++;
			this.oid = oidCounter;
            this.app = app;

            // if we were passed a subMeshCollection, then use it, otherwise make one.
            if (subMeshCollection == null)
            {
                this.subMeshCollection = new SubMeshCollection(meshName);
            }
            else
            {
                this.subMeshCollection = subMeshCollection;
            }

            AddToScene(position, scale, rotation);
        }

        public DisplayObject(string name, WorldEditor app, string type, SceneManager scene, string meshName, Vector3 position, Vector3 scale, Vector3 rotation, SubMeshCollection subMeshCollection)
        {
            this.name = name;
            this.scene = scene;
            this.meshName = meshName;
            this.type = type;
			oidCounter++;
			this.oid = oidCounter;
            this.app = app;

            // if we were passed a subMeshCollection, then use it, otherwise make one.
            if (subMeshCollection == null)
            {
                this.subMeshCollection = new SubMeshCollection(meshName);
            }
            else
            {
                this.subMeshCollection = subMeshCollection;
            }

            AddToScene(position, scale, rotation);
        }



        public DisplayObject(IWorldObject parent, string name, WorldEditor app, string type, SceneManager scene, string meshName, Vector3 position, Vector3 scale, Quaternion rotation, SubMeshCollection subMeshCollection)
        {
            this.name = name;
            this.scene = scene;
            this.meshName = meshName;
            this.type = type;
			oidCounter++;
			this.oid = oidCounter;
            this.app = app;
            this.parent = parent;

            // if we were passed a subMeshCollection, then use it, otherwise make one.
            if (subMeshCollection == null)
            {
                this.subMeshCollection = new SubMeshCollection(meshName);
            }
            else
            {
                this.subMeshCollection = subMeshCollection;
            }

            AddToScene(position, scale, rotation);
        }

        public DisplayObject(string name, WorldEditor app, string type, SceneManager scene, string meshName, Vector3 position, Vector3 scale, Quaternion rotation, SubMeshCollection subMeshCollection)
        {
            this.name = name;
            this.scene = scene;
            this.meshName = meshName;
            this.type = type;
            oidCounter++;
            this.oid = oidCounter;
            this.app = app;

            // if we were passed a subMeshCollection, then use it, otherwise make one.
            if (subMeshCollection == null)
            {
                this.subMeshCollection = new SubMeshCollection(meshName);
            }
            else
            {
                this.subMeshCollection = subMeshCollection;
            }

            AddToScene(position, scale, rotation);
        }


        public static IWorldObject LookupName(string uniqueName)
        {
            IWorldObject obj;
            nameDictionary.TryGetValue(uniqueName, out obj);
            return obj;
        }

        public string ObjectType
        {
            get
            {
                return "DisplayObject";
            }
        }

        public Axiom.Animating.AttachmentPoint GetAttachmentPoint(string attachmentPointName)
        {
            foreach (Axiom.Animating.AttachmentPoint attachmentPoint in entity.Mesh.AttachmentPoints)
            {
                if (attachmentPoint.Name == attachmentPointName)
                {
                    return attachmentPoint;
                }
            }

            if (entity.Mesh.Skeleton != null)
            {
                foreach (Axiom.Animating.AttachmentPoint attachmentPoint in entity.Mesh.Skeleton.AttachmentPoints)
                {
                    if (attachmentPoint.Name == attachmentPointName)
                    {
                        return attachmentPoint;
                    }
                }
            }
            return null;
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
            }
        }

        public Entity Entity
        {
            get
            {
                return entity;
            }
        }

        public SceneNode SceneNode
        {
            get
            {
                return node;
            }
        }

        public String MaterialName
        {
            get
            {
                return entity.MaterialName;
            }
            set
            {
                entity.MaterialName = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return node.Position;
            }
            set
            {
                node.Position = value;
                if (scaleWithCameraDistance)
                {
                    Vector3 dv = value - scene.GetCamera("PlayerCam").Position;
                    float distance = dv.Length;

                    float s = (float)Math.Sqrt(distance / 10000);
                    if (s < 1)
                    {
                        s = 1;
                    }
                    Scale = new Vector3(s,s,s);
                }
                RecreateCollisionShapes();
            }
        }

        public Vector3 Center
        {
            get
            {
                return node.WorldBoundingSphere.Center;
            }
        }

        public IWorldObject Parent
        {
            get
            {
                return parent;
            }
        }

        public void AdjustRotation(Vector3 v)
        {
            rotation += v;
            SetOrientation(Quaternion.FromAngleAxis(rotation.y * MathUtil.RADIANS_PER_DEGREE, Vector3.UnitY));
            return;
        }

        public Quaternion Orientation
        {
            get
            {
                return this.orientation;
            }
        }

        public SubMeshCollection SubMeshCollection
        {
            get
            {
                return subMeshCollection;
            }
            set
            {
                subMeshCollection = value;
                ValidateSubMeshList();
            }
        }

        public Vector3 Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string MeshName
        {
            get
            {
                return meshName;
            }
        }

        public bool CastShadows
        {
            get
            {
                return castShadows;
            }
            set
            {
                castShadows = value;
                if (entity != null)
                {
                    entity.CastShadows = castShadows;
                }
            }
        }

        public void SetOrientation(Quaternion value)
        {
            orientation = value;
            node.Orientation = this.orientation;
        }

        public void SetOrientation(float angle, Vector3 axis)
        {
            orientation = Quaternion.FromAngleAxis(angle, axis);
            node.Orientation = this.orientation;
        }

        public void SetRotation(float degree)
        {
            Quaternion quat = Quaternion.FromAngleAxis(degree * MathUtil.RADIANS_PER_DEGREE, Vector3.UnitY);

            SetOrientation(quat);
        }

        public void AdjustOrientation(float degree, Vector3 Axis)
        {
            Quaternion quat = Quaternion.FromAngleAxis(degree * MathUtil.RADIANS_PER_DEGREE, Axis);

            if (node != null)
            {
                orientation = node.Orientation + quat;
                node.Orientation = node.Orientation + quat;
            }
            return;
        }

        public Vector3 Scale
        {
            get
            {
                return node.ScaleFactor;
            }
            set
            {
                node.ScaleFactor = value;
            }
        }

        public bool ScaleWithCameraDistance
        {
            get
            {
                return scaleWithCameraDistance;
            }
            set
            {
                scaleWithCameraDistance = value;
            }
        }

        public SceneManager Scene
        {
            get
            {
                return scene;
            }
        }

        public float Radius
        {
            get
            {
                return node.WorldBoundingSphere.Radius;
            }
        }

        // make sure the object is entered into the scene so that it is viewable
        private void AddToScene(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            AddToScene(position, scale, Quaternion.FromAngleAxis(rotation.y * MathUtil.RADIANS_PER_DEGREE, Vector3.UnitY));
        }

        public void AddToScene(Vector3 position, Vector3 scale, Quaternion rotation)
        {
            string uniqueName = WorldEditor.GetUniqueName(name, type);
            if (!String.Equals(type, "Drag"))
            {
                this.uniName = uniqueName;
                nameDictionary.Add(uniqueName, parent);
            }
            entity = scene.CreateEntity(uniqueName, this.MeshName);
            entity.CastShadows = castShadows;
            node = scene.RootSceneNode.CreateChildSceneNode(position);
            node.ScaleFactor = scale;
            node.Orientation = rotation;
            node.AttachObject(entity);


            ValidateSubMeshList();

            AddCollisionObject();

            //subMeshCollection.Changed += new SubMeshChangeEventHandler(subMeshCollection_Changed);
        }

        // Remove the object from the view
        public void RemoveFromScene()
        {
            // remove the scene node from the scene's list of all nodes, and from its parent in the tree
            node.Creator.DestroySceneNode(node.Name);

            node = null;

            RemoveCollisionObject();
            
            // remove the entity from the scene
            scene.RemoveEntity(entity);

            // clean up any unmanaged resources
            entity.Dispose();

            entity = null;

            //subMeshCollection.Changed -= new SubMeshChangeEventHandler(subMeshCollection_Changed);
            if (uniName != null && nameDictionary.ContainsKey(this.uniName))
            {
                uniName = "";
            }
        }

        /// <summary>
        ///   Look up the collision shapes associated with the mesh
        ///   name, transform them and add them to the sphere tree
        /// </summary>
        public void AddCollisionObject() {
            if (collisionManager == null) {
                LogManager.Instance.Write("DisplayObject.collisionManager is null!");
                return;
            }
            List<CollisionShape> shapes = WorldEditor.Instance.FindMeshCollisionShapes(this.meshName, this.entity);
            if (shapes.Count == 0)
                return;
            foreach (CollisionShape untransformedShape in shapes) {
                CollisionShape shape = untransformedShape.Clone();
                shape.Transform(node.DerivedScale,
                                node.DerivedOrientation,
                                node.DerivedPosition);
                collisionManager.AddCollisionShape(shape, oid);
            }
        }

        /// <summary>
        ///   Remove any existing collision shapes for this
        ///   DisplayObject, and recreate them with the correct
        ///   position, orientation and scale.
        /// </summary>
        public void RecreateCollisionShapes() {
            List<CollisionShape> shapes = WorldEditor.Instance.FindMeshCollisionShapes(this.meshName, this.entity);
            if (shapes.Count == 0)
                return;
            collisionManager.RemoveCollisionShapesWithHandle(oid);
            foreach (CollisionShape originalShape in shapes) {
                CollisionShape shape = originalShape.Clone();
                shape.Transform(node.DerivedScale,
                                node.DerivedOrientation,
                                node.DerivedPosition);
                collisionManager.AddCollisionShape(shape, oid);
            }
        }
        
        /// <summary>
        ///   Remove the associated collision data from an object.
        /// </summary>
        /// <param name="objNode">the object for which we are removing the collision data</param>
        public void RemoveCollisionObject() {
            if (collisionManager == null)
                LogManager.Instance.Write("DisplayObject.collisionManager is null!");
            else
                collisionManager.RemoveCollisionShapesWithHandle(oid);
        }

        /// <summary>
        /// Highlight the object.  Use the axiom bounding box display as a cheap highlight.
        /// </summary>
        public bool Highlight
        {
            get
            {
                return entity.ShowBoundingBox;
            }
            set
            {
                entity.ShowBoundingBox = value;
            }
        }

        public AxisAlignedBox BoundingBox
        {
            get
            {
                return node.WorldAABB;
            }
        }

        #region SubMesh related methods

        /// <summary>
        /// Load the material and visibility information from SubMeshInfo into the actual subMesh
        /// </summary>
        /// <param name="subMeshInfo"></param>
        private void ValidateSubMesh(SubMeshInfo subMeshInfo)
        {
            entity.GetSubEntity(subMeshInfo.Name).IsVisible = subMeshInfo.Show;
            entity.GetSubEntity(subMeshInfo.Name).MaterialName = subMeshInfo.MaterialName;
        }

        /// <summary>
        /// This method sets up the submeshes based on the SubMeshList
        /// </summary>
        private void ValidateSubMeshList()
        {
            foreach (SubMeshInfo subMeshInfo in subMeshCollection)
            {
                ValidateSubMesh(subMeshInfo);
            }
        }

        private void subMeshCollection_Changed(object sender, EventArgs args)
        {
            ValidateSubMeshList();
        }

        /// <summary>
        /// Look up subMeshInfo by name in the SubMeshList
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private SubMeshInfo FindSubMeshInfo(string name)
        {
            return subMeshCollection.FindSubMeshInfo(name);
        }

        #endregion SubMesh related methods


        public void Dispose()
        {
            RemoveFromScene();
        }

        public void UpdateOrientation(float azimuth, float zenith)
        {
            Quaternion azimuthRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(azimuth), Vector3.UnitY);
            Quaternion zenithRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(-zenith), Vector3.UnitX);

            Quaternion displayZenithRotation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(-zenith + 90), Vector3.UnitX);
            orientation = azimuthRotation * displayZenithRotation;
            node.Orientation = orientation;
        }
    }
}
