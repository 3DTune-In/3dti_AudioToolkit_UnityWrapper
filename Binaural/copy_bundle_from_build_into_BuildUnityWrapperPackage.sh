#!/bin/bash

# MacOS and iOS/iOS-Simulator binaries are built on a Mac. Once you have built them, use this script (on a Mac) to copy them into the "BuildUnityWrapperPackage" project.
# Ensure you are building in Release mode.
echo "Copying MacOS binaries into BuildUnityWrapperPackage Unity project..."
cp -r audioplugin3DTIToolkit/MACOSX/build/Release/AudioPlugin3DTIToolkit.bundle BuildUnityWrapperPackage/Assets/Plugins/MacOS/AudioPlugin3DTIToolkit.bundle
# Merging both iOS and iOS-Simulator binaries. If this command fails, please make sure you've built the iOS project for both "Generic iOS Device" and then again for a Simulator device.
echo "Merging iOS and iOS-Simulator binaries into a single file in BuildUnityWrapperPackage project..."
lipo -create audioplugin3DTIToolkit/iOS/build/Release-iphoneos/libaudioplugin3DTIToolkit.a audioplugin3DTIToolkit/iOS/build/Release-iphonesimulator/libaudioplugin3DTIToolkit.a -output BuildUnityWrapperPackage/Assets/Plugins/iOS/libaudioplugin3DTIToolkit.a