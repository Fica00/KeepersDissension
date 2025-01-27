using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PageHandler : MonoBehaviour {
    public GameObject BackgroundObj;
    public GameObject NextPageObj;
    public GameObject PrevPageObj;
    private int PageNum = 0;
    private int MaxPageNum;
    const int Cover = 0;
    const int Page = 1;
    public Sprite[] Backgrounds;  

    public void LoadPage(int pageNum) {
        // Deactivate whatever page we are on if we are not on the cover before we change pages
        if(PageNum > 0) {
            transform.GetChild(PageNum).gameObject.SetActive(false); //first child is the Page# TextMesh, so use PageNum as is for the index (e.g. PageNum=1 means first page and 2nd child so index PageNum=1 is needed)  
        }
        // Load background as needed
        if(pageNum == 0) {
            BackgroundObj.GetComponent<Image>().sprite = Backgrounds[Cover];
        } else if(PageNum == 0 && pageNum > 0) {
            BackgroundObj.GetComponent<Image>().sprite = Backgrounds[Page];
        } 
        SetPageNum(pageNum);
        if(PageNum > 0) {
            transform.GetChild(PageNum).gameObject.SetActive(true);
        }

    }

    public int GetPageNum() {
        return PageNum;
    }

    public void SetPageNum(int num) {
        PageNum = num;
        if(PageNum == 0) {
            GetComponentInChildren<TextMeshProUGUI>().text = "";
        } else {
            GetComponentInChildren<TextMeshProUGUI>().text = PageNum.ToString();
        }
    }

    public int GetMaxPageNum() {
        return MaxPageNum;
    } 

    private void OnEnable() {
        MaxPageNum = transform.childCount - 1;
        NextPageObj.gameObject.SetActive(true);
        PrevPageObj.gameObject.SetActive(false);
        LoadPage(0);
    }
} 
