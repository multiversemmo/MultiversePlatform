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
#include "OgreMayaMaterial.h"
#include "OgreMayaOptions.h"

#include <maya/MDagPath.h>
#include <maya/MGlobal.h>
#include <maya/MPlug.h>
#include <maya/MFnLambertShader.h>
#include <maya/MFnBlinnShader.h>
#include <maya/MFnPhongShader.h>
#include <maya/MFnReflectShader.h>
#include <maya/MFnSet.h>
#include <maya/MPlugArray.h>
#include <maya/MItDependencyGraph.h>
#include <maya/MItDag.h>

#include <iostream>

namespace OgreMaya {

	using namespace std;

	//	--------------------------------------------------------------------------
	/** Standard constructor. Creates Ogre MaterialManger and defines known options.
	*/	
	//	--------------------------------------------------------------------------
	MatGenerator::MatGenerator() {
	}


	//	--------------------------------------------------------------------------
	/** Destructor.
	*/	
	//	--------------------------------------------------------------------------
	MatGenerator::~MatGenerator() {
	}


	//	--------------------------------------------------------------------------
	/** Finds and exports all connected materials in the Maya scene.
		\return		True if exported ok, false otherwise
	*/	
	//	--------------------------------------------------------------------------
	bool MatGenerator::exportAll() {

        cout << "\nMatGenerator::exportAll\n";

		MStatus status;
		bool bStatus = true;

        _extractMaterials();

        {            
            // export
            
			ofstream out(OPTIONS.outMatFile.c_str());

            out.precision(5);
            out.setf(ios::fixed);

            vector<Material*>::iterator it  = materials.begin();
            vector<Material*>::iterator end = materials.end();
            
            for(;it!=end; ++it) {
                
                Material& mat = **it;

                list<TextureUnitState>::iterator tlIt  = mat.textureLayers.begin();
                list<TextureUnitState>::iterator tlEnd = mat.textureLayers.end();
                
                out << "material " << mat.name << '\n';
                out << "{\n";   

				out << "\ttechnique\n";
				out << "\t{\n";

				out << "\t\tpass\n";
				out << "\t\t{\n";

                out << "\t\t\tshading " << mat.shadingMode << "\n\n";

                out << "\t\t\tambient "
                    << mat.ambient.r << ' '
                    << mat.ambient.g << ' '
                    << mat.ambient.b << ' '
                    << mat.ambient.a << '\n';

                out << "\t\t\tdiffuse "
                    << mat.diffuse.r << ' '
                    << mat.diffuse.g << ' '
                    << mat.diffuse.b << ' '
                    << mat.diffuse.a << '\n';

                out << "\t\t\tspecular "
                    << mat.specular.r << ' '
                    << mat.specular.g << ' '
                    << mat.specular.b << ' '
                    << mat.specular.a << ' '
					<< mat.shininess  << '\n';

                out << "\t\t\temissive "
                    << mat.selfIllumination.r << ' '
                    << mat.selfIllumination.g << ' '
                    << mat.selfIllumination.b << ' '
                    << mat.selfIllumination.a << "\n\n";

                for(;tlIt!=tlEnd; ++tlIt) {
                    TextureUnitState& layer = *tlIt;
                    out << "\t\t\ttexture_unit\n";
					out << "\t\t\t{\n";
                    out << "\t\t\t\ttexture " << layer.textureName << '\n';
                    out << "\t\t\t\ttex_coord_set " << layer.uvSet << '\n';
                    out << "\t\t\t}\n\n";
                }

				out << "\t\t}\n";
				out << "\t}\n";
                out << "}\n\n";
            }                        

            out.close();            
        }

        deleteAll(materials.begin(), materials.end());
        materials.clear();

		return true;
	}



	bool MatGenerator::_extractMaterials() {
		MStatus status;
		MItDag dagIter( MItDag::kBreadthFirst, MFn::kInvalid, &status );

		for ( ; !dagIter.isDone(); dagIter.next()) {
			MDagPath dagPath;
			status = dagIter.getPath( dagPath );

			if (status) {
				MFnDagNode dagNode( dagPath, &status );

				if(
                    dagNode.isIntermediateObject()
				    || !dagPath.hasFn( MFn::kMesh )
				    || dagPath.hasFn( MFn::kTransform )
                )
                    continue;

				MFnMesh fnMesh( dagPath );

				// Get all connected shaders (materials)
				MObjectArray ShaderSets;
				MIntArray    ShaderVertexIndices;
				unsigned int iInstance = dagPath.instanceNumber();
				fnMesh.getConnectedShaders(iInstance, ShaderSets, ShaderVertexIndices);

				// Iterate over all connected shaders
				unsigned int iShader;
				for (iShader = 0; iShader < ShaderSets.length(); ++iShader) {
					_makeMaterials(ShaderSets[iShader]);
				}
			}
		}

		return (status == MStatus::kSuccess);
	}


	void MatGenerator::_makeMaterials(MObject &ShaderSet) {
		MFnDependencyNode fnDNSet(ShaderSet);
		MPlug             ShaderPlug = fnDNSet.findPlug("surfaceShader");
		MPlugArray        ShaderPlugArray;
		ShaderPlug.connectedTo(ShaderPlugArray, true, false);

		unsigned int iPlug, nPlugs;
		nPlugs = ShaderPlugArray.length();
		for (iPlug=0; iPlug < nPlugs; ++iPlug) {
			Material *mat = NULL;			
			
			// Basic material properties
			MObject ShaderNode = ShaderPlugArray[iPlug].node();
			if (ShaderNode.hasFn(MFn::kPhong)) {
				mat = _makePhongMaterial(ShaderNode);
			}
			else if (ShaderNode.hasFn(MFn::kBlinn)) {
				mat = _makeBlinnMaterial(ShaderNode);
			}
			else if (ShaderNode.hasFn(MFn::kLambert)) {
				mat = _makeLambertMaterial(ShaderNode);
			}
			else {
				MFnDependencyNode FnShader(ShaderNode);
				cout << "\tunable to create Ogre material for shader " << FnShader.name().asChar() << '\n';
			}

			// Check for duplicates
			if (mat) {
				vector<Material*>::iterator iterMat;
				iterMat = materials.begin();
				for(;iterMat != materials.end(); ++iterMat) {
					if ((*iterMat)->name == mat->name) {
						delete mat;
						mat = NULL;
						break;
					}
				}
			}

			// Textures
			if (mat) {
                materials.push_back(mat);

				MFnDependencyNode ShaderFn(ShaderPlugArray[iPlug].node());
				MPlug thePlug = ShaderPlugArray[iPlug];
				MItDependencyGraph ItShaderGraph(thePlug, 
					                             MFn::kFileTexture, 
										         MItDependencyGraph::kUpstream);
				int iTexCoordSet = 0;
				while (!ItShaderGraph.isDone()) {
					MObject ShaderTexture = ItShaderGraph.thisNode();
					MFnDependencyNode FnTexture(ShaderTexture);

                    MString textureFile;
                    FnTexture.findPlug("fileTextureName").getValue(textureFile);

                    int substrI;
                    substrI = textureFile.rindex('\\');
                    if(substrI<0)
                        substrI = textureFile.rindex('/');

                    if(substrI>0)
                        textureFile = textureFile.substring(substrI+1, textureFile.length()-1);

		    		mat->textureLayers.push_back(
                        TextureUnitState(textureFile.asChar(), iTexCoordSet)
                    );
			          
                    ItShaderGraph.next();
					++iTexCoordSet;
				}
			}
		}
	}

	Material* MatGenerator::_makePhongMaterial(MObject &ShaderNode) {
		Material *mat = new Material;

		MFnPhongShader FnShader(ShaderNode);
		        
		mat->name = OPTIONS.matPrefix + FnShader.name().asChar();

		mat->shadingMode = "phong";

#ifdef USE_MAYA_COLOR
		mat->ambient.r = FnShader.ambientColor().r;
        mat->ambient.g = FnShader.ambientColor().g;
		mat->ambient.b = FnShader.ambientColor().b;
        mat->ambient.a = FnShader.ambientColor().a;

		mat->diffuse.r = FnShader.diffuseCoeff()*FnShader.color().r;
        mat->diffuse.g = FnShader.diffuseCoeff()*FnShader.color().g;
		mat->diffuse.b = FnShader.diffuseCoeff()*FnShader.color().b;
        mat->diffuse.a = FnShader.diffuseCoeff()*FnShader.color().a;
#else
		mat->ambient.r = 0.8f;
        mat->ambient.g = 0.8f;
		mat->ambient.b = 0.8f;
        mat->ambient.a = 1.0f;

		mat->diffuse.r = 1.0f;
        mat->diffuse.g = 1.0f;
		mat->diffuse.b = 1.0f;
        mat->diffuse.a = 1.0f;
#endif

		mat->specular.r = FnShader.specularColor().r;
		mat->specular.g = FnShader.specularColor().g;
        mat->specular.b = FnShader.specularColor().b;        
        mat->specular.a = FnShader.specularColor().a;        

        mat->selfIllumination.r = FnShader.incandescence().r;
		mat->selfIllumination.g = FnShader.incandescence().g;
        mat->selfIllumination.b = FnShader.incandescence().b;        
        mat->selfIllumination.a = FnShader.incandescence().a;        
		
		mat->shininess = FnShader.cosPower();
			
		cout << "\tCreated phong material " << mat->name << '\n';
		return mat;
	}


	Material* MatGenerator::_makeBlinnMaterial(MObject &ShaderNode) {
		Material* mat = new Material;

		MFnBlinnShader FnShader(ShaderNode);
		
		mat->name = OPTIONS.matPrefix + FnShader.name().asChar();
		
		mat->shadingMode = "gouraud";

#ifdef USE_MAYA_COLOR
		mat->ambient.r = FnShader.ambientColor().r;
        mat->ambient.g = FnShader.ambientColor().g;
		mat->ambient.b = FnShader.ambientColor().b;
        mat->ambient.a = FnShader.ambientColor().a;

		mat->diffuse.r = FnShader.diffuseCoeff()*FnShader.color().r;
        mat->diffuse.g = FnShader.diffuseCoeff()*FnShader.color().g;
		mat->diffuse.b = FnShader.diffuseCoeff()*FnShader.color().b;
        mat->diffuse.a = FnShader.diffuseCoeff()*FnShader.color().a;
#else
		mat->ambient.r = 0.8f;
        mat->ambient.g = 0.8f;
		mat->ambient.b = 0.8f;
        mat->ambient.a = 1.0f;

		mat->diffuse.r = 1.0f;
        mat->diffuse.g = 1.0f;
		mat->diffuse.b = 1.0f;
        mat->diffuse.a = 1.0f;
#endif

		mat->specular.r = FnShader.specularColor().r;
		mat->specular.g = FnShader.specularColor().g;
        mat->specular.b = FnShader.specularColor().b;        
        mat->specular.a = FnShader.specularColor().a;        

        mat->selfIllumination.r = FnShader.incandescence().r;
		mat->selfIllumination.g = FnShader.incandescence().g;
        mat->selfIllumination.b = FnShader.incandescence().b;        
        mat->selfIllumination.a = FnShader.incandescence().a;        
		
		mat->shininess = FnShader.specularRollOff();
			
		cout << "MatGenerator: Created blinn material " << mat->name << '\n';
		return mat;
	}


	Material* MatGenerator::_makeLambertMaterial(MObject &ShaderNode) {
		Material* mat = new Material;

		MFnLambertShader FnShader(ShaderNode);		
		
        mat->name = OPTIONS.matPrefix + FnShader.name().asChar();				

		mat->shadingMode = "gouraud";

#ifdef USE_MAYA_COLOR
		mat->ambient.r = FnShader.ambientColor().r;
        mat->ambient.g = FnShader.ambientColor().g;
		mat->ambient.b = FnShader.ambientColor().b;
        mat->ambient.a = FnShader.ambientColor().a;

		mat->diffuse.r = FnShader.diffuseCoeff()*FnShader.color().r;
        mat->diffuse.g = FnShader.diffuseCoeff()*FnShader.color().g;
		mat->diffuse.b = FnShader.diffuseCoeff()*FnShader.color().b;
        mat->diffuse.a = FnShader.diffuseCoeff()*FnShader.color().a;
#else
		mat->ambient.r = 0.8f;
        mat->ambient.g = 0.8f;
		mat->ambient.b = 0.8f;
        mat->ambient.a = 1.0f;

		mat->diffuse.r = 1.0f;
        mat->diffuse.g = 1.0f;
		mat->diffuse.b = 1.0f;
        mat->diffuse.a = 1.0f;
#endif

		mat->selfIllumination.r = FnShader.incandescence().r;
		mat->selfIllumination.g = FnShader.incandescence().g;
        mat->selfIllumination.b = FnShader.incandescence().b;        
        mat->selfIllumination.a = FnShader.incandescence().a;        
		
		mat->shininess = 0;
			
		cout << "\tCreated lambert material " << mat->name << '\n';
		return mat;
	}


	//	--------------------------------------------------------------------------
	/**	Get material name for a single mesh.
		Adapted from "How to Write a Simple Maya Model Exporter", Rafael Baptista, 
		Gamedev.net, April 2003
	*/	
	//	--------------------------------------------------------------------------
	MString MatGenerator::getMaterialName(MFnMesh &fnMesh) {
		MStatus status = MStatus::kSuccess;
		MString MaterialName = "";
		

		// ===== Connected sets and members
		// (Required to determine texturing of different faces)

		// Determine instance number
		fnMesh.dagPath().extendToShape();
		int iInstance = 0;
		if (fnMesh.dagPath().isInstanced()) {
			iInstance = fnMesh.dagPath().instanceNumber();
		}

		// Get the connected sets and members
		MObjectArray PolygonSets;
		MObjectArray PolygonComponents;
		status = fnMesh.getConnectedSetsAndMembers(iInstance, 
			                                       PolygonSets, 
												   PolygonComponents, 
												   true);
		if (!status) {
			MGlobal::displayError("MFnMesh::getConnectedSetsAndMembers"); 
			return MaterialName;
		}


		// ===== Materials
		unsigned int iSet;
		for (iSet = 0; iSet < PolygonSets.length(); ++iSet) {
			MObject PolygonSet = PolygonSets[iSet];
			MObject PolygonComponent = PolygonComponents[iSet];

			MFnDependencyNode dnSet(PolygonSet);
			MObject ssAttr = dnSet.attribute(MString("surfaceShader"));
			MPlug ssPlug(PolygonSet, ssAttr);

			MPlugArray srcPlugArray;
			ssPlug.connectedTo(srcPlugArray, true, false);
			
			if (srcPlugArray.length() > 0) {
				// This object contains a reference to a shader or material.
				// Check for known material types and extract material name.
				MObject srcNode = srcPlugArray[0].node();
				
				if (srcNode.hasFn(MFn::kPhong)) {
					MFnPhongShader fnPhong(srcNode);
					MaterialName = fnPhong.name();
				}
				else if (srcNode.hasFn(MFn::kLambert)) {
					MFnLambertShader fnLambert(srcNode);
					MaterialName = fnLambert.name();
				}
				else if (srcNode.hasFn(MFn::kBlinn)) {
					MFnBlinnShader fnBlinn(srcNode);
					MaterialName = fnBlinn.name();
				}
				else if (srcNode.hasFn(MFn::kReflect)) {
					MFnReflectShader fnReflect(srcNode);
					MaterialName = fnReflect.name();
				}

            }
		}


		// ===== Done
		return MaterialName;

	}

} // namespace OgreMaya
