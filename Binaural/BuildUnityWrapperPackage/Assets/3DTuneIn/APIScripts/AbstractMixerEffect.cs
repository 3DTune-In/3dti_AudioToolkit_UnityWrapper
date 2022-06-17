
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;   // For ReadOnlyCollection
using API_3DTI;
using System;


namespace API_3DTI
{
    public class AbstractMixerEffect : MonoBehaviour
    {
        protected bool _SetParameter<Parameter, T>(AudioMixer mixer, Parameter p, T value, T_ear ear = T_ear.BOTH) where T : IConvertible where Parameter : Enum
        {
            ParameterAttribute attributes = p.GetAttribute<ParameterAttribute>();
            Debug.Assert(value.GetType() == attributes.type);
            Debug.Assert(!attributes.isReadOnly);
            if (attributes.isReadOnly)
            {
                Debug.LogWarning($"Parameter {p} is read-only.");
                return false;
            }

            if (attributes.isSharedBetweenEars())
            {
                // Single parameter for both ears
                if (ear != T_ear.BOTH)
                {
                    Debug.LogWarning($"Parameter {p} cannot be set for an individual ear. It must be set with ear == {T_ear.BOTH}.");
                    ear = T_ear.BOTH;
                }
            }

            if (ear.HasFlag(T_ear.LEFT))
            {
                if (!mixer.SetFloat(attributes.mixerNameLeft, Convert.ToSingle(value)))
                {
                    Debug.LogError($"Failed to set parameter {attributes.mixerNameLeft} on mixer {mixer}", this);
                    return false;
                }
            }
            if (ear.HasFlag(T_ear.RIGHT))
            {
                if (!mixer.SetFloat(attributes.mixerNameRight, Convert.ToSingle(value)))
                {
                    Debug.LogError($"Failed to set parameter {attributes.mixerNameRight} on mixer {mixer}", this);
                    return false;
                }
            }
            return true;
        }

        protected T _GetParameter<Parameter, T>(AudioMixer mixer, Parameter p, T_ear ear) where Parameter : Enum
        {
            ParameterAttribute attributes = p.GetAttribute<ParameterAttribute>();
            Debug.Assert(typeof(T) == attributes.type);

            if (attributes.isSharedBetweenEars() && ear != T_ear.BOTH)
            {
                Debug.LogWarning($"Parameter {p} cannot be retrieved for an individual ear. It must be retrieved with ear == {T_ear.BOTH}.");
                ear = T_ear.BOTH;
            }
            else if (!attributes.isSharedBetweenEars() && ear == T_ear.BOTH)
            {
                throw new Exception($"Cannot get parameter {p} for both ears. Choose wither {T_ear.LEFT} or {T_ear.RIGHT}.");
            }

            float fValue;
            string mixerName = attributes.mixerName(ear);
            if (!mixer.GetFloat(mixerName, out fValue))
            {
                Debug.LogError($"Failed to get parameter {mixerName} from mixer {mixer}", this);
                return default(T);
            }

            return (T)Convert.ChangeType(fValue, typeof(T));
        }
    }
}
