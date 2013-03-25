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
// add needed includes
#include <maya/MObject.h>
#include <maya/MStatus.h>
#include <maya/MGlobal.h>
#include <maya/MFnPlugin.h>
#include "OgreMayaExporter.h"
#include "MayaGetFolder.h"

// entry point
MStatus initializePlugin( MObject obj )
{
	const char *vendor = "Jeremy \"DrEvil\" Swigart";
	const char *version = "Any";

	MFnPlugin pluginFn(obj, vendor, version);

	// Register the export command.
	pluginFn.registerCommand("OgreExport", OgreMayaExporter::creator, OgreMayaExporter::exporterSyntax);
	pluginFn.registerCommand("OgreGetFolder", CMayaGetFolder::creator);

	// Create the Menu item
	MGlobal::executeCommand("source \"OgreExporter.mel\"; setParent \"MayaWindow\"; menu -label \"Ogre Export\" -tearOff false ogreExportMenu; menuItem -label \"Export\" -command \"OgreExporter\";");	

	return MStatus::kSuccess;
}

MStatus uninitializePlugin( MObject obj )
{
	MStatus status;
	MFnPlugin pluginFn( obj );

	// Remove the menu item.
	MGlobal::executeCommand("deleteUI -m ogreExportMenu;");

	// deregister command
	status = pluginFn.deregisterCommand("OgreExport");

	return status;
}