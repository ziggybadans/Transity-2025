using UnityEngine;

[CreateAssetMenu(fileName = "StationObject", menuName = "Grid/StationObject")]
public class StationObject : ScriptableObject
{
    public string stationName = "New Station";
    public Sprite stationSprite;

    // Additional properties like station capacity, services, etc.
}
