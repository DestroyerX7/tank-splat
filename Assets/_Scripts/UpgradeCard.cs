using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // private void Update()
    // {
    //     transform.LookAt(Input.mousePosition + Vector3.back * 1000);
    // }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }
}
