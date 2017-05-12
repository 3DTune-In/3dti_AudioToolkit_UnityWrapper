using System.Collections.Generic;       // List
using System.Collections.ObjectModel;   // ReadOnlyCollection

namespace API_3DTI_Common
{
    //////////////////////////////////////////////////////////////
    // PUBLIC TYPE DEFINITIONS
    //////////////////////////////////////////////////////////////

    public enum T_ear
    {
        LEFT = 0,   // Left ear
        RIGHT = 1,  // Right ear
        BOTH = 2   // Both ears
                    //NONE = 3    // No ear
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