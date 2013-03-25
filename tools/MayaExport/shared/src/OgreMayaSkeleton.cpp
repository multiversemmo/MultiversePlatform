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
#include "OgreMayaSkeleton.h"
#include "OgreMayaOptions.h"

#include <maya/MString.h>
#include <maya/MArgList.h>
#include <maya/MAnimControl.h>

#include <maya/MFnMesh.h>
#include <maya/MFnAttribute.h>
#include <maya/MFnIkJoint.h>
#include <maya/MFnDagNode.h>
#include <maya/MFnSkinCluster.h>
#include <maya/MFnMatrixData.h>
#include <maya/MFnSet.h>
#include <maya/MFnLambertShader.h>
#include <maya/MFnBlinnShader.h>
#include <maya/MFnPhongShader.h>

#include <maya/MItGeometry.h>
#include <maya/MItDag.h>
#include <maya/MItDependencyGraph.h>
#include <maya/MItDependencyNodes.h>
#include <maya/MItMeshVertex.h>
#include <maya/MItMeshPolygon.h>

#include <maya/MPlug.h>
#include <maya/MDagPathArray.h>
#include <maya/MFloatPointArray.h>
#include <maya/MFloatVectorArray.h>
#include <maya/MFloatArray.h>
#include <maya/MPointArray.h>
#include <maya/MMatrix.h>
#include <maya/MEulerRotation.h>
#include <maya/MGlobal.h>
#include <maya/MStatus.h>

#include <iostream>
#include <algorithm>

#include <math.h>
#define M_PI       3.14159265358979323846

namespace OgreMaya {
	
	using namespace std;

	MVector getTranslation(const MMatrix &mTransform) {
		// Note that Maya matrices are transposed versions of the 
		// Foley et al. notation, so, for example, translation in 
		// the x directions would go in the bottom left of a 4x4 matrix.
		MVector vTranslate;
		vTranslate.x = mTransform(3, 0);
		vTranslate.y = mTransform(3, 1);
		vTranslate.z = mTransform(3, 2);
		return vTranslate;
	}

	MQuaternion getRotation(const MMatrix &mTransform) {
		MQuaternion rRotate;
		rRotate = mTransform;
		return rRotate;
	}
	
	void printRotation(const MEulerRotation &eulerRot) {
		cout.setf(ios::showpos | ios::fixed);
		cout.precision(5);
		float radToDeg = 180.0f / 3.14159f;
		MEulerRotation eulerRot1 = eulerRot.reorder(MEulerRotation::kXYZ);
		cout << "euler1: (" << eulerRot1.x * radToDeg << ", " << eulerRot1.y * radToDeg << ", " << eulerRot1.z * radToDeg << ")" << endl;
		MEulerRotation eulerRot2 = eulerRot.alternateSolution();
		cout << "euler2: (" << eulerRot2.x * radToDeg << ", " << eulerRot2.y * radToDeg << ", " << eulerRot2.z * radToDeg << ")" << endl;
	}

	void printRotation(const MQuaternion &rRot) {
		cout.setf(ios::showpos | ios::fixed);
		cout.precision(5);

		cout << "quat: (" << rRot.w << ", " << rRot.x << ", " << rRot.y << ", " << rRot.z << ")" << endl;
		printRotation(rRot.asEulerRotation());
	}

	void printRotation(const MMatrix &mRot) {
		cout.setf(ios::showpos | ios::fixed);
		cout.precision(5);
		MQuaternion rRot = getRotation(mRot);
		printRotation(rRot.asEulerRotation());
	}

	void printMQuaternion(MQuaternion const& q) {
		printRotation(q);
	}

	void printMVector(MVector const& v) {
		cout.setf(ios::showpos | ios::fixed);
		cout.precision(5);
		cout << "("<<v[0]<<", "<<v[1]<<", "<<v[2]<<")" << endl;
	}

	void printMMatrix(MMatrix const& m) {
		cout.setf(ios::showpos | ios::fixed);
		cout.precision(5);
		for (int i = 0; i < 4; ++i) {
			cout << "(";
			for (int j = 0; j < 4; ++j) {
				cout << m(i, j);
				if (j != 3)
					cout <<", ";
			}
			cout << ")" << endl;
		}
		cout << "----------------------------" << endl;
	}

#ifdef TESTING
	// The engine expects T * S * R, where matrices are evaluated from 
	// right to left.
	void getTransform(MQuaternion &rv_rRot, MVector &rv_vTrans, const MFnTransform &fnTrans) {
		MStatus status;
		MPlug plug;
		double xx, yy, zz;
		int order;
		// float radToDeg = 180.0f / 3.14159f;
		plug = fnTrans.findPlug("rotateX", &status);
		plug.getValue(xx);
		plug = fnTrans.findPlug("rotateY", &status);
		plug.getValue(yy);
		plug = fnTrans.findPlug("rotateZ", &status);
		plug.getValue(zz);
		plug = fnTrans.findPlug("rotateOrder", &status);
		plug.getValue(order);
		MEulerRotation rotation(xx, yy, zz, (MEulerRotation::RotationOrder)order);
		
		cout << "Euler rotation based on rotation info:" << endl;
		printRotation(rotation);

		MQuaternion quat = rotation.asQuaternion();
		cout << "Quaternion based on rotation info" << endl;
		printMQuaternion(quat);

		MMatrix mRot = rotation.alternateSolution().asMatrix();
		plug = fnTrans.findPlug("translateX", &status);
		plug.getValue(xx);
		plug = fnTrans.findPlug("translateY", &status);
		plug.getValue(yy);
		plug = fnTrans.findPlug("translateZ", &status);
		plug.getValue(zz);
		MMatrix mTrans = MMatrix::identity;
		mTrans(3, 0) = xx;
		mTrans(3, 1) = yy;
		mTrans(3, 2) = zz;

		rv_rRot = getRotation(mRot); // getRotation(tmp);
		rv_vTrans = getTranslation(mTrans); // getTranslation(tmp);

		MMatrix tmp = mRot * mTrans;

		cout << "Quaternion from mRot * mTrans: " << endl;
		printMQuaternion(rv_rRot);

		cout << "R * T:" << endl;
		printMMatrix(mRot * mTrans);

		cout << "T * R:" << endl;
		printMMatrix(mTrans * mRot);

		cout << "fnTrans matrix" << endl;
		MMatrix worldMatrix = fnTrans.transformation().asMatrix();
		printMMatrix(worldMatrix);

		MQuaternion kRot;
		kRot = worldMatrix;
		cout << "Quaternion from world matrix: " << endl;
		printMQuaternion(kRot);
	}
#endif

	//	--------------------------------------------------------------------------
	/** Standard constructor. Creates Ogre Mesh and defines known options.
	*/	
	//	--------------------------------------------------------------------------
	SkeletonGenerator::SkeletonGenerator() {
	}


	//	--------------------------------------------------------------------------
	/** Destructor.
	*/	
	//	--------------------------------------------------------------------------
	SkeletonGenerator::~SkeletonGenerator()
	{
		deleteAll(jointList.begin(), jointList.end());
	}

	bool SkeletonGenerator::exportSelection() {
		return exportAll();
	}

	//	--------------------------------------------------------------------------
	/** Find and export all joints

		\return		True if exported ok, false otherwise
	*/	
	//	--------------------------------------------------------------------------
	bool SkeletonGenerator::exportAll() {
		// Reset the joint list
		deleteAll(jointList.begin(), jointList.end());
		jointList.clear();

		if (!_querySkeleton())
			return false;

		if (!_querySkeletonAnim())
			return false;
		
		if (OPTIONS.exportSkeleton)
			if (!_exportSkeleton())
				return false;
		
		if (OPTIONS.outAnimFile != "")
			if (!_exportAnimations())
				return false;
		
		cout << "Exported data" << endl;
		return true;
	}

	bool SkeletonGenerator::_exportAnimations() {
		ofstream out(OPTIONS.outAnimFile.c_str());

		out.precision(OPTIONS.precision);
		out.setf(ios::fixed);
		return _exportAnimations(out);
	}


	bool SkeletonGenerator::_exportSkeleton() {
		ofstream out(OPTIONS.outSkelFile.c_str());

		out.precision(OPTIONS.precision);
		out.setf(ios::fixed);

		out << "<skeleton>\n";

		SkeletonJointList::iterator it, end;

		// BONES
		out << "\t<bones>\n";
		for (it=jointList.begin(), end=jointList.end(); it!=end; ++it) {
			SkeletonJoint& j = **it;
			MVector axis;
			double angle;
			j.relRot.getAxisAngle(axis, angle);
			out << "\t\t<bone id=\""<<j.bone_index<<"\" name=\""<<j.name<<"\">\n";
			out << "\t\t\t<position x=\""<<j.relPos.x<<"\" y=\""<<j.relPos.y<<"\" z=\""<<j.relPos.z<<"\"/>\n";
			out << "\t\t\t<rotation angle=\""<<((float)angle)<<"\">\n";
			out << "\t\t\t\t<axis x=\""<<axis.x<<"\" y=\""<<axis.y<<"\" z=\""<<axis.z<<"\"/>\n";
			out << "\t\t\t</rotation>\n";
			// Debugging code
			// out << "\t\t\t";
			// outputEulerRotation(out, j.relRot);

			out << "\t\t</bone>\n";
		}
		out << "\t</bones>\n";

		// HIERARCHY
		out << "\t<bonehierarchy>\n";
		for (it=jointList.begin(), end=jointList.end(); it!=end; ++it) {
			SkeletonJoint &j = **it;
			if (j.hasParent)
				out << "\t\t<boneparent bone=\""<<j.name<<"\" parent=\""<<j.parentName<<"\"/>\n";
		}
		out << "\t</bonehierarchy>\n";

		// ANIMATIONS
		out << "\t<animations>\n";

		// Export animations from this model
		_exportAnimations(out);

		// Import animations from another file
		if (OPTIONS.inAnimFile != "") {
			cout << "Importing animations from " << OPTIONS.inAnimFile << endl;
			ifstream inAnim(OPTIONS.inAnimFile.c_str());
			while (inAnim.good()) {
				char buf[1024];
				inAnim.read(buf, sizeof(buf));
				out.write(buf, inAnim.gcount());
			}
		}

		out << "\t</animations>\n";

		out << "</skeleton>\n";

		return true;
	}

	bool SkeletonGenerator::_exportAnimations(ostream &out) {
		AnimationMap::iterator animIt = animations.begin();
		AnimationMap::iterator animEnd = animations.end();
		for(; animIt!=animEnd; ++animIt) {
			string animName = (*animIt).first;
			Animation& anim = (*animIt).second;

			out << "\t\t<animation name=\""<<animName.c_str()<<"\" ";
			out << "length=\""<<anim.time<<"\">\n";
			out << "\t\t\t<tracks>\n";

			KeyframesMap::iterator keyframesIt = anim.keyframes.begin();
			KeyframesMap::iterator keyframesEnd = anim.keyframes.end();
			for (; keyframesIt!=keyframesEnd; ++keyframesIt) {
				string boneName = (*keyframesIt).first;
				KeyframeList& l = (*keyframesIt).second;

				out << "\t\t\t\t<track bone=\""<<boneName.c_str()<<"\">\n";
				out << "\t\t\t\t\t<keyframes>\n";

				KeyframeList::iterator it  = l.begin();
				KeyframeList::iterator end = l.end();
				double last_angle = 0;
				for( ;it != end; ++it) {
					Keyframe& k = *it;

					MVector axis;
					double angle;
					k.rot.getAxisAngle(axis, angle);

					// Ensure that angle is in the range of 0 to 2PI
					while (angle >= 2 * M_PI)
						angle -= 2 * M_PI;
					while (angle < 0)
						angle += 2 * M_PI;

					// Since we will interpolate between frames, ensure that
					// the rotation is the smaller version.
					double tmp_angle = 2 * M_PI - angle;

					if (abs(tmp_angle - last_angle) < abs(angle - last_angle)) {
						angle = tmp_angle;
						axis = -1 * axis;
					}
					last_angle = angle;

					out << "\t\t\t\t\t\t<keyframe time=\""<<k.time<<"\">\n";                        
					out << "\t\t\t\t\t\t\t<translate x=\""<<k.pos.x<<"\" y=\""<<k.pos.y<<"\" z=\""<<k.pos.z<<"\"/>\n";
					out << "\t\t\t\t\t\t\t<rotate angle=\""<<((float)angle)<<"\">\n";
					out << "\t\t\t\t\t\t\t\t<axis x=\""<<axis.x<<"\" y=\""<<axis.y<<"\" z=\""<<axis.z<<"\"/>\n";
					out << "\t\t\t\t\t\t\t</rotate>\n";

					out << "\t\t\t\t\t\t</keyframe>\n";
				}

				out << "\t\t\t\t\t</keyframes>\n";
				out << "\t\t\t\t</track>\n";
			}

			out << "\t\t\t</tracks>\n";
			out << "\t\t</animation>\n";
		}
		return true;
	}

	bool SkeletonGenerator::_queryJoint(SkeletonJoint *pkJoint, MDagPath &kDagPath) {
		MObject jointNode = kDagPath.node();
		MFnIkJoint kJointFn(jointNode);
		MStatus kStatus;

		pkJoint->dagPath = kDagPath;
		pkJoint->name = kJointFn.partialPathName().asChar();
		pkJoint->logical_index = -1;
		pkJoint->bone_index = -1;

		unsigned int uiNumParents = kJointFn.parentCount();
		// can only have one parent
		if (uiNumParents != 1) {
			cout << "\t[ERROR] joint has " << uiNumParents << " parents (only 1 allowed)" << '\n';
			return false;
		}

		MObject kParentObj = kJointFn.parent(0);
		if (kParentObj.hasFn(MFn::kJoint)) {
			MFnIkJoint kParentJointFn(kParentObj); 
			pkJoint->parentName = kParentJointFn.partialPathName().asChar();
			pkJoint->hasParent = true;
		} else {
			pkJoint->parentName = "";
			pkJoint->hasParent = false;
			if (kParentObj.hasFn(MFn::kDagNode)) {
				MFnDagNode dagNode(kParentObj);
				dagNode.getPath(rootDagPath);
			} else {
				cout << "\t[WARNING] parent object for " << pkJoint->name << " is a " << kParentObj.apiTypeStr() << endl;
			}
		}

		// Get bindpose/world matrix info for joint

		// Look up our joint bone index and world transform matrix info.
		// We will look at the joints' 'bindPose' attribute, and then 
		// look at the other end of those connections.  If the object on
		// the other end is a kDagPose object (which it should be, since
		// it is attached to our bindPose attribute) and the attribute on
		// that side is a 'worldMatrix', find the logicalIndex of that
		// entry.  This gives you the index to your bone that will be
		// meaningful for skinning.

		MObject aBindPose = kJointFn.attribute("bindPose", &kStatus);
		if (kStatus != MStatus::kSuccess) {
			cout << "\t[ERROR]  Failed to get bindPose attribute for joint\n";
			return false;
		}
		MPlug pBindPose(jointNode, aBindPose);
		MPlugArray connPlugs;
		pBindPose.connectedTo(connPlugs, false, true);
		unsigned connLength = connPlugs.length();
		if (connLength == 0) {
			cout << "\t[INFO]  No connection to bindPose for joint object: " << kJointFn.name() << endl;
			return false;
		}
		for (unsigned i = 0; i < connLength; ++i) {
			// cout << "connPlugs[i].info: " << connPlugs[i].info() << endl;
			if (!connPlugs[i].node().hasFn(MFn::kDagPose))
				continue;
			MObject aMember = connPlugs[i].attribute();
			MFnAttribute fnAttr(aMember);
			if (fnAttr.name() != "worldMatrix")
				continue;
			// get and print the world matrix data
			MObject worldMatrix;
			kStatus = connPlugs[i].getValue(worldMatrix);
			if (kStatus != MStatus::kSuccess) {
				cout << "\t[ERROR]  Problem retrieving world matrix.\n";
				return false;
			}
			MFnMatrixData kMatrixDataFn(worldMatrix);
			MMatrix kBindMatrix = kMatrixDataFn.matrix(&kStatus);
			if (kStatus != MStatus::kSuccess) {
				cout << "\t[ERROR]  Error getting world matrix data.";
				return false;
			}
			pkJoint->logical_index = connPlugs[i].logicalIndex();
			pkJoint->worldMatrix = kBindMatrix;
			pkJoint->invWorldMatrix = kBindMatrix.inverse();
			return true;
		}
		return false;
	}

	//	-------------------------------------------------------------------------
	/** Finds and exports all joints

		\return		True if exported ok, false otherwise
	*/	
	//	--------------------------------------------------------------------------

	bool SkeletonGenerator::_querySkeleton() {
		cout << "\nSkeletonGenerator::_querySkeleton\n";
		jointList.clear();

		MItDag kDagIt(MItDag::kDepthFirst, MFn::kJoint);
		MDagPath kRootPath;
		MStatus kStatus;

		kDagIt.getPath(kRootPath);

		// check if valid path
		if (!kRootPath.isValid()) {
			cout << "\tcan not find parent joint\n"; 
			return false;
		} else {
			cout << "\tfound parent joint \""<<kRootPath.partialPathName().asChar()<<"\"\n"; 
		}

		//Setup skeleton
		cout << "\tsetup skeleton\n";
		int uiNumJoints = 0;

		for (; !kDagIt.isDone(); kDagIt.next(), ++uiNumJoints) {
			MDagPath kDagPath;
			kDagIt.getPath(kDagPath);

			SkeletonJoint *pkJoint = new SkeletonJoint();
			if (!_queryJoint(pkJoint, kDagPath)) {
				delete pkJoint;
				continue;
			}
			if (!pkJoint->hasParent)
				// we've found root here -> mark
				root = pkJoint;

			if (pkJoint->logical_index == -1) {
				cout << "\t[WARN] failed to get worldMatrix attribute for " << kDagPath.partialPathName() << endl;
				delete pkJoint;
				continue;
			}
			pkJoint->bone_index = jointList.size();
			jointList.push_back(pkJoint);
		}

		// Build parenting relationships and relative/local transforms

		// Calculate relative position and rotation data
		// cout << "\tcalculate relative position and rotation data" << endl;

		SkeletonJointList::iterator jointIt = jointList.begin();
		SkeletonJointList::iterator jointEnd = jointList.end();

		for (; jointIt != jointEnd; ++jointIt) {
			SkeletonJoint *j = *jointIt;
			// search for parent node, and build parenting links based on
			// names of the joints.
			if (j->hasParent)
				j->parent = _getSkeletonJoint(j->parentName);

			if (j->hasParent)
				j->localMatrix = j->worldMatrix * j->parent->invWorldMatrix;
			else
				j->localMatrix = j->worldMatrix;

			j->invLocalMatrix = j->localMatrix.inverse();

			// DEBUG
			// if (!j->hasParent) {
			//	printMVector(getTranslation(j->localMatrix));
			//	printMQuaternion(getRotation(j->localMatrix));
			// }

			j->relRot = getRotation(j->localMatrix);
			j->relPos = OPTIONS.scale * getTranslation(j->localMatrix);
		}

		IndexLess indexCompare;
		std::sort(jointList.begin(), jointList.end(), indexCompare);
		// ===== Done
		return true;
	}

	// Find the SkeletonJoint object whose bone_index matches this boneId
	SkeletonJoint *SkeletonGenerator::_getSkeletonJoint(int boneId) const {
		SkeletonJointList::const_iterator jointIt = jointList.begin();
		SkeletonJointList::const_iterator jointEnd = jointList.end();

		for (; jointIt != jointEnd; ++jointIt) {
			SkeletonJoint *j = *jointIt;
			if (j->logical_index == boneId)
				return j;
		}
		return NULL;
	}

	SkeletonJoint *SkeletonGenerator::_getSkeletonJoint(const std::string &jointName) const {
		SkeletonJointList::const_iterator jointIt = jointList.begin();
		SkeletonJointList::const_iterator jointEnd = jointList.end();

		for (; jointIt != jointEnd; ++jointIt) {
			SkeletonJoint *j = *jointIt;
			if (j->name == jointName)
				return j;
		}
		return NULL;
	}

	SkeletonJoint *SkeletonGenerator::_getSkeletonJoint(const MDagPath &dagPath) const {
		SkeletonJointList::const_iterator jointIt = jointList.begin();
		SkeletonJointList::const_iterator jointEnd = jointList.end();

		for (; jointIt != jointEnd; ++jointIt) {
			SkeletonJoint *j = *jointIt;
			if (j->dagPath == dagPath)
				return j;
		}
		return NULL;
	}

	bool SkeletonGenerator::_querySkeletonAnim() {

		cout << "\nSkeletonGenerator::_querySkeletonAnim\n";

		animations.clear();

	    MTime kTimeMin   = MAnimControl::minTime();
	    MTime kTimeMax   = MAnimControl::maxTime();
	    MTime kTimeTotal = kTimeMax - kTimeMin;
	    float fLength    = (float)kTimeTotal.as(MTime::kSeconds);
	    int iTimeMin     = (int)kTimeMin.value();
	    int iTimeMax     = (int)kTimeMax.value();
	    int iFrames      = (iTimeMax-iTimeMin)+1;
        float secondsPerFrame = fLength / (float)iFrames;

		MAnimControl kAnimControl;

	    cout << "\tanimation start: " << iTimeMin << " end: " << iTimeMax << '\n';
	    if( iFrames < 1 )
		    return false;

        Options::KeyframeRangeMap& m = OPTIONS.animations;
        Options::KeyframeRangeMap::iterator it  = m.begin();
        Options::KeyframeRangeMap::iterator end = m.end();

        for (; it!=end; ++it) {
            string animationName = (*it).first;
            int from    = (*it).second.from;
            int to      = (*it).second.to;
            int step    = (*it).second.step;
            int frameCount = to - from + 1;
            
            if (from < iTimeMin || to > iTimeMax || !(frameCount>0)) {
                cout << "\t[WARNING] Adjusting Animation Range\n";
				if (from < iTimeMin)
					from = iTimeMin;
				if (to > iTimeMax)
					to = iTimeMax;
				frameCount = to - from + 1;
            }

            Animation& anim = animations[animationName];

            anim.time = (float)(frameCount)*secondsPerFrame;            

			SkeletonJointList::iterator jointIt = jointList.begin();
			SkeletonJointList::iterator jointEnd = jointList.end();

			for (; jointIt != jointEnd; ++jointIt) {
				SkeletonJoint *j = *jointIt;
                MTime kFrame = kTimeMin;

				for (int iFrame=0; iFrame<frameCount; iFrame+=step, kFrame+=step) {

					MStatus status;
					status = kAnimControl.setCurrentTime(kFrame);
					if (status != MStatus::kSuccess)
						cout << "\t[WARNING] Failed to set animation time\n";
					MMatrix kIncMat = j->dagPath.inclusiveMatrix(&status);
					if (status != MStatus::kSuccess)
						cout << "\t[WARNING] Failed to fetch inclusive matrix\n";
					MMatrix kExcMat = j->dagPath.exclusiveMatrix(&status);
					if (status != MStatus::kSuccess)
						cout << "\t[WARNING] Failed to fetch exclusive matrix\n";
					MMatrix kExcInvMat = j->dagPath.exclusiveMatrixInverse(&status);
					if (status != MStatus::kSuccess)
						cout << "\t[WARNING] Failed to fetch exclusive inverse matrix\n";

					MMatrix kLocalMat;

					// I think kExcInvMat is always the identity matrix, but just in case
					if (!j->hasParent) {
						// root has to be handled differently because when 
						// exporting root bone to ogre, we remove all maya 
						// parents.  since we don't generally have any parents
						// for the root bone, this should rarely be an issue,
						// (kExcInvMat is the identity matrix), but just in case
						// This effectively folds the root bone's parent 
						// transforms into the root bone.
						kLocalMat = kIncMat;
					} else {
						kLocalMat = kIncMat * kExcInvMat; //  * j->invLocalMatrix;
					}

					MQuaternion	kJointInverseRot = j->relRot.inverse();
					MVector kJointInverseTrans = -1 * j->relPos;

					MQuaternion kLocalRot = getRotation(kLocalMat);
					MVector kLocalTrans = OPTIONS.scale * getTranslation(kLocalMat);

					MQuaternion kRotation = kLocalRot * kJointInverseRot;
					MVector kTranslation = kLocalTrans + kJointInverseTrans; 

#ifdef TESTING
					// Translation = kTranslation - j->relPos;

						if (!j->hasParent) {
						MFnTransform fnTrans(j->dagPath);
						getTransform(kRotation, kTranslation, fnTrans);
						MQuaternion kWorldRot, kJointInverseRot, kJointRot;
						
						kWorldRot = fnTrans.transformationMatrix();
						kJointRot = j->relRot;
						kJointInverseRot = j->relRot.inverse();

						// The world rotation (worldMatrix) is equivalent to
						// the product of the rotation from the rotate 
						// attributes of this node at this time and the
						// rotation of the joint at bind position.

						cout << "kRotation * kJointRot" << endl;
						printRotation(kRotation * j->relRot);

						// In right to left order
						// Rw = Ra * Rj

						// Perhaps the translation should then be the sum of
						// the translation from the translate attributes of 
						// this node at this time and the translation of the
						// joint at bind position.
						kTranslation = OPTIONS.scale * kTranslation;
						kTranslation = kTranslation - j->relPos;
					}

#endif
#ifdef OLD_STYLE

					// root has to be handled differently because when 
					// exporting root bone to ogre, we remove all maya 
					// parents.  since we don't generally have any parents
					// for the root bone, this should rarely be an issue,
					// (kExcInvMat is the identity matrix), but just in case
					if (!j->hasParent) {
						kLocalMat = kIncMat * j->invLocalMatrix;

						// It isn't really clear to me why this is different
						kTranslation.x = OPTIONS.scale * (kIncMat(3, 0) - kExcMat(3, 0));
						kTranslation.y = OPTIONS.scale * (kIncMat(3, 1) - kExcMat(3, 1));
						kTranslation.z = OPTIONS.scale * (kIncMat(3, 2) - kExcMat(3, 2));

						kRotation = kLocalMat;
					}
#endif

					float timePos = (float)iFrame * secondsPerFrame;

					anim.keyframes[j->name].push_back(
						Keyframe(timePos, kTranslation, kRotation)
					);
				}
			}
		}

		return true;
	}

} // namespace OgreMaya
