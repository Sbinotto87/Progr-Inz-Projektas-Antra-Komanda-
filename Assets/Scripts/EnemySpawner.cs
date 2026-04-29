using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Unity.XR.GoogleVr;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public int maxEnemies = 10;
    public float spawnRadius = 30f;

    public int spawnStartTime = 1080;
    public int spawnStopTime = 360;

    public float spawnInterval = 1f;

    private float nextSpawnTime;

    private List<GameObject> activeEnemies = new List<GameObject>();

    private World world;
    private Transform player;

    void Start()
    {
        world = FindObjectOfType<World>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        nextSpawnTime = Time.time + 1f;
    }

    void Update()
    {
        if (world == null || player == null) return;

        if (!ShouldSpawn(world.DayTime))
        {
            DespawnAll();
            return;
        }

        if (Time.time < nextSpawnTime) return;

        nextSpawnTime = Time.time + spawnInterval;

        TrySpawn();
    }

    bool ShouldSpawn(int time)
    {
        if (time < spawnStopTime || time >= spawnStartTime)
            return true;

        return true; //?
    }

    void TrySpawn()
    {
        activeEnemies.RemoveAll(item => item == null);

        if (activeEnemies.Count >= maxEnemies) return;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(10f, spawnRadius);

            int x = Mathf.FloorToInt(player.position.x + offset.x);
            int z = Mathf.FloorToInt(player.position.z + offset.y);

            float groundY = GetGroundY(x, z);

            if (groundY < 0) continue;

            int blockBelowId = world.GetVoxel(new Vector3(x, groundY - 1, z));

            if (blockBelowId != 11)
                continue;

            Vector3 spawnPos = new Vector3(x + 0.5f, groundY + 0.1f, z + 0.5f);

            if (!IsValidSpawnPosition(x, groundY, z))
                continue;

            if (HasLineOfSight(spawnPos))
                continue;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            activeEnemies.Add(enemy);

            return;
        }
    }

    void DespawnAll()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null)
                Destroy(activeEnemies[i]);
        }

        activeEnemies.Clear();
    }

    bool IsValidSpawnPosition(int x, float groundY, int z)
    {
        int head = world.GetVoxel(new Vector3(x, groundY + 1, z));
        int head2 = world.GetVoxel(new Vector3(x, groundY + 2, z));

        return head == -1 && head2 == -1;
    }

    float GetGroundY(int x, int z)
    {
        for (int y = Chunk.Height - 1; y >= 0; y--)
        {
            if (world.GetVoxel(new Vector3(x, y, z)) != -1)
                return y + 1f;
        }

        return -1f;
    }

    bool HasLineOfSight(Vector3 spawnPos)
    {
        Vector3 origin = player.position + Vector3.up * 1.6f;
        Vector3 direction = spawnPos - origin;
        float distance = direction.magnitude;

        Ray ray = new Ray(origin, direction.normalized);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance))
        {
            if (Vector3.Distance(hit.point, spawnPos) > 0.5f)
                return false;
        }

        return true;
    }
}