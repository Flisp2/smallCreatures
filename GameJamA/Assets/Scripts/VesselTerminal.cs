using UnityEngine;

public class VesselTerminal : MonoBehaviour
{
    public VesselGenerator generator;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[VesselTerminal] trigger hit by '{other.gameObject.name}' (layer: {LayerMask.LayerToName(other.gameObject.layer)})");
        var rb = other.attachedRigidbody;
        if (rb == null) { Debug.Log("[VesselTerminal] no attachedRigidbody — skipping"); return; }
        var rbc = rb.GetComponent<RBCcode>();
        if (rbc == null) { Debug.Log($"[VesselTerminal] no RBCcode on '{rb.gameObject.name}' — skipping"); return; }
        if (!rbc.IsReady) { Debug.Log($"[VesselTerminal] RBC '{rb.gameObject.name}' still in spawn immunity — skipping"); return; }
        Debug.Log($"[VesselTerminal] destroying RBC '{rb.gameObject.name}' and respawning at root");
        Destroy(rb.gameObject);
        generator.SpawnRBCAtRoot();
    }
}
