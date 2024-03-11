using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BomberCard : CardSpecialAbility
{
    private int bombDamage = 3;
    [HideInInspector]public bool ExplodeOnDeath;


    private void OnEnable()
    {
        CardBase.OnGotDestroyed += CheckForCard;
        ExplodeOnDeath = true;
    }
    
    private void OnDisable()
    {
        CardBase.OnGotDestroyed -= CheckForCard;
    }

    private void CheckForCard(CardBase _cardBase)
    {
        if (!ExplodeOnDeath)
        {
            return;
        }
        
        if (_cardBase != CardBase)
        {
            return;
        }

        if (!_cardBase.My)
        {
            return;
        }
        
        GameplayManager.Instance.BombExploded(_cardBase.GetTablePlace().Id);
    }
}
