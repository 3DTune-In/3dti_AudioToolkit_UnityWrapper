using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestSpatializer : MonoBehaviour
{
    API_3DTI_Spatializer SpatializerAPI;    
    AudioSource testSource;
    Text debugText;

    bool toggleSource = true;
    const int COUNTTIME = 3;
    int countdown = COUNTTIME;
    float lastTime;
    float bufSize, samplingFreq;
    float bufSizeC, samplingFreqC;
    public AudioClip otherClip;

    void Start()
    {
        SpatializerAPI = Camera.main.GetComponent<API_3DTI_Spatializer>();        
        debugText = GameObject.FindGameObjectWithTag("DebugText").GetComponent<Text>();
        testSource = GameObject.Find("TestAudio Source Left").GetComponent<AudioSource>();
        lastTime = Time.time;
    }

    void Update()
    {
        //if(SpatializerAPI.GetBufferSize(out bufSize) && SpatializerAPI.GetSampleRate(out samplingFreq))
        SpatializerAPI.GetBufferSize(out bufSize);
        SpatializerAPI.GetSampleRate(out samplingFreq);
        SpatializerAPI.GetBufferSizeCore(out bufSizeC);
        SpatializerAPI.GetSampleRateCore(out samplingFreqC);


        
        //{
            debugText.text = "UNITY->BS:" + bufSize.ToString() + ";SR:" + samplingFreq.ToString() + 
            "CORE->BS:" + bufSizeC.ToString() + ";SR:" + samplingFreqC.ToString();



        //}

        //if (SpatializerAPI.GetBufferSizeCore(out bufSizeC) && SpatializerAPI.GetSampleRateCore(out samplingFreqC))
        //}
        //debugText.text = "Countdown: " + countdown + " seconds";
        //if ((Time.time - lastTime) > 1.0f)
        //{            
        //    lastTime = Time.time;
        //    countdown--;
        //    if (countdown <= 0)
        //    {
        //        if (!toggleSource)
        //        {
        //            toggleSource = true;
        //            testSource.Stop();
        //            countdown = COUNTTIME;
        //        }
        //        else
        //        {
        //            toggleSource = false;
        //            testSource.Play();
        //            //SpatializerAPI.StartBinauralSpatializer(testSource);
        //            countdown = COUNTTIME;
        //        }
        //    }
        //}

        //if (Input.GetKeyDown("space"))
        //{
        //    Debug.Log("Space pressed");
        //    toggleSource = !toggleSource;
        //    if (toggleSource)
        //    {
        //        //testSource.spatialize = true;
        //        //testSource.gameObject.SetActive(true);
        //        testSource.Play();
        //        SpatializerAPI.StartBinauralSpatializer(testSource);
        //    }
        //    else
        //    {
        //        testSource.Stop();
        //        //testSource.gameObject.SetActive(false);
        //        //testSource.spatialize = false;
        //    }
        //}

        if (Input.GetKeyDown("space"))
        {            
            toggleSource = !toggleSource;
            if (toggleSource)
            {
                //Debug.Log("Enabling spatialization!");
                //SpatializerAPI.EnableSpatialization(testSource);
                Debug.Log("Enabling distance attenuation");
                SpatializerAPI.SetModDistanceAttenuation(true, testSource);
            }
            else
            {
                //Debug.Log("Disabling spatialization!");
                //SpatializerAPI.DisableSpatialization(testSource);
                Debug.Log("Disabling distance attenuation");
                SpatializerAPI.SetModDistanceAttenuation(false, testSource);
            }
        }        

        //if (Input.GetKeyDown("l"))
        //{
        //    testSource.clip = otherClip;
        //    testSource.Play();
        //}
    }
}

