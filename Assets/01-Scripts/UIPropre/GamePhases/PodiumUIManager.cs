using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class PodiumUIManager : MonoBehaviour
{
    public static PodiumUIManager Instance { get; private set; }

    [Header("🖥️ Références UI")]
    [SerializeField] private GameObject podiumPanel;
    [SerializeField] private Transform rankingContainer;
    [SerializeField] private GameObject rankingEntryPrefab;

    [Header("🎨 Options d'affichage")]
    [Tooltip("Couleur pour le 1er joueur")]
    [SerializeField] private Color firstPlaceColor = Color.yellow;
    [Tooltip("Couleur pour le 2ème joueur")]
    [SerializeField] private Color secondPlaceColor = Color.gray;
    [Tooltip("Couleur pour le 3ème joueur")]
    [SerializeField] private Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        podiumPanel.SetActive(false);
    }

    /// <summary>
    /// Affiche le classement des joueurs sur le podium avec leurs scores.
    /// </summary>
    public void ShowRanking(List<(ulong clientId, float score)> rankings)
    {
        ClearRankingUI();

        for (int i = 0; i < rankings.Count; i++)
        {
            var entryData = rankings[i];
            var entryObj = Instantiate(rankingEntryPrefab, rankingContainer);
            var texts = entryObj.GetComponentsInChildren<TMP_Text>();

            string playerName = MultiplayerManager.Instance.GetDisplayName(entryData.clientId);
            float score = Mathf.Round(entryData.score * 10f) / 10f;  // arrondi 1 décimale

            texts[0].text = $"{i + 1}. {playerName}";
            texts[1].text = $"{score} ⭐";

            var bg = entryObj.GetComponent<Image>();
            if (bg != null)
                bg.color = GetPlaceColor(i);
        }

        podiumPanel.SetActive(true);
    }

    /// <summary>
    /// Cache le panneau du podium.
    /// </summary>
    public void HideRanking()
    {
        podiumPanel.SetActive(false);
    }

    /// <summary>
    /// Nettoie les anciennes entrées du classement.
    /// </summary>
    private void ClearRankingUI()
    {
        foreach (Transform child in rankingContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Retourne une couleur en fonction du rang.
    /// </summary>
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
