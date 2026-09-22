using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecieverTransmittorController : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private ButtonOnOffController buttonColorController;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color colorTransmitter;
    [SerializeField] private Color colorReceiver;

    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite transmitterSprite;
    [SerializeField] private Sprite receiverSprite;



    public static RecieverTransmittorController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ToggleType(bool isReciever)
    {
        Debug.Log("*** Toggle set to " + isReciever);
        typeText.text = isReciever ? "RECEIVER" : "TRANSMITTER";
        //buttonColorController.SetOnOff(isReciever);
        backgroundImage.color = isReciever ? colorReceiver : colorTransmitter;
        iconImage.sprite = isReciever ? receiverSprite : transmitterSprite;
        Settings.Instance.Receiver = isReciever;
    }

    public void ToggleType() => ToggleType(!Settings.Instance.Receiver);

    public void OnPointerClick(PointerEventData eventData) => ToggleType();
}
