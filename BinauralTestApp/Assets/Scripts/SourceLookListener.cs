using UnityEngine;
using System.Collections;

public class SourceLookListener : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.LookAt(GameObject.Find("Listener").transform);
        transform.RotateAround(transform.position, new Vector3(0.0f, 1.0f, 0.0f), 180.0f);
	}
}
