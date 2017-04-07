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

    //List<int> sampleRateValues = new List<int>() { 11025, 22050, 44100, 48000, 88200, 96000, 112000, 192000 };    
    //List<int> bufferSizeValues = new List<int>() { 64, 128, 256, 512, 1024, 2048 };    

    // Editor view look
    float spaceBetweenSections = 15.0f;
    float singleSpace = 5.0f;
    GUIStyle titleBoxStyle = null;
    GUIStyle subtitleBoxStyle;
    GUIStyle sectionStyle;
    GUIStyle subsectionStyle;
    GUIStyle dragdropStyle;

    //////////////////////////////////////////////////////////////////////////////

    void Start()
    {
        // Get access to API script
        toolkit = (API_3DTI_LoudSpeakersSpatializer)target;
        toolkit.debugLog = false;
    }

    /// <summary>
    /// This is where we create the layout for the inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Get access to API script
        toolkit = (API_3DTI_LoudSpeakersSpatializer)target;

        // Init styles
        if (titleBoxStyle == null)
        {
            titleBoxStyle = new GUIStyle(GUI.skin.box);
            titleBoxStyle.normal.textColor = Color.white;                      
        }
        subtitleBoxStyle = EditorStyles.label;
        sectionStyle = EditorStyles.miniButton;
        subsectionStyle = EditorStyles.miniButton;
        dragdropStyle = EditorStyles.textField;

        // Show 3D-Tune-In logo         
        Texture logo3DTI;
        GUIStyle logoStyle = EditorStyles.largeLabel;
        logoStyle.alignment = TextAnchor.MiddleCenter;
        logo3DTI = Resources.Load("logo3DTI_") as Texture;
        GUILayout.Box(logo3DTI, logoStyle, GUILayout.ExpandWidth(true));

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
        BeginSection("SPEAKERS CONFIGURATION:");

            // Magnitudes
            BeginSubsection("");
            CreateFloatSlider(ref toolkit.structureSide,    "Structure side size:", "F2", "meters", minStructSide, maxStructSide, SliderSpeakersConfiguration);
            //CreateFloatSlider(ref toolkit.structureYaw,     "Structure yaw:", "F2", "degrees", minStructureYawPitch, maxStructureYawPitch, SliderSpeakersConfiguration);
            //CreateFloatSlider(ref toolkit.structurePitch,   "Structure pitch:", "F0", "degrees", minStructureYawPitch, maxStructureYawPitch, SliderSpeakersConfiguration);
            EndSubsection();

        EndSection();
    }


    /// <summary>
    /// Draw panel with advanced configuration
    /// </summary>
    public void DrawAdvancedPanel()
    {
        BeginSection("ADVANCED SETUP:");

        advancedSetup = GUILayout.Toggle(advancedSetup, "Show Advanced Setup", GUILayout.ExpandWidth(false));
        if (advancedSetup)
        {
            // Scale factor slider
            SingleSpace();
            CreateFloatSlider(ref toolkit.scaleFactor, "Scale factor:", "F2", " meters = 1.0 unit in Unity", minScale, maxScale, SliderScale);
            // Mod enabler
            BeginSubsection("Modules enabler:");
            if (CreateToggle(ref toolkit.modFarLPF, "Far LPF"))
                toolkit.SetModFarLPF(toolkit.modFarLPF);
            if (CreateToggle(ref toolkit.modDistAtt, "Distance attenuation"))
                toolkit.SetModDistanceAttenuation(toolkit.modDistAtt);
            EndSubsection();

            // Magnitudes
            BeginSubsection("Physical magnitudes:");
            CreateFloatSlider(ref toolkit.magAnechoicAttenuation, "Anechoic distance attenuation:", "F2", "dB", minDB, maxDB, SliderAnechoicAttenuation);
            //CreateFloatSlider(ref toolkit.magReverbAttenuation, "Reverb distance attenuation:", "F2", "dB", minDB, maxDB, SliderReverbAttenuation);
            //CreateFloatSlider(ref toolkit.magSoundSpeed, "Sound speed:", "F0", "m/s", 0.0f, maxSoundSpeed, SliderSoundSpeed);
            EndSubsection();

            // Debug Log
            BeginSubsection("Debug log:");
            if (CreateToggle(ref toolkit.debugLog, "Write debug log file"))
                toolkit.SendWriteDebugLog(toolkit.debugLog);
            EndSubsection();
        }

        EndSection();
    }


    ///////////////////////////////////////////////////////////
    // AUXILIARY FUNCTIONS FOR CREATING FORMATTED GUI ELEMENTS
    ///////////////////////////////////////////////////////////

    /// <summary>
    /// Auxiliary function for creating sliders for float variables with specific format
    /// </summary>
    /// <returns></returns>
    public void CreateFloatSlider(ref float variable, string name, string decimalDigits, string units, float minValue, float maxValue, System.Action action)
    {
        SingleSpace();

        GUILayout.BeginHorizontal();
        
        GUILayout.Label(name);
        string valueString = GUILayout.TextField(variable.ToString(decimalDigits), GUILayout.ExpandWidth(false));
        GUILayout.Label(units, GUILayout.ExpandWidth(false));
        
        GUILayout.EndHorizontal();

        float newValue;
        bool valid = float.TryParse(valueString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out newValue);
        if (valid)
            variable = newValue;

        float previousVar = variable;
        variable = GUILayout.HorizontalSlider(variable, minValue, maxValue);        
        if (variable != previousVar)
            action();
    }
    
    /// <summary>
    /// Auxiliary function for creating sliders for int variables with predefined possible values with specific format
    /// </summary>
    public void CreateSnapSlider(ref int variable, string name, GUIContent content, List<int> values, System.Action action)
    {
        SingleSpace();

        int minValue = values[0];
        int maxValue = values[values.Count - 1];
        GUILayout.BeginHorizontal();
        GUILayout.Label(name);
        GUILayout.Label(content, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        int previousVar = variable;
        variable = (int)GUILayout.HorizontalSlider((float)variable, (float)minValue, (float)maxValue);

        int possibleValue = minValue;
        foreach (int snapValue in values)
        {
            if (variable < snapValue)
            {
                variable = possibleValue;
                break;
            }
            else
                possibleValue = snapValue;
        }

        if (variable != previousVar)
            action();        
    }

    /// <summary>
    /// Auxiliary function for creating a new section with title for parameter groups
    /// </summary>    
    public void BeginSection(string titleText)
    {
        GUILayout.BeginVertical(sectionStyle);        
        GUILayout.Box(titleText, titleBoxStyle, GUILayout.ExpandWidth(true));
    }

    /// <summary>
    /// Auxiliary function for ending section of parameter group
    /// </summary>
    public void EndSection()
    {
        GUILayout.EndVertical();
        //GUILayout.Label("");    // Line spacing        
        GUILayout.Space(spaceBetweenSections);          // Line spacing        
    }

    /// <summary>
    /// Auxiliary function for creating a new subsection with title for parameter groups within one section
    /// </summary>    
    public void BeginSubsection(string titleText)
    {
        GUILayout.BeginVertical(subsectionStyle);        
        GUILayout.Box(titleText, subtitleBoxStyle, GUILayout.ExpandWidth(false));
    }

    /// <summary>
    /// Auxiliary function for ending subsection for parameter groups within one section
    /// </summary>
    public void EndSubsection()
    {
        GUILayout.EndVertical();        
        GUILayout.Space(singleSpace);          // Line spacing        
    }

    /// <summary>
    /// Auxiliary function for creating a drag&drop text box
    /// </summary>
    public void CreateDragDropBox(ref string textvar)
    {        
        Event evt = Event.current;
        Rect drop_area = GUILayoutUtility.GetRect(0.0f, 20.0f, GUILayout.ExpandWidth(true));
        drop_area.y += 14.0f;
        GUI.Box(drop_area, textvar, dragdropStyle);
        
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    ChangeFileName(ref textvar, DragAndDrop.paths[0]); 
                }
                break;
        }
    }

    /// <summary>
    ///  Auxiliary function for creating toogle input
    /// </summary>    
    public bool CreateToggle(ref bool boolvar, string toggleText)
    {
        bool oldvar = boolvar;
        boolvar = GUILayout.Toggle(boolvar, toggleText, GUILayout.ExpandWidth(false));
        return (oldvar != boolvar);
    }

    /// <summary>
    /// Put a single horizontal space
    /// </summary>
    public void SingleSpace()
    {
        GUILayout.Space(singleSpace);
    }
}