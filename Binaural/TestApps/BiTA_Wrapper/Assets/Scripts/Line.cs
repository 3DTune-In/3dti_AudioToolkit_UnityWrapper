using UnityEngine;

public class Line : MonoBehaviour
{

    public float lineWidth = 0.05f;
    public string sortingLayer;

    public CircleInit circleInit;
    public Transform dragElement;    
    public Transform source;
    public Transform center;
    private LineRenderer lineRenderer;

    public void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        dragElement.position = new Vector3(-circleInit.CalculateNormalizeDistance(2f), 0f, 0f);
        //sourceSphere.position = new Vector3(-circleInit.radius, 0f, 0f);        
        source.position = dragElement.position;        

        lineRenderer.SetPosition(0, dragElement.position);
        lineRenderer.SetPosition(1, center.position);
        lineRenderer.enabled = true;        
    }

    private void Update()
    {
        //if (Input.GetMouseButton(0))
        //{
        //    SetupSource(Input.mousePosition, true);
        //}       
    }

    public void SetSource(Vector3 position)
    {
        lineRenderer.enabled = true;                    
        dragElement.position = position;
        lineRenderer.SetPosition(0, position);
        source.position = PlaceAudioSource(position);        
    }

    private void SetupSource(Vector3 position, bool isLeft)
    {
        lineRenderer.enabled = true;

        if (isLeft)
        {
            //Vector3 leftPos = PlaceOnCircle(position);
            Vector3 leftPos = PlaceOnMouse(position);
            dragElement.position = leftPos; 
            lineRenderer.SetPosition(0, leftPos);

            source.position = PlaceAudioSource(leftPos);

        }
        else
        {
            //Vector3 rightPos = PlaceOnCircle(position);
            //center.position = rightPos;
            //lineRenderer.SetPosition(1, rightPos);
        }
    }

    private Vector3 PlaceOnCircle(Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        Vector3 pos = ray.GetPoint(0f);

        pos = transform.InverseTransformPoint(pos);
        float angle = Mathf.Atan2(pos.x, pos.y) * Mathf.Rad2Deg;
        pos.x = circleInit.radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        pos.y = circleInit.radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        pos.z = 0f;

        return pos;
    }

    private Vector3 PlaceOnMouse(Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        Vector3 pos = ray.GetPoint(0f);
        pos = transform.InverseTransformPoint(pos);       
        return pos;
    }

    private Vector3 PlaceAudioSource(Vector3 position)
    {
        //Ray ray = Camera.main.ScreenPointToRay(position);
        //Vector3 pos = ray.GetPoint(0f);

        //pos = transform.InverseTransformPoint(pos);
        float angle = Mathf.Atan2(position.x, position.y) * Mathf.Rad2Deg;


        float radius = circleInit.CalculateDistance(Vector3.Distance(position, center.position));

        //Debug.Log(radius);

        position.x = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        position.y = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        position.z = 0f;

        return position;
    }

}
