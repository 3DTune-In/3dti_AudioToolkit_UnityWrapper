using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

using System.Collections.ObjectModel;   // For ReadOnlyCollection

using UnityEditor;
using UnityEngine;
using API_3DTI_Common;

public class audioplugin3DTIHAGUI : IAudioEffectPluginGUI
{
    // Access to the HA API
    API_3DTI_HA HAAPI;

    //bool initDone = false;

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
            HAAPI = GameObject.FindObjectOfType<API_3DTI_HA>();

            // Setup styles
            Common3DTIGUI.InitStyles();
        //}                

        //GUILayout.BeginArea(new Rect(10, 10, 100, 100));

        // DRAW CUSTOM GUI
        Common3DTIGUI.Show3DTILogo();
        DrawEars(plugin);
        DrawDynamicEq(plugin);
        DrawNoiseGenerator(plugin);
        DrawLimiter(plugin);
        //DrawDebugLog(plugin);       

        //GUILayout.EndArea();

        //initDone = true;

        //return true;        // SHOW ALSO DEFAULT CONTROLS (FOR DEBUG)
        return false;     // DO NOT SHOW DEFAULT CONTROLS
    }

    /// <summary>
    /// Draw ear icons and global on/off switches for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawEars(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginLeftColumn(plugin, ref HAAPI.PARAM_PROCESS_LEFT_ON, "Left ear", "Enable left ear hearing aid", new List<string> { "HAL" });
        {
            GUILayout.BeginHorizontal();
            Common3DTIGUI.AddLabelToParameterGroup("Overall gain");
            {
                Common3DTIGUI.DrawEar(T_ear.LEFT);
                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HAAPI.PARAM_VOLUME_L_DB, "VOLL", "Overall gain", false, "dB", "Set global volume for left ear");
            }
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndLeftColumn();

        Common3DTIGUI.BeginRightColumn(plugin, ref HAAPI.PARAM_PROCESS_RIGHT_ON, "Right ear", "Enable right ear hearing aid", new List<string> { "HAR" });        
        {
        GUILayout.BeginHorizontal();            
            Common3DTIGUI.AddLabelToParameterGroup("Overall gain");
            {                
                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HAAPI.PARAM_VOLUME_R_DB, "VOLR", "Overall gain", false, "dB", "Set global volume for right ear");                
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
            // Global EQ Controls
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HAAPI.PARAM_EQ_LPFCUTOFF_HZ, "LPF", "LPF CutOff", false, "Hz", "Set cutoff frequency of global Low Pass Filter");
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HAAPI.PARAM_EQ_HPFCUTOFF_HZ, "HPF", "HPF CutOff", false, "Hz", "Set cutoff frequency of global High Pass Filter");
            Common3DTIGUI.CreatePluginToggle(plugin, ref HAAPI.PARAM_DYNAMICEQ_INTERPOLATION_ON, "Interpolation", "EQINT", "Enable interpolation of dynamic equalizer curves");

            // Left ear
            Common3DTIGUI.BeginLeftColumn(HAAPI.PARAM_PROCESS_LEFT_ON);
            {
                DrawEQBandGains(plugin, T_ear.LEFT, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT);
                DrawEQLevelThresholds(plugin, T_ear.LEFT, ref HAAPI.PARAM_DYNAMICEQ_LEVELTHRESHOLDS_LEFT_DBFS);
                DrawEQEnvelopeFollower(plugin, T_ear.LEFT, ref HAAPI.PARAM_DYNAMICEQ_ATTACKRELEASE_LEFT_MS);
                DrawEQToneControl(plugin, T_ear.LEFT, ref HAAPI.PARAM_TONE_LOW_LEFT, ref HAAPI.PARAM_TONE_MID_LEFT, ref HAAPI.PARAM_TONE_HIGH_LEFT);
                DrawEQCompression(plugin, T_ear.LEFT, ref HAAPI.PARAM_COMPRESSION_PERCENTAGE_LEFT);
                DrawEQNormalization(plugin, T_ear.LEFT, ref HAAPI.PARAM_NORMALIZATION_SET_ON_LEFT);
            }
            Common3DTIGUI.EndLeftColumn();
                       
            // Right ear
            Common3DTIGUI.BeginRightColumn(HAAPI.PARAM_PROCESS_RIGHT_ON);
            {
                DrawEQBandGains(plugin, T_ear.RIGHT, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT);
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
    /// Draw dynamic EQ band gains
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawEQBandGains(IAudioEffectPlugin plugin, T_ear whichear, ref float [,] PARAM_ARRAY)
    {
        GUILayout.BeginHorizontal();        
        {
            for (int i = 0; i < API_3DTI_HA.NUM_EQ_CURVES; i++)
            {
                Common3DTIGUI.BeginSubColumn("Curve " + (i+1).ToString());
                {
                    Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 0], "DEQL" + i.ToString() + "B0" + Common3DTIGUI.GetEarLetter(whichear), "125Hz", false, "dB", "Set gain for 125Hz band of EQ curve " + (i+1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
                    Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 1], "DEQL" + i.ToString() + "B1" + Common3DTIGUI.GetEarLetter(whichear), "250Hz", false, "dB", "Set gain for 250Hz band of EQ curve " + (i+1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
                    Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 2], "DEQL" + i.ToString() + "B2" + Common3DTIGUI.GetEarLetter(whichear), "500Hz", false, "dB", "Set gain for 500Hz band of EQ curve " + (i+1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
                    Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 3], "DEQL" + i.ToString() + "B3" + Common3DTIGUI.GetEarLetter(whichear), "1kHz", false, "dB", "Set gain for 1KHz band of EQ curve " + (i+1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
                    Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 4], "DEQL" + i.ToString() + "B4" + Common3DTIGUI.GetEarLetter(whichear), "2kHz", false, "dB", "Set gain for 2KHz band of EQ curve " + (i+1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
                    Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 5], "DEQL" + i.ToString() + "B5" + Common3DTIGUI.GetEarLetter(whichear), "4kHz", false, "dB", "Set gain for 4KHz band of EQ curve " + (i+1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
                    Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i, 6], "DEQL" + i.ToString() + "B6" + Common3DTIGUI.GetEarLetter(whichear), "8kHz", false, "dB", "Set gain for 8KHz band of EQ curve " + (i+1).ToString() + " in " + Common3DTIGUI.GetEarName(whichear) + " ear", true);
                }
                Common3DTIGUI.EndSubColumn();
            }
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
            for (int i = 0; i < API_3DTI_HA.NUM_EQ_CURVES; i++)
            {
                Common3DTIGUI.AddLabelToParameterGroup("Threshold " + (i + 1).ToString());
            }
            for (int i = 0; i < API_3DTI_HA.NUM_EQ_CURVES; i++)
            {
                Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM_ARRAY[i], "THR" + Common3DTIGUI.GetEarLetter(whichear) + i.ToString(), "Threshold " + (i + 1).ToString(), false, "dBfs", "Set level threshold for curve " + (i + 1).ToString() + " of the dynamic equalizer in " + Common3DTIGUI.GetEarName(whichear) + " ear");
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
        Common3DTIGUI.BeginSubColumn("Envelope follower");
        Common3DTIGUI.AddLabelToParameterGroup("Attack/Release");
        {
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM, "ATRE" + Common3DTIGUI.GetEarLetter(whichear), "Atack/Release", false, "ms", "Set Attack and Release time of envelope follower in " + Common3DTIGUI.GetEarName(whichear) + " ear");
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
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HAAPI.PARAM_TONE_LOW_LEFT, "TONLO" + Common3DTIGUI.GetEarLetter(whichear), "Low", false, "dB", "Set tone for low frequencies in " + Common3DTIGUI.GetEarName(whichear) + " ear");
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HAAPI.PARAM_TONE_MID_LEFT, "TONMI" + Common3DTIGUI.GetEarLetter(whichear), "Mid", false, "dB", "Set tone for medium frequencies in " + Common3DTIGUI.GetEarName(whichear) + " ear");
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HAAPI.PARAM_TONE_HIGH_LEFT, "TONHI" + Common3DTIGUI.GetEarLetter(whichear), "High", false, "dB", "Set tone for high frequencies in " + Common3DTIGUI.GetEarName(whichear) + " ear");
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
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref PARAM, "COMPR" + Common3DTIGUI.GetEarLetter(whichear), "Compression", false, "%", "Set compression percentage of dynamic equalizer in " + Common3DTIGUI.GetEarName(whichear) + " ear");
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
            Common3DTIGUI.CreatePluginToggle(plugin, ref PARAM, "Switch Normalization", "NORMON" + Common3DTIGUI.GetEarLetter(whichear), "Enable normalization of dynamic equalizer curves in " + Common3DTIGUI.GetEarName(whichear) + " ear");
            if (PARAM)
            {
                float offset;
                HAAPI.GetNormalizationOffset(whichear, out offset);
                Common3DTIGUI.CreateReadonlyFloatText("Applied offset", "F2", "dB", "Show offset applied to dynamic equalizer curves after normalization in " + Common3DTIGUI.GetEarName(whichear)+ " ear", offset);
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
            Common3DTIGUI.CreatePluginToggle(plugin, ref HAAPI.PARAM_NOISE_BEFORE_ON, "Quantization Before", "NOISEBEF", "Enable quantization noise before HA process");
            Common3DTIGUI.CreatePluginToggle(plugin, ref HAAPI.PARAM_NOISE_AFTER_ON, "Quantization After", "NOISEAFT", "Enable quantization noise after HA process");
            float FloatNBits = (float)HAAPI.PARAM_NOISE_NUMBITS;
            Common3DTIGUI.CreatePluginParameterSlider(plugin, ref FloatNBits, "NOISEBITS", "Number of Bits", false, "", "Set number of bits for quantization noise");
            HAAPI.PARAM_NOISE_NUMBITS = (int)FloatNBits;
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
            Common3DTIGUI.CreatePluginToggle(plugin, ref HAAPI.PARAM_LIMITER_ON, "Switch Limiter", "LIMITON", "Enable dynamic limiter after HA process");
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
