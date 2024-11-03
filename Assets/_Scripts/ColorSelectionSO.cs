using UnityEngine;

[CreateAssetMenu(menuName = "Color Selection/Color Selection", fileName = "New Color Selection")]
public class ColorSelectionSO : ScriptableObject
{
    [field: SerializeField] public float HueOffset { get; private set; }
    [field: SerializeField] public Color32 Color { get; private set; }
}