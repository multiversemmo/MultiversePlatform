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
#ifndef _OGREMAYA_MAT_H_
#define _OGREMAYA_MAT_H_

#include "OgreMayaCommon.h"

#include <maya/MFnMesh.h>

#include <fstream>

#include <string>
#include <list>
#include <vector>

namespace OgreMaya {
    
//    using namespace std;
	using std::string;
	using std::list;
	using std::vector;

    struct TextureUnitState {

        TextureUnitState(string textureName, int uvSet):
            textureName(textureName), uvSet(uvSet) {}

        string textureName;
        int uvSet;
    };

    typedef list<TextureUnitState> TextureLayerList;


    struct Material {        

        string name;
        string shadingMode;
        
        ColourValue ambient;
        ColourValue diffuse;
        ColourValue selfIllumination;
        ColourValue specular;

        Real shininess;
        
        TextureLayerList textureLayers;
    };

	//	===========================================================================
	/** \class		MatGenerator
		\author		John Van Vliet, Fifty1 Software Inc.
		\version	1.0
		\date		June 2003

		Generates an Ogre material file from a Maya scene.
	*/	
	//	===========================================================================
	class MatGenerator {
	public:

		/// Utility function for other classes
		/// \return		Name of the material attached to the given mesh
		static MString getMaterialName(MFnMesh &fnMesh);

		/// Standard constructor.
		MatGenerator();
		
		/// Destructor.
		virtual ~MatGenerator();

		/// Export the complete Maya scene (called by OgreMaya.mll or OgreMaya.exe).
		bool exportAll();

		/// Export selected parts of the Maya scene (called by OgreMaya.mll).
		bool exportSelection();

	protected:
        vector<Material*> materials;

		bool _extractMaterials();
		void _makeMaterials(MObject &ShaderSet);
		Material* _makePhongMaterial(MObject &ShaderNode);
		Material* _makeBlinnMaterial(MObject &ShaderNode);
		Material* _makeLambertMaterial(MObject &ShaderNode);
	};

} // namespace OgreMaya

#endif
