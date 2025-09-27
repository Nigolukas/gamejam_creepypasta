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

    [Header("Catch Settings")]
    public float catchDistance = 1.5f; // Distancia para atraparlo
    private bool hasCaughtPlayer = false; // Para que no se repita el screamer

    private Vector3 patrolTarget;
    private float patrolTimer;
    private bool isChasing = false;
    public Camera mainCam;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.stoppingDistance = catchDistance-10;
        // Detecta automáticamente al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        SetNewPatrolPoint();
    }


    void Update()
    {
        if (player != null && !hasCaughtPlayer)
        {
            if (CanSeePlayer())
            {
                if (!isChasing) // 👈 evita spamear corutina
                    StartCoroutine(RoarThenChase());
            }
            else
            {
                isChasing = false;
                Patrol();
            }

            // Si está dentro de la distancia de atrapada
            if (Vector3.Distance(transform.position, player.position) <= catchDistance)
            {
                StartCoroutine(DoScreamer());
            }
        }

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

    IEnumerator DoScreamer()
    {
        if (hasCaughtPlayer) yield break;

        hasCaughtPlayer = true;

        agent.isStopped = true;
        agent.ResetPath();
        agent.updatePosition = false;
        agent.updateRotation = false;

        // Girar el dinosaurio hacia el jugador
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        // Bloquear cámara y movimiento del jugador
        FirstPersonControllerNew controller = player.GetComponent<FirstPersonControllerNew>();
        if (controller != null)
        {
            controller.LockCamera();
            controller.LockMovement(); // <- Bloquea WASD
        }

        // Animar la cámara suavemente hacia el dino
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 targetPos = transform.position + Vector3.up * 7f;
            Quaternion startRot = mainCam.transform.rotation;
            Quaternion targetRot = Quaternion.LookRotation(targetPos - mainCam.transform.position);
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                mainCam.transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
                yield return null;
            }

            mainCam.transform.rotation = targetRot;
        }

        animator.SetTrigger("Roar");

        yield return new WaitForSeconds(2f);

        Debug.Log("GAME OVER: el dinosaurio atrapó al jugador.");

        // Opcional: desbloquear movimiento y cámara si quieres continuar el juego
        // controller?.UnlockMovement();
        // controller?.UnlockCamera();
    }




}
