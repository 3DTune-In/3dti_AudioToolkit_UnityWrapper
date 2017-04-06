using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoudSpeakerSpatializer : MonoBehaviour {

    API_3DTI_LoudSpeakersSpatializer SpatializerAPI;
    bool toggleSource = false;
    AudioSource testSource;

    void Start()
    {
        SpatializerAPI = Camera.main.GetComponent<API_3DTI_LoudSpeakersSpatializer>();
        //SpatializerAPI.SetScaleFactor(1.0f);

        testSource = GameObject.Find("TestAudio Source Left").GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Debug.Log("Space pressed Loudspeakers");
            toggleSource = !toggleSource;
            if (toggleSource)
            {
                //testSource.spatialize = true;
                //testSource.gameObject.SetActive(true);
                testSource.Play();
                SpatializerAPI.StartLoadSpeakersSpatializer(testSource);
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
