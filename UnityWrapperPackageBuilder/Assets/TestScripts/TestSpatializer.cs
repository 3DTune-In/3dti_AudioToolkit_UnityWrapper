using UnityEngine;
using System.Collections;

public class TestSpatializer : MonoBehaviour
{
    API_3DTI_Spatializer SpatializerAPI;

    void Start()
    {
        SpatializerAPI = Camera.main.GetComponent<API_3DTI_Spatializer>();
        SpatializerAPI.SetScaleFactor(1.0f);
    }
}

