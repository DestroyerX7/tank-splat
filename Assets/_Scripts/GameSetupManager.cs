using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameSetupManager : NetworkBehaviour
{
    public static GameSetupManager Instance { get; private set; }

    private NetworkList<PlayerLobbyData> _playerLobbyDatas;
    private PlayerLobbyData _localPlayerLobbyData;
    private readonly NetworkVariable<LobbyData> _lobbyData = new();

    [SerializeField] private TextMeshProUGUI _winScoreText;
    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private Image _readyButtonImage;
    [SerializeField] private Image _mapBackground;

    [SerializeField] private PlayerVisual[] _playerVisuals;

    [Header("Selection Buttons")]
    [SerializeField] private TurretSelectionButton[] _turretSelectionButtons;
    [SerializeField] private ColorSelectionButton[] _colorSelectionButtons;
    [SerializeField] private MapSelectionButton[] _mapSelectionButtons;
    [SerializeField] private TankBodySelectionButton[] _tankBodySelectionButtons;
    [SerializeField] private TurretListSO _turretListSO;
    [SerializeField] private ColorSelectionListSO _colorSelectionListSO;
    [SerializeField] private MapListSO _mapListSO;
    [SerializeField] private TankBodyListSO _tankBodyListSO;

    [Header("Only Host Interactable")]
    [SerializeField] private Button _startButton;
    [SerializeField] private GameObject _mapSelectionButtonsContainer;
    [SerializeField] private TMP_Dropdown _winScoreDropdown;
    [SerializeField] private TextMeshProUGUI _lobbyCodeText;
    [SerializeField] private Toggle _allowUpgradesToggle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _playerLobbyDatas = new();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            _playerLobbyDatas.OnListChanged += UpdatePlayerVisuals;
            _lobbyData.OnValueChanged += UpdateLobbyDataVisuals;
            UpdatePlayerVisuals();
            UpdateLobbyDataVisuals(_lobbyData.Value, _lobbyData.Value);

            _lobbyNameText.text = LobbyManager.Instance.JoinedLobby?.Name ?? "Local Player";

            // Can possibly put these in UpdatePlayerVisuals but idk
            _turretSelectionButtons[0].Select();
            _mapSelectionButtons[0].Select();
            _tankBodySelectionButtons[0].Select();

            _startButton.gameObject.SetActive(IsServer);
            _winScoreDropdown.gameObject.SetActive(IsServer);
            _mapSelectionButtonsContainer.SetActive(IsServer);
            _allowUpgradesToggle.enabled = IsServer;
            _lobbyCodeText.gameObject.SetActive(IsServer);
        }

        // Might need to use IsHost in some places if using dedicated server but idk
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += AddPlayer;
            NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayer;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                AddPlayer(client.ClientId);
            }

            _lobbyData.Value = new(10, true, 0);

            _lobbyCodeText.text = LobbyManager.Instance.JoinedLobby != null ? "Join Code : " + LobbyManager.Instance.JoinedLobby.LobbyCode : "";
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            _playerLobbyDatas.OnListChanged -= UpdatePlayerVisuals;
            _lobbyData.OnValueChanged -= UpdateLobbyDataVisuals;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= AddPlayer;
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayer;
        }
    }

    private void AddPlayer(ulong clientId)
    {
        int colorSelectionIndex = -1;

        // Checks for a color selection index no other player has selected
        for (int i = 0; i < _colorSelectionListSO.ColorSelections.Length; i++)
        {
            bool colorAvaliable = true;

            foreach (PlayerLobbyData playerLobbyData in _playerLobbyDatas)
            {
                if (playerLobbyData.ColorSelectionIndex == i)
                {
                    colorAvaliable = false;
                }
            }

            if (colorAvaliable)
            {
                colorSelectionIndex = i;
                break;
            }
        }

        PlayerLobbyData newPlayerLobbyData = new(clientId, colorSelectionIndex: colorSelectionIndex);
        _playerLobbyDatas.Add(newPlayerLobbyData);
    }

    private void RemovePlayer(ulong clientId)
    {
        foreach (PlayerLobbyData playerLobbyData in _playerLobbyDatas)
        {
            if (playerLobbyData.ClientId == clientId)
            {
                _playerLobbyDatas.Remove(playerLobbyData);
                return;
            }
        }
    }

    private void UpdatePlayerVisuals(NetworkListEvent<PlayerLobbyData> changeEvent = default)
    {
        for (int i = 0; i < _colorSelectionButtons.Length; i++)
        {
            _colorSelectionButtons[i].Unselect();
        }

        for (int i = 0; i < _playerVisuals.Length; i++)
        {
            PlayerVisual playerVisual = _playerVisuals[i];

            if (i < _playerLobbyDatas.Count)
            {
                PlayerLobbyData playerLobbyData = _playerLobbyDatas[i];

                playerVisual.gameObject.SetActive(true);
                playerVisual.AllowPlayerKick(CanKickPlayer(i));
                playerVisual.UpdateVisual(playerLobbyData);

                _colorSelectionButtons[playerLobbyData.ColorSelectionIndex].Select();

                if (playerLobbyData.ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    _localPlayerLobbyData = playerLobbyData;

                    if (playerLobbyData.Username != LobbyManager.Instance.Username)
                    {
                        _localPlayerLobbyData.Username = LobbyManager.Instance.Username;
                        UpdatePlayerLobbyDataServerRpc(_localPlayerLobbyData);
                    }
                }
            }
            else
            {
                playerVisual.gameObject.SetActive(false);
            }
        }

        if (IsServer)
        {
            _startButton.interactable = AllPlayersReady();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerLobbyDataServerRpc(PlayerLobbyData playerCutomizationData)
    {
        for (int i = 0; i < _playerLobbyDatas.Count; i++)
        {
            if (_playerLobbyDatas[i].ClientId == playerCutomizationData.ClientId)
            {
                _playerLobbyDatas[i] = playerCutomizationData;
                return;
            }
        }
    }

    // I think the host might not need to run some this
    private void UpdateLobbyDataVisuals(LobbyData previousValue, LobbyData newValue)
    {
        string scoreText = newValue.WinScore == int.MaxValue ? "Infinity" : newValue.WinScore.ToString();
        _winScoreText.text = "Win Score : " + scoreText;
        _allowUpgradesToggle.SetIsOnWithoutNotify(newValue.AllowUpgrades);
        _mapBackground.sprite = _mapListSO.Maps[newValue.MapIndex].Sprite;
    }

    public void StartGame()
    {
        if (!IsServer)
        {
            return;
        }

        if (!AllPlayersReady())
        {
            return;
        }

        LobbyManager.Instance.LobbyData = _lobbyData.Value;
        LobbyManager.Instance.PlayerLobbyDatas.Clear();
        foreach (PlayerLobbyData playerLobbyData in _playerLobbyDatas)
        {
            LobbyManager.Instance.PlayerLobbyDatas.Add(playerLobbyData);
        }

        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void LeaveLobby()
    {
        NetworkManager.Singleton.Shutdown();
    }

    public void KickPlayer(int playerIndex)
    {
        if (!CanKickPlayer(playerIndex))
        {
            return;
        }

        NetworkManager.Singleton.DisconnectClient(_playerLobbyDatas[playerIndex].ClientId);
    }

    private bool AllPlayersReady()
    {
        foreach (PlayerLobbyData playerLobbyData in _playerLobbyDatas)
        {
            if (!playerLobbyData.IsReady)
            {
                return false;
            }
        }

        return true;
    }

    private bool CanKickPlayer(int playerIndex)
    {
        return IsServer && playerIndex < _playerLobbyDatas.Count && playerIndex >= 0 && _playerLobbyDatas[playerIndex].ClientId != NetworkManager.Singleton.LocalClientId;
    }

    public void ToggleIsReady()
    {
        _localPlayerLobbyData.IsReady = !_localPlayerLobbyData.IsReady;
        _readyButtonImage.color = _localPlayerLobbyData.IsReady ? Color.green : Color.red; // Can possibly put this in UpdatePlayerVisuals but idk
        UpdatePlayerLobbyDataServerRpc(_localPlayerLobbyData);
    }

    public void SelectTurret(TurretSelectionButton turretSelectionButton)
    {
        // Can possibly put this in UpdatePlayerVisuals but idk
        for (int i = 0; i < _turretSelectionButtons.Length; i++)
        {
            if (turretSelectionButton == _turretSelectionButtons[i])
            {
                _turretSelectionButtons[i].Select();
            }
            else
            {
                _turretSelectionButtons[i].Unselect();
            }
        }

        _localPlayerLobbyData.TurretIndex = _turretListSO.GetTurretIndex(turretSelectionButton.TurretSO);
        UpdatePlayerLobbyDataServerRpc(_localPlayerLobbyData);
    }

    public void SelectColor(ColorSelectionButton colorSelectionButton)
    {
        _localPlayerLobbyData.ColorSelectionIndex = _colorSelectionListSO.GetColorSelectionIndex(colorSelectionButton.ColorSelectionSO);
        UpdatePlayerLobbyDataServerRpc(_localPlayerLobbyData);
    }

    // Could just make _turretListSO public so it can do this
    public TurretSO GetTurretByIndex(int index)
    {
        return _turretListSO.Turrets[index];
    }

    // Could just make _colorSelectionListSO public so it can do this
    public ColorSelectionSO GetColorSelectionByIndex(int index)
    {
        return _colorSelectionListSO.ColorSelections[index];
    }

    // Could just make _tuankBodyListSO public so it can do this
    public TankBodySO GetTankBodyByIndex(int index)
    {
        return _tankBodyListSO.TankBodies[index];
    }

    public void SetWinScore(TMP_Dropdown dropdown)
    {
        string selectedOptionText = dropdown.options[dropdown.value].text;
        LobbyData lobbyData = _lobbyData.Value;

        if (int.TryParse(selectedOptionText, out int selectedWinScore))
        {
            lobbyData.WinScore = selectedWinScore;
        }
        else
        {
            lobbyData.WinScore = int.MaxValue;
        }

        _lobbyData.Value = lobbyData;
    }

    public void ToggleAllowUpgrades(bool allowUpgrades)
    {
        if (!IsServer)
        {
            return;
        }

        LobbyData lobbyData = _lobbyData.Value;
        lobbyData.AllowUpgrades = allowUpgrades;
        _lobbyData.Value = lobbyData;
    }

    public void SelectMap(MapSelectionButton mapSelectionButton)
    {
        for (int i = 0; i < _mapSelectionButtons.Length; i++)
        {
            if (mapSelectionButton == _mapSelectionButtons[i])
            {
                _mapSelectionButtons[i].Select();
            }
            else
            {
                _mapSelectionButtons[i].Unselect();
            }
        }

        LobbyData lobbyData = _lobbyData.Value;
        lobbyData.MapIndex = _mapListSO.GetMapIndex(mapSelectionButton.MapSO);
        _lobbyData.Value = lobbyData;
    }

    public void SelectTankBody(TankBodySelectionButton tankBodySelectionButton)
    {
        for (int i = 0; i < _tankBodySelectionButtons.Length; i++)
        {
            if (tankBodySelectionButton == _tankBodySelectionButtons[i])
            {
                _tankBodySelectionButtons[i].Select();
            }
            else
            {
                _tankBodySelectionButtons[i].Unselect();
            }
        }

        _localPlayerLobbyData.TankBodyIndex = _tankBodyListSO.GetTankBodyIndex(tankBodySelectionButton.TankBodySO);
        UpdatePlayerLobbyDataServerRpc(_localPlayerLobbyData);
    }
}
