using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using System.Collections.Generic;


public class TestHAAPI : MonoBehaviour {

    API_3DTI_HA HAAPI;
    AudioMixer HAMIXER;
    //float volume = 0.0f;    
    //float volIncrement = 0.1f;
    //float cutoff = 0.0f;
    //float cutIncrement = 5.0f;

    // Use this for initialization
    void Start () {
        // Find main elements (API and mixer)
        HAAPI = Camera.main.GetComponent<API_3DTI_HA>();
        //HAMIXER = HAAPI.haMixer;

        // Test Fig6
        List<float> earLossList = new List<float>();
        List<float> gains;
        earLossList.Add(0.0f);
        earLossList.Add(30.0f);
        earLossList.Add(0.0f);
        earLossList.Add(0.0f);
        earLossList.Add(0.0f);
        earLossList.Add(0.0f);
        earLossList.Add(0.0f);
        HAAPI.SetEQFromFig6(API_3DTI_HA.EAR_BOTH, earLossList, out gains);

        int band = 0;
        int level = 0;
        foreach (float gain in gains)
        {
            Debug.Log("Band " + band.ToString() + ", level " + level.ToString() + " gain = " + gain.ToString());
            level++;
            if (level == API_3DTI_HA.NUM_LEVELS)
            {
                level = 0;
                band++;
            }
        }
    }

    // Update is called once per frame
    void Update () {
        //if (volume <= -30.0f)
        //    volume = 0.0f;        
        //HAAPI.SetVolume(API_3DTI_HA.EAR_BOTH, volume);
        //volume -= volIncrement;

        //if (cutoff >= 3000.0f)
        //    cutoff = 0.0f;
        //HAAPI.SetLPFCutoff(cutoff);
        //cutoff += cutIncrement;
    }
}
