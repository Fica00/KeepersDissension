using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResolveMultipleActions : MonoBehaviour
{
    public static Action OnShowed;
    public static Action OnHided;
    public static ResolveMultipleActions Instance;
    [SerializeField] private Button actionButtonPrefab;
    [SerializeField] private GameObject holder;
    [SerializeField] private Transform buttonHolders;
    [SerializeField] private TextMeshProUGUI questionDisplay;

    private List<GameObject> shownButtons = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    public void Show(List<CardAction> _actions, Action<CardAction> _callBack)
    {
        ClearShownButtons();
        foreach (var _action in _actions)
        {
            Button _actionButton = Instantiate(actionButtonPrefab, buttonHolders);
            _actionButton.onClick.AddListener(() =>
            {
                _callBack?.Invoke(_action);
                Close();
            });

            string _actionDesc = string.Empty;

            switch (_action.Type)
            {
                case CardActionType.Attack:
                    TablePlaceHandler _defenderPlace = GameplayManager.Instance.TableHandler.GetPlace(_action
                    .FinishingPlaceId);
                    Card _defender = default;
                    foreach (var _cardOnPlace in _defenderPlace.GetCards())
                    {
                        if (_cardOnPlace is Card _card && _card.Details.Id == _action.SecondCardId)
                        {
                            _defender = _card;
                        }
                    }
                    _actionDesc = "Attack "+_defender.Details.Type;
                    break;
                case CardActionType.Move:
                    _actionDesc = "Move";
                    break;
                case CardActionType.SwitchPlace:
                    _actionDesc = "Switch places";
                    break;
                case CardActionType.RamAbility:
                    _actionDesc = "Use Ram";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _actionButton.GetComponentInChildren<TextMeshProUGUI>().text = _actionDesc;
            shownButtons.Add(_actionButton.gameObject);
        }
        
        Button _closeButton = Instantiate(actionButtonPrefab, buttonHolders);
        _closeButton.onClick.AddListener(() =>
        {
            Close();
        });
        _closeButton.GetComponentInChildren<TextMeshProUGUI>().text = "None";
        shownButtons.Add(_closeButton.gameObject);

        foreach (var _buttonObject in shownButtons)
        {
            _buttonObject.SetActive(true);
        }

        questionDisplay.text = "Looks like it is possible to do multiple actions, please select one.";
        
        holder.SetActive(true);
        OnShowed?.Invoke();
    }
    
    public void Show(List<CardBase> _cards, Action<CardBase> _callBack)
    {
        ClearShownButtons();
        foreach (var _card in _cards)
        {
            Button _actionButton = Instantiate(actionButtonPrefab, buttonHolders);
            _actionButton.onClick.AddListener(() =>
            {
                _callBack?.Invoke(_card);
                Close();
            });

            string _actionDesc = string.Empty;
            if (_card is AbilityCard _ability)
            {
                _actionDesc = _ability.gameObject.name.Split('_')[1].Split('(')[0];
            }
            else if (_card is Card _cardCard)
            {
                _actionDesc = _cardCard.Details.Type.ToString();
            }

            _actionButton.GetComponentInChildren<TextMeshProUGUI>().text = _actionDesc;
            shownButtons.Add(_actionButton.gameObject);
        }
        
        Button _closeButton = Instantiate(actionButtonPrefab, buttonHolders);
        _closeButton.onClick.AddListener(Close);
        _closeButton.GetComponentInChildren<TextMeshProUGUI>().text = "None";
        shownButtons.Add(_closeButton.gameObject);

        foreach (var _buttonObject in shownButtons)
        {
            _buttonObject.SetActive(true);
        }
        
        questionDisplay.text = "Looks like it is possible enlarge multiple cards, please select one.";

        holder.SetActive(true);
        OnShowed?.Invoke();
    }

    public void Show(List<string> _options, Action<int> _callBack, string _text)
    {
        ClearShownButtons();
        int _counter=0;
        foreach (var _option in _options)
        {
            Button _actionButton = Instantiate(actionButtonPrefab, buttonHolders);
            var _counter1 = _counter;
            _actionButton.onClick.AddListener(() =>
            {
                _callBack?.Invoke(_counter1);
                Close();
            });

            string _actionDesc = _option;

            _actionButton.GetComponentInChildren<TextMeshProUGUI>().text = _actionDesc;
            shownButtons.Add(_actionButton.gameObject);
            _counter++;
        }
        
        foreach (var _buttonObject in shownButtons)
        {
            _buttonObject.SetActive(true);
        }
        
        questionDisplay.text = _text;

        holder.SetActive(true);
        OnShowed?.Invoke();
    }

    private void ClearShownButtons()
    {
        foreach (var _shownButton in shownButtons)
        {
            Destroy(_shownButton);
        }
        
        shownButtons.Clear();
    }

    private void Close()
    {
        holder.SetActive(false);
        OnHided?.Invoke();
    }
}
