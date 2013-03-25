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
using System.Diagnostics;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Graphics {
	/// <summary>
	///		This class defines the interface that must be implemented by shadow casters.
	/// </summary>
	public abstract class ShadowCaster {
		#region Properties

		/// <summary>
		///		Gets/Sets whether or not this object currently casts a shadow.
		/// </summary>
		public abstract bool CastShadows { get; set; }

		#endregion Properties

		#region Methods

		/// <summary>
		///		Gets the world space bounding box of the dark cap, as extruded using the light provided.
		/// </summary>
		/// <param name="light"></param>
		/// <param name="dirLightExtrusionDist"></param>
		/// <returns></returns>
		public abstract AxisAlignedBox GetDarkCapBounds(Light light, float dirLightExtrusionDist);

        /// <summary>
        ///		Gets details of the edges which might be used to determine a silhouette.
        /// </summary>
        /// <remarks>Defaults to LOD index 0.</remarks>
        public EdgeData GetEdgeList() {
            return GetEdgeList(0);
        }

        /// <summary>
        ///		Gets details of the edges which might be used to determine a silhouette.
        /// </summary>
        public abstract EdgeData GetEdgeList(int lodIndex);

        /// <summary>
		///		Gets the world space bounding box of the light cap.
		/// </summary>
		/// <returns></returns>
		public abstract AxisAlignedBox GetLightCapBounds();

		/// <summary>
		///		Get the world bounding box of the caster.
		/// </summary>
		/// <param name="derive"></param>
		/// <returns></returns>
		public abstract AxisAlignedBox GetWorldBoundingBox(bool derive);

		public AxisAlignedBox GetWorldBoundingBox() {
			return GetWorldBoundingBox(false);
		}

		/// <summary>
		///		Gets an iterator over the renderables required to render the shadow volume.
		/// </summary>
		/// <remarks>
		///		Shadowable geometry should ideally be designed such that there is only one
		///		ShadowRenderable required to render the the shadow; however this is not a necessary
		///		limitation and it can be exceeded if required.
		/// </remarks>
		/// <param name="technique">The technique being used to generate the shadow.</param>
		/// <param name="light">The light to generate the shadow from.</param>
		/// <param name="indexBuffer">The index buffer to build the renderables into, 
		/// the current contents are assumed to be disposable.</param>
		/// <param name="extrudeVertices">If true, this means this class should extrude
		/// the vertices of the back of the volume in software. If false, it
		/// will not be done (a vertex program is assumed).</param>
		/// <param name="flags">Technique-specific flags, see <see cref="ShadowRenderableFlags"/></param>
		/// <returns>An iterator that will allow iteration over all renderables for the full shadow volume.</returns>
		public abstract IEnumerator GetShadowVolumeRenderableEnumerator(ShadowTechnique technique, Light light,
			HardwareIndexBuffer indexBuffer, bool extrudeVertices, float extrusionDistance, int flags);

		public IEnumerator GetShadowVolumeRenderableEnumerator(ShadowTechnique technique, Light light,
			HardwareIndexBuffer indexBuffer, float extrusionDistance, bool extrudeVertices) {

			return GetShadowVolumeRenderableEnumerator(technique, light, indexBuffer, extrudeVertices, extrusionDistance, 0);
		}

		/// <summary>
		///		Return the last calculated shadow renderables.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerator GetLastShadowVolumeRenderableEnumerator();

		/// <summary>
		///		Utility method for extruding vertices based on a light.
		/// </summary>
		/// <remarks>
		///		Unfortunately, because D3D cannot handle homogenous (4D) position
		///		coordinates in the fixed-function pipeline (GL can, but we have to
		///		be cross-API), when we extrude in software we cannot extrude to 
		///		infinity the way we do in the vertex program (by setting w to
		///		0.0f). Therefore we extrude by a fixed distance, which may cause 
		///		some problems with larger scenes. Luckily better hardware (ie
		///		vertex programs) can fix this.
		/// </remarks>
		/// <param name="vertexBuffer">The vertex buffer containing ONLY xyz position
		/// values, which must be originalVertexCount * 2 * 3 floats long.</param>
		/// <param name="originalVertexCount">The count of the original number of
		/// vertices, ie the number in the mesh, not counting the doubling
		/// which has already been done (by <see cref="VertexData.PrepareForShadowVolume"/>)
		/// to provide the extruded area of the buffer.</param>
		/// <param name="lightPosition"> 4D light position in object space, when w=0.0f this
		/// represents a directional light</param>
		/// <param name="extrudeDistance">The distance to extrude.</param>
		public static void ExtrudeVertices(HardwareVertexBuffer vertexBuffer, int originalVertexCount, Vector4 lightPosition, float extrudeDistance) {
			unsafe {
				Debug.Assert(vertexBuffer.VertexSize == sizeof(float) * 3, "Position buffer should contain only positions!");

				// Extrude the first area of the buffer into the second area
				// Lock the entire buffer for writing, even though we'll only be
				// updating the latter because you can't have 2 locks on the same
				// buffer
				IntPtr srcPtr = vertexBuffer.Lock(BufferLocking.Normal);
				IntPtr destPtr = new IntPtr(srcPtr.ToInt32() + (originalVertexCount * 3 * 4));
				float* pSrc = (float*)srcPtr.ToPointer();
				float* pDest = (float*)destPtr.ToPointer();

				int destCount = 0, srcCount = 0;

				// Assume directional light, extrusion is along light direction
				Vector3 extrusionDir = new Vector3(-lightPosition.x, -lightPosition.y, -lightPosition.z);
				extrusionDir.Normalize();
				extrusionDir *= extrudeDistance;

				for (int vert = 0; vert < originalVertexCount; vert++) {
					if (lightPosition.w != 0.0f) {
						// Point light, adjust extrusionDir
						extrusionDir.x = pSrc[srcCount + 0] - lightPosition.x;
						extrusionDir.y = pSrc[srcCount + 1] - lightPosition.y;
						extrusionDir.z = pSrc[srcCount + 2] - lightPosition.z;
						extrusionDir.Normalize();
						extrusionDir *= extrudeDistance;
					}

					pDest[destCount++] = pSrc[srcCount++] + extrusionDir.x;
					pDest[destCount++] = pSrc[srcCount++] + extrusionDir.y;
					pDest[destCount++] = pSrc[srcCount++] + extrusionDir.z;
				}
			}

			vertexBuffer.Unlock();
		}

		/// <summary>
		///		Tells the caster to perform the tasks necessary to update the 
		///		edge data's light listing. Can be overridden if the subclass needs 
		///		to do additional things.
		/// </summary>
		/// <param name="edgeData">The edge information to update.</param>
		/// <param name="lightPosition">4D vector representing the light, a directional light has w=0.0.</param>
		protected virtual void UpdateEdgeListLightFacing(EdgeData edgeData, Vector4 lightPosition) {
			edgeData.UpdateTriangleLightFacing(lightPosition);
		}

		/// <summary>
		///		Generates the indexes required to render a shadow volume into the 
		///		index buffer which is passed in, and updates shadow renderables to use it.
		/// </summary>
		/// <param name="edgeData">The edge information to use.</param>
		/// <param name="indexBuffer">The buffer into which to write data into; current 
		///	contents are assumed to be discardable.</param>
		/// <param name="light">The light, mainly for type info as silhouette calculations
		/// should already have been done in <see cref="UpdateEdgeListLightFacing"/></param>
		/// <param name="shadowRenderables">A list of shadow renderables which has 
		/// already been constructed but will need populating with details of
		/// the index ranges to be used.</param>
		/// <param name="flags">Additional controller flags, see <see cref="ShadowRenderableFlags"/>.</param>
		protected virtual void GenerateShadowVolume(EdgeData edgeData, HardwareIndexBuffer indexBuffer, Light light, 
			ShadowRenderableList shadowRenderables, int flags) {

			// Edge groups should be 1:1 with shadow renderables
			Debug.Assert(edgeData.edgeGroups.Count == shadowRenderables.Count);

			LightType lightType = light.Type;

            bool extrudeToInfinity = (flags & (int)ShadowRenderableFlags.ExtrudeToInfinity) > 0;

            // Lock index buffer for writing
			IntPtr idxPtr = indexBuffer.Lock(BufferLocking.Discard);

            int indexStart = 0;

            unsafe {
				// TODO: Will currently cause an overflow for 32 bit indices, revisit
				short* pIdx = (short*)idxPtr.ToPointer();
				int count = 0;

				// Iterate over the groups and form renderables for each based on their
				// lightFacing
				for(int groupCount = 0; groupCount < edgeData.edgeGroups.Count; groupCount++) {
					EdgeData.EdgeGroup eg = (EdgeData.EdgeGroup)edgeData.edgeGroups[groupCount];
					ShadowRenderable si = (ShadowRenderable)shadowRenderables[groupCount];

                    RenderOperation lightShadOp = null;

                    // Initialise the index bounds for this shadow renderable
					RenderOperation shadOp = si.GetRenderOperationForUpdate();
					shadOp.indexData.indexCount = 0;
					shadOp.indexData.indexStart = indexStart;

					// original number of verts (without extruded copy)
					int originalVertexCount = eg.vertexData.vertexCount;
					bool firstDarkCapTri = true;
					int darkCapStart = 0;

					for (int edgeCount = 0; edgeCount < eg.edges.Count; edgeCount++) {
						EdgeData.Edge edge = (EdgeData.Edge)eg.edges[edgeCount];

                        EdgeData.Triangle t1 = (EdgeData.Triangle)edgeData.triangles[edge.triIndex[0]];
						EdgeData.Triangle t2 = 
                            edge.isDegenerate ? (EdgeData.Triangle)edgeData.triangles[edge.triIndex[0]] : (EdgeData.Triangle)edgeData.triangles[edge.triIndex[1]];

						if (t1.lightFacing && (edge.isDegenerate || !t2.lightFacing)) {
							/* Silhouette edge, first tri facing the light
							Also covers degenerate tris where only tri 1 is valid
							Remember verts run anticlockwise along the edge from 
							tri 0 so to point shadow volume tris outward, light cap 
							indexes have to be backwards

							We emit 2 tris if light is a point light, 1 if light 
							is directional, because directional lights cause all
							points to converge to a single point at infinity.

							First side tri = near1, near0, far0
							Second tri = far0, far1, near1

							'far' indexes are 'near' index + originalVertexCount
							because 'far' verts are in the second half of the 
							buffer
							*/
							pIdx[count++] = (short)edge.vertIndex[1];
							pIdx[count++] = (short)edge.vertIndex[0];
							pIdx[count++] = (short)(edge.vertIndex[0] + originalVertexCount);
							shadOp.indexData.indexCount += 3;

							if (!(lightType == LightType.Directional && extrudeToInfinity)) {
							    // additional tri to make quad
							    pIdx[count++] = (short)(edge.vertIndex[0] + originalVertexCount);
							    pIdx[count++] = (short)(edge.vertIndex[1] + originalVertexCount);
							    pIdx[count++] = (short)edge.vertIndex[1];
							    shadOp.indexData.indexCount += 3;
							}

							// Do dark cap tri
							// Use McGuire et al method, a triangle fan covering all silhouette
							// edges and one point (taken from the initial tri)
							if ((flags & (int)ShadowRenderableFlags.IncludeDarkCap) > 0) {
								if (firstDarkCapTri) {
									darkCapStart = edge.vertIndex[0] + originalVertexCount;
									firstDarkCapTri = false;
								}
								else {
									pIdx[count++] = (short)darkCapStart;
									pIdx[count++] = (short)(edge.vertIndex[1] + originalVertexCount);
									pIdx[count++] = (short)(edge.vertIndex[0] + originalVertexCount);
									shadOp.indexData.indexCount += 3;
								}
							}
						}
						else if (!t1.lightFacing && (edge.isDegenerate || t2.lightFacing)) {
							// Silhouette edge, second tri facing the light
							// Note edge indexes inverse of when t1 is light facing 
							pIdx[count++] = (short)edge.vertIndex[0];
							pIdx[count++] = (short)edge.vertIndex[1];
							pIdx[count++] = (short)(edge.vertIndex[1] + originalVertexCount);
							shadOp.indexData.indexCount += 3;

							if (!(lightType == LightType.Directional && extrudeToInfinity)) {
							    // additional tri to make quad
							    pIdx[count++] = (short)(edge.vertIndex[1] + originalVertexCount);
							    pIdx[count++] = (short)(edge.vertIndex[0] + originalVertexCount);
							    pIdx[count++] = (short)edge.vertIndex[0];
							    shadOp.indexData.indexCount += 3;
							}

							// Do dark cap tri
							// Use McGuire et al method, a triangle fan covering all silhouette
							// edges and one point (taken from the initial tri)
							if ((flags & (int)ShadowRenderableFlags.IncludeDarkCap) > 0) {
								if (firstDarkCapTri) {
									darkCapStart = edge.vertIndex[1] + originalVertexCount;
									firstDarkCapTri = false;
								}
								else {
									pIdx[count++] = (short)darkCapStart;
									pIdx[count++] = (short)(edge.vertIndex[0] + originalVertexCount);
									pIdx[count++] = (short)(edge.vertIndex[1] + originalVertexCount);
									shadOp.indexData.indexCount += 3;
								}
							}
						}
					}

					// Do light cap
					if ((flags & (int)ShadowRenderableFlags.IncludeLightCap) > 0) {
						ShadowRenderable lightCapRend = null;

						if(si.IsLightCapSeperate) {
							// separate light cap
							lightCapRend = si.LightCapRenderable;
							lightShadOp = lightCapRend.GetRenderOperationForUpdate();
							lightShadOp.indexData.indexCount = 0;
							// start indexes after the current total
							// NB we don't update the total here since that's done below
							lightShadOp.indexData.indexStart = 
								indexStart + shadOp.indexData.indexCount;
						}

						for(int triCount = 0; triCount < edgeData.triangles.Count; triCount++) {
							EdgeData.Triangle t = (EdgeData.Triangle)edgeData.triangles[triCount];

							// Light facing, and vertex set matches
							if (t.lightFacing && t.vertexSet == eg.vertexSet) {
								pIdx[count++] = (short)t.vertIndex[0];
								pIdx[count++] = (short)t.vertIndex[1];
								pIdx[count++] = (short)t.vertIndex[2];

								if(lightShadOp != null) {
									lightShadOp.indexData.indexCount += 3;
								}
								else {
									shadOp.indexData.indexCount += 3;
								}
							}
						}
					}

					// update next indexStart (all renderables sharing the buffer)
					indexStart += shadOp.indexData.indexCount;

                    // add on the light cap too
                    if (lightShadOp != null) {
                        indexStart += lightShadOp.indexData.indexCount;
                    }
                }
			}

			// Unlock index buffer
			indexBuffer.Unlock();

            Debug.Assert(indexStart <= indexBuffer.IndexCount, "Index buffer overrun while generating shadow volume!");
        }

		/// <summary>
		///		Utility method for extruding a bounding box.
		/// </summary>
		/// <param name="box">Original bounding box, will be updated in-place.</param>
		/// <param name="lightPosition">4D light position in object space, when w=0.0f this
		/// represents a directional light</param>
		/// <param name="extrudeDistance">The distance to extrude.</param>
		protected virtual void ExtrudeBounds(AxisAlignedBox box, Vector4 lightPosition, float extrudeDistance) {
			Vector3 extrusionDir = Vector3.Zero;

			if (lightPosition.w == 0) {
				extrusionDir.x = -lightPosition.x;
				extrusionDir.y = -lightPosition.y;
				extrusionDir.z = -lightPosition.z;
				extrusionDir.Normalize();
				extrusionDir *= extrudeDistance;
				box.SetExtents(box.Minimum + extrusionDir, box.Maximum + extrusionDir);
			}
			else {
				Vector3[] corners = box.Corners;
				Vector3 vmin = new Vector3();
				Vector3 vmax = new Vector3();
  
				for(int i = 0; i < 8; i++) {
					extrusionDir.x = corners[i].x - lightPosition.x;
					extrusionDir.y = corners[i].y - lightPosition.y;
					extrusionDir.z = corners[i].z - lightPosition.z;
					extrusionDir.Normalize();
					extrusionDir *= extrudeDistance;
					Vector3 res = corners[i] + extrusionDir;
 
					if(i == 0) {
						vmin = res;
						vmax = res;
					}
					else {
						vmin.Floor(res);
						vmax.Ceil(res);
					}
				}

				box.SetExtents(vmin, vmax);
			}
		}

		/// <summary>
		///		Helper method for calculating extrusion distance.
		/// </summary>
		/// <param name="objectPos"></param>
		/// <param name="light"></param>
		/// <returns></returns>
		protected float GetExtrusionDistance(Vector3 objectPos, Light light) {
			Vector3 diff = objectPos - light.DerivedPosition;
			return light.AttenuationRange - diff.Length;
		}

		/// <summary>
		///		Get the distance to extrude for a point/spot light.
		/// </summary>
		/// <param name="light"></param>
		/// <returns></returns>
		public abstract float GetPointExtrusionDistance(Light light);

		#endregion Methods
	}
}
