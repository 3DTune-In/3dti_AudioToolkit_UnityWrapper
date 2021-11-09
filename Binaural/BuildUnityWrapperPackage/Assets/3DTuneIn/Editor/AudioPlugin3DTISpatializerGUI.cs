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
using System.Collections.Generic;
using UnityEditor;
using API_3DTI_Common;
using System;
using System.Linq;

namespace API_3DTI
{

    using static API_3DTI_Spatializer;

    [CustomEditor(typeof(API_3DTI_Spatializer))]
    public class AudioPlugin3DTISpatializerGUI : Editor
    {


        API_3DTI_Spatializer toolkit;
        bool perSourceAdvancedSetup = false;
        bool advancedSetup = false;
        bool haSetup = false;


        //////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// This is where we create the layout for the inspector
        /// </summary>
        public override void OnInspectorGUI()
        {



            toolkit = (API_3DTI_Spatializer)target; // Get access to API script       
            Common3DTIGUI.InitStyles(); // Init styles

            // Show 3D-Tune-In logo         
            Common3DTIGUI.Show3DTILogo();

            // Show About button
            Common3DTIGUI.ShowAboutButton();

            ////// LISTENER                
            DrawControls();





            if (GUI.changed)
            {

                // Test for problematic config
                toolkit.GetSampleRate(out TSampleRateEnum currentSampleRate);
                SpatializationMode currentSpatializationMode = toolkit.GetParameter<SpatializationMode>(SpatializerParameter.SpatializationMode);
                if (currentSpatializationMode == SpatializationMode.SPATIALIZATION_MODE_HIGH_PERFORMANCE && toolkit.GetBinaryResourcePath(BinaryResourceRole.HighPerformanceILD, currentSampleRate).Length == 0)
                {
                    Debug.LogError($"Default spatialization mode set to {SpatializationMode.SPATIALIZATION_MODE_HIGH_PERFORMANCE} but no {BinaryResourceRole.HighPerformanceILD} resource is loaded for the current sample rate ({currentSampleRate}).");
                }
                if (currentSpatializationMode == SpatializationMode.SPATIALIZATION_MODE_HIGH_QUALITY && toolkit.GetBinaryResourcePath(BinaryResourceRole.HighQualityHRTF, currentSampleRate).Length == 0)
                {
                    Debug.LogError($"Default spatialization mode set to {SpatializationMode.SPATIALIZATION_MODE_HIGH_QUALITY} but no {BinaryResourceRole.HighQualityHRTF} resource is loaded for the current sample rate ({currentSampleRate}).");
                }
                if (currentSpatializationMode == SpatializationMode.SPATIALIZATION_MODE_HIGH_QUALITY && toolkit.GetParameter<bool>(SpatializerParameter.EnableNearFieldILD) && toolkit.GetBinaryResourcePath(BinaryResourceRole.HighQualityILD, currentSampleRate).Length == 0)
                {
                    Debug.LogError($"Default spatialization mode set to {SpatializationMode.SPATIALIZATION_MODE_HIGH_QUALITY} with {SpatializerParameter.EnableNearFieldILD} enabled but no {BinaryResourceRole.HighQualityILD} resource is loaded for the current sample rate ({currentSampleRate}).");
                }
                if (toolkit.GetParameter<bool>(SpatializerParameter.EnableReverbSend) && toolkit.GetBinaryResourcePath(BinaryResourceRole.ReverbBRIR, currentSampleRate).Length == 0)
                {
                    Debug.LogError($"{SpatializerParameter.EnableReverbSend} is set to true but no {BinaryResourceRole.ReverbBRIR} resource is loaded for the current sample rate ({currentSampleRate}).");
                }
                if (toolkit.GetParameter<bool>(SpatializerParameter.EnableReverbProcessing) && toolkit.GetBinaryResourcePath(BinaryResourceRole.ReverbBRIR, currentSampleRate).Length == 0)
                {
                    Debug.LogError($"{SpatializerParameter.EnableReverbProcessing} is set to true but no {BinaryResourceRole.ReverbBRIR} resource is loaded for the current sample rate ({currentSampleRate}).");
                }

                // TODO: See if this results in unsynced state with DLL
                Undo.RecordObject(toolkit, "Modify 3DTI Spatializer parameter");
                EditorUtility.SetDirty(toolkit);
            }
        }







        // Create a control for a SpatializerParameter parameter. Returns true if the value changed
        public bool CreateControl(SpatializerParameter parameter, float overrideMin = float.NaN, float overrideMax = float.NaN)
        {
            SpatializerParameterAttribute p = parameter.GetAttribute<SpatializerParameterAttribute>();
            if (p == null)
            {
                throw new Exception($"Failed to find SpatializerParameterAttribute for parameter {parameter}");
            }

            string label = p.label;
            string description = p.description;
            if (p.isSourceParameter)
            {
                //label += " (initial value)";
                description += "\n\n(This parameter may be modified individually on instantiated audio sources. The value here is the default when creating a new source.)";
            }

            Common3DTIGUI.SingleSpace();

            if (p.type == typeof(float) || p.type == typeof(int))
            {
                toolkit.GetFloatParameter(parameter, out float oldValue);
                float newValue;
                string valueString;

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                Common3DTIGUI.AddLabelToParameterGroup(label);
                GUILayout.Label(new GUIContent(label, description), Common3DTIGUI.parameterLabelStyle, GUILayout.Width(Common3DTIGUI.GetParameterLabelWidth()));
                float min = float.IsNaN(overrideMin) ? p.min : overrideMin;
                float max = float.IsNaN(overrideMax) ? p.max : overrideMax;
                newValue = GUILayout.HorizontalSlider(oldValue, min, max, GUILayout.ExpandWidth(true));
                valueString = GUILayout.TextField(newValue.ToString(p.type == typeof(float) ? "F2" : "F0", System.Globalization.CultureInfo.InvariantCulture), GUILayout.ExpandWidth(false));
                GUILayout.Label(p.units, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                bool parseOk = float.TryParse(valueString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float parsedValueString);
                if (parseOk)
                {
                    newValue = parsedValueString;
                }
                if (p.validValues != null && p.validValues.Length > 0)
                {
                    // Lock to nearest valid value
                    newValue = p.validValues.OrderBy(x => Math.Abs(x - newValue)).First();
                }

                if (newValue != oldValue)
                {
                    toolkit.SetFloatParameter(parameter, newValue);
                    return true;
                }
                return false;
            }
            else if (p.type == typeof(bool))
            {
                bool oldValue = toolkit.GetFloatParameter(parameter) != 0.0f;
                bool newValue = GUILayout.Toggle(oldValue, new GUIContent(label, description), GUILayout.ExpandWidth(true));
                if (newValue != oldValue)
                {
                    bool setOK = toolkit.SetFloatParameter(parameter, Convert.ToSingle(newValue));
                    Debug.Assert(setOK);
                    return true;
                }
                return false;
            }
            else if (p.type.IsEnum)
            {
                toolkit.GetFloatParameter(parameter, out float oldFloatValue);
                int oldValue = (int)oldFloatValue;
                Debug.Assert(Enum.GetUnderlyingType(p.type) == typeof(int));
                int[] values = (int[])Enum.GetValues(p.type);
                if (!values.Contains(oldValue))
                {
                    Debug.LogWarning($"Plugin returned invalid value for {p.label}: {oldValue}");
                }

                int defaultValue = (int)Enum.GetValues(p.type).GetValue(0);

                int newValue = (int)(object)EditorGUILayout.Popup(new GUIContent(label, description), values.Contains(oldValue) ? oldValue : defaultValue, Enum.GetNames(p.type));
                Debug.Assert(Enum.IsDefined(p.type, newValue));

                if (newValue != oldValue)
                {
                    bool setOK = toolkit.SetFloatParameter(parameter, newValue);
                    Debug.Assert(setOK);
                    return true;
                }
                return false;
            }
            else
            {
                throw new Exception($"Cannot create GUI control for Parameter of type {p.type}.");
            }
        }



        public static (string prefix, List<string> paths, string suffix) GetBinaryResourcePaths(BinaryResourceRole role, TSampleRateEnum sampleRate)
        {
            // The DLL 
            //string rootDirectory = "Assets/3DTuneIn/Resources/";
            string prefix;
            switch (role)
            {
                case BinaryResourceRole.HighPerformanceILD:
                    prefix = "Data/HighPerformance/ILD/";
                    break;
                case BinaryResourceRole.HighQualityHRTF:
                    prefix = "Data/HighQuality/HRTF/";
                    break;
                case BinaryResourceRole.HighQualityILD:
                    prefix = "Data/HighQuality/ILD/";
                    break;
                case BinaryResourceRole.ReverbBRIR:
                    prefix = "Data/Reverb/BRIR/";
                    break;
                default:
                    throw new Exception("Invalid value for BinaryResourceRole.");
            }

            string sampleRateLabel =
                sampleRate == TSampleRateEnum.K44 ? "44100"
                : sampleRate == TSampleRateEnum.K48 ? "48000"
                : sampleRate == TSampleRateEnum.K96 ? "96000"
                : "(unknown sample rate)";
            // LoadAll searches relative to any "resources" folder in the project
            List<string> paths = Resources.LoadAll<TextAsset>(prefix)
                        .Where(x => x.name.Contains(sampleRateLabel))
                        .Select(item => item.name).ToList();
            return (prefix, paths, ".bytes");
        }


        public static string CreateBinaryResourceSelector(string currentSelection, string titleText, string tooltip, BinaryResourceRole role, TSampleRateEnum sampleRate)
        {
            (string prefix, List<string> items, string suffix) = GetBinaryResourcePaths(role, sampleRate);

            // For no binary selected
            const string NoneSelectedLabel = "(None)";
            items.Insert(0, NoneSelectedLabel);

            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PrefixLabel(new GUIContent(titleText, tooltip), parameterLabelStyle, GUILayout.Width(GetParameterLabelWidth()));
            int selectedIndex = -1;
            if (currentSelection.Length > prefix.Length + suffix.Length && currentSelection.StartsWith(prefix) && currentSelection.EndsWith(suffix))
            {
                string trimmedTarget = currentSelection.Remove(currentSelection.Length - suffix.Length).Remove(0, prefix.Length);
                selectedIndex = items.IndexOf(trimmedTarget);
            }
            else if (currentSelection == "")
            {
                selectedIndex = 0;
            }
            else
            {
                Debug.LogWarning("Unable to find previously selected binary resource: " + currentSelection);
            }
            int newSelectedIndex = EditorGUILayout.Popup(new GUIContent(titleText, tooltip), selectedIndex, items.ToArray());
            EditorGUILayout.EndHorizontal();
            return newSelectedIndex <= 0 ? "" : (prefix + items[newSelectedIndex] + suffix);
        }




        ///////////////////////////////////////////////////////////
        // GUI ELEMENT ACTIONS
        ///////////////////////////////////////////////////////////




        /// <summary>
        /// Action for slider HeadRadius
        /// </summary>
        public void SliderHeadRadius()
        {
            //toolkit.SetFloatParameter(API_3DTI_Spatializer.FloatParameter.HeadRadius, )
            //toolkit.SetHeadRadius(toolkit.listenerHeadRadius);
        }

        /// <summary>
        /// Action for input ResamplingStep
        /// </summary>
        public void InputResamplingStep()
        {
            //toolkit.SetHRTFResamplingStep(toolkit.HRTFstep);
        }

        /// <summary>
        /// Action for slider Scale
        /// </summary>
        public void SliderScale()
        {
            //toolkit.SetScaleFactor(toolkit.scaleFactor);
        }

        /// <summary>
        /// Action for slider Anechoic Attenuation
        /// </summary>
        public void SliderAnechoicAttenuation()
        {
            //toolkit.SetMagnitudeAnechoicAttenuation(toolkit.magAnechoicAttenuation);
        }

        /// <summary>
        /// Action for slider Sound Speed
        /// </summary>
        public void SliderSoundSpeed()
        {
            //toolkit.SetMagnitudeSoundSpeed(toolkit.magSoundSpeed);
        }

        /// <summary>
        ///  Action for slider HA Directionality Left
        /// </summary>
        public void SliderHADirectionalityLeft()
        {
            //toolkit.SetHADirectionalityExtend(T_ear.LEFT, toolkit.HADirectionalityExtendLeft);
        }

        /// <summary>
        ///  Action for slider HA Directionality Right 
        /// </summary>
        public void SliderHADirectionalityRight()
        {
            //toolkit.SetHADirectionalityExtend(T_ear.RIGHT, toolkit.HADirectionalityExtendRight);
        }


        ///////////////////////////////////////////////////////////
        // PANEL CONTENTS
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Draw panel with Listener configuration
        /// </summary>
        public void DrawControls()
        {
            Common3DTIGUI.BeginSection("DEFAULT SETTINGS FOR NEW SOUND SOURCES");

            GUILayout.Label("These parameters may be set individually on each individual AudioSource component. The values here determine their default values for new AudioSources.\n\nPlease ensure you select binary resources below for the sample rates and spatialization mode combinations you intend to use.", Common3DTIGUI.commentStyle);

            CreateControl(SpatializerParameter.SpatializationMode);

            Common3DTIGUI.SingleSpace();

            perSourceAdvancedSetup = Common3DTIGUI.CreateFoldoutToggle(ref perSourceAdvancedSetup, "Advanced");
            if (perSourceAdvancedSetup)
            {
                Common3DTIGUI.BeginSection();

                CreateControl(SpatializerParameter.EnableReverbSend);
                CreateControl(SpatializerParameter.EnableHRTFInterpolation);
                CreateControl(SpatializerParameter.EnableFarDistanceEffect);
                CreateControl(SpatializerParameter.EnableDistanceAttenuationAnechoic);
                CreateControl(SpatializerParameter.EnableDistanceAttenuationReverb);
                // For High Quality only
                CreateControl(SpatializerParameter.EnableNearFieldILD);

                Common3DTIGUI.EndSection();

            }

            Common3DTIGUI.EndSection();



            Common3DTIGUI.BeginSection("LISTENER SETUP");

            // HIGH PERFORMANCE / HIGH QUALITY CHOICE:

            void createDropdowns(BinaryResourceRole role, string label, string tooltip)
            {
                (TSampleRateEnum, string)[] AllSampleRates = {
                (TSampleRateEnum.K44, "44.1 kHz"),
                (TSampleRateEnum.K48, "48 kHz"),
                (TSampleRateEnum.K96, "96 kHz")
            };
                foreach ((TSampleRateEnum sampleRate, string sampleRateLabel) in AllSampleRates)
                {
                    // Paths should be relative to a Resources folder.
                    string oldPath = toolkit.GetBinaryResourcePath(role, sampleRate);
                    string newPath = CreateBinaryResourceSelector(oldPath, label + " " + sampleRateLabel, tooltip, role, sampleRate);
                    if (oldPath != newPath)
                    {
                        toolkit.SetBinaryResourcePath(role, sampleRate, newPath);
                        if (newPath.EndsWith(".sofa.bytes"))
                        {
                            Debug.Log("Notice: SOFA HRTF files are only supported on Windows x64 and Mac OS.");
                        }
                    }
                }
            }

            Common3DTIGUI.SingleSpace();

            GUILayout.Label("Binary resources for High Performance mode", Common3DTIGUI.subtitleBoxStyle);
            GUILayout.Label("These are required for AudioSources to be able to spatialize in High Performance mode.", Common3DTIGUI.commentStyle);



            // HIGH PERFORMANCE MODE CONTROLS
            {
                Common3DTIGUI.AddLabelToParameterGroup("High Performance ILD");

                createDropdowns(BinaryResourceRole.HighPerformanceILD, "ILD", "Select the high performance ILD filter of the listener from a .3dti-ild file");
            }

            Common3DTIGUI.SectionSpace();
            GUILayout.Label("Binary resources for High Quality mode", Common3DTIGUI.subtitleBoxStyle);
            GUILayout.Label("These are required for AudioSources to be able to spatialize in High Quality mode.", Common3DTIGUI.commentStyle);

            // HIGH QUALITY MODE CONTROLS
            {
                Common3DTIGUI.AddLabelToParameterGroup("HRTF");
                Common3DTIGUI.AddLabelToParameterGroup("Near Field Filter ILD");


                // HRTF:
                createDropdowns(BinaryResourceRole.HighQualityHRTF, "HRTF", "Select the HRTF of the listener from a .3dti-hrtf file");

                // ILD:
                Common3DTIGUI.SingleSpace();
                createDropdowns(BinaryResourceRole.HighQualityILD, "ILD", "Select the ILD near field filter of the listener from a .3dti-ild file");


            }


            Common3DTIGUI.SectionSpace();

            // BRIR Reverb:
            GUILayout.Label("Binary resources for Reverb", Common3DTIGUI.subtitleBoxStyle);
            GUILayout.Label("These are required to enable reverb processing.", Common3DTIGUI.commentStyle);

            createDropdowns(BinaryResourceRole.ReverbBRIR, "BRIR", "Select the BRIR (impulse response) for reverb processing");

            CreateControl(SpatializerParameter.EnableReverbProcessing);


            // ITD:    
            {
                CreateControl(SpatializerParameter.EnableCustomITD);
                if (toolkit.GetFloatParameter(SpatializerParameter.EnableCustomITD) != 0.0f)
                {
                    CreateControl(SpatializerParameter.HeadRadius);
                }
            }

            Common3DTIGUI.SectionSpace();

            


            advancedSetup = Common3DTIGUI.CreateFoldoutToggle(ref advancedSetup, "Advanced Listener settings");
            if (advancedSetup)
            {
                Common3DTIGUI.BeginSection();

                Common3DTIGUI.SingleSpace();
                CreateControl(SpatializerParameter.ScaleFactor, 0.1f, 10.0f);

                // HRTF interpolation
                Common3DTIGUI.BeginSubsection("HRTF Interpolation");
                CreateControl(SpatializerParameter.HRTFResamplingStep);
                Common3DTIGUI.EndSubsection();


                // Magnitudes
                Common3DTIGUI.BeginSubsection("Physical magnitudes");
                Common3DTIGUI.AddLabelToParameterGroup("Anechoic distance attenuation");
                Common3DTIGUI.AddLabelToParameterGroup("Sound speed");
                CreateControl(SpatializerParameter.AnechoicDistanceAttenuation);
                CreateControl(SpatializerParameter.ILDAttenuation);
                CreateControl(SpatializerParameter.SoundSpeed);
                Common3DTIGUI.EndSubsection();

                // Limiter
                Common3DTIGUI.BeginSubsection("Limiter");
                Common3DTIGUI.AddLabelToParameterGroup("Switch Limiter");
                CreateControl(SpatializerParameter.EnableLimiter);
                Common3DTIGUI.EndSubsection();

                Common3DTIGUI.BeginSubsection("Reverb");
                CreateControl(SpatializerParameter.ReverbOrder);
                Common3DTIGUI.EndSubsection();

                //// Debug Log
                //Common3DTIGUI.BeginSubsection("Debug log");
                //Common3DTIGUI.AddLabelToParameterGroup("Write debug log file");
                //    if (Common3DTIGUI.CreateToggle(ref toolkit.debugLog, "Write debug log file", "Enable writing of 3DTi_BinauralSpatializer_DebugLog.txt file in your project root folder; This file can be sent to the 3DTi Toolkit developers for support", isStartingPlay))
                //        toolkit.SendWriteDebugLog(toolkit.debugLog);
                //Common3DTIGUI.EndSubsection();

                Common3DTIGUI.EndSection();
            }


            Common3DTIGUI.SectionSpace();

            haSetup = Common3DTIGUI.CreateFoldoutToggle(ref haSetup, "Hearing Aid Directionality Setup");
            if (haSetup)
            {
                Common3DTIGUI.BeginSection();


                Common3DTIGUI.SingleSpace();

                // Left ear
                Common3DTIGUI.BeginSubsection("Left ear");
                Common3DTIGUI.AddLabelToParameterGroup("Switch Directionality");
                Common3DTIGUI.AddLabelToParameterGroup("Directionality extend");
                CreateControl(SpatializerParameter.EnableHearingAidDirectionalityLeft);
                CreateControl(SpatializerParameter.HearingAidDirectionalityAttenuationLeft);
                Common3DTIGUI.EndSubsection();

                // Right ear
                Common3DTIGUI.BeginSubsection("Right ear");
                Common3DTIGUI.AddLabelToParameterGroup("Switch Directionality");
                Common3DTIGUI.AddLabelToParameterGroup("Directionality extend");
                CreateControl(SpatializerParameter.EnableHearingAidDirectionalityRight);
                CreateControl(SpatializerParameter.HearingAidDirectionalityAttenuationRight);
                Common3DTIGUI.EndSubsection();
                Common3DTIGUI.EndSection();
            }
            Common3DTIGUI.EndSection();

        }

    }
}