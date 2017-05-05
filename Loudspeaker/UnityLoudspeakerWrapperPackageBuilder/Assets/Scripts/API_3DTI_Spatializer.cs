/**
*** API for 3D-Tune-In Toolkit Unity Wrapper ***
*
* version beta 1.0
* Created on: July 2016
* 
* Author: 3DI-DIANA Research Group / University of Malaga / Spain
* Contact: areyes@uma.es
* 
* Project: 3DTI (3D-games for TUNing and lEarnINg about hearing aids)
* Module: 3DTI Toolkit Unity Wrapper
**/

using UnityEngine;
using System.Collections;
using System.IO;            // Needed for FileStream
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;


public class API_3DTI_Spatializer : MonoBehaviour
{
    // LISTENER:
    public string HRTFFileName = "";            // Used by Inspector
    public string ILDFileName = "";             // Used by Inspector
    public bool customITDEnabled = false;       // Used by Inspector
    public float listenerHeadRadius = 0.0875f;  // Used by Inspector    

    // SOURCE:
    public bool runtimeInterpolateHRTF = true;  // Used by Inspector
    int lastSourceID = 0;                       // Internal use for debug log

    // ADVANCED:
    public float scaleFactor = 1.0f;            // Used by Inspector
    public bool modFarLPF = true;               // Used by Inspector
    public bool modDistAtt = true;              // Used by Inspector
    public bool modILD = true;                  // Used by Inspector
    public bool modHRTF = true;                 // Used by Inspector
    public float magAnechoicAttenuation = -6.0f;    // Used by Inspector    
    public float magSoundSpeed = 343.0f;            // Used by Inspector
    public bool debugLog = false;                   // Used by Inspector

    // HEARING AID DIRECTIONALITY:
    public const int EAR_RIGHT = 0;
    public const int EAR_LEFT = 1;
    public const int EAR_BOTH = 2;   
    public bool doHADirectionalityLeft = false;     // Used by Inspector
    public bool doHADirectionalityRight = false;    // Used by Inspector
    public float HADirectionalityExtendLeft = 15.0f;    // Used by Inspector
    public float HADirectionalityExtendRight = 15.0f;   // Used by Inspector

    // Definition of spatializer plugin commands
    int LOAD_3DTI_HRTF = 0;
    int SET_HEAD_RADIUS = 1;
    int SET_SCALE_FACTOR = 2;
    int SET_SOURCE_ID = 3;    
    int SET_CUSTOM_ITD = 4;
    int SET_HRTF_INTERPOLATION = 5;
    int SET_MOD_FARLPF = 6;
    int SET_MOD_DISTATT = 7;
    int SET_MOD_ILD = 8;
    int SET_MOD_HRTF = 9;
    int SET_MAG_ANECHATT = 10;    
    int SET_MAG_SOUNDSPEED = 11;
    int LOAD_3DTI_ILD = 12;    
    int SET_DEBUG_LOG = 13;
    int SET_HA_DIRECTIONALITY_EXTEND_LEFT = 14;
    int SET_HA_DIRECTIONALITY_EXTEND_RIGHT = 15;
    int SET_HA_DIRECTIONALITY_ON_LEFT = 16;
    int SET_HA_DIRECTIONALITY_ON_RIGHT = 17;

    // Hack for modifying one single AudioSource (TO DO: fix this)
    bool selectSource = false;
    AudioSource selectedSource;

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Automatic setup of Toolkit Core (as read from custom GUI in Unity Inspector)
    /// </summary>
    void Start ()
    {        
        StartBinauralSpatializer();
    }

    /////////////////////////////////////////////////////////////////////
    // GLOBAL METHODS
    /////////////////////////////////////////////////////////////////////
    
    /// <summary>
    /// Sends all configuration to all spatialized sources. 
    /// Use it each time you reactive an audio source or reactive its "spatialize" attribute. 
    /// </summary>
    public bool StartBinauralSpatializer(AudioSource source=null)
    {
        // Select only one AudioSource
        if (source != null)
        {
            selectSource = true;
            selectedSource = source;
        }

        // Debug log:
        if (!SendWriteDebugLog(debugLog))   return false;

        // Global setup:
        if (!SetScaleFactor(scaleFactor))   return false;
        if (!SendSourceIDs())               return false;

        // Setup modules enabler:
        if (!SetupModulesEnabler()) return false;

        // Source setup:
        if (!SetupSource()) return false;

        // Listener setup:
        if (!SetupListener()) return false;

        // Hearing Aid directionality setup:
        if (!SetupHADirectionality()) return false;

        // Go back to default state, affecting all sources
        selectSource = false;

        return true;
    }

    /////////////////////////////////////////////////////////////////////
    // LISTENER METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Setup all listener parameters
    /// </summary>
    public bool SetupListener()
    {
        if (!SetHeadRadius(listenerHeadRadius)) return false;
        if (!SetCustomITD(customITDEnabled)) return false;

        if (!ILDFileName.Equals(""))
        {            
            #if (!UNITY_EDITOR)
            if (SaveResourceAsBinary(ILDFileName, ".3dti-ild", out ILDFileName) != 1) return false;
            #endif
            if (!LoadILDBinary(ILDFileName)) return false;
        }        

        if (!HRTFFileName.Equals(""))
        {
            #if (!UNITY_EDITOR)
            if (SaveResourceAsBinary(HRTFFileName, ".3dti-hrtf", out HRTFFileName) != 1) return false; 
            #endif
            if (!LoadHRTFBinary(HRTFFileName)) return false;
        }

        return true;
    }

    /// <summary>
    /// Load one file from resources and save it as a binary file (for Android)
    /// </summary>    
    public int SaveResourceAsBinary(string originalname, string extension, out string filename)
    {        
        // Get only file name from full path
        string namewithoutextension = Path.GetFileNameWithoutExtension(originalname);        

        // Setup name for new file
        string dataPath = Application.persistentDataPath;
        string newfilename = dataPath + "/" + namewithoutextension + extension;
        filename = newfilename;        

        // Load as asset from resources 
        TextAsset txtAsset = Resources.Load(namewithoutextension) as TextAsset;
        if (txtAsset == null)
        {            
            return -1;  // Could not load asset from resources
        }        

        // Transform asset into stream and then into byte array
        MemoryStream streamData = new MemoryStream(txtAsset.bytes);
        byte[] dataArray = streamData.ToArray();

        // Write binary data to binary file        
        using (BinaryWriter writer = new BinaryWriter(File.Open(newfilename, FileMode.Create)))
        {
            writer.Write(dataArray);
        }
        
        return 1;
    }

    /// <summary>
    /// Set listener head radius
    /// </summary>
    public bool SetHeadRadius(float headRadius)
    {
        listenerHeadRadius = headRadius;
        return SendCommandForAllSources(SET_HEAD_RADIUS, headRadius);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set custom ITD enabled/disabled
    /// </summary>
    public bool SetCustomITD(bool _enable)
    {
        customITDEnabled = _enable;
        if (_enable)
            return SendCommandForAllSources(SET_CUSTOM_ITD, 1.0f);
        else
            return SendCommandForAllSources(SET_CUSTOM_ITD, 0.0f);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Load HRTF from a binary .3dti file
    /// </summary>
    public bool LoadHRTFBinary(string filename)
    {
        HRTFFileName = filename;

        List<AudioSource> audioSources;
        if (selectSource)
        {
            audioSources = new List<AudioSource>();
            audioSources.Add(selectedSource);
        }
        else
        {
            audioSources = GetAllSpatializedSources();
        }

        foreach (AudioSource source in audioSources)
        {
            source.SetSpatializerFloat(LOAD_3DTI_HRTF, (float)filename.Length);
            for (int i = 0; i < filename.Length; i++)
            {
                int chr2Int = (int)filename[i];
                float chr2Float = (float)chr2Int;
                if (!source.SetSpatializerFloat(LOAD_3DTI_HRTF, chr2Float)) return false;
            }
        }

        return true;
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Load ILD from a binary .3dti file
    /// </summary>
    public bool LoadILDBinary(string filename)
    {
        ILDFileName = filename;

        List<AudioSource> audioSources;
        if (selectSource)
        {
            audioSources = new List<AudioSource>();
            audioSources.Add(selectedSource);
        }
        else
        {
            audioSources = GetAllSpatializedSources();
        }

        foreach (AudioSource source in audioSources)
        {
            source.SetSpatializerFloat(LOAD_3DTI_ILD, (float)filename.Length);            
            for (int i = 0; i < filename.Length; i++)
            {
                int chr2Int = (int)filename[i];
                float chr2Float = (float)chr2Int;
                if (!source.SetSpatializerFloat(LOAD_3DTI_ILD, chr2Float)) return false;
            }            
        }

        return true;
    }


    /////////////////////////////////////////////////////////////////////
    // SOURCE API METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Setup all source parameters
    /// </summary>        
    public bool SetupSource()
    {
        return SetSourceInterpolation(runtimeInterpolateHRTF);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set source HRTF interpolation method
    /// </summary>
    public bool SetSourceInterpolation(bool _run)
    {
        runtimeInterpolateHRTF = _run;        
        if (!_run)            
            return SendCommandForAllSources(SET_HRTF_INTERPOLATION, 0.0f);
        else
            return SendCommandForAllSources(SET_HRTF_INTERPOLATION, 1.0f);
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
    public bool SetScaleFactor (float scale)
    {
        scaleFactor = scale;
        return SendCommandForAllSources(SET_SCALE_FACTOR, scale);
    }

    /// <summary>
    ///  Setup modules enabler, allowing to switch on/off core features
    /// </summary>
    public bool SetupModulesEnabler()
    {
        if (!SetModFarLPF(modFarLPF)) return false;
        if (!SetModDistanceAttenuation(modDistAtt)) return false;
        if (!SetModILD(modILD)) return false;
        if (!SetModHRTF(modHRTF)) return false;
        return true;
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off far distance LPF
    /// </summary>        
    public bool SetModFarLPF(bool _enable)
    {
        modFarLPF = _enable;
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
        modDistAtt = _enable;
        if (_enable)
            return SendCommandForAllSources(SET_MOD_DISTATT, 1.0f);
        else
            return SendCommandForAllSources(SET_MOD_DISTATT, 0.0f);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off near distance ILD
    /// </summary>        
    public bool SetModILD(bool _enable)
    {
        modILD = _enable;
        if (_enable)
            return SendCommandForAllSources(SET_MOD_ILD, 1.0f);
        else
            return SendCommandForAllSources(SET_MOD_ILD, 0.0f);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off HRTF convolution
    /// </summary>        
    public bool SetModHRTF(bool _enable)
    {
        modHRTF = _enable;
        if (_enable)
            return SendCommandForAllSources(SET_MOD_HRTF, 1.0f);
        else
            return SendCommandForAllSources(SET_MOD_HRTF, 0.0f);        
    }

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
    
    /// <summary>
    ///  Initial setup of HA directionality
    /// </summary>
    public bool SetupHADirectionality()
    {
        if (!SwitchOnOffHADirectionality(EAR_LEFT, doHADirectionalityLeft)) return false;
        if (!SwitchOnOffHADirectionality(EAR_RIGHT, doHADirectionalityRight)) return false;
        if (!SetHADirectionalityExtend(EAR_LEFT, HADirectionalityExtendLeft)) return false;
        if (!SetHADirectionalityExtend(EAR_RIGHT, HADirectionalityExtendRight)) return false;
        return true;
    }

    /// <summary>
    /// Switch on/off HA directionality for each ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="_enable"></param>
    public bool SwitchOnOffHADirectionality(int ear, bool _enable)
    {
        if (ear == EAR_BOTH)
        {
            SwitchOnOffHADirectionality(EAR_LEFT, _enable);
            SwitchOnOffHADirectionality(EAR_RIGHT, _enable);
        }

        if (ear == EAR_LEFT)
        {
            doHADirectionalityLeft = _enable;
            return SendCommandForAllSources(SET_HA_DIRECTIONALITY_ON_LEFT, Bool2Float(_enable));
        }
        if (ear == EAR_RIGHT)
        {
            doHADirectionalityRight = _enable;
            return SendCommandForAllSources(SET_HA_DIRECTIONALITY_ON_RIGHT, Bool2Float(_enable));
        }
        return false;
    }

    /// <summary>
    /// Set HA directionality extend (in dB) for each ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="extendDB"></param>
    public bool SetHADirectionalityExtend(int ear, float extendDB)
    {
        if (ear == EAR_BOTH)
        {
            SetHADirectionalityExtend(EAR_LEFT, extendDB);
            SetHADirectionalityExtend(EAR_RIGHT, extendDB);
        }

        if (ear == EAR_LEFT)
        {
            HADirectionalityExtendLeft = extendDB;
            return SendCommandForAllSources(SET_HA_DIRECTIONALITY_EXTEND_LEFT, extendDB);
        }
        if (ear == EAR_RIGHT)
        {
            HADirectionalityExtendRight = extendDB;
            return SendCommandForAllSources(SET_HA_DIRECTIONALITY_EXTEND_RIGHT, extendDB);
        }
        return false;
    }

    /////////////////////////////////////////////////////////////////////
    // AUXILIARY FUNCTIONS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Send command to plugin to switch on/off write to Debug Log file
    /// </summary>
    public bool SendWriteDebugLog(bool _enable)
    {
        debugLog = _enable;
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
