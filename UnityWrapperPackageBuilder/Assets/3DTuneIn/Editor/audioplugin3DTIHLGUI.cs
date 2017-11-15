﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;   // For ReadOnlyCollection

using UnityEditor;
using UnityEngine;
using API_3DTI_Common;

public class audioplugin3DTIHLGUI : IAudioEffectPluginGUI
{

    /////////////////////////////////////////////////////////
    // INTERNAL VARIABLES
    /////////////////////////////////////////////////////////

    // Access to the HL API
    API_3DTI_HL HLAPI;

    // Look and feel parameters
    Color selectedColor = Color.gray;
    Color baseColor = Color.white;

    // Global variables, mostly for template buttons activation
    float csCurveWidth = 25.0f;
    float csCurveHeight = 10.0f;
    bool advancedControls = false;  
    API_3DTI_HL.T_HLTemplate selectedAudiometryTemplateLeft = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
    bool changingAudiometryTemplateLeft = false;
    API_3DTI_HL.T_HLTemplate selectedAudiometryTemplateRight = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
    bool changingAudiometryTemplateRight = false;    
    API_3DTI_HL.T_HLClassificationScaleCurve selectedCurveLeft = API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_UNDEFINED;
    int selectedSeverityLeft = -1;
    bool changingCSLeft = false;
    API_3DTI_HL.T_HLClassificationScaleCurve selectedCurveRight = API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_UNDEFINED;
    int selectedSeverityRight = -1;    
    bool changingCSRight = false;
    API_3DTI_HL.T_HLTemplate selectedTDTemplateLeft = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
    bool changingTDTemplateLeft = false;
    API_3DTI_HL.T_HLTemplate selectedTDTemplateRight = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
    bool changingTDTemplateRight = false;
    API_3DTI_HL.T_HLTemplate selectedFSTemplateLeft = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
    bool changingFSTemplateLeft = false;
    API_3DTI_HL.T_HLTemplate selectedFSTemplateRight = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
    bool changingFSTemplateRight = false;

    // Start Play control
    bool isStartingPlay = false;
    bool playWasStarted = false;


    /////////////////////////////////////////////////////////
    // MAIN GUI CODE
    /////////////////////////////////////////////////////////
    
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

		// Setup styles
        Common3DTIGUI.InitStyles();
			
        // Check starting play
        if (EditorApplication.isPlaying && !playWasStarted)
        {
            isStartingPlay = true;
            playWasStarted = true;
        }
        if (!EditorApplication.isPlaying && playWasStarted)
        {
            playWasStarted = false;
        }

        // DRAW AUDIOMETRY GUI
        
        Common3DTIGUI.Show3DTILogo();
        Common3DTIGUI.ShowGUITitle("AUDIOMETRY");
        Common3DTIGUI.SingleSpace();
        Common3DTIGUI.ShowAboutButton();
        Common3DTIGUI.SingleSpace();

        DrawCalibration(plugin);
        DrawAudiometryEars();
        DrawAudiometryTemplates(plugin);
        DrawAudiometryClassificationScale(plugin);
        DrawAudiometryFineAdjustment(plugin);

        // DRAW HEARING LOSS GUI
        Common3DTIGUI.Show3DTILogo();
        Common3DTIGUI.ShowGUITitle("HEARING LOSS SIMULATION");
        Common3DTIGUI.SingleSpace();
        Common3DTIGUI.ShowAboutButton();        
        Common3DTIGUI.SingleSpace();
        
        DrawHLEars(plugin);        
        DrawNonLinearAttenuation(plugin);
        DrawTemporalDistortion(plugin);
        DrawFrequencySmearing(plugin);             

        // End starting play
        isStartingPlay = false;

        // End changing buttons
        changingAudiometryTemplateLeft = false;
        changingAudiometryTemplateRight = false;        
        changingCSLeft = false;
        changingCSRight = false;
        changingTDTemplateLeft = false;
        changingTDTemplateRight = false;
        changingFSTemplateLeft = false;
        changingFSTemplateRight = false;

        //return true;        // SHOW ALSO DEFAULT CONTROLS (FOR DEBUG AND EXPOSING PARAMETERS)
        return false;     // DO NOT SHOW DEFAULT CONTROLS
    }


    /////////////////////////////////////////////////////////
    // MAIN MODULES DRAW 
    /////////////////////////////////////////////////////////

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
    /// Draw ear icons for audiometry module
    /// </summary>    
    public void DrawAudiometryEars()
    {
        // LEFT EAR
        Common3DTIGUI.BeginLeftColumn(true);
        {
            // Draw ear icon
            GUILayout.BeginHorizontal();
            Common3DTIGUI.DrawEar(T_ear.LEFT);
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndLeftColumn();

        // RIGHT EAR
        Common3DTIGUI.BeginRightColumn(true);
        {
            // Draw ear icon
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            Common3DTIGUI.DrawEar(T_ear.RIGHT);
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndRightColumn();
    }

    /// <summary>
    /// Draw audiometry templates for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawAudiometryTemplates(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("TEMPLATES");
        {
            // LEFT EAR
            Common3DTIGUI.BeginLeftColumn(true);
            {
                GUILayout.BeginHorizontal();
                {
                    // Draw ear icon
                    //Common3DTIGUI.DrawEar(T_ear.LEFT);

                    // Draw template buttons    
                    GUILayout.BeginVertical();
                    {
                        if (DrawTemplateButtonsForOneEar(plugin, T_ear.LEFT, "Audiometry", ref selectedAudiometryTemplateLeft, false))
                            SetAudiometryTemplate(plugin, T_ear.LEFT, selectedAudiometryTemplateLeft);
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            Common3DTIGUI.EndLeftColumn();

            // RIGHT EAR
            Common3DTIGUI.BeginRightColumn(true);
            {
                GUILayout.BeginHorizontal();
                {
                    // Draw template buttons    
                    GUILayout.BeginVertical();
                    {
                        if (DrawTemplateButtonsForOneEar(plugin, T_ear.RIGHT, "Audiometry", ref selectedAudiometryTemplateRight, false))
                            SetAudiometryTemplate(plugin, T_ear.RIGHT, selectedAudiometryTemplateRight);
                    }
                    GUILayout.EndVertical();

                    // Draw ear icon
                    //Common3DTIGUI.DrawEar(T_ear.RIGHT);
                }
                GUILayout.EndHorizontal();
            }
            Common3DTIGUI.EndRightColumn();
        }
        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw HL classification scale controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawAudiometryClassificationScale(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("CLASSIFICATION SCALE");
        {
            // LEFT EAR
            Common3DTIGUI.BeginLeftColumn(true);
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        // Curves 
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Label("Curve:");
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_A);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_B);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_C);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_D);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_E);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_F);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_G);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_H);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_I);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_J);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_K);
                        }
                        GUILayout.EndVertical();

                        // Severities
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Label("Severity:");
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, 0);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, 1);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, 2);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, 3);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, 4);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, 5);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, 6);
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndHorizontal();

                    // Reset button
                    Common3DTIGUI.SingleSpace();
                    if (Common3DTIGUI.CreateButton("Reset", "Reset audiometry to No Hearing Loss"))
                        ResetAudiometry(plugin, T_ear.LEFT);
                }
                GUILayout.EndVertical();
            }
            Common3DTIGUI.EndLeftColumn();

            // RIGHT EAR
            Common3DTIGUI.BeginRightColumn(true);
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        // Curves 
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Label("Curve:");
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_A);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_B);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_C);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_D);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_E);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_F);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_G);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_H);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_I);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_J);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_K);
                        }
                        GUILayout.EndVertical();

                        // Severities
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Label("Severity:");
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, 0);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, 1);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, 2);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, 3);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, 4);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, 5);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, 6);
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndHorizontal();

                    // Reset button
                    Common3DTIGUI.SingleSpace();
                    if (Common3DTIGUI.CreateButton("Reset", "Reset audiometry to No Hearing Loss"))
                        ResetAudiometry(plugin, T_ear.RIGHT);
                }
                GUILayout.EndVertical();
            }
            Common3DTIGUI.EndRightColumn();
        }
        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw audiometry fine adjustment controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawAudiometryFineAdjustment(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("FINE ADJUSTMENT");
        {
            // LEFT EAR
            Common3DTIGUI.BeginLeftColumn(true);
            //Common3DTIGUI.BeginLeftColumn(HLAPI.GLOBAL_LEFT_ON);
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
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[0], "HL0L", "62.5 Hz", false, "dB HL", "Set hearing level for 62.5 Hz band in left ear")) ResetAllAudiometryButtonSelections(T_ear.LEFT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[1], "HL1L", "125 Hz", false, "dB HL", "Set hearing level for 125 Hz band in left ear")) ResetAllAudiometryButtonSelections(T_ear.LEFT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[2], "HL2L", "250 Hz", false, "dB HL", "Set hearing level for 250 Hz band in left ear")) ResetAllAudiometryButtonSelections(T_ear.LEFT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[3], "HL3L", "500 Hz", false, "dB HL", "Set hearing level for 500 Hz band in left ear")) ResetAllAudiometryButtonSelections(T_ear.LEFT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[4], "HL4L", "1 KHz", false, "dB HL", "Set hearing level for 1 KHz band in left ear")) ResetAllAudiometryButtonSelections(T_ear.LEFT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[5], "HL5L", "2 KHz", false, "dB HL", "Set hearing level for 2 KHz band in left ear")) ResetAllAudiometryButtonSelections(T_ear.LEFT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[6], "HL6L", "4 KHz", false, "dB HL", "Set hearing level for 4 KHz band in left ear")) ResetAllAudiometryButtonSelections(T_ear.LEFT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[7], "HL7L", "8 KHz", false, "dB HL", "Set hearing level for 8 KHz band in left ear")) ResetAllAudiometryButtonSelections(T_ear.LEFT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_LEFT[8], "HL8L", "16 KHz", false, "dB HL", "Set hearing level for 16 KHz band in left ear")) ResetAllAudiometryButtonSelections(T_ear.LEFT);
            }
            Common3DTIGUI.EndLeftColumn();
            //EditorGUI.EndDisabledGroup();

            // RIGHT EAR
            Common3DTIGUI.BeginRightColumn(true);
            //Common3DTIGUI.BeginRightColumn(HLAPI.GLOBAL_RIGHT_ON);
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
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[0], "HL0R", "62.5 Hz", false, "dB HL", "Set hearing level for 62.5 Hz band in right ear")) ResetAllAudiometryButtonSelections(T_ear.RIGHT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[1], "HL1R", "125 Hz", false, "dB HL", "Set hearing level for 125 Hz band in right ear")) ResetAllAudiometryButtonSelections(T_ear.RIGHT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[2], "HL2R", "250 Hz", false, "dB HL", "Set hearing level for 250 Hz band in right ear")) ResetAllAudiometryButtonSelections(T_ear.RIGHT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[3], "HL3R", "500 Hz", false, "dB HL", "Set hearing level for 500 Hz band in right ear")) ResetAllAudiometryButtonSelections(T_ear.RIGHT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[4], "HL4R", "1 KHz", false, "dB HL", "Set hearing level for 1 KHz band in right ear")) ResetAllAudiometryButtonSelections(T_ear.RIGHT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[5], "HL5R", "2 KHz", false, "dB HL", "Set hearing level for 2 KHz band in right ear")) ResetAllAudiometryButtonSelections(T_ear.RIGHT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[6], "HL6R", "4 KHz", false, "dB HL", "Set hearing level for 4 KHz band in right ear")) ResetAllAudiometryButtonSelections(T_ear.RIGHT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[7], "HL7R", "8 KHz", false, "dB HL", "Set hearing level for 8 KHz band in right ear")) ResetAllAudiometryButtonSelections(T_ear.RIGHT);
                if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_AUDIOMETRY_RIGHT[8], "HL8R", "16 KHz", false, "dB HL", "Set hearing level for 16 KHz band in right ear")) ResetAllAudiometryButtonSelections(T_ear.RIGHT);
            }
            Common3DTIGUI.EndRightColumn();
            //EditorGUI.EndDisabledGroup();
        }
        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw ear icons and global on/off switches for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawHLEars(IAudioEffectPlugin plugin)
    {
        // LEFT EAR
        if (Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.GLOBAL_LEFT_ON, "LEFT EAR", "Enable left ear hearing loss", new List<string> { "HLONL" }, isStartingPlay))
        {
            if (HLAPI.GLOBAL_LEFT_ON)
                HLAPI.EnableHearingLoss(T_ear.LEFT);
            else
                HLAPI.DisableHearingLoss(T_ear.LEFT);
        }
        {
            // Draw ear icon
            GUILayout.BeginHorizontal();
            Common3DTIGUI.DrawEar(T_ear.LEFT);
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndLeftColumn();

        // RIGHT EAR
        if (Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.GLOBAL_RIGHT_ON, "RIGHT EAR", "Enable right ear hearing loss", new List<string> { "HLONR" }, isStartingPlay))
        {
            if (HLAPI.GLOBAL_RIGHT_ON)
                HLAPI.EnableHearingLoss(T_ear.RIGHT);
            else
                HLAPI.DisableHearingLoss(T_ear.RIGHT);
        }
        {
            // Draw ear icon
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            Common3DTIGUI.DrawEar(T_ear.RIGHT);
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndRightColumn();
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
            Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.MBE_LEFT_ON, "LEFT EAR", "Enable non-linear attenuation for left ear", new List<string> { "HLMBEONL" }, isStartingPlay, true);
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
            Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.MBE_RIGHT_ON, "RIGHT EAR", "Enable non-linear attenuation for right ear", new List<string> { "HLMBEONR" }, isStartingPlay, true);
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
    /// Draw temporal distortion controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawTemporalDistortion(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("3DTUNE-IN TEMPORAL DISTORTION");
        {
            // LEFT EAR            
            EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_LEFT_ON);
            Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.TA_LEFT_ON, "LEFT EAR", "Enable temporal distortion simulation for left ear", new List<string> { "HLTAONL" }, isStartingPlay, true);
            {
                //if (HLAPI.TA_LEFT_ON)
                EditorGUI.BeginDisabledGroup(!HLAPI.TA_LEFT_ON);
                {
                    // PRESETS
                    Common3DTIGUI.SingleSpace();
                    GUILayout.BeginHorizontal();
                    {
                        if (DrawTemplateButtonsForOneEar(plugin, T_ear.LEFT, "Temporal Distortion", ref selectedTDTemplateLeft))
                            SetTemporalDistortionTemplate(plugin, T_ear.LEFT, selectedTDTemplateLeft);
                    }
                    GUILayout.EndHorizontal();
                    Common3DTIGUI.SingleSpace();

                    // CONTROLS
                    Common3DTIGUI.AddLabelToParameterGroup("Band upper limit");
                    float bandUpperLimit = HLAPI.FromBandUpperLimitEnumToFloat(HLAPI.PARAM_LEFT_TA_BANDUPPERLIMIT);
                    if (Common3DTIGUI.CreatePluginParameterDiscreteSlider(plugin, ref bandUpperLimit, "HLTABANDL", "Band upper limit", "Hz", "Set temporal distortion band upper limit in left ear", new List<float> { 200, 400, 800, 1600, 3200, 6400, 12800 })) ResetAllTemporalDistortionButtonSelections(T_ear.LEFT);
                    HLAPI.PARAM_LEFT_TA_BANDUPPERLIMIT = HLAPI.FromFloatToBandUpperLimitEnum(bandUpperLimit);

                    Common3DTIGUI.BeginSubsection("Jitter generator");
                    {
                        Common3DTIGUI.AddLabelToParameterGroup("White noise power");
                        Common3DTIGUI.AddLabelToParameterGroup("Band width");
                        if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_TA_WHITENOISEPOWER, "HLTAPOWL", "White noise power", true, "ms", "Set temporal distortion white noise power in left ear")) ResetAllTemporalDistortionButtonSelections(T_ear.LEFT);
                        if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_TA_BANDWIDTH, "HLTALPFL", "Band width", true, "Hz", "Set temporal distortion bandwidth (autocorrelation low-pass filter cutoff frequency) in left ear")) ResetAllTemporalDistortionButtonSelections(T_ear.LEFT);
                    }
                    Common3DTIGUI.EndSubsection();

                    // Copy left values to right if LRSync is on. It is done internally by the toolkit, but not shown in the GUI
                    if (HLAPI.PARAM_TA_LRSYNC_ON)
                    {
                        plugin.SetFloatParameter("HLTABANDR", HLAPI.FromBandUpperLimitEnumToFloat(HLAPI.PARAM_LEFT_TA_BANDUPPERLIMIT));
                        plugin.SetFloatParameter("HLTAPOWR", HLAPI.PARAM_LEFT_TA_WHITENOISEPOWER);
                        plugin.SetFloatParameter("HLTALPFR", HLAPI.PARAM_LEFT_TA_BANDWIDTH);
                        HLAPI.SetTemporalDistortionBandUpperLimit(T_ear.RIGHT, HLAPI.PARAM_LEFT_TA_BANDUPPERLIMIT);
                        HLAPI.SetTemporalDistortionWhiteNoisePower(T_ear.RIGHT, HLAPI.PARAM_LEFT_TA_WHITENOISEPOWER);
                        HLAPI.SetTemporalDistortionBandwidth(T_ear.RIGHT, HLAPI.PARAM_LEFT_TA_BANDWIDTH);
                        ResetAllTemporalDistortionButtonSelections(T_ear.RIGHT);
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
            if (Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.TA_RIGHT_ON, "RIGHT EAR", "Enable temporal distortion simulation for right ear", new List<string> { "HLTAONR" }, isStartingPlay, true))
            {
                // Copy left values to right when LRSync is switched on. It is done internally by the toolkit, but not shown in the GUI
                if (HLAPI.PARAM_TA_LRSYNC_ON)
                {
                    plugin.SetFloatParameter("HLTABANDR", HLAPI.FromBandUpperLimitEnumToFloat(HLAPI.PARAM_LEFT_TA_BANDUPPERLIMIT));
                    plugin.SetFloatParameter("HLTAPOWR", HLAPI.PARAM_LEFT_TA_WHITENOISEPOWER);
                    plugin.SetFloatParameter("HLTALPFR", HLAPI.PARAM_LEFT_TA_BANDWIDTH);
                    //plugin.SetFloatParameter("HLTAPOSTONR", CommonFunctions.Bool2Float(HLAPI.PARAM_LEFT_TA_POSTLPF));                    
                    HLAPI.SetTemporalDistortionBandUpperLimit(T_ear.RIGHT, HLAPI.PARAM_LEFT_TA_BANDUPPERLIMIT);
                    HLAPI.SetTemporalDistortionWhiteNoisePower(T_ear.RIGHT, HLAPI.PARAM_LEFT_TA_WHITENOISEPOWER);
                    HLAPI.SetTemporalDistortionBandwidth(T_ear.RIGHT, HLAPI.PARAM_LEFT_TA_BANDWIDTH);
                    ResetAllTemporalDistortionButtonSelections(T_ear.RIGHT);
                }
            }
            {
                //if (HLAPI.TA_RIGHT_ON)                
                EditorGUI.BeginDisabledGroup(HLAPI.PARAM_TA_LRSYNC_ON);
                EditorGUI.BeginDisabledGroup(!HLAPI.TA_RIGHT_ON);
                {
                    // PRESETS
                    Common3DTIGUI.SingleSpace();
                    GUILayout.BeginHorizontal();
                    {
                        if (DrawTemplateButtonsForOneEar(plugin, T_ear.RIGHT, "Temporal Distortion", ref selectedTDTemplateRight))
                            SetTemporalDistortionTemplate(plugin, T_ear.RIGHT, selectedTDTemplateRight);
                    }
                    GUILayout.EndHorizontal();
                    Common3DTIGUI.SingleSpace();

                    // CONTROLS
                    Common3DTIGUI.AddLabelToParameterGroup("Band upper limit");
                    float bandUpperLimit = HLAPI.FromBandUpperLimitEnumToFloat(HLAPI.PARAM_RIGHT_TA_BANDUPPERLIMIT);
                    if (Common3DTIGUI.CreatePluginParameterDiscreteSlider(plugin, ref bandUpperLimit, "HLTABANDR", "Band upper limit", "Hz", "Set temporal distortion band upper limit in right ear", new List<float> { 200, 400, 800, 1600, 3200, 6400, 12800 })) ResetAllTemporalDistortionButtonSelections(T_ear.RIGHT);
                    HLAPI.PARAM_RIGHT_TA_BANDUPPERLIMIT = HLAPI.FromFloatToBandUpperLimitEnum(bandUpperLimit);

                    Common3DTIGUI.BeginSubsection("Jitter generator");
                    {
                        Common3DTIGUI.AddLabelToParameterGroup("White noise power");
                        Common3DTIGUI.AddLabelToParameterGroup("Band width");
                        if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_TA_WHITENOISEPOWER, "HLTAPOWR", "White noise power", true, "ms", "Set temporal distortion white noise power in right ear")) ResetAllTemporalDistortionButtonSelections(T_ear.RIGHT);
                        if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_TA_BANDWIDTH, "HLTALPFR", "Band width", true, "Hz", "Set temporal distortion bandwidth (autocorrelation low-pass filter cutoff frequency) in right ear")) ResetAllTemporalDistortionButtonSelections(T_ear.RIGHT);
                    }
                    Common3DTIGUI.EndSubsection();

                    //float coeff0 = 0.0f;
                    //float coeff1 = 0.0f;
                    //HLAPI.GetAutocorrelationCoefficients(T_ear.RIGHT, out coeff0, out coeff1);
                    ////if (!plugin.GetFloatParameter("HLTA0GR", out coeff0)) coeff0 = -1.0f;
                    ////if (!plugin.GetFloatParameter("HLTA1GR", out coeff1)) coeff1 = -1.0f;
                    ////coeff1 = coeff1 / coeff0;
                    //Common3DTIGUI.CreateReadonlyFloatText("Noise RMS", "F2", "ms", "RMS power of white noise for right ear temporal distortion", coeff0);
                    //Common3DTIGUI.CreateReadonlyFloatText("Noise Autocorrelation", "F2", "", "First normalized autocorrelation coefficient of filtered noise for right ear temporal distortion", coeff1);                    
                }
                EditorGUI.EndDisabledGroup();
            }
            Common3DTIGUI.EndRightColumn();
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            // Left-right synchronicity
            EditorGUI.BeginDisabledGroup((!HLAPI.GLOBAL_LEFT_ON && !HLAPI.GLOBAL_RIGHT_ON) || (!HLAPI.TA_LEFT_ON && !HLAPI.TA_RIGHT_ON));
            {
                Common3DTIGUI.CreatePluginToggle(plugin, ref HLAPI.PARAM_TA_LRSYNC_ON, "Allow Left-Right synchronicity control", "HLTALRON", "Enable control for left-right synchronicity in temporal distortion", isStartingPlay);
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
            Common3DTIGUI.BeginLeftColumn(plugin, ref HLAPI.FS_LEFT_ON, "LEFT EAR", "Enable frequency smearing simulation for left ear", new List<string> { "HLFSONL" }, isStartingPlay, true);
            {
                EditorGUI.BeginDisabledGroup(!HLAPI.FS_LEFT_ON);
                {
                    // PRESETS
                    Common3DTIGUI.SingleSpace();
                    GUILayout.BeginHorizontal();
                    {
                        if (DrawTemplateButtonsForOneEar(plugin, T_ear.LEFT, "Frequency Smearing", ref selectedFSTemplateLeft))
                            SetFrequencySmearingTemplate(plugin, T_ear.LEFT, selectedFSTemplateLeft);
                    }
                    GUILayout.EndHorizontal();
                    Common3DTIGUI.SingleSpace();

                    // Downward
                    Common3DTIGUI.BeginSubsection("Downward smearing");
                    Common3DTIGUI.AddLabelToParameterGroup("Buffer size");
                    Common3DTIGUI.AddLabelToParameterGroup("Smearing amount");

                    float FloatDownwardSize = (float)HLAPI.PARAM_LEFT_FS_DOWN_SIZE;
                    if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref FloatDownwardSize, "HLFSDOWNSZL", "Buffer size", false, "samples", "Set buffer size for downward section of smearing window in left ear")) ResetAllFrequencySmearingButtonSelections(T_ear.LEFT);
                    HLAPI.PARAM_LEFT_FS_DOWN_SIZE = (int)FloatDownwardSize;

                    if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_FS_DOWN_HZ, "HLFSDOWNHZL", "Smearing amount", true, "Hz", "Set smearing amount (standard deviation, in Hz) for downward section of smearing window in left ear")) ResetAllFrequencySmearingButtonSelections(T_ear.LEFT);
                    Common3DTIGUI.EndSubsection();

                    // Upward
                    Common3DTIGUI.BeginSubsection("Upward smearing");
                    Common3DTIGUI.AddLabelToParameterGroup("Buffer size");
                    Common3DTIGUI.AddLabelToParameterGroup("Smearing amount");

                    float FloatUpwardSize = (float)HLAPI.PARAM_LEFT_FS_UP_SIZE;
                    if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref FloatUpwardSize, "HLFSUPSZL", "Buffer size", false, "samples", "Set buffer size for upward section of smearing window in left ear")) ResetAllFrequencySmearingButtonSelections(T_ear.LEFT);
                    HLAPI.PARAM_LEFT_FS_UP_SIZE = (int)FloatUpwardSize;

                    if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_LEFT_FS_UP_HZ, "HLFSUPHZL", "Smearing amount", true, "Hz", "Set smearing amount (standard deviation, in Hz) for upward section of smearing window in left ear")) ResetAllFrequencySmearingButtonSelections(T_ear.LEFT);
                    Common3DTIGUI.EndSubsection();
                }
                EditorGUI.EndDisabledGroup();
            }
            Common3DTIGUI.EndLeftColumn();
            EditorGUI.EndDisabledGroup();

            // RIGHT EAR            
            EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_RIGHT_ON);
            Common3DTIGUI.BeginRightColumn(plugin, ref HLAPI.FS_RIGHT_ON, "RIGHT EAR", "Enable frequency smearing simulation for right ear", new List<string> { "HLFSONR" }, isStartingPlay, true);
            {
                EditorGUI.BeginDisabledGroup(!HLAPI.FS_RIGHT_ON);
                {
                    // PRESETS
                    Common3DTIGUI.SingleSpace();
                    GUILayout.BeginHorizontal();
                    {
                        if (DrawTemplateButtonsForOneEar(plugin, T_ear.RIGHT, "Frequency Smearing", ref selectedFSTemplateRight))
                            SetFrequencySmearingTemplate(plugin, T_ear.RIGHT, selectedFSTemplateRight);
                    }
                    GUILayout.EndHorizontal();
                    Common3DTIGUI.SingleSpace();

                    // Downward
                    Common3DTIGUI.BeginSubsection("Downward smearing");
                    Common3DTIGUI.AddLabelToParameterGroup("Buffer size");
                    Common3DTIGUI.AddLabelToParameterGroup("Smearing amount");

                    float FloatDownwardSize = (float)HLAPI.PARAM_RIGHT_FS_DOWN_SIZE;
                    if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref FloatDownwardSize, "HLFSDOWNSZR", "Buffer size", false, "samples", "Set buffer size for downward section of smearing window in right ear")) ResetAllFrequencySmearingButtonSelections(T_ear.RIGHT);
                    HLAPI.PARAM_RIGHT_FS_DOWN_SIZE = (int)FloatDownwardSize;

                    if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_FS_DOWN_HZ, "HLFSDOWNHZR", "Smearing amount", true, "Hz", "Set smearing amount (standard deviation, in Hz) for downward section of smearing window in right ear")) ResetAllFrequencySmearingButtonSelections(T_ear.RIGHT);
                    Common3DTIGUI.EndSubsection();

                    // Upward
                    Common3DTIGUI.BeginSubsection("Upward smearing");
                    Common3DTIGUI.AddLabelToParameterGroup("Buffer size");
                    Common3DTIGUI.AddLabelToParameterGroup("Smearing amount");

                    float FloatUpwardSize = (float)HLAPI.PARAM_RIGHT_FS_UP_SIZE;
                    if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref FloatUpwardSize, "HLFSUPSZR", "Buffer size", false, "samples", "Set buffer size for upward section of smearing window in right ear")) ResetAllFrequencySmearingButtonSelections(T_ear.RIGHT);
                    HLAPI.PARAM_RIGHT_FS_UP_SIZE = (int)FloatUpwardSize;

                    if (Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_RIGHT_FS_UP_HZ, "HLFSUPHZR", "Smearing amount", true, "Hz", "Set smearing amount (standard deviation, in Hz) for upward section of smearing window in right ear")) ResetAllFrequencySmearingButtonSelections(T_ear.RIGHT);
                    Common3DTIGUI.EndSubsection();
                }
                EditorGUI.EndDisabledGroup();
            }
            Common3DTIGUI.EndRightColumn();
            EditorGUI.EndDisabledGroup();
        }
        Common3DTIGUI.EndSection();
    }


    /////////////////////////////////////////////////////////
    // AUXILIARY DRAW METHODS
    /////////////////////////////////////////////////////////

    /// <summary>
    ///  Draw audiometry template buttons for one ear
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns>true if one button is pressed</returns>
    public bool DrawTemplateButtonsForOneEar(IAudioEffectPlugin plugin, T_ear ear, string moduleName, ref API_3DTI_HL.T_HLTemplate selectedTemplate, bool includeNormal=true)
    {
        bool result = false;
        string earStr;

        if (ear == T_ear.LEFT)
            earStr = "left";
        else
            earStr = "right";

        if (includeNormal)
        {
            if (selectedTemplate == API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_NORMAL)
                GUI.color = selectedColor;
            else
                GUI.color = baseColor;
            if (GUILayout.Button(new GUIContent("Normal", "Set " + moduleName + " parameters for Normal Hearing in" + earStr + " ear"), GUILayout.ExpandWidth(false)))
            {
                selectedTemplate = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_NORMAL;
                result = true;
            }
        }

        if (selectedTemplate == API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_MILD)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button(new GUIContent("Mild", "Set " + moduleName + " parameters for Mild hearing loss in" + earStr + " ear"), GUILayout.ExpandWidth(false)))
        {                
            selectedTemplate = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_MILD;            
            result = true;
        }

        if (selectedTemplate == API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_MODERATE)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button(new GUIContent("Moderate", "Set " + moduleName + " parameters for Moderate hearing loss in" + earStr + " ear"), GUILayout.ExpandWidth(false)))
        {            
            selectedTemplate = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_MODERATE;            
            result = true;
        }

        if (selectedTemplate == API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_SEVERE)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button(new GUIContent("Severe", "Set " + moduleName + " parameters for Severe hearing loss in" + earStr + " ear"), GUILayout.ExpandWidth(false)))
        {            
            selectedTemplate = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_SEVERE;            
            result = true;
        }
        GUI.color = baseColor;
        return result;
    }

    /// <summary>
    /// Draw controls for one curve (letter) of audiometry classification scale
    /// </summary>
    /// <param name="letter"></param>
    public void DrawOneClassificationScaleCurveControl(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLClassificationScaleCurve curve)
    {
        string earName = "";
        API_3DTI_HL.T_HLClassificationScaleCurve selectedCurve;
        if (ear == T_ear.LEFT)
        {
            earName = "left";
            selectedCurve = selectedCurveLeft;
        }
        else
        {
            earName = "right";
            selectedCurve = selectedCurveRight;
        }

        char letter = HLAPI.FromClassificationScaleCurveToChar(curve);

        GUILayout.BeginHorizontal();
        {
            // Draw curve image
            Common3DTIGUI.DrawImage("curve" + letter, csCurveWidth, csCurveHeight);

            // Set color of button, depending on selection
            if (selectedCurve == curve)
                GUI.color = selectedColor;
            else
                GUI.color = baseColor;

            // Create button with letter and do action if pressed
            if (Common3DTIGUI.CreateButton(letter.ToString(), "Select " + HLAPI.FromClassificationScaleCurveToString(curve) + " curve of HL classification scale for " + earName + " ear"))
            {
                SetClassificationScaleCurve(plugin, ear, curve);
                if (ear == T_ear.LEFT)
                    selectedCurveLeft = curve;
                else
                    selectedCurveRight = curve;
            }
        }
        GUILayout.EndHorizontal();
        GUI.color = baseColor;
    }

    /// <summary>
    /// Draw controls for one severity of audiometry classification scale
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="ear"></param>
    /// <param name="severity"></param>
    public void DrawOneClassificationScaleSeverityControl(IAudioEffectPlugin plugin, T_ear ear, int severity)
    {
        string earName = "";
        int selectedSeverity;
        if (ear == T_ear.LEFT)
        {
            earName = "left";
            selectedSeverity = selectedSeverityLeft;
        }
        else
        {
            earName = "right";
            selectedSeverity = selectedSeverityRight;
        }

        // Set color of button, depending on selection
        if (selectedSeverity == severity)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;

        // Create button with severity level and do action if pressed
        string severityStr = HLAPI.FromClassificationScaleSeverityToString(severity);
        if (Common3DTIGUI.CreateButton(severityStr, "Select " + severityStr + " (" + severity + ") curve of HL classification scale for " + earName + " ear"))
        {
            SetClassificationScaleSeverity(plugin, ear, severity);
            if (ear == T_ear.LEFT)
                selectedSeverityLeft = severity;
            else
                selectedSeverityRight = severity;
        }
        GUI.color = baseColor;
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


    /////////////////////////////////////////////////////////
    // ACTION METHODS
    /////////////////////////////////////////////////////////
    
    /// <summary>
    ///  Set audiometry from template selection from GUI
    /// </summary>
    /// <param name="plugin"></param>
    public void SetAudiometryTemplate(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLTemplate template)
    {
        ReadOnlyCollection<float> templateGains;
        switch (template)
        {
            case API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_NORMAL:
                templateGains = API_3DTI_HL.AUDIOMETRY_TEMPLATE_NORMAL;
                break;
            case API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_MILD:
                templateGains = API_3DTI_HL.AUDIOMETRY_TEMPLATE_MILD;
                break;
            case API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_MODERATE:
                templateGains = API_3DTI_HL.AUDIOMETRY_TEMPLATE_MODERATE;
                break;
            case API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_SEVERE:
                templateGains = API_3DTI_HL.AUDIOMETRY_TEMPLATE_SEVERE;
                break;
            default:
                return;
        }

        if (ear == T_ear.LEFT)
        {
            changingAudiometryTemplateLeft = true;
            for (int b = 0; b < templateGains.Count; b++)
            {
                string paramName = "HL" + b.ToString() + "L";
                plugin.SetFloatParameter(paramName, templateGains[b]);
                HLAPI.PARAM_AUDIOMETRY_LEFT[b] = templateGains[b];
            }
        }
        else
        {
            changingAudiometryTemplateRight = true;
            for (int b = 0; b < templateGains.Count; b++)
            {
                string paramName = "HL" + b.ToString() + "R";
                plugin.SetFloatParameter(paramName, templateGains[b]);
                HLAPI.PARAM_AUDIOMETRY_RIGHT[b] = templateGains[b];
            }
        }
        ResetAllAudiometryButtonSelections(ear);
    }

    /// <summary>
    /// TEMP
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <param name=""></param>
    public void SetClassificationScaleCurve(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLClassificationScaleCurve curve, int severity)
    {
        List<float> hl;
        HLAPI.GetClassificationScaleHL(curve, severity, out hl);
        if (ear == T_ear.LEFT)
        {            
            plugin.SetFloatParameter("HL0L", hl[0]); HLAPI.PARAM_AUDIOMETRY_LEFT[0] = hl[0];
            plugin.SetFloatParameter("HL1L", hl[0]); HLAPI.PARAM_AUDIOMETRY_LEFT[0] = hl[0];
            plugin.SetFloatParameter("HL2L", hl[1]); HLAPI.PARAM_AUDIOMETRY_LEFT[0] = hl[1];
            plugin.SetFloatParameter("HL3L", hl[2]); HLAPI.PARAM_AUDIOMETRY_LEFT[0] = hl[2];
            plugin.SetFloatParameter("HL4L", hl[3]); HLAPI.PARAM_AUDIOMETRY_LEFT[0] = hl[3];
            plugin.SetFloatParameter("HL5L", hl[4]); HLAPI.PARAM_AUDIOMETRY_LEFT[0] = hl[4];
            plugin.SetFloatParameter("HL6L", hl[5]); HLAPI.PARAM_AUDIOMETRY_LEFT[0] = hl[5];
            plugin.SetFloatParameter("HL7L", hl[6]); HLAPI.PARAM_AUDIOMETRY_LEFT[0] = hl[6];
            plugin.SetFloatParameter("HL8L", hl[6]); HLAPI.PARAM_AUDIOMETRY_LEFT[0] = hl[6];
        }
        else
        {
            plugin.SetFloatParameter("HL0R", hl[0]); HLAPI.PARAM_AUDIOMETRY_RIGHT[0] = hl[0];
            plugin.SetFloatParameter("HL1R", hl[0]); HLAPI.PARAM_AUDIOMETRY_RIGHT[0] = hl[0];
            plugin.SetFloatParameter("HL2R", hl[1]); HLAPI.PARAM_AUDIOMETRY_RIGHT[0] = hl[1];
            plugin.SetFloatParameter("HL3R", hl[2]); HLAPI.PARAM_AUDIOMETRY_RIGHT[0] = hl[2];
            plugin.SetFloatParameter("HL4R", hl[3]); HLAPI.PARAM_AUDIOMETRY_RIGHT[0] = hl[3];
            plugin.SetFloatParameter("HL5R", hl[4]); HLAPI.PARAM_AUDIOMETRY_RIGHT[0] = hl[4];
            plugin.SetFloatParameter("HL6R", hl[5]); HLAPI.PARAM_AUDIOMETRY_RIGHT[0] = hl[5];
            plugin.SetFloatParameter("HL7R", hl[6]); HLAPI.PARAM_AUDIOMETRY_RIGHT[0] = hl[6];
            plugin.SetFloatParameter("HL8R", hl[6]); HLAPI.PARAM_AUDIOMETRY_RIGHT[0] = hl[6];
        }
    }

    /// <summary>
    /// Set audiometry from new classification scale curve letter
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="letter"></param>
    public void SetClassificationScaleCurve(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLClassificationScaleCurve curve)
    {
        if (ear == T_ear.LEFT)
        {
            HLAPI.PARAM_CLASSIFICATION_CURVE_LEFT = curve;
            //plugin.SetFloatParameter("HLCSCURL", HLAPI.FromClassificationScaleCurveToFloat(HLAPI.PARAM_CLASSIFICATION_CURVE_LEFT));
            SetClassificationScaleCurve(plugin, ear, curve, HLAPI.PARAM_CLASSIFICATION_SEVERITY_LEFT);
            //changingCurveLeft = true;
            changingCSLeft = true;
        }
        else
        {
            HLAPI.PARAM_CLASSIFICATION_CURVE_RIGHT = curve;
            //plugin.SetFloatParameter("HLCSCURR", HLAPI.FromClassificationScaleCurveToFloat(HLAPI.PARAM_CLASSIFICATION_CURVE_RIGHT));
            SetClassificationScaleCurve(plugin, ear, curve, HLAPI.PARAM_CLASSIFICATION_SEVERITY_RIGHT);
            //changingCurveRight = true;
            changingCSRight = true;
        }
        ResetAllAudiometryButtonSelections(ear);
    }

    /// <summary>
    /// Set audiometry from new classification scale severity
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="severity"></param>
    public void SetClassificationScaleSeverity(IAudioEffectPlugin plugin, T_ear ear, int severity)
    {
        if (ear == T_ear.LEFT)
        {
            HLAPI.PARAM_CLASSIFICATION_SEVERITY_LEFT = severity;
            //plugin.SetFloatParameter("HLCSSEVL", (float)severity);
            SetClassificationScaleCurve(plugin, ear, HLAPI.PARAM_CLASSIFICATION_CURVE_LEFT, severity);
            //changingSeverityLeft = true;
            changingCSLeft = true;           
        }
        else
        {
            HLAPI.PARAM_CLASSIFICATION_SEVERITY_RIGHT = severity;
            //plugin.SetFloatParameter("HLCSSEVR", (float)severity);
            SetClassificationScaleCurve(plugin, ear, HLAPI.PARAM_CLASSIFICATION_CURVE_RIGHT, severity);
            //changingSeverityRight = true;
            changingCSRight = true;
        }
        ResetAllAudiometryButtonSelections(ear);
    }

    /// <summary>
    /// Reset audiometry to No Hearing Loss 
    /// </summary>
    /// <param name="ear"></param>
    public void ResetAudiometry(IAudioEffectPlugin plugin, T_ear ear)
    {
        SetClassificationScaleCurve(plugin, ear, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_NOLOSS);
        SetClassificationScaleSeverity(plugin, ear, 0);
        if (ear == T_ear.LEFT)
        {
            selectedCurveLeft = API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_UNDEFINED;
            selectedSeverityLeft = -1;
            selectedAudiometryTemplateLeft = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
        }
        else
        {
            selectedCurveRight = API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_UNDEFINED;
            selectedSeverityRight = -1;
            selectedAudiometryTemplateRight = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
        }
    }

    /// <summary>
    /// Clear activation of all audiometry buttons
    /// </summary>
    /// <param name="ear"></param>
    public void ResetAllAudiometryButtonSelections(T_ear ear)
    {
        if (ear == T_ear.LEFT)
        {
            if (!changingAudiometryTemplateLeft)
                selectedAudiometryTemplateLeft = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
            if (!changingCSLeft)
            {
                selectedCurveLeft = API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_UNDEFINED;
                selectedSeverityLeft = -1;
            }
        }
        else
        {
            if (!changingAudiometryTemplateRight)
                selectedAudiometryTemplateRight = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
            if (!changingCSRight)
            {
                selectedCurveRight = API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_UNDEFINED;
                selectedSeverityRight = -1;
            }
        }
    }

    /// <summary>
    /// Clear activation of temporal distortion template buttons
    /// </summary>
    /// <param name="ear"></param>
    public void ResetAllTemporalDistortionButtonSelections(T_ear ear)
    {
        if (ear == T_ear.LEFT)
        {
            if (!changingTDTemplateLeft)
                selectedTDTemplateLeft = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
        }
        else
        {
            if (!changingTDTemplateRight)
                selectedTDTemplateRight= API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
        }
    }

    /// <summary>
    /// Set all parameters of Temporal Distortion from one template
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="ear"></param>
    /// <param name="template"></param>
    public void SetTemporalDistortionTemplate(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLTemplate template)
    {
        API_3DTI_HL.T_HLTemporalDistortionBandUpperLimit bandUpperLimit;
        float whiteNoisePower;
        float bandWidth;
        float LRSync;
        HLAPI.GetTemporalDistortionTemplateValues(template, out bandUpperLimit, out whiteNoisePower, out bandWidth, out LRSync);

        if (ear == T_ear.LEFT)
        {
            plugin.SetFloatParameter("HLTABANDL", HLAPI.FromBandUpperLimitEnumToFloat(bandUpperLimit));
            HLAPI.PARAM_LEFT_TA_BANDUPPERLIMIT = bandUpperLimit;
            plugin.SetFloatParameter("HLTAPOWL", whiteNoisePower);
            HLAPI.PARAM_LEFT_TA_WHITENOISEPOWER = whiteNoisePower;
            plugin.SetFloatParameter("HLTALPFL", bandWidth);
            HLAPI.PARAM_LEFT_TA_BANDWIDTH = bandWidth;
            changingTDTemplateLeft = true;
        }
        else
        {
            plugin.SetFloatParameter("HLTABANDR", HLAPI.FromBandUpperLimitEnumToFloat(bandUpperLimit));
            HLAPI.PARAM_RIGHT_TA_BANDUPPERLIMIT = bandUpperLimit;
            plugin.SetFloatParameter("HLTAPOWR", whiteNoisePower);
            HLAPI.PARAM_RIGHT_TA_WHITENOISEPOWER = whiteNoisePower;
            plugin.SetFloatParameter("HLTALPFR", bandWidth);
            HLAPI.PARAM_RIGHT_TA_BANDWIDTH = bandWidth;
            changingTDTemplateRight = true;
        }
        plugin.SetFloatParameter("HLTALR", LRSync);
        HLAPI.PARAM_TA_LRSYNC = LRSync;        
    }

    /// <summary>
    /// Clear activation of frequency smearing template buttons
    /// </summary>
    /// <param name="ear"></param>
    public void ResetAllFrequencySmearingButtonSelections(T_ear ear)
    {
        if (ear == T_ear.LEFT)
        {
            if (!changingFSTemplateLeft)
                selectedFSTemplateLeft = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
        }
        else
        {
            if (!changingFSTemplateRight)
                selectedFSTemplateRight = API_3DTI_HL.T_HLTemplate.HL_TEMPLATE_CUSTOM;
        }
    }

    /// <summary>
    /// Set all parameters of Frequency Smearing from one template
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="ear"></param>
    /// <param name="template"></param>
    public void SetFrequencySmearingTemplate(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLTemplate template)
    {
        int downSize, upSize;
        float downHz, upHz;
        HLAPI.GetFrequencySmearingTemplateValues(template, out downSize, out upSize, out downHz, out upHz);

        if (ear == T_ear.LEFT)
        {
            plugin.SetFloatParameter("HLFSDOWNSZL", (float)downSize);
            HLAPI.PARAM_LEFT_FS_DOWN_SIZE = downSize;
            plugin.SetFloatParameter("HLFSUPSZL", (float)upSize);
            HLAPI.PARAM_LEFT_FS_UP_SIZE = upSize;
            plugin.SetFloatParameter("HLFSDOWNHZL", downHz);
            HLAPI.PARAM_LEFT_FS_DOWN_HZ = downHz;
            plugin.SetFloatParameter("HLFSUPHZL", upHz);
            HLAPI.PARAM_LEFT_FS_UP_HZ = upHz;
            changingFSTemplateLeft = true;
        }
        else
        {
            plugin.SetFloatParameter("HLFSDOWNSZR", (float)downSize);
            HLAPI.PARAM_RIGHT_FS_DOWN_SIZE = downSize;
            plugin.SetFloatParameter("HLFSUPSZR", (float)upSize);
            HLAPI.PARAM_RIGHT_FS_UP_SIZE = upSize;
            plugin.SetFloatParameter("HLFSDOWNHZR", downHz);
            HLAPI.PARAM_RIGHT_FS_DOWN_HZ = downHz;
            plugin.SetFloatParameter("HLFSUPHZR", upHz);
            HLAPI.PARAM_RIGHT_FS_UP_HZ = upHz;
            changingFSTemplateRight = true;
        }
    }
}
