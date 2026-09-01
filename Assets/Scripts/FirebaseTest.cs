using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseTest : MonoBehaviour
{
    private DatabaseReference commandReference;

    private void Start()
    {
        commandReference =
            FirebaseDatabase.DefaultInstance
                .GetReference("rover1/command");

        commandReference.ValueChanged += OnCommandChanged;

        Debug.Log("FIREBASE: Command listener started");
    }

    // =====================================================
    // TEST BUTTON
    // =====================================================

    public void SendTestCommand()
    {
        Debug.Log("FIREBASE: Sending TEST command...");
        
        PopupText.Instance.LastCommand("FIREBASE: Sending TEST command");

        commandReference
            .SetValueAsync("LED")
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
                    "FIREBASE: TEST command written"
                );
            });
    }

    // =====================================================
    // RECEIVER
    // =====================================================

    private void OnCommandChanged( object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) {
            Debug.LogError(
                "FIREBASE RX ERROR: " +
                args.DatabaseError.Message
            );

            return;
        }

        if (!args.Snapshot.Exists) {
            Debug.Log("FIREBASE RX: No command");
            return;
        }

        string command = args.Snapshot.Value?.ToString();

        PopupText.Instance.ShowPopup("FIREBASE RX: [" + command + "] " + (Settings.Instance.Receiver ? "RECEIVER": "TRANSMITTER"));

        Debug.Log(
            "FIREBASE RX: [" +
            command +
            "]"
        );


        // ==========================================
        // SEND COMMAND TO ESP32
        // ==========================================

        if (command == "LED") {
            ESP32Usb.Instance.SendCommand("LED");
        }


        // ---------------------------------------------------------
        // Delete command after processing
        // ---------------------------------------------------------

        if(!Settings.Instance.Receiver) return; // Skip Deleting if in transmitter mode

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

    private void OnDestroy()
    {
        if (commandReference != null) {
            commandReference.ValueChanged -=
                OnCommandChanged;
        }
    }
}