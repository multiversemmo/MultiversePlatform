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
#include "OgreMayaScene.h"
#include "OgreMayaOptions.h"

#include <maya/MLibrary.h>
#include <maya/MFileIO.h>
#include <maya/MStatus.h>

#ifdef _WIN32
#include <direct.h>
#endif

#include <iostream>

namespace OgreMaya {
	
	using namespace std;

	//	--------------------------------------------------------------------------
	/**	Standard constructor.
	*/	
	//	--------------------------------------------------------------------------
	SceneMgr::SceneMgr() {
		mbInitialized = false;			
	}


	//	--------------------------------------------------------------------------
	/**	Destructor. Cleans up Maya library if required.
	*/	
	//	--------------------------------------------------------------------------
	SceneMgr::~SceneMgr() {
		if (mbInitialized) {
			MLibrary::cleanup();
			mbInitialized = false;
		}
	}



	//	--------------------------------------------------------------------------
	/**	Loads a Maya scene file, initializing the Maya library first if required.
		\param		sFilename
					Path and filename of Maya scene file to load
		\return		True if file loaded successfully, False otherwise
	*/	
	//	--------------------------------------------------------------------------
	bool SceneMgr::load() {

        cout << "\nSceneMgr::load\n";

		MStatus status;

		// Store working directory to restore later
		char szDir[300];
#ifdef _WIN32
		_getcwd(szDir, 300);
#else
		getcwd(szDir, 300);
#endif

		// Initialize Maya if required
		if (!mbInitialized) {
			cout << "\tinitializing Maya...\n";

			status = MLibrary::initialize("Maya-to-Ogre", false);
			if (!status) {
				status.perror("MLibrary::initialize");
				return false;
			}
			mbInitialized = true;

			cout << "\tMaya initialized\n";
        }

		// Prepare Maya to read a new scene file
		status = MFileIO::newFile(true);
		if (!status) {
			cout << "\t[ERROR] MFileIO::newFile() failed\n";
			return false;
		}

		// Restore working directory
#ifdef _WIN32
		_chdir(szDir);
#else
		chdir(szDir);
#endif

		// Read the scene file
		status = MFileIO::open(OPTIONS.inFile.c_str());
		if (!status) {
			cout << "\t[ERROR] MFileIO::open() failed:\n";
			return false;
		}
		cout << "\tfile " << OPTIONS.inFile.c_str() << " opened\n";

		// Done
		return true;
	}

}
