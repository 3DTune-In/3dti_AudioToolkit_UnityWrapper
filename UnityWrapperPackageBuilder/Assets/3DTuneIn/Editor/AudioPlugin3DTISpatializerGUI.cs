/**
*** Programmers GUI on Unity Inspector for 3D-Tune-In Toolkit Unity Wrapper ***
*
* version beta 1.0
* Created on: July 2016
* 
* Author: 3DI-DIANA Research Group / University of Malaga / Spain
* Contact: areyes@uma.es
* 
* Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
* Module: 3DTI Toolkit Unity Wrapper
**/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using API_3DTI_Common;

[CustomEditor(typeof(API_3DTI_Spatializer))]
public class AudioPlugin3DTISpatializerGUI : Editor
{
    API_3DTI_Spatializer toolkit;
    bool advancedSetup = false;
    bool haSetup = false;
    //bool initDone = false;

    // Limit possible values of sliders    
    float maxHeadRadius = 1.0f;
    float minScale = 0.1f;      
    float maxScale = 10.0f;
    float minDB = -30.0f;
    float maxDB = 0.0f;
    float minHADB = 0.0f;
    float maxHADB = 30.0f;
    float maxSoundSpeed = 1000.0f;


    //////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This is where we create the layout for the inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Init on first pass
        //if (!initDone)
        //{
            toolkit = (API_3DTI_Spatializer)target; // Get access to API script       
            Common3DTIGUI.InitStyles(); // Init styles
            //GameObject go = Camera.main.gameObject;
            //Common3DTIGUI.SetInspectorIcon(go);        
        //initDone = true;
        //}

        // Show 3D-Tune-In logo         
        Common3DTIGUI.Show3DTILogo();

        // Show About button
        Common3DTIGUI.ShowAboutButton();

        ////// LISTENER                
        DrawListenerPanel();

        ////// ADVANCED SETUP
        DrawAdvancedPanel();

        ////// HEARING AID DIRECTIONALITY SETUP
        DrawHADirectionalityPanel();
    }


    ///////////////////////////////////////////////////////////
    // GUI ELEMENT ACTIONS
    ///////////////////////////////////////////////////////////

    /// <summary>
    /// Action for button Load HRTF
    /// </summary>
    public void ButtonLoadHRTF()
    {        
        string filePath = EditorUtility.OpenFilePanel("Select HRTF File", "", "3dti-hrtf");
        if (filePath.Length != 0)
            Common3DTIGUI.ChangeFileName(ref toolkit.HRTFFileName, filePath);
    }

    /// <summary>
    /// Action for button Load ILD Near Field
    /// </summary>
    public void ButtonLoadILDNearField()
    {
        string filePath = EditorUtility.OpenFilePanel("Select ILD File", "", "3dti-ild");        
        if (filePath.Length != 0)
            Common3DTIGUI.ChangeFileName(ref toolkit.ILDNearFieldFileName, filePath);
    }

    /// <summary>
    /// Action for button Load ILD HIgh Performance
    /// </summary>
    public void ButtonLoadILDHighPerformance()
    {
        string filePath = EditorUtility.OpenFilePanel("Select ILD File", "", "3dti-ild");
        if (filePath.Length != 0)
            Common3DTIGUI.ChangeFileName(ref toolkit.ILDHighPerformanceFileName, filePath);
    }

    /// <summary>
    /// Action for slider HeadRadius
    /// </summary>
    public void SliderHeadRadius()
    {
        toolkit.SetHeadRadius(toolkit.listenerHeadRadius);
    }

    /// <summary>
    /// Action for input ResamplingStep
    /// </summary>
    public void InputResamplingStep()
    {
        toolkit.SetHRTFResamplingStep(toolkit.HRTFstep);
    }

    /// <summary>
    /// Action for slider Scale
    /// </summary>
    public void SliderScale()
    {        
        toolkit.SetScaleFactor(toolkit.scaleFactor);
    }

    /// <summary>
    /// Action for slider Anechoic Attenuation
    /// </summary>
    public void SliderAnechoicAttenuation()
    {
        toolkit.SetMagnitudeAnechoicAttenuation(toolkit.magAnechoicAttenuation);
    }

    /// <summary>
    /// Action for slider Sound Speed
    /// </summary>
    public void SliderSoundSpeed()
    {
        toolkit.SetMagnitudeSoundSpeed(toolkit.magSoundSpeed);
    }

    /// <summary>
    ///  Action for slider HA Directionality Left
    /// </summary>
    public void SliderHADirectionalityLeft()
    {
        toolkit.SetHADirectionalityExtend(T_ear.LEFT, toolkit.HADirectionalityExtendLeft);
    }

    /// <summary>
    ///  Action for slider HA Directionality Right 
    /// </summary>
    public void SliderHADirectionalityRight()
    {
        toolkit.SetHADirectionalityExtend(T_ear.RIGHT, toolkit.HADirectionalityExtendRight);
    }


    ///////////////////////////////////////////////////////////
    // PANEL CONTENTS
    ///////////////////////////////////////////////////////////

    /// <summary>
    /// Draw panel with Listener configuration
    /// </summary>
    public void DrawListenerPanel()
    {
        Common3DTIGUI.BeginSection("LISTENER SETUP");

        // HIGH PERFORMANCE / HIGH QUALITY CHOICE:
        //if (Common3DTIGUI.CreateRadioButtons(ref toolkit.highPerformanceMode, "High Quality mode", "Enable high quality (lower performance) mode",
        //                                                                              "High Performance Mode", "Enable high performance (lower quality) mode"))
        //    toolkit.SetHighPerformanceMode(toolkit.highPerformanceMode);
        if (Common3DTIGUI.CreateRadioButtons(ref toolkit.spatializationMode, new List<string>(new string[] { "High Quality mode", "High Performance mode", "No spatialization" }),
                new List<string>(new string[] { "Enable high quality (lower performance) mode", "Enable high performance (lower quality) mode", "Disable spatialization"})))                                                                              
            toolkit.SetSpatializationMode(toolkit.spatializationMode);

        Common3DTIGUI.SingleSpace();

        // HIGH PERFORMANCE MODE CONTROLS
        if (toolkit.spatializationMode ==  API_3DTI_Spatializer.SPATIALIZATION_MODE_HIGH_PERFORMANCE)
        {            
            Common3DTIGUI.AddLabelToParameterGroup("High Performance ILD");            

            Common3DTIGUI.CreateLoadButtonAndBox("High Performance ILD", "Select the high performance ILD filter of the listener from a .3dti-ild file", ref toolkit.ILDHighPerformanceFileName, ButtonLoadILDHighPerformance);
        }

        // HIGH QUALITY MODE CONTROLS
        if (toolkit.spatializationMode == API_3DTI_Spatializer.SPATIALIZATION_MODE_HIGH_QUALITY)
        {        
            Common3DTIGUI.AddLabelToParameterGroup("HRTF");
            Common3DTIGUI.AddLabelToParameterGroup("Near Field Filter ILD");

            // HRTF:
            Common3DTIGUI.CreateLoadButtonAndBox("HRTF", "Select the HRTF of the listener from a .3dti-hrtf file", ref toolkit.HRTFFileName, ButtonLoadHRTF);

            // ILD:
            Common3DTIGUI.SingleSpace();
            Common3DTIGUI.CreateLoadButtonAndBox("Near Field Filter ILD", "Select the ILD near field filter of the listener from a .3dti-ild file", ref toolkit.ILDNearFieldFileName, ButtonLoadILDNearField);
        }

        // ITD:    
        if (!(toolkit.spatializationMode == API_3DTI_Spatializer.SPATIALIZATION_MODE_NONE))
        {
            Common3DTIGUI.ResetParameterGroup();
            Common3DTIGUI.AddLabelToParameterGroup("Custom ITD");
            Common3DTIGUI.AddLabelToParameterGroup("Head radius");
            Common3DTIGUI.SingleSpace();
            if (Common3DTIGUI.CreateToggle(ref toolkit.customITDEnabled, "Custom ITD", "Enable Interaural Time Difference customization"))
                toolkit.SetCustomITD(toolkit.customITDEnabled);
            if (toolkit.customITDEnabled)
                Common3DTIGUI.CreateFloatSlider(ref toolkit.listenerHeadRadius, "Head radius", "F4", "meters", "Set listener head radius", 0.0f, maxHeadRadius, SliderHeadRadius);
        }

        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw panel with advanced configuration
    /// </summary>
    public void DrawAdvancedPanel()
    {
        Common3DTIGUI.BeginSection("ADVANCED SETUP");

        advancedSetup = Common3DTIGUI.CreateFoldoutToggle(ref advancedSetup, "Advanced Setup");
        if (advancedSetup)
        {
            // Scale factor slider
            Common3DTIGUI.AddLabelToParameterGroup("Scale factor");
            Common3DTIGUI.SingleSpace();
            Common3DTIGUI.CreateFloatSlider(ref toolkit.scaleFactor, "Scale factor", "F2", " meters = 1.0 unit in Unity", "Set the proportion between meters and Unity scale units", minScale, maxScale, SliderScale);

            // HRTF interpolation
            if (toolkit.spatializationMode == API_3DTI_Spatializer.SPATIALIZATION_MODE_HIGH_QUALITY)
            {
                Common3DTIGUI.BeginSubsection("HRTF Interpolation");
                Common3DTIGUI.AddLabelToParameterGroup("Runtime interpolation");
                Common3DTIGUI.AddLabelToParameterGroup("Resampling step");
                    GUILayout.BeginHorizontal();
                    if (Common3DTIGUI.CreateToggle(ref toolkit.runtimeInterpolateHRTF, "Runtime interpolation", "Enable runtime interpolation of HRIRs, to allow for smoother transitions when moving listener and/or sources"))
                        toolkit.SetSourceInterpolation(toolkit.runtimeInterpolateHRTF);
                    GUILayout.EndHorizontal();
                    Common3DTIGUI.CreateIntInput(ref toolkit.HRTFstep, "Resampling step", "º", "HRTF resampling step; Lower values give better quality at the cost of more resources", 5, 45, InputResamplingStep);
                Common3DTIGUI.EndSubsection();
            }

            // Mod enabler
            Common3DTIGUI.BeginSubsection("Switch Spatialization Effects");
            Common3DTIGUI.AddLabelToParameterGroup("Far distance LPF");
            Common3DTIGUI.AddLabelToParameterGroup("Distance attenuation");
            if (toolkit.spatializationMode == API_3DTI_Spatializer.SPATIALIZATION_MODE_HIGH_QUALITY)
            {
                Common3DTIGUI.AddLabelToParameterGroup("ILD Near Field Filter");
                //Common3DTIGUI.AddLabelToParameterGroup("HRTF convolution");
            }
                if (Common3DTIGUI.CreateToggle(ref toolkit.modFarLPF, "Far distance LPF", "Enable low pass filter to simulate sound coming from far distances"))
                    toolkit.SetModFarLPF(toolkit.modFarLPF);
                if (Common3DTIGUI.CreateToggle(ref toolkit.modDistAtt, "Distance attenuation", "Enable attenuation of sound depending on distance to listener"))
                    toolkit.SetModDistanceAttenuation(toolkit.modDistAtt);

                if (toolkit.spatializationMode == API_3DTI_Spatializer.SPATIALIZATION_MODE_HIGH_QUALITY)
                {
                    if (Common3DTIGUI.CreateToggle(ref toolkit.modILD, "ILD Near Field Filter", "Enable near field filter for sources very close to the listener ears"))
                        toolkit.SetModILD(toolkit.modILD);
                    //if (Common3DTIGUI.CreateToggle(ref toolkit.modHRTF, "HRTF convolution", "Enable HRTF convolution, the core of binaural spatialization"))
                    //    toolkit.SetModHRTF(toolkit.modHRTF);
                }
            Common3DTIGUI.EndSubsection();

            // Magnitudes
            Common3DTIGUI.BeginSubsection("Physical magnitudes");
            Common3DTIGUI.AddLabelToParameterGroup("Anechoic distance attenuation");
            Common3DTIGUI.AddLabelToParameterGroup("Sound speed");
                Common3DTIGUI.CreateFloatSlider(ref toolkit.magAnechoicAttenuation, "Anechoic distance attenuation", "F2", "dB", "Set attenuation in decibels for each double distance", minDB, maxDB, SliderAnechoicAttenuation);            
                Common3DTIGUI.CreateFloatSlider(ref toolkit.magSoundSpeed, "Sound speed", "F0", "m/s", "Set sound speed, used for ITD computation", 0.0f, maxSoundSpeed, SliderSoundSpeed);
            Common3DTIGUI.EndSubsection();

            // Limiter
            Common3DTIGUI.BeginSubsection("Limiter");
            Common3DTIGUI.AddLabelToParameterGroup("Switch Limiter");
                //GUILayout.BeginHorizontal();
                //SingleSpace();
                if (Common3DTIGUI.CreateToggle(ref toolkit.doLimiter, "Switch Limiter", "Enable dynamics limiter after spatialization, to avoid potential saturation"))
                    toolkit.SwitchOnOffLimiter(toolkit.doLimiter);
                //if (toolkit.doLimiter)
                //{
                //    bool compressing;
                //    toolkit.GetLimiterCompression(out compressing);
                //    CreateLamp(compressing);
                //}
                //GUILayout.EndHorizontal();
            Common3DTIGUI.EndSubsection();

            // Debug Log
            Common3DTIGUI.BeginSubsection("Debug log");
            Common3DTIGUI.AddLabelToParameterGroup("Write debug log file");
                if (Common3DTIGUI.CreateToggle(ref toolkit.debugLog, "Write debug log file", "Enable writing of 3DTi_BinauralSpatializer_DebugLog.txt file in your project root folder; This file can be sent to the 3DTi Toolkit developers for support"))
                    toolkit.SendWriteDebugLog(toolkit.debugLog);
            Common3DTIGUI.EndSubsection();
        }

        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw panel with Hearing Aid directionality configuration
    /// </summary>
    public void DrawHADirectionalityPanel()
    {
        Common3DTIGUI.BeginSection("DIRECTIONALITY");
        
        haSetup = Common3DTIGUI.CreateFoldoutToggle(ref haSetup, "Directionality Setup");
        if (haSetup)
        {
            Common3DTIGUI.SingleSpace();          

            // Left ear
            Common3DTIGUI.BeginSubsection("Left ear");
            Common3DTIGUI.AddLabelToParameterGroup("Switch Directionality");
            Common3DTIGUI.AddLabelToParameterGroup("Directionality extend");
                if (Common3DTIGUI.CreateToggle(ref toolkit.doHADirectionalityLeft, "Switch Directionality", "Enable directionality for left ear"))            
                    toolkit.SwitchOnOffHADirectionality(T_ear.LEFT, toolkit.doHADirectionalityLeft);
                Common3DTIGUI.CreateFloatSlider(ref toolkit.HADirectionalityExtendLeft, "Directionality extend", "F2", "dB", "Set directionality extend for left ear; The value is the attenuation in decibels applied to sources placed behind the listener", minHADB, maxHADB, SliderHADirectionalityLeft);            
            Common3DTIGUI.EndSubsection();

            // Right ear
            Common3DTIGUI.BeginSubsection("Right ear");
            Common3DTIGUI.AddLabelToParameterGroup("Switch Directionality");
            Common3DTIGUI.AddLabelToParameterGroup("Directionality extend");
                if (Common3DTIGUI.CreateToggle(ref toolkit.doHADirectionalityRight, "Switch Directionality", "Enable directionality for right ear"))
                    toolkit.SwitchOnOffHADirectionality(T_ear.RIGHT, toolkit.doHADirectionalityRight);
                Common3DTIGUI.CreateFloatSlider(ref toolkit.HADirectionalityExtendRight, "Directionality extend", "F2", "dB", "Set directionality extend for right ear; The value is the attenuation in decibels applied to sources placed behind the listener", minHADB, maxHADB, SliderHADirectionalityRight);            
            Common3DTIGUI.EndSubsection();
        }

        Common3DTIGUI.EndSection();
    }
}