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
        // Global variables
        public AudioMixer haMixer;  // Drag&drop here the HAHL_3DTI_Mixer

        // Public type definitions
        public enum T_HAToneBand { LOW = 0, MID = 1, HIGH = 2 };
        public enum T_HADynamicEQBand { HZ_125 = 0, HZ_250 = 1, HZ_500 = 2, HZ_1K = 3, HZ_2K = 4, HZ_4K = 5, HZ_8K = 6 };
        public enum T_HADynamicEQLevel { FirstLevel = 0, SecondLevel = 1, ThirdLevel = 2 };

        public enum Parameter
        {

            [Parameter(mixerNameLeft = "HA3DTI_Process_LeftOn", mixerNameRight = "HA3DTI_Process_RightOn", pluginNameLeft = "HAL", pluginNameRight = "HAR", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Enable", description = "Switch Hearing Aid process On/Off")]
            ProcessOn,

            [Parameter(mixerNameLeft = "HA3DTI_Volume_Left", mixerNameRight = "HA3DTI_Volume_Right", pluginNameLeft = "VOLL", pluginNameRight = "VOLR", units = "dB", type = typeof(float), min = -24.0f, max = 24.0f, defaultValue = 0.0f, label = "Volume", description = "Set volume in decibels of Hearing Aid for each ear)")]
            VolumeDb,

            [Parameter(mixerNameLeft = "HA3DTI_LPF_Cutoff", mixerNameRight = "HA3DTI_LPF_Cutoff", pluginNameLeft = "LPF", pluginNameRight = "LPF", units = "Hz", type = typeof(float), min = 62.5f, max = 16000.0f, defaultValue = 3000.0f, label = "LPF cutoff", description = "Cutoff frequency of the low pass filter")]
            EqLpfCutoffHz,

            [Parameter(mixerNameLeft = "HA3DTI_HPF_Cutoff", mixerNameRight = "HA3DTI_HPF_Cutoff", pluginNameLeft = "HPF", pluginNameRight = "HPF", units = "Hz", type = typeof(float), min = 62.5f, max = 16000.0f, defaultValue = 500.0f, label = "HPF cutoff", description = "Cutoff frequency of high pass filter")]
            EqHpfCutoffHz,

            [Parameter(mixerNameLeft = "HA3DTI_Interpolation_On", mixerNameRight = "HA3DTI_Interpolation_On", pluginNameLeft = "EQINT", pluginNameRight = "EQINT", units = "", type = typeof(bool), defaultValue = 1.0f, label = "EQ Level interpolation", description = "Switch On/Off Dynamic EQ Level interpolation")]
            DynamicEqInterpolationOn,

            [Parameter(mixerNameLeft = "HA3DTI_Threshold_0_Left", mixerNameRight = "HA3DTI_Threshold_0_Right", pluginNameLeft = "THR0L", pluginNameRight = "THR0R", units = "dBfs", type = typeof(float), min = -80.0f, max = 0.0f, defaultValue = -20.0f, label = "Level 1", description = "Dynamic EQ first level threshold")]
            DynamicEqLevelThreshold0Dbfs,

            [Parameter(mixerNameLeft = "HA3DTI_Threshold_1_Left", mixerNameRight = "HA3DTI_Threshold_1_Right", pluginNameLeft = "THR1L", pluginNameRight = "THR1R", units = "dBfs", type = typeof(float), min = -80.0f, max = 0.0f, defaultValue = -40.0f, label = "Level 2", description = "Dynamic EQ second level threshold")]
            DynamicEqLevelThreshold1Dbfs,

            [Parameter(mixerNameLeft = "HA3DTI_Threshold_2_Left", mixerNameRight = "HA3DTI_Threshold_2_Right", pluginNameLeft = "THR2L", pluginNameRight = "THR2R", units = "dBfs", type = typeof(float), min = -80.0f, max = 0.0f, defaultValue = -60.0f, label = "Level 3", description = "Dynamic EQ third level threshold")]
            DynamicEqLevelThreshold2Dbfs,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_0_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_0_Right", pluginNameLeft = "DEQL0B0L", pluginNameRight = "DEQL0B0R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "125 Hz", description = "EQ 125 Hz band gain (dB) for first level")]
            DynamicEqLevel0Band0Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_1_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_1_Right", pluginNameLeft = "DEQL0B1L", pluginNameRight = "DEQL0B1R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "250 Hz", description = "EQ 250 Hz band gain (dB) for first level")]
            DynamicEqLevel0Band1Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_2_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_2_Right", pluginNameLeft = "DEQL0B2L", pluginNameRight = "DEQL0B2R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "500 Hz", description = "EQ 500 Hz band gain (dB) for first level")]
            DynamicEqLevel0Band2Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_3_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_3_Right", pluginNameLeft = "DEQL0B3L", pluginNameRight = "DEQL0B3R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "1 KHz", description = "EQ 1 KHz band gain (dB) for first level")]
            DynamicEqLevel0Band3Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_4_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_4_Right", pluginNameLeft = "DEQL0B4L", pluginNameRight = "DEQL0B4R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "2 KHz", description = "EQ 2 KHz band gain (dB) for first level")]
            DynamicEqLevel0Band4Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_5_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_5_Right", pluginNameLeft = "DEQL0B5L", pluginNameRight = "DEQL0B5R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "4 KHz", description = "EQ 4 KHz band gain (dB) for first level")]
            DynamicEqLevel0Band5Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_0_Band_6_Left", mixerNameRight = "HA3DTI_Gain_Level_0_Band_6_Right", pluginNameLeft = "DEQL0B6L", pluginNameRight = "DEQL0B6R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "8 KHz", description = "EQ 8 KHz band gain (dB) for first level")]
            DynamicEqLevel0Band6Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_0_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_0_Right", pluginNameLeft = "DEQL1B0L", pluginNameRight = "DEQL1B0R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "125 Hz", description = "EQ 125 Hz band gain (dB) for second level")]
            DynamicEqLevel1Band0Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_1_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_1_Right", pluginNameLeft = "DEQL1B1L", pluginNameRight = "DEQL1B1R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "250 Hz", description = "EQ 250 Hz band gain (dB) for second level")]
            DynamicEqLevel1Band1Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_2_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_2_Right", pluginNameLeft = "DEQL1B2L", pluginNameRight = "DEQL1B2R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "500 Hz", description = "EQ 500 Hz band gain (dB) for second level")]
            DynamicEqLevel1Band2Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_3_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_3_Right", pluginNameLeft = "DEQL1B3L", pluginNameRight = "DEQL1B3R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "1 KHz", description = "EQ 1 KHz band gain (dB) for second level")]
            DynamicEqLevel1Band3Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_4_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_4_Right", pluginNameLeft = "DEQL1B4L", pluginNameRight = "DEQL1B4R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "2 KHz", description = "EQ 2 KHz band gain (dB) for second level")]
            DynamicEqLevel1Band4Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_5_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_5_Right", pluginNameLeft = "DEQL1B5L", pluginNameRight = "DEQL1B5R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "4 KHz", description = "EQ 4 KHz band gain (dB) for second level")]
            DynamicEqLevel1Band5Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_1_Band_6_Left", mixerNameRight = "HA3DTI_Gain_Level_1_Band_6_Right", pluginNameLeft = "DEQL1B6L", pluginNameRight = "DEQL1B6R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "8 KHz", description = "EQ 8 KHz band gain (dB) for second level")]
            DynamicEqLevel1Band6Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_0_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_0_Right", pluginNameLeft = "DEQL2B0L", pluginNameRight = "DEQL2B0R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "125 Hz", description = "EQ 125 Hz band gain (dB) for third level")]
            DynamicEqLevel2Band0Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_1_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_1_Right", pluginNameLeft = "DEQL2B1L", pluginNameRight = "DEQL2B1R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "250 Hz", description = "EQ 250 Hz band gain (dB) for third level")]
            DynamicEqLevel2Band1Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_2_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_2_Right", pluginNameLeft = "DEQL2B2L", pluginNameRight = "DEQL2B2R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "500 Hz", description = "EQ 500 Hz band gain (dB) for third level")]
            DynamicEqLevel2Band2Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_3_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_3_Right", pluginNameLeft = "DEQL2B3L", pluginNameRight = "DEQL2B3R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "1 KHz", description = "EQ 1 KHz band gain (dB) for third level")]
            DynamicEqLevel2Band3Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_4_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_4_Right", pluginNameLeft = "DEQL2B4L", pluginNameRight = "DEQL2B4R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "2 KHz", description = "EQ 2 KHz band gain (dB) for third level")]
            DynamicEqLevel2Band4Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_5_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_5_Right", pluginNameLeft = "DEQL2B5L", pluginNameRight = "DEQL2B5R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "4 KHz", description = "EQ 4 KHz band gain (dB) for third level")]
            DynamicEqLevel2Band5Db,

            [Parameter(mixerNameLeft = "HA3DTI_Gain_Level_2_Band_6_Left", mixerNameRight = "HA3DTI_Gain_Level_2_Band_6_Right", pluginNameLeft = "DEQL2B6L", pluginNameRight = "DEQL2B6R", units = "dB", type = typeof(float), min = 0.0f, max = 60.0f, defaultValue = 0.0f, label = "8 KHz", description = "EQ 8 KHz band gain (dB) for third level")]
            DynamicEqLevel2Band6Db,

            [Parameter(mixerNameLeft = "HA3DTI_AttackRelease_Left", mixerNameRight = "HA3DTI_AttackRelease_Right", pluginNameLeft = "ATREL", pluginNameRight = "ATRER", units = "ms", type = typeof(float), min = 10.0f, max = 2000.0f, defaultValue = 1000.0f, label = "Attack/release", description = "Set attack and release time (in milliseconds) for dynamic equalizer envelope detector")]
            DynamicEqAttackreleaseMs,

            [Parameter(mixerNameLeft = "HA3DTI_NoiseBefore_On", mixerNameRight = "HA3DTI_NoiseBefore_On", pluginNameLeft = "NOISEBEF", pluginNameRight = "NOISEBEF", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Quantization noise at start", description = "Apply quantization noise On/Off at the start of the process chain")]
            NoiseBeforeOn,

            [Parameter(mixerNameLeft = "HA3DTI_NoiseAfter_On", mixerNameRight = "HA3DTI_NoiseAfter_On", pluginNameLeft = "NOISEAFT", pluginNameRight = "NOISEAFT", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Quantization noise at end", description = "Apply quantization noise On/Off at the end of the process chain")]
            NoiseAfterOn,

            [Parameter(mixerNameLeft = "HA3DTI_NoiseBits", type = typeof(int), mixerNameRight = "HA3DTI_NoiseBits", pluginNameLeft = "NOISEBITS", pluginNameRight = "NOISEBITS", units = "bits", min = 6, max = 24, defaultValue = 16, label = "Quantization bits", description = "Number of bits of quantization noise")]
            NoiseNumbits,

            [Parameter(mixerNameLeft = "HA3DTI_Compression_Left", mixerNameRight = "HA3DTI_Compression_Right", pluginNameLeft = "COMPRL", pluginNameRight = "COMPRR", units = "%", type = typeof(float), min = 0.0f, max = 120.0f, defaultValue = 100.0f, label = "Compression", description = "Set compression percentage for the dynamic equalizer")]
            CompressionPercentage,

            [Parameter(mixerNameLeft = "HA3DTI_Limiter_On", mixerNameRight = "HA3DTI_Limiter_On", pluginNameLeft = "LIMITON", pluginNameRight = "LIMITON", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Limiter", description = "Enable Limiter after Hearing Aid process")]
            LimiterSetOn,

            [Parameter(mixerNameLeft = "HA3DTI_Get_Limiter_Compression", mixerNameRight = "HA3DTI_Get_Limiter_Compression", pluginNameLeft = "LIMITGET", pluginNameRight = "LIMITGET", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Is limiter compressing?", description = "Get the current state of the limiter (compressing or not)", isReadOnly = true)]
            LimiterGetCompression,

            [Parameter(mixerNameLeft = "HA3DTI_Normalization_LeftOn", mixerNameRight = "HA3DTI_Normalization_RightOn", pluginNameLeft = "NORMONL", pluginNameRight = "NORMONR", units = "", type = typeof(bool), defaultValue = 0.0f, label = "Normalization", description = "Enable normalization")]
            NormalizationOn,

            [Parameter(mixerNameLeft = "HA3DTI_Normalization_DB_Left", mixerNameRight = "HA3DTI_Normalization_DB_Right", pluginNameLeft = "NORMDBL", pluginNameRight = "NORMDBR", units = "dBs", type = typeof(float), min = 1.0f, max = 40.0f, defaultValue = 20.0f, label = "Normalization level", description = "Normalization level (in dBs)")]
            NormalizationDbs,

            [Parameter(mixerNameLeft = "HA3DTI_Normalization_Get_Left", mixerNameRight = "HA3DTI_Normalization_Get_Right", pluginNameLeft = "NORMGL", pluginNameRight = "NORMGR", units = "dB", type = typeof(float), defaultValue = 0.0f, label = "Is normalization applying offset?", description = "Get the current state of the normalization process (overall offset applied, in dB)")]
            NormalizationGet,

            [Parameter(mixerNameLeft = "HA3DTI_Tone_High_Left", mixerNameRight = "HA3DTI_Tone_High_Right", pluginNameLeft = "TONLOL", pluginNameRight = "TONLOR", units = "dB", type = typeof(float), min = -10.0f, max = 10.0f, defaultValue = 0.0f, label = "Low band tone control", description = "Set the level for the low tone band (dB)")]
            ToneLow,

            [Parameter(mixerNameLeft = "HA3DTI_Tone_Low_Left", mixerNameRight = "HA3DTI_Tone_Low_Right", pluginNameLeft = "TONMIL", pluginNameRight = "TONMIR", units = "dB", type = typeof(float), min = -10.0f, max = 10.0f, defaultValue = 0.0f, label = "Mid band tone control", description = "Set the level for the mid tone band (dB)")]
            ToneMid,

            [Parameter(mixerNameLeft = "HA3DTI_Tone_Mid_Left", mixerNameRight = "HA3DTI_Tone_Mid_Right", pluginNameLeft = "TONHIL", pluginNameRight = "TONHIR", units = "dB", type = typeof(float), min = -10.0f, max = 10.0f, defaultValue = 0.0f, label = "High band tone control", description = "Set the level for the high tone band (dB)")]
            ToneHigh,

            [Parameter(mixerNameLeft = "HA3DTI_Handle", mixerNameRight = "HA3DTI_Handle", pluginNameLeft = "HANDLE", pluginNameRight = "HANDLE", units = "", type = typeof(int), min = 0, max = 16777216, defaultValue = 0, label = "Handle", description = "Read-only handle identifying this plugin instance")]
            Handle,
        }

        // Public constant definitions
        public const int NUM_EQ_CURVES = 3;
        public const int NUM_EQ_BANDS = 7;

        // Internal use constants
        const float DBSPL_FOR_0_DBFS = 100.0f;


        public bool SetParameter<T>(Parameter p, T value, T_ear ear = T_ear.BOTH) where T : IConvertible
        {
            return _SetParameter(haMixer, p, value, ear);
        }

        public T GetParameter<T>(Parameter p, T_ear ear)
        {
            return _GetParameter<Parameter, T>(haMixer, p, ear);
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