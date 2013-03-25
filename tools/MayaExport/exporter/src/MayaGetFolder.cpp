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
#include "mayagetfolder.h"
#include <shlobj.h>

// creator function
void *CMayaGetFolder::creator( void )
{
	return new CMayaGetFolder;
}

MStatus CMayaGetFolder::doIt( const MArgList &args )
{
	BROWSEINFO browseInfo = 
	{
		NULL,
		NULL,
		NULL,
		"Select folder",
		NULL,
		NULL,
		NULL
	};
	LPITEMIDLIST itemList = SHBrowseForFolder(&browseInfo);	
	if(itemList)
	{
		char folder[MAX_PATH];
		SHGetPathFromIDList(itemList, folder);
		setResult(folder);
	}

	return MStatus::kSuccess;
}