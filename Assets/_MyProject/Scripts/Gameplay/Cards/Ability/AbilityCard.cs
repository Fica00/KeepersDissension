using UnityEngine;

public class AbilityCard : CardBase
{
    public AbilityCardDetails Details;
    [SerializeField] AbilityEffect effect;
    [HideInInspector] public bool IsVetoed;
    [SerializeField] private GameObject activeDisplay;

    public GameObject ActiveDisplay => activeDisplay;
    public AbilityEffect Effect => effect;

    public override void Setup(bool _isMy)
    {
        IsMy = _isMy;
        Display.Setup(this);
    }

    public void SetIsMy(bool _isMy)
    {
        IsMy = _isMy;
    }

    private void OnEnable()
    {
        TablePlaceHandler.OnPlaceClicked += TryActivateCard;
    }

    private void OnDisable()
    {
        TablePlaceHandler.OnPlaceClicked -= TryActivateCard;
    }

    private void TryActivateCard(TablePlaceHandler _clickedPlace)
    {
        if (GetTablePlace() == null)
        {
            return;
        }

        if (GetTablePlace()!=_clickedPlace)
        {
            return;
        }

        if (_clickedPlace.Id == -1 || _clickedPlace.Id==65)
        {
            return;
        }

        if (IsVetoed)
        {
            return;
        }
        
        TryActivateCard();
    }

    private void TryActivateCard()
    {
        if (Details.Type!=AbilityCardType.CrowdControl)
        {
            return;
        }
        if (GameplayManager.Instance.GameState != GameplayState.Playing)
        {
            return;
        }

        if (!IsMy)
        {
            return;
        }

        if (GameplayManager.Instance.MyPlayer.Actions<=0)
        {
            UIManager.Instance.ShowOkDialog("You need 1 action to activate this ability");
            return;
        }

        if (Subdued.IsActive)
        {
            UIManager.Instance.ShowOkDialog("Activation of the ability is blocked by Subdued ability");
            return;
        }

        UIManager.Instance.ShowYesNoDialog("Are you sure that you want to activate this ability?",YesActivate);
        void YesActivate()
        {
            if (Tax.SelectedCard!=null)
            {
                if (Tax.SelectedCard==this && !Tax.IsActiveForMe)
                {
                    if (GameplayManager.Instance.MyPlayer.StrangeMatter<=0)
                    {
                        UIManager.Instance.ShowOkDialog("You don't have enough strange matter to pay Tax");
                        return;
                    }

                    GameplayManager.Instance.MyPlayer.StrangeMatter--;
                    GameplayManager.Instance.ActivatedTaxedCard();
                }
            }
            GameplayManager.Instance.ActivateAbility(Details.Id);
        }
    }

    public override void SetParent(Transform _parent)
    {
        Parent = _parent;
        transform.SetParent(_parent);
        ResetPosition();
    }

    public override bool IsWarrior()
    {
        return false;
    }

    public void Activate()
    {
        if (IsVetoed)
        {
            return;
        }
        if (IsMy)
        {
            effect.ActivateForOwner();
        }
        else
        {
            effect.ActivateForOther();
        }
    }

    public AbilityEffect GetEffect()
    {
        return effect;
    }
}
