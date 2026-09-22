using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ESPButtonUpdater : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ButtonOnOffController espOnOffController;

    private void OnEnable()
    {
        ESP32Usb.UpdateESPButton += OnESPUpdate;
    }
    
    private void OnDisable()
    {
        ESP32Usb.UpdateESPButton -= OnESPUpdate;
    }

    private void OnESPUpdate(bool set)
    {
        espOnOffController.SetOnOff(set);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Find any ESP target
        ESP32Usb.Instance.Connect();
    }
}
