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
#include <OgreMayaOptions.h>
#include <iostream>

namespace OgreMaya {

	using namespace std;    

    Options::Options() {
        reset();
    }

    void Options::reset() {
        valid          = false;

        verboseMode    = false;

		exportSelected = false;

        exportMesh     = false;
		exportSkeleton = false;
		exportVBA      = false;
		exportNormals  = false;
        exportColours  = false;
        exportUVs      = false;
        exportMaterial = false;

		exportBounds   = false;

        inFile         = "";
		inAnimFile     = "";
        outMeshFile    = "";
        outSkelFile    = "";
        outMatFile     = "";
        outAnimFile    = "";

        matPrefix      = "";

		precision      = 8;

		scale          = 1.0f;

        animations.clear();
    }


    Options& Options::instance() {
        static Options options;
        return options;
    }

    void Options::debugOutput() {
        cout << "=== options ================================\n";
        cout << inFile << " -> mesh=" << outMeshFile << ", skel=" << outSkelFile << "\n";
        cout << "Material: prefix=" << matPrefix << ", file=" << outMatFile << "\n";
        cout << "exportMesh     :" << exportMesh << '\n';
        cout << "exportSkeleton :" << exportSkeleton << '\n';
        cout << "exportNormals  :" << exportNormals << '\n';
        cout << "exportColours  :" << exportColours << '\n';
        cout << "exportUVs      :" << exportUVs << '\n';        
        cout << "exportMaterial :" << exportMaterial << '\n';   
		cout << "exportBounds   :" << exportBounds << '\n';   
		cout << "exportVBA      :" << exportVBA << '\n';   
        cout << "============================================\n";
    }
    
} // namespace OgreMaya
