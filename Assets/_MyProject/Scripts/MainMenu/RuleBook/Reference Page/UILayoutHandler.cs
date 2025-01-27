using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UILayoutHandler : MonoBehaviour
{ 
    public GameObject HeaderText;
    
    void OnEnable() // Init routine: Loop through all children, activate the first and deactivate all others
    {
        int index = 0;
        HeaderText.GetComponent<TextMeshProUGUI>().text = "Tap one of the red boxes for info";
         foreach (Transform child in transform) {
            if (index == 0) {
                child.gameObject.SetActive(true);
                index++;
                continue;
            }
            child.gameObject.SetActive(false);
        }
    } 
}
