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
//#include <Common/Debugger.h>


// Includes for debug logging
#include <fstream>
#include <iostream>

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
		PARAM_MOD_ILD,
		PARAM_MOD_HRTF,
		PARAM_MAG_ANECHATT,
		PARAM_MAG_SOUNDSPEED,
		PARAM_ILD_FILE_STRING,		
		PARAM_DEBUG_LOG,
		
		// HA directionality
		PARAM_HA_DIRECTIONALITY_EXTEND_LEFT,
		PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT,
		PARAM_HA_DIRECTIONALITY_ON_LEFT,
		PARAM_HA_DIRECTIONALITY_ON_RIGHT,

		// Limiter
		PARAM_LIMITER_SET_ON,
		PARAM_LIMITER_GET_COMPRESSION,

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
		float parameters[P_NUM];

		// STRING SERIALIZER		
		char* strHRTFpath;
		bool strHRTFserializing;
		int strHRTFcount;
		int strHRTFlength;
		char* strILDpath;
		bool strILDserializing;
		int strILDcount;
		int strILDlength;

		// Limiter
		CDynamicCompressorStereo limiter;

		// DEBUG LOG
		bool debugLog = false;
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
		RegisterParameter(definition, "MODILD", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_ILD, "Near distance ILD module enabler");
		RegisterParameter(definition, "MODHRTF", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_HRTF, "HRTF module enabler");
		RegisterParameter(definition, "MAGAneAtt", "dB", -30.0f, 0.0f, -6.0f, 1.0f, 1.0f, PARAM_MAG_ANECHATT, "Anechoic distance attenuation");
		RegisterParameter(definition, "MAGSounSpd", "m/s", 0.0f, 1000.0f, 343.0f, 1.0f, 1.0f, PARAM_MAG_SOUNDSPEED, "Sound speed");
		RegisterParameter(definition, "ILDPath", "", 0.0f, 255.0f, 0.0f, 1.0f, 1.0f, PARAM_ILD_FILE_STRING, "String with path of ILD binary file");		
		RegisterParameter(definition, "DebugLog", "", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, PARAM_DEBUG_LOG, "Generate debug log");
		
		// HA directionality
		RegisterParameter(definition, "HADirExtL", "dB", 0.0f, 30.0f, 15.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_EXTEND_LEFT, "HA directionality attenuation (in dB) for Left ear");
		RegisterParameter(definition, "HADirExtR", "dB", 0.0f, 30.0f, 15.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT, "HA directionality attenuation (in dB) for Right ear");
		RegisterParameter(definition, "HADirOnL", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_ON_LEFT, "HA directionality switch for Left ear");
		RegisterParameter(definition, "HADirOnR", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_HA_DIRECTIONALITY_ON_RIGHT, "HA directionality switch for Right ear");		

		// Limiter
		RegisterParameter(definition, "LimitOn", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_LIMITER_SET_ON, "Limiter enabler for binaural spatializer");
		RegisterParameter(definition, "LimitGet", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_LIMITER_GET_COMPRESSION, "Is binaural spatializer limiter compressing?");
			
        definition.flags |= UnityAudioEffectDefinitionFlags_IsSpatializer;
        return numparams;
    }

	/////////////////////////////////////////////////////////////////////

	CTransform ComputeListenerTransformFromMatrix(float* listenerMatrix, float scale)
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
		CTransform listenerTransform;
		listenerTransform.SetPosition(CVector3(listenerpos_x, listenerpos_y, listenerpos_z));		

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

		CQuaternion unityQuaternion = CQuaternion(qw, qx, qy, qz);
		listenerTransform.SetOrientation(unityQuaternion.Inverse());
		return listenerTransform;
	}

	/////////////////////////////////////////////////////////////////////

	CTransform ComputeSourceTransformFromMatrix(float* sourceMatrix, float scale)
	{
		// Orientation does not matters for audio sources
		CTransform sourceTransform;
		sourceTransform.SetPosition(CVector3(sourceMatrix[12] * scale, sourceMatrix[13] * scale, sourceMatrix[14] * scale));		
		return sourceTransform;
	}

	/////////////////////////////////////////////////////////////////////

	int LoadHRTFBinaryFile(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		// Load HRTF		
		HRTF::CreateFrom3dti(data->strHRTFpath, data->listener);		
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

	int LoadILDBinaryFile(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		// Get ILD and check errors
		ILD_HashTable h;
		h = ILD::CreateFrom3dti(data->strILDpath);		
		if (h.size() > 0)		// TO DO: Improve this error check		
		{
			CILD::SetILD_HashTable(std::move(h));
			WriteLog(state, "LOAD ILD: ILD loaded from binary 3DTI file: ", data->strILDpath);
			WriteLog(state, "          Hash hable size is ", h.size());
			free(data->strILDpath);
			return TLoadResult::RESULT_LOAD_OK;
		}
		else
		{			
			WriteLog(state, "LOAD ILD: ERROR!!! could not create ILD from path: ", data->strILDpath);
			free(data->strILDpath);
			return TLoadResult::RESULT_LOAD_ERROR;
		}
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
		
		// Audio state:		
		Binaural::AudioStateBinaural_Struct audioState = data->core.GetAudioState();
		WriteLog(state, "CREATE: Sample rate set to ", audioState.sampleRate);
		WriteLog(state, "CREATE: Buffer size set to ", audioState.bufferSize);

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
		Binaural::AudioStateBinaural_Struct audioState;
		audioState.sampleRate = (int)state->samplerate;
		audioState.bufferSize = (int)state->dspbuffersize;
		audioState.HRTF_resamplingStep = 15;
		effectdata->core.SetAudioState(audioState);

		// Create listener
		effectdata->listener = effectdata->core.CreateListener();

		// Init parameters. Core is not ready until we load the HRTF. ILD will be disabled, so we don't need to worry yet
		effectdata->coreReady = false;
		effectdata->parameters[PARAM_SCALE_FACTOR] = 1.0f;
		effectdata->sourceID = -1;

		// Create source and set default interpolation method		
		effectdata->audioSource = effectdata->core.CreateSingleSourceDSP();
		if (effectdata->audioSource != nullptr)
		{
			effectdata->audioSource->SetInterpolation(true);
			effectdata->audioSource->modEnabler.doILD = false;	// ILD disabled before loading ILD data				
		}

		// STRING SERIALIZER
		effectdata->strHRTFserializing = false;
		effectdata->strHRTFcount = 0;
		effectdata->strILDserializing = false;
		effectdata->strILDcount = 0;

		// Setup limiter
		effectdata->limiter.Setup(state->samplerate, LIMITER_RATIO, LIMITER_THRESHOLD, LIMITER_ATTACK, LIMITER_RELEASE);

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


    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
    {
        EffectData* data = state->GetEffectData<EffectData>();
        if (index >= P_NUM)
            return UNITY_AUDIODSP_ERR_UNSUPPORTED;
        data->parameters[index] = value;

		CMagnitudes magnitudes;
		int loadResult;		

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
						if (!data->coreReady)
						{
							data->coreReady = true;
							WriteLog(state, "Core ready!!!!!", "");
						}
					}
				}
				break;

			case PARAM_ILD_FILE_STRING:	// Load ILD binary file (MANDATORY?)								
				loadResult = BuildPathString(state, data->strILDpath, data->strILDserializing, data->strILDlength, data->strILDcount, value);
				if (loadResult == TLoadResult::RESULT_LOAD_END)
				{
					loadResult = LoadILDBinaryFile(state);
					if (loadResult == TLoadResult::RESULT_LOAD_OK)
					{
						data->audioSource->modEnabler.doILD = true;
						WriteLog(state, "SET PARAMETER: ILD Enabled", "");
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
					data->listener->SetCustomizedITD(true);
					WriteLog(state, "SET PARAMETER: Custom ITD is ", "Enabled");
				}
				else
				{
					data->listener->SetCustomizedITD(false);
					WriteLog(state, "SET PARAMETER: Custom ITD is ", "Disabled");
				}
				break;

			case PARAM_HRTF_INTERPOLATION:	// Change interpolation method (OPTIONAL)
				if (value != 0.0f)
				{
					data->audioSource->SetInterpolation(true);
					WriteLog(state, "SET PARAMETER: Run-time HRTF Interpolation switched ", "ON");
				}
				else
				{
					data->audioSource->SetInterpolation(false);
					WriteLog(state, "SET PARAMETER: Run-time HRTF Interpolation switched ", "OFF");
				}				
				break;

			case PARAM_MOD_FARLPF:
				if (value > 0.0f)
				{
					data->audioSource->modEnabler.doFarDistance = true;
					WriteLog(state, "SET PARAMETER: Far distance LPF is ", "Enabled");
				}
				else
				{
					data->audioSource->modEnabler.doFarDistance = false;
					WriteLog(state, "SET PARAMETER: Far distance LPF is ", "Disabled");
				}
				break;

			case PARAM_MOD_DISTATT:
				if (value > 0.0f)
				{
					data->audioSource->modEnabler.doDistanceAttenuation = true;
					WriteLog(state, "SET PARAMETER: Distance attenuation is ", "Enabled");
				}
				else
				{
					data->audioSource->modEnabler.doDistanceAttenuation = false;
					WriteLog(state, "SET PARAMETER: Distance attenuation is ", "Disabled");
				}
				break;

			case PARAM_MOD_ILD:
				if (value > 0.0f)
				{
					data->audioSource->modEnabler.doILD = true;
					WriteLog(state, "SET PARAMETER: Near distance ILD is ", "Enabled");
				}
				else
				{
					data->audioSource->modEnabler.doILD = false;
					WriteLog(state, "SET PARAMETER: Near distance ILD is ", "Disabled");
				}
				break;

			case PARAM_MOD_HRTF:
				if (value > 0.0f)
				{
					data->audioSource->modEnabler.doHRTF = true;
					WriteLog(state, "SET PARAMETER: HRTF convolution is ", "Enabled");
				}
				else
				{
					data->audioSource->modEnabler.doHRTF = false;
					WriteLog(state, "SET PARAMETER: HRTF convolution is ", "Disabled");
				}
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
				}
				else
					data->debugLog = false;
				break;

			case PARAM_HA_DIRECTIONALITY_EXTEND_LEFT:
				data->listener->GetHA()->SetDirectionalityExtendL_dB(value);
				WriteLog(state, "SET PARAMETER: HA Directionality for Left ear set to (dB) ", value);
				break;

			case PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT:
				data->listener->GetHA()->SetDirectionalityExtendR_dB(value);
				WriteLog(state, "SET PARAMETER: HA Directionality for Right ear set to (dB) ", value);
				break;

			case PARAM_HA_DIRECTIONALITY_ON_LEFT:
				if (value > 0.0f)
				{
					data->listener->GetHA()->doDirectionalityL = true;
					WriteLog(state, "SET PARAMETER: HA Directionality switched ON for Left ear", "");
				}
				else
				{
					data->listener->GetHA()->doDirectionalityL = false;
					WriteLog(state, "SET PARAMETER: HA Directionality switched OFF for Left ear", "");
				}				
				break;

			case PARAM_HA_DIRECTIONALITY_ON_RIGHT:
				if (value > 0.0f)
				{
					data->listener->GetHA()->doDirectionalityR = true;
					WriteLog(state, "SET PARAMETER: HA Directionality switched ON for Right ear", "");
				}
				else
				{
					data->listener->GetHA()->doDirectionalityR = false;
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

			default:
				WriteLog(state, "SET PARAMETER: ERROR!!!! Unknown float parameter received from API: ", index);
				return UNITY_AUDIODSP_ERR_UNSUPPORTED;
				break;
		}

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
					if (data->limiter.GetCompression())
						*value = 1.0f;
					else
						*value = 0.0f;
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

		// Before doing anything, check that the core is ready
		if (!data->coreReady)
		{
			// Put silence in outbuffer
			//WriteLog(state, "PROCESS: Core is not ready yet...", "");
			memset(outbuffer, 0.0f, length * outchannels * sizeof(float));
			return UNITY_AUDIODSP_OK;
		}

		// Set source and listener transforms		
		data->audioSource->SetSourceTransform(ComputeSourceTransformFromMatrix(state->spatializerdata->sourcematrix, data->parameters[PARAM_SCALE_FACTOR]));
		data->listener->SetListenerTransform(ComputeListenerTransformFromMatrix(state->spatializerdata->listenermatrix, data->parameters[PARAM_SCALE_FACTOR]));

		// Transform input buffer
		CMonoBuffer<float> inMonoBuffer(length);
		for (int i = 0; i < length; i++)
		{
			inMonoBuffer[i] = inbuffer[i * 2]; // We take only the left channel
		}

		// Process!!
		CStereoBuffer<float> outStereoBuffer(length * 2);
		data->audioSource->UpdateBuffer(inMonoBuffer);
		data->audioSource->ProcessAnechoic(*data->listener, outStereoBuffer);

		// Limiter
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

        return UNITY_AUDIODSP_OK;
    }
}
