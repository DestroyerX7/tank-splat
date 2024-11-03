using TMPro;
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI[] _playerScoreTexts;

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
        if (IsClient)
        {
            GameManager.Instance.PlayerGameDatas.OnListChanged += UpdatePlayerScores;
            UpdatePlayerScores();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            GameManager.Instance.PlayerGameDatas.OnListChanged -= UpdatePlayerScores;
        }
    }

    private void UpdatePlayerScores(NetworkListEvent<PlayerGameData> changeEvent = default)
    {
        for (int i = 0; i < _playerScoreTexts.Length; i++)
        {
            if (i < GameManager.Instance.PlayerGameDatas.Count)
            {
                PlayerGameData playerGameData = GameManager.Instance.PlayerGameDatas[i];

                _playerScoreTexts[i].gameObject.SetActive(true);
                _playerScoreTexts[i].text = $"{playerGameData.Username} : {playerGameData.Score}";
                _playerScoreTexts[i].color = GameManager.Instance.GetColorByClientId(playerGameData.ClientId);
            }
            else
            {
                _playerScoreTexts[i].gameObject.SetActive(false);
            }
        }
    }

    public void IncreasePlayerScore(ulong clientId)
    {
        for (int i = 0; i < GameManager.Instance.PlayerGameDatas.Count; i++)
        {
            if (GameManager.Instance.PlayerGameDatas[i].ClientId == clientId)
            {
                PlayerGameData playerGameData = GameManager.Instance.PlayerGameDatas[i];
                playerGameData.Score++;
                playerGameData.WinPoints++;
                GameManager.Instance.PlayerGameDatas[i] = playerGameData;
            }
        }
    }

    public bool CheckIfAnyPlayerWon(out PlayerGameData winner)
    {
        foreach (PlayerGameData playerGameData in GameManager.Instance.PlayerGameDatas)
        {
            if (playerGameData.Score >= LobbyManager.Instance.LobbyData.WinScore)
            {
                winner = playerGameData;
                return true;
            }
        }

        winner = new();
        return false;
    }
}
