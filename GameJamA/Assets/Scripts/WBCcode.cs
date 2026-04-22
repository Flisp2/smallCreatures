using UnityEngine;
using UnityEngine.AI;

public class WBCcode : MonoBehaviour
{
    [SerializeField] private WBCState currentState;
    private enum WBCState { Patrolling, Chasing, Attacking, Stunned }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Combat")]
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float chargeTime = 2f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float rotationSpeed = 5f;

    public Transform target;
    public Transform lastSeenTarget;
    public bool seesTarget;

    private NavMeshAgent agent;
    private Rigidbody2D rb;
    private int patrolIndex;
    public float attackTimer;
    private float stunTimer;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        currentState = WBCState.Patrolling;
    }

    private void Update()
    {
        if (seesTarget)
        {
            attackTimer -= Time.deltaTime;
            lastSeenTarget = target;
            currentState = WBCState.Attacking;
        }
        else
        {
            attackTimer = chargeTime;
            currentState = WBCState.Chasing;
            if (lastSeenTarget == null)
            currentState = WBCState.Patrolling;
        }
        switch (currentState)
        {
            case WBCState.Patrolling: HandlePatrol();  break;
            case WBCState.Chasing:   HandleChase();   break;
            case WBCState.Attacking: HandleAttack();  break;
            case WBCState.Stunned:   HandleStunned(); break;
        }

        agent.nextPosition = transform.position;
        if (agent.desiredVelocity.sqrMagnitude > 0.01f)
            LookAt2D((Vector2)transform.position + (Vector2)agent.desiredVelocity);
        else if (target != null)
            LookAt2D(target.position);

    }

    // ── States ────────────────────────────────────────────────────────────────

    private Vector2 patrolTarget;
    private bool hasPatrolTarget;

    private void HandlePatrol()
    {
        if (!hasPatrolTarget || Vector2.Distance(transform.position, patrolTarget) <= 0.5f)
        {
            if (TryGetRandomNavMeshPoint(out Vector2 point))
            {
                patrolTarget = point;
                hasPatrolTarget = true;
                agent.SetDestination(patrolTarget);
            }
        }

        agent.speed = patrolSpeed;
        rb.linearVelocity = (Vector2)agent.desiredVelocity.normalized * patrolSpeed;
    }
    private void HandleChase()
    {
        if (Vector2.Distance(transform.position, lastSeenTarget.position) <= 0.5f)
        {
            currentState = WBCState.Patrolling;
        }
    }
    private void HandleAttack()
    {
        rb.linearVelocity = Vector2.zero;
        attackTimer -= Time.deltaTime;
        LookAt2D(target.position);
        if (attackTimer <= 0f)
        {
            PerformAttack();            
        }
    }
    public void HandleStunned()
    {
        rb.linearVelocity = Vector2.zero;
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
            currentState = WBCState.Chasing;
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    private void PerformAttack()
    {
        if (target == null) return;
        Vector2 chargeDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        rb.AddForce(chargeDir * 30f, ForceMode2D.Impulse);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void Stun()
    {
        stunTimer = stunDuration;
        currentState = WBCState.Stunned;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool TryGetRandomNavMeshPoint(out Vector2 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle * 10f;
            Vector3 candidate = transform.position + new Vector3(randomDir.x, randomDir.y, 0f);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                result = new Vector2(hit.position.x, hit.position.y);
                return true;
            }
        }
        result = transform.position;
        return false;
    }

    private void LookAt2D(Vector2 goal)
    {
        Vector2 direction = (goal - (Vector2)transform.position).normalized;
        if (direction == Vector2.zero) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * 360f * Time.deltaTime);
    }

    public void SetPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
        patrolIndex = 0;
    }
}


