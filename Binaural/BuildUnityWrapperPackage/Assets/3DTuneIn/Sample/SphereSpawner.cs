using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SphereSpawner : MonoBehaviour, IPointerClickHandler
{
    public GameObject spherePivot;
    public GameObject sphereEmitterPrefab;
    public bool spawnOnAwake;
    public bool spawnOnStart;

    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    private List<GameObject> spawnedSphereEmitters = new List<GameObject>();

    public void Awake()
    {
        if (spawnOnAwake)
        {
            Spawn();
        }
    }

    public void Start()
    {
        if (spawnOnStart)
        {
            Spawn();
        }

        stopwatch.Start();

    }

    public void Spawn()
    {
        Debug.Log("Spawning new source (Sphere)");
        GameObject sphereEmitter = Instantiate(sphereEmitterPrefab, new Vector3(4, 0, 0), Quaternion.identity);
        sphereEmitter.transform.parent = spherePivot.transform;
        var audioSource = sphereEmitter.GetComponent<AudioSource>();
        audioSource.Play();
        spawnedSphereEmitters.Add(sphereEmitter);
        Debug.Log($"{spawnedSphereEmitters.Count} sources active.");
    }

    public void TempTest()
    {
        var mixer = GameObject.FindObjectOfType<API_3DTI_HL>().hlMixer;
        int newValue = Random.Range(100, 1000);
        if (mixer.SetFloat("HL3DTI_Attack_Left", newValue))
        {
            Debug.Log($"Set HL3DTI_Attack_Left to {newValue}");
        }
        else
        {
            Debug.Log("Failed to set HL3DTI_Attack_Left via mixer (from button)");
        }
    }

    public void RemoveSource()
    {
        if (spawnedSphereEmitters.Count > 0)
        {
            Debug.Log("Removing oldest Source");
            GameObject sphere = spawnedSphereEmitters[0];
            spawnedSphereEmitters.RemoveAt(0);
            Destroy(sphere);
        }
        else
        {
            Debug.Log("Cannot remove source as none exist.");
        }
    }

    public void Update()
    {
    }



    // Start is called before the first frame update
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
    }
}
