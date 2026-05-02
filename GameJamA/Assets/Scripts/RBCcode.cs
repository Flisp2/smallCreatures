using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RBCcode : MonoBehaviour
{
    [SerializeField] float spinRange = 90f;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = false;
        rb.angularVelocity = Random.Range(-spinRange, spinRange);
    }
}
