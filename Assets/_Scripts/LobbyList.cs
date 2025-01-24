using System;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyList : MonoBehaviour
{
    private const float QUERY_LOBBIES_TIME = 2;
    [SerializeField] private LobbyListItem _lobbyListItemPrefab;

    private void OnEnable()
    {
        InvokeRepeating(nameof(FindLobbies), QUERY_LOBBIES_TIME, QUERY_LOBBIES_TIME);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private async void FindLobbies()
    {
        try
        {
            QueryResponse foundLobbies = await LobbyService.Instance.QueryLobbiesAsync();

            if (this == null)
            {
                return;
            }

            foreach (Transform lobby in transform)
            {
                Destroy(lobby.gameObject);
            }

            foreach (Lobby lobby in foundLobbies.Results)
            {
                LobbyListItem lobbyListItem = Instantiate(_lobbyListItemPrefab, transform);
                lobbyListItem.SetLobby(lobby);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
