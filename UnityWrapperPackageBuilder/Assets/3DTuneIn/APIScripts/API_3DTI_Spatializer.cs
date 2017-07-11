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
using API_3DTI_Common;

public class API_3DTI_Spatializer : MonoBehaviour
{
    // LISTENER:
    //public enum TSpatializationMode
    //{
    //    HIGH_QUALITY = 0,
    //    HIGH_PERFORMANCE = 1,
    //    NONE = 2
    //}
    public const int SPATIALIZATION_MODE_HIGH_QUALITY = 0;
    public const int SPATIALIZATION_MODE_HIGH_PERFORMANCE = 1;
    public const int SPATIALIZATION_MODE_NONE = 2;
    public string HRTFFileName = "";                // For internal use, DO NOT USE IT DIRECTLY
    public string ILDNearFieldFileName = "";        // For internal use, DO NOT USE IT DIRECTLY
    public string ILDHighPerformanceFileName = "";  // For internal use, DO NOT USE IT DIRECTLY
    public bool customITDEnabled = false;           // For internal use, DO NOT USE IT DIRECTLY
    public float listenerHeadRadius = 0.0875f;      // For internal use, DO NOT USE IT DIRECTLY    
    //public TSpatializationMode spatializationMode = TSpatializationMode.HIGH_QUALITY;        // For internal use, DO NOT USE IT DIRECTLY
    public int spatializationMode = SPATIALIZATION_MODE_HIGH_QUALITY;   // For internal use, DO NOT USE IT DIRECTLY

    // SOURCE:
    int lastSourceID = 0;                       // For internal use, DO NOT USE IT DIRECTLY

    // ADVANCED:
    public float scaleFactor = 1.0f;            // For internal use, DO NOT USE IT DIRECTLY
    public bool runtimeInterpolateHRTF = true;  // For internal use, DO NOT USE IT DIRECTLY
    public int HRTFstep = 15;                   // For internal use, DO NOT USE IT DIRECTLY
    public bool modFarLPF = true;               // For internal use, DO NOT USE IT DIRECTLY
    public bool modDistAtt = true;              // For internal use, DO NOT USE IT DIRECTLY
    public bool modILD = true;                  // For internal use, DO NOT USE IT DIRECTLY
    public bool modHRTF = true;                 // For internal use, DO NOT USE IT DIRECTLY
    public float magAnechoicAttenuation = -3.0f;    // For internal use, DO NOT USE IT DIRECTLY    
    public float magSoundSpeed = 343.0f;            // For internal use, DO NOT USE IT DIRECTLY
    public bool debugLog = false;                   // For internal use, DO NOT USE IT DIRECTLY    

    // HEARING AID DIRECTIONALITY:
    //public CEarAPIParameter<bool> doHADirectionality = new CEarAPIParameter<bool>(false);
    public bool doHADirectionalityLeft = false;     // For internal use, DO NOT USE IT DIRECTLY    
    public bool doHADirectionalityRight = false;    // For internal use, DO NOT USE IT DIRECTLY    
    public float HADirectionalityExtendLeft = 15.0f;    // For internal use, DO NOT USE IT DIRECTLY
    public float HADirectionalityExtendRight = 15.0f;   // For internal use, DO NOT USE IT DIRECTLY

    // LIMITER
    public bool doLimiter = true;               // For internal use, DO NOT USE IT DIRECTLY

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
    int SET_MOD_HRTF = 9;   // DEPRECATED
    int SET_MAG_ANECHATT = 10;    
    int SET_MAG_SOUNDSPEED = 11;
    int LOAD_3DTI_ILD_NEARFIELD = 12;    
    int SET_DEBUG_LOG = 13;
    int SET_HA_DIRECTIONALITY_EXTEND_LEFT = 14;
    int SET_HA_DIRECTIONALITY_EXTEND_RIGHT = 15;
    int SET_HA_DIRECTIONALITY_ON_LEFT = 16;
    int SET_HA_DIRECTIONALITY_ON_RIGHT = 17;
    int SET_LIMITER_ON = 18;
    int GET_LIMITER_COMPRESSION = 19;
    int GET_IS_CORE_READY = 20;
    int SET_HRTF_STEP = 21;
    int LOAD_3DTI_ILD_HIGHPERFORMANCE = 22; // NEW
    int SET_SPATIALIZATION_MODE = 23;     // NEW

    // Hack for modifying one single AudioSource (TO DO: fix this)
    bool selectSource = false;
    AudioSource selectedSource;

    // For high performance / High quality modes, variables to check which resources have been loaded
    bool HighQualityModeHRTFLoaded = false;
    bool HighQualityModeILDLoaded = false;
    bool HighPerformanceModeILDLoaded = false;

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

            // Check if core is already started
            bool isReady;
            if (!GetBoolParameter(GET_IS_CORE_READY, out isReady))
                return false;
            else
            {
                if (isReady)
                    return false;
            }
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

        // Hearing Aid directionality setup:
        if (!SetupHADirectionality()) return false;

        // Limiter setup:
        if (!SetupLimiter()) return false;

        // Listener setup:
        if (!SetupListener()) return false;

        // Go back to default state, affecting all sources
        selectSource = false;

        return true;
    }

    /// <summary>
    /// Enable all binaural spatialization processes for one (or all) source/s
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public bool EnableSpatialization(AudioSource source=null)
    {
        if (source != null)
        {
            selectSource = true;
            selectedSource = source;
        }

        //if (!SetModHRTF(true)) return false;
        if (!SetModILD(true)) return false;
        if (!SetModFarLPF(true)) return false;
        return SetModDistanceAttenuation(true);
    }

    /// <summary>
    /// Enable all binaural spatialization processes for one (or all) source/s
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public bool DisableSpatialization(AudioSource source = null)
    {
        if (source != null)
        {
            selectSource = true;
            selectedSource = source;
        }

        

        //if (!SetModHRTF(false)) return false;
        if (!SetModILD(false)) return false;
        if (!SetModFarLPF(false)) return false;
        return SetModDistanceAttenuation(false);
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

        // HIGH QUALITY MODE (default)
        //if (spatializationMode == TSpatializationMode.HIGH_QUALITY)
        if (spatializationMode == SPATIALIZATION_MODE_HIGH_QUALITY)
        {
            if (!HighQualityModeILDLoaded)
            {
                if (!ILDNearFieldFileName.Equals(""))
                {
                    #if (!UNITY_EDITOR)
                    if (SaveResourceAsBinary(ILDNearFieldFileName, ".3dti-ild", out ILDNearFieldFileName) != 1) return false;
                    #endif
                    if (!LoadILDNearFieldBinary(ILDNearFieldFileName)) return false;
                }
            }
            if (!HighQualityModeHRTFLoaded)
            { 
                if (!HRTFFileName.Equals(""))
                {
                    #if (!UNITY_EDITOR)
                    if (SaveResourceAsBinary(HRTFFileName, ".3dti-hrtf", out HRTFFileName) != 1) return false; 
                    #endif
                    if (!LoadHRTFBinary(HRTFFileName)) return false;
                }              
            }
        }

        // HIGH PERFORMANCE MODE
        //if (spatializationMode == TSpatializationMode.HIGH_PERFORMANCE)
        if (spatializationMode == SPATIALIZATION_MODE_HIGH_PERFORMANCE)
        {
            if (!HighPerformanceModeILDLoaded)
            {
                if (!ILDHighPerformanceFileName.Equals(""))
                {
                    #if (!UNITY_EDITOR)
                    if (SaveResourceAsBinary(ILDHighPerformanceFileName, ".3dti-ild", out ILDHighPerformanceFileName) != 1) return false;
                    #endif
                    if (!LoadILDHighPerformanceBinary(ILDHighPerformanceFileName)) return false;
                }                
            }
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
        return SendCommandForAllSources(SET_CUSTOM_ITD, CommonFunctions.Bool2Float(_enable));
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

        HighQualityModeHRTFLoaded = true;
        return true;
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Load ILD Near Field from a binary .3dti file
    /// </summary>
    public bool LoadILDNearFieldBinary(string filename)
    {
        ILDNearFieldFileName = filename;

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
            source.SetSpatializerFloat(LOAD_3DTI_ILD_NEARFIELD, (float)filename.Length);            
            for (int i = 0; i < filename.Length; i++)
            {
                int chr2Int = (int)filename[i];
                float chr2Float = (float)chr2Int;
                if (!source.SetSpatializerFloat(LOAD_3DTI_ILD_NEARFIELD, chr2Float)) return false;
            }            
        }

        HighQualityModeILDLoaded = true;
        return true;
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Load ILD High Performance from a binary .3dti file
    /// </summary>
    public bool LoadILDHighPerformanceBinary(string filename)
    {
        ILDHighPerformanceFileName = filename;

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
            source.SetSpatializerFloat(LOAD_3DTI_ILD_HIGHPERFORMANCE, (float)filename.Length);
            for (int i = 0; i < filename.Length; i++)
            {
                int chr2Int = (int)filename[i];
                float chr2Float = (float)chr2Int;
                if (!source.SetSpatializerFloat(LOAD_3DTI_ILD_HIGHPERFORMANCE, chr2Float)) return false;
            }
        }

        HighPerformanceModeILDLoaded = true;
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
        if (!SetSourceInterpolation(runtimeInterpolateHRTF)) return false;
        return SetHRTFResamplingStep(HRTFstep);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set source HRTF interpolation method
    /// </summary>
    public bool SetSourceInterpolation(bool _run)
    {
        runtimeInterpolateHRTF = _run;
        return SendCommandForAllSources(SET_HRTF_INTERPOLATION, CommonFunctions.Bool2Float(_run));     
    }

    /// <summary>
    /// Set HRTF resampling step
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    public bool SetHRTFResamplingStep(int step)
    {
        HRTFstep = step;
        return SendCommandForAllSources(SET_HRTF_STEP, (float)step);
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
        //if (!SetModHRTF(modHRTF)) return false;
        if (!SetSpatializationMode(spatializationMode)) return false;      
        return true;
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off far distance LPF
    /// </summary>        
    public bool SetModFarLPF(bool _enable)
    {
        modFarLPF = _enable;
        return SendCommandForAllSources(SET_MOD_FARLPF, CommonFunctions.Bool2Float(_enable));
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off distance attenuation
    /// </summary>        
    public bool SetModDistanceAttenuation(bool _enable)
    {
        modDistAtt = _enable;
        return SendCommandForAllSources(SET_MOD_DISTATT, CommonFunctions.Bool2Float(_enable));
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off near distance ILD
    /// </summary>        
    public bool SetModILD(bool _enable)
    {
        modILD = _enable;
        return SendCommandForAllSources(SET_MOD_ILD, CommonFunctions.Bool2Float(_enable));
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set spatialization mode (high quality, high performance, or none)
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public bool SetSpatializationMode(int mode)
    {
        spatializationMode = mode;

        // Load resources
        SetupListener();

        // Send command to plugin
        return SendCommandForAllSources(SET_SPATIALIZATION_MODE, (float)(mode));
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off HRTF convolution
    /// </summary>        
    //public bool SetModHRTF(bool _enable)
    //{
    //    modHRTF = _enable;
    //    return SendCommandForAllSources(SET_MOD_HRTF, CommonFunctions.Bool2Float(_enable));
    //}

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
    // HA DIRECTIONALITY METHODS
    /////////////////////////////////////////////////////////////////////
    
    /// <summary>
    ///  Initial setup of HA directionality
    /// </summary>
    public bool SetupHADirectionality()
    {
        //if (!SwitchOnOffHADirectionality(T_ear.LEFT, doHADirectionality.Get(T_ear.LEFT))) return false;
        //if (!SwitchOnOffHADirectionality(T_ear.RIGHT, doHADirectionality.Get(T_ear.RIGHT))) return false;
        if (!SwitchOnOffHADirectionality(T_ear.LEFT, doHADirectionalityLeft)) return false;
        if (!SwitchOnOffHADirectionality(T_ear.RIGHT, doHADirectionalityRight)) return false;
        if (!SetHADirectionalityExtend(T_ear.LEFT, HADirectionalityExtendLeft)) return false;
        if (!SetHADirectionalityExtend(T_ear.RIGHT, HADirectionalityExtendRight)) return false;
        return true;
    }

    /// <summary>
    /// Switch on/off HA directionality for each ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="_enable"></param>
    public bool SwitchOnOffHADirectionality(T_ear ear, bool _enable)
    {
        if (ear == T_ear.BOTH)
        {
            SwitchOnOffHADirectionality(T_ear.LEFT, _enable);
            SwitchOnOffHADirectionality(T_ear.RIGHT, _enable);
        }

        if (ear == T_ear.LEFT)
        {
            //doHADirectionality.Set(T_ear.LEFT, _enable);
            doHADirectionalityLeft = _enable;
            return SendCommandForAllSources(SET_HA_DIRECTIONALITY_ON_LEFT, CommonFunctions.Bool2Float(_enable));
        }
        if (ear == T_ear.RIGHT)
        {
            //doHADirectionality.Set(T_ear.RIGHT, _enable);
            doHADirectionalityRight = _enable;
            return SendCommandForAllSources(SET_HA_DIRECTIONALITY_ON_RIGHT, CommonFunctions.Bool2Float(_enable));
        }
        return false;
    }

    /// <summary>
    /// Set HA directionality extend (in dB) for each ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="extendDB"></param>
    public bool SetHADirectionalityExtend(T_ear ear, float extendDB)
    {
        if (ear == T_ear.BOTH)
        {
            SetHADirectionalityExtend(T_ear.LEFT, extendDB);
            SetHADirectionalityExtend(T_ear.RIGHT, extendDB);
        }

        if (ear == T_ear.LEFT)
        {
            HADirectionalityExtendLeft = extendDB;
            return SendCommandForAllSources(SET_HA_DIRECTIONALITY_EXTEND_LEFT, extendDB);
        }
        if (ear == T_ear.RIGHT)
        {
            HADirectionalityExtendRight = extendDB;
            return SendCommandForAllSources(SET_HA_DIRECTIONALITY_EXTEND_RIGHT, extendDB);
        }
        return false;
    }

    /////////////////////////////////////////////////////////////////////
    // LIMITER METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    ///  Initial setup of limiter
    /// </summary>
    public bool SetupLimiter()
    {
        if (!SwitchOnOffLimiter(doLimiter)) return false;
        return true;
    }

    /// <summary>
    /// Switc on/off limiter after spatialization process
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SwitchOnOffLimiter(bool _enable)
    {
        doLimiter = _enable;
        return SendCommandForAllSources(SET_LIMITER_ON, CommonFunctions.Bool2Float(_enable));
    }

    /// <summary>
    /// Get state of limiter (currently compressing or not)
    /// </summary>
    /// <param name="_compressing"></param>
    /// <returns></returns>
    public bool GetLimiterCompression(out bool _compressing)
    {
        return GetBoolParameter(GET_LIMITER_COMPRESSION, out _compressing);        
    }

    /////////////////////////////////////////////////////////////////////
    // AUXILIARY FUNCTIONS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Get the value of one bool parameter from the first instance of the spatialization plugin
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool GetBoolParameter(int parameter, out bool value)
    {
        value = false;
        float floatValue;
        if (!GetFloatParameter(parameter, out floatValue)) return false;
        value = CommonFunctions.Float2Bool(floatValue);
        return true;
    }

    /// <summary>
    /// Get the value of one float parameter from the first instance of the spatialization plugin 
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool GetFloatParameter(int parameter, out float value)
    {
        value = 0.0f;

        // We will get value from the first spatialized source
        AudioSource source;
        List<AudioSource> sources = GetAllSpatializedSources();
        if (sources.Count > 0)
            source = sources[0];
        else
            return false;

        // Send the command to get the value        
        return (source.GetSpatializerFloat(parameter, out value));
    }

    /// <summary>
    /// Send command to plugin to switch on/off write to Debug Log file
    /// </summary>
    public bool SendWriteDebugLog(bool _enable)
    {
        debugLog = _enable;
        return SendCommandForAllSources(SET_DEBUG_LOG, CommonFunctions.Bool2Float(_enable));
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

}
