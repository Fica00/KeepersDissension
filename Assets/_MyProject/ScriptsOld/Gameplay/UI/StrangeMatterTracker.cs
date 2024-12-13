using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StrangeMatterTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI myStrangeMatter;
    [SerializeField] private TextMeshProUGUI opponentsStrangeMatter;
    [SerializeField] private TextMeshProUGUI strangeMatterInEconomy;

    [SerializeField] private Button buyStrangeMatter;
    [SerializeField] private ArrowPanel economyPanel;
    private GameplayPlayer myPlayer;
    private GameplayPlayer opponentPlayer;

    public void Setup(GameplayPlayer _myPlayer, GameplayPlayer _opponentPlayer)
    {
        myPlayer = _myPlayer;
        opponentPlayer = _opponentPlayer;

        myPlayer.UpdatedStrangeMatter += ShowMyStrangeMatter;
        opponentPlayer.UpdatedStrangeMatter += ShowOpponentStrangeMatter;
        ShowAmountInEconomy();
    }

    private void OnEnable()
    {
        GameplayManager.Instance.WhiteStrangeMatter.UpdatedAmountInEconomy += ShowAmountInEconomy;
        buyStrangeMatter.onClick.AddListener(BuyStrangeMatter);
    }

    private void OnDisable()
    {
        if (myPlayer==null)
        {
            return;
        }
        myPlayer.UpdatedStrangeMatter -= ShowMyStrangeMatter;
        opponentPlayer.UpdatedStrangeMatter -= ShowOpponentStrangeMatter;
        GameplayManager.Instance.WhiteStrangeMatter.UpdatedAmountInEconomy -= ShowAmountInEconomy;
        buyStrangeMatter.onClick.RemoveListener(BuyStrangeMatter);
    }

    private void ShowMyStrangeMatter()
    {
        myStrangeMatter.text = myPlayer.StrangeMatter.ToString();
        ShowAmountInEconomy();
    }

    public void ShowOpponentStrangeMatter()
    {
        opponentsStrangeMatter.text = opponentPlayer.StrangeMatter.ToString();
        ShowAmountInEconomy();
    }

    public void ShowAmountInEconomy()
    {
        strangeMatterInEconomy.text = GameplayManager.Instance.WhiteStrangeMatter.AmountInEconomy.ToString();
    }

    private void BuyStrangeMatter()
    {
        if (GameplayManager.Instance.GameState == GameplayState.UsingSpecialAbility)
        {
            return;
        }
        economyPanel.Show();
    }
}
