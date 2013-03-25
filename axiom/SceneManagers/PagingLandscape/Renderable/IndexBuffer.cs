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

using System.Diagnostics;



using Axiom.Core;

using Axiom.MathLib;

using Axiom.Collections;

using Axiom.Media;

using Axiom.Graphics;



using Axiom.SceneManagers.PagingLandscape.Collections;



#endregion Using Directives



namespace Axiom.SceneManagers.PagingLandscape.Renderable

{

	enum Stitch_Shift : int

	{

		North = 0,

		South = 8,

		West = 16,

		East = 24

	}



	enum Stitch_Direction : long

	{

		North	=  128L << Stitch_Shift.North,
		South	=  128L << Stitch_Shift.South,
		West	=  128L << Stitch_Shift.West,
        East	=  128L << Stitch_Shift.East
	}



	/// <summary>

	/// Summary description for IndexBuffer.

	/// </summary>

	public class IndexBuffer: IDisposable

	{

		
		#region Fields
		protected long tileSize;

		protected ArrayList cache;
		/// Shared array of IndexData (reuse indexes across tiles)
		protected ArrayList levelIndex;
		// Store the indexes for every combination
		protected long numIndexes;
		#endregion Fields

		#region Singleton Implementation

		

		/// <summary>

		/// Constructor

		/// </summary>

		public IndexBuffer() 

		{

			if (instance != null) 

			{

				throw new ApplicationException("IndexBuffer.Constructor() called twice!");

			}

			instance = this;



			tileSize = Options.Instance.TileSize + 1;
			numIndexes = Options.Instance.MaxRenderLevel + 1;
			//mLevelIndex.reserve (mNumIndexes);
			cache = new ArrayList();
			levelIndex = new ArrayList((int)numIndexes);
			for ( long i = 0; i < numIndexes; i++ )
			{
				levelIndex.Add( new Map() );
			}
		}





		private static IndexBuffer instance = null;



		public static IndexBuffer Instance 

		{

			get 

			{

				return instance;

			}

		}





		#endregion Singleton Implementation



		#region IDisposable Implementation



		public void Dispose()

		{

			if (instance == this) 

			{

				levelIndex.Clear();

				cache.Clear();

				instance = null;

			}

		}



		#endregion IDisposable Implementation



		//public IndexData GetIndex( int LOD)
		//{
		//}

		/** Utility method to generate stitching indexes on the edge of a tile
		@param neighbor The neighbor direction to stitch
		@param hiLOD The LOD of this tile
		@param loLOD The LOD of the neighbor
		@param omitFirstTri Whether the first tri of the stitch (always clockwise
		relative to the centre of this tile) is to be omitted because an 
		adjoining edge is also being stitched
		@param omitLastTri Whether the last tri of the stitch (always clockwise
		relative to the centre of this tile) is to be omitted because an 
		adjoining edge is also being stitched
		@param pIdx Pointer to a pointer to the index buffer to push the results 
		into (this pointer will be updated)
		@returns The number of indexes added
		*/
		public long StitchEdge(Neighbor neighbor, long hiLOD, long loLOD, bool omitFirstTri, bool omitLastTri, IntPtr Idx, ref long pos)
		{
			Debug.Assert( loLOD > hiLOD );
			/* 
			Now do the stitching; we can stitch from any level to any level.
			The stitch pattern is like this for each pair of vertices in the lower LOD
			(excuse the poor ascii art):

			lower LOD
			*-----------*
			|\  \ 3 /  /|
			|1\2 \ / 4/5|
			*--*--*--*--*
			higher LOD

			The algorithm is, for each pair of lower LOD vertices:
			1. Iterate over the higher LOD vertices, generating tris connected to the 
			first lower LOD vertex, up to and including 1/2 the span of the lower LOD 
			over the higher LOD (tris 1-2). Skip the first tri if it is on the edge 
			of the tile and that edge is to be stitched itself.
			2. Generate a single tri for the middle using the 2 lower LOD vertices and 
			the middle vertex of the higher LOD (tri 3). 
			3. Iterate over the higher LOD vertices from 1/2 the span of the lower LOD
			to the end, generating tris connected to the second lower LOD vertex 
			(tris 4-5). Skip the last tri if it is on the edge of a tile and that
			edge is to be stitched itself.

			The same algorithm works for all edges of the patch; stitching is done
			clockwise so that the origin and steps used change, but the general
			approach does not.
			*/

			// Work out the steps ie how to increment indexes
			// Step from one vertex to another in the high detail version
			int step = 1 << (int)hiLOD;
			// Step from one vertex to another in the low detail version
			int superstep = 1 << (int)loLOD;
			// Step half way between low detail steps
			int halfsuperstep = superstep >> 1;

			// Work out the starting points and sign of increments
			// We always work the strip clockwise
			int startx = 0, starty = 0, endx = 0, rowstep = 0;
			bool horizontal = false;
			switch(neighbor)
			{
				case Neighbor.North:
					startx = starty = 0;
					endx =  (int)this.tileSize - 1;
					rowstep = step;
					horizontal = true;
					break;
				case Neighbor.South:
					// invert x AND y direction, helps to keep same winding
					startx = starty = (int)this.tileSize - 1;
					endx = 0;
					rowstep = -step;
					step = -step;
					superstep = -superstep;
					halfsuperstep = -halfsuperstep;
					horizontal = true;
					break;
				case Neighbor.East:
					startx = 0;
					endx = (int)this.tileSize - 1;
					starty = (int)this.tileSize - 1;
					rowstep = -step;
					horizontal = false;
					break;
				case Neighbor.West:
					startx = (int)this.tileSize  - 1;
					endx = 0;
					starty = 0;
					rowstep = step;
					step = -step;
					superstep = -superstep;
					halfsuperstep = -halfsuperstep;
					horizontal = false;
					break;
				default:
					break;
			};

			long numStitches = 0;

			unsafe
			{	
				ushort* pIdx = (ushort *)Idx.ToPointer();
				for ( int j = startx; j != endx; j += superstep )
				{
					int k; 
					for (k = 0; k != halfsuperstep; k += step)
					{
						int jk = j + k;
						//skip the first bit of the corner?
						if ( j != startx || k != 0 || !omitFirstTri )
						{
							if (horizontal)
							{
								pIdx[pos++] = index( j , starty );						numStitches++;
								pIdx[pos++] = index( jk, starty + rowstep );			numStitches++;
								pIdx[pos++] = index( jk + step, starty + rowstep );		numStitches++;
							}
							else
							{
								pIdx[pos++] = index( starty, j );						numStitches++;
								pIdx[pos++] = index( starty + rowstep, jk );			numStitches++;
								pIdx[pos++] = index( starty + rowstep, jk + step);		numStitches++;
							}
						}
					}

					// Middle tri
					if (horizontal)
					{
						pIdx[pos++] = index( j, starty );								numStitches++;
						pIdx[pos++] = index( j + halfsuperstep, starty + rowstep);		numStitches++;
						pIdx[pos++] = index( j + superstep, starty );					numStitches++;
					}
					else
					{
						pIdx[pos++] = index( starty, j );								numStitches++;
						pIdx[pos++] = index( starty + rowstep, j + halfsuperstep );		numStitches++;
						pIdx[pos++] = index( starty, j + superstep );					numStitches++;
					}

					for (k = halfsuperstep; k != superstep; k += step)
					{
						int jk = j + k;
						if ( j != endx - superstep || k != superstep - step || !omitLastTri )
						{
							if (horizontal)
							{
								pIdx[pos++] = index( j + superstep, starty );			numStitches++;
								pIdx[pos++] = index( jk, starty + rowstep );			numStitches++;
								pIdx[pos++] = index( jk + step, starty + rowstep );		numStitches++;
							}
							else
							{
								pIdx[pos++] = index( starty, j + superstep );			numStitches++;
								pIdx[pos++] = index( starty + rowstep, jk );			numStitches++;
								pIdx[pos++] = index( starty + rowstep, jk + step );		numStitches++;
							}
						}
					}
				}
			}

			return numStitches;
		}


		/// Gets the index data for this tile based on current settings
		public IndexData GetIndexData( long stitchFlags, long RenderLevel, Renderable[] neighbors)
		{
			Debug.Assert (levelIndex[ (int)RenderLevel ] != null);
			IEnumerator ii = ((Axiom.Collections.Map)(levelIndex[ (int)RenderLevel ])).Find( (long)stitchFlags );
			if ( ii == null)
			{
				// Create
				IndexData indexData = GenerateTriListIndexes((long)stitchFlags, RenderLevel, neighbors);
				((Axiom.Collections.Map)(levelIndex[ (int)RenderLevel ])).Insert((long) stitchFlags, indexData );
				return indexData;
			}
			else
			{
				ii.MoveNext();
				return (IndexData)ii.Current;
			}
		}

		/// Internal method for generating triangle list terrain indexes
		public IndexData GenerateTriListIndexes( long stitchFlags, long RenderLevel, Renderable[] neighbors)
		{
			long step = (1L << (int)RenderLevel);
			long north = ((stitchFlags & (long)Stitch_Direction.North) != 0 ? step : 0);
			long south = ((stitchFlags & (long)Stitch_Direction.South) != 0 ? step : 0);
			long east =  ((stitchFlags & (long)Stitch_Direction.East)  != 0 ? step : 0);
			long west =  ((stitchFlags & (long)Stitch_Direction.West)  != 0 ? step : 0);
        
			long new_length = ( tileSize * tileSize * 6 ) / step;

			IndexData indexData = new IndexData();
			indexData.indexBuffer = 
			HardwareBufferManager.Instance.CreateIndexBuffer(IndexType.Size16, (int)new_length, BufferUsage.StaticWriteOnly);

			cache.Add( indexData );

			/** Returns the index into the height array for the given coordinates. */
			IntPtr ipIdx = indexData.indexBuffer.Lock(0,indexData.indexBuffer.Size,BufferLocking.Discard);
			numIndexes = 0;

			long pos = 0;
			long step_offset = step * tileSize;
			long height_count = north * tileSize;
			unsafe
			{	
				ushort* pIdx = (ushort *)ipIdx.ToPointer();
				for (long j = north; j < tileSize - 1 - south; j += step )
				{
					for (long i = west; i <  tileSize - 1 - east; i += step )
					{
						//triangles

						pIdx[pos++] = (ushort) (i        + height_count);                 numIndexes++;      
						pIdx[pos++] = (ushort) (i        + height_count + step_offset);   numIndexes++; 
						pIdx[pos++] = (ushort) (i + step + height_count);                 numIndexes++; 
                
						pIdx[pos++] = (ushort) (i        + height_count + step_offset);   numIndexes++;    
						pIdx[pos++] = (ushort) (i + step + height_count + step_offset);   numIndexes++; 
						pIdx[pos++] = (ushort) (i + step + height_count);                 numIndexes++; 

					}
					height_count += step_offset;
				}  

			}

			// North stitching
			if ( north != 0 )
			{
				numIndexes += StitchEdge(Neighbor.North, RenderLevel, neighbors[(int)Neighbor.North].RenderLevel,
					west > 0 , east > 0 , ipIdx,ref pos);
			}
			// East stitching
			if ( east != 0 )
			{
				numIndexes += StitchEdge(Neighbor.East, RenderLevel, neighbors[(int)Neighbor.East].RenderLevel,
					north > 0, south > 0, ipIdx,ref pos);
			}
			// South stitching
			if ( south != 0 )
			{
				numIndexes += StitchEdge(Neighbor.South, RenderLevel, neighbors[(int)Neighbor.South].RenderLevel,
					east > 0 , west > 0, ipIdx,ref pos);
			}
			// West stitching
			if ( west != 0 )
			{
				numIndexes += StitchEdge(Neighbor.West, RenderLevel, neighbors[(int)Neighbor.West].RenderLevel,
					south > 0 , north > 0, ipIdx,ref pos);
			}


			indexData.indexBuffer.Unlock();
			indexData.indexCount = (int)numIndexes;
			indexData.indexStart = 0;

			return indexData;
		}

		

		/** Returns the index into the height array for the given coordinates. */
		protected ushort index( int x, int z ) 
		{
			return (ushort)(x + z * tileSize);
		}       


	}

}

