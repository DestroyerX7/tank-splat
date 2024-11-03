using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartGameButton : UITooltipHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Button _button;

    private void Start()
    {
        _button = GetComponent<Button>();
    }

    public new void OnPointerEnter(PointerEventData eventData)
    {
        _tooltip.SetActive(!_button.interactable);
    }

    public new void OnPointerExit(PointerEventData eventData)
    {
        _tooltip.SetActive(false);
    }
}
