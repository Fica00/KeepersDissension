public class SnowKeeper : CardSpecialAbility
{
    public override void UseAbility()
    {
        if (!CanUseAbility)
        {
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog("Use keepers ultimate?", Use);
    }
    
    void Use()
    {
        GameplayPlayer _player = GetPlayer();
        GameplayManager.Instance.TellOpponentSomething("Opponent used his Ultimate!");
        _player.OnStartedTurn += EndEffect;
        foreach (var _card in GameplayManager.Instance.GetAllCards())
        {
            if (_card.GetIsMy() == Card.GetIsMy())
            {
                continue;
            }
            
            Card.CardData.WarriorAbilityData.EffectedCards.Add(_card.UniqueId);
            _card.ApplyHasSnowUltimateEffect(true);
        }
        SetCanUseAbility(false);
        _player.Actions--;
        if (_player.Actions>0)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }

    private void EndEffect()
    {
        GameplayPlayer _player = GetPlayer();
        _player.OnStartedTurn -= EndEffect;

        foreach (var _cardId in Card.CardData.WarriorAbilityData.EffectedCards)
        {
            Card _card = GameplayManager.Instance.GetCard(_cardId);
            if (_card==null)
            {
                continue;
            }
            
            _card.ApplyHasSnowUltimateEffect(false);

        }
        
        Card.CardData.WarriorAbilityData.EffectedCards.Clear();
    }
}
