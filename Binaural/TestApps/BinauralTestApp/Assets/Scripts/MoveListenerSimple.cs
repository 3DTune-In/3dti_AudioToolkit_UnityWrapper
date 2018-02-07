using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MoveListenerSimple : MonoBehaviour {
    
    private Vector3 AZIMUTH_AXIS = new Vector3(0.0f, -1.0f, 0.0f);
    private float INPUT_AZIMUTH_SCALE = 1.0f;
    private float INPUT_ELEVATION_SCALE = 1.0f;        

    // Use this for initialization
    void Start ()
    {
        Screen.orientation = ScreenOrientation.Landscape;
    }
	
	// Update is called once per frame
	void Update ()
    {
        RotateListener();
	}

    /// <summary>
    /// Rotate listener head with touch 
    /// </summary>
    public void RotateListener()
    {        
        // Move azimuth/elevation
        if (Input.touchCount > 0) 
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        Vector2 touchDeltaPosition = touch.deltaPosition;
                        transform.RotateAround(transform.position, AZIMUTH_AXIS, touchDeltaPosition.x * INPUT_AZIMUTH_SCALE);
                    }
                }
            }
        }
    }

    /// <summary>
    /// For debug. remove before release
    /// </summary>        
    public void DebugWrite(string text)
    {
        GameObject textGO = GameObject.Find("DebugText");
        if (textGO == null)
            Debug.Log(text);
        else
        {
            TextMesh debugText = textGO.GetComponent<TextMesh>();
            debugText.text = debugText.text + "\n" + text;
        }
    }
}
