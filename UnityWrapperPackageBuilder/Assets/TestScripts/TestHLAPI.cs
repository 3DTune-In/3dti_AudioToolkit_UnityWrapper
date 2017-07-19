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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            if (HLAPI.SetAudiometryPreset(T_ear.BOTH, API_3DTI_HL.AUDIOMETRY_PRESET_MODERATE))
                Debug.Log("Moderate HL Preset set");
            else
                Debug.Log("ERROR!!!! Could not set Moderate HL Preset");
        }
    }
}
    

