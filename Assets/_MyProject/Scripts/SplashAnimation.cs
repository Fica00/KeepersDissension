using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SplashAnimation : MonoBehaviour
{
    public static Action OnFinished;
    [SerializeField] private Image splashImage;
    [SerializeField] private int fadeDuration;
    [SerializeField] private GameObject[] objectsToDeactivate;
    [SerializeField] private GameObject[] objectsToActivate;
    
    private void Start()
    {
        splashImage.DOFade(1f, fadeDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            splashImage.DOFade(0f, fadeDuration).SetEase(Ease.Linear). OnComplete(() =>
            {
                //finishedAnimation
                foreach (var _objectToDeactivate in objectsToDeactivate)
                {
                    _objectToDeactivate.gameObject.SetActive(false);
                }
                
                foreach (var _objectToActivate in objectsToActivate)
                {
                    _objectToActivate.gameObject.SetActive(true);
                }
                OnFinished?.Invoke();
            });
        });
    }
}
