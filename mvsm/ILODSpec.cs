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
using Axiom.MathLib;

namespace Axiom.SceneManagers.Multiverse
{
	/// <summary>
	/// Summary description for ILODSpec.
	/// </summary>
	public interface ILODSpec
	{
		/// <summary>
		/// Returns the number of tiles (along X and Z axis) for a page at the given distance from the camera.
		/// Actual number of tiles in the page is this value squared.
		/// </summary>
		/// <param name="pagesFromCamera">distance (in pages) from this the page in question to the page containing the camera</param>
		/// <returns></returns>
		int TilesPerPage(int pagesFromCamera);

		// This function computes the level of detail(meters per sample) that a tile should use, based
		// on the tiles location (distance from the camera).  Eventually this might be a delegate supplied
		// by the application to allow for more flexible configuration.
		int MetersPerSample(Vector3 tileLoc, int pagesFromCamera, int subPagesFromCamera);

		/// <summary>
        /// Gets or sets the size of a terrain tile in meters
        /// </summary>
        int PageSize 
        {
            get;
            set;
        }
        
		/// <summary>
        /// Gets or sets how far the camera can see in pages
        /// </summary>
        int VisiblePageRadius
        {
            get;
            set;
        }

	}
}
