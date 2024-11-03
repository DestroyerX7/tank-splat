using Unity.Netcode;
using UnityEngine;

public class PaintSplatter : NetworkBehaviour
{
    [SerializeField] private PaintSplatterListSO _paintSplatterListSO;
    [SerializeField] private float _despawnTime;
    [SerializeField] private float _damagePerSecond;
    [SerializeField] private AudioSource _audioSource;

    public override void OnNetworkSpawn()
    {
        _audioSource = GetComponent<AudioSource>();
        AudioClip splatterSound = _paintSplatterListSO.GetRandAudioClip();
        _audioSource.PlayOneShot(splatterSound);

        if (!IsServer)
        {
            return;
        }

        int randSpriteIndex = _paintSplatterListSO.GetRandSpriteIndex();
        Quaternion randRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        SetSpriteAndRotationClientRpc(randSpriteIndex, randRotation);
        Invoke(nameof(Despawn), _despawnTime);
    }

    public override void OnNetworkDespawn()
    {
        CancelInvoke();
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    public override void OnDestroy()
    {
        CancelInvoke();

        base.OnDestroy();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsServer)
        {
            return;
        }

        if (other.TryGetComponent(out NetworkObject networkObject) && networkObject.OwnerClientId == OwnerClientId)
        {
            return;
        }

        other.GetComponent<TankHealth>()?.TakeDamage(_damagePerSecond * Time.deltaTime);
    }

    [ClientRpc]
    private void SetSpriteAndRotationClientRpc(int spriteIndex, Quaternion rotation)
    {
        GetComponent<SpriteRenderer>().sprite = _paintSplatterListSO.PaintSplatterSprites[spriteIndex];
        GetComponent<SpriteRenderer>().color = GameManager.Instance.GetColorByClientId(OwnerClientId);

        // Alternatively you could make the prefab not have a PolygonCollider2D and just add it when you set the sprite
        // That method may not work for pooling tho
        Destroy(GetComponent<PolygonCollider2D>());
        PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;

        transform.rotation = rotation;
    }

    private void Despawn()
    {
        NetworkObject.Despawn();
    }
}
