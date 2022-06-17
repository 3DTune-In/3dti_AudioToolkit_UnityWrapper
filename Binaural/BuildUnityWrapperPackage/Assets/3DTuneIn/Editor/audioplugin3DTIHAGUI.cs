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
        {
            GUILayout.BeginHorizontal();
            Common3DTIGUI.AddLabelToParameterGroup("Overall gain");
            {
                Common3DTIGUI.DrawEar(T_ear.LEFT);
                CreateControls(plugin, T_ear.LEFT, false, VolumeDb);
            }
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndLeftColumn();

        BeginColumn(plugin, T_ear.RIGHT, ProcessOn);
        {
            GUILayout.BeginHorizontal();
            Common3DTIGUI.AddLabelToParameterGroup("Overall gain");
            {
                CreateControls(plugin, T_ear.RIGHT, false, VolumeDb);
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

            // Left ear
            Common3DTIGUI.BeginLeftColumn(plugin.GetParameter<Parameter, bool>(ProcessOn, T_ear.LEFT));
            {
                DrawFig6Button(plugin, T_ear.LEFT);
                DrawEQBandGains(plugin, T_ear.LEFT);//, ref HAAPI.PARAM_DYNAMICEQ_GAINS_LEFT);
                DrawEQLevelThresholds(plugin, T_ear.LEFT);
                DrawEQEnvelopeFollower(plugin, T_ear.LEFT);
                DrawEQToneControl(plugin, T_ear.LEFT);
                DrawEQCompression(plugin, T_ear.LEFT);
                DrawEQNormalization(plugin, T_ear.LEFT);
            }
            Common3DTIGUI.EndLeftColumn();

            // Right ear
            Common3DTIGUI.BeginRightColumn(plugin.GetParameter<Parameter, bool>(ProcessOn, T_ear.RIGHT));
            {
                DrawFig6Button(plugin, T_ear.RIGHT);
                DrawEQBandGains(plugin, T_ear.RIGHT);//, ref HAAPI.PARAM_DYNAMICEQ_GAINS_RIGHT);
                DrawEQLevelThresholds(plugin, T_ear.RIGHT);
                DrawEQEnvelopeFollower(plugin, T_ear.RIGHT);
                DrawEQToneControl(plugin, T_ear.RIGHT);
                DrawEQCompression(plugin, T_ear.RIGHT);
                DrawEQNormalization(plugin, T_ear.RIGHT);
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

        if (Common3DTIGUI.CreateButton("Fig6", "Adjusts the Dynamic Equalizer to the current audiometry settings"))
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
    public void DrawEQLevelThresholds(IAudioEffectPlugin plugin, T_ear whichear)
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
            }
        }
        Common3DTIGUI.EndSubColumn();
    }

    /// <summary>
    /// Draw EQ envelope follower controls
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="whichear"></param>
    public void DrawEQEnvelopeFollower(IAudioEffectPlugin plugin, T_ear whichear)
    {
        Common3DTIGUI.BeginSubColumn("Envelope detector");
        Common3DTIGUI.AddLabelToParameterGroup("Attack/Release");
        {
            CreateControls(plugin, whichear, false, DynamicEqAttackreleaseMs);
        }
        Common3DTIGUI.EndSubColumn();
    }

    public void DrawEQToneControl(IAudioEffectPlugin plugin, T_ear whichear)
    {
        Common3DTIGUI.BeginSubColumn("Tone Control");
        Common3DTIGUI.AddLabelToParameterGroup("Low");
        Common3DTIGUI.AddLabelToParameterGroup("Mid");
        Common3DTIGUI.AddLabelToParameterGroup("High");
        {
            CreateControls(plugin, whichear, false, ToneLow, ToneMid, ToneHigh);
        }
        Common3DTIGUI.EndSubColumn();
    }

    /// <summary>
    /// Draw EQ compression controls
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="whichear"></param>
    public void DrawEQCompression(IAudioEffectPlugin plugin, T_ear whichear)
    {
        Common3DTIGUI.BeginSubColumn("Compression");
        Common3DTIGUI.AddLabelToParameterGroup("Compression");
        {
            CreateControls(plugin, whichear, false, CompressionPercentage);
        }
        Common3DTIGUI.EndSubColumn();
    }

    /// <summary>
    /// Draw EQ normalization controls
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="whichear"></param>
    public void DrawEQNormalization(IAudioEffectPlugin plugin, T_ear whichear)
    {
        Common3DTIGUI.BeginSubColumn("Normalization");
        {
            GUILayout.BeginVertical();
            CreateControls(plugin, whichear, false, NormalizationOn);
            if (plugin.GetParameter<Parameter, bool>(NormalizationOn, whichear))
            {
                float offset = plugin.GetParameter<Parameter, float>(NormalizationGet, whichear);
                Common3DTIGUI.CreateReadonlyFloatText("Applied offset", "F2", "dB", "Show offset applied to dynamic equalizer curves when normalizing in " + Common3DTIGUI.GetEarName(whichear) + " ear. (An external extra gain of this value should be applied to compensate)", offset);
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
            CreateControls(plugin, T_ear.BOTH, false, NoiseBeforeOn, NoiseAfterOn, NoiseNumbits);
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
        }
        Common3DTIGUI.EndSection();
    }
}