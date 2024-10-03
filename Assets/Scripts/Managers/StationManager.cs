using UnityEngine;
using System.Collections.Generic;

public class StationManager : MonoBehaviour
{
    public static StationManager Instance;

    [Header("Prefabs")]
    public GameObject stationPrefab;

    private List<Station> stations = new List<Station>();

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
        InputManager.Instance.OnInputBegan += HandleInputBegan;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnInputBegan -= HandleInputBegan;
    }

    private void HandleInputBegan(Vector2 position, InputManager.InputType inputType)
    {
        // Check if the user clicked/tapped on an existing station
        Collider2D hit = Physics2D.OverlapPoint(position);
        if (hit != null && hit.CompareTag("Station"))
        {
            Station selected = hit.GetComponent<Station>();
            if (selected != null)
            {
                ConnectionManager.Instance.StartConnection(selected);
            }
        }
        else
        {
            // Place a new station
            PlaceStation(position);
        }
    }

    public Station PlaceStation(Vector2 position)
    {
        GameObject stationObj = Instantiate(stationPrefab, position, Quaternion.identity);
        Station station = stationObj.GetComponent<Station>();
        if (station != null)
        {
            stations.Add(station);
            return station;
        }
        else
        {
            Debug.LogError("Station Prefab does not have a Station component.");
            return null;
        }
    }

    public List<Station> GetAllStations()
    {
        return stations;
    }
}
