using System.Collections.Generic;
using UnityEngine;

public class VesselGenerator : MonoBehaviour
{
    [Header("Network Shape")]
    public int seed = 42;
    public int maxDepth = 5;
    public float startRadius = 36f;
    public float radiusDecay = 0.70f;
    public float minSegmentLength = 30f;
    public float maxSegmentLength = 40f;
    public float branchAngle = 35f;
    public float branchAngleVariance = 15f;
    [Range(0f, 1f)] public float straightChance = 0.15f;

    [Header("Appearance")]
    public Color arteryColor = new Color(0.85f, 0.12f, 0.12f);
    public Color veinColor   = new Color(0.12f, 0.22f, 0.80f);
    public bool debugWalls = false;

    [Header("Player")]
    public GameObject playerPrefab;
    public bool spawnPlayer = true;

    readonly List<VesselNode> _nodes = new();
    readonly List<VesselEdge> _edges = new();
    Material _lineMat;

    void Awake()
    {
        _lineMat   = MakeLineMaterial();
        Random.InitState(seed);
        BuildNetwork();
        BuildGeometry();
        if (spawnPlayer && playerPrefab != null)
            Instantiate(playerPrefab, (Vector3)_nodes[0].pos, Quaternion.identity);

        // Fit camera so the full tube width is visible — startRadius * 1.5 shows walls + breathing room
        if (Camera.main != null)
            Camera.main.orthographicSize = startRadius * 1.5f;
    }

    // ── Network generation ────────────────────────────────────────

    void BuildNetwork()
    {
        var root = new VesselNode(Vector2.zero, startRadius, 0, isArtery: true);
        _nodes.Add(root);
        Sprout(root, Vector2.up, maxDepth);
    }

    void Sprout(VesselNode parent, Vector2 dir, int depth, BranchSide side = BranchSide.None)
    {
        if (depth <= 0) return;

        float len   = Random.Range(minSegmentLength, maxSegmentLength);
        float r     = parent.radius * radiusDecay;
        bool artery = parent.isArtery && depth > Mathf.Max(1, maxDepth - 2);

        var child = new VesselNode(parent.pos + dir * len, r, parent.depth + 1, artery);
        _nodes.Add(child);
        _edges.Add(new VesselEdge(parent, child, side));

        if (depth == 1) return;

        if (Random.value < straightChance)
        {
            Sprout(child, Rot(dir, Random.Range(-12f, 12f)), depth - 1, BranchSide.None);
        }
        else
        {
            float depthScale = (float)depth / maxDepth;
            float angle = branchAngle * depthScale;
            float a = angle + Random.Range(-branchAngleVariance, branchAngleVariance) * depthScale;
            float b = angle + Random.Range(-branchAngleVariance, branchAngleVariance) * depthScale;
            Sprout(child, Rot(dir,  a), depth - 1, BranchSide.Left);
            Sprout(child, Rot(dir, -b), depth - 1, BranchSide.Right);
        }
    }

    static Vector2 Rot(Vector2 v, float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad), s = Mathf.Sin(rad);
        return new Vector2(c * v.x - s * v.y, s * v.x + c * v.y);
    }

    // ── Scene geometry ────────────────────────────────────────────

    void BuildGeometry()
    {
        var root = new GameObject("VesselNetwork");

        // Group edges by junction node to find sibling pairs
        var childEdges = new Dictionary<VesselNode, List<VesselEdge>>();
        foreach (var e in _edges)
        {
            if (!childEdges.TryGetValue(e.from, out var list))
                childEdges[e.from] = list = new List<VesselEdge>();
            list.Add(e);
        }

        // For each branching junction, compute where the two inner walls intersect
        var innerWallStart = new Dictionary<VesselEdge, Vector2>();

        foreach (var (junction, children) in childEdges)
        {
            VesselEdge eL = null, eR = null;
            foreach (var c in children)
            {
                if (c.side == BranchSide.Left)  eL = c;
                if (c.side == BranchSide.Right) eR = c;
            }
            if (eL == null || eR == null) continue;

            float r    = junction.radius;
            Vector2 j  = junction.pos;
            Vector2 dL = (eL.to.pos - j).normalized;
            Vector2 dR = (eR.to.pos - j).normalized;
            Vector2 pL = new Vector2(-dL.y, dL.x);
            Vector2 pR = new Vector2(-dR.y, dR.x);

            // Left inner wall ray:  P1 = j - pL*r, dir = dL
            // Right inner wall ray: P2 = j + pR*r, dir = dR
            Vector2 p1 = j - pL * r;
            Vector2 p2 = j + pR * r;
            Vector2 delta = p2 - p1;
            float cross = dL.x * dR.y - dL.y * dR.x; // dL × dR

            if (Mathf.Abs(cross) > 1e-4f)
            {
                float t = (delta.x * dR.y - delta.y * dR.x) / cross;
                if (t >= 0f)
                {
                    Vector2 meet = p1 + t * dL;
                    innerWallStart[eL] = meet;
                    innerWallStart[eR] = meet;
                }
            }
        }

        var hasChildren = new HashSet<VesselNode>(childEdges.Keys);

        foreach (var e in _edges)
        {
            innerWallStart.TryGetValue(e, out Vector2 meet);
            MakeCorridor(e, root, innerWallStart.ContainsKey(e) ? meet : (Vector2?)null);
        }

        foreach (var n in _nodes)
        {
            // MakeJunction(n, root);
            if (!hasChildren.Contains(n))
                MakeEndCap(n, root);
        }
    }

    void MakeCorridor(VesselEdge e, GameObject parent, Vector2? innerStart = null)
    {
        var go = new GameObject($"Seg_{e.from.depth}_{e.to.depth}");
        go.transform.SetParent(parent.transform);

        Vector2 a = e.from.pos, b = e.to.pos;
        float segLen = Vector2.Distance(a, b);
        Vector2 dir  = (b - a) / segLen;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace  = true;
        lr.positionCount  = 2;
        lr.SetPosition(0, (Vector3)a);
        lr.SetPosition(1, (Vector3)b);
        lr.startWidth = e.from.radius * 2f;
        lr.endWidth   = e.to.radius   * 2f;
        lr.material   = _lineMat;
        Color col = e.from.isArtery ? arteryColor : veinColor;
        lr.startColor = lr.endColor = col;
        lr.sortingOrder = -2;

        Vector2 outerStart = a;  // outer wall always starts at the junction node
        Vector2 wallEnd    = b;

        if (e.side == BranchSide.Left)
        {
            // +perp = outer (left side),  -perp = inner (meets sibling)
            AddWall(go, outerStart + perp * e.from.radius, wallEnd + perp * e.to.radius);
            AddWall(go, innerStart ?? a - perp * e.from.radius, wallEnd - perp * e.to.radius);
        }
        else if (e.side == BranchSide.Right)
        {
            // -perp = outer (right side), +perp = inner (meets sibling)
            AddWall(go, innerStart ?? a + perp * e.from.radius, wallEnd + perp * e.to.radius);
            AddWall(go, outerStart - perp * e.from.radius, wallEnd - perp * e.to.radius);
        }
        else
        {
            AddWall(go, a + perp * e.from.radius, b + perp * e.to.radius);
            AddWall(go, a - perp * e.from.radius, b - perp * e.to.radius);
        }
    }

    void AddWall(GameObject parent, Vector2 p0, Vector2 p1)
    {
        var wgo = new GameObject("Wall");
        wgo.transform.SetParent(parent.transform);
        var ec = wgo.AddComponent<EdgeCollider2D>();
        ec.SetPoints(new List<Vector2> { p0, p1 });

        if (!debugWalls) return;
        var lr = wgo.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.SetPosition(0, (Vector3)p0);
        lr.SetPosition(1, (Vector3)p1);
        lr.startWidth = lr.endWidth = 0.3f;
        lr.material = _lineMat;
        lr.startColor = lr.endColor = Color.yellow;
        lr.sortingOrder = 10;
    }


    // Seal the open end of a leaf segment so the player can't escape
    void MakeEndCap(VesselNode leaf, GameObject parent)
    {
        foreach (var e in _edges)
        {
            if (e.to != leaf) continue;
            Vector2 dir  = (e.to.pos - e.from.pos).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x);
            Vector2 left  = leaf.pos + perp * leaf.radius;
            Vector2 right = leaf.pos - perp * leaf.radius;

            var go = new GameObject("Cap");
            go.transform.SetParent(parent.transform);
            var ec = go.AddComponent<EdgeCollider2D>();
            ec.SetPoints(new List<Vector2> { left, right });
        }
    }

    // ── Helpers ───────────────────────────────────────────────────

    static Material MakeLineMaterial()
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        if (mat == null)
            mat = new Material(Shader.Find("Sprites/Default"));
        return mat;
    }
}

class VesselNode
{
    public readonly Vector2 pos;
    public readonly float   radius;
    public readonly int     depth;
    public readonly bool    isArtery;
    public VesselNode(Vector2 pos, float radius, int depth, bool isArtery)
    {
        this.pos = pos; this.radius = radius; this.depth = depth; this.isArtery = isArtery;
    }
}

enum BranchSide { None, Left, Right }

class VesselEdge
{
    public readonly VesselNode from, to;
    public readonly BranchSide side;
    public VesselEdge(VesselNode from, VesselNode to, BranchSide side = BranchSide.None)
    {
        this.from = from; this.to = to; this.side = side;
    }
}
