//
//  audioplugin3DTIWrapper.cpp
//  AudioPlugin3DTIWrapper
//
//  Created by Diana UMA on 4/11/16.
//  Copyright © 2016 diana. All rights reserved.
//

/**
 *** 3D-Tune-In Toolkit Unity Wrapper ***
 *
 * version beta 1.2
 * Created on: October 2016
 *
 * Author: 3DI-DIANA Research Group / University of Malaga / Spain
 * Contact: areyes@uma.es
 *
 * Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
 * Module: 3DTI Toolkit Unity Wrapper
 **/

#include "AudioPluginUtil.h"

/////////////////////////////////////////////////////////////////////

namespace UnityWrapper3DTI
{
    enum
    {
        PARAM_DUMMY,
        P_NUM
    };
    
    /////////////////////////////////////////////////////////////////////
    
    struct EffectData
    {
          float parameters[P_NUM];
    };
    
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
        int numparams = P_NUM;
        definition.paramdefs = new UnityAudioParameterDefinition[numparams];
        RegisterParameter(definition, "Dummy", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, PARAM_DUMMY, "Dummy parameter");
        definition.flags |= UnityAudioEffectDefinitionFlags_IsSpatializer;
        return numparams;
    }
    
    /////////////////////////////////////////////////////////////////////
    
    static UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK DistanceAttenuationCallback(UnityAudioEffectState* state, float distanceIn, float attenuationIn, float* attenuationOut)
    {
        *attenuationOut = attenuationIn;
        return UNITY_AUDIODSP_OK;
    }
    
    /////////////////////////////////////////////////////////////////////
    
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
    {
        EffectData* effectdata = new EffectData;
        memset(effectdata, 0, sizeof(EffectData));
        state->effectdata = effectdata;
        if (IsHostCompatible(state))
            state->spatializerdata->distanceattenuationcallback = DistanceAttenuationCallback;
        InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->parameters);
        
   
        return UNITY_AUDIODSP_OK;
    }
    
    /////////////////////////////////////////////////////////////////////
    
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
    {
        EffectData* data = state->GetEffectData<EffectData>();
        delete data;
        return UNITY_AUDIODSP_OK;
    }
    
    
    /////////////////////////////////////////////////////////////////////
    
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
    {
        EffectData* data = state->GetEffectData<EffectData>();
        if (index >= P_NUM)
            return UNITY_AUDIODSP_ERR_UNSUPPORTED;
        data->parameters[index] = value;
        return UNITY_AUDIODSP_OK;
    }
    
    /////////////////////////////////////////////////////////////////////
    
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char *valuestr)
    {
        EffectData* data = state->GetEffectData<EffectData>();
        if (index >= P_NUM)
            return UNITY_AUDIODSP_ERR_UNSUPPORTED;
        if (value != NULL)
            *value = data->parameters[index];
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
        // Check that I/O formats are right and that the host API supports this feature
        if (inchannels != 2 || outchannels != 2 ||
            !IsHostCompatible(state) || state->spatializerdata == NULL)
        {
            memcpy(outbuffer, inbuffer, length * outchannels * sizeof(float));
            return UNITY_AUDIODSP_OK;
        }
        
        EffectData* data = state->GetEffectData<EffectData>();
        
        for (int i = 0; i < length; i++)
        {
            outbuffer[i] = inbuffer[i]; // We take only the left channel
        }
        
        
        return UNITY_AUDIODSP_OK;
    }
}
