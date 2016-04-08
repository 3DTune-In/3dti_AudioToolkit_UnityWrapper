#include "AudioPluginUtil.h"

// Tell the 3DTI Toolkit Core that we will be using the Unity axis convention!
// WARNING: This define must be done before including Core.h!
#define AXIS_CONVENTION UNITY

#include "../../3DTI_Toolkit_Core/Core.h"
#include "HRTFArray.h"

//#define LOG_FILE

#define distanceScale 1.0f

// Includes for reading HRTF data and logging dor debug
#include <fstream>
#include <iostream>
#include <time.h>

using namespace std;

// DEBUG LOG FILE
//#define LOG_FILE
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
		P_DUMMY,
		P_NUM
	};

/////////////////////////////////////////////////////////////////////
	
	CCore* core;
	struct EffectData
	{
		float p[P_NUM];		
		std::shared_ptr<CSingleSourceDSP> source;
		//CCore core;
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
		/////////////////////////////////////////////////////////////////////

		// 3DTI: All parameters in the example were used only for distance attenuation, which is implicit in Core

		int numparams = P_NUM;
		definition.paramdefs = new UnityAudioParameterDefinition[numparams];
		RegisterParameter(definition, "Dummy parameter", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, P_DUMMY, "Dummy parameter");
		definition.flags |= UnityAudioEffectDefinitionFlags_IsSpatializer;
		return numparams;
	}

/////////////////////////////////////////////////////////////////////

	static UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK DistanceAttenuationCallback(UnityAudioEffectState* state, float distanceIn, float attenuationIn, float* attenuationOut)
	{
		// 3DTI: distance attenuation is included in ProcessAnechoic
		//*attenuationOut = attenuationIn*(1.0f/distanceIn);
		*attenuationOut = attenuationIn;
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	CHRTF SetupHRTF()
	{
		// From TXT File:
		//CHRTF readHRTF;
		//if (readHRTF.ReadFromFile("HRTF.txt") < 0)
		//	WriteLog("	Could not open HRTF.txt file", "");
		//else
		//	WriteLog("	File HRTF.txt read!", "");

		// From hardcoded array:
		CHRTF readHRTF;
		if (readHRTF.ReadFromArray(HRTFArray) < 0)
			WriteLog("	Could not read HRTF array. ", "");
		else
			WriteLog("	HRTF array read!", "");

		return readHRTF;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
		EffectData* effectdata = new EffectData;
		memset(effectdata, 0, sizeof(EffectData));
		state->effectdata = effectdata;
		if (IsHostCompatible(state))
			state->spatializerdata->distanceattenuationcallback = DistanceAttenuationCallback;	// TO DO: check if we can remove this
		InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->p);			// TO DO: and this		

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
		//CCore core = effectdata->core;		
		core = new CCore();
		WriteLog("Core initialized", "");

		// Set audio state
		CAudioState audioState;
		audioState.SetSampleRate((int) state->samplerate);
		core->SetAudioState(audioState);		
		WriteLog("Sample rate set to ", state->samplerate);

		// Set listener transform	
		// WARNING: the source and listener matrix passed in CreateCallback seem to be always ZERO
		float L[16];					// Inverted 4x4 listener matrix, as provided by Unity
		for (int i = 0; i < 16; i++)
			L[i] = state->spatializerdata->listenermatrix[i];
		float listenerpos_x = -(L[0] * L[12] + L[1] * L[13] + L[2] * L[14]);	// From Unity documentation
		float listenerpos_y = -(L[4] * L[12] + L[5] * L[13] + L[6] * L[14]);	// From Unity documentation
		float listenerpos_z = -(L[8] * L[12] + L[9] * L[13] + L[10] * L[14]);	// From Unity documentation
		CTransform listenerTransform;
		listenerTransform.SetOrientation(CQuaternion::UNIT);
		//listenerTransform.SetPosition(CVector3(0.0f, 0.0f, 0.0f));
		listenerTransform.SetPosition(CVector3(listenerpos_x, listenerpos_y, listenerpos_z));
		core->SetListenerTransform(listenerTransform);
		//WriteLog("Listener transform set to fixed position: ", CVector3(0.0f, 0.0f, 0.0f));
		WriteLog("Listener transform set to: ", listenerTransform.GetPosition());

		// Setup listener HRTF	
		WriteLog("Setting listener HRTF...", "");
		CHRTF listenerHRTF = SetupHRTF();
		core->LoadHRTF(std::move(listenerHRTF));
		WriteLog("	Listener HRTF set.", "");		

		// TO DO: Set listener head radius
		// TO DO: Setup room

		// Create source and set transform		
		// WARNING: the source and listener matrix passed in CreateCallback seem to be always ZERO
		effectdata->source = core->CreateSingleSourceDSP();
		CTransform sourceTransform;
		sourceTransform.SetPosition(CVector3(state->spatializerdata->sourcematrix[12]*distanceScale, 
											 state->spatializerdata->sourcematrix[13]*distanceScale, 
											 state->spatializerdata->sourcematrix[14]*distanceScale));	
		effectdata->source->SetSourceTransform(sourceTransform);			
		WriteLog("Source position set to ", sourceTransform.GetPosition());

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
		data->p[index] = value;
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char *valuestr)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		if (index >= P_NUM)
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		if (value != NULL)
			*value = data->p[index];
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

		// Read spatializer data
		EffectData* data = state->GetEffectData<EffectData>();
		float* s = state->spatializerdata->sourcematrix;

		// Set source position (we don't care about orientation)		
		CTransform sourceTransform;
		sourceTransform.SetPosition(CVector3(s[12]*distanceScale, s[13]*distanceScale, s[14]*distanceScale));
		data->source->SetSourceTransform(sourceTransform);		

		// Set listener position		
		float L[16];					// Inverted 4x4 listener matrix, as provided by Unity
		for (int i = 0; i < 16; i++)
			L[i] = state->spatializerdata->listenermatrix[i];
		//float listenerpos_x = -(L[0] * L[12] + L[1] * L[13] + L[2] * L[14]);	// From Unity documentation, if camera is rotated wrt listener
		//float listenerpos_y = -(L[4] * L[12] + L[5] * L[13] + L[6] * L[14]);	// From Unity documentation, if camera is rotated wrt listener
		//float listenerpos_z = -(L[8] * L[12] + L[9] * L[13] + L[10] * L[14]);	// From Unity documentation, if camera is rotated wrt listener
		float listenerpos_x = -L[12];	// If camera is not rotated
		float listenerpos_y = -L[13];	// If camera is not rotated
		float listenerpos_z = -L[14];	// If camera is not rotated
		CTransform listenerTransform;		
		listenerTransform.SetPosition(CVector3(listenerpos_x, listenerpos_y, listenerpos_z));
		//WriteLog("Listener position = ", listenerTransform.GetPosition());
		
		// Set listener rotation
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
		
		// Transform input buffer
		// TO DO: Avoid this copy!!!!!!		
		CMonoBuffer<float> inMonoBuffer(length);
		for (int i = 0; i < length; i++)
		{
			inMonoBuffer[i] = inbuffer[i * 2]; // We take only the left channel
		}

		// Process!!
		CStereoBuffer<float> outStereoBuffer(length * 2);
		data->source->SetInterpolation(3);
		data->source->UpdateBuffer(inMonoBuffer);
		data->source->ProcessAnechoic(outStereoBuffer);
		//data->source->ProcessAnechoic(inMonoBuffer, outStereoBuffer);				

		// Transform output buffer
		// TO DO: Avoid this copy!!!
		int i = 0;
		for (auto it = outStereoBuffer.begin(); it != outStereoBuffer.end(); it++)
		{
			outbuffer[i++] = *it;
		}

		return UNITY_AUDIODSP_OK;
	}
}
