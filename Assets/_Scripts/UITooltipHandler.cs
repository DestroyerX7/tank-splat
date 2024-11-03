using UnityEngine;
using UnityEngine.EventSystems;

public class UITooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected GameObject _tooltip;
    [SerializeField] private bool _followCursor;
    [SerializeField] private Vector3 _followOffset;

    private void Start()
    {
        _tooltip.SetActive(false);
    }

    private void Update()
    {
        if (_followCursor && _tooltip.activeInHierarchy)
        {
            _tooltip.transform.position = Input.mousePosition + _followOffset;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltip.SetActive(true);
        _tooltip.transform.SetParent(transform.root);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip.SetActive(false);
        _tooltip.transform.SetParent(transform);
    }
}
