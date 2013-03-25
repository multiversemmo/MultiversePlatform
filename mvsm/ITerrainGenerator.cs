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

using System.Xml;
using Axiom.MathLib;

namespace Axiom.SceneManagers.Multiverse
{
    public delegate void TerrainModificationStateChangedHandler(ITerrainGenerator generator, bool state);
    public delegate void TerrainChangedHandler(ITerrainGenerator generator, int worldXMeters, int worldZMeters, int sizeXMeters, int sizeZMeters);

	/// <summary>
	/// Summary description for ITerrainGenerator.
	/// </summary>
	public interface ITerrainGenerator
	{
        bool Modified { get;  }

	    event TerrainModificationStateChangedHandler TerrainModificationStateChanged;
	    event TerrainChangedHandler TerrainChanged;

		float GenerateHeightPointMM(float xWorldLocationMeters, float zWorldLocationMeters);
		float GenerateHeightPointMM(Vector3 worldLocationMM);

	    void Save(bool force);
        void ToXml(XmlWriter r);
	}
}
