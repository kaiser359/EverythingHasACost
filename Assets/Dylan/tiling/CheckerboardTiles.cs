using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class CheckerboardTiles : MonoBehaviour
{
    // two tiles to alternate between for the checkerboard pattern
    public TileBase tileA;
    public TileBase tileB;

    // Start is called before the first frame update
    void Start()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("CheckerboardTiles: No Tilemap component found on the GameObject.");
            return;
        }

        if (tileA == null || tileB == null)
        {
            Debug.LogError("CheckerboardTiles: Please assign both tileA and tileB in the inspector.");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase existing = tilemap.GetTile(cellPos);

                // skip empty cells to avoid populating the entire bounds with tiles.
                if (existing == null)
                {
                    continue;
                }

                int parity = (cellPos.x + cellPos.y) & 1;

                if (parity == 0)
                {
                    tilemap.SetTile(cellPos, tileA);
                }
                else
                {
                    tilemap.SetTile(cellPos, tileB);
                }
            }
        }
    }
}
