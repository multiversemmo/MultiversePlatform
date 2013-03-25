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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Axiom.Collections;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Graphics {
	/// <summary>
	///     General utility class for collecting the set of all
	///     triangles in a mesh; used for picking
    /// </summary>
	public class TriangleListBuilder : AnyBuilder {

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TriangleListBuilder));

        #region Methods

        public List<TriangleVertices> Build() {
            List<TriangleVertices> triangles = new  List<TriangleVertices>();
			try {
                for (int ind = 0, indexSet = 0; ind < indexDataList.Count; ind++, indexSet++) {
                    int vertexSet = (int)indexDataVertexDataSetList[ind];

                    IndexData indexData = (IndexData)indexDataList[indexSet];
                    OperationType opType = operationTypes[indexSet];

                    int iterations = 0;

                    switch (opType) {
					case OperationType.TriangleList:
						iterations = indexData.indexCount / 3;
						break;

					case OperationType.TriangleFan:
					case OperationType.TriangleStrip:
						iterations = indexData.indexCount - 2;
						break;
                    }

                    // locate position element & the buffer to go with it
                    VertexData vertexData = (VertexData)vertexDataList[vertexSet];
                    VertexElement posElem = 
                        vertexData.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.Position);

                    HardwareVertexBuffer posBuffer = vertexData.vertexBufferBinding.GetBuffer(posElem.Source);
                    try {
                        IntPtr posPtr = posBuffer.Lock(BufferLocking.ReadOnly);
                        IntPtr idxPtr = indexData.indexBuffer.Lock(BufferLocking.ReadOnly);

                        unsafe {
                            byte* pBaseVertex = (byte*)posPtr.ToPointer();

                            ushort* p16Idx = null;
                            uint* p32Idx = null;

                            // counters used for pointer indexing
                            int count16 = 0;
                            int count32 = 0;

                            if(indexData.indexBuffer.Type == IndexType.Size16) {
                                p16Idx = (ushort*)idxPtr.ToPointer();
                            }
                            else {
                                p32Idx = (uint*)idxPtr.ToPointer();
                            }

                            float* pReal = null;

                            int triStart = triangles.Count;

                            // iterate over all the groups of 3 indices
                            triangles.Capacity = triStart + iterations;

                            uint[] index = new uint[3];

                            for(int t = 0; t < iterations; t++) {
                                Vector3[] v = new Vector3[3];
                                TriangleVertices tri = new TriangleVertices(v);
                                bool broken = false;
                                for(int i = 0; i < 3; i++) {
                                    // Standard 3-index read for tri list or first tri in strip / fan
                                    if (opType == OperationType.TriangleList || t == 0) {
                                        if (indexData.indexBuffer.Type == IndexType.Size32) {
                                            index[i] = p32Idx[count32++];
                                        }
                                        else {
                                            index[i] = p16Idx[count16++];
                                        }
                                    }
                                    else {
                                        // Strips and fans are formed from last 2 indexes plus the 
                                        // current one for triangles after the first
                                        if (indexData.indexBuffer.Type == IndexType.Size32) {
                                            index[i] = p32Idx[i - 2];
                                        }
                                        else {
                                            index[i] = p16Idx[i - 2];
                                        }

                                        // Perform single-index increment at the last tri index
                                        if (i == 2) {
                                            if (indexData.indexBuffer.Type == IndexType.Size32) {
                                                count32++;
                                            }
                                            else {
                                                count16++;
                                            }
                                        }
                                    }

                                    // Retrieve the vertex position
                                    if (index[i] >= posBuffer.VertexCount) {
                                        log.InfoFormat("TriangleListBuilder.Build: Error: index[i] {0} is not less than posBuffer.VertexCount {1}, iteration t {2}",
                                            index[i], posBuffer.VertexCount, t);
                                        broken = true;
                                        break;
                                    }
                                    Debug.Assert(index[i] < posBuffer.VertexCount);
                                    byte* pVertex = pBaseVertex + (index[i] * posBuffer.VertexSize);
                                    pReal = (float*)(pVertex + posElem.Offset);
                                    v[i].x = *pReal++;
                                    v[i].y = *pReal++;
                                    v[i].z = *pReal++;
                                }
                                if (!broken) {
                                    // Put the points in in counter-clockwise order
                                    if (((v[0].x - v[2].x) * (v[1].y - v[2].y) - (v[1].x - v[2].x) * (v[0].y - v[2].y)) < 0) {
                                        // Clockwise, so reverse points 1 and 2
                                        Vector3 tmp = v[1];
                                        v[1] = v[2];
                                        v[2] = tmp;
                                    }
                                    // Debug.Assert(((v[0].x - v[2].x) * (v[1].y - v[2].y) - (v[1].x - v[2].x) * (v[0].y - v[2].y)) >= 0);
                                    // Add to the list of triangles
                                    triangles.Add(new TriangleVertices(v));
                                }
                            } // for iterations
                        } // unsafe
                    }
                    finally {
                        // unlock those buffers!
                        indexData.indexBuffer.Unlock();
                        posBuffer.Unlock();
                    }
                }
                return triangles;
            }
            catch (Exception e) {
                log.ErrorFormat("TriangleListBuilder.Build: Exception raised during triangle building: {0}", e.Message);
                triangles.Clear();
                return triangles;
            }
        }
        
        #endregion Methods
	}
	
	public class TriangleVertices {
		protected Vector3[] vertices;
		public TriangleVertices(Vector3[] vertices) {
			this.vertices = vertices;
		}

		public Vector3[] Vertices {
			get {
				return vertices;
			}
		}
	}

	public class TriangleIntersector {
		#region Fields

		List<TriangleVertices> triangles;
		
		#endregion Fields

		#region Contructor

		public TriangleIntersector(List<TriangleVertices> triangles) {
			this.triangles = triangles;
		}
			
		#endregion Constructor

		#region Public Methods

		public bool ClosestRayIntersection(Ray ray, Vector3 modelBase, float ignoreDistance, out Vector3 intersection) {
			bool intersects = false;
			intersection = Vector3.Zero;
			float minDistSquared = float.MaxValue;
			float ignoreDistanceSquared = ignoreDistance * ignoreDistance;
			Vector3 where = Vector3.Zero;
			// Iterate over the triangles
			for (int i = 0; i < triangles.Count; i++) {
				TriangleVertices t = triangles[i];
				if (RayIntersectsTriangle(ray, t.Vertices, modelBase, ref where)) {
					float distSquared = (where - ray.Origin).LengthSquared;
					if (distSquared > ignoreDistanceSquared && distSquared < minDistSquared) {
						minDistSquared = distSquared;
						intersection = where;
						intersects = true;
					}
				}
			}
			return intersects;
		}
		
        #endregion Public Methods

		#region Protected Methods
		
		// Given line pq and ccw triangle abc, return whether line pierces triangle. If
		// so, also return the barycentric coordinates (u,v,w) of the intersection point
		bool RayIntersectsTriangle(Ray ray, Vector3 [] Vertices, 
								   Vector3 modelBase, ref Vector3 where) {
			// Place the end beyond any conceivable triangle, 1000 meters away
			Vector3 v0 = modelBase + Vertices[0];
			Vector3 v1 = modelBase + Vertices[1];
			Vector3 v2 = modelBase + Vertices[2];
            Vector3 start = ray.Origin;
			Vector3 dir = ray.Direction;
            /* find vectors for two edges sharing vert0 */
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            /* begin calculating determinant - also used to calculate U parameter */
            Vector3 pvec = dir.Cross(edge2);
            /* if determinant is near zero, ray lies in plane of triangle */
            float det = edge1.Dot(pvec);
            if (det > -1.0e-5 && det < 1.0e-5)
                return false;
            float invDet = 1.0f / det;
            /* calculate distance from vert0 to ray origin */
            Vector3 tvec = start - v0;
            /* calculate U parameter and test bounds */
            float u = tvec.Dot(pvec) * invDet;
            if (u < 0.0f || u > 1.0f)
                return false;
            /* prepare to test V parameter */
            Vector3 qvec = tvec.Cross(edge1);
            /* calculate V parameter and test bounds */
            float v = dir.Dot(qvec) * invDet;
            if (v < 0.0f || (u + v) > 1.0f)
                return false;
            /* calculate t, ray intersects triangle */
            float t = edge2.Dot(qvec) * invDet;
            where = start + dir * t;
            return true;
        }
        
		#endregion Protected Methods

	}

}

