using System.Collections;
using TMPro;
using UnityEngine;

public class Keeper: Card
{
    [SerializeField] private TextMeshProUGUI healthDisplay;
    [SerializeField] private GameObject healthHolder;
    
    protected override void Setup()
    {
        StartCoroutine(ShowHealth());
        GameplayPlayer _player = My ? GameplayManager.Instance.MyPlayer: GameplayManager.Instance.OpponentPlayer;
        if (_player.FactionSo.Id==0)
        {
            Destroy(EffectsHolder.GetComponent<CyberKeeper>());
            Destroy(EffectsHolder.GetComponent<DragonKeeper>());
            Destroy(EffectsHolder.GetComponent<ForestKeeper>());
        }
        else if(_player.FactionSo.Id==1)
        {
            Destroy(EffectsHolder.GetComponent<SnowKeeper>());
            Destroy(EffectsHolder.GetComponent<DragonKeeper>());
            Destroy(EffectsHolder.GetComponent<ForestKeeper>());
        }
        else if (_player.FactionSo.Id==2)
        {
            Destroy(EffectsHolder.GetComponent<SnowKeeper>());
            Destroy(EffectsHolder.GetComponent<CyberKeeper>());
            Destroy(EffectsHolder.GetComponent<ForestKeeper>());
        }
        else if (_player.FactionSo.Id==3)
        {
            Destroy(EffectsHolder.GetComponent<SnowKeeper>());
            Destroy(EffectsHolder.GetComponent<CyberKeeper>());
            Destroy(EffectsHolder.GetComponent<DragonKeeper>());
        }
    }

    private void OnEnable()
    {
        OnPositionedOnTable += ShowUI;
        OnPositionedInHand += HideUI;
    }


    private void OnDisable()
    {
        StopAllCoroutines();
        OnPositionedOnTable -= ShowUI;
        OnPositionedInHand -= HideUI;
    }

    private IEnumerator ShowHealth()
    {
        while (gameObject.activeSelf)
        {
            healthDisplay.text = Health.ToString();
            yield return new WaitForSeconds(1);
        }
    }

    private void ShowUI(CardBase _card)
    {
        if (_card!=this)
        {
            return;
        }
        
        healthHolder.SetActive(true);
    }

    private void HideUI(CardBase _card)
    {
        if (_card!=this)
        {
            return;
        }
        
        healthHolder.SetActive(false);
    }
}