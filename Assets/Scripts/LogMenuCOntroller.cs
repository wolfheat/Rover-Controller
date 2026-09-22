using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogMenuCOntroller : MonoBehaviour
{
    [SerializeField] private LayoutElement element;
    [SerializeField] private GameObject AutoSizeParent;
    [SerializeField] private GameObject noAutoSizeParent;

    private bool normal = true;

    private float normalHeight = 300f;
    private float largeHeight = 1500f;

    private float normalHeightPrefered = 550f;
    private float largeHeightPrefered = 1800f;

    public void Toggle()
    {
        normal = !normal;
        UpdateView();
    }

    public void UpdateView()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        if (normal) {
            AutoSizeParent.SetActive(true);
            transform.parent = AutoSizeParent.transform;
            transform.SetAsFirstSibling();
            noAutoSizeParent.SetActive(false);

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 490);

            element.minHeight = normalHeight;
            element.preferredHeight = normalHeightPrefered;
        }
        else {
            noAutoSizeParent.SetActive(true);
            transform.parent = noAutoSizeParent.transform;
            AutoSizeParent.SetActive(false);

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 1500);

            element.minHeight = largeHeight;
            element.preferredHeight = largeHeightPrefered;
        }
    }

}
