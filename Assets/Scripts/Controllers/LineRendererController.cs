using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererController : MonoBehaviour
{
    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        ConfigureLine();
    }

    private void ConfigureLine()
    {
        // Example configurations. Adjust as needed.
        lr.positionCount = 2;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.green;
        lr.endColor = Color.green;
        lr.sortingOrder = -1; // Render behind stations
    }

    // Optional: Add methods to update line appearance dynamically
}
