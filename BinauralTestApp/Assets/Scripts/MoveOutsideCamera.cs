using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MoveOutsideCamera : MonoBehaviour {

    // Predefined points of view
    private Vector3 topView = new Vector3(0.0f, 2.0f, 0.0f);
    private Vector3 frontView = new Vector3(0.0f, 0.0f, 2.0f);
    private Vector3 leftView = new Vector3(-2.0f, 0.0f, 0.0f);
    private Vector3 rightView = new Vector3(2.0f, 0.0f, 0.0f);
    private Vector3 androidView = new Vector3(0.0f, 4.0f, 0.0f);

    // Mouse movement scale and limits
    private float MOUSE_AZIMUTH_SCALE = 10.0f;
    private float MOUSE_ELEVATION_SCALE = 10.0f;
    private float MOUSE_DISTANCE_SCALE = 0.1f;
    private float MOUSE_DISTANCE_MIN = 0.5f;
    private float MOUSE_DISTANCE_MAX = 10.0f;

    // Initial transform
    Vector3 defaultCameraPosition;
    Quaternion defaultCameraRotation;

    // References
    private Vector3 AZIMUTH_AXIS = new Vector3(0.0f, 1.0f, 0.0f);
    private Vector3 DOWN_DIRECTION = new Vector3(0.0f, -1.0f, 0.0f);
    private Vector3 listenerPosition = Vector3.zero;

    // Access to the camera
    private Camera outsideCamera;    

    // Access to the view text
    //private TextMesh textView;
    //private float TEXT_LEFT_MARGIN = 0.02f;
    //private float TEXT_DISTANCE = 20.0f;
    Text textView;
    //private int TEXT_LEFT_MARGIN = 0; // in pixels
    private int TEXT_TOP_MARGIN = 0; // in pixels

    // Use this for initialization
    void Start ()
    {
        outsideCamera = GameObject.FindGameObjectWithTag("OutsideCamera").GetComponent<Camera>();
        //textView = GameObject.Find("Text_View").GetComponent<TextMesh>();
        textView = GameObject.Find("Text_View").GetComponent<Text>();
        InitCamera();

        #if UNITY_ANDROID
        outsideCamera.transform.position = androidView;
        outsideCamera.transform.LookAt(listenerPosition);
        SetViewText("Top view");
        UpdateOnResize();
        #endif
    }

    // Update is called once per frame
    void Update ()
    {
#if !UNITY_ANDROID
        MoveCamera();
        UpdateOnResize();
#else
        FollowListener();
#endif
    }

    //public void SetViewText(string newText)
    //{
    //    // Find top-left corner of viewport
    //    Vector3 textPosition = outsideCamera.ViewportToWorldPoint(new Vector3(TEXT_LEFT_MARGIN, 1, TEXT_DISTANCE));        
    //    textView.transform.position = textPosition;
    //    textView.transform.forward = outsideCamera.transform.forward;
    //    textView.text = newText;
    //}

    // TO DO: it seems that Unity does not have a straightforward way of dealing with resize events. This can be studied in more depth...
    public void UpdateOnResize()
    {
        // Update camera frame
        Camera camera = outsideCamera.GetComponent<Camera>();        
        GameObject frame = GameObject.FindGameObjectWithTag("Camera_Frame");
        RectTransform frameTransform = frame.GetComponent<RectTransform>();        
        frameTransform.sizeDelta = new Vector2(camera.pixelWidth*2+102, camera.pixelHeight*2+100);

        // Update camera view text position
        Vector3 textPosition = textView.rectTransform.position;
        textPosition.y = camera.pixelHeight+15;
        textView.rectTransform.position = textPosition;
    }

    public void FollowListener()
    {
        outsideCamera.transform.position = GameObject.Find("Listener").transform.position + topView;
    }

    public void SetViewText (string newText)
    {
        textView.text = newText;
    }

    public void InitCamera()
    {
        // Get initial camera transform for Back View
        defaultCameraPosition = outsideCamera.transform.position;
        defaultCameraRotation = outsideCamera.transform.rotation;
        outsideCamera.transform.position = defaultCameraPosition;
        outsideCamera.transform.rotation = defaultCameraRotation;

        // Find text position (top-left corner of viewport) and move it
        //int height = outsideCamera.pixelHeight;
        //int width = outsideCamera.pixelWidth;
        //Vector3 textTranslation = new Vector3(0, height + TEXT_TOP_MARGIN, 0.0f);
        //textView.transform.Translate(textTranslation);

        SetViewText("Back view");
    }

    public void MoveCamera()
    {
        KeyboardInput();
        MouseInput();
    }

    public void KeyboardInput()
    {
        if ((Input.GetKey("[5]")) || (Input.GetKey("5")))  // Default view
        {
            outsideCamera.transform.position = defaultCameraPosition;
            outsideCamera.transform.rotation = defaultCameraRotation;
            SetViewText("Back view");
        }

        if ((Input.GetKey("[8]")) || (Input.GetKey("8")))  // Top view
        {
            outsideCamera.transform.position = topView;
            outsideCamera.transform.LookAt(listenerPosition);
            SetViewText("Top view");
        }

        if ((Input.GetKey("[2]")) || (Input.GetKey("2")))  // Front view
        {
            outsideCamera.transform.position = frontView;
            outsideCamera.transform.LookAt(listenerPosition);
            SetViewText("Front view");
        }

        if ((Input.GetKey("[4]")) || (Input.GetKey("4")))  // Left view
        {
            outsideCamera.transform.position = leftView;
            outsideCamera.transform.LookAt(listenerPosition);
            SetViewText("Left view");
        }

        if ((Input.GetKey("[6]")) || (Input.GetKey("6")))  // Right view
        {
            outsideCamera.transform.position = rightView;
            outsideCamera.transform.LookAt(listenerPosition);
            SetViewText("Right view");
        }
    }

    public void MouseInput()
    {
        // Check if mouse is over outside camera
        if (outsideCamera.pixelRect.Contains(Input.mousePosition))
        {
            // Move azimuth/elevation
            if (Input.GetMouseButton(0))
            {
                transform.RotateAround(listenerPosition, AZIMUTH_AXIS, Input.GetAxis("Mouse X") * MOUSE_AZIMUTH_SCALE);
                transform.RotateAround(listenerPosition, transform.right, Input.GetAxis("Mouse Y") * -MOUSE_ELEVATION_SCALE);
                SetViewText("User view");
            }


            // Move distance
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                float distance = Vector3.Distance(transform.position, listenerPosition);
                Vector3 translateVector = transform.position - listenerPosition;
                translateVector.Normalize();

                // Invert sign
                scroll = -scroll;

                // Move
                if ((distance < MOUSE_DISTANCE_MAX) && (scroll > 0))
                {
                    transform.Translate(translateVector * MOUSE_DISTANCE_SCALE, Space.World);
                }
                if ((distance > MOUSE_DISTANCE_MIN) && (scroll < 0))
                {
                    transform.Translate(translateVector * -MOUSE_DISTANCE_SCALE, Space.World);
                }

                SetViewText("User view");
            }
        }
    }
}

