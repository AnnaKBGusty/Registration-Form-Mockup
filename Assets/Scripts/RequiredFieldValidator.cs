using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class RequiredFieldValidator {
    public TMP_InputField inputField;
    public TMP_Text errorText;
    public string entryId;
    
    // Call this method to validate the field. 
    // For simplicity, we just check that the field is not empty.
    public bool Validate() {

        // If no error text is assigned, assume the field is optional and therefore valid.
        if (errorText == null) {
            return true;
        }

        bool isValid = !string.IsNullOrWhiteSpace(inputField.text);
        errorText.gameObject.SetActive(!isValid);
        
        if (!isValid){
            Debug.Log(inputField.gameObject.name + " is required");

        }
        return isValid;
    }
}
