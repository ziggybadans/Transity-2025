using UnityEngine;

[CreateAssetMenu(fileName = "LandObject", menuName = "Grid/LandObject")]
public class LandObject : ScriptableObject
{
    // Add properties and methods related to the land object
    public string landType = "DefaultLand";
    public Sprite landSprite;

    // Additional properties like terrain features can be added here
}
