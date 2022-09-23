/**
*** 3D-Tune-In Toolkit Unity Reverb ***
*
* version alpha 1.1
* Created on: June 2016
*
* Author: 3DI-DIANA Research Group / University of Malaga / Spain
* Contact: areyes@uma.es
*
* Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
* Module: 3DTI Toolkit Unity Wrapper
*
* Updated: June 2020 onwards
* by Tim Murray-Browne at the Dyson School of Engineering, Imperial College London.
**/

//#include "stdafx.h"

#include "AudioPluginUtil.h"

// Tell the 3DTI Toolkit Core that we will be using the Unity axis convention!
// WARNING: This define must be done before including Core.h!
#define AXIS_CONVENTION UNITY

//#include "Core.h"

// Includes for reading HRTF data and logging dor debug
//#include <fstream>
//#include <iostream>
//#include <time.h>
//#include "Common/AIR.h"
//#include "effect3DTISpatializerSource.h"
#include "SpatializerCore.h"

#include "effect3DTISpatializerReverb.h"
#include "CommonUtils.h"

using namespace std;

/////////////////////////////////////////////////////////////////////

using namespace Binaural;
using namespace Common;
using namespace SpatializerCore3DTI;



namespace SpatializerReverb3DTI
{


	// DEBUG LOG FILE
	//#define LOG_FILE


	enum Parameter
	{
		Wetness = 0,
		NumParameters = 1
	};


	struct EffectData
	{
		//std::shared_ptr<SpatializerCore> spatializer;
		std::array<float, NumParameters> parameters;
	};

	std::atomic<bool> doesReverbInstanceExist(false);



/////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
		if (doesReverbInstanceExist.exchange(true))
		{
			// There is already an instance
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}
		assert(doesReverbInstanceExist);

		try
		{
			auto effectdata = new EffectData;
			effectdata->parameters = {
				0.5f, // wetness
			};
			state->effectdata = effectdata;
		}
		catch (const SpatializerCore::IncorrectAudioStateException& e)
		{
			WriteLog(e.what());
			if (state->effectdata != nullptr)
			{
				delete state->effectdata;
			}
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}

		return UNITY_AUDIODSP_OK;
	}

	/////////////////////////////////////////////////////////////////////


	int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
	{
		definition.paramdefs = new UnityAudioParameterDefinition[NumParameters];
		//RegisterParameter(definition, "Enable Reverb", "", 0, 1, 1.0f, 0.0f, 1.0f, EnableReverb, "Enable reverb processing (0.0 for off, non-zero for on");
		RegisterParameter(definition, "Wetness", "", 0.0f, 1.0f, 0.5f, 1.0f, 1.0f, Wetness, "Ratio of reverb to dry audio in output mix");
		return NumParameters;
	}


/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		delete data;
		assert(doesReverbInstanceExist);
		doesReverbInstanceExist = false;
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		if (index < 0 || index >= NumParameters || data == nullptr)
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}
		// As we need to lock the core spatializer mutex anyway during processing then we reuse it here
		std::lock_guard<std::mutex> lock(SpatializerCore::mutex());
		data->parameters[index] = value;
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char *valuestr)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		if (index < 0 || index >= NumParameters || data == nullptr)
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}
		// As we need to lock spatializer mutex anyway during processing then we reuse it here
		std::lock_guard<std::mutex> lock(SpatializerCore::mutex());
		*value = data->parameters[index];
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	int UNITY_AUDIODSP_CALLBACK GetFloatBufferCallback(UnityAudioEffectState* state, const char* name, float* buffer, int numsamples)
	{
		return UNITY_AUDIODSP_ERR_UNSUPPORTED;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
	{	
		EffectData* effectData = state->GetEffectData<EffectData>();
		std::lock_guard<std::mutex> lock(SpatializerCore::mutex());
		SpatializerCore* spatializer;
		try
		{
			spatializer = SpatializerCore::instance(state->samplerate, state->dspbuffersize);
		}
		catch (const SpatializerCore::IncorrectAudioStateException& e)
		{
			WriteLog(std::string("Error: Reverb ProcessCallback called with incorrect audio state. ") + e.what());
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}

		if (inchannels != 2 || outchannels != 2)
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}

		const int bufferSize = spatializer->core.GetAudioState().bufferSize;

		assert(bufferSize == length); // This should always be true as we test the audio state above
		if (bufferSize != length)
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}

		// 7. Process reverb and generate the reverb output
		if (spatializer->enableReverbProcessing != 0.0f && spatializer->isBinaryResourceLoaded[ReverbBRIR])
		{
			assert(const_cast<CABIR&>(spatializer->environment->GetABIR()).IsInitialized());

			Common::CEarPair<CMonoBuffer<float>> bReverbOutput;
			bReverbOutput.left.resize(bufferSize);
			bReverbOutput.right.resize(bufferSize);
			assert(bReverbOutput.left.size() == length && bReverbOutput.right.size() == length);
			spatializer->environment->ProcessVirtualAmbisonicReverb(bReverbOutput.left, bReverbOutput.right);

			const float wet = clamp(effectData->parameters[Wetness], 0.0f, 1.0f);
			const float dry = 1 - wet;
			for (size_t i = 0; i < length; i++)
			{
				outbuffer[i * 2 + 0] = dry * inbuffer[i*2+0] + wet * bReverbOutput.left[i];
				outbuffer[i * 2 + 1] = dry * inbuffer[i*2+1] + wet * bReverbOutput.right[i];
			}
		}
		else
		{
			for (size_t i = 0; i < (size_t) length * 2; i++)
			{
				outbuffer[i] = inbuffer[i];
			}
		}

		return UNITY_AUDIODSP_OK;



	}

}
