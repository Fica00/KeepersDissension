using DG.Tweening;
using UnityEngine;

namespace DisplayHelper
{
    public class UIShowHideObjects: MonoBehaviour
    {
        [SerializeField] public float duration;
            
        public virtual void Show()
        {
            gameObject.transform.DOScale(Vector3.one, duration);
        }

        public virtual void Hide()
        {
            gameObject.transform.DOScale(Vector3.zero, duration);
        }
        
    }
}