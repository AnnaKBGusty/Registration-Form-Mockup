using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClassInfoPopupManager : MonoBehaviour
{
    [Header("Popup UI Components")]
    public GameObject popupPanel;
    public TMP_Text popupTitleText;
    public TMP_Text popupDescriptionText;
    public Image classImageDisplay;
    public Button goBackButton;

    void Start() {
        // Ensure the popup is hidden at start.
        HidePopup();
        if (goBackButton != null)
            goBackButton.onClick.AddListener(HidePopup);
    }

    public void ShowPopup(string className, string description, Sprite classSprite)
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            if (popupTitleText != null)
                popupTitleText.text = className;
            if (popupDescriptionText != null)
                popupDescriptionText.text = description;
            if (classImageDisplay != null)
                classImageDisplay.sprite = classSprite;

        }
    }

    public void HidePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}
