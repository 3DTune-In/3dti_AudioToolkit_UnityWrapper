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
    // Global variables
    public AudioMixer haMixer;  // Drag&drop here the HAHL_3DTI_Mixer

    // Type definitions
    public enum T_toneBand { LOW=0, MID=1, HIGH=2 };

    // Public constant definitions
    public const int NUM_EQ_CURVES = 3;
    public const int FIG6_NUMBANDS = 7;

    // Internal use constants
    const float FIG6_THRESHOLD_0_DBSPL = 40.0f;
    const float FIG6_THRESHOLD_1_DBSPL = 65.0f;
    const float FIG6_THRESHOLD_2_DBSPL = 95.0f;    
    const float DBSPL_FOR_0_DBFS = 100.0f;

    // Internal use variables
    float [,] tone = new float[2, 3] { { 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f } };    // Tone values for each EAR and each tone BAND

    //////////////////////////////////////////////////////////////
    // GLOBAL METHODS
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off whole HA process
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchHAOnOff(API_3DTI_Common.T_ear ear, bool value)
    {
        return HASwitch(ear, "HA3DTI_Process_", value);
    }

    /// <summary>
    /// Set volume in decibels of HA for each ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="volume (dB)"></param>
    /// <returns></returns>
    public bool SetVolume(API_3DTI_Common.T_ear ear, float volume)
    {
        return HASetFloat(ear, "HA3DTI_Volume_", volume);
    }

    //////////////////////////////////////////////////////////////
    // SIMPLIFIED HIGH LEVEL CONTROLS
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Set volume in decibels of one tone band 
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="band"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetTone(API_3DTI_Common.T_ear ear, T_toneBand band, float value)
    {
        if (ear == API_3DTI_Common.T_ear.BOTH)
        {
            if (!SetTone(API_3DTI_Common.T_ear.LEFT, band, value)) return false;
            return SetTone(API_3DTI_Common.T_ear.RIGHT, band, value);
        }

        switch (band) 
        {
            case T_toneBand.LOW:                                
                AddToHAFloat(ear, "HA3DTI_Gain_Level_0_Band_0_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_1_Band_0_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_2_Band_0_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_0_Band_1_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_1_Band_1_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_2_Band_1_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_0_Band_2_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_1_Band_2_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_2_Band_2_", tone[(int)ear, (int)band], value);
                break;                                           
            case T_toneBand.MID:                                 
                AddToHAFloat(ear, "HA3DTI_Gain_Level_0_Band_3_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_1_Band_3_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_2_Band_3_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_0_Band_4_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_1_Band_4_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_2_Band_4_", tone[(int)ear, (int)band], value);
                break;
            case T_toneBand.HIGH:                
                AddToHAFloat(ear, "HA3DTI_Gain_Level_0_Band_5_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_1_Band_5_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_2_Band_5_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_0_Band_6_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_1_Band_6_", tone[(int)ear, (int)band], value);
                AddToHAFloat(ear, "HA3DTI_Gain_Level_2_Band_6_", tone[(int)ear, (int)band], value);
                break;
            default:
                return false;                
        }

        tone[(int)ear, (int)band] = value;

        return true;
    }

    public bool SetCompressionPercentage(API_3DTI_Common.T_ear ear, float value)
    {
        if (ear == API_3DTI_Common.T_ear.BOTH)
        {
            if (!SetCompressionPercentage(API_3DTI_Common.T_ear.LEFT, value)) return false;
            return SetCompressionPercentage(API_3DTI_Common.T_ear.RIGHT, value);
        }

        return HASetFloat(ear, "HA3DTI_Compression_", value);
    }

    //////////////////////////////////////////////////////////////
    // DYNAMIC EQ
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off dynamic equalizer
    /// </summary>    
    /// <param name="value"></param>
    /// <returns></returns>
    //public bool SwitchDynamicEQOnOff(bool value)
    //{
    //    return haMixer.SetFloat("HA3DTI_Dynamic_On", Bool2Float(value)); 
    //}

    /// <summary>
    ///  Set gain (in dB) for one band of the standard equalizer
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="band ([0..6])"></param>
    /// <param name="gain (dB)"></param>
    /// <returns></returns>
    public bool SetStandardEQBandGain(API_3DTI_Common.T_ear ear, int band, float gain)
    {
        string paramName = "HA3DTI_Gain_Level_0_Band_0_Left";
        return HASetFloat(ear, paramName, gain);
    }

    /// <summary>
    /// Set gain (in dB) for one level of one band of the dynamic equalizer
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="band ([0..6])"></param>
    /// <param name="level ([0..2])"></param>
    /// <param name="gain (dB)"></param>
    /// <returns></returns>
    public bool SetDynamicEQBandLevelGain(API_3DTI_Common.T_ear ear, int band, int level, float gain)
    {
        string paramName = "HA3DTI_Gain_Level_" + level.ToString() + "_Band_" + band.ToString() + "_";
        return HASetFloat(ear, paramName, gain);
    }

    /// <summary>
    /// Set threshold (in dB) for one level of the dynamic equalizer
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="level ([0..2]"></param>
    /// <param name="threshold (dB)"></param>
    /// <returns></returns>
    public bool SetDynamicEQLevelThreshold(API_3DTI_Common.T_ear ear, int level, float threshold)
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
    /// <param name="ear"></param>
    /// <param name="attackRelease (ms)"></param>
    /// <returns></returns>
    public bool SetDynamicEQAttackRelease(API_3DTI_Common.T_ear ear, float attackRelease)
    {
        return HASetFloat(ear, "HA3DTI_AttackRelease_", attackRelease);
    }

    /// <summary>
    /// Configure dynamic equalizer using Fig6 method
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="earLossList (dB[])"></param>
    /// <returns></returns>
    public bool SetEQFromFig6(API_3DTI_Common.T_ear ear, List<float>earLossList, out List<float>gains)
    {
        // Both ears
        if (ear == API_3DTI_Common.T_ear.BOTH)
        {
            if (!SetEQFromFig6(API_3DTI_Common.T_ear.LEFT, earLossList, out gains))
                return false;
            return SetEQFromFig6(API_3DTI_Common.T_ear.RIGHT, earLossList, out gains);
        }

        // Init gains
        gains = new List<float>();        
        for (int band = 0; band < FIG6_NUMBANDS; band++)
        {
            for (int level = 0; level < NUM_EQ_CURVES; level++)
            {
                gains.Add(0.0f);                
            }
        }

        // Left ear
        if (ear == API_3DTI_Common.T_ear.LEFT)
        {            
            haMixer.SetFloat("HA3DTI_Threshold_0_Left", FIG6_THRESHOLD_0_DBSPL - DBSPL_FOR_0_DBFS);  // Set level threshold 0
            haMixer.SetFloat("HA3DTI_Threshold_1_Left", FIG6_THRESHOLD_1_DBSPL - DBSPL_FOR_0_DBFS);  // Set level threshold 1
            haMixer.SetFloat("HA3DTI_Threshold_2_Left", FIG6_THRESHOLD_2_DBSPL - DBSPL_FOR_0_DBFS);  // Set level threshold 2
        }

        // Left ear
        if (ear == API_3DTI_Common.T_ear.RIGHT)
        {         
            haMixer.SetFloat("HA3DTI_Threshold_0_Right", FIG6_THRESHOLD_0_DBSPL - DBSPL_FOR_0_DBFS);  // Set level threshold 0
            haMixer.SetFloat("HA3DTI_Threshold_1_Right", FIG6_THRESHOLD_1_DBSPL - DBSPL_FOR_0_DBFS);  // Set level threshold 1
            haMixer.SetFloat("HA3DTI_Threshold_2_Right", FIG6_THRESHOLD_2_DBSPL - DBSPL_FOR_0_DBFS);  // Set level threshold 2
        }

        for (int bandIndex = 0; bandIndex < FIG6_NUMBANDS; bandIndex++)
        {
            float gain0, gain1, gain2;
            if (!SetEQBandFromFig6(ear, bandIndex, earLossList[bandIndex], out gain0, out gain1, out gain2))
                return false;

            gains[bandIndex * NUM_EQ_CURVES] = gain0;
            gains[bandIndex * NUM_EQ_CURVES + 1] = gain1;
            gains[bandIndex * NUM_EQ_CURVES + 2] = gain2;
        }

        return true;
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

    public bool SetEQBandFromFig6(API_3DTI_Common.T_ear ear, int bandIndex, float earLoss, out float gain0, out float gain1, out float gain2)
    {
        // Level 0 (40 dB)        
        if (earLoss < 20.0f)
            gain0 = 0.0f;
        else
        {
            if (earLoss <= 60.0f)
                gain0 = earLoss - 20.0f;
            else
                gain0 = earLoss * 0.5f + 10.0f;
        }

        // Level 1 (65 dB)        
        if (earLoss < 20.0f)
            gain1 = 0.0f;
        else
        {
            if (earLoss <= 60.0f)
                gain1 = 0.6f * (earLoss - 20.0f);
            else
                gain1 = earLoss * 0.8f - 23.0f;
        }

        // Level 2 (95 dB)        
        if (earLoss <= 40.0f)
            gain2 = 0.0f;
        else
            gain2 = 0.1f * Mathf.Pow(earLoss - 40.0f, 1.4f);

        // Set bands
        if (!SetDynamicEQBandLevelGain(ear, bandIndex, 0, gain0)) return false;
        if (!SetDynamicEQBandLevelGain(ear, bandIndex, 1, gain1)) return false;
        if (!SetDynamicEQBandLevelGain(ear, bandIndex, 2, gain2)) return false;

        return true;
    }

    /// <summary>
    /// Method for setting value of an exposed parameter
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="paramPrefix"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool HASetFloat(API_3DTI_Common.T_ear ear, string paramPrefix, float value)
    {
        // Both ears
        if (ear == API_3DTI_Common.T_ear.BOTH)
        {
            if (!HASetFloat(API_3DTI_Common.T_ear.LEFT, paramPrefix, value)) return false;
            return HASetFloat(API_3DTI_Common.T_ear.RIGHT, paramPrefix, value);
        }

        // Build exposed parameter name string
        string paramName = paramPrefix;
        if (ear == API_3DTI_Common.T_ear.LEFT)
            paramName += "Left";
        else
            paramName += "Right";

        // Set value
        return haMixer.SetFloat(paramName, value);
    }
       
    /// <summary>
    /// Method for adding a (positive/negative) increment to an exposed parameter. 
    /// The actual increment applied is newIncrement - oldIncrement.
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="paramPrefix"></param>
    /// <param name="oldIncrement"></param>
    /// <param name="newIncrement"></param>
    /// <returns></returns>
    public bool AddToHAFloat(API_3DTI_Common.T_ear ear, string paramPrefix, float oldIncrement, float newIncrement)
    {
        // Both ears
        if (ear == API_3DTI_Common.T_ear.BOTH)
        {
            if (!AddToHAFloat(API_3DTI_Common.T_ear.LEFT, paramPrefix, oldIncrement, newIncrement)) return false;
            return AddToHAFloat(API_3DTI_Common.T_ear.RIGHT, paramPrefix, oldIncrement, newIncrement);
        }

        // Build exposed parameter name string
        string paramName = paramPrefix;
        if (ear == API_3DTI_Common.T_ear.LEFT)
            paramName += "Left";
        else
            paramName += "Right";

        // Get current value, considering oldIncrement
        float currentValue;
        if (!haMixer.GetFloat(paramName, out currentValue)) return false;
        currentValue = currentValue - oldIncrement;

        // Set new value, with newIncrement
        return haMixer.SetFloat(paramName, currentValue + newIncrement);
    }

    /// <summary>
    /// Generic Switch method
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="paramPrefix"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool HASwitch(API_3DTI_Common.T_ear ear, string paramPrefix, bool value)
    {
        // Both ears
        if (ear == API_3DTI_Common.T_ear.BOTH)
        {
            if (!HASwitch(API_3DTI_Common.T_ear.LEFT, paramPrefix, value)) return false;
            return HASwitch(API_3DTI_Common.T_ear.RIGHT, paramPrefix, value);
        }

        // Build exposed parameter name string
        string paramName = paramPrefix;
        if (ear == API_3DTI_Common.T_ear.LEFT)
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
