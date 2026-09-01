using System;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class RoverFirebase : MonoBehaviour
{
    [SerializeField] private GameObject receiverMark; 

    private DatabaseReference commandReference;

    [SerializeField] private ButtonOnOffController firebaseOnOffController;
    public static RoverFirebase Instance { get; private set; }

    public static Action<string> MessageReceived;
    public static Action<string> MessageTransmitted;
    public static Action<float> IsSliderSetting;

    private DatabaseReference connectionReference;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Settings.OnReceiverChange += SetAsReceiver;

        SetFirebaseStatus(false);
    }

    void Start()
    {
        ConnectToFirebase();

        connectionReference.ValueChanged += OnFirebaseConnectionChanged;

        Debug.Log("FIREBASE: Listening for commands");
        SetAsReceiver(Settings.Instance.Receiver);
    }

    public void ConnectToFirebase()
    {
        commandReference = FirebaseDatabase.DefaultInstance.GetReference("rover1/command");
        connectionReference = FirebaseDatabase.DefaultInstance.GetReference(".info/connected");
    }

    private void SetFirebaseStatus(bool connected)
    {
        firebaseOnOffController.SetOnOff(connected);
    }

    private void OnFirebaseConnectionChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) {
            Debug.LogError(
                "FIREBASE: Connection error: " +
                args.DatabaseError.Message
            );

            SetFirebaseStatus(false);
            return;
        }

        bool connected =
            args.Snapshot.Exists &&
            args.Snapshot.Value is bool &&
            (bool)args.Snapshot.Value;

        SetFirebaseStatus(connected);

        Debug.Log(
            "FIREBASE CONNECTION: " +
            (connected ? "CONNECTED" : "DISCONNECTED")
        );
    }
    public void SetAsReceiver(bool set)
    {
        if (set) {
            // Stop Listening to Changes
            if (commandReference != null) {
                commandReference.ValueChanged += OnCommandChanged;
            }
        }
        else {

            if (commandReference != null) {
                commandReference.ValueChanged -= OnCommandChanged;
            }
        }
    }


    // =====================================================
    // =====================================================
    // TRANSMITTOR
    // =====================================================
    // =====================================================

    public void SendCommand(string command)
    {
        PopupText.Instance.ShowPopup("FIREBASE: Sending command: " +command);
        MessageTransmitted?.Invoke(command);
        commandReference
            .SetValueAsync(command)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted) {
                    Debug.LogError(
                        "FIREBASE: Send failed: " +
                        task.Exception
                    );

                    return;
                }

                if (task.IsCanceled) {
                    Debug.LogError(
                        "FIREBASE: Send cancelled"
                    );

                    return;
                }

                Debug.Log(
                    "FIREBASE: Command SUCCESSFUL"
                );
            });
    }

    // =====================================================
    // RECEIVER
    // =====================================================

    private void OnCommandChanged(object sender, ValueChangedEventArgs args)
    {

        PopupText.Instance.LastCommand("Command: " + args.Snapshot.Value?.ToString());


        if (args.DatabaseError != null) {
            Debug.LogError(
                "FIREBASE: RX ERROR: " +
                args.DatabaseError.Message
            );

            return;
        }

        if (!args.Snapshot.Exists) {
            Debug.Log("FIREBASE: No command");
            return;
        }

        string command = args.Snapshot.Value?.ToString();

        // Any command sent unsets the mimic Button - Then maybe sets one back as ON if applicable
        RoverController.Instance.MimicButtonSetting(command);

        MessageReceived?.Invoke(command);

        PopupText.Instance.ShowPopup("FIREBASE: Recieved [" + command + "]");

        Debug.Log("FIREBASE RX: [" + command + "]" );


        // If receiving a Slider setting Update the Slider
        SliderSetting(command);
        /*
        Debug.Log("FIREBASE Slider Complete");
        if (GenericLedCommand(command)) {
            Debug.Log("FIREBASE LED Accepted");
            return;
        }

        Debug.Log("FIREBASE LED Complete");
        */

        // ==========================================
        // SEND COMMAND TO ESP32
        // ==========================================


        ESP32Usb.Instance.SendCommand(command);
        

        // ---------------------------------------------------------
        // Delete command after processing
        // ---------------------------------------------------------

        commandReference
            .RemoveValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted) {
                    Debug.LogError(
                        "FIREBASE: Failed to delete command: " +
                        task.Exception
                    );

                    return;
                }

                if (task.IsCanceled) {
                    Debug.LogError(
                        "FIREBASE: Delete command cancelled"
                    );

                    return;
                }

                Debug.Log(
                    "FIREBASE: Command processed and deleted"
                );
            });
    }

    private bool GenericLedCommand(string command)
    {
        Debug.Log("Checking for command LED: "+command);
        if (command == "LED") {
            RoverController.Instance.LED();
            return true;
        }
        return false;
    }

    private void SliderSetting(string command)
    {        
        if (command.Length > 5 && command.Substring(0, 5) == "DUTY ") {
            if (Int32.TryParse(command.Substring(5), out int duty)) {
                RoverController.Instance.MimicSliderSetting(duty);
                Debug.Log("SliderSetting: "+duty);
            }
            else {
                // Unable to parse duty value
                Debug.Log("SliderSetting: Unable to parse duty value");
            }
        }
    }

    // DESTROY

    private void OnDestroy()
    {
        if (commandReference != null) {
            commandReference.ValueChanged -= OnCommandChanged;
        }
        if (connectionReference != null) {
            connectionReference.ValueChanged -= OnFirebaseConnectionChanged;
        }

        Settings.OnReceiverChange -= SetAsReceiver;
    }
}