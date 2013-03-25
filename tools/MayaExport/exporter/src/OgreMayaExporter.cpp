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
#include "ogremayaexporter.h"

#include "OgreMayaMesh.h"
#include "OgreMayaSkeleton.h"
#include "OgreMayaMaterial.h"
#include "OgreMayaOptions.h"
#include <maya/MArgDatabase.h>

#include <maya/MDagPath.h>
#include <maya/MGlobal.h>
#include <maya/MPlug.h>
#include <maya/MFnSet.h>
#include <maya/MItDependencyGraph.h>
#include <maya/MItDag.h>

// General flags.
const char *exportName = "-n", *exportNameLongFlag = "-name";
const char *exportDir = "-dir", *exportLongDir = "-directory";
const char *animFlag = "-ani", *animLongFlag = "-animation";
const char *helpFlag = "-h", *helpLongFlag = "-help";
const char *helpText = "Ogre Exporter, by Jeremy \"DrEvil\" Swigart.";

// Export options
const char *exportMesh = "-msh", *exportLongMesh = "-exportmesh";
const char *exportBinaryMesh = "-bin", *exportLongBinaryMesh = "-exportbinarymesh";
const char *exportBinarySkel = "-bsk", *exportLongBinarySkel = "-exportbinaryskel";
const char *exportMeshNames = "-sn", *exportLongMeshNames = "-exportmeshnames";
const char *exportMaterial = "-mat", *exportLongMaterial = "-exportmaterial";
const char *exportSkeleton = "-skl", *exportLongSkeleton = "-exportskeleton";
const char *exportNormals = "-nrm", *exportLongNormals = "-exportnormals";
const char *exportUVs = "-uvs", *exportLongUVs = "-exportuvs";
const char *exportColor = "-col", *exportLongColors = "-exportcolors";
const char *exportVBA = "-vba", *exportLongVBA = "-exportvba";

// Advanced flags
const char *uvFlipFlag = "-fuv", *uvFlipLongFlag = "-flipUV";
const char *prevFlag = "-p", *prevLongFlag = "-preview";

using namespace OgreMaya;

MSyntax OgreMayaExporter::exporterSyntax() {
	MSyntax syntax;
	// Add the command line flags to the syntax.
	syntax.addFlag(exportName, exportNameLongFlag, MSyntax::kString);
	syntax.addFlag(exportDir, exportLongDir, MSyntax::kString);
	syntax.addFlag(animFlag, animLongFlag, MSyntax::kString);
	if(syntax.makeFlagMultiUse(animFlag) == MStatus::kFailure)
		MGlobal::displayError("Unable to set multi use flag.");
	syntax.addFlag(exportMesh, exportLongMesh, MSyntax::kBoolean);
	syntax.addFlag(exportBinaryMesh, exportLongBinaryMesh, MSyntax::kBoolean);
	syntax.addFlag(exportBinarySkel, exportLongBinarySkel, MSyntax::kBoolean);
	syntax.addFlag(exportMeshNames, exportLongMeshNames, MSyntax::kBoolean);
	syntax.addFlag(exportMaterial, exportLongMaterial, MSyntax::kBoolean);
	syntax.addFlag(exportSkeleton, exportLongSkeleton, MSyntax::kBoolean);
	syntax.addFlag(exportNormals, exportLongNormals, MSyntax::kBoolean);
	syntax.addFlag(exportUVs, exportLongUVs, MSyntax::kBoolean);
	syntax.addFlag(exportColor, exportLongColors, MSyntax::kBoolean);
	syntax.addFlag(exportVBA, exportLongVBA, MSyntax::kBoolean);
	syntax.addFlag(prevFlag, prevLongFlag, MSyntax::kString);

	syntax.addFlag(uvFlipFlag, uvFlipLongFlag, MSyntax::kBoolean);
	return syntax;
}

// creator function
void *OgreMayaExporter::creator( void ) {
	return new OgreMayaExporter;
}

MStatus OgreMayaExporter::doIt( const MArgList &args ) {
	// Get the command line info.
	MArgDatabase argData(syntax(), args);

	// Display help info.
	if(argData.isFlagSet(helpFlag))	{
		MGlobal::displayInfo(helpText);
		return MStatus::kSuccess;
	}


	MString exportname, exportpath, previewFolder;

	// Get the arguments.
	if(argData.isFlagSet(exportName))
		argData.getFlagArgument(exportName, 0, exportname);	
	if(argData.isFlagSet(exportDir))
		argData.getFlagArgument(exportDir, 0, exportpath);
	/*
	if(argData.isFlagSet(uvFlipFlag))		
		argData.getFlagArgument(uvFlipFlag, 0, OPTIONS.flipUV);
	*/
	if(argData.isFlagSet(exportMesh))
		argData.getFlagArgument(exportMesh, 0, OPTIONS.exportMesh);
	/*
	if(argData.isFlagSet(exportMeshNames))
		argData.getFlagArgument(exportMeshNames, 0, OPTIONS.exportMeshNames);
	*/
	if(argData.isFlagSet(exportMaterial))
		argData.getFlagArgument(exportMaterial, 0, OPTIONS.exportMaterial);
	if(argData.isFlagSet(exportSkeleton))
		argData.getFlagArgument(exportSkeleton, 0, OPTIONS.exportSkeleton);
	if(argData.isFlagSet(exportNormals))
		argData.getFlagArgument(exportNormals, 0, OPTIONS.exportNormals);
	if(argData.isFlagSet(exportUVs))
		argData.getFlagArgument(exportUVs, 0, OPTIONS.exportUVs);
	if(argData.isFlagSet(exportColor))
		argData.getFlagArgument(exportColor, 0, OPTIONS.exportColours);
	if(argData.isFlagSet(exportVBA))
		argData.getFlagArgument(exportVBA, 0, OPTIONS.exportVBA);
	/*
	if(argData.isFlagSet(exportBinaryMesh))
		argData.getFlagArgument(exportBinaryMesh, 0, OPTIONS.exportBinaryMesh);
	if(argData.isFlagSet(exportBinarySkel))
		argData.getFlagArgument(exportBinarySkel, 0, OPTIONS.exportBinarySkel);
	*/
	if(argData.isFlagSet(prevFlag))
		argData.getFlagArgument(prevFlag, 0, previewFolder);

	/*
	OPTIONS.exportName = exportname.asChar();
	OPTIONS.exportPath = exportpath.asChar();	
	OPTIONS.previewFolder = previewFolder.asChar();
	OPTIONS.outMeshFile = exportOptions.exportPath + exportOptions.exportName + ".mesh.xml";
	OPTIONS.outSkelFile = exportOptions.exportPath + exportOptions.exportName + ".skeleton.xml";
	OPTIONS.outMatFile = exportOptions.exportPath + exportOptions.exportName + ".material";
	*/
	OPTIONS.outMeshFile = (exportpath + exportname + ".mesh.xml").asChar();
	OPTIONS.outSkelFile = (exportpath + exportname + ".skeleton.xml").asChar();
	OPTIONS.outMatFile  = (exportpath + exportname + ".material").asChar();


	OPTIONS.animations.clear();
	// Get the number of animations sent in from the command line.
	unsigned int iNumAnimations = argData.numberOfFlagUses(animFlag);

	if(argData.isFlagSet(animFlag)) {		
		MString animString;
		if(argData.getFlagArgument(animFlag, 0, animString) == MStatus::kSuccess) {
			// Split the arg into pieces.
			MStringArray animArgs;
			animArgs.clear();
			animString.split(' ', animArgs);

			// Count the pieces
			int iNumArgs = animArgs.length();

			if(iNumArgs % 4 != 0) {
				MGlobal::displayError("Invalid animation argument syntax(iNumArgs % 3).");
			}
			else {
				// Loop through the arguments 4 at a time
				for(int i = 0; i < iNumArgs/4; i++)	{
					MString animName = animArgs[i*4];
					std::string temp = animName.asChar();
					unsigned int startFrame = atoi(animArgs[i*4+1].asChar());
					unsigned int endFrame = atoi(animArgs[i*4+2].asChar());
					unsigned int stepFrame = atoi(animArgs[i*4+3].asChar());

					// Make sure the from is less than the to
					if(startFrame >= endFrame)
					{
						MGlobal::displayError("Invalid animation argument syntax(from >= to).");
						continue;
					}

					if(stepFrame == 0)
					{
						MGlobal::displayError("Invalid animation argument syntax(step == 0).");
						continue;
					}

					// Otherwise add it to the animation list.
					OPTIONS.animations[animName.asChar()].from = startFrame;
					OPTIONS.animations[animName.asChar()].to   = endFrame;
					OPTIONS.animations[animName.asChar()].step = stepFrame;

					MGlobal::displayInfo("Added animation to queue");

				}
			}
		} 
		else {
			MGlobal::displayError("Error in animation syntax");
		}
	} 
	else {
		MGlobal::displayInfo("No animation flags used");
	}

	OPTIONS.debugOutput();

	MeshGenerator     meshGen;
	SkeletonGenerator skelGen;
	MatGenerator      matGen;

	if(OPTIONS.exportSkeleton) {
		MGlobal::displayInfo("Attempting to export skeleton");
		if(!skelGen.exportAll()) {
			MGlobal::displayError("Skeleton.exportAll FAILED.");
			return MStatus::kFailure;
		} 
		else {
			MGlobal::displayInfo("Skeleton Exported");
		}
	}	
	
	if(OPTIONS.exportMesh) {
		MGlobal::displayInfo("Attempting to export mesh");
		if(!meshGen.exportAll()) {
			MGlobal::displayError("Mesh.exportAll FAILED.");
			return MStatus::kFailure;
		} 
		else {
			MGlobal::displayInfo("Mesh Exported");
		}
	}
	
	if(OPTIONS.exportMaterial) {
		MGlobal::displayInfo("Attempting to export material");
		if(!matGen.exportAll()) {
			MGlobal::displayError("Material.exportAll FAILED.");
			return MStatus::kFailure;
		} 
		else {
			MGlobal::displayInfo("Material Exported");
		}
	}

	MGlobal::displayInfo("Export Successful");
	return MStatus::kSuccess;
}
