using System.Collections.Generic;
using UnityEngine;

public class VisualManager : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject landPrefab;
    public GameObject stationPrefab;
    public GameObject linePrefab;

    // Object pools
    private ObjectPool landPool;
    private ObjectPool stationPool;
    private ObjectPool linePool;

    // Dictionaries to track active visuals
    private Dictionary<Vector2Int, GameObject> landVisuals = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> stationVisuals = new Dictionary<Vector2Int, GameObject>();
    // Add more dictionaries as needed for other components

    void Start()
    {
        landPool = new ObjectPool(landPrefab, 100);
        stationPool = new ObjectPool(stationPrefab, 50);
        linePool = new ObjectPool(linePrefab, 20);

        if (gridManager != null)
        {
            gridManager.OnCellLoaded += HandleCellLoaded;
            gridManager.OnCellUnloaded += HandleCellUnloaded;
        }
    }

    void OnDestroy()
    {
        if (gridManager != null)
        {
            gridManager.OnCellLoaded -= HandleCellLoaded;
            gridManager.OnCellUnloaded -= HandleCellUnloaded;
        }
    }

    // Handle cell loading
    private void HandleCellLoaded(Cell cell)
    {
        // Handle LandObject
        if (cell.GetLandObject() != null && !landVisuals.ContainsKey(cell.worldPosition))
        {
            GameObject land = landPool.GetObject();
            land.transform.position = new Vector3(cell.worldPosition.x, cell.worldPosition.y, 0);
            land.SetActive(true);
            // Assign sprite based on LandObject data
            LandObject landData = cell.GetLandObject();
            SpriteRenderer sr = land.GetComponent<SpriteRenderer>();
            if (sr != null && landData.landSprite != null)
            {
                sr.sprite = landData.landSprite;
            }
            landVisuals.Add(cell.worldPosition, land);
        }

        // Handle StationObject
        if (cell.HasComponent<StationObject>() && !stationVisuals.ContainsKey(cell.worldPosition))
        {
            StationObject station = cell.GetComponent<StationObject>();
            GameObject stationGO = stationPool.GetObject();
            stationGO.transform.position = new Vector3(cell.worldPosition.x, cell.worldPosition.y, 0);
            stationGO.SetActive(true);
            // Assign sprite based on StationObject data
            SpriteRenderer sr = stationGO.GetComponent<SpriteRenderer>();
            if (sr != null && station.stationSprite != null)
            {
                sr.sprite = station.stationSprite;
            }
            stationVisuals.Add(cell.worldPosition, stationGO);
        }

        // Similarly, handle other components like LineObject
    }

    // Handle cell unloading
    private void HandleCellUnloaded(Cell cell)
    {
        // Handle LandObject
        if (cell.GetLandObject() != null && landVisuals.ContainsKey(cell.worldPosition))
        {
            GameObject land = landVisuals[cell.worldPosition];
            landPool.ReturnObject(land);
            landVisuals.Remove(cell.worldPosition);
        }

        // Handle StationObject
        if (cell.HasComponent<StationObject>() && stationVisuals.ContainsKey(cell.worldPosition))
        {
            GameObject station = stationVisuals[cell.worldPosition];
            stationPool.ReturnObject(station);
            stationVisuals.Remove(cell.worldPosition);
        }

        // Similarly, handle other components like LineObject
    }
}
