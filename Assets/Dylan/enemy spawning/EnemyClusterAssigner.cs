using UnityEngine;
using System.Collections.Generic;

public class EnemyClusterAssigner : MonoBehaviour
{
    [SerializeField] private List<EnemyCluster> enemyClusters = new();

    private void Awake()
    {
        EnemySpawner.AssignClusters(enemyClusters);
    }
}
