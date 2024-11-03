using Unity.Netcode;

public class SceneResetListener : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GameManager.Instance.OnSceneReset += Reset;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            GameManager.Instance.OnSceneReset -= Reset;
        }
    }

    private void OnDisable()
    {
        if (IsServer)
        {
            GameManager.Instance.OnSceneReset -= Reset;
        }
    }

    public override void OnDestroy()
    {
        if (IsServer)
        {
            GameManager.Instance.OnSceneReset -= Reset;
        }

        base.OnDestroy();
    }

    protected virtual void Reset()
    {
        NetworkObject[] networkObjects = GetComponentsInChildren<NetworkObject>();

        for (int i = networkObjects.Length - 1; i >= 0; i--)
        {
            networkObjects[i].Despawn();
        }
    }
}
