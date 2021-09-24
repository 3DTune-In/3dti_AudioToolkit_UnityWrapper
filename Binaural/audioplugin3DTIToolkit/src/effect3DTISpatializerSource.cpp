/**
*** 3D-Tune-In Toolkit Unity Wrapper: Binaural Spatializer ***
*
* Created on: February 2017
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

#include "effect3DTISpatializerSource.h"

//#include <BinauralSpatializer\3DTI_BinauralSpatializer.h>
#include <Common/ErrorHandler.h>


// Includes for debug logging
#include <fstream>
#include <iostream>

#include <mutex>

#include <sstream>
#include <cstdint>

#include <HRTF/HRTFCereal.h>
#include <ILD/ILDCereal.h>

enum TLoadResult { RESULT_LOAD_WAITING = 0, RESULT_LOAD_CONTINUE = 1, RESULT_LOAD_END = 2, RESULT_LOAD_OK = 3, RESULT_LOAD_ERROR = -1 };

// DEBUG LOG 
#ifdef UNITY_ANDROID
#define DEBUG_LOG_CAT
#else
#define DEBUG_LOG_FILE_BINSP
#endif

#ifdef DEBUG_LOG_CAT
#include <android/log.h> 
#include <string>
#include <sstream>
#endif

#include <cfloat>
#include "HRTF/HRTFFactory.h"
#include "effect3DTISpatializerCore.h"
#include "CommonUtils.h"

/////////////////////////////////////////////////////////////////////

namespace SpatializerSource3DTI
{


	using SpatializerCore3DTI::FloatParameter;




	struct EffectData
	{
		int sourceID;    // DEBUG
		std::shared_ptr<Binaural::CSingleSourceDSP> audioSource;
		SpatializerCore3DTI::SpatializerCore* spatializer;
	};



	template <class T>
	void WriteLog(string logText, const T& value, int sourceID = -1)
	{
		std::cerr << logText << " " << value;
		if (sourceID >= 0)
		{
			std::cerr << " (source " << sourceID << ")";
		}
		std::cerr << std::endl;
		//
		//    if (spatializer().debugLog)
		//    {
		//#ifdef DEBUG_LOG_FILE_BINSP
		//        ofstream logfile;
		//        logfile.open("3DTI_BinauralSpatializer_DebugLog.txt", ofstream::out | ofstream::app);
		//        if (sourceID != -1)
		//            logfile << sourceID << ": " << logtext << value << endl;
		//        else
		//            logfile << logtext << value << endl;
		//        logfile.close();
		//#endif
		//        
		//#ifdef DEBUG_LOG_CAT
		//        std::ostringstream os;
		//        os << logtext << value;
		//        string fulltext = os.str();
		//        __android_log_print(ANDROID_LOG_DEBUG, "3DTISPATIALIZER", fulltext.c_str());
		//#endif
	}


template <class T>
void WriteLog(UnityAudioEffectState* state, string logtext, const T& value)
{
	WriteLog(logtext, value, state->GetEffectData<EffectData>()->sourceID);
}

void WriteLog(string logtext)
{
	WriteLog(logtext, "");
	//std::cerr << logtext << std::endl;
}



/////////////////////////////////////////////////////////////////////




	/////////////////////////////////////////////////////////////////////

int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
{
	int numparams = FloatParameter::NumSourceParameters;
	definition.paramdefs = new UnityAudioParameterDefinition[numparams];
	//RegisterParameter(definition, "SourceID", "", -1.0f, /*FLT_MAX*/ 1e20f, -1.0f, 1.0f, 1.0f, PARAM_SOURCE_ID, "Source ID for debug");
	RegisterParameter(definition, "HRTFInterp", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, FloatParameter::EnableHRTFInterpolation, "HRTF Interpolation method");
	RegisterParameter(definition, "MODfarLPF", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, FloatParameter::EnableFarDistanceLPF, "Far distance LPF module enabler");
	RegisterParameter(definition, "MODDistAtt", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, FloatParameter::EnableDistanceAttenuationAnechoic, "Enable distance attenuation for anechoic processing");
	RegisterParameter(definition, "MODNFILD", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, FloatParameter::EnableNearFieldEffect, "Near distance ILD module enabler");
	RegisterParameter(definition, "SpatMode", "", 0.0f, 2.0f, 0.0f, 1.0f, 1.0f, FloatParameter::SpatializationMode, "Spatialization mode (0=High quality, 1=High performance, 2=None)");
	RegisterParameter(definition, "EnableReverb", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, FloatParameter::EnableReverb, "Enable reverb processing");
	RegisterParameter(definition, "RevDistAtt", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, FloatParameter::EnableDistanceAttenuationReverb, "Enable distance attenuation for reverb processing");
	//Sample Rate and BufferSize
	definition.flags |= UnityAudioEffectDefinitionFlags_IsSpatializer;
	return numparams;
}

/////////////////////////////////////////////////////////////////////

Common::CTransform ComputeListenerTransformFromMatrix(float* listenerMatrix, float scale)
{
	// SET LISTENER POSITION

	// Inverted 4x4 listener matrix, as provided by Unity
	float L[16];
	for (int i = 0; i < 16; i++)
		L[i] = listenerMatrix[i];

	float listenerpos_x = -(L[0] * L[12] + L[1] * L[13] + L[2] * L[14]) * scale;	// From Unity documentation, if listener is rotated
	float listenerpos_y = -(L[4] * L[12] + L[5] * L[13] + L[6] * L[14]) * scale;	// From Unity documentation, if listener is rotated
	float listenerpos_z = -(L[8] * L[12] + L[9] * L[13] + L[10] * L[14]) * scale;	// From Unity documentation, if listener is rotated
	//float listenerpos_x = -L[12] * scale;	// If listener is not rotated
	//float listenerpos_y = -L[13] * scale;	// If listener is not rotated
	//float listenerpos_z = -L[14] * scale;	// If listener is not rotated
	Common::CTransform listenerTransform;
	listenerTransform.SetPosition(Common::CVector3(listenerpos_x, listenerpos_y, listenerpos_z));

	// SET LISTENER ORIENTATION

	//float w = 2 * sqrt(1.0f + L[0] + L[5] + L[10]);
	//float qw = w / 4.0f;
	//float qx = (L[6] - L[9]) / w;
	//float qy = (L[8] - L[2]) / w;
	//float qz = (L[1] - L[4]) / w;
	// http://forum.unity3d.com/threads/how-to-assign-matrix4x4-to-transform.121966/
	float tr = L[0] + L[5] + L[10];
	float w, qw, qx, qy, qz;
	if (tr > 0.0f)			// General case
	{
		w = sqrt(1.0f + tr) * 2.0f;
		qw = 0.25f * w;
		qx = (L[6] - L[9]) / w;
		qy = (L[8] - L[2]) / w;
		qz = (L[1] - L[4]) / w;
	}
	// Cases with w = 0
	else if ((L[0] > L[5]) && (L[0] > L[10]))
	{
		w = sqrt(1.0f + L[0] - L[5] - L[10]) * 2.0f;
		qw = (L[6] - L[9]) / w;
		qx = 0.25f * w;
		qy = -(L[1] + L[4]) / w;
		qz = -(L[8] + L[2]) / w;
	}
	else if (L[5] > L[10])
	{
		w = sqrt(1.0f + L[5] - L[0] - L[10]) * 2.0f;
		qw = (L[8] - L[2]) / w;
		qx = -(L[1] + L[4]) / w;
		qy = 0.25f * w;
		qz = -(L[6] + L[9]) / w;
	}
	else
	{
		w = sqrt(1.0f + L[10] - L[0] - L[5]) * 2.0f;
		qw = (L[1] - L[4]) / w;
		qx = -(L[8] + L[2]) / w;
		qy = -(L[6] + L[9]) / w;
		qz = 0.25f * w;
	}

	Common::CQuaternion unityQuaternion = Common::CQuaternion(qw, qx, qy, qz);
	listenerTransform.SetOrientation(unityQuaternion.Inverse());
	return listenerTransform;
}

/////////////////////////////////////////////////////////////////////

Common::CTransform ComputeSourceTransformFromMatrix(float* sourceMatrix, float scale)
{
	// Orientation does not matters for audio sources
	Common::CTransform sourceTransform;
	sourceTransform.SetPosition(Common::CVector3(sourceMatrix[12] * scale, sourceMatrix[13] * scale, sourceMatrix[14] * scale));
	return sourceTransform;
}


	/////////////////////////////////////////////////////////////////////

void WriteLogHeader(UnityAudioEffectState* state)
{
	EffectData* data = state->GetEffectData<EffectData>();

	SpatializerCore3DTI::SpatializerCore* spatializer = data->spatializer;

	// Audio state:
	Common::TAudioStateStruct audioState = spatializer->core.GetAudioState();
	WriteLog(state, "CREATE: Sample rate set to ", audioState.sampleRate);
	WriteLog(state, "CREATE: Buffer size set to ", audioState.bufferSize);
	WriteLog(state, "CREATE: HRTF resampling step set to ", spatializer->core.GetHRTFResamplingStep());

	// Listener:
	if (spatializer->listener != nullptr)
		WriteLog(state, "CREATE: Listener created successfully", "");
	else
		WriteLog(state, "CREATE: ERROR!!!! Listener creation returned null pointer!", "");

	// Source:
	if (data->audioSource != nullptr)
		WriteLog(state, "CREATE: Source created successfully", "");
	else
		WriteLog(state, "CREATE: ERROR!!!! Source creation returned null pointer!", "");

	WriteLog(state, "--------------------------------------", "\n");
}

/////////////////////////////////////////////////////////////////////
// AUDIO PLUGIN SDK FUNCTIONS
/////////////////////////////////////////////////////////////////////

static UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK DistanceAttenuationCallback(UnityAudioEffectState* state, float distanceIn, float attenuationIn, float* attenuationOut)
{
	*attenuationOut = attenuationIn;
	return UNITY_AUDIODSP_OK;
}

/////////////////////////////////////////////////////////////////////

UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
{
	// CREATE Instance state and grab parameters

	EffectData* effectdata = new EffectData;
	{
		effectdata->spatializer = SpatializerCore3DTI::SpatializerCore::instance();
		static_assert(std::tuple_size<decltype(effectdata->spatializer->perSourceInitialValues)>::value == FloatParameter::NumSourceParameters, "NumSourceParameters should match the size of SpatializerCore::perSourceInitialValues array.");
		//for (int i = 0; i < NumSourceParameters; i++)
		//{
		//	effectdata->parameters[i] = effectdata->spatializer->perSourceInitialValues[i];
		//}

		state->effectdata = effectdata;
		if (IsHostCompatible(state))
		{
			state->spatializerdata->distanceattenuationcallback = DistanceAttenuationCallback;
		}
		effectdata->sourceID = -1;

		if (effectdata->spatializer == nullptr)
		{
			WriteLog("Error: Created spatialized audio source but there no SpatializerCore plugin has been created yet.");
			delete state->effectdata;
			state->effectdata = nullptr;
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}
	}
	auto spatializer = effectdata->spatializer;


	{
		std::lock_guard<std::mutex> lock(spatializer->mutex);
		// Create source and set default interpolation method
		effectdata->audioSource = spatializer->core.CreateSingleSourceDSP();
	}
	if (effectdata->audioSource != nullptr)
	{
		// Initialize with defaults
		for (int i = FloatParameter::FirstSourceParameter; i < FloatParameter::NumSourceParameters; i++)
		{
			float value = 0;
			bool valueReceived = SpatializerCore3DTI::Get3DTISpatializerFloat(i, &value);
			assert(valueReceived);
			SetFloatParameterCallback(state, i, value);
		}
	}


	// 3DTI Debugger
#if defined (SWITCH_ON_3DTI_ERRORHANDLER) || defined (_3DTI_ANDROID_ERRORHANDLER)
	Common::CErrorHandler::Instance().SetAssertMode(ASSERT_MODE_CONTINUE);
#endif


	return UNITY_AUDIODSP_OK;
}

/////////////////////////////////////////////////////////////////////

UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
{
	WriteLog(state, "Releasing audio plugin...", "");
	EffectData* data = state->GetEffectData<EffectData>();
	delete data;
	return UNITY_AUDIODSP_OK;
}


/////////////////////////////////////////////////////////////////////


// lockMutex is not part of the unity callback but it has a default value set in the earlier declaration.
UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
{
	EffectData* data = state->GetEffectData<EffectData>();
	SpatializerCore3DTI::SpatializerCore* spatializer = data->spatializer;
	assert(data != nullptr && spatializer != nullptr);
	if (index >= FloatParameter::NumSourceParameters)
		return UNITY_AUDIODSP_ERR_UNSUPPORTED;

	std::lock_guard<std::mutex> lock(spatializer->mutex);


	// Process command sent by C# API
	switch (index)
	{

	case FloatParameter::EnableHRTFInterpolation:	// Change interpolation method (OPTIONAL)
		if (value != 0.0f)
		{
			data->audioSource->EnableInterpolation();
			WriteLog(state, "SET PARAMETER: Run-time HRTF Interpolation switched ", "ON");
		}
		else
		{
			data->audioSource->DisableInterpolation();
			WriteLog(state, "SET PARAMETER: Run-time HRTF Interpolation switched ", "OFF");
		}
		break;

	case FloatParameter::EnableFarDistanceLPF:
		if (value > 0.0f)
		{
			data->audioSource->EnableFarDistanceEffect();
			WriteLog(state, "SET PARAMETER: Far distance LPF is ", "Enabled");
		}
		else
		{
			data->audioSource->DisableFarDistanceEffect();
			WriteLog(state, "SET PARAMETER: Far distance LPF is ", "Disabled");
		}
		break;

	case FloatParameter::EnableDistanceAttenuationAnechoic:
		if (value > 0.0f)
		{
			data->audioSource->EnableDistanceAttenuationAnechoic();
			WriteLog(state, "SET PARAMETER: Distance attenuation is ", "Enabled");
		}
		else
		{
			data->audioSource->DisableDistanceAttenuationAnechoic();
			WriteLog(state, "SET PARAMETER: Distance attenuation is ", "Disabled");
		}
		break;

	case FloatParameter::EnableNearFieldEffect:
		if (value > 0.0f)
		{
			data->audioSource->EnableNearFieldEffect();
			WriteLog(state, "SET PARAMETER: Near Field ILD is ", "Enabled");
		}
		else
		{
			data->audioSource->DisableNearFieldEffect();
			WriteLog(state, "SET PARAMETER: Near Field ILD is ", "Disabled");
		}
		break;

	case FloatParameter::SpatializationMode:
		if (value == (float)Binaural::TSpatializationMode::HighQuality)
		{
			data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::HighQuality);
		}
		else if (value == (float)Binaural::TSpatializationMode::HighPerformance)
		{
			data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::HighPerformance);
		}
		else if (value == (float)Binaural::TSpatializationMode::NoSpatialization)
		{
			data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::NoSpatialization);
			WriteLog(state, "SET PARAMETER: No spatialization mode is enabled", "");
		}
		else
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}
		break;
	case FloatParameter::EnableReverb:
		if (value != 0.0f)
		{
			data->audioSource->EnableReverbProcess();
		}
		else
		{
			data->audioSource->DisableReverbProcess();
		}
		break;
	case FloatParameter::EnableDistanceAttenuationReverb:
		if (value != 0.0f)
		{
			data->audioSource->EnableDistanceAttenuationReverb();
		}
		else
		{
			data->audioSource->DisableDistanceAttenuationReverb();
		}
		break;
	default:
		WriteLog(state, "SET PARAMETER: ERROR!!!! Unknown float parameter received from API: ", index);
		return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		break;
	}


	return UNITY_AUDIODSP_OK;
}

/////////////////////////////////////////////////////////////////////

UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char* valuestr)
{
	EffectData* data = state->GetEffectData<EffectData>();
	if (index < FloatParameter::FirstSourceParameter || index >= FloatParameter::NumSourceParameters)
	{
		return UNITY_AUDIODSP_ERR_UNSUPPORTED;
	}
	std::shared_ptr<Binaural::CSingleSourceDSP> source = data->audioSource;
	assert(source != nullptr);
	if (source == nullptr)
	{
		return UNITY_AUDIODSP_ERR_UNSUPPORTED;
	}
	if (valuestr != NULL)
	{
		valuestr[0] = '\0';
	}

	if (value != NULL)
	{
		switch (index)
		{
		case FloatParameter::EnableHRTFInterpolation:
			*value = (float) source->IsInterpolationEnabled();
			break;
		case FloatParameter::EnableFarDistanceLPF:
			*value = (float)source->IsFarDistanceEffectEnabled();
			break;
		case FloatParameter::EnableDistanceAttenuationAnechoic:
			*value = (float)source->IsDistanceAttenuationEnabledAnechoic();
			break;
		case FloatParameter::EnableNearFieldEffect:
			*value = (float)source->IsNearFieldEffectEnabled();
			break;
		case FloatParameter::SpatializationMode:
			*value = (float)source->GetSpatializationMode();
			break;
		case FloatParameter::EnableReverb:
			*value = (float)source->IsReverbProcessEnabled();
			break;
		case FloatParameter::EnableDistanceAttenuationReverb:
			*value = (float)source->IsDistanceAttenuationEnabledReverb();
			break;
		default:
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}
	}
	return UNITY_AUDIODSP_OK;
}

/////////////////////////////////////////////////////////////////////

int UNITY_AUDIODSP_CALLBACK GetFloatBufferCallback(UnityAudioEffectState* state, const char* name, float* buffer, int numsamples)
{
	return UNITY_AUDIODSP_OK;
}

/////////////////////////////////////////////////////////////////////

UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
{
	// Check that I/O formats are right and that the host API supports this feature
	if (inchannels != 2 || outchannels != 2 ||
		!IsHostCompatible(state) || state->spatializerdata == NULL)
	{
		WriteLog(state, "PROCESS: ERROR!!!! Wrong number of channels or Host is not compatible:", "");
		WriteLog(state, "         Input channels = ", inchannels);
		WriteLog(state, "         Output channels = ", outchannels);
		WriteLog(state, "         Host compatible = ", IsHostCompatible(state));
		WriteLog(state, "         Spatializer data exists = ", (state->spatializerdata != NULL));
		WriteLog(state, "         Buffer length = ", length);
		// Return silence on error.
		std::fill(outbuffer, outbuffer + length * (size_t)outchannels, 0.0f);
		return UNITY_AUDIODSP_OK;
	}

	EffectData* data = state->GetEffectData<EffectData>();

	SpatializerCore3DTI::SpatializerCore* spatializer = data->spatializer;
	assert(spatializer != nullptr);

	std::lock_guard<std::mutex> lock(spatializer->mutex);

	

	if ((data->audioSource->GetSpatializationMode() == Binaural::HighQuality && !spatializer->isBinaryResourceLoaded[SpatializerCore3DTI::HighQualityHRTF])
		||
		(data->audioSource->GetSpatializationMode() == Binaural::HighPerformance && !spatializer->isBinaryResourceLoaded[SpatializerCore3DTI::HighPerformanceILD])
		||
		(data->audioSource->IsNearFieldEffectEnabled() && !spatializer->isBinaryResourceLoaded[SpatializerCore3DTI::HighQualityILD])
		)
	{
		WriteLog(state, "PROCESS: ERROR: The required binaries are not loaded.", "");
		// Return silence on error.
		std::fill(outbuffer, outbuffer + length * (size_t)outchannels, 0.0f);
		return UNITY_AUDIODSP_OK;
	}

	  // Set source and listener transforms
	data->audioSource->SetSourceTransform(ComputeSourceTransformFromMatrix(state->spatializerdata->sourcematrix, spatializer->scaleFactor));
	spatializer->listener->SetListenerTransform(ComputeListenerTransformFromMatrix(state->spatializerdata->listenermatrix, spatializer->scaleFactor));

	// Transform input buffer
	CMonoBuffer<float> inMonoBuffer(length);
	size_t j = 0;
	for (size_t i = 0; i < length; i++)
	{
		inMonoBuffer[i] = (inbuffer[j] + inbuffer[j + 1]) / 2.0f;	// We take average of left and right channels
		j += 2;
	}

	// Process!!
	CMonoBuffer<float> outLeftBuffer(length);
	CMonoBuffer<float> outRightBuffer(length);
	data->audioSource->SetBuffer(inMonoBuffer);
	data->audioSource->ProcessAnechoic(outLeftBuffer, outRightBuffer);

	// Limiter
	CStereoBuffer<float> outStereoBuffer;
	outStereoBuffer.Interlace(outLeftBuffer, outRightBuffer);
	if (spatializer->isLimiterEnabled)
	{
		spatializer->limiter.Process(outStereoBuffer);
	}

	// Transform output buffer
	size_t i = 0;
	for (auto it = outStereoBuffer.begin(); it != outStereoBuffer.end(); it++)
	{
		outbuffer[i++] = *it;
	}

	return UNITY_AUDIODSP_OK;
}
}
