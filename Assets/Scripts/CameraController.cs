using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public Vector2 panLimitMin;
    public Vector2 panLimitMax;

    public float scrollSpeed = 20f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 pos = transform.position;

        // Panning with keyboard
        if (Input.GetKey("w") || (Input.mousePosition.y >= Screen.height - panBorderThickness && Input.mousePosition.y <= Screen.height))
        {
            pos.y += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("s") || (Input.mousePosition.y <= panBorderThickness && Input.mousePosition.y >= 0))
        {
            pos.y -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("d") || (Input.mousePosition.x >= Screen.width - panBorderThickness && Input.mousePosition.x <= Screen.width))
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("a") || (Input.mousePosition.x <= panBorderThickness && Input.mousePosition.x >= 0))
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        // Clamp panning within limits
        pos.x = Mathf.Clamp(pos.x, panLimitMin.x, panLimitMax.x);
        pos.y = Mathf.Clamp(pos.y, panLimitMin.y, panLimitMax.y);

        // Zooming with mouse scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * scrollSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }

        transform.position = pos;
    }
}