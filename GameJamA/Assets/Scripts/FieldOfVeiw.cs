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

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Ensure we have a MeshRenderer component
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Assign a default material if none exists
        if (meshRenderer.material == null)
        {
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.color = Color.red; // Make it visible
        }

        meshRenderer.material.SetInt("_Cull", 0);

        // Initialize arrays
        vertices = new Vector3[rayCount + 1 + 1];
        uv = new Vector2[vertices.Length];
        triangles = new int[rayCount * 3];

        // Set up UV coordinates (these don't change)
        uv[0] = Vector2.zero;
        for (int i = 1; i < uv.Length; i++)
        {
            uv[i] = new Vector2((float)(i-1) / rayCount, 1f);
        }
    }

    private void Update()
    {
        Vector3 origin = transform.position;
        Vector2 origin2D = new Vector2(origin.x, origin.y);
        float startAngle = -fov / 2f;
        float angleIncrease = fov / rayCount;

        // Ensure the origin vertex is exactly at the transform position (local space center)
        vertices[0] = Vector3.zero;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            // Calculate angle relative to transform's rotation
            float localAngle = startAngle + (i * angleIncrease);
            float worldAngle = localAngle + transform.eulerAngles.z;
            
            // Ray direction in world space
            Vector2 rayDirection2D = new Vector2(Mathf.Cos(worldAngle * Mathf.Deg2Rad), Mathf.Sin(worldAngle * Mathf.Deg2Rad));
            
            // Default vertex position in local space (extends FROM the origin)
            Vector3 localRayDirection = new Vector3(Mathf.Cos(localAngle * Mathf.Deg2Rad), Mathf.Sin(localAngle * Mathf.Deg2Rad), 0);
            Vector3 vertex = localRayDirection * viewDistance;
            
            RaycastHit2D hit = Physics2D.Raycast(origin2D, rayDirection2D, viewDistance);
            if (hit.collider != null && hit.collider.transform != this.transform)
            {
                // Convert world hit point to local space relative to transform
                Vector3 worldHitPoint = new Vector3(hit.point.x, hit.point.y, origin.z);
                vertex = transform.InverseTransformPoint(worldHitPoint);
            }

            vertices[vertexIndex] = vertex;

            if (i > 0)            
            {
                triangles[triangleIndex + 0] = 0; // Always connect back to origin (transform position)
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }
            vertexIndex++;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
