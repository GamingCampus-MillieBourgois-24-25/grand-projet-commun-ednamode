// ?????????????????????????????????????????????????????????????
// ?? RELAY UTILS – Allocation + Connexion UnityTransport (Fallback Compatible Unity 6)
// ?????????????????????????????????????????????????????????????

using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public static class RelayUtils
{
    public static async Task<(Allocation allocation, string joinCode)> CreateRelayAsync(int maxPlayers)
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log($"[Relay] Allocation créée. JoinCode = {joinCode}");
        return (allocation, joinCode);
    }

    public static async Task<JoinAllocation> JoinRelayAsync(string joinCode)
    {
        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        Debug.Log("[Relay] Rejoint allocation via code: " + joinCode);
        return allocation;
    }

    public static void StartHost(Allocation allocation)
    {
        var endpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == "dtls");
        var relayData = new Unity.Networking.Transport.Relay.RelayServerData(
            endpoint.Host,
            (ushort)endpoint.Port,
            allocation.AllocationIdBytes,
            allocation.ConnectionData,
            allocation.ConnectionData, // pour host, les deux sont identiques
            allocation.Key,
            endpoint.Secure
        );

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(relayData);
        NetworkManager.Singleton.StartHost();
        Debug.Log("?? Host lancé via Relay.");
    }

    public static void StartClient(JoinAllocation allocation)
    {
        var endpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == "dtls");
        var relayData = new Unity.Networking.Transport.Relay.RelayServerData(
            endpoint.Host,
            (ushort)endpoint.Port,
            allocation.AllocationIdBytes,
            allocation.ConnectionData,
            allocation.HostConnectionData,
            allocation.Key,
            endpoint.Secure
        );

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(relayData);
        NetworkManager.Singleton.StartClient();
        Debug.Log("?? Client connecté via Relay.");
    }
}