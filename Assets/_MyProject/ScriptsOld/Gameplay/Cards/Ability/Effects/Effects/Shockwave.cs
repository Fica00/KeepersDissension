using System.Collections;
using System.Linq;
using UnityEngine;

public class Shockwave : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        int _damage = FindObjectsOfType<AbilityCard>().ToList().FindAll(_ability => _ability.My &&_ability.Details
        .Type == AbilityCardType.CrowdControl && _ability.Details.Color == AbilityColor.Red && 
        GameplayManager.Instance.GetOwnedAbilities(true).ToList().Contains(_ability.Data)).Count;
        
        if (_damage==0)
        {
            RemoveAction();
            OnActivated?.Invoke();
            DialogsManager.Instance.ShowOkDialog("You don't have any red ability card");
            return;
        }

        int _myStrangeMatter = GameplayManager.Instance.MyStrangeMatter();
        int _opponentStrangeMatter = GameplayManager.Instance.OpponentsStrangeMatter();
        
        GameplayState _state = GameplayManager.Instance.GameState();
        GameplayManager.Instance.SetGameState(GameplayState.UsingSpecialAbility);
        
        foreach (var _card in FindObjectsOfType<Card>().ToList())
        {
            if (!_card.IsAttackable())
            {
                continue;
            }

            if (_card.GetTablePlace()==null)
            {
                continue;
            }

            CardAction _attackAction = new CardAction
            {
                StartingPlaceId = _card.GetTablePlace().Id,
                FirstCardId = _card.UniqueId,
                FinishingPlaceId = _card.GetTablePlace().Id,
                SecondCardId = _card.UniqueId,
                Type = CardActionType.Attack,
                Cost = 0,
                CanTransferLoot = false,
                Damage = _damage,
                CanCounter = false,
                GiveLoot = false
            };
            
            GameplayManager.Instance.ExecuteCardAction(_attackAction);
        }

        StartCoroutine(RemoveStrangeMatter());

        IEnumerator RemoveStrangeMatter()
        {
            var _gameplayInstance = GameplayManager.Instance;
            yield return new WaitForSeconds(2);
            _gameplayInstance.ChangeMyStrangeMatter(_gameplayInstance.MyStrangeMatter() - 
                                                    _myStrangeMatter);
            _gameplayInstance.ChangeOpponentsStrangeMatter(_gameplayInstance.OpponentsStrangeMatter()-_opponentStrangeMatter);
            GameplayManager.Instance.SetGameState(_state);
            RemoveAction();
            OnActivated?.Invoke();
        }
    }
}
