using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class API_3DTI_LoudSpeakersSpatializer : MonoBehaviour {

    // LISTENER:
    //public string HRTFFileName = "";            // Used by Inspector
    //public string ILDFileName = "";             // Used by Inspector
    //public bool customITDEnabled = false;       // Used by Inspector
    //public float listenerHeadRadius = 0.0875f;  // Used by Inspector    

    // SOURCE:
    //public bool runtimeInterpolateHRTF = true;  // Used by Inspector
    int lastSourceID = 0;                       // Internal use for debug log

    // ADVANCED:
    public float scaleFactor = 1.0f;            // Used by Inspector
    public bool modFarLPF_LS = true;               // Used by Inspector
    public bool modDistAtt_LS = true;              // Used by Inspector
    public float magAnechoicAttenuation = -6.0f;    // Used by Inspector    
    public float magSoundSpeed = 343.0f;            // Used by Inspector
    public bool debugLog_LS = false;                   // Used by Inspector

    public float structureSide  = 1.70f;
    public float structureYaw   = 0.0f;
    public float structurePitch = 0.0f;

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
	int SET_SPEAKER_2_X =9;
	int SET_SPEAKER_3_X =10;
	int SET_SPEAKER_4_X =11;
	int SET_SPEAKER_5_X =12;
	int SET_SPEAKER_6_X =13;
	int SET_SPEAKER_7_X =14;
	int SET_SPEAKER_8_X =15;
	int SET_SPEAKER_1_Y =16;
	int SET_SPEAKER_2_Y =17;
	int SET_SPEAKER_3_Y =18;
	int SET_SPEAKER_4_Y =19;
	int SET_SPEAKER_5_Y =20;
	int SET_SPEAKER_6_Y =21;
	int SET_SPEAKER_7_Y =22;
	int SET_SPEAKER_8_Y =23;
	int SET_SPEAKER_1_Z =24;
	int SET_SPEAKER_2_Z =25;
	int SET_SPEAKER_3_Z =26;
	int SET_SPEAKER_4_Z =27;
	int SET_SPEAKER_5_Z =28;
	int SET_SPEAKER_6_Z =29;
	int SET_SPEAKER_7_Z =30;
	int SET_SPEAKER_8_Z =31;

    // Hack for modifying one single AudioSource (TO DO: fix this)
    bool selectSource = false;
    AudioSource selectedSource;

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Automatic setup of Toolkit Core (as read from custom GUI in Unity Inspector)
    /// </summary>
    void Start()
    {
        StartLoadSpeakersSpatializer();
    }

    /////////////////////////////////////////////////////////////////////
    // GLOBAL METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Sends all configuration to all spatialized sources. 
    /// Use it each time you reactive an audio source or reactive its "spatialize" attribute. 
    /// </summary>
    public bool StartLoadSpeakersSpatializer(AudioSource source = null)
    {
        // Select only one AudioSource
        if (source != null)
        {
            selectSource = true;
            selectedSource = source;
        }

        // Debug log:
        if (!SendWriteDebugLog(debugLog_LS)) return false;

        // Global setup:
        if (!SetScaleFactor(scaleFactor)) return false;
        if (!SendSourceIDs()) return false;

        // Setup modules enabler:
        if (!SetupModulesEnabler()) return false;

        // Setup speakers configuration (position)
        if (!SetupSpeakersConfiguration(structureSide)) return false;

        // Go back to default state, affecting all sources
        selectSource = false;

        return true;
    }


    /////////////////////////////////////////////////////////////////////
    // SOURCE API METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set scale factor. Allows the toolkit to work with big-scale or small-scale scenarios
    /// </summary>
    public bool SetSpeaker(int speakerID, Vector3 speakerPosition)
    {
        if (speakerID == 1)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_1_X, speakerPosition.x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_1_Y, speakerPosition.y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_1_Z, speakerPosition.z)) return false;            
        }else if (speakerID == 2)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_2_X, speakerPosition.x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_2_Y, speakerPosition.y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_2_Z, speakerPosition.z)) return false;
        }
        else if (speakerID == 3)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_3_X, speakerPosition.x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_3_Y, speakerPosition.y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_3_Z, speakerPosition.z)) return false;
        }
        else if (speakerID == 4)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_4_X, speakerPosition.x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_4_Y, speakerPosition.y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_4_Z, speakerPosition.z)) return false;
        }
        else if (speakerID == 5)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_5_X, speakerPosition.x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_5_Y, speakerPosition.y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_5_Z, speakerPosition.z)) return false;
        }
        else if (speakerID == 6)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_6_X, speakerPosition.x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_6_Y, speakerPosition.y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_6_Z, speakerPosition.z)) return false;
        }
        else if (speakerID == 7)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_7_X, speakerPosition.x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_7_Y, speakerPosition.y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_7_Z, speakerPosition.z)) return false;
        }
        else if (speakerID == 8)
        {
            if (!SendCommandForAllSources(SET_SPEAKER_8_X, speakerPosition.x)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_8_Y, speakerPosition.y)) return false;
            if (!SendCommandForAllSources(SET_SPEAKER_8_Z, speakerPosition.z)) return false;
        }
        else
        {
            return false;
        }
        return true;
    }

    public bool SendLoadSpeakerConfiguration()
    {        
        return SendCommandForAllSources(SET_SAVE_SPEAKERS_CONFIG, 1.0f);     
    }

    /// <summary>
    ///  Setup speakers configuration
    /// </summary>
    public bool SetupSpeakersConfiguration(float _structureSide/*, float _structureYaw, float structurePitch*/)
    {
        //Calculate Speakers Positions
       // Debug.Log("_structureSide: " + _structureSide);
        float halfSize = 0.5f * _structureSide;

        //if (!SetSpeaker(1, new Vector3(1.0f, 1.0f, 1.0f))) return false;
        //if (!SetSpeaker(2, new Vector3(1.0f, -1.0f, 1.0f))) return false;
        //if (!SetSpeaker(3, new Vector3(1.0f, 1.0f, -1.0f))) return false;
        //if (!SetSpeaker(4, new Vector3(1.0f, -1.0f, -1.0f))) return false;
        //if (!SetSpeaker(5, new Vector3(-1.0f, 1.0f, 1.0f))) return false;
        //if (!SetSpeaker(6, new Vector3(-1.0f, -1.0f, 1.0f))) return false;
        //if (!SetSpeaker(7, new Vector3(-1.0f, 1.0f, -1.0f))) return false;
        //if (!SetSpeaker(8, new Vector3(-1.0f, -1.0f, -1.0f))) return false;

        if (!SetSpeaker(1, new Vector3  (halfSize, halfSize, halfSize)))    return false;
        if (!SetSpeaker(2, new Vector3  (halfSize, -halfSize, halfSize)))     return false;
        if (!SetSpeaker(3, new Vector3  (halfSize, halfSize, -halfSize)))   return false;
        if (!SetSpeaker(4, new Vector3  (halfSize, -halfSize, -halfSize)))    return false;
        if (!SetSpeaker(5, new Vector3  (-halfSize, halfSize, halfSize)))     return false;
        if (!SetSpeaker(6, new Vector3  (-halfSize, -halfSize, halfSize)))      return false;
        if (!SetSpeaker(7, new Vector3  (-halfSize, halfSize, -halfSize)))    return false;
        if (!SetSpeaker(8, new Vector3  (-halfSize, -halfSize, -halfSize)))     return false;

        //Once the configuration is ready, load it into the core
        if (!SendLoadSpeakerConfiguration()) return false;

        return true;
    }


    /////////////////////////////////////////////////////////////////////
    // ADVANCED API METHODS
    /////////////////////////////////////////////////////////////////////

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

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set scale factor. Allows the toolkit to work with big-scale or small-scale scenarios
    /// </summary>
    public bool SetScaleFactor(float scale)
    {
        scaleFactor = scale;
        return SendCommandForAllSources(SET_SCALE_FACTOR, scale);
    }

    /// <summary>
    ///  Setup modules enabler, allowing to switch on/off core features
    /// </summary>
    public bool SetupModulesEnabler()
    {
        if (!SetModFarLPF(modFarLPF_LS)) return false;
        if (!SetModDistanceAttenuation(modDistAtt_LS)) return false;       
        return true;
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off far distance LPF
    /// </summary>        
    public bool SetModFarLPF(bool _enable)
    {
        modFarLPF_LS = _enable;
        if (_enable)
            return SendCommandForAllSources(SET_MOD_FARLPF, 1.0f);
        else
            return SendCommandForAllSources(SET_MOD_FARLPF, 0.0f);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off distance attenuation
    /// </summary>        
    public bool SetModDistanceAttenuation(bool _enable)
    {
        modDistAtt_LS = _enable;
        if (_enable)
            return SendCommandForAllSources(SET_MOD_DISTATT, 1.0f);
        else
            return SendCommandForAllSources(SET_MOD_DISTATT, 0.0f);
    }

    /////////////////////////////////////////////////////////////////////
  

    /// <summary>
    /// Set magnitude Anechoic Attenuation
    /// </summary>    
    public bool SetMagnitudeAnechoicAttenuation(float value)
    {
        magAnechoicAttenuation = value;
        return SendCommandForAllSources(SET_MAG_ANECHATT, value);
    }

    /// <summary>
    /// Set magnitude Sound Speed
    /// </summary>    
    public bool SetMagnitudeSoundSpeed(float value)
    {
        magSoundSpeed = value;
        return SendCommandForAllSources(SET_MAG_SOUNDSPEED, value);
    }

    /////////////////////////////////////////////////////////////////////
    // SOURCE API METHODS
    /////////////////////////////////////////////////////////////////////

    
    /////////////////////////////////////////////////////////////////////
    // AUXILIARY FUNCTIONS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Send command to plugin to switch on/off write to Debug Log file
    /// </summary>
    public bool SendWriteDebugLog(bool _enable)
    {
        debugLog_LS = _enable;
        if (_enable)
            return SendCommandForAllSources(SET_DEBUG_LOG, 1.0f);
        else
            return SendCommandForAllSources(SET_DEBUG_LOG, 0.0f);
    }

    /// <summary>
    /// Send command to the DLL, for each registered source
    /// </summary>
    public bool SendCommandForAllSources(int command, float value)
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

    /// <summary>
    /// Returns a list with all audio sources with the Spatialized toggle checked
    /// </summary>
    public List<AudioSource> GetAllSpatializedSources()
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

    /// <summary>
    /// Auxiliary function
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    float Bool2Float(bool v)
    {
        if (v)
            return 1.0f;
        else
            return 0.0f;
    }
}
