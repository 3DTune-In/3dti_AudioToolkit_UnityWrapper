#pragma once
#include "BinauralSpatializer/Core.h"
#include "BinauralSpatializer/Listener.h"
#include "BinauralSpatializer/Environment.h"
#include "Common/DynamicCompressorStereo.h"
#include "AudioPluginInterface.h"
#include <array>



namespace SpatializerCore3DTI
{

	extern "C" UNITY_AUDIODSP_EXPORT_API bool Get3DTISpatializerFloat(int parameter, float* value);

	// Parameters set outside of the unity Parameter system
	enum FloatParameter : int
	{
		// Values here must be kept in sync with the corresponding enum in c# code.

		// Per-source parameters. We store them in the core so we know what value to initialize the values to on a new source instance.
		EnableHRTFInterpolation = 0, // ### SOURCE ####
		FirstSourceParameter = EnableHRTFInterpolation,
		EnableFarDistanceLPF = 1, // ### SOURCE ####
		EnableDistanceAttenuationAnechoic = 2, // ### SOURCE ####
		EnableNearFieldEffect = 3,// ### SOURCE ####
		SpatializationMode = 4,// ### SOURCE ####
		EnableReverb = 5,
		EnableDistanceAttenuationReverb = 6,

		NumSourceParameters = 7,

		// Listener parameters
		HeadRadius = 7,
		FirstListenerParameter = HeadRadius,
		ScaleFactor = 8,
		EnableCustomITD = 9,
		AnechoicDistanceAttenuation = 10,
		ILDAttenuation = 11,
		SoundSpeed = 12,
		HearingAidDirectionalityAttenuationLeft = 13,
		HearingAidDirectionalityAttenuationRight = 14,
		EnableHearingAidDirectionalityLeft = 15,
		EnableHearingAidDirectionalityRight = 16,
		EnableLimiter = 17,
		HRTFResamplingStep = 18,
		ReverbOrder = 19,

		NumFloatParameters = 20,
	};


	enum BinaryRole
	{
		// Must be kept in sync with c# code
		HighPerformanceILD = 0,
		HighQualityHRTF = 1,
		HighQualityILD = 2,
		ReverbBRIR = 3,
		NumBinaryRoles = 4,
	};

/////////////////////////////////////////////////////////////////////

	struct SpatializerCore
	{
		// Each instance of the reverb effect has an instance of the Core
		Binaural::CCore core;
		std::shared_ptr<Binaural::CListener> listener;
		std::shared_ptr<Binaural::CEnvironment> environment;
		Common::CDynamicCompressorStereo limiter;
        std::array<float, NumSourceParameters> perSourceInitialValues;
		//float unityParameters[P_NUM];
		float scaleFactor;
		bool isLimiterEnabled;
		std::mutex mutex;

		// Status
		std::array<bool, NumBinaryRoles> isBinaryResourceLoaded = { false, false, false, false };

	protected:
		SpatializerCore(UInt32 sampleRate, UInt32 bufferSize);

	public:
		~SpatializerCore();

		bool loadBinary(BinaryRole role, std::string path);
		//bool loadBinaries(std::string hrtfPath,	std::string ildPath, std::string highPerformanceILDPath, std::string brirPath);


		class TooManyInstancesException : public std::runtime_error
		{
        public:
            TooManyInstancesException()
            : std::runtime_error("SpatializerCore already exists. Only one SpatializerCore instance is currently supported.")
            {}
		};

		/// Create instance. Throws TooManyInstancesException if instance already exists.
		static SpatializerCore* create(UInt32 sampleRate, UInt32 bufferSize);

		/// \return instance or null if nullptr have been made yet
		static SpatializerCore* instance();

		


	private:
		static SpatializerCore*& instancePtr();
	};
}
