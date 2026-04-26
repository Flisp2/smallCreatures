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

    [Header("Combat")]
    [SerializeField] private float chargeTime = 2f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private int attackDamage = 5;

    public Transform testTarget;

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
    public GameObject searchAreaPrefab;
    public bool hunting;
    private void Start()
    {
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

    private void Awake()
    {
        startPosition = transform.position;
        Instantiate(searchAreaPrefab, startPosition, Quaternion.identity);
        searchPoint = startPosition;
        currentState = WBCState.Patrolling;
    }

    private void Update()
    {
        agent.nextPosition = transform.position;
        if (isStunned) 
        {
            currentState = WBCState.Stunned;
            HandleStunned();
            return;
        }
        if (seesTarget)
        {
            attackTimer -= Time.deltaTime;
            currentState = WBCState.Attacking;
        }
        else
        {
            attackTimer = chargeTime;
            if (target != Vector2.zero && !hunting)
                currentState = WBCState.Chasing;
            else if (target == Vector2.zero && hunting)
                currentState = WBCState.Searching;
            else currentState = WBCState.Patrolling;
        }
        switch (currentState)
        {
            case WBCState.Patrolling: HandlePatrol();  break;
            case WBCState.Chasing:   HandleChase();   break;
            case WBCState.Searching: HandleSearch();  break;
            case WBCState.Attacking: HandleAttack();  break;
            case WBCState.Stunned:   HandleStunned(); break;
        }

        if (testTarget) testTarget.position = searchPoint;

        // Drive the rigidbody from the NavMesh path
        if (currentState != WBCState.Attacking && currentState != WBCState.Stunned)
            rb.linearVelocity = agent.desiredVelocity;
        if (agent.desiredVelocity.sqrMagnitude > 0.01f)
            LookAt2D((Vector2)transform.position + (Vector2)agent.desiredVelocity);
        else if (target != Vector2.zero)
            LookAt2D(target);

    }

    // ── States ────────────────────────────────────────────────────────────────

    private Vector2 patrolTarget;
    private bool hasPatrolTarget;

    private void HandlePatrol()
    {
        agent.speed = slowSpeed;
        searchPoint = startPosition;  
        patrol();
    }
    private void HandleChase()
    {
        agent.speed = fastSpeed;
        agent.SetDestination(target);
        searchPoint = target;
        if (Vector2.Distance(transform.position, target) <= 0.5f)
        {
            target = Vector2.zero;
            hunting = true;
            searchTimer = 10f;
            currentState = WBCState.Searching;
        }
    }
    private void HandleSearch()
    {
        agent.speed = mediumSpeed;
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            hunting = false;
            hasPatrolTarget = false;
            currentState = WBCState.Patrolling;
            return;
        }
        patrol();
    }
    private void HandleAttack()
    {
        hunting = false;
        rb.linearVelocity = Vector2.zero;
        attackTimer -= Time.deltaTime;
        LookAt2D(target);
        if (attackTimer <= 0f)
        {
            Stun();
            PerformAttack();  
        }
    }
    public void HandleStunned()
    {
        attackTimer = chargeTime;
        stunTimer -= Time.deltaTime;
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

    private void patrol()
    {
        if (!hasPatrolTarget)
        {
            Vector2 randomDir = Random.insideUnitCircle * searchAreaRadius;
            Vector2 candidate = searchPoint + randomDir;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, searchAreaRadius, NavMesh.AllAreas))
                patrolTarget = hit.position;
            else
                patrolTarget = searchPoint;
            hasPatrolTarget = true;
        }
        
        agent.SetDestination(patrolTarget);
        if (Vector2.Distance(transform.position, patrolTarget) <= 0.5f)
        {
            hasPatrolTarget = false;
        }
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
            if (collision.gameObject.CompareTag("Player") )
            {
                PlayerCode player = collision.gameObject.GetComponent<PlayerCode>();
                if (player != null)
                {
                    player.TakeDamage(attackDamage);
                }
            }
            rb.linearVelocity = -rb.linearVelocity * 0.1f; // Bounce back with reduced speed
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


