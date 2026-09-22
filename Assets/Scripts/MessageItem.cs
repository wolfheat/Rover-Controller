using System.Collections;
using TMPro;
using UnityEngine;
public class MessageItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    public void SetText(string message)
    {
        messageText.text = message;

        // Make TMP calculate the new text dimensions.
        messageText.ForceMeshUpdate();

        StartCoroutine(DelayedSize());
    }

    private IEnumerator DelayedSize()
    {
        yield return null;
        yield return null;
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, messageText.rectTransform.sizeDelta.y);

    }

}
    