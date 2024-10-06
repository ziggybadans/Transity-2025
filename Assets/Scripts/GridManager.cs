using System.Collections.Generic;
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

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        UpdateCameraDimensions();
    }

    void Update()
    {
        UpdateCameraDimensions();
        Vector2 cameraPosition = mainCamera.transform.position;
        Vector2Int currentChunkCoord = GetChunkCoordinate(cameraPosition);
        LoadChunksAround(currentChunkCoord);
        UnloadChunksOutside(currentChunkCoord);
    }

    private void UpdateCameraDimensions()
    {
        if (mainCamera.orthographic)
        {
            cameraHalfHeight = mainCamera.orthographicSize;
            cameraHalfWidth = cameraHalfHeight * mainCamera.aspect;
        }
        else
        {
            cameraHalfHeight = mainCamera.orthographicSize;
            cameraHalfWidth = cameraHalfHeight * mainCamera.aspect;
        }
    }

    private Vector2Int GetChunkCoordinate(Vector2 position)
    {
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
        List<Vector2Int> chunksToUnload = new List<Vector2Int>(activeChunks.Keys);

        foreach (var chunkCoord in chunksToUnload)
        {
            if (Mathf.Abs(chunkCoord.x - center.x) > loadRadius || Mathf.Abs(chunkCoord.y - center.y) > loadRadius)
            {
                Chunk chunk = activeChunks[chunkCoord];
                chunk.OnCellLoaded -= HandleCellLoaded;
                chunk.OnCellUnloaded -= HandleCellUnloaded;
                chunk.Unload();
                activeChunks.Remove(chunkCoord);
            }
        }
    }

    // Event Handlers
    private void HandleCellLoaded(Cell cell)
    {
        OnCellLoaded?.Invoke(cell);
    }

    private void HandleCellUnloaded(Cell cell)
    {
        OnCellUnloaded?.Invoke(cell);
    }

    // Retrieves a cell at a specific world position
    public Cell GetCellAtPosition(Vector3 position)
    {
        Vector2Int chunkCoord = GetChunkCoordinate(position);
        if (activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
        {
            Vector2Int localCoord = new Vector2Int(
                Mathf.FloorToInt(position.x) - chunkCoord.x * chunkSize,
                Mathf.FloorToInt(position.y) - chunkCoord.y * chunkSize
            );
            return chunk.GetCell(localCoord);
        }
        return null;
    }

    // Draws the grid using Gizmos for visualization in the editor
    private void OnDrawGizmos()
    {
        if (activeChunks != null)
        {
            foreach (var chunk in activeChunks.Values)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(new Vector3(chunk.coordinate.x * chunkSize + chunkSize / 2.0f, chunk.coordinate.y * chunkSize + chunkSize / 2.0f, 0), new Vector3(chunkSize, chunkSize, 1));

                foreach (var cell in chunk.cells)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(new Vector3(cell.Value.worldPosition.x, cell.Value.worldPosition.y, 0), Vector3.one);
                }
            }
        }
    }
}