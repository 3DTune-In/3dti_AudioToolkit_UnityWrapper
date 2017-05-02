using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestSpatializer : MonoBehaviour
{
    API_3DTI_Spatializer SpatializerAPI;
    bool toggleSource = false;
    AudioSource testSource;
    Text debugText;
    const int COUNTTIME = 3;
    int countdown = COUNTTIME;
    float lastTime;  

    void Start()
    {
        SpatializerAPI = Camera.main.GetComponent<API_3DTI_Spatializer>();
        //SpatializerAPI.SetScaleFactor(1.0f);
        debugText = GameObject.FindGameObjectWithTag("DebugText").GetComponent<Text>();
        testSource = GameObject.Find("TestAudio Source Left").GetComponent<AudioSource>();
        lastTime = Time.time;
    }

    void Update()
    {
        debugText.text = "Countdown: " + countdown + " seconds";
        if ((Time.time - lastTime) > 1.0f)
        {            
            lastTime = Time.time;
            countdown--;
            if (countdown <= 0)
            {
                if (!toggleSource)
                {
                    toggleSource = true;
                    testSource.Stop();
                    countdown = COUNTTIME;
                }
                else
                {
                    toggleSource = false;
                    testSource.Play();
                    SpatializerAPI.StartBinauralSpatializer(testSource);
                    countdown = COUNTTIME;
                }
            }
        }

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
    }
}

