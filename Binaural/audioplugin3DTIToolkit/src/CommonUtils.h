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
#include <iostream>

//namespace Common3DTI
//{

	inline bool IsHostCompatible(UnityAudioEffectState* state)
	{
		// Somewhat convoluted error checking here because hostapiversion is only supported from SDK version 1.03 (i.e. Unity 5.2) and onwards.
		return
			state->structsize >= sizeof(UnityAudioEffectState) &&
			state->hostapiversion >= UNITY_AUDIO_PLUGIN_API_VERSION;
	}


	inline void WriteLog(std::string logText)
	{
		std::cerr << logText << std::endl;
	}

	///////////////////////////////////////

	inline float Bool2Float(bool b)
	{
		if (b)
			return 1.0f;
		else
			return 0.0f;
	}

	///////////////////////////////////////

	inline bool Float2Bool(float f)
	{
		if (f > 0.0f)
			return true;
		else
			return false;
	}

	///////////////////////////////////////

	inline std::string Bool2String(bool b)
	{
		if (b)
			return "ON";
		else
			return "OFF";
	}

	inline float clamp(float x, float min, float max)
	{
		return x < min ? min : x > max ? max : x;
	}

//}
///////////////////////////////////////

#endif
