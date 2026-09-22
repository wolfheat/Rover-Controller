using System;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class RoverFirebase : MonoBehaviour
{
    public static RoverFirebase Instance { get; private set; }

    public static Action<string> MessageReceived;
    public static Action<string> MessageTransmitted;
    public static Action<float> IsSliderSetting;

    private const string DatabaseUrl =
        "https://rover-controller-44c8b-default-rtdb.europe-west1.firebasedatabase.app";

    private FirebaseDatabase database;

    private DatabaseReference commandsReference;
    private DatabaseReference connectionReference;

    private void Awake()
    {
        
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("RoverFirebase Instance set");

        // Listen for receiver setting changes
        Settings.OnReceiverChange += SetAsReceiver;

        PopupText.Instance.ShowPopup("FB: AWAKE");

        try {
            // This works on the legacy Android/Firebase 9.4 setup.
            Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

            if (app == null) {
                PopupText.Instance.ShowPopup("FB: APP NULL");
                return;
            }

            PopupText.Instance.ShowPopup(
                "FB: APP OK: " + app.Name
            );

            // IMPORTANT:
            // Do NOT use FirebaseDatabase.DefaultInstance here.
#if UNITY_2022_3
            // Legacy Firebase 9.4 path
            database = FirebaseDatabase.GetInstance(app, DatabaseUrl);
#else
            // Newer Firebase path
            database = FirebaseDatabase.DefaultInstance;
#endif

            if (database == null) {
                PopupText.Instance.ShowPopup("FB: DB NULL");
                return;
            }

            PopupText.Instance.ShowPopup("FB: DB INSTANCE OK");

            commandsReference =
                database.GetReference("rover1/commands");

            PopupText.Instance.ShowPopup("FB: COMMAND OK");

            connectionReference =
                database.GetReference(".info/connected");

            PopupText.Instance.ShowPopup("FB: CONNECTION OK");

            Debug.Log("FIREBASE CONNECTED");


        }
        catch (Exception e) {
            PopupText.Instance.ShowPopup(
                "FB INIT EX\n" +
                e.GetType().Name +
                "\n" +
                e.Message
            );

            Debug.LogError(
                "===== FIREBASE INITIALIZATION FAILED =====\n" +
                e
            );

            Exception inner = e.InnerException;
            int level = 0;

            while (inner != null && level < 5) {
                Debug.LogError(
                    "FIREBASE INNER " +
                    level +
                    ":\n" +
                    inner
                );

                inner = inner.InnerException;
                level++;
            }

            return;
        }
    }

    private void Start()
    {

        if (connectionReference == null) {
            Debug.LogError(
                "FIREBASE: connectionReference is null in Start()"
            );

            PopupText.Instance.ShowPopup(
                "FB: NO CONNECTION REF"
            );

            return;
        }

        // Start watching connection status
        connectionReference.ValueChanged += OnFirebaseConnectionChanged;

        Debug.Log("FIREBASE: Listening for connection");

        // Start/stop command listener according to current setting
        SetAsReceiver(Settings.Instance.Receiver);

        Debug.Log("FIREBASE: Listening for commands");
    }

    private void SetFirebaseStatus(bool connected)
    {
        Debug.Log("*** Updating Firebase Status: "+connected);
        FirebaseButtonOnOffController.Instance.SetOnOff(connected);
    }

    private void OnFirebaseConnectionChanged(
        object sender,
        ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) {
            Debug.LogError(
                "FIREBASE: Connection error: " +
                args.DatabaseError.Message
            );

            SetFirebaseStatus(false);

            PopupText.Instance.ShowPopup(
                "FIREBASE ERROR:\n" +
                args.DatabaseError.Message
            );

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

        PopupText.Instance.ShowPopup(
            "FIREBASE: " +
            (connected ? "CONNECTED" : "DISCONNECTED")
        );
    }
    public void SetAsReceiver(bool set)
    {
        Debug.Log("FIREBASE: Receiver setting = " + set); 
        // Firebase commands are now consumed by the 
        // Android relay, not by the Unity application. 
        // 
        // Do not attach a Firebase command listener here.
    }

    /*
    public void SetAsReceiver(bool set)
    {
        if (commandsReference == null) {
            Debug.LogError(
                "FIREBASE: Cannot change receiver state; commandReference is null"
            );

            return;
        }

        if (set) {
            // Listen to commands
            //commandsReference.ValueChanged -= OnCommandChanged;
            //commandsReference.ValueChanged += OnCommandChanged;

            Debug.Log("FIREBASE: Command listener ENABLED");    
        }
        else {
            // Stop listening to commands
            //commandsReference.ValueChanged -= OnCommandChanged;

            Debug.Log("FIREBASE: Command listener DISABLED");
        }
    }

    */

    // =====================================================
    // TRANSMITTER
    // =====================================================

    public void SendCommand(string command)
    {
        if (commandsReference == null) {
            Debug.LogError("FIREBASE: Cannot send command; commandReference is null");
            PopupText.Instance.ShowPopup("FIREBASE: SEND FAILED\nNot connected");
            return;
        }
        if (string.IsNullOrEmpty(command)) { 
            Debug.LogWarning("FIREBASE: Ignoring empty command"); 
            return; 
        }

        MessageTransmitted?.Invoke(command);

        // Create a NEW Firebase child for every command.
        DatabaseReference newCommand = commandsReference.Push();
        string commandId = newCommand.Key;
        if (string.IsNullOrEmpty(commandId)) { 
            Debug.LogError("FIREBASE: Failed to generate command ID"); 
            return; 
        }
        var commandData = new System.Collections.Generic.Dictionary<string, object> { { "command", command }, { "timestamp", ServerValue.Timestamp } };

        newCommand.SetValueAsync(commandData).ContinueWithOnMainThread(task => 
        { 
            if (task.IsFaulted) { 
                Debug.LogError("FIREBASE: Send failed: " + task.Exception); 
                MessageReceived?.Invoke("FIREBASE SEND ERROR:\n" + task.Exception); 
                return; 
            } 
            if (task.IsCanceled) { 
                Debug.LogError("FIREBASE: Send cancelled"); 
                MessageReceived?.Invoke("FIREBASE SEND CANCELLED"); 
                return; 
            } 
            Debug.Log("FIREBASE: Command queued: " + commandId + " -> " + command); 
        });
               
    }

    private bool GenericLedCommand(string command)
    {
        Debug.Log(
            "Checking for command LED: " + command
        );
         
        if (command == "LED") {
            RoverController.Instance.LED();
            return true;
        }

        return false;
    }

    private void SliderSetting(string command)
    {
        if (command.Length > 5 && command.Substring(0, 5) == "DUTY ") {
            if (Int32.TryParse(command.Substring(5),out int duty)) {
                RoverController.Instance.MimicSliderSetting(duty);

                Debug.Log("SliderSetting: " + duty);
            }
            else {
                Debug.Log("SliderSetting: Unable to parse duty value");
            }
        }
    }

    // =====================================================
    // DESTROY
    // =====================================================

    private void OnDestroy()
    {
        if (commandsReference != null) {
            //commandsReference.ValueChanged -= OnCommandChanged;
        }

        if (connectionReference != null) {
            connectionReference.ValueChanged -= OnFirebaseConnectionChanged;
        }

        Settings.OnReceiverChange -= SetAsReceiver;

        if (Instance == this) {
            Instance = null;
        }
    }
}

