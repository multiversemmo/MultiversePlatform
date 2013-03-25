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

using System;
using Axiom.Core;

namespace Axiom.SceneManagers.Bsp
{
	/// <summary>
	///		Manages the locating and loading of BSP-based indoor levels.
	/// </summary>
	/// <remarks>
	///		Like other ResourceManager specialisations it manages the location and loading
	///		of a specific type of resource, in this case files containing Binary
	///		Space Partition (BSP) based level files e.g. Quake3 levels.</p>
	///		However, note that unlike other ResourceManager implementations,
	///		only 1 BspLevel resource is allowed to be loaded at one time. Loading
	///		another automatically unloads the currently loaded level if any.
	/// </remarks>
	public class BspResourceManager : ResourceManager 
	{
		#region Singleton implementation
		protected static BspResourceManager instance;

		public static BspResourceManager Instance 
		{
			get { return instance; }
		}

		static BspResourceManager() 
		{ 
			instance = new BspResourceManager();
		}

		protected BspResourceManager() 
		{ 
		}
        #endregion

		#region Protected members
		protected Quake3ShaderManager shaderManager;
		#endregion

		#region Methods
		/// <summary>
		///		Loads a BSP-based level from the named file.  Currently only supports loading of Quake3 .bsp files.
		/// </summary>
		public BspLevel Load(string fileName)
		{
			return Load(fileName, 1);
		}

		/// <summary>
		///		Loads a BSP-based level from the named file.  Currently only supports loading of Quake3 .bsp files.
		/// </summary>
		public BspLevel Load(string fileName, int priority)
		{
			// TODO: Bleh?!
			// UnloadAndDestroyAll();

			BspLevel bsp = (BspLevel) Create(fileName);
			base.Load(bsp, priority);

			return bsp;
		}
		
		/// <summary>
		///		Creates a BspLevel resource - mainly used internally.
		/// </summary>
		public override Resource Create(string name)
		{
			return new BspLevel(name);
		}
		#endregion
	}
}