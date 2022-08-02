using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using API_3DTI;
using UnityEngine.UI;

public class TestFunctions : MonoBehaviour
{
    public HearingAid HearingAid;
    public HearingLoss HearingLoss;
    public Spatializer Spatializer;
    public SphereSpawner SphereSpawner;

    public Toggle SpatializeToggle;
    public Toggle HearingLossToggle;
    public Toggle HearingAidToggle;
    public Toggle ReverbToggle;

    public int SoakTestMaxNumSources = 50;
    // determine whether new spheres should have spatialization enabled
    public bool isSpatializeEnabled = true;

    private void Start()
    {
        SpatializeToggle.SetIsOnWithoutNotify(isSpatializeEnabled);
        // HA still uses the old API
        if (HearingAid.haMixer.GetFloat("HA3DTI_Process_LeftOn", out float isLeftOnF) && HearingAid.haMixer.GetFloat("HA3DTI_Process_LeftOn", out float isRightOnF))
        {
            HearingAidToggle.SetIsOnWithoutNotify(isLeftOnF != 0.0f || isRightOnF != 0.0f);
        }
        // HL and Spatializer use the new API
        HearingLossToggle.SetIsOnWithoutNotify(HearingLoss.GetParameter<bool>(HearingLoss.Parameter.HLOn, T_ear.LEFT) || HearingLoss.GetParameter<bool>(HearingLoss.Parameter.HLOn, T_ear.RIGHT));
        ReverbToggle.SetIsOnWithoutNotify(Spatializer.GetParameter<bool>(Spatializer.Parameter.EnableReverbProcessing));

    }

    // Start is called before the first frame update
    void OnEnable()
    {
    }

    void OnDisable()
    {
    }


    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator SoakTestUpdate()
    {
        while (true)
        {
            if (isActiveAndEnabled)
            {
                int targetNumSources = Random.Range(0, SoakTestMaxNumSources);
                if (targetNumSources > SphereSpawner.Count)
                {
                    SphereSpawner.Spawn();
                }
                else if (targetNumSources < SphereSpawner.Count)
                {
                    SphereSpawner.RemoveSource();
                }
            }

            yield return new WaitForSeconds(Random.Range(1.0f, 3.0f));
        }
    }

    public void EnableSpatialize(bool isEnabled)
    {
        isSpatializeEnabled = isEnabled;
        SphereSpawner.SpawnedSphereEmitters.ForEach(sphere => sphere.GetComponent<AudioSource>().spatialize = isSpatializeEnabled);
    }

    public void EnableHearingLossInBothEars(bool isEnabled)
    {
        HearingLoss.SetParameter(HearingLoss.Parameter.HLOn, isEnabled, T_ear.BOTH);
    }
    public void EnableHAInBothEars(bool isEnabled)
    {
        HearingAid.SetParameter(HearingAid.Parameter.ProcessOn, isEnabled, T_ear.BOTH);
    }

    public void EnableReverb(bool isEnabled)
    {
        if (Spatializer != null)
        {
            Spatializer.SetParameter(Spatializer.Parameter.EnableReverbProcessing, isEnabled);
        }
    }

    public void EnableSoakTest(bool isEnabled)
    {
        if (isEnabled)
        {
            StartCoroutine("SoakTestUpdate");
        }
        else
        {
            StopCoroutine("SoakTestUpdate");
        }
    }
}
