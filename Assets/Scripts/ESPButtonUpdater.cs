using System;
using UnityEngine;

public class ESPButtonUpdater : MonoBehaviour
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
}
