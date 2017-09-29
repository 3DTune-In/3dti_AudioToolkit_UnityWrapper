/**
*** 3D-Tune-In Toolkit Unity Wrapper for Hearing Loss Simulation***
*
* version 1.10
* Created on: September 2017
*
* Author: 3DI-DIANA Research Group / University of Malaga / Spain
* Contact: areyes@uma.es
*
* Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
* Module: 3DTI Toolkit Unity Wrapper
**/

#ifndef _COMMON_UTILS_
#define _COMMON_UTILS_

#include <string>

///////////////////////////////////////

float Bool2Float(bool b)
{
	if (b)
		return 1.0f;
	else
		return 0.0f;
}

///////////////////////////////////////

bool Float2Bool(float f)
{
	if (f > 0.0f)
		return true;
	else
		return false;
}

///////////////////////////////////////

std::string Bool2String(bool b)
{
	if (b)
		return "ON";
	else
		return "OFF";
}

///////////////////////////////////////

#endif
