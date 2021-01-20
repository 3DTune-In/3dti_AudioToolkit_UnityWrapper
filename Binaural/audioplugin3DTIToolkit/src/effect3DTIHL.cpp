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
//#include <HAHLSimulation/ClassificationScaleHL.h>

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
#include "Common/CommonDefinitions.h"
#include "HAHLSimulation/ButterworthMultibandExpander.h"
#include "HAHLSimulation/FrequencySmearing.h"

//enum THLClassificationScaleCurve { HL_CS_ERROR =-1, HL_CS_NOLOSS =0, 
//								   HL_CS_A =1, HL_CS_B =2, HL_CS_C =3, HL_CS_D =4, HL_CS_E =5, HL_CS_F =6,
//								   HL_CS_G =7, HL_CS_H =8, HL_CS_I =9, HL_CS_J =10, HL_CS_K=11, 
//								   HL_CS_NOMORECURVES=12};

/////////////////////////////////////////////////////////////////////

namespace HLSimulation3DTI
{

	using namespace Common;

	enum FrequencySmearingApproach : int
	{
		FREQUENCYSMEARING_APPROACH_BAERMOORE = 0,
		FREQUENCYSMEARING_APPROACH_GRAF,
		FREQUENCYSMEARING_APPROACH_COUNT
	};

//////////////////////////////////////////////////////

// Default values for parameters
// Module switches:
#define DEFAULT_HL_ON				false
#define DEFAULT_HL_ON				false
#define DEFAULT_MULTIBANDEXPANDER_ON	true
#define DEFAULT_TEMPORALDISTORTION_ON	false
#define DEFAULT_FREQUENCYSMEARING_ON	false
#define DEFAULT_FREQUENCYSMEARING_APPROACH FREQUENCYSMEARING_APPROACH_BAERMOORE
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
// Temporal distortion simulator:
#define DEFAULT_TABAND				1600
#define DEFAULT_TANOISELPF			500
#define DEFAULT_TANOISEPOWER		0.0
#define DEFAULT_TALRSYNC_AMOUNT		0.0
#define DEFAULT_TALRSYNC_ON			false
// Frequency smearing:
#define DEFAULT_FSSIZE				1
#define DEFAULT_FSHZ				0.0
// Classification scale:
//#define DEFAULT_CS_CURVE		THLClassificationScaleCurve::HL_CS_NOLOSS
//#define DEFAULT_CS_SEVERITY		0

// Min/max values for parameters
// Multiband expander::
#define MIN_INIFREQ				20	
#define MAX_INIFREQ				20000
#define MAX_BANDSNUMBER			31
#define MAX_CALIBRATION_DBSPL	120
#define MIN_FILTERSPERBAND		1
#define MAX_FILTERSPERBAND		9
#define MIN_HEARINGLOSS			0
#define MAX_HEARINGLOSS			160
#define MAX_RATIO				500
#define MIN_THRESHOLD			-80
#define MAX_ATTACK				2000
#define MAX_RELEASE				2000
// Temporal Distortion simulator:
#define MIN_TABAND				200
#define MAX_TABAND				12800
#define MIN_TANOISELPF			1
#define MAX_TANOISELPF			1000
#define MIN_TANOISEPOWER		0.0
#define MAX_TANOISEPOWER		1.0
#define MIN_TALRSYNC			0.0
#define MAX_TALRSYNC			1.0
#define MIN_TAAUTOCORRELATION	-1000
#define MAX_TAAUTOCORRELATION	1000
// Frequency smearing:
#define MIN_FSSIZE				1
#define MAX_FSSIZE				255
#define MIN_FSHZ				0.0
#define MAX_FSHZ				1000.0
// Classification scale:
//#define MAX_CS_SEVERITY			6

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
	
	// Switch on/off multiband expander for each ear
	PARAM_MULTIBANDEXPANDER_ON_LEFT,
	PARAM_MULTIBANDEXPANDER_ON_RIGHT,

	// Temporal Distortion
	PARAM_TEMPORALDISTORTION_ON_LEFT,
	PARAM_TEMPORALDISTORTION_ON_RIGHT,
	PARAM_TA_BAND_LEFT,
	PARAM_TA_BAND_RIGHT,
	PARAM_TA_NOISELPF_LEFT,
	PARAM_TA_NOISELPF_RIGHT,
	PARAM_TA_NOISEPOWER_LEFT,
	PARAM_TA_NOISEPOWER_RIGHT,
	PARAM_TA_LRSYNC_AMOUNT,
	PARAM_TA_LRSYNC_ON,
	PARAM_TA_AUTOCORR0_GET_LEFT,
	PARAM_TA_AUTOCORR1_GET_LEFT,
	PARAM_TA_AUTOCORR0_GET_RIGHT,
	PARAM_TA_AUTOCORR1_GET_RIGHT,

	// Frequency smearing
	PARAM_FREQUENCYSMEARING_APPROACH_LEFT,
	PARAM_FREQUENCYSMEARING_APPROACH_RIGHT,
	PARAM_FREQUENCYSMEARING_ON_LEFT,
	PARAM_FREQUENCYSMEARING_ON_RIGHT,
	PARAM_FS_DOWN_SIZE_LEFT,
	PARAM_FS_DOWN_SIZE_RIGHT,
	PARAM_FS_UP_SIZE_LEFT,
	PARAM_FS_UP_SIZE_RIGHT,
	PARAM_FS_DOWN_HZ_LEFT,
	PARAM_FS_DOWN_HZ_RIGHT,
	PARAM_FS_UP_HZ_LEFT,
	PARAM_FS_UP_HZ_RIGHT,

	// Classification scale
	//PARAM_CLASSIFICATION_CURVE_LEFT,
	//PARAM_CLASSIFICATION_SEVERITY_LEFT,
	//PARAM_CLASSIFICATION_CURVE_RIGHT,
	//PARAM_CLASSIFICATION_SEVERITY_RIGHT,

		//// Debug log
		//PARAM_DEBUG_LOG,

	P_NUM
};

	/////////////////////////////////////////////////////////////////////

    struct EffectData
    {
		HAHLSimulation::CHearingLossSim HL;				
		float parameters[P_NUM];

		// Classification scale
		//Common::CEarPair<THLClassificationScaleCurve> csCurve;
		//Common::CEarPair<int> csSeverity;

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

	////bool IsValidClassificationScaleCurveLetter(char letter)
	////{
	////	if (letter == CS_CURVE_NOLOSS)
	////		return true;
	////	if (letter == RETURN_ERROR_CS_CURVE)
	////		return false;
	////	if ((int)letter > (int)MAX_CS_CURVE || (int)letter < (int)'A')
	////		return false;

	////	return true;
	////}

	///////////////////////////////////////////////////////////////////////

	////float FromClassificationScaleCurveLetterToFloat(char letter)
	////{
	////	// NOTE: we can compare with == since small integers have full precission representation in IEEE754 floating point. 

	////	if (letter == CS_CURVE_NOLOSS)
	////		return -1.0f;

	////	if ((int)letter <= (int)MAX_CS_CURVE && (int)letter >= (int)'A')
	////	{
	////		return (float)((int)letter - (int)'A');
	////	}
	////	else
	////		return RETURN_ERROR_CS_CURVE;	// ERROR: Unknown letter
	////}

	//float FromClassificationScaleCurveToFloat(THLClassificationScaleCurve curve)
	//{		
	//	return (float)(int)curve;
	//}

	///////////////////////////////////////////////////////////////////////

	////char FromFloatToClassificationScaleCurveLetter(float fletter)
	////{
	////	// NOTE: we can compare with == since small integers have full precission representation in IEEE754 floating point. 

	////	if (fletter == -1.0f)
	////		return CS_CURVE_NOLOSS;

	////	if (fletter >= 0.0f && fletter <= (float)(int)MAX_CS_CURVE)
	////		return (char((int)fletter));
	////	else
	////		return char(RETURN_ERROR_CS_CURVE);	// ERROR: Unknown float code for letter
	////}

	//THLClassificationScaleCurve FromFloatToClassificationScaleCurve (float fvalue)
	//{		
	//	if (fvalue >= -0.1f && fvalue < (float)(int)THLClassificationScaleCurve::HL_CS_NOMORECURVES)
	//		return (THLClassificationScaleCurve)(int)fvalue;
	//	else
	//		return THLClassificationScaleCurve::HL_CS_ERROR;
	//}

	///////////////////////////////////////////////////////////////////////

	//string FromClassificationScaleCurveToString(THLClassificationScaleCurve curve)
	//{
	//	string result = "";
	//	switch (curve)
	//	{
	//		case HL_CS_NOLOSS:
	//			result = "No hearing loss";
	//			break;
	//		case HL_CS_A:
	//			result = "A (Loss only on frequencies starting from 4000Hz and above)";
	//			break;
	//		case HL_CS_B:
	//			result = "B (Loss only on frequencies starting from 2000Hz and above)";
	//			break;
	//		case HL_CS_C:
	//			result = "C (Loss only on frequencies starting from 1000Hz and above)";
	//			break;
	//		case HL_CS_D:
	//			result = "D (Loss only on frequencies starting from 500Hz and above)";
	//			break;
	//		case HL_CS_E:
	//			result = "E (Loss only on frequencies starting from 250Hz and above)";
	//			break;
	//		case HL_CS_F:
	//			result = "F (Peak loss at 250Hz)";
	//			break;
	//		case HL_CS_G:
	//			result = "G (Peak loss at 500Hz)";
	//			break;
	//		case HL_CS_H:
	//			result = "H (Peak loss at 1000Hz)";
	//			break;
	//		case HL_CS_I:
	//			result = "I (Peak loss at 2000Hz)";
	//			break;
	//		case HL_CS_J:
	//			result = "J (Peak loss at 4000Hz)";
	//			break;
	//		case HL_CS_K:
	//			result = "K (Constant Slope)";
	//			break;
	//		default:					
	//			result = "Unknown curve!";
	//			break;
	//	}
	//	return result;
	//}

	///////////////////////////////////////////////////////////////////////

	//char FromClassificationScaleCurveToChar(THLClassificationScaleCurve curve)
	//{
	//	char result = ' ';
	//	switch (curve)
	//	{
	//		case HL_CS_A:
	//			result = 'A';
	//			break;
	//		case HL_CS_B:
	//			result = 'B';
	//			break;
	//		case HL_CS_C:
	//			result = 'C';
	//			break;
	//		case HL_CS_D:
	//			result = 'D';
	//			break;
	//		case HL_CS_E:
	//			result = 'E';
	//			break;
	//		case HL_CS_F:
	//			result = 'F';
	//			break;
	//		case HL_CS_G:
	//			result = 'G';
	//			break;
	//		case HL_CS_H:
	//			result = 'H';
	//			break;
	//		case HL_CS_I:
	//			result = 'I';
	//			break;
	//		case HL_CS_J:
	//			result = 'J';
	//			break;
	//		case HL_CS_K:
	//			result = 'K';
	//			break;
	//		default:
	//			result = ' ';
	//			break;
	//	}
	//	return result;
	//}

	void SetNewAudiometry(UnityAudioEffectState* state, Common::T_ear ear, TAudiometry& audiometry)
	{
		// Resize audiometry if needed
		if (audiometry.size() == 7)
		{
			audiometry.insert(audiometry.begin(), audiometry[0]);						// Copy first value, to have 9 bands from 7
			audiometry.insert(audiometry.end(), audiometry[audiometry.size() - 1]);	// Copy last value, to have 9 bands from 7
		}

		// Set audiometry in HL simulator
		EffectData* data = state->GetEffectData<EffectData>();
		data->HL.SetFromAudiometry_dBHL(ear, audiometry);

		// Set all audiometry (hearing loss levels) parameters to keep coherency
		if (ear == Common::T_ear::LEFT)
		{
			data->parameters[PARAM_MBE_BAND_0_LEFT] = audiometry[0];
			data->parameters[PARAM_MBE_BAND_1_LEFT] = audiometry[1];
			data->parameters[PARAM_MBE_BAND_2_LEFT] = audiometry[2];
			data->parameters[PARAM_MBE_BAND_3_LEFT] = audiometry[3];
			data->parameters[PARAM_MBE_BAND_4_LEFT] = audiometry[4];
			data->parameters[PARAM_MBE_BAND_5_LEFT] = audiometry[5];
			data->parameters[PARAM_MBE_BAND_6_LEFT] = audiometry[6];
			data->parameters[PARAM_MBE_BAND_7_LEFT] = audiometry[7];
			data->parameters[PARAM_MBE_BAND_8_LEFT] = audiometry[8];
		}
		else
		{
			data->parameters[PARAM_MBE_BAND_0_RIGHT] = audiometry[0];
			data->parameters[PARAM_MBE_BAND_1_RIGHT] = audiometry[1];
			data->parameters[PARAM_MBE_BAND_2_RIGHT] = audiometry[2];
			data->parameters[PARAM_MBE_BAND_3_RIGHT] = audiometry[3];
			data->parameters[PARAM_MBE_BAND_4_RIGHT] = audiometry[4];
			data->parameters[PARAM_MBE_BAND_5_RIGHT] = audiometry[5];
			data->parameters[PARAM_MBE_BAND_6_RIGHT] = audiometry[6];
			data->parameters[PARAM_MBE_BAND_7_RIGHT] = audiometry[7];
			data->parameters[PARAM_MBE_BAND_8_RIGHT] = audiometry[8];
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
		
		// Temporal Distortion
		RegisterParameter(definition, "HLTAONL", "", 0.0f, 1.0f, Bool2Float(DEFAULT_TEMPORALDISTORTION_ON), 1.0f, 1.0f, PARAM_TEMPORALDISTORTION_ON_LEFT, "Switch on temporal distortion simulation for left ear");
		RegisterParameter(definition, "HLTAONR", "", 0.0f, 1.0f, Bool2Float(DEFAULT_TEMPORALDISTORTION_ON), 1.0f, 1.0f, PARAM_TEMPORALDISTORTION_ON_RIGHT, "Switch on temporal distortion simulation for right ear");
		RegisterParameter(definition, "HLTABANDL", "Hz", MIN_TABAND, MAX_TABAND, DEFAULT_TABAND, 1.0f, 1.0f, PARAM_TA_BAND_LEFT, "Upper band limit for temporal distortion simulation in left ear (Hz)");
		RegisterParameter(definition, "HLTABANDR", "Hz", MIN_TABAND, MAX_TABAND, DEFAULT_TABAND, 1.0f, 1.0f, PARAM_TA_BAND_RIGHT, "Upper band limit for temporal distortion simulation in right ear (Hz)");
		RegisterParameter(definition, "HLTALPFL", "Hz", MIN_TANOISELPF, MAX_TANOISELPF, DEFAULT_TANOISELPF, 1.0f, 1.0f, PARAM_TA_NOISELPF_LEFT, "Cutoff frequency of temporal distortion jitter noise autocorrelation LPF in left ear (Hz)");
		RegisterParameter(definition, "HLTALPFR", "Hz", MIN_TANOISELPF, MAX_TANOISELPF, DEFAULT_TANOISELPF, 1.0f, 1.0f, PARAM_TA_NOISELPF_RIGHT, "Cutoff frequency of temporal distortion jitter noise autocorrelation LPF in right ear (Hz)");
		RegisterParameter(definition, "HLTAPOWL", "ms", MIN_TANOISEPOWER, MAX_TANOISEPOWER, DEFAULT_TANOISEPOWER, 1.0f, 1.0f, PARAM_TA_NOISEPOWER_LEFT, "Power of temporal distortion jitter white noise in left ear (ms)");
		RegisterParameter(definition, "HLTAPOWR", "ms", MIN_TANOISEPOWER, MAX_TANOISEPOWER, DEFAULT_TANOISEPOWER, 1.0f, 1.0f, PARAM_TA_NOISEPOWER_RIGHT, "Power of temporal distortion jitter white noise in right ear (ms)");
		RegisterParameter(definition, "HLTALR", "", MIN_TALRSYNC, MAX_TALRSYNC, DEFAULT_TALRSYNC_AMOUNT, 1.0f, 1.0f, PARAM_TA_LRSYNC_AMOUNT, "Synchronicity between left and right ears in temporal asyncrhony (0.0 to 1.0)");
		RegisterParameter(definition, "HLTALRON", "", 0.0f, 1.0f, Bool2Float(DEFAULT_TALRSYNC_ON), 1.0f, 1.0f, PARAM_TA_LRSYNC_ON, "Switch on left-right synchronicity in temporal distortion simulation");
		RegisterParameter(definition, "HLTA0GL", "", MIN_TAAUTOCORRELATION, MAX_TAAUTOCORRELATION, 0.0f, 1.0f, 1.0f, PARAM_TA_AUTOCORR0_GET_LEFT, "Autocorrelation coefficient zero in left temporal distortion noise source?");
		RegisterParameter(definition, "HLTA1GL", "", MIN_TAAUTOCORRELATION, MAX_TAAUTOCORRELATION, 0.0f, 1.0f, 1.0f, PARAM_TA_AUTOCORR1_GET_LEFT, "Autocorrelation coefficient one in left temporal distortion noise source?");
		RegisterParameter(definition, "HLTA0GR", "", MIN_TAAUTOCORRELATION, MAX_TAAUTOCORRELATION, 0.0f, 1.0f, 1.0f, PARAM_TA_AUTOCORR0_GET_RIGHT, "Autocorrelation coefficient zero in right temporal distortion noise source?");
		RegisterParameter(definition, "HLTA1GR", "", MIN_TAAUTOCORRELATION, MAX_TAAUTOCORRELATION, 0.0f, 1.0f, 1.0f, PARAM_TA_AUTOCORR1_GET_RIGHT, "Autocorrelation coefficient one in right temporal distortion noise source?");

		// Frequency smearing
		RegisterParameter(definition, "HLFSONL", "", 0.0f, 1.0f, DEFAULT_FREQUENCYSMEARING_ON, 1.0f, 1.0f, PARAM_FREQUENCYSMEARING_ON_LEFT, "Switch on frequency smearing simulation for left ear");
		RegisterParameter(definition, "HLFSONR", "", 0.0f, 1.0f, DEFAULT_FREQUENCYSMEARING_ON, 1.0f, 1.0f, PARAM_FREQUENCYSMEARING_ON_RIGHT, "Switch on frequency smearing simulation for right ear");
		RegisterParameter(definition, "HLFSAPPROACHL", "", 0.0f, FREQUENCYSMEARING_APPROACH_COUNT, DEFAULT_FREQUENCYSMEARING_APPROACH, 1.0f, 1.0f, PARAM_FREQUENCYSMEARING_APPROACH_LEFT, "Approach used for Frequency smearing");
		RegisterParameter(definition, "HLFSAPPROACHR", "", 0.0f, FREQUENCYSMEARING_APPROACH_COUNT, DEFAULT_FREQUENCYSMEARING_APPROACH, 1.0f, 1.0f, PARAM_FREQUENCYSMEARING_APPROACH_RIGHT, "Approach used for Frequency smearing");
		RegisterParameter(definition, "HLFSDOWNSZL", "", MIN_FSSIZE, MAX_FSSIZE, DEFAULT_FSSIZE, 1.0f, 1.0f, PARAM_FS_DOWN_SIZE_LEFT, "Size of downward section of smearing window for left ear");
		RegisterParameter(definition, "HLFSDOWNSZR", "", MIN_FSSIZE, MAX_FSSIZE, DEFAULT_FSSIZE, 1.0f, 1.0f, PARAM_FS_DOWN_SIZE_RIGHT, "Size of downward section of smearing window for right ear");
		RegisterParameter(definition, "HLFSUPSZL", "", MIN_FSSIZE, MAX_FSSIZE, DEFAULT_FSSIZE, 1.0f, 1.0f, PARAM_FS_UP_SIZE_LEFT, "Size of upward section of smearing window for left ear");
		RegisterParameter(definition, "HLFSUPSZR", "", MIN_FSSIZE, MAX_FSSIZE, DEFAULT_FSSIZE, 1.0f, 1.0f, PARAM_FS_UP_SIZE_RIGHT, "Size of upward section of smearing window for right ear");
		RegisterParameter(definition, "HLFSDOWNHZL", "Hz", MIN_FSHZ, MAX_FSHZ, DEFAULT_FSHZ, 1.0f, 1.0f, PARAM_FS_DOWN_HZ_LEFT, "Amount of downward smearing effect (in Hz) in left ear");
		RegisterParameter(definition, "HLFSDOWNHZR", "Hz", MIN_FSHZ, MAX_FSHZ, DEFAULT_FSHZ, 1.0f, 1.0f, PARAM_FS_DOWN_HZ_RIGHT, "Amount of downward smearing effect (in Hz) in right ear");
		RegisterParameter(definition, "HLFSUPHZL", "Hz", MIN_FSHZ, MAX_FSHZ, DEFAULT_FSHZ, 1.0f, 1.0f, PARAM_FS_UP_HZ_LEFT, "Amount of upward smearing effect (in Hz) in left ear");
		RegisterParameter(definition, "HLFSUPHZR", "Hz", MIN_FSHZ, MAX_FSHZ, DEFAULT_FSHZ, 1.0f, 1.0f, PARAM_FS_UP_HZ_RIGHT, "Amount of upward smearing effect (in Hz) in right ear");

		// Classification scale
		//RegisterParameter(definition, "HLCSCURL", "", -1.0f, FromClassificationScaleCurveToFloat(THLClassificationScaleCurve::HL_CS_NOMORECURVES), FromClassificationScaleCurveToFloat(DEFAULT_CS_CURVE), 1.0f, 1.0f, PARAM_CLASSIFICATION_CURVE_LEFT, "Curve for classification scale in left ear");
		//RegisterParameter(definition, "HLCSCURR", "", -1.0f, FromClassificationScaleCurveToFloat(THLClassificationScaleCurve::HL_CS_NOMORECURVES), FromClassificationScaleCurveToFloat(DEFAULT_CS_CURVE), 1.0f, 1.0f, PARAM_CLASSIFICATION_CURVE_RIGHT, "Curve for classification scale in right ear");		
		//RegisterParameter(definition, "HLCSSEVL", "", 0.0f, (float)MAX_CS_SEVERITY, DEFAULT_CS_SEVERITY, 1.0f, 1.0f, PARAM_CLASSIFICATION_SEVERITY_LEFT, "Severity for classification scale in left ear");
		//RegisterParameter(definition, "HLCSSEVR", "", 0.0f, (float)MAX_CS_SEVERITY, DEFAULT_CS_SEVERITY, 1.0f, 1.0f, PARAM_CLASSIFICATION_SEVERITY_RIGHT, "Severity for classification scale in right ear");

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
		WriteLog(state, "        Non-linear attenuation Left = ", DEFAULT_MULTIBANDEXPANDER_ON);
		WriteLog(state, "        Non-linear attenuation Right = ", DEFAULT_MULTIBANDEXPANDER_ON);
		WriteLog(state, "        Temporal Distortion Left = ", DEFAULT_TEMPORALDISTORTION_ON);
		WriteLog(state, "        Temporal Distortion Right = ", DEFAULT_TEMPORALDISTORTION_ON);
		WriteLog(state, "        Frequency Smearing Left = ",  DEFAULT_FREQUENCYSMEARING_APPROACH);
		WriteLog(state, "        Frequency Smearing Right = ", DEFAULT_FREQUENCYSMEARING_APPROACH);
		//WriteLog(state, "        HL Classification scale curve (might by overriden) = ", FromClassificationScaleCurveToString(DEFAULT_CS_CURVE));
		//WriteLog(state, "        HL Classification scale severity (might by overriden) = ", DEFAULT_CS_SEVERITY);

		// Non-linear attenuation:
		WriteLog(state, "CREATE: Non-linear attenuation setup:", "");
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

		// Temporal Distortion:
#ifndef UNITY_ANDROID
		string lrsync = "";
		if (DEFAULT_TALRSYNC_ON)
			lrsync = std::to_string(DEFAULT_TALRSYNC_AMOUNT) + " (ON)";
		else
			lrsync = "OFF";
#else
		float lrsync = -1;
		if (DEFAULT_TALRSYNC_ON)
			lrsync = DEFAULT_TALRSYNC_AMOUNT;		
#endif
		WriteLog(state, "CREATE: Temporal distortion setup:", "");
		WriteLog(state, "        Left-Right synchronicity = ", lrsync);
		WriteLog(state, "        Left band upper limit = ", DEFAULT_TABAND);
		WriteLog(state, "        Left autocorrelation LPF cutoff = ", DEFAULT_TANOISELPF);
		WriteLog(state, "        Left white noise power = ", DEFAULT_TANOISEPOWER);
		WriteLog(state, "        Right band upper limit = ", DEFAULT_TABAND);
		WriteLog(state, "        Right autocorrelation LPF cutoff = ", DEFAULT_TANOISELPF);
		WriteLog(state, "        Right white noise power = ", DEFAULT_TANOISEPOWER);		

		// Frequency smearing:
		WriteLog(state, "CREATE: Frequency smearing setup:", "");		
		WriteLog(state, "        Left ear downward size = ", DEFAULT_FSSIZE);
		WriteLog(state, "        Left ear upward size = ", DEFAULT_FSSIZE);
		WriteLog(state, "        Right ear downward size = ", DEFAULT_FSSIZE);
		WriteLog(state, "        Right ear upward size = ", DEFAULT_FSSIZE);
		WriteLog(state, "        Left ear downward amount = ", DEFAULT_FSHZ);
		WriteLog(state, "        Left ear upward amount = ", DEFAULT_FSHZ);
		WriteLog(state, "        Right ear downward amount = ", DEFAULT_FSHZ);
		WriteLog(state, "        Right ear upward amount = ", DEFAULT_FSHZ);

		WriteLog(state, "--------------------------------------", "\n");
	}

	/////////////////////////////////////////////////////////////////////	

	void updateFrequencySmearer(UnityAudioEffectState* state, Common::T_ear ear)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		FrequencySmearingApproach approach = data->parameters[PARAM_FREQUENCYSMEARING_APPROACH_LEFT + ear] == FREQUENCYSMEARING_APPROACH_BAERMOORE? FREQUENCYSMEARING_APPROACH_BAERMOORE  : FREQUENCYSMEARING_APPROACH_GRAF;
		assert(ear == LEFT || ear == RIGHT);
		static_assert(LEFT == 0 && RIGHT == 1, "Lets us add the ear to parameter enum");
		if (approach == FREQUENCYSMEARING_APPROACH_BAERMOORE)
		{
			auto frequencySmearer = std::make_shared<HAHLSimulation::CBaerMooreFrequencySmearing>();
			frequencySmearer->Setup(state->dspbuffersize, state->samplerate);
			frequencySmearer->SetDownwardBroadeningFactor(data->parameters[PARAM_FS_DOWN_HZ_LEFT+ear]);
			frequencySmearer->SetUpwardBroadeningFactor(data->parameters[PARAM_FS_UP_HZ_LEFT+ear]);
			data->HL.SetFrequencySmearer(ear, frequencySmearer);
			//effectdata->HL.EnableFrequencySmearing(Common::LEFT);
		}
		else
		{
			assert(approach == FREQUENCYSMEARING_APPROACH_GRAF);
			auto frequencySmearer = std::make_shared<HAHLSimulation::CGraf3DTIFrequencySmearing>();
			frequencySmearer->Setup(state->dspbuffersize, state->samplerate);
			frequencySmearer->SetDownwardSmearingBufferSize((int) data->parameters[PARAM_FS_DOWN_SIZE_LEFT + ear]);
			frequencySmearer->SetUpwardSmearingBufferSize((int) data->parameters[PARAM_FS_UP_SIZE_LEFT + ear]);
			frequencySmearer->SetDownwardSmearing_Hz(data->parameters[PARAM_FS_DOWN_HZ_LEFT + ear]);
			frequencySmearer->SetUpwardSmearing_Hz(data->parameters[PARAM_FS_UP_HZ_LEFT + ear]);
			data->HL.SetFrequencySmearer(ear, frequencySmearer);
		}
	}


	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
		EffectData* effectdata = new EffectData;
		//memset(effectdata, 0, sizeof(EffectData));
		state->effectdata = effectdata;
		InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->parameters);

		// TO DO: check errors with debugger

#pragma message(": warning: todo: I think we should be reading these defaults from effectdata->parameters rather than reusing the default values, to ensure parameters remains consistent with the state of HL")

		// Module switches
		if (DEFAULT_HL_ON)
			effectdata->HL.EnableHearingLossSimulation(Common::T_ear::BOTH);
		else
			effectdata->HL.DisableHearingLossSimulation(Common::T_ear::BOTH);
		if (DEFAULT_MULTIBANDEXPANDER_ON)
			effectdata->HL.EnableMultibandExpander(Common::T_ear::BOTH);
		else
			effectdata->HL.DisableMultibandExpander(Common::T_ear::BOTH);
		if (DEFAULT_TEMPORALDISTORTION_ON)
			effectdata->HL.EnableTemporalDistortion(Common::T_ear::BOTH);
		else
			effectdata->HL.DisableTemporalDistortion(Common::T_ear::BOTH);
		if (DEFAULT_FREQUENCYSMEARING_ON)
			effectdata->HL.EnableFrequencySmearing(Common::T_ear::BOTH);
		else
			effectdata->HL.DisableFrequencySmearing(Common::T_ear::BOTH);

		// Hearing loss simulator Setup		
		//effectdata->HL.Setup(state->samplerate, DEFAULT_CALIBRATION_DBSPL, DEFAULT_INIFREQ, DEFAULT_BANDSNUMBER, DEFAULT_FILTERSPERBAND, state->dspbuffersize);
		effectdata->HL.Setup(state->samplerate, DEFAULT_CALIBRATION_DBSPL, DEFAULT_BANDSNUMBER, state->dspbuffersize);

		// Tim: We now need to manually create multiband expanders before calling SetFromAudiometry
		for (auto ear : { Common::T_ear::LEFT, Common::T_ear::RIGHT })
		{
#pragma message(": warning: todo use a define to set which multiband expander to use")
#pragma message(": warning: todo use a define/parameter to set whether filterGrouping is enabled")
			auto multibandExpander = std::make_shared<HAHLSimulation::CButterworthMultibandExpander>();
			const bool TEMP_DEFAULT_FILTER_GROUPING = false;
			multibandExpander->Setup(state->samplerate, DEFAULT_INIFREQ, effectdata->HL.GetNumberOfBands(), TEMP_DEFAULT_FILTER_GROUPING);
			effectdata->HL.SetMultibandExpander(ear, multibandExpander);

			updateFrequencySmearer(state, ear);
		}




		// Initial setup of hearing loss levels
		effectdata->HL.SetFromAudiometry_dBHL(Common::T_ear::BOTH, DEFAULT_AUDIOMETRY);

		// Setup calibration
		effectdata->HL.SetCalibration(DEFAULT_CALIBRATION_DBSPL);

		// Setup of envelope detectors
		for (bool filterGrouping : {false, true})
		{
			effectdata->HL.SetAttackForAllBands(Common::T_ear::BOTH, DEFAULT_ATTACK, filterGrouping);
			effectdata->HL.SetReleaseForAllBands(Common::T_ear::BOTH, DEFAULT_RELEASE, filterGrouping);
		}

		// Initial setup of temporal distortion simulator
		effectdata->HL.GetTemporalDistortionSimulator()->SetBandUpperLimit(Common::T_ear::BOTH, DEFAULT_TABAND);
		effectdata->HL.GetTemporalDistortionSimulator()->SetNoiseAutocorrelationFilterCutoffFrequency(Common::T_ear::BOTH, DEFAULT_TANOISELPF);
		effectdata->HL.GetTemporalDistortionSimulator()->SetWhiteNoisePower(Common::T_ear::BOTH, DEFAULT_TANOISEPOWER);
		effectdata->HL.GetTemporalDistortionSimulator()->SetLeftRightNoiseSynchronicity(DEFAULT_TALRSYNC_AMOUNT);
		if (DEFAULT_TALRSYNC_ON)
			effectdata->HL.GetTemporalDistortionSimulator()->EnableLeftRightNoiseSynchronicity();
		else
			effectdata->HL.GetTemporalDistortionSimulator()->DisableLeftRightNoiseSynchronicity();

		// Setup audiometry from classification scale
		//effectdata->csCurve.left = DEFAULT_CS_CURVE;
		//effectdata->csCurve.right = DEFAULT_CS_CURVE;
		//effectdata->csSeverity.left = DEFAULT_CS_SEVERITY;
		//effectdata->csSeverity.right = DEFAULT_CS_SEVERITY;
		//if (DEFAULT_CS_CURVE != THLClassificationScaleCurve::HL_CS_NOLOSS)
		//{
		//	TAudiometry csAudiometry;
		//	HAHLSimulation::GetClassificationScaleHL(FromClassificationScaleCurveToChar(DEFAULT_CS_CURVE), DEFAULT_CS_SEVERITY, csAudiometry);
		//	effectdata->HL.SetFromAudiometry_dBHL(Common::T_ear::BOTH, csAudiometry);
		//}

		// Initial setup of frequency smearing
		//effectdata->HL.GetFrequencySmearingSimulator(Common::T_ear::LEFT)->...

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

		// Note that as parameters are stored in here then for some updates we can delegate to another function, such as updateFrequencySmearer(..)
        data->parameters[index] = value;

		//THLClassificationScaleCurve curve;
		//int severity;		

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

			case PARAM_TEMPORALDISTORTION_ON_LEFT:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off left temporal distortion simulation with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.DisableTemporalDistortion(Common::T_ear::LEFT);
						WriteLog(state, "SET PARAMETER: Left ear temporal distortion simulation switched OFF", "");
					}
					if (Float2Bool(value))
					{
						data->HL.EnableTemporalDistortion(Common::T_ear::LEFT);
						WriteLog(state, "SET PARAMETER: Left ear temporal distortion simulation switched ON", "");
					}
				}
				break;

			case PARAM_TEMPORALDISTORTION_ON_RIGHT:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off right temporal distortion simulation with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.DisableTemporalDistortion(Common::T_ear::RIGHT);
						WriteLog(state, "SET PARAMETER: Right ear temporal distortion simulation switched OFF", "");
					}
					if (Float2Bool(value))
					{
						data->HL.EnableTemporalDistortion(Common::T_ear::RIGHT);
						WriteLog(state, "SET PARAMETER: Right ear temporal distortion simulation switched ON", "");
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
#pragma message(": warning: todo - make separate parameters for the two filter grouping options")
				for (bool filterGrouping : {false, true})
					data->HL.SetAttackForAllBands(Common::T_ear::LEFT, value, filterGrouping);
				WriteLog(state, "SET PARAMETER: Attack time (ms) for Left envelope detectors set to: ", value);				
				break;

			case PARAM_MBE_ATTACK_RIGHT:
#pragma message(": warning: todo - make separate parameters for the two filter grouping options")
				for (bool filterGrouping : {false, true})
					data->HL.SetAttackForAllBands(Common::T_ear::RIGHT, value, filterGrouping);
				WriteLog(state, "SET PARAMETER: Attack time (ms) for Right envelope detectors set to: ", value);
				break;

			case PARAM_MBE_RELEASE_LEFT:
#pragma message(": warning: todo - make separate parameters for the two filter grouping options")
				for (bool filterGrouping : {false, true})
					data->HL.SetReleaseForAllBands(Common::T_ear::LEFT, value, filterGrouping);
				WriteLog(state, "SET PARAMETER: Release time (ms) for Left envelope detectors set to: ", value);
				break;

			case PARAM_MBE_RELEASE_RIGHT:
#pragma message(": warning: todo - make separate parameters for the two filter grouping options")
				for (bool filterGrouping : {false, true})
					data->HL.SetReleaseForAllBands(Common::T_ear::RIGHT, value, filterGrouping);
				WriteLog(state, "SET PARAMETER: Release time (ms) for Right envelope detectors set to: ", value);
				break;

			// TEMPORAL DISTORTION SIMULATION:
			case PARAM_TA_BAND_LEFT:
				data->HL.GetTemporalDistortionSimulator()->SetBandUpperLimit(Common::T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: Band upper limit (Hz) for Left temporal distortion simulator set to: ", value);
				break;

			case PARAM_TA_BAND_RIGHT:
				data->HL.GetTemporalDistortionSimulator()->SetBandUpperLimit(Common::T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: Band upper limit (Hz) for Right temporal distortion simulator set to: ", value);
				break;

			case PARAM_TA_NOISELPF_LEFT:
				data->HL.GetTemporalDistortionSimulator()->SetNoiseAutocorrelationFilterCutoffFrequency(Common::T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: Noise autocorrelation LPF cutoff (Hz) for Left temporal distortion simulator set to: ", value);
				break;

			case PARAM_TA_NOISELPF_RIGHT:
				data->HL.GetTemporalDistortionSimulator()->SetNoiseAutocorrelationFilterCutoffFrequency(Common::T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: Noise autocorrelation LPF cutoff (Hz) for Right temporal Distortion simulator set to: ", value);
				break;

			case PARAM_TA_NOISEPOWER_LEFT:
				data->HL.GetTemporalDistortionSimulator()->SetWhiteNoisePower(Common::T_ear::LEFT, value);
				WriteLog(state, "SET PARAMETER: White noise power (ms) for Left temporal Distortion simulator set to: ", value);
				break;

			case PARAM_TA_NOISEPOWER_RIGHT:
				data->HL.GetTemporalDistortionSimulator()->SetWhiteNoisePower(Common::T_ear::RIGHT, value);
				WriteLog(state, "SET PARAMETER: White noise power (ms) for Right temporal distortion simulator set to: ", value);
				break;

			case PARAM_TA_LRSYNC_AMOUNT:
				if ((value >= 0.0f) && (value <= 1.0f))
				{
					data->HL.GetTemporalDistortionSimulator()->SetLeftRightNoiseSynchronicity(value);
					WriteLog(state, "SET PARAMETER: Left-Right synchronicity for temporal distortion simulator set to: ", value);
				}
				else
					WriteLog(state, "SET PARAMETER: ERROR!!! Bad value for Left-Right synchronicity of temporal distortion (needs to be between 0.0f and 1.0f): ", value);
				break;

			case PARAM_TA_LRSYNC_ON:
				if (((int)value != 0) && ((int)value != 1))
					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off left-right synchronicity in temporal distortion simulation with non boolean value ", value);
				else
				{
					if (!Float2Bool(value))
					{
						data->HL.GetTemporalDistortionSimulator()->DisableLeftRightNoiseSynchronicity();
						WriteLog(state, "SET PARAMETER: Left-right ear synchronicity in temporal distortion simulator switched OFF", "");
					}
					if (Float2Bool(value))
					{
						data->HL.GetTemporalDistortionSimulator()->EnableLeftRightNoiseSynchronicity();
						WriteLog(state, "SET PARAMETER: Left-right ear synchronicity in temporal distortion simulator switched ON", "");
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

			// FREQUENCY SMEARING:
			case PARAM_FREQUENCYSMEARING_ON_LEFT:
			case PARAM_FS_DOWN_SIZE_LEFT:
			case PARAM_FS_UP_SIZE_LEFT:
			case PARAM_FS_DOWN_HZ_LEFT:
			case PARAM_FS_UP_HZ_LEFT:
				updateFrequencySmearer(state, LEFT);
				break;
			case PARAM_FREQUENCYSMEARING_ON_RIGHT:
			case PARAM_FS_DOWN_SIZE_RIGHT:
			case PARAM_FS_UP_SIZE_RIGHT:
			case PARAM_FS_DOWN_HZ_RIGHT:
			case PARAM_FS_UP_HZ_RIGHT:
				updateFrequencySmearer(state, RIGHT);
				break;



//			case PARAM_FREQUENCYSMEARING_ON_LEFT:
//				if (((int)value != 0) && ((int)value != 1))
//					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off frequency smearing in left ear with non boolean value ", value);
//				else
//				{
//					if (!Float2Bool(value))
//					{
//						data->HL.DisableFrequencySmearing(Common::T_ear::LEFT);
//						WriteLog(state, "SET PARAMETER: Frequency smearing in left ear switched OFF", "");
//					}
//					if (Float2Bool(value))
//					{
//						//if (std::dynamic_pointer_cast<HAHLSimulation::CBaerMooreFrequencySmearing>(data->HL.GetFrequencySmearingSimulator(Common::LEFT)) == nullptr)
//						//{
//						//	auto frequencySmearer = std::make_shared<HAHLSimulation::CBaerMooreFrequencySmearing>();
//						//	frequencySmearer->Setup(state->dspbuffersize, state->samplerate);
//						//	frequencySmearer->SetDownwardBroadeningFactor(data->parameters[PARAM_FS_DOWN_HZ_LEFT]);
//						//	frequencySmearer->SetUpwardBroadeningFactor(data->parameters[PARAM_FS_UP_HZ_LEFT]);
//						//	data->HL.SetFrequencySmearer(Common::LEFT, frequencySmearer);
//						//	data->HL.EnableFrequencySmearing(Common::LEFT);
//						//}
//						data->HL.EnableFrequencySmearing(Common::T_ear::LEFT);
//						WriteLog(state, "SET PARAMETER: Frequency smearing in left ear switched ON", "");
//					}
//				}
//				break;
//
//			case PARAM_FREQUENCYSMEARING_ON_RIGHT:
//				if (((int)value != 0) && ((int)value != 1))
//					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to switch on/off frequency smearing in right ear with non boolean value ", value);
//				else
//				{
//					if (!Float2Bool(value))
//					{
//						data->HL.DisableFrequencySmearing(Common::T_ear::RIGHT);
//						WriteLog(state, "SET PARAMETER: Frequency smearing in right ear switched OFF", "");
//					}
//					if (Float2Bool(value))
//					{
//						data->HL.EnableFrequencySmearing(Common::T_ear::RIGHT);
//						WriteLog(state, "SET PARAMETER: Frequency smearing in right ear switched ON", "");
//					}
//				}
//				break;
//
//			case PARAM_FS_DOWN_SIZE_LEFT:
//				if ((int)value > 0)
//				{
//#pragma message(": warning: todo")
//					
//					//data->HL.GetFrequencySmearingSimulator(Common::T_ear::LEFT)->SetDownwardSmearingBufferSize((int)value);
//					WriteLog(state, "SET PARAMETER: Downward smearing buffer size for left ear = ", (int)value);
//				}
//				else
//					WriteLog(state, "SET PARAMETER: ERROR!!! Wrong size for downward smearing buffer in left ear = ", (int)value);
//				break;
//
//			case PARAM_FS_DOWN_SIZE_RIGHT:
//				if ((int)value > 0)
//				{
//#pragma message(": warning: todo")
//					//data->HL.GetFrequencySmearingSimulator(Common::T_ear::RIGHT)->SetDownwardSmearingBufferSize((int)value);
//					WriteLog(state, "SET PARAMETER: Downward smearing buffer size for right ear = ", (int)value);
//				}
//				else
//					WriteLog(state, "SET PARAMETER: ERROR!!! Wrong size for downward smearing buffer in right ear = ", (int)value);
//				break;
//
//			case PARAM_FS_UP_SIZE_LEFT:
//				if ((int)value > 0)
//				{
//#pragma message(": warning: todo")
//					//data->HL.GetFrequencySmearingSimulator(Common::T_ear::LEFT)->SetUpwardSmearingBufferSize((int)value);
//					WriteLog(state, "SET PARAMETER: Upward smearing buffer size for left ear = ", (int)value);
//				}
//				else
//					WriteLog(state, "SET PARAMETER: ERROR!!! Wrong size for upward smearing buffer in left ear = ", (int)value);
//				break;
//
//			case PARAM_FS_UP_SIZE_RIGHT:
//				if ((int)value > 0)
//				{
//#pragma message(": warning: todo")
//					//data->HL.GetFrequencySmearingSimulator(Common::T_ear::RIGHT)->SetUpwardSmearingBufferSize((int)value);
//					WriteLog(state, "SET PARAMETER: Upward smearing buffer size for right ear = ", (int)value);
//				}
//				else
//					WriteLog(state, "SET PARAMETER: ERROR!!! Wrong size for upward smearing buffer in right ear = ", (int)value);
//				break;
//
//			case PARAM_FS_DOWN_HZ_LEFT:
//				if (value >= 0.0f)
//				{
//#pragma message(": warning: todo: Separate parameter for each frequency smearer? Or check which type we have before casting")
//
//					//if (data->parameters[PARAM_FREQUENCYSMEARING_APPROACH_LEFT] == FREQUENCYSMEARING_APPROACH_BAERMOORE)
//					//{
//					//	shared_ptr<HAHLSimulation::CFrequencySmearing> abstractSmearer = data->HL.GetFrequencySmearingSimulator(Common::LEFT);
//					//	assert(abstractSmearer != nullptr);
//					//	auto smearer = std::dynamic_pointer_cast<HAHLSimulation::CBaerMooreFrequencySmearing>(abstractSmearer);
//					//	// If this fails then data->parameters has fallen out of sync with the state of data->HL
//					//	assert(smearer != nullptr);
//					//	smearer->SetDownwardBroadeningFactor(value);
//					//}
//					//data->HL.GetFrequencySmearingSimulator(Common::T_ear::LEFT)->SetDownwardSmearing_Hz(value);
//					WriteLog(state, "SET PARAMETER: Downward smearing amount (in Hz) for left ear = ", value);
//				}
//				else
//					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to set a negative downward smearing amount (in Hz) for left ear = ", value);
//				break;
//
//			case PARAM_FS_DOWN_HZ_RIGHT:
//				if (value >= 0.0f)
//				{
//#pragma message(": warning: todo")
//					//data->HL.GetFrequencySmearingSimulator(Common::T_ear::RIGHT)->SetDownwardSmearing_Hz(value);
//					WriteLog(state, "SET PARAMETER: Downward smearing amount (in Hz) for right ear = ", value);
//				}
//				else
//					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to set a negative downward smearing amount (in Hz) for right ear = ", value);
//				break;
//
//			case PARAM_FS_UP_HZ_LEFT:
//				if (value >= 0.0f)
//				{
//#pragma message(": warning: todo")
//					//if (data->parameters[PARAM_FREQUENCYSMEARING_APPROACH_LEFT] == FREQUENCYSMEARING_APPROACH_BAERMOORE)
//					//{
//					//	shared_ptr<HAHLSimulation::CFrequencySmearing> abstractSmearer = data->HL.GetFrequencySmearingSimulator(Common::LEFT);
//					//	assert(abstractSmearer != nullptr);
//					//	auto smearer = std::dynamic_pointer_cast<HAHLSimulation::CBaerMooreFrequencySmearing>(abstractSmearer);
//					//	// If this fails then data->parameters has fallen out of sync with the state of data->HL
//					//	assert(smearer != nullptr);
//					//	smearer->SetUpwardBroadeningFactor(value);
//					//}
//					//data->HL.GetFrequencySmearingSimulator(Common::T_ear::LEFT)->SetUpwardSmearing_Hz(value);
//					WriteLog(state, "SET PARAMETER: Upward smearing amount (in Hz) for left ear = ", value);
//				}
//				else
//					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to set a negative upward smearing amount (in Hz) for left ear = ", value);
//				break;
//
//			case PARAM_FS_UP_HZ_RIGHT:
//				if (value >= 0.0f)
//				{
//#pragma message(": warning: todo")
//					//data->HL.GetFrequencySmearingSimulator(Common::T_ear::RIGHT)->SetUpwardSmearing_Hz(value);
//					WriteLog(state, "SET PARAMETER: Upward smearing amount (in Hz) for right ear = ", value);
//				}
//				else
//					WriteLog(state, "SET PARAMETER: ERROR!!! Attempt to set a negative upward smearing amount (in Hz) for right ear = ", value);
//				break;

			// CLASSIFICATION SCALE:

			//case PARAM_CLASSIFICATION_CURVE_LEFT:
			//	curve = FromFloatToClassificationScaleCurve(value);
			//	
			//	if (curve == THLClassificationScaleCurve::HL_CS_ERROR)
			//	{
			//		WriteLog(state, "SET PARAMETER: ERROR!!! Unknown curve in HL classification scale for left ear = ", value);
			//		break;
			//	}

			//	data->csCurve.left = curve;

			//	if (curve == THLClassificationScaleCurve::HL_CS_NOLOSS)
			//	{
			//		TAudiometry flatAudiometry;
			//		flatAudiometry.assign(data->HL.GetNumberOfBands(), 0.0f);
			//		SetNewAudiometry(state, Common::T_ear::LEFT, flatAudiometry);					
			//		WriteLog(state, "SET PARAMETER: HL classification scale set to No Hearing Loss in left ear ", "");
			//		break;
			//	}
			//	else
			//	{
			//		TAudiometry csAudiometry;
			//		HAHLSimulation::GetClassificationScaleHL(FromClassificationScaleCurveToChar(data->csCurve.left), data->csSeverity.left, csAudiometry);
			//		SetNewAudiometry(state, Common::T_ear::LEFT, csAudiometry);
			//		WriteLog(state, "SET PARAMETER: HL classification scale curve in left ear set to: ", FromClassificationScaleCurveToString(curve));
			//	}
			//	break;

			//case PARAM_CLASSIFICATION_CURVE_RIGHT:
			//	curve = FromFloatToClassificationScaleCurve(value);

			//	if (curve == THLClassificationScaleCurve::HL_CS_ERROR)
			//	{
			//		WriteLog(state, "SET PARAMETER: ERROR!!! Unknown curve in HL classification scale for right ear = ", value);
			//		break;
			//	}

			//	data->csCurve.right = curve;

			//	if (curve == THLClassificationScaleCurve::HL_CS_NOLOSS)
			//	{
			//		TAudiometry flatAudiometry;
			//		flatAudiometry.assign(data->HL.GetNumberOfBands(), 0.0f);
			//		SetNewAudiometry(state, Common::T_ear::RIGHT, flatAudiometry);
			//		WriteLog(state, "SET PARAMETER: HL classification scale set to No Hearing Loss in right ear ", "");
			//		break;
			//	}
			//	else
			//	{
			//		TAudiometry csAudiometry;
			//		HAHLSimulation::GetClassificationScaleHL(FromClassificationScaleCurveToChar(data->csCurve.right), data->csSeverity.right, csAudiometry);
			//		SetNewAudiometry(state, Common::T_ear::RIGHT, csAudiometry);
			//		WriteLog(state, "SET PARAMETER: HL classification scale curve in right ear set to: ", FromClassificationScaleCurveToString(curve));
			//	}
			//	break;

			//case PARAM_CLASSIFICATION_SEVERITY_LEFT:
			//	severity = (int)value;

			//	if (severity < 0 || severity > MAX_CS_SEVERITY)
			//	{
			//		WriteLog(state, "SET PARAMETER: ERROR!!! Wrong severity value in HL classification scale for left ear = ", (int)value);				
			//	}
			//	else
			//	{
			//		data->csSeverity.left = severity;
			//		if (data->csCurve.left == THLClassificationScaleCurve::HL_CS_NOLOSS)
			//		{
			//			WriteLog(state, "SET PARAMETER: WARNING! Changing severity of HL classification scale with No Hearing Loss curve will have no immediate effect in left ear: ", severity);
			//		}					
			//		else
			//		{
			//			TAudiometry csAudiometry;
			//			HAHLSimulation::GetClassificationScaleHL(FromClassificationScaleCurveToChar(data->csCurve.left), data->csSeverity.left, csAudiometry);
			//			SetNewAudiometry(state, Common::T_ear::LEFT, csAudiometry);
			//			WriteLog(state, "SET PARAMETER: HL classification scale severity in left ear set to ", severity);
			//		}
			//	}
			//	break;

			//case PARAM_CLASSIFICATION_SEVERITY_RIGHT:
			//	severity = (int)value;

			//	if (severity < 0 || severity > MAX_CS_SEVERITY)
			//	{
			//		WriteLog(state, "SET PARAMETER: ERROR!!! Wrong severity value in HL classification scale for right ear = ", (int)value);
			//	}
			//	else
			//	{
			//		data->csSeverity.right = severity;
			//		if (data->csCurve.right == THLClassificationScaleCurve::HL_CS_NOLOSS)
			//		{
			//			WriteLog(state, "SET PARAMETER: WARNING! Changing severity of HL classification scale with No Hearing Loss curve will have no immediate effect in right ear: ", severity);
			//		}
			//		else
			//		{
			//			TAudiometry csAudiometry;
			//			HAHLSimulation::GetClassificationScaleHL(FromClassificationScaleCurveToChar(data->csCurve.right), data->csSeverity.right, csAudiometry);
			//			SetNewAudiometry(state, Common::T_ear::RIGHT, csAudiometry);
			//			WriteLog(state, "SET PARAMETER: HL classification scale severity in right ear set to ", severity);
			//		}
			//	}
			//	break;

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
					*value = data->HL.GetTemporalDistortionSimulator()->GetPower(Common::T_ear::LEFT);
					break;

				case PARAM_TA_AUTOCORR1_GET_LEFT:
					*value = data->HL.GetTemporalDistortionSimulator()->GetNormalizedAutocorrelation(Common::T_ear::LEFT);
					break;

				case PARAM_TA_AUTOCORR0_GET_RIGHT:
					*value = data->HL.GetTemporalDistortionSimulator()->GetPower(Common::T_ear::RIGHT);
					break;

				case PARAM_TA_AUTOCORR1_GET_RIGHT:
					*value = data->HL.GetTemporalDistortionSimulator()->GetNormalizedAutocorrelation(Common::T_ear::RIGHT);
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
		//CStereoBuffer<float> inputBuffer(length*2);						
		//for (int i = 0; i < length*2; i++)
		//{
		//	inputBuffer[i] = inbuffer[i]; 
		//}
		Common::CEarPair<CMonoBuffer<float>> inBufferPair;
		inBufferPair.left.Fill(length, 0.0f);
		inBufferPair.right.Fill(length, 0.0f);
		int monoIndex = 0;
		for (int stereoIndex = 0; stereoIndex < length*outchannels; stereoIndex += 2)
		{
			inBufferPair.left[monoIndex] = inbuffer[stereoIndex];
			inBufferPair.right[monoIndex] = inbuffer[stereoIndex + 1];
			monoIndex++;
		}

		// Process!!
		//CStereoBuffer<float> outputBuffer(length*2);
		Common::CEarPair<CMonoBuffer<float>> outBufferPair;
		outBufferPair.left.Fill(length, 0.0f);
		outBufferPair.right.Fill(length, 0.0f);
		data->HL.Process(inBufferPair, outBufferPair);		

		data->parameters[PARAM_TA_AUTOCORR0_GET_LEFT] = data->HL.GetTemporalDistortionSimulator()->GetPower(Common::T_ear::LEFT);
		data->parameters[PARAM_TA_AUTOCORR1_GET_LEFT] = data->HL.GetTemporalDistortionSimulator()->GetNormalizedAutocorrelation(Common::T_ear::LEFT);
		data->parameters[PARAM_TA_AUTOCORR0_GET_RIGHT] = data->HL.GetTemporalDistortionSimulator()->GetPower(Common::T_ear::RIGHT);
		data->parameters[PARAM_TA_AUTOCORR1_GET_RIGHT] = data->HL.GetTemporalDistortionSimulator()->GetNormalizedAutocorrelation(Common::T_ear::RIGHT);

		// Transform output buffer					
		//for (int i = 0; i < length*2; i++)
		//{
		//	outbuffer[i] = outputBuffer[i];
		//}		
		monoIndex = 0;
		for (int stereoIndex = 0; stereoIndex < length * outchannels; stereoIndex += 2)
		{
			outbuffer[stereoIndex] = outBufferPair.left[monoIndex];
			outbuffer[stereoIndex + 1] = outBufferPair.right[monoIndex];
			monoIndex++;
		}

        return UNITY_AUDIODSP_OK;
    }
}
