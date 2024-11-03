using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : NetworkBehaviour
{
    private PlayerInput _playerControls;
    private InputAction _moveAction;

    [field: SerializeField] public TankBodySO TankBodySO { get; private set; }

    private Rigidbody2D _rb;
    private float _speedMultiplier;

    [SerializeField] private SpriteRenderer _playerHalo;
    [SerializeField] private TextMeshProUGUI _usernameText;

    private Animator _anim;

    public override void OnNetworkSpawn()
    {
        SetColor();

        if (!IsOwner)
        {
            ShowUsernameText();
            enabled = false;
            return;
        }

        _playerControls = GetComponent<PlayerInput>();
        _moveAction = _playerControls.actions["Move"];

        _rb = GetComponent<Rigidbody2D>();

        _anim = GetComponent<Animator>();

        _playerHalo.gameObject.SetActive(true);
        _speedMultiplier = GameManager.Instance.GetPlayerGameData(NetworkManager.Singleton.LocalClientId).SpeedMultiplier;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        _rb.velocity = TankBodySO.MoveSpeed * _speedMultiplier * moveInput.y * transform.up;

        transform.Rotate(TankBodySO.RotateSpeed * _speedMultiplier * moveInput.x * Time.deltaTime * Vector3.back);
    }

    public void SetColor()
    {
        ColorSelectionSO colorSelectionSO = GameManager.Instance.GetColorSelectionSO(OwnerClientId);

        if (colorSelectionSO == null)
        {
            return;
        }

        GetComponent<SpriteRenderer>().material.SetFloat("_HueOffset", colorSelectionSO.HueOffset);
        _playerHalo.material.SetColor("_Color", colorSelectionSO.Color);
    }

    private void ShowUsernameText()
    {
        PlayerGameData playerGameData = GameManager.Instance.GetPlayerGameData(OwnerClientId);
        _usernameText.text = playerGameData.Username.ToString();
        _usernameText.gameObject.SetActive(true);
    }
}
