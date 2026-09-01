using UnityEngine;

public class ESP32 : MonoBehaviour
{


    public static ESP32 Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }



    public void SendCommand(string command)
    {
        //PopupText.Instance.ShowPopup("Sending: "+command);

        // Add the Command End" '\r' = value 13
        ESP32Usb.Instance.SendCommand(command);

    }
}
