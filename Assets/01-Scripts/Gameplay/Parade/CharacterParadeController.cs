using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections;

public class CharacterParadeController : NetworkBehaviour
{
    [Header("Référence au personnage (assignée dynamiquement)")]
    [SerializeField] private GameObject characterInstance;

    [Header("Points de défilement")]
    public Vector3 pointA = new Vector3(-39f, 2.15f, 116f);
    public Vector3 pointB = new Vector3(-43f, 2.15f, 117.26f);
    public Vector3 pointC = new Vector3(-43f, 2.15f, 134.19f);
    public Vector3 pointD = new Vector3(-49f, 2.15f, 116.18f);

    [Header("Paramètres de défilement")]
    [SerializeField] private float pauseDurationAtC = 2f;
    [SerializeField] private float navAgentSpeed = 3.5f;

    private NavMeshAgent navAgent;
    private Animator animator;
    private Vector3[] paradePoints;
    private int currentTargetIndex = 0;
    private bool isParadeActive = false;
    private bool isPaused = false;
    private bool isFinished = false;

    public GameObject CharacterInstance
    {
        get => characterInstance;
        set
        {
            characterInstance = value;
            Debug.Log($"[CharacterParadeController] CharacterInstance assigné : {(characterInstance != null ? characterInstance.name : "null")}");
            InitializeCharacter();
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[CharacterParadeController] OnNetworkSpawn appelé, IsOwner={IsOwner}, OwnerClientId={OwnerClientId}");
        paradePoints = new Vector3[] { pointA, pointB, pointC, pointD };
        if (characterInstance != null)
        {
            Debug.Log($"[CharacterParadeController] characterInstance déjà assigné dans OnNetworkSpawn : {characterInstance.name}");
            InitializeCharacter();
        }
        else
        {
            Debug.LogWarning("[CharacterParadeController] characterInstance non assigné dans OnNetworkSpawn, tentative de récupération...");
            // Tentative de récupération du NetworkObject du joueur
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.ConnectedClients.ContainsKey(OwnerClientId))
            {
                var playerObject = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject;
                if (playerObject != null)
                {
                    characterInstance = playerObject.gameObject;
                    Debug.Log($"[CharacterParadeController] characterInstance récupéré via NetworkObject : {characterInstance.name}");
                    InitializeCharacter();
                }
            }
        }
    }

    private void InitializeCharacter()
    {
        Debug.Log("[CharacterParadeController] InitializeCharacter appelé");
        if (characterInstance == null)
        {
            Debug.LogError("[CharacterParadeController] characterInstance est null");
            return;
        }

        navAgent = characterInstance.GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("[CharacterParadeController] NavMeshAgent non trouvé sur characterInstance");
            return;
        }
        navAgent.speed = navAgentSpeed;
        navAgent.stoppingDistance = 0.1f;
        Debug.Log($"[CharacterParadeController] NavMeshAgent trouvé, speed={navAgent.speed}, stoppingDistance={navAgent.stoppingDistance}");

        animator = characterInstance.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[CharacterParadeController] Animator non trouvé sur characterInstance");
            return;
        }
        Debug.Log("[CharacterParadeController] Animator trouvé");
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
            float distanceToTarget = Vector3.Distance(characterInstance.transform.position, paradePoints[currentTargetIndex]);
            Debug.Log($"[CharacterParadeController] Déplacement vers cible {currentTargetIndex} ({paradePoints[currentTargetIndex]}), Distance={distanceToTarget:F3}, StoppingDistance={navAgent.stoppingDistance}, RemainingDistance={navAgent.remainingDistance:F3}");

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
            Debug.Log($"[CharacterParadeController] IsWalking changé à {isMoving}");
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
        Debug.Log($"[CharacterParadeController] État Animator : Clip={clipName}, NormalizedTime={stateInfo.normalizedTime:F3}, Speed={stateInfo.speed:F3}, IsWalking={currentIsWalking}");
    }

    [ClientRpc]
    public void StartParadeClientRpc()
    {
        Debug.Log($"[CharacterParadeController] StartParadeClientRpc appelé, IsOwner={IsOwner}, characterInstance={(characterInstance != null ? characterInstance.name : "null")}, navAgent={(navAgent != null ? "présent" : "null")}");
        if (!IsOwner)
        {
            Debug.Log("[CharacterParadeController] StartParadeClientRpc ignoré : non-owner");
            return;
        }

        if (characterInstance == null || navAgent == null)
        {
            Debug.LogError("[CharacterParadeController] Impossible de démarrer le défilé : characterInstance ou navAgent est null");
            return;
        }

        Debug.Log("[CharacterParadeController] Début du défilé pour ce client");
        isParadeActive = true;
        isFinished = false;
        currentTargetIndex = 0;

        navAgent.Warp(pointA);
        Debug.Log($"[CharacterParadeController] Téléportation à pointA={pointA}");
        navAgent.isStopped = false;
        SetNextDestination();
    }

    [ClientRpc]
    public void StopParadeClientRpc()
    {
        Debug.Log($"[CharacterParadeController] StopParadeClientRpc appelé, IsOwner={IsOwner}");
        if (!IsOwner)
        {
            Debug.Log("[CharacterParadeController] StopParadeClientRpc ignoré : non-owner");
            return;
        }

        Debug.Log("[CharacterParadeController] Fin du défilé pour ce client");
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
            Debug.Log("[CharacterParadeController] SetNextDestination ignoré : parade terminée ou inactive");
            return;
        }

        currentTargetIndex++;
        if (currentTargetIndex >= paradePoints.Length)
        {
            Debug.Log("[CharacterParadeController] Dernier point atteint, fin du défilé");
            isFinished = true;
            navAgent.isStopped = true;
            animator.SetBool("IsWalking", false);
            return;
        }

        Debug.Log($"[CharacterParadeController] Définition de la destination {currentTargetIndex} : {paradePoints[currentTargetIndex]}");
        navAgent.SetDestination(paradePoints[currentTargetIndex]);
    }

    private void HandleDestinationReached()
    {
        Debug.Log($"[CharacterParadeController] Destination atteinte : {paradePoints[currentTargetIndex]}, index={currentTargetIndex}");

        if (currentTargetIndex == 2) // Point C
        {
            Debug.Log("[CharacterParadeController] Point C atteint, démarrage de la pause");
            StartCoroutine(PauseAtPointC());
        }
        else if (currentTargetIndex == 3) // Point D
        {
            Debug.Log("[CharacterParadeController] Point D atteint, fin du défilé");
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
        Debug.Log($"[CharacterParadeController] Début de la pause à C, isPaused={isPaused}");
        isPaused = true;
        navAgent.isStopped = true;
        animator.SetBool("IsWalking", false);
        Debug.Log($"[CharacterParadeController] Pause à C : isPaused={isPaused}, navAgent arrêté={navAgent.isStopped}");

        yield return new WaitForSeconds(pauseDurationAtC);

        Debug.Log("[CharacterParadeController] Fin de la pause à C");
        isPaused = false;
        navAgent.isStopped = false;
        SetNextDestination();
    }

    public Vector3 GetCurrentPosition()
    {
        Vector3 position = characterInstance != null ? characterInstance.transform.position : Vector3.zero;
        Debug.Log($"[CharacterParadeController] GetCurrentPosition : {position}");
        return position;
    }

    public bool IsFinished()
    {
        Debug.Log($"[CharacterParadeController] IsFinished : {isFinished}");
        return isFinished;
    }
}