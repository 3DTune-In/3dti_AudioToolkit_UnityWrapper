using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using API_3DTI;

namespace API_3DTI
{
    public static class AudioEffectPluginExtensions
    {
        public static T GetParameter<Parameter, T>(this IAudioEffectPlugin plugin, Parameter parameter, T_ear ear = T_ear.BOTH)
            where Parameter : Enum
        {
            var attributes = parameter.GetAttribute<ParameterAttribute>();
            if (attributes == null)
            {
                throw new Exception($"Failed to get ParameterAttribute for parameter {parameter}.");
            }
            if (typeof(T) != typeof(float))
            {
                Debug.Assert(typeof(T) == attributes.type
                    || (typeof(T) == typeof(int) && attributes.type.IsEnum), $"Get<> called with incorrect return type generic ({typeof(T)}). Parameter type is {attributes.type}.");
            }

            if (attributes.isSharedBetweenEars() && ear != T_ear.BOTH)
            {
                Debug.LogWarning($"Parameter {parameter} should be called with ear={T_ear.BOTH} as it is shared between ears");
                ear = T_ear.BOTH;
            }
            else if (!attributes.isSharedBetweenEars() && ear == T_ear.BOTH)
            {
                throw new Exception($"Cannot get parameter {parameter} for both ears. Choose wither {T_ear.LEFT} or {T_ear.RIGHT}.");
            }

            if (!plugin.GetFloatParameter(attributes.pluginName(ear), out float fValue))
            {
                return default(T);
            }
            try
            {
                if (typeof(T).IsEnum)
                {
                    return (T)(object)(int)(fValue);
                }
                else
                {
                    return (T)Convert.ChangeType(fValue, typeof(T));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error when casting value {fValue} from plugin for parameter {parameter} to requested type {typeof(T)}: {e}");
                // TODO: default(T) may be an invalid value, especially for a float, in which case returning the default might be better.
                return default(T);
            }
        }

        public static bool SetParameter<Parameter, T>(this IAudioEffectPlugin plugin, Parameter parameter, T value, T_ear ear = T_ear.BOTH)
            where Parameter : Enum
        {
            var attributes = parameter.GetAttribute<ParameterAttribute>();
            if (attributes == null)
            {
                throw new Exception($"Failed to get ParameterAttribute for parameter {parameter}.");
            }
            Debug.Assert(typeof(T) == attributes.type || (typeof(T) == typeof(int) && attributes.type.IsEnum), "Get<> called with incorrect return type generic.");
            if (attributes.type.IsEnum)
            {
                Debug.Assert(Enum.IsDefined(attributes.type, value));
            }
            

            var ears = new List<T_ear>();
            if (attributes.isSharedBetweenEars())
            {
                // Single parameter for both ears
                if (ear != T_ear.BOTH)
                {
                    Debug.LogWarning($"Parameter {parameter} cannot be set for an individual ear. It must be set with ear = {T_ear.BOTH}.");
                }
                ears.Add(T_ear.BOTH);
            }
            else
            {
                if (ear.HasFlag(T_ear.LEFT)) { ears.Add(T_ear.LEFT); }
                if (ear.HasFlag(T_ear.RIGHT)) { ears.Add(T_ear.RIGHT); }
            }

            bool success = true;
            foreach (T_ear e in ears)
            {
                float fValue = Convert.ToSingle(value);
                if (!plugin.GetFloatParameterInfo(attributes.pluginName(e), out float min, out float max, out float _))
                {
                    Debug.LogError($"Failed to get min/max for parameter {attributes.pluginName(e)} for parameter {parameter} from plugin.");
                    success = false;
                }
                if (fValue < min)
                {
                    Debug.LogWarning($"{fValue} is out of valid range [{min}, {max}] for parameter {parameter} on ear {e}. Value will be clipped.");
                    fValue = min;
                }
                else if (fValue > max)
                {
                    Debug.LogWarning($"{fValue} is out of valid range [{min}, {max}] for parameter {parameter} on ear {e}. Value will be clipped.");
                    fValue = max;
                }

                fValue = Math.Min(max, Math.Max(min, fValue));
                if (!plugin.SetFloatParameter(attributes.pluginName(e), fValue))
                {
                    Debug.LogError($"Failed to set parameter {attributes.pluginName(e)} for parameter {parameter} on plugin.");
                    success = false;
                }
            }

            return success;
        }


        public static bool GetFloatParameterInfo<Parameter>(this IAudioEffectPlugin plugin, Parameter parameter, T_ear ear, out float minRange, out float maxRange, out float defaultValue) 
            where Parameter : Enum
        {
            var attributes = parameter.GetAttribute<ParameterAttribute>();
            if (attributes == null)
            {
                throw new Exception($"Failed to get ParameterAttribute for parameter {parameter}.");
            }
            return plugin.GetFloatParameterInfo(attributes.pluginName(ear), out minRange, out maxRange, out defaultValue);
        }


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
                    return (int)defaultValue;
                }
            }
            return (int)value;
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

        public static T GetEnumParameter<T>(this IAudioEffectPlugin plugin, string parameterName) where T : System.Enum
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
}