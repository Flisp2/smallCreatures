using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

// DEBUG ONLY — attach to the WBC enemy. Click anywhere to send it there.
public class DebugClickMove : MonoBehaviour
{
    private NavMeshAgent agent;
    private Rigidbody2D rb;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        var ms = Mouse.current;
        if (ms == null) return;

        if (ms.leftButton.wasPressedThisFrame)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(ms.position.ReadValue());
            mouseWorldPos.z = 0f;
            agent.SetDestination(mouseWorldPos);
        }

        rb.linearVelocity = (Vector2)agent.desiredVelocity.normalized * agent.speed;
        agent.nextPosition = transform.position;
    }
}
