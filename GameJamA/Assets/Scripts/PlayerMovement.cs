using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector2 direction;
    public GameObject bulletPrefab;
    public float Maxcooldown = 0.2f;
    [SerializeField] private float cooldown = 0f;

    private void Update()
    {
        MovePlayer();
        shoot();
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
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    private void shoot()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.spaceKey.isPressed && cooldown <= 0f)
        {
            Vector2 shootDirection = direction.normalized;

            if (shootDirection == Vector2.zero)
            {
                shootDirection = Vector2.up;
            }

            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg - 90f;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.AngleAxis(angle, Vector3.forward));
            bullet.GetComponent<Rigidbody2D>().linearVelocity = shootDirection * 10f;

            Destroy(bullet, 2f);
            cooldown = Maxcooldown;
        }
    }
}
