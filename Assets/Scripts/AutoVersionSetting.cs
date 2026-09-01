using TMPro;
using UnityEngine;

public class AutoVersionSetting : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionText;

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateVersion();
    }

    [ContextMenu("Update Version text")]
    public void UpdateVersion()
    {
        if (versionText != null) {
            versionText.text = "v. " + Application.version;
        }
    }
#endif
}
