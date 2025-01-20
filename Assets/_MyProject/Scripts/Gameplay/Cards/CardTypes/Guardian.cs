using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Guardian: Card
{
    public Action OnUnchained;
    [SerializeField] private TextMeshProUGUI healthDisplay;
    [SerializeField] private LineRenderer chain;

    public bool IsChained => !GameplayManager.Instance.DidUnchainGuardian(GetIsMy());

    public void ShowUnchain()
    {
        RectTransform _cardBaseTransform = Display.GetComponent<RectTransform>();
        Vector3 _startingScale = _cardBaseTransform.localScale;
        _cardBaseTransform.DOScale(new Vector3(_startingScale.x,0,_startingScale.z), 0.5f).OnComplete(() =>
        {
            _cardBaseTransform.DOScale(_startingScale,0.5f);
            Display.ChangeSprite(Details.Background);
        });
        chain.gameObject.SetActive(false);

        if (GameplayManager.Instance.MyPlayer.Actions != 1 || GameplayCheats.UnlimitedActions)
        {
            ChangeSpeed(1);
        }
        OnUnchained?.Invoke();
    }
    
    protected override void Setup()
    {
        StartCoroutine(ShowHealth());
        chain.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator ShowHealth()
    {
        while (gameObject.activeSelf)
        {
            healthDisplay.text = Health.ToString();
            
            if (Health<=0)
            {
                healthDisplay.text = 0.ToString();
            }

            yield return new WaitForSeconds(1);
        }
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