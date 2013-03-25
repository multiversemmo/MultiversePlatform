///////////////////////////////////////////////////////////////////////  
//	PIDContoller.h
//
//	(c) 2002 IDV, Inc.
//
//	This file contains a simple discrete PID contoller class.
//
//
//	*** INTERACTIVE DATA VISUALIZATION (IDV) PROPRIETARY INFORMATION ***
//
//	This software is supplied under the terms of a license agreement or
//	nondisclosure agreement with Interactive Data Visualization and may
//	not be copied or disclosed except in accordance with the terms of
//	that agreement.
//
//      Copyright (c) 2001-2002 IDV, Inc.
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
#include <float.h>
#include <math.h>

/////////////////////////////////////////////////////////////////////////////
// Constants

const double c_dMaxAllowableDeltaTime = 0.03;	// if more time (in seconds) than this has passed, no PID adjustments will be made


/////////////////////////////////////////////////////////////////////////////
// CPIDController

class CPIDController
{
public:
						CPIDController( ) : 
							m_dValue(0.0),
							m_dWantedValue(0.0),
							m_dPConst(1.0),
							m_dIConst(0.0),
							m_dDConst(0.0),
							m_dRunningError(0.0),
							m_dLastDelta(0.0),
							m_dLastError(0.0),
							m_bValidError(false),
							m_dMaxAcceleration(0.0),
							m_dLastDeltaTime(0.0)
						{
						}

		void			SetValue(double dValue);
		void			SetWantedValue(double dWantedValue);
		void			SetConstants(double dPConst, double dIConst, double dDConst, double dMaxAcceleration = 0.0);

		double			GetValue( ) const;
		double			GetWantedValue( ) const;
		double			GetLastError( ) const;
		double			GetLastDelta( ) const;
		double			GetValue(double dDeltaTime);

		void			ResetError(void);

		void			SetMaxAcceleration(double dValue)	{ m_dMaxAcceleration = dValue; }

private:
		double			m_dValue;				// current value of the controller
		double			m_dWantedValue;			// the value the controller is trying to achieve

		double			m_dPConst;				// proportional constant (Kp)
		double			m_dIConst;				// integral constant (Ki)
		double			m_dDConst;				// derivative constant (Kd)
		double			m_dMaxAcceleration;		// limits how fast the control can accelerate the value

		double			m_dLastError;			// previous error
		double			m_dLastDelta;			// amout of change during last adjustment
		double			m_dRunningError;		// summed errors (using as the integral value)
		bool			m_bValidError;			// prevents numerical problems on the first adjustment

		double			m_dLastDeltaTime;
};


/////////////////////////////////////////////////////////////////////////////
// CPIDController::SetValue

inline void CPIDController::SetValue(double dValue)
{ 
	m_dValue = dValue;
	m_dLastError = 0.0;
	m_dLastDelta = 0.0;
}


/////////////////////////////////////////////////////////////////////////////
// CPIDController::SetWantedValue

inline void	CPIDController::SetWantedValue(double dWantedValue)
{ 
	m_dWantedValue = dWantedValue; 
}


/////////////////////////////////////////////////////////////////////////////
// CPIDController::SetConstants

inline void	CPIDController::SetConstants(double dPConst, double dIConst, double dDConst, double dAcceleration)
{ 
	m_dPConst = dPConst; 
	m_dIConst = dIConst; 
	m_dDConst = dDConst;
	m_dMaxAcceleration = dAcceleration;
}


/////////////////////////////////////////////////////////////////////////////
// CPIDController::GetValue

inline double CPIDController::GetValue( ) const
{
	return m_dValue;
}


/////////////////////////////////////////////////////////////////////////////
// CPIDController::GetLastError

inline double CPIDController::GetLastError( ) const
{
	return m_dLastError;
}


/////////////////////////////////////////////////////////////////////////////
// CPIDController::GetWantedValue

inline double CPIDController::GetWantedValue( ) const
{
	return m_dWantedValue;
}


/////////////////////////////////////////////////////////////////////////////
// CPIDController::GetLastDelta

inline double CPIDController::GetLastDelta( ) const
{
	return m_dLastDelta;
}


/////////////////////////////////////////////////////////////////////////////
// CPIDController::GetValue

inline double CPIDController::GetValue(double dDeltaTime) 
{ 
	// if too much time has passed, do nothing
	if (dDeltaTime == 0.0f)
		return m_dValue;
	else if (dDeltaTime > c_dMaxAllowableDeltaTime)
		dDeltaTime = c_dMaxAllowableDeltaTime;

	// compute the error and sum of the errors for the integral
	double dError = (m_dWantedValue - m_dValue) * dDeltaTime;   
	m_dRunningError += dError;

	// proportional
	double dP = m_dPConst * dError;

	// integral
	double dI = m_dIConst * m_dRunningError * dDeltaTime;

	// derivative
	double dD(0.0f);
	if (m_bValidError)
		dD = m_dDConst * (m_dLastError - dError) * dDeltaTime;
	else
		m_bValidError = true;

	// remember the error for derivative
	m_dLastError = dError;

	// compute the adjustment
	double dThisDelta = dP + dI + dD;

	// clamp the acceleration
	if (m_dMaxAcceleration != 0.0f || false)
	{
		double dTimeRatio(1.0);
		if (m_dLastDeltaTime != 0.0)
			dTimeRatio = dDeltaTime / m_dLastDeltaTime;
		m_dLastDeltaTime = dDeltaTime;

		m_dLastDelta *= dTimeRatio;
		double dDifference = (dThisDelta - m_dLastDelta);
		double dAccel = m_dMaxAcceleration * dDeltaTime * dDeltaTime;

		if (dDifference < -dAccel)
			dThisDelta = m_dLastDelta - dAccel;
		else if (dDifference > dAccel)
			dThisDelta = m_dLastDelta + dAccel;
	}

	// modify the value
	m_dValue += dThisDelta;
	m_dLastDelta = dThisDelta;

	return m_dValue;
}


/////////////////////////////////////////////////////////////////////////////
// CPIDController::ResetError

inline void	CPIDController::ResetError(void)
{ 
	m_dRunningError = 0.0f;
	m_bValidError = false;
}

 