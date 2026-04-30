using UnityEngine;
using UnityEngine.AI;

public class WBCcode : MonoBehaviour
{
    [SerializeField] private WBCState currentState;
    private enum WBCState { Patrolling, Chasing, Searching, Attacking, Stunned }

    [Header("Movement")]
    [SerializeField] private float slowSpeed = 2f;
    [SerializeField] private float mediumSpeed = 4f;
    [SerializeField] private float fastSpeed = 6f;

    [Header("Optimization")]
    [SerializeField] private float activationRange = 20f;
    [SerializeField] private float navUpdateInterval = 0.25f;

    [Header("Combat")]
    [SerializeField] private float chargeTime = 2f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private int attackDamage = 5;

    public Vector2 target;
    public bool seesTarget;
    public Vector2 searchPoint;
    public float searchAreaRadius = 5f;
    public bool isStunned;
    private Vector2 startPosition;

    private NavMeshAgent agent;
    private Rigidbody2D rb;
    private Animator ani;
    private GameObject FOV;
    private int patrolIndex;
    public float attackTimer;
    private float stunTimer;
    private float searchTimer;
    public bool hunting;

    // Shared across all instances — FindWithTag only runs once total
    private static Transform _playerTransform;

    // Cached per-instance constants
    private float _sqrActivationRange;
    private float _navUpdateTimer;

    private void Awake()
    {
        startPosition = transform.position;
        searchPoint = startPosition;
        currentState = WBCState.Patrolling;
    }

    private void Start()
    {
        if (_playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) _playerTransform = playerObj.transform;
        }
        _sqrActivationRange = activationRange * activationRange;

        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        FOV = transform.Find("FOV").gameObject;
        rb.freezeRotation = true;
        currentState = WBCState.Patrolling;
    }

    private void Update()
    {
        // Cache per-frame values used multiple times
        float dt = Time.deltaTime;
        Vector2 myPos = transform.position;

        if (_playerTransform != null)
        {
            Vector2 toPlayer = (Vector2)_playerTransform.position - myPos;
            if (toPlayer.sqrMagnitude > _sqrActivationRange)
            {
                rb.linearVelocity = Vector2.zero;
                ani.enabled = false;
                return;
            }
            ani.enabled = true;
        }

        _navUpdateTimer -= dt;
        agent.nextPosition = myPos;

        if (isStunned)
        {
            currentState = WBCState.Stunned;
            HandleStunned(dt);
            return;
        }

        if (seesTarget)
        {
            attackTimer -= dt;
            currentState = WBCState.Attacking;
        }
        else
        {
            attackTimer = chargeTime;
            if (target != Vector2.zero && !hunting)
                currentState = WBCState.Chasing;
            else if (target == Vector2.zero && hunting)
                currentState = WBCState.Searching;
            else
                currentState = WBCState.Patrolling;
        }

        switch (currentState)
        {
            case WBCState.Patrolling: HandlePatrol(myPos);       break;
            case WBCState.Chasing:   HandleChase(myPos);        break;
            case WBCState.Searching: HandleSearch(myPos, dt);   break;
            case WBCState.Attacking: HandleAttack(dt);          break;
            case WBCState.Stunned:   HandleStunned(dt);         break;
        }

        bool isActive = currentState != WBCState.Attacking && currentState != WBCState.Stunned;
        Vector3 desiredVel = agent.desiredVelocity;

        if (isActive)
            rb.linearVelocity = desiredVel;

        if (isActive && desiredVel.sqrMagnitude > 0.01f)
            LookAt2D(myPos + (Vector2)desiredVel);
        else if (target != Vector2.zero)
            LookAt2D(target);
    }

    // ── States ────────────────────────────────────────────────────────────────

    public Vector2 patrolTarget;
    private bool hasPatrolTarget;

    private void HandlePatrol(Vector2 myPos)
    {
        agent.speed = slowSpeed;
        searchPoint = startPosition;
        Patrol(myPos);
    }

    private void HandleChase(Vector2 myPos)
    {
        agent.speed = fastSpeed;
        if (_navUpdateTimer <= 0f)
        {
            agent.SetDestination(target);
            _navUpdateTimer = navUpdateInterval;
        }
        searchPoint = target;
        // 0.5f^2 = 0.25f — avoids sqrt
        if ((myPos - target).sqrMagnitude <= 0.25f)
        {
            target = Vector2.zero;
            hunting = true;
            searchTimer = 10f;
            currentState = WBCState.Searching;
        }
    }

    private void HandleSearch(Vector2 myPos, float dt)
    {
        agent.speed = mediumSpeed;
        searchTimer -= dt;
        if (searchTimer <= 0f)
        {
            hunting = false;
            hasPatrolTarget = false;
            currentState = WBCState.Patrolling;
            return;
        }
        Patrol(myPos);
    }

    private void HandleAttack(float dt)
    {
        hunting = false;
        rb.linearVelocity = Vector2.zero;
        attackTimer -= dt;
        LookAt2D(target);
        if (attackTimer <= 0f)
        {
            Stun();
            PerformAttack();
        }
    }

    public void HandleStunned() => HandleStunned(Time.deltaTime);

    private void HandleStunned(float dt)
    {
        attackTimer = chargeTime;
        stunTimer -= dt;
        FOV.SetActive(false);
        ani.SetBool("isStunned", true);
        agent.speed = 0f;
        rb.freezeRotation = false;
        isStunned = true;
        if (stunTimer <= 0f)
        {
            rb.freezeRotation = true;
            FOV.SetActive(true);
            isStunned = false;
            ani.SetBool("isStunned", false);
            currentState = WBCState.Chasing;
        }
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    private void PerformAttack()
    {
        if (target == Vector2.zero) return;
        Vector2 chargeDir = (target - (Vector2)transform.position).normalized;
        rb.AddForce(chargeDir * 30f, ForceMode2D.Impulse);
    }

    public void SetTarget(Vector2 newTarget)
    {
        target = newTarget;
    }

    private void Patrol(Vector2 myPos)
    {
        if (!hasPatrolTarget)
        {
            Vector2 randomDir = Random.insideUnitCircle * searchAreaRadius;
            Vector2 candidate = searchPoint + randomDir;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, searchAreaRadius, NavMesh.AllAreas))
                patrolTarget = hit.position;
            else
            {
                Debug.LogWarning("Failed to find valid patrol point near " + candidate);
                patrolTarget = searchPoint;
            }
            hasPatrolTarget = true;
        }

        if (_navUpdateTimer <= 0f)
        {
            agent.SetDestination(patrolTarget);
            _navUpdateTimer = navUpdateInterval;
        }

        // 0.5f^2 = 0.25f — avoids sqrt
        if ((myPos - patrolTarget).sqrMagnitude <= 0.25f)
            hasPatrolTarget = false;
    }

    public void Stun()
    {
        isStunned = true;
        stunTimer = stunDuration;
        currentState = WBCState.Stunned;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == WBCState.Stunned)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerCode player = collision.gameObject.GetComponent<PlayerCode>();
                if (player != null)
                    player.TakeDamage(attackDamage);
            }
            rb.linearVelocity = -rb.linearVelocity * 0.1f;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void LookAt2D(Vector2 goal)
    {
        Vector2 direction = goal - (Vector2)transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}


