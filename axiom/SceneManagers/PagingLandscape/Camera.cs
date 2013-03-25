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



//using Axiom.SceneManagers.IPLSceneManager.Page;

//using Axiom.SceneManagers.IPLSceneManager.Query;



#endregion Using Directives



#region Versioning Information

/// File								Revision

/// ===============================================

/// OgrePagingLandScapeCamera.h				1.1

/// OgrePagingLandScapeCamera.cpp			1.3

/// 

#endregion



namespace Axiom.SceneManagers.PagingLandscape

{

	/// <summary>

	/// Summary description for Camera.

	/// </summary>

	public class Camera: Core.Camera
	{
		/** Visibility types */
		public enum Visibility
		{
			None,
			Partial,
			Full
		};

		/* Standard Constructor */
		public Camera( string name, Axiom.Core.SceneManager creator ) : base(name, creator) {}

		/** Returns the visibility of the box
			*/
		public bool GetVisibility( AxisAlignedBox bound )
		{
			return this.IsObjectVisible(bound);
		}


	}

}

