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

#endregion LGPL License



#region Using Directives



using System;

using System.Collections;



using Axiom.Core;

using Axiom.MathLib;

using Axiom.Collections;

using Axiom.Media;



using Axiom.SceneManagers.PagingLandscape.Collections;



#endregion Using Directives



namespace Axiom.SceneManagers.PagingLandscape.Tile

{

	/// <summary>

	/// Summary description for TileManager.

	/// </summary>

	public class TileManager

	{

		#region Singleton Implementation

		

		/// <summary>

		/// Constructor

		/// </summary>

		private TileManager() 

		{

			numTiles = 0;
			// Add the requested initial number
			tiles = new TileRow();
			queue = new TileQueue();
			addBatch(Options.Instance.Num_Tiles);
		}





		private static TileManager instance = null;



		public static TileManager Instance 

		{

			get 

			{

				if ( instance == null ) instance = new TileManager();

				return instance;

			}

		}





		#endregion Singleton Implementation



		#region IDisposable Implementation



		public void Dispose()

		{

			if (instance == this) 

			{

				tiles.Clear();

				instance = null;

			}

		}



		#endregion IDisposable Implementation



		#region Fields

		protected TileRow tiles;

		protected TileQueue queue;

		protected long numTiles;


		#endregion Fields


		/// <summary>

		/// Retrieve a free Tile

		/// </summary>

		/// <returns>free tile</returns>
		public Tile GetTile( )
		{
			if ( queue.Size == 0 )
			{
				// We don´t have more renderables, so we are going to add more
				addBatch(Options.Instance.Num_Tiles_Increment);
				// Increment the next batch by a 10%
				Options.Instance.Num_Tiles_Increment += (long) (Options.Instance.Num_Tiles_Increment * 0.1f);
			}
			return queue.Pop( );
		}


		/// <summary>

		/// Marks a tile as free

		/// </summary>

		/// <param name="tile">Tile to free</param>
		public void FreeTile( Tile tile )
		{
			queue.Push( tile );
		}

		public long NumTiles(  ) 
		{
			return numTiles;
		}

		public int NumFree( ) 
		{
			return queue.Size;
		}


		protected void addBatch(long num)
		{
			numTiles += num;
			//    mTiles.reserve (mNumTiles);
			//    mTiles.resize (mNumTiles);
			for ( long i = 0; i < num; i++ )
			{
				Tile tile = new Tile( );
				tiles.Add( tile );
				queue.Push( tile );
			}
		}
	}

}

