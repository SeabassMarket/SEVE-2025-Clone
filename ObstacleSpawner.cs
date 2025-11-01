using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public float spawnInterval = 2f;
    public float spawnDistance = 30f;
    public float horizontalRange = 3f;

    private Transform player;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnInterval);
    }

    void SpawnObstacle()
    {
        if (obstaclePrefabs.Length == 0 || player == null) return;

        Vector3 spawnPos = player.position + Vector3.forward * spawnDistance;
        spawnPos.x += Random.Range(-horizontalRange, horizontalRange);
        spawnPos.y = 0; // keep on water level

        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}
