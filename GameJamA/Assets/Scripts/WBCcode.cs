using UnityEngine;

public class WBCcode : MonoBehaviour
{
    [SerializeField] private WBCState currentState;
    private enum WBCState
    {
        Patrolling,
        Chasing,
        Attacking,
        Stunned
    }

    [Header("Detection")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float stunDuration = 2f;

    public Transform target;
    private Rigidbody2D rb;
    private int patrolIndex;
    private float attackTimer;
    private float stunTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = WBCState.Patrolling;
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case WBCState.Patrolling: HandlePatrol();  break;
            case WBCState.Chasing:   HandleChase();    break;
            case WBCState.Attacking: HandleAttack();   break;
            case WBCState.Stunned:   HandleStunned();  break;
        }
    }

    // ── States ────────────────────────────────────────────────────────────────

    private void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // Move towards the current patrol point
        Transform goal = patrolPoints[patrolIndex];
        MoveTowards(goal.position, patrolSpeed);

        if (Vector2.Distance(transform.position, goal.position) < 0.2f)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    private void HandleChase()
    {
        if (target == null) { currentState = WBCState.Patrolling; return; }

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > detectionRange)
        {
            target = null;
            currentState = WBCState.Patrolling;
            return;
        }

        if (dist <= attackRange)
        {
            currentState = WBCState.Attacking;
            return;
        }

        MoveTowards(target.position, moveSpeed);
    }

    private void HandleAttack()
    {
        if (target == null) { currentState = WBCState.Patrolling; return; }

        float dist = Vector2.Distance(transform.position, target.position);

        // Target moved out of attack range — chase again
        if (dist > attackRange)
        {
            currentState = WBCState.Chasing;
            return;
        }

        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            PerformAttack();
        }
    }

    private void HandleStunned()
    {
        stunTimer -= Time.deltaTime;
        rb.linearVelocity = Vector2.zero;

        if (stunTimer <= 0f)
            currentState = target != null ? WBCState.Chasing : WBCState.Patrolling;
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    private void PerformAttack()
    {
        if (target == null) return;
        Vector2 chargeDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        rb.AddForce(chargeDir * 5f, ForceMode2D.Impulse);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        currentState = WBCState.Chasing;
    }

    public void Stun()
    {
        stunTimer = stunDuration;
        currentState = WBCState.Stunned;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void MoveTowards(Vector2 goal, float speed)
    {
        Vector2 direction = ((Vector3)goal - transform.position).normalized;
        LookAt2D(goal);
        Vector2 targetVelocity = direction * speed;
        float currentSpeed = rb.linearVelocity.magnitude;
        float targetSpeed = targetVelocity.magnitude;
        
        if (targetSpeed > currentSpeed)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * 10f);
        }
    }

    private void LookAt2D(Vector2 goal)
    {
        Vector2 direction = (goal - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}

