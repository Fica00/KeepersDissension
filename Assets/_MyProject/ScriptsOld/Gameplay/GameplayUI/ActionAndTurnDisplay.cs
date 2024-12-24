using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionAndTurnDisplay : MonoBehaviour
{
    [SerializeField] private Image actionsDisplay;
    [SerializeField] private TextMeshProUGUI turnDisplay;
    [SerializeField] private TextMeshProUGUI actionAmountDisplay;
    [SerializeField] private Color myColor;
    [SerializeField] private Color opponentColor;
    [SerializeField] private Sprite[] actionDisplays;
    
    public void Setup()
    {
        actionsDisplay.enabled = true;
        StartCoroutine(ShowActionRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator ShowActionRoutine()
    {
        string _text;
        Color _color;
        int _number;
        while (gameObject.activeSelf)
        {
            if (GameplayManager.Instance.IsMyTurn())
            {
                _number = GameplayManager.Instance.AmountOfActions(true);
                _text = "Your turn";
                _color = myColor;
            }
            else
            {
                _number = GameplayManager.Instance.AmountOfActions(false);
                _text = "Opponent's turn";
                _color = opponentColor;
            }

            if (GameplayManager.Instance.IsResponseAction2())
            {
                _text = GameplayManager.Instance.IsMyResponseAction2() ? "Your response" : "Opponents response";
                _color = Color.magenta;
            }

            if (_number==0)
            {
                yield return new WaitForSeconds(1);
                continue;
            }
        
            actionsDisplay.color = _color;
            turnDisplay.text = _text;
            actionAmountDisplay.text = _number.ToString();
            actionsDisplay.sprite = actionDisplays[_number - 1];

            yield return new WaitForSeconds(1);
        }
    }
}
