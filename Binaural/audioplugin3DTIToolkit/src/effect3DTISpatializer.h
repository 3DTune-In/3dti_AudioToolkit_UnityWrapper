#pragma once
#include "AudioPluginUtil.h"
#include <BinauralSpatializer/Core.h>
#include <Common/DynamicCompressorStereo.h>


namespace Spatializer3DTI
{
    
    enum SpatializationMode : int
    {
        SPATIALIZATION_MODE_HIGH_QUALITY = 0,
        SPATIALIZATION_MODE_HIGH_PERFORMANCE = 1,
        SPATIALIZATION_MODE_NONE = 2,
    };

    
    enum
    {
        PARAM_HRTF_FILE_STRING, //0
        PARAM_HEAD_RADIUS,
        PARAM_SCALE_FACTOR,
        PARAM_SOURCE_ID,    // DEBUG
        PARAM_CUSTOM_ITD,
        PARAM_HRTF_INTERPOLATION, // 5
        PARAM_MOD_FARLPF,
        PARAM_MOD_DISTATT,
        PARAM_MOD_NEAR_FIELD_ILD,
        PARAM_MOD_HRTF,
        PARAM_MAG_ANECHATT, // 10
        PARAM_MAG_SOUNDSPEED,
        PARAM_NEAR_FIELD_ILD_FILE_STRING,
        PARAM_DEBUG_LOG,
        
        // HA directionality
        PARAM_HA_DIRECTIONALITY_EXTEND_LEFT,
        PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT, // 15
        PARAM_HA_DIRECTIONALITY_ON_LEFT,
        PARAM_HA_DIRECTIONALITY_ON_RIGHT,
        
        // Limiter
        PARAM_LIMITER_SET_ON,
        PARAM_LIMITER_GET_COMPRESSION,
        
        // INITIALIZATION CHECK
        PARAM_IS_CORE_READY, // 20
        
        // HRTF resampling step
        PARAM_HRTF_STEP,
        
        // High Performance and None modes
        PARAM_HIGH_PERFORMANCE_ILD_FILE_STRING,
        PARAM_SPATIALIZATION_MODE,
        PARAM_BUFFER_SIZE,
        PARAM_SAMPLE_RATE,
        PARAM_BUFFER_SIZE_CORE,
        PARAM_SAMPLE_RATE_CORE,
        
        
        P_NUM
    };
    
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state);
    
    // Class holding state for the whole plugin (we have a single
    // listener as permitted by Unity). This is effectively a
    // singleton class, though for simplicity we just hold an
    // instance in a free variable rather than using a static member.
    class Spatializer
    {
    public:
        std::shared_ptr<Binaural::CListener> listener;
        Binaural::CCore core;
        // replaced with function
//        bool coreReady;
        bool loadedHRTF;                // New
        bool loadedNearFieldILD;        // New
        bool loadedHighPerformanceILD;    // New
        int spatializationMode;            // New
        float parameters[P_NUM];
        
        // STRING SERIALIZER
        char* strHRTFpath;
        bool strHRTFserializing;
        int strHRTFcount;
        int strHRTFlength;
        char* strNearFieldILDpath;
        bool strNearFieldILDserializing;
        int strNearFieldILDcount;
        int strNearFieldILDlength;
        char* strHighPerformanceILDpath;
        bool strHighPerformanceILDserializing;
        int strHighPerformanceILDcount;
        int strHighPerformanceILDlength;
        
        // Limiter
        Common::CDynamicCompressorStereo limiter;
        
        // DEBUG LOG
        bool debugLog = false;
        
        // MUTEX
        std::mutex spatializerMutex;
        
        Spatializer();
        
        bool isInitialized() const
        {
            return listener != nullptr;
        }
        
        bool isReady() const
        {
            switch (spatializationMode)
            {
                case SPATIALIZATION_MODE_NONE:
                    return true;
                case SPATIALIZATION_MODE_HIGH_PERFORMANCE:
                    return loadedHighPerformanceILD;
                case SPATIALIZATION_MODE_HIGH_QUALITY:
                    return loadedHRTF;
                default:
                    return false;
            }
        }
        
    protected:
        // Init parameters. Core is not ready until we load the HRTF. ILD will be disabled, so we don't need to worry yet
        bool initialize(int sampleRate, int dspBufferSize);
        
        // initialize is only called by CreateCallback
        friend UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state);
    };
    // Get singleton instance.
    Spatializer& spatializer();
    
    // State of a single source in the scene. An instance of this is
    // created/destroyed with each Unity AudioSource.
    struct EffectData
    {
        int sourceID;    // DEBUG
        std::shared_ptr<Binaural::CSingleSourceDSP> audioSource;
    };
    
    
    int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition);
    bool LoadHRTFBinaryString(const std::basic_string<uint8_t>& hrtfData, std::shared_ptr<Binaural::CListener> listener);
    bool LoadHighPerformanceILDBinaryString(const std::basic_string<uint8_t>& ildData,  std::shared_ptr<Binaural::CListener> listener);
    bool LoadNearFieldILDBinaryString(const std::basic_string<uint8_t>& ildData, std::shared_ptr<Binaural::CListener> listener);

}


