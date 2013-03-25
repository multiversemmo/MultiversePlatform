/*
============================================================================
This source file is part of the Ogre-Maya Tools.
Distributed as part of Ogre (Object-oriented Graphics Rendering Engine).
Copyright (C) 2003 Fifty1 Software Inc., Bytelords

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
or go to http://www.gnu.org/licenses/gpl.txt
============================================================================
*/
#ifndef _OGREMAYA_SCENE_H_
#define _OGREMAYA_SCENE_H_

#include "OgreMayaCommon.h"

#include <maya/MString.h>

#include <string>

namespace OgreMaya {

//    using namespace std;	

	//	===========================================================================
	/** \class		SceneMgr
		\author		John Van Vliet, Fifty1 Software Inc.
		\version	1.0
		\date		June 2003

		Loads and unloads a Maya scene for non-plugin exporters. Initializes the
		Maya library if required on the first load, and cleans up the Maya library
		in the destructor.
	*/	
	//	===========================================================================
	class SceneMgr {
	public:

		/// Constructor.
		SceneMgr();
		
		/// Destructor.
		virtual ~SceneMgr();
		
		/// Load the Maya scene file defined in the options (initializes Maya library first if required)
		bool load();


	protected:
		bool	      mbInitialized;
	};
}

#endif
