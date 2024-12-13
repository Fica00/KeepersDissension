using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SearchingForOpponentAnimation : MonoBehaviour
{
    [SerializeField] private int amountOfDots;
    [SerializeField] private float waitTimeBetweenUpdate;
    private TextMeshProUGUI textDisplay;

    private void Awake()
    {
        textDisplay = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        StartCoroutine(AnimationRoutine());
    }

    private IEnumerator AnimationRoutine()
    {
        while (gameObject.activeSelf)
        {
            textDisplay.text = "Searching for opponent";
            yield return new WaitForSeconds(waitTimeBetweenUpdate);
            for (int _i = 0; _i < amountOfDots; _i++)
            {
                textDisplay.text += ".";
                yield return new WaitForSeconds(waitTimeBetweenUpdate);
            }

            for (int _i = 0; _i < amountOfDots; _i++)
            {
                string _text = textDisplay.text;
                textDisplay.text = _text.Substring(0, _text.Length - 1);
                yield return new WaitForSeconds(waitTimeBetweenUpdate);
            }
        }
    }
}
