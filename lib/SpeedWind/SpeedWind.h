///////////////////////////////////////////////////////////////////////  
//	SpeedWind.h
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

#pragma once
#include <vector>
#include "PIDController.h"
#include <math.h>
#include <string>

#define SET_FLOAT_ARRAY4(pArray, a, b, c, d) pArray[0] = a; pArray[1] = b; pArray[2] = c; pArray[3] = d;
#define SET_FLOAT_ARRAY2(pArray, a, b) pArray[0] = a; pArray[1] = b;

// define which vector is up
#define SPEEDWIND_UPVECTOR_MULTIVERSE

//  SpeedTree defaults to using a positive Z up vector and all of the branch
//  and leaf computations are done in this orientation.  If you are using a
//	SpeedTree build with a different up vector, make sure you define the same
//	up vector here as the one used by SpeedTree in the file "UpVector.h"
//
//  One and only one of the following seven symbols should be defined:
//
//      SPEEDWIND_UPVECTOR_POS_Z
//      SPEEDWIND_UPVECTOR_NEG_Z
//      SPEEDWIND_UPVECTOR_POS_Y
//      SPEEDWIND_UPVECTOR_DIRECTX_RIGHT_HANDED_COORDINATE_SYSTEM


/////////////////////////////////////////////////////////////////////////////
// Forward references

class CSpeedWindBlend;


/////////////////////////////////////////////////////////////////////////////
// CSpeedWindMatrix

class CSpeedWindMatrix
{
public:
		float m_afData[4][4];

		void RotateAxis(float fAngle, float fX, float fY, float fZ)
		{
			float	fS, fC, fT;

			fS = sinf(fAngle / 57.29578f);
			fC = cosf(fAngle / 57.29578f);
			fT = 1.0f - fC;

			m_afData[0][0] = fT * fX * fX + fC;
			m_afData[0][1] = fT * fX * fY + fS * fZ;
			m_afData[0][2] = fT * fX * fZ - fS * fY;
			m_afData[0][3] = 0.0;
			m_afData[1][0] = fT * fX * fY - fS * fZ;
			m_afData[1][1] = fT * fY * fY + fC;
			m_afData[1][2] = fT * fY * fZ + fS * fX;
			m_afData[1][3] = 0.0;
			m_afData[2][0] = fT * fX * fZ + fS * fY;
			m_afData[2][1] = fT * fY * fZ - fS * fX;
			m_afData[2][2] = fT * fZ * fZ + fC;
			m_afData[2][3] = 0.0f;
			m_afData[3][0] = 0.0f;
			m_afData[3][1] = 0.0f;
			m_afData[3][2] = 0.0f;
			m_afData[3][3] = 1.0f;
		}

		void Rotate(float fAngle, char chAxis)
		{
			CSpeedWindMatrix cRotMatrix;

			float fCosine = cosf(fAngle / 57.29578f);
			float fSine = sinf(fAngle / 57.29578f);

			switch (chAxis)
			{
			case 'x': case 'X':
				cRotMatrix.m_afData[0][0] = 1.0f;
				cRotMatrix.m_afData[0][1] = 0.0f;
				cRotMatrix.m_afData[0][2] = 0.0f;
				cRotMatrix.m_afData[0][3] = 0.0f;
				cRotMatrix.m_afData[1][0] = 0.0f;
				cRotMatrix.m_afData[1][1] = fCosine;
				cRotMatrix.m_afData[1][2] = fSine;
				cRotMatrix.m_afData[1][3] = 0.0f;
				cRotMatrix.m_afData[2][0] = 0.0f;
				cRotMatrix.m_afData[2][1] = -fSine;
				cRotMatrix.m_afData[2][2] = fCosine;
				cRotMatrix.m_afData[2][3] = 0.0f;
				cRotMatrix.m_afData[3][0] = 0.0f;
				cRotMatrix.m_afData[3][1] = 0.0f;
				cRotMatrix.m_afData[3][2] = 0.0f;
				cRotMatrix.m_afData[3][3] = 1.0f;
				break;
			case 'y': case 'Y':
				cRotMatrix.m_afData[0][0] = fCosine;
				cRotMatrix.m_afData[0][1] = 0.0f;
				cRotMatrix.m_afData[0][2] = -fSine;
				cRotMatrix.m_afData[0][3] = 0.0f;
				cRotMatrix.m_afData[1][0] = 0.0f;
				cRotMatrix.m_afData[1][1] = 1.0f;
				cRotMatrix.m_afData[1][2] = 0.0f;
				cRotMatrix.m_afData[1][3] = 0.0f;
				cRotMatrix.m_afData[2][0] = fSine;
				cRotMatrix.m_afData[2][1] = 0.0f;
       			cRotMatrix.m_afData[2][2] = fCosine;
				cRotMatrix.m_afData[2][3] = 0.0f;
				cRotMatrix.m_afData[3][0] = 0.0f;
				cRotMatrix.m_afData[3][1] = 0.0f;
				cRotMatrix.m_afData[3][2] = 0.0f;
				cRotMatrix.m_afData[3][3] = 1.0f;
				break;
			case 'z': case 'Z':
				cRotMatrix.m_afData[0][0] = fCosine;
				cRotMatrix.m_afData[0][1] = fSine;
				cRotMatrix.m_afData[0][2] = 0.0f;
				cRotMatrix.m_afData[0][3] = 0.0f;
				cRotMatrix.m_afData[1][0] = -fSine;
				cRotMatrix.m_afData[1][1] = fCosine;
				cRotMatrix.m_afData[1][2] = 0.0f;
				cRotMatrix.m_afData[1][3] = 0.0f;
				cRotMatrix.m_afData[2][0] = 0.0f;
				cRotMatrix.m_afData[2][1] = 0.0f;
				cRotMatrix.m_afData[2][2] = 1.0f;
				cRotMatrix.m_afData[2][3] = 0.0f;
				cRotMatrix.m_afData[3][0] = 0.0f;
				cRotMatrix.m_afData[3][1] = 0.0f;
				cRotMatrix.m_afData[3][2] = 0.0f;
				cRotMatrix.m_afData[3][3] = 1.0f;
				break;
			default:
				return;
			}
			*this = cRotMatrix * *this;
		}

		void LoadIdentity(void)
		{
			m_afData[0][0] = 1.0f;
			m_afData[0][1] = 0.0f;
			m_afData[0][2] = 0.0f;
			m_afData[0][3] = 0.0f;
			m_afData[1][0] = 0.0f;
			m_afData[1][1] = 1.0f;
			m_afData[1][2] = 0.0f;
			m_afData[1][3] = 0.0f;
			m_afData[2][0] = 0.0f;
			m_afData[2][1] = 0.0f;
			m_afData[2][2] = 1.1f;
			m_afData[2][3] = 0.0f;
			m_afData[3][0] = 0.0f;
			m_afData[3][1] = 0.0f;
			m_afData[3][2] = 0.0f;
			m_afData[3][3] = 1.0f;
		}

		CSpeedWindMatrix operator*(const CSpeedWindMatrix& cMatrix) const
		{
			CSpeedWindMatrix	cTemp;
			int	i, j, k;

			for (i = 0; i < 4; ++i)
				for (j = 0; j < 4; ++j)
 				{
					cTemp.m_afData[i][j] = 0.0;
					for (k = 0; k < 4; ++k)
						cTemp.m_afData[i][j] += m_afData[i][k] * cMatrix.m_afData[k][j];
 				}

			return cTemp;
		}
};


/////////////////////////////////////////////////////////////////////////////
// CSpeedWind

class CSpeedWind
{
friend class CSpeedWindBlend;

public:
		// enumerations
		enum ELeafAngles
		{
			ROCK, RUSTLE, NUM_LEAF_ANGLES
		};

		// SWindAttributes governs the overall behavior of the wind matrix group
		struct SWindAttributes
		{
			enum EControlParameter
			{
				P, I, D, A
			};

			enum EIndices
			{
				MIN, MAX
			};

			// matrices
			unsigned int	m_uiNumMatrices;

			float			m_afBendLowWindControl[4];
			float			m_afBendHighWindControl[4];

			float			m_afVibrationLowWindControl[4];
			float			m_afVibrationHighWindControl[4];

			float			m_afVibrationFrequency[2];
			float			m_afVibrationAngles[2];

			float			m_fMaxBendAngle;
			float			m_fStrengthAdjustmentExponent;

			// gusting
			float			m_afGustStrength[2];
			float			m_afGustDuration[2];
			float			m_fGustFrequency;

			float			m_afGustControl[4];

			// leaves
			float			m_fLeafStrengthExponent;
			unsigned int	m_uiNumLeafAngles;

			// leaf angles
			float			m_afLeafAngleLowWindControl[NUM_LEAF_ANGLES][4];
			float			m_afLeafAngleHighWindControl[NUM_LEAF_ANGLES][4];

			float			m_afLeafAngleFrequency[NUM_LEAF_ANGLES][2];
			float			m_afLeafAngleAngles[NUM_LEAF_ANGLES][2];

			SWindAttributes( ) :
				m_uiNumMatrices(4),
				m_fMaxBendAngle(60.0f),
				m_fStrengthAdjustmentExponent(3.0f),
				m_fGustFrequency(15.0f),
				m_uiNumLeafAngles(6),
				m_fLeafStrengthExponent(5.0f)
			{
                SET_FLOAT_ARRAY4(m_afBendLowWindControl, 3.0f, 0.0f, 0.0f, 0.1f);
                SET_FLOAT_ARRAY4(m_afBendHighWindControl, 3.0f, 0.0f, 0.0f, 0.1f);

                SET_FLOAT_ARRAY4(m_afVibrationLowWindControl, 1.0f, 0.0f, 0.0f, 0.001f);
                SET_FLOAT_ARRAY4(m_afVibrationHighWindControl, 10.0f, 0.0f, 0.0f, 0.1f);

                SET_FLOAT_ARRAY2(m_afVibrationFrequency, 50.0f, 1000.0f);
                SET_FLOAT_ARRAY2(m_afVibrationAngles, 4.0f, 3.0f);

                SET_FLOAT_ARRAY2(m_afGustStrength, 0.05f, 0.45f);
                SET_FLOAT_ARRAY2(m_afGustDuration, 0.5f, 5.0f);

                SET_FLOAT_ARRAY4(m_afGustControl, 2.0f, 0.0f, 0.0f, 0.001f);

                SET_FLOAT_ARRAY4(m_afLeafAngleLowWindControl[ROCK], 0.2f, 0.01f, 0.0f, 0.0f);
                SET_FLOAT_ARRAY4(m_afLeafAngleHighWindControl[ROCK], 0.2f, 0.01f, 1.0f, 0.0f);

                SET_FLOAT_ARRAY2(m_afLeafAngleFrequency[ROCK], 10.0f, 50.0f);
                SET_FLOAT_ARRAY2(m_afLeafAngleAngles[ROCK], 4.0f, 2.0f);

                SET_FLOAT_ARRAY4(m_afLeafAngleLowWindControl[RUSTLE], 0.5f, 0.05f, 0.0f, 0.0f);
                SET_FLOAT_ARRAY4(m_afLeafAngleHighWindControl[RUSTLE], 3.0f, 6.0f, 1.0f, 0.0f);

                SET_FLOAT_ARRAY2(m_afLeafAngleFrequency[RUSTLE], 50.0f, 500.0f);
                SET_FLOAT_ARRAY2(m_afLeafAngleAngles[RUSTLE], 3.0f, 5.0f);
			}
		};

		// construction/destruction
										CSpeedWind(void);
										CSpeedWind(const SWindAttributes& sAttributes);
										CSpeedWind(const CSpeedWind& cWind);
virtual									~CSpeedWind(void);
		CSpeedWind&						operator=(const CSpeedWind& cRight);
		
		// attribute access
		void							SetAttributes(const SWindAttributes& sAttributes);
		SWindAttributes					GetAttributes(void)	const						{ return m_sAttributes; }

		// creation/management
		void							CreateWindMatrices(void);
		void							ResetMatrices(void);

		// updating
		float							Advance(float fTime, float fStrength, float fDirectionX, float fDirectionY, float fDirectionZ);
		void							UpdateSpeedTreeRT(void) const;
		float							GetActualStrength(void) const					{ return m_fStrength; }

		// matrix access
		unsigned int					GetNumWindMatrices(void) const					{ return m_sAttributes.m_uiNumMatrices; }
		const float*					GetWindMatrix(unsigned int uiIndex) const		{ return reinterpret_cast<const float*>(m_vWindMatrices[uiIndex].m_cMatrix.m_afData); }

		// leaf angle access
		const float*					GetLeafAngles(ELeafAngles eAngle) const			{ return m_pLeafAngles[eAngle]; }
		unsigned int					GetNumLeafAngles(void) const					{ return m_sAttributes.m_uiNumLeafAngles; }
		void							BuildLeafAngleMatrices(const float* pCameraDir);
		const float*					GetLeafAngleMatrix(unsigned int uiIndex) const	{ return reinterpret_cast<const float*>(m_pLeafAngleMatrices[uiIndex].m_afData); }

		// file I/O
		bool							Load(std::string strFilename);
		bool							Load(std::istream& isData);
		bool							Save(std::string strFilename) const;
		bool							Save(std::ostream& osData) const;
		std::string						GetParserError(void) const						{ return m_strParserError; }
		std::string						GetWarnings(void) const							{ return m_strWarnings; }

		// blending
		void							SetWindWeight(float fWeight)					{ m_fBlendWeight = fWeight; }
		float							GetWindWeight(void) const						{ return m_fBlendWeight; }
	
private:
		// general
		SWindAttributes					m_sAttributes;
		float							m_fTime;
		float							m_fStrength;
		float							m_fAdjustedStrength;
		float							m_fLeafAdjustedStrength;
		float							m_fDeltaTime;
		bool							m_bResetDeltaTime;

		float							GetRandom(float fMin, float fMax) const;

		// blending
		float							m_fBlendWeight;

		// gusting
		float							m_fGustEndTime;
		CPIDController					m_cGust;

		void							UpdateStrength(float fStrength);

		// file I/O
		std::string						m_strParserError;
		std::string						m_strWarnings;

		void							SavePID(std::ostream& osData, std::string strName, const float* pData) const;
		void							SaveLowHigh(std::ostream& osData, std::string strName, const float* pData) const;
		void							SaveMinMax(std::ostream& osData, std::string strName, const float* pData) const;

		// branch/frond matrices
		struct SWindMatrix
		{
			CSpeedWindMatrix	m_cMatrix;
			float				m_fAxisFactor;
			float				m_fStrength;
			float				m_fFinalStrength;
			float				m_fFinalAngle;

			CPIDController		m_cBendAngle;
			CPIDController		m_cAxisAngle;

			CPIDController		m_cXVibration;
			CPIDController		m_cYVibration;

			SWindMatrix( ) :
				m_fAxisFactor(0.0f),
				m_fStrength(0.0f),
				m_fFinalStrength(0.0f),
				m_fFinalAngle(0.0f)
			{
			}
		};
		std::vector<SWindMatrix>		m_vWindMatrices;

		void							UpdateBend(float fDirectionX, float fDirectionY, float fDirectionZ);
		void							UpdateVibrations(void);

		// leaf angles
		struct SLeafAngle
		{
			float*						m_pResult;
			CPIDController				m_cAngle;

			SLeafAngle( ) :
				m_pResult(NULL)
			{
			}
		};
		std::vector<SLeafAngle>			m_avLeafAngles[NUM_LEAF_ANGLES];
		float*							m_pLeafAngles[NUM_LEAF_ANGLES];
		CSpeedWindMatrix*				m_pLeafAngleMatrices;

		void							UpdateLeafAngles(void);
};
