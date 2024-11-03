using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TankBodyListSO _tankBodyListSO;
    [SerializeField] private TurretListSO _turretListSO;
    [SerializeField] private ColorSelectionListSO _colorSelectionListSO;

    private readonly List<ulong> _playersAlive = new();
    public NetworkList<PlayerGameData> PlayerGameDatas { get; private set; }

    public event Action OnSceneReset;

    [SerializeField] private GameObject _fireworks;
    [SerializeField] private AudioSource _backgroundMusic;
    [SerializeField] private GameObject _gameMenu;
    [SerializeField] private GameObject _backToLobbyButton;
    [SerializeField] private GameObject _spectatingText;
    [SerializeField] private TextMeshProUGUI _playerWonText;
    [SerializeField] private StartCountdownUI _startCountdownUI;
    [SerializeField] private float _coundownStartTime = 3;
    [SerializeField] private int _pointsToUpgrade = 2;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        PlayerGameDatas = new();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            foreach (PlayerLobbyData playerLobbyData in LobbyManager.Instance.PlayerLobbyDatas)
            {
                PlayerGameData playerGameData = new(playerLobbyData.ClientId, playerLobbyData.Username, playerLobbyData.ColorSelectionIndex, playerLobbyData.TurretIndex, playerLobbyData.TankBodyIndex);

                PlayerGameDatas.Add(playerGameData);
            }

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += AllPlayersLoaded;
            NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayer;
        }

        if (IsClient)
        {
            foreach (PlayerGameData playerGameData in PlayerGameDatas)
            {
                if (playerGameData.ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    _spectatingText.SetActive(false);
                    break;
                }
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= AllPlayersLoaded;
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayer;
        }

        CancelInvoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleGameMenu();
        }
    }

    private void AllPlayersLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        ResetSceneClientRpc();
    }

    private void SpawnPlayers()
    {
        if (!IsServer)
        {
            return;
        }

        _playersAlive.Clear();

        for (int i = 0; i < PlayerGameDatas.Count; i++)
        {
            Vector2 spawnPos = MapManager.Instance.GetSpawnPos(i);
            PlayerGameData playerGameData = PlayerGameDatas[i];

            NetworkObject tankBody = _tankBodyListSO.TankBodies[playerGameData.TankBodyIndex].TankBodyPrefab;
            NetworkObject player = Instantiate(tankBody, spawnPos, Quaternion.identity);
            player.SpawnAsPlayerObject(playerGameData.ClientId, true);

            NetworkObject turretPrefab = _turretListSO.Turrets[playerGameData.TurretIndex].TurretPrefab;
            NetworkObject turret = Instantiate(turretPrefab, spawnPos, Quaternion.identity);
            turret.SpawnWithOwnership(playerGameData.ClientId, true);
            turret.TrySetParent(player.transform);

            _playersAlive.Add(playerGameData.ClientId);
        }
    }

    public void PlayerDied(ulong clientId)
    {
        if (!IsServer || !_playersAlive.Contains(clientId))
        {
            return;
        }

        _playersAlive.Remove(clientId);

        if (_playersAlive.Count == 1)
        {
            ScoreManager.Instance.IncreasePlayerScore(_playersAlive[0]);

            if (ScoreManager.Instance.CheckIfAnyPlayerWon(out PlayerGameData winner))
            {
                PlayerWonClientRpc(winner);
            }
            else if (UpgradeManager.Instance.PlayersShouldUpgrade(_pointsToUpgrade, out ulong[] playersToUpgrade))
            {
                UpgradeManager.Instance.UpgradePlayersClientRpc(playersToUpgrade);
            }
            else
            {
                Invoke(nameof(ResetSceneClientRpc), 1);
            }
        }
    }

    [ClientRpc]
    public void ResetSceneClientRpc()
    {
        ResetScene();
    }

    private async void ResetScene()
    {
        OnSceneReset?.Invoke();
        _backgroundMusic.Stop();

        await _startCountdownUI.WaitForCountdown(_coundownStartTime);
        _backgroundMusic.Play();

        if (IsServer && this != null)
        {
            SpawnPlayers();
        }
    }

    public PlayerGameData GetPlayerGameData(ulong clientId)
    {
        foreach (PlayerGameData playerGameData in PlayerGameDatas)
        {
            if (playerGameData.ClientId == clientId)
            {
                return playerGameData;
            }
        }

        return default;
    }

    public Color32 GetColorByClientId(ulong clientId)
    {
        return GetColorSelectionSO(clientId).Color;
    }

    public ColorSelectionSO GetColorSelectionSO(ulong clientId)
    {
        int colorSelectionIndex = GetPlayerGameData(clientId).ColorSelectionIndex;
        return _colorSelectionListSO.ColorSelections[colorSelectionIndex];
    }

    public TurretSO GetTurretSO(ulong clientId)
    {
        int turretIndex = GetPlayerGameData(clientId).TurretIndex;
        return _turretListSO.Turrets[turretIndex];
    }

    public int GetTurretIndex(TurretSO turretSO)
    {
        return _turretListSO.GetTurretIndex(turretSO);
    }

    [ClientRpc]
    private void PlayerWonClientRpc(PlayerGameData playerGameData)
    {
        _playerWonText.text = playerGameData.Username + " Wins";
        _playerWonText.color = GetColorByClientId(playerGameData.ClientId);
        _playerWonText.gameObject.SetActive(true);
        _playerWonText.GetComponent<Animator>().SetTrigger("FadeIn");
        _fireworks.SetActive(true);

        if (IsServer)
        {
            Invoke(nameof(ShowGameMenu), 3);
        }
    }

    private void ShowGameMenu()
    {
        _gameMenu.SetActive(true);
        _backToLobbyButton.SetActive(IsServer);
    }

    public void BackToLobby()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Game Setup", LoadSceneMode.Single);
    }

    public void LeaveGame()
    {
        NetworkManager.Singleton.Shutdown();
    }

    private void RemovePlayer(ulong clientId)
    {
        foreach (PlayerGameData playerGameData in PlayerGameDatas)
        {
            if (playerGameData.ClientId == clientId)
            {
                PlayerGameDatas.Remove(playerGameData);
            }
        }

        PlayerDied(clientId);
    }

    private void ToggleGameMenu()
    {
        _gameMenu.SetActive(!_gameMenu.activeInHierarchy);
        _backToLobbyButton.SetActive(IsServer);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        {
            EditorApplication.isPlaying = false;
        }
#else
        {
            Application.Quit();
        }
#endif
    }
}
