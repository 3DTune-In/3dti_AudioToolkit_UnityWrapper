# Unity Wrapper for 3D-Tune-In Toolkit

Date: January 2021
Version: 2.0
Authors (version 1): 3DI-DIANA Research Group (M. Cuevas-Rodriguez, E.J. de la Rubia-Cuestas, C. Garre-del Olmo, D. GonzalezToledo, L. Molina-Tanco, A. Reyes-Lecuona)
Authors (version 2): Tim Murray-Browne, Lorenzo Picinali (Imperial College London)

## Introduction

The 3D-Tune-In Toolkit (3DTi Toolkit) consists in a set of C++ libraries and resources providing solutions for 3D audio spatialization and simulation of hearing loss and hearing aids. The main components of the 3DTi Toolkit are:

- Binaural anechoic spatializer
- Binaural environment simulation
- Loudspeakers anechoic spatializer
- Loudspeakers environment simulation
- Hearing loss simulation
- Hearing aid simulation
- Resource files (HRTF, ILD, BRIR)
- Resource management package

The Unity Wrapper of the 3DTi Toolkit (3DTi Unity Wrapper) allows integration of the different components of the Toolkit in any Unity Scene. These components are packed in the form of a Unity Package requiring Unity 2019.4 or above. The current version of the package is built to support the following platforms:

- As Host: Microsoft Windows 10, MacOS.
- As Target: Microsoft Windows x64, MacOS, Android (4.4 or above), iOS. (We also include binaries for Windows x86 but they are not tested on a 32 bit computer so are not officially supported.)

In this version, the follow parts of the 3DTI Toolkit are made available to a Unity application:

- Binaural anechoic spatializer
- Hearing loss simulation
- Hearing aid simulation

## Usage instructions

The toolkit is distributed as a Unity package. It should be imported into your Unity project using the menu command *Assets > Import Package > Custom Package...*

### Binaural spatializer

To use the spatializer in your project, the *API_3DTI_Spatializer* component should be added to your scene. We recommend adding it to your _Main Camera_ object so it is present in your scene throughout. Next, go to *Edit > Project Settings*. In the Project Settings, under *Audio* choose *3DTI Binaural Spatializer* for the *Spatializer* option.

This should be enough for the spatializer to work on standard Unity audio sources with its default settings.

In the Inspector, the *API_3DTI_Spatializer* component provides further options, including menus to select the binary resources being used such as the different Head-related Transfer Functions (HRTFs) that are included within the package.

For further information about these additional options, please refer to the 3DTI Toolkit documentation.

Due to limitations in the Unity spatializer audio API, it's only possible for our component to pass its settings to the plugin when an audio source is created. As a workaround for this, the *API_3DTI_Spatializer* component will create a silent audio source when the scene loads giving it an opportunity to send over the plugin's settings.

### Hearing Aid and Hearing Loss simulators

Hearing Aid (HA) and Hearing Loss (HL) are implemented as standard Unity native audio plugins. These are found on the mixer in the Audio window of your project. In order to use them, you also need to add the relevant components *API_3DTI_HL* and *API_3DTI_HA* to your project. Note that HA depends on HL so if you want to use HA you need to add the HL component as well. You can add these comoponents anywhere to your hierarchy but they must be present at the moment the scene starts so we recommend also adding them to your _Main Camera_ object. You should then set the mixer property of each of these components to the mixer that has the HA and HL effects attached.

**NOTE**: Although you can add these effects to any mixer, we strongly recommend for now that you use the included *3DTI_HAHL_Mixer*. This is because the design of the Unity audio plugin interface makes it difficult to control parameters from C# code without them having been manually 'Exposed'. The included mixer has the parameters of the HA and HL effects already exposed. It is simplest to modify the HA and HL parameters using the Inspector panel in the Editor. However, if you want to modify these parameters from a C# script then the *API_3DTI_HL* and *API_3DTI_HA* components include some methods to help with this. These methods assume that you are using the *3DTI_HAHL_Mixer* as our C# calls refer to the names of the parameters exposed within the mixer.

Please note that the methods within the *API_3DTI_HL* and *API_3DTI_HA* components may change in a future version. The current approach works but is difficult to maintain or update so we are currently exploring alternative approaches.

### Notes on using within an iOS app

The package includes binary files for the four supported end platforms (MacOS, iOS, Android, Windows). These files should be automatically set to be bundled with the appropriate builds when imported but you can double check this by finding the relevant binary files in the Project file browser under *Assets/Plugins/* and then finding the subfolder referring to your platform (*x86* is Windows 32 bit, *x86_64* is Windows 64 bit). Somewhere within each of these subfolders is the single file that is the plugin binary for this platform. If you click on the binary file while Unity is set to build for that platform then you should see in the inspector checkboxes corresponding to the appropriate platforms.

![Inspector view showing options for the binary file](images/unity-inspector-for-plugin-binary-file.png)

In our testing, we have found one quirk when combining the toolkit in a Unity VR application on iOS that depends on `libvrunity.a`. There appears to be a conflict between two possible audio engines that can cause no audio to appear on the iOS application. This can be fixed within your app's Xcode project which is automatically created by Unity as part of the process of building for iOS. In the project settings under the tab *Build Phases* you should see *libaudioplugin3DTIToolkit.a* (our binary file) but also *libvrunity.a* (part of Unity's VR system). If you do then you need to ensure the files are ordered so that *libaudioplugin3DTIToolkit.a* appears before *libvrunity.a* on this list.

## Build instructions

The reposity includes the binaries for each platform ready built so it is not necessary to build them yourself.

Each platform contains a separate IDE project to build the native binary files for the toolkit under the *Binaural/audioplugin3DTIToolkit* folder. For MacOS and iOS this is an Xcode project desigend to run on MacOS. For Windows and Android it is a Visual Studio solution. These projects refer to a common C++ codebase within the *src* subfolder, as well as the toolkit code from the 3dti_AudioToolkit submodule.

Each project will output the binary plugin for its own platform. These binaries should be copied into the appropriate location within the *BuildUnityWrapperPackage/Assets/Plugins* folder.

An exception is iOS. So that the plugin works both in the iOS simluator and also on the device, two different versions need to be built. You can select these two different versions within Xocde in the Device dropdown menu in the toolbar. You should build once with _Generic iOS Device_ selected and once with a simulator selected. This will result in two bundles being created. There is then a shell script in the Binaural folder which will combine both of these into a single bundle and copy it into the Plugins folder. For convenience it will also copy the MacOS bundle as well (it doesn't copy Windows/Android as the script is assumed just to be run when you're working on a Mac).

Once all of the binaries are updated, you can then export the BuildUnityWrapperPackage project by opening it in Unity and choosing from the menu *Assets > Export package*.

## Known issues

An issue has been reported if your locale uses a comma ',' instead of a period '.' as a decimal separator. Please take care when modifying the parameters that they end up at the values you inteded.

## Future plans

This wrapper currently only integrates the 3DTI Toolkit release from 2018 'M20181003'. A future release is planned which will integrate updates that have happened to the toolkit since then.

I'm also exploring if it's possible to eliminate the need to have *API_3DTI_HL* and *API_3DTI_HA* components, and to see if there's a simpler way to directly set parameters on these plugins from C# code without relying on manually exposing each parameter within the included *3DTI_HAHL_Mixer*. 