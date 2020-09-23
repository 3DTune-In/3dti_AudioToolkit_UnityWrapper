using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereSpawner : MonoBehaviour
{
    public GameObject spherePivot;
    public GameObject sphereEmitterPrefab;
    public bool spawnOnAwake;
    public bool spawnOnStart;
    public bool spawnAfterSpatializerBinaryLoaded;

    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    readonly int NumFrameDeltasToPrintOnSpawn = 3;
    private int framesSinceSpawn = -1;

    private List<GameObject> spawnedSphereEmitters = new List<GameObject>();
    private API_3DTI_Spatializer spatializer;

    public void Awake()
    {
        if (spawnOnAwake)
        {
            spawn();
        }
    }

    public void Start()
    {
        if (spawnOnStart)
        {
            spawn();
        }

        stopwatch.Start();

        spatializer = Camera.main.GetComponent<API_3DTI_Spatializer>();
        //// Set High Quality spatialization mode
        //spatializer.SetSpatializationMode(API_3DTI_Spatializer.SPATIALIZATION_MODE_HIGH_QUALITY);
        //// Load HRTF
        //spatializer.LoadHRTFBinary("Assets/myData/myHRTF.3dti-hrtf");
        //// Disable ILD, so that we don't need to load ILD resources
        //spatializer.SetModNearFieldILD(false);
        //// Enable hearing aid directionality for both ears
        //// and set it to a moderate extend
        //spatializer.SwitchOnOffHADirectionality(API_3DTI_Common.T_ear.BOTH, true);
        //spatializer.SetHADirectionalityExtend(API_3DTI_Common.T_ear.BOTH, 15.0f);

        if (spawnAfterSpatializerBinaryLoaded)
        {
            spawn();
        }
    }

    private void spawn()
    {
        Debug.Log("Spawning new Sphere Emitter");
        GameObject sphereEmitter = Instantiate(sphereEmitterPrefab, new Vector3(4, 0, 0), Quaternion.identity);
        sphereEmitter.transform.parent = spherePivot.transform;
        //var spatializer = FindObjectOfType<API_3DTI_Spatializer>();
        var audioSource = sphereEmitter.GetComponent<AudioSource>();
        audioSource.Play();
        //bool success = spatializer.StartBinauralSpatializer(audioSource);
        //if (!success)
        //{
        //    Debug.LogWarning("Failed to start binaural spatializer on the new sphere", this);
        //}
        //FindObjectOfType<API_3DTI_Spatializer>().StartBinauralSpatializer(sphereEmitter.GetComponent<AudioSource>());


        framesSinceSpawn = 0;
        spawnedSphereEmitters.Add(sphereEmitter);
        Debug.Log($"{spawnedSphereEmitters.Count} spheres active.");
    }

    public void Update()
    {
        bool hasTouch = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        if (Input.GetMouseButtonDown(0) || hasTouch)
        {
            spawn();
        }
        else if (Input.GetMouseButtonDown(1) && spawnedSphereEmitters.Count > 0)
        {
            Debug.Log("Removing oldest Sphere Emitter");
            GameObject sphere = spawnedSphereEmitters[0];
            spawnedSphereEmitters.RemoveAt(0);
            Destroy(sphere);
        }

        if (framesSinceSpawn >= 0 && framesSinceSpawn < NumFrameDeltasToPrintOnSpawn)
        {
            //Debug.Log($"Frame delta: {stopwatch.ElapsedMilliseconds} ms");
            framesSinceSpawn++;
        }
        stopwatch.Restart();
    }



    // Start is called before the first frame update
    void OnMouseDown()
    {
        
    }
}
