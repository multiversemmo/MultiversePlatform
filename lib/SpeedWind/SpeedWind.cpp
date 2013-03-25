///////////////////////////////////////////////////////////////////////  
//	SpeedWind.cpp
//
//	(c) 2004 IDV, Inc.
//
//	This class computes wind matrices, leaf angles, and leaf angle matrices
//	suitable for use with SpeedTreeRT.
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
// Preprocessor

#pragma warning(disable:4786)
#include "stdafx.h"
#include "SpeedWind.h"
#include "SpeedTreeRT.h"
#include "SpeedWindParser.h"
#include <iostream>
#include <fstream>
#include <sstream>
using namespace std;

#define	INTERPOLATE(a, b, p)		(((b) - (a)) * (p) + (a))

//#define TEST_FILE

#ifdef TEST_FILE
static	FILE*	pFile = NULL;
#endif

/////////////////////////////////////////////////////////////////////////////
// Constants

static	const	float	c_fPi = 3.14159265358979323846f;
static	const	float	c_fHalfPi = c_fPi * 0.5f;
static	const	float	c_fQuarterPi = c_fPi * 0.25f;
static	const	float	c_fTwoPi = 2.0f * c_fPi;
static	const	float	c_fRad2Deg = 57.29578f;
static	const	float	c_fAxisScalar = 0.25f;


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::CSpeedWind

CSpeedWind::CSpeedWind(void) :
m_fTime(0.0f),
m_fDeltaTime(0.0f),
m_bResetDeltaTime(true),
m_fGustEndTime(0.0f),
m_fStrength(0.0f),
m_fAdjustedStrength(0.0f),
m_fLeafAdjustedStrength(0.0f),
m_pLeafAngleMatrices(NULL)
{
	m_cGust.SetWantedValue(0.0);
	m_cGust.SetValue(0.0);
	m_cGust.SetConstants(m_sAttributes.m_afGustControl[0], m_sAttributes.m_afGustControl[1], m_sAttributes.m_afGustControl[2], m_sAttributes.m_afGustControl[3]);

	for (unsigned int i = 0; i < CSpeedWind::NUM_LEAF_ANGLES; ++i)
		m_pLeafAngles[i] = NULL;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::CSpeedWind

CSpeedWind::CSpeedWind(const SWindAttributes& sAttributes) :
m_fTime(0.0f),
m_fDeltaTime(0.0f),
m_bResetDeltaTime(true),
m_fGustEndTime(0.0f),
m_fStrength(0.0f),
m_fAdjustedStrength(0.0f),
m_pLeafAngleMatrices(NULL)
{
	SetAttributes(sAttributes);
	m_cGust.SetConstants(m_sAttributes.m_afGustControl[0], m_sAttributes.m_afGustControl[1], m_sAttributes.m_afGustControl[2], m_sAttributes.m_afGustControl[3]);

	for (unsigned int i = 0; i < CSpeedWind::NUM_LEAF_ANGLES; ++i)
		m_pLeafAngles[i] = NULL;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::CSpeedWind

CSpeedWind::CSpeedWind(const CSpeedWind& cWind)
{
	for (unsigned int i = 0; i < CSpeedWind::NUM_LEAF_ANGLES; ++i)
		m_pLeafAngles[i] = NULL;
	m_pLeafAngleMatrices = NULL;

	*this = cWind;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::~CSpeedWind

CSpeedWind::~CSpeedWind(void)
{
	m_cGust.SetWantedValue(0.0);
	m_cGust.SetValue(0.0);
	m_cGust.SetConstants(0.1f, 0.0f, 0.0f, 0.0f);

	for (unsigned int i = 0; i < CSpeedWind::NUM_LEAF_ANGLES; ++i)
		delete[] m_pLeafAngles[i];

	delete[] m_pLeafAngleMatrices;

#ifdef TEST_FILE
	if (pFile)
		fclose(pFile);
#endif
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::operator=

CSpeedWind& CSpeedWind::operator=(const CSpeedWind& cRight)
{
	if (this != &cRight)
	{
		m_sAttributes = cRight.m_sAttributes;
		m_fTime = cRight.m_fTime;
		m_fStrength = cRight.m_fStrength;
		m_fAdjustedStrength = cRight.m_fAdjustedStrength;
		m_fLeafAdjustedStrength = cRight.m_fLeafAdjustedStrength;
		m_fDeltaTime = cRight.m_fDeltaTime;
		m_bResetDeltaTime = cRight.m_bResetDeltaTime;
		m_fBlendWeight = cRight.m_fBlendWeight;
		m_fGustEndTime = cRight.m_fGustEndTime;
		m_cGust = cRight.m_cGust;
		m_strParserError = cRight.m_strParserError;
		m_strWarnings = cRight.m_strWarnings;
		CreateWindMatrices( );
	}

	return *this;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::SetAttributes

void CSpeedWind::SetAttributes(const SWindAttributes& sAttributes)
{
	bool bComputeNewMatrices = false;
	if (sAttributes.m_uiNumMatrices != m_sAttributes.m_uiNumMatrices)
		bComputeNewMatrices = true;

	bool bComputeNewRockAngles = false;
	if (sAttributes.m_uiNumLeafAngles != m_sAttributes.m_uiNumLeafAngles)
		bComputeNewRockAngles = true;

	m_sAttributes = sAttributes;

	m_cGust.SetConstants(m_sAttributes.m_afGustControl[0], m_sAttributes.m_afGustControl[1], m_sAttributes.m_afGustControl[2], m_sAttributes.m_afGustControl[3]);

	if (bComputeNewMatrices || bComputeNewRockAngles || m_vWindMatrices.size( ) != m_sAttributes.m_uiNumMatrices)
		CreateWindMatrices( );
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::CreateWindMatrices

void CSpeedWind::CreateWindMatrices( )
{
	// wind matrices
	m_vWindMatrices.clear( );
	for (unsigned int i = 0; i < m_sAttributes.m_uiNumMatrices; ++i)
	{
		SWindMatrix sMatrix;

		sMatrix.m_cBendAngle.SetConstants(m_sAttributes.m_afBendLowWindControl[0], m_sAttributes.m_afBendLowWindControl[1], m_sAttributes.m_afBendLowWindControl[2], m_sAttributes.m_afBendLowWindControl[3]);
		sMatrix.m_cAxisAngle.SetConstants(m_sAttributes.m_afBendLowWindControl[0] * c_fAxisScalar, m_sAttributes.m_afBendLowWindControl[1] * c_fAxisScalar, m_sAttributes.m_afBendLowWindControl[2] * c_fAxisScalar, m_sAttributes.m_afBendLowWindControl[3] * c_fAxisScalar);

		sMatrix.m_cXVibration.SetConstants(m_sAttributes.m_afVibrationLowWindControl[0], m_sAttributes.m_afVibrationLowWindControl[1], m_sAttributes.m_afVibrationLowWindControl[2], m_sAttributes.m_afVibrationLowWindControl[3]);
		sMatrix.m_cYVibration.SetConstants(m_sAttributes.m_afVibrationLowWindControl[0], m_sAttributes.m_afVibrationLowWindControl[1], m_sAttributes.m_afVibrationLowWindControl[2], m_sAttributes.m_afVibrationLowWindControl[3]);
		m_vWindMatrices.push_back(sMatrix);
	}

	// leaf angles
	for (unsigned int i = 0; i < NUM_LEAF_ANGLES; ++i)
	{
		delete[] m_pLeafAngles[i];
		m_pLeafAngles[i] = new float[m_sAttributes.m_uiNumLeafAngles];
		m_avLeafAngles[i].clear( );
		for (unsigned int j = 0; j < m_sAttributes.m_uiNumLeafAngles; ++j)
		{
			SLeafAngle sLeafAngle;

			sLeafAngle.m_pResult = &m_pLeafAngles[i][j];
			sLeafAngle.m_cAngle.SetConstants(m_sAttributes.m_afLeafAngleLowWindControl[i][0], m_sAttributes.m_afLeafAngleLowWindControl[i][1], m_sAttributes.m_afLeafAngleLowWindControl[i][2], m_sAttributes.m_afLeafAngleLowWindControl[i][3]);
			sLeafAngle.m_cAngle.SetWantedValue(0.0f);
			sLeafAngle.m_cAngle.SetValue(0.0f);
			m_avLeafAngles[i].push_back(sLeafAngle);
		}
	}

	// leaf angle matrices
	delete[] m_pLeafAngleMatrices;
	m_pLeafAngleMatrices = new CSpeedWindMatrix[m_sAttributes.m_uiNumLeafAngles];

	m_bResetDeltaTime = true;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::ResetMatrices

void CSpeedWind::ResetMatrices( )
{
	// matrices
	for (unsigned int i = 0; i < m_sAttributes.m_uiNumMatrices; ++i)
	{
		SWindMatrix& sMatrix = m_vWindMatrices[i];

		sMatrix.m_cBendAngle.SetConstants(m_sAttributes.m_afBendLowWindControl[0], m_sAttributes.m_afBendLowWindControl[1], m_sAttributes.m_afBendLowWindControl[2], m_sAttributes.m_afBendLowWindControl[3]);
		sMatrix.m_cBendAngle.ResetError( );
		sMatrix.m_cBendAngle.SetWantedValue(0.0f);
		sMatrix.m_cBendAngle.SetValue(0.0f);

		sMatrix.m_cAxisAngle.SetConstants(m_sAttributes.m_afBendLowWindControl[0] * c_fAxisScalar, m_sAttributes.m_afBendLowWindControl[1] * c_fAxisScalar, m_sAttributes.m_afBendLowWindControl[2] * c_fAxisScalar, m_sAttributes.m_afBendLowWindControl[3] * c_fAxisScalar);
		sMatrix.m_cAxisAngle.ResetError( );
		sMatrix.m_cAxisAngle.SetWantedValue(0.0f);
		sMatrix.m_cAxisAngle.SetValue(0.0f);

		sMatrix.m_cXVibration.SetConstants(m_sAttributes.m_afVibrationLowWindControl[0], m_sAttributes.m_afVibrationLowWindControl[1], m_sAttributes.m_afVibrationLowWindControl[2], m_sAttributes.m_afVibrationLowWindControl[3]);
		sMatrix.m_cXVibration.ResetError( );
		sMatrix.m_cXVibration.SetWantedValue(0.0f);
		sMatrix.m_cXVibration.SetValue(0.0f);

		sMatrix.m_cYVibration.SetConstants(m_sAttributes.m_afVibrationLowWindControl[0], m_sAttributes.m_afVibrationLowWindControl[1], m_sAttributes.m_afVibrationLowWindControl[2], m_sAttributes.m_afVibrationLowWindControl[3]);
		sMatrix.m_cYVibration.ResetError( );
		sMatrix.m_cYVibration.SetWantedValue(0.0f);
		sMatrix.m_cYVibration.SetValue(0.0f);
	}

	// leaf angles
	for (unsigned int i = 0; i < NUM_LEAF_ANGLES; ++i)
	{
		for (unsigned int j = 0; j < m_sAttributes.m_uiNumLeafAngles; ++j)
		{
			SLeafAngle& sLeafAngle = m_avLeafAngles[i][j];

			sLeafAngle.m_cAngle.SetConstants(m_sAttributes.m_afLeafAngleLowWindControl[i][0], m_sAttributes.m_afLeafAngleLowWindControl[i][1], m_sAttributes.m_afLeafAngleLowWindControl[i][2], m_sAttributes.m_afLeafAngleLowWindControl[i][3]);
			sLeafAngle.m_cAngle.ResetError( );
			sLeafAngle.m_cAngle.SetWantedValue(GetRandom(-10.0f, 10.0f));
			sLeafAngle.m_cAngle.SetValue(0.0f);
		}
	}

	// gusting
	m_cGust.ResetError( );
	m_cGust.SetWantedValue(0.0f);
	m_cGust.SetValue(0.0f);

	m_bResetDeltaTime = true;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::Advance

float CSpeedWind::Advance(float fTime, float fStrength, float fDirectionX, float fDirectionY, float fDirectionZ)
{
#ifdef TEST_FILE
	if (!pFile)
	{
		pFile = fopen("c:\\tmp\\speedwind.txt", "w");
		fprintf(pFile, "time\tData\n");
	}
#endif

	m_fDeltaTime = fTime - m_fTime;
	m_fTime = fTime;

	if (m_fDeltaTime > c_dMaxAllowableDeltaTime)
		m_fDeltaTime = static_cast<float>(c_dMaxAllowableDeltaTime);

	if (m_bResetDeltaTime)
	{
		m_fDeltaTime = 0.0f;
		m_bResetDeltaTime = false;
	}

	UpdateStrength(fStrength);
	UpdateVibrations( );
	UpdateBend(fDirectionX, fDirectionY, fDirectionZ);
	UpdateLeafAngles( );

	for (unsigned int i = 0; i < m_sAttributes.m_uiNumMatrices; ++i)
	{
		SWindMatrix& sMatrix = m_vWindMatrices[i];

#ifdef TEST_FILE
		if (i == 0)
			fprintf(pFile, "%g\t%g\n", fTime, sMatrix.m_cBendAngle.GetValue( ));
#endif

		// compute maximum branch throw
		sMatrix.m_fFinalStrength = static_cast<float>(sMatrix.m_cBendAngle.GetValue(m_fDeltaTime)); // * sMatrix.m_fAxisFactor;

		// compute axis
		sMatrix.m_fFinalAngle = static_cast<float>(sMatrix.m_cAxisAngle.GetValue(m_fDeltaTime));

		// compute the new rotation matrix
		sMatrix.m_cMatrix.RotateAxis(sMatrix.m_fFinalStrength, cosf(sMatrix.m_fFinalAngle), sinf(sMatrix.m_fFinalAngle), 0.0f);
		sMatrix.m_cMatrix.Rotate(static_cast<float>(sMatrix.m_cXVibration.GetValue(m_fDeltaTime)), 'x');
		sMatrix.m_cMatrix.Rotate(static_cast<float>(sMatrix.m_cYVibration.GetValue(m_fDeltaTime)), 'y');
	}

	return m_fStrength;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::UpdateSpeedTreeRT

void CSpeedWind::UpdateSpeedTreeRT(void) const
{
	for (unsigned int i = 0; i < m_sAttributes.m_uiNumMatrices; ++i)
	{
		// make SpeedTreeRT aware of new matrix
		CSpeedTreeRT::SetWindMatrix(i, (const float*) m_vWindMatrices[i].m_cMatrix.m_afData);
	}
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::UpdateBend

void CSpeedWind::UpdateBend(float fDirectionX, float fDirectionY, float fDirectionZ)
{
	// wind axis is perpendicular to fan direction in xy plane
	float fWindAngle = static_cast<float>(atan2(fDirectionY, fDirectionX)) + c_fHalfPi;

	// axis plays less of a role as wind direction approaches (0 0 -1)
	// preventing "haywire" effects as direction crosses pole
	float fDistance = sqrtf(fDirectionX * fDirectionX + fDirectionY * fDirectionY);
	float fPitch = static_cast<float>(atan2(-fDistance, fDirectionZ));
	float fAxisFactor = static_cast<float>(fabs(sinf(fPitch)));

	for (unsigned int i = 0; i < m_sAttributes.m_uiNumMatrices; ++i)
	{
		SWindMatrix& sMatrix = m_vWindMatrices[i];

		sMatrix.m_fAxisFactor = fAxisFactor;
		sMatrix.m_fStrength = m_fStrength;

		// account for crossing between 0 and 360 degrees
		float fThisAngle = static_cast<float>(sMatrix.m_cAxisAngle.GetWantedValue( ));
		float fThisWantedAngle = INTERPOLATE(fThisAngle, fWindAngle, fAxisFactor);
		while (fThisWantedAngle < fThisAngle - c_fPi)
			fThisWantedAngle += c_fTwoPi;
		while (fThisWantedAngle > fThisAngle + c_fPi)
			fThisWantedAngle -= c_fTwoPi;

		// set the angles
		sMatrix.m_cAxisAngle.SetWantedValue(fThisWantedAngle);
		sMatrix.m_cAxisAngle.SetConstants(c_fAxisScalar * INTERPOLATE(m_sAttributes.m_afBendLowWindControl[0], m_sAttributes.m_afBendHighWindControl[0], m_fAdjustedStrength),
			c_fAxisScalar * INTERPOLATE(m_sAttributes.m_afBendLowWindControl[1], m_sAttributes.m_afBendHighWindControl[1], m_fAdjustedStrength),
			c_fAxisScalar * INTERPOLATE(m_sAttributes.m_afBendLowWindControl[2], m_sAttributes.m_afBendHighWindControl[2], m_fAdjustedStrength),
			c_fAxisScalar * INTERPOLATE(m_sAttributes.m_afBendLowWindControl[3], m_sAttributes.m_afBendHighWindControl[3], m_fAdjustedStrength));
		sMatrix.m_cBendAngle.SetWantedValue(m_fStrength * m_sAttributes.m_fMaxBendAngle);
		sMatrix.m_cBendAngle.SetConstants(INTERPOLATE(m_sAttributes.m_afBendLowWindControl[0], m_sAttributes.m_afBendHighWindControl[0], m_fAdjustedStrength),
			INTERPOLATE(m_sAttributes.m_afBendLowWindControl[1], m_sAttributes.m_afBendHighWindControl[1], m_fAdjustedStrength),
			INTERPOLATE(m_sAttributes.m_afBendLowWindControl[2], m_sAttributes.m_afBendHighWindControl[2], m_fAdjustedStrength),
			INTERPOLATE(m_sAttributes.m_afBendLowWindControl[3], m_sAttributes.m_afBendHighWindControl[3], m_fAdjustedStrength));
	}
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::UpdateVibrations

void CSpeedWind::UpdateVibrations(void)
{
	for (unsigned int i = 0; i < m_sAttributes.m_uiNumMatrices; ++i)
	{
		SWindMatrix& sMatrix = m_vWindMatrices[i];

		float fChangeTime = INTERPOLATE(m_sAttributes.m_afVibrationFrequency[0], m_sAttributes.m_afVibrationFrequency[1], m_fAdjustedStrength);
		float fChangeChance = GetRandom(0.0f, 1.0f);
		if (fChangeChance < (fChangeTime / 60.0f) * m_fDeltaTime)
		{
			// it changed, pick new values
			float fThrowLimit = INTERPOLATE(m_sAttributes.m_afVibrationAngles[0], m_sAttributes.m_afVibrationAngles[1], m_fAdjustedStrength);

			// x Vibration
			sMatrix.m_cXVibration.SetWantedValue(GetRandom(-fThrowLimit, fThrowLimit));

			// y Vibration
			sMatrix.m_cYVibration.SetWantedValue(GetRandom(-fThrowLimit, fThrowLimit));
		}

		// update x constants
		sMatrix.m_cXVibration.SetConstants(INTERPOLATE(m_sAttributes.m_afVibrationLowWindControl[0], m_sAttributes.m_afVibrationHighWindControl[0], m_fAdjustedStrength),
			INTERPOLATE(m_sAttributes.m_afVibrationLowWindControl[1], m_sAttributes.m_afVibrationHighWindControl[1], m_fAdjustedStrength),
			INTERPOLATE(m_sAttributes.m_afVibrationLowWindControl[2], m_sAttributes.m_afVibrationHighWindControl[2], m_fAdjustedStrength),
			INTERPOLATE(m_sAttributes.m_afVibrationLowWindControl[3], m_sAttributes.m_afVibrationHighWindControl[3], m_fAdjustedStrength));
		// update y constants
		sMatrix.m_cYVibration.SetConstants(INTERPOLATE(m_sAttributes.m_afVibrationLowWindControl[0], m_sAttributes.m_afVibrationHighWindControl[0], m_fAdjustedStrength),
			INTERPOLATE(m_sAttributes.m_afVibrationLowWindControl[1], m_sAttributes.m_afVibrationHighWindControl[1], m_fAdjustedStrength),
			INTERPOLATE(m_sAttributes.m_afVibrationLowWindControl[2], m_sAttributes.m_afVibrationHighWindControl[2], m_fAdjustedStrength),
			INTERPOLATE(m_sAttributes.m_afVibrationLowWindControl[3], m_sAttributes.m_afVibrationHighWindControl[3], m_fAdjustedStrength));
	}
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::GetRandom

float CSpeedWind::GetRandom(float fMin, float fMax) const
{
	float fUnit = float(rand( )) / RAND_MAX;
	float fDiff = fMax - fMin;

	return fMin + fUnit * fDiff;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::UpdateStrength

void CSpeedWind::UpdateStrength(float fStrength)
{
	if (m_fTime > m_fGustEndTime)
	{
		// make sure the gust effect diminishes
		m_cGust.SetWantedValue(0.0);

		// see if it's time for a new gust
		float fChangeChance = GetRandom(0.0f, 1.0f);
		if (fChangeChance < (m_sAttributes.m_fGustFrequency / 60.0f) * m_fDeltaTime)
		{
			float fGust = INTERPOLATE(m_sAttributes.m_afGustStrength[0], m_sAttributes.m_afGustStrength[1], GetRandom(0.0f, 1.0f));

			if (fStrength + fGust > 1.0f)
				fGust = 1.0f - fStrength;

			m_cGust.SetWantedValue(fGust);

			m_fGustEndTime = m_fTime + INTERPOLATE(m_sAttributes.m_afGustDuration[0], m_sAttributes.m_afGustDuration[1], GetRandom(0.0f, 1.0f));
		}
	}

	// update strength parameters
	m_fStrength = fStrength + static_cast<float>(m_cGust.GetValue(m_fDeltaTime));

	//	m_fStrength = fStrength;
	if (m_fStrength < 0.0f)
		m_fStrength = 0.0f;
	else if (m_fStrength > 1.0f)
		m_fStrength = 1.0f;

	// compute adjusted parameters
	m_fAdjustedStrength = powf(fStrength, m_sAttributes.m_fStrengthAdjustmentExponent);
	m_fLeafAdjustedStrength = powf(m_fStrength, m_sAttributes.m_fLeafStrengthExponent);// + m_cGust.GetValue( );
	if (m_fLeafAdjustedStrength > 1.0f)
		m_fLeafAdjustedStrength = 1.0f;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::Load

bool CSpeedWind::Load(string strFilename)
{
	bool bSuccess = false;

	// create a parser
	CSpeedWindParser cParser;

	ifstream isData(strFilename.c_str( ));

	if (isData)
	{
		if (cParser.Parse(isData))
		{
			SetAttributes(cParser.GetWindAttributes( ));
			bSuccess = true;
		}
		else
		{
			m_strParserError = cParser.GetError( );
		}
	}
	else
	{
		m_strParserError = string("Failed to open '") + strFilename + "'";
	}

	// get the warnings
	m_strWarnings = cParser.GetWarnings( );

	return bSuccess;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::Load

bool CSpeedWind::Load(std::istream& isData)
{
	bool bSuccess = false;

	// create a parser
	CSpeedWindParser cParser;

	if (isData)
	{
		if (cParser.Parse(isData))
		{
			SetAttributes(cParser.GetWindAttributes( ));
			bSuccess = true;
		}
		else
		{
			m_strParserError = cParser.GetError( );
		}
	}
	else
	{
		m_strParserError = "NULL stream passed to CSpeedWind::Load( )";
	}

	// get the warnings
	m_strWarnings = cParser.GetWarnings( );

	return bSuccess;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::Save

bool CSpeedWind::Save(string strFilename) const
{
	bool bSuccess = false;

	ofstream osData(strFilename.c_str( ));
	if (osData)
	{
		bSuccess = Save(osData);
	}

	return bSuccess;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::Save

bool CSpeedWind::Save(std::ostream& osData) const
{
	bool bSuccess = false;

	if (osData)
	{
		// general parameters
		osData << "[General]\n";
		osData << "BranchStrengthExponent=" << m_sAttributes.m_fStrengthAdjustmentExponent << endl;
		osData << "LeafStrengthExponent=" << m_sAttributes.m_fLeafStrengthExponent << endl;
		osData << "NumLeafAngles=" << m_sAttributes.m_uiNumLeafAngles << endl;
		osData << "NumMatrices=" << m_sAttributes.m_uiNumMatrices << endl;

		// bend angle
		osData << "\n[Bend Angle]\n";
		osData << "MaxBendAngle=" << m_sAttributes.m_fMaxBendAngle << endl;
		SavePID(osData, "BendLowWind", m_sAttributes.m_afBendLowWindControl);
		SavePID(osData, "BendHighWind", m_sAttributes.m_afBendHighWindControl);

		// vibration
		osData << "\n[Vibration]\n";
		SaveLowHigh(osData, "VibrationAngles", m_sAttributes.m_afVibrationAngles);
		SaveLowHigh(osData, "VibrationFrequency", m_sAttributes.m_afVibrationFrequency);
		SavePID(osData, "VibrationLowWind", m_sAttributes.m_afVibrationLowWindControl);
		SavePID(osData, "VibrationHighWind", m_sAttributes.m_afVibrationHighWindControl);

		// gusts
		osData << "\n[Gusts]\n";
		osData << "GustsPerMinute=" << m_sAttributes.m_fGustFrequency << endl;
		SaveMinMax(osData, "GustStrength", m_sAttributes.m_afGustStrength);
		SaveMinMax(osData, "GustDuration", m_sAttributes.m_afGustDuration);
		SavePID(osData, "GustResponsiveness", m_sAttributes.m_afGustControl);

		// leaf rocking
		osData << "\n[Leaf Rocking]\n";
		SaveLowHigh(osData, "RockAngles", m_sAttributes.m_afLeafAngleAngles[ROCK]);
		SaveLowHigh(osData, "RockFrequency", m_sAttributes.m_afLeafAngleFrequency[ROCK]);
		SavePID(osData, "RockLowWind", m_sAttributes.m_afLeafAngleLowWindControl[ROCK]);
		SavePID(osData, "RockHighWind", m_sAttributes.m_afLeafAngleHighWindControl[ROCK]);

		// leaf rustling
		osData << "\n[Leaf Rustling]\n";
		SaveLowHigh(osData, "RustleAngles", m_sAttributes.m_afLeafAngleAngles[RUSTLE]);
		SaveLowHigh(osData, "RustleFrequency", m_sAttributes.m_afLeafAngleFrequency[RUSTLE]);
		SavePID(osData, "RustleLowWind", m_sAttributes.m_afLeafAngleLowWindControl[RUSTLE]);
		SavePID(osData, "RustleHighWind", m_sAttributes.m_afLeafAngleHighWindControl[RUSTLE]);

		bSuccess = true;
	}

	return bSuccess;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::SavePID

void CSpeedWind::SavePID(ostream& osData, string strName, const float* pData) const
{
	osData << strName << ".p=" << pData[0] << endl;
	osData << strName << ".i=" << pData[1] << endl;
	osData << strName << ".d=" << pData[2] << endl;
	osData << strName << ".a=" << pData[3] << endl;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::SaveLowHigh

void CSpeedWind::SaveLowHigh(ostream& osData, string strName, const float* pData) const
{
	osData << strName << ".low=" << pData[0] << endl;
	osData << strName << ".high=" << pData[1] << endl;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::SaveMinMax

void CSpeedWind::SaveMinMax(ostream& osData, string strName, const float* pData) const
{
	osData << strName << ".min=" << pData[0] << endl;
	osData << strName << ".max=" << pData[1] << endl;
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::UpdateLeafAngles

void CSpeedWind::UpdateLeafAngles(void)
{
	for (unsigned int i = 0; i < NUM_LEAF_ANGLES; ++i)
	{
		for (unsigned int j = 0; j < m_sAttributes.m_uiNumLeafAngles; ++j)
		{
			SLeafAngle& sLeafAngle = m_avLeafAngles[i][j];

			if (sLeafAngle.m_cAngle.GetWantedValue( ) == 0.0f)
			{
				// it's eligible for a new position
				float fChangeTime = INTERPOLATE(m_sAttributes.m_afLeafAngleFrequency[i][0], m_sAttributes.m_afLeafAngleFrequency[i][1], m_fLeafAdjustedStrength);
				float fChangeChance = GetRandom(0.0f, 1.0f);
				if (fChangeChance < (fChangeTime / 60.0f) * m_fDeltaTime)
				{
					float fThrowLimit = INTERPOLATE(m_sAttributes.m_afLeafAngleAngles[i][0], m_sAttributes.m_afLeafAngleAngles[i][1], m_fLeafAdjustedStrength);
					sLeafAngle.m_cAngle.SetWantedValue(GetRandom(-fThrowLimit, fThrowLimit));
				}
			}
			else if (fabs(sLeafAngle.m_cAngle.GetValue( ) - sLeafAngle.m_cAngle.GetWantedValue( )) < 0.5f)
			{
				// it's close enough, send it back
				sLeafAngle.m_cAngle.SetWantedValue(0.0f);
			}

			// update the value
			*sLeafAngle.m_pResult = static_cast<float>(sLeafAngle.m_cAngle.GetValue(m_fDeltaTime));

			// update constants
			sLeafAngle.m_cAngle.SetConstants(INTERPOLATE(m_sAttributes.m_afLeafAngleLowWindControl[i][0], m_sAttributes.m_afLeafAngleHighWindControl[i][0], m_fLeafAdjustedStrength),
				INTERPOLATE(m_sAttributes.m_afLeafAngleLowWindControl[i][1], m_sAttributes.m_afLeafAngleHighWindControl[i][1], m_fLeafAdjustedStrength),
				INTERPOLATE(m_sAttributes.m_afLeafAngleLowWindControl[i][2], m_sAttributes.m_afLeafAngleHighWindControl[i][2], m_fLeafAdjustedStrength),
				INTERPOLATE(m_sAttributes.m_afLeafAngleLowWindControl[i][3], m_sAttributes.m_afLeafAngleHighWindControl[i][3], m_fLeafAdjustedStrength));
		}
	}
}


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind::BuildLeafAngleMatrices

void CSpeedWind::BuildLeafAngleMatrices(const float* pCameraDir)
{
	float afAdjustedCameraDir[3];

#ifdef SPEEDWIND_UPVECTOR_POS_Z
	afAdjustedCameraDir[0] = pCameraDir[0];
	afAdjustedCameraDir[1] = pCameraDir[1];
	afAdjustedCameraDir[2] = pCameraDir[2];
#endif
#ifdef SPEEDWIND_UPVECTOR_NEG_Z
	afAdjustedCameraDir[0] = -pCameraDir[0];
	afAdjustedCameraDir[1] = pCameraDir[1];
	afAdjustedCameraDir[2] = -pCameraDir[2];
#endif
#ifdef SPEEDWIND_UPVECTOR_POS_Y
	afAdjustedCameraDir[0] = -pCameraDir[0];
	afAdjustedCameraDir[1] = pCameraDir[2];
	afAdjustedCameraDir[2] = pCameraDir[1];
#endif
#ifdef SPEEDWIND_UPVECTOR_DIRECTX_RIGHT_HANDED_COORDINATE_SYSTEM
	afAdjustedCameraDir[0] = pCameraDir[1];
	afAdjustedCameraDir[1] = pCameraDir[0];
	afAdjustedCameraDir[2] = pCameraDir[2];
#endif
#ifdef SPEEDWIND_UPVECTOR_MULTIVERSE
	afAdjustedCameraDir[0] = pCameraDir[0];
	afAdjustedCameraDir[1] = -pCameraDir[2];
	afAdjustedCameraDir[2] = pCameraDir[1];
#endif

	float fAzimuth = atan2f(afAdjustedCameraDir[1], afAdjustedCameraDir[0]) * c_fRad2Deg;
	float fPitch = -asinf(afAdjustedCameraDir[2]) * c_fRad2Deg;

	for (unsigned int i = 0; i < m_sAttributes.m_uiNumLeafAngles; ++i)
	{
		CSpeedWindMatrix& cMatrix = m_pLeafAngleMatrices[i];

		cMatrix.LoadIdentity( );
#ifdef SPEEDWIND_UPVECTOR_POS_Z
		cMatrix.Rotate(fAzimuth, 'z');
		cMatrix.Rotate(fPitch, 'y');
		cMatrix.Rotate(m_pLeafAngles[RUSTLE][i], 'z');
		cMatrix.Rotate(m_pLeafAngles[ROCK][i], 'x');
#endif
#ifdef SPEEDWIND_UPVECTOR_NEG_Z
		cMatrix.Rotate(-fAzimuth, 'z');
		cMatrix.Rotate(fPitch, 'y');
		cMatrix.Rotate(m_pLeafAngles[RUSTLE][i], 'z');
		cMatrix.Rotate(m_pLeafAngles[ROCK][i], 'x');
#endif
#ifdef SPEEDWIND_UPVECTOR_POS_Y
		cMatrix.Rotate(fAzimuth, 'y');
		cMatrix.Rotate(fPitch, 'z');
		cMatrix.Rotate(-m_pLeafAngles[RUSTLE][i], 'y');
		cMatrix.Rotate(m_pLeafAngles[ROCK][i], 'x');
#endif
#ifdef SPEEDWIND_UPVECTOR_DIRECTX_RIGHT_HANDED_COORDINATE_SYSTEM
		cMatrix.Rotate(-fAzimuth, 'z');
		cMatrix.Rotate(-fPitch, 'x');
		cMatrix.Rotate(m_pLeafAngles[RUSTLE][i], 'z');
		cMatrix.Rotate(m_pLeafAngles[ROCK][i], 'y');
#endif
#ifdef SPEEDWIND_UPVECTOR_MULTIVERSE
		//cMatrix.Rotate(fPitch, 'z');

		cMatrix.Rotate(-fAzimuth, 'y');
		cMatrix.Rotate(fPitch, 'z');
		cMatrix.Rotate(-m_pLeafAngles[RUSTLE][i], 'y');
		cMatrix.Rotate(m_pLeafAngles[ROCK][i], 'x');
#endif
	}
}