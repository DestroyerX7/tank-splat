using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Turrets/Turret", fileName = "New Turret")]
public class TurretSO : ScriptableObject
{
    [field: SerializeField] public float ShootSpeed { get; private set; }
    [field: SerializeField] public float MaxSplatterDistance { get; private set; }
    [field: SerializeField] public float FullChargeTime { get; private set; }
    [field: SerializeField] public float FireRate { get; private set; }
    public float TimeBetweenShots => 1f / (FireRate / 60f);
    [field: SerializeField] public TurretSO Upgrade { get; private set; }
    [field: SerializeField] public NetworkObject TurretPrefab { get; private set; }
    [field: SerializeField] public Sprite DefaultSprite { get; private set; }
    [field: SerializeField] public AudioClip ShootSound { get; private set; }
}
