#pragma once
#include "AudioPluginUtil.h"
#include <BinauralSpatializer/Core.h>
#include <Common/DynamicCompressorStereo.h>
#include "effect3DTISpatializerCore.h"


namespace SpatializerSource3DTI
{
	// These values are set explicitly as they need to correspond to values in the C# components.
	enum
	{
		// Per-source parameters. We store them in the core so we know what value to initialize the values to on a new source instance.
		PARAM_HRTF_INTERPOLATION = 0, // ### SOURCE ####
		PARAM_MOD_FARLPF = 1, // ### SOURCE ####
		PARAM_MOD_DISTATT = 2, // ### SOURCE ####
		PARAM_MOD_NEAR_FIELD_ILD = 3,// ### SOURCE ####
		PARAM_SPATIALIZATION_MODE = 4,// ### SOURCE ####
		NumSourceParameters = 5,

	};

	struct EffectData
	{
		int sourceID;    // DEBUG
		std::shared_ptr<Binaural::CSingleSourceDSP> audioSource;
		SpatializerCore3DTI::SpatializerCore* spatializer;
		//float parameters[NumSourceParameters];
	};
}


