using TMPro;
using UnityEngine;

public class MessageItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    public void SetText(string message) => messageText.text = message;


}
