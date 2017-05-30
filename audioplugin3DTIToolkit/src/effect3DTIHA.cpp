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

#include <Common/DynamicCompressorStereo.h>
#include <HAHLSimulation/HearingAidSim.h>

// Includes for debug logging
#include <fstream>
#include <iostream>
#include <string>
using namespace std;

// DEBUG LOG 
#ifdef UNITY_ANDROID
#define DEBUG_LOG_CAT
#else
#define DEBUG_LOG_FILE_HA
#endif

#ifdef DEBUG_LOG_CAT
#include <android/log.h> 
#include <string>
#include <sstream>
#endif

/////////////////////////////////////////////////////////////////////

namespace HASimulation3DTI
{

//////////////////////////////////////////////////////
#define EAR_RIGHT 0
#define EAR_LEFT 1
#define F_TRUE	1.0f
#define F_FALSE	0.0f
#define TONE_BANDS 3
#define BAND_LOW 0
#define BAND_MID 1
#define BAND_HIGH 2

// Default values 
#define DEFAULT_LEVELSINTERPOLATION	F_TRUE
#define DEFAULT_PROCESSLEFT			F_FALSE
#define DEFAULT_PROCESSRIGHT		F_FALSE
#define DEFAULT_VOLDB				0.0f
#define DEFAULT_NUMLEVELS			3
#define DEFAULT_BANDSNUMBER			7
#define DEFAULT_INIFREQ				125.0f
#define DEFAULT_OCTAVEBANDSTEP		1.0f
#define DEFAULT_LPFCUTOFF			3000.0f
#define DEFAULT_HPFCUTOFF			500.0f
#define DEFAULT_QBPF				1.4142f
#define DEFAULT_QLPF				0.707f
#define DEFAULT_QHPF				0.707f
#define DEFAULT_ATTACKRELEASE		1000.0f
#define DEFAULT_BANDGAINDB			0.0f
#define DEFAULT_LEVELTHRESHOLD_0	-20.0f
#define DEFAULT_LEVELTHRESHOLD_1	-40.0f
#define DEFAULT_LEVELTHRESHOLD_2	-60.0f
#define DEFAULT_NOISENUMBITS		16
#define DEFAULT_NOISEBEFORE			F_FALSE
#define DEFAULT_NOISEAFTER			F_FALSE
#define DEFAULT_DBSPL_FOR_0DBS		0.0f
#define DEFAULT_FIG6_BANDS_PER_EAR	7
#define DEFAULT_COMPRESSION_PERCENTAGE	100.0f
#define DEFAULT_NORMALIZATION		20.0f

// Min/max values for parameters
#define MIN_VOLDB			-24.0f
#define MAX_VOLDB			24.0f
#define MIN_LPFCUTOFF		62.5f
#define MAX_LPFCUTOFF		16000.0f
#define MIN_HPFCUTOFF		62.5f
#define MAX_HPFCUTOFF		16000.0f
#define MIN_BANDGAINDB		0.0f
#define MAX_BANDGAINDB		60.0f
#define MIN_LEVELTHRESHOLD	-80.0f
#define MAX_LEVELTHRESHOLD	0.0f
#define MIN_NOISENUMBITS	6
#define MAX_NOISENUMBITS	24
#define MIN_ATTACKRELEASE	10.0f
#define MAX_ATTACKRELEASE	2000.0f
#define MIN_FIG6			0.0f
#define MAX_FIG6			80.0f
#define MAX_COMPRESSION_PERCENTAGE	120.0f
#define MIN_NORMALIZATION	1.0f
#define MAX_NORMALIZATION	40.0f
#define MIN_TONECONTROL		-10.0f
#define MAX_TONECONTROL		10.0f

// Fixed values
#define HA_LIMITER_THRESHOLD	-30.0f
#define HA_LIMITER_ATTACK		500.0f
#define HA_LIMITER_RELEASE		500.0f
#define HA_LIMITER_RATIO		6

//////////////////////////////////////////////////////

	enum
	{
		// Global parameters
		PARAM_PROCESS_LEFT_ON,
		PARAM_PROCESS_RIGHT_ON,
		PARAM_VOLUME_L_DB,
		PARAM_VOLUME_R_DB,

		// Common values for both ears in EQ		
		PARAM_EQ_LPFCUTOFF_HZ,
		PARAM_EQ_HPFCUTOFF_HZ,

		// Dynamic EQ
		PARAM_DYNAMICEQ_INTERPOLATION_ON,
		PARAM_DYNAMICEQ_LEVELTHRESHOLD_0_LEFT_DBFS,
		PARAM_DYNAMICEQ_LEVELTHRESHOLD_1_LEFT_DBFS,
		PARAM_DYNAMICEQ_LEVELTHRESHOLD_2_LEFT_DBFS,
		PARAM_DYNAMICEQ_LEVELTHRESHOLD_0_RIGHT_DBFS,
		PARAM_DYNAMICEQ_LEVELTHRESHOLD_1_RIGHT_DBFS,
		PARAM_DYNAMICEQ_LEVELTHRESHOLD_2_RIGHT_DBFS,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_0_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_1_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_2_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_3_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_4_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_5_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_6_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_0_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_1_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_2_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_3_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_4_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_5_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_6_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_0_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_1_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_2_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_3_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_4_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_5_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_6_LEFT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_0_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_1_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_2_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_3_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_4_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_5_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_0_BAND_6_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_0_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_1_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_2_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_3_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_4_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_5_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_1_BAND_6_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_0_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_1_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_2_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_3_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_4_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_5_RIGHT_DB,
		PARAM_DYNAMICEQ_LEVEL_2_BAND_6_RIGHT_DB,
		PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS,
		PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS,

		// Quantization noise
		PARAM_NOISE_BEFORE_ON,
		PARAM_NOISE_AFTER_ON,
		PARAM_NOISE_NUMBITS,

		// Simplified controls
		PARAM_COMPRESSION_PERCENTAGE_LEFT,
		PARAM_COMPRESSION_PERCENTAGE_RIGHT,

		// Limiter
		PARAM_LIMITER_SET_ON,
		PARAM_LIMITER_GET_COMPRESSION,

		// Normalization
		PARAM_NORMALIZATION_LEFT_ON,
		PARAM_NORMALIZATION_LEFT_DBS,
		PARAM_NORMALIZATION_LEFT_GET,
		PARAM_NORMALIZATION_RIGHT_ON,
		PARAM_NORMALIZATION_RIGHT_DBS,
		PARAM_NORMALIZATION_RIGHT_GET,

		// Tone control
		PARAM_TONE_LOW_LEFT,
		PARAM_TONE_MID_LEFT,
		PARAM_TONE_HIGH_LEFT,
		PARAM_TONE_LOW_RIGHT,
		PARAM_TONE_MID_RIGHT,
		PARAM_TONE_HIGH_RIGHT,

		// Debug log
		//PARAM_DEBUG_LOG,

		//// Fig6
		//PARAM_FIG6_BAND_0_LEFT,
		//PARAM_FIG6_BAND_1_LEFT,
		//PARAM_FIG6_BAND_2_LEFT,
		//PARAM_FIG6_BAND_3_LEFT,
		//PARAM_FIG6_BAND_4_LEFT,
		//PARAM_FIG6_BAND_5_LEFT,
		//PARAM_FIG6_BAND_6_LEFT,
		//PARAM_FIG6_BAND_0_RIGHT,
		//PARAM_FIG6_BAND_1_RIGHT,
		//PARAM_FIG6_BAND_2_RIGHT,
		//PARAM_FIG6_BAND_3_RIGHT,
		//PARAM_FIG6_BAND_4_RIGHT,
		//PARAM_FIG6_BAND_5_RIGHT,
		//PARAM_FIG6_BAND_6_RIGHT,

		P_NUM
	};

	/////////////////////////////////////////////////////////////////////

    struct EffectData
    {
		CHearingAidSim HA;		

		// Limiter
		CDynamicCompressorStereo limiter;
		bool limiterNotInitialized;

		// Tone control
		float toneLeft[TONE_BANDS] = { 0.0f, 0.0f, 0.0f };
		float toneRight[TONE_BANDS] = { 0.0f, 0.0f, 0.0f };

		float parameters[P_NUM];

		// READY TO PROCESS
		//bool haReady = false;

		// DEBUG LOG
		//bool debugLog = true;

		//// Fig6
		//bool settingFig6Left;
		//int fig6ReceivedBandsLeft;	// TO DO: check each individual band
		//bool settingFig6Right;
		//int fig6ReceivedBandsRight;	// TO DO: check each individual band
	};

	/////////////////////////////////////////////////////////////////////

	template <class T>
	void WriteLog(UnityAudioEffectState* state, string logtext, const T& value)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		//if (data->debugLog)
		{
			#ifdef DEBUG_LOG_FILE_HA
			ofstream logfile;
			logfile.open("3DTI_HearingAidSimulation_DebugLog.txt", ofstream::out | ofstream::app);
			logfile << logtext << value << endl;
			logfile.close();
			#endif

			#ifdef DEBUG_LOG_CAT						
			std::ostringstream os;
			os << logtext << value;
			string fulltext = os.str();
			__android_log_print(ANDROID_LOG_DEBUG, "3DTIHASIMULATION", fulltext.c_str());
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

		// Global parameters
		RegisterParameter(definition, "HAL", "", F_FALSE, F_TRUE, DEFAULT_PROCESSLEFT, 1.0f, 1.0f, PARAM_PROCESS_LEFT_ON, "Switch On/Off Left HA");
		RegisterParameter(definition, "HAR", "", F_FALSE, F_TRUE, DEFAULT_PROCESSRIGHT, 1.0f, 1.0f, PARAM_PROCESS_RIGHT_ON, "Switch On/Off Right HA");
		RegisterParameter(definition, "VOLL", "dB", MIN_VOLDB, MAX_VOLDB, DEFAULT_VOLDB, 1.0f, 1.0f, PARAM_VOLUME_L_DB, "Left HA volume (dB)");
		RegisterParameter(definition, "VOLR", "dB", MIN_VOLDB, MAX_VOLDB, DEFAULT_VOLDB, 1.0f, 1.0f, PARAM_VOLUME_R_DB, "Right HA volume (dB)");

		// Common switches and values for EQ		
		RegisterParameter(definition, "LPF", "Hz", MIN_LPFCUTOFF, MAX_LPFCUTOFF, DEFAULT_LPFCUTOFF, 1.0f, 1.0f, PARAM_EQ_LPFCUTOFF_HZ, "Cutoff frequency of LPF");
		RegisterParameter(definition, "HPF", "Hz", MIN_HPFCUTOFF, MAX_HPFCUTOFF, DEFAULT_HPFCUTOFF, 1.0f, 1.0f, PARAM_EQ_HPFCUTOFF_HZ, "Cutoff frequency of HPF");	

		// Dynamic EQ    
		RegisterParameter(definition, "EQINT", "", F_FALSE, F_TRUE, DEFAULT_LEVELSINTERPOLATION, 1.0f, 1.0f, PARAM_DYNAMICEQ_INTERPOLATION_ON, "Switch On/Off Dynamic EQ Level interpolation");
		RegisterParameter(definition, "THRL0", "dBfs", MIN_LEVELTHRESHOLD, MAX_LEVELTHRESHOLD, DEFAULT_LEVELTHRESHOLD_0, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVELTHRESHOLD_0_LEFT_DBFS, "Dynamic EQ first level threshold Left");
		RegisterParameter(definition, "THRL1", "dBfs", MIN_LEVELTHRESHOLD, MAX_LEVELTHRESHOLD, DEFAULT_LEVELTHRESHOLD_0, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVELTHRESHOLD_1_LEFT_DBFS, "Dynamic EQ second level threshold Left");
		RegisterParameter(definition, "THRL2", "dBfs", MIN_LEVELTHRESHOLD, MAX_LEVELTHRESHOLD, DEFAULT_LEVELTHRESHOLD_0, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVELTHRESHOLD_2_LEFT_DBFS, "Dynamic EQ third level threshold Left");
		RegisterParameter(definition, "THRR0", "dBfs", MIN_LEVELTHRESHOLD, MAX_LEVELTHRESHOLD, DEFAULT_LEVELTHRESHOLD_0, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVELTHRESHOLD_0_RIGHT_DBFS, "Dynamic EQ first level threshold Right");		
		RegisterParameter(definition, "THRR1", "dBfs", MIN_LEVELTHRESHOLD, MAX_LEVELTHRESHOLD, DEFAULT_LEVELTHRESHOLD_0, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVELTHRESHOLD_1_RIGHT_DBFS, "Dynamic EQ second level threshold Right");
		RegisterParameter(definition, "THRR2", "dBfs", MIN_LEVELTHRESHOLD, MAX_LEVELTHRESHOLD, DEFAULT_LEVELTHRESHOLD_0, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVELTHRESHOLD_2_RIGHT_DBFS, "Dynamic EQ third level threshold Right");
		RegisterParameter(definition, "DEQL0B0L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_0_LEFT_DB, "EQ Left 125 Hz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B1L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_1_LEFT_DB, "EQ Left 250 Hz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B2L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_2_LEFT_DB, "EQ Left 500 Hz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B3L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_3_LEFT_DB, "EQ Left 1 KHz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B4L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_4_LEFT_DB, "EQ Left 2 KHz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B5L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_5_LEFT_DB, "EQ Left 4 KHz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B6L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_6_LEFT_DB, "EQ Left 8 KHz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL1B0L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_0_LEFT_DB, "EQ Left 125 Hz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B1L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_1_LEFT_DB, "EQ Left 250 Hz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B2L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_2_LEFT_DB, "EQ Left 500 Hz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B3L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_3_LEFT_DB, "EQ Left 1 KHz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B4L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_4_LEFT_DB, "EQ Left 2 KHz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B5L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_5_LEFT_DB, "EQ Left 4 KHz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B6L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_6_LEFT_DB, "EQ Left 8 KHz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL2B0L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_0_LEFT_DB, "EQ Left 125 Hz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B1L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_1_LEFT_DB, "EQ Left 250 Hz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B2L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_2_LEFT_DB, "EQ Left 500 Hz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B3L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_3_LEFT_DB, "EQ Left 1 KHz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B4L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_4_LEFT_DB, "EQ Left 2 KHz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B5L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_5_LEFT_DB, "EQ Left 4 KHz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B6L", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_6_LEFT_DB, "EQ Left 8 KHz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL0B0R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_0_RIGHT_DB, "EQ Right 125 Hz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B1R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_1_RIGHT_DB, "EQ Right 250 Hz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B2R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_2_RIGHT_DB, "EQ Right 500 Hz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B3R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_3_RIGHT_DB, "EQ Right 1 KHz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B4R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_4_RIGHT_DB, "EQ Right 2 KHz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B5R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_5_RIGHT_DB, "EQ Right 4 KHz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL0B6R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_0_BAND_6_RIGHT_DB, "EQ Right 8 KHz band gain (dB) for first level");
		RegisterParameter(definition, "DEQL1B0R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_0_RIGHT_DB, "EQ Right 125 Hz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B1R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_1_RIGHT_DB, "EQ Right 250 Hz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B2R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_2_RIGHT_DB, "EQ Right 500 Hz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B3R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_3_RIGHT_DB, "EQ Right 1 KHz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B4R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_4_RIGHT_DB, "EQ Right 2 KHz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B5R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_5_RIGHT_DB, "EQ Right 4 KHz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL1B6R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_1_BAND_6_RIGHT_DB, "EQ Right 8 KHz band gain (dB) for second level");
		RegisterParameter(definition, "DEQL2B0R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_0_RIGHT_DB, "EQ Right 125 Hz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B1R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_1_RIGHT_DB, "EQ Right 250 Hz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B2R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_2_RIGHT_DB, "EQ Right 500 Hz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B3R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_3_RIGHT_DB, "EQ Right 1 KHz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B4R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_4_RIGHT_DB, "EQ Right 2 KHz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B5R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_5_RIGHT_DB, "EQ Right 4 KHz band gain (dB) for third level");
		RegisterParameter(definition, "DEQL2B6R", "dB", MIN_BANDGAINDB, MAX_BANDGAINDB, DEFAULT_BANDGAINDB, 1.0f, 1.0f, PARAM_DYNAMICEQ_LEVEL_2_BAND_6_RIGHT_DB, "EQ Right 8 KHz band gain (dB) for third level");
		RegisterParameter(definition, "ATREL", "ms", MIN_ATTACKRELEASE, MAX_ATTACKRELEASE, DEFAULT_ATTACKRELEASE, 1.0f, 1.0f, PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS, "Left attack/release (ms)");
		RegisterParameter(definition, "ATRER", "ms", MIN_ATTACKRELEASE, MAX_ATTACKRELEASE, DEFAULT_ATTACKRELEASE, 1.0f, 1.0f, PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS, "Right attack/release (ms)");
		
		// Quantization noise
		RegisterParameter(definition, "NOISEBEF", "", F_FALSE, F_TRUE, DEFAULT_NOISEBEFORE, 1.0f, 1.0f, PARAM_NOISE_BEFORE_ON, "Apply quantization noise On/Off at the start of the process chain");
		RegisterParameter(definition, "NOISEAFT", "", F_FALSE, F_TRUE, DEFAULT_NOISEAFTER, 1.0f, 1.0f, PARAM_NOISE_AFTER_ON, "Apply quantization noise On/Off at the end of the process chain");
		RegisterParameter(definition, "NOISEBITS", "", MIN_NOISENUMBITS, MAX_NOISENUMBITS, DEFAULT_NOISENUMBITS, 1.0f, 1.0f, PARAM_NOISE_NUMBITS, "Number of bits of quantization noise");	

		// Simplified controls
		RegisterParameter(definition, "COMPRL", "%", 0.0f, MAX_COMPRESSION_PERCENTAGE, DEFAULT_COMPRESSION_PERCENTAGE, 1.0f, 1.0f, PARAM_COMPRESSION_PERCENTAGE_LEFT, "Amount of compression, Left");
		RegisterParameter(definition, "COMPRR", "%", 0.0f, MAX_COMPRESSION_PERCENTAGE, DEFAULT_COMPRESSION_PERCENTAGE, 1.0f, 1.0f, PARAM_COMPRESSION_PERCENTAGE_RIGHT, "Amount of compression, Right");

		// Limiter
		RegisterParameter(definition, "LIMITON", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_LIMITER_SET_ON, "Limiter enabler for HA");
		RegisterParameter(definition, "LIMITGET", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_LIMITER_GET_COMPRESSION, "Is HA limiter compressing?");

		// Normalization
		RegisterParameter(definition, "NORMONL", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_NORMALIZATION_LEFT_ON, "Normalization enabler for Left ear");
		RegisterParameter(definition, "NORMDBL", "dB", MIN_NORMALIZATION, MAX_NORMALIZATION, DEFAULT_NORMALIZATION, 1.0f, 1.0f, PARAM_NORMALIZATION_LEFT_DBS, "Amount of normalization (in dBs), Left");
		RegisterParameter(definition, "NORMGL", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_NORMALIZATION_LEFT_GET, "Is left normalization applying offset?");
		RegisterParameter(definition, "NORMONR", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_NORMALIZATION_RIGHT_ON, "Normalization enabler for Right ear");
		RegisterParameter(definition, "NORMDBR", "dB", MIN_NORMALIZATION, MAX_NORMALIZATION, DEFAULT_NORMALIZATION, 1.0f, 1.0f, PARAM_NORMALIZATION_RIGHT_DBS, "Amount of normalization (in dBs), Right");
		RegisterParameter(definition, "NORMGR", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_NORMALIZATION_RIGHT_GET, "Is right normalization applying offset?");

		// Tone control
		RegisterParameter(definition, "TONLOL", "dB", MIN_TONECONTROL, MAX_TONECONTROL, 0.0f, 1.0f, 1.0f, PARAM_TONE_LOW_LEFT, "Left tone control for low band (dB)");
		RegisterParameter(definition, "TONMIL", "dB", MIN_TONECONTROL, MAX_TONECONTROL, 0.0f, 1.0f, 1.0f, PARAM_TONE_MID_LEFT, "Left tone control for mid band (dB)");
		RegisterParameter(definition, "TONHIL", "dB", MIN_TONECONTROL, MAX_TONECONTROL, 0.0f, 1.0f, 1.0f, PARAM_TONE_HIGH_LEFT, "Left tone control for high band (dB)");
		RegisterParameter(definition, "TONLOR", "dB", MIN_TONECONTROL, MAX_TONECONTROL, 0.0f, 1.0f, 1.0f, PARAM_TONE_LOW_RIGHT, "Right tone control for low band (dB)");
		RegisterParameter(definition, "TONMIR", "dB", MIN_TONECONTROL, MAX_TONECONTROL, 0.0f, 1.0f, 1.0f, PARAM_TONE_MID_RIGHT, "Right tone control for mid band (dB)");
		RegisterParameter(definition, "TONHIR", "dB", MIN_TONECONTROL, MAX_TONECONTROL, 0.0f, 1.0f, 1.0f, PARAM_TONE_HIGH_RIGHT, "Right tone control for high band (dB)");

		// Debug log
		//RegisterParameter(definition, "DebugLogHA", "", 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, PARAM_DEBUG_LOG, "Generate debug log for HA");

		//// Fig6
		//RegisterParameter(definition, "FIG60L", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_0_LEFT, "Fig6 input band 0 Left");
		//RegisterParameter(definition, "FIG61L", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_1_LEFT, "Fig6 input band 1 Left");
		//RegisterParameter(definition, "FIG62L", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_2_LEFT, "Fig6 input band 2 Left");
		//RegisterParameter(definition, "FIG63L", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_3_LEFT, "Fig6 input band 3 Left");
		//RegisterParameter(definition, "FIG64L", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_4_LEFT, "Fig6 input band 4 Left");
		//RegisterParameter(definition, "FIG65L", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_5_LEFT, "Fig6 input band 5 Left");
		//RegisterParameter(definition, "FIG66L", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_6_LEFT, "Fig6 input band 6 Left");
		//RegisterParameter(definition, "FIG60R", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_0_RIGHT, "Fig6 input band 0 Right");
		//RegisterParameter(definition, "FIG61R", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_1_RIGHT, "Fig6 input band 1 Right");
		//RegisterParameter(definition, "FIG62R", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_2_RIGHT, "Fig6 input band 2 Right");
		//RegisterParameter(definition, "FIG63R", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_3_RIGHT, "Fig6 input band 3 Right");
		//RegisterParameter(definition, "FIG64R", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_4_RIGHT, "Fig6 input band 4 Right");
		//RegisterParameter(definition, "FIG65R", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_5_RIGHT, "Fig6 input band 5 Right");
		//RegisterParameter(definition, "FIG66R", "dB", MIN_FIG6, MAX_FIG6, 0.0f, 1.0f, 1.0f, PARAM_FIG6_BAND_6_RIGHT, "Fig6 input band 6 Right");

        return numparams;
    }

	/////////////////////////////////////////////////////////////////////	

	float FromDBToGain(float dbvalue)
	{
		return pow(10.0f, dbvalue / 20.0f);
	}

	/////////////////////////////////////////////////////////////////////

	bool FromFloatToBool(float fvalue)
	{
		return (fvalue == F_TRUE);
	}

	/////////////////////////////////////////////////////////////////////

	string FromBoolToOnOffStr(bool bvalue)
	{
		if (bvalue)
			return "On";
		else
			return "Off";
	}

	/////////////////////////////////////////////////////////////////////

	void AddToBand(UnityAudioEffectState* state, T_ear ear, int eqband, int toneband, float newIncrement)
	{		
		EffectData* data = state->GetEffectData<EffectData>();

		// Go through all dynamic eq levels        
		for (int level=0; level < DEFAULT_NUMLEVELS; level++)
		{
			// Get old increment and current value
			float currentValue;
			float oldIncrement;
			if (ear == T_ear::LEFT)
			{				
				oldIncrement = data->toneLeft[toneband];
				currentValue = data->HA.GetLeftDynamicEqualizer()->GetLevelBandGain_dB(level, eqband);
			}
			else
			{
				oldIncrement = data->toneRight[toneband];
				currentValue = data->HA.GetRightDynamicEqualizer()->GetLevelBandGain_dB(level, eqband);
			}

			// Apply old increment to current value
			currentValue = currentValue - oldIncrement;

			// Set band gain with increment
			if (ear == T_ear::LEFT)
				data->HA.GetLeftDynamicEqualizer()->SetLevelBandGain_dB(level, eqband, currentValue + newIncrement);
			else
				data->HA.GetRightDynamicEqualizer()->SetLevelBandGain_dB(level, eqband, currentValue + newIncrement);
		}
	}

	/////////////////////////////////////////////////////////////////////

	void SetTone(UnityAudioEffectState* state, T_ear ear, int band, float value)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		switch (band)
		{
			case BAND_LOW:
				AddToBand(state, ear, 0, band, value);
				AddToBand(state, ear, 1, band, value);
				AddToBand(state, ear, 2, band, value);
				break;
			case BAND_MID:
				AddToBand(state, ear, 3, band, value);
				AddToBand(state, ear, 4, band, value);
				break;
			case BAND_HIGH:
				AddToBand(state, ear, 5, band, value);
				AddToBand(state, ear, 6, band, value);
				break;
		}

		if (ear == T_ear::LEFT)
			data->toneLeft[band] = value;
		else
			data->toneRight[band] = value;
	}

	/////////////////////////////////////////////////////////////////////

	void WriteLogHeader(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		// EQ:
		WriteLog(state, "CREATE: EQ setup:", "");
		WriteLog(state, "        Sample rate = ", state->samplerate);
		WriteLog(state, "        Number of levels = ", DEFAULT_NUMLEVELS);
		WriteLog(state, "        Number of bands = ", DEFAULT_BANDSNUMBER);
		WriteLog(state, "        Initial frequency = ", DEFAULT_INIFREQ);
		WriteLog(state, "        Octave step = 1/", DEFAULT_OCTAVEBANDSTEP);
		WriteLog(state, "        Q factor of LPF = ", DEFAULT_QLPF);
		WriteLog(state, "        Q factor of BPFs = ", DEFAULT_QBPF);
		WriteLog(state, "        Q factor of HPF = ", DEFAULT_QHPF);
		WriteLog(state, "        LPF cutoff = ", DEFAULT_LPFCUTOFF);
		WriteLog(state, "        HPF cutoff = ", DEFAULT_HPFCUTOFF);
		// TO DO: Add limiter and normalization

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
		// TO DO: add more WriteLog
		
		//effectdata->debugLog = true;

		//effectdata->haReady = false;

		// Setup HA
		effectdata->HA.Setup(state->samplerate, DEFAULT_NUMLEVELS, DEFAULT_INIFREQ, DEFAULT_BANDSNUMBER, DEFAULT_OCTAVEBANDSTEP,
												DEFAULT_LPFCUTOFF, DEFAULT_HPFCUTOFF, DEFAULT_QLPF, DEFAULT_QBPF, DEFAULT_QHPF);
		//WriteLog(state, "CREATE: EQ setup:", "");
		//WriteLog(state, "        Sample rate = ", state->samplerate);
		//WriteLog(state, "        Number of levels = ", DEFAULT_NUMLEVELS);
		//WriteLog(state, "        Number of bands = ", DEFAULT_BANDSNUMBER);
		//WriteLog(state, "        Initial frequency = ", DEFAULT_INIFREQ);		
		//WriteLog(state, "        Octave step = 1/", DEFAULT_OCTAVEBANDSTEP);
		//WriteLog(state, "        Q factor of LPF = ", DEFAULT_QLPF);
		//WriteLog(state, "        Q factor of BPFs = ", DEFAULT_QBPF);
		//WriteLog(state, "        Q factor of HPF = ", DEFAULT_QHPF);
		//WriteLog(state, "        LPF cutoff = ", DEFAULT_LPFCUTOFF);
		//WriteLog(state, "        HPF cutoff = ", DEFAULT_HPFCUTOFF);
		
		// Setup HA switches and default values
		effectdata->HA.volL = FromDBToGain(DEFAULT_VOLDB);	// TO DO: writelog
		effectdata->HA.volR = FromDBToGain(DEFAULT_VOLDB);	// TO DO: writelog
		effectdata->HA.addNoiseBefore = FromFloatToBool(DEFAULT_NOISEBEFORE); // TO DO: writelog
		effectdata->HA.addNoiseAfter = FromFloatToBool(DEFAULT_NOISEAFTER); // TO DO: writelog
		effectdata->HA.noiseNumBits = DEFAULT_NOISENUMBITS;		// TO DO: writelog
		effectdata->HA.GetLeftDynamicEqualizer()->SetLevelsInterpolation(DEFAULT_LEVELSINTERPOLATION);	// TO DO: writelog
		effectdata->HA.GetRightDynamicEqualizer()->SetLevelsInterpolation(DEFAULT_LEVELSINTERPOLATION);	// TO DO: writelog
		effectdata->HA.GetLeftDynamicEqualizer()->SetAttackRelease_ms(DEFAULT_ATTACKRELEASE);
		effectdata->HA.GetRightDynamicEqualizer()->SetAttackRelease_ms(DEFAULT_ATTACKRELEASE);
		effectdata->HA.GetLeftDynamicEqualizer()->SetCompressionPercentage(DEFAULT_COMPRESSION_PERCENTAGE);
		effectdata->HA.GetRightDynamicEqualizer()->SetCompressionPercentage(DEFAULT_COMPRESSION_PERCENTAGE);

		// Setup band gains (TO DO: writelog)
		for (int level = 0; level < DEFAULT_NUMLEVELS; level++)
		{
			for (int band = 0; band < DEFAULT_BANDSNUMBER; band++)
			{
				effectdata->HA.SetLevelBandGain_dB(level, band, DEFAULT_BANDGAINDB, EAR_LEFT);
				effectdata->HA.SetLevelBandGain_dB(level, band, DEFAULT_BANDGAINDB, EAR_RIGHT);
			}
		}

		// Setup level thresholds (TO DO: Write log)
		effectdata->HA.SetLevelThreshold(0, DEFAULT_LEVELTHRESHOLD_0, EAR_LEFT);
		effectdata->HA.SetLevelThreshold(0, DEFAULT_LEVELTHRESHOLD_0, EAR_RIGHT);
		effectdata->HA.SetLevelThreshold(1, DEFAULT_LEVELTHRESHOLD_1, EAR_LEFT);
		effectdata->HA.SetLevelThreshold(1, DEFAULT_LEVELTHRESHOLD_1, EAR_RIGHT);
		effectdata->HA.SetLevelThreshold(2, DEFAULT_LEVELTHRESHOLD_2, EAR_LEFT);
		effectdata->HA.SetLevelThreshold(2, DEFAULT_LEVELTHRESHOLD_2, EAR_RIGHT);	

		// New setup parameters
		effectdata->HA.GetLeftDynamicEqualizer()->SetMaxGain_dB(MAX_BANDGAINDB);
		effectdata->HA.GetLeftDynamicEqualizer()->SetMinGain_dB(MIN_BANDGAINDB);		
		effectdata->HA.GetRightDynamicEqualizer()->SetMaxGain_dB(MAX_BANDGAINDB);
		effectdata->HA.GetRightDynamicEqualizer()->SetMinGain_dB(MIN_BANDGAINDB);

		// Setup limiter
		effectdata->limiter.Setup(state->samplerate, HA_LIMITER_RATIO, HA_LIMITER_THRESHOLD, HA_LIMITER_ATTACK, HA_LIMITER_RELEASE);		

		// Setup normalization
		effectdata->HA.DisableNormalization(BOTH);		

		// Tone control
		for (int i = 0; i < TONE_BANDS; i++)
		{
			effectdata->toneLeft[i] = 0.0f;
			effectdata->toneRight[i] = 0.0f;
		}

		//// Configure Fig6
		//effectdata->settingFig6Left = false;
		//effectdata->fig6ReceivedBandsLeft = 0;
		//effectdata->settingFig6Right = false;
		//effectdata->fig6ReceivedBandsRight = 0;

		WriteLog(state, "CREATE: HA Simulation plugin created", "");		

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
		// TO DO: improve writelog

        EffectData* data = state->GetEffectData<EffectData>();
        if (index >= P_NUM)
            return UNITY_AUDIODSP_ERR_UNSUPPORTED;
        data->parameters[index] = value;
		//WriteLog(state, "SET PARAMETER: ", "");
		//WriteLog(state, "               Index = ", index);
		//WriteLog(state, "               Value = ", value);		

		// Process command sent by C# API
		// TO DO: Check errors with debugger, incorrect values...
		switch (index)
		{		
			// Global parameters
			case PARAM_PROCESS_LEFT_ON: 
				WriteLog(state, "SET PARAMETER: Left HA switched ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;
			case PARAM_PROCESS_RIGHT_ON: 
				WriteLog(state, "SET PARAMETER: Right HA switched ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;
			case PARAM_VOLUME_L_DB: 
				data->HA.volL = FromDBToGain(value); 
				WriteLog(state, "SET PARAMETER: Left volume set to (dB): ", value);
				break;
			case PARAM_VOLUME_R_DB:	
				data->HA.volR = FromDBToGain(value); 
				WriteLog(state, "SET PARAMETER: Right volume set to (dB): ", value);
				break;

			// Common switches and values for EQ			
			case PARAM_EQ_LPFCUTOFF_HZ:	
				data->HA.ConfigLPF(value, DEFAULT_QLPF); 
				WriteLog(state, "SET PARAMETER: Low pass filter cutoff frequency set to: ", value);
				break;
			case PARAM_EQ_HPFCUTOFF_HZ:	
				data->HA.ConfigHPF(value, DEFAULT_QHPF); 
				WriteLog(state, "SET PARAMETER: High pass filter cutoff frequency set to: ", value);
				break;

			// Dynamic EQ
			case PARAM_DYNAMICEQ_INTERPOLATION_ON:				
				data->HA.GetLeftDynamicEqualizer()->SetLevelsInterpolation(FromFloatToBool(value));	
				data->HA.GetRightDynamicEqualizer()->SetLevelsInterpolation(FromFloatToBool(value));
				WriteLog(state, "SET PARAMETER: Levels interpolation in dynamic equalizer set to ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_0_LEFT_DBFS:	
				data->HA.SetLevelThreshold(0, value, EAR_LEFT);			
				WriteLog(state, "SET PARAMETER: First threshold for Left channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_1_LEFT_DBFS:	
				data->HA.SetLevelThreshold(1, value, EAR_LEFT);			
				WriteLog(state, "SET PARAMETER: Second threshold for Left channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_2_LEFT_DBFS:	
				data->HA.SetLevelThreshold(2, value, EAR_LEFT);			
				WriteLog(state, "SET PARAMETER: Third threshold for Left channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_0_RIGHT_DBFS:	
				data->HA.SetLevelThreshold(0, value, EAR_RIGHT);		
				WriteLog(state, "SET PARAMETER: First threshold for Right channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_1_RIGHT_DBFS:	
				data->HA.SetLevelThreshold(1, value, EAR_RIGHT);		
				WriteLog(state, "SET PARAMETER: Second threshold for Right channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_2_RIGHT_DBFS:	
				data->HA.SetLevelThreshold(2, value, EAR_RIGHT);		
				WriteLog(state, "SET PARAMETER: Third threshold for Right channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_0_LEFT_DB:		data->HA.SetLevelBandGain_dB(0, 0, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 0, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_1_LEFT_DB:		data->HA.SetLevelBandGain_dB(0, 1, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 1, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_2_LEFT_DB:		data->HA.SetLevelBandGain_dB(0, 2, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 2, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_3_LEFT_DB:		data->HA.SetLevelBandGain_dB(0, 3, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 3, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_4_LEFT_DB:		data->HA.SetLevelBandGain_dB(0, 4, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 4, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_5_LEFT_DB:		data->HA.SetLevelBandGain_dB(0, 5, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 5, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_6_LEFT_DB:		data->HA.SetLevelBandGain_dB(0, 6, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 6, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_0_LEFT_DB:		data->HA.SetLevelBandGain_dB(1, 0, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 0, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_1_LEFT_DB:		data->HA.SetLevelBandGain_dB(1, 1, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 1, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_2_LEFT_DB:		data->HA.SetLevelBandGain_dB(1, 2, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 2, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_3_LEFT_DB:		data->HA.SetLevelBandGain_dB(1, 3, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 3, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_4_LEFT_DB:		data->HA.SetLevelBandGain_dB(1, 4, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 4, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_5_LEFT_DB:		data->HA.SetLevelBandGain_dB(1, 5, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 5, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_6_LEFT_DB:		data->HA.SetLevelBandGain_dB(1, 6, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 6, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_0_LEFT_DB:		data->HA.SetLevelBandGain_dB(2, 0, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 0, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_1_LEFT_DB:		data->HA.SetLevelBandGain_dB(2, 1, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 1, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_2_LEFT_DB:		data->HA.SetLevelBandGain_dB(2, 2, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 2, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_3_LEFT_DB:		data->HA.SetLevelBandGain_dB(2, 3, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 3, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_4_LEFT_DB:		data->HA.SetLevelBandGain_dB(2, 4, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 4, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_5_LEFT_DB:		data->HA.SetLevelBandGain_dB(2, 5, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 5, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_6_LEFT_DB:		data->HA.SetLevelBandGain_dB(2, 6, value, EAR_LEFT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 6, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_0_RIGHT_DB:		data->HA.SetLevelBandGain_dB(0, 0, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 0, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_1_RIGHT_DB:		data->HA.SetLevelBandGain_dB(0, 1, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 1, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_2_RIGHT_DB:		data->HA.SetLevelBandGain_dB(0, 2, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 2, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_3_RIGHT_DB:		data->HA.SetLevelBandGain_dB(0, 3, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 3, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_4_RIGHT_DB:		data->HA.SetLevelBandGain_dB(0, 4, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 4, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_5_RIGHT_DB:		data->HA.SetLevelBandGain_dB(0, 5, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 5, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_6_RIGHT_DB:		data->HA.SetLevelBandGain_dB(0, 6, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 6, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_0_RIGHT_DB:		data->HA.SetLevelBandGain_dB(1, 0, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 0, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_1_RIGHT_DB:		data->HA.SetLevelBandGain_dB(1, 1, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 1, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_2_RIGHT_DB:		data->HA.SetLevelBandGain_dB(1, 2, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 2, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_3_RIGHT_DB:		data->HA.SetLevelBandGain_dB(1, 3, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 3, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_4_RIGHT_DB:		data->HA.SetLevelBandGain_dB(1, 4, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 4, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_5_RIGHT_DB:		data->HA.SetLevelBandGain_dB(1, 5, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 5, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_6_RIGHT_DB:		data->HA.SetLevelBandGain_dB(1, 6, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 6, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_0_RIGHT_DB:		data->HA.SetLevelBandGain_dB(2, 0, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 0, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_1_RIGHT_DB:		data->HA.SetLevelBandGain_dB(2, 1, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 1, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_2_RIGHT_DB:		data->HA.SetLevelBandGain_dB(2, 2, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 2, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_3_RIGHT_DB:		data->HA.SetLevelBandGain_dB(2, 3, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 3, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_4_RIGHT_DB:		data->HA.SetLevelBandGain_dB(2, 4, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 4, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_5_RIGHT_DB:		data->HA.SetLevelBandGain_dB(2, 5, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 5, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_6_RIGHT_DB:		data->HA.SetLevelBandGain_dB(2, 6, value, EAR_RIGHT);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 6, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS:			
				//data->HA.GetDynamicEqualizer()->EnvL.SetAttackTime(value); 
				//data->HA.GetDynamicEqualizer()->EnvL.SetReleaseTime(value);
				data->HA.GetLeftDynamicEqualizer()->SetAttackRelease_ms(value);
				WriteLog(state, "SET PARAMETER: Attack/Release time for Left channel = ", value);
				break;
			case PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS:		
				//data->HA.GetDynamicEqualizer()->EnvR.SetAttackTime(value);
				//data->HA.GetDynamicEqualizer()->EnvR.SetReleaseTime(value);
				data->HA.GetRightDynamicEqualizer()->SetAttackRelease_ms(value);
				WriteLog(state, "SET PARAMETER: Attack/Release time for Right channel = ", value);
				break;

			// Quantization noise
			case PARAM_NOISE_BEFORE_ON:	
				data->HA.addNoiseBefore = FromFloatToBool(value); 
				WriteLog(state, "SET PARAMETER: Quantization noise Before EQ is ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;
			case PARAM_NOISE_AFTER_ON:	
				data->HA.addNoiseAfter = FromFloatToBool(value); 
				WriteLog(state, "SET PARAMETER: Quantization noise After EQ is ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;
			case PARAM_NOISE_NUMBITS:	
				data->HA.noiseNumBits = (int)value; 
				WriteLog(state, "SET PARAMETER: Quantization noise number of bits = ", (int)value);
				break;	

			// Simplified controls
			case PARAM_COMPRESSION_PERCENTAGE_LEFT: 
				data->HA.GetLeftDynamicEqualizer()->SetCompressionPercentage(value); 
				WriteLog(state, "SET PARAMETER: Compression amount for Left channel = ", value);
				break;
			case PARAM_COMPRESSION_PERCENTAGE_RIGHT: 
				data->HA.GetRightDynamicEqualizer()->SetCompressionPercentage(value); 
				WriteLog(state, "SET PARAMETER: Compression amount for Right channel = ", value);
				//data->haReady = true;
				//WriteLog(state, "Hearing Aid Simulation is ready to Process!", "");
				break;

			case PARAM_LIMITER_SET_ON:
				if (FromFloatToBool(value))
				{
					WriteLog(state, "SET PARAMETER: Limiter switched ON", "");
				}
				else
				{
					WriteLog(state, "SET PARAMETER: Limiter switched OFF", "");
				}
				break;

			case PARAM_LIMITER_GET_COMPRESSION:
				WriteLog(state, "SET PARAMETER: WARNING! PARAM_LIMIT_GET_COMPRESSION is read only", "");
				break;

			case PARAM_NORMALIZATION_LEFT_ON:
				if (FromFloatToBool(value))
					data->HA.EnableNormalization(LEFT);
				else
					data->HA.DisableNormalization(LEFT);
				WriteLog(state, "SET PARAMETER: Normalization in Left ear switched ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;

			case PARAM_NORMALIZATION_LEFT_DBS:
				data->HA.SetNormalizationLevel(LEFT, value);
				WriteLog(state, "SET PARAMETER: Normalization in Left ear set to level: ", value);
				break;

			case PARAM_NORMALIZATION_LEFT_GET:
				WriteLog(state, "SET PARAMETER: WARNING! PARAM_NORMALIZATION_LEFT_GET is read only", "");
				break;

			case PARAM_NORMALIZATION_RIGHT_ON:
				if (FromFloatToBool(value))
					data->HA.EnableNormalization(RIGHT);
				else
					data->HA.DisableNormalization(RIGHT);
				WriteLog(state, "SET PARAMETER: Normalization in Right ear switched ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;

			case PARAM_NORMALIZATION_RIGHT_DBS:
				data->HA.SetNormalizationLevel(RIGHT, value);
				WriteLog(state, "SET PARAMETER: Normalization in Right ear set to level: ", value);
				break;

			case PARAM_NORMALIZATION_RIGHT_GET:
				WriteLog(state, "SET PARAMETER: WARNING! PARAM_NORMALIZATION_RIGHT_GET is read only", "");
				break;

			case PARAM_TONE_LOW_LEFT:
				SetTone(state, T_ear::LEFT, BAND_LOW, value);
				WriteLog(state, "SET PARAMETER: Low tone band Left set to (dB) ", value);
				break;

			case PARAM_TONE_MID_LEFT:
				SetTone(state, T_ear::LEFT, BAND_MID, value);
				WriteLog(state, "SET PARAMETER: Mid tone band Left set to (dB) ", value);
				break;

			case PARAM_TONE_HIGH_LEFT:
				SetTone(state, T_ear::LEFT, BAND_HIGH, value);
				WriteLog(state, "SET PARAMETER: HIgh tone band Left set to (dB) ", value);
				break;

			case PARAM_TONE_LOW_RIGHT:
				SetTone(state, T_ear::RIGHT, BAND_LOW, value);
				WriteLog(state, "SET PARAMETER: Low tone band Right set to (dB) ", value);
				break;

			case PARAM_TONE_MID_RIGHT:
				SetTone(state, T_ear::RIGHT, BAND_MID, value);
				WriteLog(state, "SET PARAMETER: Mid tone band Right set to (dB) ", value);
				break;

			case PARAM_TONE_HIGH_RIGHT:
				SetTone(state, T_ear::RIGHT, BAND_HIGH, value);
				WriteLog(state, "SET PARAMETER: High tone band Right set to (dB) ", value);
				break;

			//case PARAM_DEBUG_LOG:
				//if (value != 0.0f)
				//{
				//	data->debugLog = true;
				//	WriteLogHeader(state);
				//}
				//else
				//	data->debugLog = false;				
				//break;

			//// Fig6
			//case PARAM_FIG6_BAND_0_LEFT: 
			//case PARAM_FIG6_BAND_1_LEFT:
			//case PARAM_FIG6_BAND_2_LEFT:
			//case PARAM_FIG6_BAND_3_LEFT:
			//case PARAM_FIG6_BAND_4_LEFT:
			//case PARAM_FIG6_BAND_5_LEFT:
			//case PARAM_FIG6_BAND_6_LEFT:
			//	data->settingFig6Left = true;
			//	data->fig6ReceivedBandsLeft++;
			//	if (data->fig6ReceivedBandsLeft == DEFAULT_FIG6_BANDS_PER_EAR)
			//	{
			//		vector<float> fig6InputVectorLeft;
			//		fig6InputVectorLeft.push_back(data->parameters[PARAM_FIG6_BAND_0_LEFT]);
			//		fig6InputVectorLeft.push_back(data->parameters[PARAM_FIG6_BAND_1_LEFT]);
			//		fig6InputVectorLeft.push_back(data->parameters[PARAM_FIG6_BAND_2_LEFT]);
			//		fig6InputVectorLeft.push_back(data->parameters[PARAM_FIG6_BAND_3_LEFT]);
			//		fig6InputVectorLeft.push_back(data->parameters[PARAM_FIG6_BAND_4_LEFT]);
			//		fig6InputVectorLeft.push_back(data->parameters[PARAM_FIG6_BAND_5_LEFT]);
			//		fig6InputVectorLeft.push_back(data->parameters[PARAM_FIG6_BAND_6_LEFT]);
			//		data->HA.ApplyFig6Alg(fig6InputVectorLeft, DEFAULT_DBSPL_FOR_0DBS, EAR_LEFT);
			//		WriteLog(state, "Fig6 method applied to Left HA", "");
			//		data->settingFig6Left = false;
			//		data->fig6ReceivedBandsLeft = 0;
			//	}
			//	break;

			//case PARAM_FIG6_BAND_0_RIGHT:
			//case PARAM_FIG6_BAND_1_RIGHT:
			//case PARAM_FIG6_BAND_2_RIGHT:
			//case PARAM_FIG6_BAND_3_RIGHT:
			//case PARAM_FIG6_BAND_4_RIGHT:
			//case PARAM_FIG6_BAND_5_RIGHT:
			//case PARAM_FIG6_BAND_6_RIGHT:
			//	data->settingFig6Right = true;
			//	data->fig6ReceivedBandsRight++;
			//	if (data->fig6ReceivedBandsRight == DEFAULT_FIG6_BANDS_PER_EAR)
			//	{
			//		vector<float> fig6InputVectorRight;
			//		fig6InputVectorRight.push_back(data->parameters[PARAM_FIG6_BAND_0_RIGHT]);
			//		fig6InputVectorRight.push_back(data->parameters[PARAM_FIG6_BAND_1_RIGHT]);
			//		fig6InputVectorRight.push_back(data->parameters[PARAM_FIG6_BAND_2_RIGHT]);
			//		fig6InputVectorRight.push_back(data->parameters[PARAM_FIG6_BAND_3_RIGHT]);
			//		fig6InputVectorRight.push_back(data->parameters[PARAM_FIG6_BAND_4_RIGHT]);
			//		fig6InputVectorRight.push_back(data->parameters[PARAM_FIG6_BAND_5_RIGHT]);
			//		fig6InputVectorRight.push_back(data->parameters[PARAM_FIG6_BAND_6_RIGHT]);
			//		data->HA.ApplyFig6Alg(fig6InputVectorRight, DEFAULT_DBSPL_FOR_0DBS, EAR_RIGHT);
			//		WriteLog(state, "Fig6 method applied to Right HA", "");
			//		data->settingFig6Right = false;
			//		data->fig6ReceivedBandsRight = 0;
			//	}
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
				case PARAM_LIMITER_GET_COMPRESSION:
					if (data->limiter.GetCompression())
						*value = 1.0f;
					else
						*value = 0.0f;					
					break;

				case PARAM_NORMALIZATION_LEFT_GET:
					*value = data->HA.GetLeftDynamicEqualizer()->GetOveralOffset_dB();
					break;

				case PARAM_NORMALIZATION_RIGHT_GET:
					*value = data->HA.GetRightDynamicEqualizer()->GetOveralOffset_dB();
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

		// Before doing anything, check that HA is ready
		//if (!data->haReady)
		//{						
		//	memset(outbuffer, 0.0f, length * outchannels * sizeof(float));
		//	return UNITY_AUDIODSP_OK;
		//}

		// Transform input buffer
		CStereoBuffer<float> inStereoBuffer(length * outchannels);		
		for (int i = 0; i < length*outchannels; i++)
		{
			inStereoBuffer[i] = inbuffer[i]; 
		}

		// Process!!
		CStereoBuffer<float> outStereoBuffer(length * outchannels);		
		data->HA.Process(inStereoBuffer, outStereoBuffer, data->parameters[PARAM_PROCESS_LEFT_ON], data->parameters[PARAM_PROCESS_RIGHT_ON]);

		// Limiter
		if (data->parameters[PARAM_LIMITER_SET_ON] > 0.0f) 
		{		
			if ((bool)data->parameters[PARAM_PROCESS_LEFT_ON] || (bool)data->parameters[PARAM_PROCESS_RIGHT_ON])
				data->limiter.Process(outStereoBuffer);
		}

		// Transform output buffer			
		int i = 0;
		for (auto it = outStereoBuffer.begin(); it != outStereoBuffer.end(); it++)
		{
			outbuffer[i++] = *it;
		}

        return UNITY_AUDIODSP_OK;
    }
}
