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

namespace Axiom.Graphics {
	/// <summary>
    ///     Interface representing a 'licensee' of a hardware buffer copy.
	/// </summary>
	/// <remarks>
	///     Often it's useful to have temporary buffers which are used for working
	///     but are not necessarily needed permanently. However, creating and 
	///     destroying buffers is expensive, so we need a way to share these 
	///     working areas, especially those based on existing fixed buffers. 
	///     Classes implementing this interface represent a licensee of one of those 
	///     temporary buffers, and must be implemented by any user of a temporary buffer 
	///     if they wish to be notified when the license is expired. 
	/// </remarks>
	public interface IHardwareBufferLicensee {
        #region Methods

        /// <summary>
        ///     This method is called when the buffer license is expired and is about
        ///     to be returned to the shared pool.
        /// </summary>
        /// <param name="buffer"></param>
        void LicenseExpired(HardwareBuffer buffer);

        #endregion Methods
	}
}
