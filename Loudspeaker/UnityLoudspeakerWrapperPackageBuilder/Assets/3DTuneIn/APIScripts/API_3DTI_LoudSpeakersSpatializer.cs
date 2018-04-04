using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;

public class API_3DTI_LoudSpeakersSpatializer : MonoBehaviour {

    // SOURCE:
    int lastSourceID = 0;                       // Internal use for debug log

    // CONFIGURATION PRESETS:
    public enum T_LoudSpeakerConfigurationPreset { LS_PRESET_CUBE=0, LS_PRESET_OCTAHEDRON=1, LS_PRESET_2DSQUARE=2 , LS_IRREGULAR_CONFIG=3};
    public T_LoudSpeakerConfigurationPreset  speakersConfigurationPreset = T_LoudSpeakerConfigurationPreset.LS_PRESET_CUBE;

    // ADVANCED:
    public float scaleFactor = 1.0f;            // Used by Inspector
    public bool modFarLPF = true;               // Used by Inspector
    public bool modDistAtt = true;              // Used by Inspector
    public float magAnechoicAttenuation = -6.0f;    // Used by Inspector    
    public float magSoundSpeed = 343.0f;            // Used by Inspector
    public bool debugLog = false;                   // Used by Inspector
    public float structureSide = 1.0f;  // Used by Inspector    

    float[,] speakerCoefficients = new float[8,9];
    public List<Vector3> speakerPositions;  // Used by Inspector
    public List<Vector3> speakerOffsets;    // Used by Inspector
    int numberOfSpeakers;    

    // Definition of spatializer plugin commands   
    int SET_SCALE_FACTOR    = 0;
    int SET_SOURCE_ID       = 1;    
    int SET_MOD_FARLPF      = 2;
    int SET_MOD_DISTATT     = 3;    
    int SET_MAG_ANECHATT    = 4;
    int SET_MAG_SOUNDSPEED  = 5;    
    int SET_DEBUG_LOG       = 6;
    int SET_SAVE_SPEAKERS_CONFIG = 7;
    int SET_SPEAKER_1_X =8;
    int SET_SPEAKER_2_X = 9;
    int SET_SPEAKER_3_X = 10;
    int SET_SPEAKER_4_X = 11;
    int SET_SPEAKER_5_X = 12;
    int SET_SPEAKER_6_X = 13;
    int SET_SPEAKER_7_X = 14;
    int SET_SPEAKER_8_X = 15;
    int SET_SPEAKER_1_Y = 16;
    int SET_SPEAKER_2_Y = 17;
    int SET_SPEAKER_3_Y = 18;
    int SET_SPEAKER_4_Y = 19;
    int SET_SPEAKER_5_Y = 20;
    int SET_SPEAKER_6_Y = 21;
    int SET_SPEAKER_7_Y = 22;
    int SET_SPEAKER_8_Y = 23;
    int SET_SPEAKER_1_Z = 24;
    int SET_SPEAKER_2_Z = 25;
    int SET_SPEAKER_3_Z = 26;
    int SET_SPEAKER_4_Z = 27;
    int SET_SPEAKER_5_Z = 28;
    int SET_SPEAKER_6_Z = 29;
    int SET_SPEAKER_7_Z = 30;
    int SET_SPEAKER_8_Z = 31;
    int GET_MINIMUM_DISTANCE = 32;

    // Hack for modifying one single AudioSource (TO DO: fix this)
    bool selectSource = false;
    AudioSource selectedSource;

    // This is needed from Unity 2017
    bool isInitialized = false;

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Automatic setup of Toolkit Core (as read from custom GUI in Unity Inspector)
    /// </summary>
    void Start()
    {
        //StartLoudSpeakersSpatializer();
    }

    void Update()
    {
        if (!isInitialized)
        {
            if (StartLoudSpeakersSpatializer())
                isInitialized = true;
        }
    }

    /////////////////////////////////////////////////////////////////////
    // GLOBAL METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Sends all configuration to all spatialized sources. 
    /// Use it each time you activate an audio source or activate its "spatialize" attribute. 
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>    
    public bool StartLoudSpeakersSpatializer(AudioSource source = null)
    {
        // Select only one AudioSource
        if (source != null)
        {
            selectSource = true;
            selectedSource = source;
        }

        // Debug log:
        if (!SendWriteDebugLog(debugLog)) return false;

        // Global setup:
        if (!SetScaleFactor(scaleFactor)) return false;
        if (!SendSourceIDs()) return false;

        // Setup modules enabler:
        if (!SetupModulesEnabler()) return false;

        // Setup speakers configuration (position)
        if (!SetSpeakersConfigurationPreset(speakersConfigurationPreset)) return false;

        // Go back to default state, affecting all sources
        selectSource = false;

        return true;
    }


    /////////////////////////////////////////////////////////////////////
    // SOURCE API METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    ///  Set one speakers configuration preset and initialize the sperkers positions
    /// </summary>
    /// <param name="preset"> Speakers setup configuration. Param type: T_LoudSpeakerConfigurationPreset</param>
    /// <returns>return false if the preset is not valid</returns>
    public bool SetSpeakersConfigurationPreset(T_LoudSpeakerConfigurationPreset preset)
    {
        if ((preset != T_LoudSpeakerConfigurationPreset.LS_PRESET_2DSQUARE) &&
            (preset != T_LoudSpeakerConfigurationPreset.LS_PRESET_CUBE) &&
            (preset != T_LoudSpeakerConfigurationPreset.LS_PRESET_OCTAHEDRON) &&
            (preset != T_LoudSpeakerConfigurationPreset.LS_IRREGULAR_CONFIG))
            return false;

        speakersConfigurationPreset = preset;
        speakerPositions.Clear();
        speakerOffsets.Clear();
        
        switch (preset)
        {
            case T_LoudSpeakerConfigurationPreset.LS_PRESET_CUBE:
                numberOfSpeakers = 8;
                break;            
            case T_LoudSpeakerConfigurationPreset.LS_PRESET_OCTAHEDRON:
                numberOfSpeakers = 6;
                break;
            case T_LoudSpeakerConfigurationPreset.LS_PRESET_2DSQUARE:
                numberOfSpeakers = 4;
                break;
            case T_LoudSpeakerConfigurationPreset.LS_IRREGULAR_CONFIG:
                numberOfSpeakers = 8;
                break;
            default:
                return false;                
        }        

        // Create positions, offsets and weights for each speaker
        for (int i=0; i < numberOfSpeakers; i++)
        {
            speakerPositions.Add(Vector3.zero);
            speakerOffsets.Add(Vector3.zero);
        }

        // Calculate speaker positions and weights and send configuration to toolkit
        CalculateSpeakerPositions();
        if (!SendLoudSpeakersConfiguration()) return false;
        return true;
    }

    /// <summary>
    /// Get number of speakers according to the currently set speakers configuration preset
    /// </summary>
    /// <returns></returns>
    public int GetNumberOfSpeakers()
    {
        return numberOfSpeakers;
    }    

    /// <summary>
    /// Set size of one side of the speakers configuration preset, in meters
    /// </summary>
    /// <param name="side"> Indicate in meter the side of the speakers structure</param>
    /// <returns></returns>
    public bool SetStructureSide(float side)
    {
        structureSide = side;
        CalculateSpeakerPositions();
        return SendLoudSpeakersConfiguration();
    }

    /// <summary>
    /// Get size of one side of the speakers configuration preset, in meters
    /// </summary>
    /// <returns>side in meters</returns>
    public float GetStructureSide()
    {
        return structureSide;
    }

    /// <summary>
    /// Get minimum distance from any source to listener according to the speakers setup)
    /// </summary>
    /// <returns></returns>
    public float GetMinimumDistanceToListener()
    {
        float returnValue;

        List<AudioSource> sources = GetAllSpatializedSources();
        if (sources.Count == 0)
            return -1.0f;

        if (!sources[0].GetSpatializerFloat(GET_MINIMUM_DISTANCE, out returnValue))
            return -1.0f;
        else
            return returnValue;
    }

    /// <summary>
    /// Get position of one speaker
    /// </summary>
    /// <param name="speakerID"></param>
    /// <returns> speakers position in m (x,y,z) inclusing offset</returns>
    public Vector3 GetSpeakerPosition(int speakerID)
    {
        //Offset is in cm
        return new Vector3( speakerPositions[speakerID].x + speakerOffsets[speakerID].x * 0.01f,
                            speakerPositions[speakerID].y + speakerOffsets[speakerID].y * 0.01f,
                            speakerPositions[speakerID].z + speakerOffsets[speakerID].z * 0.01f);
    }

    /// <summary>
    /// Set offset for one speaker
    /// </summary>
    /// <param name="speakerID"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public bool SetSpeakerOffset(int speakerID, Vector3 offset)
    {        
        if ((speakerID < numberOfSpeakers) && (speakerID >= 0))
        {
            speakerOffsets[speakerID] = offset;
            return SendLoudSpeakersConfiguration();
        }
        else
            return false;
    }

    /// <summary>
    /// Send configuration of all speakers
    /// </summary>
    public bool SendLoudSpeakersConfiguration()
    {
        // Speaker 1     
        if (numberOfSpeakers > 0)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_1_X, GetSpeakerPosition(0).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_1_Y, GetSpeakerPosition(0).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_1_Z, GetSpeakerPosition(0).z)) return false;            
        }

        // Speaker 2
        if (numberOfSpeakers > 1)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_2_X, GetSpeakerPosition(1).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_2_Y, GetSpeakerPosition(1).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_2_Z, GetSpeakerPosition(1).z)) return false;            
        }

        // Speaker 3
        if (numberOfSpeakers > 2)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_3_X, GetSpeakerPosition(2).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_3_Y, GetSpeakerPosition(2).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_3_Z, GetSpeakerPosition(2).z)) return false;            
        }

        // Speaker 4
        if (numberOfSpeakers > 3)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_4_X, GetSpeakerPosition(3).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_4_Y, GetSpeakerPosition(3).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_4_Z, GetSpeakerPosition(3).z)) return false;            
        }

        // Speaker 5
        if (numberOfSpeakers > 4)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_5_X, GetSpeakerPosition(4).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_5_Y, GetSpeakerPosition(4).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_5_Z, GetSpeakerPosition(4).z)) return false;            
        }

        // Speaker 6
        if (numberOfSpeakers > 5)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_6_X, GetSpeakerPosition(5).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_6_Y, GetSpeakerPosition(5).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_6_Z, GetSpeakerPosition(5).z)) return false;            
        }

        // Speaker 7
        if (numberOfSpeakers > 6)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_7_X, GetSpeakerPosition(6).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_7_Y, GetSpeakerPosition(6).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_7_Z, GetSpeakerPosition(6).z)) return false;            
        }

        // Speaker 8
        if (numberOfSpeakers > 7)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_8_X, GetSpeakerPosition(7).x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_8_Y, GetSpeakerPosition(7).y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_8_Z, GetSpeakerPosition(7).z)) return false;            
        }

        // Send command for ending setup
        if (!SendCommandForAllSources(SET_SAVE_SPEAKERS_CONFIG, 1.0f)) return false;

        return true;
    }

    /// <summary>
    /// Calculate positions for all speakers, but do not send them to the plugin yet
    /// </summary>
    public void CalculateSpeakerPositions()
    {
        switch (speakersConfigurationPreset)
        {
            case T_LoudSpeakerConfigurationPreset.LS_PRESET_CUBE:
                float cubeSide = 0.5f * structureSide;
                speakerPositions[0] = new Vector3(cubeSide, cubeSide, cubeSide) + speakerOffsets[0];    // Front Left Up speaker
                speakerPositions[1] = new Vector3(cubeSide, -cubeSide, cubeSide) + speakerOffsets[1];   // Front Right Up speaker
                speakerPositions[2] = new Vector3(cubeSide, cubeSide, -cubeSide) + speakerOffsets[2];   // Front Left Down speaker
                speakerPositions[3] = new Vector3(cubeSide, -cubeSide, -cubeSide) + speakerOffsets[3];  // Front Right Down speaker                
                speakerPositions[4] = new Vector3(-cubeSide, cubeSide, cubeSide) + speakerOffsets[4];   // Rear Left Up speaker
                speakerPositions[5] = new Vector3(-cubeSide, -cubeSide, cubeSide) + speakerOffsets[5];  // Rear Right Up speaker                     
                speakerPositions[6] = new Vector3(-cubeSide, cubeSide, -cubeSide) + speakerOffsets[6];  // Rear Left Down speaker
                speakerPositions[7] = new Vector3(-cubeSide, -cubeSide, -cubeSide) + speakerOffsets[7]; // Rear Right Down speaker                    
                break;
            case T_LoudSpeakerConfigurationPreset.LS_PRESET_OCTAHEDRON:
                float octahedronSide = structureSide / Mathf.Sqrt(2);
                speakerPositions[0] = new Vector3(0.0f, octahedronSide, 0.0f) + speakerOffsets[0];   // Left speaker
                speakerPositions[1] = new Vector3(0.0f, -octahedronSide, 0.0f) + speakerOffsets[1];  // Right speaker
                speakerPositions[2] = new Vector3(octahedronSide, 0.0f, 0.0f) + speakerOffsets[2];   // Front speaker                
                speakerPositions[3] = new Vector3(-octahedronSide, 0.0f, 0.0f) + speakerOffsets[3];  // Back speaker
                speakerPositions[4] = new Vector3(0.0f, 0.0f, octahedronSide) + speakerOffsets[4];   // Top speaker         
                speakerPositions[5] = new Vector3(0.0f, 0.0f, -octahedronSide) + speakerOffsets[5];  // Bottom speaker               
                break;
            case T_LoudSpeakerConfigurationPreset.LS_PRESET_2DSQUARE:
                float _2DSquareSide = structureSide / Mathf.Sqrt(2);
                speakerPositions[0] = new Vector3(0.0f, _2DSquareSide, 0.0f) + speakerOffsets[0];   // Left speaker
                speakerPositions[1] = new Vector3(0.0f, -_2DSquareSide, 0.0f) + speakerOffsets[1];   // Right speaker
                speakerPositions[2] = new Vector3(_2DSquareSide, 0.0f, 0.0f) + speakerOffsets[2];   // Front speaker
                speakerPositions[3] = new Vector3(-_2DSquareSide, 0.0f, 0.0f) + speakerOffsets[3];  // Back speaker
                break;
            case T_LoudSpeakerConfigurationPreset.LS_IRREGULAR_CONFIG:
                LoadXML();
                break;
        }
    }

    private void LoadXML()
    {
        string filePath;
        if (xmlPath == "")
        {
            filePath = "./Assets/3DTuneIn/Resources/Coef.xml";
        }
        else
        {
            filePath = xmlPath;
        }
        if (filePath != "")
        {
            XmlDocument file = new XmlDocument();
            file.Load(filePath);
            XmlNodeList nodeList = file.GetElementsByTagName("pos");
            Debug.Log("Número de miembros de nodeList para coordenadas: " + nodeList.Count.ToString());
            for(int i = 0; i < nodeList.Count; i++) {
                string coordString = nodeList[i].InnerXml;
                Debug.Log("String de coordenadas del elemento " + i.ToString() + ": " + coordString);
                string[] coordStringArray = CoefficientSplit(coordString, ',');
                string[] coordStringArrayFixed = new string[coordStringArray.Length];

                for (int k = 0; k < 3; k++)
                {
                    coordStringArrayFixed.SetValue(coordStringArray[k].Substring(1), k);
                    Debug.Log("Coordenada " + k.ToString() + " del altavoz " + i.ToString() +  ": " + coordStringArrayFixed[k].ToString());
                    
                }

                float x, y, z;

                float.TryParse(coordStringArrayFixed[0].ToString(), out x);
                float.TryParse(coordStringArrayFixed[1].ToString(), out y);
                float.TryParse(coordStringArrayFixed[2].ToString(), out z);

                Debug.Log(x.ToString() + y.ToString() + z.ToString());

                speakerPositions[i] = new Vector3(x,y,z);
                                                                                                           
                Debug.Log(speakerPositions[i].x);
                Debug.Log(speakerPositions[i].y);
                Debug.Log(speakerPositions[i].z);
            }
            nodeList = file.GetElementsByTagName("gains");
            Debug.Log("Número de miembros de nodeList para coeficientes: " + nodeList.Count.ToString());
            for (int i = 0; i < nodeList.Count; i++)
            {
                string coeffString = nodeList[i].InnerXml;
                Debug.Log("String de coeficientes del elemento " + i.ToString() +": " + coeffString);
                string[] coeffStringArray = CoefficientSplit(coeffString, ',');

                for (int k = 0; k < 9; k++)
                {
                    speakerCoefficients[i, k] = float.Parse(coeffStringArray[k].Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                    Debug.Log("Coeficiente " + k.ToString() + " del altavoz " + i.ToString() + ": " + speakerCoefficients[i, k].ToString());
                }
            }
        }

    }

    private string[] CoefficientSplit(string s, char c)
    {
        string[] numberStringVector;
        numberStringVector = s.Split(',');
        //Debug.Log("Tamaño array: " + numberStringVector.Length.ToString());
        //for(int i = 0; i < numberStringVector.Length; i++) Debug.Log(numberStringVector[i]);
        return numberStringVector;
    }

    /// <summary>
    /// Set ID for all sources, for internal use of the wrapper
    /// </summary>
    public bool SendSourceIDs()
    {
        if (!selectSource)
        {
            List<AudioSource> audioSources = GetAllSpatializedSources();
            foreach (AudioSource source in audioSources)
            {
                if (!source.SetSpatializerFloat(SET_SOURCE_ID, (float)++lastSourceID)) return false;
            }
            return true;
        }
        else
            return selectedSource.SetSpatializerFloat(SET_SOURCE_ID, (float)++lastSourceID);
    }

    
        /// <summary>
    /// Set scale factor. Allows the toolkit to work with different scaled scenarios
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    public bool SetScaleFactor(float scale)
    {
        scaleFactor = scale;
        return SendCommandForAllSources(SET_SCALE_FACTOR, scale);
    }

    /// <summary>
    /// Setup the diantance attenuation and far distance simulation according to the API_3DTI_LoudSpeakersSpatializer variables modDistAtt and modFarLPF respectively 
    /// </summary>
    /// <returns>true if the setup has been done correctly</returns>
    public bool SetupModulesEnabler()
    {
        if (!SetModFarLPF(modFarLPF)) return false;
        if (!SetModDistanceAttenuation(modDistAtt)) return false;       
        return true;
    }
    

    /// <summary>
    /// Switch on/off far distance simulation (LPF)
    /// </summary>
    /// <param name="_enable"> true to activate far away distance attenuation</param>
    /// <returns>true if the data has been sent correctly</returns>
    public bool SetModFarLPF(bool _enable)
    {
        modFarLPF = _enable;        
        return SendCommandForAllSources(SET_MOD_FARLPF, Bool2Float(_enable));
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    ///  Switch on/off global distance attenuation
    /// </summary>
    /// <param name="_enable"> true to activate distance attenuation</param>
    /// <returns>true if the data has been sent correctly</returns>
    public bool SetModDistanceAttenuation(bool _enable)
    {
        modDistAtt = _enable;
        return SendCommandForAllSources(SET_MOD_DISTATT, Bool2Float(_enable));
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    ///  Set attenuation value (dB) for distance attenuation Attenuation
    /// </summary>
    /// <param name="value"></param>
    /// <returns>true if the data has been sent correctly</returns>
    public bool SetMagnitudeAnechoicAttenuation(float value)
    {
        magAnechoicAttenuation = value;
        return SendCommandForAllSources(SET_MAG_ANECHATT, value);
    }

    /// <summary>
    ///  Set value for  Sound Speed (m/s)
    /// </summary>
    /// <param name="value"></param>
    /// <returns>true if the data has been sent correctly</returns>
    public bool SetMagnitudeSoundSpeed(float value)
    {
        magSoundSpeed = value;
        return SendCommandForAllSources(SET_MAG_SOUNDSPEED, value);
    }


    /////////////////////////////////////////////////////////////////////
    // AUXILIARY FUNCTIONS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Send command to plugin to switch on/off write to Debug Log file
    /// </summary>
    /// <param name="_enable">true to activate wrinting in the file </param>
    /// <returns></returns>
    public bool SendWriteDebugLog(bool _enable)
    {
        debugLog = _enable;
        return SendCommandForAllSources(SET_DEBUG_LOG, Bool2Float(_enable));
    }

    // Send command to the DLL, for each registered source
    private bool SendCommandForAllSources(int command, float value)
    {
        if (!selectSource)
        {
            List<AudioSource> audioSources = GetAllSpatializedSources();
            foreach (AudioSource source in audioSources)
            {
                if (!source.SetSpatializerFloat(command, value))
                    return false;
            }
            return true;
        }
        else
            return selectedSource.SetSpatializerFloat(command, value);
    }


    // Returns a list with all audio sources with the Spatialized toggle checked
    private List<AudioSource> GetAllSpatializedSources()
    {
        //GameObject[] audioSources = GameObject.FindGameObjectsWithTag("AudioSource");

        List<AudioSource> spatializedSources = new List<AudioSource>();

        AudioSource[] audioSources = UnityEngine.Object.FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source.spatialize)
                spatializedSources.Add(source);
        }

        return spatializedSources;
    }

    private float Bool2Float(bool v)
    {
        if (v)
            return 1.0f;
        else
            return 0.0f;
    }
}
