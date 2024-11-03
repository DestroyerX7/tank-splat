using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : NetworkBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private GameObject _upgradeCardsContainer;
    [SerializeField] private GameObject _waitingForPlayersToUpgradeText;
    [SerializeField] private Image _turretUpgradeImage;
    private List<ulong> _playersCurrentlyUpgrading = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.OnSceneReset += HideUI;
    }

    public override void OnNetworkDespawn()
    {
        GameManager.Instance.OnSceneReset -= HideUI;
    }

    public bool PlayersShouldUpgrade(int pointsToUpgrade, out ulong[] playersToUpgrade)
    {
        if (!LobbyManager.Instance.LobbyData.AllowUpgrades || GameManager.Instance.PlayerGameDatas.Count == 1)
        {
            playersToUpgrade = new ulong[0];
            return false;
        }

        bool playersShouldUpgrade = false;
        List<ulong> tempPlayersToUpgrade = new();

        foreach (PlayerGameData playerGameData in GameManager.Instance.PlayerGameDatas)
        {
            if (playerGameData.WinPoints >= pointsToUpgrade)
            {
                playersShouldUpgrade = true;
            }
            else
            {
                tempPlayersToUpgrade.Add(playerGameData.ClientId);
            }
        }

        playersToUpgrade = tempPlayersToUpgrade.ToArray();
        return playersShouldUpgrade;
    }

    private void HideUI()
    {
        _upgradeCardsContainer.SetActive(false);
        _waitingForPlayersToUpgradeText.SetActive(false);
    }

    [ClientRpc]
    public void UpgradePlayersClientRpc(ulong[] playersToUpgrade)
    {
        if (IsServer)
        {
            ResetWinPoints();
            _playersCurrentlyUpgrading = playersToUpgrade.ToList();
        }

        if (playersToUpgrade.Contains(NetworkManager.Singleton.LocalClientId))
        {
            _upgradeCardsContainer.SetActive(true);

            TurretSO currentTurretSO = GameManager.Instance.GetTurretSO(NetworkManager.Singleton.LocalClientId);
            if (currentTurretSO.Upgrade != null)
            {
                _turretUpgradeImage.sprite = currentTurretSO.Upgrade.DefaultSprite;
            }
        }
        else
        {
            _waitingForPlayersToUpgradeText.SetActive(true);
        }
    }

    private void ResetWinPoints()
    {
        for (int i = 0; i < GameManager.Instance.PlayerGameDatas.Count; i++)
        {
            PlayerGameData playerGameData = GameManager.Instance.PlayerGameDatas[i];
            playerGameData.WinPoints = 0;
            GameManager.Instance.PlayerGameDatas[i] = playerGameData;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpgradePlayerServerRpc(PlayerGameData playerGameData)
    {
        for (int i = 0; i < GameManager.Instance.PlayerGameDatas.Count; i++)
        {
            if (GameManager.Instance.PlayerGameDatas[i].ClientId == playerGameData.ClientId)
            {
                GameManager.Instance.PlayerGameDatas[i] = playerGameData;
            }
        }

        _playersCurrentlyUpgrading.Remove(playerGameData.ClientId);

        if (_playersCurrentlyUpgrading.Count == 0)
        {
            GameManager.Instance.ResetSceneClientRpc();
        }
    }

    public void ReduceDamageMultiplier()
    {
        _upgradeCardsContainer.SetActive(false);
        _waitingForPlayersToUpgradeText.SetActive(true);

        PlayerGameData playerGameData = GameManager.Instance.GetPlayerGameData(NetworkManager.Singleton.LocalClientId);
        playerGameData.DamageMuliplier -= 0.1f;
        UpgradePlayerServerRpc(playerGameData);
    }

    public void IncreaseSpeedMultiplier()
    {
        _upgradeCardsContainer.SetActive(false);
        _waitingForPlayersToUpgradeText.SetActive(true);

        PlayerGameData playerGameData = GameManager.Instance.GetPlayerGameData(NetworkManager.Singleton.LocalClientId);
        playerGameData.SpeedMultiplier += 0.1f;
        UpgradePlayerServerRpc(playerGameData);
    }

    public void UpgradeTurret()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        TurretSO currentTurretSO = GameManager.Instance.GetTurretSO(clientId);

        if (currentTurretSO.Upgrade == null)
        {
            Debug.LogWarning("Current turret does not have an upgrade");
            return;
        }

        _upgradeCardsContainer.SetActive(false);
        _waitingForPlayersToUpgradeText.SetActive(true);

        int upgradeIndex = GameManager.Instance.GetTurretIndex(currentTurretSO.Upgrade);
        PlayerGameData playerGameData = GameManager.Instance.GetPlayerGameData(clientId);
        playerGameData.TurretIndex = upgradeIndex;
        UpgradePlayerServerRpc(playerGameData);
    }
}
