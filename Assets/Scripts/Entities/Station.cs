using UnityEngine;

public class Station : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.yellow;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Select()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = selectedColor;
    }

    public void Deselect()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = defaultColor;
    }
}
