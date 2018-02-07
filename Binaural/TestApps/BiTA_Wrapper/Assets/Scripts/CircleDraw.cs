using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class CircleDraw : MonoBehaviour
{

    private int vertexCount;
    private float lineWidth;
    private float radius;
    

    private LineRenderer lineRenderer;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();        
    }

    public void SetParameters()
    {

    }

    public void SetupCircle(float _radius, float _lineWidth, int _vertexCount)
    {
        radius = _radius;
        lineWidth = _lineWidth;
        vertexCount = _vertexCount;


        lineRenderer.widthMultiplier = lineWidth;

        float deltaTheta = (2f * Mathf.PI) / vertexCount;
        float theta = 0f;

        lineRenderer.positionCount = vertexCount;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta), 0f);
            lineRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }    
}
