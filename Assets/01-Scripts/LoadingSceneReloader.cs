using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class LoadingSceneManager : MonoBehaviour
{
    [Header("🎨 Loading UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private Image fadeBackground;
    [SerializeField] private Slider progressBar;

    [Header("⚙️ Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool useFade = true;

    [Tooltip("Minimum time (in seconds) the loading screen stays visible")]
    [SerializeField] private float minimumDisplayTime = 3f;

    [Header("💡 Tips & Fun Facts")]
    [TextArea]
    [SerializeField]
    private List<string> loadingTips = new List<string>
    {
        "Tip: Customize your outfit to impress the jury!",
        "Fun Fact: The dev team survived on coffee and cigarettes during this project.",
        "Tip: Colors matter! Use complementary colors for better scores.",
        "Fun Fact: Our artist designed over 50 unique outfits!",
        "Fun Fact: The business team pitched this game idea in under 2 minutes!",
        "Fun Fact: One developer broke Unity twice in a single day.",
        "Tip: Collaborate with teammates for the best runway looks.",
        "Fun Fact: The game's name was decided after 3 hours of debate."
    };

    private static LoadingSceneManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void LoadSceneWithTransition(string sceneName)
    {
        if (instance == null)
        {
            GameObject loader = new GameObject("LoadingSceneManager");
            instance = loader.AddComponent<LoadingSceneManager>();
        }

        instance.StartCoroutine(instance.LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        float displayStartTime = Time.time;

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        StartCoroutine(AnimateLoadingText());
        DisplayRandomTip();

        if (progressBar != null)
            progressBar.value = 0f;

        if (useFade && fadeBackground != null)
        {
            Color initialColor = fadeBackground.color;
            initialColor.a = 0f;
            fadeBackground.color = initialColor;

            fadeBackground.DOFade(1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        AsyncOperation asyncLoad = UnitySceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            if (progressBar != null)
                progressBar.value = Mathf.Lerp(progressBar.value, asyncLoad.progress, Time.deltaTime * 3f);

            yield return null;
        }

        if (progressBar != null)
            progressBar.value = 1f;

        // Calcul du temps écoulé et attente si nécessaire
        float elapsed = Time.time - displayStartTime;
        if (elapsed < minimumDisplayTime)
        {
            yield return new WaitForSeconds(minimumDisplayTime - elapsed);
        }

        asyncLoad.allowSceneActivation = true;

        yield return null;

        if (useFade && fadeBackground != null)
        {
            fadeBackground.DOFade(0f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        Destroy(gameObject);
    }

    private void DisplayRandomTip()
    {
        if (tipText != null && loadingTips.Count > 0)
        {
            int index = Random.Range(0, loadingTips.Count);
            tipText.text = loadingTips[index];
        }
    }

    private IEnumerator AnimateLoadingText()
    {
        string baseText = "Loading";
        int dotCount = 0;

        while (true)
        {
            if (loadingText != null)
            {
                loadingText.text = baseText + new string('.', dotCount);
                dotCount = (dotCount + 1) % 4;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
