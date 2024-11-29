using UnityEngine;
using UnityEngine.EventSystems;

public class EconomyStrangeMatterDisplay : MonoBehaviour,IPointerClickHandler
{
    private bool enablePurchase;
    
    public void Setup(bool _enablePurchase=true)
    {
        enablePurchase = _enablePurchase;
        gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData _eventData)
    {
        if (!enablePurchase)
        {
            return;
        }
        
        BuyWhiteStrangeMatter();
    }
    
    private void BuyWhiteStrangeMatter()
    {
        if (GameplayManager.Instance.GameState!= GameplayState.Playing && !GameplayManager.Instance.IsKeeperResponseAction)
        {
            return;
        }

        if (Famine.IsActive)
        {
            DialogsManager.Instance.ShowOkDialog("Using strange matter is forbidden by Famine ability");
            return;
        }
        if (GameplayManager.Instance.MyPlayer.Actions==0)
        {
            DialogsManager.Instance.ShowOkDialog("You don't have enough actions");
            return;
        }

        if (GameplayManager.Instance.WhiteStrangeMatter.AmountInEconomy==0)
        {
            DialogsManager.Instance.ShowOkDialog("There is no more white strange matter in the economy reserves");
            return;
        }

        GameplayManager.Instance.MyPlayer.Actions--;
        GameplayManager.Instance.WhiteStrangeMatter.AmountInEconomy--;
        GameplayManager.Instance.BuyMatter();
        GameplayManager.Instance.ForceUpdatePlayerActions();
    }

}
