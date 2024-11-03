using UnityEngine;

[CreateAssetMenu(menuName = "Maps/Map", fileName = "New Map")]
public class MapSO : ScriptableObject
{
    [field: SerializeField] public GameObject MapPrefab { get; private set; }
    [field: SerializeField] public float CameraFov { get; private set; }
    [field: SerializeField] public Vector2[] SpawnPositions { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
}
