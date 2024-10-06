using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Vector2Int coordinate;
    public int size;
    public Dictionary<Vector2Int, Cell> cells = new Dictionary<Vector2Int, Cell>();

    // Events
    public event Action<Cell> OnCellLoaded;
    public event Action<Cell> OnCellUnloaded;

    // Constructor
    public Chunk(Vector2Int coord, int chunkSize)
    {
        coordinate = coord;
        size = chunkSize;
        InitializeCells();
    }

    // Initializes the cells in the chunk
    private void InitializeCells()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int cellCoord = new Vector2Int(coordinate.x * size + x, coordinate.y * size + y);
                Cell newCell = new Cell(cellCoord);
                cells[new Vector2Int(x, y)] = newCell;
            }
        }
    }

    // Loads the chunk (procedurally generates or loads from persistence)
    public void Load()
    {
        foreach (var cell in cells.Values)
        {
            if (UnityEngine.Random.value > 0.8f)
            {
                cell.SetLandObject(new LandObject());
            }
            OnCellLoaded?.Invoke(cell);
        }
    }

    // Unloads the chunk, clearing its data
    public void Unload()
    {
        foreach (var cell in cells.Values)
        {
            OnCellUnloaded?.Invoke(cell);
            cell.SetLandObject(null); // Remove the land object if any
        }
    }

    // Retrieves a cell within the chunk
    public Cell GetCell(Vector2Int localCoord)
    {
        cells.TryGetValue(localCoord, out Cell cell);
        return cell;
    }

    // Retrieves all cells in the chunk
    public Dictionary<Vector2Int, Cell> GetAllCells()
    {
        return cells;
    }

    // Adds or updates a cell within the chunk
    public void SetCell(Vector2Int localCoord, Cell cell)
    {
        if (localCoord.x >= 0 && localCoord.x < size && localCoord.y >= 0 && localCoord.y < size)
        {
            cells[localCoord] = cell;
            OnCellLoaded?.Invoke(cell);
        }
    }
}