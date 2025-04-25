using System;
using UnityEngine;
using DG.Tweening;

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

    #region Starting Screen
    public void PlayStartingScreen(Action onComplete = null)
    {
        startingScreenPanel.SetActive(true);
        startingScreenTransform.anchoredPosition = new Vector2(0, -Screen.height);

        startingScreenTransform.DOAnchorPosY(0, startingTransitionDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(delayBeforeHide, () =>
                {
                    startingScreenPanel.SetActive(false);
                    onComplete?.Invoke();
                });
            });
    }
    #endregion

    #region Scene Transition
    public void PlaySceneTransition(Action onComplete = null)
    {
        transitionCoverPanel.SetActive(true);
        transitionPanelTransform.anchoredPosition = new Vector2(0, Screen.height);

        transitionPanelTransform.DOAnchorPosY(0, sceneTransitionDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => onComplete?.Invoke());
    }
    #endregion

    #region Panel Juiciness
    public void AnimatePanelIn(GameObject panel)
    {
        panel.SetActive(true);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.localScale = Vector3.zero;
        rt.DOScale(1f, panelTransitionDuration).SetEase(Ease.OutBack);
    }

    public void AnimatePanelOut(GameObject panel)
    {
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.DOScale(0f, panelTransitionDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => panel.SetActive(false));
    }

    public void ReplacePanel(GameObject fromPanel, GameObject toPanel)
    {
        AnimatePanelOut(fromPanel);
        AnimatePanelIn(toPanel);
    }
    #endregion
}
