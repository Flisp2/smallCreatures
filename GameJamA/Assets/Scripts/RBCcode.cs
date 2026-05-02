using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RBCcode : MonoBehaviour
{
    [SerializeField] float spinRange = 90f;
    [SerializeField] float spawnImmunity = 0.5f;

    public bool IsReady { get; private set; }

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = false;
        rb.angularVelocity = Random.Range(-spinRange, spinRange);
    }

    void Start() => Invoke(nameof(SetReady), spawnImmunity);

    void SetReady() => IsReady = true;
}
