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



#region Using Directives



using System;

using System.Collections;



using Axiom.Core;

using Axiom.MathLib;

using Axiom.Graphics;



using Axiom.SceneManagers.PagingLandscape;

using Axiom.SceneManagers.PagingLandscape.Query;

using Axiom.SceneManagers.PagingLandscape.Data2D;



#endregion Using Directives



namespace Axiom.SceneManagers.PagingLandscape.Query

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

		OnexRes        = 1<<7

	};



	/// <summary>

	/// 	IPL's specialisation of RaySceneQuery.

	/// 	if RSQ_Height bit mask is set, RSQ_Terrain and RSQ_Entity bits will be ignored

	/// 	Otherwise data will be returned based on the mask

	/// </summary>

	public class RaySceneQuery : DefaultRaySceneQuery

	{

		protected ArrayList fragmentList =  new ArrayList();



		/// <summary>

		///		Constructor

		/// </summary>

		/// <param name="creator">SceneManager that creates this query</param>

		public RaySceneQuery(SceneManager creator): base( creator)

		{

			this.AddWorldFragmentType( WorldFragmentType.SingleIntersection );

		}



		/// <summary>

		///		<see cref="RaySceneQuery"/>

		/// </summary>

		public override void Execute( IRaySceneQueryListener listener )

		{ 

			clearFragmentList( );



			ulong mask = QueryMask;

			SceneQuery.WorldFragment frag;



			if ( (mask & (ulong)RaySceneQueryType.Height) != 0)

			{

				// we don't want to bother checking for entities because a 

				// UNIT_Y ray is assumed to be a height test, not a ray test

				frag = new SceneQuery.WorldFragment( );

				fragmentList.Add( frag );



				frag.FragmentType = WorldFragmentType.SingleIntersection; 

				Vector3 origin = this.Ray.Origin;

				origin.y = 0; // ensure that it's within bounds

				frag.SingleIntersection = getHeightAt( origin );

				listener.OnQueryResult( frag, Math.Abs(frag.SingleIntersection.y - this.Ray.Origin.y) );

			}

			else

			{

				// Check for entity contacts

				if ( (mask & (ulong)RaySceneQueryType.Entities) != 0 )

				{

					base.Execute( listener );

				}



				if ( (mask & (ulong)RaySceneQueryType.AllTerrain) != 0 || (mask & (ulong)RaySceneQueryType.FirstTerrain) !=0 )

				{

					Vector3 ray = Ray.Origin;

					Vector3 land = getHeightAt( ray );

					float dist = 0, resFactor = 1;



					// Only bother if the non-default mask has been set

					if ( ( mask & (ulong)RaySceneQueryType.OnexRes ) != 0 )

					{

						if ( (mask & (ulong)RaySceneQueryType.TwoxRes) != 0 )

						{

							resFactor = 0.5F;

						}

						else if ( (mask & (ulong)RaySceneQueryType.FourxRes) !=0 )

						{

							resFactor = 0.25F;

						}

						else if ( (mask & (ulong)RaySceneQueryType.EightxRes) != 0 )

						{

							resFactor = 0.125F;

						}

					}



					while ( land.y != -1 )

					{

						ray += Ray.Direction * resFactor;

						dist += 1 * resFactor;



						land = getHeightAt( ray );

						if ( ray.y < land.y )

						{

							frag = new SceneQuery.WorldFragment( );

							fragmentList.Add( frag );



							frag.FragmentType = WorldFragmentType.SingleIntersection; 

							frag.SingleIntersection = land;

							listener.OnQueryResult( frag, dist );



							if ( (mask & (ulong)RaySceneQueryType.FirstTerrain )!= 0)

							{

								return;

							}

						}

					} 

				}	

			}

		}             

		

		/// <summary>

		/// 

		/// </summary>

		/// <param name="origin"></param>

		/// <returns></returns>

		protected Vector3 getHeightAt( Vector3 origin )

		{

			return new Vector3( origin.x, Data2DManager.Instance.GetRealWorldHeight( origin.x, origin.z ), origin.z );

		}



		/// <summary>

		///		Removes Cached fragments from last query

		/// </summary>

		protected void clearFragmentList( )

		{

			fragmentList.Clear();

		}

	}

}

