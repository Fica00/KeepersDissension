using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Guardian: Card
{
    public Action OnUnchained;
    public CardStats GainStatsOnUnchaining; //add those stats to current stats
    [SerializeField] private TextMeshProUGUI healthDisplay;
    [SerializeField] private LineRenderer chain;
    private GameplayPlayer player;


    public bool IsChained { get; set; } = true;

    public void Unchain()
    {
        player = My ? GameplayManager.Instance.MyPlayer : GameplayManager.Instance.OpponentPlayer;
        IsChained = false;
        ChangeDamage(GainStatsOnUnchaining.Damage);
        ChangeHealth(GainStatsOnUnchaining.Health);
        ChangeRange(GainStatsOnUnchaining.Range);

        RectTransform _cardBaseTransform = Display.GetComponent<RectTransform>();
        Vector3 _startingScale = _cardBaseTransform.localScale;
        _cardBaseTransform.DOScale(new Vector3(_startingScale.x,0,_startingScale.z), 0.5f).OnComplete(() =>
        {
            _cardBaseTransform.DOScale(_startingScale,0.5f);
            Display.ChangeSprite(Details.Background);
        });
        chain.gameObject.SetActive(false);
        OnUnchained?.Invoke();
        player.OnEndedTurn += AddSpeed;
        AddSpeed();
    }
    
    
    protected override void Setup()
    {
        UpdatedHealth += ShowHealth;
        ShowHealth();
        chain.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameplayManager.OnCardAttacked += AddSpeed;
        GameplayManager.OnCardMoved += AddSpeed;
        GameplayManager.OnPlacedCard += AddSpeed;
        GameplayManager.OnSwitchedPlace += AddSpeed;
    }

    private void OnDisable()
    {
        UpdatedHealth -= ShowHealth;
        if (player!=null)
        {
            player.OnEndedTurn -= AddSpeed;
        }
        
        GameplayManager.OnCardAttacked -= AddSpeed;
        GameplayManager.OnCardMoved -= AddSpeed;
        GameplayManager.OnPlacedCard -= AddSpeed;
        GameplayManager.OnSwitchedPlace -= AddSpeed;
    }

    private void AddSpeed(CardBase _cardOne, CardBase _cardTwo)
    {
        AddSpeed();
    }

    private void AddSpeed(CardBase _arg1, CardBase _arg2, int _arg3)
    {
        AddSpeed();
    }

    private void AddSpeed(CardBase _arg1, int _arg2, int _arg3, bool _)
    {
        AddSpeed();
    }

    private void AddSpeed(CardBase _obj)
    {
        AddSpeed();
    }
    
    private void AddSpeed()
    {
        if (IsChained)
        {
            return;
        }
        SetSpeed(2);
    }

    private void ShowHealth()
    {
        if (Health<=0)
        {
            healthDisplay.text = 0.ToString();
            if (GameplayManager.Instance.IsAbilityActive<Risk>())
            {
                GameplayManager.Instance.StopGame(!My);
            }
            return;
        }
        healthDisplay.text = Health.ToString();
    }

    public void ShowChain()
    {
        StartCoroutine(ShowCainRoutine());
    }
    
    private IEnumerator ShowCainRoutine()
    {
        chain.gameObject.SetActive(true);
        LifeForce _lifeForce = FindObjectsOfType<LifeForce>().ToList().Find(_lifeForce => _lifeForce.My == My);
        chain.gameObject.SetActive(true);
        RectTransform _rectTransform = GetComponent<RectTransform>();
        RectTransform _lifeForceRectTransform = _lifeForce.GetComponent<RectTransform>();
        while (IsChained&& Health>0)
        {
            if (GetTablePlace()==null)
            {
                chain.gameObject.SetActive(false);
                continue;
            }
            Vector3 _lifeForcePosition = _lifeForceRectTransform.position;
            Vector3 _relativeLifeForcePosition = _rectTransform.InverseTransformPoint(_lifeForcePosition);
            chain.SetPosition(0, Vector3.zero);
            chain.SetPosition(1, _relativeLifeForcePosition);
            yield return null;
        }
    }

    private void Update()
    {
        if (!IsChained)
        {
            chain.gameObject.SetActive(false);
        }
    }
}