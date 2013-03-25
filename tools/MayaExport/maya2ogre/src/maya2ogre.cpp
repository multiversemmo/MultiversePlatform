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
#include "OgreMayaOptions.h"

#include "OgreMayaScene.h"
#include "OgreMayaMesh.h"
#include "OgreMayaSkeleton.h"
#include "OgreMayaMaterial.h"

#include <maya/MDagPath.h>
#include <maya/MGlobal.h>
#include <maya/MPlug.h>
#include <maya/MFnSet.h>
#include <maya/MItDependencyGraph.h>
#include <maya/MItDag.h>

#include <iostream>

void showHelp();

using namespace OgreMaya;
using namespace std;

// ------------------------------------------------------------

class CommandLineParser {
public:
    CommandLineParser() {
        argv           = 0;
        argc           = 0;
        currentArg     = 0;


        // init parameter map (for command line args)
        builderMap["-in"     ] = &CommandLineParser::parseIn;
        builderMap["-mesh"   ] = &CommandLineParser::parseMeshOut;
        builderMap["-vba"    ] = &CommandLineParser::parseVBA;
        builderMap["-skel"   ] = &CommandLineParser::parseSkelOut;
        builderMap["-mat"    ] = &CommandLineParser::parseMatOut;
        builderMap["-mprefix"] = &CommandLineParser::parseMatPrefix;
        builderMap["-anim"   ] = &CommandLineParser::parseAnimation;
        builderMap["-n"      ] = &CommandLineParser::parseN;
        builderMap["-c"      ] = &CommandLineParser::parseC;
        builderMap["-t"      ] = &CommandLineParser::parseT;
        builderMap["-v"      ] = &CommandLineParser::parseV;
        builderMap["-p"      ] = &CommandLineParser::parseP;
        builderMap["-s"      ] = &CommandLineParser::parseS;
        builderMap["-afile"  ] = &CommandLineParser::parseAnimOut;
		builderMap["-inanim" ] = &CommandLineParser::parseInAnim;
		builderMap["-b"      ] = &CommandLineParser::parseB;
    }

    void parse(int argc, char** argv) {
        this->argv = argv;
        this->argc = argc;
        for(currentArg=1; currentArg<argc; currentArg++) {
            string arg = argv[currentArg];
            void (CommandLineParser::*p)(void) = builderMap[arg];

            if(p) {
                (this->*p)();
            }
        }
    }

    bool isNextTokenOption() {
        bool res = false;
        if(currentArg+1 < argc) {
            res = argv[currentArg+1][0] == '-';
        }

        return res;
    }

    void parseIn() {
        if(++currentArg < argc) {            
            OPTIONS.inFile = argv[currentArg];
            int i = OPTIONS.inFile.find_first_of('.');

            if(i>=0) {
                OPTIONS.outMeshFile = OPTIONS.inFile.substr(0, i) + ".mesh.xml";
                OPTIONS.outSkelFile = OPTIONS.inFile.substr(0, i) + ".skeleton.xml";
                OPTIONS.outMatFile  = OPTIONS.inFile.substr(0, i) + ".material";
            }
            else {
                OPTIONS.outMeshFile = OPTIONS.inFile + ".mesh.xml";
                OPTIONS.outSkelFile = OPTIONS.inFile + ".skeleton.xml";
                OPTIONS.outMatFile  = OPTIONS.inFile + ".material";
            }

            OPTIONS.valid = true;
        }
    }

    void parseMeshOut() {
        OPTIONS.exportMesh = true;
        if(!isNextTokenOption() && currentArg+1<argc) {
            OPTIONS.outMeshFile = argv[currentArg+1];
            currentArg++;
        }
    }

    void parseSkelOut() {
        OPTIONS.exportSkeleton = true;
        if(!isNextTokenOption() && currentArg+1<argc) {
            OPTIONS.outSkelFile = argv[currentArg+1];
            currentArg++;
        }
    }

    void parseMatOut() {
        OPTIONS.exportMaterial = true;
        if(!isNextTokenOption() && currentArg+1<argc) {
            OPTIONS.outMatFile = argv[currentArg+1];
            currentArg++;
        }
    }

    void parseMatPrefix() {
		if(!isNextTokenOption() && currentArg+1<argc) {
            OPTIONS.matPrefix = argv[currentArg+1];
			currentArg++;
        }
    }

    void parseAnimation() {
        if(currentArg+4 < argc) {
            string name = argv[currentArg+1];
            int from    = atoi(argv[currentArg+2]);
            int to      = atoi(argv[currentArg+3]);
            int step    = atoi(argv[currentArg+4]);

            OPTIONS.animations[name].from = from;
            OPTIONS.animations[name].to   = to;
            OPTIONS.animations[name].step = step;

            currentArg += 4;
        }
    }
    
    void parseVBA() {
        OPTIONS.exportVBA = true;
    }

    void parseN() {
        OPTIONS.exportNormals = true;
    }

    void parseC() {
        OPTIONS.exportColours = true;
    }

    void parseT() {
        OPTIONS.exportUVs = true;
    }
    
    void parseV() {
        OPTIONS.verboseMode = true;
    }

	void parseP() {
		if (currentArg + 1 < argc) {
            OPTIONS.precision = atoi(argv[currentArg + 1]);
			currentArg++;
		}
	}

	void parseS() {
		if (currentArg + 1 < argc) {
            OPTIONS.scale = atof(argv[currentArg + 1]);
			currentArg++;
		}
    }

    void parseAnimOut() {
		if (!isNextTokenOption() && currentArg+1<argc) {
            OPTIONS.outAnimFile = argv[currentArg+1];
			currentArg++;
        }
    }

	void parseInAnim() {
		if (!isNextTokenOption() && currentArg+1<argc) {
            OPTIONS.inAnimFile = argv[currentArg+1];
			currentArg++;
        }
	}

    void parseB() {
        OPTIONS.exportBounds = true;
    }


private:
	typedef map<string, void (CommandLineParser::*)(void)> BuilderMap;

	char** argv;
    int argc;
    int currentArg;

    BuilderMap builderMap;
};

// ------------------------------------------------------------

int main(int argc, char *argv[]) {

	// ===== Parse command line options
	CommandLineParser argParser;
	argParser.parse(argc, argv);
    
    if(!OPTIONS.valid) {
        showHelp();
        return -1;
    }

    OPTIONS.debugOutput();

    {
  	    SceneMgr          sceneMgr;
	    MeshGenerator     meshGen;
	    SkeletonGenerator skelGen;
        MatGenerator      matGen;

	    bool bStatus;

	    // ===== Initialize Maya and load scene	    
	    bStatus = sceneMgr.load();
	    if (!bStatus) {
		    cout << "\tFAILED\n";
			cout.flush();
		    return -2;
	    }
	    

        if(OPTIONS.verboseMode) {
            // ===== Iterate over mesh components of DAG               
            cout << "\n=== DAG Nodes ==============================\n";
            MItDag dagIter( MItDag::kBreadthFirst, MFn::kInvalid, 0 );
            for ( ; !dagIter.isDone(); dagIter.next()) {
                MDagPath dagPath;
                dagIter.getPath( dagPath );

                cout << "Node: "
                   << dagPath.fullPathName().asChar()
                   << "\n";
            }
            cout << "============================================\n";
			cout.flush();
        }
        


	    // ===== Export
  	    // --- Skeleton
	    if ((OPTIONS.exportSkeleton || OPTIONS.animations.size()) && !OPTIONS.exportMesh) {		    
		    bStatus = skelGen.exportAll();
		    if (!bStatus) {
			    cout << "\tFAILED\n";
				cout.flush();
			    return -3;
		    }
	    }

	    
	    // --- Mesh	    
	    if (OPTIONS.exportMesh) {			
			bStatus = meshGen.exportAll();
			if (!bStatus) {
				cout << "\tFAILED\n";
				cout.flush();
				return -4;
			}
		}


		// --- Material		

        if (OPTIONS.exportMaterial) {            
		    bStatus = matGen.exportAll();
		    if (!bStatus) {            
			    cout << "\tFAILED\n";
				cout.flush();
			    return -5;
		    }
        }  

    }
	cout.flush();

    return 1;
}

// ------------------------------------------------------------

void showHelp() {
    cout << "Version : "<<__DATE__<<" "<<__TIME__<<"\n";
    cout << "Maya API: "<<MAYA_API_VERSION<<"\n\n";
	cout << "Usage: maya2ogre -in FILE [-mesh [FILE]] [-vba] [-skel [FILE]]\n";
    cout << "                 [-anim NAME START END STEP]\n";
    cout << "                 [-mat [FILE]] [-mprefix PREFIX]\n";
    cout << "                 [-n] [-c] [-t] [-v]\n";
	cout << "                 [-p DIGITS] [-s SCALE] [-noskel]\n\n";
	cout << " -in      FILE   input mb File\n";
    cout << " -mesh    FILE   export mesh (FILE is optional)\n";
    cout << " -vba            export vertex bone assignments\n";    
    cout << " -skel    FILE   export skeleton (FILE is optional)\n";
    cout << " -anim    NAME   export Animation beginning at START and ending\n";
    cout << "          START  at END with fixed STEP\n";
    cout << "          END\n";
    cout << "          STEP\n";
    cout << " -mat     FILE   export material (FILE is optional)\n";
    cout << " -mprefix PREFIX material prefix\n";
    cout << " -n              export normals\n";
    cout << " -c              export diffuse colours\n";
    cout << " -t              export texture coords\n";    
    cout << " -v              more output\n";
    cout << " -p       DIGITS precision for numbers\n";
    cout << " -s       SCALE  scale model\n";
    cout << " -afile   FILE   export animations to FILE\n";
    cout << " -inanim  FILE   include animations from FILE\n";
    cout << " -b              export bounds information\n";
	cout << "\n";
    cout << "Examples:\n";
    cout << " maya2ogre -in foo.mb -mesh -skel -mat\n";
    cout << "     => exports skeleton, mesh and material using default file names,\n";
    cout << "        in this case foo.mesh.xml, foo.skeleton.xml and foo.material\n\n";
    cout << " maya2ogre -in foo.mb -mesh custom_name.mesh.xml -skel custom_name.skel.xml\n";
    cout << "     => exports skeleton and mesh using user defined file names\n\n";
    cout << " maya2ogre -in foo.mb -skel -anim Walk 1 30 2 -anim Die 50 60 2\n";
    cout << "     => exports skeleton with animation tracks Walk and Die\n";
}