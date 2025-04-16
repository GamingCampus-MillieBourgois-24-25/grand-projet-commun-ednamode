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

    [Header("Position de t�l�portation apr�s d�lai")]
    public Vector3 teleportPositionAfterDelay = new Vector3(-40f, 2.15f, 120f); // Position o� le personnage se t�l�porte apr�s 10 secondes

    [Header("Param�tres de d�filement")]
    public float teleportDelay = 10f; // D�lai avant la t�l�portation (10 secondes)
    public float pauseDurationAtC = 5f; // Dur�e de la pause au point C (5 secondes)

    private NavMeshAgent navAgent; // R�f�rence au NavMeshAgent
    private float timer = 0f; // Timer pour le d�lai de t�l�portation
    private bool hasTeleported = false; // Indique si la t�l�portation a eu lieu
    private int currentTargetIndex = 0; // Index du point cible actuel (0 = Point A, 1 = Point B, 2 = Point C, 3 = Point D)
    private Vector3[] paradePoints; // Tableau des points de d�filement
    private bool isMoving = true; // Indique si le personnage est en mouvement
    private bool isFinished = false; // Indique si le d�filement est termin�
    private bool goingBack = false; // Indique si on est en phase de retour (apr�s le point C)
    private bool isInitialized = false; // Indique si le personnage est initialis�

    // Propri�t� publique pour assigner characterInstance dynamiquement
    public GameObject CharacterInstance
    {
        get => characterInstance;
        set
        {
            characterInstance = value;
            Debug.Log("[CharacterParadeController] characterInstance assign� : " + (characterInstance != null));
/*            InitializeCharacter(); // Appeler l'initialisation une fois que characterInstance est assign�
*/        }
    }

    void Start()
    {
        // Initialiser le tableau des points de d�filement
        paradePoints = new Vector3[] { pointA, pointB, pointC, pointD };

       /* // Si characterInstance est d�j� assign� (par exemple, via l'inspecteur), initialiser imm�diatement
        if (characterInstance != null)
        {
            InitializeCharacter();
        }*/
    }

    void Update()
    {
        if (!isInitialized || isFinished || navAgent == null) return;

        // G�rer le d�lai avant la t�l�portation
        if (!hasTeleported)
        {
            timer += Time.deltaTime;
            if (timer >= teleportDelay)
            {
                // T�l�porter le personnage � la nouvelle position
                characterInstance.transform.position = teleportPositionAfterDelay;
                navAgent.Warp(teleportPositionAfterDelay); // Utiliser Warp pour synchroniser le NavMeshAgent
                hasTeleported = true;
                Debug.Log("[CharacterParadeController] Personnage t�l�port� apr�s 10 secondes � : " + teleportPositionAfterDelay);

                // R�initialiser la destination apr�s la t�l�portation
                SetNextDestination();
            }
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

        // Positionner le personnage au point A
        characterInstance.transform.position = pointA;
        navAgent.Warp(pointA); // S'assurer que le NavMeshAgent est synchronis�
        Debug.Log("[CharacterParadeController] Personnage positionn� au point A : " + pointA);

        // Passer directement au point B
        currentTargetIndex = 1; // Point B
        SetNextDestination();

        isInitialized = true; // Marquer comme initialis�
    }

    private void SetNextDestination()
    {
        if (isFinished) return;

        // D�finir la destination suivante
        navAgent.SetDestination(paradePoints[currentTargetIndex]);
        Debug.Log("[CharacterParadeController] Destination d�finie : " + paradePoints[currentTargetIndex]);

        // Pour le multijoueur : ici, vous pourriez synchroniser la destination avec les autres clients
        // Exemple : Synchroniser via Unity Netcode ou Photon
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

            // Pour le multijoueur : ici, vous pourriez signaler aux autres clients que ce personnage a termin�
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
        navAgent.isStopped = true; // Arr�ter le NavMeshAgent pendant la pause
        Debug.Log("[CharacterParadeController] Pause de 5 secondes au point C : " + pointC);
        yield return new WaitForSeconds(pauseDurationAtC);
        goingBack = true; // Passer en mode retour apr�s la pause au point C
        currentTargetIndex = 1; // Repartir vers le point B (index 1)
        Debug.Log("[CharacterParadeController] Reprise vers le point B : " + pointB);
        navAgent.isStopped = false; // Reprendre le mouvement
        isMoving = true;
        SetNextDestination();

        // Pour le multijoueur : ici, vous pourriez synchroniser l'�tat de la pause avec les autres clients
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