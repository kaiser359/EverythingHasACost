using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class EnemyCluster
{
    public int difficultyLevel;
    public GameObject[] enemies;
}

public class EnemySpawner : MonoBehaviour
{
    private static Dictionary<int, List<GameObject[]>> enemyClusters = new();
    private Cooldown spawnCooldown;

    [Header("Spawner Settings")]
    public bool singleUse = true;
    [Range(0f, 1f)]
    public float spawnChance = 0.5f;
    [Tooltip("For if the spawner is not single-use. Maximum number of clusters to spawn at once.")]
    public int maxClustersToSpawn = 1;

    [Header("Spawn Settings")]
    [Tooltip("Time in seconds between enemy spawns.")]
    public float spawnInterval = 2f;
    public float spawnRadius = 5f;
    public int difficultyLevel = 1; // TODO: determine difficulty level based on player progress or other factors

    // static methods to manage enemy clusters
    public static void AssignClusters(List<EnemyCluster> clusters)
    {
        foreach (EnemyCluster cluster in clusters)
        {
            // initialize the list for this difficulty level if it doesn't exist
            if (!enemyClusters.ContainsKey(cluster.difficultyLevel))
            {
                enemyClusters[cluster.difficultyLevel] = new List<GameObject[]>();
            }

            enemyClusters[cluster.difficultyLevel].Add(cluster.enemies);
        }
    }

    public static Dictionary<int, List<GameObject[]>> GetClusters()
    {
        return enemyClusters;
    }

    // set up the cooldown in Awake
    private void Awake()
    {
        spawnCooldown = new Cooldown(spawnInterval);
    }

    private void Start()
    {
        if (singleUse)
        {
            SpawnEnemies();
            enabled = false; // disable the spawner after one use
        }
    }

    // spawn enemies at regular intervals based on the cooldown
    private void Update()
    {
        if (spawnCooldown.IsReady())
        {
            SpawnEnemies();
            spawnCooldown.Use();
        }
    }

    // spawn enemies from a random cluster at random positions around the spawner
    private void SpawnEnemies()
    {
        // if nothing exists :sob:
        if (enemyClusters.Count == 0) return;

        // must pass the spawn chance check to spawn enemies
        if (UnityEngine.Random.Range(0f, 1f) > spawnChance) return;

        int targetDifficultyLevel = difficultyLevel; // TODO: determine difficulty level based on player progress or other factors

        List<GameObject[]> clustersAtDifficulty;
        enemyClusters.TryGetValue(targetDifficultyLevel, out clustersAtDifficulty);

        while (clustersAtDifficulty == null || clustersAtDifficulty.Count == 0)
        {
            // if there are no clusters at the current difficulty level, try the next one
            targetDifficultyLevel--;

            // if we've gone through all difficulty levels and found nothing, return
            if (targetDifficultyLevel < 0)
            {
                Debug.LogWarning("No enemy clusters available to spawn.");
                return;
            }
            enemyClusters.TryGetValue(targetDifficultyLevel, out clustersAtDifficulty);
        }

        GameObject[] clusterToSpawn = clustersAtDifficulty[UnityEngine.Random.Range(0, clustersAtDifficulty.Count)];

        foreach (GameObject enemyPrefab in clusterToSpawn)
        {
            Vector2 spawnPosition = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * spawnRadius;
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.transform.parent = transform; // parent the enemy to the spawner for organization
        }
    }
}
