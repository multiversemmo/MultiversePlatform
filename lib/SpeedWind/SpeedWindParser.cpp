///////////////////////////////////////////////////////////////////////  
//	SpeedWindParser.cpp
//
//	(c) 2004 IDV, Inc.
//
//	This class computes wind matrices suitable for use with SpeedTreeRT.
//
//
//	*** INTERACTIVE DATA VISUALIZATION (IDV) PROPRIETARY INFORMATION ***
//
//	This software is supplied under the terms of a license agreement or
//	nondisclosure agreement with Interactive Data Visualization and may
//	not be copied or disclosed except in accordance with the terms of
//	that agreement.
//
//      Copyright (c) 2001-2004 IDV, Inc.
//      All Rights Reserved.
//
//		IDV, Inc.
//		1233 Washington St. Suite 610
//		Columbia, SC 29201
//		Voice: (803) 799-1699
//		Fax:   (803) 931-0320
//		Web:   http://www.idvinc.com


/////////////////////////////////////////////////////////////////////////////
// Includes

#pragma warning(disable:4786)
#include "stdafx.h"
#include "SpeedWindParser.h"
#include <iostream>
#include <fstream>
#include <sstream>
using namespace std;


/////////////////////////////////////////////////////////////////////////////
// Tokens

static char* g_pszTokens[ ] = 
{
	"BranchStrengthExponent",
	"LeafStrengthExponent",
	"NumLeafAngles",
	"NumMatrices",
	"MaxBendAngle",
	"BendLowWind.p",
	"BendLowWind.i",
	"BendLowWind.d",
	"BendLowWind.a",
	"BendHighWind.p",
	"BendHighWind.i",
	"BendHighWind.d",
	"BendHighWind.a",
	"VibrationAngles.low",
	"VibrationAngles.high",
	"VibrationFrequency.low",
	"VibrationFrequency.high",
	"VibrationLowWind.p",
	"VibrationLowWind.i",
	"VibrationLowWind.d",
	"VibrationLowWind.a",
	"VibrationHighWind.p",
	"VibrationHighWind.i",
	"VibrationHighWind.d",
	"VibrationHighWind.a",
	"GustsPerMinute",
	"GustStrength.min",
	"GustStrength.max",
	"GustDuration.min",
	"GustDuration.max",
	"GustResponsiveness.p",
	"GustResponsiveness.i",
	"GustResponsiveness.d",
	"GustResponsiveness.a",
	"RockAngles.low",
	"RockAngles.high",
	"RockFrequency.low",
	"RockFrequency.high",
	"RockLowWind.p",
	"RockLowWind.i",
	"RockLowWind.d",
	"RockLowWind.a",
	"RockHighWind.p",
	"RockHighWind.i",
	"RockHighWind.d",
	"RockHighWind.a",
	"RustleAngles.low",
	"RustleAngles.high",
	"RustleFrequency.low",
	"RustleFrequency.high",
	"RustleLowWind.p",
	"RustleLowWind.i",
	"RustleLowWind.d",
	"RustleLowWind.a",
	"RustleHighWind.p",
	"RustleHighWind.i",
	"RustleHighWind.d",
	"RustleHighWind.a"
};

enum ETokens
{
	BranchStrengthExponent,
	LeafStrengthExponent,
	NumLeafAngles,
	NumMatrices,
	MaxBendAngle,
	BendLowWind_p,
	BendLowWind_i,
	BendLowWind_d,
	BendLowWind_a,
	BendHighWind_p,
	BendHighWind_i,
	BendHighWind_d,
	BendHighWind_a,
	VibrationAngles_low,
	VibrationAngles_high,
	VibrationFrequency_low,
	VibrationFrequency_high,
	VibrationLowWind_p,
	VibrationLowWind_i,
	VibrationLowWind_d,
	VibrationLowWind_a,
	VibrationHighWind_p,
	VibrationHighWind_i,
	VibrationHighWind_d,
	VibrationHighWind_a,
	GustsPerMinute,
	GustStrength_min,
	GustStrength_max,
	GustDuration_min,
	GustDuration_max,
	GustResponsiveness_p,
	GustResponsiveness_i,
	GustResponsiveness_d,
	GustResponsiveness_a,
	RockAngles_low,
	RockAngles_high,
	RockFrequency_low,
	RockFrequency_high,
	RockLowWind_p,
	RockLowWind_i,
	RockLowWind_d,
	RockLowWind_a,
	RockHighWind_p,
	RockHighWind_i,
	RockHighWind_d,
	RockHighWind_a,
	RustleAngles_low,
	RustleAngles_high,
	RustleFrequency_low,
	RustleFrequency_high,
	RustleLowWind_p,
	RustleLowWind_i,
	RustleLowWind_d,
	RustleLowWind_a,
	RustleHighWind_p,
	RustleHighWind_i,
	RustleHighWind_d,
	RustleHighWind_a,
	NumTokens
};


/////////////////////////////////////////////////////////////////////////////
// CSpeedWindParser::CSpeedWindParser

CSpeedWindParser::CSpeedWindParser(void) :
	m_uiLineNumber(0)
{
	// build the map
	for (int i = 0; i < NumTokens; ++i)
		m_mTokenMap[string(g_pszTokens[i])] = i;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWindParser::ParseFile

bool CSpeedWindParser::Parse(std::istream& isData)
{
	bool bSuccess = false;

	if (isData)
	{
		bSuccess = true;
		while (!isData.eof( ))
		{
			++m_uiLineNumber;
			char szBuffer[2048];
			isData.getline(szBuffer, 2047);

			if (szBuffer[0] != '[' && szBuffer[0] != '\0' && szBuffer[0] != ' ')
			{
				string strLine(szBuffer);

				int nPos = static_cast<int>(strLine.find_first_of('=', 0));
				if (nPos != -1)
				{
					string strToken = strLine.substr(0, nPos);
					string strValue = strLine.substr(nPos + 1, strLine.size( ));
					if (!ParseToken(strToken, strValue))
					{
						AddWarning("Unrecognized token", strLine);
					}
				}
				else
				{
					AddWarning("Unrecognized token", strLine);
				}
			}
		}
	}
	else
	{
		m_strError = "Empty data stream";
	}

	if (bSuccess)
		bSuccess = m_strWarnings.length( ) == 0;

	return bSuccess;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWindParser::ParseToken

bool CSpeedWindParser::ParseToken(std::string strToken, std::string strValue)
{
	bool bSuccess = true;
	map<std::string, int>::const_iterator iMap = m_mTokenMap.find(strToken);

	if (iMap == m_mTokenMap.end( ))
		return false;

	switch (iMap->second)
	{
	case BranchStrengthExponent:
		m_sAttributes.m_fStrengthAdjustmentExponent = static_cast<float>(atof(strValue.c_str( )));
		break;
	case LeafStrengthExponent:
		m_sAttributes.m_fLeafStrengthExponent = static_cast<float>(atof(strValue.c_str( )));
		break;
	case NumLeafAngles:
		m_sAttributes.m_uiNumLeafAngles = atoi(strValue.c_str( ));
		//m_sAttributes.m_uiNumLeafAngles = c_nNumLeafAngleMatrices;//atoi(strValue.c_str( ));
		break;
	case NumMatrices:
		m_sAttributes.m_uiNumMatrices = atoi(strValue.c_str( ));
		//m_sAttributes.m_uiNumMatrices = c_nNumMatricesPerTree; //atoi(strValue.c_str( ));
		break;
	case MaxBendAngle:
		m_sAttributes.m_fMaxBendAngle = static_cast<float>(atof(strValue.c_str( )));
		break;
	case BendLowWind_p:
		m_sAttributes.m_afBendLowWindControl[0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case BendLowWind_i:
		m_sAttributes.m_afBendLowWindControl[1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case BendLowWind_d:
		m_sAttributes.m_afBendLowWindControl[2] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case BendLowWind_a:
		m_sAttributes.m_afBendLowWindControl[3] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case BendHighWind_p:
		m_sAttributes.m_afBendHighWindControl[0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case BendHighWind_i:
		m_sAttributes.m_afBendHighWindControl[1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case BendHighWind_d:
		m_sAttributes.m_afBendHighWindControl[2] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case BendHighWind_a:
		m_sAttributes.m_afBendHighWindControl[3] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationAngles_low:
		m_sAttributes.m_afVibrationAngles[0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationAngles_high:
		m_sAttributes.m_afVibrationAngles[1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationFrequency_low:
		m_sAttributes.m_afVibrationFrequency[0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationFrequency_high:
		m_sAttributes.m_afVibrationFrequency[1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationLowWind_p:
		m_sAttributes.m_afVibrationLowWindControl[0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationLowWind_i:
		m_sAttributes.m_afVibrationLowWindControl[1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationLowWind_d:
		m_sAttributes.m_afVibrationLowWindControl[2] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationLowWind_a:
		m_sAttributes.m_afVibrationLowWindControl[3] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationHighWind_p:
		m_sAttributes.m_afVibrationHighWindControl[0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationHighWind_i:
		m_sAttributes.m_afVibrationHighWindControl[1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationHighWind_d:
		m_sAttributes.m_afVibrationHighWindControl[2] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case VibrationHighWind_a:
		m_sAttributes.m_afVibrationHighWindControl[3] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case GustsPerMinute:
		m_sAttributes.m_fGustFrequency = static_cast<float>(atof(strValue.c_str( )));
		break;
	case GustStrength_min:
		m_sAttributes.m_afGustStrength[0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case GustStrength_max:
		m_sAttributes.m_afGustStrength[1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case GustDuration_min:
		m_sAttributes.m_afGustDuration[0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case GustDuration_max:
		m_sAttributes.m_afGustDuration[1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case GustResponsiveness_p:
		m_sAttributes.m_afGustControl[0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case GustResponsiveness_i:
		m_sAttributes.m_afGustControl[1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case GustResponsiveness_d:
		m_sAttributes.m_afGustControl[2] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case GustResponsiveness_a:
		m_sAttributes.m_afGustControl[3] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockAngles_low:
		m_sAttributes.m_afLeafAngleAngles[CSpeedWind::ROCK][0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockAngles_high:
		m_sAttributes.m_afLeafAngleAngles[CSpeedWind::ROCK][1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockFrequency_low:
		m_sAttributes.m_afLeafAngleFrequency[CSpeedWind::ROCK][0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockFrequency_high:
		m_sAttributes.m_afLeafAngleFrequency[CSpeedWind::ROCK][1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockLowWind_p:
		m_sAttributes.m_afLeafAngleLowWindControl[CSpeedWind::ROCK][0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockLowWind_i:
		m_sAttributes.m_afLeafAngleLowWindControl[CSpeedWind::ROCK][1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockLowWind_d:
		m_sAttributes.m_afLeafAngleLowWindControl[CSpeedWind::ROCK][2] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockLowWind_a:
		m_sAttributes.m_afLeafAngleLowWindControl[CSpeedWind::ROCK][3] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockHighWind_p:
		m_sAttributes.m_afLeafAngleHighWindControl[CSpeedWind::ROCK][0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockHighWind_i:
		m_sAttributes.m_afLeafAngleHighWindControl[CSpeedWind::ROCK][1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockHighWind_d:
		m_sAttributes.m_afLeafAngleHighWindControl[CSpeedWind::ROCK][2] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RockHighWind_a:
		m_sAttributes.m_afLeafAngleHighWindControl[CSpeedWind::ROCK][3] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleAngles_low:
		m_sAttributes.m_afLeafAngleAngles[CSpeedWind::RUSTLE][0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleAngles_high:
		m_sAttributes.m_afLeafAngleAngles[CSpeedWind::RUSTLE][1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleFrequency_low:
		m_sAttributes.m_afLeafAngleFrequency[CSpeedWind::RUSTLE][0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleFrequency_high:
		m_sAttributes.m_afLeafAngleFrequency[CSpeedWind::RUSTLE][1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleLowWind_p:
		m_sAttributes.m_afLeafAngleLowWindControl[CSpeedWind::RUSTLE][0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleLowWind_i:
		m_sAttributes.m_afLeafAngleLowWindControl[CSpeedWind::RUSTLE][1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleLowWind_d:
		m_sAttributes.m_afLeafAngleLowWindControl[CSpeedWind::RUSTLE][2] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleLowWind_a:
		m_sAttributes.m_afLeafAngleLowWindControl[CSpeedWind::RUSTLE][3] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleHighWind_p:
		m_sAttributes.m_afLeafAngleHighWindControl[CSpeedWind::RUSTLE][0] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleHighWind_i:
		m_sAttributes.m_afLeafAngleHighWindControl[CSpeedWind::RUSTLE][1] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleHighWind_d:
		m_sAttributes.m_afLeafAngleHighWindControl[CSpeedWind::RUSTLE][2] = static_cast<float>(atof(strValue.c_str( )));
		break;
	case RustleHighWind_a:
		m_sAttributes.m_afLeafAngleHighWindControl[CSpeedWind::RUSTLE][3] = static_cast<float>(atof(strValue.c_str( )));
		break;
	default:
		bSuccess = false;
		break;
	}

	return bSuccess;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWindParser::AddWarning

void CSpeedWindParser::AddWarning(std::string strWarning, std::string strLine)
{
	ostringstream ossTemp;
	ossTemp << strWarning << " [line " << m_uiLineNumber << ", '" << strLine << "']" << endl;
	m_strWarnings += ossTemp.str( );
}
