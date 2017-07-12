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

        // Magnitudes
        Common3DTIGUI.BeginSubsection("Speakers cube structure");
        //Common3DTIGUI.CreateFloatSlider(ref toolkit.structureSide, "Cube side size:", "F2", "meters", "", minStructSide, maxStructSide, SliderSpeakersConfiguration);        

        //GUILayout.BeginVertical("");
        float previousStructureSize = toolkit.structureSide;
        float structureSide = EditorGUILayout.FloatField("Cube side size (m): ", toolkit.structureSide);
        toolkit.structureSide = Mathf.Clamp(structureSide, minStructSide, maxStructSide);
        if (previousStructureSize != toolkit.structureSide) { SliderSpeakersConfiguration(); }


        //TODO: minDistance to the listener value should come from the toolkit. Due to the current GIU allows the user introduce just the speakers structure side, the minimun distance can be calculated from that size
        float minDistanceToListener = Mathf.Sqrt(3) * 0.5f * toolkit.structureSide;
        GUILayout.Label("Minimun distance between listener and source: " + minDistanceToListener + " m");
        //GUILayout.EndVertical();

        Common3DTIGUI.EndSubsection();

        Common3DTIGUI.BeginSubsection("Speakers Fine Adjustment");
        Common3DTIGUI.SingleSpace();

        GUILayout.BeginHorizontal("");
        GUILayout.BeginVertical("");
        GUILayout.Label("Spk1 " + toolkit.speaker1Position.ToString("f2") + " m");
        GUILayout.Label("Spk2 " + toolkit.speaker2Position.ToString("f2") + " m");
        GUILayout.Label("Spk3 " + toolkit.speaker1Position.ToString("f2") + " m");
        GUILayout.Label("Spk4 " + toolkit.speaker2Position.ToString("f2") + " m");
        GUILayout.Label("Spk5 " + toolkit.speaker1Position.ToString("f2") + " m");
        GUILayout.Label("Spk6 " + toolkit.speaker2Position.ToString("f2") + " m");
        GUILayout.Label("Spk7 " + toolkit.speaker1Position.ToString("f2") + " m");
        GUILayout.Label("Spk8 " + toolkit.speaker2Position.ToString("f2") + " m");
        GUILayout.EndVertical();
        GUILayout.BeginVertical("");
        GUILayout.Label("Offset (cm):");
        GUILayout.Label("Offset (cm):");
        GUILayout.Label("Offset (cm):");
        GUILayout.Label("Offset (cm):");
        GUILayout.Label("Offset (cm):");
        GUILayout.Label("Offset (cm):");
        GUILayout.Label("Offset (cm):");
        GUILayout.Label("Offset (cm):");
        GUILayout.EndVertical();

        GUILayout.BeginVertical("");

        Vector3 temp = EditorGUILayout.Vector3Field("", toolkit.speaker1Offset);        
        toolkit.speaker1Offset = CalculateSpeakerOffset(temp, toolkit.speaker1Position);
        temp = EditorGUILayout.Vector3Field("", toolkit.speaker2Offset);
        toolkit.speaker2Offset = CalculateSpeakerOffset(temp, toolkit.speaker2Position);

        temp = EditorGUILayout.Vector3Field("", toolkit.speaker3Offset);
        toolkit.speaker3Offset = CalculateSpeakerOffset(temp, toolkit.speaker3Position);

        temp = EditorGUILayout.Vector3Field("", toolkit.speaker4Offset);
        toolkit.speaker4Offset = CalculateSpeakerOffset(temp, toolkit.speaker4Position);

        temp = EditorGUILayout.Vector3Field("", toolkit.speaker5Offset);
        toolkit.speaker5Offset = CalculateSpeakerOffset(temp, toolkit.speaker5Position);

        temp = EditorGUILayout.Vector3Field("", toolkit.speaker6Offset);
        toolkit.speaker6Offset = CalculateSpeakerOffset(temp, toolkit.speaker6Position);

        temp = EditorGUILayout.Vector3Field("", toolkit.speaker7Offset);
        toolkit.speaker7Offset = CalculateSpeakerOffset(temp, toolkit.speaker7Position);

        temp = EditorGUILayout.Vector3Field("", toolkit.speaker8Offset);
        toolkit.speaker8Offset = CalculateSpeakerOffset(temp, toolkit.speaker8Position);
        
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


    /////////////////////////////////////////////////////////////
    //// AUXILIARY FUNCTIONS FOR CREATING FORMATTED GUI ELEMENTS
    /////////////////////////////////////////////////////////////

    ///// <summary>
    ///// Auxiliary function for creating sliders for float variables with specific format
    ///// </summary>
    ///// <returns></returns>
    //public void CreateFloatSlider(ref float variable, string name, string decimalDigits, string units, float minValue, float maxValue, System.Action action)
    //{
    //    SingleSpace();
      
    //    GUILayout.BeginHorizontal("");
    //    GUILayout.Label(name);
    //    //EditorGUILayout.FloatField();
    //    //GUILayout.EndVertical();
    //    //GUILayout.BeginVertical("");
    //    string valueString = GUILayout.TextField(variable.ToString(decimalDigits), GUILayout.ExpandWidth(false));
    //    //GUILayout.EndVertical();
    //    //GUILayout.BeginVertical("");
    //    GUILayout.Label(units, GUILayout.ExpandWidth(false));
    //    GUILayout.EndHorizontal();

    //    //GUILayout.BeginVertical("");
    //    //GUILayout.Label(name + " " + variable.ToString(decimalDigits) + " " +units);
    //    //GUILayout.EndVertical();
      
    //    GUILayout.BeginVertical("");
    //    float newValue;
    //    bool valid = float.TryParse(valueString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out newValue);
    //    if (valid) { variable = newValue; }
    //    float previousVar = variable;
    //    variable = GUILayout.HorizontalSlider(variable, minValue, maxValue);
    //    if (variable != previousVar) { action(); }

    //    GUILayout.EndVertical();


      
    //}

    
    
     
    ///// <summary>
    ///// Auxiliary function for creating sliders for int variables with predefined possible values with specific format
    ///// </summary>
    //public void CreateSnapSlider(ref int variable, string name, GUIContent content, List<int> values, System.Action action)
    //{
    //    SingleSpace();

    //    int minValue = values[0];
    //    int maxValue = values[values.Count - 1];
    //    GUILayout.BeginHorizontal();
    //    GUILayout.Label(name);
    //    GUILayout.Label(content, GUILayout.ExpandWidth(false));
    //    GUILayout.EndHorizontal();
    //    int previousVar = variable;
    //    variable = (int)GUILayout.HorizontalSlider((float)variable, (float)minValue, (float)maxValue);

    //    int possibleValue = minValue;
    //    foreach (int snapValue in values)
    //    {
    //        if (variable < snapValue)
    //        {
    //            variable = possibleValue;
    //            break;
    //        }
    //        else
    //            possibleValue = snapValue;
    //    }

    //    if (variable != previousVar)
    //        action();        
    //}

    ///// <summary>
    ///// Auxiliary function for creating a new section with title for parameter groups
    ///// </summary>    
    //public void BeginSection(string titleText)
    //{
    //    GUILayout.BeginVertical(sectionStyle);        
    //    GUILayout.Box(titleText, titleBoxStyle, GUILayout.ExpandWidth(true));
    //}

    ///// <summary>
    ///// Auxiliary function for ending section of parameter group
    ///// </summary>
    //public void EndSection()
    //{
    //    GUILayout.EndVertical();
    //    //GUILayout.Label("");    // Line spacing        
    //    GUILayout.Space(spaceBetweenSections);          // Line spacing        
    //}

    ///// <summary>
    ///// Auxiliary function for creating a new subsection with title for parameter groups within one section
    ///// </summary>    
    //public void BeginSubsection(string titleText)
    //{
    //    GUILayout.BeginVertical(subsectionStyle);        
    //    GUILayout.Box(titleText, subtitleBoxStyle, GUILayout.ExpandWidth(false));
    //}

    ///// <summary>
    ///// Auxiliary function for ending subsection for parameter groups within one section
    ///// </summary>
    //public void EndSubsection()
    //{
    //    GUILayout.EndVertical();        
    //    GUILayout.Space(singleSpace);          // Line spacing        
    //}

    ///// <summary>
    ///// Auxiliary function for creating a drag&drop text box
    ///// </summary>
    //public void CreateDragDropBox(ref string textvar)
    //{        
    //    Event evt = Event.current;
    //    Rect drop_area = GUILayoutUtility.GetRect(0.0f, 20.0f, GUILayout.ExpandWidth(true));
    //    drop_area.y += 14.0f;
    //    GUI.Box(drop_area, textvar, dragdropStyle);
        
    //    switch (evt.type)
    //    {
    //        case EventType.DragUpdated:
    //        case EventType.DragPerform:
    //            if (!drop_area.Contains(evt.mousePosition))
    //                return;

    //            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

    //            if (evt.type == EventType.DragPerform)
    //            {
    //                DragAndDrop.AcceptDrag();
    //                ChangeFileName(ref textvar, DragAndDrop.paths[0]); 
    //            }
    //            break;
    //    }
    //}

    ///// <summary>
    /////  Auxiliary function for creating toogle input
    ///// </summary>    
    //public bool CreateToggle(ref bool boolvar, string toggleText)
    //{
    //    bool oldvar = boolvar;
    //    boolvar = GUILayout.Toggle(boolvar, toggleText, GUILayout.ExpandWidth(false));
    //    return (oldvar != boolvar);
    //}

    ///// <summary>
    ///// Put a single horizontal space
    ///// </summary>
    //public void SingleSpace()
    //{
    //    GUILayout.Space(singleSpace);
    //}

    Vector3 CalculateSpeakerOffset(Vector3 speakerOffset, Vector3 speakerPosition )
    {        
        return new Vector3(Mathf.Clamp(speakerOffset.x, Mathf.Abs(speakerPosition.x) * -0.2f * 100.0f, Mathf.Abs(speakerPosition.x) * 0.2f * 100.0f), Mathf.Clamp(speakerOffset.y, Mathf.Abs(speakerPosition.y) * -0.2f * 100.0f, Mathf.Abs(speakerPosition.y) * 0.2f * 100.0f), Mathf.Clamp(speakerOffset.z, Mathf.Abs(speakerPosition.z) * -0.2f * 100.0f, Mathf.Abs(speakerPosition.z) * 0.2f * 100.0f));       
    }
}