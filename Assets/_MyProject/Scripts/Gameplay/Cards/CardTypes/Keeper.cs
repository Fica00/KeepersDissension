using System.Globalization;
using TMPro;
using UnityEngine;

public class Keeper: Card
{
    [SerializeField] private TextMeshProUGUI healthDisplay;
    [SerializeField] private GameObject healthHolder;
    private int percentageOfHealthToRecover=100;

    public int PercentageOfHealthToRecover
    {
        get => percentageOfHealthToRecover;
        set => percentageOfHealthToRecover = value;
    }
    
    protected override void Setup()
    {
        Stats.UpdatedHealth += ShowHealth;

        ShowHealth();
        GameplayPlayer _player = IsMy ? GameplayManager.Instance.MyPlayer: GameplayManager.Instance.OpponentPlayer;
        if (_player.FactionSO.Id==0)
        {
            Destroy(EffectsHolder.GetComponent<CyberKeeper>());
            Destroy(EffectsHolder.GetComponent<DragonKeeper>());
            Destroy(EffectsHolder.GetComponent<ForestKeeper>());
        }
        else if(_player.FactionSO.Id==1)
        {
            Destroy(EffectsHolder.GetComponent<SnowKeeper>());
            Destroy(EffectsHolder.GetComponent<DragonKeeper>());
            Destroy(EffectsHolder.GetComponent<ForestKeeper>());
        }
        else if (_player.FactionSO.Id==2)
        {
            Destroy(EffectsHolder.GetComponent<SnowKeeper>());
            Destroy(EffectsHolder.GetComponent<CyberKeeper>());
            Destroy(EffectsHolder.GetComponent<ForestKeeper>());
        }
        else if (_player.FactionSO.Id==3)
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
        Stats.UpdatedHealth -= ShowHealth;
        OnPositionedOnTable -= ShowUI;
        OnPositionedInHand -= HideUI;
    }

    private void ShowHealth()
    {
        healthDisplay.text = Stats.Health.ToString(CultureInfo.InvariantCulture);
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