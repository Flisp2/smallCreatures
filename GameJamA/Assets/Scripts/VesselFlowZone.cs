using System.Collections.Generic;
using UnityEngine;

public class VesselFlowZone : MonoBehaviour
{
    public Vector2 flowDirection;
    public float flowForce = 5f;

    readonly HashSet<Rigidbody2D> _bodies = new();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody != null)
            _bodies.Add(other.attachedRigidbody);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.attachedRigidbody != null)
            _bodies.Remove(other.attachedRigidbody);
    }

    void FixedUpdate()
    {
        _bodies.RemoveWhere(rb => rb == null);
        foreach (var rb in _bodies)
            rb.AddForce(flowDirection * flowForce);
    }
}
