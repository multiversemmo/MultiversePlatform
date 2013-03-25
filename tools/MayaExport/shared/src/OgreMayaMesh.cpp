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
#include "OgreMayaMesh.h"
#include "OgreMayaOptions.h"

#include <maya/MItGeometry.h>
#include <maya/MFnMesh.h>
#include <maya/MFnIkJoint.h>
#include <maya/MDagPath.h>
#include <maya/MDagPathArray.h>
#include <maya/MSelectionList.h>
#include <maya/MGlobal.h>
#include <maya/MPlug.h>
#include <maya/MFnLambertShader.h>
#include <maya/MFnBlinnShader.h>
#include <maya/MFnPhongShader.h>
#include <maya/MFnReflectShader.h>
#include <maya/MFnSingleIndexedComponent.h>
#include <maya/MFnSet.h>
#include <maya/MPlugArray.h>
#include <maya/MItDependencyGraph.h>
#include <maya/MItDag.h>

#include <maya/MItDependencyNodes.h>
#include <maya/MFnSkinCluster.h>

#include <iostream>
#include <algorithm>
#include <string>

#include <math.h>

namespace OgreMaya {

	using namespace std;


	//	--------------------------------------------------------------------------
	/** Standard constructor. Creates Ogre Mesh and defines known options.
	*/	
	//	--------------------------------------------------------------------------
	MeshGenerator::MeshGenerator() {
	}


	//	--------------------------------------------------------------------------
	/** Destructor.
	*/	
	//	--------------------------------------------------------------------------
	MeshGenerator::~MeshGenerator()
	{
	}

	bool MeshGenerator::exportSelection() {
		return exportAll();
	}

	//	--------------------------------------------------------------------------
	/** Finds and exports all polygonal meshes in the DAG. Each polygonal mesh
		corresponds to a single Ogre SubMesh.

		\return		True if exported ok, false otherwise
	*/	
	//	--------------------------------------------------------------------------
	bool MeshGenerator::exportAll()
	{
		// Reset the joint list
		deleteAll(jointList.begin(), jointList.end());
		jointList.clear();

		if (OPTIONS.exportSkeleton || OPTIONS.exportVBA || OPTIONS.animations.size()) {
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
		}

		if (!_exportMesh())
			return false;

		return true;
	}

	bool MeshGenerator::_exportMesh() {
		MStatus status;
		// ===== Iterate over mesh components of DAG

		// --- Setup iterator
		MItDag iterDag(MItDag::kDepthFirst, MFn::kMesh, &status);
		if (status == MStatus::kFailure) {
			MGlobal::displayError("MItDag::MItDag");
			return false;
		}

		std::vector<MeshMayaGeometry> geometryList;
		{
			MSelectionList list;
			MGlobal::getActiveSelectionList(list);

			// --- Iterate
			for(; !iterDag.isDone(); iterDag.next()) {
			
				// Get DAG path
				MDagPath dagPath;
				status = iterDag.getPath(dagPath);
				if (status != MStatus::kSuccess) {
					MGlobal::displayError("MDagPath::getPath");
					return false;
				}

				// Process this node?
				//if(OPTIONS.exportSelected && !list.hasItem(dagPath.node())) continue;
				
				if (dagPath.hasFn(MFn::kTransform))
					continue;
				if (!dagPath.hasFn(MFn::kMesh))
					continue;
				
				MFnDagNode dagNode(dagPath);
				if (dagNode.isIntermediateObject())
					continue;

				if (!_isVisible(dagNode, status))
					continue;

				MeshMayaGeometry mayaGeometry;
				status = _processPolyMesh(mayaGeometry, dagPath);
				if (status != MStatus::kSuccess) {
					cout << "\t[ERROR] Skipping invalid poly mesh\n";
					continue;
				}
				geometryList.push_back(mayaGeometry);
			}

			ofstream out(OPTIONS.outMeshFile.c_str());

			out.precision(OPTIONS.precision);
			out.setf(ios::fixed);
			std::vector<MeshMayaGeometry>::const_iterator iter;
			std::vector<MeshMayaGeometry>::const_iterator end = geometryList.end();

			out << "<mesh>\n";
			out << "\t<submeshes>\n";
			for (iter = geometryList.begin(); iter != end; ++iter) {
				for (unsigned int i = 0; i < (*iter).materialNames.size(); ++i) {
					const vector<MeshTriFace> &faces = (*iter).triangleFaces[i];
					if ((*iter).materialNames[i] != "" && faces.size() != 0)
						_exportSubmesh(out, *iter, i);
					else if (faces.size() != 0)
						cout << "Skipping invalid submesh (no material) for object " << (*iter).name 
							 << " with shader " << (*iter).shaderNames[i] << endl;
					else
						cout << "Skipping invalid submesh (no faces) for object " << (*iter).name 
							 << " with shader " << (*iter).shaderNames[i] << endl;
				}
			}
			out << "\t</submeshes>\n";

			out << "\t<submeshnames>\n";
			int submeshIndex = 0;
			for (iter = geometryList.begin(); iter != end; ++iter) {
				for (unsigned int i = 0; i < (*iter).materialNames.size(); ++i) {
					const vector<MeshTriFace> &faces = (*iter).triangleFaces[i];
					if ((*iter).materialNames[i] != "" && faces.size() != 0)
						out << "\t\t<submeshname name=\"" << (*iter).name << "/" << (*iter).shaderNames[i] << "\" index=\"" << submeshIndex++ << "\"/>\n";
				}
			}
			out << "\t</submeshnames>\n";

			if (OPTIONS.exportBounds) {
				MVector min = MVector::zero;
				MVector max = MVector::zero;
				float radius = 0.0f;
				for (iter = geometryList.begin(); iter != end; ++iter) {
					const MVector &vec_min = iter->AABBMin;
					const MVector &vec_max = iter->AABBMax;
					if (vec_min.x < min.x)
						min.x = vec_min.x;
					if (vec_min.y < min.y)
						min.y = vec_min.y;
					if (vec_min.z < min.z)
						min.z = vec_min.z;
					if (vec_max.x > max.x)
						max.x = vec_max.x;
					if (vec_max.y > max.y)
						max.y = vec_max.y;
					if (vec_max.z > max.z)
						max.z = vec_max.z;
					if (iter->radius > radius)
						radius = iter->radius;
				}

				out << "\t<boundsinfo>\n";
				out << "\t\t<boundingbox>\n";

				out << "\t\t\t<min ";
				out << "x=\"" << OPTIONS.scale * min.x << "\" ";
				out << "y=\"" << OPTIONS.scale * min.y << "\" ";
				out << "z=\"" << OPTIONS.scale * min.z << "\"/>\n";

				out << "\t\t\t<max ";
				out << "x=\"" << OPTIONS.scale * max.x << "\" ";
				out << "y=\"" << OPTIONS.scale * max.y << "\" ";
				out << "z=\"" << OPTIONS.scale * max.z << "\"/>\n";

				out << "\t\t</boundingbox>\n";
				out << "\t\t<boundingsphere radius=\"" << OPTIONS.scale * radius << "\"/>\n";

				out << "\t</boundsinfo>\n";
			}

			if (OPTIONS.exportSkeleton) {
				string skeletonName =
					OPTIONS.outSkelFile.substr(0, OPTIONS.outSkelFile.find_last_of('.'));
				out << "\t<skeletonlink name=\"" << skeletonName << "\"/>\n";
			}

			out << "</mesh>\n";
		}
		// ===== Done
		return (status == MStatus::kSuccess);
	}

	// This method exports the portion of the information for the mesh object 
	// contained in mayaGeometry that is associated with the shaderIndex as a
	// submesh xml node.
	bool MeshGenerator::_exportSubmesh(ofstream& out, const MeshMayaGeometry &mayaGeometry, int shaderIndex) const {
		// export as XML
		out << "\t\t<submesh material=\"" << OPTIONS.matPrefix << mayaGeometry.materialNames[shaderIndex] << "\" usesharedvertices=\"false\" use32bitindexes=\"false\">\n";

		// Create face list (list of vertex indices)
		const vector<MeshTriFace> &triangleFaces = mayaGeometry.triangleFaces[shaderIndex];
		vector<MeshTriFace>::const_iterator face_iter;
		vector<MeshTriFace>::const_iterator face_end = triangleFaces.end();

		out << "\t\t\t<faces count=\"" << triangleFaces.size() << "\">\n";
		for (face_iter = triangleFaces.begin(); face_iter != face_end; ++face_iter) {
			out << "\t\t\t\t<face ";
			out << "v1=\"" << face_iter->index0 << "\" ";
			out << "v2=\"" << face_iter->index1 << "\" ";
			out << "v3=\"" << face_iter->index2 << "\"/>\n";
		}
		out << "\t\t\t</faces>\n";

		// Export the geometry information

		// Create vertex buffer information
		const vector<int> &faceVertexIds = mayaGeometry.faceVertexIds[shaderIndex];
		vector<int>::const_iterator vertex_iter;
		vector<int>::const_iterator vertex_end = faceVertexIds.end();

		out << "\t\t\t<geometry vertexcount=\"" << faceVertexIds.size() << "\">\n";

		// Positions and normals in the first buffer
		out << "\t\t\t\t<vertexbuffer positions=\"true\"";
		if(OPTIONS.exportNormals)
			out << " normals=\"true\"";
		out << ">\n";

		for (vertex_iter = faceVertexIds.begin(); vertex_iter != vertex_end; ++vertex_iter) {
			const MeshFaceVertex &vertex = mayaGeometry.faceVertices[*vertex_iter];
			out << "\t\t\t\t\t<vertex>\n";

			out << "\t\t\t\t\t\t<position ";
			out << "x=\"" << vertex.vecPosition.x << "\" ";
			out << "y=\"" << vertex.vecPosition.y << "\" ";
			out << "z=\"" << vertex.vecPosition.z << "\"/>\n";

			if(OPTIONS.exportNormals) {
				out << "\t\t\t\t\t\t<normal ";
				out << "x=\"" << vertex.vecNormal.x << "\" ";
				out << "y=\"" << vertex.vecNormal.y << "\" ";
				out << "z=\"" << vertex.vecNormal.z << "\"/>\n";
			}

			out << "\t\t\t\t\t</vertex>\n";
		}

		out << "\t\t\t\t</vertexbuffer>\n";

		// Color and UV coordinates in the next buffer
		out << "\t\t\t\t<vertexbuffer";
		if (OPTIONS.exportColours)
			out << " colours_diffuse=\"true\"";
		if (mayaGeometry.UVSets.size() > 0 && OPTIONS.exportUVs) {
			for (unsigned int i = 0; i < mayaGeometry.UVSets.size(); ++i) 
				out << " texture_coord_dimensions_" << i << "=\"2\"";
			out << " texture_coords=\"" << mayaGeometry.UVSets.size() << "\"";
		}
		out << ">\n";

		for (vertex_iter = faceVertexIds.begin(); vertex_iter != vertex_end; ++vertex_iter) {
			const MeshFaceVertex &vertex = mayaGeometry.faceVertices[*vertex_iter];
			out << "\t\t\t\t\t<vertex>\n";
			if (OPTIONS.exportColours) {            
				out << "\t\t\t\t\t\t<colour_diffuse value=\"";
				out << vertex.colour.r << " ";
				out << vertex.colour.g << " ";
				out << vertex.colour.b << " ";
				out << vertex.colour.a << "\"/>\n";                
			}

			if (mayaGeometry.UVSets.size() > 0 && OPTIONS.exportUVs) {
				MeshVertexUVList::const_iterator uv_iter;
				MeshVertexUVList::const_iterator uv_end = vertex.listUV.end();
				for (uv_iter = vertex.listUV.begin(); uv_iter != uv_end; ++uv_iter) {
					out << "\t\t\t\t\t\t<texcoord ";
					out << "u=\"" << uv_iter->u << "\" ";
					out << "v=\"" << uv_iter->v << "\"/>\n";
				}
			}

			out << "\t\t\t\t\t</vertex>\n";
		}

		out << "\t\t\t\t</vertexbuffer>\n";

		out << "\t\t\t</geometry>\n";


		// Bone assigments
		if (OPTIONS.exportVBA) {
			out << "\t\t\t<boneassignments>\n";

			int i = 0;
			for (vertex_iter = faceVertexIds.begin(); vertex_iter != vertex_end; ++vertex_iter) {
				const MeshFaceVertex &vertex = mayaGeometry.faceVertices[*vertex_iter];
				VertexBoneAssignmentList::const_iterator bone_iter;
				VertexBoneAssignmentList::const_iterator bone_end = vertex.boneAssignments.end();
				for (bone_iter = vertex.boneAssignments.begin(); bone_iter != bone_end; ++bone_iter) {
					out << "\t\t\t\t<vertexboneassignment ";
					out << "vertexindex=\"" << i << "\" ";
					out << "boneindex=\"" << bone_iter->boneId << "\" ";
					out << "weight=\"" << bone_iter->weight << "\"/>\n";
				}
				i++;
			}

			out << "\t\t\t</boneassignments>\n";
		}

		out << "\t\t</submesh>\n";
		return true;
	}

	//	--------------------------------------------------------------------------
	/**	Process a Maya polyMesh to generate an Ogre SubMesh.

		\param		dagPath
					Path to the Maya polyMesh to be processed

		\return		MStatus::kSuccess if processed successfuly, 
					different MStatus otherwise

		\todo		Vertex optimization
		\todo		Submesh optimization (merge submeshes that share materials)
	*/	
	//	--------------------------------------------------------------------------
	MStatus MeshGenerator::_processPolyMesh(MeshMayaGeometry &mayaGeometry,
											const MDagPath dagPath) {

		cout << "\nMeshGenerator::_processPolyMesh\n";
		cout << "\tdagPath = \"" << dagPath.fullPathName().asChar() << "\"\n";       

		MStatus status = MStatus::kSuccess;

		//*******************************************************************
		// ===== Calculate the influence of SkinCluster if any
		bool hasSkinCluster = false;

		//search the skin cluster affecting this geometry
		MItDependencyNodes kDepNodeIt(MFn::kSkinClusterFilter);            

		MFnMesh fnMesh(dagPath, &status);

		for( ;!kDepNodeIt.isDone() && !hasSkinCluster; kDepNodeIt.next()) {            

			MObject	kInputObject, kOutputObject;                    
			MObject kObject = kDepNodeIt.item();

			MFnSkinCluster kSkinClusterFn(kObject, &status);

			cout << "\tskin cluster name: " << kSkinClusterFn.name().asChar() << '\n';

			unsigned int uiNumGeometries = kSkinClusterFn.numOutputConnections();

			cout << "\tfound " << uiNumGeometries << " geometry object(s) in skin cluster\n";

			for (unsigned int uiGeometry = 0; uiGeometry < uiNumGeometries; ++uiGeometry) {
				unsigned int uiIndex = kSkinClusterFn.indexForOutputConnection(uiGeometry, &status);

				kInputObject = kSkinClusterFn.inputShapeAtIndex(uiIndex, &status);
				kOutputObject = kSkinClusterFn.outputShapeAtIndex(uiIndex, &status);

				MDagPathArray paths;
				kSkinClusterFn.influenceObjects(paths, &status);
				if (status != MStatus::kSuccess) {
					MGlobal::displayError("MFnSkinCluster::influenceObjects");
					return status;
				}

				if (kOutputObject != fnMesh.object())
					continue;

				// We have found a skin cluster that modifies the mesh we are
				// exporting.
				hasSkinCluster = true;
				// get weights
				MItGeometry kGeometryIt(kInputObject);

				for (; !kGeometryIt.isDone(); kGeometryIt.next()) {
					MObject kComponent = kGeometryIt.component(&status);
					if (status != MStatus::kSuccess) {
						MGlobal::displayError("MItGeometry::component");
						return status;
					}
					MFloatArray kWeightArray;
					unsigned int uiNumInfluences;
					kSkinClusterFn.getWeights(dagPath, kComponent, kWeightArray, uiNumInfluences);
					
					MDagPathArray influencePaths;
					kSkinClusterFn.influenceObjects(influencePaths, &status);
					
					int iVertex = kGeometryIt.index();
					mayaGeometry.Weights.resize(iVertex + 1);
					VertexBoneAssignmentList &vbaList = mayaGeometry.Weights[iVertex];
					
					for (unsigned int influenceId = 0; influenceId < influencePaths.length(); ++influenceId) {
						const float eps = .001f;
						// Skip tiny weights
						if (kWeightArray[influenceId] < eps)
							continue;
						SkeletonJoint *joint = _getSkeletonJoint(influencePaths[influenceId]);
						if (joint == NULL)
							cout << "\t[ERROR] Unable to find matching joint for " 
								 << influencePaths[influenceId].partialPathName() << endl;
						VertexBoneAssignment vba;
						vba.boneId = joint->bone_index;
						vba.weight = kWeightArray[influenceId];
						vbaList.push_back(vba);
					}
				}
			}
		}

		// ===== Get Maya geometry		
		status = _queryMayaGeometry(fnMesh, mayaGeometry);
		if (status == MStatus::kFailure) {
			cout << "\t[ERROR] Failed to query maya geometry\n";
			return status;
		}
		
		// ===== Query bounds info
		if (OPTIONS.exportBounds)
			_buildBoundsInfo(mayaGeometry);

		// ===== Parse into MeshGenerator format
		status = _parseMayaGeometry(fnMesh, mayaGeometry);
		if (status == MStatus::kFailure) {
			cout << "\t[ERROR] Failed to parse maya geometry\n";
			return status;
		}

		// ===== Success!
		return MStatus::kSuccess;
	}

	// Get the name of the material associated with the set
	string MeshGenerator::_getMaterialName(const MObject &set, MStatus *pStatus) {
		MStatus status = MStatus::kSuccess;
		string materialName = "";

		if (!set.hasFn(MFn::kDependencyNode)) {
			cout << "\t[ERROR] Object is not a dependency node" << endl;
			if (pStatus != NULL)
				*pStatus = MStatus::kFailure;
			return materialName;
		}

		MFnDependencyNode fnDepNode(set);
		MObject ssAttr = fnDepNode.attribute("surfaceShader");
		MPlug ssPlug(set, ssAttr);
		MPlugArray srcPlugArray;
		ssPlug.connectedTo(srcPlugArray, true, false, &status);
		if (status != MStatus::kSuccess) {
			cout << "\t[ERROR] Failed to find connected surfaceShader objects\n";
			if (pStatus != NULL)
				*pStatus = status;
			return materialName;
		}

		if (srcPlugArray.length() <= 0)
			cout << "\t[WARNING] Failed to find any connected surfaceShader objects;"
				 << "Set was of type: " << set.apiTypeStr() << endl;

		for (int i = 0; i < srcPlugArray.length(); ++i) {
			// This object contains a reference to a shader or material.
			// Check for known material types and extract material name.
			MObject shader = srcPlugArray[0].node();
			if (shader.hasFn(MFn::kPhong)) {
				MFnPhongShader fnPhong(shader);
				materialName = fnPhong.name().asChar();
				break;
			} else if (shader.hasFn(MFn::kLambert)) {
				MFnLambertShader fnLambert(shader);
				materialName = fnLambert.name().asChar();
				break;
			} else if (shader.hasFn(MFn::kBlinn)) {
				MFnBlinnShader fnBlinn(shader);
				materialName = fnBlinn.name().asChar();
				break;
			} else if (shader.hasFn(MFn::kReflect)) {
				MFnReflectShader fnReflect(shader);
				materialName = fnReflect.name().asChar();
				break;
			} else {
				cout << "\t[WARNING] Found unknown shader type: " << shader.apiTypeStr() << endl;
			}
		}
		if (pStatus != NULL)
			*pStatus = status;
		return materialName;
	}

	// The returned index will be the index in the joint list (rather than the maya logical index)
	MStatus MeshGenerator::_getJoint(const MObject &node, int &index) const {
		index = -1;
		MDagPath lastJoint;
		MStatus status;
		// Do a depth first traversal of the joints, and return the last one
		// that is a parent of this node.
		MItDag iterDag(MItDag::kBreadthFirst, MFn::kJoint, &status);

		if (status != MStatus::kSuccess) {
			cout << "\t[ERROR] MItDag constructor failed\n"; 
			return status;
		}

		while (!iterDag.isDone()) {
			MFnIkJoint kJointFn = iterDag.item(&status);
			if (status != MStatus::kSuccess) {
				cout << "\t[ERROR] MItDag::item failed\n"; 
				return status;
			}
			if (kJointFn.isParentOf(node))
				kJointFn.getPath(lastJoint);
			iterDag.next();
		}
		SkeletonJointList::const_iterator iter = jointList.begin();
		SkeletonJointList::const_iterator iter_end = jointList.end();
		for (iter = jointList.begin(); iter != iter_end; ++iter)
			if ((*iter)->dagPath == lastJoint) {
				index =(*iter)->bone_index;
				return MStatus::kSuccess;
			}
		return MStatus::kSuccess;
	}

	//	--------------------------------------------------------------------------
	/**	Retrieve all Maya geometry for a single Maya mesh.
		\todo		Define materials if requested
		\todo		Fix normals
	*/	
	//	--------------------------------------------------------------------------
	MStatus MeshGenerator::_queryMayaGeometry(
		MFnMesh &fnMesh, 
		MeshMayaGeometry &rGeom
	) {
		cout << "\nMeshGenerator::_queryMayaGeometry\n";

		MStatus status = MStatus::kSuccess;

		MDagPath dagPath;
		fnMesh.getPath(dagPath);

		int instanceCount = fnMesh.instanceCount(true, &status);
		if (status != MStatus::kSuccess) {
			cout << "\t[ERROR] MFnMesh::instanceCount() failed\n";
			return status;
		}

		MObjectArray shaders;
		MIntArray    shaderVertexIndices;
		status = fnMesh.getConnectedShaders(dagPath.instanceNumber(), shaders, shaderVertexIndices);
		if (status != MStatus::kSuccess) {
			cout << "\t[ERROR] MFnMesh::getConnectedShaders() failed\n";
			return status;
		}

		// Size these vectors appropriately
		rGeom.faceVertexIds.resize(shaders.length());
		rGeom.triangleFaces.resize(shaders.length());
		rGeom.materialNames.resize(shaders.length());
		rGeom.shaderNames.resize(shaders.length());
		rGeom.materialIds.resize(shaderVertexIndices.length());

		for (unsigned int i = 0; i < shaders.length(); ++i) {
			MFnDependencyNode fnDnSet(shaders[i]);
			rGeom.shaderNames[i] = fnDnSet.name().asChar();
			rGeom.materialNames[i] = _getMaterialName(shaders[i], &status);
			if (status != MStatus::kSuccess) {
				cout << "\t[ERROR] _getMaterialName() failed\n";
				return status;
			}
			cout << "Shader[" << i << "] = " << rGeom.shaderNames[i] << " and material is " << rGeom.materialNames[i] << endl;
		}

		rGeom.materialIds.resize(shaderVertexIndices.length());
		for (unsigned int i = 0; i < shaderVertexIndices.length(); ++i)
			rGeom.materialIds[i] = shaderVertexIndices[i];

		// ===== Identification		
		rGeom.name = fnMesh.partialPathName().asChar(); // shortest unique name

		// ===== Parent Bone
		status = _getJoint(fnMesh.object(), rGeom.parentBoneId);
		cout << "\nMeshGenerator::_getJoint: " << rGeom.parentBoneId << endl;


		// ===== Geometry

		// --- Vertices
		status = fnMesh.getPoints(rGeom.Vertices, MSpace::kWorld);
		if (status != MStatus::kSuccess) {
			cout << "\t[ERROR] MFnMesh::getPoints() failed\n"; 
			return status;
		}

		cout << "\tvertices count: " << rGeom.Vertices.length() << '\n';

		// --- Vertex normals
		status = fnMesh.getNormals(rGeom.FaceVertexNormals);
		if (status == MStatus::kFailure) {
			cout << "\t[ERROR] MFnMesh::getNormals() failed\n"; 
			return status;
		}

		// --- Triangular faces
		MItMeshPolygon iterPoly(dagPath);
		
		int iPolygon, nPolygons;
		nPolygons = fnMesh.numPolygons();
		for (iPolygon=0; iPolygon < nPolygons; ++iPolygon)
		{
			MIntArray polyTriVertices;
			MPointArray polyPointsUntweaked;
			iterPoly.getTriangles(polyPointsUntweaked, polyTriVertices, MSpace::kWorld);

			_convertObjectToFace(iterPoly, polyTriVertices, rGeom.TriangleVertexIds);

			int iTriangle, nTriangles;
			iterPoly.numTriangles(nTriangles);
			for (iTriangle=0; iTriangle < nTriangles; ++iTriangle)
				rGeom.TrianglePolygonIds.append(iPolygon);

			iterPoly.next();
		}


		// ===== Colours and UVs

		// --- Face vertex colours
		status = fnMesh.getFaceVertexColors(rGeom.FaceVertexColours);
		if (status == MStatus::kFailure) {
			cout << "\t[ERROR] MFnMesh::getFaceVertexColors() failed\n"; 
			return status;
		}
		// Override non-existent colours with semi-transparent white
		unsigned int iFaceVertex;
		MColor mayaColour;
		for (iFaceVertex=0; iFaceVertex < rGeom.FaceVertexColours.length(); ++iFaceVertex) {
			mayaColour = rGeom.FaceVertexColours[iFaceVertex];
			if ((mayaColour.r) == -1) mayaColour.r = 1;
			if ((mayaColour.g) == -1) mayaColour.g = 1;
			if ((mayaColour.b) == -1) mayaColour.b = 1;
			if ((mayaColour.a) == -1) mayaColour.a = 0.2f;
			rGeom.FaceVertexColours[iFaceVertex] = mayaColour;
		}

		// --- UV set names
		MStringArray UVSetNames;
		status = fnMesh.getUVSetNames(UVSetNames);
		if (status == MStatus::kFailure) {
			cout << "\t[ERROR] MFnMesh::getUVSetNames() failed\n"; 
			return status;
		}

		// --- Linked list of UV sets
		unsigned int nUVSets = UVSetNames.length();
		unsigned int iUVSet;

		// Loop over all UV sets
		MeshMayaUVSet UVSet;
		for (iUVSet = 0; iUVSet < nUVSets; ++iUVSet) {

			// Store UV name
			UVSet.sName = UVSetNames[iUVSet];

			// Retrieve UV coordinates
			status = fnMesh.getUVs(UVSet.uArray, UVSet.vArray, &(UVSet.sName));
			if (status != MStatus::kSuccess)
				return status;

			MObjectArray textures;
			status = fnMesh.getAssociatedUVSetTextures(UVSet.sName, textures);
			if (status != MStatus::kSuccess)
				return status;

			int numTextures = textures.length();
			for (int i = 0; i < numTextures; ++i) {
				MObject obj = textures[i];
				// Is it a file texture?
				if (obj.hasFn(MFn::kFileTexture)) {
					MFnDependencyNode fnFile(obj);
					MPlug ftnPlug = fnFile.findPlug("fileTextureName", &status );
					if (status != MS::kSuccess)
						return status;
					MString fileTextureName;
					ftnPlug.getValue(fileTextureName);
					cout << "\t[INFO] File Texture: " << fileTextureName << endl;
				} else {
					cout << "\t[WARNING] Unsupported texture type" << obj.apiTypeStr() << endl;
				}
			}

			// Store UV set
			rGeom.UVSets.push_back(UVSet);
		}

		// ===== Done
		return status;

	}

	void MeshGenerator::_buildBoundsInfo(MeshMayaGeometry &rGeom) const {
		MVector &min = rGeom.AABBMin;
		MVector &max = rGeom.AABBMax;
		MPoint zeroPoint = MPoint::origin;
		for (unsigned int i = 0; i < rGeom.Vertices.length(); ++i) {
			MPoint &vec = rGeom.Vertices[i];
			if (vec.x < min.x)
				min.x = vec.x;
			if (vec.y < min.y)
				min.y = vec.y;
			if (vec.z < min.z)
				min.z = vec.z;
			if (vec.x > max.x)
				max.x = vec.x;
			if (vec.y > max.y)
				max.y = vec.y;
			if (vec.z > max.z)
				max.z = vec.z;
			if (vec.distanceTo(zeroPoint) > rGeom.radius)
				rGeom.radius = vec.distanceTo(zeroPoint);
		}
	}

	void MeshGenerator::_populateFaceVertexUVs(MeshFaceVertex &faceVertex,
		const MeshMayaGeometry &rMayaGeometry, const MFnMesh &fnMesh,
		int iPoly, int iPolyVertex) const
	{
		MStatus status;
		// Loop over UV sets
		MeshMayaUVSetList::const_iterator iterUVSet;
		iterUVSet = rMayaGeometry.UVSets.begin();
		while (iterUVSet != rMayaGeometry.UVSets.end()) {                        
			int iUV;
			status = fnMesh.getPolygonUVid(iPoly, iPolyVertex, iUV, &(iterUVSet->sName));
			MeshVertexUV vertexUV;
			if (!status.error()) {
				// Make sure u and v are in the range [0, 1]
				// Also mirror v.
				vertexUV.u = fmod(iterUVSet->uArray[iUV], 1.0f);
				if (vertexUV.u < 0)
					vertexUV.u += 1.0f;
				vertexUV.v = fmod(1.0f - iterUVSet->vArray[iUV], 1.0f);	// CJV 2004-01-05: Required for Ogre 0.13
				if (vertexUV.v < 0)
					vertexUV.v += 1.0f;
			} else {
				cout << "\t[WARNING]: unable to export polygon uv coordinate" << endl;
				vertexUV.u = 0;
				vertexUV.v = 0;
			}
			faceVertex.listUV.push_back(vertexUV);
			
			++iterUVSet;
		}

	}

	/// Only call this method on objects that have a skeleton.
	bool MeshGenerator::_populateFaceVertexVBAs(MeshFaceVertex &faceVertex,
		MeshMayaGeometry &rMayaGeometry, 
		const MIntArray &vertexIds, int iPolyVertex) const
	{
		int iVertex = vertexIds[iPolyVertex];
		
		if (iVertex >= rMayaGeometry.Weights.size())
			rMayaGeometry.Weights.resize(iVertex + 1);

		faceVertex.boneAssignments = rMayaGeometry.Weights[iVertex];

		VertexBoneAssignmentList &vbaList = faceVertex.boneAssignments;
		VertexBoneAssignmentList::iterator iter;
		if (vbaList.size() == 0) {
			// Vertex without any weight -- attach to parent bone
			if (rMayaGeometry.parentBoneId == -1) {
				cout << "\t[WARNING]: Invalid parent bone id, and no skin attachment\n";
				return false;
			}
			VertexBoneAssignment vba;
			vba.boneId = rMayaGeometry.parentBoneId;
			vba.weight = 1.0f;
			vbaList.push_back(vba);
		} else if (faceVertex.boneAssignments.size() > 4) {
			WeightLess weightCompare;
			std::sort(vbaList.begin(), vbaList.end(), weightCompare);
			cout << "\t[WARNING]: Exceeded 4 bone influences for vertex: " << iVertex << endl;
			int bonesLeft = 4;
			for (iter = vbaList.begin(); iter != vbaList.end(); ++iter)
				if (--bonesLeft < 0)
					break;
			vbaList.erase(iter, vbaList.end());
		}
		// Now normalize the list
		float totalWeight = 0.0f;
		for (iter = vbaList.begin(); iter != vbaList.end(); ++iter)
			totalWeight += (*iter).weight;
		for (iter = vbaList.begin(); iter != vbaList.end(); ++iter)
			(*iter).weight /= totalWeight;

		return true;
	}

	void MeshGenerator::_populateFaceVertex(MeshFaceVertex &faceVertex,
		const MeshMayaGeometry &rMayaGeometry,
		const MIntArray &vertexIds, const MIntArray &normalIds, 
		int iPoly, int iPolyVertex, int iColour) const
	{
		MStatus status;

		// Lookup and store face-vertex position
		MPoint mayaPoint;
		int iVertex = vertexIds[iPolyVertex];
		mayaPoint = rMayaGeometry.Vertices[iVertex];

		faceVertex.vecPosition.x = OPTIONS.scale * mayaPoint.x;
		faceVertex.vecPosition.y = OPTIONS.scale * mayaPoint.y;
		faceVertex.vecPosition.z = OPTIONS.scale * mayaPoint.z;

		// Lookup and store face-vertex normal
		MVector mayaNormal;
		int iNormal = normalIds[iPolyVertex];
		mayaNormal = rMayaGeometry.FaceVertexNormals[iNormal];
		faceVertex.vecNormal.x = mayaNormal.x;
		faceVertex.vecNormal.y = mayaNormal.y;
		faceVertex.vecNormal.z = mayaNormal.z;

		// Lookup and store face-vertex colour
		MColor mayaColour;
		mayaColour = rMayaGeometry.FaceVertexColours[iColour];
		faceVertex.colour.r = mayaColour.r;
		faceVertex.colour.g = mayaColour.g;
		faceVertex.colour.b = mayaColour.b;
		faceVertex.colour.a = mayaColour.a;
	}

	void MeshGenerator::_addFace(MeshMayaGeometry &rMayaGeometry,
								 MeshFaceVertex *faceVertices,
								 bool windForward, int shaderId) {
		int absIndex[3]; // index into the FaceVertices array (the set of all vertices for this mesh)
		int relIndex[3]; // index into the set of vertices associated with this shader
		for (int i = 0; i < 3; i++) {
			MeshFaceVertex &faceVertex = faceVertices[i];
			if (!windForward)
				faceVertex.vecNormal *= -1;

			// First check the set of all vertices, and add the vertex if it is new
			vector<MeshFaceVertex>::const_iterator face_vertex_iter = rMayaGeometry.faceVertices.begin();
			vector<MeshFaceVertex>::const_iterator face_vertex_end = rMayaGeometry.faceVertices.end();
			for (int j = 0; face_vertex_iter != face_vertex_end; ++face_vertex_iter, ++j) {
				if (faceVertex == *face_vertex_iter) {
					// this vertex is already in the set associated with this mesh
					absIndex[i] = j;
					break;
				}
			}
			// if it was not found in the set associated with this mesh, insert it now
			if (face_vertex_iter == face_vertex_end) {
				absIndex[i] = rMayaGeometry.faceVertices.size();
				rMayaGeometry.faceVertices.push_back(faceVertex);
			}

			// Now check the set of vertices associated with this shader, and add the vertex
			// reference if it is new to this shader.
			vector<int> &shaderVertexIds = rMayaGeometry.faceVertexIds[shaderId];
			vector<int>::const_iterator shader_vertex_iter = shaderVertexIds.begin();
			vector<int>::const_iterator shader_vertex_end = shaderVertexIds.end();
			for (int j = 0; shader_vertex_iter != shader_vertex_end; ++shader_vertex_iter, ++j) {
				if (absIndex[i] == *shader_vertex_iter) {
					// this vertex reference is already in the set associated with this shader
					relIndex[i] = j;
					break;
				}
			}
			// if it was not found in the set associated with this shader, insert it now
			if (shader_vertex_iter == shader_vertex_end) {
				relIndex[i] = shaderVertexIds.size();
				shaderVertexIds.push_back(absIndex[i]);
			}
		}

		// --- Define face (three face-vertices)
		MeshTriFace triFace;
		if (windForward) {
			// add the triangle with standard winding order
			triFace.index0 = (unsigned long)relIndex[0];
			triFace.index1 = (unsigned long)relIndex[1];
			triFace.index2 = (unsigned long)relIndex[2];
		} else {
			// add the triangle with opposite winding order
			triFace.index0 = (unsigned long)relIndex[0];
			triFace.index1 = (unsigned long)relIndex[2];
			triFace.index2 = (unsigned long)relIndex[1];
		}
		rMayaGeometry.triangleFaces[shaderId].push_back(triFace);
	}

	//	--------------------------------------------------------------------------
	/** Parse Maya geometry into MeshGenerator format for further processing.
	    This essentially means building the FaceVertices and TriFaces lists.
	*/	
	//	--------------------------------------------------------------------------
	MStatus MeshGenerator::_parseMayaGeometry(MFnMesh &fnMesh, MeshMayaGeometry &rMayaGeometry)
	{
		cout << "\nMeshGenerator::_parseMayaGeometry\n";

		MStatus status;

		// --- Determine number of triangles
		unsigned int nTris = rMayaGeometry.TrianglePolygonIds.length();
		if (nTris == 0)
			return MStatus::kFailure;

		// --- Confirm number of triangle vertices
		unsigned int nTriVertices = rMayaGeometry.TriangleVertexIds.length();
		if (nTriVertices != 3 * nTris) {
			cout << "\t[ERROR] "<<nTris<<" triangles require "<<(3*nTris)<<" vertices but "<<nTriVertices<<" vertices present!\n";
			return MStatus::kFailure;
		}

		// --- Loop over all triangles
		unsigned int iTri;
		cout << "\texporting " << fnMesh.numPolygons() << " polygons as " << nTris << " triangles from " << rMayaGeometry.name << endl;

		cout << "number of weights: " << rMayaGeometry.Weights.size() << endl;

		for (iTri = 0; iTri < nTris; ++iTri) {
			// --- Get polygon index
			unsigned int iPoly;
			iPoly = rMayaGeometry.TrianglePolygonIds[iTri];

			// --- Get indices of face-vertices
			MIntArray vertexIds;
			status = fnMesh.getPolygonVertices(iPoly, vertexIds);
			if (status == MStatus::kFailure) {
				MGlobal::displayError("MFnMesh::getPolygonVertices()");
				return status;
			}

			// --- Get indices of face-vertex normals
			MIntArray normalIds;
			fnMesh.getFaceNormalIds(iPoly, normalIds);
			if (status == MStatus::kFailure) {
				MGlobal::displayError("MFnMesh::getFaceNormalIds()");
				return status;
			}

			// --- Loop over all face-vertices
			unsigned int iTriVertex;
			MeshFaceVertex faceVertices[3];
			for (iTriVertex = 0; iTriVertex < 3; ++iTriVertex) {
				MeshFaceVertex& faceVertex = faceVertices[iTriVertex];
				// Get polygon vertex id
				int iPolyVertex;
				iPolyVertex = rMayaGeometry.TriangleVertexIds[3*iTri + iTriVertex];
				int iColour;
				status = fnMesh.getFaceVertexColorIndex(iPoly, iPolyVertex, iColour);
				_populateFaceVertex(faceVertex, rMayaGeometry, 
									vertexIds, normalIds, iPoly, iPolyVertex, iColour);
				_populateFaceVertexUVs(faceVertex, rMayaGeometry, fnMesh, iPoly, iPolyVertex);
				if (jointList.size() > 0) {
					// TODO: I should run this after the duplicate vertex removal below instead
					if (!_populateFaceVertexVBAs(faceVertex, rMayaGeometry, vertexIds, iPolyVertex))
						return MStatus::kFailure;
				}
			}

			// Add the face vertices for this face to the rMayaGeometry.
			// If the face vertex is already in the geometry, don't bother.
			// Set up the index array to point to the new location of these
			// face vertices in the rMayaGeometry.FaceVertices list.
			bool opposite = _getBooleanAttr(fnMesh, "opposite", status);
			bool doubleSided = _getBooleanAttr(fnMesh, "doubleSided", status);
			int shaderId = rMayaGeometry.materialIds[iPoly];
			if (!opposite || doubleSided)
				_addFace(rMayaGeometry, faceVertices, true, shaderId);
			if (opposite || doubleSided)
				_addFace(rMayaGeometry, faceVertices, false, shaderId);
		}

		cout << "\tFaceVertices.size() = " << rMayaGeometry.faceVertices.size() << endl;
		return MStatus::kSuccess;
	}

	bool MeshGenerator::_isVisible(MFnDagNode &fnDagNode, MStatus &status) {
		MFnDependencyNode fnDepNode(fnDagNode.object());
		if (!_getBooleanAttr(fnDepNode, "visibility", status))
			return false;
		if (!_getLayerVisibility(fnDepNode, status))
			return false;
		unsigned int parentCount = fnDagNode.parentCount(&status);
		if (status != MStatus::kSuccess) {
			cout << "\t[WARNING] Unable to determine parent count of dag node: "
				 << fnDagNode.partialPathName() << endl;
			return false;
		}
		if (parentCount == 0)
			return true;
		if (parentCount != 1)
			cout << "\t[WARNING] Found multiple parents for dag node: "
					 << fnDagNode.partialPathName() << endl;
		MObject parent = fnDagNode.parent(0, &status);
		if (status != MStatus::kSuccess) {
			cout << "\t[WARNING] Unable to retrieve parent for dag node: "
				 << fnDagNode.partialPathName() << endl;
			return false;
		}
		MFnDagNode parentFn(parent);
		return _isVisible(parentFn, status);
	}

	bool MeshGenerator::_getLayerVisibility(const MFnDependencyNode &fnDepNode, MStatus &status)
	{
		bool bVisible = false;
		MPlug visPlug = fnDepNode.findPlug("drawOverride", &status);
		if (status != MStatus::kSuccess)
			cout << "\t[WARNING] can not find \"drawOverride\" plug, returning false\n";
		MPlugArray connections;
		visPlug.connectedTo(connections, true, false, &status);
		if (status != MStatus::kSuccess)
			cout << "\t[WARNING] can not find connections\n";
		int connCount = connections.length();
		for (int i = 0; i < connCount; ++i) {
			MPlug plug = connections[i];
			MObject node = plug.node();
			if (node.hasFn(MFn::kDisplayLayer)) {
				MFnDependencyNode layerDepNode(node);
				return _getBooleanAttr(layerDepNode, "visibility", status);
			}
		}
		cout << "\t[INFO] Object not a member of a displayLayer - returning true for visibility" << endl;
		return true;
	}

	//	--------------------------------------------------------------------------
	/** Determines if a given node has the attribute set.
		\param		fnDepNode
					Dependency node to check
		\param		status
					Status code from Maya API

		\return		True if node's attribute is set to true
					False if node's attribute is false or if
					unable to determine the value
	*/	
	bool MeshGenerator::_getBooleanAttr(const MFnDependencyNode &fnDepNode,
										const std::string &attrName,
										MStatus &status)
	{
		bool bValue = false;
		const MPlug valuePlug = fnDepNode.findPlug(attrName.c_str(), &status);
		if (status != MStatus::kSuccess) {
			cout << "\t[WARNING] can not find \"" << attrName << "\" plug, returning false\n";
		} else {
			status = valuePlug.getValue(bValue);
			if (status != MStatus::kSuccess) {
				bValue = false;
				cout << "\t[WARNING] can not query \"" << attrName << "\" plug, returning false\n";
			}
		}
		return bValue;
	}

	//	--------------------------------------------------------------------------
	/** Convert an array of object-relative vertex indices into an array of face-
		relative vertex indices. Required because MItMeshPolygon::getTriangle()
		returns object-relative vertex indices, whereas many other methods require
		face-relative vertex indices.

		Adapted from "How do I write a polygon mesh exporter?" at URL 
		http://www.ewertb.com/maya/api/api_a18.html.

		\param		iterPoly
					Reference to a polygon iterator that is currently at the 
					polygon of interest
		\param		objIndices
					Reference to array of object-relative indices
		\param		faceIndices
					Reference to array of face-relative indices (output). Indices
					are appended to the end of this array. A value of -1 will be 
					appended if there is no corresponding vertex.
	*/	
	//	--------------------------------------------------------------------------
	void MeshGenerator::_convertObjectToFace(
		MItMeshPolygon &iterPoly, 
		MIntArray &objIndices, 
		MIntArray &faceIndices
	) {
		MIntArray polyIndices;
		iterPoly.getVertices(polyIndices);
			
		bool bMatched;
		unsigned int iPoly, iObj;
		for (iObj=0; iObj < objIndices.length(); ++iObj)
		{
			bMatched = false;

			// iPoly is face-relative vertex index
			for (iPoly=0; iPoly < polyIndices.length(); ++iPoly)
				if (objIndices[iObj] == polyIndices[iPoly]) {
					faceIndices.append(iPoly);
					bMatched = true;
					break;
				}

			// default if no match found
			if (!bMatched)
				faceIndices.append(-1);
		}
	}
	
	void MeshGenerator::_getBoneAssignmentMatrix(MMatrix *rMatrix, const VertexBoneAssignmentList &vbaList) const {
		for (int row = 0; row < 4; ++row)
			for (int col = 0; col < 4; ++col)
				(*rMatrix)(row, col) = 0.0f;
		VertexBoneAssignmentList::const_iterator iter;
		VertexBoneAssignmentList::const_iterator iter_end = vbaList.end();
		for (iter = vbaList.begin(); iter != iter_end; ++iter) {
			const SkeletonJoint *j = _getSkeletonJoint((*iter).boneId);
			*rMatrix += (*iter).weight * j->worldMatrix;
		}
	}

	void MeshGenerator::_getBoneAssignmentMatrix2(MMatrix *rMatrix, const VertexBoneAssignmentList &vbaList) const {
		for (int row = 0; row < 4; ++row)
			for (int col = 0; col < 4; ++col)
				(*rMatrix)(row, col) = 0.0f;
		VertexBoneAssignmentList::const_iterator iter;
		VertexBoneAssignmentList::const_iterator iter_end = vbaList.end();
		for (iter = vbaList.begin(); iter != iter_end; ++iter) {
			const SkeletonJoint *j = _getSkeletonJoint((*iter).boneId);
			*rMatrix += (*iter).weight * _getTransformTree(j);
		}
	}


} // namespace OgreMaya
