#pragma once
#include "BinauralSpatializer/Core.h"
#include "BinauralSpatializer/Listener.h"
#include "BinauralSpatializer/Environment.h"
#include "Common/DynamicCompressorStereo.h"
#include "AudioPluginInterface.h"



namespace SpatializerCore3DTI
{
	enum SpatializationMode : int
	{
		SPATIALIZATION_MODE_HIGH_QUALITY = 0,
		SPATIALIZATION_MODE_HIGH_PERFORMANCE = 1,
		SPATIALIZATION_MODE_NONE = 2,
	};

	// Parameters set outside of the unity Parameter system
	enum FloatParameter : int
	{
		PARAM_HEAD_RADIUS = 0,
		PARAM_SCALE_FACTOR = 1,
		PARAM_CUSTOM_ITD = 2,
		PARAM_MAG_ANECHATT = 3,
		PARAM_MAG_SOUNDSPEED = 4,
		PARAM_HA_DIRECTIONALITY_EXTEND_LEFT = 5,
		PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT = 6,
		PARAM_HA_DIRECTIONALITY_ON_LEFT = 7,
		PARAM_HA_DIRECTIONALITY_ON_RIGHT = 8,
		PARAM_LIMITER_SET_ON = 9,
		PARAM_HRTF_STEP = 10,
		NumFloatParameters = 11,
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


/////////////////////////////////////////////////////////////////////

	struct SpatializerCore
	{
		// Each instance of the reverb effect has an instance of the Core
		Binaural::CCore core;
		std::shared_ptr<Binaural::CListener> listener;
		std::shared_ptr<Binaural::CEnvironment> environment;
		Common::CDynamicCompressorStereo limiter;
		// TODO: Most of these are not used as we set parameters directly in the above classes
		float unityParameters[P_NUM];
		float scaleFactor;
		bool isLimiterEnabled;
		std::mutex mutex;

	protected:
		SpatializerCore(UInt32 sampleRate, UInt32 bufferSize);

	public:
		~SpatializerCore();

		bool loadBinaries(std::string hrtfPath,	std::string ildPath, std::string highPerformanceILDPath, std::string brirPath);


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