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
#include <fstream>
#include <iostream>
#include <time.h>
#include <HRTF/HRTFCereal.h>
#include "Common/AIR.h"
#include "effect3DTISpatializerSource.h"
#include "BRIR/BRIRCereal.h"
#include "HRTF/HRTFFactory.h"
#include "ILD/ILDCereal.h"

#include "effect3DTISpatializerCore.h"
#include "CommonUtils.h"

using namespace std;

/////////////////////////////////////////////////////////////////////

using namespace Binaural;
using namespace Common;




namespace SpatializerCore3DTI
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


	extern "C" UNITY_AUDIODSP_EXPORT_API bool Is3DTISpatializerCreated() {

		return SpatializerCore::instance() != nullptr;
	}


	extern "C" UNITY_AUDIODSP_EXPORT_API bool Load3DTISpatializerBinary(BinaryRole role, const char* path) {

		SpatializerCore* instance = SpatializerCore::instance();
		if (instance == nullptr)
		{
			WriteLog("Error: setup3DTISpatializer called before the Spatializer plugin was created.");
			return false;
		}
		return instance->loadBinary(role, path);
	}

	extern "C" UNITY_AUDIODSP_EXPORT_API bool Set3DTISpatializerFloat(int parameter, float value)
	{
		if (parameter < 0 || NumFloatParameters <= parameter)
		{
			return false;
		}

		SpatializerCore* spatializer = SpatializerCore::instance();
		if (spatializer == nullptr)
		{
			return false;
		}

		std::lock_guard<std::mutex> lock(spatializer->mutex);

		switch (parameter)
		{
			case EnableHRTFInterpolation :
			case EnableFarDistanceLPF:
			case EnableDistanceAttenuationAnechoic:
			case EnableNearFieldEffect:
			case SpatializationMode:
			case EnableReverb:
			case EnableDistanceAttenuationReverb:
				spatializer->perSourceInitialValues[parameter] = value;
				return true;

		case HeadRadius:
		{
			const float min = 0.0f;
			const float max = 1e20f;
			//const float def = 0.0875f;
			spatializer->listener->SetHeadRadius(clamp(value, min, max));
			return true;
		}
		case ScaleFactor:
		{
			const float min = 1e-20f;
			const float max = 1e20f;
			//const float def = 1.0f;
			spatializer->scaleFactor = clamp(value, min, max);
			return true;
		}
		case EnableCustomITD:
		{
			if (value == 0.0f)
			{
				spatializer->listener->DisableCustomizedITD();
			}
			else
			{
				spatializer->listener->EnableCustomizedITD();
			}
			return true;
		}
		case AnechoicDistanceAttenuation:
		{
			const float min = -30.0f;
			const float max = 0.0f;
			Common::CMagnitudes magnitudes = spatializer->core.GetMagnitudes();
			magnitudes.SetAnechoicDistanceAttenuation(clamp(value, min, max));
			spatializer->core.SetMagnitudes(magnitudes);
			return true;
		}
		case ILDAttenuation:
		{
			const float min = 0.0f;
			const float max = 30.0f;
			spatializer->listener->SetILDAttenutaion(clamp(value, min, max));
			return true;
		}
		case SoundSpeed:
		{
			const float min = 10.0f;
			const float max = 1000.0f;
			Common::CMagnitudes magnitudes = spatializer->core.GetMagnitudes();
			magnitudes.SetSoundSpeed(clamp(value, min, max));
			spatializer->core.SetMagnitudes(magnitudes);
			return true;
		}
		case HearingAidDirectionalityAttenuationLeft:
		{
			const float min = 0.0f;
			const float max = 30.0f;
			spatializer->listener->SetDirectionality_dB(Common::T_ear::LEFT, clamp(value, min, max));
			return true;
		}
		case HearingAidDirectionalityAttenuationRight:
		{
			const float min = 0.0f;
			const float max = 30.0f;
			spatializer->listener->SetDirectionality_dB(Common::T_ear::RIGHT, clamp(value, min, max));
			return true;
		}
		case EnableHearingAidDirectionalityLeft:
		{
			if (value == 0.0f)
			{
				spatializer->listener->DisableDirectionality(Common::T_ear::LEFT);
			}
			else
			{
				spatializer->listener->EnableDirectionality(Common::T_ear::LEFT);
			}
			return true;
		}
		case EnableHearingAidDirectionalityRight:
		{
			if (value == 0.0f)
			{
				spatializer->listener->DisableDirectionality(Common::T_ear::RIGHT);
			}
			else
			{
				spatializer->listener->EnableDirectionality(Common::T_ear::RIGHT);
			}
			return true;
		}
		case EnableLimiter:
		{
			spatializer->isLimiterEnabled = value != 0.0;
			return true;
		}
		case HRTFResamplingStep:
		{
			const float min = 1.0f;
			const float max = 90.0f;
			spatializer->core.SetHRTFResamplingStep((int)clamp(value, min, max));
			return true;
		}
		case ReverbOrder:
			static_assert((float)ADIMENSIONAL == 0.0f && (float)BIDIMENSIONAL == 1.0f && (float)THREEDIMENSIONAL == 2.0f, "These values are assumed by this code and the correspond c# enumerations.");
			if (value == (float)ADIMENSIONAL)
			{
				spatializer->environment->SetReverberationOrder(ADIMENSIONAL);
			}
			else if (value == (float)BIDIMENSIONAL)
			{
				spatializer->environment->SetReverberationOrder(BIDIMENSIONAL);
			}
			else if (value == (float)THREEDIMENSIONAL)
			{
				spatializer->environment->SetReverberationOrder(THREEDIMENSIONAL);
			}
			else
			{
				WriteLog("ERROR: Set3DTISpatializerFloat with parameter ReverbOrder only supports values 0.0, 1.0 and 2.0. Value received: " + to_string(value));
				return false;
			}
			return true;

		default:
			return false;
		}
	}

	extern "C" UNITY_AUDIODSP_EXPORT_API bool Get3DTISpatializerFloat(int parameter, float* value)
	{
		assert(value != nullptr);

		SpatializerCore* spatializer = SpatializerCore::instance();
		if (spatializer == nullptr || value == nullptr)
		{
			*value = std::numeric_limits<float>::quiet_NaN();
			return false;
		}

		switch (parameter)
		{
		case EnableHRTFInterpolation:
		case EnableFarDistanceLPF:
		case EnableDistanceAttenuationAnechoic:
		case EnableNearFieldEffect:
		case SpatializationMode:
		case EnableReverb:
		case EnableDistanceAttenuationReverb:
			*value = spatializer->perSourceInitialValues[parameter];
			return true;
		case HeadRadius:
			*value = spatializer->listener->GetHeadRadius();
			return true;
		case ScaleFactor:
			*value = spatializer->scaleFactor;
			return true;
		case EnableCustomITD:
			*value = spatializer->listener->IsCustomizedITDEnabled() ? 1.0f : 0.0f;
			return true;
		case AnechoicDistanceAttenuation:
			*value = spatializer->core.GetMagnitudes().GetAnechoicDistanceAttenuation();
			return true;
		case ILDAttenuation:
			*value = spatializer->listener->GetILDAttenutaion();
			return true;
		case SoundSpeed:
			*value = spatializer->core.GetMagnitudes().GetSoundSpeed();
			return true;
		case HearingAidDirectionalityAttenuationLeft:
			*value = spatializer->listener->GetAnechoicDirectionalityAttenuation_dB(LEFT);
			return true;
		case HearingAidDirectionalityAttenuationRight:
			*value = spatializer->listener->GetAnechoicDirectionalityAttenuation_dB(RIGHT);
			return true;
		case EnableHearingAidDirectionalityLeft:
			*value = spatializer->listener->IsDirectionalityEnabled(LEFT);
			return true;
		case EnableHearingAidDirectionalityRight:
			*value = spatializer->listener->IsDirectionalityEnabled(RIGHT);
			return true;
		case EnableLimiter:
			*value = spatializer->isLimiterEnabled ? 1.0f : 0.0f;
			return true;
		case HRTFResamplingStep:
			*value = (float) spatializer->core.GetHRTFResamplingStep();
			return true;
		case ReverbOrder:
			*value = (float)spatializer->environment->GetReverberationOrder();
			return true;
		default:
			*value = std::numeric_limits<float>::quiet_NaN();
			return false;
		}


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

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
		try
		{
			auto spatializer = SpatializerCore::create(state->samplerate, state->dspbuffersize);
			state->effectdata = spatializer;
		}
		catch (const SpatializerCore::TooManyInstancesException&)
		{
			WriteLog("Error: Attempted to create multiple Spatializer Core plugins. Only one is supported.");
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}

		return UNITY_AUDIODSP_OK;
	}

	/////////////////////////////////////////////////////////////////////


	int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
	{
		return 0;
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
		return UNITY_AUDIODSP_ERR_UNSUPPORTED;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char *valuestr)
	{
		return UNITY_AUDIODSP_ERR_UNSUPPORTED;

	}

/////////////////////////////////////////////////////////////////////

	int UNITY_AUDIODSP_CALLBACK GetFloatBufferCallback(UnityAudioEffectState* state, const char* name, float* buffer, int numsamples)
	{
		return UNITY_AUDIODSP_ERR_UNSUPPORTED;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
	{	
		SpatializerCore* spatializer = state->GetEffectData<SpatializerCore>();

		if (inchannels != 2 || outchannels != 2)
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
		assert(bReverbOutput.left.size() == length && bReverbOutput.right.size() == length);
		if (spatializer->isBinaryResourceLoaded[ReverbBRIR] != 0.0f)
		{
			assert(const_cast<CABIR&>(spatializer->environment->GetABIR()).IsInitialized());
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
		: scaleFactor(1.0f)
		, isLimiterEnabled(true)
	{
		perSourceInitialValues[EnableHRTFInterpolation] = 1.0f;
		perSourceInitialValues[EnableFarDistanceLPF] = 1.0f;
		perSourceInitialValues[EnableDistanceAttenuationAnechoic] = 1.0f;
		perSourceInitialValues[EnableNearFieldEffect] = 1.0f;
		perSourceInitialValues[SpatializationMode] = 0.0f;

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

		environment = core.CreateEnvironment();
	}


	SpatializerCore::~SpatializerCore()
	{
		assert(instancePtr() == this);
		instancePtr() = nullptr;
	}

	


	bool SpatializerCore::loadBinary(BinaryRole role, std::string path)
	{
		std::lock_guard<std::mutex> lock(mutex);

		const string sofaExtension = ".sofa"s;

		switch (role)
		{
		case HighQualityHRTF:
#ifdef UNITY_WIN
			if (path.size() >= sofaExtension.size() && path.substr(path.size() - sofaExtension.size()) == sofaExtension)
			{
				// We assume an ILD file holds the delays, so our SOFA file does not specify delays
				bool specifiedDelays = false;
				isBinaryResourceLoaded[HighQualityHRTF] = HRTF::CreateFromSofa(path, listener, specifiedDelays);
			}
			// If not sofa file then assume its a 3dti-hrtf file
			else
#endif
			{
				isBinaryResourceLoaded[HighQualityHRTF] = HRTF::CreateFrom3dti(path, listener);
			}
			return isBinaryResourceLoaded[HighQualityHRTF];
		case HighQualityILD:
			isBinaryResourceLoaded[HighQualityILD] = ILD::CreateFrom3dti_ILDNearFieldEffectTable(path, listener);
			return isBinaryResourceLoaded[HighQualityILD];
		case HighPerformanceILD:
			isBinaryResourceLoaded[HighPerformanceILD] = ILD::CreateFrom3dti_ILDSpatializationTable(path, listener);
			return isBinaryResourceLoaded[HighPerformanceILD];
		case ReverbBRIR:
			isBinaryResourceLoaded[ReverbBRIR] = BRIR::CreateFrom3dti(path, environment);
			return isBinaryResourceLoaded[ReverbBRIR];
		default:
			return false;
		}
	}


	SpatializerCore3DTI::SpatializerCore* SpatializerCore::create(UInt32 sampleRate, UInt32 bufferSize)
	{
		if (instancePtr() != nullptr)
		{
			throw std::exception("Only one SpatializerCore can be created at once.");
		}
		instancePtr() = new SpatializerCore(sampleRate, bufferSize);
		return instancePtr();
	}


	SpatializerCore3DTI::SpatializerCore* SpatializerCore::instance()
	{
		return instancePtr();
	}


	SpatializerCore3DTI::SpatializerCore*& SpatializerCore::instancePtr()
	{
		static SpatializerCore* s(nullptr);
		return s;
	}

}

char const* SpatializerCore3DTI::SpatializerCore::TooManyInstancesException::what() const
{
	return "SpatializerCore already exists. Only one SpatializerCore instance is currently supported.";
}
