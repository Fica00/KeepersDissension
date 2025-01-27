using System;
using UnityEngine;
using UnityEngine.UI;

public class AbilityTrigger : MonoBehaviour
{
    private Button button;
    private Action callBack;
    private Image image;
    
    public void Setup(Sprite _sprite,bool _canUse, Action _callBack)
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        
        image.sprite = _sprite;
        callBack = _callBack;
        button.gameObject.SetActive(_canUse);
        button.onClick.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        callBack?.Invoke();
    }
}
