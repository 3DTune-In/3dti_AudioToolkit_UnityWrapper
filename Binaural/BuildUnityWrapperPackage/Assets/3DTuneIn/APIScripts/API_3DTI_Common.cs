using System;
using System.Collections.Generic;       // List
using System.Collections.ObjectModel;   // ReadOnlyCollection
using UnityEngine;

namespace API_3DTI_Common
{
    //////////////////////////////////////////////////////////////
    // PUBLIC TYPE DEFINITIONS
    //////////////////////////////////////////////////////////////
    

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class ParameterAttribute : System.Attribute
    {
        // For parameters that are not ear-specific, pluginNameLeft == pluginNameRight and mixerNameLeft == mixerNameRight
        public string pluginNameLeft;
        public string pluginNameRight;
        public string mixerNameLeft;
        public string mixerNameRight;
        public Type type;
        // Label used in GUI
        public string label;
        // Tooltip used in GUI
        public string description;
        // For numeric values, the units label, e.g. "dB"
        public string units;
        // For int/float parameters: limit to these discrete values. Leave as null for no limits.
        public float[] validValues;

        public bool isSharedBetweenEars()
        {
            Debug.Assert((pluginNameLeft == pluginNameRight) == (mixerNameLeft == mixerNameRight));
            return pluginNameLeft == pluginNameRight;
        }

        public string pluginName(T_ear ear)
        {
            if (!(isSharedBetweenEars() == (ear == T_ear.BOTH)))
            {
                Debug.Assert(isSharedBetweenEars() == (ear == T_ear.BOTH), $"This parameter is shared between both ears so must be called with ear=={T_ear.BOTH}");
            }
            return ear.HasFlag(T_ear.LEFT) ? pluginNameLeft : pluginNameRight;
        }

        public string mixerName(T_ear ear)
        {
            Debug.Assert(isSharedBetweenEars() == (ear == T_ear.BOTH), $"This parameter is shared between both ears so must be called with ear=={T_ear.BOTH}");
            return ear.HasFlag(T_ear.LEFT) ? mixerNameLeft : mixerNameRight;
        }

    }

    public static class EnumHelper
    {
        // https://stackoverflow.com/a/9276348
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        public static T GetAttribute<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }
    }



    //public class Parameter<T> where T : Enum
    //{
    //    //public T id;
    //    public string pluginName;
    //    public string mixerName;
    //    public Type type;
    //    public object value;
    //}

    [Flags]
    public enum T_ear
    {
        LEFT = 1,   // Left ear
        RIGHT = 2,  // Right ear
        BOTH = LEFT | RIGHT,   // Both ears
    };

    //public class T_LevelsList: List<float> { }

    //////////////////////////////////////////////////////////////
    // CLASS DEFINITIONS FOR INTERNAL USE OF THE WRAPPER
    //////////////////////////////////////////////////////////////    

    //public class CEarAPIParameter<TDataType>
    //{
    //    TDataType left;
    //    TDataType right;

    //    public CEarAPIParameter(TDataType l, TDataType r) { left = l;  right = r; }
    //    public CEarAPIParameter(TDataType v) { left = v; right = v;  }

    //    public void Set(T_ear ear, TDataType value)
    //    {
    //        switch (ear)
    //        {
    //            case T_ear.BOTH:
    //                left = value;
    //                right = value;
    //                break;
    //            case T_ear.LEFT:
    //                left = value;
    //                break;
    //            case T_ear.RIGHT:
    //                right = value;
    //                break;
    //        }
    //    }

    //    public TDataType Get(T_ear ear)
    //    {
    //        switch (ear)
    //        {
    //            case T_ear.LEFT:
    //                return left;                    
    //            case T_ear.RIGHT:
    //                return right;                    
    //        }
    //        return left;
    //    }
    //}

    //////////////////////////////////////////////////////////////
    // AUXILIARY FUNCTIONS FOR INTERNAL USE OF THE WRAPPER
    //////////////////////////////////////////////////////////////

    public static class CommonFunctions
    {
        /// <summary>
        ///  Auxiliary function
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool Float2Bool(float v)
        {
            if (v == 1.0f)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Auxiliary function
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float Bool2Float(bool v)
        {
            if (v)
                return 1.0f;
            else
                return 0.0f;
        }

       
    }
}