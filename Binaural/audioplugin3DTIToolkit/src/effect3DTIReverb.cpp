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
**/

//#include "stdafx.h"

#include "AudioPluginUtil.h"

// Tell the 3DTI Toolkit Core that we will be using the Unity axis convention!
// WARNING: This define must be done before including Core.h!
#define AXIS_CONVENTION UNITY

//#include "Core.h"

// Includes for reading HRTF data and logging dor debug
#include <fstream>
#include <iostream>
#include <time.h>
#include <HRTF/HRTFCereal.h>
#include "Common/AIR.h"
#include "effect3DTISpatializer.h"
#include "BRIR/BRIRCereal.h"
#include "HRTF/HRTFFactory.h"
#include "ILD/ILDCereal.h"

#include "effect3DTIReverb.h"

using namespace std;

/////////////////////////////////////////////////////////////////////

using namespace Binaural;
using namespace Common;




namespace Reverb3DTI
{


	// DEBUG LOG FILE
	//#define LOG_FILE
	void WriteLog(string logText)
	{
		std::cerr << logText << std::endl;
#ifdef LOG_FILE
		string channel = "Undefined channel";
		if (channelid == 0)
			channel = "W";
		if (channelid == 1)
			channel = "X";
		if (channfelid == 2)
			channel = "Y";

		ofstream logfile;
		logfile.open("debugreverb.txt", ofstream::out | ofstream::app);
		logfile << channel << ": " << logtext << value << endl;
		logfile.close();
#endif
	}


	extern "C" UNITY_AUDIODSP_EXPORT_API bool setup3DTISpatializer(const char* hrtfPath, const char* ildPath, const char* highPerformanceILDPath, const char* brirPath) {

		SpatializerCore* instance = SpatializerCore::instance();
		if (instance == nullptr)
		{
			WriteLog("Error: setup3DTISpatializer called before the Spatializer plugin was created.");
			return false;
		}
		return instance->loadBinaries(hrtfPath, ildPath, highPerformanceILDPath, brirPath);

	}



/////////////////////////////////////////////////////////////////////

	inline bool IsHostCompatible(UnityAudioEffectState* state)
	{
		// Somewhat convoluted error checking here because hostapiversion is only supported from SDK version 1.03 (i.e. Unity 5.2) and onwards.
		return
			state->structsize >= sizeof(UnityAudioEffectState) &&
			state->hostapiversion >= UNITY_AUDIO_PLUGIN_API_VERSION;
	}


/////////////////////////////////////////////////////////////////////

	int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
	{
		int numparams = P_NUM;
		definition.paramdefs = new UnityAudioParameterDefinition[numparams];				
		RegisterParameter(definition, "HeadRadius", "m", 0.0f, /*FLT_MAX*/ 1e20f, 0.0875f, 1.0f, 1.0f, PARAM_HEAD_RADIUS, "Listener head radius");
		RegisterParameter(definition, "ScaleFactor", "", 0.0f, /*FLT_MAX*/ 1e20f, 1.0f, 1.0f, 1.0f, PARAM_SCALE_FACTOR, "Scale factor for over/under sized scenes");
		RegisterParameter(definition, "CustomITD", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_CUSTOM_ITD, "Enabled custom ITD");
		//RegisterParameter(definition, "MODfarLPF", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_FARLPF, "Far distance LPF module enabler");
		//RegisterParameter(definition, "MODDistAtt", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_DISTATT, "Distance attenuation module enabler");
		//RegisterParameter(definition, "MODNFILD", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_NEAR_FIELD_ILD, "Near distance ILD module enabler");
		// TODO: Change this default value to -1
		RegisterParameter(definition, "MAGAneAtt", "dB", -30.0f, 0.0f, -3.0f, 1.0f, 1.0f, PARAM_MAG_ANECHATT, "Anechoic distance attenuation");
		RegisterParameter(definition, "MAGSounSpd", "m/s", 10.0f, 1000.0f, 343.0f, 1.0f, 1.0f, PARAM_MAG_SOUNDSPEED, "Sound speed");

		// HA directionality
		RegisterParameter(definition, "HADirExtL", "dB", 0.0f, 30.0f, 15.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_EXTEND_LEFT, "HA directionality attenuation (in dB) for Left ear");
		RegisterParameter(definition, "HADirExtR", "dB", 0.0f, 30.0f, 15.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT, "HA directionality attenuation (in dB) for Right ear");
		RegisterParameter(definition, "HADirOnL", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_ON_LEFT, "HA directionality switch for Left ear");
		RegisterParameter(definition, "HADirOnR", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_ON_RIGHT, "HA directionality switch for Right ear");

		// Limiter
		RegisterParameter(definition, "LimitOn", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_LIMITER_SET_ON, "Limiter enabler for binaural spatializer");

		// HRTF resampling step
		RegisterParameter(definition, "HRTFstep", "deg", 1.0f, 90.0f, 15.0f, 1.0f, 1.0f, PARAM_HRTF_STEP, "HRTF resampling step (in degrees)");

		// High performance mode
		//RegisterParameter(definition, "SpatMode", "", 0.0f, 2.0f, 0.0f, 1.0f, 1.0f, PARAM_SPATIALIZATION_MODE, "Spatialization mode (0=High quality, 1=High performance, 2=None)");
		// readonly
		RegisterParameter(definition, "SpatHQHRTFReady", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_IS_HIGH_QUALITY_HRTF_LOADED, "Is the HRTF loaded for High Quality mode");
		RegisterParameter(definition, "SpatHQILDReady", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_IS_HIGH_QUALITY_ILD_LOADED, "Is the ILD loaded for High Quality mode");
		RegisterParameter(definition, "SpatHPILDReady", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_IS_HIGH_PERFORMANCE_ILD_LOADED, "Is the ILD loaded for High Performance mode");
		RegisterParameter(definition, "SpatRvBRIRReady", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_IS_REVERB_BRIR_LOADED, "Is the BRIR loaded for Reverb");

		return numparams;
	}


/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
		try
		{
			auto spatializer = SpatializerCore::create(state->samplerate, state->dspbuffersize);
			state->effectdata = spatializer;
			InitParametersFromDefinitions(InternalRegisterEffectDefinition, spatializer->parameters);
		}
		catch (const SpatializerCore::TooManyInstancesEception&)
		{
			WriteLog("Error: Attempted to create multiple Spatializer Core plugins. Only one is supported.");
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}


		// Initialization will happen when the SPATIALIZATION_MODE parameter is set.


	
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
	{
		SpatializerCore* data = state->GetEffectData<SpatializerCore>();
		delete data;
		assert(SpatializerCore::instance() == nullptr);
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
	{
		SpatializerCore* spatializer = state->GetEffectData<SpatializerCore>();
		if (index >= P_NUM)
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}
		lock_guard<mutex> lock(spatializer->mutex);
		const float prevValue = spatializer->parameters[index];
		spatializer->parameters[index] = value;

		// Process command sent by C# API
		switch (index)
		{
		case PARAM_HEAD_RADIUS:	// Set listener head radius (OPTIONAL)
			spatializer->listener->SetHeadRadius(value);
			break;

		case PARAM_SCALE_FACTOR:
			// this is read directly from parameters array
			break;

		case PARAM_CUSTOM_ITD:	// Enable custom ITD (OPTIONAL)
			if (value != 0.0f)
			{
				spatializer->listener->EnableCustomizedITD();
			}
			else
			{
				spatializer->listener->DisableCustomizedITD();
			}
			break;

		case PARAM_MAG_ANECHATT:
		{
			Common::CMagnitudes magnitudes = spatializer->core.GetMagnitudes();
			magnitudes.SetAnechoicDistanceAttenuation(min(0.0f, max(-1.0e20f, value)));
			spatializer->core.SetMagnitudes(magnitudes);
		}
			break;

		case PARAM_MAG_SOUNDSPEED:
		{
			Common::CMagnitudes magnitudes = spatializer->core.GetMagnitudes();
			magnitudes.SetSoundSpeed(value);
			spatializer->core.SetMagnitudes(magnitudes);
		}
			break;

		case PARAM_HA_DIRECTIONALITY_EXTEND_LEFT:
			spatializer->listener->SetDirectionality_dB(Common::T_ear::LEFT, value);
			break;

		case PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT:
			spatializer->listener->SetDirectionality_dB(Common::T_ear::RIGHT, value);
			break;

		case PARAM_HA_DIRECTIONALITY_ON_LEFT:
			if (value > 0.0f)
			{
				spatializer->listener->EnableDirectionality(Common::T_ear::LEFT);
			}
			else
			{
				spatializer->listener->DisableDirectionality(Common::T_ear::LEFT);
			}
			break;

		case PARAM_HA_DIRECTIONALITY_ON_RIGHT:
			if (value > 0.0f)
			{
				spatializer->listener->EnableDirectionality(Common::T_ear::RIGHT);
			}
			else
			{
				spatializer->listener->DisableDirectionality(Common::T_ear::RIGHT);
			}
			break;

		case PARAM_LIMITER_SET_ON:
			// read directly from the parameter
			break;

		case PARAM_HRTF_STEP:
			spatializer->core.SetHRTFResamplingStep((int)value);
			break;

		//case PARAM_SPATIALIZATION_MODE:
		//	if (value != prevValue)
		//	{
		//		spatializer->loadBinaries();
		//	}
		//	break;

		case PARAM_IS_HIGH_QUALITY_HRTF_LOADED:
		case PARAM_IS_HIGH_QUALITY_ILD_LOADED:
		case PARAM_IS_HIGH_PERFORMANCE_ILD_LOADED:
		case PARAM_IS_REVERB_BRIR_LOADED:
			WriteLog("Error: Attempted to set read-only parameter "+to_string(index));
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;

		default:
			WriteLog("SET PARAMETER: ERROR!!!! Unknown float parameter received from API: ");
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}

		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char *valuestr)
	{

		SpatializerCore* spatializer = state->GetEffectData<SpatializerCore>();
		const lock_guard<mutex> lock(spatializer->mutex);

		if (index >= P_NUM)
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		if (value != NULL)
			*value = spatializer->parameters[index];
		if (valuestr != NULL)
			valuestr[0] = 0;
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	int UNITY_AUDIODSP_CALLBACK GetFloatBufferCallback(UnityAudioEffectState* state, const char* name, float* buffer, int numsamples)
	{
		// TO DO: should we do something here? I don't think so
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
	{	
		SpatializerCore* spatializer = state->GetEffectData<SpatializerCore>();

		if (inchannels != 2 || outchannels != 2 || spatializer->environment == nullptr)
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}

		const int bufferSize = spatializer->core.GetAudioState().bufferSize;

		if (bufferSize != length)
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}

		const lock_guard<mutex> lock(spatializer->mutex);

		// 7. Process reverb and generate the reverb output
		Common::CEarPair<CMonoBuffer<float>> bReverbOutput;
		bReverbOutput.left.resize(bufferSize);
		bReverbOutput.right.resize(bufferSize);
		//auto environment = std::atomic_load(&Spatializer3DTI::spatializer().environment);
		assert(bReverbOutput.left.size() == length && bReverbOutput.right.size() == length);
		if (spatializer->environment != nullptr)
		{
			spatializer->environment->ProcessVirtualAmbisonicReverb(bReverbOutput.left, bReverbOutput.right);

			for (size_t i = 0; i < length; i++)
			{
				outbuffer[i * 2 + 0] = bReverbOutput.left[i];
				outbuffer[i * 2 + 1] = bReverbOutput.right[i];
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




	SpatializerCore::SpatializerCore(UInt32 sampleRate, UInt32 bufferSize)
	{

		Common::TAudioStateStruct audioState;
		audioState.sampleRate = sampleRate;
		audioState.bufferSize = bufferSize;
		core.SetAudioState(audioState);
		listener = core.CreateListener();

		const float LimiterThreshold = -30.0f;
		const float LimiterAttack = 500.0f;
		const float LimiterRelease = 500.0f;
		const float LimiterRatio = 6;
		limiter.Setup(sampleRate, LimiterRatio, LimiterThreshold, LimiterAttack, LimiterRelease);
	}


	SpatializerCore::~SpatializerCore()
	{
		assert(instancePtr() == this);
		instancePtr() = nullptr;
	}


	bool SpatializerCore::loadBinaries(std::string hrtfPath, std::string ildPath, std::string highPerformanceILDPath, std::string brirPath)
	{
		std::lock_guard<std::mutex> lock(mutex);

		if (!hrtfPath.empty())
		{
#ifdef UNITY_WIN
			const string sofaExtension = ".sofa"s;
			if (hrtfPath.size() >= sofaExtension.size() && hrtfPath.substr(hrtfPath.size() - sofaExtension.size()) == sofaExtension)
			{
				// We assume an ILD file holds the delays, so our SOFA file does not specify delays
				bool specifiedDelays = false;
				parameters[PARAM_IS_HIGH_QUALITY_HRTF_LOADED] = HRTF::CreateFromSofa(hrtfPath, listener, specifiedDelays);
			}
			// If not sofa file then assume its a 3dti-hrtf file
			else
#endif
			{
				parameters[PARAM_IS_HIGH_QUALITY_HRTF_LOADED] = HRTF::CreateFrom3dti(hrtfPath, listener);
			}
		}
		if (!ildPath.empty())
		{
			parameters[PARAM_IS_HIGH_QUALITY_ILD_LOADED] = ILD::CreateFrom3dti_ILDNearFieldEffectTable(ildPath, listener);
		}

		if (!highPerformanceILDPath.empty())
		{
			parameters[PARAM_IS_HIGH_PERFORMANCE_ILD_LOADED] = ILD::CreateFrom3dti_ILDSpatializationTable(highPerformanceILDPath, listener);
		}

		if (!brirPath.empty())
		{
			environment = core.CreateEnvironment();
			parameters[PARAM_IS_REVERB_BRIR_LOADED] = BRIR::CreateFrom3dti(brirPath, environment);
		}

		return (hrtfPath.empty() || parameters[PARAM_IS_HIGH_QUALITY_HRTF_LOADED])
			&& (ildPath.empty() || parameters[PARAM_IS_HIGH_QUALITY_ILD_LOADED])
			&& (highPerformanceILDPath.empty() || parameters[PARAM_IS_HIGH_PERFORMANCE_ILD_LOADED])
			&& (brirPath.empty() || parameters[PARAM_IS_REVERB_BRIR_LOADED]);
	}


	Reverb3DTI::SpatializerCore* SpatializerCore::create(UInt32 sampleRate, UInt32 bufferSize)
	{
		if (instancePtr() != nullptr)
		{
			throw std::exception("Only one SpatializerCore can be created at once.");
		}
		instancePtr() = new SpatializerCore(sampleRate, bufferSize);
		return instancePtr();
	}


	Reverb3DTI::SpatializerCore* SpatializerCore::instance()
	{
		return instancePtr();
	}


	Reverb3DTI::SpatializerCore*& SpatializerCore::instancePtr()
	{
		static SpatializerCore* s(nullptr);
		return s;
	}

}

char const* Reverb3DTI::SpatializerCore::TooManyInstancesEception::what() const
{
	return "SpatializerCore already exists. Only one SpatializerCore instance is currently supported.";
}
