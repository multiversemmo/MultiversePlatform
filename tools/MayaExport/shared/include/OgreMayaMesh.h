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
#ifndef _OGREMAYA_MESH_H_
#define _OGREMAYA_MESH_H_

#include "OgreMayaCommon.h"
#include "OgreMayaSkeleton.h"

#include <maya/MGlobal.h>
#include <maya/MFloatArray.h>
#include <maya/MPointArray.h>
#include <maya/MFloatVectorArray.h>
#include <maya/MColorArray.h>
#include <maya/MObjectArray.h>
#include <maya/MFnMesh.h>
#include <maya/MStatus.h>
#include <maya/MItMeshPolygon.h>

#include <fstream>

#include <string>
#include <list>
#include <vector>

namespace OgreMaya {

	using std::ofstream;
	using std::list;
	using std::string;
	using std::vector;

	//	===========================================================================
	/** \struct		MeshUV
		Simple structure with single set of UV coordinates for a single vertex.
	*/	
	//	===========================================================================
	struct MeshVertexUV {
		Real u;
		Real v;
        
		MeshVertexUV() {
			u = (Real)0.0;
			v = (Real)0.0;
		}

        bool operator ==(const MeshVertexUV& other) const {
            return u==other.u && v==other.v;
        }
	};
	typedef vector<MeshVertexUV> MeshVertexUVList;
	

	//	===========================================================================
	/** \struct		MeshMayaUVSet
		Structure that holds UVs for all vertex-faces for a single UV set.
	*/	
	//	===========================================================================
	struct MeshMayaUVSet {
		MFloatArray	uArray;
		MFloatArray	vArray;
		MString		sName;
	};
	typedef vector<MeshMayaUVSet> MeshMayaUVSetList;


    //	===========================================================================
	/** \struct		VertexBoneAssignment
		Structure that holds vertex bone assignments for a vertex.
	*/	
	//	===========================================================================
	struct VertexBoneAssignment {
        int boneId;
		float weight;
	};
	typedef vector<VertexBoneAssignment> VertexBoneAssignmentList;

	struct WeightLess : std::binary_function<VertexBoneAssignment, VertexBoneAssignment, bool> {
		bool operator()(const VertexBoneAssignment &vba1, const VertexBoneAssignment &vba2) const
		{
			return vba1.weight < vba2.weight;
		}
	};

	//	===========================================================================
	/** \struct		MeshFaceVertex
		Structure that defines a face-vertex.
	*/	
	//	===========================================================================
	struct MeshFaceVertex {
		Vector3          vecPosition;
		Vector3          vecNormal;
		ColourValue      colour;
		MeshVertexUVList listUV;

        VertexBoneAssignmentList boneAssignments;

		bool operator==(const MeshFaceVertex& other) const {
			if (this == &other)
				return true;
            // that's enough for equality (boneAssigment is not neccessery)
            return
                colour == other.colour
                && vecPosition == other.vecPosition
                && vecNormal == other.vecNormal
                && listEqual(
                    listUV.begin(), other.listUV.begin(),
                    listUV.end(), other.listUV.end()
                );                
		}

		void getBoneAssignmentMatrix(MMatrix *rMatrix) const;
#if 0
		void getBoneAssignmentMatrix2(MMatrix *rMatrix) const;
#endif

	};

	typedef vector<MeshFaceVertex> MeshFaceVertexVector;

	//	===========================================================================
	/** \struct		MeshTriFace
		Structure that defines a triangular face as a set of 3 indices into an
		arrray of MeshFaceVertex'es.
	*/	
	//	===========================================================================
	struct MeshTriFace {
		unsigned long index0;
		unsigned long index1;
		unsigned long index2;
	};
	typedef vector<MeshTriFace> MeshTriFaceList;

	//	===========================================================================
	/** \struct		MeshMayaGeometry
		Structure that holds all data for a single Maya mesh.
	*/	
	//	===========================================================================
	struct MeshMayaGeometry {
		string							 name;

		// These are populated by the _queryMayaGeometry method
		MPointArray						 Vertices;
		MFloatVectorArray				 FaceVertexNormals;	 // face-vertex normals
		MColorArray						 FaceVertexColours;
		MIntArray						 TriangleVertexIds;	 // face-relative ids
		MIntArray						 TrianglePolygonIds; // polygon number for each triangle
		vector<MeshMayaUVSet>			 UVSets;
		vector<string>					 shaderNames;		 // shaderNames[shaderId]
		vector<string>					 materialNames;		 // materialNames[shaderId]
		vector<int>						 materialIds;		 // materialIds[polygonId]

		// Populated by _processPolyMesh
		vector<VertexBoneAssignmentList> Weights;

		// Populated in _parseMayaGeometry with _addFace
		// Master set of vertex information
		vector<MeshFaceVertex>			 faceVertices;
		// maps from shader and relative vertex id to absolute vertex id entry in FaceVertices
		vector<vector<int> >			 faceVertexIds;		 // faceVertexIds[shaderId][vertexId]
		// maps from shader and relative triangle id to the indices of the vertices of the triangle
		vector<vector<MeshTriFace> >	 triangleFaces;		 // triangleFaces[shaderId][triangleId]

		// Default bone for vertices that lack bone weights
		int								 parentBoneId;

		// Bounds info
		MVector							 AABBMin;
		MVector							 AABBMax;
		double							 radius;
	};

	//	===========================================================================
	/** \class		MeshGenerator
		\author		John Van Vliet, Fifty1 Software Inc.
		\version	1.0
		\date		June 2003

		Generates an Ogre mesh from a Maya scene. The complete Maya scene is 
		represented by a single Ogre Mesh, and Maya meshes are represented by 
		Ogre SubMeshes.
	*/	
	//	===========================================================================
	class MeshGenerator : SkeletonGenerator {
	public:

		/// Standard constructor.
		MeshGenerator();
		
		/// Destructor.
		virtual ~MeshGenerator();

		/// Export the complete Maya scene (called by OgreMaya.mll or OgreMaya.exe).
		virtual bool exportAll();

		/// Export selected parts of the Maya scene (called by OgreMaya.mll).
		virtual bool exportSelection();

	protected:
		/// Required for OptionParser interface.
		//bool _validateOptions();

		/// Return the value for a boolean attribute of a dependency node.
		static bool _getBooleanAttr(const MFnDependencyNode &fnDepNode,
									const std::string &attrName,
									MStatus &status);
		/// Get the visibility information for the layer of an object.
		static bool _getLayerVisibility(const MFnDependencyNode &fnDepNode,
										MStatus &status);
		/// Get the visibility information for a dag object.  This checks the
		/// object's visibilit, the object's ancestors, and the layer.
		static bool _isVisible(MFnDagNode &fnDepNode, MStatus &status);
		/// Process a Maya polyMesh - this corresponds to a submesh in ogre/axiom
		MStatus _processPolyMesh(MeshMayaGeometry &rGeom, const MDagPath dagPath);
		bool MeshGenerator::_exportMesh();
//		bool _exportSubmesh(ofstream& out, const MeshMayaGeometry &mayaGeometry) const;
		bool _exportSubmesh(ofstream& out, const MeshMayaGeometry &mayaGeometry, int shaderIndex) const;
		MStatus _queryMayaGeometry(MFnMesh &fnMesh, MeshMayaGeometry &rGeom);
		MStatus _parseMayaGeometry(MFnMesh &fnMesh, MeshMayaGeometry &rGeom);
		static void _addFace(MeshMayaGeometry &rMayaGeometry,
							 MeshFaceVertex *faceVertices,
							 bool windForward, int shaderId);
		MStatus _getJoint(const MObject &node, int &index) const;
		void _populateFaceVertex(MeshFaceVertex &faceVertex,
								 const MeshMayaGeometry &rMayaGeometry,
								 const MIntArray &vertexIds, const MIntArray &normalIds, 
								 int iPoly, int iPolyVertex, int iColour) const;
		void _populateFaceVertexUVs(MeshFaceVertex &faceVertex,
								 const MeshMayaGeometry &rMayaGeometry, const MFnMesh &fnMesh,
								 int iPoly, int iPolyVertex) const;
		bool _populateFaceVertexVBAs(MeshFaceVertex &faceVertex,
								 MeshMayaGeometry &rMayaGeometry,
								 const MIntArray &vertexIds, int iPolyVertex) const;
		void _buildBoundsInfo(MeshMayaGeometry &rMayaGeometry) const;
		void _getBoneAssignmentMatrix(MMatrix *rMatrix, const VertexBoneAssignmentList &vbaList) const;
		void _getBoneAssignmentMatrix2(MMatrix *rMatrix, const VertexBoneAssignmentList &vbaList) const;

		void _convertObjectToFace(MItMeshPolygon &iterPoly, MIntArray &objIndices, MIntArray &faceIndices);

		string _getMaterialName(const MObject &shader, MStatus *pStatus = NULL);
	};

} // namespace OgreMaya

#endif
