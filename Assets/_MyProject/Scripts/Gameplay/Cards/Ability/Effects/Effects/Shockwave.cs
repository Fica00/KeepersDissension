using System.Collections;
using System.Linq;
using UnityEngine;

public class Shockwave : AbilityEffect
{
    public override void ActivateForOwner()
    {
        MoveToActivationField();
        int _damage = FindObjectsOfType<AbilityCard>().ToList().FindAll(_ability => _ability.My &&_ability.Details
        .Type == AbilityCardType.CrowdControl && _ability.Details.Color == AbilityColor.Red && 
        GameplayManager.Instance.MyPlayer.OwnedAbilities.Contains(_ability)).Count;
        
        if (_damage==0)
        {
            RemoveAction();
            OnActivated?.Invoke();
            DialogsManager.Instance.ShowOkDialog("You don't have any red ability card");
            return;
        }

        int _myStrangeMatter = GameplayManager.Instance.MyPlayer.StrangeMatter;
        int _opponentStrangeMatter = GameplayManager.Instance.OpponentPlayer.StrangeMatter;
        
        GameplayState _state = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
        
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
                FirstCardId = _card.Details.Id,
                FinishingPlaceId = _card.GetTablePlace().Id,
                SecondCardId = _card.Details.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                IsMy = true,
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
            yield return new WaitForSeconds(2);
            GameplayManager.Instance.MyPlayer.RemoveStrangeMatter(GameplayManager.Instance.MyPlayer.StrangeMatter - 
            _myStrangeMatter);
            GameplayManager.Instance.TellOpponentToRemoveStrangeMatter(GameplayManager.Instance.OpponentPlayer
            .StrangeMatter-_opponentStrangeMatter);
            GameplayManager.Instance.GameState = _state;
            RemoveAction();
            OnActivated?.Invoke();
        }
    }
}
