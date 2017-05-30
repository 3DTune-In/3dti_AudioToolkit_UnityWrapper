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
using API_3DTI_Common;

public class API_3DTI_HA : MonoBehaviour
{
    // Global variables
    public AudioMixer haMixer;  // Drag&drop here the HAHL_3DTI_Mixer

    // Public type definitions
    public enum T_HAToneBand { LOW =0, MID =1, HIGH =2 };
    public enum T_HADynamicEQBand { HZ_125 =0, HZ_250 =1, HZ_500 =2, HZ_1K =3, HZ_2K =4, HZ_4K =5, HZ_8K =6};    
    public enum T_HADynamicEQLevel { LEVEL_0 =0, LEVEL_1 =1, LEVEL_2 =2 };

    // Public constant definitions
    public const int NUM_EQ_CURVES = 3;
    public const int NUM_EQ_BANDS = 7;

    //

    // Internal use constants
    const float FIG6_THRESHOLD_0_DBSPL = 40.0f; // TO DO: consistent numbering
    const float FIG6_THRESHOLD_1_DBSPL = 65.0f; // TO DO: consistent numbering
    const float FIG6_THRESHOLD_2_DBSPL = 95.0f;    
    const float DBSPL_FOR_0_DBFS = 100.0f;

    // Internal use variables
    //public float [,] tone = new float[2, 3] { { 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f } };    // Tone values for each EAR and each tone BAND

    // Internal parameters for consistency with GUI
    [HideInInspector]
    public bool PARAM_PROCESS_LEFT_ON = false;
    [HideInInspector]
    public bool PARAM_PROCESS_RIGHT_ON = false;
    [HideInInspector]
    public float PARAM_VOLUME_L_DB = 0.0f;
    [HideInInspector]
    public float PARAM_VOLUME_R_DB = 0.0f;
    // Common values for both ears in EQ		
    [HideInInspector]
    public float PARAM_EQ_LPFCUTOFF_HZ = 0.0f;
    [HideInInspector]
    public float PARAM_EQ_HPFCUTOFF_HZ = 0.0f;
    // Dynamic EQ
    [HideInInspector]
    public bool PARAM_DYNAMICEQ_INTERPOLATION_ON = true;
    [HideInInspector]
    public float [] PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS = new float[3]  { 0.0f, 0.0f, 0.0f };
    [HideInInspector]
    public float [] PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS = new float[3] { 0.0f, 0.0f, 0.0f };
    [HideInInspector]
    public float [,] PARAM_DYNAMICEQ_GAINS_LEFT = new float[3, 7]  { { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f } };
    [HideInInspector]
    public float [,] PARAM_DYNAMICEQ_GAINS_RIGHT = new float[3, 7] { { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f } };
    [HideInInspector]
    public float PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS = 0.0f;
    [HideInInspector]
    public float PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS = 0.0f;
    // Quantization noise
    [HideInInspector]
    public bool PARAM_NOISE_BEFORE_ON = false;
    [HideInInspector]
    public bool PARAM_NOISE_AFTER_ON = false;
    [HideInInspector]
    public int PARAM_NOISE_NUMBITS = 24;
    // Simplified controls
    [HideInInspector]
    public float PARAM_COMPRESSION_PERCENTAGE_LEFT = 0.0f;
    [HideInInspector]
    public float PARAM_COMPRESSION_PERCENTAGE_RIGHT = 0.0f;
    // Limiter
    [HideInInspector]
    public bool PARAM_LIMITER_ON = false;
    // Normalization
    [HideInInspector]
    public bool PARAM_NORMALIZATION_SET_ON_LEFT = false;
    [HideInInspector]
    public float PARAM_NORMALIZATION_DBS_LEFT = 20.0f;
    [HideInInspector]
    public bool PARAM_NORMALIZATION_SET_ON_RIGHT = false;
    [HideInInspector]
    public float PARAM_NORMALIZATION_DBS_RIGHT = 20.0f;
    // Tone control
    [HideInInspector]
    public float PARAM_TONE_LOW_LEFT = 0.0f;
    [HideInInspector]
    public float PARAM_TONE_MID_LEFT = 0.0f;
    [HideInInspector]
    public float PARAM_TONE_HIGH_LEFT = 0.0f;
    [HideInInspector]
    public float PARAM_TONE_LOW_RIGHT = 0.0f;
    [HideInInspector]
    public float PARAM_TONE_MID_RIGHT = 0.0f;
    [HideInInspector]
    public float PARAM_TONE_HIGH_RIGHT = 0.0f;
    // Debug log
    [HideInInspector]
    public bool PARAM_DEBUG_LOG = false;    

    //////////////////////////////////////////////////////////////
    // INITIALIZATION
    //////////////////////////////////////////////////////////////

    //private void Start()
    //{
    //    Initialize();
    //}

    ///// <summary>
    ///// Set all parameters to their default values
    ///// </summary>
    //public void Initialize()
    //{
    //    SwitchHAOnOff(T_ear.LEFT, PARAM_PROCESS_LEFT_ON);
    //    SwitchHAOnOff(T_ear.RIGHT, PARAM_PROCESS_RIGHT_ON);
    //    SetVolume(T_ear.LEFT, PARAM_VOLUME_L_DB);
    //    SetVolume(T_ear.RIGHT, PARAM_VOLUME_R_DB);
    //    SetLPFCutoff(PARAM_EQ_LPFCUTOFF_HZ);
    //    SetHPFCutoff(PARAM_EQ_HPFCUTOFF_HZ);
    //    SwitchDynamicEQInterpolationOnOff(PARAM_DYNAMICEQ_INTERPOLATION_ON);
    //    for (int level = 0; level < NUM_EQ_CURVES; level++)
    //    {
    //        SetDynamicEQLevelThreshold(T_ear.LEFT, level, PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS[level]);
    //        SetDynamicEQLevelThreshold(T_ear.RIGHT, level, PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS[level]);
    //        for (int band = 0; band < FIG6_NUMBANDS; band++)
    //        {
    //            SetDynamicEQBandLevelGain(T_ear.LEFT, band, level, PARAM_DYNAMICEQ_GAINS_LEFT[level, band]);
    //            SetDynamicEQBandLevelGain(T_ear.RIGHT, band, level, PARAM_DYNAMICEQ_GAINS_RIGHT[level, band]);
    //        }                        
    //    }
    //    SetDynamicEQAttackRelease(T_ear.LEFT, PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS);
    //    SetDynamicEQAttackRelease(T_ear.RIGHT, PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS);
    //    SetQuantizationNoiseInChain(PARAM_NOISE_BEFORE_ON, PARAM_NOISE_AFTER_ON);
    //    SetQuantizationNoiseBits(PARAM_NOISE_NUMBITS);
    //    SetCompressionPercentage(T_ear.LEFT, PARAM_COMPRESSION_PERCENTAGE_LEFT);
    //    SetCompressionPercentage(T_ear.RIGHT, PARAM_COMPRESSION_PERCENTAGE_RIGHT);
    //    SwitchLimiterOnOff(PARAM_LIMITER_ON);
    //    SwitchNormalizationOnOff(T_ear.LEFT, PARAM_NORMALIZATION_SET_ON_LEFT);
    //    SwitchNormalizationOnOff(T_ear.RIGHT, PARAM_NORMALIZATION_SET_ON_RIGHT);
    //    SetNormalizationLevel(T_ear.LEFT, PARAM_NORMALIZATION_DBS_LEFT);
    //    SetNormalizationLevel(T_ear.RIGHT, PARAM_NORMALIZATION_DBS_RIGHT);
    //    //public bool PARAM_DEBUG_LOG = false;
    //}

    //////////////////////////////////////////////////////////////
    // GET METHODS
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets the current state of the limiter (compressing or not)
    /// </summary>
    /// <param name="compressing"></param>
    /// <returns></returns>
    public bool GetLimiterCompression(out bool compressing)
    {
        compressing = false;
        float floatValue;
        if (!haMixer.GetFloat("HA3DTI_Get_Limiter_Compression", out floatValue)) return false;
        compressing = CommonFunctions.Float2Bool(floatValue);
        return true;
    }

    /// <summary>
    /// Gets the current state of normalization (applying offset or not)
    /// </summary>
    /// <param name="normalizing"></param>
    /// <returns></returns>
    public bool GetNormalizationOffset(T_ear ear, out float offset)
    {
        //normalizing = false;
		offset = 0.0f;

        // Does not make sense to read a single value from both ears
        if (ear == T_ear.BOTH)
            return false;

        //float floatValue;
        //if (ear == T_ear.LEFT)
        //{
        //    if (!haMixer.GetFloat("HA3DTI_Normalization_Get_Left", out floatValue)) return false;
        //}
        //else
        //{
        //    if (!haMixer.GetFloat("HA3DTI_Normalization_Get_Right", out floatValue)) return false;
        //}

        //normalizing = Float2Bool(floatValue);
        //return true;

        // FIX: Fill the parameters from mixer sliders
        for (int i = 0; i < NUM_EQ_BANDS; i++)
        {
            string paramStrL = "HA3DTI_Gain_Level_0_Band_" + i.ToString() + "_Left";
            if (!haMixer.GetFloat(paramStrL, out PARAM_DYNAMICEQ_GAINS_LEFT[0, i])) return false;
            string paramStrR = "HA3DTI_Gain_Level_0_Band_" + i.ToString() + "_Right";
            if (!haMixer.GetFloat(paramStrR, out PARAM_DYNAMICEQ_GAINS_RIGHT[0, i])) return false;
        }

        // Find the max gain within all bands of first curve
        if (ear == T_ear.LEFT)
		{
			float max = PARAM_DYNAMICEQ_GAINS_LEFT[0,0];
			for (int i=0; i < NUM_EQ_BANDS; i++)
			{
				if (PARAM_DYNAMICEQ_GAINS_LEFT[0,i] > max)
					max = PARAM_DYNAMICEQ_GAINS_LEFT[0,i];
			}
			offset = PARAM_NORMALIZATION_DBS_LEFT - max;
		}
		else
		{
			float max = PARAM_DYNAMICEQ_GAINS_RIGHT[0,0];
			for (int i=0; i < NUM_EQ_BANDS; i++)
			{
				if (PARAM_DYNAMICEQ_GAINS_RIGHT[0,i] > max)
					max = PARAM_DYNAMICEQ_GAINS_RIGHT[0,i];
			}
			offset = PARAM_NORMALIZATION_DBS_RIGHT - max;
		}

		// The offset is applied only if the maximum gain is above the threshold
		if (offset > 0.0f)
			offset = 0.0f;
		
		return true;
    }

    //////////////////////////////////////////////////////////////
    // GLOBAL METHODS
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off whole HA process
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchHAOnOff(T_ear ear, bool value)
    {              
        return HASwitch(ear, "HA3DTI_Process_", value, ref PARAM_PROCESS_LEFT_ON, ref PARAM_PROCESS_RIGHT_ON);
    }

    /// <summary>
    /// Set volume in decibels of HA for each ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="volume (dB)"></param>
    /// <returns></returns>
    public bool SetVolume(T_ear ear, float volume)
    {
        return HASetFloat(ear, "HA3DTI_Volume_", volume, ref PARAM_VOLUME_L_DB, ref PARAM_VOLUME_R_DB);
    }

    /// <summary>
    /// Switch on/off limiter after HA process
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchLimiterOnOff(bool value)
    {
        PARAM_LIMITER_ON = value;    
        return haMixer.SetFloat("HA3DTI_Limiter_On", CommonFunctions.Bool2Float(value));
    }

    /// <summary>
    /// Switch on/off normalization for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchNormalizationOnOff(T_ear ear, bool value)
    {
        return HASwitch(ear, "HA3DTI_Normalization_", value, ref PARAM_NORMALIZATION_SET_ON_LEFT, ref PARAM_NORMALIZATION_SET_ON_RIGHT);
    }

    /// <summary>
    /// Set normalization level in decibels
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public bool SetNormalizationLevel(T_ear ear, float level)
    {
        return HASetFloat(ear, "HA3DTI_Normalization_DB_", level, ref PARAM_NORMALIZATION_DBS_LEFT, ref PARAM_NORMALIZATION_DBS_RIGHT);
    }

    public bool SetWriteDebugLog(bool value)
    {
        PARAM_DEBUG_LOG = value;
        return haMixer.SetFloat("HA3DTI_DebugLog", CommonFunctions.Bool2Float(value)); 
    }

    //////////////////////////////////////////////////////////////
    // SIMPLIFIED HIGH LEVEL CONTROLS
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Set volume in decibels of one tone band 
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="toneband"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetTone(T_ear ear, T_HAToneBand toneband, float value)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetTone(T_ear.LEFT, toneband, value)) return false;
            return SetTone(T_ear.RIGHT, toneband, value);
        }

        string paramName = "HA3DTI_Tone_";
        //string paramSufix;
        //if (ear == T_ear.LEFT)
        //    paramSufix = "Left";
        //else
        //    paramSufix = "Right";

        switch (toneband) 
        {
            case T_HAToneBand.LOW:
                paramName += "Low_";// + paramSufix;
                if (!HASetFloat(ear, paramName, value, ref PARAM_TONE_LOW_LEFT, ref PARAM_TONE_LOW_RIGHT)) return false;
                //if (!AddToHABand(ear, T_HADynamicEQBand.HZ_125, T_HAToneBand.LOW, value)) return false;
                //if (!AddToHABand(ear, T_HADynamicEQBand.HZ_250, T_HAToneBand.LOW, value)) return false;
                //if (!AddToHABand(ear, T_HADynamicEQBand.HZ_500, T_HAToneBand.LOW, value)) return false;
                break;                                           
            case T_HAToneBand.MID:
                paramName += "Mid_";// + paramSufix;
                if (!HASetFloat(ear, paramName, value, ref PARAM_TONE_MID_LEFT, ref PARAM_TONE_MID_RIGHT)) return false;
                //if (!AddToHABand(ear, T_HADynamicEQBand.HZ_1K, T_HAToneBand.MID, value)) return false;
                //if (!AddToHABand(ear, T_HADynamicEQBand.HZ_2K, T_HAToneBand.MID, value)) return false;
                break;
            case T_HAToneBand.HIGH:
                paramName += "High_";// + paramSufix;
                if (!HASetFloat(ear, paramName, value, ref PARAM_TONE_HIGH_LEFT, ref PARAM_TONE_HIGH_RIGHT)) return false;
                //if (!AddToHABand(ear, T_HADynamicEQBand.HZ_4K, T_HAToneBand.HIGH, value)) return false;
                //if (!AddToHABand(ear, T_HADynamicEQBand.HZ_8K, T_HAToneBand.HIGH, value)) return false;
                break;
            default:
                return false;                
        }

        //tone[(int)ear, (int)toneband] = value;

        return true;
    }

    /// <summary>
    /// Set compression percentage for the dynamic equalizer
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetCompressionPercentage(T_ear ear, float value)
    {
        return HASetFloat(ear, "HA3DTI_Compression_", value, ref PARAM_COMPRESSION_PERCENTAGE_LEFT, ref PARAM_COMPRESSION_PERCENTAGE_RIGHT);
    }

    //////////////////////////////////////////////////////////////
    // DYNAMIC EQ
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Set gain (in dB) for one level of one band of the dynamic equalizer
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="eqband"></param>
    /// <param name="eqlevel"></param>
    /// <param name="gain (dB)"></param>
    /// <returns></returns>
    public bool SetDynamicEQBandLevelGain(T_ear ear, T_HADynamicEQBand eqband, T_HADynamicEQLevel eqlevel, float gain)
    {
        string paramName = "HA3DTI_Gain_Level_" + ((int)eqlevel).ToString() + "_Band_" + ((int)eqband).ToString() + "_";
        return HASetFloat(ear, paramName, gain, ref PARAM_DYNAMICEQ_GAINS_LEFT[(int)eqlevel, (int)eqband], ref PARAM_DYNAMICEQ_GAINS_RIGHT[(int)eqlevel, (int)eqband]);
    }

    /// <summary>
    /// Set threshold (in dB) for one level of the dynamic equalizer
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="eqlevel"></param>
    /// <param name="threshold (dB)"></param>
    /// <returns></returns>
    public bool SetDynamicEQLevelThreshold(T_ear ear, T_HADynamicEQLevel eqlevel, float threshold)
    {
        string paramName = "HA3DTI_Threshold_" + ((int)eqlevel).ToString() + "_";
        return HASetFloat(ear, paramName, threshold, ref PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS[(int)eqlevel], ref PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS[(int)eqlevel]);
    }

    /// <summary>
    /// Set cutoff frequency (in Hz) of low pass filter
    /// </summary>    
    /// <param name="cutoff (Hz)"></param>
    /// <returns></returns>
    public bool SetLPFCutoff(float cutoff)
    {
        PARAM_EQ_LPFCUTOFF_HZ = cutoff;
        return haMixer.SetFloat("HA3DTI_LPF_Cutoff", cutoff);
    }

    /// <summary>
    /// Set cutoff frequency (in Hz) of high pass filter
    /// </summary>    
    /// <param name="cutoff (Hz)"></param>
    /// <returns></returns>
    public bool SetHPFCutoff(float cutoff)
    {
        PARAM_EQ_HPFCUTOFF_HZ = cutoff;
        return haMixer.SetFloat("HA3DTI_HPF_Cutoff", cutoff);
    }

    /// <summary>
    /// Switch on/off levels interpolation in dynamic equalizer
    /// </summary>    
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchDynamicEQInterpolationOnOff(bool value)
    {
        PARAM_DYNAMICEQ_INTERPOLATION_ON = value;
        return haMixer.SetFloat("HA3DTI_Interpolation_On", CommonFunctions.Bool2Float(value));
    }

    /// <summary>
    /// Set attack and release time (in milliseconds) for dynamic equalizer envelope detector
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="attackRelease (ms)"></param>
    /// <returns></returns>
    public bool SetDynamicEQAttackRelease(T_ear ear, float attackRelease)
    {
        return HASetFloat(ear, "HA3DTI_AttackRelease_", attackRelease, ref PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS, ref PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS);
    }

    /// <summary>
    /// Configure dynamic equalizer using Fig6 method
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="earLossList (dB[])"></param>
    /// <returns></returns>
    public bool SetEQFromFig6(T_ear ear, List<float> earLossInput, out List<float> calculatedGains)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetEQFromFig6(T_ear.LEFT, earLossInput, out calculatedGains))
                return false;
            return SetEQFromFig6(T_ear.RIGHT, earLossInput, out calculatedGains);
        }

        // Init gains
        calculatedGains = new List<float>(); 
        for (int band = 0; band < NUM_EQ_BANDS; band++)
        {
            for (int level = 0; level < NUM_EQ_CURVES; level++)
            {
                calculatedGains.Add(0.0f);                
            }
        }

        // Set level thresholds        
        SetDynamicEQLevelThreshold(ear, T_HADynamicEQLevel.LEVEL_1, FIG6_THRESHOLD_0_DBSPL - DBSPL_FOR_0_DBFS);  // TO DO: consistent numbering
        SetDynamicEQLevelThreshold(ear, T_HADynamicEQLevel.LEVEL_0, FIG6_THRESHOLD_1_DBSPL - DBSPL_FOR_0_DBFS);  // TO DO: consistent numbering
        SetDynamicEQLevelThreshold(ear, T_HADynamicEQLevel.LEVEL_2, FIG6_THRESHOLD_2_DBSPL - DBSPL_FOR_0_DBFS);

        // Set band gains        
        foreach (T_HADynamicEQBand bandIndex in T_HADynamicEQBand.GetValues(typeof(T_HADynamicEQBand)))
        {
            float gain0, gain1, gain2;
            if (!SetEQBandFromFig6(ear, bandIndex, earLossInput[(int)bandIndex], out gain0, out gain1, out gain2))
                return false;

            calculatedGains[(int)bandIndex * NUM_EQ_CURVES] = gain0;
            calculatedGains[(int)bandIndex * NUM_EQ_CURVES + 1] = gain1;
            calculatedGains[(int)bandIndex * NUM_EQ_CURVES + 2] = gain2;
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
        PARAM_NOISE_BEFORE_ON = noiseBefore;
        PARAM_NOISE_AFTER_ON = noiseAfter;

        if (!haMixer.SetFloat("HA3DTI_NoiseBefore_On", CommonFunctions.Bool2Float(noiseBefore)))
            return false;
        return haMixer.SetFloat("HA3DTI_NoiseAfter_On", CommonFunctions.Bool2Float(noiseAfter));
    }

    /// <summary>
    /// Set number of bits of quantization noise
    /// </summary>    
    /// <param name="nbits ([6..24])"></param>
    /// <returns></returns>
    public bool SetQuantizationNoiseBits(int nbits)
    {
        PARAM_NOISE_NUMBITS = nbits;
        return haMixer.SetFloat("HA3DTI_NoiseBits", (float)nbits);
    }

    //////////////////////////////////////////////////////////////
    // AUXILIARY FUNCTIONS
    //////////////////////////////////////////////////////////////

    public bool SetEQBandFromFig6(T_ear ear, T_HADynamicEQBand bandIndex, float earLoss, out float gain0, out float gain1, out float gain2)
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
        if (!SetDynamicEQBandLevelGain(ear, bandIndex, T_HADynamicEQLevel.LEVEL_1, gain0)) return false; // TO DO: coherent numbering. Curve 0 is now the reference for compression percentage
        if (!SetDynamicEQBandLevelGain(ear, bandIndex, T_HADynamicEQLevel.LEVEL_0, gain1)) return false; // TO DO: coherent numbering. Curve 0 is now the reference for compression percentage
        if (!SetDynamicEQBandLevelGain(ear, bandIndex, T_HADynamicEQLevel.LEVEL_2, gain2)) return false;

        return true;
    }

    /// <summary>
    /// Method for setting value of an exposed parameter
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="paramPrefix"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool HASetFloat(T_ear ear, string paramPrefix, float value, ref float paramLeft, ref float paramRight)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!HASetFloat(T_ear.LEFT, paramPrefix, value, ref paramLeft, ref paramRight)) return false;
            return HASetFloat(T_ear.RIGHT, paramPrefix, value, ref paramLeft, ref paramRight);
        }

        // Build exposed parameter name string and set internal API parameters
        string paramName = paramPrefix;
        if (ear == T_ear.LEFT)
        {
            paramName += "Left";
            paramLeft = value;
        }
        else
        {
            paramName += "Right";
            paramRight = value;
        }

        // Set value
        return haMixer.SetFloat(paramName, value);
    }

    ///// <summary>
    ///// Method for adding a (positive/negative) increment to a band in a curve of the dynamic eq
    ///// The actual increment applied is newIncrement - oldIncrement.
    ///// </summary>
    ///// <param name="ear"></param>    
    ///// <param name="eqband"></param>
    ///// <param name="oldIncrement"></param>
    ///// <param name="newIncrement"></param>
    ///// <returns></returns>
    //public bool AddToHABand(T_ear ear, T_HADynamicEQBand eqband, T_HAToneBand toneBand, float newIncrement)
    //{        
    //    // Both ears
    //    if (ear == T_ear.BOTH)
    //    {
    //        if (!AddToHABand(T_ear.LEFT, eqband, toneBand, newIncrement)) return false;
    //        return AddToHABand(T_ear.RIGHT, eqband, toneBand, newIncrement);
    //    }

    //    // Go through all dynamic eq levels        
    //    foreach (T_HADynamicEQLevel eqlevel in T_HADynamicEQLevel.GetValues(typeof(T_HADynamicEQLevel)))
    //    {
    //        // Build exposed parameter name string 
    //        string paramName = "HA3DTI_Gain_Level_" + ((int)eqlevel).ToString() + "_Band_" + ((int)eqband).ToString() + "_";
    //        if (ear == T_ear.LEFT)
    //        {
    //            paramName += "Left";
    //        }
    //        else
    //        {
    //            paramName += "Right";
    //        }

    //        // Get current value, considering oldIncrement
    //        float currentValue;
    //        float oldIncrement = tone[(int)ear, (int)toneBand];
    //        if (!haMixer.GetFloat(paramName, out currentValue)) return false;
    //        currentValue = currentValue - oldIncrement;

    //        // Set new value, with newIncrement    
    //        //if (!haMixer.SetFloat(paramName, currentValue + newIncrement))
    //        //    return false;

    //        //// Set internal API parameters
    //        //if (ear == T_ear.LEFT)
    //        //{
    //        //    PARAM_DYNAMICEQ_GAINS_LEFT[level, band] = currentValue + newIncrement;
    //        //}
    //        //else
    //        //{
    //        //    PARAM_DYNAMICEQ_GAINS_RIGHT[level, band] = currentValue + newIncrement;
    //        //}
    //        if (!SetDynamicEQBandLevelGain(ear, eqband, eqlevel, currentValue + newIncrement))
    //            return false;
    //    }
    //    return true;
    //}

    /// <summary>
    /// Generic Switch method
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="paramPrefix"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool HASwitch(T_ear ear, string paramPrefix, bool value, ref bool paramLeft, ref bool paramRight)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!HASwitch(T_ear.LEFT, paramPrefix, value, ref paramLeft, ref paramRight)) return false;
            return HASwitch(T_ear.RIGHT, paramPrefix, value, ref paramLeft, ref paramRight);
        }
                
        // Build exposed parameter name string and set internal API parameters
        string paramName = paramPrefix;
        if (ear == T_ear.LEFT)
        {
            paramName += "LeftOn";
            paramLeft = value;
        }
        else
        {
            paramName += "RightOn";
            paramRight = value;
        }

        // Set value
        return haMixer.SetFloat(paramName, CommonFunctions.Bool2Float(value));
    }
}
