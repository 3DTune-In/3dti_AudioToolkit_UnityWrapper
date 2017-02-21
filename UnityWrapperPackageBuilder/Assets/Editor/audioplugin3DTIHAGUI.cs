using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

using System.Collections.ObjectModel;   // For ReadOnlyCollection

using UnityEditor;
using UnityEngine;

public class audioplugin3DTIHAGUI : IAudioEffectPluginGUI
{    
    // Constant definitions
    const int EAR_RIGHT = 0;
    const int EAR_LEFT = 1;
    const int DEFAULT_COMP_RATIO = 1;
    const int DEFAULT_COMP_THRESHOLD = 0;
    const int DEFAULT_COMP_ATTACK = 20;
    const int DEFAULT_COMP_RELEASE = 100;

    // Look and feel parameters
    int logosize = 80;  // Size of 3DTI logo
    int earsize = 60;   // Size of ear clipart
    float spaceBetweenColumns = 5;
    float spaceBetweenSections = 10;
    //Color selectedColor = Color.gray;
    Color baseColor = Color.white;

    // Global variables
    bool switchLeftEar = false;
    bool switchRightEar = false;
    bool noiseBefore = true;
    bool noiseAfter = true;
    bool dynamicEq = true;
    bool interpolation = true;

    bool initDone = false;

    // GUI Styles
    GUIStyle leftAlign;
    GUIStyle rightAlign;
    GUIStyle leftColumnStyle;
    GUIStyle rightColumnStyle;
    GUIStyle titleBoxStyle;


    //The GUI name must be unique for each GUI, the one specified in PluginList.h
    public override string Name
    {
        get { return "3DTI Hearing Aid Simulation"; }
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
            // Send commands to plugin to set all parameters
            InitializePlugin(plugin);

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
        DrawEars(plugin);
        DrawDynamicEq(plugin);
        DrawNoiseGenerator(plugin);
           
        
        //return true;        // SHOW ALSO DEFAULT CONTROLS (FOR DEBUG)
        return false;     // DO NOT SHOW DEFAULT CONTROLS
    }


    public void InitializePlugin(IAudioEffectPlugin plugin)
    {
        // Boolean switches
        plugin.SetFloatParameter("HA3DTI_Process_LeftOn", Bool2Float(switchLeftEar));
        plugin.SetFloatParameter("HA3DTI_Process_RightOn", Bool2Float(switchRightEar));

        initDone = true;
    }

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

        // Add space belo
        GUILayout.Space(spaceBetweenSections);
    }


    /// <summary>
    ///  Draw preset buttons for left ear
    /// </summary>
    /// <param name="plugin"></param>
    public void SetEQPreset(IAudioEffectPlugin plugin, int ear, ReadOnlyCollection<int> presetGains)
    {
        if (ear == EAR_LEFT)
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
        }
    }

    /// <summary>
    /// Draw ear icons and global on/off switches for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawEars(IAudioEffectPlugin plugin)
    {        
        BeginLeftColumn(plugin, ref switchLeftEar, "Left ear", new List<string> { "EQLeftOn", "CompLeftOn"}, false);  
        {
            GUILayout.BeginHorizontal();
            {
                // Draw ear icon
                Texture LeftEarTexture;
                GUIStyle LeftEarStyle = EditorStyles.label;
                LeftEarStyle.alignment = TextAnchor.MiddleLeft;
                LeftEarTexture = Resources.Load("LeftEarAlpha") as Texture;
                GUILayout.Box(LeftEarTexture, LeftEarStyle, GUILayout.Width(earsize), GUILayout.Height(earsize), GUILayout.ExpandWidth(false));

                //Parameters
                CreateParameterSlider(plugin, "VOLL", "Overall gain (dB):", false, "");               
            }
            GUILayout.EndHorizontal();
        }
        EndLeftColumn(false);        

        BeginRightColumn(plugin, ref switchRightEar, "Right ear", new List<string> { "EQRightOn", "CompRightOn" }, false);
        {
            GUILayout.BeginHorizontal();
            {
                //Parameters
                CreateParameterSlider(plugin, "VOLR", "Overall gain (dB):", false, "");
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
    /// Draw Compressor controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawDynamicEq(IAudioEffectPlugin plugin)
    {
        BeginCentralColumn("Equalizer");
        {
            CreateToggle(plugin, ref dynamicEq, "Dynamic Equalizer", "DYNEQ");
            CreateParameterSlider(plugin, "LPF", "LPF CutOff", false, "Hz");
            CreateParameterSlider(plugin, "HPF", "HPF CutOff", false, "Hz");
            if (dynamicEq){ CreateToggle(plugin, ref interpolation, "Interpolation", "EQINT"); }

            BeginLeftColumn();
            {
               GUILayout.Label("LEFT EAR");
               
                if (dynamicEq) {
                    GUILayout.BeginHorizontal();
                    BeginCentralColumn("Level 0");
                }
               else { BeginCentralColumn(""); }
               { 
                    CreateParameterSlider(plugin, "DEQL0B0L", "125Hz:", true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B1L", "250Hz",  true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B2L", "500Hz",  true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B3L", "1kHz",   true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B4L", "2kHz",   true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B5L", "4kHz",   true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B6L", "8kHz",   true, "dB");
              }
              EndCentralColumn();
                if (dynamicEq)
                {
                  BeginCentralColumn("Level 1");
                  {
                        CreateParameterSlider(plugin, "DEQL1B0L", "125Hz:", false, "dB");
                        CreateParameterSlider(plugin, "DEQL1B1L", "250Hz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL1B2L", "500Hz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL1B3L", "1kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL1B4L", "2kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL1B5L", "4kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL1B6L", "8kHz", true, "dB");
                 }
                EndCentralColumn();

                BeginCentralColumn("Level 2");
                {
                CreateParameterSlider(plugin, "DEQL2B0L", "125Hz:", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B1L", "250Hz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B2L", "500Hz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B3L", "1kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B4L", "2kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B5L", "4kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B6L", "8kHz", true, "dB");
                     }
                      EndCentralColumn();

                   
                    GUILayout.EndHorizontal();
                    BeginCentralColumn("Level Threshold");
                    {                       
                        CreateParameterSlider(plugin, "THRL0", "Threshold 1", false, "dBfs");
                        CreateParameterSlider(plugin, "THRL1", "Threshold 2 ", false, "dBfs");
                        CreateParameterSlider(plugin, "THRL2", "Threshold 3 ", false, "dBfs");
                    }
                    EndCentralColumn();
                    BeginCentralColumn("Attack Release");
                    {                       
                        CreateParameterSlider(plugin, "ATREL", "Atack Release", false, "ms");
                    }
                    EndCentralColumn();
                }
            }
            EndLeftColumn(true, false);

            BeginRightColumn();
            {
                GUILayout.Label("RIGHT EAR");

                if (dynamicEq) {
                    GUILayout.BeginHorizontal();
                    BeginCentralColumn("Level 0");
                    
                }
                else { BeginCentralColumn(""); }
                { 
                    CreateParameterSlider(plugin, "DEQL0B0R", "125Hz:", true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B1R", "250Hz", true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B2R", "500Hz", true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B3R", "1kHz", true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B4R", "2kHz", true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B5R", "4kHz", true, "dB");
                    CreateParameterSlider(plugin, "DEQL0B6R", "8kHz", true, "dB");

                }
                EndCentralColumn();
                if (dynamicEq)
                {
                     BeginCentralColumn("Level 1");
                     {                                           
                        CreateParameterSlider(plugin, "DEQL1B0R", "125Hz:", true, "dB");
                        CreateParameterSlider(plugin, "DEQL1B1R", "250Hz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL1B2R", "500Hz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL1B3R", "1kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL1B4R", "2kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL1B5R", "4kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL1B6R", "8kHz", true, "dB");
                    }
                    EndCentralColumn();

                    BeginCentralColumn("Level 2");
                    {
                   
                    CreateParameterSlider(plugin, "DEQL2B0R", "125Hz:", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B1R", "250Hz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B2R", "500Hz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B3R", "1kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B4R", "2kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B5R", "4kHz", true, "dB");
                        CreateParameterSlider(plugin, "DEQL2B6R", "8kHz", true, "dB");
                    }
                     EndCentralColumn();

                    GUILayout.EndHorizontal();

                    BeginCentralColumn("Level Threshold");
                    {
                        CreateParameterSlider(plugin, "THRR0", "Threshold 1", false, "dBfs");
                        CreateParameterSlider(plugin, "THRR1", "Threshold 2 ", false, "dBfs");
                        CreateParameterSlider(plugin, "THRR2", "Threshold 3 ", false, "dBfs");
                    }
                    EndCentralColumn();
                    BeginCentralColumn("Attack Release");
                    {
                    CreateParameterSlider(plugin, "ATRER", "Atack Release", false, "ms");
                    }
                    EndCentralColumn();
                }
                EndRightColumn(true, false);
            }
        }
    EndCentralColumn();
    }

    /// <summary>
    /// Draw Noise Generator controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawNoiseGenerator(IAudioEffectPlugin plugin)
    {
        BeginCentralColumn("Quantization Noise Generator");
        {
            CreateToggle(plugin,ref noiseBefore, "Quantization Before", "NOISEBEF");
            CreateToggle(plugin,ref noiseAfter, "Quantization After", "NOISEAFT");
            CreateParameterSlider(plugin, "NOISEBITS", "Quantization Number of Bits:", true, "");
        }
        EndCentralColumn();
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
            plugin.SetFloatParameter(switchParameter, Bool2Float(boolvar));
        }
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
    public bool CreateParameterSlider(IAudioEffectPlugin plugin, string parameterName, string parameterText, bool isFloat, string units)
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
            return true;
        }

        return false;
    }

    public float Bool2Float(bool v)
    {
        if (v)
            return 1.0f;
        else
            return 0.0f;
    }

    public void BeginLeftColumn(IAudioEffectPlugin plugin, ref bool enable, string title, List<string> switchParameters, bool earDisable=true)
    {        
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));                    // Begin section             
            GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(true));  // Begin column
                if (earDisable) EditorGUI.BeginDisabledGroup(!switchLeftEar);       // Begin Disabled if left ear is switched off
                    bool previousenable = enable;
                    enable = EditorGUILayout.BeginToggleGroup(title, enable);       // Begin toggle   
                        if (previousenable != enable)
                        {
                            foreach (string switchParameter in switchParameters)
                            {
                                plugin.SetFloatParameter(switchParameter, Bool2Float(enable));
                            }
                        }
    }

    public void EndLeftColumn(bool earDisable=true, bool endToogleGroup=true)
    {
        if (endToogleGroup)
        {
            EditorGUILayout.EndToggleGroup();                               // End toggle
        }
        if (earDisable) { EditorGUI.EndDisabledGroup(); }                  // End disabled if left ear is switched off
        GUILayout.EndVertical();                                                // End column
        GUILayout.Space(spaceBetweenColumns);                                   // Space between columns
    }

    public void BeginRightColumn(IAudioEffectPlugin plugin, ref bool enable, string title, List<string> switchParameters, bool earDisable=true)
    {        
            GUILayout.BeginVertical(rightColumnStyle, GUILayout.ExpandWidth(true));    // Begin column
                if (earDisable) EditorGUI.BeginDisabledGroup(!switchRightEar);      // Begin Disabled if right ear is switched off
                    bool previousenable = enable;
                    enable = EditorGUILayout.BeginToggleGroup(title, enable);       // Begin toggle  
                        if (previousenable != enable)
                        {
                            foreach (string switchParameter in switchParameters)
                            {
                                plugin.SetFloatParameter(switchParameter, Bool2Float(enable));
                            }
                        }
    }

    public void EndRightColumn(bool earDisable=true, bool endToogleGroup = true)
    {
        if (endToogleGroup)
        {
            EditorGUILayout.EndToggleGroup();                               // End toggle
        }
        if (earDisable) { EditorGUI.EndDisabledGroup(); }                      // End disabled if right ear is switched off
        GUILayout.EndVertical();                                                // End column
        GUILayout.EndHorizontal();                                                  // End section                  
        GUILayout.Space(spaceBetweenSections);                                      // Space between sections
    }

    public void BeginLeftColumn(bool earDisable = true)
    {
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));                    // Begin section             
        GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(true));  // Begin column
        if (earDisable) EditorGUI.BeginDisabledGroup(!switchLeftEar);       // Begin Disabled if left ear is switched off
    }

    public void BeginRightColumn(bool earDisable = true)
    {
        GUILayout.BeginVertical(rightColumnStyle, GUILayout.ExpandWidth(true));    // Begin column
        if (earDisable) EditorGUI.BeginDisabledGroup(!switchRightEar);      // Begin Disabled if right ear is switched off
    }

    public void BeginCentralColumn(string title)
    {
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));                      
        GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(true));
        GUILayout.Label(title);
    }

    public void EndCentralColumn()
    {
        GUILayout.EndVertical();                                                // End column
        GUILayout.EndHorizontal();
    }

}
