/**
*** 3D-Tune-In Toolkit Unity Wrapper: Binaural Spatializer ***
*
* version 1.7
* Created on: February 2017
*
* Author: 3DI-DIANA Research Group / University of Malaga / Spain
* Contact: areyes@uma.es
*
* Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
* Module: 3DTI Toolkit Unity Wrapper
**/

#include "AudioPluginUtil.h"
//#include <BinauralSpatializer\3DTI_BinauralSpatializer.h>
#include <Common/DynamicCompressorStereo.h>
#include <BinauralSpatializer/Core.h>
#include <Common/ErrorHandler.h>


// Includes for debug logging
#include <fstream>
#include <iostream>

#include <mutex>

#include <HRTF/HRTFCereal.h>
#include <ILD/ILDCereal.h>

enum TLoadResult { RESULT_LOAD_WAITING = 0, RESULT_LOAD_CONTINUE=1, RESULT_LOAD_END=2, RESULT_LOAD_OK=3, RESULT_LOAD_ERROR = -1 };

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

/////////////////////////////////////////////////////////////////////

namespace Spatializer3DTI
{

#define LIMITER_THRESHOLD	-30.0f
#define LIMITER_ATTACK		500.0f
#define LIMITER_RELEASE		500.0f
#define LIMITER_RATIO		6

#define SPATIALIZATION_MODE_HIGH_QUALITY		0
#define SPATIALIZATION_MODE_HIGH_PERFORMANCE	1
#define SPATIALIZATION_MODE_NONE				2

    enum
    {
		PARAM_HRTF_FILE_STRING,
		PARAM_HEAD_RADIUS,
		PARAM_SCALE_FACTOR,
		PARAM_SOURCE_ID,	// DEBUG
		PARAM_CUSTOM_ITD,
		PARAM_HRTF_INTERPOLATION,
		PARAM_MOD_FARLPF,
		PARAM_MOD_DISTATT,
		PARAM_MOD_NEAR_FIELD_ILD,
		PARAM_MOD_HRTF,
		PARAM_MAG_ANECHATT,
		PARAM_MAG_SOUNDSPEED,
		PARAM_NEAR_FIELD_ILD_FILE_STRING,		
		PARAM_DEBUG_LOG,
		
		// HA directionality
		PARAM_HA_DIRECTIONALITY_EXTEND_LEFT,
		PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT,
		PARAM_HA_DIRECTIONALITY_ON_LEFT,
		PARAM_HA_DIRECTIONALITY_ON_RIGHT,

		// Limiter
		PARAM_LIMITER_SET_ON,
		PARAM_LIMITER_GET_COMPRESSION,

		// INITIALIZATION CHECK
		PARAM_IS_CORE_READY,
		
		// HRTF resampling step
		PARAM_HRTF_STEP,

		// High Performance and None modes
		PARAM_HIGH_PERFORMANCE_ILD_FILE_STRING,
		PARAM_SPATIALIZATION_MODE,
		PARAM_BUFFER_SIZE,
		PARAM_SAMPLE_RATE,
		PARAM_BUFFER_SIZE_CORE,
		PARAM_SAMPLE_RATE_CORE,


		P_NUM
	};

	/////////////////////////////////////////////////////////////////////

    struct EffectData
    {
		int sourceID;	// DEBUG
		std::shared_ptr<Binaural::CSingleSourceDSP> audioSource;
		std::shared_ptr<Binaural::CListener> listener;				
		Binaural::CCore core;
		bool coreReady;
		bool loadedHRTF;				// New
		bool loadedNearFieldILD;		// New
		bool loadedHighPerformanceILD;	// New
		int spatializationMode;			// New
		float parameters[P_NUM];
//		int bufferSize;
	//	int sampleRate;

		// STRING SERIALIZER		
		char* strHRTFpath;
		bool strHRTFserializing;
		int strHRTFcount;
		int strHRTFlength;
		char* strNearFieldILDpath;
		bool strNearFieldILDserializing;
		int strNearFieldILDcount;
		int strNearFieldILDlength;
		char* strHighPerformanceILDpath;
		bool strHighPerformanceILDserializing;
		int strHighPerformanceILDcount;
		int strHighPerformanceILDlength;

		// Limiter
		Common::CDynamicCompressorStereo limiter;

		// DEBUG LOG
		bool debugLog = false;

		// MUTEX
		std::mutex spatializerMutex;
	};

	/////////////////////////////////////////////////////////////////////

	template <class T>
	void WriteLog(UnityAudioEffectState* state, string logtext, const T& value)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		if (data->debugLog)
		{
			#ifdef DEBUG_LOG_FILE_BINSP
			ofstream logfile;
			int sourceid = data->sourceID;
			logfile.open("3DTI_BinauralSpatializer_DebugLog.txt", ofstream::out | ofstream::app);
			if (sourceid != -1)
				logfile << sourceid << ": " << logtext << value << endl;
			else
				logfile << logtext << value << endl;
			logfile.close();
			#endif

			#ifdef DEBUG_LOG_CAT						
			std::ostringstream os;
			os << logtext << value;
			string fulltext = os.str();
			__android_log_print(ANDROID_LOG_DEBUG, "3DTISPATIALIZER", fulltext.c_str());
			#endif
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

    int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
    {
        int numparams = P_NUM;
        definition.paramdefs = new UnityAudioParameterDefinition[numparams];
		RegisterParameter(definition, "HRTFPath", "", 0.0f, 255.0f, 0.0f, 1.0f, 1.0f, PARAM_HRTF_FILE_STRING, "String with path of HRTF binary file");
		RegisterParameter(definition, "HeadRadius", "m", 0.0f, FLT_MAX, 0.0875f, 1.0f, 1.0f, PARAM_HEAD_RADIUS, "Listener head radius");
		RegisterParameter(definition, "ScaleFactor", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 1.0f, PARAM_SCALE_FACTOR, "Scale factor for over/under sized scenes");
		RegisterParameter(definition, "SourceID", "", -1.0f, FLT_MAX, -1.0f, 1.0f, 1.0f, PARAM_SOURCE_ID, "Source ID for debug");
		RegisterParameter(definition, "CustomITD", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_CUSTOM_ITD, "Enabled custom ITD");
		RegisterParameter(definition, "HRTFInterp", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_HRTF_INTERPOLATION, "HRTF Interpolation method");
		RegisterParameter(definition, "MODfarLPF", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_FARLPF, "Far distance LPF module enabler");
		RegisterParameter(definition, "MODDistAtt", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_DISTATT, "Distance attenuation module enabler");
		RegisterParameter(definition, "MODNFILD", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_NEAR_FIELD_ILD, "Near distance ILD module enabler");
		RegisterParameter(definition, "MODHRTF", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_HRTF, "HRTF module enabler");
		RegisterParameter(definition, "MAGAneAtt", "dB", -30.0f, 0.0f, -3.0f, 1.0f, 1.0f, PARAM_MAG_ANECHATT, "Anechoic distance attenuation");
		RegisterParameter(definition, "MAGSounSpd", "m/s", 10.0f, 1000.0f, 343.0f, 1.0f, 1.0f, PARAM_MAG_SOUNDSPEED, "Sound speed");
		RegisterParameter(definition, "NFILDPath", "", 0.0f, 255.0f, 0.0f, 1.0f, 1.0f, PARAM_NEAR_FIELD_ILD_FILE_STRING, "String with path of ILD binary file for Near Field effect");		
		RegisterParameter(definition, "DebugLog", "", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, PARAM_DEBUG_LOG, "Generate debug log");
		
		// HA directionality
		RegisterParameter(definition, "HADirExtL", "dB", 0.0f, 30.0f, 15.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_EXTEND_LEFT, "HA directionality attenuation (in dB) for Left ear");
		RegisterParameter(definition, "HADirExtR", "dB", 0.0f, 30.0f, 15.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT, "HA directionality attenuation (in dB) for Right ear");
		RegisterParameter(definition, "HADirOnL", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_ON_LEFT, "HA directionality switch for Left ear");
		RegisterParameter(definition, "HADirOnR", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_ON_RIGHT, "HA directionality switch for Right ear");		

		// Limiter
		RegisterParameter(definition, "LimitOn", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_LIMITER_SET_ON, "Limiter enabler for binaural spatializer");
		RegisterParameter(definition, "LimitGet", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_LIMITER_GET_COMPRESSION, "Is binaural spatializer limiter compressing?");

		// Initialization check
		RegisterParameter(definition, "IsReady", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_IS_CORE_READY, "Is binaural spatializer ready?");

		// HRTF resampling step
		RegisterParameter(definition, "HRTFstep", "deg", 1.0f, 90.0f, 15.0f, 1.0f, 1.0f, PARAM_HRTF_STEP, "HRTF resampling step (in degrees)");

		// High performance mode
		RegisterParameter(definition, "HPILDPath", "", 0.0f, 255.0f, 0.0f, 1.0f, 1.0f, PARAM_HIGH_PERFORMANCE_ILD_FILE_STRING, "String with path of ILD binary file for High Performance mode");
		RegisterParameter(definition, "SpatMode", "", 0.0f, 2.0f, 0.0f, 1.0f, 1.0f, PARAM_SPATIALIZATION_MODE, "Spatialization mode (0=High quality, 1=High performance, 2=None)");
		//Sample Rate and BufferSize
		RegisterParameter(definition, "BufferSize", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 1.0f, PARAM_BUFFER_SIZE, "Buffer size used by Unity");
		RegisterParameter(definition, "SampleRate", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 1.0f, PARAM_SAMPLE_RATE, "Buffer size used by Unity");
		RegisterParameter(definition, "BufferSizeCore", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 1.0f, PARAM_BUFFER_SIZE_CORE, "Buffer size used by Core");
		RegisterParameter(definition, "SampleRateCore", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 1.0f, PARAM_SAMPLE_RATE_CORE, "Buffer size used by Core");
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
		if (tr>0.0f)			// General case
		{
			w = sqrt(1.0f + tr) * 2.0f;
			qw = 0.25f*w;
			qx = (L[6] - L[9]) / w;
			qy = (L[8] - L[2]) / w;
			qz = (L[1] - L[4]) / w;
		}
		// Cases with w = 0
		else if ((L[0] > L[5]) && (L[0] > L[10]))
		{
			w = sqrt(1.0f + L[0] - L[5] - L[10]) * 2.0f;
			qw = (L[6] - L[9]) / w;
			qx = 0.25f*w;
			qy = -(L[1] + L[4]) / w;
			qz = -(L[8] + L[2]) / w;
		}
		else if (L[5] > L[10])
		{
			w = sqrt(1.0f + L[5] - L[0] - L[10]) * 2.0f;
			qw = (L[8] - L[2]) / w;
			qx = -(L[1] + L[4]) / w;
			qy = 0.25f*w;
			qz = -(L[6] + L[9]) / w;
		}
		else
		{
			w = sqrt(1.0f + L[10] - L[0] - L[5]) * 2.0f;
			qw = (L[1] - L[4]) / w;
			qx = -(L[8] + L[2]) / w;
			qy = -(L[6] + L[9]) / w;
			qz = 0.25f*w;
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

	int LoadHRTFBinaryFile(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		// Load HRTF		
		if (!HRTF::CreateFrom3dti(data->strHRTFpath, data->listener))
		{
			//TDebuggerResultStruct result = GET_LAST_RESULT_STRUCT();
			//WriteLog(state, "ERROR TRYING TO LOAD HRTF!!! ", result.suggestion);
			return TLoadResult::RESULT_LOAD_ERROR;
		}

		if (data->listener->GetHRTF()->GetHRIRLength() != 0)
		{
			//data->listener->LoadHRTF(std::move(myHead));
			WriteLog(state, "LOAD HRTF: HRTF loaded from binary 3DTI file: ", data->strHRTFpath);
			WriteLog(state, "           HRIR length is ", data->listener->GetHRTF()->GetHRIRLength());
			WriteLog(state, "           Sample rate is ", state->samplerate);
			WriteLog(state, "           Buffer size is ", state->dspbuffersize);

			// Free memory
			free(data->strHRTFpath);

			return TLoadResult::RESULT_LOAD_OK;
		}
		else
		{			
			WriteLog(state, "LOAD HRTF: ERROR!!! Could not create HRTF from path: ", data->strHRTFpath);
			free(data->strHRTFpath);
			return TLoadResult::RESULT_LOAD_ERROR;
		}
	}

	/////////////////////////////////////////////////////////////////////

	int LoadHighPerformanceILDBinaryFile(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		/*int sampleRateInFile = ILD::GetSampleRateFrom3dti(data->strNearFieldILDpath);
		if (sampleRateInFile == (int)state->samplerate) {*/

			// Get ILD 
			//T_ILD_HashTable h;
			//h = ILD::CreateFrom3dti(data->strHighPerformanceILDpath);	
			bool boolResult = ILD::CreateFrom3dti_ILDSpatializationTable(data->strHighPerformanceILDpath, data->listener);

			// Check errors
			//TDebuggerResultStruct result = GET_LAST_RESULT_STRUCT();
			//if (result.id != RESULT_OK)
			//{
			//	WriteLog(state, "ERROR TRYING TO LOAD HIGH PERFORMANCE ILD!!! ", result.suggestion);
			//	return TLoadResult::RESULT_LOAD_ERROR;
			//}

			//if (h.size() > 0)		// TO DO: Improve this error check		
			if (boolResult)
			{
				///Binaural::CILD::SetILD_HashTable(std::move(h));
				WriteLog(state, "LOAD HIGH PERFORMANCE ILD: ILD loaded from binary 3DTI file: ", data->strHighPerformanceILDpath);
				//WriteLog(state, "          Hash hable size is ", h.size());
				free(data->strHighPerformanceILDpath);
				return TLoadResult::RESULT_LOAD_OK;
			}
			else
			{
				WriteLog(state, "LOAD HIGH PERFORMANCE ILD: ERROR!!! could not create ILD from path: ", data->strHighPerformanceILDpath);
				free(data->strHighPerformanceILDpath);
				return TLoadResult::RESULT_LOAD_ERROR;
			}
		/*}
		else
		{
			WriteLog(state, "LOAD NEAR FIELD ILD: ERROR!!! output sample rate is not the same as the ILD from path: ", data->strNearFieldILDpath);
			free(data->strNearFieldILDpath);
			return TLoadResult::RESULT_LOAD_ERROR;
		}*/
	}

	/////////////////////////////////////////////////////////////////////

	int LoadNearFieldILDBinaryFile(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		// Get ILD 
		
		/*int sampleRateInFile = ILD::GetSampleRateFrom3dti(data->strNearFieldILDpath);
		if (sampleRateInFile == (int)state->samplerate) 
		{*/
			bool boolResult = ILD::CreateFrom3dti_ILDNearFieldEffectTable(data->strNearFieldILDpath, data->listener);
			// Check errors
			//TResultStruct result = GET_LAST_RESULT_STRUCT();
			//if (result.id != RESULT_OK)
			//{
			//	WriteLog(state, "ERROR TRYING TO LOAD NEAR FIELD ILD!!! ", result.suggestion);
			//	return TLoadResult::RESULT_LOAD_ERROR;
			//}

			//if (h.size() > 0)		// TO DO: Improve this error check		
			if (boolResult)
			{
				//Binaural::CILD::SetILD_HashTable(std::move(h));
				WriteLog(state, "LOAD NEAR FIELD ILD: ILD loaded from binary 3DTI file: ", data->strNearFieldILDpath);
				//WriteLog(state, "          Hash hable size is ", h.size());
				free(data->strNearFieldILDpath);
				return TLoadResult::RESULT_LOAD_OK;
			}
			else
			{
				WriteLog(state, "LOAD NEAR FIELD ILD: ERROR!!! could not create ILD from path: ", data->strNearFieldILDpath);
				free(data->strNearFieldILDpath);
				return TLoadResult::RESULT_LOAD_ERROR;
			}

		/*}
		else 
		{
			WriteLog(state, "LOAD NEAR FIELD ILD: ERROR!!! output sample rate is not the same as the ILD from path: ", data->strNearFieldILDpath);
			free(data->strNearFieldILDpath);
			return TLoadResult::RESULT_LOAD_ERROR;
		}
		*/

		
	}

	/////////////////////////////////////////////////////////////////////

	int BuildPathString(UnityAudioEffectState* state, char*& path, bool &serializing, int &length, int &count, float value)
	{
		// Check if serialization was not started
		if (!serializing)
		{
			// Receive string length
			
			length = static_cast<int>(value);
			path = (char*)malloc((length+1) * sizeof(char));
			count = 0;
			serializing = true;
            return RESULT_LOAD_WAITING;  // TODO: @cgarre please check!!
		}
		else
		{
			// Receive next character

			// Concatenate char to string				
			int valueInt = static_cast<int>(value);
			char valueChr = static_cast<char>(valueInt);	
			path[count] = valueChr;
			++count; 

			// Check if string has ended			
			if (count == length)
			{		
				path[count] = 0;	// End character
				serializing = false;
				return RESULT_LOAD_END;
			}
			else
				return RESULT_LOAD_CONTINUE;
		}
	}

	/////////////////////////////////////////////////////////////////////

	void WriteLogHeader(UnityAudioEffectState* state)
	{		
		EffectData* data = state->GetEffectData<EffectData>();
		
		// TO DO: Change this for high performance / high quality modes

		// Audio state:		
		Common::AudioState_Struct audioState = data->core.GetAudioState();
		WriteLog(state, "CREATE: Sample rate set to ", audioState.sampleRate);
		WriteLog(state, "CREATE: Buffer size set to ", audioState.bufferSize);
		WriteLog(state, "CREATE: HRTF resampling step set to ", data->core.GetHRTFResamplingStep());

		// Listener:		
		if (data->listener != nullptr)
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
		EffectData* effectdata = new EffectData;
		//memset(effectdata, 0, sizeof(EffectData)); // Prefer not to write 0s on smart_ptr & Core objects.
		state->effectdata = effectdata;
		if (IsHostCompatible(state))
			state->spatializerdata->distanceattenuationcallback = DistanceAttenuationCallback;
		InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->parameters);

		// DEBUG LOG
		//effectdata->debugLog = false;

		WriteLog(state, "Creating audio plugin...", "");

		// Set default audio state			
		Common::AudioState_Struct audioState;
		audioState.sampleRate = (int)state->samplerate;
		audioState.bufferSize = (int)state->dspbuffersize;		
		effectdata->core.SetAudioState(audioState);
		
		//Save samplerate and buffer size into the struct
		//effectdata->bufferSize = audioState.bufferSize;
		//effectdata->sampleRate = audioState.sampleRate;		

		// Create listener
		effectdata->listener = effectdata->core.CreateListener();

		// Set default HRTF resampling step
		effectdata->core.SetHRTFResamplingStep(effectdata->parameters[PARAM_HRTF_STEP]);

		// Init parameters. Core is not ready until we load the HRTF. ILD will be disabled, so we don't need to worry yet
		effectdata->coreReady = false;
		effectdata->parameters[PARAM_SCALE_FACTOR] = 1.0f;
		effectdata->sourceID = -1;

		// Create source and set default interpolation method		
		effectdata->audioSource = effectdata->core.CreateSingleSourceDSP();
		if (effectdata->audioSource != nullptr)
		{
			effectdata->audioSource->EnableInterpolation();
			effectdata->audioSource->DisableNearFieldEffect();	// ILD disabled before loading ILD data				
		}

		// STRING SERIALIZER
		effectdata->strHRTFserializing = false;
		effectdata->strHRTFcount = 0;
		effectdata->strNearFieldILDserializing = false;
		effectdata->strNearFieldILDcount = 0;
		effectdata->strHighPerformanceILDserializing = false;
		effectdata->strHighPerformanceILDcount = 0;

		// Setup limiter
		effectdata->limiter.Setup(state->samplerate, LIMITER_RATIO, LIMITER_THRESHOLD, LIMITER_ATTACK, LIMITER_RELEASE);

		// Spatialization modes
		effectdata->spatializationMode = SPATIALIZATION_MODE_NONE;	
		effectdata->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::None);
		effectdata->loadedHRTF = false;
		effectdata->loadedNearFieldILD = false;
		effectdata->loadedHighPerformanceILD = false;

		// 3DTI Debugger
#if defined (SWITCH_ON_3DTI_ERRORHANDLER) || defined (_3DTI_ANDROID_ERRORHANDLER)
		Common::CErrorHandler::Instance().SetAssertMode(ASSERT_MODE_CONTINUE);
#endif

		WriteLog(state, "Core initialized. Waiting for configuration...", "");

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

	bool IsCoreReady(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		bool isReady = false;

		if (data->spatializationMode == SPATIALIZATION_MODE_NONE)
			isReady = true;

		if (data->spatializationMode == SPATIALIZATION_MODE_HIGH_PERFORMANCE)
		{
			if (data->loadedHighPerformanceILD)
				isReady = true;
		}

		if (data->spatializationMode == SPATIALIZATION_MODE_HIGH_QUALITY)
		{
			if (data->loadedHRTF)
				isReady = true;
			if ((!data->loadedNearFieldILD) && (data->audioSource->IsNearFieldEffectEnabled()))
				isReady = false;
		}

		return isReady;
	}

	/////////////////////////////////////////////////////////////////////

	void UpdateCoreIsReady(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		bool isReady = IsCoreReady(state);

		if (!data->coreReady && isReady)
		{
			data->coreReady = true;
			WriteLog(state, "Core ready!!!!!", "");
		}

		if (data->coreReady && !isReady)
		{
			data->coreReady = false;
			WriteLog(state, "Core stoped!!!!! Waiting for loading resources...", "");
		}
	}


	/////////////////////////////////////////////////////////////////////


    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
    {
        EffectData* data = state->GetEffectData<EffectData>();
        if (index >= P_NUM)
            return UNITY_AUDIODSP_ERR_UNSUPPORTED;
        data->parameters[index] = value;

		Common::CMagnitudes magnitudes;		
		int loadResult;		

		data->spatializerMutex.lock();

		// Process command sent by C# API
		switch (index)
		{
			case PARAM_HRTF_FILE_STRING:	// Load HRTF binary file (MANDATORY)								
				loadResult = BuildPathString(state, data->strHRTFpath, data->strHRTFserializing, data->strHRTFlength, data->strHRTFcount, value);				
				if (loadResult == TLoadResult::RESULT_LOAD_END)
				{
					loadResult = LoadHRTFBinaryFile(state);
					if (loadResult == TLoadResult::RESULT_LOAD_OK)
					{
						data->loadedHRTF = true;
						UpdateCoreIsReady(state);
					}
				}
				break;

			case PARAM_NEAR_FIELD_ILD_FILE_STRING:	// Load ILD binary file (MANDATORY?)								
				loadResult = BuildPathString(state, data->strNearFieldILDpath, data->strNearFieldILDserializing, data->strNearFieldILDlength, data->strNearFieldILDcount, value);
				if (loadResult == TLoadResult::RESULT_LOAD_END)
				{
					loadResult = LoadNearFieldILDBinaryFile(state);
					if (loadResult == TLoadResult::RESULT_LOAD_OK)
					{
						data->audioSource->EnableNearFieldEffect();
						data->loadedNearFieldILD = true;
						WriteLog(state, "SET PARAMETER: Near Field ILD Enabled", "");
						UpdateCoreIsReady(state);
					}
				}
				break;

			case PARAM_HEAD_RADIUS:	// Set listener head radius (OPTIONAL)
				data->listener->SetHeadRadius(value);				
				WriteLog(state, "SET PARAMETER: Listener head radius changed to ", value);
				break;

			// FUNCTIONALITY TO BE IMPLEMENTED
			case PARAM_SCALE_FACTOR:	// Set scale factor (OPTIONAL)				
				WriteLog(state, "SET PARAMETER: Scale factor changed to ", value);
				break;

			case PARAM_SOURCE_ID:	// DEBUG
				data->sourceID = (int)value;
				WriteLog(state, "SET PARAMETER: Source ID set to ", data->sourceID);
				break;

			case PARAM_CUSTOM_ITD:	// Enable custom ITD (OPTIONAL)
				if (value > 0.0f)
				{
					data->listener->EnableCustomizedITD();
					WriteLog(state, "SET PARAMETER: Custom ITD is ", "Enabled");
				}
				else
				{
					data->listener->DisableCustomizedITD();
					WriteLog(state, "SET PARAMETER: Custom ITD is ", "Disabled");
				}
				break;

			case PARAM_HRTF_INTERPOLATION:	// Change interpolation method (OPTIONAL)
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

			case PARAM_MOD_FARLPF:
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

			case PARAM_MOD_DISTATT:
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

			case PARAM_MOD_NEAR_FIELD_ILD:
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

			case PARAM_MOD_HRTF:
				// DEPRECATED. DO NOTHING
				WriteLog(state, "SET PARAMETER: HRTF convolution on/off parameter is deprecated. There might be a mismatch between your 3DTI plugin and your 3DTI API.", "");
				//if (value > 0.0f)
				//{
				//	data->audioSource->EnableHRTF();
				//	WriteLog(state, "SET PARAMETER: HRTF convolution is ", "Enabled");
				//}
				//else
				//{
				//	data->audioSource->DisableHRTF();
				//	WriteLog(state, "SET PARAMETER: HRTF convolution is ", "Disabled");
				//}
				break;

			case PARAM_MAG_ANECHATT:
				magnitudes = data->core.GetMagnitudes();
				magnitudes.SetAnechoicDistanceAttenuation(value);
				data->core.SetMagnitudes(magnitudes);
				WriteLog(state, "SET PARAMETER: Anechoic distance attenuation set to (dB) ", value);
				break;

			case PARAM_MAG_SOUNDSPEED:
				magnitudes = data->core.GetMagnitudes();
				magnitudes.SetSoundSpeed(value);
				data->core.SetMagnitudes(magnitudes);
				WriteLog(state, "SET PARAMETER: Sound speed set to (m/s) ", value);
				break;

			case PARAM_DEBUG_LOG:				
				if (value != 0.0f)
				{
					data->debugLog = true;
					WriteLogHeader(state);
#if defined (SWITCH_ON_3DTI_ERRORHANDLER) || defined (_3DTI_ANDROID_ERRORHANDLER)
                    Common::CErrorHandler::Instance().SetErrorLogFile("3DTi_ErrorLog.txt");
					Common::CErrorHandler::Instance().SetVerbosityMode(VERBOSITY_MODE_ONLYERRORS);
					Common::CErrorHandler::Instance().SetAssertMode(ASSERT_MODE_CONTINUE);
#endif
				}
				else
					data->debugLog = false;
				break;

			case PARAM_HA_DIRECTIONALITY_EXTEND_LEFT:
				data->listener->SetDirectionality_dB(Common::T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: HA Directionality for Left ear set to (dB) ", value);
				break;

			case PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT:
				data->listener->SetDirectionality_dB(Common::T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: HA Directionality for Right ear set to (dB) ", value);
				break;

			case PARAM_HA_DIRECTIONALITY_ON_LEFT:
				if (value > 0.0f)
				{
					data->listener->EnableDirectionality(Common::T_ear::LEFT);
					WriteLog(state, "SET PARAMETER: HA Directionality switched ON for Left ear", "");
				}
				else
				{
					data->listener->DisableDirectionality(Common::T_ear::LEFT);
					WriteLog(state, "SET PARAMETER: HA Directionality switched OFF for Left ear", "");
				}				
				break;

			case PARAM_HA_DIRECTIONALITY_ON_RIGHT:
				if (value > 0.0f)
				{
					data->listener->EnableDirectionality(Common::T_ear::RIGHT);
					WriteLog(state, "SET PARAMETER: HA Directionality switched ON for Right ear", "");
				}
				else
				{
					data->listener->DisableDirectionality(Common::T_ear::RIGHT);
					WriteLog(state, "SET PARAMETER: HA Directionality switched OFF for Right ear", "");
				}
				break;

			case PARAM_LIMITER_SET_ON:
				if (value > 0.0f)
				{					
					WriteLog(state, "SET PARAMETER: Limiter switched ON", "");
				}
				else
				{					
					WriteLog(state, "SET PARAMETER: Limiter switched OFF", "");
				}
				break;				

			case PARAM_LIMITER_GET_COMPRESSION:
				WriteLog(state, "SET PARAMETER: WARNING! PARAM_LIMIT_GET_COMPRESSION is read only", "");
				break;

			case PARAM_HRTF_STEP:				
				data->core.SetHRTFResamplingStep((int)value);
				WriteLog(state, "SET PARAMETER: HRTF resampling step set to (degrees) ", value);
				break;

			case PARAM_HIGH_PERFORMANCE_ILD_FILE_STRING:	// Load ILD binary file (MANDATORY?)								
				loadResult = BuildPathString(state, data->strHighPerformanceILDpath, data->strHighPerformanceILDserializing, data->strHighPerformanceILDlength, data->strHighPerformanceILDcount, value);
				if (loadResult == TLoadResult::RESULT_LOAD_END)
				{
					loadResult = LoadHighPerformanceILDBinaryFile(state);
					if (loadResult == TLoadResult::RESULT_LOAD_OK)
					{
						data->loadedHighPerformanceILD = true;
						UpdateCoreIsReady(state);
						//data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::HighPerformance);
						//WriteLog(state, "SET PARAMETER: High Performance ILD Enabled", "");
					}
				}
				break;

			case PARAM_SPATIALIZATION_MODE:
				if (value == 0.0f)
				{
					data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::HighQuality);
					data->spatializationMode = SPATIALIZATION_MODE_HIGH_QUALITY;
					WriteLog(state, "SET PARAMETER: High Quality spatialization mode is enabled", "");
					UpdateCoreIsReady(state);
				}
				if (value == 1.0f)
				{
					data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::HighPerformance);
					data->spatializationMode = SPATIALIZATION_MODE_HIGH_PERFORMANCE;
					WriteLog(state, "SET PARAMETER: High performance spatialization mode is enabled", "");
					UpdateCoreIsReady(state);
				}
				if (value == 2.0f)
				{
					data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::None);					
					data->spatializationMode = SPATIALIZATION_MODE_NONE;
					WriteLog(state, "SET PARAMETER: No spatialization mode is enabled", "");
					UpdateCoreIsReady(state);
				}
				break;

			default:
				WriteLog(state, "SET PARAMETER: ERROR!!!! Unknown float parameter received from API: ", index);
				return UNITY_AUDIODSP_ERR_UNSUPPORTED;
				break;
		}

		data->spatializerMutex.unlock();

        return UNITY_AUDIODSP_OK;
    }

	/////////////////////////////////////////////////////////////////////

    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char *valuestr)
    {
        EffectData* data = state->GetEffectData<EffectData>();
        if (index >= P_NUM)
            return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		if (valuestr != NULL)
			valuestr[0] = 0;

		if (value != NULL)
		{ 
			switch (index)
			{
				case PARAM_LIMITER_GET_COMPRESSION:
					if (data->limiter.IsDynamicProcessApplied())
						*value = 1.0f;
					else
						*value = 0.0f;
					break;

				case PARAM_IS_CORE_READY:
					if (data->coreReady)
						*value = 1.0f;
					else
						*value = 0.0f;
					break;
				
				case PARAM_BUFFER_SIZE:					
					//*value = float(data->bufferSize);					
					*value = (int)state->dspbuffersize;
					break;

				case PARAM_SAMPLE_RATE:
					//*value = float(data->sampleRate);
					*value = (int)state->samplerate;
					break;
				case PARAM_BUFFER_SIZE_CORE:
					//*value = float(data->bufferSize);					
					*value = state->GetEffectData<EffectData>()->core.GetAudioState().bufferSize;
					break;

				case PARAM_SAMPLE_RATE_CORE:
					//*value = float(data->sampleRate);
					*value = state->GetEffectData<EffectData>()->core.GetAudioState().sampleRate;
					break;
				default:
					*value = data->parameters[index];
					break;
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
            memcpy(outbuffer, inbuffer, length * outchannels * sizeof(float));
            return UNITY_AUDIODSP_OK;
        }

		EffectData* data = state->GetEffectData<EffectData>();

		data->spatializerMutex.lock();

		// Before doing anything, check that the core is ready
		//if (!data->coreReady)
		if (!IsCoreReady(state))
		{
			// Put silence in outbuffer
			//WriteLog(state, "PROCESS: Core is not ready yet...", "");
			memset(outbuffer, 0.0f, length * outchannels * sizeof(float));
			data->spatializerMutex.unlock();
			return UNITY_AUDIODSP_OK;
		}

		// Set source and listener transforms		
		data->audioSource->SetSourceTransform(ComputeSourceTransformFromMatrix(state->spatializerdata->sourcematrix, data->parameters[PARAM_SCALE_FACTOR]));
		data->listener->SetListenerTransform(ComputeListenerTransformFromMatrix(state->spatializerdata->listenermatrix, data->parameters[PARAM_SCALE_FACTOR]));

		// Now check that listener and source are not in the same position. 
		// This might happens in some weird cases, such as when trying to process a source with no clip
		if (data->listener->GetListenerTransform().GetVectorTo(data->audioSource->GetSourceTransform()).GetSqrDistance() < 0.0001f)
		{
			WriteLog(state, "WARNING during Process! AudioSource and Listener positions are the same (do you have a source with no clip?)", "");
			data->spatializerMutex.unlock();
			return UNITY_AUDIODSP_OK;
		}

		// Transform input buffer
		CMonoBuffer<float> inMonoBuffer(length);
		//for (int i = 0; i < length; i++)
		//{
		//	inMonoBuffer[i] = inbuffer[i * 2]; // We take only the left channel
		//}
		//for (int i = 0; i < length; i++)
		//{
		//	inMonoBuffer[i] = inbuffer[(i*2)+1]; // We take only the right channel
		//}
		int j = 0;
		for (int i = 0; i < length; i ++)
		{
			inMonoBuffer[i] = (inbuffer[j] + inbuffer[j + 1]) / 2.0f;	// We take average of left and right channels
			j+=2;
		}

		// Process!!
		CMonoBuffer<float> outLeftBuffer(length);
		CMonoBuffer<float> outRightBuffer(length);
		data->audioSource->SetBuffer(inMonoBuffer);
		data->audioSource->ProcessAnechoic(outLeftBuffer, outRightBuffer);

		// Limiter
		CStereoBuffer<float> outStereoBuffer;
		outStereoBuffer.Interlace(outLeftBuffer, outRightBuffer);
		if (data->parameters[PARAM_LIMITER_SET_ON])
		{
			data->limiter.Process(outStereoBuffer);
		}

		// Transform output buffer			
		int i = 0;
		for (auto it = outStereoBuffer.begin(); it != outStereoBuffer.end(); it++)
		{
			outbuffer[i++] = *it;
		}

		data->spatializerMutex.unlock();

        return UNITY_AUDIODSP_OK;
    }
}
