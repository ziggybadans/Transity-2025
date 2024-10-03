using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    [Header("Prefabs")]
    public LineRenderer linePrefab;

    private Station startStation;
    private LineRenderer currentLine;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        InputManager.Instance.OnInputMoved += HandleInputMoved;
        InputManager.Instance.OnInputEnded += HandleInputEnded;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnInputMoved -= HandleInputMoved;
        InputManager.Instance.OnInputEnded -= HandleInputEnded;
    }

    public void StartConnection(Station station)
    {
        if (currentLine != null)
        {
            // Already connecting
            CancelConnection();
        }

        startStation = station;
        startStation.Select();

        currentLine = Instantiate(linePrefab);
        currentLine.positionCount = 2;
        currentLine.SetPosition(0, startStation.transform.position);
        currentLine.SetPosition(1, startStation.transform.position);
    }

    private void HandleInputMoved(Vector2 position, InputManager.InputType inputType)
    {
        if (currentLine != null)
        {
            currentLine.SetPosition(1, position);
        }
    }

    private void HandleInputEnded(Vector2 position, InputManager.InputType inputType)
    {
        if (currentLine != null)
        {
            // Check if released over another station
            Collider2D hit = Physics2D.OverlapPoint(position);
            if (hit != null && hit.CompareTag("Station"))
            {
                Station endStation = hit.GetComponent<Station>();
                if (endStation != null && endStation != startStation)
                {
                    // Finalize connection
                    currentLine.SetPosition(1, endStation.transform.position);
                    // Optionally, store the connection data
                    // Example: startStation.AddConnection(endStation);
                }
                else
                {
                    // Invalid connection (same station)
                    Destroy(currentLine.gameObject);
                }
            }
            else
            {
                // Not released over a station
                Destroy(currentLine.gameObject);
            }

            // Deselect the start station
            startStation.Deselect();
            startStation = null;
            currentLine = null;
        }
    }

    public void CancelConnection()
    {
        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }
        if (startStation != null)
        {
            startStation.Deselect();
            startStation = null;
        }
    }
}
