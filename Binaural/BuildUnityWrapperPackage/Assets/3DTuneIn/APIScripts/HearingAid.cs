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
using API_3DTI;
using System.Runtime.InteropServices;
using System;
//using UnityEditor;

using static API_3DTI.HearingAid.Parameter;

namespace API_3DTI
{
    public class HearingAid : AbstractMixerEffect
    {
        //private API_3DTI_HL HLAPI;
        // Global variables
        public AudioMixer haMixer;  // Drag&drop here the HAHL_3DTI_Mixer

        // Public type definitions
        public enum T_HAToneBand { LOW = 0, MID = 1, HIGH = 2 };
        public enum T_HADynamicEQBand { HZ_125 = 0, HZ_250 = 1, HZ_500 = 2, HZ_1K = 3, HZ_2K = 4, HZ_4K = 5, HZ_8K = 6 };
        public enum T_HADynamicEQLevel { LEVEL_0 = 0, LEVEL_1 = 1, LEVEL_2 = 2 };

        public enum Parameter
        {
            [Parameter(mixerNameLeft = "HA3DTI_Process_LeftOn", mixerNameRight = "HA3DTI_Process_RightOn", pluginNameLeft = "HAL", pluginNameRight = "HAR", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Enable", description = "Switch HA On/Off")]
            ProcessOn,
            [Parameter(mixerNameLeft = "HA3DTI_Volume_Left", mixerNameRight = "HA3DTI_Volume_Right", pluginNameLeft = "VOLL", pluginNameRight = "VOLR", units = "dB", type = typeof(float), min = -24.0f, max = 24.0f, defaultValue = 0.0f, label = "Volume", description = "HA volume (dB)")]
            VolumeDb,
            [Parameter(mixerNameLeft = "HA3DTI_LPF_Cutoff", mixerNameRight = "HA3DTI_LPF_Cutoff", pluginNameLeft = "LPF", pluginNameRight = "LPF", units = "Hz", type = typeof(float), min = 62.5f, max = 16000.0f, defaultValue = 3000.0f, label = "LPF cutoff", description = "Cutoff frequency of LPF")]
            EqLpfCutoffHz,
            [Parameter(mixerNameLeft = "HA3DTI_HPF_Cutoff", mixerNameRight = "HA3DTI_HPF_Cutoff", pluginNameLeft = "HPF", pluginNameRight = "HPF", units = "Hz", type = typeof(float), min = 62.5f, max = 16000.0f, defaultValue = 500.0f, label = "HPF cutoff", description = "Cutoff frequency of HPF")]
            EqHpfCutoffHz,
            [Parameter(mixerNameLeft = "HA3DTI_Interpolation_On", mixerNameRight = "HA3DTI_Interpolation_On", pluginNameLeft = "EQINT", pluginNameRight = "EQINT", units = "", type = typeof(bool), defaultValue = 1.0f, label = "EQ Level interpolation", description = "Switch On/Off Dynamic EQ Level interpolation")]
            DynamicEqInterpolationOn,
            [Parameter(mixerNameLeft = "HA3DTI_Threshold_0_Left", mixerNameRight = "HA3DTI_Threshold_0_Right", pluginNameLeft = "THR0L", pluginNameRight = "THR0R", units = "dBfs", type = typeof(float), min = -80.0f, max = 0.0f, defaultValue = -20.0f, label = "EQ level 1 threshold", description = "Dynamic EQ first level threshold")]
            DynamicEqLevelThreshold0Dbfs,
            [Parameter(mixerNameLeft = "HA3DTI_Threshold_1_Left", mixerNameRight = "HA3DTI_Threshold_1_Right", pluginNameLeft = "THR1L", pluginNameRight = "THR1R", units = "dBfs", type = typeof(float), min = -80.0f, max = 0.0f, defaultValue = -40.0f, label = "EQ level 2 threshold", description = "Dynamic EQ second level threshold")]
            DynamicEqLevelThreshold1Dbfs,
            [Parameter(mixerNameLeft = "HA3DTI_Threshold_2_Left", mixerNameRight = "HA3DTI_Threshold_2_Right", pluginNameLeft = "THR2L", pluginNameRight = "THR2R", units = "dBfs", type = typeof(float), min = -80.0f, max = 0.0f, defaultValue = -60.0f, label = "EQ level 3 threshold", description = "Dynamic EQ third level threshold")]
            DynamicEqLevelThreshold2Dbfs,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_0_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_0_Right", pluginNameLeft = "DEQL0B0L", pluginNameRight = "DEQL0B0R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 1 Gain 125 Hz band", description = "EQ 125 Hz band gain (dB) for first level")]
            DynamicEqLevel0Band0Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_1_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_1_Right", pluginNameLeft = "DEQL0B1L", pluginNameRight = "DEQL0B1R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 1 Gain 250 Hz band", description = "EQ 250 Hz band gain (dB) for first level")]
            DynamicEqLevel0Band1Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_2_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_2_Right", pluginNameLeft = "DEQL0B2L", pluginNameRight = "DEQL0B2R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 1 Gain 500 Hz band", description = "EQ 500 Hz band gain (dB) for first level")]
            DynamicEqLevel0Band2Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_3_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_3_Right", pluginNameLeft = "DEQL0B3L", pluginNameRight = "DEQL0B3R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 1 Gain 1 KHz band", description = "EQ 1 KHz band gain (dB) for first level")]
            DynamicEqLevel0Band3Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_4_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_4_Right", pluginNameLeft = "DEQL0B4L", pluginNameRight = "DEQL0B4R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 1 Gain 2 KHz band", description = "EQ 2 KHz band gain (dB) for first level")]
            DynamicEqLevel0Band4Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_5_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_5_Right", pluginNameLeft = "DEQL0B5L", pluginNameRight = "DEQL0B5R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 1 Gain 4 KHz band", description = "EQ 4 KHz band gain (dB) for first level")]
            DynamicEqLevel0Band5Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_6_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_6_Right", pluginNameLeft = "DEQL0B6L", pluginNameRight = "DEQL0B6R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 1 Gain 8 KHz band", description = "EQ 8 KHz band gain (dB) for first level")]
            DynamicEqLevel0Band6Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_0_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_0_Right", pluginNameLeft = "DEQL1B0L", pluginNameRight = "DEQL1B0R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 2 Gain 125 Hz band", description = "EQ 125 Hz band gain (dB) for second level")]
            DynamicEqLevel1Band0Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_1_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_1_Right", pluginNameLeft = "DEQL1B1L", pluginNameRight = "DEQL1B1R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 2 Gain 250 Hz band", description = "EQ 250 Hz band gain (dB) for second level")]
            DynamicEqLevel1Band1Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_2_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_2_Right", pluginNameLeft = "DEQL1B2L", pluginNameRight = "DEQL1B2R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 2 Gain 500 Hz band", description = "EQ 500 Hz band gain (dB) for second level")]
            DynamicEqLevel1Band2Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_3_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_3_Right", pluginNameLeft = "DEQL1B3L", pluginNameRight = "DEQL1B3R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 2 Gain 1 KHz band", description = "EQ 1 KHz band gain (dB) for second level")]
            DynamicEqLevel1Band3Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_4_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_4_Right", pluginNameLeft = "DEQL1B4L", pluginNameRight = "DEQL1B4R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 2 Gain 2 KHz band", description = "EQ 2 KHz band gain (dB) for second level")]
            DynamicEqLevel1Band4Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_5_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_5_Right", pluginNameLeft = "DEQL1B5L", pluginNameRight = "DEQL1B5R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 2 Gain 4 KHz band", description = "EQ 4 KHz band gain (dB) for second level")]
            DynamicEqLevel1Band5Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_6_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_6_Right", pluginNameLeft = "DEQL1B6L", pluginNameRight = "DEQL1B6R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 2 Gain 8 KHz band", description = "EQ 8 KHz band gain (dB) for second level")]
            DynamicEqLevel1Band6Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_0_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_0_Right", pluginNameLeft = "DEQL2B0L", pluginNameRight = "DEQL2B0R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 3 Gain 125 Hz band", description = "EQ 125 Hz band gain (dB) for third level")]
            DynamicEqLevel2Band0Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_1_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_1_Right", pluginNameLeft = "DEQL2B1L", pluginNameRight = "DEQL2B1R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 3 Gain 250 Hz band", description = "EQ 250 Hz band gain (dB) for third level")]
            DynamicEqLevel2Band1Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_2_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_2_Right", pluginNameLeft = "DEQL2B2L", pluginNameRight = "DEQL2B2R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 3 Gain 500 Hz band", description = "EQ 500 Hz band gain (dB) for third level")]
            DynamicEqLevel2Band2Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_3_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_3_Right", pluginNameLeft = "DEQL2B3L", pluginNameRight = "DEQL2B3R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 3 Gain 1 KHz band", description = "EQ 1 KHz band gain (dB) for third level")]
            DynamicEqLevel2Band3Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_4_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_4_Right", pluginNameLeft = "DEQL2B4L", pluginNameRight = "DEQL2B4R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 3 Gain 2 KHz band", description = "EQ 2 KHz band gain (dB) for third level")]
            DynamicEqLevel2Band4Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_5_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_5_Right", pluginNameLeft = "DEQL2B5L", pluginNameRight = "DEQL2B5R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 3 Gain 4 KHz band", description = "EQ 4 KHz band gain (dB) for third level")]
            DynamicEqLevel2Band5Db,
            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_6_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_6_Right", pluginNameLeft = "DEQL2B6L", pluginNameRight = "DEQL2B6R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "EQ Level 3 Gain 8 KHz band", description = "EQ 8 KHz band gain (dB) for third level")]
            DynamicEqLevel2Band6Db,
            [Parameter(mixerNameLeft = "HA3DTI_AttackRelease_Left", mixerNameRight = "HA3DTI_AttackRelease_Right", pluginNameLeft = "ATREL", pluginNameRight = "ATRER", units = "ms", type = typeof(float), min = 10.0f, max = 2000.0f, defaultValue = 1000.0f, label = "Attack/release", description = "Attack/release (ms)")]
            DynamicEqAttackreleaseMs,
            [Parameter(mixerNameLeft = "HA3DTI_NoiseBefore_On", mixerNameRight = "HA3DTI_NoiseBefore_On", pluginNameLeft = "NOISEBEF", pluginNameRight = "NOISEBEF", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Quantization noise at start", description = "Apply quantization noise On/Off at the start of the process chain")]
            NoiseBeforeOn,
            [Parameter(mixerNameLeft = "HA3DTI_NoiseAfter_On", mixerNameRight = "HA3DTI_NoiseAfter_On", pluginNameLeft = "NOISEAFT", pluginNameRight = "NOISEAFT", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Quantization noise at end", description = "Apply quantization noise On/Off at the end of the process chain")]
            NoiseAfterOn,
            [Parameter(mixerNameLeft = "HA3DTI_NoiseBits", mixerNameRight = "HA3DTI_NoiseBits", pluginNameLeft = "NOISEBITS", pluginNameRight = "NOISEBITS", units = "bits", min = 6, max = 24, defaultValue = 16, label = "Quantization bits", description = "Number of bits of quantization noise")]
            NoiseNumbits,
            [Parameter(mixerNameLeft = "HA3DTI_Compression_Left", mixerNameRight = "HA3DTI_Compression_Right", pluginNameLeft = "COMPRL", pluginNameRight = "COMPRR", units = "%", type = typeof(float), min = 0.0f, max = 120.0f, defaultValue = 100.0f, label = "Compression", description = "Amount of compression")]
            CompressionPercentage,
            [Parameter(mixerNameLeft = "HA3DTI_Limiter_On", mixerNameRight = "HA3DTI_Limiter_On", pluginNameLeft = "LIMITON", pluginNameRight = "LIMITON", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Limiter", description = "Enable Limiter for HA")]
            LimiterSetOn,
            [Parameter(mixerNameLeft = "HA3DTI_Get_Limiter_Compression", mixerNameRight = "HA3DTI_Get_Limiter_Compression", pluginNameLeft = "LIMITGET", pluginNameRight = "LIMITGET", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Is limiter compressing?", description = "Is HA limiter compressing?")]
            LimiterGetCompression,
            [Parameter(mixerNameLeft = "HA3DTI_Normalization_LeftOn", mixerNameRight = "HA3DTI_Normalization_RightOn", pluginNameLeft = "NORMONL", pluginNameRight = "NORMONR", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Normalization", description = "Enable normalization")]
            NormalizationOn,
            [Parameter(mixerNameLeft = "HA3DTI_Normalization_DB_Left", mixerNameRight = "HA3DTI_Normalization_DB_Right", pluginNameLeft = "NORMDBL", pluginNameRight = "NORMDBR", units = "dBs", type = typeof(float), min = 1.0f, max = 40.0f, defaultValue = 20.0f, label = "Normalization amount", description = "Amount of normalization (in dBs)")]
            NormalizationDbs,
            [Parameter(mixerNameLeft = "HA3DTI_Normalization_Get_Left", mixerNameRight = "HA3DTI_Normalization_Get_Right", pluginNameLeft = "NORMGL", pluginNameRight = "NORMGR", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Is normalization applying offset?", description = "Is normalization applying offset?")]
            NormalizationGet,
            [Parameter(mixerNameLeft = "HA3DTI_Tone_High_Left", mixerNameRight = "HA3DTI_Tone_High_Right", pluginNameLeft = "TONLOL", pluginNameRight = "TONLOR", units = "dB", type = typeof(float), min = -10.0f, max = 10.0f, defaultValue = 0.0f, label = "Low band tone control", description = "Left tone control for low band (dB)")]
            ToneLow,
            [Parameter(mixerNameLeft = "HA3DTI_Tone_Low_Left", mixerNameRight = "HA3DTI_Tone_Low_Right", pluginNameLeft = "TONMIL", pluginNameRight = "TONMIR", units = "dB", type = typeof(float), min = -10.0f, max = 10.0f, defaultValue = 0.0f, label = "Mid band tone control", description = "Left tone control for mid band (dB)")]
            ToneMid,
            [Parameter(mixerNameLeft = "HA3DTI_Tone_Mid_Left", mixerNameRight = "HA3DTI_Tone_Mid_Right", pluginNameLeft = "TONHIL", pluginNameRight = "TONHIR", units = "dB", type = typeof(float), min = -10.0f, max = 10.0f, defaultValue = 0.0f, label = "High band tone control", description = "Left tone control for high band (dB)")]
            ToneHigh,
            [Parameter(mixerNameLeft = "HA3DTI_Handle", mixerNameRight = "HA3DTI_Handle", pluginNameLeft = "HANDLE", pluginNameRight = "HANDLE", units = "", type = typeof(int), min = 0, max = 16777216, defaultValue = 0, label = "Handle", description = "Read-only handle identifying this plugin instance")]
            Handle,
        }

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

        // Internal parameters for consistency with GUI
        [HideInInspector]
        public bool PARAM_PROCESS_LEFT_ON = false;      // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public bool PARAM_PROCESS_RIGHT_ON = false;     // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_VOLUME_L_DB = 0.0f;          // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_VOLUME_R_DB = 0.0f;          // For internal use, DO NOT USE IT DIRECTLY
                                                        // Common values for both ears in EQ		
        [HideInInspector]
        public float PARAM_EQ_LPFCUTOFF_HZ = 0.0f;      // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_EQ_HPFCUTOFF_HZ = 0.0f;      // For internal use, DO NOT USE IT DIRECTLY
                                                        // Dynamic EQ
        [HideInInspector]
        public bool PARAM_DYNAMICEQ_INTERPOLATION_ON = true;    // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float[] PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS = new float[3] { 0.0f, 0.0f, 0.0f }; // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float[] PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS = new float[3] { 0.0f, 0.0f, 0.0f }; // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float[,] PARAM_DYNAMICEQ_GAINS_LEFT = new float[3, 7]  { { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
                                                                     { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
                                                                     { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f } };    // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float[,] PARAM_DYNAMICEQ_GAINS_RIGHT = new float[3, 7] { { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
                                                                     { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },
                                                                     { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f } };    // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS = 1000.0f;   // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS = 1000.0f;  // For internal use, DO NOT USE IT DIRECTLY
                                                                        // Quantization noise
        [HideInInspector]
        public bool PARAM_NOISE_BEFORE_ON = false;      // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public bool PARAM_NOISE_AFTER_ON = false;       // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public int PARAM_NOISE_NUMBITS = 24;            // For internal use, DO NOT USE IT DIRECTLY
                                                        // Simplified controls
        [HideInInspector]
        public float PARAM_COMPRESSION_PERCENTAGE_LEFT = 0.0f;  // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_COMPRESSION_PERCENTAGE_RIGHT = 0.0f; // For internal use, DO NOT USE IT DIRECTLY
                                                                // Limiter
        [HideInInspector]
        public bool PARAM_LIMITER_ON = false;                   // For internal use, DO NOT USE IT DIRECTLY
                                                                // Normalization
        [HideInInspector]
        public bool PARAM_NORMALIZATION_SET_ON_LEFT = false;    // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_NORMALIZATION_DBS_LEFT = 20.0f;      // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public bool PARAM_NORMALIZATION_SET_ON_RIGHT = false;   // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_NORMALIZATION_DBS_RIGHT = 20.0f;     // For internal use, DO NOT USE IT DIRECTLY
                                                                // Tone control
        [HideInInspector]
        public float PARAM_TONE_LOW_LEFT = 0.0f;    // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_TONE_MID_LEFT = 0.0f;    // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_TONE_HIGH_LEFT = 0.0f;   // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_TONE_LOW_RIGHT = 0.0f;   // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_TONE_MID_RIGHT = 0.0f;   // For internal use, DO NOT USE IT DIRECTLY
        [HideInInspector]
        public float PARAM_TONE_HIGH_RIGHT = 0.0f;  // For internal use, DO NOT USE IT DIRECTLY
                                                    // Debug log
        [HideInInspector]
        public bool PARAM_DEBUG_LOG = false;        // For internal use, DO NOT USE IT DIRECTLY

        //////////////////////////////////////////////////////////////
        // INITIALIZATION
        //////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////
        // GET METHODS
        //////////////////////////////////////////////////////////////


        public bool SetParameter<T>(Parameter p, T value, T_ear ear = T_ear.BOTH) where T : IConvertible
        {
            return _SetParameter(haMixer, p, value, ear);
        }

        public T GetParameter<T>(Parameter p, T_ear ear)
        {
            return _GetParameter<Parameter, T>(haMixer, p, ear);
        }

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
                float max = PARAM_DYNAMICEQ_GAINS_LEFT[0, 0];
                for (int i = 0; i < NUM_EQ_BANDS; i++)
                {
                    if (PARAM_DYNAMICEQ_GAINS_LEFT[0, i] > max)
                        max = PARAM_DYNAMICEQ_GAINS_LEFT[0, i];
                }
                offset = PARAM_NORMALIZATION_DBS_LEFT - max;
            }
            else
            {
                float max = PARAM_DYNAMICEQ_GAINS_RIGHT[0, 0];
                for (int i = 0; i < NUM_EQ_BANDS; i++)
                {
                    if (PARAM_DYNAMICEQ_GAINS_RIGHT[0, i] > max)
                        max = PARAM_DYNAMICEQ_GAINS_RIGHT[0, i];
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
            /*if(ear == T_ear.LEFT)
            {
                paramName += "L";
            }
            else
            {
                paramName += "R";
            }*/
            //return plugin.SetFloatParameter(paramName, gain);
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
            string paramName = "HA3DTI_Threshold_";
            if (ear == T_ear.LEFT)
            {
                //paramName += "L";
                PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS[(int)eqlevel] = threshold;
            }
            else
            {
                //paramName += "R";
                PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS[(int)eqlevel] = threshold;
            }
            paramName += ((int)eqlevel).ToString();
            paramName += "_";

            //return plugin.SetFloatParameter(paramName, threshold);
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
            bool aux = haMixer.SetFloat("HA3DTI_LPF_Cutoff", cutoff);
            Debug.Log(aux.ToString());
            return aux;
        }

        /// <summary>
        /// Set cutoff frequency (in Hz) of high pass filter
        /// </summary>    
        /// <param name="cutoff (Hz)"></param>
        /// <returns></returns>
        public bool SetHPFCutoff(float cutoff)
        {
            PARAM_EQ_HPFCUTOFF_HZ = cutoff;
            bool aux = haMixer.SetFloat("HA3DTI_HPF_Cutoff", cutoff);
            Debug.Log("hpf: " + aux.ToString());
            return aux;
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


#if UNITY_IPHONE
    [DllImport ("__Internal")]
#else
        [DllImport("AudioPlugin3DTIToolkit")]
#endif
        private static extern bool SetDynamicEqualizerUsingFig6(int effectHandle, int C_Ear, float[] earLosses, int earLossesSize, float dBs_SPL_for_0_dBs_fs);

#if UNITY_IPHONE
    [DllImport ("__Internal")]
#else
        [DllImport("AudioPlugin3DTIToolkit")]
#endif
        private static extern bool GetHADynamicEqGain(int effectHandle, int level, int band, out float leftGain, out float rightGain);


        /// <summary>
        /// Grab audiometry from HearingLoss and use it to set the gains using FIG6.
        /// </summary>
        /// <param name="ear"></param>
        /// <param name="hearingLoss">An instance of HearingLoss component</param>
        /// <returns>The applied gains for both ears: ear -> level -> band. Or null if the set failed</returns>
        public float[,,] SetEQFromFig6(T_ear ear, HearingLoss hearingLoss)
        {
            var audiometry = new List<float>();
            HearingLoss.T_HLBand[] audiometryBands = {
                // top and bottom band not in HA:

                //HearingLoss.T_HLBand.HZ_62,
                HearingLoss.T_HLBand.HZ_125,
                HearingLoss.T_HLBand.HZ_250,
                HearingLoss.T_HLBand.HZ_500,
                HearingLoss.T_HLBand.HZ_1K,
                HearingLoss.T_HLBand.HZ_2K,
                HearingLoss.T_HLBand.HZ_4K,
                HearingLoss.T_HLBand.HZ_8K,
                //HearingLoss.T_HLBand.HZ_16K,
            };
            foreach (HearingLoss.T_HLBand band in audiometryBands)
            {
                int index = (int)band;
                audiometry.Add(hearingLoss.GetParameter<float>(HearingLoss.Parameter.MultibandExpansionBand0 + index, ear));
            }
            return SetEQFromFig6(ear, audiometry);
        }

        /// <summary>
        /// Configure dynamic equalizer using Fig6 method
        /// </summary>
        /// <param name="ear"></param>
        /// <param name="earLossList (dB[])">Losses for the 7 bands supported by HA</param>
        /// <returns>The applied gains for both ears: ear -> level -> band. Or null if the set failed</returns>
        public float[,,] SetEQFromFig6(T_ear ear, List<float> earLossInput)
        {
            int c_ear = ear == T_ear.LEFT ? 0 : ear == T_ear.RIGHT ? 1 : ear == T_ear.BOTH ? 2 : 3;
            bool ok = SetDynamicEqualizerUsingFig6(GetPluginHandle(), c_ear, earLossInput.ToArray(), earLossInput.Count, DBSPL_FOR_0_DBFS);
            // values are updated in the plugin but we need to poke unity to update its cache of them.
            // We will store the gains in this
            float[,,] gainsByEarBandLevel = new float[2, 3, 7];
            foreach (T_HADynamicEQLevel level in Enum.GetValues(typeof(T_HADynamicEQLevel)))
            {
                foreach (T_HADynamicEQBand band in Enum.GetValues(typeof(T_HADynamicEQBand)))
                {
                    if (!GetHADynamicEqGain(GetPluginHandle(), (int)level, (int)band, out gainsByEarBandLevel[0, (int)level, (int)band], out gainsByEarBandLevel[1, (int)level, (int)band]))
                    {
                        Debug.LogError("Failed to get gain from HA Dll.");
                        ok = false;
                    }
                    // If we are in play mode then we need to use the mixer to set these gains to make sure the cached exposed parameter values match the (now updated) values in the plugin.
                    // If we are in edit mode, then it is up to the inspector to update its UI by querying the plugin.
                    if (Application.isPlaying)
                    {
                        if (!haMixer.SetFloat($"HA3DTI_Gain_Level_{(int)level}_Band_{(int)band}_Left", gainsByEarBandLevel[0, (int)level, (int)band]))
                        {
                            Debug.LogError($"Failed to set gain parameter HA3DTI_Gain_Level_{(int)level}_Band_{(int)band}_Left on mixer.");
                        }
                        if (!haMixer.SetFloat($"HA3DTI_Gain_Level_{(int)level}_Band_{(int)band}_Right", gainsByEarBandLevel[1, (int)level, (int)band]))
                        {
                            Debug.LogError($"Failed to set gain parameter HA3DTI_Gain_Level_{(int)level}_Band_{(int)band}_Right on mixer.");
                        }
                    }
                }
            }
            return ok ? gainsByEarBandLevel : null;
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
            SetDynamicEQBandLevelGain(ear, bandIndex, T_HADynamicEQLevel.LEVEL_1, gain0); // TO DO: coherent numbering. Curve 0 is now the reference for compression percentage
            SetDynamicEQBandLevelGain(ear, bandIndex, T_HADynamicEQLevel.LEVEL_0, gain1); // TO DO: coherent numbering. Curve 0 is now the reference for compression percentage
            SetDynamicEQBandLevelGain(ear, bandIndex, T_HADynamicEQLevel.LEVEL_2, gain2);

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

        // Returns the plugin's native handle. This integer is unique per plugin instance and is needed to the Fig6 method so the native code knows which plugin instance to apply it to.
        private int GetPluginHandle()
        {
            if (haMixer.GetFloat("HA3DTI_Handle", out float fHandle))
            {
                return (int)fHandle;
            }
            else
            {
                return -1;
            }
        }
    }
}