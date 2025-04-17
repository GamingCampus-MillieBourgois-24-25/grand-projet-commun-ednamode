// 🎯 RunwayUIManager : gère l'affichage du panel pendant le défilé d'un joueur
// - Nom du joueur avec animation d'entrée
// - Système de vote dynamique selon le mode de jeu (étoiles, pouce haut/bas)
// - Timer visuel (slider)
// - Screenshot en popup à la fin du passage

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class RunwayUIManager : MonoBehaviour
{
    #region 🔗 Références

    public static RunwayUIManager Instance { get; private set; }

    [Header("🎥 UI Principal")]
    [Tooltip("Conteneur principal du panneau Runway visible pendant le passage d'un joueur.")]
    [SerializeField] private GameObject runwayPanel;

    [Tooltip("Nom du joueur actuellement en défilé.")]
    [SerializeField] private TMP_Text playerNameText;

    [Tooltip("Effet de texte animé (entrée style TV).")]
    [SerializeField] private Animator nameAnimator;

    [Tooltip("Slider de temps restant pour voter.")]
    [SerializeField] private Slider timerSlider;

    [Header("🌟 Étoiles (Mode DressToImpress)")]
    [SerializeField] private GameObject starVoteContainer;
    [SerializeField] private Button[] starButtons;
    [SerializeField] private Animator[] starAnimators;

    [Header("👍👎 Boutons (Mode Impostor)")]
    [SerializeField] private GameObject thumbsVoteContainer;
    [SerializeField] private Button thumbsUpButton;
    [SerializeField] private Button thumbsDownButton;

    [Header("📷 Screenshot")]
    [SerializeField] private GameObject screenshotPopup;

    private ulong currentTargetClientId;
    private float voteDuration;
    private Coroutine timerCoroutine;
    private bool hasVoted = false;

    #endregion

    #region 🧭 Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        starVoteContainer?.SetActive(false);
        thumbsVoteContainer?.SetActive(false);
        runwayPanel?.SetActive(false);
        screenshotPopup?.SetActive(false);
    }

    #endregion

    #region 🚀 Public API

    /// <summary>
    /// Déclenché par le serveur via ClientRpc : lance le panneau et le vote.
    /// </summary>
    public void ShowCurrentRunwayPlayer(ulong clientId)
    {
        runwayPanel.SetActive(true);
        currentTargetClientId = clientId;
        voteDuration = RunwayManager.Instance.GetRunwayDuration();
        hasVoted = false;

        string displayName = MultiplayerNetwork.Instance.GetDisplayName(clientId);
        playerNameText.text = displayName;

        nameAnimator?.SetTrigger("Enter");

        SetupVoteUIForCurrentMode();

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerCountdownCoroutine(voteDuration));
    }

    public void HideRunwayPanel()
    {
        runwayPanel.SetActive(false);
        screenshotPopup?.SetActive(false);
    }

    #endregion

    #region ⌛ Timer

    private IEnumerator TimerCountdownCoroutine(float duration)
    {
        timerSlider.maxValue = duration;
        timerSlider.value = duration;

        while (timerSlider.value > 0f)
        {
            timerSlider.value -= Time.deltaTime;
            yield return null;
        }

        timerSlider.value = 0f;
        TriggerScreenshotPopup();
    }

    #endregion

    #region ⭐ Mode DressToImpress

    private void SetupVoteUIForCurrentMode()
    {
        starVoteContainer.SetActive(false);
        thumbsVoteContainer.SetActive(false);

        var mode = MultiplayerNetwork.Instance.SelectedGameMode.Value;
        if (mode == 0) // Dress To Impress
        {
            starVoteContainer.SetActive(true);
            for (int i = 0; i < starButtons.Length; i++)
            {
                int vote = i + 1;
                starButtons[i].onClick.RemoveAllListeners();
                starButtons[i].onClick.AddListener(() => SubmitStarVote(vote));
            }
        }
        else if (mode == 1) // Impostor Mode
        {
            thumbsVoteContainer.SetActive(true);
            thumbsUpButton.onClick.RemoveAllListeners();
            thumbsDownButton.onClick.RemoveAllListeners();
            thumbsUpButton.onClick.AddListener(() => SubmitBinaryVote(true));
            thumbsDownButton.onClick.AddListener(() => SubmitBinaryVote(false));
        }
    }

    private void SubmitStarVote(int stars)
    {
        if (hasVoted) return;
        hasVoted = true;

        for (int i = 0; i < starAnimators.Length; i++)
        {
            if (i < stars)
                starAnimators[i].SetTrigger("Vote");
        }

        VotingManager.Instance.SubmitVote_ServerRpc(currentTargetClientId, stars);
    }

    private void SubmitBinaryVote(bool isInTheme)
    {
        if (hasVoted) return;
        hasVoted = true;

        int score = isInTheme ? 1 : 0;
        VotingManager.Instance.SubmitVote_ServerRpc(currentTargetClientId, score);
    }

    #endregion

    #region 📸 Screenshot

    private void TriggerScreenshotPopup()
    {
        screenshotPopup?.SetActive(true);
    }

    public void ConfirmScreenshot()
    {
        string path = Application.persistentDataPath + "/runway_" + currentTargetClientId + ".png";
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log($"📷 Screenshot sauvegardé : {path}");
        screenshotPopup?.SetActive(false);
    }

    public void CancelScreenshot()
    {
        screenshotPopup?.SetActive(false);
    }

    #endregion
}