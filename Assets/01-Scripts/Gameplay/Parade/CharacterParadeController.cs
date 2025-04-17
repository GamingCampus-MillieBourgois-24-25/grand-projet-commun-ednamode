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
                navAgent.Warp(pointA);
                characterInstance.transform.position = pointA;

                customizationCamera.enabled = false;
                paradeCamera.enabled = true;

                hasStartedParade = true;
                isMoving = true;
                currentTargetIndex = 1;
                MoveToNextPoint();
            }
            return;
        }

        if (isMoving && !navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            HandleDestinationReached();
        }

        UpdateAnimations();
    }

    private void InitializeCharacter()
    {
        if (characterInstance == null) return;

        navAgent = characterInstance.GetComponent<NavMeshAgent>();
        animator = characterInstance.GetComponent<Animator>();

        if (navAgent != null && animator != null)
        {
            isInitialized = true;
            navAgent.updateRotation = true;
        }
    }

    private void MoveToNextPoint()
    {
        if (currentTargetIndex < paradePoints.Length)
        {
            Vector3 destination = paradePoints[currentTargetIndex];
            navAgent.isStopped = false;
            navAgent.SetDestination(destination);
            isMoving = true;
            Debug.Log($"Déplacement vers le point {currentTargetIndex}: {destination}");
        }
    }

    private void HandleDestinationReached()
    {
        Debug.Log($"Point {currentTargetIndex} atteint.");

        if (currentTargetIndex == 2)
        {
            StartCoroutine(PauseAtPointC());
        }
        else if (currentTargetIndex == 3)
        {
            isFinished = true;
            isMoving = false;
            navAgent.isStopped = true;
            animator.SetTrigger("StopWalking");
            Debug.Log("Défilement terminé.");
        }
        else
        {
            currentTargetIndex++;
            MoveToNextPoint();
        }
    }

    private IEnumerator PauseAtPointC()
    {
        Debug.Log("Pause au point C.");
        isMoving = false;
        isPaused = true;
        navAgent.isStopped = true;
        animator.SetTrigger("StopWalking");

        yield return new WaitForSeconds(pauseDurationAtC);

        currentTargetIndex++;
        isPaused = false;
        isMoving = true;
        MoveToNextPoint();
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool isCurrentlyMoving = navAgent.velocity.magnitude > 0.1f && !navAgent.isStopped;
        animator.SetBool("IsWalking", isCurrentlyMoving);
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
