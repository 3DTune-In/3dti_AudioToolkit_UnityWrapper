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

using namespace std;

// DEBUG LOG FILE
//#define LOG_FILE
template <class T>
void WriteLog(int channelid, string logtext, const T& value)
{
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
	enum
	{		
		PARAM_ABIR_FILE_HANDLE,
		PARAM_CHANNEL_ID,	// W=0, X=1, Y=2
		P_NUM
	};

	extern "C" UNITY_AUDIODSP_EXPORT_API bool Create3DTISpatializer(int sampleRate, int dspBufferSize, char* brirPath) {
		//auto spat = spatializer();
		//if (!spat.isInitialized())
		//{
		//	spatializer().initialize(sampleRate, dspBufferSize);
		//	auto env = core.CreateEnvironment();
		//	

		std::ifstream brirStream(brirPath, std::ifstream::binary);
		if (brirStream)
		{
			Spatializer3DTI::Spatializer& spat = Spatializer3DTI::spatializer();
			assert(spat.isInitialized());
			auto environment = spat.core.CreateEnvironment();
			if (BRIR::CreateFrom3dtiStream(brirStream, environment))
			{
				std::atomic_store(&spat.environment, environment);
				return true;
			}

			//// get length of file:
			//brirStream.seekg(0, brirStream.end);
			//int length = brirStream.tellg();
			//brirStream.seekg(0, brirStream.beg);

			//vector<uint8_t> brirBuffer(length);

			//std::cout << "Reading " << length << " characters... ";
			//// read data as a block:
			//brirStream.read(brirBuffer.data(), length);

			//if (brirStream)
			//	std::cout << "all characters read successfully.";
			//else
			//	std::cout << "error: only " << brirStream.gcount() << " could be read";
			//brirStream.close();


			//// ...buffer contains the entire file...

			//delete[] brirBuffer;
		}
		else
		{
			return false;
		}
	}

/////////////////////////////////////////////////////////////////////
	
	struct EffectData
	{
		int channelID;	// W=0, X=1, Y=2
		bool pluginCreated=false;	// createcallback might be called more than once...
		float parameters[P_NUM];		
		//CCore* core;
		//bool coreReady;				// Temporary solution before integration of CoreState class in Core	
		//bool abirReady;				// Temporary solution before integration of CoreState class in Core	
		bool channelReady;			// Temporary solution before integration of CoreState class in Core	
		std::shared_ptr<CEnvironment> environment;
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
		RegisterParameter(definition, "ABIRHandle", "", 0.0f, FLT_MAX, 0.0f, 1.0f, 1.0f, PARAM_ABIR_FILE_HANDLE, "Handle of ABIR binary file");
		RegisterParameter(definition, "ChannelID", "", -1.0f, 2.0f, -1.0f, 1.0f, 1.0f, PARAM_CHANNEL_ID, "b-Format Channel");
		return numparams;
	}

/////////////////////////////////////////////////////////////////////

//	static UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK DistanceAttenuationCallback(UnityAudioEffectState* state, float distanceIn, float attenuationIn, float* attenuationOut)
//	{
//		// 3DTI: distance attenuation is processed per-source in C# script inside Unity. Do nothing here
//		*attenuationOut = attenuationIn;
//		return UNITY_AUDIODSP_OK;
//	}
//
///////////////////////////////////////////////////////////////////////

	//int LoadABIRBinaryFile(UnityAudioEffectState* state, float floatHandle)
	//{
	//	EffectData* data = state->GetEffectData<EffectData>();		

	//	// Cast from float to HANDLE
	//	int intHandle = (int)floatHandle;
	//	HANDLE fileHandle = (HANDLE)intHandle;

	//	// Check that handle is correct
	//	if (fileHandle == INVALID_HANDLE_VALUE)
	//	{
	//		WriteLog(data->channelID, "Error!!! Invalid file handle in ABIR binary file", "");
	//		return -1;
	//	}
	//	
	//	// Get ABIR and check errors
	//	CABIR myABIR;
	//	//CABIR myABIR = ABIR::CreateFrom3dtiHandle(fileHandle);		// <----- THIS IS THE ONLY MISSING THING!!!!
	//	if (myABIR.GetDataLength() != 0)		// TO DO: Improve this error check
	//	{
	//		Spatializer3DTI::spatializer().core->LoadABIR(std::move(myABIR));
	//		WriteLog(data->channelID, "ABIR loaded from binary 3DTI file:", "");
	//		WriteLog(data->channelID, "	Data length: ", myABIR.GetDataLength());
	//		return 1;
	//	}
	//	else
	//	{
	//		WriteLog(data->channelID, "Error!!! could not create ABIR from handle", "");
	//		return -1;
	//	}
	//}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
		EffectData* effectdata = new EffectData;
		memset(effectdata, 0, sizeof(EffectData));
		state->effectdata = effectdata;

		if (!effectdata->pluginCreated)
		{
			InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->parameters);		

			effectdata->pluginCreated = true;
			effectdata->channelID = -1;

			// DEBUG: Start log file
			#ifdef LOG_FILE
			time_t rawtime;
			struct tm * timeinfo;
			char buffer[80];
			time(&rawtime);
			timeinfo = localtime(&rawtime);
			strftime(buffer, 80, "%d-%m-%Y %I:%M:%S", timeinfo);
			std::string str(buffer);
			WriteLog(effectdata->channelID, "***************************************************************************************\nDebug log started at ", str);
			#endif
			//

			// Core initialization
			//effectdata->core = new CCore();
			//WriteLog(effectdata->channelID, "Core initialized", "");

			// Init parameters. Core is not ready until we load the ABIR and set the channel ID...
			//effectdata->coreReady = false;			
			//effectdata->abirReady = false;
			effectdata->environment = Spatializer3DTI::spatializer().core.CreateEnvironment();
			BRIR::CreateFrom3dti(R"(C:\Users\timmb\Documents\dev\3DTI_UnityWrapper\3dti_AudioToolkit\resources\BRIR\3DTI\3DTI_BRIR_large_44100Hz.3dti-brir)", effectdata->environment);
			effectdata->channelReady = false;

			// Set default audio state
			// QUESTIONS: How does this overlaps with explicit call to SetAudioState from C# API? What about buffer size?
			// TO DO: FIX THIS AFTER CORESTATE!!!
			//CAudioState audioState;
			//audioState.SetSampleRate((int)state->samplerate);
			//effectdata->core->SetAudioState(audioState);
			//WriteLog(effectdata->channelID, "Sample rate set to ", state->samplerate);
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
		string channel;
		switch (index)
		{
			//case PARAM_ABIR_FILE_HANDLE:	// Load ABIR binary file (MANDATORY)
			//	if (LoadABIRBinaryFile(state, value))
			//	{
			//		data->abirReady = true;		// Temporary solution before integration of CoreState class					
			//	}
			//	break;

			case PARAM_CHANNEL_ID:	
				data->channelID = (int)value;				
				switch (data->channelID)
				{
					case 0: channel = "W"; data->channelReady = true;  break;
					case 1: channel = "X"; data->channelReady = true;  break;
					case 2: channel = "Y"; data->channelReady = true;  break;
					default: channel = "Unknown channel"; break;
				}
				WriteLog(data->channelID, "Source ID set to: ", channel);
				break;

			default:
				WriteLog(data->channelID, "Unknown float parameter passed from API: ", index);
				return UNITY_AUDIODSP_ERR_UNSUPPORTED;
				break;
		}

		//if (!data->coreReady && data->abirReady && data->channelReady)
		//{
		//	data->coreReady = true;
		//	WriteLog(data->channelID, "Core Ready!", "");
		//}

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
		if (inchannels != 2 || outchannels != 2)
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}

		const int bufferSize = Spatializer3DTI::spatializer().core.GetAudioState().bufferSize;

		if (bufferSize != length)
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}

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



		//EffectData* data = state->GetEffectData<EffectData>();		

		//// Check that I/O formats are right and that the host API supports this feature		
		//if (inchannels != 2 || outchannels != 2 ||
		//	!IsHostCompatible(state))
		//{
		//	WriteLog(data->channelID, "	ERROR! Wrong number of channels or Host is not compatible", "");
		//	memcpy(outbuffer, inbuffer, length * outchannels * sizeof(float));
		//	return UNITY_AUDIODSP_OK;
		//}						
		//
		//// Before doing anything, check that the core is ready
		//// Temporary solution, before integration of CoreState class
		//if (!data->coreReady)
		//	return UNITY_AUDIODSP_OK;		

		//// Transform input buffer
		//// TO DO: Avoid this copy
		//CStereoBuffer<float> inStereoBuffer(length);
		//for (int i = 0; i < length*2; i++)
		//{
		//	inStereoBuffer[i] = inbuffer[i]; 
		//}
		//
		//// Process!!		
		//CStereoBuffer<float> outStereoBuffer(length * 2);	
		//data->core->ProcessEncodedChannelReverb((TBFormatChannel)data->channelID, inStereoBuffer, outStereoBuffer);		

		//// Transform output buffer
		//// TO DO: Avoid this copy
		//int i = 0;
		//for (auto it = outStereoBuffer.begin(); it != outStereoBuffer.end(); it++)
		//{
		//	outbuffer[i++] = *it;
		//}

		//return UNITY_AUDIODSP_OK;
	}
}
