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
    private GameplayPlayer myPlayer;
    private GameplayPlayer opponentPlayer;
    
    public void Setup(GameplayPlayer _myPlayer, GameplayPlayer _opponentPlayer)
    {
        myPlayer = _myPlayer;
        opponentPlayer = _opponentPlayer;
        myPlayer.UpdatedActions += ShowMyAction;
        opponentPlayer.UpdatedActions += ShowOpponentsAction;
        actionsDisplay.enabled = true;
        // if (PhotonManager.IsMasterClient)
        if(true)
        {
            ShowMyAction();
        }
        else
        {
            ShowOpponentsAction();
        }
    }

    private void OnDisable()
    {
        if (myPlayer!=null)
        {
            myPlayer.UpdatedActions -= ShowMyAction;
        }

        if (opponentPlayer!=null)
        {
            opponentPlayer.UpdatedActions -= ShowOpponentsAction;
        }
    }

    private void ShowMyAction()
    {
        ShowAction(myPlayer.Actions,true);
    }

    private void ShowOpponentsAction()
    {
        ShowAction(opponentPlayer.Actions,false);
    }

    public void ForceChange(int _actionAmount, bool _my)
    {
        ShowAction(_actionAmount,_my);
    }

    public void ShowAction(int _number, bool _my)
    {
        if (_number==0)
        {
            return;
        }
        string _text;
        Color _color;
        if (_my)
        {
            _text = "Your turn";
            _color = myColor;
        }
        else
        {
            _text = "Opponent's turn";
            _color = opponentColor;
        }

        actionsDisplay.color = _color;
        actionsDisplay.sprite = actionDisplays[_number - 1];
        actionAmountDisplay.text = _number.ToString();
        turnDisplay.text = _text;
    }
}
