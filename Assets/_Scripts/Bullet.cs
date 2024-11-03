using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float _hitDamage;
    [SerializeField] private NetworkObject _paintSplatterPrefab;

    private Vector2 _startPos;
    private float _splatterDistance;

    public override void OnNetworkSpawn()
    {
        GetComponent<SpriteRenderer>().color = GameManager.Instance.GetColorByClientId(OwnerClientId);

        if (!IsServer)
        {
            enabled = false;
            return;
        }

        _startPos = transform.position;
    }

    private ulong _ownerClientId;
    private void Start()
    {
        GetComponent<SpriteRenderer>().color = GameManager.Instance.GetColorByClientId(_ownerClientId);
    }

    private void Update()
    {
        if (Vector2.Distance(transform.position, _startPos) >= _splatterDistance)
        {
            Splatter();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out NetworkObject networkObject) && networkObject.OwnerClientId == _ownerClientId)
        {
            return;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            other.GetComponent<TankHealth>()?.TakeDamage(_hitDamage);
        }

        Splatter();
    }

    private void Splatter()
    {
        Destroy(gameObject);

        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        NetworkObject paintSplatter = NetworkObjectPooler.Instance.GetPooledObject(_paintSplatterPrefab, transform.position, Quaternion.identity);
        paintSplatter.SpawnWithOwnership(_ownerClientId, true);
        // NetworkObject.Despawn();
    }

    public void SetSplatterDistance(Vector2 startPos, float splatterDistance)
    {
        _startPos = startPos;
        _splatterDistance = splatterDistance;
    }

    public void SetOwnerClientId(ulong ownerClientId)
    {
        _ownerClientId = ownerClientId;
    }

    public virtual Vector2 CalculatePosAfterTime(Vector2 startPos, Vector2 startVelocity, float time)
    {
        return startPos + startVelocity * time;
    }

    public void SetPos(Vector2 pos)
    {
        transform.position = pos;
    }
}
