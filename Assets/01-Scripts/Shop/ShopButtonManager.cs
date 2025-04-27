using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Nécessaire pour SceneManager

public class ShopButtonManager : MonoBehaviour
{
    [Header("?? Bouton Boutique")]
    [Tooltip("Bouton qui lance la scène Melvin_Shop")]
    [SerializeField] private Button shopButton;

    [Header("?? Transition (Optionnel)")]
    [Tooltip("Panneau pour l'animation de transition (ex: fondu noir)")]
    [SerializeField] private RectTransform transitionPanel;
    [Tooltip("Position initiale du panneau (ex: hors écran)")]
    [SerializeField] private Vector2 transitionStartPosition = new(-1850, 1450);
    [Tooltip("Position finale du panneau (ex: centre écran)")]
    [SerializeField] private Vector2 transitionEndPosition = Vector2.zero;
    [Tooltip("Durée de l'animation en secondes")]
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("?? Son (Optionnel)")]
    [Tooltip("Son joué lors du clic")]
    [SerializeField] private AudioClip clickSound;
    [Tooltip("Source audio pour le son")]
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        // Configurer le bouton
        if (shopButton != null)
        {
            shopButton.onClick.RemoveAllListeners();
            shopButton.onClick.AddListener(OnShopButtonClicked);

        }
        else
        {
            Debug.LogWarning("[ShopButtonManager] ShopButton NON assigné dans l'Inspector !");
        }

        // Désactiver le panneau de transition
        if (transitionPanel != null)
        {
            transitionPanel.gameObject.SetActive(false);
        }
    }

    private void OnShopButtonClicked()
    {
        // Jouer le son
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }



        // Lancer transition ou chargement direct
        if (transitionPanel != null)
        {
            StartCoroutine(PlayTransitionAndLoadScene());
        }
        else
        {
            StartCoroutine(LoadShopSceneAsync());
        }
    }

    private IEnumerator PlayTransitionAndLoadScene()
    {
        // Activer le panneau
        transitionPanel.gameObject.SetActive(true);
        transitionPanel.anchoredPosition = transitionStartPosition;

        // Animation
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / transitionDuration);
            transitionPanel.anchoredPosition = Vector2.Lerp(transitionStartPosition, transitionEndPosition, progress);
            yield return null;
        }
        transitionPanel.anchoredPosition = transitionEndPosition;

        // Charger la scène
        yield return StartCoroutine(LoadShopSceneAsync());
    }

    private IEnumerator LoadShopSceneAsync()
    {
        // Chargement asynchrone pour éviter les freezes
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Melvin_Shop");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}