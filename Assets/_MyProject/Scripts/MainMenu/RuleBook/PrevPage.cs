using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PrevPage : MonoBehaviour
{
    public GameObject NextPageObj; // NextPage object reference so we can activate it when we leave max page
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
        if (pageNum - 1 == 0) gameObject.SetActive(false); // If where we're headed is the cover, turn off this button
        if(pageNum == PageHandler.GetMaxPageNum()) NextPageObj.SetActive(true); // If we are at the last page, turn on the NextPage button
        if(pageNum <= PageHandler.GetMaxPageNum() && pageNum > 0) { 
            PageHandler.LoadPage(pageNum-1);
        } 
    } 
}
