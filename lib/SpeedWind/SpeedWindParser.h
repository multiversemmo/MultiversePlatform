///////////////////////////////////////////////////////////////////////  
//	SpeedWindParser.h
//
//	(c) 2004 IDV, Inc.
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
#include "SpeedWind.h"
#include <map>


/////////////////////////////////////////////////////////////////////////////
// Forward references


/////////////////////////////////////////////////////////////////////////////
// CSpeedWindParser

class CSpeedWindParser 
{
public:
		// constructor 
									CSpeedWindParser(void);

		// parsing
		bool						Parse(std::istream& isData);
	
		// data retrieval
		CSpeedWind::SWindAttributes	GetWindAttributes(void) const	{ return m_sAttributes; }

		// errors/warnings
		std::string					GetWarnings(void) const			{ return m_strWarnings; }
		std::string					GetError(void) const			{ return m_strError; }

private:
		CSpeedWind::SWindAttributes	m_sAttributes;
		std::string					m_strWarnings;
		std::string					m_strError;
		unsigned int				m_uiLineNumber;
		std::map<std::string, int>	m_mTokenMap;

		bool						ParseToken(std::string strToken, std::string strValue);
		void						AddWarning(std::string strWarning, std::string strLine);
};