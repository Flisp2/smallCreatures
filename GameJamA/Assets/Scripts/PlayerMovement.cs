using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCode : MonoBehaviour
{
    public Vector2 direction;
    public float cooldown = 0f;
    private Rigidbody2D rb;
    private Animator ani;

    //Player Stats//
    public float maxHealth = 5f;
    [SerializeField] private float currentHealth = 5f;
    public float stunTime = 0f;
    public float speed = 5f;
    public float baseSpeed = 5f;
    public bool isHidden = false;
    public bool isInvulnerable = false;
    public AudioSource PlayerAudio;
    public AudioClip moveSound;

    private Keyboard kb;
    private Mouse ms;

    //Player Abilities Slots//
    public static Ability ability1;
    public static Ability ability2;

    private void Awake()
    {
        direction = Vector2.zero;
        rb = GetComponent<Rigidbody2D>();
        PlayerAudio = GetComponent<AudioSource>();
        ani = GetComponent<Animator>();

    }

    private void Update()
    {
        kb = Keyboard.current;
        ms = Mouse.current;

        MovePlayer();
        AbilityCheck();
        PlayAuido();
        if (cooldown > 0f)        {
            cooldown -= Time.deltaTime;
        }
        if (kb.spaceKey.wasPressedThisFrame)
        {
            Die();
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
        if (isHidden || isInvulnerable) return; 
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
    public void PlayAuido()
    {
        if (direction.magnitude > 0.1f)
        {
            var randomPitch = Random.Range(0.8f, 1.2f);
            var randomTime = Random.Range(1f, 100f);
            if (randomTime < 2f)
            {
                if (!PlayerAudio.isPlaying)
                {
                    PlayerAudio.pitch = randomPitch;
                    PlayerAudio.PlayOneShot(moveSound);
                }
            }
        }  
    }
    public float GetCooldown(Ability ability)
    {
        if (ability == ability1)
        {
            return ability1.timer;
        }
        else if (ability == ability2)
        {
            return ability2.timer;
        }
        return 0f;
    }

    private void Die()
    {
        Debug.Log("Player has died.");
        ani.SetTrigger("Death");
        ability1 = null;
        ability2 = null;
        StartCoroutine(SlowDownTime());
        WorldData.DeathScreen();
        this.enabled = false;
    }

    private System.Collections.IEnumerator SlowDownTime()
    {
        float duration = 3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        Time.timeScale = 0f;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
