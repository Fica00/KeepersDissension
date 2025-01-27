using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NextPage : MonoBehaviour
{
    public GameObject PrevPageObj; // PrevPage object reference so we can activate it when we leave page 0
    public PageHandler PageHandler; 
    void Start()
    {
        // Assign button to the Button component so we can add a Listener to it to know when it is clicked
        Button button = GetComponent<Button>();
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick() { 
        int pageNum = PageHandler.GetPageNum();
        if(pageNum + 1 == PageHandler.GetMaxPageNum()) gameObject.SetActive(false); // If where we're headed is the last page, turn off this button 
        if(PageHandler.GetPageNum() == 0) PrevPageObj.SetActive(true); // If we are at the cover, turn on the PrevPage button
        if(pageNum < PageHandler.GetMaxPageNum()) {   
            PageHandler.LoadPage(pageNum+1); 
        } 
    }
}
