using UnityEngine;
using UnityEngine.UI;

public class ColorSelectionButton : SelectionButton
{
    [field: SerializeField] public ColorSelectionSO ColorSelectionSO { get; private set; }
    [SerializeField] private GameObject _lockImage;

    private void Start()
    {
        GetComponent<Image>().color = ColorSelectionSO.Color;
        Button.onClick.AddListener(OnButtonClick);
    }

    public override void Select()
    {
        base.Select();
        _lockImage.SetActive(true);
    }

    public override void Unselect()
    {
        base.Unselect();
        _lockImage.SetActive(false);
    }

    private void OnButtonClick()
    {
        GameSetupManager.Instance.SelectColor(this);
    }
}
