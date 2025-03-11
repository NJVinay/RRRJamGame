using UnityEngine;

public class MG_Rectangle : MonoBehaviour
{
    public int width;
    public int height;
    public Color color = Color.green;
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    void Update()
    {
        DrawRectangle();
    }

    void DrawRectangle()
    {
        Vector3[] corners = new Vector3[4]
        {
            new Vector3(transform.position.x -width /2, transform.position.y -height/2, 0),
            new Vector3(transform.position.x +width /2, transform.position.y -height/2, 0),
            new Vector3(transform.position.x +width /2, transform.position.y +height/2, 0),
            new Vector3(transform.position.x -width /2, transform.position.y +height/2, 0)            
        };

        lineRenderer.SetPositions(corners);
    }
}
