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
using System.Collections.Generic;
using UnityEditor;
using API_3DTI_Common;
using System;
using System.Linq;
using static API_3DTI_Spatializer;

[CustomEditor(typeof(API_3DTI_Spatializer))]
public class AudioPlugin3DTISpatializerGUI : Editor
{
    

    API_3DTI_Spatializer toolkit;
    bool perSourceAdvancedSetup = false;
    bool advancedSetup = false;
    bool haSetup = false;

    // Limit possible values of sliders    
    float maxHeadRadius = 1.0f;
    float minScale = 0.1f;      
    float maxScale = 10.0f;
    float minDB = -30.0f;
    float maxDB = 0.0f;
    float minHADB = 0.0f;
    float maxHADB = 30.0f;
    float maxSoundSpeed = 1000.0f;

    // Start Play control
    bool isStartingPlay = false;
    bool playWasStarted = false;


    //////////////////////////////////////////////////////////////////////////////


    //public static (string[] highQualityHRTFs, string[] highQualityILDs, string[] highPerformanceILDs, string[] reverbBRIRs) GetFilterBinaryPaths(TSampleRateEnum sampleRate)
    //{
    //    string sampleRateLabel =
    //        sampleRate == TSampleRateEnum.K44 ? "44100"
    //        : sampleRate == TSampleRateEnum.K48 ? "48000"
    //        : sampleRate == TSampleRateEnum.K96 ? "96000"
    //        : "(unknown sample rate)";

    //    string[] highPerformanceILDs = Resources.LoadAll<TextAsset>("Data/HighPerformance/ILD")
    //        .Where(x => x.name.Contains(sampleRateLabel))
    //        .Select(item => item.name).ToArray();

    //    string[] highQualityHRTFs = Resources.LoadAll<TextAsset>("Data/HighQuality/HRTF")
    //        .Where(x => x.name.Contains(sampleRateLabel))
    //        .Select(item => item.name).ToArray();

    //    string[] highQualityILDs = Resources.LoadAll<TextAsset>("Data/HighQuality/ILD")
    //        .Where(x => x.name.Contains(sampleRateLabel))
    //        .Select(item => item.name).ToArray();

    //    string[] reverbBRIRs = Resources.LoadAll<TextAsset>("Data/Reverb/BRIR")
    //        .Where(x => x.name.Contains(sampleRateLabel))
    //        .Select(item => item.name).ToArray();

    //    return (highQualityHRTFs, highQualityILDs, highPerformanceILDs, reverbBRIRs);
    //}

    public static (string prefix, string[] paths, string suffix) GetBinaryPaths(SpatializerBinaryRole role, TSampleRateEnum sampleRate)
    {
        // The DLL 
        //string rootDirectory = "Assets/3DTuneIn/Resources/";
        string prefix;
        switch (role)
        {
            case SpatializerBinaryRole.HighPerformanceILD:
                prefix = "Data/HighPerformance/ILD/";
                break;
            case SpatializerBinaryRole.HighQualityHRTF:
                prefix = "Data/HighQuality/HRTF/";
                break;
            case SpatializerBinaryRole.HighQualityILD:
                prefix = "Data/HighQuality/ILD/";
                break;
            case SpatializerBinaryRole.ReverbBRIR:
                prefix = "Data/Reverb/BRIR/";
                break;
            default:
                throw new Exception("Invalid value for SpatializerBinaryRole.");
        }

        string sampleRateLabel =
            sampleRate == TSampleRateEnum.K44 ? "44100"
            : sampleRate == TSampleRateEnum.K48 ? "48000"
            : sampleRate == TSampleRateEnum.K96 ? "96000"
            : "(unknown sample rate)";
        // LoadAll searches relative to any "resources" folder in the project
        string[] paths = Resources.LoadAll<TextAsset>(prefix)
                    .Where(x => x.name.Contains(sampleRateLabel))
                    .Select(item => item.name).ToArray();
        return (prefix, paths, ".bytes");
    }



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
        //Debug.Log($"HRTFFileName44 = {toolkit.HRTFFileName44}, HRTFFileName48 = {toolkit.HRTFFileName48}");

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

        // Show 3D-Tune-In logo         
        Common3DTIGUI.Show3DTILogo();

        // Show About button
        Common3DTIGUI.ShowAboutButton();

        ////// LISTENER                
        DrawControls();


        // End starting play
        isStartingPlay = false;

        if (GUI.changed)
        {
            Undo.RecordObject(toolkit, "Modify 3DTI Spatializer parameter");
            EditorUtility.SetDirty(toolkit);
        }
    }






    // Create a control for a SpatializerParameter parameter. Returns true if the value changed
    public bool CreateControl(SpatializerParameter parameter, bool isCompact = false)
    {
        SpatializerParameterAttribute p = parameter.GetAttribute<SpatializerParameterAttribute>();
        if (p == null)
        {
            throw new Exception($"Failed to find SpatializerParameterAttribute for parameter {parameter}");
        }

        string label = p.label;
        string description = p.description;
        if (p.isSourceParameter)
        {
            //label += " (initial value)";
            description += "\n\n(This parameter may be modified individually on instantiated audio sources. The value here is the default when creating a new source.)";
        }

        Common3DTIGUI.SingleSpace();

        if (p.type == typeof(float) || p.type == typeof(int))
        {
            toolkit.GetFloatParameter(parameter, out float oldValue);
            float newValue;
            string valueString;
            if (isCompact)
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                GUILayout.Label(new GUIContent(label, description));
                valueString = GUILayout.TextField(oldValue.ToString(p.type == typeof(float) ? "F2" : "F0", System.Globalization.CultureInfo.InvariantCulture), GUILayout.ExpandWidth(false));
                GUILayout.Label(p.units, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                // TODO: I Think this will have a bug where newValue gets overwritten by oldvalue in the parse below
                newValue = GUILayout.HorizontalSlider(oldValue, p.min, p.max);
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                Common3DTIGUI.AddLabelToParameterGroup(label);
                GUILayout.Label(new GUIContent(label, description), Common3DTIGUI.parameterLabelStyle, GUILayout.Width(Common3DTIGUI.GetParameterLabelWidth()));
                newValue = GUILayout.HorizontalSlider(oldValue, p.min, p.max, GUILayout.ExpandWidth(true));
                valueString = GUILayout.TextField(newValue.ToString(p.type == typeof(float) ? "F2" : "F0", System.Globalization.CultureInfo.InvariantCulture), GUILayout.ExpandWidth(false));
                GUILayout.Label(p.units, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
            }

            bool parseOk = float.TryParse(valueString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float parsedValueString);
            if (parseOk)
            {
                newValue = parsedValueString;
            }
            if (p.validValues != null && p.validValues.Length > 0)
            {
                // Lock to nearest valid value
                newValue = p.validValues.OrderBy(x => Math.Abs(x - newValue)).First();
            }

            if (newValue != oldValue)
            {
                toolkit.SetFloatParameter(parameter, newValue);
                return true;
            }
            return false;
        }
        else if (p.type == typeof(bool))
        {
            bool oldValue = toolkit.GetFloatParameter(parameter) != 0.0f;
            bool newValue = GUILayout.Toggle(oldValue, new GUIContent(label, description), GUILayout.ExpandWidth(true));
            if (newValue != oldValue)
            {
                bool setOK = toolkit.SetFloatParameter(parameter, Convert.ToSingle(newValue));
                Debug.Assert(setOK);
                return true;
            }
            return false;
        }
        else if (p.type.IsEnum)
        {
            toolkit.GetFloatParameter(parameter, out float oldFloatValue);
            int oldValue = (int)oldFloatValue;
            Debug.Assert(Enum.GetUnderlyingType(p.type) == typeof(int));
            int[] values = (int[])Enum.GetValues(p.type);
            if (!values.Contains(oldValue))
            {
                Debug.LogWarning($"Plugin returned invalid value for {p.label}: {oldValue}");
            }

            int defaultValue = (int)Enum.GetValues(p.type).GetValue(0);

            int newValue = (int)(object)EditorGUILayout.Popup(new GUIContent(label, description), values.Contains(oldValue) ? oldValue : defaultValue, Enum.GetNames(p.type));
            Debug.Assert(Enum.IsDefined(p.type, newValue));

            if (newValue != oldValue)
            {
                bool setOK = toolkit.SetFloatParameter(parameter, newValue);
                Debug.Assert(setOK);
                return true;
            }
            return false;
        }
        else
        {
            throw new Exception($"Cannot create GUI control for Parameter of type {p.type}.");
        }
    }




    public static string CreateBinaryFileSelector(string currentSelection, string titleText, string tooltip, SpatializerBinaryRole role, TSampleRateEnum sampleRate)
    {
        (string prefix, string[] items, string suffix) = GetBinaryPaths(role, sampleRate);

        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.PrefixLabel(new GUIContent(titleText, tooltip), parameterLabelStyle, GUILayout.Width(GetParameterLabelWidth()));
        int selectedIndex = -1;
        if (currentSelection.Length > prefix.Length + suffix.Length && currentSelection.StartsWith(prefix) && currentSelection.EndsWith(suffix))
        {
            string trimmedTarget = currentSelection.Remove(currentSelection.Length - suffix.Length).Remove(0, prefix.Length);
            selectedIndex = new List<string>(items).IndexOf(trimmedTarget);
        }
        else if (currentSelection != "")
        {
            Debug.LogWarning("Unable to find previously selected binary: " + currentSelection);
        }
        int newSelectedIndex = EditorGUILayout.Popup(new GUIContent(titleText, tooltip), selectedIndex, items);
        EditorGUILayout.EndHorizontal();
        return newSelectedIndex < 0 ? "" : (prefix + items[newSelectedIndex] + suffix);
    }




    ///////////////////////////////////////////////////////////
    // GUI ELEMENT ACTIONS
    ///////////////////////////////////////////////////////////




    /// <summary>
    /// Action for slider HeadRadius
    /// </summary>
    public void SliderHeadRadius()
    {
        //toolkit.SetFloatParameter(API_3DTI_Spatializer.FloatParameter.PARAM_HEAD_RADIUS, )
        //toolkit.SetHeadRadius(toolkit.listenerHeadRadius);
    }

    /// <summary>
    /// Action for input ResamplingStep
    /// </summary>
    public void InputResamplingStep()
    {
        //toolkit.SetHRTFResamplingStep(toolkit.HRTFstep);
    }

    /// <summary>
    /// Action for slider Scale
    /// </summary>
    public void SliderScale()
    {        
        //toolkit.SetScaleFactor(toolkit.scaleFactor);
    }

    /// <summary>
    /// Action for slider Anechoic Attenuation
    /// </summary>
    public void SliderAnechoicAttenuation()
    {
        //toolkit.SetMagnitudeAnechoicAttenuation(toolkit.magAnechoicAttenuation);
    }

    /// <summary>
    /// Action for slider Sound Speed
    /// </summary>
    public void SliderSoundSpeed()
    {
        //toolkit.SetMagnitudeSoundSpeed(toolkit.magSoundSpeed);
    }

    /// <summary>
    ///  Action for slider HA Directionality Left
    /// </summary>
    public void SliderHADirectionalityLeft()
    {
        //toolkit.SetHADirectionalityExtend(T_ear.LEFT, toolkit.HADirectionalityExtendLeft);
    }

    /// <summary>
    ///  Action for slider HA Directionality Right 
    /// </summary>
    public void SliderHADirectionalityRight()
    {
        //toolkit.SetHADirectionalityExtend(T_ear.RIGHT, toolkit.HADirectionalityExtendRight);
    }


    ///////////////////////////////////////////////////////////
    // PANEL CONTENTS
    ///////////////////////////////////////////////////////////

    /// <summary>
    /// Draw panel with Listener configuration
    /// </summary>
    public void DrawControls()
    {
        Common3DTIGUI.BeginSection("DEFAULT SETTINGS FOR NEW SOUND SOURCES");

        GUILayout.Label("These parameters may be set individually on each individual AudioSource component. The values here determine their default values for new AudioSources.", Common3DTIGUI.commentStyle );

        CreateControl(SpatializerParameter.PARAM_SPATIALIZATION_MODE);

        Common3DTIGUI.SingleSpace();

        perSourceAdvancedSetup = Common3DTIGUI.CreateFoldoutToggle(ref perSourceAdvancedSetup, "Advanced");
        if (perSourceAdvancedSetup)
        {
            Common3DTIGUI.BeginSection();

            //GUILayout.BeginHorizontal();
            CreateControl(SpatializerParameter.PARAM_HRTF_INTERPOLATION);
            CreateControl(SpatializerParameter.PARAM_MOD_FARLPF);
            CreateControl(SpatializerParameter.PARAM_MOD_DISTATT);
            // For High Quality only
            CreateControl(SpatializerParameter.PARAM_MOD_NEAR_FIELD_ILD);
            //GUILayout.EndHorizontal();

            Common3DTIGUI.EndSection();

        }

        Common3DTIGUI.EndSection();



        Common3DTIGUI.BeginSection("LISTENER SETUP");

        // HIGH PERFORMANCE / HIGH QUALITY CHOICE:

        void createDropdowns(SpatializerBinaryRole role, string label, string tooltip)
        {
            (TSampleRateEnum, string)[] AllSampleRates = {
                (TSampleRateEnum.K44, "44.1 kHz"),
                (TSampleRateEnum.K48, "48 kHz"),
                (TSampleRateEnum.K96, "96 kHz")
            };
            foreach ((TSampleRateEnum sampleRate, string sampleRateLabel) in AllSampleRates)
            {
                // Paths should be relative to a Resources folder.
                string oldPath = toolkit.GetBinaryPath(role, sampleRate);
                string newPath = CreateBinaryFileSelector(oldPath, label + " " +sampleRateLabel, tooltip, role, sampleRate);
                if (oldPath != newPath)
                {
                    toolkit.SetBinaryPath(role, sampleRate, newPath);
                }
                if (newPath.EndsWith(".sofa.bytes"))
                {
                    Debug.Log("NB: SOFA HRTF files are only supported on Windows x64.");
                }
            }
        }

        Common3DTIGUI.SingleSpace();

        GUILayout.Label("Binaries for High Performance mode", Common3DTIGUI.subtitleBoxStyle);
        GUILayout.Label("These are required for AudioSources to be able to spatialize in High Performance mode.", Common3DTIGUI.commentStyle);

        

        // HIGH PERFORMANCE MODE CONTROLS
        {            
            Common3DTIGUI.AddLabelToParameterGroup("High Performance ILD");

            createDropdowns(SpatializerBinaryRole.HighPerformanceILD, "ILD", "Select the high performance ILD filter of the listener from a .3dti-ild file");

            //Common3DTIGUI.CreatePopupStringSelector("ILD 44.1kHz", "Select the high performance ILD filter of the listener from a .3dti-ild file", GetFilterBinaryPaths(TSampleRateEnum.K44).highPerformanceILDs, ref toolkit.ILDHighPerformanceFileName44, "Data/HighPerformance/ILD/", ".bytes");
            //Common3DTIGUI.CreatePopupStringSelector("ILD 48kHz", "Select the high performance ILD filter of the listener from a .3dti-ild file", GetFilterBinaryPaths(TSampleRateEnum.K48).highPerformanceILDs, ref toolkit.ILDHighPerformanceFileName48, "Data/HighPerformance/ILD/", ".bytes");
            //Common3DTIGUI.CreatePopupStringSelector("ILD 96kHz", "Select the high performance ILD filter of the listener from a .3dti-ild file", GetFilterBinaryPaths(TSampleRateEnum.K96).highPerformanceILDs, ref toolkit.ILDHighPerformanceFileName96, "Data/HighPerformance/ILD/", ".bytes");
        }

        Common3DTIGUI.SectionSpace();
        GUILayout.Label("Binaries for High Quality mode", Common3DTIGUI.subtitleBoxStyle);
        GUILayout.Label("These are required for AudioSources to be able to spatialize in High Quality mode.", Common3DTIGUI.commentStyle);

        // HIGH QUALITY MODE CONTROLS
        {
            Common3DTIGUI.AddLabelToParameterGroup("HRTF");
            Common3DTIGUI.AddLabelToParameterGroup("Near Field Filter ILD");


            // HRTF:
            //Common3DTIGUI.CreatePopupStringSelector("HRTF 44.1kHz", "Select the HRTF of the listener from a .3dti-hrtf file", GetFilterBinaryPaths(TSampleRateEnum.K44).highQualityHRTFs, ref toolkit.HRTFFileName44, "Data/HighQuality/HRTF/", ".bytes");
            //Common3DTIGUI.CreatePopupStringSelector("HRTF 48kHz", "Select the HRTF of the listener from a .3dti-hrtf file", GetFilterBinaryPaths(TSampleRateEnum.K48).highQualityHRTFs, ref toolkit.HRTFFileName48, "Data/HighQuality/HRTF/", ".bytes");
            //Common3DTIGUI.CreatePopupStringSelector("HRTF 96kHz", "Select the HRTF of the listener from a .3dti-hrtf file", GetFilterBinaryPaths(TSampleRateEnum.K96).highQualityHRTFs, ref toolkit.HRTFFileName96, "Data/HighQuality/HRTF/", ".bytes");
            createDropdowns(SpatializerBinaryRole.HighQualityHRTF, "HRTF", "Select the HRTF of the listener from a .3dti-hrtf file");

            // ILD:

            Common3DTIGUI.SingleSpace();
            //Common3DTIGUI.CreatePopupStringSelector("ILD 44.1kHz", "Select the ILD near field filter of the listener from a .3dti-ild file", GetFilterBinaryPaths(TSampleRateEnum.K44).highQualityILDs, ref toolkit.ILDNearFieldFileName44, "Data/HighQuality/ILD/", ".bytes");
            //Common3DTIGUI.CreatePopupStringSelector("ILD 48kHz", "Select the ILD near field filter of the listener from a .3dti-ild file", GetFilterBinaryPaths(TSampleRateEnum.K48).highQualityILDs, ref toolkit.ILDNearFieldFileName48, "Data/HighQuality/ILD/", ".bytes");
            //Common3DTIGUI.CreatePopupStringSelector("ILD 96kHz", "Select the ILD near field filter of the listener from a .3dti-ild file", GetFilterBinaryPaths(TSampleRateEnum.K96).highQualityILDs, ref toolkit.ILDNearFieldFileName96, "Data/HighQuality/ILD/", ".bytes");
            createDropdowns(SpatializerBinaryRole.HighQualityILD, "ILD", "Select the ILD near field filter of the listener from a .3dti-ild file");


        }


        Common3DTIGUI.SectionSpace();

        // BRIR Reverb:
        GUILayout.Label("Binaries for Reverb", Common3DTIGUI.subtitleBoxStyle);
        GUILayout.Label("These are required to enable reverb processing.", Common3DTIGUI.commentStyle);

        //Common3DTIGUI.CreatePopupStringSelector("BRIR 44.1kHz", "Select the BRIR (impulse response) for reverb processing", GetFilterBinaryPaths(TSampleRateEnum.K44).reverbBRIRs, ref toolkit.BRIRFileName44, "Data/Reverb/BRIR/", ".bytes");
        //Common3DTIGUI.CreatePopupStringSelector("BRIR 48kHz", "Select the BRIR (impulse response) for reverb processing", GetFilterBinaryPaths(TSampleRateEnum.K48).reverbBRIRs, ref toolkit.BRIRFileName48, "Data/Reverb/BRIR/", ".bytes");
        //Common3DTIGUI.CreatePopupStringSelector("BRIR 96kHz", "Select the BRIR (impulse response) for reverb processing", GetFilterBinaryPaths(TSampleRateEnum.K96).reverbBRIRs, ref toolkit.BRIRFileName96, "Data/Reverb/BRIR/", ".bytes");
        createDropdowns(SpatializerBinaryRole.ReverbBRIR, "BRIR", "Select the BRIR (impulse response) for reverb processing");



        Common3DTIGUI.SectionSpace();

        // ITD:    
        {
            CreateControl(SpatializerParameter.PARAM_CUSTOM_ITD);
            if (toolkit.GetFloatParameter(SpatializerParameter.PARAM_CUSTOM_ITD) != 0.0f)
            {
                CreateControl(SpatializerParameter.PARAM_HEAD_RADIUS);
            }
        }

        Common3DTIGUI.SectionSpace();


        advancedSetup = Common3DTIGUI.CreateFoldoutToggle(ref advancedSetup, "Advanced Listener settings");
        if (advancedSetup)
        {
            Common3DTIGUI.BeginSection();

            Common3DTIGUI.AddLabelToParameterGroup("Scale factor");
            Common3DTIGUI.SingleSpace();
            CreateControl(SpatializerParameter.PARAM_SCALE_FACTOR);

            // HRTF interpolation
            Common3DTIGUI.BeginSubsection("HRTF Interpolation");
            Common3DTIGUI.AddLabelToParameterGroup("Runtime interpolation");
            Common3DTIGUI.AddLabelToParameterGroup("Resampling step");
            CreateControl(SpatializerParameter.PARAM_HRTF_STEP);
            Common3DTIGUI.EndSubsection();


            // Magnitudes
            Common3DTIGUI.BeginSubsection("Physical magnitudes");
            Common3DTIGUI.AddLabelToParameterGroup("Anechoic distance attenuation");
            Common3DTIGUI.AddLabelToParameterGroup("Sound speed");
            CreateControl(SpatializerParameter.PARAM_MAG_ANECHATT);
            CreateControl(SpatializerParameter.PARAM_MAG_SOUNDSPEED);
            Common3DTIGUI.EndSubsection();

            // Limiter
            Common3DTIGUI.BeginSubsection("Limiter");
            Common3DTIGUI.AddLabelToParameterGroup("Switch Limiter");
            CreateControl(SpatializerParameter.PARAM_LIMITER_SET_ON);
            Common3DTIGUI.EndSubsection();

            //// Debug Log
            //Common3DTIGUI.BeginSubsection("Debug log");
            //Common3DTIGUI.AddLabelToParameterGroup("Write debug log file");
            //    if (Common3DTIGUI.CreateToggle(ref toolkit.debugLog, "Write debug log file", "Enable writing of 3DTi_BinauralSpatializer_DebugLog.txt file in your project root folder; This file can be sent to the 3DTi Toolkit developers for support", isStartingPlay))
            //        toolkit.SendWriteDebugLog(toolkit.debugLog);
            //Common3DTIGUI.EndSubsection();

            Common3DTIGUI.EndSection();
        }


        Common3DTIGUI.SectionSpace();

        haSetup = Common3DTIGUI.CreateFoldoutToggle(ref haSetup, "Hearing Aid Directionality Setup");
        if (haSetup)
        {
            Common3DTIGUI.BeginSection();


            Common3DTIGUI.SingleSpace();

            // Left ear
            Common3DTIGUI.BeginSubsection("Left ear");
            Common3DTIGUI.AddLabelToParameterGroup("Switch Directionality");
            Common3DTIGUI.AddLabelToParameterGroup("Directionality extend");
            CreateControl(SpatializerParameter.PARAM_HA_DIRECTIONALITY_ON_LEFT);
            CreateControl(SpatializerParameter.PARAM_HA_DIRECTIONALITY_EXTEND_LEFT);
            Common3DTIGUI.EndSubsection();

            // Right ear
            Common3DTIGUI.BeginSubsection("Right ear");
            Common3DTIGUI.AddLabelToParameterGroup("Switch Directionality");
            Common3DTIGUI.AddLabelToParameterGroup("Directionality extend");
            CreateControl(SpatializerParameter.PARAM_HA_DIRECTIONALITY_ON_RIGHT);
            CreateControl(SpatializerParameter.PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT);
            Common3DTIGUI.EndSubsection();
            Common3DTIGUI.EndSection();
        }
        Common3DTIGUI.EndSection();

    }

}