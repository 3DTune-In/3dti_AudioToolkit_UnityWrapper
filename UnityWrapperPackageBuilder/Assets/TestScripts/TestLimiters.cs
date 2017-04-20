using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestLimiters : MonoBehaviour {

    Text debugText;
    API_3DTI_Spatializer spatializer;
    API_3DTI_HA ha;    

	// Use this for initialization
	void Start ()
    {        
        debugText = GameObject.FindGameObjectWithTag("DebugText").GetComponent<Text>();   
        spatializer = Camera.main.GetComponent<API_3DTI_Spatializer>();
        ha = Camera.main.GetComponent<API_3DTI_HA>();
        ha.SwitchLimiterOnOff(true);

		//ha.SetNormalizationLevel(API_3DTI_Common.T_ear.BOTH, 10.0f);
		//ha.SwitchNormalizationOnOff(API_3DTI_Common.T_ear.BOTH, true);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.L))
            ha.SwitchLimiterOnOff(true);
        if (Input.GetKeyDown(KeyCode.O))
            ha.SwitchLimiterOnOff(false);

        if (Input.GetKeyDown(KeyCode.N))
            ha.SwitchNormalizationOnOff(API_3DTI_Common.T_ear.BOTH, true);
        if (Input.GetKeyDown(KeyCode.H))
            ha.SwitchNormalizationOnOff(API_3DTI_Common.T_ear.BOTH, false);

        //bool compSpat, compHA;
        //if (!spatializer.GetLimiterCompression(out compSpat))
        //    debugText.text = "ERROR Reading parameter from spatializer plugin";
        //else
        //{
        //    if (!ha.GetLimiterCompression(out compHA))
        //        debugText.text = "ERROR Reading parameter from HA plugin";
        //    else
        //    {
        //        debugText.text = "COMPRESSING: ";
        //        if (compSpat)
        //            debugText.text = debugText.text + "Spatializer ";
        //        if (compHA)
        //            debugText.text = debugText.text + "HA";
        //    }
        //}

        //float offsetL;
        //if (!ha.GetNormalizationOffset(API_3DTI_Common.T_ear.LEFT, out offsetL))
        //	debugText.text = "Error reading normalization offset";
        //else
        //	debugText.text = "Left Normalization offset is " + offsetL;
    }
}
