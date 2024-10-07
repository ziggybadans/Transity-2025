using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int chunkSize = 100; // Number of cells per chunk side
    public int loadRadius = 2;  // Number of chunks to load around the camera

    // Dictionary to store active chunks with their coordinates as the key
    public Dictionary<Vector2Int, Chunk> activeChunks = new Dictionary<Vector2Int, Chunk>();

    // Reference to the main camera to determine the current position
    public Camera mainCamera;

    private float cameraHalfWidth;
    private float cameraHalfHeight;

    // Events for visual updates
    public event System.Action<Cell> OnCellLoaded;
    public event System.Action<Cell> OnCellUnloaded;

    // Parent objects for organizing the hierarchy
    public Transform landParent;
    public Transform stationParent;
    // Add parents for other object types as needed

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        InitializeParents();

        // Force an initial load of chunks
        Vector2 cameraPosition = mainCamera.transform.position;

        InitializeObjectArrays();
        StartCoroutine(InitialChunkLoad());
    }

    private void InitializeObjectArrays()
    {
        LandObject[] landObjects = LoadLandObjects();
        StationObject[] stationObjects = LoadStationObjects();

        foreach (var chunk in activeChunks.Values)
        {
            chunk.landObjects = landObjects;
            chunk.stationObjects = stationObjects;
        }
    }

    private IEnumerator InitialChunkLoad()
    {
        yield return new WaitForSeconds(0.1f); // Small delay to ensure everything is initialized
        Vector2 cameraPosition = mainCamera.transform.position;
        Vector2Int currentChunkCoord = GetChunkCoordinate(cameraPosition);
        LoadChunksAround(currentChunkCoord);
    }


    void Update()
    {
        // Update camera dimensions for determining visible area
        UpdateCameraDimensions();
        Vector2 cameraPosition = mainCamera.transform.position;
        Vector2Int currentChunkCoord = GetChunkCoordinate(cameraPosition);
        LoadChunksAround(currentChunkCoord);
        UnloadChunksOutside(currentChunkCoord);
    }

    private void InitializeParents()
    {
        // Create parent GameObjects to organize land and station objects in the hierarchy
        if (landParent == null)
        {
            landParent = new GameObject("Land").transform;
            landParent.parent = this.transform;
        }
        if (stationParent == null)
        {
            stationParent = new GameObject("Stations").transform;
            stationParent.parent = this.transform;
        }
        // Initialize other parents similarly
    }

    private void UpdateCameraDimensions()
    {
        // Calculate half of the camera's width and height based on its orthographic size
        cameraHalfHeight = mainCamera.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * mainCamera.aspect;
    }

    private Vector2Int GetChunkCoordinate(Vector2 position)
    {
        // Calculate the chunk coordinates based on the world position
        int x = Mathf.FloorToInt(position.x / chunkSize);
        int y = Mathf.FloorToInt(position.y / chunkSize);
        return new Vector2Int(x, y);
    }

    private void LoadChunksAround(Vector2Int center)
    {
        for (int x = center.x - loadRadius; x <= center.x + loadRadius; x++)
        {
            for (int y = center.y - loadRadius; y <= center.y + loadRadius; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                if (!activeChunks.ContainsKey(coord))
                {
                    Chunk newChunk = new Chunk(coord, chunkSize);

                    newChunk.landObjects = LoadLandObjects();
                    newChunk.stationObjects = LoadStationObjects();

                    newChunk.OnCellLoaded += HandleCellLoaded;
                    newChunk.OnCellUnloaded += HandleCellUnloaded;

                    newChunk.Load();
                    activeChunks.Add(coord, newChunk);
                }
            }
        }
    }


    private void UnloadChunksOutside(Vector2Int center)
    {
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();

        foreach (var chunkCoord in activeChunks.Keys)
        {
            if (Mathf.Abs(chunkCoord.x - center.x) > loadRadius || Mathf.Abs(chunkCoord.y - center.y) > loadRadius)
            {
                chunksToRemove.Add(chunkCoord);
            }
        }

        foreach (var chunkCoord in chunksToRemove)
        {
            Chunk chunk = activeChunks[chunkCoord];
            chunk.Unload();

            chunk.OnCellLoaded -= HandleCellLoaded;
            chunk.OnCellUnloaded -= HandleCellUnloaded;

            activeChunks.Remove(chunkCoord);
        }
    }


    private LandObject[] LoadLandObjects()
    {
        LandObject[] objects = Resources.LoadAll<LandObject>("LandObjects");
        return objects;
    }

    private StationObject[] LoadStationObjects()
    {
        StationObject[] objects = Resources.LoadAll<StationObject>("StationObjects");
        return objects;
    }


    // Event Handlers
    private void HandleCellLoaded(Cell cell)
    {
        OnCellLoaded?.Invoke(cell);
        RenderCell(cell);
    }

    private void HandleCellUnloaded(Cell cell)
    {
        OnCellUnloaded?.Invoke(cell);
        DestroyCellVisuals(cell);
    }


    // Renders the cell's objects visually
    private void RenderCell(Cell cell)
    {
        // Render Land Object
        if (cell.landObject != null && cell.landObject.prefab != null)
        {
            // Instantiate the land object prefab at the cell's world position and parent it to landParent
            GameObject landGO = Instantiate(cell.landObject.prefab, new Vector3(cell.worldPosition.x, cell.worldPosition.y, 0), Quaternion.identity, landParent);
            landGO.name = $"Land_{cell.worldPosition}";
            cell.instantiatedObjects.Add(landGO);
        }

        // Render Station Object
        if (cell.stationObject != null && cell.stationObject.prefab != null)
        {
            // Instantiate the station object prefab at the cell's world position and parent it to stationParent
            GameObject stationGO = Instantiate(cell.stationObject.prefab, new Vector3(cell.worldPosition.x, cell.worldPosition.y, 0), Quaternion.identity, stationParent);
            stationGO.name = $"Station_{cell.worldPosition}";
            cell.instantiatedObjects.Add(stationGO);
        }

        // Render other object types similarly
    }

    // Destroys the visual representations of the cell's objects
    private void DestroyCellVisuals(Cell cell)
    {
        // Destroy each instantiated object in the cell
        foreach (var obj in cell.instantiatedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        // Clear the list of instantiated objects
        cell.instantiatedObjects.Clear();
    }

    // Retrieves a cell at a specific world position
    public Cell GetCellAtPosition(Vector3 position)
    {
        // Determine which chunk the position belongs to
        Vector2Int chunkCoord = GetChunkCoordinate(position);
        if (activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
        {
            // Calculate local coordinates within the chunk
            Vector2Int localCoord = new Vector2Int(
                Mathf.FloorToInt(position.x) - chunkCoord.x * chunkSize,
                Mathf.FloorToInt(position.y) - chunkCoord.y * chunkSize
            );
            // Retrieve the cell from the chunk using local coordinates
            return chunk.GetCell(localCoord);
        }
        return null;
    }

    // Draws the grid using Gizmos for visualization in the editor
    private void OnDrawGizmos()
    {
        if (activeChunks != null)
        {
            // Draw each chunk as a blue wireframe box
            foreach (var chunk in activeChunks.Values)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(new Vector3(chunk.coordinate.x * chunkSize + chunkSize / 2.0f, chunk.coordinate.y * chunkSize + chunkSize / 2.0f, 0), new Vector3(chunkSize, chunkSize, 1));

                // Draw each cell within the chunk as a green wireframe box
                foreach (var cell in chunk.cells)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(new Vector3(cell.Value.worldPosition.x, cell.Value.worldPosition.y, 0), Vector3.one);
                }
            }
        }
    }
}