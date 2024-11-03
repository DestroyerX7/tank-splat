using UnityEngine;

[CreateAssetMenu(menuName = "Turrets/Turret List", fileName = "New Turret List")]
public class TurretListSO : ScriptableObject
{
    [field: SerializeField] public TurretSO[] Turrets { get; private set; }

    public int GetTurretIndex(TurretSO turretSO)
    {
        for (int i = 0; i < Turrets.Length; i++)
        {
            if (Turrets[i] == turretSO)
            {
                return i;
            }
        }

        return -1;
    }
}
