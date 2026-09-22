using System;
using System.Collections;
using UnityEngine;

public class RoverController : MonoBehaviour
{
    [SerializeField] private ButtonOnOffController ledButton;
    [SerializeField] private ButtonOnOffController ledButtonBackground;

    [SerializeField] private ButtonOnOffController ledHeadButton;
    [SerializeField] private ButtonOnOffController ledHeadButtonBackground;

    [SerializeField] private ButtonOnOffController ledRearButton;
    [SerializeField] private ButtonOnOffController ledRearButtonBackground;
    
    [SerializeField] private ButtonOnOffController ledScannerButton;
    [SerializeField] private ButtonOnOffController ledScannerButtonBackground;

    [SerializeField] private ButtonOnOffController motorButton;
    [SerializeField] private ButtonOnOffController motorButtonBackground;

    [SerializeField] private ButtonOnOffController stopButton;

    [SerializeField] private ButtonOnOffController motorDriveButton;
    [SerializeField] private ButtonOnOffController motorRightButton;
    [SerializeField] private ButtonOnOffController motorLeftButton;
    [SerializeField] private ButtonOnOffController motorBackButton;


    private bool ledOn = false;
    private bool ledHeadOn = false;
    private bool ledRearOn = false;
    private bool ledScannerOn = false;


    private bool ledMotor = false;
    private bool ledStop = false;

    private bool[] directions = new bool[4];

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

        // Lights on
        ledButton.SetOnOff(ledOn);
        ledButtonBackground.SetOnOff(ledOn);

        // HeadLights
        ledHeadButton.SetOnOff(ledHeadOn);
        ledHeadButtonBackground.SetOnOff(ledHeadOn);

        // Rear/Brake-Lights
        ledRearButton.SetOnOff(ledRearOn);
        ledRearButtonBackground.SetOnOff(ledRearOn);
                
        // Scanner-Lights
        ledScannerButton.SetOnOff(ledScannerOn);
        ledScannerButtonBackground.SetOnOff(ledScannerOn);


        // Motor
        motorButton.SetOnOff(ledMotor);
        motorButtonBackground.SetOnOff(ledMotor);

        // Stop
        stopButton.SetOnOff(ledStop);
    }


    // ON OFF STUFF
    public void LED()
    {
        ledOn = !ledOn;
        Debug.Log("SETTING LED: " + ledOn);
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

    public void LED_HEAD()
    {
        ledHeadOn = !ledHeadOn;
        Debug.Log("SETTING LED_HEAD: " + ledHeadOn);
        string command = "LED_HEAD " + (ledHeadOn ? "ON" : "OFF");
        if (!Settings.Instance.Receiver) {
            // Call the Transmittor instead
            RoverFirebase.Instance.SendCommand(command);
        }
        else {
            ESP32.Instance.SendCommand(command);
        }
        UpdateButtons();
    }

    public void LED_REAR()
    {
        ledRearOn = !ledRearOn;
        Debug.Log("SETTING LED_REAR: " + ledRearOn);
        string command = "LED_REAR " + (ledRearOn ? "ON" : "OFF");
        if (!Settings.Instance.Receiver) {
            // Call the Transmittor instead
            RoverFirebase.Instance.SendCommand(command);
        }
        else {
            ESP32.Instance.SendCommand(command);
        }
        UpdateButtons();
    }
    
    public void LED_SCANNER()
    {
        ledScannerOn = !ledScannerOn;
        Debug.Log("SETTING LED_SCAN: " + ledScannerOn);
        string command = "LED_SCAN " + (ledScannerOn ? "ON" : "OFF");
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
    public void MimicSliderSetting(int duty) => SliderController.Instance.SetDutyValue(duty);
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
    
    public void SetSpeed(int value)
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
            Debug.Log("RoverFirebase.Instance = "+ RoverFirebase.Instance);
            RoverFirebase.Instance.SendCommand(command);
            return;
        }
        Debug.Log("Motor Command: "+command);
        ESP32.Instance.SendCommand(command);

        UpdateButtons();

    }

    private float timer = 0;

    private const float HoldTime = 0.5f; // Twice Each second

    private void Update()
    {
        if(!IsAnyDirectionHeld()) return;

        timer -= Time.deltaTime;

        if(timer <= 0) {
            timer = HoldTime;
            // Send the Data

            // Keep Sending the held buttons to the realtime database 
            StartCoroutine(SendHeldButtonsAsCommand());
        }
    }

    private IEnumerator SendHeldButtonsAsCommand()
    {
        if (directions[0]) {
            SendCommand("MOTOR DRIVE");
            yield return null;
            yield return null;
            yield return null;
        }
        if (directions[1]) {
            SendCommand("MOTOR LEFT");
            yield return null;
            yield return null;
            yield return null;
        }
        if (directions[2]) {
            SendCommand("MOTOR RIGHT");
            yield return null;
            yield return null;
            yield return null;
        }
        if (directions[3]) {
            SendCommand("MOTOR BACK");
        }
        yield return null;
    }

    // General Motor TOUCH Buttons
    public void MotorButtonPressed(TouchButtonType type)
    {
        if(type == TouchButtonType.STATIC) {
            // Force off
            TurnAllDirectionsOff();
        }
        else {
            // An input was sent so not stopped anymore
            ledStop = false;
            directions[(int)type] = true;
            timer = 0; // Forces send of all values
        }
        // SendCommand("MOTOR "+ type.ToString());
    }

    private void TurnAllDirectionsOff()
    {
        for (int i = 0; i < 4; i++) {
            directions[i] = false;
        }
        SendCommand("MOTOR STATIC");
    }

    public void MotorButtonReleased(TouchButtonType type)
    {
        ledStop = true;
        directions[(int)type] = false;

        // Only Send Static if none of the direction is ON - This should handle switching before releasing a button not working
        if(!IsAnyDirectionHeld())
            SendCommand("MOTOR STATIC");
        else {
            // remove just this button
            SendCommand("MOTOR "+type+"_OFF");
        }
    }

    private bool IsAnyDirectionHeld()
    {
        for (int i = 0; i < 4; i++) 
        {
            if (directions[i])
                return true;
        }
        return false;
    }
}
