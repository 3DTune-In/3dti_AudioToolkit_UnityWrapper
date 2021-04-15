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
    //public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_MILD = new ReadOnlyCollection<float>(new[] { 7f, 7f, 12f, 17f, 28f, 31f, 31f, 31f, 31f });
    //public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_MODERATE = new ReadOnlyCollection<float>(new[] { 32f, 32f, 37f, 42f, 53f, 56f, 56f, 56f, 56f });
    //public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_SEVERE = new ReadOnlyCollection<float>(new[] { 62f, 62f, 67f, 72f, 83f, 86f, 86f, 86f, 86f });
    //public static readonly ReadOnlyCollection<float> AUDIOMETRY_PRESET_NORMAL = new ReadOnlyCollection<float>(new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f });        
    public enum T_HLBand { HZ_62 =0, HZ_125 =1, HZ_250 =2, HZ_500 = 3, HZ_1K = 4, HZ_2K = 5, HZ_4K = 6, HZ_8K =7, HZ_16K =8 };
    public const int NUM_HL_BANDS = 9;
    public enum T_HLTemporalDistortionBandUpperLimit { HZ_UL_200 =0, HZ_UL_400 =1, HZ_UL_800 =2, HZ_UL_1600 =3, HZ_UL_3200 =4, HZ_UL_6400 =5, HZ_UL_12800 =6, HZ_UL_WRONG =-1 };
    public enum T_HLClassificationScaleCurve {HL_CS_UNDEFINED = -1, HL_CS_NOLOSS = 0, HL_CS_A = 1, HL_CS_B = 2, HL_CS_C = 3, HL_CS_D = 4, HL_CS_E = 5, HL_CS_F = 6,
                                              HL_CS_G = 7, HL_CS_H = 8, HL_CS_I = 9, HL_CS_J = 10, HL_CS_K = 11};      
    public enum T_HLPreset { HL_PRESET_NORMAL =0, HL_PRESET_MILD =1, HL_PRESET_MODERATE =2, HL_PRESET_SEVERE =3, HL_PRESET_CUSTOM =-1};  
    public enum T_HLClassificationScaleSeverity { HL_CS_SEVERITY_NOLOSS =0, HL_CS_SEVERITY_MILD =1, HL_CS_SEVERITY_MILDMODERATE =2,
                                                  HL_CS_SEVERITY_MODERATE =3, HL_CS_SEVERITY_MODERATESEVERE =4, HL_CS_SEVERITY_SEVERE =5,
                                                  HL_CS_SEVERITY_PROFOUND =6, HL_CS_SEVERITY_UNDEFINED = -1};

    public enum T_HLFrequencySmearingApproach : int { BAERMOORE, GRAF };

    // Internal constants
    const float DEFAULT_CALIBRATION = 100.0f;
    const float DEFAULT_ATTACK = 20.0f;
    const float DEFAULT_RELEASE = 100.0f;
    const T_HLTemporalDistortionBandUpperLimit DEFAULT_TA_BANDUPPERLIMIT = T_HLTemporalDistortionBandUpperLimit.HZ_UL_1600;
    const float DEFAULT_TA_WHITENOISEPOWER = 0.0f;
    const float DEFAULT_TA_BANDWIDTH = 500.0f;
    const int DEFAULT_FS_SIZE = 1;
    const float DEFAULT_FS_HZ = 0.0f;

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

    [HideInInspector]
    public bool MBE_LEFT_ON = true;                         // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool MBE_RIGHT_ON = true;                        // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool TA_LEFT_ON = false;                         // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool TA_RIGHT_ON = false;                        // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public T_HLTemporalDistortionBandUpperLimit PARAM_LEFT_TA_BANDUPPERLIMIT = DEFAULT_TA_BANDUPPERLIMIT;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public T_HLTemporalDistortionBandUpperLimit PARAM_RIGHT_TA_BANDUPPERLIMIT = DEFAULT_TA_BANDUPPERLIMIT;     // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_TA_WHITENOISEPOWER = DEFAULT_TA_WHITENOISEPOWER;    // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_TA_WHITENOISEPOWER = DEFAULT_TA_WHITENOISEPOWER;   // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_TA_BANDWIDTH = DEFAULT_TA_BANDWIDTH;  // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_TA_BANDWIDTH = DEFAULT_TA_BANDWIDTH; // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_TA_LRSYNC = 0.0f;                    // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool PARAM_TA_LRSYNC_ON = false;                 // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool PARAM_LEFT_TA_POSTLPF = true;                    // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool PARAM_RIGHT_TA_POSTLPF = true;                    // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool FS_LEFT_ON = false;                         // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public bool FS_RIGHT_ON = false;                        // For internal use, DO NOT USE IT DIRECTLY
    //[HideInInspector]
    //public T_HLFrequencySmearingApproach PARAM_FREQUENCYSMEARING_APPROACH_LEFT = T_HLFrequencySmearingApproach.BAERMOORE;                         // For internal use, DO NOT USE IT DIRECTLY
    //[HideInInspector]
    //public T_HLFrequencySmearingApproach PARAM_FREQUENCYSMEARING_APPROACH_RIGHT = T_HLFrequencySmearingApproach.BAERMOORE;                        // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public int PARAM_LEFT_FS_DOWN_SIZE = DEFAULT_FS_SIZE;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public int PARAM_LEFT_FS_UP_SIZE = DEFAULT_FS_SIZE;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public int PARAM_RIGHT_FS_DOWN_SIZE = DEFAULT_FS_SIZE;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public int PARAM_RIGHT_FS_UP_SIZE = DEFAULT_FS_SIZE;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_FS_DOWN_HZ = DEFAULT_FS_HZ;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_LEFT_FS_UP_HZ = DEFAULT_FS_HZ;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_FS_DOWN_HZ = DEFAULT_FS_HZ;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public float PARAM_RIGHT_FS_UP_HZ = DEFAULT_FS_HZ;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public T_HLClassificationScaleCurve PARAM_CLASSIFICATION_CURVE_LEFT = T_HLClassificationScaleCurve.HL_CS_NOLOSS;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public int PARAM_CLASSIFICATION_SLOPE_LEFT = 0;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public T_HLClassificationScaleSeverity PARAM_CLASSIFICATION_SEVERITY_LEFT = 0;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public T_HLClassificationScaleCurve PARAM_CLASSIFICATION_CURVE_RIGHT = T_HLClassificationScaleCurve.HL_CS_NOLOSS;     // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]       
    public int PARAM_CLASSIFICATION_SLOPE_RIGHT = 0;     // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public T_HLClassificationScaleSeverity PARAM_CLASSIFICATION_SEVERITY_RIGHT = 0;      // For internal use, DO NOT USE IT DIRECTLY


    // Convenience method that avoids custom type allowing it to be connected to a UI element in the Editor.
    public void EnableHearingLossInBothEars(bool isEnabled)
    {
        if (isEnabled)
        {
            EnableHearingLoss(T_ear.BOTH);
        }
        else
        {
            DisableHearingLoss(T_ear.BOTH);
        }
    }

    ///////////////////////////////////////
    // GLOBAL CONTROLS
    ///////////////////////////////////////

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
    /// Set calibration to allow conversion between dBSPL and dBHL to dBFS (internally used by the hearing loss simulator)
    /// </summary>
    /// <param name="dBSPL_for_0dBFS (how many dBSPL are measured with 0dBFS)"></param>
    /// <returns></returns>
    public bool SetCalibration(float dBSPL_for_0dBFS)
    {
        PARAM_CALIBRATION = dBSPL_for_0dBFS;
        return hlMixer.SetFloat("HL3DTI_Calibration", dBSPL_for_0dBFS);
    }

    ///////////////////////////////////////
    // AUDIOMETRY 
    ///////////////////////////////////////

    ///// <summary>
    ///// Set audiometry template for one ear
    ///// </summary>
    ///// <param name="ear"></param>
    ///// <param name="template ({HL_PRESET_NORMAL, HL_PRESET_MILD, HL_PRESET_MODERATE, HL_PRESET_SEVERE})"></param>
    ///// <returns></returns>        
    //public bool SetAudiometryFromTemplate(T_ear ear, T_HLPreset template)
    //{
    //    // Both ears
    //    if (ear == T_ear.BOTH)
    //    {
    //        if (!SetAudiometryFromTemplate(T_ear.LEFT, template)) return false;
    //        return SetAudiometryFromTemplate(T_ear.RIGHT, template);
    //    }

    //    // Get template data from hardcoded ready only collections
    //    ReadOnlyCollection<float> templateCollection;
    //    switch (template)
    //    {
    //        case T_HLPreset.HL_PRESET_NORMAL:
    //            templateCollection = AUDIOMETRY_TEMPLATE_NORMAL;
    //            break;
    //        case T_HLPreset.HL_PRESET_MILD:
    //            templateCollection = AUDIOMETRY_TEMPLATE_MILD;
    //            break;
    //        case T_HLPreset.HL_PRESET_MODERATE:
    //            templateCollection = AUDIOMETRY_TEMPLATE_MODERATE;
    //            break;
    //        case T_HLPreset.HL_PRESET_SEVERE:
    //            templateCollection = AUDIOMETRY_TEMPLATE_SEVERE;
    //            break;
    //        default:
    //            return false;
    //    }

    //    // Set audiometry from read only collection data
    //    List<float> hearingLevels = new List<float>(templateCollection); 
    //    return SetAudiometry(ear, hearingLevels);
    //}

    /// <summary>
    /// Set all hearing loss levels (full audiometry) for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="hearingLevels (dBHL[])"></param>
    /// <returns></returns>
    public bool SetAudiometry(T_ear ear, List<float> hearingLevels)
    {
        for (int b = 0; b < hearingLevels.Count; b++)
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
    /// Set audiometry from a curve and slope level using HL Classification Scale
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="curve"></param>
    /// <param name="slope"></param>
    /// <param name="severity"></param>
    /// <returns></returns>
    public bool SetAudiometryFromClassificationScale(T_ear ear, T_HLClassificationScaleCurve curve, int slope, T_HLClassificationScaleSeverity severity)
    {
        // TO DO: Range check (anyway, it is done inside the plugin)        
        if (ear == T_ear.BOTH)
        {
            if (!SetAudiometryFromClassificationScale(T_ear.LEFT, curve, slope, severity)) return false;
            return SetAudiometryFromClassificationScale(T_ear.RIGHT, curve, slope, severity);
        }

        if (ear == T_ear.LEFT)
        {
            PARAM_CLASSIFICATION_CURVE_LEFT = curve;
            PARAM_CLASSIFICATION_SLOPE_LEFT = slope;
            PARAM_CLASSIFICATION_SEVERITY_LEFT = severity;
            //if (!hlMixer.SetFloat("HL3DTI_CS_Curve_Left", FromClassificationScaleCurveToFloat(curve))) return false;
            //return hlMixer.SetFloat("HL3DTI_CS_Slope_Left", (float)slope); <-- this is not implemented in plugin, nor AudioMixer (it was Severity before)
            List<float> hl;
            GetClassificationScaleHL(curve, slope, severity, out hl);
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_0_Left", hl[0])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_1_Left", hl[1])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_2_Left", hl[2])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_3_Left", hl[3])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_4_Left", hl[4])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_5_Left", hl[5])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_6_Left", hl[6])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_7_Left", hl[7])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_8_Left", hl[8])) return false;
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_CLASSIFICATION_CURVE_RIGHT = curve;
            PARAM_CLASSIFICATION_SLOPE_RIGHT = slope;
            PARAM_CLASSIFICATION_SEVERITY_RIGHT = severity;
            //if (!hlMixer.SetFloat("HL3DTI_CS_Curve_Right", FromClassificationScaleCurveToFloat(curve))) return false;
            //return hlMixer.SetFloat("HL3DTI_CS_Slope_Right", (float)slope); <-- this is not implemented in plugin, nor AudioMixer (it was Severity before)
            List<float> hl;
            GetClassificationScaleHL(curve, slope, severity, out hl);
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_0_Right", hl[0])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_1_Right", hl[1])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_2_Right", hl[2])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_3_Right", hl[3])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_4_Right", hl[4])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_5_Right", hl[5])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_6_Right", hl[6])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_7_Right", hl[7])) return false;
            if (!hlMixer.SetFloat("HL3DTI_HL_Band_8_Right", hl[8])) return false;
        }
        return true;
    }

    ///////////////////////////////////////
    // NON-LINEAR ATTENUATION (MULTIBAND EXPANDER)
    ///////////////////////////////////////

    /// <summary>
    /// Enable non-linear attenuation (multiband expander) for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool EnableNonLinearAttenuation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!EnableNonLinearAttenuation(T_ear.LEFT)) return false;
            return EnableNonLinearAttenuation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            MBE_LEFT_ON = true;
            paramName += "MBE_LeftOn";
        }
        else
        {
            MBE_RIGHT_ON = true;
            paramName += "MBE_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(true));
    }

    /// <summary>
    /// Disable non-linear attenuation (multiband expander) for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool DisableNonLinearAttenuation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!DisableNonLinearAttenuation(T_ear.LEFT)) return false;
            return DisableNonLinearAttenuation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            MBE_LEFT_ON = false;
            paramName += "MBE_LeftOn";
        }
        else
        {
            MBE_RIGHT_ON = false;
            paramName += "MBE_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(false));
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

    ///////////////////////////////////////
    // TEMPORAL DISTORTION SIMULATION
    ///////////////////////////////////////

    /// <summary>
    /// Enable Temporal distortion simulation for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool EnableTemporalDistortionSimulation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!EnableTemporalDistortionSimulation(T_ear.LEFT)) return false;
            return EnableTemporalDistortionSimulation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            TA_LEFT_ON = true;
            paramName += "TA_LeftOn";
        }
        else
        {
            TA_RIGHT_ON = true;
            paramName += "TA_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(true));
    }

    /// <summary>
    /// Disable Temporal distortion simulation for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool DisableTemporalDistortionSimulation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!DisableTemporalDistortionSimulation(T_ear.LEFT)) return false;
            return DisableTemporalDistortionSimulation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            TA_LEFT_ON = false;
            paramName += "TA_LeftOn";
        }
        else
        {
            TA_RIGHT_ON = false;
            paramName += "TA_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(false));
    }

    /// <summary>
    /// Disable left-right synchronicity in Temporal distortion simulation
    /// </summary>    
    /// <returns></returns>
    public bool EnableTemporalDistortionLeftRightSynchronicity()
    {
        PARAM_TA_LRSYNC_ON = true;
        return hlMixer.SetFloat("HL3DTI_TA_LRSync_On", CommonFunctions.Bool2Float(true));
    }

    /// <summary>
    /// Disable left-right synchronicity in Temporal distortion simulation
    /// </summary>    
    /// <returns></returns>
    public bool DisableTemporalDistortionLeftRightSynchronicity()
    {
        PARAM_TA_LRSYNC_ON = false;
        return hlMixer.SetFloat("HL3DTI_TA_LRSync_On", CommonFunctions.Bool2Float(false));
    }

    /// <summary>
    /// Set left-right synchronicity amount (if left-right synchronicity was previously enabled)
    /// </summary>
    /// <param name="leftRightSynchronicity"></param>
    /// <returns></returns>
    public bool SetTemporalDistortionLeftRightSynchronicity(float leftRightSynchronicity)
    {
        if ((leftRightSynchronicity < 0.0f) || (leftRightSynchronicity > 1.0f))
            return false;

        PARAM_TA_LRSYNC = leftRightSynchronicity;
        return hlMixer.SetFloat("HL3DTI_TA_LRSync", leftRightSynchronicity);
    }

    /// <summary>
    /// Set band upper limit for one ear in Temporal distortion simulator
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="bandUpperLimit (Hz)"></param>
    /// <returns></returns>
    public bool SetTemporalDistortionBandUpperLimit(T_ear ear, T_HLTemporalDistortionBandUpperLimit bandUpperLimit)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetTemporalDistortionBandUpperLimit(T_ear.LEFT, bandUpperLimit)) return false;
            return SetTemporalDistortionBandUpperLimit(T_ear.RIGHT, bandUpperLimit);
        }

        // Get actual value of band upper limit in Hz
        float bandUpperLimitHz = FromBandUpperLimitEnumToFloat(bandUpperLimit);

        // And set
        if (bandUpperLimitHz != 0.0f)
        {
            if (ear == T_ear.LEFT)
            {
                PARAM_LEFT_TA_BANDUPPERLIMIT = bandUpperLimit;
                return hlMixer.SetFloat("HL3DTI_TA_Band_Left", bandUpperLimitHz);
            }
            if (ear == T_ear.RIGHT)
            {
                PARAM_RIGHT_TA_BANDUPPERLIMIT = bandUpperLimit;
                return hlMixer.SetFloat("HL3DTI_TA_Band_Right", bandUpperLimitHz);
            }
        }
        return false;
    }

    /// <summary>
    /// Set white noise power for one ear in Temporal distortion simulator
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="whiteNoisePower (ms)"></param>
    /// <returns></returns>
    public bool SetTemporalDistortionWhiteNoisePower(T_ear ear, float whiteNoisePower)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetTemporalDistortionWhiteNoisePower(T_ear.LEFT, whiteNoisePower)) return false;
            return SetTemporalDistortionWhiteNoisePower(T_ear.RIGHT, whiteNoisePower);
        }

        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_TA_WHITENOISEPOWER = whiteNoisePower;
            return hlMixer.SetFloat("HL3DTI_TA_Noise_Power_Left", whiteNoisePower);
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_TA_WHITENOISEPOWER = whiteNoisePower;
            return hlMixer.SetFloat("HL3DTI_TA_Noise_Power_Right", whiteNoisePower);
        }
        return false;
    }

    /// <summary>
    /// Set temporal distortion bandwidth (autocorrelation filter cutoff frequency) for one ear in Temporal distortion simulator
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="bandwidth (Hz)"></param>
    /// <returns></returns>
    public bool SetTemporalDistortionBandwidth(T_ear ear, float bandwidth)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetTemporalDistortionBandwidth(T_ear.LEFT, bandwidth)) return false;
            return SetTemporalDistortionBandwidth(T_ear.RIGHT, bandwidth);
        }

        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_TA_BANDWIDTH = bandwidth;
            return hlMixer.SetFloat("HL3DTI_TA_Noise_LPF_Left", bandwidth);
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_TA_BANDWIDTH = bandwidth;
            return hlMixer.SetFloat("HL3DTI_TA_Noise_LPF_Right", bandwidth);
        }
        return false;
    }

    /// <summary>
    /// Get one float value of Hz from a T_HLTemporalDistortionBandUpperLimit enum value
    /// </summary>
    /// <param name="bandLimit"></param>
    /// <returns></returns>
    public static float FromBandUpperLimitEnumToFloat(T_HLTemporalDistortionBandUpperLimit bandLimit)
    {        
        switch (bandLimit)
        {
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_200:
                return 200.0f;                
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_400:
                return 400.0f;                
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_800:
                return 800.0f;                
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_1600:
                return 1600.0f;              
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_3200:
                return 3200.0f;                
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_6400:
                return 6400.0f;
            case T_HLTemporalDistortionBandUpperLimit.HZ_UL_12800:
                return 12800.0f;
            default:
                return 0.0f;
        }
    }

    /// <summary>
    /// Get one T_HLTemporalDistortionBandUpperLimit enum value from a float value in Hz
    /// </summary>
    /// <param name="bandLimitHz"></param>
    /// <returns></returns>
    public static T_HLTemporalDistortionBandUpperLimit FromFloatToBandUpperLimitEnum(float bandLimitHz)
    {
        if (Mathf.Abs(bandLimitHz - 200.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_200;
        if (Mathf.Abs(bandLimitHz - 400.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_400;
        if (Mathf.Abs(bandLimitHz - 800.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_800;
        if (Mathf.Abs(bandLimitHz - 1600.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_1600;
        if (Mathf.Abs(bandLimitHz - 3200.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_3200;
        if (Mathf.Abs(bandLimitHz - 6400.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_6400;
        if (Mathf.Abs(bandLimitHz - 12800.0f) < 0.01)
            return T_HLTemporalDistortionBandUpperLimit.HZ_UL_12800;

        return T_HLTemporalDistortionBandUpperLimit.HZ_UL_WRONG;
    }

    /// <summary>
    /// Get the zero and one autocorrelation coefficients of the jitter noise source for Temporal distortion in one ear.
    /// The coefficient one is normalized with respect to coefficient zero.
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="coef0"></param>
    /// <param name="coef1"></param>
    /// <returns></returns>
    public bool GetAutocorrelationCoefficients(T_ear ear, out float coef0, out float coef1)
    {
        coef0 = 0.0f;
        coef1 = 0.0f;
        if (ear == T_ear.LEFT)
        {
            if (!hlMixer.GetFloat("HL3DTI_TA_Autocor0_Get_Left", out coef0)) return false;
            if (!hlMixer.GetFloat("HL3DTI_TA_Autocor1_Get_Left", out coef1)) return false;
            coef1 = coef1 / coef0;
            return true;
        }
        if (ear == T_ear.RIGHT)
        {
            if (!hlMixer.GetFloat("HL3DTI_TA_Autocor0_Get_Right", out coef0)) return false;
            if (!hlMixer.GetFloat("HL3DTI_TA_Autocor1_Get_Right", out coef1)) return false;
            coef1 = coef1 / coef0;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Set all parameters of temporal distortion module from one of the hardcoded presets
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="preset"></param>
    /// <returns></returns>
    public bool SetTemporalDistortionFromPreset(T_ear ear, T_HLPreset preset)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetTemporalDistortionFromPreset(T_ear.LEFT, preset)) return false;
            return SetTemporalDistortionFromPreset(T_ear.RIGHT, preset);
        }

        T_HLTemporalDistortionBandUpperLimit bandUpperLimit;
        float whiteNoisePower;
        float bandWidth;
        float LRSync;
        GetTemporalDistortionPresetValues(preset, out bandUpperLimit, out whiteNoisePower, out bandWidth, out LRSync);

        if (!SetTemporalDistortionBandUpperLimit(ear, bandUpperLimit)) return false;
        if (!SetTemporalDistortionWhiteNoisePower(ear, whiteNoisePower)) return false;
        if (!SetTemporalDistortionBandwidth(ear, bandWidth)) return false;
        return SetTemporalDistortionLeftRightSynchronicity(LRSync);
    }

    /// <summary>
    /// Get all parameter values from one of the hardcoded presets for temporal distortion
    /// </summary>
    /// <param name="preset"></param>
    /// <param name="bandUpperLimit"></param>
    /// <param name="whiteNoisePower"></param>
    /// <param name="bandWidth"></param>
    /// <param name="LRSync"></param>
    public static void GetTemporalDistortionPresetValues(T_HLPreset preset, out T_HLTemporalDistortionBandUpperLimit bandUpperLimit, out float whiteNoisePower, out float bandWidth, out float LRSync)
    {
        switch (preset)
        {
            case T_HLPreset.HL_PRESET_MILD:
                bandUpperLimit = T_HLTemporalDistortionBandUpperLimit.HZ_UL_1600;
                whiteNoisePower = 0.4f;
                bandWidth = 700.0f;
                LRSync = 0.0f; 
                break;

            case T_HLPreset.HL_PRESET_MODERATE:
                bandUpperLimit = T_HLTemporalDistortionBandUpperLimit.HZ_UL_3200;
                whiteNoisePower = 0.8f;
                bandWidth = 850.0f;
                LRSync = 0.0f;
                break;

            case T_HLPreset.HL_PRESET_SEVERE:
                bandUpperLimit = T_HLTemporalDistortionBandUpperLimit.HZ_UL_12800;
                whiteNoisePower = 1.0f;
                bandWidth = 1000.0f;
                LRSync = 0.0f;
                break;

            case T_HLPreset.HL_PRESET_NORMAL:
            default:
                bandUpperLimit = T_HLTemporalDistortionBandUpperLimit.HZ_UL_1600;
                whiteNoisePower = 0.0f;
                bandWidth = 500.0f;
                LRSync = 0.0f;
                break;
        }
    }

    ///////////////////////////////////////
    // FREQUENCY SMEARING SIMULATION
    ///////////////////////////////////////

    /// <summary>
    /// Enable frequency smearing simulation for one or both ears
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool EnableFrequencySmearingSimulation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!EnableFrequencySmearingSimulation(T_ear.LEFT)) return false;
            return EnableFrequencySmearingSimulation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            TA_LEFT_ON = true;
            paramName += "FS_LeftOn";
        }
        else
        {
            TA_RIGHT_ON = true;
            paramName += "FS_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(true));
    }

    /// <summary>
    /// Disable frequency smearing simulation for one or both ears
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public bool DisableFrequencySmearingSimulation(T_ear ear)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!DisableFrequencySmearingSimulation(T_ear.LEFT)) return false;
            return DisableFrequencySmearingSimulation(T_ear.RIGHT);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_";
        if (ear == T_ear.LEFT)
        {
            TA_LEFT_ON = false;
            paramName += "FS_LeftOn";
        }
        else
        {
            TA_RIGHT_ON = false;
            paramName += "FS_RightOn";
        }

        // Send command
        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(false));
    }

    /// <summary>
    /// Set buffer size for downward section of smearing window
    /// </summary>    
    /// <param name="ear"></param>
    /// <param name="downSize"></param>
    /// <returns></returns>
    public bool SetFrequencySmearingDownwardBufferSize(T_ear ear, int downSize)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetFrequencySmearingDownwardBufferSize(T_ear.LEFT, downSize)) return false;
            return SetFrequencySmearingDownwardBufferSize(T_ear.RIGHT, downSize);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_FS_Size_Down_";
        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_FS_DOWN_SIZE = downSize;
            paramName += "Left";
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_FS_DOWN_SIZE = downSize;
            paramName += "Right";
        }

        // Send command
        return hlMixer.SetFloat(paramName, (float)downSize);
    }

    /// <summary>
    /// Set buffer size for upward section of smearing window
    /// </summary>    
    /// <param name="ear"></param>
    /// <param name="upSize"></param>
    /// <returns></returns>
    public bool SetFrequencySmearingUpwardBufferSize(T_ear ear, int upSize)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetFrequencySmearingUpwardBufferSize(T_ear.LEFT, upSize)) return false;
            return SetFrequencySmearingUpwardBufferSize(T_ear.RIGHT, upSize);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_FS_Size_Up_";
        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_FS_UP_SIZE = upSize;
            paramName += "Left";
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_FS_UP_SIZE = upSize;
            paramName += "Right";
        }

        // Send command
        return hlMixer.SetFloat(paramName, (float)upSize);
    }

    /// <summary>
    /// Set smearing amount (in Hz) for downward section of smearing window
    /// </summary>    
    /// <param name="ear"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool SetFrequencySmearingDownwardAmount_Hz(T_ear ear, float amount)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetFrequencySmearingDownwardAmount_Hz(T_ear.LEFT, amount)) return false;
            return SetFrequencySmearingDownwardAmount_Hz(T_ear.RIGHT, amount);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_FS_Hz_Down_";
        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_FS_DOWN_HZ = amount;
            paramName += "Left";
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_FS_DOWN_HZ = amount;
            paramName += "Right";
        }

        // Send command
        return hlMixer.SetFloat(paramName, amount);
    }

    /// <summary>
    /// Set smearing amount (in Hz) for upward section of smearing window
    /// </summary>    
    /// <param name="ear"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool SetFrequencySmearingUpwardAmount_Hz(T_ear ear, float amount)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetFrequencySmearingUpwardAmount_Hz(T_ear.LEFT, amount)) return false;
            return SetFrequencySmearingUpwardAmount_Hz(T_ear.RIGHT, amount);
        }

        // Set internal variables and build parameter string
        string paramName = "HL3DTI_FS_Hz_Up_";
        if (ear == T_ear.LEFT)
        {
            PARAM_LEFT_FS_UP_HZ = amount;
            paramName += "Left";
        }
        if (ear == T_ear.RIGHT)
        {
            PARAM_RIGHT_FS_UP_HZ = amount;
            paramName += "Right";
        }

        // Send command
        return hlMixer.SetFloat(paramName, amount);
    }

    /// <summary>
    /// Set all parameters of frequency smearing module from one of the hardcoded presets
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="preset"></param>
    /// <returns></returns>
    public bool SetFrequencySmearingFromPreset(T_ear ear, T_HLPreset preset)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetFrequencySmearingFromPreset(T_ear.LEFT, preset)) return false;
            return SetFrequencySmearingFromPreset(T_ear.RIGHT, preset);
        }

        int downSize, upSize;
        float downHz, upHz;
        GetFrequencySmearingPresetValues(preset, out downSize, out upSize, out downHz, out upHz);

        if (!SetFrequencySmearingDownwardBufferSize(ear, downSize)) return false;
        if (!SetFrequencySmearingUpwardBufferSize(ear, upSize)) return false;
        if (!SetFrequencySmearingDownwardAmount_Hz(ear, downHz)) return false;
        return SetFrequencySmearingUpwardAmount_Hz(ear, upHz);
    }

    /// <summary>
    /// Get all parameter values from one of the hardcoded presets for frequency smearing
    /// </summary>
    /// <param name="preset"></param>
    /// <param name="bandUpperLimit"></param>
    /// <param name="whiteNoisePower"></param>
    /// <param name="bandWidth"></param>
    /// <param name="LRSync"></param>
    public static void GetFrequencySmearingPresetValues(T_HLPreset preset, out int downSize, out int upSize, out float downHz, out float upHz)
    {
        switch (preset)
        {
            case T_HLPreset.HL_PRESET_MILD:
                downSize = 15;
                upSize = 15;
                downHz = 35.0f;
                upHz = 35.0f;
                break;

            case T_HLPreset.HL_PRESET_MODERATE:
                downSize = 100;
                upSize = 100;
                downHz = 150.0f;
                upHz = 150.0f;
                break;

            case T_HLPreset.HL_PRESET_SEVERE:
                downSize = 150;
                upSize = 150;
                downHz = 650.0f;
                upHz = 650.0f;
                break;

            case T_HLPreset.HL_PRESET_NORMAL:
            default:
                downSize = 15;
                upSize = 15;
                downHz = 0.0f;
                upHz = 0.0f;
                break;
        }
    }

    /////////////////////////////////////////////////////////////////////
    // AUXILIARY FUNCTIONS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Get char with the letter corresponding to one curve of HL Classification Scale
    /// </summary>
    /// <param name="curve"></param>
    /// <returns></returns>
    public static char FromClassificationScaleCurveToChar (T_HLClassificationScaleCurve curve)
    {
        char result = ' ';
        switch (curve)
        {
            case T_HLClassificationScaleCurve.HL_CS_A:
                result = 'A';
                break;
            case T_HLClassificationScaleCurve.HL_CS_B:
                result = 'B';
                break;
            case T_HLClassificationScaleCurve.HL_CS_C:
                result = 'C';
                break;
            case T_HLClassificationScaleCurve.HL_CS_D:
                result = 'D';
                break;
            case T_HLClassificationScaleCurve.HL_CS_E:
                result = 'E';
                break;
            case T_HLClassificationScaleCurve.HL_CS_F:
                result = 'F';
                break;
            case T_HLClassificationScaleCurve.HL_CS_G:
                result = 'G';
                break;
            case T_HLClassificationScaleCurve. HL_CS_H:
                result = 'H';
                break;
            case T_HLClassificationScaleCurve.HL_CS_I:
                result = 'I';
                break;
            case T_HLClassificationScaleCurve.HL_CS_J:
                result = 'J';
                break;
            case T_HLClassificationScaleCurve.HL_CS_K:
                result = 'K';
                break;
            default:
                result = ' ';
                break;
        }
        return result;
    }

    /// <summary>
    /// Get string with the letter and description of one curve of HL Classification Scale
    /// </summary>
    /// <param name="curve"></param>
    /// <returns></returns>
    public static string FromClassificationScaleCurveToString(T_HLClassificationScaleCurve curve)
    {
        string result = "";
        switch (curve)
        {
            case T_HLClassificationScaleCurve.HL_CS_NOLOSS:
                result = "No hearing loss";
                break;
            case T_HLClassificationScaleCurve.HL_CS_A:
                result = "A (Loss only on frequencies starting from 4000Hz and above)";
                break;
            case T_HLClassificationScaleCurve.HL_CS_B:
                result = "B (Loss only on frequencies starting from 2000Hz and above)";
                break;
            case T_HLClassificationScaleCurve.HL_CS_C:
                result = "C (Loss only on frequencies starting from 1000Hz and above)";
                break;
            case T_HLClassificationScaleCurve.HL_CS_D:
                result = "D (Loss only on frequencies starting from 500Hz and above)";
                break;
            case T_HLClassificationScaleCurve.HL_CS_E:
                result = "E (Loss only on frequencies starting from 250Hz and above)";
                break;
            case T_HLClassificationScaleCurve.HL_CS_F:
                result = "F (Peak loss at 250Hz)";
                break;
            case T_HLClassificationScaleCurve.HL_CS_G:
                result = "G (Peak loss at 500Hz)";
                break;
            case T_HLClassificationScaleCurve.HL_CS_H:
                result = "H (Peak loss at 1000Hz)";
                break;
            case T_HLClassificationScaleCurve.HL_CS_I:
                result = "I (Peak loss at 2000Hz)";
                break;
            case T_HLClassificationScaleCurve.HL_CS_J:
                result = "J (Peak loss at 4000Hz)";
                break;
            case T_HLClassificationScaleCurve.HL_CS_K:
                result = "K (Constant Slope)";
                break;
            default:
                result = "Unknown curve!";
                break;
        }
        return result;
    }

    /// <summary>
    /// Get string with the name of one severity of HL Classification Scale
    /// </summary>
    /// <param name="severity"></param>
    /// <returns></returns>
    public static string FromClassificationScaleSeverityToString(T_HLClassificationScaleSeverity severity)
    {
        string result = "";
        switch (severity)
        {
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_NOLOSS:
                result = "No loss";
                break;
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MILD:
                result = "Mild";
                break;
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MILDMODERATE:
                result = "Mild-moderate";
                break;
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MODERATE:
                result = "Moderate";
                break;
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MODERATESEVERE:
                result = "Moderate-severe";
                break;
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_SEVERE:
                result = "Severe";
                break;
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_PROFOUND:
                result = "Profound";
                break;
            default:
                result = "Unknown severity!";
                break;
        }
        return result;
    }

    /// <summary>
    /// Get float value which codes one curve of the HL Classification Scale
    /// </summary>
    /// <param name="curve"></param>
    /// <returns></returns>
    public static float FromClassificationScaleCurveToFloat(T_HLClassificationScaleCurve curve)
    {
        float result = (float)(int)curve;
        return result;
    }

    /// <summary>
    /// Get int value which codes one severity of the HL Classification Scale
    /// </summary>
    /// <param name="severity"></param>
    /// <returns></returns>
    public static int FromClassificationScaleSeverityToInt(T_HLClassificationScaleSeverity severity)
    {
        return (int)severity;
    }

    /// <summary>
    /// Private method to get the HL value for one slope of the HL Classification scale
    /// </summary>
    /// <param name="slope"></param>
    /// <returns></returns>
    static float GetHLForSlope(int slope)
    {
        switch (slope)
        {
            case 0: return 0; 
            case 1: return 10;
            case 2: return 20;
            case 3: return 30;
            case 4: return 40;
            case 5: return 50;
            case 6: return 60;  
        }
        return 0;
    }

    /// <summary>
    /// Private method to get the HL value for one severity of the HL Classification scale
    /// </summary>
    /// <param name="severity"></param>
    /// <returns></returns>
    static float GetHLForSeverity(T_HLClassificationScaleSeverity severity)
    {
        switch (severity)
        {
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_NOLOSS: return 0; 
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MILD: return 21;
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MILDMODERATE: return 33;
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MODERATE: return 48;
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MODERATESEVERE: return 63;
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_SEVERE: return 81;
            case T_HLClassificationScaleSeverity.HL_CS_SEVERITY_PROFOUND: return 100;     
        }
        return 0;
    }

    /// <summary>
    /// Get all hearing loss values (in dBHL) for one curve, slope and severity of HL Classification scale
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="slope"></param>
    /// <param name="hl"></param>
    static public void GetClassificationScaleHL(T_HLClassificationScaleCurve curve, int slope, T_HLClassificationScaleSeverity severity, out List<float> hl)
    {
        float x = GetHLForSlope(slope);        
        hl = new List<float>();

        // Apply curve and slope
        switch (curve)
        {
            case T_HLClassificationScaleCurve.HL_CS_A: hl.Add(0); hl.Add(0); hl.Add(0);     hl.Add(0);         hl.Add(0);          hl.Add(0);          hl.Add(x / 2);      hl.Add(x);       hl.Add(x); break;
            case T_HLClassificationScaleCurve.HL_CS_B: hl.Add(0); hl.Add(0); hl.Add(0);     hl.Add(0);         hl.Add(0);          hl.Add(x / 2);      hl.Add(x);          hl.Add(x);       hl.Add(x); break;
            case T_HLClassificationScaleCurve.HL_CS_C: hl.Add(0); hl.Add(0); hl.Add(0);     hl.Add(0);         hl.Add(x / 2);      hl.Add(x);          hl.Add(x);          hl.Add(x);       hl.Add(x); break;
            case T_HLClassificationScaleCurve.HL_CS_D: hl.Add(0); hl.Add(0); hl.Add(0);     hl.Add(x / 2);     hl.Add(x);          hl.Add(x);          hl.Add(x);          hl.Add(x);       hl.Add(x); break;
            case T_HLClassificationScaleCurve.HL_CS_E: hl.Add(0); hl.Add(0); hl.Add(x / 2); hl.Add(x);         hl.Add(x);          hl.Add(x);          hl.Add(x);          hl.Add(x);       hl.Add(x); break;
            case T_HLClassificationScaleCurve.HL_CS_F: hl.Add(0); hl.Add(0); hl.Add(x);     hl.Add(x / 2);     hl.Add(x / 2);      hl.Add(x / 2);      hl.Add(x / 2);      hl.Add(x / 2);   hl.Add(x / 2); break;
            case T_HLClassificationScaleCurve.HL_CS_G: hl.Add(0); hl.Add(0); hl.Add(x / 2); hl.Add(x);         hl.Add(x / 2);      hl.Add(x / 2);      hl.Add(x / 2);      hl.Add(x / 2);   hl.Add(x / 2); break;
            case T_HLClassificationScaleCurve.HL_CS_H: hl.Add(0); hl.Add(0); hl.Add(0);     hl.Add(x / 2);     hl.Add(x);          hl.Add(x / 2);      hl.Add(x / 2);      hl.Add(x / 2);   hl.Add(x / 2); break;
            case T_HLClassificationScaleCurve.HL_CS_I: hl.Add(0); hl.Add(0); hl.Add(0);     hl.Add(0);         hl.Add(x / 2);      hl.Add(x);          hl.Add(x / 2);      hl.Add(x / 2);   hl.Add(x / 2); break;
            case T_HLClassificationScaleCurve.HL_CS_J: hl.Add(0); hl.Add(0); hl.Add(0);     hl.Add(0);         hl.Add(0);          hl.Add(x / 2);      hl.Add(x);          hl.Add(x / 2);   hl.Add(x / 2); break;
            case T_HLClassificationScaleCurve.HL_CS_K: hl.Add(0); hl.Add(0); hl.Add(x / 6); hl.Add(2 * x / 6); hl.Add(3 * x / 6);  hl.Add(4 * x / 6);  hl.Add(5 * x / 6);  hl.Add(x);       hl.Add(x); break;
            default: hl.Add(0); hl.Add(0); hl.Add(0); hl.Add(0); hl.Add(0); hl.Add(0); hl.Add(0); hl.Add(0); hl.Add(0); break;
        }

        // Apply severity
        for (int c = 0; c < hl.Count; c++)
            hl[c] += GetHLForSeverity(severity); 
    }

    //public float GetSomething(string what)
    //{
    //    float value;
    //    if (!hlMixer.GetFloat(what, out value))
    //        return -1.0f;
    //    else
    //        return value;
    //}
}
