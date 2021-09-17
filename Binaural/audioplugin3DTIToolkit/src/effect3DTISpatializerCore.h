#pragma once
#include "BinauralSpatializer/Core.h"
#include "BinauralSpatializer/Listener.h"
#include "BinauralSpatializer/Environment.h"
#include "Common/DynamicCompressorStereo.h"
#include "AudioPluginInterface.h"



namespace SpatializerCore3DTI
{
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
		PARAM_HRTF_INTERPOLATION = 0, // ### SOURCE ####
		PARAM_MOD_FARLPF = 1, // ### SOURCE ####
		PARAM_MOD_DISTATT = 2, // ### SOURCE ####
		PARAM_MOD_NEAR_FIELD_ILD = 3,// ### SOURCE ####
		PARAM_SPATIALIZATION_MODE = 4,// ### SOURCE ####
		NumSourceParameters = 5,

		// Core parameters
		PARAM_HEAD_RADIUS = 5,
		PARAM_SCALE_FACTOR = 6,
		PARAM_CUSTOM_ITD = 7,
		PARAM_MAG_ANECHATT = 8,
		PARAM_MAG_SOUNDSPEED = 9,
		PARAM_HA_DIRECTIONALITY_EXTEND_LEFT = 10,
		PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT = 11,
		PARAM_HA_DIRECTIONALITY_ON_LEFT = 12,
		PARAM_HA_DIRECTIONALITY_ON_RIGHT = 13,
		PARAM_LIMITER_SET_ON = 14,
		PARAM_HRTF_STEP = 15,
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