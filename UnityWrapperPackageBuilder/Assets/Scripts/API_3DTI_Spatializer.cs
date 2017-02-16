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

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Automatic setup of Toolkit Core (as read from custom GUI in Unity Inspector)
    /// </summary>
    void Start ()
    {
        // Debug log:
        SendWriteDebugLog(debugLog);

        // Global setup:
        SetScaleFactor(scaleFactor);
        SendSourceIDs();    

        // Setup modules enabler:
        SetupModulesEnabler();

        // Source setup:
        SetupSource();

        // Listener setup:
        SetupListener();

        // Hearing Aid directionality setup:
        SetupHADirectionality();
    }

    /////////////////////////////////////////////////////////////////////
    // LISTENER METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Setup all listener parameters
    /// </summary>
    public void SetupListener()
    {
        SetHeadRadius(listenerHeadRadius);
        SetCustomITD(customITDEnabled);

        if (!ILDFileName.Equals(""))
        {
            //#if (UNITY_ANDROID && !UNITY_EDITOR)
                SaveResourceAsBinary(ILDFileName, ".3dti-ild", out ILDFileName);                 
            //#endif
			//#if (!UNITY_EDITOR)
			//GetStreamingAsset(ILDFileName, out ILDFileName);
			//#endif
            LoadILDBinary(ILDFileName);
        }        

        if (!HRTFFileName.Equals(""))
        {
            //#if (UNITY_ANDROID && !UNITY_EDITOR)
                SaveResourceAsBinary(HRTFFileName, ".3dti-hrtf", out HRTFFileName); 
            //#endif
			//#if (!UNITY_EDITOR)
			//GetStreamingAsset(HRTFFileName, out HRTFFileName);
			//#endif
            LoadHRTFBinary(HRTFFileName);
        }
    }

	public int GetStreamingAsset(string originalname, out string filename)
	{
		// Get file name from full path
		string namewithoutpath = Path.GetFileName(originalname);

		// Setup name for new file
		string dataPath = Application.persistentDataPath;
		string newfilename = dataPath + "/StreamingAssets/" + namewithoutpath;
		filename = newfilename;
		return 1;
	}

    /// <summary>
    /// Load one file from resources and save it as a binary file (for Android)
    /// </summary>    
    public int SaveResourceAsBinary(string originalname, string extension, out string filename)
    {
        DebugWrite("Storing resource as binary. Original name=" + originalname + ". Extension=" + extension);

        // Get only file name from full path
        string namewithoutextension = Path.GetFileNameWithoutExtension(originalname);
        DebugWrite("Name without extension is: " + namewithoutextension);

        // Setup name for new file
        string dataPath = Application.persistentDataPath;
        string newfilename = dataPath + "/" + namewithoutextension + extension;
        filename = newfilename;
        DebugWrite("File name in Android device will be: " + filename);

        // Load as asset from resources 
        TextAsset txtAsset = Resources.Load(namewithoutextension) as TextAsset;
        if (txtAsset == null)
        {
            DebugWrite("ERROR! Could not load asset from resources");
            return -1;  // Could not load asset from resources
        }
        DebugWrite("Asset " + namewithoutextension + ".bytes loaded from resources succesfully");

        // Transform asset into stream and then into byte array
        MemoryStream streamData = new MemoryStream(txtAsset.bytes);
        byte[] dataArray = streamData.ToArray();

        // Write binary data to binary file        
        using (BinaryWriter writer = new BinaryWriter(File.Open(newfilename, FileMode.Create)))
        {
            writer.Write(dataArray);
        }

        DebugWrite("File created OK");
        return 1;
    }

    /// <summary>
    /// Set listener head radius
    /// </summary>
    public void SetHeadRadius(float headRadius)
    {
        listenerHeadRadius = headRadius;
        SendCommandForAllSources(SET_HEAD_RADIUS, headRadius);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set custom ITD enabled/disabled
    /// </summary>
    public void SetCustomITD(bool _enable)
    {
        if (_enable)
            SendCommandForAllSources(SET_CUSTOM_ITD, 1.0f);
        else
            SendCommandForAllSources(SET_CUSTOM_ITD, 0.0f);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Load HRTF from a binary .3dti file
    /// </summary>
    public void LoadHRTFBinary(string filename)
    {
        List<AudioSource> audioSources = GetAllSpatializedSources();        

        foreach (AudioSource source in audioSources)
        {
            source.SetSpatializerFloat(LOAD_3DTI_HRTF, (float)filename.Length);
            for (int i = 0; i < filename.Length; i++)
            {
                int chr2Int = (int)filename[i];
                float chr2Float = (float)chr2Int;
                source.SetSpatializerFloat(LOAD_3DTI_HRTF, chr2Float);
            }
            //source.SetSpatializerFloat(LOAD_3DTI_HRTF, 0f);
        }
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Load ILD from a binary .3dti file
    /// </summary>
    public void LoadILDBinary(string filename)
    {
        List<AudioSource> audioSources = GetAllSpatializedSources();        

        foreach (AudioSource source in audioSources)
        {
            source.SetSpatializerFloat(LOAD_3DTI_ILD, (float)filename.Length);            
            for (int i = 0; i < filename.Length; i++)
            {
                int chr2Int = (int)filename[i];
                float chr2Float = (float)chr2Int;
                source.SetSpatializerFloat(LOAD_3DTI_ILD, chr2Float);                
            }
            //source.SetSpatializerFloat(LOAD_3DTI_HRTF, 0f);
        }
    }


    /////////////////////////////////////////////////////////////////////
    // SOURCE API METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Setup all source parameters
    /// </summary>        
    public void SetupSource()
    {
        SetSourceInterpolation(runtimeInterpolateHRTF);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set source HRTF interpolation method
    /// </summary>
    public void SetSourceInterpolation(bool _run)
    {
        int value = 1;
        if (!_run)
            value = 0;
        SendCommandForAllSources(SET_HRTF_INTERPOLATION, (float)value);
    }


    /////////////////////////////////////////////////////////////////////
    // ADVANCED API METHODS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set ID for all sources, for internal use of the wrapper
    /// </summary>
    public void SendSourceIDs()
    {
        List<AudioSource> audioSources = GetAllSpatializedSources();
        int i = 1;
        foreach (AudioSource source in audioSources)
        {
            source.SetSpatializerFloat(SET_SOURCE_ID, (float)i++);
        }
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Set scale factor. Allows the toolkit to work with big-scale or small-scale scenarios
    /// </summary>
    public void SetScaleFactor (float scale)
    {
        SendCommandForAllSources(SET_SCALE_FACTOR, scale);
    }

    /// <summary>
    ///  Setup modules enabler, allowing to switch on/off core features
    /// </summary>
    public void SetupModulesEnabler()
    {
        SetModFarLPF(modFarLPF);
        SetModDistanceAttenuation(modDistAtt);
        SetModILD(modILD);
        SetModHRTF(modHRTF);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off far distance LPF
    /// </summary>        
    public void SetModFarLPF(bool _enable)
    {
        if (_enable)
            SendCommandForAllSources(SET_MOD_FARLPF, 1.0f);
        else
            SendCommandForAllSources(SET_MOD_FARLPF, 0.0f);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off distance attenuation
    /// </summary>        
    public void SetModDistanceAttenuation(bool _enable)
    {
        if (_enable)
            SendCommandForAllSources(SET_MOD_DISTATT, 1.0f);
        else
            SendCommandForAllSources(SET_MOD_DISTATT, 0.0f);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off near distance ILD
    /// </summary>        
    public void SetModILD(bool _enable)
    {
        if (_enable)
            SendCommandForAllSources(SET_MOD_ILD, 1.0f);
        else
            SendCommandForAllSources(SET_MOD_ILD, 0.0f);
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off HRTF convolution
    /// </summary>        
    public void SetModHRTF(bool _enable)
    {
        if (_enable)
            SendCommandForAllSources(SET_MOD_HRTF, 1.0f);
        else
            SendCommandForAllSources(SET_MOD_HRTF, 0.0f);        
    }

    /// <summary>
    /// Set magnitude Anechoic Attenuation
    /// </summary>    
    public void SetMagnitudeAnechoicAttenuation(float value)
    {
        SendCommandForAllSources(SET_MAG_ANECHATT, value);
    }

    /// <summary>
    /// Set magnitude Sound Speed
    /// </summary>    
    public void SetMagnitudeSoundSpeed(float value)
    {
        SendCommandForAllSources(SET_MAG_SOUNDSPEED, value);
    }

    /////////////////////////////////////////////////////////////////////
    // SOURCE API METHODS
    /////////////////////////////////////////////////////////////////////
    
    /// <summary>
    ///  Initial setup of HA directionality
    /// </summary>
    public void SetupHADirectionality()
    {
        SwitchOnOffHADirectionality(EAR_LEFT, doHADirectionalityLeft);
        SwitchOnOffHADirectionality(EAR_RIGHT, doHADirectionalityRight);
        SetHADirectionalityExtend(EAR_LEFT, HADirectionalityExtendLeft);
        SetHADirectionalityExtend(EAR_RIGHT, HADirectionalityExtendRight);
    }

    /// <summary>
    /// Switch on/off HA directionality for each ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="_enable"></param>
    public void SwitchOnOffHADirectionality(int ear, bool _enable)
    {
        if (ear == EAR_BOTH)
        {
            SwitchOnOffHADirectionality(EAR_LEFT, _enable);
            SwitchOnOffHADirectionality(EAR_RIGHT, _enable);
        }

        if (ear == EAR_LEFT)
            SendCommandForAllSources(SET_HA_DIRECTIONALITY_ON_LEFT, Bool2Float(_enable));
        if (ear == EAR_RIGHT)
            SendCommandForAllSources(SET_HA_DIRECTIONALITY_ON_RIGHT, Bool2Float(_enable));
    }

    /// <summary>
    /// Set HA directionality extend (in dB) for each ear
    /// </summary>
    /// <param name="ear"></param>
    /// <param name="extendDB"></param>
    public void SetHADirectionalityExtend(int ear, float extendDB)
    {
        if (ear == EAR_BOTH)
        {
            SetHADirectionalityExtend(EAR_LEFT, extendDB);
            SetHADirectionalityExtend(EAR_RIGHT, extendDB);
        }

        if (ear == EAR_LEFT)
            SendCommandForAllSources(SET_HA_DIRECTIONALITY_EXTEND_LEFT, extendDB);
        if (ear == EAR_RIGHT)
            SendCommandForAllSources(SET_HA_DIRECTIONALITY_EXTEND_RIGHT, extendDB);
    }

    /////////////////////////////////////////////////////////////////////
    // AUXILIARY FUNCTIONS
    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Send command to plugin to switch on/off write to Debug Log file
    /// </summary>
    public void SendWriteDebugLog(bool _enable)
    {
        if (_enable)
            SendCommandForAllSources(SET_DEBUG_LOG, 1.0f);
        else
            SendCommandForAllSources(SET_DEBUG_LOG, 0.0f);
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

    /// <summary>
    /// Send command to the DLL, for each registered source
    /// </summary>
    public void SendCommandForAllSources(int command, float value)
    {
        List<AudioSource> audioSources = GetAllSpatializedSources();
        foreach (AudioSource source in audioSources)
            source.SetSpatializerFloat(command, value);
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
