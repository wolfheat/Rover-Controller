using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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


        UpdateDelayVisual();
    }

    public void OnMessagesReceivedESPChanged(int amt) => receivedESPText.text = amt.ToString();

    public void OnMessagesSentESPChanged(int amt) => sentESPText.text = amt.ToString();
    
    public void OnMessagesSentFirebaseChanged(int amt) => sentFirebaseText.text = amt.ToString();

    public void OnMessagesReceivedFirebaseChanged(int amt) => receivedFireBaseText.text = amt.ToString();

    public void AddSecond()
    {
        Delay += 0.5f;
        UpdateDelayVisual();
    }

    private void UpdateDelayVisual() => delayText.text = Delay.ToString("F1");

    public void RemoveSecond()
    {
        Delay -= 0.5f;
        UpdateDelayVisual();
        UpdateDelayVisual();
    }

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

        if (timer < Delay) return;

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

    // ESP32 Messages
    public void OnMessageReceived(string message)
    {
        // Add A counter
        Settings.Instance.AddMessageCount(0);
        ShowPopup(message);

    }

    public void OnMessageTransmitted(string message)
    {
        // Add A counter
        Settings.Instance.AddMessageCount(1);
        ShowPopup(message);
    }

    // Firebase Messages
    public void OnMessageFirebaseReceived(string message)
    {
        // Add A counter
        Settings.Instance.AddMessageCount(2);
        ShowPopup(message);


    }

    public void OnMessageFirebaseTransmitted(string message)
    {
        // Add A counter
        Settings.Instance.AddMessageCount(3);
        ShowPopup(message);
    }


    public void ShowPopup(string message)
    {
        MessageItem item = Instantiate(messagePrefab,messageHolder.transform);
        item.SetText(message);
        popupList.Enqueue(item);
        UpdateAmt();
    }

    public void LastCommand(string message) => lastCommand.text = message;

    private Coroutine routine;

}
