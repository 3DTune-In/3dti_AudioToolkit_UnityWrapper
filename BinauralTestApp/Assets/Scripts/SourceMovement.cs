using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SourceMovement : MonoBehaviour
{
    // Spheric offsets
    private float AZIMUTH_OFFSET = 1.0f;
    private float ELEVATION_OFFSET = 1.0f;
    private float DISTANCE_OFFSET = 0.05f;

    // Limits
    private float AZIMUTH_MIN = -179;
    private float AZIMUTH_MAX = 179; // Mathf.PI;
    private float ELEVATION_MIN = -89;
    private float ELEVATION_MAX = 89; // Mathf.PI / 2;
    private float DISTANCE_MAX = 20.0f;
    //private float DISTANCE_MIN = 1.95f;
    private float DISTANCE_MIN = 0.2f;


    // GUI
    private Text textAzimuth;
    private Text textElevation;
    private Text textDistance;
    private Text textName;
    private Text textCoordinates;
    private Slider slAzimuth;
    private Slider slElevation;
    private Slider slDistance;
    private Slider slElevationFine;
    private Slider slAzimuthFine;

    // Fine adjustment sliders
    private float azimuthFine = 0.0f;
    private float elevationFine = 0.0f;
    private float AZIMUTH_FINE_MIN = -5.0f;
    private float AZIMUTH_FINE_MAX = 5.0f;
    private float ELEVATION_FINE_MIN = -5.0f;
    private float ELEVATION_FINE_MAX = 5.0f;
    private Text textElevationFine;
    private Text textAzimuthFine;

    // References
    private Vector3 AZIMUTH_AXIS = new Vector3(0.0f, 1.0f, 0.0f);
    private Vector3 listenerPosition;

    /////////////////////////////////////////////////////////

    // Use this for initialization
    void Start()
    {
        textAzimuth = GameObject.Find("Text_Azimuth").GetComponent<Text>();
        textElevation = GameObject.Find("Text_Elevation").GetComponent<Text>();
        textDistance = GameObject.Find("Text_Distance").GetComponent<Text>();
        textName = GameObject.Find("Text_Name").GetComponent<Text>();
        textCoordinates = GameObject.Find("Text_Coordinates_Value").GetComponent<Text>();
        textAzimuthFine = GameObject.Find("Text_Azimuth_Fine").GetComponent<Text>();
        textElevationFine = GameObject.Find("Text_Elevation_Fine").GetComponent<Text>();
        slAzimuth = GameObject.Find("Slider_Azimuth").GetComponent<Slider>();
        slAzimuthFine = GameObject.Find("Slider_Azimuth_Fine").GetComponent<Slider>();
        slElevation = GameObject.Find("Slider_Elevation").GetComponent<Slider>();
        slElevationFine = GameObject.Find("Slider_Elevation_Fine").GetComponent<Slider>();
        slDistance = GameObject.Find("Slider_Distance").GetComponent<Slider>();

        listenerPosition = Vector3.zero;    // fixed listener

        UpdateGUI();
    }

    // Update is called once per frame
    void Update()
    {
        KeyboardInput();
    }

    public void UpdateGUIText()
    {        
        textName.text = "Audio Source 1";//:\t X=" + transform.position.x.ToString("F1") + ", Y=" + transform.position.y.ToString("F1") + ", Z=" + transform.position.z.ToString("F1");
        textCoordinates.text = "X = " + transform.position.x.ToString("F1") + "m\nY = " + transform.position.y.ToString("F1") + "m\nZ = " + transform.position.z.ToString("F1") + "m";
        textAzimuth.text = "Azimuth:\t" + GetAzimuth().ToString("F2") + "º";
        textAzimuthFine.text = azimuthFine.ToString("Fine tuning:\t +0.00º;Fine tuning:\t -0.00º");
        textElevation.text = "Elevation:\t" + GetElevation().ToString("F2") + "º";
        textElevationFine.text = elevationFine.ToString("Fine tuning:\n +0.00º;Fine tuning:\n -0.00º");
        textDistance.text = "Distance:\t" + GetDistance().ToString("F2") + "m";
    }

    public float GetAzimuth()
    {
        float azimuth;

        // Compute azimuth
        float x = transform.position.x;
        float z = transform.position.z;
        azimuth = Mathf.Atan2(z, -x);

        // Convert from radians to degrees
        azimuth = azimuth * Mathf.Rad2Deg;

        // Adjust to our -180..180 convention        
        if (azimuth < -90)
            azimuth = 270 + azimuth;
        else
            azimuth = azimuth - 90;

        return azimuth;
    }

    public float GetElevation()
    {
        float elevation;

        // Compute elevation
        float y = transform.position.y;
        elevation = Mathf.Acos(y / GetDistance());

        // Convert from radians to degrees
        elevation = elevation * Mathf.Rad2Deg;

        // Adjust to our -90..90 convention
        elevation = 90 - elevation; // Mathf.PI / 2;

        return elevation;
    }

    public float GetDistance()
    {
        float distance = 0.0f;
        distance = Vector3.Distance(transform.position, listenerPosition);
        return distance;
    }

    public void CheckDistanceLimits()
    {
        float distance = GetDistance();

        if (distance > DISTANCE_MAX)
        {
            float offset = distance - DISTANCE_MAX;
            Vector3 translateVector = transform.position - listenerPosition;
            translateVector.Normalize();
            transform.Translate(translateVector * -offset, Space.World);
        }

        if (distance < DISTANCE_MIN)
        {
            float offset = DISTANCE_MIN - distance;
            Vector3 translateVector = transform.position - listenerPosition;
            translateVector.Normalize();
            transform.Translate(translateVector * offset, Space.World);
        }
    }

    public void KeyboardInput()
    {
        if (Input.GetKey("left"))
        {
            transform.RotateAround(listenerPosition, AZIMUTH_AXIS, -AZIMUTH_OFFSET);
            UpdateGUI();            
        }
        if (Input.GetKey("right"))
        {
            transform.RotateAround(listenerPosition, AZIMUTH_AXIS, AZIMUTH_OFFSET);
            UpdateGUI();            
        }
        if (Input.GetKey("up"))
        {
            if (GetElevation() < (ELEVATION_MAX - ELEVATION_OFFSET))
            {
                transform.RotateAround(listenerPosition, transform.right, -ELEVATION_OFFSET);
                UpdateGUI();
            }
        }
        if (Input.GetKey("down"))
        {
            if (GetElevation() > (ELEVATION_MIN + ELEVATION_OFFSET))
            {
                transform.RotateAround(listenerPosition, transform.right, ELEVATION_OFFSET);
                UpdateGUI();
            }
        }
        if (Input.GetKey("a"))
        {
            if (Vector3.Distance(transform.position, listenerPosition) < DISTANCE_MAX)
            {
                Vector3 translateVector = transform.position - listenerPosition;
                translateVector.Normalize();
                transform.Translate(translateVector * DISTANCE_OFFSET, Space.World);
                CheckDistanceLimits();
            }
            UpdateGUI();
        }
        if (Input.GetKey("z"))
        {
            if (Vector3.Distance(transform.position, listenerPosition) > DISTANCE_MIN)
            {
                Vector3 translateVector = transform.position - listenerPosition;
                translateVector.Normalize();
                transform.Translate(translateVector * -DISTANCE_OFFSET, Space.World);
            }
            UpdateGUI();
        }
    }

    /// <summary>
    /// GUI Methods:
    /// </summary>

    public void UpdateGUI()
    {
        slAzimuth.value = GetAzimuth();
        slElevation.value = GetElevation();
        slDistance.value = GetDistance();
        slElevationFine.value = elevationFine;
        slAzimuthFine.value = azimuthFine;
        UpdateGUIText();
    }

    public float SliderAzimuth
    {
        get
        {
            float azimuth = GetAzimuth();

            return azimuth;
        }
        set
        {
            float azimuth = GetAzimuth();

            // And rotate
            float offset = value - azimuth;
            transform.RotateAround(listenerPosition, AZIMUTH_AXIS, offset);
            UpdateGUIText();
        }
    }

    public float SliderAzimuthFine
    {
        get
        {
            return azimuthFine;
        }
        set
        {
            float offset = value - azimuthFine;

            // Check limits
            float currentAzimuth = GetAzimuth();
            if (currentAzimuth + offset > AZIMUTH_MAX)
            {
                offset = AZIMUTH_MAX - currentAzimuth;
            }
            else if (currentAzimuth + offset < AZIMUTH_MIN)
            {
                offset = AZIMUTH_MIN - currentAzimuth;                
            }
            else
                azimuthFine = value;

            // And rotate            
            transform.RotateAround(listenerPosition, AZIMUTH_AXIS, offset);

            UpdateGUI();
        }
    }

    public float SliderElevation
    {
        get
        {
            return GetElevation();
        }
        set
        {
            float currentElevation = GetElevation();
            float offset = currentElevation - value;

            transform.RotateAround(listenerPosition, transform.right, offset);

            UpdateGUIText();
        }
    }

    public float SliderElevationFine
    {
        get
        {
            return elevationFine;
        }
        set
        {
            float offset = elevationFine - value;
            
            // Check limits
            float currentElevation = GetElevation();
            if (currentElevation - offset > ELEVATION_MAX)
            {
                offset = -(ELEVATION_MAX - currentElevation);                
            }
            else if (currentElevation - offset < ELEVATION_MIN)
            {
                offset = (currentElevation - ELEVATION_MIN);                                
            }
            else
                elevationFine = value;            

            // And rotate
            transform.RotateAround(listenerPosition, transform.right, offset);            

            UpdateGUI();
        }
    }

    public float SliderDistance
    {
        get
        {
            return GetDistance();            
        }
        set
        {
            float currentDistance = GetDistance();
            float offset = value - currentDistance;
            Vector3 translateVector = transform.position - listenerPosition;
            translateVector.Normalize();
            transform.Translate(translateVector * offset, Space.World);
            CheckDistanceLimits();
            UpdateGUIText();
        }
    }

    //public void SliderElevationFine (Slider slider)
    //{
    //   Debug.Log(slider.value);
    //}
}

