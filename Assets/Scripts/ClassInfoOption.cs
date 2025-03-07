using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClassInfoOption : MonoBehaviour
{
    [Header("UI Components")]
    public Toggle classToggle;
    public TMP_Text classNameText;
    public TMP_Text dayOfWeekText;
    public TMP_Text timeText;
    public TMP_Text ageRangeText;
    public Button seeMoreButton;

    [Header("Additional Details")]
    [TextArea]
    public string classDescription;
    public Sprite classActionImage;

    [Header("Popup Manager")]
    public ClassInfoPopupManager popupManager;

    void Start() {
        if (seeMoreButton != null)
            seeMoreButton.onClick.AddListener(OnSeeMoreClicked);
    }

    public void OnSeeMoreClicked()
    {
        // Pass the class details to the popup manager.
        if (popupManager != null)
        {
            popupManager.ShowPopup(
                classNameText.text,
                classDescription,
                classActionImage
                
            );
        }
    }
}
