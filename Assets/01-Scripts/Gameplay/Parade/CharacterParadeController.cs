using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CharacterParadeController : MonoBehaviour
{
    [Header("R�f�rence au personnage (assign�e dynamiquement)")]
    [SerializeField] private GameObject characterInstance; // R�f�rence � _characterInstance de CustomizableCharacterUI

    [Header("Points de d�filement")]
    public Vector3 pointA = new Vector3(-39f, 2.15f, 116f); // Point de d�part (A)
    public Vector3 pointB = new Vector3(-43f, 2.15f, 117.26f); // Point B
    public Vector3 pointC = new Vector3(-43f, 2.15f, 134.19f); // Point C
    public Vector3 pointD = new Vector3(-49f, 2.15f, 116.18f); // Point D (point final)

    [Header("Param�tres de d�filement")]
    public float customizationDelay = 10f; // D�lai pour la personnalisation (10 secondes)
    public float pauseDurationAtC = 5f; // Dur�e de la pause au point C (5 secondes)

    [Header("Cam�ras")]
    [SerializeField] private Camera customizationCamera; // Cam�ra utilis�e pendant la personnalisation
    [SerializeField] private Camera paradeCamera; // Cam�ra utilis�e pendant le d�filement

    private NavMeshAgent navAgent; // R�f�rence au NavMeshAgent
    private float customizationTimer = 0f; // Timer pour le d�lai de personnalisation
    private bool hasStartedParade = false; // Indique si le d�filement a commenc�
    private int currentTargetIndex = 0; // Index du point cible actuel (0 = Point A, 1 = Point B, 2 = Point C, 3 = Point D)
    private Vector3[] paradePoints; // Tableau des points de d�filement
    private bool isMoving = false; // Indique si le personnage est en mouvement
    private bool isFinished = false; // Indique si le d�filement est termin�
    private bool goingBack = false; // Indique si on est en phase de retour (apr�s le point C)
    private bool isInitialized = false; // Indique si le personnage est initialis�
    private bool isPaused = false; // Indique si le personnage est en pause (par exemple, au point C)

    // Propri�t� publique pour assigner characterInstance dynamiquement
    public GameObject CharacterInstance
    {
        get => characterInstance;
        set
        {
            characterInstance = value;
            Debug.Log("[CharacterParadeController] characterInstance assign� : " + (characterInstance != null));
            InitializeCharacter(); // Appeler l'initialisation une fois que characterInstance est assign�
        }
    }

    void Start()
    {
        // Initialiser le tableau des points de d�filement
        paradePoints = new Vector3[] { pointA, pointB, pointC, pointD };

        // V�rifier et configurer les cam�ras au d�marrage
        if (customizationCamera != null && paradeCamera != null)
        {
            customizationCamera.enabled = true; // Activer la cam�ra de personnalisation au d�but
            paradeCamera.enabled = false; // D�sactiver la cam�ra de d�filement
            Debug.Log("[CharacterParadeController] Cam�ra de personnalisation activ�e, cam�ra de d�filement d�sactiv�e.");
        }
        else
        {
            Debug.LogError("[CharacterParadeController] Une ou les deux cam�ras ne sont pas assign�es !");
        }

        // Si characterInstance est d�j� assign� (par exemple, via l'inspecteur), initialiser imm�diatement
        if (characterInstance != null)
        {
            InitializeCharacter();
        }
    }

    void Update()
    {
        if (!isInitialized || isFinished || navAgent == null) return;

        // G�rer le d�lai de personnalisation avant de commencer le d�filement
        if (!hasStartedParade)
        {
            customizationTimer += Time.deltaTime;
            if (customizationTimer >= customizationDelay)
            {
                // T�l�porter le personnage au point A apr�s 10 secondes de personnalisation
                characterInstance.transform.position = pointA;
                navAgent.Warp(pointA);
                Debug.Log("[CharacterParadeController] Personnage t�l�port� au point A apr�s 10 secondes : " + pointA);

                // Changer de cam�ra
                if (customizationCamera != null && paradeCamera != null)
                {
                    customizationCamera.enabled = false; // D�sactiver la cam�ra de personnalisation
                    paradeCamera.enabled = true; // Activer la cam�ra de d�filement
                    Debug.Log("[CharacterParadeController] Changement de cam�ra : cam�ra de d�filement activ�e.");
                }

                // Commencer le d�filement imm�diatement
                hasStartedParade = true;
                isMoving = true;
                currentTargetIndex = 1; // Passer directement au point B
                SetNextDestination();
            }
            return; // Ne pas ex�cuter le reste de Update pendant la personnalisation
        }

        // V�rifier si le personnage a atteint sa destination
        if (isMoving && !navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            HandleDestinationReached();
        }
    }

    private void InitializeCharacter()
    {
        // V�rifier si characterInstance est assign� et r�cup�rer le NavMeshAgent
        if (characterInstance == null)
        {
            Debug.LogError("[CharacterParadeController] characterInstance n'est pas assign� !");
            return;
        }

        navAgent = characterInstance.GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("[CharacterParadeController] NavMeshAgent manquant sur le personnage !");
            return;
        }

        // Ne pas t�l�porter ici, laisser le personnage � sa position initiale (0,0,0) pour la personnalisation
        Debug.Log("[CharacterParadeController] Personnage initialis�, en attente de personnalisation � : " + characterInstance.transform.position);

        isInitialized = true; // Marquer comme initialis�
    }

    private void SetNextDestination()
    {
        if (isFinished) return;

        // D�finir la destination suivante
        navAgent.SetDestination(paradePoints[currentTargetIndex]);
        Debug.Log("[CharacterParadeController] Destination d�finie : " + paradePoints[currentTargetIndex]);
    }

    private void HandleDestinationReached()
    {
        // Si on est au point C (index 2), faire une pause
        if (currentTargetIndex == 2) // Point C
        {
            StartCoroutine(PauseAtPointC());
        }
        // Si on est au point D (index 3), arr�ter le d�filement
        else if (currentTargetIndex == 3) // Point D
        {
            isFinished = true;
            navAgent.isStopped = true; // Arr�ter compl�tement le NavMeshAgent
            navAgent.enabled = false; // D�sactiver pour �conomiser des ressources
            Debug.Log("[CharacterParadeController] D�filement termin� au point D : " + pointD);
        }
        else
        {
            // D�terminer la direction du parcours
            if (!goingBack)
            {
                // Phase aller : A -> B -> C
                currentTargetIndex++; // Avancer au point suivant
                Debug.Log("[CharacterParadeController] Prochain point : " + paradePoints[currentTargetIndex]);
            }
            else
            {
                // Phase retour : C -> B -> D
                currentTargetIndex = (currentTargetIndex == 1) ? 3 : 1; // Apr�s C, aller � B, puis apr�s B, aller � D
                Debug.Log("[CharacterParadeController] Prochain point : " + paradePoints[currentTargetIndex]);
            }
            SetNextDestination();
        }
    }

    private IEnumerator PauseAtPointC()
    {
        isMoving = false;
        isPaused = true; // Mettre en pause
        navAgent.isStopped = true; // Arr�ter le NavMeshAgent pendant la pause
        Debug.Log("[CharacterParadeController] Pause de 5 secondes au point C : " + pointC);
        yield return new WaitForSeconds(pauseDurationAtC);
        goingBack = true; // Passer en mode retour apr�s la pause au point C
        currentTargetIndex = 1; // Repartir vers le point B (index 1)
        Debug.Log("[CharacterParadeController] Reprise vers le point B : " + pointB);
        navAgent.isStopped = false; // Reprendre le mouvement
        isMoving = true;
        isPaused = false;
        SetNextDestination();
    }

    // M�thode pour obtenir la position actuelle (utile pour le multijoueur)
    public Vector3 GetCurrentPosition()
    {
        return characterInstance != null ? characterInstance.transform.position : Vector3.zero;
    }

    // M�thode pour obtenir l'�tat actuel (utile pour le multijoueur)
    public bool IsFinished()
    {
        return isFinished;
    }
}