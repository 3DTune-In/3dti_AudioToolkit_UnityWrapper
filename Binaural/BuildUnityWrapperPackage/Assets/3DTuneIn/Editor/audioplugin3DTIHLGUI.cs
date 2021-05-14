using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;   // For ReadOnlyCollection

using UnityEditor;
using UnityEngine;
using API_3DTI_Common;

using static Common3DTIGUI;
using static API_3DTI_HL;
using static API_3DTI_HL.Parameter;
public class audioplugin3DTIHLGUI : IAudioEffectPluginGUI
{

    /////////////////////////////////////////////////////////
    // INTERNAL VARIABLES
    /////////////////////////////////////////////////////////

    /// Used for exposing and debugging;
    bool showRawParameters = false;

    // Access to the HL API
    API_3DTI_HL HLAPI;

    // Look and feel parameters
    Color selectedColor = Color.gray;
    Color baseColor = Color.white;
    float csCurveWidth = 25.0f;     // Width of classification scale curve images
    float csCurveHeight = 10.0f;    // Height of classiciation scale curve images
    float csSlopeWidth = 15.0f;     // Width of blank space before classification scale slope buttons
    float csSlopeHeight = 10.0f;     // Height of blank space before classification scale slope buttons

    // Global variables for preset buttons activation
    //API_3DTI_HL.T_HLPreset selectedAudiometryPresetLeft = API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
    //bool changingAudiometryPresetLeft = false;
    //API_3DTI_HL.T_HLPreset selectedAudiometryPresetRight = API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
    //bool changingAudiometryPresetRight = false;    
    API_3DTI_HL.T_HLClassificationScaleCurve selectedCurveLeft = API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_UNDEFINED;
    int selectedSlopeLeft = -1;
    API_3DTI_HL.T_HLClassificationScaleSeverity selectedSeverityLeft = API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_UNDEFINED;
    bool changingCSLeft = false;
    API_3DTI_HL.T_HLClassificationScaleCurve selectedCurveRight = API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_UNDEFINED;
    int selectedSlopeRight = -1;
    API_3DTI_HL.T_HLClassificationScaleSeverity selectedSeverityRight = API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_UNDEFINED;
    bool changingCSRight = false;
    [SerializeField]
    API_3DTI_HL.T_HLPreset selectedTDPresetLeft = API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
    bool changingTDPresetLeft = false;
    [SerializeField]
    API_3DTI_HL.T_HLPreset selectedTDPresetRight = API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
    bool changingTDPresetRight = false;
    [SerializeField]
    API_3DTI_HL.T_HLPreset selectedFSPresetLeft = API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
    bool changingFSPresetLeft = false;
    [SerializeField]
    API_3DTI_HL.T_HLPreset selectedFSPresetRight = API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
    bool changingFSPresetRight = false;



    [HideInInspector]
    public T_HLClassificationScaleCurve PARAM_CLASSIFICATION_CURVE_LEFT = T_HLClassificationScaleCurve.HL_CS_NOLOSS;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public int PARAM_CLASSIFICATION_SLOPE_LEFT = 0;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public T_HLClassificationScaleSeverity PARAM_CLASSIFICATION_SEVERITY_LEFT = 0;      // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public T_HLClassificationScaleCurve PARAM_CLASSIFICATION_CURVE_RIGHT = T_HLClassificationScaleCurve.HL_CS_NOLOSS;     // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public int PARAM_CLASSIFICATION_SLOPE_RIGHT = 0;     // For internal use, DO NOT USE IT DIRECTLY
    [HideInInspector]
    public T_HLClassificationScaleSeverity PARAM_CLASSIFICATION_SEVERITY_RIGHT = 0;      // For internal use, DO NOT USE IT DIRECTLY


    // Start Play control
    bool isStartingPlay = false;
    bool playWasStarted = false;


    /////////////////////////////////////////////////////////
    // MAIN GUI CODE
    /////////////////////////////////////////////////////////
    
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
 

        //return false;
        // Initialization (first run)
        //if (!initDone)
        //{
            // Get HL API instance (TO DO: Error check)
            HLAPI = GameObject.FindObjectOfType<API_3DTI_HL>();
        if (HLAPI == null)
        {
            GUILayout.Label("Please create an instance of API_3DTI_HL in the scene hierarchy to use this effect.");
            return false;
        }

		// Setup styles
        Common3DTIGUI.InitStyles();

			
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

        // DRAW AUDIOMETRY GUI
        Common3DTIGUI.Show3DTILogo();

        Common3DTIGUI.ShowGUITitle("AUDIOMETRY");
        Common3DTIGUI.SingleSpace();
        Common3DTIGUI.ShowAboutButton();
        showRawParameters = GUILayout.Toggle(showRawParameters, new GUIContent("Show raw parameters", "Used for exposing parameters to the mixer and debugging"));
        Common3DTIGUI.SingleSpace();

        DrawCalibration(plugin);
        DrawAudiometryEars();
        //DrawAudiometryTemplates(plugin);
        DrawAudiometryClassificationScale(plugin);
        DrawAudiometryFineAdjustment(plugin);

        // DRAW HEARING LOSS GUI
        Common3DTIGUI.Show3DTILogo();
        Common3DTIGUI.ShowGUITitle("HEARING LOSS SIMULATION");
        Common3DTIGUI.SingleSpace();
        Common3DTIGUI.ShowAboutButton();        
        Common3DTIGUI.SingleSpace();



        DrawHLEars(plugin);        
        DrawNonLinearAttenuation(plugin);

        DrawTemporalDistortion(plugin);
        DrawFrequencySmearing(plugin);

        // End starting play
        isStartingPlay = false;

        // End changing buttons
        //changingAudiometryPresetLeft = false;
        //changingAudiometryPresetRight = false;        
        changingCSLeft = false;
        changingCSRight = false;
        changingTDPresetLeft = false;
        changingTDPresetRight = false;
        changingFSPresetLeft = false;
        changingFSPresetRight = false;

        return showRawParameters;
    }


    /////////////////////////////////////////////////////////
    // MAIN MODULES DRAW 
    /////////////////////////////////////////////////////////

    /// <summary>
    /// Draw calibration controls 
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawCalibration(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("CALIBRATION");
        {
            Common3DTIGUI.AddLabelToParameterGroup("dB SPL for 0 dB FS");
            CreateControl(plugin, Calibration, T_ear.BOTH);
            //Common3DTIGUI.CreatePluginParameterSlider(plugin, ref HLAPI.PARAM_CALIBRATION, "HLCAL", "dB SPL for 0 dB FS", false, "dB SPL", "Set how many dB SPL are assumed for 0 dB FS");
        }
        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw ear icons for audiometry module
    /// </summary>    
    public void DrawAudiometryEars()
    {
        // LEFT EAR
        Common3DTIGUI.BeginLeftColumn(true);
        {
            // Draw ear icon
            GUILayout.BeginHorizontal();
            Common3DTIGUI.DrawEar(T_ear.LEFT);
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndLeftColumn();

        // RIGHT EAR
        Common3DTIGUI.BeginRightColumn(true);
        {
            // Draw ear icon
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            Common3DTIGUI.DrawEar(T_ear.RIGHT);
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndRightColumn();
    }


    /// <summary>
    /// Draw HL classification scale controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawAudiometryClassificationScale(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("CLASSIFICATION SCALE");
        {
            EditorGUI.BeginDisabledGroup(!(plugin.GetBoolParameter("HLONL") && plugin.GetBoolParameter("HLMBEONL")));
            // LEFT EAR
            Common3DTIGUI.BeginLeftColumn(true);
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        // Curves 
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Label("Curve:");
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_A);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_B);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_C);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_D);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_E);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_F);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_G);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_H);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_I);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_J);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_K);
                        }
                        GUILayout.EndVertical();

                        // Slopes
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Label("Slope:");
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.LEFT, 0);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.LEFT, 1);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.LEFT, 2);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.LEFT, 3);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.LEFT, 4);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.LEFT, 5);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.LEFT, 6);
                        }
                        GUILayout.EndVertical();

                        // Severities
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Label("Severity:");
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_NOLOSS);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MILD);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MILDMODERATE);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MODERATE);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MODERATESEVERE);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_SEVERE);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.LEFT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_PROFOUND);
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    //// Reset button
                    //Common3DTIGUI.SingleSpace();
                    //if (Common3DTIGUI.CreateButton("Reset", "Reset audiometry to No Hearing Loss"))
                    //    ResetAudiometry(plugin, T_ear.LEFT);
                }
                GUILayout.EndVertical();
            }
            Common3DTIGUI.EndLeftColumn();
            EditorGUI.EndDisabledGroup();

            // RIGHT EAR
            EditorGUI.BeginDisabledGroup(!(plugin.GetBoolParameter("HLONR") && plugin.GetBoolParameter("HLMBEONR")));
            Common3DTIGUI.BeginRightColumn(true);
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        // Curves 
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Label("Curve:");
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_A);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_B);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_C);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_D);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_E);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_F);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_G);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_H);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_I);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_J);
                            DrawOneClassificationScaleCurveControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_K);
                        }
                        GUILayout.EndVertical();

                        // Slopes
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Label("Slope:");
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.RIGHT, 0);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.RIGHT, 1);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.RIGHT, 2);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.RIGHT, 3);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.RIGHT, 4);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.RIGHT, 5);
                            DrawOneClassificationScaleSlopeControl(plugin, T_ear.RIGHT, 6);
                        }
                        GUILayout.EndVertical();

                        // Severities
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Label("Severity:");
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_NOLOSS);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MILD);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MILDMODERATE);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MODERATE);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MODERATESEVERE);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_SEVERE);
                            DrawOneClassificationScaleSeverityControl(plugin, T_ear.RIGHT, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_PROFOUND);
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    //// Reset button
                    //Common3DTIGUI.SingleSpace();
                    //if (Common3DTIGUI.CreateButton("Reset", "Reset audiometry to No Hearing Loss"))
                    //    ResetAudiometry(plugin, T_ear.RIGHT);
                }
                GUILayout.EndVertical();
            }
            Common3DTIGUI.EndRightColumn();
            EditorGUI.EndDisabledGroup();
        }
        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw audiometry fine adjustment controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawAudiometryFineAdjustment(IAudioEffectPlugin plugin)
    {
        Common3DTIGUI.BeginSection("FINE ADJUSTMENT");
        {
            foreach (T_ear ear in new T_ear[] { T_ear.LEFT, T_ear.RIGHT })
            {
                // LEFT EAR
                //EditorGUI.BeginDisabledGroup(!(plugin.GetBoolParameter("HLONL") && plugin.GetBoolParameter("HLMBEONL")));
                EditorGUI.BeginDisabledGroup(!(plugin.GetParameter<Parameter, bool>(HLOn, ear) && plugin.GetParameter<Parameter, bool>(MultibandExpansionOn, ear)));
                //Common3DTIGUI.BeginLeftColumn(true);
                //Common3DTIGUI.BeginColumn(ear);
                if (ear == T_ear.LEFT)
                    BeginLeftColumn(true);
                else
                    BeginRightColumn(true);
                {
                    Common3DTIGUI.AddLabelToParameterGroup("62.5 Hz");
                    Common3DTIGUI.AddLabelToParameterGroup("125 Hz");
                    Common3DTIGUI.AddLabelToParameterGroup("250 Hz");
                    Common3DTIGUI.AddLabelToParameterGroup("500 Hz");
                    Common3DTIGUI.AddLabelToParameterGroup("1 KHz");
                    Common3DTIGUI.AddLabelToParameterGroup("2 KHz");
                    Common3DTIGUI.AddLabelToParameterGroup("4 KHz");
                    Common3DTIGUI.AddLabelToParameterGroup("8 KHz");
                    Common3DTIGUI.AddLabelToParameterGroup("16 KHz");
                    for (int i = 0; i < API_3DTI_HL.NumMultibandExpansionBands; i++)
                    {
                        if (CreateControl(plugin, MultibandExpansionBand0 + i, ear))
                        {
                            ResetAllAudiometryButtonSelections(ear);
                        }
                    }

                }
                Common3DTIGUI.EndColumn(ear);
                //Common3DTIGUI.EndLeftColumn();
                EditorGUI.EndDisabledGroup();
            }
            //// RIGHT EAR
            //EditorGUI.BeginDisabledGroup(!(plugin.GetBoolParameter("HLONR") && plugin.GetBoolParameter("HLMBEONR")));
            //Common3DTIGUI.BeginRightColumn(true);
            //{
            //    Common3DTIGUI.AddLabelToParameterGroup("62.5 Hz");
            //    Common3DTIGUI.AddLabelToParameterGroup("125 Hz");
            //    Common3DTIGUI.AddLabelToParameterGroup("250 Hz");
            //    Common3DTIGUI.AddLabelToParameterGroup("500 Hz");
            //    Common3DTIGUI.AddLabelToParameterGroup("1 KHz");
            //    Common3DTIGUI.AddLabelToParameterGroup("2 KHz");
            //    Common3DTIGUI.AddLabelToParameterGroup("4 KHz");
            //    Common3DTIGUI.AddLabelToParameterGroup("8 KHz");
            //    Common3DTIGUI.AddLabelToParameterGroup("16 KHz");
            //    for (int i = 0; i < API_3DTI_HL.NumMultibandExpansionBands; i++)
            //    {
            //        if (CreateControl(plugin, MultibandExpansionBand0 + i, T_ear.RIGHT))
            //        {
            //            ResetAllAudiometryButtonSelections(T_ear.RIGHT);
            //        }
            //    }
              
            //}
            //Common3DTIGUI.EndRightColumn();
        }
        //EditorGUI.EndDisabledGroup();
        Common3DTIGUI.EndSection();
    }

    /// <summary>
    /// Draw ear icons and global on/off switches for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawHLEars(IAudioEffectPlugin plugin)
    {
   

        // LEFT EAR
        Common3DTIGUI.BeginColumn(plugin, T_ear.LEFT, API_3DTI_HL.Parameter.HLOn);
        {
            // Draw ear icon
            GUILayout.BeginHorizontal();
            Common3DTIGUI.DrawEar(T_ear.LEFT);
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndColumn(T_ear.LEFT);

        // RIGHT EAR
        Common3DTIGUI.BeginColumn(plugin, T_ear.RIGHT, API_3DTI_HL.Parameter.HLOn);
        {
            // Draw ear icon
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            Common3DTIGUI.DrawEar(T_ear.RIGHT);
            GUILayout.EndHorizontal();
        }
        Common3DTIGUI.EndColumn(T_ear.RIGHT);
    }

    /// <summary>
    /// Draw non-linear attenuation controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawNonLinearAttenuation(IAudioEffectPlugin plugin)
    {
        DrawColumnForEachEar(plugin, "3DTUNE-IN NON-LINEAR ATTENUATION", new Parameter[] { HLOn }, MultibandExpansionOn, 
            (T_ear ear) => CreateControls(plugin, ear, false, MultibandExpansionApproach, MultibandExpansionAttack, MultibandExpansionRelease)
        );

    }

    /// <summary>
    /// Draw temporal distortion controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawTemporalDistortion(IAudioEffectPlugin plugin)
    {
        bool returnValue = DrawColumnForEachEar(plugin, "3DTUNE-IN TEMPORAL DISTORTION", new Parameter[] { HLOn }, TemporalDistortionOn,
            (ear) =>
        {
            // PER EAR

            // If we are syncing then disable the right ear controls as they are apparently not used in the plugin.
            EditorGUI.BeginDisabledGroup(ear == T_ear.RIGHT && plugin.GetParameter<Parameter, bool>(TemporalDistortionLRSyncOn));
            // PRESETS
            Common3DTIGUI.SingleSpace();
            GUILayout.BeginHorizontal();
            {
                if (DrawPresetButtonsForOneEar(plugin, ear, "Temporal Distortion", ref (ear==T_ear.LEFT? ref selectedTDPresetLeft : ref selectedTDPresetRight)))
                    SetTemporalDistortionPreset(plugin, ear, ear == T_ear.LEFT ? selectedTDPresetLeft : selectedTDPresetRight);
            }
            GUILayout.EndHorizontal();
            Common3DTIGUI.SingleSpace();
             
            // CONTROLS
            bool retValue = CreateControl(plugin, TemporalDistortionBandUpperLimit, ear);
            Common3DTIGUI.BeginSubsection("Jitter generator");
            {
                retValue = CreateControls(plugin, ear, false, TemporalDistortionWhiteNoisePower, TemporalDistortionNoiseBandwidth) || retValue;
            }
            Common3DTIGUI.EndSubsection();
            EditorGUI.EndDisabledGroup();
            return retValue;
        },
        () =>
        {
            // BOTH EARS

            bool canSynchronize = plugin.GetParameter<Parameter, bool>(HLOn, T_ear.LEFT) && plugin.GetParameter<Parameter, bool>(HLOn, T_ear.RIGHT) && plugin.GetParameter<Parameter, bool>(TemporalDistortionOn, T_ear.LEFT) && plugin.GetParameter<Parameter, bool>(TemporalDistortionOn, T_ear.RIGHT);
            if (!canSynchronize)
            {
                plugin.SetParameter(TemporalDistortionLRSyncOn, false, T_ear.BOTH);
            }
            EditorGUI.BeginDisabledGroup(!canSynchronize);
            bool retValue = CreateControls(plugin, T_ear.BOTH, false, TemporalDistortionLRSyncOn, TemporalDistortionLRSyncAmount);
            EditorGUI.EndDisabledGroup();
            return retValue;
        });

    }

    /// <summary>
    /// Draw frequency smearing controls for both ears
    /// </summary>
    /// <param name="plugin"></param>
    public void DrawFrequencySmearing(IAudioEffectPlugin plugin)
    {
        DrawColumnForEachEar(plugin, "3DTUNE-IN FREQUENCY SMEARING", new Parameter[] { HLOn }, FrequencySmearingOn, (ear) =>
        {
            bool didChange = false;
            didChange = CreateControl(plugin, FrequencySmearingApproach, ear) || didChange;
            Common3DTIGUI.SingleSpace();
            GUILayout.BeginHorizontal();
            if (DrawPresetButtonsForOneEar(plugin, ear, "Frequency Smearing", ref (ear == T_ear.LEFT) ? ref selectedFSPresetLeft : ref selectedFSPresetRight))
            {
                SetFrequencySmearingPreset(plugin, ear, ear == T_ear.LEFT ? selectedFSPresetLeft : selectedFSPresetRight);
            }
            GUILayout.EndHorizontal();
            Common3DTIGUI.SingleSpace();

            Common3DTIGUI.BeginSubsection("Downward smearing");
            Common3DTIGUI.AddLabelToParameterGroup("Buffer size");
            Common3DTIGUI.AddLabelToParameterGroup("Smearing amount");

            if (plugin.GetParameter<Parameter, T_HLFrequencySmearingApproach>(FrequencySmearingApproach, ear) == T_HLFrequencySmearingApproach.Graf)
            {
                if (CreateControl(plugin, FrequencySmearingDownSize, ear))
                {
                    ResetAllFrequencySmearingButtonSelections(ear);
                    didChange = true;
                }
            }

            if (CreateControl(plugin, FrequencySmearingDownHz, ear))
            {
                ResetAllFrequencySmearingButtonSelections(ear);
                didChange = true;
            }

            Common3DTIGUI.EndSubsection();

            // Upward
            Common3DTIGUI.BeginSubsection("Upward smearing");
            Common3DTIGUI.AddLabelToParameterGroup("Buffer size");
            Common3DTIGUI.AddLabelToParameterGroup("Smearing amount");

            if (plugin.GetParameter<Parameter, T_HLFrequencySmearingApproach>(FrequencySmearingApproach, ear) == T_HLFrequencySmearingApproach.Graf)
            {
                if (CreateControl(plugin, FrequencySmearingUpSize, ear)) { 
                    ResetAllFrequencySmearingButtonSelections(ear);
                    didChange = true;
                }
            }

            if (CreateControl(plugin, FrequencySmearingUpHz, ear)) { 
                ResetAllFrequencySmearingButtonSelections(ear);
                didChange = true;
            }

            Common3DTIGUI.EndSubsection();
            return didChange;
        });

 
    }


    /////////////////////////////////////////////////////////
    // AUXILIARY DRAW METHODS
    /////////////////////////////////////////////////////////

    /// <summary>
    ///  Draw preset buttons for one ear for any module
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns>true if one button is pressed</returns>
    public bool DrawPresetButtonsForOneEar(IAudioEffectPlugin plugin, T_ear ear, string moduleName, ref API_3DTI_HL.T_HLPreset selectedPreset, bool includeNormal = true)
    {
        bool result = false;
        string earStr;

        if (ear == T_ear.LEFT)
            earStr = "left";
        else
            earStr = "right";

        if (includeNormal)
        {
            if (selectedPreset == API_3DTI_HL.T_HLPreset.HL_PRESET_NORMAL)
                GUI.color = selectedColor;
            else
                GUI.color = baseColor;
            if (GUILayout.Button(new GUIContent("Normal", "Set " + moduleName + " parameters for Normal Hearing in" + earStr + " ear"), GUILayout.ExpandWidth(false)))
            {
                selectedPreset = API_3DTI_HL.T_HLPreset.HL_PRESET_NORMAL;
                result = true;
            }
        }

        if (selectedPreset == API_3DTI_HL.T_HLPreset.HL_PRESET_MILD)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button(new GUIContent("Mild", "Set " + moduleName + " parameters for Mild hearing loss in" + earStr + " ear"), GUILayout.ExpandWidth(false)))
        {
            selectedPreset = API_3DTI_HL.T_HLPreset.HL_PRESET_MILD;
            result = true;
        }

        if (selectedPreset == API_3DTI_HL.T_HLPreset.HL_PRESET_MODERATE)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button(new GUIContent("Moderate", "Set " + moduleName + " parameters for Moderate hearing loss in" + earStr + " ear"), GUILayout.ExpandWidth(false)))
        {
            selectedPreset = API_3DTI_HL.T_HLPreset.HL_PRESET_MODERATE;
            result = true;
        }

        if (selectedPreset == API_3DTI_HL.T_HLPreset.HL_PRESET_SEVERE)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;
        if (GUILayout.Button(new GUIContent("Severe", "Set " + moduleName + " parameters for Severe hearing loss in" + earStr + " ear"), GUILayout.ExpandWidth(false)))
        {
            selectedPreset = API_3DTI_HL.T_HLPreset.HL_PRESET_SEVERE;
            result = true;
        }
        GUI.color = baseColor;
        return result;
    }

    /// <summary>
    /// Draw controls for one curve (letter) of audiometry classification scale
    /// </summary>
    /// <param name="letter"></param>
    public void DrawOneClassificationScaleCurveControl(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLClassificationScaleCurve curve)
    {
        string earName = "";
        API_3DTI_HL.T_HLClassificationScaleCurve selectedCurve;
        if (ear == T_ear.LEFT)
        {
            earName = "left";
            selectedCurve = selectedCurveLeft;
        }
        else
        {
            earName = "right";
            selectedCurve = selectedCurveRight;
        }

        char letter = API_3DTI_HL.FromClassificationScaleCurveToChar(curve);

        GUILayout.BeginHorizontal();
        {
            // Draw curve image
            Common3DTIGUI.DrawImage("curve" + letter, csCurveWidth, csCurveHeight);

            // Set color of button, depending on selection
            if (selectedCurve == curve)
                GUI.color = selectedColor;
            else
                GUI.color = baseColor;

            // Create button with letter and do action if pressed
            if (Common3DTIGUI.CreateButton(letter.ToString(), "Select " + API_3DTI_HL.FromClassificationScaleCurveToString(curve) + " curve of HL classification scale for " + earName + " ear"))
            {
                SetClassificationScaleCurve(plugin, ear, curve);
                if (ear == T_ear.LEFT)
                    selectedCurveLeft = curve;
                else
                    selectedCurveRight = curve;
            }
        }
        GUILayout.EndHorizontal();
        GUI.color = baseColor;
    }

    /// <summary>
    /// Draw controls for one slope of audiometry classification scale
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="ear"></param>
    /// <param name="slope"></param>
    public void DrawOneClassificationScaleSlopeControl(IAudioEffectPlugin plugin, T_ear ear, int slope)
    {
        string earName = "";
        int selectedSlope;
        if (ear == T_ear.LEFT)
        {
            earName = "left";
            selectedSlope = selectedSlopeLeft;
        }
        else
        {
            earName = "right";
            selectedSlope = selectedSlopeRight;
        }

        // Set color of button, depending on selection
        if (selectedSlope == slope)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;

        // Create button with slope level and do action if pressed        
        GUILayout.BeginHorizontal();
        {
            Common3DTIGUI.DrawBlank(csSlopeWidth, csSlopeHeight);   // Blank space for indentation of buttons wrt "Slope" label

            if (Common3DTIGUI.CreateButton(slope.ToString(), "Select " + slope.ToString() + " slope of HL classification scale for " + earName + " ear"))
            {
                SetClassificationScaleSlope(plugin, ear, slope);
                if (ear == T_ear.LEFT)
                    selectedSlopeLeft = slope;
                else
                    selectedSlopeRight = slope;
            }
        }
        GUILayout.EndHorizontal();

        GUI.color = baseColor;
    }

    /// <summary>
    /// Draw controls for one severity of audiometry classification scale
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="ear"></param>
    /// <param name="severity"></param>
    public void DrawOneClassificationScaleSeverityControl(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLClassificationScaleSeverity severity)
    {
        string earName = "";
        API_3DTI_HL.T_HLClassificationScaleSeverity selectedSeverity;
        if (ear == T_ear.LEFT)
        {
            earName = "left";
            selectedSeverity = selectedSeverityLeft;
        }
        else
        {
            earName = "right";
            selectedSeverity = selectedSeverityRight;
        }

        // Set color of button, depending on selection
        if (selectedSeverity == severity)
            GUI.color = selectedColor;
        else
            GUI.color = baseColor;

        // Create button with severity level and do action if pressed
        string severityStr = API_3DTI_HL.FromClassificationScaleSeverityToString(severity);
        if (Common3DTIGUI.CreateButton(severityStr, "Select " + severityStr + " (" + API_3DTI_HL.FromClassificationScaleSeverityToInt(severity) + ") severity of HL classification scale for " + earName + " ear"))
        {
            SetClassificationScaleSeverity(plugin, ear, severity);
            if (ear == T_ear.LEFT)
                selectedSeverityLeft = severity;
            else
                selectedSeverityRight = severity;
        }
        GUI.color = baseColor;
    }


    public void SetClassificationScale(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLClassificationScaleCurve curve, int slope, API_3DTI_HL.T_HLClassificationScaleSeverity severity)
    {        
        List<float> hl;
        API_3DTI_HL.GetClassificationScaleHL(curve, slope, severity, out hl);
        for (int i=0; i< Math.Min(NumMultibandExpansionBands, hl.Count); i++)
        {
            plugin.SetParameter(MultibandExpansionBand0 + i, hl[i], ear);
        }
        if (ear.HasFlag(T_ear.LEFT))
        {
            selectedCurveLeft = curve;
            selectedSlopeLeft = slope;
            selectedSeverityLeft = severity;
        }
        if (ear.HasFlag(T_ear.RIGHT))
        {
            selectedCurveRight = curve;
            selectedSlopeRight = slope;
            selectedSeverityRight = severity;
        }
    }

    /// <summary>
    /// Set audiometry from new classification scale curve letter
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="letter"></param>
    public void SetClassificationScaleCurve(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLClassificationScaleCurve curve)
    {
        if (ear == T_ear.LEFT)
        {
            PARAM_CLASSIFICATION_CURVE_LEFT = curve;
            SetClassificationScale(plugin, ear, curve, PARAM_CLASSIFICATION_SLOPE_LEFT, PARAM_CLASSIFICATION_SEVERITY_LEFT);             
            changingCSLeft = true;
        }
        else
        {
            PARAM_CLASSIFICATION_CURVE_RIGHT = curve;
            SetClassificationScale(plugin, ear, curve, PARAM_CLASSIFICATION_SLOPE_RIGHT, PARAM_CLASSIFICATION_SEVERITY_RIGHT);             
            changingCSRight = true;
        }
        //ResetAllAudiometryButtonSelections(ear);
    }

    /// <summary>
    /// Set audiometry from new classification scale slope
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="slope"></param>
    public void SetClassificationScaleSlope(IAudioEffectPlugin plugin, T_ear ear, int slope)
    {
        if (ear == T_ear.LEFT)
        {
            PARAM_CLASSIFICATION_SLOPE_LEFT = slope; 
            SetClassificationScale(plugin, ear, PARAM_CLASSIFICATION_CURVE_LEFT, slope, PARAM_CLASSIFICATION_SEVERITY_LEFT);            
            changingCSLeft = true;
        }
        else
        {
            PARAM_CLASSIFICATION_SLOPE_RIGHT = slope; 
            SetClassificationScale(plugin, ear, PARAM_CLASSIFICATION_CURVE_RIGHT, slope, PARAM_CLASSIFICATION_SEVERITY_RIGHT);            
            changingCSRight = true;
        }
    }

    /// <summary>
    /// Set audiometry from new classification scale severity
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="severity"></param>
    public void SetClassificationScaleSeverity(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLClassificationScaleSeverity severity)
    {
        if (ear == T_ear.LEFT)
        {
            PARAM_CLASSIFICATION_SEVERITY_LEFT = severity; 
            SetClassificationScale(plugin, ear, PARAM_CLASSIFICATION_CURVE_LEFT, PARAM_CLASSIFICATION_SLOPE_LEFT, severity);            
            changingCSLeft = true;           
        }
        else
        {
            PARAM_CLASSIFICATION_SEVERITY_RIGHT = severity; 
            SetClassificationScale(plugin, ear, PARAM_CLASSIFICATION_CURVE_RIGHT, PARAM_CLASSIFICATION_SLOPE_RIGHT, severity);            
            changingCSRight = true;
        }
        //ResetAllAudiometryButtonSelections(ear);
    }


    /// <summary>
    /// Clear activation of all audiometry buttons
    /// </summary>
    /// <param name="ear"></param>
    public void ResetAllAudiometryButtonSelections(T_ear ear)
    {
        if (ear == T_ear.LEFT)
        {
            //if (!changingAudiometryPresetLeft)
            //    selectedAudiometryPresetLeft = API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
            if (!changingCSLeft)
            {
                selectedCurveLeft = API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_UNDEFINED;
                selectedSlopeLeft = -1;
                selectedSeverityLeft = API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_UNDEFINED;
            }
        }
        else
        {
            //if (!changingAudiometryPresetRight)
            //    selectedAudiometryPresetRight = API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
            if (!changingCSRight)
            {
                selectedCurveRight = API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_UNDEFINED;
                selectedSlopeRight = -1;
                selectedSeverityRight = API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_UNDEFINED;
            }
        }
    }

    /// <summary>
    /// Clear activation of temporal distortion preset buttons
    /// </summary>
    /// <param name="ear"></param>
    public void ResetAllTemporalDistortionButtonSelections(T_ear ear)
    {
        if (ear == T_ear.LEFT)
        {
            if (!changingTDPresetLeft)
                selectedTDPresetLeft = API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
        }
        else
        {
            if (!changingTDPresetRight)
                selectedTDPresetRight= API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
        }
    }

    /// <summary>
    /// Set all parameters of Temporal Distortion from one preset
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="ear"></param>
    /// <param name="preset"></param>
    public void SetTemporalDistortionPreset(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLPreset preset)
    {
        Debug.Assert(ear != T_ear.BOTH);

        API_3DTI_HL.T_HLTemporalDistortionBandUpperLimit bandUpperLimit;
        float whiteNoisePower;
        float bandwidth;
        float LRSync; 
        API_3DTI_HL.GetTemporalDistortionPresetValues(preset, out bandUpperLimit, out whiteNoisePower, out bandwidth, out LRSync);

        if (plugin.GetParameter<Parameter, bool>(TemporalDistortionOn, ear))
        {
            plugin.SetParameter(TemporalDistortionBandUpperLimit, FromBandUpperLimitEnumToFloat(bandUpperLimit), ear);
            plugin.SetParameter(TemporalDistortionWhiteNoisePower, whiteNoisePower, ear);
            plugin.SetParameter(TemporalDistortionNoiseBandwidth, bandwidth, ear);
        }
        else
        {
            plugin.GetFloatParameterInfo(TemporalDistortionWhiteNoisePower, ear, out float minValue, out float _, out float _);
            plugin.SetParameter(TemporalDistortionWhiteNoisePower, minValue, ear);
        }
        plugin.SetParameter(TemporalDistortionLRSyncAmount, LRSync);

        if (ear == T_ear.LEFT)
        {
            changingTDPresetLeft = true;
        } 
        else
        {
            changingTDPresetRight = true;
        }


    }

    /// <summary>
    /// Clear activation of frequency smearing preset buttons
    /// </summary>
    /// <param name="ear"></param>
    public void ResetAllFrequencySmearingButtonSelections(T_ear ear)
    {
        if (ear == T_ear.LEFT)
        {
            if (!changingFSPresetLeft)
                selectedFSPresetLeft = API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
        }
        else
        {
            if (!changingFSPresetRight)
                selectedFSPresetRight = API_3DTI_HL.T_HLPreset.HL_PRESET_CUSTOM;
        }
    }

    /// <summary>
    /// Set all parameters of Frequency Smearing from one preset
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="ear"></param>
    /// <param name="preset"></param>
    public void SetFrequencySmearingPreset(IAudioEffectPlugin plugin, T_ear ear, API_3DTI_HL.T_HLPreset preset)
    {
        int downSize, upSize;
        float downHz, upHz;
        API_3DTI_HL.GetFrequencySmearingPresetValues(preset, out downSize, out upSize, out downHz, out upHz);

        plugin.SetParameter(FrequencySmearingDownSize, downSize, ear);
        plugin.SetParameter(FrequencySmearingDownHz, downHz, ear);
        plugin.SetParameter(FrequencySmearingUpSize, upSize, ear);
        plugin.SetParameter(FrequencySmearingUpHz, upHz, ear);
    }
}
