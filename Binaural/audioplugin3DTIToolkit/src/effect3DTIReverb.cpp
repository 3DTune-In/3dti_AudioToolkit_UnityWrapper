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

using namespace std;

// DEBUG LOG FILE
//#define LOG_FILE
void WriteLog(string logText)
{
	std::cerr << logText << std::endl;
#ifdef LOG_FILE
	string channel="Undefined channel";
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

/////////////////////////////////////////////////////////////////////

using namespace Binaural;
using namespace Common;



namespace Reverb3DTI
{
	enum SpatializationMode : int
	{
		SPATIALIZATION_MODE_HIGH_QUALITY = 0,
		SPATIALIZATION_MODE_HIGH_PERFORMANCE = 1,
		SPATIALIZATION_MODE_NONE = 2,
	};

	enum
	{
		PARAM_HEAD_RADIUS,
		PARAM_CUSTOM_ITD,
		PARAM_MOD_FARLPF,
		PARAM_MOD_DISTATT,
		PARAM_MOD_NEAR_FIELD_ILD,
		PARAM_MAG_ANECHATT, // 10
		PARAM_MAG_SOUNDSPEED,

		// HA directionality
		PARAM_HA_DIRECTIONALITY_EXTEND_LEFT,
		PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT, // 15
		PARAM_HA_DIRECTIONALITY_ON_LEFT,
		PARAM_HA_DIRECTIONALITY_ON_RIGHT,

		// Limiter
		PARAM_LIMITER_SET_ON,


		// HRTF resampling step
		PARAM_HRTF_STEP,

		// High Performance and None modes
		//PARAM_HIGH_PERFORMANCE_ILD_FILE_STRING,
		PARAM_SPATIALIZATION_MODE,
		//PARAM_BUFFER_SIZE,
		//PARAM_SAMPLE_RATE,
		//PARAM_BUFFER_SIZE_CORE,
		//PARAM_SAMPLE_RATE_CORE,
		// Read only status parameters
		PARAM_IS_READY,

		P_NUM
	};

	// readonly parameters to mvoe to getfloatbuffer method
	//PARAM_LIMITER_GET_COMPRESSION,
		//PARAM_IS_CORE_READY, // 20

	

	

	
/////////////////////////////////////////////////////////////////////
	
	struct SpatializerCore
	{
		// Each instance of the reverb effect has an instance of the Core
		Binaural::CCore core;
		std::shared_ptr<Binaural::CListener> listener;
		std::shared_ptr<Binaural::CEnvironment> environment;
		Common::CDynamicCompressorStereo limiter;
		float parameters[P_NUM];
		std::mutex mutex;

		SpatializerCore(UInt32 sampleRate, UInt32 bufferSize)
		{
			sInstances.push_back(this);
			// do this to force the setFloat initialization to trigger a call to loadBinaries when it is first called()
			parameters[PARAM_SPATIALIZATION_MODE] = SPATIALIZATION_MODE_NONE;

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

		~SpatializerCore()
		{
			auto it = find(sInstances.begin(), sInstances.end(), this);
			assert(it != sInstances.end());
			if (it != sInstances.end())
			{
				sInstances.erase(it);
			}
			assert(find(sInstances.begin(), sInstances.end(), this) == sInstances.end());
		}

		bool loadBinaries()
		{
			std::lock_guard<std::mutex> lock(mutex);

			bool loadOK = true;
			if (parameters[PARAM_SPATIALIZATION_MODE] == SPATIALIZATION_MODE_HIGH_QUALITY)
			{
#ifdef UNITY_WIN
				const string sofaExtension = ".sofa"s;
				if (hrtfPath.size() >= sofaExtension.size() && hrtfPath.substr(hrtfPath.size() - sofaExtension.size()) == sofaExtension)
				{
					// We assume an ILD file holds the delays, so our SOFA file does not specify delays
					bool specifiedDelays = false;
					loadOK = HRTF::CreateFromSofa(hrtfPath, listener, specifiedDelays) && loadOK;
				}
				// If not sofa file then assume its a 3dti-hrtf file
				else
#endif
				{
					loadOK = !hrtfPath.empty() && HRTF::CreateFrom3dti(hrtfPath, listener) && loadOK;
				}
				loadOK = !ildPath.empty() && ILD::CreateFrom3dti_ILDNearFieldEffectTable(ildPath, listener) && loadOK;
			}
			else if (parameters[PARAM_SPATIALIZATION_MODE] == SPATIALIZATION_MODE_HIGH_PERFORMANCE)
			{
					loadOK = !highPerformanceILDPath.empty() && ILD::CreateFrom3dti_ILDSpatializationTable(highPerformanceILDPath, listener) && loadOK;
			}
			if (!brirPath.empty())
			{
				environment = Spatializer3DTI::spatializer().core.CreateEnvironment();
				loadOK = BRIR::CreateFrom3dti(brirPath, environment) && loadOK;
			}

			parameters[PARAM_IS_READY] = loadOK;

			return loadOK;
		}

		static string hrtfPath;
		static string ildPath;
		static string highPerformanceILDPath;
		static string brirPath;

		static vector<SpatializerCore*> instances() { return sInstances; }

	private:
		static vector<SpatializerCore*> sInstances;
	};

	string SpatializerCore::hrtfPath;
	string SpatializerCore::ildPath;
	string SpatializerCore::highPerformanceILDPath;
	string SpatializerCore::brirPath;
	vector<SpatializerCore*> SpatializerCore::sInstances;




	extern "C" UNITY_AUDIODSP_EXPORT_API bool setup3DTISpatializer(const char* hrtfPath, const char* ildPath, const char* highPerformanceILDPath, const char* brirPath) {

		SpatializerCore::hrtfPath = hrtfPath;
		SpatializerCore::ildPath = ildPath;
		SpatializerCore::highPerformanceILDPath = highPerformanceILDPath;
		SpatializerCore::brirPath = brirPath;
		auto instances = SpatializerCore::instances();
		return all_of(instances.begin(), instances.end(), [](auto instance) {
			return instance->loadBinaries();
			});

		//auto spat = spatializer();
		//if (!spat.isInitialized())
		//{
		//	spatializer().initialize(sampleRate, dspBufferSize);
		//	auto env = core.CreateEnvironment();
		//	

		//std::ifstream brirStream(brirPath, std::ifstream::binary);
		//if (brirStream)
		//{
		//	Spatializer3DTI::Spatializer& spat = Spatializer3DTI::spatializer();
		//	assert(spat.isInitialized());
		//	auto environment = spat.core.CreateEnvironment();
		//	if (BRIR::CreateFrom3dtiStream(brirStream, environment))
		//	{
		//		std::atomic_store(&spat.environment, environment);
		//		return true;
		//	}

		//	//// get length of file:
		//	//brirStream.seekg(0, brirStream.end);
		//	//int length = brirStream.tellg();
		//	//brirStream.seekg(0, brirStream.beg);

		//	//vector<uint8_t> brirBuffer(length);

		//	//std::cout << "Reading " << length << " characters... ";
		//	//// read data as a block:
		//	//brirStream.read(brirBuffer.data(), length);

		//	//if (brirStream)
		//	//	std::cout << "all characters read successfully.";
		//	//else
		//	//	std::cout << "error: only " << brirStream.gcount() << " could be read";
		//	//brirStream.close();


		//	//// ...buffer contains the entire file...

		//	//delete[] brirBuffer;
		//}
		//else
		//{
		//	return false;
		//}
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
		RegisterParameter(definition, "HeadRadius", "m", 0.0f, /*FLT_MAX*/ 1e20, 0.0875f, 1.0f, 1.0f, PARAM_HEAD_RADIUS, "Listener head radius");
		RegisterParameter(definition, "CustomITD", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_CUSTOM_ITD, "Enabled custom ITD");
		RegisterParameter(definition, "MODfarLPF", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_FARLPF, "Far distance LPF module enabler");
		RegisterParameter(definition, "MODDistAtt", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_DISTATT, "Distance attenuation module enabler");
		RegisterParameter(definition, "MODNFILD", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_NEAR_FIELD_ILD, "Near distance ILD module enabler");
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
		RegisterParameter(definition, "SpatMode", "", 0.0f, 2.0f, 0.0f, 1.0f, 1.0f, PARAM_SPATIALIZATION_MODE, "Spatialization mode (0=High quality, 1=High performance, 2=None)");
		// readonly
		RegisterParameter(definition, "SpatIsReady", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_IS_READY, "Is spatializer initialized and ready");

		return numparams;
	}


/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
		SpatializerCore* spatializer = new SpatializerCore(state->samplerate, state->dspbuffersize);
		state->effectdata = spatializer;

		// Initialization will happen when the SPATIALIZATION_MODE parameter is set.


		InitParametersFromDefinitions(InternalRegisterEffectDefinition, spatializer->parameters);
	
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
	{
		SpatializerCore* data = state->GetEffectData<SpatializerCore>();
		delete data;
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

		case PARAM_SPATIALIZATION_MODE:
			if (value != prevValue)
			{
				spatializer->loadBinaries();
			}
			break;

		default:
			WriteLog("SET PARAMETER: ERROR!!!! Unknown float parameter received from API: ");
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
			break;
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

		const int bufferSize = Spatializer3DTI::spatializer().core.GetAudioState().bufferSize;

		if (bufferSize != length)
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}

		const lock_guard<mutex> lock(spatializer->mutex);

		// 7. Process reverb and generate the reverb output
		Common::CEarPair<CMonoBuffer<float>> bReverbOutput;
		bReverbOutput.left.resize(bufferSize);
		bReverbOutput.right.resize(bufferSize);
		auto environment = std::atomic_load(&Spatializer3DTI::spatializer().environment);
		assert(bReverbOutput.left.size() == length && bReverbOutput.right.size() == length);
		if (environment != nullptr)
		{
			environment->ProcessVirtualAmbisonicReverb(bReverbOutput.left, bReverbOutput.right);

			for (int i = 0; i < length; i++)
			{
				outbuffer[i * 2 + 0] = bReverbOutput.left[i];
				outbuffer[i * 2 + 1] = bReverbOutput.right[i];
			}
		}
		else
		{
			for (int i = 0; i < length * 2; i++)
			{
				outbuffer[i] = inbuffer[i];
			}
		}

		return UNITY_AUDIODSP_OK;



	}
}
