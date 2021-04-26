using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Reflection;

public static class AudioEffectPluginExtensions
{
    public static bool GetBoolParameter(this IAudioEffectPlugin plugin, string parameterName)
    {
        if (!plugin.GetFloatParameter(parameterName, out float value))
        {
            Debug.LogError($"Failed to get parameter {parameterName} from audio plugin.");
        }
        return value != 0.0f;
    }
}