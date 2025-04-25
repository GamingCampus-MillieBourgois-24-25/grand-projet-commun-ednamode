using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;

public class PodiumUIManager : MonoBehaviour
{
    public static PodiumUIManager Instance { get; private set; }

    [Header("🖥️ Références UI")]
    [SerializeField] private RectTransform podiumPanel;  // ⚠️ RectTransform pour l'animation
    [SerializeField] private Transform rankingContainer; // Content de la ScrollView
    [SerializeField] private GameObject rankingEntryPrefab;

    [Header("🎨 Options d'affichage")]
    [SerializeField] private Color firstPlaceColor = Color.yellow;
    [SerializeField] private Color secondPlaceColor = Color.gray;
    [SerializeField] private Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        podiumPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// Affiche le classement avec une animation de slide depuis la droite.
    /// </summary>
    public void ShowRanking(List<(ulong clientId, float score)> rankings)
    {
        if (podiumPanel == null)
        {
            Debug.LogError("[PodiumUI] ❌ podiumPanel n'est pas assigné !");
            return;
        }

        ClearRankingUI();

        foreach (var (clientId, score) in rankings)
        {
            GameObject entry = Instantiate(rankingEntryPrefab, rankingContainer);

            TMP_Text[] texts = entry.GetComponentsInChildren<TMP_Text>();
            if (texts.Length < 2)
            {
                Debug.LogError("[PodiumUI] Prefab mal configuré : 2 TMP_Text attendus.");
                continue;
            }

            string playerName = MultiplayerManager.Instance.GetDisplayName(clientId);
            float roundedScore = Mathf.Round(score * 10f) / 10f;

            texts[0].text = playerName;           // Nom du joueur
            texts[1].text = $"{roundedScore} ⭐";  // Score

            Image bg = entry.GetComponent<Image>();
            if (bg != null)
                bg.color = GetPlaceColor(rankingContainer.childCount - 1);
        }

        // Force le layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(rankingContainer.GetComponent<RectTransform>());

        // Animation de slide depuis la droite
        podiumPanel.gameObject.SetActive(true);
        podiumPanel.anchoredPosition = new Vector2(Screen.width, 0);
        podiumPanel.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);
    }

    /// <summary>
    /// Cache le podium avec une animation.
    /// </summary>
    public void HideRanking()
    {
        if (podiumPanel == null)
        {
            Debug.LogError("[PodiumUI] ❌ podiumPanel non assigné !");
            podiumPanel.gameObject.SetActive(false);
            return;
        }
/*
        podiumPanel.DOKill();  // Stoppe toute animation en cours pour éviter les conflits

        podiumPanel.DOAnchorPosX(Screen.width, 0.4f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                podiumPanel.gameObject.SetActive(false);
                Debug.Log("[PodiumUI] 🏁 Podium désactivé après animation.");
            });

        // 🛡️ Sécurité : désactive de toute façon après un délai (failsafe)
        StartCoroutine(ForceDeactivateAfterDelay(0.5f));
*/    }

    /// <summary>
    /// Désactive le podium après un délai en sécurité.
    /// </summary>
    private IEnumerator ForceDeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (podiumPanel.gameObject.activeSelf)
        {
            podiumPanel.gameObject.SetActive(false);
            Debug.LogWarning("[PodiumUI] ⚠️ Podium forcé OFF après délai de sécurité.");
        }
    }

    private void ClearRankingUI()
    {
        foreach (Transform child in rankingContainer)
            Destroy(child.gameObject);
    }

    private Color GetPlaceColor(int index)
    {
        return index switch
        {
            0 => firstPlaceColor,
            1 => secondPlaceColor,
            2 => thirdPlaceColor,
            _ => Color.white
        };
    }
}
