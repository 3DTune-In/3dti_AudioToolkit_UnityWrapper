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


[CustomEditor(typeof(API_3DTI_Spatializer))]
public class AudioPlugin3DTISpatializerGUI : Editor
{
    API_3DTI_Spatializer toolkit;
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
    //List<int> sampleRateValues = new List<int>() { 11025, 22050, 44100, 48000, 88200, 96000, 112000, 192000 };    
    //List<int> bufferSizeValues = new List<int>() { 64, 128, 256, 512, 1024, 2048 };    

    // Editor view look
    int logosize = 80;
    float spaceBetweenSections = 15.0f;
    float singleSpace = 5.0f;
    GUIStyle titleBoxStyle = null;
    GUIStyle subtitleBoxStyle;
    GUIStyle sectionStyle;
    GUIStyle subsectionStyle;
    GUIStyle dragdropStyle;
    GUIStyle lampStyle = null;
    bool initDone = false;

    //////////////////////////////////////////////////////////////////////////////

    void InitGUI()
    {
        // Get access to API script
        toolkit = (API_3DTI_Spatializer)target;
        toolkit.debugLog = false;

        // Init styles
        subtitleBoxStyle = EditorStyles.label;
        sectionStyle = EditorStyles.miniButton;
        subsectionStyle = EditorStyles.miniButton;
        dragdropStyle = EditorStyles.textField;
        if (titleBoxStyle == null)
        {
            titleBoxStyle = new GUIStyle(GUI.skin.box);
            titleBoxStyle.normal.textColor = Color.white;
        }
        if (lampStyle == null)
        {
            lampStyle = new GUIStyle(GUI.skin.button);
        }
    }

    /// <summary>
    /// This is where we create the layout for the inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Get access to API script
        if (!initDone)
        {
            InitGUI();
            //initDone = true;
        }

        // Show 3D-Tune-In logo         
        Texture logo3DTI;        
        GUIStyle logoStyle = EditorStyles.label;
        logoStyle.alignment = TextAnchor.MiddleCenter;
        logo3DTI = Resources.Load("3D_tuneinNoAlpha") as Texture;        
        GUILayout.Box(logo3DTI, logoStyle, GUILayout.Width(logosize), GUILayout.Height(logosize), GUILayout.ExpandWidth(true));

        ////// LISTENER                
        DrawListenerPanel();

        ////// AUDIO    
        //DrawAudioPanel();

        ////// ADVANCED SETUP
        DrawAdvancedPanel();

        ////// HEARING AID DIRECTIONALITY SETUP
        DrawHADirectionalityPanel();

        ////// LIMITER SETUP
        //DrawLimiterPanel();
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
    /// Action for button Load HRTF
    /// </summary>
    public void ButtonLoadHRTF()
    {        
        string filePath = EditorUtility.OpenFilePanel("Select HRTF File", "", "3dti-hrtf");
        if (filePath.Length != 0)
            ChangeFileName(ref toolkit.HRTFFileName, filePath);
    }

    /// <summary>
    /// Action for button Load ILD
    /// </summary>
    public void ButtonLoadILD()
    {
        string filePath = EditorUtility.OpenFilePanel("Select ILD File", "", "3dti-ild");        
        if (filePath.Length != 0)
            ChangeFileName(ref toolkit.ILDFileName, filePath);
    }

    /// <summary>
    /// Action for slider Head Radius
    /// </summary>
    public void SliderHeadRadius()
    {
        //Debug.Log("Slider head radius changed");
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
    ///  Action for slider HA Directionality Left
    /// </summary>
    public void SliderHADirectionalityLeft()
    {
        toolkit.SetHADirectionalityExtend(API_3DTI_Spatializer.EAR_LEFT, toolkit.HADirectionalityExtendLeft);
    }

    /// <summary>
    ///  Action for slider HA Directionality Right 
    /// </summary>
    public void SliderHADirectionalityRight()
    {
        toolkit.SetHADirectionalityExtend(API_3DTI_Spatializer.EAR_RIGHT, toolkit.HADirectionalityExtendRight);
    }

    ///////////////////////////////////////////////////////////
    // PANEL CONTENTS
    ///////////////////////////////////////////////////////////

    /// <summary>
    /// Draw panel with Listener configuration
    /// </summary>
    public void DrawListenerPanel()
    {
        BeginSection("LISTENER SETUP:");

        // HRTF:
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load HRTF", GUILayout.ExpandWidth(false), GUILayout.Height(40)))
            ButtonLoadHRTF();
        CreateDragDropBox(ref toolkit.HRTFFileName);
        GUILayout.EndHorizontal();

        // ILD:
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load ILD Near Field Filter", GUILayout.ExpandWidth(false), GUILayout.Height(40)))
            ButtonLoadILD();
        CreateDragDropBox(ref toolkit.ILDFileName);
        GUILayout.EndHorizontal();

        // ITD:    
        SingleSpace();
        CreateToggle(ref toolkit.customITDEnabled, "Custom ITD");
        if (toolkit.customITDEnabled)
            CreateFloatSlider(ref toolkit.listenerHeadRadius, "Head radius:", "F4", "meters", 0.0f, maxHeadRadius, SliderHeadRadius);        

        EndSection();
    }

    /// <summary>
    /// Draw panel with audio configuration
    /// </summary>
    //public void DrawAudioPanel()
    //{
    //    BeginSection("AUDIO SETUP:");

    //    // Sample rate slider
    //    CreateSnapSlider(ref toolkit.audioSampleRate, "Sample rate:", new GUIContent(toolkit.audioSampleRate.ToString("F0") + " Hz"), sampleRateValues, SliderSampleRate);

    //    // Buffer size slider
    //    CreateSnapSlider(ref toolkit.audioBufferSize, "Buffer size:", new GUIContent(toolkit.audioBufferSize.ToString("F0") + " samples"), bufferSizeValues, SliderBufferSize);

    //    EndSection();
    //}

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

            // HRTF interpolation
            BeginSubsection("HRTF Interpolation:");                        
            GUILayout.BeginHorizontal();                                              
            bool run = CreateToggle(ref toolkit.runtimeInterpolateHRTF, "Runtime");
            if (run)
                toolkit.SetSourceInterpolation(toolkit.runtimeInterpolateHRTF);
            GUILayout.EndHorizontal();
            EndSubsection();

            // Mod enabler
            BeginSubsection("Modules enabler:");
            if (CreateToggle(ref toolkit.modFarLPF, "Far LPF"))
                toolkit.SetModFarLPF(toolkit.modFarLPF);
            if (CreateToggle(ref toolkit.modDistAtt, "Distance attenuation"))
                toolkit.SetModDistanceAttenuation(toolkit.modDistAtt);
            if (CreateToggle(ref toolkit.modILD, "ILD Near Field Filter"))
                toolkit.SetModILD(toolkit.modILD);
            if (CreateToggle(ref toolkit.modHRTF, "HRTF convolution"))
                toolkit.SetModHRTF(toolkit.modHRTF);
            EndSubsection();

            // Magnitudes
            BeginSubsection("Physical magnitudes:");
            CreateFloatSlider(ref toolkit.magAnechoicAttenuation, "Anechoic distance attenuation:", "F2", "dB", minDB, maxDB, SliderAnechoicAttenuation);
            //CreateFloatSlider(ref toolkit.magReverbAttenuation, "Reverb distance attenuation:", "F2", "dB", minDB, maxDB, SliderReverbAttenuation);
            CreateFloatSlider(ref toolkit.magSoundSpeed, "Sound speed:", "F0", "m/s", 0.0f, maxSoundSpeed, SliderSoundSpeed);
            EndSubsection();

            // Draw Limiter panel
            DrawLimiterPanel();

            // Debug Log
            BeginSubsection("Debug log:");
            if (CreateToggle(ref toolkit.debugLog, "Write debug log file"))
                toolkit.SendWriteDebugLog(toolkit.debugLog);
            EndSubsection();
        }

        EndSection();
    }

    /// <summary>
    /// Draw panel with Hearing Aid directionality configuration
    /// </summary>
    public void DrawHADirectionalityPanel()
    {
        BeginSection("HA DIRECTIONALITY:");

        haSetup = GUILayout.Toggle(haSetup, "Show Hearing Aid Directionality Setup", GUILayout.ExpandWidth(false));
        if (haSetup)
        {
            SingleSpace();          

            // Left ear
            BeginSubsection("Left ear:");            
            if (CreateToggle(ref toolkit.doHADirectionalityLeft, "Switch Directionality"))            
                toolkit.SwitchOnOffHADirectionality(API_3DTI_Spatializer.EAR_LEFT, toolkit.doHADirectionalityLeft);
            CreateFloatSlider(ref toolkit.HADirectionalityExtendLeft, "Directionality extend:", "F2", "dB", minHADB, maxHADB, SliderHADirectionalityLeft);            
            EndSubsection();

            // Right ear
            BeginSubsection("Right ear:");            
            if (CreateToggle(ref toolkit.doHADirectionalityRight, "Switch Directionality"))
                toolkit.SwitchOnOffHADirectionality(API_3DTI_Spatializer.EAR_RIGHT, toolkit.doHADirectionalityRight);
            CreateFloatSlider(ref toolkit.HADirectionalityExtendRight, "Directionality extend:", "F2", "dB", minHADB, maxHADB, SliderHADirectionalityRight);            
            EndSubsection();
        }

        EndSection();
    }

    /// <summary>
    /// Draw panel with Limiter configuration
    /// </summary>
    public void DrawLimiterPanel()
    {
        //BeginSection("LIMITER:");
        BeginSubsection("Limiter:");
        GUILayout.BeginHorizontal();
            SingleSpace();
            if (CreateToggle(ref toolkit.doLimiter, "Switch Limiter"))
                toolkit.SwitchOnOffLimiter(toolkit.doLimiter);
            if (toolkit.doLimiter)
            {
                bool compressing;
                toolkit.GetLimiterCompression(out compressing);
                CreateLamp(compressing);
            }
        GUILayout.EndHorizontal();
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
    /// Auxiliary function for creating a led/lamp indicator
    /// </summary>
    /// <param name="lightOn"></param>
    public void CreateLamp(bool lightOn)
    {
        //GUILayout.Button("", lampStyle, GUILayout.ExpandWidth(false));        
        //GUILayout.Button("", GUILayout.ExpandWidth(false));
    }

    /// <summary>
    /// Put a single horizontal space
    /// </summary>
    public void SingleSpace()
    {
        GUILayout.Space(singleSpace);
    }
}