using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    [Header("🎁 UI References")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text jewelsText;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private Button adButton;
    [SerializeField] private Button continueButton;

    [Header("⚡ Animation Settings")]
    [SerializeField] private float panelSlideDuration = 1f;
    [SerializeField] private float countDuration = 1f;

    [Header("🎯 Reward Values")]
    public int baseCoins = 50;
    public int baseJewels = 5;
    public int bonusFirstPlaceCoins = 200;
    public int bonusFirstPlaceJewels = 20;
    public int bonusSecondPlaceCoins = 150;
    public int bonusSecondPlaceJewels = 15;
    public int bonusThirdPlaceCoins = 100;
    public int bonusThirdPlaceJewels = 10;

    private int coinsEarned;
    private int jewelsEarned;
    private int xpEarned;

    public bool IsRewardPhaseComplete { get; private set; } = false;
    private bool hasWatchedAd = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        rewardPanel.SetActive(false);
    }

    public void StartRewardPhase()
    {
        Debug.Log("[RewardManager] 🎉 Début de la phase de récompense !");
        IsRewardPhaseComplete = false;
        hasWatchedAd = false;

        CalculateRewards();
        StartCoroutine(ShowRewardPanel());
    }

    private void CalculateRewards()
    {
        int playerRank = VotingManager.Instance.GetLocalPlayerRank();

        Debug.Log($"[RewardManager] 🎖 Classement du joueur : {playerRank}");

        if (playerRank == 1)
        {
            coinsEarned = bonusFirstPlaceCoins;
            jewelsEarned = bonusFirstPlaceJewels;
        }
        else if (playerRank == 2)
        {
            coinsEarned = bonusSecondPlaceCoins;
            jewelsEarned = bonusSecondPlaceJewels;
        }
        else if (playerRank == 3)
        {
            coinsEarned = bonusThirdPlaceCoins;
            jewelsEarned = bonusThirdPlaceJewels;
        }
        else
        {
            coinsEarned = baseCoins;
            jewelsEarned = baseJewels;
        }

        xpEarned = 10 + playerRank; // Exemple d'XP simple, à adapter si besoin
    }

    private IEnumerator ShowRewardPanel()
    {
        rewardPanel.SetActive(true);

        yield return new WaitForSeconds(panelSlideDuration * 0.5f);

        // Lancer les compteurs
        DOTween.To(() => 0, x => coinsText.text = $"{x}", coinsEarned, countDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => PunchScale(coinsText.transform));

        DOTween.To(() => 0, x => jewelsText.text = $"{x}", jewelsEarned, countDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => PunchScale(jewelsText.transform));

        DOTween.To(() => 0, x => xpText.text = $"{x}", xpEarned, countDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => PunchScale(xpText.transform));


        adButton.onClick.RemoveAllListeners();
        adButton.onClick.AddListener(OnAdButtonClicked);

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(OnContinueButtonClicked);
    }

    private void OnAdButtonClicked()
    {
        Debug.Log("[RewardManager] 🎬 Simulation de pub regardée !");
        hasWatchedAd = true;

        // Doubler les gains
        coinsEarned *= 2;
        jewelsEarned *= 2;
        xpEarned *= 2;

        // Remettre à jour les compteurs instantanément
        coinsText.text = $"{coinsEarned}";
        jewelsText.text = $"{jewelsEarned}";
        xpText.text = $"{xpEarned}";

        adButton.interactable = false; // désactiver le bouton après usage
    }

    private void OnContinueButtonClicked()
    {
        Debug.Log("[RewardManager] ✅ Le joueur continue.");

        // Ajouter les gains
        DataSaver.Instance.addCoins(coinsEarned);
        DataSaver.Instance.addJewels(jewelsEarned);
        DataSaver.Instance.addLevelProgress(xpEarned);

        IsRewardPhaseComplete = true;
        rewardPanel.SetActive(false);
    }

    private void PunchScale(Transform target)
    {
        if (target == null) return;

        target.DOKill(); // Toujours tuer les anciens tweens sur l'objet
        target.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0.8f);
    }

}
