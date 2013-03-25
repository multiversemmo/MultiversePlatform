using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Collections;

namespace Axiom.Core
{
	
	public class Region : MovableObject, IDisposable
	{
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Region));

		#region Inner Classes
		public class RegionShadowRenderable : ShadowRenderable
		{
			protected Region parent;
			protected HardwareVertexBuffer positionBuffer;
			protected HardwareVertexBuffer wBuffer;

			public RegionShadowRenderable(Region parent, HardwareIndexBuffer indexBuffer, VertexData vertexData, bool createSeparateLightCap, bool isLightCap)
			{
				throw new NotImplementedException();
			}

			public RegionShadowRenderable(Region parent, HardwareIndexBuffer indexBuffer, VertexData vertexData, bool createSeparateLightCap)
				: this(parent, indexBuffer, vertexData, createSeparateLightCap, false)
			{}

			public HardwareVertexBuffer PositionBuffer
			{
				get 
				{
					return positionBuffer;
				}
			}

			public HardwareVertexBuffer WBuffer
			{
				get 
				{
					return wBuffer;
				}
			}

			public override void GetWorldTransforms(Axiom.MathLib.Matrix4[] matrices)
			{
				matrices[0] = parent.ParentNodeFullTransform;
			}

			public override Quaternion WorldOrientation
			{ 
				get 
				{
					return parent.ParentNode.DerivedOrientation;
				}
			}

			public override Vector3 WorldPosition
			{
				get 
				{
					return parent.Center;
				}
			}
		}

		#endregion

		#region Fields and Properties
		protected StaticGeometry parent;
		protected SceneManager sceneMgr;
		protected SceneNode node;
		protected List<QueuedSubMesh> queuedSubMeshes;
		protected UInt32 regionID;
		protected Vector3 center;
		protected List<float> lodSquaredDistances;
		protected AxisAlignedBox aabb;
		protected float boundingRadius;
		protected ushort currentLod;
		protected float camDistanceSquared;
		protected List<LODBucket> lodBucketList;
		protected List<Light> lightList;
		protected ulong lightListUpdated;
		protected bool beyondFarDistance;
		protected EdgeData edgeList;
		protected ShadowRenderableList shadowRenderables;
		protected bool vertexProgramInUse;
			
		public StaticGeometry Parent
		{
			get 
			{
				return parent;
			}
		}

			
		public UInt32 ID
		{
			get 
			{
				return regionID;
			}
		}

		public Vector3 Center
		{
			get 
			{
				return center;
			}
		}

		public string MovableType
		{
			get 
			{
				return "StaticGeometry";
			}
		}

		public override AxisAlignedBox BoundingBox
		{
			get
			{
				return aabb;
			}
		}

        // TODO: Is this right?
        public ushort NumWorldTransforms
        {
            get
            {
                return 1;
            }
        }

		public override float BoundingRadius
		{
			get
			{
				return boundingRadius;
			}
		}
        
		public List<Light> Lights
		{
			get 
			{
				// Make sure we only update this once per frame no matter how many
				// times we're asked
				ulong frame = Root.Instance.CurrentFrameCount;
				if(frame > lightListUpdated)
				{
					lightList = node.FindLights(boundingRadius);
					lightListUpdated = frame;
				}
				return lightList;
			}
		}

		public EdgeData EdgeList
		{
			get 
			{
				return edgeList;
			}
		}

		public List<LODBucket> LodBucketList
		{
			get 
			{
				return lodBucketList;
			}
		}


		#endregion

		#region Constructors

		public Region(StaticGeometry parent, string name, SceneManager mgr, UInt32 regionID, Vector3 center)
		{
			this.parent = parent;
			this.name = name;
			this.sceneMgr = mgr;
			this.regionID = regionID;
			this.center = center;
            queuedSubMeshes = new List<QueuedSubMesh>();
            lodSquaredDistances = new List<float>();
            aabb = new AxisAlignedBox();
            lodBucketList = new List<LODBucket>();
            shadowRenderables = new ShadowRenderableList();
		}

		#endregion

		#region Public Methods
		public void Assign(QueuedSubMesh qsm)
		{
			queuedSubMeshes.Add(qsm);
			// update lod distances
			Mesh mesh = qsm.submesh.Parent;
            ushort lodLevels = (ushort)(mesh.IsLodManual ? 1 : mesh.LodLevelCount);
            if (qsm.geometryLodList.Count != lodLevels) {
                string msg = string.Format("QueuedSubMesh '{0}' lod count of {1} does not match parent count of {2}",
                    qsm.submesh.Name, qsm.geometryLodList.Count, lodLevels);
                throw new AxiomException(msg);
            }

			while(lodSquaredDistances.Count < lodLevels)
			{
				lodSquaredDistances.Add(0.0f);
			}
			// Make sure LOD levels are max of all at the requested level
			for(ushort lod = 1; lod < lodLevels; ++lod)
			{
				MeshLodUsage meshLod = qsm.submesh.Parent.GetLodLevel(lod);
				lodSquaredDistances[lod] = Math.Max((float)lodSquaredDistances[lod], meshLod.fromSquaredDepth);
			}

			// update bounds
			// Transform world bounds relative to our center
			AxisAlignedBox localBounds = new AxisAlignedBox(qsm.worldBounds.Minimum - center, qsm.worldBounds.Maximum - center);
			aabb.Merge(localBounds);
            foreach (Vector3 corner in localBounds.Corners)
                boundingRadius = Math.Max(boundingRadius, corner.Length);
		}

		// Returns the number of geometry buckets
        public int Build(bool stencilShadows, bool logDetails)
		{
			int bucketCount = 0;
            // Create a node
			node = sceneMgr.RootSceneNode.CreateChildSceneNode(name, center);
			node.AttachObject(this);
			// We need to create enough LOD buckets to deal with the highest LOD
			// we encountered in all the meshes queued
			for(ushort lod = 0; lod < lodSquaredDistances.Count; ++lod)
			{
				LODBucket lodBucket = new LODBucket(this, lod, (float)lodSquaredDistances[lod]);
				lodBucketList.Add(lodBucket);
				// Now iterate over the meshes and assign to LODs
				// LOD bucket will pick the right LOD to use
				foreach (QueuedSubMesh qsm in queuedSubMeshes)
					lodBucket.Assign(qsm, lod);
				// now build
				bucketCount += lodBucket.Build(stencilShadows, logDetails);
			}

			// Do we need to build an edge list?
			if(stencilShadows)
			{
				EdgeListBuilder eb = new EdgeListBuilder();
                foreach (LODBucket lod in lodBucketList) {
					foreach(MaterialBucket mat in lod.MaterialBucketMap.Values) {
						// Check if we have vertex programs here
						Technique t = mat.Material.GetBestTechnique();
						if(null != t)
						{
							Pass p = t.GetPass(0);
							if(null != p)
							{
								if(p.HasVertexProgram)
								{
									vertexProgramInUse = true;
								}
							}
						}

						foreach (GeometryBucket geom in mat.GeometryBucketList) {
                            bucketCount++;
							// Check we're dealing with 16-bit indexes here
							// Since stencil shadows can only deal with 16-bit
							// More than that and stencil is probably too CPU-heavy
							// in any case
							if(geom.IndexData.indexBuffer.Type != IndexType.Size16) throw new AxiomException("Only 16-bit indexes allowed when using stencil shadows");
							eb.AddVertexData(geom.VertexData);
							eb.AddIndexData(geom.IndexData);
						}
					}
				}
				edgeList = eb.Build();
			}
            return bucketCount;
		}

		public override void NotifyCurrentCamera(Camera cam)
		{
			// Determine active lod
			Vector3 diff = cam.DerivedPosition - center;
			// Distance from the edge of the bounding sphere
			camDistanceSquared = diff.LengthSquared - boundingRadius * boundingRadius;
			// Clamp to 0
			camDistanceSquared = Math.Max(0.0f, camDistanceSquared);

			float maxDist = parent.SquaredRenderingDistance;
			if(parent.RenderingDistance > 0 && camDistanceSquared > maxDist && cam.UseRenderingDistance)
			{
				beyondFarDistance = true;
			}
			else
			{
				beyondFarDistance = false;

				currentLod = (ushort)(lodSquaredDistances.Count - 1);
				for(ushort i = 0; i < lodSquaredDistances.Count; ++i)
				{
					if((float)lodSquaredDistances[i] > camDistanceSquared)
					{
						currentLod = (ushort)(i - 1);
						break;
					}
				}
			}
		}

		public override void UpdateRenderQueue(RenderQueue queue)
		{
			LODBucket lodBucket = lodBucketList[currentLod];
			lodBucket.AddRenderables(queue, renderQueueID, camDistanceSquared);
		}

		public override bool IsVisible
		{
			get
			{
				return isVisible && !beyondFarDistance;
			}
		}

		public IEnumerator GetShadowVolumeRenderableIterator(ShadowTechnique shadowTechnique, Light light, HardwareIndexBuffer indexBuffer, 
                                                             bool extrudeVertices, float extrusionDistance, ulong flags) {
            Debug.Assert(indexBuffer != null, "Only external index buffers are supported right now");
            Debug.Assert(indexBuffer.Type == IndexType.Size16,
                "Only 16-bit indexes supported for now");

            // Calculate the object space light details
            Vector4 lightPos = light.GetAs4DVector();
            Matrix4 world2Obj = parentNode.FullTransform.Inverse();
            lightPos =  world2Obj * lightPos;

            // We need to search the edge list for silhouette edges
            if (edgeList == null) {
                throw new Exception("You enabled stencil shadows after the buid process!  In " +
                    "Region.GetShadowVolumeRenderableIterator");
            }

            // Init shadow renderable list if required
            bool init = shadowRenderables.Count == 0;

            RegionShadowRenderable esr = null;
            //bool updatedSharedGeomNormals = false;
            for (int i=0; i<edgeList.EdgeGroups.Count; i++) {
                EdgeData.EdgeGroup group = (EdgeData.EdgeGroup)edgeList.EdgeGroups[i];
                if (init) {
                    // Create a new renderable, create a separate light cap if
                    // we're using a vertex program (either for this model, or
                    // for extruding the shadow volume) since otherwise we can
                    // get depth-fighting on the light cap
                    esr = new RegionShadowRenderable(this, indexBuffer, group.vertexData, vertexProgramInUse || !extrudeVertices);
                    shadowRenderables.Add(esr);
                }
                else
                    esr = (RegionShadowRenderable)shadowRenderables[i];
                // Extrude vertices in software if required
                if (extrudeVertices)
                    ExtrudeVertices(esr.PositionBuffer, group.vertexData.vertexCount, lightPos, extrusionDistance);
            }
            return (IEnumerator)shadowRenderables;
        }
        
		public void Dump()
		{
			log.DebugFormat("--------------------------");
			log.DebugFormat("Region {0}", regionID);
			log.DebugFormat("Center: {0}", center);
			log.DebugFormat("Local AABB: {0}", aabb);
			log.DebugFormat("Bounding radius: {0}", boundingRadius);
			log.DebugFormat("Number of LODs: {0}", lodBucketList.Count);
			foreach(LODBucket lodBucket in lodBucketList)
				lodBucket.Dump();
		}
		#endregion

        /// <summary>
        ///     Remove the region from the scene graph
        /// </summary>
        public virtual void Dispose() {
            node.RemoveFromParent();
            sceneMgr.DestroySceneNode(node.Name);
			foreach(LODBucket lodBucket in lodBucketList)
                lodBucket.Dispose();
		}

	}

}
