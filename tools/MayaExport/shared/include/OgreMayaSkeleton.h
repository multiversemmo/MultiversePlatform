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
#ifndef _OGREMAYA_SKELETON_H_
#define _OGREMAYA_SKELETON_H_

#include "OgreMayaCommon.h"

#include <maya/MGlobal.h>
#include <maya/MDagPath.h>
#include <maya/MFloatArray.h>
#include <maya/MPointArray.h>
#include <maya/MFloatVectorArray.h>
#include <maya/MColorArray.h>
#include <maya/MObjectArray.h>
#include <maya/MFnMesh.h>
#include <maya/MStatus.h>
#include <maya/MItMeshPolygon.h>
#include <maya/MMatrix.h>
#include <maya/MQuaternion.h>
#include <maya/MFnTransform.h>
#include <maya/MVector.h>

#include <fstream>

#include <string>
#include <list>
#include <vector>
#include <map>

namespace OgreMaya {

//    using namespace std;
	using std::list;
	using std::vector;
	using std::map;
	using std::string;

	// Debug methods
	void printMQuaternion(MQuaternion const& q);
	void printMVector(MVector const& v);
	void printMMatrix(MMatrix const& m);

    struct Keyframe {
        Keyframe(Real time, MVector pos, MQuaternion rot):
            time(time), pos(pos), rot(rot) {}

        Real time; // in s
        MVector pos;
        MQuaternion rot;
    };
    
    typedef list<Keyframe> KeyframeList;    
    typedef map<string, KeyframeList> KeyframesMap;

    struct Animation {
        Animation(): time(0) {}
        float time;

        KeyframesMap keyframes; // bonename -> keyframelist
    };

    typedef map<string, Animation> AnimationMap;
    

	//	===========================================================================
	/** \struct		SkeletonJoint
		stores joint data
	*/	
	//	===========================================================================
	struct SkeletonJoint {
        MDagPath       dagPath;

        int            logical_index;
		int            bone_index;

        std::string    name;
        std::string    parentName;
        bool           hasParent;

		// This is a bit of a misnomer, since it is really
		// based on the bind matrix, rather than a true world
		// transform matrix.
		// Tranform relative to the world - unscaled
        MMatrix        worldMatrix;
        MMatrix        invWorldMatrix;

		// Transform relative to parent - unscaled
        MMatrix        localMatrix;
        MMatrix        invLocalMatrix;

        SkeletonJoint* parent;

		// Translation relative to parent object - in scaled space.
        MVector        relPos;
        MQuaternion    relRot;
    };

	typedef vector<SkeletonJoint*> SkeletonJointList;

	struct IndexLess : std::binary_function<SkeletonJoint *, SkeletonJoint *, bool> {
		bool operator()(const SkeletonJoint *psj1, const SkeletonJoint *psj2) const
		{
			return psj1->bone_index < psj2->bone_index;
		}
	};

	//	===========================================================================
	/** \class		SkeletonGenerator
		\author		Ivica "macross" Aracic, Bytelords
		\version	1.0
		\date		August 2003

		Generates an Ogre skeleton from a Maya scene. 
	*/	
	//	===========================================================================
	class SkeletonGenerator {
	public:

		/// Standard constructor.
		SkeletonGenerator();
		
		/// Destructor.
		virtual ~SkeletonGenerator();

		/// Export the complete Maya scene (called by OgreMaya.mll or OgreMaya.exe).
		virtual bool exportAll();

		/// Export selected parts of the Maya scene (called by OgreMaya.mll).
		virtual bool exportSelection();

        int getJointCount() {
            return jointList.size();
        }


	protected:
        bool _querySkeleton();
        bool _querySkeletonAnim();
		bool _exportSkeleton();
		bool _exportAnimations();
		bool _exportAnimations(std::ostream &out);
		bool _queryJoint(SkeletonJoint *pkJoint, MDagPath &kDagPath);

		SkeletonJoint* _getSkeletonJoint(int boneId) const;
		SkeletonJoint* _getSkeletonJoint(const std::string &jointName) const;
		SkeletonJoint* _getSkeletonJoint(const MDagPath &dagPath) const;

		static MMatrix SkeletonGenerator::_getTransformTree(const SkeletonJoint *j) {
			if (j->hasParent)
				return j->localMatrix * _getTransformTree(j->parent);
			return j->localMatrix;
		}

	protected:
        SkeletonJointList jointList;
		SkeletonJoint* root;
        AnimationMap animations;
		MDagPath rootDagPath;
	};

} // namespace OgreMaya

#endif
