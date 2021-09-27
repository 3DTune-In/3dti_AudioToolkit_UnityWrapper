#include "SpatializerCore.h"
#include "CommonUtils.h"
#include "BRIR/BRIRCereal.h"
#include "HRTF/HRTFFactory.h"
#include "ILD/ILDCereal.h"

using Common::T_ear;

namespace SpatializerCore3DTI
{
	extern "C" UNITY_AUDIODSP_EXPORT_API bool Reset3DTISpatializerIfNeeded(int sampleRate, int dspBufferSize)
	{
		std::lock_guard<std::mutex> lock(SpatializerCore::mutex());

		return SpatializerCore::resetInstanceIfNecessary(sampleRate, dspBufferSize);
	}


	extern "C" UNITY_AUDIODSP_EXPORT_API bool Load3DTISpatializerBinary(BinaryRole role, const char* path, int currentSampleRate, int dspBufferSize) 
	{
		std::lock_guard<std::mutex> lock(SpatializerCore::mutex());

		SpatializerCore* instance = SpatializerCore::instance(currentSampleRate, dspBufferSize);
		if (instance == nullptr)
		{
			WriteLog("Error: setup3DTISpatializer called with incorrect sample rate or buffer size.");
			return false;
		}
		return instance->loadBinary(role, path);
	}

	extern "C" UNITY_AUDIODSP_EXPORT_API bool Set3DTISpatializerFloat(int parameter, float value)
	{

		std::lock_guard<std::mutex> lock(SpatializerCore::mutex());

		SpatializerCore* spatializer = SpatializerCore::instance();
		if (spatializer == nullptr)
		{
			return false;
		}
		return spatializer->SetFloat(parameter, value);
	}


	extern "C" UNITY_AUDIODSP_EXPORT_API bool Get3DTISpatializerFloat(int parameter, float* value)
	{
		assert(value != nullptr);

		std::lock_guard<std::mutex> lock(SpatializerCore::mutex());

		SpatializerCore* spatializer = SpatializerCore::instance();

		return spatializer->GetFloat(parameter, value);
	}



	SpatializerCore::SpatializerCore(UInt32 sampleRate, UInt32 bufferSize)
		: scaleFactor(1.0f)
		, isLimiterEnabled(true)
		, enableReverbProcessing(false)
	{
		perSourceInitialValues[EnableHRTFInterpolation] = 1.0f;
		perSourceInitialValues[EnableFarDistanceLPF] = 1.0f;
		perSourceInitialValues[EnableDistanceAttenuationAnechoic] = 1.0f;
		perSourceInitialValues[EnableNearFieldEffect] = 1.0f;
		perSourceInitialValues[SpatializationMode] = 0.0f;

		Common::TAudioStateStruct audioState;
		audioState.sampleRate = sampleRate;
		audioState.bufferSize = bufferSize;
		core.SetAudioState(audioState);
		listener = core.CreateListener();

		const float LimiterThreshold = -30.0f;
		const float LimiterAttack = 500.0f;
		const float LimiterRelease = 500.0f;
		const float LimiterRatio = 6;
		limiter.Setup(sampleRate, LimiterRatio, LimiterThreshold, LimiterAttack, LimiterRelease);

		environment = core.CreateEnvironment();
	}


	SpatializerCore::~SpatializerCore()
	{
		assert(instancePtr() == this);
		instancePtr() = nullptr;
	}




	bool SpatializerCore::loadBinary(BinaryRole role, std::string path)
	{
		const string sofaExtension = ".sofa"s;

		switch (role)
		{
		case HighQualityHRTF:
#ifdef UNITY_WIN
			if (path.size() >= sofaExtension.size() && path.substr(path.size() - sofaExtension.size()) == sofaExtension)
			{
				// We assume an ILD file holds the delays, so our SOFA file does not specify delays
				bool specifiedDelays = false;
				isBinaryResourceLoaded[HighQualityHRTF] = HRTF::CreateFromSofa(path, listener, specifiedDelays);
			}
			// If not sofa file then assume its a 3dti-hrtf file
			else
#endif
			{
				isBinaryResourceLoaded[HighQualityHRTF] = HRTF::CreateFrom3dti(path, listener);
			}
			return isBinaryResourceLoaded[HighQualityHRTF];
		case HighQualityILD:
			isBinaryResourceLoaded[HighQualityILD] = ILD::CreateFrom3dti_ILDNearFieldEffectTable(path, listener);
			return isBinaryResourceLoaded[HighQualityILD];
		case HighPerformanceILD:
			isBinaryResourceLoaded[HighPerformanceILD] = ILD::CreateFrom3dti_ILDSpatializationTable(path, listener);
			return isBinaryResourceLoaded[HighPerformanceILD];
		case ReverbBRIR:
			isBinaryResourceLoaded[ReverbBRIR] = BRIR::CreateFrom3dti(path, environment);
			return isBinaryResourceLoaded[ReverbBRIR];
		default:
			return false;
		}
	}

	bool SpatializerCore::SetFloat(int parameter, float value)
	{
		switch (parameter)
		{
		case EnableHRTFInterpolation:
		case EnableFarDistanceLPF:
		case EnableDistanceAttenuationAnechoic:
		case EnableNearFieldEffect:
		case SpatializationMode:
		case EnableReverbSend:
		case EnableDistanceAttenuationReverb:
			perSourceInitialValues[parameter] = value;
			return true;

		case HeadRadius:
		{
			const float min = 0.0f;
			const float max = 1e20f;
			//const float def = 0.0875f;
			listener->SetHeadRadius(clamp(value, min, max));
			return true;
		}
		case ScaleFactor:
		{
			const float min = 1e-20f;
			const float max = 1e20f;
			//const float def = 1.0f;
			scaleFactor = clamp(value, min, max);
			return true;
		}
		case EnableCustomITD:
		{
			if (value == 0.0f)
			{
				listener->DisableCustomizedITD();
			}
			else
			{
				listener->EnableCustomizedITD();
			}
			return true;
		}
		case AnechoicDistanceAttenuation:
		{
			const float min = -30.0f;
			const float max = 0.0f;
			Common::CMagnitudes magnitudes = core.GetMagnitudes();
			magnitudes.SetAnechoicDistanceAttenuation(clamp(value, min, max));
			core.SetMagnitudes(magnitudes);
			return true;
		}
		case ILDAttenuation:
		{
			const float min = 0.0f;
			const float max = 30.0f;
			listener->SetILDAttenutaion(clamp(value, min, max));
			return true;
		}
		case SoundSpeed:
		{
			const float min = 10.0f;
			const float max = 1000.0f;
			Common::CMagnitudes magnitudes = core.GetMagnitudes();
			magnitudes.SetSoundSpeed(clamp(value, min, max));
			core.SetMagnitudes(magnitudes);
			return true;
		}
		case HearingAidDirectionalityAttenuationLeft:
		{
			const float min = 0.0f;
			const float max = 30.0f;
			listener->SetDirectionality_dB(Common::T_ear::LEFT, clamp(value, min, max));
			return true;
		}
		case HearingAidDirectionalityAttenuationRight:
		{
			const float min = 0.0f;
			const float max = 30.0f;
			listener->SetDirectionality_dB(Common::T_ear::RIGHT, clamp(value, min, max));
			return true;
		}
		case EnableHearingAidDirectionalityLeft:
		{
			if (value == 0.0f)
			{
				listener->DisableDirectionality(Common::T_ear::LEFT);
			}
			else
			{
				listener->EnableDirectionality(Common::T_ear::LEFT);
			}
			return true;
		}
		case EnableHearingAidDirectionalityRight:
		{
			if (value == 0.0f)
			{
				listener->DisableDirectionality(Common::T_ear::RIGHT);
			}
			else
			{
				listener->EnableDirectionality(Common::T_ear::RIGHT);
			}
			return true;
		}
		case EnableLimiter:
		{
			isLimiterEnabled = value != 0.0f;
			return true;
		}
		case HRTFResamplingStep:
		{
			const float min = 1.0f;
			const float max = 90.0f;
			core.SetHRTFResamplingStep((int)clamp(value, min, max));
			return true;
		}
		case EnableReverbProcessing:
			enableReverbProcessing = value != 0.0f;
			return true;
		case ReverbOrder:
			static_assert((float)ADIMENSIONAL == 0.0f && (float)BIDIMENSIONAL == 1.0f && (float)THREEDIMENSIONAL == 2.0f, "These values are assumed by this code and the correspond c# enumerations.");
			if (value == (float)ADIMENSIONAL)
			{
				environment->SetReverberationOrder(ADIMENSIONAL);
			}
			else if (value == (float)BIDIMENSIONAL)
			{
				environment->SetReverberationOrder(BIDIMENSIONAL);
			}
			else if (value == (float)THREEDIMENSIONAL)
			{
				environment->SetReverberationOrder(THREEDIMENSIONAL);
			}
			else
			{
				WriteLog("ERROR: Set3DTISpatializerFloat with parameter ReverbOrder only supports values 0.0, 1.0 and 2.0. Value received: " + to_string(value));
				return false;
			}
			return true;

		default:
			return false;
		}
	}


	bool SpatializerCore::GetFloat(int parameter, float* value)
	{
		assert(value != nullptr);
		if (value == nullptr)
		{
			return false;
		}

		switch (parameter)
		{
		case EnableHRTFInterpolation:
		case EnableFarDistanceLPF:
		case EnableDistanceAttenuationAnechoic:
		case EnableNearFieldEffect:
		case SpatializationMode:
		case EnableReverbSend:
		case EnableDistanceAttenuationReverb:
			*value = perSourceInitialValues[parameter];
			return true;
		case HeadRadius:
			*value = listener->GetHeadRadius();
			return true;
		case ScaleFactor:
			*value = scaleFactor;
			return true;
		case EnableCustomITD:
			*value = listener->IsCustomizedITDEnabled() ? 1.0f : 0.0f;
			return true;
		case AnechoicDistanceAttenuation:
			*value = core.GetMagnitudes().GetAnechoicDistanceAttenuation();
			return true;
		case ILDAttenuation:
			*value = listener->GetILDAttenutaion();
			return true;
		case SoundSpeed:
			*value = core.GetMagnitudes().GetSoundSpeed();
			return true;
		case HearingAidDirectionalityAttenuationLeft:
			*value = listener->GetAnechoicDirectionalityAttenuation_dB(T_ear::LEFT);
			return true;
		case HearingAidDirectionalityAttenuationRight:
			*value = listener->GetAnechoicDirectionalityAttenuation_dB(T_ear::RIGHT);
			return true;
		case EnableHearingAidDirectionalityLeft:
			*value = listener->IsDirectionalityEnabled(T_ear::LEFT);
			return true;
		case EnableHearingAidDirectionalityRight:
			*value = listener->IsDirectionalityEnabled(T_ear::RIGHT);
			return true;
		case EnableLimiter:
			*value = isLimiterEnabled ? 1.0f : 0.0f;
			return true;
		case HRTFResamplingStep:
			*value = (float)core.GetHRTFResamplingStep();
			return true;
		case EnableReverbProcessing:
			*value = (float)EnableReverbProcessing;
			return true;
		case ReverbOrder:
			*value = (float)environment->GetReverberationOrder();
			return true;
		default:
			*value = std::numeric_limits<float>::quiet_NaN();
			return false;
		}


	}




	SpatializerCore* SpatializerCore::instance(UInt32 sampleRate, UInt32 bufferSize)
	{
		SpatializerCore*& s = instancePtr();
		if (s == nullptr)
		{
			s = new SpatializerCore(sampleRate, bufferSize);
		}
		if (s->core.GetAudioState().sampleRate != sampleRate ||s->core.GetAudioState().bufferSize != bufferSize)
		{
			throw IncorrectAudioStateException(sampleRate, bufferSize, s->core.GetAudioState().sampleRate, s->core.GetAudioState().bufferSize);
		}
		return s;
	}

	SpatializerCore* SpatializerCore::instance()
	{
		return instancePtr();
	}

	bool SpatializerCore::resetInstanceIfNecessary(UInt32 sampleRate, UInt32 bufferSize)
	{
		SpatializerCore*& s = instancePtr();
		if (s != nullptr && (s->core.GetAudioState().sampleRate != sampleRate || s->core.GetAudioState().bufferSize != bufferSize))
		{
			delete s;
			assert(s == nullptr); // this is done by the destructor
		}
		if (s == nullptr)
		{
			s = new SpatializerCore(sampleRate, bufferSize);
			return true;
		}
		return false;
	}

	SpatializerCore*& SpatializerCore::instancePtr()
	{
		static SpatializerCore* s = nullptr;
		return s;
	}


}