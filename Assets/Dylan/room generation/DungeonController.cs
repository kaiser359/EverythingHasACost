using UnityEngine;
using UnityEngine.Tilemaps;

public class Cooldown
{
    private readonly float cooldownTime;
    private float lastUsedTime;
    private readonly bool usesRealTime;

    public Cooldown(float cooldownTime, bool usesRealTime = false)
    {
        this.cooldownTime = cooldownTime;
        this.lastUsedTime = -cooldownTime; // Initialize to allow immediate use

        this.usesRealTime = usesRealTime; // if true, cooldown is based on real time (ignoring time scale); if false, it uses game time
    }
    public bool IsReady()
    {
        if (usesRealTime)
        {
            return Time.realtimeSinceStartup >= lastUsedTime + cooldownTime;
        }
        else
        {
            return Time.time >= lastUsedTime + cooldownTime;
        }
    }

    public void Use()
    {
        if (usesRealTime)
        {
            lastUsedTime = Time.realtimeSinceStartup;
        }
        else
        {
            lastUsedTime = Time.time;
        }
    }
}

public class DungeonController : MonoBehaviour
{
    // tracking the number of rooms spawned and the maximum allowed
    public int roomsSpawned = 0;
    public int enemiesSpawned = 0;
    public int minRooms = 30;
    private RoomSpawner startSpawner;

    private Cooldown regenerateCooldown = new(1f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startSpawner = GameObject.FindWithTag("StartPoint").GetComponent<RoomSpawner>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && regenerateCooldown.IsReady())
        {
            RegenerateDungeon();
            regenerateCooldown.Use();
        }
    }

    public void RegenerateDungeon()
    {
        roomsSpawned = 0;
        enemiesSpawned = 0;

        transform.GetChild(0).transform.GetChild(0).GetComponent<Tilemap>().ClearAllTiles();
        
        for (int i = 1; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // reset the win condition flag in RoomTemplates
        RoomTemplates.winSpawned = false;

        Invoke("DungeonRegenDelay", 0.01f);
    }
    private void DungeonRegenDelay() {
        startSpawner.RegenerateDungeon();
    }
}
