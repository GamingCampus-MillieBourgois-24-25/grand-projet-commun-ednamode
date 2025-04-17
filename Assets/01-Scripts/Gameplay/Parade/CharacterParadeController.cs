using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CharacterParadeController : MonoBehaviour
{
    [Header("Référence au personnage (assignée dynamiquement)")]
    [SerializeField] private GameObject characterInstance; // Référence à _characterInstance de CustomizableCharacterUI

    [Header("Points de défilement")]
    public Vector3 pointA = new Vector3(-39f, 2.15f, 116f); // Point de départ (A)
    public Vector3 pointB = new Vector3(-43f, 2.15f, 117.26f); // Point B
    public Vector3 pointC = new Vector3(-43f, 2.15f, 134.19f); // Point C
    public Vector3 pointD = new Vector3(-49f, 2.15f, 116.18f); // Point D (point final)

    [Header("Paramètres de défilement")]
    public float customizationDelay = 10f; // Délai pour la personnalisation (10 secondes)
    public float pauseDurationAtC = 5f; // Durée de la pause au point C (5 secondes)

    [Header("Caméras")]
    [SerializeField] private Camera customizationCamera; // Caméra utilisée pendant la personnalisation
    [SerializeField] private Camera paradeCamera; // Caméra utilisée pendant le défilement

    private NavMeshAgent navAgent; // Référence au NavMeshAgent
    private float customizationTimer = 0f; // Timer pour le délai de personnalisation
    private bool hasStartedParade = false; // Indique si le défilement a commencé
    private int currentTargetIndex = 0; // Index du point cible actuel (0 = Point A, 1 = Point B, 2 = Point C, 3 = Point D)
    private Vector3[] paradePoints; // Tableau des points de défilement
    private bool isMoving = false; // Indique si le personnage est en mouvement
    private bool isFinished = false; // Indique si le défilement est terminé
    private bool goingBack = false; // Indique si on est en phase de retour (après le point C)
    private bool isInitialized = false; // Indique si le personnage est initialisé
    private bool isPaused = false; // Indique si le personnage est en pause (par exemple, au point C)

    // Propriété publique pour assigner characterInstance dynamiquement
    public GameObject CharacterInstance
    {
        get => characterInstance;
        set
        {
            characterInstance = value;
            Debug.Log("[CharacterParadeController] characterInstance assigné : " + (characterInstance != null));
            InitializeCharacter(); // Appeler l'initialisation une fois que characterInstance est assigné
        }
    }

    void Start()
    {
        // Initialiser le tableau des points de défilement
        paradePoints = new Vector3[] { pointA, pointB, pointC, pointD };

        // Vérifier et configurer les caméras au démarrage
        if (customizationCamera != null && paradeCamera != null)
        {
            customizationCamera.enabled = true; // Activer la caméra de personnalisation au début
            paradeCamera.enabled = false; // Désactiver la caméra de défilement
            Debug.Log("[CharacterParadeController] Caméra de personnalisation activée, caméra de défilement désactivée.");
        }
        else
        {
            Debug.LogError("[CharacterParadeController] Une ou les deux caméras ne sont pas assignées !");
        }

        // Si characterInstance est déjà assigné (par exemple, via l'inspecteur), initialiser immédiatement
        if (characterInstance != null)
        {
            InitializeCharacter();
        }
    }

    void Update()
    {
        if (!isInitialized || isFinished || navAgent == null) return;

        // Gérer le délai de personnalisation avant de commencer le défilement
        if (!hasStartedParade)
        {
            customizationTimer += Time.deltaTime;
            if (customizationTimer >= customizationDelay)
            {
                // Téléporter le personnage au point A après 10 secondes de personnalisation
                characterInstance.transform.position = pointA;
                navAgent.Warp(pointA);
                Debug.Log("[CharacterParadeController] Personnage téléporté au point A après 10 secondes : " + pointA);

                // Changer de caméra
                if (customizationCamera != null && paradeCamera != null)
                {
                    customizationCamera.enabled = false; // Désactiver la caméra de personnalisation
                    paradeCamera.enabled = true; // Activer la caméra de défilement
                    Debug.Log("[CharacterParadeController] Changement de caméra : caméra de défilement activée.");
                }

                // Commencer le défilement immédiatement
                hasStartedParade = true;
                isMoving = true;
                currentTargetIndex = 1; // Passer directement au point B
                SetNextDestination();
            }
            return; // Ne pas exécuter le reste de Update pendant la personnalisation
        }

        // Vérifier si le personnage a atteint sa destination
        if (isMoving && !navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            HandleDestinationReached();
        }
    }

    private void InitializeCharacter()
    {
        // Vérifier si characterInstance est assigné et récupérer le NavMeshAgent
        if (characterInstance == null)
        {
            Debug.LogError("[CharacterParadeController] characterInstance n'est pas assigné !");
            return;
        }

        navAgent = characterInstance.GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("[CharacterParadeController] NavMeshAgent manquant sur le personnage !");
            return;
        }

        // Ne pas téléporter ici, laisser le personnage à sa position initiale (0,0,0) pour la personnalisation
        Debug.Log("[CharacterParadeController] Personnage initialisé, en attente de personnalisation à : " + characterInstance.transform.position);

        isInitialized = true; // Marquer comme initialisé
    }

    private void SetNextDestination()
    {
        if (isFinished) return;

        // Définir la destination suivante
        navAgent.SetDestination(paradePoints[currentTargetIndex]);
        Debug.Log("[CharacterParadeController] Destination définie : " + paradePoints[currentTargetIndex]);
    }

    private void HandleDestinationReached()
    {
        // Si on est au point C (index 2), faire une pause
        if (currentTargetIndex == 2) // Point C
        {
            StartCoroutine(PauseAtPointC());
        }
        // Si on est au point D (index 3), arrêter le défilement
        else if (currentTargetIndex == 3) // Point D
        {
            isFinished = true;
            navAgent.isStopped = true; // Arrêter complètement le NavMeshAgent
            navAgent.enabled = false; // Désactiver pour économiser des ressources
            Debug.Log("[CharacterParadeController] Défilement terminé au point D : " + pointD);
        }
        else
        {
            // Déterminer la direction du parcours
            if (!goingBack)
            {
                // Phase aller : A -> B -> C
                currentTargetIndex++; // Avancer au point suivant
                Debug.Log("[CharacterParadeController] Prochain point : " + paradePoints[currentTargetIndex]);
            }
            else
            {
                // Phase retour : C -> B -> D
                currentTargetIndex = (currentTargetIndex == 1) ? 3 : 1; // Après C, aller à B, puis après B, aller à D
                Debug.Log("[CharacterParadeController] Prochain point : " + paradePoints[currentTargetIndex]);
            }
            SetNextDestination();
        }
    }

    private IEnumerator PauseAtPointC()
    {
        isMoving = false;
        isPaused = true; // Mettre en pause
        navAgent.isStopped = true; // Arrêter le NavMeshAgent pendant la pause
        Debug.Log("[CharacterParadeController] Pause de 5 secondes au point C : " + pointC);
        yield return new WaitForSeconds(pauseDurationAtC);
        goingBack = true; // Passer en mode retour après la pause au point C
        currentTargetIndex = 1; // Repartir vers le point B (index 1)
        Debug.Log("[CharacterParadeController] Reprise vers le point B : " + pointB);
        navAgent.isStopped = false; // Reprendre le mouvement
        isMoving = true;
        isPaused = false;
        SetNextDestination();
    }

    // Méthode pour obtenir la position actuelle (utile pour le multijoueur)
    public Vector3 GetCurrentPosition()
    {
        return characterInstance != null ? characterInstance.transform.position : Vector3.zero;
    }

    // Méthode pour obtenir l'état actuel (utile pour le multijoueur)
    public bool IsFinished()
    {
        return isFinished;
    }
}