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
    const int PRESET_NORMAL = 3;

    // Look and feel parameters
    Color selectedColor = Color.gray;
    Color baseColor = Color.white;

    // Global variables 
    bool advancedControls = false;  
    int selectedPresetLeft = PRESET_NORMAL;
    int selectedPresetRight = PRESET_NORMAL;
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
        Common3DTIGUI.ShowAboutButton();
        DrawEars(plugin);        
        
        advancedControls = Common3DTIGUI.CreateFoldoutToggle(ref advancedControls, "Advanced controls");
        if (advancedControls)
        {
            DrawCalibration(plugin);
            DrawAudiometry(plugin);
            DrawEnvelopeDetector(plugin);            
        }

        //initDone = true;

        //return true;        // SHOW ALSO DEFAULT CONTROLS (FOR DEBUG)
        return false;     // DO NOT SHOW DEFAULT CONTROLS
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
                SetAudiometryPreset(plugin, ear, API_3DTI_HL.AUDIOMETRY_PRESET_MILD);
                selectedPreset = PRESET_MILD;
            }

            if (selectedPreset == PRESET_MODERATE)
                GUI.color = selectedColor;
            else
                GUI.color = baseColor;
            if (GUILayout.Button("Moderate", GUILayout.ExpandWidth(false)))
            {
                SetAudiometryPreset(plugin, ear, API_3DTI_HL.AUDIOMETRY_PRESET_MODERATE);
                selectedPreset = PRESET_MODERATE;
            }

            if (selectedPreset == PRESET_SEVERE)
                GUI.color = selectedColor;
            else
                GUI.color = baseColor;
            if (GUILayout.Button("Severe", GUILayout.ExpandWidth(false)))
            {
                SetAudiometryPreset(plugin, ear, API_3DTI_HL.AUDIOMETRY_PRESET_SEVERE);
                selectedPreset = PRESET_SEVERE;
            }
            GUI.color = baseColor;

        GUILayout.EndVertical();
    }

    ///// <summary>
    ///// Draw debug log controls 
    ///// </summary>
    ///// <param name="plugin"></param>
    //public void DrawDebugLog(IAudioEffectPlugin plugin)
    //{
    //    //BeginCentralColumn("Debug Log File");
    //    //{
    //    //    CreateToggle(plugin, ref debugLogHL, "Write Debug Log File", "DebugLogHL");
    //    //}
    //    //EndCentralColumn();
    //}

    /// <summary>
    ///  ...
    /// </summary>
    /// <param name="plugin"></param>
    public void SetAudiometryPreset(IAudioEffectPlugin plugin, T_ear ear, ReadOnlyCollection<float> presetGains)
    {        
        if (ear == T_ear.LEFT)
        {
            for (int b = 0; b < presetGains.Count; b++)
            {
                string paramName = "HL" + b.ToString() + "L";
                plugin.SetFloatParameter(paramName, presetGains[b]);
                HLAPI.PARAM_AUDIOMETRY_LEFT[b] = presetGains[b];
            }
        }
        else
        {
            for (int b = 0; b < presetGains.Count; b++)
            {
                string paramName = "HL" + b.ToString() + "R";
                plugin.SetFloatParameter(paramName, presetGains[b]);
                HLAPI.PARAM_AUDIOMETRY_RIGHT[b] = presetGains[b];
            }
        }
    }

    /// <summary>
    /// Draw ear icons and global on/off switches for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawEars(IAudioEffectPlugin plugin)
    {
        // LEFT EAR
        if (Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.GLOBAL_LEFT_ON, "Left ear", "Enable left ear hearing loss", new List<string> { "HLONL" }))
        {
            if (HLAPI.GLOBAL_LEFT_ON)
                HLAPI.EnableHearingLoss(T_ear.LEFT);
            else
                HLAPI.DisableHearingLoss(T_ear.LEFT);
        }
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

        // RIGHT EAR
        if (Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.GLOBAL_RIGHT_ON, "Right ear", "Enable right ear hearing loss", new List<string> { "HLONR" }))
        {
            if (HLAPI.GLOBAL_RIGHT_ON)
                HLAPI.EnableHearingLoss(T_ear.RIGHT);
            else
                HLAPI.DisableHearingLoss(T_ear.RIGHT);
        }
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
    /// Draw audiometry controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawAudiometry(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("AUDIOMETRY");
        {
            // LEFT EAR
            Common3DTIGUI.BeginLeftColumn(HLAPI.GLOBAL_LEFT_ON);
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
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[0], "HL0L", "62.5 Hz", false, "dB HL", "Set hearing level for 62.5 Hz band in left ear")) selectedPresetLeft = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[1], "HL1L", "125 Hz", false, "dB HL", "Set hearing level for 125 Hz band in left ear")) selectedPresetLeft = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[2], "HL2L", "250 Hz", false, "dB HL", "Set hearing level for 250 Hz band in left ear")) selectedPresetLeft = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[3], "HL3L", "500 Hz", false, "dB HL", "Set hearing level for 500 Hz band in left ear")) selectedPresetLeft = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[4], "HL4L", "1 KHz", false, "dB HL", "Set hearing level for 1 KHz band in left ear")) selectedPresetLeft = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[5], "HL5L", "2 KHz", false, "dB HL", "Set hearing level for 2 KHz band in left ear")) selectedPresetLeft = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[6], "HL6L", "4 KHz", false, "dB HL", "Set hearing level for 4 KHz band in left ear")) selectedPresetLeft = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[7], "HL7L", "8 KHz", false, "dB HL", "Set hearing level for 8 KHz band in left ear")) selectedPresetLeft = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[8], "HL8L", "16 KHz", false, "dB HL", "Set hearing level for 16 KHz band in left ear")) selectedPresetLeft = PRESET_CUSTOM;
            }
            Common3DTIGUI.EndLeftColumn();

            // RIGHT EAR
            Common3DTIGUI.BeginRightColumn(HLAPI.GLOBAL_RIGHT_ON);
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
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[0], "HL0R", "62.5 Hz", false, "dB HL", "Set hearing level for 62.5 Hz band in right ear")) selectedPresetRight = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[1], "HL1R", "125 Hz", false, "dB HL", "Set hearing level for 125 Hz band in right ear")) selectedPresetRight = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[2], "HL2R", "250 Hz", false, "dB HL", "Set hearing level for 250 Hz band in right ear")) selectedPresetRight = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[3], "HL3R", "500 Hz", false, "dB HL", "Set hearing level for 500 Hz band in right ear")) selectedPresetRight = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[4], "HL4R", "1 KHz", false, "dB HL", "Set hearing level for 1 KHz band in right ear")) selectedPresetRight = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[5], "HL5R", "2 KHz", false, "dB HL", "Set hearing level for 2 KHz band in right ear")) selectedPresetRight = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[6], "HL6R", "4 KHz", false, "dB HL", "Set hearing level for 4 KHz band in right ear")) selectedPresetRight = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[7], "HL7R", "8 KHz", false, "dB HL", "Set hearing level for 8 KHz band in right ear")) selectedPresetRight = PRESET_CUSTOM;
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[8], "HL8R", "16 KHz", false, "dB HL", "Set hearing level for 16 KHz band in right ear")) selectedPresetRight = PRESET_CUSTOM;
            }
            Common3DTIGUI.EndRightColumn();
        }
        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw envelope detector controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawEnvelopeDetector(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("ENVELOPE DETECTORS");
        {
            // LEFT EAR
            Common3DTIGUI.BeginLeftColumn(HLAPI.GLOBAL_LEFT_ON);
            {
                Common3DTIGUI.AddLabelToParameterGroup("Attack");
                Common3DTIGUI.AddLabelToParameterGroup("Release");
                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_ATTACK, "HLATKL", "Attack", false, "ms", "Set attack time of envelope detectors in left ear");
                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_RELEASE, "HLRELL", "Release", false, "ms", "Set release time of envelope detectors in left ear");
            }
            Common3DTIGUI.EndLeftColumn();

            // RIGHT EAR
            Common3DTIGUI.BeginRightColumn(HLAPI.GLOBAL_RIGHT_ON);
            {
                Common3DTIGUI.AddLabelToParameterGroup("Attack");
                Common3DTIGUI.AddLabelToParameterGroup("Release");
                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_ATTACK, "HLATKR", "Attack", false, "ms", "Set attack time of envelope detectors in right ear");
                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_RELEASE, "HLRELR", "Release", false, "ms", "Set release time of envelope detectors in right ear");
            }
            Common3DTIGUI.EndRightColumn();
        }
        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw calibration controls 
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawCalibration(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("CALIBRATION");
        {
            Common3DTIGUI.AddLabelToParameterGroup("dB SPL for 0 dB FS");                
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_CALIBRATION, "HLCAL", "dB SPL for 0 dB FS", false, "dB SPL", "Set how many dB SPL are assumed for 0 dB FS");
        }
        Common3DTIGUI.EndSection();
    }
}
