using UnityEngine;

// Attach this to a second Camera GameObject (child of main camera or standalone).
// It renders the full vessel network as a minimap in the top-right corner.
[RequireComponent(typeof(Camera))]
public class VesselMap : MonoBehaviour
{
    [Range(0.05f, 0.45f)] public float mapWidth  = 0.26f;
    [Range(0.05f, 0.45f)] public float mapHeight = 0.26f;
    public float padding = 6f;

    Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic  = true;
        _cam.depth         = 2;
        _cam.clearFlags    = CameraClearFlags.SolidColor;
        _cam.backgroundColor = new Color(0.04f, 0.04f, 0.10f, 1f);
        _cam.rect = new Rect(1f - mapWidth - 0.02f, 1f - mapHeight - 0.02f, mapWidth, mapHeight);
    }

    void Start()
    {
        // VesselGenerator.Awake has already run by the time Start fires
        FitToNetwork();
    }

    void FitToNetwork()
    {
        var network = GameObject.Find("VesselNetwork");
        if (network == null) return;

        var renderers = network.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds b = renderers[0].bounds;
        foreach (var r in renderers)
            b.Encapsulate(r.bounds);

        Vector3 center = b.center;
        center.z = transform.position.z;
        transform.position = center;

        float aspect = mapWidth / mapHeight;
        float sizeFromH = b.extents.y + padding;
        float sizeFromW = (b.extents.x + padding) / aspect;
        _cam.orthographicSize = Mathf.Max(sizeFromH, sizeFromW);
    }
}
