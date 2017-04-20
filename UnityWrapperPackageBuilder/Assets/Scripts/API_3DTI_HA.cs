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

    // Internal parameters for consistency with GUI
    public bool PARAM_PROCESS_LEFT_ON = false;
    public bool PARAM_PROCESS_RIGHT_ON = false;
    public float PARAM_VOLUME_L_DB = 0.0f;
    public float PARAM_VOLUME_R_DB = 0.0f;
    // Common values for both ears in EQ		
    public float PARAM_EQ_LPFCUTOFF_HZ = 0.0f;
    public float PARAM_EQ_HPFCUTOFF_HZ = 0.0f;
    // Dynamic EQ
    public bool PARAM_DYNAMICEQ_INTERPOLATION_ON = true;
    public float [] PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS = new float[3]  { 0.0f, 0.0f, 0.0f };
    public float [] PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS = new float[3] { 0.0f, 0.0f, 0.0f };
    public float [,] PARAM_DYNAMICEQ_GAINS_LEFT = new float[3, 7]  { { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f } };
    public float [,] PARAM_DYNAMICEQ_GAINS_RIGHT = new float[3, 7] { { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f } };
    public float PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS = 0.0f;
    public float PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS = 0.0f;
    // Quantization noise
    public bool PARAM_NOISE_BEFORE_ON = false;
    public bool PARAM_NOISE_AFTER_ON = false;
    public int PARAM_NOISE_NUMBITS = 24;
    // Simplified controls
    public float PARAM_COMPRESSION_PERCENTAGE_LEFT = 0.0f;
    public float PARAM_COMPRESSION_PERCENTAGE_RIGHT = 0.0f;
    // Limiter
    public bool PARAM_LIMITER_ON = false;
    // Normalization
    public bool PARAM_NORMALIZATION_SET_ON_LEFT = false;
    public float PARAM_NORMALIZATION_DBS_LEFT = 20.0f;
    public bool PARAM_NORMALIZATION_SET_ON_RIGHT = false;
    public float PARAM_NORMALIZATION_DBS_RIGHT = 20.0f;
    // Debug log
    public bool PARAM_DEBUG_LOG = false;    

    //////////////////////////////////////////////////////////////
    // INITIALIZATION
    //////////////////////////////////////////////////////////////

    //private void Start()
    //{
    //    Initialize();
    //}

    /// <summary>
    /// Set all parameters to their default values
    /// </summary>
    public void Initialize()
    {
        SwitchHAOnOff(API_3DTI_Common.T_ear.LEFT, PARAM_PROCESS_LEFT_ON);
        SwitchHAOnOff(API_3DTI_Common.T_ear.RIGHT, PARAM_PROCESS_RIGHT_ON);
        SetVolume(API_3DTI_Common.T_ear.LEFT, PARAM_VOLUME_L_DB);
        SetVolume(API_3DTI_Common.T_ear.RIGHT, PARAM_VOLUME_R_DB);
        SetLPFCutoff(PARAM_EQ_LPFCUTOFF_HZ);
        SetHPFCutoff(PARAM_EQ_HPFCUTOFF_HZ);
        SwitchDynamicEQInterpolationOnOff(PARAM_DYNAMICEQ_INTERPOLATION_ON);
        for (int level = 0; level < NUM_EQ_CURVES; level++)
        {
            SetDynamicEQLevelThreshold(API_3DTI_Common.T_ear.LEFT, level, PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS[level]);
            SetDynamicEQLevelThreshold(API_3DTI_Common.T_ear.RIGHT, level, PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS[level]);
            for (int band = 0; band < FIG6_NUMBANDS; band++)
            {
                SetDynamicEQBandLevelGain(API_3DTI_Common.T_ear.LEFT, band, level, PARAM_DYNAMICEQ_GAINS_LEFT[level, band]);
                SetDynamicEQBandLevelGain(API_3DTI_Common.T_ear.RIGHT, band, level, PARAM_DYNAMICEQ_GAINS_RIGHT[level, band]);
            }                        
        }
        SetDynamicEQAttackRelease(API_3DTI_Common.T_ear.LEFT, PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS);
        SetDynamicEQAttackRelease(API_3DTI_Common.T_ear.RIGHT, PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS);
        SetQuantizationNoiseInChain(PARAM_NOISE_BEFORE_ON, PARAM_NOISE_AFTER_ON);
        SetQuantizationNoiseBits(PARAM_NOISE_NUMBITS);
        SetCompressionPercentage(API_3DTI_Common.T_ear.LEFT, PARAM_COMPRESSION_PERCENTAGE_LEFT);
        SetCompressionPercentage(API_3DTI_Common.T_ear.RIGHT, PARAM_COMPRESSION_PERCENTAGE_RIGHT);
        SwitchLimiterOnOff(PARAM_LIMITER_ON);
        SwitchNormalizationOnOff(API_3DTI_Common.T_ear.LEFT, PARAM_NORMALIZATION_SET_ON_LEFT);
        SwitchNormalizationOnOff(API_3DTI_Common.T_ear.RIGHT, PARAM_NORMALIZATION_SET_ON_RIGHT);
        SetNormalizationLevel(API_3DTI_Common.T_ear.LEFT, PARAM_NORMALIZATION_DBS_LEFT);
        SetNormalizationLevel(API_3DTI_Common.T_ear.RIGHT, PARAM_NORMALIZATION_DBS_RIGHT);
        //public bool PARAM_DEBUG_LOG = false;
    }

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
        compressing = Float2Bool(floatValue);
        return true;
    }

    /// <summary>
    /// Gets the current state of normalization (applying offset or not)
    /// </summary>
    /// <param name="normalizing"></param>
    /// <returns></returns>
    public bool GetNormalizationOffset(API_3DTI_Common.T_ear ear, out float offset)
    {
        //normalizing = false;
		offset = 0.0f;

        // Does not make sense to read a single value from both ears
        if (ear == API_3DTI_Common.T_ear.BOTH)
            return false;
        
        //float floatValue;
        //if (ear == API_3DTI_Common.T_ear.LEFT)
        //{
        //    if (!haMixer.GetFloat("HA3DTI_Normalization_Get_Left", out floatValue)) return false;
        //}
        //else
        //{
        //    if (!haMixer.GetFloat("HA3DTI_Normalization_Get_Right", out floatValue)) return false;
        //}

        //normalizing = Float2Bool(floatValue);
        //return true;

		// Find the max gain within all bands of first curve
		if (ear == API_3DTI_Common.T_ear.LEFT)
		{
			float max = PARAM_DYNAMICEQ_GAINS_LEFT[0,0];
			for (int i=0; i < FIG6_NUMBANDS; i++)
			{
				if (PARAM_DYNAMICEQ_GAINS_LEFT[0,i] > max)
					max = PARAM_DYNAMICEQ_GAINS_LEFT[0,i];
			}
			offset = PARAM_NORMALIZATION_DBS_LEFT - max;
		}
		else
		{
			float max = PARAM_DYNAMICEQ_GAINS_RIGHT[0,0];
			for (int i=0; i < FIG6_NUMBANDS; i++)
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
    public bool SwitchHAOnOff(API_3DTI_Common.T_ear ear, bool value)
    {        
        return HASwitch(ear, "HA3DTI_Process_", value, ref PARAM_PROCESS_LEFT_ON, ref PARAM_PROCESS_RIGHT_ON);
    }

    /// <summary>
    /// Set volume in decibels of HA for each ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="volume (dB)"></param>
    /// <returns></returns>
    public bool SetVolume(API_3DTI_Common.T_ear ear, float volume)
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
        return haMixer.SetFloat("HA3DTI_Limiter_On", Bool2Float(value));
    }

    /// <summary>
    /// Switch on/off normalization for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchNormalizationOnOff(API_3DTI_Common.T_ear ear, bool value)
    {
        return HASwitch(ear, "HA3DTI_Normalization_On_", value, ref PARAM_NORMALIZATION_SET_ON_LEFT, ref PARAM_NORMALIZATION_SET_ON_RIGHT);
    }

    /// <summary>
    /// Set normalization level in decibels
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public bool SetNormalizationLevel(API_3DTI_Common.T_ear ear, float level)
    {
        return HASetFloat(ear, "HA3DTI_Normalization_DB_", level, ref PARAM_NORMALIZATION_DBS_LEFT, ref PARAM_NORMALIZATION_DBS_RIGHT);
    }

    public bool SetWriteDebugLog(bool value)
    {
        PARAM_DEBUG_LOG = value;
        return haMixer.SetFloat("HA3DTI_DebugLog", Bool2Float(value)); 
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
                AddToHABand(ear, 0, 0, value);
                AddToHABand(ear, 1, 0, value);
                AddToHABand(ear, 2, 0, value);
                AddToHABand(ear, 0, 1, value);
                AddToHABand(ear, 1, 1, value);
                AddToHABand(ear, 2, 1, value);
                AddToHABand(ear, 0, 2, value);
                AddToHABand(ear, 1, 2, value);
                AddToHABand(ear, 2, 2, value);
                break;                                           
            case T_toneBand.MID:                                 
                AddToHABand(ear, 0, 3, value);
                AddToHABand(ear, 1, 3, value);
                AddToHABand(ear, 2, 3, value);
                AddToHABand(ear, 0, 4, value);
                AddToHABand(ear, 1, 4, value);
                AddToHABand(ear, 1, 4, value);
                break;
            case T_toneBand.HIGH:                
                AddToHABand(ear, 0, 5, value);
                AddToHABand(ear, 1, 5, value);
                AddToHABand(ear, 2, 5, value);
                AddToHABand(ear, 0, 6, value);
                AddToHABand(ear, 1, 6, value);
                AddToHABand(ear, 2, 6, value);
                break;
            default:
                return false;                
        }

        tone[(int)ear, (int)band] = value;

        return true;
    }

    /// <summary>
    /// Set compression percentage for the dynamic equalizer
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetCompressionPercentage(API_3DTI_Common.T_ear ear, float value)
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
    /// <param name="band ([0..6])"></param>
    /// <param name="level ([0..2])"></param>
    /// <param name="gain (dB)"></param>
    /// <returns></returns>
    public bool SetDynamicEQBandLevelGain(API_3DTI_Common.T_ear ear, int band, int level, float gain)
    {
        string paramName = "HA3DTI_Gain_Level_" + level.ToString() + "_Band_" + band.ToString() + "_";
        return HASetFloat(ear, paramName, gain, ref PARAM_DYNAMICEQ_GAINS_LEFT[level, band], ref PARAM_DYNAMICEQ_GAINS_RIGHT[level, band]);
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
        return HASetFloat(ear, paramName, threshold, ref PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS[level], ref PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS[level]);
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
        return HASetFloat(ear, "HA3DTI_AttackRelease_", attackRelease, ref PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS, ref PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS);
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

        // Set level thresholds
        SetDynamicEQLevelThreshold(ear, 1, FIG6_THRESHOLD_0_DBSPL - DBSPL_FOR_0_DBFS);  // TO DO: coherent numbering. Curve 0 is now the reference for compression percentage
        SetDynamicEQLevelThreshold(ear, 0, FIG6_THRESHOLD_1_DBSPL - DBSPL_FOR_0_DBFS);  // TO DO: coherent numbering. Curve 0 is now the reference for compression percentage
        SetDynamicEQLevelThreshold(ear, 2, FIG6_THRESHOLD_2_DBSPL - DBSPL_FOR_0_DBFS);

        // Set band gains
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
        PARAM_NOISE_BEFORE_ON = noiseBefore;
        PARAM_NOISE_AFTER_ON = noiseAfter;

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
        PARAM_NOISE_NUMBITS = nbits;
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
        if (!SetDynamicEQBandLevelGain(ear, bandIndex, 1, gain0)) return false; // TO DO: coherent numbering. Curve 0 is now the reference for compression percentage
        if (!SetDynamicEQBandLevelGain(ear, bandIndex, 0, gain1)) return false; // TO DO: coherent numbering. Curve 0 is now the reference for compression percentage
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
    public bool HASetFloat(API_3DTI_Common.T_ear ear, string paramPrefix, float value, ref float paramLeft, ref float paramRight)
    {
        // Both ears
        if (ear == API_3DTI_Common.T_ear.BOTH)
        {
            if (!HASetFloat(API_3DTI_Common.T_ear.LEFT, paramPrefix, value, ref paramLeft, ref paramRight)) return false;
            return HASetFloat(API_3DTI_Common.T_ear.RIGHT, paramPrefix, value, ref paramLeft, ref paramRight);
        }

        // Build exposed parameter name string and set internal API parameters
        string paramName = paramPrefix;
        if (ear == API_3DTI_Common.T_ear.LEFT)
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

    /// <summary>
    /// Method for adding a (positive/negative) increment to a band in a curve of the dynamic eq
    /// The actual increment applied is newIncrement - oldIncrement.
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="level"></param>
    /// <param name="band"></param>
    /// <param name="oldIncrement"></param>
    /// <param name="newIncrement"></param>
    /// <returns></returns>
    public bool AddToHABand(API_3DTI_Common.T_ear ear, int level, int band, float newIncrement)
    {        
        // Both ears
        if (ear == API_3DTI_Common.T_ear.BOTH)
        {
            if (!AddToHABand(API_3DTI_Common.T_ear.LEFT, level, band, newIncrement)) return false;
            return AddToHABand(API_3DTI_Common.T_ear.RIGHT, level, band, newIncrement);
        }        

        // Build exposed parameter name string 
        string paramName = "HA_3DTI_Gain_Level_" + level.ToString() + "_Band_" + band.ToString() + "_";
        if (ear == API_3DTI_Common.T_ear.LEFT)
        {
            paramName += "Left";            
        }
        else
        {
            paramName += "Right";
        }

        // Get current value, considering oldIncrement
        float currentValue;
        float oldIncrement = tone[(int)ear, (int)band];
        if (!haMixer.GetFloat(paramName, out currentValue)) return false;
        currentValue = currentValue - oldIncrement;

        // Set new value, with newIncrement
        if (!haMixer.SetFloat(paramName, currentValue + newIncrement))
            return false;

        // Set internal API parameters
        if (ear == API_3DTI_Common.T_ear.LEFT)
        {
            PARAM_DYNAMICEQ_GAINS_LEFT[level, band] = currentValue + newIncrement;
        }
        else
        {
            PARAM_DYNAMICEQ_GAINS_RIGHT[level, band] = currentValue + newIncrement;
        }

        return true;
    }

    /// <summary>
    /// Generic Switch method
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="paramPrefix"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool HASwitch(API_3DTI_Common.T_ear ear, string paramPrefix, bool value, ref bool paramLeft, ref bool paramRight)
    {
        // Both ears
        if (ear == API_3DTI_Common.T_ear.BOTH)
        {
            if (!HASwitch(API_3DTI_Common.T_ear.LEFT, paramPrefix, value, ref paramLeft, ref paramRight)) return false;
            return HASwitch(API_3DTI_Common.T_ear.RIGHT, paramPrefix, value, ref paramLeft, ref paramRight);
        }
                
        // Build exposed parameter name string and set internal API parameters
        string paramName = paramPrefix;
        if (ear == API_3DTI_Common.T_ear.LEFT)
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
