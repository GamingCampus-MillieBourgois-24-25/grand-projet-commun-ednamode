using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CharacterParadeController : MonoBehaviour
{
    [Header("Référence au personnage (assignée dynamiquement)")]
    [SerializeField] private GameObject characterInstance;

    [Header("Points de défilement")]
    public Vector3 pointA = new Vector3(-39f, 2.15f, 116f);
    public Vector3 pointB = new Vector3(-43f, 2.15f, 117.26f);
    public Vector3 pointC = new Vector3(-43f, 2.15f, 134.19f);
    public Vector3 pointD = new Vector3(-49f, 2.15f, 116.18f);

    [Header("Paramètres de défilement")]
    public float customizationDelay = 10f;
    public float pauseDurationAtC = 5f;

    [Header("Caméras")]
    [SerializeField] private Camera customizationCamera;
    [SerializeField] private Camera paradeCamera;

    private NavMeshAgent navAgent;
    private Animator animator;
    private float customizationTimer = 0f;
    private bool hasStartedParade = false;
    private int currentTargetIndex = 0;
    private Vector3[] paradePoints;
    private bool isMoving = false;
    private bool isFinished = false;
    private bool isInitialized = false;
    private bool isPaused = false;
    private string lastWalkDirection = "";
    private bool wasMovingLastFrame = false;

    public GameObject CharacterInstance
    {
        get => characterInstance;
        set
        {
            characterInstance = value;
            InitializeCharacter();
        }
    }

    void Start()
    {
        paradePoints = new Vector3[] { pointA, pointB, pointC, pointD };

        if (customizationCamera != null && paradeCamera != null)
        {
            customizationCamera.enabled = true;
            paradeCamera.enabled = false;
        }

        if (characterInstance != null)
        {
            InitializeCharacter();
        }
    }

    void Update()
    {
        if (!isInitialized || isFinished || navAgent == null) return;

        if (!hasStartedParade)
        {
            customizationTimer += Time.deltaTime;
            if (customizationTimer >= customizationDelay)
            {
                characterInstance.transform.position = pointA;
                navAgent.Warp(pointA);

                if (customizationCamera != null && paradeCamera != null)
                {
                    customizationCamera.enabled = false;
                    paradeCamera.enabled = true;
                }

                hasStartedParade = true;
                isMoving = true;
                currentTargetIndex = 1;
                SetNextDestination();
            }
            return;
        }

        UpdateAnimations();

        if (isMoving && !navAgent.pathPending)
        {
            float distanceToTarget = Vector3.Distance(characterInstance.transform.position, paradePoints[currentTargetIndex]);
            Debug.Log($"[CharacterParadeController] Distance to target: {distanceToTarget}, Stopping Distance: {navAgent.stoppingDistance}, Remaining Distance: {navAgent.remainingDistance}");

            if (distanceToTarget <= navAgent.stoppingDistance + 0.3f)
            {
                HandleDestinationReached();
            }
        }

        wasMovingLastFrame = isMoving;
    }

    private void InitializeCharacter()
    {
        if (characterInstance == null)
        {
            return;
        }

        navAgent = characterInstance.GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            return;
        }

        animator = characterInstance.GetComponent<Animator>();
        if (animator == null)
        {
            return;
        }

        navAgent.stoppingDistance = 0.1f;
        isInitialized = true;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        if (isMoving)
        {
            string newWalkDirection = "WalkForward";
            animator.SetTrigger(newWalkDirection);
            lastWalkDirection = newWalkDirection;
        }

        if (wasMovingLastFrame && !isMoving)
        {
            animator.SetTrigger("StopWalking");
            lastWalkDirection = "";
        }
    }

    private void SetNextDestination()
    {
        if (isFinished) return;

        navAgent.SetDestination(paradePoints[currentTargetIndex]);
        Debug.Log($"[CharacterParadeController] Destination définie : {paradePoints[currentTargetIndex]}");
    }

    private void HandleDestinationReached()
    {
        Debug.Log($"[CharacterParadeController] Destination atteinte : {paradePoints[currentTargetIndex]}");

        if (currentTargetIndex == 2)
        {
            StartCoroutine(PauseAtPointC());
        }
        else if (currentTargetIndex == 3)
        {
            isFinished = true;
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }
        else
        {
            currentTargetIndex++;
            SetNextDestination();
        }
    }

    private IEnumerator PauseAtPointC()
    {
        isMoving = false;
        isPaused = true;
        navAgent.isStopped = true;

        animator.SetTrigger("StopWalking");

        yield return new WaitForSeconds(pauseDurationAtC);

        currentTargetIndex = 3;
        navAgent.isStopped = false;
        isMoving = true;
        isPaused = false;
        SetNextDestination();
    }

    public Vector3 GetCurrentPosition()
    {
        return characterInstance != null ? characterInstance.transform.position : Vector3.zero;
    }

    public bool IsFinished()
    {
        return isFinished;
    }
}