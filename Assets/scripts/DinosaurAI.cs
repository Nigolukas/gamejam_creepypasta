using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DinosaurAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;

    public float visionRange = 10f;
    public float visionAngle = 60f;
    public float patrolRadius = 15f;
    public float patrolDelay = 3f;

    private Vector3 patrolTarget;
    private float patrolTimer;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Detecta automáticamente al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        SetNewPatrolPoint();
    }

    void Update()
    {
        if (player != null && CanSeePlayer())
        {
            // Perseguir

            StartCoroutine(RoarThenChase());


        }
        else
        {
            // Patrullar
            isChasing = false;
            Patrol();
        }

        // Pasar la variable al Animator
        animator.SetBool("isChasing", isChasing);
    }

    IEnumerator RoarThenChase()
    {
        
        isChasing = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(1);
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolDelay)
            {
                SetNewPatrolPoint();
                patrolTimer = 0f;
            }
        }
    }

    void SetNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
        {
            patrolTarget = hit.position;
            agent.SetDestination(patrolTarget);
        }
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        float distance = Vector3.Distance(transform.position, player.position);

        return angle < visionAngle / 2 && distance < visionRange;
    }
}
