using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DisplayHelpers
{
    public class LoadingDisplay: MonoBehaviour
    {
        [SerializeField] private Text loadingText;
        [SerializeField] private Image loadingBar;
        
        
        [SerializeField] private string startingText;
        [SerializeField] private string addition;
        [SerializeField] private bool loop;
        [SerializeField] private float timeToAddCharacter;
        [SerializeField] private float barFillingDuration;

        private void Awake()
        {
            SetUp();//Temp for testing
        }


        public void SetUp()
        {
            loadingText.text = startingText;
            loadingBar.fillAmount = 0;
            StartCoroutine(Animate());
            loadingBar.DOFillAmount(1f, barFillingDuration);
        }

        private IEnumerator Animate()
        {
            var holder = startingText;
            loadingText.text = holder;
            var index = 0;
            yield return new WaitForSecondsRealtime(timeToAddCharacter);
            while (holder!=startingText+addition)
            {
           
                holder += addition[index];
                loadingText.text = holder;
                index++;
                yield return new WaitForSecondsRealtime(timeToAddCharacter);
            }

            if (loop) StartCoroutine(nameof(Animate));
        }
    }
}