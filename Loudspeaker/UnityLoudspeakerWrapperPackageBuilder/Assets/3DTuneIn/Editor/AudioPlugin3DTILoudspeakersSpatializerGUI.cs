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


[CustomEditor(typeof(API_3DTI_LoudSpeakersSpatializer))]
public class AudioPlugin3DTILoudspeakerSpatializerGUI : Editor
{
    API_3DTI_LoudSpeakersSpatializer toolkit;
    bool advancedSetup = false;

    // Limit possible values of sliders   
    float minScale = 0.1f;
    float maxScale = 10.0f;
    float minStructSide = 1.25f;
    float maxStructSide = 3.0f;
    float minStructureYawPitch = -180.0f;
    float maxStructureYawPitch = 180.0f;
    float minDB = -30.0f;
    float maxDB = 0.0f;
    float maxSoundSpeed = 1000.0f;
    

    // Editor view look
    //float spaceBetweenSections = 15.0f;
    //float singleSpace = 5.0f;
    //GUIStyle titleBoxStyle = null;
    //GUIStyle subtitleBoxStyle;
    //GUIStyle sectionStyle;
    //GUIStyle subsectionStyle;
    //GUIStyle dragdropStyle;

    //////////////////////////////////////////////////////////////////////////////

    void Start()
    {
        // Get access to API script
        //toolkit = (API_3DTI_LoudSpeakersSpatializer)target;
        //toolkit.debugLog = false;
    }

    /// <summary>
    /// This is where we create the layout for the inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Get access to API script
        toolkit = (API_3DTI_LoudSpeakersSpatializer)target;

        // Init styles
        Common3DTIGUI.InitStyles();

        //// Init styles
        //if (titleBoxStyle == null)
        //{
        //    titleBoxStyle = new GUIStyle(GUI.skin.box);
        //    titleBoxStyle.normal.textColor = Color.white;
        //}
        //subtitleBoxStyle = EditorStyles.label;
        //sectionStyle = EditorStyles.miniButton;
        //subsectionStyle = EditorStyles.miniButton;
        //dragdropStyle = EditorStyles.textField;

        //// Show 3D-Tune-In logo         
        //Texture logo3DTI;
        //GUIStyle logoStyle = EditorStyles.largeLabel;
        //logoStyle.alignment = TextAnchor.MiddleCenter;
        //logo3DTI = Resources.Load("logo3DTI_") as Texture;
        //GUILayout.Box(logo3DTI, logoStyle, GUILayout.ExpandWidth(true));

        // Show 3D-Tune-In logo         
        Common3DTIGUI.Show3DTILogo();

        // Show About button
        Common3DTIGUI.ShowAboutButton();

        // SPEAKERS CONFIGURATION
        DrawSpeakersConfigurationPanel();

         // ADVANCED SETUP
        DrawAdvancedPanel();

    }


    ///////////////////////////////////////////////////////////
    // GUI ELEMENT ACTIONS
    ///////////////////////////////////////////////////////////

    /// <summary>
    /// Action to do when a new file has been selected (either for HRTF or ILD, and either from button or drag&drop)
    /// </summary>
    public void ChangeFileName(ref string whichfilename, string newname)
    {
        // Set new name for toolkit API
        whichfilename = newname;

    //#if UNITY_ANDROID
        // Save it in resources as .byte                
        string newnamewithpath = "Assets/Resources/" + Path.GetFileNameWithoutExtension(newname) + ".bytes";
        if (!File.Exists(newnamewithpath)) 
            FileUtil.CopyFileOrDirectory(whichfilename, newnamewithpath);
    //#endif        
	}


    /// <summary>
    /// Action for slider Sample Rate
    /// </summary>
    public void SliderSampleRate()
    {
        //Debug.Log("Slider sample rate changed");
    }

    /// <summary>
    /// Action for slider Buffer Size
    /// </summary>
    public void SliderBufferSize()
    {
        //Debug.Log("Slider buffer size changed");
    }

    /// <summary>
    /// Action for slider Scale
    /// </summary>
    public void SliderScale()
    {
        //Debug.Log("Slider scale changed");
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
    /// Action for slider Anechoic Attenuation
    /// </summary>
    public void SliderSpeakersConfiguration()
    {
        toolkit.SetupSpeakersConfiguration(toolkit.structureSide);
    }

    ///////////////////////////////////////////////////////////
    // PANEL CONTENTS
    ///////////////////////////////////////////////////////////
    /// <summary>
    /// Draw panel with advanced configuration
    /// </summary>
    public void DrawSpeakersConfigurationPanel()
    {
        Common3DTIGUI.BeginSection("SPEAKER POSITIONS CONFIGURATION");       
            Common3DTIGUI.BeginSubsection("Speakers cube structure");        
                float previousStructureSize = toolkit.structureSide;
                float structureSide = EditorGUILayout.FloatField("Cube side size (m): ", toolkit.structureSide);
                toolkit.structureSide = Mathf.Clamp(structureSide, minStructSide, maxStructSide);
                if (previousStructureSize != toolkit.structureSide) { SliderSpeakersConfiguration(); }
                //TODO: minDistance to the listener value should come from the toolkit. Due to the current GIU allows the user introduce just the speakers structure side, the minimun distance can be calculated from that size
                float minDistanceToListener = Mathf.Sqrt(3) * 0.5f * toolkit.structureSide;
                GUILayout.Label("Minimun distance between listener and source: " + minDistanceToListener + " m");
            Common3DTIGUI.EndSubsection();
            Common3DTIGUI.BeginSubsection("Speakers Fine Adjustment");
                Common3DTIGUI.SingleSpace();
                GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                        ShowSpeakerPosition("Spk1", toolkit.speaker1Offset, toolkit.speaker1Position);
                        ShowSpeakerPosition("Spk2", toolkit.speaker2Offset, toolkit.speaker2Position);
                        ShowSpeakerPosition("Spk3", toolkit.speaker3Offset, toolkit.speaker3Position);
                        ShowSpeakerPosition("Spk4", toolkit.speaker4Offset, toolkit.speaker4Position);
                        ShowSpeakerPosition("Spk5", toolkit.speaker5Offset, toolkit.speaker5Position);
                        ShowSpeakerPosition("Spk6", toolkit.speaker6Offset, toolkit.speaker6Position);
                        ShowSpeakerPosition("Spk7", toolkit.speaker7Offset, toolkit.speaker7Position);
                        ShowSpeakerPosition("Spk8", toolkit.speaker8Offset, toolkit.speaker8Position);       
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                        GUILayout.Label("Offset (cm):");
                        GUILayout.Label("Offset (cm):");
                        GUILayout.Label("Offset (cm):");
                        GUILayout.Label("Offset (cm):");
                        GUILayout.Label("Offset (cm):");
                        GUILayout.Label("Offset (cm):");
                        GUILayout.Label("Offset (cm):");
                        GUILayout.Label("Offset (cm):");
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                        ShowSpeakerOffsetControl(ref toolkit.speaker1Offset, toolkit.speaker1Position);
                        ShowSpeakerOffsetControl(ref toolkit.speaker2Offset, toolkit.speaker2Position);
                        ShowSpeakerOffsetControl(ref toolkit.speaker3Offset, toolkit.speaker3Position);
                        ShowSpeakerOffsetControl(ref toolkit.speaker4Offset, toolkit.speaker4Position);
                        ShowSpeakerOffsetControl(ref toolkit.speaker5Offset, toolkit.speaker5Position);
                        ShowSpeakerOffsetControl(ref toolkit.speaker6Offset, toolkit.speaker6Position);
                        ShowSpeakerOffsetControl(ref toolkit.speaker7Offset, toolkit.speaker7Position);
                        ShowSpeakerOffsetControl(ref toolkit.speaker8Offset, toolkit.speaker8Position);
                    GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            Common3DTIGUI.EndSubsection();
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

            // Mod enabler            
            Common3DTIGUI.BeginSubsection("Switch Spatialization Effects");
            Common3DTIGUI.AddLabelToParameterGroup("Far distance LPF");
            Common3DTIGUI.AddLabelToParameterGroup("Distance attenuation");
            if (Common3DTIGUI.CreateToggle(ref toolkit.modFarLPF, "Far distance LPF", "Enable low pass filter to simulate sound coming from far distances"))
                toolkit.SetModFarLPF(toolkit.modFarLPF);
            if (Common3DTIGUI.CreateToggle(ref toolkit.modDistAtt, "Distance attenuation", "Enable attenuation of sound depending on distance to listener"))
                toolkit.SetModDistanceAttenuation(toolkit.modDistAtt);
            Common3DTIGUI.EndSubsection();

            // Magnitudes            
            Common3DTIGUI.BeginSubsection("Physical magnitudes");
            Common3DTIGUI.AddLabelToParameterGroup("Anechoic distance attenuation");
            Common3DTIGUI.AddLabelToParameterGroup("Sound speed");
            Common3DTIGUI.CreateFloatSlider(ref toolkit.magAnechoicAttenuation, "Anechoic distance attenuation", "F2", "dB", "Set attenuation in decibels for each double distance", minDB, maxDB, SliderAnechoicAttenuation);
            Common3DTIGUI.CreateFloatSlider(ref toolkit.magSoundSpeed, "Sound speed", "F0", "m/s", "Set sound speed, used for ITD computation", 0.0f, maxSoundSpeed, SliderSoundSpeed);
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


    void ShowSpeakerPosition(string label, Vector3 speakerOffset, Vector3 speakerPosition)
    {
        GUILayout.Label(label +" " + (speakerPosition + speakerOffset * 0.01f).ToString("f2") + " m");
    }

    void ShowSpeakerOffsetControl(ref Vector3 speakerOffset, Vector3 speakerPosition)
    {
        Vector3 previousSpeakerOffset = speakerOffset;
        Vector3 temp = EditorGUILayout.Vector3Field("", speakerOffset);
        speakerOffset = CalculateSpeakerOffset(temp, speakerPosition);
        if (previousSpeakerOffset != speakerOffset) { SliderSpeakersConfiguration(); }
    }


    Vector3 CalculateSpeakerOffset(Vector3 speakerOffset, Vector3 speakerPosition )
    {        
        return new Vector3(Mathf.Clamp(speakerOffset.x, Mathf.Abs(speakerPosition.x) * -0.2f * 100.0f, Mathf.Abs(speakerPosition.x) * 0.2f * 100.0f), Mathf.Clamp(speakerOffset.y, Mathf.Abs(speakerPosition.y) * -0.2f * 100.0f, Mathf.Abs(speakerPosition.y) * 0.2f * 100.0f), Mathf.Clamp(speakerOffset.z, Mathf.Abs(speakerPosition.z) * -0.2f * 100.0f, Mathf.Abs(speakerPosition.z) * 0.2f * 100.0f));       
    }
}