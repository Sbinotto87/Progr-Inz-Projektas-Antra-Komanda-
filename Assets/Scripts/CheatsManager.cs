using System;
using UnityEngine;
using Assets.Scripts;

/// <summary>
/// Central source of truth for all cheats and dev overrides.
/// Place ONE instance in your scene on an empty GameObject named "_CheatsManager"
/// (or just let it auto-create itself the first time something accesses Instance).
///
/// Other scripts (StatBarUI, FlightController, DevMenuUI, DebugMenuUI, CheatsPanelUI)
/// all read/write through this singleton so toggling a cheat anywhere takes effect everywhere.
/// </summary>
public class CheatsManager : MonoBehaviour
{
    private static CheatsManager _instance;
    public static CheatsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CheatsManager>();
                if (_instance == null)
                {
                    var go = new GameObject("_CheatsManager");
                    _instance = go.AddComponent<CheatsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // -------- Boss --------
    [Header("Combat")]
    public bool OneShotKill = false;
    public float DamageMultiplier = 1f;
    
    [Header("Boss")]
    /// <summary>Set true to force a boss to spawn next frame. BossSpawner consumes this.</summary>
    public bool ForceBossSpawn = false;
    /// <summary>Set positive to override the next boss spawn day. BossSpawner consumes this.</summary>
    public int BossDayOverride = -1;

    // --------------------------

    // -------- Master switch --------
    /// <summary>If false, ALL cheats are forced off regardless of individual flags.</summary>
    public bool CheatsEnabled = false;

    // -------- Survival cheats --------
    [Header("Survival")]
    public bool InfiniteHealth = false;
    public bool InfiniteHunger = false;
    public bool InfiniteThirst = false;
    public bool InfiniteStamina = false;
    public bool NoRadiationDamage = false;

    // -------- Movement cheats --------
    [Header("Movement")]
    public bool Flight = false;
    public float FlightSpeed = 12f;
    public float FlightFastMultiplier = 3f;   // hold Left Shift for boost
    public bool NoClip = false;               // optional - FlightController checks this

    // -------- Time --------
    [Header("Time")]
    /// <summary>If true, World.DayTime is held at FrozenDayTime every frame.</summary>
    public bool FreezeTime = false;
    public int FrozenDayTime = 720; // noon

    // -------- Combat (placeholder for boss work later) --------
    //[Header("Combat")]
    //public bool OneShotKill = false;
    //public float DamageMultiplier = 1f;

    // Events so UI can react if it wants to (e.g. re-sync toggle visuals)
    public event Action OnCheatsChanged;

    private World world;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        TryFindWorld();
    }

    private void TryFindWorld()
    {
        if (world != null) return;
        var worldGO = GameObject.Find("World");
        if (worldGO != null) world = worldGO.GetComponent<World>();
        if (world == null) world = FindFirstObjectByType<World>();
    }

    private void LateUpdate()
    {
        // Re-find world if scene was reloaded
        if (world == null) TryFindWorld();

        // Time freeze: continually overwrite DayTime so the tick system can't progress it.
        // World.DayTime is public, so this works without modifying World.cs.
        if (CheatsEnabled && FreezeTime && world != null)
        {
            world.DayTime = Mathf.Clamp(FrozenDayTime, 0, 1439);
        }
    }

    // -------- Helpers used by other scripts --------
    public bool On(bool flag) => CheatsEnabled && flag;

    public void NotifyChanged() => OnCheatsChanged?.Invoke();

    /// <summary>Directly set time of day (0..1439). Doesn't enable freeze on its own.</summary>
    public void SetDayTime(int minutes)
    {
        TryFindWorld();
        if (world == null) return;
        world.DayTime = Mathf.Clamp(minutes, 0, 1439);
        if (FreezeTime) FrozenDayTime = world.DayTime;
    }

    public int GetDayTime()
    {
        TryFindWorld();
        return world != null ? world.DayTime : 0;
    }

    public int GetCurrentDay()
    {
        TryFindWorld();
        return world != null ? world.CurrentDay : 0;
    }
}
