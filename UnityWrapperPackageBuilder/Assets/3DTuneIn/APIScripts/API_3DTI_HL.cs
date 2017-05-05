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

public class API_3DTI_HL : MonoBehaviour
{
    // Constant definitions ( TO DO: read them from the GUI dll)
    public const int EAR_LEFT = 1;
    public const int EAR_RIGHT = 0;
    public const int EAR_BOTH = 2;
    public static readonly ReadOnlyCollection<float> EQ_PRESET_MILD = new ReadOnlyCollection<float>(new[] { -7f, -7f, -12f, -15f, -22f, -25f, -25f, -25f, -25f });
    public static readonly ReadOnlyCollection<float> EQ_PRESET_MODERATE = new ReadOnlyCollection<float>(new[] { -22f, -22f, -27f, -30f, -37f, -40f, -40f, -40f, -40f });
    public static readonly ReadOnlyCollection<float> EQ_PRESET_SEVERE = new ReadOnlyCollection<float>(new[] { -47f, -47f, -52f, -55f, -62f, -65f, -65f, -65f, -65f });
    public static readonly ReadOnlyCollection<float> EQ_PRESET_PLAIN = new ReadOnlyCollection<float>(new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f });
    public const int EFFECT_EQ = 0;
    public const int EFFECT_COMPRESSOR = 1;
    public const int EFFECT_HEARINGLOSS = 2;
    public const int PROCESS_EQ_FIRST = 0;
    public const int PROCESS_COMPRESSOR_FIRST = 1;

    // Global variables
    public AudioMixer hlMixer;  // Drag&drop here the HAHL_3DTI_Mixer


    /// <summary>
    /// Set equalizer preset for one ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="preset ({EQ_PRESET_MILD, EQ_PRESET_MODERATE, EQ_PRESET_SEVERE, EQ_PRESET_PLAIN})"></param>
    /// <returns></returns>
    public bool SetEQPreset(int ear, ReadOnlyCollection<float> presetGains)
    {
        // Both ears
        if (ear == EAR_BOTH)
        {
            if (!SetEQPreset(EAR_LEFT, presetGains)) return false;
            return SetEQPreset(EAR_RIGHT, presetGains);
        }

        List<float> gains = new List<float>(presetGains);
        return SetEQGains(ear, gains);
    }

    /// <summary>
    /// Switch on/off one effect in the chain 
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="whichEffect ({EFFECT_EQ, EFFECT_COMPRESSOR, EFFECT_HEARINGLOSS})"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchOnOffEffect(int ear, int whichEffect, bool value)
    {
        // Both ears
        if (ear == EAR_BOTH)
        {
            if (!SwitchOnOffEffect(EAR_LEFT, whichEffect, value)) return false;
            return SwitchOnOffEffect(EAR_RIGHT, whichEffect, value);
        }

        // Global switch for all hearing loss
        if (whichEffect == EFFECT_HEARINGLOSS)
        {
            if (!SwitchOnOffEffect(ear, EFFECT_EQ, value)) return false;
            return SwitchOnOffEffect(ear, EFFECT_COMPRESSOR, value);
        }

        // Switch for EQ or Compressor
        string paramName = "HL3DTI_";
        switch (whichEffect)
        {
            case EFFECT_EQ: paramName += "EQ"; break;
            case EFFECT_COMPRESSOR: paramName += "Comp"; break;
            default: return false;
        }
        if (ear == EAR_LEFT)
            paramName += "LeftOn";
        else
            paramName += "RightOn";

        if (value)
            return hlMixer.SetFloat(paramName, 1.0f);
        else
            return hlMixer.SetFloat(paramName, 0.0f);
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
        eqLeft = Float2Bool(floatValue);
        if (!hlMixer.GetFloat("HL3DTI_EQRightOn", out floatValue)) return false;
        eqRight = Float2Bool(floatValue);
        if (!hlMixer.GetFloat("HL3DTI_CompLeftOn", out floatValue)) return false;
        compressorLeft = Float2Bool(floatValue);
        if (!hlMixer.GetFloat("HL3DTI_CompRightOn", out floatValue)) return false;
        compressorRight= Float2Bool(floatValue);
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
    public bool SetEQGains(int ear, List<float> gains)
    {
        if (ear == EAR_BOTH)
        {
            if (!SetEQGains(EAR_LEFT, gains)) return false;
            return SetEQGains(EAR_RIGHT, gains);
        }
        if (ear == EAR_LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_EQL0", gains[0])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQL1", gains[1])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQL2", gains[2])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQL3", gains[3])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQL4", gains[4])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQL5", gains[5])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQL6", gains[6])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQL7", gains[7])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQL8", gains[8])) return false;
        }
        if (ear == EAR_RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_EQR0", gains[0])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQR1", gains[1])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQR2", gains[2])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQR3", gains[3])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQR4", gains[4])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQR5", gains[5])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQR6", gains[6])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQR7", gains[7])) return false;
            if (!hlMixer.SetFloat("HL3DTI_EQR8", gains[8])) return false;
        }
        return true;
    }

    /// <summary>
    /// Set gain for one band of the EQ in one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="band"></param>
    /// <param name="gain (dB)"></param>
    /// <returns></returns>
    public bool SetEQGain(int ear, int band, float gain)
    {
        // Both ears
        if (ear == EAR_BOTH)
        {
            if (!SetEQGain(EAR_LEFT, band, gain)) return false;
            return SetEQGain(EAR_RIGHT, band, gain);
        }

        string paramName = "HL3DTI_EQ";
        if (ear == EAR_LEFT)
            paramName += "L";
        else
            paramName += "R";
        paramName += band.ToString();

        return hlMixer.SetFloat(paramName, gain);
    }

    /// <summary>
    /// Set default values for all parameters of the compressor in one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <returns></returns>
    public bool SetDefaultCompressor(int ear)
    {
        if (ear == EAR_BOTH)
        {
            if (!SetDefaultCompressor(EAR_LEFT)) return false;
            return SetDefaultCompressor(EAR_RIGHT);
        }
        if (ear == EAR_LEFT)
        {            
            if (!hlMixer.ClearFloat("HL3DTI_LeftRatio")) return false;
            if (!hlMixer.ClearFloat("HL3DTI_LeftThreshold")) return false;
            if (!hlMixer.ClearFloat("HL3DTI_LeftAttack")) return false;
            if (!hlMixer.ClearFloat("HL3DTI_LeftRelease")) return false;
        }
        else
        {         
            if (!hlMixer.ClearFloat("HL3DTI_RightRatio")) return false;
            if (!hlMixer.ClearFloat("HL3DTI_RightThreshold")) return false;
            if (!hlMixer.ClearFloat("HL3DTI_RightAttack")) return false;
            if (!hlMixer.ClearFloat("HL3DTI_RightRelease")) return false;
        }
        return true;
    }

    /// <summary>
    /// Set ratio of compressor for one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="ratio"></param>
    /// <returns></returns>
    public bool SetCompressorRatio(int ear, int ratio)
    {
        if (ear == EAR_BOTH)
        {
            if (!SetCompressorRatio(EAR_LEFT, ratio)) return false;
            return SetCompressorRatio(EAR_RIGHT, ratio);
        }
        if (ear == EAR_LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_LeftRatio", (float)ratio)) return false;
        }
        if (ear == EAR_RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_RightRatio", (float)ratio)) return false;
        }
        return true;
    }

    /// <summary>
    /// Set threshold of compressor for one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="threshold (dB)"></param>
    /// <returns></returns>
    public bool SetCompressorThreshold(int ear, float threshold)
    {
        if (ear == EAR_BOTH)
        {
            if (!SetCompressorThreshold(EAR_LEFT, threshold)) return false;
            return SetCompressorThreshold(EAR_RIGHT, threshold);
        }
        if (ear == EAR_LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_LeftThreshold", threshold)) return false;
        }
        if (ear == EAR_RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_RightThreshold", threshold)) return false;
        }
        return true;
    }

    /// <summary>
    /// Set attack of compressor for one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="attack (ms)"></param>
    /// <returns></returns>
    public bool SetCompressorAttack(int ear, float attack)
    {
        if (ear == EAR_BOTH)
        {
            if (!SetCompressorAttack(EAR_LEFT, attack)) return false;
            return SetCompressorAttack(EAR_RIGHT, attack);
        }
        if (ear == EAR_LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_LeftAttack", attack)) return false;
        }
        if (ear == EAR_RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_RightAttack", attack)) return false;
        }
        return true;
    }

    /// <summary>
    /// Set release of compressor for one ear
    /// </summary>
    /// <param name="ear ({EAR_LEFT, EAR_RIGHT})"></param>
    /// <param name="release (ms)"></param>
    /// <returns></returns>
    public bool SetCompressorRelease(int ear, float release)
    {
        if (ear == EAR_BOTH)
        {
            if (!SetCompressorRelease(EAR_LEFT, release)) return false;
            return SetCompressorRelease(EAR_RIGHT, release);
        }
        if (ear == EAR_LEFT)
        {
            if (!hlMixer.SetFloat("HL3DTI_LeftRelease", release)) return false;
        }
        if (ear == EAR_RIGHT)
        {
            if (!hlMixer.SetFloat("HL3DTI_RightRelease", release)) return false;
        }
        return true;
    }

    /// <summary>
    /// Set order in which the EQ and compressor processes are chained together
    /// </summary>
    /// <param name="whichGoesFirst ({PROCESS_EQ_FIRST, PROCESS_COMPRESSOR_FIRST})"></param>
    /// <returns></returns>
    public bool SetProcessChain(int whichGoesFirst)
    {
        if (whichGoesFirst == PROCESS_EQ_FIRST)
        {
            if (!hlMixer.SetFloat("HL3DTI_CompFirst", 0.0f)) return false;
        }
        else
        {
            if (!hlMixer.SetFloat("HL3DTI_CompFirst", 1.0f)) return false;
        }
        return true;
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
}
