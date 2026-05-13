using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

/// <summary>
/// Boss AI with phased behavior. Each phase has its own move speed, attack damage,
/// attack cooldown, and attack range. Phases are entered when HP drops below the
/// phase's hpThreshold (as a fraction of maxHealth).
///
/// Phases are evaluated top-down: list them in DESCENDING hpThreshold order so the
/// boss enters the highest-threshold one first.
///
/// Health is publicly readable so the boss HP bar / debug menu can display it.
/// </summary>
public class BossController : MonoBehaviour
{
    [Serializable]
    public struct BossPhase
    {
        [Range(0f, 1f)] public float hpThreshold;  // e.g. 1.0 = phase active when HP <= 100%, 0.33 = enters at 33% HP
        public float moveSpeed;
        public float attackDamage;
        public float attackCooldown;
        public float attackRange;
        public string phaseName;                   // for debug only
    }

    [Header("Identity")]
    public string bossName = "Boss";

    [Header("Health")]
    public float maxHealth = 800f;
    public float health = 800f;

    [Header("Physics")]
    public float gravity = -20f;
    public float height = 3.6f;       // taller than a normal enemy
    public float jumpForce = 6f;
    public float hearingDistance = 30f;

    [Header("Phases (list in DESCENDING hpThreshold order: 1.0, 0.66, 0.33, ...)")]
    public List<BossPhase> phases = new List<BossPhase>
    {
        new BossPhase { hpThreshold = 1.00f, moveSpeed = 2.5f, attackDamage = 20f, attackCooldown = 1.8f, attackRange = 3f,  phaseName = "Phase 1" },
        new BossPhase { hpThreshold = 0.66f, moveSpeed = 4.0f, attackDamage = 30f, attackCooldown = 1.2f, attackRange = 3f,  phaseName = "Phase 2" },
        new BossPhase { hpThreshold = 0.33f, moveSpeed = 6.0f, attackDamage = 45f, attackCooldown = 0.8f, attackRange = 3.5f, phaseName = "Phase 3 (enrage)" },
    };

    [Header("Read-only runtime")]
    [SerializeField] private int currentPhaseIndex = 0;
    public int CurrentPhaseIndex => currentPhaseIndex;
    public BossPhase CurrentPhase => phases[Mathf.Clamp(currentPhaseIndex, 0, phases.Count - 1)];

    /// <summary>Fired when boss dies. Argument is the boss that died (this).</summary>
    public event Action<BossController> OnDeath;
    /// <summary>Fired when phase changes. Args: (boss, newPhaseIndex).</summary>
    public event Action<BossController, int> OnPhaseChange;

    private Transform player;
    private Player playerComp;
    private World world;

    private float verticalVelocity;
    private bool isGrounded;
    private float nextAttackTime;

    private void Start()
    {
        health = maxHealth;

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
            playerComp = playerGO.GetComponent<Player>();
        }
        world = UnityEngine.Object.FindFirstObjectByType<World>();

        UpdatePhaseFromHP(invokeEvent: false);
    }

    private void Update()
    {
        if (player == null || world == null) return;

        var phase = CurrentPhase;

        // ---- movement (chase the player on XZ) ----
        Vector3 toPlayer = player.position - transform.position;
        Vector3 horizDir = new Vector3(toPlayer.x, 0f, toPlayer.z).normalized;
        float horizDist = new Vector2(toPlayer.x, toPlayer.z).magnitude;

        if (horizDist > phase.attackRange * 0.9f)
        {
            transform.position += horizDir * phase.moveSpeed * Time.deltaTime;
            // face the player on Y axis
            if (horizDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(horizDir)* Quaternion.Euler(0, 90, 0), 8f * Time.deltaTime);
        }
        else
        {
            // in attack range
            if (Time.time >= nextAttackTime)
            {
                Attack(phase);
                nextAttackTime = Time.time + phase.attackCooldown;
            }
        }

        // ---- gravity (mirror EnemyController approach: use World.CheckBlocks-style ground test) ----
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        // Use player's CheckBlocks as a cheap world ground test. If the block below us is solid, we're grounded.
        if (playerComp == null) return;

        Vector3 pos = transform.position;
        bool groundBelow = playerComp.CheckBlocks(pos.x, pos.y - height * 0.5f - 0.05f, pos.z);

        if (groundBelow && verticalVelocity <= 0f)
        {
            isGrounded = true;
            verticalVelocity = 0f;
            // snap to top of block
            float feetY = pos.y - height * 0.5f - 0.05f;
            float targetY = Mathf.Floor(feetY) + 1f + height * 0.5f;
            if (pos.y < targetY) { pos.y = targetY; transform.position = pos; }
        }
        else
        {
            isGrounded = false;
            verticalVelocity += gravity * Time.deltaTime;
            pos.y += verticalVelocity * Time.deltaTime;
            transform.position = pos;
        }
    }

    private void Attack(BossPhase phase)
    {
        if (playerComp == null) return;

        float damage = phase.attackDamage;
        var c = CheatsManager.Instance;
        if (c != null && c.CheatsEnabled && c.OneShotKill == false)
        {
            // OneShotKill is for the player killing enemies, not the other way around.
        }
        playerComp.TakeDamage(damage);
    }

    /// <summary>Call from PlayerCombat or similar. Respects damage multiplier and OneShotKill from cheats.</summary>
    public void TakeDamage(float incoming)
    {
        var c = CheatsManager.Instance;
        if (c != null && c.CheatsEnabled)
        {
            if (c.OneShotKill) { incoming = maxHealth + 1f; }
            else               { incoming *= c.DamageMultiplier; }
        }

        health -= incoming;
        UpdatePhaseFromHP(invokeEvent: true);

        if (health <= 0f) Die();
    }

    private void UpdatePhaseFromHP(bool invokeEvent)
    {
        if (phases == null || phases.Count == 0) return;

        float hpFrac = Mathf.Max(0f, health) / Mathf.Max(0.01f, maxHealth);

        // Find the lowest-indexed phase whose threshold is >= current hp fraction.
        // Phases are listed in descending threshold (1.0, 0.66, 0.33). The "active" phase is the LAST one
        // whose threshold >= hpFrac.
        int newIndex = 0;
        for (int i = 0; i < phases.Count; i++)
        {
            if (hpFrac <= phases[i].hpThreshold) newIndex = i;
        }

        if (newIndex != currentPhaseIndex)
        {
            currentPhaseIndex = newIndex;
            if (invokeEvent) OnPhaseChange?.Invoke(this, currentPhaseIndex);
        }
    }

    private void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}
