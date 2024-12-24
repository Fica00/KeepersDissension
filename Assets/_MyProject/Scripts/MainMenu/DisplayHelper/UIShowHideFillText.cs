using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DisplayHelper
{
    public class UIShowHideFillText: UIShowHideObjects
    {
        public override void Show()
        {
            Image _image = GetComponent<Image>();
            if (_image ==null)
            {
                transform.localScale = Vector3.one;
            }
            else
            {
                _image.DOFillAmount(1f, duration);
            }
        }
        
        public override void Hide()
        {
            Image _image = GetComponent<Image>();
            if (_image ==null)
            {
                transform.localScale = Vector3.zero;
            }
            else
            {
                _image.DOFillAmount(0f, duration);
            }
        }
    }
}