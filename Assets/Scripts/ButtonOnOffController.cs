using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOnOffController : MonoBehaviour
{
    [SerializeField] private Color offColor;
    [SerializeField] private Color onColor;
    [SerializeField] private Image image;
    [SerializeField] private Sprite spriteON;
    [SerializeField] private Sprite spriteOFF;


    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private string textOFF;
    [SerializeField] private string textON;

    public void SetOnOff(bool on)
    {
        Debug.Log("SETTING BUTTON "+name +": " + on);

        if (spriteON == null || spriteOFF == null) 
            image.color = on ? onColor : offColor;
        else // There exists a sprite
            image.sprite = on ? spriteON : spriteOFF;

        if (textField != null)
            textField.text = on ? textON :textOFF;

    }
}
