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
    // Public constant and type definitions 
    public static readonly ReadOnlyCollection<float> EQ_PRESET_MILD = new ReadOnlyCollection<float>(new[] { -7f, -7f, -12f, -15f, -22f, -25f, -25f, -25f, -25f });
    public static readonly ReadOnlyCollection<float> EQ_PRESET_MODERATE = new ReadOnlyCollection<float>(new[] { -22f, -22f, -27f, -30f, -37f, -40f, -40f, -40f, -40f });
    public static readonly ReadOnlyCollection<float> EQ_PRESET_SEVERE = new ReadOnlyCollection<float>(new[] { -47f, -47f, -52f, -55f, -62f, -65f, -65f, -65f, -65f });
    public static readonly ReadOnlyCollection<float> EQ_PRESET_PLAIN = new ReadOnlyCollection<float>(new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f });
    public enum T_HLEffect { EFFECT_EQ = 0, EFFECT_COMPRESSOR = 1, EFFECT_HEARINGLOSS = 2 };
    public enum T_HLProcessChain { PROCESS_EQ_FIRST = 0, PROCESS_COMPRESSOR_FIRST = 1 };

    // Internal constants
    const int DEFAULT_COMPRESSOR_RATIO = 1;
    const float DEFAULT_COMPRESSOR_THRESHOLD = 0.0f;
    const float DEFAULT_COMPRESSOR_ATTACK = 20.0f;
    const float DEFAULT_COMPRESSOR_RELEASE = 100.0f;

    // Global variables
    public AudioMixer hlMixer;  // Drag&drop here the HAHL_3DTI_Mixer

    // Internal parameters for consistency with GUI
    public bool GLOBAL_LEFT_ON = false;
    public bool GLOBAL_RIGHT_ON = false;
    public float [] PARAM_BANDS_DB_LEFT = new float[9] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    public float [] PARAM_BANDS_DB_RIGHT = new float[9] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    public bool PARAM_LEFT_EQ_ON = true;
	public bool PARAM_RIGHT_EQ_ON = true;
    public bool PARAM_LEFT_COMPRESSOR_ON = false;
    public bool PARAM_RIGHT_COMPRESSOR_ON = false;
    public bool PARAM_COMPRESSOR_FIRST = true;
    public float PARAM_COMP_LEFT_RATIO = DEFAULT_COMPRESSOR_RATIO;
	public float PARAM_COMP_LEFT_THRESHOLD = DEFAULT_COMPRESSOR_THRESHOLD;
    public float PARAM_COMP_RIGHT_RATIO = DEFAULT_COMPRESSOR_RATIO;
    public float PARAM_COMP_RIGHT_THRESHOLD = DEFAULT_COMPRESSOR_THRESHOLD;
    public float PARAM_COMP_LEFT_ATTACK = DEFAULT_COMPRESSOR_ATTACK;
	public float PARAM_COMP_LEFT_RELEASE = DEFAULT_COMPRESSOR_RELEASE;
    public float PARAM_COMP_RIGHT_ATTACK = DEFAULT_COMPRESSOR_ATTACK;
    public float PARAM_COMP_RIGHT_RELEASE = DEFAULT_COMPRESSOR_RELEASE;		

    /// <summary>
    /// Set equalizer preset for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="preset ({EQ_PRESET_MILD, EQ_PRESET_MODERATE, EQ_PRESET_SEVERE, EQ_PRESET_PLAIN})"></param>
    /// <returns></returns>
    public bool SetEQPreset(T_ear ear, ReadOnlyCollection<float> presetGains)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetEQPreset(T_ear.LEFT, presetGains)) return false;
            return SetEQPreset(T_ear.RIGHT, presetGains);
        }

        T_LevelsList gains = (T_LevelsList)new List<float>(presetGains); 
        return SetEQGains(ear, gains);
    }

    /// <summary>
    /// Switch on/off one effect in the chain 
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="whichEffect ({EFFECT_EQ, EFFECT_COMPRESSOR, EFFECT_HEARINGLOSS})"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchOnOffEffect(T_ear ear, T_HLEffect whichEffect, bool value, bool changeParam=true)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SwitchOnOffEffect(T_ear.LEFT, whichEffect, value, changeParam)) return false;
            return SwitchOnOffEffect(T_ear.RIGHT, whichEffect, value, changeParam);
        }

        // Global switch for all hearing loss
        if (whichEffect == T_HLEffect.EFFECT_HEARINGLOSS)
        {
            if (ear == T_ear.LEFT)
            {
                GLOBAL_LEFT_ON = value;
                if (value)
                {
                    if (!SwitchOnOffEffect(ear, T_HLEffect.EFFECT_EQ, PARAM_LEFT_EQ_ON, false)) return false;
                    return SwitchOnOffEffect(ear, T_HLEffect.EFFECT_COMPRESSOR, PARAM_LEFT_COMPRESSOR_ON, false);
                }
                else
                {
                    if (!SwitchOnOffEffect(ear, T_HLEffect.EFFECT_EQ, false, false)) return false;
                    return SwitchOnOffEffect(ear, T_HLEffect.EFFECT_COMPRESSOR, false, false);
                }
            }
            else
            {
                GLOBAL_RIGHT_ON = value;
                if (value)
                {
                    if (!SwitchOnOffEffect(ear, T_HLEffect.EFFECT_EQ, PARAM_RIGHT_EQ_ON, false)) return false;
                    return SwitchOnOffEffect(ear, T_HLEffect.EFFECT_COMPRESSOR, PARAM_RIGHT_COMPRESSOR_ON, false);
                }
                else
                {
                    if (!SwitchOnOffEffect(ear, T_HLEffect.EFFECT_EQ, false, false)) return false;
                    return SwitchOnOffEffect(ear, T_HLEffect.EFFECT_COMPRESSOR, false, false);
                }
            }
        }

        // Switch for EQ or Compressor
        //string paramName = "HL3DTI_";
        //switch (whichEffect)
        //{
        //    case EFFECT_EQ:                       
        //        paramName += "EQ";
        //        break;
        //    case EFFECT_COMPRESSOR: paramName += "Comp"; break;
        //    default: return false;
        //}
        //if (ear == T_ear.LEFT)
        //    paramName += "LeftOn";
        //else
        //    paramName += "RightOn";               
        string paramName = "HL3DTI_";
        switch (whichEffect)
        {
            case T_HLEffect.EFFECT_EQ:
                if (ear == T_ear.LEFT)
                {
                    if (changeParam) PARAM_LEFT_EQ_ON = value;
                    paramName += "EQLeftOn";
                }
                else
                {
                    if (changeParam) PARAM_RIGHT_EQ_ON = value;
                    paramName += "EQRightOn";
                }
                break;
            case T_HLEffect.EFFECT_COMPRESSOR:
                if (ear == T_ear.LEFT)
                {
                    if (changeParam) PARAM_LEFT_COMPRESSOR_ON = value;
                    paramName += "CompLeftOn";
                }
                else
                {
                    if (changeParam) PARAM_RIGHT_COMPRESSOR_ON = value;
                    paramName += "CompRightOn";
                }
                break;
            default: return false;
        }

        return hlMixer.SetFloat(paramName, CommonFunctions.Bool2Float(value));
    }

    /// <summary>
    /// Get on/off state of each effect process for each ear
    /// </summary>
    /// <param name="eqLeft"></param>
    /// <param name="eqRight"></param>
    /// <param name="compressorLeft"></param>
    /// <param name="compressorRight"></param>
    /// <returns></returns>
    public bool GetEffectSwitches(out bool eqLeft, out bool eqRight, out bool compressorLeft, out bool compressorRight)
    {
        eqLeft = eqRight = compressorLeft = compressorRight = false;
        float floatValue;

        if (!hlMixer.GetFloat("HL3DTI_EQLeftOn", out floatValue)) return false;
        eqLeft = CommonFunctions.Float2Bool(floatValue);
        PARAM_LEFT_EQ_ON = eqLeft;
        if (!hlMixer.GetFloat("HL3DTI_EQRightOn", out floatValue)) return false;
        eqRight = CommonFunctions.Float2Bool(floatValue);
        PARAM_RIGHT_EQ_ON = eqRight;
        if (!hlMixer.GetFloat("HL3DTI_CompLeftOn", out floatValue)) return false;
        compressorLeft = CommonFunctions.Float2Bool(floatValue);
        PARAM_LEFT_COMPRESSOR_ON = compressorLeft;
        if (!hlMixer.GetFloat("HL3DTI_CompRightOn", out floatValue)) return false;
        compressorRight= CommonFunctions.Float2Bool(floatValue);
        PARAM_RIGHT_COMPRESSOR_ON = compressorRight;

        return true;
    }

    //////////////////////////////////////////////////////////////
    // ADVANCED API
    //////////////////////////////////////////////////////////////

    /// <summary>
    /// Set all equalizer band gains for one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="gains (dB[])"></param>
    /// <returns></returns>
    public bool SetEQGains(T_ear ear, T_LevelsList gains)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetEQGains(T_ear.LEFT, gains)) return false;
            return SetEQGains(T_ear.RIGHT, gains);
        }

        for (int b=0; b < gains.Count; b++)
        {
            if (!SetEQGain(ear, b, gains[b])) return false;
        }

        //if (ear == T_ear.LEFT)
        //{
        //    if (!hlMixer.SetFloat("HL3DTI_EQL0", gains[0])) return false;
        //    PARAM_BANDS_DB_LEFT[0] = gains[0];            
        //    if (!hlMixer.SetFloat("HL3DTI_EQL1", gains[1])) return false;
        //    PARAM_BANDS_DB_LEFT[1] = gains[1];
        //    if (!hlMixer.SetFloat("HL3DTI_EQL2", gains[2])) return false;
        //    PARAM_BANDS_DB_LEFT[2] = gains[2];
        //    if (!hlMixer.SetFloat("HL3DTI_EQL3", gains[3])) return false;
        //    PARAM_BANDS_DB_LEFT[3] = gains[3];
        //    if (!hlMixer.SetFloat("HL3DTI_EQL4", gains[4])) return false;
        //    PARAM_BANDS_DB_LEFT[4] = gains[4];
        //    if (!hlMixer.SetFloat("HL3DTI_EQL5", gains[5])) return false;
        //    PARAM_BANDS_DB_LEFT[5] = gains[5];
        //    if (!hlMixer.SetFloat("HL3DTI_EQL6", gains[6])) return false;
        //    PARAM_BANDS_DB_LEFT[6] = gains[6];
        //    if (!hlMixer.SetFloat("HL3DTI_EQL7", gains[7])) return false;
        //    PARAM_BANDS_DB_LEFT[7] = gains[7];
        //    if (!hlMixer.SetFloat("HL3DTI_EQL8", gains[8])) return false;
        //    PARAM_BANDS_DB_LEFT[8] = gains[8];
        //}
        //if (ear == T_ear.RIGHT)
        //{
        //    if (!hlMixer.SetFloat("HL3DTI_EQR0", gains[0])) return false;
        //    PARAM_BANDS_DB_RIGHT[0] = gains[0];
        //    if (!hlMixer.SetFloat("HL3DTI_EQR1", gains[1])) return false;
        //    PARAM_BANDS_DB_RIGHT[1] = gains[1];
        //    if (!hlMixer.SetFloat("HL3DTI_EQR2", gains[2])) return false;
        //    PARAM_BANDS_DB_RIGHT[2] = gains[2];
        //    if (!hlMixer.SetFloat("HL3DTI_EQR3", gains[3])) return false;
        //    PARAM_BANDS_DB_RIGHT[3] = gains[3];
        //    if (!hlMixer.SetFloat("HL3DTI_EQR4", gains[4])) return false;
        //    PARAM_BANDS_DB_RIGHT[4] = gains[4];
        //    if (!hlMixer.SetFloat("HL3DTI_EQR5", gains[5])) return false;
        //    PARAM_BANDS_DB_RIGHT[5] = gains[5];
        //    if (!hlMixer.SetFloat("HL3DTI_EQR6", gains[6])) return false;
        //    PARAM_BANDS_DB_RIGHT[6] = gains[6];
        //    if (!hlMixer.SetFloat("HL3DTI_EQR7", gains[7])) return false;
        //    PARAM_BANDS_DB_RIGHT[7] = gains[7];
        //    if (!hlMixer.SetFloat("HL3DTI_EQR8", gains[8])) return false;
        //    PARAM_BANDS_DB_RIGHT[8] = gains[8];
        //}
        return true;
    }

    /// <summary>
    /// Set gain for one band of the EQ in one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="band"></param>
    /// <param name="gain (dB)"></param>
    /// <returns></returns>
    public bool SetEQGain(T_ear ear, int band, float gain)
    {
        // Both ears
        if (ear == T_ear.BOTH)
        {
            if (!SetEQGain(T_ear.LEFT, band, gain)) return false;
            return SetEQGain(T_ear.RIGHT, band, gain);
        }

        // Build string
        string paramName = "HL3DTI_EQ";
        if (ear == T_ear.LEFT)
            paramName += "L";
        else
            paramName += "R";
        paramName += band.ToString();

        // Update internal parameters
        if (ear == T_ear.LEFT)
            PARAM_BANDS_DB_LEFT[band] = gain;
        else
            PARAM_BANDS_DB_RIGHT[band] = gain;

        // Send command
        return hlMixer.SetFloat(paramName, gain);
    }

    /// <summary>
    /// Set default values for all parameters of the compressor in one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <returns></returns>
    public bool SetDefaultCompressor(T_ear ear)
    {
        SetCompressorRatio(ear, DEFAULT_COMPRESSOR_RATIO);
        SetCompressorThreshold(ear, DEFAULT_COMPRESSOR_THRESHOLD);
        SetCompressorAttack(ear, DEFAULT_COMPRESSOR_ATTACK);
        SetCompressorRelease(ear, DEFAULT_COMPRESSOR_RELEASE);

        //if (ear == T_ear.BOTH)
        //{
        //    if (!SetDefaultCompressor(T_ear.LEFT)) return false;
        //    return SetDefaultCompressor(T_ear.RIGHT);
        //}
        
        //if (ear == T_ear.LEFT)
        //{            
        //    if (!hlMixer.ClearFloat("HL3DTI_LeftRatio")) return false;
        //    if (!hlMixer.ClearFloat("HL3DTI_LeftThreshold")) return false;
        //    if (!hlMixer.ClearFloat("HL3DTI_LeftAttack")) return false;
        //    if (!hlMixer.ClearFloat("HL3DTI_LeftRelease")) return false;
        //}
        //else
        //{         
        //    if (!hlMixer.ClearFloat("HL3DTI_RightRatio")) return false;
        //    if (!hlMixer.ClearFloat("HL3DTI_RightThreshold")) return false;
        //    if (!hlMixer.ClearFloat("HL3DTI_RightAttack")) return false;
        //    if (!hlMixer.ClearFloat("HL3DTI_RightRelease")) return false;
        //}
        return true;
    }

    /// <summary>
    /// Set ratio of compressor for one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="ratio"></param>
    /// <returns></returns>
    public bool SetCompressorRatio(T_ear ear, int ratio)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetCompressorRatio(T_ear.LEFT, ratio)) return false;
            return SetCompressorRatio(T_ear.RIGHT, ratio);
        }
        if (ear == T_ear.LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_LeftRatio", (float)ratio)) return false;
            PARAM_COMP_LEFT_RATIO = (float)ratio;
        }
        if (ear == T_ear.RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_RightRatio", (float)ratio)) return false;
            PARAM_COMP_RIGHT_RATIO = (float)ratio;
        }
        return true;
    }

    /// <summary>
    /// Set threshold of compressor for one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="threshold (dB)"></param>
    /// <returns></returns>
    public bool SetCompressorThreshold(T_ear ear, float threshold)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetCompressorThreshold(T_ear.LEFT, threshold)) return false;
            return SetCompressorThreshold(T_ear.RIGHT, threshold);
        }
        if (ear == T_ear.LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_LeftThreshold", threshold)) return false;
            PARAM_COMP_LEFT_THRESHOLD = threshold;
        }
        if (ear == T_ear.RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_RightThreshold", threshold)) return false;
            PARAM_COMP_RIGHT_THRESHOLD = threshold;
        }
        return true;
    }

    /// <summary>
    /// Set attack of compressor for one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="attack (ms)"></param>
    /// <returns></returns>
    public bool SetCompressorAttack(T_ear ear, float attack)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetCompressorAttack(T_ear.LEFT, attack)) return false;
            return SetCompressorAttack(T_ear.RIGHT, attack);
        }
        if (ear == T_ear.LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_LeftAttack", attack)) return false;
            PARAM_COMP_LEFT_ATTACK = attack;
        }
        if (ear == T_ear.RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_RightAttack", attack)) return false;
            PARAM_COMP_RIGHT_ATTACK = attack;
        }
        return true;
    }

    /// <summary>
    /// Set release of compressor for one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="release (ms)"></param>
    /// <returns></returns>
    public bool SetCompressorRelease(T_ear ear, float release)
    {
        if (ear == T_ear.BOTH)
        {
            if (!SetCompressorRelease(T_ear.LEFT, release)) return false;
            return SetCompressorRelease(T_ear.RIGHT, release);
        }
        if (ear == T_ear.LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_LeftRelease", release)) return false;
            PARAM_COMP_LEFT_RELEASE = release;
        }
        if (ear == T_ear.RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_RightRelease", release)) return false;
            PARAM_COMP_RIGHT_RELEASE = release;
        }
        return true;
    }

    /// <summary>
    /// Set order in which the EQ and compressor processes are chained together
    /// </summary>
    /// <param name="whichGoesFirst ({PROCESS_EQ_FIRST, PROCESS_COMPRESSOR_FIRST})"></param>
    /// <returns></returns>
    public bool SetProcessChain(T_HLProcessChain whichGoesFirst)
    {
        if (whichGoesFirst == T_HLProcessChain.PROCESS_EQ_FIRST)
        {
            if (!hlMixer.SetFloat("HL3DTI_CompFirst", 0.0f)) return false;
            PARAM_COMPRESSOR_FIRST = false;
        }
        else
        {
            if (!hlMixer.SetFloat("HL3DTI_CompFirst", 1.0f)) return false;
            PARAM_COMPRESSOR_FIRST = true;
        }
        return true;
    }
}
