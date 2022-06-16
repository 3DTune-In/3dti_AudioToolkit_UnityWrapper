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
*
* Updated: June 2020 onwards
* by Tim Murray-Browne at the Dyson School of Engineering, Imperial College London.
**/

#include "AudioPluginUtil.h"

#include <Common/DynamicCompressorStereo.h>
#include <HAHLSimulation/HearingAidSim.h>

// Includes for debug logging
#include <fstream>
#include <iostream>
#include <string>
#include <map>
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
		// NB ** Do not reorder. Add new parameters to end These enum values are referenced directly by HearingAid.cs in SetEQFromFig6 **
		PARAM_DYNAMICEQ_LEVEL_0_BAND_0_LEFT_DB, // 13
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
		PARAM_DYNAMICEQ_LEVEL_1_BAND_6_LEFT_DB, // 26
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

		// A readonly handle for this plugin instance used to refer to it when using static DLL functions
		PARAM_HANDLE,

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
		HAHLSimulation::CHearingAidSim HA;		

		// Limiter
		Common::CDynamicCompressorStereo limiter;		
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

	// map from PARAM_HANDLE -> plugin instance
	std::map<int, EffectData*> instances;
	std::mutex instancesMutex;

	/////////////////////////////////////////////////////////////////////
	extern "C" UNITY_AUDIODSP_EXPORT_API bool SetDynamicEqualizerUsingFig6(int effectHandle, Common::T_ear ear, float* earLosses, int earLossesSize, float dBs_SPL_for_0_dBs_fs)
	{
		assert(earLossesSize > 0);
		using namespace Common;
		
		vector<float> losses(earLosses, earLosses + (size_t)earLossesSize);
		std::lock_guard<std::mutex> lock(instancesMutex);
		if (instances.count(effectHandle) == 0)
		{
			return false;
		}
		EffectData* effect = instances.at(effectHandle);
		if (ear == LEFT || ear == BOTH)
		{
			if (effect->HA.GetDynamicEqualizer(LEFT)->GetNumLevels() != 3 || effect->HA.GetDynamicEqualizer(LEFT)->GetNumBands() != losses.size())
			{
				return false;
			}
		}
		if (ear == RIGHT || ear == BOTH)
		{
			if (effect->HA.GetDynamicEqualizer(RIGHT)->GetNumLevels() != 3 || effect->HA.GetDynamicEqualizer(RIGHT)->GetNumBands() != losses.size())
			{
				return false;
			}
		}
		if (ear != LEFT && ear != RIGHT && ear != BOTH)
		{
			return false;
		}
		//const int NumGainParameters = 2 * 3 * 7;
		//static_assert((PARAM_DYNAMICEQ_LEVEL_2_BAND_6_RIGHT_DB + 1) - PARAM_DYNAMICEQ_LEVEL_0_BAND_0_LEFT_DB == NumGainParameters, "Code assumes gain parameters are contiguous");
		//if (NumGainParameters != out_calculatedGainsLength)
		//{
		//	return false;
		//}


		effect->HA.SetDynamicEqualizerUsingFig6(ear, losses, dBs_SPL_for_0_dBs_fs);

		// update param mirror in EffectData
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_0_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(0, 0);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_1_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(0, 1);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_2_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(0, 2);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_3_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(0, 3);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_4_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(0, 4);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_5_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(0, 5);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_6_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(0, 6);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_0_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(1, 0);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_1_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(1, 1);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_2_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(1, 2);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_3_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(1, 3);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_4_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(1, 4);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_5_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(1, 5);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_6_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(1, 6);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_0_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(2, 0);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_1_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(2, 1);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_2_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(2, 2);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_3_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(2, 3);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_4_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(2, 4);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_5_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(2, 5);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_6_LEFT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(2, 6);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_0_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(0, 0);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_1_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(0, 1);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_2_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(0, 2);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_3_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(0, 3);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_4_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(0, 4);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_5_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(0, 5);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_6_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(0, 6);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_0_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(1, 0);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_1_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(1, 1);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_2_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(1, 2);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_3_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(1, 3);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_4_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(1, 4);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_5_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(1, 5);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_1_BAND_6_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(1, 6);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_0_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(2, 0);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_1_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(2, 1);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_2_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(2, 2);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_3_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(2, 3);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_4_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(2, 4);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_5_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(2, 5);
		effect->parameters[PARAM_DYNAMICEQ_LEVEL_2_BAND_6_RIGHT_DB] = effect->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(2, 6);

		//assert(NumGainParameters <= out_calculatedGainsLength);
		//for (int i = 0; i < NumGainParameters; i++)
		//{
		//	out_calculatedGains[i] = effect->parameters[PARAM_DYNAMICEQ_LEVEL_0_BAND_0_LEFT_DB + i];
		//}

		return true;
	}

	// To bypass Unity caching of exposed mixer parameters, we need this so the c# code can grab the calculated fig6 values
	extern "C" UNITY_AUDIODSP_EXPORT_API bool GetHADynamicEqGain(int effectHandle, int level, int band, float* out_left, float* out_right)
	{
		if (level < 0 || level >= 3 || band < 0 || band >= 7)
		{
			return false;
		}
		std::lock_guard<std::mutex> lock(instancesMutex);
		if (instances.count(effectHandle) == 0)
		{
			return false;
		}
		*out_left = instances.at(effectHandle)->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(level, band);
		*out_right = instances.at(effectHandle)->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(level, band);
		return true;

	}


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

		// Handle
#define MAX_INTEGER_REPRESENTABLE_AS_FLOAT (std::numeric_limits<float>::radix / std::numeric_limits<float>::epsilon())
		RegisterParameter(definition, "HANDLE", "", 0, MAX_INTEGER_REPRESENTABLE_AS_FLOAT, 0.0f, 1.0f, 1.0f, PARAM_HANDLE, "Read-only handle identifying this plugin instance");


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

	void AddToBand(UnityAudioEffectState* state, Common::T_ear ear, int eqband, int toneband, float newIncrement)
	{		
		EffectData* data = state->GetEffectData<EffectData>();

		// Go through all dynamic eq levels        
		for (int level=0; level < DEFAULT_NUMLEVELS; level++)
		{
			// Get old increment and current value
			float currentValue;
			float oldIncrement;
			if (ear == Common::T_ear::LEFT)
			{				
				oldIncrement = data->toneLeft[toneband];
				currentValue = data->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetLevelBandGain_dB(level, eqband);
			}
			else
			{
				oldIncrement = data->toneRight[toneband];
				currentValue = data->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetLevelBandGain_dB(level, eqband);
			}

			// Apply old increment to current value
			currentValue = currentValue - oldIncrement;

			// Set band gain with increment
			if (ear == Common::T_ear::LEFT)
				data->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->SetLevelBandGain_dB(level, eqband, currentValue + newIncrement);
			else
				data->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->SetLevelBandGain_dB(level, eqband, currentValue + newIncrement);
		}
	}

	/////////////////////////////////////////////////////////////////////

	void SetTone(UnityAudioEffectState* state, Common::T_ear ear, int band, float value)
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

		if (ear == Common::T_ear::LEFT)
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

		{
			std::lock_guard<std::mutex> lock(instancesMutex);
			// Create handle
			int handle = 0;
			while (instances.count(handle) > 0)
			{
				handle++;
			}
			assert(instances.count(handle) == 0);
			effectdata->parameters[PARAM_HANDLE] = (float)handle;
			// check casting did not change value
			assert((int)effectdata->parameters[PARAM_HANDLE] == handle);
			instances[handle] = effectdata;
		}
		
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
		effectdata->HA.SetOverallGain(Common::T_ear::LEFT, FromDBToGain(DEFAULT_VOLDB));	// TO DO: writelog
		effectdata->HA.SetOverallGain(Common::T_ear::RIGHT, FromDBToGain(DEFAULT_VOLDB));	// TO DO: writelog
		effectdata->HA.DisableQuantizationBeforeEqualizer();	// TO DO: writelog (WARNING: this might not coincide with DEFAULT_NOISEBEFORE)
		effectdata->HA.DisableQuantizationAfterEqualizer(); // TO DO: writelog (WARNING: this might not coincide with DEFAULT_NOISEAFTER)
		effectdata->HA.SetQuantizationBits(DEFAULT_NOISENUMBITS);		// TO DO: writelog
		if (DEFAULT_LEVELSINTERPOLATION)
		{
			effectdata->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->EnableLevelsInterpolation();	// TO DO: writelog 
			effectdata->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->EnableLevelsInterpolation();	// TO DO: writelog 
		}
		else
		{
			effectdata->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->DisableLevelsInterpolation();	// TO DO: writelog 
			effectdata->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->DisableLevelsInterpolation();	// TO DO: writelog 
		}		
		effectdata->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->SetAttack_ms(DEFAULT_ATTACKRELEASE);
		effectdata->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->SetRelease_ms(DEFAULT_ATTACKRELEASE);
		effectdata->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->SetAttack_ms(DEFAULT_ATTACKRELEASE);
		effectdata->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->SetRelease_ms(DEFAULT_ATTACKRELEASE);
		effectdata->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->SetCompressionPercentage(DEFAULT_COMPRESSION_PERCENTAGE);
		effectdata->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->SetCompressionPercentage(DEFAULT_COMPRESSION_PERCENTAGE);

		// Setup band gains (TO DO: writelog)
		for (int level = 0; level < DEFAULT_NUMLEVELS; level++)
		{
			for (int band = 0; band < DEFAULT_BANDSNUMBER; band++)
			{
				effectdata->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, level, band, DEFAULT_BANDGAINDB);
				effectdata->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, level, band, DEFAULT_BANDGAINDB);
			}
		}

		// Setup level thresholds (TO DO: Write log)
		effectdata->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::LEFT, 0, DEFAULT_LEVELTHRESHOLD_0);
		effectdata->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::RIGHT, 0, DEFAULT_LEVELTHRESHOLD_0);
		effectdata->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::LEFT, 1, DEFAULT_LEVELTHRESHOLD_1);
		effectdata->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::RIGHT, 1, DEFAULT_LEVELTHRESHOLD_1);
		effectdata->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::LEFT, 2, DEFAULT_LEVELTHRESHOLD_2);
		effectdata->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::RIGHT, 2, DEFAULT_LEVELTHRESHOLD_2);	

		// New setup parameters
		effectdata->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->SetMaxGain_dB(MAX_BANDGAINDB);
		effectdata->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->SetMinGain_dB(MIN_BANDGAINDB);		
		effectdata->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->SetMaxGain_dB(MAX_BANDGAINDB);
		effectdata->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->SetMinGain_dB(MIN_BANDGAINDB);

		// Setup limiter
		effectdata->limiter.Setup(state->samplerate, HA_LIMITER_RATIO, HA_LIMITER_THRESHOLD, HA_LIMITER_ATTACK, HA_LIMITER_RELEASE);		

		// Setup normalization
		effectdata->HA.DisableNormalization(Common::T_ear::BOTH);

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
		{
			std::lock_guard<std::mutex> lock(instancesMutex);
			auto it = instances.find(data->parameters[PARAM_HANDLE]);
			assert(it != instances.end());
			assert(it->second == data);
			if (it != instances.end())
			{
				instances.erase(it);
			}
			assert(instances.find(data->parameters[PARAM_HANDLE]) == instances.end());
		}
        delete data;
        return UNITY_AUDIODSP_OK;
    }

	/////////////////////////////////////////////////////////////////////

    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
    {
		// TO DO: improve writelog

        EffectData* data = state->GetEffectData<EffectData>();
		if (index >= P_NUM)
		{
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}
		if (index == PARAM_HANDLE)
		{
			// if unity tries to set the HANDLE parameter then we just ignore it as it's readonly.
			return UNITY_AUDIODSP_OK;
		}
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
				if (FromFloatToBool(value))
					data->HA.EnableHearingAidSimulation(Common::T_ear::LEFT);
				else
					data->HA.DisableHearingAidSimulation(Common::T_ear::LEFT);
				WriteLog(state, "SET PARAMETER: Left HA switched ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;
			case PARAM_PROCESS_RIGHT_ON: 
				if (FromFloatToBool(value))
					data->HA.EnableHearingAidSimulation(Common::T_ear::RIGHT);
				else
					data->HA.DisableHearingAidSimulation(Common::T_ear::RIGHT);
				WriteLog(state, "SET PARAMETER: Right HA switched ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;
			case PARAM_VOLUME_L_DB: 
				data->HA.SetOverallGain(Common::T_ear::LEFT, FromDBToGain(value)); 
				WriteLog(state, "SET PARAMETER: Left volume set to (dB): ", value);
				break;
			case PARAM_VOLUME_R_DB:	
				data->HA.SetOverallGain(Common::T_ear::RIGHT, FromDBToGain(value));
				WriteLog(state, "SET PARAMETER: Right volume set to (dB): ", value);
				break;

			// Common switches and values for EQ			
			case PARAM_EQ_LPFCUTOFF_HZ:	
				data->HA.SetLowPassFilter(value, DEFAULT_QLPF); 
				WriteLog(state, "SET PARAMETER: Low pass filter cutoff frequency set to: ", value);
				break;
			case PARAM_EQ_HPFCUTOFF_HZ:	
				data->HA.SetHighPassFilter(value, DEFAULT_QHPF); 
				WriteLog(state, "SET PARAMETER: High pass filter cutoff frequency set to: ", value);
				break;

			// Dynamic EQ
			case PARAM_DYNAMICEQ_INTERPOLATION_ON:		
				if (FromFloatToBool(value))
				{
					data->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->EnableLevelsInterpolation();
					data->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->EnableLevelsInterpolation();
				}
				else
				{
					data->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->DisableLevelsInterpolation();
					data->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->DisableLevelsInterpolation();
				}
				WriteLog(state, "SET PARAMETER: Levels interpolation in dynamic equalizer set to ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_0_LEFT_DBFS:	
				data->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::LEFT, 0, value);
				WriteLog(state, "SET PARAMETER: First threshold for Left channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_1_LEFT_DBFS:	
				data->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::LEFT, 1, value);
				WriteLog(state, "SET PARAMETER: Second threshold for Left channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_2_LEFT_DBFS:	
				data->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::LEFT, 2, value);
				WriteLog(state, "SET PARAMETER: Third threshold for Left channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_0_RIGHT_DBFS:	
				data->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::RIGHT, 0, value);
				WriteLog(state, "SET PARAMETER: First threshold for Right channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_1_RIGHT_DBFS:	
				data->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::RIGHT, 1, value);
				WriteLog(state, "SET PARAMETER: Second threshold for Right channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVELTHRESHOLD_2_RIGHT_DBFS:	
				data->HA.SetDynamicEqualizerLevelThreshold(Common::T_ear::RIGHT, 2, value);
				WriteLog(state, "SET PARAMETER: Third threshold for Right channel = ", value);
				break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_0_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 0, 0, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 0, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_1_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 0, 1, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 1, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_2_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 0, 2, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 2, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_3_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 0, 3, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 3, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_4_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 0, 4, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 4, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_5_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 0, 5, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 5, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_6_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 0, 6, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 6, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_0_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 1, 0, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 0, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_1_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 1, 1, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 1, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_2_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 1, 2, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 2, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_3_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 1, 3, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 3, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_4_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 1, 4, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 4, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_5_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 1, 5, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 5, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_6_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 1, 6, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 6, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_0_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 2, 0, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 0, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_1_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 2, 1, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 1, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_2_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 2, 2, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 2, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_3_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 2, 3, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 3, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_4_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 2, 4, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 4, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_5_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 2, 5, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 5, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_6_LEFT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::LEFT, 2, 6, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 6, Left channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_0_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 0, 0, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 0, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_1_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 0, 1, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 1, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_2_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 0, 2, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 2, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_3_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 0, 3, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 3, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_4_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 0, 4, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 4, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_5_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 0, 5, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 5, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_0_BAND_6_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 0, 6, value);	WriteLog(state, "SET PARAMETER: Gain for Level 0, Band 6, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_0_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 1, 0, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 0, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_1_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 1, 1, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 1, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_2_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 1, 2, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 2, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_3_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 1, 3, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 3, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_4_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 1, 4, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 4, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_5_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 1, 5, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 5, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_1_BAND_6_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 1, 6, value);	WriteLog(state, "SET PARAMETER: Gain for Level 1, Band 6, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_0_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 2, 0, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 0, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_1_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 2, 1, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 1, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_2_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 2, 2, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 2, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_3_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 2, 3, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 3, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_4_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 2, 4, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 4, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_5_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 2, 5, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 5, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_LEVEL_2_BAND_6_RIGHT_DB:		data->HA.SetDynamicEqualizerBandGain_dB(Common::T_ear::RIGHT, 2, 6, value);	WriteLog(state, "SET PARAMETER: Gain for Level 2, Band 6, Right channel = ", value); break;
			case PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS:			
				data->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->SetAttack_ms(value);
				data->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->SetRelease_ms(value);
				WriteLog(state, "SET PARAMETER: Attack/Release time for Left channel = ", value);
				break;
			case PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS:		
				data->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->SetAttack_ms(value);
				data->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->SetRelease_ms(value);
				WriteLog(state, "SET PARAMETER: Attack/Release time for Right channel = ", value);
				break;

			// Quantization noise
			case PARAM_NOISE_BEFORE_ON:	
				if (FromFloatToBool(value))
					data->HA.EnableQuantizationBeforeEqualizer();
				else
					data->HA.DisableQuantizationBeforeEqualizer();				
				WriteLog(state, "SET PARAMETER: Quantization Before EQ is ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;
			case PARAM_NOISE_AFTER_ON:	
				if (FromFloatToBool(value))
					data->HA.EnableQuantizationAfterEqualizer();
				else
					data->HA.DisableQuantizationAfterEqualizer();				
				WriteLog(state, "SET PARAMETER: Quantization After EQ is ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;
			case PARAM_NOISE_NUMBITS:	
				data->HA.SetQuantizationBits((int)value); 
				WriteLog(state, "SET PARAMETER: Quantization number of bits = ", (int)value);
				break;	

			// Simplified controls
			case PARAM_COMPRESSION_PERCENTAGE_LEFT: 
				data->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->SetCompressionPercentage(value); 
				WriteLog(state, "SET PARAMETER: Compression amount for Left channel = ", value);
				break;
			case PARAM_COMPRESSION_PERCENTAGE_RIGHT: 
				data->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->SetCompressionPercentage(value); 
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
					data->HA.EnableNormalization(Common::T_ear::LEFT);
				else
					data->HA.DisableNormalization(Common::T_ear::LEFT);
				WriteLog(state, "SET PARAMETER: Normalization in Left ear switched ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;

			case PARAM_NORMALIZATION_LEFT_DBS:
				data->HA.SetNormalizationLevel(Common::T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: Normalization in Left ear set to level: ", value);
				break;

			case PARAM_NORMALIZATION_LEFT_GET:
				WriteLog(state, "SET PARAMETER: WARNING! PARAM_NORMALIZATION_LEFT_GET is read only", "");
				break;

			case PARAM_NORMALIZATION_RIGHT_ON:
				if (FromFloatToBool(value))
					data->HA.EnableNormalization(Common::T_ear::RIGHT);
				else
					data->HA.DisableNormalization(Common::T_ear::RIGHT);
				WriteLog(state, "SET PARAMETER: Normalization in Right ear switched ", FromBoolToOnOffStr(FromFloatToBool(value)));
				break;

			case PARAM_NORMALIZATION_RIGHT_DBS:
				data->HA.SetNormalizationLevel(Common::T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: Normalization in Right ear set to level: ", value);
				break;

			case PARAM_NORMALIZATION_RIGHT_GET:
				WriteLog(state, "SET PARAMETER: WARNING! PARAM_NORMALIZATION_RIGHT_GET is read only", "");
				break;

			case PARAM_TONE_LOW_LEFT:
				SetTone(state, Common::T_ear::LEFT, BAND_LOW, value);
				WriteLog(state, "SET PARAMETER: Low tone band Left set to (dB) ", value);
				break;

			case PARAM_TONE_MID_LEFT:
				SetTone(state, Common::T_ear::LEFT, BAND_MID, value);
				WriteLog(state, "SET PARAMETER: Mid tone band Left set to (dB) ", value);
				break;

			case PARAM_TONE_HIGH_LEFT:
				SetTone(state, Common::T_ear::LEFT, BAND_HIGH, value);
				WriteLog(state, "SET PARAMETER: HIgh tone band Left set to (dB) ", value);
				break;

			case PARAM_TONE_LOW_RIGHT:
				SetTone(state, Common::T_ear::RIGHT, BAND_LOW, value);
				WriteLog(state, "SET PARAMETER: Low tone band Right set to (dB) ", value);
				break;

			case PARAM_TONE_MID_RIGHT:
				SetTone(state, Common::T_ear::RIGHT, BAND_MID, value);
				WriteLog(state, "SET PARAMETER: Mid tone band Right set to (dB) ", value);
				break;

			case PARAM_TONE_HIGH_RIGHT:
				SetTone(state, Common::T_ear::RIGHT, BAND_HIGH, value);
				WriteLog(state, "SET PARAMETER: High tone band Right set to (dB) ", value);
				break;

			case PARAM_HANDLE:
				assert(false); // should be unreachable
				return UNITY_AUDIODSP_ERR_UNSUPPORTED;

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
					if (data->limiter.IsDynamicProcessApplied())
						*value = 1.0f;
					else
						*value = 0.0f;					
					break;

				case PARAM_NORMALIZATION_LEFT_GET:
					*value = data->HA.GetDynamicEqualizer(Common::T_ear::LEFT)->GetOveralOffset_dB();
					break;

				case PARAM_NORMALIZATION_RIGHT_GET:
					*value = data->HA.GetDynamicEqualizer(Common::T_ear::RIGHT)->GetOveralOffset_dB();
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
		//CStereoBuffer<float> inStereoBuffer(length * outchannels);		
		//for (int i = 0; i < length*outchannels; i++)
		//{
		//	inStereoBuffer[i] = inbuffer[i]; 
		//}
		Common::CEarPair<CMonoBuffer<float>> inBufferPair;
		inBufferPair.left.Fill(length, 0.0f);
		inBufferPair.right.Fill(length, 0.0f);
		int monoIndex = 0;
		for (int stereoIndex = 0; stereoIndex < length*outchannels; stereoIndex+=2)
		{
			inBufferPair.left[monoIndex] = inbuffer[stereoIndex];
			inBufferPair.right[monoIndex] = inbuffer[stereoIndex + 1];
			monoIndex++;
		}

		// Process!!
		//CStereoBuffer<float> outStereoBuffer(length * outchannels);		
		Common::CEarPair<CMonoBuffer<float>> outBufferPair;
		outBufferPair.left.Fill(length, 0.0f);
		outBufferPair.right.Fill(length, 0.0f);
		data->HA.Process(inBufferPair, outBufferPair);

		// Limiter
		//if (data->parameters[PARAM_LIMITER_SET_ON] > 0.0f) 
		//{		
		//	if ((bool)data->parameters[PARAM_PROCESS_LEFT_ON] || (bool)data->parameters[PARAM_PROCESS_RIGHT_ON])
		//		data->limiter.Process(outStereoBuffer);
		//}
		if (data->parameters[PARAM_LIMITER_SET_ON] > 0.0f)
		{
			if ((bool)data->parameters[PARAM_PROCESS_LEFT_ON] || (bool)data->parameters[PARAM_PROCESS_RIGHT_ON])
			{
				data->limiter.Process(outBufferPair);
			}
		}

		// Transform output buffer			
		//int i = 0;
		//for (auto it = outStereoBuffer.begin(); it != outStereoBuffer.end(); it++)
		//{
		//	outbuffer[i++] = *it;
		//}
		monoIndex = 0;
		for (int stereoIndex = 0; stereoIndex < length * outchannels; stereoIndex+=2)
		{
			outbuffer[stereoIndex] = outBufferPair.left[monoIndex];
			outbuffer[stereoIndex+1] = outBufferPair.right[monoIndex];
			monoIndex++;
		}

        return UNITY_AUDIODSP_OK;
    }
}
