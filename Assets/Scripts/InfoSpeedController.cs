using TMPro;
using UnityEngine;

public class InfoSpeedController : MonoBehaviour
{
    public static InfoSpeedController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start() => UpdateTime();

    [SerializeField] private TextMeshProUGUI infoText;

    public void AddTime()
    {
        Settings.Instance.AddMessageTime();
        UpdateTime();
    }

    public void SubtractTime()
    {
        Settings.Instance.SubtractMessageTime();
        UpdateTime();
    }

    public void UpdateTime() => infoText.text = Settings.Instance.MessageTime.ToString("F1");


}
