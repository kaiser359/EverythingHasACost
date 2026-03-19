using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomController : MonoBehaviour
{
    private Tilemap Walls => transform.GetChild(0).GetComponent<Tilemap>();

    public BoundsInt GetWallBounds(bool toWorld = false)
    {
        if (!toWorld)
        {
            Debug.Log("skip");
            return Walls.cellBounds;
        }

        Vector2 halfextents = new(Walls.cellBounds.size.x/2f, Walls.cellBounds.size.y/2f);
        Debug.Log($"Room Size: {halfextents}");

        Vector2 blPos = new(
            Walls.gameObject.transform.position.x - halfextents.x,
            Walls.gameObject.transform.position.y - halfextents.y
        );
        Debug.Log($"Bottom Left Position: {blPos}");

        BoundsInt bounds = new(
            new Vector3Int((int)blPos.x, (int)blPos.y, 0),
            Walls.cellBounds.size
        );

        Debug.Log($"Original wall bounds: {Walls.cellBounds}");
        Debug.Log($"Calculated wall bounds: {bounds}");

        return bounds;
    }

    public TileBase[] GetWalls()
    {
        Walls.gameObject.SetActive(false);
        return Walls.GetTilesBlock(GetWallBounds());
    }

    public int EnemyCount()
    {
        // Try to find a child named "Enemies" first, otherwise fall back to original index 3 if present.
        Transform container = transform.Find("Enemies");
        if (container == null && transform.childCount > 3)
            container = transform.GetChild(3);

        return container != null ? container.childCount : 0;
    }

    public void AssignOrigins(GameObject origin)
    {
        if (origin == null)
            return;

        foreach (Transform child in transform)
        {
            if (child.CompareTag("SpawnPoint"))
            {
                Debug.Log($"assigned {origin.transform.position} to {child.position}");

                var spawner = child.GetComponent<RoomSpawner>();
                if (spawner != null)
                    spawner.origin = origin;
            }
        }
    }
}
