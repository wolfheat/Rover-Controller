using System;
using UnityEngine;
using UnityEngine.UI;

public class RoverController : MonoBehaviour
{
    [SerializeField] private ButtonOnOffController ledButton;
    [SerializeField] private ButtonOnOffController ledButtonBackground;
    [SerializeField] private ButtonOnOffController motorButton;
    [SerializeField] private ButtonOnOffController motorButtonBackground;

    [SerializeField] private ButtonOnOffController stopButton;

    [SerializeField] private ButtonOnOffController motorDriveButton;
    [SerializeField] private ButtonOnOffController motorRightButton;
    [SerializeField] private ButtonOnOffController motorLeftButton;
    [SerializeField] private ButtonOnOffController motorBackButton;

    [SerializeField] private Slider slider;


    private bool ledOn = false;
    private bool ledMotor = false;
    private bool ledStop = false;

    public static RoverController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    private void UpdateButtons()
    {
        // Main Buttons (ON OFF)
        ledButton.SetOnOff(ledOn);
        ledButtonBackground.SetOnOff(ledOn);

        motorButton.SetOnOff(ledMotor);
        motorButtonBackground.SetOnOff(ledMotor);

        stopButton.SetOnOff(ledStop);
    }


    // ON OFF STUFF
    public void LED()
    {
        ledOn = !ledOn;
        Debug.Log("SETTING LED: "+ledOn);
        string command = "LED " + (ledOn ? "ON" : "OFF");
        if (!Settings.Instance.Receiver) {
            // Call the Transmittor instead
            RoverFirebase.Instance.SendCommand(command);
        }
        else {
            ESP32.Instance.SendCommand(command);
        }
        UpdateButtons();
    }

    public void Motor()
    {
        ledMotor = !ledMotor;
        string command = "MOTOR " + (ledMotor ? "ON" : "OFF");

        if (!Settings.Instance.Receiver) {
            // Call the Transmittor instead
            RoverFirebase.Instance.SendCommand(command);
        }
        else {
            ESP32.Instance.SendCommand(command);
        }
        UpdateButtons();
    }

    public void Stop()
    {
        if (!Settings.Instance.Receiver) {
            // Call the Transmittor instead
            RoverFirebase.Instance.SendCommand("MOTOR STATIC");
            return;
        }

        ledStop = true;
        Debug.Log("Motor Stops.");
        ESP32.Instance.SendCommand("MOTOR STATIC");
        UpdateButtons();
    }

    // SETTINGS STUFF

    public void MimicSliderSetting(int duty) => slider.SetValueWithoutNotify(duty/10f);
    public void MimicButtonSetting(string command)
    {
        // Default to not set
        motorDriveButton.SetOnOff(false);
        motorRightButton.SetOnOff(false);
        motorLeftButton.SetOnOff(false);
        motorBackButton.SetOnOff(false);

        switch (command) {
            // PRESSED BUTTONS PART
            case "MOTOR DRIVE":
                motorDriveButton.SetOnOff(true);
                break;
            case "MOTOR RIGHT":
                motorRightButton.SetOnOff(true);
                break;
            case "MOTOR LEFT":
                motorLeftButton.SetOnOff(true);
                break;
            case "MOTOR BACK":
                motorBackButton.SetOnOff(true);
                break;
            // TOGGLE PART
            case "MOTOR ON":
                ledMotor = true;
                break;
            case "MOTOR OFF":
                ledMotor = false;
                break;
            case "MOTOR STATIC":                
                stopButton.SetOnOff(true);
                break;
            case "LED ON":
                ledOn = true;
                break;
            case "LED OFF":
                ledOn = false;
                break;
            default:
                break;
        }
        // Updates all non TOUCH buttons
        UpdateButtons();
    }

    public void ReadSpeed() => SetSpeed((int)(slider.value*10));

    private void SetSpeed(int value)
    {
        if (!Settings.Instance.Receiver) {
            // Call the Transmittor instead
            RoverFirebase.Instance.SendCommand("DUTY "+value);
            return;
        }
        Debug.Log("Changing Duty.");
        ESP32.Instance.SendCommand("DUTY "+value);
        UpdateButtons();
    }

    // CONTROL STUFF
    public void SendCommand(string command)
    {
        if (!Settings.Instance.Receiver) {
            // Call the Transmittor instead
            RoverFirebase.Instance.SendCommand(command);
            return;
        }
        Debug.Log("Motor Command: "+command);
        ESP32.Instance.SendCommand(command);

        UpdateButtons();
    }
    
    // General Motor TOUCH Buttons
    public void MotorButtonPressed(TouchButtonType type)
    {
        // An input was sent so not stopped anymore
        ledStop = false;
        SendCommand("MOTOR "+ type.ToString());
    }

    public void MotorButtonReleased(TouchButtonType type)
    {
        ledStop = true;
        SendCommand("MOTOR STATIC");
    }
}
