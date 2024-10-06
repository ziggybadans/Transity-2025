using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LineObject", menuName = "Grid/LineObject")]
public class LineObject : ScriptableObject
{
    public string lineName = "New Line";
    public Color lineColor = Color.white;
    public List<Vector2Int> path = new List<Vector2Int>();

    // Additional properties like line speed, type, etc.
}
