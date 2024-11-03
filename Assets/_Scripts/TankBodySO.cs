using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "TankBodySO", menuName = "TankBody/TankBodySO")]
public class TankBodySO : ScriptableObject
{
    [field: SerializeField] public float MoveSpeed { get; private set; }
    [field: SerializeField] public float RotateSpeed { get; private set; }
    [field: SerializeField] public float MaxHealth { get; private set; }
    [field: SerializeField] public NetworkObject TankBodyPrefab { get; private set; }
    [field: SerializeField] public Sprite DefaultSprite { get; private set; }
}
