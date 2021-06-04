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

	enum
	{
		PARAM_HEAD_RADIUS,
		PARAM_SCALE_FACTOR,
		PARAM_CUSTOM_ITD,
		PARAM_MAG_ANECHATT,
		PARAM_MAG_SOUNDSPEED,

		// HA directionality
		PARAM_HA_DIRECTIONALITY_EXTEND_LEFT,
		PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT, // 15
		PARAM_HA_DIRECTIONALITY_ON_LEFT,
		PARAM_HA_DIRECTIONALITY_ON_RIGHT,

		// Limiter
		PARAM_LIMITER_SET_ON,


		// HRTF resampling step
		PARAM_HRTF_STEP,

		// High Performance and None modes
		//PARAM_HIGH_PERFORMANCE_ILD_FILE_STRING,
		//PARAM_SPATIALIZATION_MODE,
		//PARAM_BUFFER_SIZE,
		//PARAM_SAMPLE_RATE,
		//PARAM_BUFFER_SIZE_CORE,
		//PARAM_SAMPLE_RATE_CORE,
		// Read only status parameters
		PARAM_IS_HIGH_QUALITY_HRTF_LOADED,
		PARAM_IS_HIGH_QUALITY_ILD_LOADED,
		PARAM_IS_HIGH_PERFORMANCE_ILD_LOADED,
		PARAM_IS_REVERB_BRIR_LOADED,

		PARAM_LIMITER_GET_COMPRESSION,

		P_NUM
	};

	// readonly parameters to mvoe to getfloatbuffer method
	//PARAM_LIMITER_GET_COMPRESSION,
		//PARAM_IS_CORE_READY, // 20






/////////////////////////////////////////////////////////////////////

	struct SpatializerCore
	{
		// Each instance of the reverb effect has an instance of the Core
		Binaural::CCore core;
		std::shared_ptr<Binaural::CListener> listener;
		std::shared_ptr<Binaural::CEnvironment> environment;
		Common::CDynamicCompressorStereo limiter;
		// TODO: Most of these are not used as we set parameters directly in the above classes
		float parameters[P_NUM];
		std::mutex mutex;

	protected:
		SpatializerCore(UInt32 sampleRate, UInt32 bufferSize);

	public:
		~SpatializerCore();

		bool loadBinaries(std::string hrtfPath,	std::string ildPath, std::string highPerformanceILDPath,std::string brirPath);


		class TooManyInstancesEception : public std::exception
		{
		public:
			virtual char const* what() const;
		};

		/// Create instance. Throws TooManyInstancesEception if instance already exists.
		static SpatializerCore* create(UInt32 sampleRate, UInt32 bufferSize);

		/// \return instance or null if nullptr have been made yet
		static SpatializerCore* instance();

		


	private:
		static SpatializerCore*& instancePtr();
	};
}