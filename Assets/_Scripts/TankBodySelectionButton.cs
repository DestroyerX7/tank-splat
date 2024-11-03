using UnityEngine;
using UnityEngine.UI;

public class TankBodySelectionButton : SelectionButton
{
    [field: SerializeField] public TankBodySO TankBodySO { get; private set; }

    private void Start()
    {
        Button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        GameSetupManager.Instance.SelectTankBody(this);
    }
}
