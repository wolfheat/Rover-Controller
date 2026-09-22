using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings Instance { get; private set; }

    public static Action<bool> OnReceiverChange;
    public static Action MessageCountChange;

    private bool receiver = true;
    private bool[] messageTypes;



    public bool Receiver
    {
        get => receiver; set
        {
            receiver = value;
            OnReceiverChange?.Invoke(value);
        }
    }

    // ESP Received - ESP Transmitted - Firebase Received - Firebase Transmitted
    private int[] messageCount = new int[4];
    public int[] MessageCount => messageCount;

    private float messageTime = 3f;
    public const float MessageTimeStep = 0.5f;
    public float MessageTime => messageTime;

    public void AddMessageTime() => messageTime = Mathf.Clamp(messageTime + MessageTimeStep, 0, 20);

    public void SubtractMessageTime() => messageTime = Mathf.Clamp(messageTime - MessageTimeStep, 0, 20);

    public void AddMessageCount(int type)
    {
        messageCount[type]++;
        MessageCountChange?.Invoke();
    }

    [Header("Answer Colors")]
    [SerializeField] public Color NeutralGreyColor;
    [SerializeField] public Color CorrectColor;
    [SerializeField] public Color WrongColor;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Debug.Log("Created Settings");

        messageTypes = new bool[4];
        for (int i = 0; i < messageTypes.Length; i++) {
            messageTypes[i] = true;
        }
    }

    internal bool ToggleMessageTypeVisability(MessageSource type)
    {
        messageTypes[(int)type] = !messageTypes[(int)type];

        // Also notify the popup
        PopupText.Instance.UpdateVisability();

        return messageTypes[(int)type];
    }

    internal bool GetMessageVisability(MessageSource type) => messageTypes[(int)type];
}
