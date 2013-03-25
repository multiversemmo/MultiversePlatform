using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;

using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Collections;

namespace Axiom.Core
{
	
	public class LODBucket : IDisposable
	{
		#region Fields and Properties

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(LODBucket));

		protected Region parent;
		protected ushort lod;
		protected float squaredDistance;
		protected Dictionary<string, MaterialBucket> materialBucketMap;
		protected List<QueuedGeometry> queuedGeometryList;

		public Region Parent
		{
			get 
			{
				return parent;
			}
		}

		public ushort Lod
		{
			get 
			{
				return lod;
			}
		}

		public float SquaredDistance
		{
			get 
			{
				return squaredDistance;
			}
		}

		public Dictionary<string, MaterialBucket> MaterialBucketMap
		{
			get 
			{
				return materialBucketMap;
			}
		}

		#endregion

		#region Constructors

		public LODBucket(Region parent, ushort lod, float lodDist)
		{
			this.parent = parent;
			this.lod = lod;
			this.squaredDistance = lodDist;
            materialBucketMap = new Dictionary<string, MaterialBucket>();
            queuedGeometryList = new List<QueuedGeometry>();

		}
		#endregion

		#region Public Methods
			
		public void Assign(QueuedSubMesh qsm, ushort atlod)
		{
			QueuedGeometry q = new QueuedGeometry();
			queuedGeometryList.Add(q);
			q.position = qsm.position;
			q.orientation = qsm.orientation;
			q.scale = qsm.scale;
			if(qsm.geometryLodList.Count > atlod)
			{
				// This submesh has enough lods, use the right one
				q.geometry = qsm.geometryLodList[atlod];
			}
			else
			{
				// Not enough lods, use the lowest one we have
				q.geometry = qsm.geometryLodList[qsm.geometryLodList.Count - 1];
			}
			// Locate a material bucket
			MaterialBucket mbucket;
			if(materialBucketMap.ContainsKey(qsm.materialName))
			{
				mbucket = materialBucketMap[qsm.materialName];
			}
			else
			{
				mbucket = new MaterialBucket(this, qsm.materialName);
				materialBucketMap[qsm.materialName] = mbucket;
			}
			mbucket.Assign(q);
		}

		public int Build(bool stencilShadows, bool logDetails)
		{
			int bucketCount = 0;
            // Just pass this on to child buckets
			foreach(MaterialBucket mbucket in materialBucketMap.Values)
				bucketCount += mbucket.Build(stencilShadows, logDetails);
            return bucketCount;
		}

		public void AddRenderables(RenderQueue queue, RenderQueueGroupID group, float camSquaredDistance)
		{
			// Just pass this on to child buckets
			foreach(MaterialBucket mbucket in materialBucketMap.Values)
				mbucket.AddRenderables(queue, group, camSquaredDistance);
			}

		public void Dump()
		{
			log.DebugFormat("------------------");
			log.DebugFormat("LOD Bucket {0}", lod);
			log.DebugFormat("Distance: {0}", Math.Sqrt(squaredDistance));
			log.DebugFormat("Number of Materials: {0}", materialBucketMap.Count);
			foreach(MaterialBucket mbucket in materialBucketMap.Values)
				mbucket.Dump();
		}

        /// <summary>
        ///     Dispose the material buckets
        /// </summary>
        public virtual void Dispose() {
			foreach(MaterialBucket mbucket in materialBucketMap.Values)
                mbucket.Dispose();
		}


		#endregion
	}

}
