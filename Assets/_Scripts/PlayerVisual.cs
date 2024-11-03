using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private int _playerIndex = -1;
    [SerializeField] private TextMeshProUGUI _usernameText;
    [SerializeField] private TextMeshProUGUI _isReadyText;
    [SerializeField] private Image _turretImage;
    [SerializeField] private Material _materialPrefab;
    private Image _tankBodyImage;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _tankBodyImage = GetComponent<Image>();

        Material material = Instantiate(_materialPrefab);
        _turretImage.material = material;
        _tankBodyImage.material = material;
    }

    public void KickPlayer()
    {
        GameSetupManager.Instance.KickPlayer(_playerIndex);
    }

    public void AllowPlayerKick(bool value)
    {
        _button.enabled = value;
    }

    public void UpdateVisual(PlayerLobbyData playerCutomizationData)
    {
        _usernameText.text = playerCutomizationData.Username.ToString();
        _isReadyText.text = playerCutomizationData.IsReady ? "Ready" : "Not Ready";

        Sprite turretSprite = GameSetupManager.Instance.GetTurretByIndex(playerCutomizationData.TurretIndex).DefaultSprite;
        _turretImage.sprite = turretSprite;
        _turretImage.preserveAspect = true;

        float hueOffset = GameSetupManager.Instance.GetColorSelectionByIndex(playerCutomizationData.ColorSelectionIndex).HueOffset;
        _turretImage.material.SetFloat("_HueOffset", hueOffset);

        Sprite tankBodySprite = GameSetupManager.Instance.GetTankBodyByIndex(playerCutomizationData.TankBodyIndex).DefaultSprite;
        _tankBodyImage.sprite = tankBodySprite;
        _tankBodyImage.preserveAspect = true;
    }
}
