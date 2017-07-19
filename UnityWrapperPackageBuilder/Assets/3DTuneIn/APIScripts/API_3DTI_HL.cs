/**
*** API for 3D-Tune-In Toolkit HL Simulation Unity Wrapper ***
*
* version beta 1.0
* Created on: November 2016
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

public class API_3DTI_HL : MonoBehaviour
{
    // Global variables
    public AudioMixer hlMixer;  // Drag&drop here the HAHL_3DTI_Mixer

    // Public constant and type definitions 
    public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_MILD = new ReadOnlyCollection<float>(new[] { 7f, 7f, 12f, 15f, 22f, 25f, 25f, 25f, 25f });
    public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_MODERATE = new ReadOnlyCollection<float>(new[] { 22f, 22f, 27f, 30f, 37f, 40f, 40f, 40f, 40f });
    public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_SEVERE = new ReadOnlyCollection<float>(new[] { 47f, 47f, 52f, 55f, 62f, 65f, 65f, 65f, 65f });
    public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_NORMAL = new ReadOnlyCollection<float>(new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f });        
    public enum T_HLBand { HZ_62 =0, HZ_125 =1, HZ_250 =2, HZ_500 = 3, HZ_1K = 4, HZ_2K = 5, HZ_4K = 6, HZ_8K =7, HZ_16K =8 };
    public const int NUM_HL_BANDS = 9;

    // Internal constants
    const float DEFAULT_CALIBRATION = 100.0f;
    const float DEFAULT_ATTACK = 20.0f;
    const float DEFAULT_RELEASE = 100.0f;

    // Internal parameters for consistency with GUI
    [HideInInspector]
    public bool GLOBAL_LEFT_ON = false;                         // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool GLOBAL_RIGHT_ON = false;                        // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_CALIBRATION = DEFAULT_CALIBRATION;       // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float [] PARAM_AUDIOMETRY_LEFT = new float[9] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };  // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float [] PARAM_AUDIOMETRY_RIGHT = new float[9] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }; // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_ATTACK = DEFAULT_ATTACK;            // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_RELEASE = DEFAULT_RELEASE;          // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_ATTACK = DEFAULT_ATTACK;           // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_RELEASE = DEFAULT_RELEASE;         // For internal use, DO NOT USE IT DIRECTLY

    /// <summary>
    /// Set audiometry preset for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="preset ({AUDIOGRAM_PRESET_MILD, AUDIOGRAM_PRESET_MODERATE, AUDIOGRAM_PRESET_SEVERE, AUDIOGRAM_PRESET_NORMAL})"></param>
    /// <returns></returns>    
    public bool SetAudiometryPreset(T_ear ear, ReadOnlyCollection<float> presetAudiometry)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetAudiometryPreset(T_ear.LEFT, presetAudiometry)) return false;
            return SetAudiometryPreset(T_ear.RIGHT, presetAudiometry);
        }

        List<float> hearingLevels = new List<float>(presetAudiometry); 
        return SetAudiometry(ear, hearingLevels);
    }

    /// <summary>
    /// Enable hearing loss for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool EnableHearingLoss(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!EnableHearingLoss(T_ear.LEFT)) return false;
            return EnableHearingLoss(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            GLOBAL_LEFT_ON = true;
            paramName += "Process_LeftOn";
        }
        else
        {
            GLOBAL_RIGHT_ON = true;
            paramName += "Process_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(true));
    }

    /// <summary>
    /// Disable hearing loss for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool DisableHearingLoss(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!DisableHearingLoss(T_ear.LEFT)) return false;
            return DisableHearingLoss(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            GLOBAL_LEFT_ON = false;
            paramName += "Process_LeftOn";
        }
        else
        {
            GLOBAL_RIGHT_ON = false;
            paramName += "Process_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(false));
    }

    /// <summary>
    /// Set all hearing loss levels (full audiometry) for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="hearingLevels (dBHL[])"></param>
    /// <returns></returns>
    public bool SetAudiometry(T_ear ear, List<float> hearingLevels)
    {
        for (int b=0; b < hearingLevels.Count; b++)
        {
            if (!SetHearingLevel(ear, b, hearingLevels[b])) return false;
        }

        return true;
    }

    /// <summary>
    /// Set hearing loss level for one band in one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="band"></param>
    /// <param name="hearingLevel (dBHL)"></param>
    /// <returns></returns>
    public bool SetHearingLevel(T_ear ear, int band, float hearingLevel)
    {
        // Check size
        if ((band < 0) || (band > NUM_HL_BANDS))
            return false;

        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetHearingLevel(T_ear.LEFT, band, hearingLevel)) return false;
            return SetHearingLevel(T_ear.RIGHT, band, hearingLevel);
        }
        
        // Set internal variables and build parameter string
        string paramName = "HL3DTI_HL_Band_" + band.ToString() + "_";
        if (ear == T_ear.LEFT)
        {
            PARAM_AUDIOMETRY_LEFT[band] = hearingLevel;
            paramName += "Left";
        }
        else
        {
            PARAM_AUDIOMETRY_RIGHT[band] = hearingLevel;
            paramName += "Right";
        }

        // Send command
        return hlMixer.SetFloat(paramName, hearingLevel);
    }

    /// <summary>
    /// Set attack of all bands envelope detectors for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="attack (ms)"></param>
    /// <returns></returns>
    public bool SetAttackForAllBands(T_ear ear, float attack)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetAttackForAllBands(T_ear.LEFT, attack)) return false;
            return SetAttackForAllBands(T_ear.RIGHT, attack);
        }
        if (ear == T_ear.LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_Attack_Left", attack)) return false;
            PARAM_LEFT_ATTACK = attack;
        }
        if (ear == T_ear.RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_Attack_Right", attack)) return false;
            PARAM_RIGHT_ATTACK = attack;
        }
        return true;
    }

    /// <summary>
    /// Set release of all bands envelope detectors for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="release (ms)"></param>
    /// <returns></returns>
    public bool SetReleaseForAllBands(T_ear ear, float release)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetReleaseForAllBands(T_ear.LEFT, release)) return false;
            return SetReleaseForAllBands(T_ear.RIGHT, release);
        }

        if (ear == T_ear.LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_Release_Left", release)) return false;
            PARAM_LEFT_RELEASE = release;
        }
        if (ear == T_ear.RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_Release_Right", release)) return false;
            PARAM_RIGHT_RELEASE = release;
        }
        return true;
    }

    /// <summary>
    /// Set calibration to allow conversion between dBSPL and dBHL to dBFS (internally used by the hearing loss simulator)
    /// </summary>
    /// <param name="dBSPL_for_0dBFS (how many dBSPL are measured with 0dBFS)"></param>
    /// <returns></returns>
    public bool SetCalibration(float dBSPL_for_0dBFS)
    {
        PARAM_CALIBRATION = dBSPL_for_0dBFS;
        return hlMixer.SetFloat("HL3DTI_Calibration", dBSPL_for_0dBFS);
    }
}
