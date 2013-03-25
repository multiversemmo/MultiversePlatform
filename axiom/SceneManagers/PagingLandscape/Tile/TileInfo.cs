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

	/// This class holds the Tile info

	/// </summary>

	/// <remarks>

	/// This will avoid to pass a lot of data to the Renderable class.

	/// </remarks>

	public class TileInfo

	{
		//This is the Page Index in the Page Array
		public long PageX;
		public long PageZ;
    		
		//This is the tile Index in the Tile Array
		public long TileX;
		public long TileZ;

		//This is the spatial position of this Tile
		public float PosX;
		public float PosZ;
	}

}

