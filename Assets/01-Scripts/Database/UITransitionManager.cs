using System;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.UI;


public class UITransitionManager : MonoBehaviour
{
    public static UITransitionManager Instance;

    [Header("Starting Screen")]
    [SerializeField] private GameObject startingScreenPanel;
    [SerializeField] private RectTransform startingScreenTransform;
    [SerializeField] private float startingTransitionDuration = 1f;
    [SerializeField] private float delayBeforeHide = 1.5f;

    [Header("Scene Transition Panel")]
    [SerializeField] private GameObject transitionCoverPanel;
    [SerializeField] private RectTransform transitionPanelTransform;
    [SerializeField] private float sceneTransitionDuration = 0.8f;

    [Header("Panel Transition")]
    [SerializeField] private float panelTransitionDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Ensure the starting screen is hidden at the start
        startingScreenPanel.SetActive(false);
        transitionCoverPanel.SetActive(false);
        StartGame(); // Start the starting screen transition
    }
    public async void StartGame()
    {
        await UITransitionManager.Instance.PlayStartingScreenAsync();
        Debug.Log("Transition de l'écran de démarrage terminée.");
        // Code suivant après les transitions
    }

    #region Starting Screen
    public async Task PlayStartingScreenAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        startingScreenPanel.SetActive(true);
        startingScreenTransform.anchoredPosition = new Vector2(0, -Screen.height);

        startingScreenTransform.DOAnchorPosY(0, startingTransitionDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                Debug.Log("Écran de démarrage affiché. En attente d'un clic du joueur...");
            });

        // Attendre un clic du joueur
        startingScreenPanel.GetComponent<Button>().onClick.AddListener(() =>
        {
            startingScreenPanel.SetActive(false);
            tcs.SetResult(true); // Transition terminée après le clic
        });

        await tcs.Task; // Attendre la fin de la transition
    }
    #endregion

    #region Scene Transition
    public async Task PlaySceneTransitionAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        transitionCoverPanel.SetActive(true);
        transitionPanelTransform.anchoredPosition = new Vector2(0, Screen.height);

        transitionPanelTransform.DOAnchorPosY(0, sceneTransitionDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => tcs.SetResult(true)); // Transition terminée

        await tcs.Task; // Attendre la fin de la transition

        SceneManager.Instance.LoadLobby(); // Charger la scène de jeu
    }
    #endregion

    #region Panel Juiciness
    public async Task AnimatePanelInAsync(GameObject panel)
    {
        var tcs = new TaskCompletionSource<bool>();

        panel.SetActive(true);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.localScale = Vector3.zero;

        rt.DOScale(1f, panelTransitionDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => tcs.SetResult(true)); // Animation terminée

        await tcs.Task; // Attendre la fin de l'animation
    }

    public async Task AnimatePanelOutAsync(GameObject panel)
    {
        var tcs = new TaskCompletionSource<bool>();

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.DOScale(0f, panelTransitionDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                panel.SetActive(false);
                tcs.SetResult(true); // Animation terminée
            });

        await tcs.Task; // Attendre la fin de l'animation
    }

    public async Task ReplacePanelAsync(GameObject fromPanel, GameObject toPanel)
    {
        await AnimatePanelOutAsync(fromPanel); // Attendre que le panel sortant soit caché
        await AnimatePanelInAsync(toPanel);   // Attendre que le panel entrant soit affiché
    }
    #endregion
}
