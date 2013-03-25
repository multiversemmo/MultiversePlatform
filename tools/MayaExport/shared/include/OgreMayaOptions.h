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
#ifndef _OGREMAYA_OPTIONS_H_
#define	_OGREMAYA_OPTIONS_H_

#include "OgreMayaCommon.h"

#include <maya/MString.h>
#include <maya/MStringArray.h>

#include <map>
#include <string>

namespace OgreMaya {

//    using namespace std;
	using std::map;
	using std::string;


	class Options {
	public:        
        static Options& Options::instance();

        void reset();
        void debugOutput();

    public:
        struct KeyframeRange {
            KeyframeRange(int from=0, int to=0, int step=1): from(from), to(to), step(1) {}
            bool isValid() {return from>0 && to>0;}
            int from;
            int to;
            int step;
        };
        
        typedef map<string, KeyframeRange>           KeyframeRangeMap;

        string
            inFile,
			inAnimFile,

            outMeshFile,
            outSkelFile,
			outAnimFile,
            outMatFile,

            matPrefix;

        bool
            verboseMode,

			exportSelected,

            exportMesh,
			exportSkeleton,
            exportVBA,
			exportNormals,
            exportColours,
            exportUVs,
            exportMaterial,

			exportBounds;

        KeyframeRangeMap
            animations;

        bool 
			valid;

		int
			precision;

		float
			scale;

	private:
        Options();
    };

} // namespace OgreMaya

#define OPTIONS Options::instance()

#endif