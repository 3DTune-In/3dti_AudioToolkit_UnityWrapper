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
    //float tone = 0.0f;
    //bool adding = true;
    //float toneIncrement = 1.0f;

    // Compression Test:
    //float compression = 0.0f;
    //float compressionIncrement = 0.1f;

    // Use this for initialization
    void Start ()
    {
        // Find main elements (API and mixer)
		HAAPI = Camera.main.GetComponent<API_3DTI_HA>();
        HAAPI.SwitchHAOnOff(API_3DTI_Common.T_ear.BOTH, true);
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
        //if (adding)
        //{
        //    HAAPI.SetTone(API_3DTI_Common.T_ear.BOTH, API_3DTI_HA.T_toneBand.MID, tone);
        //    tone += toneIncrement;
        //    if (tone >= 30.0f)
        //        adding = false;            
        //}
        //if (!adding)
        //{
        //    HAAPI.SetTone(API_3DTI_Common.T_ear.BOTH, API_3DTI_HA.T_toneBand.MID, tone);
        //    tone -= toneIncrement;
        //    if (tone <= -30.0f)
        //        adding = true;            
        //}

        // Compression Test:
        //if (compression >= 120.0f)
        //    compression = 0.0f;
        //HAAPI.SetCompressionPercentage(API_3DTI_Common.T_ear.BOTH, compression);
        //compression += compressionIncrement;

		HAAPI.SwitchHAOnOff(API_3DTI_Common.T_ear.BOTH, true);

        // Test Fig6 and Compression
        if (Input.GetKeyDown(KeyCode.F))
            RunFig6TestMild();
        if (Input.GetKeyDown(KeyCode.O))
            HAAPI.SetCompressionPercentage(API_3DTI_Common.T_ear.BOTH, 0.0f);
        if (Input.GetKeyDown(KeyCode.P))
            HAAPI.SetCompressionPercentage(API_3DTI_Common.T_ear.BOTH, 120.0f);
        if (Input.GetKeyDown(KeyCode.K))
            HAAPI.SetDynamicEQAttackRelease(API_3DTI_Common.T_ear.BOTH, 100.0f);
        if (Input.GetKeyDown(KeyCode.L))
            HAAPI.SetWriteDebugLog(true);
		if (Input.GetKeyDown(KeyCode.V))
			HAAPI.SetVolume(API_3DTI_Common.T_ear.BOTH, 20.0f);
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

    public void RunFig6TestSevere()
    {
        List<float> earLossList = new List<float>();
        List<float> gains;        
        earLossList.Add(47.0f);
        earLossList.Add(52.0f);
        earLossList.Add(55.0f);
        earLossList.Add(62.0f);
        earLossList.Add(65.0f);
        earLossList.Add(65.0f);
        earLossList.Add(65.0f);
        HAAPI.SetEQFromFig6(API_3DTI_Common.T_ear.BOTH, earLossList, out gains);
    }

	public void RunFig6TestMild()
	{
		List<float> earLossList = new List<float>();
		List<float> gains;
		earLossList.Add(7.0f);
		earLossList.Add(12.0f);
		earLossList.Add(15.0f);
		earLossList.Add(22.0f);
		earLossList.Add(25.0f);
		earLossList.Add(25.0f);
		earLossList.Add(25.0f);
		HAAPI.SetEQFromFig6(API_3DTI_Common.T_ear.BOTH, earLossList, out gains);
	}
}
