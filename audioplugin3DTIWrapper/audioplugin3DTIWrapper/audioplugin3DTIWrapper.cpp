/**
*** 3D-Tune-In Toolkit Unity Wrapper ***
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

#include "AudioPluginUtil.h"

// Tell the 3DTI Toolkit Core that we will be using the Unity axis convention!
// WARNING: This define must be done before including Core.h!
#define AXIS_CONVENTION UNITY

#include "../../3DTI_Toolkit_Core/Core.h"

// Includes for reading HRTF data and logging dor debug
#include <fstream>
#include <iostream>
#include <time.h>

using namespace std;

// DEBUG LOG FILE
#define LOG_FILE
template <class T>
void WriteLog(string logtext, const T& value)
{
#ifdef LOG_FILE
	ofstream logfile;
	logfile.open("debuglog.txt", ofstream::out | ofstream::app);
	logfile << logtext << value << endl;
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
		P_NUM
	};

/////////////////////////////////////////////////////////////////////
	
	CCore* core;
	bool coreReady;				// Temporary solution before integration of CoreState class in Core
	bool pluginCreated = false;	// Be sure that core is not initialized more than once
	struct EffectData
	{
		float parameters[P_NUM];		
		std::shared_ptr<CSingleSourceDSP> audioSource;		
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

	void LoadHRTFBinaryFile(float floatHandle)
	{
		// Cast from float to HANDLE
		int intHandle = (int)floatHandle;
		HANDLE fileHandle = (HANDLE)intHandle;

		// Check that handle is correct
		if (fileHandle == INVALID_HANDLE_VALUE)
		{
			WriteLog("Error!!! Invalid file handle in HRTF binary file", "");
			return;
		}

		// TEST READING SOME TEXT				
		char ReadBuffer[6] = { 0 };	
		DWORD bytesToRead = 5;
		DWORD bytesActuallyRead;				
		ReadFile(fileHandle, ReadBuffer, bytesToRead, &bytesActuallyRead, NULL);
		WriteLog("Read from HRTF file TEST: ", ReadBuffer);

		// Close file
		CloseHandle(fileHandle);
	}

/////////////////////////////////////////////////////////////////////

	void SetListenerTransformFromMatrix(float* listenerMatrix, float scale)
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
		core->SetListenerTransform(listenerTransform);
		WriteLog("Listener position set to: ", listenerTransform.GetPosition());
	}

/////////////////////////////////////////////////////////////////////

	void SetSourceTransformFromMatrix(std::shared_ptr<CSingleSourceDSP> audioSource, float* sourceMatrix, float scale)
	{
		// Orientation does not matters for audio sources
		CTransform sourceTransform;
		sourceTransform.SetPosition(CVector3(sourceMatrix[12] * scale, sourceMatrix[13] * scale, sourceMatrix[14] * scale));		
		audioSource->SetSourceTransform(sourceTransform);
		WriteLog("Source position set to: ", sourceTransform.GetPosition());
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

		// For some reason, Unity eventually call this method more than once

		if (!pluginCreated)
		{
			pluginCreated = true;

			// DEBUG: Start log file
			#ifdef LOG_FILE
			time_t rawtime;
			struct tm * timeinfo;
			char buffer[80];
			time(&rawtime);
			timeinfo = localtime(&rawtime);
			strftime(buffer, 80, "%d-%m-%Y %I:%M:%S", timeinfo);
			std::string str(buffer);
			WriteLog("***************************************************************************************\nDebug log started at ", str);
			#endif
			//

			// Core initialization
			core = new CCore();
			WriteLog("Core initialized", "");

			// Init parameters. Core is not ready until we load the HRTF...
			coreReady = false;
			effectdata->parameters[PARAM_SCALE_FACTOR] = 1.0f;

			// Set default audio state
			// QUESTIONS: How does this overlaps with explicit call to SetAudioState from C# API? What about buffer size?
			CAudioState audioState;
			audioState.SetSampleRate((int)state->samplerate);
			core->SetAudioState(audioState);
			WriteLog("Sample rate set to ", state->samplerate);

			// Set listener transform	
			// WARNING: the source and listener matrix passed in CreateCallback seem to be always ZERO (this might create a problem with Quaternions)
			//SetListenerTransformFromMatrix(state->spatializerdata->listenermatrix, effectdata->parameters[PARAM_SCALE_FACTOR]);

			// Create source and set transform		
			// WARNING: the source and listener matrix passed in CreateCallback seem to be always ZERO
			effectdata->audioSource = core->CreateSingleSourceDSP();
			SetSourceTransformFromMatrix(effectdata->audioSource, state->spatializerdata->sourcematrix, effectdata->parameters[PARAM_SCALE_FACTOR]);
		}

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

		// Process command sent by C# API
		switch (index)
		{
			case PARAM_HRTF_FILE_HANDLE:	// Load HRTF binary file (MANDATORY)
				LoadHRTFBinaryFile(value);
				//coreReady = true;		// Temporary solution before integration of CoreState class
				WriteLog("Core ready! ", "");
				break;

			case PARAM_HEAD_RADIUS:	// Set listener head radius (OPTIONAL)
				core->SetHeadRadius(value);				
				WriteLog("Listener head radius changed: ", value);
				break;

			case PARAM_SCALE_FACTOR:	// Set scale factor (OPTIONAL)				
				WriteLog("Scale factor changed: ", value);
				break;

			default:
				WriteLog("Unknown float parameter passed from API: ", index);
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
		// Check that I/O formats are right and that the host API supports this feature		
		if (inchannels != 2 || outchannels != 2 ||
			!IsHostCompatible(state) || state->spatializerdata == NULL)
		{
			WriteLog("	ERROR! Wrong number of channels or Host is not compatible", "");
			memcpy(outbuffer, inbuffer, length * outchannels * sizeof(float));
			return UNITY_AUDIODSP_OK;
		}						
		EffectData* data = state->GetEffectData<EffectData>();

		// Before doing anything, check that the core is ready
		// Temporary solution, before integration of CoreState class
		if (!coreReady)
			return UNITY_AUDIODSP_OK;

		// Set source and listener transforms
		SetSourceTransformFromMatrix(data->audioSource, state->spatializerdata->sourcematrix, data->parameters[PARAM_SCALE_FACTOR]);
		SetListenerTransformFromMatrix(state->spatializerdata->listenermatrix, data->parameters[PARAM_SCALE_FACTOR]);
		
		// Transform input buffer
		// TO DO: Avoid this copy
		CMonoBuffer<float> inMonoBuffer(length);
		for (int i = 0; i < length; i++)
		{
			inMonoBuffer[i] = inbuffer[i * 2]; // We take only the left channel
		}

		// Process!!
		CStereoBuffer<float> outStereoBuffer(length * 2);
		data->audioSource->SetInterpolation(3);		// Do we still need this?
		data->audioSource->UpdateBuffer(inMonoBuffer);
		data->audioSource->ProcessAnechoic(outStereoBuffer);		

		// Transform output buffer
		// TO DO: Avoid this copy
		int i = 0;
		for (auto it = outStereoBuffer.begin(); it != outStereoBuffer.end(); it++)
		{
			outbuffer[i++] = *it;
		}

		return UNITY_AUDIODSP_OK;
	}
}
