/**
*** API for 3D-Tune-In Toolkit HA Simulation Unity Wrapper ***
*
* version beta 1.0
* Created on: January 2017
* 
* Author: 3DI-DIANA Research Group / University of Malaga / Spain
* Contact: areyes@uma.es
* 
* Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
* Module: 3DTI Toolkit Unity Wrapper
**/

using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;   // For ReadOnlyCollection

public class API_3DTI_HA : MonoBehaviour
{
    // Constant definitions 
    public const int EAR_LEFT = 1;
    public const int EAR_RIGHT = 0;
    public const int EAR_BOTH = 2;
 
    // Global variables
    public AudioMixer haMixer;  // Drag&drop here the HAHL_3DTI_Mixer

    //////////////////////////////////////////////////////////////
    // GLOBAL METHODS
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off whole HA process
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchHAOnOff(int ear, bool value)
    {
        return HASwitch(ear, "HA3DTI_Process_", value);
    }

    /// <summary>
    /// Set volume in decibels of HA for each ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="volume (dB)"></param>
    /// <returns></returns>
    public bool SetVolume(int ear, float volume)
    {
        return HASetFloat(ear, "HA3DTI_Volume_", volume);
    }


    //////////////////////////////////////////////////////////////
    // DYNAMIC EQ
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off dynamic equalizer
    /// </summary>    
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchDynamicEQOnOff(bool value)
    {
        return haMixer.SetFloat("HA3DTI_Dynamic_On", Bool2Float(value)); 
    }

    /// <summary>
    ///  Set gain (in dB) for one band of the static equalizer
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="band ([0..6])"></param>
    /// <param name="gain (dB)"></param>
    /// <returns></returns>
    public bool SetStaticEQBandGain(int ear, int band, float gain)
    {
        string paramName = "HA3DTI_Gain_Level_0_Band_" + band.ToString() + "_";
        return HASetFloat(ear, paramName, gain);
    }

    /// <summary>
    /// Set gain (in dB) for one level of one band of the dynamic equalizer
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="band ([0..6])"></param>
    /// <param name="level ([0..2])"></param>
    /// <param name="gain (dB)"></param>
    /// <returns></returns>
    public bool SetDynamicEQBandLevelGain(int ear, int band, int level, float gain)
    {
        string paramName = "HA3DTI_Gain_Level_" + level.ToString() + "_Band_" + band.ToString() + "_";
        return HASetFloat(ear, paramName, gain);
    }

    /// <summary>
    /// Set threshold (in dB) for one level of the dynamic equalizer
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="level ([0..2]"></param>
    /// <param name="threshold (dB)"></param>
    /// <returns></returns>
    public bool SetDynamicEQLevelThreshold(int ear, int level, float threshold)
    {
        string paramName = "HA3DTI_Threshold_" + level.ToString() + "_";
        return HASetFloat(ear, paramName, threshold);
    }

    /// <summary>
    /// Set cutoff frequency (in Hz) of low pass filter
    /// </summary>    
    /// <param name="cutoff (Hz)"></param>
    /// <returns></returns>
    public bool SetLPFCutoff(float cutoff)
    {
        return haMixer.SetFloat("HA3DTI_LPF_Cutoff", cutoff);
    }

    /// <summary>
    /// Set cutoff frequency (in Hz) of high pass filter
    /// </summary>    
    /// <param name="cutoff (Hz)"></param>
    /// <returns></returns>
    public bool SetHPFCutoff(float cutoff)
    {
        return haMixer.SetFloat("HA3DTI_HPF_Cutoff", cutoff);
    }

    /// <summary>
    /// Switch on/off levels interpolation in dynamic equalizer
    /// </summary>    
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchDynamicEQInterpolationOnOff(bool value)
    {
        return haMixer.SetFloat("HA3DTI_Interpolation_On", Bool2Float(value));
    }

    /// <summary>
    /// Set attack and release time (in milliseconds) for dynamic equalizer envelope detector
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="attackRelease (ms)"></param>
    /// <returns></returns>
    public bool SetDynamicEQAttackRelease(int ear, float attackRelease)
    {
        return HASetFloat(ear, "HA3DTI_AttackRelease_", attackRelease);
    }

    //////////////////////////////////////////////////////////////
    // QUANTIZATION NOISE
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Specify if quantization noise is added at the beggining (noiseBefore) or at the end (noiseAfter) of the process chain. 
    /// Quantization noise can be added either at one of these two stages, at both or never
    /// </summary>    
    /// <param name="noiseBefore"></param>
    /// <param name="noiseAfter"></param>
    /// <returns></returns>
    public bool SetQuantizationNoiseInChain(bool noiseBefore, bool noiseAfter)
    {
        if (!haMixer.SetFloat("HA3DTI_NoiseBefore_On", Bool2Float(noiseBefore)))
            return false;
        return haMixer.SetFloat("HA3DTI_NoiseAfter_On", Bool2Float(noiseAfter));
    }

    /// <summary>
    /// Set number of bits of quantization noise
    /// </summary>    
    /// <param name="nbits ([6..24])"></param>
    /// <returns></returns>
    public bool SetQuantizationNoiseBits(int nbits)
    {
        return haMixer.SetFloat("HA3DTI_NoiseBits", (float)nbits);
    }

    //////////////////////////////////////////////////////////////
    // AUXILIARY FUNCTIONS
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Generic Set method
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="paramPrefix"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool HASetFloat(int ear, string paramPrefix, float value)
    {
        // Both ears
        if (ear == EAR_BOTH)
        {
            if (!HASetFloat(EAR_LEFT, paramPrefix, value)) return false;
            return HASetFloat(EAR_RIGHT, paramPrefix, value);
        }

        // Build exposed parameter name string
        string paramName = paramPrefix;
        if (ear == EAR_LEFT)
            paramName += "Left";
        else
            paramName += "Right";

        // Set value
        return haMixer.SetFloat(paramName, value);
    }

    /// <summary>
    /// Generic Switch method
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="paramPrefix"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool HASwitch(int ear, string paramPrefix, bool value)
    {
        // Both ears
        if (ear == EAR_BOTH)
        {
            if (!HASwitch(EAR_LEFT, paramPrefix, value)) return false;
            return HASwitch(EAR_RIGHT, paramPrefix, value);
        }

        // Build exposed parameter name string
        string paramName = paramPrefix;
        if (ear == EAR_LEFT)
            paramName += "LeftOn";
        else
            paramName += "RightOn";

        // Set value
        return haMixer.SetFloat(paramName, Bool2Float(value));
    }

    /// <summary>
    ///  Auxiliary function
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    bool Float2Bool(float v)
    {
        if (v == 1.0f)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Auxiliary function
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    float Bool2Float(bool v)
    {
        if (v)
            return 1.0f;
        else
            return 0.0f;
    }
}
