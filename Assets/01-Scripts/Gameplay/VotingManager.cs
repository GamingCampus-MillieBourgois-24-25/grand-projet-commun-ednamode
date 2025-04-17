// 🎯 VotingManager : collecte et calcule les votes de tous les joueurs pour chaque passage
// Supporte deux modes : étoiles (0–5) ou vote binaire (1 = dans le thème, 0 = hors thème)

using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class VotingManager : NetworkBehaviour
{
    public static VotingManager Instance { get; private set; }

    #region 🔢 Structures internes

    /// <summary>
    /// Contient les votes reçus pour un joueur.
    /// </summary>
    private class VoteData
    {
        public List<int> scores = new();

        public void AddVote(int score)
        {
            scores.Add(score);
        }

        public float GetAverage() => scores.Count > 0 ? (float)scores.Sum() / scores.Count : 0f;
        public int Count => scores.Count;
    }

    /// <summary>
    /// Scores accumulés pour chaque joueur ayant défilé.
    /// </summary>
    private Dictionary<ulong, VoteData> allVotes = new();

    #endregion

    #region 🧭 Initialisation

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #endregion

    #region 🗳️ Réception des votes

    /// <summary>
    /// Appelée par les clients pour voter pour un joueur en défilé.
    /// Le score dépend du mode : étoiles (1 à 5) ou binaire (0 ou 1).
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SubmitVote_ServerRpc(ulong targetClientId, int score, ServerRpcParams rpcParams = default)
    {
        if (!allVotes.ContainsKey(targetClientId))
            allVotes[targetClientId] = new VoteData();

        allVotes[targetClientId].AddVote(score);
        Debug.Log($"🗳️ Vote reçu pour {targetClientId} : {score}");
    }

    #endregion

    #region 🥇 Résultats

    /// <summary>
    /// Retourne la note moyenne d’un joueur (appelé en fin de phase pour podium).
    /// </summary>
    public float GetAverageScore(ulong clientId)
    {
        return allVotes.ContainsKey(clientId) ? allVotes[clientId].GetAverage() : 0f;
    }

    /// <summary>
    /// Renvoie tous les scores pour affichage du podium trié.
    /// </summary>
    public List<(ulong clientId, float score)> GetRankedResults()
    {
        return allVotes
            .Select(kvp => (clientId: kvp.Key, score: kvp.Value.GetAverage()))
            .OrderByDescending(result => result.score)
            .ToList();
    }

    /// <summary>
    /// Réinitialise tous les votes (à appeler entre les manches).
    /// </summary>
    public void ClearAllVotes()
    {
        allVotes.Clear();
    }

    #endregion
}
