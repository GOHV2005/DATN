using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWar : MonoBehaviour
{
    public Tilemap fogTilemap;
    public TileBase fogTile;
    public Transform player;
    public int revealRadius = 5;

    private HashSet<Vector3Int> revealedTiles = new HashSet<Vector3Int>();

    void Update()
    {
        Vector3Int playerCell = fogTilemap.WorldToCell(player.position);

        for (int x = -revealRadius; x <= revealRadius; x++)
        {
            for (int y = -revealRadius; y <= revealRadius; y++)
            {
                Vector3Int cell = new Vector3Int(playerCell.x + x, playerCell.y + y, 0);
                if (Vector3.Distance(fogTilemap.CellToWorld(cell), player.position) <= revealRadius)
                {
                    if (!revealedTiles.Contains(cell))
                    {
                        fogTilemap.SetTile(cell, null); // Remove fog tile
                        revealedTiles.Add(cell);
                    }
                }
            }
        }
    }
}
