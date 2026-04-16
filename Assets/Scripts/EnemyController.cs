using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float pathUpdateInterval = 0.5f;
    public float reachDistance = 0.8f;

    [Header("Combat")]
    public float attackRange = 2f;
    public int damage = 20;
    public float attackCooldown = 1.5f;

    [Header("Body Size")]
    public float height = 1.8f;
    public float radius = 0.4f;

    private float halfHeight;

    private Transform player;
    private World world;

    private List<Vector3> path = new List<Vector3>();
    private int pathIndex;

    private float nextPathTime;
    private float nextAttackTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        world = FindObjectOfType<World>();

        halfHeight = height * 0.5f;
    }

    void Update()
    {
        if (player == null || world == null) return;

        SnapToGround();

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            TryAttack();
            return;
        }

        if (Time.time >= nextPathTime)
        {
            nextPathTime = Time.time + pathUpdateInterval;
            path = FindPath(transform.position, player.position);
            pathIndex = 0;
        }

        FollowPath();
    }

    // ---------------- COMBAT ----------------

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.health -= damage;
        }
    }

    // ---------------- MOVEMENT ----------------

    void FollowPath()
    {
        if (path == null || pathIndex >= path.Count) return;

        Vector3 target = path[pathIndex];

        Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);

        Vector3 dir = flatTarget - transform.position;
        dir.y = 0f;

        if (dir.magnitude < reachDistance)
        {
            pathIndex++;
            return;
        }

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;

        SnapToGround();
    }

    // ---------------- GROUNDING (FIXED TYPES) ----------------

    void SnapToGround()
    {
        Vector3 pos = transform.position;

        int x = Mathf.FloorToInt(pos.x);
        int z = Mathf.FloorToInt(pos.z);

        float groundY = GetGroundY(x, z);

        float targetY = groundY + halfHeight;

        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * 12f);

        transform.position = pos;
    }

    float GetGroundY(int x, int z)
    {
        for (int y = 255; y >= 0; y--)
        {
            if (world.GetVoxel(new Vector3(x, y, z)) != -1)
            {
                return (float)y + 1f;
            }
        }

        return 50f;
    }

    // ---------------- PATHFINDING (XZ ONLY) ----------------

    List<Vector3> FindPath(Vector3 startPos, Vector3 goalPos)
    {
        Vector3Int start = ToGrid(startPos);
        Vector3Int goal = ToGrid(goalPos);

        var open = new SimplePriorityQueue();
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var costSoFar = new Dictionary<Vector3Int, int>();

        open.Enqueue(start, 0);
        costSoFar[start] = 0;

        while (open.Count > 0)
        {
            Vector3Int current = open.Dequeue();

            if (current.x == goal.x && current.z == goal.z)
                break;

            foreach (Vector3Int next in GetNeighbors(current))
            {
                if (IsBlocked(next)) continue;

                int newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;

                    int priority = newCost + Heuristic(next, goal);
                    open.Enqueue(next, priority);

                    cameFrom[next] = current;
                }
            }
        }

        return BuildPath(cameFrom, start, goal);
    }

    List<Vector3> BuildPath(Dictionary<Vector3Int, Vector3Int> cameFrom,
                            Vector3Int start,
                            Vector3Int goal)
    {
        List<Vector3> result = new List<Vector3>();

        if (!cameFrom.ContainsKey(goal))
            return result;

        Vector3Int current = goal;

        while (current != start)
        {
            result.Add(ToWorld(current));
            current = cameFrom[current];
        }

        result.Reverse();
        return result;
    }

    // ---------------- GRID ----------------

    Vector3Int ToGrid(Vector3 pos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(pos.x),
            0,
            Mathf.FloorToInt(pos.z)
        );
    }

    Vector3 ToWorld(Vector3Int pos)
    {
        return new Vector3(pos.x + 0.5f, 0f, pos.z + 0.5f);
    }

    IEnumerable<Vector3Int> GetNeighbors(Vector3Int p)
    {
        yield return new Vector3Int(p.x + 1, 0, p.z);
        yield return new Vector3Int(p.x - 1, 0, p.z);
        yield return new Vector3Int(p.x, 0, p.z + 1);
        yield return new Vector3Int(p.x, 0, p.z - 1);
    }

    bool IsBlocked(Vector3Int pos)
    {
        int x = pos.x;
        int z = pos.z;

        float groundY = GetGroundY(x, z);

        int feet = world.GetVoxel(new Vector3(x, groundY, z));
        int head = world.GetVoxel(new Vector3(x, groundY + 1f, z));
        int head2 = world.GetVoxel(new Vector3(x, groundY + 2f, z));

        return feet != -1 || head != -1 || head2 != -1;
    }

    int Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
    }

    // ---------------- PRIORITY QUEUE ----------------

    class SimplePriorityQueue
    {
        private List<(Vector3Int node, int priority)> items = new List<(Vector3Int, int)>();

        public int Count => items.Count;

        public void Enqueue(Vector3Int node, int priority)
        {
            items.Add((node, priority));
        }

        public Vector3Int Dequeue()
        {
            int best = 0;

            for (int i = 1; i < items.Count; i++)
            {
                if (items[i].priority < items[best].priority)
                    best = i;
            }

            Vector3Int node = items[best].node;
            items.RemoveAt(best);
            return node;
        }
    }
}