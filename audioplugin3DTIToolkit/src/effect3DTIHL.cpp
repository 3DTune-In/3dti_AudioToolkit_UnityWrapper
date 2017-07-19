/**
*** 3D-Tune-In Toolkit Unity Wrapper for Hearing Loss Simulation***
*
* version 1.9
* Created on: July 2017
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
#define DEBUG_LOG_FILE_HL
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

// Default values for parameters
#define DEFAULT_INIFREQ				62.5
#define DEFAULT_BANDSNUMBER			9
#define DEFAULT_FILTERSPERBAND		3
#define DEFAULT_CALIBRATION_DBSPL	100
#define DEFAULT_RATIO				1
#define DEFAULT_THRESHOLD			0
#define DEFAULT_ATTACK				20
#define DEFAULT_RELEASE				100
#define DEFAULT_AUDIOMETRY			{0,  0, 0, 0, 0, 0, 0, 0, 0}

// Min/max values for parameters
#define MIN_INIFREQ				20	
#define MAX_INIFREQ				20000
#define MAX_BANDSNUMBER			31
#define MAX_CALIBRATION_DBSPL	120
#define MIN_FILTERSPERBAND		1
#define MAX_FILTERSPERBAND		9
#define MIN_HEARINGLOSS			0
#define MAX_HEARINGLOSS			90
#define MAX_RATIO				500
#define MIN_THRESHOLD			-80
#define MAX_ATTACK				2000
#define MAX_RELEASE				2000
//////////////////////////////////////////////////////

	enum
	{
		// Hearing loss levels
		PARAM_HL_0_LEFT,
		PARAM_HL_1_LEFT,
		PARAM_HL_2_LEFT,
		PARAM_HL_3_LEFT,
		PARAM_HL_4_LEFT,
		PARAM_HL_5_LEFT,
		PARAM_HL_6_LEFT,
		PARAM_HL_7_LEFT,
		PARAM_HL_8_LEFT,
		PARAM_HL_0_RIGHT,
		PARAM_HL_1_RIGHT,
		PARAM_HL_2_RIGHT,
		PARAM_HL_3_RIGHT,
		PARAM_HL_4_RIGHT,
		PARAM_HL_5_RIGHT,
		PARAM_HL_6_RIGHT,
		PARAM_HL_7_RIGHT,
		PARAM_HL_8_RIGHT,

		// Switch on/off processing for each ear 
		PARAM_ON_LEFT,
		PARAM_ON_RIGHT,

		// Calibration
		PARAM_CALIBRATION_DBSPL_FOR_0DBFS,

		//// Multiband expander envelope detectors
		PARAM_ATTACK_LEFT,
		PARAM_RELEASE_LEFT,
		PARAM_ATTACK_RIGHT,
		PARAM_RELEASE_RIGHT,

		//// Debug log
		//PARAM_DEBUG_LOG,

		P_NUM
	};

	/////////////////////////////////////////////////////////////////////

    struct EffectData
    {
		CHearingLossSim HL;				
		float parameters[P_NUM];

		// DEBUG LOG
		//bool debugLog = true;
	};

	/////////////////////////////////////////////////////////////////////

	template <class T>
	void WriteLog(UnityAudioEffectState* state, string logtext, const T& value)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		//if (data->debugLog)
		{
			#ifdef DEBUG_LOG_FILE_HL
			ofstream logfile;
			logfile.open("3DTI_HearingLossSimulation_DebugLog.txt", ofstream::out | ofstream::app);
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

		// Hearing loss levels
		RegisterParameter(definition, "HL0L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_0_LEFT, "Hearing loss level for 62.5 Hz band in left ear (dB HL)");
		RegisterParameter(definition, "HL1L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_1_LEFT, "Hearing loss level for 125 Hz band in left ear (dB HL)");
		RegisterParameter(definition, "HL2L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_2_LEFT, "Hearing loss level for 250 Hz band in left ear (dB HL)");
		RegisterParameter(definition, "HL3L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_3_LEFT, "Hearing loss level for 500 Hz band in left ear (dB HL)");
		RegisterParameter(definition, "HL4L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_4_LEFT, "Hearing loss level for 1 KHz band in left ear (dB HL)");
		RegisterParameter(definition, "HL5L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_5_LEFT, "Hearing loss level for 2 KHz band in left ear (dB HL)");
		RegisterParameter(definition, "HL6L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_6_LEFT, "Hearing loss level for 4 KHz band in left ear (dB HL)");
		RegisterParameter(definition, "HL7L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_7_LEFT, "Hearing loss level for 8 KHz band in left ear (dB HL)");
		RegisterParameter(definition, "HL8L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_8_LEFT, "Hearing loss level for 16 KHz band in left ear (dB HL)");
		RegisterParameter(definition, "HL0R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_0_RIGHT, "Hearing loss level for 62.5 Hz band in right ear (dB HL)");
		RegisterParameter(definition, "HL1R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_1_RIGHT, "Hearing loss level for 125 Hz band in right ear (dB HL)");
		RegisterParameter(definition, "HL2R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_2_RIGHT, "Hearing loss level for 250 Hz band in right ear (dB HL)");
		RegisterParameter(definition, "HL3R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_3_RIGHT, "Hearing loss level for 500 Hz band in right ear (dB HL)");
		RegisterParameter(definition, "HL4R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_4_RIGHT, "Hearing loss level for 1 KHz band in right ear (dB HL)");
		RegisterParameter(definition, "HL5R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_5_RIGHT, "Hearing loss level for 2 KHz band in right ear (dB HL)");
		RegisterParameter(definition, "HL6R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_6_RIGHT, "Hearing loss level for 4 KHz band in right ear (dB HL)");
		RegisterParameter(definition, "HL7R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_7_RIGHT, "Hearing loss level for 8 KHz band in right ear (dB HL)");
		RegisterParameter(definition, "HL8R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_HL_8_RIGHT, "Hearing loss level for 16 KHz band in right ear (dB HL)");
		
		// Switch on/off processing for each ear 
		RegisterParameter(definition, "HLONL", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_ON_LEFT, "Switch on hearing loss simulation for left ear");	// Default: OFF
		RegisterParameter(definition, "HLONR", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_ON_RIGHT, "Switch on hearing loss simulation for right ear");	// Default: OFF

		// Calibration
		RegisterParameter(definition, "HLCAL", "dBSPL", 0.0f, MAX_CALIBRATION_DBSPL, DEFAULT_CALIBRATION_DBSPL, 1.0f, 1.0f, PARAM_CALIBRATION_DBSPL_FOR_0DBFS, "Calibration: dBSPL equivalent to 0 dBFS");	

		// Multiband expander envelope detectors
		RegisterParameter(definition, "HLATKL", "ms", 0.0f, MAX_ATTACK, DEFAULT_ATTACK, 1.0f, 1.0f, PARAM_ATTACK_LEFT, "Attack time for left ear envelope detectors (ms)");
		RegisterParameter(definition, "HLRELL", "ms", 0.0f, MAX_RELEASE, DEFAULT_RELEASE, 1.0f, 1.0f, PARAM_RELEASE_LEFT, "Release time for left ear envelope detectors (ms)");
		RegisterParameter(definition, "HLATKR", "ms", 0.0f, MAX_ATTACK, DEFAULT_ATTACK, 1.0f, 1.0f, PARAM_ATTACK_RIGHT, "Attack time for right ear envelope detectors (ms)");
		RegisterParameter(definition, "HLRELR", "ms", 0.0f, MAX_RELEASE, DEFAULT_RELEASE, 1.0f, 1.0f, PARAM_RELEASE_RIGHT, "Release time for right ear envelope detectors (ms)");
				
		// Debug log
		//RegisterParameter(definition, "DebugLogHL", "", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, PARAM_DEBUG_LOG, "Generate debug log for HL");

        return numparams;
    }
	
	/////////////////////////////////////////////////////////////////////

	void WriteLogHeader(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		// EQ:
		WriteLog(state, "CREATE: Multiband expander setup:", "");
		WriteLog(state, "        Initial frequency = ", DEFAULT_INIFREQ);
		WriteLog(state, "        Number of bands = ", DEFAULT_BANDSNUMBER);
		WriteLog(state, "        Filters per band = ", DEFAULT_FILTERSPERBAND);

		// Compressor:
		WriteLog(state, "CREATE: Envelope detectors setup:", "");
		WriteLog(state, "        Sample rate = ", state->samplerate);
		WriteLog(state, "        Attack time = ", DEFAULT_ATTACK);
		WriteLog(state, "        Release time = ", DEFAULT_RELEASE);

		WriteLog(state, "CREATE: Calibration setup:", "");
		WriteLog(state, "        dBSPL for 0 dBFS = ", DEFAULT_CALIBRATION_DBSPL);

		WriteLog(state, "--------------------------------------", "\n");
	}

	/////////////////////////////////////////////////////////////////////	

    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
    {
        EffectData* effectdata = new EffectData;
        //memset(effectdata, 0, sizeof(EffectData));
        state->effectdata = effectdata;
        InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->parameters);
		
		// TO DO: check errors with debugger

		// Multiband expander Setup		
		effectdata->HL.Setup(state->samplerate, DEFAULT_CALIBRATION_DBSPL, DEFAULT_INIFREQ, DEFAULT_BANDSNUMBER, DEFAULT_FILTERSPERBAND);		

		// Initial setup of hearing loss levels
		effectdata->HL.SetFromAudiometry_dBHL(T_ear::BOTH, DEFAULT_AUDIOMETRY);		

		// Setup calibration
		effectdata->HL.SetCalibration(DEFAULT_CALIBRATION_DBSPL);

		// Setup of envelope detectors
		effectdata->HL.SetAttackForAllBands(T_ear::BOTH, DEFAULT_ATTACK);
		effectdata->HL.SetReleaseForAllBands(T_ear::BOTH, DEFAULT_RELEASE);

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

	void SetOneHearingLossLevel(UnityAudioEffectState* state, T_ear ear, int bandIndex, float valueDBHL)
	{
		// Check errors
		if ((bandIndex > DEFAULT_BANDSNUMBER) || (bandIndex < 0))
		{
			WriteLog(state, "SET PARAMETER: ERROR!!!! Attempt to set hearing level for an incorrect band index: ", bandIndex);
			return;
		}
		if ((valueDBHL < MIN_HEARINGLOSS) || (valueDBHL > MAX_HEARINGLOSS))
		{
			WriteLog(state, "SET PARAMETER: ERROR!!!! Attempt to set a wrong dBHL value for hearing loss level: ", valueDBHL);
			return;
		}
		
		// Set hearing loss level
		CHearingLossSim HL = state->GetEffectData<EffectData>()->HL;
		HL.SetHearingLevel_dBHL(ear, bandIndex, valueDBHL);
					
		// Debug log output
		string earStr = "Unknown";
		if (ear == T_ear::LEFT)
			earStr = "Left";
		if (ear == T_ear::RIGHT)
			earStr = "Right";
		#ifndef UNITY_ANDROID
		string logOutput = "SET PARAMETER: Hearing loss of band " + std::to_string(bandIndex) + " for " + earStr + " ear set to " + std::to_string(valueDBHL) + " dBHL";
		#else
		string logOutput = "SET PARAMETER: Hearing loss changed for" + earStr + " ear"; //???
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
			// SET HEARING LEVEL:
			case PARAM_HL_0_LEFT:	SetOneHearingLossLevel(state, T_ear::LEFT, 0, value);	break;
			case PARAM_HL_1_LEFT:	SetOneHearingLossLevel(state, T_ear::LEFT, 1, value);	break;
			case PARAM_HL_2_LEFT:	SetOneHearingLossLevel(state, T_ear::LEFT, 2, value);	break;
			case PARAM_HL_3_LEFT:	SetOneHearingLossLevel(state, T_ear::LEFT, 3, value);	break;
			case PARAM_HL_4_LEFT:	SetOneHearingLossLevel(state, T_ear::LEFT, 4, value);	break;
			case PARAM_HL_5_LEFT:	SetOneHearingLossLevel(state, T_ear::LEFT, 5, value);	break;
			case PARAM_HL_6_LEFT:	SetOneHearingLossLevel(state, T_ear::LEFT, 6, value);	break;
			case PARAM_HL_7_LEFT:	SetOneHearingLossLevel(state, T_ear::LEFT, 7, value);	break;
			case PARAM_HL_8_LEFT:	SetOneHearingLossLevel(state, T_ear::LEFT, 8, value);	break;
			case PARAM_HL_0_RIGHT:	SetOneHearingLossLevel(state, T_ear::RIGHT, 0, value);	break;
			case PARAM_HL_1_RIGHT:	SetOneHearingLossLevel(state, T_ear::RIGHT, 1, value);	break;
			case PARAM_HL_2_RIGHT:	SetOneHearingLossLevel(state, T_ear::RIGHT, 2, value);	break;
			case PARAM_HL_3_RIGHT:	SetOneHearingLossLevel(state, T_ear::RIGHT, 3, value);	break;
			case PARAM_HL_4_RIGHT:	SetOneHearingLossLevel(state, T_ear::RIGHT, 4, value);	break;
			case PARAM_HL_5_RIGHT:	SetOneHearingLossLevel(state, T_ear::RIGHT, 5, value);	break;
			case PARAM_HL_6_RIGHT:	SetOneHearingLossLevel(state, T_ear::RIGHT, 6, value);	break;
			case PARAM_HL_7_RIGHT:	SetOneHearingLossLevel(state, T_ear::RIGHT, 7, value);	break;
			case PARAM_HL_8_RIGHT:	SetOneHearingLossLevel(state, T_ear::RIGHT, 8, value);	break;

			// SWITCH ON/OFF PROCESS FOR EACH EAR:
			case PARAM_ON_LEFT:
				if ((int)value == 0)
					WriteLog(state, "SET PARAMETER: Left ear hearing loss simulation switched OFF", "");
				if ((int)value == 1)
					WriteLog(state, "SET PARAMETER: Left ear hearing loss simulation switched ON", "");
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off left ear hearing loss simulation with non boolean value ", value);
				break;

			case PARAM_ON_RIGHT:
				if ((int)value == 0)
					WriteLog(state, "SET PARAMETER: Right ear hearing loss simulation switched OFF", "");
				if ((int)value == 1)
					WriteLog(state, "SET PARAMETER: Right ear hearing loss simulation switched ON", "");
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off right ear hearing loss simulation with non boolean value ", value);
				break;

			// CALIBRATION:
			case PARAM_CALIBRATION_DBSPL_FOR_0DBFS:
				data->HL.SetCalibration(value);
				WriteLog(state, "SET PARAMETER: Calibration (dBSPL for 0dBFS) set to: ", value);
				break;

			// ENVELOPE DETECTORS:
			case PARAM_ATTACK_LEFT:
				data->HL.SetAttackForAllBands(T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: Attack time (ms) for Left envelope detectors set to: ", value);				
				break;

			case PARAM_ATTACK_RIGHT:
				data->HL.SetAttackForAllBands(T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: Attack time (ms) for Right envelope detectors set to: ", value);
				break;

			case PARAM_RELEASE_LEFT:
				data->HL.SetReleaseForAllBands(T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: Release time (ms) for Left envelope detectors set to: ", value);
				break;

			case PARAM_RELEASE_RIGHT:
				data->HL.SetReleaseForAllBands(T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: Release time (ms) for Right envelope detectors set to: ", value);
				break;

			//case PARAM_DEBUG_LOG:
			//	if (value != 0.0f)
			//	{
			//		data->debugLog = true;
			//		WriteLogHeader(state);
			//	}
			//	else
			//		data->debugLog = false;
			//	break;

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

		// Transform input buffer into two Mono buffers
		CMonoBuffer<float> leftInputBuffer(length);		
		CMonoBuffer<float> rightInputBuffer(length);
		int monoIndex = 0;
		for (int i = 0; i < length*2; i+=2)
		{
			leftInputBuffer[monoIndex] = inbuffer[i]; 
			rightInputBuffer[monoIndex] = inbuffer[i+1];
			monoIndex++;
		}

		// Process!!
		CMonoBuffer<float> leftOutputBuffer(length);
		CMonoBuffer<float> rightOutputBuffer(length);

		if (data->parameters[PARAM_ON_LEFT])
			data->HL.Process(T_ear::LEFT, leftInputBuffer, leftOutputBuffer);
		else
			leftOutputBuffer = leftInputBuffer;

		if (data->parameters[PARAM_ON_RIGHT])
			data->HL.Process(T_ear::RIGHT, rightInputBuffer, rightOutputBuffer);
		else
			rightOutputBuffer = rightInputBuffer;


		// Transform output buffers			
		int stereoIndex = 0;
		for (int i = 0; i < length; i++)
		{
			outbuffer[stereoIndex] = leftOutputBuffer[i];
			stereoIndex++;
			outbuffer[stereoIndex] = rightOutputBuffer[i];
			stereoIndex++;
		}		

        return UNITY_AUDIODSP_OK;
    }
}
