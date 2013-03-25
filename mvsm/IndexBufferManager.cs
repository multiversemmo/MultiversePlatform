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

using System;
using System.Collections;
using System.Diagnostics;
using Axiom.Graphics;

namespace Axiom.SceneManagers.Multiverse
{
	/// <summary>
	/// Summary description for IndexBufferManager.
	/// </summary>
	public class IndexBufferManager
	{

		private ArrayList tileIndexBuffers;
		private Hashtable stitchIndexBuffers;
		
		private IndexBufferManager()
		{
			tileIndexBuffers = new ArrayList();
			stitchIndexBuffers = new Hashtable();
		}

		private static IndexBufferManager instance = null;

		public static IndexBufferManager Instance 
		{
			get
			{
				if ( instance == null ) 
				{
					instance = new IndexBufferManager();
				}
				return instance;
			}
		}

		public IndexData GetTileIndexBuffer(int size)
		{
			while ( size >= tileIndexBuffers.Count ) 
			{
				tileIndexBuffers.Add(null);	
			}

			if ( tileIndexBuffers[size] == null ) 
			{
				tileIndexBuffers[size] = buildIndexData(size);
			}

			Debug.Assert(tileIndexBuffers[size] != null, "returning null index buffer");
			return tileIndexBuffers[size] as IndexData;
		}

		private IndexData buildIndexData(int size)
		{
			IndexData indexData = new IndexData();

			int bufLength = size * size * 6;
			
			indexData.indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
				IndexType.Size16, bufLength, BufferUsage.StaticWriteOnly);

			IntPtr indexBufferPtr = indexData.indexBuffer.Lock(0,indexData.indexBuffer.Size,BufferLocking.Discard);
			int indexCount = 0;

			int pos = 0;
			int i = 0;
			unsafe
			{	
				ushort* indexBuffer = (ushort *)indexBufferPtr.ToPointer();
				for (int z = 0; z < size - 1; z++ )
				{
					for (int x = 0; x <  size - 1; x++ )
					{
						indexBuffer[pos++] = (ushort)i;
						indexBuffer[pos++] = (ushort)(i + size);
						indexBuffer[pos++] = (ushort)(i + 1);

						indexBuffer[pos++] = (ushort)(i + size);
						indexBuffer[pos++] = (ushort)(i + 1 + size);
						indexBuffer[pos++] = (ushort)(i + 1);

						i++;
						indexCount += 6; 
					}

					i++;
				}  
			}

			indexData.indexBuffer.Unlock();
			indexData.indexCount = indexCount;
			indexData.indexStart = 0;

			return indexData;
		}

		public IndexData GetStitchIndexBuffer(int numSamples, int northSamples, int eastSamples)
		{
			string name = String.Format("{0},{1},{2}", numSamples, northSamples, eastSamples);

			IndexData indexData = this.stitchIndexBuffers[name] as IndexData;

			if ( indexData == null ) 
			{
				indexData = new IndexData();

				int triangleCount = 0;

				// count north triangles
				if ( northSamples != 0 ) 
				{
					if ( northSamples == numSamples ) 
					{
						triangleCount += ( ( numSamples - 1 )* 2 );
					} 
					else if ( northSamples > numSamples ) 
					{
						triangleCount += ( ( numSamples - 1 ) * 3 );
					} 
					else 
					{
						triangleCount += ( ( northSamples - 1 ) * 3 );
					}

					// count northeast triangles
					if ( eastSamples != 0 )
					{
						if ( eastSamples == northSamples ) 
						{
							if ( eastSamples == numSamples ) 
							{
								triangleCount += 2;
							}
							else 
							{
								triangleCount += 4;
							}
						} 
						else 
						{
							triangleCount += 3;
						}
					}
				}

				// count east triangles
				if ( eastSamples != 0 ) 
				{
					if ( eastSamples == numSamples ) 
					{
						triangleCount += ( ( numSamples - 1 )* 2 );
					} 
					else if ( eastSamples > numSamples ) 
					{
						triangleCount += ( ( numSamples - 1 ) * 3 );
					} 
					else 
					{
						triangleCount += ( ( eastSamples - 1 ) * 3 );
					}
				}




				int bufLength = triangleCount * 3;
			
				indexData.indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
					IndexType.Size16, bufLength, BufferUsage.StaticWriteOnly);

				IntPtr indexBufferPtr = indexData.indexBuffer.Lock(0,indexData.indexBuffer.Size,BufferLocking.Discard);

				int pos = 0;
				unsafe
				{	
					ushort* indexBuffer = (ushort *)indexBufferPtr.ToPointer();

					int neighborOff;
					int innerCornerOff = 0;
					int outerCornerOff = 0;

					if ( northSamples == 0 )
					{
						// there is no north side
						neighborOff = numSamples;
						outerCornerOff = numSamples;
					} 
					else if ( eastSamples == 0 ) 
					{
						// there is no east side
						neighborOff = numSamples;
					} 
					else
					{
						neighborOff = ( numSamples * 2 ) - 1;
						innerCornerOff = numSamples - 1;
						outerCornerOff = neighborOff + northSamples;
					}

					int eastTOff = 0;
					int eastNOff = 0;

					// generate north triangles
					if ( northSamples != 0 ) 
					{
						int tOff = 0;
						int nOff = neighborOff;

						if ( northSamples == numSamples ) 
						{
							for ( int i = 0; i < ( numSamples - 1 ); i++ ) 
							{
								indexBuffer[pos++] = (ushort)tOff;
								indexBuffer[pos++] = (ushort)nOff;
								indexBuffer[pos++] = (ushort)( tOff + 1 );

								indexBuffer[pos++] = (ushort)nOff;
								indexBuffer[pos++] = (ushort)( nOff + 1 );
								indexBuffer[pos++] = (ushort)( tOff + 1 );
								nOff++;
								tOff++;
							}
						} 
						else if ( northSamples > numSamples ) 
						{
							for ( int i = 0; i < ( numSamples - 1 ); i++ ) 
							{
								indexBuffer[pos++] = (ushort)tOff;
								indexBuffer[pos++] = (ushort)nOff;
								indexBuffer[pos++] = (ushort)( nOff + 1 );

								indexBuffer[pos++] = (ushort)tOff;
								indexBuffer[pos++] = (ushort)( nOff + 1 );
								indexBuffer[pos++] = (ushort)( tOff + 1 );

								indexBuffer[pos++] = (ushort)( nOff + 1 );
								indexBuffer[pos++] = (ushort)( nOff + 2 );
								indexBuffer[pos++] = (ushort)( tOff + 1 );
								nOff += 2;
								tOff++;
							}
						} 
						else 
						{
							for ( int i = 0; i < ( northSamples - 1 ); i++ ) 
							{
								indexBuffer[pos++] = (ushort)tOff;
								indexBuffer[pos++] = (ushort)nOff;
								indexBuffer[pos++] = (ushort)( tOff + 1 );

								indexBuffer[pos++] = (ushort)nOff;
								indexBuffer[pos++] = (ushort)( nOff + 1 );
								indexBuffer[pos++] = (ushort)( tOff + 1 );

								indexBuffer[pos++] = (ushort)( tOff + 1 );
								indexBuffer[pos++] = (ushort)( nOff + 1 );
								indexBuffer[pos++] = (ushort)( tOff + 2 );
								nOff++;
								tOff += 2;
							}
						}

						// draw northeast triangles
						if ( eastSamples != 0 )
						{
							if ( eastSamples == northSamples ) 
							{
								if ( eastSamples == numSamples ) 
								{ // neighbors same lod as tile
									indexBuffer[pos++] = (ushort)innerCornerOff;
									indexBuffer[pos++] = (ushort)( outerCornerOff - 1 );
									indexBuffer[pos++] = (ushort)outerCornerOff;

									indexBuffer[pos++] = (ushort)innerCornerOff;
									indexBuffer[pos++] = (ushort)outerCornerOff;
									indexBuffer[pos++] = (ushort)( outerCornerOff + 1 );

									eastTOff = innerCornerOff;
									eastNOff = outerCornerOff + 1;
								}
								else 
								{ // neighbors lesser LOD
									indexBuffer[pos++] = (ushort)( innerCornerOff - 1 );
									indexBuffer[pos++] = (ushort)( outerCornerOff - 1 );
									indexBuffer[pos++] = (ushort)innerCornerOff;

									indexBuffer[pos++] = (ushort)( outerCornerOff - 1 );
									indexBuffer[pos++] = (ushort)outerCornerOff;
									indexBuffer[pos++] = (ushort)innerCornerOff;

									indexBuffer[pos++] = (ushort)innerCornerOff;
									indexBuffer[pos++] = (ushort)outerCornerOff;
									indexBuffer[pos++] = (ushort)( outerCornerOff + 1 );

									indexBuffer[pos++] = (ushort)innerCornerOff;
									indexBuffer[pos++] = (ushort)( outerCornerOff + 1 );
									indexBuffer[pos++] = (ushort)( innerCornerOff + 1 );

									eastTOff = innerCornerOff + 1;
									eastNOff = outerCornerOff + 1;
								}
							} 
							else 
							{ // both neighbors exist.  one is the same lod as tile.
								if ( northSamples == numSamples ) 
								{
									// LOD change is to the east
									if ( eastSamples < numSamples ) 
									{ 
										// lower LOD to east
										indexBuffer[pos++] = (ushort)innerCornerOff;
										indexBuffer[pos++] = (ushort)( outerCornerOff - 1 );
										indexBuffer[pos++] = (ushort)outerCornerOff;

										indexBuffer[pos++] = (ushort)innerCornerOff;
										indexBuffer[pos++] = (ushort)outerCornerOff;
										indexBuffer[pos++] = (ushort)( outerCornerOff + 1 );

										indexBuffer[pos++] = (ushort)innerCornerOff;
										indexBuffer[pos++] = (ushort)( outerCornerOff + 1 );
										indexBuffer[pos++] = (ushort)( innerCornerOff + 1 );

										eastTOff = innerCornerOff + 1;
										eastNOff = outerCornerOff + 1;
									}
									else 
									{
										// higher LOD to east
										indexBuffer[pos++] = (ushort)( outerCornerOff - 1 );
										indexBuffer[pos++] = (ushort)outerCornerOff;
										indexBuffer[pos++] = (ushort)( outerCornerOff + 1 );

										indexBuffer[pos++] = (ushort)( outerCornerOff - 1 );
										indexBuffer[pos++] = (ushort)( outerCornerOff + 1 );
										indexBuffer[pos++] = (ushort)innerCornerOff;

										indexBuffer[pos++] = (ushort)innerCornerOff;
										indexBuffer[pos++] = (ushort)( outerCornerOff + 1 );
										indexBuffer[pos++] = (ushort)( outerCornerOff + 2 );

										eastTOff = innerCornerOff;
										eastNOff = outerCornerOff + 2;
									}
								} 
								else 
								{
									// LOD Change is to the north
									if ( northSamples < numSamples ) 
									{ 
										// lower LOD to north
										indexBuffer[pos++] = (ushort)( innerCornerOff - 1 );
										indexBuffer[pos++] = (ushort)( outerCornerOff - 1 );
										indexBuffer[pos++] = (ushort)innerCornerOff;

										indexBuffer[pos++] = (ushort)( outerCornerOff - 1 );
										indexBuffer[pos++] = (ushort)outerCornerOff;
										indexBuffer[pos++] = (ushort)innerCornerOff;

										indexBuffer[pos++] = (ushort)innerCornerOff;
										indexBuffer[pos++] = (ushort)outerCornerOff;
										indexBuffer[pos++] = (ushort)( outerCornerOff + 1 );

										eastTOff = innerCornerOff;
										eastNOff = outerCornerOff + 1;
									}
									else 
									{
										// higher LOD to north
										indexBuffer[pos++] = (ushort)innerCornerOff;
										indexBuffer[pos++] = (ushort)( outerCornerOff - 2 );
										indexBuffer[pos++] = (ushort)( outerCornerOff - 1 );

										indexBuffer[pos++] = (ushort)innerCornerOff;
										indexBuffer[pos++] = (ushort)( outerCornerOff - 1 );
										indexBuffer[pos++] = (ushort)( outerCornerOff + 1 );

										indexBuffer[pos++] = (ushort)( outerCornerOff - 1 );
										indexBuffer[pos++] = (ushort)outerCornerOff;
										indexBuffer[pos++] = (ushort)( outerCornerOff + 1 );

										eastTOff = innerCornerOff;
										eastNOff = outerCornerOff + 1;
									}
								}
							}
						}
					} 
					else 
					{
						// no north neighbor
						eastTOff = 0;
						eastNOff = numSamples;
					}

					// draw east triangles
					if ( eastSamples != 0 ) 
					{
						int tOff = eastTOff;
						int nOff = eastNOff;

						if ( eastSamples == numSamples ) 
						{
							for ( int i = 0; i < ( numSamples - 1 ); i++ ) 
							{
								indexBuffer[pos++] = (ushort)( tOff + 1 );
								indexBuffer[pos++] = (ushort)tOff;
								indexBuffer[pos++] = (ushort)( nOff + 1 );

								indexBuffer[pos++] = (ushort)tOff;
								indexBuffer[pos++] = (ushort)nOff;
								indexBuffer[pos++] = (ushort)( nOff + 1 );

								tOff++;
								nOff++;
							}
						} 
						else if ( eastSamples > numSamples ) 
						{
							for ( int i = 0; i < ( numSamples - 1 ); i++ ) 
							{
								indexBuffer[pos++] = (ushort)tOff;
								indexBuffer[pos++] = (ushort)nOff;
								indexBuffer[pos++] = (ushort)( nOff + 1 );

								indexBuffer[pos++] = (ushort)tOff;
								indexBuffer[pos++] = (ushort)( nOff + 1 );
								indexBuffer[pos++] = (ushort)( tOff + 1 );

								indexBuffer[pos++] = (ushort)( nOff + 1 );
								indexBuffer[pos++] = (ushort)( nOff + 2 );
								indexBuffer[pos++] = (ushort)( tOff + 1 );
								nOff += 2;
								tOff++;
							}
						} 
						else 
						{
							for ( int i = 0; i < ( eastSamples - 1 ); i++ ) 
							{
								indexBuffer[pos++] = (ushort)tOff;
								indexBuffer[pos++] = (ushort)nOff;
								indexBuffer[pos++] = (ushort)( tOff + 1 );

								indexBuffer[pos++] = (ushort)nOff;
								indexBuffer[pos++] = (ushort)( nOff + 1 );
								indexBuffer[pos++] = (ushort)( tOff + 1 );

								indexBuffer[pos++] = (ushort)( tOff + 1 );
								indexBuffer[pos++] = (ushort)( nOff + 1 );
								indexBuffer[pos++] = (ushort)( tOff + 2 );

								nOff++;
								tOff += 2;
							}
						}
					}
				}

				Debug.Assert(pos == ( triangleCount * 3 ), "stitching:wrong number of indices generated");

				indexData.indexBuffer.Unlock();
				indexData.indexCount = triangleCount * 3;
				indexData.indexStart = 0;

				stitchIndexBuffers[name] = indexData;
			}

			return indexData;
		}
	}
}
