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
using System.Diagnostics;
using Axiom.SceneManagers.Multiverse;
using Axiom.MathLib;

namespace Multiverse.Tools.MVSMTest
{
	/// <summary>
	/// Summary description for LODSpec.
	/// </summary>
	public class LODSpecPrev : ILODSpec
	{
		public LODSpecPrev()
		{
		}

		/// <summary>
		/// Returns the number of tiles (along X and Z axis) for a page at the given distance from the camera.
		/// Actual number of tiles in the page is this value squared.
		/// </summary>
		/// <param name="pagesFromCamera">distance (in pages) from this the page in question to the page containing the camera</param>
		/// <returns></returns>
		public int TilesPerPage(int pagesFromCamera)
		{
			int ret = 0;

			switch (pagesFromCamera) 
			{
				case 0:
				case 1:
				case 2:
				case 3:
				case 4:
					ret = 1;
					break;
				default:
					Debug.Assert(false, "TilesPerPage: out of range");
					ret = 0;
					break;
			}

			return ret;
		}

		// This function computes the level of detail(meters per sample) that a tile should use, based
		// on the tiles location (distance from the camera).  Eventually this might be a delegate supplied
		// by the application to allow for more flexible configuration.
		public int MetersPerSample(Vector3 tileLoc, int pagesFromCamera, int subPagesFromCamera)
		{
			int ret;

			switch ( pagesFromCamera )
			{
				case 0:
					ret = 8;
					break;
				case 1:
					ret = 16;
					break;
				case 2:
					ret = 32;
					break;
				case 3:
					ret = 64;
					break;
				case 4:
					ret = 128;
					break;
				default:
					Debug.Assert(false, "MetersPerSample: out of range");
					ret = 0;
					break;
			}

			return ret;
		}
	}
}
