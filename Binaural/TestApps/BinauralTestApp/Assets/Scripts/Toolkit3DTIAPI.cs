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


public class Toolkit3DTIAPI : MonoBehaviour
{
    // LISTENER:
    public string HRTFFileName = "";            // Used by Inspector
    public string ILDFileName = "";             // Used by Inspector
    public bool customITDEnabled = false;       // Used by Inspector
    public float listenerHeadRadius = 0.0875f;  // Used by Inspector
    FileStream streamHRTFFile;
    FileStream streamILDFile;

    // SOURCE:
    public bool runtimeInterpolateHRTF = true;  // Used by Inspector

    // ADVANCED:
    public float scaleFactor = 1.0f;            // Used by Inspector
    public bool modFarLPF = true;               // Used by Inspector
    public bool modDistAtt = true;              // Used by Inspector
    public bool modILD = true;                  // Used by Inspector
    public bool modHRTF = true;                 // Used by Inspector
    public float magAnechoicAttenuation = -6.0f;    // Used by Inspector
    public float magReverbAttenuation = -3.0f;      // Used by Inspector
    public float magSoundSpeed = 343.0f;            // Used by Inspector
    
    // Definition of spatializer plugin commands
    public int LOAD_3DTI_HRTF = 0;
    public int SET_HEAD_RADIUS = 1;
    public int SET_SCALE_FACTOR = 2;
    public int SET_SOURCE_ID = 3;    
    public int SET_CUSTOM_ITD = 4;
    public int SET_HRTF_INTERPOLATION = 5;
    public int SET_MOD_FARLPF = 6;
    public int SET_MOD_DISTATT = 7;
    public int SET_MOD_ILD = 8;
    public int SET_MOD_HRTF = 9;
    public int SET_MAG_ANECHATT = 10;
    public int SET_MAG_REVERBATT = 11;
    public int SET_MAG_SOUNDSPEED = 12;
    public int LOAD_3DTI_ILD = 13;
    //public int GET_LOAD_RESULT = 14;

    // STRING OUTPUT FOR DEBUG
    //public int STR_LENGTH = 15;
    //public int STR_LAST_ID = 16;
    //public int STR_START = 17;
    //public int last_id = -1;

    // GET FLOAT FOR DEBUG
    //public int RESULT_LOAD_WAITING = 0;
    //public int RESULT_LOAD_OK = 1;
    //public int RESULT_LOAD_BADHANDLE = -1;
    //public int RESULT_LOAD_WRONGDATA = -2;
    //public int last_load_result = 5;

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Automatic setup of Toolkit Core (as read from custom GUI in Unity Inspector)
    /// </summary>
    void Start ()
    {        
        // Global setup:
        //SetScaleFactor(scaleFactor);
        SendSourceIDs();    

        // Setup modules enabler:
        SetupModulesEnabler();

        // Source setup:
        SetupSource();

        // Listener setup:
        SetupListener();
    }

    /// <summary>
    /// STRING OUTPUT FOR DEBUG
    /// </summary>
    void Update()
    {
        //WriteOutputString();        
        //DebugWrite("NUMBER IS: " + GetFloatScript().ToString()); 
        //ShowLoadResult();
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
            #if (UNITY_ANDROID && !UNITY_EDITOR)
                SaveResourceAsBinary(ILDFileName, ".ild", out ILDFileName); // TODO : ".3dti-ild"                
            #endif
            if (LoadILDBinary(ILDFileName) == System.IntPtr.Zero)
                DebugWrite("ERROR! Could not load ILD file: " + ILDFileName);
            else
                DebugWrite("ILD file loaded OK!!");
        }        

        if (!HRTFFileName.Equals(""))
        {
            #if (UNITY_ANDROID && !UNITY_EDITOR)
                SaveResourceAsBinary(HRTFFileName, ".hrt", out HRTFFileName); // TODO : ".3dti-hrtf"                
            #endif
            if (LoadHRTFBinary(HRTFFileName) == System.IntPtr.Zero)
                DebugWrite("ERROR! Could not load HRTF file: " + HRTFFileName);
            else
                DebugWrite("HRTF file loaded OK!!");
        }
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
    public System.IntPtr LoadHRTFBinary(string filename)
    {        
        List<AudioSource> audioSources = GetAllSpatializedSources();

        System.IntPtr fileHandle = System.IntPtr.Zero;

        foreach (AudioSource source in audioSources)
        {
            streamHRTFFile = new FileStream(filename, FileMode.Open, FileAccess.Read);
            try
            {
                //bool bSuccess = false;
                //streamHRTFFile.SafeFileHandle.DangerousAddRef(ref bSuccess);    // Increment the ref counter of the safe handle to avoid that the system makes the handle stale
                //if (!bSuccess)
                //    return System.IntPtr.Zero;

                // Get handle (no way other than dangerously)
                fileHandle = streamHRTFFile.SafeFileHandle.DangerousGetHandle();

                if (fileHandle == System.IntPtr.Zero)
                    return fileHandle;

                // Cast handle to float, and pass it to all audio sources
                float floatHandle = (float)fileHandle;

                source.SetSpatializerFloat(LOAD_3DTI_HRTF, floatHandle);

                //streamHRTFFile.SafeFileHandle.DangerousRelease();   // Decrement ref counter after use by this source                
            }
            finally
            {
                //streamHRTFFile.Close();
            }
        }

        // Return file handle            
        return fileHandle;
    }

    /////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Load ILD from a binary .3dti file
    /// </summary>
    public System.IntPtr LoadILDBinary(string filename)
    {
        List<AudioSource> audioSources = GetAllSpatializedSources();

        System.IntPtr fileHandle = System.IntPtr.Zero;

        foreach (AudioSource source in audioSources)
        {
            streamILDFile = new FileStream(filename, FileMode.Open, FileAccess.Read);
            try
            {
                //bool bSuccess = false;
                //streamHRTFFile.SafeFileHandle.DangerousAddRef(ref bSuccess);    // Increment the ref counter of the safe handle to avoid that the system makes the handle stale
                //if (!bSuccess)
                //    return System.IntPtr.Zero;

                // Get handle (no way other than dangerously)
                fileHandle = streamILDFile.SafeFileHandle.DangerousGetHandle();

                if (fileHandle == System.IntPtr.Zero)
                    return fileHandle;

                // Cast handle to float, and pass it to all audio sources
                float floatHandle = (float)fileHandle;

                source.SetSpatializerFloat(LOAD_3DTI_ILD, floatHandle);

                //streamHRTFFile.SafeFileHandle.DangerousRelease();   // Decrement ref counter after use by this source                
            }
            finally
            {
                //streamHRTFFile.Close();
            }
        }

        // Return file handle            
        return fileHandle;
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
    /// Set magnitude Reverb Attenuation
    /// </summary>    
    public void SetMagnitudeReverbAttenuation(float value)
    {
        SendCommandForAllSources(SET_MAG_REVERBATT, value);
    }

    /// <summary>
    /// Set magnitude Sound Speed
    /// </summary>    
    public void SetMagnitudeSoundSpeed(float value)
    {
        SendCommandForAllSources(SET_MAG_SOUNDSPEED, value);
    }


    /////////////////////////////////////////////////////////////////////
    // METHODS FOR INTERNAL USE BY THE API
    /////////////////////////////////////////////////////////////////////

    ///// <summary>
    ///// STRING OUTPUT FOR DEBUG
    ///// </summary>    
    //[StructLayout(LayoutKind.Explicit)]
    //public struct TPack
    //{
    //    [FieldOffset(0)]
    //    public UInt32 i;
    //    [FieldOffset(0)]
    //    public float f;
    //}
    //public void WriteOutputString()
    //{        
    //    float newfid = GetFloatFromFirstSource(STR_LAST_ID);
    //    int newid;

    //    DebugWrite("ID Is: " + newfid.ToString());

    //    if (float.IsNaN(newfid))
    //    {
    //        DebugWrite("ERROR! Unable to read last string ID");
    //        return;
    //    }
    //    else
    //        newid = (int)newfid;
        
    //    if (newid != last_id)
    //    {            
    //        last_id = newid;
    //        int strlength = (int)GetFloatFromFirstSource(STR_LENGTH);
    //        int strparam = STR_START;
    //        string newString = "";
    //        for (int i=0; i < strlength/4; i++)
    //        {
    //            TPack pack;
    //            pack.i = 0; // To avoid compilation problems
    //            pack.f = GetFloatFromFirstSource(strparam);
    //            UInt32 packrest = pack.i;
    //            for (int c = 0; c < 4; c++)
    //            {
    //                char newChar = (char)(byte)(packrest % 256);
    //                packrest = packrest >> 8;
    //                newString += newChar;
    //            }
    //            strparam++;
    //        }
    //        DebugWrite(newString);
    //    }
    //}

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
    /// For debug, show changes in LOAD_RESULT parameter of plugin
    /// </summary>
    //public void ShowLoadResult()
    //{
    //    float fresult = GetFloatFromFirstSource(GET_LOAD_RESULT);
    //    int result = (int)fresult;

    //    if (result != last_load_result)
    //    {
    //        last_load_result = result;
    //        if (result == RESULT_LOAD_WAITING)
    //            DebugWrite("FILE LOAD: Waiting...");
    //        if (result == RESULT_LOAD_OK)
    //            DebugWrite("FILE LOAD: OK!");
    //        if (result == RESULT_LOAD_BADHANDLE)
    //            DebugWrite("FILE LOAD: ERROR! Invalid file handle");
    //        if (result == RESULT_LOAD_WRONGDATA)
    //            DebugWrite("FILE LOAD: ERROR! Wrong data read");
    //    }
    //}

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
    /// Get float parameter from the dll, from first registered source
    /// </summary>
    public float GetFloatFromFirstSource(int floatparam)
    {
        List<AudioSource> audioSources = GetAllSpatializedSources();        
        if (audioSources.Count != 0)
        {
            float value;
            if (!audioSources[0].GetSpatializerFloat(floatparam, out value))
                return float.NaN;
            return value;
        }        
        return float.NaN;
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
