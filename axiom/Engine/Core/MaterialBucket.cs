using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using log4net;

using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Collections;

namespace Axiom.Core
{
	
	public class MaterialBucket : IDisposable
	{
		#region Fields and Properties
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MaterialBucket));

		protected LODBucket parent;
		protected string materialName;
		protected Material material;
		protected Technique technique;

		protected List<GeometryBucket> geometryBucketList;
        // This only gives the most recent bucket for a given format.  
        // It needs to be changed to Dictionary<string, List<GeometryBucket>> currentGeometryMap
        // so we can search for buckets with an available slot in the face of invalidation.
		protected Dictionary<string, GeometryBucket> currentGeometryMap;

		public string MaterialName
		{
			get 
			{
				return materialName;
			}
		}

		public LODBucket Parent
		{
			get 
			{
				return parent;
			}
		}

		public Material Material
		{
			get 
			{
				return material;
			}
		}

		public Technique CurrentTechnique
		{
			get 
			{
				return technique;
			}
		}

		public List<GeometryBucket> GeometryBucketList
		{
			get 
			{
				return geometryBucketList;
			}
		}
		
		#endregion

		#region Constructors
			
		public MaterialBucket(LODBucket parent, string materialName)
		{
			this.parent = parent;
			this.materialName = materialName;
            geometryBucketList = new List<GeometryBucket>();
            currentGeometryMap = new Dictionary<string, GeometryBucket>();
		}

		#endregion

		#region Proteced Methods
		protected string GetGeometryFormatString(SubMeshLodGeometryLink geom)
		{
			// Formulate an identifying string for the geometry format
			// Must take into account the vertex declaration and the index type
			// Format is (all lines separated by '|'):
			// Index type
			// Vertex element (repeating)
			//   source
			//   semantic
			//   type
			string str = string.Format("{0}|", geom.indexData.indexBuffer.Type);

			for(int i = 0; i < geom.vertexData.vertexDeclaration.ElementCount; ++i)
			{
				VertexElement elem = geom.vertexData.vertexDeclaration.GetElement(i);
				str += string.Format("{0}|{0}|{1}|{2}|", elem.Source, elem.Semantic, elem.Type);
			}
			return str;
		}

		#endregion

		#region Public Methods
		public void Assign(QueuedGeometry qgeom)
		{
			// Look up any current geometry
			string formatString = GetGeometryFormatString(qgeom.geometry);
			bool newBucket = true;
			if(currentGeometryMap.ContainsKey(formatString))
			{
				GeometryBucket gbucket = currentGeometryMap[formatString];
				// Found existing geometry, try to assign
				newBucket = !gbucket.Assign(qgeom);
				// Note that this bucket will be replaced as the 'current'
				// for this format string below since it's out of space
			}
			// Do we need to create a new one?
			if(newBucket)
			{
				GeometryBucket gbucket = new GeometryBucket(this, formatString, qgeom.geometry.vertexData, qgeom.geometry.indexData);
				// Add to main list
				geometryBucketList.Add(gbucket);
				// Also index in 'current' list
				currentGeometryMap[formatString] = gbucket;
				if(!gbucket.Assign(qgeom)) {
					throw new AxiomException("Somehow we couldn't fit the requested geometry even in a " +
						"brand new GeometryBucket!! Must be a bug, please report.");
				}
			}
		}

		public int Build(bool stencilShadows, bool logDetails)
		{
            int bucketCount = 0;
            if (logDetails)
                log.InfoFormat("MaterialBucket.Build: Building material {0}", materialName);
			material = MaterialManager.Instance.GetByName(materialName);
			if (material == null) 
                log.ErrorFormat("MaterialBucket.Build: Could not find material {0}", materialName);
            else {
                material.Load();
                // tell the geometry buckets to build
                foreach (GeometryBucket gbucket in geometryBucketList) {
                    gbucket.Build(stencilShadows, logDetails);
                    bucketCount++;
                }
            }
            return bucketCount;
		}

		public void AddRenderables(RenderQueue queue, RenderQueueGroupID group, float camDistanceSquared)
		{
			if (material != null) {
                // determine the current material technique
                technique = material.GetBestTechnique(material.GetLodIndexSquaredDepth(camDistanceSquared));
                foreach (GeometryBucket gbucket in geometryBucketList) 
                    queue.AddRenderable(gbucket, RenderQueue.DEFAULT_PRIORITY, group);
            }
		}

		public void Dump()
		{
			log.DebugFormat("--------------------------------------------------");
			log.DebugFormat("Material Bucket {0}", materialName);
			log.DebugFormat("Geometry buckets: {0}", geometryBucketList.Count);
			foreach (GeometryBucket gbucket in geometryBucketList) 
				gbucket.Dump();
		}

        /// <summary>
        ///     Dispose the geometry buckets
        /// </summary>
        public virtual void Dispose() {
			foreach (GeometryBucket gbucket in geometryBucketList) 
                gbucket.Dispose();
		}

		#endregion
	}

}
