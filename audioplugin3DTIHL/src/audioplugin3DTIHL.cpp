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

#include <HAHLSimulation/HearingLossSim.h>

// Includes for debug logging
#include <fstream>
#include <iostream>

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

// Min/max values for parameters
#define MIN_INIFREQ			20	
#define MAX_INIFREQ			20000
#define MAX_BANDSNUMBER		31
#define MIN_OCTAVEBANDSTEP	0
#define MAX_OCTAVEBANDSTEP	FLT_MAX
#define MIN_QBPF			0
#define MAX_QBPF			FLT_MAX
#define MIN_BANDGAIN		0
#define MAX_BANDGAIN		1
#define MIN_KNEE			0
#define MAX_KNEE			20
#define MAX_RATIO			50
#define MIN_THRESHOLD		-80
#define MAX_ATTACK			200
#define MAX_RELEASE			200
//////////////////////////////////////////////////////

	enum
	{
		// EQ Setup
		PARAM_INIFREQ,
		PARAM_BANDSNUMBER,
		PARAM_OCTAVEBANDSTEP,
		PARAM_QBPF,

		// Set one EQ band for one ear
		PARAM_SETBAND_EAR,
		PARAM_SETBAND_NUMBER,
		PARAM_SETBAND_GAIN,

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

	enum TSetBandGainState
	{
		SETBG_WAITING,
		SETBG_EARSELECTED,
		SETBG_BANDSELECTED
	};

	/////////////////////////////////////////////////////////////////////

    struct EffectData
    {
		CHearingLossSim HL;				
		bool hlReady;
		TSetBandGainState setBandGainState;
		int numberOfBandsSet;
		float parameters[P_NUM];
	};

	/////////////////////////////////////////////////////////////////////

	template <class T>
	void WriteLog(UnityAudioEffectState* state, string logtext, const T& value)
	{
		#ifdef DEBUG_LOG_FILE
			ofstream logfile;
			int sourceid = state->GetEffectData<EffectData>()->sourceID;
			logfile.open("debugHL.txt", ofstream::out | ofstream::app);
			logfile << sourceid << ": " << logtext << value << endl;
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

		// EQ Setup
		RegisterParameter(definition, "IniFreq", "Hz", MIN_INIFREQ, MAX_INIFREQ, DEFAULT_INIFREQ, 1.0f, 1.0f, PARAM_INIFREQ, "EQ Initial frequency");
		RegisterParameter(definition, "BandsNumber", "", 0.0f, MAX_BANDSNUMBER, DEFAULT_BANDSNUMBER, 1.0f, 1.0f, PARAM_BANDSNUMBER, "EQ Number of bands");
		RegisterParameter(definition, "OctaveBandStep", "", MIN_OCTAVEBANDSTEP, MAX_OCTAVEBANDSTEP, DEFAULT_OCTAVEBANDSTEP, 1.0f, 1.0f, PARAM_OCTAVEBANDSTEP, "EQ Octave band step");
		RegisterParameter(definition, "QBPF", "", MIN_QBPF, MAX_QBPF, DEFAULT_QBPF, 1.0f, 1.0f, PARAM_QBPF, "EQ Q of band pass filters");

		// EQ set gain for each band
		RegisterParameter(definition, "SetBandEar", "", -1.0f, 1.0f, -1.0f, 1.0f, 1.0f, PARAM_SETBAND_EAR, "Set band gain: ear");
		RegisterParameter(definition, "SetBandNumber", "", -1.0f, MAX_BANDSNUMBER, -1.0f, 1.0f, 1.0f, PARAM_SETBAND_NUMBER, "Set band gain: band number");
		RegisterParameter(definition, "SetBandGain", "dB", MIN_BANDGAIN, MAX_BANDGAIN, 1.0f, 1.0f, 1.0f, PARAM_SETBAND_GAIN, "Set band gain: gain");		

		// Switch on/off process for each ear and EQ-compressor chain
		RegisterParameter(definition, "EQLeftOn", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_LEFT_EQ_ON, "Switch on EQ for left ear");							// Default: OFF
		RegisterParameter(definition, "EQRightOn", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_RIGHT_EQ_ON, "Switch on EQ for right ear");						// Default: OFF
		RegisterParameter(definition, "CompLeftOn", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_LEFT_COMPRESSOR_ON, "Switch on Compressor for left ear");		// Default: OFF
		RegisterParameter(definition, "CompRightOn", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_RIGHT_COMPRESSOR_ON, "Switch on Compressor for right ear");	// Default: OFF
		RegisterParameter(definition, "CompFirst", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_COMPRESSOR_FIRST, "Process Compressor before EQ");	// Default: Compressor First

		// Compressor
		RegisterParameter(definition, "LeftKnee", "", MIN_KNEE, MAX_KNEE, DEFAULT_KNEE, 1.0f, 1.0f, PARAM_COMP_LEFT_KNEE, "Left compressor: Knee");	
		RegisterParameter(definition, "LeftRatio", "", 1.0f, MAX_RATIO, DEFAULT_RATIO, 1.0f, 1.0f, PARAM_COMP_LEFT_RATIO, "Left compressor: Ratio");
		RegisterParameter(definition, "LeftThreshold", "dB", -80.0f, 0.0f, DEFAULT_THRESHOLD, 1.0f, 1.0f, PARAM_COMP_LEFT_THRESHOLD, "Left compressor: Threshold");
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

    static UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK DistanceAttenuationCallback(UnityAudioEffectState* state, float distanceIn, float attenuationIn, float* attenuationOut)
    {				
		*attenuationOut = attenuationIn;
		return UNITY_AUDIODSP_OK;
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
		
		// Initial setup of EQ (might be overriden with set parameter)
		// TO DO: check errors with debugger
		effectdata->HL.Setup(effectdata->parameters[PARAM_INIFREQ], (int)effectdata->parameters[PARAM_BANDSNUMBER], 
							(int)effectdata->parameters[PARAM_OCTAVEBANDSTEP], effectdata->parameters[PARAM_QBPF]);
		WriteLog(state, "CREATE: Initial EQ setup:", "");
		WriteLog(state, "        Initial frequency = ", effectdata->parameters[PARAM_INIFREQ]);
		WriteLog(state, "        Number of bands = ", (int)effectdata->parameters[PARAM_BANDSNUMBER]);
		WriteLog(state, "        Octave step = 1/", (int)effectdata->parameters[PARAM_OCTAVEBANDSTEP]);
		WriteLog(state, "        Q factor of BPFs = ", effectdata->parameters[PARAM_QBPF]);
		
		// Setup of Compressor
		effectdata->HL.Compr_L.Setup(state->samplerate);
		effectdata->HL.Compr_R.Setup(state->samplerate);
		WriteLog(state, "CREATE: Compressor setup with sample rate ", state->samplerate);

		// HL is not ready until we set all band gains
		effectdata->setBandGainState = SETBG_WAITING;
		effectdata->numberOfBandsSet = 0;
		effectdata->hlReady = false;		

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
			// EQ SETUP:

			case PARAM_INIFREQ:	// Initial frequency for EQ
				WriteLog(state, "SET PARAMETER: New initial EQ frequency (Hz): ", value);
				data->numberOfBandsSet = 0;
				data->setBandGainState = SETBG_WAITING;
				data->HL.Setup(data->parameters[PARAM_INIFREQ], data->parameters[PARAM_BANDSNUMBER], data->parameters[PARAM_OCTAVEBANDSTEP], data->parameters[PARAM_QBPF]);
				break;

			case PARAM_BANDSNUMBER:	// Number of bands for EQ
				WriteLog(state, "SET PARAMETER: New number of EQ bands: ", (int)value);	
				data->numberOfBandsSet = 0;
				data->setBandGainState = SETBG_WAITING;
				data->HL.Setup(data->parameters[PARAM_INIFREQ], data->parameters[PARAM_BANDSNUMBER], data->parameters[PARAM_OCTAVEBANDSTEP], data->parameters[PARAM_QBPF]);
				break;

			case PARAM_OCTAVEBANDSTEP:	// Octave step for EQ
				WriteLog(state, "SET PARAMETER: New octave step for EQ: ", value);
				data->numberOfBandsSet = 0;
				data->setBandGainState = SETBG_WAITING;
				data->HL.Setup(data->parameters[PARAM_INIFREQ], data->parameters[PARAM_BANDSNUMBER], data->parameters[PARAM_OCTAVEBANDSTEP], data->parameters[PARAM_QBPF]);
				break;

			case PARAM_QBPF:	// Q for BPFs in EQ
				WriteLog(state, "SET PARAMETER: New Q of EQ band pass filters: ", value);
				data->numberOfBandsSet = 0;
				data->setBandGainState = SETBG_WAITING;
				data->HL.Setup(data->parameters[PARAM_INIFREQ], data->parameters[PARAM_BANDSNUMBER], data->parameters[PARAM_OCTAVEBANDSTEP], data->parameters[PARAM_QBPF]);
				break;
				
			// SET EQ BAND GAIN:

			case PARAM_SETBAND_EAR:	
				if (data->setBandGainState == SETBG_WAITING)
				{
					data->setBandGainState = SETBG_EARSELECTED;
					if ((int) value == EAR_LEFT)
						WriteLog(state, "SET BAND GAIN: Ear selected: ", "Left");
					else 
					{
						if ((int)value == EAR_RIGHT)
							WriteLog(state, "SET BAND GAIN: Ear selected: ", "Right");
						else
						{
							WriteLog(state, "SET BAND GAIN: ERROR!! Unknown ear ID: ", value);
							data->setBandGainState = SETBG_WAITING;
						}
					}
					
				}
				else
				{
					WriteLog(state, "SET BAND GAIN: ERROR!! Attempt to seat ear from wrong state: ", data->setBandGainState);
				}
				break;

			case PARAM_SETBAND_NUMBER:
				if (data->setBandGainState == SETBG_EARSELECTED)
				{
					data->setBandGainState = SETBG_BANDSELECTED;
					if (((int)value < 0) || ((int)value > (int)data->parameters[PARAM_BANDSNUMBER]))
					{
						WriteLog(state, "SET BAND GAIN: ERROR!! Wrong band number: ", value);
						data->setBandGainState = SETBG_WAITING;
					}						
					else
					{
						WriteLog(state, "SET BAND GAIN: Band selected: ", (int)value);
					}
				}
				else
				{
					WriteLog(state, "SET BAND GAIN: ERROR!! Attempt to seat band number from wrong state: ", data->setBandGainState);
				}
				break;

			case PARAM_SETBAND_GAIN:
				if (data->setBandGainState == SETBG_BANDSELECTED)
				{
					data->setBandGainState = SETBG_WAITING;
					data->numberOfBandsSet++;
					if (data->numberOfBandsSet > (int)data->parameters[PARAM_BANDSNUMBER])
					{
						WriteLog(state, "SET BAND GAIN: ERROR!! Attempt to set too many bands: ", data->numberOfBandsSet);
					}
					else
					{
						// Set band
						if ((int)data->parameters[PARAM_SETBAND_EAR] == EAR_LEFT)
							data->HL.SetBandGain_dB((int)data->parameters[PARAM_SETBAND_NUMBER], value, true);
						else
							data->HL.SetBandGain_dB((int)data->parameters[PARAM_SETBAND_NUMBER], value, false);

						// Debug log output
						string earStr = "Unknown";
						if ((int)data->parameters[PARAM_SETBAND_EAR] == EAR_LEFT)
							earStr = "Left";
						if ((int)data->parameters[PARAM_SETBAND_EAR] == EAR_RIGHT)
							earStr = "Right";
						string logOutput = "SET BAND GAIN: Gain of band " + std::to_string((int)data->parameters[PARAM_SETBAND_NUMBER]) + " for " + earStr + "ear set to " + std::to_string(value) + " dB";
						WriteLog(state, logOutput, "");

						// Check if all bands are set
						if (data->numberOfBandsSet == (int)data->parameters[PARAM_BANDSNUMBER])
						{
							WriteLog(state, "SETUP OF HL SIMULATOR IS COMPLETE. Ready to process!!!", "");
							data->hlReady = true;
						}
					}
				}
				else
				{
					WriteLog(state, "SET BAND GAIN: ERROR!! Attempt to seat band gain from wrong state: ", data->setBandGainState);
				}
				break;

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

	// TO DO: GUI...
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
			WriteLog(state, "         Buffer length = ", length);
            memcpy(outbuffer, inbuffer, length * outchannels * sizeof(float));
            return UNITY_AUDIODSP_OK;
        }

		EffectData* data = state->GetEffectData<EffectData>();

		// Before doing anything, check that the HL simulator is ready
		// TO DO: We could allow running the simulator without EQ setup, if EQ is switched off for both ears...
		if (!data->hlReady)
		{
			// Put silence in outbuffer
			//WriteLog(state, "PROCESS: HL simulator is not ready yet...", "");
			memset(outbuffer, 0.0f, length * outchannels * sizeof(float));
			return UNITY_AUDIODSP_OK;
		}

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

        return UNITY_AUDIODSP_OK;
    }
}
