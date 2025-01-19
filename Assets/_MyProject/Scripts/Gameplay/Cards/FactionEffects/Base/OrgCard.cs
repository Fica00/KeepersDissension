using System.Collections.Generic;
using System.Linq;

public class OrgCard : CardSpecialAbility
{
    private GameplayPlayer player;

    private void Start()
    {
        if (!GetPlayer().IsMy)
        {
            return;
        }
        
        player = GetPlayer();
        player.OnStartedTurn += ResetAbilities;
    }

    private void OnDisable()
    {
        if (player==null)
        {
            return; 
        }
        
        player.OnStartedTurn -= ResetAbilities;
    }

    private void ResetAbilities()
    {
        SetCanUseAbility(true);
    }
    
    public override void UseAbility()
    {
        DialogsManager.Instance.ShowYesNoDialog("Use orges Ground Pound  attack?",YesUseGroundPoundAttack);
    }

    private void YesUseGroundPoundAttack()
    {
        PlayAudio();
        bool _didGetResponseAction = OrgAttack();
        var _player = GetPlayer();
        _player.Actions--;
        if (_didGetResponseAction || player.Actions>0)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }

    private void PlayAudio()
    {
        if (Card.Details.Faction.IsCyber)
        {
            GameplayManager.Instance.PlayAudioOnBoth("AllAroundMePain", Card);
        }
        else if (Card.Details.Faction.IsDragon)
        {
            GameplayManager.Instance.PlayAudioOnBoth("FeelTheGroundShake", Card);
        }
        else if (Card.Details.Faction.IsForest)
        {
            GameplayManager.Instance.PlayAudioOnBoth("FeelTheQuake", Card);
        }
        else if (Card.Details.Faction.IsSnow)
        {
            GameplayManager.Instance.PlayAudioOnBoth("PainIsComing", Card);
        }
    }
    

    private bool OrgAttack()
    {
        int _attackingPlaceId = Card.GetTablePlace().Id;
        List<Card> _availablePlaces = GameplayManager.Instance.TableHandler.GetAttackableCards(_attackingPlaceId,
                CardMovementType.EightDirections);

        bool _didGetResponseAction = false;
        foreach (var _cardOnPlace in _availablePlaces.ToList())
        {
            bool _gaveResponse = GameplayManager.Instance.DamageCardByAbility(_cardOnPlace.UniqueId, Card.Damage, _ => { GameplayManager.Instance.HideCardActions
                    ();}, true, Card.UniqueId, true,true);
            if (_gaveResponse)
            {
                _didGetResponseAction = true;
            }
        }

        return _didGetResponseAction;
    }
}