using UnityEngine;

[CreateAssetMenu(fileName = "TankBodyListSO", menuName = "TankBody/TankBodyListSO")]
public class TankBodyListSO : ScriptableObject
{
    [field: SerializeField] public TankBodySO[] TankBodies { get; private set; }

    public int GetTankBodyIndex(TankBodySO tankBodySO)
    {
        for (int i = 0; i < TankBodies.Length; i++)
        {
            if (TankBodies[i] == tankBodySO)
            {
                return i;
            }
        }

        return -1;
    }
}
