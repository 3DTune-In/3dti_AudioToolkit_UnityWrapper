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
    // Access to the HA API
    API_3DTI_HA HAAPI;

    //// Constant definitions
    //const int EAR_RIGHT = 0;
    //const int EAR_LEFT = 1;

    // Look and feel parameters
    int logosize = 80;  // Size of 3DTI logo
    int earsize = 60;   // Size of ear clipart
    float spaceBetweenColumns = 5;
    float spaceBetweenSections = 10;
    Color baseColor = Color.white;

    // Global variables 
    //bool switchLeftEar = false;
    //bool switchRightEar = false;
    //bool noiseBefore = false;
    //bool noiseAfter = false;
    ////bool dynamicEq = true;
    //bool interpolation = true;
    //bool debugLogHA = false;

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
        get { return "Hearing Aid simulation effect from 3D-Tune-In Toolkit"; }
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
            // Get HA API instance (TO DO: Error check)
            HAAPI = GameObject.FindObjectOfType<API_3DTI_HA>();

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
        DrawEars(plugin);
        DrawDynamicEq(plugin);
        DrawNoiseGenerator(plugin);
        DrawLimiter(plugin);
        //DrawDebugLog(plugin);       

        initDone = true;

        //return true;        // SHOW ALSO DEFAULT CONTROLS (FOR DEBUG)
        return false;     // DO NOT SHOW DEFAULT CONTROLS
    }


    public void InitializePlugin(IAudioEffectPlugin plugin)
    {
        //HAAPI.Initialize();        
        //plugin.SetFloatParameter("HAL", Bool2Float(switchLeftEar));
        //plugin.SetFloatParameter("HAR", Bool2Float(switchRightEar));
        //plugin.SetFloatParameter("NOISEBEF", Bool2Float(noiseBefore));
        //plugin.SetFloatParameter("NOISEAFT", Bool2Float(noiseAfter));        
        //plugin.SetFloatParameter("EQINT", Bool2Float(interpolation));
        
        //initDone = true;
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

        // Add space below
        GUILayout.Space(spaceBetweenSections);
    }

    /// <summary>
    /// Draw ear icons and global on/off switches for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawEars(IAudioEffectPlugin plugin)
    {        
        BeginLeftColumn(plugin, ref HAAPI.PARAM_PROCESS_LEFT_ON, "Left ear", new List<string> { "HAL" });
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
                CreateParameterSlider(plugin, ref HAAPI.PARAM_VOLUME_L_DB, "VOLL", "Overall gain (dB):", false, "");
            }
            GUILayout.EndHorizontal();
        }
        EndLeftColumn();        

        BeginRightColumn(plugin, ref HAAPI.PARAM_PROCESS_RIGHT_ON, "Right ear", new List<string> { "HAR" });        
        {
            GUILayout.BeginHorizontal();
            {
                //Parameters
                CreateParameterSlider(plugin, ref HAAPI.PARAM_VOLUME_R_DB, "VOLR", "Overall gain (dB):", false, "");
                // Draw ear icon
                Texture RightEarTexture;
                GUIStyle RightEarStyle = EditorStyles.label;
                RightEarStyle.alignment = TextAnchor.MiddleLeft;
                RightEarTexture = Resources.Load("RightEarAlpha") as Texture;
                GUILayout.Box(RightEarTexture, RightEarStyle, GUILayout.Width(earsize), GUILayout.Height(earsize), GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();
        }
        EndRightColumn();        
    }

    /// <summary>
    /// Draw Dynamic EQ controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawDynamicEq(IAudioEffectPlugin plugin)
    {
        BeginCentralColumn("Equalizer");
        {
            //CreateToggle(plugin, ref dynamicEq, "Dynamic Equalizer", "DYNEQ");
            CreateParameterSlider(plugin, ref HAAPI.PARAM_EQ_LPFCUTOFF_HZ, "LPF", "LPF CutOff", false, "Hz");
            CreateParameterSlider(plugin, ref HAAPI.PARAM_EQ_HPFCUTOFF_HZ, "HPF", "HPF CutOff", false, "Hz");
            CreateToggle(plugin, ref HAAPI.PARAM_DYNAMICEQ_INTERPOLATION_ON, "Interpolation", "EQINT");

            BeginLeftColumn();
            {
                GUILayout.Label("LEFT EAR");

                // Band gains
                GUILayout.BeginHorizontal();
                {
                    BeginCentralColumn("Level 0");
                    {
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[0, 0], "DEQL0B0L", "125Hz:", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[0, 1], "DEQL0B1L", "250Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[0, 2], "DEQL0B2L", "500Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[0, 3], "DEQL0B3L", "1kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[0, 4], "DEQL0B4L", "2kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[0, 5], "DEQL0B5L", "4kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[0, 6], "DEQL0B6L", "8kHz", true, "dB");
                    } EndCentralColumn();
                    
                    BeginCentralColumn("Level 1");
                    {
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[1, 0], "DEQL1B0L", "125Hz:", false, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[1, 1], "DEQL1B1L", "250Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[1, 2], "DEQL1B2L", "500Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[1, 3], "DEQL1B3L", "1kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[1, 4], "DEQL1B4L", "2kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[1, 5], "DEQL1B5L", "4kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[1, 6], "DEQL1B6L", "8kHz", true, "dB");
                    } EndCentralColumn();

                    BeginCentralColumn("Level 2");
                    {
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[2, 0], "DEQL2B0L", "125Hz:", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[2, 1], "DEQL2B1L", "250Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[2, 2], "DEQL2B2L", "500Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[2, 3], "DEQL2B3L", "1kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[2, 4], "DEQL2B4L", "2kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[2, 5], "DEQL2B5L", "4kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT[2, 6], "DEQL2B6L", "8kHz", true, "dB");
                    } EndCentralColumn();

                } GUILayout.EndHorizontal();
                // End Band Gains
                
                BeginCentralColumn("Level Threshold");
                {
                    CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS[0], "THRL0", "Threshold 1", false, "dBfs");
                    CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS[1], "THRL1", "Threshold 2 ", false, "dBfs");
                    CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS[2], "THRL2", "Threshold 3 ", false, "dBfs");
                } EndCentralColumn();

                BeginCentralColumn("Attack Release");
                {                       
                    CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS, "ATREL", "Atack Release", false, "ms");
                } EndCentralColumn();

                BeginCentralColumn("Tone Control");
                {
                    float toneLow = HAAPI.tone[(int)API_3DTI_Common.T_ear.LEFT, (int)API_3DTI_HA.T_toneBand.LOW];
                    float toneMid = HAAPI.tone[(int)API_3DTI_Common.T_ear.LEFT, (int)API_3DTI_HA.T_toneBand.MID];
                    float toneHigh = HAAPI.tone[(int)API_3DTI_Common.T_ear.LEFT, (int)API_3DTI_HA.T_toneBand.HIGH];
                    if (CreateAPIParameterSlider(plugin, ref toneLow, "Low", false, "dB", -10.0f, 10.0f))
                        HAAPI.SetTone(API_3DTI_Common.T_ear.LEFT, API_3DTI_HA.T_toneBand.LOW, toneLow);
                    if (CreateAPIParameterSlider(plugin, ref toneMid, "Mid", false, "dB", -10.0f, 10.0f))
                        HAAPI.SetTone(API_3DTI_Common.T_ear.LEFT, API_3DTI_HA.T_toneBand.MID, toneMid);
                    if (CreateAPIParameterSlider(plugin, ref toneHigh, "High", false, "dB", -10.0f, 10.0f))
                        HAAPI.SetTone(API_3DTI_Common.T_ear.LEFT, API_3DTI_HA.T_toneBand.HIGH, toneHigh);
                } EndCentralColumn();

                BeginCentralColumn("Compression");
                {                    
                    CreateParameterSlider(plugin, ref HAAPI.PARAM_COMPRESSION_PERCENTAGE_LEFT, "COMPRL", "Compression Percentage", false, "%");
                }
                EndCentralColumn();

                BeginCentralColumn("Normalization");
                {
                    GUILayout.BeginHorizontal();
                        CreateToggle(plugin, ref HAAPI.PARAM_NORMALIZATION_SET_ON_LEFT, "Switch Normalization", "NORMONL");
                        if (HAAPI.PARAM_NORMALIZATION_SET_ON_LEFT)
                        {
                            GUILayout.Label("Applied offset: ", GUILayout.ExpandWidth(false));
                            float offsetL;
                            HAAPI.GetNormalizationOffset(API_3DTI_Common.T_ear.LEFT, out offsetL);
                            string offsetStrL = offsetL.ToString();
                            GUILayout.TextArea(offsetStrL, GUILayout.ExpandWidth(false));
                            GUILayout.Label("dB", GUILayout.ExpandWidth(false));
                        }
                    GUILayout.EndHorizontal();
                }
                EndCentralColumn();
            }
            EndLeftColumn(false);

            BeginRightColumn();
            {
                GUILayout.Label("RIGHT EAR");

                // Band Gains
                GUILayout.BeginHorizontal();
                {
                    BeginCentralColumn("Level 0");
                    {
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[0, 0], "DEQL0B0R", "125Hz:", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[0, 1], "DEQL0B1R", "250Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[0, 2], "DEQL0B2R", "500Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[0, 3], "DEQL0B3R", "1kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[0, 4], "DEQL0B4R", "2kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[0, 5], "DEQL0B5R", "4kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[0, 6], "DEQL0B6R", "8kHz", true, "dB");
                    } EndCentralColumn();

                    BeginCentralColumn("Level 1");
                    {
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[1, 0], "DEQL1B0R", "125Hz:", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[1, 1], "DEQL1B1R", "250Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[1, 2], "DEQL1B2R", "500Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[1, 3], "DEQL1B3R", "1kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[1, 4], "DEQL1B4R", "2kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[1, 5], "DEQL1B5R", "4kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[1, 6], "DEQL1B6R", "8kHz", true, "dB");
                    } EndCentralColumn();

                    BeginCentralColumn("Level 2");
                    {
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[2, 0], "DEQL2B0R", "125Hz:", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[2, 1], "DEQL2B1R", "250Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[2, 2], "DEQL2B2R", "500Hz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[2, 3], "DEQL2B3R", "1kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[2, 4], "DEQL2B4R", "2kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[2, 5], "DEQL2B5R", "4kHz", true, "dB");
                        CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT[2, 6], "DEQL2B6R", "8kHz", true, "dB");
                    } EndCentralColumn();

                } GUILayout.EndHorizontal();
                // End Band Gains

                BeginCentralColumn("Level Threshold");
                {
                    CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS[0], "THRR0", "Threshold 1", false, "dBfs");
                    CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS[1], "THRR1", "Threshold 2 ", false, "dBfs");
                    CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS[2], "THRR2", "Threshold 3 ", false, "dBfs");
                } EndCentralColumn();

                BeginCentralColumn("Attack Release");
                {
                    CreateParameterSlider(plugin, ref HAAPI.PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS, "ATRER", "Atack Release", false, "ms");
                } EndCentralColumn();

                BeginCentralColumn("Tone Control");
                {
                    CreateAPIParameterSlider(plugin, ref HAAPI.tone[(int)API_3DTI_Common.T_ear.RIGHT, (int)API_3DTI_HA.T_toneBand.LOW], "Low", false, "dB", -10.0f, 10.0f);
                    CreateAPIParameterSlider(plugin, ref HAAPI.tone[(int)API_3DTI_Common.T_ear.RIGHT, (int)API_3DTI_HA.T_toneBand.MID], "Mid", false, "dB", -10.0f, 10.0f);
                    CreateAPIParameterSlider(plugin, ref HAAPI.tone[(int)API_3DTI_Common.T_ear.RIGHT, (int)API_3DTI_HA.T_toneBand.HIGH], "High", false, "dB", -10.0f, 10.0f);
                }
                EndCentralColumn();

                BeginCentralColumn("Compression");
                {
                    CreateParameterSlider(plugin, ref HAAPI.PARAM_COMPRESSION_PERCENTAGE_RIGHT, "COMPRR", "Compression Percentage", false, "%");
                }
                EndCentralColumn();

                BeginCentralColumn("Normalization");
                {
                    GUILayout.BeginHorizontal();
                    CreateToggle(plugin, ref HAAPI.PARAM_NORMALIZATION_SET_ON_RIGHT, "Switch Normalization", "NORMONR");
                    if (HAAPI.PARAM_NORMALIZATION_SET_ON_RIGHT)
                    {
                        GUILayout.Label("Applied offset: ", GUILayout.ExpandWidth(false));
                        float offsetR;
                        HAAPI.GetNormalizationOffset(API_3DTI_Common.T_ear.RIGHT, out offsetR);
                        string offsetStrR = offsetR.ToString();
                        GUILayout.TextArea(offsetStrR, GUILayout.ExpandWidth(false));
                        GUILayout.Label("dB", GUILayout.ExpandWidth(false));
                    }
                    GUILayout.EndHorizontal();                    
                }
                EndCentralColumn();

            } EndRightColumn(false);            

        }   EndCentralColumn();
    }

    /// <summary>
    /// Draw Noise Generator controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawNoiseGenerator(IAudioEffectPlugin plugin)
    {
        BeginCentralColumn("Quantization Noise Generator");
        {
            CreateToggle(plugin,ref HAAPI.PARAM_NOISE_BEFORE_ON, "Quantization Before", "NOISEBEF");
            CreateToggle(plugin,ref HAAPI.PARAM_NOISE_AFTER_ON, "Quantization After", "NOISEAFT");

            float FloatNBits = (float)HAAPI.PARAM_NOISE_NUMBITS;
            CreateParameterSlider(plugin, ref FloatNBits, "NOISEBITS", "Quantization Number of Bits:", true, "");
            HAAPI.PARAM_NOISE_NUMBITS = (int)FloatNBits;
        }
        EndCentralColumn();
    }

    /// <summary>
    /// Draw limiter controls 
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawLimiter(IAudioEffectPlugin plugin)
    {
        BeginCentralColumn("Limiter");
        {
            CreateToggle(plugin, ref HAAPI.PARAM_LIMITER_ON, "Switch Limiter", "LIMITON");            
        }
        EndCentralColumn();
    }

    /// <summary>
    /// Draw debug log controls 
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawDebugLog(IAudioEffectPlugin plugin)
    {
        BeginCentralColumn("Debug Log File");
        {
            CreateToggle(plugin, ref HAAPI.PARAM_DEBUG_LOG, "Write Debug Log File", "DebugLogHA");
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
                valueString = variable.ToString(decimalDigits);
                valueString = GUILayout.TextField(valueString, GUILayout.ExpandWidth(false));
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

    public bool CreateAPIParameterSlider(IAudioEffectPlugin plugin, ref float APIparam, string parameterText, bool isFloat, string units, float minValue, float maxValue)
    {
        // Set float resolution
        string resolution;
        if (isFloat)
            resolution = "F2";
        else
            resolution = "F0";

        // Create slider and set value        
        if (CreateFloatSlider(ref APIparam, parameterText, resolution, units, minValue, maxValue))
            return true;
        else
            return false;
    }

    public float Bool2Float(bool v)
    {
        if (v)
            return 1.0f;
        else
            return 0.0f;
    }

    bool Float2Bool(float v)
    {
        if (v == 0.0f)
            return false;
        else
            return true;
    }

    public void BeginLeftColumn(IAudioEffectPlugin plugin, ref bool enable, string title, List<string> switchParameters)
    {        
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));                    // Begin section             
            GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(true));  // Begin column                
                bool previousenable = enable;
                enable = EditorGUILayout.BeginToggleGroup(title, enable);       // Begin toggle   
                    if (previousenable != enable)
                    {
                        foreach (string switchParameter in switchParameters)
                        {
                            plugin.SetFloatParameter(switchParameter, Bool2Float(enable));
                        }
                    }
                    EditorGUI.BeginDisabledGroup(!enable); // Begin DisabledGroup 
    }

    public void EndLeftColumn(bool endToogleGroup=true)
    {
                        if (endToogleGroup)
                        {
                            EditorGUILayout.EndToggleGroup();                               // End toggle
                        }
                    EditorGUI.EndDisabledGroup();                   // End DisabledGroup
                GUILayout.EndVertical();                                                // End column
            GUILayout.Space(spaceBetweenColumns);                                   // Space between columns        
    }

    public void BeginRightColumn(IAudioEffectPlugin plugin, ref bool enable, string title, List<string> switchParameters)
    {        
            GUILayout.BeginVertical(rightColumnStyle, GUILayout.ExpandWidth(true));    // Begin column                
                bool previousenable = enable;
                enable = EditorGUILayout.BeginToggleGroup(title, enable);       // Begin toggle  
                    if (previousenable != enable)
                    {
                        foreach (string switchParameter in switchParameters)
                        {
                            plugin.SetFloatParameter(switchParameter, Bool2Float(enable));
                        }
                    }
                    EditorGUI.BeginDisabledGroup(!enable);      // Begin Disabled if right ear is switched off
    }

    public void EndRightColumn(bool endToogleGroup = true)
    {
                        if (endToogleGroup)
                        {
                            EditorGUILayout.EndToggleGroup();                               // End toggle
                        }
                    EditorGUI.EndDisabledGroup();                       // End disabled if right ear is switched off
                GUILayout.EndVertical();                                                // End column
            GUILayout.EndHorizontal();                                                  // End section                  
        GUILayout.Space(spaceBetweenSections);                                      // Space between sections
    }

    public void BeginLeftColumn()
    {
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));                    // Begin section             
           GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(true));  // Begin column
                EditorGUI.BeginDisabledGroup(!HAAPI.PARAM_PROCESS_LEFT_ON);       // Begin Disabled if left ear is switched off
    }

    public void BeginRightColumn()
    {
            GUILayout.BeginVertical(rightColumnStyle, GUILayout.ExpandWidth(true));    // Begin column
                EditorGUI.BeginDisabledGroup(!HAAPI.PARAM_PROCESS_RIGHT_ON);      // Begin Disabled if right ear is switched off
    }

    public void BeginCentralColumn(string title)
    {
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));                      
            GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label(title);
    }

    public void EndCentralColumn()
    {
            GUILayout.EndVertical();                                               // End column
        GUILayout.EndHorizontal();
    }

}
