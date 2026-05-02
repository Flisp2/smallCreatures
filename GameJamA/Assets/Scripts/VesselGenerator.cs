using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;
using NavMeshPlus.Extensions;
using UnityEngine.InputSystem;

public class VesselGenerator : MonoBehaviour
{
    [Header("Network Shape")]
    public int seed = 42;
    public int maxDepth = 5;
    public float startRadius = 36f;
    public float radiusDecay = 0.90f;
    public float minSegmentLength = 30f;
    public float maxSegmentLength = 40f;
    public float branchAngle = 35f;
    public float branchAngleVariance = 15f;
    [Range(0f, 1f)] public float straightChance = 0.15f;
    public int terminateStartDepth = 3;
    [Range(0f, 1f)] public float terminateBaseChance = 0.2f;

    [Header("Appearance")]
    public Color arteryColor = new Color(0.85f, 0.12f, 0.12f);
    public Color veinColor   = new Color(0.12f, 0.22f, 0.80f);
    public bool debugWalls = false;
    public bool debugNavMesh = false;

    [Header("Player")]
    public GameObject playerPrefab;
    public bool spawnPlayer = true;

    [Header("Red Blood Cells")]
    public GameObject rbcPrefab;
    public int rbcsPerEdge = 3;

    [Header("Enemies")]
    public GameObject wbcPrefab;
    public int maxWBCs = 2;

    [Header("Blood Flow")]
    public float vesselFlowForce = 5f;

    readonly List<VesselNode> _nodes = new();
    readonly List<VesselEdge> _edges = new();
    readonly List<VesselNode> _terminals = new();
    Material _lineMat;

    void Awake()
    {
        _lineMat   = MakeLineMaterial();
        /*Random.InitState(seed);
        BuildNetwork();
        FindTerminals();
        BuildGeometry();
        AssignSpecialRooms();
        SpawnPlayer();*/

        // Fit camera so the full tube width is visible — startRadius * 1.5 shows walls + breathing room
        if (Camera.main != null)
            Camera.main.orthographicSize = startRadius * 1.5f;
    }

    void Start()
    {
    }
    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
            GenerateNew();
    }

    // ── Network generation ────────────────────────────────────────

    void FindTerminals()
    {
        var hasOutgoing = new HashSet<VesselNode>();
        foreach (var e in _edges) hasOutgoing.Add(e.from);
        foreach (var n in _nodes)
            if (!hasOutgoing.Contains(n)) _terminals.Add(n);
    }

    void AssignSpecialRooms()
    {
        if (_terminals.Count < 4)
        {
            Debug.LogWarning($"[VesselGenerator] Only {_terminals.Count} terminal vessels — need 4 for special rooms. Trying next seed.");
            seed++;
            GenerateNew();
            return;
        }

        var pool = new List<VesselNode>(_terminals);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        PlaceRoomMarker(pool[0], Color.green);        // treasure
        PlaceRoomMarker(pool[1], Color.black);        // trap
        PlaceRoomMarker(pool[2], Color.black);        // trap
        PlaceRoomMarker(pool[3], Color.red);          // boss
    }

    void PlaceRoomMarker(VesselNode node, Color color)
    {
        var go = new GameObject("RoomMarker");
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = _lineMat;
        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = 1.5f;
        lr.sortingOrder = 5;

        const int segments = 32;
        float radius = node.radius * 0.5f;
        lr.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            lr.SetPosition(i, new Vector3(
                node.pos.x + Mathf.Cos(angle) * radius,
                node.pos.y + Mathf.Sin(angle) * radius, 0f));
        }
    }

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

        Vector2 childPos = parent.pos + dir * len;

        // Reject branch if its tube would intersect any non-adjacent existing segment
        float newAvgR = (parent.radius + r) * 0.5f;
        foreach (var existing in _edges)
        {
            if (existing.to == parent || existing.from == parent) continue;
            float minDist = SegmentDist(parent.pos, childPos, existing.from.pos, existing.to.pos);
            float minAllowed = newAvgR + (existing.from.radius + existing.to.radius) * 0.5f;
            if (minDist < minAllowed) return;
        }

        var child = new VesselNode(childPos, r, parent.depth + 1, artery);
        _nodes.Add(child);
        _edges.Add(new VesselEdge(parent, child, side));

        if (depth == 1) return;

        if (child.depth >= terminateStartDepth)
        {
            float t = Mathf.InverseLerp(terminateStartDepth, maxDepth, child.depth);
            if (Random.value < Mathf.Lerp(terminateBaseChance, 1f, t)) return;
        }

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

    static float SegmentDist(Vector2 p0, Vector2 p1, Vector2 q0, Vector2 q1)
    {
        Vector2 d1 = p1 - p0, d2 = q1 - q0, r = p0 - q0;
        float a = Vector2.Dot(d1, d1), e = Vector2.Dot(d2, d2), f = Vector2.Dot(d2, r);
        float s, t;
        if (a <= 1e-6f && e <= 1e-6f) return r.magnitude;
        if (a <= 1e-6f) { s = 0f; t = Mathf.Clamp01(f / e); }
        else
        {
            float c = Vector2.Dot(d1, r);
            if (e <= 1e-6f) { t = 0f; s = Mathf.Clamp01(-c / a); }
            else
            {
                float b = Vector2.Dot(d1, d2), denom = a * e - b * b;
                s = denom != 0f ? Mathf.Clamp01((b * f - c * e) / denom) : 0f;
                t = (b * s + f) / e;
                if      (t < 0f) { t = 0f; s = Mathf.Clamp01(-c / a); }
                else if (t > 1f) { t = 1f; s = Mathf.Clamp01((b - c) / a); }
            }
        }
        return Vector2.Distance(p0 + s * d1, q0 + t * d2);
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

        // Build parent-edge lookup so outer wall intersections can reference the incoming segment
        var parentEdge = new Dictionary<VesselNode, VesselEdge>();
        foreach (var e in _edges)
            parentEdge[e.to] = e;

        // For each branching junction, compute where the two inner walls intersect
        // and where each outer wall ray exits the parent vessel's outer wall line.
        var innerWallStart  = new Dictionary<VesselEdge, Vector2>();
        var outerWallStart  = new Dictionary<VesselEdge, Vector2>();
        var wallEndPlusPerp = new Dictionary<VesselEdge, Vector2>(); // end of parent's +perp wall
        var wallEndMinusPerp= new Dictionary<VesselEdge, Vector2>(); // end of parent's -perp wall

        foreach (var (junction, children) in childEdges)
        {
            VesselEdge eL = null, eR = null, eS = null;
            foreach (var c in children)
            {
                if (c.side == BranchSide.Left)  eL = c;
                if (c.side == BranchSide.Right) eR = c;
                if (c.side == BranchSide.None)  eS = c;
            }

            // Straight branch — snap its wall starts to the parent's edges
            if (eS != null && parentEdge.TryGetValue(junction, out VesselEdge peStraight))
            {
                Vector2 pd = (junction.pos - peStraight.from.pos).normalized;
                Vector2 pp = new Vector2(-pd.y, pd.x);
                outerWallStart[eS] = junction.pos + pp * junction.radius;
                innerWallStart[eS] = junction.pos - pp * junction.radius;
            }

            if (eL == null && eR == null) continue;

            // One branch was rejected by the intersection test — pivot its walls on the parent's edges
            if (eL == null || eR == null)
            {
                VesselEdge single = eL ?? eR;
                if (parentEdge.TryGetValue(junction, out VesselEdge ve))
                {
                    Vector2 parentDir  = (junction.pos - ve.from.pos).normalized;
                    Vector2 parentPerp = new Vector2(-parentDir.y, parentDir.x);
                    Vector2 j0 = junction.pos;
                    float   r0 = junction.radius;
                    if (single.side == BranchSide.Left)
                    {
                        outerWallStart[single] = j0 + parentPerp * r0;
                        innerWallStart[single] = j0 - parentPerp * r0;
                    }
                    else
                    {
                        outerWallStart[single] = j0 - parentPerp * r0;
                        innerWallStart[single] = j0 + parentPerp * r0;
                    }
                }
                continue;
            }

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

            // Outer wall starts: intersect each child's outer wall ray with the
            // corresponding side of the parent vessel's outer wall line.
            // Left ray:  from j + pL*r along dL  vs. parent left wall through j + parentPerp*r
            // Right ray: from j - pR*r along dR  vs. parent right wall through j - parentPerp*r
            if (parentEdge.TryGetValue(junction, out VesselEdge pe))
            {
                Vector2 parentDir  = (j - pe.from.pos).normalized;
                Vector2 parentPerp = new Vector2(-parentDir.y, parentDir.x);

                float crossL = dL.x * parentDir.y - dL.y * parentDir.x; // dL × parentDir
                if (Mathf.Abs(crossL) > 1e-4f)
                {
                    Vector2 diffL = parentPerp - pL;
                    float tL = r * (diffL.x * parentDir.y - diffL.y * parentDir.x) / crossL;
                    if (tL >= 0f)
                    {
                        outerWallStart[eL]   = j + pL * r + tL * dL;
                        wallEndPlusPerp[pe]  = outerWallStart[eL]; // parent +perp wall ends here
                    }
                }

                float crossR = dR.x * parentDir.y - dR.y * parentDir.x; // dR × parentDir
                if (Mathf.Abs(crossR) > 1e-4f)
                {
                    Vector2 diffR = pR - parentPerp;
                    float tR = r * (diffR.x * parentDir.y - diffR.y * parentDir.x) / crossR;
                    if (tR >= 0f)
                    {
                        outerWallStart[eR]    = j - pR * r + tR * dR;
                        wallEndMinusPerp[pe]  = outerWallStart[eR]; // parent -perp wall ends here
                    }
                }
            }
        }

        var hasChildren = new HashSet<VesselNode>(childEdges.Keys);

        foreach (var e in _edges)
        {
            Vector2? iStart  = innerWallStart .TryGetValue(e, out Vector2 im) ? im : (Vector2?)null;
            Vector2? oStart  = outerWallStart .TryGetValue(e, out Vector2 om) ? om : (Vector2?)null;
            Vector2? endPlus = wallEndPlusPerp .TryGetValue(e, out Vector2 ep) ? ep : (Vector2?)null;
            Vector2? endMinus= wallEndMinusPerp.TryGetValue(e, out Vector2 en) ? en : (Vector2?)null;
            MakeCorridor(e, root, iStart, oStart, endPlus, endMinus);
        }

        foreach (var n in _nodes)
        {
            // MakeJunction(n, root);
            if (!hasChildren.Contains(n))
                MakeEndCap(n, root);
        }
    }

    void MakeCorridor(VesselEdge e, GameObject parent,
        Vector2? innerStart = null, Vector2? outerStart = null,
        Vector2? wallEndPlusPerp = null, Vector2? wallEndMinusPerp = null)
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

        Vector2 endPlus  = wallEndPlusPerp  ?? b + perp * e.to.radius;
        Vector2 endMinus = wallEndMinusPerp ?? b - perp * e.to.radius;

        if (e.side == BranchSide.Left)
        {
            // +perp = outer (left side),  -perp = inner (meets sibling)
            AddWall(go, outerStart ?? a + perp * e.from.radius, endPlus);
            AddWall(go, innerStart ?? a - perp * e.from.radius, endMinus);
        }
        else if (e.side == BranchSide.Right)
        {
            // -perp = outer (right side), +perp = inner (meets sibling)
            AddWall(go, innerStart ?? a + perp * e.from.radius, endPlus);
            AddWall(go, outerStart ?? a - perp * e.from.radius, endMinus);
        }
        else
        {
            AddWall(go, outerStart ?? a + perp * e.from.radius, endPlus);
            AddWall(go, innerStart ?? a - perp * e.from.radius, endMinus);
        }

        // nav area for NavMesh — trapezoid spanning the corridor interior
        var walkable = new GameObject("Walkable");
        walkable.tag = "floor";
        walkable.transform.SetParent(go.transform);
        var poly = walkable.AddComponent<PolygonCollider2D>();
        var NavWalkArea = walkable.AddComponent<NavMeshModifier>();
        NavWalkArea.overrideArea = true;
        NavWalkArea.area = UnityEngine.AI.NavMesh.GetAreaFromName("Walkable");
        poly.isTrigger = true;
        poly.SetPath(0, new Vector2[]
        {
            a + perp * e.from.radius,
            b + perp * e.to.radius,
            b - perp * e.to.radius,
            a - perp * e.from.radius,
        });

        var flow = walkable.AddComponent<VesselFlowZone>();
        flow.flowDirection = dir;
        flow.flowForce = vesselFlowForce;
    }

    void AddWall(GameObject parent, Vector2 p0, Vector2 p1)
    {
        var wgo = new GameObject("Wall");
        wgo.transform.SetParent(parent.transform);
        var ec = wgo.AddComponent<EdgeCollider2D>();
        ec.SetPoints(new List<Vector2> { p0, p1 });
        var mod = wgo.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;

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
    public void GenerateNew()
    {
        Debug.Log("Generating new vessel network with seed " + seed);
        // Delete all existing objects
        _nodes.Clear();
        _edges.Clear();
        _terminals.Clear();

        var existingNetwork = GameObject.Find("VesselNetwork");
        if (existingNetwork != null)
            Destroy(existingNetwork);

        var existingNavSurface = GameObject.Find("NavSurface");
        if (existingNavSurface != null)
            Destroy(existingNavSurface);

        var existingNavMeshDebug = GameObject.Find("NavMeshDebug");
        if (existingNavMeshDebug != null)
            Destroy(existingNavMeshDebug);

        var existingPlayer = GameObject.FindWithTag("Player");
        if (existingPlayer != null)
        {
            Debug.LogWarning("Player object still exists when generating new vessel network. Destroying it to prevent duplicates.");            
            Destroy(existingPlayer);
        }

        var existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in existingEnemies)
            Destroy(enemy);

        var existingScenery = GameObject.FindGameObjectsWithTag("Scenery");
        foreach (var scenery in existingScenery)
            Destroy(scenery);

        // Regenerate the tree
        Random.InitState(seed);
        BuildNetwork();
        FindTerminals();
        BuildGeometry();
        AssignSpecialRooms();
        StartCoroutine(BakeNavMeshAsync());
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

            var triggerGO = new GameObject("TerminalTrigger");
            triggerGO.transform.SetParent(go.transform);
            // Step back into the vessel so the trigger overlaps before the wall stops the RBC
            triggerGO.transform.position = (Vector3)(leaf.pos - dir * leaf.radius);
            var circle = triggerGO.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = leaf.radius;
            var terminal = triggerGO.AddComponent<VesselTerminal>();
            terminal.generator = this;
        }
    }

    // ── spawning ────────────────────────────────────────────

    void SpawnRBCs()
    {
        if (!rbcPrefab) return;
        foreach (var e in _edges)
        {
            Vector2 dir  = (e.to.pos - e.from.pos).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x);

            for (int i = 0; i < rbcsPerEdge; i++)
            {
                float t = (i + 1f) / (rbcsPerEdge + 1f);
                float lateralRange = Mathf.Lerp(e.from.radius, e.to.radius, t) * 0.6f;
                Vector2 pos = Vector2.Lerp(e.from.pos, e.to.pos, t)
                            + perp * Random.Range(-lateralRange, lateralRange);
                Instantiate(rbcPrefab, (Vector3)pos, Quaternion.identity);
            }
        }
    }

    public void SpawnRBCAtRoot()
    {
        if (!rbcPrefab || _nodes.Count == 0) return;
        VesselNode root = _nodes[0];
        Vector2 pos = root.pos;
        foreach (var e in _edges)
        {
            if (e.from != root) continue;
            Vector2 perp = new Vector2(-(e.to.pos - root.pos).normalized.y, (e.to.pos - root.pos).normalized.x);
            pos = root.pos + perp * Random.Range(-root.radius * 0.6f, root.radius * 0.6f);
            break;
        }
        Instantiate(rbcPrefab, (Vector3)pos, Quaternion.identity);
    }

    void SpawnWBCs()
    {
        if (!wbcPrefab) return;
        foreach (var e in _edges)
        {
            Vector2 mid = (e.from.pos + e.to.pos) * 0.5f;
            int count = Random.Range(0, maxWBCs);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 0.3f;
                var wbc = Instantiate(wbcPrefab, (Vector3)(mid + offset), Quaternion.identity);
                var wbcCode = wbc.GetComponent<WBCcode>();
            }
        }
    }
    void SpawnPlayer()
    {
        if (spawnPlayer && playerPrefab != null)
            Instantiate(playerPrefab, (Vector3)_nodes[0].pos + Vector3.up * 0.5f, Quaternion.identity);
    }

    // ── NavMesh ───────────────────────────────────────────────────

    IEnumerator BakeNavMeshAsync()
    {
        yield return null; // wait one frame so all colliders are fully settled

        var surfaceGO = new GameObject("NavSurface");
        var surface = surfaceGO.AddComponent<NavMeshSurface>();
        surfaceGO.AddComponent<CollectSources2d>();

        surfaceGO.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // Align NavMesh with XY plane

        surface.collectObjects = CollectObjects.All;
        surface.useGeometry    = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;

        yield return surface.BuildNavMeshAsync();

        SpawnPlayer();
        SpawnRBCs();
        SpawnWBCs();

        if (debugNavMesh)
            DrawNavMeshDebug();
    }

    void DrawNavMeshDebug()
    {
        var tri = UnityEngine.AI.NavMesh.CalculateTriangulation();
        Debug.Log($"[NavMesh] triangulation: {tri.vertices.Length} vertices, {tri.indices.Length / 3} triangles");
        if (tri.vertices.Length == 0) return;

        var go = new GameObject("NavMeshDebug");
        var mesh = new Mesh { vertices = tri.vertices, triangles = tri.indices };
        mesh.RecalculateNormals();

        go.AddComponent<MeshFilter>().mesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")) { color = new Color(0f, 0.8f, 1f, 1f) };
        mr.sortingOrder = 5;
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
