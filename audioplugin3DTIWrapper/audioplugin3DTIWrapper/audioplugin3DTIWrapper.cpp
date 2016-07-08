/**
*** 3D-Tune-In Toolkit Unity Wrapper ***
*
* version beta 1.1
* Created on: July 2016
*
* Author: 3DI-DIANA Research Group / University of Malaga / Spain
* Contact: areyes@uma.es
*
* Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
* Module: 3DTI Toolkit Unity Wrapper
**/

#include "stdafx.h"

#include "AudioPluginUtil.h"

// Tell the 3DTI Toolkit Core that we will be using the Unity axis convention!
// WARNING: This define must be done before including Core.h!
#define AXIS_CONVENTION UNITY

#include "Core.h"
#include "CoreState.h"
#include "Debugger.h"

// Includes for reading HRTF and ILD data and logging dor debug
#include <fstream>
#include <iostream>
#include <time.h>
#include <HRTF/HRTFCereal.h>
#include <ILD/ILDCereal.h>

using namespace std;

// DEBUG LOG FILE
//#define LOG_FILE
template <class T>
void WriteLog(int sourceid, string logtext, const T& value)
{
#ifdef LOG_FILE
	ofstream logfile;
	logfile.open("debuglog.txt", ofstream::out | ofstream::app);
	logfile << sourceid << ": " << logtext << value << endl;
	logfile.close();
#endif
}

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
		P_NUM
	};

/////////////////////////////////////////////////////////////////////
	
	struct EffectData
	{
		int sourceID;	// DEBUG
		float parameters[P_NUM];		
		std::shared_ptr<CSingleSourceDSP> audioSource;		
		CCore* core;
		bool coreReady;				// Keep this until final version of CoreState
	};

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
		
		definition.flags |= UnityAudioEffectDefinitionFlags_IsSpatializer;
		return numparams;
	}

/////////////////////////////////////////////////////////////////////

	static UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK DistanceAttenuationCallback(UnityAudioEffectState* state, float distanceIn, float attenuationIn, float* attenuationOut)
	{
		// 3DTI: distance attenuation is included in ProcessAnechoic. Do nothing here
		*attenuationOut = attenuationIn;
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	int LoadHRTFBinaryFile(UnityAudioEffectState* state, float floatHandle)
	{
		EffectData* data = state->GetEffectData<EffectData>();		

		// Cast from float to HANDLE
		int intHandle = (int)floatHandle;
		HANDLE fileHandle = (HANDLE)intHandle;

		// Check that handle is correct
		if (fileHandle == INVALID_HANDLE_VALUE)
		{
			WriteLog(data->sourceID, "Error!!! Invalid file handle in HRTF binary file", "");
			return -1;
		}
		
		// Get HRTF and check errors
		CHRTF myHead = HRTF::CreateFrom3dtiHandle(fileHandle);		
		if (myHead.GetHRIRLength() != 0)		// TO DO: Improve this error check
		{
			data->core->BeginSetup();
			data->core->LoadHRTF(std::move(myHead));
			WriteLog(data->sourceID, "HRTF loaded from binary 3DTI file:", "");
			WriteLog(data->sourceID, "	HRIR length: ", myHead.GetHRIRLength());
			WriteLog(data->sourceID, "	Azimuth step: ", myHead.GetAzimuthStep());
			WriteLog(data->sourceID, "	Elevation step: ", myHead.GetElevationStep());
			return 1;
		}
		else
		{
			WriteLog(data->sourceID, "Error!!! could not create HRTF from handle", "");
			return -1;
		}
	}

/////////////////////////////////////////////////////////////////////

	int LoadILDBinaryFile(UnityAudioEffectState* state, float floatHandle)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		// Cast from float to HANDLE
		int intHandle = (int)floatHandle;
		HANDLE fileHandle = (HANDLE)intHandle;

		// Check that handle is correct
		if (fileHandle == INVALID_HANDLE_VALUE)
		{
			WriteLog(data->sourceID, "Error!!! Invalid file handle in ILD binary file", "");
			return -1;
		}

		// Get ILD and check errors
		ILD_HashTable h;
		h = ILD::CreateFrom3dtiHandle(fileHandle);
		if (h.size() > 0)		// TO DO: Improve this error check		
		{
			CILD::SetILD_HashTable(std::move(h));
			WriteLog(data->sourceID, "ILD loaded from binary 3DTI file: ", h.size());
			return 1;
		}
		else
		{
			WriteLog(data->sourceID, "Error!!! could not create ILD from handle", "");
			return -1;
		}		
	}

/////////////////////////////////////////////////////////////////////

	CTransform ComputeListenerTransformFromMatrix(float* listenerMatrix, float scale)
	{
		// SET LISTENER POSITION

		// Inverted 4x4 listener matrix, as provided by Unity
		float L[16];					
		for (int i = 0; i < 16; i++)
			L[i] = listenerMatrix[i];

		//float listenerpos_x = -(L[0] * L[12] + L[1] * L[13] + L[2] * L[14]) * scale;	// From Unity documentation, if camera is rotated wrt listener (NOT TESTED)
		//float listenerpos_y = -(L[4] * L[12] + L[5] * L[13] + L[6] * L[14]) * scale;	// From Unity documentation, if camera is rotated wrt listener (NOT TESTED)
		//float listenerpos_z = -(L[8] * L[12] + L[9] * L[13] + L[10] * L[14]) * scale;	// From Unity documentation, if camera is rotated wrt listener (NOT TESTED)
		float listenerpos_x = -L[12] * scale;	// If camera is not rotated
		float listenerpos_y = -L[13] * scale;	// If camera is not rotated
		float listenerpos_z = -L[14] * scale;	// If camera is not rotated
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
			state->spatializerdata->distanceattenuationcallback = DistanceAttenuationCallback;	// TO DO: check if we can remove this
		InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->parameters);		

		// DEBUG: Start log file
		#ifdef LOG_FILE
		time_t rawtime;
		struct tm * timeinfo;
		char buffer[80];
		time(&rawtime);
		timeinfo = localtime(&rawtime);
		strftime(buffer, 80, "%d-%m-%Y %I:%M:%S", timeinfo);
		std::string str(buffer);
		WriteLog(effectdata->sourceID, "***************************************************************************************\nDebug log started at ", str);
		#endif
		//

		// Set default audio state
		// QUESTION: How does this overlaps with explicit call to SetAudioState from C# API? 
		AudioState_Struct audioState;
		audioState.sampleRate = (int)state->samplerate;
		audioState.bufferSize = (int)state->dspbuffersize;
		WriteLog(effectdata->sourceID, "Sample rate set to ", state->samplerate);
		WriteLog(effectdata->sourceID, "Buffer size set to ", state->dspbuffersize);

		// Core initialization
		effectdata->core = new CCore(audioState, 0.0875f);
		effectdata->core->BeginSetup();
		WriteLog(effectdata->sourceID, "Core setup started...", "");

		// Init parameters. Core is not ready until we load the HRTF (and ILD?)...
		effectdata->coreReady = false;
		effectdata->parameters[PARAM_SCALE_FACTOR] = 1.0f;
		effectdata->sourceID = -1;

		// Set listener transform	
		// WARNING: the source and listener matrix passed in CreateCallback seem to be always ZERO (this might create a problem with Quaternions)
		//SetListenerTransformFromMatrix(state->spatializerdata->listenermatrix, effectdata->parameters[PARAM_SCALE_FACTOR]);

		// Create source and set default interpolation method		
		effectdata->audioSource = effectdata->core->CreateSingleSourceDSP();	
		effectdata->audioSource->SetInterpolation(3);	// Default
		// WARNING: the source and listener matrix passed in CreateCallback seem to be always ZERO
		//effectdata->audioSource->SetSourceTransform(ComputeSourceTransformFromMatrix(state->spatializerdata->sourcematrix, effectdata->parameters[PARAM_SCALE_FACTOR]));			

		//CDebugger::Instance().SetVerbosityMode(VERBOSITY_MODE_ALL);
		//CDebugger::Instance().SetErrorLogFile("coredebug.txt");
		//CDebugger::Instance().SetAssertMode(ASSERT_MODE_CONTINUE);

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

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		if (index >= P_NUM)
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		data->parameters[index] = value;		

		CMagnitudes magnitudes;

		// Process command sent by C# API
		switch (index)
		{
			case PARAM_HRTF_FILE_HANDLE:	// Load HRTF binary file (MANDATORY)
				if (LoadHRTFBinaryFile(state, value) == 1)
				{
					data->core->EndSetup();
					data->coreReady = true;		// Temporary solution before integration of final CoreState class					
					WriteLog(data->sourceID, "...Core ready! ", "");
				}
				break;

			case PARAM_ILD_FILE_HANDLE:	// Load ILD binary file (MANDATORY?)
				if (LoadILDBinaryFile(state, value) == 1)
				{
				}
				break;

			case PARAM_HEAD_RADIUS:	// Set listener head radius (OPTIONAL)
				data->core->SetHeadRadius(value);				
				WriteLog(data->sourceID, "Listener head radius changed: ", value);
				break;

			case PARAM_SCALE_FACTOR:	// Set scale factor (OPTIONAL)				
				WriteLog(data->sourceID, "Scale factor changed: ", value);
				break;

			case PARAM_SOURCE_ID:	// DEBUG
				data->sourceID = (int)value;
				WriteLog(data->sourceID, "Source ID set to: ", data->sourceID);
				break;

			case PARAM_CUSTOM_ITD:	// Enable custom ITD (OPTIONAL)
				if (value > 0.0f)
				{
					data->core->SetCustomizedITD(true);
					WriteLog(data->sourceID, "Custom ITD: ", "Enabled");
				}
				else
				{
					data->core->SetCustomizedITD(false);
					WriteLog(data->sourceID, "Custom ITD: ", "Disabled");
				}
				break;

			case PARAM_HRTF_INTERPOLATION:	// Change interpolation method (OPTIONAL)
				data->audioSource->SetInterpolation((int)value);	
				WriteLog(data->sourceID, "HRTF Interpolation method switched to: ", (int) value);
				break;

			case PARAM_MOD_FARLPF:
				if (value > 0.0f)
				{
					data->audioSource->modEnabler.doFarDistanceLPF = true;
					WriteLog(data->sourceID, "Far distance LPF: ", "Enabled");
				}
				else
				{
					data->audioSource->modEnabler.doFarDistanceLPF = false;
					WriteLog(data->sourceID, "Far distance LPF: ", "Disabled");
				}
				break;

			case PARAM_MOD_DISTATT:
				if (value > 0.0f)
				{
					data->audioSource->modEnabler.doDistanceAttenuation = true;
					WriteLog(data->sourceID, "Distance attenuation: ", "Enabled");
				}
				else
				{
					data->audioSource->modEnabler.doDistanceAttenuation = false;
					WriteLog(data->sourceID, "Distance attenuation: ", "Disabled");
				}
				break;

			case PARAM_MOD_ILD:
				if (value > 0.0f)
				{
					data->audioSource->modEnabler.doILD = true;
					WriteLog(data->sourceID, "Near distance ILD: ", "Enabled");
				}
				else
				{
					data->audioSource->modEnabler.doILD = false;
					WriteLog(data->sourceID, "Near distance ILD: ", "Disabled");
				}
				break;

			case PARAM_MOD_HRTF:
				if (value > 0.0f)
				{
					data->audioSource->modEnabler.doHRTF = true;
					WriteLog(data->sourceID, "HRTF convolution: ", "Enabled");
				}
				else
				{
					data->audioSource->modEnabler.doHRTF = false;
					WriteLog(data->sourceID, "HRTF convolution: ", "Disabled");
				}
				break;

			case PARAM_MAG_ANECHATT:
				magnitudes = data->core->GetMagnitudes();
				magnitudes.SetAnechoicDistanceAttenuation(value);
				data->core->SetMagnitudes(magnitudes);
				WriteLog(data->sourceID, "Anechoic distance attenuation set to (dB): ", value);
				break;

			case PARAM_MAG_REVERBATT:
				magnitudes = data->core->GetMagnitudes();
				magnitudes.SetReverbDistanceAttenuation(value);
				data->core->SetMagnitudes(magnitudes);
				WriteLog(data->sourceID, "Reverb distance attenuation set to (dB): ", value);
				break;

			case PARAM_MAG_SOUNDSPEED:
				magnitudes = data->core->GetMagnitudes();
				magnitudes.SetSoundSpeed(value);
				data->core->SetMagnitudes(magnitudes);
				WriteLog(data->sourceID, "Sound speed set to (m/s): ", value);
				break;

			default:
				WriteLog(data->sourceID, "Unknown float parameter passed from API: ", index);
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
		// TO DO: should we do something here? I don't think so
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
	{		
		EffectData* data = state->GetEffectData<EffectData>();		

		// Check that I/O formats are right and that the host API supports this feature		
		if (inchannels != 2 || outchannels != 2 ||
			!IsHostCompatible(state) || state->spatializerdata == NULL)
		{
			WriteLog(data->sourceID, "	ERROR! Wrong number of channels or Host is not compatible", "");
			//memcpy(outbuffer, inbuffer, length * outchannels * sizeof(float));
			memset(outbuffer, 0.0f, length * outchannels * sizeof(float));
			return UNITY_AUDIODSP_OK;
		}						
		
		// Before doing anything, check that the core is ready
		// Temporary solution, before integration of CoreState class
		if (!data->coreReady)
		{
			// Put silence in outbuffer
			memset(outbuffer, 0.0f, length * outchannels * sizeof(float));
			return UNITY_AUDIODSP_OK;
		}

		try
		{
			//data->core->EndSetup();

			// Set source and listener transforms
			// Orientation does not matters for audio sources
			data->audioSource->SetSourceTransform(ComputeSourceTransformFromMatrix(state->spatializerdata->sourcematrix, data->parameters[PARAM_SCALE_FACTOR]));
			data->core->SetListenerTransform(ComputeListenerTransformFromMatrix(state->spatializerdata->listenermatrix, data->parameters[PARAM_SCALE_FACTOR]));

			// Transform input buffer
			// TO DO: Avoid this copy
			CMonoBuffer<float> inMonoBuffer(length);
			for (int i = 0; i < length; i++)
			{
				inMonoBuffer[i] = inbuffer[i * 2]; // We take only the left channel
			}

			// Process!!
			CStereoBuffer<float> outStereoBuffer(length * 2);
			data->audioSource->UpdateBuffer(inMonoBuffer);
			data->audioSource->ProcessAnechoic(outStereoBuffer);

			// Transform output buffer
			// TO DO: Avoid this copy
			int i = 0;
			for (auto it = outStereoBuffer.begin(); it != outStereoBuffer.end(); it++)
			{
				outbuffer[i++] = *it;
			}
		}	
		catch (State_exception & e) 
		{
			WriteLog(data->sourceID, "Core exception! Tried to process before ending setup: ", e.what());
		}
		catch (NotInitialized_exception & e) 
		{
			WriteLog(data->sourceID, "Core exception! Tried to process before ending setup: ", e.what());
		}
		catch (...)
		{
			WriteLog(data->sourceID, "Core exception! Unknown.", "");
		}

		return UNITY_AUDIODSP_OK;
	}
}
