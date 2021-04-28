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
            if (plugin.GetFloatParameterInfo(parameterName, out float _, out float _, out float defaultValue))
            {
                return defaultValue != 0.0f;
            }
        }
        return value != 0.0f;
    }

    public static int GetIntParameter(this IAudioEffectPlugin plugin, string parameterName)
    {
        if (!plugin.GetFloatParameter(parameterName, out float value))
        {
            Debug.LogError($"Failed to get parameter {parameterName} from audio plugin.");
            if (plugin.GetFloatParameterInfo(parameterName, out float _, out float _, out float defaultValue))
            {
                return (int) defaultValue;
            }
        }
        return (int) value;
    }

    public static float GetFloatParameter(this IAudioEffectPlugin plugin, string parameterName)
    {
        if (!plugin.GetFloatParameter(parameterName, out float value))
        {
            Debug.LogError($"Failed to get parameter {parameterName} from audio plugin.");
            if (plugin.GetFloatParameterInfo(parameterName, out float _, out float _, out float defaultValue))
            {
                return defaultValue;
            }
        }
        return value;
    }

    public static T GetEnumParameter<T>(this IAudioEffectPlugin plugin, string parameterName) where T: System.Enum
    {
        if (!plugin.GetFloatParameter(parameterName, out float f_value))
        {
            Debug.LogError($"Failed to get parameter {parameterName} from audio plugin.");
        }
        int i_value = (int)f_value;
        if (Enum.IsDefined(typeof(T), (int)f_value))
        {
            return (T)(object)(int)(f_value);
        }
        Debug.LogError($"Invalid value {f_value} for type {typeof(T).Name} received from plugin.");
        // need to set some value
        return (T)Enum.GetValues(typeof(T)).GetValue(0);
    }
}