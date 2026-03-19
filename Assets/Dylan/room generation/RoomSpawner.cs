using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomSpawner : MonoBehaviour
{
    // openingDirection indicates which door is needed
    public int openingDirection;
    public List<int> otherOpeningDirections = new();
    /*
     * 1 --> need bottom door
     * 2 --> need top door
     * 3 --> need left door
     * 4 --> need right door
     */

    // reference to RoomTemplates
    private RoomTemplates templates;
    private GameObject dungeon;
    private DungeonController dungeonController;
    private int randomDoorIndex;

    public GameObject origin;
    private GameObject room;

    private List<GameObject[]> roomClass;
    private GameObject[] roomsSpawnable;

    private bool spawned = false;

    public float roomWidth = 72;
    public float roomHeight = 64;
    private List<Vector2> wallChecks;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // generate wall checks based on room dimensions
        wallChecks = new List<Vector2>
        {
            new (0, -roomHeight / 2f), // bottom
            new (0, roomHeight / 2f), // top
            new (-roomWidth / 2f, 0), // left
            new (roomWidth / 2f, 0), // right
        };

        // find the RoomTemplates object in the scene
        dungeon = GameObject.FindWithTag("Dungeon");
        templates = dungeon.GetComponent<RoomTemplates>();
        dungeonController = dungeon.GetComponent<DungeonController>();

        StartCoroutine(Generate());
    }
    private IEnumerator Generate()
    {
        // Wait until the objectToWaitFor is not null
        yield return new WaitUntil(() => templates.dictionaryIsBuilt);
        //yield return new WaitForSeconds(0.1f); // small delay to ensure everything is set up

        // the rooms are now available, you can safely access it here
        Debug.Log("rooms set up :3c");

        // if under the spawn requirement, spawn normal rooms
        if (dungeonController.roomsSpawned < dungeonController.minRooms || otherOpeningDirections.Count > 0)
        {
            roomClass = templates.rooms;
        }
        // once the spawn requirement is reached, only spawn cap rooms
        else
        {
            roomClass = templates.caps;
        }

        // spawn after delay
        Invoke("Spawn", 0.05f);
    }

    public void RegenerateDungeon()
    {
        // don't regenerate if this is not the starting room
        // since the starting room is the only one that can guarantee a valid dungeon layout
        if (!gameObject.CompareTag("StartPoint"))
        {
            Debug.LogError("Cannot regenerate starting room!");
            return;
        }

        spawned = false;
        Invoke("Spawn", 0.05f);
    }

    public void RegenerateRoom()
    {
        dungeonController.roomsSpawned--; // decrement rooms spawned count
        dungeonController.enemiesSpawned -= room.GetComponent<RoomController>().EnemyCount(); // reset enemy count

        spawned = false;
        Destroy(room);
        Spawn();
    }

    // spawn rooms based on opening direction
    private void Spawn()
    {
        // if room already spawned, exit
        if (spawned) return;

        // by this point, this spawn point is spawning a room, so
        // if win room has not been spawned yet and this is a cap room spawn point
        if (!RoomTemplates.winSpawned && roomClass == templates.caps)
        {
            RoomTemplates.winSpawned = true;

            roomClass = templates.winRooms;
            Debug.Log("WIN ROOM SPAWNER SET AT: " + transform.position);
        }

        // increment rooms spawned count
        dungeonController.roomsSpawned++;

        // room spawning debugging
        Debug.Log(dungeonController.roomsSpawned);

        // indicate that a room has been spawned
        spawned = true;

        if (openingDirection == 0)
        {
            // starting spawner
            room = Instantiate(templates.startingRoom, transform.position, Quaternion.identity, dungeon.transform);

            RoomController roomController = room.GetComponent<RoomController>();

            roomController.AssignOrigins(gameObject);
            dungeon.transform.GetChild(0).transform.GetChild(0).GetComponent<Tilemap>().SetTilesBlock(roomController.GetWallBounds(true), roomController.GetWalls());

            // terminate early since it's the starting room
            return;
        }

        // assign room class to spawnable rooms based on assigned opening direction
        roomsSpawnable = roomClass[openingDirection - 1];

        Debug.Log(transform.position + " ROOMS SPAWNABLE BEFORE CHECKS: " + roomsSpawnable.Length);

        if (roomClass == templates.caps && otherOpeningDirections.Count != 0)
        {
            // cap requires more than one door
            roomClass = templates.rooms;
            roomsSpawnable = roomClass[openingDirection - 1];

            for (int i = 0; i < 4; i++)
            {
                // exclude rooms continue dungeon generation
                if (!otherOpeningDirections.Contains(i + 1) || openingDirection != i + 1)
                {
                    roomsSpawnable = roomsSpawnable.Except(roomClass[i]).ToArray();
                }
            }
        }

        for (int i = 0; i < wallChecks.Count; i++)
        {
            Debug.Log("CHECKING WALL: " + transform.position + " / DIRECTION: " + wallChecks[i]);

            // exclude rooms that have a door in the direction of an adjacent wall
            if (Physics2D.Raycast((Vector2)transform.position + wallChecks[i], wallChecks[i], 0.02f, LayerMask.GetMask("Walls")))
            {
                Debug.Log("WALL CHECK HIT: " + transform.position + " / DIRECTION: " + (i + 1));
                roomsSpawnable = roomsSpawnable.Except(roomClass[i]).ToArray();
            }
            else
            {
                Debug.Log("WALL CHECK MISSED: " + transform.position + " / DIRECTION: " + (i + 1));
            }
        }

        if (roomsSpawnable.Length == 0)
        {
            roomsSpawnable = templates.caps[openingDirection - 1];
        }

        foreach (int otherOpeningDirection in otherOpeningDirections)
        {
            // include rooms that have a door in the direction of other spawn points that this spawn point collides with
            roomsSpawnable = roomsSpawnable.Intersect(roomClass[otherOpeningDirection - 1]).ToArray();
        }

        SpawnRoom(roomsSpawnable);
    }

    // instantiate room based on opening direction
    private void SpawnRoom(GameObject[] roomType)
    {
        randomDoorIndex = UnityEngine.Random.Range(0, roomType.Length);
        Debug.Log("RANDOM DOOR INDEX: " + randomDoorIndex);
        room = Instantiate(roomType[randomDoorIndex], transform.position, Quaternion.identity, dungeon.transform);

        RoomController roomController = room.GetComponent<RoomController>();

        dungeonController.enemiesSpawned += roomController.EnemyCount();
        dungeon.transform.GetChild(0).transform.GetChild(0).GetComponent<Tilemap>().SetTilesBlock(roomController.GetWallBounds(true), roomController.GetWalls());

        // assign the origin of the room to this spawn point so that it can be accessed by the room's spawn points
        room.GetComponent<RoomController>().AssignOrigins(gameObject);

        // room spawning debugging
        Debug.Log("SPAWNED ROOM" + transform.position + roomType[randomDoorIndex].name);
    }

    // called when another collider enters this trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // skip if already spawned
        if (spawned) return;

        // force destroy spawn point if it:
        // 1. collides with another spawn point
        // 2. the other spawn point has already spawned a room
        // 3. this spawn point has a higher opening direction value than the other
        if (!other.CompareTag("SpawnPoint") && !other.CompareTag("StartPoint")) return;

        // the other spawn point is the point is generated from, so this spawn point is redundant and should be destroyed
        if (other.gameObject == origin.GetComponent<RoomSpawner>().origin)
        {
            Debug.Log("DESTROYED SPAWN: " + GetComponent<RoomSpawner>().openingDirection + transform.position + " / REASON: ALREADY SPAWNED");

            Destroy(gameObject);
        }

        // the other spawn point has already spawned a room, so this spawn point is redundant and should be destroyed
        else if (other.GetComponent<RoomSpawner>().spawned)
        {
            Debug.Log("DESTROYED SPAWN: " + GetComponent<RoomSpawner>().openingDirection + transform.position + " / REASON: COLLIDED WITH SPAWN THAT HAS ALREADY SPAWNED");
            Destroy(gameObject);
        }

        // this spawn point has a higher opening direction value than the other
        else if (GetComponent<RoomSpawner>().openingDirection > other.GetComponent<RoomSpawner>().openingDirection)
        {
            Debug.Log("DESTROYED SPAWN: " + GetComponent<RoomSpawner>().openingDirection + transform.position + " / REASON: HIGHER OPENING DIRECTION");

            other.GetComponent<RoomSpawner>().otherOpeningDirections.Add(openingDirection);
            Destroy(gameObject);
        }
    }

    // debugging stuff
    private void OnDrawGizmos()
    {
        if (origin == null)
        {
            return;
        }

        Debug.DrawLine(Vector3.Lerp(origin.transform.position, transform.position, 0.5f), Vector3.Lerp(origin.transform.position, transform.position, 0.5f) + Vector3.right * 0.1f, Color.green);
    }
}