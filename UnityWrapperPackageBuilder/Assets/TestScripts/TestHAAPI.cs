using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class TestHAAPI : MonoBehaviour {

    API_3DTI_HA HAAPI;
    AudioMixer HAMIXER;
    float volume = 0.0f;    
    float volIncrement = 0.1f;
    float cutoff = 0.0f;
    float cutIncrement = 1.0f;

    // Use this for initialization
    void Start () {
        // Find main elements (API and mixer)
        HAAPI = Camera.main.GetComponent<API_3DTI_HA>();
        HAMIXER = HAAPI.haMixer;

        // Switch on hearing aid for both ears
        HAAPI.SwitchHAOnOff(API_3DTI_HA.EAR_BOTH, true);

        // Add 12 bit quantization only at the input signal
        HAAPI.SetQuantizationNoiseInChain(true, false);
        HAAPI.SetQuantizationNoiseBits(12);
    }
	
	// Update is called once per frame
	void Update () {
        //if (volume <= -30.0f)
        //    volume = 0.0f;        
        //HAAPI.SetVolume(API_3DTI_HA.EAR_BOTH, volume);
        //volume -= volIncrement;

        if (cutoff >= 3000.0f)
            cutoff = 0.0f;
        HAAPI.SetLPFCutoff(cutoff);
        cutoff += cutIncrement;
    }
}
