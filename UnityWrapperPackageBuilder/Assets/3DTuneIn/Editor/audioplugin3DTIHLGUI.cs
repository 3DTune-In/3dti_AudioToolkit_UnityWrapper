using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;   // For ReadOnlyCollection

using UnityEditor;
using UnityEngine;
using API_3DTI_Common;

public class audioplugin3DTIHLGUI : IAudioEffectPluginGUI
{
    // Access to the HL API
    API_3DTI_HL HLAPI;

    // Constant definitions
    const int PRESET_CUSTOM = -1;
    const int PRESET_MILD = 0;    
    const int PRESET_MODERATE = 1;
    const int PRESET_SEVERE = 2;
    const int PRESET_PLAIN = 3;

    // Look and feel parameters
    Color selectedColor = Color.gray;
    Color baseColor = Color.white;

    // Global variables 
    bool advancedControls = false;  
    int selectedPresetLeft = PRESET_PLAIN;
    int selectedPresetRight = PRESET_PLAIN;
   // bool initDone = false;

    //The GUI name must be unique for each GUI, the one specified in PluginList.h
    public override string Name
    {
        get { return "3DTI Hearing Loss Simulation"; }
    }

    public override string Description
    {
        get { return "Hearing loss simulation effect from 3D-Tune-In Toolkit"; }
    }

    public override string Vendor
    {
        get { return "3DTi Consortium"; }
    }


    /// <summary>
    ///  GUI layout
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public override bool OnGUI(IAudioEffectPlugin plugin)
    {
        // Initialization (first run)
        //if (!initDone)
        //{
            // Get HL API instance (TO DO: Error check)
            HLAPI = GameObject.FindObjectOfType<API_3DTI_HL>();

        // Send commands to plugin to set all parameters
        //InitializePlugin(plugin);
        //}

        // DRAW CUSTOM GUI

        Common3DTIGUI.Show3DTILogo();        
        DrawEars(plugin);        
        
        advancedControls = Common3DTIGUI.CreateFoldoutToggle(ref advancedControls, "Advanced controls");
        if (advancedControls)
        {
            DrawChainOrder(plugin);
            if (HLAPI.PARAM_COMPRESSOR_FIRST)
            {
                DrawCompressor(plugin);
                DrawEQ(plugin);
            }
            else
            {
                DrawEQ(plugin);
                DrawCompressor(plugin);
            }
            //DrawDebugLog(plugin);
        }        

        //initDone = true;

        //return true;        // SHOW ALSO DEFAULT CONTROLS (FOR DEBUG)
        return false;     // DO NOT SHOW DEFAULT CONTROLS
    }

    /// <summary>
    /// Draw option for changing Eq-Compressor chain
    /// </summary>
    /// <param name="plugin"></param>    
    public void DrawChainOrder(IAudioEffectPlugin plugin)
    {
        string buttonText;
        if (HLAPI.PARAM_COMPRESSOR_FIRST)
            buttonText = "Switch to: Eq->Compressor";
        else
            buttonText = "Switch to: Compressor->Eq";
        if (GUILayout.Button(buttonText, GUILayout.ExpandWidth(false)))
        {
            HLAPI.PARAM_COMPRESSOR_FIRST = !HLAPI.PARAM_COMPRESSOR_FIRST;
            plugin.SetFloatParameter("CompFirst", CommonFunctions.Bool2Float(HLAPI.PARAM_COMPRESSOR_FIRST));
        }
        Common3DTIGUI.SingleSpace();        
    }

    /// <summary>
    ///  Draw preset buttons for one ear
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawPresets(IAudioEffectPlugin plugin, T_ear ear, ref int selectedPreset)
    {
        GUILayout.BeginVertical();

        if (selectedPreset == PRESET_MILD)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button("Mild", GUILayout.ExpandWidth(false)))
        {
            SetEQPreset(plugin, ear, API_3DTI_HL.EQ_PRESET_MILD);
            selectedPreset = PRESET_MILD;
        }

        if (selectedPreset == PRESET_MODERATE)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button("Moderate", GUILayout.ExpandWidth(false)))
        {
            SetEQPreset(plugin, ear, API_3DTI_HL.EQ_PRESET_MODERATE);
            selectedPreset = PRESET_MODERATE;
        }

        if (selectedPreset == PRESET_SEVERE)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button("Severe", GUILayout.ExpandWidth(false)))
        {
            SetEQPreset(plugin, ear, API_3DTI_HL.EQ_PRESET_SEVERE);
            selectedPreset = PRESET_SEVERE;
        }
        GUI.color = baseColor;

        GUILayout.EndVertical();
    }

    /// <summary>
    /// Draw debug log controls 
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawDebugLog(IAudioEffectPlugin plugin)
    {
        //BeginCentralColumn("Debug Log File");
        //{
        //    CreateToggle(plugin, ref debugLogHL, "Write Debug Log File", "DebugLogHL");
        //}
        //EndCentralColumn();
    }

    /// <summary>
    ///  Draw preset buttons for left ear
    /// </summary>
    /// <param name="plugin"></param>
    public void SetEQPreset(IAudioEffectPlugin plugin, T_ear ear, ReadOnlyCollection<float> presetGains)
    {
        if (ear == T_ear.LEFT)
        {
            plugin.SetFloatParameter("EQL0", presetGains[0]);            
            plugin.SetFloatParameter("EQL1", presetGains[1]);
            plugin.SetFloatParameter("EQL2", presetGains[2]);
            plugin.SetFloatParameter("EQL3", presetGains[3]);
            plugin.SetFloatParameter("EQL4", presetGains[4]);
            plugin.SetFloatParameter("EQL5", presetGains[5]);
            plugin.SetFloatParameter("EQL6", presetGains[6]);
            plugin.SetFloatParameter("EQL7", presetGains[7]);
            plugin.SetFloatParameter("EQL8", presetGains[8]);
            for (int b=0; b<presetGains.Count; b++)
                HLAPI.PARAM_BANDS_DB_LEFT[b] = presetGains[b];
        }
        else
        {
            plugin.SetFloatParameter("EQR0", presetGains[0]);
            plugin.SetFloatParameter("EQR1", presetGains[1]);
            plugin.SetFloatParameter("EQR2", presetGains[2]);
            plugin.SetFloatParameter("EQR3", presetGains[3]);
            plugin.SetFloatParameter("EQR4", presetGains[4]);
            plugin.SetFloatParameter("EQR5", presetGains[5]);
            plugin.SetFloatParameter("EQR6", presetGains[6]);
            plugin.SetFloatParameter("EQR7", presetGains[7]);
            plugin.SetFloatParameter("EQR8", presetGains[8]);
            for (int b = 0; b < presetGains.Count; b++)
                HLAPI.PARAM_BANDS_DB_RIGHT[b] = presetGains[b];
        }
    }

    /// <summary>
    /// Draw ear icons and global on/off switches for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawEars(IAudioEffectPlugin plugin)
    {        
        if (Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.GLOBAL_LEFT_ON, "Left ear", "Enable left ear hearing loss", new List<string> { "EQLeftOn", "CompLeftOn" }))
            HLAPI.SwitchOnOffEffect(T_ear.LEFT, API_3DTI_HL.T_HLEffect.EFFECT_HEARINGLOSS, HLAPI.GLOBAL_LEFT_ON);
        {
            GUILayout.BeginHorizontal();
            {
                // Draw ear icon
                Common3DTIGUI.DrawEar(T_ear.LEFT);

                // Draw preset buttons                
                DrawPresets(plugin, T_ear.LEFT, ref selectedPresetLeft);                
            }
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndLeftColumn();
        
        if (Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.GLOBAL_RIGHT_ON, "RIGHT EAR", "Enable right ear hearing loss", new List<string> { "EQRightOn", "CompRightOn" }))
            HLAPI.SwitchOnOffEffect(T_ear.RIGHT, API_3DTI_HL.T_HLEffect.EFFECT_HEARINGLOSS, HLAPI.GLOBAL_RIGHT_ON);
        {
            GUILayout.BeginHorizontal();
            {
                // Draw preset buttons                
                DrawPresets(plugin, T_ear.RIGHT, ref selectedPresetRight);

                // Draw ear icon
                Common3DTIGUI.DrawEar(T_ear.RIGHT);
            }
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndRightColumn();        
    }  

    /// <summary>
    /// Draw EQ controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawEQ(IAudioEffectPlugin plugin)
    {        
        if (Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.PARAM_LEFT_EQ_ON, "Equalizer", "Enable equalizer for left ear", new List<string> { "EQLeftOn" }))
            HLAPI.SwitchOnOffEffect(T_ear.LEFT, API_3DTI_HL.T_HLEffect.EFFECT_EQ, HLAPI.PARAM_LEFT_EQ_ON);
        {
            Common3DTIGUI.AddLabelToParameterGroup("62.5 Hz");
            Common3DTIGUI.AddLabelToParameterGroup("125 Hz");
            Common3DTIGUI.AddLabelToParameterGroup("250 Hz");
            Common3DTIGUI.AddLabelToParameterGroup("500 Hz");
            Common3DTIGUI.AddLabelToParameterGroup("1 KHz");
            Common3DTIGUI.AddLabelToParameterGroup("2 KHz");
            Common3DTIGUI.AddLabelToParameterGroup("4 KHz");
            Common3DTIGUI.AddLabelToParameterGroup("8 KHz");
            Common3DTIGUI.AddLabelToParameterGroup("16 KHz");
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[0], "EQL0", "62.5 Hz", false, "dB", "Set gain for 62.5 Hz band of equalizer in left ear")) selectedPresetLeft = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[1], "EQL1", "125 Hz", false, "dB", "Set gain for 125 Hz band of equalizer in left ear")) selectedPresetLeft = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[2], "EQL2", "250 Hz", false, "dB", "Set gain for 250 Hz band of equalizer in left ear")) selectedPresetLeft = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[3], "EQL3", "500 Hz", false, "dB", "Set gain for 500 Hz band of equalizer in left ear")) selectedPresetLeft = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[4], "EQL4", "1 KHz", false, "dB", "Set gain for 1 KHz band of equalizer in left ear")) selectedPresetLeft = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[5], "EQL5", "2 KHz", false, "dB", "Set gain for 2 KHz band of equalizer in left ear")) selectedPresetLeft = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[6], "EQL6", "4 KHz", false, "dB", "Set gain for 4 KHz band of equalizer in left ear")) selectedPresetLeft = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[7], "EQL7", "8 KHz", false, "dB", "Set gain for 8 KHz band of equalizer in left ear")) selectedPresetLeft = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[8], "EQL8", "16 KHz", false, "dB", "Set gain for 16 KHz band of equalizer in left ear")) selectedPresetLeft = PRESET_CUSTOM;
        }
        Common3DTIGUI.EndLeftColumn();
        
        if (Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.PARAM_RIGHT_EQ_ON, "Equalizer", "Enable equalizer for right ear", new List<string> { "EQRightOn" }))
            HLAPI.SwitchOnOffEffect(T_ear.RIGHT, API_3DTI_HL.T_HLEffect.EFFECT_EQ, HLAPI.PARAM_RIGHT_EQ_ON);
        {
            Common3DTIGUI.AddLabelToParameterGroup("62.5 Hz");
            Common3DTIGUI.AddLabelToParameterGroup("125 Hz");
            Common3DTIGUI.AddLabelToParameterGroup("250 Hz");
            Common3DTIGUI.AddLabelToParameterGroup("500 Hz");
            Common3DTIGUI.AddLabelToParameterGroup("1 KHz");
            Common3DTIGUI.AddLabelToParameterGroup("2 KHz");
            Common3DTIGUI.AddLabelToParameterGroup("4 KHz");
            Common3DTIGUI.AddLabelToParameterGroup("8 KHz");
            Common3DTIGUI.AddLabelToParameterGroup("16 KHz");
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[0], "EQR0", "62.5 Hz", false, "dB", "Set gain for 62.5 Hz band of equalizer in right ear")) selectedPresetRight = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[1], "EQR1", "125 Hz", false, "dB", "Set gain for 125 Hz band of equalizer in right ear")) selectedPresetRight = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[2], "EQR2", "250 Hz", false, "dB", "Set gain for 250 Hz band of equalizer in right ear")) selectedPresetRight = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[3], "EQR3", "500 Hz", false, "dB", "Set gain for 500 Hz band of equalizer in right ear")) selectedPresetRight = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[4], "EQR4", "1 KHz", false, "dB", "Set gain for 1 KHz band of equalizer in right ear")) selectedPresetRight = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[5], "EQR5", "2 KHz", false, "dB", "Set gain for 2 KHz band of equalizer in right ear")) selectedPresetRight = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[6], "EQR6", "4 KHz", false, "dB", "Set gain for 4 KHz band of equalizer in right ear")) selectedPresetRight = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[7], "EQR7", "8 KHz", false, "dB", "Set gain for 8 KHz band of equalizer in right ear")) selectedPresetRight = PRESET_CUSTOM;
            if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[8], "EQR8", "16 KHz", false, "dB", "Set gain for 16 KHz band of equalizer in right ear")) selectedPresetRight = PRESET_CUSTOM;
        }
        Common3DTIGUI.EndRightColumn();
    }

    /// <summary>
    /// Draw Compressor controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawCompressor(IAudioEffectPlugin plugin)
    {        
        if (Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.PARAM_LEFT_COMPRESSOR_ON, "Compressor", "Enable compressor for left ear", new List<string> { "CompLeftOn" }))
            HLAPI.SwitchOnOffEffect(T_ear.LEFT, API_3DTI_HL.T_HLEffect.EFFECT_COMPRESSOR, HLAPI.PARAM_LEFT_COMPRESSOR_ON);
        {
            Common3DTIGUI.AddLabelToParameterGroup("Ratio");
            Common3DTIGUI.AddLabelToParameterGroup("Threshold");
            Common3DTIGUI.AddLabelToParameterGroup("Attack");
            Common3DTIGUI.AddLabelToParameterGroup("Release");
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_COMP_LEFT_RATIO, "LeftRatio", "Ratio", false, ": 1", "Set ratio of compressor in left ear");
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_COMP_LEFT_THRESHOLD, "LeftThreshold", "Threshold", false, "dB", "Set threshold of compressor in left ear");
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_COMP_LEFT_ATTACK, "LeftAttack", "Attack", false, "ms", "Set attack time of compressor in left ear");
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_COMP_LEFT_RELEASE, "LeftRelease", "Release", false, "ms", "Set release time of compressor in left ear");
        }
        Common3DTIGUI.EndLeftColumn();
        
        if (Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.PARAM_RIGHT_COMPRESSOR_ON, "Compressor", "Enable compressor for right ear", new List<string> { "CompRightOn" }))
            HLAPI.SwitchOnOffEffect(T_ear.RIGHT, API_3DTI_HL.T_HLEffect.EFFECT_COMPRESSOR, HLAPI.PARAM_RIGHT_COMPRESSOR_ON);
        {
            Common3DTIGUI.AddLabelToParameterGroup("Ratio");
            Common3DTIGUI.AddLabelToParameterGroup("Threshold");
            Common3DTIGUI.AddLabelToParameterGroup("Attack");
            Common3DTIGUI.AddLabelToParameterGroup("Release");
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_COMP_RIGHT_RATIO, "RightRatio", "Ratio", false, ": 1", "Set ratio of compressor in right ear");
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_COMP_RIGHT_THRESHOLD, "RightThreshold", "Threshold", false, "dB", "Set threshold of compressor in right ear");
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_COMP_RIGHT_ATTACK, "RightAttack", "Attack", false, "ms", "Set attack time of compressor in right ear");
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_COMP_RIGHT_RELEASE, "RightRelease", "Release", false, "ms", "Set release time of compressor in right ear");
        }
        Common3DTIGUI.EndRightColumn();
    }
}
