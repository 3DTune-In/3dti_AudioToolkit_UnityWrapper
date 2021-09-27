#pragma once
#include "AudioPluginUtil.h"
#include "SpatializerCore.h"


namespace SpatializerSource3DTI
{
	// SpatializerCore Mutex must be locked when calling this
	UNITY_AUDIODSP_RESULT SetFloatParameter(SpatializerCore3DTI::SpatializerCore* spatializer, UnityAudioEffectState* state, int index, float value);
}


