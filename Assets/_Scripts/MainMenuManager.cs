using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [SerializeField] private TMP_InputField _usernameInputField;
    [SerializeField] private Toggle _privateLobbyToggle;
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _musicVolumeSlider;

    private void Awake()
    {
        if (Instance != null && Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateUsernameText();
    }

    public void CreateLobby(TMP_InputField inputField)
    {
        LobbyManager.Instance.CreateLobby(inputField.text, _privateLobbyToggle.isOn);
    }

    public void JoinLobbyByCode(TMP_InputField inputField)
    {
        LobbyManager.Instance.JoinLobbyByCode(inputField.text);
    }

    public void UpdateUsernameText()
    {
        _usernameInputField.text = LobbyManager.Instance.Username;
    }

    public void TrySetUsername(string username)
    {
        LobbyManager.Instance.SetUsername(username);
        UpdateUsernameText();
    }

    public void LocalPlayer()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Game Setup", LoadSceneMode.Single);
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

    public void SetMasterVolume(float volume)
    {
        SettingsManager.Instance.SetMasterVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        SettingsManager.Instance.SetMusicVolume(volume);
    }
}
