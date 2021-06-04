#pragma once
#include "AudioPluginUtil.h"
#include <BinauralSpatializer/Core.h>
#include <Common/DynamicCompressorStereo.h>
#include "effect3DTISpatializerCore.h"


namespace SpatializerSource3DTI
{
    

    
    //UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state);
    
//    // Class holding state for the whole plugin (we have a single
//    // listener as permitted by Unity). This is effectively a
//    // singleton class, though for simplicity we just hold an
//    // instance in a free variable rather than using a static member.
//    class Spatializer
//    {
//    public:
//        std::shared_ptr<Binaural::CListener> listener;
//        Binaural::CCore core;
//        std::shared_ptr<Binaural::CEnvironment> environment;
//        // replaced with function
////        bool coreReady;
//        bool loadedHRTF;                // New
//        bool loadedNearFieldILD;        // New
//        bool loadedHighPerformanceILD;    // New
//        int spatializationMode;            // New
//        float parameters[P_NUM];
//        
//        // STRING SERIALIZER
//        char* strHRTFpath;
//        bool strHRTFserializing;
//        int strHRTFcount;
//        int strHRTFlength;
//        char* strNearFieldILDpath;
//        bool strNearFieldILDserializing;
//        int strNearFieldILDcount;
//        int strNearFieldILDlength;
//        char* strHighPerformanceILDpath;
//        bool strHighPerformanceILDserializing;
//        int strHighPerformanceILDcount;
//        int strHighPerformanceILDlength;
//        
//        // Limiter
//        Common::CDynamicCompressorStereo limiter;
//        
//        // DEBUG LOG
//        bool debugLog = false;
//        
//        // MUTEX
//        std::mutex spatializerMutex;
//        
//        Spatializer();
//        
//        bool isInitialized() const
//        {
//            return listener != nullptr;
//        }
//        
//        bool isReady() const
//        {
//            switch (spatializationMode)
//            {
//                case SPATIALIZATION_MODE_NONE:
//                    return true;
//                case SPATIALIZATION_MODE_HIGH_PERFORMANCE:
//                    return loadedHighPerformanceILD;
//                case SPATIALIZATION_MODE_HIGH_QUALITY:
//                    return loadedHRTF;
//                default:
//                    return false;
//            }
//        }
//        
//    protected:
//        // Init parameters. Core is not ready until we load the HRTF. ILD will be disabled, so we don't need to worry yet
//        bool initialize(int sampleRate, int dspBufferSize);
//        
//        // initialize is only called by CreateCallback
//        friend UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state);
//    };
//    // Get singleton instance.
//    Spatializer& spatializer();
    
    // State of a single source in the scene. An instance of this is
    // created/destroyed with each Unity AudioSource.

    
    
	//int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition);
	//bool LoadHRTFBinaryString(const std::basic_string<uint8_t>& hrtfData, std::shared_ptr<Binaural::CListener> listener);
	//bool LoadHighPerformanceILDBinaryString(const std::basic_string<uint8_t>& ildData, std::shared_ptr<Binaural::CListener> listener);
	//bool LoadNearFieldILDBinaryString(const std::basic_string<uint8_t>& ildData, std::shared_ptr<Binaural::CListener> listener);

}


