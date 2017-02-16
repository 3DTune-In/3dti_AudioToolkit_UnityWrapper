/**
*** 3D-Tune-In Toolkit Unity Wrapper for Hearing Loss Simulation***
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

#include <HAHLSimulation/HearingLossSim.h>

// Includes for debug logging
#include <fstream>
#include <iostream>
#include <string>
using namespace std;

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

namespace HLSimulation3DTI
{

//////////////////////////////////////////////////////
#define EAR_RIGHT 0
#define EAR_LEFT 1

// Default values for parameters
#define DEFAULT_INIFREQ			62.5
#define DEFAULT_BANDSNUMBER		9
#define DEFAULT_OCTAVEBANDSTEP	1
#define DEFAULT_QBPF			1.4142
#define DEFAULT_KNEE			0
#define DEFAULT_RATIO			1
#define DEFAULT_THRESHOLD		0
#define DEFAULT_ATTACK			20
#define DEFAULT_RELEASE			100
#define DEFAULT_BAND_GAINS		{-7,  -7, -12, -15, -22, -25, -25, -25, -25}

// Min/max values for parameters
#define MIN_INIFREQ			20	
#define MAX_INIFREQ			20000
#define MAX_BANDSNUMBER		31
#define MIN_OCTAVEBANDSTEP	0
#define MAX_OCTAVEBANDSTEP	FLT_MAX
#define MIN_QBPF			0
#define MAX_QBPF			FLT_MAX
#define MIN_BANDGAIN_DB		-75
#define MAX_BANDGAIN_DB		0
#define MIN_KNEE			0
#define MAX_KNEE			20
#define MAX_RATIO			50
#define MIN_THRESHOLD		-80
#define MAX_ATTACK			200
#define MAX_RELEASE			200
//////////////////////////////////////////////////////

	enum
	{
		// EQ Band gains
		PARAM_BAND_L_0_DB,
		PARAM_BAND_L_1_DB,
		PARAM_BAND_L_2_DB,
		PARAM_BAND_L_3_DB,
		PARAM_BAND_L_4_DB,
		PARAM_BAND_L_5_DB,
		PARAM_BAND_L_6_DB,
		PARAM_BAND_L_7_DB,
		PARAM_BAND_L_8_DB,
		PARAM_BAND_R_0_DB,
		PARAM_BAND_R_1_DB,
		PARAM_BAND_R_2_DB,
		PARAM_BAND_R_3_DB,
		PARAM_BAND_R_4_DB,
		PARAM_BAND_R_5_DB,
		PARAM_BAND_R_6_DB,
		PARAM_BAND_R_7_DB,
		PARAM_BAND_R_8_DB,

		// Switch on/off processing for each ear and EQ-Compressor chain
		PARAM_LEFT_EQ_ON,
		PARAM_RIGHT_EQ_ON,
		PARAM_LEFT_COMPRESSOR_ON,
		PARAM_RIGHT_COMPRESSOR_ON,
		PARAM_COMPRESSOR_FIRST,

		// Compressor
		PARAM_COMP_LEFT_KNEE,
		PARAM_COMP_LEFT_RATIO,
		PARAM_COMP_LEFT_THRESHOLD,
		PARAM_COMP_RIGHT_KNEE,
		PARAM_COMP_RIGHT_RATIO,
		PARAM_COMP_RIGHT_THRESHOLD,

		// Envelope detector
		PARAM_COMP_LEFT_ATTACK,
		PARAM_COMP_LEFT_RELEASE,
		PARAM_COMP_RIGHT_ATTACK,
		PARAM_COMP_RIGHT_RELEASE,		

		P_NUM
	};

	/////////////////////////////////////////////////////////////////////

    struct EffectData
    {
		CHearingLossSim HL;				
		float parameters[P_NUM];
	};

	/////////////////////////////////////////////////////////////////////

	template <class T>
	void WriteLog(UnityAudioEffectState* state, string logtext, const T& value)
	{
		#ifdef DEBUG_LOG_FILE
			ofstream logfile;			
			logfile.open("debugHL.txt", ofstream::out | ofstream::app);
			logfile << logtext << value << endl;
			logfile.close();
		#endif

		#ifdef DEBUG_LOG_CAT						
			std::ostringstream os;
			os << logtext << value;
			string fulltext = os.str();			
			__android_log_print(ANDROID_LOG_DEBUG, "3DTIHLSIMULATION", fulltext.c_str());
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

		// EQ gain for each band
		RegisterParameter(definition, "EQL0", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_L_0_DB, "EQ Left 62.5 Hz band gain (dB)");
		RegisterParameter(definition, "EQL1", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_L_1_DB, "EQ Left 125 Hz band gain (dB)");
		RegisterParameter(definition, "EQL2", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_L_2_DB, "EQ Left 250 Hz band gain (dB)");
		RegisterParameter(definition, "EQL3", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_L_3_DB, "EQ Left 500 Hz band gain (dB)");
		RegisterParameter(definition, "EQL4", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_L_4_DB, "EQ Left 1 KHz band gain (dB)");
		RegisterParameter(definition, "EQL5", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_L_5_DB, "EQ Left 2 KHz band gain (dB)");
		RegisterParameter(definition, "EQL6", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_L_6_DB, "EQ Left 4 KHz band gain (dB)");
		RegisterParameter(definition, "EQL7", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_L_7_DB, "EQ Left 8 KHz band gain (dB)");
		RegisterParameter(definition, "EQL8", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_L_8_DB, "EQ Left 16 KHz band gain (dB)");
		RegisterParameter(definition, "EQR0", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_R_0_DB, "EQ Right 62.5 Hz band gain (dB)");
		RegisterParameter(definition, "EQR1", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_R_1_DB, "EQ Right 125 Hz band gain (dB)");
		RegisterParameter(definition, "EQR2", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_R_2_DB, "EQ Right 250 Hz band gain (dB)");
		RegisterParameter(definition, "EQR3", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_R_3_DB, "EQ Right 500 Hz band gain (dB)");
		RegisterParameter(definition, "EQR4", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_R_4_DB, "EQ Right 1 KHz band gain (dB)");
		RegisterParameter(definition, "EQR5", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_R_5_DB, "EQ Right 2 KHz band gain (dB)");
		RegisterParameter(definition, "EQR6", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_R_6_DB, "EQ Right 4 KHz band gain (dB)");
		RegisterParameter(definition, "EQR7", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_R_7_DB, "EQ Right 8 KHz band gain (dB)");
		RegisterParameter(definition, "EQR8", "", MIN_BANDGAIN_DB, MAX_BANDGAIN_DB, 0.0f, 1.0f, 1.0f, PARAM_BAND_R_8_DB, "EQ Right 16 KHz band gain (dB)");
		
		// Switch on/off process for each ear and EQ-compressor chain
		RegisterParameter(definition, "EQLeftOn", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_LEFT_EQ_ON, "Switch on EQ for left ear");							// Default: ON
		RegisterParameter(definition, "EQRightOn", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_RIGHT_EQ_ON, "Switch on EQ for right ear");						// Default: ON
		RegisterParameter(definition, "CompLeftOn", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_LEFT_COMPRESSOR_ON, "Switch on Compressor for left ear");		// Default: OFF
		RegisterParameter(definition, "CompRightOn", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_RIGHT_COMPRESSOR_ON, "Switch on Compressor for right ear");	// Default: OFF
		RegisterParameter(definition, "CompFirst", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_COMPRESSOR_FIRST, "Process Compressor before EQ");	// Default: Compressor First

		// Compressor
		RegisterParameter(definition, "LeftKnee", "", MIN_KNEE, MAX_KNEE, DEFAULT_KNEE, 1.0f, 1.0f, PARAM_COMP_LEFT_KNEE, "Left compressor: Knee");	
		RegisterParameter(definition, "LeftRatio", "", 1.0f, MAX_RATIO, DEFAULT_RATIO, 1.0f, 1.0f, PARAM_COMP_LEFT_RATIO, "Left compressor: Ratio");
		RegisterParameter(definition, "LeftThreshold", "dB", MIN_THRESHOLD, 0.0f, DEFAULT_THRESHOLD, 1.0f, 1.0f, PARAM_COMP_LEFT_THRESHOLD, "Left compressor: Threshold");
		RegisterParameter(definition, "RightKnee", "", MIN_KNEE, MAX_KNEE, DEFAULT_KNEE, 1.0f, 1.0f, PARAM_COMP_RIGHT_KNEE, "Right compressor: Knee");
		RegisterParameter(definition, "RightRatio", "", 1.0f, MAX_RATIO, DEFAULT_RATIO, 1.0f, 1.0f, PARAM_COMP_RIGHT_RATIO, "Right compressor: Ratio");
		RegisterParameter(definition, "RightThreshold", "dB", MIN_THRESHOLD, 0.0f, DEFAULT_THRESHOLD, 1.0f, 1.0f, PARAM_COMP_RIGHT_THRESHOLD, "Right compressor: Threshold");

		// Envelope detector
		RegisterParameter(definition, "LeftAttack", "ms", 0.0f, MAX_ATTACK, DEFAULT_ATTACK, 1.0f, 1.0f, PARAM_COMP_LEFT_ATTACK, "Left compressor: Attack");
		RegisterParameter(definition, "LeftRelease", "ms", 0.0f, MAX_RELEASE, DEFAULT_RELEASE, 1.0f, 1.0f, PARAM_COMP_LEFT_RELEASE, "Left compressor: Release");
		RegisterParameter(definition, "RightAttack", "ms", 0.0f, MAX_ATTACK, DEFAULT_ATTACK, 1.0f, 1.0f, PARAM_COMP_RIGHT_ATTACK, "Right compressor: Attack");
		RegisterParameter(definition, "RightRelease", "ms", 0.0f, MAX_RELEASE, DEFAULT_RELEASE, 1.0f, 1.0f, PARAM_COMP_RIGHT_RELEASE, "Right compressor: Release");
		
        return numparams;
    }

	/////////////////////////////////////////////////////////////////////	

    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
    {
        EffectData* effectdata = new EffectData;
        memset(effectdata, 0, sizeof(EffectData));
        state->effectdata = effectdata;
        InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->parameters);
		
		// TO DO: check errors with debugger

		// EQ Setup		
		effectdata->HL.Setup(DEFAULT_INIFREQ, DEFAULT_BANDSNUMBER, DEFAULT_OCTAVEBANDSTEP, DEFAULT_QBPF);
		WriteLog(state, "CREATE: EQ setup:", "");
		WriteLog(state, "        Initial frequency = ", DEFAULT_INIFREQ);
		WriteLog(state, "        Number of bands = ", DEFAULT_BANDSNUMBER);
		WriteLog(state, "        Octave step = 1/", DEFAULT_OCTAVEBANDSTEP);
		WriteLog(state, "        Q factor of BPFs = ", DEFAULT_QBPF);

		// Initial setup of band gains
		effectdata->HL.SetGains_dB(DEFAULT_BAND_GAINS, EAR_LEFT);
		effectdata->HL.SetGains_dB(DEFAULT_BAND_GAINS, EAR_RIGHT);
		
		// Setup of Compressor
		effectdata->HL.Compr_L.Setup(state->samplerate);
		effectdata->HL.Compr_R.Setup(state->samplerate);
		WriteLog(state, "CREATE: Compressor setup with sample rate ", state->samplerate);

		WriteLog(state, "CREATE: HL Simulation plugin created", "");		

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

	void SetOneBandGain(UnityAudioEffectState* state, bool ear, int bandIndex, float valueDB)
	{
		// Check errors
		if ((bandIndex > DEFAULT_BANDSNUMBER) || (bandIndex < 0))
		{
			WriteLog(state, "SET PARAMETER: ERROR!!!! Attempt to set gain for an incorrect band index: ", bandIndex);
			return;
		}
		if ((valueDB < MIN_BANDGAIN_DB) || (valueDB > MAX_BANDGAIN_DB))
		{
			WriteLog(state, "SET PARAMETER: ERROR!!!! Attempt to set a wrong dB value for band gain: ", valueDB);
			return;
		}
		
		// Set band gain
		CHearingLossSim HL = state->GetEffectData<EffectData>()->HL;
		HL.SetBandGain_dB(bandIndex, valueDB, ear);
					
		// Debug log output
		string earStr = "Unknown";
		if (ear == EAR_LEFT)
			earStr = "Left";
		else
			earStr = "Right";
#ifndef UNITY_ANDROID
		string logOutput = "SET PARAMETER: Gain of band " + std::to_string(bandIndex) + " for " + earStr + " ear set to " + std::to_string(valueDB) + " dB";
#else
		string logOutput = "SET PARAMETER: Gain of EQ band changed for" + earStr + " ear";
#endif
		WriteLog(state, logOutput, "");		
	}

	/////////////////////////////////////////////////////////////////////

    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
    {
        EffectData* data = state->GetEffectData<EffectData>();
        if (index >= P_NUM)
            return UNITY_AUDIODSP_ERR_UNSUPPORTED;
        data->parameters[index] = value;

		// Process command sent by C# API
		// TO DO: Check errors with debugger, incorrect values...
		switch (index)
		{		
			// SET EQ BAND GAIN:

			case PARAM_BAND_L_0_DB:	SetOneBandGain(state, EAR_LEFT, 0, value);	break;
			case PARAM_BAND_L_1_DB:	SetOneBandGain(state, EAR_LEFT, 1, value);	break;
			case PARAM_BAND_L_2_DB:	SetOneBandGain(state, EAR_LEFT, 2, value);	break;
			case PARAM_BAND_L_3_DB:	SetOneBandGain(state, EAR_LEFT, 3, value);	break;
			case PARAM_BAND_L_4_DB:	SetOneBandGain(state, EAR_LEFT, 4, value);	break;
			case PARAM_BAND_L_5_DB:	SetOneBandGain(state, EAR_LEFT, 5, value);	break;
			case PARAM_BAND_L_6_DB:	SetOneBandGain(state, EAR_LEFT, 6, value);	break;
			case PARAM_BAND_L_7_DB:	SetOneBandGain(state, EAR_LEFT, 7, value);	break;
			case PARAM_BAND_L_8_DB:	SetOneBandGain(state, EAR_LEFT, 8, value);	break;
			case PARAM_BAND_R_0_DB:	SetOneBandGain(state, EAR_RIGHT, 0, value); break;
			case PARAM_BAND_R_1_DB:	SetOneBandGain(state, EAR_RIGHT, 1, value); break;
			case PARAM_BAND_R_2_DB:	SetOneBandGain(state, EAR_RIGHT, 2, value); break;
			case PARAM_BAND_R_3_DB:	SetOneBandGain(state, EAR_RIGHT, 3, value); break;
			case PARAM_BAND_R_4_DB:	SetOneBandGain(state, EAR_RIGHT, 4, value); break;
			case PARAM_BAND_R_5_DB:	SetOneBandGain(state, EAR_RIGHT, 5, value); break;
			case PARAM_BAND_R_6_DB:	SetOneBandGain(state, EAR_RIGHT, 6, value); break;
			case PARAM_BAND_R_7_DB:	SetOneBandGain(state, EAR_RIGHT, 7, value); break;
			case PARAM_BAND_R_8_DB:	SetOneBandGain(state, EAR_RIGHT, 8, value); break;
	
			// SWITCH ON/OFF PROCESS FOR EACH EAR:

			case PARAM_LEFT_EQ_ON:
				if ((int)value == 0)
					WriteLog(state, "SET PARAMETER: Left ear EQ process switched OFF", "");
				if ((int)value == 1)
					WriteLog(state, "SET PARAMETER: Left ear EQ process switched ON", "");
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Switching on/off left ear EQ with non boolean value ", value);
				break;

			case PARAM_RIGHT_EQ_ON:
				if ((int)value == 0)
					WriteLog(state, "SET PARAMETER: Right ear EQ process switched OFF", "");
				if ((int)value == 1)
					WriteLog(state, "SET PARAMETER: Right ear EQ process switched ON", "");
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Switching on/off right ear EQ with non boolean value ", value);
				break;

			case PARAM_LEFT_COMPRESSOR_ON:
				if ((int)value == 0)
					WriteLog(state, "SET PARAMETER: Left ear Compression process switched OFF", "");
				if ((int)value == 1)
					WriteLog(state, "SET PARAMETER: Left ear Compression process switched ON", "");
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Switching on/off left ear Compression with non boolean value ", value);
				break;

			case PARAM_RIGHT_COMPRESSOR_ON:
				if ((int)value == 0)
					WriteLog(state, "SET PARAMETER: Right ear Compression process switched OFF", "");
				if ((int)value == 1)
					WriteLog(state, "SET PARAMETER: Right ear Compression process switched ON", "");
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Switching on/off right ear Compression with non boolean value ", value);
				break;

			// SET EQ-COMPRESSOR CHAIN:

			case PARAM_COMPRESSOR_FIRST:
				if ((int)value == 0)
					WriteLog(state, "SET PARAMETER: Compressor will be processed AFTER EQ: ", "EQ->Compressor");
				if ((int)value == 1)
					WriteLog(state, "SET PARAMETER: Compressor will be processed BEFORE EQ: ", "Compressor->EQ");
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Specifying Compressor-EQ chain with non boolean value ", value);
				break;

			// COMPRESSOR:

			case PARAM_COMP_LEFT_KNEE:
				WriteLog(state, "SET PARAMETER: Knee for Left compressor set to: ", value);
				data->HL.Compr_L.knee = value;				
				break;

			case PARAM_COMP_LEFT_RATIO:
				WriteLog(state, "SET PARAMETER: Ratio for Left compressor set to: ", value);
				data->HL.Compr_L.ratio = value;
				break;

			case PARAM_COMP_LEFT_THRESHOLD:
				WriteLog(state, "SET PARAMETER: Threshold for Left compressor set to: ", value);
				data->HL.Compr_L.threshold = value;
				break;

			case PARAM_COMP_RIGHT_KNEE:
				WriteLog(state, "SET PARAMETER: Knee for Right compressor set to: ", value);
				data->HL.Compr_R.knee = value;
				break;

			case PARAM_COMP_RIGHT_RATIO:
				WriteLog(state, "SET PARAMETER: Ratio for Right compressor set to: ", value);
				data->HL.Compr_R.ratio = value;
				break;

			case PARAM_COMP_RIGHT_THRESHOLD:
				WriteLog(state, "SET PARAMETER: Threshold for Right compressor set to: ", value);
				data->HL.Compr_R.threshold = value;
				break;

			// ENVELOPE DETECTOR:

			case PARAM_COMP_LEFT_ATTACK:
				WriteLog(state, "SET PARAMETER: Attack for Left compressor set to: ", value);
				data->HL.Compr_L.envDetector.SetAttackTime(value);
				break;

			case PARAM_COMP_LEFT_RELEASE:
				WriteLog(state, "SET PARAMETER: Release for Left compressor set to: ", value);
				data->HL.Compr_L.envDetector.SetReleaseTime(value);
				break;

			case PARAM_COMP_RIGHT_ATTACK:
				WriteLog(state, "SET PARAMETER: Attack for Right compressor set to: ", value);
				data->HL.Compr_R.envDetector.SetAttackTime(value);
				break;

			case PARAM_COMP_RIGHT_RELEASE:
				WriteLog(state, "SET PARAMETER: Release for Right compressor set to: ", value);
				data->HL.Compr_R.envDetector.SetReleaseTime(value);
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
	
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
    {
        // Check that I/O formats are right and that the host API supports this feature
        if (inchannels != 2 || outchannels != 2 || !IsHostCompatible(state))
        {
			WriteLog(state, "PROCESS: ERROR!!!! Wrong number of channels or Host is not compatible:", "");
			WriteLog(state, "         Input channels = ", inchannels);
			WriteLog(state, "         Output channels = ", outchannels);
			WriteLog(state, "         Host compatible = ", IsHostCompatible(state));			
			WriteLog(state, "         Buffer length = ", length);
            memcpy(outbuffer, inbuffer, length * outchannels * sizeof(float));
            return UNITY_AUDIODSP_OK;
        }

		EffectData* data = state->GetEffectData<EffectData>();

		// Transform input buffer
		CStereoBuffer<float> inStereoBuffer(length * 2);		
		for (int i = 0; i < length*2; i++)
		{
			inStereoBuffer[i] = inbuffer[i]; 
		}

		// Process!!
		CStereoBuffer<float> outStereoBuffer(length * 2);
		data->HL.Process(inStereoBuffer, outStereoBuffer, data->parameters[PARAM_LEFT_EQ_ON], data->parameters[PARAM_RIGHT_EQ_ON], 
						data->parameters[PARAM_COMPRESSOR_FIRST], data->parameters[PARAM_LEFT_COMPRESSOR_ON], data->parameters[PARAM_RIGHT_COMPRESSOR_ON]);

		// Transform output buffer			
		int i = 0;
		for (auto it = outStereoBuffer.begin(); it != outStereoBuffer.end(); it++)
		{
			outbuffer[i++] = *it;
		}

		// DUMMY PROCESS (DEBUG):
		//for (unsigned int n = 0; n < length; n++)
		//{
		//	for (int i = 0; i < outchannels; i++)
		//	{
		//		outbuffer[n * outchannels + i] = inbuffer[n * outchannels + i];
		//	}
		//}

        return UNITY_AUDIODSP_OK;
    }
}
