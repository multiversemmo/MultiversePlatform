#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;

using Axiom.MathLib;
using Axiom.Graphics;

namespace Axiom.Core {

    /// <summary>
    ///		Handles the management of mesh resources.
    /// </summary>
    public class MeshManager : ResourceManager {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        protected static MeshManager instance;

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        protected internal MeshManager() {
            if (instance == null) {
                instance = this;
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static MeshManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

		#region Fields

		/// <summary>
		///		Flag indicating whether newly loaded meshes should also be prepared for 
		///		shadow volumes.
		/// </summary>
        private bool prepAllMeshesForShadowVolumes;

		#endregion Fields

		#region Properties

        /// <summary>
        /// Gets the names of all meshes that have been created
        /// </summary>
        public StringCollection MeshNames
        {
            get { return this.GetResourceNamesWithExtension(".mesh"); }
        }

		/// <summary>
		///		Tells the mesh manager that all future meshes should prepare themselves for
		///		shadow volumes on loading.
		/// </summary>
		public bool PrepareAllMeshesForShadowVolumes {
			get {
				return prepAllMeshesForShadowVolumes;
			}
			set {
				prepAllMeshesForShadowVolumes = value;
			}
		}

		#endregion Properties

        /// <summary>
        ///		Called internally to initialize this manager.
        /// </summary>
        public void Initialize() {
            CreatePrefabPlane();
        }
	
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override Resource Create(string name, bool isManual) {
            return new Mesh(name);
        }

        /// <summary>
        ///		Creates a barebones Mesh object that can be used to manually define geometry later on.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Mesh CreateManual(string name) {
            Mesh mesh = MeshManager.Instance.GetByName(name);

            if(mesh == null) {
                mesh = (Mesh)Create(name);
                mesh.IsManuallyDefined = true;
                Add(mesh);
            }

            return mesh;
        }

        /// <summary>
        ///		Overloaded method.
        /// </summary>
        /// <param name="name">Name of the plane mesh.</param>
        /// <param name="plane">Plane to use for distance and orientation of the mesh.</param>
        /// <param name="width">Width in world coordinates.</param>
        /// <param name="height">Height in world coordinates.</param>
        /// <returns></returns>
        public Mesh CreatePlane(string name, Plane plane, int width, int height) {
            return CreatePlane(name, plane, width, height, 1, 1, true, 1, 1.0f, 1.0f, Vector3.UnitY, BufferUsage.StaticWriteOnly, BufferUsage.StaticWriteOnly, true, true);
        }

        public Mesh CreatePlane(string name, Plane plane, float width, float height, int xSegments, int ySegments, bool normals, int numTexCoordSets, float uTile, float vTile, Vector3 upVec) {
            return CreatePlane(name, plane, width, height, xSegments, ySegments, normals, numTexCoordSets, uTile, vTile, upVec, BufferUsage.StaticWriteOnly, BufferUsage.StaticWriteOnly, true, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the plane mesh.</param>
        /// <param name="plane">Plane to use for distance and orientation of the mesh.</param>
        /// <param name="width">Width in world coordinates.</param>
        /// <param name="height">Height in world coordinates.</param>
        /// <param name="xSegments">Number of x segments for tesselation.</param>
        /// <param name="ySegments">Number of y segments for tesselation.</param>
        /// <param name="normals">If true, plane normals are created.</param>
        /// <param name="numTexCoordSets">Number of 2d texture coord sets to use.</param>
        /// <param name="uTile">Number of times the texture should be repeated in the u direction.</param>
        /// <param name="vTile">Number of times the texture should be repeated in the v direction.</param>
        /// <param name="upVec">The up direction of the plane.</param>
        /// <returns></returns>
        public Mesh CreatePlane(string name, Plane plane, float width, float height, int xSegments, int ySegments, bool normals, int numTexCoordSets, float uTile, float vTile, Vector3 upVec,
            BufferUsage vertexBufferUsage, BufferUsage indexBufferUsage, bool vertexShadowBuffer, bool indexShadowBuffer ) {
            Mesh mesh = CreateManual(name);
            SubMesh subMesh = mesh.CreateSubMesh(name + "SubMesh");

            mesh.SharedVertexData = new VertexData();
            VertexData vertexData = mesh.SharedVertexData;

            VertexDeclaration decl = vertexData.vertexDeclaration;
            int currOffset = 0;

            // add position data
            decl.AddElement(0, currOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            currOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            // normals are optional
            if(normals) {
                decl.AddElement(0, currOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
                currOffset += VertexElement.GetTypeSize(VertexElementType.Float3);
            }

            // add texture coords
            for(ushort i = 0; i < numTexCoordSets; i++) {
                decl.AddElement(0, currOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords, i);
                currOffset += VertexElement.GetTypeSize(VertexElementType.Float2);
            }

            vertexData.vertexCount = (xSegments + 1) * (ySegments + 1);

            // create a new vertex buffer (based on current API)
            HardwareVertexBuffer vbuf = 
                HardwareBufferManager.Instance.CreateVertexBuffer(decl.GetVertexSize(0), vertexData.vertexCount, vertexBufferUsage, vertexShadowBuffer);
			
            // get a reference to the vertex buffer binding
            VertexBufferBinding binding = vertexData.vertexBufferBinding;

            // bind the first vertex buffer
            binding.SetBinding(0, vbuf);

            // transform the plane based on its plane def
            Matrix4 translate = Matrix4.Identity;
            Matrix4 transform = Matrix4.Zero;
            Matrix4 rotation = Matrix4.Identity;
            Matrix3 rot3x3 = Matrix3.Zero;

            Vector3 xAxis, yAxis, zAxis;
            zAxis = plane.Normal;
            zAxis.Normalize();
            yAxis = upVec;
            yAxis.Normalize();
            xAxis = yAxis.Cross(zAxis);

            if (xAxis.Length == 0) {
                throw new AxiomException("The up vector for a plane cannot be parallel to the planes normal.");
            }

            rot3x3.FromAxes(xAxis, yAxis, zAxis);
            rotation = rot3x3;

            // set up transform from origin
            translate.Translation = plane.Normal * -plane.D;

            transform = translate * rotation;

            float xSpace = width / xSegments;
            float ySpace = height / ySegments;
            float halfWidth = width / 2;
            float halfHeight = height / 2;
            float xTexCoord = (1.0f * uTile) / xSegments;
            float yTexCoord = (1.0f * vTile) / ySegments;
            Vector3 vec = Vector3.Zero;
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;
            float maxSquaredLength = 0;
            bool firstTime = true;

            // generate vertex data
            GeneratePlaneVertexData(vbuf, ySegments, xSegments, xSpace, halfWidth, ySpace, halfHeight, transform, firstTime, normals, rotation, numTexCoordSets, xTexCoord, yTexCoord, subMesh, ref min, ref max, ref maxSquaredLength);

            // generate face list
            Tesselate2DMesh(subMesh, xSegments + 1, ySegments + 1, false, indexBufferUsage, indexShadowBuffer);

            // generate bounds for the mesh
            mesh.BoundingBox = new AxisAlignedBox(min, max);
            mesh.BoundingSphereRadius = MathUtil.Sqrt(maxSquaredLength);

			mesh.Load();
			mesh.Touch();

            return mesh;
        }

        private static void GeneratePlaneVertexData(HardwareVertexBuffer vbuf, int ySegments, int xSegments, float xSpace, float halfWidth, float ySpace, float halfHeight, Matrix4 transform, bool firstTime, bool normals, Matrix4 rotation, int numTexCoordSets, float xTexCoord, float yTexCoord, SubMesh subMesh, ref Vector3 min, ref Vector3 max, ref float maxSquaredLength) 
        {
            Vector3 vec;
            unsafe {
                // lock the vertex buffer
                IntPtr data = vbuf.Lock(BufferLocking.Discard);

                float* pData = (float*)data.ToPointer();

                for (int y = 0; y <= ySegments; y++) {
                    for (int x = 0; x <= xSegments; x++) {
                        // centered on origin
                        vec.x = (x * xSpace) - halfWidth;
                        vec.y = (y * ySpace) - halfHeight;
                        vec.z = 0.0f;

                        vec = transform * vec;

                        *pData++ = vec.x;
                        *pData++ = vec.y;
                        *pData++ = vec.z;

                        // Build bounds as we go
                        if (firstTime) {
                            min = vec;
                            max = vec;
                            maxSquaredLength = vec.LengthSquared;
                            firstTime = false;
                        } else {
                            min.Floor(vec);
                            max.Ceil(vec);
                            maxSquaredLength = MathUtil.Max(maxSquaredLength, vec.LengthSquared);
                        }

                        if (normals) {
                            vec = Vector3.UnitZ;
                            vec = rotation * vec;

                            *pData++ = vec.x;
                            *pData++ = vec.y;
                            *pData++ = vec.z;
                        }

                        for (int i = 0; i < numTexCoordSets; i++) {
                            *pData++ = x * xTexCoord;
                            *pData++ = 1 - (y * yTexCoord);
                        } // for texCoords
                    } // for x
                } // for y

                // unlock the buffer
                vbuf.Unlock();

                subMesh.useSharedVertices = true;

            } // unsafe
        }

        /// <summary>
        ///     Creates a Bezier patch based on an array of control vertices.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="controlPointBuffer"></param>
        /// <param name="declaration"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="uMaxSubdivisionLevel"></param>
        /// <param name="vMaxSubdivisionLevel"></param>
        /// <param name="visibleSide"></param>
        /// <param name="vbUsage"></param>
        /// <param name="ibUsage"></param>
        /// <param name="vbUseShadow"></param>
        /// <param name="ibUseShadow"></param>
        /// <returns></returns>
        public PatchMesh CreateBezierPatch(string name, System.Array controlPointBuffer, VertexDeclaration declaration,
            int width, int height, int uMaxSubdivisionLevel, int vMaxSubdivisionLevel, VisibleSide visibleSide,
            BufferUsage vbUsage, BufferUsage ibUsage, bool vbUseShadow, bool ibUseShadow) {

            PatchMesh mesh = (PatchMesh)GetByName(name);

            if(mesh != null) {
                throw new AxiomException("A mesh with the name {0} already exists!", name);
            }

            mesh = new PatchMesh(name, controlPointBuffer, declaration, width, height, 
                uMaxSubdivisionLevel, vMaxSubdivisionLevel, visibleSide, vbUsage, ibUsage, vbUseShadow, ibUseShadow);

            mesh.IsManuallyDefined = true;

            base.Load(mesh, 0);

            return mesh;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="plane"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="curvature"></param>
        /// <param name="xSegments"></param>
        /// <param name="ySegments"></param>
        /// <param name="normals"></param>
        /// <param name="numberOfTexCoordSets"></param>
        /// <param name="uTiles"></param>
        /// <param name="vTiles"></param>
        /// <param name="upVector"></param>
        /// <returns></returns>
        public Mesh CreateCurvedIllusionPlane(string name, Plane plane, float width, float height, float curvature, int xSegments, int ySegments, bool normals, int numberOfTexCoordSets, float uTiles, float vTiles, Vector3 upVector) {
            return CreateCurvedIllusionPlane(name, plane, width, height, curvature, xSegments, ySegments, normals, numberOfTexCoordSets, uTiles, vTiles, upVector, Quaternion.Identity, BufferUsage.StaticWriteOnly, BufferUsage.StaticWriteOnly, true, true);
        }

        /// <summary>
        ///   Helper method that generates the 6 vertices used to generate
        ///   an 8 (or 16 if you count backfaces) polygon representation of a
        ///   bone.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="bone"></param>
		private void GetVertices(ref Vector3[] points, Axiom.Animating.Bone bone) {
			Vector3 boneBase = bone.DerivedPosition;
            foreach (Axiom.Animating.Bone childBone in bone.Children) {
				// The tip of the bone:
				Vector3 boneTip = childBone.DerivedPosition;
				// the base of the bone
				Vector3 arm = boneTip - boneBase;
				Vector3 perp1 = arm.Perpendicular();
				Vector3 perp2 = arm.Cross(perp1);
				perp1.Normalize();
				perp2.Normalize();
				float boneLen = arm.Length;
				int offset = 6 * childBone.Handle;
				points[offset + 0] = boneTip;
				points[offset + 1] = boneBase;
				points[offset + 2] = boneBase + boneLen / 10 * perp1;
				points[offset + 3] = boneBase + boneLen / 10 * perp2;
				points[offset + 4] = boneBase - boneLen / 10 * perp1;
				points[offset + 5] = boneBase - boneLen / 10 * perp2;
				GetVertices(ref points, childBone);
			}
		}

		public Mesh CreateBoneMesh(string name) {
			Mesh mesh = CreateManual(name);
			mesh.SkeletonName = name + ".skeleton";
			SubMesh subMesh = mesh.CreateSubMesh("BoneSubMesh");
			subMesh.useSharedVertices = true;
			subMesh.MaterialName = "BaseWhite";

			// short[] faces = { 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 2, 1, 2, 5, 1, 5, 4, 1, 4, 3, 1, 3, 2 };
			// short[] faces = { 0, 3, 2, 0, 4, 3, 0, 5, 4, 0, 2, 5, 1, 5, 2, 1, 4, 5, 1, 3, 4, 1, 2, 3 };
			short[] faces = { 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 2, 1, 2, 5, 1, 5, 4, 1, 4, 3, 1, 3, 2,
							  0, 3, 2, 0, 4, 3, 0, 5, 4, 0, 2, 5, 1, 5, 2, 1, 4, 5, 1, 3, 4, 1, 2, 3 };
			int faceCount = faces.Length / 3; // faces per bone
			int vertexCount = 6; // vertices per bone

			// set up vertex data, use a single shared buffer
			mesh.SharedVertexData = new VertexData();
			VertexData vertexData = mesh.SharedVertexData;

			// set up vertex declaration
			VertexDeclaration vertexDeclaration = vertexData.vertexDeclaration;
			int currentOffset = 0;

			// always need positions
			vertexDeclaration.AddElement(0, currentOffset, VertexElementType.Float3, VertexElementSemantic.Position);
			currentOffset += VertexElement.GetTypeSize(VertexElementType.Float3);
			vertexDeclaration.AddElement(0, currentOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
			currentOffset += VertexElement.GetTypeSize(VertexElementType.Float3);
			
			int boneCount = mesh.Skeleton.BoneCount;

			// I want 6 vertices per bone - exclude the root bone
			vertexData.vertexCount = boneCount * vertexCount;

			// allocate vertex buffer
			HardwareVertexBuffer vertexBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(vertexDeclaration.GetVertexSize(0), vertexData.vertexCount, BufferUsage.StaticWriteOnly);

			// set up the binding, one source only
			VertexBufferBinding binding = vertexData.vertexBufferBinding;
			binding.SetBinding(0, vertexBuffer);
 
			Vector3[] vertices = new Vector3[vertexData.vertexCount];
			GetVertices(ref vertices, mesh.Skeleton.RootBone);

			// Generate vertex data
			unsafe {
				// lock the vertex buffer
				IntPtr data = vertexBuffer.Lock(BufferLocking.Discard);

				float* pData = (float*)data.ToPointer();

				foreach (Vector3 vec in vertices) {
					// assign to geometry
					*pData++ = vec.x;
					*pData++ = vec.y;
					*pData++ = vec.z;
					// fake normals
					*pData++ = 0;
					*pData++ = 1;
					*pData++ = 0;
				}
				
                // unlock the buffer
                vertexBuffer.Unlock();
            } // unsafe


			// Generate index data
			HardwareIndexBuffer indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(IndexType.Size16, faces.Length * boneCount, BufferUsage.StaticWriteOnly);
			subMesh.indexData.indexBuffer = indexBuffer;
			subMesh.indexData.indexCount = faces.Length * boneCount;
			subMesh.indexData.indexStart = 0;
			for (ushort boneIndex = 0; boneIndex < mesh.Skeleton.BoneCount; ++boneIndex) {
				Axiom.Animating.Bone bone = mesh.Skeleton.GetBone(boneIndex);
				short[] tmpFaces = new short[faces.Length];
				for (int tmp = 0; tmp < faces.Length; ++tmp)
					tmpFaces[tmp] = (short)(faces[tmp] + vertexCount * bone.Handle);
				indexBuffer.WriteData(faces.Length * bone.Handle * sizeof(short), tmpFaces.Length * sizeof(short), tmpFaces, true);
			}

			for (ushort boneIndex = 0; boneIndex < mesh.Skeleton.BoneCount; ++boneIndex) {
				Axiom.Animating.Bone bone = mesh.Skeleton.GetBone(boneIndex);
				Axiom.Animating.Bone parentBone = bone;
				if (bone.Parent != null)
					parentBone = (Axiom.Animating.Bone)bone.Parent;
				for (int vertexIndex = 0; vertexIndex < vertexCount; ++vertexIndex) {
					Axiom.Animating.VertexBoneAssignment vba = new Axiom.Animating.VertexBoneAssignment();
					// associate the base of the joint display with the bone's parent, 
					// and the rest of the points with the bone.
					vba.boneIndex = parentBone.Handle;
					vba.weight = 1.0f;
					vba.vertexIndex = vertexCount * bone.Handle + vertexIndex;
					mesh.AddBoneAssignment(vba);
				}
			}

			mesh.Load();
			mesh.Touch();

			return mesh;
		}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="plane"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="curvature"></param>
        /// <param name="xSegments"></param>
        /// <param name="ySegments"></param>
        /// <param name="normals"></param>
        /// <param name="numberOfTexCoordSets"></param>
        /// <param name="uTiles"></param>
        /// <param name="vTiles"></param>
        /// <param name="upVector"></param>
        /// <param name="orientation"></param>
        /// <param name="vertexBufferUsage"></param>
        /// <param name="indexBufferUsage"></param>
        /// <param name="vertexShadowBuffer"></param>
        /// <param name="indexShadowBuffer"></param>
        /// <returns></returns>
        public Mesh CreateCurvedIllusionPlane(string name, Plane plane, float width, float height, float curvature, int xSegments, int ySegments, bool normals, int numberOfTexCoordSets, float uTiles, float vTiles, Vector3 upVector, Quaternion orientation, BufferUsage vertexBufferUsage, BufferUsage indexBufferUsage, bool vertexShadowBuffer, bool indexShadowBuffer) {
            Mesh mesh = CreateManual(name);
            SubMesh subMesh = mesh.CreateSubMesh(name + "SubMesh");

            // set up vertex data, use a single shared buffer
            mesh.SharedVertexData = new VertexData();
            VertexData vertexData = mesh.SharedVertexData;

            // set up vertex declaration
            VertexDeclaration vertexDeclaration = vertexData.vertexDeclaration;
            int currentOffset = 0;

            // always need positions
            vertexDeclaration.AddElement(0, currentOffset, VertexElementType.Float3, VertexElementSemantic.Position);
            currentOffset += VertexElement.GetTypeSize(VertexElementType.Float3);

            // optional normals
            if(normals) {
                vertexDeclaration.AddElement(0, currentOffset, VertexElementType.Float3, VertexElementSemantic.Normal);
                currentOffset += VertexElement.GetTypeSize(VertexElementType.Float3);
            }

            for(ushort i = 0; i < numberOfTexCoordSets; i++) {
                // assumes 2d texture coordinates
                vertexDeclaration.AddElement(0, currentOffset, VertexElementType.Float2, VertexElementSemantic.TexCoords, i);
                currentOffset += VertexElement.GetTypeSize(VertexElementType.Float2);
            }

            vertexData.vertexCount = (xSegments + 1) * (ySegments + 1);

            // allocate vertex buffer
            HardwareVertexBuffer vertexBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(vertexDeclaration.GetVertexSize(0), vertexData.vertexCount, vertexBufferUsage, vertexShadowBuffer);

            // set up the binding, one source only
            VertexBufferBinding binding = vertexData.vertexBufferBinding;
            binding.SetBinding(0, vertexBuffer);

            // work out the transform required, default orientation of plane is normal along +z, distance 0
            Matrix4 xlate, xform, rot;
            Matrix3 rot3 = Matrix3.Identity;
            xlate = rot = Matrix4.Identity;

            // determine axes
            Vector3 zAxis, yAxis, xAxis;
            zAxis = plane.Normal;
            zAxis.Normalize();
            yAxis = upVector;
            yAxis.Normalize();
            xAxis = yAxis.Cross(zAxis);
            if(xAxis.Length == 0) {
                throw new AxiomException("The up vector for a plane cannot be parallel to the planes normal.");
            }

            rot3.FromAxes(xAxis, yAxis, zAxis);
            rot = rot3;

            // set up standard xform from origin
            xlate.Translation = plane.Normal * -plane.D;

            // concatenate
            xform = xlate * rot;

            // generate vertex data, imagine a large sphere with the camera located near the top,
            // the lower the curvature, the larger the sphere.  use the angle from the viewer to the
            // points on the plane
            float cameraPosition;      // camera position relative to the sphere center

            // derive sphere radius (unused)
            //float sphereDistance;      // distance from the camera to the sphere along box vertex vector
            float sphereRadius;

            // actual values irrelevant, it's the relation between the sphere's radius and the camera's position which is important
            float SPHERE_RADIUS = 100;
            float CAMERA_DISTANCE = 5;
            sphereRadius = SPHERE_RADIUS - curvature;
            cameraPosition = sphereRadius - CAMERA_DISTANCE;

            // lock the whole buffer
            float xSpace = width / xSegments;
            float ySpace = height / ySegments;
            float halfWidth = width / 2;
            float halfHeight = height / 2;
            Vector3 vec = Vector3.Zero;
            Vector3 norm = Vector3.Zero;
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;
            float maxSquaredLength = 0;
            bool firstTime = true;

            // generate vertex data
            GenerateCurvedIllusionPlaneVertexData(vertexBuffer, ySegments, xSegments, xSpace, halfWidth, ySpace, halfHeight, xform, firstTime, normals, orientation, cameraPosition, sphereRadius, uTiles, vTiles, numberOfTexCoordSets, ref min, ref max, ref maxSquaredLength);

            // generate face list
            subMesh.useSharedVertices = true;
            Tesselate2DMesh(subMesh, xSegments + 1, ySegments + 1, false, indexBufferUsage, indexShadowBuffer);

            // generate bounds for the mesh
            mesh.BoundingBox = new AxisAlignedBox(min, max);
            mesh.BoundingSphereRadius = MathUtil.Sqrt(maxSquaredLength);

            mesh.Load();
            mesh.Touch();

            return mesh;
        }

        private static void GenerateCurvedIllusionPlaneVertexData(HardwareVertexBuffer vertexBuffer, int ySegments, int xSegments, float xSpace, float halfWidth, float ySpace, float halfHeight, Matrix4 xform, bool firstTime, bool normals, Quaternion orientation, float cameraPosition, float sphereRadius, float uTiles, float vTiles, int numberOfTexCoordSets, ref Vector3 min, ref Vector3 max, ref float maxSquaredLength)
        {
            Vector3 vec;
            Vector3 norm;
            float sphereDistance;
            unsafe {
                // lock the vertex buffer
                IntPtr data = vertexBuffer.Lock(BufferLocking.Discard);

                float* pData = (float*)data.ToPointer();

                for (int y = 0; y < ySegments + 1; ++y) {
                    for (int x = 0; x < xSegments + 1; ++x) {
                        // centered on origin
                        vec.x = (x * xSpace) - halfWidth;
                        vec.y = (y * ySpace) - halfHeight;
                        vec.z = 0.0f;

                        // transform by orientation and distance
                        vec = xform * vec;

                        // assign to geometry
                        *pData++ = vec.x;
                        *pData++ = vec.y;
                        *pData++ = vec.z;

                        // build bounds as we go
                        if (firstTime) {
                            min = vec;
                            max = vec;
                            maxSquaredLength = vec.LengthSquared;
                            firstTime = false;
                        } else {
                            min.Floor(vec);
                            max.Ceil(vec);
                            maxSquaredLength = MathUtil.Max(maxSquaredLength, vec.LengthSquared);
                        }

                        if (normals) {
                            norm = Vector3.UnitZ;
                            norm = orientation * norm;

                            *pData++ = vec.x;
                            *pData++ = vec.y;
                            *pData++ = vec.z;
                        }

                        // generate texture coordinates, normalize position, modify by orientation to return +y up
                        vec = orientation.Inverse() * vec;
                        vec.Normalize();

                        // find distance to sphere
                        sphereDistance = MathUtil.Sqrt(cameraPosition * cameraPosition * (vec.y * vec.y - 1.0f) + sphereRadius * sphereRadius) - cameraPosition * vec.y;

                        vec.x *= sphereDistance;
                        vec.z *= sphereDistance;

                        // use x and y on sphere as texture coordinates, tiled
                        float s = vec.x * (0.01f * uTiles);
                        float t = vec.z * (0.01f * vTiles);
                        for (int i = 0; i < numberOfTexCoordSets; i++) {
                            *pData++ = s;
                            *pData++ = (1 - t);
                        }
                    } // x
                } // y

                // unlock the buffer
                vertexBuffer.Unlock();
            } // unsafe
        }

        private void CreatePrefabPlane() {
            Mesh mesh = (Mesh) Create("Prefab_Plane");
            SubMesh subMesh = mesh.CreateSubMesh("Prefab_Plane_Submesh");

            float[] vertices = {
                -100, -100, 0,  // pos
                0, 0, 1,        // normal
                0, 1,           // texcoord
                100, -100, 0,
                0, 0, 1,
                1, 1,
                100, 100, 0,
                0, 0, 1,
                1, 0,
                -100, 100, 0,
                0, 0, 1,
                0, 0
            };

            mesh.SharedVertexData = new VertexData();
            mesh.SharedVertexData.vertexCount = 4;
            VertexDeclaration vertexDeclaration = mesh.SharedVertexData.vertexDeclaration;
            VertexBufferBinding binding = mesh.SharedVertexData.vertexBufferBinding;

            int offset = 0;

            vertexDeclaration.AddElement(0, offset, VertexElementType.Float3, VertexElementSemantic.Position);
            offset += VertexElement.GetTypeSize(VertexElementType.Float3);

            vertexDeclaration.AddElement(0, offset, VertexElementType.Float3, VertexElementSemantic.Normal);
            offset += VertexElement.GetTypeSize(VertexElementType.Float3);

            vertexDeclaration.AddElement(0, offset, VertexElementType.Float2, VertexElementSemantic.TexCoords, 0);
            offset += VertexElement.GetTypeSize(VertexElementType.Float2);

            // allocate vertex buffer
            HardwareVertexBuffer vertexBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(offset, 4, BufferUsage.StaticWriteOnly);

            // set up the binding, one source only
            binding.SetBinding(0, vertexBuffer);

            vertexBuffer.WriteData(0, vertexBuffer.Size, vertices, true);

            subMesh.useSharedVertices = true;

            HardwareIndexBuffer indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(IndexType.Size16, 6, BufferUsage.StaticWriteOnly);
            short[] faces = {0, 1, 2, 0, 2, 3};
            subMesh.indexData.indexBuffer = indexBuffer;
            subMesh.indexData.indexCount = 6;
            subMesh.indexData.indexStart = 0;
            indexBuffer.WriteData(0, indexBuffer.Size, faces, true);

            mesh.BoundingBox = new AxisAlignedBox(new Vector3(-100, -100, 0), new Vector3(100, 100, 0));
            mesh.BoundingSphereRadius = MathUtil.Sqrt(100 * 100 + 100 * 100);

            resourceList.Add(mesh.Name, mesh);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        public Mesh Load(string name) {
            return Load(name, BufferUsage.StaticWriteOnly, BufferUsage.StaticWriteOnly, true, true, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        public Mesh Load(string name, BufferUsage vertexBufferUsage, BufferUsage indexBufferUsage) {
            return Load(name, vertexBufferUsage, indexBufferUsage, true, true, 1);
        }

        public Mesh Load(string name, BufferUsage vertexBufferUsage, BufferUsage indexBufferUsage, bool vertexBufferShadowed, bool indexBufferShadowed, int priority) {
            Mesh mesh = null;

            // if the resource isn't cached, create it
            if(!resourceList.ContainsKey(name)) {
                mesh = (Mesh)Create(name);
                mesh.SetVertexBufferPolicy(vertexBufferUsage, vertexBufferShadowed);
                mesh.SetIndexBufferPolicy(indexBufferUsage, indexBufferShadowed);
                base.Load(mesh, priority);
            }
            else {
                // get the cached version
                mesh = (Mesh)resourceList[name];
            }

            return mesh;
        }

        /// <summary>
        ///		Used to generate a face list based on vertices.
        /// </summary>
        /// <param name="subMesh"></param>
        /// <param name="xSegments"></param>
        /// <param name="ySegments"></param>
        /// <param name="doubleSided"></param>
        private void Tesselate2DMesh(SubMesh subMesh, int width, int height, bool doubleSided, BufferUsage indexBufferUsage, bool indexShadowBuffer) {
            int vInc, uInc, v, u, iterations;
            int vCount, uCount;

            vInc = 1;
            v = 0;

            iterations = doubleSided ? 2 : 1;

            // setup index count
            subMesh.indexData.indexCount = (width - 1) * (height - 1) * 2 * iterations * 3;

            // create the index buffer using the current API
            subMesh.indexData.indexBuffer = 
                HardwareBufferManager.Instance.CreateIndexBuffer(IndexType.Size16, subMesh.indexData.indexCount, indexBufferUsage, indexShadowBuffer);

            short v1, v2, v3;

            // grab a reference for easy access
            HardwareIndexBuffer idxBuffer = subMesh.indexData.indexBuffer;

            // lock the whole index buffer
            IntPtr data = idxBuffer.Lock(BufferLocking.Discard);

            unsafe {
                short* pIndex = (short*)data.ToPointer();

                while(0 < iterations--) {
                    // make tris in a zigzag pattern (strip compatible)
                    u = 0;
                    uInc = 1;

                    vCount = height - 1;

                    while(0 < vCount--) {
                        uCount = width - 1;

                        while(0 < uCount--) {
                            // First Tri in cell
                            // -----------------
                            v1 = (short)(((v + vInc) * width) + u);
                            v2 = (short)((v * width) + u);
                            v3 = (short)(((v + vInc) * width) + (u + uInc));
                            // Output indexes
                            *pIndex++ = v1;
                            *pIndex++ = v2;
                            *pIndex++ = v3;
                            // Second Tri in cell
                            // ------------------
                            v1 = (short)(((v + vInc) * width) + (u + uInc));
                            v2 = (short)((v * width) + u);
                            v3 = (short)((v * width) + (u + uInc));
                            // Output indexes
                            *pIndex++ = v1;
                            *pIndex++ = v2;
                            *pIndex++ = v3;

                            // Next column
                            u += uInc;

                        } // while uCount

                        v += vInc;
                        u = 0;

                    } // while vCount

                    v = height - 1;
                    vInc = - vInc;
                } // while iterations
            }// unsafe

            // unlock the buffer
            idxBuffer.Unlock();
        }

        public new Mesh GetByName(string name) {
            return (Mesh)base.GetByName(name);
        }

        /// <summary>
		///		Gets a material with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public new Mesh LoadExisting(string name) 
		{
			return (Mesh)base.LoadExisting(name);
		}

		public Mesh this[string name] 
		{
			get{return (Mesh)base.GetByName(name);}
		}
		
		public Mesh this[int handle] 
		{
			get{return (Mesh)base.GetByHandle(handle);}
		}
    }
}
