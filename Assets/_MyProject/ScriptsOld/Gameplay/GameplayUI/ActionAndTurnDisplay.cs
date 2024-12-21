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
        if(FirebaseManager.Instance.RoomHandler.IsOwner)
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

    public void ForceChange(int _actionAmount, bool _my, bool _isResponseAction)
    {
        ShowAction(_actionAmount,_my,_isResponseAction);
    }

    public void ShowAction(int _number, bool _my,bool _isResponseAction=false)
    {
        if (_number==0)
        {
            return;
        }

        var _gameState = GameplayManager.Instance.GameState();
        if (!_isResponseAction && (_gameState == GameplayState.WaitingForAttackResponse || _gameState == GameplayState.AttackResponse))
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

        if (_isResponseAction)
        {
            _color = Color.magenta;
            _text = _my ? "Your response" : "Opponents response";
        }
        
        actionsDisplay.color = _color;
        turnDisplay.text = _text;
        actionAmountDisplay.text = _number.ToString();
        
        if (actionDisplays.Length-1<_number-1)
        {
            return;
        }
        actionsDisplay.sprite = actionDisplays[_number - 1];
    }
}
