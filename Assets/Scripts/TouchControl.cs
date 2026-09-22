using UnityEngine;
using UnityEngine.EventSystems;

public enum TouchButtonType {DRIVE, LEFT, RIGHT, BACK, STATIC}

public class TouchControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TouchButtonType type;
    [SerializeField] private ButtonOnOffController onOffCOntroller;

    public void OnPointerDown(PointerEventData eventData)
    {
        onOffCOntroller.SetOnOff(true);
        RoverController.Instance.MotorButtonPressed(type);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onOffCOntroller.SetOnOff(false);
        RoverController.Instance.MotorButtonReleased(type);
    }
}
