using UnityEngine;

public class CircleInit : MonoBehaviour
{
    public int vertexCount = 40;
    public float lineWidth = 0.2f;
    public float radius;
    public bool circleFillScreen;
    public float oneMeterRadius = 0.5f;

    float kNormalization;

    //private LineRenderer lineRenderer;
    public GameObject circle1;
    public GameObject circle2;
    public GameObject circle10;
    public GameObject circle100;

    private void Awake()
    {        
        SetupCircleMaximum();                  
    }

    private void Start()
    {
        circle100.GetComponent<CircleDraw>().SetupCircle(radius, lineWidth, vertexCount);
        circle10.GetComponent<CircleDraw>().SetupCircle(kNormalization + oneMeterRadius, lineWidth, vertexCount);
        circle2.GetComponent<CircleDraw>().SetupCircle(CalculateNormalizeDistance(2), lineWidth, vertexCount);
        circle1.GetComponent<CircleDraw>().SetupCircle(oneMeterRadius, lineWidth, vertexCount);
    }


    private void SetupCircleMaximum()
    {
        if (circleFillScreen)
        {
            radius = Vector3.Distance(Camera.main.ScreenToWorldPoint(new Vector3(0f, Camera.main.pixelRect.yMax, 0f)), Camera.main.ScreenToWorldPoint(new Vector3(0f, Camera.main.pixelRect.yMin, 0f))) * oneMeterRadius - lineWidth;
            kNormalization = (radius - oneMeterRadius) * oneMeterRadius;    //Calculated in order to this circle represents 100meters       
        }        
    }
    
    public float CalculateDistance(float distanceNormalize)
    {
        return Mathf.Pow(10, ((distanceNormalize -oneMeterRadius) / kNormalization));
    }

    public float CalculateNormalizeDistance(float distance)
    {
        return kNormalization * Mathf.Log10(distance) + oneMeterRadius;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        float deltaTheta = (2f * Mathf.PI) / vertexCount;
        float theta = 0f;

        Vector3 oldPos = Vector3.zero;
        for (int i=0; i < vertexCount+1; i++)
        {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), radius*Mathf.Sin(theta), 0f);
            Gizmos.DrawLine(oldPos, transform.position + pos);
            oldPos = transform.position + pos;

            theta += deltaTheta;
        }
    }

#endif
}
