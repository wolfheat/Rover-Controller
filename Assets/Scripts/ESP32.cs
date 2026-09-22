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

    private void Update()
    {
        // Use this to send the commands to Firebase from a queue system
    }


    public void SendCommand(string command)
    {
        //PopupText.Instance.ShowPopup("Sending: "+command);

        // Add the Command End" '\r' = value 13
        ESP32Usb.Instance.SendCommand(command);

    }
}
