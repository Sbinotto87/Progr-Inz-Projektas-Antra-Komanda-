using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

/// <summary>
/// Spawns a boss at scheduled days. Place ONE in the scene (alongside CheatsManager / DevTools).
///
/// Drop the Enemy prefab into 'bossPrefab' for now. When you make a dedicated boss prefab later,
/// just swap it here. The spawner will scale it up and add a BossController.
///
/// Schedule logic:
///   - On Start, NextBossDay = bossDayInterval (i.e. first boss on day 5/7/whatever).
///   - Each frame, if CurrentDay >= NextBossDay AND no boss currently alive, spawn one.
///   - When the boss dies, NextBossDay = currentDay + bossDayInterval.
/// </summary>
public class BossSpawner : MonoBehaviour
{
    public float widthMultiplier = 1.1f;   // boss is only slightly wider than a normal enemy
    public float heightMultiplier = 2.5f;  // but much taller

    [Header("Spawn schedule")]
    public int bossDayInterval = 7;
    public int nextBossDay = 7;

    [Header("Prefab")]
    [Tooltip("For now, drag the Enemy prefab here. Will be scaled up and given a BossController.")]
    public GameObject bossPrefab;
    public float scaleMultiplier = 2.5f;
    public float bossMaxHealth = 800f;

    [Header("Spawn placement")]
    [Tooltip("Minimum distance from player when spawning (so boss doesn't appear on top of player).")]
    public float minSpawnDistance = 12f;
    [Tooltip("How many attempts to find a valid surface block before giving up this frame.")]
    public int maxPlacementAttempts = 30;

    [Header("Runtime (read-only)")]
    [SerializeField] private BossController currentBoss;
    public BossController CurrentBoss => currentBoss;
    public bool HasActiveBoss => currentBoss != null;

    private World world;
    private Player player;

    private void Start()
    {
        nextBossDay = bossDayInterval;
        world  = UnityEngine.Object.FindFirstObjectByType<World>();
        var pgo = GameObject.FindGameObjectWithTag("Player");
        if (pgo != null) player = pgo.GetComponent<Player>();
    }

    private void Update()
    {
        if (world == null || player == null)
        {
            if (world == null)  world  = UnityEngine.Object.FindFirstObjectByType<World>();
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (player == null && pgo != null) player = pgo.GetComponent<Player>();
            return;
        }

        // Force-spawn from cheats menu
        var c = CheatsManager.Instance;
        if (c != null && c.ForceBossSpawn && !HasActiveBoss)
        {
            c.ForceBossSpawn = false;
            TrySpawnBoss();
            return;
        }

        // Allow the cheats menu to override the next-spawn day
        if (c != null && c.BossDayOverride > 0)
        {
            nextBossDay = c.BossDayOverride;
            c.BossDayOverride = -1; // consumed
        }

        // Scheduled spawn
        if (!HasActiveBoss && world.CurrentDay >= nextBossDay)
        {
            TrySpawnBoss();
        }
    }

    public bool TrySpawnBoss()
    {
        if (bossPrefab == null) { Debug.LogWarning("[BossSpawner] No bossPrefab assigned."); return false; }
        if (HasActiveBoss)      { Debug.LogWarning("[BossSpawner] Boss already alive, skipping spawn."); return false; }

        if (!TryFindSpawnPoint(out Vector3 spawnPos))
        {
            Debug.LogWarning("[BossSpawner] Could not find a valid spawn point this frame; will retry.");
            return false;
        }

        var go = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        go.name = "Boss";


        // Scale taller, keep width roughly the same
        Vector3 s = go.transform.localScale;
        s.x *= widthMultiplier;
        s.y *= heightMultiplier;
        s.z *= widthMultiplier;
        go.transform.localScale = s;

        // Disable normal enemy AI if the prefab has it
        var enemy = go.GetComponent<EnemyController>();
        if (enemy != null) enemy.enabled = false;

        // Add boss AI
        var boss = go.GetComponent<BossController>();
        if (boss == null) boss = go.AddComponent<BossController>();
        boss.maxHealth = bossMaxHealth;
        boss.health = bossMaxHealth;
        boss.OnDeath += HandleBossDeath;

        currentBoss = boss;
        Debug.Log($"[BossSpawner] Boss spawned at {spawnPos}. Next boss day: {nextBossDay} (current day {world.CurrentDay}).");
        return true;
    }

    private void HandleBossDeath(BossController boss)
    {
        currentBoss = null;
        nextBossDay = world.CurrentDay + bossDayInterval;
        Debug.Log($"[BossSpawner] Boss died. Next boss day: {nextBossDay}.");
    }

    private bool TryFindSpawnPoint(out Vector3 result)
    {
        result = default;
        if (world == null || player == null) return false;

        var active = world.activeChunks;
        if (active == null || active.Count == 0) return false;

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            // Pick a random loaded chunk
            var coord = active[Random.Range(0, active.Count)];

            // Pick random x/z inside that chunk
            float worldX = coord.x * Chunk.Width + Random.Range(0, Chunk.Width) + 0.5f;
            float worldZ = coord.z * Chunk.Width + Random.Range(0, Chunk.Width) + 0.5f;

            // Reject if too close to player
            Vector2 toPlayer = new Vector2(worldX - player.transform.position.x, worldZ - player.transform.position.z);
            if (toPlayer.magnitude < minSpawnDistance) continue;

            // Find surface Y by scanning down from sky
            for (int y = Chunk.Height - 2; y > 1; y--)
            {
                bool solid = player.CheckBlocks(worldX, y, worldZ);
                bool aboveAir = !player.CheckBlocks(worldX, y + 1, worldZ);
                if (solid && aboveAir)
                {
                    // surface is the top of this block
                    result = new Vector3(worldX, y + 1f + 1.8f, worldZ);
                    return true;
                }
            }
        }
        return false;
    }
}
