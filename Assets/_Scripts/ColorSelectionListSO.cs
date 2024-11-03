using UnityEngine;

[CreateAssetMenu(menuName = "Color Selection/Color Selection List", fileName = "New Color Selection List")]
public class ColorSelectionListSO : ScriptableObject
{
    [field: SerializeField] public ColorSelectionSO[] ColorSelections { get; private set; }

    public int GetColorSelectionIndex(ColorSelectionSO colorSelectionSO)
    {
        for (int i = 0; i < ColorSelections.Length; i++)
        {
            if (ColorSelections[i] == colorSelectionSO)
            {
                return i;
            }
        }

        return -1;
    }
}