using UnityEngine;

public class VesselTerminal : MonoBehaviour
{
    public VesselGenerator generator;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<RBCcode>() == null) return;
        Destroy(other.gameObject);
        generator.SpawnRBCAtRoot();
    }
}
