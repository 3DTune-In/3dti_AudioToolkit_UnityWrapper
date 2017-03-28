using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using System.Collections.Generic;


public class TestHAAPI : MonoBehaviour {

    API_3DTI_HA HAAPI;
    AudioMixer HAMIXER;

    //// LPF Test:
    //float cutoff = 0.0f;
    //float cutIncrement = 5.0f;

    // Tone Test:
    float tone = 0.0f;
    bool adding = true;
    float toneIncrement = 1.0f;

    // Use this for initialization
    void Start ()
    {
        // Find main elements (API and mixer)
        HAAPI = Camera.main.GetComponent<API_3DTI_HA>();
        //HAMIXER = HAAPI.haMixer;

        HAAPI.SwitchHAOnOff(API_3DTI_Common.T_ear.BOTH, true);

        RunFig6Test();
    }

    // Update is called once per frame
    void Update ()
    {
        //// LPF Test:
        //if (cutoff >= 3000.0f)
        //    cutoff = 0.0f;
        //HAAPI.SetLPFCutoff(cutoff);
        //cutoff += cutIncrement;

        // Tone Test:
        if (adding)
        {
            HAAPI.SetTone(API_3DTI_Common.T_ear.BOTH, API_3DTI_HA.T_toneBand.MID, tone);
            tone += toneIncrement;
            if (tone >= 30.0f)
                adding = false;            
        }
        if (!adding)
        {
            HAAPI.SetTone(API_3DTI_Common.T_ear.BOTH, API_3DTI_HA.T_toneBand.MID, tone);
            tone -= toneIncrement;
            if (tone <= -30.0f)
                adding = true;            
        }
    }

    
    ////////////////////////////////////////    

    public void RunFig6Test()
    {
        List<float> earLossList = new List<float>();
        List<float> gains;
        earLossList.Add(0.0f);
        earLossList.Add(30.0f);
        earLossList.Add(0.0f);
        earLossList.Add(0.0f);
        earLossList.Add(0.0f);
        earLossList.Add(0.0f);
        earLossList.Add(0.0f);
        HAAPI.SetEQFromFig6(API_3DTI_Common.T_ear.BOTH, earLossList, out gains);

        int band = 0;
        int curve = 0;
        foreach (float gain in gains)
        {
            Debug.Log("Band " + band.ToString() + ", curve " + curve.ToString() + " gain = " + gain.ToString());
            curve++;
            if (curve == API_3DTI_HA.NUM_EQ_CURVES)
            {
                curve = 0;
                band++;
            }
        }
    }
}
