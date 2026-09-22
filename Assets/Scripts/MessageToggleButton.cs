using UnityEngine;
using UnityEngine.EventSystems;

public class MessageToggleButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private MessageSource type;
    [SerializeField] private ButtonOnOffController onOffController;

    public void OnPointerClick(PointerEventData eventData)
    {
        bool newValue = Settings.Instance.ToggleMessageTypeVisability(type);
        onOffController.SetOnOff(newValue);
    }

    private void Start() => onOffController.SetOnOff(Settings.Instance.GetMessageVisability(type));
}
