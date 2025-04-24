using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    #region Singleton
    public static UIManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializePanels();

        if (autoPlayIntroOnStart)
            PlaySceneIntro();
    }
    #endregion

    #region Structs & Fields
    [System.Serializable]
    public class PanelEntry
    {
        public string panelName;
        public GameObject panelObject;
        public Button triggerButton;
        public bool startVisible;
        public bool closeOnOutsideClick = true;

        [Header("➕ Options avancées")]
        public bool closeOtherPanelWhenOpened = true;
        public bool hasCloseButton = false;           // ❌ pour affichage croix
        public bool useSpecialAnimation = false;

        public enum SpecialAnimationType
        {
            None,
            SlideFromTop,
            SlideFromBottom,
            SlideFromLeft,
            SlideFromRight,
            FlipCard,
            PopElastic,
            BounceIn
        }
        public SpecialAnimationType specialAnimation = SpecialAnimationType.None;
    }

    [Header("🧩 Panels")]
    [SerializeField] private List<PanelEntry> panels = new();
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private Ease animationEase = Ease.OutBack;
    [SerializeField] private Vector3 hiddenScale = new(0.85f, 0.85f, 1);

    private GameObject _currentPanel;
    private Dictionary<string, GameObject> _panelDict;
    private Dictionary<Button, string> _buttonToPanel;
    private bool _isAnimating = false;

    public bool useSpecialAnimation = false;

    #endregion

    #region 🔊 UI Sounds
    [Header("🔊 Sons UI")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip panelOpenSound;
    [SerializeField] private AudioClip panelCloseSound;
    [SerializeField] private AudioClip transitionIntroSound;
    [SerializeField] private AudioClip transitionOutroSound;

    private void PlaySound(AudioClip clip)
    {
        if (uiAudioSource && clip)
            uiAudioSource.PlayOneShot(clip);
    }
    #endregion

    #region 🎬 Transition Scene UI
    [Header("🎬 Transition d'Écran")]
    [SerializeField] private RectTransform screenTransitionPanel;
    [SerializeField] private Vector2 centerPosition = Vector2.zero;
    [SerializeField] private Vector2 openExitPosition = new(1850, -1450);
    [SerializeField] private Vector2 closeStartPosition = new(-1850, 1450);
    [SerializeField] private float screenMoveDuration = 0.5f;
    [SerializeField] private bool autoPlayIntroOnStart = true;

    public void PlaySceneIntro(System.Action onComplete = null)
    {
        if (screenTransitionPanel == null) return;
        screenTransitionPanel.gameObject.SetActive(true);
        PlaySound(transitionIntroSound);
        screenTransitionPanel.anchoredPosition = centerPosition;
        StartCoroutine(MovePanel(screenTransitionPanel, centerPosition, openExitPosition, onComplete));
    }

    public void PlaySceneOutro(System.Action onComplete = null)
    {
        if (screenTransitionPanel == null) return;
        screenTransitionPanel.gameObject.SetActive(true);
        PlaySound(transitionOutroSound);
        screenTransitionPanel.anchoredPosition = closeStartPosition;
        StartCoroutine(MovePanel(screenTransitionPanel, closeStartPosition, centerPosition, onComplete));
    }

    private IEnumerator MovePanel(RectTransform panel, Vector2 from, Vector2 to, System.Action onComplete)
    {
        float elapsed = 0f;
        panel.anchoredPosition = from;

        while (elapsed < screenMoveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / screenMoveDuration);
            panel.anchoredPosition = Vector2.Lerp(from, to, progress);
            yield return null;
        }

        panel.anchoredPosition = to;
        onComplete?.Invoke();
    }
    #endregion

    #region 📱 Gestes Tactiles
    [Header("📱 Gestes Tactiles")]
    public UnityEvent OnSwipeLeft;
    public UnityEvent OnSwipeRight;
    public UnityEvent OnTap;
    public UnityEvent OnDoubleTap;

    private Vector2 _touchStartPos;
    private float _lastTapTime;
    private bool _waitingSecondTap;

    private void DetectTouchGestures()
    {
        if (Input.touchCount != 1) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                _touchStartPos = touch.position;

                if (_waitingSecondTap && Time.time - _lastTapTime < 0.3f)
                {
                    OnDoubleTap?.Invoke();
                    _waitingSecondTap = false;
                }
                else
                {
                    _lastTapTime = Time.time;
                    _waitingSecondTap = true;
                }
                break;

            case TouchPhase.Ended:
                float deltaX = touch.position.x - _touchStartPos.x;

                if (Mathf.Abs(deltaX) > 100f)
                {
                    if (deltaX > 0) OnSwipeRight?.Invoke();
                    else OnSwipeLeft?.Invoke();
                }
                else
                {
                    OnTap?.Invoke();
                }
                break;
        }
    }
    #endregion

    #region 🎯 Panel Management
    private List<GameObject> _previouslyClosedPanels = new();
    private Stack<GameObject> _panelHistory = new();

    private void InitializePanels()
    {
        _panelDict = new();
        _buttonToPanel = new();

        foreach (var entry in panels)
        {
            if (entry.panelObject == null) continue;

            string key = entry.panelName.Trim();
            _panelDict[key] = entry.panelObject;

            if (entry.startVisible)
            {
                ShowPanel(key, true);
                _currentPanel = entry.panelObject;
            }
            else
            {
                entry.panelObject.SetActive(false);
                entry.panelObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                entry.panelObject.transform.localScale = hiddenScale;
            }

            if (entry.triggerButton != null)
            {
                _buttonToPanel[entry.triggerButton] = key;
                entry.triggerButton.onClick.AddListener(() => OnPanelButtonClicked(entry.triggerButton));
            }
            if (entry.hasCloseButton)
            {
                // Recherche dans les enfants actifs/inactifs
                Button[] buttons = entry.panelObject.GetComponentsInChildren<Button>(true);
                foreach (var btn in buttons)
                {
                    string btnName = btn.name.ToLowerInvariant();

                    if (btnName.Contains("close"))
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => OnCloseAndRestore(entry.panelObject));
                        Debug.Log($"[UIManager] 🔒 Bouton 'Close' trouvé et relié pour : {entry.panelName}");
                        break;
                    }
                }
            }

        }
    }

    private void OnPanelButtonClicked(Button clickedButton)
    {
        PlaySound(clickSound);

        if (!_buttonToPanel.TryGetValue(clickedButton, out var panelName)) return;

        var targetPanel = _panelDict[panelName];

        // Cas : on clique sur le bouton du panel déjà ouvert → on le ferme
        if (_currentPanel == targetPanel)
        {
            HidePanel(targetPanel);
            return;
        }

        // Cas : un autre panel était ouvert → on le ferme puis on ouvre le nouveau
        ShowPanel(panelName);
    }

    public void ShowPanel(string panelName, bool instant = false, bool replaceCurrentPanel = true)
    {
        if (!_panelDict.ContainsKey(panelName) || _isAnimating) return;

        var panelToShow = _panelDict[panelName];
        var entry = panels.Find(p => p.panelName == panelName);

        if (_currentPanel == panelToShow)
        {
            // S'il est déjà actif et qu'on re-clique dessus
            HidePanel(panelToShow, instant);
            return;
        }

        if (_currentPanel != null && _currentPanel != panelToShow && replaceCurrentPanel)
        {
            if (!_previouslyClosedPanels.Contains(_currentPanel))
                _previouslyClosedPanels.Add(_currentPanel);

            _panelHistory.Push(_currentPanel);
            HidePanel(_currentPanel, instant);
        }

        if (panelToShow.activeSelf && _currentPanel == panelToShow)
        {
            return;
        }

        panelToShow.SetActive(true);
        PlaySound(panelOpenSound);

        RectTransform rect = panelToShow.GetComponent<RectTransform>();
        rect.localScale = hiddenScale;
        rect.anchoredPosition = Vector2.zero;

        if (instant)
        {
            rect.localScale = Vector3.one;
        }
        else
        {
            if (entry.useSpecialAnimation)
                AnimateSpecialOpen(panelToShow, entry.specialAnimation);
            else
                rect.DOScale(Vector3.one, animationDuration).SetEase(animationEase);
        }

        _currentPanel = panelToShow;
    }
    public void ShowPanelDirect(GameObject panel)
    {
        panel.SetActive(true);
    }

    public void HidePanel(GameObject panel, bool instant = false)
    {
        if (panel == null || _isAnimating) return;
        PlaySound(panelCloseSound);

        RectTransform rect = panel.GetComponent<RectTransform>();
        _isAnimating = true;

        if (instant)
        {
            panel.SetActive(false);
            _isAnimating = false;
            if (panel == _currentPanel) _currentPanel = null;
            return;
        }

        rect.DOKill();

        // 🔍 Cherche s’il y a une animation spéciale configurée
        var entry = panels.Find(p => p.panelObject == panel);

        if (entry != null && entry.useSpecialAnimation)
        {
            switch (entry.specialAnimation)
            {
                case PanelEntry.SpecialAnimationType.FlipCard:
                    panel.transform.DORotate(new Vector3(0, -90, 0), 0.3f, RotateMode.Fast)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            panel.SetActive(false);
                            panel.transform.rotation = Quaternion.identity;
                            FinalizePanelClose(panel);
                        });
                    return;

                case PanelEntry.SpecialAnimationType.SlideFromTop:
                case PanelEntry.SpecialAnimationType.SlideFromBottom:
                case PanelEntry.SpecialAnimationType.SlideFromLeft:
                case PanelEntry.SpecialAnimationType.SlideFromRight:
                    Vector2 exitDir = GetSlideDirection(entry.specialAnimation);
                    rect.DOAnchorPos(exitDir, animationDuration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            panel.SetActive(false);
                            rect.anchoredPosition = Vector2.zero;
                            FinalizePanelClose(panel);
                        });
                    return;

                case PanelEntry.SpecialAnimationType.PopElastic:
                case PanelEntry.SpecialAnimationType.BounceIn:
                    rect.DOScale(Vector3.zero, 0.3f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            panel.SetActive(false);
                            rect.localScale = Vector3.one;
                            FinalizePanelClose(panel);
                        });
                    return;
            }
        }

        // ✅ Animation par défaut : vers le bas
        rect.DOAnchorPosY(-Screen.height, animationDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                panel.SetActive(false);
                rect.anchoredPosition = Vector2.zero;
                FinalizePanelClose(panel);
            });
    }

    // Utilitaire pour masquer un panel par son nom
    public void HidePanelByName(string panelName, bool instant = false)
    {
        if (_panelDict.TryGetValue(panelName, out var panel))
        {
            HidePanel(panel, instant);
        }
        else
        {
            Debug.LogWarning($"[UIManager] Aucun panel trouvé pour le nom : {panelName}");
        }
    }


    // Utilitaire final
    private void FinalizePanelClose(GameObject panel)
    {
        _isAnimating = false;
        if (panel == _currentPanel)
            _currentPanel = null;
    }

    // 🔁 Retourne la direction de fermeture pour les slides
    private Vector2 GetSlideDirection(PanelEntry.SpecialAnimationType type)
    {
        switch (type)
        {
            case PanelEntry.SpecialAnimationType.SlideFromTop:
                return new Vector2(0, Screen.height);
            case PanelEntry.SpecialAnimationType.SlideFromBottom:
                return new Vector2(0, -Screen.height);
            case PanelEntry.SpecialAnimationType.SlideFromLeft:
                return new Vector2(-Screen.width, 0);
            case PanelEntry.SpecialAnimationType.SlideFromRight:
                return new Vector2(Screen.width, 0);
            default:
                return Vector2.zero;
        }
    }

    public void HideAllPanels(bool instant = false)
    {
        _isAnimating = false; // stoppe toute animation en cours (si mal annulée)

        foreach (var kvp in _panelDict)
        {
            if (kvp.Value.activeSelf)
                HidePanel(kvp.Value, instant);
        }

        _currentPanel = null;
    }

    public void OnCloseAndRestore()
    {
        HidePanel(_currentPanel);
        RestorePreviousPanels();
    }

    public void GoBackToPreviousPanel()
    {
        if (_panelHistory.Count > 0)
        {
            var previousPanel = _panelHistory.Pop();
            if (previousPanel != null)
                ShowPanel(previousPanel.name, false, replaceCurrentPanel: false);
        }
    }

    public void OnCloseAndRestore(GameObject panel)
    {
        HidePanel(panel);
        RestorePreviousPanels();
    }

    public void RestorePreviousPanels()
    {
        foreach (var panel in _previouslyClosedPanels)
        {
            if (panel != null)
                ShowPanel(panel.name, false, replaceCurrentPanel: false);
        }

        _previouslyClosedPanels.Clear();
    }

    public void ShowThemePanel(ThemeData theme)
    {
        var panel = _panelDict["ThemePanel"];
        panel.SetActive(true);

        panel.GetComponentInChildren<TMP_Text>().text = theme.themeName;
        panel.GetComponentInChildren<Image>().sprite = theme.themeIcon;

        // Animation d'apparition (ex: rotation type panorama)
        panel.transform.localEulerAngles = new Vector3(0, 720, 0);
        panel.transform.DORotate(Vector3.zero, 2f, RotateMode.FastBeyond360).SetEase(Ease.OutExpo);
    }

    public void ShowCategoryPanel(ThemeData.ThemeCategory category)
    {
        var panel = _panelDict["CategoryPanel"];
        panel.SetActive(true);

        var text = panel.GetComponentInChildren<TMP_Text>();
        text.text = $"Category: {category}";

        panel.transform.localScale = Vector3.zero;
        panel.transform.DOScale(1f, 1f).SetEase(Ease.OutBack);
    }

    #endregion

    #region 🖱 Outside Click Detection & Utility
    private void Update()
    {
        DetectTouchGestures();

        if (_currentPanel == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            RectTransform rect = _currentPanel.GetComponent<RectTransform>();
            if (!RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null))
            {
                string panelName = GetCurrentPanelName();
                PanelEntry entry = panels.Find(p => p.panelName == panelName);
                if (entry != null && entry.closeOnOutsideClick)
                {
                    HidePanel(_currentPanel);
                }
            }
        }
    }

    public bool IsAnyPanelOpen() => _currentPanel != null;

    public string GetCurrentPanelName()
    {
        foreach (var kvp in _panelDict)
        {
            if (kvp.Value == _currentPanel)
                return kvp.Key;
        }
        return null;
    }
    #endregion

    #region ⏳ Countdown
    [Header("⏳ Compte à rebours")]
    [Tooltip("Panel de compte à rebours à afficher.")]
    [SerializeField] private GameObject countdownPanel;
    [Tooltip("Texte du compte à rebours à mettre à jour.")]
    [SerializeField] private TMP_Text countdownText;
    [Tooltip("Durée du compte à rebours en secondes.")]
    [Range(1, 30)]
    [SerializeField] private int countdownDuration = 10;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color alertColor = Color.red;

    private Coroutine countdownRoutine;

    /// <summary>
    /// Lance le compte à rebours et exécute l'action à la fin.
    /// </summary>
    public void StartCountdown(System.Action onComplete)
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
        }
        countdownRoutine = StartCoroutine(CountdownRoutine(onComplete));
    }

    private IEnumerator CountdownRoutine(System.Action onComplete)
    {
        countdownPanel.SetActive(true);
        countdownText.gameObject.SetActive(true);

        for (int i = countdownDuration; i >= 0; i--)
        {
            countdownText.text = i.ToString();
            countdownText.fontSize = (i <= 3) ? 160 : 100;
            countdownText.color = (i <= 3) ? alertColor : normalColor;

            countdownText.transform.localScale = Vector3.one * 0.5f;
            countdownText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
   /*         if (i <= 3 && i > 0)
            {
                VibrationManager.Vibrate();
            }*/

            yield return new WaitForSeconds(1f);
        }

        countdownPanel.SetActive(false);
        countdownText.gameObject.SetActive(false);
        onComplete?.Invoke();
        countdownRoutine = null;
    }

    /// <summary>
    /// Annule le compte à rebours en cours.
    /// </summary>
    public void CancelCountdown()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        countdownText.gameObject.SetActive(false);
        countdownPanel?.SetActive(false);
    }
    #endregion

    #region 🎨 Animation Spéciale
    private void AnimateSpecialOpen(GameObject panel, PanelEntry.SpecialAnimationType type)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        Vector2 start = Vector2.zero;

        switch (type)
        {
            case PanelEntry.SpecialAnimationType.SlideFromTop:
                start = new Vector2(0, Screen.height);
                break;
            case PanelEntry.SpecialAnimationType.SlideFromBottom:
                start = new Vector2(0, -Screen.height);
                break;
            case PanelEntry.SpecialAnimationType.SlideFromLeft:
                start = new Vector2(-Screen.width, 0);
                break;
            case PanelEntry.SpecialAnimationType.SlideFromRight:
                start = new Vector2(Screen.width, 0);
                break;
            case PanelEntry.SpecialAnimationType.FlipCard:
                panel.transform.localRotation = Quaternion.Euler(0, 90, 0);
                panel.transform.DORotate(Vector3.zero, 0.5f, RotateMode.Fast).SetEase(Ease.OutBack);
                return;
            case PanelEntry.SpecialAnimationType.PopElastic:
                rect.localScale = Vector3.zero;
                rect.DOScale(1f, 0.6f).SetEase(Ease.OutElastic);
                return;
            case PanelEntry.SpecialAnimationType.BounceIn:
                rect.localScale = Vector3.one * 1.2f;
                rect.DOScale(1f, 0.4f).SetEase(Ease.OutBounce);
                return;
        }

        rect.anchoredPosition = start;
        rect.DOAnchorPos(Vector2.zero, animationDuration).SetEase(Ease.OutExpo);
    }

    #endregion

    #region SceneManagement
    
    public void loadShop()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Melvin_Shop");
    }
    #endregion
}