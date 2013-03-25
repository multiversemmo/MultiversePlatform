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
using System.Diagnostics;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Graphics {
	/// <summary>
    ///     This class contains the information required to describe the edge connectivity of a
    ///     given set of vertices and indexes. 
	/// </summary>
	public class EdgeData {
        #region Fields

        /// <summary>
        ///     List of triangles.
        /// </summary>
        protected internal TriangleList triangles = new TriangleList();
        /// <summary>
        ///     List of edge groups.
        /// </summary>
        protected internal EdgeGroupList edgeGroups = new EdgeGroupList();

        /// <summary>
        ///     Accessor needed by Region.cs
        /// </summary>
        public EdgeGroupList EdgeGroups {
            get {
                return edgeGroups;
            }
        }
        
        #endregion Fields
		
        #region Methods

        /// <summary>
        ///     Calculate the light facing state of the triangles in this edge list.
        /// </summary>
        /// <remarks>
        ///     This is normally the first stage of calculating a silhouette, ie
        ///     establishing which tris are facing the light and which are facing
        ///     away. This state is stored in the 'lightFacing' flag in each 
        ///     Triangle.
        /// </remarks>
        /// <param name="lightPos">
        ///     4D position of the light in object space, note that 
        ///     for directional lights (which have no position), the w component
        ///     is 0 and the x/y/z position are the direction.
        /// </param>
        public void UpdateTriangleLightFacing(Vector4 lightPos) {
            for(int i = 0; i < triangles.Count; i++) {
                Triangle tri = (Triangle)triangles[i];

                float dot = tri.normal.Dot(lightPos);
			
                tri.lightFacing = (dot > 0);
            }
        }

		/// <summary>
		///		Updates the face normals for this edge list based on (changed)
		///		position information, useful for animated objects. 
		/// </summary>
		/// <param name="vertexSet">The vertex set we are updating.</param>
		/// <param name="positionBuffer">The updated position buffer, must contain ONLY xyz.</param>
		public void UpdateFaceNormals(int vertexSet, HardwareVertexBuffer positionBuffer) {
			unsafe {
				Debug.Assert(positionBuffer.VertexSize == sizeof(float) * 3, "Position buffer should contain only positions!");

				// Lock buffer for reading
				IntPtr posPtr = positionBuffer.Lock(BufferLocking.ReadOnly);
				float* pVert = (float*)posPtr.ToPointer();

				// Iterate over the triangles
				for (int i = 0; i < triangles.Count; i++) {
					Triangle t = (Triangle)triangles[i];

					// Only update tris which are using this vertex set
					if (t.vertexSet == vertexSet) {
						int offset = t.vertIndex[0] * 3;
						Vector3 v1 = new Vector3(pVert[offset], pVert[offset + 1], pVert[offset + 2]);

						offset = t.vertIndex[1] * 3;
						Vector3 v2 = new Vector3(pVert[offset], pVert[offset + 1], pVert[offset + 2]);

						offset = t.vertIndex[2] * 3;
						Vector3 v3 = new Vector3(pVert[offset], pVert[offset + 1], pVert[offset + 2]);

						t.normal = MathUtil.CalculateFaceNormal(v1, v2, v3);
					}
				}
			}

			// unlock the buffer
			positionBuffer.Unlock();
		}

		public void DebugLog(Log log) {
            // TODO: 
            log.Write("Edge Data");
            log.Write("---------");

            for (int i = 0; i < triangles.Count; i++) {
				Triangle t = (Triangle)triangles[i];

				log.Write("Triangle {0} = [indexSet={1}, vertexSet={2}, v0={3}, v1={4}, v2={5}]", 
					i, t.indexSet, t.vertexSet, t.vertIndex[0], t.vertIndex[1], t.vertIndex[2]);
			}

			for (int i = 0; i < edgeGroups.Count; i++) {
				EdgeGroup group = (EdgeGroup)edgeGroups[i];

				log.Write("Edge Group vertexSet={0}", group.vertexSet);

				for (int j = 0; j < group.edges.Count; j++) {
					Edge e = (Edge)group.edges[j];

					log.Write("Edge {0} = [\ntri0={1}, \ntri1={2}, \nv0={3}, \nv1={4}, \n degenerate={5}\n]", 
						j, e.triIndex[0], e.triIndex[1], e.vertIndex[0], e.vertIndex[1], e.isDegenerate); 
				}
			}
		}

        #endregion Methods

        #region Structures

		// Note: These would typically be candidates for structs, but their usage throughout
		// the engine is more appropriate as reference types, so hopefully the benefits will
		// outweigh the massive boxing/unboxing these types would go through as value types.

        /// <summary>
        ///     Basic triangle structure.
        /// </summary>
        public class Triangle {
            #region Fields

            /// <summary>
            ///     The set of indexes this triangle came from (NB it is possible that the triangles on 
            ///     one side of an edge are using a different vertex buffer from those on the other side.)
            /// </summary>
            public int indexSet; 
            /// <summary>
            ///     The vertex set these vertices came from.
            /// </summary>
            public int vertexSet;
            /// <summary>
            ///     Vertex indexes, relative to the original buffer.
            /// </summary>
            public int[] vertIndex;
            /// <summary>
            ///     Vertex indexes, relative to a shared vertex buffer with 
            //      duplicates eliminated (this buffer is not exposed).
            /// </summary>
            public int[] sharedVertIndex;
            /// <summary>
            ///      Unit vector othogonal to this face, plus distance from origin.
            /// </summary>
            public Vector4 normal;
            /// <summary>
            ///     Working vector used when calculating the silhouette.
            /// </summary>
            public bool lightFacing;

			/// <summary>
			///		Default contructor.
			/// </summary>
			public Triangle() {
				vertIndex = new int[3];
				sharedVertIndex = new int[3];
			}

			public override string ToString() {
				return string.Format("IndexSet {0} VertexSet {1} VertIndices({2},{3},{4}) SharedVerts({5},{6},{7}) Normal({8},{9},{10},{11}) LightFacing {12})",
					indexSet, vertexSet, vertIndex[0], vertIndex[1], vertIndex[2], sharedVertIndex[0], sharedVertIndex[1], sharedVertIndex[2],
					normal.x, normal.y, normal.z, normal.w, lightFacing);
			}


            #endregion Fields
        }

        /// <summary>
        ///     Edge data.
        /// </summary>
        public class Edge {
            #region Fields

            /// <summary>
            ///     The indexes of the 2 tris attached, note that tri 0 is the one where the 
            ///     indexes run *counter* clockwise along the edge. Indexes must be
            ///     reversed for tri 1.
            /// </summary>
            public int[] triIndex;
            /// <summary>
            ///     The vertex indices for this edge. Note that both vertices will be in the vertex
            ///     set as specified in 'vertexSet', which will also be the same as tri 0.
            /// </summary>
            public int[] vertIndex;
            /// <summary>
            ///     Vertex indices as used in the shared vertex list, not exposed.
            /// </summary>
            public int[] sharedVertIndex;
			/// <summary>
			///		Indicates if this is a degenerate edge, ie it does not have 2 triangles.
			/// </summary>
			public bool isDegenerate;

            #endregion Fields

			/// <summary>
			///		Default constructor.
			/// </summary>
			public Edge() {
				triIndex = new int[2];
				vertIndex = new int[2];
				sharedVertIndex = new int[2];
			}

			public override string ToString() {
				return string.Format("TriIndex({0},{1}) VertIndex({2},{3}) SharedVertIndex({4},{5}) IsDegenerate = {6}",
					triIndex[0], triIndex[1], vertIndex[0], vertIndex[1], sharedVertIndex[0], sharedVertIndex[1], isDegenerate);
			}

        }

        /// <summary>
        ///     A group of edges sharing the same vertex data.
        /// </summary>
        public class EdgeGroup {
            #region Fields

            /// <summary>
            ///     The vertex set index that contains the vertices for this edge group.
            /// </summary>
            public int vertexSet;
            /// <summary>
            ///     Reference to vertex data used by this edge group.
            /// </summary>
            public VertexData vertexData;
            /// <summary>
            ///     The edges themselves.
            /// </summary>
            public EdgeList edges = new EdgeList();

            #endregion Fields
        }

        #endregion Structs
	}
}
