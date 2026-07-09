using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrappleRange : MonoBehaviour
{
    public float radius = 1f;
    public int segments = 50;
    private LineRenderer line;

    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();
        DrawCircle();
    }

    void DrawCircle()
    {
        line.positionCount = segments + 1;
        line.useWorldSpace = false; // Use local space so you can move the object

        float angle = 0f;
        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius; // Use Z for 3D space
            Vector3 position = new Vector3(x, 0, z); // Adjust Y as needed for plane orientation

            line.SetPosition(i, position);
            angle += (360f / segments);
        }
        line.SetPosition(segments, line.GetPosition(0)); // Close the circle
    }
}
