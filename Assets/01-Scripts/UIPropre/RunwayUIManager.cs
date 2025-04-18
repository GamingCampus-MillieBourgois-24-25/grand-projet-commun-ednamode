// 🎯 RunwayUIManager : Gère l'affichage du panel pendant le défilé d'un joueur
// - Nom du joueur avec animation d'entrée
// - Système de vote dynamique selon le mode de jeu (étoiles, pouce haut/bas)
// - Timer visuel (slider)
// - Screenshot en popup à la fin du passage
// - Animation des étoiles cliquées et inactives

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
    [SerializeField] private Color defaultStarColor = Color.yellow;
    [SerializeField] private Color inactiveStarColor = Color.gray;
    [SerializeField] private float inactiveStarScale = 0.75f;

    [Header("👍👎 Boutons (Mode Impostor)")]
    [SerializeField] private GameObject thumbsVoteContainer;
    [SerializeField] private Button thumbsUpButton;
    [SerializeField] private Button thumbsDownButton;

    // [Header("📷 Screenshot")]
    // [SerializeField] private GameObject screenshotPopup;

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
        // screenshotPopup?.SetActive(false);
    }

    #endregion

    #region 🚀 Public API

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
        // screenshotPopup?.SetActive(false);
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
         // TriggerScreenshotPopup();
    }

    #endregion

    #region ⭐ Mode DressToImpress

    private void SetupVoteUIForCurrentMode()
    {
        starVoteContainer.SetActive(false);
        thumbsVoteContainer.SetActive(false);

        int mode = MultiplayerNetwork.Instance.SelectedGameMode.Value;

        if (mode == 0) // Dress To Impress
        {
            starVoteContainer.SetActive(true);
            ResetStarsToImpulse();

            for (int i = 0; i < starButtons.Length; i++)
            {
                int vote = i + 1;
                int index = i;

                starButtons[i].onClick.RemoveAllListeners();
                starButtons[i].onClick.AddListener(() => SubmitStarVote(vote));
            }
        }
        else if (mode == 2) // Impostor Mode
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

        for (int i = 0; i < starButtons.Length; i++)
        {
            Image img = starButtons[i].GetComponent<Image>();
            DOTween.Kill("StarPulse" + i);

            if (i < stars)
            {
                img.color = defaultStarColor;
                img.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
            }
            else
            {
                img.color = inactiveStarColor;
                img.transform.DOScale(inactiveStarScale, 0.2f).SetEase(Ease.InOutQuad);
            }
        }

        VotingManager.Instance.SubmitVote_ServerRpc(currentTargetClientId, stars);
    }

    private void ResetStarsToImpulse()
    {
        for (int i = 0; i < starButtons.Length; i++)
        {
            var image = starButtons[i].GetComponent<Image>();
            image.color = defaultStarColor;
            starButtons[i].transform.localScale = Vector3.one;
            starButtons[i].transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 2, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId("StarPulse" + i);
        }
    }

    #endregion

    #region 👍👎 Mode Impostor

    private void SubmitBinaryVote(bool isInTheme)
    {
        if (hasVoted) return;
        hasVoted = true;

        int score = isInTheme ? 1 : 0;
        VotingManager.Instance.SubmitVote_ServerRpc(currentTargetClientId, score);
    }

    #endregion

/*    #region 📸 Screenshot

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
*/
}
