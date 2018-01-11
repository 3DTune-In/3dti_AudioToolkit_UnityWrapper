/**
*** 3D-Tune-In Toolkit Unity Wrapper: Loudspeakers Spatializer ***
*
* version 0.1
* Created on: March 2017
*
* Author: 3DI-DIANA Research Group / University of Malaga / Spain
* Contact: areyes@uma.es
*
* Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
* Module: 3DTI Toolkit Unity Wrapper
**/

#include "AudioPluginUtil.h"

//#include <LoudspeakersSpatializer/Core_LS.h>
#include <LoudspeakersSpatializer/3DTI_LoudspeakersSpatializer.h>
#include <Common/ErrorHandler.h>

// Includes for debug logging
#include <fstream>
#include <iostream>


enum TLoadResult { RESULT_LOAD_WAITING = 0, RESULT_LOAD_CONTINUE = 1, RESULT_LOAD_END = 2, RESULT_LOAD_OK = 3, RESULT_LOAD_ERROR = -1 };

// DEBUG LOG 
#ifdef UNITY_ANDROID
#define DEBUG_LOG_CAT
#else
#define DEBUG_LOG_FILE
#endif

#ifdef DEBUG_LOG_CAT
#include <android/log.h> 
#include <string>
#include <sstream>
#endif

#include <cfloat>

/////////////////////////////////////////////////////////////////////

namespace LoudspeakersSpatializer3DTI
{
#define MAX_NUMBER_OF_SPEAKERS 8

	enum
	{		
		PARAM_SCALE_FACTOR,
		PARAM_SOURCE_ID,	// DEBUG
		PARAM_MOD_FARLPF,
		PARAM_MOD_DISTATT,		
		PARAM_MAG_ANECHATT,
		PARAM_MAG_SOUNDSPEED,		
		PARAM_DEBUG_LOG,		
		//Speakers
		PARAM_SAVE_SPEAKERS_CONFIG,
		PARAM_SPEAKER_1_X,
		PARAM_SPEAKER_2_X,
		PARAM_SPEAKER_3_X,
		PARAM_SPEAKER_4_X,
		PARAM_SPEAKER_5_X,
		PARAM_SPEAKER_6_X,
		PARAM_SPEAKER_7_X,
		PARAM_SPEAKER_8_X,
		PARAM_SPEAKER_1_Y,
		PARAM_SPEAKER_2_Y,
		PARAM_SPEAKER_3_Y,
		PARAM_SPEAKER_4_Y,
		PARAM_SPEAKER_5_Y,
		PARAM_SPEAKER_6_Y,
		PARAM_SPEAKER_7_Y,
		PARAM_SPEAKER_8_Y,
		PARAM_SPEAKER_1_Z,
		PARAM_SPEAKER_2_Z,
		PARAM_SPEAKER_3_Z,
		PARAM_SPEAKER_4_Z,
		PARAM_SPEAKER_5_Z,
		PARAM_SPEAKER_6_Z,
		PARAM_SPEAKER_7_Z,
		PARAM_SPEAKER_8_Z,
		PARAM_SPEAKER_1_W,
		PARAM_SPEAKER_2_W,
		PARAM_SPEAKER_3_W,
		PARAM_SPEAKER_4_W,
		PARAM_SPEAKER_5_W,
		PARAM_SPEAKER_6_W,
		PARAM_SPEAKER_7_W,
		PARAM_SPEAKER_8_W,
		PARAM_GET_MINDISTANCE,

		P_NUM
	};

	/////////////////////////////////////////////////////////////////////

	struct EffectData
	{
		int sourceID;	// DEBUG
		
		std::shared_ptr<Loudspeaker::CSingleSourceDSP_LS> audioSource;			//LoudSpeakers Core			
		Loudspeaker::CCore_LS core;												//Loud AudioSource AudioSource Core	
		std::shared_ptr<Loudspeaker::CSpeakerSet> speakers;						//Speakers configuration
		Loudspeaker::CSpeakerSetConfiguration loudSpeakersConf;

		//Speakers - store geometric position of each speaker	
		std::vector<Common::CVector3> speakerPositions;
		std::vector<bool> configuringSpeakers;		

		bool coreReady;
		float parameters[P_NUM];

		//FIXME delete me
		bool firstTime = true;

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
			#ifdef DEBUG_LOG_FILE
			ofstream logfile;
			int sourceid = data->sourceID;
			logfile.open("3DTI_LoudSpeakersSpatializer_DebugLog.txt", ofstream::out | ofstream::app);
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
		
		///WARNING param names size is limited.
	
		// ADVANCED CONFIGURATION
		RegisterParameter(definition, "ScaleFactor", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 1.0f, PARAM_SCALE_FACTOR, "Scale factor for over/under sized scenes");
		RegisterParameter(definition, "SourceID", "", -1.0f, FLT_MAX, -1.0f, 1.0f, 1.0f, PARAM_SOURCE_ID, "Source ID for debug");
		RegisterParameter(definition, "MODfarLPF", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_FARLPF, "Far distance LPF module enabler");
		RegisterParameter(definition, "MODDistAtt", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_DISTATT, "Distance attenuation module enabler");
		RegisterParameter(definition, "MAGAneAtt", "dB", -30.0f, 0.0f, -6.0f, 1.0f, 1.0f, PARAM_MAG_ANECHATT, "Anechoic distance attenuation");
		RegisterParameter(definition, "MAGSounSpd", "m/s", 0.0f, 1000.0f, 343.0f, 1.0f, 1.0f, PARAM_MAG_SOUNDSPEED, "Sound speed");
		RegisterParameter(definition, "DebugLog", "", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, PARAM_DEBUG_LOG, "Generate debug log");

		// SPEAKERS CONFIGURATION
		RegisterParameter(definition, "SetSpeakers", "m", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, PARAM_SAVE_SPEAKERS_CONFIG, "Set Speakers configuration");
		RegisterParameter(definition, "speaker1_x", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_1_X, "Speaker 1 position, x coordinate");
		RegisterParameter(definition, "speaker1_y", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_1_Y, "Speaker 1 position, y coordinate");
		RegisterParameter(definition, "speaker1_z", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_1_Z, "Speaker 1 position, z coordinate");
		RegisterParameter(definition, "speaker2_x", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_2_X, "Speaker 2 position, x coordinate");
		RegisterParameter(definition, "speaker2_y", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_2_Y, "Speaker 2 position, y coordinate");
		RegisterParameter(definition, "speaker2_z", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_2_Z, "Speaker 2 position, z coordinate");
		RegisterParameter(definition, "speaker3_x", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_3_X, "Speaker 3 position, x coordinate");
		RegisterParameter(definition, "speaker3_y", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_3_Y, "Speaker 3 position, y coordinate");
		RegisterParameter(definition, "speaker3_z", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_3_Z, "Speaker 3 position, z coordinate");
		RegisterParameter(definition, "speaker4_x", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_4_X, "Speaker 4 position, x coordinate");
		RegisterParameter(definition, "speaker4_y", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_4_Y, "Speaker 4 position, y coordinate");
		RegisterParameter(definition, "speaker4_z", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_4_Z, "Speaker 4 position, z coordinate");
		RegisterParameter(definition, "speaker5_x", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_5_X, "Speaker 5 position, x coordinate");
		RegisterParameter(definition, "speaker5_y", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_5_Y, "Speaker 5 position, y coordinate");
		RegisterParameter(definition, "speaker5_z", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_5_Z, "Speaker 5 position, z coordinate");
		RegisterParameter(definition, "speaker6_x", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_6_X, "Speaker 6 position, x coordinate");
		RegisterParameter(definition, "speaker6_y", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_6_Y, "Speaker 6 position, y coordinate");
		RegisterParameter(definition, "speaker6_z", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_6_Z, "Speaker 6 position, z coordinate");
		RegisterParameter(definition, "speaker7_x", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_7_X, "Speaker 7 position, x coordinate");
		RegisterParameter(definition, "speaker7_y", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_7_Y, "Speaker 7 position, y coordinate");
		RegisterParameter(definition, "speaker7_z", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_7_Z, "Speaker 7 position, z coordinate");
		RegisterParameter(definition, "speaker8_x", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_8_X, "Speaker 8 position, x coordinate");
		RegisterParameter(definition, "speaker8_y", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_8_Y, "Speaker 8 position, y coordinate");
		RegisterParameter(definition, "speaker8_z", "m", -10000.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_SPEAKER_8_Z, "Speaker 8 position, z coordinate");
		RegisterParameter(definition, "speaker1_w", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 0.0f, PARAM_SPEAKER_1_W, "Speaker 1 gain");
		RegisterParameter(definition, "speaker2_w", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 0.0f, PARAM_SPEAKER_2_W, "Speaker 2 gain");
		RegisterParameter(definition, "speaker3_w", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 0.0f, PARAM_SPEAKER_3_W, "Speaker 3 gain");
		RegisterParameter(definition, "speaker4_w", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 0.0f, PARAM_SPEAKER_4_W, "Speaker 4 gain");
		RegisterParameter(definition, "speaker5_w", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 0.0f, PARAM_SPEAKER_5_W, "Speaker 5 gain");
		RegisterParameter(definition, "speaker6_w", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 0.0f, PARAM_SPEAKER_6_W, "Speaker 6 gain");
		RegisterParameter(definition, "speaker7_w", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 0.0f, PARAM_SPEAKER_7_W, "Speaker 7 gain");
		RegisterParameter(definition, "speaker8_w", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 0.0f, PARAM_SPEAKER_8_W, "Speaker 8 gain");
		RegisterParameter(definition, "getmindist", "m", 0.0f, FLT_MAX, 1.0f, 1.0f, 0.0f, PARAM_GET_MINDISTANCE, "Get minimum distance from sources to listener");

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


	void WriteLogHeader(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		// Audio state:
		Common::AudioState_Struct audioState = data->core.GetAudioState();
		WriteLog(state, "CREATE: Sample rate set to ", audioState.sampleRate);
		WriteLog(state, "CREATE: Buffer size set to ", audioState.bufferSize);

		// Listener:		
		//if (data->listener != nullptr)
		//WriteLog(state, "CREATE: Listener created successfully", "");
		//else
		//WriteLog(state, "CREATE: ERROR!!!! Listener creation returned null pointer!", "");

		// Source:		
		if (data->audioSource != nullptr)
			WriteLog(state, "CREATE: Source created successfully", "");
		else
			WriteLog(state, "CREATE: ERROR!!!! Source creation returned null pointer!", "");

		WriteLog(state, "--------------------------------------", "\n");
	}

	/// Load speakers configuration into the Core
	void SaveSpeakersConfiguration(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		// Configuration of Speakers
		
		data->loudSpeakersConf.BeginSetup();

		for (int i = 0; i < data->speakerPositions.size(); i++)
		{
			if (data->configuringSpeakers[i])
				data->loudSpeakersConf.AddLoudspeaker(data->speakerPositions[i]);
		}

		data->loudSpeakersConf.EndSetup();

		//Set Speakers Configuration
		data->speakers->LoadSpeakerConfiguration(std::move(data->loudSpeakersConf));

		// Clear set of configuring speakers to allow new configurations
		data->configuringSpeakers.clear();
		data->configuringSpeakers.assign(MAX_NUMBER_OF_SPEAKERS, false);
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

		// Create listener
		//effectdata->listener = effectdata->core.CreateListener();

		// Init parameters. Core is not ready until we load the HRTF. ILD will be disabled, so we don't need to worry yet
		effectdata->coreReady = false;
		effectdata->parameters[PARAM_SCALE_FACTOR] = 1.0f;
		effectdata->sourceID = -1;

		// Create source 
		effectdata->audioSource = effectdata->core.CreateSingleSourceDSP();

		//Create Speakers Configuration
		effectdata->speakers = effectdata->core.CreateSpeakers();
		effectdata->speakerPositions.assign(MAX_NUMBER_OF_SPEAKERS, Common::CVector3::ZERO);
		effectdata->configuringSpeakers.assign(MAX_NUMBER_OF_SPEAKERS, false);	// Initially set all speakers as not configured yet

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

		Common::CMagnitudes magnitudes;
		int loadResult;

		// Process command sent by C# API
		switch (index)
		{		
			// FUNCTIONALITY TO BE IMPLEMENTED
		case PARAM_SCALE_FACTOR:	// Set scale factor (OPTIONAL)				
			WriteLog(state, "SET PARAMETER: Scale factor changed to ", value);
			break;

		case PARAM_SOURCE_ID:	// DEBUG
			data->sourceID = (int)value;
			WriteLog(state, "SET PARAMETER: Source ID set to ", data->sourceID);
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

		case PARAM_SAVE_SPEAKERS_CONFIG:	// Save Speakers Configuration (MANDATORY)											
			if (value > 0.0f) {
				WriteLog(state, "SET PARAMETER: Save Speakers Config :", value);

				SaveSpeakersConfiguration(state);
				data->coreReady = true;				
				WriteLog(state, "Core ready!!!!!", "");
			}
			else
			{
				//WriteLog(state, "SET PARAMETER: Save Speakers Config", value);
			}			
			break;
			
		case PARAM_SPEAKER_1_X:
			data->speakerPositions[0].x = value;
			data->configuringSpeakers[0] = true;
			WriteLog(state, "SET PARAMETER: Speaker 1 position, x coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_1_Y:
			data->speakerPositions[0].y = value;
			data->configuringSpeakers[0] = true;
			WriteLog(state, "SET PARAMETER: Speaker 1 position, y coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_1_Z:
			data->speakerPositions[0].z = value;
			data->configuringSpeakers[0] = true;
			WriteLog(state, "SET PARAMETER: Speaker 1 position, z coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_2_X:
			data->speakerPositions[1].x = value;
			data->configuringSpeakers[1] = true;
			WriteLog(state, "SET PARAMETER: Speaker 2 position, x coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_2_Y:
			data->speakerPositions[1].y = value;
			data->configuringSpeakers[1] = true;
			WriteLog(state, "SET PARAMETER: Speaker 2 position, y coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_2_Z:
			data->speakerPositions[1].z = value;
			data->configuringSpeakers[1] = true;
			WriteLog(state, "SET PARAMETER: Speaker 2 position, z coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_3_X:
			data->speakerPositions[2].x = value;
			data->configuringSpeakers[2] = true;
			WriteLog(state, "SET PARAMETER: Speaker 3 position, x coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_3_Y:
			data->speakerPositions[2].y = value;
			data->configuringSpeakers[2] = true;
			WriteLog(state, "SET PARAMETER: Speaker 3 position, y coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_3_Z:
			data->speakerPositions[2].z = value;
			data->configuringSpeakers[2] = true;
			WriteLog(state, "SET PARAMETER: Speaker 3 position, z coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_4_X:
			data->speakerPositions[3].x = value;
			data->configuringSpeakers[3] = true;
			WriteLog(state, "SET PARAMETER: Speaker 4 position, x coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_4_Y:
			data->speakerPositions[3].y = value;
			data->configuringSpeakers[3] = true;
			WriteLog(state, "SET PARAMETER: Speaker 4 position, y coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_4_Z:
			data->speakerPositions[3].z = value;
			data->configuringSpeakers[3] = true;
			WriteLog(state, "SET PARAMETER: Speaker 4 position, z coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_5_X:
			data->speakerPositions[4].x = value;
			data->configuringSpeakers[4] = true;
			WriteLog(state, "SET PARAMETER: Speaker 5 position, x coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_5_Y:
			data->speakerPositions[4].y = value;
			data->configuringSpeakers[4] = true;
			WriteLog(state, "SET PARAMETER: Speaker 5 position, y coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_5_Z:
			data->speakerPositions[4].z = value;
			data->configuringSpeakers[4] = true;
			WriteLog(state, "SET PARAMETER: Speaker 5 position, z coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_6_X:
			data->speakerPositions[5].x = value;
			data->configuringSpeakers[5] = true;
			WriteLog(state, "SET PARAMETER: Speaker 6 position, x coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_6_Y:
			data->speakerPositions[5].y = value;
			data->configuringSpeakers[5] = true;
			WriteLog(state, "SET PARAMETER: Speaker 6 position, y coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_6_Z:
			data->speakerPositions[5].z = value;
			data->configuringSpeakers[5] = true;
			WriteLog(state, "SET PARAMETER: Speaker 6 position, z coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_7_X:
			data->speakerPositions[6].x = value;
			data->configuringSpeakers[6] = true;
			WriteLog(state, "SET PARAMETER: Speaker 7 position, x coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_7_Y:
			data->speakerPositions[6].y = value;
			data->configuringSpeakers[6] = true;
			WriteLog(state, "SET PARAMETER: Speaker 7 position, y coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_7_Z:
			data->speakerPositions[6].z = value;
			data->configuringSpeakers[6] = true;
			WriteLog(state, "SET PARAMETER: Speaker 7 position, z coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_8_X:
			data->speakerPositions[7].x = value;
			data->configuringSpeakers[7] = true;
			WriteLog(state, "SET PARAMETER: Speaker 8 position, x coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_8_Y:
			data->speakerPositions[7].y = value;
			data->configuringSpeakers[7] = true;
			WriteLog(state, "SET PARAMETER: Speaker 8 position, y coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_8_Z:
			data->speakerPositions[7].z = value;
			data->configuringSpeakers[7] = true;
			WriteLog(state, "SET PARAMETER: Speaker 8 position, z coordinate, set to ", value);
			break;

		case PARAM_SPEAKER_1_W:			
			data->speakers->GetSpeakerConfiguration().SetLoudspeakerGain(0, value);
			WriteLog(state, "SET PARAMETER: Speaker 1 gain set to ", value);
			break;

		case PARAM_SPEAKER_2_W:
			data->speakers->GetSpeakerConfiguration().SetLoudspeakerGain(1, value);
			WriteLog(state, "SET PARAMETER: Speaker 2 gain set to ", value);
			break;

		case PARAM_SPEAKER_3_W:
			data->speakers->GetSpeakerConfiguration().SetLoudspeakerGain(2, value);
			WriteLog(state, "SET PARAMETER: Speaker 3 gain set to ", value);
			break;

		case PARAM_SPEAKER_4_W:
			data->speakers->GetSpeakerConfiguration().SetLoudspeakerGain(3, value);
			WriteLog(state, "SET PARAMETER: Speaker 4 gain set to ", value);
			break;

		case PARAM_SPEAKER_5_W:
			data->speakers->GetSpeakerConfiguration().SetLoudspeakerGain(4, value);
			WriteLog(state, "SET PARAMETER: Speaker 5 gain set to ", value);
			break;

		case PARAM_SPEAKER_6_W:
			data->speakers->GetSpeakerConfiguration().SetLoudspeakerGain(5, value);
			WriteLog(state, "SET PARAMETER: Speaker 6 gain set to ", value);
			break;

		case PARAM_SPEAKER_7_W:
			data->speakers->GetSpeakerConfiguration().SetLoudspeakerGain(6, value);
			WriteLog(state, "SET PARAMETER: Speaker 7 gain set to ", value);
			break;

		case PARAM_SPEAKER_8_W:
			data->speakers->GetSpeakerConfiguration().SetLoudspeakerGain(7, value);
			WriteLog(state, "SET PARAMETER: Speaker 8 gain set to ", value);
			break;

		case PARAM_GET_MINDISTANCE:			
			WriteLog(state, "SET PARAMETER: WARNING! Attempt to set PARAM_GET_MINDISTANCE, which is a read-only parameter: ", value);
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
		if (value != NULL)
			*value = data->parameters[index];
		if (valuestr != NULL)
			valuestr[0] = 0;

		if (index == PARAM_GET_MINDISTANCE)
			*value = data->speakers->GetSpeakerSetMaximumDistance();

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
		if (inchannels != 8 || outchannels != 8 ||
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
		data->core.SetListenerTransform(ComputeListenerTransformFromMatrix(state->spatializerdata->listenermatrix, data->parameters[PARAM_SCALE_FACTOR]));

		// Transform input buffer
		CMonoBuffer<float> inMonoBuffer(length);
		for (int i = 0; i < length; i++)
		{
			inMonoBuffer[i] = inbuffer[i * inchannels]; // We take only the left channel
		}

		// Process!!
		//CMultiChannelBuffer<float> outMultiChannelBuffer;
		//CMultiChannelBuffer<float> outMultiChannelBuffer(length * outchannels);		
		CMultiChannelBuffer<float> outMultiChannelBuffer(length * data->loudSpeakersConf.GetSpeakersConfiguration().size());
		data->audioSource->UpdateBuffer(inMonoBuffer);
		data->core.ProcessLoudspeakerAnechoic(outMultiChannelBuffer);		
		
		// Transform output buffer, adding zeroed channels if necessary		
		//int i = 0;
		//bool temp = false;
		//for (auto it = outMultiChannelBuffer.begin(); it != outMultiChannelBuffer.end(); it++)
		//{
		//	outbuffer[i++] = *it;		
		//}
		size_t outBufferIndex = 0;
		size_t coreOutIndex = 0;
		for (int s = 0; s < length; s++)
		{
			for (int c = 0; c < outchannels; c++)
			{
				if (data->loudSpeakersConf.GetSpeakersConfiguration().size() > c)
				{
					outbuffer[outBufferIndex++] = outMultiChannelBuffer[coreOutIndex++];
				}
				else
					outbuffer[outBufferIndex++] = 0.0f;
			}
		}

		if (data->firstTime) {
			WriteLog(state, "PROCESS: Mono Buffer Size :", inMonoBuffer.size());
			WriteLog(state, "PROCESS: Multi Buffer Size :", length * outchannels);
			WriteLog(state, "PROCESS: SampleRate   Size :", (int)state->samplerate);
			data->firstTime = false;
		}
		
		return UNITY_AUDIODSP_OK;
	}
}