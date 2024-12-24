using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardActionsDisplay : MonoBehaviour
{
    public static CardActionsDisplay Instance;
    
    [SerializeField] private GameObject holder;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button useMoveAction;
    [SerializeField] private Button useAttackAction;
    [SerializeField] private Button flip;
    [SerializeField] private Image cardDisplay;
    [SerializeField] private AbilityTrigger abilityTriggerPrefab;
    [SerializeField] private Transform myAbilitiesHolder;
    [SerializeField] private Transform effectAbilitiesHolder;
    [SerializeField] private Transform effectsHolder;
    [SerializeField] private Button unchainButton;
    [SerializeField] private Image xDisplay;
    [SerializeField] private GameObject[] actions;

    private TableActionsHandler actionsHandler;
    private TableHandler tableHandler;
    private Card selectedCard;
    private TablePlaceHandler selectedPlace;
    private bool isFlipped;
    private List<GameObject> shownAbilityTriggers= new();
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        tableHandler = GameplayManager.Instance.TableHandler;
        actionsHandler = tableHandler.ActionsHandler;
    }

    private void OnEnable()
    {
        closeButton.onClick.AddListener(Close);
        useMoveAction.onClick.AddListener(UseMoveAction);
        useAttackAction.onClick.AddListener(UseAttackAction);
        flip.onClick.AddListener(Flip);
        unchainButton.onClick.AddListener(Unchain);
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(Close);
        useMoveAction.onClick.RemoveListener(UseMoveAction);
        useAttackAction.onClick.RemoveListener(UseAttackAction);
        flip.onClick.RemoveListener(Flip);
        unchainButton.onClick.AddListener(Unchain);
    }

    private void Unchain()
    {
        GameplayManager.Instance.TryUnchainGuardian();
    }
    
    public void Close()
    {
        GameplayManager.Instance.TableHandler.ActionsHandler.ClearPossibleActions();
        holder.SetActive(false);
    }
    
    private void UseMoveAction()
    {
        if (selectedCard==null)
        {
            return;
        }
        
        if (!selectedCard.My)
        {
            DialogsManager.Instance.ShowOkDialog("Selected card is not yours");
            return;
        }
        
        if (GameplayManager.Instance.IsResponseAction2())
        {
            if (!GameplayManager.Instance.IsMyResponseAction2())
            {
                return;
            }
        }
        
        actionsHandler.ShowPossibleActions(selectedPlace,selectedCard,CardActionType.Move);
    }

    private void UseAttackAction()
    {
        if (selectedCard==null)
        {
            return;
        }
        
        if (!selectedCard.My)
        {
            DialogsManager.Instance.ShowOkDialog("Selected card is not yours");
            return;
        }
        
        if (GameplayManager.Instance.IsResponseAction2())
        {
            if (!GameplayManager.Instance.IsMyResponseAction2())
            {
                return;
            }
        }
        
        actionsHandler.ShowPossibleActions(selectedPlace,selectedCard,CardActionType
        .Attack);
    }

    private void Flip()
    {
        if (selectedCard==null)
        {
            return;
        }

        ResetDisplays();
        cardDisplay.sprite = isFlipped ? selectedCard.Details.Foreground : selectedCard.Details.Background;
        isFlipped = !isFlipped;

        if (selectedCard is Guardian _guardian && _guardian.IsChained && isFlipped)
        {
            foreach (var _action in actions)
            {
                _action.SetActive(false);
            }
            myAbilitiesHolder.gameObject.SetActive(false);
            effectAbilitiesHolder.gameObject.SetActive(false);
            effectsHolder.gameObject.SetActive(false);
            unchainButton.gameObject.SetActive(true);
            xDisplay.gameObject.SetActive(true);
        }
    }
    
    public void Show(int _placeId)
    {
        if (GameplayManager.Instance.IsResponseAction2())
        {
            if (!GameplayManager.Instance.IsMyResponseAction2())
            {
                return;
            }
        }
        else if (!GameplayManager.Instance.IsMyTurn())
        {
            return;
        }
        
        if (!actionsHandler.ContinueWithShowingPossibleActions(_placeId))
        {
            Debug.Log("ccccccc");
            return;
        }

            Debug.Log("dddddd");
        if (GameplayManager.Instance.IsResponseAction2())
        {
            if (!GameplayManager.Instance.IsKeeperResponseAction2)
            {
                int _id = GameplayManager.Instance.GetCard(GameplayManager.Instance.IdOfCardWithResponseAction()).GetTablePlace().Id;
                if (_id != _placeId)
                {
                    _placeId = _id;
                    DialogsManager.Instance.ShowOkDialog("Due to response action you are forced to play with this card");
                }
            }
        }

        ResetDisplays();
        
        selectedPlace = tableHandler.GetPlace(_placeId);
        selectedCard = selectedPlace.GetCardNoWall();
        
        if (selectedCard==null)
        {
            return;
        }

        if (!selectedCard.CanBeUsed)
        {
            DialogsManager.Instance.ShowOkDialog("This card can't be used!");
            selectedCard = null;
            return;
        }
        
        actionsHandler.ClearPossibleActions();

        if (!selectedCard.IsWarrior())
        {
            selectedCard = null;
            return;
        }

        cardDisplay.sprite = selectedCard.Details.Foreground;
        isFlipped = false;
        holder.SetActive(true);
        if (!selectedCard.My)
        {
            ClearAbilities();
            useMoveAction.gameObject.SetActive(false);
            useAttackAction.gameObject.SetActive(false);
            return;
        }
        
        useMoveAction.gameObject.SetActive(true);
        useAttackAction.gameObject.SetActive(true);
        ShowAbilities();
        UseMoveAction();
    }

    private void ResetDisplays()
    {
        foreach (var _action in actions)
        {
            _action.SetActive(true);
        }
        myAbilitiesHolder.gameObject.SetActive(true);
        effectAbilitiesHolder.gameObject.SetActive(true);
        effectsHolder.gameObject.SetActive(true);
        unchainButton.gameObject.SetActive(false);
        xDisplay.gameObject.SetActive(false);
    }

    private void ShowAbilities()
    {
        ClearAbilities();

        List<CardSpecialAbility> _cardAbilities = selectedCard.SpecialAbilities;

        if (_cardAbilities == null || _cardAbilities.Count==0)
        {
            return;
        }

        if (selectedCard==null)
        {
            GameplayManager.Instance.CloseAllPanels();
            return;
        }

        foreach (var _cardAbility in _cardAbilities)
        {
            if (!_cardAbility.IsClickable)
            {
                continue;
            }
            Transform _abilityHolder = _cardAbility.IsBaseCardsEffect ? myAbilitiesHolder : effectAbilitiesHolder;
            AbilityTrigger _abilityTrigger = Instantiate(abilityTriggerPrefab, _abilityHolder);
            _abilityTrigger.Setup(_cardAbility.Sprite, _cardAbility.CanUseAbility,() =>
            {
                if (!selectedCard.My)
                {
                    DialogsManager.Instance.ShowOkDialog("Selected card is not yours");
                    return;
                }
                
                if (!GameplayManager.Instance.IsMyTurn())
                {
                    return;
                }
                
                _cardAbility.UseAbility();
                ShowAbilities();
            });

            shownAbilityTriggers.Add(_abilityTrigger.gameObject);
        }
    }

    private void ClearAbilities()
    {
        foreach (var _shownAbilityTrigger in shownAbilityTriggers)
        {
            Destroy(_shownAbilityTrigger);
        }
        
        shownAbilityTriggers.Clear();
    }
}