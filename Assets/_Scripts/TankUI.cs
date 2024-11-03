using UnityEngine;

public class TankUI : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}
