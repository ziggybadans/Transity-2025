using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CellData
{
    public Vector2Int worldPosition;
    public string landObjectName;
    public string stationObjectName;
    // Add other object types as needed
}

public class Cell
{
    public Vector2Int worldPosition;
    public LandObject landObject;
    public StationObject stationObject;
    // Add other object types as needed

    // Events to notify when objects change
    public event Action<Cell, GridObject> OnObjectAdded;
    public event Action<Cell, GridObject> OnObjectRemoved;

    [NonSerialized]
    public List<GameObject> instantiatedObjects = new List<GameObject>();

    public Cell(Vector2Int pos)
    {
        worldPosition = pos;
    }

    public void SetLandObject(LandObject obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        landObject = obj;
        OnObjectAdded?.Invoke(this, obj);
    }

    public void RemoveLandObject()
    {
        if (landObject != null)
        {
            OnObjectRemoved?.Invoke(this, landObject);
            landObject = null;
        }
    }

    public void SetStationObject(StationObject obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        stationObject = obj;
        OnObjectAdded?.Invoke(this, obj);
    }

    public void RemoveStationObject()
    {
        if (stationObject != null)
        {
            OnObjectRemoved?.Invoke(this, stationObject);
            stationObject = null;
        }
    }

    // Add similar methods for other object types
}