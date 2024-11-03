using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyListItem : MonoBehaviour
{
    private Lobby _lobby;
    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _playerCountText;

    public void SetLobby(Lobby lobby)
    {
        _lobby = lobby;
        _lobbyNameText.text = _lobby.Name;
        _playerCountText.text = $"{_lobby.Players.Count}/{_lobby.MaxPlayers}";
    }

    public void JoinLobby()
    {
        LobbyManager.Instance.JoinLobbyById(_lobby.Id);
    }
}
