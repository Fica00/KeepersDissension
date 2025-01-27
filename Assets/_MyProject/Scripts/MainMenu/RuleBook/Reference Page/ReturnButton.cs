using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ReturnButton : MonoBehaviour
{
    public GameObject _RefImg;
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
        _RefImg.gameObject.SetActive(true);
        HeaderText.GetComponent<TextMeshProUGUI>().text = "Tap one of the red boxes for info";
    }
}
