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
            if (HLAPI == null)
                return false;

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
            DrawNonLinearAttenuation(plugin);
            DrawTemporalDistortion(plugin);
            DrawFrequencySmearing(plugin);     
        }

        //initDone = true;

        //return true;        // SHOW ALSO DEFAULT CONTROLS (FOR DEBUG AND EXPOSING PARAMETERS)
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
        if (Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.GLOBAL_LEFT_ON, "LEFT EAR", "Enable left ear hearing loss", new List<string> { "HLONL" }))
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
        if (Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.GLOBAL_RIGHT_ON, "RIGHT EAR", "Enable right ear hearing loss", new List<string> { "HLONR" }))
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
            //EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_LEFT_ON);
            //Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.MBE_LEFT_ON, "LEFT EAR", "Enable audiometry for left ear", new List<string> { "HLMBEONL" }, true);
            //if (HLAPI.MBE_LEFT_ON)
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
            //EditorGUI.EndDisabledGroup();

            // RIGHT EAR
            Common3DTIGUI.BeginRightColumn(HLAPI.GLOBAL_RIGHT_ON);
            //EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_RIGHT_ON);
            //Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.MBE_RIGHT_ON, "RIGHT EAR", "Enable audiometry for right ear", new List<string> { "HLMBEONR" }, true);
            //if (HLAPI.MBE_RIGHT_ON)
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
            //EditorGUI.EndDisabledGroup();
        }
        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw non-linear attenuation controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawNonLinearAttenuation(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("3DTUNE-IN NON-LINEAR ATTENUATION");
        {
            // LEFT EAR
            //Common3DTIGUI.BeginLeftColumn(HLAPI.GLOBAL_LEFT_ON);
            EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_LEFT_ON);
            Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.MBE_LEFT_ON, "LEFT EAR", "Enable non-linear attenuation for left ear", new List<string> { "HLMBEONL" }, true);
            if (HLAPI.MBE_LEFT_ON)            
            {
                Common3DTIGUI.AddLabelToParameterGroup("Attack");
                Common3DTIGUI.AddLabelToParameterGroup("Release");
                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_ATTACK, "HLATKL", "Attack", false, "ms", "Set attack time of envelope detectors in left ear");
                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_RELEASE, "HLRELL", "Release", false, "ms", "Set release time of envelope detectors in left ear");
            }
            Common3DTIGUI.EndLeftColumn();
            EditorGUI.EndDisabledGroup();

            // RIGHT EAR
            //Common3DTIGUI.BeginRightColumn(HLAPI.GLOBAL_RIGHT_ON);
            EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_RIGHT_ON);
            Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.MBE_RIGHT_ON, "RIGHT EAR", "Enable non-linear attenuation for right ear", new List<string> { "HLMBEONR" }, true);
            if (HLAPI.MBE_RIGHT_ON)
            {
                Common3DTIGUI.AddLabelToParameterGroup("Attack");
                Common3DTIGUI.AddLabelToParameterGroup("Release");
                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_ATTACK, "HLATKR", "Attack", false, "ms", "Set attack time of envelope detectors in right ear");
                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_RELEASE, "HLRELR", "Release", false, "ms", "Set release time of envelope detectors in right ear");
            }
            Common3DTIGUI.EndRightColumn();
            EditorGUI.EndDisabledGroup();
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

    /// <summary>
    /// Draw temporal distortion controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawTemporalDistortion(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("3DTUNE-IN TEMPORAL DISTORTION");
        {
            // LEFT EAR            
            EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_LEFT_ON);
            Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.TA_LEFT_ON, "LEFT EAR", "Enable temporal distortion simulation for left ear", new List<string> { "HLTAONL" }, true);
            {
                //if (HLAPI.TA_LEFT_ON)
                EditorGUI.BeginDisabledGroup(!HLAPI.TA_LEFT_ON);
                {
                    Common3DTIGUI.AddLabelToParameterGroup("Band upper limit");                    
                    Common3DTIGUI.CreatePluginParameterDiscreteSlider(plugin, ref HLAPI.PARAM_LEFT_TA_BAND, "HLTABANDL", "Band upper limit", "Hz", "Set temporal distortion band upper limit in left ear", new List<float> { 200, 400, 800, 1600, 3200, 6400 });
                    Common3DTIGUI.BeginSubsection("Jitter generator");
                        Common3DTIGUI.AddLabelToParameterGroup("White noise power");
                        Common3DTIGUI.AddLabelToParameterGroup("Band width");
                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_TA_POWER, "HLTAPOWL", "White noise power", true, "ms", "Set temporal distortion white noise power in left ear");
                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_TA_CUTOFF, "HLTALPFL", "Band width", true, "Hz", "Set temporal distortion autocorrelation low-pass filter cutoff frequency in left ear");
                    Common3DTIGUI.EndSubsection();                    

                    // Copy left values to right if LRSync is on. It is done internally by the toolkit, but not shown in the GUI
                    if (HLAPI.PARAM_TA_LRSYNC_ON)
                    {
                        plugin.SetFloatParameter("HLTABANDR", HLAPI.PARAM_LEFT_TA_BAND);
                        plugin.SetFloatParameter("HLTAPOWR", HLAPI.PARAM_LEFT_TA_POWER);
                        plugin.SetFloatParameter("HLTALPFR", HLAPI.PARAM_LEFT_TA_CUTOFF);                        
                        HLAPI.SetTemporalDistortionBandUpperLimit(T_ear.RIGHT, HLAPI.FromFloatToBandUpperLimitEnum(HLAPI.PARAM_LEFT_TA_BAND));
                        HLAPI.SetTemporalDistortionWhiteNoisePower(T_ear.RIGHT, HLAPI.PARAM_LEFT_TA_POWER);
                        HLAPI.SetTemporalDistortionAutocorrelationFilterCutoff(T_ear.RIGHT, HLAPI.PARAM_LEFT_TA_CUTOFF);
                    }

                    //float coeff0=0.0f;
                    //float coeff1 = 0.0f;
                    //HLAPI.GetAutocorrelationCoefficients(T_ear.LEFT, out coeff0, out coeff1);
                    ////plugin.GetFloatParameter("HLTA0GL", out coeff0);
                    ////plugin.GetFloatParameter("HLTA1GL", out coeff1);
                    ////coeff1 = coeff1 / coeff0;
                    //Common3DTIGUI.CreateReadonlyFloatText("Noise RMS", "F2", "ms", "RMS power of white noise for left ear temporal distortion", coeff0);
                    //Common3DTIGUI.CreateReadonlyFloatText("Noise Autocorrelation", "F2", "", "First normalized autocorrelation coefficient of filtered noise for left ear temporal distortion", coeff1);
                }
                EditorGUI.EndDisabledGroup();
            }
            Common3DTIGUI.EndLeftColumn();
            EditorGUI.EndDisabledGroup();

            // RIGHT EAR            
            EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_RIGHT_ON);            
            if (Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.TA_RIGHT_ON, "RIGHT EAR", "Enable temporal distortion simulation for right ear", new List<string> { "HLTAONR" }, true))
            {
                // Copy left values to right when LRSync is switched on. It is done internally by the toolkit, but not shown in the GUI
                if (HLAPI.PARAM_TA_LRSYNC_ON)
                {
                    plugin.SetFloatParameter("HLTABANDR", HLAPI.PARAM_LEFT_TA_BAND);
                    plugin.SetFloatParameter("HLTAPOWR", HLAPI.PARAM_LEFT_TA_POWER);
                    plugin.SetFloatParameter("HLTALPFR", HLAPI.PARAM_LEFT_TA_CUTOFF);
                    //plugin.SetFloatParameter("HLTAPOSTONR", CommonFunctions.Bool2Float(HLAPI.PARAM_LEFT_TA_POSTLPF));                    
                    HLAPI.SetTemporalDistortionBandUpperLimit(T_ear.RIGHT, HLAPI.FromFloatToBandUpperLimitEnum(HLAPI.PARAM_LEFT_TA_BAND));
                    HLAPI.SetTemporalDistortionWhiteNoisePower(T_ear.RIGHT, HLAPI.PARAM_LEFT_TA_POWER);
                    HLAPI.SetTemporalDistortionAutocorrelationFilterCutoff(T_ear.RIGHT, HLAPI.PARAM_LEFT_TA_CUTOFF);
                }
            }
            {
                //if (HLAPI.TA_RIGHT_ON)                
                EditorGUI.BeginDisabledGroup(HLAPI.PARAM_TA_LRSYNC_ON);
                EditorGUI.BeginDisabledGroup(!HLAPI.TA_RIGHT_ON);
                    Common3DTIGUI.AddLabelToParameterGroup("Band upper limit");                    
                    Common3DTIGUI.CreatePluginParameterDiscreteSlider(plugin, ref HLAPI.PARAM_RIGHT_TA_BAND, "HLTABANDR", "Band upper limit", "Hz", "Set temporal distortion band upper limit in right ear", new List<float> { 200, 400, 800, 1600, 3200, 6400 });
                    Common3DTIGUI.BeginSubsection("Jitter generator");
                        Common3DTIGUI.AddLabelToParameterGroup("White noise power");
                        Common3DTIGUI.AddLabelToParameterGroup("Band width");
                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_TA_POWER, "HLTAPOWR", "White noise power", true, "ms", "Set temporal distortion white noise power in right ear");
                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_TA_CUTOFF, "HLTALPFR", "Band width", true, "Hz", "Set temporal distortion autocorrelation low-pass filter cutoff frequency in right ear");
                    Common3DTIGUI.EndSubsection();                    

                //float coeff0 = 0.0f;
                //float coeff1 = 0.0f;
                //HLAPI.GetAutocorrelationCoefficients(T_ear.RIGHT, out coeff0, out coeff1);
                ////if (!plugin.GetFloatParameter("HLTA0GR", out coeff0)) coeff0 = -1.0f;
                ////if (!plugin.GetFloatParameter("HLTA1GR", out coeff1)) coeff1 = -1.0f;
                ////coeff1 = coeff1 / coeff0;
                //Common3DTIGUI.CreateReadonlyFloatText("Noise RMS", "F2", "ms", "RMS power of white noise for right ear temporal distortion", coeff0);
                //Common3DTIGUI.CreateReadonlyFloatText("Noise Autocorrelation", "F2", "", "First normalized autocorrelation coefficient of filtered noise for right ear temporal distortion", coeff1);                    
                EditorGUI.EndDisabledGroup();
            }            
            Common3DTIGUI.EndRightColumn();
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            // Left-right synchronicity
            EditorGUI.BeginDisabledGroup((!HLAPI.GLOBAL_LEFT_ON && !HLAPI.GLOBAL_RIGHT_ON) || (!HLAPI.TA_LEFT_ON && !HLAPI.TA_RIGHT_ON));
            {                
                Common3DTIGUI.CreatePluginToggle(plugin, ref HLAPI.PARAM_TA_LRSYNC_ON, "Allow Left-Right synchronicity control", "HLTALRON", "Enable control for left-right synchronicity in temporal distortion");
                if (HLAPI.PARAM_TA_LRSYNC_ON)
                {
                    Common3DTIGUI.AddLabelToParameterGroup("L-R Synchronicity amount");
                    Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_TA_LRSYNC, "HLTALR", "L-R Synchronicity amount", true, "", "Set amount of synchronicity between left and right ears in temporal distortion simulation");
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw frequency smearing controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawFrequencySmearing(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("3DTUNE-IN FREQUENCY SMEARING");
        {
            // LEFT EAR            
            EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_LEFT_ON);
            Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.FS_LEFT_ON, "LEFT EAR", "Enable frequency smearing simulation for left ear", new List<string> { "HLFSONL" }, true);
            {
                EditorGUI.BeginDisabledGroup(!HLAPI.FS_LEFT_ON);
                {
                    // Downward
                    Common3DTIGUI.BeginSubsection("Downward smearing");
                        Common3DTIGUI.AddLabelToParameterGroup("Buffer size");
                        Common3DTIGUI.AddLabelToParameterGroup("Smearing amount");

                        float FloatDownwardSize = (float)HLAPI.PARAM_LEFT_FS_DOWN_SIZE;
                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref FloatDownwardSize, "HLFSDOWNSZL", "Buffer size", false, "samples", "Set buffer size for downward section of smearing window in left ear");
                        HLAPI.PARAM_LEFT_FS_DOWN_SIZE = (int)FloatDownwardSize;

                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_FS_DOWN_HZ, "HLFSDOWNHZL", "Smearing amount", true, "Hz", "Set smearing amount (standard deviation, in Hz) for downward section of smearing window in left ear");
                    Common3DTIGUI.EndSubsection();

                    // Upward
                    Common3DTIGUI.BeginSubsection("Upward smearing");
                        Common3DTIGUI.AddLabelToParameterGroup("Buffer size");
                        Common3DTIGUI.AddLabelToParameterGroup("Smearing amount");

                        float FloatUpwardSize = (float)HLAPI.PARAM_LEFT_FS_UP_SIZE;
                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref FloatUpwardSize, "HLFSUPSZL", "Buffer size", false, "samples", "Set buffer size for upward section of smearing window in left ear");
                        HLAPI.PARAM_LEFT_FS_UP_SIZE = (int)FloatUpwardSize;

                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_FS_UP_HZ, "HLFSUPHZL", "Smearing amount", true, "Hz", "Set smearing amount (standard deviation, in Hz) for upward section of smearing window in left ear");
                    Common3DTIGUI.EndSubsection();
                }
                EditorGUI.EndDisabledGroup();
            }
            Common3DTIGUI.EndLeftColumn();
            EditorGUI.EndDisabledGroup();

            // RIGHT EAR            
            EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_RIGHT_ON);
            Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.FS_RIGHT_ON, "RIGHT EAR", "Enable frequency smearing simulation for right ear", new List<string> { "HLFSONR" }, true);
            {
                EditorGUI.BeginDisabledGroup(!HLAPI.FS_RIGHT_ON);
                {
                    // Downward
                    Common3DTIGUI.BeginSubsection("Downward smearing");
                        Common3DTIGUI.AddLabelToParameterGroup("Buffer size");
                        Common3DTIGUI.AddLabelToParameterGroup("Smearing amount");

                        float FloatDownwardSize = (float)HLAPI.PARAM_RIGHT_FS_DOWN_SIZE;
                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref FloatDownwardSize, "HLFSDOWNSZR", "Buffer size", false, "samples", "Set buffer size for downward section of smearing window in right ear");
                        HLAPI.PARAM_RIGHT_FS_DOWN_SIZE = (int)FloatDownwardSize;

                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_FS_DOWN_HZ, "HLFSDOWNHZR", "Smearing amount", true, "Hz", "Set smearing amount (standard deviation, in Hz) for downward section of smearing window in right ear");
                    Common3DTIGUI.EndSubsection();

                    // Upward
                    Common3DTIGUI.BeginSubsection("Upward smearing");
                        Common3DTIGUI.AddLabelToParameterGroup("Buffer size");
                        Common3DTIGUI.AddLabelToParameterGroup("Smearing amount");

                        float FloatUpwardSize = (float)HLAPI.PARAM_RIGHT_FS_UP_SIZE;
                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref FloatUpwardSize, "HLFSUPSZR", "Buffer size", false, "samples", "Set buffer size for upward section of smearing window in right ear");
                        HLAPI.PARAM_RIGHT_FS_UP_SIZE = (int)FloatUpwardSize;

                        Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_FS_UP_HZ, "HLFSUPHZR", "Smearing amount", true, "Hz", "Set smearing amount (standard deviation, in Hz) for upward section of smearing window in right ear");
                    Common3DTIGUI.EndSubsection();
                }
                EditorGUI.EndDisabledGroup();
            }
            Common3DTIGUI.EndRightColumn();            
            EditorGUI.EndDisabledGroup();
        }
        Common3DTIGUI.EndSection();
    }
}
