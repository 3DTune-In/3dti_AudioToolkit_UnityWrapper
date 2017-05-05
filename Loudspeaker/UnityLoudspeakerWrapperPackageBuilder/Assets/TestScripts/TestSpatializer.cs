using UnityEngine;
using System.Collections;

public class TestSpatializer : MonoBehaviour
{
    API_3DTI_Spatializer SpatializerAPI;
    bool toggleSource = false;
    AudioSource testSource;

    void Start()
    {
        SpatializerAPI = Camera.main.GetComponent<API_3DTI_Spatializer>();
        //SpatializerAPI.SetScaleFactor(1.0f);

        testSource = GameObject.Find("TestAudio Source Left").GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Debug.Log("Space pressed");
            toggleSource = !toggleSource;
            if (toggleSource)
            {
                //testSource.spatialize = true;
                //testSource.gameObject.SetActive(true);
                testSource.Play();
                SpatializerAPI.StartBinauralSpatializer(testSource);
            }
            else
            {
                testSource.Stop();
                //testSource.gameObject.SetActive(false);
                //testSource.spatialize = false;
            }
        }
    }
}

