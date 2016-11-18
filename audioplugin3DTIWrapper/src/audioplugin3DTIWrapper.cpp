/**
*** 3D-Tune-In Toolkit Unity Wrapper ***
*
* version beta 1.2
* Created on: October 2016
*
* Author: 3DI-DIANA Research Group / University of Malaga / Spain
* Contact: areyes@uma.es
*
* Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
* Module: 3DTI Toolkit Unity Wrapper
**/

#include "AudioPluginUtil.h"

#include <BinauralSpatializer/Core.h>
#include <Common/Debugger.h>

// Includes for reading HRTF and ILD data and logging dor debug
#include <fstream>
#include <iostream>

#include <HRTF/HRTFCereal.h>
#include <ILD/ILDCereal.h>

#define RESULT_LOAD_WAITING 0
#define RESULT_LOAD_OK 1
#define RESULT_LOAD_BADHANDLE -1
#define RESULT_LOAD_WRONGDATA -2

//#ifdef UNITY_OSX FIXME: should get this define from config. 
#include <cfloat>
//#endif

// DEBUG LOG 
#ifdef UNITY_ANDROID
#define DEBUG_LOG_CAT
#else
//#define DEBUG_LOG_FILE
#endif

#ifdef DEBUG_LOG_CAT
#include <android/log.h> 
#include <string>
#include <sstream>
#endif

/////////////////////////////////////////////////////////////////////

namespace UnityWrapper3DTI
{
    enum
    {
		PARAM_HRTF_FILE_HANDLE,
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
		PARAM_MAG_REVERBATT,
		PARAM_MAG_SOUNDSPEED,
		PARAM_ILD_FILE_HANDLE,
		PARAM_LOAD_RESULT,
		P_NUM
	};

	/////////////////////////////////////////////////////////////////////

    struct EffectData
    {
		int sourceID;	// DEBUG
		std::shared_ptr<Binaural::CSingleSourceDSP> audioSource;
		std::shared_ptr<Binaural::CListener> listener;
		Binaural::CCore* core;
		bool coreReady;
		float parameters[P_NUM];
	};

	/////////////////////////////////////////////////////////////////////

	template <class T>
    void WriteLog(UnityAudioEffectState* state, std::string logtext, const T& value)
	{
		#ifdef DEBUG_LOG_FILE
			ofstream logfile;
			int sourceid = state->GetEffectData<EffectData>()->sourceID;
			#ifdef UNITY_ANDROID
				logfile.open("/storage/emulated/0/Android/data/com.Consortium3DTI.UnityWrapper/files/debuglog.txt", ofstream::out | ofstream::app);
			#else
				logfile.open("debuglog.txt", ofstream::out | ofstream::app);
			#endif
			logfile << sourceid << ": " << logtext << value << endl;
			logfile.close();
		#endif

		#ifdef DEBUG_LOG_CAT						
			std::ostringstream os;
			os << logtext << value;
			string fulltext = os.str();			
			__android_log_print(ANDROID_LOG_DEBUG, "3DTIUNITYWRAPPER", fulltext.c_str());
		#endif
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
		RegisterParameter(definition, "HRTFHandle", "", 0.0f, FLT_MAX, 0.0f, 1.0f, 1.0f, PARAM_HRTF_FILE_HANDLE, "Handle of HRTF binary file");
		RegisterParameter(definition, "HeadRadius", "m", 0.0f, FLT_MAX, 0.0875f, 1.0f, 1.0f, PARAM_HEAD_RADIUS, "Listener head radius");
		RegisterParameter(definition, "ScaleFactor", "", 0.0f, FLT_MAX, 1.0f, 1.0f, 1.0f, PARAM_SCALE_FACTOR, "Scale factor for over/under sized scenes");
		RegisterParameter(definition, "SourceID", "", 0.0f, FLT_MAX, -1.0f, 1.0f, 1.0f, PARAM_SOURCE_ID, "Source ID for debug");
		RegisterParameter(definition, "CustomITD", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_CUSTOM_ITD, "Enabled custom ITD");
		RegisterParameter(definition, "HRTFInterp", "", 0.0f, 3.0f, 3.0f, 1.0f, 1.0f, PARAM_HRTF_INTERPOLATION, "HRTF Interpolation method");
		RegisterParameter(definition, "MODfarLPF", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_FARLPF, "Far distance LPF module enabler");
		RegisterParameter(definition, "MODDistAtt", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_DISTATT, "Distance attenuation module enabler");
		RegisterParameter(definition, "MODILD", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_ILD, "Near distance ILD module enabler");
		RegisterParameter(definition, "MODHRTF", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_HRTF, "HRTF module enabler");
		RegisterParameter(definition, "MAGAneAtt", "dB", -30.0f, 0.0f, -6.0f, 1.0f, 1.0f, PARAM_MAG_ANECHATT, "Anechoic distance attenuation");
		RegisterParameter(definition, "MAGRevAtt", "dB", -30.0f, 0.0f, -6.0f, 1.0f, 1.0f, PARAM_MAG_REVERBATT, "Reverb distance attenuation");
		RegisterParameter(definition, "MAGSounSpd", "m/s", 0.0f, 1000.0f, 343.0f, 1.0f, 1.0f, PARAM_MAG_SOUNDSPEED, "Sound speed");
		RegisterParameter(definition, "ILDHandle", "", 0.0f, FLT_MAX, 0.0f, 1.0f, 1.0f, PARAM_ILD_FILE_HANDLE, "Handle of ILD binary file");
		RegisterParameter(definition, "LoadResult", "", -100.0f, FLT_MAX, 0.0f, 1.0f, 0.0f, PARAM_LOAD_RESULT, "Result of loading file");
        definition.flags |= UnityAudioEffectDefinitionFlags_IsSpatializer;
        return numparams;
    }

	/////////////////////////////////////////////////////////////////////

    static UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK DistanceAttenuationCallback(UnityAudioEffectState* state, float distanceIn, float attenuationIn, float* attenuationOut)
    {		
		*attenuationOut = attenuationIn;
		return UNITY_AUDIODSP_OK;
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
		listenerTransform.SetOrientation(CQuaternion(qw, qx, qy, qz));
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

    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
    {
        EffectData* effectdata = new EffectData;
        memset(effectdata, 0, sizeof(EffectData));
        state->effectdata = effectdata;
        if (IsHostCompatible(state))
            state->spatializerdata->distanceattenuationcallback = DistanceAttenuationCallback;
        InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->parameters);
		
		// Set default audio state
		// QUESTION: How does this overlaps with explicit call to SetAudioState from C# API? 
		AudioState_Struct audioState;
		audioState.sampleRate = (int)state->samplerate;
		audioState.bufferSize = (int)state->dspbuffersize;
		WriteLog(state, "CREATE: Sample rate set to ", audioState.sampleRate);
		WriteLog(state, "CREATE: Buffer size set to ", audioState.bufferSize);
		
		// Core and listener initialization
		effectdata->core = new Binaural::CCore(audioState);		
		effectdata->listener = effectdata->core->CreateListener();
		if (effectdata->core != nullptr)
			WriteLog(state, "CREATE: Core created successfully", "");
		else
			WriteLog(state, "CREATE: ERROR!!!! Core creation returned null pointer!", "");
		if (effectdata->listener != nullptr)
			WriteLog(state, "CREATE: Listener created successfully", "");
		else
			WriteLog(state, "CREATE: ERROR!!!! Listener creation returned null pointer!", "");
				
		// Init parameters. Core is not ready until we load the HRTF. ILD is disabled yet...
		// What about environment?		
		effectdata->coreReady = false;		
		effectdata->parameters[PARAM_SCALE_FACTOR] = 1.0f;		
		effectdata->sourceID = -1;		
		effectdata->parameters[PARAM_LOAD_RESULT] = RESULT_LOAD_WAITING;		
		WriteLog(state, "CREATE: Internal parameters set", "");

		// Create source and set default interpolation method		
		effectdata->audioSource = effectdata->core->CreateSingleSourceDSP();	
		if (effectdata->audioSource != nullptr)
		{
			WriteLog(state, "CREATE: Source created successfully", "");
			effectdata->audioSource->SetInterpolation(true);
			effectdata->audioSource->SetFrequencyConvolution(true);
			effectdata->audioSource->modEnabler.doILD = false;	// ILD disabled before loading ILD data
			WriteLog(state, "CREATE: Source has been setup", "");
		}
		else
			WriteLog(state, "CREATE: ERROR!!!! Source creation returned null pointer!", "");		

        return UNITY_AUDIODSP_OK;
    }

	/////////////////////////////////////////////////////////////////////

    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
    {
        EffectData* data = state->GetEffectData<EffectData>();
        delete data;
        return UNITY_AUDIODSP_OK;
    }

	/////////////////////////////////////////////////////////////////////

	int LoadHRTFBinaryFile(UnityAudioEffectState* state, float floatHandle)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		#ifdef UNITY_WIN

			// Cast from float to HANDLE
			int intHandle = (int)floatHandle;
			HANDLE fileHandle = (HANDLE)intHandle;

			// Check that handle is correct
			if (fileHandle == INVALID_HANDLE_VALUE)
			{
				WriteLog(state, "LOAD HRTF: ERROR!!!! Invalid file handle in HRTF binary file", "");
				return RESULT_LOAD_BADHANDLE;				
			}

			// Get HRTF and check errors
			CHRTF myHead = HRTF::CreateFrom3dtiHandle(fileHandle, state->dspbuffersize, state->samplerate);		// Check if arguments are always correct
			if (myHead.GetHRIRLength() != 0)		// TO DO: Improve this error check
			{
				data->listener->LoadHRTF(std::move(myHead));
				WriteLog(state, "LOAD HRTF: HRTF loaded from binary 3DTI file: ", "");
				WriteLog(state, "           HRIR length is ", data->listener->GetHRTF().GetHRIRLength());
				WriteLog(state, "           Sample rate is ", state->samplerate);
				WriteLog(state, "           Buffer size is ", state->dspbuffersize);
				return RESULT_LOAD_OK;
			}
			else
			{
				WriteLog(state, "LOAD HRTF: ERROR!!! Could not create HRTF from handle", "");
				return RESULT_LOAD_WRONGDATA;
			}
		#else

			// Cast from float to HANDLE
			int intHandle = (int)floatHandle;

			// TO DO: check invalid handle!

			// Get HRTF and check errors
			CHRTF myHead = HRTF::CreateFrom3dtiHandle(intHandle, state->dspbuffersize, state->samplerate);		// Check if arguments are always correct
			if (myHead.GetHRIRLength() != 0)		// TO DO: Improve this error check
			{
				data->listener->LoadHRTF(std::move(myHead));
				WriteLog(state, "LOAD HRTF: HRTF loaded from binary 3DTI file: ", "");
				WriteLog(state, "           HRIR length is ", data->listener->GetHRTF().GetHRIRLength());
				WriteLog(state, "           Sample rate is ", state->samplerate);
				WriteLog(state, "           Buffer size is ", state->dspbuffersize);
				return RESULT_LOAD_OK;
			}
			else
			{
				WriteLog(state, "LOAD HRTF: ERROR!!! Could not create HRTF from handle", "");
				return RESULT_LOAD_WRONGDATA;
			}		

		#endif
	}

	/////////////////////////////////////////////////////////////////////

	int LoadILDBinaryFile(UnityAudioEffectState* state, float floatHandle)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		#ifndef UNITY_ANDROID

		// Cast from float to HANDLE
		int intHandle = (int)floatHandle;
		HANDLE fileHandle = (HANDLE)intHandle;

		// Check that handle is correct
		if (fileHandle == INVALID_HANDLE_VALUE)
		{
			WriteLog(state, "LOAD ILD: ERROR!!!! Invalid file handle in ILD binary file", "");
			return RESULT_LOAD_BADHANDLE;
		}

		// Get ILD and check errors
		ILD_HashTable h;
		h = ILD::CreateFrom3dtiHandle(fileHandle);		
		if (h.size() > 0)		// TO DO: Improve this error check		
		{
			CILD::SetILD_HashTable(std::move(h));
			WriteLog(state, "LOAD ILD: ILD loaded from binary 3DTI file ", h.size());
			return RESULT_LOAD_OK;
		}
		else
		{
			WriteLog(state, "LOAD ILD: ERROR!!! could not create ILD from handle", "");
			return RESULT_LOAD_WRONGDATA;
		}

		#else
		
		// Cast from float to HANDLE
		int intHandle = (int)floatHandle;

		// TO DO: check invalid handle!

		// Get ILD and check errors
		ILD_HashTable h;
		h = ILD::CreateFrom3dtiHandle(intHandle);
		if (h.size() > 0)		// TO DO: Improve this error check		
		{
			CILD::SetILD_HashTable(std::move(h));
			WriteLog(state, "LOAD ILD: ILD loaded from binary 3DTI file ", h.size());
			return RESULT_LOAD_OK;
		}
		else
		{
			WriteLog(state, "LOAD ILD: ERROR!!! could not create ILD from handle", "");
			return RESULT_LOAD_WRONGDATA;
		}

		#endif
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
			case PARAM_HRTF_FILE_HANDLE:	// Load HRTF binary file (MANDATORY)
				WriteLog(state, "SET PARAMETER: Loading HRTF from file handle ", value);
				loadResult = LoadHRTFBinaryFile(state, value);
				data->parameters[PARAM_LOAD_RESULT] = loadResult;
				if (loadResult == RESULT_LOAD_OK)
				{					
					data->coreReady = true;							
					WriteLog(state, "Core ready!!!!!!!!!!!!!!!!", "");
				}
				break;

			case PARAM_ILD_FILE_HANDLE:	// Load ILD binary file (MANDATORY?)
				WriteLog(state, "SET PARAMETER: Loading ILD from file handle ", value);	// TO DO: change this when we enable ILD
				loadResult = LoadILDBinaryFile(state, value);
				data->parameters[PARAM_LOAD_RESULT] = loadResult;
				if (loadResult == RESULT_LOAD_OK)
				{
					data->audioSource->modEnabler.doILD = true;
					WriteLog(state, "SET PARAMETER: ILD Enabled", "");
				}
				break;

			case PARAM_HEAD_RADIUS:	// Set listener head radius (OPTIONAL)
				data->listener->SetHeadRadius(value);				
				WriteLog(state, "SET PARAMETER: Listener head radius changed to ", value);
				break;

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
				data->audioSource->SetInterpolation((int)value);	
				WriteLog(state, "SET PARAMETER: HRTF Interpolation method switched to ", (int) value);
				break;

			case PARAM_MOD_FARLPF:
				if (value > 0.0f)
				{
					data->audioSource->modEnabler.doFarDistanceLPF = true;
					WriteLog(state, "SET PARAMETER: Far distance LPF is ", "Enabled");
				}
				else
				{
					data->audioSource->modEnabler.doFarDistanceLPF = false;
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
				magnitudes = data->core->GetMagnitudes();
				magnitudes.SetAnechoicDistanceAttenuation(value);
				data->core->SetMagnitudes(magnitudes);
				WriteLog(state, "SET PARAMETER: Anechoic distance attenuation set to (dB) ", value);
				break;

			case PARAM_MAG_REVERBATT:
				magnitudes = data->core->GetMagnitudes();
				magnitudes.SetReverbDistanceAttenuation(value);
				data->core->SetMagnitudes(magnitudes);
				WriteLog(state, "SET PARAMETER: Reverb distance attenuation set to (dB) ", value);
				break;

			case PARAM_MAG_SOUNDSPEED:
				magnitudes = data->core->GetMagnitudes();
				magnitudes.SetSoundSpeed(value);
				data->core->SetMagnitudes(magnitudes);
				WriteLog(state, "SET PARAMETER: Sound speed set to (m/s) ", value);
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
        return UNITY_AUDIODSP_OK;
    }

	/////////////////////////////////////////////////////////////////////

    int UNITY_AUDIODSP_CALLBACK GetFloatBufferCallback(UnityAudioEffectState* state, const char* name, float* buffer, int numsamples)
    {
        return UNITY_AUDIODSP_OK;
    }

	/////////////////////////////////////////////////////////////////////

	float SquareWaveSample(int i, float amplitude, float period)
	{
		if (i < period)
			return amplitude;
		else
			return 0.0f;
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

		//CTransform sourcePosition = data->audioSource->GetSourceTransform();
		//string sourcePositionStr = "(" + std::to_string(sourcePosition.GetPosition().x) + ", " + std::to_string(sourcePosition.GetPosition().y) + ", " + std::to_string(sourcePosition.GetPosition().z) + ")";		
		//CTransform listenerPosition = data->listener->GetListenerTransform();
		//string listenerPositionStr = "(" + std::to_string(listenerPosition.GetPosition().x) + ", " + std::to_string(listenerPosition.GetPosition().y) + ", " + std::to_string(listenerPosition.GetPosition().z) + ")";
		//string listenerRotationStr = std::to_string(listenerPosition.GetOrientation().w) + "(" + std::to_string(listenerPosition.GetOrientation().x) + ", " + std::to_string(listenerPosition.GetOrientation().y) + ", " + std::to_string(listenerPosition.GetOrientation().z) + ")";
		//CVector3 vectorTo = listenerPosition.GetVectorTo(sourcePosition);
		//string vectorToStr = "(" + std::to_string(vectorTo.x) + ", " + std::to_string(vectorTo.y) + ", " + std::to_string(vectorTo.z) + ")";
		//WriteLog(state, "Source position = ", sourcePositionStr);
		//WriteLog(state, "Listener position = ", listenerPositionStr);
		//WriteLog(state, "Listener rotation = ", listenerRotationStr);
		//WriteLog(state, "Vector from listener to source = ", vectorToStr);

		// Process!!
		CStereoBuffer<float> outStereoBuffer(length * 2);
		data->audioSource->UpdateBuffer(inMonoBuffer);
		data->audioSource->ProcessAnechoic(*data->listener, outStereoBuffer);

		// Transform output buffer			
		int i = 0;
		for (auto it = outStereoBuffer.begin(); it != outStereoBuffer.end(); it++)
		{
			outbuffer[i++] = *it;
		}

        return UNITY_AUDIODSP_OK;
    }
}
