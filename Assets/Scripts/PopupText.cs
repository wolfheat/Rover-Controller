using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public enum MessageIcon { Down, Left, Right, Up, Check, CloudUpload, Cancel, Download, OBS, Joystick, Locked, Minus, Plus, Power, Question, Unlocked, Upload, FastForward}
public enum MessageSource { ESP32Received, ESP32Transmitted, FirebaseReceived, FirebaseTransmitted, Other }

public class PopupText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amtText;

    [SerializeField] private TextMeshProUGUI sentESPText;
    [SerializeField] private TextMeshProUGUI receivedESPText;
    [SerializeField] private TextMeshProUGUI sentFirebaseText;
    [SerializeField] private TextMeshProUGUI receivedFireBaseText;

    [SerializeField] private TextMeshProUGUI delayText;
    [SerializeField] private TextMeshProUGUI lastCommand;
    [SerializeField] private GameObject popupObject;

    [SerializeField] private GameObject messageHolder;
    [SerializeField] private MessageItem messagePrefab;
    [SerializeField] private MessageItemWithIcon messageWithIconPrefab;

    public Queue<MessageItem> popupList = new();

    public float Delay = 1;

    public static PopupText Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ShowPopup("Start of Application.");
    }

    public void OnMessagesReceivedESPChanged(int amt) => receivedESPText.text = amt.ToString();

    public void OnMessagesSentESPChanged(int amt) => sentESPText.text = amt.ToString();
    
    public void OnMessagesSentFirebaseChanged(int amt) => sentFirebaseText.text = amt.ToString();

    public void OnMessagesReceivedFirebaseChanged(int amt) => receivedFireBaseText.text = amt.ToString();

    private void UpdateDelayVisual() => delayText.text = Delay.ToString("F1");

    private void OnEnable()
    {
        ESP32Usb.MessageRecieved += OnMessageReceived;
        ESP32Usb.MessageTransmitted += OnMessageTransmitted;

        RoverFirebase.MessageReceived += OnMessageFirebaseReceived;
        RoverFirebase.MessageTransmitted += OnMessageFirebaseTransmitted;

        ESP32Usb.LastCommand += LastCommand;
        Settings.MessageCountChange += UpdateAmt;
    }

    private void OnDisable()
    {
        ESP32Usb.MessageRecieved -= OnMessageReceived;
        ESP32Usb.MessageTransmitted -= OnMessageTransmitted;

        RoverFirebase.MessageReceived -= OnMessageFirebaseReceived;
        RoverFirebase.MessageTransmitted -= OnMessageFirebaseTransmitted;

        ESP32Usb.LastCommand -= LastCommand;
        Settings.MessageCountChange -= UpdateAmt;
    }
    
    float timer = 0f;

    private void Update()
    {
        if(popupList.Count <= 0) return;

        timer += Time.deltaTime;

        if (timer < Settings.Instance.MessageTime) return;

        // Reset Timer
        timer = 0f;
                
        // Remove Top Message
        MessageItem item = popupList.Dequeue();
        Destroy(item.gameObject);
        UpdateAmt();
    }

    private void UpdateAmt()
    {
        amtText.text = popupList.Count.ToString();
        int[] messageAmt = Settings.Instance.MessageCount;
        receivedESPText.text = messageAmt[0].ToString();
        sentESPText.text = messageAmt[1].ToString();
        receivedFireBaseText.text = messageAmt[2].ToString();
        sentFirebaseText.text = messageAmt[3].ToString();
    }


    // ESP Received - ESP Transmitted - Firebase Received - Firebase Transmitted

    // ** ESP32 Messages ** 
    public void OnMessageReceived(string message)
    {
        // ESP Received
        Settings.Instance.AddMessageCount((int)MessageSource.ESP32Received);
        ShowPopup(message, MessageSource.ESP32Received);
    }

    public void OnMessageTransmitted(string message)
    {
        // ESP Transmitted
        Settings.Instance.AddMessageCount((int)MessageSource.ESP32Transmitted);
        ShowPopup(message,MessageSource.ESP32Transmitted);
    }

    // ** Firebase Messages ** 
    public void OnMessageFirebaseReceived(string message)
    {
        // Firebase Received
        Settings.Instance.AddMessageCount((int)MessageSource.FirebaseReceived);
        ShowPopup(message, MessageSource.FirebaseReceived);
    }

    public void OnMessageFirebaseTransmitted(string message)
    {
        // Firebase Transmitted
        Settings.Instance.AddMessageCount((int)MessageSource.FirebaseTransmitted);
        ShowPopup(message, MessageSource.FirebaseTransmitted);
    }

    public void ShowPopup(string message, MessageSource type)
    {
        // Show this message with an icon
        MessageItemWithIcon item = Instantiate(messageWithIconPrefab, messageHolder.transform);
        
        item.SetText(message);

        MessageIcon icon = GetIconByMessage(message);

        item.SetIcon(type,icon);

        if (!Settings.Instance.GetMessageVisability(type)) {
            item.gameObject.SetActive(false);
        }

        popupList.Enqueue(item);

        UpdateAmt();
    }

    private MessageIcon GetIconByMessage(string message)
    {
        // ESP32: 
        string[] messageParts = message.Split(':');
        string subString = messageParts[messageParts.Length == 1 ? 0 : 1].TrimStart();
        
        Debug.Log("Handling String: "+message);
        Debug.Log("Handling SubString: "+subString);

        return subString switch
        {
            "MOTOR ON" => MessageIcon.Power,
            "MOTOR OFF" => MessageIcon.Power,
            "LED ON" => MessageIcon.OBS,
            "LED OFF" => MessageIcon.Question,
            "MOTOR DRIVE" => MessageIcon.Up,
            "MOTOR LEFT" => MessageIcon.Left,
            "MOTOR RIGHT" => MessageIcon.Right,
            "MOTOR BACK" => MessageIcon.Down,
            "MOTOR STATIC" => MessageIcon.Cancel,
            "FIREBASE SEND ERROR" => MessageIcon.Question,
            "FIREBASE SEND CANCELLED" => MessageIcon.Question,
            "FIREBASE -> ESP32" => MessageIcon.Plus,
            var s when s.StartsWith("DUTY") => MessageIcon.FastForward,
            _ => MessageIcon.Check
        };
    }

    public void ShowPopup(string message)
    {
        MessageItem item = Instantiate(messagePrefab,messageHolder.transform);
        item.SetText(message);
        popupList.Enqueue(item);
        UpdateAmt();
    }

    public void LastCommand(string message) => lastCommand.text = message;

    internal void UpdateVisability()
    {
        foreach (MessageItem item in popupList) {
            if(item is MessageItemWithIcon itemIcon) {
                item.gameObject.SetActive((Settings.Instance.GetMessageVisability(itemIcon.MessageType))); 
            }
        }
    }
    
    public void ClearAll()
    {
        foreach (MessageItem item in popupList) {
            Destroy(item.gameObject);
        }
        popupList.Clear();
        UpdateAmt();
    }

    private Coroutine routine;

}
