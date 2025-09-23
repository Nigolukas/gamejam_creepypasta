using UnityEngine;
using UnityEngine.AI;

public class DinosaurAI : MonoBehaviour
{
    [Header("Detección")]
    public Transform player;               // Se llenará automáticamente en Start
    public string playerTag = "Player";    // Tag del jugador
    public float visionRange = 10f;
    public float visionAngle = 60f;

    [Header("Patrulla")]
    public float patrolRadius = 15f;
    public float patrolDelay = 3f;

    private NavMeshAgent agent;
    private Vector3 patrolPoint;
    private float patrolTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Si no se asignó manualmente, buscar el objeto con tag "Player"
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning(" No se encontró ningún objeto con tag 'Player'. Asegúrate de asignarlo en la escena.");
        }

        SetRandomPatrolPoint();
    }
    void Update()
    {
        if (CanSeePlayer())
        {
            agent.SetDestination(player.position);
        }
        else
        {
            Patrol();
        }
    }

    bool CanSeePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < visionRange)
        {
            float angle = Vector3.Angle(transform.forward, direction);
            if (angle < visionAngle / 2f)
            {
                // Checar si hay obstáculos en medio
                if (Physics.Raycast(transform.position, direction, out RaycastHit hit, visionRange))
                {
                    if (hit.transform == player)
                        return true;
                }
            }
        }
        return false;
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolDelay)
            {
                SetRandomPatrolPoint();
                patrolTimer = 0f;
            }
        }
    }

    void SetRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;   
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
        {
            patrolPoint = hit.position;
            agent.SetDestination(patrolPoint);
        }
    }
}
