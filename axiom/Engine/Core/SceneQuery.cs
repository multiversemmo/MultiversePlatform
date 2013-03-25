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
using Axiom.Collections;
using Axiom.Graphics;
using Axiom.MathLib;

namespace Axiom.Core {
	#region Base Query Implementation
	/// <summary>
	/// 	A class for performing queries on a scene.
	/// </summary>
	/// <remarks>
	/// 	This is an abstract class for performing a query on a scene, i.e. to retrieve
	/// 	a list of objects and/or world geometry sections which are potentially intersecting a
	/// 	given region. Note the use of the word 'potentially': the results of a scene query
	/// 	are generated based on bounding volumes, and as such are not correct at a triangle
	/// 	level; the user of the SceneQuery is expected to filter the results further if
	/// 	greater accuracy is required.
	/// 	<p/>
	/// 	Different SceneManagers will implement these queries in different ways to
	/// 	exploit their particular scene organization, and thus will provide their own
	/// 	concrete subclasses. In fact, these subclasses will be derived from subclasses
	/// 	of this class rather than directly because there will be region-type classes
	/// 	in between.
	/// 	<p/>
	/// 	These queries could have just been implemented as methods on the SceneManager,
	/// 	however, they are wrapped up as objects to allow 'compilation' of queries
	/// 	if deemed appropriate by the implementation; i.e. each concrete subclass may
	/// 	precalculate information (such as fixed scene partitions involved in the query)
	/// 	to speed up the repeated use of the query.
	/// 	<p/>
	/// 	You should never try to create a SceneQuery object yourself, they should be created
	/// 	using the SceneManager interfaces for the type of query required, e.g.
	/// 	SceneManager.CreateRaySceneQuery.
	/// </remarks>
	public abstract class SceneQuery {
		#region Fields

		/// <summary>
		///		Reference to the SceneManager that this query was created by.
		/// </summary>
		protected SceneManager creator;
		/// <summary>
		///		User definable query bit mask which can be used to filter the results of a query.
		/// </summary>
		protected ulong queryMask;
		/// <summary>
		///		A flag enum which holds the world fragment types supported by this query.
		/// </summary>
		protected WorldFragmentType worldFragmentTypes;

		#endregion Fields
		
		#region Constructors
		
		/// <summary>
		///		Internal constructor.
		/// </summary>
		/// <param name="creator">Reference to the scene manager who created this query.</param>
		internal SceneQuery(SceneManager creator) {
			this.creator = creator;

			// default to no world fragments queried
			AddWorldFragmentType(WorldFragmentType.None);
		}
		
		#endregion Constructor
		
		#region Methods

		/// <summary>
		///		Used to add a supported world fragment type to this query.
		/// </summary>
		public void AddWorldFragmentType(WorldFragmentType fragmentType) {
			worldFragmentTypes |= fragmentType;
		}
		
		#endregion Methods
		
		#region Properties
		
		/// <summary>
		///    Sets the mask for results of this query.
		/// </summary>
		/// <remarks>
		///    This property allows you to set a 'mask' to limit the results of this
		///    query to certain types of result. The actual meaning of this value is
		///    up to the application; basically SceneObject instances will only be returned
		///    from this query if a bitwise AND operation between this mask value and the
		///    SceneObject.QueryFlags value is non-zero. The application will
		///    have to decide what each of the bits means.
		/// </remarks>
		public ulong QueryMask {
			get {
				return queryMask;
			}
			set {
				queryMask = value;
			}
		}

		#endregion

		#region Nested Structs

		/// <summary>
		///		Represents part of the world geometry that is a result of a <see cref="SceneQuery"/>.
		/// </summary>
		/// <remarks>
		///		Since world geometry is normally vast and sprawling, we need a way of
		///		retrieving parts of it based on a query. That is what this struct is for;
		///		note there are potentially as many data structures for world geometry as there
		///		are SceneManagers, however this structure includes a few common abstractions as 
		///		well as a more general format.
		///		<p/>
		///		The type of world fragment that is returned from a query depends on the
		///		SceneManager, and the fragment types are supported on the query.
		/// </remarks>
		public class WorldFragment {
			/// <summary>
			///		The type of this world fragment.
			/// </summary>
			public WorldFragmentType FragmentType;
			/// <summary>
			///		Single intersection point, only applicable for <see cref="WorldFragmentType.SingleIntersection"/>.
			/// </summary>
			public Vector3 SingleIntersection;
			/// <summary>
			///		Planes bounding a convex region, only applicable for <see cref="WorldFragmentType.PlaneBoundedRegion"/>.
			/// </summary>
			public List<Plane> Planes;
			/// <summary>
			///		General render operation structure.  Fallback if nothing else is available.
			/// </summary>
			public RenderOperation RenderOp;
		}

		#endregion Nested Structs
	}

	/// <summary>
	///		Abstract class defining a query which returns single results from within a region.
	/// </summary>
	/// <remarks>
	///		This class is simply a generalization of the subtypes of query that return 
	///		a set of individual results in a region. See the <see cref="SceneQuery"/> class for 
	///		abstract information, and subclasses for the detail of each query type.
	/// </remarks>
	public abstract class RegionSceneQuery : SceneQuery, ISceneQueryListener  {
		#region Fields

		/// <summary>
		///		List of results from the last non-listener query.
		/// </summary>
		protected SceneQueryResult lastResult = new SceneQueryResult();

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Default constructor.
		/// </summary>
		/// <param name="creator">SceneManager who created this query.</param>
		internal RegionSceneQuery(SceneManager creator) : base(creator) {}

		#endregion Constructor

		#region Methods

		/// <summary>
		///		Clears out any cached results from the last query.
		/// </summary>
		public virtual void ClearResults() {
			lastResult.objects.Clear();
			lastResult.worldFragments.Clear();
		}

		/// <summary>
		///		Executes the query, returning the results back in one list.
		/// </summary>
		/// <remarks>
		///		This method executes the scene query as configured, gathers the results
		///		into one structure and returns a reference to that structure. These
		///		results will also persist in this query object until the next query is
		///		executed, or <see cref="ClearResults"/> is called. An more lightweight version of
		///		this method that returns results through a listener is also available.
		/// </remarks>
		/// <returns></returns>
		public virtual SceneQueryResult Execute() {
			ClearResults();

			// invoke callback method with ourself as the listener
			Execute(this);

			return lastResult;
		}

		/// <summary>
		///		Executes the query and returns each match through a listener interface.
		/// </summary>
		/// <remarks>
		///		Note that this method does not store the results of the query internally 
		///		so does not update the 'last result' value. This means that this version of
		///		execute is more lightweight and therefore more efficient than the version 
		///		which returns the results as a collection.
		/// </remarks>
		/// <param name="listener"></param>
		public abstract void Execute(ISceneQueryListener listener);

		#endregion Methods

		#region ISceneQueryListener Members

		/// <summary>
		///		Self-callback in order to deal with execute which returns collection.
		/// </summary>
		/// <param name="sceneObject"></param>
		/// <returns></returns>
		public bool OnQueryResult(MovableObject sceneObject) {
			lastResult.objects.Add(sceneObject);

			// continue
			return true;
		}

		/// <summary>
		///		Self-callback in order to deal with execute which returns collection.
		/// </summary>
		/// <param name="fragment"></param>
		/// <returns></returns>
		public bool OnQueryResult(Axiom.Core.SceneQuery.WorldFragment fragment) {
			lastResult.worldFragments.Add(fragment);

			// continue
			return true;
		}

		#endregion
	}

	/// <summary>
	///		Holds the results of a single scene query.
	/// </summary>
	public class SceneQueryResult {
		/// <summary>
		///		List of scene objects in the query (entities, particle systems etc).
		/// </summary>
		public MovableObjectCollection objects = new MovableObjectCollection();
		/// <summary>
		///		List of world fragments.
		/// </summary>
		public ArrayList worldFragments = new ArrayList();
	}

	/// <summary>
	///		This optional class allows you to receive per-result callbacks from
	///		SceneQuery executions instead of a single set of consolidated results.
	/// </summary>
	public interface ISceneQueryListener {
		/// <summary>
		///		Called when a <see cref="SceneObject"/> is returned by a query.
		/// </summary>
		/// <remarks>
		///		The implementor should return 'true' to continue returning objects,
		///		or 'false' to abandon any further results from this query.
		/// </remarks>
		/// <param name="sceneObject">Object found by the query.</param>
		/// <returns></returns>
		bool OnQueryResult(MovableObject sceneObject);

		/// <summary>
		///		Called when a <see cref="SceneQuery.WorldFragment"/> is returned by a query.
		/// </summary>
		/// <param name="fragment">Fragment found by the query.</param>
		/// <returns></returns>
		bool OnQueryResult(SceneQuery.WorldFragment fragment);
	}

	#endregion Base Query Implementation

	#region RaySceneQuery Implementation

	/// <summary>
	///		Specializes the SceneQuery class for querying for objects along a ray.
	/// </summary>
	public abstract class RaySceneQuery : SceneQuery, IRaySceneQueryListener {
		#region Fields

		/// <summary>
		///		Reference to a ray to use for this query.
		/// </summary>
		protected Ray ray;
		/// <summary>
		///		If true, results returned in the list 
		/// </summary>
		protected bool sortByDistance;
		/// <summary>
		///		Maximum results to return when executing the query.
		/// </summary>
		protected int maxResults;
		/// <summary>
		///		List of query results from the last execution of this query.
		/// </summary>
        protected List<RaySceneQueryResultEntry> lastResults = new List<RaySceneQueryResultEntry>();

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="creator">Scene manager who created this query.</param>
		internal RaySceneQuery(SceneManager creator) : base(creator) {}

		#endregion Constructor

		#region Properties

		/// <summary>
		///    Gets/Sets the Ray being used for this query.
		/// </summary>
		public Ray Ray {
			get {
				return ray;
			}
			set {
				ray = value;
			}
		}

		/// <summary>
		///		Gets/Sets whether this queries results are sorted by distance.
		/// </summary>
		/// <remarks>
		///		Often you want to know what was the first object a ray intersected with, and this 
		///		method allows you to ask the query to sort the results so that the nearest results
		///		are listed first.
		///		<p/>
		///		Note that because the query returns results based on bounding volumes, the ray may not
		///		actually intersect the detail of the objects returned from the query, just their 
		///		bounding volumes. For this reason the caller is advised to use more detailed 
		///		intersection tests on the results if a more accurate result is required; we use 
		///		bounds checking in order to give the most speedy results since not all applications 
		///		need extreme accuracy.
		/// </remarks>
		public bool SortByDistance {
			get {
				return sortByDistance;
			}
			set { 
				sortByDistance = value;
			}
		}

		/// <summary>
		///		Gets/Sets the maximum number of results to return from this query when 
		///		sorting is enabled.
		/// </summary>
		/// <remarks>
		///		If sorting by distance is not enabled, then this value has no affect.
		/// </remarks>
		public int MaxResults {
			get {
				return maxResults;
			}
			set {
				maxResults = value;

				// size the arraylist to hold the maximum results
				lastResults.Capacity = maxResults;
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>
		///		Clears out any cached results from the last query.
		/// </summary>
		public virtual void ClearResults() {
			lastResults.Clear();
		}

		/// <summary>
		///		Executes the query, returning the results back in one list.
		/// </summary>
		/// <remarks>
		///		This method executes the scene query as configured, gathers the results
		///		into one structure and returns a reference to that structure. These
		///		results will also persist in this query object until the next query is
		///		executed, or <see cref="ClearResults"/>. A more lightweight version of
		///		this method that returns results through a listener is also available.
		/// </remarks>
		/// <returns></returns>
        public virtual List<RaySceneQueryResultEntry> Execute() {
			ClearResults();

			// execute the callback version using ourselves as the listener
			Execute(this);

			if(sortByDistance) {
				lastResults.Sort();

				if(maxResults != 0 && lastResults.Count > maxResults) {
					// remove the results greater than the desired amount
					lastResults.RemoveRange(maxResults, lastResults.Count - maxResults);
				}
			}

			return lastResults;
		}

		/// <summary>
		///		Executes the query and returns each match through a listener interface.
		/// </summary>
		/// <remarks>
		///		Note that this method does not store the results of the query internally 
		///		so does not update the 'last result' value. This means that this version of
		///		execute is more lightweight and therefore more efficient than the version 
		///		which returns the results as a collection.
		/// </remarks>
		/// <param name="listener">Listener object to handle the result callbacks.</param>
		public abstract void Execute(IRaySceneQueryListener listener);

		#endregion Methods

		#region IRaySceneQueryListener Members

		public bool OnQueryResult(MovableObject sceneObject, float distance) {
			// create an entry and add it to the cached result list
			RaySceneQueryResultEntry entry = new RaySceneQueryResultEntry();
			entry.Distance = distance;
			entry.SceneObject = sceneObject;
			entry.worldFragment = null;
			lastResults.Add(entry);

			// continue gathering results
			return true;
		}

		bool Axiom.Core.IRaySceneQueryListener.OnQueryResult(SceneQuery.WorldFragment fragment, float distance) {
			// create an entry and add it to the cached result list
			RaySceneQueryResultEntry entry = new RaySceneQueryResultEntry();
			entry.Distance = distance;
			entry.SceneObject = null;
			entry.worldFragment = fragment;
			lastResults.Add(entry);

			// continue gathering results
			return true;
		}

		#endregion
	}

	/// <summary>
	///		Alternative listener interface for dealing with <see cref="RaySceneQuery"/>.
	/// </summary>
	public interface IRaySceneQueryListener {
		/// <summary>
		///		Called when a scene objects intersect the ray.
		/// </summary>
		/// <param name="sceneObject">Reference to the object hit by the ray.</param>
		/// <param name="distance">Distance from the origin of the ray where the intersection took place.</param>
		/// <returns>Should return false to abandon returning additional results, or true to continue.</returns>
		bool OnQueryResult(MovableObject sceneObject, float distance);

		/// <summary>
		///		Called when a world fragment is intersected by the ray.
		/// </summary>
		/// <param name="fragment">World fragment hit by the ray.</param>
		/// <param name="distance">Distance from the origin of the ray where the intersection took place.</param>
		/// <returns>Should return false to abandon returning additional results, or true to continue.</returns>
		bool OnQueryResult(SceneQuery.WorldFragment fragment, float distance);
	}

	/// <summary>
	///		This struct allows a single comparison of result data no matter what the type.
	/// </summary>
	public class RaySceneQueryResultEntry : IComparable {
		/// <summary>
		///		Distance along the ray.
		/// </summary>
		public float Distance;
		/// <summary>
		///		The object, or null if this is not a scene object result.
		/// </summary>
		public MovableObject SceneObject;
		/// <summary>
		///		The world fragment, or null if this is not a fragment result.
		/// </summary>
		public SceneQuery.WorldFragment worldFragment;

		#region IComparable Members

		/// <summary>
		///		Implemented to allow sorting of results based on distance.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int CompareTo(object obj) {
			RaySceneQueryResultEntry entry = obj as RaySceneQueryResultEntry;

			if(Distance < entry.Distance) {
				// this result is less than
				return -1;
			}
			else if(Distance > entry.Distance) {
				// this result is greater than
				return 1;
			}

			// they are equal
			return 0;
		}

		#endregion
	}

	#endregion RaySceneQuery Implementation

	#region AxisAlignedBoxRegionSceneQuery Implementation

	/// <summary>
	///		Specializes the SceneQuery class for querying items within an AxisAlignedBox.
	/// </summary>
	public abstract class AxisAlignedBoxRegionSceneQuery : RegionSceneQuery {
		#region Fields

		/// <summary>
		///		Sphere to query items within.
		/// </summary>
		protected AxisAlignedBox box;

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Default constructor.
		/// </summary>
		/// <param name="creator">SceneManager who created this query.</param>
		internal AxisAlignedBoxRegionSceneQuery(SceneManager creator) : base(creator) {}

		#endregion Constructor

		#region Properties

		/// <summary>
		///		Gets/Sets the sphere to use for the query.
		/// </summary>
		public AxisAlignedBox Box {
			get {
				return box;
			}
			set {
				box = value;
			}
		}

		#endregion Properties
	}

	#endregion AxisAlignedBoxRegionSceneQuery Implementation

	#region SphereRegionSceneQuery Implementation

	/// <summary>
	///		Specializes the SceneQuery class for querying items within a sphere.
	/// </summary>
	public abstract class SphereRegionSceneQuery : RegionSceneQuery {
		#region Fields

		/// <summary>
		///		Sphere to query items within.
		/// </summary>
		protected Sphere sphere;

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Default constructor.
		/// </summary>
		/// <param name="creator">SceneManager who created this query.</param>
		internal SphereRegionSceneQuery(SceneManager creator) : base(creator) {}

		#endregion Constructor

		#region Properties

		/// <summary>
		///		Gets/Sets the sphere to use for the query.
		/// </summary>
		public Sphere Sphere {
			get {
				return sphere;
			}
			set {
				sphere = value;
			}
		}

		#endregion Properties
	}

	#endregion SphereRegionSceneQuery Implementation

	#region PlaneBoundedVolumeListSceneQuery Implementation

	/// <summary>
	///		Specializes the SceneQuery class for querying items within PlaneBoundedVolumes.
	/// </summary>
	public abstract class PlaneBoundedVolumeListSceneQuery : RegionSceneQuery {
		#region Fields

		/// <summary>
		///		Sphere to query items within.
		/// </summary>
		protected List<PlaneBoundedVolume> volumes;

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Default constructor.
		/// </summary>
		/// <param name="creator">SceneManager who created this query.</param>
		internal PlaneBoundedVolumeListSceneQuery(SceneManager creator) : base(creator) {}

		#endregion Constructor

		#region Properties

		/// <summary>
		///		Gets/Sets the sphere to use for the query.
		/// </summary>
		public List<PlaneBoundedVolume> Volumes {
			get {
				return volumes;
			}
			set {
				volumes = value;
			}
		}

		#endregion Properties
	}

	#endregion PlaneBoundedVolumeListSceneQuery Implementation

	#region IntersectionSceneQuery Implementation

	/// <summary>
	/// Separate SceneQuery class to query for pairs of objects which are
	///	possibly intersecting one another.
	/// </summary>
	/// <remarks>
	/// This SceneQuery subclass considers the whole world and returns pairs of objects
	/// which are close enough to each other that they may be intersecting. Because of
	/// this slightly different focus, the return types and listener interface are
	/// different for this class.
	/// </remarks>
	public abstract class IntersectionSceneQuery : SceneQuery, IIntersectionSceneQueryListener {
		#region Fields

		/// <summary>
		///		List of query results from the last execution of this query.
		/// </summary>
		protected IntersectionSceneQueryResult lastResults = new IntersectionSceneQueryResult();

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="creator">Scene manager who created this query.</param>
		internal IntersectionSceneQuery(SceneManager creator) : base(creator) {}

		#endregion Constructor

		#region Methods

		/// <summary>
		///		Clears out any cached results from the last query.
		/// </summary>
		public virtual void ClearResults() {
			lastResults.Clear();
		}

		/// <summary>
		///		Executes the query, returning the results back in one list.
		/// </summary>
		/// <remarks>
		///		This method executes the scene query as configured, gathers the results
		///		into one structure and returns a reference to that structure. These
		///		results will also persist in this query object until the next query is
		///		executed, or <see cref="ClearResults"/>. A more lightweight version of
		///		this method that returns results through a listener is also available.
		/// </remarks>
		/// <returns></returns>
		public virtual IntersectionSceneQueryResult Execute() {
			ClearResults();

			// execute the callback version using ourselves as the listener
			Execute(this);

			return lastResults;
		}

		/// <summary>
		///		Executes the query and returns each match through a listener interface.
		/// </summary>
		/// <remarks>
		///		Note that this method does not store the results of the query internally 
		///		so does not update the 'last result' value. This means that this version of
		///		execute is more lightweight and therefore more efficient than the version 
		///		which returns the results as a collection.
		/// </remarks>
		/// <param name="listener">Listener object to handle the result callbacks.</param>
		public abstract void Execute(IIntersectionSceneQueryListener listener);

		#endregion Methods

		#region IIntersectionSceneQueryListener Members

		bool Axiom.Core.IIntersectionSceneQueryListener.OnQueryResult(MovableObject first, MovableObject second) {
			// create an entry and add it to the cached result list
			lastResults.Objects2Objects.Add(new SceneQueryMovableObjectPair(first,second));

			// continue gathering results
			return true;
		}

		bool Axiom.Core.IIntersectionSceneQueryListener.OnQueryResult(MovableObject obj, SceneQuery.WorldFragment fragment) {
			// create an entry and add it to the cached result list
			lastResults.Objects2World.Add(new SceneQueryMovableObjectWorldFragmentPair(obj,fragment));

			// continue gathering results
			return true;
		}

		#endregion
	}

	/// <summary>
	///		Alternative listener interface for dealing with <see cref="IntersectionSceneQuery"/>.
	/// </summary>
	/// <remarks>
	///		Because the IntersectionSceneQuery returns results in pairs, rather than singularly,
	///		the listener interface must be customised from the standard SceneQueryListener.
	/// </remarks>
	public interface IIntersectionSceneQueryListener {
		/// <summary>
		///		Called when 2 movable objects intersect one another.
		/// </summary>
		/// <param name="first">Reference to the first intersecting object.</param>
		/// <param name="second">Reference to the second intersecting object.</param>
		/// <returns>Should return false to abandon returning additional results, or true to continue.</returns>
		bool OnQueryResult(MovableObject first, MovableObject second);

		/// <summary>
		///		Called when a movable intersects a world fragment.
		/// </summary>
		/// <param name="obj">Intersecting object.</param>
		/// <param name="fragment">Intersecting world fragment.</param>
		/// <returns>Should return false to abandon returning additional results, or true to continue.</returns>
		bool OnQueryResult(MovableObject obj, SceneQuery.WorldFragment fragment);
	}

	/// <summary>Holds the results of an intersection scene query (pair values).</summary>
	public class IntersectionSceneQueryResult {
        protected List<SceneQueryMovableObjectPair> objects2Objects = new List<SceneQueryMovableObjectPair>();
        protected List<SceneQueryMovableObjectWorldFragmentPair> objects2World = new List<SceneQueryMovableObjectWorldFragmentPair>();

        public List<SceneQueryMovableObjectPair> Objects2Objects {
			get {
				return objects2Objects;
			}
		}

		public List<SceneQueryMovableObjectWorldFragmentPair> Objects2World {
			get {
				return objects2World;
			}
		}

		public void Clear() {
			objects2Objects.Clear();
			objects2World.Clear();
		}
	}

	#endregion IntersectionSceneQuery Implementation
}
