using UnityEngine;
using UnityEngine.UI;

public class SelectionButton : MonoBehaviour
{
    protected Button Button;

    public void Awake()
    {
        Button = GetComponent<Button>();
    }

    public virtual void Select()
    {
        Button.interactable = false;
    }

    public virtual void Unselect()
    {
        Button.interactable = true;
    }
}
