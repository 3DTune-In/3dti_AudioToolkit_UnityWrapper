using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using System.Collections.Generic;
using API_3DTI_Common;

public class TestHLAPI : MonoBehaviour
{
    API_3DTI_HL HLAPI;

    // Use this for initialization
    void Start()
    {
        // Find API
        HLAPI = Camera.main.GetComponent<API_3DTI_HL>();

        // Enable hearing loss in both ears
        if (HLAPI.EnableHearingLoss(T_ear.BOTH))
            Debug.Log("Hearing loss enabled");
        else
            Debug.Log("ERROR!!! Could not enable Hearing Loss");

        // Disable temporal distortion and frequency smearing in both ears   
        if (HLAPI.DisableTemporalDistortionSimulation(T_ear.BOTH))
            Debug.Log("Temporal distortion disabled");
        else
            Debug.Log("ERROR!!! Could not disable temporal distortion");
        if (HLAPI.DisableFrequencySmearingSimulation(T_ear.BOTH))
            Debug.Log("Frequency smearing disabled");
        else
            Debug.Log("ERROR!!! Could not disable frequency smearing");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            if (HLAPI.SetAudiometryFromClassificationScale(T_ear.BOTH, API_3DTI_HL.T_HLClassificationScaleCurve.HL_CS_K, 1, API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MILD))
                Debug.Log("Audiometry set from curve K with slope 3 and severity " + HLAPI.FromClassificationScaleSeverityToString(API_3DTI_HL.T_HLClassificationScaleSeverity.HL_CS_SEVERITY_MILD));
            else
                Debug.Log("ERROR!!!! Could not set HL from classification scale");
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (HLAPI.EnableTemporalDistortionSimulation(T_ear.BOTH))
                Debug.Log("Temporal distortion simulation enabled");
            else
                Debug.Log("ERROR!!!! Could not enable temporal distortion simulation");
            HLAPI.SetTemporalDistortionWhiteNoisePower(T_ear.BOTH, 1.0f);
            HLAPI.SetTemporalDistortionBandwidth(T_ear.BOTH, 500.0f);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (HLAPI.EnableFrequencySmearingSimulation(T_ear.BOTH))
                Debug.Log("Frequency smearing simulation enabled");
            else
                Debug.Log("ERROR!!!! Could not enable frequency smearing simulation");
            HLAPI.SetFrequencySmearingDownwardBufferSize(T_ear.BOTH, 20);
            HLAPI.SetFrequencySmearingUpwardBufferSize(T_ear.BOTH, 20);
            HLAPI.SetFrequencySmearingDownwardAmount_Hz(T_ear.BOTH, 200.0f); 
            HLAPI.SetFrequencySmearingUpwardAmount_Hz(T_ear.BOTH, 200.0f);
        }
    }
}
    

