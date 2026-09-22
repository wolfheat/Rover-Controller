using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageItemWithIcon : MessageItem
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Color[] colors;
    
    public MessageSource MessageType{ get; private set; }

    public void SetIcon(MessageSource type, MessageIcon icon)
    {
        // Type gives color
        image.color = colors[Math.Min((int)type,colors.Length-1)];

        // Icontype gives sprite
        image.sprite = sprites[Math.Min((int)icon,sprites.Length-1)];

        MessageType = type;
    }
}
    