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
#include "CommonUtils.h"

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
// Module switches:
#define DEFAULT_HL_ON				false
#define DEFAULT_HL_ON				false
#define DEFAULT_MULTIBANDEXPANDER_ON	true
#define DEFAULT_TEMPORALASYNCHRONY_ON	false
// Multiband expander:
#define DEFAULT_INIFREQ				62.5
#define DEFAULT_BANDSNUMBER			9
#define DEFAULT_FILTERSPERBAND		3
#define DEFAULT_CALIBRATION_DBSPL	100
#define DEFAULT_RATIO				1
#define DEFAULT_THRESHOLD			0
#define DEFAULT_ATTACK				20
#define DEFAULT_RELEASE				100
#define DEFAULT_AUDIOMETRY			{0,  0, 0, 0, 0, 0, 0, 0, 0}
// Temporal asynchrony simulator:
#define DEFAULT_TABAND				1600
#define DEFAULT_TANOISELPF			500
#define DEFAULT_TANOISEPOWER		0.0
#define DEFAULT_TALRSYNC_AMOUNT		0.0
#define DEFAULT_TALRSYNC_ON			false
#define DEFAULT_TAPOSTLPF_ON		true

// Min/max values for parameters
// Multiband expander::
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
// Temporal asynchrony simulator:
#define MIN_TABAND				200
#define MAX_TABAND				6400
#define MIN_TANOISELPF			1
#define MAX_TANOISELPF			4000
#define MIN_TANOISEPOWER		0.0
#define MAX_TANOISEPOWER		1.0
#define MIN_TALRSYNC			0.0
#define MAX_TALRSYNC			1.0
#define MIN_TAAUTOCORRELATION	-1000
#define MAX_TAAUTOCORRELATION	1000

//////////////////////////////////////////////////////

enum
{
	// Hearing loss levels
	PARAM_MBE_BAND_0_LEFT,
	PARAM_MBE_BAND_1_LEFT,
	PARAM_MBE_BAND_2_LEFT,
	PARAM_MBE_BAND_3_LEFT,
	PARAM_MBE_BAND_4_LEFT,
	PARAM_MBE_BAND_5_LEFT,
	PARAM_MBE_BAND_6_LEFT,
	PARAM_MBE_BAND_7_LEFT,
	PARAM_MBE_BAND_8_LEFT,
	PARAM_MBE_BAND_0_RIGHT,
	PARAM_MBE_BAND_1_RIGHT,
	PARAM_MBE_BAND_2_RIGHT,
	PARAM_MBE_BAND_3_RIGHT,
	PARAM_MBE_BAND_4_RIGHT,
	PARAM_MBE_BAND_5_RIGHT,
	PARAM_MBE_BAND_6_RIGHT,
	PARAM_MBE_BAND_7_RIGHT,
	PARAM_MBE_BAND_8_RIGHT,

	// Switch on/off processing for each ear 
	PARAM_HL_ON_LEFT,
	PARAM_HL_ON_RIGHT,

	// Calibration
	PARAM_CALIBRATION_DBSPL_FOR_0DBFS,

	// Multiband expander envelope detectors
	PARAM_MBE_ATTACK_LEFT,
	PARAM_MBE_RELEASE_LEFT,
	PARAM_MBE_ATTACK_RIGHT,
	PARAM_MBE_RELEASE_RIGHT,

	////////////////////////////////// NEW
	// Switch on/off multiband expander for each ear
	PARAM_MULTIBANDEXPANDER_ON_LEFT,
	PARAM_MULTIBANDEXPANDER_ON_RIGHT,

	// Temporal asynchrony
	PARAM_TEMPORALASYNCHRONY_ON_LEFT,
	PARAM_TEMPORALASYNCHRONY_ON_RIGHT,
	PARAM_TA_BAND_LEFT,
	PARAM_TA_BAND_RIGHT,
	PARAM_TA_NOISELPF_LEFT,
	PARAM_TA_NOISELPF_RIGHT,
	PARAM_TA_NOISEPOWER_LEFT,
	PARAM_TA_NOISEPOWER_RIGHT,
	PARAM_TA_LRSYNC_AMOUNT,
	PARAM_TA_LRSYNC_ON,
	PARAM_TA_POSTLPF_ON_LEFT,
	PARAM_TA_POSTLPF_ON_RIGHT,
	PARAM_TA_AUTOCORR0_GET_LEFT,
	PARAM_TA_AUTOCORR1_GET_LEFT,
	PARAM_TA_AUTOCORR0_GET_RIGHT,
	PARAM_TA_AUTOCORR1_GET_RIGHT,

		//// Debug log
		//PARAM_DEBUG_LOG,

	P_NUM
};

	/////////////////////////////////////////////////////////////////////

    struct EffectData
    {
		HAHLSimulation::CHearingLossSim HL;				
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
		RegisterParameter(definition, "HL0L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_0_LEFT, "Hearing loss level for 62.5 Hz band in left ear (dB HL)");
		RegisterParameter(definition, "HL1L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_1_LEFT, "Hearing loss level for 125 Hz band in left ear (dB HL)");
		RegisterParameter(definition, "HL2L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_2_LEFT, "Hearing loss level for 250 Hz band in left ear (dB HL)");
		RegisterParameter(definition, "HL3L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_3_LEFT, "Hearing loss level for 500 Hz band in left ear (dB HL)");
		RegisterParameter(definition, "HL4L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_4_LEFT, "Hearing loss level for 1 KHz band in left ear (dB HL)");
		RegisterParameter(definition, "HL5L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_5_LEFT, "Hearing loss level for 2 KHz band in left ear (dB HL)");
		RegisterParameter(definition, "HL6L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_6_LEFT, "Hearing loss level for 4 KHz band in left ear (dB HL)");
		RegisterParameter(definition, "HL7L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_7_LEFT, "Hearing loss level for 8 KHz band in left ear (dB HL)");
		RegisterParameter(definition, "HL8L", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_8_LEFT, "Hearing loss level for 16 KHz band in left ear (dB HL)");
		RegisterParameter(definition, "HL0R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_0_RIGHT, "Hearing loss level for 62.5 Hz band in right ear (dB HL)");
		RegisterParameter(definition, "HL1R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_1_RIGHT, "Hearing loss level for 125 Hz band in right ear (dB HL)");
		RegisterParameter(definition, "HL2R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_2_RIGHT, "Hearing loss level for 250 Hz band in right ear (dB HL)");
		RegisterParameter(definition, "HL3R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_3_RIGHT, "Hearing loss level for 500 Hz band in right ear (dB HL)");
		RegisterParameter(definition, "HL4R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_4_RIGHT, "Hearing loss level for 1 KHz band in right ear (dB HL)");
		RegisterParameter(definition, "HL5R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_5_RIGHT, "Hearing loss level for 2 KHz band in right ear (dB HL)");
		RegisterParameter(definition, "HL6R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_6_RIGHT, "Hearing loss level for 4 KHz band in right ear (dB HL)");
		RegisterParameter(definition, "HL7R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_7_RIGHT, "Hearing loss level for 8 KHz band in right ear (dB HL)");
		RegisterParameter(definition, "HL8R", "dBHL", MIN_HEARINGLOSS, MAX_HEARINGLOSS, 0.0f, 1.0f, 1.0f, PARAM_MBE_BAND_8_RIGHT, "Hearing loss level for 16 KHz band in right ear (dB HL)");
		
		// Switch on/off processing for each ear 
		RegisterParameter(definition, "HLONL", "", 0.0f, 1.0f, Bool2Float(DEFAULT_HL_ON), 1.0f, 1.0f, PARAM_HL_ON_LEFT, "Switch on hearing loss simulation for left ear");	
		RegisterParameter(definition, "HLONR", "", 0.0f, 1.0f, Bool2Float(DEFAULT_HL_ON), 1.0f, 1.0f, PARAM_HL_ON_RIGHT, "Switch on hearing loss simulation for right ear");

		// Calibration
		RegisterParameter(definition, "HLCAL", "dBSPL", 0.0f, MAX_CALIBRATION_DBSPL, DEFAULT_CALIBRATION_DBSPL, 1.0f, 1.0f, PARAM_CALIBRATION_DBSPL_FOR_0DBFS, "Calibration: dBSPL equivalent to 0 dBFS");	

		// Multiband expander envelope detectors
		RegisterParameter(definition, "HLATKL", "ms", 0.0f, MAX_ATTACK, DEFAULT_ATTACK, 1.0f, 1.0f, PARAM_MBE_ATTACK_LEFT, "Attack time for left ear envelope detectors (ms)");
		RegisterParameter(definition, "HLRELL", "ms", 0.0f, MAX_RELEASE, DEFAULT_RELEASE, 1.0f, 1.0f, PARAM_MBE_RELEASE_LEFT, "Release time for left ear envelope detectors (ms)");
		RegisterParameter(definition, "HLATKR", "ms", 0.0f, MAX_ATTACK, DEFAULT_ATTACK, 1.0f, 1.0f, PARAM_MBE_ATTACK_RIGHT, "Attack time for right ear envelope detectors (ms)");
		RegisterParameter(definition, "HLRELR", "ms", 0.0f, MAX_RELEASE, DEFAULT_RELEASE, 1.0f, 1.0f, PARAM_MBE_RELEASE_RIGHT, "Release time for right ear envelope detectors (ms)");
				
		// Switch on/off multibandexpander for each ear
		RegisterParameter(definition, "HLMBEONL", "", 0.0f, 1.0f, Bool2Float(DEFAULT_MULTIBANDEXPANDER_ON), 1.0f, 1.0f, PARAM_MULTIBANDEXPANDER_ON_LEFT, "Switch on multiband expander for left ear");	
		RegisterParameter(definition, "HLMBEONR", "", 0.0f, 1.0f, Bool2Float(DEFAULT_MULTIBANDEXPANDER_ON), 1.0f, 1.0f, PARAM_MULTIBANDEXPANDER_ON_RIGHT, "Switch on multiband expander for right ear");
		
		// Temporal asynchrony
		RegisterParameter(definition, "HLTAONL", "", 0.0f, 1.0f, Bool2Float(DEFAULT_TEMPORALASYNCHRONY_ON), 1.0f, 1.0f, PARAM_TEMPORALASYNCHRONY_ON_LEFT, "Switch on temporal asynchrony simulation for left ear");
		RegisterParameter(definition, "HLTAONR", "", 0.0f, 1.0f, Bool2Float(DEFAULT_TEMPORALASYNCHRONY_ON), 1.0f, 1.0f, PARAM_TEMPORALASYNCHRONY_ON_RIGHT, "Switch on temporal asynchrony simulation for right ear");
		RegisterParameter(definition, "HLTABANDL", "Hz", MIN_TABAND, MAX_TABAND, DEFAULT_TABAND, 1.0f, 1.0f, PARAM_TA_BAND_LEFT, "Upper band limit for temporal asynchrony simulation in left ear (Hz)");
		RegisterParameter(definition, "HLTABANDR", "Hz", MIN_TABAND, MAX_TABAND, DEFAULT_TABAND, 1.0f, 1.0f, PARAM_TA_BAND_RIGHT, "Upper band limit for temporal asynchrony simulation in right ear (Hz)");
		RegisterParameter(definition, "HLTALPFL", "Hz", MIN_TANOISELPF, MAX_TANOISELPF, DEFAULT_TANOISELPF, 1.0f, 1.0f, PARAM_TA_NOISELPF_LEFT, "Cutoff frequency of temporal asynchrony jitter noise autocorrelation LPF in left ear (Hz)");
		RegisterParameter(definition, "HLTALPFR", "Hz", MIN_TANOISELPF, MAX_TANOISELPF, DEFAULT_TANOISELPF, 1.0f, 1.0f, PARAM_TA_NOISELPF_RIGHT, "Cutoff frequency of temporal asynchrony jitter noise autocorrelation LPF in right ear (Hz)");
		RegisterParameter(definition, "HLTAPOWL", "ms", MIN_TANOISEPOWER, MAX_TANOISEPOWER, DEFAULT_TANOISEPOWER, 1.0f, 1.0f, PARAM_TA_NOISEPOWER_LEFT, "Power of temporal asynchrony jitter white noise in left ear (ms)");
		RegisterParameter(definition, "HLTAPOWR", "ms", MIN_TANOISEPOWER, MAX_TANOISEPOWER, DEFAULT_TANOISEPOWER, 1.0f, 1.0f, PARAM_TA_NOISEPOWER_RIGHT, "Power of temporal asynchrony jitter white noise in right ear (ms)");
		RegisterParameter(definition, "HLTALR", "", MIN_TALRSYNC, MAX_TALRSYNC, DEFAULT_TALRSYNC_AMOUNT, 1.0f, 1.0f, PARAM_TA_LRSYNC_AMOUNT, "Synchronicity between left and right ears in temporal asyncrhony (0.0 to 1.0)");
		RegisterParameter(definition, "HLTALRON", "", 0.0f, 1.0f, Bool2Float(DEFAULT_TALRSYNC_ON), 1.0f, 1.0f, PARAM_TA_LRSYNC_ON, "Switch on left-right synchronicity in temporal asynchrony simulation");
		RegisterParameter(definition, "HLTAPOSTONL", "", 0.0f, 1.0f, Bool2Float(DEFAULT_TAPOSTLPF_ON), 1.0f, 1.0f, PARAM_TA_POSTLPF_ON_LEFT, "Switch on post-jitter LPF in temporal asynchrony simulation in left ear");
		RegisterParameter(definition, "HLTAPOSTONR", "", 0.0f, 1.0f, Bool2Float(DEFAULT_TAPOSTLPF_ON), 1.0f, 1.0f, PARAM_TA_POSTLPF_ON_RIGHT, "Switch on post-jitter LPF in temporal asynchrony simulation in right ear");
		RegisterParameter(definition, "HLTA0GL", "", MIN_TAAUTOCORRELATION, MAX_TAAUTOCORRELATION, 0.0f, 1.0f, 1.0f, PARAM_TA_AUTOCORR0_GET_LEFT, "Autocorrelation coefficient zero in left temporal asynchrony noise source?");
		RegisterParameter(definition, "HLTA1GL", "", MIN_TAAUTOCORRELATION, MAX_TAAUTOCORRELATION, 0.0f, 1.0f, 1.0f, PARAM_TA_AUTOCORR1_GET_LEFT, "Autocorrelation coefficient one in left temporal asynchrony noise source?");
		RegisterParameter(definition, "HLTA0GR", "", MIN_TAAUTOCORRELATION, MAX_TAAUTOCORRELATION, 0.0f, 1.0f, 1.0f, PARAM_TA_AUTOCORR0_GET_RIGHT, "Autocorrelation coefficient zero in right temporal asynchrony noise source?");
		RegisterParameter(definition, "HLTA1GR", "", MIN_TAAUTOCORRELATION, MAX_TAAUTOCORRELATION, 0.0f, 1.0f, 1.0f, PARAM_TA_AUTOCORR1_GET_RIGHT, "Autocorrelation coefficient one in right temporal asynchrony noise source?");

		// Debug log
		//RegisterParameter(definition, "DebugLogHL", "", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, PARAM_DEBUG_LOG, "Generate debug log for HL");

        return numparams;
    }
	
	/////////////////////////////////////////////////////////////////////

	void WriteLogHeader(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		// Module switches:
		WriteLog(state, "CREATE: Module switches:", "");
		WriteLog(state, "        HL Left = ", DEFAULT_HL_ON);
		WriteLog(state, "        HL Right = ", DEFAULT_HL_ON);
		WriteLog(state, "        Multiband Expander Left = ", DEFAULT_MULTIBANDEXPANDER_ON);
		WriteLog(state, "        Multiband Expander Right = ", DEFAULT_MULTIBANDEXPANDER_ON);
		WriteLog(state, "        Temporal Asynchrony Left = ", DEFAULT_TEMPORALASYNCHRONY_ON);
		WriteLog(state, "        Temporal Asynchrony Right = ", DEFAULT_TEMPORALASYNCHRONY_ON);
		
		// Multiband expander EQ:
		WriteLog(state, "CREATE: Multiband expander setup:", "");
		WriteLog(state, "        Initial frequency = ", DEFAULT_INIFREQ);
		WriteLog(state, "        Number of bands = ", DEFAULT_BANDSNUMBER);
		WriteLog(state, "        Filters per band = ", DEFAULT_FILTERSPERBAND);

		// Compressor:
		WriteLog(state, "CREATE: Envelope detectors setup:", "");
		WriteLog(state, "        Sample rate = ", state->samplerate);
		WriteLog(state, "        Attack time = ", DEFAULT_ATTACK);
		WriteLog(state, "        Release time = ", DEFAULT_RELEASE);

		// Calibration:
		WriteLog(state, "CREATE: Calibration setup:", "");
		WriteLog(state, "        dBSPL for 0 dBFS = ", DEFAULT_CALIBRATION_DBSPL);

		// Temporal asynchrony:
		string lrsync = "";
		if (DEFAULT_TALRSYNC_ON)
			lrsync = std::to_string(DEFAULT_TALRSYNC_AMOUNT) + " (ON)";
		else
			lrsync = "OFF";
		WriteLog(state, "CREATE: Temporal asynchrony setup:", "");
		WriteLog(state, "        Left-Right synchronicity = ", lrsync);
		WriteLog(state, "        Left band upper limit = ", DEFAULT_TABAND);
		WriteLog(state, "        Left autocorrelation LPF cutoff = ", DEFAULT_TANOISELPF);
		WriteLog(state, "        Left white noise power = ", DEFAULT_TANOISEPOWER);
		WriteLog(state, "        Right band upper limit = ", DEFAULT_TABAND);
		WriteLog(state, "        Right autocorrelation LPF cutoff = ", DEFAULT_TANOISELPF);
		WriteLog(state, "        Right white noise power = ", DEFAULT_TANOISEPOWER);
		WriteLog(state, "        Post-jitter LPF = ", Bool2String(DEFAULT_TAPOSTLPF_ON));

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

		// Module switches
		if (DEFAULT_HL_ON)		
			effectdata->HL.EnableHearingLossSimulation(Common::T_ear::BOTH);
		if (DEFAULT_MULTIBANDEXPANDER_ON)
			effectdata->HL.EnableMultibandExpander(Common::T_ear::BOTH);
		if (DEFAULT_TEMPORALASYNCHRONY_ON)
			effectdata->HL.EnableTemporalAsynchrony(Common::T_ear::BOTH);

		// Hearing loss simulator Setup		
		effectdata->HL.Setup(state->samplerate, DEFAULT_CALIBRATION_DBSPL, DEFAULT_INIFREQ, DEFAULT_BANDSNUMBER, DEFAULT_FILTERSPERBAND, state->dspbuffersize);		

		// Initial setup of hearing loss levels
		effectdata->HL.SetFromAudiometry_dBHL(Common::T_ear::BOTH, DEFAULT_AUDIOMETRY);

		// Setup calibration
		effectdata->HL.SetCalibration(DEFAULT_CALIBRATION_DBSPL);

		// Setup of envelope detectors
		effectdata->HL.SetAttackForAllBands(Common::T_ear::BOTH, DEFAULT_ATTACK);
		effectdata->HL.SetReleaseForAllBands(Common::T_ear::BOTH, DEFAULT_RELEASE);

		// Initial setup of temporal asynchrony simulator
		effectdata->HL.GetTemporalAsynchronySimulator()->SetBandUpperLimit(Common::T_ear::BOTH, DEFAULT_TABAND);
		effectdata->HL.GetTemporalAsynchronySimulator()->SetNoiseAutocorrelationFilterCutoffFrequency(Common::T_ear::BOTH, DEFAULT_TANOISELPF);
		effectdata->HL.GetTemporalAsynchronySimulator()->SetWhiteNoisePower(Common::T_ear::BOTH, DEFAULT_TANOISEPOWER);
		effectdata->HL.GetTemporalAsynchronySimulator()->SetLeftRightNoiseSynchronicity(DEFAULT_TALRSYNC_AMOUNT);
		if (DEFAULT_TALRSYNC_ON)
			effectdata->HL.GetTemporalAsynchronySimulator()->EnableLeftRightNoiseSynchronicity();
		else
			effectdata->HL.GetTemporalAsynchronySimulator()->DisableLeftRightNoiseSynchronicity();
		if (DEFAULT_TAPOSTLPF_ON)		
			effectdata->HL.GetTemporalAsynchronySimulator()->EnablePostJitterLowPassFilter(Common::T_ear::BOTH);
		else
			effectdata->HL.GetTemporalAsynchronySimulator()->DisablePostJitterLowPassFilter(Common::T_ear::BOTH);

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

	void SetOneHearingLossLevel(UnityAudioEffectState* state, Common::T_ear ear, int bandIndex, float valueDBHL)
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
		HAHLSimulation::CHearingLossSim HL = state->GetEffectData<EffectData>()->HL;
		HL.SetHearingLevel_dBHL(ear, bandIndex, valueDBHL);
					
		// Debug log output
		string earStr = "Unknown";
		if (ear == Common::T_ear::LEFT)
			earStr = "Left";
		if (ear == Common::T_ear::RIGHT)
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
		case PARAM_MBE_BAND_0_LEFT:	SetOneHearingLossLevel(state, Common::T_ear::LEFT, 0, value);	break;
			case PARAM_MBE_BAND_1_LEFT:	SetOneHearingLossLevel(state, Common::T_ear::LEFT, 1, value);	break;
			case PARAM_MBE_BAND_2_LEFT:	SetOneHearingLossLevel(state, Common::T_ear::LEFT, 2, value);	break;
			case PARAM_MBE_BAND_3_LEFT:	SetOneHearingLossLevel(state, Common::T_ear::LEFT, 3, value);	break;
			case PARAM_MBE_BAND_4_LEFT:	SetOneHearingLossLevel(state, Common::T_ear::LEFT, 4, value);	break;
			case PARAM_MBE_BAND_5_LEFT:	SetOneHearingLossLevel(state, Common::T_ear::LEFT, 5, value);	break;
			case PARAM_MBE_BAND_6_LEFT:	SetOneHearingLossLevel(state, Common::T_ear::LEFT, 6, value);	break;
			case PARAM_MBE_BAND_7_LEFT:	SetOneHearingLossLevel(state, Common::T_ear::LEFT, 7, value);	break;
			case PARAM_MBE_BAND_8_LEFT:	SetOneHearingLossLevel(state, Common::T_ear::LEFT, 8, value);	break;
			case PARAM_MBE_BAND_0_RIGHT:	SetOneHearingLossLevel(state, Common::T_ear::RIGHT, 0, value);	break;
			case PARAM_MBE_BAND_1_RIGHT:	SetOneHearingLossLevel(state, Common::T_ear::RIGHT, 1, value);	break;
			case PARAM_MBE_BAND_2_RIGHT:	SetOneHearingLossLevel(state, Common::T_ear::RIGHT, 2, value);	break;
			case PARAM_MBE_BAND_3_RIGHT:	SetOneHearingLossLevel(state, Common::T_ear::RIGHT, 3, value);	break;
			case PARAM_MBE_BAND_4_RIGHT:	SetOneHearingLossLevel(state, Common::T_ear::RIGHT, 4, value);	break;
			case PARAM_MBE_BAND_5_RIGHT:	SetOneHearingLossLevel(state, Common::T_ear::RIGHT, 5, value);	break;
			case PARAM_MBE_BAND_6_RIGHT:	SetOneHearingLossLevel(state, Common::T_ear::RIGHT, 6, value);	break;
			case PARAM_MBE_BAND_7_RIGHT:	SetOneHearingLossLevel(state, Common::T_ear::RIGHT, 7, value);	break;
			case PARAM_MBE_BAND_8_RIGHT:	SetOneHearingLossLevel(state, Common::T_ear::RIGHT, 8, value);	break;

			// SWITCH ON/OFF MODULES FOR EACH EAR:
			case PARAM_HL_ON_LEFT:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off left ear hearing loss simulation with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.DisableHearingLossSimulation(Common::T_ear::LEFT);
						WriteLog(state, "SET PARAMETER: Left ear hearing loss simulation switched OFF", "");
					}
					if (Float2Bool(value))
					{
						data->HL.EnableHearingLossSimulation(Common::T_ear::LEFT);
						WriteLog(state, "SET PARAMETER: Left ear hearing loss simulation switched ON", "");
					}
				}
				break;

			case PARAM_HL_ON_RIGHT:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off right ear hearing loss simulation with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.DisableHearingLossSimulation(Common::T_ear::RIGHT);
						WriteLog(state, "SET PARAMETER: Right ear hearing loss simulation switched OFF", "");
					}
					if (Float2Bool(value))
					{
						data->HL.EnableHearingLossSimulation(Common::T_ear::RIGHT);
						WriteLog(state, "SET PARAMETER: Right ear hearing loss simulation switched ON", "");
					}
				}
				break;

			case PARAM_MULTIBANDEXPANDER_ON_LEFT:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off left multiband expander with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.DisableMultibandExpander(Common::T_ear::LEFT);
						WriteLog(state, "SET PARAMETER: Left ear multiband expander switched OFF", "");
					}
					if (Float2Bool(value))
					{
						data->HL.EnableMultibandExpander(Common::T_ear::LEFT);
						WriteLog(state, "SET PARAMETER: Left ear multiband expander switched ON", "");
					}
				}
				break;

			case PARAM_MULTIBANDEXPANDER_ON_RIGHT:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off right multiband expander with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.DisableMultibandExpander(Common::T_ear::RIGHT);
						WriteLog(state, "SET PARAMETER: Right ear multiband expander switched OFF", "");
					}
					if (Float2Bool(value))
					{
						data->HL.EnableMultibandExpander(Common::T_ear::RIGHT);
						WriteLog(state, "SET PARAMETER: Right ear multiband expander switched ON", "");
					}
				}
				break;

			case PARAM_TEMPORALASYNCHRONY_ON_LEFT:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off left temporal asynchrony simulation with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.DisableTemporalAsynchrony(Common::T_ear::LEFT);
						WriteLog(state, "SET PARAMETER: Left ear temporal asynchrony simulation switched OFF", "");
					}
					if (Float2Bool(value))
					{
						data->HL.EnableTemporalAsynchrony(Common::T_ear::LEFT);
						WriteLog(state, "SET PARAMETER: Left ear temporal asynchrony simulation switched ON", "");
					}
				}
				break;

			case PARAM_TEMPORALASYNCHRONY_ON_RIGHT:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off right temporal asynchrony simulation with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.DisableTemporalAsynchrony(Common::T_ear::RIGHT);
						WriteLog(state, "SET PARAMETER: Right ear temporal asynchrony simulation switched OFF", "");
					}
					if (Float2Bool(value))
					{
						data->HL.EnableTemporalAsynchrony(Common::T_ear::RIGHT);
						WriteLog(state, "SET PARAMETER: Right ear temporal asynchrony simulation switched ON", "");
					}
				}
				break;

			// CALIBRATION:
			case PARAM_CALIBRATION_DBSPL_FOR_0DBFS:
				data->HL.SetCalibration(value);
				WriteLog(state, "SET PARAMETER: Calibration (dBSPL for 0dBFS) set to: ", value);
				break;

			// ENVELOPE DETECTORS:
			case PARAM_MBE_ATTACK_LEFT:
				data->HL.SetAttackForAllBands(Common::T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: Attack time (ms) for Left envelope detectors set to: ", value);				
				break;

			case PARAM_MBE_ATTACK_RIGHT:
				data->HL.SetAttackForAllBands(Common::T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: Attack time (ms) for Right envelope detectors set to: ", value);
				break;

			case PARAM_MBE_RELEASE_LEFT:
				data->HL.SetReleaseForAllBands(Common::T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: Release time (ms) for Left envelope detectors set to: ", value);
				break;

			case PARAM_MBE_RELEASE_RIGHT:
				data->HL.SetReleaseForAllBands(Common::T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: Release time (ms) for Right envelope detectors set to: ", value);
				break;

			// TEMPORAL ASYNCHRONY SIMULATION:
			case PARAM_TA_BAND_LEFT:
				data->HL.GetTemporalAsynchronySimulator()->SetBandUpperLimit(Common::T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: Band upper limit (Hz) for Left temporal asynchrony simulator set to: ", value);
				break;

			case PARAM_TA_BAND_RIGHT:
				data->HL.GetTemporalAsynchronySimulator()->SetBandUpperLimit(Common::T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: Band upper limit (Hz) for Right temporal asynchrony simulator set to: ", value);
				break;

			case PARAM_TA_NOISELPF_LEFT:
				data->HL.GetTemporalAsynchronySimulator()->SetNoiseAutocorrelationFilterCutoffFrequency(Common::T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: Noise autocorrelation LPF cutoff (Hz) for Left temporal asynchrony simulator set to: ", value);
				break;

			case PARAM_TA_NOISELPF_RIGHT:
				data->HL.GetTemporalAsynchronySimulator()->SetNoiseAutocorrelationFilterCutoffFrequency(Common::T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: Noise autocorrelation LPF cutoff (Hz) for Right temporal asynchrony simulator set to: ", value);
				break;

			case PARAM_TA_NOISEPOWER_LEFT:
				data->HL.GetTemporalAsynchronySimulator()->SetWhiteNoisePower(Common::T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: White noise power (ms) for Left temporal asynchrony simulator set to: ", value);
				break;

			case PARAM_TA_NOISEPOWER_RIGHT:
				data->HL.GetTemporalAsynchronySimulator()->SetWhiteNoisePower(Common::T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: White noise power (ms) for Right temporal asynchrony simulator set to: ", value);
				break;

			case PARAM_TA_LRSYNC_AMOUNT:
				if ((value >= 0.0f) && (value <= 1.0f))
				{
					data->HL.GetTemporalAsynchronySimulator()->SetLeftRightNoiseSynchronicity(value);
					WriteLog(state, "SET PARAMETER: Left-Right synchronicity for temporal asynchrony simulator set to: ", value);
				}
				else
					WriteLog(state, "SET PARAMETER: ERROR!!! Bad value for Left-Right synchronicity of temporal asynchrony (needs to be between 0.0f and 1.0f): ", value);
				break;

			case PARAM_TA_LRSYNC_ON:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off left-right synchronicity in temporal asynchrony simulation with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.GetTemporalAsynchronySimulator()->DisableLeftRightNoiseSynchronicity();
						WriteLog(state, "SET PARAMETER: Left-right ear synchronicity in temporal asynchrony simulator switched OFF", "");
					}
					if (Float2Bool(value))
					{
						data->HL.GetTemporalAsynchronySimulator()->EnableLeftRightNoiseSynchronicity();
						WriteLog(state, "SET PARAMETER: Left-right ear synchronicity in temporal asynchrony simulator switched ON", "");
					}
				}
				break;

			case PARAM_TA_POSTLPF_ON_LEFT:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off post-jitter LPF in temporal asynchrony simulation with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.GetTemporalAsynchronySimulator()->DisablePostJitterLowPassFilter(Common::T_ear::LEFT);
						WriteLog(state, "SET PARAMETER: Post-jitter LPF in temporal asynchrony simulator switched OFF for left ear", "");
					}
					if (Float2Bool(value))
					{
						data->HL.GetTemporalAsynchronySimulator()->EnablePostJitterLowPassFilter(Common::T_ear::LEFT);
						WriteLog(state, "SET PARAMETER: Post-jitter LPF in temporal asynchrony simulator switched ON for left ear", "");
					}
				}
				break;

			case PARAM_TA_POSTLPF_ON_RIGHT:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off post-jitter LPF in temporal asynchrony simulation with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.GetTemporalAsynchronySimulator()->DisablePostJitterLowPassFilter(Common::T_ear::RIGHT);
						WriteLog(state, "SET PARAMETER: Post-jitter LPF in temporal asynchrony simulator switched OFF for right ear", "");
					}
					if (Float2Bool(value))
					{
						data->HL.GetTemporalAsynchronySimulator()->EnablePostJitterLowPassFilter(Common::T_ear::RIGHT);
						WriteLog(state, "SET PARAMETER: Post-jitter LPF in temporal asynchrony simulator switched ON for right ear", "");
					}
				}
				break;

			case PARAM_TA_AUTOCORR0_GET_LEFT:
				WriteLog(state, "SET PARAMETER: WARNING! PARAM_TA_AUTOCORR0_GET_LEFT is read only", "");
				break;
			case PARAM_TA_AUTOCORR1_GET_LEFT:
				WriteLog(state, "SET PARAMETER: WARNING! PARAM_TA_AUTOCORR1_GET_LEFT is read only", "");
				break;
			case PARAM_TA_AUTOCORR0_GET_RIGHT:
				WriteLog(state, "SET PARAMETER: WARNING! PARAM_TA_AUTOCORR0_GET_RIGHT is read only", "");
				break;
			case PARAM_TA_AUTOCORR1_GET_RIGHT:
				WriteLog(state, "SET PARAMETER: WARNING! PARAM_TA_AUTOCORR1_GET_RIGHT is read only", "");
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
        if (valuestr != NULL)
            valuestr[0] = 0;

		if (value != NULL)
		{
			switch (index)
			{
				case PARAM_TA_AUTOCORR0_GET_LEFT:
					*value = data->HL.GetTemporalAsynchronySimulator()->GetAutocorrelationCoefficient0(Common::T_ear::LEFT);
					break;

				case PARAM_TA_AUTOCORR1_GET_LEFT:
					*value = data->HL.GetTemporalAsynchronySimulator()->GetAutocorrelationCoefficient1(Common::T_ear::LEFT);
					break;

				case PARAM_TA_AUTOCORR0_GET_RIGHT:
					*value = data->HL.GetTemporalAsynchronySimulator()->GetAutocorrelationCoefficient0(Common::T_ear::RIGHT);
					break;

				case PARAM_TA_AUTOCORR1_GET_RIGHT:
					*value = data->HL.GetTemporalAsynchronySimulator()->GetAutocorrelationCoefficient1(Common::T_ear::RIGHT);
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
		CStereoBuffer<float> inputBuffer(length*2);						
		for (int i = 0; i < length*2; i++)
		{
			inputBuffer[i] = inbuffer[i]; 
		}

		// Process!!
		CStereoBuffer<float> outputBuffer(length*2);
		data->HL.Process(inputBuffer, outputBuffer);		

		data->parameters[PARAM_TA_AUTOCORR0_GET_LEFT] = data->HL.GetTemporalAsynchronySimulator()->GetAutocorrelationCoefficient0(Common::T_ear::LEFT);
		data->parameters[PARAM_TA_AUTOCORR1_GET_LEFT] = data->HL.GetTemporalAsynchronySimulator()->GetAutocorrelationCoefficient1(Common::T_ear::LEFT);
		data->parameters[PARAM_TA_AUTOCORR0_GET_RIGHT] = data->HL.GetTemporalAsynchronySimulator()->GetAutocorrelationCoefficient0(Common::T_ear::RIGHT);
		data->parameters[PARAM_TA_AUTOCORR1_GET_RIGHT] = data->HL.GetTemporalAsynchronySimulator()->GetAutocorrelationCoefficient1(Common::T_ear::RIGHT);

		// Transform output buffer					
		for (int i = 0; i < length*2; i++)
		{
			outbuffer[i] = outputBuffer[i];
		}		

        return UNITY_AUDIODSP_OK;
    }
}
