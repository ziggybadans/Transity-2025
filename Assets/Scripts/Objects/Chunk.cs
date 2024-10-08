using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ChunkData
{
    public Vector2Int coordinate;
    public List<CellDataEntry> cells = new List<CellDataEntry>();
}

[Serializable]
public struct CellDataEntry
{
    public Vector2Int key;
    public CellData value;
}

public class Chunk
{
    public Vector2Int coordinate;
    public int size;
    public Dictionary<Vector2Int, Cell> cells;

    // Events
    public event Action<Cell> OnCellLoaded;
    public event Action<Cell> OnCellUnloaded;

    // References to object assets
    public LandObject[] landObjects; // Assign via inspector or loader
    public StationObject[] stationObjects; // Assign via inspector or loader

    private ProceduralGenerator proceduralGenerator;
    private LandObject oceanLandObject;
    private LandObject landLandObject;
    private LandObject waterLandObject;

    // Constructor
    public Chunk(Vector2Int coord, int chunkSize)
    {
        coordinate = coord;
        size = chunkSize;
        cells = new Dictionary<Vector2Int, Cell>(size * size); // Initialize dictionary with expected capacity
        InitializeCells();
    }

    // Initializes the cells in the chunk
    private void InitializeCells()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // Calculate the world position of the cell based on chunk coordinate and size
                Vector2Int cellCoord = new Vector2Int(coordinate.x * size + x, coordinate.y * size + y);
                Cell newCell = new Cell(cellCoord);
                cells[new Vector2Int(x, y)] = newCell; // Store the cell using local coordinates
            }
        }
    }

    // Initialize Procedural Generation
    public void InitializeProceduralGeneration(ProceduralGenerator generator, LandObject ocean, LandObject land, LandObject water)
    {
        proceduralGenerator = generator;
        oceanLandObject = ocean;
        landLandObject = land;
        waterLandObject = water;
    }

    // Loads the chunk (procedurally generates or loads from persistence)
    public void Load()
    {
        if (HasSavedData())
        {
            LoadFromSavedData(); // Load chunk data from file if available
        }
        else
        {
            ProcedurallyGenerate(); // Generate chunk procedurally if no saved data exists
        }
    }


    private void ProcedurallyGenerate()
    {
        foreach (var cell in cells.Values)
        {
            // Assign a land object to the cell using ProceduralGenerator
            LandObject land = proceduralGenerator.DetermineLandObject(cell.worldPosition, oceanLandObject, landLandObject, waterLandObject);
            if (land != null)
            {
                cell.SetLandObject(land);
            }
            else
            {
                Debug.LogWarning($"No LandObject generated for cell at {cell.worldPosition} in chunk {coordinate}");
            }

            // Optionally assign other objects, such as stations
            if (ShouldHaveStation(cell.worldPosition))
            {
                StationObject station = GenerateStationObject(cell.worldPosition);
                if (station != null)
                {
                    cell.SetStationObject(station);
                }
                else
                {
                    Debug.LogWarning($"No StationObject generated for cell at {cell.worldPosition} in chunk {coordinate}");
                }
            }

            // Subscribe to cell events if needed
            cell.OnObjectAdded += HandleObjectAdded;
            cell.OnObjectRemoved += HandleObjectRemoved;

            // Notify that the cell has been loaded
            OnCellLoaded?.Invoke(cell);
        }
    }


    private void LoadFromSavedData()
    {
        string path = GetChunkSavePath();
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                ChunkData data = JsonUtility.FromJson<ChunkData>(json);
                if (data != null && data.cells != null && data.cells.Count > 0)
                {
                    LoadFromData(data);
                }
                else
                {
                    Debug.LogWarning($"Saved data for chunk {coordinate} is invalid or empty. Generating procedurally.");
                    ProcedurallyGenerate();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load chunk data from {path}: {ex.Message}");
                ProcedurallyGenerate();
            }
        }
        else
        {
            ProcedurallyGenerate();
        }
    }

    private void LoadFromData(ChunkData data)
    {
        foreach (var entry in data.cells)
        {
            Vector2Int localCoord = entry.key;
            CellData cellData = entry.value;

            if (cells.TryGetValue(localCoord, out Cell cell))
            {
                // Load land object if specified in cell data
                if (!string.IsNullOrEmpty(cellData.landObjectName))
                {
                    LandObject land = Array.Find(landObjects, lo => lo.name == cellData.landObjectName);
                    if (land != null)
                    {
                        cell.SetLandObject(land);
                    }
                    else
                    {
                        Debug.LogWarning($"LandObject named '{cellData.landObjectName}' not found for cell at {cell.worldPosition}.");
                    }
                }

                // Load station object if specified in cell data
                if (!string.IsNullOrEmpty(cellData.stationObjectName))
                {
                    StationObject station = Array.Find(stationObjects, so => so.name == cellData.stationObjectName);
                    if (station != null)
                    {
                        cell.SetStationObject(station);
                    }
                    else
                    {
                        Debug.LogWarning($"StationObject named '{cellData.stationObjectName}' not found for cell at {cell.worldPosition}.");
                    }
                }

                // Subscribe to cell events if needed
                cell.OnObjectAdded += HandleObjectAdded;
                cell.OnObjectRemoved += HandleObjectRemoved;

                // Notify that the cell has been loaded
                OnCellLoaded?.Invoke(cell);
            }
            else
            {
                Debug.LogWarning($"Cell with local coordinates {localCoord} not found in chunk {coordinate}.");
            }
        }
    }



    // Unloads the chunk, clearing its data
    public void Unload()
    {
        Save();
        foreach (var cell in cells.Values)
        {
            // Unsubscribe from cell events
            cell.OnObjectAdded -= HandleObjectAdded;
            cell.OnObjectRemoved -= HandleObjectRemoved;

            // Remove all objects
            cell.RemoveStationObject();
            cell.RemoveLandObject();

            OnCellUnloaded?.Invoke(cell);
        }
    }

    // Save the chunk data to a file
    public void Save()
    {
        ChunkData data = new ChunkData();
        data.coordinate = this.coordinate;

        // Collect data for each cell in the chunk
        foreach (var kvp in cells)
        {
            Cell cell = kvp.Value;
            CellData cellData = new CellData
            {
                worldPosition = cell.worldPosition,
                landObjectName = cell.landObject != null ? cell.landObject.name : null,
                stationObjectName = cell.stationObject != null ? cell.stationObject.name : null
                // Assign other object types as needed
            };

            CellDataEntry entry = new CellDataEntry
            {
                key = kvp.Key,
                value = cellData
            };
            data.cells.Add(entry);
        }

        string json = JsonUtility.ToJson(data);
        string path = GetChunkSavePath();

        try
        {
            // Write chunk data to file
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            // Handle exceptions during file writing
            Debug.LogError($"Failed to save chunk data to {path}: {ex.Message}");
        }
    }



    // Returns the path where the chunk data should be saved
    private string GetChunkSavePath()
    {
        string folder = Path.Combine(Application.persistentDataPath, "Chunks");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder); // Create directory if it doesn't exist
        }
        return Path.Combine(folder, $"chunk_{coordinate.x}_{coordinate.y}.json");
    }

    // Checks if there is saved data for this chunk
    public bool HasSavedData()
    {
        string path = GetChunkSavePath();
        return File.Exists(path);
    }

    // Determines whether a station should be placed in a cell
    private bool ShouldHaveStation(Vector2Int pos)
    {
        // Example logic: 10% chance of placing a station
        return UnityEngine.Random.value > 0.9f;
    }

    // Retrieves a cell within the chunk using local coordinates
    public Cell GetCell(Vector2Int localCoord)
    {
        cells.TryGetValue(localCoord, out Cell cell);
        return cell;
    }

    // Generates a land object for a given position
    private LandObject GenerateLandObject(Vector2Int pos)
    {
        if (landObjects.Length == 0) return null; // Return null if no land objects are available
        int index = UnityEngine.Random.Range(0, landObjects.Length); // Randomly select a land object
        return landObjects[index];
    }

    // Generates a station object for a given position
    private StationObject GenerateStationObject(Vector2Int pos)
    {
        if (stationObjects.Length == 0) return null; // Return null if no station objects are available
        // Use ProceduralGenerator or another method to determine station placement if needed
        int index = UnityEngine.Random.Range(0, stationObjects.Length); // Randomly select a station object
        return stationObjects[index];
    }

    // Handle object addition if needed
    private void HandleObjectAdded(Cell cell, GridObject obj)
    {
        // Implement logic when an object is added to a cell
    }

    // Handle object removal if needed
    private void HandleObjectRemoved(Cell cell, GridObject obj)
    {
        // Implement logic when an object is removed from a cell
    }
}
