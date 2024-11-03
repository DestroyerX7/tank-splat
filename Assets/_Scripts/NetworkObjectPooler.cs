using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
public class NetworkObjectPooler : NetworkBehaviour
{
    [Serializable]
    private struct PoolData
    {
        public NetworkObject PooledObjectPrefab;
        public int InitialCount;
    }

    private class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        private readonly NetworkObject _prefab;
        private readonly NetworkObjectPooler _networkObjectPooler;

        public PooledPrefabInstanceHandler(NetworkObject prefab, NetworkObjectPooler networkObjectPooler)
        {
            _prefab = prefab;
            _networkObjectPooler = networkObjectPooler;
        }

        NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return _networkObjectPooler.GetPooledObject(_prefab, position, rotation);
        }

        void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
        {
            // if (networkObject.IsSpawned)
            // {
            //     networkObject.Despawn();
            //     return;
            // }

            _networkObjectPooler.ReturnPooledObject(_prefab, networkObject);
        }
    }

    public static NetworkObjectPooler Instance { get; private set; }

    [SerializeField] private List<PoolData> _poolDatas;
    private readonly HashSet<NetworkObject> _pooledObjectPrefabs = new();
    private readonly Dictionary<NetworkObject, ObjectPool<NetworkObject>> _poolDict = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        foreach (PoolData poolData in _poolDatas)
        {
            SetupPool(poolData);
        }
    }

    public override void OnNetworkDespawn()
    {
        // Unregisters all objects in PooledPrefabsList from the cache.
        foreach (NetworkObject prefab in _pooledObjectPrefabs)
        {
            // Unregister Netcode Spawn handlers
            NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
            _poolDict[prefab].Clear();
        }

        _poolDict.Clear();
        _pooledObjectPrefabs.Clear();
    }

    private void SetupPool(PoolData poolData)
    {
        NetworkObject ActionOnCreate()
        {
            return Instantiate(poolData.PooledObjectPrefab);
        }

        void ActionOnGet(NetworkObject pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }

        void ActionOnRelease(NetworkObject pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
        }

        void ActionOnDestroy(NetworkObject pooledObject)
        {
            Destroy(pooledObject);
        }

        ObjectPool<NetworkObject> objectPool = new(ActionOnCreate, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: poolData.InitialCount);
        _poolDict.Add(poolData.PooledObjectPrefab, objectPool);
        _pooledObjectPrefabs.Add(poolData.PooledObjectPrefab);

        List<NetworkObject> pooledObjects = new();
        for (int i = 0; i < poolData.InitialCount; i++)
        {
            NetworkObject pooledObject = _poolDict[poolData.PooledObjectPrefab].Get();
            pooledObjects.Add(pooledObject);
        }

        foreach (NetworkObject pooledObject in pooledObjects)
        {
            _poolDict[poolData.PooledObjectPrefab].Release(pooledObject);
        }

        // Register Netcode Spawn handlers
        PooledPrefabInstanceHandler pooledPrefabInstanceHandler = new(poolData.PooledObjectPrefab, this);
        NetworkManager.Singleton.PrefabHandler.AddHandler(poolData.PooledObjectPrefab, pooledPrefabInstanceHandler);
    }

    public NetworkObject GetPooledObject(NetworkObject pooledObjectPrefab, Vector3 pos, Quaternion rotation)
    {
        NetworkObject pooledObject = _poolDict[pooledObjectPrefab].Get();
        pooledObject.transform.SetPositionAndRotation(pos, rotation);
        return pooledObject;
    }

    public void ReturnPooledObject(NetworkObject pooledObjectPrefab, NetworkObject pooledObject)
    {
        _poolDict[pooledObjectPrefab].Release(pooledObject);
    }
}
