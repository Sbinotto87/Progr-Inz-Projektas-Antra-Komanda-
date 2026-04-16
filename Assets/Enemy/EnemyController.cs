using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 3f;
    public float pathUpdateInterval = 1.5f;
    public float jumpForce = 6f;

    public float damageInterval = 1.5f;
    public float damageAmount = 20f;
    public float hitRange = 1.2f;

    private Transform playerTransform;
    private Player player;

    private List<Node> path = new List<Node>();
    private float pathTimer;
    private float damageTimer;

    private float verticalVelocity;
    private bool grounded;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
        {
            playerTransform = p.transform;
            player = p.GetComponent<Player>();
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
            {
                playerTransform = p.transform;
                player = p.GetComponent<Player>();
            }
            return;
        }

        pathTimer += Time.deltaTime;

        if (pathTimer >= pathUpdateInterval)
        {
            Vector3Int start = ToGrid(transform.position);
            Vector3Int target = ToGrid(playerTransform.position);

            path = FindPath(start, target);

            Debug.Log(path == null ? "NO PATH" : "PATH FOUND: " + path.Count);

            pathTimer = 0f;
        }

        if (path != null && path.Count > 0)
            FollowPath();

        HandleDamage();
    }

    void FollowPath()
    {
        Vector3Int current = ToGrid(transform.position);
        Vector3Int next = path[0].position;

        int dy = next.y - current.y;

        if (dy > 0 && grounded)
        {
            verticalVelocity = jumpForce;
            grounded = false;
        }

        Vector3 target = new Vector3(next.x + 0.5f, transform.position.y, next.z + 0.5f);

        Vector3 move = target - transform.position;
        move.y = 0;

        if (move.magnitude > 0.01f)
            transform.position += move.normalized * speed * Time.deltaTime;

        Vector2 a = new Vector2(transform.position.x, transform.position.z);
        Vector2 b = new Vector2(target.x, target.z);

        if (Vector2.Distance(a, b) < 0.3f)
            path.RemoveAt(0);

        ApplyGravity();
    }

    void ApplyGravity()
    {
        Vector3Int below = ToGrid(transform.position + Vector3.down);

        grounded = IsSolid(below);

        if (!grounded)
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        else if (verticalVelocity < 0)
            verticalVelocity = 0;

        Vector3 pos = transform.position;
        pos.y += verticalVelocity * Time.deltaTime;
        transform.position = pos;
    }

    void HandleDamage()
    {
        if (playerTransform == null || player == null)
            return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist <= hitRange)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= damageInterval)
            {
                player.health -= damageAmount;
                damageTimer = 0f;
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    List<Vector3Int> GetNeighbors(Vector3Int pos)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        Vector3Int[] dirs =
        {
            Vector3Int.forward,
            Vector3Int.back,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (var d in dirs)
        {
            Vector3Int next = pos + d;

            for (int y = -1; y <= 1; y++)
            {
                Vector3Int candidate = new Vector3Int(
                    next.x,
                    pos.y + y,
                    next.z
                );

                if (!IsSolid(candidate))
                    result.Add(candidate);
            }
        }

        return result;
    }

    List<Node> FindPath(Vector3Int start, Vector3Int target)
    {
        Dictionary<Vector3Int, Node> open = new Dictionary<Vector3Int, Node>();
        HashSet<Vector3Int> closed = new HashSet<Vector3Int>();

        open[start] = new Node { position = start };

        int safety = 0;

        while (open.Count > 0)
        {
            safety++;
            if (safety > 2000)
                return null;

            Node current = null;

            foreach (var n in open.Values)
                if (current == null || n.fCost < current.fCost)
                    current = n;

            open.Remove(current.position);
            closed.Add(current.position);

            if (current.position.x == target.x && current.position.z == target.z)
                return Retrace(current);

            foreach (Vector3Int n in GetNeighbors(current.position))
            {
                if (closed.Contains(n))
                    continue;

                int newG = current.gCost + 1;

                if (open.TryGetValue(n, out Node existing))
                {
                    if (newG < existing.gCost)
                    {
                        existing.gCost = newG;
                        existing.parent = current;
                    }
                }
                else
                {
                    open[n] = new Node
                    {
                        position = n,
                        parent = current,
                        gCost = newG,
                        hCost = Heuristic(n, target)
                    };
                }
            }
        }

        return null;
    }

    List<Node> Retrace(Node end)
    {
        List<Node> result = new List<Node>();
        Node current = end;

        while (current != null)
        {
            result.Add(current);
            current = current.parent;
        }

        result.Reverse();
        return result;
    }

    int Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
    }

    Vector3Int ToGrid(Vector3 pos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(pos.x),
            Mathf.FloorToInt(pos.y),
            Mathf.FloorToInt(pos.z)
        );
    }

    bool IsSolid(Vector3Int pos)
    {
        return World.Instance != null &&
               World.Instance.IsBlockSolid(pos);
    }
}

public class Node
{
    public Vector3Int position;
    public Node parent;
    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;
}