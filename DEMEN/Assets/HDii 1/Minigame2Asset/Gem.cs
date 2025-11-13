using UnityEngine;

public enum GemType { Red, Blue, Green, Yellow, Purple, Orange }

[RequireComponent(typeof(SpriteRenderer))]
public class Gem : MonoBehaviour
{
    public GemType type;
    public int row, col;            // logical position in grid
    [HideInInspector] public bool isMatched = false;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite s)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.sprite = s;
    }

    public void SetPosition(int r, int c)
    {
        row = r; col = c;
    }
}
