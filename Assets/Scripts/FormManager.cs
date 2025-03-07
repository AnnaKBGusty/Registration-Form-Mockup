using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System;
using System.Globalization;
using System.Collections.Generic;  // For List

using UnityEngine;
using TMPro;

[System.Serializable]
public class ClassInfoData
{
    public string className;
    public string dayOfWeek;
    public string time;       // e.g., "3:30 to 5pm"
    public string ageRange;   // e.g., "8-12"
    [TextArea]
    public string classDescription;
    public Sprite classActionImage;
}

public class FormManager : MonoBehaviour
{
    public ScrollRect formScrollRect;
    // --- 1. UI References ---
    [Header("Text Fields (TMPro)")]
    public TMP_InputField firstNameLookupInput;
    public TMP_InputField lastNameLookupInput;
    public TMP_InputField birthdayInput; // Single field for MM/DD/YYYY
    public TMP_InputField addressInput;
    public TMP_InputField zipCodeInput;
    public TMP_InputField gradeLevelInput;
    public TMP_InputField homePhoneInput;

    [Header("Display Age")]
    public TMP_Text ageText; // This text object will display "XX years old"

    [Header("Q1: Have you registered earlier this year Radio Toggles (UnityEngine.UI)")]
    public ToggleGroup qOneRadioGroup;
    public Toggle yes;  // For "Yes"
    public Toggle no;   // For "No"
    
    [Header("Q2: Chlid's Gender Radio Toggles (UnityEngine.UI)")]
    public ToggleGroup genderRadioGroup;
    public Toggle female;  // For "Female"
    public Toggle male;   // For "Male"
    public Toggle nonbinary; //for nb
    public Toggle PreferNotSay; //for prefer not to say
    public Toggle other; //for write in option.
    public TMP_InputField otherGenderInput;

    [Header("Parent #1 Fields")]
    public Toggle sameAsChildsAddressToggle;   // The toggle that says "Same as child's address"
    public TMP_InputField parentAddressInput;  // Parent's address field

    [Header("Content Groups")]
    public GameObject[] registeredContentGroup;   // Fields for registered users (Yes)
    public GameObject[] notRegisteredContentGroup;  // Fields for non-registered users (No)
    public GameObject[] classSelectionOptions;      // Fields for classes

        // --- NEW: Individual error messages for required fields (registered) ---
    [Header("Required Field Errors (Already Registered)")]
    public TMP_Text firstNameErrorText;
    public TMP_Text lastNameErrorText;
    public TMP_Text birthdayErrorText;
    public TMP_Text classSelectionErrorText;

    [Header ("Required Field Errors (New Registrations)")]
    public TMP_Text schoolNameErrorText;
    public TMP_Text gradeLevelErrorText;

        [Header("General Error Display (TMPro)")]
    public TMP_Text errorText;

    [Header("New Registration Required Fields")]
    public List<RequiredFieldValidator> newRegistrationFields;


    // --- 2. Google Form IDs and URL ---
    private string formUrl = "https://docs.google.com/forms/d/e/1FAIpQLSeKGPH5fTmKGrxyOxnpwxFcdkSP7MI__oVR1Z9FromN_Z3mPg/formResponse";
    
    private string registeredRadioEntry    = "entry.162168195";  // Yes/No have you registered.
    private string childFirstNameForLookup = "entry.1590321039";  // For registered user lookup.
    private string childLastNameForLookup  = "entry.1655111886";  // For registered user lookup.
    private string entryBirthday           = "entry.1144851944";  // For registered child lookup.
    private string entryCheckbox           = "entry.950719446";   // For selecting classes.
    private string ageEntry                = "entry.1500938098";  // For age.
    private string genderEntry              = "entry.118857172"; //for gender. 
    private string schoolEntry              = "entry.1711432728";  // for school name 

    [Header("School Info UI")]
    public GameObject schoolListPanel;
    public TMP_InputField schoolSearchInput;
    public Toggle notOnListToggle;
    public TMP_InputField customSchoolInput;
    public TMP_Text chosenSchoolText;
    public Button goBackButton;
    public List<string> allSchools = new List<string>();
    public Transform schoolListContainer;
    public GameObject schoolButtonPrefab;
    private string selectedSchoolName = "";


    [Header("Select Classes Toggles (Using ClassInfoOption Prefab)")]
    // Remove the previous checkboxToggles and use a dynamically created list:
    public List<ClassInfoOption> classInfoOptions = new List<ClassInfoOption>();

    [Header("Class Info Data")]
    // List to assign class details via the inspector.
    public List<ClassInfoData> classInfoDataList = new List<ClassInfoData>();

    [Header("Class Info Option Prefab Settings")]
    public GameObject classInfoOptionPrefab;  // Your prefab with the ClassInfoOption script.
    public Transform classInfoPanelContainer; // The panel (or container) where the prefabs will be instantiated.

    // (Optional) Reference to your popup manager, so you can assign it to each instantiated option.
    public ClassInfoPopupManager classInfoPopupManager;

    [Header("Available Classes from Spreadsheet")]
    public string availableClassesUrl; // Set this to your published spreadsheet URL.
    private List<string> availableClassNames = new List<string>();


    IEnumerator Start()
    {
        allSchools.AddRange(new List<string> {
            "Aliamanu Elementary",
            "Anuenue School",
            "Kaulani Elementary",
            "Kalihi Kai Elementary",
            "Kalihi Waena Elementary",
            "Kapalama Elementary",
            "Kauluwela Elementary",
            "Lanakila Elementary",
            "Likelike Elementary",
            "Maemae Elementary",
            "Manoa Elementary",
            "Nu'uanu Elementary",
            "Pauoa Elementary",
            "Pu'uhale Elementary",
            "Salt Lake Elementary",
            "Waimalu Elementary",
            "Halau Kumana",
            "Dole Middle School",
            "Kalakaua Middle School",
            "Kawananakoa Middle",
            "Ke'elikolani Middle (Central)",
            "Niu Valley Middle School",
            "Stevenson Middle School",
            "Washington Middle School",
            "Farrington High School",
            "McKinley High School",
            "Roosevelt High School",
            "Homeschool",
            "Other"
        });

        // Set initial visibility based on the current toggle state.
        UpdateContentFieldsVisibility();
        UpdateAgeDisplay();

        // Add listeners so the content groups and age update when fields change.
        yes.onValueChanged.AddListener(delegate { 
            UpdateContentFieldsVisibility(); 
        });
        no.onValueChanged.AddListener(delegate { 
            UpdateContentFieldsVisibility(); 
        });
        firstNameLookupInput.onEndEdit.AddListener(delegate { 
            UpdateContentFieldsVisibility(); 
        });
        lastNameLookupInput.onEndEdit.AddListener(delegate {
            UpdateContentFieldsVisibility();
        });
        birthdayInput.onEndEdit.AddListener(delegate { 
            UpdateContentFieldsVisibility();
            UpdateAgeDisplay();
        });
            // Initially hide the "Other" text field
        otherGenderInput.gameObject.SetActive(false);

        // When the "Other" toggle changes, show/hide the text field
        other.onValueChanged.AddListener(isOn => {
            otherGenderInput.gameObject.SetActive(isOn);
        });
        // 1) Build the list of school buttons
        InitializeSchoolList();

        // 2) Listen for search input changes
        if (schoolSearchInput != null)
            schoolSearchInput.onValueChanged.AddListener(FilterSchoolList);

        // 3) Toggle for "not on list"
        if (notOnListToggle != null)
            notOnListToggle.onValueChanged.AddListener(OnNotOnListToggleChanged);

        // 4) Custom school input field
        if (customSchoolInput != null)
            customSchoolInput.onEndEdit.AddListener(delegate { OnCustomSchoolInputEndEdit(); });

        // 5) Go Back button
        if (goBackButton != null)
            goBackButton.onClick.AddListener(OnGoBackButtonClicked);

        // Make sure the chosenSchoolText & goBackButton are hidden at start
        if (chosenSchoolText) chosenSchoolText.gameObject.SetActive(false);
        if (goBackButton) goBackButton.gameObject.SetActive(false);

        sameAsChildsAddressToggle.onValueChanged.AddListener(OnSameAsChildAddressToggleChanged);
    
        // Also listen for changes in the child’s address:
        addressInput.onValueChanged.AddListener(OnChildAddressChanged);

        // Listen for changes in first name, last name, and birthday
        firstNameLookupInput.onValueChanged.AddListener(OnFirstNameChanged);
        lastNameLookupInput.onValueChanged.AddListener(OnLastNameChanged);
        birthdayInput.onValueChanged.AddListener(OnBirthdayChanged);


        gradeLevelInput.onValueChanged.AddListener(OnGradeLevelChanged);
        foreach(var validator in newRegistrationFields)
        {
            // Capture the current validator in a local variable
            RequiredFieldValidator currentValidator = validator;
            currentValidator.inputField.onValueChanged.AddListener((string s) => {
                currentValidator.Validate();
            });
        }

        // Fetch available classes from the spreadsheet.
        yield return StartCoroutine(FetchAvailableClasses());

        // Filter the master list based on available class names.
        List<ClassInfoData> filteredDataList = FilterAvailableClasses();

        // Instantiate class option prefabs using the filtered list.
        InstantiateClassInfoOptions(filteredDataList);

        // Add toggle listeners to each option.
        foreach (var option in classInfoOptions)
        {
            option.classToggle.onValueChanged.AddListener(OnClassSelectionChanged);
        }
            
    }

    private IEnumerator FetchAvailableClasses()
    {
        UnityWebRequest www = UnityWebRequest.Get(availableClassesUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching available classes: " + www.error);
            yield break;
        }

        // Assume CSV format with one column of class names.
        // Optionally skip a header row if present.
        string csvText = www.downloadHandler.text;
        string[] lines = csvText.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        availableClassNames.Clear();
        bool hasHeader = true; // adjust if needed
        int startIndex = hasHeader ? 1 : 0;
        for (int i = startIndex; i < lines.Length; i++)
        {
            string className = lines[i].Trim();
            if (!string.IsNullOrEmpty(className))
            {
                availableClassNames.Add(className);
            }
        }
    }

    private List<ClassInfoData> FilterAvailableClasses()
    {
        List<ClassInfoData> filteredList = new List<ClassInfoData>();
        foreach (var data in classInfoDataList)
        {
            // Only include the class if its name is in the available list.
            if (availableClassNames.Contains(data.className))
            {
                filteredList.Add(data);
            }
        }
        return filteredList;
    }


    private void InstantiateClassInfoOptions(List<ClassInfoData> dataList)
    {
        // Clear out any existing prefabs in the container.
        foreach (Transform child in classInfoPanelContainer)
        {
            Destroy(child.gameObject);
        }
        classInfoOptions.Clear();

        // Instantiate a prefab for each available class.
        foreach (ClassInfoData data in dataList)
        {
            GameObject newOption = Instantiate(classInfoOptionPrefab, classInfoPanelContainer);
            ClassInfoOption optionComponent = newOption.GetComponent<ClassInfoOption>();
            if (optionComponent != null)
            {
                optionComponent.classNameText.text = data.className;
                optionComponent.dayOfWeekText.text = data.dayOfWeek;
                optionComponent.timeText.text = data.time;
                optionComponent.ageRangeText.text = data.ageRange;
                optionComponent.classDescription = data.classDescription;
                optionComponent.classActionImage = data.classActionImage;
                // Optionally assign the popup manager.
                optionComponent.popupManager = classInfoPopupManager;

                classInfoOptions.Add(optionComponent);
            }
        }
    }





    private void InstantiateClassInfoOptions()
    {
        // Clear out any existing children in the container.
        foreach (Transform child in classInfoPanelContainer)
        {
            Destroy(child.gameObject);
        }
        classInfoOptions.Clear();

        // Loop through the data list and instantiate a prefab for each class.
        foreach (ClassInfoData data in classInfoDataList)
        {
            GameObject newOption = Instantiate(classInfoOptionPrefab, classInfoPanelContainer);
            ClassInfoOption optionComponent = newOption.GetComponent<ClassInfoOption>();
            if (optionComponent != null)
            {
                // Set the UI elements.
                optionComponent.classNameText.text = data.className;
                optionComponent.dayOfWeekText.text = data.dayOfWeek;
                optionComponent.timeText.text = data.time;
                optionComponent.ageRangeText.text = data.ageRange;
                // Set additional details for the popup.
                optionComponent.classDescription = data.classDescription;
                optionComponent.classActionImage = data.classActionImage;
                // Assign the popup manager reference (if needed).
                optionComponent.popupManager = classInfoPopupManager;
                
                // Add to our list.
                classInfoOptions.Add(optionComponent);
            }
        }
    }

    // --- 3. Update UI Groups based on Toggle selection ---
    public void UpdateContentFieldsVisibility()
    {
        if (yes.isOn)
        {
            // Show registered content and hide non-registered content.

            foreach (var item in notRegisteredContentGroup)
                item.SetActive(false);
            foreach (var item in registeredContentGroup)
                item.SetActive(true);
        }
        else if (no.isOn)
        {
            // Show non-registered content and hide registered content.
            foreach (var item in registeredContentGroup)
                item.SetActive(false);
            foreach (var item in notRegisteredContentGroup)
                item.SetActive(true);
        }
    }

    // --- 4. Update Age Display ---
    private void UpdateAgeDisplay()
    {
        if (IsValidDate(birthdayInput.text))
        {
            int age = CalculateAgeFromBirthday(birthdayInput.text);
            ageText.text = age + " years old";
        }
        else
        {
            ageText.text = ""; // Clear text if invalid date.
        }
    }

    // --- 5. Calculate Age from Birthday ---
    private int CalculateAgeFromBirthday(string birthday)
    {
        DateTime birthDate;
        if (DateTime.TryParseExact(birthday, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate))
        {
            int age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now < birthDate.AddYears(age))
                age--;
            return age;
        }
        return 0;
    }

// --- 6. Submit Button Handler ---
    public void OnSubmitButtonClicked()
    {
        // We’ll do per-field validation if user is "already registered" (yes.isOn)
        if (yes.isOn)
        {
            bool hasError = false;

            // 1) First Name
            if (string.IsNullOrWhiteSpace(firstNameLookupInput.text))
            {
                if (firstNameErrorText) firstNameErrorText.gameObject.SetActive(true);
                hasError = true;
            }
            else
            {
                if (firstNameErrorText) firstNameErrorText.gameObject.SetActive(false);
            }

            // 2) Last Name
            if (string.IsNullOrWhiteSpace(lastNameLookupInput.text))
            {
                if (lastNameErrorText) lastNameErrorText.gameObject.SetActive(true);
                hasError = true;
            }
            else
            {
                if (lastNameErrorText) lastNameErrorText.gameObject.SetActive(false);
            }

            // 3) Birthday
            if (!IsValidDate(birthdayInput.text))
            {
                if (birthdayErrorText) birthdayErrorText.gameObject.SetActive(true);
                hasError = true;
            }
            else
            {
                if (birthdayErrorText) birthdayErrorText.gameObject.SetActive(false);
            }

            // 4) Class Selection (must have at least one checkbox on)
            bool anyClassSelected = false;
            foreach (var option in classInfoOptions)
            {
                if (option.classToggle.isOn)
                {
                    anyClassSelected = true;
                    break;
                }
            }

            if (!anyClassSelected)
            {
                if (classSelectionErrorText) classSelectionErrorText.gameObject.SetActive(true);
                hasError = true;
            }
            else
            {
                if (classSelectionErrorText) classSelectionErrorText.gameObject.SetActive(false);
            }

            // If there's any error, show a general message and stop
            if (hasError)
            {
                if (errorText) errorText.text = "Please fill out all required fields.";
                if(formScrollRect != null){
                    formScrollRect.normalizedPosition = new Vector2(0, 1);
                }
                return;
            }
            else
            {
                // Clear the general error text if all good
                if (errorText) errorText.text = "";
            }
        }

        // If user is "No" or we passed all "Yes" checks, proceed
        StartCoroutine(SubmitFormData());
    }

    // --- 7. Coroutine to Submit Data ---
    private IEnumerator SubmitFormData()
    {
        WWWForm form = new WWWForm();

        // (A) First Name and Last Name Look up
        form.AddField(childFirstNameForLookup, firstNameLookupInput.text);
        form.AddField(childLastNameForLookup, lastNameLookupInput.text);

        // (B) Birthday Look Up
        form.AddField(entryBirthday, birthdayInput.text);

        // (C) Calculate and add Age
        int age = CalculateAgeFromBirthday(birthdayInput.text);
        // Submit the age as "XX years old"
        form.AddField(ageEntry, age);

        // (E) yes or no registered already?
        if (yes.isOn)
        {
            form.AddField(registeredRadioEntry, "Yes");
        }
        else if (no.isOn)
        {
            form.AddField(registeredRadioEntry, "No");
        }

        // (F) Classes
        foreach (ClassInfoOption option in classInfoOptions)
        {
            if (option.classToggle.isOn)
            {
                // You can send option.classNameText.text or any additional info
                form.AddField(entryCheckbox, option.classNameText.text);
            }
        }

        // (G) Gender
        if (female.isOn) form.AddField(genderEntry, "Female");
        else if (male.isOn) form.AddField(genderEntry, "Male");
        else if (nonbinary.isOn) form.AddField(genderEntry, "Non-binary");
        else if (PreferNotSay.isOn) form.AddField(genderEntry, "Prefer not to say");
        else if (other.isOn)
        {
            form.AddField(genderEntry, "__other_option__");
            form.AddField(genderEntry + ".other_option_response", otherGenderInput.text);
        }

        //(H) School Name
        if (no.isOn)
        {
            // For non-registered users, school name is required.
            if (string.IsNullOrWhiteSpace(selectedSchoolName))
            {
                ValidateSchoolName();
                if(formScrollRect != null)
                {
                    formScrollRect.normalizedPosition = new Vector2(0, 1);
                }
                yield break; // Stop submission until the user fixes it.
            }
            else
            {
                form.AddField(schoolEntry, selectedSchoolName);
            }
        }
        else if (yes.isOn)
        {
            // For registered users, school name isn't required.
            // We pass an empty string so the form submission is valid.
            form.AddField(schoolEntry, "");
        }

        // (E) New Registration Fields via Generic Validator
        if (no.isOn)
        {
            bool allValid = true;
            foreach(var validator in newRegistrationFields)
            {
                if (!validator.Validate())
                    allValid = false;
            }
            if (!allValid)
            {
                if (formScrollRect != null)
                    formScrollRect.normalizedPosition = new Vector2(0, 1);
                yield break;
            }
            else
            {
                foreach(var validator in newRegistrationFields)
                {
                    form.AddField(validator.entryId, validator.inputField.text);
                }
            }
        }
        else if (yes.isOn)
        {
            // For registered users, these fields are not required.
            foreach(var validator in newRegistrationFields)
            {
                form.AddField(validator.entryId, "");
            }
        }

        // Send the request.
        UnityWebRequest www = UnityWebRequest.Post(formUrl, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError ||
            www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Form submission error: " + www.error);
        }
        else
        {
            Debug.Log("Form submitted successfully!");
        }
    }

    // --- 8. Helper Method to Validate Date Format ---
    private bool IsValidDate(string dateString)
    {
        DateTime parsedDate;
        return DateTime.TryParseExact(
            dateString,
            "MM/dd/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsedDate
        );
    }

    // --- helper method t o spawn each school in all schools, set it's lael and hook up a click event

        private void InitializeSchoolList()
    {
        if (schoolListContainer == null || schoolButtonPrefab == null) return;

        // Clear out any existing children, if desired:
        foreach (Transform child in schoolListContainer)
        {
            Destroy(child.gameObject);
        }

        // Create a button for each school
        foreach (string schoolName in allSchools)
        {
            GameObject newButton = Instantiate(schoolButtonPrefab, schoolListContainer);
            TMP_Text textComp = newButton.GetComponentInChildren<TMP_Text>();
            if (textComp != null) textComp.text = schoolName;

            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                // Important to cache the local variable to avoid closure issues
                string capturedName = schoolName;
                btn.onClick.AddListener(() => OnSchoolSelected(capturedName));
            }
        }
    }

    /// Called whenever the user types in the search bar. We show/hide 
    /// each school button if it contains the typed text.
    private void FilterSchoolList(string searchTerm)
    {
        searchTerm = searchTerm.ToLower().Trim();

        foreach (Transform child in schoolListContainer)
        {
            TMP_Text text = child.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                bool matches = text.text.ToLower().Contains(searchTerm);
                child.gameObject.SetActive(matches);
            }
        }
    }

    /// User clicked on a school from the list.
    private void OnSchoolSelected(string schoolName)
    {
        selectedSchoolName = schoolName;
        chosenSchoolText.text = schoolName;

        ValidateSchoolName();

        // Hide the list panel and search bar, show the chosen text & go-back button & grade level input
        if (schoolListPanel) schoolListPanel.SetActive(false);
        schoolSearchInput.gameObject.SetActive(false);
        if (chosenSchoolText) chosenSchoolText.gameObject.SetActive(true);
        if (goBackButton) goBackButton.gameObject.SetActive(true);
        gradeLevelInput.gameObject.SetActive(true);

        // Uncheck "not on list" if it was on
        if (notOnListToggle) notOnListToggle.isOn = false;
        if (customSchoolInput) customSchoolInput.gameObject.SetActive(false);
    }

    private void OnNotOnListToggleChanged(bool isOn)
    {
        // Show the custom school input if toggle is ON, hide it if OFF
        if (customSchoolInput) 
            customSchoolInput.gameObject.SetActive(isOn);

        // Hide the search bar and school list panel if toggle is ON
        if (schoolSearchInput) 
            schoolSearchInput.gameObject.SetActive(!isOn);
        if (schoolListPanel) 
            schoolListPanel.SetActive(!isOn);

        // Hide the chosenSchoolText if toggle is ON
        if (chosenSchoolText) 
            chosenSchoolText.gameObject.SetActive(!isOn);

        //Hide the grade level input if toggle is ON
        gradeLevelInput.gameObject.SetActive(false);

        // Optionally clear out any previously selected school if toggling ON
        if (isOn)
        {
            selectedSchoolName = "";
            chosenSchoolText.text = "";
            ValidateSchoolName();
        }
    }

    /// Called when the user finishes typing in the custom school field.
    /// We treat that as the chosen school name.
    private void OnCustomSchoolInputEndEdit()
    {
        if (string.IsNullOrWhiteSpace(customSchoolInput.text)) return;

        selectedSchoolName = customSchoolInput.text;
        chosenSchoolText.text = selectedSchoolName;
        customSchoolInput.gameObject.SetActive(false);

        ValidateSchoolName();

        // Hide the list panel, show chosen text & go-back button
        if (schoolListPanel) schoolListPanel.SetActive(false);
        if (chosenSchoolText) chosenSchoolText.gameObject.SetActive(true);
        if (goBackButton) goBackButton.gameObject.SetActive(true);
        gradeLevelInput.gameObject.SetActive(true);
    }

    /// When user hits "Go Back," we reset everything: hide the chosen text, 
    /// show the list again, clear the toggle, etc.
    private void OnGoBackButtonClicked()
    {
        selectedSchoolName = "";
        chosenSchoolText.text = "";

        if (chosenSchoolText) chosenSchoolText.gameObject.SetActive(false);
        if (goBackButton) goBackButton.gameObject.SetActive(false);
        gradeLevelInput.gameObject.SetActive(false);

        if (customSchoolInput) customSchoolInput.text = "";
        if (notOnListToggle) notOnListToggle.isOn = false;
        if (customSchoolInput) customSchoolInput.gameObject.SetActive(false);

        // Show the main panel with the school list again
        if (schoolListPanel) schoolListPanel.SetActive(true);
        schoolSearchInput.gameObject.SetActive(true);
    }

    private void OnSameAsChildAddressToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // Copy the child's address into the parent's field
            parentAddressInput.text = addressInput.text;
            
            // (Optional) Make the parent field non-editable if you want to lock it
            parentAddressInput.interactable = false;
        }
        else
        {
            // Clear it out (or leave whatever you want)
            parentAddressInput.text = "";
            
            // Re-enable editing
            parentAddressInput.interactable = true;
        }
    }


    private void OnChildAddressChanged(string newValue)
    {
        // Only update if toggle is ON
        if (sameAsChildsAddressToggle.isOn)
        {
            parentAddressInput.text = newValue;
        }
    }

    private void OnFirstNameChanged(string newValue)
    {
        // If first name is no longer empty, hide error; otherwise show it.
        bool isValid = !string.IsNullOrWhiteSpace(newValue);
        firstNameErrorText.gameObject.SetActive(!isValid);
    }

    private void OnLastNameChanged(string newValue)
    {
        bool isValid = !string.IsNullOrWhiteSpace(newValue);
        lastNameErrorText.gameObject.SetActive(!isValid);
    }

    private void OnBirthdayChanged(string newValue)
    {
        bool isValid = IsValidDate(newValue);
        birthdayErrorText.gameObject.SetActive(!isValid);
    }

    private void OnClassSelectionChanged(bool _)
    {
        bool anyClassSelected = false;
        foreach (var option in classInfoOptions)
        {
            if (option.classToggle.isOn)
            {
                anyClassSelected = true;
                break;
            }
        }
        classSelectionErrorText.gameObject.SetActive(!anyClassSelected);
    }


     private void ValidateSchoolName()
    {
        // Only required for non-registered users.
        if (no.isOn)
        {
            bool isValid = !string.IsNullOrWhiteSpace(selectedSchoolName);
            schoolNameErrorText.gameObject.SetActive(!isValid);
        }
        else
        {
            // For registered users, it's not required so hide the error.
            schoolNameErrorText.gameObject.SetActive(false);
        }
    }
    
    private void OnGradeLevelChanged(string newValue)
    {
        ValidateGradeLevel();
    }

    private void ValidateGradeLevel()
    {
        // Only required for non-registered users.
        if (no.isOn)
        {
            bool isValid = !string.IsNullOrWhiteSpace(gradeLevelInput.text);
            gradeLevelErrorText.gameObject.SetActive(!isValid);
        }
        else
        {
            // For registered users, it's not required so hide the error.
            gradeLevelErrorText.gameObject.SetActive(false);
        }
    }


}
