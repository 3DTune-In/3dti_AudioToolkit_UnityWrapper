using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MoveListener : MonoBehaviour {
    
    private Vector3 AZIMUTH_AXIS = new Vector3(0.0f, -1.0f, 0.0f);
    private float INPUT_AZIMUTH_SCALE = 1.0f;
    private float INPUT_ELEVATION_SCALE = 1.0f;
    private float SLOW_DOWN_SPEED = 0.02f;
    private float walkSpeed = 0.0f;
    private bool slowingDown = false;
    private Slider walkSlider;

    // Use this for initialization
    void Start ()
    {
        walkSlider = GameObject.Find("Slider_Walk").GetComponent<Slider>();

        // Force screen orientation. This shouldnt go here...
        #if (UNITY_ANDROID)
        Screen.orientation = ScreenOrientation.Landscape;
        #endif
    }
	
	// Update is called once per frame
	void Update ()
    {
        RotateListener();
        WalkListener();
	}

    /// <summary>
    /// Rotate listener head with touch 
    /// </summary>
    public void RotateListener()
    {
#if (UNITY_ANDROID)
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
                        //transform.RotateAround(transform.position, transform.right, touchDeltaPosition.y * -INPUT_ELEVATION_SCALE);
                    }
                }
            }
        }
#else
        if (Input.GetMouseButton(0))
        {
            transform.RotateAround(transform.position, AZIMUTH_AXIS, Input.GetAxis("Mouse X") * INPUT_AZIMUTH_SCALE);
        }
#endif
    }

    /// <summary>
    /// Walk depending on walk speed (controlled by slider)
    /// </summary>
    public void WalkListener()
    {
        if (!Mathf.Approximately(walkSpeed, 0.0f))
        {
            //transform.Translate(transform.forward * Time.deltaTime * walkSpeed);
            // Project forward vector to floor
            Vector3 walkDirection = transform.forward;
            walkDirection.y = 0.0f;
            walkDirection.Normalize();
            transform.Translate(walkDirection * Time.deltaTime * walkSpeed, Space.World);
            //Vector3 position = transform.position;
            //position.y = 0.0f;
            //transform.position = position;
            //DebugWrite("Forward = " + transform.forward.ToString());

            if (slowingDown)
            {
                walkSpeed -= SLOW_DOWN_SPEED;
                if (walkSpeed < 0.0f)
                    walkSpeed = 0.0f;
                walkSlider.value = walkSpeed;               
            }
        }
    }

    /// <summary>
    /// Event executed when Walk slider value has changed
    /// </summary>
    public float SliderWalk
    {
        set
        {            
            walkSpeed = value;            
        }
    }

    /// <summary>
    /// Event executed when the Walk slider is released from drag
    /// </summary>
    public void SliderWalkReleased()
    {
        slowingDown = true;
    }

    /// <summary>
    /// Event executed when drag of the Walk slider starts
    /// </summary>
    public void SiderWalkPressed()
    {
        slowingDown = false;
    }

    /// <summary>
    /// Move listener to home position
    /// </summary>
    public void GoHome()
    {
        transform.position = Vector3.zero;
        transform.LookAt(new Vector3(0.0f, 0.0f, 1.0f));
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
