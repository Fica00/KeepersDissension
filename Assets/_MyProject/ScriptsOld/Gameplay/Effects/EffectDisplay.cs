using System;
using UnityEngine;
using UnityEngine.UI;

public class EffectDisplay : MonoBehaviour
{
    [SerializeField] private Sprite stunSprite;
    [SerializeField] private Sprite deliverySprite;
    
    private Button button;
    private Image image;
    private string text;

    public void Setup(EffectBase _effectBase)
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        button.onClick.AddListener(Show);
        image.sprite = GetSprite(_effectBase.Type);
        text = _effectBase.Desc;
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Show);
    }

    private void Show()
    {
        DialogsManager.Instance.ShowOkDialog(text);
    }

    private Sprite GetSprite(EffectType _type)
    {
        switch (_type)
        {
            case EffectType.Stun:
                return stunSprite;
            case EffectType.Delivery:
                return deliverySprite;
            default:
                throw new ArgumentOutOfRangeException(nameof(_type), _type, null);
        }
    }
}
