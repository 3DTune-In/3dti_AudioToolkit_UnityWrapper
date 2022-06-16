using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

using System.Collections.ObjectModel;   // For ReadOnlyCollection

using UnityEditor;
using UnityEngine;
using API_3DTI;

using static Common3DTIGUI;
using static API_3DTI.HearingAid;
using static API_3DTI.HearingAid.Parameter;



public class audioplugin3DTIHAGUI : IAudioEffectPluginGUI
{
    bool showRawParameters = false;

    // Access to the HA API
    HearingAid HAAPI;
    HearingLoss HLAPI;

    // Internal use constants
    const float FIG6_THRESHOLD_0_DBSPL = 40.0f; // TO DO: consistent numbering
    const float FIG6_THRESHOLD_1_DBSPL = 65.0f; // TO DO: consistent numbering
    const float FIG6_THRESHOLD_2_DBSPL = 95.0f;
    const float DBSPL_FOR_0_DBFS = 100.0f;

    // Start Play control
    bool isStartingPlay = false;
    bool playWasStarted = false;

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
        //if (!initDone)
        //{
            // Get HA API instance (TO DO: Error check)            
            HAAPI = GameObject.FindObjectOfType<HearingAid>();
            if (HAAPI == null)
        {
                GUILayout.Label("Please create an instance of API_3DTI_HA in the scene hierarchy to use this effect.");
                return false;
        }
        // Get HA API instance (TO DO: Error check)            
        HLAPI = GameObject.FindObjectOfType<HearingLoss>();
        if (HLAPI == null)
        {
            GUILayout.Label("In addition to the API_3DTI_HA component, the Hearing Aid simulator also depends on HearingLoss. Please create an instance of HearingLoss in the scene hierarchy to use this effect.");
            return false;
        }
        // Setup styles
        Common3DTIGUI.InitStyles();
        //}                

        //GUILayout.BeginArea(new Rect(10, 10, 100, 100));

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

        // DRAW CUSTOM GUI
        Common3DTIGUI.Show3DTILogo();
        Common3DTIGUI.ShowGUITitle("HEARING AID SIMULATION");
        Common3DTIGUI.SingleSpace();
        Common3DTIGUI.ShowAboutButton();
        showRawParameters = GUILayout.Toggle(showRawParameters, new GUIContent("Show raw parameters", "Used for exposing parameters to the mixer and debugging"));
        Common3DTIGUI.SingleSpace();        

        DrawEars(plugin);
        DrawDynamicEq(plugin);
        EditorGUI.BeginDisabledGroup(!(plugin.GetParameter<Parameter, bool>(ProcessOn, T_ear.LEFT) | plugin.GetParameter<Parameter, bool>(ProcessOn, T_ear.RIGHT))); // Begin DisabledGroup

        DrawNoiseGenerator(plugin);
        DrawLimiter(plugin);
        EditorGUI.EndDisabledGroup();
        //DrawDebugLog(plugin);       

        //GUILayout.EndArea();

        // End starting play
        isStartingPlay = false;

        return showRawParameters;
        //return true;        // SHOW ALSO DEFAULT CONTROLS (FOR DEBUG AND EXPOSING PARAMETERS)
        //return false;     // DO NOT SHOW DEFAULT CONTROLS
    }

    /// <summary>
    /// Draw ear icons and global on/off switches for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawEars(IAudioEffectPlugin plugin)
    {
        BeginColumn(plugin, T_ear.LEFT, ProcessOn);
        //Common3DTIGUI.BeginLeftColumn(plugin, ref HAAPI.PARAM_PROCESS_LEFT_ON, "LEFT EAR", "Enable left ear hearing aid", new List<string> { "HAL" }, isStartingPlay);
        {
            GUILayout.BeginHorizontal();
            Common3DTIGUI.AddLabelToParameterGroup("Overall gain");
            {
                Common3DTIGUI.DrawEar(T_ear.LEFT);
                CreateControls(plugin, T_ear.LEFT, false, VolumeDb);
                //Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HAAPI.PARAM_VOLUME_L_DB, "VOLL", "Overall gain", false, "dB", "Set global volume for left ear");
            }
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndLeftColumn();

        BeginColumn(plugin, T_ear.RIGHT, ProcessOn);
        //Common3DTIGUI.BeginRightColumn(plugin, ref HAAPI.PARAM_PROCESS_RIGHT_ON, "RIGHT EAR", "Enable right ear hearing aid", new List<string> { "HAR" }, isStartingPlay);        
        {
        GUILayout.BeginHorizontal();            
            Common3DTIGUI.AddLabelToParameterGroup("Overall gain");
            {                
                CreateControls(plugin, T_ear.RIGHT, false, VolumeDb);
                //Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HAAPI.PARAM_VOLUME_R_DB, "VOLR", "Overall gain", false, "dB", "Set global volume for right ear");                
                Common3DTIGUI.DrawEar(T_ear.RIGHT);             
            }
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndRightColumn();        
    }

    /// <summary>
    /// Draw Dynamic EQ controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawDynamicEq(IAudioEffectPlugin plugin)
    {
        //BeginCentralColumn("Equalizer");
        Common3DTIGUI.BeginSection("DYNAMIC EQUALIZER");
        Common3DTIGUI.AddLabelToParameterGroup("LPF Cutoff ");
        Common3DTIGUI.AddLabelToParameterGroup("HPF Cutoff ");
        {
            EditorGUI.BeginDisabledGroup(!(plugin.GetParameter<Parameter, bool>(ProcessOn, T_ear.LEFT) | plugin.GetParameter<Parameter, bool>(ProcessOn, T_ear.RIGHT))); // Begin DisabledGroup

            // Global EQ Controls
            CreateControls(plugin, T_ear.BOTH, false, EqLpfCutoffHz, EqHpfCutoffHz, DynamicEqInterpolationOn);
            EditorGUI.EndDisabledGroup();   // End DisabledGroup 

            //Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HAAPI.PARAM_EQ_LPFCUTOFF_HZ, "LPF", "LPF CutOff", false, "Hz", "Set cutoff frequency of global Low Pass Filter");
            //Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HAAPI.PARAM_EQ_HPFCUTOFF_HZ, "HPF", "HPF CutOff", false, "Hz", "Set cutoff frequency of global High Pass Filter");
            //Common3DTIGUI.CreatePluginToggle(plugin, ref HAAPI.PARAM_DYNAMICEQ_INTERPOLATION_ON, "Interpolation", "EQINT", "Enable interpolation of dynamic equalizer curves", isStartingPlay);

            // Left ear
            Common3DTIGUI.BeginLeftColumn(plugin.GetParameter<Parameter, bool>(ProcessOn, T_ear.LEFT));
            {
                DrawFig6Button(plugin, T_ear.LEFT);
                DrawEQBandGains(plugin, T_ear.LEFT);//, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT);
                DrawEQLevelThresholds(plugin, T_ear.LEFT, ref HAAPI.PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS);
                DrawEQEnvelopeFollower(plugin, T_ear.LEFT, ref HAAPI.PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS);
                DrawEQToneControl(plugin, T_ear.LEFT, ref HAAPI.PARAM_TONE_LOW_LEFT, ref HAAPI.PARAM_TONE_MID_LEFT, ref HAAPI.PARAM_TONE_HIGH_LEFT);
                DrawEQCompression(plugin, T_ear.LEFT, ref HAAPI.PARAM_COMPRESSION_PERCENTAGE_LEFT);
                DrawEQNormalization(plugin, T_ear.LEFT, ref HAAPI.PARAM_NORMALIZATION_SET_ON_LEFT);
            }
            Common3DTIGUI.EndLeftColumn();
                       
            // Right ear
            Common3DTIGUI.BeginRightColumn(plugin.GetParameter<Parameter, bool>(ProcessOn, T_ear.RIGHT));
            {
                DrawFig6Button(plugin, T_ear.RIGHT);
                DrawEQBandGains(plugin, T_ear.RIGHT);//, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT);
                DrawEQLevelThresholds(plugin, T_ear.RIGHT, ref HAAPI.PARAM_DYNAMICEQ_LEVELTHRESHOLDS_RIGHT_DBFS);
                DrawEQEnvelopeFollower(plugin, T_ear.RIGHT, ref HAAPI.PARAM_DYNAMICEQ_ATTACKRELEASE_RIGHT_MS);
                DrawEQToneControl(plugin, T_ear.RIGHT, ref HAAPI.PARAM_TONE_LOW_RIGHT, ref HAAPI.PARAM_TONE_MID_RIGHT, ref HAAPI.PARAM_TONE_HIGH_RIGHT);
                DrawEQCompression(plugin, T_ear.RIGHT, ref HAAPI.PARAM_COMPRESSION_PERCENTAGE_RIGHT);
                DrawEQNormalization(plugin, T_ear.RIGHT, ref HAAPI.PARAM_NORMALIZATION_SET_ON_RIGHT);
            }
            Common3DTIGUI.EndRightColumn();            

        }
        Common3DTIGUI.EndSection();
    }
    /// <summary>
    /// Draw Fig6 button
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawFig6Button(IAudioEffectPlugin plugin, T_ear ear)
    {
        Debug.Assert(ear != T_ear.BOTH);

        if(Common3DTIGUI.CreateButton("Fig6", "Adjusts the Dynamic Equalizer to the current audiometry settings"))
        {
            float[,,] calculatedGains = HAAPI.SetEQFromFig6(ear, HLAPI);
            // need to manually update plugin otherwise Unity doesn't update the GUI.
            Debug.Assert(calculatedGains.GetLength(0) == 2 && calculatedGains.GetLength(1) == 3 && calculatedGains.GetLength(2) == 7);
            for (int i = 0; i < 2; i++)
            {
                if ((i == 0 && ear.HasFlag(T_ear.LEFT)) || (i == 1 && ear.HasFlag(T_ear.RIGHT)))
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 7; k++)
                        {
                            // DEQL1B0L for level 1, band 0, Left. All 0-indexed
                            string param = $"DEQL{j}B{k}{(i == 0 ? 'L' : 'R')}";
                            plugin.SetFloatParameter(param, calculatedGains[i, j, k]);
                        }
                    }

                }
            }
        }
    }

    /// <summary>
    /// Draw dynamic EQ band gains
    /// </summary>
    /// <param name="plugin"></param>
    //public void DrawEQBandGains(IAudioEffectPlugin plugin, T_ear whichear, ref float [,] PARAM_ARRAY)
    //{
    //    GUILayout.BeginHorizontal();        
    //    {
    //        for (int i = 0; i < HearingAid.NUM_EQ_CURVES; i++)
    //        {
    //            Common3DTIGUI.BeginSubColumn("Curve " + (i+1).ToString());
    //            {
    //                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 0], "DEQL" + i.ToString() + "B0" + Common3DTIGUI.GetEarLetter(whichear), "125Hz", false, "dB", "Set gain for 125Hz band of EQ curve " + (i+1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
    //                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 1], "DEQL" + i.ToString() + "B1" + Common3DTIGUI.GetEarLetter(whichear), "250Hz", false, "dB", "Set gain for 250Hz band of EQ curve " + (i + 1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
    //                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 2], "DEQL" + i.ToString() + "B2" + Common3DTIGUI.GetEarLetter(whichear), "500Hz", false, "dB", "Set gain for 500Hz band of EQ curve " + (i + 1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
    //                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 3], "DEQL" + i.ToString() + "B3" + Common3DTIGUI.GetEarLetter(whichear), "1kHz", false, "dB", "Set gain for 1KHz band of EQ curve " + (i + 1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
    //                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 4], "DEQL" + i.ToString() + "B4" + Common3DTIGUI.GetEarLetter(whichear), "2kHz", false, "dB", "Set gain for 2KHz band of EQ curve " + (i + 1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
    //                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 5], "DEQL" + i.ToString() + "B5" + Common3DTIGUI.GetEarLetter(whichear), "4kHz", false, "dB", "Set gain for 4KHz band of EQ curve " + (i + 1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
    //                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 6], "DEQL" + i.ToString() + "B6" + Common3DTIGUI.GetEarLetter(whichear), "8kHz", false, "dB", "Set gain for 8KHz band of EQ curve " + (i + 1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
    //            }
    //            Common3DTIGUI.EndSubColumn();
    //        }
    //    }
    //    GUILayout.EndHorizontal();        
    //}

    public void DrawEQBandGains(IAudioEffectPlugin plugin, T_ear whichear)
    {
        GUILayout.BeginHorizontal();
        {
            Common3DTIGUI.BeginSubColumn("Curve 1");
            {
                CreateControls(plugin, whichear, true, DynamicEqLevel0Band0Db, DynamicEqLevel0Band1Db, DynamicEqLevel0Band2Db, DynamicEqLevel0Band3Db, DynamicEqLevel0Band4Db, DynamicEqLevel0Band5Db, DynamicEqLevel0Band6Db);
            }
            Common3DTIGUI.EndSubColumn();
            Common3DTIGUI.BeginSubColumn("Curve 2");
            {
                CreateControls(plugin, whichear, true, DynamicEqLevel1Band0Db, DynamicEqLevel1Band1Db, DynamicEqLevel1Band2Db, DynamicEqLevel1Band3Db, DynamicEqLevel1Band4Db, DynamicEqLevel1Band5Db, DynamicEqLevel1Band6Db);
            }
            Common3DTIGUI.EndSubColumn();
            Common3DTIGUI.BeginSubColumn("Curve 3");
            {
                CreateControls(plugin, whichear, true, DynamicEqLevel2Band0Db, DynamicEqLevel2Band1Db, DynamicEqLevel2Band2Db, DynamicEqLevel2Band3Db, DynamicEqLevel2Band4Db, DynamicEqLevel2Band5Db, DynamicEqLevel2Band6Db);
            }
            Common3DTIGUI.EndSubColumn();
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draw EQ level thresholds
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="whichear"></param>
    public void DrawEQLevelThresholds(IAudioEffectPlugin plugin, T_ear whichear, ref float[] PARAM_ARRAY)
    {
        Common3DTIGUI.BeginSubColumn("Level Thresholds");        
        {
            for (int i = 0; i < HearingAid.NUM_EQ_CURVES; i++)
            {
                Common3DTIGUI.AddLabelToParameterGroup("Threshold " + (i + 1).ToString());
            }
            CreateControls(plugin, whichear, false, DynamicEqLevelThreshold0Dbfs, DynamicEqLevelThreshold1Dbfs, DynamicEqLevelThreshold2Dbfs);
            for (int i = 0; i < HearingAid.NUM_EQ_CURVES; i++)
            {
                ////Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i], "THR" + Common3DTIGUI.GetEarLetter(whichear) + i.ToString(), "Threshold " + (i + 1).ToString(), false, "dBfs", "Set level threshold for curve " + (i + 1).ToString() + " of the dynamic equalizer in " + Common3DTIGUI.GetEarName(whichear) + " ear");
            }
        }
        Common3DTIGUI.EndSubColumn();
    }

    /// <summary>
    /// Draw EQ envelope follower controls
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="whichear"></param>
    public void DrawEQEnvelopeFollower(IAudioEffectPlugin plugin, T_ear whichear, ref float PARAM)
    {
        Common3DTIGUI.BeginSubColumn("Envelope detector");
        Common3DTIGUI.AddLabelToParameterGroup("Attack/Release");
        {
            CreateControls(plugin, whichear, false, DynamicEqAttackreleaseMs);
            //Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM, "ATRE" + Common3DTIGUI.GetEarLetter(whichear), "Atack/Release", false, "ms", "Set Attack and Release time of envelope detector in " + Common3DTIGUI.GetEarName(whichear) + " ear");
        }
        Common3DTIGUI.EndSubColumn();
    }

    public void DrawEQToneControl(IAudioEffectPlugin plugin, T_ear whichear, ref float PARAMLOW, ref float PARAMMID, ref float PARAMHIGH)
    {
        Common3DTIGUI.BeginSubColumn("Tone Control");
        Common3DTIGUI.AddLabelToParameterGroup("Low");
        Common3DTIGUI.AddLabelToParameterGroup("Mid");
        Common3DTIGUI.AddLabelToParameterGroup("High");
        {
            CreateControls(plugin, whichear, false, ToneLow, ToneMid, ToneHigh);
            ////Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAMLOW, "TONLO" + Common3DTIGUI.GetEarLetter(whichear), "Low", false, "dB", "Set additional gain for low frequencies (125Hz, 250Hz and 500Hz) in " + Common3DTIGUI.GetEarName(whichear) + " ear");
            ////Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAMMID, "TONMI" + Common3DTIGUI.GetEarLetter(whichear), "Mid", false, "dB", "Set additional gain for medium frequencies (1KHz and 2KHz) in " + Common3DTIGUI.GetEarName(whichear) + " ear");
            ////Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAMHIGH, "TONHI" + Common3DTIGUI.GetEarLetter(whichear), "High", false, "dB", "Set additional gain for high frequencies (4Khz and 8KHz) in " + Common3DTIGUI.GetEarName(whichear) + " ear");
        }
        Common3DTIGUI.EndSubColumn();
    }

    /// <summary>
    /// Draw EQ compression controls
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="whichear"></param>
    public void DrawEQCompression(IAudioEffectPlugin plugin, T_ear whichear, ref float PARAM)
    {
        Common3DTIGUI.BeginSubColumn("Compression");
        Common3DTIGUI.AddLabelToParameterGroup("Compression");
        {
            CreateControls(plugin, whichear, false, CompressionPercentage);

            //Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM, "COMPR" + Common3DTIGUI.GetEarLetter(whichear), "Compression", false, "%", "Set compression percentage of dynamic equalizer in " + Common3DTIGUI.GetEarName(whichear) + " ear. (100% applies the levels specified in Level Thresholds)");
        }
        Common3DTIGUI.EndSubColumn();
    }

    /// <summary>
    /// Draw EQ normalization controls
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="whichear"></param>
    public void DrawEQNormalization(IAudioEffectPlugin plugin, T_ear whichear, ref bool PARAM)
    {
        Common3DTIGUI.BeginSubColumn("Normalization");
        {            
            GUILayout.BeginVertical();
            CreateControls(plugin, whichear, false, NormalizationOn);
            //Common3DTIGUI.CreatePluginToggle(plugin, ref PARAM, "Switch Normalization", "NORMON" + Common3DTIGUI.GetEarLetter(whichear), "Enable normalization of dynamic equalizer curves in " + Common3DTIGUI.GetEarName(whichear) + " ear, limiting the maximum gain to 20dB", isStartingPlay);
            //if (PARAM)
            {
                float offset;
                HAAPI.GetNormalizationOffset(whichear, out offset);
                Common3DTIGUI.CreateReadonlyFloatText("Applied offset", "F2", "dB", "Show offset applied to dynamic equalizer curves when normalizing in " + Common3DTIGUI.GetEarName(whichear)+ " ear. (An external extra gain of this value should be applied to compensate)", offset);
            }
            GUILayout.EndVertical();
        }
        Common3DTIGUI.EndSubColumn();
    }

    /// <summary>
    /// Draw Noise Generator controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawNoiseGenerator(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("QUANTIZATION NOISE");
        Common3DTIGUI.AddLabelToParameterGroup("Number of Bits");
        {
            // this line crashes
            CreateControls(plugin, T_ear.BOTH, false, NoiseBeforeOn, NoiseAfterOn, NoiseNumbits);
            //Common3DTIGUI.CreatePluginToggle(plugin, ref HAAPI.PARAM_NOISE_BEFORE_ON, "Quantization Before", "NOISEBEF", "Enable quantization noise before HA process", isStartingPlay);
            //Common3DTIGUI.CreatePluginToggle(plugin, ref HAAPI.PARAM_NOISE_AFTER_ON, "Quantization After", "NOISEAFT", "Enable quantization noise after HA process", isStartingPlay);
            //float FloatNBits = (float)HAAPI.PARAM_NOISE_NUMBITS;
            //Common3DTIGUI.CreatePluginParameterSlider(plugin, ref FloatNBits, "NOISEBITS", "Number of Bits", false, "", "Set number of bits for quantization noise");
            //HAAPI.PARAM_NOISE_NUMBITS = (int)FloatNBits;
        }
        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw limiter controls 
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawLimiter(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("DYNAMIC LIMITER");
        {
            CreateControls(plugin, T_ear.BOTH, false, LimiterSetOn);

            //Common3DTIGUI.CreatePluginToggle(plugin, ref HAAPI.PARAM_LIMITER_ON, "Switch Limiter", "LIMITON", "Enable dynamic limiter after HA process", isStartingPlay);
        }
        Common3DTIGUI.EndSection();
    }

    ///// <summary>
    ///// Draw debug log controls 
    ///// </summary>
    ///// <param name="plugin"></param>
    //public void DrawDebugLog(IAudioEffectPlugin plugin)
    //{
    //    BeginCentralColumn("Debug Log File");
    //    {
    //        CreateToggle(plugin, ref HAAPI.PARAM_DEBUG_LOG, "Write Debug Log File", "DebugLogHA");
    //    }
    //    EndCentralColumn();
    //}

    //public bool CreateAPIParameterSlider(IAudioEffectPlugin plugin, ref float APIparam, string parameterText, bool isFloat, string units, float minValue, float maxValue)
    //{
    //    // Set float resolution
    //    string resolution;
    //    if (isFloat)
    //        resolution = "F2";
    //    else
    //        resolution = "F0";

    //    // Create slider and set value    
    //    float newValue = APIparam;
    //    if (CreateFloatSlider(ref newValue, parameterText, resolution, units, minValue, maxValue))
    //    {
    //        APIparam = newValue;
    //        return true;
    //    }
    //    else
    //        return false;
    //}
}
