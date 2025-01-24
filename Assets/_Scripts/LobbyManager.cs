using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Networking.Transport.Relay;

[DisallowMultipleComponent]
public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    private string _playerId;
    public Lobby JoinedLobby { get; private set; }

    private const int MAX_PLAYERS = 4;
    private const float LOBBY_HEARTBEAT_TIME = 15;
    private const string RELAY_JOIN_CODE_KEY = "Relay Join Code";
    private const string RELAY_CONNECTION_TYPE = "dtls";

    public readonly List<PlayerLobbyData> PlayerLobbyDatas = new(); // Temp?
    public LobbyData LobbyData; // Temp?
    public string Username { get; private set; } // Temp?

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitAndSignIn();

        InvokeRepeating(nameof(SendLobbyHeartbeat), LOBBY_HEARTBEAT_TIME, LOBBY_HEARTBEAT_TIME);

        // Currently not ever unsubscribing, maybe do that later
        // Same with SignedIn but I dont think any tutorial unsubscribes from that either
        NetworkManager.Singleton.OnClientStopped += LeaveLobby;

        Username = "Player " + UnityEngine.Random.Range(0, 10000).ToString("0000");
        MainMenuManager.Instance.UpdateUsernameText();
    }

    private async void InitAndSignIn()
    {
        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                _playerId = AuthenticationService.Instance.PlayerId;
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate = false)
    {
        lobbyName = lobbyName.Trim();
        if (string.IsNullOrEmpty(lobbyName))
        {
            return;
        }

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);
            string allocationJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            CreateLobbyOptions options = new()
            {
                Data = new()
                {
                    { RELAY_JOIN_CODE_KEY, new(DataObject.VisibilityOptions.Public, allocationJoinCode) }
                },
                IsPrivate = isPrivate
            };
            JoinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYERS, options);

            RelayServerData relayServerData = allocation.ToRelayServerData(RELAY_CONNECTION_TYPE);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("Game Setup", LoadSceneMode.Single);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinLobbyById(string lobbyId)
    {
        if (string.IsNullOrEmpty(lobbyId))
        {
            return;
        }

        try
        {
            JoinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(JoinedLobby.Data[RELAY_JOIN_CODE_KEY].Value);

            RelayServerData relayServerData = joinAllocation.ToRelayServerData(RELAY_CONNECTION_TYPE);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        if (string.IsNullOrEmpty(lobbyCode))
        {
            return;
        }

        try
        {
            JoinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(JoinedLobby.Data[RELAY_JOIN_CODE_KEY].Value);

            RelayServerData relayServerData = joinAllocation.ToRelayServerData(RELAY_CONNECTION_TYPE);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private async void SendLobbyHeartbeat()
    {
        if (JoinedLobby == null || JoinedLobby.HostId != _playerId)
        {
            return;
        }

        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(JoinedLobby.Id);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private async void LeaveLobby(bool isHost)
    {
        try
        {
            if (JoinedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, _playerId);
                JoinedLobby = null;
            }

            if (this != null)
            {
                SceneManager.LoadScene("Main Menu");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void SetUsername(string username)
    {
        username = username.Trim();
        if (string.IsNullOrEmpty(username) || username.Length > 15)
        {
            return;
        }

        Username = username;
    }
}
