using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float fov = 90f;
    [SerializeField] private int rayCount = 50;
    [SerializeField] private float viewDistance = 10f;

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;
    private WBCcode wbc;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        wbc = transform.parent.GetComponent<WBCcode>();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshRenderer.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(1f, 1f, 1f, 0.5f) };
        meshRenderer.material.SetInt("_Cull", 0);

        vertices = new Vector3[rayCount + 2];
        uv = new Vector2[vertices.Length];
        triangles = new int[rayCount * 3];

        uv[0] = Vector2.zero;
        for (int i = 1; i < uv.Length; i++)
            uv[i] = new Vector2((float)(i - 1) / rayCount, 1f);
    }

    private void Update()
    {
        Vector3 origin = transform.position;
        Vector2 origin2D = new Vector2(origin.x, origin.y);
        float startAngle = -fov / 2f;
        float angleIncrease = fov / rayCount;

        vertices[0] = Vector3.zero;

        int vertexIndex = 1;
        int triangleIndex = 0;
        bool sawTarget = false;

        for (int i = 0; i <= rayCount; i++)
        {
            float localAngle = startAngle + (i * angleIncrease);
            float worldAngle = localAngle + transform.eulerAngles.z;

            Vector2 rayDir = new Vector2(Mathf.Cos(worldAngle * Mathf.Deg2Rad), Mathf.Sin(worldAngle * Mathf.Deg2Rad));
            Vector3 vertex = new Vector3(Mathf.Cos(localAngle * Mathf.Deg2Rad), Mathf.Sin(localAngle * Mathf.Deg2Rad), 0) * viewDistance;

            RaycastHit2D hit = default;
            foreach (RaycastHit2D h in Physics2D.RaycastAll(origin2D, rayDir, viewDistance))
            {
                if (!h.collider.transform.IsChildOf(transform.parent))
                {
                    hit = h;
                    break;
                }
            }
            if (hit.collider != null)
            {
                vertex = transform.InverseTransformPoint(new Vector3(hit.point.x, hit.point.y, origin.z));

                if (hit.collider.CompareTag("Player") && hit.collider.GetComponent<PlayerCode>().isHidden == false)
                {
                    wbc.SetTarget(hit.collider.transform.position);
                    sawTarget = true;
                }
            }

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex]     = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }

            vertexIndex++;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        wbc.seesTarget = sawTarget;
    }
}
