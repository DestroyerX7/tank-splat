using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class TankHealth : NetworkBehaviour
{
    [SerializeField] private Image _healthBar;
    [SerializeField] private GameObject _explosionPrefab;

    private float _maxHealth;
    private readonly NetworkVariable<float> _currentHealth = new();

    private float _damageMultiplier;

    [SerializeField] private AudioClip _explosionSound;

    public override void OnNetworkSpawn()
    {
        _maxHealth = GetComponent<TankController>().TankBodySO.MaxHealth;

        if (IsServer)
        {
            _damageMultiplier = GameManager.Instance.GetPlayerGameData(OwnerClientId).DamageMuliplier;
            _currentHealth.Value = _maxHealth;
        }
    }

    private void Update()
    {
        _healthBar.fillAmount = _currentHealth.Value / _maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer || _currentHealth.Value <= 0)
        {
            return;
        }

        _currentHealth.Value -= damage * _damageMultiplier;

        if (_currentHealth.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!IsServer)
        {
            return;
        }

        ExplodeClientRpc();
        Despawn();
        GameManager.Instance.PlayerDied(OwnerClientId);
    }

    private void Despawn()
    {
        NetworkObject[] networkObjects = GetComponentsInChildren<NetworkObject>();

        for (int i = networkObjects.Length - 1; i >= 0; i--)
        {
            networkObjects[i].Despawn();
        }
    }

    [ClientRpc]
    private void ExplodeClientRpc()
    {
        GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        AudioSource audioSource = explosion.AddComponent<AudioSource>();
        audioSource.PlayOneShot(_explosionSound);
        Destroy(explosion, 1);
    }
}
