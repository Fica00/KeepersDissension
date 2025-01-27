using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LoadReferenceImg : MonoBehaviour
{
    public GameObject _RefImg; // The image that will activate upon this button press
    public GameObject HeaderText;
     
    void Start()
    {
        Button button = GetComponent<Button>();
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }
     
    void OnButtonClick() {
        transform.parent.gameObject.SetActive(false);
        _RefImg.SetActive(true);
        HeaderText.GetComponent<TextMeshProUGUI>().text = "Tap anywhere to return";
    }
}
