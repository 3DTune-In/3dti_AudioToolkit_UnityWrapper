#pragma once
#include "AudioPluginUtil.h"
#include <BinauralSpatializer/Core.h>
#include <Common/DynamicCompressorStereo.h>
#include "effect3DTISpatializerCore.h"


namespace SpatializerSource3DTI
{
	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value);
}


