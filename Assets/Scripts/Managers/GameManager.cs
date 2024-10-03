using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Managers")]
    public InputManager inputManager;
    public StationManager stationManager;
    public ConnectionManager connectionManager;

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

        // Optionally, initialize managers if not using Singleton in each
    }

    private void Start()
    {
        // Additional initialization if needed
    }
}
