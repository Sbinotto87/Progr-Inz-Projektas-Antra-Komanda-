using UnityEngine;
using Assets.Scripts;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    public float jumpForce = 5f;
    public float gravity = -20f;
    public float height = 1.8f;

    public int maxHealth = 100;

    private int health;

    private Transform player;
    private World world;

    private float verticalVelocity;
    private bool isGrounded;
    private float nextAttackTime;

    void Start()
    {
        health = maxHealth;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        world = FindObjectOfType<World>();
    }

    void Update()
    {
        if (player == null || world == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;

        if (dist <= attackRange)
        {
            TryAttack();
            return;
        }

        Vector3 moveDir = toPlayer.normalized;

        TryJump(moveDir);
        MoveXZ(moveDir);
        ApplyGravity();
    }

    void MoveXZ(Vector3 dir)
    {
        Vector3 next = transform.position + dir * moveSpeed * Time.deltaTime;
        next.y = transform.position.y;

        if (!IsBlocked(next))
        {
            transform.position = next;
        }
    }

    void TryJump(Vector3 dir)
    {
        if (!isGrounded) return;

        Vector3 ahead = transform.position + dir * 0.6f;

        int x = Mathf.FloorToInt(ahead.x);
        int z = Mathf.FloorToInt(ahead.z);

        float currentGround = GetGroundY(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.z)
        );

        float nextGround = GetGroundY(x, z);

        float step = nextGround - currentGround;

        if (step > 0.1f && step <= 2f)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
        }
    }

    void ApplyGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y += verticalVelocity * Time.deltaTime;

        float groundY = GetGroundY(
            Mathf.FloorToInt(pos.x),
            Mathf.FloorToInt(pos.z)
        ) + height * 0.5f;

        if (pos.y <= groundY)
        {
            pos.y = groundY;
            verticalVelocity = 0f;
            isGrounded = true;
        }

        transform.position = pos;
    }

    float GetGroundY(int x, int z)
    {
        for (int y = 255; y >= 0; y--)
        {
            if (world.GetVoxel(new Vector3(x, y, z)) != -1)
                return y + 1;
        }

        return 50f;
    }

    bool IsBlocked(Vector3 pos)
    {
        Vector3 p = new Vector3(
            Mathf.FloorToInt(pos.x),
            Mathf.FloorToInt(pos.y),
            Mathf.FloorToInt(pos.z)
        );

        return world.GetVoxel(p) != -1;
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        Player p = player.GetComponent<Player>();
        if (p != null)
            p.health -= 20;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        if (health <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}