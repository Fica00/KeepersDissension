using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Back : MonoBehaviour
{
    public GameObject RuleBookObj; 
    void Start() {
        // Assign button to the Button component so we can add a Listener to it to know when it is clicked
        Button button = GetComponent<Button>();
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick() {
        RuleBookObj.gameObject.SetActive(false);
    }
}
