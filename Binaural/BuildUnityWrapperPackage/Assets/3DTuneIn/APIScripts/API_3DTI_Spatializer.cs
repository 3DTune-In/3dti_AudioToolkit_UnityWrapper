using API_3DTI_Common;
using System;
using System.Collections.Generic;
using System.IO;            // Needed for FileStream
using System.Runtime.InteropServices;
using UnityEngine.Audio;

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
using System.Linq;

public enum TSampleRateEnum
{
	K44, K48, K96
};

[System.AttributeUsage(System.AttributeTargets.Field)]
public class SpatializerParameterAttribute : System.Attribute
{
    public Type type = typeof(float);
    // Label used in GUI
    public string label;
	// Tooltip for GUI
	public string description;
    // For numeric values, the units label, e.g. "dB"
    public string units;
    // For int/float parameters: limit to these discrete values. Leave as null for no limits.
    public float[] validValues;
	public float min;
	public float max;
	public float defaultValue;
	// If true then this parameter may be set individually on a specific source
	public bool isSourceParameter = false;
}


//[System.AttributeUsage(System.AttributeTargets.Field)]
//// TODO: Remove this and just use SpatializerParameterAttribute
//public class SpatializerSourceParameterAttribute : System.Attribute
//{
//	// The name used to set using setSpatializerFloat
//	public string pluginName;
//    public Type type = typeof(float);
//    // Tooltip for GUI
//    public string description;
//    // Label used in GUI
//    public string label;
//    // For numeric values, the units label, e.g. "dB"
//    public string units;
//    // For int/float parameters: limit to these discrete values. Leave as null for no limits.
//    public float[] validValues;
//    public float min;
//    public float max;
//    public float defaultValue;
//}


public class API_3DTI_Spatializer : MonoBehaviour
{

	// Set this to the 3DTI mixer containing the SpatializerCore3DTI effect.
	public AudioMixer spatializereCoreMixer;

	// Note: The numbering of these parameters must be kept in sync with the C++ plugin source code. Per-source parameters must appear first for compatibility with the plugin.
	// The int value of these enums may change in future versions. For compatibility, always use the enum value name rather than the int value (i.e. use SptaializerParameter.PARAM_HRTF_INTERPOLATION instead of 0).
	public enum SpatializerParameter
	{
		[SpatializerParameter(/*pluginName="HRTFInterp",*/ label = "Enable HRTF interpolation", description = "Enable runtime interpolation of HRIRs, to allow for smoother transitions when moving listener and/or sources", min = 0, max = 1, type = typeof(bool), defaultValue = 1.0f, isSourceParameter = true)]
		PARAM_HRTF_INTERPOLATION = 0,

		[SpatializerParameter(/*pluginName = "MODfarLPF",*/ label = "Enable far distance LPF", description = "Enable low pass filter to simulate sound coming from far distances", min = 0, max = 1, type = typeof(bool), defaultValue = 1.0f, isSourceParameter = true)]
		PARAM_MOD_FARLPF = 1,

		[SpatializerParameter(/*pluginName = "MODDistAtt",*/ label = "Enable distance attenuation", description = "Enable attenuation of sound depending on distance to listener", min = 0, max = 1, type = typeof(bool), defaultValue = 1.0f, isSourceParameter = true)]
		PARAM_MOD_DISTATT = 2,

		[SpatializerParameter(/*pluginName = "MODNFILD",*/ label = "Enable near distance ILD", description = "Enable near field filter for sources very close to the listener. High quality only. Depends on th" +
			"e High Quality ILD binary being loaded.", min = 0, max = 1, type = typeof(bool), defaultValue = 1.0f, isSourceParameter = true)]
		PARAM_MOD_NEAR_FIELD_ILD = 3,

		[SpatializerParameter(/*pluginName = "SpatMode",*/ label = "Set spatialization mode (0=High quality, 1=High performance, 2=None)", description = "Set spatialization mode (0=High quality, 1=High performance, 2=None). Note, High quality depends on the HRTF binary being loaded and High Performance depends on the High Performance ILD binary being loaded.", min = 0, max = 2, type = typeof(SpatializationMode), defaultValue = 0.0f, isSourceParameter = true)]
		PARAM_SPATIALIZATION_MODE = 4,

		[SpatializerParameter(label = "Head radius", description = "Set listener head radius", units = "m", min = 0.0f, max = 1e20f, defaultValue = 0.0875f)]
		PARAM_HEAD_RADIUS = 5,

		// TODO: Add remaining default values
		[SpatializerParameter(label = "Scale factor", description = "Set the proportion between metres and Unity scale units", min = 1e-20f, max = 1e20f, defaultValue = 1.0f)]
		PARAM_SCALE_FACTOR = 6,

		[SpatializerParameter(label = "Enable custom ITD", description = "Enable Interaural Time Difference customization", type = typeof(bool), defaultValue = 0.0f)]
		PARAM_CUSTOM_ITD = 7,

		[SpatializerParameter(label = "Anechoic distance attenuation", description = "Set attenuation in dB for each double distance", min = -30.0f, max = 0.0f, units="dB", defaultValue = -1.0f)]
		PARAM_MAG_ANECHATT = 8,

		[SpatializerParameter(label = "Sound speed", description = "Set sound speed, used for custom ITD computation", units = "m/s", min = 10.0f, max = 1000.0f, defaultValue = 343.0f)]
		PARAM_MAG_SOUNDSPEED = 9,

		[SpatializerParameter(label = "Anechoic directionality attenuation for left ear", description = "Set directionality extend for left ear. The value is the attenuation in decibels applied to sources placed behind the listener", units = "dB", min = 0.0f, max = 30.0f, defaultValue =15.0f)]
		PARAM_HA_DIRECTIONALITY_EXTEND_LEFT = 10,

		[SpatializerParameter(label = "Anechoic directionality attenuation for right ear", description = "Set directionality extend for right ear. The value is the attenuation in decibels applied to sources placed behind the listener", units = "dB", min = 0.0f, max = 30.0f, defaultValue =15.0f)]
		PARAM_HA_DIRECTIONALITY_EXTEND_RIGHT = 11,

		[SpatializerParameter(label = "Enable directionality simulation for left ear", type = typeof(bool), defaultValue = 0.0f)]
		PARAM_HA_DIRECTIONALITY_ON_LEFT = 12,

		[SpatializerParameter(label = "Enable directionality simulation for right ear", type = typeof(bool), defaultValue =0.0f)]
		PARAM_HA_DIRECTIONALITY_ON_RIGHT = 13,

		[SpatializerParameter(label = "Enable limiter", description = "Enable dynamics limiter after spatialization, to avoid potential saturation", type = typeof(bool), defaultValue = 1.0f)]
		PARAM_LIMITER_SET_ON = 14,

		[SpatializerParameter(label = "HRTF resampling step", description = "HRTF resampling step; Lower values give better quality at the cost of more memory usage", min = 1, max = 90, type = typeof(int), defaultValue =15)]
		PARAM_HRTF_STEP = 15,
	};


	public const int NumSpatializerParameters = 16;

	// Store the parameter values here for Unity to serialize. We initialize them to their default values. This is private and clients should use the accessor/getter methods below which will ensure the plugin is kept in sync with these values.
	// NB, per-source parameters may be set on individual sources but they are also set on the core which defines their initial value.
	[SerializeField]
	private float[] spatializerParameters = Enumerable.Range(0, NumSpatializerParameters).Select(i => ((SpatializerParameter)i).GetAttribute<SpatializerParameterAttribute>().defaultValue).ToArray<float>();


	///// <summary>
	///// Parameters that can be set for each specific sound source using AudioSource.setSpatializerFloat
	///// Note: I'm not sure the pluginName is relevant for these, but it's here for completeness.
	///// </summary>
	//public enum SpatializerSourceParameter
	//   {

	//   }
	//public const int NumSpatializerSourceParameters = 5;
	//   // Initialize to NaN so we know whether to override with default values from the plugin or whether Unity has serialized these
	//   private float[] spatializerSourceParameterInitialValues = Enumerable.Range(0, NumSpatializerSourceParameters).Select(i => ((SpatializerParameter)i).GetAttribute<SpatializerParameterAttribute>().defaultValue).ToArray<float>();

	// ======

	TSampleRateEnum sampleRate;

	int sampleRateIndex = (int)TSampleRateEnum.K48; //48k by default
													// LISTENER:
													//public enum TSpatializationMode
													//{
													//    HIGH_QUALITY = 0,
													//    HIGH_PERFORMANCE = 1,
													//    NONE = 2
													//}
	// These values match Binaural::TSpatializationMode in 3DTI toolkit SingleSource.h.
	public enum SpatializationMode : int {
        SPATIALIZATION_MODE_NONE = 0,
        SPATIALIZATION_MODE_HIGH_PERFORMANCE = 1,
        SPATIALIZATION_MODE_HIGH_QUALITY = 2,
	}

	// ===== These values are written to by the Editor GUI panel

	public string HRTFFileName44 = "Assets/3DTuneIn/Resources/Data/HighQuality/HRTF/3DTI_HRTF_IRC1032_256s_44100Hz.3dti-hrtf.bytes";
	public string HRTFFileName48 = "Assets/3DTuneIn/Resources/Data/HighQuality/HRTF/3DTI_HRTF_IRC1032_256s_48000Hz.3dti-hrtf.bytes";
	public string HRTFFileName96 = "Assets/3DTuneIn/Resources/Data/HighQuality/HRTF/3DTI_HRTF_IRC1032_256s_96000Hz.3dti-hrtf.bytes";
	public string ILDNearFieldFileName44 = "Assets/3DTuneIn/Resources/Data/HighQuality/ILD/NearFieldCompensation_ILD_44100.3dti-ild.bytes";
	public string ILDNearFieldFileName48 = "Assets/3DTuneIn/Resources/Data/HighQuality/ILD/NearFieldCompensation_ILD_48000.3dti-ild.bytes";
	public string ILDNearFieldFileName96 = "Assets/3DTuneIn/Resources/Data/HighQuality/ILD/NearFieldCompensation_ILD_96000.3dti-ild.bytes";
	public string ILDHighPerformanceFileName44 = "Assets/3DTuneIn/Resources/Data/HighPerformance/ILD/HRTF_ILD_44100.3dti-ild.bytes";
	public string ILDHighPerformanceFileName48 = "Assets/3DTuneIn/Resources/Data/HighPerformance/ILD/HRTF_ILD_48000.3dti-ild.bytes";
	public string ILDHighPerformanceFileName96 = "Assets/3DTuneIn/Resources/Data/HighPerformance/ILD/HRTF_ILD_96000.3dti-ild.bytes";
    public string BRIRFileName44 = "Assets/3DTuneIn/Resources/Data/Reverb/BRIR/3DTI_BRIR_large_44100.3dti-brir.bytes";
    public string BRIRFileName48 = "Assets/3DTuneIn/Resources/Data/Reverb/BRIR/3DTI_BRIR_large_48000.3dti-brir.bytes";
    public string BRIRFileName96 = "Assets/3DTuneIn/Resources/Data/Reverb/BRIR/3DTI_BRIR_large_96000.3dti-brir.bytes";

    //[SerializeField]
    //public string HRTFFileName44 = "";
    //public string HRTFFileName48 = "";
    //public string HRTFFileName96 = "";
    //public string ILDNearFieldFileName44 = "";
    //public string ILDNearFieldFileName48 = "";
    //public string ILDNearFieldFileName96 = "";
    //public string ILDHighPerformanceFileName44 = "";
    //public string ILDHighPerformanceFileName48 = "";
    //public string ILDHighPerformanceFileName96 = "";

 //   public bool customITDEnabled = false;           // For internal use, DO NOT USE IT DIRECTLY
	//public float listenerHeadRadius = 0.0875f;      // For internal use, DO NOT USE IT DIRECTLY    
	//												//public TSpatializationMode spatializationMode = TSpatializationMode.HIGH_QUALITY;        // For internal use, DO NOT USE IT DIRECTLY
	//public int spatializationMode = SPATIALIZATION_MODE_HIGH_QUALITY;   // For internal use, DO NOT USE IT DIRECTLY

	//// SOURCE:
	//int lastSourceID = 0;                       // For internal use, DO NOT USE IT DIRECTLY

	//// ADVANCED:
	//public float scaleFactor = 1.0f;            // For internal use, DO NOT USE IT DIRECTLY
	//public bool runtimeInterpolateHRTF = true;  // For internal use, DO NOT USE IT DIRECTLY
	//public int HRTFstep = 15;                   // For internal use, DO NOT USE IT DIRECTLY
	//public bool modFarLPF = true;               // For internal use, DO NOT USE IT DIRECTLY
	//public bool modDistAtt = true;              // For internal use, DO NOT USE IT DIRECTLY
	//public bool modNearFieldILD = true;         // For internal use, DO NOT USE IT DIRECTLY
	//public bool modHRTF = true;                 // For internal use, DO NOT USE IT DIRECTLY
	//public float magAnechoicAttenuation = -1.0f;    // For internal use, DO NOT USE IT DIRECTLY    
	//public float magSoundSpeed = 343.0f;            // For internal use, DO NOT USE IT DIRECTLY
	//public bool debugLog = false;                   // For internal use, DO NOT USE IT DIRECTLY    

	//// HEARING AID DIRECTIONALITY:
	////public CEarAPIParameter<bool> doHADirectionality = new CEarAPIParameter<bool>(false);
	//public bool doHADirectionalityLeft = false;     // For internal use, DO NOT USE IT DIRECTLY    
	//public bool doHADirectionalityRight = false;    // For internal use, DO NOT USE IT DIRECTLY    
	//public float HADirectionalityExtendLeft = 15.0f;    // For internal use, DO NOT USE IT DIRECTLY
	//public float HADirectionalityExtendRight = 15.0f;   // For internal use, DO NOT USE IT DIRECTLY

	//// LIMITER
	//public bool doLimiter = true;               // For internal use, DO NOT USE IT DIRECTLY

	//// Definition of spatializer plugin commands
	//int LOAD_3DTI_HRTF = 0;
	//int SET_HEAD_RADIUS = 1;
	//int SET_SCALE_FACTOR = 2;
	//int SET_SOURCE_ID = 3;
	//int SET_CUSTOM_ITD = 4;
	//int SET_HRTF_INTERPOLATION = 5;
	//int SET_MOD_FARLPF = 6;
	//int SET_MOD_DISTATT = 7;
	//int SET_MOD_NEARFIELD_ILD = 8;
	////int SET_MOD_HRTF = 9;   // DEPRECATED
	//int SET_MAG_ANECHATT = 10;
	//int SET_MAG_SOUNDSPEED = 11;
	//int LOAD_3DTI_ILD_NEARFIELD = 12;
	//int SET_DEBUG_LOG = 13;
	//int SET_HA_DIRECTIONALITY_EXTEND_LEFT = 14;
	//int SET_HA_DIRECTIONALITY_EXTEND_RIGHT = 15;
	//int SET_HA_DIRECTIONALITY_ON_LEFT = 16;
	//int SET_HA_DIRECTIONALITY_ON_RIGHT = 17;
	//int SET_LIMITER_ON = 18;
	//int GET_LIMITER_COMPRESSION = 19;
	//int GET_IS_CORE_READY = 20;
	//int SET_HRTF_STEP = 21;
	//int LOAD_3DTI_ILD_HIGHPERFORMANCE = 22;
	//int SET_SPATIALIZATION_MODE = 23;
	//int GET_BUFFER_SIZE = 24;
	//int GET_SAMPLE_RATE = 25;
	//int GET_BUFFER_SIZE_CORE = 26;
	//int GET_SAMPLE_RATE_CORE = 27;
	//// For high performance / High quality modes, variables to check which resources have been loaded
	//bool HighQualityModeHRTFLoaded = false;
	//bool HighQualityModeILDLoaded = false;
	//bool HighPerformanceModeILDLoaded = false;

	//// This is needed from Unity 2017
	//bool isInitialized = false;

	/////////////////////////////////////////////////////////////////////
	///
	// This is to force Unity to make a spatializer instance so we can set up
	// the common values at the start of the scene.
	private AudioSource silentAudioSource;


#if UNITY_IPHONE
    [DllImport ("__Internal")]

#else
    [DllImport("AudioPlugin3DTIToolkit")]
#endif
    private static extern bool Initialize3DTISpatializer(string hrtfPath, string ildPath, string highPerformanceILDPath, string brirPath);

#if UNITY_IPHONE
    [DllImport ("__Internal")]

#else
    [DllImport("AudioPlugin3DTIToolkit")]
#endif
	private static extern bool Set3DTISpatializerFloat(int parameterID, float value);

#if UNITY_IPHONE
    [DllImport ("__Internal")]

#else
    [DllImport("AudioPlugin3DTIToolkit")]
#endif
	private static extern bool Get3DTISpatializerFloat(int parameterID, out float value);



	/// Test if a Spatializer instance has been created. This can only be done by adding the SpatializerCore 
	/// effect to a mixer. Currently only one instance is supported
#if UNITY_IPHONE
    [DllImport ("__Internal")]

#else
	[DllImport("AudioPlugin3DTIToolkit")]
#endif
	private static extern bool Is3DTISpatializerCreated();


	/// <summary>

	/// Automatic setup of Toolkit Core (as read from custom GUI in Unity Inspector)
	/// </summary>
	void Start()
	{
		if (!Is3DTISpatializerCreated())
        {
			Debug.LogError("Cannot start 3DTI Spatializer as no instance has been created. Please ensure the SpatializerCore plugin has been added to a mixer in the scene.");
			return;
        }

		for (int i=0; i<NumSpatializerParameters; i++)
        {
			if (!Set3DTISpatializerFloat(i, spatializerParameters[i]))
            {
				Debug.LogError($"Failed to set 3DTI parameter {i}.", this);
            }
        }

		
		//Debug.Assert(numBuffers == 2);
		bool loadOK = false;

		// TODO: Only enforce passing in required binaries given the setup (e.g. high performance/high quality)
		// TODO: Ensure failure to load HRTF clears the previously loaded HRTF if any.
		switch (AudioSettings.outputSampleRate)
		{
			case 44100:
                {
                    loadOK = (SaveResourceAsBinary(HRTFFileName44, out string hrtfPath) == 1)
                        && (SaveResourceAsBinary(ILDNearFieldFileName44, out string ildPath) == 1)
                        && (SaveResourceAsBinary(ILDHighPerformanceFileName44, out string ildHighPerformancePath) == 1)
                        && (SaveResourceAsBinary(BRIRFileName44, out string brirPath) == 1)
                        && Initialize3DTISpatializer(hrtfPath, ildPath, ildHighPerformancePath, brirPath);
                }
                break;
            case 48000:
                {
                    loadOK = (SaveResourceAsBinary(HRTFFileName48, out string hrtfPath) == 1)
                            && (SaveResourceAsBinary(ILDNearFieldFileName48, out string ildPath) == 1)
                            && (SaveResourceAsBinary(ILDHighPerformanceFileName48, out string ildHighPerformancePath) == 1)
                            && (SaveResourceAsBinary(BRIRFileName48, out string brirPath) == 1)
                            && Initialize3DTISpatializer(hrtfPath, ildPath, ildHighPerformancePath, brirPath);
                }
                break;
            case 96000:
                {
                    loadOK = (SaveResourceAsBinary(HRTFFileName96, out string hrtfPath) == 1)
                            && (SaveResourceAsBinary(ILDNearFieldFileName96, out string ildPath) == 1)
                            && (SaveResourceAsBinary(ILDHighPerformanceFileName96, out string ildHighPerformancePath) == 1)
                            && (SaveResourceAsBinary(BRIRFileName96, out string brirPath) == 1)
                            && Initialize3DTISpatializer(hrtfPath, ildPath, ildHighPerformancePath, brirPath);
                }
                break;
            default:
                Debug.LogError($"Unsupported sample rate for 3DTI Spatializer {AudioSettings.outputSampleRate}. Supported values are 44100, 48000 and 96000.");
                break;
        }
        if (!loadOK)
        {
			Debug.LogError("Failed to initialize Spatializer plugin");
        }
		else
        {
			Debug.Log("Spatializer plugin initialized.");
        }

        //StartBinauralSpatializer();

	}

	// --- Spatializer Core parameters

	public bool SetFloatParameter(SpatializerParameter parameter, float value, AudioSource source = null)
    {
		if (source != null)
		{
			if (!parameter.GetAttribute<SpatializerParameterAttribute>().isSourceParameter)
            {
				Debug.LogError($"Cannot set spatialization parameter {parameter} on a single AudioSource. Call this method with source==null to set this parameter on the Spatializer Core.", this);
				return false;
            }
			else
			{
				if (!source.SetSpatializerFloat((int)parameter, value))
				{
                    Debug.LogError($"Failed to set spatialization parameter {parameter} for AudioSource {source} on 3DTI Spatializer plugin.", this);
                    return false;
                }
				if (!source.GetSpatializerFloat((int)parameter, out float finalValue))
                {
                    Debug.LogError($"Failed to retrieve value of parameter {parameter} for AudioSource {source} from 3DTI Spatializer plugin after setting it.", this);
					return false;
                }
				else if (finalValue != value)
                {
					Debug.LogWarning($"Value for parameter {parameter} on source {source} was requested to be set to {value} but 3DTI Spatializer plugin corrected this value to {finalValue}");
                }
            }

		}
		else
		{
			if (!Set3DTISpatializerFloat((int)parameter, value))
			{
				Debug.LogError($"Failed to set parameter {parameter} on 3DTI Spatializer plugin.", this);
				return false;
			}
			if (!Get3DTISpatializerFloat((int)parameter, out spatializerParameters[(int)parameter]))
			{
				Debug.LogError($"Failed to retrieve value of parameter {parameter} from 3DTI Spatializer plugin after setting it.", this);
				return false;
			}
		}
		return true;
    }

	public bool GetFloatParameter(SpatializerParameter parameter, out float value, AudioSource source=null)
    {
		if (source != null)
        {
            if (!parameter.GetAttribute<SpatializerParameterAttribute>().isSourceParameter)
            {
                Debug.LogError($"Cannot get spatialization parameter {parameter} for a single AudioSource as it is not a per-source parameter. Call this method with source==null to retrieve this parameter's value on the Spatializer Core.", this);
				value = parameter.GetAttribute<SpatializerParameterAttribute>().defaultValue;
                return false;
            }
            else
            {
                if (!source.GetSpatializerFloat((int)parameter, out value))
                {
                    Debug.LogError($"Failed to retrieve value of parameter {parameter} for AudioSource {source} from 3DTI Spatializer plugin.", this);
                    return false;
                }
            }
        }
		if (!Get3DTISpatializerFloat((int)parameter, out value))
        {
            Debug.LogError($"Failed to retrieve parameter {parameter} from 3DTI Spatializer plugin.", this);
			return false;
        }
		return true;
    }

    // Throws exception on failure
    public float GetFloatParameter(SpatializerParameter parameter, AudioSource source=null)
    {
        if (!GetFloatParameter(parameter, out float value, source))
        {
			throw new Exception($"Failed to retrieve parameter {parameter} from 3DTI Spatializer plugin.");
        }
        return value;
    }

	public T GetParameter<T>(SpatializerParameter parameter, AudioSource source = null)
    {
		SpatializerParameterAttribute attributes = parameter.GetAttribute<SpatializerParameterAttribute>();
		Debug.Assert(typeof(T) == attributes.type);
		float f = GetFloatParameter(parameter, source);
		if (typeof(T).IsEnum)
        {
			return (T) Enum.ToObject(typeof(SpatializationMode), (Int32)f);
            //return (T)Convert.ChangeType((int)f, typeof(T));
        }
		else
        {
            return (T)Convert.ChangeType(f, typeof(T));
        }
    }

	public bool SetParameter<T>(SpatializerParameter parameter, T value, AudioSource source = null)
    {
        SpatializerParameterAttribute attributes = parameter.GetAttribute<SpatializerParameterAttribute>();
        Debug.Assert(typeof(T) == attributes.type);
		return SetFloatParameter(parameter, Convert.ToSingle(value), source);
    }


    void Update()
	{

	}

	/////////////////////////////////////////////////////////////////////
	// GLOBAL METHODS
	/////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Sends the configuration to the spatializer plugin. This can only be run
	/// once a spatialized AudioSource is playing.
	/// </summary>
	//private bool StartBinauralSpatializer()
	//{
	//	// Select an AudioSource
	//	AudioSource source = null;
	//	foreach (AudioSource s in FindObjectsOfType<AudioSource>())
	//	{
	//		if (s.isActiveAndEnabled && s.spatialize)
	//		{
	//			source = s;
	//		}
	//	}

	//	if (source == null)
	//	{
	//		return false;
	//	}



	//	// Debug log:
	//	if (!SendWriteDebugLog(debugLog, source))
	//	{
	//		return false;
	//	}

	//	// Global setup:
	//	if (!SetScaleFactor(scaleFactor, source))
	//	{
	//		return false;
	//	}

	//	if (!SendSourceIDs(source))
	//	{
	//		return false;
	//	}

	//	// Setup modules enabler:
	//	if (!SetupModulesEnabler(source))
	//	{
	//		return false;
	//	}

	//	// Source setup:
	//	if (!SetupSource(source))
	//	{
	//		return false;
	//	}

	//	// Hearing Aid directionality setup:
	//	if (!SetupHADirectionality(source))
	//	{
	//		return false;
	//	}

	//	// Limiter setup:
	//	if (!SetupLimiter(source))
	//	{
	//		return false;
	//	}

	//	// Listener setup:
	//	if (!SetupListener(source))
	//	{
	//		return false;
	//	}

	//	return true;
	//}

	///// <summary>
	///// Enable all binaural spatialization processes for one (or all) source/s
	///// </summary>
	///// <param name="source"></param>
	///// <returns></returns>
	//public bool EnableSpatialization(AudioSource source=null)
	//{
	//    //if (spatializationMode == SPATIALIZATION_MODE_HIGH_QUALITY)
	//    //{
	//    //    //if (!SetModHRTF(true)) return false;
	//    //    if (!SetModNearFieldILD(true, source)) return false;
	//    //    if (!SetModFarLPF(true, source)) return false;
	//    //    if (!SetModDistanceAttenuation(true, source)) return false;
	//    //    return SendCommand(SET_SPATIALIZATION_MODE, (float)(spatializationMode), source);
	//    //}
	//    //if (spatializationMode == SPATIALIZATION_MODE_HIGH_PERFORMANCE)
	//    //{
	//    //    if (!SetModFarLPF(true, source)) return false;
	//    //    if (!SetModDistanceAttenuation(true, source)) return false;
	//    //    return SendCommand(SET_SPATIALIZATION_MODE, (float)(spatializationMode), source);
	//    //}
	//    //return false;
	//    return SendCommand(SET_SPATIALIZATION_MODE, (float)(spatializationMode), source);
	//}

	///// <summary>
	///// Enable all binaural spatialization processes for one (or all) source/s
	///// </summary>
	///// <param name="source"></param>
	///// <returns></returns>
	//public bool DisableSpatialization(AudioSource source = null)
	//{
	//    //if (spatializationMode == SPATIALIZATION_MODE_HIGH_QUALITY)
	//    //{
	//    //    //if (!SetModHRTF(false, source)) return false;
	//    //    if (!SetModNearFieldILD(false, source)) return false;
	//    //    if (!SetModFarLPF(false, source)) return false;
	//    //    if (!SetModDistanceAttenuation(false, source)) return false;
	//    //    return SendCommand(SET_SPATIALIZATION_MODE, (float)(SPATIALIZATION_MODE_NONE), source);
	//    //}
	//    //if (spatializationMode == SPATIALIZATION_MODE_HIGH_PERFORMANCE)
	//    //{
	//    //    if (!SetModFarLPF(false, source)) return false;
	//    //    if (!SetModDistanceAttenuation(false, source)) return false;
	//    //    return SendCommand(SET_SPATIALIZATION_MODE, (float)(SPATIALIZATION_MODE_NONE), source);
	//    //}
	//    //return false;
	//    return SendCommand(SET_SPATIALIZATION_MODE, (float)(SPATIALIZATION_MODE_NONE), source);
	//}

	/////////////////////////////////////////////////////////////////////
	// LISTENER METHODS
	/////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Setup all listener parameters
	/// </summary>
//	public bool SetupListener(AudioSource source = null)
//	{
//		if (!SetHeadRadius(listenerHeadRadius, source))
//		{
//			return false;
//		}

//		if (!SetCustomITD(customITDEnabled, source))
//		{
//			return false;
//		}

//		sampleRateIndex = GetSampleRateEnum();
//		// HIGH QUALITY MODE (default)
//		//if (spatializationMode == TSpatializationMode.HIGH_QUALITY)
//		if (spatializationMode == SPATIALIZATION_MODE_HIGH_QUALITY)
//		{
//			if (!HighQualityModeILDLoaded)
//			{
//				string ILDNearFieldFileName = "";
//				switch (sampleRateIndex)
//				{
//					case (int)TSampleRateEnum.K44:
//						ILDNearFieldFileName = ILDNearFieldFileName44;
//						break;

//					case (int)TSampleRateEnum.K48:
//						ILDNearFieldFileName = ILDNearFieldFileName48;
//						break;

//					case (int)TSampleRateEnum.K96:
//						ILDNearFieldFileName = ILDNearFieldFileName96;
//						break;

//				}

//				if (!ILDNearFieldFileName.Equals(""))
//				{
//					//#if (!UNITY_EDITOR)
//					string binaryPath;
//                    if (SaveResourceAsBinary(ILDNearFieldFileName, out binaryPath) != 1) return false;
////#endif
//					if (!LoadILDNearFieldBinary(binaryPath, source))
//					{
//						return false;
//					}
//				}
//			}
//			if (!HighQualityModeHRTFLoaded)
//			{
//				string HRTFFileName = "";
//				switch (sampleRateIndex)
//				{
//					case (int)TSampleRateEnum.K44:
//						HRTFFileName = HRTFFileName44;
//						break;

//					case (int)TSampleRateEnum.K48:
//						HRTFFileName = HRTFFileName48;
//						break;

//					case (int)TSampleRateEnum.K96:
//						HRTFFileName = HRTFFileName96;
//						break;

//				}
//				if (!HRTFFileName.Equals(""))
//				{
//					//#if (!UNITY_EDITOR)
//					string binaryPath;
//                    if (SaveResourceAsBinary(HRTFFileName, out binaryPath) != 1) return false; 
////#endif
//					if (!LoadHRTFBinary(binaryPath, source))
//					{
//						return false;
//					}
//				}
//			}
//		}

//		// HIGH PERFORMANCE MODE
//		//if (spatializationMode == TSpatializationMode.HIGH_PERFORMANCE)
//		if (spatializationMode == SPATIALIZATION_MODE_HIGH_PERFORMANCE)
//		{
//			if (!HighPerformanceModeILDLoaded)
//			{
//				string ILDHighPerformanceFileName = "";
//				switch (sampleRateIndex)
//				{
//					case (int)TSampleRateEnum.K44:
//						ILDHighPerformanceFileName = ILDHighPerformanceFileName44;
//						break;

//					case (int)TSampleRateEnum.K48:
//						ILDHighPerformanceFileName = ILDHighPerformanceFileName48;
//						break;

//					case (int)TSampleRateEnum.K96:
//						ILDHighPerformanceFileName = ILDHighPerformanceFileName96;
//						break;

//				}
//				if (!ILDHighPerformanceFileName.Equals(""))
//				{
//					string filePath;
////#if (!UNITY_EDITOR)
//                    if (SaveResourceAsBinary(ILDHighPerformanceFileName, out filePath) != 1) return false;
////#endif
//					if (!LoadILDHighPerformanceBinary(filePath, source))
//					{
//						return false;
//					}
//				}
//			}
//		}

//		return true;
//	}

	/// <summary>
	/// Load one file from resources and save it as a binary file (for Android)
	/// </summary>    
	private int SaveResourceAsBinary(string originalName, out string newFilename)
	{
        // remove .bytes extension
        if (originalName.EndsWith(".bytes"))
        {
			originalName = originalName.Substring(0, originalName.Length - ".bytes".Length);
        }

        // Setup name for new file
        newFilename = Application.persistentDataPath + "/" + originalName;

		// Load as asset from resources 
		TextAsset txtAsset = Resources.Load(originalName) as TextAsset;
		if (txtAsset == null)
		{
			Debug.LogError($"Could not load 3DTI resource {originalName}", this);
			return -1;  // Could not load asset from resources
		}

        // Transform asset into stream and then into byte array
        MemoryStream streamData = new MemoryStream(txtAsset.bytes);
		byte[] dataArray = streamData.ToArray();


		// Write binary data to binary file        
		Directory.CreateDirectory(Path.GetDirectoryName(newFilename));
		using (BinaryWriter writer = new BinaryWriter(File.Open(newFilename, FileMode.Create)))
		{
			writer.Write(dataArray);
		}

		return 1;
	}

    ///// <summary>
    ///// Set listener head radius
    ///// </summary>
    //public bool SetHeadRadius(float headRadius, AudioSource source = null)
    //{
    //	listenerHeadRadius = headRadius;
    //	return SendCommand(SET_HEAD_RADIUS, headRadius, source);
    //}

    ///////////////////////////////////////////////////////////////////////

    ///// <summary>
    ///// Set custom ITD enabled/disabled
    ///// </summary>
    //public bool SetCustomITD(bool _enable, AudioSource source = null)
    //{
    //	customITDEnabled = _enable;
    //	return SendCommand(SET_CUSTOM_ITD, CommonFunctions.Bool2Float(_enable), source);
    //}

    ///////////////////////////////////////////////////////////////////////

    ///// <summary>
    ///// Load HRTF from a binary .3dti file
    ///// </summary>
    //public bool LoadHRTFBinary(string filename, AudioSource source = null)
    //{
    //	//switch (sampleRateIndex)
    //	//{
    //	//	case (int)TSampleRateEnum.K44:
    //	//		HRTFFileName44 = filename;
    //	//		break;

    //	//	case (int)TSampleRateEnum.K48:
    //	//		HRTFFileName48 = filename;
    //	//		break;

    //	//	case (int)TSampleRateEnum.K96:
    //	//		HRTFFileName96 = filename;
    //	//		break;

    //	//}

    //	List<AudioSource> audioSources;
    //	if (source != null)
    //	{
    //		audioSources = new List<AudioSource>();
    //		audioSources.Add(source);
    //	}
    //	else
    //	{
    //		audioSources = GetAllSpatializedSources();
    //	}

    //	foreach (AudioSource s in audioSources)
    //	{
    //		s.SetSpatializerFloat(LOAD_3DTI_HRTF, (float)filename.Length);
    //		for (int i = 0; i < filename.Length; i++)
    //		{
    //			int chr2Int = (int)filename[i];
    //			float chr2Float = (float)chr2Int;
    //			if (!s.SetSpatializerFloat(LOAD_3DTI_HRTF, chr2Float))
    //			{
    //				return false;
    //			}
    //		}
    //	}

    //	HighQualityModeHRTFLoaded = true;
    //	return true;
    //}

    ///////////////////////////////////////////////////////////////////////

    ///// <summary>
    ///// Load ILD Near Field from a binary .3dti file
    ///// </summary>
    //public bool LoadILDNearFieldBinary(string filename, AudioSource source = null)
    //{
    //	//switch (sampleRateIndex)
    //	//{
    //	//	case (int)TSampleRateEnum.K44:
    //	//		ILDNearFieldFileName44 = filename;
    //	//		break;

    //	//	case (int)TSampleRateEnum.K48:
    //	//		ILDNearFieldFileName48 = filename;
    //	//		break;

    //	//	case (int)TSampleRateEnum.K96:
    //	//		ILDNearFieldFileName96 = filename;
    //	//		break;

    //	//}

    //	List<AudioSource> audioSources;
    //	if (source != null)
    //	{
    //		audioSources = new List<AudioSource>();
    //		audioSources.Add(source);
    //	}
    //	else
    //	{
    //		audioSources = GetAllSpatializedSources();
    //	}

    //	foreach (AudioSource s in audioSources)
    //	{
    //		s.SetSpatializerFloat(LOAD_3DTI_ILD_NEARFIELD, (float)filename.Length);
    //		for (int i = 0; i < filename.Length; i++)
    //		{
    //			int chr2Int = (int)filename[i];
    //			float chr2Float = (float)chr2Int;
    //			if (!s.SetSpatializerFloat(LOAD_3DTI_ILD_NEARFIELD, chr2Float))
    //			{
    //				return false;
    //			}
    //		}
    //	}

    //	HighQualityModeILDLoaded = true;
    //	return true;
    //}

    ///////////////////////////////////////////////////////////////////////

    ///// <summary>
    ///// Load ILD High Performance from a binary .3dti file
    ///// </summary>
    //public bool LoadILDHighPerformanceBinary(string filename, AudioSource source = null)
    //{
    //	//switch (sampleRateIndex)
    //	//{
    //	//	case (int)TSampleRateEnum.K44:
    //	//		ILDHighPerformanceFileName44 = filename;
    //	//		break;

    //	//	case (int)TSampleRateEnum.K48:
    //	//		ILDHighPerformanceFileName48 = filename;
    //	//		break;

    //	//	case (int)TSampleRateEnum.K96:
    //	//		ILDHighPerformanceFileName96 = filename;
    //	//		break;

    //	//}

    //	List<AudioSource> audioSources;
    //	if (source != null)
    //	{
    //		audioSources = new List<AudioSource>();
    //		audioSources.Add(source);
    //	}
    //	else
    //	{
    //		audioSources = GetAllSpatializedSources();
    //	}

    //	foreach (AudioSource s in audioSources)
    //	{
    //		s.SetSpatializerFloat(LOAD_3DTI_ILD_HIGHPERFORMANCE, (float)filename.Length);
    //		for (int i = 0; i < filename.Length; i++)
    //		{
    //			int chr2Int = (int)filename[i];
    //			float chr2Float = (float)chr2Int;
    //			if (!s.SetSpatializerFloat(LOAD_3DTI_ILD_HIGHPERFORMANCE, chr2Float))
    //			{
    //				return false;
    //			}
    //		}
    //	}

    //	HighPerformanceModeILDLoaded = true;
    //	return true;
    //}


    ///////////////////////////////////////////////////////////////////////
    //// SOURCE API METHODS
    ///////////////////////////////////////////////////////////////////////

    ///// <summary>
    ///// Setup all source parameters
    ///// </summary>        
    //public bool SetupSource(AudioSource source = null)
    //{
    //	if (!SetSourceInterpolation(runtimeInterpolateHRTF, source))
    //	{
    //		return false;
    //	}

    //	return SetHRTFResamplingStep(HRTFstep, source);
    //}

    ///////////////////////////////////////////////////////////////////////

    ///// <summary>
    ///// Set source HRTF interpolation method
    ///// </summary>
    //public bool SetSourceInterpolation(bool _run, AudioSource source = null)
    //{
    //	runtimeInterpolateHRTF = _run;
    //	return SendCommand(SET_HRTF_INTERPOLATION, CommonFunctions.Bool2Float(_run), source);
    //}

    ///// <summary>
    ///// Set HRTF resampling step
    ///// </summary>
    ///// <param name="step"></param>
    ///// <returns></returns>
    //public bool SetHRTFResamplingStep(int step, AudioSource source = null)
    //{
    //	HRTFstep = step;
    //	return SendCommand(SET_HRTF_STEP, (float)step, source);
    //}

    ///////////////////////////////////////////////////////////////////////
    //// ADVANCED API METHODS
    ///////////////////////////////////////////////////////////////////////

    ///// <summary>
    ///// Set ID for all sources, for internal use of the wrapper
    ///// </summary>
    //public bool SendSourceIDs(AudioSource source = null)
    //{
    //	if (source == null)
    //	{
    //		List<AudioSource> audioSources = GetAllSpatializedSources();
    //		foreach (AudioSource s in audioSources)
    //		{
    //			if (!s.SetSpatializerFloat(SET_SOURCE_ID, (float)++lastSourceID))
    //			{
    //				return false;
    //			}
    //		}
    //		return true;
    //	}
    //	else
    //	{
    //		return source.SetSpatializerFloat(SET_SOURCE_ID, (float)++lastSourceID);
    //	}
    //}

    ///////////////////////////////////////////////////////////////////////

    ///// <summary>
    ///// Set scale factor. Allows the toolkit to work with big-scale or small-scale scenarios
    ///// </summary>
    //public bool SetScaleFactor(float scale, AudioSource source = null)
    //{
    //	scaleFactor = scale;
    //	return SendCommand(SET_SCALE_FACTOR, scale, source);
    //}

    ///// <summary>
    /////  Setup modules enabler, allowing to switch on/off core features
    ///// </summary>
    //public bool SetupModulesEnabler(AudioSource source = null)
    //{
    //	if (!SetModFarLPF(modFarLPF, source))
    //	{
    //		return false;
    //	}

    //	if (!SetModDistanceAttenuation(modDistAtt, source))
    //	{
    //		return false;
    //	}

    //	if (!SetModNearFieldILD(modNearFieldILD, source))
    //	{
    //		return false;
    //	}
    //	//if (!SetModHRTF(modHRTF)) return false;
    //	if (!SetSpatializationMode(spatializationMode))
    //	{
    //		return false;
    //	}

    //	return true;
    //}

    ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Switch on/off far distance LPF
    /// </summary>        
 //   public bool SetModFarLPF(bool _enable, AudioSource source = null)
 //   {
 //       modFarLPF = _enable;
 //       return SendCommand(SET_MOD_FARLPF, CommonFunctions.Bool2Float(_enable), source);
 //   }

 //   /////////////////////////////////////////////////////////////////////

 //   /// <summary>
 //   /// Switch on/off distance attenuation
 //   /// </summary>        
 //   public bool SetModDistanceAttenuation(bool _enable, AudioSource source = null)
 //   {
 //       modDistAtt = _enable;
 //       return SendCommand(SET_MOD_DISTATT, CommonFunctions.Bool2Float(_enable), source);
 //   }

 //   /////////////////////////////////////////////////////////////////////

 //   /// <summary>
 //   /// Switch on/off near field ILD
 //   /// </summary>        
 //   public bool SetModNearFieldILD(bool _enable, AudioSource source = null)
 //   {
 //       modNearFieldILD = _enable;
 //       return SendCommand(SET_MOD_NEARFIELD_ILD, CommonFunctions.Bool2Float(_enable), source);
 //   }

 //   /////////////////////////////////////////////////////////////////////

 //   /// <summary>
 //   /// Set spatialization mode (high quality, high performance, or none)
 //   /// </summary>
 //   /// <param name="mode"></param>
 //   /// <returns></returns>
 //   public bool SetSpatializationMode(int mode)
	//{
	//	// SPATIALIZATION MODE IS COMMON FOR ALL SOURCES

	//	spatializationMode = mode;

	//	// Load resources        
	//	if (!SetupListener())
	//	{
	//		Debug.LogWarning("SetupListener returned false", this);
	//	}

	//	// Send command to plugin        
	//	return SendCommand(SET_SPATIALIZATION_MODE, (float)(mode), null);
	//}

	/////////////////////////////////////////////////////////////////////

	///// <summary>
	///// Set magnitude Anechoic Attenuation
	///// </summary>    
	//public bool SetMagnitudeAnechoicAttenuation(float value, AudioSource source = null)
	//{
	//	magAnechoicAttenuation = value;
	//	return SendCommand(SET_MAG_ANECHATT, value, source);
	//}

	///// <summary>
	///// Set magnitude Sound Speed
	///// </summary>    
	//public bool SetMagnitudeSoundSpeed(float value, AudioSource source = null)
	//{
	//	magSoundSpeed = value;
	//	return SendCommand(SET_MAG_SOUNDSPEED, value, source);
	//}

	///////////////////////////////////////////////////////////////////////
	//// HA DIRECTIONALITY METHODS
	///////////////////////////////////////////////////////////////////////

	///// <summary>
	/////  Initial setup of HA directionality
	///// </summary>
	//public bool SetupHADirectionality(AudioSource source = null)
	//{
	//	//if (!SwitchOnOffHADirectionality(T_ear.LEFT, doHADirectionality.Get(T_ear.LEFT))) return false;
	//	//if (!SwitchOnOffHADirectionality(T_ear.RIGHT, doHADirectionality.Get(T_ear.RIGHT))) return false;
	//	if (!SwitchOnOffHADirectionality(T_ear.LEFT, doHADirectionalityLeft, source))
	//	{
	//		return false;
	//	}

	//	if (!SwitchOnOffHADirectionality(T_ear.RIGHT, doHADirectionalityRight, source))
	//	{
	//		return false;
	//	}

	//	if (!SetHADirectionalityExtend(T_ear.LEFT, HADirectionalityExtendLeft, source))
	//	{
	//		return false;
	//	}

	//	if (!SetHADirectionalityExtend(T_ear.RIGHT, HADirectionalityExtendRight, source))
	//	{
	//		return false;
	//	}

	//	return true;
	//}

	///// <summary>
	///// Switch on/off HA directionality for each ear
	///// </summary>
	///// <param name="ear"></param>
	///// <param name="_enable"></param>
	//public bool SwitchOnOffHADirectionality(T_ear ear, bool _enable, AudioSource source = null)
	//{
	//	if (ear == T_ear.BOTH)
	//	{
	//		SwitchOnOffHADirectionality(T_ear.LEFT, _enable, source);
	//		SwitchOnOffHADirectionality(T_ear.RIGHT, _enable, source);
	//	}

	//	if (ear == T_ear.LEFT)
	//	{
	//		//doHADirectionality.Set(T_ear.LEFT, _enable);
	//		doHADirectionalityLeft = _enable;
	//		return SendCommand(SET_HA_DIRECTIONALITY_ON_LEFT, CommonFunctions.Bool2Float(_enable), source);
	//	}
	//	if (ear == T_ear.RIGHT)
	//	{
	//		//doHADirectionality.Set(T_ear.RIGHT, _enable);
	//		doHADirectionalityRight = _enable;
	//		return SendCommand(SET_HA_DIRECTIONALITY_ON_RIGHT, CommonFunctions.Bool2Float(_enable), source);
	//	}
	//	return false;
	//}

	///// <summary>
	///// Set HA directionality extend (in dB) for each ear
	///// </summary>
	///// <param name="ear"></param>
	///// <param name="extendDB"></param>
	//public bool SetHADirectionalityExtend(T_ear ear, float extendDB, AudioSource source = null)
	//{
	//	if (ear == T_ear.BOTH)
	//	{
	//		SetHADirectionalityExtend(T_ear.LEFT, extendDB, source);
	//		SetHADirectionalityExtend(T_ear.RIGHT, extendDB, source);
	//	}

	//	if (ear == T_ear.LEFT)
	//	{
	//		HADirectionalityExtendLeft = extendDB;
	//		return SendCommand(SET_HA_DIRECTIONALITY_EXTEND_LEFT, extendDB, source);
	//	}
	//	if (ear == T_ear.RIGHT)
	//	{
	//		HADirectionalityExtendRight = extendDB;
	//		return SendCommand(SET_HA_DIRECTIONALITY_EXTEND_RIGHT, extendDB, source);
	//	}
	//	return false;
	//}

	///////////////////////////////////////////////////////////////////////
	//// LIMITER METHODS
	///////////////////////////////////////////////////////////////////////

	///// <summary>
	/////  Initial setup of limiter
	///// </summary>
	//public bool SetupLimiter(AudioSource source = null)
	//{
	//	if (!SwitchOnOffLimiter(doLimiter, source))
	//	{
	//		return false;
	//	}

	//	return true;
	//}

	///// <summary>
	///// Switc on/off limiter after spatialization process
	///// </summary>
	///// <param name="value"></param>
	///// <returns></returns>
	//public bool SwitchOnOffLimiter(bool _enable, AudioSource source = null)
	//{
	//	doLimiter = _enable;
	//	return SendCommand(SET_LIMITER_ON, CommonFunctions.Bool2Float(_enable), source);
	//}

	///// <summary>
	///// Get state of limiter (currently compressing or not)
	///// </summary>
	///// <param name="_compressing"></param>
	///// <returns></returns>
	//public bool GetLimiterCompression(out bool _compressing, AudioSource source = null)
	//{
	//	return GetBoolParameter(GET_LIMITER_COMPRESSION, out _compressing, source);
	//}
	///////////////////////////////////////////////////////////////////////
	//// SAMPLE RATE AND BUFFER SIZE GET METHODS
	///////////////////////////////////////////////////////////////////////
	///// <summary>
	///// Get audio sample rate in hertzs (Unity's)
	///// </summary>
	///// <param name="_sampleRate"></param>
	///// <returns></returns>
	//public bool GetSampleRate(out float _sampleRate)
	//{
	//	return GetFloatParameter(GET_SAMPLE_RATE, out _sampleRate);
	//}

	///// <summary>
	///// Get audio buffer size in number of samples (Unity's)
	///// </summary>
	///// <param name="_bufferSize"></param>
	///// <returns></returns>
	//public bool GetBufferSize(out float _bufferSize)
	//{
	//	return GetFloatParameter(GET_BUFFER_SIZE, out _bufferSize);
	//}
	///// <summary>
	///// Get audio sample rate in hertzs (Core's)
	///// </summary>
	///// <param name="_sampleRate"></param>
	///// <returns></returns>
	//public bool GetSampleRateCore(out float _sampleRate)
	//{
	//	return GetFloatParameter(GET_SAMPLE_RATE_CORE, out _sampleRate);
	//}

	///// <summary>
	///// Get audio buffer size in number of samples (Core's)
	///// </summary>
	///// <param name="_bufferSize"></param>
	///// <returns></returns>
	//public bool GetBufferSizeCore(out float _bufferSize)
	//{
	//	return GetFloatParameter(GET_BUFFER_SIZE_CORE, out _bufferSize);
	//}
	///////////////////////////////////////////////////////////////////////
	//// AUXILIARY FUNCTIONS
	///////////////////////////////////////////////////////////////////////

	///// <summary>
	///// Get the value of one bool parameter from the first instance of the spatialization plugin
	///// </summary>
	///// <param name="parameter"></param>
	///// <param name="value"></param>
	///// <returns></returns>
	//public bool GetBoolParameter(int parameter, out bool value, AudioSource source = null)
	//{
	//	value = false;
	//	float floatValue;
	//	if (!GetFloatParameter(parameter, out floatValue, source))
	//	{
	//		return false;
	//	}

	//	value = CommonFunctions.Float2Bool(floatValue);
	//	return true;
	//}

	///// <summary>
	///// Get the value of one float parameter from the first instance of the spatialization plugin 
	///// </summary>
	///// <param name="parameter"></param>
	///// <param name="value"></param>
	///// <returns></returns>
	//public bool GetFloatParameter(int parameter, out float value, AudioSource source = null)
	//{
	//	value = 0.0f;

	//	AudioSource s = source;

	//	// If no source is specified, we get value from the first spatialized source
	//	if (source == null)
	//	{
	//		List<AudioSource> sources = GetAllSpatializedSources();
	//		if (sources.Count > 0)
	//		{
	//			s = sources[0];
	//		}
	//		else
	//		{
	//			return false;
	//		}
	//	}

	//	// Send the command to get the value        
	//	return (s.GetSpatializerFloat(parameter, out value));
	//}

	//public bool IsCoreReadyToStart()
	//{
	//	// Test if the core has received the correct sample rate yet. This will happen after the first spatialized sound triggers the CreateCallback method
	//	float sampleRateCore;
	//	GetSampleRateCore(out sampleRateCore);
	//	return (int)sampleRateCore == AudioSettings.outputSampleRate;
	//}

	public int GetSampleRateEnum()
	{
		int _index = 0;
		int sampleRate = AudioSettings.outputSampleRate;
		//float sampleRateWrapper, sampleRateCore;
		//GetSampleRate(out sampleRateWrapper);
		//GetSampleRateCore(out sampleRateCore);
		//Debug.Log($"sampleRate: {sampleRate}, sampleRateWrapper: {sampleRateWrapper}, sampleRateCore: {sampleRateCore}");
		//if (Application.isPlaying /*|| UnityEditor.EditorApplication.isPlaying*/)
		//{
		//	if (sampleRate != sampleRateWrapper || sampleRate != sampleRateCore || sampleRateWrapper != sampleRateCore)
		//	{
		//		Debug.LogError($"Sample Rate no coincidente entre AudioSettings ({sampleRate}), Wrapper ({sampleRateWrapper}) y Core ({sampleRateCore})");
		//	}
		//}
		if (sampleRate > 0)
		{
			switch (sampleRate)
			{
				case 44100:
					_index = (int)TSampleRateEnum.K44;
					break;
				case 48000:
					_index = (int)TSampleRateEnum.K48;
					break;
				case 96000:
					_index = (int)TSampleRateEnum.K96;
					break;
				default:
					Debug.LogError("Sampling rates different than 44.1, 48 or 96kHz shall not be used." + Environment.NewLine + "Go to Edit -> Project Settings -> Audio and set System Sample Rate to a valid value.");
					Debug.Break();
					Debug.developerConsoleVisible = true;

#if (UNITY_EDITOR)
					UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif

					_index = (int)TSampleRateEnum.K48;
					break;
			}
		}
		return _index;
	}
	///// <summary>
	///// Send command to plugin to switch on/off write to Debug Log file
	///// </summary>
	//public bool SendWriteDebugLog(bool _enable, AudioSource source = null)
	//{
	//	debugLog = _enable;
	//	return SendCommand(SET_DEBUG_LOG, CommonFunctions.Bool2Float(_enable), source);
	//}

	///// <summary>
	///// Send command to the DLL, for selected source or for all registered sources
	///// </summary>
	//public bool SendCommand(int command, float value, AudioSource source)
	//{
	//	if (source == null)
	//	{
	//		List<AudioSource> audioSources = GetAllSpatializedSources();
	//		foreach (AudioSource s in audioSources)
	//		{
	//			if (!s.SetSpatializerFloat(command, value))
	//			{
	//				return false;
	//			}
	//		}
	//		return true;
	//	}
	//	else
	//	{
	//		return source.SetSpatializerFloat(command, value);
	//	}
	//}

	///// <summary>
	///// Returns a list with all audio sources with the Spatialized toggle checked
	///// </summary>
	//public List<AudioSource> GetAllSpatializedSources()
	//{
	//	//GameObject[] audioSources = GameObject.FindGameObjectsWithTag("AudioSource");

	//	List<AudioSource> spatializedSources = new List<AudioSource>();

	//	AudioSource[] audioSources = UnityEngine.Object.FindObjectsOfType<AudioSource>();
	//	foreach (AudioSource source in audioSources)
	//	{
	//		if (source.spatialize)
	//		{
	//			spatializedSources.Add(source);
	//		}
	//	}

	//	return spatializedSources;
	//}

}
