using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
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
    //static readonly ReadOnlyCollection<int> GAINS_PRESET_MILD = new ReadOnlyCollection<int>(new[] { -7, -7, -12, -15, -22, -25, -25, -25, -25 });
    //static readonly ReadOnlyCollection<int> GAINS_PRESET_MODERATE = new ReadOnlyCollection<int>(new[] { -22, -22, -27, -30, -37, -40, -40, -40, -40 });
    //static readonly ReadOnlyCollection<int> GAINS_PRESET_SEVERE = new ReadOnlyCollection<int>(new[] { -47, -47, -52, -55, -62, -65, -65, -65, -65 });
    //static readonly ReadOnlyCollection<int> GAINS_PRESET_PLAIN = new ReadOnlyCollection<int>(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });

    // Look and feel parameters
    int logosize = 80;  // Size of 3DTI logo
    int earsize = 60;   // Size of ear clipart
    float spaceBetweenColumns = 5;
    float spaceBetweenSections = 10;
    Color selectedColor = Color.gray;
    Color baseColor = Color.white;

    // Global variables 
    bool advancedControls = false;  
    //bool compressorFirst = true;
    //bool eqLeftOn = true;
    //bool eqRightOn = true;
    //bool compressorLeftOn = false;
    //bool compressorRightOn = false;
    int selectedPresetLeft = PRESET_PLAIN;
    int selectedPresetRight = PRESET_PLAIN;
    bool initDone = false;
    //bool debugLogHL = false;

    // GUI Styles
    GUIStyle leftAlign;
    GUIStyle rightAlign;
    GUIStyle leftColumnStyle;
    GUIStyle rightColumnStyle;
    GUIStyle titleBoxStyle;

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
        if (!initDone)
        {
            // Get HL API instance (TO DO: Error check)
            HLAPI = GameObject.FindObjectOfType<API_3DTI_HL>();

            // Send commands to plugin to set all parameters
            //InitializePlugin(plugin);

            // Setup styles
            leftAlign = EditorStyles.label;
            leftAlign.alignment = TextAnchor.MiddleLeft;
            rightAlign = EditorStyles.label;
            rightAlign.alignment = TextAnchor.MiddleRight;
            leftColumnStyle = EditorStyles.miniButton;
            leftColumnStyle.alignment = TextAnchor.MiddleLeft;
            rightColumnStyle = EditorStyles.miniButton;
            rightColumnStyle.alignment = TextAnchor.MiddleRight;
            if (titleBoxStyle == null)
            {
                titleBoxStyle = new GUIStyle(GUI.skin.box);
                titleBoxStyle.normal.textColor = baseColor;
            }
        }

        // DRAW CUSTOM GUI

        DrawHeader(plugin);
        DrawChainOrder(plugin);
        DrawEars(plugin);        

        advancedControls = EditorGUILayout.BeginToggleGroup("Advanced controls", advancedControls);
        if (advancedControls)
        {
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
            DrawDebugLog(plugin);
        }
        EditorGUILayout.EndToggleGroup();

        initDone = true;

        //return true;        // SHOW ALSO DEFAULT CONTROLS (FOR DEBUG)
        return false;     // DO NOT SHOW DEFAULT CONTROLS
    }


    //public void InitializePlugin(IAudioEffectPlugin plugin)
    //{
    //    // Set presets
    //    SetEQPreset(plugin, T_ear.LEFT, API_3DTI_HL.EQ_PRESET_PLAIN);
    //    SetEQPreset(plugin, T_ear.RIGHT, API_3DTI_HL.EQ_PRESET_PLAIN);

    //    // Set global switches
    //    if (!switchLeftEar)
    //    {
    //        HLAPI.SwitchOnOffEffect(T_ear.LEFT, API_3DTI_HL.T_HLEffect.EFFECT_HEARINGLOSS, false);
    //    }
    //    if (!switchRightEar)
    //    {
    //        HLAPI.SwitchOnOffEffect(T_ear.RIGHT, API_3DTI_HL.T_HLEffect.EFFECT_HEARINGLOSS, false);
    //    }

    //    // Boolean switches
    //    plugin.SetFloatParameter("EQLeftOn", Bool2Float(eqLeftOn));
    //    plugin.SetFloatParameter("EQRightOn", Bool2Float(eqRightOn));
    //    plugin.SetFloatParameter("CompLeftOn", Bool2Float(compressorLeftOn));
    //    plugin.SetFloatParameter("CompRightOn", Bool2Float(compressorRightOn));
    //    plugin.SetFloatParameter("CompFirst", Bool2Float(compressorFirst));

    //    // Compressor        
    //    plugin.SetFloatParameter("LeftRatio", (float)DEFAULT_COMP_RATIO);
    //    plugin.SetFloatParameter("LeftThreshold", (float)DEFAULT_COMP_THRESHOLD);
    //    plugin.SetFloatParameter("LeftAttack", (float)DEFAULT_COMP_ATTACK);
    //    plugin.SetFloatParameter("LeftRelease", (float)DEFAULT_COMP_RELEASE);
    //    plugin.SetFloatParameter("RightRatio", (float)DEFAULT_COMP_RATIO);
    //    plugin.SetFloatParameter("RightThreshold", (float)DEFAULT_COMP_THRESHOLD);
    //    plugin.SetFloatParameter("RightAttack", (float)DEFAULT_COMP_ATTACK);
    //    plugin.SetFloatParameter("RightRelease", (float)DEFAULT_COMP_RELEASE);

    //    initDone = true;
    //}

    /// <summary>
    /// Draw header (title and logo)
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawHeader (IAudioEffectPlugin plugin)
    {
        // Draw logo
        Texture logo3DTI;
        GUIStyle logoStyle = EditorStyles.label;
        logoStyle.alignment = TextAnchor.MiddleCenter;
        logo3DTI = Resources.Load("3D_tuneinNoAlpha") as Texture;
        GUILayout.Box(logo3DTI, logoStyle, GUILayout.Width(logosize), GUILayout.Height(logosize), GUILayout.ExpandWidth(true));

        // Add space below
        GUILayout.Space(spaceBetweenSections);
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
        GUILayout.Space(spaceBetweenSections);
    }

    /// <summary>
    ///  Draw preset buttons for left ear
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawLeftPresets(IAudioEffectPlugin plugin)
    {
        GUILayout.BeginVertical();

        if (selectedPresetLeft == PRESET_MILD)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button("Mild", GUILayout.ExpandWidth(false)))
        {
            SetEQPreset(plugin, T_ear.LEFT, API_3DTI_HL.EQ_PRESET_MILD);
            selectedPresetLeft = PRESET_MILD;
        }

        if (selectedPresetLeft == PRESET_MODERATE)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button("Moderate", GUILayout.ExpandWidth(false)))
        {
            SetEQPreset(plugin, T_ear.LEFT, API_3DTI_HL.EQ_PRESET_MODERATE);
            selectedPresetLeft = PRESET_MODERATE;
        }

        if (selectedPresetLeft == PRESET_SEVERE)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button("Severe", GUILayout.ExpandWidth(false)))
        {
            SetEQPreset(plugin, T_ear.LEFT, API_3DTI_HL.EQ_PRESET_SEVERE);
            selectedPresetLeft = PRESET_SEVERE;
        }
        GUI.color = baseColor;

        GUILayout.EndVertical();
    }

    /// <summary>
    ///  Draw preset buttons for right ear
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawRightPresets(IAudioEffectPlugin plugin)
    {
        GUILayout.BeginVertical();

        if (selectedPresetRight == PRESET_MILD)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button("Mild", GUILayout.ExpandWidth(false)))
        {
            SetEQPreset(plugin, T_ear.RIGHT, API_3DTI_HL.EQ_PRESET_MILD);
            selectedPresetRight = PRESET_MILD;
        }

        if (selectedPresetRight == PRESET_MODERATE)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button("Moderate", GUILayout.ExpandWidth(false)))
        {
            SetEQPreset(plugin, T_ear.RIGHT, API_3DTI_HL.EQ_PRESET_MODERATE);
            selectedPresetRight = PRESET_MODERATE;
        }

        if (selectedPresetRight == PRESET_SEVERE)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button("Severe", GUILayout.ExpandWidth(false)))
        {
            SetEQPreset(plugin, T_ear.RIGHT, API_3DTI_HL.EQ_PRESET_SEVERE);
            selectedPresetRight = PRESET_SEVERE;
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
    ///  Auxiliary function for creating toogle input
    /// </summary>    
    public void CreateToggle(IAudioEffectPlugin plugin, ref bool boolvar, string toggleText, string switchParameter)
    {
        bool oldvar = boolvar;
        boolvar = GUILayout.Toggle(boolvar, toggleText, GUILayout.ExpandWidth(false));
        if (oldvar != boolvar)
        {
            plugin.SetFloatParameter(switchParameter, CommonFunctions.Bool2Float(boolvar));
        }
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
        BeginLeftColumn(plugin, API_3DTI_HL.T_HLEffect.EFFECT_HEARINGLOSS, ref HLAPI.GLOBAL_LEFT_ON, "Left ear", new List<string> { "EQLeftOn", "CompLeftOn"}, false);          
        {
            GUILayout.BeginHorizontal();
            {
                // Draw ear icon
                Texture LeftEarTexture;
                GUIStyle LeftEarStyle = EditorStyles.label;
                LeftEarStyle.alignment = TextAnchor.MiddleLeft;
                LeftEarTexture = Resources.Load("LeftEarAlpha") as Texture;
                GUILayout.Box(LeftEarTexture, LeftEarStyle, GUILayout.Width(earsize), GUILayout.Height(earsize), GUILayout.ExpandWidth(false));

                // Draw preset buttons
                GUILayout.Space(spaceBetweenSections);
                DrawLeftPresets(plugin);                
            }
            GUILayout.EndHorizontal();
        }
        EndLeftColumn(false);

        BeginRightColumn(plugin, API_3DTI_HL.T_HLEffect.EFFECT_HEARINGLOSS, ref HLAPI.GLOBAL_RIGHT_ON, "Right ear", new List<string> { "EQRightOn", "CompRightOn" }, false);        
        {
            GUILayout.BeginHorizontal();
            {
                // Draw preset buttons
                GUILayout.Space(spaceBetweenSections);
                DrawRightPresets(plugin);                

                // Draw ear icon
                Texture RightEarTexture;
                GUIStyle RightEarStyle = EditorStyles.label;
                RightEarStyle.alignment = TextAnchor.MiddleLeft;
                RightEarTexture = Resources.Load("RightEarAlpha") as Texture;
                GUILayout.Box(RightEarTexture, RightEarStyle, GUILayout.Width(earsize), GUILayout.Height(earsize), GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();
        }
        EndRightColumn(false);        
    }  

    /// <summary>
    /// Draw EQ controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawEQ(IAudioEffectPlugin plugin)
    {
        BeginLeftColumn(plugin, API_3DTI_HL.T_HLEffect.EFFECT_EQ, ref HLAPI.PARAM_LEFT_EQ_ON, "Equalizer", new List<string> { "EQLeftOn" });        
        {            
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[0], "EQL0", "62.5 Hz", false, "dB")) selectedPresetLeft = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[1], "EQL1", "125 Hz", false, "dB")) selectedPresetLeft = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[2], "EQL2", "250 Hz", false, "dB")) selectedPresetLeft = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[3], "EQL3", "500 Hz", false, "dB")) selectedPresetLeft = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[4], "EQL4", "1 KHz", false, "dB")) selectedPresetLeft = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[5], "EQL5", "2 KHz", false, "dB")) selectedPresetLeft = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[6], "EQL6", "4 KHz", false, "dB")) selectedPresetLeft = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[7], "EQL7", "8 KHz", false, "dB")) selectedPresetLeft = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_LEFT[8], "EQL8", "16 KHz", false, "dB")) selectedPresetLeft = PRESET_CUSTOM;
        }
        EndLeftColumn();

        BeginRightColumn(plugin, API_3DTI_HL.T_HLEffect.EFFECT_EQ, ref HLAPI.PARAM_RIGHT_EQ_ON, "Equalizer", new List<string> { "EQRightOn" });        
        {
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[0], "EQR0", "62.5 Hz", false, "dB")) selectedPresetRight = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[1], "EQR1", "125 Hz", false, "dB")) selectedPresetRight = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[2], "EQR2", "250 Hz", false, "dB")) selectedPresetRight = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[3], "EQR3", "500 Hz", false, "dB")) selectedPresetRight = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[4], "EQR4", "1 KHz", false, "dB")) selectedPresetRight = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[5], "EQR5", "2 KHz", false, "dB")) selectedPresetRight = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[6], "EQR6", "4 KHz", false, "dB")) selectedPresetRight = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[7], "EQR7", "8 KHz", false, "dB")) selectedPresetRight = PRESET_CUSTOM;
            if (CreateParameterSlider(plugin, ref HLAPI.PARAM_BANDS_DB_RIGHT[8], "EQR8", "16 KHz", false, "dB")) selectedPresetRight = PRESET_CUSTOM;
        }
        EndRightColumn();
    }

    /// <summary>
    /// Draw Compressor controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawCompressor(IAudioEffectPlugin plugin)
    {

        BeginLeftColumn(plugin, API_3DTI_HL.T_HLEffect.EFFECT_COMPRESSOR, ref HLAPI.PARAM_LEFT_COMPRESSOR_ON, "Compressor", new List<string> { "CompLeftOn" });        
        {
            CreateParameterSlider(plugin, ref HLAPI.PARAM_COMP_LEFT_RATIO, "LeftRatio", "Ratio 1:", false, "");
            CreateParameterSlider(plugin, ref HLAPI.PARAM_COMP_LEFT_THRESHOLD, "LeftThreshold", "Threshold", false, "dB");
            CreateParameterSlider(plugin, ref HLAPI.PARAM_COMP_LEFT_ATTACK, "LeftAttack", "Attack", false, "ms");
            CreateParameterSlider(plugin, ref HLAPI.PARAM_COMP_LEFT_RELEASE, "LeftRelease", "Release", false, "ms");
        }
        EndLeftColumn();

        BeginRightColumn(plugin, API_3DTI_HL.T_HLEffect.EFFECT_COMPRESSOR, ref HLAPI.PARAM_RIGHT_COMPRESSOR_ON, "Compressor", new List<string> { "CompRightOn" });        
        {
            CreateParameterSlider(plugin, ref HLAPI.PARAM_COMP_RIGHT_RATIO, "RightRatio", "Ratio 1:", false, "");
            CreateParameterSlider(plugin, ref HLAPI.PARAM_COMP_RIGHT_THRESHOLD, "RightThreshold", "Threshold", false, "dB");
            CreateParameterSlider(plugin, ref HLAPI.PARAM_COMP_RIGHT_ATTACK, "RightAttack", "Attack", false, "ms");
            CreateParameterSlider(plugin, ref HLAPI.PARAM_COMP_RIGHT_RELEASE, "RightRelease", "Release", false, "ms");
        }
        EndRightColumn();
    }

    /// <summary>
    ///  Auxiliary function for creating toogle input
    /// </summary>    
    public bool CreateToggle(ref bool boolvar, string toggleText, GUIStyle toggleStyle)
    {
        bool oldvar = boolvar;
        boolvar = GUILayout.Toggle(boolvar, toggleText, toggleStyle, GUILayout.ExpandWidth(false));
        return (oldvar != boolvar);
    }

    /// <summary>
    /// Auxiliary function for creating sliders for float variables with specific format
    /// </summary>
    /// <returns></returns>
    public bool CreateFloatSlider(ref float variable, string name, string decimalDigits, string units, float minValue, float maxValue)
    {
        string valueString;
        float previousVar;

        GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(name);
                valueString = GUILayout.TextField(variable.ToString(decimalDigits), GUILayout.ExpandWidth(false));
                GUILayout.Label(units, GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();

            float newValue;
            bool valid = float.TryParse(valueString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out newValue);
            if (valid)
                variable = newValue;

            previousVar = variable;
            variable = GUILayout.HorizontalSlider(variable, minValue, maxValue);
        }
        GUILayout.EndVertical();

        return (variable != previousVar);            
    }

    /// <summary>
    /// Create a slider associated to a parameter of the plugin
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="parameterName"></param>
    /// <param name="parameterText"></param>
    /// <param name="isFloat"></param>
    /// <param name="units"></param>
    /// <returns>True if slider value has changed</returns>
    public bool CreateParameterSlider(IAudioEffectPlugin plugin, ref float APIparam, string parameterName, string parameterText, bool isFloat, string units)
    {
        // Get parameter info
        float newValue;
        float minValue, maxValue;
        plugin.GetFloatParameterInfo(parameterName, out minValue, out maxValue, out newValue);

        // Set float resolution
        string resolution;
        if (isFloat)
            resolution = "F2";
        else
            resolution = "F0";

        // Create slider and set value
        plugin.GetFloatParameter(parameterName, out newValue);
        if (CreateFloatSlider(ref newValue, parameterText, resolution, units, minValue, maxValue))
        {
            plugin.SetFloatParameter(parameterName, newValue);
            APIparam = newValue;
            return true;
        }

        return false;
    }

    public void BeginLeftColumn(IAudioEffectPlugin plugin, API_3DTI_HL.T_HLEffect whichEffect, ref bool enable, string title, List<string> switchParameters, bool earDisable = true)
    {
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));                    // Begin section             
        GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(true));  // Begin column
        if (earDisable) EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_LEFT_ON);       // Begin Disabled if left ear is switched off
        bool previousenable = enable;
        enable = EditorGUILayout.BeginToggleGroup(title, enable);       // Begin toggle   
        if (previousenable != enable)
        {
            HLAPI.SwitchOnOffEffect(T_ear.LEFT, whichEffect, enable);
            foreach (string switchParameter in switchParameters)
            {
                plugin.SetFloatParameter(switchParameter, CommonFunctions.Bool2Float(enable));
            }
        }
    }

    public void EndLeftColumn(bool earDisable=true)
    {
                    EditorGUILayout.EndToggleGroup();                               // End toggle
                 if (earDisable) EditorGUI.EndDisabledGroup();                      // End disabled if left ear is switched off
            GUILayout.EndVertical();                                                // End column
            GUILayout.Space(spaceBetweenColumns);                                   // Space between columns
    }

    public void BeginRightColumn(IAudioEffectPlugin plugin, API_3DTI_HL.T_HLEffect whichEffect, ref bool enable, string title, List<string> switchParameters, bool earDisable = true)
    {
        GUILayout.BeginVertical(rightColumnStyle, GUILayout.ExpandWidth(true));    // Begin column
        if (earDisable) EditorGUI.BeginDisabledGroup(!HLAPI.GLOBAL_RIGHT_ON);      // Begin Disabled if right ear is switched off
        bool previousenable = enable;
        enable = EditorGUILayout.BeginToggleGroup(title, enable);       // Begin toggle  
        if (previousenable != enable)
        {
            HLAPI.SwitchOnOffEffect(T_ear.RIGHT, whichEffect, enable);
            foreach (string switchParameter in switchParameters)
            {
                plugin.SetFloatParameter(switchParameter, CommonFunctions.Bool2Float(enable));
            }
        }
    }

    public void EndRightColumn(bool earDisable=true)
    {
                    EditorGUILayout.EndToggleGroup();                               // End toggle
                if (earDisable) EditorGUI.EndDisabledGroup();                       // End disabled if right ear is switched off
            GUILayout.EndVertical();                                                // End column
        GUILayout.EndHorizontal();                                                  // End section                  
        GUILayout.Space(spaceBetweenSections);                                      // Space between sections
    }
}
