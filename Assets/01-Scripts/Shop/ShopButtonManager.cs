using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // N�cessaire pour SceneManager

public class ShopButtonManager : MonoBehaviour
{
    [Header("?? Bouton Boutique")]
    [Tooltip("Bouton qui lance la sc�ne Melvin_Shop")]
    [SerializeField] private Button shopButton;

    [Header("?? Transition (Optionnel)")]
    [Tooltip("Panneau pour l'animation de transition (ex: fondu noir)")]
    [SerializeField] private RectTransform transitionPanel;
    [Tooltip("Position initiale du panneau (ex: hors �cran)")]
    [SerializeField] private Vector2 transitionStartPosition = new(-1850, 1450);
    [Tooltip("Position finale du panneau (ex: centre �cran)")]
    [SerializeField] private Vector2 transitionEndPosition = Vector2.zero;
    [Tooltip("Dur�e de l'animation en secondes")]
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("?? Son (Optionnel)")]
    [Tooltip("Son jou� lors du clic")]
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
            Debug.LogWarning("[ShopButtonManager] ShopButton NON assign� dans l'Inspector !");
        }

        // D�sactiver le panneau de transition
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

        // Charger la sc�ne
        yield return StartCoroutine(LoadShopSceneAsync());
    }

    private IEnumerator LoadShopSceneAsync()
    {
        // Chargement asynchrone pour �viter les freezes
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Melvin_Shop");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}