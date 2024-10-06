using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public Vector2Int worldPosition;
    private LandObject landObject;
    private Dictionary<System.Type, ScriptableObject> components = new Dictionary<System.Type, ScriptableObject>();

    public Cell(Vector2Int position)
    {
        worldPosition = position;
    }

    // Sets the mandatory land object
    public void SetLandObject(LandObject land)
    {
        landObject = land;
        // Initialize visual representation if necessary
    }

    public LandObject GetLandObject()
    {
        return landObject;
    }

    // Adds an optional component
    public void AddComponent<T>(T component) where T : ScriptableObject
    {
        components[typeof(T)] = component;
        // Initialize visual representation if necessary
    }

    // Retrieves an optional component
    public T GetComponent<T>() where T : ScriptableObject
    {
        if (components.ContainsKey(typeof(T)))
        {
            return components[typeof(T)] as T;
        }
        return null;
    }

    // Removes an optional component
    public void RemoveComponent<T>() where T : ScriptableObject
    {
        if (components.ContainsKey(typeof(T)))
        {
            components.Remove(typeof(T));
            // Cleanup visual representation if necessary
        }
    }

    // Checks if a component exists
    public bool HasComponent<T>() where T : ScriptableObject
    {
        return components.ContainsKey(typeof(T));
    }
}
