#pragma once
#include "BinauralSpatializer/Core.h"
#include "BinauralSpatializer/Listener.h"
#include "BinauralSpatializer/Environment.h"
#include "Common/DynamicCompressorStereo.h"
#include "AudioPluginInterface.h"



namespace SpatializerCore3DTI
{

	extern "C" UNITY_AUDIODSP_EXPORT_API bool Get3DTISpatializerFloat(int parameter, float* value);

	//enum SpatializationMode : int
	//{
	//	SPATIALIZATION_MODE_HIGH_QUALITY = 0,
	//	SPATIALIZATION_MODE_HIGH_PERFORMANCE = 1,
	//	SPATIALIZATION_MODE_NONE = 2,
	//};

	// Parameters set outside of the unity Parameter system
	enum FloatParameter : int
	{
		// Values here must be kept in sync with the corresponding enum in c# code.

		// Per-source parameters. We store them in the core so we know what value to initialize the values to on a new source instance.
		EnableHRTFInterpolation = 0, // ### SOURCE ####
		FirstSourceParameter = EnableHRTFInterpolation,
		EnableFarDistanceLPF = 1, // ### SOURCE ####
		EnableDistanceAttenuation = 2, // ### SOURCE ####
		EnableNearFieldILD = 3,// ### SOURCE ####
		SpatializationMode = 4,// ### SOURCE ####
		//EnableReverb,

		NumSourceParameters = 5,

		// Core parameters
		HeadRadius = 5,
		ScaleFactor = 6,
		EnableCustomITD = 7,
		AnechoicDistanceAttenuation = 8,
		SoundSpeed = 9,
		HearingAidDirectionalityAttenuationLeft = 10,
		HearingAidDirectionalityAttenuationRight = 11,
		EnableHearingAidDirectionalityLeft = 12,
		EnableHearingAidDirectionalityRight = 13,
		EnableLimiter = 14,
		HRTFResamplingStep = 15,
		// The following are per-source parameters. We store their values on the SptializerCore plugin as initialization values for when a source is instantiated.
		NumFloatParameters = 16,
	};

		// Define unity parameters separately
	enum UnityParameters
	{
		// Read only status parameters
		PARAM_IS_HIGH_QUALITY_HRTF_LOADED,
		PARAM_IS_HIGH_QUALITY_ILD_LOADED,
		PARAM_IS_HIGH_PERFORMANCE_ILD_LOADED,
		PARAM_IS_REVERB_BRIR_LOADED,

		P_NUM
	};

	enum BinaryRole
	{
		// Must be kept in sync with c# code
		HighPerformanceILD = 0,
		HighQualityHRTF = 1,
		HighQualityILD = 2,
		ReverbBRIR = 3,
	};

/////////////////////////////////////////////////////////////////////

	struct SpatializerCore
	{
		// Each instance of the reverb effect has an instance of the Core
		Binaural::CCore core;
		std::shared_ptr<Binaural::CListener> listener;
		std::shared_ptr<Binaural::CEnvironment> environment;
		Common::CDynamicCompressorStereo limiter;
		float perSourceInitialValues[NumSourceParameters];
		float unityParameters[P_NUM];
		float scaleFactor;
		bool isLimiterEnabled;
		std::mutex mutex;

	protected:
		SpatializerCore(UInt32 sampleRate, UInt32 bufferSize);

	public:
		~SpatializerCore();

		bool loadBinary(BinaryRole role, std::string path);
		//bool loadBinaries(std::string hrtfPath,	std::string ildPath, std::string highPerformanceILDPath, std::string brirPath);


		class TooManyInstancesException : public std::exception
		{
		public:
			virtual char const* what() const;
		};

		/// Create instance. Throws TooManyInstancesException if instance already exists.
		static SpatializerCore* create(UInt32 sampleRate, UInt32 bufferSize);

		/// \return instance or null if nullptr have been made yet
		static SpatializerCore* instance();

		


	private:
		static SpatializerCore*& instancePtr();
	};
}