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

#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;

#endregion Using Directives

namespace Axiom.SceneManagers.Multiverse
{
	public enum  RaySceneQueryType : ulong
	{
		// Height will return the height at the origin
		// Distance will always be 0
		Height       = 1<<0,

		// AllTerrain will return all terrain contacts
		// along the ray
		AllTerrain   = 1<<1,

		// FirstTerrain will return only the first
		// contact along the ray
		FirstTerrain = 1<<2,

        // Entities will return all entity contacts along the ray
		Entities     = 1<<3,

		// Different resolution scales.  It defaults to 1 unit
		// resolution.  2x resolution tests every 0.5 units
		// 4x tests every 0.25 and 8x every 0.125
		EightxRes        = 1<<4,
		FourxRes        = 1<<5,
		TwoxRes        = 1<<6,
        OnexRes        = 1<<7,

		// FirstEntityTriangle will return only the first
		// contact with a triangle of an Entity along the ray.  Used
		// by the World Editor code for drawing interior
		// path boundaries.
		FirstEntityTriangle = 1<<8,
		
        // Return a list of _all_ entity triangles encountered by the
        // ray
        AllEntityTriangles = 1<<9,    
	};
	
    public class EntityAndIntersection {
        
        public EntityAndIntersection(Entity entity, Vector3 intersection) {
            this.entity = entity;
            this.intersection = intersection;
        }

        public Entity entity;
        public Vector3 intersection;
    }
    
    /// <summary>
	/// 	IPL's specialisation of RaySceneQuery.
	/// 	if RSQ_Height bit mask is set, RSQ_Terrain and RSQ_Entity bits will be ignored
	/// 	Otherwise data will be returned based on the mask
	/// </summary>
	public class RaySceneQuery : DefaultRaySceneQuery
	{
        protected float nearDist = -1f;
        protected List<SceneQuery.WorldFragment> fragmentList = new List<SceneQuery.WorldFragment>();

		/// <summary>
		///		Constructor
		/// </summary>
		/// <param name="creator">SceneManager that creates this query</param>
		public RaySceneQuery(SceneManager creator): base( creator)
		{
			this.AddWorldFragmentType( WorldFragmentType.SingleIntersection );
		}

        public float NearDistance
        {
            get
            {
                return nearDist;
            }
            set
            {
                nearDist = value;
            }
        }

		/// <summary>
		///		<see cref="RaySceneQuery"/>
		/// </summary>
		public override void Execute( IRaySceneQueryListener listener )
		{ 
			clearFragmentList( );

            // if the world is not initialized, then just quit out now with no hits
            if (!TerrainManager.Instance.Initialized)
            {
                return;
            }

			ulong mask = QueryMask;
			SceneQuery.WorldFragment frag;

            bool terrainQuery = (mask & (ulong)RaySceneQueryType.AllTerrain) != 0 || (mask & (ulong)RaySceneQueryType.FirstTerrain) != 0;

            // if the query is a terrain query that is exactly vertical, then force it into the "Height" fastpath
			if (( (mask & (ulong)RaySceneQueryType.Height) != 0) || ( terrainQuery && ( this.Ray.Direction.x == 0.0f ) && ( this.Ray.Direction.z == 0.0f)))
			{
				// we don't want to bother checking for entities because a 
				// UNIT_Y ray is assumed to be a height test, not a ray test
				frag = new SceneQuery.WorldFragment( );
				fragmentList.Add( frag );

				frag.FragmentType = WorldFragmentType.SingleIntersection; 
				Vector3 origin = this.Ray.Origin;
				origin.y = 0; // ensure that it's within bounds
				frag.SingleIntersection = getHeightAt(origin);
				listener.OnQueryResult( frag, Math.Abs(frag.SingleIntersection.y - this.Ray.Origin.y) );
			}
			else
			{
				// Check for all entity contacts
				if ( (mask & (ulong)RaySceneQueryType.Entities) != 0 )
				{
					base.Execute( listener );
				}

				// Check for contact with the closest entity triangle
				// or all entity triangles.  Ignores entities that
				// don't have a TriangleIntersector associated with
				// their meshes.
				bool firstTriangleQuery = (mask & (ulong)(RaySceneQueryType.FirstEntityTriangle)) != 0;
                bool allTrianglesQuery = (mask & (ulong)(RaySceneQueryType.AllEntityTriangles)) != 0;
                if (firstTriangleQuery | allTrianglesQuery)
				{
					rayOrigin = this.ray.Origin;
                    // Start by getting the entities whose bounding
					// boxes intersect the ray.  If there are none,
					// we're done.
                    List<MovableObject> candidates = new List<MovableObject>();
                    foreach (Dictionary<string, MovableObject> objectMap in creator.MovableObjectMaps) {
                        foreach (MovableObject obj in objectMap.Values) {
                            // skip if unattached or filtered out by query flags
                            if (!obj.IsAttached || (obj.QueryFlags & queryMask) == 0)
                                continue;

                            // test the intersection against the world bounding box of the entity
                            IntersectResult results = MathUtil.Intersects(ray, obj.GetWorldBoundingBox());
                            if (results.Hit)
                                candidates.Add(obj);
                        }
                    }
					
					// Get the camera.Near value
                    float nearDistance;
                    if (nearDist < 0)
                    {
                        Camera cam = Axiom.SceneManagers.Multiverse.TerrainManager.Instance.SceneManager.GetCamera("PlayerCam");
                        nearDistance = cam.Near;
                    }
                    else
                    {
                        nearDistance = nearDist;
                    }
					float closestDistance = float.MaxValue;
					Vector3 closestIntersection = Vector3.Zero;
                    Entity closestEntity = null;
                    List<EntityAndIntersection> allEntities = new List<EntityAndIntersection>();
                    foreach (MovableObject obj in candidates) {
						// skip if unattached or filtered out by query flags
                        if (!obj.IsAttached || (obj.QueryFlags & queryMask) == 0)
							continue;

                        Entity entity = obj as Entity;
                        if (entity == null)
                            continue;

						// skip if its mesh doesn't have triangles
						if (entity.Mesh == null || entity.Mesh.TriangleIntersector == null)
							continue;
						
						// transform the ray to the space of the mesh
						Matrix4 inverseTransform = entity.ParentNodeFullTransform.Inverse();
						Matrix4 inverseWithoutTranslation = inverseTransform;
						inverseWithoutTranslation.Translation = Vector3.Zero;
                        Vector3 transformedOrigin = inverseTransform * ray.Origin;
						Ray transformedRay = new Ray(transformedOrigin,
													 (inverseWithoutTranslation * ray.Direction).ToNormalized());
						
						// test the intersection against the world bounding box of the entity
						Vector3 untransformedIntersection;
						if (entity.Mesh.TriangleIntersector.ClosestRayIntersection(transformedRay, Vector3.Zero, 
																			       nearDistance, out untransformedIntersection))
						{
							Vector3 intersection = entity.ParentNodeFullTransform * untransformedIntersection;
                            if (allTrianglesQuery)
                                allEntities.Add(new EntityAndIntersection(entity, intersection));
                            float distance = (ray.Origin - intersection).Length;
							if (firstTriangleQuery && distance < closestDistance)
							{
								closestDistance = distance;
								closestEntity = entity;
								closestIntersection = intersection;
							}
						}
					}
                    if (firstTriangleQuery && closestEntity != null)
					{
						frag = new SceneQuery.WorldFragment( );
						fragmentList.Add( frag );
						frag.FragmentType = WorldFragmentType.SingleIntersection; 
						frag.SingleIntersection = closestIntersection;
						listener.OnQueryResult(frag, closestDistance);
					}
                    else if (allTrianglesQuery && allEntities.Count > 0)
                    {
                        allEntities.Sort(distanceToCameraCompare);
                        foreach (EntityAndIntersection ei in allEntities)
                            listener.OnQueryResult(ei.entity, (rayOrigin - ei.intersection).Length);
                    }
				}

				if ( terrainQuery )
				{
					Vector3 ray = Ray.Origin;
					Vector3 land = getHeightAt( ray );
					float dist = 0, resFactor = TerrainManager.oneMeter;

                    // find the larger of x and z directions of the ray direction
                    float maxXZ = Math.Max(Math.Abs(Ray.Direction.x), Math.Abs(Ray.Direction.z));
                    
					// Only bother if the non-default mask has been set
					if ( ( mask & (ulong)RaySceneQueryType.OnexRes ) != 0 )
					{
						if ( (mask & (ulong)RaySceneQueryType.TwoxRes) != 0 )
						{
							resFactor = TerrainManager.oneMeter / 2;
						}
						else if ( (mask & (ulong)RaySceneQueryType.FourxRes) !=0 )
						{
							resFactor = TerrainManager.oneMeter / 4;
						}
						else if ( (mask & (ulong)RaySceneQueryType.EightxRes) != 0 )
						{
							resFactor = TerrainManager.oneMeter / 8;
						}
					}

                    // this scales the res factor so that we move along the ray by a distance that results
                    // in shift of one meter along either the X or Z axis (whichever is longer)
                    resFactor = resFactor / maxXZ;

					SubPageHeightMap sp;
					// bool east = false; (unused)
					// bool south = false; (unused)

					// if ( Ray.Origin.x > 0 ) 
					// {
					//	east = true;
					// }
					// if ( Ray.Origin.z > 0 ) 
					// {
					//	south = true;
					// }

					ray = Ray.Origin;
					sp = TerrainManager.Instance.LookupSubPage(ray);

					while ( sp != null ) 
					{
						SubPageHeightMap  newsp;
						AxisAlignedBox tileBounds = sp.BoundingBox;

						IntersectResult intersect = MathUtil.Intersects(Ray, tileBounds);

                        if (intersect.Hit)
                        {
                            // step through this tile
                            while ((newsp = TerrainManager.Instance.LookupSubPage(RoundRay(ray))) == sp)
                            {
                                land = getHeightAt(ray);
                                if (ray.y < land.y)
                                {
                                    frag = new SceneQuery.WorldFragment();
                                    fragmentList.Add(frag);

                                    frag.FragmentType = WorldFragmentType.SingleIntersection;
                                    frag.SingleIntersection = land;
                                    listener.OnQueryResult(frag, dist);

                                    if ((mask & (ulong)RaySceneQueryType.FirstTerrain) != 0)
                                    {
                                        return;
                                    }
                                }

                                ray += Ray.Direction * resFactor;
                                dist += 1 * resFactor;
                            }
                            // if we fall off the end of the above loop without getting a hit, then the hit should be
                            // right at the far edge of the tile, so handle that case.
                            land = getHeightAt(ray);
                            if (ray.y < land.y)
                            {
                                frag = new SceneQuery.WorldFragment();
                                fragmentList.Add(frag);

                                frag.FragmentType = WorldFragmentType.SingleIntersection;
                                frag.SingleIntersection = land;
                                listener.OnQueryResult(frag, dist);

                                //LogManager.Instance.Write("MVSM:RaySceneQuery:End of tile ray collision");

                                if ((mask & (ulong)RaySceneQueryType.FirstTerrain) != 0)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                //LogManager.Instance.Write("MVSM:RaySceneQuery:End of tile reached without expected intersection");
                            }
                        }
                        else
                        {
                            // step over this tile
                            while ((newsp = TerrainManager.Instance.LookupSubPage(RoundRay(ray))) == sp)
                            { // XXX - this is not the most efficient method...
                                ray += Ray.Direction * resFactor;
                                dist += 1 * resFactor;
                            }
                        }

						sp = newsp;
					}
				}	
			}
		}

        /// <summary>
        ///		<see cref="RaySceneQuery"/>
        /// </summary>
        public void Execute(IRaySceneQueryListener listener, Camera camera)
        {
            clearFragmentList();

            // if the world is not initialized, then just quit out now with no hits
            if (!TerrainManager.Instance.Initialized)
            {
                return;
            }

            ulong mask = QueryMask;
            SceneQuery.WorldFragment frag;

            bool terrainQuery = (mask & (ulong)RaySceneQueryType.AllTerrain) != 0 || (mask & (ulong)RaySceneQueryType.FirstTerrain) != 0;

            // if the query is a terrain query that is exactly vertical, then force it into the "Height" fastpath
            if (((mask & (ulong)RaySceneQueryType.Height) != 0) || (terrainQuery && (this.Ray.Direction.x == 0.0f) && (this.Ray.Direction.z == 0.0f)))
            {
                // we don't want to bother checking for entities because a 
                // UNIT_Y ray is assumed to be a height test, not a ray test
                frag = new SceneQuery.WorldFragment();
                fragmentList.Add(frag);

                frag.FragmentType = WorldFragmentType.SingleIntersection;
                Vector3 origin = this.Ray.Origin;
                origin.y = 0; // ensure that it's within bounds
                frag.SingleIntersection = getHeightAt(origin);
                listener.OnQueryResult(frag, Math.Abs(frag.SingleIntersection.y - this.Ray.Origin.y));
            }
            else
            {
                // Check for all entity contacts
                if ((mask & (ulong)RaySceneQueryType.Entities) != 0)
                {
                    base.Execute(listener);
                }

                // Check for contact with the closest entity triangle
                // or all entity triangles.  Ignores entities that
                // don't have a TriangleIntersector associated with
                // their meshes.
                bool firstTriangleQuery = (mask & (ulong)(RaySceneQueryType.FirstEntityTriangle)) != 0;
                bool allTrianglesQuery = (mask & (ulong)(RaySceneQueryType.AllEntityTriangles)) != 0;
                if (firstTriangleQuery | allTrianglesQuery)
                {
                    rayOrigin = this.ray.Origin;
                    // Start by getting the entities whose bounding
                    // boxes intersect the ray.  If there are none,
                    // we're done.
                    List<MovableObject> candidates = new List<MovableObject>();
                    foreach (Dictionary<string, MovableObject> objectMap in creator.MovableObjectMaps)
                    {
                        foreach (MovableObject obj in objectMap.Values)
                        {
                            // skip if unattached or filtered out by query flags
                            if (!obj.IsAttached || (obj.QueryFlags & queryMask) == 0)
                                continue;

                            // test the intersection against the world bounding box of the entity
                            IntersectResult results = MathUtil.Intersects(ray, obj.GetWorldBoundingBox());
                            if (results.Hit)
                                candidates.Add(obj);
                        }
                    }

                    // Get the camera.Near value
                    Camera cam = camera;
                    float nearDistance = cam.Near;
                    float closestDistance = float.MaxValue;
                    Vector3 closestIntersection = Vector3.Zero;
                    Entity closestEntity = null;
                    List<EntityAndIntersection> allEntities = new List<EntityAndIntersection>();
                    foreach (MovableObject obj in candidates)
                    {
                        // skip if unattached or filtered out by query flags
                        if (!obj.IsAttached || (obj.QueryFlags & queryMask) == 0)
                            continue;

                        Entity entity = obj as Entity;
                        if (entity == null)
                            continue;

                        // skip if its mesh doesn't have triangles
                        if (entity.Mesh == null || entity.Mesh.TriangleIntersector == null)
                            continue;

                        // transform the ray to the space of the mesh
                        Matrix4 inverseTransform = entity.ParentNodeFullTransform.Inverse();
                        Matrix4 inverseWithoutTranslation = inverseTransform;
                        inverseWithoutTranslation.Translation = Vector3.Zero;
                        Vector3 transformedOrigin = inverseTransform * ray.Origin;
                        Ray transformedRay = new Ray(transformedOrigin,
                                                     (inverseWithoutTranslation * ray.Direction).ToNormalized());

                        // test the intersection against the world bounding box of the entity
                        Vector3 untransformedIntersection;
                        if (entity.Mesh.TriangleIntersector.ClosestRayIntersection(transformedRay, Vector3.Zero,
                                                                                   nearDistance, out untransformedIntersection))
                        {
                            Vector3 intersection = entity.ParentNodeFullTransform * untransformedIntersection;
                            if (allTrianglesQuery)
                                allEntities.Add(new EntityAndIntersection(entity, intersection));
                            float distance = (ray.Origin - intersection).Length;
                            if (firstTriangleQuery && distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestEntity = entity;
                                closestIntersection = intersection;
                            }
                        }
                    }
                    if (firstTriangleQuery && closestEntity != null)
                    {
                        frag = new SceneQuery.WorldFragment();
                        fragmentList.Add(frag);
                        frag.FragmentType = WorldFragmentType.SingleIntersection;
                        frag.SingleIntersection = closestIntersection;
                        listener.OnQueryResult(frag, closestDistance);
                    }
                    else if (allTrianglesQuery && allEntities.Count > 0)
                    {
                        allEntities.Sort(distanceToCameraCompare);
                        foreach (EntityAndIntersection ei in allEntities)
                            listener.OnQueryResult(ei.entity, (rayOrigin - ei.intersection).Length);
                    }
                }

                if (terrainQuery)
                {
                    Vector3 ray = Ray.Origin;
                    Vector3 land = getHeightAt(ray);
                    float dist = 0, resFactor = TerrainManager.oneMeter;

                    // find the larger of x and z directions of the ray direction
                    float maxXZ = Math.Max(Math.Abs(Ray.Direction.x), Math.Abs(Ray.Direction.z));

                    // Only bother if the non-default mask has been set
                    if ((mask & (ulong)RaySceneQueryType.OnexRes) != 0)
                    {
                        if ((mask & (ulong)RaySceneQueryType.TwoxRes) != 0)
                        {
                            resFactor = TerrainManager.oneMeter / 2;
                        }
                        else if ((mask & (ulong)RaySceneQueryType.FourxRes) != 0)
                        {
                            resFactor = TerrainManager.oneMeter / 4;
                        }
                        else if ((mask & (ulong)RaySceneQueryType.EightxRes) != 0)
                        {
                            resFactor = TerrainManager.oneMeter / 8;
                        }
                    }

                    // this scales the res factor so that we move along the ray by a distance that results
                    // in shift of one meter along either the X or Z axis (whichever is longer)
                    resFactor = resFactor / maxXZ;

                    SubPageHeightMap sp;
                    // bool east = false; (unused)
                    // bool south = false; (unused)

                    // if ( Ray.Origin.x > 0 ) 
                    // {
                    //	east = true;
                    // }
                    // if ( Ray.Origin.z > 0 ) 
                    // {
                    //	south = true;
                    // }

                    ray = Ray.Origin;
                    sp = TerrainManager.Instance.LookupSubPage(ray);

                    while (sp != null)
                    {
                        SubPageHeightMap newsp;
                        AxisAlignedBox tileBounds = sp.BoundingBox;

                        IntersectResult intersect = MathUtil.Intersects(Ray, tileBounds);

                        if (intersect.Hit)
                        {
                            // step through this tile
                            while ((newsp = TerrainManager.Instance.LookupSubPage(RoundRay(ray))) == sp)
                            {
                                land = getHeightAt(ray);
                                if (ray.y < land.y)
                                {
                                    frag = new SceneQuery.WorldFragment();
                                    fragmentList.Add(frag);

                                    frag.FragmentType = WorldFragmentType.SingleIntersection;
                                    frag.SingleIntersection = land;
                                    listener.OnQueryResult(frag, dist);

                                    if ((mask & (ulong)RaySceneQueryType.FirstTerrain) != 0)
                                    {
                                        return;
                                    }
                                }

                                ray += Ray.Direction * resFactor;
                                dist += 1 * resFactor;
                            }
                            // if we fall off the end of the above loop without getting a hit, then the hit should be
                            // right at the far edge of the tile, so handle that case.
                            land = getHeightAt(ray);
                            if (ray.y < land.y)
                            {
                                frag = new SceneQuery.WorldFragment();
                                fragmentList.Add(frag);

                                frag.FragmentType = WorldFragmentType.SingleIntersection;
                                frag.SingleIntersection = land;
                                listener.OnQueryResult(frag, dist);

                                //LogManager.Instance.Write("MVSM:RaySceneQuery:End of tile ray collision");

                                if ((mask & (ulong)RaySceneQueryType.FirstTerrain) != 0)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                //LogManager.Instance.Write("MVSM:RaySceneQuery:End of tile reached without expected intersection");
                            }
                        }
                        else
                        {
                            // step over this tile
                            while ((newsp = TerrainManager.Instance.LookupSubPage(RoundRay(ray))) == sp)
                            { // XXX - this is not the most efficient method...
                                ray += Ray.Direction * resFactor;
                                dist += 1 * resFactor;
                            }
                        }

                        sp = newsp;
                    }
                }
            }
        }


        private Vector3 RoundRay(Vector3 v)
        {
            return new Vector3((float)Math.Round(v.x), (float)Math.Round(v.y), (float)Math.Round(v.z));
        }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		/// <returns></returns>
		protected Vector3 getHeightAt( Vector3 origin )
		{
			return new Vector3( origin.x, TerrainManager.Instance.GetTerrainHeight(origin, GetHeightMode.Interpolate, GetHeightLOD.MaxLOD), origin.z );
		}

		/// <summary>
		///		Removes Cached fragments from last query
		/// </summary>
		protected void clearFragmentList( )
		{
			fragmentList.Clear();
		}

        private static Vector3 rayOrigin = Vector3.Zero;

        private static int distanceToCameraCompare(EntityAndIntersection ei1, EntityAndIntersection ei2)
        {
           float distanceDiff = (rayOrigin - ei1.intersection).Length - (rayOrigin - ei2.intersection).Length;
           if (distanceDiff < 0)
               return -1;
           else if (distanceDiff > 0)
               return 1;
           else
               return 0;
        }
	}
 
}
