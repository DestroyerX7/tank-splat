using UnityEngine;

[CreateAssetMenu(menuName = "Maps/Map List", fileName = "New Map List")]
public class MapListSO : ScriptableObject
{
    [field: SerializeField] public MapSO[] Maps { get; private set; }

    public int GetMapIndex(MapSO mapSO)
    {
        for (int i = 0; i < Maps.Length; i++)
        {
            if (Maps[i] == mapSO)
            {
                return i;
            }
        }

        return -1;
    }
}
