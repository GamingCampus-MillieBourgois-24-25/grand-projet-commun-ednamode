using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VotingManager : NetworkBehaviour
{
    private Dictionary<ulong, List<int>> votes = new();

    public void SubmitVote(ulong targetClientId, int note)
    {
        if (!IsClient) return;
        SubmitVoteServerRpc(targetClientId, note);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitVoteServerRpc(ulong targetClientId, int note)
    {
        if (!votes.ContainsKey(targetClientId))
            votes[targetClientId] = new List<int>();

        votes[targetClientId].Add(note);
    }

    public float GetAverage(ulong clientId)
    {
        if (!votes.ContainsKey(clientId)) return 0f;
        List<int> playerVotes = votes[clientId];
        float total = 0;
        foreach (int v in playerVotes) total += v;
        return total / playerVotes.Count;
    }

    public List<ulong> GetTopThree()
    {
        var sorted = new List<ulong>(votes.Keys);
        sorted.Sort((a, b) => GetAverage(b).CompareTo(GetAverage(a)));
        return sorted.GetRange(0, Mathf.Min(3, sorted.Count));
    }
}