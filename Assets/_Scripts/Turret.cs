using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerInput))]
public class Turret : NetworkBehaviour
{
    [SerializeField] private TurretSO _turretSO;

    private PlayerInput _playerControls;
    private InputAction _shootAction;

    private float _currentSplatterDistance;
    private float _currentChargeTime;
    private float _shootTimer;

    private Animator _anim;

    [SerializeField] private NetworkObject _bulletPrefab;
    [SerializeField] private Transform[] _shootPositions;
    [SerializeField] private Slider _chargeTimeIndicator;

    private AudioSource _audioSource;

    public override void OnNetworkSpawn()
    {
        SetColor();

        _audioSource = GetComponent<AudioSource>();

        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        _playerControls = GetComponent<PlayerInput>();
        _shootAction = _playerControls.actions["Shoot"];

        _anim = GetComponent<Animator>();

        _chargeTimeIndicator.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (_shootTimer >= 0)
        {
            _shootTimer -= Time.deltaTime;
        }

        float chargeTimePercent = _turretSO.FullChargeTime > 0 ? _currentChargeTime / _turretSO.FullChargeTime : 1;
        _currentSplatterDistance = Mathf.Lerp(0, _turretSO.MaxSplatterDistance, chargeTimePercent);
        _chargeTimeIndicator.value = chargeTimePercent;

        if (_shootTimer <= 0 && _shootAction.inProgress && _currentChargeTime < _turretSO.FullChargeTime)
        {
            _currentChargeTime += Time.deltaTime;
        }
        else if (_shootTimer <= 0 && _shootAction.WasReleasedThisFrame())
        {
            Shoot();
        }

        Aim();
    }

    private void Aim()
    {
        Vector2 mouseScreenPos = Input.mousePosition;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 aimDirection = mouseWorldPos - (Vector2)transform.position;
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.Euler(0, 0, aimAngle);
    }

    private void Shoot()
    {
        BulletData[] bulletDatas = new BulletData[_shootPositions.Length];

        for (int i = 0; i < _shootPositions.Length; i++)
        {
            Transform shootPos = _shootPositions[i];

            NetworkObject bullet = Instantiate(_bulletPrefab, shootPos.position, shootPos.rotation);
            Vector2 velocity = shootPos.up * _turretSO.ShootSpeed;
            bullet.GetComponent<Rigidbody2D>().linearVelocity = velocity;
            bullet.GetComponent<Bullet>().SetSplatterDistance(shootPos.position, _currentSplatterDistance);
            bullet.GetComponent<Bullet>().SetOwnerClientId(OwnerClientId);

            bulletDatas[i] = new BulletData()
            {
                Pos = shootPos.position,
                Rotation = shootPos.rotation,
                Velocity = velocity,
                SplatterDistance = _currentSplatterDistance,
            };
        }

        _audioSource.PlayOneShot(_turretSO.ShootSound);
        _anim.SetTrigger("Shoot");
        _currentChargeTime = 0;
        _shootTimer = _turretSO.TimeBetweenShots;

        int shootTick = NetworkManager.Singleton.ServerTime.Tick;
        ShootServerRpc(bulletDatas, shootTick, OwnerClientId);
    }

    [ServerRpc]
    private void ShootServerRpc(BulletData[] bulletDatas, int shotTick, ulong ownerClientId)
    {
        ShootClientRpc(bulletDatas, shotTick, ownerClientId);
    }

    [ClientRpc]
    private void ShootClientRpc(BulletData[] bulletDatas, int shotTick, ulong ownerClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == ownerClientId)
        {
            return;
        }

        foreach (BulletData bulletData in bulletDatas)
        {
            float currentTick = NetworkManager.Singleton.ServerTime.Tick;
            float timeDifference = (float)(currentTick - shotTick) / NetworkManager.Singleton.LocalTime.TickRate;
            NetworkObject bullet = Instantiate(_bulletPrefab, bulletData.Pos, bulletData.Rotation);
            bullet.GetComponent<Rigidbody2D>().linearVelocity = bulletData.Velocity;
            bullet.GetComponent<Bullet>().SetSplatterDistance(bulletData.Pos, bulletData.SplatterDistance);
            bullet.GetComponent<Bullet>().SetOwnerClientId(ownerClientId);

            Vector2 spawnPos = bullet.GetComponent<Bullet>().CalculatePosAfterTime(bulletData.Pos, bulletData.Velocity, timeDifference);
            bullet.GetComponent<Bullet>().SetPos(spawnPos);
        }

        _audioSource.PlayOneShot(_turretSO.ShootSound);
    }

    public void SetColor()
    {
        ColorSelectionSO colorSelectionSO = GameManager.Instance.GetColorSelectionSO(OwnerClientId);

        if (colorSelectionSO == null)
        {
            return;
        }

        GetComponent<SpriteRenderer>().material.SetFloat("_HueOffset", colorSelectionSO.HueOffset);
    }

    private struct BulletData : INetworkSerializable
    {
        public Vector2 Velocity;
        public Vector2 Pos;
        public Quaternion Rotation;
        public float SplatterDistance;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref Pos);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref SplatterDistance);
        }
    }
}
