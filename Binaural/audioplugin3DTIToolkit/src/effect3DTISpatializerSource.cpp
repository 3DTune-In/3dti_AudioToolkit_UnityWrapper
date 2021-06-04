/**
*** 3D-Tune-In Toolkit Unity Wrapper: Binaural Spatializer ***
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
* Updated: June - August 2020
* by Tim Murray-Browne at the Dyson School of Engineering, Imperial College London.
**/

#include "effect3DTISpatializerSource.h"

//#include <BinauralSpatializer\3DTI_BinauralSpatializer.h>
#include <Common/ErrorHandler.h>


// Includes for debug logging
#include <fstream>
#include <iostream>

#include <mutex>

#include <sstream>
#include <cstdint>

#include <HRTF/HRTFCereal.h>
#include <ILD/ILDCereal.h>

enum TLoadResult { RESULT_LOAD_WAITING = 0, RESULT_LOAD_CONTINUE = 1, RESULT_LOAD_END = 2, RESULT_LOAD_OK = 3, RESULT_LOAD_ERROR = -1 };

// DEBUG LOG 
#ifdef UNITY_ANDROID
#define DEBUG_LOG_CAT
#else
#define DEBUG_LOG_FILE_BINSP
#endif

#ifdef DEBUG_LOG_CAT
#include <android/log.h> 
#include <string>
#include <sstream>
#endif

#include <cfloat>
#include "HRTF/HRTFFactory.h"
#include "effect3DTISpatializerCore.h"
#include "CommonUtils.h"

/////////////////////////////////////////////////////////////////////

namespace SpatializerSource3DTI
{

	//#define LIMITER_THRESHOLD	-30.0f
	//#define LIMITER_ATTACK		500.0f
	//#define LIMITER_RELEASE		500.0f
	//#define LIMITER_RATIO		6


	//// Single state instance shared across all audio sources
	//Spatializer& spatializer()
	//{
	//    static Spatializer spatializer;
	//    return spatializer;
	//}


	template <class T>
	void WriteLog(string logText, const T& value, int sourceID = -1)
	{
		std::cerr << logText << " " << value;
		if (sourceID >= 0)
		{
			std::cerr << " (source " << sourceID << ")";
		}
		std::cerr << std::endl;
		//
		//    if (spatializer().debugLog)
		//    {
		//#ifdef DEBUG_LOG_FILE_BINSP
		//        ofstream logfile;
		//        logfile.open("3DTI_BinauralSpatializer_DebugLog.txt", ofstream::out | ofstream::app);
		//        if (sourceID != -1)
		//            logfile << sourceID << ": " << logtext << value << endl;
		//        else
		//            logfile << logtext << value << endl;
		//        logfile.close();
		//#endif
		//        
		//#ifdef DEBUG_LOG_CAT
		//        std::ostringstream os;
		//        os << logtext << value;
		//        string fulltext = os.str();
		//        __android_log_print(ANDROID_LOG_DEBUG, "3DTISPATIALIZER", fulltext.c_str());
		//#endif
	}


template <class T>
void WriteLog(UnityAudioEffectState* state, string logtext, const T& value)
{
	WriteLog(logtext, value, state->GetEffectData<EffectData>()->sourceID);
}

void WriteLog(string logtext)
{
	WriteLog(logtext, "");
	//std::cerr << logtext << std::endl;
}

enum SpatializationMode : int
{
	SPATIALIZATION_MODE_HIGH_QUALITY = 0,
	SPATIALIZATION_MODE_HIGH_PERFORMANCE = 1,
	SPATIALIZATION_MODE_NONE = 2,
};


enum
{
	PARAM_SOURCE_ID,    // DEBUG
	PARAM_HRTF_INTERPOLATION, // 5 ### SOURCE ####
	PARAM_MOD_FARLPF, // ### SOURCE ####
	PARAM_MOD_DISTATT, // ### SOURCE ####
	PARAM_MOD_NEAR_FIELD_ILD,// ### SOURCE ####
	PARAM_SPATIALIZATION_MODE,// ### SOURCE ####


	P_NUM
};

struct EffectData
{
	int sourceID;    // DEBUG
	std::shared_ptr<Binaural::CSingleSourceDSP> audioSource;
	SpatializerCore3DTI::SpatializerCore* spatializer;
	float parameters[P_NUM];
};

/////////////////////////////////////////////////////////////////////

//    // Init parameters. Core is not ready until we load the HRTF. ILD will be disabled, so we don't need to worry yet
//    Spatializer::Spatializer()
//    : //coreReady(false)
////    ,
//loadedHRTF(false)
//    , loadedNearFieldILD(false)
//    , loadedHighPerformanceILD(false)
//    , spatializationMode(SPATIALIZATION_MODE_NONE)
//    , strHRTFpath(nullptr)
//    , strHRTFserializing(false)
//    , strHRTFcount(0)
//    , strHRTFlength(0)
//    , strNearFieldILDpath(nullptr)
//    , strNearFieldILDserializing(false)
//    , strNearFieldILDcount(0)
//    , strNearFieldILDlength(0)
//    , strHighPerformanceILDpath(nullptr)
//    , strHighPerformanceILDserializing(false)
//    , strHighPerformanceILDcount(0)
//    , strHighPerformanceILDlength(0)
//{
//    
//}
//
//    bool Spatializer::initialize(int sampleRate, int dspBufferSize)
//    {
//        WriteLog("Initializing 3DTI Spatializer...");
//        
//        InitParametersFromDefinitions(InternalRegisterEffectDefinition, parameters);
//        parameters[PARAM_SCALE_FACTOR] = 1.0f;
//
//        // Set default audio state
//        Common::TAudioStateStruct audioState;
//        audioState.sampleRate = sampleRate;
//        audioState.bufferSize = dspBufferSize;
//        core.SetAudioState(audioState);
//        listener = core.CreateListener();
//        
//        // Set default HRTF resampling step
//        core.SetHRTFResamplingStep(parameters[PARAM_HRTF_STEP]);
//
//        limiter.Setup(sampleRate, LIMITER_RATIO, LIMITER_THRESHOLD, LIMITER_ATTACK, LIMITER_RELEASE);
//
//        WriteLog("3DTI Spatializer initialized but awaiting listener binaries (HRTF and ILD).");
//
//        return true;
//    }




/////////////////////////////////////////////////////////////////////




	/////////////////////////////////////////////////////////////////////

int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
{
	int numparams = P_NUM;
	definition.paramdefs = new UnityAudioParameterDefinition[numparams];
	RegisterParameter(definition, "SourceID", "", -1.0f, /*FLT_MAX*/ 1e20f, -1.0f, 1.0f, 1.0f, PARAM_SOURCE_ID, "Source ID for debug");
	RegisterParameter(definition, "HRTFInterp", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_HRTF_INTERPOLATION, "HRTF Interpolation method");
	RegisterParameter(definition, "MODfarLPF", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_FARLPF, "Far distance LPF module enabler");
	RegisterParameter(definition, "MODDistAtt", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_DISTATT, "Distance attenuation module enabler");
	RegisterParameter(definition, "MODNFILD", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, PARAM_MOD_NEAR_FIELD_ILD, "Near distance ILD module enabler");
	RegisterParameter(definition, "SpatMode", "", 0.0f, 2.0f, 0.0f, 1.0f, 1.0f, PARAM_SPATIALIZATION_MODE, "Spatialization mode (0=High quality, 1=High performance, 2=None)");
	//Sample Rate and BufferSize
	definition.flags |= UnityAudioEffectDefinitionFlags_IsSpatializer;
	return numparams;
}

/////////////////////////////////////////////////////////////////////

Common::CTransform ComputeListenerTransformFromMatrix(float* listenerMatrix, float scale)
{
	// SET LISTENER POSITION

	// Inverted 4x4 listener matrix, as provided by Unity
	float L[16];
	for (int i = 0; i < 16; i++)
		L[i] = listenerMatrix[i];

	float listenerpos_x = -(L[0] * L[12] + L[1] * L[13] + L[2] * L[14]) * scale;	// From Unity documentation, if listener is rotated
	float listenerpos_y = -(L[4] * L[12] + L[5] * L[13] + L[6] * L[14]) * scale;	// From Unity documentation, if listener is rotated
	float listenerpos_z = -(L[8] * L[12] + L[9] * L[13] + L[10] * L[14]) * scale;	// From Unity documentation, if listener is rotated
	//float listenerpos_x = -L[12] * scale;	// If listener is not rotated
	//float listenerpos_y = -L[13] * scale;	// If listener is not rotated
	//float listenerpos_z = -L[14] * scale;	// If listener is not rotated
	Common::CTransform listenerTransform;
	listenerTransform.SetPosition(Common::CVector3(listenerpos_x, listenerpos_y, listenerpos_z));

	// SET LISTENER ORIENTATION

	//float w = 2 * sqrt(1.0f + L[0] + L[5] + L[10]);
	//float qw = w / 4.0f;
	//float qx = (L[6] - L[9]) / w;
	//float qy = (L[8] - L[2]) / w;
	//float qz = (L[1] - L[4]) / w;
	// http://forum.unity3d.com/threads/how-to-assign-matrix4x4-to-transform.121966/
	float tr = L[0] + L[5] + L[10];
	float w, qw, qx, qy, qz;
	if (tr > 0.0f)			// General case
	{
		w = sqrt(1.0f + tr) * 2.0f;
		qw = 0.25f * w;
		qx = (L[6] - L[9]) / w;
		qy = (L[8] - L[2]) / w;
		qz = (L[1] - L[4]) / w;
	}
	// Cases with w = 0
	else if ((L[0] > L[5]) && (L[0] > L[10]))
	{
		w = sqrt(1.0f + L[0] - L[5] - L[10]) * 2.0f;
		qw = (L[6] - L[9]) / w;
		qx = 0.25f * w;
		qy = -(L[1] + L[4]) / w;
		qz = -(L[8] + L[2]) / w;
	}
	else if (L[5] > L[10])
	{
		w = sqrt(1.0f + L[5] - L[0] - L[10]) * 2.0f;
		qw = (L[8] - L[2]) / w;
		qx = -(L[1] + L[4]) / w;
		qy = 0.25f * w;
		qz = -(L[6] + L[9]) / w;
	}
	else
	{
		w = sqrt(1.0f + L[10] - L[0] - L[5]) * 2.0f;
		qw = (L[1] - L[4]) / w;
		qx = -(L[8] + L[2]) / w;
		qy = -(L[6] + L[9]) / w;
		qz = 0.25f * w;
	}

	Common::CQuaternion unityQuaternion = Common::CQuaternion(qw, qx, qy, qz);
	listenerTransform.SetOrientation(unityQuaternion.Inverse());
	return listenerTransform;
}

/////////////////////////////////////////////////////////////////////

Common::CTransform ComputeSourceTransformFromMatrix(float* sourceMatrix, float scale)
{
	// Orientation does not matters for audio sources
	Common::CTransform sourceTransform;
	sourceTransform.SetPosition(Common::CVector3(sourceMatrix[12] * scale, sourceMatrix[13] * scale, sourceMatrix[14] * scale));
	return sourceTransform;
}

//	/////////////////////////////////////////////////////////////////////
//
//	bool LoadHRTFBinaryString(const std::basic_string<uint8_t>& hrtfData, std::shared_ptr<Binaural::CListener> listener)
//	{
//		std::istringstream stream(reinterpret_cast<const std::basic_string<char>&>(hrtfData));
//		return HRTF::CreateFrom3dtiStream(stream, listener);
//
//	}
//
//	int LoadHRTFBinaryFile(UnityAudioEffectState* state)
//	{
//		// Load HRTF
//		const string hrtfPath(spatializer().strHRTFpath);
//#ifdef UNITY_WIN
//		const string sofaExtension = ".sofa"s;
//		if (hrtfPath.size() >= sofaExtension.size() && hrtfPath.substr(hrtfPath.size() - sofaExtension.size()) == sofaExtension)
//		{
//			// We assume an ILD file holds the delays, so our SOFA file does not specify delays
//			bool specifiedDelays = false;
//			if (!HRTF::CreateFromSofa(hrtfPath, spatializer().listener, specifiedDelays))
//			{
//				return TLoadResult::RESULT_LOAD_ERROR;
//			}
//		}
//		// If not sofa file then assume its a 3dti-hrtf file
//		else
//#endif
//			if (!HRTF::CreateFrom3dti(spatializer().strHRTFpath, spatializer().listener))
//		{
//			//TDebuggerResultStruct result = GET_LAST_RESULT_STRUCT();
//			//WriteLog(state, "ERROR TRYING TO LOAD HRTF!!! ", result.suggestion);
//			return TLoadResult::RESULT_LOAD_ERROR;
//		}
//
//		if (spatializer().listener->GetHRTF()->GetHRIRLength() != 0)
//		{
//			//data->listener->LoadHRTF(std::move(myHead));
//			WriteLog(state, "LOAD HRTF: HRTF loaded from binary 3DTI/sofa file: ", spatializer().strHRTFpath);
//			WriteLog(state, "           HRIR length is ", spatializer().listener->GetHRTF()->GetHRIRLength());
//			WriteLog(state, "           Sample rate is ", state->samplerate);
//			WriteLog(state, "           Buffer size is ", state->dspbuffersize);
//
//			// Free memory
//			free(spatializer().strHRTFpath);
//
//			return TLoadResult::RESULT_LOAD_OK;
//		}
//		else
//		{
//			WriteLog(state, "LOAD HRTF: ERROR!!! Could not create HRTF from path: ", spatializer().strHRTFpath);
//			free(spatializer().strHRTFpath);
//			return TLoadResult::RESULT_LOAD_ERROR;
//		}
//    }
//
//    /////////////////////////////////////////////////////////////////////
//
//    bool LoadHighPerformanceILDBinaryString(const std::basic_string<uint8_t>& ildData, std::shared_ptr<Binaural::CListener> listener)
//    {
//        std::istringstream stream(reinterpret_cast<const std::basic_string<char>&>(ildData));
//        return ILD::CreateFrom3dtiStream(stream, listener, ILD::T_ILDTable::ILDSpatializationTable);
//    }
//
//	int LoadHighPerformanceILDBinaryFile(UnityAudioEffectState* state)
//	{
//		/*int sampleRateInFile = ILD::GetSampleRateFrom3dti(data->strNearFieldILDpath);
//		if (sampleRateInFile == (int)state->samplerate) {*/
//
//			// Get ILD
//			//T_ILD_HashTable h;
//			//h = ILD::CreateFrom3dti(data->strHighPerformanceILDpath);
//			bool boolResult = ILD::CreateFrom3dti_ILDSpatializationTable(spatializer().strHighPerformanceILDpath, spatializer().listener);
//
//			// Check errors
//			//TDebuggerResultStruct result = GET_LAST_RESULT_STRUCT();
//			//if (result.id != RESULT_OK)
//			//{
//			//	WriteLog(state, "ERROR TRYING TO LOAD HIGH PERFORMANCE ILD!!! ", result.suggestion);
//			//	return TLoadResult::RESULT_LOAD_ERROR;
//			//}
//
//			//if (h.size() > 0)		// TO DO: Improve this error check
//			if (boolResult)
//			{
//				///Binaural::CILD::SetILD_HashTable(std::move(h));
//				WriteLog(state, "LOAD HIGH PERFORMANCE ILD: ILD loaded from binary 3DTI file: ", spatializer().strHighPerformanceILDpath);
//				//WriteLog(state, "          Hash hable size is ", h.size());
//				free(spatializer().strHighPerformanceILDpath);
//				return TLoadResult::RESULT_LOAD_OK;
//			}
//			else
//			{
//				WriteLog(state, "LOAD HIGH PERFORMANCE ILD: ERROR!!! could not create ILD from path: ", spatializer().strHighPerformanceILDpath);
//				free(spatializer().strHighPerformanceILDpath);
//				return TLoadResult::RESULT_LOAD_ERROR;
//			}
//		/*}
//		else
//		{
//			WriteLog(state, "LOAD NEAR FIELD ILD: ERROR!!! output sample rate is not the same as the ILD from path: ", data->strNearFieldILDpath);
//			free(data->strNearFieldILDpath);
//			return TLoadResult::RESULT_LOAD_ERROR;
//		}*/
//	}
//
//	/////////////////////////////////////////////////////////////////////
//
//    bool LoadNearFieldILDBinaryString(const std::basic_string<uint8_t>& ildData, std::shared_ptr<Binaural::CListener> listener)
//    {
//        std::istringstream stream(reinterpret_cast<const std::basic_string<char>&>(ildData));
//        return ILD::CreateFrom3dtiStream(stream, listener, ILD::T_ILDTable::ILDNearFieldEffectTable);
//    }
//
//	int LoadNearFieldILDBinaryFile(UnityAudioEffectState* state)
//	{
//		// Get ILD
//		
//		/*int sampleRateInFile = ILD::GetSampleRateFrom3dti(data->strNearFieldILDpath);
//		if (sampleRateInFile == (int)state->samplerate)
//		{*/
//			bool boolResult = ILD::CreateFrom3dti_ILDNearFieldEffectTable(spatializer().strNearFieldILDpath, spatializer().listener);
//			// Check errors
//			//TResultStruct result = GET_LAST_RESULT_STRUCT();
//			//if (result.id != RESULT_OK)
//			//{
//			//	WriteLog(state, "ERROR TRYING TO LOAD NEAR FIELD ILD!!! ", result.suggestion);
//			//	return TLoadResult::RESULT_LOAD_ERROR;
//			//}
//
//			//if (h.size() > 0)		// TO DO: Improve this error check
//			if (boolResult)
//			{
//				//Binaural::CILD::SetILD_HashTable(std::move(h));
//				WriteLog(state, "LOAD NEAR FIELD ILD: ILD loaded from binary 3DTI file: ", spatializer().strNearFieldILDpath);
//				//WriteLog(state, "          Hash hable size is ", h.size());
//				free(spatializer().strNearFieldILDpath);
//				return TLoadResult::RESULT_LOAD_OK;
//			}
//			else
//			{
//				WriteLog(state, "LOAD NEAR FIELD ILD: ERROR!!! could not create ILD from path: ", spatializer().strNearFieldILDpath);
//				free(spatializer().strNearFieldILDpath);
//				return TLoadResult::RESULT_LOAD_ERROR;
//			}
//
//		/*}
//		else
//		{
//			WriteLog(state, "LOAD NEAR FIELD ILD: ERROR!!! output sample rate is not the same as the ILD from path: ", data->strNearFieldILDpath);
//			free(data->strNearFieldILDpath);
//			return TLoadResult::RESULT_LOAD_ERROR;
//		}
//		*/
//
//		
//	}
//
//	/////////////////////////////////////////////////////////////////////
//
//	int BuildPathString(UnityAudioEffectState* state, char*& path, bool &serializing, int &length, int &count, float value)
//	{
//		// Check if serialization was not started
//		if (!serializing)
//		{
//			// Receive string length
//			
//			length = static_cast<int>(value);
//			path = (char*)malloc((length+1) * sizeof(char));
//			count = 0;
//			serializing = true;
//            return RESULT_LOAD_WAITING;  // TODO: @cgarre please check!!
//		}
//		else
//		{
//			// Receive next character
//
//			// Concatenate char to string
//			int valueInt = static_cast<int>(value);
//			char valueChr = static_cast<char>(valueInt);
//			path[count] = valueChr;
//			++count;
//
//			// Check if string has ended
//			if (count == length)
//			{
//				path[count] = 0;	// End character
//				serializing = false;
//				return RESULT_LOAD_END;
//			}
//			else
//				return RESULT_LOAD_CONTINUE;
//		}
//	}

	/////////////////////////////////////////////////////////////////////

void WriteLogHeader(UnityAudioEffectState* state)
{
	EffectData* data = state->GetEffectData<EffectData>();

	// TO DO: Change this for high performance / high quality modes

	SpatializerCore3DTI::SpatializerCore* spatializer = data->spatializer;

	// Audio state:
	Common::TAudioStateStruct audioState = spatializer->core.GetAudioState();
	WriteLog(state, "CREATE: Sample rate set to ", audioState.sampleRate);
	WriteLog(state, "CREATE: Buffer size set to ", audioState.bufferSize);
	WriteLog(state, "CREATE: HRTF resampling step set to ", spatializer->core.GetHRTFResamplingStep());

	// Listener:
	if (spatializer->listener != nullptr)
		WriteLog(state, "CREATE: Listener created successfully", "");
	else
		WriteLog(state, "CREATE: ERROR!!!! Listener creation returned null pointer!", "");

	// Source:
	if (data->audioSource != nullptr)
		WriteLog(state, "CREATE: Source created successfully", "");
	else
		WriteLog(state, "CREATE: ERROR!!!! Source creation returned null pointer!", "");

	WriteLog(state, "--------------------------------------", "\n");
}

/////////////////////////////////////////////////////////////////////
// AUDIO PLUGIN SDK FUNCTIONS
/////////////////////////////////////////////////////////////////////

static UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK DistanceAttenuationCallback(UnityAudioEffectState* state, float distanceIn, float attenuationIn, float* attenuationOut)
{
	*attenuationOut = attenuationIn;
	return UNITY_AUDIODSP_OK;
}

/////////////////////////////////////////////////////////////////////

UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
{




	// CREATE Instance state and grab parameters

	EffectData* effectdata = new EffectData;
	{
		state->effectdata = effectdata;
		if (IsHostCompatible(state))
		{
			state->spatializerdata->distanceattenuationcallback = DistanceAttenuationCallback;
		}
		effectdata->sourceID = -1;
		effectdata->spatializer = SpatializerCore3DTI::SpatializerCore::instance();

		if (effectdata->spatializer == nullptr)
		{
			WriteLog("Error: Created spatialized audio source but there no SpatializerCore plugin has been created yet.");
			delete state->effectdata;
			state->effectdata = nullptr;
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		}
	}
	auto spatializer = effectdata->spatializer;


	// Create source and set default interpolation method
	effectdata->audioSource = spatializer->core.CreateSingleSourceDSP();
	if (effectdata->audioSource != nullptr)
	{
		effectdata->audioSource->EnableInterpolation();
		//if (spatializer->spatializationMode == SPATIALIZATION_MODE_HIGH_QUALITY && spatializer->loadedHighPerformanceILD)
		//{
		//    effectdata->audioSource->EnableNearFieldEffect();
		//}
		//else
		//{
		//    effectdata->audioSource->DisableNearFieldEffect();    // ILD disabled before loading ILD data
		//}

	}


	// 3DTI Debugger
#if defined (SWITCH_ON_3DTI_ERRORHANDLER) || defined (_3DTI_ANDROID_ERRORHANDLER)
	Common::CErrorHandler::Instance().SetAssertMode(ASSERT_MODE_CONTINUE);
#endif


	return UNITY_AUDIODSP_OK;
}

/////////////////////////////////////////////////////////////////////

UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
{
	WriteLog(state, "Releasing audio plugin...", "");
	EffectData* data = state->GetEffectData<EffectData>();
	delete data;
	return UNITY_AUDIODSP_OK;
}


/////////////////////////////////////////////////////////////////////


UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
{
	EffectData* data = state->GetEffectData<EffectData>();
	SpatializerCore3DTI::SpatializerCore* spatializer = data->spatializer;
	assert(data != nullptr && spatializer != nullptr);
	if (index >= P_NUM)
		return UNITY_AUDIODSP_ERR_UNSUPPORTED;
	data->parameters[index] = value;

	//Common::CMagnitudes magnitudes;
	//int loadResult;

	std::lock_guard<std::mutex> lock(spatializer->mutex);

	//spatializer().spatializerMutex.lock();

	// Process command sent by C# API
	switch (index)
	{
		//			case PARAM_HRTF_FILE_STRING:	// Load HRTF binary file (MANDATORY)
		//				loadResult = BuildPathString(state, spatializer().strHRTFpath, spatializer().strHRTFserializing, spatializer().strHRTFlength, spatializer().strHRTFcount, value);
		//				if (loadResult == TLoadResult::RESULT_LOAD_END)
		//				{
		//					loadResult = LoadHRTFBinaryFile(state);
		//					if (loadResult == TLoadResult::RESULT_LOAD_OK)
		//					{
		//						spatializer().loadedHRTF = true;
		////						UpdateCoreIsReady();
		//					}
		//				}
		//				break;
		//
		//			case PARAM_NEAR_FIELD_ILD_FILE_STRING:	// Load ILD binary file (MANDATORY?)
		//				loadResult = BuildPathString(state, spatializer().strNearFieldILDpath, spatializer().strNearFieldILDserializing, spatializer().strNearFieldILDlength, spatializer().strNearFieldILDcount, value);
		//				if (loadResult == TLoadResult::RESULT_LOAD_END)
		//				{
		//					loadResult = LoadNearFieldILDBinaryFile(state);
		//					if (loadResult == TLoadResult::RESULT_LOAD_OK)
		//					{
		//                        // This is now enabled when instantiating a new source
		////						data->audioSource->EnableNearFieldEffect();
		//						spatializer().loadedNearFieldILD = true;
		//						WriteLog(state, "SET PARAMETER: Near Field ILD Enabled", "");
		////						UpdateCoreIsReady();
		//					}
		//				}
		//				break;

					//case PARAM_HEAD_RADIUS:	// Set listener head radius (OPTIONAL)
					//	spatializer().listener->SetHeadRadius(value);
					//	WriteLog(state, "SET PARAMETER: Listener head radius changed to ", value);
					//	break;

					// FUNCTIONALITY TO BE IMPLEMENTED
					//case PARAM_SCALE_FACTOR:	// Set scale factor (OPTIONAL)
					//	// Used directly in the parameters array
					//	WriteLog(state, "SET PARAMETER: Scale factor changed to ", value);
					//	break;

	case PARAM_SOURCE_ID:	// DEBUG
		data->sourceID = (int)value;
		WriteLog(state, "SET PARAMETER: Source ID set to ", data->sourceID);
		break;

		//case PARAM_CUSTOM_ITD:	// Enable custom ITD (OPTIONAL)
		//	if (value > 0.0f)
		//	{
		//		spatializer().listener->EnableCustomizedITD();
		//		WriteLog(state, "SET PARAMETER: Custom ITD is ", "Enabled");
		//	}
		//	else
		//	{
		//		spatializer().listener->DisableCustomizedITD();
		//		WriteLog(state, "SET PARAMETER: Custom ITD is ", "Disabled");
		//	}
		//	break;

	case PARAM_HRTF_INTERPOLATION:	// Change interpolation method (OPTIONAL)
		if (value != 0.0f)
		{
			data->audioSource->EnableInterpolation();
			WriteLog(state, "SET PARAMETER: Run-time HRTF Interpolation switched ", "ON");
		}
		else
		{
			data->audioSource->DisableInterpolation();
			WriteLog(state, "SET PARAMETER: Run-time HRTF Interpolation switched ", "OFF");
		}
		break;

	case PARAM_MOD_FARLPF:
		if (value > 0.0f)
		{
			data->audioSource->EnableFarDistanceEffect();
			WriteLog(state, "SET PARAMETER: Far distance LPF is ", "Enabled");
		}
		else
		{
			data->audioSource->DisableFarDistanceEffect();
			WriteLog(state, "SET PARAMETER: Far distance LPF is ", "Disabled");
		}
		break;

	case PARAM_MOD_DISTATT:
		if (value > 0.0f)
		{
			data->audioSource->EnableDistanceAttenuationAnechoic();
			WriteLog(state, "SET PARAMETER: Distance attenuation is ", "Enabled");
		}
		else
		{
			data->audioSource->DisableDistanceAttenuationAnechoic();
			WriteLog(state, "SET PARAMETER: Distance attenuation is ", "Disabled");
		}
		break;

	case PARAM_MOD_NEAR_FIELD_ILD:
		if (value > 0.0f)
		{
			data->audioSource->EnableNearFieldEffect();
			WriteLog(state, "SET PARAMETER: Near Field ILD is ", "Enabled");
		}
		else
		{
			data->audioSource->DisableNearFieldEffect();
			WriteLog(state, "SET PARAMETER: Near Field ILD is ", "Disabled");
		}
		break;

		//			case PARAM_MOD_HRTF:
		//				// DEPRECATED. DO NOTHING
		//				WriteLog(state, "SET PARAMETER: HRTF convolution on/off parameter is deprecated. There might be a mismatch between your 3DTI plugin and your 3DTI API.", "");
		//				//if (value > 0.0f)
		//				//{
		//				//	data->audioSource->EnableHRTF();
		//				//	WriteLog(state, "SET PARAMETER: HRTF convolution is ", "Enabled");
		//				//}
		//				//else
		//				//{
		//				//	data->audioSource->DisableHRTF();
		//				//	WriteLog(state, "SET PARAMETER: HRTF convolution is ", "Disabled");
		//				//}
		//				break;
		//
		//			case PARAM_MAG_ANECHATT:
		//                magnitudes = spatializer().core.GetMagnitudes();
		//				magnitudes.SetAnechoicDistanceAttenuation(min(0.0f, max(-1.0e20f, value)));
		//                spatializer().core.SetMagnitudes(magnitudes);
		//				WriteLog(state, "SET PARAMETER: Anechoic distance attenuation set to (dB) ", value);
		//				break;
		//
		//			case PARAM_MAG_SOUNDSPEED:
		//                magnitudes = spatializer().core.GetMagnitudes();
		//				magnitudes.SetSoundSpeed(value);
		//                spatializer().core.SetMagnitudes(magnitudes);
		//				WriteLog(state, "SET PARAMETER: Sound speed set to (m/s) ", value);
		//				break;
		//
		//			case PARAM_DEBUG_LOG:
		//				if (value != 0.0f)
		//				{
		//					spatializer().debugLog = true;
		//					WriteLogHeader(state);
		//#if defined (SWITCH_ON_3DTI_ERRORHANDLER) || defined (_3DTI_ANDROID_ERRORHANDLER)
		//                    Common::CErrorHandler::Instance().SetErrorLogFile("3DTi_ErrorLog.txt");
		//					Common::CErrorHandler::Instance().SetVerbosityMode(VERBOSITY_MODE_ONLYERRORS);
		//					Common::CErrorHandler::Instance().SetAssertMode(ASSERT_MODE_CONTINUE);
		//#endif
		//				}
		//				else
		//					spatializer().debugLog = false;
		//				break;
		//
		//			case PARAM_HA_DIRECTIONALITY_EXTEND_LEFT:
		//				spatializer().listener->SetDirectionality_dB(Common::T_ear::LEFT, value);
		//				WriteLog(state, "SET PARAMETER: HA Directionality for Left ear set to (dB) ", value);
		//				break;
		//
		//			case PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT:
		//				spatializer().listener->SetDirectionality_dB(Common::T_ear::RIGHT, value);
		//				WriteLog(state, "SET PARAMETER: HA Directionality for Right ear set to (dB) ", value);
		//				break;
		//
		//			case PARAM_HA_DIRECTIONALITY_ON_LEFT:
		//				if (value > 0.0f)
		//				{
		//					spatializer().listener->EnableDirectionality(Common::T_ear::LEFT);
		//					WriteLog(state, "SET PARAMETER: HA Directionality switched ON for Left ear", "");
		//				}
		//				else
		//				{
		//					spatializer().listener->DisableDirectionality(Common::T_ear::LEFT);
		//					WriteLog(state, "SET PARAMETER: HA Directionality switched OFF for Left ear", "");
		//				}
		//				break;
		//
		//			case PARAM_HA_DIRECTIONALITY_ON_RIGHT:
		//				if (value > 0.0f)
		//				{
		//					spatializer().listener->EnableDirectionality(Common::T_ear::RIGHT);
		//					WriteLog(state, "SET PARAMETER: HA Directionality switched ON for Right ear", "");
		//				}
		//				else
		//				{
		//					spatializer().listener->DisableDirectionality(Common::T_ear::RIGHT);
		//					WriteLog(state, "SET PARAMETER: HA Directionality switched OFF for Right ear", "");
		//				}
		//				break;
		//
		//			case PARAM_LIMITER_SET_ON:
		//				if (value > 0.0f)
		//				{
		//					WriteLog(state, "SET PARAMETER: Limiter switched ON", "");
		//				}
		//				else
		//				{
		//					WriteLog(state, "SET PARAMETER: Limiter switched OFF", "");
		//				}
		//				break;
		//
		//			case PARAM_LIMITER_GET_COMPRESSION:
		//				WriteLog(state, "SET PARAMETER: WARNING! PARAM_LIMIT_GET_COMPRESSION is read only", "");
		//				break;
		//
		//			case PARAM_HRTF_STEP:
		//                spatializer().core.SetHRTFResamplingStep((int)value);
		//				WriteLog(state, "SET PARAMETER: HRTF resampling step set to (degrees) ", value);
		//				break;
		//
		//			case PARAM_HIGH_PERFORMANCE_ILD_FILE_STRING:	// Load ILD binary file (MANDATORY?)
		//				loadResult = BuildPathString(state, spatializer().strHighPerformanceILDpath, spatializer().strHighPerformanceILDserializing, spatializer().strHighPerformanceILDlength, spatializer().strHighPerformanceILDcount, value);
		//				if (loadResult == TLoadResult::RESULT_LOAD_END)
		//				{
		//					loadResult = LoadHighPerformanceILDBinaryFile(state);
		//					if (loadResult == TLoadResult::RESULT_LOAD_OK)
		//					{
		//						spatializer().loadedHighPerformanceILD = true;
		////						UpdateCoreIsReady();
		//						//data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::HighPerformance);
		//						//WriteLog(state, "SET PARAMETER: High Performance ILD Enabled", "");
		//					}
		//				}
		//				break;

	case PARAM_SPATIALIZATION_MODE:
		if (value == 0.0f)
		{
			if (spatializer->parameters[SpatializerCore3DTI::PARAM_IS_HIGH_QUALITY_HRTF_LOADED] == 0)
			{
				WriteLog("Error: Cannot set Spatialization mode to High Quality as no HRTF is loaded.");
				data->parameters[PARAM_SPATIALIZATION_MODE] = SPATIALIZATION_MODE_NONE;
				data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::NoSpatialization);
				data->audioSource->DisableNearFieldEffect();
			}
			else
			{
				data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::HighQuality);
				WriteLog(state, "SET PARAMETER: High Quality spatialization mode is enabled", "");
				if (spatializer->parameters[SpatializerCore3DTI::PARAM_IS_HIGH_QUALITY_ILD_LOADED])
				{
					data->audioSource->EnableNearFieldEffect();
				}
				else
				{
					data->audioSource->DisableNearFieldEffect();
				}
			}
		}
		else if (value == 1.0f)
		{
			if (spatializer->parameters[SpatializerCore3DTI::PARAM_IS_HIGH_PERFORMANCE_ILD_LOADED] == 0)
			{
				WriteLog("Error: Cannot set Spatialization mode to High Performance as no HRTF is loaded.");
				data->parameters[PARAM_SPATIALIZATION_MODE] = SPATIALIZATION_MODE_NONE;
				data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::NoSpatialization);
			}
			data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::HighPerformance);
			data->audioSource->DisableNearFieldEffect();
			WriteLog(state, "SET PARAMETER: High performance spatialization mode is enabled", "");
		}
		if (value == 2.0f)
		{
			data->audioSource->SetSpatializationMode(Binaural::TSpatializationMode::NoSpatialization);
			data->audioSource->DisableNearFieldEffect();
			WriteLog(state, "SET PARAMETER: No spatialization mode is enabled", "");
		}
		break;

	default:
		WriteLog(state, "SET PARAMETER: ERROR!!!! Unknown float parameter received from API: ", index);
		return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		break;
	}

	//spatializer().spatializerMutex.unlock();

	return UNITY_AUDIODSP_OK;
}

/////////////////////////////////////////////////////////////////////

UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char* valuestr)
{
	EffectData* data = state->GetEffectData<EffectData>();
	if (index < 0 || index >= P_NUM)
		return UNITY_AUDIODSP_ERR_UNSUPPORTED;
	if (valuestr != NULL)
		valuestr[0] = 0;

	if (value != NULL)
	{
		switch (index)
		{
			//case PARAM_LIMITER_GET_COMPRESSION:
			//	if (spatializer().limiter.IsDynamicProcessApplied())
			//		*value = 1.0f;
			//	else
			//		*value = 0.0f;
			//	break;

			//case PARAM_IS_CORE_READY:
			//	if (spatializer().isReady())
			//		*value = 1.0f;
			//	else
			//		*value = 0.0f;
			//	break;
			//
			//case PARAM_BUFFER_SIZE:
			//	//*value = float(data->bufferSize);
			//	*value = (int)state->dspbuffersize;
			//	break;

			//case PARAM_SAMPLE_RATE:
			//	//*value = float(data->sampleRate);
			//	*value = (int)state->samplerate;
			//	break;
			//case PARAM_BUFFER_SIZE_CORE:
			//	//*value = float(data->bufferSize);
//                *value = spatializer().core.GetAudioState().bufferSize;
			//	break;

			//case PARAM_SAMPLE_RATE_CORE:
			//	//*value = float(data->sampleRate);
			//	*value = spatializer().core.GetAudioState().sampleRate;
			//	break;
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
	if (inchannels != 2 || outchannels != 2 ||
		!IsHostCompatible(state) || state->spatializerdata == NULL)
	{
		WriteLog(state, "PROCESS: ERROR!!!! Wrong number of channels or Host is not compatible:", "");
		WriteLog(state, "         Input channels = ", inchannels);
		WriteLog(state, "         Output channels = ", outchannels);
		WriteLog(state, "         Host compatible = ", IsHostCompatible(state));
		WriteLog(state, "         Spatializer data exists = ", (state->spatializerdata != NULL));
		WriteLog(state, "         Buffer length = ", length);
		//memcpy(outbuffer, inbuffer, length * (size_t) outchannels * sizeof(float));
		//std::copy(inbuffer, inbuffer + length * (size_t)outchannels, outbuffer);
		// Return silence on error.
		std::fill(inbuffer, inbuffer + length * (size_t)outchannels, 0.0f);
		return UNITY_AUDIODSP_OK;
	}

	EffectData* data = state->GetEffectData<EffectData>();

	SpatializerCore3DTI::SpatializerCore* spatializer = data->spatializer;
	assert(spatializer != nullptr);

	std::lock_guard<std::mutex> lock(spatializer->mutex);

	//spatializer.spatializerMutex.lock();

	if (data->audioSource->GetSpatializationMode() == Binaural::HighQuality && !spatializer->parameters[SpatializerCore3DTI::PARAM_IS_HIGH_QUALITY_HRTF_LOADED])
	{
		//std::copy(inbuffer, inbuffer + length * (size_t)outchannels, outbuffer);
		//memset(outbuffer, 0.0f, length * (size_t)outchannels * sizeof(float));
		std::fill(inbuffer, inbuffer + length * (size_t)outchannels, 0.0f);
		return UNITY_AUDIODSP_OK;
	}
	else if (data->audioSource->GetSpatializationMode() == Binaural::HighPerformance && !spatializer->parameters[SpatializerCore3DTI::PARAM_IS_HIGH_PERFORMANCE_ILD_LOADED])
	{
		//std::copy(inbuffer, inbuffer + length * (size_t)outchannels, outbuffer);
		//memset(outbuffer, 0.0f, length * (size_t)outchannels * sizeof(float));
		std::fill(inbuffer, inbuffer + length * (size_t)outchannels, 0.0f);
		return UNITY_AUDIODSP_OK;
	}


	//// Before doing anything, check that the core is ready
	//if (!spatializer->isReady())
	//{
	//	//WriteLog(state, "PROCESS: Core is not ready yet...", "");
	//	memset(outbuffer, 0.0f, length * outchannels * sizeof(float));
//          // TODO: Use a lock guard instead of manual locking/unlocking
	  //	//spatializer->spatializerMutex.unlock();
	  //	return UNITY_AUDIODSP_OK;
	  //}

	  // Set source and listener transforms
	data->audioSource->SetSourceTransform(ComputeSourceTransformFromMatrix(state->spatializerdata->sourcematrix, spatializer->parameters[SpatializerCore3DTI::PARAM_SCALE_FACTOR]));
	spatializer->listener->SetListenerTransform(ComputeListenerTransformFromMatrix(state->spatializerdata->listenermatrix, spatializer->parameters[SpatializerCore3DTI::PARAM_SCALE_FACTOR]));

	// Now check that listener and source are not in the same position.
	// This might happens in some weird cases, such as when trying to process a source with no clip
	//if (spatializer->listener->GetListenerTransform().GetVectorTo(data->audioSource->GetSourceTransform()).GetSqrDistance() < 0.0001f)
	//{
	//	WriteLog(state, "WARNING during Process! AudioSource and Listener positions are the same (do you have a source with no clip?)", "");
	//	spatializer->spatializerMutex.unlock();
	//	return UNITY_AUDIODSP_OK;
	//}

	// Transform input buffer
	CMonoBuffer<float> inMonoBuffer(length);
	//for (int i = 0; i < length; i++)
	//{
	//	inMonoBuffer[i] = inbuffer[i * 2]; // We take only the left channel
	//}
	//for (int i = 0; i < length; i++)
	//{
	//	inMonoBuffer[i] = inbuffer[(i*2)+1]; // We take only the right channel
	//}
	size_t j = 0;
	for (size_t i = 0; i < length; i++)
	{
		inMonoBuffer[i] = (inbuffer[j] + inbuffer[j + 1]) / 2.0f;	// We take average of left and right channels
		j += 2;
	}

	// Process!!
	CMonoBuffer<float> outLeftBuffer(length);
	CMonoBuffer<float> outRightBuffer(length);
	data->audioSource->SetBuffer(inMonoBuffer);
	data->audioSource->ProcessAnechoic(outLeftBuffer, outRightBuffer);

	// Limiter
	CStereoBuffer<float> outStereoBuffer;
	outStereoBuffer.Interlace(outLeftBuffer, outRightBuffer);
	if (spatializer->parameters[SpatializerCore3DTI::PARAM_LIMITER_SET_ON])
	{
		spatializer->limiter.Process(outStereoBuffer);
	}

	// Transform output buffer
	size_t i = 0;
	for (auto it = outStereoBuffer.begin(); it != outStereoBuffer.end(); it++)
	{
		outbuffer[i++] = *it;
	}

	//spatializer->spatializerMutex.unlock();

	return UNITY_AUDIODSP_OK;
}
}
