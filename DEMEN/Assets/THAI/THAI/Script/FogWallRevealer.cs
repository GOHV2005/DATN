using UnityEngine.Tilemaps;
using UnityEngine;

public class FogWallRevealer : MonoBehaviour
{
    public Tilemap fogTilemap;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BoundsInt bounds = fogTilemap.cellBounds;
            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                if (fogTilemap.HasTile(pos))
                {
                    fogTilemap.SetTile(pos, null); // Remove fog tile
                }
            }
        }
    }
}
