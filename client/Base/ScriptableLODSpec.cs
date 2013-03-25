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

namespace Multiverse.Base
{
    /// <summary>
	/// Delegate for the TilesPerPage interface method
	/// </summary>
	public delegate int TilesPerPageDelegate(Axiom.SceneManagers.Multiverse.ILODSpec lodSpec,
                                             int pagesFromCamera);
    
    /// <summary>
	/// Delegate for the MetersPerSample interface method
	/// </summary>
	public delegate int MetersPerSampleDelegate(Axiom.SceneManagers.Multiverse.ILODSpec lodSpec,
                                                Vector3 tileLoc,
                                                int pagesFromCamera,
                                                int subPagesFromCamera);
    
    /// <summary>
	/// Summary description for LODSpec.
	/// </summary>
	public class ScriptableLODSpec : Axiom.SceneManagers.Multiverse.DefaultLODSpec
	{
		public ScriptableLODSpec(TilesPerPageDelegate tilesPerPageDelegate,
                                 MetersPerSampleDelegate metersPerSampleDelegate,
                                 int pageSize,
                                 int visPageRadius) : base(pageSize, visPageRadius)
        {
            this.tilesPerPageDelegate = tilesPerPageDelegate;
            this.metersPerSampleDelegate = metersPerSampleDelegate;
		}

		/// <summary>
		/// Returns the number of tiles (along X and Z axis) for a page at the given distance from the camera.
		/// Actual number of tiles in the page is this value squared.
		/// </summary>
		/// <param name="pagesFromCamera">distance (in pages) from this the page in question to the page containing the camera</param>
		/// <returns></returns>
		public override int TilesPerPage(int pagesFromCamera)
		{
			return tilesPerPageDelegate(this, pagesFromCamera);
		}

		/// <summary>
		// This function computes the level of detail(meters per sample) that a tile should use, based
		// on the tiles location (distance from the camera).  Eventually this might be a delegate supplied
		// by the application to allow for more flexible configuration.
		/// </summary>
		public override int MetersPerSample(Vector3 tileLoc, int pagesFromCamera, int subPagesFromCamera)
		{
            return metersPerSampleDelegate(this, tileLoc, pagesFromCamera, subPagesFromCamera);
		}

        protected TilesPerPageDelegate tilesPerPageDelegate;
        protected MetersPerSampleDelegate metersPerSampleDelegate;

	}
}
