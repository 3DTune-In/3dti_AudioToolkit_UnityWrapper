# Change Log
All notable changes to the 3DTuneIn Toolkit will be documented in this file.

## v3.0:
**General:**
- **Warning**: Version 3.0 introduces a new API and is not backwards compatible with scripts controlling the toolkit with version 2.0. However, the new API is lightweight and, particularly for the Spatializer, requires much less work to integrate.
- A new API for parameters has been introduced with the aim of reducing the maintenance overhead of exposing 3DTI Toolkit features:
	- Currently this is implemented for HearingLoss (and the Spatializer - see below). HearingAid is yet to be updated.
	- Parameters are defined by a single enum in the effects component script:	`API_3DTI_HL.Parameter` and `API_3DTI_Spatializer.SpatializerParameter`.
	- The parameter enums have attributes which declares all required properties. The components have generic functions SetParameter and GetParameter. E.g.,
		- to turn on hearing loss: `SetParameter(API_3DTI_HL.Parameter.HLOn, true, T_ear.BOTH)`
		- to get the multiband expansion attack in the left ear: `GetParameter<float>(API_3DTI_HL.Parameter.MultibandExpansionNumFiltersPerBand, T_ear.LEFT)`
	- There is a big reduction in variables within the C# code that duplicate the state of the audio plugins, opting instead to read these values from the plugins where needed. In the spatializer where this isn't possible, parameter values are stored in a single serialized array.
  

**Binaural spatializer and reverb:**
- The API has been rewritten following a similar pattern to that introduced for Hearing Loss.
- SOFA format HRTF files are now supported (Windows x64 / MacOS only).
- Binaural reverb is now supported. Reverb is calculated for all spatialized audio sources in a single pass. Add the SpatializerReverb audio plugin to a mixer strip to get the reverberation signal. 
  - AudioSources may be individually set in terms of whether their signal is sent to the reverb.
- The Spatializer has been split into two effects:
	- SpatializerSource is attached to each spatialized audio source
	- SpatializerReverb is a mixer effect which handles spatialization parameters and processes the return of the binaural reverb
- The Spatializer scripts now communicate directly with the native audio plugin code, rather than going through the Unity AudioMixer interface. This greatly simplifies passing complex parameters such as paths for binary resource files.
	- Spatializer parameters follow a similar patterns as the new HearingLoss parameters but without the pluginName and mixerName attributes which are no longer needed, and with additional parameters: min, max, defaultValue, isSourceParameter 
	- Spatializer parameters where `isSourceParameter` is true may be set individually on specific audio sources. The `API_3DTI_SpatializerSetParameter` has an extra argument to specify the audio source. This may only be specified for parameters where `isSourceParameter` is true. If `isSourceParameter` is true but the source is left as `null` then the default value for this parameter used on new audio sources is set.
	- The GUI for the Spatializer has been redesigned with an aim to clarify which parameters are per-source and which are global.
	- Spatializer binary resources such as impulse responses are no longer defined using the Parameter enums. There are now specific functions:
	- `public bool SetBinaryResourcePath(BinaryResourceRole role, TSampleRateEnum sampleRate, string path)`
	- `public string GetBinaryResourcePath(BinaryResourceRole role, TSampleRateEnum sampleRate)`
- Debug logging to text files is being phased out as it had a huge overhead of code to maintain. If you need details of what's happening in the native plugin, you can build from source and attach the debugger to Unity. For mobile, the error console can be accessed using `adb` on Android and Xcode on iOS.
- Many bugs have been fixed through the above simplification, including the following (thanks to Kevin for reporting these):
    - Glitching noises when replaying a scene after a source had previously had Stop() called on it.
    - Comb filtering effect on an audio source when another source is loaded to play on awake but has no AudioClip loaded.
    - Incorrect amplitude (and otherwise incorrect values) of AudioSources when PlayOnAwake is set which is corrected when a subsequent source is played.

## v2.0:
**General:**

- Target Unity build is now 2019.4.
- Spatializer plugin now shares HRTF data between audio sources.
- Audio sources can be created dynamically while a scene is running.
- It is no longer necessary to make any C# calls when instantiating an audio source.
- When the scene starts, the spatializer component will create a silent audio source to trigger the one-time loading of the HRTF data by the spatializer plugin
- HRTFs are bundled in 'bytes' format rather than converted manually in the Editor.
- Replaced manual specification of file location for HRTF files with dropdown menu that is automatically populated
- Removed now obsolete test applications and created a simple sample scene to test the spatializer, hearing aid and hearing loss plugins.
- Streamlined build process:
    - Visual Studio solutions automatically copy built binaries into the BuildUnityWrapper package for x86 and x64.
    - On Mac, there is a shell script `copy_bundle_from_build_into_BuildUnityWrapperPackage.sh` which will combine iOS simulator and device binaries into a single bundle, as well as copying in the MacOS bundle into the BuildUnityWrapperPackage project.
- C++ project files updated to:
    - Visual Studio 2019 (Windows, Android)
    - Xcode 11 (MacOS, iOS)

## v1.15.0:
**General:**

- Fixed compiling error in new Unity versions

**Hearing aid simulation:**

- Fixed Fig6 button bug

## v1.14.0:
**General:**

- Uses new version of the Toolkit that fixes axes issues.
- Fixed no sound bug

## v1.13.0:
**Hearing aid simulation:**

- Added Fig6 button to Dynamic Equalizer

**Hearing loss simulation:**

- Fixed temporal distortion bug

## v1.12.0:
**General:**

- Uses new repository
- GUI now admits different HRTF and ILD files for each permitted sample rate

**Binaural Spatialization:**

- New API functions to get buffer size and sample rate
- Sample rates different than 44.1, 48 or 96kHz are not permitted

**Documentation:**

- Reference manual: added description of the new functions

## v1.11.0:
**General:**

- Added 11 .png files to Assets/3DTuneIn/Resources with images of the hearing loss clasiffication scale curves

**Hearing aid simulation:**

- Added global title in GUI: HEARING AID SIMULATION

**Hearing loss simulation:**

- Added support for HL Classification Scale in: plugin, API and GUI
- Added support for Temporal Distortion presets (in GUI and API)
- Added support for Frequency Smearing presets (in GUI and API)
- Removed audiometry presets, from GUI and API
- SetTemporalDistortionAutocorrelationFilterCutoff method renamed to SetTemporalDistortionBandwidth
- Added support for 12800Hz value in BandUpperLimit parameter of Temporal Distortion
- Split of HL GUI into Audiometry controls and HL controls
- Removed the concept of Basic and Advanced HL controls in GUI and API
- Moved calibration to global controls, in GUI and API documentation
- Changed range of dBHL levels in audiometry, from 0 to 160 dB (maximum possible with classification scale)
- Added global titles in GUI: AUDIOMETRY and HEARING LOSS SIMULATION
- Improved some error log messages     

**Documentation:**

- Reference manual: reestructuration of section 4, removing the concept of Basic vs Advanced HL API
- Reference manual: added description of HL classification scale methods and types
- Reference manual: added methods for setting HL presets in temporal distortion and frequency smearing, in HL
- Reference manual: changed example of HL API (section 4.7) after removing audiometry presets
- Quick start guide and Reference manual: changed screenshots of HL and HA GUIs

## v1.10.2: 
**Binaural spatialization:**

- Fixed bug in Toolkit core: directionality enable/disable was always affecting only to left ear
- Fixed bug causing some toggles in GUI to show values not coherent with API and plugin

**Hearing loss simulation:**

- Fixed bug of GUI styles initialization in HL editor script (when HL was alone in the scene, without HA).
- Fixed bug causing some toggles in GUI to show values not coherent with API and plugin

**Hearing aid simulation:**

- Fixed bug of consistency between GUI and API in Tone Control parameters.
- Dynamic EQ band gains are now editable in GUI text boxes
- Fixed bug causing some toggles in GUI to show values not coherent with API and plugin

## v1.10.1:
**Binaural spatialization:**

- Fixed problem in scenes created from scratch in Unity 2017, where spatialization was not initialized on Start.

**Hearing loss simulation:**

- Fixed bug: HL GUI was not shown in Audio Mixer if HA API script was not added to the scene.
- Fixed bug: HL GUI was partially shown even if HL API script was not added to the scene.
- Plugin rebuilt to fix a bug internal to the toolkit, where the frequency smearing window was not properly normalized.

**Hearing aid simulation:**

- Fixed bug: HA GUI was partially shown even if HA API script was not added to the scene.

## v1.10.0:
**General:**

- Tested in Unity 2017. 

**Hearing loss simulation:**

- New temporal distortion module, with corresponding API and GUI controls.
- New frequency smearing module, with corresponding API and GUI controls.
- New switches for enabling/disabling non-linear attenuation (multiband expander) for each ear.
- Reestructuration of HL modules, with new names, in GUI.

**Documentation:**

- Reference manual: added description of new methods and types in HL API.
- Reference manual: changed screenshots of the HL editor GUI.
- Quick start guide: changed screenshots of the HL editor GUI. 

## v1.9.0:
**General:**

- Added file Assets/3DTuneIn/Editor/Common3DTIGUI.cs with common definitions for shared look and feel of all editor GUIs.
- Added About button (with associated file Assets/3DTuneIn/Editor/About3DTI.cs) to all GUIs.
- Removed files GUIHelpers.cs and MathHelpers.cs from Assets/3DTuneIn/Editor.

**Binaural spatialization:**

- New High Performance mode, replacing HRTF convolution and near field filter with a single high performance ILD filter. 
- New No Spatialization mode, disabling HRTF convolution and all ILD filters. 
- The former spatialization mode (HRTF and near field ILD) is now called High Quality mode. 
- Included resource file "HRTF_ILD_44100.3dti-ild" (in 3DTuneIn/Data/HighPerformance/ILD) for modelling listener in High Performance mode.
- Moved resource files for High Quality mode to folder 3DTuneIn/Data/HighQuality.
- Replaced file "default.3dti-ild" (for near field ILD) with "NearFieldCompensation_ILD_44100.3dti-ild".    
- Added new method for changing spatialization mode (SetSpatializationMode), between High performance, High quality and None (see Reference Manual). 
- Modified implementation of EnableSpatialization and DisableSpatialization. These methods now do not enable/disable far LPF and distance attenuation.
- The SetModHRTF has been removed from the API. Now HRTF convolution can de disabled by calling: SetSpatializationMode(SPATIALIZATION_MODE_NONE).
- The look and feel of the editor GUI has been fully changed.
- Tooltips included in most controls of the GUI.
- Added option for calling API methods independently for each AudioSource.

**Hearing aid simulation:**

- When attaching API_3DTI_HA.cs to a game object, the only public parameter shown in Inspector is the mixer. 
- Fixed bug in HA limiter, causing saturation.
- The look and feel of the editor GUI has been fully changed.
- Tooltips included in most controls of the GUI.

**Hearing loss simulation:**

- Hearing loss simulator is redone from scratch. All API methods and GUI controls are new. 
- When attaching API_3DTI_HL.cs to a game object, the only public parameter shown in Inspector is the mixer. 
- The look and feel of the editor GUI has been fully changed.
- Tooltips included in most controls of the GUI.   

**Documentation:**

- Reference manual: added description of the three spatialization modes in section 3.1.
- Reference manual: added description of AudioSource optional parameters in section 3.1.
- Reference manual: changed description of EnableSpatialization and DisableSpatialization methods in section 3.1.
- Reference manual: added description of method SetSpatializationMode in section 3.2.
- Reference manual: added description of "source" optional parameter to all methods having it. 
- Reference manual: changed description of LoadHRTFBinary method in section 3.2.
- Reference manual: method LoadILDBinary is now called LoadILDNearFieldBinary and its description is changed in section 3.2.
- Reference manual: added description of method LoadILDHighPerformanceBinary in section 3.2.
- Reference manual: changed description of method SetCustomITD in section 3.2.
- Reference manual: method SetModILD is changed to SetModNearFieldILD in section 3.3.
- Reference manual: method SetModHRTF removed from section 3.3.
- Reference manual: changed description of method GetLimiterCompression in section 3.3.
- Reference manual: title of section 3.4 changed from "Hearing Aid Directionality" to "Directionality".
- Reference manual: improved example in section 3.5.
- Reference manual: section 4 is fully rewritten, after implementing the new HL simulator from scratch.
- Reference manual: changed example in section 5.5.
- Reference manual: changed all screenshots of the editor GUI.
- Quick start guide: changed most screenshots. 
- Quick start guide: brief explanation of the spatialization modes and new resource files in section 2. 
- Quick start guide: added description of how the spatialization plugin interacts with default AudioSource parameters in section 2.
- Quick start guide: all text is revised. 

## v1.8.0:
**General:**

- Added API_3DTI_Common.cs script with common definitions for all APIs. 
- All "int ear" parameters in all APIs are replaced with T_ear type from API_3DTI_Common.
- Simplified iOS builds with AppDelegate to avoid the need of modifying the XCode project built by Unity.
- Reorganization of Assets folder structure 
- Fixed clicks when changing filter coefficients, i.e: when changing HA LPF or HPF cutoff, when moving fast along far or near distances (from Toolkit).

**Binaural spatialization:**

- Fixed bug of crash after multiple calls to StartBinauralSpatializer. 
- Fixed bug of crash when trying to spatialize an AudioSource with no clip assigned.
- Fixed bug of crash when an audio source is inside the listener's head (now a warning is reported to the debug log).
- Improved debug log of binaural spatializer.
- Resources folder is automatically created when the first .bytes file is generated.
- Added optional limiter control, both in GUI and API.
- Changed default magnitude for anechoic distance attenuation to -3dB. 
- Added HRTF resampling step, both in GUI and API.

**Hearing aid simulation:**

- New type and constant definitions in API. 
- Method SetEQFromFig6 method now returns the list of gains calculated for each band.
- High level control in HA for EQ Tone (low, mid, high), both in GUI and API.
- Full debug log messages for HA.
- Removed dynamic EQ ON/Off switch, both from GUI and from API, now replaced with Compression Percentage control.
- Added normalization control for Dynamic EQ curves, both in GUI and API. 
- Sliders shown in HA editor GUI for EQ band gains and quantization noise bits, now show integer values.
- Added optional imiter control, both in GUI and API.
- Fixed bug related with coherency in parameters between plugin, GUI and API scripts.

**Hearing loss simulation:**

- Replaced constant definitions in API with enumerated types.
- Fixed bug in API and GUI, not handling correctly the relation between the global HL switch and the independent switches for each subprocess.
- Fixed bug related with coherency in parameters between plugin, GUI and API scripts.

**Documentation:**

- New section in Reference Manual (Section 2) explaining the common definitions of API_3DTI_Common: T_ear.
- Added new methods to Binaural Spatializer API in Reference Maunal (Section 3.3): SwithOnOffLimiter, GetLimiterCompression.
- Improved description of SetHADirectionalityExtend method in Reference Manual (Section 3.4).
- Improved example of use of Binaural Spatializer API in Reference Manual (Section 3.5). 
- Improved section in Reference Manual (Section 4.1) with updated type and constant definitions for HL: T_HLEffect, T_HLProcessChain, T_HLEQBand, EQ presets and NUM_EQ_BANDS.
- Updated example of use of HL in Reference Manual (Section 4.4).
- New section in Reference Manual (Section 5.1) explaining the type and constant definitions for HA: T_HAToneBand, T_HADynamicEQBand, T_HADynamicEQLevel, NUM_EQ_CURVES, NUM_EQ_BANDS.
- Added new Global Settings method in HA Reference Manual (Section 5.2): SwitchLimiterOnOff.
- Added new methods to Dynamic EQ in HA Reference Manual (Section 5.3): SetTone, SetCompressionPercentage, SwitchNormalizationOnOff, SetNormalizationLevel, GetNormalizationOffset. 
- Removed method SetStandardEQBandGain from HA Reference Manual (Section 5.3). 
- Improved example of use of HA API (Section 5.5).
- Removed old section 5 (Building Apps for Target Platforms) from Quick Start Guide.
- Added tips on audio clip import options to Quick Guide (new Section 5), thanks to Riccardo Braga. 

## v1.7:
- First version with full support for iOS target platform.
- All audio plugins are joined into one single plugin (audioplugin3DTIToolkit).
- Added method StartBinauralSpatializer to Spatializer API to support enabling/disabling audio sources, enabling/disabling spatialize attribute, creating new spatialized audio sources on runtime, and switching off the PlayOnAwake attribute.
- Fixed error in standalone builds in Windows, where the path of the resource files was not found (Now, .bytes resource files are created for all platforms to avoid future issues).
- Fixed wrong name of one method in HA API (SetStaticEQBandGain, now is SetStandardEQBandGain). Now, all methods have the names written in the Reference Manual. 
- Added "ILD" to the Spatializer GUI text, where appropriate.
- Fixed bug in GUI Editor scripts with potential risk of showing toggles with incorrect values.
- Reduced output from Toolkit debugger while processing (for Android logcat). 
- New section on Quick Start Guide regarding target builds.
- Fixed bug in HA GUI causing the toggles for each ear not sending appropriate commands to the plugin.
- Now all methods of the Spatializer API return a bool for checking communication errors. 
- Code cleaning of all Editor and API scripts.
- First version with this README file.

## v1.6.2:
- Fixed issue with code signing in Mac OS Sierra
- Fixed bug in creation of binary files for Spatializer in Android builds
- Cleaning of warnings from editor GUI scripts

## v1.6.1:
- Fixed bug in core with distance attenuation

## v1.6:
- Full support for Hearing Aid Simulation (both spatialization and postprocessing effects). 
- Hearing Loss Simulation for Mac OSX platform.
- Added "Write Debug Log" to GUI to help support.
- Spatializer plugin is now called "3DTi Binaural Spatializer".
- HRTF memory usage dramatically reduced.
- HL GUI code included in package.
- Added Scale Factor parameter to Spatializer

