#include "AudioPluginUtil.h"

#include "../../3DTI_Toolkit_Core/Core.h"

#define LOG_FILE

// Includes for reading HRTF data
//#include <tchar.h>
//#include <regex>
#include <fstream>
#include <iostream>
#include <time.h>

using namespace std;

namespace UnityWrapper3DTI
{
	enum
	{
		P_DUMMY,
		P_NUM
	};

/////////////////////////////////////////////////////////////////////

	struct EffectData
	{
		float p[P_NUM];		
		std::shared_ptr<CSingleSourceDSP> source;
		CCore *core;
	};

/////////////////////////////////////////////////////////////////////

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

//	class WriteLog
//	{
//	public:
//		WriteLog(UnityAudioEffectState* _state) 
//		{ 
//			state = _state; 
//#ifdef LOG_FILE
//			state->GetEffectData<EffectData>()->logfile << logText << endl;
//#endif
//		}
//		template <class T>
//		WriteLog& operator<<(const T& value) {
//			logText << value << padding;
//			return *this;
//		}
//		//WriteLog& operator<<(std::ostream& (*func)(string&))
//		//{
//		//	func(logText);
//		//	//state->GetEffectData<EffectData>()->logfile << logtext << endl;
//		//	return *this;
//		//}
//	private:
//		UnityAudioEffectState* state;		
//		string logText;
//		//std::ostringstream logText;
//	};

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
		*attenuationOut = attenuationIn;
		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	//void ReadHRIR(const TCHAR* fileName, CHRTF myHead)
	//{
	//	/* For each subject, there are 187 stereo 24 bits WAV files,
	//	one for each position of the source. The generic name is:
	//	IRC_<subject_ID>_<status>_R<radius>_T<azimuth>_P<elevation>.WAV */
	//	std::regex patAz("_T[0-9]{3}"), patEl("_P[0-9]{3}"); // Pattern for azimuth and elevation.
	//	string filename = fileName;		// WARNING!! This requires this project setting in VS: General->Character Set -> Multi-byte
	//	std::sregex_iterator itAz(filename.begin(), filename.end(), patAz), itEl(filename.begin(), filename.end(), patEl);
	//	string sAz = itAz->str(), sEl = itEl->str();
	//	sAz.erase(0, 2); sEl.erase(0, 2);

	//	// For the moment just load one HRIR, the one corresponding with current quantized azimuth and quantized elevation.
	//	int tempAzimuth = stoi(sAz), tempElevation = stoi(sEl);

	//	float* hrir_left, hrir_right; const int left = 0, right = 1;
	//	hrir_left.load(file->getAbsolutePath(), left);
	//	hrir_right.load(file->getAbsolutePath(), right);

	//	// Take smallest
	//	unsigned int length = (512 < hrir_left.length) ? 512 : hrir_left.length;
	//	CMonoBuffer<float> hrir_left_float(length);
	//	CMonoBuffer<float> hrir_right_float(length);
	//	for (int i = 0; i < length; i++)
	//	{
	//		hrir_left_float[i] = static_cast<double>(hrir_left.temp[i]) / 32767.0; // cast from short
	//	}
	//	for (int i = 0; i < length; i++)
	//	{
	//		hrir_right_float[i] = static_cast<double>(hrir_right.temp[i]) / 32767.0;
	//	}
	//	HRIR_type onehrir = std::make_pair(hrir_left_float, hrir_right_float);
	//	myHead.AddHRIR(tempAzimuth, tempElevation, std::move(onehrir));

	//}

/////////////////////////////////////////////////////////////////////

	//CHRTF SetupHRTF()
	//{
	//	// Things to do for opening all files in a folder in Windows
	//	WIN32_FIND_DATA FindFileData;
	//	HANDLE hFind;
	//	//TCHAR  *directory = TEXT("G:\\Work");
	//	TCHAR *directory = TEXT(".\HRTFData");
	//	TCHAR patter[MAX_PATH];
	//	TCHAR fileName[MAX_PATH];
	//	memset(patter, 0x00, MAX_PATH);
	//	_stprintf(patter, TEXT("%s\\*.wav"), directory);
	//	hFind = FindFirstFile(patter, &FindFileData);
	//	if (hFind == INVALID_HANDLE_VALUE)
	//	{
	//		// ERROR!!!
	//		return CHRTF();
	//	}

	//	// TO DO: Avoid these hardcoded constants
	//	float azimuthStep = 15;
	//	int buffersize = 512;

	//	/// Start filling HRTF matrix
	//	CHRTF myHead(azimuthStep, 15);
	//	myHead.BeginSetup(buffersize); 

	//	// Now read the wav files
	//	do
	//	{
	//		//ignore current and parent directories
	//		//if (_tcscmp(FindFileData.cFileName, TEXT(".")) == 0 || _tcscmp(FindFileData.cFileName, TEXT("..")) == 0)
	//		//	continue;

	//		if (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
	//		{
	//			//ignore directories
	//		}
	//		else
	//		{
	//			//list the Files
	//			_tprintf(TEXT("%s\n"), FindFileData.cFileName);
	//			memset(fileName, 0x00, sizeof(fileName));
	//			_stprintf(fileName, TEXT("%s\\%s"), directory, FindFileData.cFileName);
	//			FILE *fptr = _tfopen((const TCHAR *)fileName, TEXT("r"));
	//			
	//			// Read one HRIR
	//			ReadHRIR(fileName, myHead);				

	//			fclose(fptr);
	//			
	//		}
	//	} while (FindNextFile(hFind, &FindFileData));
	//	FindClose(hFind);
	//	
	//	/// Stop filling HRTF matrix and load
	//	myHead.EndSetup();
	//}

	CHRTF SetupHRTF()
	{
		// No error check
		// This is a horrible hardcoded quick test
		
		// Create CHRTF object with needed size
		//HRIR_type dummyHRIR;
		//CHRTF dummyHRTF(15,15);
		//dummyHRTF.BeginSetup(512);
		//for (int i = 0; i < 187; i++)
		//	dummyHRTF.AddHRIR(0.0f, 0.0f, move(dummyHRIR));
		//dummyHRTF.EndSetup();
		//int headsize = sizeof(dummyHRTF);

		//// Read from binary file
		//CHRTF* readHRTF;
		//ifstream ifs("binaryHRTF.hrtf", ios::binary);
		//ifs.read((char *)readHRTF, headsize);		
		//ifs.close();
		//
		//ofstream logfile;
		//logfile.open("debuglog.txt", ofstream::out | ofstream::app);
		//logfile << "HRTF Read for 180,0: " << endl;
		//CMonoBuffer<float> LeftChannel = readHRTF->GetInterpolatedHRIR(180, 0, true).first;
		//for (int i = 0; i < 512; i++)
		//	logfile << "	" << LeftChannel[i] <<  endl;
		//logfile.close();

		//return *readHRTF;

		CHRTF readHRTF;
		if (readHRTF.ReadFromFile("HRTF.txt") < 0)
			WriteLog("	Could not open HRTF.txt file", "");
		else
			WriteLog("	File HRTF.txt read!", "");
		
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

		// Start log file
		time_t rawtime;
		struct tm * timeinfo;
		char buffer[80];
		time(&rawtime);
		timeinfo = localtime(&rawtime);
		strftime(buffer, 80, "%d-%m-%Y %I:%M:%S", timeinfo);
		std::string str(buffer);
		WriteLog("***************************************************************************************\nDebug log started at ", str);

		// Core initialization
		CCore *core = effectdata->core;		
		core = new CCore();
		WriteLog("Core initialized", "");

		// Set audio state
		CAudioState audioState;
		audioState.SetSampleRate((int) state->samplerate);
		core->SetAudioState(audioState);		
		WriteLog("Sample rate set to ", state->samplerate);

		// Set listener transform		: TO DO
		//float L[16];					// Inverted 4x4 listener matrix, as provided by Unity
		//for (int i = 0; i < 16; i++)
		//	L[i] = state->spatializerdata->listenermatrix[i];
		//float listenerpos_x = -(L[0] * L[12] + L[1] * L[13] + L[2] * L[14]);	// From Unity documentation
		//float listenerpos_y = -(L[4] * L[12] + L[5] * L[13] + L[6] * L[14]);	// From Unity documentation
		//float listenerpos_z = -(L[8] * L[12] + L[9] * L[13] + L[10] * L[14]);	// From Unity documentation
		CTransform listenerTransform;
		listenerTransform.SetOrientation(CQuaternion::UNIT);
		listenerTransform.SetPosition(CVector3(0.0f, 0.0f, 0.0f));
		core->SetListenerTransform(listenerTransform);
		WriteLog("Listener transform set", "");

		// Set listener head circumference : TO DO

		// Setup listener HRTF	
		WriteLog("Setting listener HRTF...", "");
		CHRTF listenerHRTF = SetupHRTF();
		core->LoadHRTF(std::move(listenerHRTF));
		WriteLog("	Listener HRTF set.", "");

		//ofstream logfile;
		//logfile.open("debuglog.txt", ofstream::out | ofstream::app);
		//logfile << "Listener HRTF for 180,0: " << endl;
		//CMonoBuffer<float> leftChannel = listenerHRTF.GetInterpolatedHRIR(180, 0, 3).first;
		//for (int i = 0; i < 512; i++)
		//	logfile << "	" << leftChannel[i] << endl;
		//logfile.close();

		// TO DO: Setup room

		// Create source and set transform		
		effectdata->source = core->CreateSingleSourceDSP();
		CTransform sourceTransform;
		sourceTransform.SetPosition(CVector3(state->spatializerdata->sourcematrix[12], 
											 state->spatializerdata->sourcematrix[13], 
											 state->spatializerdata->sourcematrix[14]));	
		effectdata->source->SetSourceTransform(sourceTransform);			
		WriteLog("Source position set to ", sourceTransform.GetPosition());

		return UNITY_AUDIODSP_OK;
	}

/////////////////////////////////////////////////////////////////////

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();
#ifdef LOG_FILE
		//data->logfile.close();
#endif
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
		//WriteLog("Starting process with input buffer size (converted to MONO): ", length/2);

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
		float distanceScale = 0.0001f;
		CTransform sourceTransform;
		sourceTransform.SetPosition(CVector3(s[12]*distanceScale, s[13]*distanceScale, s[14]*distanceScale));
		data->source->SetSourceTransform(sourceTransform);
		//WriteLog("	Source position set to ", sourceTransform.GetPosition());

		// We assume a fixed listener
		
		// Transform input buffer
		// TO DO: Avoid this copy!!!!!!
		CStereoBuffer<float> outStereoBuffer(length);
		int monoLength = length / 2;
		CMonoBuffer<float> inMonoBuffer(monoLength);
		for (int i = 0; i < monoLength; i++)
		{
			inMonoBuffer[i] = inbuffer[i * 2];
		}

		// Process!!
		data->source->SetInterpolation(3);
		data->source->ProcessAnechoic(inMonoBuffer, outStereoBuffer);				

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
