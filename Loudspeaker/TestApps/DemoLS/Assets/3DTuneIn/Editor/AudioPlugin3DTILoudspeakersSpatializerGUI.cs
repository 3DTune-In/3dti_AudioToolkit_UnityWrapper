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
    API_3DTI_LoudSpeakersSpatializer toolkitAPI;
    bool advancedSetup = false;

    // Limit possible values of sliders   
    float minScale = 0.1f;
    float maxScale = 10.0f;
    float minStructSide = 1.25f;
    float maxStructSide = 3.0f;
    //float minStructureYawPitch = -180.0f;
    //float maxStructureYawPitch = 180.0f;
    float minDB = -30.0f;
    float maxDB = 0.0f;
    float maxSoundSpeed = 1000.0f;

    //////////////////////////////////////////////////////////////////////////////

    
    /// <summary> This is where we create the layout for the inspector </summary>
    public override void OnInspectorGUI()
    {        
        toolkitAPI = (API_3DTI_LoudSpeakersSpatializer)target;      // Get access to API script        
        Common3DTIGUI.InitStyles();         // Init styles        
        Common3DTIGUI.Show3DTILogo();       // Show 3D-Tune-In logo                 
        Common3DTIGUI.ShowAboutButton();    // Show About button
        DrawSpeakersConfigurationPanel();   // SPEAKERS CONFIGURATION        
        DrawAdvancedPanel();                // ADVANCED SETUP
    }


    ///////////////////////////////////////////////////////////
    // GUI ELEMENT ACTIONS
    ///////////////////////////////////////////////////////////

    /// <summary>
    /// Action for slider Scale
    /// </summary>
    public void SliderScale()
    {
        toolkitAPI.SetScaleFactor(toolkitAPI.scaleFactor);
    }

    /// <summary>
    /// Action for slider Anechoic Attenuation
    /// </summary>
    public void SliderAnechoicAttenuation()
    {
        toolkitAPI.SetMagnitudeAnechoicAttenuation(toolkitAPI.magAnechoicAttenuation);
    }

    /// <summary>
    /// Action for slider Sound Speed
    /// </summary>
    public void SliderSoundSpeed()
    {
        toolkitAPI.SetMagnitudeSoundSpeed(toolkitAPI.magSoundSpeed);
    }

    /// <summary>
    /// Action for slider Structure side
    /// </summary>
    public void SliderStructureSide()
    {
        toolkitAPI.SetStructureSide(toolkitAPI.structureSide);
    }
    
    /// <summary>
    /// Draw panel for choosing speakers configuration preset
    /// </summary>
    public void DrawSpeakersConfigurationPanel()
    {
        Common3DTIGUI.BeginSection("SPEAKER CONFIGURATION PRESETS");
        {
            // Radio buttons
            int presetInt = (int)toolkitAPI.speakersConfigurationPreset;
            if (Common3DTIGUI.CreateRadioButtons(ref presetInt, 
                                                 new List<string>(new string[] { "Cube", "Octahedron", "2D Square" }),
                                                 new List<string>(new string[] { "Set cube configuration preset", "Set octahedron configuration preset", "Set 2D square configuration preset" })))
            {
                toolkitAPI.SetSpeakersConfigurationPreset((API_3DTI_LoudSpeakersSpatializer.T_LoudSpeakerConfigurationPreset)presetInt);
            }                
            // First time the number of speakers has been init to 0, we have to update it
            if (toolkitAPI.GetNumberOfSpeakers() == 0)
            {
                toolkitAPI.SetSpeakersConfigurationPreset((API_3DTI_LoudSpeakersSpatializer.T_LoudSpeakerConfigurationPreset)presetInt);
            }
            Common3DTIGUI.CreateReadonlyFloatText("Number of speakers", "", "", "Number of speakers of selected speakers configuration preset", toolkitAPI.GetNumberOfSpeakers());     // Show number of speakers 
            Common3DTIGUI.SingleSpace();

            // Structure side         
            Common3DTIGUI.AddLabelToParameterGroup("Structure side");   
            Common3DTIGUI.CreateFloatSlider(ref toolkitAPI.structureSide, "Structure side", "F2", "m", "Set one side of the speakers configuration structure, in meters", minStructSide, maxStructSide, SliderStructureSide);
            //Common3DTIGUI.CreateReadonlyFloatText("Minimum distance to listener", "F2", "m", "Minimum distance from any source to listener, in meters", toolkit.GetMinimumDistanceToListener());

            Common3DTIGUI.BeginSubsection("Speakers Fine Adjustment");
            {
                Common3DTIGUI.SingleSpace();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        for (int i = 0; i < toolkitAPI.GetNumberOfSpeakers(); i++)                        
                            ShowSpeakerPosition("Spk" + i.ToString(), toolkitAPI.GetSpeakerPosition(i));                        
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    {
                        for (int i = 0; i < toolkitAPI.GetNumberOfSpeakers(); i++)
                            GUILayout.Label("Offset (cm):");
                    }
                    GUILayout.EndVertical();                    
                    GUILayout.BeginVertical();
                    {
                        for (int i = 0; i < toolkitAPI.GetNumberOfSpeakers(); i++)
                        {
                            Vector3 speakerOffset = toolkitAPI.speakerOffsets[i];
                            if (ShowSpeakerOffsetControl(ref speakerOffset, toolkitAPI.speakerPositions[i]))
                                toolkitAPI.SetSpeakerOffset(i, speakerOffset);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            Common3DTIGUI.EndSubsection();
        }
        Common3DTIGUI.EndSection();
    }
    
    /// <summary>
    /// Draw panel with advanced configuration
    /// </summary>
    public void DrawAdvancedPanel()
    {
        Common3DTIGUI.BeginSection("ADVANCED SETUP");
      
        advancedSetup = Common3DTIGUI.CreateFoldoutToggle(ref advancedSetup, "Advanced Setup");
        if (advancedSetup)
        {
            // Scale factor slider
            Common3DTIGUI.AddLabelToParameterGroup("Scale factor");
            Common3DTIGUI.SingleSpace();
            Common3DTIGUI.CreateFloatSlider(ref toolkitAPI.scaleFactor, "Scale factor", "F2", " meters = 1.0 unit in Unity", "Set the proportion between meters and Unity scale units", minScale, maxScale, SliderScale);

            // Mod enabler            
            Common3DTIGUI.BeginSubsection("Switch Spatialization Effects");
            Common3DTIGUI.AddLabelToParameterGroup("Far distance LPF");
            Common3DTIGUI.AddLabelToParameterGroup("Distance attenuation");
            if (Common3DTIGUI.CreateToggle(ref toolkitAPI.modFarLPF, "Far distance LPF", "Enable low pass filter to simulate sound coming from far distances"))
                toolkitAPI.SetModFarLPF(toolkitAPI.modFarLPF);
            if (Common3DTIGUI.CreateToggle(ref toolkitAPI.modDistAtt, "Distance attenuation", "Enable attenuation of sound depending on distance to listener"))
                toolkitAPI.SetModDistanceAttenuation(toolkitAPI.modDistAtt);
            Common3DTIGUI.EndSubsection();

            // Magnitudes            
            Common3DTIGUI.BeginSubsection("Physical magnitudes");
            Common3DTIGUI.AddLabelToParameterGroup("Anechoic distance attenuation");
            Common3DTIGUI.AddLabelToParameterGroup("Sound speed");
            Common3DTIGUI.CreateFloatSlider(ref toolkitAPI.magAnechoicAttenuation, "Anechoic distance attenuation", "F2", "dB", "Set attenuation in decibels for each double distance", minDB, maxDB, SliderAnechoicAttenuation);
            Common3DTIGUI.CreateFloatSlider(ref toolkitAPI.magSoundSpeed, "Sound speed", "F0", "m/s", "Set sound speed, used for ITD computation", 0.0f, maxSoundSpeed, SliderSoundSpeed);
            Common3DTIGUI.EndSubsection();

            // Debug Log
            Common3DTIGUI.BeginSubsection("Debug log");
            Common3DTIGUI.AddLabelToParameterGroup("Write debug log file");
            if (Common3DTIGUI.CreateToggle(ref toolkitAPI.debugLog, "Write debug log file", "Enable writing of 3DTi_BinauralSpatializer_DebugLog.txt file in your project root folder; This file can be sent to the 3DTi Toolkit developers for support"))
                toolkitAPI.SendWriteDebugLog(toolkitAPI.debugLog);
            Common3DTIGUI.EndSubsection();
        }

        Common3DTIGUI.EndSection();
    }


    void ShowSpeakerPosition(string label, Vector3 speakerPosition)
    {
        //GUILayout.Label(label +" " + (speakerPosition + speakerOffset * 0.01f).ToString("f2") + " m");
        GUILayout.Label(label + " " + (speakerPosition).ToString("f2") + " m");
    }

    bool ShowSpeakerOffsetControl(ref Vector3 speakerOffset, Vector3 speakerPosition)
    {
        Vector3 previousSpeakerOffset = speakerOffset;
        Vector3 temp = EditorGUILayout.Vector3Field("", speakerOffset);
        speakerOffset = CalculateSpeakerOffsetBounded(temp, speakerPosition);
        return (previousSpeakerOffset != speakerOffset);            
    }


    Vector3 CalculateSpeakerOffsetBounded(Vector3 speakerOffset, Vector3 speakerPosition )
    {
        //Calculation of the maximum and minimum dimensions of the three coordinates

        float percentageOfChange = 10f;        //We only allow a change of +-10%        

        float minimum = toolkitAPI.GetStructureSide() * percentageOfChange *-1.0f;
        float maximum = toolkitAPI.GetStructureSide() * percentageOfChange;

        ////X
        //float x_minimum;
        //float x_maximum;
        //if (speakerPosition.x != 0.0f)
        //{
        //    x_minimum = Mathf.Abs(speakerPosition.x) * -percentageOfChange * 100.0f;
        //    x_maximum = Mathf.Abs(speakerPosition.x) * percentageOfChange * 100.0f;
        //}
        //else
        //{
        //    x_minimum = -percentageOfChange * 100.0f;
        //    x_maximum = percentageOfChange * 100.0f;
        //}
        ////Y
        //float y_minimum;
        //float y_maximum;
        //if (speakerPosition.y != 0.0f)
        //{
        //    y_minimum = Mathf.Abs(speakerPosition.y) * -percentageOfChange * 100.0f;
        //    y_maximum = Mathf.Abs(speakerPosition.y) * percentageOfChange * 100.0f;
        //}
        //else
        //{
        //    y_minimum = -percentageOfChange * 100.0f;
        //    y_maximum = percentageOfChange * 100.0f;
        //}
        ////Z
        //float z_minimum;
        //float z_maximum;
        //if (speakerPosition.z != 0.0f)
        //{
        //    z_minimum = Mathf.Abs(speakerPosition.z) * -percentageOfChange * 100.0f;
        //    z_maximum = Mathf.Abs(speakerPosition.z) * percentageOfChange * 100.0f;
        //}
        //else
        //{
        //    z_minimum = -percentageOfChange * 100.0f;
        //    z_maximum = percentageOfChange * 100.0f;
        //}


        //Calculate new vector
        return new Vector3(Mathf.Clamp(speakerOffset.x, minimum, maximum), Mathf.Clamp(speakerOffset.y, minimum, maximum), Mathf.Clamp(speakerOffset.z, minimum, maximum));       
    }
}