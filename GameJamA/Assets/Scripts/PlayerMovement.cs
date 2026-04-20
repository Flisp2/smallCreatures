using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector2 direction;
    public float Maxcooldown = 0.2f;
    [SerializeField] private float cooldown = 0f;
    private Rigidbody2D rb;

    private void Awake()
    {
        direction = Vector2.zero;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        MovePlayer();
        if (cooldown > 0f)        {
            cooldown -= Time.deltaTime;
        }
    }

    private void MovePlayer()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        direction = Vector2.zero;

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        direction = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        Vector2 targetVelocity = direction * moveSpeed;
        float currentSpeed = rb.linearVelocity.magnitude;
        float targetSpeed = targetVelocity.magnitude;
        
        if (targetSpeed > currentSpeed)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * 10f);
        }
        if (rb.linearVelocity.x < 0f || rb.linearVelocity.y < 0f)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, -50f, Time.deltaTime * 200f);
        }
        else if (rb.linearVelocity.x > 0f || rb.linearVelocity.y > 0f)
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 50f, Time.deltaTime * 200f);
        }
        else
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0f, Time.deltaTime * 200f);
        }
    }
}
