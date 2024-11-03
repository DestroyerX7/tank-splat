using UnityEngine;

public class TurretSelectionButton : SelectionButton
{
    [field: SerializeField] public TurretSO TurretSO { get; private set; }

    private void Start()
    {
        Button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        GameSetupManager.Instance.SelectTurret(this);
    }
}
