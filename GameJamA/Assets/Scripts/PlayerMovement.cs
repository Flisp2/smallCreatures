using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCode : MonoBehaviour
{
    public Vector2 direction;
    [SerializeField] private float cooldown = 0f;
    private Rigidbody2D rb;

    //Player Stats//
    public float maxHealth = 10f;
    [SerializeField] private float currentHealth = 10f;
    public float stunTime = 0f;
    public float speed = 5f;
    public float baseSpeed = 5f;
    public bool isHidden = false;

    private Keyboard kb;
    private Mouse ms;

    //Player Abilities Slots//
    public Ability ability1;
    public Ability ability2;

    private void Awake()
    {
        direction = Vector2.zero;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        kb = Keyboard.current;
        ms = Mouse.current;

        MovePlayer();
        AbilityCheck();
        if (cooldown > 0f)        {
            cooldown -= Time.deltaTime;
        }
    }

    private void MovePlayer()
    {
        if (kb == null) return;

        direction = Vector2.zero;

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        direction = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        Vector2 targetVelocity = direction * speed;
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

    private void AbilityCheck()
    {
        if (ability1 != null)
        {
            ability1.UpdateTimer();
        }
        if (ability2 != null)
        {
            ability2.UpdateTimer();
        }
        if (ms.rightButton.wasPressedThisFrame)
        {
            if (ability1 != null)
            {
                ability1.Use(gameObject);
            }
            else
            {
                Debug.Log("No ability assigned to right click.");
            }
        }
        if (ms.leftButton.wasPressedThisFrame)
        {
            if (ability2 != null)
            {
                ability2.Use(gameObject);
            }
            else
            {
                Debug.Log("No ability assigned to left click.");
            }
        }
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void UpdateColliderScale(Vector2 newSize)
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.radius = newSize.x;

        }
    }

    private void Die()
    {
        Debug.Log("Player has died.");
        // Implement death behavior (e.g., respawn, game over screen)
    }
}
