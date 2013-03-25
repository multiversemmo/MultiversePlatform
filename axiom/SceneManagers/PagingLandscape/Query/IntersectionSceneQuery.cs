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



#endregion Using Directives



namespace Axiom.SceneManagers.PagingLandscape.Query

{

	/// <summary>

	/// Summary description for IPLIntersectionSceneQuery.

	/// </summary>

	public class IntersectionSceneQuery: DefaultIntersectionSceneQuery

	{

		/// <summary>

		///		Constructor

		/// </summary>

		/// <param name="creator">SceneManage that created this query</param>

		public IntersectionSceneQuery( SceneManager creator ) : base( creator )

		{

			worldFragmentTypes &= WorldFragmentType.RenderOperation;

		}



		/// <summary>

		///		<see cref="IntersectionSceneQuery"/>

		/// </summary>

		public override void Execute( IIntersectionSceneQueryListener listener )

		{

			// Do movables to movables as before

			base.Execute( listener );

			SceneQuery.WorldFragment frag = new SceneQuery.WorldFragment();



			// Do entities to world

		    SceneManager sceneMgr = (SceneManager)( this.creator );



            for(int i = 0; i < sceneMgr.Entities.Count; i++) 

			{

                Entity entityA = sceneMgr.Entities[i];



				// Apply mask 

				if ( ( entityA.QueryFlags & queryMask) == 0 ) 

				{

					AxisAlignedBox box = entityA.GetWorldBoundingBox( );

					ArrayList opList = new ArrayList();



		/*

					for ( int j = 0; j < mOptions->world_height; j++ )

					{

						for ( int i = 0; i < mOptions->world_width; i++ )

						{

		//					if ( sceneMgr->mPages[ i ][ j ]->isLoaded( ) == true )

		//					{

		//						sceneMgr->mPages[ i ][ j ]->getIPLRenderOpsInBox( box, opList );

		//					}

						}

					}						

		*/          

					for ( int j = 0; j < opList.Count; ++j )

					{

						frag.FragmentType = WorldFragmentType.RenderOperation;

						frag.RenderOp = (RenderOperation) opList[i];

						listener.OnQueryResult( entityA, frag );

					}

				}

			}

		}

	}



}

