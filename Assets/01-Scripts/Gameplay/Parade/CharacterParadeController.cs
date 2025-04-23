using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections;

public class CharacterParadeController : NetworkBehaviour
{
    [Header("Points de d�filement")]
    public Vector3 pointA = new Vector3(-39f, 2.15f, 116f);
    public Vector3 pointB = new Vector3(-43f, 2.15f, 117.26f);
    public Vector3 pointC = new Vector3(-43f, 2.15f, 134.19f);
    public Vector3 pointD = new Vector3(-49f, 2.15f, 116.18f);

    [Header("Param�tres de d�filement")]
    [SerializeField] private float pauseDurationAtC = 2f;
    [SerializeField] private float navAgentSpeed = 3.5f;

    private NavMeshAgent navAgent;
    private Animator animator;
    private Vector3[] paradePoints;
    private int currentTargetIndex = 0;
    private bool isParadeActive = false;
    private bool isPaused = false;
    private bool isFinished = false;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[CharacterParadeController] OnNetworkSpawn appel�, IsOwner={IsOwner}, OwnerClientId={OwnerClientId}");
        paradePoints = new Vector3[] { pointA, pointB, pointC, pointD };
        InitializeCharacter();
    }

    private void InitializeCharacter()
    {
        Debug.Log("[CharacterParadeController] InitializeCharacter appel�");

        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("[CharacterParadeController] NavMeshAgent non trouv� sur ce GameObject");
            return;
        }
        navAgent.speed = navAgentSpeed;
        navAgent.stoppingDistance = 0.1f;
        Debug.Log($"[CharacterParadeController] NavMeshAgent trouv�, speed={navAgent.speed}, stoppingDistance={navAgent.stoppingDistance}");

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[CharacterParadeController] Animator non trouv� sur ce GameObject");
            return;
        }
        Debug.Log("[CharacterParadeController] Animator trouv�");
    }

    private void Update()
    {
        if (!IsOwner || !isParadeActive || isFinished || navAgent == null || animator == null)
        {
            return;
        }

        UpdateAnimations();

        if (!isPaused && !navAgent.pathPending)
        {
            float distanceToTarget = Vector3.Distance(transform.position, paradePoints[currentTargetIndex]);
            Debug.Log($"[CharacterParadeController] D�placement vers cible {currentTargetIndex} ({paradePoints[currentTargetIndex]}), Distance={distanceToTarget:F3}, StoppingDistance={navAgent.stoppingDistance}, RemainingDistance={navAgent.remainingDistance:F3}");

            if (distanceToTarget <= navAgent.stoppingDistance + 0.3f)
            {
                Debug.Log($"[CharacterParadeController] Cible atteinte (distance={distanceToTarget:F3} <= {navAgent.stoppingDistance + 0.3f})");
                HandleDestinationReached();
            }
        }
    }

    private void UpdateAnimations()
    {
        bool isMoving = navAgent.velocity.magnitude > 0.1f && !isPaused;
        bool currentIsWalking = animator.GetBool("IsWalking");
        if (currentIsWalking != isMoving)
        {
            animator.SetBool("IsWalking", isMoving);
            Debug.Log($"[CharacterParadeController] IsWalking chang� � {isMoving}");
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        string clipName = "Inconnu";
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (stateInfo.IsName(clip.name))
            {
                clipName = clip.name;
                break;
            }
        }
        Debug.Log($"[CharacterParadeController] �tat Animator : Clip={clipName}, NormalizedTime={stateInfo.normalizedTime:F3}, Speed={stateInfo.speed:F3}, IsWalking={currentIsWalking}");
    }

    [ClientRpc]
    public void StartParadeClientRpc()
    {
        Debug.Log($"[CharacterParadeController] StartParadeClientRpc appel�, IsOwner={IsOwner}, GameObject={gameObject.name}, navAgent={(navAgent != null ? "pr�sent" : "null")}");
        if (!IsOwner)
        {
            Debug.Log("[CharacterParadeController] StartParadeClientRpc ignor� : non-owner");
            return;
        }

        if (navAgent == null || animator == null)
        {
            Debug.LogError("[CharacterParadeController] Impossible de d�marrer le d�fil� : navAgent ou animator est null");
            return;
        }

        Debug.Log("[CharacterParadeController] D�but du d�fil� pour ce client");
        isParadeActive = true;
        isFinished = false;
        currentTargetIndex = 0;

        // Forcer la t�l�portation
        transform.position = pointA;
        navAgent.Warp(pointA);
        Debug.Log($"[CharacterParadeController] T�l�portation � pointA={pointA}, position actuelle={transform.position}");
        navAgent.isStopped = false;
        SetNextDestination();
    }

    [ClientRpc]
    public void StopParadeClientRpc()
    {
        Debug.Log($"[CharacterParadeController] StopParadeClientRpc appel�, IsOwner={IsOwner}");
        if (!IsOwner)
        {
            Debug.Log("[CharacterParadeController] StopParadeClientRpc ignor� : non-owner");
            return;
        }

        Debug.Log("[CharacterParadeController] Fin du d�fil� pour ce client");
        isParadeActive = false;
        isFinished = true;
        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
    }

    private void SetNextDestination()
    {
        if (isFinished || !isParadeActive)
        {
            Debug.Log("[CharacterParadeController] SetNextDestination ignor� : parade termin�e ou inactive");
            return;
        }

        currentTargetIndex++;
        if (currentTargetIndex >= paradePoints.Length)
        {
            Debug.Log("[CharacterParadeController] Dernier point atteint, fin du d�fil�");
            isFinished = true;
            navAgent.isStopped = true;
            animator.SetBool("IsWalking", false);
            return;
        }

        Debug.Log($"[CharacterParadeController] D�finition de la destination {currentTargetIndex} : {paradePoints[currentTargetIndex]}");
        navAgent.SetDestination(paradePoints[currentTargetIndex]);
    }

    private void HandleDestinationReached()
    {
        Debug.Log($"[CharacterParadeController] Destination atteinte : {paradePoints[currentTargetIndex]}, index={currentTargetIndex}");

        if (currentTargetIndex == 2) // Point C
        {
            Debug.Log("[CharacterParadeController] Point C atteint, d�marrage de la pause");
            StartCoroutine(PauseAtPointC());
        }
        else if (currentTargetIndex == 3) // Point D
        {
            Debug.Log("[CharacterParadeController] Point D atteint, fin du d�fil�");
            isFinished = true;
            navAgent.isStopped = true;
            animator.SetBool("IsWalking", false);
        }
        else
        {
            SetNextDestination();
        }
    }

    private IEnumerator PauseAtPointC()
    {
        Debug.Log($"[CharacterParadeController] D�but de la pause � C, isPaused={isPaused}");
        isPaused = true;
        navAgent.isStopped = true;
        animator.SetBool("IsWalking", false);
        Debug.Log($"[CharacterParadeController] Pause � C : isPaused={isPaused}, navAgent arr�t�={navAgent.isStopped}");

        yield return new WaitForSeconds(pauseDurationAtC);

        Debug.Log("[CharacterParadeController] Fin de la pause � C");
        isPaused = false;
        navAgent.isStopped = false;
        SetNextDestination();
    }

    public Vector3 GetCurrentPosition()
    {
        Vector3 position = transform.position;
        Debug.Log($"[CharacterParadeController] GetCurrentPosition : {position}");
        return position;
    }

    public bool IsFinished()
    {
        Debug.Log($"[CharacterParadeController] IsFinished : {isFinished}");
        return isFinished;
    }
}